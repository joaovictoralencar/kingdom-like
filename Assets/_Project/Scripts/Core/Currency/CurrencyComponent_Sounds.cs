using Ami.BroAudio;
using KingdomLike.Currency;
using UnityEngine;

namespace KingdomLike.Core.Currency
{
    [RequireComponent(typeof(CurrencyComponent))]
    public class CurrencyComponent_Sounds : MonoBehaviour
    {
        [Header("Sounds")] [SerializeField] private SoundID _collectSound;
        [SerializeField] private SoundID _dropSound;
        [SerializeField] private SoundID _collectionStartSound;

        private CurrencyComponent _currencyComponent;

        private void Awake()
        {
            _currencyComponent = GetComponent<CurrencyComponent>();
        }

        private void OnEnable()
        {
            if (_currencyComponent == null)
                return;

            _currencyComponent.OnSpawned += HandleSpawned;

            _currencyComponent.OnCollectionStarted += HandleCollectionStarted;

            _currencyComponent.OnTargetReached += HandleCollected;

            _currencyComponent.OnCollectionCancelled += HandleCollectionCancelled;
        }

        private void OnDisable()
        {
            if (_currencyComponent == null)
                return;

            _currencyComponent.OnSpawned -= HandleSpawned;

            _currencyComponent.OnCollectionStarted -= HandleCollectionStarted;

            _currencyComponent.OnTargetReached -= HandleCollected;

            _currencyComponent.OnCollectionCancelled -= HandleCollectionCancelled;
        }

        private void HandleSpawned()
        {
            PlaySound(_dropSound, follow: true);
        }

        private void HandleCollectionStarted()
        {
            PlaySound(_collectionStartSound, follow: true);
        }

        private void HandleCollected(CurrencyComponent currencyComponent)
        {
            PlaySound(_collectSound, follow: false);
        }

        private void HandleCollectionCancelled()
        {
            PlaySound(_dropSound, follow: true);
        }

        private void PlaySound(SoundID sound, bool follow)
        {
            if (follow)
            {
                BroAudio.Play(sound, transform);
            }
            else
            {
                BroAudio.Play(sound, transform.position);
            }
        }
    }
}