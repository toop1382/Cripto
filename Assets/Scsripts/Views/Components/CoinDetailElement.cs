using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Cripto.Game.Models;

namespace Cripto.Game.Views.Components
{
    /// <summary>
    /// Full-page detail UI for a single coin: title, price, holdings, large chart, Buy/Sell, and Back.
    /// Purely presentational; no business logic.
    /// </summary>
    public class CoinDetailElement : VisualElement
    {
        private const string PriceFormat = "F6";
        private static readonly Color ButtonBg = new(0.2f, 0.22f, 0.25f, 1f);
        private static readonly Color LightText = new(0.95f, 0.97f, 1f, 1f);

        private readonly Button _backBtn;
        private readonly Label _title;
        private readonly Label _price;
        private readonly Label _holdings;
        private readonly Button _buyBtn;
        private readonly Button _sellBtn;
        private readonly TextField _qtyField;
        private readonly Cripto.Game.Views.LineChartElement _chart;

        private string _coinId;
        private readonly List<float> _history;
        private readonly int _capacity;
        private Action _onBack;
        private Action<string, decimal> _onBuy;
        private Action<string, decimal> _onSell;

        public new class UxmlFactory : UxmlFactory<CoinDetailElement, UxmlTraits> { }

        public CoinDetailElement() : this(300) {}

        public CoinDetailElement(int historyCapacity)
        {
            _capacity = Mathf.Max(2, historyCapacity);
            _history = new List<float>(_capacity);
            BuildLayout();

            // Header
            var header = CreateHeader(out _backBtn, out _title);
            Add(header);

            // Info row
            var infoRow = CreateInfoRow(out _price, out _holdings, out _qtyField, out _buyBtn, out _sellBtn);
            Add(infoRow);

            // Chart
            _chart = CreateChart();
        }

        public void Bind(string coinId, string name, CoinCategory category, Action<string, decimal> onBuy, Action<string, decimal> onSell, Action onBack)
        {
            _coinId = coinId;
            _title.text = $"{name} ({coinId})";
            _onBuy = onBuy;
            _onSell = onSell;
            _onBack = onBack;
            _chart.SetColor(GetColorForCategory(category));
        }

        public void UpdatePrice(decimal price)
        {
            UpdatePriceLabel(price);
            AppendToHistory((float)price);
            TrimHistoryIfNeeded();
            _chart.SetData(_history);
        }

        public void UpdateHoldings(decimal quantity)
        {
            _holdings.text = $"مقدار: {quantity}";
        }

        /// <summary>
        /// Set initial history so the detail chart continues from the list's sparkline.
        /// </summary>
        public void SetHistory(IReadOnlyList<float> data)
        {
            _history.Clear();
            if (data != null && data.Count > 0)
            {
                // Only keep the last _capacity values
                int start = Mathf.Max(0, data.Count - _capacity);
                for (int i = start; i < data.Count; i++)
                {
                    _history.Add(data[i]);
                }
            }
            _chart.SetData(_history);
        }

        private void BuildLayout()
        {
            style.flexDirection = FlexDirection.Column;
            // style.gap is not available in this Unity version; using margins between elements instead.
            style.color = LightText; // light text on dark background
        }

        private VisualElement CreateHeader(out Button backBtn, out Label title)
        {
            var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            header.style.marginBottom = 6;

            backBtn = new Button(() => _onBack?.Invoke()) { text = "← بازگشت" };
            backBtn.style.color = Color.white;
            backBtn.style.backgroundColor = ButtonBg;
            header.Add(backBtn);

            title = new Label("ارز")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 18, flexGrow = 1f, unityTextAlign = TextAnchor.MiddleLeft }
            };
            header.Add(title);

            return header;
        }

        private VisualElement CreateInfoRow(out Label price, out Label holdings, out TextField qtyField, out Button buyBtn, out Button sellBtn)
        {
            var infoRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            price = new Label("0.000000") { style = { width = 140 } };
            price.style.marginRight = 10;
            infoRow.Add(price);

            holdings = new Label("مقدار: 0") { style = { width = 160 } };
            holdings.style.marginRight = 10;
            infoRow.Add(holdings);

            qtyField = new TextField("مقدار");
            qtyField.value = "0.1";
            qtyField.style.width = new StyleLength(StyleKeyword.Auto);
            qtyField.style.marginRight = 10;
            // Outer container uses dark background and subtle border
            qtyField.style.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
            qtyField.style.borderBottomColor = new Color(1f, 1f, 1f, 0.25f);
            qtyField.style.borderTopColor = new Color(1f, 1f, 1f, 0.15f);
            qtyField.style.borderLeftColor = new Color(1f, 1f, 1f, 0.15f);
            qtyField.style.borderRightColor = new Color(1f, 1f, 1f, 0.15f);
            qtyField.style.borderBottomWidth = 1;
            qtyField.style.borderTopWidth = 1;
            qtyField.style.borderLeftWidth = 1;
            qtyField.style.borderRightWidth = 1;
            // Label color ("Qty")
            qtyField.labelElement.style.color = LightText;
            // Inner text input styling (actual typing area)
            var input = qtyField.Q(TextField.textInputUssName);
            if (input != null)
            {
                input.style.color = LightText;
                input.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            }
            infoRow.Add(qtyField);

            buyBtn = new Button(OnBuyClicked) { text = "خرید" };
            buyBtn.style.marginRight = 10;
            buyBtn.style.color = Color.white;
            buyBtn.style.backgroundColor = ButtonBg;
            infoRow.Add(buyBtn);

            sellBtn = new Button(OnSellClicked) { text = "فروش" };
            sellBtn.style.color = Color.white;
            sellBtn.style.backgroundColor = ButtonBg;
            infoRow.Add(sellBtn);

            return infoRow;
        }

        private Cripto.Game.Views.LineChartElement CreateChart()
        {
            var chart = new Cripto.Game.Views.LineChartElement();
            chart.style.height = 120;
            chart.style.marginTop = 4;
            Add(chart);
            return chart;
        }

        private void UpdatePriceLabel(decimal price)
        {
            _price.text = price.ToString(PriceFormat);
        }

        private void AppendToHistory(float v)
        {
            _history.Add(v);
        }

        private void TrimHistoryIfNeeded()
        {
            if (_history.Count <= _capacity) return;
            int remove = _history.Count - _capacity;
            _history.RemoveRange(0, remove);
        }

        private bool TryGetQuantity(out decimal qty)
        {
            qty = 0m;
            if (_qtyField == null) return false;
            var s = _qtyField.value?.Trim();
            if (string.IsNullOrEmpty(s)) return false;
            if (!decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out qty)) return false;
            if (qty <= 0m) return false;
            return true;
        }

        private void OnBuyClicked()
        {
            if (TryGetQuantity(out var qty)) _onBuy?.Invoke(_coinId, qty);
        }

        private void OnSellClicked()
        {
            if (TryGetQuantity(out var qty)) _onSell?.Invoke(_coinId, qty);
        }

        private static Color GetColorForCategory(CoinCategory cat)
        {
            switch (cat)
            {
                case CoinCategory.LowRisk: return new Color(0.2f, 0.8f, 0.3f);
                case CoinCategory.Fake: return new Color(1.0f, 0.7f, 0.0f);
                case CoinCategory.Shitcoin: return new Color(1.0f, 0.2f, 0.2f);
                default: return Color.cyan;
            }
        }
    }
}
