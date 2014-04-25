﻿//
// AssemblyInfo.cs
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle ("MimeKit")]
[assembly: AssemblyDescription ("A complete MIME library with support for S/MIME, PGP and Unix mbox spools.")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("Xamarin Inc.")]
[assembly: AssemblyProduct ("MimeKit")]
[assembly: AssemblyCopyright ("Copyright © 2013-2014 Xamarin Inc. (www.xamarin.com)")]
[assembly: AssemblyTrademark ("Xamarin Inc.")]
[assembly: AssemblyCulture ("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible (false)]

#if !PORTABLE
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid ("2fe79b66-d107-45da-9493-175f59c4a53c")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Micro Version
//      Build Number
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//
// Note: AssemblyVersion is what the CLR matches against at runtime, so be careful
// about updating it. The AssemblyFileVersion is the official release version while
// the AssemblyInformationalVersion is just used as a display version.
//
// Based on my current understanding, AssemblyVersion is essentially the "API Version"
// and so should only be updated when the API changes. The other 2 Version attributes
// represent the "Release Version".
//
// Making releases:
//
// If any classes, methods, or enum values have been added, bump the Micro Version
//    in all version attributes and set the Build Number back to 0.
//
// If there have only been bug fixes, bump the Micro Version and/or the Build Number
//    in the AssemblyFileVersion attribute.
[assembly: AssemblyInformationalVersion ("0.32.0")]
[assembly: AssemblyFileVersion ("0.32.0.0")]
[assembly: AssemblyVersion ("0.32.0.0")]
