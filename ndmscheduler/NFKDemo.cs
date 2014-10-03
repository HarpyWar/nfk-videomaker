using Helper;
using SharpCompress.Compressor.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ndmscheduler
{
    class NFKDemo
    {
        public static MapSize BrickSize = new MapSize { Width = 32, Height = 16 };

        public bool Error = false;
        private MapSize map;
        public NFKDemo(string fileName)
        {
            map = new MapSize();
            try
            {
                readDemo(fileName);
            }
            catch { }
        }

        public MapSize GetMapSize()
        {
            return this.map;
        }


        void readDemo(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    // check if header is bad
                    var header = br.ReadBytes(7);
                    if (Encoding.Default.GetString(header) != "NFKDEMO")
                        return;
                    // separator 0x2D
                    br.ReadByte();
                    // gzip compressed data
                    var data = br.ReadBytes((int)fs.Length - 8);

                    var data2 = Decompress(data);
                    map.Width = data2[147];
                    map.Height = data2[148];
                }
            }
        }

        byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (BZip2Stream stream = new BZip2Stream(new MemoryStream(gzip), SharpCompress.Compressor.CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }


        public struct MapSize
        {
            public int Width;
            public int Height;
        }
    }
}
