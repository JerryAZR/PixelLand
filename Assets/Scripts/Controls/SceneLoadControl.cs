using System.Threading;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using Scripts.Systems;
using UnityEngine;

namespace Scripts.Controls {
    public class SceneLoadControl : NetworkBehaviour {
        private const float SceneMoveCooldown = 1.0f; // seconds

        private float _lastSceneMoveTime = -Mathf.Infinity;
        private NetworkObject _playerObject;

        public override void OnStartClient() {
            base.OnStartClient();

            _playerObject = GetComponentInParent<NetworkTransform>().NetworkObject;

            if (!IsOwner) return;

            LoadScene(new SceneIdentifier("MainWorld"), new[] { _playerObject });
        }

        public void MoveToScene(SceneIdentifier scene) {
            if (Time.time - _lastSceneMoveTime < SceneMoveCooldown) {
                Debug.Log("MoveToScene ignored due to cooldown.");
                return;
            }

            _lastSceneMoveTime = Time.time;
            Debug.Log($"Moving player to {scene}");
            LoadScene(scene, new[] { _playerObject });
        }

        [ServerRpc]
        private void LoadScene(SceneIdentifier scene, NetworkObject[] moves, NetworkConnection conn = null) {
            _ = SceneLoader.Instance.ServerLoadScene(scene, moves, conn!);
        }
    }
}
