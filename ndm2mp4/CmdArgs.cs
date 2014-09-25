﻿using Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ndm2mp4
{
    class CmdArgs
    {
        public static void ParseOptions(string[] args)
        {
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
            if (Config.PlayerNumber <= 0)
            {
                Log.Error("Bad '-playeri'd.");
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
