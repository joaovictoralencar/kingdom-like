using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
using UnityEngine;

namespace KingdomLike.Interactables
{
    public class ChestInteractable : InteractableBase, ICurrencyCost
    {
        [Header("Cost")] [SerializeField] private CurrencyDataSO _currencyType;
        [SerializeField] private int _requiredAmount = 1;

        public CurrencyDataSO CurrencyType => _currencyType;
        public int RequiredAmount => _requiredAmount;

        public bool IsOpen { get; private set; }

        [Header("References")] [SerializeField]
        private Animator _animator;

        protected override void OnInteract(IInteractor interactor)
        {
            GameObject interactorObject = interactor.InteractorObject;

            // Try to find currency component in siblings, parent, or children
            EntityCurrencyComponent currency = interactorObject.GetComponentInParent<EntityCurrencyComponent>();

            if (currency == null)
            {
                currency = interactorObject.GetComponentInChildren<EntityCurrencyComponent>();
            }

            if (currency == null && interactorObject.transform.parent)
            {
                currency = interactorObject.transform.parent.GetComponentInChildren<EntityCurrencyComponent>();
            }

            if (currency == null)
            {
                Debug.LogError("Currency component not found on interactor object");
                return;
            }

            currency.Remove(_requiredAmount);
            OpenChest(interactor);
        }

        protected virtual void OpenChest(IInteractor interactor)
        {
            // Play opening animation.
            // Spawn rewards.
            // Play opening sound.
            Debug.Log("Chest opened");
            _animator.SetTrigger("Open");
            IsOpen = true;
        }

        public override void OnFocus(IInteractor interactor)
        {
            //Make chest crumble
            if (!IsOpen) _animator.SetTrigger("Crumble");
        }

        public override void OnUnfocus(IInteractor interactor)
        {
            if (!IsOpen) _animator.SetTrigger("Idle");
        }
    }
}