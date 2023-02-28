using System;
using System.Collections.Generic;
using UnityEngine;

namespace iShape.BezierTool {
    
    public readonly struct Curve {

        public readonly Spline[] splines;
        public readonly float length;
        private readonly Range[] ranges;
        private readonly bool isClosed;

        public Curve(IReadOnlyList<Anchor> anchors, bool isClosed, int stepCount = 20) {
            this.isClosed = isClosed;
            int n = anchors.Count;
            
            int m = isClosed ? n : n - 1;         
            splines = new Spline[m];
            var lengths = new float[m];
            
            float l = 0f;

            for (int i = 0; i < m; i++) {
                int j = (i + 1) % n;
                var spline = SplineBuilder.Create(anchors[i], anchors[j]);
                float dl = spline.GetLength(stepCount);
                splines[i] = spline;
                lengths[i] = dl;
                l += dl;
            }

            length = l;
            ranges = new Range[m];
            
            float w = 0f;
            
            for (int i = 0; i < m; i++) {
                float dl = lengths[i];
                
                float dw = dl / l;

                ranges[i] = new Range {
                    start = w,
                    weight = dw
                };

                w += dw;
            }
        }

        public Vector2[] GetPoints(float step, Vector2 pos) {
            int n = splines.Length;
            int m = (int)(length / step + 0.5f);
            var result = new Vector2[m + 1];

            float t = 0;
            float s = length / m;
            int i = 0;
            var sp = splines[0];
            var r = ranges[0];
            for (int j = 0; j < m; j++) {
                while (t > r.start && i < n) {
                    i += 1;
                    sp = splines[i];
                    r = ranges[i];
                }

                float k = (t - r.start) / r.weight;
                var p = sp.GetPoint(k) + pos;
                result[j] = p;
                t += s;
            }

            result[m] = sp.GetPoint(1) + pos;

            return result;
        }
        
        /// <summary>
        /// Get points for segment 
        /// </summary>
        /// <param name="start">Start path in percent</param>
        /// <param name="end">End path in percent</param>
        /// <param name="dw">Interval between points</param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2[] GetPoints(float start, float end, float dw, Vector2 pos) {
            float w;
            if (isClosed) {
                start = start.Normalize();
                end = end.Normalize();
                float delta = end - start;
                if (start <= end) {
                    w = delta;
                } else {
                    w = 1 + delta;
                }
            } else {
                start = Math.Min(1, Math.Max(0, start));
                end = Math.Min(1, Math.Max(0, end));
                if (start > end) {
                    (start, end) = (end, start);
                }

                w = end - start;
            }

            int i = ranges.FindIndex(start);
            var sp = splines[i];

            if (w < 0.005f * dw) {
                return new[] { GetPoint(start) + pos };
            }

            int count = (int)(w / dw + 0.5f);
            float s = w / count;
            
            var result = new Vector2[count + 1];

            var r = ranges[i];
            var t = start;
            for (int j = 0; j <= count; j++) {
                float k = (t - r.start) / r.weight;
                var p = sp.GetPoint(k) + pos;
                result[j] = p;
                t += s;
                while (t > r.end) {
                    if (t >= 1) {
                        if (isClosed) {
                            t = t.Normalize();
                            i = 0;
                        } else {
                            t = 1f;
                            break;
                        }
                    } else {
                        i += 1;    
                    }
                    r = ranges[i];
                    sp = splines[i];
                }
            }

            return result;
        }

        public Vector2 GetPoint(float weight) {
            int i = ranges.FindIndex(weight);
            var r = ranges[i];
            var sp = splines[i];
            float k = (weight - r.start) / r.weight;
            return sp.GetPoint(k);
        }
    }

}