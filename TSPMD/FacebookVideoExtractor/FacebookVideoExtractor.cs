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

namespace TSPMD
{
    public static class FacebookVideoExtractor
    {
        public static String Title(string Url)
        {
            string content = Web.getContentFromUrlWithProperty(Url);

            if (Log.getMode())
                Log.println("Content : " + content);

            String title = Helper.ExtractValue(content, "\"pageTitle\">", "</title><link");

            if (Log.getMode())
                Log.println("Title : " + title);

            if (Log.getMode())
                Log.println("********************************");

            return title;
        }

        public static String DownloadUrl(string Url)
        {
            string content = Web.getContentFromUrlWithProperty(Url);

            if (Log.getMode())
                Log.println("Content : " + content);

            String URL = Helper.ExtractValue(content.Replace("\r\n", ""), "sd_src_no_ratelimit:\"", "\",hd_src_no_ratelimit:");

            if (Log.getMode())
                Log.println("Url : " + URL);

            if (Log.getMode())
                Log.println("********************************");

            return URL;
        }
    }
}
