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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TSPMD
{
    public static class xHamsterExtractor
    {
        static List<xHamsterExtractorComponents> items;

        static String title;
        static String format;
        static String resolution;
        static String url;

        public static List<xHamsterExtractorComponents> Query(String Url)
        {
            items = new List<xHamsterExtractorComponents>();

            // Search address
            string content = Web.getContentFromUrlWithProperty(Url);

            if (Log.getMode())
                Log.println("Match: " + content);

            title = Helper.ExtractValue(content, "title\" content=\"", "\">");

            // Search string
            string pattern = "download\":.*?,{\"name\"";
            MatchCollection result = Regex.Matches(content, pattern, RegexOptions.Singleline);

            for (int ctr = 0; ctr <= result.Count - 1; ctr++)
            {
                if (Log.getMode())
                    Log.println("Content: " + result[ctr].Value);

                if (Log.getMode())
                    Log.println("Match: " + result[ctr].Value);

                /* 240p */

                if (Log.getMode())
                    Log.println("Title: " + title);

                format = "mp4";

                if (Log.getMode())
                    Log.println("Format: " + format);

                resolution = "240p";

                if (Log.getMode())
                    Log.println("Resolution: " + resolution);

                url = Helper.ExtractValue(result[ctr].Value, "\"240p\":\"", "\"}").Replace("\\", "");

                if (Log.getMode())
                    Log.println("Url: " + url);

                var comp = new xHamsterExtractorComponents(title, format, resolution, url);

                items.Add(comp);

                /* 480p */

                if (Log.getMode())
                    Log.println("Title: " + title);

                format = "mp4";

                if (Log.getMode())
                    Log.println("Format: " + format);

                resolution = "480p";

                if (Log.getMode())
                    Log.println("Resolution: " + resolution);

                url = Helper.ExtractValue(result[ctr].Value, "\"480p\":\"", "\",\"").Replace("\\", "");

                if (Log.getMode())
                    Log.println("Url: " + url);

                comp = new xHamsterExtractorComponents(title, format, resolution, url);

                items.Add(comp);

                /* 720p */

                if (Log.getMode())
                    Log.println("Title: " + title);

                format = "mp4";

                if (Log.getMode())
                    Log.println("Format: " + format);

                resolution = "720p";

                if (Log.getMode())
                    Log.println("Resolution: " + resolution);

                url = Helper.ExtractValue(result[ctr].Value, "\"720p\":\"", "\",\"").Replace("\\", "");

                if (Log.getMode())
                    Log.println("Url: " + url);

                comp = new xHamsterExtractorComponents(title, format, resolution, url);

                items.Add(comp);
            }

            return items;
        }
    }
}
