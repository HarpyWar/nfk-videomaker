using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Helper
{
    public class Common
    {

        // RU -> EN
        public static string Translit(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }


        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }


        public static DemoItem ReadJsonDemo(string text)
        {
            try
            {
                var jsonItem = Newtonsoft.Json.JsonConvert.DeserializeObject<DemoItem>(text);

                return jsonItem;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return null;
        }

        public static void SaveJsonDemo(string fileName, DemoItem demo)
        {
            Log.Info("Demo was processed. Save data into " + fileName);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(demo, Newtonsoft.Json.Formatting.Indented);

            // save into json with all needed data that can be uploaded by ndmuploader.exe
            File.WriteAllText(fileName, json);
        }


        public static void freeHangProcesses(bool nfkonly = false)
        {
            if (!nfkonly && Config.Data.ExternalVideoCapture)
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
