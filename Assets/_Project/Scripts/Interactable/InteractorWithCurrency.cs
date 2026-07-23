using KingdomLike.Currency.Data;

namespace KingdomLike.Interactables
{
    public readonly struct InteractorWithCurrency
    {
        public IInteractor Interactor { get; }
        public CurrencyDataSO CurrencyType { get; }
        public int RequiredAmount { get; }

        public InteractorWithCurrency(
            IInteractor interactor,
            CurrencyDataSO currencyType,
            int requiredAmount)
        {
            Interactor = interactor;
            CurrencyType = currencyType;
            RequiredAmount = requiredAmount;
        }
    }
}