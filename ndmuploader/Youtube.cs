using Google.GData.Client;
using Google.GData.Client.ResumableUpload;
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
        int chunkSize = 2; //MB
        ResumableUploader ru = null;
        ClientLoginAuthenticator ya;
        string videoId = null;

        Video newVideo;

        public Youtube()
        {
            ru = new ResumableUploader(chunkSize);
            ya = new ClientLoginAuthenticator(Config.Data.YoutubeAppName, ServiceNames.YouTube, Config.Data.YoutubeUserName, Config.Data.YoutubePassword);
            ya.DeveloperKey = Config.Data.YoutubeDeveloperKey;

            newVideo = new Video();

            // add the upload uri to video
            AtomLink link = new AtomLink("http://uploads.gdata.youtube.com/resumable/feeds/api/users/" + Config.Data.YoutubeUserName.Split('@')[0] + "/uploads");
            link.Rel = ResumableUploader.CreateMediaRelation;
            newVideo.YouTubeEntry.Links.Add(link);
        }

        public string Upload(string fileName, string title, string description)
        {
            byte[] chunk = new byte[chunkSize*1024];
            try
            {
                newVideo.YouTubeEntry.Private = false;
                
                // https://developers.google.com/youtube/2.0/reference#youtube_data_api_tag_media:description
                newVideo.Title = title
                    .Replace("<", "[").Replace(">", "]") // "<" and ">" are not allowed
                    .Substring(0, title.Length > 100 ? 100 : title.Length); // max 100 symbols
                newVideo.Description = description.Replace("<", "[")
                    .Replace(">", "]");

                newVideo.Tags.Add(new MediaCategory("Games", YouTubeNameTable.CategorySchema)); // category
                newVideo.Keywords = Config.Data.VideoKeyWords;
                
                newVideo.Private = false;
                newVideo.MediaSource = new MediaFileSource(fileName, MediaFileSource.GetContentTypeForFileName(fileName));

                // async upload
                //ru.InsertAsync(ya, newVideo.YouTubeEntry, u);

                var response = ru.Insert(ya, newVideo.YouTubeEntry);
                Log.Info("Response: " + response.ResponseUri.PathAndQuery);
                videoId = response.ResponseUri.PathAndQuery.Split('/').Last();

                AddVideoToPlayList(videoId);
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
            return videoId;
        }

        private static Feed<Playlist> AddVideoToPlayList(string videoId)
        {
            var request = DoYouTubeRequest();
            Feed<Playlist> userPlaylists = request.GetPlaylistsFeed(Config.Data.YoutubeUserName.Split('@')[0]);

            foreach (Playlist p in userPlaylists.Entries)
            {
                if (p.Title == Config.Data.VideoPlayList)
                {
                    var pm = getPlayListMember(videoId, p);
                    //p.Summary = "updated summary " + DateTime.Now; // update playlist description
                    request.AddToPlaylist(p, pm);
                }
            }
            return null;
        }

        private static PlayListMember getPlayListMember(string videoId, Playlist playlist)
        {
            YouTubeRequest request = DoYouTubeRequest();

            PlayListMember pm = new PlayListMember();
            pm.VideoId = videoId;
            return pm;
        }

        private static YouTubeRequest DoYouTubeRequest()
        {
            YouTubeRequestSettings settings = new YouTubeRequestSettings(Config.Data.YoutubeAppName, Config.Data.YoutubeDeveloperKey, Config.Data.YoutubeUserName, Config.Data.YoutubePassword);
            YouTubeRequest request = new YouTubeRequest(settings);
            return request;
        }
        
    }

}
