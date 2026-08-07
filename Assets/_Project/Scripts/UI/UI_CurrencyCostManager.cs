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

        private void OnInteractableUnfocused(InteractionPayload payload)
        {
            int index = _spawnedCostDisplays.FindIndex(data => data.Interactable == payload.Interactable && data.Interactor == payload.Interactor);
            if (index < 0) return;
            InteractionCostData costData = _spawnedCostDisplays[index];
            _spawnedCostDisplays.Remove(costData);
            costData.CostDisplay.HideAndDestroy();
        }

        private async void OnInteractableFocused(InteractionPayload payload)
        {
            if (payload is not { IsFocused: true, IsInteracting: false }) return;
            
            //Grab the interactable component from the payload
            IInteractable interactable = payload.Interactable;
            
            IInteractionCostDisplayer costDisplayer = interactable.InteractorObject.GetComponent<IInteractionCostDisplayer>();
            if (costDisplayer is null) return;
            
            ICurrencyCost currencyCost = interactable.InteractorObject.GetComponent<ICurrencyCost>();
            if (currencyCost is null) return;

            GameObject costDisplayGameObject = await Loader.InstantiateAsync(_currencyCostPrefab, _currencyCostContainer);
            UI_CurrencyCostDisplay costDisplay = costDisplayGameObject.GetComponent<UI_CurrencyCostDisplay>();
            
            _spawnedCostDisplays.Add(new InteractionCostData()
            {
                Interactable = interactable,
                Interactor = payload.Interactor,
                CostDisplay = costDisplay
            });
            costDisplay.SetCurrencyCost(currencyCost, costDisplayer.UICostDisplayTarget);
        }
    }

    struct InteractionCostData : IEquatable<InteractionCostData>
    {
        public IInteractable Interactable;
        public IInteractor Interactor;
        public UI_CurrencyCostDisplay CostDisplay;

        public bool Equals(InteractionCostData other)
        {
            return Equals(Interactable, other.Interactable) && Equals(Interactor, other.Interactor) && Equals(CostDisplay, other.CostDisplay);
        }

        public override bool Equals(object obj)
        {
            return obj is InteractionCostData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Interactable, Interactor, CostDisplay);
        }
    }
}