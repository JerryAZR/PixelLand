using FishNet;
using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;

namespace Scripts.WorldGen {
    public class DummySpawner : NetworkBehaviour {
        [SerializeField] private NetworkObject trainingDummyPrefab;
        [SerializeField] private Vector2[] spawnPoints;

        public override void OnStartServer() {
            base.OnStartServer();
            
            foreach (Vector2 point in spawnPoints)
            {
                NetworkObject dummy = Instantiate(trainingDummyPrefab, point, Quaternion.identity);
                Spawn(dummy, scene: gameObject.scene);
            }
        }
    }
}