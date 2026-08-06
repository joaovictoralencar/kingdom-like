using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KingdomLike.Currency;
using KingdomLike.Currency.Data;
using UnityEngine;

namespace KingdomLike.Core.Currency
{
    public interface ICurrencyPoolManager
    {
        UniTask<CurrencyComponent> SpawnAsync(CurrencyDataSO currencyData, Vector3 position, CancellationToken cancellationToken = default);

        UniTask<CurrencyComponent> SpawnAsync(CurrencyDataSO currencyData, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default);

        UniTask<CurrencyComponent> SpawnAsync(Guid currencyId, Vector3 position, Quaternion rotation = default, CancellationToken cancellationToken = default);

        void ReturnToPool(CurrencyComponent currency);

        bool HasPool(CurrencyDataSO currencyData);

        bool HasPool(Guid currencyId);

        int GetAvailableCount(CurrencyDataSO currencyData);

        int GetAvailableCount(Guid currencyId);
        UniTask<bool> SaveAsync(CancellationToken cancellationToken);
    }
}