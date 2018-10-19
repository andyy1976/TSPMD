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
using System.Text;

namespace TSPMD
{
    public static class Helper
    {
        public static String ExtractValue(String Source, String Start, String End)
        {
            int start, end;

            try
            {
                if (Source.Contains(Start) && Source.Contains(End))
                {
                    start = Source.IndexOf(Start, 0) + Start.Length;
                    end = Source.IndexOf(End, start);

                    return Source.Substring(start, end - start);
                }
                else
                    return printZero();
            }
            catch (Exception ex)
            {
                if (Log.getMode())
                    Log.println(ex.ToString());

                return printZero();
            }
        }

        public static String Concat(String a, String b)
        {
            return new StringBuilder().Append(a).Append(b).ToString();
        }

        public static String Concat(String a, String b, String c)
        {
            return new StringBuilder().Append(a).Append(b).Append(c).ToString();
        }

        public static String printZero()
        {
            return " ";
        }

        public static String formatDuration(String Duration)
        {
            int timeInSeconds = Int32.Parse(Duration);

            int hours = timeInSeconds / 3600;
            int secondsLeft = timeInSeconds - hours * 3600;
            int minutes = secondsLeft / 60;
            int seconds = secondsLeft - minutes * 60;

            String formattedTime = "";

            if (hours < 10)
                formattedTime += "0";
            formattedTime += hours + ":";

            if (formattedTime.Equals("00:"))
                formattedTime = "";

            if (minutes < 10)
                formattedTime += "0";
            formattedTime += minutes + ":";

            if (seconds < 10)
                formattedTime += "0";
            formattedTime += seconds;

            return formattedTime;
        }
    }
}
