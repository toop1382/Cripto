using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cripto.Game.Models;

namespace Cripto.Game.Views.Components
{
    /// <summary>
    /// One coin row: name, price, buy/sell, holdings, and sparkline.
    /// Focused, self-contained UI Toolkit element. No business logic inside.
    /// </summary>
    public class CoinRowElement : VisualElement
    {
        private const string PriceFormat = "F6";
        private const int NameWidth = 200;
        private const int PriceWidth = 110;
        private const int HoldingsWidth = 120;
        private static readonly Color ButtonBg = new(0.2f, 0.22f, 0.25f, 1f);
        private static readonly Color LightText = new(0.95f, 0.97f, 1f, 1f);

        private readonly Label _name;
        private readonly Label _price;
        private readonly Label _holdings;
        private readonly Button _buyBtn;
        private readonly Button _sellBtn;
        private readonly Cripto.Game.Views.LineChartElement _chart;

        // State
        private string _coinId;
        private CoinCategory _category;
        private readonly List<float> _history;
        private readonly int _capacity;

        // Callbacks provided by parent (MarketView)
        private Action<string> _onBuy;
        private Action<string> _onSell;

        public new class UxmlFactory : UxmlFactory<CoinRowElement, UxmlTraits> { }

        public CoinRowElement() : this(200) {}

        public CoinRowElement(int historyCapacity = 200)
        {
            _capacity = Mathf.Max(2, historyCapacity);
            _history = new List<float>(_capacity);
            BuildLayout();
            _name = CreateNameLabel();
            _price = CreatePriceLabel();
            _holdings = CreateHoldingsLabel();
            _buyBtn = CreateActionButton("Buy 10", () => _onBuy?.Invoke(_coinId));
            _sellBtn = CreateActionButton("Sell 10", () => _onSell?.Invoke(_coinId));
            _chart = CreateChart();
        }

        public void Bind(string coinId, string name, CoinCategory category, Action<string> onBuy, Action<string> onSell)
        {
            _coinId = coinId;
            _category = category;
            _name.text = $"{name} ({coinId})";
            _onBuy = onBuy;
            _onSell = onSell;
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
            _holdings.text = $"Qty: {quantity}";
        }

        // Expose a copy of current history so detail page can continue the same chart
        public IReadOnlyList<float> GetHistorySnapshot()
        {
            // Return a defensive copy to avoid external mutation
            return _history.ToArray();
        }

        private void BuildLayout()
        {
            style.flexDirection = FlexDirection.Column;
            style.marginBottom = 6;
            style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            style.borderBottomWidth = 1;
            style.color = LightText; // light text on dark background

            // top line container
            var line = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            Add(line);
        }

        private Label CreateNameLabel()
        {
            var line = this[0] as VisualElement; // first child is the line container
            var name = new Label("") { style = { width = NameWidth, unityFontStyleAndWeight = FontStyle.Bold } };
            line?.Add(name);
            return name;
        }

        private Label CreatePriceLabel()
        {
            var line = this[0] as VisualElement;
            var price = new Label("0.000000") { style = { width = PriceWidth } };
            line?.Add(price);
            return price;
        }

        private Label CreateHoldingsLabel()
        {
            var line = this[0] as VisualElement;
            var holdings = new Label("Qty: 0") { style = { width = HoldingsWidth } };
            line?.Add(holdings);
            return holdings;
        }

        private Button CreateActionButton(string text, Action onClick)
        {
            var line = this[0] as VisualElement;
            var btn = new Button(() => onClick?.Invoke()) { text = text };
            btn.style.color = Color.white;
            btn.style.backgroundColor = ButtonBg;
            line?.Add(btn);
            return btn;
        }

        private Cripto.Game.Views.LineChartElement CreateChart()
        {
            var chart = new Cripto.Game.Views.LineChartElement
            {
                style =
                {
                    height = 40,
                    marginTop = 2
                }
            };
            Add(chart);
            return chart;
        }

        private void UpdatePriceLabel(decimal price)
        {
            _price.text = price.ToString(PriceFormat);
        }

        private void AppendToHistory(float value)
        {
            _history.Add(value);
        }

        private void TrimHistoryIfNeeded()
        {
            if (_history.Count <= _capacity) return;
            int remove = _history.Count - _capacity;
            _history.RemoveRange(0, remove);
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
