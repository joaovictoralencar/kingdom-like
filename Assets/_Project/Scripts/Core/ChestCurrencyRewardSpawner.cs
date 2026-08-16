using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KingdomLike.Core;
using KingdomLike.Currency;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Spawns the chest's reward when the chest becomes unlocked.
    /// </summary>
    [RequireComponent(typeof(ChestInteractable))]
    public class ChestCurrencyRewardSpawner : MonoBehaviour
    {
        #region References

        [Title("References")]
        [Required]
        [SerializeField]
        private ChestInteractable _chest;

        [Required]
        [SerializeField]
        private CurrencyPoolManagerLocatorSO _currencyPoolManagerLocator;

        #endregion

        #region Spawn Configuration

        [Title("Spawn Configuration")]
        [MinValue(0)]
        [SerializeField]
        private float _radius = 2f;

        #endregion

        #region Ground Detection

        [Title("Ground Detection")]
        [SerializeField]
        private LayerMask _groundLayerMask;

        [MinValue(0)]
        [SerializeField]
        private float _raycastHeight = 5f;

        [MinValue(0)]
        [SerializeField]
        private float _raycastDistance = 20f;

        #endregion

        #region Arc Animation

        [Title("Arc Animation")]
        [MinValue(0)]
        [SerializeField]
        private float _arcDuration = 0.6f;

        [MinValue(0)]
        [SerializeField]
        private float _arcHeight = 2f;

        [SerializeField]
        private Ease _ease = Ease.OutQuad;

        [MinValue(0)]
        [SerializeField]
        private float _delayBetweenSpawns = 0.03f;

        #endregion

        private CancellationToken _destroyCancellationToken;

        private void Reset()
        {
            TryGetComponent(out _chest);
        }

        private void Awake()
        {
            _destroyCancellationToken =
                this.GetCancellationTokenOnDestroy();

            if (_chest == null)
                TryGetComponent(out _chest);
        }

        private void OnEnable()
        {
            if (_chest != null)
                _chest.OnUnlockedStateChanged += HandleChestUnlocked;
        }

        private void OnDisable()
        {
            if (_chest != null)
                _chest.OnUnlockedStateChanged -= HandleChestUnlocked;
        }

        private void HandleChestUnlocked(bool unlocked)
        {
            if (!unlocked)
                return;

            SpawnRewardAsync().Forget();
        }

        private async UniTask SpawnRewardAsync()
        {
            if (_currencyPoolManagerLocator == null ||
                _chest == null ||
                _chest.CurrencyType == null)
            {
                Debug.LogError(
                    $"[{nameof(ChestCurrencyRewardSpawner)}] Missing CurrencyPoolManager or reward CurrencyDataSO.",
                    this);

                return;
            }

            Vector3 originPosition = transform.position;

            for (int i = 0; i < _chest.RewardAmount; i++)
            {
                _destroyCancellationToken.ThrowIfCancellationRequested();

                Vector2 randomOffset =
                    UnityEngine.Random.insideUnitCircle * _radius;

                Vector3 candidatePoint = new(
                    originPosition.x + randomOffset.x,
                    originPosition.y,
                    originPosition.z + randomOffset.y);

                Vector3 targetPosition =
                    ResolveGroundPosition(
                        candidatePoint,
                        originPosition.y);

                CurrencyComponent currency =
                    await _currencyPoolManagerLocator
                        .Get()
                        .SpawnAsync(
                            _chest.CurrencyType,
                            originPosition,
                            Quaternion.identity,
                            _destroyCancellationToken);

                if (currency == null)
                    continue;

                _ = CurrencyArcMotion.PlayArc(
                    currency.transform,
                    originPosition,
                    targetPosition,
                    _arcDuration,
                    _arcHeight,
                    _ease);

                if (_delayBetweenSpawns > 0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_delayBetweenSpawns),
                        cancellationToken:
                        _destroyCancellationToken);
                }
            }

            await _currencyPoolManagerLocator
                .Get()
                .SaveAsync(_destroyCancellationToken);
        }

        private Vector3 ResolveGroundPosition(
            Vector3 candidatePoint,
            float fallbackY)
        {
            Vector3 rayOrigin =
                candidatePoint +
                Vector3.up * _raycastHeight;

            if (Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    _raycastDistance,
                    _groundLayerMask))
            {
                return hit.point;
            }

            return new Vector3(candidatePoint.x, fallbackY, candidatePoint.z);
        }
    }
}