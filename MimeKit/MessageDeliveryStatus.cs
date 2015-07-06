//
// MessageDeliveryStatus.cs
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

using System;

using MimeKit.IO;

namespace MimeKit {
	/// <summary>
	/// A message delivery status MIME part.
	/// </summary>
	/// <remarks>
	/// A message delivery status MIME part is a machine readable notification denoting the
	/// delivery status of a message and has a MIME-type of message/delivery-status.
	/// </remarks>
	public class MessageDeliveryStatus : MimePart
	{
		HeaderListCollection groups;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessageDeliveryStatus"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MessageDeliveryStatus (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessageDeliveryStatus"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MessageDeliveryStatus"/>.
		/// </remarks>
		public MessageDeliveryStatus () : base ("message", "delivery-status")
		{
		}

		/// <summary>
		/// Get the groups of delivery status fields.
		/// </summary>
		/// <remarks>
		/// Gets the groups of delivery status fields. The first group of fields
		/// contains the per-message fields while each of the following groups
		/// contains fields that pertain to particular recipients of the message.
		/// </remarks>
		/// <value>The fields.</value>
		public HeaderListCollection StatusGroups {
			get {
				if (groups == null) {
					if (ContentObject == null) {
						ContentObject = new ContentObject (new MemoryBlockStream ());
						groups = new HeaderListCollection ();
					} else {
						groups = new HeaderListCollection ();

						using (var stream = ContentObject.Open ()) {
							var parser = new MimeParser (stream, MimeFormat.Entity);

							while (!parser.IsEndOfStream) {
								var fields = parser.ParseHeaders ();
								groups.Add (fields);
							}
						}
					}

					groups.Changed += OnGroupsChanged;
				}

				return groups;
			}
		}

		void OnGroupsChanged (object sender, EventArgs e)
		{
			var stream = new MemoryBlockStream ();
			var options = FormatOptions.Default;

			for (int i = 0; i < groups.Count; i++)
				groups[i].WriteTo (options, stream);

			stream.Position = 0;

			ContentObject = new ContentObject (stream);
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.MimeEntity"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMimeEntity"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMimeEntity"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException ("visitor");

			visitor.VisitMessageDeliveryStatus (this);
		}
	}
}
