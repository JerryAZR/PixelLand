using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.WorldGen.RoomPlacement {
    public class StaticPlaceableRoom : PlaceableRoomBase {
        [Tooltip("Tilemap that contains the walkable tiles")]
        public Tilemap walkableTilemap;

        [Tooltip("Tilemap that contains the non-walkable tiles")]
        public Tilemap blockerTilemap;

        [SerializeField] [BoxGroup("Constraints")]
        private RoomType roomType;

        [SerializeField] [BoxGroup("Constraints")]
        private bool forcePosition;

        [SerializeField] [BoxGroup("Constraints")] [ShowIf(nameof(forcePosition))]
        private Vector2Int position;

        [SerializeField] [BoxGroup("Constraints")]
        private Tilemap sizeReference;
        
        public RoomConstraints CachedConstraints {get; private set;}

        public override RoomConstraints Constraints {
            get {
                if (!sizeReference) return null;

                sizeReference.CompressBounds();
                RectInt bounds = new(
                    sizeReference.cellBounds.xMin,
                    sizeReference.cellBounds.yMin,
                    sizeReference.cellBounds.xMax - sizeReference.cellBounds.xMin,
                    sizeReference.cellBounds.yMax - sizeReference.cellBounds.yMin
                );
                return new RoomConstraints {
                    Type = roomType,
                    ForcePosition = forcePosition,
                    Position = position,
                    Bounds = bounds
                };
            }
        }

        public override RoomInfo BuildAt(Vector2Int origin, Transform parent, int rngSeed) {
            EnsurePrefab();
            var instance = (StaticPlaceableRoom)InstantiatePrefab(origin, parent);
            instance.CachedConstraints = Constraints;
            RoomInfo roomInfo = new() {
                Bounds = new RectInt(
                    Constraints.Bounds.xMin + origin.x,
                    Constraints.Bounds.yMin + origin.y,
                    Constraints.Bounds.width,
                    Constraints.Bounds.height
                ),
                Tiles = new Dictionary<Vector2Int, bool>()
            };

            instance.walkableTilemap.CompressBounds();
            foreach (Vector3Int localPos in instance.walkableTilemap.cellBounds.allPositionsWithin)
                if (instance.walkableTilemap.GetTile(localPos))
                    roomInfo.Tiles[(Vector2Int)localPos + origin] = true;

            instance.blockerTilemap.CompressBounds();
            foreach (Vector3Int localPos in instance.blockerTilemap.cellBounds.allPositionsWithin)
                if (instance.blockerTilemap.GetTile(localPos))
                    roomInfo.Tiles[(Vector2Int)localPos + origin] = false;

            return roomInfo;
        }
    }
}