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
    public static class PornhubExtractor
    {
        static List<PornhubExtractorComponents> items;

        public static List<PornhubExtractorComponents> Query(string Url)
        {
            items = new List<PornhubExtractorComponents>();

            string content = Web.getContentFromUrlWithProperty(Url);

            if (Log.getMode())
                Log.println("Content: " + content);

            // Search string
            string pattern = "720\",\"videoUrl\":\".*?\"";
            MatchCollection result = Regex.Matches(content, pattern, RegexOptions.Singleline);

            String TITLE = string.Empty;
            String URL = String.Empty;

            for (int ctr = 0; ctr <= result.Count - 1; ctr++)
            {
                if (Log.getMode())
                    Log.println("Match: " + content);

                TITLE = Helper.ExtractValue(content, "\"video_title\":\"", "\",\"");

                if (Log.getMode())
                    Log.println("Title : " + TITLE);

                URL = result[ctr].Value.Replace("\\", "").Replace("720\",\"videoUrl\":\"", "")
                    .Replace("\"", "");

                if (Log.getMode())
                    Log.println("Url : " + URL);

                if (Log.getMode())
                    Log.println("********************************");

                PornhubExtractorComponents comp = new PornhubExtractorComponents (TITLE, URL);

                items.Add(comp);
            }

            return items;
        }
    }
}
