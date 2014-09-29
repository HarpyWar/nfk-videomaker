using CG.Web.MegaApiClient;
using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ndmuploader
{
    class Mega
    {
        public static string Upload(string fileName)
        {
            try
            {
                var client = new MegaApiClient();

                client.Login(Config.Data.MegaUserName, Config.Data.MegaPassword);
                var nodes = client.GetNodes();

                var root = nodes.Single(n => n.Type == NodeType.Root);
                var myFile = client.Upload(fileName, root);

                var downloadUrl = client.GetDownloadLink(myFile);
                return downloadUrl.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return null;
            }
        }
    }
}
