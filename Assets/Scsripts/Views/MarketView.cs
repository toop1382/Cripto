using System;
using System.Collections.Generic;
using System.Linq;
using Cripto.Game.Models;
using Cripto.Game.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using R3;
using Cripto.Game.Views.Components;

namespace Cripto.Game.Views
{
    /// <summary>
    /// MarketView orchestrates UI components and binds them to the ViewModel.
    /// The logic remains unchanged; this class focuses on composition and wiring.
    /// </summary>
    public class MarketView : MonoBehaviour
    {
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private int historyCapacity = 200;

        private MarketViewModel _vm;
        private IDisposable _coinsSub;
        private IDisposable _walletSub;
        private IDisposable _portfolioSub;

        // UI Toolkit
        private UIDocument _document;
        private VisualElement _root;
        private WalletHeaderElement _header;
        private VisualElement _content; // swaps between list and detail pages
        private ScrollView _coinsScroll;
        private CoinDetailElement _detailPage;

        private readonly Dictionary<string, CoinRowElement> _rows = new();

        private IReadOnlyList<Coin> _lastCoins = Array.Empty<Coin>();
        private decimal _wallet;
        private IReadOnlyList<PortfolioPosition> _portfolio = Array.Empty<PortfolioPosition>();
        private string _selectedCoinId = null;

        [Inject]
        public void Construct(MarketViewModel vm)
        {
            _vm = vm;
        }

        private void OnEnable()
        {
            if (_vm == null) return;
            SetupDocumentAndUI();
            SubscribeToViewModel();
        }

        private void OnDisable()
        {
            UnsubscribeFromViewModel();
        }

        private void OnCoins(IReadOnlyList<Coin> coins)
        {
            _lastCoins = coins;
            LogTick(coins);
            HandleCoinsUpdate(coins);
        }

        #region Wiring / Subscriptions
        private void SubscribeToViewModel()
        {
            _coinsSub = _vm.Coins.Subscribe(OnCoins);
            _walletSub = _vm.Wallet.Subscribe(v =>
            {
                _wallet = v;
                UpdateWallet();
            });
            _portfolioSub = _vm.Portfolio.Subscribe(p =>
            {
                _portfolio = p;
                UpdateHoldingsForAll();
            });
        }

        private void UnsubscribeFromViewModel()
        {
            _coinsSub?.Dispose();
            _walletSub?.Dispose();
            _portfolioSub?.Dispose();
            _coinsSub = null;
            _walletSub = null;
            _portfolioSub = null;
        }
        #endregion

        #region UI Build
        private void SetupDocumentAndUI()
        {
            _document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            BuildUI();
        }

        private void BuildUI()
        {
            _root = _document.rootVisualElement;
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.paddingLeft = 8;
            _root.style.paddingRight = 8;
            _root.style.paddingTop = 8;
            _root.style.paddingBottom = 8;
            _root.style.backgroundColor = Color.black; // dark backdrop on root
            AddBackdrop(_root);

            _header = new WalletHeaderElement();
            _root.Add(_header);

            _content = new VisualElement
            {
                style =
                {
                    flexGrow = 1f,
                    flexDirection = FlexDirection.Column
                }
            };
            _root.Add(_content);

            _coinsScroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1f } };
            _content.Add(_coinsScroll);

            UpdateWallet();
        }

        private static void AddBackdrop(VisualElement root)
        {
            // Absolute black panel behind everything
            var backdrop = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    right = 0,
                    bottom = 0,
                    backgroundColor = Color.black
                },
                pickingMode = PickingMode.Ignore // don't block input
            };
            root.Add(backdrop);
        }
        #endregion

        #region Coins updates
        private void HandleCoinsUpdate(IReadOnlyList<Coin> coins)
        {
            if (_content == null) return;

            if (MaybeUpdateSelectedCoinDetail(coins))
                return;

            if (_coinsScroll == null) return;

            // Build or update rows
            foreach (var coin in coins)
            {
                EnsureCoinRow(coin);
                _rows[coin.Id].UpdatePrice(coin.Price);
            }
        }

        private bool MaybeUpdateSelectedCoinDetail(IReadOnlyList<Coin> coins)
        {
            if (string.IsNullOrEmpty(_selectedCoinId)) return false;

            // Update only selected coin detail
            var coin = coins.FirstOrDefault(c => c.Id == _selectedCoinId);
            if (coin != null && _detailPage != null)
            {
                _detailPage.UpdatePrice(coin.Price);
            }
            return true;
        }
        #endregion

        #region Header / Wallet
        private void UpdateWallet()
        {
            _header?.SetWallet(_wallet);
        }
        #endregion

        #region Page switching
        private void ShowListPage()
        {
            _selectedCoinId = null;
            _detailPage = null;
            _content.Clear();
            _content.Add(_coinsScroll);
            UpdateHoldingsForAll();
        }

        private void ShowDetailPage(Coin coin)
        {
            _selectedCoinId = coin.Id;
            _detailPage = new CoinDetailElement(Mathf.Max(200, historyCapacity));
            _detailPage.Bind(
                coin.Id,
                coin.Name,
                coin.Category,
                onBuy: id => { if (!_vm.Buy(id, 10m, out var err) && logToConsole) Debug.LogWarning($"Buy failed: {err}"); },
                onSell: id => { if (!_vm.Sell(id, 10m, out var err) && logToConsole) Debug.LogWarning($"Sell failed: {err}"); },
                onBack: ShowListPage
            );

            // Seed chart with the same history used in the list row so it continues seamlessly
            if (_rows.TryGetValue(coin.Id, out var row))
            {
                _detailPage.SetHistory(row.GetHistorySnapshot());
            }

            // Seed current values
            _detailPage.UpdatePrice(coin.Price);
            _detailPage.UpdateHoldings(GetQuantityForCoin(coin.Id));

            _content.Clear();
            _content.Add(_detailPage);
        }
        #endregion

        #region Rows / Holdings
        private void EnsureCoinRow(Coin coin)
        {
            if (_rows.ContainsKey(coin.Id)) return;

            var row = new CoinRowElement(historyCapacity);
            row.Bind(
                coin.Id,
                coin.Name,
                coin.Category,
                onBuy: id => { if (!_vm.Buy(id, 10m, out var err) && logToConsole) Debug.LogWarning($"Buy failed: {err}"); },
                onSell: id => { if (!_vm.Sell(id, 10m, out var err) && logToConsole) Debug.LogWarning($"Sell failed: {err}"); }
            );

            // Initial holdings set if already available
            var qty = GetQuantityForCoin(coin.Id);
            row.UpdateHoldings(qty);

            _coinsScroll.Add(row);
            _rows[coin.Id] = row;

            // Open detail page when row (not its buttons) is clicked
            row.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is Button) return; // ignore button clicks
                var current = _lastCoins.FirstOrDefault(c => c.Id == coin.Id);
                if (current != null) ShowDetailPage(current);
            });
        }

        private void UpdateHoldingsForAll()
        {
            if (!string.IsNullOrEmpty(_selectedCoinId))
            {
                var qtySel = GetQuantityForCoin(_selectedCoinId);
                _detailPage?.UpdateHoldings(qtySel);
                return;
            }

            foreach (var kv in _rows)
            {
                var qty = GetQuantityForCoin(kv.Key);
                kv.Value.UpdateHoldings(qty);
            }
        }

        private decimal GetQuantityForCoin(string coinId)
        {
            if (_portfolio == null) return 0m;
            for (int i = 0; i < _portfolio.Count; i++)
            {
                if (_portfolio[i].CoinId == coinId) return _portfolio[i].Quantity;
            }
            return 0m;
        }
        #endregion

        #region Logging
        private void LogTick(IReadOnlyList<Coin> coins)
        {
            if (!logToConsole) return;
            var top = string.Join(", ", coins.Take(5).Select(c => $"{c.Name}:{c.Price:F6}"));
            Debug.Log($"[MarketView] Tick => {top}");
        }
        #endregion
    }
}