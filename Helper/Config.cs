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

        [System.Xml.Serialization.XmlElement("LogInfo")]
        public bool LogInfo { get; set; }

        [System.Xml.Serialization.XmlElement("LogError")]
        public bool LogError { get; set; }


#region ndm2video

        [System.Xml.Serialization.XmlElement("GameExeFile")]
        public string GameExeFile { get; set; }

        [System.Xml.Serialization.XmlElement("Autoexec")]
        public string Autoexec { get; set; }

        [System.Xml.Serialization.XmlElement("ExtraTime")]
        public int ExtraTime { get; set; }

        [System.Xml.Serialization.XmlElement("VideoOutputExtension")]
        public string VideoOutputExtension { get; set; }

        [System.Xml.Serialization.XmlElement("ExternalVideoCapture")]
        public bool ExternalVideoCapture { get; set; }

        [System.Xml.Serialization.XmlElement("ParallelEncoding")]
        public bool ParallelEncoding { get; set; }

        [System.Xml.Serialization.XmlArray("ExternalToolRoundTrip")]
        public Round[] ExternalToolRoundTrip { get; set; }


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
        public string ProcessorAffinity
        {
            get
            {
                return _processorAffinity.ToString();
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

        [System.Xml.Serialization.XmlElement("ShowScoreBoard")]
        public bool ShowScoreBoard { get; set; }

        [System.Xml.Serialization.XmlElement("ScoreboardInterval")]
        public float ScoreboardInterval { get; set; }

        [System.Xml.Serialization.XmlElement("ScoreboardDuration")]
        public float ScoreboardDuration { get; set; }



#endregion

#region ndmscheduler

        [System.Xml.Serialization.XmlElement("Ndm2VideoFile")]
        public string Ndm2VideoFile { get; set; }


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

#region ndmuploader

        [System.Xml.Serialization.XmlElement("VideoTitle")]
        public string VideoTitle { get; set; }

        [System.Xml.Serialization.XmlElement("VideoDescription")]
        public string VideoDescription { get; set; }

        [System.Xml.Serialization.XmlElement("VideoKeyWords")]
        public string VideoKeyWords { get; set; }

        [System.Xml.Serialization.XmlElement("VideoPlayList")]
        public string VideoPlayList { get; set; }

        [System.Xml.Serialization.XmlElement("VideoMimeType")]
        public string VideoMimeType { get; set; }




        [System.Xml.Serialization.XmlElement("UploadMaxAttempts")]
        public int UploadMaxAttempts { get; set; }

        [System.Xml.Serialization.XmlElement("YoutubeUploadCompleteExec")]
        public string YoutubeUploadCompleteExec { get; set; }

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


        public class Round
        {
            [System.Xml.Serialization.XmlElement("FileName")]
            public string FileName;
            [System.Xml.Serialization.XmlElement("Args")]
            public string Args;

            [System.Xml.Serialization.XmlElement("ProcessorAffinity")]
            public string ProcessorAffinity
            {
                get
                {
                    return _processorAffinity.ToString();
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
            private int? _processorAffinity;

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

        }


    }
}
