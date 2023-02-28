using System.Collections.Generic;
using UnityEngine;

namespace iShape.BezierTool {

    public class BezierCurve : MonoBehaviour {
    
        [HideInInspector]
        [SerializeField]
        public List<Anchor> anchors = new();

        [HideInInspector]
        [SerializeField]
        public float stepLength = 0.1f;

        [HideInInspector]
        public int curvePrecision = 3;
        
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
            anchors.Clear();
            anchors.Add(new Anchor(new Vector2(-1, -1)));
            anchors.Add(new Anchor(new Vector2(-1, 1)));
            anchors.Add(new Anchor(new Vector2(1, 1)));
            anchors.Add(new Anchor(new Vector2(1, -1)));
        }

        private void OnEnable() {
            if (anchors.Count == 0) {
                this.Reset();
            }
        }
        
        public Vector2[] GetGlobalPath() {
            
            Vector3 globPos = transform.position; 
            Vector2 pos = globPos;
            var contour = new Contour(anchors, isClosed);
            return contour.GetPoints(stepLength, pos);
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
