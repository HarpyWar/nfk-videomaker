using Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ndm2video
{
    class ExternalVideoTool : IDisposable
    {

        private Process _process;
        private System.Timers.Timer externalToolTimer;

        private int roundNumber;
        private string outFile;

        public ExternalVideoTool()
        {
            this.outFile = Config.VideoFile;
        }

        /// <summary>
        /// Starts external video capture program
        /// </summary>
        public void ExternalToolStart(string infile, int roundNumber)
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
                        .Replace("{outfile}", Config.VideoFile),

                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            Log.Info("Starting " + Config.Data.ExternalToolRoundTrip[roundNumber].FileName + " " + _process.StartInfo.Arguments);
            _process.EnableRaisingEvents = true;
            _process.Exited += externalToolProcess_Exited;
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

            // start auto-exit timer with interval of game duration
            externalToolTimer = new System.Timers.Timer() { Interval = (Config.DemoDuration + Config.Data.ExtraTime) * 1000 };
            externalToolTimer.Elapsed += externalToolTimer_Elapsed;
            externalToolTimer.Start();
        }

        /// <summary>
        /// When demo is over we should stop recording using external tool 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void externalToolTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Info("Duration of " + externalToolTimer.Interval/1000 + " seconds was elapsed");
            Program.movieProcessing = true;
            externalToolTimer.Stop();

            // after killing NFK external capture tool should stop capture itself!
            Program.KillNFK();

            // also kill the external process
            _process.Kill();
        }

        /// <summary>
        /// Handle when external video tool process was exited (out/in)side the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void externalToolProcess_Exited(object sender, EventArgs e)
        {
            // last round
            if (Config.Data.ParallelEncoding)
            if (this.roundNumber == Config.Data.ExternalToolRoundTrip.Length - 1)
            {
                Program.endMovieCreation();
            }

            if (!Program.movieProcessing)
            {
                Log.Error("External video tool process is crashed");
                Program.KillNFK();

                // if external tool was killed from outside just close app with error code
                Environment.Exit(1);
            }
            else
            {
                Log.Info("Result is success. Kill the external video tool process.");

                if (Config.Data.ParallelEncoding)
                    Program.endMovieCreation();
                else
                    startNextRound();
            }
        }


        private void startNextRound()
        {
            // check is movie file created or size < 1MB
            if (!File.Exists(this.outFile) || new FileInfo(this.outFile).Length < (1024 * 1024))
            {
                Log.Info("Movie file does not exist " + this.outFile);
                Environment.Exit(1);
            }

            // rename result file into (path + "input" + ext)
            var infile = Path.Combine(Path.GetDirectoryName(this.outFile), "input" + Path.GetExtension(this.outFile));
            if (File.Exists(infile))
                File.Delete(infile);
            Log.Info(string.Format("Rename {0} -> {1}", this.outFile, infile));
            File.Move(this.outFile, infile);

            this.roundNumber++;
            // now start concat tool (intro file + result file)
            new ExternalVideoTool().ExternalToolStart(infile, this.roundNumber);
        }


        public void Dispose()
        {
            if (externalToolTimer != null)
            {
                externalToolTimer.Stop();
                externalToolTimer.Dispose();

            }
            if (_process != null && !_process.HasExited)
                _process.Kill();
        }
    }
}
