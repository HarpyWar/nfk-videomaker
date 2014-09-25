using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace ndm2mp4
{
    public class Config : Helper.Config
    {

        // cmd args
        public static string DemoFile = null;
        public static string VideoFile = null;
        public static int DemoDuration;
        public static int PlayerNumber;


        /// <summary>
        /// Full path to basenfk
        /// </summary>
        public static string BaseNfkPath
        {
            get
            {
                if (string.IsNullOrEmpty(_baseNfkPath))
                    _baseNfkPath = Path.Combine(Path.GetDirectoryName(Data.GameExeFile), "basenfk");
                return _baseNfkPath;
            }
        }
        public static string _baseNfkPath;


        // watch for new images files counter
        // 5 first = game start count
        // 5 second = delay after game delay
        // 25 = images per a second
        public static int ImageLimitCount
        {
            get
            {
                if (_imageLimitCount == 0)
                    _imageLimitCount = (5 + DemoDuration + 5) * 25;
                return _imageLimitCount;
            }
        }
        public static int _imageLimitCount;




    }
}
