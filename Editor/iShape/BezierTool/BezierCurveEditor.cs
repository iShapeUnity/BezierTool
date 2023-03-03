using UnityEditor;
using UnityEngine;

namespace iShape.BezierTool {

    [CustomEditor(typeof(BezierCurve))]
    [CanEditMultipleObjects]
    public class BezierCurveEditor : UnityEditor.Editor
    {
        private const int maxCurveShapePrecision = 10;
        private bool hasCandidatePoint;
        private Vector2 candidatePoint;
        private Vector2 mouseStartDrag;
        private static readonly Color orange = new Color(1.0f, 0.5f, 0.0f);
        private static readonly Color orangeHalfBlend = new Color(1.0f, 0.5f, 0.0f, 0.95f);
        private static readonly Color orangeFullBlend = new Color(1.0f, 0.5f, 0.0f, 0.1f);

        private Texture[] _icons;

        private Texture[] icons {
            get {
                if (_icons == null) {
                    _icons = new Texture[6];

                    _icons[0] = loadTexture("add_point");
                    _icons[1] = loadTexture("remove_point");
                    _icons[2] = loadTexture("add_pinch_for_point");
                    _icons[3] = loadTexture("remove_pinch_for_point");
                    _icons[4] = loadTexture("add_pinch_between_points");
                    _icons[5] = loadTexture("remove_pinch_between_points");
                }

                return _icons;
            }
        }

        private static Texture loadTexture(string name) {
            var texture = Resources.Load<Texture>(name);
            if (texture == null) {
                throw new System.Exception("could not load material");
            }

            return texture;
        }


        private GUIStyle _positionStyle;

        private GUIStyle positionStyle {
            get {
                if (_positionStyle == null) {
                    _positionStyle = new GUIStyle {normal = {textColor = orange}, fontStyle = FontStyle.Bold};
                }

                return _positionStyle;
            }
        }

        private PinchHandle _pinchHandle;

        private PinchHandle pinchHandle {
            get {
                if (_pinchHandle == null) {
                    _pinchHandle = new PinchHandle(Color.clear, orange, orange, Color.white, 0.25f, 1.0f);
                }

                return _pinchHandle;
            }
        }

        private PointHandle _pointHandle;

        private PointHandle pointHandle {
            get {
                if (_pointHandle == null) {
                    _pointHandle = new PointHandle(new Color(0.1f, 0.1f, 0.1f), orange, orange, Color.white,
                        0.5f, 1.2f);
                }

                return _pointHandle;
            }
        }


        private MouseSelectionHandle _multiSelection;

        private MouseSelectionHandle MultiSelection {
            get {
                if (_multiSelection == null) {
                    _multiSelection = new MouseSelectionHandle(orangeFullBlend, orangeHalfBlend, 0.2f);
                }

                return _multiSelection;
            }
        }


        private void UndoCallback() {
            var shape = target as BezierCurve;
            if (shape != null) {
                shape.UpdateCurve();
            }
        }


        private void OnEnable() {
            Undo.undoRedoPerformed += UndoCallback;
        }


        private void OnSceneGUI() {
            var shape = target as BezierCurve;

            if (shape == null) {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var scene = SceneView.currentDrawingSceneView;
            if (scene == null) {
                return; 
            }

            float scale = scene.camera.farClipPlane * 0.000007f;

            int selectedCount = shape.SelectedCount();

            var anchors = shape.dots;
            bool isModified = false;
            var pathPosition = shape.transform.position;



            int n = anchors.Count;

            // drawing handles
            for (int i = 0; i < n; i++) {
                var anchor = anchors[i];

                {
                    var result = pointHandle.MoveHandle(out var movedPosition, anchor, pathPosition, scale);
                    switch (result) {
                        case PointResult.Select when !anchor.IsSelectedPoint:
                            shape.DeselectAll();
                            anchors[(i - 1 + n) % n].isHighlighted = true;
                            anchors[(i + 1) % n].isHighlighted = true;
                            anchors[i].IsSelectedPoint = true;
                            break;
                        case PointResult.Move: {
                            isModified = true;
                            Undo.RecordObject(target, "Changed anchor point");
                            var delta = movedPosition - anchor.Position;
                            foreach (var iAnchor in anchors) {
                                if (iAnchor.IsSelectedPoint) {
                                    iAnchor.Move(delta);
                                }
                            }

                            break;
                        }
                    }
                }

                {
                    bool isHidden = !shape.isClosed && i == n - 1;
                    if (!isHidden) {
                        var result = pinchHandle.MoveHandle(out var movedPosition, anchor, pathPosition, true, scale);
                        switch (result) {
                            case PinchResult.Select when !anchor.IsSelectedNextPinch:
                                shape.DeselectAll();
                                anchors[(i - 1 + n) % n].isHighlighted = true;
                                anchors[(i + 1) % n].isHighlighted = true;
                                anchors[i].IsSelectedNextPinch = true;
                                break;
                            case PinchResult.Move: {
                                isModified = true;
                                Undo.RecordObject(target, "Changed next pinch");
                                anchor.NextPoint = movedPosition;
                                if (Event.current.shift) {
                                    var direction = (movedPosition - anchor.Position).normalized;
                                    float length = (anchor.Position - anchor.PrevPoint).magnitude;
                                    anchor.PrevPoint = anchor.Position - direction * length;
                                }

                                break;
                            }
                        }
                    }
                }

                {
                    bool isHidden = !shape.isClosed && i == 0;
                    if (!isHidden) {
                        var result = pinchHandle.MoveHandle(out var movedPosition, anchor, pathPosition, false, scale);
                        switch (result) {
                            case PinchResult.Select when !anchor.IsSelectedPrevPinch:
                                shape.DeselectAll();
                                anchors[(i - 1 + n) % n].isHighlighted = true;
                                anchors[(i + 1) % n].isHighlighted = true;
                                anchors[i].IsSelectedPrevPinch = true;
                                break;
                            case PinchResult.Move: {
                                isModified = true;
                                Undo.RecordObject(target, "Changed prev pinch");
                                anchor.PrevPoint = movedPosition;
                                if (Event.current.shift) {
                                    var direction = (movedPosition - anchor.Position).normalized;
                                    float length = (anchor.Position - anchor.NextPoint).magnitude;
                                    anchor.NextPoint = anchor.Position - direction * length;
                                }

                                break;
                            }
                        }                        
                    }
                }

                // draw coordinate label
                if (selectedCount == 1) {
                    if (anchor.IsSelectedPoint) {
                        var position =
                            new Vector3(anchor.Position.x + pathPosition.x, anchor.Position.y + pathPosition.y, 0) +
                            new Vector3(1f, 3f, 0f) * scale;
                        var text = i + ". " + anchor.Position;
                        Handles.Label(position, text, positionStyle);
                    } else if (anchor.IsSelectedNextPinch) {
                        var position =
                            new Vector3(anchor.NextPoint.x + pathPosition.x, anchor.NextPoint.y + pathPosition.y,
                                0) + new Vector3(1f, 3f, 0f) * scale;
                        Handles.Label(position, anchor.NextPoint.ToString(), positionStyle);
                    } else if (anchor.IsSelectedPrevPinch) {
                        var position =
                            new Vector3(anchor.PrevPoint.x + pathPosition.x, anchor.PrevPoint.y + pathPosition.y,
                                0) + new Vector3(1f, 3f, 0f) * scale;
                        Handles.Label(position, anchor.PrevPoint.ToString(), positionStyle);
                    }
                }
            }
            

            // toolkit buttons
            if (shape.HasAnySelection() && icons.Length > 0) {
                Handles.BeginGUI();

                const float btnSize = 40f;

                float width = 0.5f * scene.camera.pixelRect.width;
                
                GUILayout.BeginArea(new Rect(0, 2, width, btnSize));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(icons[0], "insert new nodes into selected segments"),
                    GUILayout.Width(btnSize), GUILayout.Height(btnSize))) {
                    // add point
                    Event.current.Use();
                    Undo.RecordObject(target, "Add points");
                    if (shape.AddPointsToSelected()) {
                        isModified = true;
                        GUI.changed = true;
                    }
                }

                if (GUILayout.Button(new GUIContent(icons[1], "delete selected nodes"), GUILayout.Width(btnSize),
                    GUILayout.Height(btnSize))) {
                    // remove point
                    Event.current.Use();
                    Undo.RecordObject(target, "Remove points");
                    if (shape.RemoveSelected()) {
                        shape.DeselectAll();
                        GUI.changed = true;
                        isModified = true;
                    }
                }

                if (GUILayout.Button(new GUIContent(icons[2], "make selected nodes smooth"), GUILayout.Width(btnSize),
                    GUILayout.Height(btnSize))) {
                    // add pinches to selected points
                    Event.current.Use();
                    Undo.RecordObject(target, "Add pinches to points");
                    if (shape.SelectedPointAddPinches()) {
                        GUI.changed = true;
                        isModified = true;
                    }
                }

                if (GUILayout.Button(new GUIContent(icons[3], "make selected nodes corner"), GUILayout.Width(btnSize),
                    GUILayout.Height(btnSize))) {
                    // remove pinches from selected points
                    Event.current.Use();
                    Undo.RecordObject(target, "Remove pinches from points");
                    shape.SelectedPointRemovePinches();
                    GUI.changed = true;
                    isModified = true;
                }

                if (GUILayout.Button(new GUIContent(icons[4], "make selected segments curves"), GUILayout.Width(btnSize),
                    GUILayout.Height(btnSize))) {
                    // add pinches between selected points
                    Event.current.Use();
                    Undo.RecordObject(target, "Add pinches between points");
                    if (shape.SelectedPointsAddBetweenPinches()) {
                        GUI.changed = true;
                        isModified = true;
                    }
                }

                if (GUILayout.Button(new GUIContent(icons[5], "make selected segments lines"), GUILayout.Width(btnSize),
                    GUILayout.Height(btnSize))) {
                    // remove pinches between selected points
                    Event.current.Use();
                    Undo.RecordObject(target, "Remove pinches between points");
                    if (shape.SelectedPointsRemoveBetweenPinches()) {
                        GUI.changed = true;
                        isModified = true;
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.EndArea();
                Handles.EndGUI();
            }


            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.shift) {
                if (shape.HasAnySelection()) {
                    shape.DeselectAll();
                    GUI.changed = true;
                }
            }


            // mouse area selection
            {
                var result = MultiSelection.MoveHandle(out var selectionRect, scale);

                if (result == MouseSelectionResult.Move) {
                    var localRect = selectionRect;
                    localRect.position = selectionRect.position - (Vector2) pathPosition;
                    shape.Highlight(localRect);
                } else if (result == MouseSelectionResult.End) {
                    var localRect = selectionRect;
                    localRect.position = selectionRect.position - (Vector2) pathPosition;
                    shape.Select(localRect);
                }
            }

            // add point with control + mouse click
            {
                if (Event.current.control) {
                    var points = shape.GetGlobalPath();

                    // TODO adjust scale
                    this.hasCandidatePoint = HandleUtil.HasPointNearPolyLine(points, shape.transform.position.z, 15.0f, out var newCandidatePoint);
                    if (this.hasCandidatePoint) {
                        this.candidatePoint = newCandidatePoint;
                        if (Event.current.type == EventType.MouseDown) {
                            Undo.RecordObject(target, "Add anchor point");
                            Event.current.Use();
                            shape.AddPoint(this.candidatePoint);
                            shape.UpdateCurve();
                        } else {
                            HandleUtility.Repaint();
                        }
                    }
                } else if (this.hasCandidatePoint) {
                    this.hasCandidatePoint = false;
                    HandleUtility.Repaint();
                }

                switch (Event.current.type) {
                    case EventType.Layout when this.hasCandidatePoint:
                        // fix object deselection problem while adding point control + mouse click
                        HandleUtility.AddDefaultControl(0);
                        break;
                    case EventType.Repaint: {
                        if (this.hasCandidatePoint) {
                            pointHandle.DrawPotentialPoint(this.candidatePoint, scale);
                        }

                        break;
                    }
                }
            }

            if (target != null) {
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(target);
                }

                if (isModified) {
                    shape.UpdateCurve();
                }
            }
        }


        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            var shape = target as BezierCurve;
            if (shape == null) {
                return;
            }

            GUILayout.Label("Curve", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var newCurvePrecision = EditorGUILayout.IntSlider("Precision", shape.curvePrecision, 0, maxCurveShapePrecision);
            if (newCurvePrecision != shape.curvePrecision) {
                shape.curvePrecision = newCurvePrecision;
                shape.UpdateCurve();

                EditorUtility.SetDirty(target);
                return;
            }


            EditorGUILayout.Space();

            var rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // build section

        }
    }
}