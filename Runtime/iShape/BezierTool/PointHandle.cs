#if UNITY_EDITOR

using iShape.Mesh2d;
using Unity.Collections;
using Unity.Mathematics;
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

            var rectMesh = MeshGenerator.FillCircle(float2.zero, radius, 4, false, 0, Allocator.Temp);

            this.defaultMesh = new Mesh();
            rectMesh.Fill(defaultMesh, normal);
            
            this.selectedMesh = new Mesh();
            rectMesh.Fill(selectedMesh, selected);
            
            this.hoverMesh = new Mesh();
            rectMesh.Fill(hoverMesh, hover);

            this.highlightedMesh = new Mesh();
            rectMesh.Fill(highlightedMesh, highlighted);
            
            rectMesh.Dispose();

            this.strokeMesh = MeshGenerator.StrokeForCircle(float2.zero, radius, 16, new StrokeStyle(1.6f * stroke), 0, Allocator.Temp).Convert(Color.yellow);
        }

        public PointResult MoveHandle(out Vector2 movedPosition, Dot dot, Vector3 pathPosition, float scale) {
            movedPosition = dot.Position;
            var position = dot.Position + (Vector2)pathPosition;
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

                    if(dot.IsMultiSelection) {
                        Graphics.DrawMeshNow(hoverMesh, matrix);
                        Graphics.DrawMeshNow(strokeMesh, matrix);
                    } else if(dot.IsSelectedPoint) {
                        Graphics.DrawMeshNow(selectedMesh, matrix);
                    } else if(HandleUtility.nearestControl == id) { 
                        Graphics.DrawMeshNow(hoverMesh, matrix);
                        Graphics.DrawMeshNow(strokeMesh, matrix);
                    } else if(dot.IsSelectedNextPinch || dot.IsSelectedPrevPinch) {
                        Graphics.DrawMeshNow(selectedMesh, matrix);
                    } else if(dot.isHighlighted) {
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

#endif
