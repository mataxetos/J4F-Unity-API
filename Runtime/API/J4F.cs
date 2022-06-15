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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using J4FApi.Contracts;
using J4FApi.Accounts;
using J4FApi.Dto;
using J4FApi.Dto.Account;
using J4FApi.Transactions;
using Newtonsoft.Json;

namespace J4FApi
{
    public class J4F
    {
        private readonly HttpClient _client;

        private J4F()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        /// <summary>
        /// Create a Web3 client to interact with J4F Blockchain
        /// </summary>
        /// <param name="uri"></param>
        public static async Task<J4F> Web3(string uri)
        {
            var j4F = new J4F();
            j4F._client.BaseAddress = new Uri(uri);
            
            try
            {
                var response = await j4F._client.GetAsync($"/");
                response.EnsureSuccessStatusCode();
                return j4F;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("J4F.Web3.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Calculate gas for a transaction
        /// </summary>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> CalculateGasAsync(string to, string data)
        {
            try
            {
                var response = await _client.PostAsync($"/j4f/calculateGas", new StringContent(JsonConvert.SerializeObject(new
                {
                    To = to,
                    Data = data
                })));
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var dataParsed = JsonConvert.DeserializeObject<CalculateGasDto>(responseJson);
                if (dataParsed == null)
                {
                    throw new Exception("J4F.CalculateGas.Error: Error parsing response");
                }
                return dataParsed.Gas;   
            }
            catch (Exception e)
            {
                throw new Exception("J4F.CalculateGas.Error: " + e.Message);
                return 0;
            }
        }
        
        /// <summary>
        /// Get account pointer
        /// </summary>
        /// <param name="address"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Account> GetAccountAsync(string address, string password)
        {
            return await Account.GetAccountAsync(_client, address, password);
        }

        /// <summary>
        /// Create account and return account pointer
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Account> CreateAccountAsync(string password)
        {
            return await Account.CreateAccountAsync(_client, password);
        }

        /// <summary>
        /// Get transaction pointer
        /// </summary>
        /// <param name="hash">Transaction hash</param>
        /// <returns></returns>
        public async Task<Transaction> GetTransactionAsync(string hash)
        {
            return await Transaction.GetTransactionAsync(_client, hash);
        }
        
        /// <summary>
        /// Get pending transaction pointer
        /// </summary>
        /// <param name="hash">Transaction hash</param>
        /// <returns></returns>
        public async Task<PendingTransaction> GetPendingTransactionAsync(string hash)
        {
            return await PendingTransaction.GetPendingTransactionAsync(_client, hash);
        }
        
        /// <summary>
        /// Get contract object given contract address
        /// </summary>
        /// <param name="contractHash"></param>
        /// <returns></returns>
        public async Task<Contract> GetContractAsync(string contractHash)
        {
            return await Contract.GetContractAsync(_client, contractHash);
        }
    }
}