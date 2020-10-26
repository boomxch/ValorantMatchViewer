using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ValorantMatchViewer
{
    public class MatchManager
    {
        public MatchManager()
        {
            var roundBlue = new ValorantOcr(ValorantOcr.OcrWhiteVariablesEnum.Round, ValorantOcr.AreaEnum.RoundBlue);
            var roundRed = new ValorantOcr(ValorantOcr.OcrWhiteVariablesEnum.Round, ValorantOcr.AreaEnum.RoundRed);
            var time = new ValorantOcr(ValorantOcr.OcrWhiteVariablesEnum.Time, ValorantOcr.AreaEnum.Time);
            var killLog1 = new ValorantOcr(ValorantOcr.OcrWhiteVariablesEnum.KillLog, ValorantOcr.AreaEnum.KillLog1);
            var killLog2 = new ValorantOcr(ValorantOcr.OcrWhiteVariablesEnum.KillLog, ValorantOcr.AreaEnum.KillLog2);
            var killLog3 = new ValorantOcr(ValorantOcr.OcrWhiteVariablesEnum.KillLog, ValorantOcr.AreaEnum.KillLog3);

            var sw = new Stopwatch();
            sw.Start();
            var vm = new VideoManager();
            var count = -1;
            var roundStartTime = new List<int>();
            var timeNow = 0;
            foreach (var bmp in vm.GetScreenshotsFromVideo("Video/20201018_774049452_VALORANT_1.mp4", 5 * 1000))
            {
                // time,round と キルログで閾値を変えたほうが良い logは武器とキャラと数字を消さないといけない
                // timeやラウンドは前後の結果をリスペクトしないとまともに動かない
                // 同様にキルログは表示された前後の5秒間(?)を数フレームずつ解析して平均的な値をとりたい(それぞれのフレームで誰と誰が戦ったかを求めてその結果の平均)
                bmp.Save("testAll.png");
                var d1 = time.GetTextFromImage(bmp);
                var d2 = roundBlue.GetTextFromImage(bmp);
                var d3 = roundRed.GetTextFromImage(bmp);
                var k1 = killLog1.GetTextFromImage(bmp);
                var k2 = killLog2.GetTextFromImage(bmp);
                var k3 = killLog3.GetTextFromImage(bmp);

                if (!int.TryParse(d2, out var test) || !int.TryParse(d3, out var test2))
                    MessageBox.Show(d2 + " " + d3);

                if (int.Parse(d2) + int.Parse(d3) > count)
                {
                    count = int.Parse(d2) + int.Parse(d3);
                    roundStartTime.Add(timeNow);
                }

                timeNow += 5;
            }
            sw.Stop();
            MessageBox.Show(sw.Elapsed.TotalSeconds + " S");
        }
    }

    public class ValorantOcr
    {
        private OcrManager ocr;
        private ImageProcessor ip;
        private (Point XY, Size WH) area;

        public enum OcrWhiteVariablesEnum
        {
            Time,
            Round,
            KillLog
        }

        public enum AreaEnum
        {
            RoundBlue,
            RoundRed,
            Time,
            KillLog1,
            KillLog2,
            KillLog3,
            KillLog4,
            KillLog5,
            KillLog6
        }

        public ValorantOcr(OcrWhiteVariablesEnum owve, AreaEnum ae)
        {
            var whiteVariables = GetWhiteVariablesFromEnum(owve);
            area = GetCaptureAreaFromEnum(ae);
            var isNumberOnly = GetIsNumberOnlyCaptureFromEnum(owve);

            ocr = new OcrManager(whiteVariables, isNumberOnly);
            ip = new ValorantImageProcessor();
        }

        public ValorantOcr(string whiteVariables, Point xy, Size wh)
        {
            ocr = new OcrManager(whiteVariables);
            ip = new ValorantImageProcessor();
            area = (xy, wh);
        }

        public string GetTextFromImage(Bitmap img)
        {
            img = ip.TrimImageFromFullHd(img, area.XY, area.WH);
            ip.ConvertImage(img);
            img.Save("test.png");
            return ocr.GetTextFromBitmap(img);
        }

        private string GetWhiteVariablesFromEnum(OcrWhiteVariablesEnum owve)
        {
            var str = owve switch
            {
                OcrWhiteVariablesEnum.Time => "0123456789:",
                OcrWhiteVariablesEnum.Round => "0123456789",
                OcrWhiteVariablesEnum.KillLog => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzⅢⅣⅤⅥ",
                _ => ""
            };

            return str;
        }

        private (Point XY, Size WH) GetCaptureAreaFromEnum(AreaEnum ae)
        {
            (Point XY, Size WH) area = (new Point(), new Size());

            switch (ae)
            {
                case AreaEnum.RoundBlue:
                    area.XY.X = 805;
                    area.XY.Y = 35;
                    area.WH.Width = 40;
                    area.WH.Height = 30;
                    break;
                case AreaEnum.RoundRed:
                    area.XY.X = 1080;
                    area.XY.Y = 35;
                    area.WH.Width = 40;
                    area.WH.Height = 30;
                    break;
                case AreaEnum.Time:
                    area.XY.X = 930;
                    area.XY.Y = 32;
                    area.WH.Width = 65;
                    area.WH.Height = 30;
                    break;
                case AreaEnum.KillLog1:
                    area.XY.X = 1480;
                    area.XY.Y = 108;
                    area.WH.Width = 370;
                    area.WH.Height = 18;
                    break;
                case AreaEnum.KillLog2:
                    area.XY.X = 1480;
                    area.XY.Y = 145;
                    area.WH.Width = 370;
                    area.WH.Height = 18;
                    break;
                case AreaEnum.KillLog3:
                    area.XY.X = 1480;
                    area.XY.Y = 182;
                    area.WH.Width = 370;
                    area.WH.Height = 18;
                    break;
                case AreaEnum.KillLog4:
                    area.XY.X = 1480;
                    area.XY.Y = 219;
                    area.WH.Width = 370;
                    area.WH.Height = 18;
                    break;
                case AreaEnum.KillLog5:
                    area.XY.X = 1480;
                    area.XY.Y = 256;
                    area.WH.Width = 370;
                    area.WH.Height = 18;
                    break;
                case AreaEnum.KillLog6:
                    area.XY.X = 1480;
                    area.XY.Y = 293;
                    area.WH.Width = 370;
                    area.WH.Height = 18;
                    break;
                default:
                    break;
            }

            return area;
        }

        private bool GetIsNumberOnlyCaptureFromEnum(OcrWhiteVariablesEnum owve)
        {
            return owve == OcrWhiteVariablesEnum.Round;
        }
    }

    public class ValorantImageProcessor : ImageProcessor
    {
        public override void ConvertImage(Bitmap bitmap)
        {
            const int threshold = 210;

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            var buf = new byte[bitmap.Width * bitmap.Height * 4];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            for (var i = 0; i < buf.Length;)
            {
                var num = (buf[i] > threshold && buf[i + 1] > threshold && buf[i + 2] > threshold) ? (byte)255 : (byte)0;
                buf[i++] = num;
                buf[i++] = num;
                buf[i++] = num;
                i++;
            }
            Marshal.Copy(buf, 0, data.Scan0, buf.Length);
            bitmap.UnlockBits(data);
        }

        public Bitmap TrimAndConvertImage(Bitmap bmp, Point fhdxy, Size fhdwh)
        {
            bmp = TrimImageFromFullHd(bmp, fhdxy, fhdwh);
            ConvertImage(bmp);
            return bmp;
        }
    }
}
