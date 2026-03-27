using System;
using System.IO;
using QRCoder;

namespace WaitifyApi.Helpers;

public class QRCodeHelper
{
    public static void GenerateToFile(
        QRCodeData url,
        // string filePath, 
        int pixelsPerModule = 20
        )
    {
        // using var qrGenerator = new QRCodeGenerator();
        // using var qrData = qrGenerator.CreateQrCode(url);
        var qrCode = new PngByteQRCode(url);

        // Get PNG as byte array
        var pngBytes = qrCode.GetGraphic(pixelsPerModule);

        // return pngBytes;

        // Save to file
        // File.WriteAllBytes(filePath, pngBytes);
    }
}