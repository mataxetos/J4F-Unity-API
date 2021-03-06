// Copyright(c) 2022, Pere Gomis Moreno <mataxetos.es>
//
// All rights reserved.
//
// This file is part of the J4FAPI.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 1 - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// 2 - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// 3 - Neither the name of copyright holders nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL COPYRIGHT HOLDERS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Threading.Tasks;
using UnityEngine;
using J4FApi;
using J4FApi.Maths;
using Newtonsoft.Json;

public class ExampleApi : MonoBehaviour
{
    public string ApiURI = "http://79.137.126.129:3939";
    public string ContractAddress = "45AD0DB9652CA3E7108A1F4DBA1A90F6159CD6F05F82D0A9EA467AE1E398324D";
    public string AccountPassword = "prueba123";
    public string AccountAddress = "J4FA614BBA7286DE0F9F28B1145174EC1432CCE185BEB91D8DF7108D79D94B8A1DF";
    public string TransactionHash = "1BFFA739E9170DB5627C877E94CB0261553DA4C5D8086592984E8157ACE93536";
    
    // Start is called before the first frame update
    private async void Start()
    {
        await CallApi();
    }

    private async Task CallApi()
    {
        //Get web3 pointer
        var web3 = await J4F.Web3(ApiURI);
        if (web3 == null)
        {
            return;
        }
        
        //Get contract pointer
        var contract = await web3.GetContractAsync(ContractAddress);
        
        //Get function pointer
        var getAdnFunction = contract.GetFunction("GetADN");
        
        //Check if exists function
        if (getAdnFunction != null)
        {
            var responseGetAdnFunction = await getAdnFunction.CallReadAsync("1");
            Debug.Log(getAdnFunction.Method+"(1) = " + responseGetAdnFunction);
        }

        var contractRc10Transactions = await contract.GetRc10Transactions();
        foreach (var rc10Transaction in contractRc10Transactions)
        {
            Debug.Log("Contract RC10 Transaction Info: " + JsonConvert.SerializeObject(rc10Transaction));   
        }
        
        var contractRc20Transactions = await contract.GetRc20Transactions();
        foreach (var rc20Transaction in contractRc20Transactions)
        {
            Debug.Log("Contract RC20 Transaction Info: " + JsonConvert.SerializeObject(rc20Transaction));   
        }

        //Get transaction info
        var transactionDto = await web3.GetTransactionAsync(TransactionHash);
        Debug.Log("Transaction Info: " + JsonConvert.SerializeObject(transactionDto));
        
        var rc10Transactions = await transactionDto.GetRc10Transactions();
        Debug.Log("RC10 Transaction Info: " + JsonConvert.SerializeObject(rc10Transactions));
        
        var rc20Transactions = await transactionDto.GetRc20Transactions();
        Debug.Log("RC20 Transaction Info: " + JsonConvert.SerializeObject(rc20Transactions));

        /*
        //Create new account
        var accountCreated = await web3.CreateAccountAsync(AccountPassword);
        if (accountCreated == null)
        {
            return;
        }
        Debug.Log("New Wallet created: " + accountCreated.Hash);
        */

        //Get info of existing account
        var account = await web3.GetAccountAsync(AccountAddress, AccountPassword);
        if (account != null)
        {
            //Get account balance & pending balance
            var balance = await account.GetBalanceAsync();
            var pendingBalance = await account.GetPendingBalanceAsync();
            Debug.Log("Balance = " + balance + " | Pending Balance = " + pendingBalance);

            //Get account inventory
            var inventory = await account.GetInventoryAsync();
            if (inventory != null)
            {
                Debug.Log("Inventory = " + JsonConvert.SerializeObject(inventory));
            }
            
            /*
            var buyBoxFunction = contract.GetFunction("BuyBox");
            if (buyBoxFunction != null)
            {
                //Get Data parsed to execute function
                var dataParsedBuyBox = await buyBoxFunction.CallWriteAsync();
                
                //Get required gas to execute function
                var requiredGas = await web3.CalculateGasAsync(ContractAddress, dataParsedBuyBox);
                
                //Send transaction
                var transaction = await account.SendTransactionAsync(
                    AccountPassword,
                    ContractAddress,
                    BigDecimal.Parse("0"),
                    requiredGas,
                    BigDecimal.Parse("0"),
                    dataParsedBuyBox);
                Debug.Log("Transaction Hash of BuyDonut: " + transaction.Hash);
                
                //Get pending transaction info
                var pendingTransactionDto = await web3.GetPendingTransactionAsync(transaction.Hash);
                Debug.Log("Pending Transaction Info: " + JsonConvert.SerializeObject(pendingTransactionDto));   
            }
            */
        }
    }
}
