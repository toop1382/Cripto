using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cripto.Game.Services;

namespace Cripto.Game.Views
{
    public class MarketTriggerButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private MarketView _marketView;
        [Inject] private IWelcomeTutorialService _tutorialService;

        private void Start()
        {
            if (_marketView != null)
                _marketView.DisableView();

            if (button != null)
                button.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (button != null)
                button.gameObject.SetActive(true);

            // Notify tutorial that player reached the computer
            _tutorialService?.NotifyReachedComputer();
        }

        private void OnTriggerExit(Collider other)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }

        public void Click()
        {
            _marketView?.EnableView();
        }
    }
}