namespace KingdomLike.Interactables
{
    public interface IInteractionCondition
    {
        bool Evaluate(IInteractor interactor, IInteractable interactable);
    }
}