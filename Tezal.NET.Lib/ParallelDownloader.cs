using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Tezal.NET.Lib
{
    public class ParallelDownloader
    {
        public readonly List<DownloadRange> DownloadRanges;
        public ParallelDownloader(string fileUrl, string destinationFolderPath)
        {
            FileUri = new Uri(fileUrl);

            FileSize = GetFileSize();

            DestinationFilePath = Path.Combine(destinationFolderPath, FileUri.Segments.Last());

            DownloadRanges = GetFileRanges();
        }

        public string DestinationFilePath { get; private set; }
        public long FileSize { get; private set; }

        public Uri FileUri { get; private set; }

        public int NumberOfParallelConnections { get; } = Environment.ProcessorCount;
        public void StartDownloadParallel()
        {
            // Create a dictionary which holds paths to temp files
            var tempFileDict = new ConcurrentDictionary<int, string>();

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = NumberOfParallelConnections,
            };

            Parallel.For(0, NumberOfParallelConnections, (downloadRangeIndex) =>
            {
                var httpRequest = CreateFileDownloadRequest(DownloadRanges[downloadRangeIndex]);

                using (var httpResponse = httpRequest.GetResponse() as HttpWebResponse)
                {
                    var tempFilePath = Path.GetTempFileName();
                    using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        httpResponse.GetResponseStream().CopyTo(tempFileStream);
                        tempFileDict.TryAdd(downloadRangeIndex, tempFilePath);
                    }
                }

                downloadRangeIndex++;
            });

            MergeTempFiles(tempFileDict);
        }

        private HttpWebRequest CreateFileDownloadRequest(DownloadRange range)
        {
            var request = HttpWebRequest.Create(FileUri) as HttpWebRequest;
            request.Method = "GET";
            request.AddRange(range.Start, range.End);

            return request;
        }

        private List<DownloadRange> GetFileRanges()
        {
            var ranges = new List<DownloadRange>();

            // Add all ranges except for the last one
            for (int chunkNumber = 0; chunkNumber < NumberOfParallelConnections - 1; chunkNumber++)
            {
                var range = new DownloadRange()
                {
                    Start = chunkNumber * (FileSize / NumberOfParallelConnections),
                    End = ((chunkNumber + 1) * (FileSize / NumberOfParallelConnections)) - 1
                };

                ranges.Add(range);
            }

            // Add the last range
            ranges.Add(new DownloadRange()
            {
                Start = ranges.Any() ? ranges.Last().End + 1 : 0,
                End = FileSize - 1
            });

            return ranges;
        }

        private long GetFileSize()
        {
            var request = WebRequest.Create(FileUri);
            request.Method = "HEAD";
                
            using (var response = request.GetResponse())
            {
                return long.Parse(response.Headers.Get("Content-Length"));
            }
        }
        private void MergeTempFiles(ConcurrentDictionary<int, string> tempFileDict)
        {
            tempFileDict.OrderBy(x => x.Key);

            using (var fileStream = new FileStream(DestinationFilePath, FileMode.Append))
            {
                foreach (var tempFile in tempFileDict)
                {
                    var tempFileBytes = File.ReadAllBytes(tempFile.Value);
                    fileStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                    File.Delete(tempFile.Value);
                }
            }
        }
    }
}