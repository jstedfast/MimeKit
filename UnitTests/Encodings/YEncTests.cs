//
// YEncTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using System.Text;

using MimeKit;
using MimeKit.Encodings;
using UnitTests.IO;

namespace UnitTests.Encodings {
	[TestFixture]
	public class YEncTests
	{
		const string EmptyBlock = "=ybegin line=128 size=0 name=empty.bin\r\n=yend size=0 crc32=00000000\r\n";
		const string VectorBlock = "=ybegin line=128 size=9 name=test.bin\r\n[\\]^_`abc\r\n=yend size=9 crc32=cbf43926\r\n";
		static readonly byte[] Vector = Encoding.ASCII.GetBytes ("123456789");

		static Task<bool> ReadNext (YEncReader reader, bool doAsync, CancellationToken token = default)
		{
			return doAsync ? reader.ReadNextAsync (token) : Task.FromResult (reader.ReadNext (token));
		}

		static Task DecodeTo (YEncReader reader, Stream output, bool doAsync, CancellationToken token = default)
		{
			if (doAsync)
				return reader.DecodeToAsync (output, token);
			reader.DecodeTo (output, token);
			return Task.CompletedTask;
		}

		static Task<uint> Write (YEncWriter writer, YEncHeader header, Stream input, bool doAsync, uint? checksum = null, CancellationToken token = default)
		{
			return doAsync ? writer.WriteAsync (header, input, checksum, token) : Task.FromResult (writer.Write (header, input, checksum, token));
		}

		static async Task RoundTrip (NewLineFormat format, int chunkSize, int length, bool doAsync)
		{
			var data = Enumerable.Range (0, length).Select (i => (byte) (i + 214)).ToArray ();
			var options = new YEncOptions { NewLineFormat = format };
			using var encoded = new MemoryStream ();
			using var input = new ChunkedReadStream (data, chunkSize);
			using (var writer = new YEncWriter (encoded, options, true)) {
				var checksum = await Write (writer, new YEncHeader ("file name.bin", length, 60), input, doAsync);
				using var chunked = new ChunkedReadStream (encoded.ToArray (), chunkSize);
				using var nonseekable = new NonSeekableStream (chunked);
				using var reader = new YEncReader (nonseekable);
				using var output = new MemoryStream ();
				Assert.That (await ReadNext (reader, doAsync), Is.True);
				Assert.That (reader.Header.FileName, Is.EqualTo ("file name.bin"));
				Assert.That (reader.Header.Size, Is.EqualTo (length));
				Assert.That (reader.Header.LineLength, Is.EqualTo (60));
				Assert.That (reader.Checksum, Is.Null);
				Assert.That (reader.DecodedSize, Is.Zero);
				await DecodeTo (reader, output, doAsync);
				Assert.That (output.ToArray (), Is.EqualTo (data));
				Assert.That (reader.DecodedSize, Is.EqualTo (length));
				Assert.That (reader.Checksum, Is.EqualTo (checksum));
				Assert.That (reader.ExpectedChecksum, Is.EqualTo (checksum));
				Assert.That (reader.ExpectedPartChecksum, Is.Null);
				Assert.That (await ReadNext (reader, doAsync), Is.False);
				Assert.That (reader.Header, Is.Null);
				Assert.That (reader.Checksum, Is.Null);
				Assert.That (await ReadNext (reader, doAsync), Is.False);
			}
			Assert.That (input.CanRead, Is.True);
			Assert.That (encoded.CanWrite, Is.True);
			if (format == NewLineFormat.Unix)
				Assert.That (encoded.ToArray (), Does.Not.Contain ((byte) '\r'));
		}

		[Test]
		public void TestRoundTrip ([Values (NewLineFormat.Dos, NewLineFormat.Unix)] NewLineFormat format, [Values (1, 17, 4096)] int chunkSize, [Values (0, 1, 256, 16385)] int length)
		{
			RoundTrip (format, chunkSize, length, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestRoundTripAsync ([Values (NewLineFormat.Dos, NewLineFormat.Unix)] NewLineFormat format, [Values (1, 17, 4096)] int chunkSize, [Values (0, 1, 256, 16385)] int length)
		{
			return RoundTrip (format, chunkSize, length, true);
		}

		static async Task WireFormat (bool doAsync)
		{
			using var output = new MemoryStream ();
			using var writer = new YEncWriter (output, leaveOpen: true);
			using var empty = new MemoryStream ();
			Assert.That (await Write (writer, new YEncHeader ("empty.bin", 0), empty, doAsync), Is.Zero);
			using var input = new NonSeekableStream (Vector);
			Assert.That (await Write (writer, new YEncHeader ("test.bin", 9), input, doAsync, 0xcbf43926), Is.EqualTo (0xcbf43926));
			Assert.That (Encoding.ASCII.GetString (output.ToArray ()), Is.EqualTo (EmptyBlock + VectorBlock));

			using var multipart = new MemoryStream ();
			using var partWriter = new YEncWriter (multipart);
			using var source = new MemoryStream (Vector);
			var header = new YEncHeader ("test.bin", 20, partNumber: 2, totalParts: 3, begin: 10, end: 18);
			Assert.That (await Write (partWriter, header, source, doAsync, 0x12345678), Is.EqualTo (0xcbf43926));
			var text = Encoding.ASCII.GetString (multipart.ToArray ());
			Assert.That (text, Is.EqualTo ("=ybegin part=2 total=3 line=128 size=20 name=test.bin\r\n=ypart begin=10 end=18\r\n[\\]^_`abc\r\n=yend size=9 part=2 total=3 pcrc32=cbf43926 crc32=12345678\r\n"));
			multipart.Position = 0;
			using var reader = new YEncReader (multipart, leaveOpen: true);
			Assert.That (await ReadNext (reader, doAsync), Is.True);
			Assert.That (reader.Header.Begin, Is.EqualTo (10));
			Assert.That (reader.Header.End, Is.EqualTo (18));
			Assert.That (reader.Header.PartNumber, Is.EqualTo (2));
			Assert.That (reader.Header.TotalParts, Is.EqualTo (3));
			Assert.That (reader.Header.DecodedSize, Is.EqualTo (9));
			await DecodeTo (reader, Stream.Null, doAsync);
			Assert.That (reader.ExpectedPartChecksum, Is.EqualTo (0xcbf43926));
			Assert.That (reader.ExpectedChecksum, Is.EqualTo (0x12345678));
		}

		[Test]
		public void TestWireFormat ()
		{
			WireFormat (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestWireFormatAsync ()
		{
			return WireFormat (true);
		}

		static async Task Corpus (int chunkSize, bool doAsync)
		{
			string dir = Path.Combine (TestHelper.ProjectDir, "TestData", "yenc");
			var expected = File.ReadAllBytes (Path.Combine (dir, "joystick.jpg"));
			var parts = new[] { "00000020.ntx", "00000021.ntx" };
			var checksums = new uint[] { 0xbfae5c0b, 0xaca76043 };
			using var output = new MemoryStream ();
			for (int i = 0; i < parts.Length; i++) {
				using var input = new ChunkedReadStream (File.ReadAllBytes (Path.Combine (dir, parts[i])), chunkSize);
				using var reader = new YEncReader (input);
				Assert.That (await ReadNext (reader, doAsync), Is.True);
				Assert.That (reader.Header.FileName, Is.EqualTo ("joystick.jpg"));
				Assert.That (reader.Header.Size, Is.EqualTo (19338));
				Assert.That (reader.Header.PartNumber, Is.EqualTo (i + 1));
				Assert.That (reader.Header.Begin, Is.EqualTo (i == 0 ? 1 : 11251));
				Assert.That (reader.Header.End, Is.EqualTo (i == 0 ? 11250 : 19338));
				await DecodeTo (reader, output, doAsync);
				Assert.That (reader.Checksum, Is.EqualTo (checksums[i]));
				Assert.That (reader.ExpectedPartChecksum, Is.EqualTo (checksums[i]));
				Assert.That (await ReadNext (reader, doAsync), Is.False);
			}
			Assert.That (output.ToArray (), Is.EqualTo (expected));
		}

		[Test]
		public void TestCorpus ([Values (1, 4096)] int chunkSize)
		{
			Corpus (chunkSize, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestCorpusAsync ([Values (1, 4096)] int chunkSize)
		{
			return Corpus (chunkSize, true);
		}

		static async Task MultipleBlocks (bool doAsync)
		{
			var text = "preamble\r\n=ybegin2 ignored\r\n" + EmptyBlock + "interstitial\n" + VectorBlock + "trailing";
			using var source = new ChunkedReadStream (Encoding.ASCII.GetBytes (text), 7);
			using var reader = new YEncReader (source);
			using var output = new MemoryStream ();
			Assert.That (await ReadNext (reader, doAsync), Is.True);
			await DecodeTo (reader, output, doAsync);
			Assert.That (reader.Checksum, Is.Zero);
			Assert.That (await ReadNext (reader, doAsync), Is.True);
			await DecodeTo (reader, output, doAsync);
			Assert.That (output.ToArray (), Is.EqualTo (Vector));
			Assert.That (reader.Checksum, Is.EqualTo (0xcbf43926));
			Assert.That (await ReadNext (reader, doAsync), Is.False);
		}

		[Test]
		public void TestMultipleBlocks ()
		{
			MultipleBlocks (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestMultipleBlocksAsync ()
		{
			return MultipleBlocks (true);
		}

		static readonly string[] ValidBlocks = {
			"=ybegin line=128 size=0 name=empty.bin\n=yend size=0",
			"=ybegin line=128 size=0 name=empty.bin\r\n=yend size=0\r",
			"=ybegin  future=1 line=128 size=0 name=  empty name=1.bin  \n=yend size=0 crc32=00000000",
			"=ybegin line=128 size=9 name=test.bin\n[\\]^_`abc\n=yend size=9 crc32=CBF43926",
			"=ybegin part=1 line=128 size=9 name=test.bin\n=ypart begin=1 end=9\n[\\]^_`abc\n=yend size=9 part=1",
			"=ybegin part=1 total=1 line=128 size=9 name=test.bin\n=ypart begin=1 end=9\n[\\]^_`abc\n=yend size=9 part=1 total=1 pcrc32=cbf43926 crc32=cbf43926",
			"=ybegin line=60 size=60 name=test.bin\n" + new string ('*', 59) + "=@\n=yend size=60"
		};

		static async Task ReadBlock (string text, bool doAsync)
		{
			using var input = new ChunkedReadStream (Encoding.Latin1.GetBytes (text), 1);
			using var reader = new YEncReader (input);
			Assert.That (await ReadNext (reader, doAsync), Is.True);
			await DecodeTo (reader, Stream.Null, doAsync);
			Assert.That (reader.DecodedSize, Is.EqualTo (reader.Header.DecodedSize));
			Assert.That (reader.Checksum, Is.Not.Null);
		}

		[TestCaseSource (nameof (ValidBlocks))]
		public void TestValidBlock (string text)
		{
			ReadBlock (text, false).GetAwaiter ().GetResult ();
		}

		[TestCaseSource (nameof (ValidBlocks))]
		public Task TestValidBlockAsync (string text)
		{
			return ReadBlock (text, true);
		}

		static readonly string[] InvalidBlocks = {
			"=ybegin line=128 size=0",
			"=ybegin line=128 size=0 size=0 name=x\n",
			"=ybegin line=128 size=-1 name=x\n",
			"=ybegin line=128 size=9223372036854775808 name=x\n",
			"=ybegin line=2147483648 size=0 name=x\n",
			"=ybegin line=0 size=0 name=x\n",
			"=ybegin line= size=0 name=x\n",
			"=ybegin line=128 invalid size=0 name=x\n",
			"=ybegin line=128 size=0 name=\n",
			"=ybegin line=128 size=0 name=a\0b\n",
			"=ybegin part=0 line=128 size=1 name=x\n=ypart begin=1 end=1\n",
			"=ybegin part=2 total=1 line=128 size=1 name=x\n=ypart begin=1 end=1\n",
			"=ybegin part=1 line=128 size=1 name=x\n",
			"=ybegin part=1 line=128 size=1 name=x\n=yend size=0\n",
			"=ybegin part=1 line=128 size=1 name=x\n=ypart begin=0 end=1\n",
			"=ybegin part=1 line=128 size=1 name=x\n=ypart begin=1 end=2\n",
			"=ybegin line=128 size=0 name=x\n",
			"=ybegin line=128 size=0 name=x\n=yend\n",
			"=ybegin line=128 size=0 name=x\n=ybegin line=128 size=0 name=y\n",
			"=ybegin line=128 size=0 name=x\n=yend size=1\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 crc32=00000001\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 pcrc32=00000001\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 crc32=0\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 crc32=gggggggg\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 name=x crc32=00000001\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 name=x size=1\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 part=1\n",
			"=ybegin line=128 size=0 name=x\n=yend size=0 total=1\n",
			"=ybegin line=128 size=1 name=x\n=yend size=1\n",
			"=ybegin line=128 size=1 name=x\n**\n=yend size=2\n",
			"=ybegin line=128 size=1 name=x\n=\n=yend size=1\n",
			"=ybegin line=128 size=1 name=x\n=\0\n=yend size=1\n",
			"=ybegin line=128 size=1 name=x\n\0\n=yend size=1\n",
			"=ybegin line=128 size=1 name=x\n\r*\n=yend size=1\n",
			"=ybegin line=1 size=2 name=x\n**\n=yend size=2\n",
			"=ybegin line=2 size=2 name=x\n==*\n=yend size=2\n",
			"=ybegin part=1 line=128 size=1 name=x\n=ypart begin=1 end=1\n*\n=yend size=1\n",
			"=ybegin part=1 line=128 size=1 name=x\n=ypart begin=1 end=1\n*\n=yend size=1 part=2\n",
			"=ybegin part=1 line=128 size=1 name=x\n=ypart begin=1 end=1\n*\n=yend size=1 part=1 crc32=00000000\n",
		};

		static async Task InvalidBlock (string text, bool doAsync)
		{
			using var input = new ChunkedReadStream (Encoding.Latin1.GetBytes (text), 1);
			using var reader = new YEncReader (input);
			Assert.ThrowsAsync<FormatException> (async () => {
				if (await ReadNext (reader, doAsync))
					await DecodeTo (reader, Stream.Null, doAsync);
			});
			Assert.That (reader.Checksum, Is.Null);
			Assert.ThrowsAsync<InvalidOperationException> (async () => await ReadNext (reader, doAsync));
			await Task.CompletedTask;
		}

		[TestCaseSource (nameof (InvalidBlocks))]
		public void TestInvalidBlock (string text)
		{
			InvalidBlock (text, false).GetAwaiter ().GetResult ();
		}

		[TestCaseSource (nameof (InvalidBlocks))]
		public Task TestInvalidBlockAsync (string text)
		{
			return InvalidBlock (text, true);
		}

		static async Task WriterInvalidSource (int size, uint? checksum, bool doAsync)
		{
			using var output = new MemoryStream ();
			using var writer = new YEncWriter (output);
			using var source = new MemoryStream (Vector);
			Assert.ThrowsAsync<FormatException> (async () => await Write (writer, new YEncHeader ("x", size), source, doAsync, checksum));
			Assert.That (Encoding.ASCII.GetString (output.ToArray ()), Does.Not.Contain ("=yend "));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await Write (writer, new YEncHeader ("x", 0), source, doAsync));
			Assert.That (source.CanRead, Is.True);
			await Task.CompletedTask;
		}

		[TestCase (8, null)]
		[TestCase (10, null)]
		[TestCase (9, 0u)]
		public void TestWriterInvalidSource (int size, uint? checksum)
		{
			WriterInvalidSource (size, checksum, false).GetAwaiter ().GetResult ();
		}

		[TestCase (8, null)]
		[TestCase (10, null)]
		[TestCase (9, 0u)]
		public Task TestWriterInvalidSourceAsync (int size, uint? checksum)
		{
			return WriterInvalidSource (size, checksum, true);
		}

		static async Task FileNameEncoding (Encoding encoding, string name, bool doAsync)
		{
			var options = new YEncOptions { FileNameEncoding = encoding };
			using var output = new MemoryStream ();
			using var writer = new YEncWriter (output, options, true);
			options.FileNameEncoding = Encoding.ASCII;
			using var empty = new MemoryStream ();
			await Write (writer, new YEncHeader (name, 0), empty, doAsync);
			Assert.That (encoding.GetString (output.ToArray ()), Does.Contain ("name=" + name + "\r\n"));
			output.Position = 0;
			using var reader = new YEncReader (output, new YEncOptions { FileNameEncoding = encoding });
			Assert.That (await ReadNext (reader, doAsync), Is.True);
			Assert.That (reader.Header.FileName, Is.EqualTo (name));
			await DecodeTo (reader, Stream.Null, doAsync);
		}

		static async Task InvalidFileNameEncoding (bool doAsync)
		{
			using var output = new MemoryStream ();
			using var writer = new YEncWriter (output, new YEncOptions { FileNameEncoding = Encoding.Unicode });
			using var empty = new MemoryStream ();
			Assert.ThrowsAsync<ArgumentException> (async () => await Write (writer, new YEncHeader ("x", 0), empty, doAsync));
			Assert.That (output.Length, Is.Zero);
			using var input = new MemoryStream (Encoding.Latin1.GetBytes ("=ybegin line=128 size=0 name=\u00ff\n=yend size=0\n"));
			using var reader = new YEncReader (input, new YEncOptions { FileNameEncoding = Encoding.UTF8 });
			Assert.ThrowsAsync<FormatException> (async () => await ReadNext (reader, doAsync));
			await Task.CompletedTask;
		}

		[Test]
		public void TestInvalidFileNameEncoding ()
		{
			InvalidFileNameEncoding (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestInvalidFileNameEncodingAsync ()
		{
			return InvalidFileNameEncoding (true);
		}

		[Test]
		public void TestFileNameEncoding ()
		{
			FileNameEncoding (Encoding.Latin1, "caf\u00e9.bin", false).GetAwaiter ().GetResult ();
			FileNameEncoding (Encoding.UTF8, "\u65e5\u672c\u8a9e.bin", false).GetAwaiter ().GetResult ();
		}

		[Test]
		public async Task TestFileNameEncodingAsync ()
		{
			await FileNameEncoding (Encoding.Latin1, "caf\u00e9.bin", true);
			await FileNameEncoding (Encoding.UTF8, "\u65e5\u672c\u8a9e.bin", true);
		}

		static async Task Limits (bool doAsync)
		{
			var options = new YEncOptions { MaxLineLength = 128, MaxDecodedSize = 9 };
			var longLine = new string ('x', 128);
			using (var input = new ChunkedReadStream (Encoding.ASCII.GetBytes (longLine + "\r\n" + VectorBlock), 1))
			using (var reader = new YEncReader (input, options)) {
				Assert.That (await ReadNext (reader, doAsync), Is.True);
				await DecodeTo (reader, Stream.Null, doAsync);
			}
			foreach (var text in new[] { longLine + "x\r\n", longLine + "\rx", VectorBlock.Replace ("size=9", "size=10"), EmptyBlock.Replace ("line=128", "line=129") }) {
				using var input = new ChunkedReadStream (Encoding.ASCII.GetBytes (text), 1);
				using var reader = new YEncReader (input, options);
				Assert.ThrowsAsync<FormatException> (async () => await ReadNext (reader, doAsync));
			}
			using var source = new ChunkedReadStream (Encoding.ASCII.GetBytes (EmptyBlock), 1);
			using var zeroReader = new YEncReader (source, new YEncOptions { MaxDecodedSize = 0 });
			Assert.That (await ReadNext (zeroReader, doAsync), Is.True);
			await DecodeTo (zeroReader, Stream.Null, doAsync);

			using var output = new MemoryStream ();
			using var writer = new YEncWriter (output, new YEncOptions { MaxLineLength = 61, MaxDecodedSize = long.MaxValue });
			using var empty = new MemoryStream ();
			var largePart = new YEncHeader ("x", 1000000000000000000, 60, partNumber: 1, begin: 1, end: 1000000000000000000);
			Assert.ThrowsAsync<ArgumentException> (async () => await Write (writer, largePart, empty, doAsync, 0));
			Assert.That (output.Length, Is.Zero);
			await Write (writer, new YEncHeader ("x", 0, 60), empty, doAsync);
		}

		[Test]
		public void TestLimits ()
		{
			Limits (false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestLimitsAsync ()
		{
			return Limits (true);
		}

		static async Task StateAndOwnership (bool leaveOpen, bool doAsync)
		{
			using var stream = new MemoryStream (Encoding.ASCII.GetBytes (EmptyBlock));
			var reader = new YEncReader (stream, leaveOpen: leaveOpen);
			Assert.ThrowsAsync<InvalidOperationException> (async () => await DecodeTo (reader, Stream.Null, doAsync));
			Assert.That (await ReadNext (reader, doAsync), Is.True);
			Assert.ThrowsAsync<InvalidOperationException> (async () => await ReadNext (reader, doAsync));
			var canceled = new CancellationToken (true);
			Assert.ThrowsAsync<OperationCanceledException> (async () => await DecodeTo (reader, Stream.Null, doAsync, canceled));
			await DecodeTo (reader, Stream.Null, doAsync);
			Assert.ThrowsAsync<InvalidOperationException> (async () => await DecodeTo (reader, Stream.Null, doAsync));
			Assert.ThrowsAsync<OperationCanceledException> (async () => await ReadNext (reader, doAsync, canceled));
			Assert.That (await ReadNext (reader, doAsync), Is.False);
			reader.Dispose ();
			reader.Dispose ();
			Assert.That (stream.CanRead, Is.EqualTo (leaveOpen));
			Assert.ThrowsAsync<ObjectDisposedException> (async () => await ReadNext (reader, doAsync));
			Assert.ThrowsAsync<ObjectDisposedException> (async () => await DecodeTo (reader, Stream.Null, doAsync));

			using var destination = new MemoryStream ();
			using var empty = new MemoryStream ();
			var writer = new YEncWriter (destination, leaveOpen: leaveOpen);
			var header = new YEncHeader ("x", 0);
			Assert.ThrowsAsync<OperationCanceledException> (async () => await Write (writer, header, empty, doAsync, token: canceled));
			Assert.That (destination.Length, Is.Zero);
			await Write (writer, header, empty, doAsync);
			writer.Dispose ();
			writer.Dispose ();
			Assert.That (destination.CanWrite, Is.EqualTo (leaveOpen));
			Assert.ThrowsAsync<ObjectDisposedException> (async () => await Write (writer, header, empty, doAsync));
		}

		[Test]
		public void TestStateAndOwnership ([Values] bool leaveOpen)
		{
			StateAndOwnership (leaveOpen, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestStateAndOwnershipAsync ([Values] bool leaveOpen)
		{
			return StateAndOwnership (leaveOpen, true);
		}

		sealed class AsyncOnlyStream : MemoryStream
		{
			public AsyncOnlyStream () { }
			public AsyncOnlyStream (byte[] bytes) : base (bytes) { }
			public override bool CanSeek => false;
			public override long Position { get => throw new NotSupportedException (); set => throw new NotSupportedException (); }
			public override int Read (byte[] buffer, int offset, int count) => throw new AssertionException ("Unexpected synchronous read.");
			public override void Write (byte[] buffer, int offset, int count) => throw new AssertionException ("Unexpected synchronous write.");
			public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken token)
			{
				token.ThrowIfCancellationRequested ();
				return Task.FromResult (base.Read (buffer, offset, Math.Min (count, 1)));
			}
			public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken token)
			{
				token.ThrowIfCancellationRequested ();
				base.Write (buffer, offset, count);
				return Task.CompletedTask;
			}
		}

		[Test]
		public async Task TestAsyncOnlyStreams ()
		{
			using var input = new AsyncOnlyStream (Vector);
			using var encoded = new AsyncOnlyStream ();
			using var writer = new YEncWriter (encoded, leaveOpen: true);
			Assert.That (await writer.WriteAsync (new YEncHeader ("test.bin", 9), input), Is.EqualTo (0xcbf43926));
			using var source = new AsyncOnlyStream (encoded.ToArray ());
			using var output = new AsyncOnlyStream ();
			using var reader = new YEncReader (source);
			Assert.That (await reader.ReadNextAsync (), Is.True);
			await reader.DecodeToAsync (output);
			Assert.That (output.ToArray (), Is.EqualTo (Vector));
		}

		[Test]
		public void TestCancellableStreams ()
		{
			using var input = new CancellableMemoryStream ();
			input.Write (Vector, 0, Vector.Length);
			input.Position = 0;
			using var encoded = new CancellableMemoryStream ();
			using var writer = new YEncWriter (encoded, leaveOpen: true);
			writer.Write (new YEncHeader ("test.bin", 9), input);
			Assert.That (input.CancellableReads, Is.GreaterThan (0));
			Assert.That (encoded.CancellableWrites, Is.GreaterThan (0));
			encoded.Position = 0;
			using var output = new CancellableMemoryStream ();
			using var reader = new YEncReader (encoded);
			Assert.That (reader.ReadNext (), Is.True);
			reader.DecodeTo (output);
			Assert.That (encoded.CancellableReads, Is.GreaterThan (0));
			Assert.That (output.CancellableWrites, Is.GreaterThan (0));
			Assert.That (output.ToArray (), Is.EqualTo (Vector));
		}

		static async Task StreamFailures (bool cancel, bool doAsync)
		{
			Func<Exception> error = () => cancel ? new OperationCanceledException () : new IOException ();
			var expected = cancel ? typeof (OperationCanceledException) : typeof (IOException);
			using var readError = new ThrowingReadStream (error);
			using var reader = new YEncReader (readError);
			Assert.ThrowsAsync (expected, async () => await ReadNext (reader, doAsync));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await ReadNext (reader, doAsync));

			using var source = new MemoryStream (Encoding.ASCII.GetBytes (VectorBlock));
			using var decodingReader = new YEncReader (source);
			using var writeError = new ThrowingWriteStream (error);
			Assert.That (await ReadNext (decodingReader, doAsync), Is.True);
			Assert.ThrowsAsync (expected, async () => await DecodeTo (decodingReader, writeError, doAsync));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await DecodeTo (decodingReader, Stream.Null, doAsync));
			Assert.That (decodingReader.Checksum, Is.Null);

			using var writer = new YEncWriter (writeError);
			using var empty = new MemoryStream ();
			var header = new YEncHeader ("x", 0);
			Assert.ThrowsAsync (expected, async () => await Write (writer, header, empty, doAsync));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await Write (writer, header, empty, doAsync));

			using var output = new MemoryStream ();
			using var copyingWriter = new YEncWriter (output);
			Assert.ThrowsAsync (expected, async () => await Write (copyingWriter, header, readError, doAsync));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await Write (copyingWriter, header, empty, doAsync));
		}

		[Test]
		public void TestStreamFailures ([Values] bool cancel)
		{
			StreamFailures (cancel, false).GetAwaiter ().GetResult ();
		}

		[Test]
		public Task TestStreamFailuresAsync ([Values] bool cancel)
		{
			return StreamFailures (cancel, true);
		}
	}
}
