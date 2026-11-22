using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using EmotionsGame.Audio;
using EmotionsGame.Core;
using EmotionsGame.Levels;
using EmotionsGame.Save;

namespace EmotionsGame.UI
{
    [DisallowMultipleComponent]
    public sealed class GameUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private CanvasGroup menuPanel;
        [SerializeField] private CanvasGroup hudPanel;
        [SerializeField] private CanvasGroup pausePanel;
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private CanvasGroup creditsPanel;
        [SerializeField] private CanvasGroup endPanel;
        [SerializeField] private CanvasGroup transitionPanel;

        [Header("Menu")]
        [SerializeField] private Text menuBestText;
        [SerializeField] private Text menuContinueText;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("HUD")]
        [SerializeField] private Text levelText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text timerText;

        [Header("Pause")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMenuButton;
        [SerializeField] private Button pauseSettingsButton;

        [Header("Settings")]
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Button sfxMinusButton;
        [SerializeField] private Button sfxPlusButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button settingsBackButton;

        [Header("Credits")]
        [SerializeField] private Text creditsText;
        [SerializeField] private Button creditsBackButton;

        [Header("End")]
        [SerializeField] private Text endTitleText;
        [SerializeField] private Text endDetailsText;
        [SerializeField] private Button endNextButton;
        [SerializeField] private Button endRetryButton;
        [SerializeField] private Button endMenuButton;

        [Header("Transition")]
        [SerializeField] private Text transitionText;

        public bool IsMenuVisible => menuPanel != null && menuPanel.alpha > 0.5f;

        private Coroutine flashRoutine;

        private void Awake()
        {
            Wire(newGameButton, () => GameFlowController.Instance?.StartNewGame());
            Wire(continueButton, () => GameFlowController.Instance?.ContinueGame());
            Wire(settingsButton, () => GameFlowController.Instance?.ShowSettings());
            Wire(creditsButton, () => GameFlowController.Instance?.ShowCredits());
            Wire(quitButton, () => GameFlowController.Instance?.QuitGame());

            Wire(resumeButton, () => GameFlowController.Instance?.Resume());
            Wire(pauseRestartButton, () => GameFlowController.Instance?.RestartLevel());
            Wire(pauseMenuButton, () => GameFlowController.Instance?.ReturnToMenu());
            Wire(pauseSettingsButton, () => GameFlowController.Instance?.ShowSettings());

            Wire(sfxMinusButton, () => AdjustSfx(-0.1f));
            Wire(sfxPlusButton, () => AdjustSfx(0.1f));
            Wire(resetButton, () => GameFlowController.Instance?.ResetProgress());
            Wire(settingsBackButton, () => GameFlowController.Instance?.Back());

            Wire(creditsBackButton, () => GameFlowController.Instance?.Back());

            Wire(endNextButton, () => GameFlowController.Instance?.NextLevel());
            Wire(endRetryButton, () => GameFlowController.Instance?.RestartLevel());
            Wire(endMenuButton, () => GameFlowController.Instance?.ReturnToMenu());

            GameState.Changed += Refresh;
            SetPanel(transitionPanel, false);
        }

        private void OnDestroy()
        {
            GameState.Changed -= Refresh;
        }

        public void Show(GameFlowController.Screen screen)
        {
            SetPanel(menuPanel, screen == GameFlowController.Screen.Menu);
            SetPanel(hudPanel, screen == GameFlowController.Screen.Playing || screen == GameFlowController.Screen.Paused || screen == GameFlowController.Screen.Settings || screen == GameFlowController.Screen.Credits);
            SetPanel(pausePanel, screen == GameFlowController.Screen.Paused);
            SetPanel(settingsPanel, screen == GameFlowController.Screen.Settings);
            SetPanel(creditsPanel, screen == GameFlowController.Screen.Credits);
            SetPanel(endPanel, screen == GameFlowController.Screen.End);

            if (screen == GameFlowController.Screen.Menu)
            {
                SaveData save = SaveService.Data;
                if (menuBestText != null) menuBestText.text = $"Mejor puntuación: {save.bestScore}";
                if (menuContinueText != null)
                {
                    menuContinueText.text = save.lastPlayedUtc.Length > 0
                        ? $"Continuar nivel {Mathf.Clamp(save.lastLevel + 1, 1, 3)}"
                        : "Continuar (sin partida)";
                }

                if (continueButton != null) continueButton.interactable = save.lastPlayedUtc.Length > 0;
            }

            if (screen == GameFlowController.Screen.Settings)
            {
                RefreshSfx();
            }

            if (screen == GameFlowController.Screen.Credits && creditsText != null)
            {
                creditsText.text = "Emotions Game\n\nDiseño y código: equipo del proyecto\nMúsica: se añadirá manualmente\nGracias por jugar.";
            }

            Refresh();
        }

        public void SetObjective(string text)
        {
            if (objectiveText != null)
            {
                objectiveText.text = text;
            }
        }

        public void SetTimer(float seconds)
        {
            if (timerText == null)
            {
                return;
            }

            timerText.text = seconds <= 0f ? string.Empty : $"Tiempo: {Mathf.CeilToInt(seconds)}s";
        }

        public void SetEnd(bool victory, bool runComplete, int score, string reason)
        {
            if (endTitleText != null)
            {
                endTitleText.text = runComplete ? "¡JUGO COMPLETADO!" : victory ? "NIVEL SUPERADO" : "FIN DEL INTENTO";
            }

            if (endDetailsText != null)
            {
                endDetailsText.text = $"{reason}\n\nPuntuación total: {score}\nMejor: {SaveService.Data.bestScore}";
            }

            if (endNextButton != null)
            {
                endNextButton.gameObject.SetActive(victory && !runComplete);
            }

            if (endRetryButton != null)
            {
                endRetryButton.gameObject.SetActive(!victory || runComplete);
            }
        }

        public void Flash(string message)
        {
            if (transitionPanel == null)
            {
                return;
            }

            if (transitionText != null)
            {
                transitionText.text = message;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            transitionPanel.alpha = 1f;
            transitionPanel.blocksRaycasts = true;
            yield return new WaitForSecondsRealtime(0.35f);

            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.unscaledDeltaTime;
                transitionPanel.alpha = 1f - Mathf.Clamp01(elapsed / 0.4f);
                yield return null;
            }

            transitionPanel.alpha = 0f;
            transitionPanel.blocksRaycasts = false;
            flashRoutine = null;
        }

        private void Refresh()
        {
            if (levelText != null) levelText.text = $"Nivel {GameState.DisplayLevel} / 3";
            if (scoreText != null) scoreText.text = $"Puntos: {GameState.TotalScore}";
            if (livesText != null) livesText.text = $"Vidas: {GameState.Lives}";
            if (ammoText != null)
            {
                ammoText.text = $"Munición: {GameState.Ammo}";
                ammoText.gameObject.SetActive(GameState.Ammo > 0);
            }
        }

        private void AdjustSfx(float delta)
        {
            float value = Mathf.Clamp01(SaveService.Data.sfxVolume + delta);
            SaveService.SetSfxVolume(value);
            SfxService.Instance?.SetVolume(value);
            SfxService.Instance?.Play(SfxCue.UiClick);
            RefreshSfx();
        }

        private void RefreshSfx()
        {
            if (sfxValueText != null)
            {
                sfxValueText.text = $"Efectos: {Mathf.RoundToInt(SaveService.Data.sfxVolume * 100f)}%";
            }
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            button.onClick.AddListener(() => SfxService.Instance?.Play(SfxCue.UiClick));
        }

        private static void SetPanel(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
