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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Steelkiwi.Com.Library;

namespace TSPMD
{
    /// <summary>
    /// Search fragment
    /// </summary>
    public class SearchFragment : Fragment, SeekBar.IOnSeekBarChangeListener
    {
        private List<ListViewItem> items = null;
        private ListView listView;
        SearchListViewAdapter adapter;
        public TextView textView;
        public SeekBar seekBar;
        public Spinner spinner;
        string spinnerSelected;

        public Android.Media.MediaPlayer mp;
        public Handler mpHandler;
        public Action mpAction;

        int notificationID = 0;

        View view;

        Button buttonSearch;

        EditText editText;

        DotsLoaderView dotsLoaderView;

        static readonly string CHANNEL_ID = "location_notification";

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            /* UI */
            var view = inflater.Inflate(Resource.Layout.Search, container, false);

            this.view = view;

            // Get layout resources and attach an event to it

            buttonSearch = view.FindViewById<Button>(Resource.Id.buttonSearch);
            editText = view.FindViewById<EditText>(Resource.Id.editTextSearch);

            listView = view.FindViewById<ListView>(Resource.Id.listViewResult);
            textView = view.FindViewById<TextView>(Resource.Id.textViewPlaying);
            seekBar = view.FindViewById<SeekBar>(Resource.Id.seekBarPlayer);
            Button buttonStop = view.FindViewById<Button>(Resource.Id.buttonStop);

            spinner = view.FindViewById<Spinner>(Resource.Id.spinner);

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var spinneradapter = ArrayAdapter.CreateFromResource(
                    ActivityContext.mActivity, Resource.Array.tubes_array, Android.Resource.Layout.SimpleSpinnerItem);

            spinneradapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = spinneradapter;

            dotsLoaderView = view.FindViewById<DotsLoaderView>(Resource.Id.dotsLoaderView);

            // Prevent sleeping
            ActivityContext.mActivity.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            if (items == null)
                items = new List<ListViewItem>();

            if (adapter == null)
                adapter = new SearchListViewAdapter(ActivityContext.mActivity, items, this);

            listView.Adapter = adapter;

            buttonSearch.Click += delegate
            {
                // Clear listView
                items.Clear();

                try
                {
                    adapter.NotifyDataSetChanged();
                }
                catch
                { }

                if (!String.IsNullOrEmpty(spinnerSelected) && !spinnerSelected.Equals("Tube"))
                {
                    // Search
                    Thread thread = new Thread(() => search(editText.Text, spinnerSelected));
                    ActivityContext.mActivity.RunOnUiThread(() => thread.Start());
                }
                else
                    Toast.MakeText(ActivityContext.mActivity, "Please select a tube", ToastLength.Long).Show();

                // Hide keyboard
                View view_ = ActivityContext.mActivity.CurrentFocus;

                InputMethodManager manager = (InputMethodManager)ActivityContext.mActivity.GetSystemService(Context.InputMethodService);
                manager.HideSoftInputFromWindow(view_.WindowToken, 0);
            };

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

            textView.Text = "-";

            // Initialize seekbar
            seekBar.SetOnSeekBarChangeListener(this);

            // Request permissions
#pragma warning disable CS4014
            ActivityContext.mActivity.RunOnUiThread(() => requestPermissionsAsync());
#pragma warning restore CS4014

            // Set focus to textview
            editText.RequestFocus();

#if DEBUG
            Console.WriteLine("App is loaded");
#endif

            Log.println("App is loaded");

            CreateNotificationChannel();

            return view;
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            spinnerSelected = spinner.GetItemAtPosition(e.Position).ToString();
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

        /// <summary>
        /// Notification channel
        /// </summary>
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var name = "Local Notifications";
            var description = "Local Notifications";
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
            {
                Description = description
            };

            channel.SetSound(null, null);

            var notificationManager = (NotificationManager)ActivityContext.mActivity.GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        /// <summary>
        /// Publish notification
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="notificationID"></param>
        public void publishnotification(string title, string text, int notificationID)
        {
            // Generate notification
            Intent notificationIntent = new Intent(ActivityContext.mActivity, typeof(MainActivity));

            TaskStackBuilder stackBuilder = TaskStackBuilder.Create(ActivityContext.mActivity);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(notificationIntent);

            PendingIntent contentIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            Notification.Builder builder = new Notification.Builder(ActivityContext.mActivity, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetAutoCancel(true)
                .SetContentIntent(contentIntent);

            NotificationManager notificationManager = (NotificationManager)ActivityContext.mActivity.GetSystemService(Context.NotificationService);
            notificationManager.Notify(notificationID, builder.Build());

#if DEBUG
            Console.WriteLine("Notification published: " + notificationID);
#endif

            Log.println("Notification published: " + notificationID);
        }

        /// <summary>
        /// Notification builder
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="notificationID"></param>
        /// <returns></returns>
        private Notification.Builder publishnotification_(string title, string text, int notificationID)
        {
            // Generate notification
            Intent notificationIntent = new Intent(ActivityContext.mActivity, typeof(MainActivity));

            TaskStackBuilder stackBuilder = TaskStackBuilder.Create(ActivityContext.mActivity);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(notificationIntent);

            PendingIntent contentIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            Notification.Builder builder = new Notification.Builder(ActivityContext.mActivity, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetAutoCancel(true)
                .SetContentIntent(contentIntent);

            NotificationManager notificationManager = (NotificationManager)ActivityContext.mActivity.GetSystemService(Context.NotificationService);
            notificationManager.Notify(notificationID, builder.Build());

#if DEBUG
            Console.WriteLine("Notification published: " + notificationID);
#endif

            Log.println("Notification published: " + notificationID);

            return builder;
        }

        public int uniquenotificationID()
        {
            notificationID = notificationID + 1;

#if DEBUG
            Console.WriteLine("NotificationID: " + notificationID);
#endif

            Log.println("Notification published: " + notificationID);

            return notificationID;
        }

        /// <summary>
        /// Search
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tube"></param>
        private void search(string value, string tube)
        {
            if (SEARCH.DO(value, tube, items, dotsLoaderView))
            {
                ActivityContext.mActivity.RunOnUiThread(() => adapter.NotifyDataSetChanged());
            }
            else
            {
                ActivityContext.mActivity.RunOnUiThread(() =>
                Toast.MakeText(ActivityContext.mActivity, "No media found", ToastLength.Long).Show());

                Log.println("No media found");
            }
        }

        #region Download

        /// <summary>
        /// Download
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        public void Download(string title, string url, bool isVideo)
        {
            // TITLE
            string mediaTitle = title;

            // URL
            string mediaUrl = null;

            // File extension
            string file_extension = string.Empty;

            try
            {
                switch (spinnerSelected)
                {
                    case "YouTube":
                        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url, false);

                        VideoInfo video = null;

                        if (isVideo)
                        {
                            video = videoInfos
                                .First(info => info.VideoType == VideoType.Mp4 && info.AudioBitrate == 192); // mp4 video

                            file_extension = ".mp4";
                        }
                        else
                        {
                            video = videoInfos
                                .First(info => info.VideoType == VideoType.Mp4 && info.AudioBitrate == 128); // m4a audio
                        }

                        if (video.RequiresDecryption)
                        {
                            DownloadUrlResolver.DecryptDownloadUrl(video);
                        }

                        mediaUrl = video.DownloadUrl;
                        break;
                    case "ccMixter":
                        mediaUrl = ccMixterExtractor.DownloadUrl(url);
                        break;
                    case "Dailymotion":
                        mediaUrl = DailymotionExtractor.DownloadUrl(url);
                        file_extension = ".mp4";
                        break;
                    case "Eroprofile":
                        mediaUrl = EroprofileExtractor.DownloadUrl(url);
                        file_extension = ".m4u";
                        break;
                    case "Pornhub":
                        var phitems = PornhubExtractor.Query(url);

                        foreach (var item in phitems)
                        {
                            if (!String.IsNullOrEmpty(item.getUrl()))
                                mediaUrl = item.getUrl();
                        }

                        file_extension = ".mp4";
                        break;
                    case "Vimeo":
                        mediaUrl = VimeoExtractor.DownloadUrl(url);

                        file_extension = ".mp4";
                        break;
                    case "xHamster":
                        var xhitmes = xHamsterExtractor.Query(url);

                        foreach (var item in xhitmes)
                        {
                            if (!String.IsNullOrEmpty(item.getUrl()))
                                mediaUrl = item.getUrl();
                        }

                        file_extension = ".mp4";
                        break;
                    default:
                        return;
                }
                
            }
            catch
            {
#if DEBUG
                Console.WriteLine("File not found", title);
#endif

                Log.println("File not found: " + title);

                publishnotification("File not found", title, uniquenotificationID());
                return;
            }

#if DEBUG
            Console.WriteLine(mediaUrl);
#endif

            Log.println(mediaUrl);

            // Check if media url has some value
            if (mediaUrl != null && mediaTitle != null)
            {
                // Start download video file
                DownloadFile(mediaUrl, mediaTitle, isVideo, file_extension);

#if DEBUG
                Console.WriteLine("Start download");
#endif

                Log.println("Start download");
            }
            else
            {
#if DEBUG
                Console.WriteLine("File not found", title);
#endif
                Log.println("File not found: " + title);

                publishnotification("File not found", title, uniquenotificationID());
                return;
            }
        }

        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="Title"></param>
        /// <param name="isVideo"></param>
        private void DownloadFile(string URL, string Title, bool isVideo, string file_extension)
        {
            WebClient DownloadFile = new WebClient();

            // Event when download progess changed
            int notificationID = uniquenotificationID();

            Notification.Builder builder_ = publishnotification_("Downloading", Title, notificationID);

            DownloadFile.DownloadProgressChanged += (sender, e) => DownloadFileProgressChanged(sender, e, builder_);

            // Event when download completed
            DownloadFile.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

            // Start download
            var directory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

            // Check if directory exists
            try
            {
                if (!System.IO.Directory.Exists(directory.AbsolutePath))
                    System.IO.Directory.CreateDirectory(directory.AbsolutePath);
            }
            catch { }

            string file = string.Empty;
            if (isVideo)
            {
                file = Path.Combine(directory.AbsolutePath, makeFilenameValid(Title).Replace("/", "")
                    .Replace(".", "")
                    .Replace("|", "")
                    .Replace("?", "")
                    .Replace("!", "") + file_extension);
            }
            else
            {
                if (URL.Contains("youtube"))
                {
                    file = Path.Combine(directory.AbsolutePath, makeFilenameValid(Title).Replace("/", "")
                    .Replace(".", "")
                    .Replace("|", "")
                    .Replace("?", "")
                    .Replace("!", "") + ".m4a");
                }
                else
                {
                    file = Path.Combine(directory.AbsolutePath, makeFilenameValid(Title).Replace("/", "")
                    .Replace(".", "")
                    .Replace("|", "")
                    .Replace("?", "")
                    .Replace("!", "") + ".mp3");
                }
            }
            
#if DEBUG
            Console.WriteLine(file);
#endif

            Log.println(file);

            DownloadFile.DownloadFileAsync(new Uri(URL), file, file + "|" + Title + "|" + notificationID);

#if DEBUG
            Console.WriteLine("Download started");
#endif

            Log.println("Download started");
        }

        /// <summary>
        /// Download progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="builder_"></param>
        private void DownloadFileProgressChanged(object sender, DownloadProgressChangedEventArgs e, Notification.Builder builder_)
        {
            // Progress
            string s = e.UserState as String;
            string[] s_ = s.Split(new char[] { '|' });

            try
            {
                // Notification
                builder_.SetContentTitle("Downloading - " + e.ProgressPercentage + " %");
                builder_.SetProgress(100, e.ProgressPercentage, false);

                NotificationManager notificationManager = (NotificationManager)ActivityContext.mActivity.GetSystemService(Context.NotificationService);
                notificationManager.Notify(Convert.ToInt32(s_[2]), builder_.Build());
            }
            catch
            {
            }

#if DEBUG
            Console.WriteLine(e.ProgressPercentage + " %");
#endif

            Log.println(e.ProgressPercentage + " %");
        }

        /// <summary>
        /// Download file completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Split arguments
            string s = e.UserState as String;

            string[] s_ = s.Split(new char[] { '|' });

            // Directory
            var directory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

            // File
            string mediaFile = s_[0];

            // Filename
            var filename = mediaFile.Replace(Convert.ToString(directory), "").Replace("/", "");

            // Write m4a tags
            try
            {
                string performer = "";
                string title = "";

                if (filename.Contains("-"))
                {
                    string[] split = filename.Split(new char[] { '-' });

                    performer = split[0].TrimStart().TrimEnd();

                    foreach (var s__ in split)
                    {
                        if (!s__.Contains(split[0]))
                            title += s__;
                    }

                    title = title.TrimStart().TrimEnd().Replace(".m4a", "").Replace(".mp3", "").Replace(".mp4", "").Replace(".m4u", "");
                }
                else
                {
                    if (filename.Contains(" "))
                    {
                        string[] split = filename.Split(new char[] { ' ' });

                        performer = split[0].TrimStart().TrimEnd();

                        foreach (var s__ in split)
                        {
                            if (!s__.Contains(split[0]))
                                title += s__ + " ";
                        }

                        title = title.TrimStart().TrimEnd().Replace(".m4a", "").Replace(".mp3", "").Replace(".mp4", "").Replace(".m4u", "");
                    }
                    else
                    {
                        performer = filename;
                        title = " ";
                    }
                }

                TagLib.File file = TagLib.File.Create(mediaFile);

                file.Tag.Performers = new string[] { performer };
                file.Tag.Title = title;

                file.Save();

#if DEBUG
                Console.WriteLine("Tags written");
#endif

                Log.println("Tags written");
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Failed to write tags");
#endif

                Log.println(ex.ToString());
            }

            try
            {
                publishnotification("Download completed", makeFilenameValid(filename), Convert.ToInt32(s_[2]));
            }
            catch
            {

            }

#if DEBUG
            Console.WriteLine("Download completed");
#endif

            Log.println("Download completed");
        }

        #endregion Download

        /// <summary>
        /// Remove invalid characters from filename
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string makeFilenameValid(string file)
        {
            char replacementChar = '_';

            var blacklist = new HashSet<char>(Path.GetInvalidFileNameChars());

            var output = file.ToCharArray();

            for (int i = 0, ln = output.Length; i < ln; i++)
            {
                if (blacklist.Contains(output[i]))
                    output[i] = replacementChar;
            }

            return new string(output);
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

    #region Search

    /// <summary>
    /// Perform search
    /// </summary>
    public static class SEARCH
    {
        /// <summary>
        /// Search
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tube"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool DO(string value, string tube, List<ListViewItem> items, DotsLoaderView dotsLoaderView)
        {
            try
            {
                Log.println("Search, tube: " + tube + ", search string: " + value);

                ActivityContext.mActivity.RunOnUiThread(() => dotsLoaderView.Show());
                
                switch (tube)
                {
                    case "YouTube":
                        var ytitems = YouTubeSearch.Query(value, 5);

                        foreach (var item in ytitems)
                        {
                            if (!String.IsNullOrEmpty(item.getTitle()) && 
                                !String.IsNullOrEmpty(item.getDuration()) &&
                                item.getDuration() != " ")
                            {
                                // Add items
                                items.Add(new ListViewItem()
                                {
                                    Title = item.getTitle(),
                                    Author = item.getAuthor(),
                                    Url = item.getUrl(),
                                    Duration = item.getDuration(),
                                    Thumbnail = item.getThumbnail()
                                });
                            }
                        }
                        break;
                    case "ccMixter":
                        var ccitems = ccMixterSearch.Query(value, 5);

                        foreach (var item in ccitems)
                        {
                            // Add items
                            items.Add(new ListViewItem()
                            {
                                Title = item.getTitle(),
                                Author = item.getAuthor(),
                                Url = item.getUrl()
                            });
                        }
                        break;
                    case "Dailymotion":
                        var dmitems = DailymotionSearch.Query(value, 5);

                        foreach (var item in dmitems)
                        {
                            // Add items
                            items.Add(new ListViewItem()
                            {
                                Title = item.getTitle(),
                                Author = item.getAuthor(),
                                Url = item.getUrl()
                            });
                        }
                        break;
                    case "Eroprofile":
                        var epitems = EroprofileSearch.Query(value, 5);

                        foreach (var item in epitems)
                        {
                            // Add items
                            items.Add(new ListViewItem()
                            {
                                Title = item.getTitle(),
                                Url = item.getUrl(),
                                Duration = item.getDuration(),
                                Thumbnail = item.getThumbnail()
                            });
                        }
                        break;
                    case "Pornhub":
                        var phitems = PornhubSearch.Query(value, 5);

                        foreach (var item in phitems)
                        {
                            // Add items
                            items.Add(new ListViewItem()
                            {
                                Title = item.getTitle(),
                                Url = item.getUrl(),
                                Duration = item.getDuration(),
                                Thumbnail = item.getThumbnail()
                            });
                        }
                        break;
                    case "Vimeo":
                        var vmitems = VimeoSearch.Query(value, 5);

                        foreach (var item in vmitems)
                        {
                            // Add items
                            items.Add(new ListViewItem()
                            {
                                Title = item.getTitle(),
                                Author = item.getAuthor(),
                                Url = item.getUrl(),
                                Duration = item.getDuration(),
                                Thumbnail = item.getThumbnail()
                            });
                        }
                        break;
                    case "xHamster":
                        var xhitems = xHamsterSearch.Query(value, 5);

                        foreach (var item in xhitems)
                        {
                            // Add items
                            items.Add(new ListViewItem()
                            {
                                Title = item.getTitle(),
                                Url = item.getUrl(),
                                Duration = item.getDuration(),
                                Thumbnail = item.getThumbnail()
                            });
                        }
                        break;
                    default:
                        ActivityContext.mActivity.RunOnUiThread(() => dotsLoaderView.Hide());
                        return false;
                }

                try
                {
                    items.Sort();
                }
                catch { }

                ActivityContext.mActivity.RunOnUiThread(() => dotsLoaderView.Hide());

                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif

                Log.println(ex.ToString());

                ActivityContext.mActivity.RunOnUiThread(() => dotsLoaderView.Hide());

                return false;
            }
        }
    }

    #endregion Search
}