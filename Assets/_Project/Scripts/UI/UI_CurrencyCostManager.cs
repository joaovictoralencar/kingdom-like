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
            if (payload is not { IsFocused: true, IsInteracting: false })
            {
                return;
            }

            IInteractionTarget target = payload.Target;

            if (target == null || target.InteractorObject == null)
                return;

            IInteractionCostDisplayer costDisplayer = target.InteractorObject.GetComponent<IInteractionCostDisplayer>();

            if (costDisplayer == null)
                return;

            if (!costDisplayer.TryGetInteractionCost(payload.Interactor, out ICurrencyCost currencyCost))
            {
                return;
            }

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

                int index = _spawnedCostDisplays.FindIndex(d => d.Interactable == ia && d.Interactor == inter);

                if (index < 0)
                    return;

                InteractionCostData data =
                    _spawnedCostDisplays[index];

                _spawnedCostDisplays.RemoveAt(index);

                data.CostDisplay.HideAndDestroy();

                if (data.HoldController != null)
                {
                    if (data.ProgressHandler != null) data.HoldController.OnHoldProgress -= data.ProgressHandler;
                    if (data.CancelHandler != null) data.HoldController.OnHoldCancelled -= data.CancelHandler;
                    if (data.CompleteHandler != null) data.HoldController.OnHoldCompleted -= data.CompleteHandler;
                    if (data.StartHandler != null) data.HoldController.OnHoldStarted -= data.StartHandler;
                }
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