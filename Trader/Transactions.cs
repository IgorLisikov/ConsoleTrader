using System;
using System.Collections.Generic;


namespace Trader
{
    public interface ITransaction
    {
        UserAccount User { get; set; }
        double Amount { get; set; }
        List<string> Response { get; set; }
        void GetProcessed();
    }

    public class Purchase : ITransaction
    {
        public UserAccount User { get; set; }
        public double Amount { get; set; }
        public List<string> Response { get; set; }
        public Coin Coin { get; set; }
        public Purchase(UserAccount user, Coin coin)
        {
            User = user;
            Coin = coin;
            Amount = 0;
            Response = new List<string>();
        }
        public void GetProcessed()
        {
            if (!Verification.CheckServerLoad(Coin.Trend))
            {
                Response.Add("Error: Transaction declined.");
                Response.Add("Message: Transactions temporarily unavailable. We apologise for inconvinience.");
                return;
            }
            if (!Verification.CheckEnoughFunds(Amount, User.BalanceMoney))
            {
                Response.Add("Error: Transaction declined.");
                Response.Add("Message: Insufficient funds!");
                return;
            }
            double amountOfUsdYouPay = Amount;
            double amountOfCoinYouGet = Math.Round(amountOfUsdYouPay / Coin.Price, 6);
            User.BalanceMoney -= amountOfUsdYouPay;
            User.BalanceCoin += amountOfCoinYouGet;
            Response.Add("Message: Transaction accepted.");
            Response.Add($"Message: You get {amountOfCoinYouGet:0.000000} Coin for {amountOfUsdYouPay:0.00} Money.");
        }
    }

    public class Sale : ITransaction
    {
        public UserAccount User { get; set; }
        public double Amount { get; set; }
        public List<string> Response { get; set; }
        public Coin Coin { get; set; }
        public Sale(UserAccount user, Coin coin)
        {
            User = user;
            Coin = coin;
            Amount = 0;
            Response = new List<string>();
        }
        public void GetProcessed()
        {
            if (!Verification.CheckServerLoad(Coin.Trend))
            {
                Response.Add("Error: Transaction declined.");
                Response.Add("Message: Transactions temporarily unavailable. We apologise for inconvinience.");
                return;
            }
            if (!Verification.CheckEnoughFunds(Amount, User.BalanceCoin))
            {
                Response.Add("Error: Transaction declined.");
                Response.Add("Message: Insufficient funds!");
                return;
            }
            double amountOfCoinYouPay = Amount;
            double amountOfUsdYouGet = Math.Round(amountOfCoinYouPay * Coin.Price, 2);
            User.BalanceMoney += amountOfUsdYouGet;
            User.BalanceCoin -= amountOfCoinYouPay;
            Response.Add("Message: Transaction accepted.");
            Response.Add($"Message: You get {amountOfUsdYouGet:0.00} Money for {amountOfCoinYouPay:0.000000} Coin.");
        }
    }

    public class Withdrawal : ITransaction
    {
        public UserAccount User { get; set; }
        public double Amount { get; set; }
        public List<string> Response { get; set; }
        public Withdrawal(UserAccount user)
        {
            User = user;
            Amount = 0;
            Response = new List<string>();
        }
        public void GetProcessed()
        {
            if (!Verification.CheckEnoughFunds(Amount, User.BalanceMoney))
            {
                Response.Add("Error: Transaction declined.");
                Response.Add("Message: Insufficient funds!");
                return;
            }
            double amountWithdraw = Math.Round(Amount, 2);
            User.BalanceMoney -= amountWithdraw;
            Response.Add($"Message: You withdrawal of {amountWithdraw:0.00} Money has been processed.");
        }
    }

    public class Deposit : ITransaction
    {
        public UserAccount User { get; set; }
        public double Amount { get; set; }
        public List<string> Response { get; set; }
        public Deposit(UserAccount user)
        {
            User = user;
            Amount = 0;  
            Response = new List<string>();
        }
        public void GetProcessed()
        {
            if (!Verification.CheckDepositAmount(Amount))
            {
                Response.Add("Error: Transaction declined.");
                Response.Add("Message: Up to 10 000 per one deposit");
                return;
            }
            double amountDeposit = Math.Round(Amount, 2);
            User.BalanceMoney += Amount;
            Response.Add($"Message: Your account has been credited with {amountDeposit:0.00} Money.");
        }
    }
    public static class Verification
    {
        public static bool CheckEnoughFunds(double amount, double balance)
        {
            if (Math.Round(amount,6) > Math.Round(balance,6))
            {
                return false;
            }
            return true;
        }
        public static bool CheckServerLoad(Trend trend)
        {
            if (trend.ServerLoad == ServerLoadState.High.ToString())
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                int temp = rnd.Next(1, 11);
                if (temp % 10 == 0)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool CheckDepositAmount(double amount)
        {
            if (amount > 10000)
            {
                return false;
            }
            return true;
        }
    }
}
