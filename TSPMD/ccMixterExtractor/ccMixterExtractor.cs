/*****************************************************************************
 *
 * This program is free software ; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
 *
 *
 * The Simple Pocket Media Downloader
 * Copyright (c) 2018 Torsten Klinger
 *
 ****************************************************************************/

using System;
using System.Text.RegularExpressions;

namespace TSPMD
{
    public static class ccMixterExtractor
    {
        static string url;

        public static String DownloadUrl(String Url)
        {
            // Address
            var content = Web.getContentFromUrl(Url);

            // Search string
            string pattern = "http://ccmixter.org/content/.*?.mp3";
            MatchCollection result = Regex.Matches(content, pattern, RegexOptions.Singleline);

            for (int ctr = 0; ctr <= result.Count - 1; ctr++)
            {
                if (!result[ctr].Value.Contains("</script>"))
                {
                    if (Log.getMode())
                        Log.println("Match: " + result[ctr].Value);

                    url = result[ctr].Value;

                    if (Log.getMode())
                        Log.println("Url : " + result[ctr].Value);

                    if (Log.getMode())
                        Log.println("********************************");
                }
            }

            return url;
        }
    }
}
