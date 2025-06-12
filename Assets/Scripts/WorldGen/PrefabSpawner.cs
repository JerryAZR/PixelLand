using System;
using System.Linq;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.WorldGen {
    /// <summary>
    /// Finds all networked scene objects and spawns them
    /// </summary>
    public class PrefabSpawner : MonoBehaviour {
        [SerializeField] private GameObject[] sceneRoots;

        private void Start() {
            if (!InstanceFinder.IsServerStarted) return;
            
            // Ensure this object isn't a NetworkObject.
            if (TryGetComponent(out NetworkObject nob)) {
                Debug.LogError("PrefabSpawner should not have a NetworkObject component attached.");
                Destroy(nob);
            }

            foreach (GameObject prefab in sceneRoots) {
                GameObject go = Instantiate(prefab);
                go.transform.position = transform.position;
                if (!go.TryGetComponent<NetworkObject>(out _)) continue;
                try {
                    InstanceFinder.ServerManager.Spawn(go, scene: gameObject.scene);
                } catch (Exception ex) {
                    Debug.Log($"{name}: Exception while spawning {go} for {gameObject.scene}.");
                    Debug.LogException(ex, prefab);
                }
            }
            
            Destroy(gameObject);
        }
    }
}