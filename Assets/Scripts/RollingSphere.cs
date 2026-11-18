// RollingSphere.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class RollingSphere : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 20f;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera camDimensionA;
    [SerializeField] private CinemachineCamera camDimensionB;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody _rb;
    private int _currentDimension = 0; // 0 = X Axis (Lateral), 1 = Z Axis (Frontal)

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        UpdateCameras();
    }

    private void Update()
    {
        HandleDimensionSwitch();
    }

    private void FixedUpdate()
    {
        float inputHorizontal = 0f;
        float inputDepth = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputHorizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputHorizontal += 1f;
            if (Keyboard.current.wKey.isPressed) inputDepth += 1f;
            if (Keyboard.current.sKey.isPressed) inputDepth -= 1f;
        }

        if (Gamepad.current != null)
        {
            inputHorizontal += Gamepad.current.leftStick.x.ReadValue();
            inputDepth += Gamepad.current.leftStick.y.ReadValue();
        }

        inputHorizontal = Mathf.Clamp(inputHorizontal, -1f, 1f);
        inputDepth = Mathf.Clamp(inputDepth, -1f, 1f);

        Vector3 newVelocity = _rb.linearVelocity;

        if (_currentDimension == 0)
        {
            float targetX = inputHorizontal * maxSpeed;
            float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetX, acceleration * Time.fixedDeltaTime);
            newVelocity = new Vector3(newX, _rb.linearVelocity.y, 0f);
            
            ApplyRotation(0f);
        }
        else
        {
            float targetZ = inputHorizontal * maxSpeed;
            float newZ = Mathf.MoveTowards(_rb.linearVelocity.z, targetZ, acceleration * Time.fixedDeltaTime);
            newVelocity = new Vector3(0f, _rb.linearVelocity.y, newZ);

            ApplyRotation(90f);
        }

        // Movimiento en la dimensión inactiva con W/S (profundidad)
        if (_currentDimension == 0)
        {
            float targetZDepth = inputDepth * maxSpeed;
            float newZDepth = Mathf.MoveTowards(_rb.linearVelocity.z, targetZDepth, acceleration * Time.fixedDeltaTime);
            newVelocity = new Vector3(newVelocity.x, _rb.linearVelocity.y, newZDepth);
        }
        else
        {
            float targetXDepth = inputDepth * maxSpeed;
            float newXDepth = Mathf.MoveTowards(_rb.linearVelocity.x, targetXDepth, acceleration * Time.fixedDeltaTime);
            newVelocity = new Vector3(newXDepth, _rb.linearVelocity.y, newVelocity.z);
        }

        _rb.linearVelocity = newVelocity;
    }

    private void HandleDimensionSwitch()
    {
        bool switchPressed = false;

        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            switchPressed = true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame) // Or Fire3 / alternative
        {
            switchPressed = true;
        }

        if (switchPressed)
        {
            _currentDimension = (_currentDimension == 0) ? 1 : 0;
            UpdateCameras();
        }
    }

    private void UpdateCameras()
    {
        if (camDimensionA != null)
            camDimensionA.Priority = (_currentDimension == 0) ? 10 : 0;

        if (camDimensionB != null)
            camDimensionB.Priority = (_currentDimension == 1) ? 10 : 0;
    }

    private void ApplyRotation(float targetYAngle)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, targetYAngle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}
