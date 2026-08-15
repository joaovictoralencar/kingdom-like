using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using KingdomLike.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    public abstract class InteractableBase : InteractionTargetBase, IInteractable, IInteractionCostDisplayer
    {
        [TabGroup("Interactable", "InteractableBase")] [Header("References")] [SerializeField]
        private Transform _costDisplayTarget;

        [TabGroup("Interactable", "InteractableBase")] [Header("Focus Conditions")] [SerializeField]
        private List<InteractionConditionSO> _focusConditions = new();

        [TabGroup("Interactable", "InteractableBase")] [Header("Interaction Conditions")] [SerializeField]
        private List<InteractionConditionSO> _interactConditions = new();

        [TabGroup("Interactable", "InteractableBase")] [Header("Behavior")] [SerializeField]
        private bool _useOnce;

        [TabGroup("Interactable", "InteractableBase")] [Header("Events")] [SerializeField]
        private UnityEvent _onExecute = new();

        private bool _hasBeenUsed;

        public Transform UICostDisplayTarget => _costDisplayTarget;

        public override bool CanFocus(IInteractor interactor)
        {
            if (interactor == null)
                return false;

            if (_useOnce && _hasBeenUsed)
                return false;

            return EvaluateConditions(_focusConditions, interactor);
        }

        public virtual bool CanInteract(IInteractor interactor)
        {
            if (interactor == null)
                return false;

            if (_useOnce && _hasBeenUsed)
                return false;

            return EvaluateConditions(_interactConditions, interactor);
        }

        public bool ExecuteInteraction(IInteractor interactor)
        {
            if (!CanInteract(interactor))
                return false;

            OnExecuteInteraction(interactor);
            _onExecute.Invoke();

            if (!_useOnce)
                return true;

            _hasBeenUsed = true;

            RefreshInteractorCandidates();

            if (InteractionCollider != null)
                InteractionCollider.enabled = false;

            return true;
        }

        public virtual bool TryGetInteractionCost(IInteractor interactor, out ICurrencyCost currencyCost)
        {
            currencyCost = null;

            if (this is not ICurrencyCost cost)
                return false;

            currencyCost = cost;
            return true;
        }

        protected abstract void OnExecuteInteraction(IInteractor interactor);

        private bool EvaluateConditions(List<InteractionConditionSO> conditions, IInteractor interactor)
        {
            if (interactor == null)
                return false;

            foreach (InteractionConditionSO condition in conditions)
            {
                if (condition == null)
                    continue;

                if (!condition.Evaluate(interactor, this))
                    return false;
            }

            return true;
        }

        protected override InteractionTargetState SaveState()
        {
            return new InteractableBaseState(_hasBeenUsed);
        }

        protected override UniTask LoadState(InteractionTargetState state)
        {
            if (state is not InteractableBaseState interactableBaseSnapShot)
                return UniTask.CompletedTask;

            _hasBeenUsed = interactableBaseSnapShot._hasBeenUsed;
            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public class InteractableBaseState : InteractionTargetState
    {
        public bool _hasBeenUsed;

        public InteractableBaseState(bool hasBeenUsed) : base()
        {
            _hasBeenUsed = hasBeenUsed;
        }
    }
}