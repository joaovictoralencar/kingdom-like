using HelloDev.Loaders;
using UnityEngine;

namespace KingdomLike.System.Loader
{
    public class SceneLoaderManager : SceneLoader
    {
        [SerializeField] private SceneLoaderLocatorSO _locator;

        private void Awake() => _locator.Register(this, gameObject);
        private void OnDestroy() => _locator.Unregister(this);
    }
}
