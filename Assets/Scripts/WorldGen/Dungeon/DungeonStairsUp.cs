using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scripts.Controls;
using Scripts.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace Scripts.WorldGen.Dungeon {
    public class DungeonStairsUp : NetworkBehaviour {
        private static readonly SceneIdentifier _mainWorld = new("MainWorld");
        private readonly SyncVar<SceneIdentifier> _nextScene = new();

        private SceneIdentifier NextScene {
            get => _nextScene.Value;
            [Server] set => _nextScene.Value = value;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent(out PlayerControl player) && player.IsOwner) {
                var slc = player.GetComponentInChildren<SceneLoadControl>();
                slc.MoveToScene(NextScene);
                // Move player position
                player.transform.position = Vector3.zero;
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
            NextScene = identifier.id == 0 ? _mainWorld : new SceneIdentifier("DungeonLevel", identifier.id - 1);
            ;
        }
    }
}