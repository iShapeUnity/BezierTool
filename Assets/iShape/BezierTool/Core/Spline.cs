using System;
using UnityEngine;

namespace iShape.BezierTool {
    public readonly struct Spline {
        private enum Type {
            line,
            cube,
            tetra
        }
        
        private readonly Type type;
        private readonly Vector2 pointA;
        private readonly Vector2 pointB;
        private readonly Vector2 anchorA;
        private readonly Vector2 anchorB;
       
        public Spline(Vector2 pointA, Vector2 pointB) {
            this.type = Type.line;
            this.pointA = pointA;
            this.pointB = pointB;
            this.anchorA = Vector2.zero;
            this.anchorB = Vector2.zero;
        }
        
        public Spline(Vector2 pointA, Vector2 pointB, Vector2 anchor) {
            this.type = Type.cube;
            this.pointA = pointA;
            this.pointB = pointB;
            this.anchorA = anchor;
            this.anchorB = Vector2.zero;
        }
        
        public Spline(Vector2 pointA, Vector2 pointB, Vector2 anchorA, Vector2 anchorB) {
            this.type = Type.tetra;
            this.pointA = pointA;
            this.pointB = pointB;
            this.anchorA = anchorA;
            this.anchorB = anchorB;
        }

        public float GetLength(int stepCount) {
            var prevPoint = GetPoint(0f);

            float step = 1.0f / stepCount;
            float path = step;
            float length = 0;

            for (int i = 0; i < stepCount; i++) {
                var nextPoint = GetPoint(path);
                length += Vector2.Distance(nextPoint, prevPoint);

                prevPoint = nextPoint;
                path += step;
            }

            return length;
        }
        
        public Vector2 GetPoint(float k) {
            return type switch {
                Type.line => GetPointFromLine(pointA, pointB, k),
                Type.cube => GetPointFromCube(pointA, pointB, anchorA, k),
                Type.tetra => GetPointFromTetra(pointA, pointB, anchorA, anchorB, k),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Vector2 GetPointFromLine(Vector2 pA, Vector2 pB, float k) {
            float x = pA.x + k * (pB.x - pA.x);
            float y = pA.y + k * (pB.y - pA.y);

            return new Vector2(x, y);
        }


        private static Vector2 GetPointFromCube(Vector2 pA, Vector2 pB, Vector2 pC, float k) {
            Vector2 ppA = GetPointFromLine(pA, pC, k);
            Vector2 ppB = GetPointFromLine(pC, pB, k);

            Vector2 p = GetPointFromLine(ppA, ppB, k);

            return p;
        }


        private static Vector2 GetPointFromTetra(Vector2 pA, Vector2 pB, Vector2 pC, Vector2 pD, float k) {
            Vector2 ppA = GetPointFromLine(pA, pC, k);
            Vector2 ppC = GetPointFromLine(pC, pD, k);
            Vector2 ppB = GetPointFromLine(pD, pB, k);

            Vector2 pppA = GetPointFromLine(ppA, ppC, k);
            Vector2 pppB = GetPointFromLine(ppC, ppB, k);

            Vector2 p = GetPointFromLine(pppA, pppB, k);

            return p;
        }
        
        public Vector2[] GetPoints(float stepLength) {
            float length = GetLength(20);
            int n = (int)(length / stepLength + 0.5f);
            float s = 1.0f / n;
            float t = 0;
            var result = new Vector2[n + 1];
            
            for (int i = 0; i < n; i++) {
                result[i] = GetPoint(t);
                t += s;
            }
            
            result[n] = GetPoint(t);

            return result;
        }
    }

}