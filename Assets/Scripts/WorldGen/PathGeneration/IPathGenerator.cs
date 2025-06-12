using System.Collections.Generic;
using Scripts.WorldGen.RoomPlacement;
using UnityEngine;

namespace Scripts.WorldGen.PathGeneration {
    public interface IPathGenerator {
        /// <summary>
        ///     Connects all disconnected walkable components in the layout using corridors.
        /// </summary>
        /// <param name="layout">The existing room layout with disconnected regions</param>
        /// <param name="pathWidth">Preferred corridor width (best-effort only)</param>
        /// <param name="rngSeed">Seed for deterministic randomness</param>
        /// <returns>Path</returns>
        public IEnumerable<Vector2Int> Connect(RoomLayout layout, int pathWidth, int rngSeed);
    }
}