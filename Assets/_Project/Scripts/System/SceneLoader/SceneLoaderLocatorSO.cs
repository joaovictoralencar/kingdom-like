using HelloDev.Loader;
using HelloDev.Utils.Locator.Locator;
using UnityEngine;

namespace KingdomLike.System.Loader
{
    [CreateAssetMenu(fileName = "SO_Locator_SceneLoader", menuName = "KingdomLike/Scriptable Objects/Locators/Scene Loader")]
    public class SceneLoaderLocatorSO : ServiceLocatorSO<ISceneLoader> { }
}
