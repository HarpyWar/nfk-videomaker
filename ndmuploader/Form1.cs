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
using System.Threading;
using System.Globalization;

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

            try
            {

                string jsonFile = null;
                for (int j = 0; j < files.Length; j++)
                {
                    if (IsFileLocked(new FileInfo(files[j])))
                        continue;

                    jsonFile = files[j];
                    break;
                }
                if (jsonFile == null)
                    Environment.Exit(0);

                Log.Info("Reading demo info file " + jsonFile);


                bool result;
                // lock file when processing
                using (var  fs = new FileStream(jsonFile, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        // open first available json file with demo info
                        var demo = Config.ReadJsonDemo(sr.ReadToEnd());

                        result = processDemo(demo);
                    }
                }
                // remove file is result is success and file was unlocked
                if (result)
                {
                    Log.Info("Remove " + jsonFile + " to close session");
                    File.Delete(jsonFile);
                }
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
            }

            Environment.Exit(0);
        }

        private bool processDemo(DemoItem demo)
        {
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
                // replace filename
                demo.local_videos[i].FileName = newVideoFile;

                // stop if no follow player
                if (demo.local_followplayer == false)
                    break;
            }

            string videoString = string.Empty;
            try
            {
                // set empty id's for  (replace null values)
                if (demo.local_followplayer == false)
                    videoString = demo.local_videos[0].YoutubeId;
                else
                    videoString = string.Join(" ", demo.local_videos.Select(v => v.YoutubeId)).Trim();
            }
            catch (Exception e)
            {
                Log.Error("Error when concat videostring. " + e.Message);
            }
            if (videoString == string.Empty)
            {
                Environment.Exit(1);
            }

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

                if (!string.IsNullOrEmpty(Config.Data.YoutubeUploadCompleteExec))
                    startMatchUploadCallbackProcess(demo);

                return true;
            }
            else
            {
                Log.Error("Could not setvideo with content: " + videoString);
            }
            return false;
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
            string vsnickname = "", nickname = "", gametype = demo.gametype;
            nickname = demo.players[pindex];
            //duel
            if (demo.players.Length == 2)
            {
                if (demo.players[pindex] == demo.players[0])
                    vsnickname = " vs " + demo.players[1];
                else
                    vsnickname = " vs " + demo.players[0];
            }
            else
            {
                if (demo.local_followplayer == false)
                    nickname = string.Join(", ", demo.players).Trim(new char[] { ',', ' ' });
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
                        vsnickname = " in massacre";
                }

            }

            var originalCulture = Thread.CurrentThread.CurrentCulture;
            // set current culture to English
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var recDate = APIClient.UnixTimeToDateTime(demo.date);
            var output = template
                .Replace("{nickname}", nickname)
                .Replace("{vsnickname}", vsnickname)
                .Replace("{gametype}", gametype)
                .Replace("{mapname}", demo.map)
                .Replace("{vsplayer}", vsnickname)
                .Replace("{demoid}", demo.id.ToString())
                .Replace("{demourl}", demo.file)
                .Replace("{date}", recDate.ToLongDateString())
                .Replace("{playerlist}", string.Join(", ", demo.players).Trim(new char[] { ',', ' ' }));

            // restore culture
            Thread.CurrentThread.CurrentCulture = originalCulture;

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
                    .Replace("{playerlist}", Translit(string.Join(", ", demo.players).Trim(new char[] { ',', ' ' })))
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




        // RU -> EN
        public static string Translit(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }


        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
