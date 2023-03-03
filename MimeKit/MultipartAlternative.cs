// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using MimeKit.Text;

namespace MimeKit {
	/// <summary>
	/// A multipart/alternative MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/alternative MIME entity contains, as one might expect, is used to offer a list of
	/// alternative formats for the main body of the message (usually they will be "text/plain" and
	/// "text/html"). These alternatives are in order of increasing faithfulness to the original document
	/// (in other words, the last entity will be in a format that, when rendered, will most closely match
	/// what the sending client's WYSISYG editor produced).
	/// </remarks>
	public class MultipartAlternative : Multipart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartAlternative"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartAlternative (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartAlternative"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartAlternative"/> part.
		/// </remarks>
		/// <param name="args">An array of initialization parameters: headers and MIME entities.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="args"/> contains one or more arguments of an unknown type.
		/// </exception>
		public MultipartAlternative (params object[] args) : base ("alternative", args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartAlternative"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartAlternative"/> part.
		/// </remarks>
		public MultipartAlternative () : base ("alternative")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (MultipartAlternative));
		}

		/// <summary>
		/// Get the text of the text/plain alternative.
		/// </summary>
		/// <remarks>
		/// Gets the text of the text/plain alternative, if it exists.
		/// </remarks>
		/// <value>The text if a text/plain alternative exists; otherwise, <c>null</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartAlternative"/> has been disposed.
		/// </exception>
		public string TextBody {
			get { return GetTextBody (TextFormat.Plain); }
		}

		/// <summary>
		/// Get the HTML-formatted text of the text/html alternative.
		/// </summary>
		/// <remarks>
		/// Gets the HTML-formatted text of the text/html alternative, if it exists.
		/// </remarks>
		/// <value>The HTML if a text/html alternative exists; otherwise, <c>null</c>.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartAlternative"/> has been disposed.
		/// </exception>
		public string HtmlBody {
			get { return GetTextBody (TextFormat.Html); }
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MultipartAlternative"/> nodes
		/// calls <see cref="MimeVisitor.VisitMultipartAlternative"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMultipartAlternative"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartAlternative"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMultipartAlternative (this);
		}

		internal static string GetText (TextPart text)
		{
			if (text.IsFlowed) {
				var converter = new FlowedToText ();

				if (text.ContentType.Parameters.TryGetValue ("delsp", out string delsp))
					converter.DeleteSpace = string.Equals (delsp, "yes", StringComparison.OrdinalIgnoreCase);

				return converter.Convert (text.Text);
			}

			return text.Text;
		}

		/// <summary>
		/// Get the text body in the specified format.
		/// </summary>
		/// <remarks>
		/// Gets the text body in the specified format, if it exists.
		/// </remarks>
		/// <returns>The text body in the desired format if it exists; otherwise, <c>null</c>.</returns>
		/// <param name="format">The desired text format.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartAlternative"/> has been disposed.
		/// </exception>
		public string GetTextBody (TextFormat format)
		{
			CheckDisposed ();

			// walk the multipart/alternative children backwards from greatest level of faithfulness to the least faithful
			for (int i = Count - 1; i >= 0; i--) {
				if (this[i] is MultipartAlternative alternative) {
					// Note: nested multipart/alternative parts make no sense... yet here we are.
					return alternative.GetTextBody (format);
				}

				TextPart text;

				if (this[i] is MultipartRelated related) {
					var root = related.Root;

					alternative = root as MultipartAlternative;
					if (alternative != null)
						return alternative.GetTextBody (format);

					text = root as TextPart;
				} else {
					text = this[i] as TextPart;
				}

				if (text != null && text.IsFormat (format))
					return GetText (text);
			}

			return null;
		}
	}
}
