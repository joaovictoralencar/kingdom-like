namespace KingdomLike.Interactables
{
    public interface IInteractable
    {
        bool CanFocus(IInteractor interactor);
        bool CanInteract(IInteractor interactor);

        void Interact(IInteractor interactor);

        void OnFocus(IInteractor interactor);
        void OnUnfocus(IInteractor interactor);
    }
}