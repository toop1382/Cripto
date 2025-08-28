using UnityEngine;
using UnityEngine.UIElements;

namespace Cripto.Game.Views.Components
{
    // Simple header that shows title and wallet balance
    public class WalletHeaderElement : VisualElement
    {
        private readonly Label _title;
        private readonly Label _wallet;

        public new class UxmlFactory : UxmlFactory<WalletHeaderElement, UxmlTraits> { }

        public WalletHeaderElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.marginBottom = 6;

            _title = new Label("Crypto Market")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18,
                    flexGrow = 1f
                }
            };
            _title.style.color = Color.white;
            Add(_title);

            _wallet = new Label("Wallet: ...")
            {
                style = { unityTextAlign = TextAnchor.MiddleRight }
            };
            _wallet.style.color = Color.white;
            Add(_wallet);
        }

        public void SetTitle(string title)
        {
            _title.text = title;
        }

        public void SetWallet(decimal balance)
        {
            _wallet.text = $"Wallet: {balance:F2} USD";
        }
    }
}
