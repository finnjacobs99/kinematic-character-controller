using KinematicCharacterController;
using System;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 7f;
    [Space]
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField, Range(0f, 1f)] private float standCameraTargetHeight = 0.9f;
    [SerializeField, Range(0f, 1f)] private float crouchCameraTargetHeight = 0.7f;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedCrouch;

    private bool _crouching = false;

    public Transform CameraTarget => cameraTarget;

    public void Initialize()
    {
        motor.CharacterController = this;
    }

    public void UpdateInput(PlayerInput input, Quaternion rotation)
    {
        _requestedRotation = rotation;

        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        _requestedMovement = rotation * _requestedMovement;

        _requestedJump = _requestedJump || input.ConsumeJump();
        _requestedCrouch = input.Crouch;
    }

    public void UpdateBody()
    {
        float currentHeight = motor.Capsule.height;
        float cameraTargetHeight = currentHeight * (_crouching ? crouchCameraTargetHeight : standCameraTargetHeight);
        CameraTarget.localPosition = Vector3.up * cameraTargetHeight;

        float normalizedHeight = currentHeight / standHeight;
        Vector3 rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
        root.localScale = rootTargetScale;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            Vector3 groundedMovement = motor.GetDirectionTangentToSurface(
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            currentVelocity = groundedMovement * walkSpeed;
        }
        else
        {
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if (_requestedJump)
        {
            _requestedJump = false;

            motor.ForceUnground();

            float currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpForce);
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 cameraForward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);
        currentRotation = Quaternion.LookRotation(cameraForward, motor.CharacterUp);
    }

    public void BeforeCharacterUpdate(float deltaTime) 
    {
        if (_requestedCrouch && !_crouching)
        {
            _crouching = true;
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }

    public void AfterCharacterUpdate(float deltaTime) 
    {
        if (_crouching && !_requestedCrouch)
        {
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: standHeight * 0.5f
            );

            int overlaps = motor.CharacterOverlap(
                motor.TransientPosition, 
                motor.TransientRotation, 
                new Collider[8], 
                motor.CollidableLayers, 
                QueryTriggerInteraction.Ignore
            );

            if (overlaps > 0)
            {
                _requestedCrouch = true;
                motor.SetCapsuleDimensions(
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
            else
            {
                _crouching = false;
            }
        }
    }



    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
}
