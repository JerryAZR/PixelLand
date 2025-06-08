using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.WorldGen {
    public class TownGroundGenerator : MonoBehaviour, IMapGenerator {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase grassTile;

        public void Generate(MapData mapData) {
            mapData.Terrain = new Dictionary<Vector3Int, TerrainType>();
            foreach (Vector3Int cell in mapData.MapBounds.allPositionsWithin) {
                mapData.Terrain[cell] = TerrainType.Grass;
            }
        }

        public void Populate(MapData mapData) {
            foreach ((Vector3Int cell, TerrainType type) in mapData.Terrain) {
                switch (type) {
                    case TerrainType.Grass:
                        tilemap.SetTile(cell, grassTile);
                        break;
                    case TerrainType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}