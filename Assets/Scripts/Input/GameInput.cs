using UnityEngine;
using UnityEngine.InputSystem;

namespace EmotionsGame.Input
{
    [DisallowMultipleComponent]
    public sealed class GameInput : MonoBehaviour
    {
        public static GameInput Instance { get; private set; }

        public event System.Action PausePressed;
        public event System.Action ConfirmPressed;
        public event System.Action CancelPressed;
        public event System.Action ShotPressed;
        public event System.Action DashPressed;

        public Vector2 Move
        {
            get
            {
                Vector2 value = Vector2.zero;
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value.x -= 1f;
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value.x += 1f;
                    if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value.y -= 1f;
                    if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value.y += 1f;
                }

                Gamepad pad = Gamepad.current;
                if (pad != null)
                {
                    value += pad.leftStick.ReadValue();
                }

                return Vector2.ClampMagnitude(value, 1f);
            }
        }

        public bool ShotHeld
        {
            get
            {
                bool held = Mouse.current != null && Mouse.current.leftButton.isPressed;
                held |= Keyboard.current != null && Keyboard.current.jKey.isPressed;
                held |= Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.35f;
                return held;
            }
        }

        public bool DashHeld
        {
            get
            {
                bool held = Keyboard.current != null && Keyboard.current.kKey.isPressed;
                held |= Gamepad.current != null && Gamepad.current.leftShoulder.isPressed;
                return held;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;

            bool pause = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                         || (pad != null && pad.startButton.wasPressedThisFrame);
            bool confirm = (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
                           || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            bool cancel = (keyboard != null && keyboard.backspaceKey.wasPressedThisFrame)
                          || (pad != null && pad.buttonEast.wasPressedThisFrame);
            bool shot = (keyboard != null && keyboard.jKey.wasPressedThisFrame)
                        || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                        || (pad != null && pad.rightTrigger.wasPressedThisFrame);
            bool dash = (keyboard != null && keyboard.kKey.wasPressedThisFrame)
                        || (pad != null && pad.leftShoulder.wasPressedThisFrame);

            if (pause) PausePressed?.Invoke();
            if (confirm) ConfirmPressed?.Invoke();
            if (cancel) CancelPressed?.Invoke();
            if (shot) ShotPressed?.Invoke();
            if (dash) DashPressed?.Invoke();
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
