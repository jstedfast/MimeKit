using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit
{
	/// <summary>
	/// An interface for parsing messages, entities, and/or headers.
	/// </summary>
	/// <remarks>
	/// An interface for parsing messages, entities, and/or headers. Implemented by <see cref="MimeParser"/> and
	/// <see cref="ExperimentalMimeParser"/>.
	/// </remarks>
	public interface IMimeParser
	{
		/// <summary>
		/// Get or set the parser options.
		/// </summary>
		/// <remarks>
		/// Gets or sets the parser options.
		/// </remarks>
		/// <value>The parser options.</value>
		public ParserOptions Options { get; set; }

		/// <summary>
		/// Get a value indicating whether the parser has reached the end of the input stream.
		/// </summary>
		/// <remarks>
		/// Gets a value indicating whether the parser has reached the end of the input stream.
		/// </remarks>
		/// <value><see langword="true" /> if this parser has reached the end of the input stream;
		/// otherwise, <see langword="false" />.</value>
		bool IsEndOfStream { get; }

		/// <summary>
		/// Get the current position of the parser within the stream.
		/// </summary>
		/// <remarks>
		/// Gets the current position of the parser within the stream.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long Position { get; }

		/// <summary>
		/// Get the mbox marker stream offset for the most recently parsed message.
		/// </summary>
		/// <remarks>
		/// <para>Gets the mbox marker stream offset for the most recently parsed message.</para>
		/// <para>If the <see cref="IMimeParser"/> was not initialized to parse the <see cref="MimeFormat.Mbox"/> format or if
		/// the most recent call to <see cref="ParseMessage(CancellationToken)"/> or <see cref="ParseMessageAsync(CancellationToken)"/>
		/// was not successful, then this property will return <c>-1</c>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMbox" />
		/// </example>
		/// <value>The mbox marker stream offset.</value>
		long MboxMarkerOffset { get; }

		/// <summary>
		/// Get the mbox marker for the most recently parsed message.
		/// </summary>
		/// <remarks>
		/// <para>Gets the mbox marker for the most recently parsed message.</para>
		/// <para>If the <see cref="IMimeParser"/> was not initialized to parse the <see cref="MimeFormat.Mbox"/> format or if
		/// the most recent call to <see cref="ParseMessage(CancellationToken)"/> or <see cref="ParseMessageAsync(CancellationToken)"/>
		/// was not successful, then this property will return <see langword="null"/>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeParserExamples.cs" region="ParseMbox" />
		/// </example>
		/// <value>The mbox marker.</value>
		string? MboxMarker { get; }

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <see langword="true" /> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <param name="persistent"><see langword="true" /> if the stream is persistent; otherwise, <see langword="false" />.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		void SetStream (Stream stream, MimeFormat format, bool persistent);

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// Sets the stream to parse.
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="format">The format of the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		void SetStream (Stream stream, MimeFormat format = MimeFormat.Default);

		/// <summary>
		/// Set the stream to parse.
		/// </summary>
		/// <remarks>
		/// <para>Sets the stream to parse.</para>
		/// <para>If <paramref name="persistent"/> is <see langword="true" /> and <paramref name="stream"/> is seekable, then
		/// the <see cref="MimeParser"/> will not copy the content of <see cref="MimePart"/>s into memory. Instead,
		/// it will use a <see cref="MimeKit.IO.BoundStream"/> to reference a substream of <paramref name="stream"/>.
		/// This has the potential to not only save memory usage, but also improve <see cref="MimeParser"/>
		/// performance.</para>
		/// <para>It should be noted, however, that disposing <paramref name="stream"/> will make it impossible
		/// for <see cref="MimeContent"/> to read the content.</para>
		/// </remarks>
		/// <param name="stream">The stream to parse.</param>
		/// <param name="persistent"><see langword="true" /> if the stream is persistent; otherwise, <see langword="false" />.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		void SetStream (Stream stream, bool persistent);

		/// <summary>
		/// Parse a list of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a list of headers from the stream.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		HeaderList ParseHeaders (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously parse a list of headers from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously parses a list of headers from the stream.
		/// </remarks>
		/// <returns>The parsed list of headers.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the headers.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task<HeaderList> ParseHeadersAsync (CancellationToken cancellationToken = default);

		/// <summary>
		/// Parse an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Parses an entity from the stream.
		/// </remarks>
		/// <returns>The parsed entity.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		MimeEntity ParseEntity (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously parse an entity from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously parses an entity from the stream.
		/// </remarks>
		/// <returns>The parsed entity.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the entity.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task<MimeEntity> ParseEntityAsync (CancellationToken cancellationToken = default);

		/// <summary>
		/// Parse a message from the stream.
		/// </summary>
		/// <remarks>
		/// Parses a message from the stream.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the message.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		MimeMessage ParseMessage (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously parse a message from the stream.
		/// </summary>
		/// <remarks>
		/// Asynchronously parses a message from the stream.
		/// </remarks>
		/// <returns>The parsed message.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// There was an error parsing the message.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task<MimeMessage> ParseMessageAsync (CancellationToken cancellationToken = default);
	}
}
