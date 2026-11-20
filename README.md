<p align="center">
  <img src="./assets/readme/hero.svg" width="100%" alt="EmotionsGame: a Unity physics game with two movement dimensions, Cinemachine cameras, and pooled enemy waves">
</p>

# EmotionsGame

A small Unity 6 playground where a smiley sphere rolls across two orthogonal dimensions, flips perspective with Cinemachine, and faces enemy waves that are spawned, animated and recycled without hand-placed enemies.

**Stack:** Unity `6000.6.4f1` · URP `17.6.0` · Cinemachine `6.6.0` · Input System `1.20.0` · DOTween

---

## Start here

1. Open this folder as a project in Unity `6000.6.4f1`.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press **Play**.
4. Move with `WASD` or the left stick; press `Tab` / gamepad **Y** to switch dimension.

The sample `Assets/EnemySystem/EnemySystem.prefab` is ready to drop into a scene and already links a pool, a spawner, an enemy prefab and a basic config.

---

## Controls

| Action | Keyboard | Gamepad |
| :--- | :--- | :--- |
| Move | `W` / `A` / `S` / `D` or arrow keys | Left stick |
| Switch dimension | `Tab` | Button North (`Y` / `Triangle`) |

---

## How the player moves

`Assets/Scripts/RollingSphere.cs` drives a `Rigidbody` with `ContinuousDynamic` collision detection.

- **Dimension A (X axis):** horizontal input moves on X; the camera assigned to `camDimensionA` gets priority `10`.
- **Dimension B (Z axis):** horizontal input moves on Z; the camera assigned to `camDimensionB` gets priority `10`.
- `W` / `S` still adds depth movement inside the active dimension.
- `rotationSpeed` controls how quickly the visual rotates toward `0°` or `90°`.
- `maxSpeed` and `acceleration` control the target velocity and how fast it is reached.

---

## Enemy system

<p align="center">
  <img src="./assets/readme/enemy-system.svg" width="100%" alt="Enemy system flow from config and spawner to pool, controller, animation, motion, targeting, damage and recycling">
</p>

The enemy system is data-driven and pooled. `EnemySpawner` reads `EnemyConfigSO` assets, asks `EnemyPool` for instances, and initializes each `EnemyController`. Animation, movement, target lookup and damage handling stay in separate components. When an enemy is defeated, its tweens and movement coroutine stop and the instance returns to the pool.

### Drop-in setup

1. Drag `Assets/EnemySystem/EnemySystem.prefab` into the scene.
2. Tag the player `Player`, or assign `assignedTarget` on the spawner.
3. Add one or more `EnemyConfigSO` assets to the spawner's `enemyTypes`.
4. Optionally fill `spawnPoints`; otherwise waves use `spawnRadius` around the spawner.
5. Press **Play**. Waves activate when the target is inside `activationDistance`.

### Components

| File | Job |
| :--- | :--- |
| `EnemyConfigSO.cs` | Serialized tuning for health, score, effect, DOTween motion, patrol/chase and proximity activation. Validates values in `OnValidate`. |
| `EnemySpawner.cs` | Coroutine-based waves. Grows wave size by `waveIncrement`, shortens `spawnInterval` per wave, and stops its routine on disable/destroy. |
| `EnemyPool.cs` | Prewarm, get and return. Tracks active enemies in a `HashSet` so instances cannot be returned twice. |
| `EnemyController.cs` | Implements `IDamageable`, tracks health, handles target proximity, applies damage and recycles on defeat. |
| `EnemyTweenAnimator.cs` | Owns the float and rotation `Tween`s; kills them when disabled, destroyed or reused. |
| `EnemyMotion.cs` | Patrol/chase state machine using one active coroutine at a time. |
| `TargetResolver.cs` | Uses an assigned target, a cached target, or `GameObject.FindGameObjectWithTag`. |
| `IDamageable.cs` | Minimal `TakeDamage(float amount)` contract. |

### Main tuning values

- **Combat:** `maxHealth`, `score`, `destructionEffect`.
- **DOTween:** `floatAmplitude`, `floatDuration`, `floatDelay`, `floatEase`; `rotationAxis`, `rotationDuration`, `rotationDelay`, `rotationEase`, `fullTurn`.
- **Movement:** `patrolRadius`, `patrolSpeed`, `chaseSpeed`, `minPause`, `maxPause`, `waypointReachDistance`.
- **Activation:** `detectionRadius`, `releaseRadiusMultiplier`, `activationBehavior` (`Chase` or `HoldPosition`).
- **Waves:** `initialDelay`, `activationDistance`, `enemiesPerWave`, `waveIncrement`, `wavePause`, `spawnInterval`, `minimumSpawnInterval`, `intervalReductionPerWave`, `spawnRadius`.

### Editor smoke check

The project was recompiled with **0 errors and 0 warnings**. In a short Play Mode check with the sample prefab, the pool reached **12 active enemies** and **24 running tweens**; after `TakeDamage(999f)`, **0 enemies remained active** and **0 tweens were running**.

---

## Project layout

```text
Assets/
├── Scenes/
│   └── SampleScene.unity
├── Scripts/
│   ├── RollingSphere.cs
│   └── EnemySystem/
│       ├── EnemyConfigSO.cs
│       ├── EnemyController.cs
│       ├── EnemyMotion.cs
│       ├── EnemyPool.cs
│       ├── EnemySpawner.cs
│       ├── EnemyTweenAnimator.cs
│       ├── IDamageable.cs
│       └── TargetResolver.cs
├── EnemySystem/
│   ├── EnemyConfig_Basic.asset
│   ├── Enemy.prefab
│   └── EnemySystem.prefab
├── Settings/                  # URP assets and volume profiles
├── Textures/                  # Smiley texture and 3D material assets
├── models/                    # Source meshes
├── prefabs/
│   └── player.prefab
├── Plugins/Demigiant/DOTween/ # DOTween runtime and editor assemblies
└── InputSystem_Actions.inputactions
```
