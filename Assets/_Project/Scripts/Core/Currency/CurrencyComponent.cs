using System;
using KingdomLike.Core;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Currency
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MoveToTarget))]
    public class CurrencyComponent : MonoBehaviour
    {
        public enum CurrencyState
        {
            Available,
            Collecting,
            ReachedTarget,
            Collected
        }

        private const string LogId = "Currency.Component";

        #region Settings

        [FoldoutGroup("Settings")]
        [MinValue(1)]
        [SerializeField]
        private int _amount = 1;

        [FoldoutGroup("Settings")]
        [Tooltip("Layers considered ground when snapping the currency down after a failed collection.")]
        [SerializeField]
        private LayerMask _groundMask;

        [FoldoutGroup("Settings")]
        [MinValue(0f)]
        [SerializeField]
        private float _groundRaycastDistance = 5f;

        #endregion

        #region Data

        [FoldoutGroup("Data")]
        [Required]
        [SerializeField]
        private CurrencyDataSO _currencyData;

        #endregion

        #region References

        [FoldoutGroup("References")]
        [Required]
        [SerializeField]
        private MoveToTarget _moveToTarget;

        [FoldoutGroup("References")]
        [SerializeField]
        private GameObject _visual;

        #endregion

        #region Runtime

        private Collider _collider;
        private CurrencyState _state;

        #endregion

        #region Properties

        public int Amount => _amount;

        public CurrencyDataSO CurrencyData => _currencyData;

        public CurrencyState State => _state;

        public MoveToTarget MoveToTarget => _moveToTarget;

        public bool IsAvailable => _state == CurrencyState.Available;

        public bool IsCollecting => _state == CurrencyState.Collecting;

        public bool HasReachedTarget => _state == CurrencyState.ReachedTarget;

        public bool IsCollected => _state == CurrencyState.Collected;

        #endregion

        #region Events

        public event Action OnSpawned;
        public event Action OnCollectionStarted;
        public event Action<CurrencyComponent> OnTargetReached;
        public event Action OnCollected;
        public event Action OnCollectionCancelled;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            if (_moveToTarget == null)
                _moveToTarget = GetComponent<MoveToTarget>();
        }

        private void OnEnable()
        {
            _moveToTarget.OnTargetReached.AddListener(HandleTargetReached);

            ResetForSpawn();

            OnSpawned?.Invoke();
        }

        private void OnDisable()
        {
            _moveToTarget.OnTargetReached.RemoveListener(HandleTargetReached);
            _moveToTarget.Stop();
        }

        #endregion

        #region Collection

        public bool StartCollecting(Transform target)
        {
            if (!IsAvailable || target == null)
                return false;

            _state = CurrencyState.Collecting;

            OnCollectionStarted?.Invoke();

            _moveToTarget.SetTarget(target);

            return true;
        }

        public void CancelCollection()
        {
            if (!IsCollecting && !HasReachedTarget)
                return;

            _moveToTarget.Stop();

            _state = CurrencyState.Available;

            DropToGround();

            OnCollectionCancelled?.Invoke();
        }

        public void Collect()
        {
            if (!HasReachedTarget)
                return;

            _state = CurrencyState.Collected;

            _moveToTarget.Stop();

            if (_visual != null)
                _visual.SetActive(false);

            if (_collider != null)
                _collider.enabled = false;

            OnCollected?.Invoke();
        }

        private void HandleTargetReached()
        {
            if (!IsCollecting)
                return;

            _state = CurrencyState.ReachedTarget;

            OnTargetReached?.Invoke(this);
        }

        #endregion

        #region Pool Lifecycle

        public void PrepareForSpawn(int amount)
        {
            _amount = amount;
        }

        private void ResetForSpawn()
        {
            _state = CurrencyState.Available;

            _moveToTarget.Stop();

            if (_visual != null)
                _visual.SetActive(true);

            if (_collider != null)
                _collider.enabled = true;
        }

        #endregion

        #region Ground

        private void DropToGround()
        {
            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _groundRaycastDistance, _groundMask))
                return;

            transform.position = hit.point;
        }

        #endregion
    }
}