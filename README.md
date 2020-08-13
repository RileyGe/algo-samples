# algo-samples
Some samples of dotnet-algorand-sdk

```c#
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
```



```c#
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
```





```c#
// Utility function for sending a raw signed transaction to the network        
public static void Run(params string[] args) //throws Exception
{
    string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/ps1";
    string ALGOD_API_TOKEN = "GeHdp7CCGt7ApLuPNppXN4LtrW07Mm1kaFNJ5Ovr";
    AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

    // 这三个账号只用于演示，在实际使用时永远不要直接将助记词放在代码中
    string account1_mnemonic = "portion never forward pill lunch organ biology"
                                + " weird catch curve isolate plug innocent skin grunt"
                                + " bounce clown mercy hole eagle soul chunk type absorb trim";
    string account2_mnemonic = "place blouse sad pigeon wing warrior wild script"
                        + " problem team blouse camp soldier breeze twist mother"
                        + " vanish public glass code arrow execute convince ability"
                        + " there";
    string account3_mnemonic = "image travel claw climb bottom spot path roast "
                        + "century also task cherry address curious save item "
                        + "clean theme amateur loyal apart hybrid steak about blanket";

    Account acct1 = new Account(account1_mnemonic);
    Account acct2 = new Account(account2_mnemonic);
    Account acct3 = new Account(account3_mnemonic);
    // get last round and suggested tx fee
    // We use these to get the latest round and tx fees
    // These parameters will be required before every 
    // Transaction
    // We will account for changing transaction parameters
    // before every transaction in this example
    var transParams = algodApiInstance.TransactionParams();

    // The following parameters are asset specific
    // and will be re-used throughout the example. 

    // Create the Asset（创建ASA）
    // Total number of this asset available for circulation    
    var ap = new AssetParams(creator: acct1.Address.ToString(), assetname: "latikum22", 
        unitname: "LAT", defaultfrozen: false, total: 10000,
        url: "http://this.test.com", metadatahash: Convert.ToBase64String(
            Encoding.ASCII.GetBytes("16efaa3924a6fd9d3a4880099a4ac65d")))
    {
        // 每种地址的意义请参照https://developer.algorand.org/docs/features/asa/
        // 默认情况下manager/reserve/freeze/clawback账号都是sender
        // 如果设置了manager,其他没有设置的地址reserve/freeze/clawback都会是manager
        Managerkey = acct1.Address.ToString(),
        Clawbackaddr = acct2.Address.ToString(),
        Freezeaddr = acct1.Address.ToString(),
        Reserveaddr = acct1.Address.ToString()
    };

    var tx = Utils.GetCreateAssetTransaction(ap, transParams, "asset tx message");

    // Sign the Transaction by sender
    SignedTransaction signedTx = acct1.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    ulong? assetID = 0;
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
        // Now that the transaction is confirmed we can get the assetID
        Algorand.Algod.Client.Model.Transaction ptx = algodApiInstance.PendingTransactionInformation(id.TxId);
        assetID = ptx.Txresults.Createdasset;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.StackTrace);
        return;
    }
    Console.WriteLine("AssetID = " + assetID);
    // 现在ASA已经创建


    // 修改ASA的设置
    // Next we will change the asset configuration
    // First we update standard Transaction parameters
    // To account for changes in the state of the blockchain
    transParams = algodApiInstance.TransactionParams();
    ap = algodApiInstance.AssetInformation((long?)assetID);

    // 修改ASA设置必须由manager账号执行，在本例是中acct2
    // Note in this transaction we are re-using the asset
    // creation parameters and only changing the manager
    // and transaction parameters like first and last round
    // now update the manager to acct1
    ap.Managerkey = acct2.Address.ToString();
    tx = Utils.GetConfigAssetTransaction(acct1.Address, assetID, ap, transParams, "config trans");

    // The transaction must be signed by the current manager account
    // We are reusing the signedTx variable from the first transaction in the example    
    signedTx = acct1.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id.TxId);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
    }
    catch (Exception e)
    {
        //e.printStackTrace();
        Console.WriteLine(e.Message);
        return;
    }

    // Next we will list the newly created asset
    // Get the asset information for the newly changed asset            
    ap = algodApiInstance.AssetInformation((long?)assetID);
    //The manager should now be the same as the creator
    Console.WriteLine(ap);


    // 激活(Opting in)某种ASA
    // 如果你需要给其他用户转ASA，那么对方必须先激活
    // 然后才能接收ASA       
    // First we update standard Transaction parameters
    // To account for changes in the state of the blockchain
    transParams = algodApiInstance.TransactionParams();
    tx = Utils.GetActivateAssetTransaction(acct3.Address, assetID, transParams, "opt in transaction");

    // The transaction must be signed by the current manager account
    // We are reusing the signedTx variable from the first transaction in the example    
    signedTx = acct3.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    Algorand.Algod.Client.Model.Account act = null;
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id.TxId);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
        // We can now list the account information for acct3 
        // and see that it can accept the new asseet
        act = algodApiInstance.AccountInformation(acct3.Address.ToString());
        Console.WriteLine(act);
    }
    catch (Exception e)
    {
        //e.printStackTrace();
        Console.WriteLine(e.Message);
        return;
    }

    // ASA转账
    // 激活后account3就可以接收ASA了
    // 现在我们从acctout1向account3转账
    // First we update standard Transaction parameters
    // To account for changes in the state of the blockchain
    transParams = algodApiInstance.TransactionParams();
    // Next we set asset xfer specific parameters
    // We set the assetCloseTo to null so we do not close the asset out
    ulong assetAmount = 10;
    tx = Utils.GetTransferAssetTransaction(acct1.Address, acct3.Address, assetID, assetAmount, transParams, null, "transfer message");
    // The transaction must be signed by the sender account
    // We are reusing the signedTx variable from the first transaction in the example    
    signedTx = acct1.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id.TxId);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
        // We can now list the account information for acct3 
        // and see that it now has 5 of the new asset
        act = algodApiInstance.AccountInformation(acct3.Address.ToString());
        Console.WriteLine(act.GetHolding(assetID).Amount);
    }
    catch (Exception e)
    {
        //e.printStackTrace();
        Console.WriteLine(e.Message);
        return;
    }

    // 冻结资产
    // 如果freeze address当时没有设置，则无法冻结资产
    // 此例中冻结account3中的资产
    // 冻结事件须由freeze acount来发出，本例中为account1
    // First we update standard Transaction parameters
    // To account for changes in the state of the blockchain
    transParams = algodApiInstance.TransactionParams();
    // Next we set asset xfer specific parameters
    // The sender should be freeze account acct2
    // Theaccount to freeze should be set to acct3
    tx = Utils.GetFreezeAssetTransaction(acct1.Address, acct3.Address, assetID, true, transParams, "freeze transaction");
    // The transaction must be signed by the freeze account acct2
    // We are reusing the signedTx variable from the first transaction in the example    
    signedTx = acct1.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id.TxId);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
        // We can now list the account information for acct3 
        // and see that it now frozen 
        // Note--currently no getter method for frozen state
        act = algodApiInstance.AccountInformation(acct3.Address.ToString());
        Console.WriteLine(act.GetHolding(assetID).ToString());
    }
    catch (Exception e)
    {
        //e.printStackTrace();
        Console.WriteLine(e.Message);
        return;
    }

    // 撤回转账
    // 撤加转账必须由clawbackaddress发起。
    // 如果资产的manager将clawbackaddress设为空，则此操作不可执行
    // 本例中会将10个资产从account3撤回到account1
    // 此操作需要由clawbackaccount（account2）进行签名
    // 此操作发送者为原操作的发起者（acct1)
    // First we update standard Transaction parameters
    // To account for changes in the state of the blockchain
    transParams = algodApiInstance.TransactionParams();
    // Next we set asset xfer specific parameters
    assetAmount = 10;
    tx = Utils.GetRevokeAssetTransaction(acct2.Address, acct3.Address, acct1.Address, assetID, assetAmount, transParams, "revoke transaction");
    // The transaction must be signed by the clawback account
    // We are reusing the signedTx variable from the first transaction in the example    
    signedTx = acct2.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
        // We can now list the account information for acct3 
        // and see that it now has 0 of the new asset
        act = algodApiInstance.AccountInformation(acct3.Address.ToString());
        Console.WriteLine(act.GetHolding(assetID).Amount);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        return;
    }

    // 销毁资产
    // 销毁资产前所有资产需要回到创建者账号中
    // 销毁资产需要由Manage Addr进行操作
    // First we update standard Transaction parameters
    // To account for changes in the state of the blockchain
    transParams = algodApiInstance.TransactionParams();
    // Next we set asset xfer specific parameters
    // The manager must sign and submit the transaction
    // This is currently set to acct2
    tx = Utils.GetDestroyAssetTransaction(acct2.Address, assetID, transParams, "destroy transaction");
    // The transaction must be signed by the manager account
    // We are reusing the signedTx variable from the first transaction in the example    
    signedTx = acct2.SignTransaction(tx);
    // send the transaction to the network and
    // wait for the transaction to be confirmed
    try
    {
        TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
        Console.WriteLine("Transaction ID: " + id);
        Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
        // We can now list the account information for acct1 
        // and see that the asset is no longer there
        act = algodApiInstance.AccountInformation(acct1.Address.ToString());
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        return;
    }
    Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
    Console.ReadKey();
}
```

