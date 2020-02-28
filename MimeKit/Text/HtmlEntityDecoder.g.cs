//
// HtmlEntityDecoder.g.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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

// WARNING: This file is auto-generated. DO NOT EDIT!

namespace MimeKit.Text {
	public partial class HtmlEntityDecoder {
		const int MaxEntityLength = 33;

		bool PushNamedEntity (char c)
		{
			int state = states[index - 1];

			switch (c) {
			case '1':
				switch (state) {
				case 566: state = 567; break; // &blk -> &blk1
				case 2280: state = 2282; break; // &emsp -> &emsp1
				case 2649: state = 2650; break; // &frac -> &frac1
				case 8284: state = 8286; break; // &sup -> &sup1
				default: return false;
				}
				break;
			case '2':
				switch (state) {
				case 567: state = 568; break; // &blk1 -> &blk12
				case 2649: state = 2663; break; // &frac -> &frac2
				case 2650: state = 2651; break; // &frac1 -> &frac12
				case 8284: state = 8288; break; // &sup -> &sup2
				default: return false;
				}
				break;
			case '3':
				switch (state) {
				case 566: state = 572; break; // &blk -> &blk3
				case 2282: state = 2283; break; // &emsp1 -> &emsp13
				case 2649: state = 2668; break; // &frac -> &frac3
				case 2650: state = 2653; break; // &frac1 -> &frac13
				case 2663: state = 2664; break; // &frac2 -> &frac23
				case 8284: state = 8290; break; // &sup -> &sup3
				default: return false;
				}
				break;
			case '4':
				switch (state) {
				case 567: state = 570; break; // &blk1 -> &blk14
				case 572: state = 573; break; // &blk3 -> &blk34
				case 2282: state = 2285; break; // &emsp1 -> &emsp14
				case 2649: state = 2675; break; // &frac -> &frac4
				case 2650: state = 2655; break; // &frac1 -> &frac14
				case 2668: state = 2669; break; // &frac3 -> &frac34
				case 8464: state = 8465; break; // &there -> &there4
				default: return false;
				}
				break;
			case '5':
				switch (state) {
				case 2649: state = 2678; break; // &frac -> &frac5
				case 2650: state = 2657; break; // &frac1 -> &frac15
				case 2663: state = 2666; break; // &frac2 -> &frac25
				case 2668: state = 2671; break; // &frac3 -> &frac35
				case 2675: state = 2676; break; // &frac4 -> &frac45
				default: return false;
				}
				break;
			case '6':
				switch (state) {
				case 2650: state = 2659; break; // &frac1 -> &frac16
				case 2678: state = 2679; break; // &frac5 -> &frac56
				default: return false;
				}
				break;
			case '7':
				switch (state) {
				case 2649: state = 2683; break; // &frac -> &frac7
				default: return false;
				}
				break;
			case '8':
				switch (state) {
				case 2650: state = 2661; break; // &frac1 -> &frac18
				case 2668: state = 2673; break; // &frac3 -> &frac38
				case 2678: state = 2681; break; // &frac5 -> &frac58
				case 2683: state = 2684; break; // &frac7 -> &frac78
				default: return false;
				}
				break;
			case ';':
				switch (state) {
				case 6: state = 7; break; // &Aacute -> &Aacute;
				case 13: state = 14; break; // &aacute -> &aacute;
				case 19: state = 20; break; // &Abreve -> &Abreve;
				case 25: state = 26; break; // &abreve -> &abreve;
				case 27: state = 28; break; // &ac -> &ac;
				case 29: state = 30; break; // &acd -> &acd;
				case 31: state = 32; break; // &acE -> &acE;
				case 36: state = 37; break; // &Acirc -> &Acirc;
				case 40: state = 41; break; // &acirc -> &acirc;
				case 44: state = 45; break; // &acute -> &acute;
				case 46: state = 47; break; // &Acy -> &Acy;
				case 48: state = 49; break; // &acy -> &acy;
				case 53: state = 54; break; // &AElig -> &AElig;
				case 58: state = 59; break; // &aelig -> &aelig;
				case 60: state = 61; break; // &af -> &af;
				case 63: state = 64; break; // &Afr -> &Afr;
				case 65: state = 66; break; // &afr -> &afr;
				case 71: state = 72; break; // &Agrave -> &Agrave;
				case 77: state = 78; break; // &agrave -> &agrave;
				case 84: state = 85; break; // &alefsym -> &alefsym;
				case 87: state = 88; break; // &aleph -> &aleph;
				case 92: state = 93; break; // &Alpha -> &Alpha;
				case 96: state = 97; break; // &alpha -> &alpha;
				case 101: state = 102; break; // &Amacr -> &Amacr;
				case 106: state = 107; break; // &amacr -> &amacr;
				case 109: state = 110; break; // &amalg -> &amalg;
				case 112: state = 113; break; // &AMP -> &AMP;
				case 114: state = 115; break; // &amp -> &amp;
				case 117: state = 118; break; // &And -> &And;
				case 120: state = 121; break; // &and -> &and;
				case 124: state = 125; break; // &andand -> &andand;
				case 126: state = 127; break; // &andd -> &andd;
				case 132: state = 133; break; // &andslope -> &andslope;
				case 134: state = 135; break; // &andv -> &andv;
				case 136: state = 137; break; // &ang -> &ang;
				case 138: state = 139; break; // &ange -> &ange;
				case 141: state = 142; break; // &angle -> &angle;
				case 145: state = 146; break; // &angmsd -> &angmsd;
				case 148: state = 149; break; // &angmsdaa -> &angmsdaa;
				case 150: state = 151; break; // &angmsdab -> &angmsdab;
				case 152: state = 153; break; // &angmsdac -> &angmsdac;
				case 154: state = 155; break; // &angmsdad -> &angmsdad;
				case 156: state = 157; break; // &angmsdae -> &angmsdae;
				case 158: state = 159; break; // &angmsdaf -> &angmsdaf;
				case 160: state = 161; break; // &angmsdag -> &angmsdag;
				case 162: state = 163; break; // &angmsdah -> &angmsdah;
				case 165: state = 166; break; // &angrt -> &angrt;
				case 168: state = 169; break; // &angrtvb -> &angrtvb;
				case 170: state = 171; break; // &angrtvbd -> &angrtvbd;
				case 174: state = 175; break; // &angsph -> &angsph;
				case 176: state = 177; break; // &angst -> &angst;
				case 181: state = 182; break; // &angzarr -> &angzarr;
				case 186: state = 187; break; // &Aogon -> &Aogon;
				case 191: state = 192; break; // &aogon -> &aogon;
				case 194: state = 195; break; // &Aopf -> &Aopf;
				case 197: state = 198; break; // &aopf -> &aopf;
				case 199: state = 200; break; // &ap -> &ap;
				case 204: state = 205; break; // &apacir -> &apacir;
				case 206: state = 207; break; // &apE -> &apE;
				case 208: state = 209; break; // &ape -> &ape;
				case 211: state = 212; break; // &apid -> &apid;
				case 214: state = 215; break; // &apos -> &apos;
				case 227: state = 228; break; // &ApplyFunction -> &ApplyFunction;
				case 232: state = 233; break; // &approx -> &approx;
				case 235: state = 236; break; // &approxeq -> &approxeq;
				case 240: state = 241; break; // &Aring -> &Aring;
				case 245: state = 246; break; // &aring -> &aring;
				case 249: state = 250; break; // &Ascr -> &Ascr;
				case 253: state = 254; break; // &ascr -> &ascr;
				case 258: state = 259; break; // &Assign -> &Assign;
				case 260: state = 261; break; // &ast -> &ast;
				case 264: state = 265; break; // &asymp -> &asymp;
				case 267: state = 268; break; // &asympeq -> &asympeq;
				case 273: state = 274; break; // &Atilde -> &Atilde;
				case 279: state = 280; break; // &atilde -> &atilde;
				case 283: state = 284; break; // &Auml -> &Auml;
				case 287: state = 288; break; // &auml -> &auml;
				case 295: state = 296; break; // &awconint -> &awconint;
				case 299: state = 300; break; // &awint -> &awint;
				case 308: state = 309; break; // &backcong -> &backcong;
				case 316: state = 317; break; // &backepsilon -> &backepsilon;
				case 322: state = 323; break; // &backprime -> &backprime;
				case 326: state = 327; break; // &backsim -> &backsim;
				case 329: state = 330; break; // &backsimeq -> &backsimeq;
				case 339: state = 340; break; // &Backslash -> &Backslash;
				case 342: state = 343; break; // &Barv -> &Barv;
				case 347: state = 348; break; // &barvee -> &barvee;
				case 351: state = 352; break; // &Barwed -> &Barwed;
				case 355: state = 356; break; // &barwed -> &barwed;
				case 358: state = 359; break; // &barwedge -> &barwedge;
				case 362: state = 363; break; // &bbrk -> &bbrk;
				case 367: state = 368; break; // &bbrktbrk -> &bbrktbrk;
				case 372: state = 373; break; // &bcong -> &bcong;
				case 375: state = 376; break; // &Bcy -> &Bcy;
				case 377: state = 378; break; // &bcy -> &bcy;
				case 382: state = 383; break; // &bdquo -> &bdquo;
				case 388: state = 389; break; // &becaus -> &becaus;
				case 395: state = 396; break; // &Because -> &Because;
				case 397: state = 398; break; // &because -> &because;
				case 403: state = 404; break; // &bemptyv -> &bemptyv;
				case 407: state = 408; break; // &bepsi -> &bepsi;
				case 412: state = 413; break; // &bernou -> &bernou;
				case 421: state = 422; break; // &Bernoullis -> &Bernoullis;
				case 424: state = 425; break; // &Beta -> &Beta;
				case 427: state = 428; break; // &beta -> &beta;
				case 429: state = 430; break; // &beth -> &beth;
				case 434: state = 435; break; // &between -> &between;
				case 437: state = 438; break; // &Bfr -> &Bfr;
				case 440: state = 441; break; // &bfr -> &bfr;
				case 446: state = 447; break; // &bigcap -> &bigcap;
				case 450: state = 451; break; // &bigcirc -> &bigcirc;
				case 453: state = 454; break; // &bigcup -> &bigcup;
				case 458: state = 459; break; // &bigodot -> &bigodot;
				case 463: state = 464; break; // &bigoplus -> &bigoplus;
				case 469: state = 470; break; // &bigotimes -> &bigotimes;
				case 475: state = 476; break; // &bigsqcup -> &bigsqcup;
				case 479: state = 480; break; // &bigstar -> &bigstar;
				case 492: state = 493; break; // &bigtriangledown -> &bigtriangledown;
				case 495: state = 496; break; // &bigtriangleup -> &bigtriangleup;
				case 501: state = 502; break; // &biguplus -> &biguplus;
				case 505: state = 506; break; // &bigvee -> &bigvee;
				case 511: state = 512; break; // &bigwedge -> &bigwedge;
				case 517: state = 518; break; // &bkarow -> &bkarow;
				case 529: state = 530; break; // &blacklozenge -> &blacklozenge;
				case 536: state = 537; break; // &blacksquare -> &blacksquare;
				case 545: state = 546; break; // &blacktriangle -> &blacktriangle;
				case 550: state = 551; break; // &blacktriangledown -> &blacktriangledown;
				case 555: state = 556; break; // &blacktriangleleft -> &blacktriangleleft;
				case 561: state = 562; break; // &blacktriangleright -> &blacktriangleright;
				case 564: state = 565; break; // &blank -> &blank;
				case 568: state = 569; break; // &blk12 -> &blk12;
				case 570: state = 571; break; // &blk14 -> &blk14;
				case 573: state = 574; break; // &blk34 -> &blk34;
				case 577: state = 578; break; // &block -> &block;
				case 580: state = 581; break; // &bne -> &bne;
				case 585: state = 586; break; // &bnequiv -> &bnequiv;
				case 589: state = 590; break; // &bNot -> &bNot;
				case 592: state = 593; break; // &bnot -> &bnot;
				case 596: state = 597; break; // &Bopf -> &Bopf;
				case 600: state = 601; break; // &bopf -> &bopf;
				case 602: state = 603; break; // &bot -> &bot;
				case 606: state = 607; break; // &bottom -> &bottom;
				case 611: state = 612; break; // &bowtie -> &bowtie;
				case 616: state = 617; break; // &boxbox -> &boxbox;
				case 619: state = 620; break; // &boxDL -> &boxDL;
				case 621: state = 622; break; // &boxDl -> &boxDl;
				case 624: state = 625; break; // &boxdL -> &boxdL;
				case 626: state = 627; break; // &boxdl -> &boxdl;
				case 628: state = 629; break; // &boxDR -> &boxDR;
				case 630: state = 631; break; // &boxDr -> &boxDr;
				case 632: state = 633; break; // &boxdR -> &boxdR;
				case 634: state = 635; break; // &boxdr -> &boxdr;
				case 636: state = 637; break; // &boxH -> &boxH;
				case 638: state = 639; break; // &boxh -> &boxh;
				case 640: state = 641; break; // &boxHD -> &boxHD;
				case 642: state = 643; break; // &boxHd -> &boxHd;
				case 644: state = 645; break; // &boxhD -> &boxhD;
				case 646: state = 647; break; // &boxhd -> &boxhd;
				case 648: state = 649; break; // &boxHU -> &boxHU;
				case 650: state = 651; break; // &boxHu -> &boxHu;
				case 652: state = 653; break; // &boxhU -> &boxhU;
				case 654: state = 655; break; // &boxhu -> &boxhu;
				case 660: state = 661; break; // &boxminus -> &boxminus;
				case 665: state = 666; break; // &boxplus -> &boxplus;
				case 671: state = 672; break; // &boxtimes -> &boxtimes;
				case 674: state = 675; break; // &boxUL -> &boxUL;
				case 676: state = 677; break; // &boxUl -> &boxUl;
				case 679: state = 680; break; // &boxuL -> &boxuL;
				case 681: state = 682; break; // &boxul -> &boxul;
				case 683: state = 684; break; // &boxUR -> &boxUR;
				case 685: state = 686; break; // &boxUr -> &boxUr;
				case 687: state = 688; break; // &boxuR -> &boxuR;
				case 689: state = 690; break; // &boxur -> &boxur;
				case 691: state = 692; break; // &boxV -> &boxV;
				case 693: state = 694; break; // &boxv -> &boxv;
				case 695: state = 696; break; // &boxVH -> &boxVH;
				case 697: state = 698; break; // &boxVh -> &boxVh;
				case 699: state = 700; break; // &boxvH -> &boxvH;
				case 701: state = 702; break; // &boxvh -> &boxvh;
				case 703: state = 704; break; // &boxVL -> &boxVL;
				case 705: state = 706; break; // &boxVl -> &boxVl;
				case 707: state = 708; break; // &boxvL -> &boxvL;
				case 709: state = 710; break; // &boxvl -> &boxvl;
				case 711: state = 712; break; // &boxVR -> &boxVR;
				case 713: state = 714; break; // &boxVr -> &boxVr;
				case 715: state = 716; break; // &boxvR -> &boxvR;
				case 717: state = 718; break; // &boxvr -> &boxvr;
				case 723: state = 724; break; // &bprime -> &bprime;
				case 728: state = 729; break; // &Breve -> &Breve;
				case 733: state = 734; break; // &breve -> &breve;
				case 738: state = 739; break; // &brvbar -> &brvbar;
				case 742: state = 743; break; // &Bscr -> &Bscr;
				case 746: state = 747; break; // &bscr -> &bscr;
				case 750: state = 751; break; // &bsemi -> &bsemi;
				case 753: state = 754; break; // &bsim -> &bsim;
				case 755: state = 756; break; // &bsime -> &bsime;
				case 758: state = 759; break; // &bsol -> &bsol;
				case 760: state = 761; break; // &bsolb -> &bsolb;
				case 765: state = 766; break; // &bsolhsub -> &bsolhsub;
				case 769: state = 770; break; // &bull -> &bull;
				case 772: state = 773; break; // &bullet -> &bullet;
				case 775: state = 776; break; // &bump -> &bump;
				case 777: state = 778; break; // &bumpE -> &bumpE;
				case 779: state = 780; break; // &bumpe -> &bumpe;
				case 785: state = 786; break; // &Bumpeq -> &Bumpeq;
				case 787: state = 788; break; // &bumpeq -> &bumpeq;
				case 794: state = 795; break; // &Cacute -> &Cacute;
				case 801: state = 802; break; // &cacute -> &cacute;
				case 803: state = 804; break; // &Cap -> &Cap;
				case 805: state = 806; break; // &cap -> &cap;
				case 809: state = 810; break; // &capand -> &capand;
				case 815: state = 816; break; // &capbrcup -> &capbrcup;
				case 819: state = 820; break; // &capcap -> &capcap;
				case 822: state = 823; break; // &capcup -> &capcup;
				case 826: state = 827; break; // &capdot -> &capdot;
				case 844: state = 845; break; // &CapitalDifferentialD -> &CapitalDifferentialD;
				case 846: state = 847; break; // &caps -> &caps;
				case 850: state = 851; break; // &caret -> &caret;
				case 853: state = 854; break; // &caron -> &caron;
				case 859: state = 860; break; // &Cayleys -> &Cayleys;
				case 864: state = 865; break; // &ccaps -> &ccaps;
				case 870: state = 871; break; // &Ccaron -> &Ccaron;
				case 874: state = 875; break; // &ccaron -> &ccaron;
				case 879: state = 880; break; // &Ccedil -> &Ccedil;
				case 884: state = 885; break; // &ccedil -> &ccedil;
				case 888: state = 889; break; // &Ccirc -> &Ccirc;
				case 892: state = 893; break; // &ccirc -> &ccirc;
				case 898: state = 899; break; // &Cconint -> &Cconint;
				case 902: state = 903; break; // &ccups -> &ccups;
				case 905: state = 906; break; // &ccupssm -> &ccupssm;
				case 909: state = 910; break; // &Cdot -> &Cdot;
				case 913: state = 914; break; // &cdot -> &cdot;
				case 918: state = 919; break; // &cedil -> &cedil;
				case 925: state = 926; break; // &Cedilla -> &Cedilla;
				case 931: state = 932; break; // &cemptyv -> &cemptyv;
				case 934: state = 935; break; // &cent -> &cent;
				case 942: state = 943; break; // &CenterDot -> &CenterDot;
				case 948: state = 949; break; // &centerdot -> &centerdot;
				case 951: state = 952; break; // &Cfr -> &Cfr;
				case 954: state = 955; break; // &cfr -> &cfr;
				case 958: state = 959; break; // &CHcy -> &CHcy;
				case 962: state = 963; break; // &chcy -> &chcy;
				case 966: state = 967; break; // &check -> &check;
				case 971: state = 972; break; // &checkmark -> &checkmark;
				case 974: state = 975; break; // &Chi -> &Chi;
				case 976: state = 977; break; // &chi -> &chi;
				case 979: state = 980; break; // &cir -> &cir;
				case 981: state = 982; break; // &circ -> &circ;
				case 984: state = 985; break; // &circeq -> &circeq;
				case 996: state = 997; break; // &circlearrowleft -> &circlearrowleft;
				case 1002: state = 1003; break; // &circlearrowright -> &circlearrowright;
				case 1007: state = 1008; break; // &circledast -> &circledast;
				case 1012: state = 1013; break; // &circledcirc -> &circledcirc;
				case 1017: state = 1018; break; // &circleddash -> &circleddash;
				case 1026: state = 1027; break; // &CircleDot -> &CircleDot;
				case 1028: state = 1029; break; // &circledR -> &circledR;
				case 1030: state = 1031; break; // &circledS -> &circledS;
				case 1036: state = 1037; break; // &CircleMinus -> &CircleMinus;
				case 1041: state = 1042; break; // &CirclePlus -> &CirclePlus;
				case 1047: state = 1048; break; // &CircleTimes -> &CircleTimes;
				case 1049: state = 1050; break; // &cirE -> &cirE;
				case 1051: state = 1052; break; // &cire -> &cire;
				case 1057: state = 1058; break; // &cirfnint -> &cirfnint;
				case 1061: state = 1062; break; // &cirmid -> &cirmid;
				case 1066: state = 1067; break; // &cirscir -> &cirscir;
				case 1090: state = 1091; break; // &ClockwiseContourIntegral -> &ClockwiseContourIntegral;
				case 1109: state = 1110; break; // &CloseCurlyDoubleQuote -> &CloseCurlyDoubleQuote;
				case 1115: state = 1116; break; // &CloseCurlyQuote -> &CloseCurlyQuote;
				case 1120: state = 1121; break; // &clubs -> &clubs;
				case 1124: state = 1125; break; // &clubsuit -> &clubsuit;
				case 1129: state = 1130; break; // &Colon -> &Colon;
				case 1134: state = 1135; break; // &colon -> &colon;
				case 1136: state = 1137; break; // &Colone -> &Colone;
				case 1138: state = 1139; break; // &colone -> &colone;
				case 1140: state = 1141; break; // &coloneq -> &coloneq;
				case 1144: state = 1145; break; // &comma -> &comma;
				case 1146: state = 1147; break; // &commat -> &commat;
				case 1148: state = 1149; break; // &comp -> &comp;
				case 1151: state = 1152; break; // &compfn -> &compfn;
				case 1158: state = 1159; break; // &complement -> &complement;
				case 1162: state = 1163; break; // &complexes -> &complexes;
				case 1165: state = 1166; break; // &cong -> &cong;
				case 1169: state = 1170; break; // &congdot -> &congdot;
				case 1177: state = 1178; break; // &Congruent -> &Congruent;
				case 1181: state = 1182; break; // &Conint -> &Conint;
				case 1185: state = 1186; break; // &conint -> &conint;
				case 1198: state = 1199; break; // &ContourIntegral -> &ContourIntegral;
				case 1201: state = 1202; break; // &Copf -> &Copf;
				case 1204: state = 1205; break; // &copf -> &copf;
				case 1208: state = 1209; break; // &coprod -> &coprod;
				case 1215: state = 1216; break; // &Coproduct -> &Coproduct;
				case 1219: state = 1220; break; // &COPY -> &COPY;
				case 1221: state = 1222; break; // &copy -> &copy;
				case 1224: state = 1225; break; // &copysr -> &copysr;
				case 1254: state = 1255; break; // &CounterClockwiseContourIntegral -> &CounterClockwiseContourIntegral;
				case 1259: state = 1260; break; // &crarr -> &crarr;
				case 1264: state = 1265; break; // &Cross -> &Cross;
				case 1268: state = 1269; break; // &cross -> &cross;
				case 1272: state = 1273; break; // &Cscr -> &Cscr;
				case 1276: state = 1277; break; // &cscr -> &cscr;
				case 1279: state = 1280; break; // &csub -> &csub;
				case 1281: state = 1282; break; // &csube -> &csube;
				case 1283: state = 1284; break; // &csup -> &csup;
				case 1285: state = 1286; break; // &csupe -> &csupe;
				case 1290: state = 1291; break; // &ctdot -> &ctdot;
				case 1297: state = 1298; break; // &cudarrl -> &cudarrl;
				case 1299: state = 1300; break; // &cudarrr -> &cudarrr;
				case 1303: state = 1304; break; // &cuepr -> &cuepr;
				case 1306: state = 1307; break; // &cuesc -> &cuesc;
				case 1311: state = 1312; break; // &cularr -> &cularr;
				case 1313: state = 1314; break; // &cularrp -> &cularrp;
				case 1316: state = 1317; break; // &Cup -> &Cup;
				case 1318: state = 1319; break; // &cup -> &cup;
				case 1324: state = 1325; break; // &cupbrcap -> &cupbrcap;
				case 1328: state = 1329; break; // &CupCap -> &CupCap;
				case 1332: state = 1333; break; // &cupcap -> &cupcap;
				case 1335: state = 1336; break; // &cupcup -> &cupcup;
				case 1339: state = 1340; break; // &cupdot -> &cupdot;
				case 1342: state = 1343; break; // &cupor -> &cupor;
				case 1344: state = 1345; break; // &cups -> &cups;
				case 1349: state = 1350; break; // &curarr -> &curarr;
				case 1351: state = 1352; break; // &curarrm -> &curarrm;
				case 1360: state = 1361; break; // &curlyeqprec -> &curlyeqprec;
				case 1365: state = 1366; break; // &curlyeqsucc -> &curlyeqsucc;
				case 1369: state = 1370; break; // &curlyvee -> &curlyvee;
				case 1375: state = 1376; break; // &curlywedge -> &curlywedge;
				case 1379: state = 1380; break; // &curren -> &curren;
				case 1391: state = 1392; break; // &curvearrowleft -> &curvearrowleft;
				case 1397: state = 1398; break; // &curvearrowright -> &curvearrowright;
				case 1401: state = 1402; break; // &cuvee -> &cuvee;
				case 1405: state = 1406; break; // &cuwed -> &cuwed;
				case 1413: state = 1414; break; // &cwconint -> &cwconint;
				case 1417: state = 1418; break; // &cwint -> &cwint;
				case 1423: state = 1424; break; // &cylcty -> &cylcty;
				case 1430: state = 1431; break; // &Dagger -> &Dagger;
				case 1437: state = 1438; break; // &dagger -> &dagger;
				case 1442: state = 1443; break; // &daleth -> &daleth;
				case 1445: state = 1446; break; // &Darr -> &Darr;
				case 1449: state = 1450; break; // &dArr -> &dArr;
				case 1452: state = 1453; break; // &darr -> &darr;
				case 1455: state = 1456; break; // &dash -> &dash;
				case 1459: state = 1460; break; // &Dashv -> &Dashv;
				case 1461: state = 1462; break; // &dashv -> &dashv;
				case 1468: state = 1469; break; // &dbkarow -> &dbkarow;
				case 1472: state = 1473; break; // &dblac -> &dblac;
				case 1478: state = 1479; break; // &Dcaron -> &Dcaron;
				case 1484: state = 1485; break; // &dcaron -> &dcaron;
				case 1486: state = 1487; break; // &Dcy -> &Dcy;
				case 1488: state = 1489; break; // &dcy -> &dcy;
				case 1490: state = 1491; break; // &DD -> &DD;
				case 1492: state = 1493; break; // &dd -> &dd;
				case 1498: state = 1499; break; // &ddagger -> &ddagger;
				case 1501: state = 1502; break; // &ddarr -> &ddarr;
				case 1508: state = 1509; break; // &DDotrahd -> &DDotrahd;
				case 1514: state = 1515; break; // &ddotseq -> &ddotseq;
				case 1517: state = 1518; break; // &deg -> &deg;
				case 1520: state = 1521; break; // &Del -> &Del;
				case 1523: state = 1524; break; // &Delta -> &Delta;
				case 1527: state = 1528; break; // &delta -> &delta;
				case 1533: state = 1534; break; // &demptyv -> &demptyv;
				case 1539: state = 1540; break; // &dfisht -> &dfisht;
				case 1542: state = 1543; break; // &Dfr -> &Dfr;
				case 1544: state = 1545; break; // &dfr -> &dfr;
				case 1548: state = 1549; break; // &dHar -> &dHar;
				case 1553: state = 1554; break; // &dharl -> &dharl;
				case 1555: state = 1556; break; // &dharr -> &dharr;
				case 1571: state = 1572; break; // &DiacriticalAcute -> &DiacriticalAcute;
				case 1575: state = 1576; break; // &DiacriticalDot -> &DiacriticalDot;
				case 1585: state = 1586; break; // &DiacriticalDoubleAcute -> &DiacriticalDoubleAcute;
				case 1591: state = 1592; break; // &DiacriticalGrave -> &DiacriticalGrave;
				case 1597: state = 1598; break; // &DiacriticalTilde -> &DiacriticalTilde;
				case 1601: state = 1602; break; // &diam -> &diam;
				case 1606: state = 1607; break; // &Diamond -> &Diamond;
				case 1610: state = 1611; break; // &diamond -> &diamond;
				case 1615: state = 1616; break; // &diamondsuit -> &diamondsuit;
				case 1617: state = 1618; break; // &diams -> &diams;
				case 1619: state = 1620; break; // &die -> &die;
				case 1631: state = 1632; break; // &DifferentialD -> &DifferentialD;
				case 1637: state = 1638; break; // &digamma -> &digamma;
				case 1641: state = 1642; break; // &disin -> &disin;
				case 1643: state = 1644; break; // &div -> &div;
				case 1647: state = 1648; break; // &divide -> &divide;
				case 1655: state = 1656; break; // &divideontimes -> &divideontimes;
				case 1659: state = 1660; break; // &divonx -> &divonx;
				case 1663: state = 1664; break; // &DJcy -> &DJcy;
				case 1667: state = 1668; break; // &djcy -> &djcy;
				case 1673: state = 1674; break; // &dlcorn -> &dlcorn;
				case 1677: state = 1678; break; // &dlcrop -> &dlcrop;
				case 1683: state = 1684; break; // &dollar -> &dollar;
				case 1687: state = 1688; break; // &Dopf -> &Dopf;
				case 1690: state = 1691; break; // &dopf -> &dopf;
				case 1692: state = 1693; break; // &Dot -> &Dot;
				case 1694: state = 1695; break; // &dot -> &dot;
				case 1698: state = 1699; break; // &DotDot -> &DotDot;
				case 1701: state = 1702; break; // &doteq -> &doteq;
				case 1705: state = 1706; break; // &doteqdot -> &doteqdot;
				case 1711: state = 1712; break; // &DotEqual -> &DotEqual;
				case 1717: state = 1718; break; // &dotminus -> &dotminus;
				case 1722: state = 1723; break; // &dotplus -> &dotplus;
				case 1729: state = 1730; break; // &dotsquare -> &dotsquare;
				case 1742: state = 1743; break; // &doublebarwedge -> &doublebarwedge;
				case 1762: state = 1763; break; // &DoubleContourIntegral -> &DoubleContourIntegral;
				case 1766: state = 1767; break; // &DoubleDot -> &DoubleDot;
				case 1774: state = 1775; break; // &DoubleDownArrow -> &DoubleDownArrow;
				case 1784: state = 1785; break; // &DoubleLeftArrow -> &DoubleLeftArrow;
				case 1795: state = 1796; break; // &DoubleLeftRightArrow -> &DoubleLeftRightArrow;
				case 1799: state = 1800; break; // &DoubleLeftTee -> &DoubleLeftTee;
				case 1812: state = 1813; break; // &DoubleLongLeftArrow -> &DoubleLongLeftArrow;
				case 1823: state = 1824; break; // &DoubleLongLeftRightArrow -> &DoubleLongLeftRightArrow;
				case 1834: state = 1835; break; // &DoubleLongRightArrow -> &DoubleLongRightArrow;
				case 1845: state = 1846; break; // &DoubleRightArrow -> &DoubleRightArrow;
				case 1849: state = 1850; break; // &DoubleRightTee -> &DoubleRightTee;
				case 1857: state = 1858; break; // &DoubleUpArrow -> &DoubleUpArrow;
				case 1867: state = 1868; break; // &DoubleUpDownArrow -> &DoubleUpDownArrow;
				case 1879: state = 1880; break; // &DoubleVerticalBar -> &DoubleVerticalBar;
				case 1887: state = 1888; break; // &DownArrow -> &DownArrow;
				case 1893: state = 1894; break; // &Downarrow -> &Downarrow;
				case 1901: state = 1902; break; // &downarrow -> &downarrow;
				case 1905: state = 1906; break; // &DownArrowBar -> &DownArrowBar;
				case 1913: state = 1914; break; // &DownArrowUpArrow -> &DownArrowUpArrow;
				case 1919: state = 1920; break; // &DownBreve -> &DownBreve;
				case 1930: state = 1931; break; // &downdownarrows -> &downdownarrows;
				case 1942: state = 1943; break; // &downharpoonleft -> &downharpoonleft;
				case 1948: state = 1949; break; // &downharpoonright -> &downharpoonright;
				case 1964: state = 1965; break; // &DownLeftRightVector -> &DownLeftRightVector;
				case 1974: state = 1975; break; // &DownLeftTeeVector -> &DownLeftTeeVector;
				case 1981: state = 1982; break; // &DownLeftVector -> &DownLeftVector;
				case 1985: state = 1986; break; // &DownLeftVectorBar -> &DownLeftVectorBar;
				case 2000: state = 2001; break; // &DownRightTeeVector -> &DownRightTeeVector;
				case 2007: state = 2008; break; // &DownRightVector -> &DownRightVector;
				case 2011: state = 2012; break; // &DownRightVectorBar -> &DownRightVectorBar;
				case 2015: state = 2016; break; // &DownTee -> &DownTee;
				case 2021: state = 2022; break; // &DownTeeArrow -> &DownTeeArrow;
				case 2029: state = 2030; break; // &drbkarow -> &drbkarow;
				case 2034: state = 2035; break; // &drcorn -> &drcorn;
				case 2038: state = 2039; break; // &drcrop -> &drcrop;
				case 2042: state = 2043; break; // &Dscr -> &Dscr;
				case 2046: state = 2047; break; // &dscr -> &dscr;
				case 2050: state = 2051; break; // &DScy -> &DScy;
				case 2052: state = 2053; break; // &dscy -> &dscy;
				case 2055: state = 2056; break; // &dsol -> &dsol;
				case 2060: state = 2061; break; // &Dstrok -> &Dstrok;
				case 2065: state = 2066; break; // &dstrok -> &dstrok;
				case 2070: state = 2071; break; // &dtdot -> &dtdot;
				case 2073: state = 2074; break; // &dtri -> &dtri;
				case 2075: state = 2076; break; // &dtrif -> &dtrif;
				case 2080: state = 2081; break; // &duarr -> &duarr;
				case 2084: state = 2085; break; // &duhar -> &duhar;
				case 2091: state = 2092; break; // &dwangle -> &dwangle;
				case 2095: state = 2096; break; // &DZcy -> &DZcy;
				case 2099: state = 2100; break; // &dzcy -> &dzcy;
				case 2106: state = 2107; break; // &dzigrarr -> &dzigrarr;
				case 2113: state = 2114; break; // &Eacute -> &Eacute;
				case 2120: state = 2121; break; // &eacute -> &eacute;
				case 2125: state = 2126; break; // &easter -> &easter;
				case 2131: state = 2132; break; // &Ecaron -> &Ecaron;
				case 2137: state = 2138; break; // &ecaron -> &ecaron;
				case 2140: state = 2141; break; // &ecir -> &ecir;
				case 2144: state = 2145; break; // &Ecirc -> &Ecirc;
				case 2146: state = 2147; break; // &ecirc -> &ecirc;
				case 2151: state = 2152; break; // &ecolon -> &ecolon;
				case 2153: state = 2154; break; // &Ecy -> &Ecy;
				case 2155: state = 2156; break; // &ecy -> &ecy;
				case 2160: state = 2161; break; // &eDDot -> &eDDot;
				case 2164: state = 2165; break; // &Edot -> &Edot;
				case 2167: state = 2168; break; // &eDot -> &eDot;
				case 2171: state = 2172; break; // &edot -> &edot;
				case 2173: state = 2174; break; // &ee -> &ee;
				case 2178: state = 2179; break; // &efDot -> &efDot;
				case 2181: state = 2182; break; // &Efr -> &Efr;
				case 2183: state = 2184; break; // &efr -> &efr;
				case 2185: state = 2186; break; // &eg -> &eg;
				case 2191: state = 2192; break; // &Egrave -> &Egrave;
				case 2196: state = 2197; break; // &egrave -> &egrave;
				case 2198: state = 2199; break; // &egs -> &egs;
				case 2202: state = 2203; break; // &egsdot -> &egsdot;
				case 2204: state = 2205; break; // &el -> &el;
				case 2211: state = 2212; break; // &Element -> &Element;
				case 2218: state = 2219; break; // &elinters -> &elinters;
				case 2220: state = 2221; break; // &ell -> &ell;
				case 2222: state = 2223; break; // &els -> &els;
				case 2226: state = 2227; break; // &elsdot -> &elsdot;
				case 2231: state = 2232; break; // &Emacr -> &Emacr;
				case 2236: state = 2237; break; // &emacr -> &emacr;
				case 2240: state = 2241; break; // &empty -> &empty;
				case 2244: state = 2245; break; // &emptyset -> &emptyset;
				case 2259: state = 2260; break; // &EmptySmallSquare -> &EmptySmallSquare;
				case 2261: state = 2262; break; // &emptyv -> &emptyv;
				case 2277: state = 2278; break; // &EmptyVerySmallSquare -> &EmptyVerySmallSquare;
				case 2280: state = 2281; break; // &emsp -> &emsp;
				case 2283: state = 2284; break; // &emsp13 -> &emsp13;
				case 2285: state = 2286; break; // &emsp14 -> &emsp14;
				case 2288: state = 2289; break; // &ENG -> &ENG;
				case 2291: state = 2292; break; // &eng -> &eng;
				case 2294: state = 2295; break; // &ensp -> &ensp;
				case 2299: state = 2300; break; // &Eogon -> &Eogon;
				case 2304: state = 2305; break; // &eogon -> &eogon;
				case 2307: state = 2308; break; // &Eopf -> &Eopf;
				case 2310: state = 2311; break; // &eopf -> &eopf;
				case 2314: state = 2315; break; // &epar -> &epar;
				case 2317: state = 2318; break; // &eparsl -> &eparsl;
				case 2321: state = 2322; break; // &eplus -> &eplus;
				case 2324: state = 2325; break; // &epsi -> &epsi;
				case 2331: state = 2332; break; // &Epsilon -> &Epsilon;
				case 2335: state = 2336; break; // &epsilon -> &epsilon;
				case 2337: state = 2338; break; // &epsiv -> &epsiv;
				case 2343: state = 2344; break; // &eqcirc -> &eqcirc;
				case 2348: state = 2349; break; // &eqcolon -> &eqcolon;
				case 2352: state = 2353; break; // &eqsim -> &eqsim;
				case 2360: state = 2361; break; // &eqslantgtr -> &eqslantgtr;
				case 2365: state = 2366; break; // &eqslantless -> &eqslantless;
				case 2370: state = 2371; break; // &Equal -> &Equal;
				case 2375: state = 2376; break; // &equals -> &equals;
				case 2381: state = 2382; break; // &EqualTilde -> &EqualTilde;
				case 2385: state = 2386; break; // &equest -> &equest;
				case 2394: state = 2395; break; // &Equilibrium -> &Equilibrium;
				case 2397: state = 2398; break; // &equiv -> &equiv;
				case 2400: state = 2401; break; // &equivDD -> &equivDD;
				case 2407: state = 2408; break; // &eqvparsl -> &eqvparsl;
				case 2412: state = 2413; break; // &erarr -> &erarr;
				case 2416: state = 2417; break; // &erDot -> &erDot;
				case 2420: state = 2421; break; // &Escr -> &Escr;
				case 2424: state = 2425; break; // &escr -> &escr;
				case 2428: state = 2429; break; // &esdot -> &esdot;
				case 2431: state = 2432; break; // &Esim -> &Esim;
				case 2434: state = 2435; break; // &esim -> &esim;
				case 2437: state = 2438; break; // &Eta -> &Eta;
				case 2440: state = 2441; break; // &eta -> &eta;
				case 2443: state = 2444; break; // &ETH -> &ETH;
				case 2445: state = 2446; break; // &eth -> &eth;
				case 2449: state = 2450; break; // &Euml -> &Euml;
				case 2453: state = 2454; break; // &euml -> &euml;
				case 2456: state = 2457; break; // &euro -> &euro;
				case 2460: state = 2461; break; // &excl -> &excl;
				case 2464: state = 2465; break; // &exist -> &exist;
				case 2470: state = 2471; break; // &Exists -> &Exists;
				case 2480: state = 2481; break; // &expectation -> &expectation;
				case 2491: state = 2492; break; // &ExponentialE -> &ExponentialE;
				case 2501: state = 2502; break; // &exponentiale -> &exponentiale;
				case 2515: state = 2516; break; // &fallingdotseq -> &fallingdotseq;
				case 2519: state = 2520; break; // &Fcy -> &Fcy;
				case 2522: state = 2523; break; // &fcy -> &fcy;
				case 2528: state = 2529; break; // &female -> &female;
				case 2534: state = 2535; break; // &ffilig -> &ffilig;
				case 2538: state = 2539; break; // &fflig -> &fflig;
				case 2542: state = 2543; break; // &ffllig -> &ffllig;
				case 2545: state = 2546; break; // &Ffr -> &Ffr;
				case 2547: state = 2548; break; // &ffr -> &ffr;
				case 2552: state = 2553; break; // &filig -> &filig;
				case 2569: state = 2570; break; // &FilledSmallSquare -> &FilledSmallSquare;
				case 2585: state = 2586; break; // &FilledVerySmallSquare -> &FilledVerySmallSquare;
				case 2590: state = 2591; break; // &fjlig -> &fjlig;
				case 2594: state = 2595; break; // &flat -> &flat;
				case 2598: state = 2599; break; // &fllig -> &fllig;
				case 2602: state = 2603; break; // &fltns -> &fltns;
				case 2606: state = 2607; break; // &fnof -> &fnof;
				case 2610: state = 2611; break; // &Fopf -> &Fopf;
				case 2614: state = 2615; break; // &fopf -> &fopf;
				case 2619: state = 2620; break; // &ForAll -> &ForAll;
				case 2624: state = 2625; break; // &forall -> &forall;
				case 2626: state = 2627; break; // &fork -> &fork;
				case 2628: state = 2629; break; // &forkv -> &forkv;
				case 2637: state = 2638; break; // &Fouriertrf -> &Fouriertrf;
				case 2645: state = 2646; break; // &fpartint -> &fpartint;
				case 2651: state = 2652; break; // &frac12 -> &frac12;
				case 2653: state = 2654; break; // &frac13 -> &frac13;
				case 2655: state = 2656; break; // &frac14 -> &frac14;
				case 2657: state = 2658; break; // &frac15 -> &frac15;
				case 2659: state = 2660; break; // &frac16 -> &frac16;
				case 2661: state = 2662; break; // &frac18 -> &frac18;
				case 2664: state = 2665; break; // &frac23 -> &frac23;
				case 2666: state = 2667; break; // &frac25 -> &frac25;
				case 2669: state = 2670; break; // &frac34 -> &frac34;
				case 2671: state = 2672; break; // &frac35 -> &frac35;
				case 2673: state = 2674; break; // &frac38 -> &frac38;
				case 2676: state = 2677; break; // &frac45 -> &frac45;
				case 2679: state = 2680; break; // &frac56 -> &frac56;
				case 2681: state = 2682; break; // &frac58 -> &frac58;
				case 2684: state = 2685; break; // &frac78 -> &frac78;
				case 2687: state = 2688; break; // &frasl -> &frasl;
				case 2691: state = 2692; break; // &frown -> &frown;
				case 2695: state = 2696; break; // &Fscr -> &Fscr;
				case 2699: state = 2700; break; // &fscr -> &fscr;
				case 2706: state = 2707; break; // &gacute -> &gacute;
				case 2712: state = 2713; break; // &Gamma -> &Gamma;
				case 2716: state = 2717; break; // &gamma -> &gamma;
				case 2718: state = 2719; break; // &Gammad -> &Gammad;
				case 2720: state = 2721; break; // &gammad -> &gammad;
				case 2722: state = 2723; break; // &gap -> &gap;
				case 2728: state = 2729; break; // &Gbreve -> &Gbreve;
				case 2734: state = 2735; break; // &gbreve -> &gbreve;
				case 2740: state = 2741; break; // &Gcedil -> &Gcedil;
				case 2744: state = 2745; break; // &Gcirc -> &Gcirc;
				case 2749: state = 2750; break; // &gcirc -> &gcirc;
				case 2751: state = 2752; break; // &Gcy -> &Gcy;
				case 2753: state = 2754; break; // &gcy -> &gcy;
				case 2757: state = 2758; break; // &Gdot -> &Gdot;
				case 2761: state = 2762; break; // &gdot -> &gdot;
				case 2763: state = 2764; break; // &gE -> &gE;
				case 2765: state = 2766; break; // &ge -> &ge;
				case 2767: state = 2768; break; // &gEl -> &gEl;
				case 2769: state = 2770; break; // &gel -> &gel;
				case 2771: state = 2772; break; // &geq -> &geq;
				case 2773: state = 2774; break; // &geqq -> &geqq;
				case 2779: state = 2780; break; // &geqslant -> &geqslant;
				case 2781: state = 2782; break; // &ges -> &ges;
				case 2784: state = 2785; break; // &gescc -> &gescc;
				case 2788: state = 2789; break; // &gesdot -> &gesdot;
				case 2790: state = 2791; break; // &gesdoto -> &gesdoto;
				case 2792: state = 2793; break; // &gesdotol -> &gesdotol;
				case 2794: state = 2795; break; // &gesl -> &gesl;
				case 2797: state = 2798; break; // &gesles -> &gesles;
				case 2800: state = 2801; break; // &Gfr -> &Gfr;
				case 2803: state = 2804; break; // &gfr -> &gfr;
				case 2805: state = 2806; break; // &Gg -> &Gg;
				case 2807: state = 2808; break; // &gg -> &gg;
				case 2809: state = 2810; break; // &ggg -> &ggg;
				case 2814: state = 2815; break; // &gimel -> &gimel;
				case 2818: state = 2819; break; // &GJcy -> &GJcy;
				case 2822: state = 2823; break; // &gjcy -> &gjcy;
				case 2824: state = 2825; break; // &gl -> &gl;
				case 2826: state = 2827; break; // &gla -> &gla;
				case 2828: state = 2829; break; // &glE -> &glE;
				case 2830: state = 2831; break; // &glj -> &glj;
				case 2834: state = 2835; break; // &gnap -> &gnap;
				case 2839: state = 2840; break; // &gnapprox -> &gnapprox;
				case 2841: state = 2842; break; // &gnE -> &gnE;
				case 2843: state = 2844; break; // &gne -> &gne;
				case 2845: state = 2846; break; // &gneq -> &gneq;
				case 2847: state = 2848; break; // &gneqq -> &gneqq;
				case 2851: state = 2852; break; // &gnsim -> &gnsim;
				case 2855: state = 2856; break; // &Gopf -> &Gopf;
				case 2859: state = 2860; break; // &gopf -> &gopf;
				case 2864: state = 2865; break; // &grave -> &grave;
				case 2876: state = 2877; break; // &GreaterEqual -> &GreaterEqual;
				case 2881: state = 2882; break; // &GreaterEqualLess -> &GreaterEqualLess;
				case 2891: state = 2892; break; // &GreaterFullEqual -> &GreaterFullEqual;
				case 2899: state = 2900; break; // &GreaterGreater -> &GreaterGreater;
				case 2904: state = 2905; break; // &GreaterLess -> &GreaterLess;
				case 2915: state = 2916; break; // &GreaterSlantEqual -> &GreaterSlantEqual;
				case 2921: state = 2922; break; // &GreaterTilde -> &GreaterTilde;
				case 2925: state = 2926; break; // &Gscr -> &Gscr;
				case 2929: state = 2930; break; // &gscr -> &gscr;
				case 2932: state = 2933; break; // &gsim -> &gsim;
				case 2934: state = 2935; break; // &gsime -> &gsime;
				case 2936: state = 2937; break; // &gsiml -> &gsiml;
				case 2938: state = 2939; break; // &GT -> &GT;
				case 2940: state = 2941; break; // &Gt -> &Gt;
				case 2942: state = 2943; break; // &gt -> &gt;
				case 2945: state = 2946; break; // &gtcc -> &gtcc;
				case 2948: state = 2949; break; // &gtcir -> &gtcir;
				case 2952: state = 2953; break; // &gtdot -> &gtdot;
				case 2957: state = 2958; break; // &gtlPar -> &gtlPar;
				case 2963: state = 2964; break; // &gtquest -> &gtquest;
				case 2971: state = 2972; break; // &gtrapprox -> &gtrapprox;
				case 2974: state = 2975; break; // &gtrarr -> &gtrarr;
				case 2978: state = 2979; break; // &gtrdot -> &gtrdot;
				case 2985: state = 2986; break; // &gtreqless -> &gtreqless;
				case 2991: state = 2992; break; // &gtreqqless -> &gtreqqless;
				case 2996: state = 2997; break; // &gtrless -> &gtrless;
				case 3000: state = 3001; break; // &gtrsim -> &gtrsim;
				case 3009: state = 3010; break; // &gvertneqq -> &gvertneqq;
				case 3012: state = 3013; break; // &gvnE -> &gvnE;
				case 3018: state = 3019; break; // &Hacek -> &Hacek;
				case 3025: state = 3026; break; // &hairsp -> &hairsp;
				case 3028: state = 3029; break; // &half -> &half;
				case 3033: state = 3034; break; // &hamilt -> &hamilt;
				case 3039: state = 3040; break; // &HARDcy -> &HARDcy;
				case 3044: state = 3045; break; // &hardcy -> &hardcy;
				case 3048: state = 3049; break; // &hArr -> &hArr;
				case 3050: state = 3051; break; // &harr -> &harr;
				case 3054: state = 3055; break; // &harrcir -> &harrcir;
				case 3056: state = 3057; break; // &harrw -> &harrw;
				case 3058: state = 3059; break; // &Hat -> &Hat;
				case 3062: state = 3063; break; // &hbar -> &hbar;
				case 3067: state = 3068; break; // &Hcirc -> &Hcirc;
				case 3072: state = 3073; break; // &hcirc -> &hcirc;
				case 3078: state = 3079; break; // &hearts -> &hearts;
				case 3082: state = 3083; break; // &heartsuit -> &heartsuit;
				case 3087: state = 3088; break; // &hellip -> &hellip;
				case 3092: state = 3093; break; // &hercon -> &hercon;
				case 3095: state = 3096; break; // &Hfr -> &Hfr;
				case 3098: state = 3099; break; // &hfr -> &hfr;
				case 3110: state = 3111; break; // &HilbertSpace -> &HilbertSpace;
				case 3118: state = 3119; break; // &hksearow -> &hksearow;
				case 3124: state = 3125; break; // &hkswarow -> &hkswarow;
				case 3129: state = 3130; break; // &hoarr -> &hoarr;
				case 3134: state = 3135; break; // &homtht -> &homtht;
				case 3146: state = 3147; break; // &hookleftarrow -> &hookleftarrow;
				case 3157: state = 3158; break; // &hookrightarrow -> &hookrightarrow;
				case 3161: state = 3162; break; // &Hopf -> &Hopf;
				case 3164: state = 3165; break; // &hopf -> &hopf;
				case 3169: state = 3170; break; // &horbar -> &horbar;
				case 3182: state = 3183; break; // &HorizontalLine -> &HorizontalLine;
				case 3186: state = 3187; break; // &Hscr -> &Hscr;
				case 3190: state = 3191; break; // &hscr -> &hscr;
				case 3195: state = 3196; break; // &hslash -> &hslash;
				case 3200: state = 3201; break; // &Hstrok -> &Hstrok;
				case 3205: state = 3206; break; // &hstrok -> &hstrok;
				case 3217: state = 3218; break; // &HumpDownHump -> &HumpDownHump;
				case 3223: state = 3224; break; // &HumpEqual -> &HumpEqual;
				case 3229: state = 3230; break; // &hybull -> &hybull;
				case 3234: state = 3235; break; // &hyphen -> &hyphen;
				case 3241: state = 3242; break; // &Iacute -> &Iacute;
				case 3248: state = 3249; break; // &iacute -> &iacute;
				case 3250: state = 3251; break; // &ic -> &ic;
				case 3255: state = 3256; break; // &Icirc -> &Icirc;
				case 3259: state = 3260; break; // &icirc -> &icirc;
				case 3261: state = 3262; break; // &Icy -> &Icy;
				case 3263: state = 3264; break; // &icy -> &icy;
				case 3267: state = 3268; break; // &Idot -> &Idot;
				case 3271: state = 3272; break; // &IEcy -> &IEcy;
				case 3275: state = 3276; break; // &iecy -> &iecy;
				case 3279: state = 3280; break; // &iexcl -> &iexcl;
				case 3282: state = 3283; break; // &iff -> &iff;
				case 3285: state = 3286; break; // &Ifr -> &Ifr;
				case 3287: state = 3288; break; // &ifr -> &ifr;
				case 3293: state = 3294; break; // &Igrave -> &Igrave;
				case 3299: state = 3300; break; // &igrave -> &igrave;
				case 3301: state = 3302; break; // &ii -> &ii;
				case 3306: state = 3307; break; // &iiiint -> &iiiint;
				case 3309: state = 3310; break; // &iiint -> &iiint;
				case 3314: state = 3315; break; // &iinfin -> &iinfin;
				case 3318: state = 3319; break; // &iiota -> &iiota;
				case 3323: state = 3324; break; // &IJlig -> &IJlig;
				case 3328: state = 3329; break; // &ijlig -> &ijlig;
				case 3330: state = 3331; break; // &Im -> &Im;
				case 3334: state = 3335; break; // &Imacr -> &Imacr;
				case 3339: state = 3340; break; // &imacr -> &imacr;
				case 3342: state = 3343; break; // &image -> &image;
				case 3350: state = 3351; break; // &ImaginaryI -> &ImaginaryI;
				case 3355: state = 3356; break; // &imagline -> &imagline;
				case 3360: state = 3361; break; // &imagpart -> &imagpart;
				case 3363: state = 3364; break; // &imath -> &imath;
				case 3366: state = 3367; break; // &imof -> &imof;
				case 3370: state = 3371; break; // &imped -> &imped;
				case 3376: state = 3377; break; // &Implies -> &Implies;
				case 3378: state = 3379; break; // &in -> &in;
				case 3383: state = 3384; break; // &incare -> &incare;
				case 3387: state = 3388; break; // &infin -> &infin;
				case 3391: state = 3392; break; // &infintie -> &infintie;
				case 3396: state = 3397; break; // &inodot -> &inodot;
				case 3399: state = 3400; break; // &Int -> &Int;
				case 3401: state = 3402; break; // &int -> &int;
				case 3405: state = 3406; break; // &intcal -> &intcal;
				case 3411: state = 3412; break; // &integers -> &integers;
				case 3417: state = 3418; break; // &Integral -> &Integral;
				case 3422: state = 3423; break; // &intercal -> &intercal;
				case 3431: state = 3432; break; // &Intersection -> &Intersection;
				case 3437: state = 3438; break; // &intlarhk -> &intlarhk;
				case 3442: state = 3443; break; // &intprod -> &intprod;
				case 3455: state = 3456; break; // &InvisibleComma -> &InvisibleComma;
				case 3461: state = 3462; break; // &InvisibleTimes -> &InvisibleTimes;
				case 3465: state = 3466; break; // &IOcy -> &IOcy;
				case 3469: state = 3470; break; // &iocy -> &iocy;
				case 3474: state = 3475; break; // &Iogon -> &Iogon;
				case 3478: state = 3479; break; // &iogon -> &iogon;
				case 3481: state = 3482; break; // &Iopf -> &Iopf;
				case 3484: state = 3485; break; // &iopf -> &iopf;
				case 3487: state = 3488; break; // &Iota -> &Iota;
				case 3490: state = 3491; break; // &iota -> &iota;
				case 3495: state = 3496; break; // &iprod -> &iprod;
				case 3501: state = 3502; break; // &iquest -> &iquest;
				case 3505: state = 3506; break; // &Iscr -> &Iscr;
				case 3509: state = 3510; break; // &iscr -> &iscr;
				case 3512: state = 3513; break; // &isin -> &isin;
				case 3516: state = 3517; break; // &isindot -> &isindot;
				case 3518: state = 3519; break; // &isinE -> &isinE;
				case 3520: state = 3521; break; // &isins -> &isins;
				case 3522: state = 3523; break; // &isinsv -> &isinsv;
				case 3524: state = 3525; break; // &isinv -> &isinv;
				case 3526: state = 3527; break; // &it -> &it;
				case 3532: state = 3533; break; // &Itilde -> &Itilde;
				case 3537: state = 3538; break; // &itilde -> &itilde;
				case 3542: state = 3543; break; // &Iukcy -> &Iukcy;
				case 3547: state = 3548; break; // &iukcy -> &iukcy;
				case 3550: state = 3551; break; // &Iuml -> &Iuml;
				case 3553: state = 3554; break; // &iuml -> &iuml;
				case 3559: state = 3560; break; // &Jcirc -> &Jcirc;
				case 3565: state = 3566; break; // &jcirc -> &jcirc;
				case 3567: state = 3568; break; // &Jcy -> &Jcy;
				case 3569: state = 3570; break; // &jcy -> &jcy;
				case 3572: state = 3573; break; // &Jfr -> &Jfr;
				case 3575: state = 3576; break; // &jfr -> &jfr;
				case 3580: state = 3581; break; // &jmath -> &jmath;
				case 3584: state = 3585; break; // &Jopf -> &Jopf;
				case 3588: state = 3589; break; // &jopf -> &jopf;
				case 3592: state = 3593; break; // &Jscr -> &Jscr;
				case 3596: state = 3597; break; // &jscr -> &jscr;
				case 3601: state = 3602; break; // &Jsercy -> &Jsercy;
				case 3606: state = 3607; break; // &jsercy -> &jsercy;
				case 3611: state = 3612; break; // &Jukcy -> &Jukcy;
				case 3616: state = 3617; break; // &jukcy -> &jukcy;
				case 3622: state = 3623; break; // &Kappa -> &Kappa;
				case 3628: state = 3629; break; // &kappa -> &kappa;
				case 3630: state = 3631; break; // &kappav -> &kappav;
				case 3636: state = 3637; break; // &Kcedil -> &Kcedil;
				case 3642: state = 3643; break; // &kcedil -> &kcedil;
				case 3644: state = 3645; break; // &Kcy -> &Kcy;
				case 3646: state = 3647; break; // &kcy -> &kcy;
				case 3649: state = 3650; break; // &Kfr -> &Kfr;
				case 3652: state = 3653; break; // &kfr -> &kfr;
				case 3658: state = 3659; break; // &kgreen -> &kgreen;
				case 3662: state = 3663; break; // &KHcy -> &KHcy;
				case 3666: state = 3667; break; // &khcy -> &khcy;
				case 3670: state = 3671; break; // &KJcy -> &KJcy;
				case 3674: state = 3675; break; // &kjcy -> &kjcy;
				case 3678: state = 3679; break; // &Kopf -> &Kopf;
				case 3682: state = 3683; break; // &kopf -> &kopf;
				case 3686: state = 3687; break; // &Kscr -> &Kscr;
				case 3690: state = 3691; break; // &kscr -> &kscr;
				case 3696: state = 3697; break; // &lAarr -> &lAarr;
				case 3703: state = 3704; break; // &Lacute -> &Lacute;
				case 3709: state = 3710; break; // &lacute -> &lacute;
				case 3716: state = 3717; break; // &laemptyv -> &laemptyv;
				case 3721: state = 3722; break; // &lagran -> &lagran;
				case 3726: state = 3727; break; // &Lambda -> &Lambda;
				case 3731: state = 3732; break; // &lambda -> &lambda;
				case 3734: state = 3735; break; // &Lang -> &Lang;
				case 3737: state = 3738; break; // &lang -> &lang;
				case 3739: state = 3740; break; // &langd -> &langd;
				case 3742: state = 3743; break; // &langle -> &langle;
				case 3744: state = 3745; break; // &lap -> &lap;
				case 3753: state = 3754; break; // &Laplacetrf -> &Laplacetrf;
				case 3757: state = 3758; break; // &laquo -> &laquo;
				case 3760: state = 3761; break; // &Larr -> &Larr;
				case 3763: state = 3764; break; // &lArr -> &lArr;
				case 3766: state = 3767; break; // &larr -> &larr;
				case 3768: state = 3769; break; // &larrb -> &larrb;
				case 3771: state = 3772; break; // &larrbfs -> &larrbfs;
				case 3774: state = 3775; break; // &larrfs -> &larrfs;
				case 3777: state = 3778; break; // &larrhk -> &larrhk;
				case 3780: state = 3781; break; // &larrlp -> &larrlp;
				case 3783: state = 3784; break; // &larrpl -> &larrpl;
				case 3787: state = 3788; break; // &larrsim -> &larrsim;
				case 3790: state = 3791; break; // &larrtl -> &larrtl;
				case 3792: state = 3793; break; // &lat -> &lat;
				case 3797: state = 3798; break; // &lAtail -> &lAtail;
				case 3801: state = 3802; break; // &latail -> &latail;
				case 3803: state = 3804; break; // &late -> &late;
				case 3805: state = 3806; break; // &lates -> &lates;
				case 3810: state = 3811; break; // &lBarr -> &lBarr;
				case 3815: state = 3816; break; // &lbarr -> &lbarr;
				case 3819: state = 3820; break; // &lbbrk -> &lbbrk;
				case 3824: state = 3825; break; // &lbrace -> &lbrace;
				case 3826: state = 3827; break; // &lbrack -> &lbrack;
				case 3829: state = 3830; break; // &lbrke -> &lbrke;
				case 3833: state = 3834; break; // &lbrksld -> &lbrksld;
				case 3835: state = 3836; break; // &lbrkslu -> &lbrkslu;
				case 3841: state = 3842; break; // &Lcaron -> &Lcaron;
				case 3847: state = 3848; break; // &lcaron -> &lcaron;
				case 3852: state = 3853; break; // &Lcedil -> &Lcedil;
				case 3857: state = 3858; break; // &lcedil -> &lcedil;
				case 3860: state = 3861; break; // &lceil -> &lceil;
				case 3863: state = 3864; break; // &lcub -> &lcub;
				case 3865: state = 3866; break; // &Lcy -> &Lcy;
				case 3867: state = 3868; break; // &lcy -> &lcy;
				case 3871: state = 3872; break; // &ldca -> &ldca;
				case 3875: state = 3876; break; // &ldquo -> &ldquo;
				case 3877: state = 3878; break; // &ldquor -> &ldquor;
				case 3883: state = 3884; break; // &ldrdhar -> &ldrdhar;
				case 3889: state = 3890; break; // &ldrushar -> &ldrushar;
				case 3892: state = 3893; break; // &ldsh -> &ldsh;
				case 3894: state = 3895; break; // &lE -> &lE;
				case 3896: state = 3897; break; // &le -> &le;
				case 3912: state = 3913; break; // &LeftAngleBracket -> &LeftAngleBracket;
				case 3917: state = 3918; break; // &LeftArrow -> &LeftArrow;
				case 3923: state = 3924; break; // &Leftarrow -> &Leftarrow;
				case 3931: state = 3932; break; // &leftarrow -> &leftarrow;
				case 3935: state = 3936; break; // &LeftArrowBar -> &LeftArrowBar;
				case 3946: state = 3947; break; // &LeftArrowRightArrow -> &LeftArrowRightArrow;
				case 3951: state = 3952; break; // &leftarrowtail -> &leftarrowtail;
				case 3959: state = 3960; break; // &LeftCeiling -> &LeftCeiling;
				case 3973: state = 3974; break; // &LeftDoubleBracket -> &LeftDoubleBracket;
				case 3985: state = 3986; break; // &LeftDownTeeVector -> &LeftDownTeeVector;
				case 3992: state = 3993; break; // &LeftDownVector -> &LeftDownVector;
				case 3996: state = 3997; break; // &LeftDownVectorBar -> &LeftDownVectorBar;
				case 4002: state = 4003; break; // &LeftFloor -> &LeftFloor;
				case 4014: state = 4015; break; // &leftharpoondown -> &leftharpoondown;
				case 4017: state = 4018; break; // &leftharpoonup -> &leftharpoonup;
				case 4028: state = 4029; break; // &leftleftarrows -> &leftleftarrows;
				case 4039: state = 4040; break; // &LeftRightArrow -> &LeftRightArrow;
				case 4050: state = 4051; break; // &Leftrightarrow -> &Leftrightarrow;
				case 4061: state = 4062; break; // &leftrightarrow -> &leftrightarrow;
				case 4063: state = 4064; break; // &leftrightarrows -> &leftrightarrows;
				case 4072: state = 4073; break; // &leftrightharpoons -> &leftrightharpoons;
				case 4083: state = 4084; break; // &leftrightsquigarrow -> &leftrightsquigarrow;
				case 4090: state = 4091; break; // &LeftRightVector -> &LeftRightVector;
				case 4094: state = 4095; break; // &LeftTee -> &LeftTee;
				case 4100: state = 4101; break; // &LeftTeeArrow -> &LeftTeeArrow;
				case 4107: state = 4108; break; // &LeftTeeVector -> &LeftTeeVector;
				case 4118: state = 4119; break; // &leftthreetimes -> &leftthreetimes;
				case 4126: state = 4127; break; // &LeftTriangle -> &LeftTriangle;
				case 4130: state = 4131; break; // &LeftTriangleBar -> &LeftTriangleBar;
				case 4136: state = 4137; break; // &LeftTriangleEqual -> &LeftTriangleEqual;
				case 4149: state = 4150; break; // &LeftUpDownVector -> &LeftUpDownVector;
				case 4159: state = 4160; break; // &LeftUpTeeVector -> &LeftUpTeeVector;
				case 4166: state = 4167; break; // &LeftUpVector -> &LeftUpVector;
				case 4170: state = 4171; break; // &LeftUpVectorBar -> &LeftUpVectorBar;
				case 4177: state = 4178; break; // &LeftVector -> &LeftVector;
				case 4181: state = 4182; break; // &LeftVectorBar -> &LeftVectorBar;
				case 4183: state = 4184; break; // &lEg -> &lEg;
				case 4185: state = 4186; break; // &leg -> &leg;
				case 4187: state = 4188; break; // &leq -> &leq;
				case 4189: state = 4190; break; // &leqq -> &leqq;
				case 4195: state = 4196; break; // &leqslant -> &leqslant;
				case 4197: state = 4198; break; // &les -> &les;
				case 4200: state = 4201; break; // &lescc -> &lescc;
				case 4204: state = 4205; break; // &lesdot -> &lesdot;
				case 4206: state = 4207; break; // &lesdoto -> &lesdoto;
				case 4208: state = 4209; break; // &lesdotor -> &lesdotor;
				case 4210: state = 4211; break; // &lesg -> &lesg;
				case 4213: state = 4214; break; // &lesges -> &lesges;
				case 4221: state = 4222; break; // &lessapprox -> &lessapprox;
				case 4225: state = 4226; break; // &lessdot -> &lessdot;
				case 4231: state = 4232; break; // &lesseqgtr -> &lesseqgtr;
				case 4236: state = 4237; break; // &lesseqqgtr -> &lesseqqgtr;
				case 4251: state = 4252; break; // &LessEqualGreater -> &LessEqualGreater;
				case 4261: state = 4262; break; // &LessFullEqual -> &LessFullEqual;
				case 4269: state = 4270; break; // &LessGreater -> &LessGreater;
				case 4273: state = 4274; break; // &lessgtr -> &lessgtr;
				case 4278: state = 4279; break; // &LessLess -> &LessLess;
				case 4282: state = 4283; break; // &lesssim -> &lesssim;
				case 4293: state = 4294; break; // &LessSlantEqual -> &LessSlantEqual;
				case 4299: state = 4300; break; // &LessTilde -> &LessTilde;
				case 4305: state = 4306; break; // &lfisht -> &lfisht;
				case 4310: state = 4311; break; // &lfloor -> &lfloor;
				case 4313: state = 4314; break; // &Lfr -> &Lfr;
				case 4315: state = 4316; break; // &lfr -> &lfr;
				case 4317: state = 4318; break; // &lg -> &lg;
				case 4319: state = 4320; break; // &lgE -> &lgE;
				case 4323: state = 4324; break; // &lHar -> &lHar;
				case 4328: state = 4329; break; // &lhard -> &lhard;
				case 4330: state = 4331; break; // &lharu -> &lharu;
				case 4332: state = 4333; break; // &lharul -> &lharul;
				case 4336: state = 4337; break; // &lhblk -> &lhblk;
				case 4340: state = 4341; break; // &LJcy -> &LJcy;
				case 4344: state = 4345; break; // &ljcy -> &ljcy;
				case 4346: state = 4347; break; // &Ll -> &Ll;
				case 4348: state = 4349; break; // &ll -> &ll;
				case 4352: state = 4353; break; // &llarr -> &llarr;
				case 4359: state = 4360; break; // &llcorner -> &llcorner;
				case 4368: state = 4369; break; // &Lleftarrow -> &Lleftarrow;
				case 4373: state = 4374; break; // &llhard -> &llhard;
				case 4377: state = 4378; break; // &lltri -> &lltri;
				case 4383: state = 4384; break; // &Lmidot -> &Lmidot;
				case 4389: state = 4390; break; // &lmidot -> &lmidot;
				case 4394: state = 4395; break; // &lmoust -> &lmoust;
				case 4399: state = 4400; break; // &lmoustache -> &lmoustache;
				case 4403: state = 4404; break; // &lnap -> &lnap;
				case 4408: state = 4409; break; // &lnapprox -> &lnapprox;
				case 4410: state = 4411; break; // &lnE -> &lnE;
				case 4412: state = 4413; break; // &lne -> &lne;
				case 4414: state = 4415; break; // &lneq -> &lneq;
				case 4416: state = 4417; break; // &lneqq -> &lneqq;
				case 4420: state = 4421; break; // &lnsim -> &lnsim;
				case 4425: state = 4426; break; // &loang -> &loang;
				case 4428: state = 4429; break; // &loarr -> &loarr;
				case 4432: state = 4433; break; // &lobrk -> &lobrk;
				case 4445: state = 4446; break; // &LongLeftArrow -> &LongLeftArrow;
				case 4455: state = 4456; break; // &Longleftarrow -> &Longleftarrow;
				case 4467: state = 4468; break; // &longleftarrow -> &longleftarrow;
				case 4478: state = 4479; break; // &LongLeftRightArrow -> &LongLeftRightArrow;
				case 4489: state = 4490; break; // &Longleftrightarrow -> &Longleftrightarrow;
				case 4500: state = 4501; break; // &longleftrightarrow -> &longleftrightarrow;
				case 4507: state = 4508; break; // &longmapsto -> &longmapsto;
				case 4518: state = 4519; break; // &LongRightArrow -> &LongRightArrow;
				case 4529: state = 4530; break; // &Longrightarrow -> &Longrightarrow;
				case 4540: state = 4541; break; // &longrightarrow -> &longrightarrow;
				case 4552: state = 4553; break; // &looparrowleft -> &looparrowleft;
				case 4558: state = 4559; break; // &looparrowright -> &looparrowright;
				case 4562: state = 4563; break; // &lopar -> &lopar;
				case 4565: state = 4566; break; // &Lopf -> &Lopf;
				case 4567: state = 4568; break; // &lopf -> &lopf;
				case 4571: state = 4572; break; // &loplus -> &loplus;
				case 4577: state = 4578; break; // &lotimes -> &lotimes;
				case 4582: state = 4583; break; // &lowast -> &lowast;
				case 4586: state = 4587; break; // &lowbar -> &lowbar;
				case 4599: state = 4600; break; // &LowerLeftArrow -> &LowerLeftArrow;
				case 4610: state = 4611; break; // &LowerRightArrow -> &LowerRightArrow;
				case 4612: state = 4613; break; // &loz -> &loz;
				case 4617: state = 4618; break; // &lozenge -> &lozenge;
				case 4619: state = 4620; break; // &lozf -> &lozf;
				case 4623: state = 4624; break; // &lpar -> &lpar;
				case 4626: state = 4627; break; // &lparlt -> &lparlt;
				case 4631: state = 4632; break; // &lrarr -> &lrarr;
				case 4638: state = 4639; break; // &lrcorner -> &lrcorner;
				case 4642: state = 4643; break; // &lrhar -> &lrhar;
				case 4644: state = 4645; break; // &lrhard -> &lrhard;
				case 4646: state = 4647; break; // &lrm -> &lrm;
				case 4650: state = 4651; break; // &lrtri -> &lrtri;
				case 4656: state = 4657; break; // &lsaquo -> &lsaquo;
				case 4660: state = 4661; break; // &Lscr -> &Lscr;
				case 4663: state = 4664; break; // &lscr -> &lscr;
				case 4665: state = 4666; break; // &Lsh -> &Lsh;
				case 4667: state = 4668; break; // &lsh -> &lsh;
				case 4670: state = 4671; break; // &lsim -> &lsim;
				case 4672: state = 4673; break; // &lsime -> &lsime;
				case 4674: state = 4675; break; // &lsimg -> &lsimg;
				case 4677: state = 4678; break; // &lsqb -> &lsqb;
				case 4680: state = 4681; break; // &lsquo -> &lsquo;
				case 4682: state = 4683; break; // &lsquor -> &lsquor;
				case 4687: state = 4688; break; // &Lstrok -> &Lstrok;
				case 4692: state = 4693; break; // &lstrok -> &lstrok;
				case 4694: state = 4695; break; // &LT -> &LT;
				case 4696: state = 4697; break; // &Lt -> &Lt;
				case 4698: state = 4699; break; // &lt -> &lt;
				case 4701: state = 4702; break; // &ltcc -> &ltcc;
				case 4704: state = 4705; break; // &ltcir -> &ltcir;
				case 4708: state = 4709; break; // &ltdot -> &ltdot;
				case 4713: state = 4714; break; // &lthree -> &lthree;
				case 4718: state = 4719; break; // &ltimes -> &ltimes;
				case 4723: state = 4724; break; // &ltlarr -> &ltlarr;
				case 4729: state = 4730; break; // &ltquest -> &ltquest;
				case 4732: state = 4733; break; // &ltri -> &ltri;
				case 4734: state = 4735; break; // &ltrie -> &ltrie;
				case 4736: state = 4737; break; // &ltrif -> &ltrif;
				case 4740: state = 4741; break; // &ltrPar -> &ltrPar;
				case 4748: state = 4749; break; // &lurdshar -> &lurdshar;
				case 4753: state = 4754; break; // &luruhar -> &luruhar;
				case 4762: state = 4763; break; // &lvertneqq -> &lvertneqq;
				case 4765: state = 4766; break; // &lvnE -> &lvnE;
				case 4770: state = 4771; break; // &macr -> &macr;
				case 4773: state = 4774; break; // &male -> &male;
				case 4775: state = 4776; break; // &malt -> &malt;
				case 4779: state = 4780; break; // &maltese -> &maltese;
				case 4783: state = 4784; break; // &Map -> &Map;
				case 4785: state = 4786; break; // &map -> &map;
				case 4789: state = 4790; break; // &mapsto -> &mapsto;
				case 4794: state = 4795; break; // &mapstodown -> &mapstodown;
				case 4799: state = 4800; break; // &mapstoleft -> &mapstoleft;
				case 4802: state = 4803; break; // &mapstoup -> &mapstoup;
				case 4807: state = 4808; break; // &marker -> &marker;
				case 4813: state = 4814; break; // &mcomma -> &mcomma;
				case 4816: state = 4817; break; // &Mcy -> &Mcy;
				case 4818: state = 4819; break; // &mcy -> &mcy;
				case 4823: state = 4824; break; // &mdash -> &mdash;
				case 4828: state = 4829; break; // &mDDot -> &mDDot;
				case 4841: state = 4842; break; // &measuredangle -> &measuredangle;
				case 4852: state = 4853; break; // &MediumSpace -> &MediumSpace;
				case 4860: state = 4861; break; // &Mellintrf -> &Mellintrf;
				case 4863: state = 4864; break; // &Mfr -> &Mfr;
				case 4866: state = 4867; break; // &mfr -> &mfr;
				case 4869: state = 4870; break; // &mho -> &mho;
				case 4874: state = 4875; break; // &micro -> &micro;
				case 4876: state = 4877; break; // &mid -> &mid;
				case 4880: state = 4881; break; // &midast -> &midast;
				case 4884: state = 4885; break; // &midcir -> &midcir;
				case 4888: state = 4889; break; // &middot -> &middot;
				case 4892: state = 4893; break; // &minus -> &minus;
				case 4894: state = 4895; break; // &minusb -> &minusb;
				case 4896: state = 4897; break; // &minusd -> &minusd;
				case 4898: state = 4899; break; // &minusdu -> &minusdu;
				case 4907: state = 4908; break; // &MinusPlus -> &MinusPlus;
				case 4911: state = 4912; break; // &mlcp -> &mlcp;
				case 4914: state = 4915; break; // &mldr -> &mldr;
				case 4920: state = 4921; break; // &mnplus -> &mnplus;
				case 4926: state = 4927; break; // &models -> &models;
				case 4930: state = 4931; break; // &Mopf -> &Mopf;
				case 4933: state = 4934; break; // &mopf -> &mopf;
				case 4935: state = 4936; break; // &mp -> &mp;
				case 4939: state = 4940; break; // &Mscr -> &Mscr;
				case 4943: state = 4944; break; // &mscr -> &mscr;
				case 4948: state = 4949; break; // &mstpos -> &mstpos;
				case 4950: state = 4951; break; // &Mu -> &Mu;
				case 4952: state = 4953; break; // &mu -> &mu;
				case 4959: state = 4960; break; // &multimap -> &multimap;
				case 4963: state = 4964; break; // &mumap -> &mumap;
				case 4969: state = 4970; break; // &nabla -> &nabla;
				case 4976: state = 4977; break; // &Nacute -> &Nacute;
				case 4981: state = 4982; break; // &nacute -> &nacute;
				case 4984: state = 4985; break; // &nang -> &nang;
				case 4986: state = 4987; break; // &nap -> &nap;
				case 4988: state = 4989; break; // &napE -> &napE;
				case 4991: state = 4992; break; // &napid -> &napid;
				case 4994: state = 4995; break; // &napos -> &napos;
				case 4999: state = 5000; break; // &napprox -> &napprox;
				case 5003: state = 5004; break; // &natur -> &natur;
				case 5006: state = 5007; break; // &natural -> &natural;
				case 5008: state = 5009; break; // &naturals -> &naturals;
				case 5012: state = 5013; break; // &nbsp -> &nbsp;
				case 5016: state = 5017; break; // &nbump -> &nbump;
				case 5018: state = 5019; break; // &nbumpe -> &nbumpe;
				case 5022: state = 5023; break; // &ncap -> &ncap;
				case 5028: state = 5029; break; // &Ncaron -> &Ncaron;
				case 5032: state = 5033; break; // &ncaron -> &ncaron;
				case 5037: state = 5038; break; // &Ncedil -> &Ncedil;
				case 5042: state = 5043; break; // &ncedil -> &ncedil;
				case 5046: state = 5047; break; // &ncong -> &ncong;
				case 5050: state = 5051; break; // &ncongdot -> &ncongdot;
				case 5053: state = 5054; break; // &ncup -> &ncup;
				case 5055: state = 5056; break; // &Ncy -> &Ncy;
				case 5057: state = 5058; break; // &ncy -> &ncy;
				case 5062: state = 5063; break; // &ndash -> &ndash;
				case 5064: state = 5065; break; // &ne -> &ne;
				case 5069: state = 5070; break; // &nearhk -> &nearhk;
				case 5073: state = 5074; break; // &neArr -> &neArr;
				case 5075: state = 5076; break; // &nearr -> &nearr;
				case 5078: state = 5079; break; // &nearrow -> &nearrow;
				case 5082: state = 5083; break; // &nedot -> &nedot;
				case 5101: state = 5102; break; // &NegativeMediumSpace -> &NegativeMediumSpace;
				case 5112: state = 5113; break; // &NegativeThickSpace -> &NegativeThickSpace;
				case 5119: state = 5120; break; // &NegativeThinSpace -> &NegativeThinSpace;
				case 5133: state = 5134; break; // &NegativeVeryThinSpace -> &NegativeVeryThinSpace;
				case 5138: state = 5139; break; // &nequiv -> &nequiv;
				case 5143: state = 5144; break; // &nesear -> &nesear;
				case 5146: state = 5147; break; // &nesim -> &nesim;
				case 5165: state = 5166; break; // &NestedGreaterGreater -> &NestedGreaterGreater;
				case 5174: state = 5175; break; // &NestedLessLess -> &NestedLessLess;
				case 5180: state = 5181; break; // &NewLine -> &NewLine;
				case 5185: state = 5186; break; // &nexist -> &nexist;
				case 5187: state = 5188; break; // &nexists -> &nexists;
				case 5190: state = 5191; break; // &Nfr -> &Nfr;
				case 5193: state = 5194; break; // &nfr -> &nfr;
				case 5196: state = 5197; break; // &ngE -> &ngE;
				case 5198: state = 5199; break; // &nge -> &nge;
				case 5200: state = 5201; break; // &ngeq -> &ngeq;
				case 5202: state = 5203; break; // &ngeqq -> &ngeqq;
				case 5208: state = 5209; break; // &ngeqslant -> &ngeqslant;
				case 5210: state = 5211; break; // &nges -> &nges;
				case 5213: state = 5214; break; // &nGg -> &nGg;
				case 5217: state = 5218; break; // &ngsim -> &ngsim;
				case 5219: state = 5220; break; // &nGt -> &nGt;
				case 5221: state = 5222; break; // &ngt -> &ngt;
				case 5223: state = 5224; break; // &ngtr -> &ngtr;
				case 5225: state = 5226; break; // &nGtv -> &nGtv;
				case 5230: state = 5231; break; // &nhArr -> &nhArr;
				case 5234: state = 5235; break; // &nharr -> &nharr;
				case 5238: state = 5239; break; // &nhpar -> &nhpar;
				case 5240: state = 5241; break; // &ni -> &ni;
				case 5242: state = 5243; break; // &nis -> &nis;
				case 5244: state = 5245; break; // &nisd -> &nisd;
				case 5246: state = 5247; break; // &niv -> &niv;
				case 5250: state = 5251; break; // &NJcy -> &NJcy;
				case 5254: state = 5255; break; // &njcy -> &njcy;
				case 5259: state = 5260; break; // &nlArr -> &nlArr;
				case 5263: state = 5264; break; // &nlarr -> &nlarr;
				case 5266: state = 5267; break; // &nldr -> &nldr;
				case 5268: state = 5269; break; // &nlE -> &nlE;
				case 5270: state = 5271; break; // &nle -> &nle;
				case 5280: state = 5281; break; // &nLeftarrow -> &nLeftarrow;
				case 5288: state = 5289; break; // &nleftarrow -> &nleftarrow;
				case 5299: state = 5300; break; // &nLeftrightarrow -> &nLeftrightarrow;
				case 5310: state = 5311; break; // &nleftrightarrow -> &nleftrightarrow;
				case 5312: state = 5313; break; // &nleq -> &nleq;
				case 5314: state = 5315; break; // &nleqq -> &nleqq;
				case 5320: state = 5321; break; // &nleqslant -> &nleqslant;
				case 5322: state = 5323; break; // &nles -> &nles;
				case 5324: state = 5325; break; // &nless -> &nless;
				case 5326: state = 5327; break; // &nLl -> &nLl;
				case 5330: state = 5331; break; // &nlsim -> &nlsim;
				case 5332: state = 5333; break; // &nLt -> &nLt;
				case 5334: state = 5335; break; // &nlt -> &nlt;
				case 5337: state = 5338; break; // &nltri -> &nltri;
				case 5339: state = 5340; break; // &nltrie -> &nltrie;
				case 5341: state = 5342; break; // &nLtv -> &nLtv;
				case 5345: state = 5346; break; // &nmid -> &nmid;
				case 5352: state = 5353; break; // &NoBreak -> &NoBreak;
				case 5367: state = 5368; break; // &NonBreakingSpace -> &NonBreakingSpace;
				case 5370: state = 5371; break; // &Nopf -> &Nopf;
				case 5374: state = 5375; break; // &nopf -> &nopf;
				case 5376: state = 5377; break; // &Not -> &Not;
				case 5378: state = 5379; break; // &not -> &not;
				case 5388: state = 5389; break; // &NotCongruent -> &NotCongruent;
				case 5394: state = 5395; break; // &NotCupCap -> &NotCupCap;
				case 5412: state = 5413; break; // &NotDoubleVerticalBar -> &NotDoubleVerticalBar;
				case 5420: state = 5421; break; // &NotElement -> &NotElement;
				case 5425: state = 5426; break; // &NotEqual -> &NotEqual;
				case 5431: state = 5432; break; // &NotEqualTilde -> &NotEqualTilde;
				case 5437: state = 5438; break; // &NotExists -> &NotExists;
				case 5445: state = 5446; break; // &NotGreater -> &NotGreater;
				case 5451: state = 5452; break; // &NotGreaterEqual -> &NotGreaterEqual;
				case 5461: state = 5462; break; // &NotGreaterFullEqual -> &NotGreaterFullEqual;
				case 5469: state = 5470; break; // &NotGreaterGreater -> &NotGreaterGreater;
				case 5474: state = 5475; break; // &NotGreaterLess -> &NotGreaterLess;
				case 5485: state = 5486; break; // &NotGreaterSlantEqual -> &NotGreaterSlantEqual;
				case 5491: state = 5492; break; // &NotGreaterTilde -> &NotGreaterTilde;
				case 5504: state = 5505; break; // &NotHumpDownHump -> &NotHumpDownHump;
				case 5510: state = 5511; break; // &NotHumpEqual -> &NotHumpEqual;
				case 5513: state = 5514; break; // &notin -> &notin;
				case 5517: state = 5518; break; // &notindot -> &notindot;
				case 5519: state = 5520; break; // &notinE -> &notinE;
				case 5522: state = 5523; break; // &notinva -> &notinva;
				case 5524: state = 5525; break; // &notinvb -> &notinvb;
				case 5526: state = 5527; break; // &notinvc -> &notinvc;
				case 5539: state = 5540; break; // &NotLeftTriangle -> &NotLeftTriangle;
				case 5543: state = 5544; break; // &NotLeftTriangleBar -> &NotLeftTriangleBar;
				case 5549: state = 5550; break; // &NotLeftTriangleEqual -> &NotLeftTriangleEqual;
				case 5552: state = 5553; break; // &NotLess -> &NotLess;
				case 5558: state = 5559; break; // &NotLessEqual -> &NotLessEqual;
				case 5566: state = 5567; break; // &NotLessGreater -> &NotLessGreater;
				case 5571: state = 5572; break; // &NotLessLess -> &NotLessLess;
				case 5582: state = 5583; break; // &NotLessSlantEqual -> &NotLessSlantEqual;
				case 5588: state = 5589; break; // &NotLessTilde -> &NotLessTilde;
				case 5609: state = 5610; break; // &NotNestedGreaterGreater -> &NotNestedGreaterGreater;
				case 5618: state = 5619; break; // &NotNestedLessLess -> &NotNestedLessLess;
				case 5621: state = 5622; break; // &notni -> &notni;
				case 5624: state = 5625; break; // &notniva -> &notniva;
				case 5626: state = 5627; break; // &notnivb -> &notnivb;
				case 5628: state = 5629; break; // &notnivc -> &notnivc;
				case 5637: state = 5638; break; // &NotPrecedes -> &NotPrecedes;
				case 5643: state = 5644; break; // &NotPrecedesEqual -> &NotPrecedesEqual;
				case 5654: state = 5655; break; // &NotPrecedesSlantEqual -> &NotPrecedesSlantEqual;
				case 5669: state = 5670; break; // &NotReverseElement -> &NotReverseElement;
				case 5682: state = 5683; break; // &NotRightTriangle -> &NotRightTriangle;
				case 5686: state = 5687; break; // &NotRightTriangleBar -> &NotRightTriangleBar;
				case 5692: state = 5693; break; // &NotRightTriangleEqual -> &NotRightTriangleEqual;
				case 5705: state = 5706; break; // &NotSquareSubset -> &NotSquareSubset;
				case 5711: state = 5712; break; // &NotSquareSubsetEqual -> &NotSquareSubsetEqual;
				case 5718: state = 5719; break; // &NotSquareSuperset -> &NotSquareSuperset;
				case 5724: state = 5725; break; // &NotSquareSupersetEqual -> &NotSquareSupersetEqual;
				case 5730: state = 5731; break; // &NotSubset -> &NotSubset;
				case 5736: state = 5737; break; // &NotSubsetEqual -> &NotSubsetEqual;
				case 5743: state = 5744; break; // &NotSucceeds -> &NotSucceeds;
				case 5749: state = 5750; break; // &NotSucceedsEqual -> &NotSucceedsEqual;
				case 5760: state = 5761; break; // &NotSucceedsSlantEqual -> &NotSucceedsSlantEqual;
				case 5766: state = 5767; break; // &NotSucceedsTilde -> &NotSucceedsTilde;
				case 5773: state = 5774; break; // &NotSuperset -> &NotSuperset;
				case 5779: state = 5780; break; // &NotSupersetEqual -> &NotSupersetEqual;
				case 5785: state = 5786; break; // &NotTilde -> &NotTilde;
				case 5791: state = 5792; break; // &NotTildeEqual -> &NotTildeEqual;
				case 5801: state = 5802; break; // &NotTildeFullEqual -> &NotTildeFullEqual;
				case 5807: state = 5808; break; // &NotTildeTilde -> &NotTildeTilde;
				case 5819: state = 5820; break; // &NotVerticalBar -> &NotVerticalBar;
				case 5823: state = 5824; break; // &npar -> &npar;
				case 5829: state = 5830; break; // &nparallel -> &nparallel;
				case 5832: state = 5833; break; // &nparsl -> &nparsl;
				case 5834: state = 5835; break; // &npart -> &npart;
				case 5840: state = 5841; break; // &npolint -> &npolint;
				case 5842: state = 5843; break; // &npr -> &npr;
				case 5846: state = 5847; break; // &nprcue -> &nprcue;
				case 5848: state = 5849; break; // &npre -> &npre;
				case 5850: state = 5851; break; // &nprec -> &nprec;
				case 5853: state = 5854; break; // &npreceq -> &npreceq;
				case 5858: state = 5859; break; // &nrArr -> &nrArr;
				case 5862: state = 5863; break; // &nrarr -> &nrarr;
				case 5864: state = 5865; break; // &nrarrc -> &nrarrc;
				case 5866: state = 5867; break; // &nrarrw -> &nrarrw;
				case 5877: state = 5878; break; // &nRightarrow -> &nRightarrow;
				case 5887: state = 5888; break; // &nrightarrow -> &nrightarrow;
				case 5891: state = 5892; break; // &nrtri -> &nrtri;
				case 5893: state = 5894; break; // &nrtrie -> &nrtrie;
				case 5896: state = 5897; break; // &nsc -> &nsc;
				case 5900: state = 5901; break; // &nsccue -> &nsccue;
				case 5902: state = 5903; break; // &nsce -> &nsce;
				case 5906: state = 5907; break; // &Nscr -> &Nscr;
				case 5908: state = 5909; break; // &nscr -> &nscr;
				case 5916: state = 5917; break; // &nshortmid -> &nshortmid;
				case 5925: state = 5926; break; // &nshortparallel -> &nshortparallel;
				case 5928: state = 5929; break; // &nsim -> &nsim;
				case 5930: state = 5931; break; // &nsime -> &nsime;
				case 5932: state = 5933; break; // &nsimeq -> &nsimeq;
				case 5936: state = 5937; break; // &nsmid -> &nsmid;
				case 5940: state = 5941; break; // &nspar -> &nspar;
				case 5946: state = 5947; break; // &nsqsube -> &nsqsube;
				case 5949: state = 5950; break; // &nsqsupe -> &nsqsupe;
				case 5952: state = 5953; break; // &nsub -> &nsub;
				case 5954: state = 5955; break; // &nsubE -> &nsubE;
				case 5956: state = 5957; break; // &nsube -> &nsube;
				case 5960: state = 5961; break; // &nsubset -> &nsubset;
				case 5963: state = 5964; break; // &nsubseteq -> &nsubseteq;
				case 5965: state = 5966; break; // &nsubseteqq -> &nsubseteqq;
				case 5968: state = 5969; break; // &nsucc -> &nsucc;
				case 5971: state = 5972; break; // &nsucceq -> &nsucceq;
				case 5973: state = 5974; break; // &nsup -> &nsup;
				case 5975: state = 5976; break; // &nsupE -> &nsupE;
				case 5977: state = 5978; break; // &nsupe -> &nsupe;
				case 5981: state = 5982; break; // &nsupset -> &nsupset;
				case 5984: state = 5985; break; // &nsupseteq -> &nsupseteq;
				case 5986: state = 5987; break; // &nsupseteqq -> &nsupseteqq;
				case 5990: state = 5991; break; // &ntgl -> &ntgl;
				case 5996: state = 5997; break; // &Ntilde -> &Ntilde;
				case 6001: state = 6002; break; // &ntilde -> &ntilde;
				case 6004: state = 6005; break; // &ntlg -> &ntlg;
				case 6016: state = 6017; break; // &ntriangleleft -> &ntriangleleft;
				case 6019: state = 6020; break; // &ntrianglelefteq -> &ntrianglelefteq;
				case 6025: state = 6026; break; // &ntriangleright -> &ntriangleright;
				case 6028: state = 6029; break; // &ntrianglerighteq -> &ntrianglerighteq;
				case 6030: state = 6031; break; // &Nu -> &Nu;
				case 6032: state = 6033; break; // &nu -> &nu;
				case 6034: state = 6035; break; // &num -> &num;
				case 6038: state = 6039; break; // &numero -> &numero;
				case 6041: state = 6042; break; // &numsp -> &numsp;
				case 6045: state = 6046; break; // &nvap -> &nvap;
				case 6051: state = 6052; break; // &nVDash -> &nVDash;
				case 6056: state = 6057; break; // &nVdash -> &nVdash;
				case 6061: state = 6062; break; // &nvDash -> &nvDash;
				case 6066: state = 6067; break; // &nvdash -> &nvdash;
				case 6069: state = 6070; break; // &nvge -> &nvge;
				case 6071: state = 6072; break; // &nvgt -> &nvgt;
				case 6076: state = 6077; break; // &nvHarr -> &nvHarr;
				case 6082: state = 6083; break; // &nvinfin -> &nvinfin;
				case 6087: state = 6088; break; // &nvlArr -> &nvlArr;
				case 6089: state = 6090; break; // &nvle -> &nvle;
				case 6091: state = 6092; break; // &nvlt -> &nvlt;
				case 6095: state = 6096; break; // &nvltrie -> &nvltrie;
				case 6100: state = 6101; break; // &nvrArr -> &nvrArr;
				case 6105: state = 6106; break; // &nvrtrie -> &nvrtrie;
				case 6109: state = 6110; break; // &nvsim -> &nvsim;
				case 6115: state = 6116; break; // &nwarhk -> &nwarhk;
				case 6119: state = 6120; break; // &nwArr -> &nwArr;
				case 6121: state = 6122; break; // &nwarr -> &nwarr;
				case 6124: state = 6125; break; // &nwarrow -> &nwarrow;
				case 6129: state = 6130; break; // &nwnear -> &nwnear;
				case 6136: state = 6137; break; // &Oacute -> &Oacute;
				case 6143: state = 6144; break; // &oacute -> &oacute;
				case 6146: state = 6147; break; // &oast -> &oast;
				case 6150: state = 6151; break; // &ocir -> &ocir;
				case 6155: state = 6156; break; // &Ocirc -> &Ocirc;
				case 6157: state = 6158; break; // &ocirc -> &ocirc;
				case 6159: state = 6160; break; // &Ocy -> &Ocy;
				case 6161: state = 6162; break; // &ocy -> &ocy;
				case 6166: state = 6167; break; // &odash -> &odash;
				case 6172: state = 6173; break; // &Odblac -> &Odblac;
				case 6177: state = 6178; break; // &odblac -> &odblac;
				case 6180: state = 6181; break; // &odiv -> &odiv;
				case 6183: state = 6184; break; // &odot -> &odot;
				case 6188: state = 6189; break; // &odsold -> &odsold;
				case 6193: state = 6194; break; // &OElig -> &OElig;
				case 6198: state = 6199; break; // &oelig -> &oelig;
				case 6203: state = 6204; break; // &ofcir -> &ofcir;
				case 6206: state = 6207; break; // &Ofr -> &Ofr;
				case 6208: state = 6209; break; // &ofr -> &ofr;
				case 6212: state = 6213; break; // &ogon -> &ogon;
				case 6218: state = 6219; break; // &Ograve -> &Ograve;
				case 6223: state = 6224; break; // &ograve -> &ograve;
				case 6225: state = 6226; break; // &ogt -> &ogt;
				case 6230: state = 6231; break; // &ohbar -> &ohbar;
				case 6232: state = 6233; break; // &ohm -> &ohm;
				case 6236: state = 6237; break; // &oint -> &oint;
				case 6241: state = 6242; break; // &olarr -> &olarr;
				case 6245: state = 6246; break; // &olcir -> &olcir;
				case 6250: state = 6251; break; // &olcross -> &olcross;
				case 6254: state = 6255; break; // &oline -> &oline;
				case 6256: state = 6257; break; // &olt -> &olt;
				case 6261: state = 6262; break; // &Omacr -> &Omacr;
				case 6266: state = 6267; break; // &omacr -> &omacr;
				case 6270: state = 6271; break; // &Omega -> &Omega;
				case 6274: state = 6275; break; // &omega -> &omega;
				case 6280: state = 6281; break; // &Omicron -> &Omicron;
				case 6286: state = 6287; break; // &omicron -> &omicron;
				case 6288: state = 6289; break; // &omid -> &omid;
				case 6292: state = 6293; break; // &ominus -> &ominus;
				case 6296: state = 6297; break; // &Oopf -> &Oopf;
				case 6300: state = 6301; break; // &oopf -> &oopf;
				case 6304: state = 6305; break; // &opar -> &opar;
				case 6324: state = 6325; break; // &OpenCurlyDoubleQuote -> &OpenCurlyDoubleQuote;
				case 6330: state = 6331; break; // &OpenCurlyQuote -> &OpenCurlyQuote;
				case 6334: state = 6335; break; // &operp -> &operp;
				case 6338: state = 6339; break; // &oplus -> &oplus;
				case 6340: state = 6341; break; // &Or -> &Or;
				case 6342: state = 6343; break; // &or -> &or;
				case 6346: state = 6347; break; // &orarr -> &orarr;
				case 6348: state = 6349; break; // &ord -> &ord;
				case 6351: state = 6352; break; // &order -> &order;
				case 6354: state = 6355; break; // &orderof -> &orderof;
				case 6356: state = 6357; break; // &ordf -> &ordf;
				case 6358: state = 6359; break; // &ordm -> &ordm;
				case 6363: state = 6364; break; // &origof -> &origof;
				case 6366: state = 6367; break; // &oror -> &oror;
				case 6372: state = 6373; break; // &orslope -> &orslope;
				case 6374: state = 6375; break; // &orv -> &orv;
				case 6376: state = 6377; break; // &oS -> &oS;
				case 6380: state = 6381; break; // &Oscr -> &Oscr;
				case 6384: state = 6385; break; // &oscr -> &oscr;
				case 6389: state = 6390; break; // &Oslash -> &Oslash;
				case 6394: state = 6395; break; // &oslash -> &oslash;
				case 6397: state = 6398; break; // &osol -> &osol;
				case 6403: state = 6404; break; // &Otilde -> &Otilde;
				case 6409: state = 6410; break; // &otilde -> &otilde;
				case 6413: state = 6414; break; // &Otimes -> &Otimes;
				case 6417: state = 6418; break; // &otimes -> &otimes;
				case 6420: state = 6421; break; // &otimesas -> &otimesas;
				case 6424: state = 6425; break; // &Ouml -> &Ouml;
				case 6428: state = 6429; break; // &ouml -> &ouml;
				case 6433: state = 6434; break; // &ovbar -> &ovbar;
				case 6440: state = 6441; break; // &OverBar -> &OverBar;
				case 6445: state = 6446; break; // &OverBrace -> &OverBrace;
				case 6449: state = 6450; break; // &OverBracket -> &OverBracket;
				case 6461: state = 6462; break; // &OverParenthesis -> &OverParenthesis;
				case 6465: state = 6466; break; // &par -> &par;
				case 6467: state = 6468; break; // &para -> &para;
				case 6472: state = 6473; break; // &parallel -> &parallel;
				case 6476: state = 6477; break; // &parsim -> &parsim;
				case 6478: state = 6479; break; // &parsl -> &parsl;
				case 6480: state = 6481; break; // &part -> &part;
				case 6489: state = 6490; break; // &PartialD -> &PartialD;
				case 6492: state = 6493; break; // &Pcy -> &Pcy;
				case 6495: state = 6496; break; // &pcy -> &pcy;
				case 6501: state = 6502; break; // &percnt -> &percnt;
				case 6505: state = 6506; break; // &period -> &period;
				case 6509: state = 6510; break; // &permil -> &permil;
				case 6511: state = 6512; break; // &perp -> &perp;
				case 6516: state = 6517; break; // &pertenk -> &pertenk;
				case 6519: state = 6520; break; // &Pfr -> &Pfr;
				case 6522: state = 6523; break; // &pfr -> &pfr;
				case 6525: state = 6526; break; // &Phi -> &Phi;
				case 6528: state = 6529; break; // &phi -> &phi;
				case 6530: state = 6531; break; // &phiv -> &phiv;
				case 6535: state = 6536; break; // &phmmat -> &phmmat;
				case 6539: state = 6540; break; // &phone -> &phone;
				case 6541: state = 6542; break; // &Pi -> &Pi;
				case 6543: state = 6544; break; // &pi -> &pi;
				case 6551: state = 6552; break; // &pitchfork -> &pitchfork;
				case 6553: state = 6554; break; // &piv -> &piv;
				case 6559: state = 6560; break; // &planck -> &planck;
				case 6561: state = 6562; break; // &planckh -> &planckh;
				case 6564: state = 6565; break; // &plankv -> &plankv;
				case 6567: state = 6568; break; // &plus -> &plus;
				case 6572: state = 6573; break; // &plusacir -> &plusacir;
				case 6574: state = 6575; break; // &plusb -> &plusb;
				case 6578: state = 6579; break; // &pluscir -> &pluscir;
				case 6581: state = 6582; break; // &plusdo -> &plusdo;
				case 6583: state = 6584; break; // &plusdu -> &plusdu;
				case 6585: state = 6586; break; // &pluse -> &pluse;
				case 6594: state = 6595; break; // &PlusMinus -> &PlusMinus;
				case 6597: state = 6598; break; // &plusmn -> &plusmn;
				case 6601: state = 6602; break; // &plussim -> &plussim;
				case 6605: state = 6606; break; // &plustwo -> &plustwo;
				case 6607: state = 6608; break; // &pm -> &pm;
				case 6620: state = 6621; break; // &Poincareplane -> &Poincareplane;
				case 6628: state = 6629; break; // &pointint -> &pointint;
				case 6631: state = 6632; break; // &Popf -> &Popf;
				case 6634: state = 6635; break; // &popf -> &popf;
				case 6638: state = 6639; break; // &pound -> &pound;
				case 6640: state = 6641; break; // &Pr -> &Pr;
				case 6642: state = 6643; break; // &pr -> &pr;
				case 6645: state = 6646; break; // &prap -> &prap;
				case 6649: state = 6650; break; // &prcue -> &prcue;
				case 6651: state = 6652; break; // &prE -> &prE;
				case 6653: state = 6654; break; // &pre -> &pre;
				case 6655: state = 6656; break; // &prec -> &prec;
				case 6662: state = 6663; break; // &precapprox -> &precapprox;
				case 6670: state = 6671; break; // &preccurlyeq -> &preccurlyeq;
				case 6677: state = 6678; break; // &Precedes -> &Precedes;
				case 6683: state = 6684; break; // &PrecedesEqual -> &PrecedesEqual;
				case 6694: state = 6695; break; // &PrecedesSlantEqual -> &PrecedesSlantEqual;
				case 6700: state = 6701; break; // &PrecedesTilde -> &PrecedesTilde;
				case 6703: state = 6704; break; // &preceq -> &preceq;
				case 6711: state = 6712; break; // &precnapprox -> &precnapprox;
				case 6715: state = 6716; break; // &precneqq -> &precneqq;
				case 6719: state = 6720; break; // &precnsim -> &precnsim;
				case 6723: state = 6724; break; // &precsim -> &precsim;
				case 6727: state = 6728; break; // &Prime -> &Prime;
				case 6731: state = 6732; break; // &prime -> &prime;
				case 6733: state = 6734; break; // &primes -> &primes;
				case 6737: state = 6738; break; // &prnap -> &prnap;
				case 6739: state = 6740; break; // &prnE -> &prnE;
				case 6743: state = 6744; break; // &prnsim -> &prnsim;
				case 6746: state = 6747; break; // &prod -> &prod;
				case 6752: state = 6753; break; // &Product -> &Product;
				case 6758: state = 6759; break; // &profalar -> &profalar;
				case 6763: state = 6764; break; // &profline -> &profline;
				case 6768: state = 6769; break; // &profsurf -> &profsurf;
				case 6770: state = 6771; break; // &prop -> &prop;
				case 6778: state = 6779; break; // &Proportion -> &Proportion;
				case 6781: state = 6782; break; // &Proportional -> &Proportional;
				case 6784: state = 6785; break; // &propto -> &propto;
				case 6788: state = 6789; break; // &prsim -> &prsim;
				case 6793: state = 6794; break; // &prurel -> &prurel;
				case 6797: state = 6798; break; // &Pscr -> &Pscr;
				case 6801: state = 6802; break; // &pscr -> &pscr;
				case 6803: state = 6804; break; // &Psi -> &Psi;
				case 6805: state = 6806; break; // &psi -> &psi;
				case 6811: state = 6812; break; // &puncsp -> &puncsp;
				case 6815: state = 6816; break; // &Qfr -> &Qfr;
				case 6819: state = 6820; break; // &qfr -> &qfr;
				case 6823: state = 6824; break; // &qint -> &qint;
				case 6827: state = 6828; break; // &Qopf -> &Qopf;
				case 6831: state = 6832; break; // &qopf -> &qopf;
				case 6837: state = 6838; break; // &qprime -> &qprime;
				case 6841: state = 6842; break; // &Qscr -> &Qscr;
				case 6845: state = 6846; break; // &qscr -> &qscr;
				case 6856: state = 6857; break; // &quaternions -> &quaternions;
				case 6860: state = 6861; break; // &quatint -> &quatint;
				case 6864: state = 6865; break; // &quest -> &quest;
				case 6867: state = 6868; break; // &questeq -> &questeq;
				case 6871: state = 6872; break; // &QUOT -> &QUOT;
				case 6874: state = 6875; break; // &quot -> &quot;
				case 6880: state = 6881; break; // &rAarr -> &rAarr;
				case 6884: state = 6885; break; // &race -> &race;
				case 6891: state = 6892; break; // &Racute -> &Racute;
				case 6895: state = 6896; break; // &racute -> &racute;
				case 6899: state = 6900; break; // &radic -> &radic;
				case 6906: state = 6907; break; // &raemptyv -> &raemptyv;
				case 6909: state = 6910; break; // &Rang -> &Rang;
				case 6912: state = 6913; break; // &rang -> &rang;
				case 6914: state = 6915; break; // &rangd -> &rangd;
				case 6916: state = 6917; break; // &range -> &range;
				case 6919: state = 6920; break; // &rangle -> &rangle;
				case 6923: state = 6924; break; // &raquo -> &raquo;
				case 6926: state = 6927; break; // &Rarr -> &Rarr;
				case 6929: state = 6930; break; // &rArr -> &rArr;
				case 6932: state = 6933; break; // &rarr -> &rarr;
				case 6935: state = 6936; break; // &rarrap -> &rarrap;
				case 6937: state = 6938; break; // &rarrb -> &rarrb;
				case 6940: state = 6941; break; // &rarrbfs -> &rarrbfs;
				case 6942: state = 6943; break; // &rarrc -> &rarrc;
				case 6945: state = 6946; break; // &rarrfs -> &rarrfs;
				case 6948: state = 6949; break; // &rarrhk -> &rarrhk;
				case 6951: state = 6952; break; // &rarrlp -> &rarrlp;
				case 6954: state = 6955; break; // &rarrpl -> &rarrpl;
				case 6958: state = 6959; break; // &rarrsim -> &rarrsim;
				case 6961: state = 6962; break; // &Rarrtl -> &Rarrtl;
				case 6964: state = 6965; break; // &rarrtl -> &rarrtl;
				case 6966: state = 6967; break; // &rarrw -> &rarrw;
				case 6971: state = 6972; break; // &rAtail -> &rAtail;
				case 6976: state = 6977; break; // &ratail -> &ratail;
				case 6979: state = 6980; break; // &ratio -> &ratio;
				case 6984: state = 6985; break; // &rationals -> &rationals;
				case 6989: state = 6990; break; // &RBarr -> &RBarr;
				case 6994: state = 6995; break; // &rBarr -> &rBarr;
				case 6999: state = 7000; break; // &rbarr -> &rbarr;
				case 7003: state = 7004; break; // &rbbrk -> &rbbrk;
				case 7008: state = 7009; break; // &rbrace -> &rbrace;
				case 7010: state = 7011; break; // &rbrack -> &rbrack;
				case 7013: state = 7014; break; // &rbrke -> &rbrke;
				case 7017: state = 7018; break; // &rbrksld -> &rbrksld;
				case 7019: state = 7020; break; // &rbrkslu -> &rbrkslu;
				case 7025: state = 7026; break; // &Rcaron -> &Rcaron;
				case 7031: state = 7032; break; // &rcaron -> &rcaron;
				case 7036: state = 7037; break; // &Rcedil -> &Rcedil;
				case 7041: state = 7042; break; // &rcedil -> &rcedil;
				case 7044: state = 7045; break; // &rceil -> &rceil;
				case 7047: state = 7048; break; // &rcub -> &rcub;
				case 7049: state = 7050; break; // &Rcy -> &Rcy;
				case 7051: state = 7052; break; // &rcy -> &rcy;
				case 7055: state = 7056; break; // &rdca -> &rdca;
				case 7061: state = 7062; break; // &rdldhar -> &rdldhar;
				case 7065: state = 7066; break; // &rdquo -> &rdquo;
				case 7067: state = 7068; break; // &rdquor -> &rdquor;
				case 7070: state = 7071; break; // &rdsh -> &rdsh;
				case 7072: state = 7073; break; // &Re -> &Re;
				case 7076: state = 7077; break; // &real -> &real;
				case 7080: state = 7081; break; // &realine -> &realine;
				case 7085: state = 7086; break; // &realpart -> &realpart;
				case 7087: state = 7088; break; // &reals -> &reals;
				case 7090: state = 7091; break; // &rect -> &rect;
				case 7093: state = 7094; break; // &REG -> &REG;
				case 7095: state = 7096; break; // &reg -> &reg;
				case 7108: state = 7109; break; // &ReverseElement -> &ReverseElement;
				case 7119: state = 7120; break; // &ReverseEquilibrium -> &ReverseEquilibrium;
				case 7133: state = 7134; break; // &ReverseUpEquilibrium -> &ReverseUpEquilibrium;
				case 7139: state = 7140; break; // &rfisht -> &rfisht;
				case 7144: state = 7145; break; // &rfloor -> &rfloor;
				case 7147: state = 7148; break; // &Rfr -> &Rfr;
				case 7149: state = 7150; break; // &rfr -> &rfr;
				case 7153: state = 7154; break; // &rHar -> &rHar;
				case 7158: state = 7159; break; // &rhard -> &rhard;
				case 7160: state = 7161; break; // &rharu -> &rharu;
				case 7162: state = 7163; break; // &rharul -> &rharul;
				case 7165: state = 7166; break; // &Rho -> &Rho;
				case 7167: state = 7168; break; // &rho -> &rho;
				case 7169: state = 7170; break; // &rhov -> &rhov;
				case 7186: state = 7187; break; // &RightAngleBracket -> &RightAngleBracket;
				case 7191: state = 7192; break; // &RightArrow -> &RightArrow;
				case 7197: state = 7198; break; // &Rightarrow -> &Rightarrow;
				case 7207: state = 7208; break; // &rightarrow -> &rightarrow;
				case 7211: state = 7212; break; // &RightArrowBar -> &RightArrowBar;
				case 7221: state = 7222; break; // &RightArrowLeftArrow -> &RightArrowLeftArrow;
				case 7226: state = 7227; break; // &rightarrowtail -> &rightarrowtail;
				case 7234: state = 7235; break; // &RightCeiling -> &RightCeiling;
				case 7248: state = 7249; break; // &RightDoubleBracket -> &RightDoubleBracket;
				case 7260: state = 7261; break; // &RightDownTeeVector -> &RightDownTeeVector;
				case 7267: state = 7268; break; // &RightDownVector -> &RightDownVector;
				case 7271: state = 7272; break; // &RightDownVectorBar -> &RightDownVectorBar;
				case 7277: state = 7278; break; // &RightFloor -> &RightFloor;
				case 7289: state = 7290; break; // &rightharpoondown -> &rightharpoondown;
				case 7292: state = 7293; break; // &rightharpoonup -> &rightharpoonup;
				case 7303: state = 7304; break; // &rightleftarrows -> &rightleftarrows;
				case 7312: state = 7313; break; // &rightleftharpoons -> &rightleftharpoons;
				case 7324: state = 7325; break; // &rightrightarrows -> &rightrightarrows;
				case 7335: state = 7336; break; // &rightsquigarrow -> &rightsquigarrow;
				case 7339: state = 7340; break; // &RightTee -> &RightTee;
				case 7345: state = 7346; break; // &RightTeeArrow -> &RightTeeArrow;
				case 7352: state = 7353; break; // &RightTeeVector -> &RightTeeVector;
				case 7363: state = 7364; break; // &rightthreetimes -> &rightthreetimes;
				case 7371: state = 7372; break; // &RightTriangle -> &RightTriangle;
				case 7375: state = 7376; break; // &RightTriangleBar -> &RightTriangleBar;
				case 7381: state = 7382; break; // &RightTriangleEqual -> &RightTriangleEqual;
				case 7394: state = 7395; break; // &RightUpDownVector -> &RightUpDownVector;
				case 7404: state = 7405; break; // &RightUpTeeVector -> &RightUpTeeVector;
				case 7411: state = 7412; break; // &RightUpVector -> &RightUpVector;
				case 7415: state = 7416; break; // &RightUpVectorBar -> &RightUpVectorBar;
				case 7422: state = 7423; break; // &RightVector -> &RightVector;
				case 7426: state = 7427; break; // &RightVectorBar -> &RightVectorBar;
				case 7429: state = 7430; break; // &ring -> &ring;
				case 7440: state = 7441; break; // &risingdotseq -> &risingdotseq;
				case 7445: state = 7446; break; // &rlarr -> &rlarr;
				case 7449: state = 7450; break; // &rlhar -> &rlhar;
				case 7451: state = 7452; break; // &rlm -> &rlm;
				case 7457: state = 7458; break; // &rmoust -> &rmoust;
				case 7462: state = 7463; break; // &rmoustache -> &rmoustache;
				case 7467: state = 7468; break; // &rnmid -> &rnmid;
				case 7472: state = 7473; break; // &roang -> &roang;
				case 7475: state = 7476; break; // &roarr -> &roarr;
				case 7479: state = 7480; break; // &robrk -> &robrk;
				case 7483: state = 7484; break; // &ropar -> &ropar;
				case 7487: state = 7488; break; // &Ropf -> &Ropf;
				case 7489: state = 7490; break; // &ropf -> &ropf;
				case 7493: state = 7494; break; // &roplus -> &roplus;
				case 7499: state = 7500; break; // &rotimes -> &rotimes;
				case 7510: state = 7511; break; // &RoundImplies -> &RoundImplies;
				case 7514: state = 7515; break; // &rpar -> &rpar;
				case 7517: state = 7518; break; // &rpargt -> &rpargt;
				case 7524: state = 7525; break; // &rppolint -> &rppolint;
				case 7529: state = 7530; break; // &rrarr -> &rrarr;
				case 7540: state = 7541; break; // &Rrightarrow -> &Rrightarrow;
				case 7546: state = 7547; break; // &rsaquo -> &rsaquo;
				case 7550: state = 7551; break; // &Rscr -> &Rscr;
				case 7553: state = 7554; break; // &rscr -> &rscr;
				case 7555: state = 7556; break; // &Rsh -> &Rsh;
				case 7557: state = 7558; break; // &rsh -> &rsh;
				case 7560: state = 7561; break; // &rsqb -> &rsqb;
				case 7563: state = 7564; break; // &rsquo -> &rsquo;
				case 7565: state = 7566; break; // &rsquor -> &rsquor;
				case 7571: state = 7572; break; // &rthree -> &rthree;
				case 7576: state = 7577; break; // &rtimes -> &rtimes;
				case 7579: state = 7580; break; // &rtri -> &rtri;
				case 7581: state = 7582; break; // &rtrie -> &rtrie;
				case 7583: state = 7584; break; // &rtrif -> &rtrif;
				case 7588: state = 7589; break; // &rtriltri -> &rtriltri;
				case 7599: state = 7600; break; // &RuleDelayed -> &RuleDelayed;
				case 7606: state = 7607; break; // &ruluhar -> &ruluhar;
				case 7608: state = 7609; break; // &rx -> &rx;
				case 7615: state = 7616; break; // &Sacute -> &Sacute;
				case 7622: state = 7623; break; // &sacute -> &sacute;
				case 7627: state = 7628; break; // &sbquo -> &sbquo;
				case 7629: state = 7630; break; // &Sc -> &Sc;
				case 7631: state = 7632; break; // &sc -> &sc;
				case 7634: state = 7635; break; // &scap -> &scap;
				case 7639: state = 7640; break; // &Scaron -> &Scaron;
				case 7643: state = 7644; break; // &scaron -> &scaron;
				case 7647: state = 7648; break; // &sccue -> &sccue;
				case 7649: state = 7650; break; // &scE -> &scE;
				case 7651: state = 7652; break; // &sce -> &sce;
				case 7656: state = 7657; break; // &Scedil -> &Scedil;
				case 7660: state = 7661; break; // &scedil -> &scedil;
				case 7664: state = 7665; break; // &Scirc -> &Scirc;
				case 7668: state = 7669; break; // &scirc -> &scirc;
				case 7672: state = 7673; break; // &scnap -> &scnap;
				case 7674: state = 7675; break; // &scnE -> &scnE;
				case 7678: state = 7679; break; // &scnsim -> &scnsim;
				case 7685: state = 7686; break; // &scpolint -> &scpolint;
				case 7689: state = 7690; break; // &scsim -> &scsim;
				case 7691: state = 7692; break; // &Scy -> &Scy;
				case 7693: state = 7694; break; // &scy -> &scy;
				case 7697: state = 7698; break; // &sdot -> &sdot;
				case 7699: state = 7700; break; // &sdotb -> &sdotb;
				case 7701: state = 7702; break; // &sdote -> &sdote;
				case 7707: state = 7708; break; // &searhk -> &searhk;
				case 7711: state = 7712; break; // &seArr -> &seArr;
				case 7713: state = 7714; break; // &searr -> &searr;
				case 7716: state = 7717; break; // &searrow -> &searrow;
				case 7719: state = 7720; break; // &sect -> &sect;
				case 7722: state = 7723; break; // &semi -> &semi;
				case 7727: state = 7728; break; // &seswar -> &seswar;
				case 7734: state = 7735; break; // &setminus -> &setminus;
				case 7736: state = 7737; break; // &setmn -> &setmn;
				case 7739: state = 7740; break; // &sext -> &sext;
				case 7742: state = 7743; break; // &Sfr -> &Sfr;
				case 7745: state = 7746; break; // &sfr -> &sfr;
				case 7749: state = 7750; break; // &sfrown -> &sfrown;
				case 7754: state = 7755; break; // &sharp -> &sharp;
				case 7760: state = 7761; break; // &SHCHcy -> &SHCHcy;
				case 7765: state = 7766; break; // &shchcy -> &shchcy;
				case 7768: state = 7769; break; // &SHcy -> &SHcy;
				case 7770: state = 7771; break; // &shcy -> &shcy;
				case 7784: state = 7785; break; // &ShortDownArrow -> &ShortDownArrow;
				case 7794: state = 7795; break; // &ShortLeftArrow -> &ShortLeftArrow;
				case 7801: state = 7802; break; // &shortmid -> &shortmid;
				case 7810: state = 7811; break; // &shortparallel -> &shortparallel;
				case 7821: state = 7822; break; // &ShortRightArrow -> &ShortRightArrow;
				case 7829: state = 7830; break; // &ShortUpArrow -> &ShortUpArrow;
				case 7831: state = 7832; break; // &shy -> &shy;
				case 7836: state = 7837; break; // &Sigma -> &Sigma;
				case 7841: state = 7842; break; // &sigma -> &sigma;
				case 7843: state = 7844; break; // &sigmaf -> &sigmaf;
				case 7845: state = 7846; break; // &sigmav -> &sigmav;
				case 7847: state = 7848; break; // &sim -> &sim;
				case 7851: state = 7852; break; // &simdot -> &simdot;
				case 7853: state = 7854; break; // &sime -> &sime;
				case 7855: state = 7856; break; // &simeq -> &simeq;
				case 7857: state = 7858; break; // &simg -> &simg;
				case 7859: state = 7860; break; // &simgE -> &simgE;
				case 7861: state = 7862; break; // &siml -> &siml;
				case 7863: state = 7864; break; // &simlE -> &simlE;
				case 7866: state = 7867; break; // &simne -> &simne;
				case 7871: state = 7872; break; // &simplus -> &simplus;
				case 7876: state = 7877; break; // &simrarr -> &simrarr;
				case 7881: state = 7882; break; // &slarr -> &slarr;
				case 7892: state = 7893; break; // &SmallCircle -> &SmallCircle;
				case 7905: state = 7906; break; // &smallsetminus -> &smallsetminus;
				case 7909: state = 7910; break; // &smashp -> &smashp;
				case 7916: state = 7917; break; // &smeparsl -> &smeparsl;
				case 7919: state = 7920; break; // &smid -> &smid;
				case 7922: state = 7923; break; // &smile -> &smile;
				case 7924: state = 7925; break; // &smt -> &smt;
				case 7926: state = 7927; break; // &smte -> &smte;
				case 7928: state = 7929; break; // &smtes -> &smtes;
				case 7934: state = 7935; break; // &SOFTcy -> &SOFTcy;
				case 7940: state = 7941; break; // &softcy -> &softcy;
				case 7942: state = 7943; break; // &sol -> &sol;
				case 7944: state = 7945; break; // &solb -> &solb;
				case 7947: state = 7948; break; // &solbar -> &solbar;
				case 7951: state = 7952; break; // &Sopf -> &Sopf;
				case 7954: state = 7955; break; // &sopf -> &sopf;
				case 7960: state = 7961; break; // &spades -> &spades;
				case 7964: state = 7965; break; // &spadesuit -> &spadesuit;
				case 7966: state = 7967; break; // &spar -> &spar;
				case 7971: state = 7972; break; // &sqcap -> &sqcap;
				case 7973: state = 7974; break; // &sqcaps -> &sqcaps;
				case 7976: state = 7977; break; // &sqcup -> &sqcup;
				case 7978: state = 7979; break; // &sqcups -> &sqcups;
				case 7982: state = 7983; break; // &Sqrt -> &Sqrt;
				case 7986: state = 7987; break; // &sqsub -> &sqsub;
				case 7988: state = 7989; break; // &sqsube -> &sqsube;
				case 7992: state = 7993; break; // &sqsubset -> &sqsubset;
				case 7995: state = 7996; break; // &sqsubseteq -> &sqsubseteq;
				case 7997: state = 7998; break; // &sqsup -> &sqsup;
				case 7999: state = 8000; break; // &sqsupe -> &sqsupe;
				case 8003: state = 8004; break; // &sqsupset -> &sqsupset;
				case 8006: state = 8007; break; // &sqsupseteq -> &sqsupseteq;
				case 8008: state = 8009; break; // &squ -> &squ;
				case 8013: state = 8014; break; // &Square -> &Square;
				case 8017: state = 8018; break; // &square -> &square;
				case 8030: state = 8031; break; // &SquareIntersection -> &SquareIntersection;
				case 8037: state = 8038; break; // &SquareSubset -> &SquareSubset;
				case 8043: state = 8044; break; // &SquareSubsetEqual -> &SquareSubsetEqual;
				case 8050: state = 8051; break; // &SquareSuperset -> &SquareSuperset;
				case 8056: state = 8057; break; // &SquareSupersetEqual -> &SquareSupersetEqual;
				case 8062: state = 8063; break; // &SquareUnion -> &SquareUnion;
				case 8064: state = 8065; break; // &squarf -> &squarf;
				case 8066: state = 8067; break; // &squf -> &squf;
				case 8071: state = 8072; break; // &srarr -> &srarr;
				case 8075: state = 8076; break; // &Sscr -> &Sscr;
				case 8079: state = 8080; break; // &sscr -> &sscr;
				case 8084: state = 8085; break; // &ssetmn -> &ssetmn;
				case 8089: state = 8090; break; // &ssmile -> &ssmile;
				case 8094: state = 8095; break; // &sstarf -> &sstarf;
				case 8098: state = 8099; break; // &Star -> &Star;
				case 8102: state = 8103; break; // &star -> &star;
				case 8104: state = 8105; break; // &starf -> &starf;
				case 8118: state = 8119; break; // &straightepsilon -> &straightepsilon;
				case 8122: state = 8123; break; // &straightphi -> &straightphi;
				case 8125: state = 8126; break; // &strns -> &strns;
				case 8128: state = 8129; break; // &Sub -> &Sub;
				case 8131: state = 8132; break; // &sub -> &sub;
				case 8135: state = 8136; break; // &subdot -> &subdot;
				case 8137: state = 8138; break; // &subE -> &subE;
				case 8139: state = 8140; break; // &sube -> &sube;
				case 8143: state = 8144; break; // &subedot -> &subedot;
				case 8148: state = 8149; break; // &submult -> &submult;
				case 8151: state = 8152; break; // &subnE -> &subnE;
				case 8153: state = 8154; break; // &subne -> &subne;
				case 8158: state = 8159; break; // &subplus -> &subplus;
				case 8163: state = 8164; break; // &subrarr -> &subrarr;
				case 8167: state = 8168; break; // &Subset -> &Subset;
				case 8171: state = 8172; break; // &subset -> &subset;
				case 8174: state = 8175; break; // &subseteq -> &subseteq;
				case 8176: state = 8177; break; // &subseteqq -> &subseteqq;
				case 8182: state = 8183; break; // &SubsetEqual -> &SubsetEqual;
				case 8186: state = 8187; break; // &subsetneq -> &subsetneq;
				case 8188: state = 8189; break; // &subsetneqq -> &subsetneqq;
				case 8191: state = 8192; break; // &subsim -> &subsim;
				case 8194: state = 8195; break; // &subsub -> &subsub;
				case 8196: state = 8197; break; // &subsup -> &subsup;
				case 8199: state = 8200; break; // &succ -> &succ;
				case 8206: state = 8207; break; // &succapprox -> &succapprox;
				case 8214: state = 8215; break; // &succcurlyeq -> &succcurlyeq;
				case 8221: state = 8222; break; // &Succeeds -> &Succeeds;
				case 8227: state = 8228; break; // &SucceedsEqual -> &SucceedsEqual;
				case 8238: state = 8239; break; // &SucceedsSlantEqual -> &SucceedsSlantEqual;
				case 8244: state = 8245; break; // &SucceedsTilde -> &SucceedsTilde;
				case 8247: state = 8248; break; // &succeq -> &succeq;
				case 8255: state = 8256; break; // &succnapprox -> &succnapprox;
				case 8259: state = 8260; break; // &succneqq -> &succneqq;
				case 8263: state = 8264; break; // &succnsim -> &succnsim;
				case 8267: state = 8268; break; // &succsim -> &succsim;
				case 8273: state = 8274; break; // &SuchThat -> &SuchThat;
				case 8275: state = 8276; break; // &Sum -> &Sum;
				case 8277: state = 8278; break; // &sum -> &sum;
				case 8280: state = 8281; break; // &sung -> &sung;
				case 8282: state = 8283; break; // &Sup -> &Sup;
				case 8284: state = 8285; break; // &sup -> &sup;
				case 8286: state = 8287; break; // &sup1 -> &sup1;
				case 8288: state = 8289; break; // &sup2 -> &sup2;
				case 8290: state = 8291; break; // &sup3 -> &sup3;
				case 8294: state = 8295; break; // &supdot -> &supdot;
				case 8298: state = 8299; break; // &supdsub -> &supdsub;
				case 8300: state = 8301; break; // &supE -> &supE;
				case 8302: state = 8303; break; // &supe -> &supe;
				case 8306: state = 8307; break; // &supedot -> &supedot;
				case 8312: state = 8313; break; // &Superset -> &Superset;
				case 8318: state = 8319; break; // &SupersetEqual -> &SupersetEqual;
				case 8323: state = 8324; break; // &suphsol -> &suphsol;
				case 8326: state = 8327; break; // &suphsub -> &suphsub;
				case 8331: state = 8332; break; // &suplarr -> &suplarr;
				case 8336: state = 8337; break; // &supmult -> &supmult;
				case 8339: state = 8340; break; // &supnE -> &supnE;
				case 8341: state = 8342; break; // &supne -> &supne;
				case 8346: state = 8347; break; // &supplus -> &supplus;
				case 8350: state = 8351; break; // &Supset -> &Supset;
				case 8354: state = 8355; break; // &supset -> &supset;
				case 8357: state = 8358; break; // &supseteq -> &supseteq;
				case 8359: state = 8360; break; // &supseteqq -> &supseteqq;
				case 8363: state = 8364; break; // &supsetneq -> &supsetneq;
				case 8365: state = 8366; break; // &supsetneqq -> &supsetneqq;
				case 8368: state = 8369; break; // &supsim -> &supsim;
				case 8371: state = 8372; break; // &supsub -> &supsub;
				case 8373: state = 8374; break; // &supsup -> &supsup;
				case 8379: state = 8380; break; // &swarhk -> &swarhk;
				case 8383: state = 8384; break; // &swArr -> &swArr;
				case 8385: state = 8386; break; // &swarr -> &swarr;
				case 8388: state = 8389; break; // &swarrow -> &swarrow;
				case 8393: state = 8394; break; // &swnwar -> &swnwar;
				case 8398: state = 8399; break; // &szlig -> &szlig;
				case 8402: state = 8403; break; // &Tab -> &Tab;
				case 8409: state = 8410; break; // &target -> &target;
				case 8411: state = 8412; break; // &Tau -> &Tau;
				case 8413: state = 8414; break; // &tau -> &tau;
				case 8417: state = 8418; break; // &tbrk -> &tbrk;
				case 8423: state = 8424; break; // &Tcaron -> &Tcaron;
				case 8429: state = 8430; break; // &tcaron -> &tcaron;
				case 8434: state = 8435; break; // &Tcedil -> &Tcedil;
				case 8439: state = 8440; break; // &tcedil -> &tcedil;
				case 8441: state = 8442; break; // &Tcy -> &Tcy;
				case 8443: state = 8444; break; // &tcy -> &tcy;
				case 8447: state = 8448; break; // &tdot -> &tdot;
				case 8453: state = 8454; break; // &telrec -> &telrec;
				case 8456: state = 8457; break; // &Tfr -> &Tfr;
				case 8459: state = 8460; break; // &tfr -> &tfr;
				case 8465: state = 8466; break; // &there4 -> &there4;
				case 8474: state = 8475; break; // &Therefore -> &Therefore;
				case 8479: state = 8480; break; // &therefore -> &therefore;
				case 8482: state = 8483; break; // &Theta -> &Theta;
				case 8485: state = 8486; break; // &theta -> &theta;
				case 8489: state = 8490; break; // &thetasym -> &thetasym;
				case 8491: state = 8492; break; // &thetav -> &thetav;
				case 8501: state = 8502; break; // &thickapprox -> &thickapprox;
				case 8505: state = 8506; break; // &thicksim -> &thicksim;
				case 8514: state = 8515; break; // &ThickSpace -> &ThickSpace;
				case 8518: state = 8519; break; // &thinsp -> &thinsp;
				case 8525: state = 8526; break; // &ThinSpace -> &ThinSpace;
				case 8529: state = 8530; break; // &thkap -> &thkap;
				case 8533: state = 8534; break; // &thksim -> &thksim;
				case 8538: state = 8539; break; // &THORN -> &THORN;
				case 8542: state = 8543; break; // &thorn -> &thorn;
				case 8547: state = 8548; break; // &Tilde -> &Tilde;
				case 8552: state = 8553; break; // &tilde -> &tilde;
				case 8558: state = 8559; break; // &TildeEqual -> &TildeEqual;
				case 8568: state = 8569; break; // &TildeFullEqual -> &TildeFullEqual;
				case 8574: state = 8575; break; // &TildeTilde -> &TildeTilde;
				case 8578: state = 8579; break; // &times -> &times;
				case 8580: state = 8581; break; // &timesb -> &timesb;
				case 8583: state = 8584; break; // &timesbar -> &timesbar;
				case 8585: state = 8586; break; // &timesd -> &timesd;
				case 8588: state = 8589; break; // &tint -> &tint;
				case 8592: state = 8593; break; // &toea -> &toea;
				case 8594: state = 8595; break; // &top -> &top;
				case 8598: state = 8599; break; // &topbot -> &topbot;
				case 8602: state = 8603; break; // &topcir -> &topcir;
				case 8606: state = 8607; break; // &Topf -> &Topf;
				case 8608: state = 8609; break; // &topf -> &topf;
				case 8612: state = 8613; break; // &topfork -> &topfork;
				case 8615: state = 8616; break; // &tosa -> &tosa;
				case 8621: state = 8622; break; // &tprime -> &tprime;
				case 8626: state = 8627; break; // &TRADE -> &TRADE;
				case 8631: state = 8632; break; // &trade -> &trade;
				case 8638: state = 8639; break; // &triangle -> &triangle;
				case 8643: state = 8644; break; // &triangledown -> &triangledown;
				case 8648: state = 8649; break; // &triangleleft -> &triangleleft;
				case 8651: state = 8652; break; // &trianglelefteq -> &trianglelefteq;
				case 8653: state = 8654; break; // &triangleq -> &triangleq;
				case 8659: state = 8660; break; // &triangleright -> &triangleright;
				case 8662: state = 8663; break; // &trianglerighteq -> &trianglerighteq;
				case 8666: state = 8667; break; // &tridot -> &tridot;
				case 8668: state = 8669; break; // &trie -> &trie;
				case 8674: state = 8675; break; // &triminus -> &triminus;
				case 8683: state = 8684; break; // &TripleDot -> &TripleDot;
				case 8688: state = 8689; break; // &triplus -> &triplus;
				case 8691: state = 8692; break; // &trisb -> &trisb;
				case 8696: state = 8697; break; // &tritime -> &tritime;
				case 8703: state = 8704; break; // &trpezium -> &trpezium;
				case 8707: state = 8708; break; // &Tscr -> &Tscr;
				case 8711: state = 8712; break; // &tscr -> &tscr;
				case 8715: state = 8716; break; // &TScy -> &TScy;
				case 8717: state = 8718; break; // &tscy -> &tscy;
				case 8721: state = 8722; break; // &TSHcy -> &TSHcy;
				case 8725: state = 8726; break; // &tshcy -> &tshcy;
				case 8730: state = 8731; break; // &Tstrok -> &Tstrok;
				case 8735: state = 8736; break; // &tstrok -> &tstrok;
				case 8740: state = 8741; break; // &twixt -> &twixt;
				case 8755: state = 8756; break; // &twoheadleftarrow -> &twoheadleftarrow;
				case 8766: state = 8767; break; // &twoheadrightarrow -> &twoheadrightarrow;
				case 8773: state = 8774; break; // &Uacute -> &Uacute;
				case 8780: state = 8781; break; // &uacute -> &uacute;
				case 8783: state = 8784; break; // &Uarr -> &Uarr;
				case 8787: state = 8788; break; // &uArr -> &uArr;
				case 8790: state = 8791; break; // &uarr -> &uarr;
				case 8795: state = 8796; break; // &Uarrocir -> &Uarrocir;
				case 8800: state = 8801; break; // &Ubrcy -> &Ubrcy;
				case 8805: state = 8806; break; // &ubrcy -> &ubrcy;
				case 8809: state = 8810; break; // &Ubreve -> &Ubreve;
				case 8813: state = 8814; break; // &ubreve -> &ubreve;
				case 8818: state = 8819; break; // &Ucirc -> &Ucirc;
				case 8823: state = 8824; break; // &ucirc -> &ucirc;
				case 8825: state = 8826; break; // &Ucy -> &Ucy;
				case 8827: state = 8828; break; // &ucy -> &ucy;
				case 8832: state = 8833; break; // &udarr -> &udarr;
				case 8838: state = 8839; break; // &Udblac -> &Udblac;
				case 8843: state = 8844; break; // &udblac -> &udblac;
				case 8847: state = 8848; break; // &udhar -> &udhar;
				case 8853: state = 8854; break; // &ufisht -> &ufisht;
				case 8856: state = 8857; break; // &Ufr -> &Ufr;
				case 8858: state = 8859; break; // &ufr -> &ufr;
				case 8864: state = 8865; break; // &Ugrave -> &Ugrave;
				case 8870: state = 8871; break; // &ugrave -> &ugrave;
				case 8874: state = 8875; break; // &uHar -> &uHar;
				case 8879: state = 8880; break; // &uharl -> &uharl;
				case 8881: state = 8882; break; // &uharr -> &uharr;
				case 8885: state = 8886; break; // &uhblk -> &uhblk;
				case 8891: state = 8892; break; // &ulcorn -> &ulcorn;
				case 8894: state = 8895; break; // &ulcorner -> &ulcorner;
				case 8898: state = 8899; break; // &ulcrop -> &ulcrop;
				case 8902: state = 8903; break; // &ultri -> &ultri;
				case 8907: state = 8908; break; // &Umacr -> &Umacr;
				case 8912: state = 8913; break; // &umacr -> &umacr;
				case 8914: state = 8915; break; // &uml -> &uml;
				case 8922: state = 8923; break; // &UnderBar -> &UnderBar;
				case 8927: state = 8928; break; // &UnderBrace -> &UnderBrace;
				case 8931: state = 8932; break; // &UnderBracket -> &UnderBracket;
				case 8943: state = 8944; break; // &UnderParenthesis -> &UnderParenthesis;
				case 8947: state = 8948; break; // &Union -> &Union;
				case 8952: state = 8953; break; // &UnionPlus -> &UnionPlus;
				case 8957: state = 8958; break; // &Uogon -> &Uogon;
				case 8962: state = 8963; break; // &uogon -> &uogon;
				case 8965: state = 8966; break; // &Uopf -> &Uopf;
				case 8968: state = 8969; break; // &uopf -> &uopf;
				case 8975: state = 8976; break; // &UpArrow -> &UpArrow;
				case 8981: state = 8982; break; // &Uparrow -> &Uparrow;
				case 8988: state = 8989; break; // &uparrow -> &uparrow;
				case 8992: state = 8993; break; // &UpArrowBar -> &UpArrowBar;
				case 9002: state = 9003; break; // &UpArrowDownArrow -> &UpArrowDownArrow;
				case 9012: state = 9013; break; // &UpDownArrow -> &UpDownArrow;
				case 9022: state = 9023; break; // &Updownarrow -> &Updownarrow;
				case 9032: state = 9033; break; // &updownarrow -> &updownarrow;
				case 9044: state = 9045; break; // &UpEquilibrium -> &UpEquilibrium;
				case 9056: state = 9057; break; // &upharpoonleft -> &upharpoonleft;
				case 9062: state = 9063; break; // &upharpoonright -> &upharpoonright;
				case 9066: state = 9067; break; // &uplus -> &uplus;
				case 9079: state = 9080; break; // &UpperLeftArrow -> &UpperLeftArrow;
				case 9090: state = 9091; break; // &UpperRightArrow -> &UpperRightArrow;
				case 9093: state = 9094; break; // &Upsi -> &Upsi;
				case 9096: state = 9097; break; // &upsi -> &upsi;
				case 9098: state = 9099; break; // &upsih -> &upsih;
				case 9102: state = 9103; break; // &Upsilon -> &Upsilon;
				case 9106: state = 9107; break; // &upsilon -> &upsilon;
				case 9110: state = 9111; break; // &UpTee -> &UpTee;
				case 9116: state = 9117; break; // &UpTeeArrow -> &UpTeeArrow;
				case 9125: state = 9126; break; // &upuparrows -> &upuparrows;
				case 9131: state = 9132; break; // &urcorn -> &urcorn;
				case 9134: state = 9135; break; // &urcorner -> &urcorner;
				case 9138: state = 9139; break; // &urcrop -> &urcrop;
				case 9143: state = 9144; break; // &Uring -> &Uring;
				case 9147: state = 9148; break; // &uring -> &uring;
				case 9151: state = 9152; break; // &urtri -> &urtri;
				case 9155: state = 9156; break; // &Uscr -> &Uscr;
				case 9159: state = 9160; break; // &uscr -> &uscr;
				case 9164: state = 9165; break; // &utdot -> &utdot;
				case 9170: state = 9171; break; // &Utilde -> &Utilde;
				case 9175: state = 9176; break; // &utilde -> &utilde;
				case 9178: state = 9179; break; // &utri -> &utri;
				case 9180: state = 9181; break; // &utrif -> &utrif;
				case 9185: state = 9186; break; // &uuarr -> &uuarr;
				case 9189: state = 9190; break; // &Uuml -> &Uuml;
				case 9192: state = 9193; break; // &uuml -> &uuml;
				case 9199: state = 9200; break; // &uwangle -> &uwangle;
				case 9206: state = 9207; break; // &vangrt -> &vangrt;
				case 9215: state = 9216; break; // &varepsilon -> &varepsilon;
				case 9221: state = 9222; break; // &varkappa -> &varkappa;
				case 9229: state = 9230; break; // &varnothing -> &varnothing;
				case 9233: state = 9234; break; // &varphi -> &varphi;
				case 9235: state = 9236; break; // &varpi -> &varpi;
				case 9241: state = 9242; break; // &varpropto -> &varpropto;
				case 9245: state = 9246; break; // &vArr -> &vArr;
				case 9247: state = 9248; break; // &varr -> &varr;
				case 9250: state = 9251; break; // &varrho -> &varrho;
				case 9256: state = 9257; break; // &varsigma -> &varsigma;
				case 9265: state = 9266; break; // &varsubsetneq -> &varsubsetneq;
				case 9267: state = 9268; break; // &varsubsetneqq -> &varsubsetneqq;
				case 9275: state = 9276; break; // &varsupsetneq -> &varsupsetneq;
				case 9277: state = 9278; break; // &varsupsetneqq -> &varsupsetneqq;
				case 9283: state = 9284; break; // &vartheta -> &vartheta;
				case 9295: state = 9296; break; // &vartriangleleft -> &vartriangleleft;
				case 9301: state = 9302; break; // &vartriangleright -> &vartriangleright;
				case 9306: state = 9307; break; // &Vbar -> &Vbar;
				case 9310: state = 9311; break; // &vBar -> &vBar;
				case 9312: state = 9313; break; // &vBarv -> &vBarv;
				case 9315: state = 9316; break; // &Vcy -> &Vcy;
				case 9318: state = 9319; break; // &vcy -> &vcy;
				case 9323: state = 9324; break; // &VDash -> &VDash;
				case 9328: state = 9329; break; // &Vdash -> &Vdash;
				case 9333: state = 9334; break; // &vDash -> &vDash;
				case 9338: state = 9339; break; // &vdash -> &vdash;
				case 9340: state = 9341; break; // &Vdashl -> &Vdashl;
				case 9343: state = 9344; break; // &Vee -> &Vee;
				case 9346: state = 9347; break; // &vee -> &vee;
				case 9350: state = 9351; break; // &veebar -> &veebar;
				case 9353: state = 9354; break; // &veeeq -> &veeeq;
				case 9358: state = 9359; break; // &vellip -> &vellip;
				case 9363: state = 9364; break; // &Verbar -> &Verbar;
				case 9368: state = 9369; break; // &verbar -> &verbar;
				case 9370: state = 9371; break; // &Vert -> &Vert;
				case 9372: state = 9373; break; // &vert -> &vert;
				case 9380: state = 9381; break; // &VerticalBar -> &VerticalBar;
				case 9385: state = 9386; break; // &VerticalLine -> &VerticalLine;
				case 9395: state = 9396; break; // &VerticalSeparator -> &VerticalSeparator;
				case 9401: state = 9402; break; // &VerticalTilde -> &VerticalTilde;
				case 9412: state = 9413; break; // &VeryThinSpace -> &VeryThinSpace;
				case 9415: state = 9416; break; // &Vfr -> &Vfr;
				case 9418: state = 9419; break; // &vfr -> &vfr;
				case 9423: state = 9424; break; // &vltri -> &vltri;
				case 9428: state = 9429; break; // &vnsub -> &vnsub;
				case 9430: state = 9431; break; // &vnsup -> &vnsup;
				case 9434: state = 9435; break; // &Vopf -> &Vopf;
				case 9438: state = 9439; break; // &vopf -> &vopf;
				case 9443: state = 9444; break; // &vprop -> &vprop;
				case 9448: state = 9449; break; // &vrtri -> &vrtri;
				case 9452: state = 9453; break; // &Vscr -> &Vscr;
				case 9456: state = 9457; break; // &vscr -> &vscr;
				case 9461: state = 9462; break; // &vsubnE -> &vsubnE;
				case 9463: state = 9464; break; // &vsubne -> &vsubne;
				case 9467: state = 9468; break; // &vsupnE -> &vsupnE;
				case 9469: state = 9470; break; // &vsupne -> &vsupne;
				case 9475: state = 9476; break; // &Vvdash -> &Vvdash;
				case 9482: state = 9483; break; // &vzigzag -> &vzigzag;
				case 9488: state = 9489; break; // &Wcirc -> &Wcirc;
				case 9494: state = 9495; break; // &wcirc -> &wcirc;
				case 9500: state = 9501; break; // &wedbar -> &wedbar;
				case 9505: state = 9506; break; // &Wedge -> &Wedge;
				case 9508: state = 9509; break; // &wedge -> &wedge;
				case 9510: state = 9511; break; // &wedgeq -> &wedgeq;
				case 9515: state = 9516; break; // &weierp -> &weierp;
				case 9518: state = 9519; break; // &Wfr -> &Wfr;
				case 9521: state = 9522; break; // &wfr -> &wfr;
				case 9525: state = 9526; break; // &Wopf -> &Wopf;
				case 9529: state = 9530; break; // &wopf -> &wopf;
				case 9531: state = 9532; break; // &wp -> &wp;
				case 9533: state = 9534; break; // &wr -> &wr;
				case 9538: state = 9539; break; // &wreath -> &wreath;
				case 9542: state = 9543; break; // &Wscr -> &Wscr;
				case 9546: state = 9547; break; // &wscr -> &wscr;
				case 9551: state = 9552; break; // &xcap -> &xcap;
				case 9555: state = 9556; break; // &xcirc -> &xcirc;
				case 9558: state = 9559; break; // &xcup -> &xcup;
				case 9563: state = 9564; break; // &xdtri -> &xdtri;
				case 9567: state = 9568; break; // &Xfr -> &Xfr;
				case 9570: state = 9571; break; // &xfr -> &xfr;
				case 9575: state = 9576; break; // &xhArr -> &xhArr;
				case 9579: state = 9580; break; // &xharr -> &xharr;
				case 9581: state = 9582; break; // &Xi -> &Xi;
				case 9583: state = 9584; break; // &xi -> &xi;
				case 9588: state = 9589; break; // &xlArr -> &xlArr;
				case 9592: state = 9593; break; // &xlarr -> &xlarr;
				case 9596: state = 9597; break; // &xmap -> &xmap;
				case 9600: state = 9601; break; // &xnis -> &xnis;
				case 9605: state = 9606; break; // &xodot -> &xodot;
				case 9609: state = 9610; break; // &Xopf -> &Xopf;
				case 9612: state = 9613; break; // &xopf -> &xopf;
				case 9616: state = 9617; break; // &xoplus -> &xoplus;
				case 9621: state = 9622; break; // &xotime -> &xotime;
				case 9626: state = 9627; break; // &xrArr -> &xrArr;
				case 9630: state = 9631; break; // &xrarr -> &xrarr;
				case 9634: state = 9635; break; // &Xscr -> &Xscr;
				case 9638: state = 9639; break; // &xscr -> &xscr;
				case 9643: state = 9644; break; // &xsqcup -> &xsqcup;
				case 9649: state = 9650; break; // &xuplus -> &xuplus;
				case 9653: state = 9654; break; // &xutri -> &xutri;
				case 9657: state = 9658; break; // &xvee -> &xvee;
				case 9663: state = 9664; break; // &xwedge -> &xwedge;
				case 9670: state = 9671; break; // &Yacute -> &Yacute;
				case 9677: state = 9678; break; // &yacute -> &yacute;
				case 9681: state = 9682; break; // &YAcy -> &YAcy;
				case 9683: state = 9684; break; // &yacy -> &yacy;
				case 9688: state = 9689; break; // &Ycirc -> &Ycirc;
				case 9693: state = 9694; break; // &ycirc -> &ycirc;
				case 9695: state = 9696; break; // &Ycy -> &Ycy;
				case 9697: state = 9698; break; // &ycy -> &ycy;
				case 9700: state = 9701; break; // &yen -> &yen;
				case 9703: state = 9704; break; // &Yfr -> &Yfr;
				case 9706: state = 9707; break; // &yfr -> &yfr;
				case 9710: state = 9711; break; // &YIcy -> &YIcy;
				case 9714: state = 9715; break; // &yicy -> &yicy;
				case 9718: state = 9719; break; // &Yopf -> &Yopf;
				case 9722: state = 9723; break; // &yopf -> &yopf;
				case 9726: state = 9727; break; // &Yscr -> &Yscr;
				case 9730: state = 9731; break; // &yscr -> &yscr;
				case 9734: state = 9735; break; // &YUcy -> &YUcy;
				case 9738: state = 9739; break; // &yucy -> &yucy;
				case 9742: state = 9743; break; // &Yuml -> &Yuml;
				case 9745: state = 9746; break; // &yuml -> &yuml;
				case 9752: state = 9753; break; // &Zacute -> &Zacute;
				case 9759: state = 9760; break; // &zacute -> &zacute;
				case 9765: state = 9766; break; // &Zcaron -> &Zcaron;
				case 9771: state = 9772; break; // &zcaron -> &zcaron;
				case 9773: state = 9774; break; // &Zcy -> &Zcy;
				case 9775: state = 9776; break; // &zcy -> &zcy;
				case 9779: state = 9780; break; // &Zdot -> &Zdot;
				case 9783: state = 9784; break; // &zdot -> &zdot;
				case 9789: state = 9790; break; // &zeetrf -> &zeetrf;
				case 9803: state = 9804; break; // &ZeroWidthSpace -> &ZeroWidthSpace;
				case 9806: state = 9807; break; // &Zeta -> &Zeta;
				case 9809: state = 9810; break; // &zeta -> &zeta;
				case 9812: state = 9813; break; // &Zfr -> &Zfr;
				case 9815: state = 9816; break; // &zfr -> &zfr;
				case 9819: state = 9820; break; // &ZHcy -> &ZHcy;
				case 9823: state = 9824; break; // &zhcy -> &zhcy;
				case 9830: state = 9831; break; // &zigrarr -> &zigrarr;
				case 9834: state = 9835; break; // &Zopf -> &Zopf;
				case 9838: state = 9839; break; // &zopf -> &zopf;
				case 9842: state = 9843; break; // &Zscr -> &Zscr;
				case 9846: state = 9847; break; // &zscr -> &zscr;
				case 9849: state = 9850; break; // &zwj -> &zwj;
				case 9852: state = 9853; break; // &zwnj -> &zwnj;
				default: return false;
				}
				break;
			case 'A':
				switch (state) {
				case 0: state = 1; break; // & -> &A
				case 1432: state = 1447; break; // &d -> &dA
				case 1566: state = 1567; break; // &Diacritical -> &DiacriticalA
				case 1580: state = 1581; break; // &DiacriticalDouble -> &DiacriticalDoubleA
				case 1769: state = 1770; break; // &DoubleDown -> &DoubleDownA
				case 1779: state = 1780; break; // &DoubleLeft -> &DoubleLeftA
				case 1790: state = 1791; break; // &DoubleLeftRight -> &DoubleLeftRightA
				case 1807: state = 1808; break; // &DoubleLongLeft -> &DoubleLongLeftA
				case 1818: state = 1819; break; // &DoubleLongLeftRight -> &DoubleLongLeftRightA
				case 1829: state = 1830; break; // &DoubleLongRight -> &DoubleLongRightA
				case 1840: state = 1841; break; // &DoubleRight -> &DoubleRightA
				case 1852: state = 1853; break; // &DoubleUp -> &DoubleUpA
				case 1862: state = 1863; break; // &DoubleUpDown -> &DoubleUpDownA
				case 1882: state = 1883; break; // &Down -> &DownA
				case 1908: state = 1909; break; // &DownArrowUp -> &DownArrowUpA
				case 2015: state = 2017; break; // &DownTee -> &DownTeeA
				case 2616: state = 2617; break; // &For -> &ForA
				case 3014: state = 3035; break; // &H -> &HA
				case 3020: state = 3046; break; // &h -> &hA
				case 3692: state = 3693; break; // &l -> &lA
				case 3900: state = 3901; break; // &Left -> &LeftA
				case 3941: state = 3942; break; // &LeftArrowRight -> &LeftArrowRightA
				case 4034: state = 4035; break; // &LeftRight -> &LeftRightA
				case 4094: state = 4096; break; // &LeftTee -> &LeftTeeA
				case 4440: state = 4441; break; // &LongLeft -> &LongLeftA
				case 4473: state = 4474; break; // &LongLeftRight -> &LongLeftRightA
				case 4513: state = 4514; break; // &LongRight -> &LongRightA
				case 4594: state = 4595; break; // &LowerLeft -> &LowerLeftA
				case 4605: state = 4606; break; // &LowerRight -> &LowerRightA
				case 5064: state = 5071; break; // &ne -> &neA
				case 5227: state = 5228; break; // &nh -> &nhA
				case 5256: state = 5257; break; // &nl -> &nlA
				case 5855: state = 5856; break; // &nr -> &nrA
				case 6084: state = 6085; break; // &nvl -> &nvlA
				case 6097: state = 6098; break; // &nvr -> &nvrA
				case 6111: state = 6117; break; // &nw -> &nwA
				case 6876: state = 6877; break; // &r -> &rA
				case 7174: state = 7175; break; // &Right -> &RightA
				case 7216: state = 7217; break; // &RightArrowLeft -> &RightArrowLeftA
				case 7339: state = 7341; break; // &RightTee -> &RightTeeA
				case 7703: state = 7709; break; // &se -> &seA
				case 7779: state = 7780; break; // &ShortDown -> &ShortDownA
				case 7789: state = 7790; break; // &ShortLeft -> &ShortLeftA
				case 7816: state = 7817; break; // &ShortRight -> &ShortRightA
				case 7824: state = 7825; break; // &ShortUp -> &ShortUpA
				case 8375: state = 8381; break; // &sw -> &swA
				case 8623: state = 8624; break; // &TR -> &TRA
				case 8775: state = 8785; break; // &u -> &uA
				case 8970: state = 8971; break; // &Up -> &UpA
				case 8997: state = 8998; break; // &UpArrowDown -> &UpArrowDownA
				case 9007: state = 9008; break; // &UpDown -> &UpDownA
				case 9074: state = 9075; break; // &UpperLeft -> &UpperLeftA
				case 9085: state = 9086; break; // &UpperRight -> &UpperRightA
				case 9110: state = 9112; break; // &UpTee -> &UpTeeA
				case 9201: state = 9243; break; // &v -> &vA
				case 9572: state = 9573; break; // &xh -> &xhA
				case 9585: state = 9586; break; // &xl -> &xlA
				case 9623: state = 9624; break; // &xr -> &xrA
				case 9665: state = 9679; break; // &Y -> &YA
				default: return false;
				}
				break;
			case 'B':
				switch (state) {
				case 0: state = 331; break; // & -> &B
				case 1876: state = 1877; break; // &DoubleVertical -> &DoubleVerticalB
				case 1882: state = 1915; break; // &Down -> &DownB
				case 1887: state = 1903; break; // &DownArrow -> &DownArrowB
				case 1981: state = 1983; break; // &DownLeftVector -> &DownLeftVectorB
				case 2007: state = 2009; break; // &DownRightVector -> &DownRightVectorB
				case 3692: state = 3807; break; // &l -> &lB
				case 3905: state = 3906; break; // &LeftAngle -> &LeftAngleB
				case 3917: state = 3933; break; // &LeftArrow -> &LeftArrowB
				case 3966: state = 3967; break; // &LeftDouble -> &LeftDoubleB
				case 3992: state = 3994; break; // &LeftDownVector -> &LeftDownVectorB
				case 4126: state = 4128; break; // &LeftTriangle -> &LeftTriangleB
				case 4166: state = 4168; break; // &LeftUpVector -> &LeftUpVectorB
				case 4177: state = 4179; break; // &LeftVector -> &LeftVectorB
				case 5347: state = 5348; break; // &No -> &NoB
				case 5354: state = 5355; break; // &Non -> &NonB
				case 5409: state = 5410; break; // &NotDoubleVertical -> &NotDoubleVerticalB
				case 5539: state = 5541; break; // &NotLeftTriangle -> &NotLeftTriangleB
				case 5682: state = 5684; break; // &NotRightTriangle -> &NotRightTriangleB
				case 5816: state = 5817; break; // &NotVertical -> &NotVerticalB
				case 6437: state = 6438; break; // &Over -> &OverB
				case 6876: state = 6991; break; // &r -> &rB
				case 6886: state = 6986; break; // &R -> &RB
				case 7179: state = 7180; break; // &RightAngle -> &RightAngleB
				case 7191: state = 7209; break; // &RightArrow -> &RightArrowB
				case 7241: state = 7242; break; // &RightDouble -> &RightDoubleB
				case 7267: state = 7269; break; // &RightDownVector -> &RightDownVectorB
				case 7371: state = 7373; break; // &RightTriangle -> &RightTriangleB
				case 7411: state = 7413; break; // &RightUpVector -> &RightUpVectorB
				case 7422: state = 7424; break; // &RightVector -> &RightVectorB
				case 8919: state = 8920; break; // &Under -> &UnderB
				case 8975: state = 8990; break; // &UpArrow -> &UpArrowB
				case 9201: state = 9308; break; // &v -> &vB
				case 9377: state = 9378; break; // &Vertical -> &VerticalB
				default: return false;
				}
				break;
			case 'C':
				switch (state) {
				case 0: state = 789; break; // & -> &C
				case 1075: state = 1076; break; // &Clockwise -> &ClockwiseC
				case 1093: state = 1094; break; // &Close -> &CloseC
				case 1230: state = 1231; break; // &Counter -> &CounterC
				case 1239: state = 1240; break; // &CounterClockwise -> &CounterClockwiseC
				case 1316: state = 1326; break; // &Cup -> &CupC
				case 1747: state = 1748; break; // &Double -> &DoubleC
				case 3450: state = 3451; break; // &Invisible -> &InvisibleC
				case 3900: state = 3953; break; // &Left -> &LeftC
				case 5376: state = 5380; break; // &Not -> &NotC
				case 5391: state = 5392; break; // &NotCup -> &NotCupC
				case 6308: state = 6309; break; // &Open -> &OpenC
				case 7174: state = 7228; break; // &Right -> &RightC
				case 7756: state = 7757; break; // &SH -> &SHC
				case 7886: state = 7887; break; // &Small -> &SmallC
				default: return false;
				}
				break;
			case 'D':
				switch (state) {
				case 0: state = 1425; break; // & -> &D
				case 613: state = 618; break; // &box -> &boxD
				case 636: state = 640; break; // &boxH -> &boxHD
				case 638: state = 644; break; // &boxh -> &boxhD
				case 831: state = 832; break; // &Capital -> &CapitalD
				case 843: state = 844; break; // &CapitalDifferential -> &CapitalDifferentialD
				case 939: state = 940; break; // &Center -> &CenterD
				case 1023: state = 1024; break; // &Circle -> &CircleD
				case 1098: state = 1099; break; // &CloseCurly -> &CloseCurlyD
				case 1425: state = 1490; break; // &D -> &DD
				case 1566: state = 1573; break; // &Diacritical -> &DiacriticalD
				case 1630: state = 1631; break; // &Differential -> &DifferentialD
				case 1692: state = 1696; break; // &Dot -> &DotD
				case 1747: state = 1764; break; // &Double -> &DoubleD
				case 1852: state = 1859; break; // &DoubleUp -> &DoubleUpD
				case 2115: state = 2157; break; // &e -> &eD
				case 2157: state = 2158; break; // &eD -> &eDD
				case 2175: state = 2176; break; // &ef -> &efD
				case 2397: state = 2399; break; // &equiv -> &equivD
				case 2399: state = 2400; break; // &equivD -> &equivDD
				case 2409: state = 2414; break; // &er -> &erD
				case 3036: state = 3037; break; // &HAR -> &HARD
				case 3209: state = 3210; break; // &Hump -> &HumpD
				case 3900: state = 3961; break; // &Left -> &LeftD
				case 4139: state = 4140; break; // &LeftUp -> &LeftUpD
				case 4767: state = 4825; break; // &m -> &mD
				case 4825: state = 4826; break; // &mD -> &mDD
				case 5376: state = 5396; break; // &Not -> &NotD
				case 5496: state = 5497; break; // &NotHump -> &NotHumpD
				case 6043: state = 6058; break; // &nv -> &nvD
				case 6047: state = 6048; break; // &nV -> &nVD
				case 6313: state = 6314; break; // &OpenCurly -> &OpenCurlyD
				case 6488: state = 6489; break; // &Partial -> &PartialD
				case 7174: state = 7236; break; // &Right -> &RightD
				case 7384: state = 7385; break; // &RightUp -> &RightUpD
				case 7592: state = 7593; break; // &Rule -> &RuleD
				case 7775: state = 7776; break; // &Short -> &ShortD
				case 8624: state = 8625; break; // &TRA -> &TRAD
				case 8680: state = 8681; break; // &Triple -> &TripleD
				case 8970: state = 9004; break; // &Up -> &UpD
				case 8975: state = 8994; break; // &UpArrow -> &UpArrowD
				case 9201: state = 9330; break; // &v -> &vD
				case 9303: state = 9320; break; // &V -> &VD
				default: return false;
				}
				break;
			case 'E':
				switch (state) {
				case 0: state = 2108; break; // & -> &E
				case 1: state = 50; break; // &A -> &AE
				case 27: state = 31; break; // &ac -> &acE
				case 199: state = 206; break; // &ap -> &apE
				case 775: state = 777; break; // &bump -> &bumpE
				case 979: state = 1049; break; // &cir -> &cirE
				case 1692: state = 1707; break; // &Dot -> &DotE
				case 2490: state = 2491; break; // &Exponential -> &ExponentialE
				case 2701: state = 2763; break; // &g -> &gE
				case 2824: state = 2828; break; // &gl -> &glE
				case 2832: state = 2841; break; // &gn -> &gnE
				case 2871: state = 2872; break; // &Greater -> &GreaterE
				case 2886: state = 2887; break; // &GreaterFull -> &GreaterFullE
				case 2910: state = 2911; break; // &GreaterSlant -> &GreaterSlantE
				case 3011: state = 3012; break; // &gvn -> &gvnE
				case 3209: state = 3219; break; // &Hump -> &HumpE
				case 3236: state = 3269; break; // &I -> &IE
				case 3512: state = 3518; break; // &isin -> &isinE
				case 3692: state = 3894; break; // &l -> &lE
				case 4126: state = 4132; break; // &LeftTriangle -> &LeftTriangleE
				case 4239: state = 4240; break; // &Less -> &LessE
				case 4256: state = 4257; break; // &LessFull -> &LessFullE
				case 4288: state = 4289; break; // &LessSlant -> &LessSlantE
				case 4317: state = 4319; break; // &lg -> &lgE
				case 4401: state = 4410; break; // &ln -> &lnE
				case 4764: state = 4765; break; // &lvn -> &lvnE
				case 4986: state = 4988; break; // &nap -> &napE
				case 5195: state = 5196; break; // &ng -> &ngE
				case 5256: state = 5268; break; // &nl -> &nlE
				case 5376: state = 5414; break; // &Not -> &NotE
				case 5445: state = 5447; break; // &NotGreater -> &NotGreaterE
				case 5456: state = 5457; break; // &NotGreaterFull -> &NotGreaterFullE
				case 5480: state = 5481; break; // &NotGreaterSlant -> &NotGreaterSlantE
				case 5496: state = 5506; break; // &NotHump -> &NotHumpE
				case 5513: state = 5519; break; // &notin -> &notinE
				case 5539: state = 5545; break; // &NotLeftTriangle -> &NotLeftTriangleE
				case 5552: state = 5554; break; // &NotLess -> &NotLessE
				case 5577: state = 5578; break; // &NotLessSlant -> &NotLessSlantE
				case 5637: state = 5639; break; // &NotPrecedes -> &NotPrecedesE
				case 5649: state = 5650; break; // &NotPrecedesSlant -> &NotPrecedesSlantE
				case 5662: state = 5663; break; // &NotReverse -> &NotReverseE
				case 5682: state = 5688; break; // &NotRightTriangle -> &NotRightTriangleE
				case 5705: state = 5707; break; // &NotSquareSubset -> &NotSquareSubsetE
				case 5718: state = 5720; break; // &NotSquareSuperset -> &NotSquareSupersetE
				case 5730: state = 5732; break; // &NotSubset -> &NotSubsetE
				case 5743: state = 5745; break; // &NotSucceeds -> &NotSucceedsE
				case 5755: state = 5756; break; // &NotSucceedsSlant -> &NotSucceedsSlantE
				case 5773: state = 5775; break; // &NotSuperset -> &NotSupersetE
				case 5785: state = 5787; break; // &NotTilde -> &NotTildeE
				case 5796: state = 5797; break; // &NotTildeFull -> &NotTildeFullE
				case 5952: state = 5954; break; // &nsub -> &nsubE
				case 5973: state = 5975; break; // &nsup -> &nsupE
				case 6131: state = 6190; break; // &O -> &OE
				case 6642: state = 6651; break; // &pr -> &prE
				case 6677: state = 6679; break; // &Precedes -> &PrecedesE
				case 6689: state = 6690; break; // &PrecedesSlant -> &PrecedesSlantE
				case 6735: state = 6739; break; // &prn -> &prnE
				case 6886: state = 7092; break; // &R -> &RE
				case 7101: state = 7102; break; // &Reverse -> &ReverseE
				case 7122: state = 7123; break; // &ReverseUp -> &ReverseUpE
				case 7371: state = 7377; break; // &RightTriangle -> &RightTriangleE
				case 7631: state = 7649; break; // &sc -> &scE
				case 7670: state = 7674; break; // &scn -> &scnE
				case 7857: state = 7859; break; // &simg -> &simgE
				case 7861: state = 7863; break; // &siml -> &simlE
				case 8037: state = 8039; break; // &SquareSubset -> &SquareSubsetE
				case 8050: state = 8052; break; // &SquareSuperset -> &SquareSupersetE
				case 8131: state = 8137; break; // &sub -> &subE
				case 8150: state = 8151; break; // &subn -> &subnE
				case 8167: state = 8178; break; // &Subset -> &SubsetE
				case 8221: state = 8223; break; // &Succeeds -> &SucceedsE
				case 8233: state = 8234; break; // &SucceedsSlant -> &SucceedsSlantE
				case 8284: state = 8300; break; // &sup -> &supE
				case 8312: state = 8314; break; // &Superset -> &SupersetE
				case 8338: state = 8339; break; // &supn -> &supnE
				case 8547: state = 8554; break; // &Tilde -> &TildeE
				case 8563: state = 8564; break; // &TildeFull -> &TildeFullE
				case 8625: state = 8626; break; // &TRAD -> &TRADE
				case 8970: state = 9034; break; // &Up -> &UpE
				case 9460: state = 9461; break; // &vsubn -> &vsubnE
				case 9466: state = 9467; break; // &vsupn -> &vsupnE
				default: return false;
				}
				break;
			case 'F':
				switch (state) {
				case 0: state = 2517; break; // & -> &F
				case 219: state = 220; break; // &Apply -> &ApplyF
				case 2871: state = 2883; break; // &Greater -> &GreaterF
				case 3900: state = 3998; break; // &Left -> &LeftF
				case 4239: state = 4253; break; // &Less -> &LessF
				case 5445: state = 5453; break; // &NotGreater -> &NotGreaterF
				case 5785: state = 5793; break; // &NotTilde -> &NotTildeF
				case 7174: state = 7273; break; // &Right -> &RightF
				case 7930: state = 7931; break; // &SO -> &SOF
				case 8547: state = 8560; break; // &Tilde -> &TildeF
				default: return false;
				}
				break;
			case 'G':
				switch (state) {
				case 0: state = 2708; break; // & -> &G
				case 1566: state = 1587; break; // &Diacritical -> &DiacriticalG
				case 2287: state = 2288; break; // &EN -> &ENG
				case 2871: state = 2893; break; // &Greater -> &GreaterG
				case 4239: state = 4263; break; // &Less -> &LessG
				case 4244: state = 4245; break; // &LessEqual -> &LessEqualG
				case 4965: state = 5212; break; // &n -> &nG
				case 5151: state = 5152; break; // &Nested -> &NestedG
				case 5158: state = 5159; break; // &NestedGreater -> &NestedGreaterG
				case 5376: state = 5439; break; // &Not -> &NotG
				case 5445: state = 5463; break; // &NotGreater -> &NotGreaterG
				case 5552: state = 5560; break; // &NotLess -> &NotLessG
				case 5595: state = 5596; break; // &NotNested -> &NotNestedG
				case 5602: state = 5603; break; // &NotNestedGreater -> &NotNestedGreaterG
				case 7092: state = 7093; break; // &RE -> &REG
				default: return false;
				}
				break;
			case 'H':
				switch (state) {
				case 0: state = 3014; break; // & -> &H
				case 613: state = 636; break; // &box -> &boxH
				case 691: state = 695; break; // &boxV -> &boxVH
				case 693: state = 699; break; // &boxv -> &boxvH
				case 789: state = 956; break; // &C -> &CH
				case 1432: state = 1546; break; // &d -> &dH
				case 2442: state = 2443; break; // &ET -> &ETH
				case 3213: state = 3214; break; // &HumpDown -> &HumpDownH
				case 3618: state = 3660; break; // &K -> &KH
				case 3692: state = 4321; break; // &l -> &lH
				case 5376: state = 5493; break; // &Not -> &NotH
				case 5500: state = 5501; break; // &NotHumpDown -> &NotHumpDownH
				case 6043: state = 6073; break; // &nv -> &nvH
				case 6876: state = 7151; break; // &r -> &rH
				case 7610: state = 7756; break; // &S -> &SH
				case 7757: state = 7758; break; // &SHC -> &SHCH
				case 8400: state = 8535; break; // &T -> &TH
				case 8713: state = 8719; break; // &TS -> &TSH
				case 8775: state = 8872; break; // &u -> &uH
				case 9747: state = 9817; break; // &Z -> &ZH
				default: return false;
				}
				break;
			case 'I':
				switch (state) {
				case 0: state = 3236; break; // & -> &I
				case 1082: state = 1083; break; // &ClockwiseContour -> &ClockwiseContourI
				case 1190: state = 1191; break; // &Contour -> &ContourI
				case 1246: state = 1247; break; // &CounterClockwiseContour -> &CounterClockwiseContourI
				case 1754: state = 1755; break; // &DoubleContour -> &DoubleContourI
				case 3349: state = 3350; break; // &Imaginary -> &ImaginaryI
				case 7503: state = 7504; break; // &Round -> &RoundI
				case 8013: state = 8019; break; // &Square -> &SquareI
				case 9665: state = 9708; break; // &Y -> &YI
				default: return false;
				}
				break;
			case 'J':
				switch (state) {
				case 0: state = 3555; break; // & -> &J
				case 1425: state = 1661; break; // &D -> &DJ
				case 2708: state = 2816; break; // &G -> &GJ
				case 3236: state = 3320; break; // &I -> &IJ
				case 3618: state = 3668; break; // &K -> &KJ
				case 3698: state = 4338; break; // &L -> &LJ
				case 4971: state = 5248; break; // &N -> &NJ
				default: return false;
				}
				break;
			case 'K':
				switch (state) {
				case 0: state = 3618; break; // & -> &K
				default: return false;
				}
				break;
			case 'L':
				switch (state) {
				case 0: state = 3698; break; // & -> &L
				case 618: state = 619; break; // &boxD -> &boxDL
				case 623: state = 624; break; // &boxd -> &boxdL
				case 673: state = 674; break; // &boxU -> &boxUL
				case 678: state = 679; break; // &boxu -> &boxuL
				case 691: state = 703; break; // &boxV -> &boxVL
				case 693: state = 707; break; // &boxv -> &boxvL
				case 1747: state = 1776; break; // &Double -> &DoubleL
				case 1803: state = 1804; break; // &DoubleLong -> &DoubleLongL
				case 1882: state = 1950; break; // &Down -> &DownL
				case 2871: state = 2901; break; // &Greater -> &GreaterL
				case 2876: state = 2878; break; // &GreaterEqual -> &GreaterEqualL
				case 3178: state = 3179; break; // &Horizontal -> &HorizontalL
				case 4239: state = 4275; break; // &Less -> &LessL
				case 4436: state = 4437; break; // &Long -> &LongL
				case 4590: state = 4591; break; // &Lower -> &LowerL
				case 4965: state = 5272; break; // &n -> &nL
				case 5151: state = 5167; break; // &Nested -> &NestedL
				case 5170: state = 5171; break; // &NestedLess -> &NestedLessL
				case 5176: state = 5177; break; // &New -> &NewL
				case 5376: state = 5528; break; // &Not -> &NotL
				case 5445: state = 5471; break; // &NotGreater -> &NotGreaterL
				case 5552: state = 5568; break; // &NotLess -> &NotLessL
				case 5595: state = 5611; break; // &NotNested -> &NotNestedL
				case 5614: state = 5615; break; // &NotNestedLess -> &NotNestedLessL
				case 7191: state = 7213; break; // &RightArrow -> &RightArrowL
				case 7775: state = 7786; break; // &Short -> &ShortL
				case 9070: state = 9071; break; // &Upper -> &UpperL
				case 9377: state = 9382; break; // &Vertical -> &VerticalL
				default: return false;
				}
				break;
			case 'M':
				switch (state) {
				case 0: state = 4781; break; // & -> &M
				case 1: state = 111; break; // &A -> &AM
				case 1023: state = 1032; break; // &Circle -> &CircleM
				case 5090: state = 5091; break; // &Negative -> &NegativeM
				case 6589: state = 6590; break; // &Plus -> &PlusM
				default: return false;
				}
				break;
			case 'N':
				switch (state) {
				case 0: state = 4971; break; // & -> &N
				case 301: state = 587; break; // &b -> &bN
				case 2108: state = 2287; break; // &E -> &EN
				case 5376: state = 5590; break; // &Not -> &NotN
				case 8537: state = 8538; break; // &THOR -> &THORN
				default: return false;
				}
				break;
			case 'O':
				switch (state) {
				case 0: state = 6131; break; // & -> &O
				case 789: state = 1217; break; // &C -> &CO
				case 3236: state = 3463; break; // &I -> &IO
				case 6869: state = 6870; break; // &QU -> &QUO
				case 7610: state = 7930; break; // &S -> &SO
				case 8535: state = 8536; break; // &TH -> &THO
				default: return false;
				}
				break;
			case 'P':
				switch (state) {
				case 0: state = 6482; break; // & -> &P
				case 111: state = 112; break; // &AM -> &AMP
				case 1023: state = 1038; break; // &Circle -> &CircleP
				case 1217: state = 1218; break; // &CO -> &COP
				case 2954: state = 2955; break; // &gtl -> &gtlP
				case 4731: state = 4738; break; // &ltr -> &ltrP
				case 4903: state = 4904; break; // &Minus -> &MinusP
				case 5376: state = 5630; break; // &Not -> &NotP
				case 6437: state = 6451; break; // &Over -> &OverP
				case 8919: state = 8933; break; // &Under -> &UnderP
				case 8947: state = 8949; break; // &Union -> &UnionP
				default: return false;
				}
				break;
			case 'Q':
				switch (state) {
				case 0: state = 6813; break; // & -> &Q
				case 1098: state = 1111; break; // &CloseCurly -> &CloseCurlyQ
				case 1104: state = 1105; break; // &CloseCurlyDouble -> &CloseCurlyDoubleQ
				case 6313: state = 6326; break; // &OpenCurly -> &OpenCurlyQ
				case 6319: state = 6320; break; // &OpenCurlyDouble -> &OpenCurlyDoubleQ
				default: return false;
				}
				break;
			case 'R':
				switch (state) {
				case 0: state = 6886; break; // & -> &R
				case 618: state = 628; break; // &boxD -> &boxDR
				case 623: state = 632; break; // &boxd -> &boxdR
				case 673: state = 683; break; // &boxU -> &boxUR
				case 678: state = 687; break; // &boxu -> &boxuR
				case 691: state = 711; break; // &boxV -> &boxVR
				case 693: state = 715; break; // &boxv -> &boxvR
				case 1004: state = 1028; break; // &circled -> &circledR
				case 1747: state = 1836; break; // &Double -> &DoubleR
				case 1779: state = 1786; break; // &DoubleLeft -> &DoubleLeftR
				case 1803: state = 1825; break; // &DoubleLong -> &DoubleLongR
				case 1807: state = 1814; break; // &DoubleLongLeft -> &DoubleLongLeftR
				case 1882: state = 1987; break; // &Down -> &DownR
				case 1953: state = 1954; break; // &DownLeft -> &DownLeftR
				case 3035: state = 3036; break; // &HA -> &HAR
				case 3900: state = 4030; break; // &Left -> &LeftR
				case 3917: state = 3937; break; // &LeftArrow -> &LeftArrowR
				case 4436: state = 4509; break; // &Long -> &LongR
				case 4440: state = 4469; break; // &LongLeft -> &LongLeftR
				case 4590: state = 4601; break; // &Lower -> &LowerR
				case 4965: state = 5868; break; // &n -> &nR
				case 5376: state = 5656; break; // &Not -> &NotR
				case 7775: state = 7812; break; // &Short -> &ShortR
				case 8400: state = 8623; break; // &T -> &TR
				case 8536: state = 8537; break; // &THO -> &THOR
				case 9070: state = 9081; break; // &Upper -> &UpperR
				default: return false;
				}
				break;
			case 'S':
				switch (state) {
				case 0: state = 7610; break; // & -> &S
				case 1004: state = 1030; break; // &circled -> &circledS
				case 1425: state = 2048; break; // &D -> &DS
				case 2248: state = 2249; break; // &Empty -> &EmptyS
				case 2253: state = 2254; break; // &EmptySmall -> &EmptySmallS
				case 2266: state = 2267; break; // &EmptyVery -> &EmptyVeryS
				case 2271: state = 2272; break; // &EmptyVerySmall -> &EmptyVerySmallS
				case 2558: state = 2559; break; // &Filled -> &FilledS
				case 2563: state = 2564; break; // &FilledSmall -> &FilledSmallS
				case 2574: state = 2575; break; // &FilledVery -> &FilledVeryS
				case 2579: state = 2580; break; // &FilledVerySmall -> &FilledVerySmallS
				case 2871: state = 2906; break; // &Greater -> &GreaterS
				case 3105: state = 3106; break; // &Hilbert -> &HilbertS
				case 4239: state = 4284; break; // &Less -> &LessS
				case 4847: state = 4848; break; // &Medium -> &MediumS
				case 5096: state = 5097; break; // &NegativeMedium -> &NegativeMediumS
				case 5107: state = 5108; break; // &NegativeThick -> &NegativeThickS
				case 5114: state = 5115; break; // &NegativeThin -> &NegativeThinS
				case 5128: state = 5129; break; // &NegativeVeryThin -> &NegativeVeryThinS
				case 5362: state = 5363; break; // &NonBreaking -> &NonBreakingS
				case 5376: state = 5694; break; // &Not -> &NotS
				case 5445: state = 5476; break; // &NotGreater -> &NotGreaterS
				case 5552: state = 5573; break; // &NotLess -> &NotLessS
				case 5637: state = 5645; break; // &NotPrecedes -> &NotPrecedesS
				case 5699: state = 5700; break; // &NotSquare -> &NotSquareS
				case 5743: state = 5751; break; // &NotSucceeds -> &NotSucceedsS
				case 6138: state = 6376; break; // &o -> &oS
				case 6677: state = 6685; break; // &Precedes -> &PrecedesS
				case 8013: state = 8032; break; // &Square -> &SquareS
				case 8221: state = 8229; break; // &Succeeds -> &SucceedsS
				case 8400: state = 8713; break; // &T -> &TS
				case 8509: state = 8510; break; // &Thick -> &ThickS
				case 8520: state = 8521; break; // &Thin -> &ThinS
				case 9377: state = 9387; break; // &Vertical -> &VerticalS
				case 9407: state = 9408; break; // &VeryThin -> &VeryThinS
				case 9798: state = 9799; break; // &ZeroWidth -> &ZeroWidthS
				default: return false;
				}
				break;
			case 'T':
				switch (state) {
				case 0: state = 8400; break; // & -> &T
				case 1023: state = 1043; break; // &Circle -> &CircleT
				case 1566: state = 1593; break; // &Diacritical -> &DiacriticalT
				case 1779: state = 1797; break; // &DoubleLeft -> &DoubleLeftT
				case 1840: state = 1847; break; // &DoubleRight -> &DoubleRightT
				case 1882: state = 2013; break; // &Down -> &DownT
				case 1953: state = 1966; break; // &DownLeft -> &DownLeftT
				case 1991: state = 1992; break; // &DownRight -> &DownRightT
				case 2108: state = 2442; break; // &E -> &ET
				case 2370: state = 2377; break; // &Equal -> &EqualT
				case 2708: state = 2938; break; // &G -> &GT
				case 2871: state = 2917; break; // &Greater -> &GreaterT
				case 3450: state = 3457; break; // &Invisible -> &InvisibleT
				case 3698: state = 4694; break; // &L -> &LT
				case 3900: state = 4092; break; // &Left -> &LeftT
				case 3976: state = 3977; break; // &LeftDown -> &LeftDownT
				case 4139: state = 4151; break; // &LeftUp -> &LeftUpT
				case 4239: state = 4295; break; // &Less -> &LessT
				case 5090: state = 5103; break; // &Negative -> &NegativeT
				case 5124: state = 5125; break; // &NegativeVery -> &NegativeVeryT
				case 5376: state = 5781; break; // &Not -> &NotT
				case 5425: state = 5427; break; // &NotEqual -> &NotEqualT
				case 5445: state = 5487; break; // &NotGreater -> &NotGreaterT
				case 5531: state = 5532; break; // &NotLeft -> &NotLeftT
				case 5552: state = 5584; break; // &NotLess -> &NotLessT
				case 5674: state = 5675; break; // &NotRight -> &NotRightT
				case 5743: state = 5762; break; // &NotSucceeds -> &NotSucceedsT
				case 5785: state = 5803; break; // &NotTilde -> &NotTildeT
				case 6677: state = 6696; break; // &Precedes -> &PrecedesT
				case 6870: state = 6871; break; // &QUO -> &QUOT
				case 7174: state = 7337; break; // &Right -> &RightT
				case 7251: state = 7252; break; // &RightDown -> &RightDownT
				case 7384: state = 7396; break; // &RightUp -> &RightUpT
				case 7931: state = 7932; break; // &SOF -> &SOFT
				case 8221: state = 8240; break; // &Succeeds -> &SucceedsT
				case 8269: state = 8270; break; // &Such -> &SuchT
				case 8547: state = 8570; break; // &Tilde -> &TildeT
				case 8970: state = 9108; break; // &Up -> &UpT
				case 9377: state = 9397; break; // &Vertical -> &VerticalT
				case 9403: state = 9404; break; // &Very -> &VeryT
				default: return false;
				}
				break;
			case 'U':
				switch (state) {
				case 0: state = 8768; break; // & -> &U
				case 613: state = 673; break; // &box -> &boxU
				case 636: state = 648; break; // &boxH -> &boxHU
				case 638: state = 652; break; // &boxh -> &boxhU
				case 1747: state = 1851; break; // &Double -> &DoubleU
				case 1887: state = 1907; break; // &DownArrow -> &DownArrowU
				case 3900: state = 4138; break; // &Left -> &LeftU
				case 6813: state = 6869; break; // &Q -> &QU
				case 7101: state = 7121; break; // &Reverse -> &ReverseU
				case 7174: state = 7383; break; // &Right -> &RightU
				case 7775: state = 7823; break; // &Short -> &ShortU
				case 8013: state = 8058; break; // &Square -> &SquareU
				case 9665: state = 9732; break; // &Y -> &YU
				default: return false;
				}
				break;
			case 'V':
				switch (state) {
				case 0: state = 9303; break; // & -> &V
				case 613: state = 691; break; // &box -> &boxV
				case 1747: state = 1869; break; // &Double -> &DoubleV
				case 1953: state = 1976; break; // &DownLeft -> &DownLeftV
				case 1958: state = 1959; break; // &DownLeftRight -> &DownLeftRightV
				case 1968: state = 1969; break; // &DownLeftTee -> &DownLeftTeeV
				case 1991: state = 2002; break; // &DownRight -> &DownRightV
				case 1994: state = 1995; break; // &DownRightTee -> &DownRightTeeV
				case 2248: state = 2263; break; // &Empty -> &EmptyV
				case 2558: state = 2571; break; // &Filled -> &FilledV
				case 3900: state = 4172; break; // &Left -> &LeftV
				case 3976: state = 3987; break; // &LeftDown -> &LeftDownV
				case 3979: state = 3980; break; // &LeftDownTee -> &LeftDownTeeV
				case 4034: state = 4085; break; // &LeftRight -> &LeftRightV
				case 4094: state = 4102; break; // &LeftTee -> &LeftTeeV
				case 4139: state = 4161; break; // &LeftUp -> &LeftUpV
				case 4143: state = 4144; break; // &LeftUpDown -> &LeftUpDownV
				case 4153: state = 4154; break; // &LeftUpTee -> &LeftUpTeeV
				case 4965: state = 6047; break; // &n -> &nV
				case 5090: state = 5121; break; // &Negative -> &NegativeV
				case 5376: state = 5809; break; // &Not -> &NotV
				case 5401: state = 5402; break; // &NotDouble -> &NotDoubleV
				case 7174: state = 7417; break; // &Right -> &RightV
				case 7251: state = 7262; break; // &RightDown -> &RightDownV
				case 7254: state = 7255; break; // &RightDownTee -> &RightDownTeeV
				case 7339: state = 7347; break; // &RightTee -> &RightTeeV
				case 7384: state = 7406; break; // &RightUp -> &RightUpV
				case 7388: state = 7389; break; // &RightUpDown -> &RightUpDownV
				case 7398: state = 7399; break; // &RightUpTee -> &RightUpTeeV
				default: return false;
				}
				break;
			case 'W':
				switch (state) {
				case 0: state = 9484; break; // & -> &W
				case 9793: state = 9794; break; // &Zero -> &ZeroW
				default: return false;
				}
				break;
			case 'X':
				switch (state) {
				case 0: state = 9565; break; // & -> &X
				default: return false;
				}
				break;
			case 'Y':
				switch (state) {
				case 0: state = 9665; break; // & -> &Y
				case 1218: state = 1219; break; // &COP -> &COPY
				default: return false;
				}
				break;
			case 'Z':
				switch (state) {
				case 0: state = 9747; break; // & -> &Z
				case 1425: state = 2093; break; // &D -> &DZ
				default: return false;
				}
				break;
			case 'a':
				switch (state) {
				case 0: state = 8; break; // & -> &a
				case 1: state = 2; break; // &A -> &Aa
				case 8: state = 9; break; // &a -> &aa
				case 68: state = 69; break; // &Agr -> &Agra
				case 74: state = 75; break; // &agr -> &agra
				case 91: state = 92; break; // &Alph -> &Alpha
				case 95: state = 96; break; // &alph -> &alpha
				case 98: state = 99; break; // &Am -> &Ama
				case 103: state = 104; break; // &am -> &ama
				case 120: state = 122; break; // &and -> &anda
				case 145: state = 147; break; // &angmsd -> &angmsda
				case 147: state = 148; break; // &angmsda -> &angmsdaa
				case 178: state = 179; break; // &angz -> &angza
				case 199: state = 201; break; // &ap -> &apa
				case 301: state = 302; break; // &b -> &ba
				case 331: state = 332; break; // &B -> &Ba
				case 336: state = 337; break; // &Backsl -> &Backsla
				case 385: state = 386; break; // &bec -> &beca
				case 391: state = 392; break; // &Bec -> &Beca
				case 423: state = 424; break; // &Bet -> &Beta
				case 426: state = 427; break; // &bet -> &beta
				case 444: state = 445; break; // &bigc -> &bigca
				case 477: state = 478; break; // &bigst -> &bigsta
				case 483: state = 484; break; // &bigtri -> &bigtria
				case 513: state = 514; break; // &bk -> &bka
				case 519: state = 520; break; // &bl -> &bla
				case 533: state = 534; break; // &blacksqu -> &blacksqua
				case 540: state = 541; break; // &blacktri -> &blacktria
				case 736: state = 737; break; // &brvb -> &brvba
				case 789: state = 790; break; // &C -> &Ca
				case 796: state = 797; break; // &c -> &ca
				case 805: state = 807; break; // &cap -> &capa
				case 817: state = 818; break; // &capc -> &capca
				case 829: state = 830; break; // &Capit -> &Capita
				case 841: state = 842; break; // &CapitalDifferenti -> &CapitalDifferentia
				case 861: state = 862; break; // &cc -> &cca
				case 866: state = 867; break; // &Cc -> &Cca
				case 924: state = 925; break; // &Cedill -> &Cedilla
				case 968: state = 969; break; // &checkm -> &checkma
				case 987: state = 988; break; // &circle -> &circlea
				case 1004: state = 1005; break; // &circled -> &circleda
				case 1014: state = 1015; break; // &circledd -> &circledda
				case 1088: state = 1089; break; // &ClockwiseContourIntegr -> &ClockwiseContourIntegra
				case 1143: state = 1144; break; // &comm -> &comma
				case 1196: state = 1197; break; // &ContourIntegr -> &ContourIntegra
				case 1252: state = 1253; break; // &CounterClockwiseContourIntegr -> &CounterClockwiseContourIntegra
				case 1256: state = 1257; break; // &cr -> &cra
				case 1293: state = 1294; break; // &cud -> &cuda
				case 1308: state = 1309; break; // &cul -> &cula
				case 1322: state = 1323; break; // &cupbrc -> &cupbrca
				case 1326: state = 1327; break; // &CupC -> &CupCa
				case 1330: state = 1331; break; // &cupc -> &cupca
				case 1346: state = 1347; break; // &cur -> &cura
				case 1382: state = 1383; break; // &curve -> &curvea
				case 1425: state = 1426; break; // &D -> &Da
				case 1432: state = 1433; break; // &d -> &da
				case 1464: state = 1465; break; // &dbk -> &dbka
				case 1470: state = 1471; break; // &dbl -> &dbla
				case 1474: state = 1475; break; // &Dc -> &Dca
				case 1480: state = 1481; break; // &dc -> &dca
				case 1492: state = 1494; break; // &dd -> &dda
				case 1505: state = 1506; break; // &DDotr -> &DDotra
				case 1522: state = 1523; break; // &Delt -> &Delta
				case 1526: state = 1527; break; // &delt -> &delta
				case 1546: state = 1547; break; // &dH -> &dHa
				case 1550: state = 1551; break; // &dh -> &dha
				case 1557: state = 1558; break; // &Di -> &Dia
				case 1564: state = 1565; break; // &Diacritic -> &Diacritica
				case 1588: state = 1589; break; // &DiacriticalGr -> &DiacriticalGra
				case 1599: state = 1600; break; // &di -> &dia
				case 1628: state = 1629; break; // &Differenti -> &Differentia
				case 1633: state = 1634; break; // &dig -> &diga
				case 1636: state = 1637; break; // &digamm -> &digamma
				case 1681: state = 1682; break; // &doll -> &dolla
				case 1709: state = 1710; break; // &DotEqu -> &DotEqua
				case 1726: state = 1727; break; // &dotsqu -> &dotsqua
				case 1735: state = 1736; break; // &doubleb -> &doubleba
				case 1760: state = 1761; break; // &DoubleContourIntegr -> &DoubleContourIntegra
				case 1874: state = 1875; break; // &DoubleVertic -> &DoubleVertica
				case 1877: state = 1878; break; // &DoubleVerticalB -> &DoubleVerticalBa
				case 1882: state = 1889; break; // &Down -> &Downa
				case 1896: state = 1897; break; // &down -> &downa
				case 1903: state = 1904; break; // &DownArrowB -> &DownArrowBa
				case 1924: state = 1925; break; // &downdown -> &downdowna
				case 1932: state = 1933; break; // &downh -> &downha
				case 1983: state = 1984; break; // &DownLeftVectorB -> &DownLeftVectorBa
				case 2009: state = 2010; break; // &DownRightVectorB -> &DownRightVectorBa
				case 2025: state = 2026; break; // &drbk -> &drbka
				case 2077: state = 2078; break; // &du -> &dua
				case 2082: state = 2083; break; // &duh -> &duha
				case 2086: state = 2087; break; // &dw -> &dwa
				case 2103: state = 2104; break; // &dzigr -> &dzigra
				case 2108: state = 2109; break; // &E -> &Ea
				case 2115: state = 2116; break; // &e -> &ea
				case 2127: state = 2128; break; // &Ec -> &Eca
				case 2133: state = 2134; break; // &ec -> &eca
				case 2188: state = 2189; break; // &Egr -> &Egra
				case 2193: state = 2194; break; // &egr -> &egra
				case 2228: state = 2229; break; // &Em -> &Ema
				case 2233: state = 2234; break; // &em -> &ema
				case 2250: state = 2251; break; // &EmptySm -> &EmptySma
				case 2256: state = 2257; break; // &EmptySmallSqu -> &EmptySmallSqua
				case 2268: state = 2269; break; // &EmptyVerySm -> &EmptyVerySma
				case 2274: state = 2275; break; // &EmptyVerySmallSqu -> &EmptyVerySmallSqua
				case 2312: state = 2313; break; // &ep -> &epa
				case 2354: state = 2355; break; // &eqsl -> &eqsla
				case 2368: state = 2369; break; // &Equ -> &Equa
				case 2372: state = 2373; break; // &equ -> &equa
				case 2403: state = 2404; break; // &eqvp -> &eqvpa
				case 2409: state = 2410; break; // &er -> &era
				case 2436: state = 2437; break; // &Et -> &Eta
				case 2439: state = 2440; break; // &et -> &eta
				case 2475: state = 2476; break; // &expect -> &expecta
				case 2488: state = 2489; break; // &Exponenti -> &Exponentia
				case 2498: state = 2499; break; // &exponenti -> &exponentia
				case 2503: state = 2504; break; // &f -> &fa
				case 2525: state = 2526; break; // &fem -> &fema
				case 2560: state = 2561; break; // &FilledSm -> &FilledSma
				case 2566: state = 2567; break; // &FilledSmallSqu -> &FilledSmallSqua
				case 2576: state = 2577; break; // &FilledVerySm -> &FilledVerySma
				case 2582: state = 2583; break; // &FilledVerySmallSqu -> &FilledVerySmallSqua
				case 2592: state = 2593; break; // &fl -> &fla
				case 2621: state = 2622; break; // &for -> &fora
				case 2639: state = 2640; break; // &fp -> &fpa
				case 2647: state = 2648; break; // &fr -> &fra
				case 2701: state = 2702; break; // &g -> &ga
				case 2708: state = 2709; break; // &G -> &Ga
				case 2711: state = 2712; break; // &Gamm -> &Gamma
				case 2715: state = 2716; break; // &gamm -> &gamma
				case 2776: state = 2777; break; // &geqsl -> &geqsla
				case 2824: state = 2826; break; // &gl -> &gla
				case 2832: state = 2833; break; // &gn -> &gna
				case 2861: state = 2862; break; // &gr -> &gra
				case 2867: state = 2868; break; // &Gre -> &Grea
				case 2874: state = 2875; break; // &GreaterEqu -> &GreaterEqua
				case 2889: state = 2890; break; // &GreaterFullEqu -> &GreaterFullEqua
				case 2895: state = 2896; break; // &GreaterGre -> &GreaterGrea
				case 2907: state = 2908; break; // &GreaterSl -> &GreaterSla
				case 2913: state = 2914; break; // &GreaterSlantEqu -> &GreaterSlantEqua
				case 2955: state = 2956; break; // &gtlP -> &gtlPa
				case 2965: state = 2966; break; // &gtr -> &gtra
				case 3014: state = 3015; break; // &H -> &Ha
				case 3020: state = 3021; break; // &h -> &ha
				case 3060: state = 3061; break; // &hb -> &hba
				case 3074: state = 3075; break; // &he -> &hea
				case 3107: state = 3108; break; // &HilbertSp -> &HilbertSpa
				case 3114: state = 3115; break; // &hkse -> &hksea
				case 3120: state = 3121; break; // &hksw -> &hkswa
				case 3126: state = 3127; break; // &ho -> &hoa
				case 3141: state = 3142; break; // &hookleft -> &hooklefta
				case 3152: state = 3153; break; // &hookright -> &hookrighta
				case 3167: state = 3168; break; // &horb -> &horba
				case 3176: state = 3177; break; // &Horizont -> &Horizonta
				case 3192: state = 3193; break; // &hsl -> &hsla
				case 3221: state = 3222; break; // &HumpEqu -> &HumpEqua
				case 3236: state = 3237; break; // &I -> &Ia
				case 3243: state = 3244; break; // &i -> &ia
				case 3290: state = 3291; break; // &Igr -> &Igra
				case 3296: state = 3297; break; // &igr -> &igra
				case 3317: state = 3318; break; // &iiot -> &iiota
				case 3330: state = 3332; break; // &Im -> &Ima
				case 3336: state = 3337; break; // &im -> &ima
				case 3346: state = 3347; break; // &Imagin -> &Imagina
				case 3357: state = 3358; break; // &imagp -> &imagpa
				case 3380: state = 3381; break; // &inc -> &inca
				case 3403: state = 3404; break; // &intc -> &intca
				case 3415: state = 3416; break; // &Integr -> &Integra
				case 3420: state = 3421; break; // &interc -> &interca
				case 3433: state = 3434; break; // &intl -> &intla
				case 3454: state = 3455; break; // &InvisibleComm -> &InvisibleComma
				case 3486: state = 3487; break; // &Iot -> &Iota
				case 3489: state = 3490; break; // &iot -> &iota
				case 3577: state = 3578; break; // &jm -> &jma
				case 3618: state = 3619; break; // &K -> &Ka
				case 3621: state = 3622; break; // &Kapp -> &Kappa
				case 3624: state = 3625; break; // &k -> &ka
				case 3627: state = 3628; break; // &kapp -> &kappa
				case 3692: state = 3705; break; // &l -> &la
				case 3693: state = 3694; break; // &lA -> &lAa
				case 3698: state = 3699; break; // &L -> &La
				case 3719: state = 3720; break; // &lagr -> &lagra
				case 3725: state = 3726; break; // &Lambd -> &Lambda
				case 3730: state = 3731; break; // &lambd -> &lambda
				case 3747: state = 3748; break; // &Lapl -> &Lapla
				case 3792: state = 3799; break; // &lat -> &lata
				case 3794: state = 3795; break; // &lAt -> &lAta
				case 3807: state = 3808; break; // &lB -> &lBa
				case 3812: state = 3813; break; // &lb -> &lba
				case 3821: state = 3822; break; // &lbr -> &lbra
				case 3837: state = 3838; break; // &Lc -> &Lca
				case 3843: state = 3844; break; // &lc -> &lca
				case 3870: state = 3871; break; // &ldc -> &ldca
				case 3881: state = 3882; break; // &ldrdh -> &ldrdha
				case 3887: state = 3888; break; // &ldrush -> &ldrusha
				case 3900: state = 3919; break; // &Left -> &Lefta
				case 3907: state = 3908; break; // &LeftAngleBr -> &LeftAngleBra
				case 3926: state = 3927; break; // &left -> &lefta
				case 3933: state = 3934; break; // &LeftArrowB -> &LeftArrowBa
				case 3948: state = 3949; break; // &leftarrowt -> &leftarrowta
				case 3968: state = 3969; break; // &LeftDoubleBr -> &LeftDoubleBra
				case 3994: state = 3995; break; // &LeftDownVectorB -> &LeftDownVectorBa
				case 4004: state = 4005; break; // &lefth -> &leftha
				case 4022: state = 4023; break; // &leftleft -> &leftlefta
				case 4045: state = 4046; break; // &Leftright -> &Leftrighta
				case 4056: state = 4057; break; // &leftright -> &leftrighta
				case 4065: state = 4066; break; // &leftrighth -> &leftrightha
				case 4078: state = 4079; break; // &leftrightsquig -> &leftrightsquiga
				case 4121: state = 4122; break; // &LeftTri -> &LeftTria
				case 4128: state = 4129; break; // &LeftTriangleB -> &LeftTriangleBa
				case 4134: state = 4135; break; // &LeftTriangleEqu -> &LeftTriangleEqua
				case 4168: state = 4169; break; // &LeftUpVectorB -> &LeftUpVectorBa
				case 4179: state = 4180; break; // &LeftVectorB -> &LeftVectorBa
				case 4192: state = 4193; break; // &leqsl -> &leqsla
				case 4215: state = 4216; break; // &less -> &lessa
				case 4242: state = 4243; break; // &LessEqu -> &LessEqua
				case 4247: state = 4248; break; // &LessEqualGre -> &LessEqualGrea
				case 4259: state = 4260; break; // &LessFullEqu -> &LessFullEqua
				case 4265: state = 4266; break; // &LessGre -> &LessGrea
				case 4285: state = 4286; break; // &LessSl -> &LessSla
				case 4291: state = 4292; break; // &LessSlantEqu -> &LessSlantEqua
				case 4321: state = 4322; break; // &lH -> &lHa
				case 4325: state = 4326; break; // &lh -> &lha
				case 4348: state = 4350; break; // &ll -> &lla
				case 4363: state = 4364; break; // &Lleft -> &Llefta
				case 4370: state = 4371; break; // &llh -> &llha
				case 4394: state = 4396; break; // &lmoust -> &lmousta
				case 4401: state = 4402; break; // &ln -> &lna
				case 4422: state = 4423; break; // &lo -> &loa
				case 4450: state = 4451; break; // &Longleft -> &Longlefta
				case 4462: state = 4463; break; // &longleft -> &longlefta
				case 4484: state = 4485; break; // &Longleftright -> &Longleftrighta
				case 4495: state = 4496; break; // &longleftright -> &longleftrighta
				case 4502: state = 4503; break; // &longm -> &longma
				case 4524: state = 4525; break; // &Longright -> &Longrighta
				case 4535: state = 4536; break; // &longright -> &longrighta
				case 4543: state = 4544; break; // &loop -> &loopa
				case 4560: state = 4561; break; // &lop -> &lopa
				case 4579: state = 4580; break; // &low -> &lowa
				case 4584: state = 4585; break; // &lowb -> &lowba
				case 4621: state = 4622; break; // &lp -> &lpa
				case 4628: state = 4629; break; // &lr -> &lra
				case 4640: state = 4641; break; // &lrh -> &lrha
				case 4652: state = 4653; break; // &ls -> &lsa
				case 4720: state = 4721; break; // &ltl -> &ltla
				case 4738: state = 4739; break; // &ltrP -> &ltrPa
				case 4746: state = 4747; break; // &lurdsh -> &lurdsha
				case 4751: state = 4752; break; // &luruh -> &luruha
				case 4767: state = 4768; break; // &m -> &ma
				case 4781: state = 4782; break; // &M -> &Ma
				case 4812: state = 4813; break; // &mcomm -> &mcomma
				case 4820: state = 4821; break; // &md -> &mda
				case 4830: state = 4831; break; // &me -> &mea
				case 4836: state = 4837; break; // &measured -> &measureda
				case 4849: state = 4850; break; // &MediumSp -> &MediumSpa
				case 4876: state = 4878; break; // &mid -> &mida
				case 4957: state = 4958; break; // &multim -> &multima
				case 4961: state = 4962; break; // &mum -> &muma
				case 4965: state = 4966; break; // &n -> &na
				case 4968: state = 4969; break; // &nabl -> &nabla
				case 4971: state = 4972; break; // &N -> &Na
				case 5003: state = 5005; break; // &natur -> &natura
				case 5020: state = 5021; break; // &nc -> &nca
				case 5024: state = 5025; break; // &Nc -> &Nca
				case 5059: state = 5060; break; // &nd -> &nda
				case 5064: state = 5066; break; // &ne -> &nea
				case 5085: state = 5086; break; // &Neg -> &Nega
				case 5098: state = 5099; break; // &NegativeMediumSp -> &NegativeMediumSpa
				case 5109: state = 5110; break; // &NegativeThickSp -> &NegativeThickSpa
				case 5116: state = 5117; break; // &NegativeThinSp -> &NegativeThinSpa
				case 5130: state = 5131; break; // &NegativeVeryThinSp -> &NegativeVeryThinSpa
				case 5141: state = 5142; break; // &nese -> &nesea
				case 5154: state = 5155; break; // &NestedGre -> &NestedGrea
				case 5161: state = 5162; break; // &NestedGreaterGre -> &NestedGreaterGrea
				case 5205: state = 5206; break; // &ngeqsl -> &ngeqsla
				case 5227: state = 5232; break; // &nh -> &nha
				case 5236: state = 5237; break; // &nhp -> &nhpa
				case 5256: state = 5261; break; // &nl -> &nla
				case 5275: state = 5276; break; // &nLeft -> &nLefta
				case 5283: state = 5284; break; // &nleft -> &nlefta
				case 5294: state = 5295; break; // &nLeftright -> &nLeftrighta
				case 5305: state = 5306; break; // &nleftright -> &nleftrighta
				case 5317: state = 5318; break; // &nleqsl -> &nleqsla
				case 5350: state = 5351; break; // &NoBre -> &NoBrea
				case 5357: state = 5358; break; // &NonBre -> &NonBrea
				case 5364: state = 5365; break; // &NonBreakingSp -> &NonBreakingSpa
				case 5392: state = 5393; break; // &NotCupC -> &NotCupCa
				case 5407: state = 5408; break; // &NotDoubleVertic -> &NotDoubleVertica
				case 5410: state = 5411; break; // &NotDoubleVerticalB -> &NotDoubleVerticalBa
				case 5423: state = 5424; break; // &NotEqu -> &NotEqua
				case 5441: state = 5442; break; // &NotGre -> &NotGrea
				case 5449: state = 5450; break; // &NotGreaterEqu -> &NotGreaterEqua
				case 5459: state = 5460; break; // &NotGreaterFullEqu -> &NotGreaterFullEqua
				case 5465: state = 5466; break; // &NotGreaterGre -> &NotGreaterGrea
				case 5477: state = 5478; break; // &NotGreaterSl -> &NotGreaterSla
				case 5483: state = 5484; break; // &NotGreaterSlantEqu -> &NotGreaterSlantEqua
				case 5508: state = 5509; break; // &NotHumpEqu -> &NotHumpEqua
				case 5521: state = 5522; break; // &notinv -> &notinva
				case 5534: state = 5535; break; // &NotLeftTri -> &NotLeftTria
				case 5541: state = 5542; break; // &NotLeftTriangleB -> &NotLeftTriangleBa
				case 5547: state = 5548; break; // &NotLeftTriangleEqu -> &NotLeftTriangleEqua
				case 5556: state = 5557; break; // &NotLessEqu -> &NotLessEqua
				case 5562: state = 5563; break; // &NotLessGre -> &NotLessGrea
				case 5574: state = 5575; break; // &NotLessSl -> &NotLessSla
				case 5580: state = 5581; break; // &NotLessSlantEqu -> &NotLessSlantEqua
				case 5598: state = 5599; break; // &NotNestedGre -> &NotNestedGrea
				case 5605: state = 5606; break; // &NotNestedGreaterGre -> &NotNestedGreaterGrea
				case 5623: state = 5624; break; // &notniv -> &notniva
				case 5641: state = 5642; break; // &NotPrecedesEqu -> &NotPrecedesEqua
				case 5646: state = 5647; break; // &NotPrecedesSl -> &NotPrecedesSla
				case 5652: state = 5653; break; // &NotPrecedesSlantEqu -> &NotPrecedesSlantEqua
				case 5677: state = 5678; break; // &NotRightTri -> &NotRightTria
				case 5684: state = 5685; break; // &NotRightTriangleB -> &NotRightTriangleBa
				case 5690: state = 5691; break; // &NotRightTriangleEqu -> &NotRightTriangleEqua
				case 5696: state = 5697; break; // &NotSqu -> &NotSqua
				case 5709: state = 5710; break; // &NotSquareSubsetEqu -> &NotSquareSubsetEqua
				case 5722: state = 5723; break; // &NotSquareSupersetEqu -> &NotSquareSupersetEqua
				case 5734: state = 5735; break; // &NotSubsetEqu -> &NotSubsetEqua
				case 5747: state = 5748; break; // &NotSucceedsEqu -> &NotSucceedsEqua
				case 5752: state = 5753; break; // &NotSucceedsSl -> &NotSucceedsSla
				case 5758: state = 5759; break; // &NotSucceedsSlantEqu -> &NotSucceedsSlantEqua
				case 5777: state = 5778; break; // &NotSupersetEqu -> &NotSupersetEqua
				case 5789: state = 5790; break; // &NotTildeEqu -> &NotTildeEqua
				case 5799: state = 5800; break; // &NotTildeFullEqu -> &NotTildeFullEqua
				case 5814: state = 5815; break; // &NotVertic -> &NotVertica
				case 5817: state = 5818; break; // &NotVerticalB -> &NotVerticalBa
				case 5821: state = 5822; break; // &np -> &npa
				case 5823: state = 5825; break; // &npar -> &npara
				case 5855: state = 5860; break; // &nr -> &nra
				case 5872: state = 5873; break; // &nRight -> &nRighta
				case 5882: state = 5883; break; // &nright -> &nrighta
				case 5918: state = 5919; break; // &nshortp -> &nshortpa
				case 5920: state = 5921; break; // &nshortpar -> &nshortpara
				case 5938: state = 5939; break; // &nsp -> &nspa
				case 6007: state = 6008; break; // &ntri -> &ntria
				case 6043: state = 6044; break; // &nv -> &nva
				case 6048: state = 6049; break; // &nVD -> &nVDa
				case 6053: state = 6054; break; // &nVd -> &nVda
				case 6058: state = 6059; break; // &nvD -> &nvDa
				case 6063: state = 6064; break; // &nvd -> &nvda
				case 6073: state = 6074; break; // &nvH -> &nvHa
				case 6111: state = 6112; break; // &nw -> &nwa
				case 6127: state = 6128; break; // &nwne -> &nwnea
				case 6131: state = 6132; break; // &O -> &Oa
				case 6138: state = 6139; break; // &o -> &oa
				case 6163: state = 6164; break; // &od -> &oda
				case 6170: state = 6171; break; // &Odbl -> &Odbla
				case 6175: state = 6176; break; // &odbl -> &odbla
				case 6215: state = 6216; break; // &Ogr -> &Ogra
				case 6220: state = 6221; break; // &ogr -> &ogra
				case 6228: state = 6229; break; // &ohb -> &ohba
				case 6238: state = 6239; break; // &ol -> &ola
				case 6258: state = 6259; break; // &Om -> &Oma
				case 6263: state = 6264; break; // &om -> &oma
				case 6269: state = 6270; break; // &Omeg -> &Omega
				case 6273: state = 6274; break; // &omeg -> &omega
				case 6302: state = 6303; break; // &op -> &opa
				case 6342: state = 6344; break; // &or -> &ora
				case 6386: state = 6387; break; // &Osl -> &Osla
				case 6391: state = 6392; break; // &osl -> &osla
				case 6417: state = 6419; break; // &otimes -> &otimesa
				case 6431: state = 6432; break; // &ovb -> &ovba
				case 6438: state = 6439; break; // &OverB -> &OverBa
				case 6442: state = 6443; break; // &OverBr -> &OverBra
				case 6451: state = 6452; break; // &OverP -> &OverPa
				case 6463: state = 6464; break; // &p -> &pa
				case 6465: state = 6467; break; // &par -> &para
				case 6482: state = 6483; break; // &P -> &Pa
				case 6486: state = 6487; break; // &Parti -> &Partia
				case 6533: state = 6534; break; // &phmm -> &phmma
				case 6555: state = 6556; break; // &pl -> &pla
				case 6567: state = 6569; break; // &plus -> &plusa
				case 6612: state = 6613; break; // &Poinc -> &Poinca
				case 6617: state = 6618; break; // &Poincarepl -> &Poincarepla
				case 6642: state = 6644; break; // &pr -> &pra
				case 6655: state = 6657; break; // &prec -> &preca
				case 6681: state = 6682; break; // &PrecedesEqu -> &PrecedesEqua
				case 6686: state = 6687; break; // &PrecedesSl -> &PrecedesSla
				case 6692: state = 6693; break; // &PrecedesSlantEqu -> &PrecedesSlantEqua
				case 6705: state = 6706; break; // &precn -> &precna
				case 6735: state = 6736; break; // &prn -> &prna
				case 6754: state = 6755; break; // &prof -> &profa
				case 6756: state = 6757; break; // &profal -> &profala
				case 6778: state = 6780; break; // &Proportion -> &Proportiona
				case 6847: state = 6848; break; // &qu -> &qua
				case 6876: state = 6882; break; // &r -> &ra
				case 6877: state = 6878; break; // &rA -> &rAa
				case 6886: state = 6887; break; // &R -> &Ra
				case 6932: state = 6934; break; // &rarr -> &rarra
				case 6968: state = 6969; break; // &rAt -> &rAta
				case 6973: state = 6974; break; // &rat -> &rata
				case 6981: state = 6982; break; // &ration -> &rationa
				case 6986: state = 6987; break; // &RB -> &RBa
				case 6991: state = 6992; break; // &rB -> &rBa
				case 6996: state = 6997; break; // &rb -> &rba
				case 7005: state = 7006; break; // &rbr -> &rbra
				case 7021: state = 7022; break; // &Rc -> &Rca
				case 7027: state = 7028; break; // &rc -> &rca
				case 7054: state = 7055; break; // &rdc -> &rdca
				case 7059: state = 7060; break; // &rdldh -> &rdldha
				case 7074: state = 7075; break; // &re -> &rea
				case 7082: state = 7083; break; // &realp -> &realpa
				case 7151: state = 7152; break; // &rH -> &rHa
				case 7155: state = 7156; break; // &rh -> &rha
				case 7174: state = 7193; break; // &Right -> &Righta
				case 7181: state = 7182; break; // &RightAngleBr -> &RightAngleBra
				case 7202: state = 7203; break; // &right -> &righta
				case 7209: state = 7210; break; // &RightArrowB -> &RightArrowBa
				case 7223: state = 7224; break; // &rightarrowt -> &rightarrowta
				case 7243: state = 7244; break; // &RightDoubleBr -> &RightDoubleBra
				case 7269: state = 7270; break; // &RightDownVectorB -> &RightDownVectorBa
				case 7279: state = 7280; break; // &righth -> &rightha
				case 7297: state = 7298; break; // &rightleft -> &rightlefta
				case 7305: state = 7306; break; // &rightlefth -> &rightleftha
				case 7318: state = 7319; break; // &rightright -> &rightrighta
				case 7330: state = 7331; break; // &rightsquig -> &rightsquiga
				case 7366: state = 7367; break; // &RightTri -> &RightTria
				case 7373: state = 7374; break; // &RightTriangleB -> &RightTriangleBa
				case 7379: state = 7380; break; // &RightTriangleEqu -> &RightTriangleEqua
				case 7413: state = 7414; break; // &RightUpVectorB -> &RightUpVectorBa
				case 7424: state = 7425; break; // &RightVectorB -> &RightVectorBa
				case 7442: state = 7443; break; // &rl -> &rla
				case 7447: state = 7448; break; // &rlh -> &rlha
				case 7457: state = 7459; break; // &rmoust -> &rmousta
				case 7469: state = 7470; break; // &ro -> &roa
				case 7481: state = 7482; break; // &rop -> &ropa
				case 7512: state = 7513; break; // &rp -> &rpa
				case 7526: state = 7527; break; // &rr -> &rra
				case 7535: state = 7536; break; // &Rright -> &Rrighta
				case 7542: state = 7543; break; // &rs -> &rsa
				case 7595: state = 7596; break; // &RuleDel -> &RuleDela
				case 7604: state = 7605; break; // &ruluh -> &ruluha
				case 7610: state = 7611; break; // &S -> &Sa
				case 7617: state = 7618; break; // &s -> &sa
				case 7629: state = 7636; break; // &Sc -> &Sca
				case 7631: state = 7633; break; // &sc -> &sca
				case 7670: state = 7671; break; // &scn -> &scna
				case 7703: state = 7704; break; // &se -> &sea
				case 7725: state = 7726; break; // &sesw -> &seswa
				case 7751: state = 7752; break; // &sh -> &sha
				case 7803: state = 7804; break; // &shortp -> &shortpa
				case 7805: state = 7806; break; // &shortpar -> &shortpara
				case 7835: state = 7836; break; // &Sigm -> &Sigma
				case 7840: state = 7841; break; // &sigm -> &sigma
				case 7873: state = 7874; break; // &simr -> &simra
				case 7878: state = 7879; break; // &sl -> &sla
				case 7883: state = 7884; break; // &Sm -> &Sma
				case 7894: state = 7895; break; // &sm -> &sma
				case 7912: state = 7913; break; // &smep -> &smepa
				case 7944: state = 7946; break; // &solb -> &solba
				case 7956: state = 7957; break; // &sp -> &spa
				case 7969: state = 7970; break; // &sqc -> &sqca
				case 8008: state = 8015; break; // &squ -> &squa
				case 8010: state = 8011; break; // &Squ -> &Squa
				case 8041: state = 8042; break; // &SquareSubsetEqu -> &SquareSubsetEqua
				case 8054: state = 8055; break; // &SquareSupersetEqu -> &SquareSupersetEqua
				case 8068: state = 8069; break; // &sr -> &sra
				case 8091: state = 8092; break; // &sst -> &ssta
				case 8096: state = 8097; break; // &St -> &Sta
				case 8100: state = 8101; break; // &st -> &sta
				case 8106: state = 8107; break; // &str -> &stra
				case 8160: state = 8161; break; // &subr -> &subra
				case 8180: state = 8181; break; // &SubsetEqu -> &SubsetEqua
				case 8199: state = 8201; break; // &succ -> &succa
				case 8225: state = 8226; break; // &SucceedsEqu -> &SucceedsEqua
				case 8230: state = 8231; break; // &SucceedsSl -> &SucceedsSla
				case 8236: state = 8237; break; // &SucceedsSlantEqu -> &SucceedsSlantEqua
				case 8249: state = 8250; break; // &succn -> &succna
				case 8271: state = 8272; break; // &SuchTh -> &SuchTha
				case 8316: state = 8317; break; // &SupersetEqu -> &SupersetEqua
				case 8328: state = 8329; break; // &supl -> &supla
				case 8375: state = 8376; break; // &sw -> &swa
				case 8391: state = 8392; break; // &swnw -> &swnwa
				case 8400: state = 8401; break; // &T -> &Ta
				case 8404: state = 8405; break; // &t -> &ta
				case 8419: state = 8420; break; // &Tc -> &Tca
				case 8425: state = 8426; break; // &tc -> &tca
				case 8481: state = 8482; break; // &Thet -> &Theta
				case 8484: state = 8485; break; // &thet -> &theta
				case 8495: state = 8496; break; // &thick -> &thicka
				case 8511: state = 8512; break; // &ThickSp -> &ThickSpa
				case 8522: state = 8523; break; // &ThinSp -> &ThinSpa
				case 8527: state = 8528; break; // &thk -> &thka
				case 8556: state = 8557; break; // &TildeEqu -> &TildeEqua
				case 8566: state = 8567; break; // &TildeFullEqu -> &TildeFullEqua
				case 8580: state = 8582; break; // &timesb -> &timesba
				case 8591: state = 8592; break; // &toe -> &toea
				case 8614: state = 8615; break; // &tos -> &tosa
				case 8628: state = 8629; break; // &tr -> &tra
				case 8633: state = 8634; break; // &tri -> &tria
				case 8744: state = 8745; break; // &twohe -> &twohea
				case 8750: state = 8751; break; // &twoheadleft -> &twoheadlefta
				case 8761: state = 8762; break; // &twoheadright -> &twoheadrighta
				case 8768: state = 8769; break; // &U -> &Ua
				case 8775: state = 8776; break; // &u -> &ua
				case 8829: state = 8830; break; // &ud -> &uda
				case 8836: state = 8837; break; // &Udbl -> &Udbla
				case 8841: state = 8842; break; // &udbl -> &udbla
				case 8845: state = 8846; break; // &udh -> &udha
				case 8861: state = 8862; break; // &Ugr -> &Ugra
				case 8867: state = 8868; break; // &ugr -> &ugra
				case 8872: state = 8873; break; // &uH -> &uHa
				case 8876: state = 8877; break; // &uh -> &uha
				case 8904: state = 8905; break; // &Um -> &Uma
				case 8909: state = 8910; break; // &um -> &uma
				case 8920: state = 8921; break; // &UnderB -> &UnderBa
				case 8924: state = 8925; break; // &UnderBr -> &UnderBra
				case 8933: state = 8934; break; // &UnderP -> &UnderPa
				case 8970: state = 8977; break; // &Up -> &Upa
				case 8983: state = 8984; break; // &up -> &upa
				case 8990: state = 8991; break; // &UpArrowB -> &UpArrowBa
				case 9017: state = 9018; break; // &Updown -> &Updowna
				case 9027: state = 9028; break; // &updown -> &updowna
				case 9046: state = 9047; break; // &uph -> &upha
				case 9119: state = 9120; break; // &upup -> &upupa
				case 9182: state = 9183; break; // &uu -> &uua
				case 9194: state = 9195; break; // &uw -> &uwa
				case 9201: state = 9202; break; // &v -> &va
				case 9217: state = 9218; break; // &vark -> &varka
				case 9220: state = 9221; break; // &varkapp -> &varkappa
				case 9255: state = 9256; break; // &varsigm -> &varsigma
				case 9282: state = 9283; break; // &varthet -> &vartheta
				case 9286: state = 9287; break; // &vartri -> &vartria
				case 9304: state = 9305; break; // &Vb -> &Vba
				case 9308: state = 9309; break; // &vB -> &vBa
				case 9320: state = 9321; break; // &VD -> &VDa
				case 9325: state = 9326; break; // &Vd -> &Vda
				case 9330: state = 9331; break; // &vD -> &vDa
				case 9335: state = 9336; break; // &vd -> &vda
				case 9348: state = 9349; break; // &veeb -> &veeba
				case 9361: state = 9362; break; // &Verb -> &Verba
				case 9366: state = 9367; break; // &verb -> &verba
				case 9375: state = 9376; break; // &Vertic -> &Vertica
				case 9378: state = 9379; break; // &VerticalB -> &VerticalBa
				case 9389: state = 9390; break; // &VerticalSep -> &VerticalSepa
				case 9391: state = 9392; break; // &VerticalSepar -> &VerticalSepara
				case 9409: state = 9410; break; // &VeryThinSp -> &VeryThinSpa
				case 9472: state = 9473; break; // &Vvd -> &Vvda
				case 9480: state = 9481; break; // &vzigz -> &vzigza
				case 9498: state = 9499; break; // &wedb -> &wedba
				case 9535: state = 9536; break; // &wre -> &wrea
				case 9549: state = 9550; break; // &xc -> &xca
				case 9572: state = 9577; break; // &xh -> &xha
				case 9585: state = 9590; break; // &xl -> &xla
				case 9594: state = 9595; break; // &xm -> &xma
				case 9623: state = 9628; break; // &xr -> &xra
				case 9665: state = 9666; break; // &Y -> &Ya
				case 9672: state = 9673; break; // &y -> &ya
				case 9747: state = 9748; break; // &Z -> &Za
				case 9754: state = 9755; break; // &z -> &za
				case 9761: state = 9762; break; // &Zc -> &Zca
				case 9767: state = 9768; break; // &zc -> &zca
				case 9800: state = 9801; break; // &ZeroWidthSp -> &ZeroWidthSpa
				case 9805: state = 9806; break; // &Zet -> &Zeta
				case 9808: state = 9809; break; // &zet -> &zeta
				case 9827: state = 9828; break; // &zigr -> &zigra
				default: return false;
				}
				break;
			case 'b':
				switch (state) {
				case 0: state = 301; break; // & -> &b
				case 1: state = 15; break; // &A -> &Ab
				case 8: state = 21; break; // &a -> &ab
				case 147: state = 150; break; // &angmsda -> &angmsdab
				case 167: state = 168; break; // &angrtv -> &angrtvb
				case 301: state = 360; break; // &b -> &bb
				case 364: state = 365; break; // &bbrkt -> &bbrktb
				case 613: state = 614; break; // &box -> &boxb
				case 735: state = 736; break; // &brv -> &brvb
				case 758: state = 760; break; // &bsol -> &bsolb
				case 764: state = 765; break; // &bsolhsu -> &bsolhsub
				case 805: state = 811; break; // &cap -> &capb
				case 1101: state = 1102; break; // &CloseCurlyDou -> &CloseCurlyDoub
				case 1118: state = 1119; break; // &clu -> &club
				case 1278: state = 1279; break; // &csu -> &csub
				case 1318: state = 1320; break; // &cup -> &cupb
				case 1432: state = 1463; break; // &d -> &db
				case 1577: state = 1578; break; // &DiacriticalDou -> &DiacriticalDoub
				case 1731: state = 1732; break; // &dou -> &doub
				case 1734: state = 1735; break; // &double -> &doubleb
				case 1744: state = 1745; break; // &Dou -> &Doub
				case 2023: state = 2024; break; // &dr -> &drb
				case 2389: state = 2390; break; // &Equili -> &Equilib
				case 2701: state = 2730; break; // &g -> &gb
				case 2708: state = 2724; break; // &G -> &Gb
				case 3020: state = 3060; break; // &h -> &hb
				case 3101: state = 3102; break; // &Hil -> &Hilb
				case 3166: state = 3167; break; // &hor -> &horb
				case 3225: state = 3226; break; // &hy -> &hyb
				case 3447: state = 3448; break; // &Invisi -> &Invisib
				case 3692: state = 3812; break; // &l -> &lb
				case 3723: state = 3724; break; // &Lam -> &Lamb
				case 3728: state = 3729; break; // &lam -> &lamb
				case 3766: state = 3768; break; // &larr -> &larrb
				case 3812: state = 3817; break; // &lb -> &lbb
				case 3862: state = 3863; break; // &lcu -> &lcub
				case 3963: state = 3964; break; // &LeftDou -> &LeftDoub
				case 4325: state = 4334; break; // &lh -> &lhb
				case 4422: state = 4430; break; // &lo -> &lob
				case 4579: state = 4584; break; // &low -> &lowb
				case 4676: state = 4677; break; // &lsq -> &lsqb
				case 4892: state = 4894; break; // &minus -> &minusb
				case 4965: state = 5010; break; // &n -> &nb
				case 4966: state = 4967; break; // &na -> &nab
				case 5398: state = 5399; break; // &NotDou -> &NotDoub
				case 5521: state = 5524; break; // &notinv -> &notinvb
				case 5623: state = 5626; break; // &notniv -> &notnivb
				case 5701: state = 5702; break; // &NotSquareSu -> &NotSquareSub
				case 5726: state = 5727; break; // &NotSu -> &NotSub
				case 5944: state = 5945; break; // &nsqsu -> &nsqsub
				case 5951: state = 5952; break; // &nsu -> &nsub
				case 6163: state = 6174; break; // &od -> &odb
				case 6168: state = 6169; break; // &Od -> &Odb
				case 6227: state = 6228; break; // &oh -> &ohb
				case 6316: state = 6317; break; // &OpenCurlyDou -> &OpenCurlyDoub
				case 6430: state = 6431; break; // &ov -> &ovb
				case 6567: state = 6574; break; // &plus -> &plusb
				case 6876: state = 6996; break; // &r -> &rb
				case 6932: state = 6937; break; // &rarr -> &rarrb
				case 6996: state = 7001; break; // &rb -> &rbb
				case 7046: state = 7047; break; // &rcu -> &rcub
				case 7114: state = 7115; break; // &ReverseEquili -> &ReverseEquilib
				case 7128: state = 7129; break; // &ReverseUpEquili -> &ReverseUpEquilib
				case 7238: state = 7239; break; // &RightDou -> &RightDoub
				case 7469: state = 7477; break; // &ro -> &rob
				case 7559: state = 7560; break; // &rsq -> &rsqb
				case 7617: state = 7624; break; // &s -> &sb
				case 7697: state = 7699; break; // &sdot -> &sdotb
				case 7942: state = 7944; break; // &sol -> &solb
				case 7985: state = 7986; break; // &sqsu -> &sqsub
				case 8033: state = 8034; break; // &SquareSu -> &SquareSub
				case 8127: state = 8128; break; // &Su -> &Sub
				case 8130: state = 8131; break; // &su -> &sub
				case 8193: state = 8194; break; // &subsu -> &subsub
				case 8297: state = 8298; break; // &supdsu -> &supdsub
				case 8325: state = 8326; break; // &suphsu -> &suphsub
				case 8370: state = 8371; break; // &supsu -> &supsub
				case 8401: state = 8402; break; // &Ta -> &Tab
				case 8404: state = 8415; break; // &t -> &tb
				case 8578: state = 8580; break; // &times -> &timesb
				case 8594: state = 8596; break; // &top -> &topb
				case 8690: state = 8691; break; // &tris -> &trisb
				case 8768: state = 8797; break; // &U -> &Ub
				case 8775: state = 8802; break; // &u -> &ub
				case 8829: state = 8840; break; // &ud -> &udb
				case 8834: state = 8835; break; // &Ud -> &Udb
				case 8876: state = 8883; break; // &uh -> &uhb
				case 9039: state = 9040; break; // &UpEquili -> &UpEquilib
				case 9258: state = 9259; break; // &varsu -> &varsub
				case 9303: state = 9304; break; // &V -> &Vb
				case 9346: state = 9348; break; // &vee -> &veeb
				case 9360: state = 9361; break; // &Ver -> &Verb
				case 9365: state = 9366; break; // &ver -> &verb
				case 9427: state = 9428; break; // &vnsu -> &vnsub
				case 9458: state = 9459; break; // &vsu -> &vsub
				case 9497: state = 9498; break; // &wed -> &wedb
				default: return false;
				}
				break;
			case 'c':
				switch (state) {
				case 0: state = 796; break; // & -> &c
				case 1: state = 33; break; // &A -> &Ac
				case 2: state = 3; break; // &Aa -> &Aac
				case 8: state = 27; break; // &a -> &ac
				case 9: state = 10; break; // &aa -> &aac
				case 35: state = 36; break; // &Acir -> &Acirc
				case 39: state = 40; break; // &acir -> &acirc
				case 99: state = 100; break; // &Ama -> &Amac
				case 104: state = 105; break; // &ama -> &amac
				case 147: state = 152; break; // &angmsda -> &angmsdac
				case 201: state = 202; break; // &apa -> &apac
				case 222: state = 223; break; // &ApplyFun -> &ApplyFunc
				case 247: state = 248; break; // &As -> &Asc
				case 251: state = 252; break; // &as -> &asc
				case 289: state = 290; break; // &aw -> &awc
				case 301: state = 369; break; // &b -> &bc
				case 302: state = 303; break; // &ba -> &bac
				case 304: state = 305; break; // &back -> &backc
				case 331: state = 374; break; // &B -> &Bc
				case 332: state = 333; break; // &Ba -> &Bac
				case 384: state = 385; break; // &be -> &bec
				case 390: state = 391; break; // &Be -> &Bec
				case 443: state = 444; break; // &big -> &bigc
				case 449: state = 450; break; // &bigcir -> &bigcirc
				case 472: state = 473; break; // &bigsq -> &bigsqc
				case 520: state = 521; break; // &bla -> &blac
				case 575: state = 576; break; // &blo -> &bloc
				case 740: state = 741; break; // &Bs -> &Bsc
				case 744: state = 745; break; // &bs -> &bsc
				case 789: state = 866; break; // &C -> &Cc
				case 790: state = 791; break; // &Ca -> &Cac
				case 796: state = 861; break; // &c -> &cc
				case 797: state = 798; break; // &ca -> &cac
				case 805: state = 817; break; // &cap -> &capc
				case 812: state = 813; break; // &capbr -> &capbrc
				case 887: state = 888; break; // &Ccir -> &Ccirc
				case 891: state = 892; break; // &ccir -> &ccirc
				case 956: state = 957; break; // &CH -> &CHc
				case 960: state = 961; break; // &ch -> &chc
				case 964: state = 965; break; // &che -> &chec
				case 979: state = 981; break; // &cir -> &circ
				case 1004: state = 1009; break; // &circled -> &circledc
				case 1011: state = 1012; break; // &circledcir -> &circledcirc
				case 1020: state = 1021; break; // &Cir -> &Circ
				case 1063: state = 1064; break; // &cirs -> &cirsc
				case 1069: state = 1070; break; // &Clo -> &Cloc
				case 1213: state = 1214; break; // &Coprodu -> &Coproduc
				case 1233: state = 1234; break; // &CounterClo -> &CounterCloc
				case 1270: state = 1271; break; // &Cs -> &Csc
				case 1274: state = 1275; break; // &cs -> &csc
				case 1305: state = 1306; break; // &cues -> &cuesc
				case 1318: state = 1330; break; // &cup -> &cupc
				case 1321: state = 1322; break; // &cupbr -> &cupbrc
				case 1359: state = 1360; break; // &curlyeqpre -> &curlyeqprec
				case 1363: state = 1364; break; // &curlyeqsu -> &curlyeqsuc
				case 1364: state = 1365; break; // &curlyeqsuc -> &curlyeqsucc
				case 1407: state = 1408; break; // &cw -> &cwc
				case 1420: state = 1421; break; // &cyl -> &cylc
				case 1425: state = 1474; break; // &D -> &Dc
				case 1432: state = 1480; break; // &d -> &dc
				case 1471: state = 1472; break; // &dbla -> &dblac
				case 1558: state = 1559; break; // &Dia -> &Diac
				case 1563: state = 1564; break; // &Diacriti -> &Diacritic
				case 1567: state = 1568; break; // &DiacriticalA -> &DiacriticalAc
				case 1581: state = 1582; break; // &DiacriticalDoubleA -> &DiacriticalDoubleAc
				case 1661: state = 1662; break; // &DJ -> &DJc
				case 1665: state = 1666; break; // &dj -> &djc
				case 1669: state = 1670; break; // &dl -> &dlc
				case 1873: state = 1874; break; // &DoubleVerti -> &DoubleVertic
				case 1960: state = 1961; break; // &DownLeftRightVe -> &DownLeftRightVec
				case 1970: state = 1971; break; // &DownLeftTeeVe -> &DownLeftTeeVec
				case 1977: state = 1978; break; // &DownLeftVe -> &DownLeftVec
				case 1996: state = 1997; break; // &DownRightTeeVe -> &DownRightTeeVec
				case 2003: state = 2004; break; // &DownRightVe -> &DownRightVec
				case 2023: state = 2031; break; // &dr -> &drc
				case 2040: state = 2041; break; // &Ds -> &Dsc
				case 2044: state = 2045; break; // &ds -> &dsc
				case 2048: state = 2049; break; // &DS -> &DSc
				case 2093: state = 2094; break; // &DZ -> &DZc
				case 2097: state = 2098; break; // &dz -> &dzc
				case 2108: state = 2127; break; // &E -> &Ec
				case 2109: state = 2110; break; // &Ea -> &Eac
				case 2115: state = 2133; break; // &e -> &ec
				case 2116: state = 2117; break; // &ea -> &eac
				case 2140: state = 2146; break; // &ecir -> &ecirc
				case 2143: state = 2144; break; // &Ecir -> &Ecirc
				case 2229: state = 2230; break; // &Ema -> &Emac
				case 2234: state = 2235; break; // &ema -> &emac
				case 2339: state = 2340; break; // &eq -> &eqc
				case 2342: state = 2343; break; // &eqcir -> &eqcirc
				case 2418: state = 2419; break; // &Es -> &Esc
				case 2422: state = 2423; break; // &es -> &esc
				case 2458: state = 2459; break; // &ex -> &exc
				case 2473: state = 2474; break; // &expe -> &expec
				case 2503: state = 2521; break; // &f -> &fc
				case 2517: state = 2518; break; // &F -> &Fc
				case 2648: state = 2649; break; // &fra -> &frac
				case 2693: state = 2694; break; // &Fs -> &Fsc
				case 2697: state = 2698; break; // &fs -> &fsc
				case 2701: state = 2746; break; // &g -> &gc
				case 2702: state = 2703; break; // &ga -> &gac
				case 2708: state = 2736; break; // &G -> &Gc
				case 2743: state = 2744; break; // &Gcir -> &Gcirc
				case 2748: state = 2749; break; // &gcir -> &gcirc
				case 2781: state = 2783; break; // &ges -> &gesc
				case 2783: state = 2784; break; // &gesc -> &gescc
				case 2816: state = 2817; break; // &GJ -> &GJc
				case 2820: state = 2821; break; // &gj -> &gjc
				case 2923: state = 2924; break; // &Gs -> &Gsc
				case 2927: state = 2928; break; // &gs -> &gsc
				case 2942: state = 2944; break; // &gt -> &gtc
				case 2944: state = 2945; break; // &gtc -> &gtcc
				case 3014: state = 3064; break; // &H -> &Hc
				case 3015: state = 3016; break; // &Ha -> &Hac
				case 3020: state = 3069; break; // &h -> &hc
				case 3037: state = 3038; break; // &HARD -> &HARDc
				case 3042: state = 3043; break; // &hard -> &hardc
				case 3050: state = 3052; break; // &harr -> &harrc
				case 3066: state = 3067; break; // &Hcir -> &Hcirc
				case 3071: state = 3072; break; // &hcir -> &hcirc
				case 3089: state = 3090; break; // &her -> &herc
				case 3108: state = 3109; break; // &HilbertSpa -> &HilbertSpac
				case 3184: state = 3185; break; // &Hs -> &Hsc
				case 3188: state = 3189; break; // &hs -> &hsc
				case 3236: state = 3252; break; // &I -> &Ic
				case 3237: state = 3238; break; // &Ia -> &Iac
				case 3243: state = 3250; break; // &i -> &ic
				case 3244: state = 3245; break; // &ia -> &iac
				case 3254: state = 3255; break; // &Icir -> &Icirc
				case 3258: state = 3259; break; // &icir -> &icirc
				case 3269: state = 3270; break; // &IE -> &IEc
				case 3273: state = 3274; break; // &ie -> &iec
				case 3277: state = 3278; break; // &iex -> &iexc
				case 3332: state = 3333; break; // &Ima -> &Imac
				case 3337: state = 3338; break; // &ima -> &imac
				case 3378: state = 3380; break; // &in -> &inc
				case 3401: state = 3403; break; // &int -> &intc
				case 3419: state = 3420; break; // &inter -> &interc
				case 3426: state = 3427; break; // &Interse -> &Intersec
				case 3463: state = 3464; break; // &IO -> &IOc
				case 3467: state = 3468; break; // &io -> &ioc
				case 3503: state = 3504; break; // &Is -> &Isc
				case 3507: state = 3508; break; // &is -> &isc
				case 3540: state = 3541; break; // &Iuk -> &Iukc
				case 3545: state = 3546; break; // &iuk -> &iukc
				case 3555: state = 3556; break; // &J -> &Jc
				case 3558: state = 3559; break; // &Jcir -> &Jcirc
				case 3561: state = 3562; break; // &j -> &jc
				case 3564: state = 3565; break; // &jcir -> &jcirc
				case 3590: state = 3591; break; // &Js -> &Jsc
				case 3594: state = 3595; break; // &js -> &jsc
				case 3599: state = 3600; break; // &Jser -> &Jserc
				case 3604: state = 3605; break; // &jser -> &jserc
				case 3609: state = 3610; break; // &Juk -> &Jukc
				case 3614: state = 3615; break; // &juk -> &jukc
				case 3618: state = 3632; break; // &K -> &Kc
				case 3624: state = 3638; break; // &k -> &kc
				case 3660: state = 3661; break; // &KH -> &KHc
				case 3664: state = 3665; break; // &kh -> &khc
				case 3668: state = 3669; break; // &KJ -> &KJc
				case 3672: state = 3673; break; // &kj -> &kjc
				case 3684: state = 3685; break; // &Ks -> &Ksc
				case 3688: state = 3689; break; // &ks -> &ksc
				case 3692: state = 3843; break; // &l -> &lc
				case 3698: state = 3837; break; // &L -> &Lc
				case 3699: state = 3700; break; // &La -> &Lac
				case 3705: state = 3706; break; // &la -> &lac
				case 3748: state = 3749; break; // &Lapla -> &Laplac
				case 3822: state = 3823; break; // &lbra -> &lbrac
				case 3869: state = 3870; break; // &ld -> &ldc
				case 3908: state = 3909; break; // &LeftAngleBra -> &LeftAngleBrac
				case 3969: state = 3970; break; // &LeftDoubleBra -> &LeftDoubleBrac
				case 3981: state = 3982; break; // &LeftDownTeeVe -> &LeftDownTeeVec
				case 3988: state = 3989; break; // &LeftDownVe -> &LeftDownVec
				case 4086: state = 4087; break; // &LeftRightVe -> &LeftRightVec
				case 4103: state = 4104; break; // &LeftTeeVe -> &LeftTeeVec
				case 4145: state = 4146; break; // &LeftUpDownVe -> &LeftUpDownVec
				case 4155: state = 4156; break; // &LeftUpTeeVe -> &LeftUpTeeVec
				case 4162: state = 4163; break; // &LeftUpVe -> &LeftUpVec
				case 4173: state = 4174; break; // &LeftVe -> &LeftVec
				case 4197: state = 4199; break; // &les -> &lesc
				case 4199: state = 4200; break; // &lesc -> &lescc
				case 4338: state = 4339; break; // &LJ -> &LJc
				case 4342: state = 4343; break; // &lj -> &ljc
				case 4348: state = 4354; break; // &ll -> &llc
				case 4396: state = 4397; break; // &lmousta -> &lmoustac
				case 4628: state = 4633; break; // &lr -> &lrc
				case 4652: state = 4662; break; // &ls -> &lsc
				case 4658: state = 4659; break; // &Ls -> &Lsc
				case 4698: state = 4700; break; // &lt -> &ltc
				case 4700: state = 4701; break; // &ltc -> &ltcc
				case 4767: state = 4809; break; // &m -> &mc
				case 4768: state = 4769; break; // &ma -> &mac
				case 4781: state = 4815; break; // &M -> &Mc
				case 4850: state = 4851; break; // &MediumSpa -> &MediumSpac
				case 4871: state = 4872; break; // &mi -> &mic
				case 4876: state = 4882; break; // &mid -> &midc
				case 4909: state = 4910; break; // &ml -> &mlc
				case 4937: state = 4938; break; // &Ms -> &Msc
				case 4941: state = 4942; break; // &ms -> &msc
				case 4965: state = 5020; break; // &n -> &nc
				case 4966: state = 4978; break; // &na -> &nac
				case 4971: state = 5024; break; // &N -> &Nc
				case 4972: state = 4973; break; // &Na -> &Nac
				case 5099: state = 5100; break; // &NegativeMediumSpa -> &NegativeMediumSpac
				case 5105: state = 5106; break; // &NegativeThi -> &NegativeThic
				case 5110: state = 5111; break; // &NegativeThickSpa -> &NegativeThickSpac
				case 5117: state = 5118; break; // &NegativeThinSpa -> &NegativeThinSpac
				case 5131: state = 5132; break; // &NegativeVeryThinSpa -> &NegativeVeryThinSpac
				case 5248: state = 5249; break; // &NJ -> &NJc
				case 5252: state = 5253; break; // &nj -> &njc
				case 5365: state = 5366; break; // &NonBreakingSpa -> &NonBreakingSpac
				case 5406: state = 5407; break; // &NotDoubleVerti -> &NotDoubleVertic
				case 5521: state = 5526; break; // &notinv -> &notinvc
				case 5623: state = 5628; break; // &notniv -> &notnivc
				case 5632: state = 5633; break; // &NotPre -> &NotPrec
				case 5726: state = 5738; break; // &NotSu -> &NotSuc
				case 5738: state = 5739; break; // &NotSuc -> &NotSucc
				case 5813: state = 5814; break; // &NotVerti -> &NotVertic
				case 5842: state = 5844; break; // &npr -> &nprc
				case 5848: state = 5850; break; // &npre -> &nprec
				case 5862: state = 5864; break; // &nrarr -> &nrarrc
				case 5895: state = 5896; break; // &ns -> &nsc
				case 5896: state = 5898; break; // &nsc -> &nscc
				case 5904: state = 5905; break; // &Ns -> &Nsc
				case 5951: state = 5967; break; // &nsu -> &nsuc
				case 5967: state = 5968; break; // &nsuc -> &nsucc
				case 6131: state = 6152; break; // &O -> &Oc
				case 6132: state = 6133; break; // &Oa -> &Oac
				case 6138: state = 6148; break; // &o -> &oc
				case 6139: state = 6140; break; // &oa -> &oac
				case 6150: state = 6157; break; // &ocir -> &ocirc
				case 6154: state = 6155; break; // &Ocir -> &Ocirc
				case 6171: state = 6172; break; // &Odbla -> &Odblac
				case 6176: state = 6177; break; // &odbla -> &odblac
				case 6200: state = 6201; break; // &of -> &ofc
				case 6238: state = 6243; break; // &ol -> &olc
				case 6259: state = 6260; break; // &Oma -> &Omac
				case 6264: state = 6265; break; // &oma -> &omac
				case 6276: state = 6277; break; // &Omi -> &Omic
				case 6282: state = 6283; break; // &omi -> &omic
				case 6378: state = 6379; break; // &Os -> &Osc
				case 6382: state = 6383; break; // &os -> &osc
				case 6443: state = 6444; break; // &OverBra -> &OverBrac
				case 6463: state = 6494; break; // &p -> &pc
				case 6482: state = 6491; break; // &P -> &Pc
				case 6498: state = 6499; break; // &per -> &perc
				case 6545: state = 6546; break; // &pit -> &pitc
				case 6557: state = 6558; break; // &plan -> &planc
				case 6567: state = 6576; break; // &plus -> &plusc
				case 6569: state = 6570; break; // &plusa -> &plusac
				case 6611: state = 6612; break; // &Poin -> &Poinc
				case 6642: state = 6647; break; // &pr -> &prc
				case 6653: state = 6655; break; // &pre -> &prec
				case 6655: state = 6664; break; // &prec -> &precc
				case 6672: state = 6673; break; // &Pre -> &Prec
				case 6750: state = 6751; break; // &Produ -> &Produc
				case 6795: state = 6796; break; // &Ps -> &Psc
				case 6799: state = 6800; break; // &ps -> &psc
				case 6808: state = 6809; break; // &pun -> &punc
				case 6839: state = 6840; break; // &Qs -> &Qsc
				case 6843: state = 6844; break; // &qs -> &qsc
				case 6876: state = 7027; break; // &r -> &rc
				case 6882: state = 6883; break; // &ra -> &rac
				case 6886: state = 7021; break; // &R -> &Rc
				case 6887: state = 6888; break; // &Ra -> &Rac
				case 6898: state = 6899; break; // &radi -> &radic
				case 6932: state = 6942; break; // &rarr -> &rarrc
				case 7006: state = 7007; break; // &rbra -> &rbrac
				case 7053: state = 7054; break; // &rd -> &rdc
				case 7074: state = 7089; break; // &re -> &rec
				case 7182: state = 7183; break; // &RightAngleBra -> &RightAngleBrac
				case 7244: state = 7245; break; // &RightDoubleBra -> &RightDoubleBrac
				case 7256: state = 7257; break; // &RightDownTeeVe -> &RightDownTeeVec
				case 7263: state = 7264; break; // &RightDownVe -> &RightDownVec
				case 7348: state = 7349; break; // &RightTeeVe -> &RightTeeVec
				case 7390: state = 7391; break; // &RightUpDownVe -> &RightUpDownVec
				case 7400: state = 7401; break; // &RightUpTeeVe -> &RightUpTeeVec
				case 7407: state = 7408; break; // &RightUpVe -> &RightUpVec
				case 7418: state = 7419; break; // &RightVe -> &RightVec
				case 7459: state = 7460; break; // &rmousta -> &rmoustac
				case 7542: state = 7552; break; // &rs -> &rsc
				case 7548: state = 7549; break; // &Rs -> &Rsc
				case 7610: state = 7629; break; // &S -> &Sc
				case 7611: state = 7612; break; // &Sa -> &Sac
				case 7617: state = 7631; break; // &s -> &sc
				case 7618: state = 7619; break; // &sa -> &sac
				case 7631: state = 7645; break; // &sc -> &scc
				case 7663: state = 7664; break; // &Scir -> &Scirc
				case 7667: state = 7668; break; // &scir -> &scirc
				case 7703: state = 7718; break; // &se -> &sec
				case 7751: state = 7762; break; // &sh -> &shc
				case 7756: state = 7767; break; // &SH -> &SHc
				case 7758: state = 7759; break; // &SHCH -> &SHCHc
				case 7763: state = 7764; break; // &shch -> &shchc
				case 7889: state = 7890; break; // &SmallCir -> &SmallCirc
				case 7932: state = 7933; break; // &SOFT -> &SOFTc
				case 7938: state = 7939; break; // &soft -> &softc
				case 7968: state = 7969; break; // &sq -> &sqc
				case 8025: state = 8026; break; // &SquareInterse -> &SquareIntersec
				case 8073: state = 8074; break; // &Ss -> &Ssc
				case 8077: state = 8078; break; // &ss -> &ssc
				case 8127: state = 8216; break; // &Su -> &Suc
				case 8130: state = 8198; break; // &su -> &suc
				case 8198: state = 8199; break; // &suc -> &succ
				case 8199: state = 8208; break; // &succ -> &succc
				case 8216: state = 8217; break; // &Suc -> &Succ
				case 8400: state = 8419; break; // &T -> &Tc
				case 8404: state = 8425; break; // &t -> &tc
				case 8452: state = 8453; break; // &telre -> &telrec
				case 8493: state = 8494; break; // &thi -> &thic
				case 8507: state = 8508; break; // &Thi -> &Thic
				case 8512: state = 8513; break; // &ThickSpa -> &ThickSpac
				case 8523: state = 8524; break; // &ThinSpa -> &ThinSpac
				case 8594: state = 8600; break; // &top -> &topc
				case 8705: state = 8706; break; // &Ts -> &Tsc
				case 8709: state = 8710; break; // &ts -> &tsc
				case 8713: state = 8714; break; // &TS -> &TSc
				case 8719: state = 8720; break; // &TSH -> &TSHc
				case 8723: state = 8724; break; // &tsh -> &tshc
				case 8768: state = 8815; break; // &U -> &Uc
				case 8769: state = 8770; break; // &Ua -> &Uac
				case 8775: state = 8820; break; // &u -> &uc
				case 8776: state = 8777; break; // &ua -> &uac
				case 8792: state = 8793; break; // &Uarro -> &Uarroc
				case 8798: state = 8799; break; // &Ubr -> &Ubrc
				case 8803: state = 8804; break; // &ubr -> &ubrc
				case 8817: state = 8818; break; // &Ucir -> &Ucirc
				case 8822: state = 8823; break; // &ucir -> &ucirc
				case 8837: state = 8838; break; // &Udbla -> &Udblac
				case 8842: state = 8843; break; // &udbla -> &udblac
				case 8887: state = 8888; break; // &ul -> &ulc
				case 8905: state = 8906; break; // &Uma -> &Umac
				case 8910: state = 8911; break; // &uma -> &umac
				case 8925: state = 8926; break; // &UnderBra -> &UnderBrac
				case 9127: state = 9128; break; // &ur -> &urc
				case 9153: state = 9154; break; // &Us -> &Usc
				case 9157: state = 9158; break; // &us -> &usc
				case 9201: state = 9317; break; // &v -> &vc
				case 9303: state = 9314; break; // &V -> &Vc
				case 9374: state = 9375; break; // &Verti -> &Vertic
				case 9410: state = 9411; break; // &VeryThinSpa -> &VeryThinSpac
				case 9450: state = 9451; break; // &Vs -> &Vsc
				case 9454: state = 9455; break; // &vs -> &vsc
				case 9484: state = 9485; break; // &W -> &Wc
				case 9487: state = 9488; break; // &Wcir -> &Wcirc
				case 9490: state = 9491; break; // &w -> &wc
				case 9493: state = 9494; break; // &wcir -> &wcirc
				case 9540: state = 9541; break; // &Ws -> &Wsc
				case 9544: state = 9545; break; // &ws -> &wsc
				case 9548: state = 9549; break; // &x -> &xc
				case 9554: state = 9555; break; // &xcir -> &xcirc
				case 9632: state = 9633; break; // &Xs -> &Xsc
				case 9636: state = 9637; break; // &xs -> &xsc
				case 9640: state = 9641; break; // &xsq -> &xsqc
				case 9665: state = 9685; break; // &Y -> &Yc
				case 9666: state = 9667; break; // &Ya -> &Yac
				case 9672: state = 9690; break; // &y -> &yc
				case 9673: state = 9674; break; // &ya -> &yac
				case 9679: state = 9680; break; // &YA -> &YAc
				case 9687: state = 9688; break; // &Ycir -> &Ycirc
				case 9692: state = 9693; break; // &ycir -> &ycirc
				case 9708: state = 9709; break; // &YI -> &YIc
				case 9712: state = 9713; break; // &yi -> &yic
				case 9724: state = 9725; break; // &Ys -> &Ysc
				case 9728: state = 9729; break; // &ys -> &ysc
				case 9732: state = 9733; break; // &YU -> &YUc
				case 9736: state = 9737; break; // &yu -> &yuc
				case 9747: state = 9761; break; // &Z -> &Zc
				case 9748: state = 9749; break; // &Za -> &Zac
				case 9754: state = 9767; break; // &z -> &zc
				case 9755: state = 9756; break; // &za -> &zac
				case 9801: state = 9802; break; // &ZeroWidthSpa -> &ZeroWidthSpac
				case 9817: state = 9818; break; // &ZH -> &ZHc
				case 9821: state = 9822; break; // &zh -> &zhc
				case 9840: state = 9841; break; // &Zs -> &Zsc
				case 9844: state = 9845; break; // &zs -> &zsc
				default: return false;
				}
				break;
			case 'd':
				switch (state) {
				case 0: state = 1432; break; // & -> &d
				case 27: state = 29; break; // &ac -> &acd
				case 116: state = 117; break; // &An -> &And
				case 119: state = 120; break; // &an -> &and
				case 120: state = 126; break; // &and -> &andd
				case 123: state = 124; break; // &andan -> &andand
				case 144: state = 145; break; // &angms -> &angmsd
				case 147: state = 154; break; // &angmsda -> &angmsdad
				case 168: state = 170; break; // &angrtvb -> &angrtvbd
				case 210: state = 211; break; // &api -> &apid
				case 271: state = 272; break; // &Atil -> &Atild
				case 277: state = 278; break; // &atil -> &atild
				case 301: state = 379; break; // &b -> &bd
				case 350: state = 351; break; // &Barwe -> &Barwed
				case 354: state = 355; break; // &barwe -> &barwed
				case 455: state = 456; break; // &bigo -> &bigod
				case 488: state = 489; break; // &bigtriangle -> &bigtriangled
				case 508: state = 509; break; // &bigwe -> &bigwed
				case 545: state = 547; break; // &blacktriangle -> &blacktriangled
				case 613: state = 623; break; // &box -> &boxd
				case 636: state = 642; break; // &boxH -> &boxHd
				case 638: state = 646; break; // &boxh -> &boxhd
				case 789: state = 907; break; // &C -> &Cd
				case 796: state = 911; break; // &c -> &cd
				case 805: state = 824; break; // &cap -> &capd
				case 808: state = 809; break; // &capan -> &capand
				case 876: state = 877; break; // &Cce -> &Cced
				case 881: state = 882; break; // &cce -> &cced
				case 915: state = 916; break; // &ce -> &ced
				case 920: state = 921; break; // &Ce -> &Ced
				case 945: state = 946; break; // &center -> &centerd
				case 987: state = 1004; break; // &circle -> &circled
				case 1004: state = 1014; break; // &circled -> &circledd
				case 1060: state = 1061; break; // &cirmi -> &cirmid
				case 1165: state = 1167; break; // &cong -> &congd
				case 1207: state = 1208; break; // &copro -> &coprod
				case 1211: state = 1212; break; // &Copro -> &Coprod
				case 1287: state = 1288; break; // &ct -> &ctd
				case 1292: state = 1293; break; // &cu -> &cud
				case 1318: state = 1337; break; // &cup -> &cupd
				case 1372: state = 1373; break; // &curlywe -> &curlywed
				case 1404: state = 1405; break; // &cuwe -> &cuwed
				case 1432: state = 1492; break; // &d -> &dd
				case 1507: state = 1508; break; // &DDotrah -> &DDotrahd
				case 1595: state = 1596; break; // &DiacriticalTil -> &DiacriticalTild
				case 1605: state = 1606; break; // &Diamon -> &Diamond
				case 1609: state = 1610; break; // &diamon -> &diamond
				case 1645: state = 1646; break; // &divi -> &divid
				case 1701: state = 1703; break; // &doteq -> &doteqd
				case 1739: state = 1740; break; // &doublebarwe -> &doublebarwed
				case 1896: state = 1921; break; // &down -> &downd
				case 2067: state = 2068; break; // &dt -> &dtd
				case 2108: state = 2162; break; // &E -> &Ed
				case 2115: state = 2169; break; // &e -> &ed
				case 2198: state = 2200; break; // &egs -> &egsd
				case 2222: state = 2224; break; // &els -> &elsd
				case 2379: state = 2380; break; // &EqualTil -> &EqualTild
				case 2422: state = 2426; break; // &es -> &esd
				case 2509: state = 2510; break; // &falling -> &fallingd
				case 2557: state = 2558; break; // &Fille -> &Filled
				case 2701: state = 2759; break; // &g -> &gd
				case 2708: state = 2755; break; // &G -> &Gd
				case 2712: state = 2718; break; // &Gamma -> &Gammad
				case 2716: state = 2720; break; // &gamma -> &gammad
				case 2737: state = 2738; break; // &Gce -> &Gced
				case 2781: state = 2786; break; // &ges -> &gesd
				case 2919: state = 2920; break; // &GreaterTil -> &GreaterTild
				case 2942: state = 2950; break; // &gt -> &gtd
				case 2965: state = 2976; break; // &gtr -> &gtrd
				case 3041: state = 3042; break; // &har -> &hard
				case 3236: state = 3265; break; // &I -> &Id
				case 3369: state = 3370; break; // &impe -> &imped
				case 3393: state = 3394; break; // &ino -> &inod
				case 3441: state = 3442; break; // &intpro -> &intprod
				case 3494: state = 3495; break; // &ipro -> &iprod
				case 3512: state = 3514; break; // &isin -> &isind
				case 3530: state = 3531; break; // &Itil -> &Itild
				case 3535: state = 3536; break; // &itil -> &itild
				case 3633: state = 3634; break; // &Kce -> &Kced
				case 3639: state = 3640; break; // &kce -> &kced
				case 3692: state = 3869; break; // &l -> &ld
				case 3724: state = 3725; break; // &Lamb -> &Lambd
				case 3729: state = 3730; break; // &lamb -> &lambd
				case 3737: state = 3739; break; // &lang -> &langd
				case 3832: state = 3833; break; // &lbrksl -> &lbrksld
				case 3849: state = 3850; break; // &Lce -> &Lced
				case 3854: state = 3855; break; // &lce -> &lced
				case 3879: state = 3880; break; // &ldr -> &ldrd
				case 4010: state = 4011; break; // &leftharpoon -> &leftharpoond
				case 4197: state = 4202; break; // &les -> &lesd
				case 4215: state = 4223; break; // &less -> &lessd
				case 4297: state = 4298; break; // &LessTil -> &LessTild
				case 4327: state = 4328; break; // &lhar -> &lhard
				case 4372: state = 4373; break; // &llhar -> &llhard
				case 4380: state = 4381; break; // &Lmi -> &Lmid
				case 4386: state = 4387; break; // &lmi -> &lmid
				case 4642: state = 4644; break; // &lrhar -> &lrhard
				case 4698: state = 4706; break; // &lt -> &ltd
				case 4743: state = 4744; break; // &lur -> &lurd
				case 4767: state = 4820; break; // &m -> &md
				case 4789: state = 4791; break; // &mapsto -> &mapstod
				case 4835: state = 4836; break; // &measure -> &measured
				case 4843: state = 4844; break; // &Me -> &Med
				case 4871: state = 4876; break; // &mi -> &mid
				case 4876: state = 4886; break; // &mid -> &midd
				case 4892: state = 4896; break; // &minus -> &minusd
				case 4909: state = 4913; break; // &ml -> &mld
				case 4922: state = 4923; break; // &mo -> &mod
				case 4965: state = 5059; break; // &n -> &nd
				case 4990: state = 4991; break; // &napi -> &napid
				case 5034: state = 5035; break; // &Nce -> &Nced
				case 5039: state = 5040; break; // &nce -> &nced
				case 5046: state = 5048; break; // &ncong -> &ncongd
				case 5064: state = 5080; break; // &ne -> &ned
				case 5092: state = 5093; break; // &NegativeMe -> &NegativeMed
				case 5150: state = 5151; break; // &Neste -> &Nested
				case 5242: state = 5244; break; // &nis -> &nisd
				case 5256: state = 5265; break; // &nl -> &nld
				case 5344: state = 5345; break; // &nmi -> &nmid
				case 5429: state = 5430; break; // &NotEqualTil -> &NotEqualTild
				case 5489: state = 5490; break; // &NotGreaterTil -> &NotGreaterTild
				case 5513: state = 5515; break; // &notin -> &notind
				case 5586: state = 5587; break; // &NotLessTil -> &NotLessTild
				case 5594: state = 5595; break; // &NotNeste -> &NotNested
				case 5634: state = 5635; break; // &NotPrece -> &NotPreced
				case 5741: state = 5742; break; // &NotSuccee -> &NotSucceed
				case 5764: state = 5765; break; // &NotSucceedsTil -> &NotSucceedsTild
				case 5783: state = 5784; break; // &NotTil -> &NotTild
				case 5805: state = 5806; break; // &NotTildeTil -> &NotTildeTild
				case 5915: state = 5916; break; // &nshortmi -> &nshortmid
				case 5935: state = 5936; break; // &nsmi -> &nsmid
				case 5994: state = 5995; break; // &Ntil -> &Ntild
				case 5999: state = 6000; break; // &ntil -> &ntild
				case 6043: state = 6063; break; // &nv -> &nvd
				case 6047: state = 6053; break; // &nV -> &nVd
				case 6131: state = 6168; break; // &O -> &Od
				case 6138: state = 6163; break; // &o -> &od
				case 6187: state = 6188; break; // &odsol -> &odsold
				case 6282: state = 6288; break; // &omi -> &omid
				case 6342: state = 6348; break; // &or -> &ord
				case 6401: state = 6402; break; // &Otil -> &Otild
				case 6407: state = 6408; break; // &otil -> &otild
				case 6504: state = 6505; break; // &perio -> &period
				case 6567: state = 6580; break; // &plus -> &plusd
				case 6637: state = 6638; break; // &poun -> &pound
				case 6674: state = 6675; break; // &Prece -> &Preced
				case 6698: state = 6699; break; // &PrecedesTil -> &PrecedesTild
				case 6745: state = 6746; break; // &pro -> &prod
				case 6748: state = 6749; break; // &Pro -> &Prod
				case 6876: state = 7053; break; // &r -> &rd
				case 6882: state = 6897; break; // &ra -> &rad
				case 6912: state = 6914; break; // &rang -> &rangd
				case 7016: state = 7017; break; // &rbrksl -> &rbrksld
				case 7033: state = 7034; break; // &Rce -> &Rced
				case 7038: state = 7039; break; // &rce -> &rced
				case 7057: state = 7058; break; // &rdl -> &rdld
				case 7157: state = 7158; break; // &rhar -> &rhard
				case 7285: state = 7286; break; // &rightharpoon -> &rightharpoond
				case 7434: state = 7435; break; // &rising -> &risingd
				case 7466: state = 7467; break; // &rnmi -> &rnmid
				case 7502: state = 7503; break; // &Roun -> &Round
				case 7598: state = 7599; break; // &RuleDelaye -> &RuleDelayed
				case 7617: state = 7695; break; // &s -> &sd
				case 7651: state = 7658; break; // &sce -> &sced
				case 7653: state = 7654; break; // &Sce -> &Sced
				case 7800: state = 7801; break; // &shortmi -> &shortmid
				case 7847: state = 7849; break; // &sim -> &simd
				case 7918: state = 7919; break; // &smi -> &smid
				case 7957: state = 7958; break; // &spa -> &spad
				case 8131: state = 8133; break; // &sub -> &subd
				case 8139: state = 8141; break; // &sube -> &subed
				case 8219: state = 8220; break; // &Succee -> &Succeed
				case 8242: state = 8243; break; // &SucceedsTil -> &SucceedsTild
				case 8284: state = 8292; break; // &sup -> &supd
				case 8302: state = 8304; break; // &supe -> &suped
				case 8404: state = 8445; break; // &t -> &td
				case 8431: state = 8432; break; // &Tce -> &Tced
				case 8436: state = 8437; break; // &tce -> &tced
				case 8545: state = 8546; break; // &Til -> &Tild
				case 8550: state = 8551; break; // &til -> &tild
				case 8572: state = 8573; break; // &TildeTil -> &TildeTild
				case 8578: state = 8585; break; // &times -> &timesd
				case 8629: state = 8630; break; // &tra -> &trad
				case 8633: state = 8664; break; // &tri -> &trid
				case 8638: state = 8640; break; // &triangle -> &triangled
				case 8745: state = 8746; break; // &twohea -> &twohead
				case 8768: state = 8834; break; // &U -> &Ud
				case 8775: state = 8829; break; // &u -> &ud
				case 8916: state = 8917; break; // &Un -> &Und
				case 8970: state = 9014; break; // &Up -> &Upd
				case 8983: state = 9024; break; // &up -> &upd
				case 9161: state = 9162; break; // &ut -> &utd
				case 9168: state = 9169; break; // &Util -> &Utild
				case 9173: state = 9174; break; // &util -> &utild
				case 9201: state = 9335; break; // &v -> &vd
				case 9303: state = 9325; break; // &V -> &Vd
				case 9399: state = 9400; break; // &VerticalTil -> &VerticalTild
				case 9471: state = 9472; break; // &Vv -> &Vvd
				case 9496: state = 9497; break; // &we -> &wed
				case 9502: state = 9503; break; // &We -> &Wed
				case 9548: state = 9560; break; // &x -> &xd
				case 9602: state = 9603; break; // &xo -> &xod
				case 9660: state = 9661; break; // &xwe -> &xwed
				case 9747: state = 9777; break; // &Z -> &Zd
				case 9754: state = 9781; break; // &z -> &zd
				case 9795: state = 9796; break; // &ZeroWi -> &ZeroWid
				default: return false;
				}
				break;
			case 'e':
				switch (state) {
				case 0: state = 2115; break; // & -> &e
				case 5: state = 6; break; // &Aacut -> &Aacute
				case 8: state = 55; break; // &a -> &ae
				case 12: state = 13; break; // &aacut -> &aacute
				case 16: state = 17; break; // &Abr -> &Abre
				case 18: state = 19; break; // &Abrev -> &Abreve
				case 22: state = 23; break; // &abr -> &abre
				case 24: state = 25; break; // &abrev -> &abreve
				case 43: state = 44; break; // &acut -> &acute
				case 70: state = 71; break; // &Agrav -> &Agrave
				case 76: state = 77; break; // &agrav -> &agrave
				case 79: state = 80; break; // &al -> &ale
				case 131: state = 132; break; // &andslop -> &andslope
				case 136: state = 138; break; // &ang -> &ange
				case 140: state = 141; break; // &angl -> &angle
				case 147: state = 156; break; // &angmsda -> &angmsdae
				case 199: state = 208; break; // &ap -> &ape
				case 232: state = 234; break; // &approx -> &approxe
				case 264: state = 266; break; // &asymp -> &asympe
				case 272: state = 273; break; // &Atild -> &Atilde
				case 278: state = 279; break; // &atild -> &atilde
				case 301: state = 384; break; // &b -> &be
				case 304: state = 310; break; // &back -> &backe
				case 321: state = 322; break; // &backprim -> &backprime
				case 326: state = 328; break; // &backsim -> &backsime
				case 331: state = 390; break; // &B -> &Be
				case 345: state = 346; break; // &barv -> &barve
				case 346: state = 347; break; // &barve -> &barvee
				case 349: state = 350; break; // &Barw -> &Barwe
				case 353: state = 354; break; // &barw -> &barwe
				case 357: state = 358; break; // &barwedg -> &barwedge
				case 388: state = 397; break; // &becaus -> &because
				case 394: state = 395; break; // &Becaus -> &Because
				case 431: state = 432; break; // &betw -> &betwe
				case 432: state = 433; break; // &betwe -> &betwee
				case 467: state = 468; break; // &bigotim -> &bigotime
				case 487: state = 488; break; // &bigtriangl -> &bigtriangle
				case 503: state = 504; break; // &bigv -> &bigve
				case 504: state = 505; break; // &bigve -> &bigvee
				case 507: state = 508; break; // &bigw -> &bigwe
				case 510: state = 511; break; // &bigwedg -> &bigwedge
				case 525: state = 526; break; // &blackloz -> &blackloze
				case 528: state = 529; break; // &blacklozeng -> &blacklozenge
				case 535: state = 536; break; // &blacksquar -> &blacksquare
				case 544: state = 545; break; // &blacktriangl -> &blacktriangle
				case 552: state = 553; break; // &blacktrianglel -> &blacktrianglele
				case 579: state = 580; break; // &bn -> &bne
				case 610: state = 611; break; // &bowti -> &bowtie
				case 669: state = 670; break; // &boxtim -> &boxtime
				case 722: state = 723; break; // &bprim -> &bprime
				case 725: state = 726; break; // &Br -> &Bre
				case 727: state = 728; break; // &Brev -> &Breve
				case 730: state = 731; break; // &br -> &bre
				case 732: state = 733; break; // &brev -> &breve
				case 744: state = 748; break; // &bs -> &bse
				case 753: state = 755; break; // &bsim -> &bsime
				case 769: state = 771; break; // &bull -> &bulle
				case 775: state = 779; break; // &bump -> &bumpe
				case 783: state = 784; break; // &Bump -> &Bumpe
				case 789: state = 920; break; // &C -> &Ce
				case 793: state = 794; break; // &Cacut -> &Cacute
				case 796: state = 915; break; // &c -> &ce
				case 800: state = 801; break; // &cacut -> &cacute
				case 835: state = 836; break; // &CapitalDiff -> &CapitalDiffe
				case 837: state = 838; break; // &CapitalDiffer -> &CapitalDiffere
				case 848: state = 849; break; // &car -> &care
				case 856: state = 857; break; // &Cayl -> &Cayle
				case 861: state = 881; break; // &cc -> &cce
				case 866: state = 876; break; // &Cc -> &Cce
				case 934: state = 944; break; // &cent -> &cente
				case 937: state = 938; break; // &Cent -> &Cente
				case 960: state = 964; break; // &ch -> &che
				case 979: state = 1051; break; // &cir -> &cire
				case 981: state = 983; break; // &circ -> &circe
				case 986: state = 987; break; // &circl -> &circle
				case 993: state = 994; break; // &circlearrowl -> &circlearrowle
				case 1022: state = 1023; break; // &Circl -> &Circle
				case 1045: state = 1046; break; // &CircleTim -> &CircleTime
				case 1074: state = 1075; break; // &Clockwis -> &Clockwise
				case 1085: state = 1086; break; // &ClockwiseContourInt -> &ClockwiseContourInte
				case 1092: state = 1093; break; // &Clos -> &Close
				case 1103: state = 1104; break; // &CloseCurlyDoubl -> &CloseCurlyDouble
				case 1108: state = 1109; break; // &CloseCurlyDoubleQuot -> &CloseCurlyDoubleQuote
				case 1114: state = 1115; break; // &CloseCurlyQuot -> &CloseCurlyQuote
				case 1129: state = 1136; break; // &Colon -> &Colone
				case 1134: state = 1138; break; // &colon -> &colone
				case 1153: state = 1154; break; // &compl -> &comple
				case 1155: state = 1156; break; // &complem -> &compleme
				case 1160: state = 1161; break; // &complex -> &complexe
				case 1174: state = 1175; break; // &Congru -> &Congrue
				case 1193: state = 1194; break; // &ContourInt -> &ContourInte
				case 1228: state = 1229; break; // &Count -> &Counte
				case 1238: state = 1239; break; // &CounterClockwis -> &CounterClockwise
				case 1249: state = 1250; break; // &CounterClockwiseContourInt -> &CounterClockwiseContourInte
				case 1279: state = 1281; break; // &csub -> &csube
				case 1283: state = 1285; break; // &csup -> &csupe
				case 1292: state = 1301; break; // &cu -> &cue
				case 1354: state = 1355; break; // &curly -> &curlye
				case 1358: state = 1359; break; // &curlyeqpr -> &curlyeqpre
				case 1367: state = 1368; break; // &curlyv -> &curlyve
				case 1368: state = 1369; break; // &curlyve -> &curlyvee
				case 1371: state = 1372; break; // &curlyw -> &curlywe
				case 1374: state = 1375; break; // &curlywedg -> &curlywedge
				case 1377: state = 1378; break; // &curr -> &curre
				case 1381: state = 1382; break; // &curv -> &curve
				case 1388: state = 1389; break; // &curvearrowl -> &curvearrowle
				case 1399: state = 1400; break; // &cuv -> &cuve
				case 1400: state = 1401; break; // &cuve -> &cuvee
				case 1403: state = 1404; break; // &cuw -> &cuwe
				case 1425: state = 1519; break; // &D -> &De
				case 1428: state = 1429; break; // &Dagg -> &Dagge
				case 1432: state = 1516; break; // &d -> &de
				case 1435: state = 1436; break; // &dagg -> &dagge
				case 1439: state = 1440; break; // &dal -> &dale
				case 1496: state = 1497; break; // &ddagg -> &ddagge
				case 1512: state = 1513; break; // &ddots -> &ddotse
				case 1570: state = 1571; break; // &DiacriticalAcut -> &DiacriticalAcute
				case 1579: state = 1580; break; // &DiacriticalDoubl -> &DiacriticalDouble
				case 1584: state = 1585; break; // &DiacriticalDoubleAcut -> &DiacriticalDoubleAcute
				case 1590: state = 1591; break; // &DiacriticalGrav -> &DiacriticalGrave
				case 1596: state = 1597; break; // &DiacriticalTild -> &DiacriticalTilde
				case 1599: state = 1619; break; // &di -> &die
				case 1622: state = 1623; break; // &Diff -> &Diffe
				case 1624: state = 1625; break; // &Differ -> &Differe
				case 1646: state = 1647; break; // &divid -> &divide
				case 1653: state = 1654; break; // &divideontim -> &divideontime
				case 1694: state = 1700; break; // &dot -> &dote
				case 1728: state = 1729; break; // &dotsquar -> &dotsquare
				case 1733: state = 1734; break; // &doubl -> &double
				case 1738: state = 1739; break; // &doublebarw -> &doublebarwe
				case 1741: state = 1742; break; // &doublebarwedg -> &doublebarwedge
				case 1746: state = 1747; break; // &Doubl -> &Double
				case 1757: state = 1758; break; // &DoubleContourInt -> &DoubleContourInte
				case 1776: state = 1777; break; // &DoubleL -> &DoubleLe
				case 1797: state = 1798; break; // &DoubleLeftT -> &DoubleLeftTe
				case 1798: state = 1799; break; // &DoubleLeftTe -> &DoubleLeftTee
				case 1804: state = 1805; break; // &DoubleLongL -> &DoubleLongLe
				case 1847: state = 1848; break; // &DoubleRightT -> &DoubleRightTe
				case 1848: state = 1849; break; // &DoubleRightTe -> &DoubleRightTee
				case 1869: state = 1870; break; // &DoubleV -> &DoubleVe
				case 1916: state = 1917; break; // &DownBr -> &DownBre
				case 1918: state = 1919; break; // &DownBrev -> &DownBreve
				case 1939: state = 1940; break; // &downharpoonl -> &downharpoonle
				case 1950: state = 1951; break; // &DownL -> &DownLe
				case 1959: state = 1960; break; // &DownLeftRightV -> &DownLeftRightVe
				case 1966: state = 1967; break; // &DownLeftT -> &DownLeftTe
				case 1967: state = 1968; break; // &DownLeftTe -> &DownLeftTee
				case 1969: state = 1970; break; // &DownLeftTeeV -> &DownLeftTeeVe
				case 1976: state = 1977; break; // &DownLeftV -> &DownLeftVe
				case 1992: state = 1993; break; // &DownRightT -> &DownRightTe
				case 1993: state = 1994; break; // &DownRightTe -> &DownRightTee
				case 1995: state = 1996; break; // &DownRightTeeV -> &DownRightTeeVe
				case 2002: state = 2003; break; // &DownRightV -> &DownRightVe
				case 2013: state = 2014; break; // &DownT -> &DownTe
				case 2014: state = 2015; break; // &DownTe -> &DownTee
				case 2090: state = 2091; break; // &dwangl -> &dwangle
				case 2112: state = 2113; break; // &Eacut -> &Eacute
				case 2115: state = 2173; break; // &e -> &ee
				case 2119: state = 2120; break; // &eacut -> &eacute
				case 2123: state = 2124; break; // &east -> &easte
				case 2190: state = 2191; break; // &Egrav -> &Egrave
				case 2195: state = 2196; break; // &egrav -> &egrave
				case 2206: state = 2207; break; // &El -> &Ele
				case 2208: state = 2209; break; // &Elem -> &Eleme
				case 2215: state = 2216; break; // &elint -> &elinte
				case 2242: state = 2243; break; // &emptys -> &emptyse
				case 2258: state = 2259; break; // &EmptySmallSquar -> &EmptySmallSquare
				case 2263: state = 2264; break; // &EmptyV -> &EmptyVe
				case 2276: state = 2277; break; // &EmptyVerySmallSquar -> &EmptyVerySmallSquare
				case 2362: state = 2363; break; // &eqslantl -> &eqslantle
				case 2372: state = 2383; break; // &equ -> &eque
				case 2380: state = 2381; break; // &EqualTild -> &EqualTilde
				case 2472: state = 2473; break; // &exp -> &expe
				case 2484: state = 2485; break; // &Expon -> &Expone
				case 2494: state = 2495; break; // &expon -> &expone
				case 2500: state = 2501; break; // &exponential -> &exponentiale
				case 2503: state = 2524; break; // &f -> &fe
				case 2513: state = 2514; break; // &fallingdots -> &fallingdotse
				case 2527: state = 2528; break; // &femal -> &female
				case 2556: state = 2557; break; // &Fill -> &Fille
				case 2568: state = 2569; break; // &FilledSmallSquar -> &FilledSmallSquare
				case 2571: state = 2572; break; // &FilledV -> &FilledVe
				case 2584: state = 2585; break; // &FilledVerySmallSquar -> &FilledVerySmallSquare
				case 2632: state = 2633; break; // &Fouri -> &Fourie
				case 2701: state = 2765; break; // &g -> &ge
				case 2705: state = 2706; break; // &gacut -> &gacute
				case 2725: state = 2726; break; // &Gbr -> &Gbre
				case 2727: state = 2728; break; // &Gbrev -> &Gbreve
				case 2731: state = 2732; break; // &gbr -> &gbre
				case 2733: state = 2734; break; // &gbrev -> &gbreve
				case 2736: state = 2737; break; // &Gc -> &Gce
				case 2794: state = 2796; break; // &gesl -> &gesle
				case 2812: state = 2813; break; // &gim -> &gime
				case 2832: state = 2843; break; // &gn -> &gne
				case 2863: state = 2864; break; // &grav -> &grave
				case 2866: state = 2867; break; // &Gr -> &Gre
				case 2869: state = 2870; break; // &Great -> &Greate
				case 2878: state = 2879; break; // &GreaterEqualL -> &GreaterEqualLe
				case 2894: state = 2895; break; // &GreaterGr -> &GreaterGre
				case 2897: state = 2898; break; // &GreaterGreat -> &GreaterGreate
				case 2901: state = 2902; break; // &GreaterL -> &GreaterLe
				case 2920: state = 2921; break; // &GreaterTild -> &GreaterTilde
				case 2932: state = 2934; break; // &gsim -> &gsime
				case 2960: state = 2961; break; // &gtqu -> &gtque
				case 2965: state = 2980; break; // &gtr -> &gtre
				case 2982: state = 2983; break; // &gtreql -> &gtreqle
				case 2988: state = 2989; break; // &gtreqql -> &gtreqqle
				case 2993: state = 2994; break; // &gtrl -> &gtrle
				case 3002: state = 3003; break; // &gv -> &gve
				case 3006: state = 3007; break; // &gvertn -> &gvertne
				case 3016: state = 3017; break; // &Hac -> &Hace
				case 3020: state = 3074; break; // &h -> &he
				case 3102: state = 3103; break; // &Hilb -> &Hilbe
				case 3109: state = 3110; break; // &HilbertSpac -> &HilbertSpace
				case 3113: state = 3114; break; // &hks -> &hkse
				case 3138: state = 3139; break; // &hookl -> &hookle
				case 3181: state = 3182; break; // &HorizontalLin -> &HorizontalLine
				case 3232: state = 3233; break; // &hyph -> &hyphe
				case 3240: state = 3241; break; // &Iacut -> &Iacute
				case 3243: state = 3273; break; // &i -> &ie
				case 3247: state = 3248; break; // &iacut -> &iacute
				case 3292: state = 3293; break; // &Igrav -> &Igrave
				case 3298: state = 3299; break; // &igrav -> &igrave
				case 3341: state = 3342; break; // &imag -> &image
				case 3354: state = 3355; break; // &imaglin -> &imagline
				case 3368: state = 3369; break; // &imp -> &impe
				case 3374: state = 3375; break; // &Impli -> &Implie
				case 3382: state = 3383; break; // &incar -> &incare
				case 3390: state = 3391; break; // &infinti -> &infintie
				case 3399: state = 3413; break; // &Int -> &Inte
				case 3401: state = 3407; break; // &int -> &inte
				case 3408: state = 3409; break; // &integ -> &intege
				case 3425: state = 3426; break; // &Inters -> &Interse
				case 3449: state = 3450; break; // &Invisibl -> &Invisible
				case 3459: state = 3460; break; // &InvisibleTim -> &InvisibleTime
				case 3498: state = 3499; break; // &iqu -> &ique
				case 3531: state = 3532; break; // &Itild -> &Itilde
				case 3536: state = 3537; break; // &itild -> &itilde
				case 3590: state = 3598; break; // &Js -> &Jse
				case 3594: state = 3603; break; // &js -> &jse
				case 3632: state = 3633; break; // &Kc -> &Kce
				case 3638: state = 3639; break; // &kc -> &kce
				case 3655: state = 3656; break; // &kgr -> &kgre
				case 3656: state = 3657; break; // &kgre -> &kgree
				case 3692: state = 3896; break; // &l -> &le
				case 3698: state = 3898; break; // &L -> &Le
				case 3702: state = 3703; break; // &Lacut -> &Lacute
				case 3705: state = 3711; break; // &la -> &lae
				case 3708: state = 3709; break; // &lacut -> &lacute
				case 3741: state = 3742; break; // &langl -> &langle
				case 3749: state = 3750; break; // &Laplac -> &Laplace
				case 3792: state = 3803; break; // &lat -> &late
				case 3823: state = 3824; break; // &lbrac -> &lbrace
				case 3828: state = 3829; break; // &lbrk -> &lbrke
				case 3837: state = 3849; break; // &Lc -> &Lce
				case 3843: state = 3854; break; // &lc -> &lce
				case 3904: state = 3905; break; // &LeftAngl -> &LeftAngle
				case 3910: state = 3911; break; // &LeftAngleBrack -> &LeftAngleBracke
				case 3953: state = 3954; break; // &LeftC -> &LeftCe
				case 3965: state = 3966; break; // &LeftDoubl -> &LeftDouble
				case 3971: state = 3972; break; // &LeftDoubleBrack -> &LeftDoubleBracke
				case 3977: state = 3978; break; // &LeftDownT -> &LeftDownTe
				case 3978: state = 3979; break; // &LeftDownTe -> &LeftDownTee
				case 3980: state = 3981; break; // &LeftDownTeeV -> &LeftDownTeeVe
				case 3987: state = 3988; break; // &LeftDownV -> &LeftDownVe
				case 4019: state = 4020; break; // &leftl -> &leftle
				case 4085: state = 4086; break; // &LeftRightV -> &LeftRightVe
				case 4092: state = 4093; break; // &LeftT -> &LeftTe
				case 4093: state = 4094; break; // &LeftTe -> &LeftTee
				case 4102: state = 4103; break; // &LeftTeeV -> &LeftTeeVe
				case 4111: state = 4112; break; // &leftthr -> &leftthre
				case 4112: state = 4113; break; // &leftthre -> &leftthree
				case 4116: state = 4117; break; // &leftthreetim -> &leftthreetime
				case 4125: state = 4126; break; // &LeftTriangl -> &LeftTriangle
				case 4144: state = 4145; break; // &LeftUpDownV -> &LeftUpDownVe
				case 4151: state = 4152; break; // &LeftUpT -> &LeftUpTe
				case 4152: state = 4153; break; // &LeftUpTe -> &LeftUpTee
				case 4154: state = 4155; break; // &LeftUpTeeV -> &LeftUpTeeVe
				case 4161: state = 4162; break; // &LeftUpV -> &LeftUpVe
				case 4172: state = 4173; break; // &LeftV -> &LeftVe
				case 4210: state = 4212; break; // &lesg -> &lesge
				case 4215: state = 4227; break; // &less -> &lesse
				case 4246: state = 4247; break; // &LessEqualGr -> &LessEqualGre
				case 4249: state = 4250; break; // &LessEqualGreat -> &LessEqualGreate
				case 4264: state = 4265; break; // &LessGr -> &LessGre
				case 4267: state = 4268; break; // &LessGreat -> &LessGreate
				case 4275: state = 4276; break; // &LessL -> &LessLe
				case 4298: state = 4299; break; // &LessTild -> &LessTilde
				case 4346: state = 4361; break; // &Ll -> &Lle
				case 4357: state = 4358; break; // &llcorn -> &llcorne
				case 4398: state = 4399; break; // &lmoustach -> &lmoustache
				case 4401: state = 4412; break; // &ln -> &lne
				case 4437: state = 4438; break; // &LongL -> &LongLe
				case 4447: state = 4448; break; // &Longl -> &Longle
				case 4459: state = 4460; break; // &longl -> &longle
				case 4549: state = 4550; break; // &looparrowl -> &looparrowle
				case 4575: state = 4576; break; // &lotim -> &lotime
				case 4588: state = 4589; break; // &Low -> &Lowe
				case 4591: state = 4592; break; // &LowerL -> &LowerLe
				case 4612: state = 4614; break; // &loz -> &loze
				case 4616: state = 4617; break; // &lozeng -> &lozenge
				case 4636: state = 4637; break; // &lrcorn -> &lrcorne
				case 4670: state = 4672; break; // &lsim -> &lsime
				case 4711: state = 4712; break; // &lthr -> &lthre
				case 4712: state = 4713; break; // &lthre -> &lthree
				case 4716: state = 4717; break; // &ltim -> &ltime
				case 4726: state = 4727; break; // &ltqu -> &ltque
				case 4732: state = 4734; break; // &ltri -> &ltrie
				case 4755: state = 4756; break; // &lv -> &lve
				case 4759: state = 4760; break; // &lvertn -> &lvertne
				case 4767: state = 4830; break; // &m -> &me
				case 4772: state = 4773; break; // &mal -> &male
				case 4775: state = 4777; break; // &malt -> &malte
				case 4778: state = 4779; break; // &maltes -> &maltese
				case 4781: state = 4843; break; // &M -> &Me
				case 4796: state = 4797; break; // &mapstol -> &mapstole
				case 4805: state = 4806; break; // &mark -> &marke
				case 4834: state = 4835; break; // &measur -> &measure
				case 4840: state = 4841; break; // &measuredangl -> &measuredangle
				case 4851: state = 4852; break; // &MediumSpac -> &MediumSpace
				case 4923: state = 4924; break; // &mod -> &mode
				case 4965: state = 5064; break; // &n -> &ne
				case 4971: state = 5084; break; // &N -> &Ne
				case 4975: state = 4976; break; // &Nacut -> &Nacute
				case 4980: state = 4981; break; // &nacut -> &nacute
				case 5016: state = 5018; break; // &nbump -> &nbumpe
				case 5020: state = 5039; break; // &nc -> &nce
				case 5024: state = 5034; break; // &Nc -> &Nce
				case 5089: state = 5090; break; // &Negativ -> &Negative
				case 5091: state = 5092; break; // &NegativeM -> &NegativeMe
				case 5100: state = 5101; break; // &NegativeMediumSpac -> &NegativeMediumSpace
				case 5111: state = 5112; break; // &NegativeThickSpac -> &NegativeThickSpace
				case 5118: state = 5119; break; // &NegativeThinSpac -> &NegativeThinSpace
				case 5121: state = 5122; break; // &NegativeV -> &NegativeVe
				case 5132: state = 5133; break; // &NegativeVeryThinSpac -> &NegativeVeryThinSpace
				case 5140: state = 5141; break; // &nes -> &nese
				case 5149: state = 5150; break; // &Nest -> &Neste
				case 5153: state = 5154; break; // &NestedGr -> &NestedGre
				case 5156: state = 5157; break; // &NestedGreat -> &NestedGreate
				case 5160: state = 5161; break; // &NestedGreaterGr -> &NestedGreaterGre
				case 5163: state = 5164; break; // &NestedGreaterGreat -> &NestedGreaterGreate
				case 5167: state = 5168; break; // &NestedL -> &NestedLe
				case 5171: state = 5172; break; // &NestedLessL -> &NestedLessLe
				case 5179: state = 5180; break; // &NewLin -> &NewLine
				case 5195: state = 5198; break; // &ng -> &nge
				case 5256: state = 5270; break; // &nl -> &nle
				case 5272: state = 5273; break; // &nL -> &nLe
				case 5337: state = 5339; break; // &nltri -> &nltrie
				case 5349: state = 5350; break; // &NoBr -> &NoBre
				case 5356: state = 5357; break; // &NonBr -> &NonBre
				case 5366: state = 5367; break; // &NonBreakingSpac -> &NonBreakingSpace
				case 5385: state = 5386; break; // &NotCongru -> &NotCongrue
				case 5400: state = 5401; break; // &NotDoubl -> &NotDouble
				case 5402: state = 5403; break; // &NotDoubleV -> &NotDoubleVe
				case 5415: state = 5416; break; // &NotEl -> &NotEle
				case 5417: state = 5418; break; // &NotElem -> &NotEleme
				case 5430: state = 5431; break; // &NotEqualTild -> &NotEqualTilde
				case 5440: state = 5441; break; // &NotGr -> &NotGre
				case 5443: state = 5444; break; // &NotGreat -> &NotGreate
				case 5464: state = 5465; break; // &NotGreaterGr -> &NotGreaterGre
				case 5467: state = 5468; break; // &NotGreaterGreat -> &NotGreaterGreate
				case 5471: state = 5472; break; // &NotGreaterL -> &NotGreaterLe
				case 5490: state = 5491; break; // &NotGreaterTild -> &NotGreaterTilde
				case 5528: state = 5529; break; // &NotL -> &NotLe
				case 5538: state = 5539; break; // &NotLeftTriangl -> &NotLeftTriangle
				case 5561: state = 5562; break; // &NotLessGr -> &NotLessGre
				case 5564: state = 5565; break; // &NotLessGreat -> &NotLessGreate
				case 5568: state = 5569; break; // &NotLessL -> &NotLessLe
				case 5587: state = 5588; break; // &NotLessTild -> &NotLessTilde
				case 5590: state = 5591; break; // &NotN -> &NotNe
				case 5593: state = 5594; break; // &NotNest -> &NotNeste
				case 5597: state = 5598; break; // &NotNestedGr -> &NotNestedGre
				case 5600: state = 5601; break; // &NotNestedGreat -> &NotNestedGreate
				case 5604: state = 5605; break; // &NotNestedGreaterGr -> &NotNestedGreaterGre
				case 5607: state = 5608; break; // &NotNestedGreaterGreat -> &NotNestedGreaterGreate
				case 5611: state = 5612; break; // &NotNestedL -> &NotNestedLe
				case 5615: state = 5616; break; // &NotNestedLessL -> &NotNestedLessLe
				case 5631: state = 5632; break; // &NotPr -> &NotPre
				case 5633: state = 5634; break; // &NotPrec -> &NotPrece
				case 5635: state = 5636; break; // &NotPreced -> &NotPrecede
				case 5656: state = 5657; break; // &NotR -> &NotRe
				case 5658: state = 5659; break; // &NotRev -> &NotReve
				case 5661: state = 5662; break; // &NotRevers -> &NotReverse
				case 5664: state = 5665; break; // &NotReverseEl -> &NotReverseEle
				case 5666: state = 5667; break; // &NotReverseElem -> &NotReverseEleme
				case 5681: state = 5682; break; // &NotRightTriangl -> &NotRightTriangle
				case 5698: state = 5699; break; // &NotSquar -> &NotSquare
				case 5703: state = 5704; break; // &NotSquareSubs -> &NotSquareSubse
				case 5713: state = 5714; break; // &NotSquareSup -> &NotSquareSupe
				case 5716: state = 5717; break; // &NotSquareSupers -> &NotSquareSuperse
				case 5728: state = 5729; break; // &NotSubs -> &NotSubse
				case 5739: state = 5740; break; // &NotSucc -> &NotSucce
				case 5740: state = 5741; break; // &NotSucce -> &NotSuccee
				case 5765: state = 5766; break; // &NotSucceedsTild -> &NotSucceedsTilde
				case 5768: state = 5769; break; // &NotSup -> &NotSupe
				case 5771: state = 5772; break; // &NotSupers -> &NotSuperse
				case 5784: state = 5785; break; // &NotTild -> &NotTilde
				case 5806: state = 5807; break; // &NotTildeTild -> &NotTildeTilde
				case 5809: state = 5810; break; // &NotV -> &NotVe
				case 5827: state = 5828; break; // &nparall -> &nparalle
				case 5842: state = 5848; break; // &npr -> &npre
				case 5845: state = 5846; break; // &nprcu -> &nprcue
				case 5850: state = 5852; break; // &nprec -> &nprece
				case 5891: state = 5893; break; // &nrtri -> &nrtrie
				case 5896: state = 5902; break; // &nsc -> &nsce
				case 5899: state = 5900; break; // &nsccu -> &nsccue
				case 5923: state = 5924; break; // &nshortparall -> &nshortparalle
				case 5928: state = 5930; break; // &nsim -> &nsime
				case 5945: state = 5946; break; // &nsqsub -> &nsqsube
				case 5948: state = 5949; break; // &nsqsup -> &nsqsupe
				case 5952: state = 5956; break; // &nsub -> &nsube
				case 5958: state = 5959; break; // &nsubs -> &nsubse
				case 5960: state = 5962; break; // &nsubset -> &nsubsete
				case 5968: state = 5970; break; // &nsucc -> &nsucce
				case 5973: state = 5977; break; // &nsup -> &nsupe
				case 5979: state = 5980; break; // &nsups -> &nsupse
				case 5981: state = 5983; break; // &nsupset -> &nsupsete
				case 5995: state = 5996; break; // &Ntild -> &Ntilde
				case 6000: state = 6001; break; // &ntild -> &ntilde
				case 6011: state = 6012; break; // &ntriangl -> &ntriangle
				case 6013: state = 6014; break; // &ntrianglel -> &ntrianglele
				case 6016: state = 6018; break; // &ntriangleleft -> &ntrianglelefte
				case 6025: state = 6027; break; // &ntriangleright -> &ntrianglerighte
				case 6034: state = 6036; break; // &num -> &nume
				case 6068: state = 6069; break; // &nvg -> &nvge
				case 6084: state = 6089; break; // &nvl -> &nvle
				case 6094: state = 6095; break; // &nvltri -> &nvltrie
				case 6104: state = 6105; break; // &nvrtri -> &nvrtrie
				case 6126: state = 6127; break; // &nwn -> &nwne
				case 6135: state = 6136; break; // &Oacut -> &Oacute
				case 6138: state = 6195; break; // &o -> &oe
				case 6142: state = 6143; break; // &oacut -> &oacute
				case 6217: state = 6218; break; // &Ograv -> &Ograve
				case 6222: state = 6223; break; // &ograv -> &ograve
				case 6253: state = 6254; break; // &olin -> &oline
				case 6258: state = 6268; break; // &Om -> &Ome
				case 6263: state = 6272; break; // &om -> &ome
				case 6302: state = 6332; break; // &op -> &ope
				case 6306: state = 6307; break; // &Op -> &Ope
				case 6318: state = 6319; break; // &OpenCurlyDoubl -> &OpenCurlyDouble
				case 6323: state = 6324; break; // &OpenCurlyDoubleQuot -> &OpenCurlyDoubleQuote
				case 6329: state = 6330; break; // &OpenCurlyQuot -> &OpenCurlyQuote
				case 6348: state = 6350; break; // &ord -> &orde
				case 6371: state = 6372; break; // &orslop -> &orslope
				case 6402: state = 6403; break; // &Otild -> &Otilde
				case 6408: state = 6409; break; // &otild -> &otilde
				case 6411: state = 6412; break; // &Otim -> &Otime
				case 6415: state = 6416; break; // &otim -> &otime
				case 6435: state = 6436; break; // &Ov -> &Ove
				case 6444: state = 6445; break; // &OverBrac -> &OverBrace
				case 6447: state = 6448; break; // &OverBrack -> &OverBracke
				case 6453: state = 6454; break; // &OverPar -> &OverPare
				case 6457: state = 6458; break; // &OverParenth -> &OverParenthe
				case 6463: state = 6497; break; // &p -> &pe
				case 6470: state = 6471; break; // &parall -> &paralle
				case 6513: state = 6514; break; // &pert -> &perte
				case 6538: state = 6539; break; // &phon -> &phone
				case 6567: state = 6585; break; // &plus -> &pluse
				case 6614: state = 6615; break; // &Poincar -> &Poincare
				case 6619: state = 6620; break; // &Poincareplan -> &Poincareplane
				case 6640: state = 6672; break; // &Pr -> &Pre
				case 6642: state = 6653; break; // &pr -> &pre
				case 6648: state = 6649; break; // &prcu -> &prcue
				case 6655: state = 6702; break; // &prec -> &prece
				case 6668: state = 6669; break; // &preccurly -> &preccurlye
				case 6673: state = 6674; break; // &Prec -> &Prece
				case 6675: state = 6676; break; // &Preced -> &Precede
				case 6699: state = 6700; break; // &PrecedesTild -> &PrecedesTilde
				case 6705: state = 6713; break; // &precn -> &precne
				case 6726: state = 6727; break; // &Prim -> &Prime
				case 6730: state = 6731; break; // &prim -> &prime
				case 6762: state = 6763; break; // &proflin -> &profline
				case 6791: state = 6792; break; // &prur -> &prure
				case 6836: state = 6837; break; // &qprim -> &qprime
				case 6847: state = 6862; break; // &qu -> &que
				case 6849: state = 6850; break; // &quat -> &quate
				case 6864: state = 6866; break; // &quest -> &queste
				case 6876: state = 7074; break; // &r -> &re
				case 6882: state = 6901; break; // &ra -> &rae
				case 6883: state = 6884; break; // &rac -> &race
				case 6886: state = 7072; break; // &R -> &Re
				case 6890: state = 6891; break; // &Racut -> &Racute
				case 6894: state = 6895; break; // &racut -> &racute
				case 6912: state = 6916; break; // &rang -> &range
				case 6918: state = 6919; break; // &rangl -> &rangle
				case 7007: state = 7008; break; // &rbrac -> &rbrace
				case 7012: state = 7013; break; // &rbrk -> &rbrke
				case 7021: state = 7033; break; // &Rc -> &Rce
				case 7027: state = 7038; break; // &rc -> &rce
				case 7079: state = 7080; break; // &realin -> &realine
				case 7097: state = 7098; break; // &Rev -> &Reve
				case 7100: state = 7101; break; // &Revers -> &Reverse
				case 7103: state = 7104; break; // &ReverseEl -> &ReverseEle
				case 7105: state = 7106; break; // &ReverseElem -> &ReverseEleme
				case 7178: state = 7179; break; // &RightAngl -> &RightAngle
				case 7184: state = 7185; break; // &RightAngleBrack -> &RightAngleBracke
				case 7213: state = 7214; break; // &RightArrowL -> &RightArrowLe
				case 7228: state = 7229; break; // &RightC -> &RightCe
				case 7240: state = 7241; break; // &RightDoubl -> &RightDouble
				case 7246: state = 7247; break; // &RightDoubleBrack -> &RightDoubleBracke
				case 7252: state = 7253; break; // &RightDownT -> &RightDownTe
				case 7253: state = 7254; break; // &RightDownTe -> &RightDownTee
				case 7255: state = 7256; break; // &RightDownTeeV -> &RightDownTeeVe
				case 7262: state = 7263; break; // &RightDownV -> &RightDownVe
				case 7294: state = 7295; break; // &rightl -> &rightle
				case 7337: state = 7338; break; // &RightT -> &RightTe
				case 7338: state = 7339; break; // &RightTe -> &RightTee
				case 7347: state = 7348; break; // &RightTeeV -> &RightTeeVe
				case 7356: state = 7357; break; // &rightthr -> &rightthre
				case 7357: state = 7358; break; // &rightthre -> &rightthree
				case 7361: state = 7362; break; // &rightthreetim -> &rightthreetime
				case 7370: state = 7371; break; // &RightTriangl -> &RightTriangle
				case 7389: state = 7390; break; // &RightUpDownV -> &RightUpDownVe
				case 7396: state = 7397; break; // &RightUpT -> &RightUpTe
				case 7397: state = 7398; break; // &RightUpTe -> &RightUpTee
				case 7399: state = 7400; break; // &RightUpTeeV -> &RightUpTeeVe
				case 7406: state = 7407; break; // &RightUpV -> &RightUpVe
				case 7417: state = 7418; break; // &RightV -> &RightVe
				case 7438: state = 7439; break; // &risingdots -> &risingdotse
				case 7461: state = 7462; break; // &rmoustach -> &rmoustache
				case 7497: state = 7498; break; // &rotim -> &rotime
				case 7508: state = 7509; break; // &RoundImpli -> &RoundImplie
				case 7569: state = 7570; break; // &rthr -> &rthre
				case 7570: state = 7571; break; // &rthre -> &rthree
				case 7574: state = 7575; break; // &rtim -> &rtime
				case 7579: state = 7581; break; // &rtri -> &rtrie
				case 7591: state = 7592; break; // &Rul -> &Rule
				case 7593: state = 7594; break; // &RuleD -> &RuleDe
				case 7597: state = 7598; break; // &RuleDelay -> &RuleDelaye
				case 7614: state = 7615; break; // &Sacut -> &Sacute
				case 7617: state = 7703; break; // &s -> &se
				case 7621: state = 7622; break; // &sacut -> &sacute
				case 7629: state = 7653; break; // &Sc -> &Sce
				case 7631: state = 7651; break; // &sc -> &sce
				case 7646: state = 7647; break; // &sccu -> &sccue
				case 7697: state = 7701; break; // &sdot -> &sdote
				case 7786: state = 7787; break; // &ShortL -> &ShortLe
				case 7808: state = 7809; break; // &shortparall -> &shortparalle
				case 7847: state = 7853; break; // &sim -> &sime
				case 7865: state = 7866; break; // &simn -> &simne
				case 7891: state = 7892; break; // &SmallCircl -> &SmallCircle
				case 7894: state = 7911; break; // &sm -> &sme
				case 7898: state = 7899; break; // &smalls -> &smallse
				case 7921: state = 7922; break; // &smil -> &smile
				case 7924: state = 7926; break; // &smt -> &smte
				case 7958: state = 7959; break; // &spad -> &spade
				case 7986: state = 7988; break; // &sqsub -> &sqsube
				case 7990: state = 7991; break; // &sqsubs -> &sqsubse
				case 7992: state = 7994; break; // &sqsubset -> &sqsubsete
				case 7997: state = 7999; break; // &sqsup -> &sqsupe
				case 8001: state = 8002; break; // &sqsups -> &sqsupse
				case 8003: state = 8005; break; // &sqsupset -> &sqsupsete
				case 8012: state = 8013; break; // &Squar -> &Square
				case 8016: state = 8017; break; // &squar -> &square
				case 8021: state = 8022; break; // &SquareInt -> &SquareInte
				case 8024: state = 8025; break; // &SquareInters -> &SquareInterse
				case 8035: state = 8036; break; // &SquareSubs -> &SquareSubse
				case 8045: state = 8046; break; // &SquareSup -> &SquareSupe
				case 8048: state = 8049; break; // &SquareSupers -> &SquareSuperse
				case 8077: state = 8081; break; // &ss -> &sse
				case 8088: state = 8089; break; // &ssmil -> &ssmile
				case 8111: state = 8112; break; // &straight -> &straighte
				case 8131: state = 8139; break; // &sub -> &sube
				case 8150: state = 8153; break; // &subn -> &subne
				case 8165: state = 8166; break; // &Subs -> &Subse
				case 8169: state = 8170; break; // &subs -> &subse
				case 8171: state = 8173; break; // &subset -> &subsete
				case 8184: state = 8185; break; // &subsetn -> &subsetne
				case 8199: state = 8246; break; // &succ -> &succe
				case 8212: state = 8213; break; // &succcurly -> &succcurlye
				case 8217: state = 8218; break; // &Succ -> &Succe
				case 8218: state = 8219; break; // &Succe -> &Succee
				case 8243: state = 8244; break; // &SucceedsTild -> &SucceedsTilde
				case 8249: state = 8257; break; // &succn -> &succne
				case 8282: state = 8308; break; // &Sup -> &Supe
				case 8284: state = 8302; break; // &sup -> &supe
				case 8310: state = 8311; break; // &Supers -> &Superse
				case 8338: state = 8341; break; // &supn -> &supne
				case 8348: state = 8349; break; // &Sups -> &Supse
				case 8352: state = 8353; break; // &sups -> &supse
				case 8354: state = 8356; break; // &supset -> &supsete
				case 8361: state = 8362; break; // &supsetn -> &supsetne
				case 8404: state = 8449; break; // &t -> &te
				case 8407: state = 8408; break; // &targ -> &targe
				case 8419: state = 8431; break; // &Tc -> &Tce
				case 8425: state = 8436; break; // &tc -> &tce
				case 8451: state = 8452; break; // &telr -> &telre
				case 8461: state = 8462; break; // &th -> &the
				case 8463: state = 8464; break; // &ther -> &there
				case 8467: state = 8468; break; // &Th -> &The
				case 8469: state = 8470; break; // &Ther -> &There
				case 8473: state = 8474; break; // &Therefor -> &Therefore
				case 8478: state = 8479; break; // &therefor -> &therefore
				case 8513: state = 8514; break; // &ThickSpac -> &ThickSpace
				case 8524: state = 8525; break; // &ThinSpac -> &ThinSpace
				case 8546: state = 8547; break; // &Tild -> &Tilde
				case 8551: state = 8552; break; // &tild -> &tilde
				case 8573: state = 8574; break; // &TildeTild -> &TildeTilde
				case 8576: state = 8577; break; // &tim -> &time
				case 8590: state = 8591; break; // &to -> &toe
				case 8620: state = 8621; break; // &tprim -> &tprime
				case 8630: state = 8631; break; // &trad -> &trade
				case 8633: state = 8668; break; // &tri -> &trie
				case 8637: state = 8638; break; // &triangl -> &triangle
				case 8645: state = 8646; break; // &trianglel -> &trianglele
				case 8648: state = 8650; break; // &triangleleft -> &trianglelefte
				case 8659: state = 8661; break; // &triangleright -> &trianglerighte
				case 8679: state = 8680; break; // &Tripl -> &Triple
				case 8695: state = 8696; break; // &tritim -> &tritime
				case 8698: state = 8699; break; // &trp -> &trpe
				case 8743: state = 8744; break; // &twoh -> &twohe
				case 8747: state = 8748; break; // &twoheadl -> &twoheadle
				case 8772: state = 8773; break; // &Uacut -> &Uacute
				case 8779: state = 8780; break; // &uacut -> &uacute
				case 8798: state = 8807; break; // &Ubr -> &Ubre
				case 8803: state = 8811; break; // &ubr -> &ubre
				case 8808: state = 8809; break; // &Ubrev -> &Ubreve
				case 8812: state = 8813; break; // &ubrev -> &ubreve
				case 8863: state = 8864; break; // &Ugrav -> &Ugrave
				case 8869: state = 8870; break; // &ugrav -> &ugrave
				case 8891: state = 8893; break; // &ulcorn -> &ulcorne
				case 8917: state = 8918; break; // &Und -> &Unde
				case 8926: state = 8927; break; // &UnderBrac -> &UnderBrace
				case 8929: state = 8930; break; // &UnderBrack -> &UnderBracke
				case 8935: state = 8936; break; // &UnderPar -> &UnderPare
				case 8939: state = 8940; break; // &UnderParenth -> &UnderParenthe
				case 9053: state = 9054; break; // &upharpoonl -> &upharpoonle
				case 9068: state = 9069; break; // &Upp -> &Uppe
				case 9071: state = 9072; break; // &UpperL -> &UpperLe
				case 9108: state = 9109; break; // &UpT -> &UpTe
				case 9109: state = 9110; break; // &UpTe -> &UpTee
				case 9131: state = 9133; break; // &urcorn -> &urcorne
				case 9169: state = 9170; break; // &Utild -> &Utilde
				case 9174: state = 9175; break; // &utild -> &utilde
				case 9198: state = 9199; break; // &uwangl -> &uwangle
				case 9201: state = 9345; break; // &v -> &ve
				case 9208: state = 9209; break; // &var -> &vare
				case 9260: state = 9261; break; // &varsubs -> &varsubse
				case 9263: state = 9264; break; // &varsubsetn -> &varsubsetne
				case 9270: state = 9271; break; // &varsups -> &varsupse
				case 9273: state = 9274; break; // &varsupsetn -> &varsupsetne
				case 9280: state = 9281; break; // &varth -> &varthe
				case 9290: state = 9291; break; // &vartriangl -> &vartriangle
				case 9292: state = 9293; break; // &vartrianglel -> &vartrianglele
				case 9303: state = 9342; break; // &V -> &Ve
				case 9342: state = 9343; break; // &Ve -> &Vee
				case 9345: state = 9346; break; // &ve -> &vee
				case 9346: state = 9352; break; // &vee -> &veee
				case 9384: state = 9385; break; // &VerticalLin -> &VerticalLine
				case 9387: state = 9388; break; // &VerticalS -> &VerticalSe
				case 9400: state = 9401; break; // &VerticalTild -> &VerticalTilde
				case 9411: state = 9412; break; // &VeryThinSpac -> &VeryThinSpace
				case 9460: state = 9463; break; // &vsubn -> &vsubne
				case 9466: state = 9469; break; // &vsupn -> &vsupne
				case 9484: state = 9502; break; // &W -> &We
				case 9490: state = 9496; break; // &w -> &we
				case 9504: state = 9505; break; // &Wedg -> &Wedge
				case 9507: state = 9508; break; // &wedg -> &wedge
				case 9512: state = 9513; break; // &wei -> &weie
				case 9533: state = 9535; break; // &wr -> &wre
				case 9620: state = 9621; break; // &xotim -> &xotime
				case 9655: state = 9656; break; // &xv -> &xve
				case 9656: state = 9657; break; // &xve -> &xvee
				case 9659: state = 9660; break; // &xw -> &xwe
				case 9662: state = 9663; break; // &xwedg -> &xwedge
				case 9669: state = 9670; break; // &Yacut -> &Yacute
				case 9672: state = 9699; break; // &y -> &ye
				case 9676: state = 9677; break; // &yacut -> &yacute
				case 9747: state = 9791; break; // &Z -> &Ze
				case 9751: state = 9752; break; // &Zacut -> &Zacute
				case 9754: state = 9785; break; // &z -> &ze
				case 9758: state = 9759; break; // &zacut -> &zacute
				case 9785: state = 9786; break; // &ze -> &zee
				case 9802: state = 9803; break; // &ZeroWidthSpac -> &ZeroWidthSpace
				default: return false;
				}
				break;
			case 'f':
				switch (state) {
				case 0: state = 2503; break; // & -> &f
				case 1: state = 62; break; // &A -> &Af
				case 8: state = 60; break; // &a -> &af
				case 80: state = 81; break; // &ale -> &alef
				case 147: state = 158; break; // &angmsda -> &angmsdaf
				case 193: state = 194; break; // &Aop -> &Aopf
				case 196: state = 197; break; // &aop -> &aopf
				case 301: state = 439; break; // &b -> &bf
				case 331: state = 436; break; // &B -> &Bf
				case 553: state = 554; break; // &blacktrianglele -> &blacktrianglelef
				case 595: state = 596; break; // &Bop -> &Bopf
				case 599: state = 600; break; // &bop -> &bopf
				case 789: state = 950; break; // &C -> &Cf
				case 796: state = 953; break; // &c -> &cf
				case 833: state = 834; break; // &CapitalDi -> &CapitalDif
				case 834: state = 835; break; // &CapitalDif -> &CapitalDiff
				case 979: state = 1053; break; // &cir -> &cirf
				case 994: state = 995; break; // &circlearrowle -> &circlearrowlef
				case 1148: state = 1150; break; // &comp -> &compf
				case 1200: state = 1201; break; // &Cop -> &Copf
				case 1203: state = 1204; break; // &cop -> &copf
				case 1389: state = 1390; break; // &curvearrowle -> &curvearrowlef
				case 1425: state = 1541; break; // &D -> &Df
				case 1432: state = 1535; break; // &d -> &df
				case 1557: state = 1621; break; // &Di -> &Dif
				case 1621: state = 1622; break; // &Dif -> &Diff
				case 1686: state = 1687; break; // &Dop -> &Dopf
				case 1689: state = 1690; break; // &dop -> &dopf
				case 1777: state = 1778; break; // &DoubleLe -> &DoubleLef
				case 1805: state = 1806; break; // &DoubleLongLe -> &DoubleLongLef
				case 1940: state = 1941; break; // &downharpoonle -> &downharpoonlef
				case 1951: state = 1952; break; // &DownLe -> &DownLef
				case 2073: state = 2075; break; // &dtri -> &dtrif
				case 2108: state = 2180; break; // &E -> &Ef
				case 2115: state = 2175; break; // &e -> &ef
				case 2306: state = 2307; break; // &Eop -> &Eopf
				case 2309: state = 2310; break; // &eop -> &eopf
				case 2503: state = 2530; break; // &f -> &ff
				case 2517: state = 2544; break; // &F -> &Ff
				case 2605: state = 2606; break; // &fno -> &fnof
				case 2609: state = 2610; break; // &Fop -> &Fopf
				case 2613: state = 2614; break; // &fop -> &fopf
				case 2636: state = 2637; break; // &Fouriertr -> &Fouriertrf
				case 2701: state = 2802; break; // &g -> &gf
				case 2708: state = 2799; break; // &G -> &Gf
				case 2854: state = 2855; break; // &Gop -> &Gopf
				case 2858: state = 2859; break; // &gop -> &gopf
				case 3014: state = 3094; break; // &H -> &Hf
				case 3020: state = 3097; break; // &h -> &hf
				case 3027: state = 3028; break; // &hal -> &half
				case 3139: state = 3140; break; // &hookle -> &hooklef
				case 3160: state = 3161; break; // &Hop -> &Hopf
				case 3163: state = 3164; break; // &hop -> &hopf
				case 3236: state = 3284; break; // &I -> &If
				case 3243: state = 3281; break; // &i -> &if
				case 3281: state = 3282; break; // &if -> &iff
				case 3311: state = 3312; break; // &iin -> &iinf
				case 3365: state = 3366; break; // &imo -> &imof
				case 3378: state = 3385; break; // &in -> &inf
				case 3480: state = 3481; break; // &Iop -> &Iopf
				case 3483: state = 3484; break; // &iop -> &iopf
				case 3555: state = 3571; break; // &J -> &Jf
				case 3561: state = 3574; break; // &j -> &jf
				case 3583: state = 3584; break; // &Jop -> &Jopf
				case 3587: state = 3588; break; // &jop -> &jopf
				case 3618: state = 3648; break; // &K -> &Kf
				case 3624: state = 3651; break; // &k -> &kf
				case 3677: state = 3678; break; // &Kop -> &Kopf
				case 3681: state = 3682; break; // &kop -> &kopf
				case 3692: state = 4301; break; // &l -> &lf
				case 3698: state = 4312; break; // &L -> &Lf
				case 3752: state = 3753; break; // &Laplacetr -> &Laplacetrf
				case 3766: state = 3773; break; // &larr -> &larrf
				case 3768: state = 3770; break; // &larrb -> &larrbf
				case 3896: state = 3925; break; // &le -> &lef
				case 3898: state = 3899; break; // &Le -> &Lef
				case 4020: state = 4021; break; // &leftle -> &leftlef
				case 4361: state = 4362; break; // &Lle -> &Llef
				case 4438: state = 4439; break; // &LongLe -> &LongLef
				case 4448: state = 4449; break; // &Longle -> &Longlef
				case 4460: state = 4461; break; // &longle -> &longlef
				case 4550: state = 4551; break; // &looparrowle -> &looparrowlef
				case 4560: state = 4567; break; // &lop -> &lopf
				case 4564: state = 4565; break; // &Lop -> &Lopf
				case 4592: state = 4593; break; // &LowerLe -> &LowerLef
				case 4612: state = 4619; break; // &loz -> &lozf
				case 4732: state = 4736; break; // &ltri -> &ltrif
				case 4767: state = 4865; break; // &m -> &mf
				case 4781: state = 4862; break; // &M -> &Mf
				case 4797: state = 4798; break; // &mapstole -> &mapstolef
				case 4859: state = 4860; break; // &Mellintr -> &Mellintrf
				case 4929: state = 4930; break; // &Mop -> &Mopf
				case 4932: state = 4933; break; // &mop -> &mopf
				case 4965: state = 5192; break; // &n -> &nf
				case 4971: state = 5189; break; // &N -> &Nf
				case 5270: state = 5282; break; // &nle -> &nlef
				case 5273: state = 5274; break; // &nLe -> &nLef
				case 5369: state = 5370; break; // &Nop -> &Nopf
				case 5373: state = 5374; break; // &nop -> &nopf
				case 5529: state = 5530; break; // &NotLe -> &NotLef
				case 6014: state = 6015; break; // &ntrianglele -> &ntrianglelef
				case 6079: state = 6080; break; // &nvin -> &nvinf
				case 6131: state = 6205; break; // &O -> &Of
				case 6138: state = 6200; break; // &o -> &of
				case 6295: state = 6296; break; // &Oop -> &Oopf
				case 6299: state = 6300; break; // &oop -> &oopf
				case 6348: state = 6356; break; // &ord -> &ordf
				case 6353: state = 6354; break; // &ordero -> &orderof
				case 6362: state = 6363; break; // &origo -> &origof
				case 6463: state = 6521; break; // &p -> &pf
				case 6482: state = 6518; break; // &P -> &Pf
				case 6547: state = 6548; break; // &pitch -> &pitchf
				case 6630: state = 6631; break; // &Pop -> &Popf
				case 6633: state = 6634; break; // &pop -> &popf
				case 6745: state = 6754; break; // &pro -> &prof
				case 6767: state = 6768; break; // &profsur -> &profsurf
				case 6813: state = 6814; break; // &Q -> &Qf
				case 6817: state = 6818; break; // &q -> &qf
				case 6826: state = 6827; break; // &Qop -> &Qopf
				case 6830: state = 6831; break; // &qop -> &qopf
				case 6876: state = 7135; break; // &r -> &rf
				case 6886: state = 7146; break; // &R -> &Rf
				case 6932: state = 6944; break; // &rarr -> &rarrf
				case 6937: state = 6939; break; // &rarrb -> &rarrbf
				case 7214: state = 7215; break; // &RightArrowLe -> &RightArrowLef
				case 7295: state = 7296; break; // &rightle -> &rightlef
				case 7481: state = 7489; break; // &rop -> &ropf
				case 7486: state = 7487; break; // &Rop -> &Ropf
				case 7579: state = 7583; break; // &rtri -> &rtrif
				case 7610: state = 7741; break; // &S -> &Sf
				case 7617: state = 7744; break; // &s -> &sf
				case 7787: state = 7788; break; // &ShortLe -> &ShortLef
				case 7841: state = 7843; break; // &sigma -> &sigmaf
				case 7936: state = 7937; break; // &so -> &sof
				case 7950: state = 7951; break; // &Sop -> &Sopf
				case 7953: state = 7954; break; // &sop -> &sopf
				case 8008: state = 8066; break; // &squ -> &squf
				case 8016: state = 8064; break; // &squar -> &squarf
				case 8093: state = 8094; break; // &sstar -> &sstarf
				case 8102: state = 8104; break; // &star -> &starf
				case 8400: state = 8455; break; // &T -> &Tf
				case 8404: state = 8458; break; // &t -> &tf
				case 8464: state = 8476; break; // &there -> &theref
				case 8470: state = 8471; break; // &There -> &Theref
				case 8594: state = 8608; break; // &top -> &topf
				case 8605: state = 8606; break; // &Top -> &Topf
				case 8646: state = 8647; break; // &trianglele -> &trianglelef
				case 8748: state = 8749; break; // &twoheadle -> &twoheadlef
				case 8768: state = 8855; break; // &U -> &Uf
				case 8775: state = 8849; break; // &u -> &uf
				case 8964: state = 8965; break; // &Uop -> &Uopf
				case 8967: state = 8968; break; // &uop -> &uopf
				case 9054: state = 9055; break; // &upharpoonle -> &upharpoonlef
				case 9072: state = 9073; break; // &UpperLe -> &UpperLef
				case 9178: state = 9180; break; // &utri -> &utrif
				case 9201: state = 9417; break; // &v -> &vf
				case 9293: state = 9294; break; // &vartrianglele -> &vartrianglelef
				case 9303: state = 9414; break; // &V -> &Vf
				case 9433: state = 9434; break; // &Vop -> &Vopf
				case 9437: state = 9438; break; // &vop -> &vopf
				case 9484: state = 9517; break; // &W -> &Wf
				case 9490: state = 9520; break; // &w -> &wf
				case 9524: state = 9525; break; // &Wop -> &Wopf
				case 9528: state = 9529; break; // &wop -> &wopf
				case 9548: state = 9569; break; // &x -> &xf
				case 9565: state = 9566; break; // &X -> &Xf
				case 9608: state = 9609; break; // &Xop -> &Xopf
				case 9611: state = 9612; break; // &xop -> &xopf
				case 9665: state = 9702; break; // &Y -> &Yf
				case 9672: state = 9705; break; // &y -> &yf
				case 9717: state = 9718; break; // &Yop -> &Yopf
				case 9721: state = 9722; break; // &yop -> &yopf
				case 9747: state = 9811; break; // &Z -> &Zf
				case 9754: state = 9814; break; // &z -> &zf
				case 9788: state = 9789; break; // &zeetr -> &zeetrf
				case 9833: state = 9834; break; // &Zop -> &Zopf
				case 9837: state = 9838; break; // &zop -> &zopf
				default: return false;
				}
				break;
			case 'g':
				switch (state) {
				case 0: state = 2701; break; // & -> &g
				case 1: state = 67; break; // &A -> &Ag
				case 8: state = 73; break; // &a -> &ag
				case 52: state = 53; break; // &AEli -> &AElig
				case 57: state = 58; break; // &aeli -> &aelig
				case 108: state = 109; break; // &amal -> &amalg
				case 119: state = 136; break; // &an -> &ang
				case 147: state = 160; break; // &angmsda -> &angmsdag
				case 183: state = 184; break; // &Ao -> &Aog
				case 188: state = 189; break; // &ao -> &aog
				case 239: state = 240; break; // &Arin -> &Aring
				case 244: state = 245; break; // &arin -> &aring
				case 256: state = 257; break; // &Assi -> &Assig
				case 307: state = 308; break; // &backcon -> &backcong
				case 355: state = 357; break; // &barwed -> &barwedg
				case 371: state = 372; break; // &bcon -> &bcong
				case 442: state = 443; break; // &bi -> &big
				case 485: state = 486; break; // &bigtrian -> &bigtriang
				case 509: state = 510; break; // &bigwed -> &bigwedg
				case 527: state = 528; break; // &blacklozen -> &blacklozeng
				case 542: state = 543; break; // &blacktrian -> &blacktriang
				case 558: state = 559; break; // &blacktriangleri -> &blacktrianglerig
				case 999: state = 1000; break; // &circlearrowri -> &circlearrowrig
				case 1086: state = 1087; break; // &ClockwiseContourInte -> &ClockwiseContourInteg
				case 1164: state = 1165; break; // &con -> &cong
				case 1171: state = 1172; break; // &Con -> &Cong
				case 1194: state = 1195; break; // &ContourInte -> &ContourInteg
				case 1250: state = 1251; break; // &CounterClockwiseContourInte -> &CounterClockwiseContourInteg
				case 1373: state = 1374; break; // &curlywed -> &curlywedg
				case 1394: state = 1395; break; // &curvearrowri -> &curvearrowrig
				case 1426: state = 1427; break; // &Da -> &Dag
				case 1427: state = 1428; break; // &Dag -> &Dagg
				case 1433: state = 1434; break; // &da -> &dag
				case 1434: state = 1435; break; // &dag -> &dagg
				case 1494: state = 1495; break; // &dda -> &ddag
				case 1495: state = 1496; break; // &ddag -> &ddagg
				case 1516: state = 1517; break; // &de -> &deg
				case 1599: state = 1633; break; // &di -> &dig
				case 1740: state = 1741; break; // &doublebarwed -> &doublebarwedg
				case 1758: state = 1759; break; // &DoubleContourInte -> &DoubleContourInteg
				case 1787: state = 1788; break; // &DoubleLeftRi -> &DoubleLeftRig
				case 1802: state = 1803; break; // &DoubleLon -> &DoubleLong
				case 1815: state = 1816; break; // &DoubleLongLeftRi -> &DoubleLongLeftRig
				case 1826: state = 1827; break; // &DoubleLongRi -> &DoubleLongRig
				case 1837: state = 1838; break; // &DoubleRi -> &DoubleRig
				case 1945: state = 1946; break; // &downharpoonri -> &downharpoonrig
				case 1955: state = 1956; break; // &DownLeftRi -> &DownLeftRig
				case 1988: state = 1989; break; // &DownRi -> &DownRig
				case 2088: state = 2089; break; // &dwan -> &dwang
				case 2101: state = 2102; break; // &dzi -> &dzig
				case 2108: state = 2187; break; // &E -> &Eg
				case 2115: state = 2185; break; // &e -> &eg
				case 2290: state = 2291; break; // &en -> &eng
				case 2296: state = 2297; break; // &Eo -> &Eog
				case 2301: state = 2302; break; // &eo -> &eog
				case 2357: state = 2358; break; // &eqslant -> &eqslantg
				case 2508: state = 2509; break; // &fallin -> &falling
				case 2533: state = 2534; break; // &ffili -> &ffilig
				case 2537: state = 2538; break; // &ffli -> &fflig
				case 2541: state = 2542; break; // &fflli -> &ffllig
				case 2551: state = 2552; break; // &fili -> &filig
				case 2589: state = 2590; break; // &fjli -> &fjlig
				case 2597: state = 2598; break; // &flli -> &fllig
				case 2701: state = 2807; break; // &g -> &gg
				case 2708: state = 2805; break; // &G -> &Gg
				case 2807: state = 2809; break; // &gg -> &ggg
				case 3149: state = 3150; break; // &hookri -> &hookrig
				case 3236: state = 3289; break; // &I -> &Ig
				case 3243: state = 3295; break; // &i -> &ig
				case 3322: state = 3323; break; // &IJli -> &IJlig
				case 3327: state = 3328; break; // &ijli -> &ijlig
				case 3332: state = 3344; break; // &Ima -> &Imag
				case 3337: state = 3341; break; // &ima -> &imag
				case 3407: state = 3408; break; // &inte -> &integ
				case 3413: state = 3414; break; // &Inte -> &Integ
				case 3467: state = 3476; break; // &io -> &iog
				case 3471: state = 3472; break; // &Io -> &Iog
				case 3624: state = 3654; break; // &k -> &kg
				case 3692: state = 4317; break; // &l -> &lg
				case 3705: state = 3718; break; // &la -> &lag
				case 3733: state = 3734; break; // &Lan -> &Lang
				case 3736: state = 3737; break; // &lan -> &lang
				case 3894: state = 4183; break; // &lE -> &lEg
				case 3896: state = 4185; break; // &le -> &leg
				case 3902: state = 3903; break; // &LeftAn -> &LeftAng
				case 3938: state = 3939; break; // &LeftArrowRi -> &LeftArrowRig
				case 3958: state = 3959; break; // &LeftCeilin -> &LeftCeiling
				case 4031: state = 4032; break; // &LeftRi -> &LeftRig
				case 4042: state = 4043; break; // &Leftri -> &Leftrig
				case 4053: state = 4054; break; // &leftri -> &leftrig
				case 4077: state = 4078; break; // &leftrightsqui -> &leftrightsquig
				case 4123: state = 4124; break; // &LeftTrian -> &LeftTriang
				case 4197: state = 4210; break; // &les -> &lesg
				case 4215: state = 4271; break; // &less -> &lessg
				case 4228: state = 4229; break; // &lesseq -> &lesseqg
				case 4233: state = 4234; break; // &lesseqq -> &lesseqqg
				case 4424: state = 4425; break; // &loan -> &loang
				case 4435: state = 4436; break; // &Lon -> &Long
				case 4457: state = 4458; break; // &lon -> &long
				case 4470: state = 4471; break; // &LongLeftRi -> &LongLeftRig
				case 4481: state = 4482; break; // &Longleftri -> &Longleftrig
				case 4492: state = 4493; break; // &longleftri -> &longleftrig
				case 4510: state = 4511; break; // &LongRi -> &LongRig
				case 4521: state = 4522; break; // &Longri -> &Longrig
				case 4532: state = 4533; break; // &longri -> &longrig
				case 4555: state = 4556; break; // &looparrowri -> &looparrowrig
				case 4602: state = 4603; break; // &LowerRi -> &LowerRig
				case 4615: state = 4616; break; // &lozen -> &lozeng
				case 4670: state = 4674; break; // &lsim -> &lsimg
				case 4838: state = 4839; break; // &measuredan -> &measuredang
				case 4965: state = 5195; break; // &n -> &ng
				case 4983: state = 4984; break; // &nan -> &nang
				case 5045: state = 5046; break; // &ncon -> &ncong
				case 5084: state = 5085; break; // &Ne -> &Neg
				case 5212: state = 5213; break; // &nG -> &nGg
				case 5291: state = 5292; break; // &nLeftri -> &nLeftrig
				case 5302: state = 5303; break; // &nleftri -> &nleftrig
				case 5361: state = 5362; break; // &NonBreakin -> &NonBreaking
				case 5382: state = 5383; break; // &NotCon -> &NotCong
				case 5536: state = 5537; break; // &NotLeftTrian -> &NotLeftTriang
				case 5671: state = 5672; break; // &NotRi -> &NotRig
				case 5679: state = 5680; break; // &NotRightTrian -> &NotRightTriang
				case 5869: state = 5870; break; // &nRi -> &nRig
				case 5879: state = 5880; break; // &nri -> &nrig
				case 5988: state = 5989; break; // &nt -> &ntg
				case 6003: state = 6004; break; // &ntl -> &ntlg
				case 6009: state = 6010; break; // &ntrian -> &ntriang
				case 6022: state = 6023; break; // &ntriangleri -> &ntrianglerig
				case 6043: state = 6068; break; // &nv -> &nvg
				case 6131: state = 6214; break; // &O -> &Og
				case 6138: state = 6210; break; // &o -> &og
				case 6192: state = 6193; break; // &OEli -> &OElig
				case 6197: state = 6198; break; // &oeli -> &oelig
				case 6268: state = 6269; break; // &Ome -> &Omeg
				case 6272: state = 6273; break; // &ome -> &omeg
				case 6360: state = 6361; break; // &ori -> &orig
				case 6908: state = 6909; break; // &Ran -> &Rang
				case 6911: state = 6912; break; // &ran -> &rang
				case 7074: state = 7095; break; // &re -> &reg
				case 7171: state = 7172; break; // &Ri -> &Rig
				case 7176: state = 7177; break; // &RightAn -> &RightAng
				case 7199: state = 7200; break; // &ri -> &rig
				case 7233: state = 7234; break; // &RightCeilin -> &RightCeiling
				case 7315: state = 7316; break; // &rightri -> &rightrig
				case 7329: state = 7330; break; // &rightsqui -> &rightsquig
				case 7368: state = 7369; break; // &RightTrian -> &RightTriang
				case 7428: state = 7429; break; // &rin -> &ring
				case 7433: state = 7434; break; // &risin -> &rising
				case 7471: state = 7472; break; // &roan -> &roang
				case 7514: state = 7516; break; // &rpar -> &rparg
				case 7532: state = 7533; break; // &Rri -> &Rrig
				case 7813: state = 7814; break; // &ShortRi -> &ShortRig
				case 7833: state = 7834; break; // &Si -> &Sig
				case 7838: state = 7839; break; // &si -> &sig
				case 7847: state = 7857; break; // &sim -> &simg
				case 8108: state = 8109; break; // &strai -> &straig
				case 8279: state = 8280; break; // &sun -> &sung
				case 8397: state = 8398; break; // &szli -> &szlig
				case 8406: state = 8407; break; // &tar -> &targ
				case 8635: state = 8636; break; // &trian -> &triang
				case 8656: state = 8657; break; // &triangleri -> &trianglerig
				case 8758: state = 8759; break; // &twoheadri -> &twoheadrig
				case 8768: state = 8860; break; // &U -> &Ug
				case 8775: state = 8866; break; // &u -> &ug
				case 8954: state = 8955; break; // &Uo -> &Uog
				case 8959: state = 8960; break; // &uo -> &uog
				case 9059: state = 9060; break; // &upharpoonri -> &upharpoonrig
				case 9082: state = 9083; break; // &UpperRi -> &UpperRig
				case 9142: state = 9143; break; // &Urin -> &Uring
				case 9146: state = 9147; break; // &urin -> &uring
				case 9196: state = 9197; break; // &uwan -> &uwang
				case 9203: state = 9204; break; // &van -> &vang
				case 9228: state = 9229; break; // &varnothin -> &varnothing
				case 9253: state = 9254; break; // &varsi -> &varsig
				case 9288: state = 9289; break; // &vartrian -> &vartriang
				case 9298: state = 9299; break; // &vartriangleri -> &vartrianglerig
				case 9478: state = 9479; break; // &vzi -> &vzig
				case 9481: state = 9482; break; // &vzigza -> &vzigzag
				case 9497: state = 9507; break; // &wed -> &wedg
				case 9503: state = 9504; break; // &Wed -> &Wedg
				case 9661: state = 9662; break; // &xwed -> &xwedg
				case 9825: state = 9826; break; // &zi -> &zig
				default: return false;
				}
				break;
			case 'h':
				switch (state) {
				case 0: state = 3020; break; // & -> &h
				case 86: state = 87; break; // &alep -> &aleph
				case 90: state = 91; break; // &Alp -> &Alph
				case 94: state = 95; break; // &alp -> &alph
				case 147: state = 162; break; // &angmsda -> &angmsdah
				case 173: state = 174; break; // &angsp -> &angsph
				case 338: state = 339; break; // &Backslas -> &Backslash
				case 426: state = 429; break; // &bet -> &beth
				case 559: state = 560; break; // &blacktrianglerig -> &blacktrianglerigh
				case 613: state = 638; break; // &box -> &boxh
				case 691: state = 697; break; // &boxV -> &boxVh
				case 693: state = 701; break; // &boxv -> &boxvh
				case 758: state = 762; break; // &bsol -> &bsolh
				case 789: state = 973; break; // &C -> &Ch
				case 796: state = 960; break; // &c -> &ch
				case 1000: state = 1001; break; // &circlearrowrig -> &circlearrowrigh
				case 1016: state = 1017; break; // &circleddas -> &circleddash
				case 1395: state = 1396; break; // &curvearrowrig -> &curvearrowrigh
				case 1432: state = 1550; break; // &d -> &dh
				case 1441: state = 1442; break; // &dalet -> &daleth
				case 1454: state = 1455; break; // &das -> &dash
				case 1457: state = 1458; break; // &Das -> &Dash
				case 1506: state = 1507; break; // &DDotra -> &DDotrah
				case 1537: state = 1538; break; // &dfis -> &dfish
				case 1788: state = 1789; break; // &DoubleLeftRig -> &DoubleLeftRigh
				case 1816: state = 1817; break; // &DoubleLongLeftRig -> &DoubleLongLeftRigh
				case 1827: state = 1828; break; // &DoubleLongRig -> &DoubleLongRigh
				case 1838: state = 1839; break; // &DoubleRig -> &DoubleRigh
				case 1896: state = 1932; break; // &down -> &downh
				case 1946: state = 1947; break; // &downharpoonrig -> &downharpoonrigh
				case 1956: state = 1957; break; // &DownLeftRig -> &DownLeftRigh
				case 1989: state = 1990; break; // &DownRig -> &DownRigh
				case 2077: state = 2082; break; // &du -> &duh
				case 2439: state = 2445; break; // &et -> &eth
				case 3132: state = 3133; break; // &homt -> &homth
				case 3150: state = 3151; break; // &hookrig -> &hookrigh
				case 3194: state = 3195; break; // &hslas -> &hslash
				case 3231: state = 3232; break; // &hyp -> &hyph
				case 3362: state = 3363; break; // &imat -> &imath
				case 3435: state = 3436; break; // &intlar -> &intlarh
				case 3579: state = 3580; break; // &jmat -> &jmath
				case 3624: state = 3664; break; // &k -> &kh
				case 3692: state = 4325; break; // &l -> &lh
				case 3766: state = 3776; break; // &larr -> &larrh
				case 3880: state = 3881; break; // &ldrd -> &ldrdh
				case 3886: state = 3887; break; // &ldrus -> &ldrush
				case 3891: state = 3892; break; // &lds -> &ldsh
				case 3926: state = 4004; break; // &left -> &lefth
				case 3939: state = 3940; break; // &LeftArrowRig -> &LeftArrowRigh
				case 4032: state = 4033; break; // &LeftRig -> &LeftRigh
				case 4043: state = 4044; break; // &Leftrig -> &Leftrigh
				case 4054: state = 4055; break; // &leftrig -> &leftrigh
				case 4056: state = 4065; break; // &leftright -> &leftrighth
				case 4109: state = 4110; break; // &leftt -> &leftth
				case 4303: state = 4304; break; // &lfis -> &lfish
				case 4348: state = 4370; break; // &ll -> &llh
				case 4397: state = 4398; break; // &lmoustac -> &lmoustach
				case 4471: state = 4472; break; // &LongLeftRig -> &LongLeftRigh
				case 4482: state = 4483; break; // &Longleftrig -> &Longleftrigh
				case 4493: state = 4494; break; // &longleftrig -> &longleftrigh
				case 4511: state = 4512; break; // &LongRig -> &LongRigh
				case 4522: state = 4523; break; // &Longrig -> &Longrigh
				case 4533: state = 4534; break; // &longrig -> &longrigh
				case 4556: state = 4557; break; // &looparrowrig -> &looparrowrigh
				case 4603: state = 4604; break; // &LowerRig -> &LowerRigh
				case 4628: state = 4640; break; // &lr -> &lrh
				case 4652: state = 4667; break; // &ls -> &lsh
				case 4658: state = 4665; break; // &Ls -> &Lsh
				case 4698: state = 4710; break; // &lt -> &lth
				case 4745: state = 4746; break; // &lurds -> &lurdsh
				case 4750: state = 4751; break; // &luru -> &luruh
				case 4767: state = 4868; break; // &m -> &mh
				case 4822: state = 4823; break; // &mdas -> &mdash
				case 4965: state = 5227; break; // &n -> &nh
				case 5061: state = 5062; break; // &ndas -> &ndash
				case 5067: state = 5068; break; // &near -> &nearh
				case 5103: state = 5104; break; // &NegativeT -> &NegativeTh
				case 5125: state = 5126; break; // &NegativeVeryT -> &NegativeVeryTh
				case 5292: state = 5293; break; // &nLeftrig -> &nLeftrigh
				case 5303: state = 5304; break; // &nleftrig -> &nleftrigh
				case 5672: state = 5673; break; // &NotRig -> &NotRigh
				case 5870: state = 5871; break; // &nRig -> &nRigh
				case 5880: state = 5881; break; // &nrig -> &nrigh
				case 5895: state = 5910; break; // &ns -> &nsh
				case 6023: state = 6024; break; // &ntrianglerig -> &ntrianglerigh
				case 6050: state = 6051; break; // &nVDas -> &nVDash
				case 6055: state = 6056; break; // &nVdas -> &nVdash
				case 6060: state = 6061; break; // &nvDas -> &nvDash
				case 6065: state = 6066; break; // &nvdas -> &nvdash
				case 6113: state = 6114; break; // &nwar -> &nwarh
				case 6138: state = 6227; break; // &o -> &oh
				case 6165: state = 6166; break; // &odas -> &odash
				case 6388: state = 6389; break; // &Oslas -> &Oslash
				case 6393: state = 6394; break; // &oslas -> &oslash
				case 6456: state = 6457; break; // &OverParent -> &OverParenth
				case 6463: state = 6527; break; // &p -> &ph
				case 6482: state = 6524; break; // &P -> &Ph
				case 6546: state = 6547; break; // &pitc -> &pitch
				case 6559: state = 6561; break; // &planck -> &planckh
				case 6876: state = 7155; break; // &r -> &rh
				case 6886: state = 7164; break; // &R -> &Rh
				case 6932: state = 6947; break; // &rarr -> &rarrh
				case 7058: state = 7059; break; // &rdld -> &rdldh
				case 7069: state = 7070; break; // &rds -> &rdsh
				case 7137: state = 7138; break; // &rfis -> &rfish
				case 7172: state = 7173; break; // &Rig -> &Righ
				case 7200: state = 7201; break; // &rig -> &righ
				case 7202: state = 7279; break; // &right -> &righth
				case 7297: state = 7305; break; // &rightleft -> &rightlefth
				case 7316: state = 7317; break; // &rightrig -> &rightrigh
				case 7354: state = 7355; break; // &rightt -> &rightth
				case 7442: state = 7447; break; // &rl -> &rlh
				case 7460: state = 7461; break; // &rmoustac -> &rmoustach
				case 7533: state = 7534; break; // &Rrig -> &Rrigh
				case 7542: state = 7557; break; // &rs -> &rsh
				case 7548: state = 7555; break; // &Rs -> &Rsh
				case 7567: state = 7568; break; // &rt -> &rth
				case 7603: state = 7604; break; // &rulu -> &ruluh
				case 7610: state = 7772; break; // &S -> &Sh
				case 7617: state = 7751; break; // &s -> &sh
				case 7705: state = 7706; break; // &sear -> &searh
				case 7762: state = 7763; break; // &shc -> &shch
				case 7814: state = 7815; break; // &ShortRig -> &ShortRigh
				case 7907: state = 7908; break; // &smas -> &smash
				case 8109: state = 8110; break; // &straig -> &straigh
				case 8120: state = 8121; break; // &straightp -> &straightph
				case 8216: state = 8269; break; // &Suc -> &Such
				case 8270: state = 8271; break; // &SuchT -> &SuchTh
				case 8284: state = 8320; break; // &sup -> &suph
				case 8377: state = 8378; break; // &swar -> &swarh
				case 8400: state = 8467; break; // &T -> &Th
				case 8404: state = 8461; break; // &t -> &th
				case 8657: state = 8658; break; // &trianglerig -> &trianglerigh
				case 8709: state = 8723; break; // &ts -> &tsh
				case 8742: state = 8743; break; // &two -> &twoh
				case 8759: state = 8760; break; // &twoheadrig -> &twoheadrigh
				case 8775: state = 8876; break; // &u -> &uh
				case 8829: state = 8845; break; // &ud -> &udh
				case 8851: state = 8852; break; // &ufis -> &ufish
				case 8938: state = 8939; break; // &UnderParent -> &UnderParenth
				case 8983: state = 9046; break; // &up -> &uph
				case 9060: state = 9061; break; // &upharpoonrig -> &upharpoonrigh
				case 9083: state = 9084; break; // &UpperRig -> &UpperRigh
				case 9096: state = 9098; break; // &upsi -> &upsih
				case 9225: state = 9226; break; // &varnot -> &varnoth
				case 9231: state = 9232; break; // &varp -> &varph
				case 9247: state = 9249; break; // &varr -> &varrh
				case 9279: state = 9280; break; // &vart -> &varth
				case 9299: state = 9300; break; // &vartrianglerig -> &vartrianglerigh
				case 9322: state = 9323; break; // &VDas -> &VDash
				case 9327: state = 9328; break; // &Vdas -> &Vdash
				case 9332: state = 9333; break; // &vDas -> &vDash
				case 9337: state = 9338; break; // &vdas -> &vdash
				case 9404: state = 9405; break; // &VeryT -> &VeryTh
				case 9474: state = 9475; break; // &Vvdas -> &Vvdash
				case 9537: state = 9538; break; // &wreat -> &wreath
				case 9548: state = 9572; break; // &x -> &xh
				case 9754: state = 9821; break; // &z -> &zh
				case 9797: state = 9798; break; // &ZeroWidt -> &ZeroWidth
				default: return false;
				}
				break;
			case 'i':
				switch (state) {
				case 0: state = 3243; break; // & -> &i
				case 27: state = 38; break; // &ac -> &aci
				case 33: state = 34; break; // &Ac -> &Aci
				case 51: state = 52; break; // &AEl -> &AEli
				case 56: state = 57; break; // &ael -> &aeli
				case 199: state = 210; break; // &ap -> &api
				case 202: state = 203; break; // &apac -> &apaci
				case 224: state = 225; break; // &ApplyFunct -> &ApplyFuncti
				case 237: state = 238; break; // &Ar -> &Ari
				case 242: state = 243; break; // &ar -> &ari
				case 255: state = 256; break; // &Ass -> &Assi
				case 269: state = 270; break; // &At -> &Ati
				case 275: state = 276; break; // &at -> &ati
				case 289: state = 297; break; // &aw -> &awi
				case 292: state = 293; break; // &awcon -> &awconi
				case 301: state = 442; break; // &b -> &bi
				case 312: state = 313; break; // &backeps -> &backepsi
				case 319: state = 320; break; // &backpr -> &backpri
				case 324: state = 325; break; // &backs -> &backsi
				case 406: state = 407; break; // &beps -> &bepsi
				case 419: state = 420; break; // &Bernoull -> &Bernoulli
				case 444: state = 448; break; // &bigc -> &bigci
				case 465: state = 466; break; // &bigot -> &bigoti
				case 482: state = 483; break; // &bigtr -> &bigtri
				case 539: state = 540; break; // &blacktr -> &blacktri
				case 557: state = 558; break; // &blacktriangler -> &blacktriangleri
				case 583: state = 584; break; // &bnequ -> &bnequi
				case 609: state = 610; break; // &bowt -> &bowti
				case 656: state = 657; break; // &boxm -> &boxmi
				case 667: state = 668; break; // &boxt -> &boxti
				case 720: state = 721; break; // &bpr -> &bpri
				case 744: state = 752; break; // &bs -> &bsi
				case 749: state = 750; break; // &bsem -> &bsemi
				case 789: state = 1019; break; // &C -> &Ci
				case 796: state = 978; break; // &c -> &ci
				case 803: state = 828; break; // &Cap -> &Capi
				case 832: state = 833; break; // &CapitalD -> &CapitalDi
				case 840: state = 841; break; // &CapitalDifferent -> &CapitalDifferenti
				case 861: state = 890; break; // &cc -> &cci
				case 866: state = 886; break; // &Cc -> &Cci
				case 877: state = 878; break; // &Cced -> &Ccedi
				case 882: state = 883; break; // &cced -> &ccedi
				case 895: state = 896; break; // &Ccon -> &Cconi
				case 916: state = 917; break; // &ced -> &cedi
				case 921: state = 922; break; // &Ced -> &Cedi
				case 960: state = 976; break; // &ch -> &chi
				case 973: state = 974; break; // &Ch -> &Chi
				case 998: state = 999; break; // &circlearrowr -> &circlearrowri
				case 1009: state = 1010; break; // &circledc -> &circledci
				case 1032: state = 1033; break; // &CircleM -> &CircleMi
				case 1043: state = 1044; break; // &CircleT -> &CircleTi
				case 1054: state = 1055; break; // &cirfn -> &cirfni
				case 1059: state = 1060; break; // &cirm -> &cirmi
				case 1064: state = 1065; break; // &cirsc -> &cirsci
				case 1072: state = 1073; break; // &Clockw -> &Clockwi
				case 1122: state = 1123; break; // &clubsu -> &clubsui
				case 1164: state = 1183; break; // &con -> &coni
				case 1171: state = 1179; break; // &Con -> &Coni
				case 1236: state = 1237; break; // &CounterClockw -> &CounterClockwi
				case 1393: state = 1394; break; // &curvearrowr -> &curvearrowri
				case 1407: state = 1415; break; // &cw -> &cwi
				case 1410: state = 1411; break; // &cwcon -> &cwconi
				case 1425: state = 1557; break; // &D -> &Di
				case 1432: state = 1599; break; // &d -> &di
				case 1535: state = 1536; break; // &df -> &dfi
				case 1560: state = 1561; break; // &Diacr -> &Diacri
				case 1562: state = 1563; break; // &Diacrit -> &Diacriti
				case 1593: state = 1594; break; // &DiacriticalT -> &DiacriticalTi
				case 1613: state = 1614; break; // &diamondsu -> &diamondsui
				case 1627: state = 1628; break; // &Different -> &Differenti
				case 1639: state = 1640; break; // &dis -> &disi
				case 1643: state = 1645; break; // &div -> &divi
				case 1651: state = 1652; break; // &divideont -> &divideonti
				case 1713: state = 1714; break; // &dotm -> &dotmi
				case 1786: state = 1787; break; // &DoubleLeftR -> &DoubleLeftRi
				case 1814: state = 1815; break; // &DoubleLongLeftR -> &DoubleLongLeftRi
				case 1825: state = 1826; break; // &DoubleLongR -> &DoubleLongRi
				case 1836: state = 1837; break; // &DoubleR -> &DoubleRi
				case 1872: state = 1873; break; // &DoubleVert -> &DoubleVerti
				case 1944: state = 1945; break; // &downharpoonr -> &downharpoonri
				case 1954: state = 1955; break; // &DownLeftR -> &DownLeftRi
				case 1987: state = 1988; break; // &DownR -> &DownRi
				case 2072: state = 2073; break; // &dtr -> &dtri
				case 2097: state = 2101; break; // &dz -> &dzi
				case 2127: state = 2142; break; // &Ec -> &Eci
				case 2133: state = 2139; break; // &ec -> &eci
				case 2204: state = 2213; break; // &el -> &eli
				case 2323: state = 2324; break; // &eps -> &epsi
				case 2327: state = 2328; break; // &Eps -> &Epsi
				case 2340: state = 2341; break; // &eqc -> &eqci
				case 2350: state = 2351; break; // &eqs -> &eqsi
				case 2368: state = 2387; break; // &Equ -> &Equi
				case 2372: state = 2396; break; // &equ -> &equi
				case 2377: state = 2378; break; // &EqualT -> &EqualTi
				case 2388: state = 2389; break; // &Equil -> &Equili
				case 2391: state = 2392; break; // &Equilibr -> &Equilibri
				case 2418: state = 2430; break; // &Es -> &Esi
				case 2422: state = 2433; break; // &es -> &esi
				case 2458: state = 2462; break; // &ex -> &exi
				case 2466: state = 2467; break; // &Ex -> &Exi
				case 2477: state = 2478; break; // &expectat -> &expectati
				case 2487: state = 2488; break; // &Exponent -> &Exponenti
				case 2497: state = 2498; break; // &exponent -> &exponenti
				case 2503: state = 2549; break; // &f -> &fi
				case 2506: state = 2507; break; // &fall -> &falli
				case 2517: state = 2554; break; // &F -> &Fi
				case 2530: state = 2531; break; // &ff -> &ffi
				case 2532: state = 2533; break; // &ffil -> &ffili
				case 2536: state = 2537; break; // &ffl -> &ffli
				case 2540: state = 2541; break; // &ffll -> &fflli
				case 2550: state = 2551; break; // &fil -> &fili
				case 2588: state = 2589; break; // &fjl -> &fjli
				case 2596: state = 2597; break; // &fll -> &flli
				case 2631: state = 2632; break; // &Four -> &Fouri
				case 2642: state = 2643; break; // &fpart -> &fparti
				case 2701: state = 2811; break; // &g -> &gi
				case 2736: state = 2742; break; // &Gc -> &Gci
				case 2738: state = 2739; break; // &Gced -> &Gcedi
				case 2746: state = 2747; break; // &gc -> &gci
				case 2849: state = 2850; break; // &gns -> &gnsi
				case 2917: state = 2918; break; // &GreaterT -> &GreaterTi
				case 2927: state = 2931; break; // &gs -> &gsi
				case 2944: state = 2947; break; // &gtc -> &gtci
				case 2998: state = 2999; break; // &gtrs -> &gtrsi
				case 3014: state = 3100; break; // &H -> &Hi
				case 3021: state = 3022; break; // &ha -> &hai
				case 3030: state = 3031; break; // &ham -> &hami
				case 3052: state = 3053; break; // &harrc -> &harrci
				case 3064: state = 3065; break; // &Hc -> &Hci
				case 3069: state = 3070; break; // &hc -> &hci
				case 3080: state = 3081; break; // &heartsu -> &heartsui
				case 3085: state = 3086; break; // &hell -> &helli
				case 3148: state = 3149; break; // &hookr -> &hookri
				case 3171: state = 3172; break; // &Hor -> &Hori
				case 3179: state = 3180; break; // &HorizontalL -> &HorizontalLi
				case 3243: state = 3301; break; // &i -> &ii
				case 3250: state = 3257; break; // &ic -> &ici
				case 3252: state = 3253; break; // &Ic -> &Ici
				case 3301: state = 3303; break; // &ii -> &iii
				case 3303: state = 3304; break; // &iii -> &iiii
				case 3312: state = 3313; break; // &iinf -> &iinfi
				case 3321: state = 3322; break; // &IJl -> &IJli
				case 3326: state = 3327; break; // &ijl -> &ijli
				case 3344: state = 3345; break; // &Imag -> &Imagi
				case 3352: state = 3353; break; // &imagl -> &imagli
				case 3373: state = 3374; break; // &Impl -> &Impli
				case 3385: state = 3386; break; // &inf -> &infi
				case 3389: state = 3390; break; // &infint -> &infinti
				case 3428: state = 3429; break; // &Intersect -> &Intersecti
				case 3444: state = 3445; break; // &Inv -> &Invi
				case 3446: state = 3447; break; // &Invis -> &Invisi
				case 3457: state = 3458; break; // &InvisibleT -> &InvisibleTi
				case 3507: state = 3511; break; // &is -> &isi
				case 3526: state = 3534; break; // &it -> &iti
				case 3528: state = 3529; break; // &It -> &Iti
				case 3556: state = 3557; break; // &Jc -> &Jci
				case 3562: state = 3563; break; // &jc -> &jci
				case 3634: state = 3635; break; // &Kced -> &Kcedi
				case 3640: state = 3641; break; // &kced -> &kcedi
				case 3785: state = 3786; break; // &larrs -> &larrsi
				case 3795: state = 3796; break; // &lAta -> &lAtai
				case 3799: state = 3800; break; // &lata -> &latai
				case 3850: state = 3851; break; // &Lced -> &Lcedi
				case 3854: state = 3859; break; // &lce -> &lcei
				case 3855: state = 3856; break; // &lced -> &lcedi
				case 3937: state = 3938; break; // &LeftArrowR -> &LeftArrowRi
				case 3949: state = 3950; break; // &leftarrowta -> &leftarrowtai
				case 3954: state = 3955; break; // &LeftCe -> &LeftCei
				case 3956: state = 3957; break; // &LeftCeil -> &LeftCeili
				case 4030: state = 4031; break; // &LeftR -> &LeftRi
				case 4041: state = 4042; break; // &Leftr -> &Leftri
				case 4052: state = 4053; break; // &leftr -> &leftri
				case 4076: state = 4077; break; // &leftrightsqu -> &leftrightsqui
				case 4114: state = 4115; break; // &leftthreet -> &leftthreeti
				case 4120: state = 4121; break; // &LeftTr -> &LeftTri
				case 4280: state = 4281; break; // &lesss -> &lesssi
				case 4295: state = 4296; break; // &LessT -> &LessTi
				case 4301: state = 4302; break; // &lf -> &lfi
				case 4376: state = 4377; break; // &lltr -> &lltri
				case 4379: state = 4380; break; // &Lm -> &Lmi
				case 4385: state = 4386; break; // &lm -> &lmi
				case 4418: state = 4419; break; // &lns -> &lnsi
				case 4469: state = 4470; break; // &LongLeftR -> &LongLeftRi
				case 4480: state = 4481; break; // &Longleftr -> &Longleftri
				case 4491: state = 4492; break; // &longleftr -> &longleftri
				case 4509: state = 4510; break; // &LongR -> &LongRi
				case 4520: state = 4521; break; // &Longr -> &Longri
				case 4531: state = 4532; break; // &longr -> &longri
				case 4554: state = 4555; break; // &looparrowr -> &looparrowri
				case 4573: state = 4574; break; // &lot -> &loti
				case 4601: state = 4602; break; // &LowerR -> &LowerRi
				case 4649: state = 4650; break; // &lrtr -> &lrtri
				case 4652: state = 4669; break; // &ls -> &lsi
				case 4698: state = 4715; break; // &lt -> &lti
				case 4700: state = 4703; break; // &ltc -> &ltci
				case 4731: state = 4732; break; // &ltr -> &ltri
				case 4767: state = 4871; break; // &m -> &mi
				case 4781: state = 4900; break; // &M -> &Mi
				case 4844: state = 4845; break; // &Med -> &Medi
				case 4855: state = 4856; break; // &Mell -> &Melli
				case 4882: state = 4883; break; // &midc -> &midci
				case 4955: state = 4956; break; // &mult -> &multi
				case 4965: state = 5240; break; // &n -> &ni
				case 4986: state = 4990; break; // &nap -> &napi
				case 5035: state = 5036; break; // &Nced -> &Ncedi
				case 5040: state = 5041; break; // &nced -> &ncedi
				case 5087: state = 5088; break; // &Negat -> &Negati
				case 5093: state = 5094; break; // &NegativeMed -> &NegativeMedi
				case 5104: state = 5105; break; // &NegativeTh -> &NegativeThi
				case 5126: state = 5127; break; // &NegativeVeryTh -> &NegativeVeryThi
				case 5136: state = 5137; break; // &nequ -> &nequi
				case 5140: state = 5145; break; // &nes -> &nesi
				case 5177: state = 5178; break; // &NewL -> &NewLi
				case 5182: state = 5183; break; // &nex -> &nexi
				case 5215: state = 5216; break; // &ngs -> &ngsi
				case 5290: state = 5291; break; // &nLeftr -> &nLeftri
				case 5301: state = 5302; break; // &nleftr -> &nleftri
				case 5328: state = 5329; break; // &nls -> &nlsi
				case 5336: state = 5337; break; // &nltr -> &nltri
				case 5343: state = 5344; break; // &nm -> &nmi
				case 5359: state = 5360; break; // &NonBreak -> &NonBreaki
				case 5378: state = 5512; break; // &not -> &noti
				case 5405: state = 5406; break; // &NotDoubleVert -> &NotDoubleVerti
				case 5427: state = 5428; break; // &NotEqualT -> &NotEqualTi
				case 5433: state = 5434; break; // &NotEx -> &NotExi
				case 5487: state = 5488; break; // &NotGreaterT -> &NotGreaterTi
				case 5533: state = 5534; break; // &NotLeftTr -> &NotLeftTri
				case 5584: state = 5585; break; // &NotLessT -> &NotLessTi
				case 5620: state = 5621; break; // &notn -> &notni
				case 5656: state = 5671; break; // &NotR -> &NotRi
				case 5676: state = 5677; break; // &NotRightTr -> &NotRightTri
				case 5762: state = 5763; break; // &NotSucceedsT -> &NotSucceedsTi
				case 5781: state = 5782; break; // &NotT -> &NotTi
				case 5803: state = 5804; break; // &NotTildeT -> &NotTildeTi
				case 5812: state = 5813; break; // &NotVert -> &NotVerti
				case 5837: state = 5838; break; // &npol -> &npoli
				case 5855: state = 5879; break; // &nr -> &nri
				case 5868: state = 5869; break; // &nR -> &nRi
				case 5890: state = 5891; break; // &nrtr -> &nrtri
				case 5895: state = 5927; break; // &ns -> &nsi
				case 5914: state = 5915; break; // &nshortm -> &nshortmi
				case 5934: state = 5935; break; // &nsm -> &nsmi
				case 5988: state = 5998; break; // &nt -> &nti
				case 5992: state = 5993; break; // &Nt -> &Nti
				case 6006: state = 6007; break; // &ntr -> &ntri
				case 6021: state = 6022; break; // &ntriangler -> &ntriangleri
				case 6043: state = 6078; break; // &nv -> &nvi
				case 6080: state = 6081; break; // &nvinf -> &nvinfi
				case 6093: state = 6094; break; // &nvltr -> &nvltri
				case 6103: state = 6104; break; // &nvrtr -> &nvrtri
				case 6107: state = 6108; break; // &nvs -> &nvsi
				case 6138: state = 6234; break; // &o -> &oi
				case 6148: state = 6149; break; // &oc -> &oci
				case 6152: state = 6153; break; // &Oc -> &Oci
				case 6163: state = 6179; break; // &od -> &odi
				case 6191: state = 6192; break; // &OEl -> &OEli
				case 6196: state = 6197; break; // &oel -> &oeli
				case 6201: state = 6202; break; // &ofc -> &ofci
				case 6238: state = 6252; break; // &ol -> &oli
				case 6243: state = 6244; break; // &olc -> &olci
				case 6258: state = 6276; break; // &Om -> &Omi
				case 6263: state = 6282; break; // &om -> &omi
				case 6342: state = 6360; break; // &or -> &ori
				case 6399: state = 6400; break; // &Ot -> &Oti
				case 6405: state = 6406; break; // &ot -> &oti
				case 6459: state = 6460; break; // &OverParenthes -> &OverParenthesi
				case 6463: state = 6543; break; // &p -> &pi
				case 6474: state = 6475; break; // &pars -> &parsi
				case 6482: state = 6541; break; // &P -> &Pi
				case 6485: state = 6486; break; // &Part -> &Parti
				case 6498: state = 6503; break; // &per -> &peri
				case 6507: state = 6508; break; // &perm -> &permi
				case 6524: state = 6525; break; // &Ph -> &Phi
				case 6527: state = 6528; break; // &ph -> &phi
				case 6570: state = 6571; break; // &plusac -> &plusaci
				case 6576: state = 6577; break; // &plusc -> &plusci
				case 6590: state = 6591; break; // &PlusM -> &PlusMi
				case 6599: state = 6600; break; // &pluss -> &plussi
				case 6609: state = 6610; break; // &Po -> &Poi
				case 6622: state = 6623; break; // &po -> &poi
				case 6625: state = 6626; break; // &point -> &pointi
				case 6640: state = 6725; break; // &Pr -> &Pri
				case 6642: state = 6729; break; // &pr -> &pri
				case 6696: state = 6697; break; // &PrecedesT -> &PrecedesTi
				case 6717: state = 6718; break; // &precns -> &precnsi
				case 6721: state = 6722; break; // &precs -> &precsi
				case 6741: state = 6742; break; // &prns -> &prnsi
				case 6760: state = 6761; break; // &profl -> &profli
				case 6775: state = 6776; break; // &Proport -> &Proporti
				case 6786: state = 6787; break; // &prs -> &prsi
				case 6795: state = 6803; break; // &Ps -> &Psi
				case 6799: state = 6805; break; // &ps -> &psi
				case 6817: state = 6821; break; // &q -> &qi
				case 6834: state = 6835; break; // &qpr -> &qpri
				case 6849: state = 6858; break; // &quat -> &quati
				case 6852: state = 6853; break; // &quatern -> &quaterni
				case 6876: state = 7199; break; // &r -> &ri
				case 6886: state = 7171; break; // &R -> &Ri
				case 6897: state = 6898; break; // &rad -> &radi
				case 6956: state = 6957; break; // &rarrs -> &rarrsi
				case 6969: state = 6970; break; // &rAta -> &rAtai
				case 6973: state = 6978; break; // &rat -> &rati
				case 6974: state = 6975; break; // &rata -> &ratai
				case 7034: state = 7035; break; // &Rced -> &Rcedi
				case 7038: state = 7043; break; // &rce -> &rcei
				case 7039: state = 7040; break; // &rced -> &rcedi
				case 7076: state = 7078; break; // &real -> &reali
				case 7111: state = 7112; break; // &ReverseEqu -> &ReverseEqui
				case 7113: state = 7114; break; // &ReverseEquil -> &ReverseEquili
				case 7116: state = 7117; break; // &ReverseEquilibr -> &ReverseEquilibri
				case 7125: state = 7126; break; // &ReverseUpEqu -> &ReverseUpEqui
				case 7127: state = 7128; break; // &ReverseUpEquil -> &ReverseUpEquili
				case 7130: state = 7131; break; // &ReverseUpEquilibr -> &ReverseUpEquilibri
				case 7135: state = 7136; break; // &rf -> &rfi
				case 7224: state = 7225; break; // &rightarrowta -> &rightarrowtai
				case 7229: state = 7230; break; // &RightCe -> &RightCei
				case 7231: state = 7232; break; // &RightCeil -> &RightCeili
				case 7314: state = 7315; break; // &rightr -> &rightri
				case 7328: state = 7329; break; // &rightsqu -> &rightsqui
				case 7359: state = 7360; break; // &rightthreet -> &rightthreeti
				case 7365: state = 7366; break; // &RightTr -> &RightTri
				case 7431: state = 7432; break; // &ris -> &risi
				case 7465: state = 7466; break; // &rnm -> &rnmi
				case 7495: state = 7496; break; // &rot -> &roti
				case 7507: state = 7508; break; // &RoundImpl -> &RoundImpli
				case 7521: state = 7522; break; // &rppol -> &rppoli
				case 7531: state = 7532; break; // &Rr -> &Rri
				case 7567: state = 7573; break; // &rt -> &rti
				case 7578: state = 7579; break; // &rtr -> &rtri
				case 7587: state = 7588; break; // &rtriltr -> &rtriltri
				case 7610: state = 7833; break; // &S -> &Si
				case 7617: state = 7838; break; // &s -> &si
				case 7629: state = 7662; break; // &Sc -> &Sci
				case 7631: state = 7666; break; // &sc -> &sci
				case 7654: state = 7655; break; // &Sced -> &Scedi
				case 7658: state = 7659; break; // &sced -> &scedi
				case 7676: state = 7677; break; // &scns -> &scnsi
				case 7682: state = 7683; break; // &scpol -> &scpoli
				case 7687: state = 7688; break; // &scs -> &scsi
				case 7721: state = 7722; break; // &sem -> &semi
				case 7730: state = 7731; break; // &setm -> &setmi
				case 7799: state = 7800; break; // &shortm -> &shortmi
				case 7812: state = 7813; break; // &ShortR -> &ShortRi
				case 7887: state = 7888; break; // &SmallC -> &SmallCi
				case 7894: state = 7918; break; // &sm -> &smi
				case 7901: state = 7902; break; // &smallsetm -> &smallsetmi
				case 7962: state = 7963; break; // &spadesu -> &spadesui
				case 8027: state = 8028; break; // &SquareIntersect -> &SquareIntersecti
				case 8059: state = 8060; break; // &SquareUn -> &SquareUni
				case 8086: state = 8087; break; // &ssm -> &ssmi
				case 8107: state = 8108; break; // &stra -> &strai
				case 8114: state = 8115; break; // &straighteps -> &straightepsi
				case 8121: state = 8122; break; // &straightph -> &straightphi
				case 8169: state = 8190; break; // &subs -> &subsi
				case 8240: state = 8241; break; // &SucceedsT -> &SucceedsTi
				case 8261: state = 8262; break; // &succns -> &succnsi
				case 8265: state = 8266; break; // &succs -> &succsi
				case 8352: state = 8367; break; // &sups -> &supsi
				case 8396: state = 8397; break; // &szl -> &szli
				case 8400: state = 8544; break; // &T -> &Ti
				case 8404: state = 8549; break; // &t -> &ti
				case 8432: state = 8433; break; // &Tced -> &Tcedi
				case 8437: state = 8438; break; // &tced -> &tcedi
				case 8461: state = 8493; break; // &th -> &thi
				case 8467: state = 8507; break; // &Th -> &Thi
				case 8503: state = 8504; break; // &thicks -> &thicksi
				case 8531: state = 8532; break; // &thks -> &thksi
				case 8570: state = 8571; break; // &TildeT -> &TildeTi
				case 8600: state = 8601; break; // &topc -> &topci
				case 8618: state = 8619; break; // &tpr -> &tpri
				case 8628: state = 8633; break; // &tr -> &tri
				case 8655: state = 8656; break; // &triangler -> &triangleri
				case 8670: state = 8671; break; // &trim -> &trimi
				case 8676: state = 8677; break; // &Tr -> &Tri
				case 8693: state = 8694; break; // &trit -> &triti
				case 8700: state = 8701; break; // &trpez -> &trpezi
				case 8737: state = 8738; break; // &tw -> &twi
				case 8757: state = 8758; break; // &twoheadr -> &twoheadri
				case 8793: state = 8794; break; // &Uarroc -> &Uarroci
				case 8815: state = 8816; break; // &Uc -> &Uci
				case 8820: state = 8821; break; // &uc -> &uci
				case 8849: state = 8850; break; // &uf -> &ufi
				case 8901: state = 8902; break; // &ultr -> &ultri
				case 8916: state = 8945; break; // &Un -> &Uni
				case 8941: state = 8942; break; // &UnderParenthes -> &UnderParenthesi
				case 9036: state = 9037; break; // &UpEqu -> &UpEqui
				case 9038: state = 9039; break; // &UpEquil -> &UpEquili
				case 9041: state = 9042; break; // &UpEquilibr -> &UpEquilibri
				case 9058: state = 9059; break; // &upharpoonr -> &upharpoonri
				case 9081: state = 9082; break; // &UpperR -> &UpperRi
				case 9092: state = 9093; break; // &Ups -> &Upsi
				case 9095: state = 9096; break; // &ups -> &upsi
				case 9127: state = 9145; break; // &ur -> &uri
				case 9140: state = 9141; break; // &Ur -> &Uri
				case 9150: state = 9151; break; // &urtr -> &urtri
				case 9161: state = 9172; break; // &ut -> &uti
				case 9166: state = 9167; break; // &Ut -> &Uti
				case 9177: state = 9178; break; // &utr -> &utri
				case 9211: state = 9212; break; // &vareps -> &varepsi
				case 9226: state = 9227; break; // &varnoth -> &varnothi
				case 9231: state = 9235; break; // &varp -> &varpi
				case 9232: state = 9233; break; // &varph -> &varphi
				case 9252: state = 9253; break; // &vars -> &varsi
				case 9285: state = 9286; break; // &vartr -> &vartri
				case 9297: state = 9298; break; // &vartriangler -> &vartriangleri
				case 9356: state = 9357; break; // &vell -> &velli
				case 9370: state = 9374; break; // &Vert -> &Verti
				case 9382: state = 9383; break; // &VerticalL -> &VerticalLi
				case 9397: state = 9398; break; // &VerticalT -> &VerticalTi
				case 9405: state = 9406; break; // &VeryTh -> &VeryThi
				case 9422: state = 9423; break; // &vltr -> &vltri
				case 9447: state = 9448; break; // &vrtr -> &vrtri
				case 9477: state = 9478; break; // &vz -> &vzi
				case 9485: state = 9486; break; // &Wc -> &Wci
				case 9491: state = 9492; break; // &wc -> &wci
				case 9496: state = 9512; break; // &we -> &wei
				case 9548: state = 9583; break; // &x -> &xi
				case 9549: state = 9553; break; // &xc -> &xci
				case 9562: state = 9563; break; // &xdtr -> &xdtri
				case 9565: state = 9581; break; // &X -> &Xi
				case 9598: state = 9599; break; // &xn -> &xni
				case 9618: state = 9619; break; // &xot -> &xoti
				case 9652: state = 9653; break; // &xutr -> &xutri
				case 9672: state = 9712; break; // &y -> &yi
				case 9685: state = 9686; break; // &Yc -> &Yci
				case 9690: state = 9691; break; // &yc -> &yci
				case 9754: state = 9825; break; // &z -> &zi
				case 9794: state = 9795; break; // &ZeroW -> &ZeroWi
				default: return false;
				}
				break;
			case 'j':
				switch (state) {
				case 0: state = 3561; break; // & -> &j
				case 1432: state = 1665; break; // &d -> &dj
				case 2503: state = 2587; break; // &f -> &fj
				case 2701: state = 2820; break; // &g -> &gj
				case 2824: state = 2830; break; // &gl -> &glj
				case 3243: state = 3325; break; // &i -> &ij
				case 3624: state = 3672; break; // &k -> &kj
				case 3692: state = 4342; break; // &l -> &lj
				case 4965: state = 5252; break; // &n -> &nj
				case 9848: state = 9849; break; // &zw -> &zwj
				case 9851: state = 9852; break; // &zwn -> &zwnj
				default: return false;
				}
				break;
			case 'k':
				switch (state) {
				case 0: state = 3624; break; // & -> &k
				case 301: state = 513; break; // &b -> &bk
				case 303: state = 304; break; // &bac -> &back
				case 333: state = 334; break; // &Bac -> &Back
				case 361: state = 362; break; // &bbr -> &bbrk
				case 366: state = 367; break; // &bbrktbr -> &bbrktbrk
				case 519: state = 566; break; // &bl -> &blk
				case 521: state = 522; break; // &blac -> &black
				case 563: state = 564; break; // &blan -> &blank
				case 576: state = 577; break; // &bloc -> &block
				case 965: state = 966; break; // &chec -> &check
				case 970: state = 971; break; // &checkmar -> &checkmark
				case 1070: state = 1071; break; // &Cloc -> &Clock
				case 1234: state = 1235; break; // &CounterCloc -> &CounterClock
				case 1463: state = 1464; break; // &db -> &dbk
				case 2024: state = 2025; break; // &drb -> &drbk
				case 2059: state = 2060; break; // &Dstro -> &Dstrok
				case 2064: state = 2065; break; // &dstro -> &dstrok
				case 2621: state = 2626; break; // &for -> &fork
				case 3017: state = 3018; break; // &Hace -> &Hacek
				case 3020: state = 3112; break; // &h -> &hk
				case 3136: state = 3137; break; // &hoo -> &hook
				case 3199: state = 3200; break; // &Hstro -> &Hstrok
				case 3204: state = 3205; break; // &hstro -> &hstrok
				case 3436: state = 3437; break; // &intlarh -> &intlarhk
				case 3539: state = 3540; break; // &Iu -> &Iuk
				case 3544: state = 3545; break; // &iu -> &iuk
				case 3608: state = 3609; break; // &Ju -> &Juk
				case 3613: state = 3614; break; // &ju -> &juk
				case 3776: state = 3777; break; // &larrh -> &larrhk
				case 3818: state = 3819; break; // &lbbr -> &lbbrk
				case 3821: state = 3828; break; // &lbr -> &lbrk
				case 3823: state = 3826; break; // &lbrac -> &lbrack
				case 3909: state = 3910; break; // &LeftAngleBrac -> &LeftAngleBrack
				case 3970: state = 3971; break; // &LeftDoubleBrac -> &LeftDoubleBrack
				case 4335: state = 4336; break; // &lhbl -> &lhblk
				case 4431: state = 4432; break; // &lobr -> &lobrk
				case 4686: state = 4687; break; // &Lstro -> &Lstrok
				case 4691: state = 4692; break; // &lstro -> &lstrok
				case 4804: state = 4805; break; // &mar -> &mark
				case 5068: state = 5069; break; // &nearh -> &nearhk
				case 5106: state = 5107; break; // &NegativeThic -> &NegativeThick
				case 5351: state = 5352; break; // &NoBrea -> &NoBreak
				case 5358: state = 5359; break; // &NonBrea -> &NonBreak
				case 6114: state = 6115; break; // &nwarh -> &nwarhk
				case 6444: state = 6447; break; // &OverBrac -> &OverBrack
				case 6515: state = 6516; break; // &perten -> &pertenk
				case 6550: state = 6551; break; // &pitchfor -> &pitchfork
				case 6557: state = 6563; break; // &plan -> &plank
				case 6558: state = 6559; break; // &planc -> &planck
				case 6947: state = 6948; break; // &rarrh -> &rarrhk
				case 7002: state = 7003; break; // &rbbr -> &rbbrk
				case 7005: state = 7012; break; // &rbr -> &rbrk
				case 7007: state = 7010; break; // &rbrac -> &rbrack
				case 7183: state = 7184; break; // &RightAngleBrac -> &RightAngleBrack
				case 7245: state = 7246; break; // &RightDoubleBrac -> &RightDoubleBrack
				case 7478: state = 7479; break; // &robr -> &robrk
				case 7706: state = 7707; break; // &searh -> &searhk
				case 8378: state = 8379; break; // &swarh -> &swarhk
				case 8416: state = 8417; break; // &tbr -> &tbrk
				case 8461: state = 8527; break; // &th -> &thk
				case 8494: state = 8495; break; // &thic -> &thick
				case 8508: state = 8509; break; // &Thic -> &Thick
				case 8611: state = 8612; break; // &topfor -> &topfork
				case 8729: state = 8730; break; // &Tstro -> &Tstrok
				case 8734: state = 8735; break; // &tstro -> &tstrok
				case 8884: state = 8885; break; // &uhbl -> &uhblk
				case 8926: state = 8929; break; // &UnderBrac -> &UnderBrack
				case 9208: state = 9217; break; // &var -> &vark
				default: return false;
				}
				break;
			case 'l':
				switch (state) {
				case 0: state = 3692; break; // & -> &l
				case 1: state = 89; break; // &A -> &Al
				case 8: state = 79; break; // &a -> &al
				case 50: state = 51; break; // &AE -> &AEl
				case 55: state = 56; break; // &ae -> &ael
				case 104: state = 108; break; // &ama -> &amal
				case 128: state = 129; break; // &ands -> &andsl
				case 136: state = 140; break; // &ang -> &angl
				case 217: state = 218; break; // &App -> &Appl
				case 270: state = 271; break; // &Ati -> &Atil
				case 276: state = 277; break; // &ati -> &atil
				case 282: state = 283; break; // &Aum -> &Auml
				case 286: state = 287; break; // &aum -> &auml
				case 301: state = 519; break; // &b -> &bl
				case 313: state = 314; break; // &backepsi -> &backepsil
				case 335: state = 336; break; // &Backs -> &Backsl
				case 417: state = 418; break; // &Bernou -> &Bernoul
				case 418: state = 419; break; // &Bernoul -> &Bernoull
				case 460: state = 461; break; // &bigop -> &bigopl
				case 486: state = 487; break; // &bigtriang -> &bigtriangl
				case 498: state = 499; break; // &bigup -> &bigupl
				case 522: state = 523; break; // &black -> &blackl
				case 543: state = 544; break; // &blacktriang -> &blacktriangl
				case 545: state = 552; break; // &blacktriangle -> &blacktrianglel
				case 618: state = 621; break; // &boxD -> &boxDl
				case 623: state = 626; break; // &boxd -> &boxdl
				case 662: state = 663; break; // &boxp -> &boxpl
				case 673: state = 676; break; // &boxU -> &boxUl
				case 678: state = 681; break; // &boxu -> &boxul
				case 691: state = 705; break; // &boxV -> &boxVl
				case 693: state = 709; break; // &boxv -> &boxvl
				case 757: state = 758; break; // &bso -> &bsol
				case 767: state = 768; break; // &bu -> &bul
				case 768: state = 769; break; // &bul -> &bull
				case 789: state = 1068; break; // &C -> &Cl
				case 796: state = 1117; break; // &c -> &cl
				case 830: state = 831; break; // &Capita -> &Capital
				case 842: state = 843; break; // &CapitalDifferentia -> &CapitalDifferential
				case 855: state = 856; break; // &Cay -> &Cayl
				case 878: state = 879; break; // &Ccedi -> &Ccedil
				case 883: state = 884; break; // &ccedi -> &ccedil
				case 917: state = 918; break; // &cedi -> &cedil
				case 922: state = 923; break; // &Cedi -> &Cedil
				case 923: state = 924; break; // &Cedil -> &Cedill
				case 981: state = 986; break; // &circ -> &circl
				case 992: state = 993; break; // &circlearrow -> &circlearrowl
				case 1021: state = 1022; break; // &Circ -> &Circl
				case 1038: state = 1039; break; // &CircleP -> &CirclePl
				case 1089: state = 1090; break; // &ClockwiseContourIntegra -> &ClockwiseContourIntegral
				case 1096: state = 1097; break; // &CloseCur -> &CloseCurl
				case 1102: state = 1103; break; // &CloseCurlyDoub -> &CloseCurlyDoubl
				case 1126: state = 1127; break; // &Co -> &Col
				case 1131: state = 1132; break; // &co -> &col
				case 1148: state = 1153; break; // &comp -> &compl
				case 1197: state = 1198; break; // &ContourIntegra -> &ContourIntegral
				case 1231: state = 1232; break; // &CounterC -> &CounterCl
				case 1253: state = 1254; break; // &CounterClockwiseContourIntegra -> &CounterClockwiseContourIntegral
				case 1292: state = 1308; break; // &cu -> &cul
				case 1296: state = 1297; break; // &cudarr -> &cudarrl
				case 1346: state = 1353; break; // &cur -> &curl
				case 1387: state = 1388; break; // &curvearrow -> &curvearrowl
				case 1419: state = 1420; break; // &cy -> &cyl
				case 1432: state = 1669; break; // &d -> &dl
				case 1433: state = 1439; break; // &da -> &dal
				case 1463: state = 1470; break; // &db -> &dbl
				case 1516: state = 1525; break; // &de -> &del
				case 1519: state = 1520; break; // &De -> &Del
				case 1552: state = 1553; break; // &dhar -> &dharl
				case 1565: state = 1566; break; // &Diacritica -> &Diacritical
				case 1578: state = 1579; break; // &DiacriticalDoub -> &DiacriticalDoubl
				case 1594: state = 1595; break; // &DiacriticalTi -> &DiacriticalTil
				case 1629: state = 1630; break; // &Differentia -> &Differential
				case 1679: state = 1680; break; // &do -> &dol
				case 1680: state = 1681; break; // &dol -> &doll
				case 1710: state = 1711; break; // &DotEqua -> &DotEqual
				case 1719: state = 1720; break; // &dotp -> &dotpl
				case 1732: state = 1733; break; // &doub -> &doubl
				case 1745: state = 1746; break; // &Doub -> &Doubl
				case 1761: state = 1762; break; // &DoubleContourIntegra -> &DoubleContourIntegral
				case 1875: state = 1876; break; // &DoubleVertica -> &DoubleVertical
				case 1938: state = 1939; break; // &downharpoon -> &downharpoonl
				case 2054: state = 2055; break; // &dso -> &dsol
				case 2089: state = 2090; break; // &dwang -> &dwangl
				case 2108: state = 2206; break; // &E -> &El
				case 2115: state = 2204; break; // &e -> &el
				case 2148: state = 2149; break; // &eco -> &ecol
				case 2204: state = 2220; break; // &el -> &ell
				case 2251: state = 2252; break; // &EmptySma -> &EmptySmal
				case 2252: state = 2253; break; // &EmptySmal -> &EmptySmall
				case 2269: state = 2270; break; // &EmptyVerySma -> &EmptyVerySmal
				case 2270: state = 2271; break; // &EmptyVerySmal -> &EmptyVerySmall
				case 2312: state = 2319; break; // &ep -> &epl
				case 2316: state = 2317; break; // &epars -> &eparsl
				case 2324: state = 2333; break; // &epsi -> &epsil
				case 2328: state = 2329; break; // &Epsi -> &Epsil
				case 2345: state = 2346; break; // &eqco -> &eqcol
				case 2350: state = 2354; break; // &eqs -> &eqsl
				case 2357: state = 2362; break; // &eqslant -> &eqslantl
				case 2369: state = 2370; break; // &Equa -> &Equal
				case 2373: state = 2374; break; // &equa -> &equal
				case 2378: state = 2379; break; // &EqualTi -> &EqualTil
				case 2387: state = 2388; break; // &Equi -> &Equil
				case 2406: state = 2407; break; // &eqvpars -> &eqvparsl
				case 2448: state = 2449; break; // &Eum -> &Euml
				case 2452: state = 2453; break; // &eum -> &euml
				case 2459: state = 2460; break; // &exc -> &excl
				case 2489: state = 2490; break; // &Exponentia -> &Exponential
				case 2499: state = 2500; break; // &exponentia -> &exponential
				case 2503: state = 2592; break; // &f -> &fl
				case 2504: state = 2505; break; // &fa -> &fal
				case 2505: state = 2506; break; // &fal -> &fall
				case 2526: state = 2527; break; // &fema -> &femal
				case 2530: state = 2536; break; // &ff -> &ffl
				case 2531: state = 2532; break; // &ffi -> &ffil
				case 2536: state = 2540; break; // &ffl -> &ffll
				case 2549: state = 2550; break; // &fi -> &fil
				case 2554: state = 2555; break; // &Fi -> &Fil
				case 2555: state = 2556; break; // &Fil -> &Fill
				case 2561: state = 2562; break; // &FilledSma -> &FilledSmal
				case 2562: state = 2563; break; // &FilledSmal -> &FilledSmall
				case 2577: state = 2578; break; // &FilledVerySma -> &FilledVerySmal
				case 2578: state = 2579; break; // &FilledVerySmal -> &FilledVerySmall
				case 2587: state = 2588; break; // &fj -> &fjl
				case 2592: state = 2596; break; // &fl -> &fll
				case 2617: state = 2618; break; // &ForA -> &ForAl
				case 2618: state = 2619; break; // &ForAl -> &ForAll
				case 2622: state = 2623; break; // &fora -> &foral
				case 2623: state = 2624; break; // &foral -> &forall
				case 2686: state = 2687; break; // &fras -> &frasl
				case 2701: state = 2824; break; // &g -> &gl
				case 2739: state = 2740; break; // &Gcedi -> &Gcedil
				case 2763: state = 2767; break; // &gE -> &gEl
				case 2765: state = 2769; break; // &ge -> &gel
				case 2775: state = 2776; break; // &geqs -> &geqsl
				case 2781: state = 2794; break; // &ges -> &gesl
				case 2790: state = 2792; break; // &gesdoto -> &gesdotol
				case 2813: state = 2814; break; // &gime -> &gimel
				case 2875: state = 2876; break; // &GreaterEqua -> &GreaterEqual
				case 2884: state = 2885; break; // &GreaterFu -> &GreaterFul
				case 2885: state = 2886; break; // &GreaterFul -> &GreaterFull
				case 2890: state = 2891; break; // &GreaterFullEqua -> &GreaterFullEqual
				case 2906: state = 2907; break; // &GreaterS -> &GreaterSl
				case 2914: state = 2915; break; // &GreaterSlantEqua -> &GreaterSlantEqual
				case 2918: state = 2919; break; // &GreaterTi -> &GreaterTil
				case 2932: state = 2936; break; // &gsim -> &gsiml
				case 2942: state = 2954; break; // &gt -> &gtl
				case 2965: state = 2993; break; // &gtr -> &gtrl
				case 2981: state = 2982; break; // &gtreq -> &gtreql
				case 2987: state = 2988; break; // &gtreqq -> &gtreqql
				case 3021: state = 3027; break; // &ha -> &hal
				case 3031: state = 3032; break; // &hami -> &hamil
				case 3074: state = 3084; break; // &he -> &hel
				case 3084: state = 3085; break; // &hel -> &hell
				case 3100: state = 3101; break; // &Hi -> &Hil
				case 3137: state = 3138; break; // &hook -> &hookl
				case 3177: state = 3178; break; // &Horizonta -> &Horizontal
				case 3188: state = 3192; break; // &hs -> &hsl
				case 3222: state = 3223; break; // &HumpEqua -> &HumpEqual
				case 3227: state = 3228; break; // &hybu -> &hybul
				case 3228: state = 3229; break; // &hybul -> &hybull
				case 3278: state = 3279; break; // &iexc -> &iexcl
				case 3320: state = 3321; break; // &IJ -> &IJl
				case 3325: state = 3326; break; // &ij -> &ijl
				case 3341: state = 3352; break; // &imag -> &imagl
				case 3372: state = 3373; break; // &Imp -> &Impl
				case 3401: state = 3433; break; // &int -> &intl
				case 3404: state = 3405; break; // &intca -> &intcal
				case 3416: state = 3417; break; // &Integra -> &Integral
				case 3421: state = 3422; break; // &interca -> &intercal
				case 3448: state = 3449; break; // &Invisib -> &Invisibl
				case 3529: state = 3530; break; // &Iti -> &Itil
				case 3534: state = 3535; break; // &iti -> &itil
				case 3549: state = 3550; break; // &Ium -> &Iuml
				case 3552: state = 3553; break; // &ium -> &iuml
				case 3635: state = 3636; break; // &Kcedi -> &Kcedil
				case 3641: state = 3642; break; // &kcedi -> &kcedil
				case 3692: state = 4348; break; // &l -> &ll
				case 3698: state = 4346; break; // &L -> &Ll
				case 3737: state = 3741; break; // &lang -> &langl
				case 3746: state = 3747; break; // &Lap -> &Lapl
				case 3766: state = 3779; break; // &larr -> &larrl
				case 3782: state = 3783; break; // &larrp -> &larrpl
				case 3789: state = 3790; break; // &larrt -> &larrtl
				case 3796: state = 3797; break; // &lAtai -> &lAtail
				case 3800: state = 3801; break; // &latai -> &latail
				case 3831: state = 3832; break; // &lbrks -> &lbrksl
				case 3851: state = 3852; break; // &Lcedi -> &Lcedil
				case 3856: state = 3857; break; // &lcedi -> &lcedil
				case 3859: state = 3860; break; // &lcei -> &lceil
				case 3903: state = 3904; break; // &LeftAng -> &LeftAngl
				case 3926: state = 4019; break; // &left -> &leftl
				case 3950: state = 3951; break; // &leftarrowtai -> &leftarrowtail
				case 3955: state = 3956; break; // &LeftCei -> &LeftCeil
				case 3964: state = 3965; break; // &LeftDoub -> &LeftDoubl
				case 3998: state = 3999; break; // &LeftF -> &LeftFl
				case 4124: state = 4125; break; // &LeftTriang -> &LeftTriangl
				case 4135: state = 4136; break; // &LeftTriangleEqua -> &LeftTriangleEqual
				case 4191: state = 4192; break; // &leqs -> &leqsl
				case 4243: state = 4244; break; // &LessEqua -> &LessEqual
				case 4254: state = 4255; break; // &LessFu -> &LessFul
				case 4255: state = 4256; break; // &LessFul -> &LessFull
				case 4260: state = 4261; break; // &LessFullEqua -> &LessFullEqual
				case 4284: state = 4285; break; // &LessS -> &LessSl
				case 4292: state = 4293; break; // &LessSlantEqua -> &LessSlantEqual
				case 4296: state = 4297; break; // &LessTi -> &LessTil
				case 4301: state = 4307; break; // &lf -> &lfl
				case 4330: state = 4332; break; // &lharu -> &lharul
				case 4334: state = 4335; break; // &lhb -> &lhbl
				case 4436: state = 4447; break; // &Long -> &Longl
				case 4458: state = 4459; break; // &long -> &longl
				case 4548: state = 4549; break; // &looparrow -> &looparrowl
				case 4560: state = 4569; break; // &lop -> &lopl
				case 4623: state = 4625; break; // &lpar -> &lparl
				case 4698: state = 4720; break; // &lt -> &ltl
				case 4767: state = 4909; break; // &m -> &ml
				case 4768: state = 4772; break; // &ma -> &mal
				case 4789: state = 4796; break; // &mapsto -> &mapstol
				case 4839: state = 4840; break; // &measuredang -> &measuredangl
				case 4843: state = 4854; break; // &Me -> &Mel
				case 4854: state = 4855; break; // &Mel -> &Mell
				case 4904: state = 4905; break; // &MinusP -> &MinusPl
				case 4917: state = 4918; break; // &mnp -> &mnpl
				case 4924: state = 4925; break; // &mode -> &model
				case 4952: state = 4954; break; // &mu -> &mul
				case 4965: state = 5256; break; // &n -> &nl
				case 4967: state = 4968; break; // &nab -> &nabl
				case 5005: state = 5006; break; // &natura -> &natural
				case 5036: state = 5037; break; // &Ncedi -> &Ncedil
				case 5041: state = 5042; break; // &ncedi -> &ncedil
				case 5204: state = 5205; break; // &ngeqs -> &ngeqsl
				case 5272: state = 5326; break; // &nL -> &nLl
				case 5316: state = 5317; break; // &nleqs -> &nleqsl
				case 5399: state = 5400; break; // &NotDoub -> &NotDoubl
				case 5408: state = 5409; break; // &NotDoubleVertica -> &NotDoubleVertical
				case 5414: state = 5415; break; // &NotE -> &NotEl
				case 5424: state = 5425; break; // &NotEqua -> &NotEqual
				case 5428: state = 5429; break; // &NotEqualTi -> &NotEqualTil
				case 5450: state = 5451; break; // &NotGreaterEqua -> &NotGreaterEqual
				case 5454: state = 5455; break; // &NotGreaterFu -> &NotGreaterFul
				case 5455: state = 5456; break; // &NotGreaterFul -> &NotGreaterFull
				case 5460: state = 5461; break; // &NotGreaterFullEqua -> &NotGreaterFullEqual
				case 5476: state = 5477; break; // &NotGreaterS -> &NotGreaterSl
				case 5484: state = 5485; break; // &NotGreaterSlantEqua -> &NotGreaterSlantEqual
				case 5488: state = 5489; break; // &NotGreaterTi -> &NotGreaterTil
				case 5509: state = 5510; break; // &NotHumpEqua -> &NotHumpEqual
				case 5537: state = 5538; break; // &NotLeftTriang -> &NotLeftTriangl
				case 5548: state = 5549; break; // &NotLeftTriangleEqua -> &NotLeftTriangleEqual
				case 5557: state = 5558; break; // &NotLessEqua -> &NotLessEqual
				case 5573: state = 5574; break; // &NotLessS -> &NotLessSl
				case 5581: state = 5582; break; // &NotLessSlantEqua -> &NotLessSlantEqual
				case 5585: state = 5586; break; // &NotLessTi -> &NotLessTil
				case 5642: state = 5643; break; // &NotPrecedesEqua -> &NotPrecedesEqual
				case 5645: state = 5646; break; // &NotPrecedesS -> &NotPrecedesSl
				case 5653: state = 5654; break; // &NotPrecedesSlantEqua -> &NotPrecedesSlantEqual
				case 5663: state = 5664; break; // &NotReverseE -> &NotReverseEl
				case 5680: state = 5681; break; // &NotRightTriang -> &NotRightTriangl
				case 5691: state = 5692; break; // &NotRightTriangleEqua -> &NotRightTriangleEqual
				case 5710: state = 5711; break; // &NotSquareSubsetEqua -> &NotSquareSubsetEqual
				case 5723: state = 5724; break; // &NotSquareSupersetEqua -> &NotSquareSupersetEqual
				case 5735: state = 5736; break; // &NotSubsetEqua -> &NotSubsetEqual
				case 5748: state = 5749; break; // &NotSucceedsEqua -> &NotSucceedsEqual
				case 5751: state = 5752; break; // &NotSucceedsS -> &NotSucceedsSl
				case 5759: state = 5760; break; // &NotSucceedsSlantEqua -> &NotSucceedsSlantEqual
				case 5763: state = 5764; break; // &NotSucceedsTi -> &NotSucceedsTil
				case 5778: state = 5779; break; // &NotSupersetEqua -> &NotSupersetEqual
				case 5782: state = 5783; break; // &NotTi -> &NotTil
				case 5790: state = 5791; break; // &NotTildeEqua -> &NotTildeEqual
				case 5794: state = 5795; break; // &NotTildeFu -> &NotTildeFul
				case 5795: state = 5796; break; // &NotTildeFul -> &NotTildeFull
				case 5800: state = 5801; break; // &NotTildeFullEqua -> &NotTildeFullEqual
				case 5804: state = 5805; break; // &NotTildeTi -> &NotTildeTil
				case 5815: state = 5816; break; // &NotVertica -> &NotVertical
				case 5825: state = 5826; break; // &npara -> &nparal
				case 5826: state = 5827; break; // &nparal -> &nparall
				case 5828: state = 5829; break; // &nparalle -> &nparallel
				case 5831: state = 5832; break; // &npars -> &nparsl
				case 5836: state = 5837; break; // &npo -> &npol
				case 5921: state = 5922; break; // &nshortpara -> &nshortparal
				case 5922: state = 5923; break; // &nshortparal -> &nshortparall
				case 5924: state = 5925; break; // &nshortparalle -> &nshortparallel
				case 5988: state = 6003; break; // &nt -> &ntl
				case 5989: state = 5990; break; // &ntg -> &ntgl
				case 5993: state = 5994; break; // &Nti -> &Ntil
				case 5998: state = 5999; break; // &nti -> &ntil
				case 6010: state = 6011; break; // &ntriang -> &ntriangl
				case 6012: state = 6013; break; // &ntriangle -> &ntrianglel
				case 6043: state = 6084; break; // &nv -> &nvl
				case 6138: state = 6238; break; // &o -> &ol
				case 6169: state = 6170; break; // &Odb -> &Odbl
				case 6174: state = 6175; break; // &odb -> &odbl
				case 6186: state = 6187; break; // &odso -> &odsol
				case 6190: state = 6191; break; // &OE -> &OEl
				case 6195: state = 6196; break; // &oe -> &oel
				case 6302: state = 6336; break; // &op -> &opl
				case 6311: state = 6312; break; // &OpenCur -> &OpenCurl
				case 6317: state = 6318; break; // &OpenCurlyDoub -> &OpenCurlyDoubl
				case 6368: state = 6369; break; // &ors -> &orsl
				case 6378: state = 6386; break; // &Os -> &Osl
				case 6382: state = 6391; break; // &os -> &osl
				case 6396: state = 6397; break; // &oso -> &osol
				case 6400: state = 6401; break; // &Oti -> &Otil
				case 6406: state = 6407; break; // &oti -> &otil
				case 6423: state = 6424; break; // &Oum -> &Ouml
				case 6427: state = 6428; break; // &oum -> &ouml
				case 6463: state = 6555; break; // &p -> &pl
				case 6467: state = 6469; break; // &para -> &paral
				case 6469: state = 6470; break; // &paral -> &parall
				case 6471: state = 6472; break; // &paralle -> &parallel
				case 6474: state = 6478; break; // &pars -> &parsl
				case 6482: state = 6587; break; // &P -> &Pl
				case 6487: state = 6488; break; // &Partia -> &Partial
				case 6508: state = 6509; break; // &permi -> &permil
				case 6616: state = 6617; break; // &Poincarep -> &Poincarepl
				case 6666: state = 6667; break; // &preccur -> &preccurl
				case 6682: state = 6683; break; // &PrecedesEqua -> &PrecedesEqual
				case 6685: state = 6686; break; // &PrecedesS -> &PrecedesSl
				case 6693: state = 6694; break; // &PrecedesSlantEqua -> &PrecedesSlantEqual
				case 6697: state = 6698; break; // &PrecedesTi -> &PrecedesTil
				case 6754: state = 6760; break; // &prof -> &profl
				case 6755: state = 6756; break; // &profa -> &profal
				case 6780: state = 6781; break; // &Proportiona -> &Proportional
				case 6792: state = 6793; break; // &prure -> &prurel
				case 6876: state = 7442; break; // &r -> &rl
				case 6912: state = 6918; break; // &rang -> &rangl
				case 6932: state = 6950; break; // &rarr -> &rarrl
				case 6953: state = 6954; break; // &rarrp -> &rarrpl
				case 6960: state = 6961; break; // &Rarrt -> &Rarrtl
				case 6963: state = 6964; break; // &rarrt -> &rarrtl
				case 6970: state = 6971; break; // &rAtai -> &rAtail
				case 6975: state = 6976; break; // &ratai -> &ratail
				case 6982: state = 6983; break; // &rationa -> &rational
				case 7015: state = 7016; break; // &rbrks -> &rbrksl
				case 7035: state = 7036; break; // &Rcedi -> &Rcedil
				case 7040: state = 7041; break; // &rcedi -> &rcedil
				case 7043: state = 7044; break; // &rcei -> &rceil
				case 7053: state = 7057; break; // &rd -> &rdl
				case 7075: state = 7076; break; // &rea -> &real
				case 7102: state = 7103; break; // &ReverseE -> &ReverseEl
				case 7112: state = 7113; break; // &ReverseEqui -> &ReverseEquil
				case 7126: state = 7127; break; // &ReverseUpEqui -> &ReverseUpEquil
				case 7135: state = 7141; break; // &rf -> &rfl
				case 7160: state = 7162; break; // &rharu -> &rharul
				case 7177: state = 7178; break; // &RightAng -> &RightAngl
				case 7202: state = 7294; break; // &right -> &rightl
				case 7225: state = 7226; break; // &rightarrowtai -> &rightarrowtail
				case 7230: state = 7231; break; // &RightCei -> &RightCeil
				case 7239: state = 7240; break; // &RightDoub -> &RightDoubl
				case 7273: state = 7274; break; // &RightF -> &RightFl
				case 7369: state = 7370; break; // &RightTriang -> &RightTriangl
				case 7380: state = 7381; break; // &RightTriangleEqua -> &RightTriangleEqual
				case 7481: state = 7491; break; // &rop -> &ropl
				case 7506: state = 7507; break; // &RoundImp -> &RoundImpl
				case 7520: state = 7521; break; // &rppo -> &rppol
				case 7579: state = 7585; break; // &rtri -> &rtril
				case 7590: state = 7591; break; // &Ru -> &Rul
				case 7594: state = 7595; break; // &RuleDe -> &RuleDel
				case 7601: state = 7602; break; // &ru -> &rul
				case 7617: state = 7878; break; // &s -> &sl
				case 7655: state = 7656; break; // &Scedi -> &Scedil
				case 7659: state = 7660; break; // &scedi -> &scedil
				case 7681: state = 7682; break; // &scpo -> &scpol
				case 7806: state = 7807; break; // &shortpara -> &shortparal
				case 7807: state = 7808; break; // &shortparal -> &shortparall
				case 7809: state = 7810; break; // &shortparalle -> &shortparallel
				case 7847: state = 7861; break; // &sim -> &siml
				case 7868: state = 7869; break; // &simp -> &simpl
				case 7884: state = 7885; break; // &Sma -> &Smal
				case 7885: state = 7886; break; // &Smal -> &Small
				case 7890: state = 7891; break; // &SmallCirc -> &SmallCircl
				case 7895: state = 7896; break; // &sma -> &smal
				case 7896: state = 7897; break; // &smal -> &small
				case 7915: state = 7916; break; // &smepars -> &smeparsl
				case 7918: state = 7921; break; // &smi -> &smil
				case 7936: state = 7942; break; // &so -> &sol
				case 8042: state = 8043; break; // &SquareSubsetEqua -> &SquareSubsetEqual
				case 8055: state = 8056; break; // &SquareSupersetEqua -> &SquareSupersetEqual
				case 8087: state = 8088; break; // &ssmi -> &ssmil
				case 8115: state = 8116; break; // &straightepsi -> &straightepsil
				case 8146: state = 8147; break; // &submu -> &submul
				case 8155: state = 8156; break; // &subp -> &subpl
				case 8181: state = 8182; break; // &SubsetEqua -> &SubsetEqual
				case 8210: state = 8211; break; // &succcur -> &succcurl
				case 8226: state = 8227; break; // &SucceedsEqua -> &SucceedsEqual
				case 8229: state = 8230; break; // &SucceedsS -> &SucceedsSl
				case 8237: state = 8238; break; // &SucceedsSlantEqua -> &SucceedsSlantEqual
				case 8241: state = 8242; break; // &SucceedsTi -> &SucceedsTil
				case 8284: state = 8328; break; // &sup -> &supl
				case 8317: state = 8318; break; // &SupersetEqua -> &SupersetEqual
				case 8322: state = 8323; break; // &suphso -> &suphsol
				case 8334: state = 8335; break; // &supmu -> &supmul
				case 8343: state = 8344; break; // &supp -> &suppl
				case 8395: state = 8396; break; // &sz -> &szl
				case 8433: state = 8434; break; // &Tcedi -> &Tcedil
				case 8438: state = 8439; break; // &tcedi -> &tcedil
				case 8449: state = 8450; break; // &te -> &tel
				case 8544: state = 8545; break; // &Ti -> &Til
				case 8549: state = 8550; break; // &ti -> &til
				case 8557: state = 8558; break; // &TildeEqua -> &TildeEqual
				case 8561: state = 8562; break; // &TildeFu -> &TildeFul
				case 8562: state = 8563; break; // &TildeFul -> &TildeFull
				case 8567: state = 8568; break; // &TildeFullEqua -> &TildeFullEqual
				case 8571: state = 8572; break; // &TildeTi -> &TildeTil
				case 8636: state = 8637; break; // &triang -> &triangl
				case 8638: state = 8645; break; // &triangle -> &trianglel
				case 8678: state = 8679; break; // &Trip -> &Tripl
				case 8685: state = 8686; break; // &trip -> &tripl
				case 8746: state = 8747; break; // &twohead -> &twoheadl
				case 8775: state = 8887; break; // &u -> &ul
				case 8835: state = 8836; break; // &Udb -> &Udbl
				case 8840: state = 8841; break; // &udb -> &udbl
				case 8878: state = 8879; break; // &uhar -> &uharl
				case 8883: state = 8884; break; // &uhb -> &uhbl
				case 8909: state = 8914; break; // &um -> &uml
				case 8949: state = 8950; break; // &UnionP -> &UnionPl
				case 8983: state = 9064; break; // &up -> &upl
				case 9037: state = 9038; break; // &UpEqui -> &UpEquil
				case 9052: state = 9053; break; // &upharpoon -> &upharpoonl
				case 9093: state = 9100; break; // &Upsi -> &Upsil
				case 9096: state = 9104; break; // &upsi -> &upsil
				case 9167: state = 9168; break; // &Uti -> &Util
				case 9172: state = 9173; break; // &uti -> &util
				case 9188: state = 9189; break; // &Uum -> &Uuml
				case 9191: state = 9192; break; // &uum -> &uuml
				case 9197: state = 9198; break; // &uwang -> &uwangl
				case 9201: state = 9420; break; // &v -> &vl
				case 9212: state = 9213; break; // &varepsi -> &varepsil
				case 9289: state = 9290; break; // &vartriang -> &vartriangl
				case 9291: state = 9292; break; // &vartriangle -> &vartrianglel
				case 9328: state = 9340; break; // &Vdash -> &Vdashl
				case 9345: state = 9355; break; // &ve -> &vel
				case 9355: state = 9356; break; // &vel -> &vell
				case 9376: state = 9377; break; // &Vertica -> &Vertical
				case 9398: state = 9399; break; // &VerticalTi -> &VerticalTil
				case 9548: state = 9585; break; // &x -> &xl
				case 9611: state = 9614; break; // &xop -> &xopl
				case 9646: state = 9647; break; // &xup -> &xupl
				case 9741: state = 9742; break; // &Yum -> &Yuml
				case 9744: state = 9745; break; // &yum -> &yuml
				default: return false;
				}
				break;
			case 'm':
				switch (state) {
				case 0: state = 4767; break; // & -> &m
				case 1: state = 98; break; // &A -> &Am
				case 8: state = 103; break; // &a -> &am
				case 83: state = 84; break; // &alefsy -> &alefsym
				case 136: state = 143; break; // &ang -> &angm
				case 262: state = 263; break; // &asy -> &asym
				case 281: state = 282; break; // &Au -> &Aum
				case 285: state = 286; break; // &au -> &aum
				case 320: state = 321; break; // &backpri -> &backprim
				case 325: state = 326; break; // &backsi -> &backsim
				case 384: state = 399; break; // &be -> &bem
				case 466: state = 467; break; // &bigoti -> &bigotim
				case 605: state = 606; break; // &botto -> &bottom
				case 613: state = 656; break; // &box -> &boxm
				case 668: state = 669; break; // &boxti -> &boxtim
				case 721: state = 722; break; // &bpri -> &bprim
				case 748: state = 749; break; // &bse -> &bsem
				case 752: state = 753; break; // &bsi -> &bsim
				case 767: state = 774; break; // &bu -> &bum
				case 781: state = 782; break; // &Bu -> &Bum
				case 904: state = 905; break; // &ccupss -> &ccupssm
				case 915: state = 927; break; // &ce -> &cem
				case 966: state = 968; break; // &check -> &checkm
				case 979: state = 1059; break; // &cir -> &cirm
				case 1044: state = 1045; break; // &CircleTi -> &CircleTim
				case 1131: state = 1142; break; // &co -> &com
				case 1142: state = 1143; break; // &com -> &comm
				case 1154: state = 1155; break; // &comple -> &complem
				case 1349: state = 1351; break; // &curarr -> &curarrm
				case 1516: state = 1529; break; // &de -> &dem
				case 1558: state = 1603; break; // &Dia -> &Diam
				case 1600: state = 1601; break; // &dia -> &diam
				case 1634: state = 1635; break; // &diga -> &digam
				case 1635: state = 1636; break; // &digam -> &digamm
				case 1652: state = 1653; break; // &divideonti -> &divideontim
				case 1694: state = 1713; break; // &dot -> &dotm
				case 2108: state = 2228; break; // &E -> &Em
				case 2115: state = 2233; break; // &e -> &em
				case 2207: state = 2208; break; // &Ele -> &Elem
				case 2249: state = 2250; break; // &EmptyS -> &EmptySm
				case 2267: state = 2268; break; // &EmptyVeryS -> &EmptyVerySm
				case 2351: state = 2352; break; // &eqsi -> &eqsim
				case 2393: state = 2394; break; // &Equilibriu -> &Equilibrium
				case 2430: state = 2431; break; // &Esi -> &Esim
				case 2433: state = 2434; break; // &esi -> &esim
				case 2447: state = 2448; break; // &Eu -> &Eum
				case 2451: state = 2452; break; // &eu -> &eum
				case 2524: state = 2525; break; // &fe -> &fem
				case 2559: state = 2560; break; // &FilledS -> &FilledSm
				case 2575: state = 2576; break; // &FilledVeryS -> &FilledVerySm
				case 2702: state = 2714; break; // &ga -> &gam
				case 2709: state = 2710; break; // &Ga -> &Gam
				case 2710: state = 2711; break; // &Gam -> &Gamm
				case 2714: state = 2715; break; // &gam -> &gamm
				case 2811: state = 2812; break; // &gi -> &gim
				case 2850: state = 2851; break; // &gnsi -> &gnsim
				case 2931: state = 2932; break; // &gsi -> &gsim
				case 2999: state = 3000; break; // &gtrsi -> &gtrsim
				case 3021: state = 3030; break; // &ha -> &ham
				case 3126: state = 3131; break; // &ho -> &hom
				case 3207: state = 3208; break; // &Hu -> &Hum
				case 3215: state = 3216; break; // &HumpDownHu -> &HumpDownHum
				case 3236: state = 3330; break; // &I -> &Im
				case 3243: state = 3336; break; // &i -> &im
				case 3452: state = 3453; break; // &InvisibleCo -> &InvisibleCom
				case 3453: state = 3454; break; // &InvisibleCom -> &InvisibleComm
				case 3458: state = 3459; break; // &InvisibleTi -> &InvisibleTim
				case 3539: state = 3549; break; // &Iu -> &Ium
				case 3544: state = 3552; break; // &iu -> &ium
				case 3561: state = 3577; break; // &j -> &jm
				case 3692: state = 4385; break; // &l -> &lm
				case 3698: state = 4379; break; // &L -> &Lm
				case 3699: state = 3723; break; // &La -> &Lam
				case 3705: state = 3728; break; // &la -> &lam
				case 3711: state = 3712; break; // &lae -> &laem
				case 3786: state = 3787; break; // &larrsi -> &larrsim
				case 4115: state = 4116; break; // &leftthreeti -> &leftthreetim
				case 4281: state = 4282; break; // &lesssi -> &lesssim
				case 4419: state = 4420; break; // &lnsi -> &lnsim
				case 4458: state = 4502; break; // &long -> &longm
				case 4574: state = 4575; break; // &loti -> &lotim
				case 4628: state = 4646; break; // &lr -> &lrm
				case 4669: state = 4670; break; // &lsi -> &lsim
				case 4715: state = 4716; break; // &lti -> &ltim
				case 4810: state = 4811; break; // &mco -> &mcom
				case 4811: state = 4812; break; // &mcom -> &mcomm
				case 4846: state = 4847; break; // &Mediu -> &Medium
				case 4952: state = 4961; break; // &mu -> &mum
				case 4956: state = 4957; break; // &multi -> &multim
				case 4965: state = 5343; break; // &n -> &nm
				case 5014: state = 5015; break; // &nbu -> &nbum
				case 5095: state = 5096; break; // &NegativeMediu -> &NegativeMedium
				case 5145: state = 5146; break; // &nesi -> &nesim
				case 5216: state = 5217; break; // &ngsi -> &ngsim
				case 5329: state = 5330; break; // &nlsi -> &nlsim
				case 5416: state = 5417; break; // &NotEle -> &NotElem
				case 5494: state = 5495; break; // &NotHu -> &NotHum
				case 5502: state = 5503; break; // &NotHumpDownHu -> &NotHumpDownHum
				case 5665: state = 5666; break; // &NotReverseEle -> &NotReverseElem
				case 5895: state = 5934; break; // &ns -> &nsm
				case 5913: state = 5914; break; // &nshort -> &nshortm
				case 5927: state = 5928; break; // &nsi -> &nsim
				case 6032: state = 6034; break; // &nu -> &num
				case 6108: state = 6109; break; // &nvsi -> &nvsim
				case 6131: state = 6258; break; // &O -> &Om
				case 6138: state = 6263; break; // &o -> &om
				case 6227: state = 6232; break; // &oh -> &ohm
				case 6348: state = 6358; break; // &ord -> &ordm
				case 6400: state = 6411; break; // &Oti -> &Otim
				case 6406: state = 6415; break; // &oti -> &otim
				case 6422: state = 6423; break; // &Ou -> &Oum
				case 6426: state = 6427; break; // &ou -> &oum
				case 6463: state = 6607; break; // &p -> &pm
				case 6475: state = 6476; break; // &parsi -> &parsim
				case 6498: state = 6507; break; // &per -> &perm
				case 6527: state = 6532; break; // &ph -> &phm
				case 6532: state = 6533; break; // &phm -> &phmm
				case 6567: state = 6596; break; // &plus -> &plusm
				case 6600: state = 6601; break; // &plussi -> &plussim
				case 6718: state = 6719; break; // &precnsi -> &precnsim
				case 6722: state = 6723; break; // &precsi -> &precsim
				case 6725: state = 6726; break; // &Pri -> &Prim
				case 6729: state = 6730; break; // &pri -> &prim
				case 6742: state = 6743; break; // &prnsi -> &prnsim
				case 6787: state = 6788; break; // &prsi -> &prsim
				case 6835: state = 6836; break; // &qpri -> &qprim
				case 6876: state = 7453; break; // &r -> &rm
				case 6901: state = 6902; break; // &rae -> &raem
				case 6957: state = 6958; break; // &rarrsi -> &rarrsim
				case 7104: state = 7105; break; // &ReverseEle -> &ReverseElem
				case 7118: state = 7119; break; // &ReverseEquilibriu -> &ReverseEquilibrium
				case 7132: state = 7133; break; // &ReverseUpEquilibriu -> &ReverseUpEquilibrium
				case 7360: state = 7361; break; // &rightthreeti -> &rightthreetim
				case 7442: state = 7451; break; // &rl -> &rlm
				case 7464: state = 7465; break; // &rn -> &rnm
				case 7496: state = 7497; break; // &roti -> &rotim
				case 7504: state = 7505; break; // &RoundI -> &RoundIm
				case 7573: state = 7574; break; // &rti -> &rtim
				case 7610: state = 7883; break; // &S -> &Sm
				case 7617: state = 7894; break; // &s -> &sm
				case 7677: state = 7678; break; // &scnsi -> &scnsim
				case 7688: state = 7689; break; // &scsi -> &scsim
				case 7703: state = 7721; break; // &se -> &sem
				case 7729: state = 7730; break; // &set -> &setm
				case 7798: state = 7799; break; // &short -> &shortm
				case 7834: state = 7835; break; // &Sig -> &Sigm
				case 7838: state = 7847; break; // &si -> &sim
				case 7839: state = 7840; break; // &sig -> &sigm
				case 7900: state = 7901; break; // &smallset -> &smallsetm
				case 8077: state = 8086; break; // &ss -> &ssm
				case 8082: state = 8083; break; // &sset -> &ssetm
				case 8127: state = 8275; break; // &Su -> &Sum
				case 8130: state = 8277; break; // &su -> &sum
				case 8131: state = 8145; break; // &sub -> &subm
				case 8190: state = 8191; break; // &subsi -> &subsim
				case 8262: state = 8263; break; // &succnsi -> &succnsim
				case 8266: state = 8267; break; // &succsi -> &succsim
				case 8284: state = 8333; break; // &sup -> &supm
				case 8367: state = 8368; break; // &supsi -> &supsim
				case 8488: state = 8489; break; // &thetasy -> &thetasym
				case 8504: state = 8505; break; // &thicksi -> &thicksim
				case 8532: state = 8533; break; // &thksi -> &thksim
				case 8549: state = 8576; break; // &ti -> &tim
				case 8619: state = 8620; break; // &tpri -> &tprim
				case 8633: state = 8670; break; // &tri -> &trim
				case 8694: state = 8695; break; // &triti -> &tritim
				case 8702: state = 8703; break; // &trpeziu -> &trpezium
				case 8768: state = 8904; break; // &U -> &Um
				case 8775: state = 8909; break; // &u -> &um
				case 9043: state = 9044; break; // &UpEquilibriu -> &UpEquilibrium
				case 9182: state = 9191; break; // &uu -> &uum
				case 9187: state = 9188; break; // &Uu -> &Uum
				case 9254: state = 9255; break; // &varsig -> &varsigm
				case 9548: state = 9594; break; // &x -> &xm
				case 9619: state = 9620; break; // &xoti -> &xotim
				case 9736: state = 9744; break; // &yu -> &yum
				case 9740: state = 9741; break; // &Yu -> &Yum
				default: return false;
				}
				break;
			case 'n':
				switch (state) {
				case 0: state = 4965; break; // & -> &n
				case 1: state = 116; break; // &A -> &An
				case 8: state = 119; break; // &a -> &an
				case 122: state = 123; break; // &anda -> &andan
				case 185: state = 186; break; // &Aogo -> &Aogon
				case 190: state = 191; break; // &aogo -> &aogon
				case 221: state = 222; break; // &ApplyFu -> &ApplyFun
				case 226: state = 227; break; // &ApplyFunctio -> &ApplyFunction
				case 238: state = 239; break; // &Ari -> &Arin
				case 243: state = 244; break; // &ari -> &arin
				case 257: state = 258; break; // &Assig -> &Assign
				case 291: state = 292; break; // &awco -> &awcon
				case 293: state = 294; break; // &awconi -> &awconin
				case 297: state = 298; break; // &awi -> &awin
				case 301: state = 579; break; // &b -> &bn
				case 306: state = 307; break; // &backco -> &backcon
				case 315: state = 316; break; // &backepsilo -> &backepsilon
				case 370: state = 371; break; // &bco -> &bcon
				case 409: state = 410; break; // &ber -> &bern
				case 414: state = 415; break; // &Ber -> &Bern
				case 433: state = 434; break; // &betwee -> &between
				case 484: state = 485; break; // &bigtria -> &bigtrian
				case 491: state = 492; break; // &bigtriangledow -> &bigtriangledown
				case 520: state = 563; break; // &bla -> &blan
				case 526: state = 527; break; // &blackloze -> &blacklozen
				case 541: state = 542; break; // &blacktria -> &blacktrian
				case 549: state = 550; break; // &blacktriangledow -> &blacktriangledown
				case 657: state = 658; break; // &boxmi -> &boxmin
				case 807: state = 808; break; // &capa -> &capan
				case 838: state = 839; break; // &CapitalDiffere -> &CapitalDifferen
				case 852: state = 853; break; // &caro -> &caron
				case 869: state = 870; break; // &Ccaro -> &Ccaron
				case 873: state = 874; break; // &ccaro -> &ccaron
				case 894: state = 895; break; // &Cco -> &Ccon
				case 896: state = 897; break; // &Cconi -> &Cconin
				case 915: state = 933; break; // &ce -> &cen
				case 920: state = 936; break; // &Ce -> &Cen
				case 1033: state = 1034; break; // &CircleMi -> &CircleMin
				case 1053: state = 1054; break; // &cirf -> &cirfn
				case 1055: state = 1056; break; // &cirfni -> &cirfnin
				case 1077: state = 1078; break; // &ClockwiseCo -> &ClockwiseCon
				case 1083: state = 1084; break; // &ClockwiseContourI -> &ClockwiseContourIn
				case 1126: state = 1171; break; // &Co -> &Con
				case 1128: state = 1129; break; // &Colo -> &Colon
				case 1131: state = 1164; break; // &co -> &con
				case 1133: state = 1134; break; // &colo -> &colon
				case 1150: state = 1151; break; // &compf -> &compfn
				case 1156: state = 1157; break; // &compleme -> &complemen
				case 1175: state = 1176; break; // &Congrue -> &Congruen
				case 1179: state = 1180; break; // &Coni -> &Conin
				case 1183: state = 1184; break; // &coni -> &conin
				case 1191: state = 1192; break; // &ContourI -> &ContourIn
				case 1226: state = 1227; break; // &Cou -> &Coun
				case 1241: state = 1242; break; // &CounterClockwiseCo -> &CounterClockwiseCon
				case 1247: state = 1248; break; // &CounterClockwiseContourI -> &CounterClockwiseContourIn
				case 1378: state = 1379; break; // &curre -> &curren
				case 1409: state = 1410; break; // &cwco -> &cwcon
				case 1411: state = 1412; break; // &cwconi -> &cwconin
				case 1415: state = 1416; break; // &cwi -> &cwin
				case 1477: state = 1478; break; // &Dcaro -> &Dcaron
				case 1483: state = 1484; break; // &dcaro -> &dcaron
				case 1604: state = 1605; break; // &Diamo -> &Diamon
				case 1608: state = 1609; break; // &diamo -> &diamon
				case 1625: state = 1626; break; // &Differe -> &Differen
				case 1640: state = 1641; break; // &disi -> &disin
				case 1649: state = 1650; break; // &divideo -> &divideon
				case 1657: state = 1658; break; // &divo -> &divon
				case 1672: state = 1673; break; // &dlcor -> &dlcorn
				case 1714: state = 1715; break; // &dotmi -> &dotmin
				case 1749: state = 1750; break; // &DoubleCo -> &DoubleCon
				case 1755: state = 1756; break; // &DoubleContourI -> &DoubleContourIn
				case 1768: state = 1769; break; // &DoubleDow -> &DoubleDown
				case 1801: state = 1802; break; // &DoubleLo -> &DoubleLon
				case 1861: state = 1862; break; // &DoubleUpDow -> &DoubleUpDown
				case 1881: state = 1882; break; // &Dow -> &Down
				case 1895: state = 1896; break; // &dow -> &down
				case 1923: state = 1924; break; // &downdow -> &downdown
				case 1937: state = 1938; break; // &downharpoo -> &downharpoon
				case 2033: state = 2034; break; // &drcor -> &drcorn
				case 2087: state = 2088; break; // &dwa -> &dwan
				case 2115: state = 2290; break; // &e -> &en
				case 2130: state = 2131; break; // &Ecaro -> &Ecaron
				case 2136: state = 2137; break; // &ecaro -> &ecaron
				case 2150: state = 2151; break; // &ecolo -> &ecolon
				case 2209: state = 2210; break; // &Eleme -> &Elemen
				case 2213: state = 2214; break; // &eli -> &elin
				case 2298: state = 2299; break; // &Eogo -> &Eogon
				case 2303: state = 2304; break; // &eogo -> &eogon
				case 2330: state = 2331; break; // &Epsilo -> &Epsilon
				case 2334: state = 2335; break; // &epsilo -> &epsilon
				case 2347: state = 2348; break; // &eqcolo -> &eqcolon
				case 2355: state = 2356; break; // &eqsla -> &eqslan
				case 2479: state = 2480; break; // &expectatio -> &expectation
				case 2483: state = 2484; break; // &Expo -> &Expon
				case 2485: state = 2486; break; // &Expone -> &Exponen
				case 2493: state = 2494; break; // &expo -> &expon
				case 2495: state = 2496; break; // &expone -> &exponen
				case 2503: state = 2604; break; // &f -> &fn
				case 2507: state = 2508; break; // &falli -> &fallin
				case 2600: state = 2601; break; // &flt -> &fltn
				case 2643: state = 2644; break; // &fparti -> &fpartin
				case 2690: state = 2691; break; // &frow -> &frown
				case 2701: state = 2832; break; // &g -> &gn
				case 2777: state = 2778; break; // &geqsla -> &geqslan
				case 2908: state = 2909; break; // &GreaterSla -> &GreaterSlan
				case 3002: state = 3011; break; // &gv -> &gvn
				case 3005: state = 3006; break; // &gvert -> &gvertn
				case 3091: state = 3092; break; // &herco -> &hercon
				case 3174: state = 3175; break; // &Horizo -> &Horizon
				case 3180: state = 3181; break; // &HorizontalLi -> &HorizontalLin
				case 3212: state = 3213; break; // &HumpDow -> &HumpDown
				case 3233: state = 3234; break; // &hyphe -> &hyphen
				case 3236: state = 3398; break; // &I -> &In
				case 3243: state = 3378; break; // &i -> &in
				case 3301: state = 3311; break; // &ii -> &iin
				case 3303: state = 3308; break; // &iii -> &iiin
				case 3304: state = 3305; break; // &iiii -> &iiiin
				case 3313: state = 3314; break; // &iinfi -> &iinfin
				case 3345: state = 3346; break; // &Imagi -> &Imagin
				case 3353: state = 3354; break; // &imagli -> &imaglin
				case 3386: state = 3387; break; // &infi -> &infin
				case 3430: state = 3431; break; // &Intersectio -> &Intersection
				case 3473: state = 3474; break; // &Iogo -> &Iogon
				case 3477: state = 3478; break; // &iogo -> &iogon
				case 3511: state = 3512; break; // &isi -> &isin
				case 3657: state = 3658; break; // &kgree -> &kgreen
				case 3692: state = 4401; break; // &l -> &ln
				case 3699: state = 3733; break; // &La -> &Lan
				case 3705: state = 3736; break; // &la -> &lan
				case 3720: state = 3721; break; // &lagra -> &lagran
				case 3840: state = 3841; break; // &Lcaro -> &Lcaron
				case 3846: state = 3847; break; // &lcaro -> &lcaron
				case 3901: state = 3902; break; // &LeftA -> &LeftAn
				case 3957: state = 3958; break; // &LeftCeili -> &LeftCeilin
				case 3975: state = 3976; break; // &LeftDow -> &LeftDown
				case 4009: state = 4010; break; // &leftharpoo -> &leftharpoon
				case 4013: state = 4014; break; // &leftharpoondow -> &leftharpoondown
				case 4070: state = 4071; break; // &leftrightharpoo -> &leftrightharpoon
				case 4122: state = 4123; break; // &LeftTria -> &LeftTrian
				case 4142: state = 4143; break; // &LeftUpDow -> &LeftUpDown
				case 4193: state = 4194; break; // &leqsla -> &leqslan
				case 4286: state = 4287; break; // &LessSla -> &LessSlan
				case 4356: state = 4357; break; // &llcor -> &llcorn
				case 4422: state = 4457; break; // &lo -> &lon
				case 4423: state = 4424; break; // &loa -> &loan
				case 4434: state = 4435; break; // &Lo -> &Lon
				case 4614: state = 4615; break; // &loze -> &lozen
				case 4635: state = 4636; break; // &lrcor -> &lrcorn
				case 4755: state = 4764; break; // &lv -> &lvn
				case 4758: state = 4759; break; // &lvert -> &lvertn
				case 4767: state = 4916; break; // &m -> &mn
				case 4793: state = 4794; break; // &mapstodow -> &mapstodown
				case 4837: state = 4838; break; // &measureda -> &measuredan
				case 4856: state = 4857; break; // &Melli -> &Mellin
				case 4871: state = 4890; break; // &mi -> &min
				case 4900: state = 4901; break; // &Mi -> &Min
				case 4966: state = 4983; break; // &na -> &nan
				case 5027: state = 5028; break; // &Ncaro -> &Ncaron
				case 5031: state = 5032; break; // &ncaro -> &ncaron
				case 5044: state = 5045; break; // &nco -> &ncon
				case 5105: state = 5114; break; // &NegativeThi -> &NegativeThin
				case 5127: state = 5128; break; // &NegativeVeryThi -> &NegativeVeryThin
				case 5178: state = 5179; break; // &NewLi -> &NewLin
				case 5206: state = 5207; break; // &ngeqsla -> &ngeqslan
				case 5318: state = 5319; break; // &nleqsla -> &nleqslan
				case 5347: state = 5354; break; // &No -> &Non
				case 5360: state = 5361; break; // &NonBreaki -> &NonBreakin
				case 5378: state = 5620; break; // &not -> &notn
				case 5381: state = 5382; break; // &NotCo -> &NotCon
				case 5386: state = 5387; break; // &NotCongrue -> &NotCongruen
				case 5418: state = 5419; break; // &NotEleme -> &NotElemen
				case 5478: state = 5479; break; // &NotGreaterSla -> &NotGreaterSlan
				case 5499: state = 5500; break; // &NotHumpDow -> &NotHumpDown
				case 5512: state = 5513; break; // &noti -> &notin
				case 5535: state = 5536; break; // &NotLeftTria -> &NotLeftTrian
				case 5575: state = 5576; break; // &NotLessSla -> &NotLessSlan
				case 5647: state = 5648; break; // &NotPrecedesSla -> &NotPrecedesSlan
				case 5667: state = 5668; break; // &NotReverseEleme -> &NotReverseElemen
				case 5678: state = 5679; break; // &NotRightTria -> &NotRightTrian
				case 5753: state = 5754; break; // &NotSucceedsSla -> &NotSucceedsSlan
				case 5838: state = 5839; break; // &npoli -> &npolin
				case 6008: state = 6009; break; // &ntria -> &ntrian
				case 6078: state = 6079; break; // &nvi -> &nvin
				case 6081: state = 6082; break; // &nvinfi -> &nvinfin
				case 6111: state = 6126; break; // &nw -> &nwn
				case 6211: state = 6212; break; // &ogo -> &ogon
				case 6234: state = 6235; break; // &oi -> &oin
				case 6252: state = 6253; break; // &oli -> &olin
				case 6279: state = 6280; break; // &Omicro -> &Omicron
				case 6282: state = 6290; break; // &omi -> &omin
				case 6285: state = 6286; break; // &omicro -> &omicron
				case 6307: state = 6308; break; // &Ope -> &Open
				case 6454: state = 6455; break; // &OverPare -> &OverParen
				case 6499: state = 6500; break; // &perc -> &percn
				case 6514: state = 6515; break; // &perte -> &perten
				case 6537: state = 6538; break; // &pho -> &phon
				case 6556: state = 6557; break; // &pla -> &plan
				case 6591: state = 6592; break; // &PlusMi -> &PlusMin
				case 6596: state = 6597; break; // &plusm -> &plusmn
				case 6610: state = 6611; break; // &Poi -> &Poin
				case 6618: state = 6619; break; // &Poincarepla -> &Poincareplan
				case 6623: state = 6624; break; // &poi -> &poin
				case 6626: state = 6627; break; // &pointi -> &pointin
				case 6636: state = 6637; break; // &pou -> &poun
				case 6642: state = 6735; break; // &pr -> &prn
				case 6655: state = 6705; break; // &prec -> &precn
				case 6687: state = 6688; break; // &PrecedesSla -> &PrecedesSlan
				case 6761: state = 6762; break; // &profli -> &proflin
				case 6777: state = 6778; break; // &Proportio -> &Proportion
				case 6807: state = 6808; break; // &pu -> &pun
				case 6821: state = 6822; break; // &qi -> &qin
				case 6851: state = 6852; break; // &quater -> &quatern
				case 6854: state = 6855; break; // &quaternio -> &quaternion
				case 6858: state = 6859; break; // &quati -> &quatin
				case 6876: state = 7464; break; // &r -> &rn
				case 6882: state = 6911; break; // &ra -> &ran
				case 6887: state = 6908; break; // &Ra -> &Ran
				case 6979: state = 6981; break; // &ratio -> &ration
				case 7024: state = 7025; break; // &Rcaro -> &Rcaron
				case 7030: state = 7031; break; // &rcaro -> &rcaron
				case 7078: state = 7079; break; // &reali -> &realin
				case 7106: state = 7107; break; // &ReverseEleme -> &ReverseElemen
				case 7175: state = 7176; break; // &RightA -> &RightAn
				case 7199: state = 7428; break; // &ri -> &rin
				case 7232: state = 7233; break; // &RightCeili -> &RightCeilin
				case 7250: state = 7251; break; // &RightDow -> &RightDown
				case 7284: state = 7285; break; // &rightharpoo -> &rightharpoon
				case 7288: state = 7289; break; // &rightharpoondow -> &rightharpoondown
				case 7310: state = 7311; break; // &rightleftharpoo -> &rightleftharpoon
				case 7367: state = 7368; break; // &RightTria -> &RightTrian
				case 7387: state = 7388; break; // &RightUpDow -> &RightUpDown
				case 7432: state = 7433; break; // &risi -> &risin
				case 7470: state = 7471; break; // &roa -> &roan
				case 7501: state = 7502; break; // &Rou -> &Roun
				case 7522: state = 7523; break; // &rppoli -> &rppolin
				case 7631: state = 7670; break; // &sc -> &scn
				case 7638: state = 7639; break; // &Scaro -> &Scaron
				case 7642: state = 7643; break; // &scaro -> &scaron
				case 7683: state = 7684; break; // &scpoli -> &scpolin
				case 7730: state = 7736; break; // &setm -> &setmn
				case 7731: state = 7732; break; // &setmi -> &setmin
				case 7748: state = 7749; break; // &sfrow -> &sfrown
				case 7778: state = 7779; break; // &ShortDow -> &ShortDown
				case 7847: state = 7865; break; // &sim -> &simn
				case 7902: state = 7903; break; // &smallsetmi -> &smallsetmin
				case 8019: state = 8020; break; // &SquareI -> &SquareIn
				case 8029: state = 8030; break; // &SquareIntersectio -> &SquareIntersection
				case 8058: state = 8059; break; // &SquareU -> &SquareUn
				case 8061: state = 8062; break; // &SquareUnio -> &SquareUnion
				case 8083: state = 8084; break; // &ssetm -> &ssetmn
				case 8106: state = 8124; break; // &str -> &strn
				case 8117: state = 8118; break; // &straightepsilo -> &straightepsilon
				case 8130: state = 8279; break; // &su -> &sun
				case 8131: state = 8150; break; // &sub -> &subn
				case 8171: state = 8184; break; // &subset -> &subsetn
				case 8199: state = 8249; break; // &succ -> &succn
				case 8231: state = 8232; break; // &SucceedsSla -> &SucceedsSlan
				case 8284: state = 8338; break; // &sup -> &supn
				case 8354: state = 8361; break; // &supset -> &supsetn
				case 8375: state = 8390; break; // &sw -> &swn
				case 8422: state = 8423; break; // &Tcaro -> &Tcaron
				case 8428: state = 8429; break; // &tcaro -> &tcaron
				case 8493: state = 8516; break; // &thi -> &thin
				case 8507: state = 8520; break; // &Thi -> &Thin
				case 8541: state = 8542; break; // &thor -> &thorn
				case 8549: state = 8587; break; // &ti -> &tin
				case 8634: state = 8635; break; // &tria -> &trian
				case 8642: state = 8643; break; // &triangledow -> &triangledown
				case 8671: state = 8672; break; // &trimi -> &trimin
				case 8768: state = 8916; break; // &U -> &Un
				case 8890: state = 8891; break; // &ulcor -> &ulcorn
				case 8936: state = 8937; break; // &UnderPare -> &UnderParen
				case 8946: state = 8947; break; // &Unio -> &Union
				case 8956: state = 8957; break; // &Uogo -> &Uogon
				case 8961: state = 8962; break; // &uogo -> &uogon
				case 8996: state = 8997; break; // &UpArrowDow -> &UpArrowDown
				case 9006: state = 9007; break; // &UpDow -> &UpDown
				case 9016: state = 9017; break; // &Updow -> &Updown
				case 9026: state = 9027; break; // &updow -> &updown
				case 9051: state = 9052; break; // &upharpoo -> &upharpoon
				case 9101: state = 9102; break; // &Upsilo -> &Upsilon
				case 9105: state = 9106; break; // &upsilo -> &upsilon
				case 9130: state = 9131; break; // &urcor -> &urcorn
				case 9141: state = 9142; break; // &Uri -> &Urin
				case 9145: state = 9146; break; // &uri -> &urin
				case 9195: state = 9196; break; // &uwa -> &uwan
				case 9201: state = 9425; break; // &v -> &vn
				case 9202: state = 9203; break; // &va -> &van
				case 9208: state = 9223; break; // &var -> &varn
				case 9214: state = 9215; break; // &varepsilo -> &varepsilon
				case 9227: state = 9228; break; // &varnothi -> &varnothin
				case 9262: state = 9263; break; // &varsubset -> &varsubsetn
				case 9272: state = 9273; break; // &varsupset -> &varsupsetn
				case 9287: state = 9288; break; // &vartria -> &vartrian
				case 9383: state = 9384; break; // &VerticalLi -> &VerticalLin
				case 9406: state = 9407; break; // &VeryThi -> &VeryThin
				case 9459: state = 9460; break; // &vsub -> &vsubn
				case 9465: state = 9466; break; // &vsup -> &vsupn
				case 9548: state = 9598; break; // &x -> &xn
				case 9699: state = 9700; break; // &ye -> &yen
				case 9764: state = 9765; break; // &Zcaro -> &Zcaron
				case 9770: state = 9771; break; // &zcaro -> &zcaron
				case 9848: state = 9851; break; // &zw -> &zwn
				default: return false;
				}
				break;
			case 'o':
				switch (state) {
				case 0: state = 6138; break; // & -> &o
				case 1: state = 183; break; // &A -> &Ao
				case 8: state = 188; break; // &a -> &ao
				case 129: state = 130; break; // &andsl -> &andslo
				case 184: state = 185; break; // &Aog -> &Aogo
				case 189: state = 190; break; // &aog -> &aogo
				case 199: state = 213; break; // &ap -> &apo
				case 225: state = 226; break; // &ApplyFuncti -> &ApplyFunctio
				case 230: state = 231; break; // &appr -> &appro
				case 290: state = 291; break; // &awc -> &awco
				case 301: state = 598; break; // &b -> &bo
				case 305: state = 306; break; // &backc -> &backco
				case 314: state = 315; break; // &backepsil -> &backepsilo
				case 331: state = 594; break; // &B -> &Bo
				case 369: state = 370; break; // &bc -> &bco
				case 381: state = 382; break; // &bdqu -> &bdquo
				case 410: state = 411; break; // &bern -> &berno
				case 415: state = 416; break; // &Bern -> &Berno
				case 443: state = 455; break; // &big -> &bigo
				case 456: state = 457; break; // &bigod -> &bigodo
				case 489: state = 490; break; // &bigtriangled -> &bigtriangledo
				case 515: state = 516; break; // &bkar -> &bkaro
				case 519: state = 575; break; // &bl -> &blo
				case 523: state = 524; break; // &blackl -> &blacklo
				case 547: state = 548; break; // &blacktriangled -> &blacktriangledo
				case 579: state = 591; break; // &bn -> &bno
				case 587: state = 588; break; // &bN -> &bNo
				case 604: state = 605; break; // &bott -> &botto
				case 614: state = 615; break; // &boxb -> &boxbo
				case 744: state = 757; break; // &bs -> &bso
				case 789: state = 1126; break; // &C -> &Co
				case 796: state = 1131; break; // &c -> &co
				case 824: state = 825; break; // &capd -> &capdo
				case 848: state = 852; break; // &car -> &caro
				case 866: state = 894; break; // &Cc -> &Cco
				case 868: state = 869; break; // &Ccar -> &Ccaro
				case 872: state = 873; break; // &ccar -> &ccaro
				case 907: state = 908; break; // &Cd -> &Cdo
				case 911: state = 912; break; // &cd -> &cdo
				case 940: state = 941; break; // &CenterD -> &CenterDo
				case 946: state = 947; break; // &centerd -> &centerdo
				case 990: state = 991; break; // &circlearr -> &circlearro
				case 1024: state = 1025; break; // &CircleD -> &CircleDo
				case 1068: state = 1069; break; // &Cl -> &Clo
				case 1076: state = 1077; break; // &ClockwiseC -> &ClockwiseCo
				case 1079: state = 1080; break; // &ClockwiseCont -> &ClockwiseConto
				case 1099: state = 1100; break; // &CloseCurlyD -> &CloseCurlyDo
				case 1106: state = 1107; break; // &CloseCurlyDoubleQu -> &CloseCurlyDoubleQuo
				case 1112: state = 1113; break; // &CloseCurlyQu -> &CloseCurlyQuo
				case 1127: state = 1128; break; // &Col -> &Colo
				case 1132: state = 1133; break; // &col -> &colo
				case 1167: state = 1168; break; // &congd -> &congdo
				case 1187: state = 1188; break; // &Cont -> &Conto
				case 1206: state = 1207; break; // &copr -> &copro
				case 1210: state = 1211; break; // &Copr -> &Copro
				case 1232: state = 1233; break; // &CounterCl -> &CounterClo
				case 1240: state = 1241; break; // &CounterClockwiseC -> &CounterClockwiseCo
				case 1243: state = 1244; break; // &CounterClockwiseCont -> &CounterClockwiseConto
				case 1256: state = 1266; break; // &cr -> &cro
				case 1261: state = 1262; break; // &Cr -> &Cro
				case 1288: state = 1289; break; // &ctd -> &ctdo
				case 1318: state = 1341; break; // &cup -> &cupo
				case 1337: state = 1338; break; // &cupd -> &cupdo
				case 1385: state = 1386; break; // &curvearr -> &curvearro
				case 1408: state = 1409; break; // &cwc -> &cwco
				case 1425: state = 1685; break; // &D -> &Do
				case 1432: state = 1679; break; // &d -> &do
				case 1466: state = 1467; break; // &dbkar -> &dbkaro
				case 1476: state = 1477; break; // &Dcar -> &Dcaro
				case 1482: state = 1483; break; // &dcar -> &dcaro
				case 1490: state = 1503; break; // &DD -> &DDo
				case 1492: state = 1510; break; // &dd -> &ddo
				case 1573: state = 1574; break; // &DiacriticalD -> &DiacriticalDo
				case 1601: state = 1608; break; // &diam -> &diamo
				case 1603: state = 1604; break; // &Diam -> &Diamo
				case 1643: state = 1657; break; // &div -> &divo
				case 1647: state = 1649; break; // &divide -> &divideo
				case 1670: state = 1671; break; // &dlc -> &dlco
				case 1675: state = 1676; break; // &dlcr -> &dlcro
				case 1696: state = 1697; break; // &DotD -> &DotDo
				case 1703: state = 1704; break; // &doteqd -> &doteqdo
				case 1748: state = 1749; break; // &DoubleC -> &DoubleCo
				case 1751: state = 1752; break; // &DoubleCont -> &DoubleConto
				case 1764: state = 1765; break; // &DoubleD -> &DoubleDo
				case 1772: state = 1773; break; // &DoubleDownArr -> &DoubleDownArro
				case 1776: state = 1801; break; // &DoubleL -> &DoubleLo
				case 1782: state = 1783; break; // &DoubleLeftArr -> &DoubleLeftArro
				case 1793: state = 1794; break; // &DoubleLeftRightArr -> &DoubleLeftRightArro
				case 1810: state = 1811; break; // &DoubleLongLeftArr -> &DoubleLongLeftArro
				case 1821: state = 1822; break; // &DoubleLongLeftRightArr -> &DoubleLongLeftRightArro
				case 1832: state = 1833; break; // &DoubleLongRightArr -> &DoubleLongRightArro
				case 1843: state = 1844; break; // &DoubleRightArr -> &DoubleRightArro
				case 1855: state = 1856; break; // &DoubleUpArr -> &DoubleUpArro
				case 1859: state = 1860; break; // &DoubleUpD -> &DoubleUpDo
				case 1865: state = 1866; break; // &DoubleUpDownArr -> &DoubleUpDownArro
				case 1885: state = 1886; break; // &DownArr -> &DownArro
				case 1891: state = 1892; break; // &Downarr -> &Downarro
				case 1899: state = 1900; break; // &downarr -> &downarro
				case 1911: state = 1912; break; // &DownArrowUpArr -> &DownArrowUpArro
				case 1921: state = 1922; break; // &downd -> &downdo
				case 1927: state = 1928; break; // &downdownarr -> &downdownarro
				case 1935: state = 1936; break; // &downharp -> &downharpo
				case 1936: state = 1937; break; // &downharpo -> &downharpoo
				case 1962: state = 1963; break; // &DownLeftRightVect -> &DownLeftRightVecto
				case 1972: state = 1973; break; // &DownLeftTeeVect -> &DownLeftTeeVecto
				case 1979: state = 1980; break; // &DownLeftVect -> &DownLeftVecto
				case 1998: state = 1999; break; // &DownRightTeeVect -> &DownRightTeeVecto
				case 2005: state = 2006; break; // &DownRightVect -> &DownRightVecto
				case 2019: state = 2020; break; // &DownTeeArr -> &DownTeeArro
				case 2027: state = 2028; break; // &drbkar -> &drbkaro
				case 2031: state = 2032; break; // &drc -> &drco
				case 2036: state = 2037; break; // &drcr -> &drcro
				case 2044: state = 2054; break; // &ds -> &dso
				case 2058: state = 2059; break; // &Dstr -> &Dstro
				case 2063: state = 2064; break; // &dstr -> &dstro
				case 2068: state = 2069; break; // &dtd -> &dtdo
				case 2108: state = 2296; break; // &E -> &Eo
				case 2115: state = 2301; break; // &e -> &eo
				case 2129: state = 2130; break; // &Ecar -> &Ecaro
				case 2133: state = 2148; break; // &ec -> &eco
				case 2135: state = 2136; break; // &ecar -> &ecaro
				case 2149: state = 2150; break; // &ecol -> &ecolo
				case 2157: state = 2166; break; // &eD -> &eDo
				case 2158: state = 2159; break; // &eDD -> &eDDo
				case 2162: state = 2163; break; // &Ed -> &Edo
				case 2169: state = 2170; break; // &ed -> &edo
				case 2176: state = 2177; break; // &efD -> &efDo
				case 2200: state = 2201; break; // &egsd -> &egsdo
				case 2224: state = 2225; break; // &elsd -> &elsdo
				case 2297: state = 2298; break; // &Eog -> &Eogo
				case 2302: state = 2303; break; // &eog -> &eogo
				case 2329: state = 2330; break; // &Epsil -> &Epsilo
				case 2333: state = 2334; break; // &epsil -> &epsilo
				case 2340: state = 2345; break; // &eqc -> &eqco
				case 2346: state = 2347; break; // &eqcol -> &eqcolo
				case 2414: state = 2415; break; // &erD -> &erDo
				case 2426: state = 2427; break; // &esd -> &esdo
				case 2455: state = 2456; break; // &eur -> &euro
				case 2472: state = 2493; break; // &exp -> &expo
				case 2478: state = 2479; break; // &expectati -> &expectatio
				case 2482: state = 2483; break; // &Exp -> &Expo
				case 2503: state = 2612; break; // &f -> &fo
				case 2510: state = 2511; break; // &fallingd -> &fallingdo
				case 2517: state = 2608; break; // &F -> &Fo
				case 2604: state = 2605; break; // &fn -> &fno
				case 2647: state = 2689; break; // &fr -> &fro
				case 2701: state = 2857; break; // &g -> &go
				case 2708: state = 2853; break; // &G -> &Go
				case 2755: state = 2756; break; // &Gd -> &Gdo
				case 2759: state = 2760; break; // &gd -> &gdo
				case 2786: state = 2787; break; // &gesd -> &gesdo
				case 2788: state = 2790; break; // &gesdot -> &gesdoto
				case 2837: state = 2838; break; // &gnappr -> &gnappro
				case 2950: state = 2951; break; // &gtd -> &gtdo
				case 2969: state = 2970; break; // &gtrappr -> &gtrappro
				case 2976: state = 2977; break; // &gtrd -> &gtrdo
				case 3014: state = 3159; break; // &H -> &Ho
				case 3020: state = 3126; break; // &h -> &ho
				case 3090: state = 3091; break; // &herc -> &herco
				case 3116: state = 3117; break; // &hksear -> &hksearo
				case 3122: state = 3123; break; // &hkswar -> &hkswaro
				case 3126: state = 3136; break; // &ho -> &hoo
				case 3144: state = 3145; break; // &hookleftarr -> &hookleftarro
				case 3155: state = 3156; break; // &hookrightarr -> &hookrightarro
				case 3173: state = 3174; break; // &Horiz -> &Horizo
				case 3198: state = 3199; break; // &Hstr -> &Hstro
				case 3203: state = 3204; break; // &hstr -> &hstro
				case 3210: state = 3211; break; // &HumpD -> &HumpDo
				case 3236: state = 3471; break; // &I -> &Io
				case 3243: state = 3467; break; // &i -> &io
				case 3265: state = 3266; break; // &Id -> &Ido
				case 3301: state = 3316; break; // &ii -> &iio
				case 3336: state = 3365; break; // &im -> &imo
				case 3378: state = 3393; break; // &in -> &ino
				case 3394: state = 3395; break; // &inod -> &inodo
				case 3429: state = 3430; break; // &Intersecti -> &Intersectio
				case 3440: state = 3441; break; // &intpr -> &intpro
				case 3451: state = 3452; break; // &InvisibleC -> &InvisibleCo
				case 3472: state = 3473; break; // &Iog -> &Iogo
				case 3476: state = 3477; break; // &iog -> &iogo
				case 3493: state = 3494; break; // &ipr -> &ipro
				case 3514: state = 3515; break; // &isind -> &isindo
				case 3555: state = 3582; break; // &J -> &Jo
				case 3561: state = 3586; break; // &j -> &jo
				case 3618: state = 3676; break; // &K -> &Ko
				case 3624: state = 3680; break; // &k -> &ko
				case 3692: state = 4422; break; // &l -> &lo
				case 3698: state = 4434; break; // &L -> &Lo
				case 3756: state = 3757; break; // &laqu -> &laquo
				case 3839: state = 3840; break; // &Lcar -> &Lcaro
				case 3845: state = 3846; break; // &lcar -> &lcaro
				case 3874: state = 3875; break; // &ldqu -> &ldquo
				case 3915: state = 3916; break; // &LeftArr -> &LeftArro
				case 3921: state = 3922; break; // &Leftarr -> &Leftarro
				case 3929: state = 3930; break; // &leftarr -> &leftarro
				case 3944: state = 3945; break; // &LeftArrowRightArr -> &LeftArrowRightArro
				case 3961: state = 3962; break; // &LeftD -> &LeftDo
				case 3983: state = 3984; break; // &LeftDownTeeVect -> &LeftDownTeeVecto
				case 3990: state = 3991; break; // &LeftDownVect -> &LeftDownVecto
				case 3999: state = 4000; break; // &LeftFl -> &LeftFlo
				case 4000: state = 4001; break; // &LeftFlo -> &LeftFloo
				case 4007: state = 4008; break; // &leftharp -> &leftharpo
				case 4008: state = 4009; break; // &leftharpo -> &leftharpoo
				case 4011: state = 4012; break; // &leftharpoond -> &leftharpoondo
				case 4025: state = 4026; break; // &leftleftarr -> &leftleftarro
				case 4037: state = 4038; break; // &LeftRightArr -> &LeftRightArro
				case 4048: state = 4049; break; // &Leftrightarr -> &Leftrightarro
				case 4059: state = 4060; break; // &leftrightarr -> &leftrightarro
				case 4068: state = 4069; break; // &leftrightharp -> &leftrightharpo
				case 4069: state = 4070; break; // &leftrightharpo -> &leftrightharpoo
				case 4081: state = 4082; break; // &leftrightsquigarr -> &leftrightsquigarro
				case 4088: state = 4089; break; // &LeftRightVect -> &LeftRightVecto
				case 4098: state = 4099; break; // &LeftTeeArr -> &LeftTeeArro
				case 4105: state = 4106; break; // &LeftTeeVect -> &LeftTeeVecto
				case 4140: state = 4141; break; // &LeftUpD -> &LeftUpDo
				case 4147: state = 4148; break; // &LeftUpDownVect -> &LeftUpDownVecto
				case 4157: state = 4158; break; // &LeftUpTeeVect -> &LeftUpTeeVecto
				case 4164: state = 4165; break; // &LeftUpVect -> &LeftUpVecto
				case 4175: state = 4176; break; // &LeftVect -> &LeftVecto
				case 4202: state = 4203; break; // &lesd -> &lesdo
				case 4204: state = 4206; break; // &lesdot -> &lesdoto
				case 4219: state = 4220; break; // &lessappr -> &lessappro
				case 4223: state = 4224; break; // &lessd -> &lessdo
				case 4307: state = 4308; break; // &lfl -> &lflo
				case 4308: state = 4309; break; // &lflo -> &lfloo
				case 4354: state = 4355; break; // &llc -> &llco
				case 4366: state = 4367; break; // &Lleftarr -> &Lleftarro
				case 4381: state = 4382; break; // &Lmid -> &Lmido
				case 4385: state = 4391; break; // &lm -> &lmo
				case 4387: state = 4388; break; // &lmid -> &lmido
				case 4406: state = 4407; break; // &lnappr -> &lnappro
				case 4422: state = 4542; break; // &lo -> &loo
				case 4443: state = 4444; break; // &LongLeftArr -> &LongLeftArro
				case 4453: state = 4454; break; // &Longleftarr -> &Longleftarro
				case 4465: state = 4466; break; // &longleftarr -> &longleftarro
				case 4476: state = 4477; break; // &LongLeftRightArr -> &LongLeftRightArro
				case 4487: state = 4488; break; // &Longleftrightarr -> &Longleftrightarro
				case 4498: state = 4499; break; // &longleftrightarr -> &longleftrightarro
				case 4506: state = 4507; break; // &longmapst -> &longmapsto
				case 4516: state = 4517; break; // &LongRightArr -> &LongRightArro
				case 4527: state = 4528; break; // &Longrightarr -> &Longrightarro
				case 4538: state = 4539; break; // &longrightarr -> &longrightarro
				case 4546: state = 4547; break; // &looparr -> &looparro
				case 4597: state = 4598; break; // &LowerLeftArr -> &LowerLeftArro
				case 4608: state = 4609; break; // &LowerRightArr -> &LowerRightArro
				case 4633: state = 4634; break; // &lrc -> &lrco
				case 4655: state = 4656; break; // &lsaqu -> &lsaquo
				case 4679: state = 4680; break; // &lsqu -> &lsquo
				case 4685: state = 4686; break; // &Lstr -> &Lstro
				case 4690: state = 4691; break; // &lstr -> &lstro
				case 4706: state = 4707; break; // &ltd -> &ltdo
				case 4767: state = 4922; break; // &m -> &mo
				case 4781: state = 4928; break; // &M -> &Mo
				case 4788: state = 4789; break; // &mapst -> &mapsto
				case 4791: state = 4792; break; // &mapstod -> &mapstodo
				case 4809: state = 4810; break; // &mc -> &mco
				case 4826: state = 4827; break; // &mDD -> &mDDo
				case 4868: state = 4869; break; // &mh -> &mho
				case 4873: state = 4874; break; // &micr -> &micro
				case 4886: state = 4887; break; // &midd -> &middo
				case 4946: state = 4947; break; // &mstp -> &mstpo
				case 4965: state = 5372; break; // &n -> &no
				case 4971: state = 5347; break; // &N -> &No
				case 4986: state = 4993; break; // &nap -> &napo
				case 4997: state = 4998; break; // &nappr -> &nappro
				case 5020: state = 5044; break; // &nc -> &nco
				case 5026: state = 5027; break; // &Ncar -> &Ncaro
				case 5030: state = 5031; break; // &ncar -> &ncaro
				case 5048: state = 5049; break; // &ncongd -> &ncongdo
				case 5075: state = 5077; break; // &nearr -> &nearro
				case 5080: state = 5081; break; // &ned -> &nedo
				case 5278: state = 5279; break; // &nLeftarr -> &nLeftarro
				case 5286: state = 5287; break; // &nleftarr -> &nleftarro
				case 5297: state = 5298; break; // &nLeftrightarr -> &nLeftrightarro
				case 5308: state = 5309; break; // &nleftrightarr -> &nleftrightarro
				case 5380: state = 5381; break; // &NotC -> &NotCo
				case 5396: state = 5397; break; // &NotD -> &NotDo
				case 5497: state = 5498; break; // &NotHumpD -> &NotHumpDo
				case 5515: state = 5516; break; // &notind -> &notindo
				case 5821: state = 5836; break; // &np -> &npo
				case 5875: state = 5876; break; // &nRightarr -> &nRightarro
				case 5885: state = 5886; break; // &nrightarr -> &nrightarro
				case 5910: state = 5911; break; // &nsh -> &nsho
				case 6037: state = 6038; break; // &numer -> &numero
				case 6121: state = 6123; break; // &nwarr -> &nwarro
				case 6131: state = 6294; break; // &O -> &Oo
				case 6138: state = 6298; break; // &o -> &oo
				case 6163: state = 6182; break; // &od -> &odo
				case 6185: state = 6186; break; // &ods -> &odso
				case 6210: state = 6211; break; // &og -> &ogo
				case 6247: state = 6248; break; // &olcr -> &olcro
				case 6278: state = 6279; break; // &Omicr -> &Omicro
				case 6284: state = 6285; break; // &omicr -> &omicro
				case 6314: state = 6315; break; // &OpenCurlyD -> &OpenCurlyDo
				case 6321: state = 6322; break; // &OpenCurlyDoubleQu -> &OpenCurlyDoubleQuo
				case 6327: state = 6328; break; // &OpenCurlyQu -> &OpenCurlyQuo
				case 6342: state = 6365; break; // &or -> &oro
				case 6351: state = 6353; break; // &order -> &ordero
				case 6361: state = 6362; break; // &orig -> &origo
				case 6369: state = 6370; break; // &orsl -> &orslo
				case 6382: state = 6396; break; // &os -> &oso
				case 6463: state = 6622; break; // &p -> &po
				case 6482: state = 6609; break; // &P -> &Po
				case 6503: state = 6504; break; // &peri -> &perio
				case 6527: state = 6537; break; // &ph -> &pho
				case 6548: state = 6549; break; // &pitchf -> &pitchfo
				case 6580: state = 6581; break; // &plusd -> &plusdo
				case 6604: state = 6605; break; // &plustw -> &plustwo
				case 6640: state = 6748; break; // &Pr -> &Pro
				case 6642: state = 6745; break; // &pr -> &pro
				case 6660: state = 6661; break; // &precappr -> &precappro
				case 6709: state = 6710; break; // &precnappr -> &precnappro
				case 6772: state = 6773; break; // &Prop -> &Propo
				case 6776: state = 6777; break; // &Proporti -> &Proportio
				case 6783: state = 6784; break; // &propt -> &propto
				case 6813: state = 6825; break; // &Q -> &Qo
				case 6817: state = 6829; break; // &q -> &qo
				case 6847: state = 6873; break; // &qu -> &quo
				case 6853: state = 6854; break; // &quaterni -> &quaternio
				case 6876: state = 7469; break; // &r -> &ro
				case 6886: state = 7485; break; // &R -> &Ro
				case 6922: state = 6923; break; // &raqu -> &raquo
				case 6978: state = 6979; break; // &rati -> &ratio
				case 7023: state = 7024; break; // &Rcar -> &Rcaro
				case 7029: state = 7030; break; // &rcar -> &rcaro
				case 7064: state = 7065; break; // &rdqu -> &rdquo
				case 7141: state = 7142; break; // &rfl -> &rflo
				case 7142: state = 7143; break; // &rflo -> &rfloo
				case 7155: state = 7167; break; // &rh -> &rho
				case 7164: state = 7165; break; // &Rh -> &Rho
				case 7189: state = 7190; break; // &RightArr -> &RightArro
				case 7195: state = 7196; break; // &Rightarr -> &Rightarro
				case 7205: state = 7206; break; // &rightarr -> &rightarro
				case 7219: state = 7220; break; // &RightArrowLeftArr -> &RightArrowLeftArro
				case 7236: state = 7237; break; // &RightD -> &RightDo
				case 7258: state = 7259; break; // &RightDownTeeVect -> &RightDownTeeVecto
				case 7265: state = 7266; break; // &RightDownVect -> &RightDownVecto
				case 7274: state = 7275; break; // &RightFl -> &RightFlo
				case 7275: state = 7276; break; // &RightFlo -> &RightFloo
				case 7282: state = 7283; break; // &rightharp -> &rightharpo
				case 7283: state = 7284; break; // &rightharpo -> &rightharpoo
				case 7286: state = 7287; break; // &rightharpoond -> &rightharpoondo
				case 7300: state = 7301; break; // &rightleftarr -> &rightleftarro
				case 7308: state = 7309; break; // &rightleftharp -> &rightleftharpo
				case 7309: state = 7310; break; // &rightleftharpo -> &rightleftharpoo
				case 7321: state = 7322; break; // &rightrightarr -> &rightrightarro
				case 7333: state = 7334; break; // &rightsquigarr -> &rightsquigarro
				case 7343: state = 7344; break; // &RightTeeArr -> &RightTeeArro
				case 7350: state = 7351; break; // &RightTeeVect -> &RightTeeVecto
				case 7385: state = 7386; break; // &RightUpD -> &RightUpDo
				case 7392: state = 7393; break; // &RightUpDownVect -> &RightUpDownVecto
				case 7402: state = 7403; break; // &RightUpTeeVect -> &RightUpTeeVecto
				case 7409: state = 7410; break; // &RightUpVect -> &RightUpVecto
				case 7420: state = 7421; break; // &RightVect -> &RightVecto
				case 7435: state = 7436; break; // &risingd -> &risingdo
				case 7453: state = 7454; break; // &rm -> &rmo
				case 7519: state = 7520; break; // &rpp -> &rppo
				case 7538: state = 7539; break; // &Rrightarr -> &Rrightarro
				case 7545: state = 7546; break; // &rsaqu -> &rsaquo
				case 7562: state = 7563; break; // &rsqu -> &rsquo
				case 7610: state = 7949; break; // &S -> &So
				case 7617: state = 7936; break; // &s -> &so
				case 7626: state = 7627; break; // &sbqu -> &sbquo
				case 7637: state = 7638; break; // &Scar -> &Scaro
				case 7641: state = 7642; break; // &scar -> &scaro
				case 7680: state = 7681; break; // &scp -> &scpo
				case 7695: state = 7696; break; // &sd -> &sdo
				case 7713: state = 7715; break; // &searr -> &searro
				case 7745: state = 7747; break; // &sfr -> &sfro
				case 7751: state = 7796; break; // &sh -> &sho
				case 7772: state = 7773; break; // &Sh -> &Sho
				case 7776: state = 7777; break; // &ShortD -> &ShortDo
				case 7782: state = 7783; break; // &ShortDownArr -> &ShortDownArro
				case 7792: state = 7793; break; // &ShortLeftArr -> &ShortLeftArro
				case 7819: state = 7820; break; // &ShortRightArr -> &ShortRightArro
				case 7827: state = 7828; break; // &ShortUpArr -> &ShortUpArro
				case 7849: state = 7850; break; // &simd -> &simdo
				case 8028: state = 8029; break; // &SquareIntersecti -> &SquareIntersectio
				case 8060: state = 8061; break; // &SquareUni -> &SquareUnio
				case 8116: state = 8117; break; // &straightepsil -> &straightepsilo
				case 8133: state = 8134; break; // &subd -> &subdo
				case 8141: state = 8142; break; // &subed -> &subedo
				case 8204: state = 8205; break; // &succappr -> &succappro
				case 8253: state = 8254; break; // &succnappr -> &succnappro
				case 8292: state = 8293; break; // &supd -> &supdo
				case 8304: state = 8305; break; // &suped -> &supedo
				case 8321: state = 8322; break; // &suphs -> &suphso
				case 8385: state = 8387; break; // &swarr -> &swarro
				case 8400: state = 8604; break; // &T -> &To
				case 8404: state = 8590; break; // &t -> &to
				case 8421: state = 8422; break; // &Tcar -> &Tcaro
				case 8427: state = 8428; break; // &tcar -> &tcaro
				case 8445: state = 8446; break; // &td -> &tdo
				case 8461: state = 8540; break; // &th -> &tho
				case 8471: state = 8472; break; // &Theref -> &Therefo
				case 8476: state = 8477; break; // &theref -> &therefo
				case 8499: state = 8500; break; // &thickappr -> &thickappro
				case 8596: state = 8597; break; // &topb -> &topbo
				case 8608: state = 8610; break; // &topf -> &topfo
				case 8640: state = 8641; break; // &triangled -> &triangledo
				case 8664: state = 8665; break; // &trid -> &trido
				case 8681: state = 8682; break; // &TripleD -> &TripleDo
				case 8728: state = 8729; break; // &Tstr -> &Tstro
				case 8733: state = 8734; break; // &tstr -> &tstro
				case 8737: state = 8742; break; // &tw -> &two
				case 8753: state = 8754; break; // &twoheadleftarr -> &twoheadleftarro
				case 8764: state = 8765; break; // &twoheadrightarr -> &twoheadrightarro
				case 8768: state = 8954; break; // &U -> &Uo
				case 8775: state = 8959; break; // &u -> &uo
				case 8783: state = 8792; break; // &Uarr -> &Uarro
				case 8888: state = 8889; break; // &ulc -> &ulco
				case 8896: state = 8897; break; // &ulcr -> &ulcro
				case 8945: state = 8946; break; // &Uni -> &Unio
				case 8955: state = 8956; break; // &Uog -> &Uogo
				case 8960: state = 8961; break; // &uog -> &uogo
				case 8973: state = 8974; break; // &UpArr -> &UpArro
				case 8979: state = 8980; break; // &Uparr -> &Uparro
				case 8986: state = 8987; break; // &uparr -> &uparro
				case 8994: state = 8995; break; // &UpArrowD -> &UpArrowDo
				case 9000: state = 9001; break; // &UpArrowDownArr -> &UpArrowDownArro
				case 9004: state = 9005; break; // &UpD -> &UpDo
				case 9010: state = 9011; break; // &UpDownArr -> &UpDownArro
				case 9014: state = 9015; break; // &Upd -> &Updo
				case 9020: state = 9021; break; // &Updownarr -> &Updownarro
				case 9024: state = 9025; break; // &upd -> &updo
				case 9030: state = 9031; break; // &updownarr -> &updownarro
				case 9049: state = 9050; break; // &upharp -> &upharpo
				case 9050: state = 9051; break; // &upharpo -> &upharpoo
				case 9077: state = 9078; break; // &UpperLeftArr -> &UpperLeftArro
				case 9088: state = 9089; break; // &UpperRightArr -> &UpperRightArro
				case 9100: state = 9101; break; // &Upsil -> &Upsilo
				case 9104: state = 9105; break; // &upsil -> &upsilo
				case 9114: state = 9115; break; // &UpTeeArr -> &UpTeeArro
				case 9122: state = 9123; break; // &upuparr -> &upuparro
				case 9128: state = 9129; break; // &urc -> &urco
				case 9136: state = 9137; break; // &urcr -> &urcro
				case 9162: state = 9163; break; // &utd -> &utdo
				case 9201: state = 9436; break; // &v -> &vo
				case 9213: state = 9214; break; // &varepsil -> &varepsilo
				case 9223: state = 9224; break; // &varn -> &varno
				case 9237: state = 9238; break; // &varpr -> &varpro
				case 9240: state = 9241; break; // &varpropt -> &varpropto
				case 9249: state = 9250; break; // &varrh -> &varrho
				case 9303: state = 9432; break; // &V -> &Vo
				case 9393: state = 9394; break; // &VerticalSeparat -> &VerticalSeparato
				case 9441: state = 9442; break; // &vpr -> &vpro
				case 9484: state = 9523; break; // &W -> &Wo
				case 9490: state = 9527; break; // &w -> &wo
				case 9548: state = 9602; break; // &x -> &xo
				case 9565: state = 9607; break; // &X -> &Xo
				case 9603: state = 9604; break; // &xod -> &xodo
				case 9665: state = 9716; break; // &Y -> &Yo
				case 9672: state = 9720; break; // &y -> &yo
				case 9747: state = 9832; break; // &Z -> &Zo
				case 9754: state = 9836; break; // &z -> &zo
				case 9763: state = 9764; break; // &Zcar -> &Zcaro
				case 9769: state = 9770; break; // &zcar -> &zcaro
				case 9777: state = 9778; break; // &Zd -> &Zdo
				case 9781: state = 9782; break; // &zd -> &zdo
				case 9792: state = 9793; break; // &Zer -> &Zero
				default: return false;
				}
				break;
			case 'p':
				switch (state) {
				case 0: state = 6463; break; // & -> &p
				case 1: state = 216; break; // &A -> &Ap
				case 8: state = 199; break; // &a -> &ap
				case 79: state = 94; break; // &al -> &alp
				case 80: state = 86; break; // &ale -> &alep
				case 89: state = 90; break; // &Al -> &Alp
				case 103: state = 114; break; // &am -> &amp
				case 130: state = 131; break; // &andslo -> &andslop
				case 172: state = 173; break; // &angs -> &angsp
				case 183: state = 193; break; // &Ao -> &Aop
				case 188: state = 196; break; // &ao -> &aop
				case 199: state = 229; break; // &ap -> &app
				case 216: state = 217; break; // &Ap -> &App
				case 263: state = 264; break; // &asym -> &asymp
				case 301: state = 719; break; // &b -> &bp
				case 304: state = 318; break; // &back -> &backp
				case 310: state = 311; break; // &backe -> &backep
				case 384: state = 405; break; // &be -> &bep
				case 399: state = 400; break; // &bem -> &bemp
				case 445: state = 446; break; // &bigca -> &bigcap
				case 452: state = 453; break; // &bigcu -> &bigcup
				case 455: state = 460; break; // &bigo -> &bigop
				case 474: state = 475; break; // &bigsqcu -> &bigsqcup
				case 494: state = 495; break; // &bigtriangleu -> &bigtriangleup
				case 497: state = 498; break; // &bigu -> &bigup
				case 594: state = 595; break; // &Bo -> &Bop
				case 598: state = 599; break; // &bo -> &bop
				case 613: state = 662; break; // &box -> &boxp
				case 774: state = 775; break; // &bum -> &bump
				case 782: state = 783; break; // &Bum -> &Bump
				case 790: state = 803; break; // &Ca -> &Cap
				case 797: state = 805; break; // &ca -> &cap
				case 814: state = 815; break; // &capbrcu -> &capbrcup
				case 818: state = 819; break; // &capca -> &capcap
				case 821: state = 822; break; // &capcu -> &capcup
				case 862: state = 863; break; // &cca -> &ccap
				case 900: state = 901; break; // &ccu -> &ccup
				case 927: state = 928; break; // &cem -> &cemp
				case 1126: state = 1200; break; // &Co -> &Cop
				case 1131: state = 1203; break; // &co -> &cop
				case 1142: state = 1148; break; // &com -> &comp
				case 1278: state = 1283; break; // &csu -> &csup
				case 1292: state = 1318; break; // &cu -> &cup
				case 1301: state = 1302; break; // &cue -> &cuep
				case 1311: state = 1313; break; // &cularr -> &cularrp
				case 1315: state = 1316; break; // &Cu -> &Cup
				case 1323: state = 1324; break; // &cupbrca -> &cupbrcap
				case 1327: state = 1328; break; // &CupCa -> &CupCap
				case 1331: state = 1332; break; // &cupca -> &cupcap
				case 1334: state = 1335; break; // &cupcu -> &cupcup
				case 1356: state = 1357; break; // &curlyeq -> &curlyeqp
				case 1529: state = 1530; break; // &dem -> &demp
				case 1676: state = 1677; break; // &dlcro -> &dlcrop
				case 1679: state = 1689; break; // &do -> &dop
				case 1685: state = 1686; break; // &Do -> &Dop
				case 1694: state = 1719; break; // &dot -> &dotp
				case 1851: state = 1852; break; // &DoubleU -> &DoubleUp
				case 1907: state = 1908; break; // &DownArrowU -> &DownArrowUp
				case 1934: state = 1935; break; // &downhar -> &downharp
				case 2037: state = 2038; break; // &drcro -> &drcrop
				case 2108: state = 2326; break; // &E -> &Ep
				case 2115: state = 2312; break; // &e -> &ep
				case 2228: state = 2246; break; // &Em -> &Emp
				case 2233: state = 2238; break; // &em -> &emp
				case 2279: state = 2280; break; // &ems -> &emsp
				case 2293: state = 2294; break; // &ens -> &ensp
				case 2296: state = 2306; break; // &Eo -> &Eop
				case 2301: state = 2309; break; // &eo -> &eop
				case 2402: state = 2403; break; // &eqv -> &eqvp
				case 2458: state = 2472; break; // &ex -> &exp
				case 2466: state = 2482; break; // &Ex -> &Exp
				case 2503: state = 2639; break; // &f -> &fp
				case 2608: state = 2609; break; // &Fo -> &Fop
				case 2612: state = 2613; break; // &fo -> &fop
				case 2702: state = 2722; break; // &ga -> &gap
				case 2833: state = 2834; break; // &gna -> &gnap
				case 2834: state = 2836; break; // &gnap -> &gnapp
				case 2853: state = 2854; break; // &Go -> &Gop
				case 2857: state = 2858; break; // &go -> &gop
				case 2966: state = 2967; break; // &gtra -> &gtrap
				case 2967: state = 2968; break; // &gtrap -> &gtrapp
				case 3024: state = 3025; break; // &hairs -> &hairsp
				case 3086: state = 3087; break; // &helli -> &hellip
				case 3106: state = 3107; break; // &HilbertS -> &HilbertSp
				case 3126: state = 3163; break; // &ho -> &hop
				case 3159: state = 3160; break; // &Ho -> &Hop
				case 3208: state = 3209; break; // &Hum -> &Hump
				case 3216: state = 3217; break; // &HumpDownHum -> &HumpDownHump
				case 3225: state = 3231; break; // &hy -> &hyp
				case 3243: state = 3492; break; // &i -> &ip
				case 3330: state = 3372; break; // &Im -> &Imp
				case 3336: state = 3368; break; // &im -> &imp
				case 3341: state = 3357; break; // &imag -> &imagp
				case 3401: state = 3439; break; // &int -> &intp
				case 3467: state = 3483; break; // &io -> &iop
				case 3471: state = 3480; break; // &Io -> &Iop
				case 3582: state = 3583; break; // &Jo -> &Jop
				case 3586: state = 3587; break; // &jo -> &jop
				case 3619: state = 3620; break; // &Ka -> &Kap
				case 3620: state = 3621; break; // &Kap -> &Kapp
				case 3625: state = 3626; break; // &ka -> &kap
				case 3626: state = 3627; break; // &kap -> &kapp
				case 3676: state = 3677; break; // &Ko -> &Kop
				case 3680: state = 3681; break; // &ko -> &kop
				case 3692: state = 4621; break; // &l -> &lp
				case 3699: state = 3746; break; // &La -> &Lap
				case 3705: state = 3744; break; // &la -> &lap
				case 3712: state = 3713; break; // &laem -> &laemp
				case 3766: state = 3782; break; // &larr -> &larrp
				case 3779: state = 3780; break; // &larrl -> &larrlp
				case 4006: state = 4007; break; // &lefthar -> &leftharp
				case 4016: state = 4017; break; // &leftharpoonu -> &leftharpoonup
				case 4067: state = 4068; break; // &leftrighthar -> &leftrightharp
				case 4138: state = 4139; break; // &LeftU -> &LeftUp
				case 4216: state = 4217; break; // &lessa -> &lessap
				case 4217: state = 4218; break; // &lessap -> &lessapp
				case 4402: state = 4403; break; // &lna -> &lnap
				case 4403: state = 4405; break; // &lnap -> &lnapp
				case 4422: state = 4560; break; // &lo -> &lop
				case 4434: state = 4564; break; // &Lo -> &Lop
				case 4503: state = 4504; break; // &longma -> &longmap
				case 4542: state = 4543; break; // &loo -> &loop
				case 4767: state = 4935; break; // &m -> &mp
				case 4768: state = 4785; break; // &ma -> &map
				case 4782: state = 4783; break; // &Ma -> &Map
				case 4801: state = 4802; break; // &mapstou -> &mapstoup
				case 4848: state = 4849; break; // &MediumS -> &MediumSp
				case 4910: state = 4911; break; // &mlc -> &mlcp
				case 4916: state = 4917; break; // &mn -> &mnp
				case 4922: state = 4932; break; // &mo -> &mop
				case 4928: state = 4929; break; // &Mo -> &Mop
				case 4945: state = 4946; break; // &mst -> &mstp
				case 4958: state = 4959; break; // &multima -> &multimap
				case 4962: state = 4963; break; // &muma -> &mumap
				case 4965: state = 5821; break; // &n -> &np
				case 4966: state = 4986; break; // &na -> &nap
				case 4986: state = 4996; break; // &nap -> &napp
				case 5011: state = 5012; break; // &nbs -> &nbsp
				case 5015: state = 5016; break; // &nbum -> &nbump
				case 5021: state = 5022; break; // &nca -> &ncap
				case 5052: state = 5053; break; // &ncu -> &ncup
				case 5097: state = 5098; break; // &NegativeMediumS -> &NegativeMediumSp
				case 5108: state = 5109; break; // &NegativeThickS -> &NegativeThickSp
				case 5115: state = 5116; break; // &NegativeThinS -> &NegativeThinSp
				case 5129: state = 5130; break; // &NegativeVeryThinS -> &NegativeVeryThinSp
				case 5227: state = 5236; break; // &nh -> &nhp
				case 5347: state = 5369; break; // &No -> &Nop
				case 5363: state = 5364; break; // &NonBreakingS -> &NonBreakingSp
				case 5372: state = 5373; break; // &no -> &nop
				case 5390: state = 5391; break; // &NotCu -> &NotCup
				case 5393: state = 5394; break; // &NotCupCa -> &NotCupCap
				case 5495: state = 5496; break; // &NotHum -> &NotHump
				case 5503: state = 5504; break; // &NotHumpDownHum -> &NotHumpDownHump
				case 5701: state = 5713; break; // &NotSquareSu -> &NotSquareSup
				case 5726: state = 5768; break; // &NotSu -> &NotSup
				case 5895: state = 5938; break; // &ns -> &nsp
				case 5913: state = 5918; break; // &nshort -> &nshortp
				case 5944: state = 5948; break; // &nsqsu -> &nsqsup
				case 5951: state = 5973; break; // &nsu -> &nsup
				case 6040: state = 6041; break; // &nums -> &numsp
				case 6044: state = 6045; break; // &nva -> &nvap
				case 6131: state = 6306; break; // &O -> &Op
				case 6138: state = 6302; break; // &o -> &op
				case 6294: state = 6295; break; // &Oo -> &Oop
				case 6298: state = 6299; break; // &oo -> &oop
				case 6333: state = 6334; break; // &oper -> &operp
				case 6370: state = 6371; break; // &orslo -> &orslop
				case 6498: state = 6511; break; // &per -> &perp
				case 6609: state = 6630; break; // &Po -> &Pop
				case 6615: state = 6616; break; // &Poincare -> &Poincarep
				case 6622: state = 6633; break; // &po -> &pop
				case 6644: state = 6645; break; // &pra -> &prap
				case 6657: state = 6658; break; // &preca -> &precap
				case 6658: state = 6659; break; // &precap -> &precapp
				case 6706: state = 6707; break; // &precna -> &precnap
				case 6707: state = 6708; break; // &precnap -> &precnapp
				case 6736: state = 6737; break; // &prna -> &prnap
				case 6745: state = 6770; break; // &pro -> &prop
				case 6748: state = 6772; break; // &Pro -> &Prop
				case 6810: state = 6811; break; // &puncs -> &puncsp
				case 6817: state = 6833; break; // &q -> &qp
				case 6825: state = 6826; break; // &Qo -> &Qop
				case 6829: state = 6830; break; // &qo -> &qop
				case 6876: state = 7512; break; // &r -> &rp
				case 6902: state = 6903; break; // &raem -> &raemp
				case 6932: state = 6953; break; // &rarr -> &rarrp
				case 6934: state = 6935; break; // &rarra -> &rarrap
				case 6950: state = 6951; break; // &rarrl -> &rarrlp
				case 7076: state = 7082; break; // &real -> &realp
				case 7121: state = 7122; break; // &ReverseU -> &ReverseUp
				case 7281: state = 7282; break; // &righthar -> &rightharp
				case 7291: state = 7292; break; // &rightharpoonu -> &rightharpoonup
				case 7307: state = 7308; break; // &rightlefthar -> &rightleftharp
				case 7383: state = 7384; break; // &RightU -> &RightUp
				case 7469: state = 7481; break; // &ro -> &rop
				case 7485: state = 7486; break; // &Ro -> &Rop
				case 7505: state = 7506; break; // &RoundIm -> &RoundImp
				case 7512: state = 7519; break; // &rp -> &rpp
				case 7617: state = 7956; break; // &s -> &sp
				case 7631: state = 7680; break; // &sc -> &scp
				case 7633: state = 7634; break; // &sca -> &scap
				case 7671: state = 7672; break; // &scna -> &scnap
				case 7753: state = 7754; break; // &shar -> &sharp
				case 7798: state = 7803; break; // &short -> &shortp
				case 7823: state = 7824; break; // &ShortU -> &ShortUp
				case 7847: state = 7868; break; // &sim -> &simp
				case 7908: state = 7909; break; // &smash -> &smashp
				case 7911: state = 7912; break; // &sme -> &smep
				case 7936: state = 7953; break; // &so -> &sop
				case 7949: state = 7950; break; // &So -> &Sop
				case 7970: state = 7971; break; // &sqca -> &sqcap
				case 7975: state = 7976; break; // &sqcu -> &sqcup
				case 7985: state = 7997; break; // &sqsu -> &sqsup
				case 8033: state = 8045; break; // &SquareSu -> &SquareSup
				case 8111: state = 8120; break; // &straight -> &straightp
				case 8112: state = 8113; break; // &straighte -> &straightep
				case 8127: state = 8282; break; // &Su -> &Sup
				case 8130: state = 8284; break; // &su -> &sup
				case 8131: state = 8155; break; // &sub -> &subp
				case 8193: state = 8196; break; // &subsu -> &subsup
				case 8201: state = 8202; break; // &succa -> &succap
				case 8202: state = 8203; break; // &succap -> &succapp
				case 8250: state = 8251; break; // &succna -> &succnap
				case 8251: state = 8252; break; // &succnap -> &succnapp
				case 8284: state = 8343; break; // &sup -> &supp
				case 8370: state = 8373; break; // &supsu -> &supsup
				case 8404: state = 8617; break; // &t -> &tp
				case 8496: state = 8497; break; // &thicka -> &thickap
				case 8497: state = 8498; break; // &thickap -> &thickapp
				case 8510: state = 8511; break; // &ThickS -> &ThickSp
				case 8517: state = 8518; break; // &thins -> &thinsp
				case 8521: state = 8522; break; // &ThinS -> &ThinSp
				case 8528: state = 8529; break; // &thka -> &thkap
				case 8590: state = 8594; break; // &to -> &top
				case 8604: state = 8605; break; // &To -> &Top
				case 8628: state = 8698; break; // &tr -> &trp
				case 8633: state = 8685; break; // &tri -> &trip
				case 8677: state = 8678; break; // &Tri -> &Trip
				case 8768: state = 8970; break; // &U -> &Up
				case 8775: state = 8983; break; // &u -> &up
				case 8897: state = 8898; break; // &ulcro -> &ulcrop
				case 8954: state = 8964; break; // &Uo -> &Uop
				case 8959: state = 8967; break; // &uo -> &uop
				case 8970: state = 9068; break; // &Up -> &Upp
				case 9048: state = 9049; break; // &uphar -> &upharp
				case 9118: state = 9119; break; // &upu -> &upup
				case 9137: state = 9138; break; // &urcro -> &urcrop
				case 9201: state = 9440; break; // &v -> &vp
				case 9208: state = 9231; break; // &var -> &varp
				case 9209: state = 9210; break; // &vare -> &varep
				case 9218: state = 9219; break; // &varka -> &varkap
				case 9219: state = 9220; break; // &varkap -> &varkapp
				case 9238: state = 9239; break; // &varpro -> &varprop
				case 9258: state = 9269; break; // &varsu -> &varsup
				case 9357: state = 9358; break; // &velli -> &vellip
				case 9388: state = 9389; break; // &VerticalSe -> &VerticalSep
				case 9408: state = 9409; break; // &VeryThinS -> &VeryThinSp
				case 9427: state = 9430; break; // &vnsu -> &vnsup
				case 9432: state = 9433; break; // &Vo -> &Vop
				case 9436: state = 9437; break; // &vo -> &vop
				case 9442: state = 9443; break; // &vpro -> &vprop
				case 9458: state = 9465; break; // &vsu -> &vsup
				case 9490: state = 9531; break; // &w -> &wp
				case 9514: state = 9515; break; // &weier -> &weierp
				case 9523: state = 9524; break; // &Wo -> &Wop
				case 9527: state = 9528; break; // &wo -> &wop
				case 9550: state = 9551; break; // &xca -> &xcap
				case 9557: state = 9558; break; // &xcu -> &xcup
				case 9595: state = 9596; break; // &xma -> &xmap
				case 9602: state = 9611; break; // &xo -> &xop
				case 9607: state = 9608; break; // &Xo -> &Xop
				case 9642: state = 9643; break; // &xsqcu -> &xsqcup
				case 9645: state = 9646; break; // &xu -> &xup
				case 9716: state = 9717; break; // &Yo -> &Yop
				case 9720: state = 9721; break; // &yo -> &yop
				case 9799: state = 9800; break; // &ZeroWidthS -> &ZeroWidthSp
				case 9832: state = 9833; break; // &Zo -> &Zop
				case 9836: state = 9837; break; // &zo -> &zop
				default: return false;
				}
				break;
			case 'q':
				switch (state) {
				case 0: state = 6817; break; // & -> &q
				case 234: state = 235; break; // &approxe -> &approxeq
				case 266: state = 267; break; // &asympe -> &asympeq
				case 328: state = 329; break; // &backsime -> &backsimeq
				case 379: state = 380; break; // &bd -> &bdq
				case 471: state = 472; break; // &bigs -> &bigsq
				case 531: state = 532; break; // &blacks -> &blacksq
				case 580: state = 582; break; // &bne -> &bneq
				case 779: state = 787; break; // &bumpe -> &bumpeq
				case 784: state = 785; break; // &Bumpe -> &Bumpeq
				case 983: state = 984; break; // &circe -> &circeq
				case 1138: state = 1140; break; // &colone -> &coloneq
				case 1355: state = 1356; break; // &curlye -> &curlyeq
				case 1513: state = 1514; break; // &ddotse -> &ddotseq
				case 1700: state = 1701; break; // &dote -> &doteq
				case 1707: state = 1708; break; // &DotE -> &DotEq
				case 1724: state = 1725; break; // &dots -> &dotsq
				case 2108: state = 2367; break; // &E -> &Eq
				case 2115: state = 2339; break; // &e -> &eq
				case 2254: state = 2255; break; // &EmptySmallS -> &EmptySmallSq
				case 2272: state = 2273; break; // &EmptyVerySmallS -> &EmptyVerySmallSq
				case 2514: state = 2515; break; // &fallingdotse -> &fallingdotseq
				case 2564: state = 2565; break; // &FilledSmallS -> &FilledSmallSq
				case 2580: state = 2581; break; // &FilledVerySmallS -> &FilledVerySmallSq
				case 2765: state = 2771; break; // &ge -> &geq
				case 2771: state = 2773; break; // &geq -> &geqq
				case 2843: state = 2845; break; // &gne -> &gneq
				case 2845: state = 2847; break; // &gneq -> &gneqq
				case 2872: state = 2873; break; // &GreaterE -> &GreaterEq
				case 2887: state = 2888; break; // &GreaterFullE -> &GreaterFullEq
				case 2911: state = 2912; break; // &GreaterSlantE -> &GreaterSlantEq
				case 2942: state = 2959; break; // &gt -> &gtq
				case 2980: state = 2981; break; // &gtre -> &gtreq
				case 2981: state = 2987; break; // &gtreq -> &gtreqq
				case 3007: state = 3008; break; // &gvertne -> &gvertneq
				case 3008: state = 3009; break; // &gvertneq -> &gvertneqq
				case 3219: state = 3220; break; // &HumpE -> &HumpEq
				case 3243: state = 3497; break; // &i -> &iq
				case 3705: state = 3755; break; // &la -> &laq
				case 3869: state = 3873; break; // &ld -> &ldq
				case 3896: state = 4187; break; // &le -> &leq
				case 4074: state = 4075; break; // &leftrights -> &leftrightsq
				case 4132: state = 4133; break; // &LeftTriangleE -> &LeftTriangleEq
				case 4187: state = 4189; break; // &leq -> &leqq
				case 4227: state = 4228; break; // &lesse -> &lesseq
				case 4228: state = 4233; break; // &lesseq -> &lesseqq
				case 4240: state = 4241; break; // &LessE -> &LessEq
				case 4257: state = 4258; break; // &LessFullE -> &LessFullEq
				case 4289: state = 4290; break; // &LessSlantE -> &LessSlantEq
				case 4412: state = 4414; break; // &lne -> &lneq
				case 4414: state = 4416; break; // &lneq -> &lneqq
				case 4652: state = 4676; break; // &ls -> &lsq
				case 4653: state = 4654; break; // &lsa -> &lsaq
				case 4698: state = 4725; break; // &lt -> &ltq
				case 4760: state = 4761; break; // &lvertne -> &lvertneq
				case 4761: state = 4762; break; // &lvertneq -> &lvertneqq
				case 5064: state = 5135; break; // &ne -> &neq
				case 5198: state = 5200; break; // &nge -> &ngeq
				case 5200: state = 5202; break; // &ngeq -> &ngeqq
				case 5270: state = 5312; break; // &nle -> &nleq
				case 5312: state = 5314; break; // &nleq -> &nleqq
				case 5414: state = 5422; break; // &NotE -> &NotEq
				case 5447: state = 5448; break; // &NotGreaterE -> &NotGreaterEq
				case 5457: state = 5458; break; // &NotGreaterFullE -> &NotGreaterFullEq
				case 5481: state = 5482; break; // &NotGreaterSlantE -> &NotGreaterSlantEq
				case 5506: state = 5507; break; // &NotHumpE -> &NotHumpEq
				case 5545: state = 5546; break; // &NotLeftTriangleE -> &NotLeftTriangleEq
				case 5554: state = 5555; break; // &NotLessE -> &NotLessEq
				case 5578: state = 5579; break; // &NotLessSlantE -> &NotLessSlantEq
				case 5639: state = 5640; break; // &NotPrecedesE -> &NotPrecedesEq
				case 5650: state = 5651; break; // &NotPrecedesSlantE -> &NotPrecedesSlantEq
				case 5688: state = 5689; break; // &NotRightTriangleE -> &NotRightTriangleEq
				case 5694: state = 5695; break; // &NotS -> &NotSq
				case 5707: state = 5708; break; // &NotSquareSubsetE -> &NotSquareSubsetEq
				case 5720: state = 5721; break; // &NotSquareSupersetE -> &NotSquareSupersetEq
				case 5732: state = 5733; break; // &NotSubsetE -> &NotSubsetEq
				case 5745: state = 5746; break; // &NotSucceedsE -> &NotSucceedsEq
				case 5756: state = 5757; break; // &NotSucceedsSlantE -> &NotSucceedsSlantEq
				case 5775: state = 5776; break; // &NotSupersetE -> &NotSupersetEq
				case 5787: state = 5788; break; // &NotTildeE -> &NotTildeEq
				case 5797: state = 5798; break; // &NotTildeFullE -> &NotTildeFullEq
				case 5852: state = 5853; break; // &nprece -> &npreceq
				case 5895: state = 5942; break; // &ns -> &nsq
				case 5930: state = 5932; break; // &nsime -> &nsimeq
				case 5962: state = 5963; break; // &nsubsete -> &nsubseteq
				case 5963: state = 5965; break; // &nsubseteq -> &nsubseteqq
				case 5970: state = 5971; break; // &nsucce -> &nsucceq
				case 5983: state = 5984; break; // &nsupsete -> &nsupseteq
				case 5984: state = 5986; break; // &nsupseteq -> &nsupseteqq
				case 6018: state = 6019; break; // &ntrianglelefte -> &ntrianglelefteq
				case 6027: state = 6028; break; // &ntrianglerighte -> &ntrianglerighteq
				case 6669: state = 6670; break; // &preccurlye -> &preccurlyeq
				case 6679: state = 6680; break; // &PrecedesE -> &PrecedesEq
				case 6690: state = 6691; break; // &PrecedesSlantE -> &PrecedesSlantEq
				case 6702: state = 6703; break; // &prece -> &preceq
				case 6713: state = 6714; break; // &precne -> &precneq
				case 6714: state = 6715; break; // &precneq -> &precneqq
				case 6866: state = 6867; break; // &queste -> &questeq
				case 6882: state = 6921; break; // &ra -> &raq
				case 7053: state = 7063; break; // &rd -> &rdq
				case 7102: state = 7110; break; // &ReverseE -> &ReverseEq
				case 7123: state = 7124; break; // &ReverseUpE -> &ReverseUpEq
				case 7326: state = 7327; break; // &rights -> &rightsq
				case 7377: state = 7378; break; // &RightTriangleE -> &RightTriangleEq
				case 7439: state = 7440; break; // &risingdotse -> &risingdotseq
				case 7542: state = 7559; break; // &rs -> &rsq
				case 7543: state = 7544; break; // &rsa -> &rsaq
				case 7610: state = 7980; break; // &S -> &Sq
				case 7617: state = 7968; break; // &s -> &sq
				case 7624: state = 7625; break; // &sb -> &sbq
				case 7853: state = 7855; break; // &sime -> &simeq
				case 7994: state = 7995; break; // &sqsubsete -> &sqsubseteq
				case 8005: state = 8006; break; // &sqsupsete -> &sqsupseteq
				case 8039: state = 8040; break; // &SquareSubsetE -> &SquareSubsetEq
				case 8052: state = 8053; break; // &SquareSupersetE -> &SquareSupersetEq
				case 8173: state = 8174; break; // &subsete -> &subseteq
				case 8174: state = 8176; break; // &subseteq -> &subseteqq
				case 8178: state = 8179; break; // &SubsetE -> &SubsetEq
				case 8185: state = 8186; break; // &subsetne -> &subsetneq
				case 8186: state = 8188; break; // &subsetneq -> &subsetneqq
				case 8213: state = 8214; break; // &succcurlye -> &succcurlyeq
				case 8223: state = 8224; break; // &SucceedsE -> &SucceedsEq
				case 8234: state = 8235; break; // &SucceedsSlantE -> &SucceedsSlantEq
				case 8246: state = 8247; break; // &succe -> &succeq
				case 8257: state = 8258; break; // &succne -> &succneq
				case 8258: state = 8259; break; // &succneq -> &succneqq
				case 8314: state = 8315; break; // &SupersetE -> &SupersetEq
				case 8356: state = 8357; break; // &supsete -> &supseteq
				case 8357: state = 8359; break; // &supseteq -> &supseteqq
				case 8362: state = 8363; break; // &supsetne -> &supsetneq
				case 8363: state = 8365; break; // &supsetneq -> &supsetneqq
				case 8554: state = 8555; break; // &TildeE -> &TildeEq
				case 8564: state = 8565; break; // &TildeFullE -> &TildeFullEq
				case 8638: state = 8653; break; // &triangle -> &triangleq
				case 8650: state = 8651; break; // &trianglelefte -> &trianglelefteq
				case 8661: state = 8662; break; // &trianglerighte -> &trianglerighteq
				case 9034: state = 9035; break; // &UpE -> &UpEq
				case 9264: state = 9265; break; // &varsubsetne -> &varsubsetneq
				case 9265: state = 9267; break; // &varsubsetneq -> &varsubsetneqq
				case 9274: state = 9275; break; // &varsupsetne -> &varsupsetneq
				case 9275: state = 9277; break; // &varsupsetneq -> &varsupsetneqq
				case 9352: state = 9353; break; // &veee -> &veeeq
				case 9508: state = 9510; break; // &wedge -> &wedgeq
				case 9636: state = 9640; break; // &xs -> &xsq
				default: return false;
				}
				break;
			case 'r':
				switch (state) {
				case 0: state = 6876; break; // & -> &r
				case 1: state = 237; break; // &A -> &Ar
				case 8: state = 242; break; // &a -> &ar
				case 15: state = 16; break; // &Ab -> &Abr
				case 21: state = 22; break; // &ab -> &abr
				case 34: state = 35; break; // &Aci -> &Acir
				case 38: state = 39; break; // &aci -> &acir
				case 60: state = 65; break; // &af -> &afr
				case 62: state = 63; break; // &Af -> &Afr
				case 67: state = 68; break; // &Ag -> &Agr
				case 73: state = 74; break; // &ag -> &agr
				case 100: state = 101; break; // &Amac -> &Amacr
				case 105: state = 106; break; // &amac -> &amacr
				case 136: state = 164; break; // &ang -> &angr
				case 179: state = 180; break; // &angza -> &angzar
				case 180: state = 181; break; // &angzar -> &angzarr
				case 203: state = 204; break; // &apaci -> &apacir
				case 229: state = 230; break; // &app -> &appr
				case 248: state = 249; break; // &Asc -> &Ascr
				case 252: state = 253; break; // &asc -> &ascr
				case 301: state = 730; break; // &b -> &br
				case 302: state = 344; break; // &ba -> &bar
				case 318: state = 319; break; // &backp -> &backpr
				case 331: state = 725; break; // &B -> &Br
				case 332: state = 341; break; // &Ba -> &Bar
				case 360: state = 361; break; // &bb -> &bbr
				case 365: state = 366; break; // &bbrktb -> &bbrktbr
				case 384: state = 409; break; // &be -> &ber
				case 390: state = 414; break; // &Be -> &Ber
				case 436: state = 437; break; // &Bf -> &Bfr
				case 439: state = 440; break; // &bf -> &bfr
				case 448: state = 449; break; // &bigci -> &bigcir
				case 478: state = 479; break; // &bigsta -> &bigstar
				case 481: state = 482; break; // &bigt -> &bigtr
				case 514: state = 515; break; // &bka -> &bkar
				case 534: state = 535; break; // &blacksqua -> &blacksquar
				case 538: state = 539; break; // &blackt -> &blacktr
				case 545: state = 557; break; // &blacktriangle -> &blacktriangler
				case 618: state = 630; break; // &boxD -> &boxDr
				case 623: state = 634; break; // &boxd -> &boxdr
				case 673: state = 685; break; // &boxU -> &boxUr
				case 678: state = 689; break; // &boxu -> &boxur
				case 691: state = 713; break; // &boxV -> &boxVr
				case 693: state = 717; break; // &boxv -> &boxvr
				case 719: state = 720; break; // &bp -> &bpr
				case 737: state = 738; break; // &brvba -> &brvbar
				case 741: state = 742; break; // &Bsc -> &Bscr
				case 745: state = 746; break; // &bsc -> &bscr
				case 789: state = 1261; break; // &C -> &Cr
				case 796: state = 1256; break; // &c -> &cr
				case 797: state = 848; break; // &ca -> &car
				case 811: state = 812; break; // &capb -> &capbr
				case 836: state = 837; break; // &CapitalDiffe -> &CapitalDiffer
				case 862: state = 872; break; // &cca -> &ccar
				case 867: state = 868; break; // &Cca -> &Ccar
				case 886: state = 887; break; // &Cci -> &Ccir
				case 890: state = 891; break; // &cci -> &ccir
				case 938: state = 939; break; // &Cente -> &Center
				case 944: state = 945; break; // &cente -> &center
				case 950: state = 951; break; // &Cf -> &Cfr
				case 953: state = 954; break; // &cf -> &cfr
				case 969: state = 970; break; // &checkma -> &checkmar
				case 978: state = 979; break; // &ci -> &cir
				case 988: state = 989; break; // &circlea -> &circlear
				case 989: state = 990; break; // &circlear -> &circlearr
				case 992: state = 998; break; // &circlearrow -> &circlearrowr
				case 1010: state = 1011; break; // &circledci -> &circledcir
				case 1019: state = 1020; break; // &Ci -> &Cir
				case 1065: state = 1066; break; // &cirsci -> &cirscir
				case 1081: state = 1082; break; // &ClockwiseContou -> &ClockwiseContour
				case 1087: state = 1088; break; // &ClockwiseContourInteg -> &ClockwiseContourIntegr
				case 1095: state = 1096; break; // &CloseCu -> &CloseCur
				case 1172: state = 1173; break; // &Cong -> &Congr
				case 1189: state = 1190; break; // &Contou -> &Contour
				case 1195: state = 1196; break; // &ContourInteg -> &ContourIntegr
				case 1200: state = 1210; break; // &Cop -> &Copr
				case 1203: state = 1206; break; // &cop -> &copr
				case 1223: state = 1224; break; // &copys -> &copysr
				case 1229: state = 1230; break; // &Counte -> &Counter
				case 1245: state = 1246; break; // &CounterClockwiseContou -> &CounterClockwiseContour
				case 1251: state = 1252; break; // &CounterClockwiseContourInteg -> &CounterClockwiseContourIntegr
				case 1257: state = 1258; break; // &cra -> &crar
				case 1258: state = 1259; break; // &crar -> &crarr
				case 1271: state = 1272; break; // &Csc -> &Cscr
				case 1275: state = 1276; break; // &csc -> &cscr
				case 1292: state = 1346; break; // &cu -> &cur
				case 1294: state = 1295; break; // &cuda -> &cudar
				case 1295: state = 1296; break; // &cudar -> &cudarr
				case 1296: state = 1299; break; // &cudarr -> &cudarrr
				case 1302: state = 1303; break; // &cuep -> &cuepr
				case 1309: state = 1310; break; // &cula -> &cular
				case 1310: state = 1311; break; // &cular -> &cularr
				case 1320: state = 1321; break; // &cupb -> &cupbr
				case 1341: state = 1342; break; // &cupo -> &cupor
				case 1346: state = 1377; break; // &cur -> &curr
				case 1347: state = 1348; break; // &cura -> &curar
				case 1348: state = 1349; break; // &curar -> &curarr
				case 1357: state = 1358; break; // &curlyeqp -> &curlyeqpr
				case 1383: state = 1384; break; // &curvea -> &curvear
				case 1384: state = 1385; break; // &curvear -> &curvearr
				case 1387: state = 1393; break; // &curvearrow -> &curvearrowr
				case 1426: state = 1444; break; // &Da -> &Dar
				case 1429: state = 1430; break; // &Dagge -> &Dagger
				case 1432: state = 2023; break; // &d -> &dr
				case 1433: state = 1451; break; // &da -> &dar
				case 1436: state = 1437; break; // &dagge -> &dagger
				case 1444: state = 1445; break; // &Dar -> &Darr
				case 1447: state = 1448; break; // &dA -> &dAr
				case 1448: state = 1449; break; // &dAr -> &dArr
				case 1451: state = 1452; break; // &dar -> &darr
				case 1465: state = 1466; break; // &dbka -> &dbkar
				case 1475: state = 1476; break; // &Dca -> &Dcar
				case 1481: state = 1482; break; // &dca -> &dcar
				case 1494: state = 1500; break; // &dda -> &ddar
				case 1497: state = 1498; break; // &ddagge -> &ddagger
				case 1500: state = 1501; break; // &ddar -> &ddarr
				case 1504: state = 1505; break; // &DDot -> &DDotr
				case 1535: state = 1544; break; // &df -> &dfr
				case 1541: state = 1542; break; // &Df -> &Dfr
				case 1547: state = 1548; break; // &dHa -> &dHar
				case 1551: state = 1552; break; // &dha -> &dhar
				case 1552: state = 1555; break; // &dhar -> &dharr
				case 1559: state = 1560; break; // &Diac -> &Diacr
				case 1587: state = 1588; break; // &DiacriticalG -> &DiacriticalGr
				case 1623: state = 1624; break; // &Diffe -> &Differ
				case 1670: state = 1675; break; // &dlc -> &dlcr
				case 1671: state = 1672; break; // &dlco -> &dlcor
				case 1682: state = 1683; break; // &dolla -> &dollar
				case 1727: state = 1728; break; // &dotsqua -> &dotsquar
				case 1736: state = 1737; break; // &doubleba -> &doublebar
				case 1753: state = 1754; break; // &DoubleContou -> &DoubleContour
				case 1759: state = 1760; break; // &DoubleContourInteg -> &DoubleContourIntegr
				case 1770: state = 1771; break; // &DoubleDownA -> &DoubleDownAr
				case 1771: state = 1772; break; // &DoubleDownAr -> &DoubleDownArr
				case 1780: state = 1781; break; // &DoubleLeftA -> &DoubleLeftAr
				case 1781: state = 1782; break; // &DoubleLeftAr -> &DoubleLeftArr
				case 1791: state = 1792; break; // &DoubleLeftRightA -> &DoubleLeftRightAr
				case 1792: state = 1793; break; // &DoubleLeftRightAr -> &DoubleLeftRightArr
				case 1808: state = 1809; break; // &DoubleLongLeftA -> &DoubleLongLeftAr
				case 1809: state = 1810; break; // &DoubleLongLeftAr -> &DoubleLongLeftArr
				case 1819: state = 1820; break; // &DoubleLongLeftRightA -> &DoubleLongLeftRightAr
				case 1820: state = 1821; break; // &DoubleLongLeftRightAr -> &DoubleLongLeftRightArr
				case 1830: state = 1831; break; // &DoubleLongRightA -> &DoubleLongRightAr
				case 1831: state = 1832; break; // &DoubleLongRightAr -> &DoubleLongRightArr
				case 1841: state = 1842; break; // &DoubleRightA -> &DoubleRightAr
				case 1842: state = 1843; break; // &DoubleRightAr -> &DoubleRightArr
				case 1853: state = 1854; break; // &DoubleUpA -> &DoubleUpAr
				case 1854: state = 1855; break; // &DoubleUpAr -> &DoubleUpArr
				case 1863: state = 1864; break; // &DoubleUpDownA -> &DoubleUpDownAr
				case 1864: state = 1865; break; // &DoubleUpDownAr -> &DoubleUpDownArr
				case 1870: state = 1871; break; // &DoubleVe -> &DoubleVer
				case 1878: state = 1879; break; // &DoubleVerticalBa -> &DoubleVerticalBar
				case 1883: state = 1884; break; // &DownA -> &DownAr
				case 1884: state = 1885; break; // &DownAr -> &DownArr
				case 1889: state = 1890; break; // &Downa -> &Downar
				case 1890: state = 1891; break; // &Downar -> &Downarr
				case 1897: state = 1898; break; // &downa -> &downar
				case 1898: state = 1899; break; // &downar -> &downarr
				case 1904: state = 1905; break; // &DownArrowBa -> &DownArrowBar
				case 1909: state = 1910; break; // &DownArrowUpA -> &DownArrowUpAr
				case 1910: state = 1911; break; // &DownArrowUpAr -> &DownArrowUpArr
				case 1915: state = 1916; break; // &DownB -> &DownBr
				case 1925: state = 1926; break; // &downdowna -> &downdownar
				case 1926: state = 1927; break; // &downdownar -> &downdownarr
				case 1933: state = 1934; break; // &downha -> &downhar
				case 1938: state = 1944; break; // &downharpoon -> &downharpoonr
				case 1963: state = 1964; break; // &DownLeftRightVecto -> &DownLeftRightVector
				case 1973: state = 1974; break; // &DownLeftTeeVecto -> &DownLeftTeeVector
				case 1980: state = 1981; break; // &DownLeftVecto -> &DownLeftVector
				case 1984: state = 1985; break; // &DownLeftVectorBa -> &DownLeftVectorBar
				case 1999: state = 2000; break; // &DownRightTeeVecto -> &DownRightTeeVector
				case 2006: state = 2007; break; // &DownRightVecto -> &DownRightVector
				case 2010: state = 2011; break; // &DownRightVectorBa -> &DownRightVectorBar
				case 2017: state = 2018; break; // &DownTeeA -> &DownTeeAr
				case 2018: state = 2019; break; // &DownTeeAr -> &DownTeeArr
				case 2026: state = 2027; break; // &drbka -> &drbkar
				case 2031: state = 2036; break; // &drc -> &drcr
				case 2032: state = 2033; break; // &drco -> &drcor
				case 2041: state = 2042; break; // &Dsc -> &Dscr
				case 2045: state = 2046; break; // &dsc -> &dscr
				case 2057: state = 2058; break; // &Dst -> &Dstr
				case 2062: state = 2063; break; // &dst -> &dstr
				case 2067: state = 2072; break; // &dt -> &dtr
				case 2078: state = 2079; break; // &dua -> &duar
				case 2079: state = 2080; break; // &duar -> &duarr
				case 2083: state = 2084; break; // &duha -> &duhar
				case 2102: state = 2103; break; // &dzig -> &dzigr
				case 2104: state = 2105; break; // &dzigra -> &dzigrar
				case 2105: state = 2106; break; // &dzigrar -> &dzigrarr
				case 2115: state = 2409; break; // &e -> &er
				case 2124: state = 2125; break; // &easte -> &easter
				case 2128: state = 2129; break; // &Eca -> &Ecar
				case 2134: state = 2135; break; // &eca -> &ecar
				case 2139: state = 2140; break; // &eci -> &ecir
				case 2142: state = 2143; break; // &Eci -> &Ecir
				case 2175: state = 2183; break; // &ef -> &efr
				case 2180: state = 2181; break; // &Ef -> &Efr
				case 2185: state = 2193; break; // &eg -> &egr
				case 2187: state = 2188; break; // &Eg -> &Egr
				case 2216: state = 2217; break; // &elinte -> &elinter
				case 2230: state = 2231; break; // &Emac -> &Emacr
				case 2235: state = 2236; break; // &emac -> &emacr
				case 2257: state = 2258; break; // &EmptySmallSqua -> &EmptySmallSquar
				case 2264: state = 2265; break; // &EmptyVe -> &EmptyVer
				case 2275: state = 2276; break; // &EmptyVerySmallSqua -> &EmptyVerySmallSquar
				case 2313: state = 2314; break; // &epa -> &epar
				case 2341: state = 2342; break; // &eqci -> &eqcir
				case 2359: state = 2360; break; // &eqslantgt -> &eqslantgtr
				case 2390: state = 2391; break; // &Equilib -> &Equilibr
				case 2404: state = 2405; break; // &eqvpa -> &eqvpar
				case 2410: state = 2411; break; // &era -> &erar
				case 2411: state = 2412; break; // &erar -> &erarr
				case 2419: state = 2420; break; // &Esc -> &Escr
				case 2423: state = 2424; break; // &esc -> &escr
				case 2451: state = 2455; break; // &eu -> &eur
				case 2503: state = 2647; break; // &f -> &fr
				case 2530: state = 2547; break; // &ff -> &ffr
				case 2544: state = 2545; break; // &Ff -> &Ffr
				case 2567: state = 2568; break; // &FilledSmallSqua -> &FilledSmallSquar
				case 2572: state = 2573; break; // &FilledVe -> &FilledVer
				case 2583: state = 2584; break; // &FilledVerySmallSqua -> &FilledVerySmallSquar
				case 2608: state = 2616; break; // &Fo -> &For
				case 2612: state = 2621; break; // &fo -> &for
				case 2630: state = 2631; break; // &Fou -> &Four
				case 2633: state = 2634; break; // &Fourie -> &Fourier
				case 2635: state = 2636; break; // &Fouriert -> &Fouriertr
				case 2640: state = 2641; break; // &fpa -> &fpar
				case 2694: state = 2695; break; // &Fsc -> &Fscr
				case 2698: state = 2699; break; // &fsc -> &fscr
				case 2701: state = 2861; break; // &g -> &gr
				case 2708: state = 2866; break; // &G -> &Gr
				case 2724: state = 2725; break; // &Gb -> &Gbr
				case 2730: state = 2731; break; // &gb -> &gbr
				case 2742: state = 2743; break; // &Gci -> &Gcir
				case 2747: state = 2748; break; // &gci -> &gcir
				case 2799: state = 2800; break; // &Gf -> &Gfr
				case 2802: state = 2803; break; // &gf -> &gfr
				case 2836: state = 2837; break; // &gnapp -> &gnappr
				case 2870: state = 2871; break; // &Greate -> &Greater
				case 2893: state = 2894; break; // &GreaterG -> &GreaterGr
				case 2898: state = 2899; break; // &GreaterGreate -> &GreaterGreater
				case 2924: state = 2925; break; // &Gsc -> &Gscr
				case 2928: state = 2929; break; // &gsc -> &gscr
				case 2942: state = 2965; break; // &gt -> &gtr
				case 2947: state = 2948; break; // &gtci -> &gtcir
				case 2956: state = 2957; break; // &gtlPa -> &gtlPar
				case 2966: state = 2973; break; // &gtra -> &gtrar
				case 2968: state = 2969; break; // &gtrapp -> &gtrappr
				case 2973: state = 2974; break; // &gtrar -> &gtrarr
				case 3003: state = 3004; break; // &gve -> &gver
				case 3021: state = 3041; break; // &ha -> &har
				case 3022: state = 3023; break; // &hai -> &hair
				case 3041: state = 3050; break; // &har -> &harr
				case 3046: state = 3047; break; // &hA -> &hAr
				case 3047: state = 3048; break; // &hAr -> &hArr
				case 3053: state = 3054; break; // &harrci -> &harrcir
				case 3061: state = 3062; break; // &hba -> &hbar
				case 3065: state = 3066; break; // &Hci -> &Hcir
				case 3070: state = 3071; break; // &hci -> &hcir
				case 3074: state = 3089; break; // &he -> &her
				case 3075: state = 3076; break; // &hea -> &hear
				case 3094: state = 3095; break; // &Hf -> &Hfr
				case 3097: state = 3098; break; // &hf -> &hfr
				case 3103: state = 3104; break; // &Hilbe -> &Hilber
				case 3115: state = 3116; break; // &hksea -> &hksear
				case 3121: state = 3122; break; // &hkswa -> &hkswar
				case 3126: state = 3166; break; // &ho -> &hor
				case 3127: state = 3128; break; // &hoa -> &hoar
				case 3128: state = 3129; break; // &hoar -> &hoarr
				case 3137: state = 3148; break; // &hook -> &hookr
				case 3142: state = 3143; break; // &hooklefta -> &hookleftar
				case 3143: state = 3144; break; // &hookleftar -> &hookleftarr
				case 3153: state = 3154; break; // &hookrighta -> &hookrightar
				case 3154: state = 3155; break; // &hookrightar -> &hookrightarr
				case 3159: state = 3171; break; // &Ho -> &Hor
				case 3168: state = 3169; break; // &horba -> &horbar
				case 3185: state = 3186; break; // &Hsc -> &Hscr
				case 3189: state = 3190; break; // &hsc -> &hscr
				case 3197: state = 3198; break; // &Hst -> &Hstr
				case 3202: state = 3203; break; // &hst -> &hstr
				case 3253: state = 3254; break; // &Ici -> &Icir
				case 3257: state = 3258; break; // &ici -> &icir
				case 3281: state = 3287; break; // &if -> &ifr
				case 3284: state = 3285; break; // &If -> &Ifr
				case 3289: state = 3290; break; // &Ig -> &Igr
				case 3295: state = 3296; break; // &ig -> &igr
				case 3333: state = 3334; break; // &Imac -> &Imacr
				case 3338: state = 3339; break; // &imac -> &imacr
				case 3347: state = 3348; break; // &Imagina -> &Imaginar
				case 3358: state = 3359; break; // &imagpa -> &imagpar
				case 3381: state = 3382; break; // &inca -> &incar
				case 3407: state = 3419; break; // &inte -> &inter
				case 3409: state = 3410; break; // &intege -> &integer
				case 3413: state = 3424; break; // &Inte -> &Inter
				case 3414: state = 3415; break; // &Integ -> &Integr
				case 3434: state = 3435; break; // &intla -> &intlar
				case 3439: state = 3440; break; // &intp -> &intpr
				case 3492: state = 3493; break; // &ip -> &ipr
				case 3504: state = 3505; break; // &Isc -> &Iscr
				case 3508: state = 3509; break; // &isc -> &iscr
				case 3557: state = 3558; break; // &Jci -> &Jcir
				case 3563: state = 3564; break; // &jci -> &jcir
				case 3571: state = 3572; break; // &Jf -> &Jfr
				case 3574: state = 3575; break; // &jf -> &jfr
				case 3591: state = 3592; break; // &Jsc -> &Jscr
				case 3595: state = 3596; break; // &jsc -> &jscr
				case 3598: state = 3599; break; // &Jse -> &Jser
				case 3603: state = 3604; break; // &jse -> &jser
				case 3648: state = 3649; break; // &Kf -> &Kfr
				case 3651: state = 3652; break; // &kf -> &kfr
				case 3654: state = 3655; break; // &kg -> &kgr
				case 3685: state = 3686; break; // &Ksc -> &Kscr
				case 3689: state = 3690; break; // &ksc -> &kscr
				case 3692: state = 4628; break; // &l -> &lr
				case 3693: state = 3762; break; // &lA -> &lAr
				case 3694: state = 3695; break; // &lAa -> &lAar
				case 3695: state = 3696; break; // &lAar -> &lAarr
				case 3699: state = 3759; break; // &La -> &Lar
				case 3705: state = 3765; break; // &la -> &lar
				case 3718: state = 3719; break; // &lag -> &lagr
				case 3751: state = 3752; break; // &Laplacet -> &Laplacetr
				case 3759: state = 3760; break; // &Lar -> &Larr
				case 3762: state = 3763; break; // &lAr -> &lArr
				case 3765: state = 3766; break; // &lar -> &larr
				case 3808: state = 3809; break; // &lBa -> &lBar
				case 3809: state = 3810; break; // &lBar -> &lBarr
				case 3812: state = 3821; break; // &lb -> &lbr
				case 3813: state = 3814; break; // &lba -> &lbar
				case 3814: state = 3815; break; // &lbar -> &lbarr
				case 3817: state = 3818; break; // &lbb -> &lbbr
				case 3838: state = 3839; break; // &Lca -> &Lcar
				case 3844: state = 3845; break; // &lca -> &lcar
				case 3869: state = 3879; break; // &ld -> &ldr
				case 3875: state = 3877; break; // &ldquo -> &ldquor
				case 3882: state = 3883; break; // &ldrdha -> &ldrdhar
				case 3888: state = 3889; break; // &ldrusha -> &ldrushar
				case 3900: state = 4041; break; // &Left -> &Leftr
				case 3901: state = 3914; break; // &LeftA -> &LeftAr
				case 3906: state = 3907; break; // &LeftAngleB -> &LeftAngleBr
				case 3914: state = 3915; break; // &LeftAr -> &LeftArr
				case 3919: state = 3920; break; // &Lefta -> &Leftar
				case 3920: state = 3921; break; // &Leftar -> &Leftarr
				case 3926: state = 4052; break; // &left -> &leftr
				case 3927: state = 3928; break; // &lefta -> &leftar
				case 3928: state = 3929; break; // &leftar -> &leftarr
				case 3934: state = 3935; break; // &LeftArrowBa -> &LeftArrowBar
				case 3942: state = 3943; break; // &LeftArrowRightA -> &LeftArrowRightAr
				case 3943: state = 3944; break; // &LeftArrowRightAr -> &LeftArrowRightArr
				case 3967: state = 3968; break; // &LeftDoubleB -> &LeftDoubleBr
				case 3984: state = 3985; break; // &LeftDownTeeVecto -> &LeftDownTeeVector
				case 3991: state = 3992; break; // &LeftDownVecto -> &LeftDownVector
				case 3995: state = 3996; break; // &LeftDownVectorBa -> &LeftDownVectorBar
				case 4001: state = 4002; break; // &LeftFloo -> &LeftFloor
				case 4005: state = 4006; break; // &leftha -> &lefthar
				case 4023: state = 4024; break; // &leftlefta -> &leftleftar
				case 4024: state = 4025; break; // &leftleftar -> &leftleftarr
				case 4035: state = 4036; break; // &LeftRightA -> &LeftRightAr
				case 4036: state = 4037; break; // &LeftRightAr -> &LeftRightArr
				case 4046: state = 4047; break; // &Leftrighta -> &Leftrightar
				case 4047: state = 4048; break; // &Leftrightar -> &Leftrightarr
				case 4057: state = 4058; break; // &leftrighta -> &leftrightar
				case 4058: state = 4059; break; // &leftrightar -> &leftrightarr
				case 4066: state = 4067; break; // &leftrightha -> &leftrighthar
				case 4079: state = 4080; break; // &leftrightsquiga -> &leftrightsquigar
				case 4080: state = 4081; break; // &leftrightsquigar -> &leftrightsquigarr
				case 4089: state = 4090; break; // &LeftRightVecto -> &LeftRightVector
				case 4092: state = 4120; break; // &LeftT -> &LeftTr
				case 4096: state = 4097; break; // &LeftTeeA -> &LeftTeeAr
				case 4097: state = 4098; break; // &LeftTeeAr -> &LeftTeeArr
				case 4106: state = 4107; break; // &LeftTeeVecto -> &LeftTeeVector
				case 4110: state = 4111; break; // &leftth -> &leftthr
				case 4129: state = 4130; break; // &LeftTriangleBa -> &LeftTriangleBar
				case 4148: state = 4149; break; // &LeftUpDownVecto -> &LeftUpDownVector
				case 4158: state = 4159; break; // &LeftUpTeeVecto -> &LeftUpTeeVector
				case 4165: state = 4166; break; // &LeftUpVecto -> &LeftUpVector
				case 4169: state = 4170; break; // &LeftUpVectorBa -> &LeftUpVectorBar
				case 4176: state = 4177; break; // &LeftVecto -> &LeftVector
				case 4180: state = 4181; break; // &LeftVectorBa -> &LeftVectorBar
				case 4206: state = 4208; break; // &lesdoto -> &lesdotor
				case 4218: state = 4219; break; // &lessapp -> &lessappr
				case 4230: state = 4231; break; // &lesseqgt -> &lesseqgtr
				case 4235: state = 4236; break; // &lesseqqgt -> &lesseqqgtr
				case 4245: state = 4246; break; // &LessEqualG -> &LessEqualGr
				case 4250: state = 4251; break; // &LessEqualGreate -> &LessEqualGreater
				case 4263: state = 4264; break; // &LessG -> &LessGr
				case 4268: state = 4269; break; // &LessGreate -> &LessGreater
				case 4272: state = 4273; break; // &lessgt -> &lessgtr
				case 4301: state = 4315; break; // &lf -> &lfr
				case 4309: state = 4310; break; // &lfloo -> &lfloor
				case 4312: state = 4313; break; // &Lf -> &Lfr
				case 4322: state = 4323; break; // &lHa -> &lHar
				case 4326: state = 4327; break; // &lha -> &lhar
				case 4350: state = 4351; break; // &lla -> &llar
				case 4351: state = 4352; break; // &llar -> &llarr
				case 4355: state = 4356; break; // &llco -> &llcor
				case 4358: state = 4359; break; // &llcorne -> &llcorner
				case 4364: state = 4365; break; // &Llefta -> &Lleftar
				case 4365: state = 4366; break; // &Lleftar -> &Lleftarr
				case 4371: state = 4372; break; // &llha -> &llhar
				case 4375: state = 4376; break; // &llt -> &lltr
				case 4405: state = 4406; break; // &lnapp -> &lnappr
				case 4423: state = 4427; break; // &loa -> &loar
				case 4427: state = 4428; break; // &loar -> &loarr
				case 4430: state = 4431; break; // &lob -> &lobr
				case 4436: state = 4520; break; // &Long -> &Longr
				case 4441: state = 4442; break; // &LongLeftA -> &LongLeftAr
				case 4442: state = 4443; break; // &LongLeftAr -> &LongLeftArr
				case 4450: state = 4480; break; // &Longleft -> &Longleftr
				case 4451: state = 4452; break; // &Longlefta -> &Longleftar
				case 4452: state = 4453; break; // &Longleftar -> &Longleftarr
				case 4458: state = 4531; break; // &long -> &longr
				case 4462: state = 4491; break; // &longleft -> &longleftr
				case 4463: state = 4464; break; // &longlefta -> &longleftar
				case 4464: state = 4465; break; // &longleftar -> &longleftarr
				case 4474: state = 4475; break; // &LongLeftRightA -> &LongLeftRightAr
				case 4475: state = 4476; break; // &LongLeftRightAr -> &LongLeftRightArr
				case 4485: state = 4486; break; // &Longleftrighta -> &Longleftrightar
				case 4486: state = 4487; break; // &Longleftrightar -> &Longleftrightarr
				case 4496: state = 4497; break; // &longleftrighta -> &longleftrightar
				case 4497: state = 4498; break; // &longleftrightar -> &longleftrightarr
				case 4514: state = 4515; break; // &LongRightA -> &LongRightAr
				case 4515: state = 4516; break; // &LongRightAr -> &LongRightArr
				case 4525: state = 4526; break; // &Longrighta -> &Longrightar
				case 4526: state = 4527; break; // &Longrightar -> &Longrightarr
				case 4536: state = 4537; break; // &longrighta -> &longrightar
				case 4537: state = 4538; break; // &longrightar -> &longrightarr
				case 4544: state = 4545; break; // &loopa -> &loopar
				case 4545: state = 4546; break; // &loopar -> &looparr
				case 4548: state = 4554; break; // &looparrow -> &looparrowr
				case 4561: state = 4562; break; // &lopa -> &lopar
				case 4585: state = 4586; break; // &lowba -> &lowbar
				case 4589: state = 4590; break; // &Lowe -> &Lower
				case 4595: state = 4596; break; // &LowerLeftA -> &LowerLeftAr
				case 4596: state = 4597; break; // &LowerLeftAr -> &LowerLeftArr
				case 4606: state = 4607; break; // &LowerRightA -> &LowerRightAr
				case 4607: state = 4608; break; // &LowerRightAr -> &LowerRightArr
				case 4622: state = 4623; break; // &lpa -> &lpar
				case 4629: state = 4630; break; // &lra -> &lrar
				case 4630: state = 4631; break; // &lrar -> &lrarr
				case 4634: state = 4635; break; // &lrco -> &lrcor
				case 4637: state = 4638; break; // &lrcorne -> &lrcorner
				case 4641: state = 4642; break; // &lrha -> &lrhar
				case 4648: state = 4649; break; // &lrt -> &lrtr
				case 4659: state = 4660; break; // &Lsc -> &Lscr
				case 4662: state = 4663; break; // &lsc -> &lscr
				case 4680: state = 4682; break; // &lsquo -> &lsquor
				case 4684: state = 4685; break; // &Lst -> &Lstr
				case 4689: state = 4690; break; // &lst -> &lstr
				case 4698: state = 4731; break; // &lt -> &ltr
				case 4703: state = 4704; break; // &ltci -> &ltcir
				case 4710: state = 4711; break; // &lth -> &lthr
				case 4721: state = 4722; break; // &ltla -> &ltlar
				case 4722: state = 4723; break; // &ltlar -> &ltlarr
				case 4739: state = 4740; break; // &ltrPa -> &ltrPar
				case 4742: state = 4743; break; // &lu -> &lur
				case 4747: state = 4748; break; // &lurdsha -> &lurdshar
				case 4752: state = 4753; break; // &luruha -> &luruhar
				case 4756: state = 4757; break; // &lve -> &lver
				case 4768: state = 4804; break; // &ma -> &mar
				case 4769: state = 4770; break; // &mac -> &macr
				case 4806: state = 4807; break; // &marke -> &marker
				case 4833: state = 4834; break; // &measu -> &measur
				case 4858: state = 4859; break; // &Mellint -> &Mellintr
				case 4862: state = 4863; break; // &Mf -> &Mfr
				case 4865: state = 4866; break; // &mf -> &mfr
				case 4872: state = 4873; break; // &mic -> &micr
				case 4883: state = 4884; break; // &midci -> &midcir
				case 4913: state = 4914; break; // &mld -> &mldr
				case 4938: state = 4939; break; // &Msc -> &Mscr
				case 4942: state = 4943; break; // &msc -> &mscr
				case 4965: state = 5855; break; // &n -> &nr
				case 4996: state = 4997; break; // &napp -> &nappr
				case 5002: state = 5003; break; // &natu -> &natur
				case 5021: state = 5030; break; // &nca -> &ncar
				case 5025: state = 5026; break; // &Nca -> &Ncar
				case 5066: state = 5067; break; // &nea -> &near
				case 5067: state = 5075; break; // &near -> &nearr
				case 5071: state = 5072; break; // &neA -> &neAr
				case 5072: state = 5073; break; // &neAr -> &neArr
				case 5122: state = 5123; break; // &NegativeVe -> &NegativeVer
				case 5142: state = 5143; break; // &nesea -> &nesear
				case 5152: state = 5153; break; // &NestedG -> &NestedGr
				case 5157: state = 5158; break; // &NestedGreate -> &NestedGreater
				case 5159: state = 5160; break; // &NestedGreaterG -> &NestedGreaterGr
				case 5164: state = 5165; break; // &NestedGreaterGreate -> &NestedGreaterGreater
				case 5189: state = 5190; break; // &Nf -> &Nfr
				case 5192: state = 5193; break; // &nf -> &nfr
				case 5221: state = 5223; break; // &ngt -> &ngtr
				case 5228: state = 5229; break; // &nhA -> &nhAr
				case 5229: state = 5230; break; // &nhAr -> &nhArr
				case 5232: state = 5233; break; // &nha -> &nhar
				case 5233: state = 5234; break; // &nhar -> &nharr
				case 5237: state = 5238; break; // &nhpa -> &nhpar
				case 5257: state = 5258; break; // &nlA -> &nlAr
				case 5258: state = 5259; break; // &nlAr -> &nlArr
				case 5261: state = 5262; break; // &nla -> &nlar
				case 5262: state = 5263; break; // &nlar -> &nlarr
				case 5265: state = 5266; break; // &nld -> &nldr
				case 5275: state = 5290; break; // &nLeft -> &nLeftr
				case 5276: state = 5277; break; // &nLefta -> &nLeftar
				case 5277: state = 5278; break; // &nLeftar -> &nLeftarr
				case 5283: state = 5301; break; // &nleft -> &nleftr
				case 5284: state = 5285; break; // &nlefta -> &nleftar
				case 5285: state = 5286; break; // &nleftar -> &nleftarr
				case 5295: state = 5296; break; // &nLeftrighta -> &nLeftrightar
				case 5296: state = 5297; break; // &nLeftrightar -> &nLeftrightarr
				case 5306: state = 5307; break; // &nleftrighta -> &nleftrightar
				case 5307: state = 5308; break; // &nleftrightar -> &nleftrightarr
				case 5334: state = 5336; break; // &nlt -> &nltr
				case 5348: state = 5349; break; // &NoB -> &NoBr
				case 5355: state = 5356; break; // &NonB -> &NonBr
				case 5383: state = 5384; break; // &NotCong -> &NotCongr
				case 5403: state = 5404; break; // &NotDoubleVe -> &NotDoubleVer
				case 5411: state = 5412; break; // &NotDoubleVerticalBa -> &NotDoubleVerticalBar
				case 5439: state = 5440; break; // &NotG -> &NotGr
				case 5444: state = 5445; break; // &NotGreate -> &NotGreater
				case 5463: state = 5464; break; // &NotGreaterG -> &NotGreaterGr
				case 5468: state = 5469; break; // &NotGreaterGreate -> &NotGreaterGreater
				case 5532: state = 5533; break; // &NotLeftT -> &NotLeftTr
				case 5542: state = 5543; break; // &NotLeftTriangleBa -> &NotLeftTriangleBar
				case 5560: state = 5561; break; // &NotLessG -> &NotLessGr
				case 5565: state = 5566; break; // &NotLessGreate -> &NotLessGreater
				case 5596: state = 5597; break; // &NotNestedG -> &NotNestedGr
				case 5601: state = 5602; break; // &NotNestedGreate -> &NotNestedGreater
				case 5603: state = 5604; break; // &NotNestedGreaterG -> &NotNestedGreaterGr
				case 5608: state = 5609; break; // &NotNestedGreaterGreate -> &NotNestedGreaterGreater
				case 5630: state = 5631; break; // &NotP -> &NotPr
				case 5659: state = 5660; break; // &NotReve -> &NotRever
				case 5675: state = 5676; break; // &NotRightT -> &NotRightTr
				case 5685: state = 5686; break; // &NotRightTriangleBa -> &NotRightTriangleBar
				case 5697: state = 5698; break; // &NotSqua -> &NotSquar
				case 5714: state = 5715; break; // &NotSquareSupe -> &NotSquareSuper
				case 5769: state = 5770; break; // &NotSupe -> &NotSuper
				case 5810: state = 5811; break; // &NotVe -> &NotVer
				case 5818: state = 5819; break; // &NotVerticalBa -> &NotVerticalBar
				case 5821: state = 5842; break; // &np -> &npr
				case 5822: state = 5823; break; // &npa -> &npar
				case 5856: state = 5857; break; // &nrA -> &nrAr
				case 5857: state = 5858; break; // &nrAr -> &nrArr
				case 5860: state = 5861; break; // &nra -> &nrar
				case 5861: state = 5862; break; // &nrar -> &nrarr
				case 5873: state = 5874; break; // &nRighta -> &nRightar
				case 5874: state = 5875; break; // &nRightar -> &nRightarr
				case 5883: state = 5884; break; // &nrighta -> &nrightar
				case 5884: state = 5885; break; // &nrightar -> &nrightarr
				case 5889: state = 5890; break; // &nrt -> &nrtr
				case 5896: state = 5908; break; // &nsc -> &nscr
				case 5905: state = 5906; break; // &Nsc -> &Nscr
				case 5911: state = 5912; break; // &nsho -> &nshor
				case 5919: state = 5920; break; // &nshortpa -> &nshortpar
				case 5939: state = 5940; break; // &nspa -> &nspar
				case 5988: state = 6006; break; // &nt -> &ntr
				case 6012: state = 6021; break; // &ntriangle -> &ntriangler
				case 6036: state = 6037; break; // &nume -> &numer
				case 6043: state = 6097; break; // &nv -> &nvr
				case 6074: state = 6075; break; // &nvHa -> &nvHar
				case 6075: state = 6076; break; // &nvHar -> &nvHarr
				case 6085: state = 6086; break; // &nvlA -> &nvlAr
				case 6086: state = 6087; break; // &nvlAr -> &nvlArr
				case 6091: state = 6093; break; // &nvlt -> &nvltr
				case 6098: state = 6099; break; // &nvrA -> &nvrAr
				case 6099: state = 6100; break; // &nvrAr -> &nvrArr
				case 6102: state = 6103; break; // &nvrt -> &nvrtr
				case 6112: state = 6113; break; // &nwa -> &nwar
				case 6113: state = 6121; break; // &nwar -> &nwarr
				case 6117: state = 6118; break; // &nwA -> &nwAr
				case 6118: state = 6119; break; // &nwAr -> &nwArr
				case 6128: state = 6129; break; // &nwnea -> &nwnear
				case 6131: state = 6340; break; // &O -> &Or
				case 6138: state = 6342; break; // &o -> &or
				case 6149: state = 6150; break; // &oci -> &ocir
				case 6153: state = 6154; break; // &Oci -> &Ocir
				case 6200: state = 6208; break; // &of -> &ofr
				case 6202: state = 6203; break; // &ofci -> &ofcir
				case 6205: state = 6206; break; // &Of -> &Ofr
				case 6210: state = 6220; break; // &og -> &ogr
				case 6214: state = 6215; break; // &Og -> &Ogr
				case 6229: state = 6230; break; // &ohba -> &ohbar
				case 6239: state = 6240; break; // &ola -> &olar
				case 6240: state = 6241; break; // &olar -> &olarr
				case 6243: state = 6247; break; // &olc -> &olcr
				case 6244: state = 6245; break; // &olci -> &olcir
				case 6260: state = 6261; break; // &Omac -> &Omacr
				case 6265: state = 6266; break; // &omac -> &omacr
				case 6277: state = 6278; break; // &Omic -> &Omicr
				case 6283: state = 6284; break; // &omic -> &omicr
				case 6303: state = 6304; break; // &opa -> &opar
				case 6310: state = 6311; break; // &OpenCu -> &OpenCur
				case 6332: state = 6333; break; // &ope -> &oper
				case 6344: state = 6345; break; // &ora -> &orar
				case 6345: state = 6346; break; // &orar -> &orarr
				case 6350: state = 6351; break; // &orde -> &order
				case 6365: state = 6366; break; // &oro -> &oror
				case 6379: state = 6380; break; // &Osc -> &Oscr
				case 6383: state = 6384; break; // &osc -> &oscr
				case 6432: state = 6433; break; // &ovba -> &ovbar
				case 6436: state = 6437; break; // &Ove -> &Over
				case 6438: state = 6442; break; // &OverB -> &OverBr
				case 6439: state = 6440; break; // &OverBa -> &OverBar
				case 6452: state = 6453; break; // &OverPa -> &OverPar
				case 6463: state = 6642; break; // &p -> &pr
				case 6464: state = 6465; break; // &pa -> &par
				case 6482: state = 6640; break; // &P -> &Pr
				case 6483: state = 6484; break; // &Pa -> &Par
				case 6497: state = 6498; break; // &pe -> &per
				case 6518: state = 6519; break; // &Pf -> &Pfr
				case 6521: state = 6522; break; // &pf -> &pfr
				case 6549: state = 6550; break; // &pitchfo -> &pitchfor
				case 6571: state = 6572; break; // &plusaci -> &plusacir
				case 6577: state = 6578; break; // &plusci -> &pluscir
				case 6613: state = 6614; break; // &Poinca -> &Poincar
				case 6659: state = 6660; break; // &precapp -> &precappr
				case 6665: state = 6666; break; // &preccu -> &preccur
				case 6708: state = 6709; break; // &precnapp -> &precnappr
				case 6757: state = 6758; break; // &profala -> &profalar
				case 6766: state = 6767; break; // &profsu -> &profsur
				case 6773: state = 6774; break; // &Propo -> &Propor
				case 6790: state = 6791; break; // &pru -> &prur
				case 6796: state = 6797; break; // &Psc -> &Pscr
				case 6800: state = 6801; break; // &psc -> &pscr
				case 6814: state = 6815; break; // &Qf -> &Qfr
				case 6818: state = 6819; break; // &qf -> &qfr
				case 6833: state = 6834; break; // &qp -> &qpr
				case 6840: state = 6841; break; // &Qsc -> &Qscr
				case 6844: state = 6845; break; // &qsc -> &qscr
				case 6850: state = 6851; break; // &quate -> &quater
				case 6876: state = 7526; break; // &r -> &rr
				case 6877: state = 6928; break; // &rA -> &rAr
				case 6878: state = 6879; break; // &rAa -> &rAar
				case 6879: state = 6880; break; // &rAar -> &rAarr
				case 6882: state = 6931; break; // &ra -> &rar
				case 6886: state = 7531; break; // &R -> &Rr
				case 6887: state = 6925; break; // &Ra -> &Rar
				case 6925: state = 6926; break; // &Rar -> &Rarr
				case 6928: state = 6929; break; // &rAr -> &rArr
				case 6931: state = 6932; break; // &rar -> &rarr
				case 6987: state = 6988; break; // &RBa -> &RBar
				case 6988: state = 6989; break; // &RBar -> &RBarr
				case 6992: state = 6993; break; // &rBa -> &rBar
				case 6993: state = 6994; break; // &rBar -> &rBarr
				case 6996: state = 7005; break; // &rb -> &rbr
				case 6997: state = 6998; break; // &rba -> &rbar
				case 6998: state = 6999; break; // &rbar -> &rbarr
				case 7001: state = 7002; break; // &rbb -> &rbbr
				case 7022: state = 7023; break; // &Rca -> &Rcar
				case 7028: state = 7029; break; // &rca -> &rcar
				case 7060: state = 7061; break; // &rdldha -> &rdldhar
				case 7065: state = 7067; break; // &rdquo -> &rdquor
				case 7083: state = 7084; break; // &realpa -> &realpar
				case 7098: state = 7099; break; // &Reve -> &Rever
				case 7115: state = 7116; break; // &ReverseEquilib -> &ReverseEquilibr
				case 7129: state = 7130; break; // &ReverseUpEquilib -> &ReverseUpEquilibr
				case 7135: state = 7149; break; // &rf -> &rfr
				case 7143: state = 7144; break; // &rfloo -> &rfloor
				case 7146: state = 7147; break; // &Rf -> &Rfr
				case 7152: state = 7153; break; // &rHa -> &rHar
				case 7156: state = 7157; break; // &rha -> &rhar
				case 7175: state = 7188; break; // &RightA -> &RightAr
				case 7180: state = 7181; break; // &RightAngleB -> &RightAngleBr
				case 7188: state = 7189; break; // &RightAr -> &RightArr
				case 7193: state = 7194; break; // &Righta -> &Rightar
				case 7194: state = 7195; break; // &Rightar -> &Rightarr
				case 7202: state = 7314; break; // &right -> &rightr
				case 7203: state = 7204; break; // &righta -> &rightar
				case 7204: state = 7205; break; // &rightar -> &rightarr
				case 7210: state = 7211; break; // &RightArrowBa -> &RightArrowBar
				case 7217: state = 7218; break; // &RightArrowLeftA -> &RightArrowLeftAr
				case 7218: state = 7219; break; // &RightArrowLeftAr -> &RightArrowLeftArr
				case 7242: state = 7243; break; // &RightDoubleB -> &RightDoubleBr
				case 7259: state = 7260; break; // &RightDownTeeVecto -> &RightDownTeeVector
				case 7266: state = 7267; break; // &RightDownVecto -> &RightDownVector
				case 7270: state = 7271; break; // &RightDownVectorBa -> &RightDownVectorBar
				case 7276: state = 7277; break; // &RightFloo -> &RightFloor
				case 7280: state = 7281; break; // &rightha -> &righthar
				case 7298: state = 7299; break; // &rightlefta -> &rightleftar
				case 7299: state = 7300; break; // &rightleftar -> &rightleftarr
				case 7306: state = 7307; break; // &rightleftha -> &rightlefthar
				case 7319: state = 7320; break; // &rightrighta -> &rightrightar
				case 7320: state = 7321; break; // &rightrightar -> &rightrightarr
				case 7331: state = 7332; break; // &rightsquiga -> &rightsquigar
				case 7332: state = 7333; break; // &rightsquigar -> &rightsquigarr
				case 7337: state = 7365; break; // &RightT -> &RightTr
				case 7341: state = 7342; break; // &RightTeeA -> &RightTeeAr
				case 7342: state = 7343; break; // &RightTeeAr -> &RightTeeArr
				case 7351: state = 7352; break; // &RightTeeVecto -> &RightTeeVector
				case 7355: state = 7356; break; // &rightth -> &rightthr
				case 7374: state = 7375; break; // &RightTriangleBa -> &RightTriangleBar
				case 7393: state = 7394; break; // &RightUpDownVecto -> &RightUpDownVector
				case 7403: state = 7404; break; // &RightUpTeeVecto -> &RightUpTeeVector
				case 7410: state = 7411; break; // &RightUpVecto -> &RightUpVector
				case 7414: state = 7415; break; // &RightUpVectorBa -> &RightUpVectorBar
				case 7421: state = 7422; break; // &RightVecto -> &RightVector
				case 7425: state = 7426; break; // &RightVectorBa -> &RightVectorBar
				case 7443: state = 7444; break; // &rla -> &rlar
				case 7444: state = 7445; break; // &rlar -> &rlarr
				case 7448: state = 7449; break; // &rlha -> &rlhar
				case 7470: state = 7474; break; // &roa -> &roar
				case 7474: state = 7475; break; // &roar -> &roarr
				case 7477: state = 7478; break; // &rob -> &robr
				case 7482: state = 7483; break; // &ropa -> &ropar
				case 7513: state = 7514; break; // &rpa -> &rpar
				case 7527: state = 7528; break; // &rra -> &rrar
				case 7528: state = 7529; break; // &rrar -> &rrarr
				case 7536: state = 7537; break; // &Rrighta -> &Rrightar
				case 7537: state = 7538; break; // &Rrightar -> &Rrightarr
				case 7549: state = 7550; break; // &Rsc -> &Rscr
				case 7552: state = 7553; break; // &rsc -> &rscr
				case 7563: state = 7565; break; // &rsquo -> &rsquor
				case 7567: state = 7578; break; // &rt -> &rtr
				case 7568: state = 7569; break; // &rth -> &rthr
				case 7586: state = 7587; break; // &rtrilt -> &rtriltr
				case 7605: state = 7606; break; // &ruluha -> &ruluhar
				case 7617: state = 8068; break; // &s -> &sr
				case 7633: state = 7641; break; // &sca -> &scar
				case 7636: state = 7637; break; // &Sca -> &Scar
				case 7662: state = 7663; break; // &Sci -> &Scir
				case 7666: state = 7667; break; // &sci -> &scir
				case 7704: state = 7705; break; // &sea -> &sear
				case 7705: state = 7713; break; // &sear -> &searr
				case 7709: state = 7710; break; // &seA -> &seAr
				case 7710: state = 7711; break; // &seAr -> &seArr
				case 7726: state = 7727; break; // &seswa -> &seswar
				case 7741: state = 7742; break; // &Sf -> &Sfr
				case 7744: state = 7745; break; // &sf -> &sfr
				case 7752: state = 7753; break; // &sha -> &shar
				case 7773: state = 7774; break; // &Sho -> &Shor
				case 7780: state = 7781; break; // &ShortDownA -> &ShortDownAr
				case 7781: state = 7782; break; // &ShortDownAr -> &ShortDownArr
				case 7790: state = 7791; break; // &ShortLeftA -> &ShortLeftAr
				case 7791: state = 7792; break; // &ShortLeftAr -> &ShortLeftArr
				case 7796: state = 7797; break; // &sho -> &shor
				case 7804: state = 7805; break; // &shortpa -> &shortpar
				case 7817: state = 7818; break; // &ShortRightA -> &ShortRightAr
				case 7818: state = 7819; break; // &ShortRightAr -> &ShortRightArr
				case 7825: state = 7826; break; // &ShortUpA -> &ShortUpAr
				case 7826: state = 7827; break; // &ShortUpAr -> &ShortUpArr
				case 7847: state = 7873; break; // &sim -> &simr
				case 7874: state = 7875; break; // &simra -> &simrar
				case 7875: state = 7876; break; // &simrar -> &simrarr
				case 7879: state = 7880; break; // &sla -> &slar
				case 7880: state = 7881; break; // &slar -> &slarr
				case 7888: state = 7889; break; // &SmallCi -> &SmallCir
				case 7913: state = 7914; break; // &smepa -> &smepar
				case 7946: state = 7947; break; // &solba -> &solbar
				case 7957: state = 7966; break; // &spa -> &spar
				case 7980: state = 7981; break; // &Sq -> &Sqr
				case 8011: state = 8012; break; // &Squa -> &Squar
				case 8015: state = 8016; break; // &squa -> &squar
				case 8022: state = 8023; break; // &SquareInte -> &SquareInter
				case 8046: state = 8047; break; // &SquareSupe -> &SquareSuper
				case 8069: state = 8070; break; // &sra -> &srar
				case 8070: state = 8071; break; // &srar -> &srarr
				case 8074: state = 8075; break; // &Ssc -> &Sscr
				case 8078: state = 8079; break; // &ssc -> &sscr
				case 8092: state = 8093; break; // &ssta -> &sstar
				case 8097: state = 8098; break; // &Sta -> &Star
				case 8100: state = 8106; break; // &st -> &str
				case 8101: state = 8102; break; // &sta -> &star
				case 8131: state = 8160; break; // &sub -> &subr
				case 8161: state = 8162; break; // &subra -> &subrar
				case 8162: state = 8163; break; // &subrar -> &subrarr
				case 8203: state = 8204; break; // &succapp -> &succappr
				case 8209: state = 8210; break; // &succcu -> &succcur
				case 8252: state = 8253; break; // &succnapp -> &succnappr
				case 8308: state = 8309; break; // &Supe -> &Super
				case 8329: state = 8330; break; // &supla -> &suplar
				case 8330: state = 8331; break; // &suplar -> &suplarr
				case 8376: state = 8377; break; // &swa -> &swar
				case 8377: state = 8385; break; // &swar -> &swarr
				case 8381: state = 8382; break; // &swA -> &swAr
				case 8382: state = 8383; break; // &swAr -> &swArr
				case 8392: state = 8393; break; // &swnwa -> &swnwar
				case 8400: state = 8676; break; // &T -> &Tr
				case 8404: state = 8628; break; // &t -> &tr
				case 8405: state = 8406; break; // &ta -> &tar
				case 8415: state = 8416; break; // &tb -> &tbr
				case 8420: state = 8421; break; // &Tca -> &Tcar
				case 8426: state = 8427; break; // &tca -> &tcar
				case 8450: state = 8451; break; // &tel -> &telr
				case 8455: state = 8456; break; // &Tf -> &Tfr
				case 8458: state = 8459; break; // &tf -> &tfr
				case 8462: state = 8463; break; // &the -> &ther
				case 8468: state = 8469; break; // &The -> &Ther
				case 8472: state = 8473; break; // &Therefo -> &Therefor
				case 8477: state = 8478; break; // &therefo -> &therefor
				case 8498: state = 8499; break; // &thickapp -> &thickappr
				case 8540: state = 8541; break; // &tho -> &thor
				case 8582: state = 8583; break; // &timesba -> &timesbar
				case 8601: state = 8602; break; // &topci -> &topcir
				case 8610: state = 8611; break; // &topfo -> &topfor
				case 8617: state = 8618; break; // &tp -> &tpr
				case 8638: state = 8655; break; // &triangle -> &triangler
				case 8706: state = 8707; break; // &Tsc -> &Tscr
				case 8710: state = 8711; break; // &tsc -> &tscr
				case 8727: state = 8728; break; // &Tst -> &Tstr
				case 8732: state = 8733; break; // &tst -> &tstr
				case 8746: state = 8757; break; // &twohead -> &twoheadr
				case 8751: state = 8752; break; // &twoheadlefta -> &twoheadleftar
				case 8752: state = 8753; break; // &twoheadleftar -> &twoheadleftarr
				case 8762: state = 8763; break; // &twoheadrighta -> &twoheadrightar
				case 8763: state = 8764; break; // &twoheadrightar -> &twoheadrightarr
				case 8768: state = 9140; break; // &U -> &Ur
				case 8769: state = 8782; break; // &Ua -> &Uar
				case 8775: state = 9127; break; // &u -> &ur
				case 8776: state = 8789; break; // &ua -> &uar
				case 8782: state = 8783; break; // &Uar -> &Uarr
				case 8785: state = 8786; break; // &uA -> &uAr
				case 8786: state = 8787; break; // &uAr -> &uArr
				case 8789: state = 8790; break; // &uar -> &uarr
				case 8794: state = 8795; break; // &Uarroci -> &Uarrocir
				case 8797: state = 8798; break; // &Ub -> &Ubr
				case 8802: state = 8803; break; // &ub -> &ubr
				case 8816: state = 8817; break; // &Uci -> &Ucir
				case 8821: state = 8822; break; // &uci -> &ucir
				case 8830: state = 8831; break; // &uda -> &udar
				case 8831: state = 8832; break; // &udar -> &udarr
				case 8846: state = 8847; break; // &udha -> &udhar
				case 8849: state = 8858; break; // &uf -> &ufr
				case 8855: state = 8856; break; // &Uf -> &Ufr
				case 8860: state = 8861; break; // &Ug -> &Ugr
				case 8866: state = 8867; break; // &ug -> &ugr
				case 8873: state = 8874; break; // &uHa -> &uHar
				case 8877: state = 8878; break; // &uha -> &uhar
				case 8878: state = 8881; break; // &uhar -> &uharr
				case 8888: state = 8896; break; // &ulc -> &ulcr
				case 8889: state = 8890; break; // &ulco -> &ulcor
				case 8893: state = 8894; break; // &ulcorne -> &ulcorner
				case 8900: state = 8901; break; // &ult -> &ultr
				case 8906: state = 8907; break; // &Umac -> &Umacr
				case 8911: state = 8912; break; // &umac -> &umacr
				case 8918: state = 8919; break; // &Unde -> &Under
				case 8920: state = 8924; break; // &UnderB -> &UnderBr
				case 8921: state = 8922; break; // &UnderBa -> &UnderBar
				case 8934: state = 8935; break; // &UnderPa -> &UnderPar
				case 8971: state = 8972; break; // &UpA -> &UpAr
				case 8972: state = 8973; break; // &UpAr -> &UpArr
				case 8977: state = 8978; break; // &Upa -> &Upar
				case 8978: state = 8979; break; // &Upar -> &Uparr
				case 8984: state = 8985; break; // &upa -> &upar
				case 8985: state = 8986; break; // &upar -> &uparr
				case 8991: state = 8992; break; // &UpArrowBa -> &UpArrowBar
				case 8998: state = 8999; break; // &UpArrowDownA -> &UpArrowDownAr
				case 8999: state = 9000; break; // &UpArrowDownAr -> &UpArrowDownArr
				case 9008: state = 9009; break; // &UpDownA -> &UpDownAr
				case 9009: state = 9010; break; // &UpDownAr -> &UpDownArr
				case 9018: state = 9019; break; // &Updowna -> &Updownar
				case 9019: state = 9020; break; // &Updownar -> &Updownarr
				case 9028: state = 9029; break; // &updowna -> &updownar
				case 9029: state = 9030; break; // &updownar -> &updownarr
				case 9040: state = 9041; break; // &UpEquilib -> &UpEquilibr
				case 9047: state = 9048; break; // &upha -> &uphar
				case 9052: state = 9058; break; // &upharpoon -> &upharpoonr
				case 9069: state = 9070; break; // &Uppe -> &Upper
				case 9075: state = 9076; break; // &UpperLeftA -> &UpperLeftAr
				case 9076: state = 9077; break; // &UpperLeftAr -> &UpperLeftArr
				case 9086: state = 9087; break; // &UpperRightA -> &UpperRightAr
				case 9087: state = 9088; break; // &UpperRightAr -> &UpperRightArr
				case 9112: state = 9113; break; // &UpTeeA -> &UpTeeAr
				case 9113: state = 9114; break; // &UpTeeAr -> &UpTeeArr
				case 9120: state = 9121; break; // &upupa -> &upupar
				case 9121: state = 9122; break; // &upupar -> &upuparr
				case 9128: state = 9136; break; // &urc -> &urcr
				case 9129: state = 9130; break; // &urco -> &urcor
				case 9133: state = 9134; break; // &urcorne -> &urcorner
				case 9149: state = 9150; break; // &urt -> &urtr
				case 9154: state = 9155; break; // &Usc -> &Uscr
				case 9158: state = 9159; break; // &usc -> &uscr
				case 9161: state = 9177; break; // &ut -> &utr
				case 9183: state = 9184; break; // &uua -> &uuar
				case 9184: state = 9185; break; // &uuar -> &uuarr
				case 9201: state = 9445; break; // &v -> &vr
				case 9202: state = 9208; break; // &va -> &var
				case 9204: state = 9205; break; // &vang -> &vangr
				case 9208: state = 9247; break; // &var -> &varr
				case 9231: state = 9237; break; // &varp -> &varpr
				case 9243: state = 9244; break; // &vA -> &vAr
				case 9244: state = 9245; break; // &vAr -> &vArr
				case 9279: state = 9285; break; // &vart -> &vartr
				case 9291: state = 9297; break; // &vartriangle -> &vartriangler
				case 9305: state = 9306; break; // &Vba -> &Vbar
				case 9309: state = 9310; break; // &vBa -> &vBar
				case 9342: state = 9360; break; // &Ve -> &Ver
				case 9345: state = 9365; break; // &ve -> &ver
				case 9349: state = 9350; break; // &veeba -> &veebar
				case 9362: state = 9363; break; // &Verba -> &Verbar
				case 9367: state = 9368; break; // &verba -> &verbar
				case 9379: state = 9380; break; // &VerticalBa -> &VerticalBar
				case 9390: state = 9391; break; // &VerticalSepa -> &VerticalSepar
				case 9394: state = 9395; break; // &VerticalSeparato -> &VerticalSeparator
				case 9414: state = 9415; break; // &Vf -> &Vfr
				case 9417: state = 9418; break; // &vf -> &vfr
				case 9421: state = 9422; break; // &vlt -> &vltr
				case 9440: state = 9441; break; // &vp -> &vpr
				case 9446: state = 9447; break; // &vrt -> &vrtr
				case 9451: state = 9452; break; // &Vsc -> &Vscr
				case 9455: state = 9456; break; // &vsc -> &vscr
				case 9486: state = 9487; break; // &Wci -> &Wcir
				case 9490: state = 9533; break; // &w -> &wr
				case 9492: state = 9493; break; // &wci -> &wcir
				case 9499: state = 9500; break; // &wedba -> &wedbar
				case 9513: state = 9514; break; // &weie -> &weier
				case 9517: state = 9518; break; // &Wf -> &Wfr
				case 9520: state = 9521; break; // &wf -> &wfr
				case 9541: state = 9542; break; // &Wsc -> &Wscr
				case 9545: state = 9546; break; // &wsc -> &wscr
				case 9548: state = 9623; break; // &x -> &xr
				case 9553: state = 9554; break; // &xci -> &xcir
				case 9561: state = 9562; break; // &xdt -> &xdtr
				case 9566: state = 9567; break; // &Xf -> &Xfr
				case 9569: state = 9570; break; // &xf -> &xfr
				case 9573: state = 9574; break; // &xhA -> &xhAr
				case 9574: state = 9575; break; // &xhAr -> &xhArr
				case 9577: state = 9578; break; // &xha -> &xhar
				case 9578: state = 9579; break; // &xhar -> &xharr
				case 9586: state = 9587; break; // &xlA -> &xlAr
				case 9587: state = 9588; break; // &xlAr -> &xlArr
				case 9590: state = 9591; break; // &xla -> &xlar
				case 9591: state = 9592; break; // &xlar -> &xlarr
				case 9624: state = 9625; break; // &xrA -> &xrAr
				case 9625: state = 9626; break; // &xrAr -> &xrArr
				case 9628: state = 9629; break; // &xra -> &xrar
				case 9629: state = 9630; break; // &xrar -> &xrarr
				case 9633: state = 9634; break; // &Xsc -> &Xscr
				case 9637: state = 9638; break; // &xsc -> &xscr
				case 9651: state = 9652; break; // &xut -> &xutr
				case 9686: state = 9687; break; // &Yci -> &Ycir
				case 9691: state = 9692; break; // &yci -> &ycir
				case 9702: state = 9703; break; // &Yf -> &Yfr
				case 9705: state = 9706; break; // &yf -> &yfr
				case 9725: state = 9726; break; // &Ysc -> &Yscr
				case 9729: state = 9730; break; // &ysc -> &yscr
				case 9762: state = 9763; break; // &Zca -> &Zcar
				case 9768: state = 9769; break; // &zca -> &zcar
				case 9787: state = 9788; break; // &zeet -> &zeetr
				case 9791: state = 9792; break; // &Ze -> &Zer
				case 9811: state = 9812; break; // &Zf -> &Zfr
				case 9814: state = 9815; break; // &zf -> &zfr
				case 9826: state = 9827; break; // &zig -> &zigr
				case 9828: state = 9829; break; // &zigra -> &zigrar
				case 9829: state = 9830; break; // &zigrar -> &zigrarr
				case 9841: state = 9842; break; // &Zsc -> &Zscr
				case 9845: state = 9846; break; // &zsc -> &zscr
				default: return false;
				}
				break;
			case 's':
				switch (state) {
				case 0: state = 7617; break; // & -> &s
				case 1: state = 247; break; // &A -> &As
				case 8: state = 251; break; // &a -> &as
				case 81: state = 82; break; // &alef -> &alefs
				case 120: state = 128; break; // &and -> &ands
				case 136: state = 172; break; // &ang -> &angs
				case 143: state = 144; break; // &angm -> &angms
				case 213: state = 214; break; // &apo -> &apos
				case 247: state = 255; break; // &As -> &Ass
				case 301: state = 744; break; // &b -> &bs
				case 304: state = 324; break; // &back -> &backs
				case 311: state = 312; break; // &backep -> &backeps
				case 331: state = 740; break; // &B -> &Bs
				case 334: state = 335; break; // &Back -> &Backs
				case 337: state = 338; break; // &Backsla -> &Backslas
				case 387: state = 388; break; // &becau -> &becaus
				case 393: state = 394; break; // &Becau -> &Becaus
				case 405: state = 406; break; // &bep -> &beps
				case 420: state = 421; break; // &Bernoulli -> &Bernoullis
				case 443: state = 471; break; // &big -> &bigs
				case 462: state = 463; break; // &bigoplu -> &bigoplus
				case 468: state = 469; break; // &bigotime -> &bigotimes
				case 500: state = 501; break; // &biguplu -> &biguplus
				case 522: state = 531; break; // &black -> &blacks
				case 659: state = 660; break; // &boxminu -> &boxminus
				case 664: state = 665; break; // &boxplu -> &boxplus
				case 670: state = 671; break; // &boxtime -> &boxtimes
				case 762: state = 763; break; // &bsolh -> &bsolhs
				case 789: state = 1270; break; // &C -> &Cs
				case 796: state = 1274; break; // &c -> &cs
				case 805: state = 846; break; // &cap -> &caps
				case 858: state = 859; break; // &Cayley -> &Cayleys
				case 863: state = 864; break; // &ccap -> &ccaps
				case 901: state = 902; break; // &ccup -> &ccups
				case 902: state = 904; break; // &ccups -> &ccupss
				case 979: state = 1063; break; // &cir -> &cirs
				case 1005: state = 1006; break; // &circleda -> &circledas
				case 1015: state = 1016; break; // &circledda -> &circleddas
				case 1035: state = 1036; break; // &CircleMinu -> &CircleMinus
				case 1040: state = 1041; break; // &CirclePlu -> &CirclePlus
				case 1046: state = 1047; break; // &CircleTime -> &CircleTimes
				case 1069: state = 1092; break; // &Clo -> &Clos
				case 1073: state = 1074; break; // &Clockwi -> &Clockwis
				case 1119: state = 1120; break; // &club -> &clubs
				case 1161: state = 1162; break; // &complexe -> &complexes
				case 1221: state = 1223; break; // &copy -> &copys
				case 1237: state = 1238; break; // &CounterClockwi -> &CounterClockwis
				case 1262: state = 1263; break; // &Cro -> &Cros
				case 1263: state = 1264; break; // &Cros -> &Cross
				case 1266: state = 1267; break; // &cro -> &cros
				case 1267: state = 1268; break; // &cros -> &cross
				case 1301: state = 1305; break; // &cue -> &cues
				case 1318: state = 1344; break; // &cup -> &cups
				case 1356: state = 1362; break; // &curlyeq -> &curlyeqs
				case 1425: state = 2040; break; // &D -> &Ds
				case 1426: state = 1457; break; // &Da -> &Das
				case 1432: state = 2044; break; // &d -> &ds
				case 1433: state = 1454; break; // &da -> &das
				case 1511: state = 1512; break; // &ddot -> &ddots
				case 1536: state = 1537; break; // &dfi -> &dfis
				case 1599: state = 1639; break; // &di -> &dis
				case 1601: state = 1617; break; // &diam -> &diams
				case 1610: state = 1612; break; // &diamond -> &diamonds
				case 1654: state = 1655; break; // &divideontime -> &divideontimes
				case 1694: state = 1724; break; // &dot -> &dots
				case 1716: state = 1717; break; // &dotminu -> &dotminus
				case 1721: state = 1722; break; // &dotplu -> &dotplus
				case 1929: state = 1930; break; // &downdownarrow -> &downdownarrows
				case 2108: state = 2418; break; // &E -> &Es
				case 2115: state = 2422; break; // &e -> &es
				case 2116: state = 2122; break; // &ea -> &eas
				case 2185: state = 2198; break; // &eg -> &egs
				case 2204: state = 2222; break; // &el -> &els
				case 2217: state = 2218; break; // &elinter -> &elinters
				case 2233: state = 2279; break; // &em -> &ems
				case 2240: state = 2242; break; // &empty -> &emptys
				case 2290: state = 2293; break; // &en -> &ens
				case 2312: state = 2323; break; // &ep -> &eps
				case 2314: state = 2316; break; // &epar -> &epars
				case 2320: state = 2321; break; // &eplu -> &eplus
				case 2326: state = 2327; break; // &Ep -> &Eps
				case 2339: state = 2350; break; // &eq -> &eqs
				case 2363: state = 2364; break; // &eqslantle -> &eqslantles
				case 2364: state = 2365; break; // &eqslantles -> &eqslantless
				case 2374: state = 2375; break; // &equal -> &equals
				case 2383: state = 2384; break; // &eque -> &eques
				case 2405: state = 2406; break; // &eqvpar -> &eqvpars
				case 2462: state = 2463; break; // &exi -> &exis
				case 2467: state = 2468; break; // &Exi -> &Exis
				case 2469: state = 2470; break; // &Exist -> &Exists
				case 2503: state = 2697; break; // &f -> &fs
				case 2512: state = 2513; break; // &fallingdot -> &fallingdots
				case 2517: state = 2693; break; // &F -> &Fs
				case 2601: state = 2602; break; // &fltn -> &fltns
				case 2648: state = 2686; break; // &fra -> &fras
				case 2701: state = 2927; break; // &g -> &gs
				case 2708: state = 2923; break; // &G -> &Gs
				case 2765: state = 2781; break; // &ge -> &ges
				case 2771: state = 2775; break; // &geq -> &geqs
				case 2796: state = 2797; break; // &gesle -> &gesles
				case 2832: state = 2849; break; // &gn -> &gns
				case 2879: state = 2880; break; // &GreaterEqualLe -> &GreaterEqualLes
				case 2880: state = 2881; break; // &GreaterEqualLes -> &GreaterEqualLess
				case 2902: state = 2903; break; // &GreaterLe -> &GreaterLes
				case 2903: state = 2904; break; // &GreaterLes -> &GreaterLess
				case 2961: state = 2962; break; // &gtque -> &gtques
				case 2965: state = 2998; break; // &gtr -> &gtrs
				case 2983: state = 2984; break; // &gtreqle -> &gtreqles
				case 2984: state = 2985; break; // &gtreqles -> &gtreqless
				case 2989: state = 2990; break; // &gtreqqle -> &gtreqqles
				case 2990: state = 2991; break; // &gtreqqles -> &gtreqqless
				case 2994: state = 2995; break; // &gtrle -> &gtrles
				case 2995: state = 2996; break; // &gtrles -> &gtrless
				case 3014: state = 3184; break; // &H -> &Hs
				case 3020: state = 3188; break; // &h -> &hs
				case 3023: state = 3024; break; // &hair -> &hairs
				case 3077: state = 3078; break; // &heart -> &hearts
				case 3112: state = 3113; break; // &hk -> &hks
				case 3193: state = 3194; break; // &hsla -> &hslas
				case 3236: state = 3503; break; // &I -> &Is
				case 3243: state = 3507; break; // &i -> &is
				case 3375: state = 3376; break; // &Implie -> &Implies
				case 3410: state = 3411; break; // &integer -> &integers
				case 3424: state = 3425; break; // &Inter -> &Inters
				case 3445: state = 3446; break; // &Invi -> &Invis
				case 3460: state = 3461; break; // &InvisibleTime -> &InvisibleTimes
				case 3499: state = 3500; break; // &ique -> &iques
				case 3512: state = 3520; break; // &isin -> &isins
				case 3555: state = 3590; break; // &J -> &Js
				case 3561: state = 3594; break; // &j -> &js
				case 3618: state = 3684; break; // &K -> &Ks
				case 3624: state = 3688; break; // &k -> &ks
				case 3692: state = 4652; break; // &l -> &ls
				case 3698: state = 4658; break; // &L -> &Ls
				case 3766: state = 3785; break; // &larr -> &larrs
				case 3770: state = 3771; break; // &larrbf -> &larrbfs
				case 3773: state = 3774; break; // &larrf -> &larrfs
				case 3803: state = 3805; break; // &late -> &lates
				case 3828: state = 3831; break; // &lbrk -> &lbrks
				case 3869: state = 3891; break; // &ld -> &lds
				case 3885: state = 3886; break; // &ldru -> &ldrus
				case 3896: state = 4197; break; // &le -> &les
				case 3898: state = 4238; break; // &Le -> &Les
				case 4027: state = 4028; break; // &leftleftarrow -> &leftleftarrows
				case 4056: state = 4074; break; // &leftright -> &leftrights
				case 4061: state = 4063; break; // &leftrightarrow -> &leftrightarrows
				case 4071: state = 4072; break; // &leftrightharpoon -> &leftrightharpoons
				case 4117: state = 4118; break; // &leftthreetime -> &leftthreetimes
				case 4187: state = 4191; break; // &leq -> &leqs
				case 4197: state = 4215; break; // &les -> &less
				case 4212: state = 4213; break; // &lesge -> &lesges
				case 4215: state = 4280; break; // &less -> &lesss
				case 4238: state = 4239; break; // &Les -> &Less
				case 4276: state = 4277; break; // &LessLe -> &LessLes
				case 4277: state = 4278; break; // &LessLes -> &LessLess
				case 4302: state = 4303; break; // &lfi -> &lfis
				case 4392: state = 4393; break; // &lmou -> &lmous
				case 4401: state = 4418; break; // &ln -> &lns
				case 4504: state = 4505; break; // &longmap -> &longmaps
				case 4570: state = 4571; break; // &loplu -> &loplus
				case 4576: state = 4577; break; // &lotime -> &lotimes
				case 4580: state = 4581; break; // &lowa -> &lowas
				case 4717: state = 4718; break; // &ltime -> &ltimes
				case 4727: state = 4728; break; // &ltque -> &ltques
				case 4744: state = 4745; break; // &lurd -> &lurds
				case 4767: state = 4941; break; // &m -> &ms
				case 4777: state = 4778; break; // &malte -> &maltes
				case 4781: state = 4937; break; // &M -> &Ms
				case 4785: state = 4787; break; // &map -> &maps
				case 4821: state = 4822; break; // &mda -> &mdas
				case 4831: state = 4832; break; // &mea -> &meas
				case 4878: state = 4879; break; // &mida -> &midas
				case 4891: state = 4892; break; // &minu -> &minus
				case 4902: state = 4903; break; // &Minu -> &Minus
				case 4906: state = 4907; break; // &MinusPlu -> &MinusPlus
				case 4919: state = 4920; break; // &mnplu -> &mnplus
				case 4925: state = 4926; break; // &model -> &models
				case 4947: state = 4948; break; // &mstpo -> &mstpos
				case 4965: state = 5895; break; // &n -> &ns
				case 4971: state = 5904; break; // &N -> &Ns
				case 4993: state = 4994; break; // &napo -> &napos
				case 5006: state = 5008; break; // &natural -> &naturals
				case 5010: state = 5011; break; // &nb -> &nbs
				case 5060: state = 5061; break; // &nda -> &ndas
				case 5064: state = 5140; break; // &ne -> &nes
				case 5084: state = 5148; break; // &Ne -> &Nes
				case 5168: state = 5169; break; // &NestedLe -> &NestedLes
				case 5169: state = 5170; break; // &NestedLes -> &NestedLess
				case 5172: state = 5173; break; // &NestedLessLe -> &NestedLessLes
				case 5173: state = 5174; break; // &NestedLessLes -> &NestedLessLess
				case 5183: state = 5184; break; // &nexi -> &nexis
				case 5185: state = 5187; break; // &nexist -> &nexists
				case 5195: state = 5215; break; // &ng -> &ngs
				case 5198: state = 5210; break; // &nge -> &nges
				case 5200: state = 5204; break; // &ngeq -> &ngeqs
				case 5240: state = 5242; break; // &ni -> &nis
				case 5256: state = 5328; break; // &nl -> &nls
				case 5270: state = 5322; break; // &nle -> &nles
				case 5312: state = 5316; break; // &nleq -> &nleqs
				case 5322: state = 5324; break; // &nles -> &nless
				case 5434: state = 5435; break; // &NotExi -> &NotExis
				case 5436: state = 5437; break; // &NotExist -> &NotExists
				case 5472: state = 5473; break; // &NotGreaterLe -> &NotGreaterLes
				case 5473: state = 5474; break; // &NotGreaterLes -> &NotGreaterLess
				case 5529: state = 5551; break; // &NotLe -> &NotLes
				case 5551: state = 5552; break; // &NotLes -> &NotLess
				case 5569: state = 5570; break; // &NotLessLe -> &NotLessLes
				case 5570: state = 5571; break; // &NotLessLes -> &NotLessLess
				case 5591: state = 5592; break; // &NotNe -> &NotNes
				case 5612: state = 5613; break; // &NotNestedLe -> &NotNestedLes
				case 5613: state = 5614; break; // &NotNestedLes -> &NotNestedLess
				case 5616: state = 5617; break; // &NotNestedLessLe -> &NotNestedLessLes
				case 5617: state = 5618; break; // &NotNestedLessLes -> &NotNestedLessLess
				case 5636: state = 5637; break; // &NotPrecede -> &NotPrecedes
				case 5660: state = 5661; break; // &NotRever -> &NotRevers
				case 5702: state = 5703; break; // &NotSquareSub -> &NotSquareSubs
				case 5715: state = 5716; break; // &NotSquareSuper -> &NotSquareSupers
				case 5727: state = 5728; break; // &NotSub -> &NotSubs
				case 5742: state = 5743; break; // &NotSucceed -> &NotSucceeds
				case 5770: state = 5771; break; // &NotSuper -> &NotSupers
				case 5823: state = 5831; break; // &npar -> &npars
				case 5942: state = 5943; break; // &nsq -> &nsqs
				case 5952: state = 5958; break; // &nsub -> &nsubs
				case 5973: state = 5979; break; // &nsup -> &nsups
				case 6034: state = 6040; break; // &num -> &nums
				case 6043: state = 6107; break; // &nv -> &nvs
				case 6049: state = 6050; break; // &nVDa -> &nVDas
				case 6054: state = 6055; break; // &nVda -> &nVdas
				case 6059: state = 6060; break; // &nvDa -> &nvDas
				case 6064: state = 6065; break; // &nvda -> &nvdas
				case 6131: state = 6378; break; // &O -> &Os
				case 6138: state = 6382; break; // &o -> &os
				case 6139: state = 6145; break; // &oa -> &oas
				case 6163: state = 6185; break; // &od -> &ods
				case 6164: state = 6165; break; // &oda -> &odas
				case 6248: state = 6249; break; // &olcro -> &olcros
				case 6249: state = 6250; break; // &olcros -> &olcross
				case 6291: state = 6292; break; // &ominu -> &ominus
				case 6337: state = 6338; break; // &oplu -> &oplus
				case 6342: state = 6368; break; // &or -> &ors
				case 6387: state = 6388; break; // &Osla -> &Oslas
				case 6392: state = 6393; break; // &osla -> &oslas
				case 6412: state = 6413; break; // &Otime -> &Otimes
				case 6416: state = 6417; break; // &otime -> &otimes
				case 6419: state = 6420; break; // &otimesa -> &otimesas
				case 6458: state = 6459; break; // &OverParenthe -> &OverParenthes
				case 6460: state = 6461; break; // &OverParenthesi -> &OverParenthesis
				case 6463: state = 6799; break; // &p -> &ps
				case 6465: state = 6474; break; // &par -> &pars
				case 6482: state = 6795; break; // &P -> &Ps
				case 6566: state = 6567; break; // &plu -> &plus
				case 6567: state = 6599; break; // &plus -> &pluss
				case 6588: state = 6589; break; // &Plu -> &Plus
				case 6593: state = 6594; break; // &PlusMinu -> &PlusMinus
				case 6642: state = 6786; break; // &pr -> &prs
				case 6655: state = 6721; break; // &prec -> &precs
				case 6676: state = 6677; break; // &Precede -> &Precedes
				case 6705: state = 6717; break; // &precn -> &precns
				case 6731: state = 6733; break; // &prime -> &primes
				case 6735: state = 6741; break; // &prn -> &prns
				case 6754: state = 6765; break; // &prof -> &profs
				case 6809: state = 6810; break; // &punc -> &puncs
				case 6813: state = 6839; break; // &Q -> &Qs
				case 6817: state = 6843; break; // &q -> &qs
				case 6855: state = 6856; break; // &quaternion -> &quaternions
				case 6862: state = 6863; break; // &que -> &ques
				case 6876: state = 7542; break; // &r -> &rs
				case 6886: state = 7548; break; // &R -> &Rs
				case 6932: state = 6956; break; // &rarr -> &rarrs
				case 6939: state = 6940; break; // &rarrbf -> &rarrbfs
				case 6944: state = 6945; break; // &rarrf -> &rarrfs
				case 6983: state = 6984; break; // &rational -> &rationals
				case 7012: state = 7015; break; // &rbrk -> &rbrks
				case 7053: state = 7069; break; // &rd -> &rds
				case 7076: state = 7087; break; // &real -> &reals
				case 7099: state = 7100; break; // &Rever -> &Revers
				case 7136: state = 7137; break; // &rfi -> &rfis
				case 7199: state = 7431; break; // &ri -> &ris
				case 7202: state = 7326; break; // &right -> &rights
				case 7302: state = 7303; break; // &rightleftarrow -> &rightleftarrows
				case 7311: state = 7312; break; // &rightleftharpoon -> &rightleftharpoons
				case 7323: state = 7324; break; // &rightrightarrow -> &rightrightarrows
				case 7362: state = 7363; break; // &rightthreetime -> &rightthreetimes
				case 7437: state = 7438; break; // &risingdot -> &risingdots
				case 7455: state = 7456; break; // &rmou -> &rmous
				case 7492: state = 7493; break; // &roplu -> &roplus
				case 7498: state = 7499; break; // &rotime -> &rotimes
				case 7509: state = 7510; break; // &RoundImplie -> &RoundImplies
				case 7575: state = 7576; break; // &rtime -> &rtimes
				case 7610: state = 8073; break; // &S -> &Ss
				case 7617: state = 8077; break; // &s -> &ss
				case 7631: state = 7687; break; // &sc -> &scs
				case 7670: state = 7676; break; // &scn -> &scns
				case 7703: state = 7724; break; // &se -> &ses
				case 7733: state = 7734; break; // &setminu -> &setminus
				case 7870: state = 7871; break; // &simplu -> &simplus
				case 7895: state = 7907; break; // &sma -> &smas
				case 7897: state = 7898; break; // &small -> &smalls
				case 7904: state = 7905; break; // &smallsetminu -> &smallsetminus
				case 7914: state = 7915; break; // &smepar -> &smepars
				case 7926: state = 7928; break; // &smte -> &smtes
				case 7959: state = 7960; break; // &spade -> &spades
				case 7968: state = 7984; break; // &sq -> &sqs
				case 7971: state = 7973; break; // &sqcap -> &sqcaps
				case 7976: state = 7978; break; // &sqcup -> &sqcups
				case 7986: state = 7990; break; // &sqsub -> &sqsubs
				case 7997: state = 8001; break; // &sqsup -> &sqsups
				case 8023: state = 8024; break; // &SquareInter -> &SquareInters
				case 8034: state = 8035; break; // &SquareSub -> &SquareSubs
				case 8047: state = 8048; break; // &SquareSuper -> &SquareSupers
				case 8113: state = 8114; break; // &straightep -> &straighteps
				case 8124: state = 8125; break; // &strn -> &strns
				case 8128: state = 8165; break; // &Sub -> &Subs
				case 8131: state = 8169; break; // &sub -> &subs
				case 8157: state = 8158; break; // &subplu -> &subplus
				case 8199: state = 8265; break; // &succ -> &succs
				case 8220: state = 8221; break; // &Succeed -> &Succeeds
				case 8249: state = 8261; break; // &succn -> &succns
				case 8282: state = 8348; break; // &Sup -> &Sups
				case 8284: state = 8352; break; // &sup -> &sups
				case 8292: state = 8296; break; // &supd -> &supds
				case 8309: state = 8310; break; // &Super -> &Supers
				case 8320: state = 8321; break; // &suph -> &suphs
				case 8345: state = 8346; break; // &supplu -> &supplus
				case 8400: state = 8705; break; // &T -> &Ts
				case 8404: state = 8709; break; // &t -> &ts
				case 8485: state = 8487; break; // &theta -> &thetas
				case 8495: state = 8503; break; // &thick -> &thicks
				case 8516: state = 8517; break; // &thin -> &thins
				case 8527: state = 8531; break; // &thk -> &thks
				case 8577: state = 8578; break; // &time -> &times
				case 8590: state = 8614; break; // &to -> &tos
				case 8633: state = 8690; break; // &tri -> &tris
				case 8673: state = 8674; break; // &triminu -> &triminus
				case 8687: state = 8688; break; // &triplu -> &triplus
				case 8768: state = 9153; break; // &U -> &Us
				case 8775: state = 9157; break; // &u -> &us
				case 8850: state = 8851; break; // &ufi -> &ufis
				case 8940: state = 8941; break; // &UnderParenthe -> &UnderParenthes
				case 8942: state = 8943; break; // &UnderParenthesi -> &UnderParenthesis
				case 8951: state = 8952; break; // &UnionPlu -> &UnionPlus
				case 8970: state = 9092; break; // &Up -> &Ups
				case 8983: state = 9095; break; // &up -> &ups
				case 9065: state = 9066; break; // &uplu -> &uplus
				case 9124: state = 9125; break; // &upuparrow -> &upuparrows
				case 9201: state = 9454; break; // &v -> &vs
				case 9208: state = 9252; break; // &var -> &vars
				case 9210: state = 9211; break; // &varep -> &vareps
				case 9259: state = 9260; break; // &varsub -> &varsubs
				case 9269: state = 9270; break; // &varsup -> &varsups
				case 9303: state = 9450; break; // &V -> &Vs
				case 9321: state = 9322; break; // &VDa -> &VDas
				case 9326: state = 9327; break; // &Vda -> &Vdas
				case 9331: state = 9332; break; // &vDa -> &vDas
				case 9336: state = 9337; break; // &vda -> &vdas
				case 9425: state = 9426; break; // &vn -> &vns
				case 9473: state = 9474; break; // &Vvda -> &Vvdas
				case 9484: state = 9540; break; // &W -> &Ws
				case 9490: state = 9544; break; // &w -> &ws
				case 9548: state = 9636; break; // &x -> &xs
				case 9565: state = 9632; break; // &X -> &Xs
				case 9599: state = 9600; break; // &xni -> &xnis
				case 9615: state = 9616; break; // &xoplu -> &xoplus
				case 9648: state = 9649; break; // &xuplu -> &xuplus
				case 9665: state = 9724; break; // &Y -> &Ys
				case 9672: state = 9728; break; // &y -> &ys
				case 9747: state = 9840; break; // &Z -> &Zs
				case 9754: state = 9844; break; // &z -> &zs
				default: return false;
				}
				break;
			case 't':
				switch (state) {
				case 0: state = 8404; break; // & -> &t
				case 1: state = 269; break; // &A -> &At
				case 4: state = 5; break; // &Aacu -> &Aacut
				case 8: state = 275; break; // &a -> &at
				case 11: state = 12; break; // &aacu -> &aacut
				case 42: state = 43; break; // &acu -> &acut
				case 164: state = 165; break; // &angr -> &angrt
				case 172: state = 176; break; // &angs -> &angst
				case 223: state = 224; break; // &ApplyFunc -> &ApplyFunct
				case 251: state = 260; break; // &as -> &ast
				case 294: state = 295; break; // &awconin -> &awconint
				case 298: state = 299; break; // &awin -> &awint
				case 362: state = 364; break; // &bbrk -> &bbrkt
				case 384: state = 426; break; // &be -> &bet
				case 390: state = 423; break; // &Be -> &Bet
				case 400: state = 401; break; // &bemp -> &bempt
				case 443: state = 481; break; // &big -> &bigt
				case 455: state = 465; break; // &bigo -> &bigot
				case 457: state = 458; break; // &bigodo -> &bigodot
				case 471: state = 477; break; // &bigs -> &bigst
				case 522: state = 538; break; // &black -> &blackt
				case 554: state = 555; break; // &blacktrianglelef -> &blacktriangleleft
				case 560: state = 561; break; // &blacktrianglerigh -> &blacktriangleright
				case 588: state = 589; break; // &bNo -> &bNot
				case 591: state = 592; break; // &bno -> &bnot
				case 598: state = 602; break; // &bo -> &bot
				case 602: state = 604; break; // &bot -> &bott
				case 608: state = 609; break; // &bow -> &bowt
				case 613: state = 667; break; // &box -> &boxt
				case 771: state = 772; break; // &bulle -> &bullet
				case 792: state = 793; break; // &Cacu -> &Cacut
				case 796: state = 1287; break; // &c -> &ct
				case 799: state = 800; break; // &cacu -> &cacut
				case 825: state = 826; break; // &capdo -> &capdot
				case 828: state = 829; break; // &Capi -> &Capit
				case 839: state = 840; break; // &CapitalDifferen -> &CapitalDifferent
				case 849: state = 850; break; // &care -> &caret
				case 897: state = 898; break; // &Cconin -> &Cconint
				case 908: state = 909; break; // &Cdo -> &Cdot
				case 912: state = 913; break; // &cdo -> &cdot
				case 928: state = 929; break; // &cemp -> &cempt
				case 933: state = 934; break; // &cen -> &cent
				case 936: state = 937; break; // &Cen -> &Cent
				case 941: state = 942; break; // &CenterDo -> &CenterDot
				case 947: state = 948; break; // &centerdo -> &centerdot
				case 995: state = 996; break; // &circlearrowlef -> &circlearrowleft
				case 1001: state = 1002; break; // &circlearrowrigh -> &circlearrowright
				case 1006: state = 1007; break; // &circledas -> &circledast
				case 1025: state = 1026; break; // &CircleDo -> &CircleDot
				case 1056: state = 1057; break; // &cirfnin -> &cirfnint
				case 1078: state = 1079; break; // &ClockwiseCon -> &ClockwiseCont
				case 1084: state = 1085; break; // &ClockwiseContourIn -> &ClockwiseContourInt
				case 1107: state = 1108; break; // &CloseCurlyDoubleQuo -> &CloseCurlyDoubleQuot
				case 1113: state = 1114; break; // &CloseCurlyQuo -> &CloseCurlyQuot
				case 1123: state = 1124; break; // &clubsui -> &clubsuit
				case 1144: state = 1146; break; // &comma -> &commat
				case 1157: state = 1158; break; // &complemen -> &complement
				case 1168: state = 1169; break; // &congdo -> &congdot
				case 1171: state = 1187; break; // &Con -> &Cont
				case 1176: state = 1177; break; // &Congruen -> &Congruent
				case 1180: state = 1181; break; // &Conin -> &Conint
				case 1184: state = 1185; break; // &conin -> &conint
				case 1192: state = 1193; break; // &ContourIn -> &ContourInt
				case 1214: state = 1215; break; // &Coproduc -> &Coproduct
				case 1227: state = 1228; break; // &Coun -> &Count
				case 1242: state = 1243; break; // &CounterClockwiseCon -> &CounterClockwiseCont
				case 1248: state = 1249; break; // &CounterClockwiseContourIn -> &CounterClockwiseContourInt
				case 1289: state = 1290; break; // &ctdo -> &ctdot
				case 1338: state = 1339; break; // &cupdo -> &cupdot
				case 1390: state = 1391; break; // &curvearrowlef -> &curvearrowleft
				case 1396: state = 1397; break; // &curvearrowrigh -> &curvearrowright
				case 1412: state = 1413; break; // &cwconin -> &cwconint
				case 1416: state = 1417; break; // &cwin -> &cwint
				case 1421: state = 1422; break; // &cylc -> &cylct
				case 1432: state = 2067; break; // &d -> &dt
				case 1440: state = 1441; break; // &dale -> &dalet
				case 1503: state = 1504; break; // &DDo -> &DDot
				case 1510: state = 1511; break; // &ddo -> &ddot
				case 1520: state = 1522; break; // &Del -> &Delt
				case 1525: state = 1526; break; // &del -> &delt
				case 1530: state = 1531; break; // &demp -> &dempt
				case 1538: state = 1539; break; // &dfish -> &dfisht
				case 1561: state = 1562; break; // &Diacri -> &Diacrit
				case 1569: state = 1570; break; // &DiacriticalAcu -> &DiacriticalAcut
				case 1574: state = 1575; break; // &DiacriticalDo -> &DiacriticalDot
				case 1583: state = 1584; break; // &DiacriticalDoubleAcu -> &DiacriticalDoubleAcut
				case 1614: state = 1615; break; // &diamondsui -> &diamondsuit
				case 1626: state = 1627; break; // &Differen -> &Different
				case 1650: state = 1651; break; // &divideon -> &divideont
				case 1679: state = 1694; break; // &do -> &dot
				case 1685: state = 1692; break; // &Do -> &Dot
				case 1697: state = 1698; break; // &DotDo -> &DotDot
				case 1704: state = 1705; break; // &doteqdo -> &doteqdot
				case 1750: state = 1751; break; // &DoubleCon -> &DoubleCont
				case 1756: state = 1757; break; // &DoubleContourIn -> &DoubleContourInt
				case 1765: state = 1766; break; // &DoubleDo -> &DoubleDot
				case 1778: state = 1779; break; // &DoubleLef -> &DoubleLeft
				case 1789: state = 1790; break; // &DoubleLeftRigh -> &DoubleLeftRight
				case 1806: state = 1807; break; // &DoubleLongLef -> &DoubleLongLeft
				case 1817: state = 1818; break; // &DoubleLongLeftRigh -> &DoubleLongLeftRight
				case 1828: state = 1829; break; // &DoubleLongRigh -> &DoubleLongRight
				case 1839: state = 1840; break; // &DoubleRigh -> &DoubleRight
				case 1871: state = 1872; break; // &DoubleVer -> &DoubleVert
				case 1941: state = 1942; break; // &downharpoonlef -> &downharpoonleft
				case 1947: state = 1948; break; // &downharpoonrigh -> &downharpoonright
				case 1952: state = 1953; break; // &DownLef -> &DownLeft
				case 1957: state = 1958; break; // &DownLeftRigh -> &DownLeftRight
				case 1961: state = 1962; break; // &DownLeftRightVec -> &DownLeftRightVect
				case 1971: state = 1972; break; // &DownLeftTeeVec -> &DownLeftTeeVect
				case 1978: state = 1979; break; // &DownLeftVec -> &DownLeftVect
				case 1990: state = 1991; break; // &DownRigh -> &DownRight
				case 1997: state = 1998; break; // &DownRightTeeVec -> &DownRightTeeVect
				case 2004: state = 2005; break; // &DownRightVec -> &DownRightVect
				case 2040: state = 2057; break; // &Ds -> &Dst
				case 2044: state = 2062; break; // &ds -> &dst
				case 2069: state = 2070; break; // &dtdo -> &dtdot
				case 2108: state = 2436; break; // &E -> &Et
				case 2111: state = 2112; break; // &Eacu -> &Eacut
				case 2115: state = 2439; break; // &e -> &et
				case 2118: state = 2119; break; // &eacu -> &eacut
				case 2122: state = 2123; break; // &eas -> &east
				case 2159: state = 2160; break; // &eDDo -> &eDDot
				case 2163: state = 2164; break; // &Edo -> &Edot
				case 2166: state = 2167; break; // &eDo -> &eDot
				case 2170: state = 2171; break; // &edo -> &edot
				case 2177: state = 2178; break; // &efDo -> &efDot
				case 2201: state = 2202; break; // &egsdo -> &egsdot
				case 2210: state = 2211; break; // &Elemen -> &Element
				case 2214: state = 2215; break; // &elin -> &elint
				case 2225: state = 2226; break; // &elsdo -> &elsdot
				case 2238: state = 2239; break; // &emp -> &empt
				case 2243: state = 2244; break; // &emptyse -> &emptyset
				case 2246: state = 2247; break; // &Emp -> &Empt
				case 2356: state = 2357; break; // &eqslan -> &eqslant
				case 2358: state = 2359; break; // &eqslantg -> &eqslantgt
				case 2384: state = 2385; break; // &eques -> &equest
				case 2415: state = 2416; break; // &erDo -> &erDot
				case 2427: state = 2428; break; // &esdo -> &esdot
				case 2463: state = 2464; break; // &exis -> &exist
				case 2468: state = 2469; break; // &Exis -> &Exist
				case 2474: state = 2475; break; // &expec -> &expect
				case 2476: state = 2477; break; // &expecta -> &expectat
				case 2486: state = 2487; break; // &Exponen -> &Exponent
				case 2496: state = 2497; break; // &exponen -> &exponent
				case 2511: state = 2512; break; // &fallingdo -> &fallingdot
				case 2592: state = 2600; break; // &fl -> &flt
				case 2593: state = 2594; break; // &fla -> &flat
				case 2634: state = 2635; break; // &Fourier -> &Fouriert
				case 2641: state = 2642; break; // &fpar -> &fpart
				case 2644: state = 2645; break; // &fpartin -> &fpartint
				case 2701: state = 2942; break; // &g -> &gt
				case 2704: state = 2705; break; // &gacu -> &gacut
				case 2708: state = 2940; break; // &G -> &Gt
				case 2756: state = 2757; break; // &Gdo -> &Gdot
				case 2760: state = 2761; break; // &gdo -> &gdot
				case 2778: state = 2779; break; // &geqslan -> &geqslant
				case 2787: state = 2788; break; // &gesdo -> &gesdot
				case 2868: state = 2869; break; // &Grea -> &Great
				case 2896: state = 2897; break; // &GreaterGrea -> &GreaterGreat
				case 2909: state = 2910; break; // &GreaterSlan -> &GreaterSlant
				case 2951: state = 2952; break; // &gtdo -> &gtdot
				case 2962: state = 2963; break; // &gtques -> &gtquest
				case 2977: state = 2978; break; // &gtrdo -> &gtrdot
				case 3004: state = 3005; break; // &gver -> &gvert
				case 3015: state = 3058; break; // &Ha -> &Hat
				case 3032: state = 3033; break; // &hamil -> &hamilt
				case 3076: state = 3077; break; // &hear -> &heart
				case 3081: state = 3082; break; // &heartsui -> &heartsuit
				case 3104: state = 3105; break; // &Hilber -> &Hilbert
				case 3131: state = 3132; break; // &hom -> &homt
				case 3133: state = 3134; break; // &homth -> &homtht
				case 3140: state = 3141; break; // &hooklef -> &hookleft
				case 3151: state = 3152; break; // &hookrigh -> &hookright
				case 3175: state = 3176; break; // &Horizon -> &Horizont
				case 3184: state = 3197; break; // &Hs -> &Hst
				case 3188: state = 3202; break; // &hs -> &hst
				case 3236: state = 3528; break; // &I -> &It
				case 3239: state = 3240; break; // &Iacu -> &Iacut
				case 3243: state = 3526; break; // &i -> &it
				case 3246: state = 3247; break; // &iacu -> &iacut
				case 3266: state = 3267; break; // &Ido -> &Idot
				case 3305: state = 3306; break; // &iiiin -> &iiiint
				case 3308: state = 3309; break; // &iiin -> &iiint
				case 3316: state = 3317; break; // &iio -> &iiot
				case 3337: state = 3362; break; // &ima -> &imat
				case 3359: state = 3360; break; // &imagpar -> &imagpart
				case 3378: state = 3401; break; // &in -> &int
				case 3387: state = 3389; break; // &infin -> &infint
				case 3395: state = 3396; break; // &inodo -> &inodot
				case 3398: state = 3399; break; // &In -> &Int
				case 3427: state = 3428; break; // &Intersec -> &Intersect
				case 3467: state = 3489; break; // &io -> &iot
				case 3471: state = 3486; break; // &Io -> &Iot
				case 3500: state = 3501; break; // &iques -> &iquest
				case 3515: state = 3516; break; // &isindo -> &isindot
				case 3578: state = 3579; break; // &jma -> &jmat
				case 3692: state = 4698; break; // &l -> &lt
				case 3693: state = 3794; break; // &lA -> &lAt
				case 3698: state = 4696; break; // &L -> &Lt
				case 3701: state = 3702; break; // &Lacu -> &Lacut
				case 3705: state = 3792; break; // &la -> &lat
				case 3707: state = 3708; break; // &lacu -> &lacut
				case 3713: state = 3714; break; // &laemp -> &laempt
				case 3750: state = 3751; break; // &Laplace -> &Laplacet
				case 3766: state = 3789; break; // &larr -> &larrt
				case 3899: state = 3900; break; // &Lef -> &Left
				case 3911: state = 3912; break; // &LeftAngleBracke -> &LeftAngleBracket
				case 3925: state = 3926; break; // &lef -> &left
				case 3926: state = 4109; break; // &left -> &leftt
				case 3931: state = 3948; break; // &leftarrow -> &leftarrowt
				case 3940: state = 3941; break; // &LeftArrowRigh -> &LeftArrowRight
				case 3972: state = 3973; break; // &LeftDoubleBracke -> &LeftDoubleBracket
				case 3982: state = 3983; break; // &LeftDownTeeVec -> &LeftDownTeeVect
				case 3989: state = 3990; break; // &LeftDownVec -> &LeftDownVect
				case 4021: state = 4022; break; // &leftlef -> &leftleft
				case 4033: state = 4034; break; // &LeftRigh -> &LeftRight
				case 4044: state = 4045; break; // &Leftrigh -> &Leftright
				case 4055: state = 4056; break; // &leftrigh -> &leftright
				case 4087: state = 4088; break; // &LeftRightVec -> &LeftRightVect
				case 4104: state = 4105; break; // &LeftTeeVec -> &LeftTeeVect
				case 4113: state = 4114; break; // &leftthree -> &leftthreet
				case 4146: state = 4147; break; // &LeftUpDownVec -> &LeftUpDownVect
				case 4156: state = 4157; break; // &LeftUpTeeVec -> &LeftUpTeeVect
				case 4163: state = 4164; break; // &LeftUpVec -> &LeftUpVect
				case 4174: state = 4175; break; // &LeftVec -> &LeftVect
				case 4194: state = 4195; break; // &leqslan -> &leqslant
				case 4203: state = 4204; break; // &lesdo -> &lesdot
				case 4224: state = 4225; break; // &lessdo -> &lessdot
				case 4229: state = 4230; break; // &lesseqg -> &lesseqgt
				case 4234: state = 4235; break; // &lesseqqg -> &lesseqqgt
				case 4248: state = 4249; break; // &LessEqualGrea -> &LessEqualGreat
				case 4266: state = 4267; break; // &LessGrea -> &LessGreat
				case 4271: state = 4272; break; // &lessg -> &lessgt
				case 4287: state = 4288; break; // &LessSlan -> &LessSlant
				case 4304: state = 4305; break; // &lfish -> &lfisht
				case 4348: state = 4375; break; // &ll -> &llt
				case 4362: state = 4363; break; // &Llef -> &Lleft
				case 4382: state = 4383; break; // &Lmido -> &Lmidot
				case 4388: state = 4389; break; // &lmido -> &lmidot
				case 4393: state = 4394; break; // &lmous -> &lmoust
				case 4422: state = 4573; break; // &lo -> &lot
				case 4439: state = 4440; break; // &LongLef -> &LongLeft
				case 4449: state = 4450; break; // &Longlef -> &Longleft
				case 4461: state = 4462; break; // &longlef -> &longleft
				case 4472: state = 4473; break; // &LongLeftRigh -> &LongLeftRight
				case 4483: state = 4484; break; // &Longleftrigh -> &Longleftright
				case 4494: state = 4495; break; // &longleftrigh -> &longleftright
				case 4505: state = 4506; break; // &longmaps -> &longmapst
				case 4512: state = 4513; break; // &LongRigh -> &LongRight
				case 4523: state = 4524; break; // &Longrigh -> &Longright
				case 4534: state = 4535; break; // &longrigh -> &longright
				case 4551: state = 4552; break; // &looparrowlef -> &looparrowleft
				case 4557: state = 4558; break; // &looparrowrigh -> &looparrowright
				case 4581: state = 4582; break; // &lowas -> &lowast
				case 4593: state = 4594; break; // &LowerLef -> &LowerLeft
				case 4604: state = 4605; break; // &LowerRigh -> &LowerRight
				case 4625: state = 4626; break; // &lparl -> &lparlt
				case 4628: state = 4648; break; // &lr -> &lrt
				case 4652: state = 4689; break; // &ls -> &lst
				case 4658: state = 4684; break; // &Ls -> &Lst
				case 4707: state = 4708; break; // &ltdo -> &ltdot
				case 4728: state = 4729; break; // &ltques -> &ltquest
				case 4757: state = 4758; break; // &lver -> &lvert
				case 4772: state = 4775; break; // &mal -> &malt
				case 4787: state = 4788; break; // &maps -> &mapst
				case 4798: state = 4799; break; // &mapstolef -> &mapstoleft
				case 4827: state = 4828; break; // &mDDo -> &mDDot
				case 4857: state = 4858; break; // &Mellin -> &Mellint
				case 4879: state = 4880; break; // &midas -> &midast
				case 4887: state = 4888; break; // &middo -> &middot
				case 4941: state = 4945; break; // &ms -> &mst
				case 4954: state = 4955; break; // &mul -> &mult
				case 4965: state = 5988; break; // &n -> &nt
				case 4966: state = 5001; break; // &na -> &nat
				case 4971: state = 5992; break; // &N -> &Nt
				case 4974: state = 4975; break; // &Nacu -> &Nacut
				case 4979: state = 4980; break; // &nacu -> &nacut
				case 5049: state = 5050; break; // &ncongdo -> &ncongdot
				case 5081: state = 5082; break; // &nedo -> &nedot
				case 5086: state = 5087; break; // &Nega -> &Negat
				case 5148: state = 5149; break; // &Nes -> &Nest
				case 5155: state = 5156; break; // &NestedGrea -> &NestedGreat
				case 5162: state = 5163; break; // &NestedGreaterGrea -> &NestedGreaterGreat
				case 5184: state = 5185; break; // &nexis -> &nexist
				case 5195: state = 5221; break; // &ng -> &ngt
				case 5207: state = 5208; break; // &ngeqslan -> &ngeqslant
				case 5212: state = 5219; break; // &nG -> &nGt
				case 5256: state = 5334; break; // &nl -> &nlt
				case 5272: state = 5332; break; // &nL -> &nLt
				case 5274: state = 5275; break; // &nLef -> &nLeft
				case 5282: state = 5283; break; // &nlef -> &nleft
				case 5293: state = 5294; break; // &nLeftrigh -> &nLeftright
				case 5304: state = 5305; break; // &nleftrigh -> &nleftright
				case 5319: state = 5320; break; // &nleqslan -> &nleqslant
				case 5347: state = 5376; break; // &No -> &Not
				case 5372: state = 5378; break; // &no -> &not
				case 5387: state = 5388; break; // &NotCongruen -> &NotCongruent
				case 5404: state = 5405; break; // &NotDoubleVer -> &NotDoubleVert
				case 5419: state = 5420; break; // &NotElemen -> &NotElement
				case 5435: state = 5436; break; // &NotExis -> &NotExist
				case 5442: state = 5443; break; // &NotGrea -> &NotGreat
				case 5466: state = 5467; break; // &NotGreaterGrea -> &NotGreaterGreat
				case 5479: state = 5480; break; // &NotGreaterSlan -> &NotGreaterSlant
				case 5516: state = 5517; break; // &notindo -> &notindot
				case 5530: state = 5531; break; // &NotLef -> &NotLeft
				case 5563: state = 5564; break; // &NotLessGrea -> &NotLessGreat
				case 5576: state = 5577; break; // &NotLessSlan -> &NotLessSlant
				case 5592: state = 5593; break; // &NotNes -> &NotNest
				case 5599: state = 5600; break; // &NotNestedGrea -> &NotNestedGreat
				case 5606: state = 5607; break; // &NotNestedGreaterGrea -> &NotNestedGreaterGreat
				case 5648: state = 5649; break; // &NotPrecedesSlan -> &NotPrecedesSlant
				case 5668: state = 5669; break; // &NotReverseElemen -> &NotReverseElement
				case 5673: state = 5674; break; // &NotRigh -> &NotRight
				case 5704: state = 5705; break; // &NotSquareSubse -> &NotSquareSubset
				case 5717: state = 5718; break; // &NotSquareSuperse -> &NotSquareSuperset
				case 5729: state = 5730; break; // &NotSubse -> &NotSubset
				case 5754: state = 5755; break; // &NotSucceedsSlan -> &NotSucceedsSlant
				case 5772: state = 5773; break; // &NotSuperse -> &NotSuperset
				case 5811: state = 5812; break; // &NotVer -> &NotVert
				case 5823: state = 5834; break; // &npar -> &npart
				case 5839: state = 5840; break; // &npolin -> &npolint
				case 5855: state = 5889; break; // &nr -> &nrt
				case 5871: state = 5872; break; // &nRigh -> &nRight
				case 5881: state = 5882; break; // &nrigh -> &nright
				case 5912: state = 5913; break; // &nshor -> &nshort
				case 5959: state = 5960; break; // &nsubse -> &nsubset
				case 5980: state = 5981; break; // &nsupse -> &nsupset
				case 6015: state = 6016; break; // &ntrianglelef -> &ntriangleleft
				case 6024: state = 6025; break; // &ntrianglerigh -> &ntriangleright
				case 6068: state = 6071; break; // &nvg -> &nvgt
				case 6084: state = 6091; break; // &nvl -> &nvlt
				case 6097: state = 6102; break; // &nvr -> &nvrt
				case 6131: state = 6399; break; // &O -> &Ot
				case 6134: state = 6135; break; // &Oacu -> &Oacut
				case 6138: state = 6405; break; // &o -> &ot
				case 6141: state = 6142; break; // &oacu -> &oacut
				case 6145: state = 6146; break; // &oas -> &oast
				case 6182: state = 6183; break; // &odo -> &odot
				case 6210: state = 6225; break; // &og -> &ogt
				case 6235: state = 6236; break; // &oin -> &oint
				case 6238: state = 6256; break; // &ol -> &olt
				case 6322: state = 6323; break; // &OpenCurlyDoubleQuo -> &OpenCurlyDoubleQuot
				case 6328: state = 6329; break; // &OpenCurlyQuo -> &OpenCurlyQuot
				case 6448: state = 6449; break; // &OverBracke -> &OverBracket
				case 6455: state = 6456; break; // &OverParen -> &OverParent
				case 6465: state = 6480; break; // &par -> &part
				case 6484: state = 6485; break; // &Par -> &Part
				case 6498: state = 6513; break; // &per -> &pert
				case 6500: state = 6501; break; // &percn -> &percnt
				case 6534: state = 6535; break; // &phmma -> &phmmat
				case 6543: state = 6545; break; // &pi -> &pit
				case 6567: state = 6603; break; // &plus -> &plust
				case 6624: state = 6625; break; // &poin -> &point
				case 6627: state = 6628; break; // &pointin -> &pointint
				case 6688: state = 6689; break; // &PrecedesSlan -> &PrecedesSlant
				case 6751: state = 6752; break; // &Produc -> &Product
				case 6770: state = 6783; break; // &prop -> &propt
				case 6774: state = 6775; break; // &Propor -> &Proport
				case 6822: state = 6823; break; // &qin -> &qint
				case 6848: state = 6849; break; // &qua -> &quat
				case 6859: state = 6860; break; // &quatin -> &quatint
				case 6863: state = 6864; break; // &ques -> &quest
				case 6873: state = 6874; break; // &quo -> &quot
				case 6876: state = 7567; break; // &r -> &rt
				case 6877: state = 6968; break; // &rA -> &rAt
				case 6882: state = 6973; break; // &ra -> &rat
				case 6889: state = 6890; break; // &Racu -> &Racut
				case 6893: state = 6894; break; // &racu -> &racut
				case 6903: state = 6904; break; // &raemp -> &raempt
				case 6926: state = 6960; break; // &Rarr -> &Rarrt
				case 6932: state = 6963; break; // &rarr -> &rarrt
				case 7084: state = 7085; break; // &realpar -> &realpart
				case 7089: state = 7090; break; // &rec -> &rect
				case 7107: state = 7108; break; // &ReverseElemen -> &ReverseElement
				case 7138: state = 7139; break; // &rfish -> &rfisht
				case 7173: state = 7174; break; // &Righ -> &Right
				case 7185: state = 7186; break; // &RightAngleBracke -> &RightAngleBracket
				case 7201: state = 7202; break; // &righ -> &right
				case 7202: state = 7354; break; // &right -> &rightt
				case 7207: state = 7223; break; // &rightarrow -> &rightarrowt
				case 7215: state = 7216; break; // &RightArrowLef -> &RightArrowLeft
				case 7247: state = 7248; break; // &RightDoubleBracke -> &RightDoubleBracket
				case 7257: state = 7258; break; // &RightDownTeeVec -> &RightDownTeeVect
				case 7264: state = 7265; break; // &RightDownVec -> &RightDownVect
				case 7296: state = 7297; break; // &rightlef -> &rightleft
				case 7317: state = 7318; break; // &rightrigh -> &rightright
				case 7349: state = 7350; break; // &RightTeeVec -> &RightTeeVect
				case 7358: state = 7359; break; // &rightthree -> &rightthreet
				case 7391: state = 7392; break; // &RightUpDownVec -> &RightUpDownVect
				case 7401: state = 7402; break; // &RightUpTeeVec -> &RightUpTeeVect
				case 7408: state = 7409; break; // &RightUpVec -> &RightUpVect
				case 7419: state = 7420; break; // &RightVec -> &RightVect
				case 7436: state = 7437; break; // &risingdo -> &risingdot
				case 7456: state = 7457; break; // &rmous -> &rmoust
				case 7469: state = 7495; break; // &ro -> &rot
				case 7516: state = 7517; break; // &rparg -> &rpargt
				case 7523: state = 7524; break; // &rppolin -> &rppolint
				case 7534: state = 7535; break; // &Rrigh -> &Rright
				case 7585: state = 7586; break; // &rtril -> &rtrilt
				case 7610: state = 8096; break; // &S -> &St
				case 7613: state = 7614; break; // &Sacu -> &Sacut
				case 7617: state = 8100; break; // &s -> &st
				case 7620: state = 7621; break; // &sacu -> &sacut
				case 7684: state = 7685; break; // &scpolin -> &scpolint
				case 7696: state = 7697; break; // &sdo -> &sdot
				case 7703: state = 7729; break; // &se -> &set
				case 7718: state = 7719; break; // &sec -> &sect
				case 7738: state = 7739; break; // &sex -> &sext
				case 7774: state = 7775; break; // &Shor -> &Short
				case 7788: state = 7789; break; // &ShortLef -> &ShortLeft
				case 7797: state = 7798; break; // &shor -> &short
				case 7815: state = 7816; break; // &ShortRigh -> &ShortRight
				case 7850: state = 7851; break; // &simdo -> &simdot
				case 7894: state = 7924; break; // &sm -> &smt
				case 7899: state = 7900; break; // &smallse -> &smallset
				case 7937: state = 7938; break; // &sof -> &soft
				case 7963: state = 7964; break; // &spadesui -> &spadesuit
				case 7981: state = 7982; break; // &Sqr -> &Sqrt
				case 7991: state = 7992; break; // &sqsubse -> &sqsubset
				case 8002: state = 8003; break; // &sqsupse -> &sqsupset
				case 8020: state = 8021; break; // &SquareIn -> &SquareInt
				case 8026: state = 8027; break; // &SquareIntersec -> &SquareIntersect
				case 8036: state = 8037; break; // &SquareSubse -> &SquareSubset
				case 8049: state = 8050; break; // &SquareSuperse -> &SquareSuperset
				case 8077: state = 8091; break; // &ss -> &sst
				case 8081: state = 8082; break; // &sse -> &sset
				case 8110: state = 8111; break; // &straigh -> &straight
				case 8134: state = 8135; break; // &subdo -> &subdot
				case 8142: state = 8143; break; // &subedo -> &subedot
				case 8147: state = 8148; break; // &submul -> &submult
				case 8166: state = 8167; break; // &Subse -> &Subset
				case 8170: state = 8171; break; // &subse -> &subset
				case 8232: state = 8233; break; // &SucceedsSlan -> &SucceedsSlant
				case 8272: state = 8273; break; // &SuchTha -> &SuchThat
				case 8293: state = 8294; break; // &supdo -> &supdot
				case 8305: state = 8306; break; // &supedo -> &supedot
				case 8311: state = 8312; break; // &Superse -> &Superset
				case 8335: state = 8336; break; // &supmul -> &supmult
				case 8349: state = 8350; break; // &Supse -> &Supset
				case 8353: state = 8354; break; // &supse -> &supset
				case 8408: state = 8409; break; // &targe -> &target
				case 8446: state = 8447; break; // &tdo -> &tdot
				case 8462: state = 8484; break; // &the -> &thet
				case 8468: state = 8481; break; // &The -> &Thet
				case 8587: state = 8588; break; // &tin -> &tint
				case 8597: state = 8598; break; // &topbo -> &topbot
				case 8633: state = 8693; break; // &tri -> &trit
				case 8647: state = 8648; break; // &trianglelef -> &triangleleft
				case 8658: state = 8659; break; // &trianglerigh -> &triangleright
				case 8665: state = 8666; break; // &trido -> &tridot
				case 8682: state = 8683; break; // &TripleDo -> &TripleDot
				case 8705: state = 8727; break; // &Ts -> &Tst
				case 8709: state = 8732; break; // &ts -> &tst
				case 8739: state = 8740; break; // &twix -> &twixt
				case 8749: state = 8750; break; // &twoheadlef -> &twoheadleft
				case 8760: state = 8761; break; // &twoheadrigh -> &twoheadright
				case 8768: state = 9166; break; // &U -> &Ut
				case 8771: state = 8772; break; // &Uacu -> &Uacut
				case 8775: state = 9161; break; // &u -> &ut
				case 8778: state = 8779; break; // &uacu -> &uacut
				case 8852: state = 8853; break; // &ufish -> &ufisht
				case 8887: state = 8900; break; // &ul -> &ult
				case 8930: state = 8931; break; // &UnderBracke -> &UnderBracket
				case 8937: state = 8938; break; // &UnderParen -> &UnderParent
				case 9055: state = 9056; break; // &upharpoonlef -> &upharpoonleft
				case 9061: state = 9062; break; // &upharpoonrigh -> &upharpoonright
				case 9073: state = 9074; break; // &UpperLef -> &UpperLeft
				case 9084: state = 9085; break; // &UpperRigh -> &UpperRight
				case 9127: state = 9149; break; // &ur -> &urt
				case 9163: state = 9164; break; // &utdo -> &utdot
				case 9205: state = 9206; break; // &vangr -> &vangrt
				case 9208: state = 9279; break; // &var -> &vart
				case 9224: state = 9225; break; // &varno -> &varnot
				case 9239: state = 9240; break; // &varprop -> &varpropt
				case 9261: state = 9262; break; // &varsubse -> &varsubset
				case 9271: state = 9272; break; // &varsupse -> &varsupset
				case 9281: state = 9282; break; // &varthe -> &varthet
				case 9294: state = 9295; break; // &vartrianglelef -> &vartriangleleft
				case 9300: state = 9301; break; // &vartrianglerigh -> &vartriangleright
				case 9360: state = 9370; break; // &Ver -> &Vert
				case 9365: state = 9372; break; // &ver -> &vert
				case 9392: state = 9393; break; // &VerticalSepara -> &VerticalSeparat
				case 9420: state = 9421; break; // &vl -> &vlt
				case 9445: state = 9446; break; // &vr -> &vrt
				case 9536: state = 9537; break; // &wrea -> &wreat
				case 9560: state = 9561; break; // &xd -> &xdt
				case 9602: state = 9618; break; // &xo -> &xot
				case 9604: state = 9605; break; // &xodo -> &xodot
				case 9645: state = 9651; break; // &xu -> &xut
				case 9668: state = 9669; break; // &Yacu -> &Yacut
				case 9675: state = 9676; break; // &yacu -> &yacut
				case 9750: state = 9751; break; // &Zacu -> &Zacut
				case 9757: state = 9758; break; // &zacu -> &zacut
				case 9778: state = 9779; break; // &Zdo -> &Zdot
				case 9782: state = 9783; break; // &zdo -> &zdot
				case 9785: state = 9808; break; // &ze -> &zet
				case 9786: state = 9787; break; // &zee -> &zeet
				case 9791: state = 9805; break; // &Ze -> &Zet
				case 9796: state = 9797; break; // &ZeroWid -> &ZeroWidt
				default: return false;
				}
				break;
			case 'u':
				switch (state) {
				case 0: state = 8775; break; // & -> &u
				case 1: state = 281; break; // &A -> &Au
				case 3: state = 4; break; // &Aac -> &Aacu
				case 8: state = 285; break; // &a -> &au
				case 10: state = 11; break; // &aac -> &aacu
				case 27: state = 42; break; // &ac -> &acu
				case 220: state = 221; break; // &ApplyF -> &ApplyFu
				case 301: state = 767; break; // &b -> &bu
				case 331: state = 781; break; // &B -> &Bu
				case 380: state = 381; break; // &bdq -> &bdqu
				case 386: state = 387; break; // &beca -> &becau
				case 392: state = 393; break; // &Beca -> &Becau
				case 411: state = 412; break; // &berno -> &bernou
				case 416: state = 417; break; // &Berno -> &Bernou
				case 443: state = 497; break; // &big -> &bigu
				case 444: state = 452; break; // &bigc -> &bigcu
				case 461: state = 462; break; // &bigopl -> &bigoplu
				case 473: state = 474; break; // &bigsqc -> &bigsqcu
				case 488: state = 494; break; // &bigtriangle -> &bigtriangleu
				case 499: state = 500; break; // &bigupl -> &biguplu
				case 532: state = 533; break; // &blacksq -> &blacksqu
				case 582: state = 583; break; // &bneq -> &bnequ
				case 613: state = 678; break; // &box -> &boxu
				case 636: state = 650; break; // &boxH -> &boxHu
				case 638: state = 654; break; // &boxh -> &boxhu
				case 658: state = 659; break; // &boxmin -> &boxminu
				case 663: state = 664; break; // &boxpl -> &boxplu
				case 763: state = 764; break; // &bsolhs -> &bsolhsu
				case 789: state = 1315; break; // &C -> &Cu
				case 791: state = 792; break; // &Cac -> &Cacu
				case 796: state = 1292; break; // &c -> &cu
				case 798: state = 799; break; // &cac -> &cacu
				case 813: state = 814; break; // &capbrc -> &capbrcu
				case 817: state = 821; break; // &capc -> &capcu
				case 861: state = 900; break; // &cc -> &ccu
				case 1034: state = 1035; break; // &CircleMin -> &CircleMinu
				case 1039: state = 1040; break; // &CirclePl -> &CirclePlu
				case 1080: state = 1081; break; // &ClockwiseConto -> &ClockwiseContou
				case 1094: state = 1095; break; // &CloseC -> &CloseCu
				case 1100: state = 1101; break; // &CloseCurlyDo -> &CloseCurlyDou
				case 1105: state = 1106; break; // &CloseCurlyDoubleQ -> &CloseCurlyDoubleQu
				case 1111: state = 1112; break; // &CloseCurlyQ -> &CloseCurlyQu
				case 1117: state = 1118; break; // &cl -> &clu
				case 1120: state = 1122; break; // &clubs -> &clubsu
				case 1126: state = 1226; break; // &Co -> &Cou
				case 1173: state = 1174; break; // &Congr -> &Congru
				case 1188: state = 1189; break; // &Conto -> &Contou
				case 1212: state = 1213; break; // &Coprod -> &Coprodu
				case 1244: state = 1245; break; // &CounterClockwiseConto -> &CounterClockwiseContou
				case 1274: state = 1278; break; // &cs -> &csu
				case 1330: state = 1334; break; // &cupc -> &cupcu
				case 1362: state = 1363; break; // &curlyeqs -> &curlyeqsu
				case 1432: state = 2077; break; // &d -> &du
				case 1568: state = 1569; break; // &DiacriticalAc -> &DiacriticalAcu
				case 1574: state = 1577; break; // &DiacriticalDo -> &DiacriticalDou
				case 1582: state = 1583; break; // &DiacriticalDoubleAc -> &DiacriticalDoubleAcu
				case 1612: state = 1613; break; // &diamonds -> &diamondsu
				case 1679: state = 1731; break; // &do -> &dou
				case 1685: state = 1744; break; // &Do -> &Dou
				case 1708: state = 1709; break; // &DotEq -> &DotEqu
				case 1715: state = 1716; break; // &dotmin -> &dotminu
				case 1720: state = 1721; break; // &dotpl -> &dotplu
				case 1725: state = 1726; break; // &dotsq -> &dotsqu
				case 1752: state = 1753; break; // &DoubleConto -> &DoubleContou
				case 2108: state = 2447; break; // &E -> &Eu
				case 2110: state = 2111; break; // &Eac -> &Eacu
				case 2115: state = 2451; break; // &e -> &eu
				case 2117: state = 2118; break; // &eac -> &eacu
				case 2255: state = 2256; break; // &EmptySmallSq -> &EmptySmallSqu
				case 2273: state = 2274; break; // &EmptyVerySmallSq -> &EmptyVerySmallSqu
				case 2319: state = 2320; break; // &epl -> &eplu
				case 2339: state = 2372; break; // &eq -> &equ
				case 2367: state = 2368; break; // &Eq -> &Equ
				case 2392: state = 2393; break; // &Equilibri -> &Equilibriu
				case 2565: state = 2566; break; // &FilledSmallSq -> &FilledSmallSqu
				case 2581: state = 2582; break; // &FilledVerySmallSq -> &FilledVerySmallSqu
				case 2608: state = 2630; break; // &Fo -> &Fou
				case 2703: state = 2704; break; // &gac -> &gacu
				case 2873: state = 2874; break; // &GreaterEq -> &GreaterEqu
				case 2883: state = 2884; break; // &GreaterF -> &GreaterFu
				case 2888: state = 2889; break; // &GreaterFullEq -> &GreaterFullEqu
				case 2912: state = 2913; break; // &GreaterSlantEq -> &GreaterSlantEqu
				case 2959: state = 2960; break; // &gtq -> &gtqu
				case 3014: state = 3207; break; // &H -> &Hu
				case 3078: state = 3080; break; // &hearts -> &heartsu
				case 3214: state = 3215; break; // &HumpDownH -> &HumpDownHu
				case 3220: state = 3221; break; // &HumpEq -> &HumpEqu
				case 3226: state = 3227; break; // &hyb -> &hybu
				case 3236: state = 3539; break; // &I -> &Iu
				case 3238: state = 3239; break; // &Iac -> &Iacu
				case 3243: state = 3544; break; // &i -> &iu
				case 3245: state = 3246; break; // &iac -> &iacu
				case 3497: state = 3498; break; // &iq -> &iqu
				case 3555: state = 3608; break; // &J -> &Ju
				case 3561: state = 3613; break; // &j -> &ju
				case 3692: state = 4742; break; // &l -> &lu
				case 3700: state = 3701; break; // &Lac -> &Lacu
				case 3706: state = 3707; break; // &lac -> &lacu
				case 3755: state = 3756; break; // &laq -> &laqu
				case 3832: state = 3835; break; // &lbrksl -> &lbrkslu
				case 3843: state = 3862; break; // &lc -> &lcu
				case 3873: state = 3874; break; // &ldq -> &ldqu
				case 3879: state = 3885; break; // &ldr -> &ldru
				case 3962: state = 3963; break; // &LeftDo -> &LeftDou
				case 4010: state = 4016; break; // &leftharpoon -> &leftharpoonu
				case 4075: state = 4076; break; // &leftrightsq -> &leftrightsqu
				case 4133: state = 4134; break; // &LeftTriangleEq -> &LeftTriangleEqu
				case 4241: state = 4242; break; // &LessEq -> &LessEqu
				case 4253: state = 4254; break; // &LessF -> &LessFu
				case 4258: state = 4259; break; // &LessFullEq -> &LessFullEqu
				case 4290: state = 4291; break; // &LessSlantEq -> &LessSlantEqu
				case 4327: state = 4330; break; // &lhar -> &lharu
				case 4391: state = 4392; break; // &lmo -> &lmou
				case 4569: state = 4570; break; // &lopl -> &loplu
				case 4654: state = 4655; break; // &lsaq -> &lsaqu
				case 4676: state = 4679; break; // &lsq -> &lsqu
				case 4725: state = 4726; break; // &ltq -> &ltqu
				case 4743: state = 4750; break; // &lur -> &luru
				case 4767: state = 4952; break; // &m -> &mu
				case 4781: state = 4950; break; // &M -> &Mu
				case 4789: state = 4801; break; // &mapsto -> &mapstou
				case 4832: state = 4833; break; // &meas -> &measu
				case 4845: state = 4846; break; // &Medi -> &Mediu
				case 4890: state = 4891; break; // &min -> &minu
				case 4896: state = 4898; break; // &minusd -> &minusdu
				case 4901: state = 4902; break; // &Min -> &Minu
				case 4905: state = 4906; break; // &MinusPl -> &MinusPlu
				case 4918: state = 4919; break; // &mnpl -> &mnplu
				case 4965: state = 6032; break; // &n -> &nu
				case 4971: state = 6030; break; // &N -> &Nu
				case 4973: state = 4974; break; // &Nac -> &Nacu
				case 4978: state = 4979; break; // &nac -> &nacu
				case 5001: state = 5002; break; // &nat -> &natu
				case 5010: state = 5014; break; // &nb -> &nbu
				case 5020: state = 5052; break; // &nc -> &ncu
				case 5094: state = 5095; break; // &NegativeMedi -> &NegativeMediu
				case 5135: state = 5136; break; // &neq -> &nequ
				case 5380: state = 5390; break; // &NotC -> &NotCu
				case 5384: state = 5385; break; // &NotCongr -> &NotCongru
				case 5397: state = 5398; break; // &NotDo -> &NotDou
				case 5422: state = 5423; break; // &NotEq -> &NotEqu
				case 5448: state = 5449; break; // &NotGreaterEq -> &NotGreaterEqu
				case 5453: state = 5454; break; // &NotGreaterF -> &NotGreaterFu
				case 5458: state = 5459; break; // &NotGreaterFullEq -> &NotGreaterFullEqu
				case 5482: state = 5483; break; // &NotGreaterSlantEq -> &NotGreaterSlantEqu
				case 5493: state = 5494; break; // &NotH -> &NotHu
				case 5501: state = 5502; break; // &NotHumpDownH -> &NotHumpDownHu
				case 5507: state = 5508; break; // &NotHumpEq -> &NotHumpEqu
				case 5546: state = 5547; break; // &NotLeftTriangleEq -> &NotLeftTriangleEqu
				case 5555: state = 5556; break; // &NotLessEq -> &NotLessEqu
				case 5579: state = 5580; break; // &NotLessSlantEq -> &NotLessSlantEqu
				case 5640: state = 5641; break; // &NotPrecedesEq -> &NotPrecedesEqu
				case 5651: state = 5652; break; // &NotPrecedesSlantEq -> &NotPrecedesSlantEqu
				case 5689: state = 5690; break; // &NotRightTriangleEq -> &NotRightTriangleEqu
				case 5694: state = 5726; break; // &NotS -> &NotSu
				case 5695: state = 5696; break; // &NotSq -> &NotSqu
				case 5700: state = 5701; break; // &NotSquareS -> &NotSquareSu
				case 5708: state = 5709; break; // &NotSquareSubsetEq -> &NotSquareSubsetEqu
				case 5721: state = 5722; break; // &NotSquareSupersetEq -> &NotSquareSupersetEqu
				case 5733: state = 5734; break; // &NotSubsetEq -> &NotSubsetEqu
				case 5746: state = 5747; break; // &NotSucceedsEq -> &NotSucceedsEqu
				case 5757: state = 5758; break; // &NotSucceedsSlantEq -> &NotSucceedsSlantEqu
				case 5776: state = 5777; break; // &NotSupersetEq -> &NotSupersetEqu
				case 5788: state = 5789; break; // &NotTildeEq -> &NotTildeEqu
				case 5793: state = 5794; break; // &NotTildeF -> &NotTildeFu
				case 5798: state = 5799; break; // &NotTildeFullEq -> &NotTildeFullEqu
				case 5844: state = 5845; break; // &nprc -> &nprcu
				case 5895: state = 5951; break; // &ns -> &nsu
				case 5898: state = 5899; break; // &nscc -> &nsccu
				case 5943: state = 5944; break; // &nsqs -> &nsqsu
				case 6131: state = 6422; break; // &O -> &Ou
				case 6133: state = 6134; break; // &Oac -> &Oacu
				case 6138: state = 6426; break; // &o -> &ou
				case 6140: state = 6141; break; // &oac -> &oacu
				case 6290: state = 6291; break; // &omin -> &ominu
				case 6309: state = 6310; break; // &OpenC -> &OpenCu
				case 6315: state = 6316; break; // &OpenCurlyDo -> &OpenCurlyDou
				case 6320: state = 6321; break; // &OpenCurlyDoubleQ -> &OpenCurlyDoubleQu
				case 6326: state = 6327; break; // &OpenCurlyQ -> &OpenCurlyQu
				case 6336: state = 6337; break; // &opl -> &oplu
				case 6463: state = 6807; break; // &p -> &pu
				case 6555: state = 6566; break; // &pl -> &plu
				case 6580: state = 6583; break; // &plusd -> &plusdu
				case 6587: state = 6588; break; // &Pl -> &Plu
				case 6592: state = 6593; break; // &PlusMin -> &PlusMinu
				case 6622: state = 6636; break; // &po -> &pou
				case 6642: state = 6790; break; // &pr -> &pru
				case 6647: state = 6648; break; // &prc -> &prcu
				case 6664: state = 6665; break; // &precc -> &preccu
				case 6680: state = 6681; break; // &PrecedesEq -> &PrecedesEqu
				case 6691: state = 6692; break; // &PrecedesSlantEq -> &PrecedesSlantEqu
				case 6749: state = 6750; break; // &Prod -> &Produ
				case 6765: state = 6766; break; // &profs -> &profsu
				case 6817: state = 6847; break; // &q -> &qu
				case 6876: state = 7601; break; // &r -> &ru
				case 6883: state = 6893; break; // &rac -> &racu
				case 6886: state = 7590; break; // &R -> &Ru
				case 6888: state = 6889; break; // &Rac -> &Racu
				case 6921: state = 6922; break; // &raq -> &raqu
				case 7016: state = 7019; break; // &rbrksl -> &rbrkslu
				case 7027: state = 7046; break; // &rc -> &rcu
				case 7063: state = 7064; break; // &rdq -> &rdqu
				case 7110: state = 7111; break; // &ReverseEq -> &ReverseEqu
				case 7117: state = 7118; break; // &ReverseEquilibri -> &ReverseEquilibriu
				case 7124: state = 7125; break; // &ReverseUpEq -> &ReverseUpEqu
				case 7131: state = 7132; break; // &ReverseUpEquilibri -> &ReverseUpEquilibriu
				case 7157: state = 7160; break; // &rhar -> &rharu
				case 7237: state = 7238; break; // &RightDo -> &RightDou
				case 7285: state = 7291; break; // &rightharpoon -> &rightharpoonu
				case 7327: state = 7328; break; // &rightsq -> &rightsqu
				case 7378: state = 7379; break; // &RightTriangleEq -> &RightTriangleEqu
				case 7454: state = 7455; break; // &rmo -> &rmou
				case 7485: state = 7501; break; // &Ro -> &Rou
				case 7491: state = 7492; break; // &ropl -> &roplu
				case 7544: state = 7545; break; // &rsaq -> &rsaqu
				case 7559: state = 7562; break; // &rsq -> &rsqu
				case 7602: state = 7603; break; // &rul -> &rulu
				case 7610: state = 8127; break; // &S -> &Su
				case 7612: state = 7613; break; // &Sac -> &Sacu
				case 7617: state = 8130; break; // &s -> &su
				case 7619: state = 7620; break; // &sac -> &sacu
				case 7625: state = 7626; break; // &sbq -> &sbqu
				case 7645: state = 7646; break; // &scc -> &sccu
				case 7732: state = 7733; break; // &setmin -> &setminu
				case 7869: state = 7870; break; // &simpl -> &simplu
				case 7903: state = 7904; break; // &smallsetmin -> &smallsetminu
				case 7960: state = 7962; break; // &spades -> &spadesu
				case 7968: state = 8008; break; // &sq -> &squ
				case 7969: state = 7975; break; // &sqc -> &sqcu
				case 7980: state = 8010; break; // &Sq -> &Squ
				case 7984: state = 7985; break; // &sqs -> &sqsu
				case 8032: state = 8033; break; // &SquareS -> &SquareSu
				case 8040: state = 8041; break; // &SquareSubsetEq -> &SquareSubsetEqu
				case 8053: state = 8054; break; // &SquareSupersetEq -> &SquareSupersetEqu
				case 8145: state = 8146; break; // &subm -> &submu
				case 8156: state = 8157; break; // &subpl -> &subplu
				case 8169: state = 8193; break; // &subs -> &subsu
				case 8179: state = 8180; break; // &SubsetEq -> &SubsetEqu
				case 8208: state = 8209; break; // &succc -> &succcu
				case 8224: state = 8225; break; // &SucceedsEq -> &SucceedsEqu
				case 8235: state = 8236; break; // &SucceedsSlantEq -> &SucceedsSlantEqu
				case 8296: state = 8297; break; // &supds -> &supdsu
				case 8315: state = 8316; break; // &SupersetEq -> &SupersetEqu
				case 8321: state = 8325; break; // &suphs -> &suphsu
				case 8333: state = 8334; break; // &supm -> &supmu
				case 8344: state = 8345; break; // &suppl -> &supplu
				case 8352: state = 8370; break; // &sups -> &supsu
				case 8401: state = 8411; break; // &Ta -> &Tau
				case 8405: state = 8413; break; // &ta -> &tau
				case 8555: state = 8556; break; // &TildeEq -> &TildeEqu
				case 8560: state = 8561; break; // &TildeF -> &TildeFu
				case 8565: state = 8566; break; // &TildeFullEq -> &TildeFullEqu
				case 8672: state = 8673; break; // &trimin -> &triminu
				case 8686: state = 8687; break; // &tripl -> &triplu
				case 8701: state = 8702; break; // &trpezi -> &trpeziu
				case 8768: state = 9187; break; // &U -> &Uu
				case 8770: state = 8771; break; // &Uac -> &Uacu
				case 8775: state = 9182; break; // &u -> &uu
				case 8777: state = 8778; break; // &uac -> &uacu
				case 8950: state = 8951; break; // &UnionPl -> &UnionPlu
				case 8983: state = 9118; break; // &up -> &upu
				case 9035: state = 9036; break; // &UpEq -> &UpEqu
				case 9042: state = 9043; break; // &UpEquilibri -> &UpEquilibriu
				case 9064: state = 9065; break; // &upl -> &uplu
				case 9252: state = 9258; break; // &vars -> &varsu
				case 9426: state = 9427; break; // &vns -> &vnsu
				case 9454: state = 9458; break; // &vs -> &vsu
				case 9548: state = 9645; break; // &x -> &xu
				case 9549: state = 9557; break; // &xc -> &xcu
				case 9614: state = 9615; break; // &xopl -> &xoplu
				case 9641: state = 9642; break; // &xsqc -> &xsqcu
				case 9647: state = 9648; break; // &xupl -> &xuplu
				case 9665: state = 9740; break; // &Y -> &Yu
				case 9667: state = 9668; break; // &Yac -> &Yacu
				case 9672: state = 9736; break; // &y -> &yu
				case 9674: state = 9675; break; // &yac -> &yacu
				case 9749: state = 9750; break; // &Zac -> &Zacu
				case 9756: state = 9757; break; // &zac -> &zacu
				default: return false;
				}
				break;
			case 'v':
				switch (state) {
				case 0: state = 9201; break; // & -> &v
				case 17: state = 18; break; // &Abre -> &Abrev
				case 23: state = 24; break; // &abre -> &abrev
				case 69: state = 70; break; // &Agra -> &Agrav
				case 75: state = 76; break; // &agra -> &agrav
				case 120: state = 134; break; // &and -> &andv
				case 165: state = 167; break; // &angrt -> &angrtv
				case 341: state = 342; break; // &Bar -> &Barv
				case 344: state = 345; break; // &bar -> &barv
				case 402: state = 403; break; // &bempty -> &bemptyv
				case 443: state = 503; break; // &big -> &bigv
				case 584: state = 585; break; // &bnequi -> &bnequiv
				case 613: state = 693; break; // &box -> &boxv
				case 726: state = 727; break; // &Bre -> &Brev
				case 730: state = 735; break; // &br -> &brv
				case 731: state = 732; break; // &bre -> &brev
				case 930: state = 931; break; // &cempty -> &cemptyv
				case 1292: state = 1399; break; // &cu -> &cuv
				case 1346: state = 1381; break; // &cur -> &curv
				case 1354: state = 1367; break; // &curly -> &curlyv
				case 1455: state = 1461; break; // &dash -> &dashv
				case 1458: state = 1459; break; // &Dash -> &Dashv
				case 1532: state = 1533; break; // &dempty -> &demptyv
				case 1589: state = 1590; break; // &DiacriticalGra -> &DiacriticalGrav
				case 1599: state = 1643; break; // &di -> &div
				case 1917: state = 1918; break; // &DownBre -> &DownBrev
				case 2189: state = 2190; break; // &Egra -> &Egrav
				case 2194: state = 2195; break; // &egra -> &egrav
				case 2240: state = 2261; break; // &empty -> &emptyv
				case 2324: state = 2337; break; // &epsi -> &epsiv
				case 2339: state = 2402; break; // &eq -> &eqv
				case 2396: state = 2397; break; // &equi -> &equiv
				case 2626: state = 2628; break; // &fork -> &forkv
				case 2701: state = 3002; break; // &g -> &gv
				case 2726: state = 2727; break; // &Gbre -> &Gbrev
				case 2732: state = 2733; break; // &gbre -> &gbrev
				case 2862: state = 2863; break; // &gra -> &grav
				case 3291: state = 3292; break; // &Igra -> &Igrav
				case 3297: state = 3298; break; // &igra -> &igrav
				case 3398: state = 3444; break; // &In -> &Inv
				case 3512: state = 3524; break; // &isin -> &isinv
				case 3520: state = 3522; break; // &isins -> &isinsv
				case 3628: state = 3630; break; // &kappa -> &kappav
				case 3692: state = 4755; break; // &l -> &lv
				case 3715: state = 3716; break; // &laempty -> &laemptyv
				case 4965: state = 6043; break; // &n -> &nv
				case 5088: state = 5089; break; // &Negati -> &Negativ
				case 5137: state = 5138; break; // &nequi -> &nequiv
				case 5219: state = 5225; break; // &nGt -> &nGtv
				case 5240: state = 5246; break; // &ni -> &niv
				case 5332: state = 5341; break; // &nLt -> &nLtv
				case 5513: state = 5521; break; // &notin -> &notinv
				case 5621: state = 5623; break; // &notni -> &notniv
				case 5657: state = 5658; break; // &NotRe -> &NotRev
				case 6131: state = 6435; break; // &O -> &Ov
				case 6138: state = 6430; break; // &o -> &ov
				case 6179: state = 6180; break; // &odi -> &odiv
				case 6216: state = 6217; break; // &Ogra -> &Ograv
				case 6221: state = 6222; break; // &ogra -> &ograv
				case 6342: state = 6374; break; // &or -> &orv
				case 6528: state = 6530; break; // &phi -> &phiv
				case 6543: state = 6553; break; // &pi -> &piv
				case 6563: state = 6564; break; // &plank -> &plankv
				case 6905: state = 6906; break; // &raempty -> &raemptyv
				case 7072: state = 7097; break; // &Re -> &Rev
				case 7167: state = 7169; break; // &rho -> &rhov
				case 7841: state = 7845; break; // &sigma -> &sigmav
				case 8485: state = 8491; break; // &theta -> &thetav
				case 8807: state = 8808; break; // &Ubre -> &Ubrev
				case 8811: state = 8812; break; // &ubre -> &ubrev
				case 8862: state = 8863; break; // &Ugra -> &Ugrav
				case 8868: state = 8869; break; // &ugra -> &ugrav
				case 9303: state = 9471; break; // &V -> &Vv
				case 9310: state = 9312; break; // &vBar -> &vBarv
				case 9548: state = 9655; break; // &x -> &xv
				default: return false;
				}
				break;
			case 'w':
				switch (state) {
				case 0: state = 9490; break; // & -> &w
				case 8: state = 289; break; // &a -> &aw
				case 341: state = 349; break; // &Bar -> &Barw
				case 344: state = 353; break; // &bar -> &barw
				case 426: state = 431; break; // &bet -> &betw
				case 443: state = 507; break; // &big -> &bigw
				case 490: state = 491; break; // &bigtriangledo -> &bigtriangledow
				case 516: state = 517; break; // &bkaro -> &bkarow
				case 548: state = 549; break; // &blacktriangledo -> &blacktriangledow
				case 598: state = 608; break; // &bo -> &bow
				case 796: state = 1407; break; // &c -> &cw
				case 991: state = 992; break; // &circlearro -> &circlearrow
				case 1071: state = 1072; break; // &Clock -> &Clockw
				case 1235: state = 1236; break; // &CounterClock -> &CounterClockw
				case 1292: state = 1403; break; // &cu -> &cuw
				case 1354: state = 1371; break; // &curly -> &curlyw
				case 1386: state = 1387; break; // &curvearro -> &curvearrow
				case 1432: state = 2086; break; // &d -> &dw
				case 1467: state = 1468; break; // &dbkaro -> &dbkarow
				case 1679: state = 1895; break; // &do -> &dow
				case 1685: state = 1881; break; // &Do -> &Dow
				case 1737: state = 1738; break; // &doublebar -> &doublebarw
				case 1765: state = 1768; break; // &DoubleDo -> &DoubleDow
				case 1773: state = 1774; break; // &DoubleDownArro -> &DoubleDownArrow
				case 1783: state = 1784; break; // &DoubleLeftArro -> &DoubleLeftArrow
				case 1794: state = 1795; break; // &DoubleLeftRightArro -> &DoubleLeftRightArrow
				case 1811: state = 1812; break; // &DoubleLongLeftArro -> &DoubleLongLeftArrow
				case 1822: state = 1823; break; // &DoubleLongLeftRightArro -> &DoubleLongLeftRightArrow
				case 1833: state = 1834; break; // &DoubleLongRightArro -> &DoubleLongRightArrow
				case 1844: state = 1845; break; // &DoubleRightArro -> &DoubleRightArrow
				case 1856: state = 1857; break; // &DoubleUpArro -> &DoubleUpArrow
				case 1860: state = 1861; break; // &DoubleUpDo -> &DoubleUpDow
				case 1866: state = 1867; break; // &DoubleUpDownArro -> &DoubleUpDownArrow
				case 1886: state = 1887; break; // &DownArro -> &DownArrow
				case 1892: state = 1893; break; // &Downarro -> &Downarrow
				case 1900: state = 1901; break; // &downarro -> &downarrow
				case 1912: state = 1913; break; // &DownArrowUpArro -> &DownArrowUpArrow
				case 1922: state = 1923; break; // &downdo -> &downdow
				case 1928: state = 1929; break; // &downdownarro -> &downdownarrow
				case 2020: state = 2021; break; // &DownTeeArro -> &DownTeeArrow
				case 2028: state = 2029; break; // &drbkaro -> &drbkarow
				case 2689: state = 2690; break; // &fro -> &frow
				case 3050: state = 3056; break; // &harr -> &harrw
				case 3113: state = 3120; break; // &hks -> &hksw
				case 3117: state = 3118; break; // &hksearo -> &hksearow
				case 3123: state = 3124; break; // &hkswaro -> &hkswarow
				case 3145: state = 3146; break; // &hookleftarro -> &hookleftarrow
				case 3156: state = 3157; break; // &hookrightarro -> &hookrightarrow
				case 3211: state = 3212; break; // &HumpDo -> &HumpDow
				case 3916: state = 3917; break; // &LeftArro -> &LeftArrow
				case 3922: state = 3923; break; // &Leftarro -> &Leftarrow
				case 3930: state = 3931; break; // &leftarro -> &leftarrow
				case 3945: state = 3946; break; // &LeftArrowRightArro -> &LeftArrowRightArrow
				case 3962: state = 3975; break; // &LeftDo -> &LeftDow
				case 4012: state = 4013; break; // &leftharpoondo -> &leftharpoondow
				case 4026: state = 4027; break; // &leftleftarro -> &leftleftarrow
				case 4038: state = 4039; break; // &LeftRightArro -> &LeftRightArrow
				case 4049: state = 4050; break; // &Leftrightarro -> &Leftrightarrow
				case 4060: state = 4061; break; // &leftrightarro -> &leftrightarrow
				case 4082: state = 4083; break; // &leftrightsquigarro -> &leftrightsquigarrow
				case 4099: state = 4100; break; // &LeftTeeArro -> &LeftTeeArrow
				case 4141: state = 4142; break; // &LeftUpDo -> &LeftUpDow
				case 4367: state = 4368; break; // &Lleftarro -> &Lleftarrow
				case 4422: state = 4579; break; // &lo -> &low
				case 4434: state = 4588; break; // &Lo -> &Low
				case 4444: state = 4445; break; // &LongLeftArro -> &LongLeftArrow
				case 4454: state = 4455; break; // &Longleftarro -> &Longleftarrow
				case 4466: state = 4467; break; // &longleftarro -> &longleftarrow
				case 4477: state = 4478; break; // &LongLeftRightArro -> &LongLeftRightArrow
				case 4488: state = 4489; break; // &Longleftrightarro -> &Longleftrightarrow
				case 4499: state = 4500; break; // &longleftrightarro -> &longleftrightarrow
				case 4517: state = 4518; break; // &LongRightArro -> &LongRightArrow
				case 4528: state = 4529; break; // &Longrightarro -> &Longrightarrow
				case 4539: state = 4540; break; // &longrightarro -> &longrightarrow
				case 4547: state = 4548; break; // &looparro -> &looparrow
				case 4598: state = 4599; break; // &LowerLeftArro -> &LowerLeftArrow
				case 4609: state = 4610; break; // &LowerRightArro -> &LowerRightArrow
				case 4792: state = 4793; break; // &mapstodo -> &mapstodow
				case 4965: state = 6111; break; // &n -> &nw
				case 5077: state = 5078; break; // &nearro -> &nearrow
				case 5084: state = 5176; break; // &Ne -> &New
				case 5279: state = 5280; break; // &nLeftarro -> &nLeftarrow
				case 5287: state = 5288; break; // &nleftarro -> &nleftarrow
				case 5298: state = 5299; break; // &nLeftrightarro -> &nLeftrightarrow
				case 5309: state = 5310; break; // &nleftrightarro -> &nleftrightarrow
				case 5498: state = 5499; break; // &NotHumpDo -> &NotHumpDow
				case 5862: state = 5866; break; // &nrarr -> &nrarrw
				case 5876: state = 5877; break; // &nRightarro -> &nRightarrow
				case 5886: state = 5887; break; // &nrightarro -> &nrightarrow
				case 6123: state = 6124; break; // &nwarro -> &nwarrow
				case 6603: state = 6604; break; // &plust -> &plustw
				case 6932: state = 6966; break; // &rarr -> &rarrw
				case 7190: state = 7191; break; // &RightArro -> &RightArrow
				case 7196: state = 7197; break; // &Rightarro -> &Rightarrow
				case 7206: state = 7207; break; // &rightarro -> &rightarrow
				case 7220: state = 7221; break; // &RightArrowLeftArro -> &RightArrowLeftArrow
				case 7237: state = 7250; break; // &RightDo -> &RightDow
				case 7287: state = 7288; break; // &rightharpoondo -> &rightharpoondow
				case 7301: state = 7302; break; // &rightleftarro -> &rightleftarrow
				case 7322: state = 7323; break; // &rightrightarro -> &rightrightarrow
				case 7334: state = 7335; break; // &rightsquigarro -> &rightsquigarrow
				case 7344: state = 7345; break; // &RightTeeArro -> &RightTeeArrow
				case 7386: state = 7387; break; // &RightUpDo -> &RightUpDow
				case 7539: state = 7540; break; // &Rrightarro -> &Rrightarrow
				case 7617: state = 8375; break; // &s -> &sw
				case 7715: state = 7716; break; // &searro -> &searrow
				case 7724: state = 7725; break; // &ses -> &sesw
				case 7747: state = 7748; break; // &sfro -> &sfrow
				case 7777: state = 7778; break; // &ShortDo -> &ShortDow
				case 7783: state = 7784; break; // &ShortDownArro -> &ShortDownArrow
				case 7793: state = 7794; break; // &ShortLeftArro -> &ShortLeftArrow
				case 7820: state = 7821; break; // &ShortRightArro -> &ShortRightArrow
				case 7828: state = 7829; break; // &ShortUpArro -> &ShortUpArrow
				case 8387: state = 8388; break; // &swarro -> &swarrow
				case 8390: state = 8391; break; // &swn -> &swnw
				case 8404: state = 8737; break; // &t -> &tw
				case 8641: state = 8642; break; // &triangledo -> &triangledow
				case 8754: state = 8755; break; // &twoheadleftarro -> &twoheadleftarrow
				case 8765: state = 8766; break; // &twoheadrightarro -> &twoheadrightarrow
				case 8775: state = 9194; break; // &u -> &uw
				case 8974: state = 8975; break; // &UpArro -> &UpArrow
				case 8980: state = 8981; break; // &Uparro -> &Uparrow
				case 8987: state = 8988; break; // &uparro -> &uparrow
				case 8995: state = 8996; break; // &UpArrowDo -> &UpArrowDow
				case 9001: state = 9002; break; // &UpArrowDownArro -> &UpArrowDownArrow
				case 9005: state = 9006; break; // &UpDo -> &UpDow
				case 9011: state = 9012; break; // &UpDownArro -> &UpDownArrow
				case 9015: state = 9016; break; // &Updo -> &Updow
				case 9021: state = 9022; break; // &Updownarro -> &Updownarrow
				case 9025: state = 9026; break; // &updo -> &updow
				case 9031: state = 9032; break; // &updownarro -> &updownarrow
				case 9078: state = 9079; break; // &UpperLeftArro -> &UpperLeftArrow
				case 9089: state = 9090; break; // &UpperRightArro -> &UpperRightArrow
				case 9115: state = 9116; break; // &UpTeeArro -> &UpTeeArrow
				case 9123: state = 9124; break; // &upuparro -> &upuparrow
				case 9548: state = 9659; break; // &x -> &xw
				case 9754: state = 9848; break; // &z -> &zw
				default: return false;
				}
				break;
			case 'x':
				switch (state) {
				case 0: state = 9548; break; // & -> &x
				case 231: state = 232; break; // &appro -> &approx
				case 598: state = 613; break; // &bo -> &box
				case 615: state = 616; break; // &boxbo -> &boxbox
				case 1154: state = 1160; break; // &comple -> &complex
				case 1658: state = 1659; break; // &divon -> &divonx
				case 2108: state = 2466; break; // &E -> &Ex
				case 2115: state = 2458; break; // &e -> &ex
				case 2838: state = 2839; break; // &gnappro -> &gnapprox
				case 2970: state = 2971; break; // &gtrappro -> &gtrapprox
				case 3273: state = 3277; break; // &ie -> &iex
				case 4220: state = 4221; break; // &lessappro -> &lessapprox
				case 4407: state = 4408; break; // &lnappro -> &lnapprox
				case 4998: state = 4999; break; // &nappro -> &napprox
				case 5064: state = 5182; break; // &ne -> &nex
				case 5414: state = 5433; break; // &NotE -> &NotEx
				case 6661: state = 6662; break; // &precappro -> &precapprox
				case 6710: state = 6711; break; // &precnappro -> &precnapprox
				case 6876: state = 7608; break; // &r -> &rx
				case 7703: state = 7738; break; // &se -> &sex
				case 8205: state = 8206; break; // &succappro -> &succapprox
				case 8254: state = 8255; break; // &succnappro -> &succnapprox
				case 8500: state = 8501; break; // &thickappro -> &thickapprox
				case 8738: state = 8739; break; // &twi -> &twix
				default: return false;
				}
				break;
			case 'y':
				switch (state) {
				case 0: state = 9672; break; // & -> &y
				case 27: state = 48; break; // &ac -> &acy
				case 33: state = 46; break; // &Ac -> &Acy
				case 82: state = 83; break; // &alefs -> &alefsy
				case 218: state = 219; break; // &Appl -> &Apply
				case 251: state = 262; break; // &as -> &asy
				case 369: state = 377; break; // &bc -> &bcy
				case 374: state = 375; break; // &Bc -> &Bcy
				case 401: state = 402; break; // &bempt -> &bempty
				case 790: state = 855; break; // &Ca -> &Cay
				case 796: state = 1419; break; // &c -> &cy
				case 857: state = 858; break; // &Cayle -> &Cayley
				case 929: state = 930; break; // &cempt -> &cempty
				case 957: state = 958; break; // &CHc -> &CHcy
				case 961: state = 962; break; // &chc -> &chcy
				case 1097: state = 1098; break; // &CloseCurl -> &CloseCurly
				case 1203: state = 1221; break; // &cop -> &copy
				case 1353: state = 1354; break; // &curl -> &curly
				case 1422: state = 1423; break; // &cylct -> &cylcty
				case 1474: state = 1486; break; // &Dc -> &Dcy
				case 1480: state = 1488; break; // &dc -> &dcy
				case 1531: state = 1532; break; // &dempt -> &dempty
				case 1662: state = 1663; break; // &DJc -> &DJcy
				case 1666: state = 1667; break; // &djc -> &djcy
				case 2045: state = 2052; break; // &dsc -> &dscy
				case 2049: state = 2050; break; // &DSc -> &DScy
				case 2094: state = 2095; break; // &DZc -> &DZcy
				case 2098: state = 2099; break; // &dzc -> &dzcy
				case 2127: state = 2153; break; // &Ec -> &Ecy
				case 2133: state = 2155; break; // &ec -> &ecy
				case 2239: state = 2240; break; // &empt -> &empty
				case 2247: state = 2248; break; // &Empt -> &Empty
				case 2265: state = 2266; break; // &EmptyVer -> &EmptyVery
				case 2518: state = 2519; break; // &Fc -> &Fcy
				case 2521: state = 2522; break; // &fc -> &fcy
				case 2573: state = 2574; break; // &FilledVer -> &FilledVery
				case 2736: state = 2751; break; // &Gc -> &Gcy
				case 2746: state = 2753; break; // &gc -> &gcy
				case 2817: state = 2818; break; // &GJc -> &GJcy
				case 2821: state = 2822; break; // &gjc -> &gjcy
				case 3020: state = 3225; break; // &h -> &hy
				case 3038: state = 3039; break; // &HARDc -> &HARDcy
				case 3043: state = 3044; break; // &hardc -> &hardcy
				case 3250: state = 3263; break; // &ic -> &icy
				case 3252: state = 3261; break; // &Ic -> &Icy
				case 3270: state = 3271; break; // &IEc -> &IEcy
				case 3274: state = 3275; break; // &iec -> &iecy
				case 3348: state = 3349; break; // &Imaginar -> &Imaginary
				case 3464: state = 3465; break; // &IOc -> &IOcy
				case 3468: state = 3469; break; // &ioc -> &iocy
				case 3541: state = 3542; break; // &Iukc -> &Iukcy
				case 3546: state = 3547; break; // &iukc -> &iukcy
				case 3556: state = 3567; break; // &Jc -> &Jcy
				case 3562: state = 3569; break; // &jc -> &jcy
				case 3600: state = 3601; break; // &Jserc -> &Jsercy
				case 3605: state = 3606; break; // &jserc -> &jsercy
				case 3610: state = 3611; break; // &Jukc -> &Jukcy
				case 3615: state = 3616; break; // &jukc -> &jukcy
				case 3632: state = 3644; break; // &Kc -> &Kcy
				case 3638: state = 3646; break; // &kc -> &kcy
				case 3661: state = 3662; break; // &KHc -> &KHcy
				case 3665: state = 3666; break; // &khc -> &khcy
				case 3669: state = 3670; break; // &KJc -> &KJcy
				case 3673: state = 3674; break; // &kjc -> &kjcy
				case 3714: state = 3715; break; // &laempt -> &laempty
				case 3837: state = 3865; break; // &Lc -> &Lcy
				case 3843: state = 3867; break; // &lc -> &lcy
				case 4339: state = 4340; break; // &LJc -> &LJcy
				case 4343: state = 4344; break; // &ljc -> &ljcy
				case 4809: state = 4818; break; // &mc -> &mcy
				case 4815: state = 4816; break; // &Mc -> &Mcy
				case 5020: state = 5057; break; // &nc -> &ncy
				case 5024: state = 5055; break; // &Nc -> &Ncy
				case 5123: state = 5124; break; // &NegativeVer -> &NegativeVery
				case 5249: state = 5250; break; // &NJc -> &NJcy
				case 5253: state = 5254; break; // &njc -> &njcy
				case 6148: state = 6161; break; // &oc -> &ocy
				case 6152: state = 6159; break; // &Oc -> &Ocy
				case 6312: state = 6313; break; // &OpenCurl -> &OpenCurly
				case 6491: state = 6492; break; // &Pc -> &Pcy
				case 6494: state = 6495; break; // &pc -> &pcy
				case 6667: state = 6668; break; // &preccurl -> &preccurly
				case 6904: state = 6905; break; // &raempt -> &raempty
				case 7021: state = 7049; break; // &Rc -> &Rcy
				case 7027: state = 7051; break; // &rc -> &rcy
				case 7596: state = 7597; break; // &RuleDela -> &RuleDelay
				case 7629: state = 7691; break; // &Sc -> &Scy
				case 7631: state = 7693; break; // &sc -> &scy
				case 7751: state = 7831; break; // &sh -> &shy
				case 7759: state = 7760; break; // &SHCHc -> &SHCHcy
				case 7762: state = 7770; break; // &shc -> &shcy
				case 7764: state = 7765; break; // &shchc -> &shchcy
				case 7767: state = 7768; break; // &SHc -> &SHcy
				case 7933: state = 7934; break; // &SOFTc -> &SOFTcy
				case 7939: state = 7940; break; // &softc -> &softcy
				case 8211: state = 8212; break; // &succcurl -> &succcurly
				case 8419: state = 8441; break; // &Tc -> &Tcy
				case 8425: state = 8443; break; // &tc -> &tcy
				case 8487: state = 8488; break; // &thetas -> &thetasy
				case 8710: state = 8717; break; // &tsc -> &tscy
				case 8714: state = 8715; break; // &TSc -> &TScy
				case 8720: state = 8721; break; // &TSHc -> &TSHcy
				case 8724: state = 8725; break; // &tshc -> &tshcy
				case 8799: state = 8800; break; // &Ubrc -> &Ubrcy
				case 8804: state = 8805; break; // &ubrc -> &ubrcy
				case 8815: state = 8825; break; // &Uc -> &Ucy
				case 8820: state = 8827; break; // &uc -> &ucy
				case 9314: state = 9315; break; // &Vc -> &Vcy
				case 9317: state = 9318; break; // &vc -> &vcy
				case 9360: state = 9403; break; // &Ver -> &Very
				case 9674: state = 9683; break; // &yac -> &yacy
				case 9680: state = 9681; break; // &YAc -> &YAcy
				case 9685: state = 9695; break; // &Yc -> &Ycy
				case 9690: state = 9697; break; // &yc -> &ycy
				case 9709: state = 9710; break; // &YIc -> &YIcy
				case 9713: state = 9714; break; // &yic -> &yicy
				case 9733: state = 9734; break; // &YUc -> &YUcy
				case 9737: state = 9738; break; // &yuc -> &yucy
				case 9761: state = 9773; break; // &Zc -> &Zcy
				case 9767: state = 9775; break; // &zc -> &zcy
				case 9818: state = 9819; break; // &ZHc -> &ZHcy
				case 9822: state = 9823; break; // &zhc -> &zhcy
				default: return false;
				}
				break;
			case 'z':
				switch (state) {
				case 0: state = 9754; break; // & -> &z
				case 136: state = 178; break; // &ang -> &angz
				case 524: state = 525; break; // &blacklo -> &blackloz
				case 1432: state = 2097; break; // &d -> &dz
				case 3172: state = 3173; break; // &Hori -> &Horiz
				case 4422: state = 4612; break; // &lo -> &loz
				case 7617: state = 8395; break; // &s -> &sz
				case 8699: state = 8700; break; // &trpe -> &trpez
				case 9201: state = 9477; break; // &v -> &vz
				case 9479: state = 9480; break; // &vzig -> &vzigz
				default: return false;
				}
				break;
			default: return false;
			}

			states[index] = state;
			pushed[index] = c;
			index++;

			return true;
		}

		static string GetNamedEntityValue (int state)
		{
			switch (state) {
			case 6: return "\u00C1"; // &Aacute
			case 7: return "\u00C1"; // &Aacute;
			case 13: return "\u00E1"; // &aacute
			case 14: return "\u00E1"; // &aacute;
			case 20: return "\u0102"; // &Abreve;
			case 26: return "\u0103"; // &abreve;
			case 28: return "\u223E"; // &ac;
			case 30: return "\u223F"; // &acd;
			case 32: return "\u223E\u0333"; // &acE;
			case 36: return "\u00C2"; // &Acirc
			case 37: return "\u00C2"; // &Acirc;
			case 40: return "\u00E2"; // &acirc
			case 41: return "\u00E2"; // &acirc;
			case 44: return "\u00B4"; // &acute
			case 45: return "\u00B4"; // &acute;
			case 47: return "\u0410"; // &Acy;
			case 49: return "\u0430"; // &acy;
			case 53: return "\u00C6"; // &AElig
			case 54: return "\u00C6"; // &AElig;
			case 58: return "\u00E6"; // &aelig
			case 59: return "\u00E6"; // &aelig;
			case 61: return "\u2061"; // &af;
			case 64: return "\uD835\uDD04"; // &Afr;
			case 66: return "\uD835\uDD1E"; // &afr;
			case 71: return "\u00C0"; // &Agrave
			case 72: return "\u00C0"; // &Agrave;
			case 77: return "\u00E0"; // &agrave
			case 78: return "\u00E0"; // &agrave;
			case 85: return "\u2135"; // &alefsym;
			case 88: return "\u2135"; // &aleph;
			case 93: return "\u0391"; // &Alpha;
			case 97: return "\u03B1"; // &alpha;
			case 102: return "\u0100"; // &Amacr;
			case 107: return "\u0101"; // &amacr;
			case 110: return "\u2A3F"; // &amalg;
			case 112: return "\u0026"; // &AMP
			case 113: return "\u0026"; // &AMP;
			case 114: return "\u0026"; // &amp
			case 115: return "\u0026"; // &amp;
			case 118: return "\u2A53"; // &And;
			case 121: return "\u2227"; // &and;
			case 125: return "\u2A55"; // &andand;
			case 127: return "\u2A5C"; // &andd;
			case 133: return "\u2A58"; // &andslope;
			case 135: return "\u2A5A"; // &andv;
			case 137: return "\u2220"; // &ang;
			case 139: return "\u29A4"; // &ange;
			case 142: return "\u2220"; // &angle;
			case 146: return "\u2221"; // &angmsd;
			case 149: return "\u29A8"; // &angmsdaa;
			case 151: return "\u29A9"; // &angmsdab;
			case 153: return "\u29AA"; // &angmsdac;
			case 155: return "\u29AB"; // &angmsdad;
			case 157: return "\u29AC"; // &angmsdae;
			case 159: return "\u29AD"; // &angmsdaf;
			case 161: return "\u29AE"; // &angmsdag;
			case 163: return "\u29AF"; // &angmsdah;
			case 166: return "\u221F"; // &angrt;
			case 169: return "\u22BE"; // &angrtvb;
			case 171: return "\u299D"; // &angrtvbd;
			case 175: return "\u2222"; // &angsph;
			case 177: return "\u00C5"; // &angst;
			case 182: return "\u237C"; // &angzarr;
			case 187: return "\u0104"; // &Aogon;
			case 192: return "\u0105"; // &aogon;
			case 195: return "\uD835\uDD38"; // &Aopf;
			case 198: return "\uD835\uDD52"; // &aopf;
			case 200: return "\u2248"; // &ap;
			case 205: return "\u2A6F"; // &apacir;
			case 207: return "\u2A70"; // &apE;
			case 209: return "\u224A"; // &ape;
			case 212: return "\u224B"; // &apid;
			case 215: return "\u0027"; // &apos;
			case 228: return "\u2061"; // &ApplyFunction;
			case 233: return "\u2248"; // &approx;
			case 236: return "\u224A"; // &approxeq;
			case 240: return "\u00C5"; // &Aring
			case 241: return "\u00C5"; // &Aring;
			case 245: return "\u00E5"; // &aring
			case 246: return "\u00E5"; // &aring;
			case 250: return "\uD835\uDC9C"; // &Ascr;
			case 254: return "\uD835\uDCB6"; // &ascr;
			case 259: return "\u2254"; // &Assign;
			case 261: return "\u002A"; // &ast;
			case 265: return "\u2248"; // &asymp;
			case 268: return "\u224D"; // &asympeq;
			case 273: return "\u00C3"; // &Atilde
			case 274: return "\u00C3"; // &Atilde;
			case 279: return "\u00E3"; // &atilde
			case 280: return "\u00E3"; // &atilde;
			case 283: return "\u00C4"; // &Auml
			case 284: return "\u00C4"; // &Auml;
			case 287: return "\u00E4"; // &auml
			case 288: return "\u00E4"; // &auml;
			case 296: return "\u2233"; // &awconint;
			case 300: return "\u2A11"; // &awint;
			case 309: return "\u224C"; // &backcong;
			case 317: return "\u03F6"; // &backepsilon;
			case 323: return "\u2035"; // &backprime;
			case 327: return "\u223D"; // &backsim;
			case 330: return "\u22CD"; // &backsimeq;
			case 340: return "\u2216"; // &Backslash;
			case 343: return "\u2AE7"; // &Barv;
			case 348: return "\u22BD"; // &barvee;
			case 352: return "\u2306"; // &Barwed;
			case 356: return "\u2305"; // &barwed;
			case 359: return "\u2305"; // &barwedge;
			case 363: return "\u23B5"; // &bbrk;
			case 368: return "\u23B6"; // &bbrktbrk;
			case 373: return "\u224C"; // &bcong;
			case 376: return "\u0411"; // &Bcy;
			case 378: return "\u0431"; // &bcy;
			case 383: return "\u201E"; // &bdquo;
			case 389: return "\u2235"; // &becaus;
			case 396: return "\u2235"; // &Because;
			case 398: return "\u2235"; // &because;
			case 404: return "\u29B0"; // &bemptyv;
			case 408: return "\u03F6"; // &bepsi;
			case 413: return "\u212C"; // &bernou;
			case 422: return "\u212C"; // &Bernoullis;
			case 425: return "\u0392"; // &Beta;
			case 428: return "\u03B2"; // &beta;
			case 430: return "\u2136"; // &beth;
			case 435: return "\u226C"; // &between;
			case 438: return "\uD835\uDD05"; // &Bfr;
			case 441: return "\uD835\uDD1F"; // &bfr;
			case 447: return "\u22C2"; // &bigcap;
			case 451: return "\u25EF"; // &bigcirc;
			case 454: return "\u22C3"; // &bigcup;
			case 459: return "\u2A00"; // &bigodot;
			case 464: return "\u2A01"; // &bigoplus;
			case 470: return "\u2A02"; // &bigotimes;
			case 476: return "\u2A06"; // &bigsqcup;
			case 480: return "\u2605"; // &bigstar;
			case 493: return "\u25BD"; // &bigtriangledown;
			case 496: return "\u25B3"; // &bigtriangleup;
			case 502: return "\u2A04"; // &biguplus;
			case 506: return "\u22C1"; // &bigvee;
			case 512: return "\u22C0"; // &bigwedge;
			case 518: return "\u290D"; // &bkarow;
			case 530: return "\u29EB"; // &blacklozenge;
			case 537: return "\u25AA"; // &blacksquare;
			case 546: return "\u25B4"; // &blacktriangle;
			case 551: return "\u25BE"; // &blacktriangledown;
			case 556: return "\u25C2"; // &blacktriangleleft;
			case 562: return "\u25B8"; // &blacktriangleright;
			case 565: return "\u2423"; // &blank;
			case 569: return "\u2592"; // &blk12;
			case 571: return "\u2591"; // &blk14;
			case 574: return "\u2593"; // &blk34;
			case 578: return "\u2588"; // &block;
			case 581: return "\u003D\u20E5"; // &bne;
			case 586: return "\u2261\u20E5"; // &bnequiv;
			case 590: return "\u2AED"; // &bNot;
			case 593: return "\u2310"; // &bnot;
			case 597: return "\uD835\uDD39"; // &Bopf;
			case 601: return "\uD835\uDD53"; // &bopf;
			case 603: return "\u22A5"; // &bot;
			case 607: return "\u22A5"; // &bottom;
			case 612: return "\u22C8"; // &bowtie;
			case 617: return "\u29C9"; // &boxbox;
			case 620: return "\u2557"; // &boxDL;
			case 622: return "\u2556"; // &boxDl;
			case 625: return "\u2555"; // &boxdL;
			case 627: return "\u2510"; // &boxdl;
			case 629: return "\u2554"; // &boxDR;
			case 631: return "\u2553"; // &boxDr;
			case 633: return "\u2552"; // &boxdR;
			case 635: return "\u250C"; // &boxdr;
			case 637: return "\u2550"; // &boxH;
			case 639: return "\u2500"; // &boxh;
			case 641: return "\u2566"; // &boxHD;
			case 643: return "\u2564"; // &boxHd;
			case 645: return "\u2565"; // &boxhD;
			case 647: return "\u252C"; // &boxhd;
			case 649: return "\u2569"; // &boxHU;
			case 651: return "\u2567"; // &boxHu;
			case 653: return "\u2568"; // &boxhU;
			case 655: return "\u2534"; // &boxhu;
			case 661: return "\u229F"; // &boxminus;
			case 666: return "\u229E"; // &boxplus;
			case 672: return "\u22A0"; // &boxtimes;
			case 675: return "\u255D"; // &boxUL;
			case 677: return "\u255C"; // &boxUl;
			case 680: return "\u255B"; // &boxuL;
			case 682: return "\u2518"; // &boxul;
			case 684: return "\u255A"; // &boxUR;
			case 686: return "\u2559"; // &boxUr;
			case 688: return "\u2558"; // &boxuR;
			case 690: return "\u2514"; // &boxur;
			case 692: return "\u2551"; // &boxV;
			case 694: return "\u2502"; // &boxv;
			case 696: return "\u256C"; // &boxVH;
			case 698: return "\u256B"; // &boxVh;
			case 700: return "\u256A"; // &boxvH;
			case 702: return "\u253C"; // &boxvh;
			case 704: return "\u2563"; // &boxVL;
			case 706: return "\u2562"; // &boxVl;
			case 708: return "\u2561"; // &boxvL;
			case 710: return "\u2524"; // &boxvl;
			case 712: return "\u2560"; // &boxVR;
			case 714: return "\u255F"; // &boxVr;
			case 716: return "\u255E"; // &boxvR;
			case 718: return "\u251C"; // &boxvr;
			case 724: return "\u2035"; // &bprime;
			case 729: return "\u02D8"; // &Breve;
			case 734: return "\u02D8"; // &breve;
			case 738: return "\u00A6"; // &brvbar
			case 739: return "\u00A6"; // &brvbar;
			case 743: return "\u212C"; // &Bscr;
			case 747: return "\uD835\uDCB7"; // &bscr;
			case 751: return "\u204F"; // &bsemi;
			case 754: return "\u223D"; // &bsim;
			case 756: return "\u22CD"; // &bsime;
			case 759: return "\u005C"; // &bsol;
			case 761: return "\u29C5"; // &bsolb;
			case 766: return "\u27C8"; // &bsolhsub;
			case 770: return "\u2022"; // &bull;
			case 773: return "\u2022"; // &bullet;
			case 776: return "\u224E"; // &bump;
			case 778: return "\u2AAE"; // &bumpE;
			case 780: return "\u224F"; // &bumpe;
			case 786: return "\u224E"; // &Bumpeq;
			case 788: return "\u224F"; // &bumpeq;
			case 795: return "\u0106"; // &Cacute;
			case 802: return "\u0107"; // &cacute;
			case 804: return "\u22D2"; // &Cap;
			case 806: return "\u2229"; // &cap;
			case 810: return "\u2A44"; // &capand;
			case 816: return "\u2A49"; // &capbrcup;
			case 820: return "\u2A4B"; // &capcap;
			case 823: return "\u2A47"; // &capcup;
			case 827: return "\u2A40"; // &capdot;
			case 845: return "\u2145"; // &CapitalDifferentialD;
			case 847: return "\u2229\uFE00"; // &caps;
			case 851: return "\u2041"; // &caret;
			case 854: return "\u02C7"; // &caron;
			case 860: return "\u212D"; // &Cayleys;
			case 865: return "\u2A4D"; // &ccaps;
			case 871: return "\u010C"; // &Ccaron;
			case 875: return "\u010D"; // &ccaron;
			case 879: return "\u00C7"; // &Ccedil
			case 880: return "\u00C7"; // &Ccedil;
			case 884: return "\u00E7"; // &ccedil
			case 885: return "\u00E7"; // &ccedil;
			case 889: return "\u0108"; // &Ccirc;
			case 893: return "\u0109"; // &ccirc;
			case 899: return "\u2230"; // &Cconint;
			case 903: return "\u2A4C"; // &ccups;
			case 906: return "\u2A50"; // &ccupssm;
			case 910: return "\u010A"; // &Cdot;
			case 914: return "\u010B"; // &cdot;
			case 918: return "\u00B8"; // &cedil
			case 919: return "\u00B8"; // &cedil;
			case 926: return "\u00B8"; // &Cedilla;
			case 932: return "\u29B2"; // &cemptyv;
			case 934: return "\u00A2"; // &cent
			case 935: return "\u00A2"; // &cent;
			case 943: return "\u00B7"; // &CenterDot;
			case 949: return "\u00B7"; // &centerdot;
			case 952: return "\u212D"; // &Cfr;
			case 955: return "\uD835\uDD20"; // &cfr;
			case 959: return "\u0427"; // &CHcy;
			case 963: return "\u0447"; // &chcy;
			case 967: return "\u2713"; // &check;
			case 972: return "\u2713"; // &checkmark;
			case 975: return "\u03A7"; // &Chi;
			case 977: return "\u03C7"; // &chi;
			case 980: return "\u25CB"; // &cir;
			case 982: return "\u02C6"; // &circ;
			case 985: return "\u2257"; // &circeq;
			case 997: return "\u21BA"; // &circlearrowleft;
			case 1003: return "\u21BB"; // &circlearrowright;
			case 1008: return "\u229B"; // &circledast;
			case 1013: return "\u229A"; // &circledcirc;
			case 1018: return "\u229D"; // &circleddash;
			case 1027: return "\u2299"; // &CircleDot;
			case 1029: return "\u00AE"; // &circledR;
			case 1031: return "\u24C8"; // &circledS;
			case 1037: return "\u2296"; // &CircleMinus;
			case 1042: return "\u2295"; // &CirclePlus;
			case 1048: return "\u2297"; // &CircleTimes;
			case 1050: return "\u29C3"; // &cirE;
			case 1052: return "\u2257"; // &cire;
			case 1058: return "\u2A10"; // &cirfnint;
			case 1062: return "\u2AEF"; // &cirmid;
			case 1067: return "\u29C2"; // &cirscir;
			case 1091: return "\u2232"; // &ClockwiseContourIntegral;
			case 1110: return "\u201D"; // &CloseCurlyDoubleQuote;
			case 1116: return "\u2019"; // &CloseCurlyQuote;
			case 1121: return "\u2663"; // &clubs;
			case 1125: return "\u2663"; // &clubsuit;
			case 1130: return "\u2237"; // &Colon;
			case 1135: return "\u003A"; // &colon;
			case 1137: return "\u2A74"; // &Colone;
			case 1139: return "\u2254"; // &colone;
			case 1141: return "\u2254"; // &coloneq;
			case 1145: return "\u002C"; // &comma;
			case 1147: return "\u0040"; // &commat;
			case 1149: return "\u2201"; // &comp;
			case 1152: return "\u2218"; // &compfn;
			case 1159: return "\u2201"; // &complement;
			case 1163: return "\u2102"; // &complexes;
			case 1166: return "\u2245"; // &cong;
			case 1170: return "\u2A6D"; // &congdot;
			case 1178: return "\u2261"; // &Congruent;
			case 1182: return "\u222F"; // &Conint;
			case 1186: return "\u222E"; // &conint;
			case 1199: return "\u222E"; // &ContourIntegral;
			case 1202: return "\u2102"; // &Copf;
			case 1205: return "\uD835\uDD54"; // &copf;
			case 1209: return "\u2210"; // &coprod;
			case 1216: return "\u2210"; // &Coproduct;
			case 1219: return "\u00A9"; // &COPY
			case 1220: return "\u00A9"; // &COPY;
			case 1221: return "\u00A9"; // &copy
			case 1222: return "\u00A9"; // &copy;
			case 1225: return "\u2117"; // &copysr;
			case 1255: return "\u2233"; // &CounterClockwiseContourIntegral;
			case 1260: return "\u21B5"; // &crarr;
			case 1265: return "\u2A2F"; // &Cross;
			case 1269: return "\u2717"; // &cross;
			case 1273: return "\uD835\uDC9E"; // &Cscr;
			case 1277: return "\uD835\uDCB8"; // &cscr;
			case 1280: return "\u2ACF"; // &csub;
			case 1282: return "\u2AD1"; // &csube;
			case 1284: return "\u2AD0"; // &csup;
			case 1286: return "\u2AD2"; // &csupe;
			case 1291: return "\u22EF"; // &ctdot;
			case 1298: return "\u2938"; // &cudarrl;
			case 1300: return "\u2935"; // &cudarrr;
			case 1304: return "\u22DE"; // &cuepr;
			case 1307: return "\u22DF"; // &cuesc;
			case 1312: return "\u21B6"; // &cularr;
			case 1314: return "\u293D"; // &cularrp;
			case 1317: return "\u22D3"; // &Cup;
			case 1319: return "\u222A"; // &cup;
			case 1325: return "\u2A48"; // &cupbrcap;
			case 1329: return "\u224D"; // &CupCap;
			case 1333: return "\u2A46"; // &cupcap;
			case 1336: return "\u2A4A"; // &cupcup;
			case 1340: return "\u228D"; // &cupdot;
			case 1343: return "\u2A45"; // &cupor;
			case 1345: return "\u222A\uFE00"; // &cups;
			case 1350: return "\u21B7"; // &curarr;
			case 1352: return "\u293C"; // &curarrm;
			case 1361: return "\u22DE"; // &curlyeqprec;
			case 1366: return "\u22DF"; // &curlyeqsucc;
			case 1370: return "\u22CE"; // &curlyvee;
			case 1376: return "\u22CF"; // &curlywedge;
			case 1379: return "\u00A4"; // &curren
			case 1380: return "\u00A4"; // &curren;
			case 1392: return "\u21B6"; // &curvearrowleft;
			case 1398: return "\u21B7"; // &curvearrowright;
			case 1402: return "\u22CE"; // &cuvee;
			case 1406: return "\u22CF"; // &cuwed;
			case 1414: return "\u2232"; // &cwconint;
			case 1418: return "\u2231"; // &cwint;
			case 1424: return "\u232D"; // &cylcty;
			case 1431: return "\u2021"; // &Dagger;
			case 1438: return "\u2020"; // &dagger;
			case 1443: return "\u2138"; // &daleth;
			case 1446: return "\u21A1"; // &Darr;
			case 1450: return "\u21D3"; // &dArr;
			case 1453: return "\u2193"; // &darr;
			case 1456: return "\u2010"; // &dash;
			case 1460: return "\u2AE4"; // &Dashv;
			case 1462: return "\u22A3"; // &dashv;
			case 1469: return "\u290F"; // &dbkarow;
			case 1473: return "\u02DD"; // &dblac;
			case 1479: return "\u010E"; // &Dcaron;
			case 1485: return "\u010F"; // &dcaron;
			case 1487: return "\u0414"; // &Dcy;
			case 1489: return "\u0434"; // &dcy;
			case 1491: return "\u2145"; // &DD;
			case 1493: return "\u2146"; // &dd;
			case 1499: return "\u2021"; // &ddagger;
			case 1502: return "\u21CA"; // &ddarr;
			case 1509: return "\u2911"; // &DDotrahd;
			case 1515: return "\u2A77"; // &ddotseq;
			case 1517: return "\u00B0"; // &deg
			case 1518: return "\u00B0"; // &deg;
			case 1521: return "\u2207"; // &Del;
			case 1524: return "\u0394"; // &Delta;
			case 1528: return "\u03B4"; // &delta;
			case 1534: return "\u29B1"; // &demptyv;
			case 1540: return "\u297F"; // &dfisht;
			case 1543: return "\uD835\uDD07"; // &Dfr;
			case 1545: return "\uD835\uDD21"; // &dfr;
			case 1549: return "\u2965"; // &dHar;
			case 1554: return "\u21C3"; // &dharl;
			case 1556: return "\u21C2"; // &dharr;
			case 1572: return "\u00B4"; // &DiacriticalAcute;
			case 1576: return "\u02D9"; // &DiacriticalDot;
			case 1586: return "\u02DD"; // &DiacriticalDoubleAcute;
			case 1592: return "\u0060"; // &DiacriticalGrave;
			case 1598: return "\u02DC"; // &DiacriticalTilde;
			case 1602: return "\u22C4"; // &diam;
			case 1607: return "\u22C4"; // &Diamond;
			case 1611: return "\u22C4"; // &diamond;
			case 1616: return "\u2666"; // &diamondsuit;
			case 1618: return "\u2666"; // &diams;
			case 1620: return "\u00A8"; // &die;
			case 1632: return "\u2146"; // &DifferentialD;
			case 1638: return "\u03DD"; // &digamma;
			case 1642: return "\u22F2"; // &disin;
			case 1644: return "\u00F7"; // &div;
			case 1647: return "\u00F7"; // &divide
			case 1648: return "\u00F7"; // &divide;
			case 1656: return "\u22C7"; // &divideontimes;
			case 1660: return "\u22C7"; // &divonx;
			case 1664: return "\u0402"; // &DJcy;
			case 1668: return "\u0452"; // &djcy;
			case 1674: return "\u231E"; // &dlcorn;
			case 1678: return "\u230D"; // &dlcrop;
			case 1684: return "\u0024"; // &dollar;
			case 1688: return "\uD835\uDD3B"; // &Dopf;
			case 1691: return "\uD835\uDD55"; // &dopf;
			case 1693: return "\u00A8"; // &Dot;
			case 1695: return "\u02D9"; // &dot;
			case 1699: return "\u20DC"; // &DotDot;
			case 1702: return "\u2250"; // &doteq;
			case 1706: return "\u2251"; // &doteqdot;
			case 1712: return "\u2250"; // &DotEqual;
			case 1718: return "\u2238"; // &dotminus;
			case 1723: return "\u2214"; // &dotplus;
			case 1730: return "\u22A1"; // &dotsquare;
			case 1743: return "\u2306"; // &doublebarwedge;
			case 1763: return "\u222F"; // &DoubleContourIntegral;
			case 1767: return "\u00A8"; // &DoubleDot;
			case 1775: return "\u21D3"; // &DoubleDownArrow;
			case 1785: return "\u21D0"; // &DoubleLeftArrow;
			case 1796: return "\u21D4"; // &DoubleLeftRightArrow;
			case 1800: return "\u2AE4"; // &DoubleLeftTee;
			case 1813: return "\u27F8"; // &DoubleLongLeftArrow;
			case 1824: return "\u27FA"; // &DoubleLongLeftRightArrow;
			case 1835: return "\u27F9"; // &DoubleLongRightArrow;
			case 1846: return "\u21D2"; // &DoubleRightArrow;
			case 1850: return "\u22A8"; // &DoubleRightTee;
			case 1858: return "\u21D1"; // &DoubleUpArrow;
			case 1868: return "\u21D5"; // &DoubleUpDownArrow;
			case 1880: return "\u2225"; // &DoubleVerticalBar;
			case 1888: return "\u2193"; // &DownArrow;
			case 1894: return "\u21D3"; // &Downarrow;
			case 1902: return "\u2193"; // &downarrow;
			case 1906: return "\u2913"; // &DownArrowBar;
			case 1914: return "\u21F5"; // &DownArrowUpArrow;
			case 1920: return "\u0311"; // &DownBreve;
			case 1931: return "\u21CA"; // &downdownarrows;
			case 1943: return "\u21C3"; // &downharpoonleft;
			case 1949: return "\u21C2"; // &downharpoonright;
			case 1965: return "\u2950"; // &DownLeftRightVector;
			case 1975: return "\u295E"; // &DownLeftTeeVector;
			case 1982: return "\u21BD"; // &DownLeftVector;
			case 1986: return "\u2956"; // &DownLeftVectorBar;
			case 2001: return "\u295F"; // &DownRightTeeVector;
			case 2008: return "\u21C1"; // &DownRightVector;
			case 2012: return "\u2957"; // &DownRightVectorBar;
			case 2016: return "\u22A4"; // &DownTee;
			case 2022: return "\u21A7"; // &DownTeeArrow;
			case 2030: return "\u2910"; // &drbkarow;
			case 2035: return "\u231F"; // &drcorn;
			case 2039: return "\u230C"; // &drcrop;
			case 2043: return "\uD835\uDC9F"; // &Dscr;
			case 2047: return "\uD835\uDCB9"; // &dscr;
			case 2051: return "\u0405"; // &DScy;
			case 2053: return "\u0455"; // &dscy;
			case 2056: return "\u29F6"; // &dsol;
			case 2061: return "\u0110"; // &Dstrok;
			case 2066: return "\u0111"; // &dstrok;
			case 2071: return "\u22F1"; // &dtdot;
			case 2074: return "\u25BF"; // &dtri;
			case 2076: return "\u25BE"; // &dtrif;
			case 2081: return "\u21F5"; // &duarr;
			case 2085: return "\u296F"; // &duhar;
			case 2092: return "\u29A6"; // &dwangle;
			case 2096: return "\u040F"; // &DZcy;
			case 2100: return "\u045F"; // &dzcy;
			case 2107: return "\u27FF"; // &dzigrarr;
			case 2113: return "\u00C9"; // &Eacute
			case 2114: return "\u00C9"; // &Eacute;
			case 2120: return "\u00E9"; // &eacute
			case 2121: return "\u00E9"; // &eacute;
			case 2126: return "\u2A6E"; // &easter;
			case 2132: return "\u011A"; // &Ecaron;
			case 2138: return "\u011B"; // &ecaron;
			case 2141: return "\u2256"; // &ecir;
			case 2144: return "\u00CA"; // &Ecirc
			case 2145: return "\u00CA"; // &Ecirc;
			case 2146: return "\u00EA"; // &ecirc
			case 2147: return "\u00EA"; // &ecirc;
			case 2152: return "\u2255"; // &ecolon;
			case 2154: return "\u042D"; // &Ecy;
			case 2156: return "\u044D"; // &ecy;
			case 2161: return "\u2A77"; // &eDDot;
			case 2165: return "\u0116"; // &Edot;
			case 2168: return "\u2251"; // &eDot;
			case 2172: return "\u0117"; // &edot;
			case 2174: return "\u2147"; // &ee;
			case 2179: return "\u2252"; // &efDot;
			case 2182: return "\uD835\uDD08"; // &Efr;
			case 2184: return "\uD835\uDD22"; // &efr;
			case 2186: return "\u2A9A"; // &eg;
			case 2191: return "\u00C8"; // &Egrave
			case 2192: return "\u00C8"; // &Egrave;
			case 2196: return "\u00E8"; // &egrave
			case 2197: return "\u00E8"; // &egrave;
			case 2199: return "\u2A96"; // &egs;
			case 2203: return "\u2A98"; // &egsdot;
			case 2205: return "\u2A99"; // &el;
			case 2212: return "\u2208"; // &Element;
			case 2219: return "\u23E7"; // &elinters;
			case 2221: return "\u2113"; // &ell;
			case 2223: return "\u2A95"; // &els;
			case 2227: return "\u2A97"; // &elsdot;
			case 2232: return "\u0112"; // &Emacr;
			case 2237: return "\u0113"; // &emacr;
			case 2241: return "\u2205"; // &empty;
			case 2245: return "\u2205"; // &emptyset;
			case 2260: return "\u25FB"; // &EmptySmallSquare;
			case 2262: return "\u2205"; // &emptyv;
			case 2278: return "\u25AB"; // &EmptyVerySmallSquare;
			case 2281: return "\u2003"; // &emsp;
			case 2284: return "\u2004"; // &emsp13;
			case 2286: return "\u2005"; // &emsp14;
			case 2289: return "\u014A"; // &ENG;
			case 2292: return "\u014B"; // &eng;
			case 2295: return "\u2002"; // &ensp;
			case 2300: return "\u0118"; // &Eogon;
			case 2305: return "\u0119"; // &eogon;
			case 2308: return "\uD835\uDD3C"; // &Eopf;
			case 2311: return "\uD835\uDD56"; // &eopf;
			case 2315: return "\u22D5"; // &epar;
			case 2318: return "\u29E3"; // &eparsl;
			case 2322: return "\u2A71"; // &eplus;
			case 2325: return "\u03B5"; // &epsi;
			case 2332: return "\u0395"; // &Epsilon;
			case 2336: return "\u03B5"; // &epsilon;
			case 2338: return "\u03F5"; // &epsiv;
			case 2344: return "\u2256"; // &eqcirc;
			case 2349: return "\u2255"; // &eqcolon;
			case 2353: return "\u2242"; // &eqsim;
			case 2361: return "\u2A96"; // &eqslantgtr;
			case 2366: return "\u2A95"; // &eqslantless;
			case 2371: return "\u2A75"; // &Equal;
			case 2376: return "\u003D"; // &equals;
			case 2382: return "\u2242"; // &EqualTilde;
			case 2386: return "\u225F"; // &equest;
			case 2395: return "\u21CC"; // &Equilibrium;
			case 2398: return "\u2261"; // &equiv;
			case 2401: return "\u2A78"; // &equivDD;
			case 2408: return "\u29E5"; // &eqvparsl;
			case 2413: return "\u2971"; // &erarr;
			case 2417: return "\u2253"; // &erDot;
			case 2421: return "\u2130"; // &Escr;
			case 2425: return "\u212F"; // &escr;
			case 2429: return "\u2250"; // &esdot;
			case 2432: return "\u2A73"; // &Esim;
			case 2435: return "\u2242"; // &esim;
			case 2438: return "\u0397"; // &Eta;
			case 2441: return "\u03B7"; // &eta;
			case 2443: return "\u00D0"; // &ETH
			case 2444: return "\u00D0"; // &ETH;
			case 2445: return "\u00F0"; // &eth
			case 2446: return "\u00F0"; // &eth;
			case 2449: return "\u00CB"; // &Euml
			case 2450: return "\u00CB"; // &Euml;
			case 2453: return "\u00EB"; // &euml
			case 2454: return "\u00EB"; // &euml;
			case 2457: return "\u20AC"; // &euro;
			case 2461: return "\u0021"; // &excl;
			case 2465: return "\u2203"; // &exist;
			case 2471: return "\u2203"; // &Exists;
			case 2481: return "\u2130"; // &expectation;
			case 2492: return "\u2147"; // &ExponentialE;
			case 2502: return "\u2147"; // &exponentiale;
			case 2516: return "\u2252"; // &fallingdotseq;
			case 2520: return "\u0424"; // &Fcy;
			case 2523: return "\u0444"; // &fcy;
			case 2529: return "\u2640"; // &female;
			case 2535: return "\uFB03"; // &ffilig;
			case 2539: return "\uFB00"; // &fflig;
			case 2543: return "\uFB04"; // &ffllig;
			case 2546: return "\uD835\uDD09"; // &Ffr;
			case 2548: return "\uD835\uDD23"; // &ffr;
			case 2553: return "\uFB01"; // &filig;
			case 2570: return "\u25FC"; // &FilledSmallSquare;
			case 2586: return "\u25AA"; // &FilledVerySmallSquare;
			case 2591: return "\u0066\u006A"; // &fjlig;
			case 2595: return "\u266D"; // &flat;
			case 2599: return "\uFB02"; // &fllig;
			case 2603: return "\u25B1"; // &fltns;
			case 2607: return "\u0192"; // &fnof;
			case 2611: return "\uD835\uDD3D"; // &Fopf;
			case 2615: return "\uD835\uDD57"; // &fopf;
			case 2620: return "\u2200"; // &ForAll;
			case 2625: return "\u2200"; // &forall;
			case 2627: return "\u22D4"; // &fork;
			case 2629: return "\u2AD9"; // &forkv;
			case 2638: return "\u2131"; // &Fouriertrf;
			case 2646: return "\u2A0D"; // &fpartint;
			case 2651: return "\u00BD"; // &frac12
			case 2652: return "\u00BD"; // &frac12;
			case 2654: return "\u2153"; // &frac13;
			case 2655: return "\u00BC"; // &frac14
			case 2656: return "\u00BC"; // &frac14;
			case 2658: return "\u2155"; // &frac15;
			case 2660: return "\u2159"; // &frac16;
			case 2662: return "\u215B"; // &frac18;
			case 2665: return "\u2154"; // &frac23;
			case 2667: return "\u2156"; // &frac25;
			case 2669: return "\u00BE"; // &frac34
			case 2670: return "\u00BE"; // &frac34;
			case 2672: return "\u2157"; // &frac35;
			case 2674: return "\u215C"; // &frac38;
			case 2677: return "\u2158"; // &frac45;
			case 2680: return "\u215A"; // &frac56;
			case 2682: return "\u215D"; // &frac58;
			case 2685: return "\u215E"; // &frac78;
			case 2688: return "\u2044"; // &frasl;
			case 2692: return "\u2322"; // &frown;
			case 2696: return "\u2131"; // &Fscr;
			case 2700: return "\uD835\uDCBB"; // &fscr;
			case 2707: return "\u01F5"; // &gacute;
			case 2713: return "\u0393"; // &Gamma;
			case 2717: return "\u03B3"; // &gamma;
			case 2719: return "\u03DC"; // &Gammad;
			case 2721: return "\u03DD"; // &gammad;
			case 2723: return "\u2A86"; // &gap;
			case 2729: return "\u011E"; // &Gbreve;
			case 2735: return "\u011F"; // &gbreve;
			case 2741: return "\u0122"; // &Gcedil;
			case 2745: return "\u011C"; // &Gcirc;
			case 2750: return "\u011D"; // &gcirc;
			case 2752: return "\u0413"; // &Gcy;
			case 2754: return "\u0433"; // &gcy;
			case 2758: return "\u0120"; // &Gdot;
			case 2762: return "\u0121"; // &gdot;
			case 2764: return "\u2267"; // &gE;
			case 2766: return "\u2265"; // &ge;
			case 2768: return "\u2A8C"; // &gEl;
			case 2770: return "\u22DB"; // &gel;
			case 2772: return "\u2265"; // &geq;
			case 2774: return "\u2267"; // &geqq;
			case 2780: return "\u2A7E"; // &geqslant;
			case 2782: return "\u2A7E"; // &ges;
			case 2785: return "\u2AA9"; // &gescc;
			case 2789: return "\u2A80"; // &gesdot;
			case 2791: return "\u2A82"; // &gesdoto;
			case 2793: return "\u2A84"; // &gesdotol;
			case 2795: return "\u22DB\uFE00"; // &gesl;
			case 2798: return "\u2A94"; // &gesles;
			case 2801: return "\uD835\uDD0A"; // &Gfr;
			case 2804: return "\uD835\uDD24"; // &gfr;
			case 2806: return "\u22D9"; // &Gg;
			case 2808: return "\u226B"; // &gg;
			case 2810: return "\u22D9"; // &ggg;
			case 2815: return "\u2137"; // &gimel;
			case 2819: return "\u0403"; // &GJcy;
			case 2823: return "\u0453"; // &gjcy;
			case 2825: return "\u2277"; // &gl;
			case 2827: return "\u2AA5"; // &gla;
			case 2829: return "\u2A92"; // &glE;
			case 2831: return "\u2AA4"; // &glj;
			case 2835: return "\u2A8A"; // &gnap;
			case 2840: return "\u2A8A"; // &gnapprox;
			case 2842: return "\u2269"; // &gnE;
			case 2844: return "\u2A88"; // &gne;
			case 2846: return "\u2A88"; // &gneq;
			case 2848: return "\u2269"; // &gneqq;
			case 2852: return "\u22E7"; // &gnsim;
			case 2856: return "\uD835\uDD3E"; // &Gopf;
			case 2860: return "\uD835\uDD58"; // &gopf;
			case 2865: return "\u0060"; // &grave;
			case 2877: return "\u2265"; // &GreaterEqual;
			case 2882: return "\u22DB"; // &GreaterEqualLess;
			case 2892: return "\u2267"; // &GreaterFullEqual;
			case 2900: return "\u2AA2"; // &GreaterGreater;
			case 2905: return "\u2277"; // &GreaterLess;
			case 2916: return "\u2A7E"; // &GreaterSlantEqual;
			case 2922: return "\u2273"; // &GreaterTilde;
			case 2926: return "\uD835\uDCA2"; // &Gscr;
			case 2930: return "\u210A"; // &gscr;
			case 2933: return "\u2273"; // &gsim;
			case 2935: return "\u2A8E"; // &gsime;
			case 2937: return "\u2A90"; // &gsiml;
			case 2938: return "\u003E"; // &GT
			case 2939: return "\u003E"; // &GT;
			case 2941: return "\u226B"; // &Gt;
			case 2942: return "\u003E"; // &gt
			case 2943: return "\u003E"; // &gt;
			case 2946: return "\u2AA7"; // &gtcc;
			case 2949: return "\u2A7A"; // &gtcir;
			case 2953: return "\u22D7"; // &gtdot;
			case 2958: return "\u2995"; // &gtlPar;
			case 2964: return "\u2A7C"; // &gtquest;
			case 2972: return "\u2A86"; // &gtrapprox;
			case 2975: return "\u2978"; // &gtrarr;
			case 2979: return "\u22D7"; // &gtrdot;
			case 2986: return "\u22DB"; // &gtreqless;
			case 2992: return "\u2A8C"; // &gtreqqless;
			case 2997: return "\u2277"; // &gtrless;
			case 3001: return "\u2273"; // &gtrsim;
			case 3010: return "\u2269\uFE00"; // &gvertneqq;
			case 3013: return "\u2269\uFE00"; // &gvnE;
			case 3019: return "\u02C7"; // &Hacek;
			case 3026: return "\u200A"; // &hairsp;
			case 3029: return "\u00BD"; // &half;
			case 3034: return "\u210B"; // &hamilt;
			case 3040: return "\u042A"; // &HARDcy;
			case 3045: return "\u044A"; // &hardcy;
			case 3049: return "\u21D4"; // &hArr;
			case 3051: return "\u2194"; // &harr;
			case 3055: return "\u2948"; // &harrcir;
			case 3057: return "\u21AD"; // &harrw;
			case 3059: return "\u005E"; // &Hat;
			case 3063: return "\u210F"; // &hbar;
			case 3068: return "\u0124"; // &Hcirc;
			case 3073: return "\u0125"; // &hcirc;
			case 3079: return "\u2665"; // &hearts;
			case 3083: return "\u2665"; // &heartsuit;
			case 3088: return "\u2026"; // &hellip;
			case 3093: return "\u22B9"; // &hercon;
			case 3096: return "\u210C"; // &Hfr;
			case 3099: return "\uD835\uDD25"; // &hfr;
			case 3111: return "\u210B"; // &HilbertSpace;
			case 3119: return "\u2925"; // &hksearow;
			case 3125: return "\u2926"; // &hkswarow;
			case 3130: return "\u21FF"; // &hoarr;
			case 3135: return "\u223B"; // &homtht;
			case 3147: return "\u21A9"; // &hookleftarrow;
			case 3158: return "\u21AA"; // &hookrightarrow;
			case 3162: return "\u210D"; // &Hopf;
			case 3165: return "\uD835\uDD59"; // &hopf;
			case 3170: return "\u2015"; // &horbar;
			case 3183: return "\u2500"; // &HorizontalLine;
			case 3187: return "\u210B"; // &Hscr;
			case 3191: return "\uD835\uDCBD"; // &hscr;
			case 3196: return "\u210F"; // &hslash;
			case 3201: return "\u0126"; // &Hstrok;
			case 3206: return "\u0127"; // &hstrok;
			case 3218: return "\u224E"; // &HumpDownHump;
			case 3224: return "\u224F"; // &HumpEqual;
			case 3230: return "\u2043"; // &hybull;
			case 3235: return "\u2010"; // &hyphen;
			case 3241: return "\u00CD"; // &Iacute
			case 3242: return "\u00CD"; // &Iacute;
			case 3248: return "\u00ED"; // &iacute
			case 3249: return "\u00ED"; // &iacute;
			case 3251: return "\u2063"; // &ic;
			case 3255: return "\u00CE"; // &Icirc
			case 3256: return "\u00CE"; // &Icirc;
			case 3259: return "\u00EE"; // &icirc
			case 3260: return "\u00EE"; // &icirc;
			case 3262: return "\u0418"; // &Icy;
			case 3264: return "\u0438"; // &icy;
			case 3268: return "\u0130"; // &Idot;
			case 3272: return "\u0415"; // &IEcy;
			case 3276: return "\u0435"; // &iecy;
			case 3279: return "\u00A1"; // &iexcl
			case 3280: return "\u00A1"; // &iexcl;
			case 3283: return "\u21D4"; // &iff;
			case 3286: return "\u2111"; // &Ifr;
			case 3288: return "\uD835\uDD26"; // &ifr;
			case 3293: return "\u00CC"; // &Igrave
			case 3294: return "\u00CC"; // &Igrave;
			case 3299: return "\u00EC"; // &igrave
			case 3300: return "\u00EC"; // &igrave;
			case 3302: return "\u2148"; // &ii;
			case 3307: return "\u2A0C"; // &iiiint;
			case 3310: return "\u222D"; // &iiint;
			case 3315: return "\u29DC"; // &iinfin;
			case 3319: return "\u2129"; // &iiota;
			case 3324: return "\u0132"; // &IJlig;
			case 3329: return "\u0133"; // &ijlig;
			case 3331: return "\u2111"; // &Im;
			case 3335: return "\u012A"; // &Imacr;
			case 3340: return "\u012B"; // &imacr;
			case 3343: return "\u2111"; // &image;
			case 3351: return "\u2148"; // &ImaginaryI;
			case 3356: return "\u2110"; // &imagline;
			case 3361: return "\u2111"; // &imagpart;
			case 3364: return "\u0131"; // &imath;
			case 3367: return "\u22B7"; // &imof;
			case 3371: return "\u01B5"; // &imped;
			case 3377: return "\u21D2"; // &Implies;
			case 3379: return "\u2208"; // &in;
			case 3384: return "\u2105"; // &incare;
			case 3388: return "\u221E"; // &infin;
			case 3392: return "\u29DD"; // &infintie;
			case 3397: return "\u0131"; // &inodot;
			case 3400: return "\u222C"; // &Int;
			case 3402: return "\u222B"; // &int;
			case 3406: return "\u22BA"; // &intcal;
			case 3412: return "\u2124"; // &integers;
			case 3418: return "\u222B"; // &Integral;
			case 3423: return "\u22BA"; // &intercal;
			case 3432: return "\u22C2"; // &Intersection;
			case 3438: return "\u2A17"; // &intlarhk;
			case 3443: return "\u2A3C"; // &intprod;
			case 3456: return "\u2063"; // &InvisibleComma;
			case 3462: return "\u2062"; // &InvisibleTimes;
			case 3466: return "\u0401"; // &IOcy;
			case 3470: return "\u0451"; // &iocy;
			case 3475: return "\u012E"; // &Iogon;
			case 3479: return "\u012F"; // &iogon;
			case 3482: return "\uD835\uDD40"; // &Iopf;
			case 3485: return "\uD835\uDD5A"; // &iopf;
			case 3488: return "\u0399"; // &Iota;
			case 3491: return "\u03B9"; // &iota;
			case 3496: return "\u2A3C"; // &iprod;
			case 3501: return "\u00BF"; // &iquest
			case 3502: return "\u00BF"; // &iquest;
			case 3506: return "\u2110"; // &Iscr;
			case 3510: return "\uD835\uDCBE"; // &iscr;
			case 3513: return "\u2208"; // &isin;
			case 3517: return "\u22F5"; // &isindot;
			case 3519: return "\u22F9"; // &isinE;
			case 3521: return "\u22F4"; // &isins;
			case 3523: return "\u22F3"; // &isinsv;
			case 3525: return "\u2208"; // &isinv;
			case 3527: return "\u2062"; // &it;
			case 3533: return "\u0128"; // &Itilde;
			case 3538: return "\u0129"; // &itilde;
			case 3543: return "\u0406"; // &Iukcy;
			case 3548: return "\u0456"; // &iukcy;
			case 3550: return "\u00CF"; // &Iuml
			case 3551: return "\u00CF"; // &Iuml;
			case 3553: return "\u00EF"; // &iuml
			case 3554: return "\u00EF"; // &iuml;
			case 3560: return "\u0134"; // &Jcirc;
			case 3566: return "\u0135"; // &jcirc;
			case 3568: return "\u0419"; // &Jcy;
			case 3570: return "\u0439"; // &jcy;
			case 3573: return "\uD835\uDD0D"; // &Jfr;
			case 3576: return "\uD835\uDD27"; // &jfr;
			case 3581: return "\u0237"; // &jmath;
			case 3585: return "\uD835\uDD41"; // &Jopf;
			case 3589: return "\uD835\uDD5B"; // &jopf;
			case 3593: return "\uD835\uDCA5"; // &Jscr;
			case 3597: return "\uD835\uDCBF"; // &jscr;
			case 3602: return "\u0408"; // &Jsercy;
			case 3607: return "\u0458"; // &jsercy;
			case 3612: return "\u0404"; // &Jukcy;
			case 3617: return "\u0454"; // &jukcy;
			case 3623: return "\u039A"; // &Kappa;
			case 3629: return "\u03BA"; // &kappa;
			case 3631: return "\u03F0"; // &kappav;
			case 3637: return "\u0136"; // &Kcedil;
			case 3643: return "\u0137"; // &kcedil;
			case 3645: return "\u041A"; // &Kcy;
			case 3647: return "\u043A"; // &kcy;
			case 3650: return "\uD835\uDD0E"; // &Kfr;
			case 3653: return "\uD835\uDD28"; // &kfr;
			case 3659: return "\u0138"; // &kgreen;
			case 3663: return "\u0425"; // &KHcy;
			case 3667: return "\u0445"; // &khcy;
			case 3671: return "\u040C"; // &KJcy;
			case 3675: return "\u045C"; // &kjcy;
			case 3679: return "\uD835\uDD42"; // &Kopf;
			case 3683: return "\uD835\uDD5C"; // &kopf;
			case 3687: return "\uD835\uDCA6"; // &Kscr;
			case 3691: return "\uD835\uDCC0"; // &kscr;
			case 3697: return "\u21DA"; // &lAarr;
			case 3704: return "\u0139"; // &Lacute;
			case 3710: return "\u013A"; // &lacute;
			case 3717: return "\u29B4"; // &laemptyv;
			case 3722: return "\u2112"; // &lagran;
			case 3727: return "\u039B"; // &Lambda;
			case 3732: return "\u03BB"; // &lambda;
			case 3735: return "\u27EA"; // &Lang;
			case 3738: return "\u27E8"; // &lang;
			case 3740: return "\u2991"; // &langd;
			case 3743: return "\u27E8"; // &langle;
			case 3745: return "\u2A85"; // &lap;
			case 3754: return "\u2112"; // &Laplacetrf;
			case 3757: return "\u00AB"; // &laquo
			case 3758: return "\u00AB"; // &laquo;
			case 3761: return "\u219E"; // &Larr;
			case 3764: return "\u21D0"; // &lArr;
			case 3767: return "\u2190"; // &larr;
			case 3769: return "\u21E4"; // &larrb;
			case 3772: return "\u291F"; // &larrbfs;
			case 3775: return "\u291D"; // &larrfs;
			case 3778: return "\u21A9"; // &larrhk;
			case 3781: return "\u21AB"; // &larrlp;
			case 3784: return "\u2939"; // &larrpl;
			case 3788: return "\u2973"; // &larrsim;
			case 3791: return "\u21A2"; // &larrtl;
			case 3793: return "\u2AAB"; // &lat;
			case 3798: return "\u291B"; // &lAtail;
			case 3802: return "\u2919"; // &latail;
			case 3804: return "\u2AAD"; // &late;
			case 3806: return "\u2AAD\uFE00"; // &lates;
			case 3811: return "\u290E"; // &lBarr;
			case 3816: return "\u290C"; // &lbarr;
			case 3820: return "\u2772"; // &lbbrk;
			case 3825: return "\u007B"; // &lbrace;
			case 3827: return "\u005B"; // &lbrack;
			case 3830: return "\u298B"; // &lbrke;
			case 3834: return "\u298F"; // &lbrksld;
			case 3836: return "\u298D"; // &lbrkslu;
			case 3842: return "\u013D"; // &Lcaron;
			case 3848: return "\u013E"; // &lcaron;
			case 3853: return "\u013B"; // &Lcedil;
			case 3858: return "\u013C"; // &lcedil;
			case 3861: return "\u2308"; // &lceil;
			case 3864: return "\u007B"; // &lcub;
			case 3866: return "\u041B"; // &Lcy;
			case 3868: return "\u043B"; // &lcy;
			case 3872: return "\u2936"; // &ldca;
			case 3876: return "\u201C"; // &ldquo;
			case 3878: return "\u201E"; // &ldquor;
			case 3884: return "\u2967"; // &ldrdhar;
			case 3890: return "\u294B"; // &ldrushar;
			case 3893: return "\u21B2"; // &ldsh;
			case 3895: return "\u2266"; // &lE;
			case 3897: return "\u2264"; // &le;
			case 3913: return "\u27E8"; // &LeftAngleBracket;
			case 3918: return "\u2190"; // &LeftArrow;
			case 3924: return "\u21D0"; // &Leftarrow;
			case 3932: return "\u2190"; // &leftarrow;
			case 3936: return "\u21E4"; // &LeftArrowBar;
			case 3947: return "\u21C6"; // &LeftArrowRightArrow;
			case 3952: return "\u21A2"; // &leftarrowtail;
			case 3960: return "\u2308"; // &LeftCeiling;
			case 3974: return "\u27E6"; // &LeftDoubleBracket;
			case 3986: return "\u2961"; // &LeftDownTeeVector;
			case 3993: return "\u21C3"; // &LeftDownVector;
			case 3997: return "\u2959"; // &LeftDownVectorBar;
			case 4003: return "\u230A"; // &LeftFloor;
			case 4015: return "\u21BD"; // &leftharpoondown;
			case 4018: return "\u21BC"; // &leftharpoonup;
			case 4029: return "\u21C7"; // &leftleftarrows;
			case 4040: return "\u2194"; // &LeftRightArrow;
			case 4051: return "\u21D4"; // &Leftrightarrow;
			case 4062: return "\u2194"; // &leftrightarrow;
			case 4064: return "\u21C6"; // &leftrightarrows;
			case 4073: return "\u21CB"; // &leftrightharpoons;
			case 4084: return "\u21AD"; // &leftrightsquigarrow;
			case 4091: return "\u294E"; // &LeftRightVector;
			case 4095: return "\u22A3"; // &LeftTee;
			case 4101: return "\u21A4"; // &LeftTeeArrow;
			case 4108: return "\u295A"; // &LeftTeeVector;
			case 4119: return "\u22CB"; // &leftthreetimes;
			case 4127: return "\u22B2"; // &LeftTriangle;
			case 4131: return "\u29CF"; // &LeftTriangleBar;
			case 4137: return "\u22B4"; // &LeftTriangleEqual;
			case 4150: return "\u2951"; // &LeftUpDownVector;
			case 4160: return "\u2960"; // &LeftUpTeeVector;
			case 4167: return "\u21BF"; // &LeftUpVector;
			case 4171: return "\u2958"; // &LeftUpVectorBar;
			case 4178: return "\u21BC"; // &LeftVector;
			case 4182: return "\u2952"; // &LeftVectorBar;
			case 4184: return "\u2A8B"; // &lEg;
			case 4186: return "\u22DA"; // &leg;
			case 4188: return "\u2264"; // &leq;
			case 4190: return "\u2266"; // &leqq;
			case 4196: return "\u2A7D"; // &leqslant;
			case 4198: return "\u2A7D"; // &les;
			case 4201: return "\u2AA8"; // &lescc;
			case 4205: return "\u2A7F"; // &lesdot;
			case 4207: return "\u2A81"; // &lesdoto;
			case 4209: return "\u2A83"; // &lesdotor;
			case 4211: return "\u22DA\uFE00"; // &lesg;
			case 4214: return "\u2A93"; // &lesges;
			case 4222: return "\u2A85"; // &lessapprox;
			case 4226: return "\u22D6"; // &lessdot;
			case 4232: return "\u22DA"; // &lesseqgtr;
			case 4237: return "\u2A8B"; // &lesseqqgtr;
			case 4252: return "\u22DA"; // &LessEqualGreater;
			case 4262: return "\u2266"; // &LessFullEqual;
			case 4270: return "\u2276"; // &LessGreater;
			case 4274: return "\u2276"; // &lessgtr;
			case 4279: return "\u2AA1"; // &LessLess;
			case 4283: return "\u2272"; // &lesssim;
			case 4294: return "\u2A7D"; // &LessSlantEqual;
			case 4300: return "\u2272"; // &LessTilde;
			case 4306: return "\u297C"; // &lfisht;
			case 4311: return "\u230A"; // &lfloor;
			case 4314: return "\uD835\uDD0F"; // &Lfr;
			case 4316: return "\uD835\uDD29"; // &lfr;
			case 4318: return "\u2276"; // &lg;
			case 4320: return "\u2A91"; // &lgE;
			case 4324: return "\u2962"; // &lHar;
			case 4329: return "\u21BD"; // &lhard;
			case 4331: return "\u21BC"; // &lharu;
			case 4333: return "\u296A"; // &lharul;
			case 4337: return "\u2584"; // &lhblk;
			case 4341: return "\u0409"; // &LJcy;
			case 4345: return "\u0459"; // &ljcy;
			case 4347: return "\u22D8"; // &Ll;
			case 4349: return "\u226A"; // &ll;
			case 4353: return "\u21C7"; // &llarr;
			case 4360: return "\u231E"; // &llcorner;
			case 4369: return "\u21DA"; // &Lleftarrow;
			case 4374: return "\u296B"; // &llhard;
			case 4378: return "\u25FA"; // &lltri;
			case 4384: return "\u013F"; // &Lmidot;
			case 4390: return "\u0140"; // &lmidot;
			case 4395: return "\u23B0"; // &lmoust;
			case 4400: return "\u23B0"; // &lmoustache;
			case 4404: return "\u2A89"; // &lnap;
			case 4409: return "\u2A89"; // &lnapprox;
			case 4411: return "\u2268"; // &lnE;
			case 4413: return "\u2A87"; // &lne;
			case 4415: return "\u2A87"; // &lneq;
			case 4417: return "\u2268"; // &lneqq;
			case 4421: return "\u22E6"; // &lnsim;
			case 4426: return "\u27EC"; // &loang;
			case 4429: return "\u21FD"; // &loarr;
			case 4433: return "\u27E6"; // &lobrk;
			case 4446: return "\u27F5"; // &LongLeftArrow;
			case 4456: return "\u27F8"; // &Longleftarrow;
			case 4468: return "\u27F5"; // &longleftarrow;
			case 4479: return "\u27F7"; // &LongLeftRightArrow;
			case 4490: return "\u27FA"; // &Longleftrightarrow;
			case 4501: return "\u27F7"; // &longleftrightarrow;
			case 4508: return "\u27FC"; // &longmapsto;
			case 4519: return "\u27F6"; // &LongRightArrow;
			case 4530: return "\u27F9"; // &Longrightarrow;
			case 4541: return "\u27F6"; // &longrightarrow;
			case 4553: return "\u21AB"; // &looparrowleft;
			case 4559: return "\u21AC"; // &looparrowright;
			case 4563: return "\u2985"; // &lopar;
			case 4566: return "\uD835\uDD43"; // &Lopf;
			case 4568: return "\uD835\uDD5D"; // &lopf;
			case 4572: return "\u2A2D"; // &loplus;
			case 4578: return "\u2A34"; // &lotimes;
			case 4583: return "\u2217"; // &lowast;
			case 4587: return "\u005F"; // &lowbar;
			case 4600: return "\u2199"; // &LowerLeftArrow;
			case 4611: return "\u2198"; // &LowerRightArrow;
			case 4613: return "\u25CA"; // &loz;
			case 4618: return "\u25CA"; // &lozenge;
			case 4620: return "\u29EB"; // &lozf;
			case 4624: return "\u0028"; // &lpar;
			case 4627: return "\u2993"; // &lparlt;
			case 4632: return "\u21C6"; // &lrarr;
			case 4639: return "\u231F"; // &lrcorner;
			case 4643: return "\u21CB"; // &lrhar;
			case 4645: return "\u296D"; // &lrhard;
			case 4647: return "\u200E"; // &lrm;
			case 4651: return "\u22BF"; // &lrtri;
			case 4657: return "\u2039"; // &lsaquo;
			case 4661: return "\u2112"; // &Lscr;
			case 4664: return "\uD835\uDCC1"; // &lscr;
			case 4666: return "\u21B0"; // &Lsh;
			case 4668: return "\u21B0"; // &lsh;
			case 4671: return "\u2272"; // &lsim;
			case 4673: return "\u2A8D"; // &lsime;
			case 4675: return "\u2A8F"; // &lsimg;
			case 4678: return "\u005B"; // &lsqb;
			case 4681: return "\u2018"; // &lsquo;
			case 4683: return "\u201A"; // &lsquor;
			case 4688: return "\u0141"; // &Lstrok;
			case 4693: return "\u0142"; // &lstrok;
			case 4694: return "\u003C"; // &LT
			case 4695: return "\u003C"; // &LT;
			case 4697: return "\u226A"; // &Lt;
			case 4698: return "\u003C"; // &lt
			case 4699: return "\u003C"; // &lt;
			case 4702: return "\u2AA6"; // &ltcc;
			case 4705: return "\u2A79"; // &ltcir;
			case 4709: return "\u22D6"; // &ltdot;
			case 4714: return "\u22CB"; // &lthree;
			case 4719: return "\u22C9"; // &ltimes;
			case 4724: return "\u2976"; // &ltlarr;
			case 4730: return "\u2A7B"; // &ltquest;
			case 4733: return "\u25C3"; // &ltri;
			case 4735: return "\u22B4"; // &ltrie;
			case 4737: return "\u25C2"; // &ltrif;
			case 4741: return "\u2996"; // &ltrPar;
			case 4749: return "\u294A"; // &lurdshar;
			case 4754: return "\u2966"; // &luruhar;
			case 4763: return "\u2268\uFE00"; // &lvertneqq;
			case 4766: return "\u2268\uFE00"; // &lvnE;
			case 4770: return "\u00AF"; // &macr
			case 4771: return "\u00AF"; // &macr;
			case 4774: return "\u2642"; // &male;
			case 4776: return "\u2720"; // &malt;
			case 4780: return "\u2720"; // &maltese;
			case 4784: return "\u2905"; // &Map;
			case 4786: return "\u21A6"; // &map;
			case 4790: return "\u21A6"; // &mapsto;
			case 4795: return "\u21A7"; // &mapstodown;
			case 4800: return "\u21A4"; // &mapstoleft;
			case 4803: return "\u21A5"; // &mapstoup;
			case 4808: return "\u25AE"; // &marker;
			case 4814: return "\u2A29"; // &mcomma;
			case 4817: return "\u041C"; // &Mcy;
			case 4819: return "\u043C"; // &mcy;
			case 4824: return "\u2014"; // &mdash;
			case 4829: return "\u223A"; // &mDDot;
			case 4842: return "\u2221"; // &measuredangle;
			case 4853: return "\u205F"; // &MediumSpace;
			case 4861: return "\u2133"; // &Mellintrf;
			case 4864: return "\uD835\uDD10"; // &Mfr;
			case 4867: return "\uD835\uDD2A"; // &mfr;
			case 4870: return "\u2127"; // &mho;
			case 4874: return "\u00B5"; // &micro
			case 4875: return "\u00B5"; // &micro;
			case 4877: return "\u2223"; // &mid;
			case 4881: return "\u002A"; // &midast;
			case 4885: return "\u2AF0"; // &midcir;
			case 4888: return "\u00B7"; // &middot
			case 4889: return "\u00B7"; // &middot;
			case 4893: return "\u2212"; // &minus;
			case 4895: return "\u229F"; // &minusb;
			case 4897: return "\u2238"; // &minusd;
			case 4899: return "\u2A2A"; // &minusdu;
			case 4908: return "\u2213"; // &MinusPlus;
			case 4912: return "\u2ADB"; // &mlcp;
			case 4915: return "\u2026"; // &mldr;
			case 4921: return "\u2213"; // &mnplus;
			case 4927: return "\u22A7"; // &models;
			case 4931: return "\uD835\uDD44"; // &Mopf;
			case 4934: return "\uD835\uDD5E"; // &mopf;
			case 4936: return "\u2213"; // &mp;
			case 4940: return "\u2133"; // &Mscr;
			case 4944: return "\uD835\uDCC2"; // &mscr;
			case 4949: return "\u223E"; // &mstpos;
			case 4951: return "\u039C"; // &Mu;
			case 4953: return "\u03BC"; // &mu;
			case 4960: return "\u22B8"; // &multimap;
			case 4964: return "\u22B8"; // &mumap;
			case 4970: return "\u2207"; // &nabla;
			case 4977: return "\u0143"; // &Nacute;
			case 4982: return "\u0144"; // &nacute;
			case 4985: return "\u2220\u20D2"; // &nang;
			case 4987: return "\u2249"; // &nap;
			case 4989: return "\u2A70\u0338"; // &napE;
			case 4992: return "\u224B\u0338"; // &napid;
			case 4995: return "\u0149"; // &napos;
			case 5000: return "\u2249"; // &napprox;
			case 5004: return "\u266E"; // &natur;
			case 5007: return "\u266E"; // &natural;
			case 5009: return "\u2115"; // &naturals;
			case 5012: return "\u00A0"; // &nbsp
			case 5013: return "\u00A0"; // &nbsp;
			case 5017: return "\u224E\u0338"; // &nbump;
			case 5019: return "\u224F\u0338"; // &nbumpe;
			case 5023: return "\u2A43"; // &ncap;
			case 5029: return "\u0147"; // &Ncaron;
			case 5033: return "\u0148"; // &ncaron;
			case 5038: return "\u0145"; // &Ncedil;
			case 5043: return "\u0146"; // &ncedil;
			case 5047: return "\u2247"; // &ncong;
			case 5051: return "\u2A6D\u0338"; // &ncongdot;
			case 5054: return "\u2A42"; // &ncup;
			case 5056: return "\u041D"; // &Ncy;
			case 5058: return "\u043D"; // &ncy;
			case 5063: return "\u2013"; // &ndash;
			case 5065: return "\u2260"; // &ne;
			case 5070: return "\u2924"; // &nearhk;
			case 5074: return "\u21D7"; // &neArr;
			case 5076: return "\u2197"; // &nearr;
			case 5079: return "\u2197"; // &nearrow;
			case 5083: return "\u2250\u0338"; // &nedot;
			case 5102: return "\u200B"; // &NegativeMediumSpace;
			case 5113: return "\u200B"; // &NegativeThickSpace;
			case 5120: return "\u200B"; // &NegativeThinSpace;
			case 5134: return "\u200B"; // &NegativeVeryThinSpace;
			case 5139: return "\u2262"; // &nequiv;
			case 5144: return "\u2928"; // &nesear;
			case 5147: return "\u2242\u0338"; // &nesim;
			case 5166: return "\u226B"; // &NestedGreaterGreater;
			case 5175: return "\u226A"; // &NestedLessLess;
			case 5181: return "\u000A"; // &NewLine;
			case 5186: return "\u2204"; // &nexist;
			case 5188: return "\u2204"; // &nexists;
			case 5191: return "\uD835\uDD11"; // &Nfr;
			case 5194: return "\uD835\uDD2B"; // &nfr;
			case 5197: return "\u2267\u0338"; // &ngE;
			case 5199: return "\u2271"; // &nge;
			case 5201: return "\u2271"; // &ngeq;
			case 5203: return "\u2267\u0338"; // &ngeqq;
			case 5209: return "\u2A7E\u0338"; // &ngeqslant;
			case 5211: return "\u2A7E\u0338"; // &nges;
			case 5214: return "\u22D9\u0338"; // &nGg;
			case 5218: return "\u2275"; // &ngsim;
			case 5220: return "\u226B\u20D2"; // &nGt;
			case 5222: return "\u226F"; // &ngt;
			case 5224: return "\u226F"; // &ngtr;
			case 5226: return "\u226B\u0338"; // &nGtv;
			case 5231: return "\u21CE"; // &nhArr;
			case 5235: return "\u21AE"; // &nharr;
			case 5239: return "\u2AF2"; // &nhpar;
			case 5241: return "\u220B"; // &ni;
			case 5243: return "\u22FC"; // &nis;
			case 5245: return "\u22FA"; // &nisd;
			case 5247: return "\u220B"; // &niv;
			case 5251: return "\u040A"; // &NJcy;
			case 5255: return "\u045A"; // &njcy;
			case 5260: return "\u21CD"; // &nlArr;
			case 5264: return "\u219A"; // &nlarr;
			case 5267: return "\u2025"; // &nldr;
			case 5269: return "\u2266\u0338"; // &nlE;
			case 5271: return "\u2270"; // &nle;
			case 5281: return "\u21CD"; // &nLeftarrow;
			case 5289: return "\u219A"; // &nleftarrow;
			case 5300: return "\u21CE"; // &nLeftrightarrow;
			case 5311: return "\u21AE"; // &nleftrightarrow;
			case 5313: return "\u2270"; // &nleq;
			case 5315: return "\u2266\u0338"; // &nleqq;
			case 5321: return "\u2A7D\u0338"; // &nleqslant;
			case 5323: return "\u2A7D\u0338"; // &nles;
			case 5325: return "\u226E"; // &nless;
			case 5327: return "\u22D8\u0338"; // &nLl;
			case 5331: return "\u2274"; // &nlsim;
			case 5333: return "\u226A\u20D2"; // &nLt;
			case 5335: return "\u226E"; // &nlt;
			case 5338: return "\u22EA"; // &nltri;
			case 5340: return "\u22EC"; // &nltrie;
			case 5342: return "\u226A\u0338"; // &nLtv;
			case 5346: return "\u2224"; // &nmid;
			case 5353: return "\u2060"; // &NoBreak;
			case 5368: return "\u00A0"; // &NonBreakingSpace;
			case 5371: return "\u2115"; // &Nopf;
			case 5375: return "\uD835\uDD5F"; // &nopf;
			case 5377: return "\u2AEC"; // &Not;
			case 5378: return "\u00AC"; // &not
			case 5379: return "\u00AC"; // &not;
			case 5389: return "\u2262"; // &NotCongruent;
			case 5395: return "\u226D"; // &NotCupCap;
			case 5413: return "\u2226"; // &NotDoubleVerticalBar;
			case 5421: return "\u2209"; // &NotElement;
			case 5426: return "\u2260"; // &NotEqual;
			case 5432: return "\u2242\u0338"; // &NotEqualTilde;
			case 5438: return "\u2204"; // &NotExists;
			case 5446: return "\u226F"; // &NotGreater;
			case 5452: return "\u2271"; // &NotGreaterEqual;
			case 5462: return "\u2267\u0338"; // &NotGreaterFullEqual;
			case 5470: return "\u226B\u0338"; // &NotGreaterGreater;
			case 5475: return "\u2279"; // &NotGreaterLess;
			case 5486: return "\u2A7E\u0338"; // &NotGreaterSlantEqual;
			case 5492: return "\u2275"; // &NotGreaterTilde;
			case 5505: return "\u224E\u0338"; // &NotHumpDownHump;
			case 5511: return "\u224F\u0338"; // &NotHumpEqual;
			case 5514: return "\u2209"; // &notin;
			case 5518: return "\u22F5\u0338"; // &notindot;
			case 5520: return "\u22F9\u0338"; // &notinE;
			case 5523: return "\u2209"; // &notinva;
			case 5525: return "\u22F7"; // &notinvb;
			case 5527: return "\u22F6"; // &notinvc;
			case 5540: return "\u22EA"; // &NotLeftTriangle;
			case 5544: return "\u29CF\u0338"; // &NotLeftTriangleBar;
			case 5550: return "\u22EC"; // &NotLeftTriangleEqual;
			case 5553: return "\u226E"; // &NotLess;
			case 5559: return "\u2270"; // &NotLessEqual;
			case 5567: return "\u2278"; // &NotLessGreater;
			case 5572: return "\u226A\u0338"; // &NotLessLess;
			case 5583: return "\u2A7D\u0338"; // &NotLessSlantEqual;
			case 5589: return "\u2274"; // &NotLessTilde;
			case 5610: return "\u2AA2\u0338"; // &NotNestedGreaterGreater;
			case 5619: return "\u2AA1\u0338"; // &NotNestedLessLess;
			case 5622: return "\u220C"; // &notni;
			case 5625: return "\u220C"; // &notniva;
			case 5627: return "\u22FE"; // &notnivb;
			case 5629: return "\u22FD"; // &notnivc;
			case 5638: return "\u2280"; // &NotPrecedes;
			case 5644: return "\u2AAF\u0338"; // &NotPrecedesEqual;
			case 5655: return "\u22E0"; // &NotPrecedesSlantEqual;
			case 5670: return "\u220C"; // &NotReverseElement;
			case 5683: return "\u22EB"; // &NotRightTriangle;
			case 5687: return "\u29D0\u0338"; // &NotRightTriangleBar;
			case 5693: return "\u22ED"; // &NotRightTriangleEqual;
			case 5706: return "\u228F\u0338"; // &NotSquareSubset;
			case 5712: return "\u22E2"; // &NotSquareSubsetEqual;
			case 5719: return "\u2290\u0338"; // &NotSquareSuperset;
			case 5725: return "\u22E3"; // &NotSquareSupersetEqual;
			case 5731: return "\u2282\u20D2"; // &NotSubset;
			case 5737: return "\u2288"; // &NotSubsetEqual;
			case 5744: return "\u2281"; // &NotSucceeds;
			case 5750: return "\u2AB0\u0338"; // &NotSucceedsEqual;
			case 5761: return "\u22E1"; // &NotSucceedsSlantEqual;
			case 5767: return "\u227F\u0338"; // &NotSucceedsTilde;
			case 5774: return "\u2283\u20D2"; // &NotSuperset;
			case 5780: return "\u2289"; // &NotSupersetEqual;
			case 5786: return "\u2241"; // &NotTilde;
			case 5792: return "\u2244"; // &NotTildeEqual;
			case 5802: return "\u2247"; // &NotTildeFullEqual;
			case 5808: return "\u2249"; // &NotTildeTilde;
			case 5820: return "\u2224"; // &NotVerticalBar;
			case 5824: return "\u2226"; // &npar;
			case 5830: return "\u2226"; // &nparallel;
			case 5833: return "\u2AFD\u20E5"; // &nparsl;
			case 5835: return "\u2202\u0338"; // &npart;
			case 5841: return "\u2A14"; // &npolint;
			case 5843: return "\u2280"; // &npr;
			case 5847: return "\u22E0"; // &nprcue;
			case 5849: return "\u2AAF\u0338"; // &npre;
			case 5851: return "\u2280"; // &nprec;
			case 5854: return "\u2AAF\u0338"; // &npreceq;
			case 5859: return "\u21CF"; // &nrArr;
			case 5863: return "\u219B"; // &nrarr;
			case 5865: return "\u2933\u0338"; // &nrarrc;
			case 5867: return "\u219D\u0338"; // &nrarrw;
			case 5878: return "\u21CF"; // &nRightarrow;
			case 5888: return "\u219B"; // &nrightarrow;
			case 5892: return "\u22EB"; // &nrtri;
			case 5894: return "\u22ED"; // &nrtrie;
			case 5897: return "\u2281"; // &nsc;
			case 5901: return "\u22E1"; // &nsccue;
			case 5903: return "\u2AB0\u0338"; // &nsce;
			case 5907: return "\uD835\uDCA9"; // &Nscr;
			case 5909: return "\uD835\uDCC3"; // &nscr;
			case 5917: return "\u2224"; // &nshortmid;
			case 5926: return "\u2226"; // &nshortparallel;
			case 5929: return "\u2241"; // &nsim;
			case 5931: return "\u2244"; // &nsime;
			case 5933: return "\u2244"; // &nsimeq;
			case 5937: return "\u2224"; // &nsmid;
			case 5941: return "\u2226"; // &nspar;
			case 5947: return "\u22E2"; // &nsqsube;
			case 5950: return "\u22E3"; // &nsqsupe;
			case 5953: return "\u2284"; // &nsub;
			case 5955: return "\u2AC5\u0338"; // &nsubE;
			case 5957: return "\u2288"; // &nsube;
			case 5961: return "\u2282\u20D2"; // &nsubset;
			case 5964: return "\u2288"; // &nsubseteq;
			case 5966: return "\u2AC5\u0338"; // &nsubseteqq;
			case 5969: return "\u2281"; // &nsucc;
			case 5972: return "\u2AB0\u0338"; // &nsucceq;
			case 5974: return "\u2285"; // &nsup;
			case 5976: return "\u2AC6\u0338"; // &nsupE;
			case 5978: return "\u2289"; // &nsupe;
			case 5982: return "\u2283\u20D2"; // &nsupset;
			case 5985: return "\u2289"; // &nsupseteq;
			case 5987: return "\u2AC6\u0338"; // &nsupseteqq;
			case 5991: return "\u2279"; // &ntgl;
			case 5996: return "\u00D1"; // &Ntilde
			case 5997: return "\u00D1"; // &Ntilde;
			case 6001: return "\u00F1"; // &ntilde
			case 6002: return "\u00F1"; // &ntilde;
			case 6005: return "\u2278"; // &ntlg;
			case 6017: return "\u22EA"; // &ntriangleleft;
			case 6020: return "\u22EC"; // &ntrianglelefteq;
			case 6026: return "\u22EB"; // &ntriangleright;
			case 6029: return "\u22ED"; // &ntrianglerighteq;
			case 6031: return "\u039D"; // &Nu;
			case 6033: return "\u03BD"; // &nu;
			case 6035: return "\u0023"; // &num;
			case 6039: return "\u2116"; // &numero;
			case 6042: return "\u2007"; // &numsp;
			case 6046: return "\u224D\u20D2"; // &nvap;
			case 6052: return "\u22AF"; // &nVDash;
			case 6057: return "\u22AE"; // &nVdash;
			case 6062: return "\u22AD"; // &nvDash;
			case 6067: return "\u22AC"; // &nvdash;
			case 6070: return "\u2265\u20D2"; // &nvge;
			case 6072: return "\u003E\u20D2"; // &nvgt;
			case 6077: return "\u2904"; // &nvHarr;
			case 6083: return "\u29DE"; // &nvinfin;
			case 6088: return "\u2902"; // &nvlArr;
			case 6090: return "\u2264\u20D2"; // &nvle;
			case 6092: return "\u003C\u20D2"; // &nvlt;
			case 6096: return "\u22B4\u20D2"; // &nvltrie;
			case 6101: return "\u2903"; // &nvrArr;
			case 6106: return "\u22B5\u20D2"; // &nvrtrie;
			case 6110: return "\u223C\u20D2"; // &nvsim;
			case 6116: return "\u2923"; // &nwarhk;
			case 6120: return "\u21D6"; // &nwArr;
			case 6122: return "\u2196"; // &nwarr;
			case 6125: return "\u2196"; // &nwarrow;
			case 6130: return "\u2927"; // &nwnear;
			case 6136: return "\u00D3"; // &Oacute
			case 6137: return "\u00D3"; // &Oacute;
			case 6143: return "\u00F3"; // &oacute
			case 6144: return "\u00F3"; // &oacute;
			case 6147: return "\u229B"; // &oast;
			case 6151: return "\u229A"; // &ocir;
			case 6155: return "\u00D4"; // &Ocirc
			case 6156: return "\u00D4"; // &Ocirc;
			case 6157: return "\u00F4"; // &ocirc
			case 6158: return "\u00F4"; // &ocirc;
			case 6160: return "\u041E"; // &Ocy;
			case 6162: return "\u043E"; // &ocy;
			case 6167: return "\u229D"; // &odash;
			case 6173: return "\u0150"; // &Odblac;
			case 6178: return "\u0151"; // &odblac;
			case 6181: return "\u2A38"; // &odiv;
			case 6184: return "\u2299"; // &odot;
			case 6189: return "\u29BC"; // &odsold;
			case 6194: return "\u0152"; // &OElig;
			case 6199: return "\u0153"; // &oelig;
			case 6204: return "\u29BF"; // &ofcir;
			case 6207: return "\uD835\uDD12"; // &Ofr;
			case 6209: return "\uD835\uDD2C"; // &ofr;
			case 6213: return "\u02DB"; // &ogon;
			case 6218: return "\u00D2"; // &Ograve
			case 6219: return "\u00D2"; // &Ograve;
			case 6223: return "\u00F2"; // &ograve
			case 6224: return "\u00F2"; // &ograve;
			case 6226: return "\u29C1"; // &ogt;
			case 6231: return "\u29B5"; // &ohbar;
			case 6233: return "\u03A9"; // &ohm;
			case 6237: return "\u222E"; // &oint;
			case 6242: return "\u21BA"; // &olarr;
			case 6246: return "\u29BE"; // &olcir;
			case 6251: return "\u29BB"; // &olcross;
			case 6255: return "\u203E"; // &oline;
			case 6257: return "\u29C0"; // &olt;
			case 6262: return "\u014C"; // &Omacr;
			case 6267: return "\u014D"; // &omacr;
			case 6271: return "\u03A9"; // &Omega;
			case 6275: return "\u03C9"; // &omega;
			case 6281: return "\u039F"; // &Omicron;
			case 6287: return "\u03BF"; // &omicron;
			case 6289: return "\u29B6"; // &omid;
			case 6293: return "\u2296"; // &ominus;
			case 6297: return "\uD835\uDD46"; // &Oopf;
			case 6301: return "\uD835\uDD60"; // &oopf;
			case 6305: return "\u29B7"; // &opar;
			case 6325: return "\u201C"; // &OpenCurlyDoubleQuote;
			case 6331: return "\u2018"; // &OpenCurlyQuote;
			case 6335: return "\u29B9"; // &operp;
			case 6339: return "\u2295"; // &oplus;
			case 6341: return "\u2A54"; // &Or;
			case 6343: return "\u2228"; // &or;
			case 6347: return "\u21BB"; // &orarr;
			case 6349: return "\u2A5D"; // &ord;
			case 6352: return "\u2134"; // &order;
			case 6355: return "\u2134"; // &orderof;
			case 6356: return "\u00AA"; // &ordf
			case 6357: return "\u00AA"; // &ordf;
			case 6358: return "\u00BA"; // &ordm
			case 6359: return "\u00BA"; // &ordm;
			case 6364: return "\u22B6"; // &origof;
			case 6367: return "\u2A56"; // &oror;
			case 6373: return "\u2A57"; // &orslope;
			case 6375: return "\u2A5B"; // &orv;
			case 6377: return "\u24C8"; // &oS;
			case 6381: return "\uD835\uDCAA"; // &Oscr;
			case 6385: return "\u2134"; // &oscr;
			case 6389: return "\u00D8"; // &Oslash
			case 6390: return "\u00D8"; // &Oslash;
			case 6394: return "\u00F8"; // &oslash
			case 6395: return "\u00F8"; // &oslash;
			case 6398: return "\u2298"; // &osol;
			case 6403: return "\u00D5"; // &Otilde
			case 6404: return "\u00D5"; // &Otilde;
			case 6409: return "\u00F5"; // &otilde
			case 6410: return "\u00F5"; // &otilde;
			case 6414: return "\u2A37"; // &Otimes;
			case 6418: return "\u2297"; // &otimes;
			case 6421: return "\u2A36"; // &otimesas;
			case 6424: return "\u00D6"; // &Ouml
			case 6425: return "\u00D6"; // &Ouml;
			case 6428: return "\u00F6"; // &ouml
			case 6429: return "\u00F6"; // &ouml;
			case 6434: return "\u233D"; // &ovbar;
			case 6441: return "\u203E"; // &OverBar;
			case 6446: return "\u23DE"; // &OverBrace;
			case 6450: return "\u23B4"; // &OverBracket;
			case 6462: return "\u23DC"; // &OverParenthesis;
			case 6466: return "\u2225"; // &par;
			case 6467: return "\u00B6"; // &para
			case 6468: return "\u00B6"; // &para;
			case 6473: return "\u2225"; // &parallel;
			case 6477: return "\u2AF3"; // &parsim;
			case 6479: return "\u2AFD"; // &parsl;
			case 6481: return "\u2202"; // &part;
			case 6490: return "\u2202"; // &PartialD;
			case 6493: return "\u041F"; // &Pcy;
			case 6496: return "\u043F"; // &pcy;
			case 6502: return "\u0025"; // &percnt;
			case 6506: return "\u002E"; // &period;
			case 6510: return "\u2030"; // &permil;
			case 6512: return "\u22A5"; // &perp;
			case 6517: return "\u2031"; // &pertenk;
			case 6520: return "\uD835\uDD13"; // &Pfr;
			case 6523: return "\uD835\uDD2D"; // &pfr;
			case 6526: return "\u03A6"; // &Phi;
			case 6529: return "\u03C6"; // &phi;
			case 6531: return "\u03D5"; // &phiv;
			case 6536: return "\u2133"; // &phmmat;
			case 6540: return "\u260E"; // &phone;
			case 6542: return "\u03A0"; // &Pi;
			case 6544: return "\u03C0"; // &pi;
			case 6552: return "\u22D4"; // &pitchfork;
			case 6554: return "\u03D6"; // &piv;
			case 6560: return "\u210F"; // &planck;
			case 6562: return "\u210E"; // &planckh;
			case 6565: return "\u210F"; // &plankv;
			case 6568: return "\u002B"; // &plus;
			case 6573: return "\u2A23"; // &plusacir;
			case 6575: return "\u229E"; // &plusb;
			case 6579: return "\u2A22"; // &pluscir;
			case 6582: return "\u2214"; // &plusdo;
			case 6584: return "\u2A25"; // &plusdu;
			case 6586: return "\u2A72"; // &pluse;
			case 6595: return "\u00B1"; // &PlusMinus;
			case 6597: return "\u00B1"; // &plusmn
			case 6598: return "\u00B1"; // &plusmn;
			case 6602: return "\u2A26"; // &plussim;
			case 6606: return "\u2A27"; // &plustwo;
			case 6608: return "\u00B1"; // &pm;
			case 6621: return "\u210C"; // &Poincareplane;
			case 6629: return "\u2A15"; // &pointint;
			case 6632: return "\u2119"; // &Popf;
			case 6635: return "\uD835\uDD61"; // &popf;
			case 6638: return "\u00A3"; // &pound
			case 6639: return "\u00A3"; // &pound;
			case 6641: return "\u2ABB"; // &Pr;
			case 6643: return "\u227A"; // &pr;
			case 6646: return "\u2AB7"; // &prap;
			case 6650: return "\u227C"; // &prcue;
			case 6652: return "\u2AB3"; // &prE;
			case 6654: return "\u2AAF"; // &pre;
			case 6656: return "\u227A"; // &prec;
			case 6663: return "\u2AB7"; // &precapprox;
			case 6671: return "\u227C"; // &preccurlyeq;
			case 6678: return "\u227A"; // &Precedes;
			case 6684: return "\u2AAF"; // &PrecedesEqual;
			case 6695: return "\u227C"; // &PrecedesSlantEqual;
			case 6701: return "\u227E"; // &PrecedesTilde;
			case 6704: return "\u2AAF"; // &preceq;
			case 6712: return "\u2AB9"; // &precnapprox;
			case 6716: return "\u2AB5"; // &precneqq;
			case 6720: return "\u22E8"; // &precnsim;
			case 6724: return "\u227E"; // &precsim;
			case 6728: return "\u2033"; // &Prime;
			case 6732: return "\u2032"; // &prime;
			case 6734: return "\u2119"; // &primes;
			case 6738: return "\u2AB9"; // &prnap;
			case 6740: return "\u2AB5"; // &prnE;
			case 6744: return "\u22E8"; // &prnsim;
			case 6747: return "\u220F"; // &prod;
			case 6753: return "\u220F"; // &Product;
			case 6759: return "\u232E"; // &profalar;
			case 6764: return "\u2312"; // &profline;
			case 6769: return "\u2313"; // &profsurf;
			case 6771: return "\u221D"; // &prop;
			case 6779: return "\u2237"; // &Proportion;
			case 6782: return "\u221D"; // &Proportional;
			case 6785: return "\u221D"; // &propto;
			case 6789: return "\u227E"; // &prsim;
			case 6794: return "\u22B0"; // &prurel;
			case 6798: return "\uD835\uDCAB"; // &Pscr;
			case 6802: return "\uD835\uDCC5"; // &pscr;
			case 6804: return "\u03A8"; // &Psi;
			case 6806: return "\u03C8"; // &psi;
			case 6812: return "\u2008"; // &puncsp;
			case 6816: return "\uD835\uDD14"; // &Qfr;
			case 6820: return "\uD835\uDD2E"; // &qfr;
			case 6824: return "\u2A0C"; // &qint;
			case 6828: return "\u211A"; // &Qopf;
			case 6832: return "\uD835\uDD62"; // &qopf;
			case 6838: return "\u2057"; // &qprime;
			case 6842: return "\uD835\uDCAC"; // &Qscr;
			case 6846: return "\uD835\uDCC6"; // &qscr;
			case 6857: return "\u210D"; // &quaternions;
			case 6861: return "\u2A16"; // &quatint;
			case 6865: return "\u003F"; // &quest;
			case 6868: return "\u225F"; // &questeq;
			case 6871: return "\u0022"; // &QUOT
			case 6872: return "\u0022"; // &QUOT;
			case 6874: return "\u0022"; // &quot
			case 6875: return "\u0022"; // &quot;
			case 6881: return "\u21DB"; // &rAarr;
			case 6885: return "\u223D\u0331"; // &race;
			case 6892: return "\u0154"; // &Racute;
			case 6896: return "\u0155"; // &racute;
			case 6900: return "\u221A"; // &radic;
			case 6907: return "\u29B3"; // &raemptyv;
			case 6910: return "\u27EB"; // &Rang;
			case 6913: return "\u27E9"; // &rang;
			case 6915: return "\u2992"; // &rangd;
			case 6917: return "\u29A5"; // &range;
			case 6920: return "\u27E9"; // &rangle;
			case 6923: return "\u00BB"; // &raquo
			case 6924: return "\u00BB"; // &raquo;
			case 6927: return "\u21A0"; // &Rarr;
			case 6930: return "\u21D2"; // &rArr;
			case 6933: return "\u2192"; // &rarr;
			case 6936: return "\u2975"; // &rarrap;
			case 6938: return "\u21E5"; // &rarrb;
			case 6941: return "\u2920"; // &rarrbfs;
			case 6943: return "\u2933"; // &rarrc;
			case 6946: return "\u291E"; // &rarrfs;
			case 6949: return "\u21AA"; // &rarrhk;
			case 6952: return "\u21AC"; // &rarrlp;
			case 6955: return "\u2945"; // &rarrpl;
			case 6959: return "\u2974"; // &rarrsim;
			case 6962: return "\u2916"; // &Rarrtl;
			case 6965: return "\u21A3"; // &rarrtl;
			case 6967: return "\u219D"; // &rarrw;
			case 6972: return "\u291C"; // &rAtail;
			case 6977: return "\u291A"; // &ratail;
			case 6980: return "\u2236"; // &ratio;
			case 6985: return "\u211A"; // &rationals;
			case 6990: return "\u2910"; // &RBarr;
			case 6995: return "\u290F"; // &rBarr;
			case 7000: return "\u290D"; // &rbarr;
			case 7004: return "\u2773"; // &rbbrk;
			case 7009: return "\u007D"; // &rbrace;
			case 7011: return "\u005D"; // &rbrack;
			case 7014: return "\u298C"; // &rbrke;
			case 7018: return "\u298E"; // &rbrksld;
			case 7020: return "\u2990"; // &rbrkslu;
			case 7026: return "\u0158"; // &Rcaron;
			case 7032: return "\u0159"; // &rcaron;
			case 7037: return "\u0156"; // &Rcedil;
			case 7042: return "\u0157"; // &rcedil;
			case 7045: return "\u2309"; // &rceil;
			case 7048: return "\u007D"; // &rcub;
			case 7050: return "\u0420"; // &Rcy;
			case 7052: return "\u0440"; // &rcy;
			case 7056: return "\u2937"; // &rdca;
			case 7062: return "\u2969"; // &rdldhar;
			case 7066: return "\u201D"; // &rdquo;
			case 7068: return "\u201D"; // &rdquor;
			case 7071: return "\u21B3"; // &rdsh;
			case 7073: return "\u211C"; // &Re;
			case 7077: return "\u211C"; // &real;
			case 7081: return "\u211B"; // &realine;
			case 7086: return "\u211C"; // &realpart;
			case 7088: return "\u211D"; // &reals;
			case 7091: return "\u25AD"; // &rect;
			case 7093: return "\u00AE"; // &REG
			case 7094: return "\u00AE"; // &REG;
			case 7095: return "\u00AE"; // &reg
			case 7096: return "\u00AE"; // &reg;
			case 7109: return "\u220B"; // &ReverseElement;
			case 7120: return "\u21CB"; // &ReverseEquilibrium;
			case 7134: return "\u296F"; // &ReverseUpEquilibrium;
			case 7140: return "\u297D"; // &rfisht;
			case 7145: return "\u230B"; // &rfloor;
			case 7148: return "\u211C"; // &Rfr;
			case 7150: return "\uD835\uDD2F"; // &rfr;
			case 7154: return "\u2964"; // &rHar;
			case 7159: return "\u21C1"; // &rhard;
			case 7161: return "\u21C0"; // &rharu;
			case 7163: return "\u296C"; // &rharul;
			case 7166: return "\u03A1"; // &Rho;
			case 7168: return "\u03C1"; // &rho;
			case 7170: return "\u03F1"; // &rhov;
			case 7187: return "\u27E9"; // &RightAngleBracket;
			case 7192: return "\u2192"; // &RightArrow;
			case 7198: return "\u21D2"; // &Rightarrow;
			case 7208: return "\u2192"; // &rightarrow;
			case 7212: return "\u21E5"; // &RightArrowBar;
			case 7222: return "\u21C4"; // &RightArrowLeftArrow;
			case 7227: return "\u21A3"; // &rightarrowtail;
			case 7235: return "\u2309"; // &RightCeiling;
			case 7249: return "\u27E7"; // &RightDoubleBracket;
			case 7261: return "\u295D"; // &RightDownTeeVector;
			case 7268: return "\u21C2"; // &RightDownVector;
			case 7272: return "\u2955"; // &RightDownVectorBar;
			case 7278: return "\u230B"; // &RightFloor;
			case 7290: return "\u21C1"; // &rightharpoondown;
			case 7293: return "\u21C0"; // &rightharpoonup;
			case 7304: return "\u21C4"; // &rightleftarrows;
			case 7313: return "\u21CC"; // &rightleftharpoons;
			case 7325: return "\u21C9"; // &rightrightarrows;
			case 7336: return "\u219D"; // &rightsquigarrow;
			case 7340: return "\u22A2"; // &RightTee;
			case 7346: return "\u21A6"; // &RightTeeArrow;
			case 7353: return "\u295B"; // &RightTeeVector;
			case 7364: return "\u22CC"; // &rightthreetimes;
			case 7372: return "\u22B3"; // &RightTriangle;
			case 7376: return "\u29D0"; // &RightTriangleBar;
			case 7382: return "\u22B5"; // &RightTriangleEqual;
			case 7395: return "\u294F"; // &RightUpDownVector;
			case 7405: return "\u295C"; // &RightUpTeeVector;
			case 7412: return "\u21BE"; // &RightUpVector;
			case 7416: return "\u2954"; // &RightUpVectorBar;
			case 7423: return "\u21C0"; // &RightVector;
			case 7427: return "\u2953"; // &RightVectorBar;
			case 7430: return "\u02DA"; // &ring;
			case 7441: return "\u2253"; // &risingdotseq;
			case 7446: return "\u21C4"; // &rlarr;
			case 7450: return "\u21CC"; // &rlhar;
			case 7452: return "\u200F"; // &rlm;
			case 7458: return "\u23B1"; // &rmoust;
			case 7463: return "\u23B1"; // &rmoustache;
			case 7468: return "\u2AEE"; // &rnmid;
			case 7473: return "\u27ED"; // &roang;
			case 7476: return "\u21FE"; // &roarr;
			case 7480: return "\u27E7"; // &robrk;
			case 7484: return "\u2986"; // &ropar;
			case 7488: return "\u211D"; // &Ropf;
			case 7490: return "\uD835\uDD63"; // &ropf;
			case 7494: return "\u2A2E"; // &roplus;
			case 7500: return "\u2A35"; // &rotimes;
			case 7511: return "\u2970"; // &RoundImplies;
			case 7515: return "\u0029"; // &rpar;
			case 7518: return "\u2994"; // &rpargt;
			case 7525: return "\u2A12"; // &rppolint;
			case 7530: return "\u21C9"; // &rrarr;
			case 7541: return "\u21DB"; // &Rrightarrow;
			case 7547: return "\u203A"; // &rsaquo;
			case 7551: return "\u211B"; // &Rscr;
			case 7554: return "\uD835\uDCC7"; // &rscr;
			case 7556: return "\u21B1"; // &Rsh;
			case 7558: return "\u21B1"; // &rsh;
			case 7561: return "\u005D"; // &rsqb;
			case 7564: return "\u2019"; // &rsquo;
			case 7566: return "\u2019"; // &rsquor;
			case 7572: return "\u22CC"; // &rthree;
			case 7577: return "\u22CA"; // &rtimes;
			case 7580: return "\u25B9"; // &rtri;
			case 7582: return "\u22B5"; // &rtrie;
			case 7584: return "\u25B8"; // &rtrif;
			case 7589: return "\u29CE"; // &rtriltri;
			case 7600: return "\u29F4"; // &RuleDelayed;
			case 7607: return "\u2968"; // &ruluhar;
			case 7609: return "\u211E"; // &rx;
			case 7616: return "\u015A"; // &Sacute;
			case 7623: return "\u015B"; // &sacute;
			case 7628: return "\u201A"; // &sbquo;
			case 7630: return "\u2ABC"; // &Sc;
			case 7632: return "\u227B"; // &sc;
			case 7635: return "\u2AB8"; // &scap;
			case 7640: return "\u0160"; // &Scaron;
			case 7644: return "\u0161"; // &scaron;
			case 7648: return "\u227D"; // &sccue;
			case 7650: return "\u2AB4"; // &scE;
			case 7652: return "\u2AB0"; // &sce;
			case 7657: return "\u015E"; // &Scedil;
			case 7661: return "\u015F"; // &scedil;
			case 7665: return "\u015C"; // &Scirc;
			case 7669: return "\u015D"; // &scirc;
			case 7673: return "\u2ABA"; // &scnap;
			case 7675: return "\u2AB6"; // &scnE;
			case 7679: return "\u22E9"; // &scnsim;
			case 7686: return "\u2A13"; // &scpolint;
			case 7690: return "\u227F"; // &scsim;
			case 7692: return "\u0421"; // &Scy;
			case 7694: return "\u0441"; // &scy;
			case 7698: return "\u22C5"; // &sdot;
			case 7700: return "\u22A1"; // &sdotb;
			case 7702: return "\u2A66"; // &sdote;
			case 7708: return "\u2925"; // &searhk;
			case 7712: return "\u21D8"; // &seArr;
			case 7714: return "\u2198"; // &searr;
			case 7717: return "\u2198"; // &searrow;
			case 7719: return "\u00A7"; // &sect
			case 7720: return "\u00A7"; // &sect;
			case 7723: return "\u003B"; // &semi;
			case 7728: return "\u2929"; // &seswar;
			case 7735: return "\u2216"; // &setminus;
			case 7737: return "\u2216"; // &setmn;
			case 7740: return "\u2736"; // &sext;
			case 7743: return "\uD835\uDD16"; // &Sfr;
			case 7746: return "\uD835\uDD30"; // &sfr;
			case 7750: return "\u2322"; // &sfrown;
			case 7755: return "\u266F"; // &sharp;
			case 7761: return "\u0429"; // &SHCHcy;
			case 7766: return "\u0449"; // &shchcy;
			case 7769: return "\u0428"; // &SHcy;
			case 7771: return "\u0448"; // &shcy;
			case 7785: return "\u2193"; // &ShortDownArrow;
			case 7795: return "\u2190"; // &ShortLeftArrow;
			case 7802: return "\u2223"; // &shortmid;
			case 7811: return "\u2225"; // &shortparallel;
			case 7822: return "\u2192"; // &ShortRightArrow;
			case 7830: return "\u2191"; // &ShortUpArrow;
			case 7831: return "\u00AD"; // &shy
			case 7832: return "\u00AD"; // &shy;
			case 7837: return "\u03A3"; // &Sigma;
			case 7842: return "\u03C3"; // &sigma;
			case 7844: return "\u03C2"; // &sigmaf;
			case 7846: return "\u03C2"; // &sigmav;
			case 7848: return "\u223C"; // &sim;
			case 7852: return "\u2A6A"; // &simdot;
			case 7854: return "\u2243"; // &sime;
			case 7856: return "\u2243"; // &simeq;
			case 7858: return "\u2A9E"; // &simg;
			case 7860: return "\u2AA0"; // &simgE;
			case 7862: return "\u2A9D"; // &siml;
			case 7864: return "\u2A9F"; // &simlE;
			case 7867: return "\u2246"; // &simne;
			case 7872: return "\u2A24"; // &simplus;
			case 7877: return "\u2972"; // &simrarr;
			case 7882: return "\u2190"; // &slarr;
			case 7893: return "\u2218"; // &SmallCircle;
			case 7906: return "\u2216"; // &smallsetminus;
			case 7910: return "\u2A33"; // &smashp;
			case 7917: return "\u29E4"; // &smeparsl;
			case 7920: return "\u2223"; // &smid;
			case 7923: return "\u2323"; // &smile;
			case 7925: return "\u2AAA"; // &smt;
			case 7927: return "\u2AAC"; // &smte;
			case 7929: return "\u2AAC\uFE00"; // &smtes;
			case 7935: return "\u042C"; // &SOFTcy;
			case 7941: return "\u044C"; // &softcy;
			case 7943: return "\u002F"; // &sol;
			case 7945: return "\u29C4"; // &solb;
			case 7948: return "\u233F"; // &solbar;
			case 7952: return "\uD835\uDD4A"; // &Sopf;
			case 7955: return "\uD835\uDD64"; // &sopf;
			case 7961: return "\u2660"; // &spades;
			case 7965: return "\u2660"; // &spadesuit;
			case 7967: return "\u2225"; // &spar;
			case 7972: return "\u2293"; // &sqcap;
			case 7974: return "\u2293\uFE00"; // &sqcaps;
			case 7977: return "\u2294"; // &sqcup;
			case 7979: return "\u2294\uFE00"; // &sqcups;
			case 7983: return "\u221A"; // &Sqrt;
			case 7987: return "\u228F"; // &sqsub;
			case 7989: return "\u2291"; // &sqsube;
			case 7993: return "\u228F"; // &sqsubset;
			case 7996: return "\u2291"; // &sqsubseteq;
			case 7998: return "\u2290"; // &sqsup;
			case 8000: return "\u2292"; // &sqsupe;
			case 8004: return "\u2290"; // &sqsupset;
			case 8007: return "\u2292"; // &sqsupseteq;
			case 8009: return "\u25A1"; // &squ;
			case 8014: return "\u25A1"; // &Square;
			case 8018: return "\u25A1"; // &square;
			case 8031: return "\u2293"; // &SquareIntersection;
			case 8038: return "\u228F"; // &SquareSubset;
			case 8044: return "\u2291"; // &SquareSubsetEqual;
			case 8051: return "\u2290"; // &SquareSuperset;
			case 8057: return "\u2292"; // &SquareSupersetEqual;
			case 8063: return "\u2294"; // &SquareUnion;
			case 8065: return "\u25AA"; // &squarf;
			case 8067: return "\u25AA"; // &squf;
			case 8072: return "\u2192"; // &srarr;
			case 8076: return "\uD835\uDCAE"; // &Sscr;
			case 8080: return "\uD835\uDCC8"; // &sscr;
			case 8085: return "\u2216"; // &ssetmn;
			case 8090: return "\u2323"; // &ssmile;
			case 8095: return "\u22C6"; // &sstarf;
			case 8099: return "\u22C6"; // &Star;
			case 8103: return "\u2606"; // &star;
			case 8105: return "\u2605"; // &starf;
			case 8119: return "\u03F5"; // &straightepsilon;
			case 8123: return "\u03D5"; // &straightphi;
			case 8126: return "\u00AF"; // &strns;
			case 8129: return "\u22D0"; // &Sub;
			case 8132: return "\u2282"; // &sub;
			case 8136: return "\u2ABD"; // &subdot;
			case 8138: return "\u2AC5"; // &subE;
			case 8140: return "\u2286"; // &sube;
			case 8144: return "\u2AC3"; // &subedot;
			case 8149: return "\u2AC1"; // &submult;
			case 8152: return "\u2ACB"; // &subnE;
			case 8154: return "\u228A"; // &subne;
			case 8159: return "\u2ABF"; // &subplus;
			case 8164: return "\u2979"; // &subrarr;
			case 8168: return "\u22D0"; // &Subset;
			case 8172: return "\u2282"; // &subset;
			case 8175: return "\u2286"; // &subseteq;
			case 8177: return "\u2AC5"; // &subseteqq;
			case 8183: return "\u2286"; // &SubsetEqual;
			case 8187: return "\u228A"; // &subsetneq;
			case 8189: return "\u2ACB"; // &subsetneqq;
			case 8192: return "\u2AC7"; // &subsim;
			case 8195: return "\u2AD5"; // &subsub;
			case 8197: return "\u2AD3"; // &subsup;
			case 8200: return "\u227B"; // &succ;
			case 8207: return "\u2AB8"; // &succapprox;
			case 8215: return "\u227D"; // &succcurlyeq;
			case 8222: return "\u227B"; // &Succeeds;
			case 8228: return "\u2AB0"; // &SucceedsEqual;
			case 8239: return "\u227D"; // &SucceedsSlantEqual;
			case 8245: return "\u227F"; // &SucceedsTilde;
			case 8248: return "\u2AB0"; // &succeq;
			case 8256: return "\u2ABA"; // &succnapprox;
			case 8260: return "\u2AB6"; // &succneqq;
			case 8264: return "\u22E9"; // &succnsim;
			case 8268: return "\u227F"; // &succsim;
			case 8274: return "\u220B"; // &SuchThat;
			case 8276: return "\u2211"; // &Sum;
			case 8278: return "\u2211"; // &sum;
			case 8281: return "\u266A"; // &sung;
			case 8283: return "\u22D1"; // &Sup;
			case 8285: return "\u2283"; // &sup;
			case 8286: return "\u00B9"; // &sup1
			case 8287: return "\u00B9"; // &sup1;
			case 8288: return "\u00B2"; // &sup2
			case 8289: return "\u00B2"; // &sup2;
			case 8290: return "\u00B3"; // &sup3
			case 8291: return "\u00B3"; // &sup3;
			case 8295: return "\u2ABE"; // &supdot;
			case 8299: return "\u2AD8"; // &supdsub;
			case 8301: return "\u2AC6"; // &supE;
			case 8303: return "\u2287"; // &supe;
			case 8307: return "\u2AC4"; // &supedot;
			case 8313: return "\u2283"; // &Superset;
			case 8319: return "\u2287"; // &SupersetEqual;
			case 8324: return "\u27C9"; // &suphsol;
			case 8327: return "\u2AD7"; // &suphsub;
			case 8332: return "\u297B"; // &suplarr;
			case 8337: return "\u2AC2"; // &supmult;
			case 8340: return "\u2ACC"; // &supnE;
			case 8342: return "\u228B"; // &supne;
			case 8347: return "\u2AC0"; // &supplus;
			case 8351: return "\u22D1"; // &Supset;
			case 8355: return "\u2283"; // &supset;
			case 8358: return "\u2287"; // &supseteq;
			case 8360: return "\u2AC6"; // &supseteqq;
			case 8364: return "\u228B"; // &supsetneq;
			case 8366: return "\u2ACC"; // &supsetneqq;
			case 8369: return "\u2AC8"; // &supsim;
			case 8372: return "\u2AD4"; // &supsub;
			case 8374: return "\u2AD6"; // &supsup;
			case 8380: return "\u2926"; // &swarhk;
			case 8384: return "\u21D9"; // &swArr;
			case 8386: return "\u2199"; // &swarr;
			case 8389: return "\u2199"; // &swarrow;
			case 8394: return "\u292A"; // &swnwar;
			case 8398: return "\u00DF"; // &szlig
			case 8399: return "\u00DF"; // &szlig;
			case 8403: return "\u0009"; // &Tab;
			case 8410: return "\u2316"; // &target;
			case 8412: return "\u03A4"; // &Tau;
			case 8414: return "\u03C4"; // &tau;
			case 8418: return "\u23B4"; // &tbrk;
			case 8424: return "\u0164"; // &Tcaron;
			case 8430: return "\u0165"; // &tcaron;
			case 8435: return "\u0162"; // &Tcedil;
			case 8440: return "\u0163"; // &tcedil;
			case 8442: return "\u0422"; // &Tcy;
			case 8444: return "\u0442"; // &tcy;
			case 8448: return "\u20DB"; // &tdot;
			case 8454: return "\u2315"; // &telrec;
			case 8457: return "\uD835\uDD17"; // &Tfr;
			case 8460: return "\uD835\uDD31"; // &tfr;
			case 8466: return "\u2234"; // &there4;
			case 8475: return "\u2234"; // &Therefore;
			case 8480: return "\u2234"; // &therefore;
			case 8483: return "\u0398"; // &Theta;
			case 8486: return "\u03B8"; // &theta;
			case 8490: return "\u03D1"; // &thetasym;
			case 8492: return "\u03D1"; // &thetav;
			case 8502: return "\u2248"; // &thickapprox;
			case 8506: return "\u223C"; // &thicksim;
			case 8515: return "\u205F\u200A"; // &ThickSpace;
			case 8519: return "\u2009"; // &thinsp;
			case 8526: return "\u2009"; // &ThinSpace;
			case 8530: return "\u2248"; // &thkap;
			case 8534: return "\u223C"; // &thksim;
			case 8538: return "\u00DE"; // &THORN
			case 8539: return "\u00DE"; // &THORN;
			case 8542: return "\u00FE"; // &thorn
			case 8543: return "\u00FE"; // &thorn;
			case 8548: return "\u223C"; // &Tilde;
			case 8553: return "\u02DC"; // &tilde;
			case 8559: return "\u2243"; // &TildeEqual;
			case 8569: return "\u2245"; // &TildeFullEqual;
			case 8575: return "\u2248"; // &TildeTilde;
			case 8578: return "\u00D7"; // &times
			case 8579: return "\u00D7"; // &times;
			case 8581: return "\u22A0"; // &timesb;
			case 8584: return "\u2A31"; // &timesbar;
			case 8586: return "\u2A30"; // &timesd;
			case 8589: return "\u222D"; // &tint;
			case 8593: return "\u2928"; // &toea;
			case 8595: return "\u22A4"; // &top;
			case 8599: return "\u2336"; // &topbot;
			case 8603: return "\u2AF1"; // &topcir;
			case 8607: return "\uD835\uDD4B"; // &Topf;
			case 8609: return "\uD835\uDD65"; // &topf;
			case 8613: return "\u2ADA"; // &topfork;
			case 8616: return "\u2929"; // &tosa;
			case 8622: return "\u2034"; // &tprime;
			case 8627: return "\u2122"; // &TRADE;
			case 8632: return "\u2122"; // &trade;
			case 8639: return "\u25B5"; // &triangle;
			case 8644: return "\u25BF"; // &triangledown;
			case 8649: return "\u25C3"; // &triangleleft;
			case 8652: return "\u22B4"; // &trianglelefteq;
			case 8654: return "\u225C"; // &triangleq;
			case 8660: return "\u25B9"; // &triangleright;
			case 8663: return "\u22B5"; // &trianglerighteq;
			case 8667: return "\u25EC"; // &tridot;
			case 8669: return "\u225C"; // &trie;
			case 8675: return "\u2A3A"; // &triminus;
			case 8684: return "\u20DB"; // &TripleDot;
			case 8689: return "\u2A39"; // &triplus;
			case 8692: return "\u29CD"; // &trisb;
			case 8697: return "\u2A3B"; // &tritime;
			case 8704: return "\u23E2"; // &trpezium;
			case 8708: return "\uD835\uDCAF"; // &Tscr;
			case 8712: return "\uD835\uDCC9"; // &tscr;
			case 8716: return "\u0426"; // &TScy;
			case 8718: return "\u0446"; // &tscy;
			case 8722: return "\u040B"; // &TSHcy;
			case 8726: return "\u045B"; // &tshcy;
			case 8731: return "\u0166"; // &Tstrok;
			case 8736: return "\u0167"; // &tstrok;
			case 8741: return "\u226C"; // &twixt;
			case 8756: return "\u219E"; // &twoheadleftarrow;
			case 8767: return "\u21A0"; // &twoheadrightarrow;
			case 8773: return "\u00DA"; // &Uacute
			case 8774: return "\u00DA"; // &Uacute;
			case 8780: return "\u00FA"; // &uacute
			case 8781: return "\u00FA"; // &uacute;
			case 8784: return "\u219F"; // &Uarr;
			case 8788: return "\u21D1"; // &uArr;
			case 8791: return "\u2191"; // &uarr;
			case 8796: return "\u2949"; // &Uarrocir;
			case 8801: return "\u040E"; // &Ubrcy;
			case 8806: return "\u045E"; // &ubrcy;
			case 8810: return "\u016C"; // &Ubreve;
			case 8814: return "\u016D"; // &ubreve;
			case 8818: return "\u00DB"; // &Ucirc
			case 8819: return "\u00DB"; // &Ucirc;
			case 8823: return "\u00FB"; // &ucirc
			case 8824: return "\u00FB"; // &ucirc;
			case 8826: return "\u0423"; // &Ucy;
			case 8828: return "\u0443"; // &ucy;
			case 8833: return "\u21C5"; // &udarr;
			case 8839: return "\u0170"; // &Udblac;
			case 8844: return "\u0171"; // &udblac;
			case 8848: return "\u296E"; // &udhar;
			case 8854: return "\u297E"; // &ufisht;
			case 8857: return "\uD835\uDD18"; // &Ufr;
			case 8859: return "\uD835\uDD32"; // &ufr;
			case 8864: return "\u00D9"; // &Ugrave
			case 8865: return "\u00D9"; // &Ugrave;
			case 8870: return "\u00F9"; // &ugrave
			case 8871: return "\u00F9"; // &ugrave;
			case 8875: return "\u2963"; // &uHar;
			case 8880: return "\u21BF"; // &uharl;
			case 8882: return "\u21BE"; // &uharr;
			case 8886: return "\u2580"; // &uhblk;
			case 8892: return "\u231C"; // &ulcorn;
			case 8895: return "\u231C"; // &ulcorner;
			case 8899: return "\u230F"; // &ulcrop;
			case 8903: return "\u25F8"; // &ultri;
			case 8908: return "\u016A"; // &Umacr;
			case 8913: return "\u016B"; // &umacr;
			case 8914: return "\u00A8"; // &uml
			case 8915: return "\u00A8"; // &uml;
			case 8923: return "\u005F"; // &UnderBar;
			case 8928: return "\u23DF"; // &UnderBrace;
			case 8932: return "\u23B5"; // &UnderBracket;
			case 8944: return "\u23DD"; // &UnderParenthesis;
			case 8948: return "\u22C3"; // &Union;
			case 8953: return "\u228E"; // &UnionPlus;
			case 8958: return "\u0172"; // &Uogon;
			case 8963: return "\u0173"; // &uogon;
			case 8966: return "\uD835\uDD4C"; // &Uopf;
			case 8969: return "\uD835\uDD66"; // &uopf;
			case 8976: return "\u2191"; // &UpArrow;
			case 8982: return "\u21D1"; // &Uparrow;
			case 8989: return "\u2191"; // &uparrow;
			case 8993: return "\u2912"; // &UpArrowBar;
			case 9003: return "\u21C5"; // &UpArrowDownArrow;
			case 9013: return "\u2195"; // &UpDownArrow;
			case 9023: return "\u21D5"; // &Updownarrow;
			case 9033: return "\u2195"; // &updownarrow;
			case 9045: return "\u296E"; // &UpEquilibrium;
			case 9057: return "\u21BF"; // &upharpoonleft;
			case 9063: return "\u21BE"; // &upharpoonright;
			case 9067: return "\u228E"; // &uplus;
			case 9080: return "\u2196"; // &UpperLeftArrow;
			case 9091: return "\u2197"; // &UpperRightArrow;
			case 9094: return "\u03D2"; // &Upsi;
			case 9097: return "\u03C5"; // &upsi;
			case 9099: return "\u03D2"; // &upsih;
			case 9103: return "\u03A5"; // &Upsilon;
			case 9107: return "\u03C5"; // &upsilon;
			case 9111: return "\u22A5"; // &UpTee;
			case 9117: return "\u21A5"; // &UpTeeArrow;
			case 9126: return "\u21C8"; // &upuparrows;
			case 9132: return "\u231D"; // &urcorn;
			case 9135: return "\u231D"; // &urcorner;
			case 9139: return "\u230E"; // &urcrop;
			case 9144: return "\u016E"; // &Uring;
			case 9148: return "\u016F"; // &uring;
			case 9152: return "\u25F9"; // &urtri;
			case 9156: return "\uD835\uDCB0"; // &Uscr;
			case 9160: return "\uD835\uDCCA"; // &uscr;
			case 9165: return "\u22F0"; // &utdot;
			case 9171: return "\u0168"; // &Utilde;
			case 9176: return "\u0169"; // &utilde;
			case 9179: return "\u25B5"; // &utri;
			case 9181: return "\u25B4"; // &utrif;
			case 9186: return "\u21C8"; // &uuarr;
			case 9189: return "\u00DC"; // &Uuml
			case 9190: return "\u00DC"; // &Uuml;
			case 9192: return "\u00FC"; // &uuml
			case 9193: return "\u00FC"; // &uuml;
			case 9200: return "\u29A7"; // &uwangle;
			case 9207: return "\u299C"; // &vangrt;
			case 9216: return "\u03F5"; // &varepsilon;
			case 9222: return "\u03F0"; // &varkappa;
			case 9230: return "\u2205"; // &varnothing;
			case 9234: return "\u03D5"; // &varphi;
			case 9236: return "\u03D6"; // &varpi;
			case 9242: return "\u221D"; // &varpropto;
			case 9246: return "\u21D5"; // &vArr;
			case 9248: return "\u2195"; // &varr;
			case 9251: return "\u03F1"; // &varrho;
			case 9257: return "\u03C2"; // &varsigma;
			case 9266: return "\u228A\uFE00"; // &varsubsetneq;
			case 9268: return "\u2ACB\uFE00"; // &varsubsetneqq;
			case 9276: return "\u228B\uFE00"; // &varsupsetneq;
			case 9278: return "\u2ACC\uFE00"; // &varsupsetneqq;
			case 9284: return "\u03D1"; // &vartheta;
			case 9296: return "\u22B2"; // &vartriangleleft;
			case 9302: return "\u22B3"; // &vartriangleright;
			case 9307: return "\u2AEB"; // &Vbar;
			case 9311: return "\u2AE8"; // &vBar;
			case 9313: return "\u2AE9"; // &vBarv;
			case 9316: return "\u0412"; // &Vcy;
			case 9319: return "\u0432"; // &vcy;
			case 9324: return "\u22AB"; // &VDash;
			case 9329: return "\u22A9"; // &Vdash;
			case 9334: return "\u22A8"; // &vDash;
			case 9339: return "\u22A2"; // &vdash;
			case 9341: return "\u2AE6"; // &Vdashl;
			case 9344: return "\u22C1"; // &Vee;
			case 9347: return "\u2228"; // &vee;
			case 9351: return "\u22BB"; // &veebar;
			case 9354: return "\u225A"; // &veeeq;
			case 9359: return "\u22EE"; // &vellip;
			case 9364: return "\u2016"; // &Verbar;
			case 9369: return "\u007C"; // &verbar;
			case 9371: return "\u2016"; // &Vert;
			case 9373: return "\u007C"; // &vert;
			case 9381: return "\u2223"; // &VerticalBar;
			case 9386: return "\u007C"; // &VerticalLine;
			case 9396: return "\u2758"; // &VerticalSeparator;
			case 9402: return "\u2240"; // &VerticalTilde;
			case 9413: return "\u200A"; // &VeryThinSpace;
			case 9416: return "\uD835\uDD19"; // &Vfr;
			case 9419: return "\uD835\uDD33"; // &vfr;
			case 9424: return "\u22B2"; // &vltri;
			case 9429: return "\u2282\u20D2"; // &vnsub;
			case 9431: return "\u2283\u20D2"; // &vnsup;
			case 9435: return "\uD835\uDD4D"; // &Vopf;
			case 9439: return "\uD835\uDD67"; // &vopf;
			case 9444: return "\u221D"; // &vprop;
			case 9449: return "\u22B3"; // &vrtri;
			case 9453: return "\uD835\uDCB1"; // &Vscr;
			case 9457: return "\uD835\uDCCB"; // &vscr;
			case 9462: return "\u2ACB\uFE00"; // &vsubnE;
			case 9464: return "\u228A\uFE00"; // &vsubne;
			case 9468: return "\u2ACC\uFE00"; // &vsupnE;
			case 9470: return "\u228B\uFE00"; // &vsupne;
			case 9476: return "\u22AA"; // &Vvdash;
			case 9483: return "\u299A"; // &vzigzag;
			case 9489: return "\u0174"; // &Wcirc;
			case 9495: return "\u0175"; // &wcirc;
			case 9501: return "\u2A5F"; // &wedbar;
			case 9506: return "\u22C0"; // &Wedge;
			case 9509: return "\u2227"; // &wedge;
			case 9511: return "\u2259"; // &wedgeq;
			case 9516: return "\u2118"; // &weierp;
			case 9519: return "\uD835\uDD1A"; // &Wfr;
			case 9522: return "\uD835\uDD34"; // &wfr;
			case 9526: return "\uD835\uDD4E"; // &Wopf;
			case 9530: return "\uD835\uDD68"; // &wopf;
			case 9532: return "\u2118"; // &wp;
			case 9534: return "\u2240"; // &wr;
			case 9539: return "\u2240"; // &wreath;
			case 9543: return "\uD835\uDCB2"; // &Wscr;
			case 9547: return "\uD835\uDCCC"; // &wscr;
			case 9552: return "\u22C2"; // &xcap;
			case 9556: return "\u25EF"; // &xcirc;
			case 9559: return "\u22C3"; // &xcup;
			case 9564: return "\u25BD"; // &xdtri;
			case 9568: return "\uD835\uDD1B"; // &Xfr;
			case 9571: return "\uD835\uDD35"; // &xfr;
			case 9576: return "\u27FA"; // &xhArr;
			case 9580: return "\u27F7"; // &xharr;
			case 9582: return "\u039E"; // &Xi;
			case 9584: return "\u03BE"; // &xi;
			case 9589: return "\u27F8"; // &xlArr;
			case 9593: return "\u27F5"; // &xlarr;
			case 9597: return "\u27FC"; // &xmap;
			case 9601: return "\u22FB"; // &xnis;
			case 9606: return "\u2A00"; // &xodot;
			case 9610: return "\uD835\uDD4F"; // &Xopf;
			case 9613: return "\uD835\uDD69"; // &xopf;
			case 9617: return "\u2A01"; // &xoplus;
			case 9622: return "\u2A02"; // &xotime;
			case 9627: return "\u27F9"; // &xrArr;
			case 9631: return "\u27F6"; // &xrarr;
			case 9635: return "\uD835\uDCB3"; // &Xscr;
			case 9639: return "\uD835\uDCCD"; // &xscr;
			case 9644: return "\u2A06"; // &xsqcup;
			case 9650: return "\u2A04"; // &xuplus;
			case 9654: return "\u25B3"; // &xutri;
			case 9658: return "\u22C1"; // &xvee;
			case 9664: return "\u22C0"; // &xwedge;
			case 9670: return "\u00DD"; // &Yacute
			case 9671: return "\u00DD"; // &Yacute;
			case 9677: return "\u00FD"; // &yacute
			case 9678: return "\u00FD"; // &yacute;
			case 9682: return "\u042F"; // &YAcy;
			case 9684: return "\u044F"; // &yacy;
			case 9689: return "\u0176"; // &Ycirc;
			case 9694: return "\u0177"; // &ycirc;
			case 9696: return "\u042B"; // &Ycy;
			case 9698: return "\u044B"; // &ycy;
			case 9700: return "\u00A5"; // &yen
			case 9701: return "\u00A5"; // &yen;
			case 9704: return "\uD835\uDD1C"; // &Yfr;
			case 9707: return "\uD835\uDD36"; // &yfr;
			case 9711: return "\u0407"; // &YIcy;
			case 9715: return "\u0457"; // &yicy;
			case 9719: return "\uD835\uDD50"; // &Yopf;
			case 9723: return "\uD835\uDD6A"; // &yopf;
			case 9727: return "\uD835\uDCB4"; // &Yscr;
			case 9731: return "\uD835\uDCCE"; // &yscr;
			case 9735: return "\u042E"; // &YUcy;
			case 9739: return "\u044E"; // &yucy;
			case 9743: return "\u0178"; // &Yuml;
			case 9745: return "\u00FF"; // &yuml
			case 9746: return "\u00FF"; // &yuml;
			case 9753: return "\u0179"; // &Zacute;
			case 9760: return "\u017A"; // &zacute;
			case 9766: return "\u017D"; // &Zcaron;
			case 9772: return "\u017E"; // &zcaron;
			case 9774: return "\u0417"; // &Zcy;
			case 9776: return "\u0437"; // &zcy;
			case 9780: return "\u017B"; // &Zdot;
			case 9784: return "\u017C"; // &zdot;
			case 9790: return "\u2128"; // &zeetrf;
			case 9804: return "\u200B"; // &ZeroWidthSpace;
			case 9807: return "\u0396"; // &Zeta;
			case 9810: return "\u03B6"; // &zeta;
			case 9813: return "\u2128"; // &Zfr;
			case 9816: return "\uD835\uDD37"; // &zfr;
			case 9820: return "\u0416"; // &ZHcy;
			case 9824: return "\u0436"; // &zhcy;
			case 9831: return "\u21DD"; // &zigrarr;
			case 9835: return "\u2124"; // &Zopf;
			case 9839: return "\uD835\uDD6B"; // &zopf;
			case 9843: return "\uD835\uDCB5"; // &Zscr;
			case 9847: return "\uD835\uDCCF"; // &zscr;
			case 9850: return "\u200D"; // &zwj;
			case 9853: return "\u200C"; // &zwnj;
			default: return null;
			}
		}

		string GetNamedEntityValue ()
		{
			int startIndex = index;
			string decoded = null;

			while (startIndex > 0) {
				if ((decoded = GetNamedEntityValue (states[startIndex - 1])) != null)
					break;

				startIndex--;
			}

			if (decoded == null)
				decoded = string.Empty;

			if (startIndex < index)
				decoded += new string (pushed, startIndex, index - startIndex);

			return decoded;
		}
	}
}
