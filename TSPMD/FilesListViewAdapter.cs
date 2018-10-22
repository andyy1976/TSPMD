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
    /// List view adapter
    /// Files fragment
    /// </summary>
    class FilesListViewAdapter : BaseAdapter<ListViewItem>
    {
        private List<ListViewItem> items_;
        private Context context_;
        private FilesFragment filesActivity_;
        private Android.Media.MediaPlayer mp;
        private Handler mpHandler;
        private Action mpAction;

        public FilesListViewAdapter(Context context, List<ListViewItem> items, FilesFragment filesActivity)
        {
            items_ = items;
            context_ = context;
            filesActivity_ = filesActivity;
        }

        public override int Count
        {
            get
            {
                return items_.Count;
            }
        }

        public override ListViewItem this[int position]
        {
            get { return items_[position]; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return base.GetItem(position);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public class LocalOnclickListener : Java.Lang.Object, View.IOnClickListener
        {
            public void OnClick(View v)
            {
                HandleOnClick();
            }

            public System.Action HandleOnClick { get; set; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.From(context_).Inflate(Resource.Layout.FilesRow, null, false);
            }

            // Title
            TextView txtTitle = row.FindViewById<TextView>(Resource.Id.textViewRow_);
            txtTitle.Text = items_[position].Title;

            // Url
            string valueUrl = items_[position].Url;

            // Duration
            TextView txtDuration = row.FindViewById<TextView>(Resource.Id.textViewDuration_);
            txtDuration.Text = items_[position].Duration;

            // Play
            Button buttonPlay = row.FindViewById<Button>(Resource.Id.buttonRowPlay_);

            var localPlay = new LocalOnclickListener();

            localPlay.HandleOnClick = () =>
            {
                Thread thread = new Thread(() => play(valueUrl, position));
                thread.Start();
            };

            buttonPlay.SetOnClickListener(localPlay);

            // Delete
            Button buttonDelete = row.FindViewById<Button>(Resource.Id.buttonRowDelete);

            var localDelete = new LocalOnclickListener();

            localDelete.HandleOnClick = () =>
            {
                File.Delete(valueUrl);
                ActivityContext.mActivity.RunOnUiThread(() => filesActivity_.loadAsync());
            };

            buttonDelete.SetOnClickListener(localDelete);

            return row;
        }

        private void play(string valueUrl, int position)
        {
            mediaplayer(valueUrl, position);
        }

        private void mediaplayer(string url, int position)
        {
#if DEBUG
            Console.WriteLine("Start player");
#endif

#if DEBUG
            Console.WriteLine(url);
#endif

            if (string.IsNullOrEmpty(url))
                return;

            if (mp != null)
            {
                try
                {
                    if (mp.IsPlaying)
                        mp.Stop();

                    mp.Reset();

                    mp.Release();
                }
                catch
                {
                }

                mp = null;
            }

            if (mp == null)
            {
                mp = new Android.Media.MediaPlayer();
                filesActivity_.mp = mp;

                mp.Completion += delegate
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

                        mp.Reset();

                        mp.Release();

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

                        filesActivity_.seekBar.Progress = 0;
                        filesActivity_.textView.Text = "-";

#if DEBUG
                        Console.WriteLine("Player finished");
#endif
                    }
                };
            }

            mp = new Android.Media.MediaPlayer();
            filesActivity_.mp = mp;

            mp.Completion += delegate
            {
                try
                {
                    mp.Reset();
                }
                catch { }

                filesActivity_.seekBar.Progress = 0;
                filesActivity_.textView.Text = "-";

#if DEBUG
                Console.WriteLine("Player finished");
#endif
            };

            mp.SetDataSource(url);
            mp.Prepared += (sender, e) => startPlayer(sender, e, position);

            try
            {
                mp.PrepareAsync();

#if DEBUG
                Console.WriteLine("Player prepared");
#endif
            }
            catch
            {
#if DEBUG
                Console.WriteLine("Prepairing failed");
#endif
            }
        }

        private void startPlayer(object s, EventArgs e, int position)
        {
            mp.Start();

            // Duration
            filesActivity_.seekBar.Max = mp.Duration;

            // Title
            filesActivity_.textView.Text = items_[position].Title;

            // Update seekbar and textview in main activity
            mpHandler = new Handler();
            filesActivity_.mpHandler = mpHandler;

            mpAction = () =>
            {
                filesActivity_.mediaplayerIsPlaying();
            };

            filesActivity_.mpAction = mpAction;

            ActivityContext.mActivity.RunOnUiThread(() => filesActivity_.mediaplayerIsPlaying());

#if DEBUG
            Console.WriteLine("Player started");
#endif
        }
    }
}