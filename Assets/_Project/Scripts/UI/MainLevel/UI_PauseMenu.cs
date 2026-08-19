using HelloDev.UI.Default;
using KingdomLike.System.Loader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace KingdomLike.UI
{
    public class UI_PauseMenu : MonoBehaviour
    {
        [SerializeField] private UIContainer _pauseContainer;
        [SerializeField] UI_ManagerGame _managerGame;

        [SerializeField] private SceneLoaderLocatorSO _locator;
        [SerializeField] private AssetReference[] ScenesToLoad;

        [SerializeField] private UI_Button _mainMenuButton;
        [SerializeField] private UI_Button _resumeButton;

        private void OnEnable()
        {
            _mainMenuButton.OnClick.AddListener(OnMainMenuButtonClicked);
            _resumeButton.OnClick.AddListener(OnResumeButtonClicked);
        }
        
        private void OnDisable()
        {
            _mainMenuButton.OnClick.RemoveListener(OnMainMenuButtonClicked);
            _resumeButton.OnClick.RemoveListener(OnResumeButtonClicked);
        }
        
        private void OnResumeButtonClicked()
        {
            _managerGame.SetGameHUDActive(true);
        }

        private void OnMainMenuButtonClicked()
        {
            _locator.Get().LoadSceneAsyncForget(ScenesToLoad, LoadSceneMode.Additive, true, true, true);
        }
    }
}