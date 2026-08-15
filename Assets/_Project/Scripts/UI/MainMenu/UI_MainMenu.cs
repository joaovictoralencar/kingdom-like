using KingdomLike.System.Loader;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace KingdomLike.UI
{
    public class UI_MainMenu : MonoBehaviour
    {
        [SerializeField] private SceneLoaderLocatorSO _locator;

        [Header("Scenes")] [SerializeField] private AssetReference[] ScenesToLoad;

        [Space(15)] [FoldoutGroup("Button References")] [SerializeField]
        private UI_Button _startButton;

        [FoldoutGroup("Button References")] [SerializeField]
        private UI_Button _quitButton;

        private void OnEnable()
        {
            _startButton.OnClick.AddListener(StartGame);
            _quitButton.OnClick.AddListener(QuitGame);
            _startButton.Select();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            if (_startButton != null) _startButton?.OnClick?.RemoveListener(StartGame);
            if (_quitButton != null) _quitButton?.OnClick?.RemoveListener(QuitGame);
        }

        public void StartGame()
        {
            _locator.Get().LoadSceneAsyncForget(ScenesToLoad, LoadSceneMode.Additive, true, true, true);
        }

        public void QuitGame()
        {
            if (Application.isEditor)
            {
                EditorApplication.ExitPlaymode();
            }
            else Application.Quit();
        }
    }
}