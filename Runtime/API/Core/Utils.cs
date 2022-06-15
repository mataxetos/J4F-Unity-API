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
using J4FApi.Dto.Utilities;
using UnityEngine;
using Newtonsoft.Json;

namespace J4FApi.Utilities
{
    public class Utils
    {
        /// <summary>
        /// Parse string using J4F API
        /// </summary>
        /// <param name="dataToParse"></param>
        /// <returns></returns>
        public static async Task<string> Parse(HttpClient client, string dataToParse)
        {
            try
            {
                var response = await client.PostAsync($"/j4f/parse", new StringContent(JsonConvert.SerializeObject(new
                {
                    Data = dataToParse
                })));
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var dataParsed = JsonConvert.DeserializeObject<ParseDto>(responseJson);
                return dataParsed?.Data;
            }
            catch (Exception e)
            {
                Debug.LogError("Utilities.Parse.Error: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Parse string using J4F API
        /// </summary>
        /// <param name="dataToParse"></param>
        /// <returns></returns>
        public static async Task<string> UnParse(HttpClient client, string dataToParse)
        {
            try
            {
                var response = await client.PostAsync($"/j4f/unparse", new StringContent(JsonConvert.SerializeObject(new
                {
                    Data = dataToParse
                })));
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var dataUnParsed = JsonConvert.DeserializeObject<UnParseDto>(responseJson);
                return dataUnParsed?.Data;
            }
            catch (Exception e)
            {
                Debug.LogError("Utilities.UnParse.Error: " + e.Message);
                return null;
            }
        }
    }
}