using HelloDev.Saving.Core;
using HelloDev.Saving.Interfaces;
using KingdomLike.Interactables;
using UnityEngine;

namespace KingdomLike.Core.Interactables
{
    [RequireComponent(typeof(CurrencyUnlockable))]
    public class CurrencyInteractableUnlockable : CurrencyInteractable
    {
        protected CurrencyUnlockable currencyUnlockable;

        protected override void Awake()
        {
            base.Awake();
            currencyUnlockable = GetComponent<CurrencyUnlockable>();
            currencyUnlockable.OnUnlockedStateChanged += HandleUnlockedStateChanged;
        }

        private void HandleUnlockedStateChanged(bool unlocked)
        {
            if (unlocked)
                OnUnlock();
            else
                OnLock();
        }

        public override string ModuleId { get; protected set; } = "CurrencyInteractableUnlockable";
        public override IUnifiedSaveManager SaveManager => UnifiedSaveManagerBehaviour.Instance.Manager;

        public override bool CanFocus(IInteractor interactor)
        {
            if (currencyUnlockable == null)
                return false;

            if (!currencyUnlockable.IsUnlocked)
                return false;

            return base.CanFocus(interactor);
        }

        protected override void OnInteract(IInteractor interactor)
        {
        }

        protected virtual void OnLock()
        {
        }

        protected virtual void OnUnlock()
        {
        }
    }
}