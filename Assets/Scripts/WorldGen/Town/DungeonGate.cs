using Scripts.Controls;
using Scripts.Systems;
using UnityEngine;

namespace Scripts.WorldGen.Town {
    public class DungeonGate : MonoBehaviour {
        private static readonly SceneIdentifier _dungeonIdentifier = new("DungeonLevel");

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.TryGetComponent(out PlayerControl player) || !player.IsOwner) return;

            player.GetComponentInChildren<SceneLoadControl>().MoveToScene(_dungeonIdentifier);
            player.transform.position = Vector3.zero;
        }
    }
}