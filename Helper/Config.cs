using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Helper
{
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("configuration")]
    public class Config 
    {
        public static Config Data { get; set; }

        public static void Load()
        {
            string exeName = Assembly.GetEntryAssembly().Location;
            string configName = "config.xml";

            try
            {
                Log.Separator();

                var serializer = new XmlSerializer(typeof(Config));

                StreamReader reader = new StreamReader(configName);
                Data = (Config)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception e)
            {
                Log.Error("Could not read configuration file " + configName);
                Log.Error(e.Message);

                Environment.Exit(0);
            }
        }

        public static string LogFile
        {
            get
            {
                if (_logFile != null)
                    return _logFile;

                // program path + exename + .log
                _logFile = Path.Combine(System.Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".log");
                return _logFile;
            }
        }
        private static string _logFile;




#region ndm2mp4

        [System.Xml.Serialization.XmlElement("GameExeFile")]
        public string GameExeFile { get; set; }

        [System.Xml.Serialization.XmlElement("Autoexec")]
        public string Autoexec { get; set; }

        [System.Xml.Serialization.XmlElement("RecordAudio")]
        public bool RecordAudio { get; set; }

        [System.Xml.Serialization.XmlElement("AudioDevice")]
        public int AudioDevice { get; set; }

        [System.Xml.Serialization.XmlElement("VideoWidth")]
        public int VideoWidth { get; set; }

        [System.Xml.Serialization.XmlElement("VideoHeight")]
        public int VideoHeight { get; set; }

        [System.Xml.Serialization.XmlElement("VideoFps")]
        public int VideoFps { get; set; }

        [System.Xml.Serialization.XmlElement("VideoBitrate")]
        public int VideoBitrate { get; set; }

        [System.Xml.Serialization.XmlElement("VideoPaddingTop")]
        public int VideoPaddingTop { get; set; }

        [System.Xml.Serialization.XmlElement("VideoPaddingLeft")]
        public int VideoPaddingLeft { get; set; }

        [System.Xml.Serialization.XmlElement("ProcessorAffinity")]
        public int ProcessorAffinity
        {
            get
            {
                return _processorAffinity;
            }
            set
            {
                int value2;
                if (int.TryParse(value.ToString(), System.Globalization.NumberStyles.HexNumber,
                                 null, out value2))
                    if (value2 > 0)
                        _processorAffinity = value2;
                    else
                        _processorAffinity = 1; // default first processor
            }
        }
        private int _processorAffinity;

        [System.Xml.Serialization.XmlElement("ProcessorPriority")]
        public int ProcessorPriority
        {
            get
            {
                return (int)_processorPriority;
            }
            set
            {
                switch ((int)value)
                {
                    case 0:
                        _processorPriority = ProcessPriorityClass.Idle;
                        break;
                    case 1:
                        _processorPriority = ProcessPriorityClass.BelowNormal;
                        break;
                    case 2:
                        _processorPriority = ProcessPriorityClass.Normal;
                        break;
                    case 3:
                        _processorPriority = ProcessPriorityClass.AboveNormal;
                        break;
                    case 4:
                        _processorPriority = ProcessPriorityClass.High;
                        break;
                    case 5:
                        _processorPriority = ProcessPriorityClass.RealTime;
                        break;
                }
            }
        }
        private ProcessPriorityClass _processorPriority;

        [System.Xml.Serialization.XmlElement("GameProcessTimeout")]
        public int GameProcessTimeout { get; set; }

        [System.Xml.Serialization.XmlElement("AlwaysOnTop")]
        public bool AlwaysOnTop { get; set; }

#endregion

#region nfkscheduler

        [System.Xml.Serialization.XmlElement("Ndm2Mp4File")]
        public string Ndm2Mp4File { get; set; }


        [System.Xml.Serialization.XmlElement("TempDir")]
        public string TempDir
        {
            get
            {
                if (!Directory.Exists(_tempDir))
                    Directory.CreateDirectory(_tempDir);
                return _tempDir;
            }
            set
            {
                _tempDir = value;
            }
        }
        string _tempDir;


        [System.Xml.Serialization.XmlElement("AppId")]
        public string AppId { get; set; }

        [System.Xml.Serialization.XmlElement("ApiKey")]
        public string ApiKey { get; set; }

        [System.Xml.Serialization.XmlElement("GetDemoUrl")]
        public string GetDemoUrl { get; set; }

        [System.Xml.Serialization.XmlElement("SetVideoUrl")]
        public string SetVideoUrl { get; set; }


#endregion

#region nfkuploader

        [System.Xml.Serialization.XmlElement("VideoTitle")]
        public string VideoTitle { get; set; }

        [System.Xml.Serialization.XmlElement("VideoDescription")]
        public string VideoDescription { get; set; }

        [System.Xml.Serialization.XmlElement("VideoKeyWords")]
        public string VideoKeyWords { get; set; }

        [System.Xml.Serialization.XmlElement("YoutubeUserName")]
        public string YoutubeUserName { get; set; }

        [System.Xml.Serialization.XmlElement("YoutubePassword")]
        public string YoutubePassword { get; set; }

        [System.Xml.Serialization.XmlElement("YoutubeAppName")]
        public string YoutubeAppName { get; set; }

        [System.Xml.Serialization.XmlElement("YoutubeDeveloperKey")]
        public string YoutubeDeveloperKey { get; set; }

        [System.Xml.Serialization.XmlElement("MegaUpload")]
        public bool MegaUpload { get; set; }

        [System.Xml.Serialization.XmlElement("MegaUserName")]
        public string MegaUserName { get; set; }

        [System.Xml.Serialization.XmlElement("MegaPassword")]
        public string MegaPassword { get; set; }

#endregion

    }
}
