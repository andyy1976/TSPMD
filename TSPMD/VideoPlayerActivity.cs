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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace TSPMD
{
    /// <summary>
    /// Video player activity
    /// </summary>
    [Activity(Label = "TSPMD")]
    public class VideoPlayerActivity : AppCompatActivity
    {
        private VideoView videoPlayer;
        private MediaController mediaController;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Remove Title
            this.RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.Player);

            // Button close
            Button buttonClose = FindViewById<Button>(Resource.Id.buttonClose);

            // Prevent sleeping
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            buttonClose.Click += delegate
            {
                Finish();
            };

            videoPlayer = FindViewById<VideoView>(Resource.Id.PlayerVideoView);

            var url = Intent.GetStringExtra("url") ?? "Not available";
            var title = Intent.GetStringExtra("title") ?? "Not available";

            videoPlayer.SetVideoURI(Android.Net.Uri.Parse(url));
            mediaController = new MediaController(this, true);
            videoPlayer.SetMediaController(mediaController);
            videoPlayer.Start();
        }
    }
}