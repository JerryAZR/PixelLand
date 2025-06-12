using System;
using System.Collections.Generic;
using Scripts.WorldGen.Dungeon;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.WorldGen.RoomPlacement {
    public interface IRoomPlacer {
        public RoomLayout PlaceRooms(RoomPlacementConfig config, Transform parent);
    }

    /// <summary>
    ///     Current implementation is just a prototype.
    ///     In the future we may add more fields for better control.
    /// </summary>
    public class RoomPlacementConfig {
        /// <summary>
        ///     These rooms should be used to fill empty space
        /// </summary>
        public IList<PlaceableRoomBase> Fillers;
        
        /// <summary>
        ///     These rooms must be present and unique
        /// </summary>
        public IEnumerable<PlaceableRoomBase> RequiredRooms;

        /// <summary>
        ///     Constraint on room centers. Not enforced for every tile
        /// </summary>
        public RectInt SoftBounds;

        /// <summary>
        ///     Minimum spacing between rooms
        /// </summary>
        public int Spacing;

        public int RngSeed { get; set; }
    }

    public class RoomLayout {
        public RectInt Bounds;
        public List<RoomInfo> Rooms;

        /// <summary>
        ///     Blocking: false; Non-blocking: true
        /// </summary>
        public Dictionary<Vector2Int, bool> Tiles;
    }


    public enum CellUsage {
        None,

        /// <summary>
        ///     Occupied tiles cannot overlap with any
        /// </summary>
        Occupied,

        /// <summary>
        ///     Spacing tiles can overlap
        /// </summary>
        Spacing
    }

    public static class CellUsageExtensions {
        public static bool IsCompatible(this CellUsage usage, CellUsage other) {
            return usage switch {
                CellUsage.None => true,
                CellUsage.Occupied => other == CellUsage.None,
                CellUsage.Spacing => other != CellUsage.Occupied,
                _ => throw new ArgumentException($"{usage} is not a valid cell usage")
            };
        }
    }

    public class RoomPlacementState {
        public Vector2Int BoundsMax = new(int.MinValue, int.MinValue);
        public Vector2Int BoundsMin = new(int.MaxValue, int.MaxValue);
        public List<RoomInfo> Rooms = new();
        public Dictionary<Vector2Int, bool> Tiles = new();
        public Dictionary<Vector2Int, CellUsage> UsageTracker = new();

        public RoomLayout Layout {
            get {
                RectInt mapBounds = new();
                mapBounds.SetMinMax(BoundsMin, BoundsMax);
                return new RoomLayout {
                    Bounds = mapBounds,
                    Rooms = Rooms,
                    Tiles = Tiles
                };
            }
        }

        public bool TryAddRoom(PlaceableRoomBase prefab, Vector2Int position, Transform parent, int spacing,
            int rngSeed) {
            RoomConstraints constraints = prefab.Constraints;
            // If position is enforced, check that first
            if (constraints.ForcePosition && constraints.Position != position) return false;
            // Offset bounds with position
            RectInt bounds = new(
                constraints.Bounds.xMin + position.x,
                constraints.Bounds.yMin + position.y,
                constraints.Bounds.width,
                constraints.Bounds.height
            );
            // Collision check
            if (CheckCollision(bounds, spacing)) return false;
            // Place room
            RoomInfo placed = prefab.BuildAt(position, parent, rngSeed);
            AddRoom(placed, spacing);
            return true;
        }

        /// <summary>
        ///     Check if the given rect collides with any existing tiles
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        private bool CheckCollision(RectInt bounds, int spacing) {
            RectInt expanded = bounds.Expand(spacing);
            foreach (Vector2Int pos in expanded.allPositionsWithin) {
                CellUsage proposedUsage = bounds.Contains(pos) ? CellUsage.Occupied : CellUsage.Spacing;
                if (!UsageTracker.TryGetValue(pos, out CellUsage existingUsage)) continue;
                if (!existingUsage.IsCompatible(proposedUsage)) return true; // conflict
            }

            return false;
        }

        /// <summary>
        ///     Add a room and update states accordingly
        /// </summary>
        /// <param name="room"></param>
        /// <param name="spacing"></param>
        private void AddRoom(RoomInfo room, int spacing) {
            Rooms.Add(room);
            // Mark all cells within room bounds as occupied
            // Mark all cells within room bounds + spacing as spacing
            RectInt expanded = room.Bounds.Expand(spacing);
            foreach (Vector2Int pos in expanded.allPositionsWithin) {
                CellUsage usage = room.Bounds.Contains(pos) ? CellUsage.Occupied : CellUsage.Spacing;
                UsageTracker[pos] = usage;
            }

            // Add floors and walls
            Tiles.AddRange(room.Tiles);

            // Update global bounds
            BoundsMin = Vector2Int.Min(BoundsMin, room.Bounds.min);
            BoundsMax = Vector2Int.Max(BoundsMax, room.Bounds.max);
        }
    }
}