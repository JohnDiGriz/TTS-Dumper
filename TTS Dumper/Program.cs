using System;
using System.IO;

namespace TTS_Dumper
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = File.ReadAllLines("config.txt");
            var res = DownloadService.DownloadMods(config[0], config[1]);
            Console.WriteLine(res);
        }
    }
}
