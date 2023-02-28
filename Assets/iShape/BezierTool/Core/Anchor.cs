using UnityEngine;

namespace iShape.BezierTool {

    [System.Serializable]
    public class Anchor {
        
        public enum Type {
            point,
            nextPinch,
            prevPinch,
            doublePinch
        }
        
        public Type type = Type.point;
        public bool IsSelectedPoint = false;
        public bool IsSelectedNextPinch = false;
        public bool IsSelectedPrevPinch = false;
        
        // is highlighted as neighbour
        public bool isHighlighted;

        // is highlighted as candidate for multi selection
        public bool IsMultiSelection = false;
        
        public Vector2 NextPoint;
        public Vector2 PrevPoint;
        public Vector2 Position;

        public bool IsNextPinchAvailable => type is Type.nextPinch or Type.doublePinch;
        public bool IsPrevPinchAvailable => type is Type.prevPinch or Type.doublePinch;

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

        public Anchor(Vector2 position, Vector2 prevPoint, Vector2 nextPoint) {
            Position = position;
            PrevPoint = prevPoint;
            NextPoint = nextPoint;
        }


        public Anchor(Anchor anchor, Vector2 move) {
            Position = anchor.Position + move;
            PrevPoint = anchor.PrevPoint + move;
            NextPoint = anchor.NextPoint + move;
        }
        
        public Anchor(Vector2 position) {
            Position = position;
            PrevPoint = Vector2.zero;
            NextPoint = Vector2.zero;
        }
    }

}