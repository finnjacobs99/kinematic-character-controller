using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CrouchState : PlayerState
{
    public CrouchState(PlayerContext context, PlayerCharacter.PlayerState key) : base(context, key)
    {
    }

    public override void EnterState()
    {
    }

    public override void ExitState()
    {
    }

    public override PlayerCharacter.PlayerState GetNextState()
    {
        if (!Player.Input.Crouch)
        {
            return Player.Input.Move.sqrMagnitude > 0f
                ? PlayerCharacter.PlayerState.Run
                : PlayerCharacter.PlayerState.Idle;
        }
        
        return StateKey;
    }


    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity = Player.Motor.GroundingStatus.IsStableOnGround
                    ? HandleGroundMovement(currentVelocity, deltaTime)
                    : HandleAirMovement(currentVelocity, deltaTime);
    }

    private Vector3 HandleGroundMovement(Vector3 currentVelocity, float deltaTime)
    {
        Player.TimeSinceUngrounded = 0f;
        Player.UngrounedDueToJump = false;

        Vector3 groundedMovement = Player.Motor.GetDirectionTangentToSurface(
                direction: Player.RequestedMovement,
                surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
        ) * Player.RequestedMovement.magnitude;

        Vector3 targetVelocity = groundedMovement * Player.Config.CrouchSpeed;

        currentVelocity = Vector3.Lerp(
            a: currentVelocity,
            b: targetVelocity,
            t: 1f - Mathf.Exp(-Player.Config.CrouchMovementResponsiveness * deltaTime)
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
}
