using KingdomLike.Interactables;
using UnityEngine;
using IInteractor = KingdomLike.Interactables.IInteractor;

namespace KingdomLike.Core
{
    public interface IInteractionCostDisplayer
    {
        Transform UICostDisplayTarget { get; }

        bool TryGetInteractionCost(IInteractor interactor, out ICurrencyCost currencyCost);
    }
}