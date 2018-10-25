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
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading.Tasks;

namespace TSPMD
{
    /// <summary>
    /// Main view
    /// </summary>
    [Activity(Label = "@string/app_name", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity
    {
        DrawerLayout drawerLayout;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            // Init toolbar
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            // Attach item selected handler to navigation view
            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;

            // Create ActionBarDrawerToggle button and add it to the toolbar
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);

#pragma warning disable CS0618 // Typ oder Element ist veraltet
            drawerLayout.SetDrawerListener(drawerToggle);
#pragma warning restore CS0618 // Typ oder Element ist veraltet

            drawerToggle.SyncState();

            ActivityContext.mActivity = this;

            var SearchFragment = new SearchFragment();
            var fragmentSearch = FragmentManager.BeginTransaction();
            fragmentSearch.Replace(Resource.Id.fragment_container, SearchFragment);
            fragmentSearch.AddToBackStack(null);
            fragmentSearch.Commit();

            // Request permissions
#pragma warning disable CS4014
            requestPermissionsAsync();
#pragma warning restore CS4014
        }

        void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case (Resource.Id.nav_search): // Search
                    var SearchFragment = new SearchFragment();
                    var fragmentSearch = FragmentManager.BeginTransaction();
                    fragmentSearch.Replace(Resource.Id.fragment_container, SearchFragment);
                    fragmentSearch.AddToBackStack(null);
                    fragmentSearch.Commit();

                    this.Title = "TSPMD";

                    break;
                case (Resource.Id.nav_files): // Files
                    var FilesFragment = new FilesFragment();
                    var fragmentFiles = FragmentManager.BeginTransaction();
                    fragmentFiles.Replace(Resource.Id.fragment_container, FilesFragment);
                    fragmentFiles.AddToBackStack(null);
                    fragmentFiles.Commit();

                    this.Title = "TSPMD";

                    break;
                case (Resource.Id.nav_about): // About
                    var AboutFragment = new AboutFragment();
                    var fragmentAbout = FragmentManager.BeginTransaction();
                    fragmentAbout.Replace(Resource.Id.fragment_container, AboutFragment);
                    fragmentAbout.AddToBackStack(null);
                    fragmentAbout.Commit();

                    this.Title = "TSPMD";

                    break;
            }

            // Close drawer
            drawerLayout.CloseDrawers();
        }

        async Task requestPermissionsAsync()
        {
            await Task.Delay(2500);

            await permissionsAsync();
        }

        // Permissions
        private async Task permissionsAsync()
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
    }
}

