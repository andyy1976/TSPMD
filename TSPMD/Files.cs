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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Webkit;
using Android.Widget;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

using Square.Picasso;

namespace TSPMD
{
    /// <summary>
    /// Files fragment
    /// </summary>
    public class Files : Fragment, SeekBar.IOnSeekBarChangeListener
    {
        private List<Information> items = null;
        private ListView listView;
        ListViewAdapter_ adapter;
        public TextView textView;
        public SeekBar seekBar;

        public Android.Media.MediaPlayer mp;
        public Handler mpHandler;
        public Action mpAction;

        View view;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ///////////////////////////////////////////////////////////////////////////////
            /// 
            /// UI
            /// 
            ///////////////////////////////////////////////////////////////////////////////

            var view = inflater.Inflate(Resource.Layout.Files, container, false);

            this.view = view;

            // Get layout resources and attach an event to it
            listView = view.FindViewById<ListView>(Resource.Id.listViewResult_);
            textView = view.FindViewById<TextView>(Resource.Id.textViewPlaying_);
            seekBar = view.FindViewById<SeekBar>(Resource.Id.seekBarPlayer_);
            Button buttonStop = view.FindViewById<Button>(Resource.Id.buttonStop_);

            // Prevent sleeping
            ActivityContext.mActivity.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            buttonStop.Click += delegate
            {
                if (mp != null)
                {
                    try
                    {
                        if (mp.IsPlaying)
                            mp.Stop();
                    }
                    catch
                    {
                    }

                    try
                    {
                        mp.Reset();

                        mp.Release();
                    }
                    catch
                    {
                    }

                    mp = null;

#if DEBUG
                    Console.WriteLine("Player resetted");
#endif
                }

                if (mpHandler != null)
                {
                    if (mpAction != null)
                    {
                        mpHandler.RemoveCallbacks(mpAction);
                    }

                    seekBar.Progress = 0;
                    textView.Text = "-";

#if DEBUG
                    Console.WriteLine("Player stopped");
#endif
                }
            };

            if (items == null)
                items = new List<Information>();

            if (adapter == null)
                adapter = new ListViewAdapter_(ActivityContext.mActivity, items, this);

            listView.Adapter = adapter;

            textView.Text = "-";

            // Clear listView
            items.Clear();

            try
            {
                adapter.NotifyDataSetChanged();
            }
            catch
            { }

            try
            {
                // Load
                Thread thread = new Thread(() => loadAsync());
                ActivityContext.mActivity.RunOnUiThread(() => thread.Start());
            }
            catch { }

            // Initialize seekbar
            seekBar.SetOnSeekBarChangeListener(this);

            return view;
        }

        public async void loadAsync()
        {
            try
            {
                // Clear listView
                items.Clear();
                ActivityContext.mActivity.RunOnUiThread(() => adapter.NotifyDataSetChanged());
            }
            catch { }

            await permissionsAsync();

            try
            {
                // Get files
                var directory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic);

                string[] filters = new[] { "*.mp3", "*.m4a", "*.mp4" };
                string[] files = filters.SelectMany(f => Directory.GetFiles(directory.AbsolutePath, f)).ToArray();

                Array.Sort(files, StringComparer.InvariantCulture);

                foreach (var item in files)
                {
                    var file = TagLib.File.Create(item);

                    var duration = file.Properties.Duration.ToString().Replace("00:00", "00:")
                                       .Replace("::", ":")
                                       .Replace("00:1", "0:1")
                                       .Replace("00:2", "0:2")
                                       .Replace("00:3", "0:3")
                                       .Replace("00:4", "0:4")
                                       .Replace("00:5", "0:5")
                                       .Replace("00:6", "0:6")
                                       .Replace("00:7", "0:7")
                                       .Replace("00:8", "0:8")
                                       .Replace("00:9", "0:9")
                                       .Replace("00:", "")
                                       .Replace("01:", "1:")
                                       .Replace("02:", "2:")
                                       .Replace("03:", "3:")
                                       .Replace("04:", "4:")
                                       .Replace("05:", "5:")
                                       .Replace("06:", "6:")
                                       .Replace("07:", "7:")
                                       .Replace("08:", "8:")
                                       .Replace("09:", "9:")
                                       .Substring(0, 5)
                                       .Replace(".", "");

                    int count = duration.Count(c => c == ':');

                    if (count > 1)
                    {
                        duration = file.Properties.Duration.ToString().Replace("00:00", "00:")
                                       .Replace("00:", "")
                                       .Replace("01:", "1:")
                                       .Replace("02:", "2:")
                                       .Replace("03:", "3:")
                                       .Replace("04:", "4:")
                                       .Replace("05:", "5:")
                                       .Replace("06:", "6:")
                                       .Replace("07:", "7:")
                                       .Replace("08:", "8:")
                                       .Replace("09:", "9:")
                                       .Substring(0, 5)
                                       .Replace(".", "");
                    }

                    // Add values to ListView
                    items.Add(new Information()
                    {
                        Title = item.Replace(directory.AbsolutePath + "/", "").Replace(".mp3", "").Replace(".m4a", "").Replace(".mp4", ""),
                        Url = item,
                        Duration = duration
                    });

#if DEBUG
                    Console.WriteLine(item.Replace(directory.AbsolutePath + "/", "").Replace(".mp3", "").Replace(".m4a", "").Replace(".mp4", "") + " | " + item);
#endif
                }
            }
            catch
            {
#if DEBUG
                Console.WriteLine("Parsing error");
#endif
                try
                {
                    ActivityContext.mActivity.RunOnUiThread(() => adapter.NotifyDataSetChanged());
                }
                catch
                { }

                return;
            }

            try
            {
                ActivityContext.mActivity.RunOnUiThread(() => adapter.NotifyDataSetChanged());
            }
            catch
            { }
        }

        // Permissions
        private async System.Threading.Tasks.Task permissionsAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    {
#if DEBUG
                        Console.WriteLine("Request permissions");
#endif
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                    status = results[Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                {
#if DEBUG
                    Console.WriteLine("Permission granted");
#endif
                }
                else if (status != PermissionStatus.Unknown)
                {
#if DEBUG
                    Console.WriteLine("Permission denied");
#endif
                }
            }
            catch { }
        }

        // Seekbar
        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            if (fromUser)
            {
                try
                {
                    mp.SeekTo(progress);
                }
                catch
                {
                }
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {

        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {

        }

        // Media player
        public void mediaplayerIsPlaying()
        {
            try
            {
                if (mp != null)
                {
                    int currentPosition = mp.CurrentPosition;
                    seekBar.Progress = currentPosition;
                }

                mpHandler.PostDelayed(mpAction, 100);
            }
            catch
            {
            }
        }
    }
}