namespace iShape.BezierTool {

    public static class SplineBuilder {

        internal static Spline Create(Anchor first, Anchor second) {
            var pA = first.Position;
            var pB = second.Position;

            if (first.IsNextPinchAvailable && second.IsPrevPinchAvailable) {
                var pC = first.NextPoint;
                var pD = second.PrevPoint;

                return new Spline(pA, pB, pC, pD);
            }

            if (first.IsNextPinchAvailable) {
                var pC = first.NextPoint;

                return new Spline(pA, pB, pC);
            }

            if (second.IsPrevPinchAvailable) {
                var pC = second.PrevPoint;
                
                return new Spline(pA, pB, pC);
            }

            return new Spline(pA, pB);
        }

    }

}