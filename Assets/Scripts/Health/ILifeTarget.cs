namespace EmotionsGame.Health
{
    /// <summary>Contract for entities that can receive extra lives from a pickup.</summary>
    public interface ILifeTarget
    {
        void AddLives(int amount);
    }
}
