using FishNet.Object;
using UnityEngine;

namespace Scripts.Systems {
    public class GameRoot : MonoBehaviour
    {
        private void Awake()
        {
            if (TryGetComponent<NetworkObject>(out _)) {
                Debug.LogError("NetworkObject is not allowed on GameRoot");
            }
            DontDestroyOnLoad(gameObject);
        }
    }
}


