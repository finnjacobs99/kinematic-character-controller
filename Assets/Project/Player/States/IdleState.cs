using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerContext Player, PlayerCharacter.PlayerState key) : base(Player, key)
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
        if (Player.Input.Move.sqrMagnitude > 0)
        {
            return PlayerCharacter.PlayerState.Run;
        }

        if (Player.Input.Crouch)
        {
            return PlayerCharacter.PlayerState.Crouch;
        }

        return StateKey;
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (Player.Motor.GroundingStatus.IsStableOnGround)
        {
            Player.TimeSinceUngrounded = 0f;
            Player.UngrounedDueToJump = false;

            currentVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: Vector3.zero,
                t: 1f - Mathf.Exp(-Player.Config.WalkMovementResponsiveness * deltaTime)
            );
        }
        else
        {
            Player.TimeSinceUngrounded += deltaTime;

            float effectiveGravity = Player.Config.Gravity;

            if (Player.RequestedSustainedJump && Vector3.Dot(currentVelocity, Player.Motor.CharacterUp) > 0)
            {
                effectiveGravity *= Player.Config.JumpSustainGravity;
            }

            currentVelocity += Player.Motor.CharacterUp * effectiveGravity * deltaTime;
        }
    }
}
