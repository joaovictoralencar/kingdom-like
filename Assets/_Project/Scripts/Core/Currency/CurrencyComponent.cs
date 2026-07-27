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
            Collected
        }

        #region Config

        [FoldoutGroup("Config")] [Tooltip("Layers considered ground when snapping the currency down after spawn/cancel.")] [SerializeField]
        private LayerMask _groundMask;

        [FoldoutGroup("Config")] [MinValue(0f)] [SerializeField]
        private float _groundRaycastDistance = 5f;

        [FoldoutGroup("Config")] [Required] [SerializeField]
        private CurrencyDataSO _currencyData;

        [FoldoutGroup("Config")] [Required] [SerializeField]
        private MoveToTarget _moveToTarget;

        [FoldoutGroup("Events")] [Required] [SerializeField]
        private CurrencyPoolManagerLocatorSO _currencyPoolManagerLocator;

        #endregion

        #region Runtime

        private CurrencyState _state;

        #endregion

        #region Properties

        public CurrencyDataSO CurrencyData => _currencyData;
        public CurrencyState State => _state;
        public MoveToTarget MoveToTarget => _moveToTarget;
        public bool IsAvailable => _state == CurrencyState.Available;
        public bool IsCollecting => _state == CurrencyState.Collecting;
        public bool IsCollected => _state == CurrencyState.Collected;

        #endregion

        #region Events

        public event Action OnSpawned;
        public event Action OnCollectionStarted;
        public event Action<CurrencyComponent> OnTargetReached;
        public event Action OnCollectionCancelled;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_moveToTarget == null)
                _moveToTarget = GetComponent<MoveToTarget>();
        }

        private void OnEnable()
        {
            _moveToTarget.OnTargetReached.AddListener(HandleTargetReached);
            ResetForSpawn();
            OnSpawned?.Invoke();
            DropToGround();
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
            if (!IsCollecting)
                return;

            _moveToTarget.Stop();
            _state = CurrencyState.Available;
            DropToGround();
            OnCollectionCancelled?.Invoke();
        }

        private void HandleTargetReached()
        {
            if (!IsCollecting)
                return;

            _state = CurrencyState.Collected;
            OnTargetReached?.Invoke(this);
            
            _currencyPoolManagerLocator.Get().ReturnToPool(this);
        }

        #endregion

        #region Pool Lifecycle

        private void ResetForSpawn()
        {
            _state = CurrencyState.Available;
            _moveToTarget.Stop();
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