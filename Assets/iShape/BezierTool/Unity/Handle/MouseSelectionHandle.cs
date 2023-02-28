using UnityEditor;
using UnityEngine;

namespace iShape.BezierTool {

    public enum MouseSelectionResult {
        None,
        Move,
        End
    }

    public class MouseSelectionHandle {

        private static readonly int hash = "MultiSelection".GetHashCode();

        private readonly Mesh rectMesh;

        private readonly Material material;

        private bool isDrag;
        private Vector2 mouseStartDrag;
        private Vector2 mouseEndDrag;
        private Rect mouseDragRect;
        private readonly float stroke;

        public MouseSelectionHandle(Color fillColor, Color strokeColor, float stroke) {
            this.stroke = stroke;
            this.material = Resources.Load<Material>("FillHandleMaterial");
            if(this.material == null) {
                throw new System.Exception("could not load material");
            }

            this.rectMesh = GetRectMesh(fillColor, strokeColor);
        }


        private static Mesh GetRectMesh(Color fillColor, Color strokeColor) {
            var vertices = new Vector3[8];
            var colors = new Color[8];
            var triangles = new int[3 * 10];
            
            for(int i = 0; i < 4; i++) {
                colors[i] = fillColor;
            }

            for(int i = 4; i < 8; i++) {
                colors[i] = strokeColor;
            }

            int trianglesCounter = 0;

            triangles[trianglesCounter++] = 0;
            triangles[trianglesCounter++] = 1;
            triangles[trianglesCounter++] = 3;

            triangles[trianglesCounter++] = 1;
            triangles[trianglesCounter++] = 2;
            triangles[trianglesCounter++] = 3;


            // left

            triangles[trianglesCounter++] = 0;
            triangles[trianglesCounter++] = 4;
            triangles[trianglesCounter++] = 5;

            triangles[trianglesCounter++] = 0;
            triangles[trianglesCounter++] = 5;
            triangles[trianglesCounter++] = 1;


            // top

            triangles[trianglesCounter++] = 2;
            triangles[trianglesCounter++] = 1;
            triangles[trianglesCounter++] = 5;

            triangles[trianglesCounter++] = 2;
            triangles[trianglesCounter++] = 5;
            triangles[trianglesCounter++] = 6;


            // right

            triangles[trianglesCounter++] = 7;
            triangles[trianglesCounter++] = 2;
            triangles[trianglesCounter++] = 3;

            triangles[trianglesCounter++] = 7;
            triangles[trianglesCounter++] = 2;
            triangles[trianglesCounter++] = 6;


            // bottom

            triangles[trianglesCounter++] = 0;
            triangles[trianglesCounter++] = 7;
            triangles[trianglesCounter++] = 4;

            triangles[trianglesCounter++] = 0;
            triangles[trianglesCounter++] = 3;
            triangles[trianglesCounter] = 7;

            var mesh = new Mesh() {
                vertices = vertices,
                colors = colors,
                triangles = triangles
            };

            mesh.MarkDynamic();

            return mesh;
        }


        private void updateMesh(Rect rect, float scale) {
            float st = stroke * scale;

            var vertices = new Vector3[8];
            vertices[0] = rect.min;
            vertices[1] = new Vector2(rect.min.x, rect.max.y);
            vertices[2] = rect.max;
            vertices[3] = new Vector2(rect.max.x, rect.min.y);

            vertices[4] = vertices[0] + new Vector3(-st, -st);
            vertices[5] = vertices[1] + new Vector3(-st, st);
            vertices[6] = vertices[2] + new Vector3(st, st);
            vertices[7] = vertices[3] + new Vector3(st, -st);

            this.rectMesh.vertices = vertices;
        }

        public MouseSelectionResult MoveHandle(out Rect rect, float scale) {
            var handleEvent = Event.current;

            if(!handleEvent.shift) {
                rect = Rect.zero;
                return MouseSelectionResult.None;
            }

            int id = GUIUtility.GetControlID(hash, FocusType.Passive);
            bool newDragStatus = this.isDrag;

            switch(handleEvent.type) {
                case EventType.MouseDown:
                    if(handleEvent.button == 0 && handleEvent.shift) {
                        newDragStatus = true;
                        HandleUtil.Move2DHandle(ref this.mouseStartDrag);
                        handleEvent.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if(this.isDrag) {
                        newDragStatus = false;
                        handleEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if(!handleEvent.shift || handleEvent.button != 0) {
                        newDragStatus = false;
                        break;
                    }
                    newDragStatus = true;
                    HandleUtil.Move2DHandle(ref this.mouseEndDrag);


                    float x;
                    float dx;

                    if(mouseEndDrag.x > mouseStartDrag.x) {
                        x = mouseStartDrag.x;
                        dx = mouseEndDrag.x - mouseStartDrag.x;
                    } else {
                        x = mouseEndDrag.x;
                        dx = mouseStartDrag.x - mouseEndDrag.x;
                    }

                    float y;
                    float dy;

                    if(mouseEndDrag.y > mouseStartDrag.y) {
                        y = mouseStartDrag.y;
                        dy = mouseEndDrag.y - mouseStartDrag.y;
                    } else {
                        y = mouseEndDrag.y;
                        dy = mouseStartDrag.y - mouseEndDrag.y;
                    }

                    this.mouseDragRect = new Rect(x, y, dx, dy);

                    handleEvent.Use();
                    
                    break;
                case EventType.MouseMove:
                    newDragStatus = false;
                    break;
                case EventType.Repaint:
                    if(!this.isDrag) {
                        break;
                    }

                    this.material.SetPass(0);

                    var matrix = Matrix4x4.identity;
                    this.updateMesh(this.mouseDragRect, scale);
                    Graphics.DrawMeshNow(rectMesh, matrix);

                    break;
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(id);
                    break;
            }

            MouseSelectionResult result;
            rect = this.mouseDragRect;

            if(this.isDrag && newDragStatus == false) {
                result = MouseSelectionResult.End;
                this.mouseDragRect = Rect.zero;
            } else if(this.isDrag) {
                result = MouseSelectionResult.Move;
            } else {
                result = MouseSelectionResult.None;
            }

            this.isDrag = newDragStatus;

            return result;
        }
    }


}
