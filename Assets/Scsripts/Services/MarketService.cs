using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cripto.Game.Models;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cripto.Game.Services
{
    public interface IMarketService : IDisposable
    {
        Observable<IReadOnlyList<Coin>> CoinsStream { get; }
        void Start(CancellationToken token);
        IReadOnlyList<Coin> GetSnapshot();
    }

    public class MarketService : IMarketService
    {
        private readonly Subject<IReadOnlyList<Coin>> _coinsSubject = new();

        public Observable<IReadOnlyList<Coin>> CoinsStream => _coinsSubject;

        private readonly List<Coin> _coins;
        private Task _loop;
        private CancellationTokenSource _linkedCts;

        public MarketService()
        {
            // Seed some sample coins across categories
            _coins = new List<Coin>
            {
                new Coin { Id = "SC1", Name = "DogeFlop", Category = CoinCategory.Shitcoin, Price = 0.0005m },
                new Coin { Id = "SC2", Name = "MoonRug", Category = CoinCategory.Fake, Price = 0.003m },
                new Coin { Id = "LR1", Name = "BlueCoin", Category = CoinCategory.LowRisk, Price = 100m },
                new Coin { Id = "LR2", Name = "StableX", Category = CoinCategory.LowRisk, Price = 1m },
                new Coin { Id = "SC3", Name = "Pepe2.0", Category = CoinCategory.Shitcoin, Price = 0.00009m },
            };
        }

        public void Start(CancellationToken token)
        {
            if (_loop != null && !_loop.IsCompleted) return;
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
             _ = PriceLoop(_linkedCts.Token);
        }

        private async Task PriceLoop(CancellationToken ct)
        {
            // Simple random-walk price simulation per tick
            while (!ct.IsCancellationRequested)
            {
                foreach (var c in _coins)
                {
                    // Volatility by category
                    float vol = c.Category switch
                    {
                        CoinCategory.LowRisk => 0.0025f,
                        CoinCategory.Fake => 0.05f,
                        CoinCategory.Shitcoin => 0.15f,
                        _ => 0.01f
                    };
                    var drift = 1f + Random.Range(-vol, vol);
                    var newPrice = Mathf.Max(0.0000001f, (float)c.Price * drift);
                    c.Price = (decimal)newPrice;
                }

                // Push snapshot to observers
                try
                {
                    _coinsSubject.OnNext(_coins.Select(x => new Coin
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Category = x.Category,
                        Price = x.Price
                    }).ToList());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                await Task.Delay(500, ct);
            }
        }

        public IReadOnlyList<Coin> GetSnapshot() => _coins.Select(x => new Coin
        {
            Id = x.Id,
            Name = x.Name,
            Category = x.Category,
            Price = x.Price
        }).ToList();

        public void Dispose()
        {
            try
            {
                _linkedCts?.Cancel();
            }
            catch
            {
            }

            _linkedCts?.Dispose();
#if !R3
            _coinsSubject?.OnCompleted();
            _coinsSubject?.Dispose();
#endif
        }
    }
}