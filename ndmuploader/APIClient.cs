using Helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ndmuploader
{
    public enum UploadService
    {
        Youtube,
        Mega
    }

    public class APIClient : Helper.APIClient
    {

        /// <summary>
        /// Upload with several attempts
        /// (if upload failed then exit program!)
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="service"></param>
        /// <return>result string from a remote service</return>
        public static string Upload2Cloud(string fileName, UploadService service, string arg1 = null, string arg2 = null)
        {
            int attempts = 0;
            string downloadUrl = string.Empty;
            while (true)
            {
                if (++attempts > Config.Data.UploadMaxAttempts)
                    break;

                if (!File.Exists(fileName))
                {
                    Log.Info("Could not find file " + fileName);
                    Environment.Exit(1);
                }

                var fsize = new FileInfo(fileName).Length;
                if (fsize > 0)
                    fsize = fsize / 1024 / 1024;

                Log.Info(string.Format("Uploading a file \"{0}\" ({1}MB) to {2} (attempt #{3})", fileName, fsize,service, attempts));

                if (service == UploadService.Mega)
                    downloadUrl = Mega.Upload(fileName);
                else if (service == UploadService.Youtube)
                    downloadUrl = Youtube.Upload(fileName, arg1, arg2);

                if (downloadUrl != null)
                    break;
            }
            if (downloadUrl == null)
            {
                Log.Info("Could not upload file, max attempts exceed.");
                Environment.Exit(1);
            }

            Log.Info("Upload completed: " + downloadUrl);
            return downloadUrl;
        }
    }

}
