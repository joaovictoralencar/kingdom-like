using System;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base class responsible exclusively for unlock state and progression.
    ///
    /// No interaction-system plumbing is contained here.
    /// </summary>
    public abstract class UnlockableBase : MonoBehaviour, IUnlockable
    {
        [Header("Unlock")]
        [SerializeField] private bool _initiallyUnlocked;

        [Header("Events")]
        [SerializeField] private UnityEvent _onUnlocked = new();

        public bool IsUnlocked { get; private set; }

        public event Action OnUnlocked;

        protected virtual void Awake()
        {
            IsUnlocked = _initiallyUnlocked;

            if (IsUnlocked)
                OnRestoredAsUnlocked();
        }

        public virtual bool CanUnlock(IInteractor interactor)
        {
            if (IsUnlocked)
                return false;

            return interactor != null;
        }

        public bool Unlock(IInteractor interactor)
        {
            if (!CanUnlock(interactor))
                return false;

            if (!TryUnlock(interactor))
                return false;

            SetUnlocked();

            return true;
        }

        protected abstract bool TryUnlock(IInteractor interactor);

        protected void SetUnlocked()
        {
            if (IsUnlocked)
                return;

            IsUnlocked = true;

            OnUnlocked?.Invoke();
            _onUnlocked.Invoke();
        }

        protected virtual void OnRestoredAsUnlocked()
        {
        }

        /// <summary>
        /// Allows the existing save system to capture the unlock state
        /// without coupling this class to a specific save implementation.
        /// </summary>
        public UnlockableState CaptureUnlockState()
        {
            return new UnlockableState
            {
                IsUnlocked = IsUnlocked
            };
        }

        /// <summary>
        /// Allows the existing save system to restore the unlock state.
        /// </summary>
        public void RestoreUnlockState(UnlockableState state)
        {
            bool wasUnlocked = IsUnlocked;

            IsUnlocked = state.IsUnlocked;

            if (!wasUnlocked && IsUnlocked)
                OnRestoredAsUnlocked();
        }
    }

    [Serializable]
    public struct UnlockableState
    {
        public bool IsUnlocked;
    }
}