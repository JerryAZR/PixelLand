using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scripts.Combat;
using Scripts.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Controls {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerControl : NetworkBehaviour {
        private static CharacterActions _inputs;

        // Configurable params
        [SerializeField] private float moveSpeed;
        [SerializeField] private MeleeAttack attackObject;

        // Private states
        private readonly SyncVar<Vector2> _velocity =
            new(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));

        private Rigidbody2D _rb;

        public Vector2 Velocity {
            get => _velocity.Value;
            [ServerRpc(RunLocally = true)] set => _velocity.Value = value;
        }

        private void Start() {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update() {
            if (IsOwner) {
                Velocity = _inputs.Field.enabled ?
                    _inputs.Field.Move.ReadValue<Vector2>() * moveSpeed : Vector2.zero;
            }
        }

        private void FixedUpdate() {
            _rb.linearVelocity = Velocity;
        }

        public override void OnStartClient() {
            base.OnStartClient();

            if (!IsOwner) return;

            _inputs = new CharacterActions();
            InputActionStack.PushInputMap(_inputs.Field.Get(), this);
            _inputs.Field.Attack.performed += AttackOnPerformed;

            // Configure camera follow
            CameraFinder.VirtualCamera.Follow = transform;
        }

        private void AttackOnPerformed(InputAction.CallbackContext obj) {
            if (!IsOwner) return;
            Vector2 mousePos = _inputs.Field.Aim.ReadValue<Vector2>();
            Vector2 direction = (Vector2)CameraFinder.MainCamera.ScreenToWorldPoint(mousePos) - _rb.position;
            direction.Normalize();
            Debug.Log($"Mouse position: {mousePos}, Attack direction: {direction}");
            
            // Set rotation (2D: angle between right and direction)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            attackObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            attackObject.PerformAttack();
        }

        public override void OnStopClient() {
            if (IsOwner) InputActionStack.PopInputMap(this);
        }
    }
}