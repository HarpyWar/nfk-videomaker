using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Helper;
using System.Threading;

namespace ndmscheduler
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            this.Visible = false;

            // preload settings from config file
            Config.Load();

            DemoItem demo;
            string jsonFile;
            var appId = Config.Data.AppId;
            var appIndex = 0;
            string appFullId;

            // get appFullId here and remote demo info for processing
            while (true)
            {
                appIndex++;
                appFullId = appId + appIndex;

                // retrieve demo info that will be processed
                demo = APIClient.GetDemo(appFullId);
                if (demo == null)
                {
                    Log.Error("Empty demo info!");
                    Environment.Exit(1);
                }
                // file that created as a successfull result of this program
                jsonFile = Path.Combine(Config.Data.TempDir, demo.id + ".json");

                // if json file does not exist from a previous run
                if (!File.Exists(jsonFile))
                    break;
                // else appId will be increased again
            }
            demo.local_appid = appFullId;
            Log.Error("Using appid = " + appFullId);

            demo.file = System.Net.WebUtility.UrlDecode(demo.file);


            // get local path to demofile (filename extracted from the url)
            string demoFile = Path.Combine(Config.Data.TempDir, Path.GetFileName(demo.file));
            // download demo file
            Log.Info(String.Format("Downloading demo #{0} from {1} to {2}", demo.id, demo.file, Config.Data.TempDir));
            if (!APIClient.DownloadDemo(demo.file, demoFile))
            {
                Log.Error(String.Format("Could not download file"));
                APIClient.SetVideo(demo.id, "BAD_DEMO_FILE", demo.local_appid);
                Environment.Exit(0);
            }



            // read map width/height and set followplayer
            var ndm = new NFKDemo(demoFile);
            var map = ndm.GetMapSize();
            if (map.Width == 0 || map.Height == 0)
            {
                Log.Error("Bad nfk demo");
                // send back result with bad demo file
                APIClient.SetVideo(demo.id, "BAD_DEMO_FILE", demo.local_appid);
                Environment.Exit(1);
            }
            var logMapSize = string.Format("Map size is {0}x{1}. ", map.Width, map.Height);
            // if mapsize out of bounds video size then use follow player
            if ((map.Width * NFKDemo.BrickSize.Width) > Config.Data.VideoWidth || (map.Height * NFKDemo.BrickSize.Height) > Config.Data.VideoHeight)
            {
                demo.local_followplayer = true;
                logMapSize += string.Format("Enable following a player ({0} videos will be produced).", demo.players.Length);
            }
            else
            {
                logMapSize += "Disable following a player (1 video will be produced).";
                demo.local_followplayer = false;
            }
            Log.Info(logMapSize);

            demo.local_videos = new VideoItem[demo.players.Length];

            int playerid;
            int followplayerid;
            // create video in first-person of each player
            for (int i = 0; i < demo.players.Length; i++)
            {
                playerid = i + 1;
                // video filename that will be generated after ndm2video.exe processing
                var videoFile = Path.Combine(Config.Data.TempDir, string.Format("{0}_{1}.{2}", demo.id, playerid, Config.Data.VideoOutputExtension));
                demo.local_videos[i] = new VideoItem() { 
                    FileName = videoFile 
                };

                // do not create the same video it is was done earlier
                if (File.Exists(videoFile))
                    continue;

                Log.Info("Creating video through ndm2video.exe ...");

                followplayerid = (demo.local_followplayer) ? playerid : 0;
                // run process ndm2video.exe with video creation 
                var p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = Config.Data.Ndm2VideoFile,
                        WorkingDirectory = Path.GetDirectoryName(Config.Data.Ndm2VideoFile),
                        Arguments = string.Format("-demo \"{0}\" -duration {1} -playerid {2} -output \"{3}\"", demoFile, demo.duration, followplayerid, videoFile),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                try
                {
                    p.Start();

                    // wait for process exit
                    do { }
                    while (!p.WaitForExit(1000));

                    // process exit code (2 = success)
                    if (p.ExitCode != 2)
                    {
                        Log.Error("ndm2video.exe exiting with error!");
                        // remove file because it's bad
                        if (File.Exists(videoFile))
                        {
                            freeHangProcesses();
                            try {
                                Log.Info("Removing unfinished video " + videoFile);
                                File.Delete(videoFile); 
                            }
                            catch (Exception e) {
                                Log.Error(e.Message);
                            }
                        }
                        Environment.Exit(1);
                    }
                }
                finally
                {
                    if (p != null)
                        p.Close();
                }

                // stop if no follow player
                if (demo.local_followplayer == false)
                    break;
            }


            Log.Info("Demo was processed. Save data into " + jsonFile);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(demo, Newtonsoft.Json.Formatting.Indented);

            // save into json with all needed data that can be uploaded by ndmuploader.exe
            File.WriteAllText(jsonFile, json);
            Environment.Exit(0);
        }




        private void freeHangProcesses()
        {
            if (Config.Data.ExternalVideoCapture)
            {
                // 1) kill all external processes
                foreach (var round in Config.Data.ExternalToolRoundTrip)
                {
                    using (var p = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = "taskkill.exe",
                            Arguments = "/F /IM " + round.FileName,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    })
                    {
                        p.Start();
                    }
                }
            }
            // 2) kill nfk
            using (var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "taskkill.exe",
                    Arguments = "/F /IM " + Config.Data.GameExeFile,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                p.Start();
            }
            // just in case that previous commands was exited
            Thread.Sleep(10000);
        }


    }
}
