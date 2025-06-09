using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Scripts.WorldGen {
    public class TownRoot : NetworkBehaviour {
        private readonly SyncVar<int> _randomSeed = new();
        private MapData MapData { get; set; }

        private int RandomSeed {
            get => _randomSeed.Value;
            [Server] set => _randomSeed.Value = value;
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            if (IsServerStarted) RandomSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateAllWithSeed(RandomSeed);
        }

        // [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void GenerateAllWithSeed(int seed) {
            Debug.Log($"Generating map with seed {seed:X}");
            BoundsInt bounds = new();
            bounds.SetMinMax(
                new Vector3Int(-64, -64, 0),
                new Vector3Int(64, 64, 1)
            );
            MapData = new MapData { MapBounds = bounds };
            foreach (IMapGenerator generator in GetComponents<IMapGenerator>()) {
                generator.Generate(MapData);
                generator.Populate(MapData);
            }
        }
    }
}