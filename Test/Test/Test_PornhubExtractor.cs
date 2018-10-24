using System;
using TSPMD;

namespace Test
{
    public class Test_PornhubExtractor
    {
        public Test_PornhubExtractor()
        {
            var url = "https://pornhub.com/view_video.php?viewkey=ph56ff8f0781857";

            var items = PornhubExtractor.Query(url);

            int i = 1;

            foreach (var item in items)
            {
                Console.WriteLine("Item: " + i);
                Console.WriteLine("Title: " + item.getTitle());
                Console.WriteLine("Url: " + item.getUrl());
                Console.WriteLine(Environment.NewLine);

                i++;
            }
        }
    }
}
