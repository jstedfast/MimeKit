//
// HtmlAttributeId.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit.Text {
	/// <summary>
	/// HTML attribute identifiers.
	/// </summary>
	/// <remarks>
	/// HTML attribute identifiers.
	/// </remarks>
	public enum HtmlAttributeId {
		/// <summary>
		/// An unknown HTML attribute identifier.
		/// </summary>
		Unknown,

		/// <summary>
		/// The "abbr" attribute.
		/// </summary>
		Abbr,

		/// <summary>
		/// The "accept" attribute.
		/// </summary>
		Accept,

		/// <summary>
		/// The "accept-charset" attribute.
		/// </summary>
		AcceptCharset,

		/// <summary>
		/// The "accesskey" attribute.
		/// </summary>
		AccessKey,

		/// <summary>
		/// The "action" attribute.
		/// </summary>
		Action,

		/// <summary>
		/// The "align" attribute.
		/// </summary>
		Align,

		/// <summary>
		/// The "alink" attribute.
		/// </summary>
		Alink,

		/// <summary>
		/// The "alt" attribute.
		/// </summary>
		Alt,

		/// <summary>
		/// The "archive" attribute.
		/// </summary>
		Archive,

		/// <summary>
		/// The "axis" attribute.
		/// </summary>
		Axis,

		/// <summary>
		/// The "background" attribute.
		/// </summary>
		Background,

		/// <summary>
		/// The "bgcolor" attribute.
		/// </summary>
		BGColor,

		/// <summary>
		/// The "border" attribute.
		/// </summary>
		Border,

		/// <summary>
		/// The "cellpadding" attribute.
		/// </summary>
		CellPadding,

		/// <summary>
		/// The "cellspacing" attribute.
		/// </summary>
		CellSpacing,

		/// <summary>
		/// The "char" attribute.
		/// </summary>
		Char,

		/// <summary>
		/// The "charoff" attribute.
		/// </summary>
		CharOff,

		/// <summary>
		/// The "charset" attribute.
		/// </summary>
		Charset,

		/// <summary>
		/// The "checked" attribute.
		/// </summary>
		Checked,

		/// <summary>
		/// The "cite" attribute.
		/// </summary>
		Cite,

		/// <summary>
		/// The "class" attribute.
		/// </summary>
		Class,

		/// <summary>
		/// The "classid" attribute.
		/// </summary>
		ClassId,

		/// <summary>
		/// The "clear" attribute.
		/// </summary>
		Clear,

		/// <summary>
		/// The "code" attribute.
		/// </summary>
		Code,

		/// <summary>
		/// The "codebase" attribute.
		/// </summary>
		CodeBase,

		/// <summary>
		/// The "codetype" attribute.
		/// </summary>
		CodeType,

		/// <summary>
		/// The "color" attribute.
		/// </summary>
		Color,

		/// <summary>
		/// The "cols" attribute.
		/// </summary>
		Cols,

		/// <summary>
		/// The "colspan" attribute.
		/// </summary>
		ColSpan,

		/// <summary>
		/// The "compact" attribute.
		/// </summary>
		Compact,

		/// <summary>
		/// The "content" attribute.
		/// </summary>
		Content,

		/// <summary>
		/// The "coords" attribute.
		/// </summary>
		Coords,

		/// <summary>
		/// The "data" attribute.
		/// </summary>
		Data,

		/// <summary>
		/// The "datetime" attribute.
		/// </summary>
		DateTime,

		/// <summary>
		/// The "declare" attribute.
		/// </summary>
		Declare,

		/// <summary>
		/// The "defer" attribute.
		/// </summary>
		Defer,

		/// <summary>
		/// The "dir" attribute.
		/// </summary>
		Dir,

		/// <summary>
		/// The "disabled" attribute.
		/// </summary>
		Disabled,

		/// <summary>
		/// The "dynsrc" attribute.
		/// </summary>
		DynSrc,

		/// <summary>
		/// The "enctype" attribute.
		/// </summary>
		EncType,

		/// <summary>
		/// The "face" attribute.
		/// </summary>
		Face,

		/// <summary>
		/// The "for" attribute.
		/// </summary>
		For,

		/// <summary>
		/// The "frame" attribute.
		/// </summary>
		Frame,

		/// <summary>
		/// The "frameborder" attribute.
		/// </summary>
		FrameBorder,

		/// <summary>
		/// The "headers" attribute.
		/// </summary>
		Headers,

		/// <summary>
		/// The "height" attribute.
		/// </summary>
		Height,

		/// <summary>
		/// The "href" attribute.
		/// </summary>
		Href,

		/// <summary>
		/// The "hreflang" attribute.
		/// </summary>
		HrefLang,

		/// <summary>
		/// The "hspace" attribute.
		/// </summary>
		Hspace,

		/// <summary>
		/// The "http-equiv" attribute.
		/// </summary>
		HttpEquiv,

		/// <summary>
		/// The "id" attribute.
		/// </summary>
		Id,

		/// <summary>
		/// The "ismap" attribute.
		/// </summary>
		IsMap,

		/// <summary>
		/// The "label" attribute.
		/// </summary>
		Label,

		/// <summary>
		/// The "lang" attribute.
		/// </summary>
		Lang,

		/// <summary>
		/// The "language" attribute.
		/// </summary>
		Language,

		/// <summary>
		/// The "leftmargin" attribute.
		/// </summary>
		LeftMargin,

		/// <summary>
		/// The "link" attribute.
		/// </summary>
		Link,

		/// <summary>
		/// The "longdesc" attribute.
		/// </summary>
		LongDesc,

		/// <summary>
		/// The "lowsrc" attribute.
		/// </summary>
		LowSrc,

		/// <summary>
		/// The "marginheight" attribute.
		/// </summary>
		MarginHeight,

		/// <summary>
		/// The "marginwidth" attribute.
		/// </summary>
		MarginWidth,

		/// <summary>
		/// The "maxlength" attribute.
		/// </summary>
		MaxLength,

		/// <summary>
		/// The "media" attribute.
		/// </summary>
		Media,

		/// <summary>
		/// The "method" attribute.
		/// </summary>
		Method,

		/// <summary>
		/// The "multiple" attribute.
		/// </summary>
		Multiple,

		/// <summary>
		/// The "name" attribute.
		/// </summary>
		Name,

		/// <summary>
		/// The "nohref" attribute.
		/// </summary>
		NoHref,

		/// <summary>
		/// The "noresize" attribute.
		/// </summary>
		NoResize,

		/// <summary>
		/// The "noshade" attribute.
		/// </summary>
		NoShade,

		/// <summary>
		/// The "nowrap" attribute.
		/// </summary>
		NoWrap,

		/// <summary>
		/// The "object" attribute.
		/// </summary>
		Object,

		/// <summary>
		/// The "profile" attribute.
		/// </summary>
		Profile,

		/// <summary>
		/// The "prompt" attribute.
		/// </summary>
		Prompt,

		/// <summary>
		/// The "readonly" attribute.
		/// </summary>
		ReadOnly,

		/// <summary>
		/// The "rel" attribute.
		/// </summary>
		Rel,

		/// <summary>
		/// The "rev" attribute.
		/// </summary>
		Rev,

		/// <summary>
		/// The "rows" attribute.
		/// </summary>
		Rows,

		/// <summary>
		/// The "rowspan" attribute.
		/// </summary>
		RowSpan,

		/// <summary>
		/// The "rules" attribute.
		/// </summary>
		Rules,

		/// <summary>
		/// The "scheme" attribute.
		/// </summary>
		Scheme,

		/// <summary>
		/// The "scope" attribute.
		/// </summary>
		Scope,

		/// <summary>
		/// The "scrolling" attribute.
		/// </summary>
		Scrolling,

		/// <summary>
		/// The "selected" attribute.
		/// </summary>
		Selected,

		/// <summary>
		/// The "shape" attribute.
		/// </summary>
		Shape,

		/// <summary>
		/// The "size" attribute.
		/// </summary>
		Size,

		/// <summary>
		/// The "span" attribute.
		/// </summary>
		Span,

		/// <summary>
		/// The "src" attribute.
		/// </summary>
		Src,

		/// <summary>
		/// The "standby" attribute.
		/// </summary>
		StandBy,

		/// <summary>
		/// The "start" attribute.
		/// </summary>
		Start,

		/// <summary>
		/// The "style" attribute.
		/// </summary>
		Style,

		/// <summary>
		/// The "summary" attribute.
		/// </summary>
		Summary,

		/// <summary>
		/// The "tabindex" attribute.
		/// </summary>
		TabIndex,

		/// <summary>
		/// The "target" attribute.
		/// </summary>
		Target,

		/// <summary>
		/// The "text" attribute.
		/// </summary>
		Text,

		/// <summary>
		/// The "title" attribute.
		/// </summary>
		Title,

		/// <summary>
		/// The "topmargin" attribute.
		/// </summary>
		TopMargin,

		/// <summary>
		/// The "type" attribute.
		/// </summary>
		Type,

		/// <summary>
		/// The "usemap" attribute.
		/// </summary>
		UseMap,

		/// <summary>
		/// The "valign" attribute.
		/// </summary>
		Valign,

		/// <summary>
		/// The "value" attribute.
		/// </summary>
		Value,

		/// <summary>
		/// The "valuetype" attribute.
		/// </summary>
		ValueType,

		/// <summary>
		/// The "version" attribute.
		/// </summary>
		Version,

		/// <summary>
		/// The "vlink" attribute.
		/// </summary>
		Vlink,

		/// <summary>
		/// The "vspace" attribute.
		/// </summary>
		Vspace,

		/// <summary>
		/// The "width" attribute.
		/// </summary>
		Width,

		/// <summary>
		/// The "xmlns" attribute.
		/// </summary>
		XmlNS
	}

	/// <summary>
	/// <see cref="HtmlAttributeId"/> extension methods.
	/// </summary>
	/// <remarks>
	/// <see cref="HtmlAttributeId"/> extension methods.
	/// </remarks>
	public static class HtmlAttributeIdExtensions
	{
		static readonly string[] AttributeNames = new string[] {
			"abbr",
			"accept",
			"accept-charset",
			"accesskey",
			"action",
			"align",
			"alink",
			"alt",
			"archive",
			"axis",
			"background",
			"bgcolor",
			"border",
			"cellpadding",
			"cellspacing",
			"char",
			"charoff",
			"charset",
			"checked",
			"cite",
			"class",
			"classid",
			"clear",
			"code",
			"codebase",
			"codetype",
			"color",
			"cols",
			"colspan",
			"compact",
			"content",
			"coords",
			"data",
			"datetime",
			"declare",
			"defer",
			"dir",
			"disabled",
			"dynsrc",
			"enctype",
			"face",
			"for",
			"frame",
			"frameborder",
			"headers",
			"height",
			"href",
			"hreflang",
			"hspace",
			"http-equiv",
			"id",
			"ismap",
			"label",
			"lang",
			"language",
			"leftmargin",
			"link",
			"longdesc",
			"lowsrc",
			"marginheight",
			"marginwidth",
			"maxlength",
			"media",
			"method",
			"multiple",
			"name",
			"nohref",
			"noresize",
			"noshade",
			"nowrap",
			"object",
			"profile",
			"prompt",
			"readonly",
			"rel",
			"rev",
			"rows",
			"rowspan",
			"rules",
			"scheme",
			"scope",
			"scrolling",
			"selected",
			"shape",
			"size",
			"span",
			"src",
			"standby",
			"start",
			"style",
			"summary",
			"tabindex",
			"target",
			"text",
			"title",
			"topmargin",
			"type",
			"usemap",
			"valign",
			"value",
			"valuetype",
			"version",
			"vlink",
			"vspace",
			"width",
			"xmlns",
		};
		static readonly Dictionary<string, HtmlAttributeId> IdMapping;

		static HtmlAttributeIdExtensions ()
		{
			var values = (HtmlAttributeId[]) Enum.GetValues (typeof (HtmlAttributeId));

			IdMapping = new Dictionary<string, HtmlAttributeId> (values.Length - 1, MimeUtils.OrdinalIgnoreCase);

			for (int i = 1; i < values.Length; i++)
				IdMapping.Add (values[i].ToAttributeName (), values[i]);
		}

		/// <summary>
		/// Converts the enum value into the equivalent attribute name.
		/// </summary>
		/// <remarks>
		/// Converts the enum value into the equivalent attribute name.
		/// </remarks>
		/// <returns>The attribute name.</returns>
		/// <param name="value">The enum value.</param>
		public static string ToAttributeName (this HtmlAttributeId value)
		{
			int index = (int) value;

			if (index > 0 && index <= AttributeNames.Length)
				return AttributeNames[index - 1];

			return value.ToString ();
		}

		/// <summary>
		/// Converts the attribute name into the equivalent attribute id.
		/// </summary>
		/// <remarks>
		/// Converts the attribute name into the equivalent attribute id.
		/// </remarks>
		/// <returns>The attribute id.</returns>
		/// <param name="name">The attribute name.</param>
		internal static HtmlAttributeId ToHtmlAttributeId (this string name)
		{
			if (!IdMapping.TryGetValue (name, out HtmlAttributeId value))
				return HtmlAttributeId.Unknown;

			return value;
		}
	}
}
