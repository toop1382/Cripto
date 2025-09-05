using UnityEngine;
using VContainer;
using Cripto.Game.Views;

namespace Cripto.Game.Services
{
    /// <summary>
    /// Manages first-run welcome/tutorial flow and persistence.
    /// </summary>
    public interface IWelcomeTutorialService
    {
        bool HasSeen { get; }
        void ShowIfNeeded();
        void MarkSeen();
    }

    public class WelcomeTutorialService : IWelcomeTutorialService
    {
        private const string PlayerPrefsKey = "Cripto_WelcomeTutorialShown";
        [Inject] private WelcomeTutorialView _view;

        public bool HasSeen => PlayerPrefs.GetInt(PlayerPrefsKey, 0) == 1;

        public void ShowIfNeeded()
        {
            if (HasSeen) return;
            if (_view == null)
            {
                var go = new GameObject("WelcomeTutorialView");
                Object.DontDestroyOnLoad(go);
                _view = go.AddComponent<WelcomeTutorialView>();
                _view.Construct(this);
            }

            _view.Show();
        }

        public void MarkSeen()
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, 1);
            PlayerPrefs.Save();
        }
    }
}