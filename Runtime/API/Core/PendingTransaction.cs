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
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using J4FApi.Dto.Transactions;
using J4FApi.Maths;

namespace J4FApi.Transactions
{
    public class PendingTransaction
    {
        public string Hash { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public BigDecimal Amount { get; set; }
        public BigDecimal GasPrice { get; set; }
        public int GasLimit { get; set; }
        public long TimeStamp { get; set; }
        public string Data { get; set; }
        private HttpClient Client { get; set; }

        private PendingTransaction(HttpClient client, PendingTransactionDto pendingTransactionDto)
        {
            Client = client;
            Hash = pendingTransactionDto.Hash;
            From = pendingTransactionDto.From;
            To = pendingTransactionDto.To;
            Amount = BigDecimal.Parse(pendingTransactionDto.Amount);
            GasPrice = BigDecimal.Parse(pendingTransactionDto.GasPrice);
            GasLimit = pendingTransactionDto.GasLimit;
            TimeStamp = pendingTransactionDto.TimeStamp;
            Data = pendingTransactionDto.Data;
        }
        
        /// <summary>
        /// Static async method that behave like a constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="transactionHash"></param>
        /// <returns>Transaction instance</returns> 
        public static async Task<PendingTransaction> GetPendingTransactionAsync(HttpClient client, string transactionHash)  
        {       
            return await _GetPendingTransaction(client, transactionHash);
        }

        /// <summary>
        /// Get transaction info
        /// </summary>
        /// <param name="client"></param>
        /// <param name="transactionHash"></param>
        /// <returns>Transaction instance</returns>
        private static async Task<PendingTransaction> _GetPendingTransaction(HttpClient client, string transactionHash)
        {
            try
            {
                var response = await client.GetAsync($"/j4f/pendingTransactionByHash?Hash={transactionHash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var pendingTransactionDto = JsonConvert.DeserializeObject<PendingTransactionDto>(responseJson);
                if (pendingTransactionDto?.Hash == null)
                {
                    throw new Exception("Transaction not found");
                }
                
                return new PendingTransaction(client, pendingTransactionDto);
            }
            catch (Exception e)
            {
                Debug.LogError("PendingTransaction.GetPendingTransaction.Error: " + e.Message);
                return null;
            }
        }
    }
}