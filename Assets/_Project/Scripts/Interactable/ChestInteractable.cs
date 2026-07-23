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
        }

        public override void OnFocus(IInteractor interactor)
        {
            //Make chest crumble
        }

        public override void OnUnfocus(IInteractor interactor)
        {
        }
    }
}