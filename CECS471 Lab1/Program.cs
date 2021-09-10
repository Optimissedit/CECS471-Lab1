using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Stock
{
    public class Stock
    {
        public event EventHandler<StockNotification> StockEvent;

        private readonly Thread _thread;
        public string StockName { get => StockName; set => StockName = value; }
        public int InitialValue { get => InitialValue; set => InitialValue = value; }
        public int CurrentValue { get => CurrentValue; set => CurrentValue = value; }
        public int MaxChange { get => MaxChange; set => MaxChange = value; }
        public int Threshold { get => Threshold; set => Threshold = value; }
        public int NumChanges { get => NumChanges; set => NumChanges = value; }

        /// <summary>
        /// Stock class that contains all the information and changes of the stock
        /// </summary>
        /// <param name="name">Stock name</param>
        /// <param name="startingValue">Starting stock value</param>
        /// <param name="maxChange">The max value change of the stock</param>
        /// <param name="threshold">The range for the stock</param>
        public Stock(string name, int startingValue, int maxChange, int threshold)
        {
            StockName = name;
            InitialValue = startingValue;
            MaxChange = maxChange;
            Threshold = threshold;
            NumChanges = 0;
            CurrentValue = startingValue;

            _thread = new Thread(new ThreadStart(Activate));
            _thread.Start();
        }

        /// <summary>
        /// Activates the threads synchronizations
        /// </summary>
        public void Activate()
        {
            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(500); // 1/2 second
                //Call the function ChangeStockValue
                ChangeStockValue();
            }
        }
        /// <summary>
        /// Changes the stock value and also raising the event of stock value changes
        /// </summary>
        public void ChangeStockValue()
        {
            var rand = new Random();
            CurrentValue += rand.Next(0, MaxChange);
            NumChanges++;

            if ((CurrentValue - InitialValue) > Threshold)
            {
                StockEvent?.Invoke(this, new StockNotification(StockName, CurrentValue, NumChanges));
            }
        }
    }

    public class StockBroker
    {
        public string BrokerName { get => BrokerName; set => BrokerName = value; }

        public List<Stock> stocks = new List<Stock>();
        public static ReaderWriterLockSlim myLock = new ReaderWriterLockSlim(); readonly string docPath = @"C:\Users\TeaLAUREY\Bureau\CECS 475\Lab1_output.txt";
        public string titles = "Broker".PadRight(10) + "Stock".PadRight(15) +
"Value".PadRight(10) + "Changes".PadRight(10) + "Date and Time";

        /// <summary>
        /// The stockbroker object
        /// </summary>
        /// <param name="brokerName">The stockbroker's name</param>
        public StockBroker(string brokerName)
        {
            BrokerName = brokerName;
        }

        /// <summary>
        /// Adds stock objects to the stock list
        /// </summary>
        /// <param name="stock">Stock object</param>
        public void AddStock(Stock stock)
        {
            stocks.Add(stock);
            stock.StockEvent += EventHandler;
        }

        /// <summary>
        /// The eventhandler that raises the event of a change
        /// </summary>
        /// <param name="sender">The sender that indicated a change</param>
        /// <param name="e">Event arguments</param>
        void EventHandler(Object sender, EventArgs e)
        {
            myLock.EnterWriteLock();
            Stock nStock = (Stock)sender;
            string output = BrokerName.PadRight(10) + nStock.StockName.PadRight(12) + nStock.CurrentValue.ToString().PadRight(7) + nStock.NumChanges;

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "output.txt"), true))
            { 
                outputFile.WriteLine(output);
            }

            Console.WriteLine(output);
            myLock.ExitWriteLock();
        }
    }

    public class StockNotification : EventArgs
    {

        public string StockName { get => StockName; set => StockName = value; }
        public int CurrentValue { get => CurrentValue; set => CurrentValue = value; }
        public int NumChanges { get => NumChanges; set => NumChanges = value; }
        /// <summary>
        /// Stock notification attributes that are set and changed
        /// </summary>
        /// <param name="stockName">Name of stock</param>
        /// <param name="currentValue">Current vallue of the stock</param>
        /// <param name="numChanges">Number of changes the stock goes through</param>
        public StockNotification(string stockName, int currentValue, int numChanges)
        {
            StockName = stockName;
            CurrentValue = currentValue;
            NumChanges = numChanges;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Stock stock1 = new Stock("Technology", 160, 5, 15);
            Stock stock2 = new Stock("Retail", 30, 2, 6);
            Stock stock3 = new Stock("Banking", 90, 4, 10);
            Stock stock4 = new Stock("Commodity", 500, 20, 50);

            StockBroker b1 = new StockBroker("Broker 1");
            b1.AddStock(stock1);
            b1.AddStock(stock2);

            StockBroker b2 = new StockBroker("Broker 2");
            b2.AddStock(stock1);
            b2.AddStock(stock3);
            b2.AddStock(stock4);

            StockBroker b3 = new StockBroker("Broker 3");
            b3.AddStock(stock1);
            b3.AddStock(stock3);

            StockBroker b4 = new StockBroker("Broker 4");
            b4.AddStock(stock1);
            b4.AddStock(stock2);
            b4.AddStock(stock3);
            b4.AddStock(stock4);
        }
    }

}
