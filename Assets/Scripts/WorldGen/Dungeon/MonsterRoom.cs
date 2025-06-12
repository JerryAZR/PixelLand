using System.Collections.Generic;
using Scripts.WorldGen.RoomPlacement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Scripts.WorldGen.Dungeon {
    public class MonsterRoom : PlaceableRoomBase {
        [SerializeField] private Vector2Int size;

        public override RoomConstraints Constraints => new() {
            Type = RoomType.Monster,
            Bounds = new RectInt(Vector2Int.zero, size),
            ForcePosition = false
        };

        public override RoomInfo BuildAt(Vector2Int origin, Transform parent, int rngSeed) {
            EnsurePrefab();
            System.Random rng = new(rngSeed);
            RoomConstraints constraints = Constraints; // Cache constraints
            var instance = (MonsterRoom)InstantiatePrefab(origin, parent);

            RoomInfo roomInfo = new() {
                Bounds = new RectInt(origin, size),
                Tiles = new Dictionary<Vector2Int, bool>()
            };

            // For now, we simply mark all cells within as walkable
            // Later we want to generate fun, natural-looking rooms
            RectInt inside = constraints.Bounds.Expand(-1);
            foreach (Vector2Int localPos in inside.allPositionsWithin) {
                roomInfo.Tiles[localPos + origin] = true;
            }
            
            for (int i = 0; i < size.x + size.y; i++) {
                Vector2Int wallPos = RoomPlacementHelpers.SamplePointInRing(inside, constraints.Bounds, rng);
                roomInfo.Tiles[wallPos + origin] = false;
            }

            return roomInfo;
        }
    }
}