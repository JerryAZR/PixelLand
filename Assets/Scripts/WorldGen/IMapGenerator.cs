using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.WorldGen {
    public interface IMapGenerator {
        public void Generate(MapData mapData);
        public void Populate(MapData mapData);
    }

    public class MapData {
        public BoundsInt MapBounds;
        public Dictionary<Vector3Int, TerrainType> Terrain;
        public Dictionary<Vector3Int, StructureType> Structures;
        public List<House> Houses;
    }

    public enum MapLayer {
    }
}