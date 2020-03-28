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
                htmlToImage.GenerateImageFromFile("https://www.petmd.com/cat/behavior/4-facts-about-your-cats-brain", ImageFormat.Bmp, outputStream);
                outputStream.Position = 0;
                var bitmap = Image.FromStream(outputStream);
                return (Bitmap) bitmap;
            }
        }
    }
}
