using Helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Helper
{

    /// <summary>
    /// API format
    /// http://pastebin.com/NeKQQfqg
    /// </summary>
    public class APIClient
    {

        public static bool SetVideo(string video, string appId)
        {
            var url = Config.Data.SetVideoUrl
                .Replace("{apikey}", Config.Data.ApiKey)
                .Replace("{appid}", appId);
            try
            {
                using (var wc = new WebClient())
                {
                    var data = new NameValueCollection();
                    data["video"] = video;
                    var response = wc.UploadValues(url, "POST", data);

                    if (Encoding.Default.GetString(response) == "success")
                        return true;
                }
            } 
            catch(Exception e)
            {
                Log.Error(e.Message);
            }
            return false;
        }


    }

}
