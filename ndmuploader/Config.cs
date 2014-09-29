using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Helper;

namespace ndmuploader
{
    public class Config : Helper.Config
    {
        public static DemoItem ReadJsonDemo(string fileName)
        {
            try
            {
                var data = File.ReadAllText(fileName);
                var jsonItem = Newtonsoft.Json.JsonConvert.DeserializeObject<DemoItem>(data);

                return jsonItem;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return null;
        }

    }
}
