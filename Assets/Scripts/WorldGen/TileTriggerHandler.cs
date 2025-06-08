using System;
using System.Collections.Generic;
using UnityEngine;

using TriggerEnterAction = System.Action<UnityEngine.Vector3Int, UnityEngine.Collider2D>;

namespace Scripts.WorldGen {
    public class TileTriggerHandler : MonoBehaviour {
        private readonly Dictionary<Vector3Int, TriggerEnterAction> _triggerEnterActions = new();

        public void SetAction(Vector3Int pos, TriggerEnterAction action) {
            _triggerEnterActions[pos] = action;
        }

        public void Invoke(Vector3Int pos, Collider2D collision) {
            if (_triggerEnterActions.TryGetValue(pos, out var action)) action?.Invoke(pos, collision);
        }

        public TriggerEnterAction GetAction(Vector3Int pos) {
            return _triggerEnterActions.GetValueOrDefault(pos, null);
        }
    }
}