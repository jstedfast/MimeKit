//
// TextToFlowedTests.cs
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

using System.Text;

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class TextToFlowedTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new TextToFlowed ();
			var reader = new StringReader ("");
			var writer = new StringWriter ();

			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, new StreamWriter (Stream.Null)));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (TextWriter) null));
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new TextToFlowed ();

			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.InputEncoding, Is.EqualTo (Encoding.UTF8), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Text), "InputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Flowed), "OutputFormat");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
		}

		[Test]
		public void TestSimpleTextToFlowed ()
		{
			string expected = "> Thou art a villainous ill-breeding spongy dizzy-eyed reeky elf-skinned  " + Environment.NewLine +
				"> pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the  " + Environment.NewLine +
				">>>> use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			string text = "> Thou art a villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			TextConverter converter = new TextToFlowed { Header = null, Footer = null };
			string result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));

			converter = new FlowedToText { DeleteSpace = true };
			result = converter.Convert (expected);

			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestSpaceStuffingFromLine ()
		{
			string expected = "My favorite James Bond movie is" + Environment.NewLine +
				" From Russia with love." + Environment.NewLine;
			string text = "My favorite James Bond movie is" + Environment.NewLine +
				"From Russia with love." + Environment.NewLine;
			TextConverter converter = new TextToFlowed ();
			string result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));

			converter = new FlowedToText ();
			result = converter.Convert (expected);

			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestSpaceStuffingLinesStartingWithSpace ()
		{
			string expected = "This is a regular line." + Environment.NewLine +
				"  This line starts with a space." + Environment.NewLine;
			string text = "This is a regular line." + Environment.NewLine +
				" This line starts with a space." + Environment.NewLine;
			TextConverter converter = new TextToFlowed ();
			string result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));

			converter = new FlowedToText ();
			result = converter.Convert (expected);

			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestFlowingLongLines ()
		{
			const string text = "But, soft! what light through yonder window breaks? " +
				"It is the east, and Juliet is the sun. " +
				"Arise, fair sun, and kill the envious moon, " +
				"Who is already sick and pale with grief, " +
				"That thou her maid art far more fair than she: " +
				"Be not her maid, since she is envious; " +
				"Her vestal livery is but sick and green " +
				"And none but fools do wear it; cast it off. " +
				"It is my lady, O, it is my love! " +
				"O, that she knew she were! " +
				"She speaks yet she says nothing: what of that? " +
				"Her eye discourses; I will answer it. " +
				"I am too bold, 'tis not to me she speaks: " +
				"Two of the fairest stars in all the heaven, " +
				"Having some business, do entreat her eyes " +
				"To twinkle in their spheres till they return. " +
				"What if her eyes were there, they in her head? " +
				"The brightness of her cheek would shame those stars, " +
				"As daylight doth a lamp; her eyes in heaven " +
				"Would through the airy region stream so bright " +
				"That birds would sing and think it were not night. " +
				"See, how she leans her cheek upon her hand! " +
				"O, that I were a glove upon that hand, " +
				"That I might touch that cheek!\n";
			string expected = @"But, soft! what light through yonder window breaks? It is the east, and  
Juliet is the sun. Arise, fair sun, and kill the envious moon, Who is  
already sick and pale with grief, That thou her maid art far more fair than  
she: Be not her maid, since she is envious; Her vestal livery is but sick  
and green And none but fools do wear it; cast it off. It is my lady, O, it  
is my love! O, that she knew she were! She speaks yet she says nothing: what  
of that? Her eye discourses; I will answer it. I am too bold, 'tis not to me  
she speaks: Two of the fairest stars in all the heaven, Having some  
business, do entreat her eyes To twinkle in their spheres till they return.  
What if her eyes were there, they in her head? The brightness of her cheek  
would shame those stars, As daylight doth a lamp; her eyes in heaven Would  
through the airy region stream so bright That birds would sing and think it  
were not night. See, how she leans her cheek upon her hand! O, that I were a  
glove upon that hand, That I might touch that cheek!
".Replace ("\r\n", "\n");
			TextConverter converter = new TextToFlowed ();
			string result = converter.Convert (text).Replace ("\r\n", "\n");

			Assert.That (result, Is.EqualTo (expected));

			converter = new FlowedToText () { DeleteSpace = true };
			result = converter.Convert (expected).Replace ("\r\n", "\n");

			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestFlowingLongQuotedLines ()
		{
			const string text = "A passage from Shakespear's Romeo + Juliet:\n" +
				"> Begin quote\n" +
				">> But, soft! what light through yonder window breaks? " +
				"It is the east, and Juliet is the sun. " +
				"Arise, fair sun, and kill the envious moon, " +
				"Who is already sick and pale with grief, " +
				"That thou her maid art far more fair than she: " +
				"Be not her maid, since she is envious; " +
				"Her vestal livery is but sick and green " +
				"And none but fools do wear it; cast it off. " +
				"It is my lady, O, it is my love! " +
				"O, that she knew she were! " +
				"She speaks yet she says nothing: what of that? " +
				"Her eye discourses; I will answer it. " +
				"I am too bold, 'tis not to me she speaks: " +
				"Two of the fairest stars in all the heaven, " +
				"Having some business, do entreat her eyes " +
				"To twinkle in their spheres till they return. " +
				"What if her eyes were there, they in her head? " +
				"The brightness of her cheek would shame those stars, " +
				"As daylight doth a lamp; her eyes in heaven " +
				"Would through the airy region stream so bright " +
				"That birds would sing and think it were not night. " +
				"See, how she leans her cheek upon her hand! " +
				"O, that I were a glove upon that hand, " +
				"That I might touch that cheek!\n" +
				"> End quote\n\n" +
				"Did that flow correctly?\n";
			string expected = @"A passage from Shakespear's Romeo + Juliet:
> Begin quote
>> But, soft! what light through yonder window breaks? It is the east, and  
>> Juliet is the sun. Arise, fair sun, and kill the envious moon, Who is  
>> already sick and pale with grief, That thou her maid art far more fair  
>> than she: Be not her maid, since she is envious; Her vestal livery is but  
>> sick and green And none but fools do wear it; cast it off. It is my lady,  
>> O, it is my love! O, that she knew she were! She speaks yet she says  
>> nothing: what of that? Her eye discourses; I will answer it. I am too  
>> bold, 'tis not to me she speaks: Two of the fairest stars in all the  
>> heaven, Having some business, do entreat her eyes To twinkle in their  
>> spheres till they return. What if her eyes were there, they in her head?  
>> The brightness of her cheek would shame those stars, As daylight doth a  
>> lamp; her eyes in heaven Would through the airy region stream so bright  
>> That birds would sing and think it were not night. See, how she leans her  
>> cheek upon her hand! O, that I were a glove upon that hand, That I might  
>> touch that cheek!
> End quote

Did that flow correctly?
".Replace ("\r\n", "\n");
			TextConverter converter = new TextToFlowed ();
			string result = converter.Convert (text).Replace ("\r\n", "\n");

			Assert.That (result, Is.EqualTo (expected));

			converter = new FlowedToText () { DeleteSpace = true };
			result = converter.Convert (expected).Replace ("\r\n", "\n");

			Assert.That (result, Is.EqualTo (text));
		}
	}
}
