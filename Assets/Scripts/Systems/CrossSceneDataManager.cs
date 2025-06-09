using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Scripts.Systems
{
    public class CrossSceneDataManager : MonoBehaviour
    {
        public static CrossSceneDataManager Instance { get; private set; }
        public static Dictionary<SceneIdentifier, object> SceneData { get; } = new();
        private void Awake() {
            if (Instance && Instance != this) {
                Debug.LogError("Multiple SceneLoaders found!");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public static bool TryGetData<T>(SceneIdentifier key, out T data) {
            if (SceneData.TryGetValue(key, out object raw)) {
                if (raw is T t) {
                    data = t;
                    return true;
                }
            }
            data = default(T);
            return false;
        }

        public static void SetData(SceneIdentifier key, object data) {
            SceneData[key] = data;
        }
    }
}
