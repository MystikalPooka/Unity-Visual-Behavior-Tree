namespace Assets.Scripts.AI
{
    //TODO: Error state? Needed? Useful?
    /// <summary>
    /// Null usually means this behavior has not started or is in an error state.
    /// Fail, Success, and Running are for checking the current state from the parent object.
    /// </summary>
    public enum BehaviorState
    {
        Null = 0,
        Fail,
        Success,
        Running
    } 
}