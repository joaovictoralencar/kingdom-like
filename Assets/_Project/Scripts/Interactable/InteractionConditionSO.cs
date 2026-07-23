using HelloDev.Conditions;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base ScriptableObject for conditions that evaluate an interaction.
    /// </summary>
    public abstract class InteractionConditionSO : Condition_SO, IInteractionCondition
    {
        /// <summary>
        /// Evaluates the condition against an interactor and interactable.
        /// </summary>
        /// <param name="interactor">The object attempting the interaction.</param>
        /// <param name="interactable">The target being interacted with.</param>
        /// <returns>True when the interaction condition is satisfied.</returns>
        public abstract bool Evaluate(IInteractor interactor, IInteractable interactable);
    }

    /// <summary>
    /// Generic base class for interaction conditions that require a strongly typed context.
    /// </summary>
    /// <typeparam name="TContext">The context required by the condition.</typeparam>
    public abstract class InteractionCondition_SO<TContext> : InteractionConditionSO, ICondition<TContext>
    {
        /// <summary>
        /// Evaluates the condition using a strongly typed context.
        /// </summary>
        /// <param name="context">The context to evaluate.</param>
        /// <returns>The result of the condition evaluation.</returns>
        public bool Evaluate(TContext context)
        {
            bool result = EvaluateContext(context);
            return IsInverted ? !result : result;
        }

        /// <summary>
        /// Evaluates the condition against an interactor and interactable.
        /// </summary>
        /// <param name="interactor">The object attempting the interaction.</param>
        /// <param name="interactable">The target being interacted with.</param>
        /// <returns>The result of the condition evaluation.</returns>
        public sealed override bool Evaluate(
            IInteractor interactor,
            IInteractable interactable)
        {
            if (!TryCreateContext(interactor, interactable, out TContext context))
                return false;

            return Evaluate(context);
        }

        /// <summary>
        /// Creates the strongly typed context required by the condition.
        /// </summary>
        /// <param name="interactor">The object attempting the interaction.</param>
        /// <param name="interactable">The target being interacted with.</param>
        /// <param name="context">The resulting strongly typed context.</param>
        /// <returns>True when the context could be created successfully.</returns>
        protected abstract bool TryCreateContext(
            IInteractor interactor,
            IInteractable interactable,
            out TContext context);

        /// <summary>
        /// Evaluates the condition against the strongly typed context.
        /// </summary>
        /// <param name="context">The context to evaluate.</param>
        /// <returns>The result of the condition evaluation.</returns>
        protected abstract bool EvaluateContext(TContext context);

        /// <summary>
        /// Evaluates the condition without an interaction context.
        /// </summary>
        public override bool Evaluate()
        {
            return Evaluate(default);
        }
    }
}