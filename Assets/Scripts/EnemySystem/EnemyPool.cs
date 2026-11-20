using System.Collections.Generic;
using UnityEngine;

namespace EmotionsGame.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance { get; private set; }

        [SerializeField] private EnemyController prefab;
        [SerializeField, Min(0)] private int prewarmCount = 8;
        [SerializeField] private bool expandable = true;

        private readonly Queue<EnemyController> available = new Queue<EnemyController>();
        private readonly HashSet<EnemyController> active = new HashSet<EnemyController>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Prewarm();
        }

        public void Prewarm()
        {
            if (prefab == null)
            {
                Debug.LogWarning("EnemyPool has no prefab assigned; no enemies were prewarmed.", this);
                return;
            }

            for (int i = 0; i < prewarmCount; i++)
            {
                EnemyController instance = CreateInstance();
                available.Enqueue(instance);
            }
        }

        public EnemyController Get(Vector3 position, Quaternion rotation)
        {
            EnemyController instance;

            if (available.Count > 0)
            {
                instance = available.Dequeue();
            }
            else if (expandable)
            {
                instance = CreateInstance();
            }
            else
            {
                return null;
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            active.Add(instance);
            return instance;
        }

        public void Return(EnemyController instance)
        {
            if (instance == null || !active.Remove(instance))
            {
                return;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(transform, true);
            available.Enqueue(instance);
        }

        private EnemyController CreateInstance()
        {
            EnemyController instance = Instantiate(prefab, transform);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
