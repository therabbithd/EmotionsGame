using UnityEngine;

namespace EmotionsGame.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyDimensionRotation : MonoBehaviour
    {
        [SerializeField] private float dimensionAYAngle;
        [SerializeField] private float dimensionBYAngle = 90f;
        [SerializeField] private float fixedXAngle;
        [SerializeField] private float fixedZAngle;
        [SerializeField, Min(0f)] private float rotationSpeed = 10f;

        private Quaternion targetRotation = Quaternion.identity;

        private void OnEnable()
        {
            RollingSphere.DimensionChanged += OnDimensionChanged;
            SetTargetRotation(RollingSphere.ActiveDimension);
            transform.rotation = targetRotation;
        }

        private void OnDisable()
        {
            RollingSphere.DimensionChanged -= OnDimensionChanged;
        }

        private void OnDimensionChanged(int dimension)
        {
            SetTargetRotation(dimension);
        }

        private void Update()
        {
            if (transform.rotation == targetRotation)
            {
                return;
            }

            transform.rotation = rotationSpeed <= 0f
                ? targetRotation
                : Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void SetTargetRotation(int dimension)
        {
            float yAngle = dimension == 0 ? dimensionAYAngle : dimensionBYAngle;
            targetRotation = Quaternion.Euler(fixedXAngle, yAngle, fixedZAngle);
        }
    }
}
