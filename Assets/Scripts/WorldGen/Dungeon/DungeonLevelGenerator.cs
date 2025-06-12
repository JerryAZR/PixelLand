using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scripts.WorldGen.PathGeneration;
using Scripts.WorldGen.RoomPlacement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Scripts.WorldGen.Dungeon {
    public class DungeonLevelGenerator : NetworkBehaviour {
        [SerializeField] private RoomPlacementAlgorithm roomPlacementAlgorithm;
        [SerializeField] private PathGenerationAlgorithm pathGenerationAlgorithm;
        [SerializeField] private List<PlaceableRoomBase> requiredRooms;
        [SerializeField] private List<PlaceableRoomBase> fillerRooms;
        [SerializeField] private TileBase defaultWall;
        [SerializeField] private TileBase defaultFloor;
        [SerializeField] private Tilemap wallMap;
        [SerializeField] private Tilemap floorMap;
        [SerializeField] private Vector2Int levelSize;
        [SerializeField] [Range(1, 10)] private int roomSpacing;
        private readonly SyncVar<int> _rngSeed = new();

        private int RngSeed {
            get => _rngSeed.Value;
            [Server] set => _rngSeed.Value = value;
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            if (IsServerStarted) RngSeed = Random.Range(int.MinValue, int.MaxValue);
            Vector2Int boundsPos = levelSize / -2;
            RoomPlacementConfig config = new() {
                Fillers = fillerRooms,
                RequiredRooms = requiredRooms,
                SoftBounds = new RectInt(boundsPos, levelSize),
                Spacing = roomSpacing,
                RngSeed = RngSeed
            };
            IRoomPlacer roomPlacer = GetRoomPlacer(roomPlacementAlgorithm);
            RoomLayout layout = roomPlacer.PlaceRooms(config, transform);
            IPathGenerator pathGen = GetPathGenerator(pathGenerationAlgorithm);
            pathGen.Connect(layout, 1, RngSeed + 1);
            // Fill default tiles
            RectInt wallBounds = new();
            wallBounds.SetMinMax(
                layout.Bounds.min - new Vector2Int(5, 5),
                layout.Bounds.max + new Vector2Int(5, 5)
                );
            foreach (Vector2Int pos in wallBounds.allPositionsWithin) {
                if (layout.Tiles.GetValueOrDefault(pos, false)) {
                    floorMap.SetTile((Vector3Int)pos, defaultFloor);
                } else {
                    wallMap.SetTile((Vector3Int)pos, defaultWall);
                }
            }
        }

        private static IRoomPlacer GetRoomPlacer(RoomPlacementAlgorithm algo) {
            return algo switch {
                RoomPlacementAlgorithm.Simple => new SimpleRoomPlacer(),
                _ => throw new NotImplementedException($"Placement algorithm {algo} not implemented")
            };
        }

        private static IPathGenerator GetPathGenerator(PathGenerationAlgorithm algo) {
            return algo switch {
                PathGenerationAlgorithm.RandomWalk => new RandomWalkPathGenerator(),
                _ => throw new NotImplementedException($"Path algorithm {algo} not implemented")
            };
        }
    }

    public enum RoomPlacementAlgorithm {
        Simple
    }

    public enum PathGenerationAlgorithm {
        RandomWalk
    }
}