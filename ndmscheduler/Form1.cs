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
            Log.Info("Using appid = " + appFullId);

            // get local path to demofile (filename extracted from the url)
            string demoFile = System.Net.WebUtility.UrlDecode(Path.Combine(Config.Data.TempDir, Path.GetFileName(demo.file)));
            // download demo file
            Log.Info(String.Format("Downloading demo #{0} from {1} to {2}", demo.id, demo.file, demoFile));
            if (!APIClient.DownloadDemo(demo.file, demoFile))
            {
                Log.Error(String.Format("Could not download file"));
                APIClient.SetVideo(demo.id, "BAD_DEMO_FILE", demo.local_appid);
                Environment.Exit(0);
            }
            // decode after download
            demo.file = System.Net.WebUtility.UrlDecode(demo.file);


            // read map width/height and set followplayer
            var ndm = new nfklib.NDemo.NFKDemo();
            var dem = ndm.Read(demoFile);
            if (dem == null)
            {
                Log.Error("Bad nfk demo");
                // send back result with bad demo file
                APIClient.SetVideo(demo.id, "BAD_DEMO_FILE", demo.local_appid);
                Environment.Exit(1);
            }
            // override playerlist with players fetched from demo
            demo.players = new string[dem.Players.Count];
            for (int i = 0; i < dem.Players.Count; i++)
            {
                demo.players[i] = nfklib.Helper.GetRealNick(nfklib.Helper.GetDelphiString(dem.Players[i].netname));
                if (string.IsNullOrEmpty(demo.players[i]))
                    demo.players[i] = "---";
            }

            var logMapSize = string.Format("Map size is {0}x{1}. ", dem.Map.Header.MapSizeX, dem.Map.Header.MapSizeY);
            // if mapsize out of bounds video size then use follow player
            if ((dem.Map.Header.MapSizeX * dem.Map.Header.MapSizeY) > Config.Data.VideoWidth || (dem.Map.Header.MapSizeY * nfklib.NMap.NFKMap.BrickHeight) > Config.Data.VideoHeight)
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
                demo.local_videos[i] = new VideoItem()
                {
                    FileName = videoFile
                };

                // do not create the same video it is was done earlier && size > 1MB
                if (File.Exists(videoFile) && new FileInfo(videoFile).Length > (1024 * 1024))
                    goto continue_for;

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
                    while (!p.HasExited)
                    {
                        Thread.Sleep(5000);

                        if (!p.Responding)
                        {
                            p.Kill();
                            break;
                        }
                    }

                    // process exit code (2 = success)
                    if (p.ExitCode != 2)
                    {
                        Log.Error("ndm2video.exe exiting with error!");
                        Common.freeHangProcesses();
                        // remove file because it's bad
                        if (File.Exists(videoFile))
                        {
                            try
                            {
                                Log.Info("Removing unfinished video " + videoFile);
                                File.Delete(videoFile);
                            }
                            catch (Exception e)
                            {
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

            continue_for:
                // stop if no follow player
                if (demo.local_followplayer == false)
                    break;
            }

            if (!Config.Data.ParallelEncoding)
                demo.local_completed = true;

            Common.SaveJsonDemo(jsonFile, demo);
            Environment.Exit(0);
        }


    }
}
