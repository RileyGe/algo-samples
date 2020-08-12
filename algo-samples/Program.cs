using Algorand.Algod.Client;
using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using System;

namespace algo_samples
{
    class Program
    {
        static void Main(string[] args)
        {
            string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/ps1";
            string ALGOD_API_TOKEN = "GeHdp7CCGt7ApLuPNppXN4LtrW07Mm1kaFNJ5Ovr"; 
            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            try
            {
                Supply supply = algodApiInstance.GetSupply();
                Console.WriteLine("Total Algorand Supply: " + supply.TotalMoney);
                Console.WriteLine("Online Algorand Supply: " + supply.OnlineMoney);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling algod#getSupply: " + e.Message);
            }
        }
    }
}
