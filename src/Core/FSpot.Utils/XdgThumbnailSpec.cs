//
// XdgThumbnailSpec.cs
//
// Author:
//   Ruben Vermeersch <ruben@savanne.be>
//
// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using Gdk;

using Hyena;

namespace FSpot.Utils
{
    public enum ThumbnailSize
    {
        Normal,
        Large
    }

    public static class XdgThumbnailSpec
    {
#region Public API
        public delegate Pixbuf PixbufLoader (SafeUri uri);
        public static PixbufLoader DefaultLoader { get; set; }

        public static Pixbuf LoadThumbnail (SafeUri uri, ThumbnailSize size)
        {
            return LoadThumbnail (uri, size, DefaultLoader);
        }

        public static Pixbuf LoadThumbnail (SafeUri uri, ThumbnailSize size, PixbufLoader loader)
        {
            var thumb_uri = ThumbUri (uri, size);
            var pixbuf = LoadFromUri (thumb_uri);
            if (!IsValid (uri, pixbuf)) {
                Log.DebugFormat ("Invalid thumbnail, reloading: {0}", uri);
                if (pixbuf != null)
                    pixbuf.Dispose ();

                if (loader == null)
                    return null;

                pixbuf = CreateFrom (uri, thumb_uri, size, loader);
            }
            return pixbuf;
        }

        public static void RemoveThumbnail (SafeUri uri)
        {
            var normal_uri = ThumbUri (uri, ThumbnailSize.Normal);
            var large_uri = ThumbUri (uri, ThumbnailSize.Large);

            var file = GLib.FileFactory.NewForUri (normal_uri);
            if (file.Exists)
                file.Delete (null);

            file = GLib.FileFactory.NewForUri (large_uri);
            if (file.Exists)
                file.Delete (null);
        }
#endregion

#region Private helpers
        const string ThumbMTimeOpt = "tEXt::Thumb::MTime";
        const string ThumbUriOpt = "tEXt::Thumb::URI";

        static SafeUri home_dir = new SafeUri (Environment.GetFolderPath (Environment.SpecialFolder.Personal));

        private static Pixbuf CreateFrom (SafeUri uri, SafeUri thumb_uri, ThumbnailSize size, PixbufLoader loader)
        {
            var pixels = size == ThumbnailSize.Normal ? 128 : 256;
            Pixbuf pixbuf;
            try {
                pixbuf = loader (uri);
            } catch (Exception e) {
                Log.DebugFormat ("Failed loading image for thumbnailing: {0}", uri);
                Log.DebugException (e);
                return null;
            }
            double scale_x = (double) pixbuf.Width / pixels;
            double scale_y = (double) pixbuf.Height / pixels;
            double scale = Math.Max (1.0, Math.Max (scale_x, scale_y));
            int target_x = (int) (pixbuf.Width / scale);
            int target_y = (int) (pixbuf.Height / scale);
	    // FIXME, This isn't correct, but for now it ensures that the minimum
	    //        value is 1 so that pixbuf.ScaleSimple doesn't return null
	    //        Seems to only happen in rare(?) cases
	    if (target_x == 0)
		target_x = 1;
	    if (target_y == 0)
		target_y = 1;
            var thumb_pixbuf = pixbuf.ScaleSimple (target_x, target_y, InterpType.Bilinear);
            pixbuf.Dispose ();

            var file = GLib.FileFactory.NewForUri (uri);
            var info = file.QueryInfo ("time::modified", GLib.FileQueryInfoFlags.None, null);
            var mtime = info.GetAttributeULong ("time::modified").ToString ();

            thumb_pixbuf.Savev (thumb_uri.LocalPath, "png",
                                new string [] {ThumbUriOpt, ThumbMTimeOpt, null},
                                new string [] {uri, mtime});

            return thumb_pixbuf;
        }

        static SafeUri ThumbUri (SafeUri uri, ThumbnailSize size)
        {
            var hash = CryptoUtil.Md5Encode (uri);
            return home_dir.Append (".thumbnails")
                           .Append (size == ThumbnailSize.Normal ? "normal" : "large")
                           .Append (hash + ".png");
        }

        static Pixbuf LoadFromUri (SafeUri uri)
        {
            var file = GLib.FileFactory.NewForUri (uri);
            if (!file.Exists)
                return null;
            Pixbuf pixbuf;
            using (var stream = new GLib.GioStream (file.Read (null))) {
                try {
                    pixbuf = new Pixbuf (stream);
                } catch (Exception e) {
                    file.Delete ();
                    Log.DebugFormat ("Failed thumbnail: {0}", uri);
                    Log.DebugException (e);
                    return null;
                }
            }
            return pixbuf;
        }

        static bool IsValid (SafeUri uri, Pixbuf pixbuf)
        {
            if (pixbuf == null) {
                return false;
            }

            if (pixbuf.GetOption (ThumbUriOpt) != uri.ToString ()) {
                return false;
            }

            var file = GLib.FileFactory.NewForUri (uri);
            if (!file.Exists)
                return false;

            var info = file.QueryInfo ("time::modified", GLib.FileQueryInfoFlags.None, null);

            if (pixbuf.GetOption (ThumbMTimeOpt) != info.GetAttributeULong ("time::modified").ToString ()) {
                return false;
            }

            return true;
        }
#endregion

    }
}
