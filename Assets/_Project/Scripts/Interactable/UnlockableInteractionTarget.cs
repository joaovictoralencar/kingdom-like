using UnityEngine;

namespace KingdomLike.Interactables
{
    [RequireComponent(typeof(Collider))]
    public sealed class UnlockableInteractionTarget : InteractionTargetBase
    {
        private IUnlockable _unlockable;
        private IInteractionFocusReceiver _focusReceiver;

        protected override void Awake()
        {
            base.Awake();

            _unlockable = GetComponent<IUnlockable>();
            _focusReceiver = GetComponent<IInteractionFocusReceiver>();

            if (_unlockable == null)
            {
                Debug.LogError($"{name} requires an IUnlockable component.", this);
            }
        }

        public override bool CanFocus(IInteractor interactor)
        {
            if (_unlockable == null)
                return false;

            if (interactor == null)
                return false;

            if (_unlockable.IsUnlocked)
                return true;

            return _unlockable.CanUnlock(interactor);
        }

        public override void OnFocus(IInteractor interactor)
        {
            _focusReceiver?.OnInteractionFocus(interactor);
        }

        public override void OnUnfocus(IInteractor interactor)
        {
            _focusReceiver?.OnInteractionUnfocus(interactor);
        }
    }
}