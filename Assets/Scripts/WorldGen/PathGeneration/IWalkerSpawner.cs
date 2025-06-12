using System.Collections.Generic;
using Scripts.Utilities;
using UnityEngine;
using Random = System.Random;

namespace Scripts.WorldGen.PathGeneration {
    public interface IWalkerSpawner {
        /// <summary>
        ///     An externally-managed dictionary
        /// </summary>
        public IReadOnlyDictionary<Vector2Int, bool> AllVisited { get; }

        public void Initialize(
            Dictionary<Vector2Int, bool> allVisited,
            UnionFind<Vector2Int> unionFind
        );

        /// <summary>
        ///     Called after each walker finishes a path.
        ///     <br />
        ///     IMPORTANT: This should be called only after <paramref name="newPath" /> has been committed to
        ///     <see cref="AllVisited" /> and <paramref name="unionFind" />. That is, all tiles in
        ///     <paramref name="newPath" /> must already be reflected in the global visited map and the
        ///     connectivity tracker.
        ///     This allows the implementation to make decisions based on the fully updated global state,
        ///     while optionally leveraging <paramref name="newPath" /> as the most recent modification.
        /// </summary>
        /// <param name="newPath">The set of tiles visited by this path (including filled area).</param>
        /// <param name="unionFind">Connectivity tracker for walkable areas.</param>
        public void CommitPath(
            HashSet<Vector2Int> newPath,
            UnionFind<Vector2Int> unionFind);

        /// <summary>
        ///     Returns the next spawn location for a random walker.
        ///     Can throw if no valid spawn locations remain.
        /// </summary>
        public Vector2Int PickSpawnPoint(Random rng);
    }
}