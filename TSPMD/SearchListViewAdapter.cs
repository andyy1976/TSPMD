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
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
                if (!String.IsNullOrEmpty(items_[position].Thumbnail))
                    Picasso.With(this.context_).Load(items_[position].Thumbnail).Into(imageView);
                else
                    Picasso.With(this.context_).Load("file:///android_asset/Thumbnail.png").Into(imageView);
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
                    if (valueUrl.Contains("youtube"))
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
                    }
                    else if (valueUrl.Contains("ccmixter"))
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
                // Request permissions
#pragma warning disable CS4014
                ActivityContext.mActivity.RunOnUiThread(() => requestPermissionsAsync());
#pragma warning restore CS4014

                ActivityContext.mActivity.RunOnUiThread(delegate
                {
                    if (valueUrl.Contains("youtube"))
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
                    else if (valueUrl.Contains("ccmixter"))
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

        async System.Threading.Tasks.Task requestPermissionsAsync()
        {
            await permissionsAsync();
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
                    status = results[Permission.Storage];
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

        /// <summary>
        /// Play file
        /// </summary>
        /// <param name="valueUrl"></param>
        /// <param name="title"></param>
        /// <param name="position"></param>
        private void play(string valueUrl, string title, int position, bool isVideo)
        {
            try
            {
                var tube = string.Empty;

                if (valueUrl.Contains("youtube"))
                    tube = "YouTube";
                else if (valueUrl.Contains("ccmixter"))
                    tube = "ccMixter";
                else if (valueUrl.Contains("dailymotion"))
                    tube = "Dailymotion";
                else if (valueUrl.Contains("eroprofile"))
                    tube = "Eroprofile";
                else if (valueUrl.Contains("pornhub"))
                    tube = "Pornhub";
                else if (valueUrl.Contains("vimeo"))
                    tube = "Vimeo";
                else if (valueUrl.Contains("xhamster"))
                    tube = "xHamster";

                var mediaUrl = string.Empty;

                switch (tube)
                {
                    case "YouTube":
                        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(valueUrl, false);

                        VideoInfo video = null;

                        if (isVideo)
                        {
                            video = videoInfos
                                .First(info => info.VideoType == VideoType.Mp4 && info.AudioBitrate == 96); // mp4 video

                            if (String.IsNullOrEmpty(video.DownloadUrl)) // Fallback
                                video = videoInfos
                                .First(info => info.VideoType == VideoType.Mp4 && info.AudioBitrate == 96 && info.Resolution == 240); // mp4 video
                        }
                        else
                        {
                            video = videoInfos
                                .First(info => info.VideoType == VideoType.Mobile); // 3gp
                        }

                        if (video.RequiresDecryption)
                        {
                            DownloadUrlResolver.DecryptDownloadUrl(video);
                        }

                        mediaUrl = video.DownloadUrl;
                        break;
                    case "ccMixter":
                        mediaUrl = ccMixterExtractor.DownloadUrl(valueUrl);
                        break;
                    case "Dailymotion":
                        mediaUrl = DailymotionExtractor.DownloadUrl(valueUrl);
                        break;
                    case "Eroprofile":
                        mediaUrl = EroprofileExtractor.DownloadUrl(valueUrl);
                        break;
                    case "Pornhub":
                        var phitems = PornhubExtractor.Query(valueUrl);

                        foreach (var item in phitems)
                        {
                            if (!String.IsNullOrEmpty(item.getUrl()))
                                mediaUrl = item.getUrl();
                        }
                        break;
                    case "Vimeo":
                        mediaUrl = VimeoExtractor.DownloadUrl(valueUrl);
                        break;
                    case "xHamster":
                        var xhitmes = xHamsterExtractor.Query(valueUrl);

                        foreach (var item in xhitmes)
                        {
                            if (!String.IsNullOrEmpty(item.getUrl()))
                                mediaUrl = item.getUrl();
                        }
                        break;
                    default:
                        return;
                }

#if DEBUG
                Console.WriteLine("Media url: " + mediaUrl);
#endif

                Log.println("Media url: " + mediaUrl);

                if (String.IsNullOrEmpty(mediaUrl))
                {
#if DEBUG
                    Console.WriteLine("File not found", title);
#endif

                    Log.println("File not found: " + title);

                    searchActivity_.publishnotification("File not found", title, searchActivity_.uniquenotificationID());
                }
                else
                {
                    if (isVideo)
                    {
                        var intent = new Intent(ActivityContext.mActivity, typeof(VideoPlayerActivity));
                        intent.PutExtra("url", mediaUrl);
                        intent.PutExtra("title", title);
                        ActivityContext.mActivity.StartActivity(intent);
                    }
                    else
                        mediaplayer(mediaUrl, position);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                Toast.MakeText(ActivityContext.mActivity, "Parsing error", ToastLength.Long).Show();

                Log.println(ex.ToString());
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

                        Log.println("Player resetted");
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

                        Log.println("Player finished");
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

            Log.println("Player started");
        }
    }
}