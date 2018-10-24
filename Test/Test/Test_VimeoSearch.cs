using System;
using TSPMD;

namespace Test
{
    public class Test_VimeoSearch
    {
        public Test_VimeoSearch()
        {
            var items = VimeoSearch.Query("Test", 1);

            int i = 1;

            foreach (var item in items)
            {
                Console.WriteLine("Item: " + i);
                Console.WriteLine("Author: " + item.getAuthor());
                Console.WriteLine("Duration: " + item.getDuration());
                Console.WriteLine("Thumbnail: " + item.getThumbnail());
                Console.WriteLine("Title: " + item.getTitle());
                Console.WriteLine("Url: " + item.getUrl());
                Console.WriteLine(Environment.NewLine);

                i++;
            }
        }
    }
}
