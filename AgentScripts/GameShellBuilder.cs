using System;
using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EmotionsGame.Audio;
using EmotionsGame.Core;
using EmotionsGame.Enemies;
using EmotionsGame.Gameplay;
using EmotionsGame.Health;
using EmotionsGame.Levels;
using EmotionsGame.Obstacles;
using EmotionsGame.PowerUps;
using EmotionsGame.Score;
using EmotionsGame.UI;

public static class GameShellBuilder
{
    private const string UiPrefabPath = "Assets/Resources/UI/GameUI.prefab";
    private const string PlayerPrefabPath = "Assets/prefabs/player.prefab";
    private const string EnemySystemPrefabPath = "Assets/EnemySystem/EnemySystem.prefab";
    private const string LifePrefabPath = "Assets/PowerUps/LifePowerUp.prefab";
    private const string AmmoPrefabPath = "Assets/PowerUps/AmmoPowerUp.prefab";

    public static string Build()
    {
        EnsureFolder("Assets/Resources/UI");
        EnsureFolder("Assets/Resources/Materials");
        EnsureFolder("Assets/Levels");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Settings/LevelVolumes");

        BuildInputActions();
        BuildMaterials();
        BuildLevelAssets();
        BuildAmmoPrefab();
        GameObject uiRoot = BuildUiPrefab();
        UnityEngine.Object.DestroyImmediate(uiRoot);

        GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UiPrefabPath);
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        GameObject enemySystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemySystemPrefabPath);
        GameObject lifePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LifePrefabPath);
        GameObject ammoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AmmoPrefabPath);

        if (uiPrefab == null || playerPrefab == null || enemySystemPrefab == null || lifePrefab == null || ammoPrefab == null)
        {
            throw new InvalidOperationException("No se pudieron cargar los prefabs base para construir las escenas.");
        }

        BuildMenuScene(uiPrefab);

        for (int i = 0; i < GameSceneNames.LevelCount; i++)
        {
            BuildLevelScene(i, uiPrefab, playerPrefab, enemySystemPrefab, lifePrefab, ammoPrefab);
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level3.unity", true)
        };

        PlayerSettings.productName = "Emotions Game";
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Game shell construido: UI, 3 niveles, menu, ajustes y Build Settings actualizados.";
    }

    private static void BuildInputActions()
    {
        InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
        InputActionMap map = new InputActionMap("GameShell");
        AddButton(map, "Pause", new[] { "<Keyboard>/escape", "<Gamepad>/start" });
        AddButton(map, "Confirm", new[] { "<Keyboard>/enter", "<Keyboard>/numpadEnter", "<Gamepad>/buttonSouth" });
        AddButton(map, "Cancel", new[] { "<Keyboard>/backspace", "<Gamepad>/buttonEast" });
        AddButton(map, "Shoot", new[] { "<Keyboard>/j", "<Mouse>/leftButton", "<Gamepad>/rightTrigger" });
        AddButton(map, "Dash", new[] { "<Keyboard>/k", "<Gamepad>/leftShoulder" });
        AddButton(map, "Dimension", new[] { "<Keyboard>/tab", "<Gamepad>/buttonNorth" });
        asset.AddActionMap(map);

        string path = "Assets/InputSystem_GameShell.inputactions";
        File.WriteAllText(path, asset.ToJson());
        AssetDatabase.ImportAsset(path);
        UnityEngine.Object.DestroyImmediate(asset);
    }

    private static void AddButton(InputActionMap map, string name, string[] bindings)
    {
        InputAction action = map.AddAction(name, InputActionType.Button, bindings[0]);
        for (int i = 1; i < bindings.Length; i++)
        {
            action.AddBinding(bindings[i]);
        }
    }

    private static void BuildMaterials()
    {
        MakeMaterial("Assets/Resources/Materials/Projectile.mat", new Color(1f, 0.82f, 0.15f), true);
        MakeMaterial("Assets/Materials/Level1Floor.mat", new Color(0.16f, 0.2f, 0.27f), false);
        MakeMaterial("Assets/Materials/Level1Accent.mat", new Color(0.2f, 0.8f, 0.95f), true);
        MakeMaterial("Assets/Materials/Level2Floor.mat", new Color(0.25f, 0.14f, 0.2f), false);
        MakeMaterial("Assets/Materials/Level2Accent.mat", new Color(1f, 0.42f, 0.35f), true);
        MakeMaterial("Assets/Materials/Level3Floor.mat", new Color(0.12f, 0.24f, 0.18f), false);
        MakeMaterial("Assets/Materials/Level3Accent.mat", new Color(0.65f, 1f, 0.42f), true);
        MakeMaterial("Assets/Materials/NeutralWall.mat", new Color(0.32f, 0.34f, 0.4f), false);
    }

    private static void MakeMaterial(string path, Color color, bool unlit)
    {
        AssetDatabase.DeleteAsset(path);
        Shader shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 0.35f);
        AssetDatabase.CreateAsset(material, path);
    }

    private static void BuildLevelAssets()
    {
        CreateLevelConfig("Assets/Levels/Level1Config.asset", "El despertar", "Derrota 6 enemigos para abrir la salida.", 6, 0f,
            new Color(0.16f, 0.2f, 0.27f), new Color(0.2f, 0.8f, 0.95f), new Color(0.05f, 0.07f, 0.11f));
        CreateLevelConfig("Assets/Levels/Level2Config.asset", "Tensión", "Derrota 10 enemigos antes de que se acabe el tiempo.", 10, 95f,
            new Color(0.25f, 0.14f, 0.2f), new Color(1f, 0.42f, 0.35f), new Color(0.12f, 0.04f, 0.07f));
        CreateLevelConfig("Assets/Levels/Level3Config.asset", "Catarsis", "Derrota 14 enemigos y completa la prueba final.", 14, 120f,
            new Color(0.12f, 0.24f, 0.18f), new Color(0.65f, 1f, 0.42f), new Color(0.03f, 0.1f, 0.06f));

        CreateEnemyConfig("Assets/Levels/Enemy_Calm.asset", 40f, 10, 8f, 3.2f, 6f);
        CreateEnemyConfig("Assets/Levels/Enemy_Intense.asset", 65f, 15, 10f, 4f, 8f);
        CreateEnemyConfig("Assets/Levels/Enemy_Chaos.asset", 90f, 20, 12f, 4.8f, 10f);

        CreateVolumeProfile("Assets/Settings/LevelVolumes/Level1Volume.asset", new Color(0.18f, 0.45f, 0.8f), 0.35f, 0.22f);
        CreateVolumeProfile("Assets/Settings/LevelVolumes/Level2Volume.asset", new Color(0.9f, 0.25f, 0.2f), 0.6f, 0.32f);
        CreateVolumeProfile("Assets/Settings/LevelVolumes/Level3Volume.asset", new Color(0.35f, 0.9f, 0.4f), 0.8f, 0.4f);
    }

    private static void CreateLevelConfig(string path, string name, string objective, int defeats, float time, Color floor, Color accent, Color fog)
    {
        AssetDatabase.DeleteAsset(path);
        LevelConfig config = ScriptableObject.CreateInstance<LevelConfig>();
        SetString(config, "displayName", name);
        SetString(config, "objective", objective);
        SetInt(config, "targetDefeats", defeats);
        SetFloat(config, "timeLimit", time);
        SetColor(config, "floorColor", floor);
        SetColor(config, "accentColor", accent);
        SetColor(config, "fogColor", fog);
        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateEnemyConfig(string path, float health, int score, float patrol, float chase, float detection)
    {
        AssetDatabase.DeleteAsset(path);
        EnemyConfigSO config = ScriptableObject.CreateInstance<EnemyConfigSO>();
        SetFloat(config, "maxHealth", health);
        SetInt(config, "score", score);
        SetFloat(config, "patrolRadius", patrol);
        SetFloat(config, "chaseSpeed", chase);
        SetFloat(config, "detectionRadius", detection);
        SetFloat(config, "floatAmplitude", 0.4f);
        SetFloat(config, "rotationDuration", 5f);
        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateVolumeProfile(string path, Color accent, float bloomIntensity, float vignette)
    {
        AssetDatabase.DeleteAsset(path);
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(bloomIntensity);
        bloom.threshold.Override(0.85f);
        bloom.scatter.Override(0.6f);
        AssetDatabase.AddObjectToAsset(bloom, profile);

        Vignette vignetteOverride = profile.Add<Vignette>(true);
        vignetteOverride.intensity.Override(vignette);
        vignetteOverride.smoothness.Override(0.4f);
        AssetDatabase.AddObjectToAsset(vignetteOverride, profile);

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.contrast.Override(10f);
        color.saturation.Override(8f);
        color.colorFilter.Override(Color.Lerp(Color.white, accent, 0.08f));
        AssetDatabase.AddObjectToAsset(color, profile);
        EditorUtility.SetDirty(profile);
    }

    private static void BuildAmmoPrefab()
    {
        AssetDatabase.DeleteAsset(AmmoPrefabPath);
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(LifePrefabPath);
        if (source == null) throw new InvalidOperationException("Falta LifePowerUp.prefab");
        GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        instance.name = "AmmoPowerUp";
        PowerUp powerUp = instance.GetComponent<PowerUp>();
        SetEnum(powerUp, "powerUpType", (int)PowerUpType.Ammo);
        SetInt(powerUp, "amount", 5);
        PrefabUtility.SaveAsPrefabAsset(instance, AmmoPrefabPath);
        UnityEngine.Object.DestroyImmediate(instance);
    }

    private static GameObject BuildUiPrefab()
    {
        AssetDatabase.DeleteAsset(UiPrefabPath);
        GameObject root = new GameObject("GameUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(GameUI));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetParent(root.transform, false);

        CanvasGroup menu = MakePanel(root.transform, "MenuPanel", new Color(0.03f, 0.04f, 0.07f, 0.96f));
        CanvasGroup hud = MakePanel(root.transform, "HudPanel", new Color(0f, 0f, 0f, 0f));
        CanvasGroup pause = MakePanel(root.transform, "PausePanel", new Color(0.02f, 0.02f, 0.04f, 0.85f));
        CanvasGroup settings = MakePanel(root.transform, "SettingsPanel", new Color(0.02f, 0.04f, 0.05f, 0.92f));
        CanvasGroup credits = MakePanel(root.transform, "CreditsPanel", new Color(0.03f, 0.03f, 0.06f, 0.92f));
        CanvasGroup end = MakePanel(root.transform, "EndPanel", new Color(0.02f, 0.02f, 0.03f, 0.93f));
        CanvasGroup transition = MakePanel(root.transform, "TransitionPanel", new Color(0f, 0f, 0f, 0.92f));

        Text title = MakeText(menu.transform, "Title", "EMOTIONS GAME", 86, new Vector2(0.5f, 0.78f), new Vector2(1000f, 140f), TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f));
        title.fontStyle = FontStyle.Bold;
        MakeText(menu.transform, "Subtitle", "Una aventura de emociones en tres actos", 30, new Vector2(0.5f, 0.68f), new Vector2(900f, 60f), TextAnchor.MiddleCenter, new Color(0.8f, 0.86f, 1f));
        Text menuBest = MakeText(menu.transform, "Best", "Mejor puntuación: 0", 28, new Vector2(0.5f, 0.58f), new Vector2(700f, 50f), TextAnchor.MiddleCenter, Color.white);
        Text menuContinue = MakeText(menu.transform, "ContinueLabel", "Continuar", 24, new Vector2(0.5f, 0.52f), new Vector2(700f, 44f), TextAnchor.MiddleCenter, new Color(0.75f, 0.85f, 1f));

        Button newGame = MakeButton(menu.transform, "NewGameButton", "NUEVA PARTIDA", new Vector2(0.5f, 0.42f));
        Button continueGame = MakeButton(menu.transform, "ContinueButton", "CONTINUAR", new Vector2(0.5f, 0.34f));
        Button settingsButton = MakeButton(menu.transform, "SettingsButton", "AJUSTES", new Vector2(0.5f, 0.26f));
        Button creditsButton = MakeButton(menu.transform, "CreditsButton", "CRÉDITOS", new Vector2(0.5f, 0.18f));
        Button quitButton = MakeButton(menu.transform, "QuitButton", "SALIR", new Vector2(0.5f, 0.1f));

        Text levelText = MakeText(hud.transform, "Level", "Nivel 1 / 3", 28, new Vector2(0.03f, 0.94f), new Vector2(420f, 50f), TextAnchor.MiddleLeft, Color.white);
        Text scoreText = MakeText(hud.transform, "Score", "Puntos: 0", 28, new Vector2(0.03f, 0.89f), new Vector2(420f, 50f), TextAnchor.MiddleLeft, Color.white);
        Text livesText = MakeText(hud.transform, "Lives", "Vidas: 3", 28, new Vector2(0.03f, 0.84f), new Vector2(420f, 50f), TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.6f));
        Text ammoText = MakeText(hud.transform, "Ammo", "Munición: 0", 28, new Vector2(0.97f, 0.94f), new Vector2(420f, 50f), TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.2f));
        Text objectiveText = MakeText(hud.transform, "Objective", "Objetivo", 26, new Vector2(0.5f, 0.96f), new Vector2(900f, 70f), TextAnchor.UpperCenter, new Color(0.8f, 0.95f, 1f));
        Text timerText = MakeText(hud.transform, "Timer", "", 26, new Vector2(0.97f, 0.89f), new Vector2(420f, 50f), TextAnchor.MiddleRight, Color.white);
        MakeText(hud.transform, "Controls", "WASD mover · Tab dimensión · J disparar · K dash · Esc pausa", 20, new Vector2(0.5f, 0.035f), new Vector2(1200f, 40f), TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.65f));

        MakeText(pause.transform, "PauseTitle", "PAUSA", 70, new Vector2(0.5f, 0.78f), new Vector2(700f, 110f), TextAnchor.MiddleCenter, Color.white);
        Button resume = MakeButton(pause.transform, "ResumeButton", "REANUDAR", new Vector2(0.5f, 0.55f));
        Button pauseRestart = MakeButton(pause.transform, "PauseRestartButton", "REINICIAR NIVEL", new Vector2(0.5f, 0.45f));
        Button pauseSettings = MakeButton(pause.transform, "PauseSettingsButton", "AJUSTES", new Vector2(0.5f, 0.35f));
        Button pauseMenu = MakeButton(pause.transform, "PauseMenuButton", "MENÚ PRINCIPAL", new Vector2(0.5f, 0.25f));

        MakeText(settings.transform, "SettingsTitle", "AJUSTES", 70, new Vector2(0.5f, 0.8f), new Vector2(700f, 110f), TextAnchor.MiddleCenter, Color.white);
        Text sfxValue = MakeText(settings.transform, "SfxValue", "Efectos: 80%", 32, new Vector2(0.5f, 0.64f), new Vector2(600f, 60f), TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f));
        Button sfxMinus = MakeButton(settings.transform, "SfxMinus", "-", new Vector2(0.42f, 0.54f), new Vector2(100f, 70f));
        Button sfxPlus = MakeButton(settings.transform, "SfxPlus", "+", new Vector2(0.58f, 0.54f), new Vector2(100f, 70f));
        Button reset = MakeButton(settings.transform, "ResetButton", "BORRAR PROGRESO", new Vector2(0.5f, 0.4f));
        Button settingsBack = MakeButton(settings.transform, "SettingsBack", "VOLVER", new Vector2(0.5f, 0.25f));

        MakeText(credits.transform, "CreditsTitle", "CRÉDITOS", 70, new Vector2(0.5f, 0.82f), new Vector2(700f, 110f), TextAnchor.MiddleCenter, Color.white);
        Text creditsText = MakeText(credits.transform, "CreditsBody", "", 30, new Vector2(0.5f, 0.55f), new Vector2(1000f, 300f), TextAnchor.MiddleCenter, new Color(0.9f, 0.94f, 1f));
        Button creditsBack = MakeButton(credits.transform, "CreditsBack", "VOLVER", new Vector2(0.5f, 0.25f));

        Text endTitle = MakeText(end.transform, "EndTitle", "NIVEL SUPERADO", 72, new Vector2(0.5f, 0.78f), new Vector2(1100f, 120f), TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f));
        Text endDetails = MakeText(end.transform, "EndDetails", "", 30, new Vector2(0.5f, 0.58f), new Vector2(1000f, 220f), TextAnchor.MiddleCenter, Color.white);
        Button endNext = MakeButton(end.transform, "EndNext", "SIGUIENTE NIVEL", new Vector2(0.5f, 0.35f));
        Button endRetry = MakeButton(end.transform, "EndRetry", "REINTENTAR", new Vector2(0.5f, 0.25f));
        Button endMenu = MakeButton(end.transform, "EndMenu", "MENÚ PRINCIPAL", new Vector2(0.5f, 0.15f));

        Text transitionText = MakeText(transition.transform, "TransitionText", "NIVEL 1", 64, new Vector2(0.5f, 0.5f), new Vector2(900f, 120f), TextAnchor.MiddleCenter, Color.white);
        transitionText.fontStyle = FontStyle.Bold;

        GameUI ui = root.GetComponent<GameUI>();
        SetObject(ui, "menuPanel", menu);
        SetObject(ui, "hudPanel", hud);
        SetObject(ui, "pausePanel", pause);
        SetObject(ui, "settingsPanel", settings);
        SetObject(ui, "creditsPanel", credits);
        SetObject(ui, "endPanel", end);
        SetObject(ui, "transitionPanel", transition);
        SetObject(ui, "menuBestText", menuBest);
        SetObject(ui, "menuContinueText", menuContinue);
        SetObject(ui, "newGameButton", newGame);
        SetObject(ui, "continueButton", continueGame);
        SetObject(ui, "settingsButton", settingsButton);
        SetObject(ui, "creditsButton", creditsButton);
        SetObject(ui, "quitButton", quitButton);
        SetObject(ui, "levelText", levelText);
        SetObject(ui, "scoreText", scoreText);
        SetObject(ui, "livesText", livesText);
        SetObject(ui, "ammoText", ammoText);
        SetObject(ui, "objectiveText", objectiveText);
        SetObject(ui, "timerText", timerText);
        SetObject(ui, "resumeButton", resume);
        SetObject(ui, "pauseRestartButton", pauseRestart);
        SetObject(ui, "pauseMenuButton", pauseMenu);
        SetObject(ui, "pauseSettingsButton", pauseSettings);
        SetObject(ui, "sfxValueText", sfxValue);
        SetObject(ui, "sfxMinusButton", sfxMinus);
        SetObject(ui, "sfxPlusButton", sfxPlus);
        SetObject(ui, "resetButton", reset);
        SetObject(ui, "settingsBackButton", settingsBack);
        SetObject(ui, "creditsText", creditsText);
        SetObject(ui, "creditsBackButton", creditsBack);
        SetObject(ui, "endTitleText", endTitle);
        SetObject(ui, "endDetailsText", endDetails);
        SetObject(ui, "endNextButton", endNext);
        SetObject(ui, "endRetryButton", endRetry);
        SetObject(ui, "endMenuButton", endMenu);
        SetObject(ui, "transitionText", transitionText);

        PrefabUtility.SaveAsPrefabAsset(root, UiPrefabPath);
        return root;
    }

    private static CanvasGroup MakePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        FullRect(panel.GetComponent<RectTransform>());
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = color.a > 0.01f;
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.alpha = name == "HudPanel" ? 1f : 0f;
        group.interactable = group.alpha > 0f;
        group.blocksRaycasts = group.alpha > 0f;
        return group;
    }

    private static Text MakeText(Transform parent, string name, string content, int size, Vector2 anchor, Vector2 dimensions, TextAnchor alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        Place(rect, anchor, dimensions);
        Text text = go.GetComponent<Text>();
        text.font = UiFont();
        text.text = content;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2? dimensions = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        Place(rect, anchor, dimensions ?? new Vector2(420f, 64f));
        Image image = go.GetComponent<Image>();
        image.color = new Color(0.12f, 0.18f, 0.28f, 0.96f);
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.7f, 0.8f, 1f);
        colors.selectedColor = new Color(1f, 0.9f, 0.5f);
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Text text = MakeText(go.transform, "Label", label, 26, new Vector2(0.5f, 0.5f), new Vector2(400f, 56f), TextAnchor.MiddleCenter, Color.white);
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    private static Font UiFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    private static void BuildMenuScene(GameObject uiPrefab)
    {
        Scene scene = CreateCleanScene("MainMenu");
        MakeCamera(new Color(0.03f, 0.04f, 0.08f), false);
        MakeLight();
        PrefabUtility.InstantiatePrefab(uiPrefab);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    private static void BuildLevelScene(int index, GameObject uiPrefab, GameObject playerPrefab, GameObject enemyPrefab, GameObject lifePrefab, GameObject ammoPrefab)
    {
        string sceneName = GameSceneNames.Levels[index];
        Scene scene = CreateCleanScene(sceneName);

        LevelConfig config = AssetDatabase.LoadAssetAtPath<LevelConfig>($"Assets/Levels/Level{index + 1}Config.asset");
        EnemyConfigSO enemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfigSO>($"Assets/Levels/Enemy_{(index == 0 ? "Calm" : index == 1 ? "Intense" : "Chaos")}.asset");
        Material floorMaterial = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Level{index + 1}Floor.mat");
        Material accentMaterial = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Level{index + 1}Accent.mat");
        VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>($"Assets/Settings/LevelVolumes/Level{index + 1}Volume.asset");

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = config.FogColor;
        RenderSettings.fogDensity = 0.012f + index * 0.003f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(config.FogColor, Color.white, 0.28f);

        MakeCamera(config.FogColor, true);
        MakeLight();

        GameObject volumeObject = new GameObject("Global Volume", typeof(Volume));
        Volume volume = volumeObject.GetComponent<Volume>();
        volume.isGlobal = true;
        volume.profile = volumeProfile;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(4f, 1f, 4f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
        floor.isStatic = true;

        Material wallMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeutralWall.mat");
        MakeWall("Wall North", new Vector3(0f, 1f, 20f), new Vector3(40f, 2f, 0.5f), wallMaterial);
        MakeWall("Wall South", new Vector3(0f, 1f, -20f), new Vector3(40f, 2f, 0.5f), wallMaterial);
        MakeWall("Wall East", new Vector3(20f, 1f, 0f), new Vector3(0.5f, 2f, 40f), wallMaterial);
        MakeWall("Wall West", new Vector3(-20f, 1f, 0f), new Vector3(0.5f, 2f, 40f), wallMaterial);

        GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.2f, 0f);
        RollingSphere rolling = player.AddComponent<RollingSphere>();
        player.AddComponent<AmmoController>();
        player.AddComponent<DashController>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        SetObject(health, "movement", rolling);

        CinemachineCamera cameraA = MakeVirtualCamera("Camera - Lateral", player.transform, new Vector3(0f, 11f, -15f), 10);
        CinemachineCamera cameraB = MakeVirtualCamera("Camera - Frontal", player.transform, new Vector3(15f, 11f, 0f), 0);
        SetObject(rolling, "camDimensionA", cameraA);
        SetObject(rolling, "camDimensionB", cameraB);

        GameObject enemies = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
        EnemySpawner spawner = enemies.GetComponentInChildren<EnemySpawner>();
        SetObject(spawner, "assignedTarget", player.transform);
        SetObjectArray(spawner, "enemyTypes", new UnityEngine.Object[] { enemyConfig });
        SetInt(spawner, "enemiesPerWave", 3 + index);
        SetFloat(spawner, "wavePause", Mathf.Max(2f, 5f - index));
        SetFloat(spawner, "initialDelay", 1.5f);
        SetFloat(spawner, "activationDistance", 35f);

        MakePickup(lifePrefab, new Vector3(-8f, 1f, 6f));
        MakePickup(ammoPrefab, new Vector3(8f, 1f, 6f));
        if (index > 0) MakePickup(ammoPrefab, new Vector3(-4f, 1f, -8f));

        MakeObstacle("Obstacle Rotating", new Vector3(-5f, 1f, 2f), accentMaterial, index == 0 ? 0 : 1);
        MakeObstacle("Obstacle Moving", new Vector3(5f, 1f, -3f), accentMaterial, 2);
        if (index == 2) MakeObstacle("Obstacle Scaling", new Vector3(0f, 1f, -10f), accentMaterial, 3);

        GameObject directorObject = new GameObject("Level Director", typeof(LevelDirector));
        LevelDirector director = directorObject.GetComponent<LevelDirector>();
        SetString(director, "displayName", config.DisplayName);
        SetString(director, "objective", config.Objective);
        SetInt(director, "targetDefeats", config.TargetDefeats);
        SetFloat(director, "timeLimit", config.TimeLimit);
        SetBool(director, "failOnTimeout", index > 0);

        GameObject scoreObject = new GameObject("Score Manager", typeof(ScoreManager));
        scoreObject.name = "ScoreManager";

        PrefabUtility.InstantiatePrefab(uiPrefab);
        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{sceneName}.unity");
    }

    private static Scene CreateCleanScene(string name)
    {
        for (int i = EditorSceneManager.sceneCount - 1; i >= 0; i--)
        {
            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneAt(i), true);
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = name;
        SceneManager.SetActiveScene(scene);
        return scene;
    }

    private static void MakeCamera(Color background, bool brain)
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 12f, -16f);
        cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        if (brain) cameraObject.AddComponent<CinemachineBrain>();
    }

    private static void MakeLight()
    {
        GameObject lightObject = new GameObject("Directional Light", typeof(Light));
        lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        light.color = new Color(1f, 0.94f, 0.86f);
        light.shadows = LightShadows.Soft;
    }

    private static void MakeWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = material;
        wall.isStatic = true;
    }

    private static CinemachineCamera MakeVirtualCamera(string name, Transform target, Vector3 position, int priority)
    {
        GameObject go = new GameObject(name, typeof(CinemachineCamera));
        go.transform.position = position;
        CinemachineCamera camera = go.GetComponent<CinemachineCamera>();
        camera.Follow = target;
        camera.LookAt = target;
        SerializedObject so = new SerializedObject(camera);
        SerializedProperty priorityEnabled = so.FindProperty("Priority.Enabled");
        if (priorityEnabled != null) priorityEnabled.boolValue = true;
        SerializedProperty priorityProperty = so.FindProperty("Priority.m_Value");
        if (priorityProperty != null) priorityProperty.intValue = priority;
        so.ApplyModifiedPropertiesWithoutUndo();
        return camera;
    }

    private static void MakePickup(GameObject prefab, Vector3 position)
    {
        GameObject pickup = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        pickup.transform.position = position;
    }

    private static void MakeObstacle(string name, Vector3 position, Material material, int behavior)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = name;
        obstacle.transform.position = position;
        obstacle.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        obstacle.GetComponent<Renderer>().sharedMaterial = material;

        if (behavior == 0 || behavior == 1)
        {
            RotatingObstacle rotating = obstacle.AddComponent<RotatingObstacle>();
            SetVector(rotating, "rotationEuler", behavior == 0 ? new Vector3(0f, 360f, 0f) : new Vector3(180f, 360f, 90f));
            SetFloat(rotating, "duration", behavior == 0 ? 4f : 2.4f);
        }
        else if (behavior == 2)
        {
            MovingObstacle moving = obstacle.AddComponent<MovingObstacle>();
            SetVector(moving, "movement", new Vector3(0f, 0f, 6f));
            SetFloat(moving, "duration", 2.2f);
        }
        else
        {
            ScalingObstacle scaling = obstacle.AddComponent<ScalingObstacle>();
            SetVector(scaling, "targetScale", new Vector3(2.6f, 2.6f, 2.6f));
            SetFloat(scaling, "duration", 1.4f);
        }

        DamageDealer damage = obstacle.AddComponent<DamageDealer>();
        SetInt(damage, "damage", 1);
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void FullRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
    }

    private static SerializedProperty Prop(UnityEngine.Object target, string field)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        return property;
    }

    private static void SetObject(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(UnityEngine.Object target, string field, string value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(UnityEngine.Object target, string field, int value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(UnityEngine.Object target, string field, float value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(UnityEngine.Object target, string field, bool value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetColor(UnityEngine.Object target, string field, Color value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.colorValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector(UnityEngine.Object target, string field, Vector3 value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.vector3Value = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(UnityEngine.Object target, string field, int value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null) throw new InvalidOperationException($"Campo '{field}' no encontrado en {target.GetType().Name}");
        property.enumValueIndex = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
