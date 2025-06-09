namespace Scripts.Combat {
    public interface IHittable {
        public void TakeHit(float damage);
    }

    public enum Faction {
        Player,
        Breakable,
        Enemy
    }
}