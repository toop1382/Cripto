using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cripto.Game.Views
{
    public class MarketTriggerButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private MarketView _marketView;

        private void Start()
        {
            _marketView.DisableView();
        }

        private void OnTriggerEnter(Collider other)
        {
            button.gameObject.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            button.gameObject.SetActive(false);
        }

        public void Click()
        {
            _marketView.EnableView();

        }
    }
}