using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
using KingdomLike.Interactables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Core.Interactables
{
    /// <summary>
    /// Interactable that requires a currency payment to interact with.
    /// </summary>
    public abstract class CurrencyInteractable : InteractableBase, ICurrencyCost
    {
        [TabGroup("Cost")]
        [Header("Currency Cost")] [SerializeField]
        private CurrencyDataSO _currencyType;
        [TabGroup("Cost")]
        [Min(1)] [SerializeField] protected int requiredAmount = 1;

        public CurrencyDataSO CurrencyType => _currencyType;

        public int RequiredAmount => requiredAmount;

        [TabGroup("Interactable", "BuildingInteractable")] [Header("Building Action")] [SerializeField]
        private UnityEvent _onExecuteInteraction = new();

        protected sealed override void OnExecuteInteraction(IInteractor interactor)
        {
            if (!CanPay(interactor)) return;
            OnInteract(interactor);
            _onExecuteInteraction.Invoke();
        }
        
        public void UpdateRequiredAmount(int amount)
        {
            requiredAmount = amount;
        }

        bool CanPay(IInteractor interactor)
        {
            EntityCurrencyComponent currency = FindCurrencyComponent(interactor);

            if (currency == null)
                return false;

            if (_currencyType == null)
                return false;

            if (!currency.CanReceive(_currencyType))
                return false;

            if (currency.Currency == null)
                return false;

            return currency.TryRemove(requiredAmount);
        }

        protected abstract void OnInteract(IInteractor interactor);


        private static EntityCurrencyComponent FindCurrencyComponent(IInteractor interactor)
        {
            if (interactor == null)
                return null;

            GameObject interactorObject = interactor.InteractorObject;

            if (interactorObject == null)
                return null;

            EntityCurrencyComponent currency = interactorObject.GetComponentInParent<EntityCurrencyComponent>();

            if (currency != null)
                return currency;

            currency = interactorObject.GetComponentInChildren<EntityCurrencyComponent>();

            if (currency != null)
                return currency;

            if (interactorObject.transform.parent != null)
            {
                currency = interactorObject.transform.parent.GetComponentInChildren<EntityCurrencyComponent>();
            }

            return currency;
        }
    }
}