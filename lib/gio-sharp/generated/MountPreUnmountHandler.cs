// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;

	public delegate void MountPreUnmountHandler(object o, MountPreUnmountArgs args);

	public class MountPreUnmountArgs : GLib.SignalArgs {
		public GLib.Mount Mount{
			get {
				return (GLib.Mount) Args[0];
			}
		}

	}
}