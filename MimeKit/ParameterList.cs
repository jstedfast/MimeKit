//
// ParameterList.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public sealed class ParameterList : ICollection<Parameter>, IList<Parameter>
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;

		Dictionary<string, Parameter> table;
		List<Parameter> parameters;

		public ParameterList ()
		{
			table = new Dictionary<string, Parameter> (icase);
			parameters = new List<Parameter> ();
		}

		public void Add (string name, string value)
		{
			Add (new Parameter (name, value));
		}

		public void Insert (int index, string name, string value)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			Insert (index, new Parameter (name, value));
		}

		public bool Remove (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Parameter param;
			if (!table.TryGetValue (name, out param))
				return false;

			return Remove (param);
		}

		public string this [string name] {
			get {
				if (name == null)
					throw new ArgumentNullException ("name");

				Parameter param;
				if (table.TryGetValue (name, out param))
					return param.Value;

				return null;
			}
			set {
				if (name == null)
					throw new ArgumentNullException ("name");

				if (value == null)
					throw new ArgumentNullException ("value");

				Parameter param;
				if (table.TryGetValue (name, out param)) {
					param.Value = value;
				} else {
					Add (name, value);
				}
			}
		}

		#region ICollection implementation

		public int Count {
			get { return parameters.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public void Add (Parameter param)
		{
			if (param == null)
				throw new ArgumentNullException ("param");

			if (table.ContainsKey (param.Name))
				throw new ArgumentException ("A parameter of that name already exists.");

			param.Changed += OnParamChanged;
			table.Add (param.Name, param);
			parameters.Add (param);

			OnChanged ();
		}

		public void Clear ()
		{
			foreach (var param in parameters)
				param.Changed -= OnParamChanged;

			parameters.Clear ();
			table.Clear ();

			OnChanged ();
		}

		public bool Contains (Parameter param)
		{
			return parameters.Contains (param);
		}

		public void CopyTo (Parameter[] array, int arrayIndex)
		{
			parameters.CopyTo (array, arrayIndex);
		}

		public bool Remove (Parameter param)
		{
			if (param == null)
				throw new ArgumentNullException ("param");

			if (!parameters.Remove (param))
				return false;

			param.Changed -= OnParamChanged;
			table.Remove (param.Name);

			OnChanged ();

			return true;
		}

		#endregion

		#region IList implementation

		public int IndexOf (Parameter param)
		{
			return parameters.IndexOf (param);
		}

		public void Insert (int index, Parameter param)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			if (param == null)
				throw new ArgumentNullException ("param");

			if (table.ContainsKey (param.Name))
				throw new ArgumentException ("A parameter of that name already exists.");

			parameters.Insert (index, param);
			table.Add (param.Name, param);
			param.Changed += OnParamChanged;

			OnChanged ();
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			var param = parameters[index];

			param.Changed -= OnParamChanged;
			parameters.RemoveAt (index);
			table.Remove (param.Name);

			OnChanged ();
		}

		public Parameter this [int index] {
			get {
				return parameters[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				var param = parameters[index];

				if (param == value)
					return;

				if (icase.Compare (param.Name, value.Name) == 0) {
					// replace the old param with the new one
					if (table[param.Name] == param)
						table[param.Name] = value;
				} else if (table.ContainsKey (value.Name)) {
					throw new ArgumentException ("A parameter of that name already exists.");
				} else {
					table.Add (value.Name, value);
					table.Remove (param.Name);
				}

				param.Changed -= OnParamChanged;
				value.Changed += OnParamChanged;
				parameters[index] = value;

				OnParamChanged ();
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<Parameter> GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		#endregion

		public event EventHandler Changed;

		void OnParamChanged (object sender, EventArgs args)
		{
			OnChanged ();
		}

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}
