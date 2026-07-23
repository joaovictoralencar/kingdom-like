namespace KingdomLike.Interactables
{
    public interface IInteractionCandidate
    {
        void AddInteractionCandidate(IInteractable interactable);
        void RemoveInteractionCandidate(IInteractable interactable);
        void RefreshInteractionCandidates();
    }
}