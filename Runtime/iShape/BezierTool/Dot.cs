using iShape.Spline;
using Unity.Mathematics;
using UnityEngine;

namespace iShape.BezierTool {

    [System.Serializable]
    public class Dot {
        
        public Anchor Anchor;
        public bool IsSelectedPoint ;
        public bool IsSelectedNextPinch;
        public bool IsSelectedPrevPinch;
        
        // is highlighted as neighbour
        public bool isHighlighted;

        // is highlighted as candidate for multi selection
        public bool IsMultiSelection;

        public Anchor.Type type {
            get => Anchor.type;
            set => Anchor.type = value;
        }
        
        public Vector2 NextPoint {
            get => Anchor.NextPoint;
            set => Anchor.NextPoint = value;
        }
        public Vector2 PrevPoint {
            get => Anchor.PrevPoint;
            set => Anchor.PrevPoint = value;
        }
        public Vector2 Position {
            get => Anchor.Position;
            set => Anchor.Position = value;
        }

        public bool IsNextPinchAvailable => Anchor.type is Anchor.Type.nextPinch or Anchor.Type.doublePinch;
        public bool IsPrevPinchAvailable => Anchor.type is Anchor.Type.prevPinch or Anchor.Type.doublePinch;

        public bool IsVisible => IsSelectedNextPinch || IsSelectedPrevPinch || IsSelectedPoint || isHighlighted;
        
        public void Deselect() {
            this.IsSelectedPoint = false;
            this.IsSelectedNextPinch = false;
            this.IsSelectedPrevPinch = false;
            this.isHighlighted = false;
            this.IsMultiSelection = false;
        }
        
        public void Transform(Vector2 position) {
            var delta = position - Position;
            NextPoint += delta;
            PrevPoint += delta;
            Position = position;
        }

        public void Move(Vector2 delta) {
            NextPoint += delta;
            PrevPoint += delta;
            Position += delta;
        }

        public Dot(Vector2 position, Vector2 prevPoint, Vector2 nextPoint) {
            Position = position;
            PrevPoint = prevPoint;
            NextPoint = nextPoint;
        }


        public Dot(Anchor anchor, Vector2 move) {
            Position = anchor.Position + new float2(move);
            PrevPoint = anchor.PrevPoint + new float2(move);
            NextPoint = anchor.NextPoint + new float2(move);
        }
        
        public Dot(Anchor anchor) {
            Position = anchor.Position;
            PrevPoint = anchor.PrevPoint;
            NextPoint = anchor.NextPoint;
        }
        
        public Dot(Vector2 position) {
            Position = position;
            PrevPoint = Vector2.zero;
            NextPoint = Vector2.zero;
        }
    }

}