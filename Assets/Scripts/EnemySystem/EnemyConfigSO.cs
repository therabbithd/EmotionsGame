using DG.Tweening;
using UnityEngine;

namespace EmotionsGame.Enemies
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Emotions Game/Enemies/Enemy Config")]
    public sealed class EnemyConfigSO : ScriptableObject
    {
        public enum ActivationBehavior
        {
            Chase,
            HoldPosition
        }

        [Header("Identity / Combat")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0)] private int score = 10;
        [SerializeField] private GameObject destructionEffect;

        [Header("Float Motion (DOTween)")]
        [SerializeField, Min(0f)] private float floatAmplitude = 0.35f;
        [SerializeField, Min(0.01f)] private float floatDuration = 2f;
        [SerializeField, Min(0f)] private float floatDelay = 0f;
        [SerializeField] private Ease floatEase = Ease.InOutSine;

        [Header("Rotation (DOTween)")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField, Min(0.01f)] private float rotationDuration = 6f;
        [SerializeField, Min(0f)] private float rotationDelay = 0f;
        [SerializeField] private Ease rotationEase = Ease.Linear;
        [SerializeField] private bool fullTurn = true;

        [Header("Free Movement")]
        [SerializeField, Min(0f)] private float patrolRadius = 8f;
        [SerializeField, Min(0.01f)] private float patrolSpeed = 2f;
        [SerializeField, Min(0.01f)] private float chaseSpeed = 3.5f;
        [SerializeField, Min(0f)] private float minPause = 1f;
        [SerializeField, Min(0f)] private float maxPause = 3f;
        [SerializeField, Min(0.01f)] private float waypointReachDistance = 0.5f;

        [Header("Proximity Activation")]
        [SerializeField, Min(0f)] private float detectionRadius = 6f;
        [SerializeField, Min(1f)] private float releaseRadiusMultiplier = 1.5f;
        [SerializeField] private ActivationBehavior activationBehavior = ActivationBehavior.Chase;

        public float MaxHealth => maxHealth;
        public int Score => score;
        public GameObject DestructionEffect => destructionEffect;
        public float FloatAmplitude => floatAmplitude;
        public float FloatDuration => floatDuration;
        public float FloatDelay => floatDelay;
        public Ease FloatEase => floatEase;
        public Vector3 RotationAxis => rotationAxis;
        public float RotationDuration => rotationDuration;
        public float RotationDelay => rotationDelay;
        public Ease RotationEase => rotationEase;
        public bool FullTurn => fullTurn;
        public float PatrolRadius => patrolRadius;
        public float PatrolSpeed => patrolSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float MinPause => minPause;
        public float MaxPause => maxPause;
        public float WaypointReachDistance => waypointReachDistance;
        public float DetectionRadius => detectionRadius;
        public float ReleaseRadiusMultiplier => releaseRadiusMultiplier;
        public ActivationBehavior Behavior => activationBehavior;

        private void OnValidate()
        {
            maxPause = Mathf.Max(minPause, maxPause);
            releaseRadiusMultiplier = Mathf.Max(1f, releaseRadiusMultiplier);
            if (rotationAxis.sqrMagnitude < 0.0001f)
            {
                rotationAxis = Vector3.up;
            }
        }
    }
}
