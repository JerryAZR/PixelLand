using UnityEngine;

namespace Scripts.WorldGen {
    public enum TerrainType {
        None,
        Grass,
    }

    public enum StructureType {
        None,
        House
    }

    public struct House {
        public BoundsInt Bounds;
        public int DoorX;
    }
}