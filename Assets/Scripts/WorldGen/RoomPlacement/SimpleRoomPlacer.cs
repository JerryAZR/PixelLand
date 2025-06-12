using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Scripts.WorldGen.RoomPlacement {
    public class SimpleRoomPlacer : IRoomPlacer {
        private Random _rng = new(0);

        public int MaxRetires = 20;

        public RoomLayout PlaceRooms(RoomPlacementConfig config, Transform parent) {
            _rng = new Random(config.RngSeed);
            RoomPlacementState state = new();
            // 1. Place all required rooms
            foreach (PlaceableRoomBase prefab in config.RequiredRooms.Where(r => r.Constraints.ForcePosition)) {
                Vector2Int pos = prefab.Constraints.Position;
                bool success = state.TryAddRoom(prefab, pos, parent, config.Spacing, _rng.Next());
                if (!success) Debug.LogError($"Failed to place required room at forced position {pos}: {prefab.name}");
            }

            foreach (PlaceableRoomBase prefab in config.RequiredRooms.Where(r => !r.Constraints.ForcePosition)) {
                bool success = TryPlaceRequiredRoom(state, prefab, config.SoftBounds, config.Spacing, parent);
                if (!success)
                    Debug.LogError($"Failed to place required room after {MaxRetires} expansions: {prefab.name}");
            }

            // Fill remaining space with filler rooms
            int fillersPlaced = PlaceFillerRooms(state, config, parent);
            Debug.Log($"Placed {fillersPlaced} filler rooms");

            return state.Layout;
        }

        private bool TryPlaceRequiredRoom(
            RoomPlacementState state,
            PlaceableRoomBase prefab,
            RectInt softBounds,
            int spacing,
            Transform parent
        ) {
            RectInt currentInner = new(softBounds.position, Vector2Int.zero);
            RectInt currentOuter = softBounds;

            for (int expansion = 0; expansion <= MaxRetires; expansion++) {
                for (int attempt = 0; attempt < MaxRetires; attempt++) {
                    Vector2Int candidateCenter =
                        RoomPlacementHelpers.SamplePointInRing(currentInner, currentOuter, _rng);
                    if (state.TryAddRoom(prefab, candidateCenter, parent, spacing, _rng.Next())) return true;
                }

                // Expand outer bounds outward in all directions
                currentInner = currentOuter;
                currentOuter = currentOuter.Expand(2);
            }

            return false;
        }

        private int PlaceFillerRooms(
            RoomPlacementState state,
            RoomPlacementConfig config,
            Transform parent
        ) {
            int totalPlaced = 0;
            for (int attempt = 0; attempt < MaxRetires; attempt++) {
                // 1. Randomly pick a filler room
                PlaceableRoomBase prefab = config.Fillers[_rng.Next(config.Fillers.Count)];

                // 2. Randomly pick a position in soft bounds
                Vector2Int candidateCenter = config.SoftBounds.SampleInside(_rng);

                // 3. Try placing the room
                bool success = state.TryAddRoom(prefab, candidateCenter, parent, config.Spacing, _rng.Next());

                if (success) {
                    // Optional: Reset attempt counter to allow more total filler rooms
                    totalPlaced++;
                    attempt = 0;
                }
            }

            return totalPlaced;
        }

        private static Vector2Int SamplePointInRect(RectInt rect, Random rng) {
            int x = rng.Next(rect.xMin, rect.xMax);
            int y = rng.Next(rect.yMin, rect.yMax);
            return new Vector2Int(x, y);
        }
    }
}