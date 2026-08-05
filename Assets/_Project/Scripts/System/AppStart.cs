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
        private void Start()
        {
            foreach (AssetReference scene in ScenesToLoad)
            {
                _locator.Get().LoadSceneAsyncForget(scene, LoadSceneMode.Additive, true, false, true);
            }
        }
    }
}
