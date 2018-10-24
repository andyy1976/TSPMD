using System;
using TSPMD;

namespace Test
{
    public class Test_VimeoExtractor
    {
        public Test_VimeoExtractor()
        {
            var url = "https://vimeo.com/11330269";

            var extractedUrl = VimeoExtractor.DownloadUrl(url);

            Console.WriteLine("Url: " + extractedUrl);
        }
    }
}
