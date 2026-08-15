using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Common interaction-system target.
    ///
    /// This interface contains only interaction plumbing.
    /// It does not define unlocking or gameplay actions.
    /// </summary>
    public interface IInteractionTarget
    {
        GameObject InteractorObject { get; }

        bool  CanFocus(IInteractor interactor);

        void OnFocus(IInteractor interactor);

        void OnUnfocus(IInteractor interactor);
    }
}