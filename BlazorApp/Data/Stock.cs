namespace BlazorApp.Data
{
    public class Stock
    {
        public int pos { get; set; }
        public string ticker { get; set; }

        public double spotPrice { get; set; }

        public int QtyPrev { get; set; }
        public int QtyNext { get; set; }
        public int QtyChg { get; set; }

        public int QtySum { get; set; }

    }
}