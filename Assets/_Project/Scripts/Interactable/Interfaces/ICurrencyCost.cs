using KingdomLike.Currency.Data;

namespace KingdomLike.Interactables
{
    public interface ICurrencyCost
    {
        CurrencyDataSO CurrencyType { get; }
        int RequiredAmount { get; }
    }
}