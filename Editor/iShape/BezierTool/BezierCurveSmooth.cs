#if UNITY_EDITOR

using System.Collections.Generic;

namespace iShape.BezierTool {

    public static class BezierCurveSmooth {

        public static void Smoothe(this BezierCurve curve) {
            var oldDots = new List<Dot>(curve.dots);

            int n = curve.dots.Count;
            int start;
            int end;
            if (curve.isClosed) {
                start = 0;
                end = n - 1;
            } else {
                start = 1;
                end = n - 2;
            }

            int i = start;
            while (i <= end) {
                var a = oldDots[(i - 1 + n) % n];
                var b = oldDots[i];
                var c = oldDots[(i + 1) % n];

                ++i;
                if (a.IsNextPinchAvailable || b.IsNextPinchAvailable || b.IsPrevPinchAvailable || c.IsPrevPinchAvailable) {
                    continue;
                }

                var ba = b.Position - a.Position;
                var cb = c.Position - b.Position;

                var baLen = ba.magnitude;
                var cbLen = cb.magnitude;


            }
        }

        private static void Smoothe(this BezierCurve curve, int ai, int bi, int ci, float radius, float minStep) {
            var a = curve.dots[ai];
            var b = curve.dots[bi];
            var c = curve.dots[ci];
            
        }

    }

}
#endif