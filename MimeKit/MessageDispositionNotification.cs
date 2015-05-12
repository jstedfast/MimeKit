//
// MessageDispositionNotification.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A message disposition notification MIME part.
	/// </summary>
	/// <remarks>
	/// A message disposition notification MIME part is a machine readable notification denoting the
	/// delivery status of a previously sent message.
	/// </remarks>
	public class MessageDispositionNotification : MimePart
	{
		HeaderList fields;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessageDispositionNotification"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public MessageDispositionNotification (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessageDispositionNotification"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MessageDispositionNotification"/>.
		/// </remarks>
		public MessageDispositionNotification () : base ("message", "disposition-notification")
		{
		}

		/// <summary>
		/// Get the disposition notification fields.
		/// </summary>
		/// <remarks>
		/// Gets the disposition notification fields.
		/// </remarks>
		/// <value>The fields.</value>
		public HeaderList Fields {
			get {
				if (fields == null) {
					if (ContentObject == null) {
						ContentObject = new ContentObject (new MemoryBlockStream ());
						fields = new HeaderList ();
					} else {
						using (var stream = ContentObject.Open ()) {
							fields = HeaderList.Load (stream);
						}
					}

					fields.Changed += OnFieldsChanged;
				}

				return fields;
			}
		}

		void OnFieldsChanged (object sender, HeaderListChangedEventArgs e)
		{
			var options = FormatOptions.GetDefault ();
			var stream = new MemoryBlockStream ();

			fields.WriteTo (options, stream);
			stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
			stream.Position = 0;

			ContentObject = new ContentObject (stream);
		}
	}
}
