using System;
using HelloDev.UI.Default;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomLike.UI
{
    public class UI_ManagerGame : MonoBehaviour
    {
        [SerializeField] private UIContainer _pauseMenu;
        [SerializeField] private UIContainer _gameHUD;

        [SerializeField] private InputActionReference _pauseAction;

        private void Awake()
        {
            _pauseAction.action.started += OnPauseActionPerformed;
        }

        private void OnDestroy()
        {
            _pauseAction.action.started -= OnPauseActionPerformed;
        }

        private void OnEnable()
        {
            _pauseMenu.onShow.AddListener(OnPauseContainerShow);
            _gameHUD.onStartShow.AddListener(OnHUDContainerShow);
        }

        private void OnDisable()
        {
            _pauseMenu.onShow.RemoveListener(OnPauseContainerShow);
            _gameHUD.onStartShow.RemoveListener(OnHUDContainerShow);
        }

        private void OnPauseActionPerformed(InputAction.CallbackContext context)
        {
            if (_pauseMenu.IsVisible()) SetGameHUDActive(true);
            else SetPauseMenuActive(true);
        }

        public void SetPauseMenuActive(bool active)
        {
            if (active) _pauseMenu.ShowContainer();
            else _pauseMenu.HideContainer();
        }

        public void SetGameHUDActive(bool active)
        {
            if (active) _gameHUD.ShowContainer();
            else _gameHUD.HideContainer();
        }
        
        private void OnHUDContainerShow()
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnPauseContainerShow()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }
}