using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
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

        [FoldoutGroup("Data")]
        [Required]
        [SerializeField]
        private CurrencyDataSO _currencyData;

        [FoldoutGroup("References")]
        [Required]
        [SerializeField]
        private MInteract _interactable;

        private void OnEnable()
        {
            _interactable.OnInteractWithGO.AddListener(OnInteract);
        }

        private void OnDisable()
        {
            _interactable.OnInteractWithGO.RemoveListener(OnInteract);
        }

        private void OnInteract(GameObject interactable)
        {
            if (interactable == null)
                return;

            PlayerCurrencyComponent playerCurrencyComponent = interactable.GetComponentInChildren<PlayerCurrencyComponent>();

            if (playerCurrencyComponent == null)
                return;

            if (!playerCurrencyComponent.CanReceive(_currencyData))
                return;

            if (!playerCurrencyComponent.TryAdd(_amount))
                return;

            OnCollected();
        }

        protected virtual void OnCollected()
        {
            Destroy(gameObject);
        }
    }
}