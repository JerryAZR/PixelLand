using System.Linq;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.WorldGen {
    /// <summary>
    /// Finds all networked scene objects and spawns them
    /// </summary>
    public class NetworkSpawner : MonoBehaviour {
        [SerializeField] private NetworkObject[] sceneRoots;

        private void Awake() {
            if (!InstanceFinder.IsServerStarted) return;
            
            // Ensure this object isn't a NetworkObject.
            if (TryGetComponent(out NetworkObject nob)) {
                Debug.LogError("NetworkSpawner should not have a NetworkObject component attached.");
                Destroy(nob);
            }

            RegisterSceneNetworkObjects();
        }

        private void RegisterSceneNetworkObjects() {
            // Find all currently disabled network objects and spawn them
            var sceneObjs = gameObject.scene.GetRootGameObjects()
                .SelectMany(obj => obj.GetComponents<NetworkObject>())
                .Where(nob => !nob.gameObject.activeSelf);

            foreach (NetworkObject nob in sceneObjs) InstanceFinder.ServerManager.Spawn(nob, scene: gameObject.scene);
            // Also spawn network prefabs
            foreach (NetworkObject prefab in sceneRoots) {
                NetworkObject nob = Instantiate(prefab);
                InstanceFinder.ServerManager.Spawn(nob, scene: gameObject.scene);
            }
        }
    }
}