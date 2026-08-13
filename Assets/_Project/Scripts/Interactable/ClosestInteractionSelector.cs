using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Interactables
{
    [CreateAssetMenu(
        fileName = "SO_ClosestInteractionSelector",
        menuName = "KingdomLike/Scriptable Objects/Interactables/ClosestInteractionSelector",
        order = 0)]
    public class ClosestInteractionSelector : InteractionSelectorSO
    {
        public override IInteractionTarget Select(IInteractor interactor, IReadOnlyList<IInteractionTarget> targets)
        {
            if (interactor == null || interactor.InteractorObject == null)
                return null;

            if (targets == null || targets.Count == 0)
                return null;

            Vector3 interactorPosition = interactor.InteractorObject.transform.position;

            IInteractionTarget closestTarget = null;
            float closestDistanceSqr = float.MaxValue;

            for (int i = 0; i < targets.Count; i++)
            {
                IInteractionTarget target = targets[i];

                if (target == null)
                    continue;

                if (target.InteractorObject == null)
                    continue;

                if (!target.CanFocus(interactor))
                    continue;

                float distanceSqr =
                    (target.InteractorObject.transform.position - interactorPosition).sqrMagnitude;

                if (distanceSqr >= closestDistanceSqr)
                    continue;

                closestDistanceSqr = distanceSqr;
                closestTarget = target;
            }

            return closestTarget;
        }
    }
}