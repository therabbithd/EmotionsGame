using UnityEngine;

namespace EmotionsGame.Enemies
{
    public static class TargetResolver
    {
        public static Transform Resolve(Transform assigned, string tag, ref Transform cached)
        {
            if (assigned != null)
            {
                cached = assigned;
                return cached;
            }

            if (cached != null)
            {
                return cached;
            }

            GameObject found = GameObject.FindGameObjectWithTag(tag);
            if (found != null)
            {
                cached = found.transform;
            }

            return cached;
        }
    }
}
