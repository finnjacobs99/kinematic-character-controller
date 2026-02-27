using UnityEditor.Build;
using UnityEngine;

public abstract class PlayerState : BaseState<PlayerCharacter.PlayerState>
{
    protected PlayerContext Player;

    public PlayerState(PlayerContext player, PlayerCharacter.PlayerState key) : base(key)
    {
        Player = player;
    }

    public abstract void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);
}