using System;
using System.IO;

namespace Benchmarks
{
	class LoopedInputStream : Stream
	{
		readonly int iterationCount;
		readonly Stream innerStream;
		readonly long length;
		int iteration;
		long position;

		public LoopedInputStream (Stream innerStream, int iterationCount)
		{
			this.iterationCount = iterationCount;
			this.innerStream = innerStream;

			length = innerStream.Length;
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length {
			get {
				return iterationCount * length;
			}
		}

		public override long Position {
			get {
				return (iteration * length) + position;
			}
			set {
				Seek (value, SeekOrigin.Begin);
			}
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int nread = 0;

			do {
				int n;

				if ((n = innerStream.Read (buffer, offset + nread, count - nread)) > 0) {
					position += n;
					nread += n;
				}

				if (position == length) {
					if (iteration < iterationCount) {
						innerStream.Position = position = 0;
						iteration++;
					} else {
						break;
					}
				}
			} while (nread < count);

			return nread;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			switch (origin) {
			case SeekOrigin.Begin:
				position = offset % length;
				iteration = (int) (offset / length);
				innerStream.Seek (position, SeekOrigin.Begin);
				break;
			case SeekOrigin.Current:
				offset = Position + offset;
				if (offset < 0 || offset > Length)
					throw new IOException ();

				goto case SeekOrigin.Begin;
			case SeekOrigin.End:
				offset = Length + offset;
				if (offset < 0 || offset > Length)
					throw new IOException ();

				goto case SeekOrigin.Begin;
			}

			return Position;
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}
	}
}
