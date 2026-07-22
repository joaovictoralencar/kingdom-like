using Ami.BroAudio;
using KingdomLike.Currency;
using UnityEngine;

namespace KingdomLike.Core.Currency
{
    [RequireComponent(typeof(InteractableCurrency))]
    public class InteractableCurrency_Sounds : MonoBehaviour
    {
        [Header("Sounds")] [SerializeField] private SoundID _collectSound;

        [SerializeField] private SoundID _dropSound;

        [SerializeField] private SoundID _onInteractSound;

        private InteractableCurrency _interactableCurrency;

        private void Awake()
        {
            _interactableCurrency = GetComponent<InteractableCurrency>();
        }

        private void OnEnable()
        {
            if (_interactableCurrency == null)
                return;

            _interactableCurrency.OnCollected += HandleCollected;
            _interactableCurrency.OnDrop += HandleDrop;
            _interactableCurrency.OnInteract += HandleInteract;
        }

        private void OnDisable()
        {
            if (_interactableCurrency == null)
                return;

            _interactableCurrency.OnCollected -= HandleCollected;
            _interactableCurrency.OnDrop -= HandleDrop;
            _interactableCurrency.OnInteract -= HandleInteract;
        }

        private void HandleCollected()
        {
            PlaySound(_collectSound, false);
        }

        private void HandleDrop()
        {
            PlaySound(_dropSound, true);
        }

        private void HandleInteract(GameObject interactor)
        {
            PlaySound(_onInteractSound, true);
        }

        private void PlaySound(SoundID sound, bool follow)
        {
            if (follow) BroAudio.Play(sound, transform);
            else BroAudio.Play(sound, transform.position);
        }
    }
}