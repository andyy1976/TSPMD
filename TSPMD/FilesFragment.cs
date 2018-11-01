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
using System.IO;
using System.Linq;
using System.Threading;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace TSPMD
{
    /// <summary>
    /// Files fragment
    /// </summary>
    public class FilesFragment : Fragment, SeekBar.IOnSeekBarChangeListener
    {
        private List<ListViewItem> items = null;
        public ListView listView;
        FilesListViewAdapter adapter;
        public TextView textView;
        public SeekBar seekBar;

        public Android.Media.MediaPlayer mp;
        public Handler mpHandler;
        public Action mpAction;

        View view;

        ViewGroup container;
        LayoutInflater inflater;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            /* UI */

            var view = inflater.Inflate(Resource.Layout.Files, container, false);

            this.inflater = inflater;
            this.container = container;

            this.view = view;

            // Get layout resources and attach an event to it
            listView = view.FindViewById<ListView>(Resource.Id.listViewResult_);
            listView.ChoiceMode = ChoiceMode.Single;
            listView.ItemLongClick += new EventHandler<AdapterView.ItemLongClickEventArgs>(ItemLong_OnClick);

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

                    Log.println("Player resetted");
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

                    Log.println("Player stopped");
                }
            };

            if (items == null)
                items = new List<ListViewItem>();

            if (adapter == null)
                adapter = new FilesListViewAdapter(ActivityContext.mActivity, items, this);

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

        private void ItemLong_OnClick(object sender_, AdapterView.ItemLongClickEventArgs e_)
        {
            var item = items[e_.Position];

            var itemFileName = item.Title.Split('|');

            var view = inflater.Inflate(Resource.Layout.FilesDialog, container, false);

            var edittext = view.FindViewById<EditText>(Resource.Id.filesDialogedittext);
            edittext.Text = itemFileName[0];

            ActivityContext.mActivity.RunOnUiThread(delegate
            {
                new Android.Support.V7.App.AlertDialog.Builder(ActivityContext.mActivity)
                                       .SetPositiveButton("Save", (sender, e) =>
                                       {
                                           try
                                           {
                                               System.IO.File.Move(item.Url, item.Url.Replace(itemFileName[0], edittext.Text));

                                               ActivityContext.mActivity.RunOnUiThread(() => loadAsync());
                                           }
                                           catch (Exception ex)
                                           {
                                               Log.println(ex.ToString());

                                               ActivityContext.mActivity.RunOnUiThread(() =>
                                               Toast.MakeText(ActivityContext.mActivity, "Error! Can't rename file.", ToastLength.Long).Show());
                                           }
                                       })
                                       .SetNegativeButton("Cancel", (sender, e) =>
                                       {
                                           // Do nothing
                                       })
                                       .SetTitle("Rename")
                                       .SetIcon(Resource.Drawable.Icon)
                                       .SetView(view)
                                       .Show();
            });
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
                var directory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

                string[] filters = new[] { "*.mp3", "*.m4a", "*.mp4", "*.m4u" };
                string[] files = filters.SelectMany(f => Directory.GetFiles(directory.AbsolutePath, f)).ToArray();

                Array.Sort(files, StringComparer.InvariantCulture);

                foreach (var item in files)
                {
                    var file = TagLib.File.Create(item);

                    var duration = file.Properties.Duration.ToString();

                    // Add values to ListView
                    items.Add(new ListViewItem()
                    {
                        Title = item.Replace(directory.AbsolutePath + "/", "").Replace(".mp3", "").Replace(".m4a", "").Replace(".mp4", "").Replace(".m4u", ""),
                        Url = item,
                        Duration = duration
                    });

#if DEBUG
                    Console.WriteLine(item.Replace(directory.AbsolutePath + "/", "").Replace(".mp3", "").Replace(".m4a", "").Replace(".mp4", "").Replace(".m4u", "") + " | " + item);
#endif
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Parsing error");
#endif

                Log.println(ex.ToString());

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

                    Log.println("Permission granted");
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