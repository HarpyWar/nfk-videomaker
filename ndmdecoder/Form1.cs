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

namespace ndmdecoder
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
                    if (Common.IsFileLocked(new FileInfo(files[j])))
                        continue;

                    jsonFile = files[j];

                    // if demo already completed and waiting for upload
                    var d = Common.ReadJsonDemo(File.ReadAllText(jsonFile));
                    if (d.local_completed)
                    {
                        jsonFile = null;
                        continue;
                    }

                    break;
                }
                if (jsonFile == null)
                    Environment.Exit(0);

                Log.Info("Reading demo info file " + jsonFile);

                bool result = false;
                DemoItem demo;
                // lock file while processing
                using (var fs = new FileStream(jsonFile, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        // open first available json file with demo info
                        demo = Common.ReadJsonDemo(sr.ReadToEnd());
                        result = processDemo(demo);
                    }
                }
                if (result)
                {
                    // set complete flag
                    demo.local_completed = true;
                    // save with modified flag back to json
                    Common.SaveJsonDemo(jsonFile, demo);
                }
                // if something wrong then remove json
                else
                {
                    Log.Info("Consistently is wrong. Delete " + jsonFile);
                    File.Delete(jsonFile);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }

            Environment.Exit(0);

        }

        private bool processDemo(DemoItem demo)
        {
            // if external tool not used 
            if (!Config.Data.ExternalVideoCapture)
            {
                return false;
            }

            for (int i = 0; i < demo.local_videos.Length; i++ )
            {
                var fileName = demo.local_videos[i].FileName;
                // it's assumed that video file exists

                var result = new ExternalVideoTool(fileName)
                    .StartNextRound(); // start with second round

                if (!result)
                    return false;

                // stop if no follow player
                if (demo.local_followplayer == false)
                    break;
            }
            return true;
        }
    }
}
