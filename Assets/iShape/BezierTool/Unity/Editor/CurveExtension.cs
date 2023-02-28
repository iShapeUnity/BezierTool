using System.Collections.Generic;
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
                var points = spriteList[i].GetPoints(curveStepLength);

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
            }                
            
            return result;
        }
        
        internal static Anchor AddPoint(this List<Anchor> anchorList, Vector2 localPoint, Vector2 containerLineStart, Vector2 containerLineEnd, int index) {
            int n = anchorList.Count;

            int i = (index + n) % n;
            int j = (index + 1 + n) % n;


            var anchorPrev = Vector2.zero;
            bool isPrevAnchorPresent = anchorList[i].type is Anchor.Type.doublePinch or Anchor.Type.nextPinch;
            if (isPrevAnchorPresent) {
                var leftAuxiliaryPoint = Vector2.zero;
                bool hasLeftAuxiliaryPoint = false;
                if(isSuitForIntersection(anchorList[i].Position, anchorList[i].NextPoint, containerLineStart, containerLineEnd)) {
                    hasLeftAuxiliaryPoint = Line.FindIntersectionPoint(out leftAuxiliaryPoint, anchorList[i].Position, anchorList[i].NextPoint, containerLineStart, containerLineEnd);
                }

                if(hasLeftAuxiliaryPoint) {
                    anchorList[i].NextPoint = 0.5f * (leftAuxiliaryPoint + anchorList[i].Position);
                    anchorPrev = 0.5f * (leftAuxiliaryPoint + localPoint);
                } else {
                    // the vectors is almost collinear
                    var nextPointDirection = anchorList[i].NextPoint - anchorList[i].Position;
                    var direction = (containerLineStart - containerLineEnd).normalized;
                    
                    float distance = (anchorList[i].Position - localPoint).magnitude;
                    const float k = 0.25f;

                    if(Vector2.Dot(nextPointDirection, direction) > 0.0f) {
                        anchorPrev = localPoint - k * distance * direction;
                    } else {
                        anchorPrev = localPoint + k * distance * direction;
                    }

                    anchorList[i].NextPoint = anchorList[i].Position + nextPointDirection.normalized * k;
                }
            }


            var anchorNext = Vector2.zero;
            bool isNextAnchorPresent = anchorList[j].type is Anchor.Type.doublePinch or Anchor.Type.prevPinch;
            if(isNextAnchorPresent) {
                var rightAuxiliaryPoint = Vector2.zero;

                bool hasRightAuxiliaryPoint = false;
                if(isSuitForIntersection(anchorList[j].Position, anchorList[j].PrevPoint, containerLineStart, containerLineEnd)) {
                    hasRightAuxiliaryPoint = Line.FindIntersectionPoint(out rightAuxiliaryPoint, anchorList[j].Position, anchorList[j].PrevPoint, containerLineStart, containerLineEnd);
                }

                if(hasRightAuxiliaryPoint) {
                    anchorList[j].PrevPoint = 0.5f * (rightAuxiliaryPoint + anchorList[j].Position);
                    anchorNext = 0.5f * (rightAuxiliaryPoint + localPoint);
                } else {
                    
                    var prevPointDirection = anchorList[j].PrevPoint - anchorList[j].Position;
                    var direction = (containerLineEnd - containerLineStart).normalized;

                    float distance = (anchorList[j].Position - localPoint).magnitude;
                    float k = 0.25f;

                    if(Vector2.Dot(prevPointDirection, direction) > 0.0f) {
                        anchorNext = localPoint - k * distance * direction;
                    } else {
                        anchorNext = localPoint + k * distance * direction;
                    }

                    anchorList[j].PrevPoint = anchorList[j].Position + prevPointDirection.normalized * k;
                }

            }

            var newAnchor = new Anchor(localPoint, anchorPrev, anchorNext);

            if(isPrevAnchorPresent && isNextAnchorPresent) {
                newAnchor.type = Anchor.Type.doublePinch;
            } else if(isPrevAnchorPresent) {
                newAnchor.type = Anchor.Type.prevPinch;
            } else if(isNextAnchorPresent) {
                newAnchor.type = Anchor.Type.nextPinch;
            } else {
                newAnchor.type = Anchor.Type.point;
            }

            anchorList.Insert(j, newAnchor);

            return newAnchor;
        }
        
        private static bool isSuitForIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            var v1 = (a - b).normalized;
            var v2 = (c - d).normalized;
            float mul = Vector2.Dot(v1, v2);
            return Mathf.Abs(mul) < 0.8f;
        }
        
    }

}