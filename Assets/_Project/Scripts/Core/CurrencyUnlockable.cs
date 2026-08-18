using HelloDev.Saving.Core;
using HelloDev.Saving.Interfaces;
using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Unlockable that requires a currency payment.
    ///
    /// Currency validation and payment belong here,
    /// not in UnlockableBase.
    /// </summary>
    public class CurrencyUnlockable : UnlockableBase, ICurrencyCost
    {
        [FoldoutGroup("Cost")] [Header("Currency Cost")] [SerializeField]
        private CurrencyDataSO _currencyType;

        [FoldoutGroup("Cost")] [Min(1)] [SerializeField]
        private int _requiredAmount = 1;

        public CurrencyDataSO CurrencyType => _currencyType;

        public int RequiredAmount => _requiredAmount;

        public override bool CanFocus(IInteractor interactor)
        {
            if (IsUnlocked) return false;
            return base.CanFocus(interactor);
        }

        public override bool CanUnlock(IInteractor interactor)
        {
            if (!base.CanUnlock(interactor))
                return false;

            EntityCurrencyComponent currency = FindCurrencyComponent(interactor);

            if (currency == null)
                return false;

            if (_currencyType == null)
                return false;

            if (!currency.CanReceive(_currencyType))
                return false;

            if (currency.Currency == null)
                return false;

            return currency.Currency.Has(_requiredAmount);
        }

        protected override bool TryUnlock(IInteractor interactor)
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

            return currency.TryRemove(_requiredAmount);
        }

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

        public override string ModuleId { get; protected set; } = "CurrencyUnlockable";
        public override IUnifiedSaveManager SaveManager => UnifiedSaveManagerBehaviour.Instance.Manager;
    }
}