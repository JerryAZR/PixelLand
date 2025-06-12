using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Scripts.Systems {
    public class SceneLoader : MonoBehaviour {
        [FormerlySerializedAs("_scenePhysics")] [SerializeField]
        private LocalPhysicsMode scenePhysics;

        private readonly Dictionary<SceneIdentifier, int> _sceneHandles = new();
        private readonly Dictionary<int, SceneIdentifier> _sceneIdentifiers = new();
        private readonly ConcurrentDictionary<SceneIdentifier, SemaphoreSlim> _sceneLocks = new();
        public static SceneLoader Instance { get; private set; }
        
        private void Awake() {
            if (Instance && Instance != this) {
                Debug.LogError("Multiple SceneLoaders found!");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void Start() {
            InstanceFinder.SceneManager.OnLoadEnd += SceneLoadEndHandler;
            SceneManager.sceneUnloaded += SceneUnloadStartHandler;
        }

        public void OnDestroy() {
            // InstanceFinder.SceneManager.OnLoadEnd -= SceneLoadEndHandler;
            // UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= SceneUnloadStartHandler;
        }

        public bool TryGetSceneIdentifier(Scene scene, out SceneIdentifier identifier) {
            return _sceneIdentifiers.TryGetValue(scene.handle, out identifier);
        }

        public async Task ServerLoadScene(SceneIdentifier scene, NetworkObject[] moves = null, NetworkConnection conn = null) {
            Debug.Log($"[Server] Loading scene {scene} for {conn!.ClientId}");
            SemaphoreSlim sceneLock = _sceneLocks.GetOrAdd(scene, new SemaphoreSlim(1, 1));
            await sceneLock.WaitAsync(); // Acquire lock

            int handle = _sceneHandles.GetValueOrDefault(scene, 0);
            Debug.Log($"Using scene handle {handle} for {scene}");
            SceneLoadData data = new(handle, scene.name) {
                ReplaceScenes = ReplaceOption.All,
                MovedNetworkObjects = moves,
                Options = new LoadOptions {
                    AutomaticallyUnload = true, // Avoid scene unloading problems
                    AllowStacking = true,
                    LocalPhysics = scenePhysics
                },
                Params = new LoadParams {
                    ServerParams = new object[] { scene }
                }
            };

            InstanceFinder.SceneManager.LoadConnectionScenes(conn, data);
        }

        private void SceneLoadEndHandler(SceneLoadEndEventArgs args) {
            LoadParams param = args.QueueData.SceneLoadData.Params;
            if (param.ServerParams.Length > 0 && param.ServerParams[0] is SceneIdentifier identifier) {
                _sceneLocks[identifier].Release();
                if (args.LoadedScenes.Length > 0) {
                    _sceneHandles[identifier] = args.LoadedScenes[0].handle;
                    _sceneIdentifiers[args.LoadedScenes[0].handle] = identifier;
                }
            }

            foreach (Scene scene in args.LoadedScenes) {
                PhysicsSimulator.Instance.RegisterScene(scene.GetPhysicsScene2D());
                Debug.Log($"Register physics scene {scene}");
            }
        }

        private void SceneUnloadStartHandler(Scene scene) {
            // Remove from scene handle
            if (_sceneIdentifiers.Remove(scene.handle, out SceneIdentifier sid)) _sceneHandles.Remove(sid);
            // Stop physics simulation of the scene
            PhysicsSimulator.Instance.UnregisterScene(scene.GetPhysicsScene2D());
        }
    }


    [Serializable]
    public struct SceneIdentifier : IEquatable<SceneIdentifier> {
        public string name;
        public int id; // e.g., dungeon level, house interior ID, etc.

        public SceneIdentifier(string sceneName, int instanceId = 0) {
            name = sceneName;
            id = instanceId;
        }

        public readonly override string ToString() {
            return $"{name}_{id:x8}";
        }

        public readonly bool Equals(SceneIdentifier other) {
            return name == other.name && id == other.id;
        }

        public readonly override bool Equals(object obj) {
            return obj is SceneIdentifier other && Equals(other);
        }

        public readonly override int GetHashCode() {
            return HashCode.Combine(name, id);
        }

        public static bool operator ==(SceneIdentifier left, SceneIdentifier right) {
            return left.Equals(right);
        }

        public static bool operator !=(SceneIdentifier left, SceneIdentifier right) {
            return !left.Equals(right);
        }
    }
}