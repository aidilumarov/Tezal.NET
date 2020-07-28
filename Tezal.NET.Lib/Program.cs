using System;
using System.IO;

namespace Tezal.NET.Lib
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloader = new ParallelDownloader("https://www.lokeshdhakar.com/projects/lightbox2/images/image-5.jpg", 
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            downloader.StartDownloadParallel();
        }
    }
}
