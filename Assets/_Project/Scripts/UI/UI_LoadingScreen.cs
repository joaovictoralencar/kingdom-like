using System;
using HelloDev.Loader;
using UnityEngine;

namespace KingdomLike.UI
{
    public class UI_LoadingScreen : MonoBehaviour, ILoadingScreen
    {
        [SerializeField] private Camera _cam;
        private void OnEnable()
        {
            if (Camera.main == null)
            {
                _cam.gameObject.SetActive(true);
                Camera.SetupCurrent(_cam);
            } else if (_cam != null && _cam.gameObject.activeSelf)
            {
                _cam.gameObject.SetActive(false);
            }
        }

        public void OnStart(string sceneName)
        {
        }

        public void OnProgress(float progress)
        {
        }

        public void OnComplete(string sceneName)
        {
        }
    }
}
