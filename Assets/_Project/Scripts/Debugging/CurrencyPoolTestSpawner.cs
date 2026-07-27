using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KingdomLike.Core.Currency;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomLike.Debugging
{
    public class CurrencyPoolTestSpawner : MonoBehaviour
    {
        #region References

        [Title("References")] [Required] [SerializeField]
        private CurrencyPoolManager _currencyPoolManager;

        [Required] [SerializeField] private CurrencyDataSO _currencyData;
        
        #endregion

        #region Configuration

        [Title("Spawn Configuration")] [MinValue(0)] [SerializeField]
        private float _radius = 5f;
        
        [MinValue(1)] [SerializeField] private int _currenciesPerPress = 1;

        #endregion

        #region Runtime

        private CancellationToken _destroyCancellationToken;

        private bool _isSpawning;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _destroyCancellationToken = this.GetCancellationTokenOnDestroy();
        }

        #endregion

        #region Input

        private void OnSpawnPerformed(InputAction.CallbackContext context)
        {
            SpawnCurrenciesAsync().Forget();
        }

        #endregion

        #region Spawning

        [Button("Spawn Currencies")]
        private void SpawnCurrenciesFromInspector()
        {
            SpawnCurrenciesAsync().Forget();
        }

        private async UniTask SpawnCurrenciesAsync()
        {
            if (_isSpawning)
                return;

            if (_currencyPoolManager == null)
            {
                Debug.LogError($"[{nameof(CurrencyPoolTestSpawner)}] " +
                               "CurrencyPoolManager is not assigned.",
                    this);

                return;
            }

            if (_currencyData == null)
            {
                Debug.LogError($"[{nameof(CurrencyPoolTestSpawner)}] " +
                               "CurrencyDataSO is not assigned.",
                    this);

                return;
            }

            if (!_currencyPoolManager.IsInitialized)
            {
                Debug.LogWarning($"[{nameof(CurrencyPoolTestSpawner)}] " +
                                 "CurrencyPoolManager is not initialized yet.",
                    this);

                return;
            }

            _isSpawning = true;

            try
            {
                Vector3 center =
                    _currencyPoolManager.transform.position;

                for (int i = 0; i < _currenciesPerPress; i++)
                {
                    _destroyCancellationToken
                        .ThrowIfCancellationRequested();

                    Vector2 randomOffset =
                        UnityEngine.Random.insideUnitCircle * _radius;

                    Vector3 spawnPosition = new(
                        center.x + randomOffset.x,
                        center.y,
                        center.z + randomOffset.y);

                    await _currencyPoolManager.SpawnAsync(
                        _currencyData,
                        spawnPosition,
                        Quaternion.identity,
                        _destroyCancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the object is destroyed.
            }
            finally
            {
                _isSpawning = false;
            }
        }

        #endregion

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                SpawnCurrenciesAsync().Forget();
            }
        }
    }
}