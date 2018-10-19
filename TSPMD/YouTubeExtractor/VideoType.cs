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

namespace TSPMD
{
    /// <summary>
	/// The video type. Also known as video container.
	/// </summary>
	public enum VideoType
    {
        /// <summary>
        /// Video for mobile devices (3GP).
        /// </summary>
        Mobile,

        Flash,
        Mp4,
        WebM,

        /// <summary>
        /// The video type is unknown. This can occur if YoutubeExtractor is not up-to-date.
        /// </summary>
        Unknown
    }
}
