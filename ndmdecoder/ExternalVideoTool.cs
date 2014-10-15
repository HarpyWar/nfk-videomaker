using Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ndmdecoder
{
    class ExternalVideoTool
    {

        private Process _process;

        private int roundNumber = 0;
        private string outFile;
        public ExternalVideoTool(string outFile)
        {
            this.outFile = outFile;
        }


        /// <summary>
        /// Starts external video capture program
        /// </summary>
        private bool externalToolStart(string infile, int roundNumber)
        {
            this.roundNumber = roundNumber;

            // start external tool
            _process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Config.Data.ExternalToolRoundTrip[roundNumber].FileName,
                    WorkingDirectory = Path.GetDirectoryName(Config.Data.ExternalToolRoundTrip[roundNumber].FileName),
                    Arguments = Config.Data.ExternalToolRoundTrip[roundNumber].Args
                        .Replace("{infile}", infile)
                        .Replace("{outfile}", this.outFile),

                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            Log.Info("Starting " + Config.Data.ExternalToolRoundTrip[roundNumber].FileName + " " + _process.StartInfo.Arguments);
            _process.EnableRaisingEvents = true;
            _process.Start();

            // delay before set affinity + priority
            Thread.Sleep(10000);
            if (!_process.HasExited)
            {
                var affinity = int.Parse(Config.Data.ExternalToolRoundTrip[roundNumber].ProcessorAffinity);
                if (affinity > 0)
                    _process.ProcessorAffinity = (IntPtr)affinity;

                _process.PriorityClass = (ProcessPriorityClass)Config.Data.ExternalToolRoundTrip[roundNumber].ProcessorPriority;
            }

            // wait for process exit
            while (!_process.HasExited)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            Log.Info("Round is completed.");

            // check last round
            if (this.roundNumber == Config.Data.ExternalToolRoundTrip.Length - 1)
                return endMovieCreation(); 
            else
                return StartNextRound();
        }


        public bool StartNextRound()
        {
            // check is movie file created or size < 1MB
            if (!File.Exists(this.outFile) || new FileInfo(this.outFile).Length < (1024 * 1024))
            {
                Log.Info("Movie file does not exist " + this.outFile);
                return false;
            }

            // rename result file into (path + "input" + ext)
            var infile = Path.Combine(Path.GetDirectoryName(this.outFile), "input" + Path.GetExtension(this.outFile));
            if (File.Exists(infile))
                File.Delete(infile);
            Log.Info(string.Format("Rename {0} -> {1}", this.outFile, infile));
            File.Move(this.outFile, infile);

            this.roundNumber++;
            // now start concat tool (intro file + result file)
            return new ExternalVideoTool(this.outFile).externalToolStart(infile, this.roundNumber);
        }


        bool endMovieCreation()
        {
            // check is movie file created or size < 1MB
            if (!File.Exists(this.outFile) || new FileInfo(this.outFile).Length < (1024 * 1024))
            {
                Log.Error("Movie file does not exist " + this.outFile);
                return false;
            }

            Log.Info("Well done!");
            return true;
        }


    }
}
