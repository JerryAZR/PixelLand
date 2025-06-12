using System.Collections;
using System.Collections.Generic;

namespace Scripts.Utilities {
    public class IndexedSet<T> : IEnumerable<T> {
        private readonly Dictionary<T, int> _indexMap = new();
        private readonly List<T> _list = new();

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        public IEnumerator<T> GetEnumerator() {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool Contains(T item) {
            return _indexMap.ContainsKey(item);
        }

        public bool Add(T item) {
            if (_indexMap.ContainsKey(item)) return false;

            _indexMap[item] = _list.Count;
            _list.Add(item);
            return true;
        }

        public bool Remove(T item) {
            if (!_indexMap.TryGetValue(item, out int index)) return false;

            int lastIndex = _list.Count - 1;
            T lastItem = _list[lastIndex];

            if (index != lastIndex) {
                _list[index] = lastItem;
                _indexMap[lastItem] = index;
            }

            _list.RemoveAt(lastIndex);
            _indexMap.Remove(item);
            return true;
        }

        public T ElementAt(int index) {
            return _list[index];
        }

        public void Clear() {
            _list.Clear();
            _indexMap.Clear();
        }
    }
}