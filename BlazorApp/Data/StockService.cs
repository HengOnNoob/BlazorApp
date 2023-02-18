using BlazorApp.Hubs;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.JavaScript;

namespace BlazorApp.Data
{
    public class StockService
    {
        private readonly SemaphoreSlim _StateLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _UpdateLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, Stock> _stocks = new ConcurrentDictionary<string, Stock>();
        private readonly Subject<Stock> _subject = new Subject<Stock>();
        private Timer _timer;
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(250);
        private volatile bool _updating;

        public StockService(IHubContext<StockHub> hub)
        {
            Hub = hub;
            LoadStocks();

        }

        private IHubContext<StockHub> Hub
        {
            get;
            set;
        }

        public List<Stock> GetAllStocks()
        {
            return (List<Stock>)_stocks.Values.OrderBy(x => x.pos).ToList();
        }

        private void LoadStocks()
        {
            _stocks.Clear();

            var stocks = new List<Stock>
            {
                new Stock { pos = 1, ticker = "MSFT", spotPrice = 156.5, QtyPrev = 1500, QtyNext = 0 },
                new Stock { pos = 2, ticker = "AAPL", spotPrice = 137.25, QtyPrev = 2500, QtyNext = 0 },
                new Stock { pos = 3, ticker = "GOOG", spotPrice = 256.5, QtyPrev = 3500, QtyNext = 0 },
                new Stock { pos = 4, ticker = "TSLA", spotPrice = 86.35, QtyPrev = 4500, QtyNext = 0 },
                new Stock { pos = 5, ticker = "SPY", spotPrice = 556.45, QtyPrev = 5500, QtyNext = 0 }
            };

            stocks.ForEach(stock => _stocks.TryAdd(stock.ticker, stock));
        }
        public async Task StartStockMovement()
        {
            await _StateLock.WaitAsync();
            try
            {
                _timer = new Timer(UpdateStock, null, 250, 250);

            }
            finally
            {
                _StateLock.Release();
            }
        }

        public async Task StopStockMovement()
        {
            await _StateLock.WaitAsync();
            try
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }

            }
            finally
            {
                _StateLock.Release();
            }
        }
        public async Task Reset()
        {
            await _StateLock.WaitAsync();
            try
            {
                LoadStocks();

            }
            finally
            {
                _StateLock.Release();
            }
        }
        private async void UpdateStock(object state)
        {
            await _UpdateLock.WaitAsync();
            try
            {
                if (!_updating)
                {
                    _updating = true;

                    foreach (var stock in _stocks.Values)
                    {
                        TryUpdateStockPrice(stock);

                        _subject.OnNext(stock);
                    }

                    _updating = false;
                }
            }
            finally
            {
                _UpdateLock.Release();
            }
        }

        private async Task TryUpdateStockPrice(Stock stock)
        {
            await _UpdateLock.WaitAsync();
            try
            {
                Random _updateRnd = new Random();
                var r = _updateRnd.NextDouble();
                if (r <= 0.3)
                {
                    Random _rnd = new Random();
                    // Random update the stock QTY
                    int change = _rnd.Next(1, 50) * 100;
                    stock.QtyPrev = stock.QtyNext;
                    stock.QtyNext = change;
                    stock.QtyChg = change - stock.QtyPrev;
                }
                // Decide if update the spot price
                _updateRnd = new Random();
                r = _updateRnd.NextDouble();
                if (r <= 0.1)
                {
                    Random _priceRnd = new Random((int)Math.Floor(stock.spotPrice));
                    var percentChange = _priceRnd.NextDouble() * 0.002;
                    var pos = _priceRnd.NextDouble() > 0.51;
                    var pirce_change = Math.Round(stock.spotPrice * (double)percentChange, 2);
                    pirce_change = pos ? pirce_change : -pirce_change;

                    stock.spotPrice += pirce_change;
                    stock.spotPrice = Math.Round(stock.spotPrice, 2);
                }
            }
            finally
            {
                _UpdateLock.Release();
            }
        }
    };

    //public Task<StockPos[]> GetForecastAsync(DateOnly startDate)
    //{
    //    return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //    {
    //        Date = startDate.AddDays(index),
    //        TemperatureC = Random.Shared.Next(-20, 55),
    //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //    }).ToArray());
    //}
}
