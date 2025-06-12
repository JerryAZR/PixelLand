using System.Collections.Generic;

namespace Scripts.Utilities {
    public class UnionFind<T> {
        private readonly Dictionary<T, int> elementToIndex;
        private readonly List<int> parent;
        private readonly List<int> rank;

        public UnionFind() {
            elementToIndex = new Dictionary<T, int>();
            parent = new List<int>();
            rank = new List<int>();
            ClusterCount = 0;
        }

        public int ClusterCount { get; private set; }

        private int FindByIndex(int index) {
            if (parent[index] != index) parent[index] = FindByIndex(parent[index]); // Path compression

            return parent[index];
        }

        public int Find(T item) {
            if (!elementToIndex.ContainsKey(item))
                Add(item);

            return FindByIndex(elementToIndex[item]);
        }

        public bool Union(T x, T y) {
            int rootX = Find(x);
            int rootY = Find(y);
            if (rootX != rootY) {
                if (rank[rootX] > rank[rootY]) {
                    parent[rootY] = rootX;
                } else if (rank[rootX] < rank[rootY]) {
                    parent[rootX] = rootY;
                } else {
                    parent[rootY] = rootX;
                    rank[rootX]++;
                }

                // Union successful
                ClusterCount--;
                return true;
            }

            // Union failed - already in the same set
            return false;
        }

        private void Add(T item) {
            if (elementToIndex.ContainsKey(item)) return;
            
            elementToIndex[item] = parent.Count;
            parent.Add(parent.Count);
            rank.Add(0);
            ClusterCount++;
        }

        public bool Contains(T item) {
            return elementToIndex.ContainsKey(item);
        }
    }
}