using System;
using TSPMD;
namespace Test
{
    public class Test_ccMixterExtractor
    {
        public Test_ccMixterExtractor()
        {
            var url = "http://ccmixter.org/files/robwalkerpoet/57386";

            var extractedUrl = ccMixterExtractor.DownloadUrl(url);

            Console.WriteLine("Url: " + extractedUrl);
        }
    }
}
