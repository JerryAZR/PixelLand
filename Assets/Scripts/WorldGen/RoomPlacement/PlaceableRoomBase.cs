using System.Collections.Generic;
using UnityEngine;

namespace Scripts.WorldGen.RoomPlacement {
    public abstract class PlaceableRoomBase : MonoBehaviour {
        public abstract RoomConstraints Constraints { get; }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos() {
            if (Constraints == null) return;
            RectInt bounds = Constraints.Bounds;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube((Vector3)bounds.center + transform.position,
                new Vector3(bounds.size.x + 0.25f, bounds.size.y + 0.25f, 0));
        }
#endif
        public abstract RoomInfo BuildAt(Vector2Int origin, Transform parent, int rngSeed);

        protected bool EnsurePrefab(string action = "") {
            if (!gameObject.scene.IsValid() || gameObject.scene.name == null) return true;
            Debug.LogError($"{name}: {action} called on scene instance.");
            return false;
        }

        protected PlaceableRoomBase InstantiatePrefab(Vector2Int center, Transform parent) {
            PlaceableRoomBase instance = Instantiate(this, parent);
            instance.transform.position = new Vector3(center.x, center.y, 0);
            return instance;
        }
    }

    public enum RoomType {
        None,

        // -------- Town Section --------
        NpcHome,
        PlayerHome,
        DungeonGate,

        // -------- Dungeon Section --------
        /// <summary>
        ///     Contains pathways to the previous and next level
        /// </summary>
        EntryExit,

        /// <summary>
        ///     Spawns regular monsters
        /// </summary>
        Monster
    }

    /// <summary>
    ///     Constraints on how the room should be placed
    /// </summary>
    public class RoomConstraints {
        public RectInt Bounds;

        public bool ForcePosition;
        public Vector2Int Position;
        public RoomType Type;
    }

    /// <summary>
    ///     Room placement decision and relevant data
    /// </summary>
    public class RoomInfo {
        public RectInt Bounds;

        /// <summary>
        ///     Contains walkable tiles as true and walls as false
        /// </summary>
        public Dictionary<Vector2Int, bool> Tiles = new();
    }
}