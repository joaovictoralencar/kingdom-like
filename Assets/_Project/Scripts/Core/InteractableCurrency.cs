using KingdomLike.Core;
using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
using MalbersAnimations.HAP;
using MalbersAnimations.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Currency
{
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

        private PlayerCurrencyComponent _pendingReceiver;
        private bool _isCollecting;

        private void OnEnable()
        {
            _interactable.OnInteractWithGO.AddListener(OnInteract);
            _moveToTarget.OnTargetReached.AddListener(OnTargetReached);
        }

        private void OnDisable()
        {
            _interactable.OnInteractWithGO.RemoveListener(OnInteract);
            _moveToTarget.OnTargetReached.RemoveListener(OnTargetReached);
        }

        private void OnInteract(GameObject interactable)
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

            OnCollected();
        }

        private void DropToGround()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _groundRaycastDistance, _groundMask))
            {
                transform.position = hit.point;
            }
        }

        protected virtual void OnCollected()
        {
            Destroy(gameObject);
        }
    }
}