using Ami.BroAudio;
using KingdomLike.Core.Components;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Interactables
{
    public class ChestInteractable : InteractableBase, ICurrencyCost
    {
        [Header("Cost")] [SerializeField] private CurrencyDataSO _currencyType;
        [SerializeField] private int _requiredAmount = 1;
        [MinValue(1)] [SerializeField] private int _rewardAmount = 5;
        
        [SerializeField] private SoundID _focusSoundID;
        [SerializeField] private SoundID _openSoundID;

        public CurrencyDataSO CurrencyType => _currencyType;
        public int RequiredAmount => _requiredAmount;

        public bool IsOpen { get; private set; }

        public int RewardAmount
        {
            get => _rewardAmount;
            set => _rewardAmount = value;
        }

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
            _animator.SetTrigger("Open");
            BroAudio.Play(_openSoundID);
            BroAudio.Stop(_focusSoundID);
            IsOpen = true;
        }

        public override void OnFocus(IInteractor interactor)
        {
            base.OnFocus(interactor);
            //Make chest crumble
            if (!IsOpen)
            {
                _animator.SetTrigger("Crumble");
                BroAudio.Play(_focusSoundID);
            }
        }

        public override void OnUnfocus(IInteractor interactor)
        {
            base.OnUnfocus(interactor);
            if (!IsOpen)
            {
                _animator.SetTrigger("Idle");
                BroAudio.Stop(_focusSoundID);
            }
        }
    }
}