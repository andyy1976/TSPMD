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
        }

        void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case (Resource.Id.nav_search):
                    var SearchFragment = new SearchFragment();
                    var fragmentSearch = FragmentManager.BeginTransaction();
                    fragmentSearch.Replace(Resource.Id.fragment_container, SearchFragment);
                    fragmentSearch.AddToBackStack(null);
                    fragmentSearch.Commit();

                    this.Title = "TSPMD";

                    break;
                case (Resource.Id.nav_files):
                    var MusicFragment = new FilesFragment();
                    var fragmentMusic = FragmentManager.BeginTransaction();
                    fragmentMusic.Replace(Resource.Id.fragment_container, MusicFragment);
                    fragmentMusic.AddToBackStack(null);
                    fragmentMusic.Commit();

                    this.Title = "TSPMD";

                    break;
            }

            // Close drawer
            drawerLayout.CloseDrawers();
        }
    }
}

