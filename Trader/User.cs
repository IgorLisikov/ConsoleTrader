using System;
using System.Collections.Generic;


namespace Trader
{
    [Serializable]
    public class UserAccount
    {
        public string Name { get; private set; }
        public double BalanceMoney { get; set; }
        public double BalanceCoin { get; set; }
        public UserAccount(string name)
        {
            Name = name;
        }
        public UserAccount()
        {

        }
        public List<string> RequestTransaction(ITransaction transaction)
        {
            transaction.GetProcessed();
            return transaction.Response;
        }
    }
}
