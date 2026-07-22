using System;
using Ami.BroAudio;
using Ami.BroAudio.Runtime;
using MalbersAnimations.Controller;
using MalbersAnimations.HAP;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Core
{
    public class Horse_Sounds : MonoBehaviour
    {
        private const string DEBUG_PREFIX = "[Horse_Sounds]";

        [FoldoutGroup("References")]
        [SerializeField]
        private Mount _mount;

        [FoldoutGroup("References")]
        [SerializeField]
        private MAnimal _animalController;

        [FoldoutGroup("Sound IDs")]
        [SerializeField]
        private SoundID _onMountSoundID;

        [FoldoutGroup("Sound IDs")]
        [SerializeField]
        private SoundID _onDismountSoundID;

        [FoldoutGroup("Sound IDs")]
        [SerializeField]
        private SoundID _onSprintEnabledSoundID;

        [FoldoutGroup("Sound IDs")]
        [SerializeField]
        private SoundID _onSprintDisabledSoundID;

        [FoldoutGroup("Sound IDs")]
        [SerializeField]
        private SoundID _breathingSoundID;

        [FoldoutGroup("Sound IDs")]
        [SerializeField]
        private SoundID _randomSoundID;

        [FoldoutGroup("Breathing")]
        [Tooltip("Whether the horse should breathe while it is in an active gameplay state.")]
        [SerializeField]
        private bool _breathingEnabled = true;

        [FoldoutGroup("Breathing")]
        [Tooltip("Whether breathing should continue while the horse is sprinting.")]
        [SerializeField]
        private bool _breathingWhileSprinting = true;

        [FoldoutGroup("Random Sounds")]
        [Tooltip("Minimum and maximum delay between random horse sounds.")]
        [SerializeField]
        [MinMaxSlider(1f, 120f, true)]
        private Vector2 _randomSoundDelayRange = new(10f, 30f);

        [FoldoutGroup("Random Sounds")]
        [Tooltip("Random sounds are only played while the horse is mounted.")]
        [SerializeField]
        private bool _randomSoundsOnlyWhileMounted;

        [FoldoutGroup("Random Sounds")]
        [Tooltip("Random sounds are only played while the horse is not sprinting.")]
        [SerializeField]
        private bool _randomSoundsOnlyWhileNotSprinting = true;

        [FoldoutGroup("Random Sounds")]
        [Tooltip("Whether the random sound timer should start when this component is enabled.")]
        [SerializeField]
        private bool _scheduleRandomSoundOnEnable = true;

        private IAudioPlayer _breathingPlayer;

        private float _nextRandomSoundTime;

        private bool _isMounted;
        private bool _isSprinting;
        private bool _isDead;

        private void Awake()
        {
            if (_mount == null)
                _mount = GetComponent<Mount>();

            if (_animalController == null)
                _animalController = GetComponent<MAnimal>();
        }

        private void OnEnable()
        {
            SubscribeToEvents();

            _isMounted = false;
            _isSprinting = false;
            _isDead = false;

            if (_scheduleRandomSoundOnEnable)
            {
                ScheduleNextRandomSound();
            }

            UpdateBreathingState();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            StopBreathing();
        }
        

        private void Update()
        {
            HandleRandomSounds();
        }

        #region Events

        private void SubscribeToEvents()
        {
            if (_mount != null)
            {
                _mount.OnMounted.AddListener(OnMounted);
                _mount.OnDismounted.AddListener(OnDismounted);
            }

            if (_animalController != null)
            {
                _animalController.OnSprintEnabled.AddListener(OnSprintEnabled);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_mount != null)
            {
                _mount.OnMounted.RemoveListener(OnMounted);
                _mount.OnDismounted.RemoveListener(OnDismounted);
            }

            if (_animalController != null)
            {
                _animalController.OnSprintEnabled.RemoveListener(OnSprintEnabled);
            }
        }

        private void OnMounted(GameObject rider)
        {
            _isMounted = true;

            PlaySound(_onMountSoundID);

            UpdateBreathingState();
        }

        private void OnDismounted(GameObject rider)
        {
            _isMounted = false;

            PlaySound(_onDismountSoundID);

            UpdateBreathingState();
        }

        private void OnSprintEnabled(bool enabled)
        {
            _isSprinting = enabled;

            if (enabled)
            {
                PlaySound(_onSprintEnabledSoundID);
            }
            else
            {
                PlaySound(_onSprintDisabledSoundID);
            }

            UpdateBreathingState();
        }

        #endregion

        #region Breathing

        private void UpdateBreathingState()
        {
            if (!ShouldBeBreathing())
            {
                StopBreathing();
                return;
            }

            StartBreathing();
        }

        private bool ShouldBeBreathing()
        {
            if (!_breathingEnabled)
                return false;

            if (_isDead)
                return false;

            if (!isActiveAndEnabled)
                return false;

            if (_animalController == null || !_animalController.enabled)
                return false;

            if (_isSprinting && !_breathingWhileSprinting)
                return false;

            return true;
        }

        private void StartBreathing()
        {
            if (_breathingPlayer != null)
                return;

            if (!_breathingSoundID.IsValid())
                return;

            _breathingPlayer = BroAudio.Play(
                _breathingSoundID,
                transform);
        }

        private void StopBreathing()
        {
            if (_breathingPlayer == null)
                return;
            if (SoundManager.Instance) _breathingPlayer.Stop();
            _breathingPlayer = null;
        }

        #endregion

        #region Random Sounds

        private void HandleRandomSounds()
        {
            if (Time.time < _nextRandomSoundTime)
                return;

            ScheduleNextRandomSound();

            if (!CanPlayRandomSound())
                return;

            PlaySound(_randomSoundID);
        }

        private bool CanPlayRandomSound()
        {
            if (!isActiveAndEnabled)
                return false;

            if (_animalController == null || !_animalController.enabled)
                return false;

            if (_isDead)
                return false;

            if (_randomSoundsOnlyWhileMounted && !_isMounted)
                return false;

            if (_randomSoundsOnlyWhileNotSprinting && _isSprinting)
                return false;

            return true;
        }

        private void ScheduleNextRandomSound()
        {
            float delay = UnityEngine.Random.Range(
                _randomSoundDelayRange.x,
                _randomSoundDelayRange.y);

            _nextRandomSoundTime = Time.time + delay;
        }

        #endregion

        #region Playback

        private void PlaySound(SoundID soundID)
        {
            if (!soundID.IsValid())
                return;

            BroAudio.Play(
                soundID,
                transform);
        }

        #endregion
    }
}