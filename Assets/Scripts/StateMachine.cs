using System;

[Serializable]
public class StateMachine<T> where T : StateBase
{
    public T state;
    public virtual void ChangeState(T newState)
    {
        state?.Exit();
        state = newState;
        state?.Enter();
    }
}
[Serializable]
public class StateMachine
{
    public StateBase state;
    public virtual void ChangeState(StateBase newState)
    {
        state?.Exit();
        state = newState;
        state?.Enter();
    }
}
[Serializable]
public class StateBase
{
    public virtual void Enter()
    {

    }
    public virtual void Update()
    {

    }
    public virtual void Exit()
    {

    }
}