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

using Android.App;
using Android.Content;

namespace TSPMD
{
    class Settings
    {
        public static void Save(String Key, String Value)
        {
            var prefs = Application.Context.GetSharedPreferences("TSPMD", FileCreationMode.Private);

            var prefEditor = prefs.Edit();
            prefEditor.PutString(Key, Value);
            prefEditor.Commit();

        }

        public static String Retrieve(String Key)
        {
            var prefs = Application.Context.GetSharedPreferences("TSPMD", FileCreationMode.Private);

            var Pref = prefs.GetString(Key, null);
            return Pref;
        }
    }
}