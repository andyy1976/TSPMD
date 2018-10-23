using System;
using TSPMD;
namespace Test
{
    public class Test_ccMixterSearch
    {
        public Test_ccMixterSearch()
        {
            var items = ccMixterSearch.Query("Test", 1);

            int i = 1;

            foreach (var item in items)
            {
                Console.WriteLine("Item: " + i);
                Console.WriteLine("Author: " + item.getAuthor());
                Console.WriteLine("Title: " + item.getTitle());
                Console.WriteLine("Url: " + item.getUrl());
                Console.WriteLine(Environment.NewLine);

                i++;
            }
        }
    }
}
