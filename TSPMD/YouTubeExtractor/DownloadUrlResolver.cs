﻿///////////////////////////////////////////////////////////////////////////////
/// 
/// YoutubeExtractor
/// 
/// (c) 2016 flagbug
/// https://github.com/flagbug/YoutubeExtractor
/// 
/// Forked 2016 mrklintscher
/// https://github.com/mrklintscher/YoutubeExtractor
/// 
/// Forked 2016 Kimmax
/// https://github.com/Kimmax/SYMMExtractor
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy 
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
/// of the Software, and to permit persons to whom the Software is furnished to do so,
/// subject to the following conditions:
///
///	The above copyright notice and this permission notice shall be included in all 
/// copies or substantial portions of the Software.
///
///	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
/// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
/// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
/// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
/// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace TSPMD
{
    /// <summary>
	/// Provides a method to get the download link of a YouTube video.
	/// </summary>
	public static class DownloadUrlResolver
    {
        private const string RateBypassFlag = "ratebypass";
        private const string SignatureQuery = "signature";

        /// <summary>
        /// Decrypts the signature in the <see cref="VideoInfo.DownloadUrl" /> property and sets it
        /// to the decrypted URL. Use this method, if you have decryptSignature in the <see
        /// cref="GetDownloadUrls" /> method set to false.
        /// </summary>
        /// <param name="videoInfo">The video info which's downlaod URL should be decrypted.</param>
        /// <exception cref="YoutubeParseException">
        /// There was an error while deciphering the signature.
        /// </exception>
        public static void DecryptDownloadUrl(VideoInfo videoInfo)
        {
            IDictionary<string, string> queries = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);

            if (queries.ContainsKey(SignatureQuery))
            {
                string encryptedSignature = queries[SignatureQuery];

                string decrypted;

                try
                {
                    decrypted = GetDecipheredSignature(videoInfo.HtmlPlayerVersion, encryptedSignature);

                    videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SignatureQuery, decrypted);
                    videoInfo.RequiresDecryption = false;
                }

                catch (Exception ex)
                {
                    Log.println("Could not decipher signature. " +  ex);
                }
            }
        }

        /// <summary>
        /// Gets a list of <see cref="VideoInfo" />s for the specified URL.
        /// </summary>
        /// <param name="videoUrl">The URL of the YouTube video.</param>
        /// <param name="decryptSignature">
        /// A value indicating whether the video signatures should be decrypted or not. Decrypting
        /// consists of a HTTP request for each <see cref="VideoInfo" />, so you may want to set
        /// this to false and call <see cref="DecryptDownloadUrl" /> on your selected <see
        /// cref="VideoInfo" /> later.
        /// </param>
        /// <returns>A list of <see cref="VideoInfo" />s that can be used to download the video.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="videoUrl" /> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="videoUrl" /> parameter is not a valid YouTube URL.
        /// </exception>
        /// <exception cref="VideoNotAvailableException">The video is not available.</exception>
        /// <exception cref="WebException">
        /// An error occurred while downloading the YouTube page html.
        /// </exception>
        /// <exception cref="YoutubeParseException">The Youtube page could not be parsed.</exception>
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
        {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            bool isYoutubeUrl = TryNormalizeYoutubeUrl(videoUrl, out videoUrl);

            if (!isYoutubeUrl)
            {
                Log.println("URL is not a valid youtube URL!");

                return null;
            }

            try
            {
                var json = LoadJson(videoUrl);

                string videoTitle = GetVideoTitle(json);

                IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(json);

                IEnumerable<VideoInfo> infos = GetVideoInfos(downloadUrls, videoTitle).ToList();

                string htmlPlayerVersion = GetHtml5PlayerVersion(json);

                foreach (VideoInfo info in infos)
                {
                    info.HtmlPlayerVersion = htmlPlayerVersion;

                    if (decryptSignature && info.RequiresDecryption)
                    {
                        DecryptDownloadUrl(info);
                    }
                }

                return infos;
            }

            catch (Exception ex)
            {
                Log.println(ex + " " + videoUrl);
            }

            return null; // Will never happen, but the compiler requires it
        }

#if PORTABLE

        public static System.Threading.Tasks.Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl, bool decryptSignature = true)
        {
            return System.Threading.Tasks.Task.Run(() => GetDownloadUrls(videoUrl, decryptSignature));
        }

#endif

        /// <summary>
        /// Normalizes the given YouTube URL to the format http://youtube.com/watch?v={youtube-id}
        /// and returns whether the normalization was successful or not.
        /// </summary>
        /// <param name="url">The YouTube URL to normalize.</param>
        /// <param name="normalizedUrl">The normalized YouTube URL.</param>
        /// <returns>
        /// <c>true</c>, if the normalization was successful; <c>false</c>, if the URL is invalid.
        /// </returns>
        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/"))
            {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = HttpHelper.ParseQueryString(url);

            string v;

            if (!query.TryGetValue("v", out v))
            {
                normalizedUrl = null;
                return false;
            }

            normalizedUrl = "http://youtube.com/watch?v=" + v;

            return true;
        }

        private static IEnumerable<ExtractionInfo> ExtractDownloadUrls(JObject json)
        {
            string[] splitByUrls = GetStreamMap(json).Split(',');
            string[] adaptiveFmtSplitByUrls = GetAdaptiveStreamMap(json).Split(',');
            splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray();

            foreach (string s in splitByUrls)
            {
                IDictionary<string, string> queries = HttpHelper.ParseQueryString(s);
                string url;

                bool requiresDecryption = false;

                if (queries.ContainsKey("s") || queries.ContainsKey("sig"))
                {
                    requiresDecryption = queries.ContainsKey("s");
                    string signature = queries.ContainsKey("s") ? queries["s"] : queries["sig"];

                    url = string.Format("{0}&{1}={2}", queries["url"], SignatureQuery, signature);

                    string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;

                    url += fallbackHost;
                }

                else
                {
                    url = queries["url"];
                }

                url = HttpHelper.UrlDecode(url);
                url = HttpHelper.UrlDecode(url);

                IDictionary<string, string> parameters = HttpHelper.ParseQueryString(url);
                if (!parameters.ContainsKey(RateBypassFlag))
                    url += string.Format("&{0}={1}", RateBypassFlag, "yes");

                yield return new ExtractionInfo { RequiresDecryption = requiresDecryption, Uri = new Uri(url) };
            }
        }

        private static string GetAdaptiveStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["adaptive_fmts"];

            // bugfix: adaptive_fmts is missing in some videos, use url_encoded_fmt_stream_map instead
            if (streamMap == null)
            {
                streamMap = json["args"]["url_encoded_fmt_stream_map"];
            }

            return streamMap.ToString();
        }

        private static string GetDecipheredSignature(string htmlPlayerVersion, string signature)
        {
            return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
        }

        private static string GetHtml5PlayerVersion(JObject json)
        {
            try
            {
                var regex = new Regex(@"player(.+?).js");

                string js = json["assets"]["js"].ToString();

                return regex.Match(js).Result("$1");
            }
            catch (Exception ex)
            {
                Log.println(ex.ToString());

                return string.Empty;
            }
        }

        private static string GetStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["url_encoded_fmt_stream_map"];

            string streamMapString = streamMap == null ? null : streamMap.ToString();

            if (streamMapString == null || streamMapString.Contains("been+removed"))
            {
                Log.println("Video is removed or has an age restriction.");
            }

            return streamMapString;
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<ExtractionInfo> extractionInfos, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (ExtractionInfo extractionInfo in extractionInfos)
            {
                string itag = HttpHelper.ParseQueryString(extractionInfo.Uri.Query)["itag"];

                int formatCode = int.Parse(itag);

                VideoInfo info = VideoInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

                if (info != null)
                {
                    info = new VideoInfo(info)
                    {
                        DownloadUrl = extractionInfo.Uri.ToString(),
                        Title = videoTitle,
                        RequiresDecryption = extractionInfo.RequiresDecryption
                    };
                }

                else
                {
                    info = new VideoInfo(formatCode)
                    {
                        DownloadUrl = extractionInfo.Uri.ToString()
                    };
                }

                downLoadInfos.Add(info);
            }

            return downLoadInfos;
        }

        private static string GetVideoTitle(JObject json)
        {
            JToken title = json["args"]["title"];

            return title == null ? String.Empty : title.ToString();
        }

        private static bool IsVideoUnavailable(string pageSource)
        {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

            return pageSource.Contains(unavailableContainer);
        }

        private static JObject LoadJson(string url)
        {
            string pageSource = HttpHelper.DownloadString(url);

            if (IsVideoUnavailable(pageSource))
            {
                return null;
            }

            var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);

            string extractedJson = dataRegex.Match(pageSource).Result("$1");

            return JObject.Parse(extractedJson);
        }

        private static void ThrowYoutubeParseException(Exception innerException, string videoUrl)
        {
            Log.println("Could not parse the Youtube page for URL " + videoUrl + "\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues" + innerException);
        }

        private class ExtractionInfo
        {
            public bool RequiresDecryption { get; set; }

            public Uri Uri { get; set; }
        }
    }
}
