namespace KingdomLike.Interactables
{
    /// <summary>
    /// Optional extension for interactors that support focused interaction targets.
    /// The interactor owns and manages its own focus and interaction execution.
    /// </summary>
    public interface IFocusableInteractor : IInteractor
    {
        IInteractionTarget FocusedTarget { get; }

        void SetFocusedTarget(IInteractionTarget target);

        void ClearFocusedTarget(IInteractionTarget target);

        void Interact();
    }
}