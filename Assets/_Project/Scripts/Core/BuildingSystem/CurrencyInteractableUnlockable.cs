using HelloDev.Saving.Core;
using HelloDev.Saving.Interfaces;
using KingdomLike.Interactables;
using UnityEngine;

namespace KingdomLike.Core.Interactables
{
    [RequireComponent(typeof(CurrencyUnlockable))]
    public class CurrencyInteractableUnlockable : CurrencyInteractable
    {
        private CurrencyUnlockable _currencyUnlockable;

        protected override void Awake()
        {
            base.Awake();
            _currencyUnlockable = GetComponent<CurrencyUnlockable>();
        }

        public override string ModuleId { get; protected set; } = "CurrencyInteractableUnlockable";
        public override IUnifiedSaveManager SaveManager => UnifiedSaveManagerBehaviour.Instance.Manager;

        public override bool CanFocus(IInteractor interactor)
        {
            if (_currencyUnlockable == null)
                return false;

            if (!_currencyUnlockable.IsUnlocked)
                return false;

            return base.CanFocus(interactor);
        }

        protected override void OnInteract(IInteractor interactor)
        {
        }
    }
}