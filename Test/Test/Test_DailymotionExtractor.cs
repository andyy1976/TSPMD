using System;
using TSPMD;
namespace Test
{
    public class Test_DailymotionExtractor
    {
        public Test_DailymotionExtractor()
        {
            var url = "http://www.dailymotion.com/video/x6vwwhh";

            var extractedUrl = DailymotionExtractor.DownloadUrl(url);

            Console.WriteLine("Url: " + extractedUrl);
        }
    }
}
