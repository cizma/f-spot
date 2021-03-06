// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[StructLayout(LayoutKind.Sequential)]
	public struct InputVector {

		private IntPtr _buffer;
		private UIntPtr size;
		public ulong Size {
			get {
				return (ulong) size;
			}
			set {
				size = new UIntPtr (value);
			}
		}

		public static GLib.InputVector Zero = new GLib.InputVector ();

		public static GLib.InputVector New(IntPtr raw) {
			if (raw == IntPtr.Zero)
				return GLib.InputVector.Zero;
			return (GLib.InputVector) Marshal.PtrToStructure (raw, typeof (GLib.InputVector));
		}

		private static GLib.GType GType {
			get { return GLib.GType.Pointer; }
		}
#endregion
	}
}
