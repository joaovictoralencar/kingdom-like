using System;
using KingdomLike.Currency.Interfaces;
using UnityEngine;

namespace KingdomLike.Currency.Factories
{
    [Serializable]
    public class StandardCurrencyFactory : CurrencyFactory
    {
        [SerializeField]
        private int _min;

        [SerializeField]
        private int _max = int.MaxValue;

        public override ICurrency Create()
        {
            return new Currency(_min, _max);
        }
    }
}