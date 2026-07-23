using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Interactables
{
    [CreateAssetMenu(fileName = "SO_ClosestInteractionSelector", menuName = "KingdomLike/Scriptable Objects/Interactables/ClosestInteractionSelector", order = 0)]
    public class ClosestInteractionSelector : InteractionSelectorSO
    {
        public override IInteractable Select(IInteractor interactor, IReadOnlyList<IInteractable> interactables)
        {
            if (interactor == null || interactor.InteractorObject == null)
                return null;

            if (interactables == null || interactables.Count == 0)
                return null;

            Vector3 interactorPosition = interactor.InteractorObject.transform.position;

            IInteractable closestInteractable = null;
            float closestDistanceSqr = float.MaxValue;

            for (int i = 0; i < interactables.Count; i++)
            {
                IInteractable interactable = interactables[i];

                if (interactable == null)
                    continue;

                if (interactable is not Component component)
                    continue;

                float distanceSqr = (component.transform.position - interactorPosition).sqrMagnitude;

                if (distanceSqr >= closestDistanceSqr)
                    continue;

                closestDistanceSqr = distanceSqr;
                closestInteractable = interactable;
            }

            return closestInteractable;
        }
    }
}