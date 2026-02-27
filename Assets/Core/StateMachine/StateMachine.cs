using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine<TState, TBaseState> : MonoBehaviour where TState : Enum where TBaseState : BaseState<TState>
{
    protected Dictionary<TState, TBaseState> States = new();
    protected TBaseState CurrentState;
    protected bool IsTransitioningState = false;

    public void UpdateState()
    {
        TState nextStateKey = CurrentState.GetNextState();

        if (nextStateKey.Equals(CurrentState.StateKey))
        {
            return;
        }

        if (!IsTransitioningState)
        {
            TransitionToState(nextStateKey);
        }   
    }

    protected void TransitionToState(TState stateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[stateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
    }
}