using System;
using UnityEngine;

public abstract class BaseState<TState> where TState : Enum
{
    public BaseState(TState key)
    {
        StateKey = key;
    }

    public TState StateKey { get; private set; }

    public abstract void EnterState();
    public abstract TState GetNextState();
    public abstract void ExitState();
}