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

namespace TSPMD
{
    public class PornhubSearchComponents
    {
        private String Title;
        private String Duration;
        private String Url;
        private String Thumbnail;

        public PornhubSearchComponents(String Title, String Duration, String Url, String Thumbnail)
        {
            this.setTitle(Title);
            this.setDuration(Duration);
            this.setUrl(Url);
            this.setThumbnail(Thumbnail);
        }

        public String getTitle()
        {
            return Title;
        }

        public void setTitle(String title)
        {
            Title = title;
        }

        public String getDuration()
        {
            return Duration;
        }

        public void setDuration(String duration)
        {
            Duration = duration;
        }

        public String getUrl()
        {
            return Url;
        }

        public void setUrl(String url)
        {
            Url = url;
        }

        public String getThumbnail()
        {
            return Thumbnail;
        }

        public void setThumbnail(String thumbnail)
        {
            Thumbnail = thumbnail;
        }
    }
}
