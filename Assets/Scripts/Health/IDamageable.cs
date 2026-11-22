namespace EmotionsGame.Health
{
    /// <summary>Common contract for entities that can receive integer damage.</summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
