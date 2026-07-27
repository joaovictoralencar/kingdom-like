using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KingdomLike.Currency;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Core.Components
{
    public class CurrencyMagnetRadiusComponent : MonoBehaviour
    {
        private const string LogId = "Currency.MagnetRadius";

        #region Settings

        [FoldoutGroup("Settings")] [MinValue(0f)] [SerializeField]
        private float _radius = 3f;

        [FoldoutGroup("Settings")] [MinValue(0.01f)] [SerializeField]
        private float _checkInterval = 0.1f;

        [FoldoutGroup("Settings")] [Required] [SerializeField]
        private LayerMask _currencyLayer;

        #endregion

        #region Data

        [FoldoutGroup("Data")] [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)] [SerializeField]
        private List<CurrencyDataSO> _currencyData = new();

        #endregion

        #region References

        [FoldoutGroup("References")] [Required] [SerializeField]
        private Transform _collectionTarget;
        
        [FoldoutGroup("References")] [Required] [SerializeField]
        private EntityCurrencyComponent _entityCurrencyComponent;

        [FoldoutGroup("Events")] [Required] [SerializeField]
        private CurrencyPoolManagerLocatorSO _currencyPoolManagerLocator;

        #endregion

        #region Runtime

        private readonly HashSet<CurrencyComponent> _detectedCurrencies = new();

        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            CheckForCurrenciesAsync(_cancellationTokenSource.Token).Forget();
        }
        

        private void OnDisable()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _detectedCurrencies.Clear();
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
        #endif
        #endregion

        #region Detection

        private async UniTaskVoid CheckForCurrenciesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                CheckForCurrencies();

                await UniTask.Delay(System.TimeSpan.FromSeconds(_checkInterval), cancellationToken: cancellationToken);
            }
        }

        private void CheckForCurrencies()
        {
            if (_collectionTarget == null)
                return;

            Collider[] colliders = new Collider[50];
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, colliders, _currencyLayer);
            for (int i = 0; i < hitCount; i++)
            {
                CurrencyComponent currency = colliders[i].GetComponentInParent<CurrencyComponent>();

                if (currency == null)
                    continue;

                if (!_detectedCurrencies.Add(currency))
                    continue;

                TryCollect(currency);
            }
        }

        #endregion

        #region Collection

        private void TryCollect(CurrencyComponent currency)
        {
            if (currency == null)
                return;

            if (!currency.IsAvailable)
                return;

            if (!CanCollect(currency))
                return;

            if (currency.StartCollecting(_collectionTarget))
            {
                currency.OnTargetReached -= OnCurrencyTargetReached;
                currency.OnTargetReached += OnCurrencyTargetReached;
            }
        }

        private void OnCurrencyTargetReached(CurrencyComponent currencyComponent)
        {
            _entityCurrencyComponent.Add(currencyComponent.CurrencyData.Amount);
            if (_detectedCurrencies.Contains(currencyComponent)) _detectedCurrencies.Remove(currencyComponent);
            _currencyPoolManagerLocator.Get().ReturnToPool(currencyComponent);
        }

        private bool CanCollect(CurrencyComponent currency)
        {
            if (_currencyData == null || _currencyData.Count == 0)
            {
                return true;
            }

            return _currencyData.Contains(currency.CurrencyData);
        }

        #endregion
    }
}