using Ami.BroAudio;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Chest that can only be unlocked.
    ///
    /// Unlocking opens the chest. Any reward spawning is handled
    /// by separate components listening to the unlock event.
    /// </summary>
    [RequireComponent(typeof(UnlockableInteractionTarget))]
    public class ChestInteractable : CurrencyUnlockable, IInteractionFocusReceiver
    {
        [Header("Chest")]
        [SerializeField] private Animator _animator;

        [SerializeField] private SoundID _focusSoundID;
        [SerializeField] private SoundID _openSoundID;

        [Header("Reward")]
        [MinValue(1)]
        [SerializeField] private int _rewardAmount = 5;

        public int RewardAmount
        {
            get => _rewardAmount;
            set => _rewardAmount = Mathf.Max(1, value);
        }

        protected override bool TryUnlock(IInteractor interactor)
        {
            bool unlocked = base.TryUnlock(interactor);

            if (!unlocked)
                return false;

            if (_animator != null)
                _animator.SetTrigger("Open");

            BroAudio.Play(_openSoundID);
            BroAudio.Stop(_focusSoundID);

            return true;
        }

        public void OnInteractionFocus(IInteractor interactor)
        {
            if (IsUnlocked)
                return;

            if (_animator != null)
                _animator.SetTrigger("Crumble");

            BroAudio.Play(_focusSoundID);
        }

        public void OnInteractionUnfocus(IInteractor interactor)
        {
            if (IsUnlocked)
                return;

            if (_animator != null)
                _animator.SetTrigger("Idle");

            BroAudio.Stop(_focusSoundID);
        }
    }
}