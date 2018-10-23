using System;
using TSPMD;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.setMode(true);

            #region YouTube

            /* Search */
            //var YouTubeSearch = new Test_YouTubeSearch();

            /* Extract */
            //var YouTubeExtractor = new Test_YouTubeExtractor();

            #endregion

            #region ccMixter

            /* Search */
            //var ccMixterSearch = new Test_ccMixterSearch();

            /* Extract */
            //var ccMixterExtractor = new Test_ccMixterExtractor();

            #endregion

            #region Dailymotion

            /* Search */
            //var DailymotionSearch = new Test_DailymotionSearch();

            /* Extract */
            //var DailymotionExtractor = new Test_DailymotionExtractor();

            #endregion

            #region Eroprofile

            /* Search */
            //var EroprofileSearch = new Test_EroprofileSearch();

            /* Extract */
            var EroprofileExtractor = new Test_EroprofileExtractor();

            #endregion

            Console.ReadLine();
        }
    }
}
