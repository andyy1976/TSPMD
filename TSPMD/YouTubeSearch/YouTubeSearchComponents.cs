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
    public class YouTubeSearchComponents
    {
        private String Title;
        private String Author;
        private String Description;
        private String Duration;
        private String Url;
        private String Thumbnail;

        public YouTubeSearchComponents(String Title, String Author, String Description, String Duration, String Url, String Thumbnail)
        {
            this.setTitle(Title);
            this.setAuthor(Author);
            this.setDescription(Description);
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

        public String getAuthor()
        {
            return Author;
        }

        public void setAuthor(String author)
        {
            Author = author;
        }

        public String getDescription()
        {
            return Description;
        }

        public void setDescription(String description)
        {
            Description = description;
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
