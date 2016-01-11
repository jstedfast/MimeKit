Delivered-To: mailkit.test@gmail.com
Received: by 10.202.87.23 with SMTP id l23csp2156992oib;
        Mon, 11 Jan 2016 07:33:37 -0800 (PST)
X-Received: by 10.28.125.20 with SMTP id y20mr14315854wmc.19.1452526417231;
        Mon, 11 Jan 2016 07:33:37 -0800 (PST)
Return-Path: <mimekit.test@gmail.com>
Received: from mail-wm0-x22d.google.com (mail-wm0-x22d.google.com. [2a00:1450:400c:c09::22d])
        by mx.google.com with ESMTPS id f16si37284738wjn.136.2016.01.11.07.33.37
        for <mailkit.test@gmail.com>
        (version=TLS1_2 cipher=ECDHE-RSA-AES128-GCM-SHA256 bits=128/128);
        Mon, 11 Jan 2016 07:33:37 -0800 (PST)
Received-SPF: pass (google.com: domain of mimekit.test@gmail.com designates 2a00:1450:400c:c09::22d as permitted sender) client-ip=2a00:1450:400c:c09::22d;
Authentication-Results: mx.google.com;
       spf=pass (google.com: domain of mimekit.test@gmail.com designates 2a00:1450:400c:c09::22d as permitted sender) smtp.mailfrom=mimekit.test@gmail.com;
       dkim=pass header.i=@gmail.com;
       dmarc=pass (p=NONE dis=NONE) header.from=gmail.com
Received: by mail-wm0-x22d.google.com with SMTP id f206so266381337wmf.0
        for <mailkit.test@gmail.com>; Mon, 11 Jan 2016 07:33:37 -0800 (PST)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=20120113;
        h=message-id:date:subject:from:to:content-type;
        bh=5Cf5R557Rceump1cxPGJg6+EdeB9ebj51ZXgzDp3d7Y=;
        b=FJtQKptyeDXuLuQEKcwVn65cen1xe9R+s450cMhP+8ZPPrHavrVgowmgbQozmh93Qf
         lDRmJtt+m9GntHMMmZYjdFjEU+3h6iJ+T0NcBYgl0pNZ7TThZ769aWIry2e8HujgnmEW
         QR3WD43/6J2ABSL6r3MetlUrcB87+lcnYvZwoOh/WQAazqZpefRaL/3wO63HgzJWi5mV
         Ez9xG7cY+LbrFgEYOs63JL+yHtGTG9w6NpxzTYRw8zOAp3Lr7asGxDVpFZ7iSwhnEDKR
         Nbwc7KJ29JPFr4eFHbvQME0256ys92bFhZsMmuDj3Bc65qu1MU1jnESEeoj4LHcVcnOL
         KOHQ==
X-Received: by 10.28.55.76 with SMTP id e73mr15100383wma.53.1452526416961;
        Mon, 11 Jan 2016 07:33:36 -0800 (PST)
Return-Path: <mimekit.test@gmail.com>
Received: from Test ([127.0.0.1])
        by smtp.gmail.com with ESMTPSA id dl8sm120393245wjb.29.2016.01.11.07.33.11
        for <mailkit.test@gmail.com>
        (version=TLS1_2 cipher=ECDHE-RSA-AES128-GCM-SHA256 bits=128/128);
        Mon, 11 Jan 2016 07:33:35 -0800 (PST)
Message-ID: <5693cb4f.a85fc20a.99e77.ffff8ac8@mx.google.com>
Date: Mon, 11 Jan 2016 16:32:47 +0100
Subject: Multipart message without closing boundary
From: "mimekit.test@gmail.com" <mimekit.test@gmail.com>
To: mailkit.test@gmail.com
Content-Type: multipart/alternative; boundary=047d7bd761b6234d3005291030da

--047d7bd761b6234d3005291030da
Content-Type: text/plain; charset=UTF-8

Multipart message without closing boundary

--047d7bd761b6234d3005291030da
Content-Type: text/html; charset=UTF-8

<div dir="ltr">Multipart message without closing boundary<br></div>


