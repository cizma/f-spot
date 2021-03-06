// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;

#region Autogenerated code
	public interface Seekable : GLib.IWrapper {

		bool CanTruncate();
		long Position { 
			get;
		}
		bool Truncate(long offset, GLib.Cancellable cancellable);
		bool Seek(long offset, GLib.SeekType type, GLib.Cancellable cancellable);
		bool CanSeek { 
			get;
		}
	}

	[GLib.GInterface (typeof (SeekableAdapter))]
	public interface SeekableImplementor : GLib.IWrapper {

		long Tell ();
		bool CanSeek ();
		bool Seek (long offset, GLib.SeekType type, GLib.Cancellable cancellable);
		bool CanTruncate ();
		bool TruncateFn (long offset, GLib.Cancellable cancellable);
	}
#endregion
}
