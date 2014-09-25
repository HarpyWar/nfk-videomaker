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

namespace nfkscheduler
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

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

                // if json file does not exist from a previous run (it's waiting to be processing by nfkuploader)
                if (!File.Exists(jsonFile))
                    break;
                // else appId will be increased again
            }
            demo.local_appid = appFullId;

            // get local path to demofile (filename extracted from the url)
            string demoFile = Path.Combine(Config.Data.TempDir, Path.GetFileName(demo.file));
            // download demo file
            Log.Info(String.Format("Downloading demo from {0} to {1}", demo.file, Config.Data.TempDir));
            if (!APIClient.DownloadDemo(demo.file, demoFile))
            {
                Log.Error(String.Format("Could not download file"));
                APIClient.SetVideo("BAD_DEMO_FILE", demo.local_appid);
                Environment.Exit(0);
            }

            // check the file is nfk demo
            if (!isDemoValid(demoFile))
            {
                Log.Error("Bad nfk demo");
                // send back result with bad demo file
                APIClient.SetVideo("BAD_DEMO_FILE", demo.local_appid);
                Environment.Exit(1);
            }

            demo.local_videos = new VideoItem[demo.players.Length];

            int playerid;
            // create video in first-person of each player
            for (int i = 0; i < demo.players.Length; i++)
            {
                playerid = i + 1;
                // video filename that will be generated after ndm2mp4.exe processing
                var videoFile = Path.Combine(Config.Data.TempDir, string.Format("{0}_{1}.mp4", demo.id, playerid));
                demo.local_videos[i] = new VideoItem() { 
                    FileName = videoFile 
                };

                // do not create the same video it is was done earlier
                if (File.Exists(videoFile))
                    continue;

                Log.Info("Creating video using ndm2mp4.exe ...");

                // run process ndm2mp4.exe with video creation 
                var p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = Config.Data.Ndm2Mp4File,
                        WorkingDirectory = Path.GetDirectoryName(Config.Data.Ndm2Mp4File),
                        Arguments = string.Format("-demo \"{0}\" -duration {1} -playerid {2} -output \"{3}\"", demoFile, demo.duration, playerid, videoFile)
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
                        Log.Error("ndm2mp4.exe exiting with error!");
                        Environment.Exit(1);
                    }
                }
                finally
                {
                    if (p != null)
                        p.Close();
                }
            }


            Log.Info("Demo was processed. Save data into " + jsonFile);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(demo, Newtonsoft.Json.Formatting.Indented);

            // save into json with all needed data that can be uploaded by nfkuploader.exe
            File.WriteAllText(jsonFile, json);

        }



        private bool isDemoValid(string fileName)
        {
            string header = "NFKDEMO";
            try
            {
                // check demo file (first 7 bytes)
                using (FileStream fs = File.OpenRead(fileName))
                {
                    if (fs.Length < header.Length)
                        return false;

                    byte[] buffer = new byte[header.Length];
                    fs.Read(buffer, 0, header.Length);
                    if (Encoding.Default.GetString(buffer) != header)
                        return false;
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
