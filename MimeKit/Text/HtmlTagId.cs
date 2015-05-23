﻿//
// HtmlTagId.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace MimeKit.Text {
	/// <summary>
	/// HTML tag identifiers.
	/// </summary>
	/// <remarks>
	/// HTML tag identifiers.
	/// </remarks>
	public enum HtmlTagId {
		/// <summary>
		/// An unknown HTML tag identifier.
		/// </summary>
		Unknown,

		/// <summary>
		/// The HTML &lt;a&gt; tag.
		/// </summary>
		A,

		/// <summary>
		/// The HTML &lt;abbr&gt; tag.
		/// </summary>
		Abbr,

		/// <summary>
		/// The HTML &lt;acronym&gt; tag.
		/// </summary>
		Acronym,

		/// <summary>
		/// The HTML &lt;address&gt; tag.
		/// </summary>
		Address,

		/// <summary>
		/// The HTML &lt;applet&gt; tag.
		/// </summary>
		Applet,

		/// <summary>
		/// The HTML &lt;area&gt; tag.
		/// </summary>
		Area,

		/// <summary>
		/// The HTML &lt;article&gt; tag.
		/// </summary>
		Article,

		/// <summary>
		/// The HTML &lt;aside&gt; tag.
		/// </summary>
		Aside,

		/// <summary>
		/// The HTML &lt;audio&gt; tag.
		/// </summary>
		Audio,

		/// <summary>
		/// The HTML &lt;b&gt; tag.
		/// </summary>
		B,

		/// <summary>
		/// The HTML &lt;base&gt; tag.
		/// </summary>
		Base,

		/// <summary>
		/// The HTML &lt;basefont&gt; tag.
		/// </summary>
		BaseFont,

		/// <summary>
		/// The HTML &lt;bdi&gt; tag.
		/// </summary>
		Bdi,

		/// <summary>
		/// The HTML &lt;bdo&gt; tag.
		/// </summary>
		Bdo,

		/// <summary>
		/// The HTML &lt;bgsound&gt; tag.
		/// </summary>
		BGSound,

		/// <summary>
		/// The HTML &lt;big&gt; tag.
		/// </summary>
		Big,

		/// <summary>
		/// The HTML &lt;blink&gt; tag.
		/// </summary>
		Blink,

		/// <summary>
		/// The HTML &lt;blockquote&gt; tag.
		/// </summary>
		BlockQuote,

		/// <summary>
		/// The HTML &lt;body&gt; tag.
		/// </summary>
		Body,

		/// <summary>
		/// The HTML &lt;br&gt; tag.
		/// </summary>
		Br,

		/// <summary>
		/// The HTML &lt;button&gt; tag.
		/// </summary>
		Button,

		/// <summary>
		/// The HTML &lt;canvas&gt; tag.
		/// </summary>
		Canvas,

		/// <summary>
		/// The HTML &lt;caption&gt; tag.
		/// </summary>
		Caption,

		/// <summary>
		/// The HTML &lt;center&gt; tag.
		/// </summary>
		Center,

		/// <summary>
		/// The HTML &lt;cite&gt; tag.
		/// </summary>
		Cite,

		/// <summary>
		/// The HTML &lt;code&gt; tag.
		/// </summary>
		Code,

		/// <summary>
		/// The HTML &lt;col&gt; tag.
		/// </summary>
		Col,

		/// <summary>
		/// The HTML &lt;colgroup&gt; tag.
		/// </summary>
		ColGroup,

		/// <summary>
		/// The HTML comment tag.
		/// </summary>
		Comment,

		/// <summary>
		/// The HTML &lt;datalist&gt; tag.
		/// </summary>
		DataList,

		/// <summary>
		/// The HTML &lt;dd&gt; tag.
		/// </summary>
		DD,

		/// <summary>
		/// The HTML &lt;del&gt; tag.
		/// </summary>
		Del,

		/// <summary>
		/// The HTML &lt;details&gt; tag.
		/// </summary>
		Details,

		/// <summary>
		/// The HTML &lt;dfn&gt; tag.
		/// </summary>
		Dfn,

		/// <summary>
		/// The HTML &lt;dialog&gt; tag.
		/// </summary>
		Dialog,

		/// <summary>
		/// The HTML &lt;dir&gt; tag.
		/// </summary>
		Dir,

		/// <summary>
		/// The HTML &lt;div&gt; tag.
		/// </summary>
		Div,

		/// <summary>
		/// The HTML &lt;dl&gt; tag.
		/// </summary>
		DL,

		/// <summary>
		/// The HTML &lt;dt&gt; tag.
		/// </summary>
		DT,

		/// <summary>
		/// The HTML &lt;em&gt; tag.
		/// </summary>
		EM,

		/// <summary>
		/// The HTML &lt;embed&gt; tag.
		/// </summary>
		Embed,

		/// <summary>
		/// The HTML &lt;fieldset&gt; tag.
		/// </summary>
		FieldSet,

		/// <summary>
		/// The HTML &lt;figcaption&gt; tag.
		/// </summary>
		FigCaption,

		/// <summary>
		/// The HTML &lt;figure&gt; tag.
		/// </summary>
		Figure,

		/// <summary>
		/// The HTML &lt;font&gt; tag.
		/// </summary>
		Font,

		/// <summary>
		/// The HTML &lt;footer&gt; tag.
		/// </summary>
		Footer,

		/// <summary>
		/// The HTML &lt;form&gt; tag.
		/// </summary>
		Form,

		/// <summary>
		/// The HTML &lt;frame&gt; tag.
		/// </summary>
		Frame,

		/// <summary>
		/// The HTML &lt;frameset&gt; tag.
		/// </summary>
		FrameSet,

		/// <summary>
		/// The HTML &lt;h1&gt; tag.
		/// </summary>
		H1,

		/// <summary>
		/// The HTML &lt;h2&gt; tag.
		/// </summary>
		H2,

		/// <summary>
		/// The HTML &lt;h3&gt; tag.
		/// </summary>
		H3,

		/// <summary>
		/// The HTML &lt;h4&gt; tag.
		/// </summary>
		H4,

		/// <summary>
		/// The HTML &lt;h5&gt; tag.
		/// </summary>
		H5,

		/// <summary>
		/// The HTML &lt;h6&gt; tag.
		/// </summary>
		H6,

		/// <summary>
		/// The HTML &lt;head&gt; tag.
		/// </summary>
		Head,

		/// <summary>
		/// The HTML &lt;header&gt; tag.
		/// </summary>
		Header,

		/// <summary>
		/// The HTML &lt;hr&gt; tag.
		/// </summary>
		HR,

		/// <summary>
		/// The HTML &lt;html&gt; tag.
		/// </summary>
		Html,

		/// <summary>
		/// The HTML &lt;i&gt; tag.
		/// </summary>
		I,

		/// <summary>
		/// The HTML &lt;iframe&gt; tag.
		/// </summary>
		Iframe,

		/// <summary>
		/// The HTML &lt;image&gt; tag.
		/// </summary>
		[HtmlTagName ("img")]
		Image,

		/// <summary>
		/// The HTML &lt;input&gt; tag.
		/// </summary>
		Input,

		/// <summary>
		/// The HTML &lt;ins&gt; tag.
		/// </summary>
		Ins,

		/// <summary>
		/// The HTML &lt;isindex&gt; tag.
		/// </summary>
		IsIndex,

		/// <summary>
		/// The HTML &lt;kbd&gt; tag.
		/// </summary>
		Kbd,

		/// <summary>
		/// The HTML &lt;label&gt; tag.
		/// </summary>
		Label,

		/// <summary>
		/// The HTML &lt;legend&gt; tag.
		/// </summary>
		Legend,

		/// <summary>
		/// The HTML &lt;li&gt; tag.
		/// </summary>
		LI,

		/// <summary>
		/// The HTML &lt;link&gt; tag.
		/// </summary>
		Link,

		/// <summary>
		/// The HTML &lt;listing&gt; tag.
		/// </summary>
		Listing,

		/// <summary>
		/// The HTML &lt;main&gt; tag.
		/// </summary>
		Main,

		/// <summary>
		/// The HTML &lt;map&gt; tag.
		/// </summary>
		Map,

		/// <summary>
		/// The HTML &lt;mark&gt; tag.
		/// </summary>
		Mark,

		/// <summary>
		/// The HTML &lt;marquee&gt; tag.
		/// </summary>
		Marquee,

		/// <summary>
		/// The HTML &lt;menu&gt; tag.
		/// </summary>
		Menu,

		/// <summary>
		/// The HTML &lt;menuitem&gt; tag.
		/// </summary>
		MenuItem,

		/// <summary>
		/// The HTML &lt;meta&gt; tag.
		/// </summary>
		Meta,

		/// <summary>
		/// The HTML &lt;meter&gt; tag.
		/// </summary>
		Meter,

		/// <summary>
		/// The HTML &lt;nav&gt; tag.
		/// </summary>
		Nav,

		/// <summary>
		/// The HTML &lt;nextid&gt; tag.
		/// </summary>
		NextId,

		/// <summary>
		/// The HTML &lt;nobr&gt; tag.
		/// </summary>
		NoBR,

		/// <summary>
		/// The HTML &lt;noembed&gt; tag.
		/// </summary>
		NoEmbed,

		/// <summary>
		/// The HTML &lt;noframes&gt; tag.
		/// </summary>
		NoFrames,

		/// <summary>
		/// The HTML &lt;noscript&gt; tag.
		/// </summary>
		NoScript,

		/// <summary>
		/// The HTML &lt;object&gt; tag.
		/// </summary>
		Object,

		/// <summary>
		/// The HTML &lt;ol&gt; tag.
		/// </summary>
		OL,

		/// <summary>
		/// The HTML &lt;optgroup&gt; tag.
		/// </summary>
		OptGroup,

		/// <summary>
		/// The HTML &lt;option&gt; tag.
		/// </summary>
		Option,

		/// <summary>
		/// The HTML &lt;output&gt; tag.
		/// </summary>
		Output,

		/// <summary>
		/// The HTML &lt;p&gt; tag.
		/// </summary>
		P,

		/// <summary>
		/// The HTML &lt;param&gt; tag.
		/// </summary>
		Param,

		/// <summary>
		/// The HTML &lt;plaintext&gt; tag.
		/// </summary>
		PlainText,

		/// <summary>
		/// The HTML &lt;pre&gt; tag.
		/// </summary>
		Pre,

		/// <summary>
		/// The HTML &lt;progress&gt; tag.
		/// </summary>
		Progress,

		/// <summary>
		/// The HTML &lt;q&gt; tag.
		/// </summary>
		Q,

		/// <summary>
		/// The HTML &lt;rp&gt; tag.
		/// </summary>
		RP,

		/// <summary>
		/// The HTML &lt;rt&gt; tag.
		/// </summary>
		RT,

		/// <summary>
		/// The HTML &lt;ruby&gt; tag.
		/// </summary>
		Ruby,

		/// <summary>
		/// The HTML &lt;s&gt; tag.
		/// </summary>
		S,

		/// <summary>
		/// The HTML &lt;samp&gt; tag.
		/// </summary>
		Samp,

		/// <summary>
		/// The HTML &lt;script&gt; tag.
		/// </summary>
		Script,

		/// <summary>
		/// The HTML &lt;section&gt; tag.
		/// </summary>
		Section,

		/// <summary>
		/// The HTML &lt;select&gt; tag.
		/// </summary>
		Select,

		/// <summary>
		/// The HTML &lt;small&gt; tag.
		/// </summary>
		Small,

		/// <summary>
		/// The HTML &lt;source&gt; tag.
		/// </summary>
		Source,

		/// <summary>
		/// The HTML &lt;span&gt; tag.
		/// </summary>
		Span,

		/// <summary>
		/// The HTML &lt;strike&gt; tag.
		/// </summary>
		Strike,

		/// <summary>
		/// The HTML &lt;strong&gt; tag.
		/// </summary>
		Strong,

		/// <summary>
		/// The HTML &lt;style&gt; tag.
		/// </summary>
		Style,

		/// <summary>
		/// The HTML &lt;sub&gt; tag.
		/// </summary>
		Sub,

		/// <summary>
		/// The HTML &lt;summary&gt; tag.
		/// </summary>
		Summary,

		/// <summary>
		/// The HTML &lt;sup&gt; tag.
		/// </summary>
		Sup,

		/// <summary>
		/// The HTML &lt;table&gt; tag.
		/// </summary>
		Table,

		/// <summary>
		/// The HTML &lt;tbody&gt; tag.
		/// </summary>
		TBody,

		/// <summary>
		/// The HTML &lt;td&gt; tag.
		/// </summary>
		TD,

		/// <summary>
		/// The HTML &lt;textarea&gt; tag.
		/// </summary>
		TextArea,

		/// <summary>
		/// The HTML &lt;tfoot&gt; tag.
		/// </summary>
		Tfoot,

		/// <summary>
		/// The HTML &lt;th&gt; tag.
		/// </summary>
		TH,

		/// <summary>
		/// The HTML &lt;thread&gt; tag.
		/// </summary>
		Thread,

		/// <summary>
		/// The HTML &lt;time&gt; tag.
		/// </summary>
		Time,

		/// <summary>
		/// The HTML &lt;title&gt; tag.
		/// </summary>
		Title,

		/// <summary>
		/// The HTML &lt;tr&gt; tag.
		/// </summary>
		TR,

		/// <summary>
		/// The HTML &lt;track&gt; tag.
		/// </summary>
		Track,

		/// <summary>
		/// The HTML &lt;tt&gt; tag.
		/// </summary>
		TT,

		/// <summary>
		/// The HTML &lt;u&gt; tag.
		/// </summary>
		U,

		/// <summary>
		/// The HTML &lt;ul&gt; tag.
		/// </summary>
		UL,

		/// <summary>
		/// The HTML &lt;var&gt; tag.
		/// </summary>
		Var,

		/// <summary>
		/// The HTML &lt;video&gt; tag.
		/// </summary>
		Video,

		/// <summary>
		/// The HTML &lt;wbr&gt; tag.
		/// </summary>
		Wbr,

		/// <summary>
		/// The HTML &lt;xml&gt; tag.
		/// </summary>
		Xml,

		/// <summary>
		/// The HTML &lt;xmp&gt; tag.
		/// </summary>
		Xmp,
	}

	[AttributeUsage (AttributeTargets.Field)]
	class HtmlTagNameAttribute : Attribute {
		public HtmlTagNameAttribute (string name)
		{
			Name = name;
		}

		public string Name {
			get; protected set;
		}
	}

	/// <summary>
	/// <see cref="HtmlTagId"/> extension methods.
	/// </summary>
	/// <remarks>
	/// <see cref="HtmlTagId"/> extension methods.
	/// </remarks>
	static class HtmlTagIdExtensions
	{
		static readonly Dictionary<string, HtmlTagId> dict;

		static HtmlTagIdExtensions ()
		{
			var values = (HtmlTagId[]) Enum.GetValues (typeof (HtmlTagId));

			dict = new Dictionary<string, HtmlTagId> (values.Length - 1, StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < values.Length - 1; i++)
				dict.Add (values[i].ToHtmlTagName (), values[i]);
		}

		/// <summary>
		/// Converts the enum value into the equivalent tag name.
		/// </summary>
		/// <remarks>
		/// Converts the enum value into the equivalent tag name.
		/// </remarks>
		/// <returns>The tag name.</returns>
		/// <param name="value">The enum value.</param>
		public static string ToHtmlTagName (this HtmlTagId value)
		{
			if (value == HtmlTagId.Comment)
				return "!";

			var name = value.ToString ();

#if PORTABLE
			var field = typeof (HtmlTagId).GetTypeInfo ().GetDeclaredField (name);
			var attrs = field.GetCustomAttributes (typeof (HtmlTagNameAttribute), false).ToArray ();
#else
			var field = typeof (HtmlTagId).GetField (name);
			var attrs = field.GetCustomAttributes (typeof (HtmlTagNameAttribute), false);
#endif

			if (attrs != null && attrs.Length == 1)
				return ((HtmlTagNameAttribute) attrs[0]).Name;

			return name.ToLowerInvariant ();
		}

		/// <summary>
		/// Converts the tag name into the equivalent tag id.
		/// </summary>
		/// <remarks>
		/// Converts the tag name into the equivalent tag id.
		/// </remarks>
		/// <returns>The tag id.</returns>
		/// <param name="name">The tag name.</param>
		public static HtmlTagId ToHtmlTagId (this string name)
		{
			HtmlTagId value;

			if (string.IsNullOrEmpty (name))
				return HtmlTagId.Unknown;

			if (name[0] == '!')
				return HtmlTagId.Comment;

			if (!dict.TryGetValue (name, out value)) {
				#if !PORTABLE
				Console.WriteLine ("unknown html tag: {0}", name);
				#endif
				return HtmlTagId.Unknown;
			}

			return value;
		}
	}
}
