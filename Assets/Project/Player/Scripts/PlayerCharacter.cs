using KinematicCharacterController;
using System;
using TMPro.EditorUtilities;
using UnityEngine;

public class PlayerCharacter : StateMachine<PlayerCharacter.PlayerState, PlayerState>, ICharacterController
{
    public enum PlayerState
    {
        Idle,
        Run,
        Crouch
    }

    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private PlayerMovementConfig movementConfig;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;

    public Transform CameraTarget => cameraTarget;

    protected PlayerContext Player;

    #region Init
    public void Initialize(PlayerInput playerInput)
    {
        motor.CharacterController = this;
        Player = new PlayerContext(playerInput, motor, movementConfig);
        InitializeStates(Player);
    }

    private void InitializeStates(PlayerContext Player)
    {
        States.Add(PlayerState.Crouch, new CrouchState(Player, PlayerState.Crouch));
        States.Add(PlayerState.Idle, new IdleState(Player, PlayerState.Idle));
        States.Add(PlayerState.Run, new RunState(Player, PlayerState.Run));

        CurrentState = States[PlayerState.Idle];
        CurrentState.EnterState();
    }
    #endregion

    public void UpdateInput(PlayerInput input, Quaternion rotation)
    {
        Player.RequestedRotation = rotation;

        Player.RequestedMovement = new Vector3(Player.Input.Move.x, 0f, Player.Input.Move.y);
        Player.RequestedMovement = Vector3.ClampMagnitude(Player.RequestedMovement, 1f);
        Player.RequestedMovement = rotation * Player.RequestedMovement;

        bool wasRequestingJump = Player.RequestedJump;
        Player.RequestedJump = Player.RequestedJump || Player.Input.ConsumeJump();
        if (Player.RequestedJump && !wasRequestingJump)
        {
            Player.TimeSinceJumpRequested = 0f;
        }

        Player.RequestedSustainedJump = Player.Input.Jump;
        
        Player.RequestedCrouch = Player.Input.Crouch;
    }

    public void UpdateBody()
    {
        float currentHeight = Player.Motor.Capsule.height;
        Vector3 cameraTargetPosition = Vector3.up * (currentHeight * (Player.Crouching ? Player.Config.CrouchCameraTargetHeight : Player.Config.StandCameraTargetHeight));
        CameraTarget.localPosition = Vector3.Lerp(
            a: CameraTarget.localPosition,
            b: cameraTargetPosition,
            t: 1f - Mathf.Exp(-Player.Config.CrouchAnimationSpeed * Time.deltaTime)
        );

        float normalizedHeight = currentHeight / Player.Config.StandHeight;
        Vector3 rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
        root.localScale = Vector3.Lerp(
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-Player.Config.CrouchAnimationSpeed * Time.deltaTime)
        );
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
    {
        CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);

        if (Player.RequestedJump)
        {
            currentVelocity = HandleJump(currentVelocity, deltaTime);
        }
    }

    #region helper
    private Vector3 HandleGroundMovement(Vector3 currentVelocity, float deltaTime)
    {
        Player.TimeSinceUngrounded = 0f;
        Player.UngrounedDueToJump = false;
        
        Vector3 groundedMovement = Player.Motor.GetDirectionTangentToSurface(
                direction: Player.RequestedMovement,
                surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
        ) * Player.RequestedMovement.magnitude;

        float speed = Player.Crouching ? Player.Config.CrouchSpeed : Player.Config.WalkSpeed;
        float acceleration = Player.Crouching ? Player.Config.CrouchMovementResponsiveness : Player.Config.WalkMovementResponsiveness;

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
        Player.TimeSinceUngrounded += deltaTime;

        // Movement
        if (Player.RequestedMovement.sqrMagnitude > 0f)
        {
            Vector3 movement = Vector3.ProjectOnPlane(
                vector: Player.RequestedMovement,
                planeNormal: Player.Motor.CharacterUp
            ).normalized * Player.RequestedMovement.magnitude;

            Vector3 currentHorizontalVelocity = Vector3.ProjectOnPlane(
                vector: currentVelocity,
                planeNormal: Player.Motor.CharacterUp
            );

            Vector3 acceleration = movement * Player.Config.AirAcceleration * deltaTime;

            Vector3 targetHorizonalVelocity = currentHorizontalVelocity + acceleration;
            targetHorizonalVelocity = Vector3.ClampMagnitude(targetHorizonalVelocity, Player.Config.AirSpeed);

            currentVelocity += targetHorizonalVelocity - currentHorizontalVelocity;
        }

        // Gravity
        float effectiveGravity = Player.Config.Gravity;

        if (Player.RequestedSustainedJump && Vector3.Dot(currentVelocity, Player.Motor.CharacterUp) > 0)
        {
            effectiveGravity *= Player.Config.JumpSustainGravity;
        }

        currentVelocity += Player.Motor.CharacterUp * effectiveGravity * deltaTime;

        return currentVelocity;
    }

    private Vector3 HandleJump(Vector3 currentVelocity, float deltaTime)
    {
        bool grounded = Player.Motor.GroundingStatus.IsStableOnGround;
        bool canCoyoteJump = Player.TimeSinceUngrounded < Player.Config.CoyoteTime && !Player.UngrounedDueToJump;

        if (grounded || canCoyoteJump)
        {
            Player.RequestedJump = false;

            Player.Motor.ForceUnground();
            Player.UngrounedDueToJump = true;

            float currentVerticalSpeed = Vector3.Dot(currentVelocity, Player.Motor.CharacterUp);
            float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, Player.Config.JumpForce);
            currentVelocity += Player.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        else
        {
            Player.TimeSinceJumpRequested += deltaTime;

            bool canJumpLater = Player.TimeSinceJumpRequested < Player.Config.CoyoteTime;
            Player.RequestedJump = canJumpLater;
        }

        return currentVelocity;
    }
    #endregion

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 cameraForward = Vector3.ProjectOnPlane(Player.RequestedRotation * Vector3.forward, Player.Motor.CharacterUp);
        currentRotation = Quaternion.LookRotation(cameraForward, Player.Motor.CharacterUp);
    }


    public void BeforeCharacterUpdate(float deltaTime) 
    {
        if (Player.RequestedCrouch && !Player.Crouching)
        {
            Player.Crouching = true;
            Player.Motor.SetCapsuleDimensions(
                radius: Player.Motor.Capsule.radius,
                height: Player.Config.CrouchHeight,
                yOffset: Player.Config.CrouchHeight * 0.5f
            );
        }
    }

    public void AfterCharacterUpdate(float deltaTime) 
    {
        if (Player.Crouching && !Player.RequestedCrouch)
        {
            Player.Motor.SetCapsuleDimensions(
                radius: Player.Motor.Capsule.radius,
                height: Player.Config.StandHeight,
                yOffset: Player.Config.StandHeight * 0.5f
            );

            int overlaps = Player.Motor.CharacterOverlap(
                Player.Motor.TransientPosition, 
                Player.Motor.TransientRotation, 
                new Collider[8], 
                Player.Motor.CollidableLayers, 
                QueryTriggerInteraction.Ignore
            );

            if (overlaps > 0)
            {
                Player.RequestedCrouch = true;
                Player.Motor.SetCapsuleDimensions(
                    radius: Player.Motor.Capsule.radius,
                    height: Player.Config.CrouchHeight,
                    yOffset: Player.Config.CrouchHeight * 0.5f
                );
            }
            else
            {
                Player.Crouching = false;
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