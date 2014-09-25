using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helper
{
    public static class Log
    {
        private static StringBuilder oldtext = new StringBuilder();


        public static void Separator()
        {
            Write("----------------------------------------------\n");
        }

        public static void Error(string text)
        {
            Write(String.Format("[{0}] [ERROR] {1}\n", DateTime.Now, text));
        }

        public static void Info(string text)
        {
            Write(String.Format("[{0}] [INFO] {1}\n", DateTime.Now, text));
        }

        // write to log
        static void Write(string text)
        {
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
