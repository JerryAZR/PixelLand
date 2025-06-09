using FishNet.Object;
using Scripts.Combat;
using UnityEngine;

namespace Scripts.Characters {
    public class TrainingDummy : NetworkBehaviour, IHittable {
        private static readonly int _hitTrigger = Animator.StringToHash("Hit");
        [SerializeField] private Animator animator;
        
        [ServerRpc(RequireOwnership = false)]
        public void TakeHit(float damage) {
            Debug.Log($"TrainingDummy was hit! Damage: {damage}");
            SetObserverTrigger(_hitTrigger);
        }

        [ObserversRpc]
        private void SetObserverTrigger(int id) {
            animator.SetTrigger(id);
        }
    }
}