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
        /// Selects the best interaction target from a list of candidates.
        /// </summary>
        /// <param name="interactor">The interactor</param>
        /// <param name="targets">The interaction targets</param>
        /// <returns></returns>
        public abstract IInteractionTarget Select(IInteractor interactor, IReadOnlyList<IInteractionTarget> targets);
    }
}