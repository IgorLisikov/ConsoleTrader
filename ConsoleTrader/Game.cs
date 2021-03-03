using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Timers;
using Trader;


namespace ConsoleTrader
{
    [DataContract]
    [KnownType(typeof(FlatTrend))]
    [KnownType(typeof(AscendingFlatTrend))]
    [KnownType(typeof(DescendingFlatTrend))]
    [KnownType(typeof(BearMarketTrend))]
    [KnownType(typeof(BullMarketTrend))]
    public class Game
    {
        [DataMember]
        public Coin Coin { get; private set; }
        [DataMember]
        public Graph Graph { get; private set; }
        [DataMember]
        public UserAccount User { get; private set; }
        [DataMember]
        System.Timers.Timer trendTimer;

        System.Timers.Timer priceTimer;
        List<ValueProbability> ListOfTrendProbabilities { get; set; }
        List<string> gameFrame;
        string commandRequest;
        string inputLine;
        List<string> commandResponse;
        CommandNavigator commNavigator;
        object locker;
        readonly string pathToSavedGames;
        public Game()
        {
            pathToSavedGames = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saved games");
            Directory.CreateDirectory(pathToSavedGames);

            ListOfTrendProbabilities = new List<ValueProbability>
            {
                new ValueProbability(1, 25, 1),     // 24% chance to get flat trend 
                new ValueProbability(25, 49, 2),    // 24% chance to get descending flat
                new ValueProbability(49, 73, 3),    // 24% chance to get ascending flat
                new ValueProbability(73, 87, 4),    // 14% chance to get bull market trend
                new ValueProbability(87, 101, 5)    // 14% chance to get bear market trend
            };
            gameFrame = new List<string>();
            commandRequest = "Enter command";
            inputLine = "";
            commandResponse = new List<string>();
            commNavigator = CommandNavigator.BaseCommands;
            locker = new object();
            priceTimer = new System.Timers.Timer(300);
            trendTimer = new System.Timers.Timer(10000);
            SetTimers();
        }
        private void SetTimers()
        {
            Random randomizer = new Random(DateTime.Now.Millisecond);
            priceTimer.Elapsed += (source, e) =>
            {
                Coin.GetNextPrice(randomizer);
                Graph.BuildGraph(Coin.Price);
                PrintFrame(null, null);
            };

            trendTimer.AutoReset = false;
            trendTimer.Elapsed += (source, e) =>
            {
                int rangeValue = randomizer.Next(1, 101);
                TrendName trend = (TrendName)ListOfTrendProbabilities.Single((listItem) => rangeValue >= listItem.Begin && rangeValue < listItem.End).Value;
                SwitchTrend(trend);
                trendTimer.Stop();
                trendTimer.Interval = randomizer.Next(10000, 30000);
                trendTimer.Start();
            };
        }


        public void Start()
        {
            Thread userInput = new Thread(ReadUserInput);
            userInput.Start();
            priceTimer.Start();
            trendTimer.Start();
        }
        public void Stop()
        {
            priceTimer.Stop();
            trendTimer.Stop();
        }
        public void Save()
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Game));
            string relativePath = User.Name + ".json";
            string path = Path.Combine(pathToSavedGames, relativePath);
            using (FileStream file = new FileStream(path, FileMode.OpenOrCreate))
            {
                jsonFormatter.WriteObject(file, this);
            }
        }
        public void Load(string userName, out bool result)
        {
            result = false;
            string relativePath = userName + ".json";
            string path = Path.Combine(pathToSavedGames, relativePath);
            try
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Game));
                using (FileStream file = new FileStream(path, FileMode.Open))
                {
                    Game gameState = jsonFormatter.ReadObject(file) as Game;
                    Coin = gameState.Coin;
                    Graph = gameState.Graph;
                    User = gameState.User;
                    trendTimer = gameState.trendTimer;
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
        }

        public void ShowStartDialog()
        {
            Console.WriteLine("Enter \"1\" to load game, or enter any key to start new game:");
            string str = Console.ReadLine();
            if (str == "1")
            {
                string extension = ".json";
                string[] savedGames = GetSavedGameFileNames("*"+extension);
                if (savedGames.Count() == 0)
                {
                    Console.WriteLine("No saved games found.");
                    Console.WriteLine("Press any key to start new game:");
                    Console.ReadKey();
                    ShowNewGameDialog();
                    return;
                }
                Console.Clear();
                Console.WriteLine("List of saved games:");
                foreach (var item in savedGames)
                {
                    Console.WriteLine(item.Remove(item.Length - extension.Length));
                }
                Console.WriteLine();
                Console.WriteLine("Enter game name to load:");
                string name = Console.ReadLine();
                if (!savedGames.Contains(name + extension))
                {
                    Console.WriteLine($"No saved game named \"{name}\" was found.");
                    Console.WriteLine("Press any key to continue:");
                    Console.ReadKey();
                    Console.Clear();
                    ShowStartDialog();
                    return;
                }

                Load(name, out bool result);
                if (!result)
                {
                    Console.Clear();
                    Console.WriteLine("Error: Unable to load this file.");
                    ShowStartDialog();
                    return;
                }
                Start();
            }
            else
            {
                ShowNewGameDialog();
            }
        }

        public void ShowNewGameDialog()
        {
            Console.Clear();
            Console.WriteLine("Enter your name:");
            string name = Console.ReadLine();
            while (name.Length == 0 || name.Length > 20)
            {
                Console.WriteLine("1-20 symbols, try again!");
                name = Console.ReadLine();
            }

            string[] files = GetSavedGameFileNames("*.json");
            while (files.Contains(name + ".json"))
            {
                Console.WriteLine("This name already exists!");
                Console.WriteLine("Press any key to continue:");
                Console.ReadKey();
                ShowNewGameDialog();
                return;
            }

            CreateNew(name);
 
            Console.Clear();
            Console.WriteLine($"Hello {User.Name}!");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("\"help\" - to show list of commands;");
            Console.WriteLine("\"buy\" - to buy;");
            Console.WriteLine("\"sell\" - to sell;");
            Console.WriteLine("\"deposit\" - to make a deposit;");
            Console.WriteLine("\"withdraw\" - to make a withdrawal;");
            Console.WriteLine("\"exit\" - to save and exit game.");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue:");
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Your balance is:");
            Console.WriteLine($"Money: {User.BalanceMoney}; Coin: {User.BalanceCoin}");
            Console.WriteLine();
            Console.WriteLine("Press any key to start game:");
            Console.ReadKey();
            Start();
            
        }
        public void CreateNew(string name)
        {
            Coin = new Coin
            {
                Price = 7200,
                Trend = new FlatTrend()
            };

            Graph = new Graph(30, 100, Coin.Price);
            User = new UserAccount(name)
            {
                BalanceMoney = 100
            };

        }
        private string[] GetSavedGameFileNames(string searchPattern)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToSavedGames);
            FileInfo[] files = directoryInfo.GetFiles(searchPattern);
            string[] fileNames = files.Select(file => file.Name).ToArray();
            return fileNames;
        }

        private void PrintFrame(Object source, ElapsedEventArgs e)
        {
            lock(locker)
            {
                gameFrame.Clear();
                gameFrame.Add($"Money: {User.BalanceMoney:0.00}\t\tCoin: {User.BalanceCoin:0.000000}      ");
                gameFrame.Add(new string(' ', Console.WindowWidth - 1));
                gameFrame.AddRange(Graph.GraphImage);
                gameFrame.Add(commandRequest);
                gameFrame.Add(inputLine);
                gameFrame.AddRange(commandResponse);
                Console.SetCursorPosition(0, 0);
                foreach (var item in gameFrame)
                {
                    Console.WriteLine(item);
                }
                Console.CursorVisible = false;
                Console.SetCursorPosition(0, 0);
            }
        }
       
        private void HandleCommand()
        {
            if (commNavigator == CommandNavigator.BaseCommands)
            {
                HandleBaseCommand();
                return;
            }
            else
            {
                switch (commNavigator)
                {
                    case CommandNavigator.Buy:
                        HandleTransactionCommand(new Purchase(User, Coin));
                        break;
                    case CommandNavigator.Sell:
                        HandleTransactionCommand(new Sale(User, Coin));
                        break;
                    case CommandNavigator.Deposit:
                        HandleTransactionCommand(new Deposit(User));
                        break;
                    case CommandNavigator.Withdraw:
                        HandleTransactionCommand(new Withdrawal(User));
                        break;
                }
            }
        }


        private void HandleBaseCommand()
        {
            string emptyString = new string(' ', Console.WindowWidth - 1);
            string responseString;
            List<string> emptyResponse = new List<string>
            {
                emptyString,
                emptyString,
                emptyString
            };
            commNavigator = CommandNavigator.BaseCommands;
            commandResponse.Clear();
            if (Enum.TryParse(inputLine, true, out Command command))
            {
                switch (command)
                {
                    case Command.Help:
                        commandRequest = "Enter command:";
                        AppendSpaces(ref commandRequest);
                        responseString = "\"help\", \"buy\", \"sell\", \"deposit\", \"withdraw\", \"exit\".";
                        AppendSpaces(ref responseString);
                        commandResponse.Add(responseString);
                        commandResponse.Add(emptyString);
                        commandResponse.Add(emptyString);
                        break;
                    case Command.Buy:
                        commandRequest = "Enter amount, Money:";
                        AppendSpaces(ref commandRequest);
                        commandResponse.AddRange(emptyResponse);
                        commNavigator = CommandNavigator.Buy;
                        break;
                    case Command.Sell:
                        commandRequest = "Enter amount, Coin:";
                        AppendSpaces(ref commandRequest);
                        commandResponse.AddRange(emptyResponse);
                        commNavigator = CommandNavigator.Sell;
                        break;
                    case Command.Deposit:
                        commandRequest = "Enter amount, Money:";
                        AppendSpaces(ref commandRequest);
                        commandResponse.AddRange(emptyResponse);
                        commNavigator = CommandNavigator.Deposit;
                        break;
                    case Command.Withdraw:
                        commandRequest = "Enter amount, Money:";
                        AppendSpaces(ref commandRequest);
                        commandResponse.AddRange(emptyResponse);
                        commNavigator = CommandNavigator.Withdraw;
                        break;
                    case Command.Exit:
                        Stop();
                        Save();
                        Environment.Exit(0);
                        break;
                    default:
                        commandRequest = "Enter command:";
                        AppendSpaces(ref commandRequest);
                        responseString = "Error: Unknown command";
                        AppendSpaces(ref responseString);
                        commandResponse.Add(responseString);
                        commandResponse.Add(emptyString);
                        commandResponse.Add(emptyString);
                        break;
                }
            }
            else
            {
                commandRequest = "Enter command:";
                AppendSpaces(ref commandRequest);
                responseString = "Error: Unknown command";
                AppendSpaces(ref responseString);
                commandResponse.Add(responseString);
                commandResponse.Add(emptyString);
                commandResponse.Add(emptyString);
            }
        }

        private void HandleTransactionCommand(ITransaction transaction)
        {
            commNavigator = CommandNavigator.BaseCommands;
            commandRequest = "Enter command:";
            commandResponse.Clear();
            string emptyString = new string(' ', Console.WindowWidth - 1);
            string responseString;
            AppendSpaces(ref commandRequest);
            if (double.TryParse(inputLine.Replace(".",","), out double amount) && amount > 0)
            {
                transaction.Amount = amount;
                List<string> response = User.RequestTransaction(transaction);
                commandResponse.AddRange(response);
                commandResponse.Add(emptyString);
            }
            else
            {
                responseString = "Error: Wrong amount";
                AppendSpaces(ref responseString);
                commandResponse.Add(responseString);
                commandResponse.Add(emptyString);
                commandResponse.Add(emptyString);
            }
        }

        private void ReadUserInput()
        {
            while (true)
            {
                ConsoleKeyInfo ci = Console.ReadKey();
                char ch = ci.KeyChar;
                switch (ci.Key)
                {
                    case ConsoleKey.Backspace:
                        if (inputLine.Length > 0)
                        {
                            inputLine = inputLine.Remove(inputLine.Length - 1);
                            inputLine += ' ';
                            PrintFrame(null, null);
                            inputLine = inputLine.Remove(inputLine.Length - 1);
                        }
                        break;
                    case ConsoleKey.Enter:
                        HandleCommand();
                        inputLine = new string(' ', Console.WindowWidth - 1);
                        PrintFrame(null, null);
                        inputLine = commandRequest = "";
                        commandResponse.Clear();
                        break;

                    default:
                        inputLine += ch;
                        PrintFrame(null, null);
                        break;
                }
            }
        }
        private void SwitchTrend(TrendName trend)
        {
            switch (trend)
            {
                case TrendName.Flat:
                    Coin.Trend = new FlatTrend();
                    break;
                case TrendName.DescendingFlat:
                    Coin.Trend = new DescendingFlatTrend();
                    break;
                case TrendName.AscendingFlat:
                    Coin.Trend = new AscendingFlatTrend();
                    break;
                case TrendName.BullMarket:
                    Coin.Trend = new BearMarketTrend();
                    break;
                case TrendName.BearMarket:
                    Coin.Trend = new BullMarketTrend();
                    break;
            }
        }

        private static void AppendSpaces(ref string str)
        {
            str = str.PadRight(Console.WindowWidth - str.Length - 1, ' ');
        }

    }
}
