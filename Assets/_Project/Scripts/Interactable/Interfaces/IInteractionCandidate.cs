namespace KingdomLike.Interactables
{
    public interface IInteractionCandidate
    {
        void AddInteractionTarget(IInteractionTarget target);

        void RemoveInteractionTarget(IInteractionTarget target);

        void RefreshInteractionCandidates();
    }
}