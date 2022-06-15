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
using J4FApi.Payloads.Contracts;
using J4FApi.Dto.Contracts;
using J4FApi.Enums;
using J4FApi.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace J4FApi.Contracts
{
    public class ContractFunction
    {
        /// <summary>
        /// Pointer to HttpClient
        /// </summary>
        private HttpClient Client { get; }
        
        /// <summary>
        /// Hash of contract
        /// </summary>
        private string ContractHash { get; }
        
        private TypeFunctionEnum TypeFunction { get; }

        /// <summary>
        /// Method name of function
        /// </summary>
        public string Method { get; }
        
        /// <summary>
        /// Parameters of function
        /// </summary>
        private Dictionary<string,string> Parameters { get; }
        
        /// <summary>
        /// Return type of function
        /// </summary>
        private string Return { get; }

        public ContractFunction(HttpClient client, string contractHash, FunctionDto function)
        {
            Client = client;
            ContractHash = contractHash;
            Method = function.Method;
            Parameters = function.Parameters;
            Return = function.Return;
            TypeFunction = string.IsNullOrEmpty(Return) ? TypeFunctionEnum.Write : TypeFunctionEnum.Read;
        }
        
        /// <summary>
        /// Execute a read function on the contract
        /// </summary>
        /// <param name="functionInput"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> CallReadAsync(params object[] functionInput)
        {
            try
            {
                if (TypeFunction == TypeFunctionEnum.Write || TypeFunction == TypeFunctionEnum.NotDefined)
                {
                    throw new Exception("Function "+Method+" is not a read function. Use CallWriteAsync");
                }

                //Parse method
                var methodParsed = await Utils.Parse(Client, Method);
            
                //Check parameters
                if (functionInput.Length != Parameters.Count)
                {
                    throw new Exception("Invalid number of parameters. Expected: " + Parameters.Count + " | Defined: " + functionInput.Length);
                }

                //Parse parameters
                var parsedParams = new string[Parameters.Count];
                var paramPosition = 0;
                foreach (var param in functionInput)
                {
                    parsedParams[paramPosition] = await Utils.Parse(Client, (string) param);
                    paramPosition++;
                }

                //Make payload
                var payload = new CallFunctionPayload
                {
                    Method = methodParsed,
                    Parameters = parsedParams
                };

                return await CallReadAPI(payload);
            }
            catch (Exception e)
            {
                Debug.LogError("Contract.Function.CallReadAsync.Error: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Call a read function on J4F API
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private async Task<string> CallReadAPI(CallFunctionPayload payload)
        {
            try
            {
                var payloadSerialized = JsonConvert.SerializeObject(payload);
                
                var payloadParsed = await Utils.Parse(Client, payloadSerialized);
                
                var response = await Client.PostAsync($"/contract/callReadFunction", new StringContent(JsonConvert.SerializeObject(new
                {
                    Hash = ContractHash,
                    CallCode = payloadParsed
                })));
                
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseCallRead = JsonConvert.DeserializeObject<CallReadDto>(responseJson);

                if (string.IsNullOrEmpty(responseCallRead.Error))
                {
                    return responseCallRead.Output;
                }
                
                return responseCallRead.Error;
            }
            catch (Exception e)
            {
                Debug.LogError("Contract.Function.CallReadAPI.Error: " + e.Message);
            }

            return null;
        }
        
        /// <summary>
        /// Execute a write function on the contract
        /// </summary>
        /// <param name="functionInput"></param>
        /// <returns>Data parsed to be execute method on contract when send transaction</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> CallWriteAsync(params object[] functionInput)
        {
            try
            {
                if (TypeFunction == TypeFunctionEnum.Read || TypeFunction == TypeFunctionEnum.NotDefined)
                {
                    throw new Exception("Function "+Method+" is not a write function. Use CallReadAsync");
                }
                
                //Parse method
                var methodParsed = await Utils.Parse(Client, Method);
            
                //Check parameters
                if (functionInput.Length != Parameters.Count)
                {
                    throw new Exception("Invalid number of parameters. Expected: " + Parameters.Count + " | Defined: " + functionInput.Length);
                }

                //Parse parameters
                var parsedParams = new string[Parameters.Count];
                var paramPosition = 0;
                foreach (var param in functionInput)
                {
                    parsedParams[paramPosition] = await Utils.Parse(Client, (string) param);
                    paramPosition++;
                }

                //Make payload
                var payload = new CallFunctionPayload
                {
                    Method = methodParsed,
                    Parameters = parsedParams
                };

                var payloadSerialized = JsonConvert.SerializeObject(payload);
                
                return await Utils.Parse(Client, payloadSerialized);
            }
            catch (Exception e)
            {
                Debug.LogError("Contract.Function.CallWriteAsync.Error: " + e.Message);
                return null;
            }
        }
    }
}