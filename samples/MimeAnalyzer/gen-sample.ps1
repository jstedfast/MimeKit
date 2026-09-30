$ErrorActionPreference = 'Stop'

$ms = New-Object System.IO.MemoryStream

function W([byte[]] $bytes) { $ms.Write($bytes, 0, $bytes.Length) }
function A([string] $text) { W ([System.Text.Encoding]::ASCII.GetBytes($text)) }
function L([string] $text = '') { A $text; W ([byte[]](13, 10)) }
function B() { L '--=-demo-boundary' }

function UUEncodeLine([byte[]] $data) {
	$sb = New-Object System.Text.StringBuilder
	$uu = {
		param($value)

		$value = $value -band 0x3F
		if ($value -eq 0) { $value = 0x40 }

		return [char](0x20 + $value)
	}

	[void] $sb.Append((& $uu $data.Length))

	for ($i = 0; $i -lt $data.Length; $i += 3) {
		$b0 = $data[$i]
		$b1 = 0
		$b2 = 0

		if ($i + 1 -lt $data.Length) { $b1 = $data[$i + 1] }
		if ($i + 2 -lt $data.Length) { $b2 = $data[$i + 2] }

		[void] $sb.Append((& $uu ($b0 -shr 2)))
		[void] $sb.Append((& $uu ((($b0 -shl 4) -bor ($b1 -shr 4)))))
		[void] $sb.Append((& $uu ((($b1 -shl 2) -bor ($b2 -shr 6)))))
		[void] $sb.Append((& $uu $b2))
	}

	return $sb.ToString()
}

# "Otkuda on poyavilsya?" encoded in koi8-r
$koi8r = [byte[]](0xEF,0xD4,0xCB,0xD5,0xC4,0xC1,0x20,0xCF,0xCE,0x20,0xD0,0xCF,0xD1,0xD7,0xC9,0xCC,0xD3,0xD1,0x3F)

#region message headers
L 'From: John Q. Public <john@example.com>'
L 'Sender: Bad <first@example.com, second@example.com>'
L 'To: :;, , Bob <@relay.example.com:bob@example.com>, nodomain,'
L '    user@example..com., admin@[bad literal]'
L 'Cc: "unterminated quote <quoted@example.com>'
L 'Bcc: (unterminated comment <hidden@example.com>'
L 'Reply-To: friends: alice@example.com, bob@example.com'
L 'Resent-To: <<<nested@example.com>>>'
L 'Resent-From: bob@example.com <bob@example.com>'
A 'Resent-Cc: '; W $koi8r; A ' <cc@example.com>'; W ([byte[]](13, 10))
L 'Date: Fri, 26 Sep 2025 12:00:00 -0400'
L 'Date: Fri, 26 Sep 2025 12:00:01 -0400'
L 'Subject: A message that violates as many rules as it can'
A 'Subject: '; W $koi8r; W ([byte[]](13, 10))
L 'Message-Id: <compliance-demo@example.org>'
L 'X-Invalid Header: the field name above contains a space'
A "X-Bare-Linefeed: this header line ends with a bare linefeed`n"
L ('X-Long-Header: ' + ('X' * 1000))
L 'MIME-Version: 1.0'
L 'Content-Type: multipart/mixed; boundary="=-demo-boundary"'
L
#endregion

L ('This preamble contains 8-bit bytes: ')
W $koi8r; W ([byte[]](13, 10))
L

#region part 1: repeated headers, 8-bit and null bytes in the body
B
L 'Content-Type: text/plain; charset=us-ascii'
L 'Content-Type: text/html'
L 'Content-Transfer-Encoding: 7bit'
L 'Content-Transfer-Encoding: 8bit'
L
A 'This body claims to be us-ascii but is not: '; W $koi8r; W ([byte[]](13, 10))
A 'It also contains a NUL byte '; W ([byte[]](0x00)); A " and ends with a bare linefeed.`n"
L
#endregion

#region part 2: broken base64
B
L 'Content-Type: application/octet-stream; name=broken.bin'
L 'Content-Transfer-Encoding: base64'
L
L 'VGhpcyBpcyBhIGJhc2U2NCBib2R5IHdpdGgg{}aW52YWxpZCBjaGFyYWN0ZXJz'
L 'c29tZSBwYWRkaW5nPT0gaW4gdGhlIG1pZGRsZQ==and more data after it'
L 'QQ'
L
#endregion

#region part 3: broken quoted-printable
B
L 'Content-Type: text/plain; charset=us-ascii'
L 'Content-Transfer-Encoding: quoted-printable'
L
L 'This is a quoted-printable body with an invalid =ZZ escape sequence'
L 'and a soft line break followed by trailing whitespace =  '
L 'and a truncated escape at the very end of the body ='
L
#endregion

#region part 4: broken uuencode
B
L 'Content-Type: application/octet-stream; name=broken.txt'
L 'Content-Transfer-Encoding: x-uuencode'
L
L 'this pretext does not belong before the begin line'
L 'begin 9999 broken.txt'
L (UUEncodeLine ([System.Text.Encoding]::ASCII.GetBytes('This is the only uuencoded line in this part that is valid.')))
L ((UUEncodeLine ([System.Text.Encoding]::ASCII.GetBytes('This line claims to hold more bytes than it does.'))).Substring(0, 20))
L ((UUEncodeLine ([System.Text.Encoding]::ASCII.GetBytes('This line holds fewer bytes than it does.'))) + 'and then some extra data')
L 'finish'
L
#endregion

#region part 5: message/rfc822 with an illegal content-transfer-encoding
B
L 'Content-Type: message/rfc822'
L 'Content-Transfer-Encoding: base64'
L
L 'From: inner@example.com'
L 'Subject: An embedded message'
L
L 'The embedded message body.'
L
#endregion

#region part 6: multipart with an illegal cte and no boundary parameter
B
L 'Content-Type: multipart/alternative'
L 'Content-Transfer-Encoding: base64'
L
L 'This nested multipart declares no boundary parameter, so it cannot'
L 'have any child parts at all.'
L
#endregion

#region part 7: unparsable content-type and content-transfer-encoding
B
L 'Content-Type: */'
L 'Content-Transfer-Encoding: super-encoding'
L
L 'Neither of the headers above can be parsed.'
L
#endregion

#region part 8: a multipart whose boundary parameter is not usable
B
A 'Content-Type: multipart/mixed; boundary="a boundary that is much too long to ever be legal, and it also contains an illegal '
W ([byte[]](0x7F))
L ' character"'
L
L 'No boundary delimiter appears anywhere within this part.'
L
#endregion

#region part 9: a header that is never terminated
B
L 'Content-Type: text/plain'
A 'Content-Description: this header is never terminated'
#endregion

# Note: the closing "--=-demo-boundary--" delimiter is deliberately missing.

$path = Join-Path $PSScriptRoot 'noncompliant.eml'
[System.IO.File]::WriteAllBytes($path, $ms.ToArray())
$ms.Dispose()

Write-Host "wrote $path ($((Get-Item $path).Length) bytes)"
