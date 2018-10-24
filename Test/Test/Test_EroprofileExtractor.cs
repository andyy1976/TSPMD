using System;
using TSPMD;
namespace Test
{
    public class Test_EroprofileExtractor
    {
        public Test_EroprofileExtractor()
        {
            var url = "http://www.eroprofile.com/m/videos/view/Anal-porn-movie-s-starlet-test";

            var extractedUrl = EroprofileExtractor.DownloadUrl(url);

            Console.WriteLine("Url: " + extractedUrl);
        }
    }
}
