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
using Android.Views;
using Android.Widget;

namespace TSPMD
{
    /// <summary>
    /// Settings fragment
    /// </summary>
    public class SettingsFragment : Fragment
    {
        View view;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            /* UI */

            var view = inflater.Inflate(Resource.Layout.Settings, container, false);

            this.view = view;

            var editTextSearchPages = view.FindViewById<EditText>(Resource.Id.editTextSearchPages);
            editTextSearchPages.Text = Settings.Retrieve("SearchPages");
            
            var buttonSave = view.FindViewById<Button>(Resource.Id.buttonSave);
            buttonSave.Click += delegate
            {
                Settings.Save("SearchPages", editTextSearchPages.Text);
            };
            
            return view;
        }
    }
}