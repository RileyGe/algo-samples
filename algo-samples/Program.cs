using Algorand;
using Algorand.Algod.Client;
using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using Org.BouncyCastle.Crypto.Generators;
using System;
using Account = Algorand.Account;
using Transaction = Algorand.Transaction;

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
            ulong? feePerByte;
            string genesisID;
            Digest genesisHash;
            ulong? firstRound;
            try
            {
                TransactionParams transParams = algodApiInstance.TransactionParams();
                feePerByte = transParams.Fee;
                genesisHash = new Digest(Convert.FromBase64String(transParams.Genesishashb64));
                genesisID = transParams.GenesisID;
                Console.WriteLine("Suggested Fee: " + feePerByte);
                NodeStatus s = algodApiInstance.GetStatus();
                firstRound = s.LastRound;
                Console.WriteLine("Current Round: " + firstRound);
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }
            // 向DEST_ADDR转账0.1algo
            ulong? amount = 100000;
            ulong? lastRound = firstRound + 1000; // 1000 is the max tx window            
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            Account src = new Account(SRC_ACCOUNT);
            Console.WriteLine("My account address is:" + src.Address.ToString());
            string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
            Transaction tx = new Transaction(src.Address, new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
            //sign the transaction before send it to the blockchain
            SignedTransaction signedTx = src.SignTransactionWithFeePerByte(tx, (ulong)feePerByte);
            Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);
            // send the transaction to the network
            try
            {
                //encode to msg-pack
                var encodedMsg = Algorand.Encoder.EncodeToMsgPack(signedTx);
                TransactionID id = algodApiInstance.RawTransaction(encodedMsg);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }

        }
    }
}
