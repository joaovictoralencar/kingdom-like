using PrimeTween;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Moves a transform from a start point to an end point along a
    /// parabolic arc (no physics/gravity involved), driven by PrimeTween.
    /// </summary>
    public static class CurrencyArcMotion
    {
        /// <summary>
        /// Plays the arc: horizontal position is linearly interpolated,
        /// while a vertical offset following 4 * h * t * (1 - t) is added
        /// on top, producing a smooth up-then-down parabola that peaks at
        /// t = 0.5 with height <paramref name="arcHeight"/>.
        /// </summary>
        public static Tween PlayArc(Transform target, Vector3 startPosition, Vector3 endPosition, float duration, float arcHeight, Ease ease = Ease.OutQuad)
        {
            target.position = startPosition;

            return Tween.Custom(0f, 1f, duration, onValueChange: t =>
            {
                Vector3 flatPosition = Vector3.Lerp(startPosition, endPosition, t);
                float height = 4f * arcHeight * t * (1f - t);
                target.position = flatPosition + Vector3.up * height;
            },
            ease: ease);
        }
    }
}