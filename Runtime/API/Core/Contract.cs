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
using J4FApi.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace J4FApi.Contracts
{
    public class Contract
    {
        public string Hash { get; set; }
        private HttpClient Client { get; set; }
        private ContractInfoDto ContractInfo { get; set; }
        private InfoByHashDto ContractFullInfo { get; set; }

        private Contract(HttpClient client, string contractHash, ContractInfoDto contractInfo, InfoByHashDto contractFullInfo)
        {
            Client = client;
            Hash = contractHash;
            ContractInfo = contractInfo;
            ContractFullInfo = contractFullInfo;
        }
        
        /// <summary>
        /// Static async method that behave like a constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="contractHash"></param>
        /// <returns>Contract instance</returns> 
        public static async Task<Contract> GetContractAsync(HttpClient client, string contractHash)  
        {       
            return await _GetContract(client, contractHash);
        }
        
        /// <summary>
        /// Get contract info and methods
        /// </summary>
        /// <param name="client"></param>
        /// <param name="contractHash"></param>
        /// <returns></returns>
        private static async Task<Contract> _GetContract(HttpClient client, string contractHash)
        {
            var response = await client.GetAsync($"/contract/byHash?Hash={contractHash}");
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var contractInfo = JsonConvert.DeserializeObject<ContractInfoDto>(responseJson);
            if (contractInfo == null)
            {
                UnityEngine.Debug.LogError("Contract info not defined");
                return null;
            }
            
            //Get functions of contract
            response = await client.GetAsync($"/contract/infoByHash?Hash={contractHash}");
            response.EnsureSuccessStatusCode();
            responseJson = await response.Content.ReadAsStringAsync();
            var contractFullInfo = JsonConvert.DeserializeObject<InfoByHashDto>(responseJson);
            
            //return contract object
            return new Contract(client, contractHash, contractInfo, contractFullInfo);
        }

        /// <summary>
        /// Get function of contract
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public ContractFunction GetFunction(string functionName)
        {
            try
            {
                if (ContractInfo == null || ContractFullInfo == null || ContractInfo.ContractHash == null)
                {
                    throw new Exception("Contract not found");
                }

                FunctionDto foundFunction = null;
                
                //Search function on public methods
                foreach (var function in ContractFullInfo.Methods.Public)
                {
                    if (function.Method == functionName)
                    {
                        foundFunction = function;
                    }
                }

                if (foundFunction == null)
                {
                    throw new Exception("Function not exists or its private");
                }

                return new ContractFunction(Client, Hash, foundFunction);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Contract.Error: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get RC10 transactions of contract
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Rc10TransactionDto>> GetRc10Transactions()
        {
            try
            {
                var response = await Client.GetAsync($"/contract/rc10Transactions?Hash={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var contractRc10Transactions = JsonConvert.DeserializeObject<List<Rc10TransactionDto>>(responseJson);
                return contractRc10Transactions;
            }
            catch (Exception e)
            {
                Debug.LogError("Contract.GetRc10Transactions.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Get RC20 transactions of contract
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Rc20TransactionDto>> GetRc20Transactions()
        {
            try
            {
                var response = await Client.GetAsync($"/contract/rc20Transactions?Hash={Hash}");
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var contractRc10Transactions = JsonConvert.DeserializeObject<List<Rc20TransactionDto>>(responseJson);
                return contractRc10Transactions;
            }
            catch (Exception e)
            {
                Debug.LogError("Contract.GetRc20Transactions.Error: " + e.Message);
                return null;
            }
        }
    }
}