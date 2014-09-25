using AForge.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Helper;


namespace ndm2mp4
{
    class VideoMaker
    {

        VideoFileWriter writer;
        public VideoMaker(string filename)
        {
            // create instance of video writer
            writer = new VideoFileWriter();

            try
            {
                // create new video file
                writer.Open(filename, Config.Data.VideoWidth, Config.Data.VideoHeight, Config.Data.VideoFps, VideoCodec.MPEG4, Config.Data.VideoBitrate);
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
            }
        }

        /// <summary>
        /// Add new image frame into a video
        /// </summary>
        /// <param name="image"></param>
        public void AddFrame(Bitmap image)
        {
            var newImage = new Bitmap(Config.Data.VideoWidth, Config.Data.VideoHeight);
            // cut region of source image
            CopyRegionIntoImage(image, new Rectangle(Config.Data.VideoPaddingLeft, Config.Data.VideoPaddingTop, Config.Data.VideoWidth, Config.Data.VideoHeight), ref newImage, new Rectangle(0, 0, Config.Data.VideoWidth, Config.Data.VideoHeight));
            writer.WriteVideoFrame(newImage);
        }

        private void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }

        public void SaveAndClose()
        {
            if (writer != null && writer.IsOpen)
                writer.Close();
        }
    }
}
