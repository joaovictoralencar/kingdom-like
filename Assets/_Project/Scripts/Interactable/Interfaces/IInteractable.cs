using UnityEngine;

namespace KingdomLike.Interactables
{
    public interface IInteractable
    {
        GameObject InteractorObject { get; }
        
        bool CanFocus(IInteractor interactor);
        bool CanInteract(IInteractor interactor);

        void Interact(IInteractor interactor);

        void OnFocus(IInteractor interactor);
        void OnUnfocus(IInteractor interactor);
    }
}