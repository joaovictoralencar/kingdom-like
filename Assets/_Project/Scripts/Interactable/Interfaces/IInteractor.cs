using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Pure marker: anything that can interact with an IInteractable.
    /// No fields, no state — keeps the contract cheap for any object to implement.
    /// </summary>
    public interface IInteractor
    {
        GameObject InteractorObject { get; }
    }
}