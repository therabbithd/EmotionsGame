using UnityEngine;

namespace EmotionsGame.Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Emotions Game/Levels/Level Config")]
    public sealed class LevelConfig : ScriptableObject
    {
        [SerializeField] private string displayName = "Nivel";
        [TextArea(2, 4)]
        [SerializeField] private string objective = "Derrota a los enemigos.";
        [SerializeField, Min(1)] private int targetDefeats = 8;
        [SerializeField, Min(0f)] private float timeLimit;
        [SerializeField] private Color floorColor = new Color(0.18f, 0.2f, 0.25f);
        [SerializeField] private Color accentColor = new Color(1f, 0.55f, 0.25f);
        [SerializeField] private Color fogColor = new Color(0.06f, 0.07f, 0.1f);

        public string DisplayName => displayName;
        public string Objective => objective;
        public int TargetDefeats => targetDefeats;
        public float TimeLimit => timeLimit;
        public Color FloorColor => floorColor;
        public Color AccentColor => accentColor;
        public Color FogColor => fogColor;
    }
}
