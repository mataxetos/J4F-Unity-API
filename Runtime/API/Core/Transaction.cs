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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using J4FApi.Dto.Contracts;
using UnityEngine;
using Newtonsoft.Json;
using J4FApi.Dto.Transactions;
using J4FApi.Maths;

namespace J4FApi.Transactions
{
    public class Transaction
    {
        public string BlockHash { get; set; }
        public string BlockHeight { get; set; }
        public string Hash { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public BigDecimal Amount { get; set; }
        public BigDecimal GasPrice { get; set; }
        public int GasLimit { get; set; }
        public long TimeStamp { get; set; }
        public string Data { get; set; }
        private HttpClient Client { get; set; }

        private Transaction(HttpClient client, TransactionDto transactionDto)
        {
            Client = client;
            BlockHash = transactionDto.BlockHash;
            BlockHeight = transactionDto.BlockHeight;
            Hash = transactionDto.Hash;
            From = transactionDto.From;
            To = transactionDto.To;
            Amount = BigDecimal.Parse(transactionDto.Amount);
            GasPrice = BigDecimal.Parse(transactionDto.GasPrice);
            GasLimit = transactionDto.GasLimit;
            TimeStamp = transactionDto.TimeStamp;
            Data = transactionDto.Data;
        }
        
        /// <summary>
        /// Static async method that behave like a constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="transactionHash"></param>
        /// <returns>Transaction instance</returns> 
        public static async Task<Transaction> GetTransactionAsync(HttpClient client, string transactionHash)  
        {       
            return await _GetTransaction(client, transactionHash);
        }

        /// <summary>
        /// Get transaction info
        /// </summary>
        /// <param name="client"></param>
        /// <param name="transactionHash"></param>
        /// <returns>Transaction instance</returns>
        private static async Task<Transaction> _GetTransaction(HttpClient client, string transactionHash)
        {
            try
            {
                var response = await client.GetAsync($"/j4f/transactionByHash?Hash={transactionHash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var transactionDto = JsonConvert.DeserializeObject<TransactionDto>(responseJson);
                if (transactionDto?.Hash == null)
                {
                    throw new Exception("Transaction not found");
                }
                
                return new Transaction(client, transactionDto);
            }
            catch (Exception e)
            {
                Debug.LogError("Transaction.GetTransaction.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Get RC10 transactions of transaction
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Rc10TransactionDto> GetRc10Transactions()
        {
            try
            {
                var response = await Client.GetAsync($"/contract/rc10TransactionsByTxnHash?Hash={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var contractRc10Transactions = JsonConvert.DeserializeObject<Rc10TransactionDto>(responseJson);

                if (contractRc10Transactions?.Hash == null)
                {
                    return null;
                }
                
                return contractRc10Transactions;
            }
            catch (Exception e)
            {
                Debug.LogError("Transaction.GetRc10Transactions.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Get RC20 transactions of transaction
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Rc20TransactionDto> GetRc20Transactions()
        {
            try
            {
                var response = await Client.GetAsync($"/contract/rc20TransactionsByTxnHash?Hash={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var contractRc10Transactions = JsonConvert.DeserializeObject<Rc20TransactionDto>(responseJson);

                if (contractRc10Transactions?.Hash == null)
                {
                    return null;
                }
                
                return contractRc10Transactions;
            }
            catch (Exception e)
            {
                Debug.LogError("Transaction.GetRc20Transactions.Error: " + e.Message);
                return null;
            }
        }
    }
}