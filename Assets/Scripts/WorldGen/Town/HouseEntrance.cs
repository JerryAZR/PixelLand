using Scripts.Controls;
using Scripts.Systems;
using UnityEngine;

namespace Scripts.WorldGen.Town {
    public class HouseEntrance : MonoBehaviour {
        private HouseSceneData _sceneData;
        private SceneIdentifier _sceneToLoad;

        private void Start() {
            // var tilemap = GetComponentInParent<Tilemap>();
            // Assert.IsNotNull(tilemap);
            var coords = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            _sceneToLoad = new SceneIdentifier("HouseInterior", IdFromCoords(coords));
            _sceneData = new HouseSceneData { EntrancePos = transform.position };

            CrossSceneDataManager.SetData(_sceneToLoad, _sceneData);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent(out PlayerControl player) && player.IsOwner) {
                var slc = player.GetComponentInChildren<SceneLoadControl>();
                slc.MoveToScene(_sceneToLoad);
                player.transform.position = new Vector3(0.5f, 0.5f, 0);
            } else {
                Debug.LogWarning($"Scene transition through HouseEntrance not implemented for {other.gameObject.name}");
            }
        }

        public static int IdFromCoords(Vector3Int coords) {
            return (coords.y << 16) | coords.x;
        }
    }

    public class HouseSceneData {
        public Vector3 EntrancePos;
    }
}