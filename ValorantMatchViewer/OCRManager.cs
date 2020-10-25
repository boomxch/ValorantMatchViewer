using System;
using System.Drawing;
using System.IO;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace ValorantMatchViewer
{
    public class OcrManager : IDisposable
    {
        private const string LangPath = @"tessdata";
        private const string LngStr = "eng";
        private readonly TesseractEngine _tesseract;

        public OcrManager(string whiteVariables = "", bool isNumberOnly = false)
        {
            _tesseract = new TesseractEngine(LangPath, LngStr);
            _tesseract.SetVariable("tessedit_char_whitelist", whiteVariables);
            _tesseract.DefaultPageSegMode = isNumberOnly ? PageSegMode.Count : PageSegMode.SingleLine;
        }

        public string GetTextFromBitmap(Bitmap bmp)
        {
            var pix = Pix.LoadTiffFromMemory(GetByteArrayFromImage(bmp));
            var page = _tesseract.Process(pix);
            var text = page.GetText().Replace(" ", "").Replace("　", "").Replace("\n", "");

            return text;
        }

        public void Dispose()
        {
            _tesseract?.Dispose();
        }

        private static byte[] GetByteArrayFromImage(Image bmp)
        {
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Tiff);
            return ms.GetBuffer();
        }
    }
}
