using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Scripts.DataObjects {
    [CreateAssetMenu(fileName = "New House", menuName = "New House TileSet", order = 0)]
    public class HouseTiles : ScriptableObject {
        public TileBase roofL;
        public TileBase roofM;
        public TileBase roofR;
        public TileBase roofSpecial;
        
        public TileBase door;
        public TileBase wallL;
        public TileBase wallM;
        public TileBase wallR;
    }
}