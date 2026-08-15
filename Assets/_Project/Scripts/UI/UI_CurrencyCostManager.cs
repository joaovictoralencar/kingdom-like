using System;
using System.Collections.Generic;
using System.Linq;
using HelloDev.Loader;
using KingdomLike.Core;
using KingdomLike.Interactables;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KingdomLike.UI
{
    public class UI_CurrencyCostManager : MonoBehaviour
    {
        [SerializeField] AssetReferenceGameObject _currencyCostPrefab;
        [SerializeField] Transform _currencyCostContainer;

        [SerializeField] private InteractionEventSO InteractableFocusedEvent;
        [SerializeField] private InteractionEventSO InteractableUnfocusedEvent;

        List<InteractionCostData> _spawnedCostDisplays = new();

        private void Awake()
        {
            InteractableFocusedEvent.AddListener(OnInteractableFocused);
            InteractableUnfocusedEvent.AddListener(OnInteractableUnfocused);
        }

        private void OnDestroy()
        {
            InteractableFocusedEvent.RemoveListener(OnInteractableFocused);
            InteractableUnfocusedEvent.RemoveListener(OnInteractableUnfocused);
        }

        [Header("Hold Visual Config")] [SerializeField]
        private float _holdVisualScale = 1.15f;

        [SerializeField] private float _holdShakeMagnitude = 4f;

        private void OnInteractableUnfocused(InteractionPayload payload)
        {
            var matches = _spawnedCostDisplays
                .Where(d => d.Interactable == payload.Target && d.Interactor == payload.Interactor)
                .ToList();

            foreach (var costData in matches)
            {
                if (costData.HoldController != null)
                {
                    if (costData.ProgressHandler != null) costData.HoldController.OnHoldProgress -= costData.ProgressHandler;
                    if (costData.CancelHandler != null) costData.HoldController.OnHoldCancelled -= costData.CancelHandler;
                    if (costData.CompleteHandler != null) costData.HoldController.OnHoldCompleted -= costData.CompleteHandler;
                    if (costData.StartHandler != null) costData.HoldController.OnHoldStarted -= costData.StartHandler;
                }

                _spawnedCostDisplays.Remove(costData);
                costData.CostDisplay.HideAndDestroy();
            }
        }

        private async void OnInteractableFocused(InteractionPayload payload)
        {
            if (payload is not { IsFocused: true, IsInteracting: false })
            {
                return;
            }

            IInteractionTarget target = payload.Target;

            if (target == null || target.InteractorObject == null)
                return;

            IInteractionCostDisplayer[] costDisplayers = target.InteractorObject.GetComponents<IInteractionCostDisplayer>();
            ICurrencyCost currencyCost = null;

            IInteractionCostDisplayer costDisplayer = costDisplayers.FirstOrDefault(cd => cd.TryGetInteractionCost(payload.Interactor, out currencyCost));

            int existingIndex = _spawnedCostDisplays.FindIndex(data => data.Interactable == target && data.Interactor == payload.Interactor);

            if (existingIndex >= 0)
            {
                InteractionCostData existing = _spawnedCostDisplays[existingIndex];

                if (costDisplayer == null)
                {
                    // Target can no longer be interacted with (e.g. maxed out) - hide it.
                    UnsubscribeHoldEvents(existing);
                    _spawnedCostDisplays.RemoveAt(existingIndex);
                    existing.CostDisplay.HideAndDestroy();
                    return;
                }

                // Same target, still valid - just refresh the displayed cost, don't respawn.
                existing.CostDisplay.SetCurrencyCost(currencyCost, costDisplayer.UICostDisplayTarget);
                return;
            }

            if (costDisplayer == null)
                return;

            GameObject costDisplayGameObject = await Loader.InstantiateAsync(_currencyCostPrefab, _currencyCostContainer);

            if (costDisplayGameObject == null)
                return;

            UI_CurrencyCostDisplay costDisplay = costDisplayGameObject.GetComponent<UI_CurrencyCostDisplay>();

            if (costDisplay == null)
            {
                Destroy(costDisplayGameObject);
                return;
            }

            var costData = new InteractionCostData
            {
                Interactable = target,
                Interactor = payload.Interactor,
                CostDisplay = costDisplay
            };

            _spawnedCostDisplays.Add(costData);

            costDisplay.SetCurrencyCost(currencyCost, costDisplayer.UICostDisplayTarget);

            InteractionHoldController holdController = payload.Interactor.InteractorObject.GetComponent<InteractionHoldController>();

            if (holdController == null)
                return;

            costData.HoldController = holdController;

            costData.ProgressHandler = (ia, inter, progress) =>
            {
                if (ia == target && inter == payload.Interactor)
                    costDisplay.SetProgress(progress);
            };

            costData.CancelHandler = (ia, inter) =>
            {
                if (ia != target || inter != payload.Interactor)
                    return;

                costDisplay.SetProgress(0f);
                costDisplay.StopHoldVisuals(true);
            };

            costData.CompleteHandler = (ia, inter) =>
            {
                if (ia != target || inter != payload.Interactor)
                    return;

                // No removal here anymore: Interact() now re-raises Focused (refresh path above)
                // if the target is still valid, or ClearFocusedTarget fires a real Unfocused
                // event (handled by OnInteractableUnfocused) if it's not. Either way this
                // display's fate is decided by one of those two paths.
            };

            costData.StartHandler = (ia, inter) =>
            {
                if (ia == target && inter == payload.Interactor)
                {
                    costDisplay.StartHoldVisuals(_holdVisualScale, _holdShakeMagnitude);
                }
            };

            holdController.OnHoldProgress += costData.ProgressHandler;
            holdController.OnHoldCancelled += costData.CancelHandler;
            holdController.OnHoldCompleted += costData.CompleteHandler;
            holdController.OnHoldStarted += costData.StartHandler;
        }
        
        private void UnsubscribeHoldEvents(InteractionCostData costData)
        {
            if (costData.HoldController == null)
                return;

            if (costData.ProgressHandler != null) costData.HoldController.OnHoldProgress -= costData.ProgressHandler;
            if (costData.CancelHandler != null) costData.HoldController.OnHoldCancelled -= costData.CancelHandler;
            if (costData.CompleteHandler != null) costData.HoldController.OnHoldCompleted -= costData.CompleteHandler;
            if (costData.StartHandler != null) costData.HoldController.OnHoldStarted -= costData.StartHandler;
        }
    }

    class InteractionCostData
    {
        public IInteractionTarget Interactable;
        public IInteractor Interactor;
        public UI_CurrencyCostDisplay CostDisplay;

        public InteractionHoldController HoldController;
        public Action<IInteractionTarget, IInteractor, float> ProgressHandler;
        public Action<IInteractionTarget, IInteractor> CancelHandler;
        public Action<IInteractionTarget, IInteractor> CompleteHandler;
        public Action<IInteractionTarget, IInteractor> StartHandler;
    }
}