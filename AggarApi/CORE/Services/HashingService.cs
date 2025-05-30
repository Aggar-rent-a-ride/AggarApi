using CORE.DTOs.Keys;
using CORE.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CORE.Services
{
    public class HashingService : IHashingService
    {
        private readonly string secretKey;

        public HashingService(IOptions<SecretKeys> options)
        {
            secretKey = "MSaid,Naru,Dos,Esraa=AggarTeam.";
        }

        public string Hash(string token)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }
}
