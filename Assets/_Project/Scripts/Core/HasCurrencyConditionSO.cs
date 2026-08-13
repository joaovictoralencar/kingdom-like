using KingdomLike.Core.Components;
using KingdomLike.Interactables;
using UnityEngine;

namespace KingdomLike.Conditions
{
    [CreateAssetMenu(
        fileName = "HasCurrencyCondition",
        menuName = "KingdomLike/Conditions/Interactor/Has Currency")]
    public class HasCurrencyConditionSO : ConditionIInteractorWithCurrencySO
    {
        protected override bool EvaluateContext(InteractorWithCurrency context)
        {
            if (context.Interactor?.InteractorObject == null)
                return false;

            if (context.CurrencyType == null)
                return false;

            GameObject interactorObject = context.Interactor.InteractorObject;
            EntityCurrencyComponent currency = interactorObject.GetComponentInChildren<EntityCurrencyComponent>();

            if (currency == null)
                return false;

            if (currency.CurrencyData != context.CurrencyType)
                return false;

            return currency.Currency.Has(context.RequiredAmount);
        }
    }
}