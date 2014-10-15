using System;
using System.Collections.Generic;
using System.Text;

namespace Helper
{
    public class DemoItem
    {
        public int id;
        public string file;
        public string gametype;
        public int date;
        public int duration;
        public string map;
        public string[] players;

        /// <summary>
        /// This field is internal and does not return by a server
        /// </summary>
        public VideoItem[] local_videos;
        public string local_appid;
        public bool local_followplayer;
        public bool local_completed;
    }
}
