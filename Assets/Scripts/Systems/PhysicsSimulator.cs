using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Systems {
    public class PhysicsSimulator : MonoBehaviour {
        private readonly HashSet<PhysicsScene2D> _physicsScene2Ds = new();
        private readonly HashSet<PhysicsScene> _physicsScene3Ds = new();
        public static PhysicsSimulator Instance { get; private set; }

        private void Awake() {
            if (Instance && Instance != this) {
                Debug.LogError("Multiple ServerPhysicsManagers found!");
                Destroy(this);
                return;
            }

            Instance = this;

            Physics.simulationMode = SimulationMode.Script;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        private void FixedUpdate() {
            // if (InstanceFinder.IsServerStarted == false) return;
            foreach (PhysicsScene2D scene in _physicsScene2Ds) scene.Simulate(Time.fixedDeltaTime);

            foreach (PhysicsScene scene in _physicsScene3Ds) scene.Simulate(Time.fixedDeltaTime);
        }

        public void RegisterScene(PhysicsScene2D scene) {
            _physicsScene2Ds.Add(scene);
        }

        public void UnregisterScene(PhysicsScene2D scene) {
            _physicsScene2Ds.Remove(scene);
        }

        public void RegisterScene(PhysicsScene scene) {
            _physicsScene3Ds.Add(scene);
        }

        public void UnregisterScene(PhysicsScene scene) {
            _physicsScene3Ds.Remove(scene);
        }
    }
}