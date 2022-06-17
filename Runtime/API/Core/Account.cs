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

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using J4FApi.Dto.Account;
using J4FApi.Maths;

namespace J4FApi.Accounts
{
    public class Account
    {
        public string Hash { get; set; }
        private HttpClient Client { get; set; }
        
        private string Password { get; set; }

        private Account(HttpClient client, string accountHash, string password)
        {
            Client = client;
            Hash = accountHash;
            Password = password;
        }
        
        private Account(HttpClient client, string accountHash, string password, string publicKey, string privateKey)
        {
            Client = client;
            Hash = accountHash;
            Password = password;

            var pathWallet = Path.Combine(Application.persistentDataPath, "Wallets");
            var walletFileName = accountHash;
            var pathToSave = Path.Combine(pathWallet, walletFileName+".dat");

            if (!Directory.Exists(pathWallet))
            {
                Directory.CreateDirectory(pathWallet);
            }

            if (File.Exists(pathToSave)) return;
            
            var privateKeyDto = JsonConvert.DeserializeObject<PrivateKeyDto>(privateKey);
            var publicKeyDto = JsonConvert.DeserializeObject<PublicKeyDto>(publicKey);
                
            var walletLines = new string[]
            {
                "[PRIVATE]",
                "D = " + privateKeyDto.D,
                "DP = " + privateKeyDto.Dp,
                "DQ = " + privateKeyDto.Dq,
                "Exponent = " + privateKeyDto.Exponent,
                "InverseQ = " + privateKeyDto.InverseQ,
                "Modulus = " + privateKeyDto.Modulus,
                "P = " + privateKeyDto.P,
                "Q = " + privateKeyDto.Q,
                "",
                "[PUBLIC]",
                "Exponent = " + publicKeyDto.Exponent,
                "Modulus" + publicKeyDto.Modulus,
            };
            
            //Write wallet file
            File.WriteAllLines(pathToSave,walletLines);
        }
        
        /// <summary>
        /// Static async method that behave like a constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="accountHash"></param>
        /// <param name="password"></param>
        /// <returns>Account instance</returns> 
        public static async Task<Account> GetAccountAsync(HttpClient client, string accountHash, string password)  
        {       
            return await _GetAccount(client, accountHash, password);
        }

        /// <summary>
        /// Get balance of account
        /// </summary>
        /// <returns></returns>
        public async Task<BigDecimal> GetBalanceAsync()
        {
            try
            {
                var response = await Client.GetAsync($"/wallet/balance?wallet={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var balanceDto = JsonConvert.DeserializeObject<BalanceDto>(responseJson);
                if (balanceDto == null)
                {
                    throw new Exception("BalanceDto is null");
                }
                
                return BigDecimal.Parse(balanceDto.Balance);
            }
            catch (Exception e)
            {
                Debug.LogError("Account.GetBalance.Error: " + e.Message);
                return 0;
            }
        }
        
        /// <summary>
        /// Get pending balance of account
        /// </summary>
        /// <returns></returns>
        public async Task<BigDecimal> GetPendingBalanceAsync()
        {
            try
            {
                var response = await Client.GetAsync($"/wallet/pendingBalance?wallet={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var balanceDto = JsonConvert.DeserializeObject<PendingBalanceDto>(responseJson);
                if (balanceDto == null)
                {
                    throw new Exception("BalanceDto is null");
                }
                
                return BigDecimal.Parse(balanceDto.Balance);
            }
            catch (Exception e)
            {
                Debug.LogError("Account.GetPendingBalanceAsync.Error: " + e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Create new account on blockchain and return Account instance
        /// </summary>
        /// <param name="client"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Account> CreateAccountAsync(HttpClient client, string password)
        {
            try
            {
                var response = await client.PostAsync($"/wallet/create", new StringContent(JsonConvert.SerializeObject(new
                {
                    Password = password
                })));
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                
                var accountDto = JsonConvert.DeserializeObject<CreateAccountDto>(responseJson);
                if (accountDto == null)
                {
                    throw new Exception("CreateAccountDto is null");
                }
                
                return new Account(client, accountDto.Wallet, password, accountDto.PublicKey, accountDto.PrivateKey);
            }
            catch (Exception e)
            {
                Debug.LogError("Account.CreateAccountAsync.Error: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get inventory tokens of account (J4FRC10 & J4FRC20)
        /// </summary>
        /// <returns></returns>
        public async Task<InventoryDto> GetInventoryAsync()
        {
            try
            {
                var response = await Client.GetAsync($"/wallet/inventory?Hash={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var accountInventoryDto = JsonConvert.DeserializeObject<InventoryDto>(responseJson);
                if (accountInventoryDto == null)
                {
                    throw new Exception("AccountInventoryDto is null");
                }
                return accountInventoryDto;
            }
            catch (Exception e)
            {
                Debug.LogError("Account.GetInventoryAsync.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Send transaction over blockchain
        /// </summary>
        /// <returns></returns>
        public async Task<SendTransactionDto> SendTransactionAsync(string password, string to, BigDecimal amount, int gasLimit, BigDecimal gasPrice, string data = "")
        {
            try
            {
                if (password != Password) {
                    throw new Exception("Password is not correct");
                }

                if (string.IsNullOrEmpty(data))
                {
                    data = "0x";
                }
                
                var response = await Client.PostAsync($"/wallet/sendTransaction", new StringContent(JsonConvert.SerializeObject(new
                {
                    From = Hash,
                    Password = Password,
                    To = to,
                    Amount = amount.ToString(),
                    GasLimit = gasLimit.ToString(),
                    GasPrice = gasPrice.ToString(),
                    Data = data
                })));
                
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var transactionDto = JsonConvert.DeserializeObject<SendTransactionDto>(responseJson);
                
                if (transactionDto == null)
                {
                    throw new Exception("TransactionDto is null");
                }
                
                return transactionDto;
            }
            catch (Exception e)
            {
                Debug.LogError("Account.SendTransaction.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Get contract info and methods
        /// </summary>
        /// <param name="client"></param>
        /// <param name="accountHash"></param>
        /// <param name="password"></param>
        /// <returns>Account instance</returns>
        private static async Task<Account> _GetAccount(HttpClient client, string accountHash, string password)
        {
            var responseSign = await SignAsync(client, accountHash, password);

            if (string.IsNullOrEmpty(responseSign))
            {
                Debug.LogError("Can't login in to account");
                return null;
            }
            
            //return account object
            return new Account(client, accountHash, password);
        }
        
        /// <summary>
        /// Sign in to account
        /// </summary>
        /// <param name="client"></param>
        /// <param name="accountHash"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static async Task<string> SignAsync(HttpClient client, string accountHash, string password)
        {
            try
            {
                var response = await client.PostAsync($"/wallet/sign", new StringContent(JsonConvert.SerializeObject(new
                {
                    Wallet = accountHash,
                    Password = password
                })));
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                
                var dataParsed = JsonConvert.DeserializeObject<SignDto>(responseJson);
                
                return dataParsed?.Signed;
            }
            catch (Exception e)
            {
                Debug.LogError("Parse Error: " + e.Message);
                return null;
            }
        }
    }
}