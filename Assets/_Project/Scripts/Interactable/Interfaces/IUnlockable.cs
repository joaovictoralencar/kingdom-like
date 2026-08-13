namespace KingdomLike.Interactables
{
    /// <summary>
    /// Represents an object that has an unlockable state.
    ///
    /// This interface is intentionally independent from IInteractable.
    /// </summary>
    public interface IUnlockable
    {
        bool IsUnlocked { get; }

        bool CanUnlock(IInteractor interactor);

        bool Unlock(IInteractor interactor);
    }
}