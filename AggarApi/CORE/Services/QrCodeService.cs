using CORE.DTOs.Keys;
using CORE.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CORE.Services
{
    public class QrCodeService : IQrCodeService
    {
        private string qrSecretKey;

        public QrCodeService(IOptions<SecretKeys> options)
        {
            qrSecretKey = "PleaseDon'tUnLockOurQrSecrectKey,AndGiveUsAnIntern(:";

        }

        public string GenerateQrHashToken(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(qrSecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        public byte[] GenerateQrCode(string token)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            byte[] qrCodeImage =  qrCode.GetGraphic(20);

            return qrCodeImage;
        }
    }
}
