﻿//
// MessageDispositionNotification.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

using System;

using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A message disposition notification MIME part.
	/// </summary>
	/// <remarks>
	/// A message disposition notification MIME part is a machine readable notification
	/// denoting the disposition of a message once it has been successfully delivered 
	/// and has a MIME-type of message/disposition-notification.
	/// <seealso cref="MimeKit.MultipartReport"/>
	/// </remarks>
	public class MessageDispositionNotification : MimePart
	{
		HeaderList fields;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessageDispositionNotification"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MessageDispositionNotification (MimeEntityConstructorArgs args) : base (args)
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
					if (Content == null) {
						Content = new MimeContent (new MemoryBlockStream ());
						fields = new HeaderList ();
					} else {
						using (var stream = Content.Open ()) {
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
			var stream = new MemoryBlockStream ();
			var options = FormatOptions.Default;

			fields.WriteTo (options, stream);
			stream.Position = 0;

			Content = new MimeContent (stream);
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.MessageDispositionNotification"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMessageDispositionNotification"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMessageDispositionNotification"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMessageDispositionNotification (this);
		}
	}
}
