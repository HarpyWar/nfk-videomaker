using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Helper;
using System.IO;
using System.Diagnostics;

namespace ndmuploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Visible = false;

            Config.Load();
            var files = Directory.GetFiles(Config.Data.TempDir, "*.json");
            if (files.Length == 0)
                Environment.Exit(0);

            var jsonFile = files.First();
            Log.Info("Reading demo info file " + jsonFile);

            // open first available json file with demo info
            var demo = Config.ReadJsonDemo(jsonFile);

            // get demoFile from url (it was downloaded by ndmscheduler)
            var demoFile = Path.Combine(Config.Data.TempDir, Path.GetFileName(demo.file));

            if (Config.Data.MegaUpload)
            {
                // upload demo file into cloud
                var demourl = APIClient.Upload2Cloud(demoFile, UploadService.Mega);
                // if file uploaded then replace source url
                if (demourl != null) 
                    demo.file = demourl;
            }

            // upload each video (do it after processing ALLvideos because of more robust)
            for (int i = 0; i < demo.players.Length; i++)
            {
                // if video file already uploaded to youtube (from a previous unfinished session)
                // get youtube id from the filename
                var pattern = demo.local_videos[i].FileName.Replace("." + Config.Data.VideoOutputExtension, "") + "_";// 1234_1.mp4 -> 1234_1_
                if ((demo.local_videos[i].YoutubeId = FindLocalVideoFile(pattern)) != null) 
                    continue;

                if (!File.Exists(demo.local_videos[i].FileName))
                {
                    Log.Error("Could not find " + demo.local_videos[i].FileName);
                    Environment.Exit(1);
                }

                var title = FormatDemoInfo(Config.Data.VideoTitle, demo, i);
                var description = FormatDemoInfo(Config.Data.VideoDescription, demo, i);
                // upload video to youtube and return id
                demo.local_videos[i].YoutubeId = APIClient.Upload2Cloud(demo.local_videos[i].FileName, UploadService.Youtube, title, description);

                // add youtube id in filename (1234_1.mp4 -> 1234_1_youtubeid.mp4)
                var newVideoFile = demo.local_videos[i].FileName.Replace("." + Config.Data.VideoOutputExtension, "_" + demo.local_videos[i].YoutubeId + "." + Config.Data.VideoOutputExtension);
                Log.Info("Rename " + demo.local_videos[i].FileName + " -> " + newVideoFile);
                File.Move(demo.local_videos[i].FileName, newVideoFile);

                // stop if no follow player
                if (demo.local_followplayer == false)
                    break;
            }

            var videoString = string.Join(" ", demo.local_videos.Select(v => v.YoutubeId));
            // send request with video ids to the statistics site
            if (APIClient.SetVideo(demo.id, videoString, demo.local_appid))
            {

                // remove all video files only
                foreach (var v in demo.local_videos)
                {
                    Log.Info("Removing " + v.FileName);
                    File.Delete(v.FileName);

                    // stop if no follow player
                    if (demo.local_followplayer == false)
                        break;
                }
                // remove demo file
                Log.Info("Removing " + demoFile);
                File.Delete(demoFile);
                // remove json file
                Log.Info("Removing " + jsonFile);
                File.Delete(jsonFile);

                Log.Info("Well done!");


                if (!string.IsNullOrEmpty(Config.Data.YoutubeUploadCompleteExec))
                    startMatchUploadCallbackProcess(demo);
            }
            Environment.Exit(0);
        }


        /// <summary>
        /// Find video file by pattern and return string from the pattern (youtube id)
        /// </summary>
        /// <param name="startsWith">1234_123_</param>
        /// <returns>1234_123_xxx.mp4 -> xxx</returns>
        private string FindLocalVideoFile(string startsWith)
        {
            foreach (var f in Directory.GetFiles(Config.Data.TempDir, "*." + Config.Data.VideoOutputExtension))
            {
                if (f.StartsWith(startsWith))
                {
                    string youtubeId = f.Substring(startsWith.Length, f.Length - ("." + Config.Data.VideoOutputExtension).Length);
                    if (youtubeId != string.Empty)
                        return youtubeId;
                }
            }
            return null;
        }

        private static string FormatDemoInfo(string template, DemoItem demo, int pindex)
        {
            string vsplayer = "", gametype = demo.gametype;
            //duel
            if (demo.players.Length == 2)
            {
                if (demo.players[pindex] == demo.players[0])
                    vsplayer = " vs " + demo.players[1];
                else
                    vsplayer = " vs " + demo.players[0];
            }
            else
            {
                // teamplay
                if (gametype == "CTF" || gametype == "TDM" || gametype == "DOM")
                {
                    var playersin1team = Math.Ceiling((double)demo.players.Length / 2);
                    var playersin2team = demo.players.Length - playersin1team;

                    gametype = string.Format("{0}x{1} {2}", playersin1team, playersin2team, gametype);
                }
                // ffa
                else
                    vsplayer = " in massacre";
            }
            var recDate = APIClient.UnixTimeToDateTime(demo.date);
            var output = template
                .Replace("{nickname}", demo.local_followplayer ? demo.players[pindex]: string.Join(", ", demo.players).Trim(new char[] { ',', ' ' }))
                .Replace("{vsnickname}", demo.local_followplayer ? vsplayer : string.Empty)
                .Replace("{gametype}", gametype)
                .Replace("{mapname}", demo.map)
                .Replace("{vsplayer}", vsplayer)
                .Replace("{demoid}", demo.id.ToString())
                .Replace("{demourl}", demo.file)
                .Replace("{date}", recDate.ToShortDateString() + " " + recDate.ToShortTimeString())
                .Replace("{playerlist}", string.Join("\n", demo.players));
            return replaceBadString(output);
        }

        static string goodChars = "\n\r абвгдеежзиклмнопрстуфхцчшщъюьэюяАБВГДЕЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЮЬЭЮЯabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890~!@#$%^&*()_+`-=/?.>,<\\|'\";:]}[{";

        private static string replaceBadString(string input)
        {
            char[] str = input.ToCharArray();
            for (int i = 0; i < str.Length; i++)
                if (!goodChars.Contains(str[i]))
                    str[i] = '-';
            return new string(str);
        }

        /// <summary>
        /// Start process when video is uploaded
        /// </summary>
        /// <param name="videoId"></param>
        private void startMatchUploadCallbackProcess(DemoItem demo)
        {
            var tmp = Config.Data.YoutubeUploadCompleteExec.Split(' ');
            var fileName = tmp[0];
            var args = new StringBuilder();
            for (int i = 1; i < tmp.Length; i++)
                args.Append(" " + tmp[i]
                    .Replace("{demoid}", demo.id.ToString())
                    .Replace("{gametype}", demo.gametype)
                    .Replace("{players}", string.Join(", ", demo.players).Trim(new char[] { ',', ' ' }))
                    );
            
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = replaceBadString(args.ToString()).Trim(),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            try
            {
                Log.Info(string.Format("Starting {0}{1}", fileName, replaceBadString(args.ToString())));
                p.Start();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

    }
}
