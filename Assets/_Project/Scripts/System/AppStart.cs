using HelloDev.Logging;
using KingdomLike.System.Loader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace KingdomLike.System
{
    public class AppStart : MonoBehaviour
    {
        [SerializeField] private SceneLoaderLocatorSO _locator;
        [SerializeField] private AssetReference[] ScenesToLoad;

        [Header("Logger")] [SerializeField] private LoggerSettings_SO _loggerSettings;

        private void Awake()
        {
            _loggerSettings?.ApplyToLogger();
        }

        private void Start()
        {
            _locator.Get().LoadSceneAsyncForget(ScenesToLoad, LoadSceneMode.Additive, true, true, true);
        }
    }
}
