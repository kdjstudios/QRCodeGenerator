using Microsoft.AspNetCore.Mvc;
using QRGenerator.Models;
using System.Diagnostics;
using ZXing.QrCode.Internal;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing.QrCode;

namespace QRGenerator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Index(string qrText)
        {
            if (!string.IsNullOrEmpty(qrText))
            {
                Byte[] byteArray;
                var width = 250; // width of the Qr Code   
                var height = 250; // height of the Qr Code   
                var margin = 0;
                var qrCodeWriter = new ZXing.BarcodeWriterPixelData
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = margin
                    }
                };
                var pixelData = qrCodeWriter.Write(qrText);

                //https://github.com/micjahn/ZXing.Net
                //https://jeremylindsayni.wordpress.com/2016/04/02/how-to-read-and-create-barcode-images-using-c-and-zxing-net/

                // creating a bitmap from the raw pixel data; if only black and white colors are used it makes no difference   
                // that the pixel data ist BGRA oriented and the bitmap is initialized with RGB   
                using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                {
                    using (var ms = new MemoryStream())
                    {
                        var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        try
                        {
                            // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image   
                            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                        }
                        finally
                        {
                            bitmap.UnlockBits(bitmapData);
                        }
                        // save to stream as PNG   
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byteArray = ms.ToArray();
                    }
                }
                return View(byteArray);
            }

            return View();
        }
    }
}
