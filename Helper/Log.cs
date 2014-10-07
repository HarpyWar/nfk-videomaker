using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helper
{
    public static class Log
    {
        private static StringBuilder oldtext = new StringBuilder();
        private static bool firstLog = true;

        public static void Separator()
        {
            Write("----------------------------------------------\n");
        }

        public static void Error(string text)
        {
            if (Config.Data == null || Config.Data.LogError)
                Write(String.Format("[{0}] [ERROR] {1}\n", DateTime.Now, text));
        }

        public static void Info(string text)
        {
            if (Config.Data == null || Config.Data.LogInfo)
                Write(String.Format("[{0}] [INFO] {1}\n", DateTime.Now, text));
        }

        // write to log
        static void Write(string text)
        {
            if (firstLog)
            {
                firstLog = false;
                Separator();
            }

            try
            {
                using (StreamWriter w = File.AppendText(Config.LogFile))
                    w.Write(text);

                Console.Write(text);
            }
            catch (Exception)
            {
                Console.WriteLine("Can't write to log " + Config.LogFile);
            }
        }
    }
}
