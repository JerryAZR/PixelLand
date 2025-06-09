using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Scripts.Combat
{
    public class MeleeAttack : NetworkBehaviour {
        private static readonly int _slash = Animator.StringToHash("Slash");
        [SerializeField] private Transform hitbox; // Hitbox object
        [SerializeField] private Animator animator;
        [SerializeField] private LayerMask hittableLayers;
        [SerializeField] private int damage = 10;
        
        private readonly List<Collider2D> _attackHits = new();

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (!hitbox) return;

            Gizmos.color = Color.red;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(hitbox.position, hitbox.rotation, hitbox.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
        
        public void PerformAttack() {
            if (!hitbox) {
                Debug.LogWarning("Hitbox not assigned.");
                return;
            }
            animator.SetTrigger(_slash);
            BroadcastTrigger(_slash);

            // Use lossyScale to get world size, and rotation.z as angle
            Vector2 size = new(
                Mathf.Abs(hitbox.lossyScale.x),
                Mathf.Abs(hitbox.lossyScale.y)
            );

            Vector2 center = hitbox.position;
            float angle = hitbox.eulerAngles.z;
            
            Debug.Log($"Checking Box center:{center} size:{size} angle:{angle}");
            
            ContactFilter2D filter = new();
            filter.SetLayerMask(hittableLayers);
            _attackHits.Clear();
            PhysicsScene2D physicsScene = gameObject.scene.GetPhysicsScene2D();
            int numHits = physicsScene.OverlapBox(center, size, angle, filter, _attackHits);
            Debug.Log($"Hits: {numHits}");
            for (int i = 0; i < numHits; i++) {
                Collider2D col = _attackHits[i];
                if (col.TryGetComponent(out IHittable target))
                    target.TakeHit(damage);
                else {
                    Debug.Log($"{col} is not a valid hittable.");
                }
            }

        }

        [ServerRpc(RequireOwnership = true)]
        private void BroadcastTrigger(int id) {
            SetObserverTrigger(id);
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void SetObserverTrigger(int id) {
            animator.SetTrigger(id);
        }
    }
}