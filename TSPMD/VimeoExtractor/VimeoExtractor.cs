﻿/*****************************************************************************
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
    public static class VimeoExtractor
    {
        static String url;
        static String url_;

        public static string DownloadUrl(string Url)
        {
            url = "";
            url_ = "";

            // Download html content
            string content = Web.getContentFromUrlWithProperty(Url);

            if (Log.getMode())
                Log.println("Content: " + content);

            // Search string
            string pattern = "config_url.*?player_url";
            MatchCollection result = Regex.Matches(content, pattern, RegexOptions.Singleline);

            for (int ctr = 0; ctr <= result.Count - 1; ctr++)
            {
                if (Log.getMode())
                    Log.println("Match: " + result[ctr].Value);

                url = result[ctr].Value;

                url = url.Replace("config_url\":\"", "");
                url = url.Replace("\",\"player_url", "");
                url = url.Replace("\\", "");

                if (Log.getMode())
                    Log.println("ConfigUrl : " + url);

                // Download html content
                string content_ = Web.getContentFromUrlWithProperty(url);

                // Search string
                string pattern_ = "fps.*?level3";
                MatchCollection result_ = Regex.Matches(content_, pattern_, RegexOptions.Singleline);

                for (int ctr_ = 0; ctr_ <= result_.Count - 1; ctr_++)
                {
                    if (Log.getMode())
                        Log.println("Match: " + result_[ctr_].Value);

                    url_ = Helper.ExtractValue(result_[ctr_].Value, "fps\":", "\",\"cdn\":\"level3");

                    url_ = url_ + "\"";

                    url_ = Helper.ExtractValue(url_, "url\":\"", "\"");
                }

                if (Log.getMode())
                    Log.println("Url : " + url_);

                if (Log.getMode())
                    Log.println("********************************");
            }

            return url_;
        }
    }
}
