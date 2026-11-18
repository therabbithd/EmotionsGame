# EmotionsGame

A multi-dimensional physics-based Unity project featuring dynamic perspective shifting and emotional mechanics using Unity 6, Universal Render Pipeline (URP), Cinemachine, and the new Input System.

---

## 📂 Project Assets & File Structure

```text
Assets/
├── Scenes/                  # Game scenes
│   └── SampleScene.unity    # Main gameplay scene featuring dimension setups and lighting.
├── Scripts/                 # C# gameplay scripts
│   └── RollingSphere.cs     # Core character controller for multi-dimensional movement.
├── Textures/                # Visual resources, materials and 3D assets
│   ├── materials/           # Project materials (e.g., smileyX2.png)
│   └── texture/             # 3D models and FBX assets (e.g., smileycont.fbx)
├── models/                  # 3D Source meshes and character assets
│   ├── smiley.fbx           # Base smiley character model mesh
│   ├── smiley 1.fbx         # Alternative variation or secondary mesh
│   ├── materials/           # Model materials (Material.004.mat)
│   └── textures/            # Model texture maps (smileyX2.png)
├── prefabs/                 # Reusable game object prefabs
│   └── player.prefab        # Fully configured player entity with Rigidbody and Controller
└── InputSystem_Actions.inputactions # Input System configuration asset for keyboard and gamepad mapping.
```

---

## 📜 C# Scripts & Functions

### `RollingSphere.cs` (`Assets/Scripts/RollingSphere.cs`)
The primary controller script responsible for physics-based sphere movement, dimensional plane switching, dynamic camera priority blending, and rotational alignment.

#### Core Fields & Configuration:
* **`maxSpeed`** (`float`, default: `8f`): Maximum movement velocity for the player.
* **`acceleration`** (`float`, default: `20f`): Rate at which the player accelerates towards target input speed.
* **`camDimensionA`** (`CinemachineCamera`): Cinemachine camera active during Dimension A (X-axis/Lateral view).
* **`camDimensionB`** (`CinemachineCamera`): Cinemachine camera active during Dimension B (Z-axis/Frontal view).
* **`rotationSpeed`** (`float`, default: `10f`): Speed at which the character model smoothly rotates to match the active movement dimension.

#### Functions:
* **`Awake()`**: Initializes the `Rigidbody` reference, forces continuous dynamic collision detection (`CollisionDetectionMode.ContinuousDynamic`), and triggers initial camera priorities.
* **`Update()`**: Polls each frame for dimension switch inputs.
* **`FixedUpdate()`**: Reads keyboard (`WASD` / Arrow Keys) and gamepad (`Left Stick`) inputs, computes movement vectors mapped to the current dimensional plane (`_currentDimension`), applies acceleration/deceleration, and updates `Rigidbody.linearVelocity`.
* **`HandleDimensionSwitch()`**: Detects `Tab` key presses on keyboard or `Button North` on gamepads to toggle between Dimension A (`0`) and Dimension B (`1`), instantly updating camera priorities.
* **`UpdateCameras()`**: Adjusts Cinemachine camera priorities (`10` for active, `0` for inactive) to smoothly transition perspectives between dimensions.
* **`ApplyRotation(float targetYAngle)`**: Smoothly rotates (`Quaternion.Slerp`) the character transform toward the target orientation angle depending on the active dimension axis.

---

## 🕹️ Controls

| Action | Keyboard | Gamepad |
| :--- | :--- | :--- |
| **Move** | `W` / `A` / `S` / `D` or Arrow Keys | Left Stick |
| **Switch Dimension** | `Tab` | Button North (Y / Triangle) |
