using System.Collections.Generic;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ValorantMatchViewer
{
    public class VideoManager
    {
        public VideoManager()
        {

        }

        /// <summary>
        /// .mp4から指定されたインターバルの間隔ごとにBitmapをキャプチャして返す
        /// </summary>
        /// <param name="filePath">.mp4のファイルパス</param>
        /// <param name="intervalMs">間隔 ミリ秒で指定</param>
        /// <param name="startTimeS">開始時間 秒で指定</param>
        /// <returns>スクリーンショット群</returns>
        public IEnumerable<Bitmap> GetScreenshotsFromVideo(string filePath, int intervalMs, int startTimeS = 0)
        {
            using var capture = new VideoCapture(filePath);
            var img = new Mat();
            for (var i = (int)(capture.Fps * startTimeS); i < capture.FrameCount; i += (int)(capture.Fps * intervalMs / 1000))
            {
                capture.PosFrames = i;
                capture.Read(img);
                yield return img.ToBitmap();
            }

            capture.Dispose();
        }
    }
}
