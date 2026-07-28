using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base ScriptableObject for interaction selection strategies.
    /// </summary>
    public abstract class InteractionSelectorSO : ScriptableObject, IInteractionSelector
    {
        /// <summary>
        /// Selects the best interactable from the available candidates.
        /// </summary>
        /// <param name="interactor">The interactor selecting an interactable.</param>
        /// <param name="interactables">The available interaction candidates.</param>
        /// <returns>The selected interactable, or null when none can be selected.</returns>
        public abstract IInteractable Select(IInteractor interactor, IReadOnlyList<IInteractable> interactables);
    }
}