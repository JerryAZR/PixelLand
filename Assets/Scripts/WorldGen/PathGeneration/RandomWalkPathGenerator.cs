using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Scripts.Utilities;
using Scripts.WorldGen.RoomPlacement;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace Scripts.WorldGen.PathGeneration {
    public class RandomWalkPathGenerator : IPathGenerator {
        private readonly UnionFind<Vector2Int> _unionFind = new();
        private Random _rng;

        public IEnumerable<Vector2Int> Connect(RoomLayout layout, int pathWidth, int rngSeed) {
            _rng = new Random(rngSeed);
            HashSet<Vector2Int> path = new();
            // Add initial sets
            foreach (RoomInfo room in layout.Rooms) {
                Vector2Int[] walkableTiles = room.Tiles.Where(kv => kv.Value).Select(kv => kv.Key).ToArray();
                if (walkableTiles.Length == 0) continue;
                Vector2Int first = walkableTiles[0];
                foreach (Vector2Int tile in walkableTiles) {
                    _unionFind.Union(first, tile);
                }
            }

            // Initialize worker
            IWalkerSpawner spawner = new BalancedFrontierSpawner();
            spawner.Initialize(layout.Tiles, _unionFind);
            RectInt pathBounds = layout.Bounds.Expand(pathWidth);
            RandomWalker walker = new(pathBounds, _rng);

            // Let it walk
            for (int i = 0; i < 1 << 16; i++) {
                Vector2Int start = spawner.PickSpawnPoint(_rng);
                walker.Reset(start, layout.Tiles);
                HashSet<Vector2Int> result = walker.Walk(pathWidth);
                // Path quality check
                if (result.Count < 32 * pathWidth) {
                    // Short paths are discarded unless they add value
                    HashSet<int> nearby = new();
                    IEnumerable<Vector2Int> validNeighbors = result
                        .SelectMany(p => p.Neighbors())
                        .Where(n => _unionFind.Contains(n));
                    foreach (Vector2Int neighbor in validNeighbors) {
                        nearby.Add(_unionFind.Find(neighbor));
                        if (nearby.Count > 1) break;
                    }

                    if (nearby.Count <= 1) continue;
                }

                // Update unionFind and layout tiles
                foreach (Vector2Int tile in result) {
                    foreach (Vector2Int neighbor in tile.Neighbors())
                        if (layout.Tiles.TryGetValue(neighbor, out bool floor)) {
                            // Union with existing walkable neighbors
                            if (floor) _unionFind.Union(tile, neighbor);
                        }

                    // Add self to global map
                    layout.Tiles[tile] = true;
                }
                // Update spawner states
                spawner.CommitPath(result, _unionFind);

                path.UnionWith(result);
                if (_unionFind.ClusterCount != 1) continue;
                Debug.Log($"Path generation completed after {i} iterations.");
                break;
            }

            if (_unionFind.ClusterCount > 1)
                Debug.LogWarning("RandomWalkPathGenerator failed to connect all walkable areas.");

            return path;
        }
    }

    public class RandomWalker {
        private readonly List<Vector2Int> _path = new();
        private readonly HashSet<Vector2Int> _pathSet = new();
        private readonly Random _rng;
        private RectInt _bounds;
        private Vector2Int _current;
        private Vector2Int _lastDirection = Vector2Int.zero;
        private ReadOnlyDictionary<Vector2Int, bool> _map;
        public int MaxSteps = 64;
        public Vector2Int Start;
        public double TerminationRate = 0.02;
        public double TurnRate = 0.125;

        /// <summary>
        /// </summary>
        /// <param name="start">Not included in the final path</param>
        /// <param name="map"></param>
        /// <param name="bounds"></param>
        /// <param name="rng"></param>
        public RandomWalker(Vector2Int start, Dictionary<Vector2Int, bool> map, RectInt bounds, Random rng) {
            Start = start;
            _current = start;
            _rng = rng;
            _map = new ReadOnlyDictionary<Vector2Int, bool>(map);
            _bounds = bounds;
        }

        public RandomWalker(RectInt bounds, Random rng) {
            _rng = rng;
            _bounds = bounds;
        }

        /// <summary>
        /// </summary>
        /// <param name="start">Not included in the final path</param>
        /// <param name="map"></param>
        public void Reset(Vector2Int start, Dictionary<Vector2Int, bool> map) {
            Start = start;
            _current = start;
            _map = new ReadOnlyDictionary<Vector2Int, bool>(map);

            _lastDirection = Vector2Int.zero;
            _path.Clear();
            _pathSet.Clear();
        }

        /// <summary>
        ///     Take a single step
        /// </summary>
        /// <returns>True if random walk can continue</returns>
        public bool Next() {
            List<Vector2Int> directions = GetShuffledDirections();
            int numTries = Mathf.Min(directions.Count, 2);
            for (int i = 0; i < numTries; i++) {
                Vector2Int next = _current + directions[i];
                if (!IsLegalMove(next)) continue;
                _current = next;
                _lastDirection = directions[i];
                _path.Add(next);
                _pathSet.Add(next);
                bool continueWalking = _rng.NextDouble() > TerminationRate && _path.Count < MaxSteps;
                return continueWalking;
            }

            Assert.IsTrue(_lastDirection.x == 0 || _lastDirection.y == 0);
            return false;
        }

        public HashSet<Vector2Int> Walk(int pathWidth = 1) {
            while (Next()) ;

            if (pathWidth <= 1) return _pathSet;

            for (int i = 0; i < _path.Count; i++) {
                Vector2Int current = _path[i];

                // Determine direction before and after this point
                Vector2Int prevDir = i > 0 ? _path[i] - _path[i - 1] : _path[0] - Start;
                Vector2Int? nextDir = i < _path.Count - 1 ? _path[i + 1] - _path[i] : null;

                // If turning (or no consistent direction), expand in a square
                bool isCorner = nextDir.HasValue && prevDir != nextDir.Value;

                if (isCorner) {
                    // Fill square around the current point
                    int radius = pathWidth / 2;
                    for (int dx = -radius; dx < pathWidth - radius; dx++)
                    for (int dy = -radius; dy < pathWidth - radius; dy++) {
                        Vector2Int pos = current + new Vector2Int(dx, dy);
                        if (!_map.ContainsKey(pos)) _pathSet.Add(pos);
                    }
                } else {
                    // Fill a strip perpendicular to the direction
                    Vector2Int perp = new(prevDir.y, -prevDir.x);
                    int half = pathWidth / 2;
                    for (int offset = -half; offset < pathWidth - half; offset++) {
                        Vector2Int pos = current + perp * offset;
                        if (!_map.ContainsKey(pos)) _pathSet.Add(pos);
                    }
                }
            }

            return _pathSet;
        }


        private List<Vector2Int> GetShuffledDirections() {
            if (_lastDirection == Vector2Int.zero) {
                // First step: pick any unvisited neighbor
                List<Vector2Int> firstStepCandidates =
                    Start.Neighbors()
                        .Where(n => !_map.ContainsKey(n))
                        .Select(n => n - Start)
                        .ToList();
                firstStepCandidates.Shuffle(_rng);
                return firstStepCandidates;
            }

            Vector2Int forward = _lastDirection;
            Vector2Int left = new(forward.y, -forward.x);
            Vector2Int right = new(-forward.y, forward.x);

            double roll = _rng.NextDouble();
            if (roll > TurnRate)
                // Forward-heavy, deterministic left/right order
                return _rng.NextDouble() < 0.5
                    ? new List<Vector2Int> { forward, left, right }
                    : new List<Vector2Int> { forward, right, left };

            // More chaotic: forward at random position
            List<Vector2Int> result = new() { left, right };
            result.Shuffle(_rng);
            result.Insert(_rng.Next(1, 3), forward); // 1 or 2
            return result;
        }

        // A move is legal if the tile was never visited before
        private bool IsLegalMove(Vector2Int pos) {
            return !_pathSet.Contains(pos) && !_map.ContainsKey(pos) && _bounds.Contains(pos);
        }
    }
}