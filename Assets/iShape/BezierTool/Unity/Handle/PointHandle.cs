using UnityEditor;
using UnityEngine;

namespace iShape.BezierTool {

    public enum PointResult {
        None,
        Select,
        Deselect,
        Move
    }

    public class PointHandle {

        private static readonly int pinch_handle_hash = "PointHandle".GetHashCode();

        private readonly Mesh defaultMesh;
        private readonly Mesh hoverMesh;
        private readonly Mesh selectedMesh;
        private readonly Mesh highlightedMesh;

        private readonly Mesh strokeMesh;
        private readonly Material material;
        private readonly float radius;


        public PointHandle(Color normal, Color selected, Color hover, Color highlighted, float stroke, float radius) {
            this.radius = radius;
            this.material = Resources.Load<Material>("FillHandleMaterial");
            if(this.material == null) {
                throw new System.Exception("could not load material");
            }

            var points = getHandlePoints(radius);
            this.defaultMesh = HandleUtil.GetPolygonCircleMesh(points, normal);
            this.selectedMesh = HandleUtil.GetPolygonCircleMesh(points, selected);
            this.hoverMesh = HandleUtil.GetPolygonCircleMesh(points, hover);
            this.highlightedMesh = HandleUtil.GetPolygonCircleMesh(points, highlighted);
            this.strokeMesh = HandleUtil.GetPolygonStrokeMesh(points, highlighted, stroke);
        }


        private static Vector2[] getHandlePoints(float r) {
            const int n = 4;
            var points = new Vector2[n];

            float angle = - 0.5f * Mathf.PI;
            const float dAngle = 2 * Mathf.PI / n;

            for(int i = 0; i < n; i++) {
                points[i].x = r * Mathf.Cos(angle);
                points[i].y = r * Mathf.Sin(angle);
                angle += dAngle;
            }

            return points;
        }

        public PointResult MoveHandle(out Vector2 movedPosition, Anchor anchor, Vector3 pathPosition, float scale) {
            movedPosition = anchor.Position;
            var position = anchor.Position + (Vector2)pathPosition;
            var result = PointResult.None;

            int id = GUIUtility.GetControlID(pinch_handle_hash, FocusType.Passive);
            var handleEvent = Event.current;

            switch(handleEvent.type) {
                case EventType.MouseDown:
                    if(HandleUtility.nearestControl == id && handleEvent.button == 0) {
                        GUIUtility.hotControl = id;
                        result = PointResult.Select;
                        handleEvent.Use();
                    } else {
                        result = PointResult.Deselect;
                    }
                    break;


                case EventType.MouseUp:
                    if(GUIUtility.hotControl == id && handleEvent.button == 0) {
                        GUIUtility.hotControl = 0;
                        handleEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if(GUIUtility.hotControl == id) {

                        Vector2 pointBefore = position;

                        HandleUtil.Move2DHandle(ref position);
                        if(handleEvent.control) {
                            position = HandleUtil.RoundToGrid(position, scale);
                        }

                        if(pointBefore != position) {
                            result = PointResult.Move;
                            movedPosition = position - (Vector2)pathPosition;
                        }

                        handleEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    this.material.SetPass(0);

                    var matrix = Matrix4x4.Translate(new Vector3(position.x, position.y, pathPosition.z)) * Matrix4x4.Scale(new Vector3(scale, scale, scale));

                    if(anchor.IsMultiSelection) {
                        Graphics.DrawMeshNow(hoverMesh, matrix);
                        Graphics.DrawMeshNow(strokeMesh, matrix);
                    } else if(anchor.IsSelectedPoint) {
                        Graphics.DrawMeshNow(selectedMesh, matrix);
                    } else if(HandleUtility.nearestControl == id) { 
                        Graphics.DrawMeshNow(hoverMesh, matrix);
                        Graphics.DrawMeshNow(strokeMesh, matrix);
                    } else if(anchor.IsSelectedNextPinch || anchor.IsSelectedPrevPinch) {
                        Graphics.DrawMeshNow(selectedMesh, matrix);
                    } else if(anchor.isHighlighted) {
                        Graphics.DrawMeshNow(highlightedMesh, matrix);
                    } else {
                        Graphics.DrawMeshNow(defaultMesh, matrix);
                    }
                    break;
                case EventType.Layout:
                    float distance = HandleUtility.DistanceToCircle(position, scale * radius);
                    HandleUtility.AddControl(id, distance);
                    break;
            }

            return result;
        }


        public void DrawPotentialPoint(Vector2 position, float scale) {
            this.material.SetPass(0);
            var matrix = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(scale, scale, scale));
            Graphics.DrawMeshNow(defaultMesh, matrix);
        }
    }


}
