using System;
using TSPMD;

namespace Test
{
    public class Test_xHamsterExtractor
    {
        public Test_xHamsterExtractor()
        {
            var url = "https://de.xhamster.com/videos/red-teamer-markus-hired-for-state-sponsored-anal-pen-test-9016713";

            var items = xHamsterExtractor.Query(url);

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
