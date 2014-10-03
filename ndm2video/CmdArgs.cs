using Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ndm2video
{
    class CmdArgs
    {
        public static void ParseOptions(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("(c) 2014 HarpyWar (harpywar@gmail.com)\n\nUsage: \n\nndm2video.exe -demo [filename] -duration [demo duration in seconds] -playerid [follow player id] -output [video file result]\n");

                Environment.Exit(0);
            }

            int value;
            Log.Info("Given cmd args: " + string.Join(" ", args));

            for (int i = 0; i < args.Length; i++ )
            {
                if (i+1 >= args.Length)
                    continue;

                switch(args[i])
                {
                    case "-demo":
                        Config.DemoFile = args[i + 1].Trim();
                        break;
                    case "-duration":
                        int.TryParse(args[i+1], out value);
                        Config.DemoDuration = value;
                        break;

                    case "-playerid":
                        int.TryParse(args[i+1], out value);
                        Config.PlayerNumber = value;
                        break;

                    case "-output":
                        Config.VideoFile = args[i + 1].Trim();
                        break;
                }
            }


            if (Config.DemoDuration <= 0)
            {
                Log.Error("Bad '-duration'.");
                Environment.Exit(1);
            }
            if (Config.PlayerNumber < 0)
            {
                Log.Error("Bad '-playerid.");
                Environment.Exit(1);
            }
            if (string.IsNullOrEmpty(Config.VideoFile))
            {
                Log.Error("Please, set '-output' video file path.");
                Environment.Exit(1);
            }
            if (string.IsNullOrEmpty(Config.VideoFile))
            {
                Log.Error("Please, set '-demo' file path.");
                Environment.Exit(1);
            }

        }

    }
}
