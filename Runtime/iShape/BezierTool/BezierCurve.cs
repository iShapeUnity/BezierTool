using System.Collections.Generic;
using iShape.Spline;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace iShape.BezierTool {

    public class BezierCurve : MonoBehaviour {
    
        [FormerlySerializedAs("anchors")]
        [HideInInspector]
        [SerializeField]
        public List<Dot> dots = new();

        [HideInInspector]
        [SerializeField]
        public float stepLength = 0.1f;

        [HideInInspector]
        public int curvePrecision = 3;
        
        [HideInInspector]
        public float cornerRadius = 0.2f;
        
        [SerializeField]
        public bool isClosed = true;

        [SerializeField]
        public Color color = Color.white;
    
        public delegate void BezierCurveUpdatedHandler();
        public event BezierCurveUpdatedHandler OnBezierCurveUpdated;
        
        public void UpdateCurve() {
            stepLength = Mathf.Pow(2.0f, curvePrecision - 6);
            OnBezierCurveUpdated?.Invoke();
        }

        public void RemoveSubscribers() {
            OnBezierCurveUpdated = null;
        }

        public void Reset() {
            dots.Clear();
            dots.Add(new Dot(new Vector2(-1, -1)));
            dots.Add(new Dot(new Vector2(-1, 1)));
            dots.Add(new Dot(new Vector2(1, 1)));
            dots.Add(new Dot(new Vector2(1, -1)));
        }

        private void OnEnable() {
            if (dots.Count == 0) {
                this.Reset();
            }
        }
        
        public Vector2[] GetGlobalPath() {
            Vector3 globPos = transform.position; 
            float2 pos = new float2(globPos.x, globPos.y);
            var contour = this.BuildContour(Allocator.Temp);
            var nPoints = contour.GetPoints(stepLength, pos, Allocator.Temp);
            contour.Dispose();
            var points = nPoints.Reinterpret<Vector2>().ToArray();
            nPoints.Dispose();
            return points;
        }

        public Contour BuildContour(Allocator allocator) {
            return BuildContour(20, allocator);
        }
        
        public Contour BuildContour(int count, Allocator allocator) {
            var anchors = dots.CreateAnchors(Allocator.Temp);
            var contour = new Contour(anchors, isClosed, count, allocator);
            anchors.Dispose();
            return contour;
        }
        
        public Curve BuildCurve(Allocator allocator) {
            return BuildCurve(20, allocator);
        }
        
        public Curve BuildCurve(int count, Allocator allocator) {
            var anchors = dots.CreateAnchors(Allocator.Temp);
            var curve = new Curve(anchors, isClosed, count, allocator);
            anchors.Dispose();
            return curve;
        }

#if UNITY_EDITOR
        // [UnityEditor.DrawGizmo(UnityEditor.GizmoType.Pickable)]
        private void OnDrawGizmos() {
            if(Application.isPlaying || UnityEditor.SceneView.currentDrawingSceneView == null) {
                // remove debug draw calls
                return;
            }
            Vector3 globPos = this.transform.position; 

            var points = this.GetGlobalPath();
            int n = points.Length;
            if (n > 1) {
                Gizmos.color = color;

                Vector3 a;
                int start;
                if (isClosed) {
                    a = points[n - 1];
                    start = 0;
                } else {
                    a = points[0];
                    start = 1;
                }

                a.z = globPos.z;
                for (int i = start; i < n; i++) {
                    Vector3 b = points[i];
                    b.z = globPos.z;
                    Gizmos.DrawLine(a, b);
                    a = b;
                }
            }
        }
#endif
    }

}
