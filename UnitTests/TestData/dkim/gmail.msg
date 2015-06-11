Delivered-To: jeff@xamarin.com
Received: by 10.152.28.6 with SMTP id x6csp3014333lag;
        Wed, 10 Jun 2015 19:05:37 -0700 (PDT)
X-Received: by 10.112.150.130 with SMTP id ui2mr7261261lbb.116.1433988337576;
        Wed, 10 Jun 2015 19:05:37 -0700 (PDT)
Return-Path: <jeff.stedfast@gmail.com>
Received: from mail-la0-x241.google.com (mail-la0-x241.google.com. [2a00:1450:4010:c03::241])
        by mx.google.com with ESMTPS id pt2si10769662lbb.46.2015.06.10.19.05.37
        for <jeff@xamarin.com>
        (version=TLSv1.2 cipher=ECDHE-RSA-AES128-GCM-SHA256 bits=128/128);
        Wed, 10 Jun 2015 19:05:37 -0700 (PDT)
Received-SPF: pass (google.com: domain of jeff.stedfast@gmail.com designates 2a00:1450:4010:c03::241 as permitted sender) client-ip=2a00:1450:4010:c03::241;
Authentication-Results: mx.google.com;
       spf=pass (google.com: domain of jeff.stedfast@gmail.com designates 2a00:1450:4010:c03::241 as permitted sender) smtp.mail=jeff.stedfast@gmail.com;
       dkim=pass header.i=@gmail.com;
       dmarc=pass (p=NONE dis=NONE) header.from=gmail.com
Received: by lams18 with SMTP id s18so7168202lam.3
        for <jeff@xamarin.com>; Wed, 10 Jun 2015 19:05:37 -0700 (PDT)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=20120113;
        h=mime-version:date:message-id:subject:from:to:content-type;
        bh=2f2TQdW2+LvAjDQiv8+jr1l3/3EOZp+Gp0P1YbMNKTk=;
        b=vV7a8hT9nqVmIZ+RzVF4Tca2llKXmIColNiFDJCIt4BGomWYQjmf3i6BhHajn5JUYQ
         cMykdBDwNreibQL1ChM0xOj+SSJW1ybeznhswvClcqXb/nc60SnS+5evo53Kr89qArUC
         x/mr2JC+HZ3wkNaMoNxP8Qk7R0La9TKUDgYQcpwqjbbEZwBecthb/pMrC4Wn2Hl4mTP/
         xCV5X4uT0FrXsgCnTPcwYTB+TiLGlh5knQpdmBbxUlJRXMkTw21Mp23Zq9nA62Xl9N4I
         hS5IrxglGQ/UNV5AQtyyK8ActlfvSPtWv5ypJfeOxTBq3JrHrfZRuIuN8Ub38TvxBTHN
         0hOQ==
MIME-Version: 1.0
X-Received: by 10.152.3.33 with SMTP id 1mr7214329laz.109.1433988337354; Wed,
 10 Jun 2015 19:05:37 -0700 (PDT)
Received: by 10.25.21.73 with HTTP; Wed, 10 Jun 2015 19:05:37 -0700 (PDT)
Date: Wed, 10 Jun 2015 22:05:37 -0400
Message-ID: <CAFksUUeRp=6VFpYM-UAXbKOmK_yEn2ma5LpLUH5j05WGFaJVFQ@mail.gmail.com>
Subject: Testing verification of Google DKIM-Signature headers
From: Jeffrey Stedfast <jeff.stedfast@gmail.com>
To: Jeffrey Stedfast <jeff@xamarin.com>
Content-Type: multipart/alternative; boundary=089e0141a0b675ccdd0518346db8

--089e0141a0b675ccdd0518346db8
Content-Type: text/plain; charset=UTF-8

This is the message body.

Jeff

--089e0141a0b675ccdd0518346db8
Content-Type: text/html; charset=UTF-8

<div dir="ltr">This is the message body.<div><br></div><div>Jeff</div><div><br></div></div>

--089e0141a0b675ccdd0518346db8--
