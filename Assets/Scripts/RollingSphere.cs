// RollingSphere.cs
using System;
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
    [SerializeField] private float faceZAngle = 75f;

    private Rigidbody _rb;
    private int _currentDimension = 0; // 0 = X Axis (Lateral), 1 = Z Axis (Frontal)
    private bool movementEnabled = true;

    public static event Action<int> DimensionChanged;

    public static int ActiveDimension { get; private set; }

    public int CurrentDimension => _currentDimension;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        transform.rotation = Quaternion.Euler(0f, _currentDimension == 0 ? 0f : 90f, faceZAngle);
        ActiveDimension = _currentDimension;
        UpdateCameras();
        DimensionChanged?.Invoke(_currentDimension);
    }

    /// <summary>Enables or disables player control without changing the Rigidbody setup.</summary>
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled && _rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!movementEnabled)
        {
            return;
        }

        HandleDimensionSwitch();
    }

    private void FixedUpdate()
{
    if (!movementEnabled)
    {
        return;
    }

    float inputHorizontal = 0f;

    if (Keyboard.current != null)
    {
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputHorizontal -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputHorizontal += 1f;
    }

    if (Gamepad.current != null)
    {
        inputHorizontal += Gamepad.current.leftStick.x.ReadValue();
    }

    inputHorizontal = Mathf.Clamp(inputHorizontal, -1f, 1f);

    Vector3 newVelocity;

    if (_currentDimension == 0) // Vista A -> solo eje X
    {
        float targetX = inputHorizontal * maxSpeed;
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetX, acceleration * Time.fixedDeltaTime);
        newVelocity = new Vector3(newX, _rb.linearVelocity.y, 0f);

        ApplyRotation(90f);
    }
    else // Vista B -> solo eje Z
    {
        float targetZ = inputHorizontal * maxSpeed;
        float newZ = Mathf.MoveTowards(_rb.linearVelocity.z, targetZ, acceleration * Time.fixedDeltaTime);
        newVelocity = new Vector3(0f, _rb.linearVelocity.y, newZ);

        ApplyRotation(0f);
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
            ActiveDimension = _currentDimension;
            UpdateCameras();
            DimensionChanged?.Invoke(_currentDimension);
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
        Quaternion targetRotation = Quaternion.Euler(0f, targetYAngle, faceZAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}
