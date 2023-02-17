using BlazorApp.Data;
using Microsoft.AspNetCore.SignalR;


namespace BlazorApp.Hubs
{
    public class StockHub : Hub
    {
        private readonly StockService _stock;
        public StockHub(StockService stock)
        {
            _stock = stock;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public IEnumerable<Stock> GetAllStocks()
        {
            return _stock.GetAllStocks();
        }

        public async Task StartStockMovement()
        {
            await _stock.StartStockMovement();
        }

        public async Task StopStockMovement()
        {
            await _stock.StopStockMovement();
        }

        public async Task Reset()
        {
            await _stock.Reset();
        }
    }
}
