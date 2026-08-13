namespace KingdomLike.Interactables
{
    public interface IInteractionFocusReceiver
    {
        void OnInteractionFocus(IInteractor interactor);

        void OnInteractionUnfocus(IInteractor interactor);
    }
}