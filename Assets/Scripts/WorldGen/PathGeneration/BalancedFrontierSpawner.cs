using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Utilities;
using Scripts.WorldGen.RoomPlacement;
using UnityEngine;
using Random = System.Random;

namespace Scripts.WorldGen.PathGeneration {
    public class BalancedFrontierSpawner : IWalkerSpawner {
        // === 1. Internal State ===
        // Cached list of active component roots (fast random access)
        private readonly IndexedSet<int> _activeComponents = new();

        // Frontier points grouped by component root
        private readonly Dictionary<int, IndexedSet<Vector2Int>> _componentFrontiers = new();

        // Cached weights for each component to support weighted random selection
        private Dictionary<int, int> _componentWeights = new();

        public IReadOnlyDictionary<Vector2Int, bool> AllVisited { get; private set; }

        public void Initialize(Dictionary<Vector2Int, bool> allVisited, UnionFind<Vector2Int> unionFind) {
            _componentFrontiers.Clear();
            _activeComponents.Clear();
            AllVisited = allVisited;
            foreach (Vector2Int tile in allVisited.Where(kv => kv.Value).Select(kv => kv.Key)) {
                int root = unionFind.Find(tile);
                if (_activeComponents.Add(root))
                    // New cluster, needs a new set
                    _componentFrontiers.Add(root, new IndexedSet<Vector2Int>());

                _componentFrontiers[root].Add(tile);
            }
        }

        // === 2. Update Logic ===
        public void CommitPath(HashSet<Vector2Int> newPath, UnionFind<Vector2Int> unionFind) {
            // For each tile in newPath:
            //   - Check each neighbor:
            //       - If neighbor is unvisited (not in allVisited), this tile is a frontier
            //   - If frontier, add it to the corresponding frontier set in _componentFrontiers
            int pathRoot = unionFind.Find(newPath.First());
            foreach (Vector2Int pos in newPath.Where(pos => pos.Neighbors().Any(n => !AllVisited.ContainsKey(n))))
                _componentFrontiers[pathRoot].Add(pos);

            // The path might have merged frontiers
            // Find frontiers that are no longer root, and merge them with root
            List<int> toRemove = new();
            foreach ((int oldRoot, IndexedSet<Vector2Int> frontier) in _componentFrontiers) {
                int newRoot = unionFind.FindByIndex(oldRoot);
                if (newRoot == oldRoot) continue;

                // Merge frontier into the new root
                if (!_componentFrontiers.ContainsKey(newRoot)) {
                    _componentFrontiers[newRoot] = new IndexedSet<Vector2Int>();
                    Debug.LogError("New root does not contain a valid frontier set.");
                }

                _componentFrontiers[newRoot].UnionWith(frontier);
                toRemove.Add(oldRoot);
            }

            foreach (int rm in toRemove) {
                _activeComponents.Remove(rm);
                _componentFrontiers.Remove(rm);
            }

            // After processing:
            //   - Recalculate weights:
            //       - Weight could be the number of frontiers or use a non-linear scale to prioritize smaller components
            //   - Refresh _activeComponents if any components gained new frontier tiles
        }

        // === 3. Spawn Point Selection ===
        public Vector2Int PickSpawnPoint(Random rng) {
            if (_activeComponents.Count == 0)
                throw new InvalidOperationException("No active components available for spawning.");

            int root = _activeComponents[rng.Next(_activeComponents.Count)];
            IndexedSet<Vector2Int> frontier = _componentFrontiers[root];

            while (frontier.Count > 0) {
                int i = rng.Next(frontier.Count);
                Vector2Int candidate = frontier[i];

                if (candidate.Neighbors().Any(n => !AllVisited.ContainsKey(n)))
                    return candidate;

                frontier.Remove(candidate); // prune invalid
            }

            _activeComponents.Remove(root); // no valid frontier left
            throw new InvalidOperationException($"Component {root} has no valid frontier left.");
        }
    }
}