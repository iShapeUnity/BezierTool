#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace iShape.BezierTool {

    public static class HandleUtil {
        private static bool GetMousePositionInWorld(out Vector3 position) {
            var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            return GetPointOnPlane(Matrix4x4.identity, r, out position);
        }

        internal static void Move2DHandle(ref Vector2 position) {
            if (GetMousePositionInWorld(out var mouseWorldPos)) {
                var pointOnPlane = Matrix4x4.identity.inverse.MultiplyPoint3x4(mouseWorldPos);

                if (Event.current.delta != Vector2.zero)
                    GUI.changed = true;

                position = pointOnPlane;
            }
        }

        private static bool GetPointOnPlane(Matrix4x4 planeTransform, Ray ray, out Vector3 position) {
            position = Vector3.zero;
            var p = new Plane(planeTransform * Vector3.forward, planeTransform.MultiplyPoint3x4(position));

            p.Raycast(ray, out var dist);

            if (dist < 0)
                return false;

            position = ray.GetPoint(dist);

            return true;
        }

        public static bool HasPointNearPolyLine(Vector2[] points, float z, float distance, out Vector2 point) {
            int n = points.Length;
            var vertices = new Vector3[n];
            vertices[0] = points[n - 2];
            vertices[0].z = z;
            for (int i = 1; i < n; i++) {
                vertices[i] = points[i - 1];
                vertices[i].z = z;
            }

            if (HandleUtility.DistanceToPolyLine(vertices) > distance) {
                point = Vector2.zero;
                return false;
            }

            point = HandleUtility.ClosestPointToPolyLine(vertices);

            return true;
        }


        internal static Vector2 RoundToGrid(Vector2 point, float scale) {
            const float step = 0.1f;
            float s = scale;
            float m = 1.0f;

            if (scale > 1) {
                while (s > 1.0f) {
                    s *= step;
                    m *= step;
                }
            } else {
                while (s < 1.0f) {
                    s /= step;
                    m /= step;
                }

                m *= step;
            }

            Vector2 p;

            p.x = Mathf.Round(point.x * m) / m;
            p.y = Mathf.Round(point.y * m) / m;

            return p;
        }
    }

}
#endif