using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Scriptable Objects/PlayerMovementConfig")]
public class PlayerMovementConfig : ScriptableObject
{
    [SerializeField] private float _walkSpeed = 20f;
    [SerializeField] private float _crouchSpeed = 7f;
    [SerializeField] private float _walkMovementResponsiveness = 25f;
    [SerializeField] private float _crouchMovementResponsiveness = 20f;
    [Space]
    [SerializeField] private float _airSpeed = 15f;
    [SerializeField] private float _airAcceleration = 70f;
    [Space]
    [SerializeField] private float _jumpForce = 20f;
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _jumpSustainGravity = 0.4f;
    [SerializeField] private float _gravity = -90f;
    [Space]
    [SerializeField] private float _standHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchAnimationSpeed = 15f;
    [SerializeField, Range(0f, 1f)] private float _standCameraTargetHeight = 0.9f;
    [SerializeField, Range(0f, 1f)] private float _crouchCameraTargetHeight = 0.7f;

    public float WalkSpeed => _walkSpeed;
    public float CrouchSpeed => _crouchSpeed;
    public float WalkMovementResponsiveness => _walkMovementResponsiveness;
    public float CrouchMovementResponsiveness => _crouchMovementResponsiveness;
    public float AirSpeed => _airSpeed;
    public float AirAcceleration => _airAcceleration;
    public float JumpForce => _jumpForce;
    public float CoyoteTime => _coyoteTime;
    public float JumpSustainGravity => _jumpSustainGravity;
    public float Gravity => _gravity;
    public float StandHeight => _standHeight;
    public float CrouchHeight => _crouchHeight;
    public float CrouchAnimationSpeed => _crouchAnimationSpeed;
    public float StandCameraTargetHeight => _standCameraTargetHeight;
    public float CrouchCameraTargetHeight => _crouchCameraTargetHeight;
}
