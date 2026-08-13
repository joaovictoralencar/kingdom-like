using System.Collections.Generic;

namespace KingdomLike.Interactables
{
    public interface IInteractionSelector
    {
        IInteractionTarget Select(IInteractor interactor, IReadOnlyList<IInteractionTarget> targets);
    }
}