using KingdomLike.System.Loader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace KingdomLike.UI
{
    public class UI_ManagerGame : MonoBehaviour
    {
        [SerializeField] private UI_Button _mainMenuButton;
        [SerializeField] private SceneLoaderLocatorSO _locator;
        [SerializeField] private AssetReference[] ScenesToLoad;
        
        private void OnEnable()
        {
            _mainMenuButton.OnClick.AddListener(OnMainMenuButtonClicked);
        }

        private void OnMainMenuButtonClicked()
        {
            _locator.Get().LoadSceneAsyncForget(ScenesToLoad, LoadSceneMode.Additive, true, true, true);
        }

        private void OnDisable()
        {
            _mainMenuButton.OnClick.RemoveListener(OnMainMenuButtonClicked);
        }
    }
}