using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.WorldGen.RoomPlacement {
    public static class RoomPlacementHelpers {
        /// <summary>
        /// Add a ring to the original spacing
        /// </summary>
        /// <param name="original"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        public static RectInt Expand(this RectInt original, int spacing) {
            return new RectInt(
                original.xMin - spacing,
                original.yMin - spacing,
                original.width + 2 * spacing,
                original.height + 2 * spacing
            );
        }

        public static Vector2Int SampleInside(this RectInt original, System.Random rng) {
            return new Vector2Int(
                rng.Next(original.xMin, original.xMax),
                rng.Next(original.yMin, original.yMax)
            );
        }
        
        public static Vector2Int SamplePointInRing(RectInt inner, RectInt outer, System.Random rng) {
            List<Vector2Int> ringPoints = new();
            foreach (Vector2Int pos in outer.allPositionsWithin)
                if (!inner.Contains(pos))
                    ringPoints.Add(pos);

            if (ringPoints.Count == 0)
                throw new InvalidOperationException("No valid ring points to sample");

            return ringPoints[rng.Next(ringPoints.Count)];
        }

        public static IEnumerable<Vector2Int> Neighbors(this Vector2Int center) {
            yield return center + Vector2Int.right;
            yield return center + Vector2Int.left;
            yield return center + Vector2Int.up;
            yield return center + Vector2Int.down;
        }
    }
}