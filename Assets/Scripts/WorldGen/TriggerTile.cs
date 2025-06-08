using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.WorldGen
{
    public class TriggerTile : MonoBehaviour
    {
        private Tilemap _tilemap;
        private Vector3Int _pos;
        private Action<Vector3Int, Collider2D> _triggerEnterAction;
        private void Start()
        {
            _tilemap = GetComponentInParent<Tilemap>();
            _pos = _tilemap.WorldToCell(transform.position);
            _triggerEnterAction = GetComponentInParent<TileTriggerHandler>()?.GetAction(_pos);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            Debug.Log("TriggerTile enter: " + collision.gameObject.name);
            _triggerEnterAction?.Invoke(_pos, collision);
        }
    }
}
