using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scripts.Systems;
using UnityEngine;

namespace Scripts.Controls {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerControl : NetworkBehaviour {
        private static CharacterActions _inputs;

        // Configurable params
        [SerializeField] private float moveSpeed;

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

            // Configure camera follow
            CameraFinder.VirtualCamera.Follow = transform;
        }

        public override void OnStopClient() {
            if (IsOwner) InputActionStack.PopInputMap(this);
        }
    }
}