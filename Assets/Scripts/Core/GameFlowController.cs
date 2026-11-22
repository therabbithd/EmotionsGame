using UnityEngine;
using UnityEngine.SceneManagement;
using EmotionsGame.Audio;
using EmotionsGame.Core;
using EmotionsGame.Gameplay;
using EmotionsGame.Input;
using EmotionsGame.Levels;
using EmotionsGame.Save;
using EmotionsGame.UI;
using EmotionsGame.Health;
using EmotionsGame.Score;

namespace EmotionsGame.Core
{
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour
    {
        public enum Screen
        {
            Boot,
            Menu,
            Playing,
            Paused,
            Settings,
            Credits,
            End
        }

        public static GameFlowController Instance { get; private set; }

        public Screen CurrentScreen => currentScreen;

        private Screen currentScreen = Screen.Boot;
        private Screen overlayReturnScreen = Screen.Menu;
        private GameUI ui;
        private PlayerHealth playerHealth;
        private ScoreManager levelScore;
        private LevelDirector director;
        private AmmoController ammoController;
        private DashController dashController;
        private int lastScoreValue;
        private bool lastResultVictory;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject root = new GameObject("EmotionsGame - Game Flow");
            DontDestroyOnLoad(root);
            root.AddComponent<GameFlowController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveService.Load();

            if (GetComponent<GameInput>() == null) gameObject.AddComponent<GameInput>();
            if (GetComponent<SfxService>() == null) gameObject.AddComponent<SfxService>();
            GetComponent<SfxService>()?.SetVolume(SaveService.Data.sfxVolume);

            GameInput input = GameInput.Instance;
            if (input != null)
            {
                input.PausePressed += OnPause;
                input.ConfirmPressed += OnConfirm;
                input.CancelPressed += OnCancel;
                input.ShotPressed += OnShot;
                input.DashPressed += OnDash;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void Start()
        {
            if (ui == null)
            {
                ui = FindAnyObjectByType<GameUI>();
            }

            if (ui == null)
            {
                GameUI prefab = Resources.Load<GameUI>("UI/GameUI");
                if (prefab != null)
                {
                    ui = Instantiate(prefab, transform);
                }
                else
                {
                    GameObject fallback = new GameObject("Runtime Game UI");
                    fallback.transform.SetParent(transform, false);
                    ui = fallback.AddComponent<GameUI>();
                }
            }

            if (currentScreen == Screen.Boot)
            {
                string scene = SceneManager.GetActiveScene().name;
                if (scene == GameSceneNames.MainMenu)
                {
                    ShowMenu();
                }
                else if (IsLevelScene(scene))
                {
                    LoadLevel(Mathf.Max(0, System.Array.IndexOf(GameSceneNames.Levels, scene)));
                }
                else
                {
                    ShowMenu();
                }
            }
        }

        public void StartNewGame()
        {
            GameState.BeginNewRun();
            SaveService.RecordRunStart(0);
            LoadLevel(0);
        }

        public void ContinueGame()
        {
            SaveData save = SaveService.Data;
            if (save.lastPlayedUtc.Length == 0)
            {
                StartNewGame();
                return;
            }

            GameState.ContinueRun(save.lastLevel, save.lastScore, save.lastLives);
            SaveService.RecordRunStart(save.lastLevel);
            LoadLevel(save.lastLevel);
        }

        public void RestartLevel()
        {
            GameState.ResetLevelScore();
            SaveService.RecordRunStart(GameState.CurrentLevelIndex);
            LoadLevel(GameState.CurrentLevelIndex);
        }

        public void NextLevel()
        {
            if (GameState.CurrentLevelIndex >= GameSceneNames.LevelCount - 1)
            {
                ReturnToMenu();
                return;
            }

            GameState.SetLevel(GameState.CurrentLevelIndex + 1);
            SaveService.RecordRunStart(GameState.CurrentLevelIndex);
            LoadLevel(GameState.CurrentLevelIndex);
        }

        public void LoadLevel(int index)
        {
            int clamped = Mathf.Clamp(index, 0, GameSceneNames.LevelCount - 1);
            GameState.SetLevel(clamped);
            Time.timeScale = 1f;
            ui?.Flash(GameSceneNames.Levels[clamped].Replace("Level", "Nivel "));
            SceneManager.LoadScene(GameSceneNames.ForIndex(clamped));
        }

        public void ReturnToMenu()
        {
            GameState.StopRun();
            UnbindScene();
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameSceneNames.MainMenu);
            ShowMenu();
        }

        public void Pause()
        {
            if (currentScreen != Screen.Playing)
            {
                return;
            }

            GameState.SetPaused(true);
            SetScreen(Screen.Paused);
        }

        public void Resume()
        {
            if (currentScreen != Screen.Paused && currentScreen != Screen.Settings && currentScreen != Screen.Credits)
            {
                return;
            }

            GameState.SetPaused(false);
            SetScreen(Screen.Playing);
        }

        public void ShowSettings()
        {
            overlayReturnScreen = currentScreen == Screen.Settings ? overlayReturnScreen : currentScreen;
            if (currentScreen == Screen.Playing)
            {
                GameState.SetPaused(true);
            }

            SetScreen(Screen.Settings);
        }

        public void ShowCredits()
        {
            overlayReturnScreen = currentScreen == Screen.Credits ? overlayReturnScreen : currentScreen;
            if (currentScreen == Screen.Playing)
            {
                GameState.SetPaused(true);
            }

            SetScreen(Screen.Credits);
        }

        public void Back()
        {
            if (currentScreen != Screen.Settings && currentScreen != Screen.Credits)
            {
                return;
            }

            if (overlayReturnScreen == Screen.Paused)
            {
                SetScreen(Screen.Paused);
            }
            else if (overlayReturnScreen == Screen.Menu || !GameState.Running)
            {
                ShowMenu();
            }
            else
            {
                Resume();
            }
        }

        public void CompleteLevel()
        {
            if (!GameState.Running)
            {
                return;
            }

            director?.StopLevel();
            SfxService.Instance?.Play(SfxCue.Win);
            int level = GameState.CurrentLevelIndex;
            bool runComplete = level >= GameSceneNames.LevelCount - 1;
            SaveService.RecordLevelComplete(level, GameState.TotalScore);
            if (runComplete)
            {
                SaveService.RecordRunComplete(GameState.TotalScore);
            }

            lastResultVictory = true;
            GameState.StopRun();
            ui?.SetEnd(true, runComplete, GameState.TotalScore, runComplete ? "Has superado los 3 niveles." : "Objetivo completado.");
            SetScreen(Screen.End);
        }

        public void GameOver()
        {
            if (!GameState.Running)
            {
                return;
            }

            director?.StopLevel();
            SfxService.Instance?.Play(SfxCue.Lose);
            SaveService.RecordGameOver(GameState.CurrentLevelIndex, GameState.TotalScore);
            lastResultVictory = false;
            GameState.StopRun();
            ui?.SetEnd(false, false, GameState.TotalScore, "Has perdido todas las vidas.");
            SetScreen(Screen.End);
        }

        public void ResetProgress()
        {
            SaveService.ResetAll();
            SfxService.Instance?.SetVolume(SaveService.Data.sfxVolume);
            ui?.Flash("Progreso borrado");
            ShowMenu();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMenu()
        {
            Time.timeScale = 1f;
            SetScreen(Screen.Menu);
        }

        private void SetScreen(Screen screen)
        {
            currentScreen = screen;
            ui?.Show(screen);
        }

        private void OnPause()
        {
            if (currentScreen == Screen.Playing) Pause();
            else if (currentScreen == Screen.Paused) Resume();
            else if (currentScreen == Screen.Settings || currentScreen == Screen.Credits) Back();
        }

        private void OnConfirm()
        {
            if (currentScreen == Screen.Menu) StartNewGame();
            else if (currentScreen == Screen.End)
            {
                if (lastResultVictory) NextLevel();
                else RestartLevel();
            }
            else if (currentScreen == Screen.Settings || currentScreen == Screen.Credits) Back();
        }

        private void OnCancel()
        {
            if (currentScreen == Screen.Settings || currentScreen == Screen.Credits || currentScreen == Screen.Paused)
            {
                Back();
            }
        }

        private void OnShot()
        {
            if (currentScreen == Screen.Playing)
            {
                ammoController?.TryFire();
            }
        }

        private void OnDash()
        {
            if (currentScreen == Screen.Playing)
            {
                dashController?.TryDash();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnbindScene();

            if (!IsLevelScene(scene.name))
            {
                ShowMenu();
                return;
            }

            int index = Mathf.Max(0, System.Array.IndexOf(GameSceneNames.Levels, scene.name));
            GameState.SetLevel(index);
            GameState.SetPaused(false);
            Time.timeScale = 1f;

            levelScore = FindAnyObjectByType<ScoreManager>();
            if (levelScore != null)
            {
                levelScore.ResetScore();
                lastScoreValue = 0;
                levelScore.ScoreChanged += OnLevelScoreChanged;
            }

            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.LivesChanged += OnLivesChanged;
                playerHealth.Died += GameOver;
                GameState.SetLives(playerHealth.CurrentLives);
            }

            ammoController = FindAnyObjectByType<AmmoController>();
            dashController = FindAnyObjectByType<DashController>();

            director = FindAnyObjectByType<LevelDirector>();
            if (director != null)
            {
                director.InfoChanged += OnLevelInfo;
                director.Completed += CompleteLevel;
                director.Failed += GameOver;
                ui?.SetObjective(director.ObjectiveText);
                director.Begin();
            }

            SfxService.Instance?.Play(SfxCue.LevelStart);
            ui?.Flash($"NIVEL {index + 1}");
            SetScreen(Screen.Playing);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            UnbindScene();
        }

        private void UnbindScene()
        {
            if (levelScore != null) levelScore.ScoreChanged -= OnLevelScoreChanged;
            if (playerHealth != null)
            {
                playerHealth.LivesChanged -= OnLivesChanged;
                playerHealth.Died -= GameOver;
            }

            if (director != null)
            {
                director.InfoChanged -= OnLevelInfo;
                director.Completed -= CompleteLevel;
                director.Failed -= GameOver;
                director.StopLevel();
            }

            levelScore = null;
            playerHealth = null;
            director = null;
            ammoController = null;
            dashController = null;
        }

        private void OnLevelScoreChanged(int score)
        {
            int delta = score - lastScoreValue;
            lastScoreValue = score;
            if (delta > 0)
            {
                GameState.AddScore(delta);
            }
        }

        private void OnLivesChanged(int current, int max)
        {
            GameState.SetLives(current);
        }

        private void OnLevelInfo(string objective, float remaining)
        {
            ui?.SetObjective(objective);
            ui?.SetTimer(remaining);
        }

        private static bool IsLevelScene(string scene)
        {
            return System.Array.IndexOf(GameSceneNames.Levels, scene) >= 0;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            UnbindScene();

            GameInput input = GameInput.Instance;
            if (input != null)
            {
                input.PausePressed -= OnPause;
                input.ConfirmPressed -= OnConfirm;
                input.CancelPressed -= OnCancel;
                input.ShotPressed -= OnShot;
                input.DashPressed -= OnDash;
            }

            Time.timeScale = 1f;
        }
    }
}
