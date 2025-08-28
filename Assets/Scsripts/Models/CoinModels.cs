using System;

namespace Cripto.Game.Models
{
    public enum CoinCategory
    {
        Shitcoin,
        Fake,
        LowRisk
    }

    [Serializable]
    public class Coin
    {
        public string Id;
        public string Name;
        public CoinCategory Category;
        public decimal Price;
    }

    [Serializable]
    public class PortfolioPosition
    {
        public string CoinId;
        public decimal Quantity;
        public decimal AvgPrice;
    }
}
