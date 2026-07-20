using System;
using KingdomLike.Currency.Interfaces;

namespace KingdomLike.Currency
{
    [Serializable]
    public abstract class CurrencyFactory
    {
        public abstract ICurrency Create();
    }
}