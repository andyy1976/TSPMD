using System;
using System.Collections.Generic;
using System.Linq;
using TSPMD;

namespace Test
{
    public class Test_YouTubeExtractor
    {
        public Test_YouTubeExtractor()
        {
            var url = "https://www.youtube.com/watch?v=ALUhXkqXuHs";

            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url, false);

            var video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.AudioBitrate == 192); // mp4 video

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            Console.WriteLine("Url: " + video.DownloadUrl);
        }
    }
}
