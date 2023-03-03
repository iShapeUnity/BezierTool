using System.Collections.Generic;
using iShape.Spline;
using Unity.Collections;

namespace iShape.BezierTool {

    public static class DotArray {

        public static NativeArray<Anchor> CreateAnchors(this Dot[] dots, Allocator allocator) {
            int n = dots.Length;
            var anchors = new NativeArray<Anchor>(n, allocator);
            for (int i = 0; i < n; ++i) {
                anchors[i] = dots[i].Anchor;
            }
            return anchors;
        }
        
        public static NativeArray<Anchor> CreateAnchors(this List<Dot> dots, Allocator allocator) {
            int n = dots.Count;
            var anchors = new NativeArray<Anchor>(n, allocator);
            for (int i = 0; i < n; ++i) {
                anchors[i] = dots[i].Anchor;
            }
            return anchors;
        }
    }

}