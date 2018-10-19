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
using System.Net;
using System.Text;

namespace TSPMD
{
    class Web
    {
        static WebClient webclient;

        public static String getContentFromUrl (String Url)
        {
            try
            {
                webclient = new WebClient();

                webclient.Encoding = Encoding.GetEncoding("ISO-8859-1");

                string content = webclient.DownloadString(Url);

                return content.Replace('\r', ' ').Replace('\n', ' ');
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String getContentFromUrlWithProperty(String Url)
        {
            try
            {
                webclient = new WebClient();

                webclient.Encoding = Encoding.GetEncoding("ISO-8859-1");

                webclient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";

                string content = webclient.DownloadString(Url);

                return content.Replace('\r', ' ').Replace('\n', ' ');
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
