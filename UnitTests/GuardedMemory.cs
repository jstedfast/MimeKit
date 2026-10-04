//
// GuardedMemory.cs
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

using System.Runtime.InteropServices;

namespace UnitTests {
	// Allocates memory surrounded by inaccessible guard pages so that any out-of-bounds read or write faults immediately.
	sealed unsafe class GuardedMemory : IDisposable
	{
		const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_RELEASE = 0x8000;
		const uint PAGE_NOACCESS = 0x01, PAGE_READWRITE = 0x04;
		const int PROT_NONE = 0, PROT_READ = 1, PROT_WRITE = 2, MAP_PRIVATE = 2;

		[DllImport ("kernel32.dll", SetLastError = true)]
		static extern void* VirtualAlloc (void* address, nuint size, uint allocationType, uint protect);

		[DllImport ("kernel32.dll", SetLastError = true)]
		static extern bool VirtualProtect (void* address, nuint size, uint newProtect, out uint oldProtect);

		[DllImport ("kernel32.dll", SetLastError = true)]
		static extern bool VirtualFree (void* address, nuint size, uint freeType);

		[DllImport ("libc", SetLastError = true)]
		static extern void* mmap (void* address, nuint length, int prot, int flags, int fd, nint offset);

		[DllImport ("libc", SetLastError = true)]
		static extern int mprotect (void* address, nuint length, int prot);

		[DllImport ("libc", SetLastError = true)]
		static extern int munmap (void* address, nuint length);

		readonly byte* allocation;
		readonly nuint allocationSize;

		// The usable region is [Start, Start + Length), and either Start immediately follows the leading guard page or
		// Start + Length immediately precedes the trailing guard page.
		public readonly byte* Start;
		public readonly int Length;

		public GuardedMemory (int length, bool alignEnd)
		{
			int pageSize = Environment.SystemPageSize;
			int dataPages = Math.Max (1, (length + pageSize - 1) / pageSize);

			allocationSize = (nuint) ((dataPages + 2) * pageSize);

			if (OperatingSystem.IsWindows ()) {
				allocation = (byte*) VirtualAlloc (null, allocationSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
				Assert.That ((IntPtr) allocation, Is.Not.EqualTo (IntPtr.Zero), "VirtualAlloc");
				Assert.That (VirtualProtect (allocation, (nuint) pageSize, PAGE_NOACCESS, out _), Is.True, "VirtualProtect");
				Assert.That (VirtualProtect (allocation + (dataPages + 1) * pageSize, (nuint) pageSize, PAGE_NOACCESS, out _), Is.True, "VirtualProtect");
			} else {
				int mapAnonymous = OperatingSystem.IsMacOS () ? 0x1000 : 0x20;

				allocation = (byte*) mmap (null, allocationSize, PROT_READ | PROT_WRITE, MAP_PRIVATE | mapAnonymous, -1, 0);
				Assert.That ((IntPtr) allocation, Is.Not.EqualTo ((IntPtr) (-1)), "mmap");
				Assert.That (mprotect (allocation, (nuint) pageSize, PROT_NONE), Is.EqualTo (0), "mprotect");
				Assert.That (mprotect (allocation + (dataPages + 1) * pageSize, (nuint) pageSize, PROT_NONE), Is.EqualTo (0), "mprotect");
			}

			byte* data = allocation + pageSize;

			Start = alignEnd ? data + dataPages * pageSize - length : data;
			Length = length;
		}

		public void Dispose ()
		{
			if (OperatingSystem.IsWindows ())
				VirtualFree (allocation, 0, MEM_RELEASE);
			else
				munmap (allocation, allocationSize);
		}
	}
}
