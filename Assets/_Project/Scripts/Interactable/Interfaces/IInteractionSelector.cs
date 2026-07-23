using System.Collections.Generic;

namespace KingdomLike.Interactables
{
    public interface IInteractionSelector
    {
        IInteractable Select(IInteractor interactor, IReadOnlyList<IInteractable> interactables);
    }
}