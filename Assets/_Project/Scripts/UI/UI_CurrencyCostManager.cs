using System;
using System.Collections.Generic;
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
            int index = _spawnedCostDisplays.FindIndex(data => data.Interactable == payload.Target && data.Interactor == payload.Interactor);
            if (index < 0) return;
            InteractionCostData costData = _spawnedCostDisplays[index];

            // Unsubscribe any hold event handlers
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

        private async void OnInteractableFocused(InteractionPayload payload)
        {
            if (payload is not { IsFocused: true, IsInteracting: false }) return;
            
            //Grab the interactable component from the payload
            IInteractionTarget interactable = payload.Target;
            
            IInteractionCostDisplayer costDisplayer = interactable.InteractorObject.GetComponent<IInteractionCostDisplayer>();
            if (costDisplayer is null) return;
            
            ICurrencyCost currencyCost = interactable.InteractorObject.GetComponent<ICurrencyCost>();
            if (currencyCost is null) return;

            GameObject costDisplayGameObject = await Loader.InstantiateAsync(_currencyCostPrefab, _currencyCostContainer);
            UI_CurrencyCostDisplay costDisplay = costDisplayGameObject.GetComponent<UI_CurrencyCostDisplay>();
            
            var costData = new InteractionCostData()
            {
                Interactable = interactable,
                Interactor = payload.Interactor,
                CostDisplay = costDisplay
            };

            _spawnedCostDisplays.Add(costData);
            costDisplay.SetCurrencyCost(currencyCost, costDisplayer.UICostDisplayTarget);

            // Wire up hold controller events if available
            var holdController = payload.Interactor.InteractorObject.GetComponent<InteractionHoldController>();
            if (holdController != null)
            {
                costData.HoldController = holdController;

                costData.ProgressHandler = (ia, inter, progress) =>
                {
                    if (ia == interactable && inter == payload.Interactor)
                    {
                        costDisplay.SetProgress(progress);
                    }
                };

                costData.CancelHandler = (ia, inter) =>
                {
                    if (ia == interactable && inter == payload.Interactor)
                    {
                        // Reset visuals and progress but keep the display active
                        costDisplay.SetProgress(0f);
                        costDisplay.StopHoldVisuals(true);
                    }
                };

                costData.CompleteHandler = (ia, inter) =>
                {
                    if (ia == interactable && inter == payload.Interactor)
                    {
                        int idx = _spawnedCostDisplays.FindIndex(d => d.Interactable == ia && d.Interactor == inter);
                        if (idx >= 0)
                        {
                            var data = _spawnedCostDisplays[idx];
                            _spawnedCostDisplays.RemoveAt(idx);
                            data.CostDisplay.HideAndDestroy();

                            // unsubscribe
                            if (data.HoldController != null)
                            {
                                if (data.ProgressHandler != null) data.HoldController.OnHoldProgress -= data.ProgressHandler;
                                if (data.CancelHandler != null) data.HoldController.OnHoldCancelled -= data.CancelHandler;
                                if (data.CompleteHandler != null) data.HoldController.OnHoldCompleted -= data.CompleteHandler;
                                if (data.StartHandler != null) data.HoldController.OnHoldStarted -= data.StartHandler;
                            }
                        }
                    }
                };

                // Start handler triggers visuals
                costData.StartHandler = (ia, inter) =>
                {
                    if (ia == interactable && inter == payload.Interactor)
                    {
                        costDisplay.StartHoldVisuals(_holdVisualScale, _holdShakeMagnitude);
                    }
                };

                holdController.OnHoldProgress += costData.ProgressHandler;
                holdController.OnHoldCancelled += costData.CancelHandler;
                holdController.OnHoldCompleted += costData.CompleteHandler;
                holdController.OnHoldStarted += costData.StartHandler;
            }
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