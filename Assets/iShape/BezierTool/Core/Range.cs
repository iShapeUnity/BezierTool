using System;

namespace iShape.BezierTool {

    internal struct Range {
        internal float start;
        internal float weight;
        internal float end => start + weight;
    }

    internal static class SegmentExtensions
    {
        internal static int FindIndex(this Range[] ranges, float value)
        {
            float x = value.Normalize();
            int n = ranges.Length;
            int left = 0;
            int right = n - 1;

            int j = -1;
            int i = right / 2;
            var range = ranges[i];

            while (i != j)
            {
                if (range.start > x)
                {
                    right = i - 1;
                }
                else if (range.end < x)
                {
                    left = i + 1;
                }
                else
                {
                    return i;
                }

                j = i;
                i = (left + right) / 2;

                range = ranges[i];
            }

            if (x < 0.5f)
            {
                return 0;
            }
            else
            {
                return n - 1;
            }
        }
    }

    internal static class FloatExtensions
    {
        internal static float Normalize(this float value)
        {
            if (value > 1)
            {
                return value - (int)value;
            }
            else if (value < 0)
            {
                int a = (int)Math.Abs(value) + 1;
                return a + value % 1;
            }
            else
            {
                return value;
            }
        }
    }
    
}