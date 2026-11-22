using UnityEngine;

namespace EmotionsGame.Audio
{
    public enum SfxCue
    {
        UiClick,
        Pickup,
        Damage,
        Win,
        Lose,
        Shoot,
        Dash,
        LevelStart
    }

    [DisallowMultipleComponent]
    public sealed class SfxService : MonoBehaviour
    {
        public static SfxService Instance { get; private set; }

        private AudioSource source;
        private AudioClip uiClick;
        private AudioClip pickup;
        private AudioClip damage;
        private AudioClip win;
        private AudioClip lose;
        private AudioClip shoot;
        private AudioClip dash;
        private AudioClip levelStart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = Mathf.Clamp01(UnityEngine.PlayerPrefs.GetFloat("EmotionsSfx", 0.8f));

            uiClick = Tone("UI Click", 620f, 0.06f, 0.4f, false);
            pickup = Tone("Pickup", 880f, 0.14f, 0.6f, false);
            damage = Tone("Damage", 160f, 0.18f, 0.8f, true);
            win = Tone("Win", 660f, 0.35f, 0.7f, false);
            lose = Tone("Lose", 180f, 0.42f, 0.8f, false);
            shoot = Tone("Shoot", 420f, 0.08f, 0.7f, true);
            dash = Tone("Dash", 320f, 0.12f, 0.6f, true);
            levelStart = Tone("Level Start", 520f, 0.2f, 0.5f, false);
        }

        public void SetVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            source.volume = clamped;
            UnityEngine.PlayerPrefs.SetFloat("EmotionsSfx", clamped);
            UnityEngine.PlayerPrefs.Save();
        }

        public void Play(SfxCue cue)
        {
            if (source == null)
            {
                return;
            }

            AudioClip clip = cue switch
            {
                SfxCue.UiClick => uiClick,
                SfxCue.Pickup => pickup,
                SfxCue.Damage => damage,
                SfxCue.Win => win,
                SfxCue.Lose => lose,
                SfxCue.Shoot => shoot,
                SfxCue.Dash => dash,
                SfxCue.LevelStart => levelStart,
                _ => null
            };

            if (clip != null)
            {
                source.PlayOneShot(clip, 0.7f);
            }
        }

        private static AudioClip Tone(string name, float frequency, float duration, float decay, bool noise)
        {
            const int sampleRate = 22050;
            int length = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-decay * 8f * t / Mathf.Max(0.01f, duration));
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
                if (noise)
                {
                    wave = wave * 0.6f + (Mathf.PerlinNoise(i * 0.17f, 0.5f) * 2f - 1f) * 0.4f;
                }

                samples[i] = wave * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
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
