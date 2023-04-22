#if UNITY_EDITOR

using UnityEngine;

namespace iShape.BezierTool {

    internal static class Line {
        internal static bool FindIntersectionPoint(out Vector2 point, Vector2 startLineA, Vector2 endLineA, Vector2 startLineB, Vector2 endLineB) {
            float divider = (startLineA.x - endLineA.x) * (startLineB.y - endLineB.y) - (startLineA.y - endLineA.y) * (startLineB.x - endLineB.x);

            if (!Mathf.Approximately(divider, 0.0f)) {
                float xyA = startLineA.x * endLineA.y - startLineA.y * endLineA.x;
                float xyB = startLineB.x * endLineB.y - startLineB.y * endLineB.x;


                float invert_divider = 1.0f / divider;

                float x = xyA * (startLineB.x - endLineB.x) - (startLineA.x - endLineA.x) * xyB;
                float y = xyA * (startLineB.y - endLineB.y) - (startLineA.y - endLineA.y) * xyB;

                point = new Vector2(x * invert_divider, y * invert_divider);

                return true;
            }

            point = Vector2.zero;

            return false;
        }

        private static Vector2 NearestPointToEdge(Vector2 point, Vector2 start, Vector2 end) {
            Vector2 rhs = point - start;
            Vector2 vector = end - start;
            float magnitude = vector.magnitude;

            if (magnitude > 1E-06f) {
                vector /= magnitude;
            }

            float num = Vector2.Dot(vector, rhs);
            num = Mathf.Clamp(num, 0f, magnitude);
            return start + vector * num;
        }


        internal static float SqrDistanceToEdge(Vector2 point, Vector2 start, Vector2 end) {
            Vector2 nearest = NearestPointToEdge(point, start, end);
            Vector2 distance = point - nearest;
            return distance.sqrMagnitude;
        }
    }

}
#endif