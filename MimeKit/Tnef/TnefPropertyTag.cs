//
// TnefPropertyTag.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Tnef {
	public struct TnefPropertyTag
	{
		public static readonly TnefPropertyTag AbDefaultDir = new TnefPropertyTag (TnefPropertyId.AbDefaultDir, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbDefaultPab = new TnefPropertyTag (TnefPropertyId.AbDefaultPab, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbProviderId = new TnefPropertyTag (TnefPropertyId.AbProviderId, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbProviders = new TnefPropertyTag (TnefPropertyId.AbProviders, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag AbSearchPath = new TnefPropertyTag (TnefPropertyId.AbSearchPath, TnefPropertyType.Unspecified);
		public static readonly TnefPropertyTag AbSearchPathUpdate = new TnefPropertyTag (TnefPropertyId.AbSearchPathUpdate, TnefPropertyType.Binary);
		public static readonly TnefPropertyTag Access = new TnefPropertyTag (TnefPropertyId.Access, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AccessLevel = new TnefPropertyTag (TnefPropertyId.AccessLevel, TnefPropertyType.Long);
		public static readonly TnefPropertyTag AccountA = new TnefPropertyTag (TnefPropertyId.Account, TnefPropertyType.String8);
		public static readonly TnefPropertyTag AccountW = new TnefPropertyTag (TnefPropertyId.Account, TnefPropertyType.Unicode);


		public TnefPropertyId Id {
			get { throw new NotImplementedException (); }
		}

		public bool IsMultiValued {
			get { throw new NotImplementedException (); }
		}

		public bool IsNamed {
			get { throw new NotImplementedException (); }
		}

		public bool IsTnefTypeValid {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyType TnefType {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyType ValueTnefType {
			get { throw new NotImplementedException (); }
		}

		public TnefPropertyTag (int tag)
		{
			throw new NotImplementedException ();
		}

		public TnefPropertyTag (TnefPropertyId id, TnefPropertyType type)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator TnefPropertyTag (int tag)
		{
			return new TnefPropertyTag (tag);
		}

		public static implicit operator int (TnefPropertyTag tag)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			return ((int) this).GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (!(obj is TnefPropertyTag))
				return false;

			var tag = (TnefPropertyTag) obj;

			return tag.Id == Id && tag.TnefType == TnefType;
		}

		public override string ToString ()
		{
			return string.Format ("[TnefPropertyTag: Id={0}, IsMultiValues={1}, IsNamed={2}, IsTnefTypeValid={3}, TnefType={4}, ValueTnefType={5}]", Id, IsMultiValued, IsNamed, IsTnefTypeValid, TnefType, ValueTnefType);
		}

		public TnefPropertyTag ToUnicode ()
		{
			throw new NotImplementedException ();
		}
	}
}
