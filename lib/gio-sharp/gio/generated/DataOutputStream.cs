// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class DataOutputStream : GLib.FilterOutputStream {

		[Obsolete]
		protected DataOutputStream(GLib.GType gtype) : base(gtype) {}
		public DataOutputStream(IntPtr raw) : base(raw) {}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_data_output_stream_new(IntPtr base_stream);

		public DataOutputStream (GLib.OutputStream base_stream) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (DataOutputStream)) {
				ArrayList vals = new ArrayList();
				ArrayList names = new ArrayList();
				if (base_stream != null) {
					names.Add ("base_stream");
					vals.Add (new GLib.Value (base_stream));
				}
				CreateNativeObject ((string[])names.ToArray (typeof (string)), (GLib.Value[])vals.ToArray (typeof (GLib.Value)));
				return;
			}
			Raw = g_data_output_stream_new(base_stream == null ? IntPtr.Zero : base_stream.Handle);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern int g_data_output_stream_get_byte_order(IntPtr raw);

		[DllImport("libgio-2.0-0.dll")]
		static extern void g_data_output_stream_set_byte_order(IntPtr raw, int order);

		[GLib.Property ("byte-order")]
		public GLib.DataStreamByteOrder ByteOrder {
			get  {
				int raw_ret = g_data_output_stream_get_byte_order(Handle);
				GLib.DataStreamByteOrder ret = (GLib.DataStreamByteOrder) raw_ret;
				return ret;
			}
			set  {
				g_data_output_stream_set_byte_order(Handle, (int) value);
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_byte(IntPtr raw, byte data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutByte(byte data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_byte(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_int32(IntPtr raw, int data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutInt32(int data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_int32(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_uint16(IntPtr raw, ushort data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutUint16(ushort data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_uint16(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_string(IntPtr raw, IntPtr str, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutString(string str, GLib.Cancellable cancellable) {
			IntPtr native_str = GLib.Marshaller.StringToPtrGStrdup (str);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_string(Handle, native_str, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			GLib.Marshaller.Free (native_str);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_uint64(IntPtr raw, ulong data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutUint64(ulong data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_uint64(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_int16(IntPtr raw, short data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutInt16(short data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_int16(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_int64(IntPtr raw, long data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutInt64(long data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_int64(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_data_output_stream_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = g_data_output_stream_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern unsafe bool g_data_output_stream_put_uint32(IntPtr raw, uint data, IntPtr cancellable, out IntPtr error);

		public unsafe bool PutUint32(uint data, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_data_output_stream_put_uint32(Handle, data, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

#endregion
	}
}