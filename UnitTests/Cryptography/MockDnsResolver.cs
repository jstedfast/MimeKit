//
// MockDnsResolver.cs
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

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	class MockDnsResolver : IDnsResolver
	{
		readonly Dictionary<string, List<string>> records = new Dictionary<string, List<string>> (StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, DnsQueryStatus> failures = new Dictionary<string, DnsQueryStatus> (StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, Exception> exceptions = new Dictionary<string, Exception> (StringComparer.OrdinalIgnoreCase);

		public List<string> Queries { get; } = new List<string> ();

		public void Add (string name, string txt)
		{
			if (!records.TryGetValue (name, out var list))
				records.Add (name, list = new List<string> ());

			list.Add (txt);
		}

		public void AddFailure (string name, DnsQueryStatus status)
		{
			failures[name] = status;
		}

		public void AddException (string name, Exception ex)
		{
			exceptions[name] = ex;
		}

		public DnsTxtResponse QueryTxt (string domain, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			Queries.Add (domain);

			if (exceptions.TryGetValue (domain, out var ex))
				throw ex;

			if (failures.TryGetValue (domain, out var status))
				return new DnsTxtResponse (status);

			if (records.TryGetValue (domain, out var list))
				return new DnsTxtResponse (list);

			return new DnsTxtResponse (DnsQueryStatus.NonExistentDomain);
		}

		public Task<DnsTxtResponse> QueryTxtAsync (string domain, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (QueryTxt (domain, cancellationToken));
		}
	}
}
