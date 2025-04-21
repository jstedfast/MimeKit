using System;

namespace MimeKit.Utils
{
	internal delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) where TEventArgs : EventArgs;
}
