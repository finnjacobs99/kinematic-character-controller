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
    [SerializeField] private float walkMovementResponsiveness = 25f;
    [SerializeField] private float crouchMovementResponsiveness = 20f;
    [Space]
    [SerializeField] private float airSpeed = 15;
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField, Range(0f, 1f)] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchAnimationSpeed = 15f;
    [SerializeField, Range(0f, 1f)] private float standCameraTargetHeight = 0.9f;
    [SerializeField, Range(0f, 1f)] private float crouchCameraTargetHeight = 0.7f;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequested;
    private bool _ungrounedDueToJump;
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

        bool wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.ConsumeJump();
        if (_requestedJump && !wasRequestingJump)
        {
            _timeSinceJumpRequested = 0f;
        }

        _requestedSustainedJump = input.Jump;
        
        _requestedCrouch = input.Crouch;
    }

    public void UpdateBody()
    {
        float currentHeight = motor.Capsule.height;
        Vector3 cameraTargetPosition = Vector3.up * (currentHeight * (_crouching ? crouchCameraTargetHeight : standCameraTargetHeight));
        CameraTarget.localPosition = Vector3.Lerp(
            a: CameraTarget.localPosition,
            b: cameraTargetPosition,
            t: 1f - Mathf.Exp(-crouchAnimationSpeed * Time.deltaTime)
        );

        float normalizedHeight = currentHeight / standHeight;
        Vector3 rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
        root.localScale = Vector3.Lerp(
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchAnimationSpeed * Time.deltaTime)
        );
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
    {
        currentVelocity = motor.GroundingStatus.IsStableOnGround
            ? HandleGroundMovement(currentVelocity, deltaTime)
            : HandleAirMovement(currentVelocity, deltaTime);

        if (_requestedJump)
        {
            currentVelocity = HandleJump(currentVelocity, deltaTime);
        }
    }

    #region helper
    private Vector3 HandleGroundMovement(Vector3 currentVelocity, float deltaTime)
    {
        _timeSinceUngrounded = 0f;
        _ungrounedDueToJump = false;
        
        Vector3 groundedMovement = motor.GetDirectionTangentToSurface(
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
        ) * _requestedMovement.magnitude;

        float speed = _crouching ? crouchSpeed : walkSpeed;
        float acceleration = _crouching ? crouchMovementResponsiveness : walkMovementResponsiveness;

        Vector3 targetVelocity = groundedMovement * speed;

        currentVelocity = Vector3.Lerp(
            a: currentVelocity,
            b: targetVelocity,
            t: 1f - Mathf.Exp(-acceleration * deltaTime)
        );

        return currentVelocity;
    }

    private Vector3 HandleAirMovement(Vector3 currentVelocity, float deltaTime)
    {
        _timeSinceUngrounded += deltaTime;

        // Movement
        if (_requestedMovement.sqrMagnitude > 0f)
        {
            Vector3 movement = Vector3.ProjectOnPlane(
                vector: _requestedMovement,
                planeNormal: motor.CharacterUp
            ).normalized * _requestedMovement.magnitude;

            Vector3 currentHorizontalVelocity = Vector3.ProjectOnPlane(
                vector: currentVelocity,
                planeNormal: motor.CharacterUp
            );

            Vector3 acceleration = movement * airAcceleration * deltaTime;

            Vector3 targetHorizonalVelocity = currentHorizontalVelocity + acceleration;
            targetHorizonalVelocity = Vector3.ClampMagnitude(targetHorizonalVelocity, airSpeed);

            currentVelocity += targetHorizonalVelocity - currentHorizontalVelocity;
        }

        // Gravity
        float effectiveGravity = gravity;

        if (_requestedSustainedJump && Vector3.Dot(currentVelocity, motor.CharacterUp) > 0)
        {
            effectiveGravity *= jumpSustainGravity;
        }

        currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;

        return currentVelocity;
    }

    private Vector3 HandleJump(Vector3 currentVelocity, float deltaTime)
    {
        bool grounded = motor.GroundingStatus.IsStableOnGround;
        bool canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungrounedDueToJump;

        if (grounded || canCoyoteJump)
        {
            _requestedJump = false;

            motor.ForceUnground();
            _ungrounedDueToJump = true;

            float currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpForce);
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        else
        {
            _timeSinceJumpRequested += deltaTime;

            bool canJumpLater = _timeSinceJumpRequested < coyoteTime;
            _requestedJump = canJumpLater;
        }

        return currentVelocity;
    }
    #endregion

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