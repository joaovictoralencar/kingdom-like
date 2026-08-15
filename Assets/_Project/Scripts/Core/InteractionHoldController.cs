using System;
using System.Linq;
using KingdomLike.Core;
using KingdomLike.Core.Components;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Handles "hold to interact" lifecycle for a single interactor.
    /// Emits progress events without owning UI or gameplay behavior.
    /// </summary>
    public class InteractionHoldController : MonoBehaviour
    {
        [Tooltip("Seconds of hold required per currency unit.")]
        [SerializeField]
        private float _secondsPerCoin = 0.25f;

        private IFocusableInteractor _agent;

        private bool _isHolding;
        private IInteractionTarget _target;
        private float _duration;
        private float _elapsed;

        public event Action<IInteractionTarget, IInteractor> OnHoldStarted;
        public event Action<IInteractionTarget, IInteractor, float> OnHoldProgress;
        public event Action<IInteractionTarget, IInteractor> OnHoldCancelled;
        public event Action<IInteractionTarget, IInteractor> OnHoldCompleted;

        private void Awake()
        {
            _agent = GetComponent<IFocusableInteractor>();

            if (_agent == null)
            {
                Debug.LogError($"{nameof(InteractionHoldController)} requires a component that implements IFocusableInteractor on the same GameObject.", this);
            }
        }

        private void Update()
        {
            if (!_isHolding)
                return;

            if (_agent == null || _agent.FocusedTarget != _target)
            {
                CancelHold();
                return;
            }

            _elapsed += Time.deltaTime;

            float progress = _duration <= 0f ? 1f : Mathf.Clamp01(_elapsed / _duration);

            try
            {
                OnHoldProgress?.Invoke(_target, _agent, progress);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            if (progress >= 1f)
                CompleteHold();
        }

        public bool TryStartHold(IInteractionTarget target)
        {
            if (_isHolding)
                return false;

            if (target == null)
                return false;

            if (_agent == null)
                return false;

            if (!CanExecuteTarget(target))
                return false;

            float duration = CalculateDuration(target);

            if (duration < 0f)
                return false;

            _target = target;
            _duration = duration;
            _elapsed = 0f;
            _isHolding = true;

            try
            {
                OnHoldStarted?.Invoke(_target, _agent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            if (_duration <= 0f)
                CompleteHold();

            return true;
        }

        public void StopHold()
        {
            if (!_isHolding)
                return;

            CancelHold();
        }

        private bool CanExecuteTarget(IInteractionTarget target)
        {
            IUnlockable unlockable = FindCapability<IUnlockable>(target);

            if (unlockable != null && !unlockable.IsUnlocked)
                return unlockable.CanUnlock(_agent);

            IInteractable interactable = FindCapability<IInteractable>(target);

            if (interactable != null) return interactable.CanInteract(_agent);

            return false;
        }

        private float CalculateDuration(IInteractionTarget target)
        {
            if (target.InteractorObject != null)
            {
                var costDisplayers = target.InteractorObject.GetComponents<IInteractionCostDisplayer>();
                ICurrencyCost currencyCost = null;

                var costDisplayer = costDisplayers.FirstOrDefault(cd => cd.TryGetInteractionCost(_agent, out currencyCost));

                if (costDisplayer != null && currencyCost != null)
                {
                    EntityCurrencyComponent currency = FindCurrencyComponentOnAgent();

                    if (currency == null)
                    {
                        Debug.LogWarning("Interactor has no EntityCurrencyComponent; cannot start currency-based hold.", this);
                        return -1f;
                    }

                    if (currency.CurrencyData != currencyCost.CurrencyType)
                    {
                        Debug.LogWarning("Interactor currency type does not match target cost type.", this);

                        return -1f;
                    }

                    if (currency.Currency == null || !currency.Currency.Has(currencyCost.RequiredAmount))
                    {
                        Debug.LogWarning("Interactor does not have enough currency to start hold.", this);

                        return -1f;
                    }

                    return Mathf.Max(0f, _secondsPerCoin * currencyCost.RequiredAmount);
                }
            }

            return _secondsPerCoin;
        }

        private void CancelHold()
        {
            if (!_isHolding)
                return;

            _isHolding = false;

            try
            {
                OnHoldCancelled?.Invoke(_target, _agent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            ResetState();
        }

        private void CompleteHold()
        {
            if (!_isHolding)
                return;

            _isHolding = false;

            IInteractionTarget completedTarget = _target;

            try
            {
                /*
                 * The agent owns the state machine:
                 *
                 * Locked + IUnlockable
                 *     -> Unlock()
                 *
                 * Unlocked + IInteractable
                 *     -> ExecuteInteraction()
                 */
                _agent.Interact();

                OnHoldCompleted?.Invoke(completedTarget, _agent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            ResetState();
        }

        private void ResetState()
        {
            _target = null;
            _elapsed = 0f;
            _duration = 0f;
        }

        private static T FindCapability<T>(IInteractionTarget target) where T : class
        {
            if (target is T capability)
                return capability;

            if (target?.InteractorObject == null)
                return null;

            return target.InteractorObject.GetComponent<T>();
        }

        private EntityCurrencyComponent FindCurrencyComponentOnAgent()
        {
            if (_agent == null || _agent.InteractorObject == null)
                return null;

            GameObject go = _agent.InteractorObject;

            EntityCurrencyComponent currency = go.GetComponentInParent<EntityCurrencyComponent>();

            if (currency != null)
                return currency;

            currency = go.GetComponentInChildren<EntityCurrencyComponent>();

            if (currency != null)
                return currency;

            if (go.transform.parent != null)
            {
                currency = go.transform.parent.GetComponentInChildren<EntityCurrencyComponent>();
            }

            return currency;
        }
    }
}