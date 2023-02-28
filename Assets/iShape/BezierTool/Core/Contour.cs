using System;
using System.Collections.Generic;
using UnityEngine;

namespace iShape.BezierTool {

    public readonly struct Contour {

        private readonly Spline[] splines;
        private readonly float[] lengths;

        public Contour(IReadOnlyList<Anchor> anchors, bool isClosed, int stepCount = 20) {
            int n = anchors.Count;
            int m = isClosed ? n : n - 1;
            splines = new Spline[m];
            lengths = new float[m];
            for (int i = 0; i < m; i++) {
                int j = (i + 1) % n;
                var spline = SplineBuilder.Create(anchors[i], anchors[j]);
                splines[i] = spline;
                lengths[i] = spline.GetLength(stepCount);
            }
        }

        public Vector2[] GetPoints(float step, Vector2 pos) {
            int n = splines.Length;

            int count = 1;
            for (int i = 0; i < n; i++) {
                float dl = lengths[i];
                int m = (int)(dl / step + 0.5f);
                m = Math.Max(1, m);
                count += m;
            }
            
            var result = new Vector2[count];
            int index = 0;
            
            for (int i = 0; i < n; i++) {
                var sp = splines[i];
                float dl = lengths[i];

                int m = (int)(dl / step + 0.5f);
                m = Math.Max(1, m);

                float s = 1f / m;
                float t = 0;
                for (int j = 0; j < m; j++) {
                    result[index++] = sp.GetPoint(t) + pos; 
                    t += s;
                }
            }

            Debug.Assert(index == count - 1, "last index is not equal count - 1");
            
            result[index] = splines[n - 1].GetPoint(1) + pos;

            return result;
        }
    }

}