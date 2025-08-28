using System;
using System.Collections.Generic;
using System.Linq;
using Cripto.Game.Models;
using R3;

namespace Cripto.Game.Services
{
    public interface ITradingService : IDisposable
    {
        Observable<decimal> WalletBalanceStream { get; }
        Observable<IReadOnlyList<PortfolioPosition>> PortfolioStream { get; }

        decimal GetWalletBalance();
        IReadOnlyList<PortfolioPosition> GetPortfolio();

        bool Buy(string coinId, decimal quantity, out string error);
        bool Sell(string coinId, decimal quantity, out string error);
        void Hold();
    }

    public class TradingService : ITradingService
    {
        private readonly IMarketService _market;
        private readonly Subject<decimal> _walletSubject = new();
        private readonly Subject<IReadOnlyList<PortfolioPosition>> _portfolioSubject = new();
        private decimal _cash;
        private readonly Dictionary<string, PortfolioPosition> _positions = new();

        public Observable<decimal> WalletBalanceStream => _walletSubject;
        public Observable<IReadOnlyList<PortfolioPosition>> PortfolioStream => _portfolioSubject;

        public TradingService(IMarketService market)
        {
            _market = market;
            _cash = 1000m; // initial demo balance
            Push();
        }

        public decimal GetWalletBalance() => _cash;

        public IReadOnlyList<PortfolioPosition> GetPortfolio() => _positions.Values.Select(Clone).ToList();

        public bool Buy(string coinId, decimal quantity, out string error)
        {
            error = string.Empty;
            if (quantity <= 0) { error = "Quantity must be positive"; return false; }
            var price = GetPrice(coinId);
            if (price <= 0) { error = "Invalid coin or price"; return false; }
            var cost = price * quantity;
            if (cost > _cash) { error = "Insufficient cash"; return false; }

            _cash -= cost;
            if (!_positions.TryGetValue(coinId, out var pos))
            {
                pos = new PortfolioPosition { CoinId = coinId, Quantity = 0, AvgPrice = 0 };
                _positions[coinId] = pos;
            }
            // Update weighted average price
            var totalCost = pos.AvgPrice * pos.Quantity + cost;
            pos.Quantity += quantity;
            pos.AvgPrice = pos.Quantity > 0 ? totalCost / pos.Quantity : 0;

            Push();
            return true;
        }

        public bool Sell(string coinId, decimal quantity, out string error)
        {
            error = string.Empty;
            if (quantity <= 0) { error = "Quantity must be positive"; return false; }
            if (!_positions.TryGetValue(coinId, out var pos) || pos.Quantity <= 0) { error = "No position"; return false; }
            if (quantity > pos.Quantity) { error = "Not enough quantity"; return false; }
            var price = GetPrice(coinId);
            if (price <= 0) { error = "Invalid coin or price"; return false; }

            var proceeds = price * quantity;
            _cash += proceeds;
            pos.Quantity -= quantity;
            if (pos.Quantity == 0)
            {
                pos.AvgPrice = 0;
            }
            Push();
            return true;
        }

        public void Hold()
        {
            // No-op for now; positions simply fluctuate with market prices
            // Expose streams to UI to reflect value changes if needed
        }

        private decimal GetPrice(string coinId)
        {
            var snap = _market.GetSnapshot();
            var coin = snap.FirstOrDefault(c => c.Id == coinId);
            return coin?.Price ?? 0m;
        }

        private void Push()
        {
            try
            {
                _walletSubject.OnNext(_cash);
                _portfolioSubject.OnNext(_positions.Values.Select(Clone).ToList());
            }
            catch { }
        }

        private static PortfolioPosition Clone(PortfolioPosition p)
        {
            return new PortfolioPosition { CoinId = p.CoinId, Quantity = p.Quantity, AvgPrice = p.AvgPrice };
        }

        public void Dispose()
        {
#if !R3
            _walletSubject?.OnCompleted();
            _portfolioSubject?.OnCompleted();
            _walletSubject?.Dispose();
            _portfolioSubject?.Dispose();
#endif
        }
    }
}
