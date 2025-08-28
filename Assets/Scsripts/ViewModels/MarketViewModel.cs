using System;
using System.Collections.Generic;
using System.Threading;
using Cripto.Game.Models;
using Cripto.Game.Services;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using R3;
namespace Cripto.Game.ViewModels
{
    // MVVM ViewModel for market overview + simple trading
    public class MarketViewModel : IInitializable, IDisposable
    {
        private readonly IMarketService _marketService;
        private readonly ITradingService _tradingService;
        private readonly CancellationTokenSource _cts = new();

        public Observable<IReadOnlyList<Coin>> Coins => _marketService.CoinsStream;
        public Observable<decimal> Wallet => _tradingService.WalletBalanceStream;
        public Observable<IReadOnlyList<PortfolioPosition>> Portfolio => _tradingService.PortfolioStream;

        [Inject]
        public MarketViewModel(IMarketService marketService, ITradingService tradingService)
        {
            _marketService = marketService;
            _tradingService = tradingService;
        }

        public void Initialize()
        {
            // Start market simulation when scope starts
            _marketService.Start(_cts.Token);
            Debug.Log("MarketViewModel initialized.");
        }

        public bool Buy(string coinId, decimal qty, out string error) => _tradingService.Buy(coinId, qty, out error);
        public bool Sell(string coinId, decimal qty, out string error) => _tradingService.Sell(coinId, qty, out error);
        public void Hold() => _tradingService.Hold();

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
