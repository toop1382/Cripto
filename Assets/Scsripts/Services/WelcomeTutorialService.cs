using UnityEngine;
using Cripto.Game.Views;
using VContainer;

namespace Cripto.Game.Services
{
    /// <summary>
    /// Manages first-run welcome/tutorial flow and persistence.
    /// Now supports a guide step: walk to the computer before showing steps.
    /// </summary>
    public interface IWelcomeTutorialService
    {
        bool HasSeen { get; }
        void ShowIfNeeded();
        void NotifyReachedComputer();
        void MarkSeen();
    }

    public class WelcomeTutorialService : IWelcomeTutorialService
    {
        private const string PlayerPrefsKey = "Cripto_WelcomeTutorialShown";
        [Inject] private WelcomeTutorialView _view;
        private bool _waitingForComputer;

        public bool HasSeen => PlayerPrefs.GetInt(PlayerPrefsKey, 0) == 1;

        public void ShowIfNeeded()
        {
            if (HasSeen) return;
            EnsureView();
            _waitingForComputer = true;
            _view.ShowIntro();
        }

        public void NotifyReachedComputer()
        {
            if (HasSeen) return;
            EnsureView();
            if (_waitingForComputer)
            {
                _waitingForComputer = false;
                _view.ShowSteps();
            }
        }

        public void MarkSeen()
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, 1);
            PlayerPrefs.Save();
        }

        private void EnsureView()
        {
            _view.Construct(this);
        }
    }
}