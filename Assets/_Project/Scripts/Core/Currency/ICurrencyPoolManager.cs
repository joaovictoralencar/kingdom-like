using System.Threading;
using Cysharp.Threading.Tasks;
using KingdomLike.Currency;
using KingdomLike.Currency.Data;
using UnityEngine;

namespace KingdomLike.Core.Currency
{
    public interface ICurrencyPoolManager
    {
        UniTask<CurrencyComponent> SpawnAsync(CurrencyDataSO currencyData, Vector3 position, Quaternion rotation = default, CancellationToken cancellationToken = default);

        void ReturnToPool(CurrencyComponent currency);

        bool HasPool(CurrencyDataSO currencyData);

        int GetAvailableCount(CurrencyDataSO currencyData);
    }
}
