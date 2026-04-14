using System;
using System.IO;
using QRCoder;

namespace WaitifyApi.Helpers;

public sealed class QRCodeGeneratorService
{
    public async Task<string> GenerateQRCode(
        string url
        // string filePath
        )
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(qrCodeData);
        byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
        string base64String = Convert.ToBase64String(qrCodeAsPngByteArr, 0, qrCodeAsPngByteArr.Length);
        var qrCodeGenerated = $"<img src='data:image/png;base64,{base64String}' />";

        return qrCodeGenerated;

        // Save to file
        // File.WriteAllBytes(filePath, pngBytes);
    }
}