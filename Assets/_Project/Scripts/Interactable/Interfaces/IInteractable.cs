namespace KingdomLike.Interactables
{
    /// <summary>
    /// Represents the actual gameplay action performed by an object
    /// after any required unlocking has been completed.
    ///
    /// This interface is intentionally independent from IUnlockable.
    /// </summary>
    public interface IInteractable
    {
        bool CanInteract(IInteractor interactor);

        bool ExecuteInteraction(IInteractor interactor);
    }
}