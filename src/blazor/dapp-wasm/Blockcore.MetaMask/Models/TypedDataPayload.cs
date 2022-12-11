﻿using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Blockcore.AtomicSwaps.MetaMask.Models
{
    public record struct Message(string contents);


    public class TypedDataPayload<T>
    {
        public Dictionary<string, TypeMemberValue[]> Types { get; set; } = new();
        public string? PrimaryType { get; set; }
        public Domain? Domain { get; set; }
        public T? Message { get; set; }


        public string ToJson()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                    {
                        ProcessDictionaryKeys = false
                    }
                },

                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, serializerSettings);
        }
    }

    public class Domain
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public BigInteger? ChainId { get; set; }
    }

    public class TypeMemberValue
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}
