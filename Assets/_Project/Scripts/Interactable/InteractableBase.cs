using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KingdomLike.Core;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base class for objects that execute an interaction action.
    ///
    /// Contains interaction plumbing only.
    /// Contains no unlocking logic.
    /// </summary>
    public abstract class InteractableBase : InteractionTargetBase, IInteractable, IInteractionCostDisplayer
    {
        [Header("References")]
        [SerializeField] private Transform _costDisplayTarget;

        [Header("Focus Conditions")]
        [SerializeField] private List<InteractionConditionSO> _focusConditions = new();

        [Header("Interaction Conditions")]
        [SerializeField] private List<InteractionConditionSO> _interactConditions = new();

        [Header("Behavior")]
        [SerializeField] private bool _useOnce;

        [Header("Events")]
        [SerializeField] private UnityEvent _onExecute = new();

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

        public void ExecuteInteraction(IInteractor interactor)
        {
            if (!CanInteract(interactor))
                return;

            OnExecuteInteraction(interactor);

            _onExecute.Invoke();

            if (!_useOnce)
                return;

            _hasBeenUsed = true;

            RefreshInteractorCandidates();

            if (InteractionCollider) InteractionCollider.enabled = false;
        }

        protected abstract void OnExecuteInteraction(IInteractor interactor);

        public override void OnFocus(IInteractor interactor)
        {
            base.OnFocus(interactor);
        }

        public override void OnUnfocus(IInteractor interactor)
        {
            base.OnUnfocus(interactor);
        }

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
    }
}