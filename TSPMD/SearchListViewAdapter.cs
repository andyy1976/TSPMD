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
using System.Linq;
using System.Threading;

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Square.Picasso;

namespace TSPMD
{
    /// <summary>
    /// List view adapter
    /// Search fragment
    /// </summary>
    class SearchListViewAdapter : BaseAdapter<ListViewItem>
    {
        private List<ListViewItem> items_;
        private Context context_;
        private SearchFragment searchActivity_;
        private Android.Media.MediaPlayer mp;
        private Handler mpHandler;
        private Action mpAction;

        public SearchListViewAdapter(Context context, List<ListViewItem> items, SearchFragment searchActivity)
        {
            items_ = items;
            context_ = context;
            searchActivity_ = searchActivity;
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
                row = LayoutInflater.From(context_).Inflate(Resource.Layout.SearchRow, null, false);
            }

            // Title
            TextView txtTitle = row.FindViewById<TextView>(Resource.Id.textViewRow);
            txtTitle.Text = items_[position].Title;

            // Url
            string valueUrl = items_[position].Url;

            // Thumbnail
            ImageView imageView = row.FindViewById<ImageView>(Resource.Id.imageView);

            // Author
            TextView txtAuthor = row.FindViewById<TextView>(Resource.Id.textViewRowAuthor);
            txtAuthor.Text = items_[position].Author;

            try
            {
                Picasso.With(this.context_).Load(items_[position].Thumbnail).Into(imageView);
            }
            catch
            {
            }

            // Duration
            TextView txtDuration = row.FindViewById<TextView>(Resource.Id.textViewDuration);
            txtDuration.Text = items_[position].Duration;

            // Play
            Button buttonPlay = row.FindViewById<Button>(Resource.Id.buttonRowPlay);

            var localPlay = new LocalOnclickListener();

            localPlay.HandleOnClick = () =>
            {
                ActivityContext.mActivity.RunOnUiThread(delegate
                {
                    if (valueUrl.Contains("www.youtube"))
                    {
                        ActivityContext.mActivity.RunOnUiThread(delegate
                        {
                            new Android.Support.V7.App.AlertDialog.Builder(ActivityContext.mActivity)
                                           .SetPositiveButton("Audio", (sender, e) =>
                                           {
                                               Thread thread = new Thread(() => play(valueUrl, txtTitle.Text, position, false));
                                               thread.Start();
                                           })
                                           .SetNegativeButton("Video", (sender, e) =>
                                           {
                                               Thread thread = new Thread(() => play(valueUrl, txtTitle.Text, position, true));
                                               thread.Start();
                                           })
                                           .SetTitle(txtTitle.Text)
                                           .SetIcon(Resource.Drawable.Icon)
                                           .Show();
                        });
                    }
                    else if (valueUrl.Contains("www.ccmixter"))
                    {
                        Thread thread = new Thread(() => play(valueUrl, txtTitle.Text, position, false));
                        thread.Start();
                    }
                    else
                    {
                        Thread thread = new Thread(() => play(valueUrl, txtTitle.Text, position, true));
                        thread.Start();
                    }
                });
            };

            buttonPlay.SetOnClickListener(localPlay);

            // Download
            Button buttonDownload = row.FindViewById<Button>(Resource.Id.buttonRowDownload);

            var localDownload = new LocalOnclickListener();

            localDownload.HandleOnClick = () =>
            {
                ActivityContext.mActivity.RunOnUiThread(delegate
                {
                    if (valueUrl.Contains("www.youtube"))
                    {
                        ActivityContext.mActivity.RunOnUiThread(delegate
                        {
                            new Android.Support.V7.App.AlertDialog.Builder(ActivityContext.mActivity)
                                           .SetPositiveButton("Audio", (sender, e) =>
                                           {
                                               Thread thread = new Thread(() => searchActivity_.Download(txtTitle.Text, valueUrl, false));
                                               thread.Start();
                                           })
                                           .SetNegativeButton("Video", (sender, e) =>
                                           {
                                               Thread thread = new Thread(() => searchActivity_.Download(txtTitle.Text, valueUrl, true));
                                               thread.Start();
                                           })
                                           .SetTitle(txtTitle.Text)
                                           .SetIcon(Resource.Drawable.Icon)
                                           .Show();
                        });
                    }
                    else if (valueUrl.Contains("www.ccmixter"))
                    {
                        Thread thread = new Thread(() => searchActivity_.Download(txtTitle.Text, valueUrl, false));
                        thread.Start();
                    }
                    else
                    {
                        Thread thread = new Thread(() => searchActivity_.Download(txtTitle.Text, valueUrl, true));
                        thread.Start();
                    }  
                });
            };

            buttonDownload.SetOnClickListener(localDownload);

            return row;
        }

        /// <summary>
        /// Play file
        /// </summary>
        /// <param name="valueUrl"></param>
        /// <param name="title"></param>
        /// <param name="position"></param>
        private void play(string valueUrl, string title, int position, bool isVideo)
        {
            string threegpurl = "";

            threegpurl = threegp(valueUrl);

            if (String.IsNullOrEmpty(threegpurl))
            {
#if DEBUG
                Console.WriteLine("File not found", title);
#endif
                searchActivity_.publishnotification("File not found", title, searchActivity_.uniquenotificationID());
            }
            else
                mediaplayer(threegpurl, position);
        }

        private string threegp(string url)
        {
            ///////////////////////////////////////////////////////////////////////////////
            /// 
            /// 3gp video cx 
            /// 
            ///////////////////////////////////////////////////////////////////////////////

            // URL
            string threegpurl = "";

            try
            {
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url, false);

                VideoInfo video = videoInfos
                    .First(info => info.VideoType == VideoType.Mobile);

                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                threegpurl = video.DownloadUrl;

#if DEBUG
                Console.WriteLine(threegpurl);
#endif

                if (string.IsNullOrEmpty(threegpurl))
                    return "";
                else
                    return threegpurl;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Audio player
        /// </summary>
        /// <param name="url"></param>
        /// <param name="position"></param>
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
                searchActivity_.mp = mp;

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

                        searchActivity_.seekBar.Progress = 0;
                        searchActivity_.textView.Text = "-";

#if DEBUG
                        Console.WriteLine("Player finished");
#endif
                    }
                };
            }

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

        /// <summary>
        /// Start audio player
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="position"></param>
        private void startPlayer(object s, EventArgs e, int position)
        {
            mp.Start();

            // Duration
            searchActivity_.seekBar.Max = mp.Duration;

            // Title
            searchActivity_.textView.Text = items_[position].Title;

            // Update seekbar and textview in main activity
            mpHandler = new Handler();
            searchActivity_.mpHandler = mpHandler;

            mpAction = () =>
            {
                searchActivity_.mediaplayerIsPlaying();
            };

            searchActivity_.mpAction = mpAction;

            ActivityContext.mActivity.RunOnUiThread(() => searchActivity_.mediaplayerIsPlaying());

#if DEBUG
            Console.WriteLine("Player started");
#endif
        }
    }
}