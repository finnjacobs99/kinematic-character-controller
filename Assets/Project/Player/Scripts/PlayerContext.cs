using KinematicCharacterController;
using UnityEngine;

public class PlayerContext
{
    public PlayerInput Input { get; private set; }
    public KinematicCharacterMotor Motor { get; private set; }
    public PlayerMovementConfig Config { get; private set; }

    public Quaternion RequestedRotation { get; set; }
    public Vector3 RequestedMovement { get; set; }

    public bool RequestedJump { get; set; }
    public bool RequestedSustainedJump { get; set; }
    public float TimeSinceUngrounded { get; set; }
    public float TimeSinceJumpRequested { get; set; }
    public bool UngrounedDueToJump { get; set; }

    public bool RequestedCrouch { get; set; }
    public bool Crouching { get; set; }

    public PlayerContext(PlayerInput playerInput, KinematicCharacterMotor motor, PlayerMovementConfig config)
    {
        Input = playerInput;
        Motor = motor;
        Config = config;
    }
}
