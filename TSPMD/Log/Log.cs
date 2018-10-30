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
using System.IO;

namespace TSPMD
{
    public static class Log
    {
        static bool isEnabled;

        public static bool getMode()
        {
            return isEnabled;
        }

        public static bool setMode(bool Mode)
        {
            return isEnabled = Mode;
        }

        public static void println(String Message)
        {
#if DEBUG
            Console.WriteLine("[" + DateTime.Now + "]" + " " + Message);
#endif

            try
            {
                // Directory
                // Personal folder path
                string documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;

                // Data folder path
                string dataPath = Path.Combine(documentsPath, "TSPMD");

                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }

                // Logfile
                dataPath = Path.Combine(dataPath, "log");

                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }

                if (!File.Exists(Path.Combine(dataPath, "log.txt")))
                {
                    using (File.Create(Path.Combine(dataPath, "log.txt")))
                    { };
                }

                File.AppendAllText(Path.Combine(dataPath, "log.txt"), "[" + DateTime.Now + "]" + " " + Message + Environment.NewLine);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("[" + DateTime.Now + "]" + " " + ex.ToString());
#endif
            }

        }
    }
}
