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
            var settings = new YouTubeRequestSettings(Config.Data.YoutubeAppName, Config.Data.YoutubeDeveloperKey, Config.Data.YoutubeUserName, Config.Data.YoutubePassword)
                {
                    Timeout = int.MaxValue
                };
            var request = new YouTubeRequest(settings);
            var newVideo = new Video();

            try
            {
                // "<" and ">" are not allowed
                // https://developers.google.com/youtube/2.0/reference#youtube_data_api_tag_media:description
                newVideo.Title = title.Replace("<", "[").Replace(">", "]");
                newVideo.Description = description.Replace("<", "[").Replace(">", "]");

                newVideo.Tags.Add(new MediaCategory("Games", YouTubeNameTable.CategorySchema)); // category
                newVideo.Tags.Add(new MediaCategory("NFK", YouTubeNameTable.DeveloperTagSchema));
                newVideo.Keywords = Config.Data.VideoKeyWords;
                
                newVideo.YouTubeEntry.Private = false;
                newVideo.YouTubeEntry.MediaSource = new MediaFileSource(fileName, Config.Data.VideoMimeType);

                ((GDataRequestFactory)request.Service.RequestFactory).Timeout = int.MaxValue;
                var createdVideo = request.Upload(newVideo);

                return createdVideo.VideoId;
            }
            catch (GDataRequestException e)
            {
                Log.Error(e.Message + "\n(" + e.ResponseString + ")");
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return null;
            }
            finally
            {
                request = null;
            }
        }


    }

}
