namespace KingdomLike.Interactables
{
    /// <summary>
    /// Optional extension for interactors that support focused interactables.
    /// The interactor owns and manages its own focus state.
    /// </summary>
    public interface IFocusableInteractor : IInteractor
    {
        IInteractable FocusedInteractable { get; }

        void SetFocusedInteractable(IInteractable interactable);
    }
}