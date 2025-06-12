using System.Collections.Generic;
using FishNet;
using Scripts.Controls;
using Scripts.DataObjects;
using Scripts.Systems;
using Scripts.WorldGen.Town;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.WorldGen {
    public class TownHouseGenerator : MonoBehaviour, IMapGenerator {
        [SerializeField] private HouseTiles[] houseTiles;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileTriggerHandler triggerHandler;

        public void Generate(MapData mapData) {
            BoundsInt bounds1 = new();
            bounds1.SetMinMax(new Vector3Int(3, 3, 0), new Vector3Int(6, 6, 1));
            House house1 = new() {
                Bounds = bounds1,
                DoorX = 4
            };

            BoundsInt bounds2 = new();
            bounds2.SetMinMax(new Vector3Int(-6, 0, 0), new Vector3Int(-1, 4, 1));
            House house2 = new() {
                Bounds = bounds2,
                DoorX = -4
            };

            mapData.Houses = new List<House> { house1, house2 };
        }

        public void Populate(MapData mapData) {
            foreach (House house in mapData.Houses) BuildHouse(house, 0);
        }

        private void BuildHouse(House house, int style) {
            HouseTiles tileset = houseTiles[style];
            // Build roof
            int roofY = house.Bounds.yMax - 2;
            tilemap.SetTile(new Vector3Int(house.Bounds.xMin, roofY), tileset.roofL);
            tilemap.SetTile(new Vector3Int(house.Bounds.xMax - 1, roofY), tileset.roofR);
            tilemap.SetTile(new Vector3Int(house.DoorX, roofY), tileset.roofSpecial);
            for (int x = house.Bounds.xMin + 1; x < house.Bounds.xMax - 1; x++) {
                if (x == house.DoorX) continue;
                tilemap.SetTile(new Vector3Int(x, roofY), tileset.roofM);
            }

            // Build floors
            for (int y = house.Bounds.yMin; y < house.Bounds.yMax - 2; y++) {
                tilemap.SetTile(new Vector3Int(house.Bounds.xMin, y), tileset.wallL);
                tilemap.SetTile(new Vector3Int(house.Bounds.xMax - 1, y), tileset.wallR);
                for (int x = house.Bounds.xMin + 1; x < house.Bounds.xMax - 1; x++)
                    tilemap.SetTile(new Vector3Int(x, y), tileset.wallM);
            }

            tilemap.SetTile(new Vector3Int(house.DoorX, house.Bounds.yMin), tileset.door);
            triggerHandler.SetAction(new Vector3Int(house.DoorX, house.Bounds.yMin), EnterHouse);

            if (!InstanceFinder.IsServerStarted) return;
            // Preload house interior
            int sceneId = HouseEntrance.IdFromCoords(new Vector3Int(house.DoorX, house.Bounds.yMin));
            SceneIdentifier identifier = new("HouseInterior", sceneId);
            _ = SceneLoader.Instance.ServerLoadScene(identifier);
        }

        private static void EnterHouse(Vector3Int doorPos, Collider2D collider) {
            if (collider.TryGetComponent(out PlayerControl player) && player.IsOwner) {
                // TODO: Load house interior and move player
                uint houseId = (uint)((doorPos.y << 16) | doorPos.x);
                Debug.Log($"Entering house {houseId:X} {doorPos}");
            } else {
                Debug.Log($"Unimplemented collision trigger handler for {collider.gameObject.name}");
            }
        }
    }
}