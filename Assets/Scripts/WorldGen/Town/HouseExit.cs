using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scripts.Controls;
using Scripts.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace Scripts.WorldGen.Town {
    public class HouseExit : NetworkBehaviour {
        private static readonly SceneIdentifier _mainWorld = new("MainWorld");
        private readonly SyncVar<Vector3> _exitPosition = new();

        private Vector3 ExitPosition {
            get => _exitPosition.Value;
            [Server] set => _exitPosition.Value = value;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent(out PlayerControl player) && player.IsOwner) {
                var slc = player.GetComponentInChildren<SceneLoadControl>();
                slc.MoveToScene(_mainWorld);
                // Move player position
                player.transform.position = ExitPosition;
            } else {
                Debug.LogWarning($"Scene transition through HouseEntrance not implemented for {other.gameObject.name}");
            }
        }

        public override void OnStartServer() {
            StartCoroutine(ServerInit());
        }

        private IEnumerator ServerInit() {
            yield return new WaitUntil(() => SceneLoader.Instance.TryGetSceneIdentifier(gameObject.scene, out _));
            Assert.IsTrue(SceneLoader.Instance.TryGetSceneIdentifier(gameObject.scene, out SceneIdentifier identifier));
            Assert.IsTrue(CrossSceneDataManager.TryGetData(identifier, out HouseSceneData data));
            ExitPosition = data.EntrancePos + Vector3.down;
        }
    }
}