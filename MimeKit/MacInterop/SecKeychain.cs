//
// SecKeychain.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;

using MimeKit.Cryptography;

namespace MimeKit.MacInterop {
	class SecKeychain : CFObject
	{
		const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";

		/// <summary>
		/// The default login keychain.
		/// </summary>
		public static readonly SecKeychain Default = GetDefault ();

		bool disposed;

		SecKeychain (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		SecKeychain (IntPtr handle) : base (handle, false)
		{
		}

		#region Managing Certificates

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecCertificateAddToKeychain (IntPtr certificate, IntPtr keychain);

		[DllImport (SecurityLibrary)]
		static extern IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr data);

		[DllImport (SecurityLibrary)]
		static extern IntPtr SecCertificateCopyData (IntPtr certificate);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecCertificateCopyCommonName (IntPtr certificate, out IntPtr commonName);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecPKCS12Import (IntPtr pkcs12DataRef, IntPtr options, ref IntPtr items);

		#endregion

		#region Managing Identities

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecIdentityCopyCertificate (IntPtr identityRef, out IntPtr certificateRef);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecIdentityCopyPrivateKey (IntPtr identityRef, out IntPtr privateKeyRef);

		// WARNING: deprecated in Mac OS X 10.7
		[DllImport (SecurityLibrary)]
		static extern OSStatus SecIdentitySearchCreate (IntPtr keychainOrArray, CssmKeyUse keyUsage, out IntPtr searchRef);

		// WARNING: deprecated in Mac OS X 10.7
		[DllImport (SecurityLibrary)]
		static extern OSStatus SecIdentitySearchCopyNext (IntPtr searchRef, out IntPtr identity);

		// Note: SecIdentitySearch* has been replaced with SecItemCopyMatching

		//[DllImport (SecurityLib)]
		//OSStatus SecItemCopyMatching (CFDictionaryRef query, CFTypeRef *result);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecItemImport (IntPtr importedData, IntPtr fileName, ref SecExternalFormat format, IntPtr type, SecItemImportExportFlags flags, IntPtr keyParams, IntPtr keychain, ref IntPtr items);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecItemExport (IntPtr itemRef, SecExternalFormat format, SecItemImportExportFlags flags, IntPtr keyParams, out IntPtr exportedData);

		#endregion

		#region Getting Information About Security Result Codes

		[DllImport (SecurityLibrary)]
		static extern IntPtr SecCopyErrorMessageString (OSStatus status, IntPtr reserved);

		#endregion

		#region Managing Keychains

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainCopyDefault (ref IntPtr keychain);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainCreate (string path, uint passwordLength, byte[] password, bool promptUser, IntPtr initialAccess, ref IntPtr keychain);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainOpen (string path, ref IntPtr keychain);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainDelete (IntPtr keychain);

		static SecKeychain GetDefault ()
		{
			IntPtr handle = IntPtr.Zero;

			if (SecKeychainCopyDefault (ref handle) == OSStatus.Ok)
				return new SecKeychain (handle, true);

			return null;
		}

		/// <summary>
		/// Create a keychain at the specified path with the specified password.
		/// </summary>
		/// <param name="path">The path to the keychain.</param>
		/// <param name="password">The password for unlocking the keychain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="path"/> was <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> was <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.Exception">
		/// An unknown error creating the keychain occurred.
		/// </exception>
		public static SecKeychain Create (string path, string password)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (password == null)
				throw new ArgumentNullException ("password");

			var passwd = Encoding.UTF8.GetBytes (password);
			var handle = IntPtr.Zero;

			var status = SecKeychainCreate (path, (uint) passwd.Length, passwd, false, IntPtr.Zero, ref handle);
			if (status != OSStatus.Ok)
				throw new Exception (GetError (status));

			return new SecKeychain (handle);
		}

		/// <summary>
		/// Opens the keychain at the specified path.
		/// </summary>
		/// <param name="path">The path to the keychain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="path"/> was <c>null</c>.
		/// </exception>
		/// <exception cref="System.Exception">
		/// An unknown error opening the keychain occurred.
		/// </exception>
		public static SecKeychain Open (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			var handle = IntPtr.Zero;

			var status = SecKeychainOpen (path, ref handle);
			if (status != OSStatus.Ok)
				throw new Exception (GetError (status));

			return new SecKeychain (handle);
		}

		/// <summary>
		/// Deletes the specified keychain.
		/// </summary>
		/// <param name="keychain">Keychain.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="keychain"/> was <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="keychain"/> has been disposed.
		/// </exception>
		/// <exception cref="System.Exception">
		/// An unknown error deleting the keychain occurred.
		/// </exception>
		public static void Delete (SecKeychain keychain)
		{
			if (keychain == null)
				throw new ArgumentNullException ("keychain");

			if (keychain.disposed)
				throw new ObjectDisposedException ("SecKeychain");

			if (keychain.Handle == IntPtr.Zero)
				throw new InvalidOperationException ();

			var status = SecKeychainDelete (keychain.Handle);
			if (status != OSStatus.Ok)
				throw new Exception (GetError (status));

			keychain.Dispose ();
		}

		#endregion

		#region Searching for Keychain Items

		[DllImport (SecurityLibrary)]
		static extern unsafe OSStatus SecKeychainSearchCreateFromAttributes (IntPtr keychainOrArray, SecItemClass itemClass, SecKeychainAttributeList *attrList, out IntPtr searchRef);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainSearchCopyNext (IntPtr searchRef, out IntPtr itemRef);

		#endregion

		#region Creating and Deleting Keychain Items

		[DllImport (SecurityLibrary)]
		static extern unsafe OSStatus SecKeychainItemCreateFromContent (SecItemClass itemClass, SecKeychainAttributeList *attrList,
			uint passwordLength, byte[] password, IntPtr keychain,
			IntPtr initialAccess, ref IntPtr itemRef);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainItemDelete (IntPtr itemRef);

		#endregion

		#region Managing Keychain Items

		[DllImport (SecurityLibrary)]
		static extern unsafe OSStatus SecKeychainItemModifyAttributesAndData (IntPtr itemRef, SecKeychainAttributeList *attrList, uint length, byte [] data);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainItemCopyContent (IntPtr itemRef, ref SecItemClass itemClass, IntPtr attrList, ref uint length, ref IntPtr data);

		[DllImport (SecurityLibrary)]
		static extern OSStatus SecKeychainItemFreeContent (IntPtr attrList, IntPtr data);

		#endregion

		static string GetError (OSStatus status)
		{
			CFString str = null;

			try {
				str = new CFString (SecCopyErrorMessageString (status, IntPtr.Zero), true);
				return str.ToString ();
			} catch {
				return status.ToString ();
			} finally {
				if (str != null)
					str.Dispose ();
			}
		}

		/// <summary>
		/// Gets a list of all certificates suitable for the given key usage.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="keyUsage">The key usage.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The keychain has been disposed.
		/// </exception>
		public IList<X509Certificate> GetCertificates (CssmKeyUse keyUsage)
		{
			if (disposed)
				throw new ObjectDisposedException ("SecKeychain");

			var parser = new X509CertificateParser ();
			var certs = new List<X509Certificate> ();
			IntPtr searchRef, itemRef, certRef;
			OSStatus status;

			status = SecIdentitySearchCreate (Handle, keyUsage, out searchRef);
			if (status != OSStatus.Ok)
				return certs;

			while (SecIdentitySearchCopyNext (searchRef, out itemRef) == OSStatus.Ok) {
				if (SecIdentityCopyCertificate (itemRef, out certRef) == OSStatus.Ok) {
					using (var data = new CFData (SecCertificateCopyData (certRef), true)) {
						var rawData = data.GetBuffer ();

						try {
							certs.Add (parser.ReadCertificate (rawData));
						} catch (CertificateException ex) {
							Debug.WriteLine ("Failed to parse X509 certificate from keychain: {0}", ex);
						}
					}
				}

				CFRelease (itemRef);
			}

			CFRelease (searchRef);

			return certs;
		}

		public IList<CmsSigner> GetAllCmsSigners ()
		{
			if (disposed)
				throw new ObjectDisposedException ("SecKeychain");

			var signers = new List<CmsSigner> ();
			IntPtr searchRef, itemRef, dataRef;
			OSStatus status;

			status = SecIdentitySearchCreate (Handle, CssmKeyUse.Sign, out searchRef);
			if (status != OSStatus.Ok)
				return signers;

			while (SecIdentitySearchCopyNext (searchRef, out itemRef) == OSStatus.Ok) {
				if (SecItemExport (itemRef, SecExternalFormat.PKCS12, SecItemImportExportFlags.None, IntPtr.Zero, out dataRef) == OSStatus.Ok) {
					var data = new CFData (dataRef, true);
					var rawData = data.GetBuffer ();
					data.Dispose ();

					try {
						using (var memory = new MemoryStream (rawData, false)) {
							var pkcs12 = new Pkcs12Store (memory, new char[0]);

							foreach (string alias in pkcs12.Aliases) {
								if (!pkcs12.IsKeyEntry (alias))
									continue;

								var chain = pkcs12.GetCertificateChain (alias);
								var entry = pkcs12.GetKey (alias);

								signers.Add (new CmsSigner (chain, entry.Key));
							}
						}
					} catch (Exception ex) {
						Debug.WriteLine ("Failed to decode keychain pkcs12 data: {0}", ex);
					}
				}

				CFRelease (itemRef);
			}

			CFRelease (searchRef);

			return signers;
		}

		public bool Add (AsymmetricKeyParameter key)
		{
			// FIXME: how do we convert an AsymmetricKeyParameter into something usable by MacOS?
			throw new NotImplementedException ();
		}

		public bool Add (X509Certificate certificate)
		{
			using (var cert = SecCertificate.Create (certificate.GetEncoded ())) {
				var status = SecCertificateAddToKeychain (cert.Handle, Handle);
				return status == OSStatus.Ok || status == OSStatus.DuplicateItem;
			}
		}

		public unsafe bool Contains (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (disposed)
				throw new ObjectDisposedException ("SecKeychain");

			// Note: we don't have to use an alias attribute, it's just that it might be faster to use it (fewer certificates we have to compare raw data for)
			byte[] alias = Encoding.UTF8.GetBytes (certificate.GetCommonName ());
			IntPtr searchRef, itemRef;
			bool found = false;
			byte[] certData;
			OSStatus status;

			fixed (byte* aliasPtr = alias) {
				SecKeychainAttribute* attrs = stackalloc SecKeychainAttribute [1];
				int n = 0;

				if (alias != null)
					attrs[n++] = new SecKeychainAttribute (SecItemAttr.Alias, (uint) alias.Length, (IntPtr) aliasPtr);

				SecKeychainAttributeList attrList = new SecKeychainAttributeList (n, (IntPtr) attrs);

				status = SecKeychainSearchCreateFromAttributes (Handle, SecItemClass.Certificate, &attrList, out searchRef);
				if (status != OSStatus.Ok)
					throw new Exception ("Could not enumerate certificates from the keychain. Error:\n" + GetError (status));

				certData = certificate.GetEncoded ();

				while (!found && SecKeychainSearchCopyNext (searchRef, out itemRef) == OSStatus.Ok) {
					SecItemClass itemClass = 0;
					IntPtr data = IntPtr.Zero;
					uint length = 0;

					status = SecKeychainItemCopyContent (itemRef, ref itemClass, IntPtr.Zero, ref length, ref data);
					if (status == OSStatus.Ok) {
						if (certData.Length == (int) length) {
							byte[] rawData = new byte[(int) length];

							Marshal.Copy (data, rawData, 0, (int) length);

							found = true;
							for (int i = 0; i < rawData.Length; i++) {
								if (rawData[i] != certData[i]) {
									found = false;
									break;
								}
							}
						}

						SecKeychainItemFreeContent (IntPtr.Zero, data);
					}

					CFRelease (itemRef);
				}

				CFRelease (searchRef);
			}

			return found;
		}

//		public void ImportPkcs12 (byte[] rawData, string password)
//		{
//			if (rawData == null)
//				throw new ArgumentNullException ("rawData");
//
//			if (password == null)
//				throw new ArgumentNullException ("password");
//
//			if (disposed)
//				throw new ObjectDisposedException ("SecKeychain");
//
//			using (var data = new CFData (rawData)) {
//				var options = IntPtr.Zero;
//				var items = IntPtr.Zero;
//
//				var status = SecPKCS12Import (data.Handle, options, ref items);
//				CFRelease (options);
//				CFRelease (items);
//			}
//		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			disposed = true;
		}
	}
}
