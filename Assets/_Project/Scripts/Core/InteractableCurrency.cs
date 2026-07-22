using KingdomLike.Core;
using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
using MalbersAnimations.HAP;
using MalbersAnimations.Utilities;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace KingdomLike.Currency
{
    [RequireComponent(typeof(Collider))]
    public class InteractableCurrency : MonoBehaviour
    {
        [FoldoutGroup("Settings")]
        [MinValue(1)]
        [SerializeField]
        private int _amount = 1;

        [FoldoutGroup("Settings")]
        [Tooltip("Layers considered 'ground' when snapping the coin down after a failed collection.")]
        [SerializeField]
        private LayerMask _groundMask;

        [FoldoutGroup("Settings")]
        [SerializeField]
        private float _groundRaycastDistance = 5f;

        [FoldoutGroup("Data")]
        [Required]
        [SerializeField]
        private CurrencyDataSO _currencyData;

        [FoldoutGroup("References")]
        [Required]
        [SerializeField]
        private MInteract _interactable;

        [FoldoutGroup("References")]
        [Required]
        [SerializeField]
        private MoveToTarget _moveToTarget;

        [FoldoutGroup("References")]
        [Tooltip("Visual representation of the coin. Hidden as soon as collection succeeds, before the object is destroyed.")]
        [SerializeField]
        private GameObject _visual;

        private Collider _collider;
        private PlayerCurrencyComponent _pendingReceiver;
        private bool _isCollecting;

        /// <summary>Fired once collection succeeds, right before the object is destroyed.</summary>
        public event Action OnCollected;

        /// <summary>Fired when the coin spawns (OnEnable) and when a collection attempt fails and it drops back to the ground.</summary>
        public event Action OnDrop;

        /// <summary>Fired when a valid interactor starts collecting this currency.</summary>
        public event Action<GameObject> OnInteract;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _interactable.OnInteractWithGO.AddListener(HandleInteract);
            _moveToTarget.OnTargetReached.AddListener(OnTargetReached);

            if (_visual != null)
                _visual.SetActive(true);

            if (_collider != null)
                _collider.enabled = true;

            OnDrop?.Invoke();
        }

        private void OnDisable()
        {
            _interactable.OnInteractWithGO.RemoveListener(HandleInteract);
            _moveToTarget.OnTargetReached.RemoveListener(OnTargetReached);
        }

        private void HandleInteract(GameObject interactable)
        {
            if (_isCollecting || interactable == null)
                return;

            Mount mount = interactable.GetComponentInChildren<Mount>();
            if (mount)
            {
                interactable = mount.Rider.gameObject;
            }

            PlayerCurrencyComponent playerCurrencyComponent = interactable.GetComponentInChildren<PlayerCurrencyComponent>();

            if (playerCurrencyComponent == null)
                return;

            if (!playerCurrencyComponent.CanReceive(_currencyData))
                return;

            _pendingReceiver = playerCurrencyComponent;
            _isCollecting = true;
            _moveToTarget.SetTarget(interactable.transform);

            OnInteract?.Invoke(interactable);
        }

        private void OnTargetReached()
        {
            if (_pendingReceiver == null || !_pendingReceiver.TryAdd(_amount))
            {
                _isCollecting = false;
                _pendingReceiver = null;
                DropToGround();
                return;
            }

            Collected();
        }

        private void DropToGround()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _groundRaycastDistance, _groundMask))
            {
                transform.position = hit.point;
            }

            OnDrop?.Invoke();
        }

        protected virtual void Collected()
        {
            if (_visual != null)
                _visual.SetActive(false);

            if (_collider != null)
                _collider.enabled = false;

            OnCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}