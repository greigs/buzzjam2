using System.Drawing;
using System.IO;
using NReco.ImageGenerator;

namespace HtmlRenderer
{
    public static class HtmlRenderer
    {
        public static Bitmap RenderUrl()
        {
            var htmlToImage = new NReco.ImageGenerator.HtmlToImageConverter();
            using (Stream outputStream = new MemoryStream())
            {
                htmlToImage.GenerateImageFromFile("http://" + "www.google.com", ImageFormat.Bmp, outputStream);
                outputStream.Position = 0;
                var bitmap = Bitmap.FromStream(outputStream);
                return (Bitmap) bitmap;
            }
        }
    }
}
