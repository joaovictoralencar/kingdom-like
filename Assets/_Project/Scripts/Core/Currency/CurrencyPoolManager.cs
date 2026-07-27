using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HelloDev.Loader;
using HelloDev.Saving;
using KingdomLike.Currency;
using KingdomLike.Currency.Data;
using KingdomLike.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = HelloDev.Logging.Logger;

namespace KingdomLike.Core.Currency
{
    public class CurrencyPoolManager : BootstrappedSaveableSystem<CurrencyPoolSnapshot>, ICurrencyPoolManager
    {
        private const string LogId = "Currency.Pool";

        #region Configuration

        [Serializable]
        private class CurrencyPoolConfiguration
        {
            [Required]
            public CurrencyDataSO CurrencyData;

            [Required]
            public AssetReferenceGameObject PrefabReference;

            [MinValue(0)] [BoxGroup("Pool Size")] public int InitialPoolSize = 20;

            [MinValue(0)] [BoxGroup("Pool Size")] [Tooltip("0 means unlimited.")]
            public int MaxPoolSize;
        }

        private class CurrencyPool
        {
            public readonly CurrencyPoolConfiguration Configuration;

            public readonly Queue<CurrencyComponent>
                AvailableCurrencies = new();

            public readonly HashSet<CurrencyComponent>
                AllCurrencies = new();

            public CurrencyPool(
                CurrencyPoolConfiguration configuration)
            {
                Configuration = configuration;
            }
        }

        #endregion

        #region Data

        [FoldoutGroup("Pool Configurations")] [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)] [SerializeField]
        private List<CurrencyPoolConfiguration> _currencyPools = new();

        #endregion

        #region References

        [FoldoutGroup("References")] [Required] [SerializeField]
        private Transform _poolParent;
        
        [FoldoutGroup("References")] [Required] [SerializeField]
        private CurrencyPoolManagerLocatorSO _currencyPoolLocator;

        #endregion

        #region Bootstrap

        [FoldoutGroup("Bootstrap")] [SerializeField]
        private bool _selfInitialize = true;

        #endregion

        #region Runtime

        private readonly Dictionary<Guid, CurrencyPool> _pools = new();

        private readonly Dictionary<CurrencyComponent, CurrencyPool> _currencyToPool = new();

        private readonly HashSet<CurrencyComponent> _activeCurrencies = new();

        #endregion

        #region Properties

        public override string SystemKey => "CurrencyPool";

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (SelfInitialize && !IsInitialized)
            {
                InitializeAsync().Forget();
            }
        }
        
        #endregion
        

        #region Initialization

        public override async UniTask InitializeAsync()
        {
            if (IsInitialized)
                return;

            await Loader.InitializeAsync();

            await InitializePoolsAsync();

            await base.InitializeAsync();

            _currencyPoolLocator.Register(this);

            Logger.LogVerbose(LogId, $"Initialized {_pools.Count} currency pools.", this);
        }

        private async UniTask InitializePoolsAsync()
        {
            _pools.Clear();
            _currencyToPool.Clear();
            _activeCurrencies.Clear();

            foreach (CurrencyPoolConfiguration configuration in _currencyPools)
            {
                if (configuration == null)
                    continue;

                if (configuration.CurrencyData == null)
                {
                    Logger.LogError(LogId, $"Currency data is not assigned on {name}.", this);
                    continue;
                }

                if (configuration.CurrencyData.Id == null)
                {
                    Logger.LogError(LogId, $"Currency {configuration.CurrencyData.name} does not have an ID.", this);

                    continue;
                }

                if (configuration.PrefabReference == null || !configuration.PrefabReference.RuntimeKeyIsValid())
                {
                    Logger.LogError(LogId, $"No valid prefab reference is configured for {configuration.CurrencyData.name}.", this);
                    continue;
                }

                Guid currencyId = configuration.CurrencyData.Id.Id;

                if (_pools.ContainsKey(currencyId))
                {
                    Logger.LogError(LogId, $"Duplicate pool configuration found for {configuration.CurrencyData.name}.", this);
                    continue;
                }

                CurrencyPool pool = new(configuration);

                _pools.Add(currencyId, pool);

                await PrewarmPoolAsync(pool);
            }
        }

        private async UniTask PrewarmPoolAsync(CurrencyPool pool)
        {
            for (int i = 0; i < pool.Configuration.InitialPoolSize; i++)
            {
                CurrencyComponent currency = await CreateCurrencyAsync(pool);
                ReturnToPool(currency);
            }
        }

        #endregion

        #region Spawning

        public async UniTask<CurrencyComponent> SpawnAsync(CurrencyDataSO currencyData, Vector3 position, CancellationToken cancellationToken = default)
        {
            return await SpawnAsync(currencyData, position, Quaternion.identity, cancellationToken);
        }

        public async UniTask<CurrencyComponent> SpawnAsync(CurrencyDataSO currencyData, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
        {
            if (currencyData == null)
            {
                Logger.LogError(LogId, "Cannot spawn currency because the CurrencyDataSO is null.", this);

                return null;
            }

            if (currencyData.Id == null)
            {
                Logger.LogError(LogId, $"Currency {currencyData.name} does not have an ID.", this);
                return null;
            }

            return await SpawnAsync(currencyData.Id.Id, position, rotation, cancellationToken);
        }

        public async UniTask<CurrencyComponent> SpawnAsync(Guid currencyId, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
        {
            if (!_pools.TryGetValue(currencyId, out CurrencyPool pool))
            {
                Logger.LogError(LogId, $"No pool is configured for currency ID {currencyId}.", this);
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            CurrencyComponent currency;

            if (pool.AvailableCurrencies.Count > 0)
            {
                currency = pool.AvailableCurrencies.Dequeue();
            }
            else
            {
                if (!CanCreateNewCurrency(pool))
                {
                    Logger.LogVerbose(LogId, $"Cannot spawn currency {currencyId}. Pool reached its maximum size.", this);
                    return null;
                }

                currency = await CreateCurrencyAsync(pool, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            currency.transform.SetPositionAndRotation(position, rotation);
            currency.gameObject.SetActive(true);
            _activeCurrencies.Add(currency);

            return currency;
        }

        #endregion

        #region Pooling

        public void ReturnToPool(CurrencyComponent currency)
        {
            if (currency == null)
                return;

            if (!_currencyToPool.TryGetValue(currency, out CurrencyPool pool))
            {
                Logger.LogError(LogId, $"Received a currency that does not belong to this pool: {currency.name}.", this);
                return;
            }

            if (pool.AvailableCurrencies.Contains(currency))
                return;

            _activeCurrencies.Remove(currency);

            if (currency.gameObject.activeSelf)
                currency.gameObject.SetActive(false);

            pool.AvailableCurrencies.Enqueue(currency);
        }

        private async UniTask<CurrencyComponent> CreateCurrencyAsync(CurrencyPool pool, CancellationToken cancellationToken = default)
        {
            GameObject currencyGameObject = await Loader.InstantiateAsync(pool.Configuration.PrefabReference, _poolParent.transform).WithCancellation(cancellationToken);
            currencyGameObject.transform.localScale = Vector3.one;

            CurrencyComponent currency = currencyGameObject.GetComponent<CurrencyComponent>();
            currency.gameObject.SetActive(false);

            cancellationToken.ThrowIfCancellationRequested();

            pool.AllCurrencies.Add(currency);
            _currencyToPool.Add(currency, pool);

            return currency;
        }

        private bool CanCreateNewCurrency(CurrencyPool pool)
        {
            int maxPoolSize = pool.Configuration.MaxPoolSize;
            return maxPoolSize <= 0 || pool.AllCurrencies.Count < maxPoolSize;
        }

        #endregion

        #region Queries

        public bool HasPool(CurrencyDataSO currencyData)
        {
            return currencyData != null && currencyData.Id != null && HasPool(currencyData.Id.Id);
        }

        public bool HasPool(Guid currencyId)
        {
            return _pools.ContainsKey(currencyId);
        }

        public int GetAvailableCount(CurrencyDataSO currencyData)
        {
            if (currencyData == null || currencyData.Id == null)
            {
                return 0;
            }

            return GetAvailableCount(currencyData.Id.Id);
        }

        public int GetAvailableCount(Guid currencyId)
        {
            return _pools.TryGetValue(currencyId, out CurrencyPool pool) ? pool.AvailableCurrencies.Count : 0;
        }

        #endregion

        #region Save Lifecycle

        public override void OnBeforeSave()
        {
        }

        public override void OnAfterSave(bool success)
        {
        }

        public override void OnBeforeLoad()
        {
        }

        public override void OnAfterLoad(bool success)
        {
        }

        protected override CurrencyPoolSnapshot Capture()
        {
            CurrencyPoolSnapshot snapshot = new();

            foreach (CurrencyComponent currency in _activeCurrencies)
            {
                if (currency == null || !currency.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (currency.CurrencyData == null || currency.CurrencyData.Id == null)
                {
                    Logger.LogError(LogId, $"Cannot save currency {currency.name} because its CurrencyData or ID is missing.", this);
                    continue;
                }

                snapshot.SpawnedCurrencies.Add(new SpawnedCurrencySnapshot
                {
                    CurrencyId = currency.CurrencyData.Id.Id.ToString(),
                    Position = currency.transform.position,
                    Rotation = currency.transform.rotation
                });
            }

            Logger.LogVerbose(LogId, $"Captured {snapshot.SpawnedCurrencies.Count} active currency instances.", this);
            return snapshot;
        }

        protected override async UniTask<bool> Restore(CurrencyPoolSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Logger.LogError(LogId, "Cannot restore currency pool because the snapshot is null.", this);
                return false;
            }

            foreach (SpawnedCurrencySnapshot savedCurrency in snapshot.SpawnedCurrencies)
            {
                if (!Guid.TryParse(savedCurrency.CurrencyId, out Guid currencyId))
                {
                    Logger.LogError(LogId, $"Cannot restore currency. Invalid currency ID: {savedCurrency.CurrencyId}.", this);
                    continue;
                }

                if (!IDDatabase.TryGet(currencyId, out CurrencyDataSO _))
                {
                    Logger.LogError(LogId, $"Cannot restore currency. No CurrencyDataSO was found for ID {currencyId}.", this);
                    continue;
                }

                await RestoreCurrencyAsync(currencyId, savedCurrency.Position, savedCurrency.Rotation);
            }

            Logger.LogVerbose(LogId, $"Restoring {snapshot.SpawnedCurrencies.Count} currency instances.", this);
            return true;
        }

        private async UniTask RestoreCurrencyAsync(Guid currencyId, Vector3 position, Quaternion rotation)
        {
            CurrencyComponent currency = await SpawnAsync(currencyId, position, rotation);
            if (currency == null)
            {
                Logger.LogError(LogId, $"Failed to restore currency {currencyId}.", this);
            }
        }

        #endregion

        #region Bootstrap

        public override void Shutdown()
        {
            base.Shutdown();
            _activeCurrencies.Clear();
            _currencyToPool.Clear();
            _pools.Clear();
            Logger.LogVerbose(LogId, "Currency pool manager shut down.", this);
        }

        #endregion

        #region Cleanup

        protected override void OnDestroy()
        {
            Shutdown();
            base.OnDestroy();
        }

        #endregion
    }

    [Serializable]
    public class CurrencyPoolSnapshot
    {
        public List<SpawnedCurrencySnapshot> SpawnedCurrencies = new();
    }

    [Serializable]
    public class SpawnedCurrencySnapshot
    {
        public string CurrencyId;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}