using Helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ndmscheduler
{
    public class APIClient : Helper.APIClient
    {
        public static DemoItem GetDemo(string appId)
        {
            var url = Config.Data.GetDemoUrl
                .Replace("{apikey}", Config.Data.ApiKey)
                .Replace("{appid}", appId);
            try
            {
                using (var wc = new WebClient())
                {
                    Log.Info("Get match from " + url);
                    var response = wc.DownloadString(url);
                    var jsonItem = Newtonsoft.Json.JsonConvert.DeserializeObject<DemoItem>(response);

                    return jsonItem;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return null;
        }


        public static bool DownloadDemo(string url, string fileName)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadFile(url, fileName);
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return false;
        }


    }
}
