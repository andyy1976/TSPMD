using System;
using TSPMD;
namespace Test
{
    public class Test_EroprofileExtractor
    {
        public Test_EroprofileExtractor()
        {
            var url = "http://www.eroprofile.com/m/videos/suggestNiche/Solo-Dildo-Test-Alles-was-reinpasst";

            var extractedUrl = EroprofileExtractor.DownloadUrl(url);

            Console.WriteLine("Url: " + extractedUrl);
        }
    }
}
