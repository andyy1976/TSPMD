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
using System.Threading;

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

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

            // Media type
            TextView txtMediaType = row.FindViewById<TextView>(Resource.Id.textViewMediaType);

            if (valueUrl.EndsWith(".mp3") || valueUrl.EndsWith(".m4a"))
                txtMediaType.Text = "Audio";
            else
                txtMediaType.Text = "Video";

            // Duration
            TextView txtDuration = row.FindViewById<TextView>(Resource.Id.textViewDuration_);

            if (items_[position].Duration.Length > 8)
                txtDuration.Text = items_[position].Duration.Substring(0, 8);
            else
                txtDuration.Text = items_[position].Duration;

            // Play
            Button buttonPlay = row.FindViewById<Button>(Resource.Id.buttonRowPlay_);

            var localPlay = new LocalOnclickListener();

            localPlay.HandleOnClick = () =>
            {
#if DEBUG
                Console.WriteLine(valueUrl);
#endif

                Log.println(valueUrl);

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
            if (valueUrl.EndsWith("mp4") || valueUrl.EndsWith("m4u")) // Video
            {
                var intent = new Intent(ActivityContext.mActivity, typeof(VideoPlayerActivity));
                intent.PutExtra("url", valueUrl);
                intent.PutExtra("title", "Video");
                ActivityContext.mActivity.StartActivity(intent);
            }
            else
                mediaplayer(valueUrl, position);
        }

        private void mediaplayer(string url, int position)
        {
#if DEBUG
            Console.WriteLine("Start player");
#endif

            Log.println("Start player");

#if DEBUG
            Console.WriteLine(url);
#endif

            Log.println(url);

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
                        Log.println("Player resetted");

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

                        Log.println("Player finished");
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

                Log.println("Player finished");
            };

            mp.SetDataSource(url);
            mp.Prepared += (sender, e) => startPlayer(sender, e, position);

            try
            {
                mp.PrepareAsync();

#if DEBUG
                Console.WriteLine("Player prepared");
#endif

                Log.println("Player prepared");
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Prepairing failed");
#endif

                Log.println(ex.ToString());
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

            Log.println("Player started");
        }
    }
}