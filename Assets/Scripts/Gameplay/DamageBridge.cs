using UnityEngine;
using EmotionsGameHealth = EmotionsGame.Health.IDamageable;
using EmotionsGameEnemy = EmotionsGame.Enemies.IDamageable;

namespace EmotionsGame.Gameplay
{
    public static class DamageBridge
    {
        public static bool TryDealDamage(GameObject target, float amount)
        {
            if (target == null || amount <= 0f)
            {
                return false;
            }

            bool applied = false;
            EmotionsGameEnemy[] enemyTargets = target.GetComponentsInParent<EmotionsGameEnemy>(true);
            foreach (EmotionsGameEnemy enemy in enemyTargets)
            {
                enemy.TakeDamage(amount);
                applied = true;
            }

            EmotionsGameHealth[] healthTargets = target.GetComponentsInParent<EmotionsGameHealth>(true);
            foreach (EmotionsGameHealth health in healthTargets)
            {
                health.TakeDamage(Mathf.CeilToInt(amount));
                applied = true;
            }

            return applied;
        }
    }
}
