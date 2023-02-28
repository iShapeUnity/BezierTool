using UnityEditor;
using UnityEngine;

namespace iShape.BezierTool {

    public enum PinchResult {
        None,
        Select,
        Move,
        Deselect
    }

    public class PinchHandle {

        private static readonly int pinch_handle_hash = "PinchHandle".GetHashCode();

        private readonly Mesh hoverMesh;
        private readonly Mesh defaultMesh;
        private readonly Mesh selectedMesh;
        private readonly Mesh strokeMesh;
        private readonly Material material;
        private readonly float radius;
        private readonly float stroke;
        private readonly Color strokeColor;

        public PinchHandle(Color normal, Color selected, Color hover, Color strokeColor, float stroke, float radius) {
            this.radius = radius;
            this.stroke = stroke;
            this.strokeColor = strokeColor;
            this.material = Resources.Load<Material>("FillHandleMaterial");
            if(this.material == null) {
                throw new System.Exception("could not load material");
            }

            var points = GetHandlePoints(radius);
            this.defaultMesh = HandleUtil.GetPolygonCircleMesh(points, normal);
            this.selectedMesh = HandleUtil.GetPolygonCircleMesh(points, selected);
            this.hoverMesh = HandleUtil.GetPolygonCircleMesh(points, hover);
            this.strokeMesh = HandleUtil.GetPolygonStrokeMesh(points, Color.yellow, 1.6f * stroke);
        }

        
        private static Vector2[] GetHandlePoints(float radius, int n = 32) {
            var points = new Vector2[n];

            float angle = 0.0f;
            float dAngle = 2 * Mathf.PI / n;

            for(int i = 0; i < n; i++) {
                points[i].x = radius * Mathf.Cos(angle);
                points[i].y = radius * Mathf.Sin(angle);
                angle += dAngle;
            }

            return points;
        }

        public PinchResult MoveHandle(out Vector2 movedPosition, Anchor anchor, Vector3 pathPosition, bool isNext, float scale) {
            movedPosition = (isNext ? anchor.NextPoint : anchor.PrevPoint) + (Vector2)pathPosition;
            Vector2 position = movedPosition;
            PinchResult result = PinchResult.None;

            int id = GUIUtility.GetControlID(pinch_handle_hash, FocusType.Passive);
            var handleEvent = Event.current;

            switch(handleEvent.type) {
                case EventType.MouseDown:
                    if(HandleUtility.nearestControl == id && handleEvent.button == 0) {
                        GUIUtility.hotControl = id;
                        result = PinchResult.Select;
                        handleEvent.Use();
                    } else {
                        result = PinchResult.Deselect;
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

                        position -= (Vector2)pathPosition;

                        if(pointBefore != position) {
                            result = PinchResult.Move;
                            movedPosition = position;
                        }

                        handleEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if(!(anchor.IsVisible && (isNext && anchor.IsNextPinchAvailable || !isNext && anchor.IsPrevPinchAvailable))) {
                        break;
                    }

                    Matrix4x4 matrix = Matrix4x4.Translate(new Vector3(position.x, position.y, pathPosition.z)) * Matrix4x4.Scale(new Vector3(scale, scale, scale));

                    this.material.SetPass(0);

                    if(isNext && anchor.IsSelectedNextPinch || !isNext && anchor.IsSelectedPrevPinch) {
                        // selected
                        Graphics.DrawMeshNow(selectedMesh, matrix);
                    } else if(HandleUtility.nearestControl == id) {
                        // highlighted
                        Graphics.DrawMeshNow(hoverMesh, matrix);
                        Graphics.DrawMeshNow(strokeMesh, matrix);
                    } else {
                        // default
                        Graphics.DrawMeshNow(defaultMesh, matrix);
                        Graphics.DrawMeshNow(strokeMesh, matrix);
                    }


                    var lineMatrix = Matrix4x4.Translate(pathPosition);
                    if (isNext) {
                        var lineMesh = this.getLine(anchor.NextPoint, anchor.Position, scale);
                        Graphics.DrawMeshNow(lineMesh, lineMatrix);
                    } else {
                        var lineMesh = this.getLine(anchor.PrevPoint, anchor.Position, scale);
                        Graphics.DrawMeshNow(lineMesh, lineMatrix);                            
                    }

                    break;
                case EventType.Layout:
                    float distance = HandleUtility.DistanceToCircle(position, scale * radius);
                    HandleUtility.AddControl(id, distance);
                    break;
            }

            return result;
        }


        private Mesh getLine(Vector2 a, Vector2 b, float scale) {
            var colors = new Color[4];
            
            for (int i = 0; i < 4; i++) {
                colors[i] = this.strokeColor;
            }

            var triangles = new int[3 * 2];
            
            int trianglesCounter = 0;

            triangles[trianglesCounter++] = 0;
            triangles[trianglesCounter++] = 1;
            triangles[trianglesCounter++] = 3;

            triangles[trianglesCounter++] = 1;
            triangles[trianglesCounter++] = 2;
            triangles[trianglesCounter] = 3;
            
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float angle = Mathf.Atan2(dy, dx);
            float nAngle = angle + 0.5f * Mathf.PI;
            float x = 0.5f * this.stroke * scale * Mathf.Cos(nAngle);
            float y = 0.5f * this.stroke * scale * Mathf.Sin(nAngle);

            Vector2 offset;
            offset.x = (this.radius - this.stroke) * scale * Mathf.Cos(angle);
            offset.y = (this.radius - this.stroke) * scale * Mathf.Sin(angle);

            var dV = new Vector2(x, y);

            var vertices = new Vector3[4];
            vertices[0] = a - dV - offset;
            vertices[1] = a + dV - offset;
            vertices[2] = b + dV;
            vertices[3] = b - dV;

            var mesh = new Mesh {
                vertices = vertices,
                colors = colors,
                triangles = triangles
            };

            return mesh;
        }

    }


}
