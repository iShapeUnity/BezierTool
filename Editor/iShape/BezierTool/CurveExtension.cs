#if UNITY_EDITOR

using System.Collections.Generic;
using iShape.Spline;
using Unity.Collections;
using UnityEngine;

namespace iShape.BezierTool {

    internal static class CurveExtension {
        
        internal static int FindSpriteContainPoint(this Curve curve, Vector2 point, out Vector2 lineStart, out Vector2 lineEnd, float curveStepLength) {

            lineEnd = Vector2.zero;
            lineStart = Vector2.zero;

            int result = -1;
            
            var spriteList = curve.splines;
            int n = spriteList.Length;
            
            float minSqrDistance = float.MaxValue;

            for(int i = 0; i < n; i++) {
                var points = spriteList[i].Points(curveStepLength, Allocator.Temp);

                int m = points.Length;
                for(int j = 1; j < m; j++) {
                    float sqrDistance = Line.SqrDistanceToEdge(point, points[j - 1], points[j]);
                    if(sqrDistance < minSqrDistance) {
                        
                        result = i;
                        lineEnd = points[j];
                        lineStart = points[j - 1];
                        minSqrDistance = sqrDistance;
                        if(Mathf.Approximately(minSqrDistance, 0)) {
                            return result;
                        }
                    }
                }

                points.Dispose();
            }                
            
            return result;
        }
        
        internal static Dot AddPoint(this List<Dot> dots, Vector2 localPoint, Vector2 containerLineStart, Vector2 containerLineEnd, int index) {
            int n = dots.Count;

            int i = (index + n) % n;
            int j = (index + 1 + n) % n;


            var anchorPrev = Vector2.zero;
            bool isPrevAnchorPresent = dots[i].type is Anchor.Type.doublePinch or Anchor.Type.nextPinch;
            if (isPrevAnchorPresent) {
                var leftAuxiliaryPoint = Vector2.zero;
                bool hasLeftAuxiliaryPoint = false;
                if(isSuitForIntersection(dots[i].Position, dots[i].NextPoint, containerLineStart, containerLineEnd)) {
                    hasLeftAuxiliaryPoint = Line.FindIntersectionPoint(out leftAuxiliaryPoint, dots[i].Position, dots[i].NextPoint, containerLineStart, containerLineEnd);
                }

                if(hasLeftAuxiliaryPoint) {
                    dots[i].NextPoint = 0.5f * (leftAuxiliaryPoint + dots[i].Position);
                    anchorPrev = 0.5f * (leftAuxiliaryPoint + localPoint);
                } else {
                    // the vectors is almost collinear
                    var nextPointDirection = dots[i].NextPoint - dots[i].Position;
                    var direction = (containerLineStart - containerLineEnd).normalized;
                    
                    float distance = (dots[i].Position - localPoint).magnitude;
                    const float k = 0.25f;

                    if(Vector2.Dot(nextPointDirection, direction) > 0.0f) {
                        anchorPrev = localPoint - k * distance * direction;
                    } else {
                        anchorPrev = localPoint + k * distance * direction;
                    }

                    dots[i].NextPoint = dots[i].Position + nextPointDirection.normalized * k;
                }
            }


            var anchorNext = Vector2.zero;
            bool isNextAnchorPresent = dots[j].type is Anchor.Type.doublePinch or Anchor.Type.prevPinch;
            if(isNextAnchorPresent) {
                var rightAuxiliaryPoint = Vector2.zero;

                bool hasRightAuxiliaryPoint = false;
                if(isSuitForIntersection(dots[j].Position, dots[j].PrevPoint, containerLineStart, containerLineEnd)) {
                    hasRightAuxiliaryPoint = Line.FindIntersectionPoint(out rightAuxiliaryPoint, dots[j].Position, dots[j].PrevPoint, containerLineStart, containerLineEnd);
                }

                if(hasRightAuxiliaryPoint) {
                    dots[j].PrevPoint = 0.5f * (rightAuxiliaryPoint + dots[j].Position);
                    anchorNext = 0.5f * (rightAuxiliaryPoint + localPoint);
                } else {
                    
                    var prevPointDirection = dots[j].PrevPoint - dots[j].Position;
                    var direction = (containerLineEnd - containerLineStart).normalized;

                    float distance = (dots[j].Position - localPoint).magnitude;
                    float k = 0.25f;

                    if(Vector2.Dot(prevPointDirection, direction) > 0.0f) {
                        anchorNext = localPoint - k * distance * direction;
                    } else {
                        anchorNext = localPoint + k * distance * direction;
                    }

                    dots[j].PrevPoint = dots[j].Position + prevPointDirection.normalized * k;
                }

            }

            var newDot = new Dot(localPoint, anchorPrev, anchorNext);

            if(isPrevAnchorPresent && isNextAnchorPresent) {
                newDot.type = Anchor.Type.doublePinch;
            } else if(isPrevAnchorPresent) {
                newDot.type = Anchor.Type.prevPinch;
            } else if(isNextAnchorPresent) {
                newDot.type = Anchor.Type.nextPinch;
            } else {
                newDot.type = Anchor.Type.point;
            }

            dots.Insert(j, newDot);

            return newDot;
        }
        
        private static bool isSuitForIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            var v1 = (a - b).normalized;
            var v2 = (c - d).normalized;
            float mul = Vector2.Dot(v1, v2);
            return Mathf.Abs(mul) < 0.8f;
        }
        
    }

}
#endif