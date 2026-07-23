namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base class for interaction conditions that require currency-related context.
    /// </summary>
    public abstract class ConditionIInteractorWithCurrencySO : InteractionCondition_SO<InteractorWithCurrency>
    {
        protected override bool TryCreateContext(
            IInteractor interactor,
            IInteractable interactable,
            out InteractorWithCurrency context)
        {
            if (interactable is not ICurrencyCost currencyCost)
            {
                context = default;
                return false;
            }

            context = new InteractorWithCurrency(
                interactor,
                currencyCost.CurrencyType,
                currencyCost.RequiredAmount);

            return true;
        }
    }
}