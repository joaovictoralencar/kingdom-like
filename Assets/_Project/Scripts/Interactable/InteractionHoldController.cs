using System;
using KingdomLike.Core.Components;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Handles "hold to interact" lifecycle for a single interactor.
    /// Emits progress events; does NOT touch UI or animations directly.
    /// </summary>
    public class InteractionHoldController : MonoBehaviour
    {
        [Tooltip("Seconds of hold required per currency unit.")]
        [SerializeField]
        private float _secondsPerCoin = 0.25f;

        private IFocusableInteractor _agent;

        private bool _isHolding;
        private IInteractable _target;
        private float _duration;
        private float _elapsed;

        public event Action<IInteractable, IInteractor> OnHoldStarted;
        public event Action<IInteractable, IInteractor, float> OnHoldProgress; // progress 0..1
        public event Action<IInteractable, IInteractor> OnHoldCancelled;
        public event Action<IInteractable, IInteractor> OnHoldCompleted;

        private void Awake()
        {
            _agent = GetComponent<IFocusableInteractor>();

            if (_agent == null) Debug.LogError($"{nameof(InteractionHoldController)} requires a component that implements IFocusableInteractor on the same GameObject.", this);
        }

        private void Update()
        {
            if (!_isHolding)
                return;

            // if focus changed, cancel
            if (_agent == null || _agent.FocusedInteractable != _target)
            {
                CancelHold();
                return;
            }

            // progress
            _elapsed += Time.deltaTime;
            float progress = _duration <= 0f ? 1f : Mathf.Clamp01(_elapsed / _duration);

            try
            {
                OnHoldProgress?.Invoke(_target, _agent as IInteractor, progress);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            if (progress >= 1f)
            {
                CompleteHold();
            }
        }

        public bool TryStartHold(IInteractable target)
        {
            if (_isHolding)
                return false;

            if (target == null)
                return false;

            // validate the target still accepts interaction from this agent
            if (!target.CanInteract(_agent as IInteractor))
                return false;

            // compute duration from currency cost if present
            float duration = 0f;

            if (target.InteractorObject != null)
            {
                var currencyCost = target.InteractorObject.GetComponent<ICurrencyCost>();
                if (currencyCost != null)
                {
                    // find currency component on the interactor
                    var currencyComp = FindCurrencyComponentOnAgent();
                    if (currencyComp == null)
                    {
                        Debug.LogWarning("Interactor has no EntityCurrencyComponent; cannot start currency-based hold.", this);
                        return false;
                    }

                    // ensure the interactor's currency type matches the cost
                    if (currencyComp.CurrencyData != currencyCost.CurrencyType)
                    {
                        Debug.LogWarning("Interactor currency type doesn't match interactable cost type.", this);
                        return false;
                    }

                    // ensure enough currency
                    if (currencyComp.Currency == null || !currencyComp.Currency.Has(currencyCost.RequiredAmount))
                    {
                        Debug.LogWarning("Interactor does not have enough currency to start hold.", this);
                        return false;
                    }

                    duration = Mathf.Max(0f, _secondsPerCoin * currencyCost.RequiredAmount);
                }
            }

            // fallback duration for non-currency interactions
            if (duration <= 0f)
                duration = _secondsPerCoin;

            _target = target;
            _duration = duration;
            _elapsed = 0f;
            _isHolding = true;

            OnHoldStarted?.Invoke(_target, _agent as IInteractor);

            // If duration is zero, complete immediately
            if (_duration <= 0f)
            {
                CompleteHold();
            }

            return true;
        }

        public void StopHold()
        {
            if (!_isHolding)
                return;

            CancelHold();
        }

        private void CancelHold()
        {
            _isHolding = false;
            try
            {
                OnHoldCancelled?.Invoke(_target, _agent as IInteractor);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            _target = null;
            _elapsed = 0f;
            _duration = 0f;
        }

        private void CompleteHold()
        {
            _isHolding = false;

            try
            {
                // perform the interaction (interactable will deduct currency and trigger events)
                _target?.Interact(_agent);

                OnHoldCompleted?.Invoke(_target, _agent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            _target = null;
            _elapsed = 0f;
            _duration = 0f;
        }

        private EntityCurrencyComponent FindCurrencyComponentOnAgent()
        {
            if (_agent == null || _agent.InteractorObject == null)
                return null;

            var go = _agent.InteractorObject;

            var comp = go.GetComponentInParent<EntityCurrencyComponent>();
            if (comp != null)
                return comp;

            comp = go.GetComponentInChildren<EntityCurrencyComponent>();
            if (comp != null)
                return comp;

            if (go.transform.parent != null)
                return go.transform.parent.GetComponentInChildren<EntityCurrencyComponent>();

            return null;
        }
    }
}