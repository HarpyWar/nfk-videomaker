using Google.GData.Client;
using Google.GData.Extensions.Location;
using Google.GData.Extensions.MediaRss;
using Google.GData.YouTube;
using Google.YouTube;
using Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ndmuploader
{
    class Youtube
    {
        public static string Upload(string fileName, string title, string description)
        {
            var settings = new YouTubeRequestSettings(Config.Data.YoutubeAppName, Config.Data.YoutubeDeveloperKey, Config.Data.YoutubeUserName, Config.Data.YoutubePassword);
            var request = new YouTubeRequest(settings);
            var newVideo = new Video();

            try
            {
                newVideo.Title = title;
                newVideo.Tags.Add(new MediaCategory("Games", YouTubeNameTable.CategorySchema)); // category
                newVideo.Tags.Add(new MediaCategory("NFK, Need For Kill", YouTubeNameTable.DeveloperTagSchema));
                newVideo.Keywords = Config.Data.VideoKeyWords;
                newVideo.Description = description;
            
                newVideo.YouTubeEntry.Private = false;
                newVideo.YouTubeEntry.MediaSource = new MediaFileSource(fileName, Config.Data.VideoMimeType);

                ((GDataRequestFactory)request.Service.RequestFactory).Timeout = 9999999;
                var createdVideo = request.Upload(newVideo);

                return createdVideo.VideoId;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return null;
            }
            finally
            {

            }
        }


    }

}
