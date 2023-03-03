using System.Collections.Generic;
using iShape.Spline;
using Unity.Collections;
using UnityEngine;

namespace iShape.BezierTool {

    public static class BezierCurveExtension {
        
        public static bool HasAnySelection(this BezierCurve curve) {
            var anchors = curve.dots;
            int n = anchors.Count;
            for(int i = 0; i < n; i++) {
                var anchor = anchors[i];
                if(anchor.IsSelectedPoint || anchor.IsSelectedPrevPinch || anchor.IsSelectedNextPinch) {
                    return true;
                }
            }                

            return false;
        }

        public static void DeselectAll(this BezierCurve curve) {
            var anchors = curve.dots;
            int n = anchors.Count;
            for (int i = 0; i < n; i++) {
                var anchor = anchors[i];
                anchor.Deselect();
            }
        }

        public static void Highlight(this BezierCurve curve, Rect rect) {
            var anchors = curve.dots;
            int n = anchors.Count;
            for(int i = 0; i < n; i++) {
                var anchor = anchors[i];
                anchor.IsMultiSelection = rect.Contains(anchor.Position);
            }
        }

        public static void Select(this BezierCurve curve, Rect rect) {
            var anchors = curve.dots;
            int n = anchors.Count;
            for(int i = 0; i < n; i++) {
                var anchor = anchors[i];
                if (rect.Contains(anchor.Position)) {
                    var prevAnchor = anchors[(i - 1 + n) % n];
                    var nextAnchor = anchors[(i + 1) % n];
                    if (!prevAnchor.IsSelectedPoint) {
                        prevAnchor.isHighlighted = true;
                    }

                    if (!nextAnchor.IsSelectedPoint) {
                        nextAnchor.isHighlighted = true;
                    }

                    anchor.IsSelectedPoint = true;
                    anchor.isHighlighted = false;
                }

                anchor.IsMultiSelection = false;
                anchor.IsSelectedNextPinch = false;
                anchor.IsSelectedPrevPinch = false;
            }
            
        }

        public static int SelectedCount(this BezierCurve curve) {
            var anchors = curve.dots;
            int n = anchors.Count;
            int count = 0;
            for(int i = 0; i < n; i++) {
                if (anchors[i].IsSelectedPoint) {
                    count++;
                }
            }

            return count;
        }

        public static bool RemoveSelected(this BezierCurve curve) {
            bool result = false;
            var anchors = curve.dots;
            int n = anchors.Count;
            for (int i = n - 1; i >= 0; i--) {
                if (anchors[i].IsSelectedPoint) {
                    curve.RemoveAnchor(i);
                    result = true;
                }
            }
            return result;
        }

        public static bool AddPointsToSelected(this BezierCurve curve) {
            var dots = curve.dots;
            int n = dots.Count;
            bool isPrevSelected = dots[0].IsSelectedPoint;
            var suitIndexes = new List<int>();
            for (int i = 1; i <= n; i++) {
                bool isCurrentSelected = dots[i % n].IsSelectedPoint;
                if (isPrevSelected && isCurrentSelected) {
                    suitIndexes.Add(i - 1);
                }

                isPrevSelected = isCurrentSelected;
            }

            n = suitIndexes.Count;
            if (n == 0) {
                return false;
            }

            var anchors = dots.CreateAnchors(Allocator.Temp);
            var sCurve = new Curve(anchors, isClosed: curve.isClosed);
            anchors.Dispose();

            for (int i = n - 1; i >= 0; i--) {
                int index = suitIndexes[i];
                var spline = sCurve.splines[index];

                var containerLineStart = spline.Point(0.48f);
                var localPoint = spline.Point(0.5f);
                var containerLineEnd = spline.Point(0.52f);

                var newAnchor = curve.AddPoint(localPoint, containerLineStart, containerLineEnd, index);
                newAnchor.IsSelectedPoint = true;
            }
            

            return true;
        }

        public static bool SelectedPointRemovePinches(this BezierCurve curve) {
            bool result = false;
            var anchors = curve.dots;
            int n = anchors.Count;

            for (int i = 0; i < n; i++) {
                var anchor = anchors[i];
                if (anchor.IsSelectedPoint) {
                    anchor.type = Anchor.Type.point;
                    result = true;
                }
            }

            return result;
        }

        public static bool SelectedPointAddPinches(this BezierCurve curve) {
            
            bool result = false;
            var dots = curve.dots;
            var anchors = dots.CreateAnchors(Allocator.Temp);
                
            var sCurve = new Curve(anchors, isClosed: curve.isClosed);
            anchors.Dispose();
            
            int n = dots.Count;

            
            for (int i = 0; i < n; ++i) {
                var dot = dots[i];
                if (dot.IsSelectedPoint) {
                    bool isNext = false;
                    if (curve.isClosed || i > 0) {
                        if (!dot.IsPrevPinchAvailable) {
                            dot.PrevPoint = sCurve.splines[(i - 1 + n) % n].Point(0.8f);
                            result = true;
                        }
                        isNext = true;
                    }

                    bool isPrev = false;
                    if (curve.isClosed || i < n - 1) {
                        if (!dot.IsNextPinchAvailable) {
                            dot.NextPoint = sCurve.splines[i].Point(0.2f);
                            result = true;
                        }

                        isPrev = true;
                    }

                    if (isNext && isPrev) {
                        dot.type = Anchor.Type.doublePinch;
                    } else if (isNext) {
                        dot.type = Anchor.Type.prevPinch;
                    } else if (isPrev) {
                        dot.type = Anchor.Type.nextPinch;
                    } else {
                        dot.type = Anchor.Type.point;
                    }
                }
            }

            return result;
        }

        public static bool SelectedPointsRemoveBetweenPinches(this BezierCurve curve) {
            bool result = false;
            var anchors = curve.dots;
            int n = anchors.Count;

            var prevAnchor = anchors[n - 1];
            
            for (int i = 0; i < n; i++) {
                var anchor = anchors[i];
                if (anchor.IsSelectedPoint && prevAnchor.IsSelectedPoint) {
                    result = true;
                    if (anchor.type == Anchor.Type.doublePinch) {
                        anchor.type = Anchor.Type.nextPinch;
                    } else {
                        anchor.type = Anchor.Type.point;
                    }

                    if (prevAnchor.type == Anchor.Type.doublePinch) {
                        prevAnchor.type = Anchor.Type.prevPinch;
                    } else {
                        prevAnchor.type = Anchor.Type.point;
                    }
                }

                prevAnchor = anchor;
            }

            return result;
        }

        public static bool SelectedPointsAddBetweenPinches(this BezierCurve curve) {
            bool result = false;
            var dots = curve.dots;
            
            int n = dots.Count;
            var anchors = dots.CreateAnchors(Allocator.Temp);
            var sCurve = new Curve(anchors, isClosed: curve.isClosed);
            anchors.Dispose();
            
            var prevAnchor = dots[n - 1];
            
            for (int i = 0; i < n; i++) {
                var anchor = dots[i];
                if (anchor.IsSelectedPoint && prevAnchor.IsSelectedPoint) {
                    var spline = sCurve.splines[(i - 1 + n) % n];
                    if (!anchor.IsPrevPinchAvailable) {
                        if (anchor.type == Anchor.Type.nextPinch) {
                            anchor.type = Anchor.Type.doublePinch;
                        } else {
                            anchor.type = Anchor.Type.prevPinch;
                        }

                        anchor.PrevPoint = spline.Point(0.8f);
                        result = true;
                    }

                    if (!prevAnchor.IsNextPinchAvailable) {
                        if (prevAnchor.type == Anchor.Type.prevPinch) {
                            prevAnchor.type = Anchor.Type.doublePinch;
                        } else {
                            prevAnchor.type = Anchor.Type.nextPinch;
                        }

                        prevAnchor.NextPoint = spline.Point(0.2f);
                        result = true;
                    }
                }

                prevAnchor = anchor;
            }
            

            return result;
        }
        
        public static void AddPoint(this BezierCurve curve, Vector2 point) {
            Vector2 position = curve.transform.position;
            var localPoint = point - position;

            var anchors = curve.dots.CreateAnchors(Allocator.Temp);
            var sCurve = new Curve(anchors, isClosed: curve.isClosed);
            anchors.Dispose();
            var result = sCurve.FindSpriteContainPoint(localPoint, out var containerLineStart, out var containerLineEnd, curve.stepLength);

            curve.AddPoint(localPoint, containerLineStart, containerLineEnd, result);
        }

        private static Dot AddPoint(this BezierCurve curve, Vector2 localPoint, Vector2 containerLineStart, Vector2 containerLineEnd, int index) {
            return curve.dots.AddPoint(localPoint, containerLineStart, containerLineEnd, index);
        }

        private static void RemoveAnchor(this BezierCurve curve, int index) {
            if (index >= 0) {
                int n = curve.dots.Count;
                int i = (index + n) % n;
                curve.dots.RemoveAt(i);
            }
        }
    }

}