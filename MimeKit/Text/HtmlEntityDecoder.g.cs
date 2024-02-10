//
// HtmlEntityDecoder.g.cs
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

// WARNING: This file is auto-generated. DO NOT EDIT!

using System.Collections.Generic;

namespace MimeKit.Text {
	public partial class HtmlEntityDecoder
	{
		const int MaxEntityLength = 33;

		static readonly Dictionary<int, string> NamedEntities;

		readonly struct Transition
		{
			public readonly int From;
			public readonly int To;

			public Transition (int from, int to)
			{
				From = from;
				To = to;
			}
		}

		static readonly Transition[] TransitionTable_1;
		static readonly Transition[] TransitionTable_2;
		static readonly Transition[] TransitionTable_3;
		static readonly Transition[] TransitionTable_4;
		static readonly Transition[] TransitionTable_5;
		static readonly Transition[] TransitionTable_6;
		static readonly Transition[] TransitionTable_7;
		static readonly Transition[] TransitionTable_8;
		static readonly Transition[] TransitionTable_semicolon;
		static readonly Transition[] TransitionTable_A;
		static readonly Transition[] TransitionTable_B;
		static readonly Transition[] TransitionTable_C;
		static readonly Transition[] TransitionTable_D;
		static readonly Transition[] TransitionTable_E;
		static readonly Transition[] TransitionTable_F;
		static readonly Transition[] TransitionTable_G;
		static readonly Transition[] TransitionTable_H;
		static readonly Transition[] TransitionTable_I;
		static readonly Transition[] TransitionTable_J;
		static readonly Transition[] TransitionTable_K;
		static readonly Transition[] TransitionTable_L;
		static readonly Transition[] TransitionTable_M;
		static readonly Transition[] TransitionTable_N;
		static readonly Transition[] TransitionTable_O;
		static readonly Transition[] TransitionTable_P;
		static readonly Transition[] TransitionTable_Q;
		static readonly Transition[] TransitionTable_R;
		static readonly Transition[] TransitionTable_S;
		static readonly Transition[] TransitionTable_T;
		static readonly Transition[] TransitionTable_U;
		static readonly Transition[] TransitionTable_V;
		static readonly Transition[] TransitionTable_W;
		static readonly Transition[] TransitionTable_X;
		static readonly Transition[] TransitionTable_Y;
		static readonly Transition[] TransitionTable_Z;
		static readonly Transition[] TransitionTable_a;
		static readonly Transition[] TransitionTable_b;
		static readonly Transition[] TransitionTable_c;
		static readonly Transition[] TransitionTable_d;
		static readonly Transition[] TransitionTable_e;
		static readonly Transition[] TransitionTable_f;
		static readonly Transition[] TransitionTable_g;
		static readonly Transition[] TransitionTable_h;
		static readonly Transition[] TransitionTable_i;
		static readonly Transition[] TransitionTable_j;
		static readonly Transition[] TransitionTable_k;
		static readonly Transition[] TransitionTable_l;
		static readonly Transition[] TransitionTable_m;
		static readonly Transition[] TransitionTable_n;
		static readonly Transition[] TransitionTable_o;
		static readonly Transition[] TransitionTable_p;
		static readonly Transition[] TransitionTable_q;
		static readonly Transition[] TransitionTable_r;
		static readonly Transition[] TransitionTable_s;
		static readonly Transition[] TransitionTable_t;
		static readonly Transition[] TransitionTable_u;
		static readonly Transition[] TransitionTable_v;
		static readonly Transition[] TransitionTable_w;
		static readonly Transition[] TransitionTable_x;
		static readonly Transition[] TransitionTable_y;
		static readonly Transition[] TransitionTable_z;

		static HtmlEntityDecoder ()
		{
			TransitionTable_1 = new Transition[4] {
				new Transition (566, 567), // &blk -> &blk1
				new Transition (2280, 2282), // &emsp -> &emsp1
				new Transition (2649, 2650), // &frac -> &frac1
				new Transition (8284, 8286) // &sup -> &sup1
			};
			TransitionTable_2 = new Transition[4] {
				new Transition (567, 568), // &blk1 -> &blk12
				new Transition (2649, 2663), // &frac -> &frac2
				new Transition (2650, 2651), // &frac1 -> &frac12
				new Transition (8284, 8288) // &sup -> &sup2
			};
			TransitionTable_3 = new Transition[6] {
				new Transition (566, 572), // &blk -> &blk3
				new Transition (2282, 2283), // &emsp1 -> &emsp13
				new Transition (2649, 2668), // &frac -> &frac3
				new Transition (2650, 2653), // &frac1 -> &frac13
				new Transition (2663, 2664), // &frac2 -> &frac23
				new Transition (8284, 8290) // &sup -> &sup3
			};
			TransitionTable_4 = new Transition[7] {
				new Transition (567, 570), // &blk1 -> &blk14
				new Transition (572, 573), // &blk3 -> &blk34
				new Transition (2282, 2285), // &emsp1 -> &emsp14
				new Transition (2649, 2675), // &frac -> &frac4
				new Transition (2650, 2655), // &frac1 -> &frac14
				new Transition (2668, 2669), // &frac3 -> &frac34
				new Transition (8464, 8465) // &there -> &there4
			};
			TransitionTable_5 = new Transition[5] {
				new Transition (2649, 2678), // &frac -> &frac5
				new Transition (2650, 2657), // &frac1 -> &frac15
				new Transition (2663, 2666), // &frac2 -> &frac25
				new Transition (2668, 2671), // &frac3 -> &frac35
				new Transition (2675, 2676) // &frac4 -> &frac45
			};
			TransitionTable_6 = new Transition[2] {
				new Transition (2650, 2659), // &frac1 -> &frac16
				new Transition (2678, 2679) // &frac5 -> &frac56
			};
			TransitionTable_7 = new Transition[1] {
				new Transition (2649, 2683) // &frac -> &frac7
			};
			TransitionTable_8 = new Transition[4] {
				new Transition (2650, 2661), // &frac1 -> &frac18
				new Transition (2668, 2673), // &frac3 -> &frac38
				new Transition (2678, 2681), // &frac5 -> &frac58
				new Transition (2683, 2684) // &frac7 -> &frac78
			};
			TransitionTable_semicolon = new Transition[2125] {
				new Transition (6, 7), // &Aacute -> &Aacute;
				new Transition (13, 14), // &aacute -> &aacute;
				new Transition (19, 20), // &Abreve -> &Abreve;
				new Transition (25, 26), // &abreve -> &abreve;
				new Transition (27, 28), // &ac -> &ac;
				new Transition (29, 30), // &acd -> &acd;
				new Transition (31, 32), // &acE -> &acE;
				new Transition (36, 37), // &Acirc -> &Acirc;
				new Transition (40, 41), // &acirc -> &acirc;
				new Transition (44, 45), // &acute -> &acute;
				new Transition (46, 47), // &Acy -> &Acy;
				new Transition (48, 49), // &acy -> &acy;
				new Transition (53, 54), // &AElig -> &AElig;
				new Transition (58, 59), // &aelig -> &aelig;
				new Transition (60, 61), // &af -> &af;
				new Transition (63, 64), // &Afr -> &Afr;
				new Transition (65, 66), // &afr -> &afr;
				new Transition (71, 72), // &Agrave -> &Agrave;
				new Transition (77, 78), // &agrave -> &agrave;
				new Transition (84, 85), // &alefsym -> &alefsym;
				new Transition (87, 88), // &aleph -> &aleph;
				new Transition (92, 93), // &Alpha -> &Alpha;
				new Transition (96, 97), // &alpha -> &alpha;
				new Transition (101, 102), // &Amacr -> &Amacr;
				new Transition (106, 107), // &amacr -> &amacr;
				new Transition (109, 110), // &amalg -> &amalg;
				new Transition (112, 113), // &AMP -> &AMP;
				new Transition (114, 115), // &amp -> &amp;
				new Transition (117, 118), // &And -> &And;
				new Transition (120, 121), // &and -> &and;
				new Transition (124, 125), // &andand -> &andand;
				new Transition (126, 127), // &andd -> &andd;
				new Transition (132, 133), // &andslope -> &andslope;
				new Transition (134, 135), // &andv -> &andv;
				new Transition (136, 137), // &ang -> &ang;
				new Transition (138, 139), // &ange -> &ange;
				new Transition (141, 142), // &angle -> &angle;
				new Transition (145, 146), // &angmsd -> &angmsd;
				new Transition (148, 149), // &angmsdaa -> &angmsdaa;
				new Transition (150, 151), // &angmsdab -> &angmsdab;
				new Transition (152, 153), // &angmsdac -> &angmsdac;
				new Transition (154, 155), // &angmsdad -> &angmsdad;
				new Transition (156, 157), // &angmsdae -> &angmsdae;
				new Transition (158, 159), // &angmsdaf -> &angmsdaf;
				new Transition (160, 161), // &angmsdag -> &angmsdag;
				new Transition (162, 163), // &angmsdah -> &angmsdah;
				new Transition (165, 166), // &angrt -> &angrt;
				new Transition (168, 169), // &angrtvb -> &angrtvb;
				new Transition (170, 171), // &angrtvbd -> &angrtvbd;
				new Transition (174, 175), // &angsph -> &angsph;
				new Transition (176, 177), // &angst -> &angst;
				new Transition (181, 182), // &angzarr -> &angzarr;
				new Transition (186, 187), // &Aogon -> &Aogon;
				new Transition (191, 192), // &aogon -> &aogon;
				new Transition (194, 195), // &Aopf -> &Aopf;
				new Transition (197, 198), // &aopf -> &aopf;
				new Transition (199, 200), // &ap -> &ap;
				new Transition (204, 205), // &apacir -> &apacir;
				new Transition (206, 207), // &apE -> &apE;
				new Transition (208, 209), // &ape -> &ape;
				new Transition (211, 212), // &apid -> &apid;
				new Transition (214, 215), // &apos -> &apos;
				new Transition (227, 228), // &ApplyFunction -> &ApplyFunction;
				new Transition (232, 233), // &approx -> &approx;
				new Transition (235, 236), // &approxeq -> &approxeq;
				new Transition (240, 241), // &Aring -> &Aring;
				new Transition (245, 246), // &aring -> &aring;
				new Transition (249, 250), // &Ascr -> &Ascr;
				new Transition (253, 254), // &ascr -> &ascr;
				new Transition (258, 259), // &Assign -> &Assign;
				new Transition (260, 261), // &ast -> &ast;
				new Transition (264, 265), // &asymp -> &asymp;
				new Transition (267, 268), // &asympeq -> &asympeq;
				new Transition (273, 274), // &Atilde -> &Atilde;
				new Transition (279, 280), // &atilde -> &atilde;
				new Transition (283, 284), // &Auml -> &Auml;
				new Transition (287, 288), // &auml -> &auml;
				new Transition (295, 296), // &awconint -> &awconint;
				new Transition (299, 300), // &awint -> &awint;
				new Transition (308, 309), // &backcong -> &backcong;
				new Transition (316, 317), // &backepsilon -> &backepsilon;
				new Transition (322, 323), // &backprime -> &backprime;
				new Transition (326, 327), // &backsim -> &backsim;
				new Transition (329, 330), // &backsimeq -> &backsimeq;
				new Transition (339, 340), // &Backslash -> &Backslash;
				new Transition (342, 343), // &Barv -> &Barv;
				new Transition (347, 348), // &barvee -> &barvee;
				new Transition (351, 352), // &Barwed -> &Barwed;
				new Transition (355, 356), // &barwed -> &barwed;
				new Transition (358, 359), // &barwedge -> &barwedge;
				new Transition (362, 363), // &bbrk -> &bbrk;
				new Transition (367, 368), // &bbrktbrk -> &bbrktbrk;
				new Transition (372, 373), // &bcong -> &bcong;
				new Transition (375, 376), // &Bcy -> &Bcy;
				new Transition (377, 378), // &bcy -> &bcy;
				new Transition (382, 383), // &bdquo -> &bdquo;
				new Transition (388, 389), // &becaus -> &becaus;
				new Transition (395, 396), // &Because -> &Because;
				new Transition (397, 398), // &because -> &because;
				new Transition (403, 404), // &bemptyv -> &bemptyv;
				new Transition (407, 408), // &bepsi -> &bepsi;
				new Transition (412, 413), // &bernou -> &bernou;
				new Transition (421, 422), // &Bernoullis -> &Bernoullis;
				new Transition (424, 425), // &Beta -> &Beta;
				new Transition (427, 428), // &beta -> &beta;
				new Transition (429, 430), // &beth -> &beth;
				new Transition (434, 435), // &between -> &between;
				new Transition (437, 438), // &Bfr -> &Bfr;
				new Transition (440, 441), // &bfr -> &bfr;
				new Transition (446, 447), // &bigcap -> &bigcap;
				new Transition (450, 451), // &bigcirc -> &bigcirc;
				new Transition (453, 454), // &bigcup -> &bigcup;
				new Transition (458, 459), // &bigodot -> &bigodot;
				new Transition (463, 464), // &bigoplus -> &bigoplus;
				new Transition (469, 470), // &bigotimes -> &bigotimes;
				new Transition (475, 476), // &bigsqcup -> &bigsqcup;
				new Transition (479, 480), // &bigstar -> &bigstar;
				new Transition (492, 493), // &bigtriangledown -> &bigtriangledown;
				new Transition (495, 496), // &bigtriangleup -> &bigtriangleup;
				new Transition (501, 502), // &biguplus -> &biguplus;
				new Transition (505, 506), // &bigvee -> &bigvee;
				new Transition (511, 512), // &bigwedge -> &bigwedge;
				new Transition (517, 518), // &bkarow -> &bkarow;
				new Transition (529, 530), // &blacklozenge -> &blacklozenge;
				new Transition (536, 537), // &blacksquare -> &blacksquare;
				new Transition (545, 546), // &blacktriangle -> &blacktriangle;
				new Transition (550, 551), // &blacktriangledown -> &blacktriangledown;
				new Transition (555, 556), // &blacktriangleleft -> &blacktriangleleft;
				new Transition (561, 562), // &blacktriangleright -> &blacktriangleright;
				new Transition (564, 565), // &blank -> &blank;
				new Transition (568, 569), // &blk12 -> &blk12;
				new Transition (570, 571), // &blk14 -> &blk14;
				new Transition (573, 574), // &blk34 -> &blk34;
				new Transition (577, 578), // &block -> &block;
				new Transition (580, 581), // &bne -> &bne;
				new Transition (585, 586), // &bnequiv -> &bnequiv;
				new Transition (589, 590), // &bNot -> &bNot;
				new Transition (592, 593), // &bnot -> &bnot;
				new Transition (596, 597), // &Bopf -> &Bopf;
				new Transition (600, 601), // &bopf -> &bopf;
				new Transition (602, 603), // &bot -> &bot;
				new Transition (606, 607), // &bottom -> &bottom;
				new Transition (611, 612), // &bowtie -> &bowtie;
				new Transition (616, 617), // &boxbox -> &boxbox;
				new Transition (619, 620), // &boxDL -> &boxDL;
				new Transition (621, 622), // &boxDl -> &boxDl;
				new Transition (624, 625), // &boxdL -> &boxdL;
				new Transition (626, 627), // &boxdl -> &boxdl;
				new Transition (628, 629), // &boxDR -> &boxDR;
				new Transition (630, 631), // &boxDr -> &boxDr;
				new Transition (632, 633), // &boxdR -> &boxdR;
				new Transition (634, 635), // &boxdr -> &boxdr;
				new Transition (636, 637), // &boxH -> &boxH;
				new Transition (638, 639), // &boxh -> &boxh;
				new Transition (640, 641), // &boxHD -> &boxHD;
				new Transition (642, 643), // &boxHd -> &boxHd;
				new Transition (644, 645), // &boxhD -> &boxhD;
				new Transition (646, 647), // &boxhd -> &boxhd;
				new Transition (648, 649), // &boxHU -> &boxHU;
				new Transition (650, 651), // &boxHu -> &boxHu;
				new Transition (652, 653), // &boxhU -> &boxhU;
				new Transition (654, 655), // &boxhu -> &boxhu;
				new Transition (660, 661), // &boxminus -> &boxminus;
				new Transition (665, 666), // &boxplus -> &boxplus;
				new Transition (671, 672), // &boxtimes -> &boxtimes;
				new Transition (674, 675), // &boxUL -> &boxUL;
				new Transition (676, 677), // &boxUl -> &boxUl;
				new Transition (679, 680), // &boxuL -> &boxuL;
				new Transition (681, 682), // &boxul -> &boxul;
				new Transition (683, 684), // &boxUR -> &boxUR;
				new Transition (685, 686), // &boxUr -> &boxUr;
				new Transition (687, 688), // &boxuR -> &boxuR;
				new Transition (689, 690), // &boxur -> &boxur;
				new Transition (691, 692), // &boxV -> &boxV;
				new Transition (693, 694), // &boxv -> &boxv;
				new Transition (695, 696), // &boxVH -> &boxVH;
				new Transition (697, 698), // &boxVh -> &boxVh;
				new Transition (699, 700), // &boxvH -> &boxvH;
				new Transition (701, 702), // &boxvh -> &boxvh;
				new Transition (703, 704), // &boxVL -> &boxVL;
				new Transition (705, 706), // &boxVl -> &boxVl;
				new Transition (707, 708), // &boxvL -> &boxvL;
				new Transition (709, 710), // &boxvl -> &boxvl;
				new Transition (711, 712), // &boxVR -> &boxVR;
				new Transition (713, 714), // &boxVr -> &boxVr;
				new Transition (715, 716), // &boxvR -> &boxvR;
				new Transition (717, 718), // &boxvr -> &boxvr;
				new Transition (723, 724), // &bprime -> &bprime;
				new Transition (728, 729), // &Breve -> &Breve;
				new Transition (733, 734), // &breve -> &breve;
				new Transition (738, 739), // &brvbar -> &brvbar;
				new Transition (742, 743), // &Bscr -> &Bscr;
				new Transition (746, 747), // &bscr -> &bscr;
				new Transition (750, 751), // &bsemi -> &bsemi;
				new Transition (753, 754), // &bsim -> &bsim;
				new Transition (755, 756), // &bsime -> &bsime;
				new Transition (758, 759), // &bsol -> &bsol;
				new Transition (760, 761), // &bsolb -> &bsolb;
				new Transition (765, 766), // &bsolhsub -> &bsolhsub;
				new Transition (769, 770), // &bull -> &bull;
				new Transition (772, 773), // &bullet -> &bullet;
				new Transition (775, 776), // &bump -> &bump;
				new Transition (777, 778), // &bumpE -> &bumpE;
				new Transition (779, 780), // &bumpe -> &bumpe;
				new Transition (785, 786), // &Bumpeq -> &Bumpeq;
				new Transition (787, 788), // &bumpeq -> &bumpeq;
				new Transition (794, 795), // &Cacute -> &Cacute;
				new Transition (801, 802), // &cacute -> &cacute;
				new Transition (803, 804), // &Cap -> &Cap;
				new Transition (805, 806), // &cap -> &cap;
				new Transition (809, 810), // &capand -> &capand;
				new Transition (815, 816), // &capbrcup -> &capbrcup;
				new Transition (819, 820), // &capcap -> &capcap;
				new Transition (822, 823), // &capcup -> &capcup;
				new Transition (826, 827), // &capdot -> &capdot;
				new Transition (844, 845), // &CapitalDifferentialD -> &CapitalDifferentialD;
				new Transition (846, 847), // &caps -> &caps;
				new Transition (850, 851), // &caret -> &caret;
				new Transition (853, 854), // &caron -> &caron;
				new Transition (859, 860), // &Cayleys -> &Cayleys;
				new Transition (864, 865), // &ccaps -> &ccaps;
				new Transition (870, 871), // &Ccaron -> &Ccaron;
				new Transition (874, 875), // &ccaron -> &ccaron;
				new Transition (879, 880), // &Ccedil -> &Ccedil;
				new Transition (884, 885), // &ccedil -> &ccedil;
				new Transition (888, 889), // &Ccirc -> &Ccirc;
				new Transition (892, 893), // &ccirc -> &ccirc;
				new Transition (898, 899), // &Cconint -> &Cconint;
				new Transition (902, 903), // &ccups -> &ccups;
				new Transition (905, 906), // &ccupssm -> &ccupssm;
				new Transition (909, 910), // &Cdot -> &Cdot;
				new Transition (913, 914), // &cdot -> &cdot;
				new Transition (918, 919), // &cedil -> &cedil;
				new Transition (925, 926), // &Cedilla -> &Cedilla;
				new Transition (931, 932), // &cemptyv -> &cemptyv;
				new Transition (934, 935), // &cent -> &cent;
				new Transition (942, 943), // &CenterDot -> &CenterDot;
				new Transition (948, 949), // &centerdot -> &centerdot;
				new Transition (951, 952), // &Cfr -> &Cfr;
				new Transition (954, 955), // &cfr -> &cfr;
				new Transition (958, 959), // &CHcy -> &CHcy;
				new Transition (962, 963), // &chcy -> &chcy;
				new Transition (966, 967), // &check -> &check;
				new Transition (971, 972), // &checkmark -> &checkmark;
				new Transition (974, 975), // &Chi -> &Chi;
				new Transition (976, 977), // &chi -> &chi;
				new Transition (979, 980), // &cir -> &cir;
				new Transition (981, 982), // &circ -> &circ;
				new Transition (984, 985), // &circeq -> &circeq;
				new Transition (996, 997), // &circlearrowleft -> &circlearrowleft;
				new Transition (1002, 1003), // &circlearrowright -> &circlearrowright;
				new Transition (1007, 1008), // &circledast -> &circledast;
				new Transition (1012, 1013), // &circledcirc -> &circledcirc;
				new Transition (1017, 1018), // &circleddash -> &circleddash;
				new Transition (1026, 1027), // &CircleDot -> &CircleDot;
				new Transition (1028, 1029), // &circledR -> &circledR;
				new Transition (1030, 1031), // &circledS -> &circledS;
				new Transition (1036, 1037), // &CircleMinus -> &CircleMinus;
				new Transition (1041, 1042), // &CirclePlus -> &CirclePlus;
				new Transition (1047, 1048), // &CircleTimes -> &CircleTimes;
				new Transition (1049, 1050), // &cirE -> &cirE;
				new Transition (1051, 1052), // &cire -> &cire;
				new Transition (1057, 1058), // &cirfnint -> &cirfnint;
				new Transition (1061, 1062), // &cirmid -> &cirmid;
				new Transition (1066, 1067), // &cirscir -> &cirscir;
				new Transition (1090, 1091), // &ClockwiseContourIntegral -> &ClockwiseContourIntegral;
				new Transition (1109, 1110), // &CloseCurlyDoubleQuote -> &CloseCurlyDoubleQuote;
				new Transition (1115, 1116), // &CloseCurlyQuote -> &CloseCurlyQuote;
				new Transition (1120, 1121), // &clubs -> &clubs;
				new Transition (1124, 1125), // &clubsuit -> &clubsuit;
				new Transition (1129, 1130), // &Colon -> &Colon;
				new Transition (1134, 1135), // &colon -> &colon;
				new Transition (1136, 1137), // &Colone -> &Colone;
				new Transition (1138, 1139), // &colone -> &colone;
				new Transition (1140, 1141), // &coloneq -> &coloneq;
				new Transition (1144, 1145), // &comma -> &comma;
				new Transition (1146, 1147), // &commat -> &commat;
				new Transition (1148, 1149), // &comp -> &comp;
				new Transition (1151, 1152), // &compfn -> &compfn;
				new Transition (1158, 1159), // &complement -> &complement;
				new Transition (1162, 1163), // &complexes -> &complexes;
				new Transition (1165, 1166), // &cong -> &cong;
				new Transition (1169, 1170), // &congdot -> &congdot;
				new Transition (1177, 1178), // &Congruent -> &Congruent;
				new Transition (1181, 1182), // &Conint -> &Conint;
				new Transition (1185, 1186), // &conint -> &conint;
				new Transition (1198, 1199), // &ContourIntegral -> &ContourIntegral;
				new Transition (1201, 1202), // &Copf -> &Copf;
				new Transition (1204, 1205), // &copf -> &copf;
				new Transition (1208, 1209), // &coprod -> &coprod;
				new Transition (1215, 1216), // &Coproduct -> &Coproduct;
				new Transition (1219, 1220), // &COPY -> &COPY;
				new Transition (1221, 1222), // &copy -> &copy;
				new Transition (1224, 1225), // &copysr -> &copysr;
				new Transition (1254, 1255), // &CounterClockwiseContourIntegral -> &CounterClockwiseContourIntegral;
				new Transition (1259, 1260), // &crarr -> &crarr;
				new Transition (1264, 1265), // &Cross -> &Cross;
				new Transition (1268, 1269), // &cross -> &cross;
				new Transition (1272, 1273), // &Cscr -> &Cscr;
				new Transition (1276, 1277), // &cscr -> &cscr;
				new Transition (1279, 1280), // &csub -> &csub;
				new Transition (1281, 1282), // &csube -> &csube;
				new Transition (1283, 1284), // &csup -> &csup;
				new Transition (1285, 1286), // &csupe -> &csupe;
				new Transition (1290, 1291), // &ctdot -> &ctdot;
				new Transition (1297, 1298), // &cudarrl -> &cudarrl;
				new Transition (1299, 1300), // &cudarrr -> &cudarrr;
				new Transition (1303, 1304), // &cuepr -> &cuepr;
				new Transition (1306, 1307), // &cuesc -> &cuesc;
				new Transition (1311, 1312), // &cularr -> &cularr;
				new Transition (1313, 1314), // &cularrp -> &cularrp;
				new Transition (1316, 1317), // &Cup -> &Cup;
				new Transition (1318, 1319), // &cup -> &cup;
				new Transition (1324, 1325), // &cupbrcap -> &cupbrcap;
				new Transition (1328, 1329), // &CupCap -> &CupCap;
				new Transition (1332, 1333), // &cupcap -> &cupcap;
				new Transition (1335, 1336), // &cupcup -> &cupcup;
				new Transition (1339, 1340), // &cupdot -> &cupdot;
				new Transition (1342, 1343), // &cupor -> &cupor;
				new Transition (1344, 1345), // &cups -> &cups;
				new Transition (1349, 1350), // &curarr -> &curarr;
				new Transition (1351, 1352), // &curarrm -> &curarrm;
				new Transition (1360, 1361), // &curlyeqprec -> &curlyeqprec;
				new Transition (1365, 1366), // &curlyeqsucc -> &curlyeqsucc;
				new Transition (1369, 1370), // &curlyvee -> &curlyvee;
				new Transition (1375, 1376), // &curlywedge -> &curlywedge;
				new Transition (1379, 1380), // &curren -> &curren;
				new Transition (1391, 1392), // &curvearrowleft -> &curvearrowleft;
				new Transition (1397, 1398), // &curvearrowright -> &curvearrowright;
				new Transition (1401, 1402), // &cuvee -> &cuvee;
				new Transition (1405, 1406), // &cuwed -> &cuwed;
				new Transition (1413, 1414), // &cwconint -> &cwconint;
				new Transition (1417, 1418), // &cwint -> &cwint;
				new Transition (1423, 1424), // &cylcty -> &cylcty;
				new Transition (1430, 1431), // &Dagger -> &Dagger;
				new Transition (1437, 1438), // &dagger -> &dagger;
				new Transition (1442, 1443), // &daleth -> &daleth;
				new Transition (1445, 1446), // &Darr -> &Darr;
				new Transition (1449, 1450), // &dArr -> &dArr;
				new Transition (1452, 1453), // &darr -> &darr;
				new Transition (1455, 1456), // &dash -> &dash;
				new Transition (1459, 1460), // &Dashv -> &Dashv;
				new Transition (1461, 1462), // &dashv -> &dashv;
				new Transition (1468, 1469), // &dbkarow -> &dbkarow;
				new Transition (1472, 1473), // &dblac -> &dblac;
				new Transition (1478, 1479), // &Dcaron -> &Dcaron;
				new Transition (1484, 1485), // &dcaron -> &dcaron;
				new Transition (1486, 1487), // &Dcy -> &Dcy;
				new Transition (1488, 1489), // &dcy -> &dcy;
				new Transition (1490, 1491), // &DD -> &DD;
				new Transition (1492, 1493), // &dd -> &dd;
				new Transition (1498, 1499), // &ddagger -> &ddagger;
				new Transition (1501, 1502), // &ddarr -> &ddarr;
				new Transition (1508, 1509), // &DDotrahd -> &DDotrahd;
				new Transition (1514, 1515), // &ddotseq -> &ddotseq;
				new Transition (1517, 1518), // &deg -> &deg;
				new Transition (1520, 1521), // &Del -> &Del;
				new Transition (1523, 1524), // &Delta -> &Delta;
				new Transition (1527, 1528), // &delta -> &delta;
				new Transition (1533, 1534), // &demptyv -> &demptyv;
				new Transition (1539, 1540), // &dfisht -> &dfisht;
				new Transition (1542, 1543), // &Dfr -> &Dfr;
				new Transition (1544, 1545), // &dfr -> &dfr;
				new Transition (1548, 1549), // &dHar -> &dHar;
				new Transition (1553, 1554), // &dharl -> &dharl;
				new Transition (1555, 1556), // &dharr -> &dharr;
				new Transition (1571, 1572), // &DiacriticalAcute -> &DiacriticalAcute;
				new Transition (1575, 1576), // &DiacriticalDot -> &DiacriticalDot;
				new Transition (1585, 1586), // &DiacriticalDoubleAcute -> &DiacriticalDoubleAcute;
				new Transition (1591, 1592), // &DiacriticalGrave -> &DiacriticalGrave;
				new Transition (1597, 1598), // &DiacriticalTilde -> &DiacriticalTilde;
				new Transition (1601, 1602), // &diam -> &diam;
				new Transition (1606, 1607), // &Diamond -> &Diamond;
				new Transition (1610, 1611), // &diamond -> &diamond;
				new Transition (1615, 1616), // &diamondsuit -> &diamondsuit;
				new Transition (1617, 1618), // &diams -> &diams;
				new Transition (1619, 1620), // &die -> &die;
				new Transition (1631, 1632), // &DifferentialD -> &DifferentialD;
				new Transition (1637, 1638), // &digamma -> &digamma;
				new Transition (1641, 1642), // &disin -> &disin;
				new Transition (1643, 1644), // &div -> &div;
				new Transition (1647, 1648), // &divide -> &divide;
				new Transition (1655, 1656), // &divideontimes -> &divideontimes;
				new Transition (1659, 1660), // &divonx -> &divonx;
				new Transition (1663, 1664), // &DJcy -> &DJcy;
				new Transition (1667, 1668), // &djcy -> &djcy;
				new Transition (1673, 1674), // &dlcorn -> &dlcorn;
				new Transition (1677, 1678), // &dlcrop -> &dlcrop;
				new Transition (1683, 1684), // &dollar -> &dollar;
				new Transition (1687, 1688), // &Dopf -> &Dopf;
				new Transition (1690, 1691), // &dopf -> &dopf;
				new Transition (1692, 1693), // &Dot -> &Dot;
				new Transition (1694, 1695), // &dot -> &dot;
				new Transition (1698, 1699), // &DotDot -> &DotDot;
				new Transition (1701, 1702), // &doteq -> &doteq;
				new Transition (1705, 1706), // &doteqdot -> &doteqdot;
				new Transition (1711, 1712), // &DotEqual -> &DotEqual;
				new Transition (1717, 1718), // &dotminus -> &dotminus;
				new Transition (1722, 1723), // &dotplus -> &dotplus;
				new Transition (1729, 1730), // &dotsquare -> &dotsquare;
				new Transition (1742, 1743), // &doublebarwedge -> &doublebarwedge;
				new Transition (1762, 1763), // &DoubleContourIntegral -> &DoubleContourIntegral;
				new Transition (1766, 1767), // &DoubleDot -> &DoubleDot;
				new Transition (1774, 1775), // &DoubleDownArrow -> &DoubleDownArrow;
				new Transition (1784, 1785), // &DoubleLeftArrow -> &DoubleLeftArrow;
				new Transition (1795, 1796), // &DoubleLeftRightArrow -> &DoubleLeftRightArrow;
				new Transition (1799, 1800), // &DoubleLeftTee -> &DoubleLeftTee;
				new Transition (1812, 1813), // &DoubleLongLeftArrow -> &DoubleLongLeftArrow;
				new Transition (1823, 1824), // &DoubleLongLeftRightArrow -> &DoubleLongLeftRightArrow;
				new Transition (1834, 1835), // &DoubleLongRightArrow -> &DoubleLongRightArrow;
				new Transition (1845, 1846), // &DoubleRightArrow -> &DoubleRightArrow;
				new Transition (1849, 1850), // &DoubleRightTee -> &DoubleRightTee;
				new Transition (1857, 1858), // &DoubleUpArrow -> &DoubleUpArrow;
				new Transition (1867, 1868), // &DoubleUpDownArrow -> &DoubleUpDownArrow;
				new Transition (1879, 1880), // &DoubleVerticalBar -> &DoubleVerticalBar;
				new Transition (1887, 1888), // &DownArrow -> &DownArrow;
				new Transition (1893, 1894), // &Downarrow -> &Downarrow;
				new Transition (1901, 1902), // &downarrow -> &downarrow;
				new Transition (1905, 1906), // &DownArrowBar -> &DownArrowBar;
				new Transition (1913, 1914), // &DownArrowUpArrow -> &DownArrowUpArrow;
				new Transition (1919, 1920), // &DownBreve -> &DownBreve;
				new Transition (1930, 1931), // &downdownarrows -> &downdownarrows;
				new Transition (1942, 1943), // &downharpoonleft -> &downharpoonleft;
				new Transition (1948, 1949), // &downharpoonright -> &downharpoonright;
				new Transition (1964, 1965), // &DownLeftRightVector -> &DownLeftRightVector;
				new Transition (1974, 1975), // &DownLeftTeeVector -> &DownLeftTeeVector;
				new Transition (1981, 1982), // &DownLeftVector -> &DownLeftVector;
				new Transition (1985, 1986), // &DownLeftVectorBar -> &DownLeftVectorBar;
				new Transition (2000, 2001), // &DownRightTeeVector -> &DownRightTeeVector;
				new Transition (2007, 2008), // &DownRightVector -> &DownRightVector;
				new Transition (2011, 2012), // &DownRightVectorBar -> &DownRightVectorBar;
				new Transition (2015, 2016), // &DownTee -> &DownTee;
				new Transition (2021, 2022), // &DownTeeArrow -> &DownTeeArrow;
				new Transition (2029, 2030), // &drbkarow -> &drbkarow;
				new Transition (2034, 2035), // &drcorn -> &drcorn;
				new Transition (2038, 2039), // &drcrop -> &drcrop;
				new Transition (2042, 2043), // &Dscr -> &Dscr;
				new Transition (2046, 2047), // &dscr -> &dscr;
				new Transition (2050, 2051), // &DScy -> &DScy;
				new Transition (2052, 2053), // &dscy -> &dscy;
				new Transition (2055, 2056), // &dsol -> &dsol;
				new Transition (2060, 2061), // &Dstrok -> &Dstrok;
				new Transition (2065, 2066), // &dstrok -> &dstrok;
				new Transition (2070, 2071), // &dtdot -> &dtdot;
				new Transition (2073, 2074), // &dtri -> &dtri;
				new Transition (2075, 2076), // &dtrif -> &dtrif;
				new Transition (2080, 2081), // &duarr -> &duarr;
				new Transition (2084, 2085), // &duhar -> &duhar;
				new Transition (2091, 2092), // &dwangle -> &dwangle;
				new Transition (2095, 2096), // &DZcy -> &DZcy;
				new Transition (2099, 2100), // &dzcy -> &dzcy;
				new Transition (2106, 2107), // &dzigrarr -> &dzigrarr;
				new Transition (2113, 2114), // &Eacute -> &Eacute;
				new Transition (2120, 2121), // &eacute -> &eacute;
				new Transition (2125, 2126), // &easter -> &easter;
				new Transition (2131, 2132), // &Ecaron -> &Ecaron;
				new Transition (2137, 2138), // &ecaron -> &ecaron;
				new Transition (2140, 2141), // &ecir -> &ecir;
				new Transition (2144, 2145), // &Ecirc -> &Ecirc;
				new Transition (2146, 2147), // &ecirc -> &ecirc;
				new Transition (2151, 2152), // &ecolon -> &ecolon;
				new Transition (2153, 2154), // &Ecy -> &Ecy;
				new Transition (2155, 2156), // &ecy -> &ecy;
				new Transition (2160, 2161), // &eDDot -> &eDDot;
				new Transition (2164, 2165), // &Edot -> &Edot;
				new Transition (2167, 2168), // &eDot -> &eDot;
				new Transition (2171, 2172), // &edot -> &edot;
				new Transition (2173, 2174), // &ee -> &ee;
				new Transition (2178, 2179), // &efDot -> &efDot;
				new Transition (2181, 2182), // &Efr -> &Efr;
				new Transition (2183, 2184), // &efr -> &efr;
				new Transition (2185, 2186), // &eg -> &eg;
				new Transition (2191, 2192), // &Egrave -> &Egrave;
				new Transition (2196, 2197), // &egrave -> &egrave;
				new Transition (2198, 2199), // &egs -> &egs;
				new Transition (2202, 2203), // &egsdot -> &egsdot;
				new Transition (2204, 2205), // &el -> &el;
				new Transition (2211, 2212), // &Element -> &Element;
				new Transition (2218, 2219), // &elinters -> &elinters;
				new Transition (2220, 2221), // &ell -> &ell;
				new Transition (2222, 2223), // &els -> &els;
				new Transition (2226, 2227), // &elsdot -> &elsdot;
				new Transition (2231, 2232), // &Emacr -> &Emacr;
				new Transition (2236, 2237), // &emacr -> &emacr;
				new Transition (2240, 2241), // &empty -> &empty;
				new Transition (2244, 2245), // &emptyset -> &emptyset;
				new Transition (2259, 2260), // &EmptySmallSquare -> &EmptySmallSquare;
				new Transition (2261, 2262), // &emptyv -> &emptyv;
				new Transition (2277, 2278), // &EmptyVerySmallSquare -> &EmptyVerySmallSquare;
				new Transition (2280, 2281), // &emsp -> &emsp;
				new Transition (2283, 2284), // &emsp13 -> &emsp13;
				new Transition (2285, 2286), // &emsp14 -> &emsp14;
				new Transition (2288, 2289), // &ENG -> &ENG;
				new Transition (2291, 2292), // &eng -> &eng;
				new Transition (2294, 2295), // &ensp -> &ensp;
				new Transition (2299, 2300), // &Eogon -> &Eogon;
				new Transition (2304, 2305), // &eogon -> &eogon;
				new Transition (2307, 2308), // &Eopf -> &Eopf;
				new Transition (2310, 2311), // &eopf -> &eopf;
				new Transition (2314, 2315), // &epar -> &epar;
				new Transition (2317, 2318), // &eparsl -> &eparsl;
				new Transition (2321, 2322), // &eplus -> &eplus;
				new Transition (2324, 2325), // &epsi -> &epsi;
				new Transition (2331, 2332), // &Epsilon -> &Epsilon;
				new Transition (2335, 2336), // &epsilon -> &epsilon;
				new Transition (2337, 2338), // &epsiv -> &epsiv;
				new Transition (2343, 2344), // &eqcirc -> &eqcirc;
				new Transition (2348, 2349), // &eqcolon -> &eqcolon;
				new Transition (2352, 2353), // &eqsim -> &eqsim;
				new Transition (2360, 2361), // &eqslantgtr -> &eqslantgtr;
				new Transition (2365, 2366), // &eqslantless -> &eqslantless;
				new Transition (2370, 2371), // &Equal -> &Equal;
				new Transition (2375, 2376), // &equals -> &equals;
				new Transition (2381, 2382), // &EqualTilde -> &EqualTilde;
				new Transition (2385, 2386), // &equest -> &equest;
				new Transition (2394, 2395), // &Equilibrium -> &Equilibrium;
				new Transition (2397, 2398), // &equiv -> &equiv;
				new Transition (2400, 2401), // &equivDD -> &equivDD;
				new Transition (2407, 2408), // &eqvparsl -> &eqvparsl;
				new Transition (2412, 2413), // &erarr -> &erarr;
				new Transition (2416, 2417), // &erDot -> &erDot;
				new Transition (2420, 2421), // &Escr -> &Escr;
				new Transition (2424, 2425), // &escr -> &escr;
				new Transition (2428, 2429), // &esdot -> &esdot;
				new Transition (2431, 2432), // &Esim -> &Esim;
				new Transition (2434, 2435), // &esim -> &esim;
				new Transition (2437, 2438), // &Eta -> &Eta;
				new Transition (2440, 2441), // &eta -> &eta;
				new Transition (2443, 2444), // &ETH -> &ETH;
				new Transition (2445, 2446), // &eth -> &eth;
				new Transition (2449, 2450), // &Euml -> &Euml;
				new Transition (2453, 2454), // &euml -> &euml;
				new Transition (2456, 2457), // &euro -> &euro;
				new Transition (2460, 2461), // &excl -> &excl;
				new Transition (2464, 2465), // &exist -> &exist;
				new Transition (2470, 2471), // &Exists -> &Exists;
				new Transition (2480, 2481), // &expectation -> &expectation;
				new Transition (2491, 2492), // &ExponentialE -> &ExponentialE;
				new Transition (2501, 2502), // &exponentiale -> &exponentiale;
				new Transition (2515, 2516), // &fallingdotseq -> &fallingdotseq;
				new Transition (2519, 2520), // &Fcy -> &Fcy;
				new Transition (2522, 2523), // &fcy -> &fcy;
				new Transition (2528, 2529), // &female -> &female;
				new Transition (2534, 2535), // &ffilig -> &ffilig;
				new Transition (2538, 2539), // &fflig -> &fflig;
				new Transition (2542, 2543), // &ffllig -> &ffllig;
				new Transition (2545, 2546), // &Ffr -> &Ffr;
				new Transition (2547, 2548), // &ffr -> &ffr;
				new Transition (2552, 2553), // &filig -> &filig;
				new Transition (2569, 2570), // &FilledSmallSquare -> &FilledSmallSquare;
				new Transition (2585, 2586), // &FilledVerySmallSquare -> &FilledVerySmallSquare;
				new Transition (2590, 2591), // &fjlig -> &fjlig;
				new Transition (2594, 2595), // &flat -> &flat;
				new Transition (2598, 2599), // &fllig -> &fllig;
				new Transition (2602, 2603), // &fltns -> &fltns;
				new Transition (2606, 2607), // &fnof -> &fnof;
				new Transition (2610, 2611), // &Fopf -> &Fopf;
				new Transition (2614, 2615), // &fopf -> &fopf;
				new Transition (2619, 2620), // &ForAll -> &ForAll;
				new Transition (2624, 2625), // &forall -> &forall;
				new Transition (2626, 2627), // &fork -> &fork;
				new Transition (2628, 2629), // &forkv -> &forkv;
				new Transition (2637, 2638), // &Fouriertrf -> &Fouriertrf;
				new Transition (2645, 2646), // &fpartint -> &fpartint;
				new Transition (2651, 2652), // &frac12 -> &frac12;
				new Transition (2653, 2654), // &frac13 -> &frac13;
				new Transition (2655, 2656), // &frac14 -> &frac14;
				new Transition (2657, 2658), // &frac15 -> &frac15;
				new Transition (2659, 2660), // &frac16 -> &frac16;
				new Transition (2661, 2662), // &frac18 -> &frac18;
				new Transition (2664, 2665), // &frac23 -> &frac23;
				new Transition (2666, 2667), // &frac25 -> &frac25;
				new Transition (2669, 2670), // &frac34 -> &frac34;
				new Transition (2671, 2672), // &frac35 -> &frac35;
				new Transition (2673, 2674), // &frac38 -> &frac38;
				new Transition (2676, 2677), // &frac45 -> &frac45;
				new Transition (2679, 2680), // &frac56 -> &frac56;
				new Transition (2681, 2682), // &frac58 -> &frac58;
				new Transition (2684, 2685), // &frac78 -> &frac78;
				new Transition (2687, 2688), // &frasl -> &frasl;
				new Transition (2691, 2692), // &frown -> &frown;
				new Transition (2695, 2696), // &Fscr -> &Fscr;
				new Transition (2699, 2700), // &fscr -> &fscr;
				new Transition (2706, 2707), // &gacute -> &gacute;
				new Transition (2712, 2713), // &Gamma -> &Gamma;
				new Transition (2716, 2717), // &gamma -> &gamma;
				new Transition (2718, 2719), // &Gammad -> &Gammad;
				new Transition (2720, 2721), // &gammad -> &gammad;
				new Transition (2722, 2723), // &gap -> &gap;
				new Transition (2728, 2729), // &Gbreve -> &Gbreve;
				new Transition (2734, 2735), // &gbreve -> &gbreve;
				new Transition (2740, 2741), // &Gcedil -> &Gcedil;
				new Transition (2744, 2745), // &Gcirc -> &Gcirc;
				new Transition (2749, 2750), // &gcirc -> &gcirc;
				new Transition (2751, 2752), // &Gcy -> &Gcy;
				new Transition (2753, 2754), // &gcy -> &gcy;
				new Transition (2757, 2758), // &Gdot -> &Gdot;
				new Transition (2761, 2762), // &gdot -> &gdot;
				new Transition (2763, 2764), // &gE -> &gE;
				new Transition (2765, 2766), // &ge -> &ge;
				new Transition (2767, 2768), // &gEl -> &gEl;
				new Transition (2769, 2770), // &gel -> &gel;
				new Transition (2771, 2772), // &geq -> &geq;
				new Transition (2773, 2774), // &geqq -> &geqq;
				new Transition (2779, 2780), // &geqslant -> &geqslant;
				new Transition (2781, 2782), // &ges -> &ges;
				new Transition (2784, 2785), // &gescc -> &gescc;
				new Transition (2788, 2789), // &gesdot -> &gesdot;
				new Transition (2790, 2791), // &gesdoto -> &gesdoto;
				new Transition (2792, 2793), // &gesdotol -> &gesdotol;
				new Transition (2794, 2795), // &gesl -> &gesl;
				new Transition (2797, 2798), // &gesles -> &gesles;
				new Transition (2800, 2801), // &Gfr -> &Gfr;
				new Transition (2803, 2804), // &gfr -> &gfr;
				new Transition (2805, 2806), // &Gg -> &Gg;
				new Transition (2807, 2808), // &gg -> &gg;
				new Transition (2809, 2810), // &ggg -> &ggg;
				new Transition (2814, 2815), // &gimel -> &gimel;
				new Transition (2818, 2819), // &GJcy -> &GJcy;
				new Transition (2822, 2823), // &gjcy -> &gjcy;
				new Transition (2824, 2825), // &gl -> &gl;
				new Transition (2826, 2827), // &gla -> &gla;
				new Transition (2828, 2829), // &glE -> &glE;
				new Transition (2830, 2831), // &glj -> &glj;
				new Transition (2834, 2835), // &gnap -> &gnap;
				new Transition (2839, 2840), // &gnapprox -> &gnapprox;
				new Transition (2841, 2842), // &gnE -> &gnE;
				new Transition (2843, 2844), // &gne -> &gne;
				new Transition (2845, 2846), // &gneq -> &gneq;
				new Transition (2847, 2848), // &gneqq -> &gneqq;
				new Transition (2851, 2852), // &gnsim -> &gnsim;
				new Transition (2855, 2856), // &Gopf -> &Gopf;
				new Transition (2859, 2860), // &gopf -> &gopf;
				new Transition (2864, 2865), // &grave -> &grave;
				new Transition (2876, 2877), // &GreaterEqual -> &GreaterEqual;
				new Transition (2881, 2882), // &GreaterEqualLess -> &GreaterEqualLess;
				new Transition (2891, 2892), // &GreaterFullEqual -> &GreaterFullEqual;
				new Transition (2899, 2900), // &GreaterGreater -> &GreaterGreater;
				new Transition (2904, 2905), // &GreaterLess -> &GreaterLess;
				new Transition (2915, 2916), // &GreaterSlantEqual -> &GreaterSlantEqual;
				new Transition (2921, 2922), // &GreaterTilde -> &GreaterTilde;
				new Transition (2925, 2926), // &Gscr -> &Gscr;
				new Transition (2929, 2930), // &gscr -> &gscr;
				new Transition (2932, 2933), // &gsim -> &gsim;
				new Transition (2934, 2935), // &gsime -> &gsime;
				new Transition (2936, 2937), // &gsiml -> &gsiml;
				new Transition (2938, 2939), // &GT -> &GT;
				new Transition (2940, 2941), // &Gt -> &Gt;
				new Transition (2942, 2943), // &gt -> &gt;
				new Transition (2945, 2946), // &gtcc -> &gtcc;
				new Transition (2948, 2949), // &gtcir -> &gtcir;
				new Transition (2952, 2953), // &gtdot -> &gtdot;
				new Transition (2957, 2958), // &gtlPar -> &gtlPar;
				new Transition (2963, 2964), // &gtquest -> &gtquest;
				new Transition (2971, 2972), // &gtrapprox -> &gtrapprox;
				new Transition (2974, 2975), // &gtrarr -> &gtrarr;
				new Transition (2978, 2979), // &gtrdot -> &gtrdot;
				new Transition (2985, 2986), // &gtreqless -> &gtreqless;
				new Transition (2991, 2992), // &gtreqqless -> &gtreqqless;
				new Transition (2996, 2997), // &gtrless -> &gtrless;
				new Transition (3000, 3001), // &gtrsim -> &gtrsim;
				new Transition (3009, 3010), // &gvertneqq -> &gvertneqq;
				new Transition (3012, 3013), // &gvnE -> &gvnE;
				new Transition (3018, 3019), // &Hacek -> &Hacek;
				new Transition (3025, 3026), // &hairsp -> &hairsp;
				new Transition (3028, 3029), // &half -> &half;
				new Transition (3033, 3034), // &hamilt -> &hamilt;
				new Transition (3039, 3040), // &HARDcy -> &HARDcy;
				new Transition (3044, 3045), // &hardcy -> &hardcy;
				new Transition (3048, 3049), // &hArr -> &hArr;
				new Transition (3050, 3051), // &harr -> &harr;
				new Transition (3054, 3055), // &harrcir -> &harrcir;
				new Transition (3056, 3057), // &harrw -> &harrw;
				new Transition (3058, 3059), // &Hat -> &Hat;
				new Transition (3062, 3063), // &hbar -> &hbar;
				new Transition (3067, 3068), // &Hcirc -> &Hcirc;
				new Transition (3072, 3073), // &hcirc -> &hcirc;
				new Transition (3078, 3079), // &hearts -> &hearts;
				new Transition (3082, 3083), // &heartsuit -> &heartsuit;
				new Transition (3087, 3088), // &hellip -> &hellip;
				new Transition (3092, 3093), // &hercon -> &hercon;
				new Transition (3095, 3096), // &Hfr -> &Hfr;
				new Transition (3098, 3099), // &hfr -> &hfr;
				new Transition (3110, 3111), // &HilbertSpace -> &HilbertSpace;
				new Transition (3118, 3119), // &hksearow -> &hksearow;
				new Transition (3124, 3125), // &hkswarow -> &hkswarow;
				new Transition (3129, 3130), // &hoarr -> &hoarr;
				new Transition (3134, 3135), // &homtht -> &homtht;
				new Transition (3146, 3147), // &hookleftarrow -> &hookleftarrow;
				new Transition (3157, 3158), // &hookrightarrow -> &hookrightarrow;
				new Transition (3161, 3162), // &Hopf -> &Hopf;
				new Transition (3164, 3165), // &hopf -> &hopf;
				new Transition (3169, 3170), // &horbar -> &horbar;
				new Transition (3182, 3183), // &HorizontalLine -> &HorizontalLine;
				new Transition (3186, 3187), // &Hscr -> &Hscr;
				new Transition (3190, 3191), // &hscr -> &hscr;
				new Transition (3195, 3196), // &hslash -> &hslash;
				new Transition (3200, 3201), // &Hstrok -> &Hstrok;
				new Transition (3205, 3206), // &hstrok -> &hstrok;
				new Transition (3217, 3218), // &HumpDownHump -> &HumpDownHump;
				new Transition (3223, 3224), // &HumpEqual -> &HumpEqual;
				new Transition (3229, 3230), // &hybull -> &hybull;
				new Transition (3234, 3235), // &hyphen -> &hyphen;
				new Transition (3241, 3242), // &Iacute -> &Iacute;
				new Transition (3248, 3249), // &iacute -> &iacute;
				new Transition (3250, 3251), // &ic -> &ic;
				new Transition (3255, 3256), // &Icirc -> &Icirc;
				new Transition (3259, 3260), // &icirc -> &icirc;
				new Transition (3261, 3262), // &Icy -> &Icy;
				new Transition (3263, 3264), // &icy -> &icy;
				new Transition (3267, 3268), // &Idot -> &Idot;
				new Transition (3271, 3272), // &IEcy -> &IEcy;
				new Transition (3275, 3276), // &iecy -> &iecy;
				new Transition (3279, 3280), // &iexcl -> &iexcl;
				new Transition (3282, 3283), // &iff -> &iff;
				new Transition (3285, 3286), // &Ifr -> &Ifr;
				new Transition (3287, 3288), // &ifr -> &ifr;
				new Transition (3293, 3294), // &Igrave -> &Igrave;
				new Transition (3299, 3300), // &igrave -> &igrave;
				new Transition (3301, 3302), // &ii -> &ii;
				new Transition (3306, 3307), // &iiiint -> &iiiint;
				new Transition (3309, 3310), // &iiint -> &iiint;
				new Transition (3314, 3315), // &iinfin -> &iinfin;
				new Transition (3318, 3319), // &iiota -> &iiota;
				new Transition (3323, 3324), // &IJlig -> &IJlig;
				new Transition (3328, 3329), // &ijlig -> &ijlig;
				new Transition (3330, 3331), // &Im -> &Im;
				new Transition (3334, 3335), // &Imacr -> &Imacr;
				new Transition (3339, 3340), // &imacr -> &imacr;
				new Transition (3342, 3343), // &image -> &image;
				new Transition (3350, 3351), // &ImaginaryI -> &ImaginaryI;
				new Transition (3355, 3356), // &imagline -> &imagline;
				new Transition (3360, 3361), // &imagpart -> &imagpart;
				new Transition (3363, 3364), // &imath -> &imath;
				new Transition (3366, 3367), // &imof -> &imof;
				new Transition (3370, 3371), // &imped -> &imped;
				new Transition (3376, 3377), // &Implies -> &Implies;
				new Transition (3378, 3379), // &in -> &in;
				new Transition (3383, 3384), // &incare -> &incare;
				new Transition (3387, 3388), // &infin -> &infin;
				new Transition (3391, 3392), // &infintie -> &infintie;
				new Transition (3396, 3397), // &inodot -> &inodot;
				new Transition (3399, 3400), // &Int -> &Int;
				new Transition (3401, 3402), // &int -> &int;
				new Transition (3405, 3406), // &intcal -> &intcal;
				new Transition (3411, 3412), // &integers -> &integers;
				new Transition (3417, 3418), // &Integral -> &Integral;
				new Transition (3422, 3423), // &intercal -> &intercal;
				new Transition (3431, 3432), // &Intersection -> &Intersection;
				new Transition (3437, 3438), // &intlarhk -> &intlarhk;
				new Transition (3442, 3443), // &intprod -> &intprod;
				new Transition (3455, 3456), // &InvisibleComma -> &InvisibleComma;
				new Transition (3461, 3462), // &InvisibleTimes -> &InvisibleTimes;
				new Transition (3465, 3466), // &IOcy -> &IOcy;
				new Transition (3469, 3470), // &iocy -> &iocy;
				new Transition (3474, 3475), // &Iogon -> &Iogon;
				new Transition (3478, 3479), // &iogon -> &iogon;
				new Transition (3481, 3482), // &Iopf -> &Iopf;
				new Transition (3484, 3485), // &iopf -> &iopf;
				new Transition (3487, 3488), // &Iota -> &Iota;
				new Transition (3490, 3491), // &iota -> &iota;
				new Transition (3495, 3496), // &iprod -> &iprod;
				new Transition (3501, 3502), // &iquest -> &iquest;
				new Transition (3505, 3506), // &Iscr -> &Iscr;
				new Transition (3509, 3510), // &iscr -> &iscr;
				new Transition (3512, 3513), // &isin -> &isin;
				new Transition (3516, 3517), // &isindot -> &isindot;
				new Transition (3518, 3519), // &isinE -> &isinE;
				new Transition (3520, 3521), // &isins -> &isins;
				new Transition (3522, 3523), // &isinsv -> &isinsv;
				new Transition (3524, 3525), // &isinv -> &isinv;
				new Transition (3526, 3527), // &it -> &it;
				new Transition (3532, 3533), // &Itilde -> &Itilde;
				new Transition (3537, 3538), // &itilde -> &itilde;
				new Transition (3542, 3543), // &Iukcy -> &Iukcy;
				new Transition (3547, 3548), // &iukcy -> &iukcy;
				new Transition (3550, 3551), // &Iuml -> &Iuml;
				new Transition (3553, 3554), // &iuml -> &iuml;
				new Transition (3559, 3560), // &Jcirc -> &Jcirc;
				new Transition (3565, 3566), // &jcirc -> &jcirc;
				new Transition (3567, 3568), // &Jcy -> &Jcy;
				new Transition (3569, 3570), // &jcy -> &jcy;
				new Transition (3572, 3573), // &Jfr -> &Jfr;
				new Transition (3575, 3576), // &jfr -> &jfr;
				new Transition (3580, 3581), // &jmath -> &jmath;
				new Transition (3584, 3585), // &Jopf -> &Jopf;
				new Transition (3588, 3589), // &jopf -> &jopf;
				new Transition (3592, 3593), // &Jscr -> &Jscr;
				new Transition (3596, 3597), // &jscr -> &jscr;
				new Transition (3601, 3602), // &Jsercy -> &Jsercy;
				new Transition (3606, 3607), // &jsercy -> &jsercy;
				new Transition (3611, 3612), // &Jukcy -> &Jukcy;
				new Transition (3616, 3617), // &jukcy -> &jukcy;
				new Transition (3622, 3623), // &Kappa -> &Kappa;
				new Transition (3628, 3629), // &kappa -> &kappa;
				new Transition (3630, 3631), // &kappav -> &kappav;
				new Transition (3636, 3637), // &Kcedil -> &Kcedil;
				new Transition (3642, 3643), // &kcedil -> &kcedil;
				new Transition (3644, 3645), // &Kcy -> &Kcy;
				new Transition (3646, 3647), // &kcy -> &kcy;
				new Transition (3649, 3650), // &Kfr -> &Kfr;
				new Transition (3652, 3653), // &kfr -> &kfr;
				new Transition (3658, 3659), // &kgreen -> &kgreen;
				new Transition (3662, 3663), // &KHcy -> &KHcy;
				new Transition (3666, 3667), // &khcy -> &khcy;
				new Transition (3670, 3671), // &KJcy -> &KJcy;
				new Transition (3674, 3675), // &kjcy -> &kjcy;
				new Transition (3678, 3679), // &Kopf -> &Kopf;
				new Transition (3682, 3683), // &kopf -> &kopf;
				new Transition (3686, 3687), // &Kscr -> &Kscr;
				new Transition (3690, 3691), // &kscr -> &kscr;
				new Transition (3696, 3697), // &lAarr -> &lAarr;
				new Transition (3703, 3704), // &Lacute -> &Lacute;
				new Transition (3709, 3710), // &lacute -> &lacute;
				new Transition (3716, 3717), // &laemptyv -> &laemptyv;
				new Transition (3721, 3722), // &lagran -> &lagran;
				new Transition (3726, 3727), // &Lambda -> &Lambda;
				new Transition (3731, 3732), // &lambda -> &lambda;
				new Transition (3734, 3735), // &Lang -> &Lang;
				new Transition (3737, 3738), // &lang -> &lang;
				new Transition (3739, 3740), // &langd -> &langd;
				new Transition (3742, 3743), // &langle -> &langle;
				new Transition (3744, 3745), // &lap -> &lap;
				new Transition (3753, 3754), // &Laplacetrf -> &Laplacetrf;
				new Transition (3757, 3758), // &laquo -> &laquo;
				new Transition (3760, 3761), // &Larr -> &Larr;
				new Transition (3763, 3764), // &lArr -> &lArr;
				new Transition (3766, 3767), // &larr -> &larr;
				new Transition (3768, 3769), // &larrb -> &larrb;
				new Transition (3771, 3772), // &larrbfs -> &larrbfs;
				new Transition (3774, 3775), // &larrfs -> &larrfs;
				new Transition (3777, 3778), // &larrhk -> &larrhk;
				new Transition (3780, 3781), // &larrlp -> &larrlp;
				new Transition (3783, 3784), // &larrpl -> &larrpl;
				new Transition (3787, 3788), // &larrsim -> &larrsim;
				new Transition (3790, 3791), // &larrtl -> &larrtl;
				new Transition (3792, 3793), // &lat -> &lat;
				new Transition (3797, 3798), // &lAtail -> &lAtail;
				new Transition (3801, 3802), // &latail -> &latail;
				new Transition (3803, 3804), // &late -> &late;
				new Transition (3805, 3806), // &lates -> &lates;
				new Transition (3810, 3811), // &lBarr -> &lBarr;
				new Transition (3815, 3816), // &lbarr -> &lbarr;
				new Transition (3819, 3820), // &lbbrk -> &lbbrk;
				new Transition (3824, 3825), // &lbrace -> &lbrace;
				new Transition (3826, 3827), // &lbrack -> &lbrack;
				new Transition (3829, 3830), // &lbrke -> &lbrke;
				new Transition (3833, 3834), // &lbrksld -> &lbrksld;
				new Transition (3835, 3836), // &lbrkslu -> &lbrkslu;
				new Transition (3841, 3842), // &Lcaron -> &Lcaron;
				new Transition (3847, 3848), // &lcaron -> &lcaron;
				new Transition (3852, 3853), // &Lcedil -> &Lcedil;
				new Transition (3857, 3858), // &lcedil -> &lcedil;
				new Transition (3860, 3861), // &lceil -> &lceil;
				new Transition (3863, 3864), // &lcub -> &lcub;
				new Transition (3865, 3866), // &Lcy -> &Lcy;
				new Transition (3867, 3868), // &lcy -> &lcy;
				new Transition (3871, 3872), // &ldca -> &ldca;
				new Transition (3875, 3876), // &ldquo -> &ldquo;
				new Transition (3877, 3878), // &ldquor -> &ldquor;
				new Transition (3883, 3884), // &ldrdhar -> &ldrdhar;
				new Transition (3889, 3890), // &ldrushar -> &ldrushar;
				new Transition (3892, 3893), // &ldsh -> &ldsh;
				new Transition (3894, 3895), // &lE -> &lE;
				new Transition (3896, 3897), // &le -> &le;
				new Transition (3912, 3913), // &LeftAngleBracket -> &LeftAngleBracket;
				new Transition (3917, 3918), // &LeftArrow -> &LeftArrow;
				new Transition (3923, 3924), // &Leftarrow -> &Leftarrow;
				new Transition (3931, 3932), // &leftarrow -> &leftarrow;
				new Transition (3935, 3936), // &LeftArrowBar -> &LeftArrowBar;
				new Transition (3946, 3947), // &LeftArrowRightArrow -> &LeftArrowRightArrow;
				new Transition (3951, 3952), // &leftarrowtail -> &leftarrowtail;
				new Transition (3959, 3960), // &LeftCeiling -> &LeftCeiling;
				new Transition (3973, 3974), // &LeftDoubleBracket -> &LeftDoubleBracket;
				new Transition (3985, 3986), // &LeftDownTeeVector -> &LeftDownTeeVector;
				new Transition (3992, 3993), // &LeftDownVector -> &LeftDownVector;
				new Transition (3996, 3997), // &LeftDownVectorBar -> &LeftDownVectorBar;
				new Transition (4002, 4003), // &LeftFloor -> &LeftFloor;
				new Transition (4014, 4015), // &leftharpoondown -> &leftharpoondown;
				new Transition (4017, 4018), // &leftharpoonup -> &leftharpoonup;
				new Transition (4028, 4029), // &leftleftarrows -> &leftleftarrows;
				new Transition (4039, 4040), // &LeftRightArrow -> &LeftRightArrow;
				new Transition (4050, 4051), // &Leftrightarrow -> &Leftrightarrow;
				new Transition (4061, 4062), // &leftrightarrow -> &leftrightarrow;
				new Transition (4063, 4064), // &leftrightarrows -> &leftrightarrows;
				new Transition (4072, 4073), // &leftrightharpoons -> &leftrightharpoons;
				new Transition (4083, 4084), // &leftrightsquigarrow -> &leftrightsquigarrow;
				new Transition (4090, 4091), // &LeftRightVector -> &LeftRightVector;
				new Transition (4094, 4095), // &LeftTee -> &LeftTee;
				new Transition (4100, 4101), // &LeftTeeArrow -> &LeftTeeArrow;
				new Transition (4107, 4108), // &LeftTeeVector -> &LeftTeeVector;
				new Transition (4118, 4119), // &leftthreetimes -> &leftthreetimes;
				new Transition (4126, 4127), // &LeftTriangle -> &LeftTriangle;
				new Transition (4130, 4131), // &LeftTriangleBar -> &LeftTriangleBar;
				new Transition (4136, 4137), // &LeftTriangleEqual -> &LeftTriangleEqual;
				new Transition (4149, 4150), // &LeftUpDownVector -> &LeftUpDownVector;
				new Transition (4159, 4160), // &LeftUpTeeVector -> &LeftUpTeeVector;
				new Transition (4166, 4167), // &LeftUpVector -> &LeftUpVector;
				new Transition (4170, 4171), // &LeftUpVectorBar -> &LeftUpVectorBar;
				new Transition (4177, 4178), // &LeftVector -> &LeftVector;
				new Transition (4181, 4182), // &LeftVectorBar -> &LeftVectorBar;
				new Transition (4183, 4184), // &lEg -> &lEg;
				new Transition (4185, 4186), // &leg -> &leg;
				new Transition (4187, 4188), // &leq -> &leq;
				new Transition (4189, 4190), // &leqq -> &leqq;
				new Transition (4195, 4196), // &leqslant -> &leqslant;
				new Transition (4197, 4198), // &les -> &les;
				new Transition (4200, 4201), // &lescc -> &lescc;
				new Transition (4204, 4205), // &lesdot -> &lesdot;
				new Transition (4206, 4207), // &lesdoto -> &lesdoto;
				new Transition (4208, 4209), // &lesdotor -> &lesdotor;
				new Transition (4210, 4211), // &lesg -> &lesg;
				new Transition (4213, 4214), // &lesges -> &lesges;
				new Transition (4221, 4222), // &lessapprox -> &lessapprox;
				new Transition (4225, 4226), // &lessdot -> &lessdot;
				new Transition (4231, 4232), // &lesseqgtr -> &lesseqgtr;
				new Transition (4236, 4237), // &lesseqqgtr -> &lesseqqgtr;
				new Transition (4251, 4252), // &LessEqualGreater -> &LessEqualGreater;
				new Transition (4261, 4262), // &LessFullEqual -> &LessFullEqual;
				new Transition (4269, 4270), // &LessGreater -> &LessGreater;
				new Transition (4273, 4274), // &lessgtr -> &lessgtr;
				new Transition (4278, 4279), // &LessLess -> &LessLess;
				new Transition (4282, 4283), // &lesssim -> &lesssim;
				new Transition (4293, 4294), // &LessSlantEqual -> &LessSlantEqual;
				new Transition (4299, 4300), // &LessTilde -> &LessTilde;
				new Transition (4305, 4306), // &lfisht -> &lfisht;
				new Transition (4310, 4311), // &lfloor -> &lfloor;
				new Transition (4313, 4314), // &Lfr -> &Lfr;
				new Transition (4315, 4316), // &lfr -> &lfr;
				new Transition (4317, 4318), // &lg -> &lg;
				new Transition (4319, 4320), // &lgE -> &lgE;
				new Transition (4323, 4324), // &lHar -> &lHar;
				new Transition (4328, 4329), // &lhard -> &lhard;
				new Transition (4330, 4331), // &lharu -> &lharu;
				new Transition (4332, 4333), // &lharul -> &lharul;
				new Transition (4336, 4337), // &lhblk -> &lhblk;
				new Transition (4340, 4341), // &LJcy -> &LJcy;
				new Transition (4344, 4345), // &ljcy -> &ljcy;
				new Transition (4346, 4347), // &Ll -> &Ll;
				new Transition (4348, 4349), // &ll -> &ll;
				new Transition (4352, 4353), // &llarr -> &llarr;
				new Transition (4359, 4360), // &llcorner -> &llcorner;
				new Transition (4368, 4369), // &Lleftarrow -> &Lleftarrow;
				new Transition (4373, 4374), // &llhard -> &llhard;
				new Transition (4377, 4378), // &lltri -> &lltri;
				new Transition (4383, 4384), // &Lmidot -> &Lmidot;
				new Transition (4389, 4390), // &lmidot -> &lmidot;
				new Transition (4394, 4395), // &lmoust -> &lmoust;
				new Transition (4399, 4400), // &lmoustache -> &lmoustache;
				new Transition (4403, 4404), // &lnap -> &lnap;
				new Transition (4408, 4409), // &lnapprox -> &lnapprox;
				new Transition (4410, 4411), // &lnE -> &lnE;
				new Transition (4412, 4413), // &lne -> &lne;
				new Transition (4414, 4415), // &lneq -> &lneq;
				new Transition (4416, 4417), // &lneqq -> &lneqq;
				new Transition (4420, 4421), // &lnsim -> &lnsim;
				new Transition (4425, 4426), // &loang -> &loang;
				new Transition (4428, 4429), // &loarr -> &loarr;
				new Transition (4432, 4433), // &lobrk -> &lobrk;
				new Transition (4445, 4446), // &LongLeftArrow -> &LongLeftArrow;
				new Transition (4455, 4456), // &Longleftarrow -> &Longleftarrow;
				new Transition (4467, 4468), // &longleftarrow -> &longleftarrow;
				new Transition (4478, 4479), // &LongLeftRightArrow -> &LongLeftRightArrow;
				new Transition (4489, 4490), // &Longleftrightarrow -> &Longleftrightarrow;
				new Transition (4500, 4501), // &longleftrightarrow -> &longleftrightarrow;
				new Transition (4507, 4508), // &longmapsto -> &longmapsto;
				new Transition (4518, 4519), // &LongRightArrow -> &LongRightArrow;
				new Transition (4529, 4530), // &Longrightarrow -> &Longrightarrow;
				new Transition (4540, 4541), // &longrightarrow -> &longrightarrow;
				new Transition (4552, 4553), // &looparrowleft -> &looparrowleft;
				new Transition (4558, 4559), // &looparrowright -> &looparrowright;
				new Transition (4562, 4563), // &lopar -> &lopar;
				new Transition (4565, 4566), // &Lopf -> &Lopf;
				new Transition (4567, 4568), // &lopf -> &lopf;
				new Transition (4571, 4572), // &loplus -> &loplus;
				new Transition (4577, 4578), // &lotimes -> &lotimes;
				new Transition (4582, 4583), // &lowast -> &lowast;
				new Transition (4586, 4587), // &lowbar -> &lowbar;
				new Transition (4599, 4600), // &LowerLeftArrow -> &LowerLeftArrow;
				new Transition (4610, 4611), // &LowerRightArrow -> &LowerRightArrow;
				new Transition (4612, 4613), // &loz -> &loz;
				new Transition (4617, 4618), // &lozenge -> &lozenge;
				new Transition (4619, 4620), // &lozf -> &lozf;
				new Transition (4623, 4624), // &lpar -> &lpar;
				new Transition (4626, 4627), // &lparlt -> &lparlt;
				new Transition (4631, 4632), // &lrarr -> &lrarr;
				new Transition (4638, 4639), // &lrcorner -> &lrcorner;
				new Transition (4642, 4643), // &lrhar -> &lrhar;
				new Transition (4644, 4645), // &lrhard -> &lrhard;
				new Transition (4646, 4647), // &lrm -> &lrm;
				new Transition (4650, 4651), // &lrtri -> &lrtri;
				new Transition (4656, 4657), // &lsaquo -> &lsaquo;
				new Transition (4660, 4661), // &Lscr -> &Lscr;
				new Transition (4663, 4664), // &lscr -> &lscr;
				new Transition (4665, 4666), // &Lsh -> &Lsh;
				new Transition (4667, 4668), // &lsh -> &lsh;
				new Transition (4670, 4671), // &lsim -> &lsim;
				new Transition (4672, 4673), // &lsime -> &lsime;
				new Transition (4674, 4675), // &lsimg -> &lsimg;
				new Transition (4677, 4678), // &lsqb -> &lsqb;
				new Transition (4680, 4681), // &lsquo -> &lsquo;
				new Transition (4682, 4683), // &lsquor -> &lsquor;
				new Transition (4687, 4688), // &Lstrok -> &Lstrok;
				new Transition (4692, 4693), // &lstrok -> &lstrok;
				new Transition (4694, 4695), // &LT -> &LT;
				new Transition (4696, 4697), // &Lt -> &Lt;
				new Transition (4698, 4699), // &lt -> &lt;
				new Transition (4701, 4702), // &ltcc -> &ltcc;
				new Transition (4704, 4705), // &ltcir -> &ltcir;
				new Transition (4708, 4709), // &ltdot -> &ltdot;
				new Transition (4713, 4714), // &lthree -> &lthree;
				new Transition (4718, 4719), // &ltimes -> &ltimes;
				new Transition (4723, 4724), // &ltlarr -> &ltlarr;
				new Transition (4729, 4730), // &ltquest -> &ltquest;
				new Transition (4732, 4733), // &ltri -> &ltri;
				new Transition (4734, 4735), // &ltrie -> &ltrie;
				new Transition (4736, 4737), // &ltrif -> &ltrif;
				new Transition (4740, 4741), // &ltrPar -> &ltrPar;
				new Transition (4748, 4749), // &lurdshar -> &lurdshar;
				new Transition (4753, 4754), // &luruhar -> &luruhar;
				new Transition (4762, 4763), // &lvertneqq -> &lvertneqq;
				new Transition (4765, 4766), // &lvnE -> &lvnE;
				new Transition (4770, 4771), // &macr -> &macr;
				new Transition (4773, 4774), // &male -> &male;
				new Transition (4775, 4776), // &malt -> &malt;
				new Transition (4779, 4780), // &maltese -> &maltese;
				new Transition (4783, 4784), // &Map -> &Map;
				new Transition (4785, 4786), // &map -> &map;
				new Transition (4789, 4790), // &mapsto -> &mapsto;
				new Transition (4794, 4795), // &mapstodown -> &mapstodown;
				new Transition (4799, 4800), // &mapstoleft -> &mapstoleft;
				new Transition (4802, 4803), // &mapstoup -> &mapstoup;
				new Transition (4807, 4808), // &marker -> &marker;
				new Transition (4813, 4814), // &mcomma -> &mcomma;
				new Transition (4816, 4817), // &Mcy -> &Mcy;
				new Transition (4818, 4819), // &mcy -> &mcy;
				new Transition (4823, 4824), // &mdash -> &mdash;
				new Transition (4828, 4829), // &mDDot -> &mDDot;
				new Transition (4841, 4842), // &measuredangle -> &measuredangle;
				new Transition (4852, 4853), // &MediumSpace -> &MediumSpace;
				new Transition (4860, 4861), // &Mellintrf -> &Mellintrf;
				new Transition (4863, 4864), // &Mfr -> &Mfr;
				new Transition (4866, 4867), // &mfr -> &mfr;
				new Transition (4869, 4870), // &mho -> &mho;
				new Transition (4874, 4875), // &micro -> &micro;
				new Transition (4876, 4877), // &mid -> &mid;
				new Transition (4880, 4881), // &midast -> &midast;
				new Transition (4884, 4885), // &midcir -> &midcir;
				new Transition (4888, 4889), // &middot -> &middot;
				new Transition (4892, 4893), // &minus -> &minus;
				new Transition (4894, 4895), // &minusb -> &minusb;
				new Transition (4896, 4897), // &minusd -> &minusd;
				new Transition (4898, 4899), // &minusdu -> &minusdu;
				new Transition (4907, 4908), // &MinusPlus -> &MinusPlus;
				new Transition (4911, 4912), // &mlcp -> &mlcp;
				new Transition (4914, 4915), // &mldr -> &mldr;
				new Transition (4920, 4921), // &mnplus -> &mnplus;
				new Transition (4926, 4927), // &models -> &models;
				new Transition (4930, 4931), // &Mopf -> &Mopf;
				new Transition (4933, 4934), // &mopf -> &mopf;
				new Transition (4935, 4936), // &mp -> &mp;
				new Transition (4939, 4940), // &Mscr -> &Mscr;
				new Transition (4943, 4944), // &mscr -> &mscr;
				new Transition (4948, 4949), // &mstpos -> &mstpos;
				new Transition (4950, 4951), // &Mu -> &Mu;
				new Transition (4952, 4953), // &mu -> &mu;
				new Transition (4959, 4960), // &multimap -> &multimap;
				new Transition (4963, 4964), // &mumap -> &mumap;
				new Transition (4969, 4970), // &nabla -> &nabla;
				new Transition (4976, 4977), // &Nacute -> &Nacute;
				new Transition (4981, 4982), // &nacute -> &nacute;
				new Transition (4984, 4985), // &nang -> &nang;
				new Transition (4986, 4987), // &nap -> &nap;
				new Transition (4988, 4989), // &napE -> &napE;
				new Transition (4991, 4992), // &napid -> &napid;
				new Transition (4994, 4995), // &napos -> &napos;
				new Transition (4999, 5000), // &napprox -> &napprox;
				new Transition (5003, 5004), // &natur -> &natur;
				new Transition (5006, 5007), // &natural -> &natural;
				new Transition (5008, 5009), // &naturals -> &naturals;
				new Transition (5012, 5013), // &nbsp -> &nbsp;
				new Transition (5016, 5017), // &nbump -> &nbump;
				new Transition (5018, 5019), // &nbumpe -> &nbumpe;
				new Transition (5022, 5023), // &ncap -> &ncap;
				new Transition (5028, 5029), // &Ncaron -> &Ncaron;
				new Transition (5032, 5033), // &ncaron -> &ncaron;
				new Transition (5037, 5038), // &Ncedil -> &Ncedil;
				new Transition (5042, 5043), // &ncedil -> &ncedil;
				new Transition (5046, 5047), // &ncong -> &ncong;
				new Transition (5050, 5051), // &ncongdot -> &ncongdot;
				new Transition (5053, 5054), // &ncup -> &ncup;
				new Transition (5055, 5056), // &Ncy -> &Ncy;
				new Transition (5057, 5058), // &ncy -> &ncy;
				new Transition (5062, 5063), // &ndash -> &ndash;
				new Transition (5064, 5065), // &ne -> &ne;
				new Transition (5069, 5070), // &nearhk -> &nearhk;
				new Transition (5073, 5074), // &neArr -> &neArr;
				new Transition (5075, 5076), // &nearr -> &nearr;
				new Transition (5078, 5079), // &nearrow -> &nearrow;
				new Transition (5082, 5083), // &nedot -> &nedot;
				new Transition (5101, 5102), // &NegativeMediumSpace -> &NegativeMediumSpace;
				new Transition (5112, 5113), // &NegativeThickSpace -> &NegativeThickSpace;
				new Transition (5119, 5120), // &NegativeThinSpace -> &NegativeThinSpace;
				new Transition (5133, 5134), // &NegativeVeryThinSpace -> &NegativeVeryThinSpace;
				new Transition (5138, 5139), // &nequiv -> &nequiv;
				new Transition (5143, 5144), // &nesear -> &nesear;
				new Transition (5146, 5147), // &nesim -> &nesim;
				new Transition (5165, 5166), // &NestedGreaterGreater -> &NestedGreaterGreater;
				new Transition (5174, 5175), // &NestedLessLess -> &NestedLessLess;
				new Transition (5180, 5181), // &NewLine -> &NewLine;
				new Transition (5185, 5186), // &nexist -> &nexist;
				new Transition (5187, 5188), // &nexists -> &nexists;
				new Transition (5190, 5191), // &Nfr -> &Nfr;
				new Transition (5193, 5194), // &nfr -> &nfr;
				new Transition (5196, 5197), // &ngE -> &ngE;
				new Transition (5198, 5199), // &nge -> &nge;
				new Transition (5200, 5201), // &ngeq -> &ngeq;
				new Transition (5202, 5203), // &ngeqq -> &ngeqq;
				new Transition (5208, 5209), // &ngeqslant -> &ngeqslant;
				new Transition (5210, 5211), // &nges -> &nges;
				new Transition (5213, 5214), // &nGg -> &nGg;
				new Transition (5217, 5218), // &ngsim -> &ngsim;
				new Transition (5219, 5220), // &nGt -> &nGt;
				new Transition (5221, 5222), // &ngt -> &ngt;
				new Transition (5223, 5224), // &ngtr -> &ngtr;
				new Transition (5225, 5226), // &nGtv -> &nGtv;
				new Transition (5230, 5231), // &nhArr -> &nhArr;
				new Transition (5234, 5235), // &nharr -> &nharr;
				new Transition (5238, 5239), // &nhpar -> &nhpar;
				new Transition (5240, 5241), // &ni -> &ni;
				new Transition (5242, 5243), // &nis -> &nis;
				new Transition (5244, 5245), // &nisd -> &nisd;
				new Transition (5246, 5247), // &niv -> &niv;
				new Transition (5250, 5251), // &NJcy -> &NJcy;
				new Transition (5254, 5255), // &njcy -> &njcy;
				new Transition (5259, 5260), // &nlArr -> &nlArr;
				new Transition (5263, 5264), // &nlarr -> &nlarr;
				new Transition (5266, 5267), // &nldr -> &nldr;
				new Transition (5268, 5269), // &nlE -> &nlE;
				new Transition (5270, 5271), // &nle -> &nle;
				new Transition (5280, 5281), // &nLeftarrow -> &nLeftarrow;
				new Transition (5288, 5289), // &nleftarrow -> &nleftarrow;
				new Transition (5299, 5300), // &nLeftrightarrow -> &nLeftrightarrow;
				new Transition (5310, 5311), // &nleftrightarrow -> &nleftrightarrow;
				new Transition (5312, 5313), // &nleq -> &nleq;
				new Transition (5314, 5315), // &nleqq -> &nleqq;
				new Transition (5320, 5321), // &nleqslant -> &nleqslant;
				new Transition (5322, 5323), // &nles -> &nles;
				new Transition (5324, 5325), // &nless -> &nless;
				new Transition (5326, 5327), // &nLl -> &nLl;
				new Transition (5330, 5331), // &nlsim -> &nlsim;
				new Transition (5332, 5333), // &nLt -> &nLt;
				new Transition (5334, 5335), // &nlt -> &nlt;
				new Transition (5337, 5338), // &nltri -> &nltri;
				new Transition (5339, 5340), // &nltrie -> &nltrie;
				new Transition (5341, 5342), // &nLtv -> &nLtv;
				new Transition (5345, 5346), // &nmid -> &nmid;
				new Transition (5352, 5353), // &NoBreak -> &NoBreak;
				new Transition (5367, 5368), // &NonBreakingSpace -> &NonBreakingSpace;
				new Transition (5370, 5371), // &Nopf -> &Nopf;
				new Transition (5374, 5375), // &nopf -> &nopf;
				new Transition (5376, 5377), // &Not -> &Not;
				new Transition (5378, 5379), // &not -> &not;
				new Transition (5388, 5389), // &NotCongruent -> &NotCongruent;
				new Transition (5394, 5395), // &NotCupCap -> &NotCupCap;
				new Transition (5412, 5413), // &NotDoubleVerticalBar -> &NotDoubleVerticalBar;
				new Transition (5420, 5421), // &NotElement -> &NotElement;
				new Transition (5425, 5426), // &NotEqual -> &NotEqual;
				new Transition (5431, 5432), // &NotEqualTilde -> &NotEqualTilde;
				new Transition (5437, 5438), // &NotExists -> &NotExists;
				new Transition (5445, 5446), // &NotGreater -> &NotGreater;
				new Transition (5451, 5452), // &NotGreaterEqual -> &NotGreaterEqual;
				new Transition (5461, 5462), // &NotGreaterFullEqual -> &NotGreaterFullEqual;
				new Transition (5469, 5470), // &NotGreaterGreater -> &NotGreaterGreater;
				new Transition (5474, 5475), // &NotGreaterLess -> &NotGreaterLess;
				new Transition (5485, 5486), // &NotGreaterSlantEqual -> &NotGreaterSlantEqual;
				new Transition (5491, 5492), // &NotGreaterTilde -> &NotGreaterTilde;
				new Transition (5504, 5505), // &NotHumpDownHump -> &NotHumpDownHump;
				new Transition (5510, 5511), // &NotHumpEqual -> &NotHumpEqual;
				new Transition (5513, 5514), // &notin -> &notin;
				new Transition (5517, 5518), // &notindot -> &notindot;
				new Transition (5519, 5520), // &notinE -> &notinE;
				new Transition (5522, 5523), // &notinva -> &notinva;
				new Transition (5524, 5525), // &notinvb -> &notinvb;
				new Transition (5526, 5527), // &notinvc -> &notinvc;
				new Transition (5539, 5540), // &NotLeftTriangle -> &NotLeftTriangle;
				new Transition (5543, 5544), // &NotLeftTriangleBar -> &NotLeftTriangleBar;
				new Transition (5549, 5550), // &NotLeftTriangleEqual -> &NotLeftTriangleEqual;
				new Transition (5552, 5553), // &NotLess -> &NotLess;
				new Transition (5558, 5559), // &NotLessEqual -> &NotLessEqual;
				new Transition (5566, 5567), // &NotLessGreater -> &NotLessGreater;
				new Transition (5571, 5572), // &NotLessLess -> &NotLessLess;
				new Transition (5582, 5583), // &NotLessSlantEqual -> &NotLessSlantEqual;
				new Transition (5588, 5589), // &NotLessTilde -> &NotLessTilde;
				new Transition (5609, 5610), // &NotNestedGreaterGreater -> &NotNestedGreaterGreater;
				new Transition (5618, 5619), // &NotNestedLessLess -> &NotNestedLessLess;
				new Transition (5621, 5622), // &notni -> &notni;
				new Transition (5624, 5625), // &notniva -> &notniva;
				new Transition (5626, 5627), // &notnivb -> &notnivb;
				new Transition (5628, 5629), // &notnivc -> &notnivc;
				new Transition (5637, 5638), // &NotPrecedes -> &NotPrecedes;
				new Transition (5643, 5644), // &NotPrecedesEqual -> &NotPrecedesEqual;
				new Transition (5654, 5655), // &NotPrecedesSlantEqual -> &NotPrecedesSlantEqual;
				new Transition (5669, 5670), // &NotReverseElement -> &NotReverseElement;
				new Transition (5682, 5683), // &NotRightTriangle -> &NotRightTriangle;
				new Transition (5686, 5687), // &NotRightTriangleBar -> &NotRightTriangleBar;
				new Transition (5692, 5693), // &NotRightTriangleEqual -> &NotRightTriangleEqual;
				new Transition (5705, 5706), // &NotSquareSubset -> &NotSquareSubset;
				new Transition (5711, 5712), // &NotSquareSubsetEqual -> &NotSquareSubsetEqual;
				new Transition (5718, 5719), // &NotSquareSuperset -> &NotSquareSuperset;
				new Transition (5724, 5725), // &NotSquareSupersetEqual -> &NotSquareSupersetEqual;
				new Transition (5730, 5731), // &NotSubset -> &NotSubset;
				new Transition (5736, 5737), // &NotSubsetEqual -> &NotSubsetEqual;
				new Transition (5743, 5744), // &NotSucceeds -> &NotSucceeds;
				new Transition (5749, 5750), // &NotSucceedsEqual -> &NotSucceedsEqual;
				new Transition (5760, 5761), // &NotSucceedsSlantEqual -> &NotSucceedsSlantEqual;
				new Transition (5766, 5767), // &NotSucceedsTilde -> &NotSucceedsTilde;
				new Transition (5773, 5774), // &NotSuperset -> &NotSuperset;
				new Transition (5779, 5780), // &NotSupersetEqual -> &NotSupersetEqual;
				new Transition (5785, 5786), // &NotTilde -> &NotTilde;
				new Transition (5791, 5792), // &NotTildeEqual -> &NotTildeEqual;
				new Transition (5801, 5802), // &NotTildeFullEqual -> &NotTildeFullEqual;
				new Transition (5807, 5808), // &NotTildeTilde -> &NotTildeTilde;
				new Transition (5819, 5820), // &NotVerticalBar -> &NotVerticalBar;
				new Transition (5823, 5824), // &npar -> &npar;
				new Transition (5829, 5830), // &nparallel -> &nparallel;
				new Transition (5832, 5833), // &nparsl -> &nparsl;
				new Transition (5834, 5835), // &npart -> &npart;
				new Transition (5840, 5841), // &npolint -> &npolint;
				new Transition (5842, 5843), // &npr -> &npr;
				new Transition (5846, 5847), // &nprcue -> &nprcue;
				new Transition (5848, 5849), // &npre -> &npre;
				new Transition (5850, 5851), // &nprec -> &nprec;
				new Transition (5853, 5854), // &npreceq -> &npreceq;
				new Transition (5858, 5859), // &nrArr -> &nrArr;
				new Transition (5862, 5863), // &nrarr -> &nrarr;
				new Transition (5864, 5865), // &nrarrc -> &nrarrc;
				new Transition (5866, 5867), // &nrarrw -> &nrarrw;
				new Transition (5877, 5878), // &nRightarrow -> &nRightarrow;
				new Transition (5887, 5888), // &nrightarrow -> &nrightarrow;
				new Transition (5891, 5892), // &nrtri -> &nrtri;
				new Transition (5893, 5894), // &nrtrie -> &nrtrie;
				new Transition (5896, 5897), // &nsc -> &nsc;
				new Transition (5900, 5901), // &nsccue -> &nsccue;
				new Transition (5902, 5903), // &nsce -> &nsce;
				new Transition (5906, 5907), // &Nscr -> &Nscr;
				new Transition (5908, 5909), // &nscr -> &nscr;
				new Transition (5916, 5917), // &nshortmid -> &nshortmid;
				new Transition (5925, 5926), // &nshortparallel -> &nshortparallel;
				new Transition (5928, 5929), // &nsim -> &nsim;
				new Transition (5930, 5931), // &nsime -> &nsime;
				new Transition (5932, 5933), // &nsimeq -> &nsimeq;
				new Transition (5936, 5937), // &nsmid -> &nsmid;
				new Transition (5940, 5941), // &nspar -> &nspar;
				new Transition (5946, 5947), // &nsqsube -> &nsqsube;
				new Transition (5949, 5950), // &nsqsupe -> &nsqsupe;
				new Transition (5952, 5953), // &nsub -> &nsub;
				new Transition (5954, 5955), // &nsubE -> &nsubE;
				new Transition (5956, 5957), // &nsube -> &nsube;
				new Transition (5960, 5961), // &nsubset -> &nsubset;
				new Transition (5963, 5964), // &nsubseteq -> &nsubseteq;
				new Transition (5965, 5966), // &nsubseteqq -> &nsubseteqq;
				new Transition (5968, 5969), // &nsucc -> &nsucc;
				new Transition (5971, 5972), // &nsucceq -> &nsucceq;
				new Transition (5973, 5974), // &nsup -> &nsup;
				new Transition (5975, 5976), // &nsupE -> &nsupE;
				new Transition (5977, 5978), // &nsupe -> &nsupe;
				new Transition (5981, 5982), // &nsupset -> &nsupset;
				new Transition (5984, 5985), // &nsupseteq -> &nsupseteq;
				new Transition (5986, 5987), // &nsupseteqq -> &nsupseteqq;
				new Transition (5990, 5991), // &ntgl -> &ntgl;
				new Transition (5996, 5997), // &Ntilde -> &Ntilde;
				new Transition (6001, 6002), // &ntilde -> &ntilde;
				new Transition (6004, 6005), // &ntlg -> &ntlg;
				new Transition (6016, 6017), // &ntriangleleft -> &ntriangleleft;
				new Transition (6019, 6020), // &ntrianglelefteq -> &ntrianglelefteq;
				new Transition (6025, 6026), // &ntriangleright -> &ntriangleright;
				new Transition (6028, 6029), // &ntrianglerighteq -> &ntrianglerighteq;
				new Transition (6030, 6031), // &Nu -> &Nu;
				new Transition (6032, 6033), // &nu -> &nu;
				new Transition (6034, 6035), // &num -> &num;
				new Transition (6038, 6039), // &numero -> &numero;
				new Transition (6041, 6042), // &numsp -> &numsp;
				new Transition (6045, 6046), // &nvap -> &nvap;
				new Transition (6051, 6052), // &nVDash -> &nVDash;
				new Transition (6056, 6057), // &nVdash -> &nVdash;
				new Transition (6061, 6062), // &nvDash -> &nvDash;
				new Transition (6066, 6067), // &nvdash -> &nvdash;
				new Transition (6069, 6070), // &nvge -> &nvge;
				new Transition (6071, 6072), // &nvgt -> &nvgt;
				new Transition (6076, 6077), // &nvHarr -> &nvHarr;
				new Transition (6082, 6083), // &nvinfin -> &nvinfin;
				new Transition (6087, 6088), // &nvlArr -> &nvlArr;
				new Transition (6089, 6090), // &nvle -> &nvle;
				new Transition (6091, 6092), // &nvlt -> &nvlt;
				new Transition (6095, 6096), // &nvltrie -> &nvltrie;
				new Transition (6100, 6101), // &nvrArr -> &nvrArr;
				new Transition (6105, 6106), // &nvrtrie -> &nvrtrie;
				new Transition (6109, 6110), // &nvsim -> &nvsim;
				new Transition (6115, 6116), // &nwarhk -> &nwarhk;
				new Transition (6119, 6120), // &nwArr -> &nwArr;
				new Transition (6121, 6122), // &nwarr -> &nwarr;
				new Transition (6124, 6125), // &nwarrow -> &nwarrow;
				new Transition (6129, 6130), // &nwnear -> &nwnear;
				new Transition (6136, 6137), // &Oacute -> &Oacute;
				new Transition (6143, 6144), // &oacute -> &oacute;
				new Transition (6146, 6147), // &oast -> &oast;
				new Transition (6150, 6151), // &ocir -> &ocir;
				new Transition (6155, 6156), // &Ocirc -> &Ocirc;
				new Transition (6157, 6158), // &ocirc -> &ocirc;
				new Transition (6159, 6160), // &Ocy -> &Ocy;
				new Transition (6161, 6162), // &ocy -> &ocy;
				new Transition (6166, 6167), // &odash -> &odash;
				new Transition (6172, 6173), // &Odblac -> &Odblac;
				new Transition (6177, 6178), // &odblac -> &odblac;
				new Transition (6180, 6181), // &odiv -> &odiv;
				new Transition (6183, 6184), // &odot -> &odot;
				new Transition (6188, 6189), // &odsold -> &odsold;
				new Transition (6193, 6194), // &OElig -> &OElig;
				new Transition (6198, 6199), // &oelig -> &oelig;
				new Transition (6203, 6204), // &ofcir -> &ofcir;
				new Transition (6206, 6207), // &Ofr -> &Ofr;
				new Transition (6208, 6209), // &ofr -> &ofr;
				new Transition (6212, 6213), // &ogon -> &ogon;
				new Transition (6218, 6219), // &Ograve -> &Ograve;
				new Transition (6223, 6224), // &ograve -> &ograve;
				new Transition (6225, 6226), // &ogt -> &ogt;
				new Transition (6230, 6231), // &ohbar -> &ohbar;
				new Transition (6232, 6233), // &ohm -> &ohm;
				new Transition (6236, 6237), // &oint -> &oint;
				new Transition (6241, 6242), // &olarr -> &olarr;
				new Transition (6245, 6246), // &olcir -> &olcir;
				new Transition (6250, 6251), // &olcross -> &olcross;
				new Transition (6254, 6255), // &oline -> &oline;
				new Transition (6256, 6257), // &olt -> &olt;
				new Transition (6261, 6262), // &Omacr -> &Omacr;
				new Transition (6266, 6267), // &omacr -> &omacr;
				new Transition (6270, 6271), // &Omega -> &Omega;
				new Transition (6274, 6275), // &omega -> &omega;
				new Transition (6280, 6281), // &Omicron -> &Omicron;
				new Transition (6286, 6287), // &omicron -> &omicron;
				new Transition (6288, 6289), // &omid -> &omid;
				new Transition (6292, 6293), // &ominus -> &ominus;
				new Transition (6296, 6297), // &Oopf -> &Oopf;
				new Transition (6300, 6301), // &oopf -> &oopf;
				new Transition (6304, 6305), // &opar -> &opar;
				new Transition (6324, 6325), // &OpenCurlyDoubleQuote -> &OpenCurlyDoubleQuote;
				new Transition (6330, 6331), // &OpenCurlyQuote -> &OpenCurlyQuote;
				new Transition (6334, 6335), // &operp -> &operp;
				new Transition (6338, 6339), // &oplus -> &oplus;
				new Transition (6340, 6341), // &Or -> &Or;
				new Transition (6342, 6343), // &or -> &or;
				new Transition (6346, 6347), // &orarr -> &orarr;
				new Transition (6348, 6349), // &ord -> &ord;
				new Transition (6351, 6352), // &order -> &order;
				new Transition (6354, 6355), // &orderof -> &orderof;
				new Transition (6356, 6357), // &ordf -> &ordf;
				new Transition (6358, 6359), // &ordm -> &ordm;
				new Transition (6363, 6364), // &origof -> &origof;
				new Transition (6366, 6367), // &oror -> &oror;
				new Transition (6372, 6373), // &orslope -> &orslope;
				new Transition (6374, 6375), // &orv -> &orv;
				new Transition (6376, 6377), // &oS -> &oS;
				new Transition (6380, 6381), // &Oscr -> &Oscr;
				new Transition (6384, 6385), // &oscr -> &oscr;
				new Transition (6389, 6390), // &Oslash -> &Oslash;
				new Transition (6394, 6395), // &oslash -> &oslash;
				new Transition (6397, 6398), // &osol -> &osol;
				new Transition (6403, 6404), // &Otilde -> &Otilde;
				new Transition (6409, 6410), // &otilde -> &otilde;
				new Transition (6413, 6414), // &Otimes -> &Otimes;
				new Transition (6417, 6418), // &otimes -> &otimes;
				new Transition (6420, 6421), // &otimesas -> &otimesas;
				new Transition (6424, 6425), // &Ouml -> &Ouml;
				new Transition (6428, 6429), // &ouml -> &ouml;
				new Transition (6433, 6434), // &ovbar -> &ovbar;
				new Transition (6440, 6441), // &OverBar -> &OverBar;
				new Transition (6445, 6446), // &OverBrace -> &OverBrace;
				new Transition (6449, 6450), // &OverBracket -> &OverBracket;
				new Transition (6461, 6462), // &OverParenthesis -> &OverParenthesis;
				new Transition (6465, 6466), // &par -> &par;
				new Transition (6467, 6468), // &para -> &para;
				new Transition (6472, 6473), // &parallel -> &parallel;
				new Transition (6476, 6477), // &parsim -> &parsim;
				new Transition (6478, 6479), // &parsl -> &parsl;
				new Transition (6480, 6481), // &part -> &part;
				new Transition (6489, 6490), // &PartialD -> &PartialD;
				new Transition (6492, 6493), // &Pcy -> &Pcy;
				new Transition (6495, 6496), // &pcy -> &pcy;
				new Transition (6501, 6502), // &percnt -> &percnt;
				new Transition (6505, 6506), // &period -> &period;
				new Transition (6509, 6510), // &permil -> &permil;
				new Transition (6511, 6512), // &perp -> &perp;
				new Transition (6516, 6517), // &pertenk -> &pertenk;
				new Transition (6519, 6520), // &Pfr -> &Pfr;
				new Transition (6522, 6523), // &pfr -> &pfr;
				new Transition (6525, 6526), // &Phi -> &Phi;
				new Transition (6528, 6529), // &phi -> &phi;
				new Transition (6530, 6531), // &phiv -> &phiv;
				new Transition (6535, 6536), // &phmmat -> &phmmat;
				new Transition (6539, 6540), // &phone -> &phone;
				new Transition (6541, 6542), // &Pi -> &Pi;
				new Transition (6543, 6544), // &pi -> &pi;
				new Transition (6551, 6552), // &pitchfork -> &pitchfork;
				new Transition (6553, 6554), // &piv -> &piv;
				new Transition (6559, 6560), // &planck -> &planck;
				new Transition (6561, 6562), // &planckh -> &planckh;
				new Transition (6564, 6565), // &plankv -> &plankv;
				new Transition (6567, 6568), // &plus -> &plus;
				new Transition (6572, 6573), // &plusacir -> &plusacir;
				new Transition (6574, 6575), // &plusb -> &plusb;
				new Transition (6578, 6579), // &pluscir -> &pluscir;
				new Transition (6581, 6582), // &plusdo -> &plusdo;
				new Transition (6583, 6584), // &plusdu -> &plusdu;
				new Transition (6585, 6586), // &pluse -> &pluse;
				new Transition (6594, 6595), // &PlusMinus -> &PlusMinus;
				new Transition (6597, 6598), // &plusmn -> &plusmn;
				new Transition (6601, 6602), // &plussim -> &plussim;
				new Transition (6605, 6606), // &plustwo -> &plustwo;
				new Transition (6607, 6608), // &pm -> &pm;
				new Transition (6620, 6621), // &Poincareplane -> &Poincareplane;
				new Transition (6628, 6629), // &pointint -> &pointint;
				new Transition (6631, 6632), // &Popf -> &Popf;
				new Transition (6634, 6635), // &popf -> &popf;
				new Transition (6638, 6639), // &pound -> &pound;
				new Transition (6640, 6641), // &Pr -> &Pr;
				new Transition (6642, 6643), // &pr -> &pr;
				new Transition (6645, 6646), // &prap -> &prap;
				new Transition (6649, 6650), // &prcue -> &prcue;
				new Transition (6651, 6652), // &prE -> &prE;
				new Transition (6653, 6654), // &pre -> &pre;
				new Transition (6655, 6656), // &prec -> &prec;
				new Transition (6662, 6663), // &precapprox -> &precapprox;
				new Transition (6670, 6671), // &preccurlyeq -> &preccurlyeq;
				new Transition (6677, 6678), // &Precedes -> &Precedes;
				new Transition (6683, 6684), // &PrecedesEqual -> &PrecedesEqual;
				new Transition (6694, 6695), // &PrecedesSlantEqual -> &PrecedesSlantEqual;
				new Transition (6700, 6701), // &PrecedesTilde -> &PrecedesTilde;
				new Transition (6703, 6704), // &preceq -> &preceq;
				new Transition (6711, 6712), // &precnapprox -> &precnapprox;
				new Transition (6715, 6716), // &precneqq -> &precneqq;
				new Transition (6719, 6720), // &precnsim -> &precnsim;
				new Transition (6723, 6724), // &precsim -> &precsim;
				new Transition (6727, 6728), // &Prime -> &Prime;
				new Transition (6731, 6732), // &prime -> &prime;
				new Transition (6733, 6734), // &primes -> &primes;
				new Transition (6737, 6738), // &prnap -> &prnap;
				new Transition (6739, 6740), // &prnE -> &prnE;
				new Transition (6743, 6744), // &prnsim -> &prnsim;
				new Transition (6746, 6747), // &prod -> &prod;
				new Transition (6752, 6753), // &Product -> &Product;
				new Transition (6758, 6759), // &profalar -> &profalar;
				new Transition (6763, 6764), // &profline -> &profline;
				new Transition (6768, 6769), // &profsurf -> &profsurf;
				new Transition (6770, 6771), // &prop -> &prop;
				new Transition (6778, 6779), // &Proportion -> &Proportion;
				new Transition (6781, 6782), // &Proportional -> &Proportional;
				new Transition (6784, 6785), // &propto -> &propto;
				new Transition (6788, 6789), // &prsim -> &prsim;
				new Transition (6793, 6794), // &prurel -> &prurel;
				new Transition (6797, 6798), // &Pscr -> &Pscr;
				new Transition (6801, 6802), // &pscr -> &pscr;
				new Transition (6803, 6804), // &Psi -> &Psi;
				new Transition (6805, 6806), // &psi -> &psi;
				new Transition (6811, 6812), // &puncsp -> &puncsp;
				new Transition (6815, 6816), // &Qfr -> &Qfr;
				new Transition (6819, 6820), // &qfr -> &qfr;
				new Transition (6823, 6824), // &qint -> &qint;
				new Transition (6827, 6828), // &Qopf -> &Qopf;
				new Transition (6831, 6832), // &qopf -> &qopf;
				new Transition (6837, 6838), // &qprime -> &qprime;
				new Transition (6841, 6842), // &Qscr -> &Qscr;
				new Transition (6845, 6846), // &qscr -> &qscr;
				new Transition (6856, 6857), // &quaternions -> &quaternions;
				new Transition (6860, 6861), // &quatint -> &quatint;
				new Transition (6864, 6865), // &quest -> &quest;
				new Transition (6867, 6868), // &questeq -> &questeq;
				new Transition (6871, 6872), // &QUOT -> &QUOT;
				new Transition (6874, 6875), // &quot -> &quot;
				new Transition (6880, 6881), // &rAarr -> &rAarr;
				new Transition (6884, 6885), // &race -> &race;
				new Transition (6891, 6892), // &Racute -> &Racute;
				new Transition (6895, 6896), // &racute -> &racute;
				new Transition (6899, 6900), // &radic -> &radic;
				new Transition (6906, 6907), // &raemptyv -> &raemptyv;
				new Transition (6909, 6910), // &Rang -> &Rang;
				new Transition (6912, 6913), // &rang -> &rang;
				new Transition (6914, 6915), // &rangd -> &rangd;
				new Transition (6916, 6917), // &range -> &range;
				new Transition (6919, 6920), // &rangle -> &rangle;
				new Transition (6923, 6924), // &raquo -> &raquo;
				new Transition (6926, 6927), // &Rarr -> &Rarr;
				new Transition (6929, 6930), // &rArr -> &rArr;
				new Transition (6932, 6933), // &rarr -> &rarr;
				new Transition (6935, 6936), // &rarrap -> &rarrap;
				new Transition (6937, 6938), // &rarrb -> &rarrb;
				new Transition (6940, 6941), // &rarrbfs -> &rarrbfs;
				new Transition (6942, 6943), // &rarrc -> &rarrc;
				new Transition (6945, 6946), // &rarrfs -> &rarrfs;
				new Transition (6948, 6949), // &rarrhk -> &rarrhk;
				new Transition (6951, 6952), // &rarrlp -> &rarrlp;
				new Transition (6954, 6955), // &rarrpl -> &rarrpl;
				new Transition (6958, 6959), // &rarrsim -> &rarrsim;
				new Transition (6961, 6962), // &Rarrtl -> &Rarrtl;
				new Transition (6964, 6965), // &rarrtl -> &rarrtl;
				new Transition (6966, 6967), // &rarrw -> &rarrw;
				new Transition (6971, 6972), // &rAtail -> &rAtail;
				new Transition (6976, 6977), // &ratail -> &ratail;
				new Transition (6979, 6980), // &ratio -> &ratio;
				new Transition (6984, 6985), // &rationals -> &rationals;
				new Transition (6989, 6990), // &RBarr -> &RBarr;
				new Transition (6994, 6995), // &rBarr -> &rBarr;
				new Transition (6999, 7000), // &rbarr -> &rbarr;
				new Transition (7003, 7004), // &rbbrk -> &rbbrk;
				new Transition (7008, 7009), // &rbrace -> &rbrace;
				new Transition (7010, 7011), // &rbrack -> &rbrack;
				new Transition (7013, 7014), // &rbrke -> &rbrke;
				new Transition (7017, 7018), // &rbrksld -> &rbrksld;
				new Transition (7019, 7020), // &rbrkslu -> &rbrkslu;
				new Transition (7025, 7026), // &Rcaron -> &Rcaron;
				new Transition (7031, 7032), // &rcaron -> &rcaron;
				new Transition (7036, 7037), // &Rcedil -> &Rcedil;
				new Transition (7041, 7042), // &rcedil -> &rcedil;
				new Transition (7044, 7045), // &rceil -> &rceil;
				new Transition (7047, 7048), // &rcub -> &rcub;
				new Transition (7049, 7050), // &Rcy -> &Rcy;
				new Transition (7051, 7052), // &rcy -> &rcy;
				new Transition (7055, 7056), // &rdca -> &rdca;
				new Transition (7061, 7062), // &rdldhar -> &rdldhar;
				new Transition (7065, 7066), // &rdquo -> &rdquo;
				new Transition (7067, 7068), // &rdquor -> &rdquor;
				new Transition (7070, 7071), // &rdsh -> &rdsh;
				new Transition (7072, 7073), // &Re -> &Re;
				new Transition (7076, 7077), // &real -> &real;
				new Transition (7080, 7081), // &realine -> &realine;
				new Transition (7085, 7086), // &realpart -> &realpart;
				new Transition (7087, 7088), // &reals -> &reals;
				new Transition (7090, 7091), // &rect -> &rect;
				new Transition (7093, 7094), // &REG -> &REG;
				new Transition (7095, 7096), // &reg -> &reg;
				new Transition (7108, 7109), // &ReverseElement -> &ReverseElement;
				new Transition (7119, 7120), // &ReverseEquilibrium -> &ReverseEquilibrium;
				new Transition (7133, 7134), // &ReverseUpEquilibrium -> &ReverseUpEquilibrium;
				new Transition (7139, 7140), // &rfisht -> &rfisht;
				new Transition (7144, 7145), // &rfloor -> &rfloor;
				new Transition (7147, 7148), // &Rfr -> &Rfr;
				new Transition (7149, 7150), // &rfr -> &rfr;
				new Transition (7153, 7154), // &rHar -> &rHar;
				new Transition (7158, 7159), // &rhard -> &rhard;
				new Transition (7160, 7161), // &rharu -> &rharu;
				new Transition (7162, 7163), // &rharul -> &rharul;
				new Transition (7165, 7166), // &Rho -> &Rho;
				new Transition (7167, 7168), // &rho -> &rho;
				new Transition (7169, 7170), // &rhov -> &rhov;
				new Transition (7186, 7187), // &RightAngleBracket -> &RightAngleBracket;
				new Transition (7191, 7192), // &RightArrow -> &RightArrow;
				new Transition (7197, 7198), // &Rightarrow -> &Rightarrow;
				new Transition (7207, 7208), // &rightarrow -> &rightarrow;
				new Transition (7211, 7212), // &RightArrowBar -> &RightArrowBar;
				new Transition (7221, 7222), // &RightArrowLeftArrow -> &RightArrowLeftArrow;
				new Transition (7226, 7227), // &rightarrowtail -> &rightarrowtail;
				new Transition (7234, 7235), // &RightCeiling -> &RightCeiling;
				new Transition (7248, 7249), // &RightDoubleBracket -> &RightDoubleBracket;
				new Transition (7260, 7261), // &RightDownTeeVector -> &RightDownTeeVector;
				new Transition (7267, 7268), // &RightDownVector -> &RightDownVector;
				new Transition (7271, 7272), // &RightDownVectorBar -> &RightDownVectorBar;
				new Transition (7277, 7278), // &RightFloor -> &RightFloor;
				new Transition (7289, 7290), // &rightharpoondown -> &rightharpoondown;
				new Transition (7292, 7293), // &rightharpoonup -> &rightharpoonup;
				new Transition (7303, 7304), // &rightleftarrows -> &rightleftarrows;
				new Transition (7312, 7313), // &rightleftharpoons -> &rightleftharpoons;
				new Transition (7324, 7325), // &rightrightarrows -> &rightrightarrows;
				new Transition (7335, 7336), // &rightsquigarrow -> &rightsquigarrow;
				new Transition (7339, 7340), // &RightTee -> &RightTee;
				new Transition (7345, 7346), // &RightTeeArrow -> &RightTeeArrow;
				new Transition (7352, 7353), // &RightTeeVector -> &RightTeeVector;
				new Transition (7363, 7364), // &rightthreetimes -> &rightthreetimes;
				new Transition (7371, 7372), // &RightTriangle -> &RightTriangle;
				new Transition (7375, 7376), // &RightTriangleBar -> &RightTriangleBar;
				new Transition (7381, 7382), // &RightTriangleEqual -> &RightTriangleEqual;
				new Transition (7394, 7395), // &RightUpDownVector -> &RightUpDownVector;
				new Transition (7404, 7405), // &RightUpTeeVector -> &RightUpTeeVector;
				new Transition (7411, 7412), // &RightUpVector -> &RightUpVector;
				new Transition (7415, 7416), // &RightUpVectorBar -> &RightUpVectorBar;
				new Transition (7422, 7423), // &RightVector -> &RightVector;
				new Transition (7426, 7427), // &RightVectorBar -> &RightVectorBar;
				new Transition (7429, 7430), // &ring -> &ring;
				new Transition (7440, 7441), // &risingdotseq -> &risingdotseq;
				new Transition (7445, 7446), // &rlarr -> &rlarr;
				new Transition (7449, 7450), // &rlhar -> &rlhar;
				new Transition (7451, 7452), // &rlm -> &rlm;
				new Transition (7457, 7458), // &rmoust -> &rmoust;
				new Transition (7462, 7463), // &rmoustache -> &rmoustache;
				new Transition (7467, 7468), // &rnmid -> &rnmid;
				new Transition (7472, 7473), // &roang -> &roang;
				new Transition (7475, 7476), // &roarr -> &roarr;
				new Transition (7479, 7480), // &robrk -> &robrk;
				new Transition (7483, 7484), // &ropar -> &ropar;
				new Transition (7487, 7488), // &Ropf -> &Ropf;
				new Transition (7489, 7490), // &ropf -> &ropf;
				new Transition (7493, 7494), // &roplus -> &roplus;
				new Transition (7499, 7500), // &rotimes -> &rotimes;
				new Transition (7510, 7511), // &RoundImplies -> &RoundImplies;
				new Transition (7514, 7515), // &rpar -> &rpar;
				new Transition (7517, 7518), // &rpargt -> &rpargt;
				new Transition (7524, 7525), // &rppolint -> &rppolint;
				new Transition (7529, 7530), // &rrarr -> &rrarr;
				new Transition (7540, 7541), // &Rrightarrow -> &Rrightarrow;
				new Transition (7546, 7547), // &rsaquo -> &rsaquo;
				new Transition (7550, 7551), // &Rscr -> &Rscr;
				new Transition (7553, 7554), // &rscr -> &rscr;
				new Transition (7555, 7556), // &Rsh -> &Rsh;
				new Transition (7557, 7558), // &rsh -> &rsh;
				new Transition (7560, 7561), // &rsqb -> &rsqb;
				new Transition (7563, 7564), // &rsquo -> &rsquo;
				new Transition (7565, 7566), // &rsquor -> &rsquor;
				new Transition (7571, 7572), // &rthree -> &rthree;
				new Transition (7576, 7577), // &rtimes -> &rtimes;
				new Transition (7579, 7580), // &rtri -> &rtri;
				new Transition (7581, 7582), // &rtrie -> &rtrie;
				new Transition (7583, 7584), // &rtrif -> &rtrif;
				new Transition (7588, 7589), // &rtriltri -> &rtriltri;
				new Transition (7599, 7600), // &RuleDelayed -> &RuleDelayed;
				new Transition (7606, 7607), // &ruluhar -> &ruluhar;
				new Transition (7608, 7609), // &rx -> &rx;
				new Transition (7615, 7616), // &Sacute -> &Sacute;
				new Transition (7622, 7623), // &sacute -> &sacute;
				new Transition (7627, 7628), // &sbquo -> &sbquo;
				new Transition (7629, 7630), // &Sc -> &Sc;
				new Transition (7631, 7632), // &sc -> &sc;
				new Transition (7634, 7635), // &scap -> &scap;
				new Transition (7639, 7640), // &Scaron -> &Scaron;
				new Transition (7643, 7644), // &scaron -> &scaron;
				new Transition (7647, 7648), // &sccue -> &sccue;
				new Transition (7649, 7650), // &scE -> &scE;
				new Transition (7651, 7652), // &sce -> &sce;
				new Transition (7656, 7657), // &Scedil -> &Scedil;
				new Transition (7660, 7661), // &scedil -> &scedil;
				new Transition (7664, 7665), // &Scirc -> &Scirc;
				new Transition (7668, 7669), // &scirc -> &scirc;
				new Transition (7672, 7673), // &scnap -> &scnap;
				new Transition (7674, 7675), // &scnE -> &scnE;
				new Transition (7678, 7679), // &scnsim -> &scnsim;
				new Transition (7685, 7686), // &scpolint -> &scpolint;
				new Transition (7689, 7690), // &scsim -> &scsim;
				new Transition (7691, 7692), // &Scy -> &Scy;
				new Transition (7693, 7694), // &scy -> &scy;
				new Transition (7697, 7698), // &sdot -> &sdot;
				new Transition (7699, 7700), // &sdotb -> &sdotb;
				new Transition (7701, 7702), // &sdote -> &sdote;
				new Transition (7707, 7708), // &searhk -> &searhk;
				new Transition (7711, 7712), // &seArr -> &seArr;
				new Transition (7713, 7714), // &searr -> &searr;
				new Transition (7716, 7717), // &searrow -> &searrow;
				new Transition (7719, 7720), // &sect -> &sect;
				new Transition (7722, 7723), // &semi -> &semi;
				new Transition (7727, 7728), // &seswar -> &seswar;
				new Transition (7734, 7735), // &setminus -> &setminus;
				new Transition (7736, 7737), // &setmn -> &setmn;
				new Transition (7739, 7740), // &sext -> &sext;
				new Transition (7742, 7743), // &Sfr -> &Sfr;
				new Transition (7745, 7746), // &sfr -> &sfr;
				new Transition (7749, 7750), // &sfrown -> &sfrown;
				new Transition (7754, 7755), // &sharp -> &sharp;
				new Transition (7760, 7761), // &SHCHcy -> &SHCHcy;
				new Transition (7765, 7766), // &shchcy -> &shchcy;
				new Transition (7768, 7769), // &SHcy -> &SHcy;
				new Transition (7770, 7771), // &shcy -> &shcy;
				new Transition (7784, 7785), // &ShortDownArrow -> &ShortDownArrow;
				new Transition (7794, 7795), // &ShortLeftArrow -> &ShortLeftArrow;
				new Transition (7801, 7802), // &shortmid -> &shortmid;
				new Transition (7810, 7811), // &shortparallel -> &shortparallel;
				new Transition (7821, 7822), // &ShortRightArrow -> &ShortRightArrow;
				new Transition (7829, 7830), // &ShortUpArrow -> &ShortUpArrow;
				new Transition (7831, 7832), // &shy -> &shy;
				new Transition (7836, 7837), // &Sigma -> &Sigma;
				new Transition (7841, 7842), // &sigma -> &sigma;
				new Transition (7843, 7844), // &sigmaf -> &sigmaf;
				new Transition (7845, 7846), // &sigmav -> &sigmav;
				new Transition (7847, 7848), // &sim -> &sim;
				new Transition (7851, 7852), // &simdot -> &simdot;
				new Transition (7853, 7854), // &sime -> &sime;
				new Transition (7855, 7856), // &simeq -> &simeq;
				new Transition (7857, 7858), // &simg -> &simg;
				new Transition (7859, 7860), // &simgE -> &simgE;
				new Transition (7861, 7862), // &siml -> &siml;
				new Transition (7863, 7864), // &simlE -> &simlE;
				new Transition (7866, 7867), // &simne -> &simne;
				new Transition (7871, 7872), // &simplus -> &simplus;
				new Transition (7876, 7877), // &simrarr -> &simrarr;
				new Transition (7881, 7882), // &slarr -> &slarr;
				new Transition (7892, 7893), // &SmallCircle -> &SmallCircle;
				new Transition (7905, 7906), // &smallsetminus -> &smallsetminus;
				new Transition (7909, 7910), // &smashp -> &smashp;
				new Transition (7916, 7917), // &smeparsl -> &smeparsl;
				new Transition (7919, 7920), // &smid -> &smid;
				new Transition (7922, 7923), // &smile -> &smile;
				new Transition (7924, 7925), // &smt -> &smt;
				new Transition (7926, 7927), // &smte -> &smte;
				new Transition (7928, 7929), // &smtes -> &smtes;
				new Transition (7934, 7935), // &SOFTcy -> &SOFTcy;
				new Transition (7940, 7941), // &softcy -> &softcy;
				new Transition (7942, 7943), // &sol -> &sol;
				new Transition (7944, 7945), // &solb -> &solb;
				new Transition (7947, 7948), // &solbar -> &solbar;
				new Transition (7951, 7952), // &Sopf -> &Sopf;
				new Transition (7954, 7955), // &sopf -> &sopf;
				new Transition (7960, 7961), // &spades -> &spades;
				new Transition (7964, 7965), // &spadesuit -> &spadesuit;
				new Transition (7966, 7967), // &spar -> &spar;
				new Transition (7971, 7972), // &sqcap -> &sqcap;
				new Transition (7973, 7974), // &sqcaps -> &sqcaps;
				new Transition (7976, 7977), // &sqcup -> &sqcup;
				new Transition (7978, 7979), // &sqcups -> &sqcups;
				new Transition (7982, 7983), // &Sqrt -> &Sqrt;
				new Transition (7986, 7987), // &sqsub -> &sqsub;
				new Transition (7988, 7989), // &sqsube -> &sqsube;
				new Transition (7992, 7993), // &sqsubset -> &sqsubset;
				new Transition (7995, 7996), // &sqsubseteq -> &sqsubseteq;
				new Transition (7997, 7998), // &sqsup -> &sqsup;
				new Transition (7999, 8000), // &sqsupe -> &sqsupe;
				new Transition (8003, 8004), // &sqsupset -> &sqsupset;
				new Transition (8006, 8007), // &sqsupseteq -> &sqsupseteq;
				new Transition (8008, 8009), // &squ -> &squ;
				new Transition (8013, 8014), // &Square -> &Square;
				new Transition (8017, 8018), // &square -> &square;
				new Transition (8030, 8031), // &SquareIntersection -> &SquareIntersection;
				new Transition (8037, 8038), // &SquareSubset -> &SquareSubset;
				new Transition (8043, 8044), // &SquareSubsetEqual -> &SquareSubsetEqual;
				new Transition (8050, 8051), // &SquareSuperset -> &SquareSuperset;
				new Transition (8056, 8057), // &SquareSupersetEqual -> &SquareSupersetEqual;
				new Transition (8062, 8063), // &SquareUnion -> &SquareUnion;
				new Transition (8064, 8065), // &squarf -> &squarf;
				new Transition (8066, 8067), // &squf -> &squf;
				new Transition (8071, 8072), // &srarr -> &srarr;
				new Transition (8075, 8076), // &Sscr -> &Sscr;
				new Transition (8079, 8080), // &sscr -> &sscr;
				new Transition (8084, 8085), // &ssetmn -> &ssetmn;
				new Transition (8089, 8090), // &ssmile -> &ssmile;
				new Transition (8094, 8095), // &sstarf -> &sstarf;
				new Transition (8098, 8099), // &Star -> &Star;
				new Transition (8102, 8103), // &star -> &star;
				new Transition (8104, 8105), // &starf -> &starf;
				new Transition (8118, 8119), // &straightepsilon -> &straightepsilon;
				new Transition (8122, 8123), // &straightphi -> &straightphi;
				new Transition (8125, 8126), // &strns -> &strns;
				new Transition (8128, 8129), // &Sub -> &Sub;
				new Transition (8131, 8132), // &sub -> &sub;
				new Transition (8135, 8136), // &subdot -> &subdot;
				new Transition (8137, 8138), // &subE -> &subE;
				new Transition (8139, 8140), // &sube -> &sube;
				new Transition (8143, 8144), // &subedot -> &subedot;
				new Transition (8148, 8149), // &submult -> &submult;
				new Transition (8151, 8152), // &subnE -> &subnE;
				new Transition (8153, 8154), // &subne -> &subne;
				new Transition (8158, 8159), // &subplus -> &subplus;
				new Transition (8163, 8164), // &subrarr -> &subrarr;
				new Transition (8167, 8168), // &Subset -> &Subset;
				new Transition (8171, 8172), // &subset -> &subset;
				new Transition (8174, 8175), // &subseteq -> &subseteq;
				new Transition (8176, 8177), // &subseteqq -> &subseteqq;
				new Transition (8182, 8183), // &SubsetEqual -> &SubsetEqual;
				new Transition (8186, 8187), // &subsetneq -> &subsetneq;
				new Transition (8188, 8189), // &subsetneqq -> &subsetneqq;
				new Transition (8191, 8192), // &subsim -> &subsim;
				new Transition (8194, 8195), // &subsub -> &subsub;
				new Transition (8196, 8197), // &subsup -> &subsup;
				new Transition (8199, 8200), // &succ -> &succ;
				new Transition (8206, 8207), // &succapprox -> &succapprox;
				new Transition (8214, 8215), // &succcurlyeq -> &succcurlyeq;
				new Transition (8221, 8222), // &Succeeds -> &Succeeds;
				new Transition (8227, 8228), // &SucceedsEqual -> &SucceedsEqual;
				new Transition (8238, 8239), // &SucceedsSlantEqual -> &SucceedsSlantEqual;
				new Transition (8244, 8245), // &SucceedsTilde -> &SucceedsTilde;
				new Transition (8247, 8248), // &succeq -> &succeq;
				new Transition (8255, 8256), // &succnapprox -> &succnapprox;
				new Transition (8259, 8260), // &succneqq -> &succneqq;
				new Transition (8263, 8264), // &succnsim -> &succnsim;
				new Transition (8267, 8268), // &succsim -> &succsim;
				new Transition (8273, 8274), // &SuchThat -> &SuchThat;
				new Transition (8275, 8276), // &Sum -> &Sum;
				new Transition (8277, 8278), // &sum -> &sum;
				new Transition (8280, 8281), // &sung -> &sung;
				new Transition (8282, 8283), // &Sup -> &Sup;
				new Transition (8284, 8285), // &sup -> &sup;
				new Transition (8286, 8287), // &sup1 -> &sup1;
				new Transition (8288, 8289), // &sup2 -> &sup2;
				new Transition (8290, 8291), // &sup3 -> &sup3;
				new Transition (8294, 8295), // &supdot -> &supdot;
				new Transition (8298, 8299), // &supdsub -> &supdsub;
				new Transition (8300, 8301), // &supE -> &supE;
				new Transition (8302, 8303), // &supe -> &supe;
				new Transition (8306, 8307), // &supedot -> &supedot;
				new Transition (8312, 8313), // &Superset -> &Superset;
				new Transition (8318, 8319), // &SupersetEqual -> &SupersetEqual;
				new Transition (8323, 8324), // &suphsol -> &suphsol;
				new Transition (8326, 8327), // &suphsub -> &suphsub;
				new Transition (8331, 8332), // &suplarr -> &suplarr;
				new Transition (8336, 8337), // &supmult -> &supmult;
				new Transition (8339, 8340), // &supnE -> &supnE;
				new Transition (8341, 8342), // &supne -> &supne;
				new Transition (8346, 8347), // &supplus -> &supplus;
				new Transition (8350, 8351), // &Supset -> &Supset;
				new Transition (8354, 8355), // &supset -> &supset;
				new Transition (8357, 8358), // &supseteq -> &supseteq;
				new Transition (8359, 8360), // &supseteqq -> &supseteqq;
				new Transition (8363, 8364), // &supsetneq -> &supsetneq;
				new Transition (8365, 8366), // &supsetneqq -> &supsetneqq;
				new Transition (8368, 8369), // &supsim -> &supsim;
				new Transition (8371, 8372), // &supsub -> &supsub;
				new Transition (8373, 8374), // &supsup -> &supsup;
				new Transition (8379, 8380), // &swarhk -> &swarhk;
				new Transition (8383, 8384), // &swArr -> &swArr;
				new Transition (8385, 8386), // &swarr -> &swarr;
				new Transition (8388, 8389), // &swarrow -> &swarrow;
				new Transition (8393, 8394), // &swnwar -> &swnwar;
				new Transition (8398, 8399), // &szlig -> &szlig;
				new Transition (8402, 8403), // &Tab -> &Tab;
				new Transition (8409, 8410), // &target -> &target;
				new Transition (8411, 8412), // &Tau -> &Tau;
				new Transition (8413, 8414), // &tau -> &tau;
				new Transition (8417, 8418), // &tbrk -> &tbrk;
				new Transition (8423, 8424), // &Tcaron -> &Tcaron;
				new Transition (8429, 8430), // &tcaron -> &tcaron;
				new Transition (8434, 8435), // &Tcedil -> &Tcedil;
				new Transition (8439, 8440), // &tcedil -> &tcedil;
				new Transition (8441, 8442), // &Tcy -> &Tcy;
				new Transition (8443, 8444), // &tcy -> &tcy;
				new Transition (8447, 8448), // &tdot -> &tdot;
				new Transition (8453, 8454), // &telrec -> &telrec;
				new Transition (8456, 8457), // &Tfr -> &Tfr;
				new Transition (8459, 8460), // &tfr -> &tfr;
				new Transition (8465, 8466), // &there4 -> &there4;
				new Transition (8474, 8475), // &Therefore -> &Therefore;
				new Transition (8479, 8480), // &therefore -> &therefore;
				new Transition (8482, 8483), // &Theta -> &Theta;
				new Transition (8485, 8486), // &theta -> &theta;
				new Transition (8489, 8490), // &thetasym -> &thetasym;
				new Transition (8491, 8492), // &thetav -> &thetav;
				new Transition (8501, 8502), // &thickapprox -> &thickapprox;
				new Transition (8505, 8506), // &thicksim -> &thicksim;
				new Transition (8514, 8515), // &ThickSpace -> &ThickSpace;
				new Transition (8518, 8519), // &thinsp -> &thinsp;
				new Transition (8525, 8526), // &ThinSpace -> &ThinSpace;
				new Transition (8529, 8530), // &thkap -> &thkap;
				new Transition (8533, 8534), // &thksim -> &thksim;
				new Transition (8538, 8539), // &THORN -> &THORN;
				new Transition (8542, 8543), // &thorn -> &thorn;
				new Transition (8547, 8548), // &Tilde -> &Tilde;
				new Transition (8552, 8553), // &tilde -> &tilde;
				new Transition (8558, 8559), // &TildeEqual -> &TildeEqual;
				new Transition (8568, 8569), // &TildeFullEqual -> &TildeFullEqual;
				new Transition (8574, 8575), // &TildeTilde -> &TildeTilde;
				new Transition (8578, 8579), // &times -> &times;
				new Transition (8580, 8581), // &timesb -> &timesb;
				new Transition (8583, 8584), // &timesbar -> &timesbar;
				new Transition (8585, 8586), // &timesd -> &timesd;
				new Transition (8588, 8589), // &tint -> &tint;
				new Transition (8592, 8593), // &toea -> &toea;
				new Transition (8594, 8595), // &top -> &top;
				new Transition (8598, 8599), // &topbot -> &topbot;
				new Transition (8602, 8603), // &topcir -> &topcir;
				new Transition (8606, 8607), // &Topf -> &Topf;
				new Transition (8608, 8609), // &topf -> &topf;
				new Transition (8612, 8613), // &topfork -> &topfork;
				new Transition (8615, 8616), // &tosa -> &tosa;
				new Transition (8621, 8622), // &tprime -> &tprime;
				new Transition (8626, 8627), // &TRADE -> &TRADE;
				new Transition (8631, 8632), // &trade -> &trade;
				new Transition (8638, 8639), // &triangle -> &triangle;
				new Transition (8643, 8644), // &triangledown -> &triangledown;
				new Transition (8648, 8649), // &triangleleft -> &triangleleft;
				new Transition (8651, 8652), // &trianglelefteq -> &trianglelefteq;
				new Transition (8653, 8654), // &triangleq -> &triangleq;
				new Transition (8659, 8660), // &triangleright -> &triangleright;
				new Transition (8662, 8663), // &trianglerighteq -> &trianglerighteq;
				new Transition (8666, 8667), // &tridot -> &tridot;
				new Transition (8668, 8669), // &trie -> &trie;
				new Transition (8674, 8675), // &triminus -> &triminus;
				new Transition (8683, 8684), // &TripleDot -> &TripleDot;
				new Transition (8688, 8689), // &triplus -> &triplus;
				new Transition (8691, 8692), // &trisb -> &trisb;
				new Transition (8696, 8697), // &tritime -> &tritime;
				new Transition (8703, 8704), // &trpezium -> &trpezium;
				new Transition (8707, 8708), // &Tscr -> &Tscr;
				new Transition (8711, 8712), // &tscr -> &tscr;
				new Transition (8715, 8716), // &TScy -> &TScy;
				new Transition (8717, 8718), // &tscy -> &tscy;
				new Transition (8721, 8722), // &TSHcy -> &TSHcy;
				new Transition (8725, 8726), // &tshcy -> &tshcy;
				new Transition (8730, 8731), // &Tstrok -> &Tstrok;
				new Transition (8735, 8736), // &tstrok -> &tstrok;
				new Transition (8740, 8741), // &twixt -> &twixt;
				new Transition (8755, 8756), // &twoheadleftarrow -> &twoheadleftarrow;
				new Transition (8766, 8767), // &twoheadrightarrow -> &twoheadrightarrow;
				new Transition (8773, 8774), // &Uacute -> &Uacute;
				new Transition (8780, 8781), // &uacute -> &uacute;
				new Transition (8783, 8784), // &Uarr -> &Uarr;
				new Transition (8787, 8788), // &uArr -> &uArr;
				new Transition (8790, 8791), // &uarr -> &uarr;
				new Transition (8795, 8796), // &Uarrocir -> &Uarrocir;
				new Transition (8800, 8801), // &Ubrcy -> &Ubrcy;
				new Transition (8805, 8806), // &ubrcy -> &ubrcy;
				new Transition (8809, 8810), // &Ubreve -> &Ubreve;
				new Transition (8813, 8814), // &ubreve -> &ubreve;
				new Transition (8818, 8819), // &Ucirc -> &Ucirc;
				new Transition (8823, 8824), // &ucirc -> &ucirc;
				new Transition (8825, 8826), // &Ucy -> &Ucy;
				new Transition (8827, 8828), // &ucy -> &ucy;
				new Transition (8832, 8833), // &udarr -> &udarr;
				new Transition (8838, 8839), // &Udblac -> &Udblac;
				new Transition (8843, 8844), // &udblac -> &udblac;
				new Transition (8847, 8848), // &udhar -> &udhar;
				new Transition (8853, 8854), // &ufisht -> &ufisht;
				new Transition (8856, 8857), // &Ufr -> &Ufr;
				new Transition (8858, 8859), // &ufr -> &ufr;
				new Transition (8864, 8865), // &Ugrave -> &Ugrave;
				new Transition (8870, 8871), // &ugrave -> &ugrave;
				new Transition (8874, 8875), // &uHar -> &uHar;
				new Transition (8879, 8880), // &uharl -> &uharl;
				new Transition (8881, 8882), // &uharr -> &uharr;
				new Transition (8885, 8886), // &uhblk -> &uhblk;
				new Transition (8891, 8892), // &ulcorn -> &ulcorn;
				new Transition (8894, 8895), // &ulcorner -> &ulcorner;
				new Transition (8898, 8899), // &ulcrop -> &ulcrop;
				new Transition (8902, 8903), // &ultri -> &ultri;
				new Transition (8907, 8908), // &Umacr -> &Umacr;
				new Transition (8912, 8913), // &umacr -> &umacr;
				new Transition (8914, 8915), // &uml -> &uml;
				new Transition (8922, 8923), // &UnderBar -> &UnderBar;
				new Transition (8927, 8928), // &UnderBrace -> &UnderBrace;
				new Transition (8931, 8932), // &UnderBracket -> &UnderBracket;
				new Transition (8943, 8944), // &UnderParenthesis -> &UnderParenthesis;
				new Transition (8947, 8948), // &Union -> &Union;
				new Transition (8952, 8953), // &UnionPlus -> &UnionPlus;
				new Transition (8957, 8958), // &Uogon -> &Uogon;
				new Transition (8962, 8963), // &uogon -> &uogon;
				new Transition (8965, 8966), // &Uopf -> &Uopf;
				new Transition (8968, 8969), // &uopf -> &uopf;
				new Transition (8975, 8976), // &UpArrow -> &UpArrow;
				new Transition (8981, 8982), // &Uparrow -> &Uparrow;
				new Transition (8988, 8989), // &uparrow -> &uparrow;
				new Transition (8992, 8993), // &UpArrowBar -> &UpArrowBar;
				new Transition (9002, 9003), // &UpArrowDownArrow -> &UpArrowDownArrow;
				new Transition (9012, 9013), // &UpDownArrow -> &UpDownArrow;
				new Transition (9022, 9023), // &Updownarrow -> &Updownarrow;
				new Transition (9032, 9033), // &updownarrow -> &updownarrow;
				new Transition (9044, 9045), // &UpEquilibrium -> &UpEquilibrium;
				new Transition (9056, 9057), // &upharpoonleft -> &upharpoonleft;
				new Transition (9062, 9063), // &upharpoonright -> &upharpoonright;
				new Transition (9066, 9067), // &uplus -> &uplus;
				new Transition (9079, 9080), // &UpperLeftArrow -> &UpperLeftArrow;
				new Transition (9090, 9091), // &UpperRightArrow -> &UpperRightArrow;
				new Transition (9093, 9094), // &Upsi -> &Upsi;
				new Transition (9096, 9097), // &upsi -> &upsi;
				new Transition (9098, 9099), // &upsih -> &upsih;
				new Transition (9102, 9103), // &Upsilon -> &Upsilon;
				new Transition (9106, 9107), // &upsilon -> &upsilon;
				new Transition (9110, 9111), // &UpTee -> &UpTee;
				new Transition (9116, 9117), // &UpTeeArrow -> &UpTeeArrow;
				new Transition (9125, 9126), // &upuparrows -> &upuparrows;
				new Transition (9131, 9132), // &urcorn -> &urcorn;
				new Transition (9134, 9135), // &urcorner -> &urcorner;
				new Transition (9138, 9139), // &urcrop -> &urcrop;
				new Transition (9143, 9144), // &Uring -> &Uring;
				new Transition (9147, 9148), // &uring -> &uring;
				new Transition (9151, 9152), // &urtri -> &urtri;
				new Transition (9155, 9156), // &Uscr -> &Uscr;
				new Transition (9159, 9160), // &uscr -> &uscr;
				new Transition (9164, 9165), // &utdot -> &utdot;
				new Transition (9170, 9171), // &Utilde -> &Utilde;
				new Transition (9175, 9176), // &utilde -> &utilde;
				new Transition (9178, 9179), // &utri -> &utri;
				new Transition (9180, 9181), // &utrif -> &utrif;
				new Transition (9185, 9186), // &uuarr -> &uuarr;
				new Transition (9189, 9190), // &Uuml -> &Uuml;
				new Transition (9192, 9193), // &uuml -> &uuml;
				new Transition (9199, 9200), // &uwangle -> &uwangle;
				new Transition (9206, 9207), // &vangrt -> &vangrt;
				new Transition (9215, 9216), // &varepsilon -> &varepsilon;
				new Transition (9221, 9222), // &varkappa -> &varkappa;
				new Transition (9229, 9230), // &varnothing -> &varnothing;
				new Transition (9233, 9234), // &varphi -> &varphi;
				new Transition (9235, 9236), // &varpi -> &varpi;
				new Transition (9241, 9242), // &varpropto -> &varpropto;
				new Transition (9245, 9246), // &vArr -> &vArr;
				new Transition (9247, 9248), // &varr -> &varr;
				new Transition (9250, 9251), // &varrho -> &varrho;
				new Transition (9256, 9257), // &varsigma -> &varsigma;
				new Transition (9265, 9266), // &varsubsetneq -> &varsubsetneq;
				new Transition (9267, 9268), // &varsubsetneqq -> &varsubsetneqq;
				new Transition (9275, 9276), // &varsupsetneq -> &varsupsetneq;
				new Transition (9277, 9278), // &varsupsetneqq -> &varsupsetneqq;
				new Transition (9283, 9284), // &vartheta -> &vartheta;
				new Transition (9295, 9296), // &vartriangleleft -> &vartriangleleft;
				new Transition (9301, 9302), // &vartriangleright -> &vartriangleright;
				new Transition (9306, 9307), // &Vbar -> &Vbar;
				new Transition (9310, 9311), // &vBar -> &vBar;
				new Transition (9312, 9313), // &vBarv -> &vBarv;
				new Transition (9315, 9316), // &Vcy -> &Vcy;
				new Transition (9318, 9319), // &vcy -> &vcy;
				new Transition (9323, 9324), // &VDash -> &VDash;
				new Transition (9328, 9329), // &Vdash -> &Vdash;
				new Transition (9333, 9334), // &vDash -> &vDash;
				new Transition (9338, 9339), // &vdash -> &vdash;
				new Transition (9340, 9341), // &Vdashl -> &Vdashl;
				new Transition (9343, 9344), // &Vee -> &Vee;
				new Transition (9346, 9347), // &vee -> &vee;
				new Transition (9350, 9351), // &veebar -> &veebar;
				new Transition (9353, 9354), // &veeeq -> &veeeq;
				new Transition (9358, 9359), // &vellip -> &vellip;
				new Transition (9363, 9364), // &Verbar -> &Verbar;
				new Transition (9368, 9369), // &verbar -> &verbar;
				new Transition (9370, 9371), // &Vert -> &Vert;
				new Transition (9372, 9373), // &vert -> &vert;
				new Transition (9380, 9381), // &VerticalBar -> &VerticalBar;
				new Transition (9385, 9386), // &VerticalLine -> &VerticalLine;
				new Transition (9395, 9396), // &VerticalSeparator -> &VerticalSeparator;
				new Transition (9401, 9402), // &VerticalTilde -> &VerticalTilde;
				new Transition (9412, 9413), // &VeryThinSpace -> &VeryThinSpace;
				new Transition (9415, 9416), // &Vfr -> &Vfr;
				new Transition (9418, 9419), // &vfr -> &vfr;
				new Transition (9423, 9424), // &vltri -> &vltri;
				new Transition (9428, 9429), // &vnsub -> &vnsub;
				new Transition (9430, 9431), // &vnsup -> &vnsup;
				new Transition (9434, 9435), // &Vopf -> &Vopf;
				new Transition (9438, 9439), // &vopf -> &vopf;
				new Transition (9443, 9444), // &vprop -> &vprop;
				new Transition (9448, 9449), // &vrtri -> &vrtri;
				new Transition (9452, 9453), // &Vscr -> &Vscr;
				new Transition (9456, 9457), // &vscr -> &vscr;
				new Transition (9461, 9462), // &vsubnE -> &vsubnE;
				new Transition (9463, 9464), // &vsubne -> &vsubne;
				new Transition (9467, 9468), // &vsupnE -> &vsupnE;
				new Transition (9469, 9470), // &vsupne -> &vsupne;
				new Transition (9475, 9476), // &Vvdash -> &Vvdash;
				new Transition (9482, 9483), // &vzigzag -> &vzigzag;
				new Transition (9488, 9489), // &Wcirc -> &Wcirc;
				new Transition (9494, 9495), // &wcirc -> &wcirc;
				new Transition (9500, 9501), // &wedbar -> &wedbar;
				new Transition (9505, 9506), // &Wedge -> &Wedge;
				new Transition (9508, 9509), // &wedge -> &wedge;
				new Transition (9510, 9511), // &wedgeq -> &wedgeq;
				new Transition (9515, 9516), // &weierp -> &weierp;
				new Transition (9518, 9519), // &Wfr -> &Wfr;
				new Transition (9521, 9522), // &wfr -> &wfr;
				new Transition (9525, 9526), // &Wopf -> &Wopf;
				new Transition (9529, 9530), // &wopf -> &wopf;
				new Transition (9531, 9532), // &wp -> &wp;
				new Transition (9533, 9534), // &wr -> &wr;
				new Transition (9538, 9539), // &wreath -> &wreath;
				new Transition (9542, 9543), // &Wscr -> &Wscr;
				new Transition (9546, 9547), // &wscr -> &wscr;
				new Transition (9551, 9552), // &xcap -> &xcap;
				new Transition (9555, 9556), // &xcirc -> &xcirc;
				new Transition (9558, 9559), // &xcup -> &xcup;
				new Transition (9563, 9564), // &xdtri -> &xdtri;
				new Transition (9567, 9568), // &Xfr -> &Xfr;
				new Transition (9570, 9571), // &xfr -> &xfr;
				new Transition (9575, 9576), // &xhArr -> &xhArr;
				new Transition (9579, 9580), // &xharr -> &xharr;
				new Transition (9581, 9582), // &Xi -> &Xi;
				new Transition (9583, 9584), // &xi -> &xi;
				new Transition (9588, 9589), // &xlArr -> &xlArr;
				new Transition (9592, 9593), // &xlarr -> &xlarr;
				new Transition (9596, 9597), // &xmap -> &xmap;
				new Transition (9600, 9601), // &xnis -> &xnis;
				new Transition (9605, 9606), // &xodot -> &xodot;
				new Transition (9609, 9610), // &Xopf -> &Xopf;
				new Transition (9612, 9613), // &xopf -> &xopf;
				new Transition (9616, 9617), // &xoplus -> &xoplus;
				new Transition (9621, 9622), // &xotime -> &xotime;
				new Transition (9626, 9627), // &xrArr -> &xrArr;
				new Transition (9630, 9631), // &xrarr -> &xrarr;
				new Transition (9634, 9635), // &Xscr -> &Xscr;
				new Transition (9638, 9639), // &xscr -> &xscr;
				new Transition (9643, 9644), // &xsqcup -> &xsqcup;
				new Transition (9649, 9650), // &xuplus -> &xuplus;
				new Transition (9653, 9654), // &xutri -> &xutri;
				new Transition (9657, 9658), // &xvee -> &xvee;
				new Transition (9663, 9664), // &xwedge -> &xwedge;
				new Transition (9670, 9671), // &Yacute -> &Yacute;
				new Transition (9677, 9678), // &yacute -> &yacute;
				new Transition (9681, 9682), // &YAcy -> &YAcy;
				new Transition (9683, 9684), // &yacy -> &yacy;
				new Transition (9688, 9689), // &Ycirc -> &Ycirc;
				new Transition (9693, 9694), // &ycirc -> &ycirc;
				new Transition (9695, 9696), // &Ycy -> &Ycy;
				new Transition (9697, 9698), // &ycy -> &ycy;
				new Transition (9700, 9701), // &yen -> &yen;
				new Transition (9703, 9704), // &Yfr -> &Yfr;
				new Transition (9706, 9707), // &yfr -> &yfr;
				new Transition (9710, 9711), // &YIcy -> &YIcy;
				new Transition (9714, 9715), // &yicy -> &yicy;
				new Transition (9718, 9719), // &Yopf -> &Yopf;
				new Transition (9722, 9723), // &yopf -> &yopf;
				new Transition (9726, 9727), // &Yscr -> &Yscr;
				new Transition (9730, 9731), // &yscr -> &yscr;
				new Transition (9734, 9735), // &YUcy -> &YUcy;
				new Transition (9738, 9739), // &yucy -> &yucy;
				new Transition (9742, 9743), // &Yuml -> &Yuml;
				new Transition (9745, 9746), // &yuml -> &yuml;
				new Transition (9752, 9753), // &Zacute -> &Zacute;
				new Transition (9759, 9760), // &zacute -> &zacute;
				new Transition (9765, 9766), // &Zcaron -> &Zcaron;
				new Transition (9771, 9772), // &zcaron -> &zcaron;
				new Transition (9773, 9774), // &Zcy -> &Zcy;
				new Transition (9775, 9776), // &zcy -> &zcy;
				new Transition (9779, 9780), // &Zdot -> &Zdot;
				new Transition (9783, 9784), // &zdot -> &zdot;
				new Transition (9789, 9790), // &zeetrf -> &zeetrf;
				new Transition (9803, 9804), // &ZeroWidthSpace -> &ZeroWidthSpace;
				new Transition (9806, 9807), // &Zeta -> &Zeta;
				new Transition (9809, 9810), // &zeta -> &zeta;
				new Transition (9812, 9813), // &Zfr -> &Zfr;
				new Transition (9815, 9816), // &zfr -> &zfr;
				new Transition (9819, 9820), // &ZHcy -> &ZHcy;
				new Transition (9823, 9824), // &zhcy -> &zhcy;
				new Transition (9830, 9831), // &zigrarr -> &zigrarr;
				new Transition (9834, 9835), // &Zopf -> &Zopf;
				new Transition (9838, 9839), // &zopf -> &zopf;
				new Transition (9842, 9843), // &Zscr -> &Zscr;
				new Transition (9846, 9847), // &zscr -> &zscr;
				new Transition (9849, 9850), // &zwj -> &zwj;
				new Transition (9852, 9853) // &zwnj -> &zwnj;
			};
			TransitionTable_A = new Transition[59] {
				new Transition (0, 1), // & -> &A
				new Transition (1432, 1447), // &d -> &dA
				new Transition (1566, 1567), // &Diacritical -> &DiacriticalA
				new Transition (1580, 1581), // &DiacriticalDouble -> &DiacriticalDoubleA
				new Transition (1769, 1770), // &DoubleDown -> &DoubleDownA
				new Transition (1779, 1780), // &DoubleLeft -> &DoubleLeftA
				new Transition (1790, 1791), // &DoubleLeftRight -> &DoubleLeftRightA
				new Transition (1807, 1808), // &DoubleLongLeft -> &DoubleLongLeftA
				new Transition (1818, 1819), // &DoubleLongLeftRight -> &DoubleLongLeftRightA
				new Transition (1829, 1830), // &DoubleLongRight -> &DoubleLongRightA
				new Transition (1840, 1841), // &DoubleRight -> &DoubleRightA
				new Transition (1852, 1853), // &DoubleUp -> &DoubleUpA
				new Transition (1862, 1863), // &DoubleUpDown -> &DoubleUpDownA
				new Transition (1882, 1883), // &Down -> &DownA
				new Transition (1908, 1909), // &DownArrowUp -> &DownArrowUpA
				new Transition (2015, 2017), // &DownTee -> &DownTeeA
				new Transition (2616, 2617), // &For -> &ForA
				new Transition (3014, 3035), // &H -> &HA
				new Transition (3020, 3046), // &h -> &hA
				new Transition (3692, 3693), // &l -> &lA
				new Transition (3900, 3901), // &Left -> &LeftA
				new Transition (3941, 3942), // &LeftArrowRight -> &LeftArrowRightA
				new Transition (4034, 4035), // &LeftRight -> &LeftRightA
				new Transition (4094, 4096), // &LeftTee -> &LeftTeeA
				new Transition (4440, 4441), // &LongLeft -> &LongLeftA
				new Transition (4473, 4474), // &LongLeftRight -> &LongLeftRightA
				new Transition (4513, 4514), // &LongRight -> &LongRightA
				new Transition (4594, 4595), // &LowerLeft -> &LowerLeftA
				new Transition (4605, 4606), // &LowerRight -> &LowerRightA
				new Transition (5064, 5071), // &ne -> &neA
				new Transition (5227, 5228), // &nh -> &nhA
				new Transition (5256, 5257), // &nl -> &nlA
				new Transition (5855, 5856), // &nr -> &nrA
				new Transition (6084, 6085), // &nvl -> &nvlA
				new Transition (6097, 6098), // &nvr -> &nvrA
				new Transition (6111, 6117), // &nw -> &nwA
				new Transition (6876, 6877), // &r -> &rA
				new Transition (7174, 7175), // &Right -> &RightA
				new Transition (7216, 7217), // &RightArrowLeft -> &RightArrowLeftA
				new Transition (7339, 7341), // &RightTee -> &RightTeeA
				new Transition (7703, 7709), // &se -> &seA
				new Transition (7779, 7780), // &ShortDown -> &ShortDownA
				new Transition (7789, 7790), // &ShortLeft -> &ShortLeftA
				new Transition (7816, 7817), // &ShortRight -> &ShortRightA
				new Transition (7824, 7825), // &ShortUp -> &ShortUpA
				new Transition (8375, 8381), // &sw -> &swA
				new Transition (8623, 8624), // &TR -> &TRA
				new Transition (8775, 8785), // &u -> &uA
				new Transition (8970, 8971), // &Up -> &UpA
				new Transition (8997, 8998), // &UpArrowDown -> &UpArrowDownA
				new Transition (9007, 9008), // &UpDown -> &UpDownA
				new Transition (9074, 9075), // &UpperLeft -> &UpperLeftA
				new Transition (9085, 9086), // &UpperRight -> &UpperRightA
				new Transition (9110, 9112), // &UpTee -> &UpTeeA
				new Transition (9201, 9243), // &v -> &vA
				new Transition (9572, 9573), // &xh -> &xhA
				new Transition (9585, 9586), // &xl -> &xlA
				new Transition (9623, 9624), // &xr -> &xrA
				new Transition (9665, 9679) // &Y -> &YA
			};
			TransitionTable_B = new Transition[34] {
				new Transition (0, 331), // & -> &B
				new Transition (1876, 1877), // &DoubleVertical -> &DoubleVerticalB
				new Transition (1882, 1915), // &Down -> &DownB
				new Transition (1887, 1903), // &DownArrow -> &DownArrowB
				new Transition (1981, 1983), // &DownLeftVector -> &DownLeftVectorB
				new Transition (2007, 2009), // &DownRightVector -> &DownRightVectorB
				new Transition (3692, 3807), // &l -> &lB
				new Transition (3905, 3906), // &LeftAngle -> &LeftAngleB
				new Transition (3917, 3933), // &LeftArrow -> &LeftArrowB
				new Transition (3966, 3967), // &LeftDouble -> &LeftDoubleB
				new Transition (3992, 3994), // &LeftDownVector -> &LeftDownVectorB
				new Transition (4126, 4128), // &LeftTriangle -> &LeftTriangleB
				new Transition (4166, 4168), // &LeftUpVector -> &LeftUpVectorB
				new Transition (4177, 4179), // &LeftVector -> &LeftVectorB
				new Transition (5347, 5348), // &No -> &NoB
				new Transition (5354, 5355), // &Non -> &NonB
				new Transition (5409, 5410), // &NotDoubleVertical -> &NotDoubleVerticalB
				new Transition (5539, 5541), // &NotLeftTriangle -> &NotLeftTriangleB
				new Transition (5682, 5684), // &NotRightTriangle -> &NotRightTriangleB
				new Transition (5816, 5817), // &NotVertical -> &NotVerticalB
				new Transition (6437, 6438), // &Over -> &OverB
				new Transition (6876, 6991), // &r -> &rB
				new Transition (6886, 6986), // &R -> &RB
				new Transition (7179, 7180), // &RightAngle -> &RightAngleB
				new Transition (7191, 7209), // &RightArrow -> &RightArrowB
				new Transition (7241, 7242), // &RightDouble -> &RightDoubleB
				new Transition (7267, 7269), // &RightDownVector -> &RightDownVectorB
				new Transition (7371, 7373), // &RightTriangle -> &RightTriangleB
				new Transition (7411, 7413), // &RightUpVector -> &RightUpVectorB
				new Transition (7422, 7424), // &RightVector -> &RightVectorB
				new Transition (8919, 8920), // &Under -> &UnderB
				new Transition (8975, 8990), // &UpArrow -> &UpArrowB
				new Transition (9201, 9308), // &v -> &vB
				new Transition (9377, 9378) // &Vertical -> &VerticalB
			};
			TransitionTable_C = new Transition[15] {
				new Transition (0, 789), // & -> &C
				new Transition (1075, 1076), // &Clockwise -> &ClockwiseC
				new Transition (1093, 1094), // &Close -> &CloseC
				new Transition (1230, 1231), // &Counter -> &CounterC
				new Transition (1239, 1240), // &CounterClockwise -> &CounterClockwiseC
				new Transition (1316, 1326), // &Cup -> &CupC
				new Transition (1747, 1748), // &Double -> &DoubleC
				new Transition (3450, 3451), // &Invisible -> &InvisibleC
				new Transition (3900, 3953), // &Left -> &LeftC
				new Transition (5376, 5380), // &Not -> &NotC
				new Transition (5391, 5392), // &NotCup -> &NotCupC
				new Transition (6308, 6309), // &Open -> &OpenC
				new Transition (7174, 7228), // &Right -> &RightC
				new Transition (7756, 7757), // &SH -> &SHC
				new Transition (7886, 7887) // &Small -> &SmallC
			};
			TransitionTable_D = new Transition[43] {
				new Transition (0, 1425), // & -> &D
				new Transition (613, 618), // &box -> &boxD
				new Transition (636, 640), // &boxH -> &boxHD
				new Transition (638, 644), // &boxh -> &boxhD
				new Transition (831, 832), // &Capital -> &CapitalD
				new Transition (843, 844), // &CapitalDifferential -> &CapitalDifferentialD
				new Transition (939, 940), // &Center -> &CenterD
				new Transition (1023, 1024), // &Circle -> &CircleD
				new Transition (1098, 1099), // &CloseCurly -> &CloseCurlyD
				new Transition (1425, 1490), // &D -> &DD
				new Transition (1566, 1573), // &Diacritical -> &DiacriticalD
				new Transition (1630, 1631), // &Differential -> &DifferentialD
				new Transition (1692, 1696), // &Dot -> &DotD
				new Transition (1747, 1764), // &Double -> &DoubleD
				new Transition (1852, 1859), // &DoubleUp -> &DoubleUpD
				new Transition (2115, 2157), // &e -> &eD
				new Transition (2157, 2158), // &eD -> &eDD
				new Transition (2175, 2176), // &ef -> &efD
				new Transition (2397, 2399), // &equiv -> &equivD
				new Transition (2399, 2400), // &equivD -> &equivDD
				new Transition (2409, 2414), // &er -> &erD
				new Transition (3036, 3037), // &HAR -> &HARD
				new Transition (3209, 3210), // &Hump -> &HumpD
				new Transition (3900, 3961), // &Left -> &LeftD
				new Transition (4139, 4140), // &LeftUp -> &LeftUpD
				new Transition (4767, 4825), // &m -> &mD
				new Transition (4825, 4826), // &mD -> &mDD
				new Transition (5376, 5396), // &Not -> &NotD
				new Transition (5496, 5497), // &NotHump -> &NotHumpD
				new Transition (6043, 6058), // &nv -> &nvD
				new Transition (6047, 6048), // &nV -> &nVD
				new Transition (6313, 6314), // &OpenCurly -> &OpenCurlyD
				new Transition (6488, 6489), // &Partial -> &PartialD
				new Transition (7174, 7236), // &Right -> &RightD
				new Transition (7384, 7385), // &RightUp -> &RightUpD
				new Transition (7592, 7593), // &Rule -> &RuleD
				new Transition (7775, 7776), // &Short -> &ShortD
				new Transition (8624, 8625), // &TRA -> &TRAD
				new Transition (8680, 8681), // &Triple -> &TripleD
				new Transition (8970, 9004), // &Up -> &UpD
				new Transition (8975, 8994), // &UpArrow -> &UpArrowD
				new Transition (9201, 9330), // &v -> &vD
				new Transition (9303, 9320) // &V -> &VD
			};
			TransitionTable_E = new Transition[81] {
				new Transition (0, 2108), // & -> &E
				new Transition (1, 50), // &A -> &AE
				new Transition (27, 31), // &ac -> &acE
				new Transition (199, 206), // &ap -> &apE
				new Transition (775, 777), // &bump -> &bumpE
				new Transition (979, 1049), // &cir -> &cirE
				new Transition (1692, 1707), // &Dot -> &DotE
				new Transition (2490, 2491), // &Exponential -> &ExponentialE
				new Transition (2701, 2763), // &g -> &gE
				new Transition (2824, 2828), // &gl -> &glE
				new Transition (2832, 2841), // &gn -> &gnE
				new Transition (2871, 2872), // &Greater -> &GreaterE
				new Transition (2886, 2887), // &GreaterFull -> &GreaterFullE
				new Transition (2910, 2911), // &GreaterSlant -> &GreaterSlantE
				new Transition (3011, 3012), // &gvn -> &gvnE
				new Transition (3209, 3219), // &Hump -> &HumpE
				new Transition (3236, 3269), // &I -> &IE
				new Transition (3512, 3518), // &isin -> &isinE
				new Transition (3692, 3894), // &l -> &lE
				new Transition (4126, 4132), // &LeftTriangle -> &LeftTriangleE
				new Transition (4239, 4240), // &Less -> &LessE
				new Transition (4256, 4257), // &LessFull -> &LessFullE
				new Transition (4288, 4289), // &LessSlant -> &LessSlantE
				new Transition (4317, 4319), // &lg -> &lgE
				new Transition (4401, 4410), // &ln -> &lnE
				new Transition (4764, 4765), // &lvn -> &lvnE
				new Transition (4986, 4988), // &nap -> &napE
				new Transition (5195, 5196), // &ng -> &ngE
				new Transition (5256, 5268), // &nl -> &nlE
				new Transition (5376, 5414), // &Not -> &NotE
				new Transition (5445, 5447), // &NotGreater -> &NotGreaterE
				new Transition (5456, 5457), // &NotGreaterFull -> &NotGreaterFullE
				new Transition (5480, 5481), // &NotGreaterSlant -> &NotGreaterSlantE
				new Transition (5496, 5506), // &NotHump -> &NotHumpE
				new Transition (5513, 5519), // &notin -> &notinE
				new Transition (5539, 5545), // &NotLeftTriangle -> &NotLeftTriangleE
				new Transition (5552, 5554), // &NotLess -> &NotLessE
				new Transition (5577, 5578), // &NotLessSlant -> &NotLessSlantE
				new Transition (5637, 5639), // &NotPrecedes -> &NotPrecedesE
				new Transition (5649, 5650), // &NotPrecedesSlant -> &NotPrecedesSlantE
				new Transition (5662, 5663), // &NotReverse -> &NotReverseE
				new Transition (5682, 5688), // &NotRightTriangle -> &NotRightTriangleE
				new Transition (5705, 5707), // &NotSquareSubset -> &NotSquareSubsetE
				new Transition (5718, 5720), // &NotSquareSuperset -> &NotSquareSupersetE
				new Transition (5730, 5732), // &NotSubset -> &NotSubsetE
				new Transition (5743, 5745), // &NotSucceeds -> &NotSucceedsE
				new Transition (5755, 5756), // &NotSucceedsSlant -> &NotSucceedsSlantE
				new Transition (5773, 5775), // &NotSuperset -> &NotSupersetE
				new Transition (5785, 5787), // &NotTilde -> &NotTildeE
				new Transition (5796, 5797), // &NotTildeFull -> &NotTildeFullE
				new Transition (5952, 5954), // &nsub -> &nsubE
				new Transition (5973, 5975), // &nsup -> &nsupE
				new Transition (6131, 6190), // &O -> &OE
				new Transition (6642, 6651), // &pr -> &prE
				new Transition (6677, 6679), // &Precedes -> &PrecedesE
				new Transition (6689, 6690), // &PrecedesSlant -> &PrecedesSlantE
				new Transition (6735, 6739), // &prn -> &prnE
				new Transition (6886, 7092), // &R -> &RE
				new Transition (7101, 7102), // &Reverse -> &ReverseE
				new Transition (7122, 7123), // &ReverseUp -> &ReverseUpE
				new Transition (7371, 7377), // &RightTriangle -> &RightTriangleE
				new Transition (7631, 7649), // &sc -> &scE
				new Transition (7670, 7674), // &scn -> &scnE
				new Transition (7857, 7859), // &simg -> &simgE
				new Transition (7861, 7863), // &siml -> &simlE
				new Transition (8037, 8039), // &SquareSubset -> &SquareSubsetE
				new Transition (8050, 8052), // &SquareSuperset -> &SquareSupersetE
				new Transition (8131, 8137), // &sub -> &subE
				new Transition (8150, 8151), // &subn -> &subnE
				new Transition (8167, 8178), // &Subset -> &SubsetE
				new Transition (8221, 8223), // &Succeeds -> &SucceedsE
				new Transition (8233, 8234), // &SucceedsSlant -> &SucceedsSlantE
				new Transition (8284, 8300), // &sup -> &supE
				new Transition (8312, 8314), // &Superset -> &SupersetE
				new Transition (8338, 8339), // &supn -> &supnE
				new Transition (8547, 8554), // &Tilde -> &TildeE
				new Transition (8563, 8564), // &TildeFull -> &TildeFullE
				new Transition (8625, 8626), // &TRAD -> &TRADE
				new Transition (8970, 9034), // &Up -> &UpE
				new Transition (9460, 9461), // &vsubn -> &vsubnE
				new Transition (9466, 9467) // &vsupn -> &vsupnE
			};
			TransitionTable_F = new Transition[10] {
				new Transition (0, 2517), // & -> &F
				new Transition (219, 220), // &Apply -> &ApplyF
				new Transition (2871, 2883), // &Greater -> &GreaterF
				new Transition (3900, 3998), // &Left -> &LeftF
				new Transition (4239, 4253), // &Less -> &LessF
				new Transition (5445, 5453), // &NotGreater -> &NotGreaterF
				new Transition (5785, 5793), // &NotTilde -> &NotTildeF
				new Transition (7174, 7273), // &Right -> &RightF
				new Transition (7930, 7931), // &SO -> &SOF
				new Transition (8547, 8560) // &Tilde -> &TildeF
			};
			TransitionTable_G = new Transition[15] {
				new Transition (0, 2708), // & -> &G
				new Transition (1566, 1587), // &Diacritical -> &DiacriticalG
				new Transition (2287, 2288), // &EN -> &ENG
				new Transition (2871, 2893), // &Greater -> &GreaterG
				new Transition (4239, 4263), // &Less -> &LessG
				new Transition (4244, 4245), // &LessEqual -> &LessEqualG
				new Transition (4965, 5212), // &n -> &nG
				new Transition (5151, 5152), // &Nested -> &NestedG
				new Transition (5158, 5159), // &NestedGreater -> &NestedGreaterG
				new Transition (5376, 5439), // &Not -> &NotG
				new Transition (5445, 5463), // &NotGreater -> &NotGreaterG
				new Transition (5552, 5560), // &NotLess -> &NotLessG
				new Transition (5595, 5596), // &NotNested -> &NotNestedG
				new Transition (5602, 5603), // &NotNestedGreater -> &NotNestedGreaterG
				new Transition (7092, 7093) // &RE -> &REG
			};
			TransitionTable_H = new Transition[20] {
				new Transition (0, 3014), // & -> &H
				new Transition (613, 636), // &box -> &boxH
				new Transition (691, 695), // &boxV -> &boxVH
				new Transition (693, 699), // &boxv -> &boxvH
				new Transition (789, 956), // &C -> &CH
				new Transition (1432, 1546), // &d -> &dH
				new Transition (2442, 2443), // &ET -> &ETH
				new Transition (3213, 3214), // &HumpDown -> &HumpDownH
				new Transition (3618, 3660), // &K -> &KH
				new Transition (3692, 4321), // &l -> &lH
				new Transition (5376, 5493), // &Not -> &NotH
				new Transition (5500, 5501), // &NotHumpDown -> &NotHumpDownH
				new Transition (6043, 6073), // &nv -> &nvH
				new Transition (6876, 7151), // &r -> &rH
				new Transition (7610, 7756), // &S -> &SH
				new Transition (7757, 7758), // &SHC -> &SHCH
				new Transition (8400, 8535), // &T -> &TH
				new Transition (8713, 8719), // &TS -> &TSH
				new Transition (8775, 8872), // &u -> &uH
				new Transition (9747, 9817) // &Z -> &ZH
			};
			TransitionTable_I = new Transition[9] {
				new Transition (0, 3236), // & -> &I
				new Transition (1082, 1083), // &ClockwiseContour -> &ClockwiseContourI
				new Transition (1190, 1191), // &Contour -> &ContourI
				new Transition (1246, 1247), // &CounterClockwiseContour -> &CounterClockwiseContourI
				new Transition (1754, 1755), // &DoubleContour -> &DoubleContourI
				new Transition (3349, 3350), // &Imaginary -> &ImaginaryI
				new Transition (7503, 7504), // &Round -> &RoundI
				new Transition (8013, 8019), // &Square -> &SquareI
				new Transition (9665, 9708) // &Y -> &YI
			};
			TransitionTable_J = new Transition[7] {
				new Transition (0, 3555), // & -> &J
				new Transition (1425, 1661), // &D -> &DJ
				new Transition (2708, 2816), // &G -> &GJ
				new Transition (3236, 3320), // &I -> &IJ
				new Transition (3618, 3668), // &K -> &KJ
				new Transition (3698, 4338), // &L -> &LJ
				new Transition (4971, 5248) // &N -> &NJ
			};
			TransitionTable_K = new Transition[1] {
				new Transition (0, 3618) // & -> &K
			};
			TransitionTable_L = new Transition[29] {
				new Transition (0, 3698), // & -> &L
				new Transition (618, 619), // &boxD -> &boxDL
				new Transition (623, 624), // &boxd -> &boxdL
				new Transition (673, 674), // &boxU -> &boxUL
				new Transition (678, 679), // &boxu -> &boxuL
				new Transition (691, 703), // &boxV -> &boxVL
				new Transition (693, 707), // &boxv -> &boxvL
				new Transition (1747, 1776), // &Double -> &DoubleL
				new Transition (1803, 1804), // &DoubleLong -> &DoubleLongL
				new Transition (1882, 1950), // &Down -> &DownL
				new Transition (2871, 2901), // &Greater -> &GreaterL
				new Transition (2876, 2878), // &GreaterEqual -> &GreaterEqualL
				new Transition (3178, 3179), // &Horizontal -> &HorizontalL
				new Transition (4239, 4275), // &Less -> &LessL
				new Transition (4436, 4437), // &Long -> &LongL
				new Transition (4590, 4591), // &Lower -> &LowerL
				new Transition (4965, 5272), // &n -> &nL
				new Transition (5151, 5167), // &Nested -> &NestedL
				new Transition (5170, 5171), // &NestedLess -> &NestedLessL
				new Transition (5176, 5177), // &New -> &NewL
				new Transition (5376, 5528), // &Not -> &NotL
				new Transition (5445, 5471), // &NotGreater -> &NotGreaterL
				new Transition (5552, 5568), // &NotLess -> &NotLessL
				new Transition (5595, 5611), // &NotNested -> &NotNestedL
				new Transition (5614, 5615), // &NotNestedLess -> &NotNestedLessL
				new Transition (7191, 7213), // &RightArrow -> &RightArrowL
				new Transition (7775, 7786), // &Short -> &ShortL
				new Transition (9070, 9071), // &Upper -> &UpperL
				new Transition (9377, 9382) // &Vertical -> &VerticalL
			};
			TransitionTable_M = new Transition[5] {
				new Transition (0, 4781), // & -> &M
				new Transition (1, 111), // &A -> &AM
				new Transition (1023, 1032), // &Circle -> &CircleM
				new Transition (5090, 5091), // &Negative -> &NegativeM
				new Transition (6589, 6590) // &Plus -> &PlusM
			};
			TransitionTable_N = new Transition[5] {
				new Transition (0, 4971), // & -> &N
				new Transition (301, 587), // &b -> &bN
				new Transition (2108, 2287), // &E -> &EN
				new Transition (5376, 5590), // &Not -> &NotN
				new Transition (8537, 8538) // &THOR -> &THORN
			};
			TransitionTable_O = new Transition[6] {
				new Transition (0, 6131), // & -> &O
				new Transition (789, 1217), // &C -> &CO
				new Transition (3236, 3463), // &I -> &IO
				new Transition (6869, 6870), // &QU -> &QUO
				new Transition (7610, 7930), // &S -> &SO
				new Transition (8535, 8536) // &TH -> &THO
			};
			TransitionTable_P = new Transition[11] {
				new Transition (0, 6482), // & -> &P
				new Transition (111, 112), // &AM -> &AMP
				new Transition (1023, 1038), // &Circle -> &CircleP
				new Transition (1217, 1218), // &CO -> &COP
				new Transition (2954, 2955), // &gtl -> &gtlP
				new Transition (4731, 4738), // &ltr -> &ltrP
				new Transition (4903, 4904), // &Minus -> &MinusP
				new Transition (5376, 5630), // &Not -> &NotP
				new Transition (6437, 6451), // &Over -> &OverP
				new Transition (8919, 8933), // &Under -> &UnderP
				new Transition (8947, 8949) // &Union -> &UnionP
			};
			TransitionTable_Q = new Transition[5] {
				new Transition (0, 6813), // & -> &Q
				new Transition (1098, 1111), // &CloseCurly -> &CloseCurlyQ
				new Transition (1104, 1105), // &CloseCurlyDouble -> &CloseCurlyDoubleQ
				new Transition (6313, 6326), // &OpenCurly -> &OpenCurlyQ
				new Transition (6319, 6320) // &OpenCurlyDouble -> &OpenCurlyDoubleQ
			};
			TransitionTable_R = new Transition[26] {
				new Transition (0, 6886), // & -> &R
				new Transition (618, 628), // &boxD -> &boxDR
				new Transition (623, 632), // &boxd -> &boxdR
				new Transition (673, 683), // &boxU -> &boxUR
				new Transition (678, 687), // &boxu -> &boxuR
				new Transition (691, 711), // &boxV -> &boxVR
				new Transition (693, 715), // &boxv -> &boxvR
				new Transition (1004, 1028), // &circled -> &circledR
				new Transition (1747, 1836), // &Double -> &DoubleR
				new Transition (1779, 1786), // &DoubleLeft -> &DoubleLeftR
				new Transition (1803, 1825), // &DoubleLong -> &DoubleLongR
				new Transition (1807, 1814), // &DoubleLongLeft -> &DoubleLongLeftR
				new Transition (1882, 1987), // &Down -> &DownR
				new Transition (1953, 1954), // &DownLeft -> &DownLeftR
				new Transition (3035, 3036), // &HA -> &HAR
				new Transition (3900, 4030), // &Left -> &LeftR
				new Transition (3917, 3937), // &LeftArrow -> &LeftArrowR
				new Transition (4436, 4509), // &Long -> &LongR
				new Transition (4440, 4469), // &LongLeft -> &LongLeftR
				new Transition (4590, 4601), // &Lower -> &LowerR
				new Transition (4965, 5868), // &n -> &nR
				new Transition (5376, 5656), // &Not -> &NotR
				new Transition (7775, 7812), // &Short -> &ShortR
				new Transition (8400, 8623), // &T -> &TR
				new Transition (8536, 8537), // &THO -> &THOR
				new Transition (9070, 9081) // &Upper -> &UpperR
			};
			TransitionTable_S = new Transition[36] {
				new Transition (0, 7610), // & -> &S
				new Transition (1004, 1030), // &circled -> &circledS
				new Transition (1425, 2048), // &D -> &DS
				new Transition (2248, 2249), // &Empty -> &EmptyS
				new Transition (2253, 2254), // &EmptySmall -> &EmptySmallS
				new Transition (2266, 2267), // &EmptyVery -> &EmptyVeryS
				new Transition (2271, 2272), // &EmptyVerySmall -> &EmptyVerySmallS
				new Transition (2558, 2559), // &Filled -> &FilledS
				new Transition (2563, 2564), // &FilledSmall -> &FilledSmallS
				new Transition (2574, 2575), // &FilledVery -> &FilledVeryS
				new Transition (2579, 2580), // &FilledVerySmall -> &FilledVerySmallS
				new Transition (2871, 2906), // &Greater -> &GreaterS
				new Transition (3105, 3106), // &Hilbert -> &HilbertS
				new Transition (4239, 4284), // &Less -> &LessS
				new Transition (4847, 4848), // &Medium -> &MediumS
				new Transition (5096, 5097), // &NegativeMedium -> &NegativeMediumS
				new Transition (5107, 5108), // &NegativeThick -> &NegativeThickS
				new Transition (5114, 5115), // &NegativeThin -> &NegativeThinS
				new Transition (5128, 5129), // &NegativeVeryThin -> &NegativeVeryThinS
				new Transition (5362, 5363), // &NonBreaking -> &NonBreakingS
				new Transition (5376, 5694), // &Not -> &NotS
				new Transition (5445, 5476), // &NotGreater -> &NotGreaterS
				new Transition (5552, 5573), // &NotLess -> &NotLessS
				new Transition (5637, 5645), // &NotPrecedes -> &NotPrecedesS
				new Transition (5699, 5700), // &NotSquare -> &NotSquareS
				new Transition (5743, 5751), // &NotSucceeds -> &NotSucceedsS
				new Transition (6138, 6376), // &o -> &oS
				new Transition (6677, 6685), // &Precedes -> &PrecedesS
				new Transition (8013, 8032), // &Square -> &SquareS
				new Transition (8221, 8229), // &Succeeds -> &SucceedsS
				new Transition (8400, 8713), // &T -> &TS
				new Transition (8509, 8510), // &Thick -> &ThickS
				new Transition (8520, 8521), // &Thin -> &ThinS
				new Transition (9377, 9387), // &Vertical -> &VerticalS
				new Transition (9407, 9408), // &VeryThin -> &VeryThinS
				new Transition (9798, 9799) // &ZeroWidth -> &ZeroWidthS
			};
			TransitionTable_T = new Transition[40] {
				new Transition (0, 8400), // & -> &T
				new Transition (1023, 1043), // &Circle -> &CircleT
				new Transition (1566, 1593), // &Diacritical -> &DiacriticalT
				new Transition (1779, 1797), // &DoubleLeft -> &DoubleLeftT
				new Transition (1840, 1847), // &DoubleRight -> &DoubleRightT
				new Transition (1882, 2013), // &Down -> &DownT
				new Transition (1953, 1966), // &DownLeft -> &DownLeftT
				new Transition (1991, 1992), // &DownRight -> &DownRightT
				new Transition (2108, 2442), // &E -> &ET
				new Transition (2370, 2377), // &Equal -> &EqualT
				new Transition (2708, 2938), // &G -> &GT
				new Transition (2871, 2917), // &Greater -> &GreaterT
				new Transition (3450, 3457), // &Invisible -> &InvisibleT
				new Transition (3698, 4694), // &L -> &LT
				new Transition (3900, 4092), // &Left -> &LeftT
				new Transition (3976, 3977), // &LeftDown -> &LeftDownT
				new Transition (4139, 4151), // &LeftUp -> &LeftUpT
				new Transition (4239, 4295), // &Less -> &LessT
				new Transition (5090, 5103), // &Negative -> &NegativeT
				new Transition (5124, 5125), // &NegativeVery -> &NegativeVeryT
				new Transition (5376, 5781), // &Not -> &NotT
				new Transition (5425, 5427), // &NotEqual -> &NotEqualT
				new Transition (5445, 5487), // &NotGreater -> &NotGreaterT
				new Transition (5531, 5532), // &NotLeft -> &NotLeftT
				new Transition (5552, 5584), // &NotLess -> &NotLessT
				new Transition (5674, 5675), // &NotRight -> &NotRightT
				new Transition (5743, 5762), // &NotSucceeds -> &NotSucceedsT
				new Transition (5785, 5803), // &NotTilde -> &NotTildeT
				new Transition (6677, 6696), // &Precedes -> &PrecedesT
				new Transition (6870, 6871), // &QUO -> &QUOT
				new Transition (7174, 7337), // &Right -> &RightT
				new Transition (7251, 7252), // &RightDown -> &RightDownT
				new Transition (7384, 7396), // &RightUp -> &RightUpT
				new Transition (7931, 7932), // &SOF -> &SOFT
				new Transition (8221, 8240), // &Succeeds -> &SucceedsT
				new Transition (8269, 8270), // &Such -> &SuchT
				new Transition (8547, 8570), // &Tilde -> &TildeT
				new Transition (8970, 9108), // &Up -> &UpT
				new Transition (9377, 9397), // &Vertical -> &VerticalT
				new Transition (9403, 9404) // &Very -> &VeryT
			};
			TransitionTable_U = new Transition[13] {
				new Transition (0, 8768), // & -> &U
				new Transition (613, 673), // &box -> &boxU
				new Transition (636, 648), // &boxH -> &boxHU
				new Transition (638, 652), // &boxh -> &boxhU
				new Transition (1747, 1851), // &Double -> &DoubleU
				new Transition (1887, 1907), // &DownArrow -> &DownArrowU
				new Transition (3900, 4138), // &Left -> &LeftU
				new Transition (6813, 6869), // &Q -> &QU
				new Transition (7101, 7121), // &Reverse -> &ReverseU
				new Transition (7174, 7383), // &Right -> &RightU
				new Transition (7775, 7823), // &Short -> &ShortU
				new Transition (8013, 8058), // &Square -> &SquareU
				new Transition (9665, 9732) // &Y -> &YU
			};
			TransitionTable_V = new Transition[29] {
				new Transition (0, 9303), // & -> &V
				new Transition (613, 691), // &box -> &boxV
				new Transition (1747, 1869), // &Double -> &DoubleV
				new Transition (1953, 1976), // &DownLeft -> &DownLeftV
				new Transition (1958, 1959), // &DownLeftRight -> &DownLeftRightV
				new Transition (1968, 1969), // &DownLeftTee -> &DownLeftTeeV
				new Transition (1991, 2002), // &DownRight -> &DownRightV
				new Transition (1994, 1995), // &DownRightTee -> &DownRightTeeV
				new Transition (2248, 2263), // &Empty -> &EmptyV
				new Transition (2558, 2571), // &Filled -> &FilledV
				new Transition (3900, 4172), // &Left -> &LeftV
				new Transition (3976, 3987), // &LeftDown -> &LeftDownV
				new Transition (3979, 3980), // &LeftDownTee -> &LeftDownTeeV
				new Transition (4034, 4085), // &LeftRight -> &LeftRightV
				new Transition (4094, 4102), // &LeftTee -> &LeftTeeV
				new Transition (4139, 4161), // &LeftUp -> &LeftUpV
				new Transition (4143, 4144), // &LeftUpDown -> &LeftUpDownV
				new Transition (4153, 4154), // &LeftUpTee -> &LeftUpTeeV
				new Transition (4965, 6047), // &n -> &nV
				new Transition (5090, 5121), // &Negative -> &NegativeV
				new Transition (5376, 5809), // &Not -> &NotV
				new Transition (5401, 5402), // &NotDouble -> &NotDoubleV
				new Transition (7174, 7417), // &Right -> &RightV
				new Transition (7251, 7262), // &RightDown -> &RightDownV
				new Transition (7254, 7255), // &RightDownTee -> &RightDownTeeV
				new Transition (7339, 7347), // &RightTee -> &RightTeeV
				new Transition (7384, 7406), // &RightUp -> &RightUpV
				new Transition (7388, 7389), // &RightUpDown -> &RightUpDownV
				new Transition (7398, 7399) // &RightUpTee -> &RightUpTeeV
			};
			TransitionTable_W = new Transition[2] {
				new Transition (0, 9484), // & -> &W
				new Transition (9793, 9794) // &Zero -> &ZeroW
			};
			TransitionTable_X = new Transition[1] {
				new Transition (0, 9565) // & -> &X
			};
			TransitionTable_Y = new Transition[2] {
				new Transition (0, 9665), // & -> &Y
				new Transition (1218, 1219) // &COP -> &COPY
			};
			TransitionTable_Z = new Transition[2] {
				new Transition (0, 9747), // & -> &Z
				new Transition (1425, 2093) // &D -> &DZ
			};
			TransitionTable_a = new Transition[555] {
				new Transition (0, 8), // & -> &a
				new Transition (1, 2), // &A -> &Aa
				new Transition (8, 9), // &a -> &aa
				new Transition (68, 69), // &Agr -> &Agra
				new Transition (74, 75), // &agr -> &agra
				new Transition (91, 92), // &Alph -> &Alpha
				new Transition (95, 96), // &alph -> &alpha
				new Transition (98, 99), // &Am -> &Ama
				new Transition (103, 104), // &am -> &ama
				new Transition (120, 122), // &and -> &anda
				new Transition (145, 147), // &angmsd -> &angmsda
				new Transition (147, 148), // &angmsda -> &angmsdaa
				new Transition (178, 179), // &angz -> &angza
				new Transition (199, 201), // &ap -> &apa
				new Transition (301, 302), // &b -> &ba
				new Transition (331, 332), // &B -> &Ba
				new Transition (336, 337), // &Backsl -> &Backsla
				new Transition (385, 386), // &bec -> &beca
				new Transition (391, 392), // &Bec -> &Beca
				new Transition (423, 424), // &Bet -> &Beta
				new Transition (426, 427), // &bet -> &beta
				new Transition (444, 445), // &bigc -> &bigca
				new Transition (477, 478), // &bigst -> &bigsta
				new Transition (483, 484), // &bigtri -> &bigtria
				new Transition (513, 514), // &bk -> &bka
				new Transition (519, 520), // &bl -> &bla
				new Transition (533, 534), // &blacksqu -> &blacksqua
				new Transition (540, 541), // &blacktri -> &blacktria
				new Transition (736, 737), // &brvb -> &brvba
				new Transition (789, 790), // &C -> &Ca
				new Transition (796, 797), // &c -> &ca
				new Transition (805, 807), // &cap -> &capa
				new Transition (817, 818), // &capc -> &capca
				new Transition (829, 830), // &Capit -> &Capita
				new Transition (841, 842), // &CapitalDifferenti -> &CapitalDifferentia
				new Transition (861, 862), // &cc -> &cca
				new Transition (866, 867), // &Cc -> &Cca
				new Transition (924, 925), // &Cedill -> &Cedilla
				new Transition (968, 969), // &checkm -> &checkma
				new Transition (987, 988), // &circle -> &circlea
				new Transition (1004, 1005), // &circled -> &circleda
				new Transition (1014, 1015), // &circledd -> &circledda
				new Transition (1088, 1089), // &ClockwiseContourIntegr -> &ClockwiseContourIntegra
				new Transition (1143, 1144), // &comm -> &comma
				new Transition (1196, 1197), // &ContourIntegr -> &ContourIntegra
				new Transition (1252, 1253), // &CounterClockwiseContourIntegr -> &CounterClockwiseContourIntegra
				new Transition (1256, 1257), // &cr -> &cra
				new Transition (1293, 1294), // &cud -> &cuda
				new Transition (1308, 1309), // &cul -> &cula
				new Transition (1322, 1323), // &cupbrc -> &cupbrca
				new Transition (1326, 1327), // &CupC -> &CupCa
				new Transition (1330, 1331), // &cupc -> &cupca
				new Transition (1346, 1347), // &cur -> &cura
				new Transition (1382, 1383), // &curve -> &curvea
				new Transition (1425, 1426), // &D -> &Da
				new Transition (1432, 1433), // &d -> &da
				new Transition (1464, 1465), // &dbk -> &dbka
				new Transition (1470, 1471), // &dbl -> &dbla
				new Transition (1474, 1475), // &Dc -> &Dca
				new Transition (1480, 1481), // &dc -> &dca
				new Transition (1492, 1494), // &dd -> &dda
				new Transition (1505, 1506), // &DDotr -> &DDotra
				new Transition (1522, 1523), // &Delt -> &Delta
				new Transition (1526, 1527), // &delt -> &delta
				new Transition (1546, 1547), // &dH -> &dHa
				new Transition (1550, 1551), // &dh -> &dha
				new Transition (1557, 1558), // &Di -> &Dia
				new Transition (1564, 1565), // &Diacritic -> &Diacritica
				new Transition (1588, 1589), // &DiacriticalGr -> &DiacriticalGra
				new Transition (1599, 1600), // &di -> &dia
				new Transition (1628, 1629), // &Differenti -> &Differentia
				new Transition (1633, 1634), // &dig -> &diga
				new Transition (1636, 1637), // &digamm -> &digamma
				new Transition (1681, 1682), // &doll -> &dolla
				new Transition (1709, 1710), // &DotEqu -> &DotEqua
				new Transition (1726, 1727), // &dotsqu -> &dotsqua
				new Transition (1735, 1736), // &doubleb -> &doubleba
				new Transition (1760, 1761), // &DoubleContourIntegr -> &DoubleContourIntegra
				new Transition (1874, 1875), // &DoubleVertic -> &DoubleVertica
				new Transition (1877, 1878), // &DoubleVerticalB -> &DoubleVerticalBa
				new Transition (1882, 1889), // &Down -> &Downa
				new Transition (1896, 1897), // &down -> &downa
				new Transition (1903, 1904), // &DownArrowB -> &DownArrowBa
				new Transition (1924, 1925), // &downdown -> &downdowna
				new Transition (1932, 1933), // &downh -> &downha
				new Transition (1983, 1984), // &DownLeftVectorB -> &DownLeftVectorBa
				new Transition (2009, 2010), // &DownRightVectorB -> &DownRightVectorBa
				new Transition (2025, 2026), // &drbk -> &drbka
				new Transition (2077, 2078), // &du -> &dua
				new Transition (2082, 2083), // &duh -> &duha
				new Transition (2086, 2087), // &dw -> &dwa
				new Transition (2103, 2104), // &dzigr -> &dzigra
				new Transition (2108, 2109), // &E -> &Ea
				new Transition (2115, 2116), // &e -> &ea
				new Transition (2127, 2128), // &Ec -> &Eca
				new Transition (2133, 2134), // &ec -> &eca
				new Transition (2188, 2189), // &Egr -> &Egra
				new Transition (2193, 2194), // &egr -> &egra
				new Transition (2228, 2229), // &Em -> &Ema
				new Transition (2233, 2234), // &em -> &ema
				new Transition (2250, 2251), // &EmptySm -> &EmptySma
				new Transition (2256, 2257), // &EmptySmallSqu -> &EmptySmallSqua
				new Transition (2268, 2269), // &EmptyVerySm -> &EmptyVerySma
				new Transition (2274, 2275), // &EmptyVerySmallSqu -> &EmptyVerySmallSqua
				new Transition (2312, 2313), // &ep -> &epa
				new Transition (2354, 2355), // &eqsl -> &eqsla
				new Transition (2368, 2369), // &Equ -> &Equa
				new Transition (2372, 2373), // &equ -> &equa
				new Transition (2403, 2404), // &eqvp -> &eqvpa
				new Transition (2409, 2410), // &er -> &era
				new Transition (2436, 2437), // &Et -> &Eta
				new Transition (2439, 2440), // &et -> &eta
				new Transition (2475, 2476), // &expect -> &expecta
				new Transition (2488, 2489), // &Exponenti -> &Exponentia
				new Transition (2498, 2499), // &exponenti -> &exponentia
				new Transition (2503, 2504), // &f -> &fa
				new Transition (2525, 2526), // &fem -> &fema
				new Transition (2560, 2561), // &FilledSm -> &FilledSma
				new Transition (2566, 2567), // &FilledSmallSqu -> &FilledSmallSqua
				new Transition (2576, 2577), // &FilledVerySm -> &FilledVerySma
				new Transition (2582, 2583), // &FilledVerySmallSqu -> &FilledVerySmallSqua
				new Transition (2592, 2593), // &fl -> &fla
				new Transition (2621, 2622), // &for -> &fora
				new Transition (2639, 2640), // &fp -> &fpa
				new Transition (2647, 2648), // &fr -> &fra
				new Transition (2701, 2702), // &g -> &ga
				new Transition (2708, 2709), // &G -> &Ga
				new Transition (2711, 2712), // &Gamm -> &Gamma
				new Transition (2715, 2716), // &gamm -> &gamma
				new Transition (2776, 2777), // &geqsl -> &geqsla
				new Transition (2824, 2826), // &gl -> &gla
				new Transition (2832, 2833), // &gn -> &gna
				new Transition (2861, 2862), // &gr -> &gra
				new Transition (2867, 2868), // &Gre -> &Grea
				new Transition (2874, 2875), // &GreaterEqu -> &GreaterEqua
				new Transition (2889, 2890), // &GreaterFullEqu -> &GreaterFullEqua
				new Transition (2895, 2896), // &GreaterGre -> &GreaterGrea
				new Transition (2907, 2908), // &GreaterSl -> &GreaterSla
				new Transition (2913, 2914), // &GreaterSlantEqu -> &GreaterSlantEqua
				new Transition (2955, 2956), // &gtlP -> &gtlPa
				new Transition (2965, 2966), // &gtr -> &gtra
				new Transition (3014, 3015), // &H -> &Ha
				new Transition (3020, 3021), // &h -> &ha
				new Transition (3060, 3061), // &hb -> &hba
				new Transition (3074, 3075), // &he -> &hea
				new Transition (3107, 3108), // &HilbertSp -> &HilbertSpa
				new Transition (3114, 3115), // &hkse -> &hksea
				new Transition (3120, 3121), // &hksw -> &hkswa
				new Transition (3126, 3127), // &ho -> &hoa
				new Transition (3141, 3142), // &hookleft -> &hooklefta
				new Transition (3152, 3153), // &hookright -> &hookrighta
				new Transition (3167, 3168), // &horb -> &horba
				new Transition (3176, 3177), // &Horizont -> &Horizonta
				new Transition (3192, 3193), // &hsl -> &hsla
				new Transition (3221, 3222), // &HumpEqu -> &HumpEqua
				new Transition (3236, 3237), // &I -> &Ia
				new Transition (3243, 3244), // &i -> &ia
				new Transition (3290, 3291), // &Igr -> &Igra
				new Transition (3296, 3297), // &igr -> &igra
				new Transition (3317, 3318), // &iiot -> &iiota
				new Transition (3330, 3332), // &Im -> &Ima
				new Transition (3336, 3337), // &im -> &ima
				new Transition (3346, 3347), // &Imagin -> &Imagina
				new Transition (3357, 3358), // &imagp -> &imagpa
				new Transition (3380, 3381), // &inc -> &inca
				new Transition (3403, 3404), // &intc -> &intca
				new Transition (3415, 3416), // &Integr -> &Integra
				new Transition (3420, 3421), // &interc -> &interca
				new Transition (3433, 3434), // &intl -> &intla
				new Transition (3454, 3455), // &InvisibleComm -> &InvisibleComma
				new Transition (3486, 3487), // &Iot -> &Iota
				new Transition (3489, 3490), // &iot -> &iota
				new Transition (3577, 3578), // &jm -> &jma
				new Transition (3618, 3619), // &K -> &Ka
				new Transition (3621, 3622), // &Kapp -> &Kappa
				new Transition (3624, 3625), // &k -> &ka
				new Transition (3627, 3628), // &kapp -> &kappa
				new Transition (3692, 3705), // &l -> &la
				new Transition (3693, 3694), // &lA -> &lAa
				new Transition (3698, 3699), // &L -> &La
				new Transition (3719, 3720), // &lagr -> &lagra
				new Transition (3725, 3726), // &Lambd -> &Lambda
				new Transition (3730, 3731), // &lambd -> &lambda
				new Transition (3747, 3748), // &Lapl -> &Lapla
				new Transition (3792, 3799), // &lat -> &lata
				new Transition (3794, 3795), // &lAt -> &lAta
				new Transition (3807, 3808), // &lB -> &lBa
				new Transition (3812, 3813), // &lb -> &lba
				new Transition (3821, 3822), // &lbr -> &lbra
				new Transition (3837, 3838), // &Lc -> &Lca
				new Transition (3843, 3844), // &lc -> &lca
				new Transition (3870, 3871), // &ldc -> &ldca
				new Transition (3881, 3882), // &ldrdh -> &ldrdha
				new Transition (3887, 3888), // &ldrush -> &ldrusha
				new Transition (3900, 3919), // &Left -> &Lefta
				new Transition (3907, 3908), // &LeftAngleBr -> &LeftAngleBra
				new Transition (3926, 3927), // &left -> &lefta
				new Transition (3933, 3934), // &LeftArrowB -> &LeftArrowBa
				new Transition (3948, 3949), // &leftarrowt -> &leftarrowta
				new Transition (3968, 3969), // &LeftDoubleBr -> &LeftDoubleBra
				new Transition (3994, 3995), // &LeftDownVectorB -> &LeftDownVectorBa
				new Transition (4004, 4005), // &lefth -> &leftha
				new Transition (4022, 4023), // &leftleft -> &leftlefta
				new Transition (4045, 4046), // &Leftright -> &Leftrighta
				new Transition (4056, 4057), // &leftright -> &leftrighta
				new Transition (4065, 4066), // &leftrighth -> &leftrightha
				new Transition (4078, 4079), // &leftrightsquig -> &leftrightsquiga
				new Transition (4121, 4122), // &LeftTri -> &LeftTria
				new Transition (4128, 4129), // &LeftTriangleB -> &LeftTriangleBa
				new Transition (4134, 4135), // &LeftTriangleEqu -> &LeftTriangleEqua
				new Transition (4168, 4169), // &LeftUpVectorB -> &LeftUpVectorBa
				new Transition (4179, 4180), // &LeftVectorB -> &LeftVectorBa
				new Transition (4192, 4193), // &leqsl -> &leqsla
				new Transition (4215, 4216), // &less -> &lessa
				new Transition (4242, 4243), // &LessEqu -> &LessEqua
				new Transition (4247, 4248), // &LessEqualGre -> &LessEqualGrea
				new Transition (4259, 4260), // &LessFullEqu -> &LessFullEqua
				new Transition (4265, 4266), // &LessGre -> &LessGrea
				new Transition (4285, 4286), // &LessSl -> &LessSla
				new Transition (4291, 4292), // &LessSlantEqu -> &LessSlantEqua
				new Transition (4321, 4322), // &lH -> &lHa
				new Transition (4325, 4326), // &lh -> &lha
				new Transition (4348, 4350), // &ll -> &lla
				new Transition (4363, 4364), // &Lleft -> &Llefta
				new Transition (4370, 4371), // &llh -> &llha
				new Transition (4394, 4396), // &lmoust -> &lmousta
				new Transition (4401, 4402), // &ln -> &lna
				new Transition (4422, 4423), // &lo -> &loa
				new Transition (4450, 4451), // &Longleft -> &Longlefta
				new Transition (4462, 4463), // &longleft -> &longlefta
				new Transition (4484, 4485), // &Longleftright -> &Longleftrighta
				new Transition (4495, 4496), // &longleftright -> &longleftrighta
				new Transition (4502, 4503), // &longm -> &longma
				new Transition (4524, 4525), // &Longright -> &Longrighta
				new Transition (4535, 4536), // &longright -> &longrighta
				new Transition (4543, 4544), // &loop -> &loopa
				new Transition (4560, 4561), // &lop -> &lopa
				new Transition (4579, 4580), // &low -> &lowa
				new Transition (4584, 4585), // &lowb -> &lowba
				new Transition (4621, 4622), // &lp -> &lpa
				new Transition (4628, 4629), // &lr -> &lra
				new Transition (4640, 4641), // &lrh -> &lrha
				new Transition (4652, 4653), // &ls -> &lsa
				new Transition (4720, 4721), // &ltl -> &ltla
				new Transition (4738, 4739), // &ltrP -> &ltrPa
				new Transition (4746, 4747), // &lurdsh -> &lurdsha
				new Transition (4751, 4752), // &luruh -> &luruha
				new Transition (4767, 4768), // &m -> &ma
				new Transition (4781, 4782), // &M -> &Ma
				new Transition (4812, 4813), // &mcomm -> &mcomma
				new Transition (4820, 4821), // &md -> &mda
				new Transition (4830, 4831), // &me -> &mea
				new Transition (4836, 4837), // &measured -> &measureda
				new Transition (4849, 4850), // &MediumSp -> &MediumSpa
				new Transition (4876, 4878), // &mid -> &mida
				new Transition (4957, 4958), // &multim -> &multima
				new Transition (4961, 4962), // &mum -> &muma
				new Transition (4965, 4966), // &n -> &na
				new Transition (4968, 4969), // &nabl -> &nabla
				new Transition (4971, 4972), // &N -> &Na
				new Transition (5003, 5005), // &natur -> &natura
				new Transition (5020, 5021), // &nc -> &nca
				new Transition (5024, 5025), // &Nc -> &Nca
				new Transition (5059, 5060), // &nd -> &nda
				new Transition (5064, 5066), // &ne -> &nea
				new Transition (5085, 5086), // &Neg -> &Nega
				new Transition (5098, 5099), // &NegativeMediumSp -> &NegativeMediumSpa
				new Transition (5109, 5110), // &NegativeThickSp -> &NegativeThickSpa
				new Transition (5116, 5117), // &NegativeThinSp -> &NegativeThinSpa
				new Transition (5130, 5131), // &NegativeVeryThinSp -> &NegativeVeryThinSpa
				new Transition (5141, 5142), // &nese -> &nesea
				new Transition (5154, 5155), // &NestedGre -> &NestedGrea
				new Transition (5161, 5162), // &NestedGreaterGre -> &NestedGreaterGrea
				new Transition (5205, 5206), // &ngeqsl -> &ngeqsla
				new Transition (5227, 5232), // &nh -> &nha
				new Transition (5236, 5237), // &nhp -> &nhpa
				new Transition (5256, 5261), // &nl -> &nla
				new Transition (5275, 5276), // &nLeft -> &nLefta
				new Transition (5283, 5284), // &nleft -> &nlefta
				new Transition (5294, 5295), // &nLeftright -> &nLeftrighta
				new Transition (5305, 5306), // &nleftright -> &nleftrighta
				new Transition (5317, 5318), // &nleqsl -> &nleqsla
				new Transition (5350, 5351), // &NoBre -> &NoBrea
				new Transition (5357, 5358), // &NonBre -> &NonBrea
				new Transition (5364, 5365), // &NonBreakingSp -> &NonBreakingSpa
				new Transition (5392, 5393), // &NotCupC -> &NotCupCa
				new Transition (5407, 5408), // &NotDoubleVertic -> &NotDoubleVertica
				new Transition (5410, 5411), // &NotDoubleVerticalB -> &NotDoubleVerticalBa
				new Transition (5423, 5424), // &NotEqu -> &NotEqua
				new Transition (5441, 5442), // &NotGre -> &NotGrea
				new Transition (5449, 5450), // &NotGreaterEqu -> &NotGreaterEqua
				new Transition (5459, 5460), // &NotGreaterFullEqu -> &NotGreaterFullEqua
				new Transition (5465, 5466), // &NotGreaterGre -> &NotGreaterGrea
				new Transition (5477, 5478), // &NotGreaterSl -> &NotGreaterSla
				new Transition (5483, 5484), // &NotGreaterSlantEqu -> &NotGreaterSlantEqua
				new Transition (5508, 5509), // &NotHumpEqu -> &NotHumpEqua
				new Transition (5521, 5522), // &notinv -> &notinva
				new Transition (5534, 5535), // &NotLeftTri -> &NotLeftTria
				new Transition (5541, 5542), // &NotLeftTriangleB -> &NotLeftTriangleBa
				new Transition (5547, 5548), // &NotLeftTriangleEqu -> &NotLeftTriangleEqua
				new Transition (5556, 5557), // &NotLessEqu -> &NotLessEqua
				new Transition (5562, 5563), // &NotLessGre -> &NotLessGrea
				new Transition (5574, 5575), // &NotLessSl -> &NotLessSla
				new Transition (5580, 5581), // &NotLessSlantEqu -> &NotLessSlantEqua
				new Transition (5598, 5599), // &NotNestedGre -> &NotNestedGrea
				new Transition (5605, 5606), // &NotNestedGreaterGre -> &NotNestedGreaterGrea
				new Transition (5623, 5624), // &notniv -> &notniva
				new Transition (5641, 5642), // &NotPrecedesEqu -> &NotPrecedesEqua
				new Transition (5646, 5647), // &NotPrecedesSl -> &NotPrecedesSla
				new Transition (5652, 5653), // &NotPrecedesSlantEqu -> &NotPrecedesSlantEqua
				new Transition (5677, 5678), // &NotRightTri -> &NotRightTria
				new Transition (5684, 5685), // &NotRightTriangleB -> &NotRightTriangleBa
				new Transition (5690, 5691), // &NotRightTriangleEqu -> &NotRightTriangleEqua
				new Transition (5696, 5697), // &NotSqu -> &NotSqua
				new Transition (5709, 5710), // &NotSquareSubsetEqu -> &NotSquareSubsetEqua
				new Transition (5722, 5723), // &NotSquareSupersetEqu -> &NotSquareSupersetEqua
				new Transition (5734, 5735), // &NotSubsetEqu -> &NotSubsetEqua
				new Transition (5747, 5748), // &NotSucceedsEqu -> &NotSucceedsEqua
				new Transition (5752, 5753), // &NotSucceedsSl -> &NotSucceedsSla
				new Transition (5758, 5759), // &NotSucceedsSlantEqu -> &NotSucceedsSlantEqua
				new Transition (5777, 5778), // &NotSupersetEqu -> &NotSupersetEqua
				new Transition (5789, 5790), // &NotTildeEqu -> &NotTildeEqua
				new Transition (5799, 5800), // &NotTildeFullEqu -> &NotTildeFullEqua
				new Transition (5814, 5815), // &NotVertic -> &NotVertica
				new Transition (5817, 5818), // &NotVerticalB -> &NotVerticalBa
				new Transition (5821, 5822), // &np -> &npa
				new Transition (5823, 5825), // &npar -> &npara
				new Transition (5855, 5860), // &nr -> &nra
				new Transition (5872, 5873), // &nRight -> &nRighta
				new Transition (5882, 5883), // &nright -> &nrighta
				new Transition (5918, 5919), // &nshortp -> &nshortpa
				new Transition (5920, 5921), // &nshortpar -> &nshortpara
				new Transition (5938, 5939), // &nsp -> &nspa
				new Transition (6007, 6008), // &ntri -> &ntria
				new Transition (6043, 6044), // &nv -> &nva
				new Transition (6048, 6049), // &nVD -> &nVDa
				new Transition (6053, 6054), // &nVd -> &nVda
				new Transition (6058, 6059), // &nvD -> &nvDa
				new Transition (6063, 6064), // &nvd -> &nvda
				new Transition (6073, 6074), // &nvH -> &nvHa
				new Transition (6111, 6112), // &nw -> &nwa
				new Transition (6127, 6128), // &nwne -> &nwnea
				new Transition (6131, 6132), // &O -> &Oa
				new Transition (6138, 6139), // &o -> &oa
				new Transition (6163, 6164), // &od -> &oda
				new Transition (6170, 6171), // &Odbl -> &Odbla
				new Transition (6175, 6176), // &odbl -> &odbla
				new Transition (6215, 6216), // &Ogr -> &Ogra
				new Transition (6220, 6221), // &ogr -> &ogra
				new Transition (6228, 6229), // &ohb -> &ohba
				new Transition (6238, 6239), // &ol -> &ola
				new Transition (6258, 6259), // &Om -> &Oma
				new Transition (6263, 6264), // &om -> &oma
				new Transition (6269, 6270), // &Omeg -> &Omega
				new Transition (6273, 6274), // &omeg -> &omega
				new Transition (6302, 6303), // &op -> &opa
				new Transition (6342, 6344), // &or -> &ora
				new Transition (6386, 6387), // &Osl -> &Osla
				new Transition (6391, 6392), // &osl -> &osla
				new Transition (6417, 6419), // &otimes -> &otimesa
				new Transition (6431, 6432), // &ovb -> &ovba
				new Transition (6438, 6439), // &OverB -> &OverBa
				new Transition (6442, 6443), // &OverBr -> &OverBra
				new Transition (6451, 6452), // &OverP -> &OverPa
				new Transition (6463, 6464), // &p -> &pa
				new Transition (6465, 6467), // &par -> &para
				new Transition (6482, 6483), // &P -> &Pa
				new Transition (6486, 6487), // &Parti -> &Partia
				new Transition (6533, 6534), // &phmm -> &phmma
				new Transition (6555, 6556), // &pl -> &pla
				new Transition (6567, 6569), // &plus -> &plusa
				new Transition (6612, 6613), // &Poinc -> &Poinca
				new Transition (6617, 6618), // &Poincarepl -> &Poincarepla
				new Transition (6642, 6644), // &pr -> &pra
				new Transition (6655, 6657), // &prec -> &preca
				new Transition (6681, 6682), // &PrecedesEqu -> &PrecedesEqua
				new Transition (6686, 6687), // &PrecedesSl -> &PrecedesSla
				new Transition (6692, 6693), // &PrecedesSlantEqu -> &PrecedesSlantEqua
				new Transition (6705, 6706), // &precn -> &precna
				new Transition (6735, 6736), // &prn -> &prna
				new Transition (6754, 6755), // &prof -> &profa
				new Transition (6756, 6757), // &profal -> &profala
				new Transition (6778, 6780), // &Proportion -> &Proportiona
				new Transition (6847, 6848), // &qu -> &qua
				new Transition (6876, 6882), // &r -> &ra
				new Transition (6877, 6878), // &rA -> &rAa
				new Transition (6886, 6887), // &R -> &Ra
				new Transition (6932, 6934), // &rarr -> &rarra
				new Transition (6968, 6969), // &rAt -> &rAta
				new Transition (6973, 6974), // &rat -> &rata
				new Transition (6981, 6982), // &ration -> &rationa
				new Transition (6986, 6987), // &RB -> &RBa
				new Transition (6991, 6992), // &rB -> &rBa
				new Transition (6996, 6997), // &rb -> &rba
				new Transition (7005, 7006), // &rbr -> &rbra
				new Transition (7021, 7022), // &Rc -> &Rca
				new Transition (7027, 7028), // &rc -> &rca
				new Transition (7054, 7055), // &rdc -> &rdca
				new Transition (7059, 7060), // &rdldh -> &rdldha
				new Transition (7074, 7075), // &re -> &rea
				new Transition (7082, 7083), // &realp -> &realpa
				new Transition (7151, 7152), // &rH -> &rHa
				new Transition (7155, 7156), // &rh -> &rha
				new Transition (7174, 7193), // &Right -> &Righta
				new Transition (7181, 7182), // &RightAngleBr -> &RightAngleBra
				new Transition (7202, 7203), // &right -> &righta
				new Transition (7209, 7210), // &RightArrowB -> &RightArrowBa
				new Transition (7223, 7224), // &rightarrowt -> &rightarrowta
				new Transition (7243, 7244), // &RightDoubleBr -> &RightDoubleBra
				new Transition (7269, 7270), // &RightDownVectorB -> &RightDownVectorBa
				new Transition (7279, 7280), // &righth -> &rightha
				new Transition (7297, 7298), // &rightleft -> &rightlefta
				new Transition (7305, 7306), // &rightlefth -> &rightleftha
				new Transition (7318, 7319), // &rightright -> &rightrighta
				new Transition (7330, 7331), // &rightsquig -> &rightsquiga
				new Transition (7366, 7367), // &RightTri -> &RightTria
				new Transition (7373, 7374), // &RightTriangleB -> &RightTriangleBa
				new Transition (7379, 7380), // &RightTriangleEqu -> &RightTriangleEqua
				new Transition (7413, 7414), // &RightUpVectorB -> &RightUpVectorBa
				new Transition (7424, 7425), // &RightVectorB -> &RightVectorBa
				new Transition (7442, 7443), // &rl -> &rla
				new Transition (7447, 7448), // &rlh -> &rlha
				new Transition (7457, 7459), // &rmoust -> &rmousta
				new Transition (7469, 7470), // &ro -> &roa
				new Transition (7481, 7482), // &rop -> &ropa
				new Transition (7512, 7513), // &rp -> &rpa
				new Transition (7526, 7527), // &rr -> &rra
				new Transition (7535, 7536), // &Rright -> &Rrighta
				new Transition (7542, 7543), // &rs -> &rsa
				new Transition (7595, 7596), // &RuleDel -> &RuleDela
				new Transition (7604, 7605), // &ruluh -> &ruluha
				new Transition (7610, 7611), // &S -> &Sa
				new Transition (7617, 7618), // &s -> &sa
				new Transition (7629, 7636), // &Sc -> &Sca
				new Transition (7631, 7633), // &sc -> &sca
				new Transition (7670, 7671), // &scn -> &scna
				new Transition (7703, 7704), // &se -> &sea
				new Transition (7725, 7726), // &sesw -> &seswa
				new Transition (7751, 7752), // &sh -> &sha
				new Transition (7803, 7804), // &shortp -> &shortpa
				new Transition (7805, 7806), // &shortpar -> &shortpara
				new Transition (7835, 7836), // &Sigm -> &Sigma
				new Transition (7840, 7841), // &sigm -> &sigma
				new Transition (7873, 7874), // &simr -> &simra
				new Transition (7878, 7879), // &sl -> &sla
				new Transition (7883, 7884), // &Sm -> &Sma
				new Transition (7894, 7895), // &sm -> &sma
				new Transition (7912, 7913), // &smep -> &smepa
				new Transition (7944, 7946), // &solb -> &solba
				new Transition (7956, 7957), // &sp -> &spa
				new Transition (7969, 7970), // &sqc -> &sqca
				new Transition (8008, 8015), // &squ -> &squa
				new Transition (8010, 8011), // &Squ -> &Squa
				new Transition (8041, 8042), // &SquareSubsetEqu -> &SquareSubsetEqua
				new Transition (8054, 8055), // &SquareSupersetEqu -> &SquareSupersetEqua
				new Transition (8068, 8069), // &sr -> &sra
				new Transition (8091, 8092), // &sst -> &ssta
				new Transition (8096, 8097), // &St -> &Sta
				new Transition (8100, 8101), // &st -> &sta
				new Transition (8106, 8107), // &str -> &stra
				new Transition (8160, 8161), // &subr -> &subra
				new Transition (8180, 8181), // &SubsetEqu -> &SubsetEqua
				new Transition (8199, 8201), // &succ -> &succa
				new Transition (8225, 8226), // &SucceedsEqu -> &SucceedsEqua
				new Transition (8230, 8231), // &SucceedsSl -> &SucceedsSla
				new Transition (8236, 8237), // &SucceedsSlantEqu -> &SucceedsSlantEqua
				new Transition (8249, 8250), // &succn -> &succna
				new Transition (8271, 8272), // &SuchTh -> &SuchTha
				new Transition (8316, 8317), // &SupersetEqu -> &SupersetEqua
				new Transition (8328, 8329), // &supl -> &supla
				new Transition (8375, 8376), // &sw -> &swa
				new Transition (8391, 8392), // &swnw -> &swnwa
				new Transition (8400, 8401), // &T -> &Ta
				new Transition (8404, 8405), // &t -> &ta
				new Transition (8419, 8420), // &Tc -> &Tca
				new Transition (8425, 8426), // &tc -> &tca
				new Transition (8481, 8482), // &Thet -> &Theta
				new Transition (8484, 8485), // &thet -> &theta
				new Transition (8495, 8496), // &thick -> &thicka
				new Transition (8511, 8512), // &ThickSp -> &ThickSpa
				new Transition (8522, 8523), // &ThinSp -> &ThinSpa
				new Transition (8527, 8528), // &thk -> &thka
				new Transition (8556, 8557), // &TildeEqu -> &TildeEqua
				new Transition (8566, 8567), // &TildeFullEqu -> &TildeFullEqua
				new Transition (8580, 8582), // &timesb -> &timesba
				new Transition (8591, 8592), // &toe -> &toea
				new Transition (8614, 8615), // &tos -> &tosa
				new Transition (8628, 8629), // &tr -> &tra
				new Transition (8633, 8634), // &tri -> &tria
				new Transition (8744, 8745), // &twohe -> &twohea
				new Transition (8750, 8751), // &twoheadleft -> &twoheadlefta
				new Transition (8761, 8762), // &twoheadright -> &twoheadrighta
				new Transition (8768, 8769), // &U -> &Ua
				new Transition (8775, 8776), // &u -> &ua
				new Transition (8829, 8830), // &ud -> &uda
				new Transition (8836, 8837), // &Udbl -> &Udbla
				new Transition (8841, 8842), // &udbl -> &udbla
				new Transition (8845, 8846), // &udh -> &udha
				new Transition (8861, 8862), // &Ugr -> &Ugra
				new Transition (8867, 8868), // &ugr -> &ugra
				new Transition (8872, 8873), // &uH -> &uHa
				new Transition (8876, 8877), // &uh -> &uha
				new Transition (8904, 8905), // &Um -> &Uma
				new Transition (8909, 8910), // &um -> &uma
				new Transition (8920, 8921), // &UnderB -> &UnderBa
				new Transition (8924, 8925), // &UnderBr -> &UnderBra
				new Transition (8933, 8934), // &UnderP -> &UnderPa
				new Transition (8970, 8977), // &Up -> &Upa
				new Transition (8983, 8984), // &up -> &upa
				new Transition (8990, 8991), // &UpArrowB -> &UpArrowBa
				new Transition (9017, 9018), // &Updown -> &Updowna
				new Transition (9027, 9028), // &updown -> &updowna
				new Transition (9046, 9047), // &uph -> &upha
				new Transition (9119, 9120), // &upup -> &upupa
				new Transition (9182, 9183), // &uu -> &uua
				new Transition (9194, 9195), // &uw -> &uwa
				new Transition (9201, 9202), // &v -> &va
				new Transition (9217, 9218), // &vark -> &varka
				new Transition (9220, 9221), // &varkapp -> &varkappa
				new Transition (9255, 9256), // &varsigm -> &varsigma
				new Transition (9282, 9283), // &varthet -> &vartheta
				new Transition (9286, 9287), // &vartri -> &vartria
				new Transition (9304, 9305), // &Vb -> &Vba
				new Transition (9308, 9309), // &vB -> &vBa
				new Transition (9320, 9321), // &VD -> &VDa
				new Transition (9325, 9326), // &Vd -> &Vda
				new Transition (9330, 9331), // &vD -> &vDa
				new Transition (9335, 9336), // &vd -> &vda
				new Transition (9348, 9349), // &veeb -> &veeba
				new Transition (9361, 9362), // &Verb -> &Verba
				new Transition (9366, 9367), // &verb -> &verba
				new Transition (9375, 9376), // &Vertic -> &Vertica
				new Transition (9378, 9379), // &VerticalB -> &VerticalBa
				new Transition (9389, 9390), // &VerticalSep -> &VerticalSepa
				new Transition (9391, 9392), // &VerticalSepar -> &VerticalSepara
				new Transition (9409, 9410), // &VeryThinSp -> &VeryThinSpa
				new Transition (9472, 9473), // &Vvd -> &Vvda
				new Transition (9480, 9481), // &vzigz -> &vzigza
				new Transition (9498, 9499), // &wedb -> &wedba
				new Transition (9535, 9536), // &wre -> &wrea
				new Transition (9549, 9550), // &xc -> &xca
				new Transition (9572, 9577), // &xh -> &xha
				new Transition (9585, 9590), // &xl -> &xla
				new Transition (9594, 9595), // &xm -> &xma
				new Transition (9623, 9628), // &xr -> &xra
				new Transition (9665, 9666), // &Y -> &Ya
				new Transition (9672, 9673), // &y -> &ya
				new Transition (9747, 9748), // &Z -> &Za
				new Transition (9754, 9755), // &z -> &za
				new Transition (9761, 9762), // &Zc -> &Zca
				new Transition (9767, 9768), // &zc -> &zca
				new Transition (9800, 9801), // &ZeroWidthSp -> &ZeroWidthSpa
				new Transition (9805, 9806), // &Zet -> &Zeta
				new Transition (9808, 9809), // &zet -> &zeta
				new Transition (9827, 9828) // &zigr -> &zigra
			};
			TransitionTable_b = new Transition[96] {
				new Transition (0, 301), // & -> &b
				new Transition (1, 15), // &A -> &Ab
				new Transition (8, 21), // &a -> &ab
				new Transition (147, 150), // &angmsda -> &angmsdab
				new Transition (167, 168), // &angrtv -> &angrtvb
				new Transition (301, 360), // &b -> &bb
				new Transition (364, 365), // &bbrkt -> &bbrktb
				new Transition (613, 614), // &box -> &boxb
				new Transition (735, 736), // &brv -> &brvb
				new Transition (758, 760), // &bsol -> &bsolb
				new Transition (764, 765), // &bsolhsu -> &bsolhsub
				new Transition (805, 811), // &cap -> &capb
				new Transition (1101, 1102), // &CloseCurlyDou -> &CloseCurlyDoub
				new Transition (1118, 1119), // &clu -> &club
				new Transition (1278, 1279), // &csu -> &csub
				new Transition (1318, 1320), // &cup -> &cupb
				new Transition (1432, 1463), // &d -> &db
				new Transition (1577, 1578), // &DiacriticalDou -> &DiacriticalDoub
				new Transition (1731, 1732), // &dou -> &doub
				new Transition (1734, 1735), // &double -> &doubleb
				new Transition (1744, 1745), // &Dou -> &Doub
				new Transition (2023, 2024), // &dr -> &drb
				new Transition (2389, 2390), // &Equili -> &Equilib
				new Transition (2701, 2730), // &g -> &gb
				new Transition (2708, 2724), // &G -> &Gb
				new Transition (3020, 3060), // &h -> &hb
				new Transition (3101, 3102), // &Hil -> &Hilb
				new Transition (3166, 3167), // &hor -> &horb
				new Transition (3225, 3226), // &hy -> &hyb
				new Transition (3447, 3448), // &Invisi -> &Invisib
				new Transition (3692, 3812), // &l -> &lb
				new Transition (3723, 3724), // &Lam -> &Lamb
				new Transition (3728, 3729), // &lam -> &lamb
				new Transition (3766, 3768), // &larr -> &larrb
				new Transition (3812, 3817), // &lb -> &lbb
				new Transition (3862, 3863), // &lcu -> &lcub
				new Transition (3963, 3964), // &LeftDou -> &LeftDoub
				new Transition (4325, 4334), // &lh -> &lhb
				new Transition (4422, 4430), // &lo -> &lob
				new Transition (4579, 4584), // &low -> &lowb
				new Transition (4676, 4677), // &lsq -> &lsqb
				new Transition (4892, 4894), // &minus -> &minusb
				new Transition (4965, 5010), // &n -> &nb
				new Transition (4966, 4967), // &na -> &nab
				new Transition (5398, 5399), // &NotDou -> &NotDoub
				new Transition (5521, 5524), // &notinv -> &notinvb
				new Transition (5623, 5626), // &notniv -> &notnivb
				new Transition (5701, 5702), // &NotSquareSu -> &NotSquareSub
				new Transition (5726, 5727), // &NotSu -> &NotSub
				new Transition (5944, 5945), // &nsqsu -> &nsqsub
				new Transition (5951, 5952), // &nsu -> &nsub
				new Transition (6163, 6174), // &od -> &odb
				new Transition (6168, 6169), // &Od -> &Odb
				new Transition (6227, 6228), // &oh -> &ohb
				new Transition (6316, 6317), // &OpenCurlyDou -> &OpenCurlyDoub
				new Transition (6430, 6431), // &ov -> &ovb
				new Transition (6567, 6574), // &plus -> &plusb
				new Transition (6876, 6996), // &r -> &rb
				new Transition (6932, 6937), // &rarr -> &rarrb
				new Transition (6996, 7001), // &rb -> &rbb
				new Transition (7046, 7047), // &rcu -> &rcub
				new Transition (7114, 7115), // &ReverseEquili -> &ReverseEquilib
				new Transition (7128, 7129), // &ReverseUpEquili -> &ReverseUpEquilib
				new Transition (7238, 7239), // &RightDou -> &RightDoub
				new Transition (7469, 7477), // &ro -> &rob
				new Transition (7559, 7560), // &rsq -> &rsqb
				new Transition (7617, 7624), // &s -> &sb
				new Transition (7697, 7699), // &sdot -> &sdotb
				new Transition (7942, 7944), // &sol -> &solb
				new Transition (7985, 7986), // &sqsu -> &sqsub
				new Transition (8033, 8034), // &SquareSu -> &SquareSub
				new Transition (8127, 8128), // &Su -> &Sub
				new Transition (8130, 8131), // &su -> &sub
				new Transition (8193, 8194), // &subsu -> &subsub
				new Transition (8297, 8298), // &supdsu -> &supdsub
				new Transition (8325, 8326), // &suphsu -> &suphsub
				new Transition (8370, 8371), // &supsu -> &supsub
				new Transition (8401, 8402), // &Ta -> &Tab
				new Transition (8404, 8415), // &t -> &tb
				new Transition (8578, 8580), // &times -> &timesb
				new Transition (8594, 8596), // &top -> &topb
				new Transition (8690, 8691), // &tris -> &trisb
				new Transition (8768, 8797), // &U -> &Ub
				new Transition (8775, 8802), // &u -> &ub
				new Transition (8829, 8840), // &ud -> &udb
				new Transition (8834, 8835), // &Ud -> &Udb
				new Transition (8876, 8883), // &uh -> &uhb
				new Transition (9039, 9040), // &UpEquili -> &UpEquilib
				new Transition (9258, 9259), // &varsu -> &varsub
				new Transition (9303, 9304), // &V -> &Vb
				new Transition (9346, 9348), // &vee -> &veeb
				new Transition (9360, 9361), // &Ver -> &Verb
				new Transition (9365, 9366), // &ver -> &verb
				new Transition (9427, 9428), // &vnsu -> &vnsub
				new Transition (9458, 9459), // &vsu -> &vsub
				new Transition (9497, 9498) // &wed -> &wedb
			};
			TransitionTable_c = new Transition[377] {
				new Transition (0, 796), // & -> &c
				new Transition (1, 33), // &A -> &Ac
				new Transition (2, 3), // &Aa -> &Aac
				new Transition (8, 27), // &a -> &ac
				new Transition (9, 10), // &aa -> &aac
				new Transition (35, 36), // &Acir -> &Acirc
				new Transition (39, 40), // &acir -> &acirc
				new Transition (99, 100), // &Ama -> &Amac
				new Transition (104, 105), // &ama -> &amac
				new Transition (147, 152), // &angmsda -> &angmsdac
				new Transition (201, 202), // &apa -> &apac
				new Transition (222, 223), // &ApplyFun -> &ApplyFunc
				new Transition (247, 248), // &As -> &Asc
				new Transition (251, 252), // &as -> &asc
				new Transition (289, 290), // &aw -> &awc
				new Transition (301, 369), // &b -> &bc
				new Transition (302, 303), // &ba -> &bac
				new Transition (304, 305), // &back -> &backc
				new Transition (331, 374), // &B -> &Bc
				new Transition (332, 333), // &Ba -> &Bac
				new Transition (384, 385), // &be -> &bec
				new Transition (390, 391), // &Be -> &Bec
				new Transition (443, 444), // &big -> &bigc
				new Transition (449, 450), // &bigcir -> &bigcirc
				new Transition (472, 473), // &bigsq -> &bigsqc
				new Transition (520, 521), // &bla -> &blac
				new Transition (575, 576), // &blo -> &bloc
				new Transition (740, 741), // &Bs -> &Bsc
				new Transition (744, 745), // &bs -> &bsc
				new Transition (789, 866), // &C -> &Cc
				new Transition (790, 791), // &Ca -> &Cac
				new Transition (796, 861), // &c -> &cc
				new Transition (797, 798), // &ca -> &cac
				new Transition (805, 817), // &cap -> &capc
				new Transition (812, 813), // &capbr -> &capbrc
				new Transition (887, 888), // &Ccir -> &Ccirc
				new Transition (891, 892), // &ccir -> &ccirc
				new Transition (956, 957), // &CH -> &CHc
				new Transition (960, 961), // &ch -> &chc
				new Transition (964, 965), // &che -> &chec
				new Transition (979, 981), // &cir -> &circ
				new Transition (1004, 1009), // &circled -> &circledc
				new Transition (1011, 1012), // &circledcir -> &circledcirc
				new Transition (1020, 1021), // &Cir -> &Circ
				new Transition (1063, 1064), // &cirs -> &cirsc
				new Transition (1069, 1070), // &Clo -> &Cloc
				new Transition (1213, 1214), // &Coprodu -> &Coproduc
				new Transition (1233, 1234), // &CounterClo -> &CounterCloc
				new Transition (1270, 1271), // &Cs -> &Csc
				new Transition (1274, 1275), // &cs -> &csc
				new Transition (1305, 1306), // &cues -> &cuesc
				new Transition (1318, 1330), // &cup -> &cupc
				new Transition (1321, 1322), // &cupbr -> &cupbrc
				new Transition (1359, 1360), // &curlyeqpre -> &curlyeqprec
				new Transition (1363, 1364), // &curlyeqsu -> &curlyeqsuc
				new Transition (1364, 1365), // &curlyeqsuc -> &curlyeqsucc
				new Transition (1407, 1408), // &cw -> &cwc
				new Transition (1420, 1421), // &cyl -> &cylc
				new Transition (1425, 1474), // &D -> &Dc
				new Transition (1432, 1480), // &d -> &dc
				new Transition (1471, 1472), // &dbla -> &dblac
				new Transition (1558, 1559), // &Dia -> &Diac
				new Transition (1563, 1564), // &Diacriti -> &Diacritic
				new Transition (1567, 1568), // &DiacriticalA -> &DiacriticalAc
				new Transition (1581, 1582), // &DiacriticalDoubleA -> &DiacriticalDoubleAc
				new Transition (1661, 1662), // &DJ -> &DJc
				new Transition (1665, 1666), // &dj -> &djc
				new Transition (1669, 1670), // &dl -> &dlc
				new Transition (1873, 1874), // &DoubleVerti -> &DoubleVertic
				new Transition (1960, 1961), // &DownLeftRightVe -> &DownLeftRightVec
				new Transition (1970, 1971), // &DownLeftTeeVe -> &DownLeftTeeVec
				new Transition (1977, 1978), // &DownLeftVe -> &DownLeftVec
				new Transition (1996, 1997), // &DownRightTeeVe -> &DownRightTeeVec
				new Transition (2003, 2004), // &DownRightVe -> &DownRightVec
				new Transition (2023, 2031), // &dr -> &drc
				new Transition (2040, 2041), // &Ds -> &Dsc
				new Transition (2044, 2045), // &ds -> &dsc
				new Transition (2048, 2049), // &DS -> &DSc
				new Transition (2093, 2094), // &DZ -> &DZc
				new Transition (2097, 2098), // &dz -> &dzc
				new Transition (2108, 2127), // &E -> &Ec
				new Transition (2109, 2110), // &Ea -> &Eac
				new Transition (2115, 2133), // &e -> &ec
				new Transition (2116, 2117), // &ea -> &eac
				new Transition (2140, 2146), // &ecir -> &ecirc
				new Transition (2143, 2144), // &Ecir -> &Ecirc
				new Transition (2229, 2230), // &Ema -> &Emac
				new Transition (2234, 2235), // &ema -> &emac
				new Transition (2339, 2340), // &eq -> &eqc
				new Transition (2342, 2343), // &eqcir -> &eqcirc
				new Transition (2418, 2419), // &Es -> &Esc
				new Transition (2422, 2423), // &es -> &esc
				new Transition (2458, 2459), // &ex -> &exc
				new Transition (2473, 2474), // &expe -> &expec
				new Transition (2503, 2521), // &f -> &fc
				new Transition (2517, 2518), // &F -> &Fc
				new Transition (2648, 2649), // &fra -> &frac
				new Transition (2693, 2694), // &Fs -> &Fsc
				new Transition (2697, 2698), // &fs -> &fsc
				new Transition (2701, 2746), // &g -> &gc
				new Transition (2702, 2703), // &ga -> &gac
				new Transition (2708, 2736), // &G -> &Gc
				new Transition (2743, 2744), // &Gcir -> &Gcirc
				new Transition (2748, 2749), // &gcir -> &gcirc
				new Transition (2781, 2783), // &ges -> &gesc
				new Transition (2783, 2784), // &gesc -> &gescc
				new Transition (2816, 2817), // &GJ -> &GJc
				new Transition (2820, 2821), // &gj -> &gjc
				new Transition (2923, 2924), // &Gs -> &Gsc
				new Transition (2927, 2928), // &gs -> &gsc
				new Transition (2942, 2944), // &gt -> &gtc
				new Transition (2944, 2945), // &gtc -> &gtcc
				new Transition (3014, 3064), // &H -> &Hc
				new Transition (3015, 3016), // &Ha -> &Hac
				new Transition (3020, 3069), // &h -> &hc
				new Transition (3037, 3038), // &HARD -> &HARDc
				new Transition (3042, 3043), // &hard -> &hardc
				new Transition (3050, 3052), // &harr -> &harrc
				new Transition (3066, 3067), // &Hcir -> &Hcirc
				new Transition (3071, 3072), // &hcir -> &hcirc
				new Transition (3089, 3090), // &her -> &herc
				new Transition (3108, 3109), // &HilbertSpa -> &HilbertSpac
				new Transition (3184, 3185), // &Hs -> &Hsc
				new Transition (3188, 3189), // &hs -> &hsc
				new Transition (3236, 3252), // &I -> &Ic
				new Transition (3237, 3238), // &Ia -> &Iac
				new Transition (3243, 3250), // &i -> &ic
				new Transition (3244, 3245), // &ia -> &iac
				new Transition (3254, 3255), // &Icir -> &Icirc
				new Transition (3258, 3259), // &icir -> &icirc
				new Transition (3269, 3270), // &IE -> &IEc
				new Transition (3273, 3274), // &ie -> &iec
				new Transition (3277, 3278), // &iex -> &iexc
				new Transition (3332, 3333), // &Ima -> &Imac
				new Transition (3337, 3338), // &ima -> &imac
				new Transition (3378, 3380), // &in -> &inc
				new Transition (3401, 3403), // &int -> &intc
				new Transition (3419, 3420), // &inter -> &interc
				new Transition (3426, 3427), // &Interse -> &Intersec
				new Transition (3463, 3464), // &IO -> &IOc
				new Transition (3467, 3468), // &io -> &ioc
				new Transition (3503, 3504), // &Is -> &Isc
				new Transition (3507, 3508), // &is -> &isc
				new Transition (3540, 3541), // &Iuk -> &Iukc
				new Transition (3545, 3546), // &iuk -> &iukc
				new Transition (3555, 3556), // &J -> &Jc
				new Transition (3558, 3559), // &Jcir -> &Jcirc
				new Transition (3561, 3562), // &j -> &jc
				new Transition (3564, 3565), // &jcir -> &jcirc
				new Transition (3590, 3591), // &Js -> &Jsc
				new Transition (3594, 3595), // &js -> &jsc
				new Transition (3599, 3600), // &Jser -> &Jserc
				new Transition (3604, 3605), // &jser -> &jserc
				new Transition (3609, 3610), // &Juk -> &Jukc
				new Transition (3614, 3615), // &juk -> &jukc
				new Transition (3618, 3632), // &K -> &Kc
				new Transition (3624, 3638), // &k -> &kc
				new Transition (3660, 3661), // &KH -> &KHc
				new Transition (3664, 3665), // &kh -> &khc
				new Transition (3668, 3669), // &KJ -> &KJc
				new Transition (3672, 3673), // &kj -> &kjc
				new Transition (3684, 3685), // &Ks -> &Ksc
				new Transition (3688, 3689), // &ks -> &ksc
				new Transition (3692, 3843), // &l -> &lc
				new Transition (3698, 3837), // &L -> &Lc
				new Transition (3699, 3700), // &La -> &Lac
				new Transition (3705, 3706), // &la -> &lac
				new Transition (3748, 3749), // &Lapla -> &Laplac
				new Transition (3822, 3823), // &lbra -> &lbrac
				new Transition (3869, 3870), // &ld -> &ldc
				new Transition (3908, 3909), // &LeftAngleBra -> &LeftAngleBrac
				new Transition (3969, 3970), // &LeftDoubleBra -> &LeftDoubleBrac
				new Transition (3981, 3982), // &LeftDownTeeVe -> &LeftDownTeeVec
				new Transition (3988, 3989), // &LeftDownVe -> &LeftDownVec
				new Transition (4086, 4087), // &LeftRightVe -> &LeftRightVec
				new Transition (4103, 4104), // &LeftTeeVe -> &LeftTeeVec
				new Transition (4145, 4146), // &LeftUpDownVe -> &LeftUpDownVec
				new Transition (4155, 4156), // &LeftUpTeeVe -> &LeftUpTeeVec
				new Transition (4162, 4163), // &LeftUpVe -> &LeftUpVec
				new Transition (4173, 4174), // &LeftVe -> &LeftVec
				new Transition (4197, 4199), // &les -> &lesc
				new Transition (4199, 4200), // &lesc -> &lescc
				new Transition (4338, 4339), // &LJ -> &LJc
				new Transition (4342, 4343), // &lj -> &ljc
				new Transition (4348, 4354), // &ll -> &llc
				new Transition (4396, 4397), // &lmousta -> &lmoustac
				new Transition (4628, 4633), // &lr -> &lrc
				new Transition (4652, 4662), // &ls -> &lsc
				new Transition (4658, 4659), // &Ls -> &Lsc
				new Transition (4698, 4700), // &lt -> &ltc
				new Transition (4700, 4701), // &ltc -> &ltcc
				new Transition (4767, 4809), // &m -> &mc
				new Transition (4768, 4769), // &ma -> &mac
				new Transition (4781, 4815), // &M -> &Mc
				new Transition (4850, 4851), // &MediumSpa -> &MediumSpac
				new Transition (4871, 4872), // &mi -> &mic
				new Transition (4876, 4882), // &mid -> &midc
				new Transition (4909, 4910), // &ml -> &mlc
				new Transition (4937, 4938), // &Ms -> &Msc
				new Transition (4941, 4942), // &ms -> &msc
				new Transition (4965, 5020), // &n -> &nc
				new Transition (4966, 4978), // &na -> &nac
				new Transition (4971, 5024), // &N -> &Nc
				new Transition (4972, 4973), // &Na -> &Nac
				new Transition (5099, 5100), // &NegativeMediumSpa -> &NegativeMediumSpac
				new Transition (5105, 5106), // &NegativeThi -> &NegativeThic
				new Transition (5110, 5111), // &NegativeThickSpa -> &NegativeThickSpac
				new Transition (5117, 5118), // &NegativeThinSpa -> &NegativeThinSpac
				new Transition (5131, 5132), // &NegativeVeryThinSpa -> &NegativeVeryThinSpac
				new Transition (5248, 5249), // &NJ -> &NJc
				new Transition (5252, 5253), // &nj -> &njc
				new Transition (5365, 5366), // &NonBreakingSpa -> &NonBreakingSpac
				new Transition (5406, 5407), // &NotDoubleVerti -> &NotDoubleVertic
				new Transition (5521, 5526), // &notinv -> &notinvc
				new Transition (5623, 5628), // &notniv -> &notnivc
				new Transition (5632, 5633), // &NotPre -> &NotPrec
				new Transition (5726, 5738), // &NotSu -> &NotSuc
				new Transition (5738, 5739), // &NotSuc -> &NotSucc
				new Transition (5813, 5814), // &NotVerti -> &NotVertic
				new Transition (5842, 5844), // &npr -> &nprc
				new Transition (5848, 5850), // &npre -> &nprec
				new Transition (5862, 5864), // &nrarr -> &nrarrc
				new Transition (5895, 5896), // &ns -> &nsc
				new Transition (5896, 5898), // &nsc -> &nscc
				new Transition (5904, 5905), // &Ns -> &Nsc
				new Transition (5951, 5967), // &nsu -> &nsuc
				new Transition (5967, 5968), // &nsuc -> &nsucc
				new Transition (6131, 6152), // &O -> &Oc
				new Transition (6132, 6133), // &Oa -> &Oac
				new Transition (6138, 6148), // &o -> &oc
				new Transition (6139, 6140), // &oa -> &oac
				new Transition (6150, 6157), // &ocir -> &ocirc
				new Transition (6154, 6155), // &Ocir -> &Ocirc
				new Transition (6171, 6172), // &Odbla -> &Odblac
				new Transition (6176, 6177), // &odbla -> &odblac
				new Transition (6200, 6201), // &of -> &ofc
				new Transition (6238, 6243), // &ol -> &olc
				new Transition (6259, 6260), // &Oma -> &Omac
				new Transition (6264, 6265), // &oma -> &omac
				new Transition (6276, 6277), // &Omi -> &Omic
				new Transition (6282, 6283), // &omi -> &omic
				new Transition (6378, 6379), // &Os -> &Osc
				new Transition (6382, 6383), // &os -> &osc
				new Transition (6443, 6444), // &OverBra -> &OverBrac
				new Transition (6463, 6494), // &p -> &pc
				new Transition (6482, 6491), // &P -> &Pc
				new Transition (6498, 6499), // &per -> &perc
				new Transition (6545, 6546), // &pit -> &pitc
				new Transition (6557, 6558), // &plan -> &planc
				new Transition (6567, 6576), // &plus -> &plusc
				new Transition (6569, 6570), // &plusa -> &plusac
				new Transition (6611, 6612), // &Poin -> &Poinc
				new Transition (6642, 6647), // &pr -> &prc
				new Transition (6653, 6655), // &pre -> &prec
				new Transition (6655, 6664), // &prec -> &precc
				new Transition (6672, 6673), // &Pre -> &Prec
				new Transition (6750, 6751), // &Produ -> &Produc
				new Transition (6795, 6796), // &Ps -> &Psc
				new Transition (6799, 6800), // &ps -> &psc
				new Transition (6808, 6809), // &pun -> &punc
				new Transition (6839, 6840), // &Qs -> &Qsc
				new Transition (6843, 6844), // &qs -> &qsc
				new Transition (6876, 7027), // &r -> &rc
				new Transition (6882, 6883), // &ra -> &rac
				new Transition (6886, 7021), // &R -> &Rc
				new Transition (6887, 6888), // &Ra -> &Rac
				new Transition (6898, 6899), // &radi -> &radic
				new Transition (6932, 6942), // &rarr -> &rarrc
				new Transition (7006, 7007), // &rbra -> &rbrac
				new Transition (7053, 7054), // &rd -> &rdc
				new Transition (7074, 7089), // &re -> &rec
				new Transition (7182, 7183), // &RightAngleBra -> &RightAngleBrac
				new Transition (7244, 7245), // &RightDoubleBra -> &RightDoubleBrac
				new Transition (7256, 7257), // &RightDownTeeVe -> &RightDownTeeVec
				new Transition (7263, 7264), // &RightDownVe -> &RightDownVec
				new Transition (7348, 7349), // &RightTeeVe -> &RightTeeVec
				new Transition (7390, 7391), // &RightUpDownVe -> &RightUpDownVec
				new Transition (7400, 7401), // &RightUpTeeVe -> &RightUpTeeVec
				new Transition (7407, 7408), // &RightUpVe -> &RightUpVec
				new Transition (7418, 7419), // &RightVe -> &RightVec
				new Transition (7459, 7460), // &rmousta -> &rmoustac
				new Transition (7542, 7552), // &rs -> &rsc
				new Transition (7548, 7549), // &Rs -> &Rsc
				new Transition (7610, 7629), // &S -> &Sc
				new Transition (7611, 7612), // &Sa -> &Sac
				new Transition (7617, 7631), // &s -> &sc
				new Transition (7618, 7619), // &sa -> &sac
				new Transition (7631, 7645), // &sc -> &scc
				new Transition (7663, 7664), // &Scir -> &Scirc
				new Transition (7667, 7668), // &scir -> &scirc
				new Transition (7703, 7718), // &se -> &sec
				new Transition (7751, 7762), // &sh -> &shc
				new Transition (7756, 7767), // &SH -> &SHc
				new Transition (7758, 7759), // &SHCH -> &SHCHc
				new Transition (7763, 7764), // &shch -> &shchc
				new Transition (7889, 7890), // &SmallCir -> &SmallCirc
				new Transition (7932, 7933), // &SOFT -> &SOFTc
				new Transition (7938, 7939), // &soft -> &softc
				new Transition (7968, 7969), // &sq -> &sqc
				new Transition (8025, 8026), // &SquareInterse -> &SquareIntersec
				new Transition (8073, 8074), // &Ss -> &Ssc
				new Transition (8077, 8078), // &ss -> &ssc
				new Transition (8127, 8216), // &Su -> &Suc
				new Transition (8130, 8198), // &su -> &suc
				new Transition (8198, 8199), // &suc -> &succ
				new Transition (8199, 8208), // &succ -> &succc
				new Transition (8216, 8217), // &Suc -> &Succ
				new Transition (8400, 8419), // &T -> &Tc
				new Transition (8404, 8425), // &t -> &tc
				new Transition (8452, 8453), // &telre -> &telrec
				new Transition (8493, 8494), // &thi -> &thic
				new Transition (8507, 8508), // &Thi -> &Thic
				new Transition (8512, 8513), // &ThickSpa -> &ThickSpac
				new Transition (8523, 8524), // &ThinSpa -> &ThinSpac
				new Transition (8594, 8600), // &top -> &topc
				new Transition (8705, 8706), // &Ts -> &Tsc
				new Transition (8709, 8710), // &ts -> &tsc
				new Transition (8713, 8714), // &TS -> &TSc
				new Transition (8719, 8720), // &TSH -> &TSHc
				new Transition (8723, 8724), // &tsh -> &tshc
				new Transition (8768, 8815), // &U -> &Uc
				new Transition (8769, 8770), // &Ua -> &Uac
				new Transition (8775, 8820), // &u -> &uc
				new Transition (8776, 8777), // &ua -> &uac
				new Transition (8792, 8793), // &Uarro -> &Uarroc
				new Transition (8798, 8799), // &Ubr -> &Ubrc
				new Transition (8803, 8804), // &ubr -> &ubrc
				new Transition (8817, 8818), // &Ucir -> &Ucirc
				new Transition (8822, 8823), // &ucir -> &ucirc
				new Transition (8837, 8838), // &Udbla -> &Udblac
				new Transition (8842, 8843), // &udbla -> &udblac
				new Transition (8887, 8888), // &ul -> &ulc
				new Transition (8905, 8906), // &Uma -> &Umac
				new Transition (8910, 8911), // &uma -> &umac
				new Transition (8925, 8926), // &UnderBra -> &UnderBrac
				new Transition (9127, 9128), // &ur -> &urc
				new Transition (9153, 9154), // &Us -> &Usc
				new Transition (9157, 9158), // &us -> &usc
				new Transition (9201, 9317), // &v -> &vc
				new Transition (9303, 9314), // &V -> &Vc
				new Transition (9374, 9375), // &Verti -> &Vertic
				new Transition (9410, 9411), // &VeryThinSpa -> &VeryThinSpac
				new Transition (9450, 9451), // &Vs -> &Vsc
				new Transition (9454, 9455), // &vs -> &vsc
				new Transition (9484, 9485), // &W -> &Wc
				new Transition (9487, 9488), // &Wcir -> &Wcirc
				new Transition (9490, 9491), // &w -> &wc
				new Transition (9493, 9494), // &wcir -> &wcirc
				new Transition (9540, 9541), // &Ws -> &Wsc
				new Transition (9544, 9545), // &ws -> &wsc
				new Transition (9548, 9549), // &x -> &xc
				new Transition (9554, 9555), // &xcir -> &xcirc
				new Transition (9632, 9633), // &Xs -> &Xsc
				new Transition (9636, 9637), // &xs -> &xsc
				new Transition (9640, 9641), // &xsq -> &xsqc
				new Transition (9665, 9685), // &Y -> &Yc
				new Transition (9666, 9667), // &Ya -> &Yac
				new Transition (9672, 9690), // &y -> &yc
				new Transition (9673, 9674), // &ya -> &yac
				new Transition (9679, 9680), // &YA -> &YAc
				new Transition (9687, 9688), // &Ycir -> &Ycirc
				new Transition (9692, 9693), // &ycir -> &ycirc
				new Transition (9708, 9709), // &YI -> &YIc
				new Transition (9712, 9713), // &yi -> &yic
				new Transition (9724, 9725), // &Ys -> &Ysc
				new Transition (9728, 9729), // &ys -> &ysc
				new Transition (9732, 9733), // &YU -> &YUc
				new Transition (9736, 9737), // &yu -> &yuc
				new Transition (9747, 9761), // &Z -> &Zc
				new Transition (9748, 9749), // &Za -> &Zac
				new Transition (9754, 9767), // &z -> &zc
				new Transition (9755, 9756), // &za -> &zac
				new Transition (9801, 9802), // &ZeroWidthSpa -> &ZeroWidthSpac
				new Transition (9817, 9818), // &ZH -> &ZHc
				new Transition (9821, 9822), // &zh -> &zhc
				new Transition (9840, 9841), // &Zs -> &Zsc
				new Transition (9844, 9845) // &zs -> &zsc
			};
			TransitionTable_d = new Transition[206] {
				new Transition (0, 1432), // & -> &d
				new Transition (27, 29), // &ac -> &acd
				new Transition (116, 117), // &An -> &And
				new Transition (119, 120), // &an -> &and
				new Transition (120, 126), // &and -> &andd
				new Transition (123, 124), // &andan -> &andand
				new Transition (144, 145), // &angms -> &angmsd
				new Transition (147, 154), // &angmsda -> &angmsdad
				new Transition (168, 170), // &angrtvb -> &angrtvbd
				new Transition (210, 211), // &api -> &apid
				new Transition (271, 272), // &Atil -> &Atild
				new Transition (277, 278), // &atil -> &atild
				new Transition (301, 379), // &b -> &bd
				new Transition (350, 351), // &Barwe -> &Barwed
				new Transition (354, 355), // &barwe -> &barwed
				new Transition (455, 456), // &bigo -> &bigod
				new Transition (488, 489), // &bigtriangle -> &bigtriangled
				new Transition (508, 509), // &bigwe -> &bigwed
				new Transition (545, 547), // &blacktriangle -> &blacktriangled
				new Transition (613, 623), // &box -> &boxd
				new Transition (636, 642), // &boxH -> &boxHd
				new Transition (638, 646), // &boxh -> &boxhd
				new Transition (789, 907), // &C -> &Cd
				new Transition (796, 911), // &c -> &cd
				new Transition (805, 824), // &cap -> &capd
				new Transition (808, 809), // &capan -> &capand
				new Transition (876, 877), // &Cce -> &Cced
				new Transition (881, 882), // &cce -> &cced
				new Transition (915, 916), // &ce -> &ced
				new Transition (920, 921), // &Ce -> &Ced
				new Transition (945, 946), // &center -> &centerd
				new Transition (987, 1004), // &circle -> &circled
				new Transition (1004, 1014), // &circled -> &circledd
				new Transition (1060, 1061), // &cirmi -> &cirmid
				new Transition (1165, 1167), // &cong -> &congd
				new Transition (1207, 1208), // &copro -> &coprod
				new Transition (1211, 1212), // &Copro -> &Coprod
				new Transition (1287, 1288), // &ct -> &ctd
				new Transition (1292, 1293), // &cu -> &cud
				new Transition (1318, 1337), // &cup -> &cupd
				new Transition (1372, 1373), // &curlywe -> &curlywed
				new Transition (1404, 1405), // &cuwe -> &cuwed
				new Transition (1432, 1492), // &d -> &dd
				new Transition (1507, 1508), // &DDotrah -> &DDotrahd
				new Transition (1595, 1596), // &DiacriticalTil -> &DiacriticalTild
				new Transition (1605, 1606), // &Diamon -> &Diamond
				new Transition (1609, 1610), // &diamon -> &diamond
				new Transition (1645, 1646), // &divi -> &divid
				new Transition (1701, 1703), // &doteq -> &doteqd
				new Transition (1739, 1740), // &doublebarwe -> &doublebarwed
				new Transition (1896, 1921), // &down -> &downd
				new Transition (2067, 2068), // &dt -> &dtd
				new Transition (2108, 2162), // &E -> &Ed
				new Transition (2115, 2169), // &e -> &ed
				new Transition (2198, 2200), // &egs -> &egsd
				new Transition (2222, 2224), // &els -> &elsd
				new Transition (2379, 2380), // &EqualTil -> &EqualTild
				new Transition (2422, 2426), // &es -> &esd
				new Transition (2509, 2510), // &falling -> &fallingd
				new Transition (2557, 2558), // &Fille -> &Filled
				new Transition (2701, 2759), // &g -> &gd
				new Transition (2708, 2755), // &G -> &Gd
				new Transition (2712, 2718), // &Gamma -> &Gammad
				new Transition (2716, 2720), // &gamma -> &gammad
				new Transition (2737, 2738), // &Gce -> &Gced
				new Transition (2781, 2786), // &ges -> &gesd
				new Transition (2919, 2920), // &GreaterTil -> &GreaterTild
				new Transition (2942, 2950), // &gt -> &gtd
				new Transition (2965, 2976), // &gtr -> &gtrd
				new Transition (3041, 3042), // &har -> &hard
				new Transition (3236, 3265), // &I -> &Id
				new Transition (3369, 3370), // &impe -> &imped
				new Transition (3393, 3394), // &ino -> &inod
				new Transition (3441, 3442), // &intpro -> &intprod
				new Transition (3494, 3495), // &ipro -> &iprod
				new Transition (3512, 3514), // &isin -> &isind
				new Transition (3530, 3531), // &Itil -> &Itild
				new Transition (3535, 3536), // &itil -> &itild
				new Transition (3633, 3634), // &Kce -> &Kced
				new Transition (3639, 3640), // &kce -> &kced
				new Transition (3692, 3869), // &l -> &ld
				new Transition (3724, 3725), // &Lamb -> &Lambd
				new Transition (3729, 3730), // &lamb -> &lambd
				new Transition (3737, 3739), // &lang -> &langd
				new Transition (3832, 3833), // &lbrksl -> &lbrksld
				new Transition (3849, 3850), // &Lce -> &Lced
				new Transition (3854, 3855), // &lce -> &lced
				new Transition (3879, 3880), // &ldr -> &ldrd
				new Transition (4010, 4011), // &leftharpoon -> &leftharpoond
				new Transition (4197, 4202), // &les -> &lesd
				new Transition (4215, 4223), // &less -> &lessd
				new Transition (4297, 4298), // &LessTil -> &LessTild
				new Transition (4327, 4328), // &lhar -> &lhard
				new Transition (4372, 4373), // &llhar -> &llhard
				new Transition (4380, 4381), // &Lmi -> &Lmid
				new Transition (4386, 4387), // &lmi -> &lmid
				new Transition (4642, 4644), // &lrhar -> &lrhard
				new Transition (4698, 4706), // &lt -> &ltd
				new Transition (4743, 4744), // &lur -> &lurd
				new Transition (4767, 4820), // &m -> &md
				new Transition (4789, 4791), // &mapsto -> &mapstod
				new Transition (4835, 4836), // &measure -> &measured
				new Transition (4843, 4844), // &Me -> &Med
				new Transition (4871, 4876), // &mi -> &mid
				new Transition (4876, 4886), // &mid -> &midd
				new Transition (4892, 4896), // &minus -> &minusd
				new Transition (4909, 4913), // &ml -> &mld
				new Transition (4922, 4923), // &mo -> &mod
				new Transition (4965, 5059), // &n -> &nd
				new Transition (4990, 4991), // &napi -> &napid
				new Transition (5034, 5035), // &Nce -> &Nced
				new Transition (5039, 5040), // &nce -> &nced
				new Transition (5046, 5048), // &ncong -> &ncongd
				new Transition (5064, 5080), // &ne -> &ned
				new Transition (5092, 5093), // &NegativeMe -> &NegativeMed
				new Transition (5150, 5151), // &Neste -> &Nested
				new Transition (5242, 5244), // &nis -> &nisd
				new Transition (5256, 5265), // &nl -> &nld
				new Transition (5344, 5345), // &nmi -> &nmid
				new Transition (5429, 5430), // &NotEqualTil -> &NotEqualTild
				new Transition (5489, 5490), // &NotGreaterTil -> &NotGreaterTild
				new Transition (5513, 5515), // &notin -> &notind
				new Transition (5586, 5587), // &NotLessTil -> &NotLessTild
				new Transition (5594, 5595), // &NotNeste -> &NotNested
				new Transition (5634, 5635), // &NotPrece -> &NotPreced
				new Transition (5741, 5742), // &NotSuccee -> &NotSucceed
				new Transition (5764, 5765), // &NotSucceedsTil -> &NotSucceedsTild
				new Transition (5783, 5784), // &NotTil -> &NotTild
				new Transition (5805, 5806), // &NotTildeTil -> &NotTildeTild
				new Transition (5915, 5916), // &nshortmi -> &nshortmid
				new Transition (5935, 5936), // &nsmi -> &nsmid
				new Transition (5994, 5995), // &Ntil -> &Ntild
				new Transition (5999, 6000), // &ntil -> &ntild
				new Transition (6043, 6063), // &nv -> &nvd
				new Transition (6047, 6053), // &nV -> &nVd
				new Transition (6131, 6168), // &O -> &Od
				new Transition (6138, 6163), // &o -> &od
				new Transition (6187, 6188), // &odsol -> &odsold
				new Transition (6282, 6288), // &omi -> &omid
				new Transition (6342, 6348), // &or -> &ord
				new Transition (6401, 6402), // &Otil -> &Otild
				new Transition (6407, 6408), // &otil -> &otild
				new Transition (6504, 6505), // &perio -> &period
				new Transition (6567, 6580), // &plus -> &plusd
				new Transition (6637, 6638), // &poun -> &pound
				new Transition (6674, 6675), // &Prece -> &Preced
				new Transition (6698, 6699), // &PrecedesTil -> &PrecedesTild
				new Transition (6745, 6746), // &pro -> &prod
				new Transition (6748, 6749), // &Pro -> &Prod
				new Transition (6876, 7053), // &r -> &rd
				new Transition (6882, 6897), // &ra -> &rad
				new Transition (6912, 6914), // &rang -> &rangd
				new Transition (7016, 7017), // &rbrksl -> &rbrksld
				new Transition (7033, 7034), // &Rce -> &Rced
				new Transition (7038, 7039), // &rce -> &rced
				new Transition (7057, 7058), // &rdl -> &rdld
				new Transition (7157, 7158), // &rhar -> &rhard
				new Transition (7285, 7286), // &rightharpoon -> &rightharpoond
				new Transition (7434, 7435), // &rising -> &risingd
				new Transition (7466, 7467), // &rnmi -> &rnmid
				new Transition (7502, 7503), // &Roun -> &Round
				new Transition (7598, 7599), // &RuleDelaye -> &RuleDelayed
				new Transition (7617, 7695), // &s -> &sd
				new Transition (7651, 7658), // &sce -> &sced
				new Transition (7653, 7654), // &Sce -> &Sced
				new Transition (7800, 7801), // &shortmi -> &shortmid
				new Transition (7847, 7849), // &sim -> &simd
				new Transition (7918, 7919), // &smi -> &smid
				new Transition (7957, 7958), // &spa -> &spad
				new Transition (8131, 8133), // &sub -> &subd
				new Transition (8139, 8141), // &sube -> &subed
				new Transition (8219, 8220), // &Succee -> &Succeed
				new Transition (8242, 8243), // &SucceedsTil -> &SucceedsTild
				new Transition (8284, 8292), // &sup -> &supd
				new Transition (8302, 8304), // &supe -> &suped
				new Transition (8404, 8445), // &t -> &td
				new Transition (8431, 8432), // &Tce -> &Tced
				new Transition (8436, 8437), // &tce -> &tced
				new Transition (8545, 8546), // &Til -> &Tild
				new Transition (8550, 8551), // &til -> &tild
				new Transition (8572, 8573), // &TildeTil -> &TildeTild
				new Transition (8578, 8585), // &times -> &timesd
				new Transition (8629, 8630), // &tra -> &trad
				new Transition (8633, 8664), // &tri -> &trid
				new Transition (8638, 8640), // &triangle -> &triangled
				new Transition (8745, 8746), // &twohea -> &twohead
				new Transition (8768, 8834), // &U -> &Ud
				new Transition (8775, 8829), // &u -> &ud
				new Transition (8916, 8917), // &Un -> &Und
				new Transition (8970, 9014), // &Up -> &Upd
				new Transition (8983, 9024), // &up -> &upd
				new Transition (9161, 9162), // &ut -> &utd
				new Transition (9168, 9169), // &Util -> &Utild
				new Transition (9173, 9174), // &util -> &utild
				new Transition (9201, 9335), // &v -> &vd
				new Transition (9303, 9325), // &V -> &Vd
				new Transition (9399, 9400), // &VerticalTil -> &VerticalTild
				new Transition (9471, 9472), // &Vv -> &Vvd
				new Transition (9496, 9497), // &we -> &wed
				new Transition (9502, 9503), // &We -> &Wed
				new Transition (9548, 9560), // &x -> &xd
				new Transition (9602, 9603), // &xo -> &xod
				new Transition (9660, 9661), // &xwe -> &xwed
				new Transition (9747, 9777), // &Z -> &Zd
				new Transition (9754, 9781), // &z -> &zd
				new Transition (9795, 9796) // &ZeroWi -> &ZeroWid
			};
			TransitionTable_e = new Transition[674] {
				new Transition (0, 2115), // & -> &e
				new Transition (5, 6), // &Aacut -> &Aacute
				new Transition (8, 55), // &a -> &ae
				new Transition (12, 13), // &aacut -> &aacute
				new Transition (16, 17), // &Abr -> &Abre
				new Transition (18, 19), // &Abrev -> &Abreve
				new Transition (22, 23), // &abr -> &abre
				new Transition (24, 25), // &abrev -> &abreve
				new Transition (43, 44), // &acut -> &acute
				new Transition (70, 71), // &Agrav -> &Agrave
				new Transition (76, 77), // &agrav -> &agrave
				new Transition (79, 80), // &al -> &ale
				new Transition (131, 132), // &andslop -> &andslope
				new Transition (136, 138), // &ang -> &ange
				new Transition (140, 141), // &angl -> &angle
				new Transition (147, 156), // &angmsda -> &angmsdae
				new Transition (199, 208), // &ap -> &ape
				new Transition (232, 234), // &approx -> &approxe
				new Transition (264, 266), // &asymp -> &asympe
				new Transition (272, 273), // &Atild -> &Atilde
				new Transition (278, 279), // &atild -> &atilde
				new Transition (301, 384), // &b -> &be
				new Transition (304, 310), // &back -> &backe
				new Transition (321, 322), // &backprim -> &backprime
				new Transition (326, 328), // &backsim -> &backsime
				new Transition (331, 390), // &B -> &Be
				new Transition (345, 346), // &barv -> &barve
				new Transition (346, 347), // &barve -> &barvee
				new Transition (349, 350), // &Barw -> &Barwe
				new Transition (353, 354), // &barw -> &barwe
				new Transition (357, 358), // &barwedg -> &barwedge
				new Transition (388, 397), // &becaus -> &because
				new Transition (394, 395), // &Becaus -> &Because
				new Transition (431, 432), // &betw -> &betwe
				new Transition (432, 433), // &betwe -> &betwee
				new Transition (467, 468), // &bigotim -> &bigotime
				new Transition (487, 488), // &bigtriangl -> &bigtriangle
				new Transition (503, 504), // &bigv -> &bigve
				new Transition (504, 505), // &bigve -> &bigvee
				new Transition (507, 508), // &bigw -> &bigwe
				new Transition (510, 511), // &bigwedg -> &bigwedge
				new Transition (525, 526), // &blackloz -> &blackloze
				new Transition (528, 529), // &blacklozeng -> &blacklozenge
				new Transition (535, 536), // &blacksquar -> &blacksquare
				new Transition (544, 545), // &blacktriangl -> &blacktriangle
				new Transition (552, 553), // &blacktrianglel -> &blacktrianglele
				new Transition (579, 580), // &bn -> &bne
				new Transition (610, 611), // &bowti -> &bowtie
				new Transition (669, 670), // &boxtim -> &boxtime
				new Transition (722, 723), // &bprim -> &bprime
				new Transition (725, 726), // &Br -> &Bre
				new Transition (727, 728), // &Brev -> &Breve
				new Transition (730, 731), // &br -> &bre
				new Transition (732, 733), // &brev -> &breve
				new Transition (744, 748), // &bs -> &bse
				new Transition (753, 755), // &bsim -> &bsime
				new Transition (769, 771), // &bull -> &bulle
				new Transition (775, 779), // &bump -> &bumpe
				new Transition (783, 784), // &Bump -> &Bumpe
				new Transition (789, 920), // &C -> &Ce
				new Transition (793, 794), // &Cacut -> &Cacute
				new Transition (796, 915), // &c -> &ce
				new Transition (800, 801), // &cacut -> &cacute
				new Transition (835, 836), // &CapitalDiff -> &CapitalDiffe
				new Transition (837, 838), // &CapitalDiffer -> &CapitalDiffere
				new Transition (848, 849), // &car -> &care
				new Transition (856, 857), // &Cayl -> &Cayle
				new Transition (861, 881), // &cc -> &cce
				new Transition (866, 876), // &Cc -> &Cce
				new Transition (934, 944), // &cent -> &cente
				new Transition (937, 938), // &Cent -> &Cente
				new Transition (960, 964), // &ch -> &che
				new Transition (979, 1051), // &cir -> &cire
				new Transition (981, 983), // &circ -> &circe
				new Transition (986, 987), // &circl -> &circle
				new Transition (993, 994), // &circlearrowl -> &circlearrowle
				new Transition (1022, 1023), // &Circl -> &Circle
				new Transition (1045, 1046), // &CircleTim -> &CircleTime
				new Transition (1074, 1075), // &Clockwis -> &Clockwise
				new Transition (1085, 1086), // &ClockwiseContourInt -> &ClockwiseContourInte
				new Transition (1092, 1093), // &Clos -> &Close
				new Transition (1103, 1104), // &CloseCurlyDoubl -> &CloseCurlyDouble
				new Transition (1108, 1109), // &CloseCurlyDoubleQuot -> &CloseCurlyDoubleQuote
				new Transition (1114, 1115), // &CloseCurlyQuot -> &CloseCurlyQuote
				new Transition (1129, 1136), // &Colon -> &Colone
				new Transition (1134, 1138), // &colon -> &colone
				new Transition (1153, 1154), // &compl -> &comple
				new Transition (1155, 1156), // &complem -> &compleme
				new Transition (1160, 1161), // &complex -> &complexe
				new Transition (1174, 1175), // &Congru -> &Congrue
				new Transition (1193, 1194), // &ContourInt -> &ContourInte
				new Transition (1228, 1229), // &Count -> &Counte
				new Transition (1238, 1239), // &CounterClockwis -> &CounterClockwise
				new Transition (1249, 1250), // &CounterClockwiseContourInt -> &CounterClockwiseContourInte
				new Transition (1279, 1281), // &csub -> &csube
				new Transition (1283, 1285), // &csup -> &csupe
				new Transition (1292, 1301), // &cu -> &cue
				new Transition (1354, 1355), // &curly -> &curlye
				new Transition (1358, 1359), // &curlyeqpr -> &curlyeqpre
				new Transition (1367, 1368), // &curlyv -> &curlyve
				new Transition (1368, 1369), // &curlyve -> &curlyvee
				new Transition (1371, 1372), // &curlyw -> &curlywe
				new Transition (1374, 1375), // &curlywedg -> &curlywedge
				new Transition (1377, 1378), // &curr -> &curre
				new Transition (1381, 1382), // &curv -> &curve
				new Transition (1388, 1389), // &curvearrowl -> &curvearrowle
				new Transition (1399, 1400), // &cuv -> &cuve
				new Transition (1400, 1401), // &cuve -> &cuvee
				new Transition (1403, 1404), // &cuw -> &cuwe
				new Transition (1425, 1519), // &D -> &De
				new Transition (1428, 1429), // &Dagg -> &Dagge
				new Transition (1432, 1516), // &d -> &de
				new Transition (1435, 1436), // &dagg -> &dagge
				new Transition (1439, 1440), // &dal -> &dale
				new Transition (1496, 1497), // &ddagg -> &ddagge
				new Transition (1512, 1513), // &ddots -> &ddotse
				new Transition (1570, 1571), // &DiacriticalAcut -> &DiacriticalAcute
				new Transition (1579, 1580), // &DiacriticalDoubl -> &DiacriticalDouble
				new Transition (1584, 1585), // &DiacriticalDoubleAcut -> &DiacriticalDoubleAcute
				new Transition (1590, 1591), // &DiacriticalGrav -> &DiacriticalGrave
				new Transition (1596, 1597), // &DiacriticalTild -> &DiacriticalTilde
				new Transition (1599, 1619), // &di -> &die
				new Transition (1622, 1623), // &Diff -> &Diffe
				new Transition (1624, 1625), // &Differ -> &Differe
				new Transition (1646, 1647), // &divid -> &divide
				new Transition (1653, 1654), // &divideontim -> &divideontime
				new Transition (1694, 1700), // &dot -> &dote
				new Transition (1728, 1729), // &dotsquar -> &dotsquare
				new Transition (1733, 1734), // &doubl -> &double
				new Transition (1738, 1739), // &doublebarw -> &doublebarwe
				new Transition (1741, 1742), // &doublebarwedg -> &doublebarwedge
				new Transition (1746, 1747), // &Doubl -> &Double
				new Transition (1757, 1758), // &DoubleContourInt -> &DoubleContourInte
				new Transition (1776, 1777), // &DoubleL -> &DoubleLe
				new Transition (1797, 1798), // &DoubleLeftT -> &DoubleLeftTe
				new Transition (1798, 1799), // &DoubleLeftTe -> &DoubleLeftTee
				new Transition (1804, 1805), // &DoubleLongL -> &DoubleLongLe
				new Transition (1847, 1848), // &DoubleRightT -> &DoubleRightTe
				new Transition (1848, 1849), // &DoubleRightTe -> &DoubleRightTee
				new Transition (1869, 1870), // &DoubleV -> &DoubleVe
				new Transition (1916, 1917), // &DownBr -> &DownBre
				new Transition (1918, 1919), // &DownBrev -> &DownBreve
				new Transition (1939, 1940), // &downharpoonl -> &downharpoonle
				new Transition (1950, 1951), // &DownL -> &DownLe
				new Transition (1959, 1960), // &DownLeftRightV -> &DownLeftRightVe
				new Transition (1966, 1967), // &DownLeftT -> &DownLeftTe
				new Transition (1967, 1968), // &DownLeftTe -> &DownLeftTee
				new Transition (1969, 1970), // &DownLeftTeeV -> &DownLeftTeeVe
				new Transition (1976, 1977), // &DownLeftV -> &DownLeftVe
				new Transition (1992, 1993), // &DownRightT -> &DownRightTe
				new Transition (1993, 1994), // &DownRightTe -> &DownRightTee
				new Transition (1995, 1996), // &DownRightTeeV -> &DownRightTeeVe
				new Transition (2002, 2003), // &DownRightV -> &DownRightVe
				new Transition (2013, 2014), // &DownT -> &DownTe
				new Transition (2014, 2015), // &DownTe -> &DownTee
				new Transition (2090, 2091), // &dwangl -> &dwangle
				new Transition (2112, 2113), // &Eacut -> &Eacute
				new Transition (2115, 2173), // &e -> &ee
				new Transition (2119, 2120), // &eacut -> &eacute
				new Transition (2123, 2124), // &east -> &easte
				new Transition (2190, 2191), // &Egrav -> &Egrave
				new Transition (2195, 2196), // &egrav -> &egrave
				new Transition (2206, 2207), // &El -> &Ele
				new Transition (2208, 2209), // &Elem -> &Eleme
				new Transition (2215, 2216), // &elint -> &elinte
				new Transition (2242, 2243), // &emptys -> &emptyse
				new Transition (2258, 2259), // &EmptySmallSquar -> &EmptySmallSquare
				new Transition (2263, 2264), // &EmptyV -> &EmptyVe
				new Transition (2276, 2277), // &EmptyVerySmallSquar -> &EmptyVerySmallSquare
				new Transition (2362, 2363), // &eqslantl -> &eqslantle
				new Transition (2372, 2383), // &equ -> &eque
				new Transition (2380, 2381), // &EqualTild -> &EqualTilde
				new Transition (2472, 2473), // &exp -> &expe
				new Transition (2484, 2485), // &Expon -> &Expone
				new Transition (2494, 2495), // &expon -> &expone
				new Transition (2500, 2501), // &exponential -> &exponentiale
				new Transition (2503, 2524), // &f -> &fe
				new Transition (2513, 2514), // &fallingdots -> &fallingdotse
				new Transition (2527, 2528), // &femal -> &female
				new Transition (2556, 2557), // &Fill -> &Fille
				new Transition (2568, 2569), // &FilledSmallSquar -> &FilledSmallSquare
				new Transition (2571, 2572), // &FilledV -> &FilledVe
				new Transition (2584, 2585), // &FilledVerySmallSquar -> &FilledVerySmallSquare
				new Transition (2632, 2633), // &Fouri -> &Fourie
				new Transition (2701, 2765), // &g -> &ge
				new Transition (2705, 2706), // &gacut -> &gacute
				new Transition (2725, 2726), // &Gbr -> &Gbre
				new Transition (2727, 2728), // &Gbrev -> &Gbreve
				new Transition (2731, 2732), // &gbr -> &gbre
				new Transition (2733, 2734), // &gbrev -> &gbreve
				new Transition (2736, 2737), // &Gc -> &Gce
				new Transition (2794, 2796), // &gesl -> &gesle
				new Transition (2812, 2813), // &gim -> &gime
				new Transition (2832, 2843), // &gn -> &gne
				new Transition (2863, 2864), // &grav -> &grave
				new Transition (2866, 2867), // &Gr -> &Gre
				new Transition (2869, 2870), // &Great -> &Greate
				new Transition (2878, 2879), // &GreaterEqualL -> &GreaterEqualLe
				new Transition (2894, 2895), // &GreaterGr -> &GreaterGre
				new Transition (2897, 2898), // &GreaterGreat -> &GreaterGreate
				new Transition (2901, 2902), // &GreaterL -> &GreaterLe
				new Transition (2920, 2921), // &GreaterTild -> &GreaterTilde
				new Transition (2932, 2934), // &gsim -> &gsime
				new Transition (2960, 2961), // &gtqu -> &gtque
				new Transition (2965, 2980), // &gtr -> &gtre
				new Transition (2982, 2983), // &gtreql -> &gtreqle
				new Transition (2988, 2989), // &gtreqql -> &gtreqqle
				new Transition (2993, 2994), // &gtrl -> &gtrle
				new Transition (3002, 3003), // &gv -> &gve
				new Transition (3006, 3007), // &gvertn -> &gvertne
				new Transition (3016, 3017), // &Hac -> &Hace
				new Transition (3020, 3074), // &h -> &he
				new Transition (3102, 3103), // &Hilb -> &Hilbe
				new Transition (3109, 3110), // &HilbertSpac -> &HilbertSpace
				new Transition (3113, 3114), // &hks -> &hkse
				new Transition (3138, 3139), // &hookl -> &hookle
				new Transition (3181, 3182), // &HorizontalLin -> &HorizontalLine
				new Transition (3232, 3233), // &hyph -> &hyphe
				new Transition (3240, 3241), // &Iacut -> &Iacute
				new Transition (3243, 3273), // &i -> &ie
				new Transition (3247, 3248), // &iacut -> &iacute
				new Transition (3292, 3293), // &Igrav -> &Igrave
				new Transition (3298, 3299), // &igrav -> &igrave
				new Transition (3341, 3342), // &imag -> &image
				new Transition (3354, 3355), // &imaglin -> &imagline
				new Transition (3368, 3369), // &imp -> &impe
				new Transition (3374, 3375), // &Impli -> &Implie
				new Transition (3382, 3383), // &incar -> &incare
				new Transition (3390, 3391), // &infinti -> &infintie
				new Transition (3399, 3413), // &Int -> &Inte
				new Transition (3401, 3407), // &int -> &inte
				new Transition (3408, 3409), // &integ -> &intege
				new Transition (3425, 3426), // &Inters -> &Interse
				new Transition (3449, 3450), // &Invisibl -> &Invisible
				new Transition (3459, 3460), // &InvisibleTim -> &InvisibleTime
				new Transition (3498, 3499), // &iqu -> &ique
				new Transition (3531, 3532), // &Itild -> &Itilde
				new Transition (3536, 3537), // &itild -> &itilde
				new Transition (3590, 3598), // &Js -> &Jse
				new Transition (3594, 3603), // &js -> &jse
				new Transition (3632, 3633), // &Kc -> &Kce
				new Transition (3638, 3639), // &kc -> &kce
				new Transition (3655, 3656), // &kgr -> &kgre
				new Transition (3656, 3657), // &kgre -> &kgree
				new Transition (3692, 3896), // &l -> &le
				new Transition (3698, 3898), // &L -> &Le
				new Transition (3702, 3703), // &Lacut -> &Lacute
				new Transition (3705, 3711), // &la -> &lae
				new Transition (3708, 3709), // &lacut -> &lacute
				new Transition (3741, 3742), // &langl -> &langle
				new Transition (3749, 3750), // &Laplac -> &Laplace
				new Transition (3792, 3803), // &lat -> &late
				new Transition (3823, 3824), // &lbrac -> &lbrace
				new Transition (3828, 3829), // &lbrk -> &lbrke
				new Transition (3837, 3849), // &Lc -> &Lce
				new Transition (3843, 3854), // &lc -> &lce
				new Transition (3904, 3905), // &LeftAngl -> &LeftAngle
				new Transition (3910, 3911), // &LeftAngleBrack -> &LeftAngleBracke
				new Transition (3953, 3954), // &LeftC -> &LeftCe
				new Transition (3965, 3966), // &LeftDoubl -> &LeftDouble
				new Transition (3971, 3972), // &LeftDoubleBrack -> &LeftDoubleBracke
				new Transition (3977, 3978), // &LeftDownT -> &LeftDownTe
				new Transition (3978, 3979), // &LeftDownTe -> &LeftDownTee
				new Transition (3980, 3981), // &LeftDownTeeV -> &LeftDownTeeVe
				new Transition (3987, 3988), // &LeftDownV -> &LeftDownVe
				new Transition (4019, 4020), // &leftl -> &leftle
				new Transition (4085, 4086), // &LeftRightV -> &LeftRightVe
				new Transition (4092, 4093), // &LeftT -> &LeftTe
				new Transition (4093, 4094), // &LeftTe -> &LeftTee
				new Transition (4102, 4103), // &LeftTeeV -> &LeftTeeVe
				new Transition (4111, 4112), // &leftthr -> &leftthre
				new Transition (4112, 4113), // &leftthre -> &leftthree
				new Transition (4116, 4117), // &leftthreetim -> &leftthreetime
				new Transition (4125, 4126), // &LeftTriangl -> &LeftTriangle
				new Transition (4144, 4145), // &LeftUpDownV -> &LeftUpDownVe
				new Transition (4151, 4152), // &LeftUpT -> &LeftUpTe
				new Transition (4152, 4153), // &LeftUpTe -> &LeftUpTee
				new Transition (4154, 4155), // &LeftUpTeeV -> &LeftUpTeeVe
				new Transition (4161, 4162), // &LeftUpV -> &LeftUpVe
				new Transition (4172, 4173), // &LeftV -> &LeftVe
				new Transition (4210, 4212), // &lesg -> &lesge
				new Transition (4215, 4227), // &less -> &lesse
				new Transition (4246, 4247), // &LessEqualGr -> &LessEqualGre
				new Transition (4249, 4250), // &LessEqualGreat -> &LessEqualGreate
				new Transition (4264, 4265), // &LessGr -> &LessGre
				new Transition (4267, 4268), // &LessGreat -> &LessGreate
				new Transition (4275, 4276), // &LessL -> &LessLe
				new Transition (4298, 4299), // &LessTild -> &LessTilde
				new Transition (4346, 4361), // &Ll -> &Lle
				new Transition (4357, 4358), // &llcorn -> &llcorne
				new Transition (4398, 4399), // &lmoustach -> &lmoustache
				new Transition (4401, 4412), // &ln -> &lne
				new Transition (4437, 4438), // &LongL -> &LongLe
				new Transition (4447, 4448), // &Longl -> &Longle
				new Transition (4459, 4460), // &longl -> &longle
				new Transition (4549, 4550), // &looparrowl -> &looparrowle
				new Transition (4575, 4576), // &lotim -> &lotime
				new Transition (4588, 4589), // &Low -> &Lowe
				new Transition (4591, 4592), // &LowerL -> &LowerLe
				new Transition (4612, 4614), // &loz -> &loze
				new Transition (4616, 4617), // &lozeng -> &lozenge
				new Transition (4636, 4637), // &lrcorn -> &lrcorne
				new Transition (4670, 4672), // &lsim -> &lsime
				new Transition (4711, 4712), // &lthr -> &lthre
				new Transition (4712, 4713), // &lthre -> &lthree
				new Transition (4716, 4717), // &ltim -> &ltime
				new Transition (4726, 4727), // &ltqu -> &ltque
				new Transition (4732, 4734), // &ltri -> &ltrie
				new Transition (4755, 4756), // &lv -> &lve
				new Transition (4759, 4760), // &lvertn -> &lvertne
				new Transition (4767, 4830), // &m -> &me
				new Transition (4772, 4773), // &mal -> &male
				new Transition (4775, 4777), // &malt -> &malte
				new Transition (4778, 4779), // &maltes -> &maltese
				new Transition (4781, 4843), // &M -> &Me
				new Transition (4796, 4797), // &mapstol -> &mapstole
				new Transition (4805, 4806), // &mark -> &marke
				new Transition (4834, 4835), // &measur -> &measure
				new Transition (4840, 4841), // &measuredangl -> &measuredangle
				new Transition (4851, 4852), // &MediumSpac -> &MediumSpace
				new Transition (4923, 4924), // &mod -> &mode
				new Transition (4965, 5064), // &n -> &ne
				new Transition (4971, 5084), // &N -> &Ne
				new Transition (4975, 4976), // &Nacut -> &Nacute
				new Transition (4980, 4981), // &nacut -> &nacute
				new Transition (5016, 5018), // &nbump -> &nbumpe
				new Transition (5020, 5039), // &nc -> &nce
				new Transition (5024, 5034), // &Nc -> &Nce
				new Transition (5089, 5090), // &Negativ -> &Negative
				new Transition (5091, 5092), // &NegativeM -> &NegativeMe
				new Transition (5100, 5101), // &NegativeMediumSpac -> &NegativeMediumSpace
				new Transition (5111, 5112), // &NegativeThickSpac -> &NegativeThickSpace
				new Transition (5118, 5119), // &NegativeThinSpac -> &NegativeThinSpace
				new Transition (5121, 5122), // &NegativeV -> &NegativeVe
				new Transition (5132, 5133), // &NegativeVeryThinSpac -> &NegativeVeryThinSpace
				new Transition (5140, 5141), // &nes -> &nese
				new Transition (5149, 5150), // &Nest -> &Neste
				new Transition (5153, 5154), // &NestedGr -> &NestedGre
				new Transition (5156, 5157), // &NestedGreat -> &NestedGreate
				new Transition (5160, 5161), // &NestedGreaterGr -> &NestedGreaterGre
				new Transition (5163, 5164), // &NestedGreaterGreat -> &NestedGreaterGreate
				new Transition (5167, 5168), // &NestedL -> &NestedLe
				new Transition (5171, 5172), // &NestedLessL -> &NestedLessLe
				new Transition (5179, 5180), // &NewLin -> &NewLine
				new Transition (5195, 5198), // &ng -> &nge
				new Transition (5256, 5270), // &nl -> &nle
				new Transition (5272, 5273), // &nL -> &nLe
				new Transition (5337, 5339), // &nltri -> &nltrie
				new Transition (5349, 5350), // &NoBr -> &NoBre
				new Transition (5356, 5357), // &NonBr -> &NonBre
				new Transition (5366, 5367), // &NonBreakingSpac -> &NonBreakingSpace
				new Transition (5385, 5386), // &NotCongru -> &NotCongrue
				new Transition (5400, 5401), // &NotDoubl -> &NotDouble
				new Transition (5402, 5403), // &NotDoubleV -> &NotDoubleVe
				new Transition (5415, 5416), // &NotEl -> &NotEle
				new Transition (5417, 5418), // &NotElem -> &NotEleme
				new Transition (5430, 5431), // &NotEqualTild -> &NotEqualTilde
				new Transition (5440, 5441), // &NotGr -> &NotGre
				new Transition (5443, 5444), // &NotGreat -> &NotGreate
				new Transition (5464, 5465), // &NotGreaterGr -> &NotGreaterGre
				new Transition (5467, 5468), // &NotGreaterGreat -> &NotGreaterGreate
				new Transition (5471, 5472), // &NotGreaterL -> &NotGreaterLe
				new Transition (5490, 5491), // &NotGreaterTild -> &NotGreaterTilde
				new Transition (5528, 5529), // &NotL -> &NotLe
				new Transition (5538, 5539), // &NotLeftTriangl -> &NotLeftTriangle
				new Transition (5561, 5562), // &NotLessGr -> &NotLessGre
				new Transition (5564, 5565), // &NotLessGreat -> &NotLessGreate
				new Transition (5568, 5569), // &NotLessL -> &NotLessLe
				new Transition (5587, 5588), // &NotLessTild -> &NotLessTilde
				new Transition (5590, 5591), // &NotN -> &NotNe
				new Transition (5593, 5594), // &NotNest -> &NotNeste
				new Transition (5597, 5598), // &NotNestedGr -> &NotNestedGre
				new Transition (5600, 5601), // &NotNestedGreat -> &NotNestedGreate
				new Transition (5604, 5605), // &NotNestedGreaterGr -> &NotNestedGreaterGre
				new Transition (5607, 5608), // &NotNestedGreaterGreat -> &NotNestedGreaterGreate
				new Transition (5611, 5612), // &NotNestedL -> &NotNestedLe
				new Transition (5615, 5616), // &NotNestedLessL -> &NotNestedLessLe
				new Transition (5631, 5632), // &NotPr -> &NotPre
				new Transition (5633, 5634), // &NotPrec -> &NotPrece
				new Transition (5635, 5636), // &NotPreced -> &NotPrecede
				new Transition (5656, 5657), // &NotR -> &NotRe
				new Transition (5658, 5659), // &NotRev -> &NotReve
				new Transition (5661, 5662), // &NotRevers -> &NotReverse
				new Transition (5664, 5665), // &NotReverseEl -> &NotReverseEle
				new Transition (5666, 5667), // &NotReverseElem -> &NotReverseEleme
				new Transition (5681, 5682), // &NotRightTriangl -> &NotRightTriangle
				new Transition (5698, 5699), // &NotSquar -> &NotSquare
				new Transition (5703, 5704), // &NotSquareSubs -> &NotSquareSubse
				new Transition (5713, 5714), // &NotSquareSup -> &NotSquareSupe
				new Transition (5716, 5717), // &NotSquareSupers -> &NotSquareSuperse
				new Transition (5728, 5729), // &NotSubs -> &NotSubse
				new Transition (5739, 5740), // &NotSucc -> &NotSucce
				new Transition (5740, 5741), // &NotSucce -> &NotSuccee
				new Transition (5765, 5766), // &NotSucceedsTild -> &NotSucceedsTilde
				new Transition (5768, 5769), // &NotSup -> &NotSupe
				new Transition (5771, 5772), // &NotSupers -> &NotSuperse
				new Transition (5784, 5785), // &NotTild -> &NotTilde
				new Transition (5806, 5807), // &NotTildeTild -> &NotTildeTilde
				new Transition (5809, 5810), // &NotV -> &NotVe
				new Transition (5827, 5828), // &nparall -> &nparalle
				new Transition (5842, 5848), // &npr -> &npre
				new Transition (5845, 5846), // &nprcu -> &nprcue
				new Transition (5850, 5852), // &nprec -> &nprece
				new Transition (5891, 5893), // &nrtri -> &nrtrie
				new Transition (5896, 5902), // &nsc -> &nsce
				new Transition (5899, 5900), // &nsccu -> &nsccue
				new Transition (5923, 5924), // &nshortparall -> &nshortparalle
				new Transition (5928, 5930), // &nsim -> &nsime
				new Transition (5945, 5946), // &nsqsub -> &nsqsube
				new Transition (5948, 5949), // &nsqsup -> &nsqsupe
				new Transition (5952, 5956), // &nsub -> &nsube
				new Transition (5958, 5959), // &nsubs -> &nsubse
				new Transition (5960, 5962), // &nsubset -> &nsubsete
				new Transition (5968, 5970), // &nsucc -> &nsucce
				new Transition (5973, 5977), // &nsup -> &nsupe
				new Transition (5979, 5980), // &nsups -> &nsupse
				new Transition (5981, 5983), // &nsupset -> &nsupsete
				new Transition (5995, 5996), // &Ntild -> &Ntilde
				new Transition (6000, 6001), // &ntild -> &ntilde
				new Transition (6011, 6012), // &ntriangl -> &ntriangle
				new Transition (6013, 6014), // &ntrianglel -> &ntrianglele
				new Transition (6016, 6018), // &ntriangleleft -> &ntrianglelefte
				new Transition (6025, 6027), // &ntriangleright -> &ntrianglerighte
				new Transition (6034, 6036), // &num -> &nume
				new Transition (6068, 6069), // &nvg -> &nvge
				new Transition (6084, 6089), // &nvl -> &nvle
				new Transition (6094, 6095), // &nvltri -> &nvltrie
				new Transition (6104, 6105), // &nvrtri -> &nvrtrie
				new Transition (6126, 6127), // &nwn -> &nwne
				new Transition (6135, 6136), // &Oacut -> &Oacute
				new Transition (6138, 6195), // &o -> &oe
				new Transition (6142, 6143), // &oacut -> &oacute
				new Transition (6217, 6218), // &Ograv -> &Ograve
				new Transition (6222, 6223), // &ograv -> &ograve
				new Transition (6253, 6254), // &olin -> &oline
				new Transition (6258, 6268), // &Om -> &Ome
				new Transition (6263, 6272), // &om -> &ome
				new Transition (6302, 6332), // &op -> &ope
				new Transition (6306, 6307), // &Op -> &Ope
				new Transition (6318, 6319), // &OpenCurlyDoubl -> &OpenCurlyDouble
				new Transition (6323, 6324), // &OpenCurlyDoubleQuot -> &OpenCurlyDoubleQuote
				new Transition (6329, 6330), // &OpenCurlyQuot -> &OpenCurlyQuote
				new Transition (6348, 6350), // &ord -> &orde
				new Transition (6371, 6372), // &orslop -> &orslope
				new Transition (6402, 6403), // &Otild -> &Otilde
				new Transition (6408, 6409), // &otild -> &otilde
				new Transition (6411, 6412), // &Otim -> &Otime
				new Transition (6415, 6416), // &otim -> &otime
				new Transition (6435, 6436), // &Ov -> &Ove
				new Transition (6444, 6445), // &OverBrac -> &OverBrace
				new Transition (6447, 6448), // &OverBrack -> &OverBracke
				new Transition (6453, 6454), // &OverPar -> &OverPare
				new Transition (6457, 6458), // &OverParenth -> &OverParenthe
				new Transition (6463, 6497), // &p -> &pe
				new Transition (6470, 6471), // &parall -> &paralle
				new Transition (6513, 6514), // &pert -> &perte
				new Transition (6538, 6539), // &phon -> &phone
				new Transition (6567, 6585), // &plus -> &pluse
				new Transition (6614, 6615), // &Poincar -> &Poincare
				new Transition (6619, 6620), // &Poincareplan -> &Poincareplane
				new Transition (6640, 6672), // &Pr -> &Pre
				new Transition (6642, 6653), // &pr -> &pre
				new Transition (6648, 6649), // &prcu -> &prcue
				new Transition (6655, 6702), // &prec -> &prece
				new Transition (6668, 6669), // &preccurly -> &preccurlye
				new Transition (6673, 6674), // &Prec -> &Prece
				new Transition (6675, 6676), // &Preced -> &Precede
				new Transition (6699, 6700), // &PrecedesTild -> &PrecedesTilde
				new Transition (6705, 6713), // &precn -> &precne
				new Transition (6726, 6727), // &Prim -> &Prime
				new Transition (6730, 6731), // &prim -> &prime
				new Transition (6762, 6763), // &proflin -> &profline
				new Transition (6791, 6792), // &prur -> &prure
				new Transition (6836, 6837), // &qprim -> &qprime
				new Transition (6847, 6862), // &qu -> &que
				new Transition (6849, 6850), // &quat -> &quate
				new Transition (6864, 6866), // &quest -> &queste
				new Transition (6876, 7074), // &r -> &re
				new Transition (6882, 6901), // &ra -> &rae
				new Transition (6883, 6884), // &rac -> &race
				new Transition (6886, 7072), // &R -> &Re
				new Transition (6890, 6891), // &Racut -> &Racute
				new Transition (6894, 6895), // &racut -> &racute
				new Transition (6912, 6916), // &rang -> &range
				new Transition (6918, 6919), // &rangl -> &rangle
				new Transition (7007, 7008), // &rbrac -> &rbrace
				new Transition (7012, 7013), // &rbrk -> &rbrke
				new Transition (7021, 7033), // &Rc -> &Rce
				new Transition (7027, 7038), // &rc -> &rce
				new Transition (7079, 7080), // &realin -> &realine
				new Transition (7097, 7098), // &Rev -> &Reve
				new Transition (7100, 7101), // &Revers -> &Reverse
				new Transition (7103, 7104), // &ReverseEl -> &ReverseEle
				new Transition (7105, 7106), // &ReverseElem -> &ReverseEleme
				new Transition (7178, 7179), // &RightAngl -> &RightAngle
				new Transition (7184, 7185), // &RightAngleBrack -> &RightAngleBracke
				new Transition (7213, 7214), // &RightArrowL -> &RightArrowLe
				new Transition (7228, 7229), // &RightC -> &RightCe
				new Transition (7240, 7241), // &RightDoubl -> &RightDouble
				new Transition (7246, 7247), // &RightDoubleBrack -> &RightDoubleBracke
				new Transition (7252, 7253), // &RightDownT -> &RightDownTe
				new Transition (7253, 7254), // &RightDownTe -> &RightDownTee
				new Transition (7255, 7256), // &RightDownTeeV -> &RightDownTeeVe
				new Transition (7262, 7263), // &RightDownV -> &RightDownVe
				new Transition (7294, 7295), // &rightl -> &rightle
				new Transition (7337, 7338), // &RightT -> &RightTe
				new Transition (7338, 7339), // &RightTe -> &RightTee
				new Transition (7347, 7348), // &RightTeeV -> &RightTeeVe
				new Transition (7356, 7357), // &rightthr -> &rightthre
				new Transition (7357, 7358), // &rightthre -> &rightthree
				new Transition (7361, 7362), // &rightthreetim -> &rightthreetime
				new Transition (7370, 7371), // &RightTriangl -> &RightTriangle
				new Transition (7389, 7390), // &RightUpDownV -> &RightUpDownVe
				new Transition (7396, 7397), // &RightUpT -> &RightUpTe
				new Transition (7397, 7398), // &RightUpTe -> &RightUpTee
				new Transition (7399, 7400), // &RightUpTeeV -> &RightUpTeeVe
				new Transition (7406, 7407), // &RightUpV -> &RightUpVe
				new Transition (7417, 7418), // &RightV -> &RightVe
				new Transition (7438, 7439), // &risingdots -> &risingdotse
				new Transition (7461, 7462), // &rmoustach -> &rmoustache
				new Transition (7497, 7498), // &rotim -> &rotime
				new Transition (7508, 7509), // &RoundImpli -> &RoundImplie
				new Transition (7569, 7570), // &rthr -> &rthre
				new Transition (7570, 7571), // &rthre -> &rthree
				new Transition (7574, 7575), // &rtim -> &rtime
				new Transition (7579, 7581), // &rtri -> &rtrie
				new Transition (7591, 7592), // &Rul -> &Rule
				new Transition (7593, 7594), // &RuleD -> &RuleDe
				new Transition (7597, 7598), // &RuleDelay -> &RuleDelaye
				new Transition (7614, 7615), // &Sacut -> &Sacute
				new Transition (7617, 7703), // &s -> &se
				new Transition (7621, 7622), // &sacut -> &sacute
				new Transition (7629, 7653), // &Sc -> &Sce
				new Transition (7631, 7651), // &sc -> &sce
				new Transition (7646, 7647), // &sccu -> &sccue
				new Transition (7697, 7701), // &sdot -> &sdote
				new Transition (7786, 7787), // &ShortL -> &ShortLe
				new Transition (7808, 7809), // &shortparall -> &shortparalle
				new Transition (7847, 7853), // &sim -> &sime
				new Transition (7865, 7866), // &simn -> &simne
				new Transition (7891, 7892), // &SmallCircl -> &SmallCircle
				new Transition (7894, 7911), // &sm -> &sme
				new Transition (7898, 7899), // &smalls -> &smallse
				new Transition (7921, 7922), // &smil -> &smile
				new Transition (7924, 7926), // &smt -> &smte
				new Transition (7958, 7959), // &spad -> &spade
				new Transition (7986, 7988), // &sqsub -> &sqsube
				new Transition (7990, 7991), // &sqsubs -> &sqsubse
				new Transition (7992, 7994), // &sqsubset -> &sqsubsete
				new Transition (7997, 7999), // &sqsup -> &sqsupe
				new Transition (8001, 8002), // &sqsups -> &sqsupse
				new Transition (8003, 8005), // &sqsupset -> &sqsupsete
				new Transition (8012, 8013), // &Squar -> &Square
				new Transition (8016, 8017), // &squar -> &square
				new Transition (8021, 8022), // &SquareInt -> &SquareInte
				new Transition (8024, 8025), // &SquareInters -> &SquareInterse
				new Transition (8035, 8036), // &SquareSubs -> &SquareSubse
				new Transition (8045, 8046), // &SquareSup -> &SquareSupe
				new Transition (8048, 8049), // &SquareSupers -> &SquareSuperse
				new Transition (8077, 8081), // &ss -> &sse
				new Transition (8088, 8089), // &ssmil -> &ssmile
				new Transition (8111, 8112), // &straight -> &straighte
				new Transition (8131, 8139), // &sub -> &sube
				new Transition (8150, 8153), // &subn -> &subne
				new Transition (8165, 8166), // &Subs -> &Subse
				new Transition (8169, 8170), // &subs -> &subse
				new Transition (8171, 8173), // &subset -> &subsete
				new Transition (8184, 8185), // &subsetn -> &subsetne
				new Transition (8199, 8246), // &succ -> &succe
				new Transition (8212, 8213), // &succcurly -> &succcurlye
				new Transition (8217, 8218), // &Succ -> &Succe
				new Transition (8218, 8219), // &Succe -> &Succee
				new Transition (8243, 8244), // &SucceedsTild -> &SucceedsTilde
				new Transition (8249, 8257), // &succn -> &succne
				new Transition (8282, 8308), // &Sup -> &Supe
				new Transition (8284, 8302), // &sup -> &supe
				new Transition (8310, 8311), // &Supers -> &Superse
				new Transition (8338, 8341), // &supn -> &supne
				new Transition (8348, 8349), // &Sups -> &Supse
				new Transition (8352, 8353), // &sups -> &supse
				new Transition (8354, 8356), // &supset -> &supsete
				new Transition (8361, 8362), // &supsetn -> &supsetne
				new Transition (8404, 8449), // &t -> &te
				new Transition (8407, 8408), // &targ -> &targe
				new Transition (8419, 8431), // &Tc -> &Tce
				new Transition (8425, 8436), // &tc -> &tce
				new Transition (8451, 8452), // &telr -> &telre
				new Transition (8461, 8462), // &th -> &the
				new Transition (8463, 8464), // &ther -> &there
				new Transition (8467, 8468), // &Th -> &The
				new Transition (8469, 8470), // &Ther -> &There
				new Transition (8473, 8474), // &Therefor -> &Therefore
				new Transition (8478, 8479), // &therefor -> &therefore
				new Transition (8513, 8514), // &ThickSpac -> &ThickSpace
				new Transition (8524, 8525), // &ThinSpac -> &ThinSpace
				new Transition (8546, 8547), // &Tild -> &Tilde
				new Transition (8551, 8552), // &tild -> &tilde
				new Transition (8573, 8574), // &TildeTild -> &TildeTilde
				new Transition (8576, 8577), // &tim -> &time
				new Transition (8590, 8591), // &to -> &toe
				new Transition (8620, 8621), // &tprim -> &tprime
				new Transition (8630, 8631), // &trad -> &trade
				new Transition (8633, 8668), // &tri -> &trie
				new Transition (8637, 8638), // &triangl -> &triangle
				new Transition (8645, 8646), // &trianglel -> &trianglele
				new Transition (8648, 8650), // &triangleleft -> &trianglelefte
				new Transition (8659, 8661), // &triangleright -> &trianglerighte
				new Transition (8679, 8680), // &Tripl -> &Triple
				new Transition (8695, 8696), // &tritim -> &tritime
				new Transition (8698, 8699), // &trp -> &trpe
				new Transition (8743, 8744), // &twoh -> &twohe
				new Transition (8747, 8748), // &twoheadl -> &twoheadle
				new Transition (8772, 8773), // &Uacut -> &Uacute
				new Transition (8779, 8780), // &uacut -> &uacute
				new Transition (8798, 8807), // &Ubr -> &Ubre
				new Transition (8803, 8811), // &ubr -> &ubre
				new Transition (8808, 8809), // &Ubrev -> &Ubreve
				new Transition (8812, 8813), // &ubrev -> &ubreve
				new Transition (8863, 8864), // &Ugrav -> &Ugrave
				new Transition (8869, 8870), // &ugrav -> &ugrave
				new Transition (8891, 8893), // &ulcorn -> &ulcorne
				new Transition (8917, 8918), // &Und -> &Unde
				new Transition (8926, 8927), // &UnderBrac -> &UnderBrace
				new Transition (8929, 8930), // &UnderBrack -> &UnderBracke
				new Transition (8935, 8936), // &UnderPar -> &UnderPare
				new Transition (8939, 8940), // &UnderParenth -> &UnderParenthe
				new Transition (9053, 9054), // &upharpoonl -> &upharpoonle
				new Transition (9068, 9069), // &Upp -> &Uppe
				new Transition (9071, 9072), // &UpperL -> &UpperLe
				new Transition (9108, 9109), // &UpT -> &UpTe
				new Transition (9109, 9110), // &UpTe -> &UpTee
				new Transition (9131, 9133), // &urcorn -> &urcorne
				new Transition (9169, 9170), // &Utild -> &Utilde
				new Transition (9174, 9175), // &utild -> &utilde
				new Transition (9198, 9199), // &uwangl -> &uwangle
				new Transition (9201, 9345), // &v -> &ve
				new Transition (9208, 9209), // &var -> &vare
				new Transition (9260, 9261), // &varsubs -> &varsubse
				new Transition (9263, 9264), // &varsubsetn -> &varsubsetne
				new Transition (9270, 9271), // &varsups -> &varsupse
				new Transition (9273, 9274), // &varsupsetn -> &varsupsetne
				new Transition (9280, 9281), // &varth -> &varthe
				new Transition (9290, 9291), // &vartriangl -> &vartriangle
				new Transition (9292, 9293), // &vartrianglel -> &vartrianglele
				new Transition (9303, 9342), // &V -> &Ve
				new Transition (9342, 9343), // &Ve -> &Vee
				new Transition (9345, 9346), // &ve -> &vee
				new Transition (9346, 9352), // &vee -> &veee
				new Transition (9384, 9385), // &VerticalLin -> &VerticalLine
				new Transition (9387, 9388), // &VerticalS -> &VerticalSe
				new Transition (9400, 9401), // &VerticalTild -> &VerticalTilde
				new Transition (9411, 9412), // &VeryThinSpac -> &VeryThinSpace
				new Transition (9460, 9463), // &vsubn -> &vsubne
				new Transition (9466, 9469), // &vsupn -> &vsupne
				new Transition (9484, 9502), // &W -> &We
				new Transition (9490, 9496), // &w -> &we
				new Transition (9504, 9505), // &Wedg -> &Wedge
				new Transition (9507, 9508), // &wedg -> &wedge
				new Transition (9512, 9513), // &wei -> &weie
				new Transition (9533, 9535), // &wr -> &wre
				new Transition (9620, 9621), // &xotim -> &xotime
				new Transition (9655, 9656), // &xv -> &xve
				new Transition (9656, 9657), // &xve -> &xvee
				new Transition (9659, 9660), // &xw -> &xwe
				new Transition (9662, 9663), // &xwedg -> &xwedge
				new Transition (9669, 9670), // &Yacut -> &Yacute
				new Transition (9672, 9699), // &y -> &ye
				new Transition (9676, 9677), // &yacut -> &yacute
				new Transition (9747, 9791), // &Z -> &Ze
				new Transition (9751, 9752), // &Zacut -> &Zacute
				new Transition (9754, 9785), // &z -> &ze
				new Transition (9758, 9759), // &zacut -> &zacute
				new Transition (9785, 9786), // &ze -> &zee
				new Transition (9802, 9803) // &ZeroWidthSpac -> &ZeroWidthSpace
			};
			TransitionTable_f = new Transition[177] {
				new Transition (0, 2503), // & -> &f
				new Transition (1, 62), // &A -> &Af
				new Transition (8, 60), // &a -> &af
				new Transition (80, 81), // &ale -> &alef
				new Transition (147, 158), // &angmsda -> &angmsdaf
				new Transition (193, 194), // &Aop -> &Aopf
				new Transition (196, 197), // &aop -> &aopf
				new Transition (301, 439), // &b -> &bf
				new Transition (331, 436), // &B -> &Bf
				new Transition (553, 554), // &blacktrianglele -> &blacktrianglelef
				new Transition (595, 596), // &Bop -> &Bopf
				new Transition (599, 600), // &bop -> &bopf
				new Transition (789, 950), // &C -> &Cf
				new Transition (796, 953), // &c -> &cf
				new Transition (833, 834), // &CapitalDi -> &CapitalDif
				new Transition (834, 835), // &CapitalDif -> &CapitalDiff
				new Transition (979, 1053), // &cir -> &cirf
				new Transition (994, 995), // &circlearrowle -> &circlearrowlef
				new Transition (1148, 1150), // &comp -> &compf
				new Transition (1200, 1201), // &Cop -> &Copf
				new Transition (1203, 1204), // &cop -> &copf
				new Transition (1389, 1390), // &curvearrowle -> &curvearrowlef
				new Transition (1425, 1541), // &D -> &Df
				new Transition (1432, 1535), // &d -> &df
				new Transition (1557, 1621), // &Di -> &Dif
				new Transition (1621, 1622), // &Dif -> &Diff
				new Transition (1686, 1687), // &Dop -> &Dopf
				new Transition (1689, 1690), // &dop -> &dopf
				new Transition (1777, 1778), // &DoubleLe -> &DoubleLef
				new Transition (1805, 1806), // &DoubleLongLe -> &DoubleLongLef
				new Transition (1940, 1941), // &downharpoonle -> &downharpoonlef
				new Transition (1951, 1952), // &DownLe -> &DownLef
				new Transition (2073, 2075), // &dtri -> &dtrif
				new Transition (2108, 2180), // &E -> &Ef
				new Transition (2115, 2175), // &e -> &ef
				new Transition (2306, 2307), // &Eop -> &Eopf
				new Transition (2309, 2310), // &eop -> &eopf
				new Transition (2503, 2530), // &f -> &ff
				new Transition (2517, 2544), // &F -> &Ff
				new Transition (2605, 2606), // &fno -> &fnof
				new Transition (2609, 2610), // &Fop -> &Fopf
				new Transition (2613, 2614), // &fop -> &fopf
				new Transition (2636, 2637), // &Fouriertr -> &Fouriertrf
				new Transition (2701, 2802), // &g -> &gf
				new Transition (2708, 2799), // &G -> &Gf
				new Transition (2854, 2855), // &Gop -> &Gopf
				new Transition (2858, 2859), // &gop -> &gopf
				new Transition (3014, 3094), // &H -> &Hf
				new Transition (3020, 3097), // &h -> &hf
				new Transition (3027, 3028), // &hal -> &half
				new Transition (3139, 3140), // &hookle -> &hooklef
				new Transition (3160, 3161), // &Hop -> &Hopf
				new Transition (3163, 3164), // &hop -> &hopf
				new Transition (3236, 3284), // &I -> &If
				new Transition (3243, 3281), // &i -> &if
				new Transition (3281, 3282), // &if -> &iff
				new Transition (3311, 3312), // &iin -> &iinf
				new Transition (3365, 3366), // &imo -> &imof
				new Transition (3378, 3385), // &in -> &inf
				new Transition (3480, 3481), // &Iop -> &Iopf
				new Transition (3483, 3484), // &iop -> &iopf
				new Transition (3555, 3571), // &J -> &Jf
				new Transition (3561, 3574), // &j -> &jf
				new Transition (3583, 3584), // &Jop -> &Jopf
				new Transition (3587, 3588), // &jop -> &jopf
				new Transition (3618, 3648), // &K -> &Kf
				new Transition (3624, 3651), // &k -> &kf
				new Transition (3677, 3678), // &Kop -> &Kopf
				new Transition (3681, 3682), // &kop -> &kopf
				new Transition (3692, 4301), // &l -> &lf
				new Transition (3698, 4312), // &L -> &Lf
				new Transition (3752, 3753), // &Laplacetr -> &Laplacetrf
				new Transition (3766, 3773), // &larr -> &larrf
				new Transition (3768, 3770), // &larrb -> &larrbf
				new Transition (3896, 3925), // &le -> &lef
				new Transition (3898, 3899), // &Le -> &Lef
				new Transition (4020, 4021), // &leftle -> &leftlef
				new Transition (4361, 4362), // &Lle -> &Llef
				new Transition (4438, 4439), // &LongLe -> &LongLef
				new Transition (4448, 4449), // &Longle -> &Longlef
				new Transition (4460, 4461), // &longle -> &longlef
				new Transition (4550, 4551), // &looparrowle -> &looparrowlef
				new Transition (4560, 4567), // &lop -> &lopf
				new Transition (4564, 4565), // &Lop -> &Lopf
				new Transition (4592, 4593), // &LowerLe -> &LowerLef
				new Transition (4612, 4619), // &loz -> &lozf
				new Transition (4732, 4736), // &ltri -> &ltrif
				new Transition (4767, 4865), // &m -> &mf
				new Transition (4781, 4862), // &M -> &Mf
				new Transition (4797, 4798), // &mapstole -> &mapstolef
				new Transition (4859, 4860), // &Mellintr -> &Mellintrf
				new Transition (4929, 4930), // &Mop -> &Mopf
				new Transition (4932, 4933), // &mop -> &mopf
				new Transition (4965, 5192), // &n -> &nf
				new Transition (4971, 5189), // &N -> &Nf
				new Transition (5270, 5282), // &nle -> &nlef
				new Transition (5273, 5274), // &nLe -> &nLef
				new Transition (5369, 5370), // &Nop -> &Nopf
				new Transition (5373, 5374), // &nop -> &nopf
				new Transition (5529, 5530), // &NotLe -> &NotLef
				new Transition (6014, 6015), // &ntrianglele -> &ntrianglelef
				new Transition (6079, 6080), // &nvin -> &nvinf
				new Transition (6131, 6205), // &O -> &Of
				new Transition (6138, 6200), // &o -> &of
				new Transition (6295, 6296), // &Oop -> &Oopf
				new Transition (6299, 6300), // &oop -> &oopf
				new Transition (6348, 6356), // &ord -> &ordf
				new Transition (6353, 6354), // &ordero -> &orderof
				new Transition (6362, 6363), // &origo -> &origof
				new Transition (6463, 6521), // &p -> &pf
				new Transition (6482, 6518), // &P -> &Pf
				new Transition (6547, 6548), // &pitch -> &pitchf
				new Transition (6630, 6631), // &Pop -> &Popf
				new Transition (6633, 6634), // &pop -> &popf
				new Transition (6745, 6754), // &pro -> &prof
				new Transition (6767, 6768), // &profsur -> &profsurf
				new Transition (6813, 6814), // &Q -> &Qf
				new Transition (6817, 6818), // &q -> &qf
				new Transition (6826, 6827), // &Qop -> &Qopf
				new Transition (6830, 6831), // &qop -> &qopf
				new Transition (6876, 7135), // &r -> &rf
				new Transition (6886, 7146), // &R -> &Rf
				new Transition (6932, 6944), // &rarr -> &rarrf
				new Transition (6937, 6939), // &rarrb -> &rarrbf
				new Transition (7214, 7215), // &RightArrowLe -> &RightArrowLef
				new Transition (7295, 7296), // &rightle -> &rightlef
				new Transition (7481, 7489), // &rop -> &ropf
				new Transition (7486, 7487), // &Rop -> &Ropf
				new Transition (7579, 7583), // &rtri -> &rtrif
				new Transition (7610, 7741), // &S -> &Sf
				new Transition (7617, 7744), // &s -> &sf
				new Transition (7787, 7788), // &ShortLe -> &ShortLef
				new Transition (7841, 7843), // &sigma -> &sigmaf
				new Transition (7936, 7937), // &so -> &sof
				new Transition (7950, 7951), // &Sop -> &Sopf
				new Transition (7953, 7954), // &sop -> &sopf
				new Transition (8008, 8066), // &squ -> &squf
				new Transition (8016, 8064), // &squar -> &squarf
				new Transition (8093, 8094), // &sstar -> &sstarf
				new Transition (8102, 8104), // &star -> &starf
				new Transition (8400, 8455), // &T -> &Tf
				new Transition (8404, 8458), // &t -> &tf
				new Transition (8464, 8476), // &there -> &theref
				new Transition (8470, 8471), // &There -> &Theref
				new Transition (8594, 8608), // &top -> &topf
				new Transition (8605, 8606), // &Top -> &Topf
				new Transition (8646, 8647), // &trianglele -> &trianglelef
				new Transition (8748, 8749), // &twoheadle -> &twoheadlef
				new Transition (8768, 8855), // &U -> &Uf
				new Transition (8775, 8849), // &u -> &uf
				new Transition (8964, 8965), // &Uop -> &Uopf
				new Transition (8967, 8968), // &uop -> &uopf
				new Transition (9054, 9055), // &upharpoonle -> &upharpoonlef
				new Transition (9072, 9073), // &UpperLe -> &UpperLef
				new Transition (9178, 9180), // &utri -> &utrif
				new Transition (9201, 9417), // &v -> &vf
				new Transition (9293, 9294), // &vartrianglele -> &vartrianglelef
				new Transition (9303, 9414), // &V -> &Vf
				new Transition (9433, 9434), // &Vop -> &Vopf
				new Transition (9437, 9438), // &vop -> &vopf
				new Transition (9484, 9517), // &W -> &Wf
				new Transition (9490, 9520), // &w -> &wf
				new Transition (9524, 9525), // &Wop -> &Wopf
				new Transition (9528, 9529), // &wop -> &wopf
				new Transition (9548, 9569), // &x -> &xf
				new Transition (9565, 9566), // &X -> &Xf
				new Transition (9608, 9609), // &Xop -> &Xopf
				new Transition (9611, 9612), // &xop -> &xopf
				new Transition (9665, 9702), // &Y -> &Yf
				new Transition (9672, 9705), // &y -> &yf
				new Transition (9717, 9718), // &Yop -> &Yopf
				new Transition (9721, 9722), // &yop -> &yopf
				new Transition (9747, 9811), // &Z -> &Zf
				new Transition (9754, 9814), // &z -> &zf
				new Transition (9788, 9789), // &zeetr -> &zeetrf
				new Transition (9833, 9834), // &Zop -> &Zopf
				new Transition (9837, 9838) // &zop -> &zopf
			};
			TransitionTable_g = new Transition[182] {
				new Transition (0, 2701), // & -> &g
				new Transition (1, 67), // &A -> &Ag
				new Transition (8, 73), // &a -> &ag
				new Transition (52, 53), // &AEli -> &AElig
				new Transition (57, 58), // &aeli -> &aelig
				new Transition (108, 109), // &amal -> &amalg
				new Transition (119, 136), // &an -> &ang
				new Transition (147, 160), // &angmsda -> &angmsdag
				new Transition (183, 184), // &Ao -> &Aog
				new Transition (188, 189), // &ao -> &aog
				new Transition (239, 240), // &Arin -> &Aring
				new Transition (244, 245), // &arin -> &aring
				new Transition (256, 257), // &Assi -> &Assig
				new Transition (307, 308), // &backcon -> &backcong
				new Transition (355, 357), // &barwed -> &barwedg
				new Transition (371, 372), // &bcon -> &bcong
				new Transition (442, 443), // &bi -> &big
				new Transition (485, 486), // &bigtrian -> &bigtriang
				new Transition (509, 510), // &bigwed -> &bigwedg
				new Transition (527, 528), // &blacklozen -> &blacklozeng
				new Transition (542, 543), // &blacktrian -> &blacktriang
				new Transition (558, 559), // &blacktriangleri -> &blacktrianglerig
				new Transition (999, 1000), // &circlearrowri -> &circlearrowrig
				new Transition (1086, 1087), // &ClockwiseContourInte -> &ClockwiseContourInteg
				new Transition (1164, 1165), // &con -> &cong
				new Transition (1171, 1172), // &Con -> &Cong
				new Transition (1194, 1195), // &ContourInte -> &ContourInteg
				new Transition (1250, 1251), // &CounterClockwiseContourInte -> &CounterClockwiseContourInteg
				new Transition (1373, 1374), // &curlywed -> &curlywedg
				new Transition (1394, 1395), // &curvearrowri -> &curvearrowrig
				new Transition (1426, 1427), // &Da -> &Dag
				new Transition (1427, 1428), // &Dag -> &Dagg
				new Transition (1433, 1434), // &da -> &dag
				new Transition (1434, 1435), // &dag -> &dagg
				new Transition (1494, 1495), // &dda -> &ddag
				new Transition (1495, 1496), // &ddag -> &ddagg
				new Transition (1516, 1517), // &de -> &deg
				new Transition (1599, 1633), // &di -> &dig
				new Transition (1740, 1741), // &doublebarwed -> &doublebarwedg
				new Transition (1758, 1759), // &DoubleContourInte -> &DoubleContourInteg
				new Transition (1787, 1788), // &DoubleLeftRi -> &DoubleLeftRig
				new Transition (1802, 1803), // &DoubleLon -> &DoubleLong
				new Transition (1815, 1816), // &DoubleLongLeftRi -> &DoubleLongLeftRig
				new Transition (1826, 1827), // &DoubleLongRi -> &DoubleLongRig
				new Transition (1837, 1838), // &DoubleRi -> &DoubleRig
				new Transition (1945, 1946), // &downharpoonri -> &downharpoonrig
				new Transition (1955, 1956), // &DownLeftRi -> &DownLeftRig
				new Transition (1988, 1989), // &DownRi -> &DownRig
				new Transition (2088, 2089), // &dwan -> &dwang
				new Transition (2101, 2102), // &dzi -> &dzig
				new Transition (2108, 2187), // &E -> &Eg
				new Transition (2115, 2185), // &e -> &eg
				new Transition (2290, 2291), // &en -> &eng
				new Transition (2296, 2297), // &Eo -> &Eog
				new Transition (2301, 2302), // &eo -> &eog
				new Transition (2357, 2358), // &eqslant -> &eqslantg
				new Transition (2508, 2509), // &fallin -> &falling
				new Transition (2533, 2534), // &ffili -> &ffilig
				new Transition (2537, 2538), // &ffli -> &fflig
				new Transition (2541, 2542), // &fflli -> &ffllig
				new Transition (2551, 2552), // &fili -> &filig
				new Transition (2589, 2590), // &fjli -> &fjlig
				new Transition (2597, 2598), // &flli -> &fllig
				new Transition (2701, 2807), // &g -> &gg
				new Transition (2708, 2805), // &G -> &Gg
				new Transition (2807, 2809), // &gg -> &ggg
				new Transition (3149, 3150), // &hookri -> &hookrig
				new Transition (3236, 3289), // &I -> &Ig
				new Transition (3243, 3295), // &i -> &ig
				new Transition (3322, 3323), // &IJli -> &IJlig
				new Transition (3327, 3328), // &ijli -> &ijlig
				new Transition (3332, 3344), // &Ima -> &Imag
				new Transition (3337, 3341), // &ima -> &imag
				new Transition (3407, 3408), // &inte -> &integ
				new Transition (3413, 3414), // &Inte -> &Integ
				new Transition (3467, 3476), // &io -> &iog
				new Transition (3471, 3472), // &Io -> &Iog
				new Transition (3624, 3654), // &k -> &kg
				new Transition (3692, 4317), // &l -> &lg
				new Transition (3705, 3718), // &la -> &lag
				new Transition (3733, 3734), // &Lan -> &Lang
				new Transition (3736, 3737), // &lan -> &lang
				new Transition (3894, 4183), // &lE -> &lEg
				new Transition (3896, 4185), // &le -> &leg
				new Transition (3902, 3903), // &LeftAn -> &LeftAng
				new Transition (3938, 3939), // &LeftArrowRi -> &LeftArrowRig
				new Transition (3958, 3959), // &LeftCeilin -> &LeftCeiling
				new Transition (4031, 4032), // &LeftRi -> &LeftRig
				new Transition (4042, 4043), // &Leftri -> &Leftrig
				new Transition (4053, 4054), // &leftri -> &leftrig
				new Transition (4077, 4078), // &leftrightsqui -> &leftrightsquig
				new Transition (4123, 4124), // &LeftTrian -> &LeftTriang
				new Transition (4197, 4210), // &les -> &lesg
				new Transition (4215, 4271), // &less -> &lessg
				new Transition (4228, 4229), // &lesseq -> &lesseqg
				new Transition (4233, 4234), // &lesseqq -> &lesseqqg
				new Transition (4424, 4425), // &loan -> &loang
				new Transition (4435, 4436), // &Lon -> &Long
				new Transition (4457, 4458), // &lon -> &long
				new Transition (4470, 4471), // &LongLeftRi -> &LongLeftRig
				new Transition (4481, 4482), // &Longleftri -> &Longleftrig
				new Transition (4492, 4493), // &longleftri -> &longleftrig
				new Transition (4510, 4511), // &LongRi -> &LongRig
				new Transition (4521, 4522), // &Longri -> &Longrig
				new Transition (4532, 4533), // &longri -> &longrig
				new Transition (4555, 4556), // &looparrowri -> &looparrowrig
				new Transition (4602, 4603), // &LowerRi -> &LowerRig
				new Transition (4615, 4616), // &lozen -> &lozeng
				new Transition (4670, 4674), // &lsim -> &lsimg
				new Transition (4838, 4839), // &measuredan -> &measuredang
				new Transition (4965, 5195), // &n -> &ng
				new Transition (4983, 4984), // &nan -> &nang
				new Transition (5045, 5046), // &ncon -> &ncong
				new Transition (5084, 5085), // &Ne -> &Neg
				new Transition (5212, 5213), // &nG -> &nGg
				new Transition (5291, 5292), // &nLeftri -> &nLeftrig
				new Transition (5302, 5303), // &nleftri -> &nleftrig
				new Transition (5361, 5362), // &NonBreakin -> &NonBreaking
				new Transition (5382, 5383), // &NotCon -> &NotCong
				new Transition (5536, 5537), // &NotLeftTrian -> &NotLeftTriang
				new Transition (5671, 5672), // &NotRi -> &NotRig
				new Transition (5679, 5680), // &NotRightTrian -> &NotRightTriang
				new Transition (5869, 5870), // &nRi -> &nRig
				new Transition (5879, 5880), // &nri -> &nrig
				new Transition (5988, 5989), // &nt -> &ntg
				new Transition (6003, 6004), // &ntl -> &ntlg
				new Transition (6009, 6010), // &ntrian -> &ntriang
				new Transition (6022, 6023), // &ntriangleri -> &ntrianglerig
				new Transition (6043, 6068), // &nv -> &nvg
				new Transition (6131, 6214), // &O -> &Og
				new Transition (6138, 6210), // &o -> &og
				new Transition (6192, 6193), // &OEli -> &OElig
				new Transition (6197, 6198), // &oeli -> &oelig
				new Transition (6268, 6269), // &Ome -> &Omeg
				new Transition (6272, 6273), // &ome -> &omeg
				new Transition (6360, 6361), // &ori -> &orig
				new Transition (6908, 6909), // &Ran -> &Rang
				new Transition (6911, 6912), // &ran -> &rang
				new Transition (7074, 7095), // &re -> &reg
				new Transition (7171, 7172), // &Ri -> &Rig
				new Transition (7176, 7177), // &RightAn -> &RightAng
				new Transition (7199, 7200), // &ri -> &rig
				new Transition (7233, 7234), // &RightCeilin -> &RightCeiling
				new Transition (7315, 7316), // &rightri -> &rightrig
				new Transition (7329, 7330), // &rightsqui -> &rightsquig
				new Transition (7368, 7369), // &RightTrian -> &RightTriang
				new Transition (7428, 7429), // &rin -> &ring
				new Transition (7433, 7434), // &risin -> &rising
				new Transition (7471, 7472), // &roan -> &roang
				new Transition (7514, 7516), // &rpar -> &rparg
				new Transition (7532, 7533), // &Rri -> &Rrig
				new Transition (7813, 7814), // &ShortRi -> &ShortRig
				new Transition (7833, 7834), // &Si -> &Sig
				new Transition (7838, 7839), // &si -> &sig
				new Transition (7847, 7857), // &sim -> &simg
				new Transition (8108, 8109), // &strai -> &straig
				new Transition (8279, 8280), // &sun -> &sung
				new Transition (8397, 8398), // &szli -> &szlig
				new Transition (8406, 8407), // &tar -> &targ
				new Transition (8635, 8636), // &trian -> &triang
				new Transition (8656, 8657), // &triangleri -> &trianglerig
				new Transition (8758, 8759), // &twoheadri -> &twoheadrig
				new Transition (8768, 8860), // &U -> &Ug
				new Transition (8775, 8866), // &u -> &ug
				new Transition (8954, 8955), // &Uo -> &Uog
				new Transition (8959, 8960), // &uo -> &uog
				new Transition (9059, 9060), // &upharpoonri -> &upharpoonrig
				new Transition (9082, 9083), // &UpperRi -> &UpperRig
				new Transition (9142, 9143), // &Urin -> &Uring
				new Transition (9146, 9147), // &urin -> &uring
				new Transition (9196, 9197), // &uwan -> &uwang
				new Transition (9203, 9204), // &van -> &vang
				new Transition (9228, 9229), // &varnothin -> &varnothing
				new Transition (9253, 9254), // &varsi -> &varsig
				new Transition (9288, 9289), // &vartrian -> &vartriang
				new Transition (9298, 9299), // &vartriangleri -> &vartrianglerig
				new Transition (9478, 9479), // &vzi -> &vzig
				new Transition (9481, 9482), // &vzigza -> &vzigzag
				new Transition (9497, 9507), // &wed -> &wedg
				new Transition (9503, 9504), // &Wed -> &Wedg
				new Transition (9661, 9662), // &xwed -> &xwedg
				new Transition (9825, 9826) // &zi -> &zig
			};
			TransitionTable_h = new Transition[159] {
				new Transition (0, 3020), // & -> &h
				new Transition (86, 87), // &alep -> &aleph
				new Transition (90, 91), // &Alp -> &Alph
				new Transition (94, 95), // &alp -> &alph
				new Transition (147, 162), // &angmsda -> &angmsdah
				new Transition (173, 174), // &angsp -> &angsph
				new Transition (338, 339), // &Backslas -> &Backslash
				new Transition (426, 429), // &bet -> &beth
				new Transition (559, 560), // &blacktrianglerig -> &blacktrianglerigh
				new Transition (613, 638), // &box -> &boxh
				new Transition (691, 697), // &boxV -> &boxVh
				new Transition (693, 701), // &boxv -> &boxvh
				new Transition (758, 762), // &bsol -> &bsolh
				new Transition (789, 973), // &C -> &Ch
				new Transition (796, 960), // &c -> &ch
				new Transition (1000, 1001), // &circlearrowrig -> &circlearrowrigh
				new Transition (1016, 1017), // &circleddas -> &circleddash
				new Transition (1395, 1396), // &curvearrowrig -> &curvearrowrigh
				new Transition (1432, 1550), // &d -> &dh
				new Transition (1441, 1442), // &dalet -> &daleth
				new Transition (1454, 1455), // &das -> &dash
				new Transition (1457, 1458), // &Das -> &Dash
				new Transition (1506, 1507), // &DDotra -> &DDotrah
				new Transition (1537, 1538), // &dfis -> &dfish
				new Transition (1788, 1789), // &DoubleLeftRig -> &DoubleLeftRigh
				new Transition (1816, 1817), // &DoubleLongLeftRig -> &DoubleLongLeftRigh
				new Transition (1827, 1828), // &DoubleLongRig -> &DoubleLongRigh
				new Transition (1838, 1839), // &DoubleRig -> &DoubleRigh
				new Transition (1896, 1932), // &down -> &downh
				new Transition (1946, 1947), // &downharpoonrig -> &downharpoonrigh
				new Transition (1956, 1957), // &DownLeftRig -> &DownLeftRigh
				new Transition (1989, 1990), // &DownRig -> &DownRigh
				new Transition (2077, 2082), // &du -> &duh
				new Transition (2439, 2445), // &et -> &eth
				new Transition (3132, 3133), // &homt -> &homth
				new Transition (3150, 3151), // &hookrig -> &hookrigh
				new Transition (3194, 3195), // &hslas -> &hslash
				new Transition (3231, 3232), // &hyp -> &hyph
				new Transition (3362, 3363), // &imat -> &imath
				new Transition (3435, 3436), // &intlar -> &intlarh
				new Transition (3579, 3580), // &jmat -> &jmath
				new Transition (3624, 3664), // &k -> &kh
				new Transition (3692, 4325), // &l -> &lh
				new Transition (3766, 3776), // &larr -> &larrh
				new Transition (3880, 3881), // &ldrd -> &ldrdh
				new Transition (3886, 3887), // &ldrus -> &ldrush
				new Transition (3891, 3892), // &lds -> &ldsh
				new Transition (3926, 4004), // &left -> &lefth
				new Transition (3939, 3940), // &LeftArrowRig -> &LeftArrowRigh
				new Transition (4032, 4033), // &LeftRig -> &LeftRigh
				new Transition (4043, 4044), // &Leftrig -> &Leftrigh
				new Transition (4054, 4055), // &leftrig -> &leftrigh
				new Transition (4056, 4065), // &leftright -> &leftrighth
				new Transition (4109, 4110), // &leftt -> &leftth
				new Transition (4303, 4304), // &lfis -> &lfish
				new Transition (4348, 4370), // &ll -> &llh
				new Transition (4397, 4398), // &lmoustac -> &lmoustach
				new Transition (4471, 4472), // &LongLeftRig -> &LongLeftRigh
				new Transition (4482, 4483), // &Longleftrig -> &Longleftrigh
				new Transition (4493, 4494), // &longleftrig -> &longleftrigh
				new Transition (4511, 4512), // &LongRig -> &LongRigh
				new Transition (4522, 4523), // &Longrig -> &Longrigh
				new Transition (4533, 4534), // &longrig -> &longrigh
				new Transition (4556, 4557), // &looparrowrig -> &looparrowrigh
				new Transition (4603, 4604), // &LowerRig -> &LowerRigh
				new Transition (4628, 4640), // &lr -> &lrh
				new Transition (4652, 4667), // &ls -> &lsh
				new Transition (4658, 4665), // &Ls -> &Lsh
				new Transition (4698, 4710), // &lt -> &lth
				new Transition (4745, 4746), // &lurds -> &lurdsh
				new Transition (4750, 4751), // &luru -> &luruh
				new Transition (4767, 4868), // &m -> &mh
				new Transition (4822, 4823), // &mdas -> &mdash
				new Transition (4965, 5227), // &n -> &nh
				new Transition (5061, 5062), // &ndas -> &ndash
				new Transition (5067, 5068), // &near -> &nearh
				new Transition (5103, 5104), // &NegativeT -> &NegativeTh
				new Transition (5125, 5126), // &NegativeVeryT -> &NegativeVeryTh
				new Transition (5292, 5293), // &nLeftrig -> &nLeftrigh
				new Transition (5303, 5304), // &nleftrig -> &nleftrigh
				new Transition (5672, 5673), // &NotRig -> &NotRigh
				new Transition (5870, 5871), // &nRig -> &nRigh
				new Transition (5880, 5881), // &nrig -> &nrigh
				new Transition (5895, 5910), // &ns -> &nsh
				new Transition (6023, 6024), // &ntrianglerig -> &ntrianglerigh
				new Transition (6050, 6051), // &nVDas -> &nVDash
				new Transition (6055, 6056), // &nVdas -> &nVdash
				new Transition (6060, 6061), // &nvDas -> &nvDash
				new Transition (6065, 6066), // &nvdas -> &nvdash
				new Transition (6113, 6114), // &nwar -> &nwarh
				new Transition (6138, 6227), // &o -> &oh
				new Transition (6165, 6166), // &odas -> &odash
				new Transition (6388, 6389), // &Oslas -> &Oslash
				new Transition (6393, 6394), // &oslas -> &oslash
				new Transition (6456, 6457), // &OverParent -> &OverParenth
				new Transition (6463, 6527), // &p -> &ph
				new Transition (6482, 6524), // &P -> &Ph
				new Transition (6546, 6547), // &pitc -> &pitch
				new Transition (6559, 6561), // &planck -> &planckh
				new Transition (6876, 7155), // &r -> &rh
				new Transition (6886, 7164), // &R -> &Rh
				new Transition (6932, 6947), // &rarr -> &rarrh
				new Transition (7058, 7059), // &rdld -> &rdldh
				new Transition (7069, 7070), // &rds -> &rdsh
				new Transition (7137, 7138), // &rfis -> &rfish
				new Transition (7172, 7173), // &Rig -> &Righ
				new Transition (7200, 7201), // &rig -> &righ
				new Transition (7202, 7279), // &right -> &righth
				new Transition (7297, 7305), // &rightleft -> &rightlefth
				new Transition (7316, 7317), // &rightrig -> &rightrigh
				new Transition (7354, 7355), // &rightt -> &rightth
				new Transition (7442, 7447), // &rl -> &rlh
				new Transition (7460, 7461), // &rmoustac -> &rmoustach
				new Transition (7533, 7534), // &Rrig -> &Rrigh
				new Transition (7542, 7557), // &rs -> &rsh
				new Transition (7548, 7555), // &Rs -> &Rsh
				new Transition (7567, 7568), // &rt -> &rth
				new Transition (7603, 7604), // &rulu -> &ruluh
				new Transition (7610, 7772), // &S -> &Sh
				new Transition (7617, 7751), // &s -> &sh
				new Transition (7705, 7706), // &sear -> &searh
				new Transition (7762, 7763), // &shc -> &shch
				new Transition (7814, 7815), // &ShortRig -> &ShortRigh
				new Transition (7907, 7908), // &smas -> &smash
				new Transition (8109, 8110), // &straig -> &straigh
				new Transition (8120, 8121), // &straightp -> &straightph
				new Transition (8216, 8269), // &Suc -> &Such
				new Transition (8270, 8271), // &SuchT -> &SuchTh
				new Transition (8284, 8320), // &sup -> &suph
				new Transition (8377, 8378), // &swar -> &swarh
				new Transition (8400, 8467), // &T -> &Th
				new Transition (8404, 8461), // &t -> &th
				new Transition (8657, 8658), // &trianglerig -> &trianglerigh
				new Transition (8709, 8723), // &ts -> &tsh
				new Transition (8742, 8743), // &two -> &twoh
				new Transition (8759, 8760), // &twoheadrig -> &twoheadrigh
				new Transition (8775, 8876), // &u -> &uh
				new Transition (8829, 8845), // &ud -> &udh
				new Transition (8851, 8852), // &ufis -> &ufish
				new Transition (8938, 8939), // &UnderParent -> &UnderParenth
				new Transition (8983, 9046), // &up -> &uph
				new Transition (9060, 9061), // &upharpoonrig -> &upharpoonrigh
				new Transition (9083, 9084), // &UpperRig -> &UpperRigh
				new Transition (9096, 9098), // &upsi -> &upsih
				new Transition (9225, 9226), // &varnot -> &varnoth
				new Transition (9231, 9232), // &varp -> &varph
				new Transition (9247, 9249), // &varr -> &varrh
				new Transition (9279, 9280), // &vart -> &varth
				new Transition (9299, 9300), // &vartrianglerig -> &vartrianglerigh
				new Transition (9322, 9323), // &VDas -> &VDash
				new Transition (9327, 9328), // &Vdas -> &Vdash
				new Transition (9332, 9333), // &vDas -> &vDash
				new Transition (9337, 9338), // &vdas -> &vdash
				new Transition (9404, 9405), // &VeryT -> &VeryTh
				new Transition (9474, 9475), // &Vvdas -> &Vvdash
				new Transition (9537, 9538), // &wreat -> &wreath
				new Transition (9548, 9572), // &x -> &xh
				new Transition (9754, 9821), // &z -> &zh
				new Transition (9797, 9798) // &ZeroWidt -> &ZeroWidth
			};
			TransitionTable_i = new Transition[428] {
				new Transition (0, 3243), // & -> &i
				new Transition (27, 38), // &ac -> &aci
				new Transition (33, 34), // &Ac -> &Aci
				new Transition (51, 52), // &AEl -> &AEli
				new Transition (56, 57), // &ael -> &aeli
				new Transition (199, 210), // &ap -> &api
				new Transition (202, 203), // &apac -> &apaci
				new Transition (224, 225), // &ApplyFunct -> &ApplyFuncti
				new Transition (237, 238), // &Ar -> &Ari
				new Transition (242, 243), // &ar -> &ari
				new Transition (255, 256), // &Ass -> &Assi
				new Transition (269, 270), // &At -> &Ati
				new Transition (275, 276), // &at -> &ati
				new Transition (289, 297), // &aw -> &awi
				new Transition (292, 293), // &awcon -> &awconi
				new Transition (301, 442), // &b -> &bi
				new Transition (312, 313), // &backeps -> &backepsi
				new Transition (319, 320), // &backpr -> &backpri
				new Transition (324, 325), // &backs -> &backsi
				new Transition (406, 407), // &beps -> &bepsi
				new Transition (419, 420), // &Bernoull -> &Bernoulli
				new Transition (444, 448), // &bigc -> &bigci
				new Transition (465, 466), // &bigot -> &bigoti
				new Transition (482, 483), // &bigtr -> &bigtri
				new Transition (539, 540), // &blacktr -> &blacktri
				new Transition (557, 558), // &blacktriangler -> &blacktriangleri
				new Transition (583, 584), // &bnequ -> &bnequi
				new Transition (609, 610), // &bowt -> &bowti
				new Transition (656, 657), // &boxm -> &boxmi
				new Transition (667, 668), // &boxt -> &boxti
				new Transition (720, 721), // &bpr -> &bpri
				new Transition (744, 752), // &bs -> &bsi
				new Transition (749, 750), // &bsem -> &bsemi
				new Transition (789, 1019), // &C -> &Ci
				new Transition (796, 978), // &c -> &ci
				new Transition (803, 828), // &Cap -> &Capi
				new Transition (832, 833), // &CapitalD -> &CapitalDi
				new Transition (840, 841), // &CapitalDifferent -> &CapitalDifferenti
				new Transition (861, 890), // &cc -> &cci
				new Transition (866, 886), // &Cc -> &Cci
				new Transition (877, 878), // &Cced -> &Ccedi
				new Transition (882, 883), // &cced -> &ccedi
				new Transition (895, 896), // &Ccon -> &Cconi
				new Transition (916, 917), // &ced -> &cedi
				new Transition (921, 922), // &Ced -> &Cedi
				new Transition (960, 976), // &ch -> &chi
				new Transition (973, 974), // &Ch -> &Chi
				new Transition (998, 999), // &circlearrowr -> &circlearrowri
				new Transition (1009, 1010), // &circledc -> &circledci
				new Transition (1032, 1033), // &CircleM -> &CircleMi
				new Transition (1043, 1044), // &CircleT -> &CircleTi
				new Transition (1054, 1055), // &cirfn -> &cirfni
				new Transition (1059, 1060), // &cirm -> &cirmi
				new Transition (1064, 1065), // &cirsc -> &cirsci
				new Transition (1072, 1073), // &Clockw -> &Clockwi
				new Transition (1122, 1123), // &clubsu -> &clubsui
				new Transition (1164, 1183), // &con -> &coni
				new Transition (1171, 1179), // &Con -> &Coni
				new Transition (1236, 1237), // &CounterClockw -> &CounterClockwi
				new Transition (1393, 1394), // &curvearrowr -> &curvearrowri
				new Transition (1407, 1415), // &cw -> &cwi
				new Transition (1410, 1411), // &cwcon -> &cwconi
				new Transition (1425, 1557), // &D -> &Di
				new Transition (1432, 1599), // &d -> &di
				new Transition (1535, 1536), // &df -> &dfi
				new Transition (1560, 1561), // &Diacr -> &Diacri
				new Transition (1562, 1563), // &Diacrit -> &Diacriti
				new Transition (1593, 1594), // &DiacriticalT -> &DiacriticalTi
				new Transition (1613, 1614), // &diamondsu -> &diamondsui
				new Transition (1627, 1628), // &Different -> &Differenti
				new Transition (1639, 1640), // &dis -> &disi
				new Transition (1643, 1645), // &div -> &divi
				new Transition (1651, 1652), // &divideont -> &divideonti
				new Transition (1713, 1714), // &dotm -> &dotmi
				new Transition (1786, 1787), // &DoubleLeftR -> &DoubleLeftRi
				new Transition (1814, 1815), // &DoubleLongLeftR -> &DoubleLongLeftRi
				new Transition (1825, 1826), // &DoubleLongR -> &DoubleLongRi
				new Transition (1836, 1837), // &DoubleR -> &DoubleRi
				new Transition (1872, 1873), // &DoubleVert -> &DoubleVerti
				new Transition (1944, 1945), // &downharpoonr -> &downharpoonri
				new Transition (1954, 1955), // &DownLeftR -> &DownLeftRi
				new Transition (1987, 1988), // &DownR -> &DownRi
				new Transition (2072, 2073), // &dtr -> &dtri
				new Transition (2097, 2101), // &dz -> &dzi
				new Transition (2127, 2142), // &Ec -> &Eci
				new Transition (2133, 2139), // &ec -> &eci
				new Transition (2204, 2213), // &el -> &eli
				new Transition (2323, 2324), // &eps -> &epsi
				new Transition (2327, 2328), // &Eps -> &Epsi
				new Transition (2340, 2341), // &eqc -> &eqci
				new Transition (2350, 2351), // &eqs -> &eqsi
				new Transition (2368, 2387), // &Equ -> &Equi
				new Transition (2372, 2396), // &equ -> &equi
				new Transition (2377, 2378), // &EqualT -> &EqualTi
				new Transition (2388, 2389), // &Equil -> &Equili
				new Transition (2391, 2392), // &Equilibr -> &Equilibri
				new Transition (2418, 2430), // &Es -> &Esi
				new Transition (2422, 2433), // &es -> &esi
				new Transition (2458, 2462), // &ex -> &exi
				new Transition (2466, 2467), // &Ex -> &Exi
				new Transition (2477, 2478), // &expectat -> &expectati
				new Transition (2487, 2488), // &Exponent -> &Exponenti
				new Transition (2497, 2498), // &exponent -> &exponenti
				new Transition (2503, 2549), // &f -> &fi
				new Transition (2506, 2507), // &fall -> &falli
				new Transition (2517, 2554), // &F -> &Fi
				new Transition (2530, 2531), // &ff -> &ffi
				new Transition (2532, 2533), // &ffil -> &ffili
				new Transition (2536, 2537), // &ffl -> &ffli
				new Transition (2540, 2541), // &ffll -> &fflli
				new Transition (2550, 2551), // &fil -> &fili
				new Transition (2588, 2589), // &fjl -> &fjli
				new Transition (2596, 2597), // &fll -> &flli
				new Transition (2631, 2632), // &Four -> &Fouri
				new Transition (2642, 2643), // &fpart -> &fparti
				new Transition (2701, 2811), // &g -> &gi
				new Transition (2736, 2742), // &Gc -> &Gci
				new Transition (2738, 2739), // &Gced -> &Gcedi
				new Transition (2746, 2747), // &gc -> &gci
				new Transition (2849, 2850), // &gns -> &gnsi
				new Transition (2917, 2918), // &GreaterT -> &GreaterTi
				new Transition (2927, 2931), // &gs -> &gsi
				new Transition (2944, 2947), // &gtc -> &gtci
				new Transition (2998, 2999), // &gtrs -> &gtrsi
				new Transition (3014, 3100), // &H -> &Hi
				new Transition (3021, 3022), // &ha -> &hai
				new Transition (3030, 3031), // &ham -> &hami
				new Transition (3052, 3053), // &harrc -> &harrci
				new Transition (3064, 3065), // &Hc -> &Hci
				new Transition (3069, 3070), // &hc -> &hci
				new Transition (3080, 3081), // &heartsu -> &heartsui
				new Transition (3085, 3086), // &hell -> &helli
				new Transition (3148, 3149), // &hookr -> &hookri
				new Transition (3171, 3172), // &Hor -> &Hori
				new Transition (3179, 3180), // &HorizontalL -> &HorizontalLi
				new Transition (3243, 3301), // &i -> &ii
				new Transition (3250, 3257), // &ic -> &ici
				new Transition (3252, 3253), // &Ic -> &Ici
				new Transition (3301, 3303), // &ii -> &iii
				new Transition (3303, 3304), // &iii -> &iiii
				new Transition (3312, 3313), // &iinf -> &iinfi
				new Transition (3321, 3322), // &IJl -> &IJli
				new Transition (3326, 3327), // &ijl -> &ijli
				new Transition (3344, 3345), // &Imag -> &Imagi
				new Transition (3352, 3353), // &imagl -> &imagli
				new Transition (3373, 3374), // &Impl -> &Impli
				new Transition (3385, 3386), // &inf -> &infi
				new Transition (3389, 3390), // &infint -> &infinti
				new Transition (3428, 3429), // &Intersect -> &Intersecti
				new Transition (3444, 3445), // &Inv -> &Invi
				new Transition (3446, 3447), // &Invis -> &Invisi
				new Transition (3457, 3458), // &InvisibleT -> &InvisibleTi
				new Transition (3507, 3511), // &is -> &isi
				new Transition (3526, 3534), // &it -> &iti
				new Transition (3528, 3529), // &It -> &Iti
				new Transition (3556, 3557), // &Jc -> &Jci
				new Transition (3562, 3563), // &jc -> &jci
				new Transition (3634, 3635), // &Kced -> &Kcedi
				new Transition (3640, 3641), // &kced -> &kcedi
				new Transition (3785, 3786), // &larrs -> &larrsi
				new Transition (3795, 3796), // &lAta -> &lAtai
				new Transition (3799, 3800), // &lata -> &latai
				new Transition (3850, 3851), // &Lced -> &Lcedi
				new Transition (3854, 3859), // &lce -> &lcei
				new Transition (3855, 3856), // &lced -> &lcedi
				new Transition (3937, 3938), // &LeftArrowR -> &LeftArrowRi
				new Transition (3949, 3950), // &leftarrowta -> &leftarrowtai
				new Transition (3954, 3955), // &LeftCe -> &LeftCei
				new Transition (3956, 3957), // &LeftCeil -> &LeftCeili
				new Transition (4030, 4031), // &LeftR -> &LeftRi
				new Transition (4041, 4042), // &Leftr -> &Leftri
				new Transition (4052, 4053), // &leftr -> &leftri
				new Transition (4076, 4077), // &leftrightsqu -> &leftrightsqui
				new Transition (4114, 4115), // &leftthreet -> &leftthreeti
				new Transition (4120, 4121), // &LeftTr -> &LeftTri
				new Transition (4280, 4281), // &lesss -> &lesssi
				new Transition (4295, 4296), // &LessT -> &LessTi
				new Transition (4301, 4302), // &lf -> &lfi
				new Transition (4376, 4377), // &lltr -> &lltri
				new Transition (4379, 4380), // &Lm -> &Lmi
				new Transition (4385, 4386), // &lm -> &lmi
				new Transition (4418, 4419), // &lns -> &lnsi
				new Transition (4469, 4470), // &LongLeftR -> &LongLeftRi
				new Transition (4480, 4481), // &Longleftr -> &Longleftri
				new Transition (4491, 4492), // &longleftr -> &longleftri
				new Transition (4509, 4510), // &LongR -> &LongRi
				new Transition (4520, 4521), // &Longr -> &Longri
				new Transition (4531, 4532), // &longr -> &longri
				new Transition (4554, 4555), // &looparrowr -> &looparrowri
				new Transition (4573, 4574), // &lot -> &loti
				new Transition (4601, 4602), // &LowerR -> &LowerRi
				new Transition (4649, 4650), // &lrtr -> &lrtri
				new Transition (4652, 4669), // &ls -> &lsi
				new Transition (4698, 4715), // &lt -> &lti
				new Transition (4700, 4703), // &ltc -> &ltci
				new Transition (4731, 4732), // &ltr -> &ltri
				new Transition (4767, 4871), // &m -> &mi
				new Transition (4781, 4900), // &M -> &Mi
				new Transition (4844, 4845), // &Med -> &Medi
				new Transition (4855, 4856), // &Mell -> &Melli
				new Transition (4882, 4883), // &midc -> &midci
				new Transition (4955, 4956), // &mult -> &multi
				new Transition (4965, 5240), // &n -> &ni
				new Transition (4986, 4990), // &nap -> &napi
				new Transition (5035, 5036), // &Nced -> &Ncedi
				new Transition (5040, 5041), // &nced -> &ncedi
				new Transition (5087, 5088), // &Negat -> &Negati
				new Transition (5093, 5094), // &NegativeMed -> &NegativeMedi
				new Transition (5104, 5105), // &NegativeTh -> &NegativeThi
				new Transition (5126, 5127), // &NegativeVeryTh -> &NegativeVeryThi
				new Transition (5136, 5137), // &nequ -> &nequi
				new Transition (5140, 5145), // &nes -> &nesi
				new Transition (5177, 5178), // &NewL -> &NewLi
				new Transition (5182, 5183), // &nex -> &nexi
				new Transition (5215, 5216), // &ngs -> &ngsi
				new Transition (5290, 5291), // &nLeftr -> &nLeftri
				new Transition (5301, 5302), // &nleftr -> &nleftri
				new Transition (5328, 5329), // &nls -> &nlsi
				new Transition (5336, 5337), // &nltr -> &nltri
				new Transition (5343, 5344), // &nm -> &nmi
				new Transition (5359, 5360), // &NonBreak -> &NonBreaki
				new Transition (5378, 5512), // &not -> &noti
				new Transition (5405, 5406), // &NotDoubleVert -> &NotDoubleVerti
				new Transition (5427, 5428), // &NotEqualT -> &NotEqualTi
				new Transition (5433, 5434), // &NotEx -> &NotExi
				new Transition (5487, 5488), // &NotGreaterT -> &NotGreaterTi
				new Transition (5533, 5534), // &NotLeftTr -> &NotLeftTri
				new Transition (5584, 5585), // &NotLessT -> &NotLessTi
				new Transition (5620, 5621), // &notn -> &notni
				new Transition (5656, 5671), // &NotR -> &NotRi
				new Transition (5676, 5677), // &NotRightTr -> &NotRightTri
				new Transition (5762, 5763), // &NotSucceedsT -> &NotSucceedsTi
				new Transition (5781, 5782), // &NotT -> &NotTi
				new Transition (5803, 5804), // &NotTildeT -> &NotTildeTi
				new Transition (5812, 5813), // &NotVert -> &NotVerti
				new Transition (5837, 5838), // &npol -> &npoli
				new Transition (5855, 5879), // &nr -> &nri
				new Transition (5868, 5869), // &nR -> &nRi
				new Transition (5890, 5891), // &nrtr -> &nrtri
				new Transition (5895, 5927), // &ns -> &nsi
				new Transition (5914, 5915), // &nshortm -> &nshortmi
				new Transition (5934, 5935), // &nsm -> &nsmi
				new Transition (5988, 5998), // &nt -> &nti
				new Transition (5992, 5993), // &Nt -> &Nti
				new Transition (6006, 6007), // &ntr -> &ntri
				new Transition (6021, 6022), // &ntriangler -> &ntriangleri
				new Transition (6043, 6078), // &nv -> &nvi
				new Transition (6080, 6081), // &nvinf -> &nvinfi
				new Transition (6093, 6094), // &nvltr -> &nvltri
				new Transition (6103, 6104), // &nvrtr -> &nvrtri
				new Transition (6107, 6108), // &nvs -> &nvsi
				new Transition (6138, 6234), // &o -> &oi
				new Transition (6148, 6149), // &oc -> &oci
				new Transition (6152, 6153), // &Oc -> &Oci
				new Transition (6163, 6179), // &od -> &odi
				new Transition (6191, 6192), // &OEl -> &OEli
				new Transition (6196, 6197), // &oel -> &oeli
				new Transition (6201, 6202), // &ofc -> &ofci
				new Transition (6238, 6252), // &ol -> &oli
				new Transition (6243, 6244), // &olc -> &olci
				new Transition (6258, 6276), // &Om -> &Omi
				new Transition (6263, 6282), // &om -> &omi
				new Transition (6342, 6360), // &or -> &ori
				new Transition (6399, 6400), // &Ot -> &Oti
				new Transition (6405, 6406), // &ot -> &oti
				new Transition (6459, 6460), // &OverParenthes -> &OverParenthesi
				new Transition (6463, 6543), // &p -> &pi
				new Transition (6474, 6475), // &pars -> &parsi
				new Transition (6482, 6541), // &P -> &Pi
				new Transition (6485, 6486), // &Part -> &Parti
				new Transition (6498, 6503), // &per -> &peri
				new Transition (6507, 6508), // &perm -> &permi
				new Transition (6524, 6525), // &Ph -> &Phi
				new Transition (6527, 6528), // &ph -> &phi
				new Transition (6570, 6571), // &plusac -> &plusaci
				new Transition (6576, 6577), // &plusc -> &plusci
				new Transition (6590, 6591), // &PlusM -> &PlusMi
				new Transition (6599, 6600), // &pluss -> &plussi
				new Transition (6609, 6610), // &Po -> &Poi
				new Transition (6622, 6623), // &po -> &poi
				new Transition (6625, 6626), // &point -> &pointi
				new Transition (6640, 6725), // &Pr -> &Pri
				new Transition (6642, 6729), // &pr -> &pri
				new Transition (6696, 6697), // &PrecedesT -> &PrecedesTi
				new Transition (6717, 6718), // &precns -> &precnsi
				new Transition (6721, 6722), // &precs -> &precsi
				new Transition (6741, 6742), // &prns -> &prnsi
				new Transition (6760, 6761), // &profl -> &profli
				new Transition (6775, 6776), // &Proport -> &Proporti
				new Transition (6786, 6787), // &prs -> &prsi
				new Transition (6795, 6803), // &Ps -> &Psi
				new Transition (6799, 6805), // &ps -> &psi
				new Transition (6817, 6821), // &q -> &qi
				new Transition (6834, 6835), // &qpr -> &qpri
				new Transition (6849, 6858), // &quat -> &quati
				new Transition (6852, 6853), // &quatern -> &quaterni
				new Transition (6876, 7199), // &r -> &ri
				new Transition (6886, 7171), // &R -> &Ri
				new Transition (6897, 6898), // &rad -> &radi
				new Transition (6956, 6957), // &rarrs -> &rarrsi
				new Transition (6969, 6970), // &rAta -> &rAtai
				new Transition (6973, 6978), // &rat -> &rati
				new Transition (6974, 6975), // &rata -> &ratai
				new Transition (7034, 7035), // &Rced -> &Rcedi
				new Transition (7038, 7043), // &rce -> &rcei
				new Transition (7039, 7040), // &rced -> &rcedi
				new Transition (7076, 7078), // &real -> &reali
				new Transition (7111, 7112), // &ReverseEqu -> &ReverseEqui
				new Transition (7113, 7114), // &ReverseEquil -> &ReverseEquili
				new Transition (7116, 7117), // &ReverseEquilibr -> &ReverseEquilibri
				new Transition (7125, 7126), // &ReverseUpEqu -> &ReverseUpEqui
				new Transition (7127, 7128), // &ReverseUpEquil -> &ReverseUpEquili
				new Transition (7130, 7131), // &ReverseUpEquilibr -> &ReverseUpEquilibri
				new Transition (7135, 7136), // &rf -> &rfi
				new Transition (7224, 7225), // &rightarrowta -> &rightarrowtai
				new Transition (7229, 7230), // &RightCe -> &RightCei
				new Transition (7231, 7232), // &RightCeil -> &RightCeili
				new Transition (7314, 7315), // &rightr -> &rightri
				new Transition (7328, 7329), // &rightsqu -> &rightsqui
				new Transition (7359, 7360), // &rightthreet -> &rightthreeti
				new Transition (7365, 7366), // &RightTr -> &RightTri
				new Transition (7431, 7432), // &ris -> &risi
				new Transition (7465, 7466), // &rnm -> &rnmi
				new Transition (7495, 7496), // &rot -> &roti
				new Transition (7507, 7508), // &RoundImpl -> &RoundImpli
				new Transition (7521, 7522), // &rppol -> &rppoli
				new Transition (7531, 7532), // &Rr -> &Rri
				new Transition (7567, 7573), // &rt -> &rti
				new Transition (7578, 7579), // &rtr -> &rtri
				new Transition (7587, 7588), // &rtriltr -> &rtriltri
				new Transition (7610, 7833), // &S -> &Si
				new Transition (7617, 7838), // &s -> &si
				new Transition (7629, 7662), // &Sc -> &Sci
				new Transition (7631, 7666), // &sc -> &sci
				new Transition (7654, 7655), // &Sced -> &Scedi
				new Transition (7658, 7659), // &sced -> &scedi
				new Transition (7676, 7677), // &scns -> &scnsi
				new Transition (7682, 7683), // &scpol -> &scpoli
				new Transition (7687, 7688), // &scs -> &scsi
				new Transition (7721, 7722), // &sem -> &semi
				new Transition (7730, 7731), // &setm -> &setmi
				new Transition (7799, 7800), // &shortm -> &shortmi
				new Transition (7812, 7813), // &ShortR -> &ShortRi
				new Transition (7887, 7888), // &SmallC -> &SmallCi
				new Transition (7894, 7918), // &sm -> &smi
				new Transition (7901, 7902), // &smallsetm -> &smallsetmi
				new Transition (7962, 7963), // &spadesu -> &spadesui
				new Transition (8027, 8028), // &SquareIntersect -> &SquareIntersecti
				new Transition (8059, 8060), // &SquareUn -> &SquareUni
				new Transition (8086, 8087), // &ssm -> &ssmi
				new Transition (8107, 8108), // &stra -> &strai
				new Transition (8114, 8115), // &straighteps -> &straightepsi
				new Transition (8121, 8122), // &straightph -> &straightphi
				new Transition (8169, 8190), // &subs -> &subsi
				new Transition (8240, 8241), // &SucceedsT -> &SucceedsTi
				new Transition (8261, 8262), // &succns -> &succnsi
				new Transition (8265, 8266), // &succs -> &succsi
				new Transition (8352, 8367), // &sups -> &supsi
				new Transition (8396, 8397), // &szl -> &szli
				new Transition (8400, 8544), // &T -> &Ti
				new Transition (8404, 8549), // &t -> &ti
				new Transition (8432, 8433), // &Tced -> &Tcedi
				new Transition (8437, 8438), // &tced -> &tcedi
				new Transition (8461, 8493), // &th -> &thi
				new Transition (8467, 8507), // &Th -> &Thi
				new Transition (8503, 8504), // &thicks -> &thicksi
				new Transition (8531, 8532), // &thks -> &thksi
				new Transition (8570, 8571), // &TildeT -> &TildeTi
				new Transition (8600, 8601), // &topc -> &topci
				new Transition (8618, 8619), // &tpr -> &tpri
				new Transition (8628, 8633), // &tr -> &tri
				new Transition (8655, 8656), // &triangler -> &triangleri
				new Transition (8670, 8671), // &trim -> &trimi
				new Transition (8676, 8677), // &Tr -> &Tri
				new Transition (8693, 8694), // &trit -> &triti
				new Transition (8700, 8701), // &trpez -> &trpezi
				new Transition (8737, 8738), // &tw -> &twi
				new Transition (8757, 8758), // &twoheadr -> &twoheadri
				new Transition (8793, 8794), // &Uarroc -> &Uarroci
				new Transition (8815, 8816), // &Uc -> &Uci
				new Transition (8820, 8821), // &uc -> &uci
				new Transition (8849, 8850), // &uf -> &ufi
				new Transition (8901, 8902), // &ultr -> &ultri
				new Transition (8916, 8945), // &Un -> &Uni
				new Transition (8941, 8942), // &UnderParenthes -> &UnderParenthesi
				new Transition (9036, 9037), // &UpEqu -> &UpEqui
				new Transition (9038, 9039), // &UpEquil -> &UpEquili
				new Transition (9041, 9042), // &UpEquilibr -> &UpEquilibri
				new Transition (9058, 9059), // &upharpoonr -> &upharpoonri
				new Transition (9081, 9082), // &UpperR -> &UpperRi
				new Transition (9092, 9093), // &Ups -> &Upsi
				new Transition (9095, 9096), // &ups -> &upsi
				new Transition (9127, 9145), // &ur -> &uri
				new Transition (9140, 9141), // &Ur -> &Uri
				new Transition (9150, 9151), // &urtr -> &urtri
				new Transition (9161, 9172), // &ut -> &uti
				new Transition (9166, 9167), // &Ut -> &Uti
				new Transition (9177, 9178), // &utr -> &utri
				new Transition (9211, 9212), // &vareps -> &varepsi
				new Transition (9226, 9227), // &varnoth -> &varnothi
				new Transition (9231, 9235), // &varp -> &varpi
				new Transition (9232, 9233), // &varph -> &varphi
				new Transition (9252, 9253), // &vars -> &varsi
				new Transition (9285, 9286), // &vartr -> &vartri
				new Transition (9297, 9298), // &vartriangler -> &vartriangleri
				new Transition (9356, 9357), // &vell -> &velli
				new Transition (9370, 9374), // &Vert -> &Verti
				new Transition (9382, 9383), // &VerticalL -> &VerticalLi
				new Transition (9397, 9398), // &VerticalT -> &VerticalTi
				new Transition (9405, 9406), // &VeryTh -> &VeryThi
				new Transition (9422, 9423), // &vltr -> &vltri
				new Transition (9447, 9448), // &vrtr -> &vrtri
				new Transition (9477, 9478), // &vz -> &vzi
				new Transition (9485, 9486), // &Wc -> &Wci
				new Transition (9491, 9492), // &wc -> &wci
				new Transition (9496, 9512), // &we -> &wei
				new Transition (9548, 9583), // &x -> &xi
				new Transition (9549, 9553), // &xc -> &xci
				new Transition (9562, 9563), // &xdtr -> &xdtri
				new Transition (9565, 9581), // &X -> &Xi
				new Transition (9598, 9599), // &xn -> &xni
				new Transition (9618, 9619), // &xot -> &xoti
				new Transition (9652, 9653), // &xutr -> &xutri
				new Transition (9672, 9712), // &y -> &yi
				new Transition (9685, 9686), // &Yc -> &Yci
				new Transition (9690, 9691), // &yc -> &yci
				new Transition (9754, 9825), // &z -> &zi
				new Transition (9794, 9795) // &ZeroW -> &ZeroWi
			};
			TransitionTable_j = new Transition[11] {
				new Transition (0, 3561), // & -> &j
				new Transition (1432, 1665), // &d -> &dj
				new Transition (2503, 2587), // &f -> &fj
				new Transition (2701, 2820), // &g -> &gj
				new Transition (2824, 2830), // &gl -> &glj
				new Transition (3243, 3325), // &i -> &ij
				new Transition (3624, 3672), // &k -> &kj
				new Transition (3692, 4342), // &l -> &lj
				new Transition (4965, 5252), // &n -> &nj
				new Transition (9848, 9849), // &zw -> &zwj
				new Transition (9851, 9852) // &zwn -> &zwnj
			};
			TransitionTable_k = new Transition[69] {
				new Transition (0, 3624), // & -> &k
				new Transition (301, 513), // &b -> &bk
				new Transition (303, 304), // &bac -> &back
				new Transition (333, 334), // &Bac -> &Back
				new Transition (361, 362), // &bbr -> &bbrk
				new Transition (366, 367), // &bbrktbr -> &bbrktbrk
				new Transition (519, 566), // &bl -> &blk
				new Transition (521, 522), // &blac -> &black
				new Transition (563, 564), // &blan -> &blank
				new Transition (576, 577), // &bloc -> &block
				new Transition (965, 966), // &chec -> &check
				new Transition (970, 971), // &checkmar -> &checkmark
				new Transition (1070, 1071), // &Cloc -> &Clock
				new Transition (1234, 1235), // &CounterCloc -> &CounterClock
				new Transition (1463, 1464), // &db -> &dbk
				new Transition (2024, 2025), // &drb -> &drbk
				new Transition (2059, 2060), // &Dstro -> &Dstrok
				new Transition (2064, 2065), // &dstro -> &dstrok
				new Transition (2621, 2626), // &for -> &fork
				new Transition (3017, 3018), // &Hace -> &Hacek
				new Transition (3020, 3112), // &h -> &hk
				new Transition (3136, 3137), // &hoo -> &hook
				new Transition (3199, 3200), // &Hstro -> &Hstrok
				new Transition (3204, 3205), // &hstro -> &hstrok
				new Transition (3436, 3437), // &intlarh -> &intlarhk
				new Transition (3539, 3540), // &Iu -> &Iuk
				new Transition (3544, 3545), // &iu -> &iuk
				new Transition (3608, 3609), // &Ju -> &Juk
				new Transition (3613, 3614), // &ju -> &juk
				new Transition (3776, 3777), // &larrh -> &larrhk
				new Transition (3818, 3819), // &lbbr -> &lbbrk
				new Transition (3821, 3828), // &lbr -> &lbrk
				new Transition (3823, 3826), // &lbrac -> &lbrack
				new Transition (3909, 3910), // &LeftAngleBrac -> &LeftAngleBrack
				new Transition (3970, 3971), // &LeftDoubleBrac -> &LeftDoubleBrack
				new Transition (4335, 4336), // &lhbl -> &lhblk
				new Transition (4431, 4432), // &lobr -> &lobrk
				new Transition (4686, 4687), // &Lstro -> &Lstrok
				new Transition (4691, 4692), // &lstro -> &lstrok
				new Transition (4804, 4805), // &mar -> &mark
				new Transition (5068, 5069), // &nearh -> &nearhk
				new Transition (5106, 5107), // &NegativeThic -> &NegativeThick
				new Transition (5351, 5352), // &NoBrea -> &NoBreak
				new Transition (5358, 5359), // &NonBrea -> &NonBreak
				new Transition (6114, 6115), // &nwarh -> &nwarhk
				new Transition (6444, 6447), // &OverBrac -> &OverBrack
				new Transition (6515, 6516), // &perten -> &pertenk
				new Transition (6550, 6551), // &pitchfor -> &pitchfork
				new Transition (6557, 6563), // &plan -> &plank
				new Transition (6558, 6559), // &planc -> &planck
				new Transition (6947, 6948), // &rarrh -> &rarrhk
				new Transition (7002, 7003), // &rbbr -> &rbbrk
				new Transition (7005, 7012), // &rbr -> &rbrk
				new Transition (7007, 7010), // &rbrac -> &rbrack
				new Transition (7183, 7184), // &RightAngleBrac -> &RightAngleBrack
				new Transition (7245, 7246), // &RightDoubleBrac -> &RightDoubleBrack
				new Transition (7478, 7479), // &robr -> &robrk
				new Transition (7706, 7707), // &searh -> &searhk
				new Transition (8378, 8379), // &swarh -> &swarhk
				new Transition (8416, 8417), // &tbr -> &tbrk
				new Transition (8461, 8527), // &th -> &thk
				new Transition (8494, 8495), // &thic -> &thick
				new Transition (8508, 8509), // &Thic -> &Thick
				new Transition (8611, 8612), // &topfor -> &topfork
				new Transition (8729, 8730), // &Tstro -> &Tstrok
				new Transition (8734, 8735), // &tstro -> &tstrok
				new Transition (8884, 8885), // &uhbl -> &uhblk
				new Transition (8926, 8929), // &UnderBrac -> &UnderBrack
				new Transition (9208, 9217) // &var -> &vark
			};
			TransitionTable_l = new Transition[438] {
				new Transition (0, 3692), // & -> &l
				new Transition (1, 89), // &A -> &Al
				new Transition (8, 79), // &a -> &al
				new Transition (50, 51), // &AE -> &AEl
				new Transition (55, 56), // &ae -> &ael
				new Transition (104, 108), // &ama -> &amal
				new Transition (128, 129), // &ands -> &andsl
				new Transition (136, 140), // &ang -> &angl
				new Transition (217, 218), // &App -> &Appl
				new Transition (270, 271), // &Ati -> &Atil
				new Transition (276, 277), // &ati -> &atil
				new Transition (282, 283), // &Aum -> &Auml
				new Transition (286, 287), // &aum -> &auml
				new Transition (301, 519), // &b -> &bl
				new Transition (313, 314), // &backepsi -> &backepsil
				new Transition (335, 336), // &Backs -> &Backsl
				new Transition (417, 418), // &Bernou -> &Bernoul
				new Transition (418, 419), // &Bernoul -> &Bernoull
				new Transition (460, 461), // &bigop -> &bigopl
				new Transition (486, 487), // &bigtriang -> &bigtriangl
				new Transition (498, 499), // &bigup -> &bigupl
				new Transition (522, 523), // &black -> &blackl
				new Transition (543, 544), // &blacktriang -> &blacktriangl
				new Transition (545, 552), // &blacktriangle -> &blacktrianglel
				new Transition (618, 621), // &boxD -> &boxDl
				new Transition (623, 626), // &boxd -> &boxdl
				new Transition (662, 663), // &boxp -> &boxpl
				new Transition (673, 676), // &boxU -> &boxUl
				new Transition (678, 681), // &boxu -> &boxul
				new Transition (691, 705), // &boxV -> &boxVl
				new Transition (693, 709), // &boxv -> &boxvl
				new Transition (757, 758), // &bso -> &bsol
				new Transition (767, 768), // &bu -> &bul
				new Transition (768, 769), // &bul -> &bull
				new Transition (789, 1068), // &C -> &Cl
				new Transition (796, 1117), // &c -> &cl
				new Transition (830, 831), // &Capita -> &Capital
				new Transition (842, 843), // &CapitalDifferentia -> &CapitalDifferential
				new Transition (855, 856), // &Cay -> &Cayl
				new Transition (878, 879), // &Ccedi -> &Ccedil
				new Transition (883, 884), // &ccedi -> &ccedil
				new Transition (917, 918), // &cedi -> &cedil
				new Transition (922, 923), // &Cedi -> &Cedil
				new Transition (923, 924), // &Cedil -> &Cedill
				new Transition (981, 986), // &circ -> &circl
				new Transition (992, 993), // &circlearrow -> &circlearrowl
				new Transition (1021, 1022), // &Circ -> &Circl
				new Transition (1038, 1039), // &CircleP -> &CirclePl
				new Transition (1089, 1090), // &ClockwiseContourIntegra -> &ClockwiseContourIntegral
				new Transition (1096, 1097), // &CloseCur -> &CloseCurl
				new Transition (1102, 1103), // &CloseCurlyDoub -> &CloseCurlyDoubl
				new Transition (1126, 1127), // &Co -> &Col
				new Transition (1131, 1132), // &co -> &col
				new Transition (1148, 1153), // &comp -> &compl
				new Transition (1197, 1198), // &ContourIntegra -> &ContourIntegral
				new Transition (1231, 1232), // &CounterC -> &CounterCl
				new Transition (1253, 1254), // &CounterClockwiseContourIntegra -> &CounterClockwiseContourIntegral
				new Transition (1292, 1308), // &cu -> &cul
				new Transition (1296, 1297), // &cudarr -> &cudarrl
				new Transition (1346, 1353), // &cur -> &curl
				new Transition (1387, 1388), // &curvearrow -> &curvearrowl
				new Transition (1419, 1420), // &cy -> &cyl
				new Transition (1432, 1669), // &d -> &dl
				new Transition (1433, 1439), // &da -> &dal
				new Transition (1463, 1470), // &db -> &dbl
				new Transition (1516, 1525), // &de -> &del
				new Transition (1519, 1520), // &De -> &Del
				new Transition (1552, 1553), // &dhar -> &dharl
				new Transition (1565, 1566), // &Diacritica -> &Diacritical
				new Transition (1578, 1579), // &DiacriticalDoub -> &DiacriticalDoubl
				new Transition (1594, 1595), // &DiacriticalTi -> &DiacriticalTil
				new Transition (1629, 1630), // &Differentia -> &Differential
				new Transition (1679, 1680), // &do -> &dol
				new Transition (1680, 1681), // &dol -> &doll
				new Transition (1710, 1711), // &DotEqua -> &DotEqual
				new Transition (1719, 1720), // &dotp -> &dotpl
				new Transition (1732, 1733), // &doub -> &doubl
				new Transition (1745, 1746), // &Doub -> &Doubl
				new Transition (1761, 1762), // &DoubleContourIntegra -> &DoubleContourIntegral
				new Transition (1875, 1876), // &DoubleVertica -> &DoubleVertical
				new Transition (1938, 1939), // &downharpoon -> &downharpoonl
				new Transition (2054, 2055), // &dso -> &dsol
				new Transition (2089, 2090), // &dwang -> &dwangl
				new Transition (2108, 2206), // &E -> &El
				new Transition (2115, 2204), // &e -> &el
				new Transition (2148, 2149), // &eco -> &ecol
				new Transition (2204, 2220), // &el -> &ell
				new Transition (2251, 2252), // &EmptySma -> &EmptySmal
				new Transition (2252, 2253), // &EmptySmal -> &EmptySmall
				new Transition (2269, 2270), // &EmptyVerySma -> &EmptyVerySmal
				new Transition (2270, 2271), // &EmptyVerySmal -> &EmptyVerySmall
				new Transition (2312, 2319), // &ep -> &epl
				new Transition (2316, 2317), // &epars -> &eparsl
				new Transition (2324, 2333), // &epsi -> &epsil
				new Transition (2328, 2329), // &Epsi -> &Epsil
				new Transition (2345, 2346), // &eqco -> &eqcol
				new Transition (2350, 2354), // &eqs -> &eqsl
				new Transition (2357, 2362), // &eqslant -> &eqslantl
				new Transition (2369, 2370), // &Equa -> &Equal
				new Transition (2373, 2374), // &equa -> &equal
				new Transition (2378, 2379), // &EqualTi -> &EqualTil
				new Transition (2387, 2388), // &Equi -> &Equil
				new Transition (2406, 2407), // &eqvpars -> &eqvparsl
				new Transition (2448, 2449), // &Eum -> &Euml
				new Transition (2452, 2453), // &eum -> &euml
				new Transition (2459, 2460), // &exc -> &excl
				new Transition (2489, 2490), // &Exponentia -> &Exponential
				new Transition (2499, 2500), // &exponentia -> &exponential
				new Transition (2503, 2592), // &f -> &fl
				new Transition (2504, 2505), // &fa -> &fal
				new Transition (2505, 2506), // &fal -> &fall
				new Transition (2526, 2527), // &fema -> &femal
				new Transition (2530, 2536), // &ff -> &ffl
				new Transition (2531, 2532), // &ffi -> &ffil
				new Transition (2536, 2540), // &ffl -> &ffll
				new Transition (2549, 2550), // &fi -> &fil
				new Transition (2554, 2555), // &Fi -> &Fil
				new Transition (2555, 2556), // &Fil -> &Fill
				new Transition (2561, 2562), // &FilledSma -> &FilledSmal
				new Transition (2562, 2563), // &FilledSmal -> &FilledSmall
				new Transition (2577, 2578), // &FilledVerySma -> &FilledVerySmal
				new Transition (2578, 2579), // &FilledVerySmal -> &FilledVerySmall
				new Transition (2587, 2588), // &fj -> &fjl
				new Transition (2592, 2596), // &fl -> &fll
				new Transition (2617, 2618), // &ForA -> &ForAl
				new Transition (2618, 2619), // &ForAl -> &ForAll
				new Transition (2622, 2623), // &fora -> &foral
				new Transition (2623, 2624), // &foral -> &forall
				new Transition (2686, 2687), // &fras -> &frasl
				new Transition (2701, 2824), // &g -> &gl
				new Transition (2739, 2740), // &Gcedi -> &Gcedil
				new Transition (2763, 2767), // &gE -> &gEl
				new Transition (2765, 2769), // &ge -> &gel
				new Transition (2775, 2776), // &geqs -> &geqsl
				new Transition (2781, 2794), // &ges -> &gesl
				new Transition (2790, 2792), // &gesdoto -> &gesdotol
				new Transition (2813, 2814), // &gime -> &gimel
				new Transition (2875, 2876), // &GreaterEqua -> &GreaterEqual
				new Transition (2884, 2885), // &GreaterFu -> &GreaterFul
				new Transition (2885, 2886), // &GreaterFul -> &GreaterFull
				new Transition (2890, 2891), // &GreaterFullEqua -> &GreaterFullEqual
				new Transition (2906, 2907), // &GreaterS -> &GreaterSl
				new Transition (2914, 2915), // &GreaterSlantEqua -> &GreaterSlantEqual
				new Transition (2918, 2919), // &GreaterTi -> &GreaterTil
				new Transition (2932, 2936), // &gsim -> &gsiml
				new Transition (2942, 2954), // &gt -> &gtl
				new Transition (2965, 2993), // &gtr -> &gtrl
				new Transition (2981, 2982), // &gtreq -> &gtreql
				new Transition (2987, 2988), // &gtreqq -> &gtreqql
				new Transition (3021, 3027), // &ha -> &hal
				new Transition (3031, 3032), // &hami -> &hamil
				new Transition (3074, 3084), // &he -> &hel
				new Transition (3084, 3085), // &hel -> &hell
				new Transition (3100, 3101), // &Hi -> &Hil
				new Transition (3137, 3138), // &hook -> &hookl
				new Transition (3177, 3178), // &Horizonta -> &Horizontal
				new Transition (3188, 3192), // &hs -> &hsl
				new Transition (3222, 3223), // &HumpEqua -> &HumpEqual
				new Transition (3227, 3228), // &hybu -> &hybul
				new Transition (3228, 3229), // &hybul -> &hybull
				new Transition (3278, 3279), // &iexc -> &iexcl
				new Transition (3320, 3321), // &IJ -> &IJl
				new Transition (3325, 3326), // &ij -> &ijl
				new Transition (3341, 3352), // &imag -> &imagl
				new Transition (3372, 3373), // &Imp -> &Impl
				new Transition (3401, 3433), // &int -> &intl
				new Transition (3404, 3405), // &intca -> &intcal
				new Transition (3416, 3417), // &Integra -> &Integral
				new Transition (3421, 3422), // &interca -> &intercal
				new Transition (3448, 3449), // &Invisib -> &Invisibl
				new Transition (3529, 3530), // &Iti -> &Itil
				new Transition (3534, 3535), // &iti -> &itil
				new Transition (3549, 3550), // &Ium -> &Iuml
				new Transition (3552, 3553), // &ium -> &iuml
				new Transition (3635, 3636), // &Kcedi -> &Kcedil
				new Transition (3641, 3642), // &kcedi -> &kcedil
				new Transition (3692, 4348), // &l -> &ll
				new Transition (3698, 4346), // &L -> &Ll
				new Transition (3737, 3741), // &lang -> &langl
				new Transition (3746, 3747), // &Lap -> &Lapl
				new Transition (3766, 3779), // &larr -> &larrl
				new Transition (3782, 3783), // &larrp -> &larrpl
				new Transition (3789, 3790), // &larrt -> &larrtl
				new Transition (3796, 3797), // &lAtai -> &lAtail
				new Transition (3800, 3801), // &latai -> &latail
				new Transition (3831, 3832), // &lbrks -> &lbrksl
				new Transition (3851, 3852), // &Lcedi -> &Lcedil
				new Transition (3856, 3857), // &lcedi -> &lcedil
				new Transition (3859, 3860), // &lcei -> &lceil
				new Transition (3903, 3904), // &LeftAng -> &LeftAngl
				new Transition (3926, 4019), // &left -> &leftl
				new Transition (3950, 3951), // &leftarrowtai -> &leftarrowtail
				new Transition (3955, 3956), // &LeftCei -> &LeftCeil
				new Transition (3964, 3965), // &LeftDoub -> &LeftDoubl
				new Transition (3998, 3999), // &LeftF -> &LeftFl
				new Transition (4124, 4125), // &LeftTriang -> &LeftTriangl
				new Transition (4135, 4136), // &LeftTriangleEqua -> &LeftTriangleEqual
				new Transition (4191, 4192), // &leqs -> &leqsl
				new Transition (4243, 4244), // &LessEqua -> &LessEqual
				new Transition (4254, 4255), // &LessFu -> &LessFul
				new Transition (4255, 4256), // &LessFul -> &LessFull
				new Transition (4260, 4261), // &LessFullEqua -> &LessFullEqual
				new Transition (4284, 4285), // &LessS -> &LessSl
				new Transition (4292, 4293), // &LessSlantEqua -> &LessSlantEqual
				new Transition (4296, 4297), // &LessTi -> &LessTil
				new Transition (4301, 4307), // &lf -> &lfl
				new Transition (4330, 4332), // &lharu -> &lharul
				new Transition (4334, 4335), // &lhb -> &lhbl
				new Transition (4436, 4447), // &Long -> &Longl
				new Transition (4458, 4459), // &long -> &longl
				new Transition (4548, 4549), // &looparrow -> &looparrowl
				new Transition (4560, 4569), // &lop -> &lopl
				new Transition (4623, 4625), // &lpar -> &lparl
				new Transition (4698, 4720), // &lt -> &ltl
				new Transition (4767, 4909), // &m -> &ml
				new Transition (4768, 4772), // &ma -> &mal
				new Transition (4789, 4796), // &mapsto -> &mapstol
				new Transition (4839, 4840), // &measuredang -> &measuredangl
				new Transition (4843, 4854), // &Me -> &Mel
				new Transition (4854, 4855), // &Mel -> &Mell
				new Transition (4904, 4905), // &MinusP -> &MinusPl
				new Transition (4917, 4918), // &mnp -> &mnpl
				new Transition (4924, 4925), // &mode -> &model
				new Transition (4952, 4954), // &mu -> &mul
				new Transition (4965, 5256), // &n -> &nl
				new Transition (4967, 4968), // &nab -> &nabl
				new Transition (5005, 5006), // &natura -> &natural
				new Transition (5036, 5037), // &Ncedi -> &Ncedil
				new Transition (5041, 5042), // &ncedi -> &ncedil
				new Transition (5204, 5205), // &ngeqs -> &ngeqsl
				new Transition (5272, 5326), // &nL -> &nLl
				new Transition (5316, 5317), // &nleqs -> &nleqsl
				new Transition (5399, 5400), // &NotDoub -> &NotDoubl
				new Transition (5408, 5409), // &NotDoubleVertica -> &NotDoubleVertical
				new Transition (5414, 5415), // &NotE -> &NotEl
				new Transition (5424, 5425), // &NotEqua -> &NotEqual
				new Transition (5428, 5429), // &NotEqualTi -> &NotEqualTil
				new Transition (5450, 5451), // &NotGreaterEqua -> &NotGreaterEqual
				new Transition (5454, 5455), // &NotGreaterFu -> &NotGreaterFul
				new Transition (5455, 5456), // &NotGreaterFul -> &NotGreaterFull
				new Transition (5460, 5461), // &NotGreaterFullEqua -> &NotGreaterFullEqual
				new Transition (5476, 5477), // &NotGreaterS -> &NotGreaterSl
				new Transition (5484, 5485), // &NotGreaterSlantEqua -> &NotGreaterSlantEqual
				new Transition (5488, 5489), // &NotGreaterTi -> &NotGreaterTil
				new Transition (5509, 5510), // &NotHumpEqua -> &NotHumpEqual
				new Transition (5537, 5538), // &NotLeftTriang -> &NotLeftTriangl
				new Transition (5548, 5549), // &NotLeftTriangleEqua -> &NotLeftTriangleEqual
				new Transition (5557, 5558), // &NotLessEqua -> &NotLessEqual
				new Transition (5573, 5574), // &NotLessS -> &NotLessSl
				new Transition (5581, 5582), // &NotLessSlantEqua -> &NotLessSlantEqual
				new Transition (5585, 5586), // &NotLessTi -> &NotLessTil
				new Transition (5642, 5643), // &NotPrecedesEqua -> &NotPrecedesEqual
				new Transition (5645, 5646), // &NotPrecedesS -> &NotPrecedesSl
				new Transition (5653, 5654), // &NotPrecedesSlantEqua -> &NotPrecedesSlantEqual
				new Transition (5663, 5664), // &NotReverseE -> &NotReverseEl
				new Transition (5680, 5681), // &NotRightTriang -> &NotRightTriangl
				new Transition (5691, 5692), // &NotRightTriangleEqua -> &NotRightTriangleEqual
				new Transition (5710, 5711), // &NotSquareSubsetEqua -> &NotSquareSubsetEqual
				new Transition (5723, 5724), // &NotSquareSupersetEqua -> &NotSquareSupersetEqual
				new Transition (5735, 5736), // &NotSubsetEqua -> &NotSubsetEqual
				new Transition (5748, 5749), // &NotSucceedsEqua -> &NotSucceedsEqual
				new Transition (5751, 5752), // &NotSucceedsS -> &NotSucceedsSl
				new Transition (5759, 5760), // &NotSucceedsSlantEqua -> &NotSucceedsSlantEqual
				new Transition (5763, 5764), // &NotSucceedsTi -> &NotSucceedsTil
				new Transition (5778, 5779), // &NotSupersetEqua -> &NotSupersetEqual
				new Transition (5782, 5783), // &NotTi -> &NotTil
				new Transition (5790, 5791), // &NotTildeEqua -> &NotTildeEqual
				new Transition (5794, 5795), // &NotTildeFu -> &NotTildeFul
				new Transition (5795, 5796), // &NotTildeFul -> &NotTildeFull
				new Transition (5800, 5801), // &NotTildeFullEqua -> &NotTildeFullEqual
				new Transition (5804, 5805), // &NotTildeTi -> &NotTildeTil
				new Transition (5815, 5816), // &NotVertica -> &NotVertical
				new Transition (5825, 5826), // &npara -> &nparal
				new Transition (5826, 5827), // &nparal -> &nparall
				new Transition (5828, 5829), // &nparalle -> &nparallel
				new Transition (5831, 5832), // &npars -> &nparsl
				new Transition (5836, 5837), // &npo -> &npol
				new Transition (5921, 5922), // &nshortpara -> &nshortparal
				new Transition (5922, 5923), // &nshortparal -> &nshortparall
				new Transition (5924, 5925), // &nshortparalle -> &nshortparallel
				new Transition (5988, 6003), // &nt -> &ntl
				new Transition (5989, 5990), // &ntg -> &ntgl
				new Transition (5993, 5994), // &Nti -> &Ntil
				new Transition (5998, 5999), // &nti -> &ntil
				new Transition (6010, 6011), // &ntriang -> &ntriangl
				new Transition (6012, 6013), // &ntriangle -> &ntrianglel
				new Transition (6043, 6084), // &nv -> &nvl
				new Transition (6138, 6238), // &o -> &ol
				new Transition (6169, 6170), // &Odb -> &Odbl
				new Transition (6174, 6175), // &odb -> &odbl
				new Transition (6186, 6187), // &odso -> &odsol
				new Transition (6190, 6191), // &OE -> &OEl
				new Transition (6195, 6196), // &oe -> &oel
				new Transition (6302, 6336), // &op -> &opl
				new Transition (6311, 6312), // &OpenCur -> &OpenCurl
				new Transition (6317, 6318), // &OpenCurlyDoub -> &OpenCurlyDoubl
				new Transition (6368, 6369), // &ors -> &orsl
				new Transition (6378, 6386), // &Os -> &Osl
				new Transition (6382, 6391), // &os -> &osl
				new Transition (6396, 6397), // &oso -> &osol
				new Transition (6400, 6401), // &Oti -> &Otil
				new Transition (6406, 6407), // &oti -> &otil
				new Transition (6423, 6424), // &Oum -> &Ouml
				new Transition (6427, 6428), // &oum -> &ouml
				new Transition (6463, 6555), // &p -> &pl
				new Transition (6467, 6469), // &para -> &paral
				new Transition (6469, 6470), // &paral -> &parall
				new Transition (6471, 6472), // &paralle -> &parallel
				new Transition (6474, 6478), // &pars -> &parsl
				new Transition (6482, 6587), // &P -> &Pl
				new Transition (6487, 6488), // &Partia -> &Partial
				new Transition (6508, 6509), // &permi -> &permil
				new Transition (6616, 6617), // &Poincarep -> &Poincarepl
				new Transition (6666, 6667), // &preccur -> &preccurl
				new Transition (6682, 6683), // &PrecedesEqua -> &PrecedesEqual
				new Transition (6685, 6686), // &PrecedesS -> &PrecedesSl
				new Transition (6693, 6694), // &PrecedesSlantEqua -> &PrecedesSlantEqual
				new Transition (6697, 6698), // &PrecedesTi -> &PrecedesTil
				new Transition (6754, 6760), // &prof -> &profl
				new Transition (6755, 6756), // &profa -> &profal
				new Transition (6780, 6781), // &Proportiona -> &Proportional
				new Transition (6792, 6793), // &prure -> &prurel
				new Transition (6876, 7442), // &r -> &rl
				new Transition (6912, 6918), // &rang -> &rangl
				new Transition (6932, 6950), // &rarr -> &rarrl
				new Transition (6953, 6954), // &rarrp -> &rarrpl
				new Transition (6960, 6961), // &Rarrt -> &Rarrtl
				new Transition (6963, 6964), // &rarrt -> &rarrtl
				new Transition (6970, 6971), // &rAtai -> &rAtail
				new Transition (6975, 6976), // &ratai -> &ratail
				new Transition (6982, 6983), // &rationa -> &rational
				new Transition (7015, 7016), // &rbrks -> &rbrksl
				new Transition (7035, 7036), // &Rcedi -> &Rcedil
				new Transition (7040, 7041), // &rcedi -> &rcedil
				new Transition (7043, 7044), // &rcei -> &rceil
				new Transition (7053, 7057), // &rd -> &rdl
				new Transition (7075, 7076), // &rea -> &real
				new Transition (7102, 7103), // &ReverseE -> &ReverseEl
				new Transition (7112, 7113), // &ReverseEqui -> &ReverseEquil
				new Transition (7126, 7127), // &ReverseUpEqui -> &ReverseUpEquil
				new Transition (7135, 7141), // &rf -> &rfl
				new Transition (7160, 7162), // &rharu -> &rharul
				new Transition (7177, 7178), // &RightAng -> &RightAngl
				new Transition (7202, 7294), // &right -> &rightl
				new Transition (7225, 7226), // &rightarrowtai -> &rightarrowtail
				new Transition (7230, 7231), // &RightCei -> &RightCeil
				new Transition (7239, 7240), // &RightDoub -> &RightDoubl
				new Transition (7273, 7274), // &RightF -> &RightFl
				new Transition (7369, 7370), // &RightTriang -> &RightTriangl
				new Transition (7380, 7381), // &RightTriangleEqua -> &RightTriangleEqual
				new Transition (7481, 7491), // &rop -> &ropl
				new Transition (7506, 7507), // &RoundImp -> &RoundImpl
				new Transition (7520, 7521), // &rppo -> &rppol
				new Transition (7579, 7585), // &rtri -> &rtril
				new Transition (7590, 7591), // &Ru -> &Rul
				new Transition (7594, 7595), // &RuleDe -> &RuleDel
				new Transition (7601, 7602), // &ru -> &rul
				new Transition (7617, 7878), // &s -> &sl
				new Transition (7655, 7656), // &Scedi -> &Scedil
				new Transition (7659, 7660), // &scedi -> &scedil
				new Transition (7681, 7682), // &scpo -> &scpol
				new Transition (7806, 7807), // &shortpara -> &shortparal
				new Transition (7807, 7808), // &shortparal -> &shortparall
				new Transition (7809, 7810), // &shortparalle -> &shortparallel
				new Transition (7847, 7861), // &sim -> &siml
				new Transition (7868, 7869), // &simp -> &simpl
				new Transition (7884, 7885), // &Sma -> &Smal
				new Transition (7885, 7886), // &Smal -> &Small
				new Transition (7890, 7891), // &SmallCirc -> &SmallCircl
				new Transition (7895, 7896), // &sma -> &smal
				new Transition (7896, 7897), // &smal -> &small
				new Transition (7915, 7916), // &smepars -> &smeparsl
				new Transition (7918, 7921), // &smi -> &smil
				new Transition (7936, 7942), // &so -> &sol
				new Transition (8042, 8043), // &SquareSubsetEqua -> &SquareSubsetEqual
				new Transition (8055, 8056), // &SquareSupersetEqua -> &SquareSupersetEqual
				new Transition (8087, 8088), // &ssmi -> &ssmil
				new Transition (8115, 8116), // &straightepsi -> &straightepsil
				new Transition (8146, 8147), // &submu -> &submul
				new Transition (8155, 8156), // &subp -> &subpl
				new Transition (8181, 8182), // &SubsetEqua -> &SubsetEqual
				new Transition (8210, 8211), // &succcur -> &succcurl
				new Transition (8226, 8227), // &SucceedsEqua -> &SucceedsEqual
				new Transition (8229, 8230), // &SucceedsS -> &SucceedsSl
				new Transition (8237, 8238), // &SucceedsSlantEqua -> &SucceedsSlantEqual
				new Transition (8241, 8242), // &SucceedsTi -> &SucceedsTil
				new Transition (8284, 8328), // &sup -> &supl
				new Transition (8317, 8318), // &SupersetEqua -> &SupersetEqual
				new Transition (8322, 8323), // &suphso -> &suphsol
				new Transition (8334, 8335), // &supmu -> &supmul
				new Transition (8343, 8344), // &supp -> &suppl
				new Transition (8395, 8396), // &sz -> &szl
				new Transition (8433, 8434), // &Tcedi -> &Tcedil
				new Transition (8438, 8439), // &tcedi -> &tcedil
				new Transition (8449, 8450), // &te -> &tel
				new Transition (8544, 8545), // &Ti -> &Til
				new Transition (8549, 8550), // &ti -> &til
				new Transition (8557, 8558), // &TildeEqua -> &TildeEqual
				new Transition (8561, 8562), // &TildeFu -> &TildeFul
				new Transition (8562, 8563), // &TildeFul -> &TildeFull
				new Transition (8567, 8568), // &TildeFullEqua -> &TildeFullEqual
				new Transition (8571, 8572), // &TildeTi -> &TildeTil
				new Transition (8636, 8637), // &triang -> &triangl
				new Transition (8638, 8645), // &triangle -> &trianglel
				new Transition (8678, 8679), // &Trip -> &Tripl
				new Transition (8685, 8686), // &trip -> &tripl
				new Transition (8746, 8747), // &twohead -> &twoheadl
				new Transition (8775, 8887), // &u -> &ul
				new Transition (8835, 8836), // &Udb -> &Udbl
				new Transition (8840, 8841), // &udb -> &udbl
				new Transition (8878, 8879), // &uhar -> &uharl
				new Transition (8883, 8884), // &uhb -> &uhbl
				new Transition (8909, 8914), // &um -> &uml
				new Transition (8949, 8950), // &UnionP -> &UnionPl
				new Transition (8983, 9064), // &up -> &upl
				new Transition (9037, 9038), // &UpEqui -> &UpEquil
				new Transition (9052, 9053), // &upharpoon -> &upharpoonl
				new Transition (9093, 9100), // &Upsi -> &Upsil
				new Transition (9096, 9104), // &upsi -> &upsil
				new Transition (9167, 9168), // &Uti -> &Util
				new Transition (9172, 9173), // &uti -> &util
				new Transition (9188, 9189), // &Uum -> &Uuml
				new Transition (9191, 9192), // &uum -> &uuml
				new Transition (9197, 9198), // &uwang -> &uwangl
				new Transition (9201, 9420), // &v -> &vl
				new Transition (9212, 9213), // &varepsi -> &varepsil
				new Transition (9289, 9290), // &vartriang -> &vartriangl
				new Transition (9291, 9292), // &vartriangle -> &vartrianglel
				new Transition (9328, 9340), // &Vdash -> &Vdashl
				new Transition (9345, 9355), // &ve -> &vel
				new Transition (9355, 9356), // &vel -> &vell
				new Transition (9376, 9377), // &Vertica -> &Vertical
				new Transition (9398, 9399), // &VerticalTi -> &VerticalTil
				new Transition (9548, 9585), // &x -> &xl
				new Transition (9611, 9614), // &xop -> &xopl
				new Transition (9646, 9647), // &xup -> &xupl
				new Transition (9741, 9742), // &Yum -> &Yuml
				new Transition (9744, 9745) // &yum -> &yuml
			};
			TransitionTable_m = new Transition[177] {
				new Transition (0, 4767), // & -> &m
				new Transition (1, 98), // &A -> &Am
				new Transition (8, 103), // &a -> &am
				new Transition (83, 84), // &alefsy -> &alefsym
				new Transition (136, 143), // &ang -> &angm
				new Transition (262, 263), // &asy -> &asym
				new Transition (281, 282), // &Au -> &Aum
				new Transition (285, 286), // &au -> &aum
				new Transition (320, 321), // &backpri -> &backprim
				new Transition (325, 326), // &backsi -> &backsim
				new Transition (384, 399), // &be -> &bem
				new Transition (466, 467), // &bigoti -> &bigotim
				new Transition (605, 606), // &botto -> &bottom
				new Transition (613, 656), // &box -> &boxm
				new Transition (668, 669), // &boxti -> &boxtim
				new Transition (721, 722), // &bpri -> &bprim
				new Transition (748, 749), // &bse -> &bsem
				new Transition (752, 753), // &bsi -> &bsim
				new Transition (767, 774), // &bu -> &bum
				new Transition (781, 782), // &Bu -> &Bum
				new Transition (904, 905), // &ccupss -> &ccupssm
				new Transition (915, 927), // &ce -> &cem
				new Transition (966, 968), // &check -> &checkm
				new Transition (979, 1059), // &cir -> &cirm
				new Transition (1044, 1045), // &CircleTi -> &CircleTim
				new Transition (1131, 1142), // &co -> &com
				new Transition (1142, 1143), // &com -> &comm
				new Transition (1154, 1155), // &comple -> &complem
				new Transition (1349, 1351), // &curarr -> &curarrm
				new Transition (1516, 1529), // &de -> &dem
				new Transition (1558, 1603), // &Dia -> &Diam
				new Transition (1600, 1601), // &dia -> &diam
				new Transition (1634, 1635), // &diga -> &digam
				new Transition (1635, 1636), // &digam -> &digamm
				new Transition (1652, 1653), // &divideonti -> &divideontim
				new Transition (1694, 1713), // &dot -> &dotm
				new Transition (2108, 2228), // &E -> &Em
				new Transition (2115, 2233), // &e -> &em
				new Transition (2207, 2208), // &Ele -> &Elem
				new Transition (2249, 2250), // &EmptyS -> &EmptySm
				new Transition (2267, 2268), // &EmptyVeryS -> &EmptyVerySm
				new Transition (2351, 2352), // &eqsi -> &eqsim
				new Transition (2393, 2394), // &Equilibriu -> &Equilibrium
				new Transition (2430, 2431), // &Esi -> &Esim
				new Transition (2433, 2434), // &esi -> &esim
				new Transition (2447, 2448), // &Eu -> &Eum
				new Transition (2451, 2452), // &eu -> &eum
				new Transition (2524, 2525), // &fe -> &fem
				new Transition (2559, 2560), // &FilledS -> &FilledSm
				new Transition (2575, 2576), // &FilledVeryS -> &FilledVerySm
				new Transition (2702, 2714), // &ga -> &gam
				new Transition (2709, 2710), // &Ga -> &Gam
				new Transition (2710, 2711), // &Gam -> &Gamm
				new Transition (2714, 2715), // &gam -> &gamm
				new Transition (2811, 2812), // &gi -> &gim
				new Transition (2850, 2851), // &gnsi -> &gnsim
				new Transition (2931, 2932), // &gsi -> &gsim
				new Transition (2999, 3000), // &gtrsi -> &gtrsim
				new Transition (3021, 3030), // &ha -> &ham
				new Transition (3126, 3131), // &ho -> &hom
				new Transition (3207, 3208), // &Hu -> &Hum
				new Transition (3215, 3216), // &HumpDownHu -> &HumpDownHum
				new Transition (3236, 3330), // &I -> &Im
				new Transition (3243, 3336), // &i -> &im
				new Transition (3452, 3453), // &InvisibleCo -> &InvisibleCom
				new Transition (3453, 3454), // &InvisibleCom -> &InvisibleComm
				new Transition (3458, 3459), // &InvisibleTi -> &InvisibleTim
				new Transition (3539, 3549), // &Iu -> &Ium
				new Transition (3544, 3552), // &iu -> &ium
				new Transition (3561, 3577), // &j -> &jm
				new Transition (3692, 4385), // &l -> &lm
				new Transition (3698, 4379), // &L -> &Lm
				new Transition (3699, 3723), // &La -> &Lam
				new Transition (3705, 3728), // &la -> &lam
				new Transition (3711, 3712), // &lae -> &laem
				new Transition (3786, 3787), // &larrsi -> &larrsim
				new Transition (4115, 4116), // &leftthreeti -> &leftthreetim
				new Transition (4281, 4282), // &lesssi -> &lesssim
				new Transition (4419, 4420), // &lnsi -> &lnsim
				new Transition (4458, 4502), // &long -> &longm
				new Transition (4574, 4575), // &loti -> &lotim
				new Transition (4628, 4646), // &lr -> &lrm
				new Transition (4669, 4670), // &lsi -> &lsim
				new Transition (4715, 4716), // &lti -> &ltim
				new Transition (4810, 4811), // &mco -> &mcom
				new Transition (4811, 4812), // &mcom -> &mcomm
				new Transition (4846, 4847), // &Mediu -> &Medium
				new Transition (4952, 4961), // &mu -> &mum
				new Transition (4956, 4957), // &multi -> &multim
				new Transition (4965, 5343), // &n -> &nm
				new Transition (5014, 5015), // &nbu -> &nbum
				new Transition (5095, 5096), // &NegativeMediu -> &NegativeMedium
				new Transition (5145, 5146), // &nesi -> &nesim
				new Transition (5216, 5217), // &ngsi -> &ngsim
				new Transition (5329, 5330), // &nlsi -> &nlsim
				new Transition (5416, 5417), // &NotEle -> &NotElem
				new Transition (5494, 5495), // &NotHu -> &NotHum
				new Transition (5502, 5503), // &NotHumpDownHu -> &NotHumpDownHum
				new Transition (5665, 5666), // &NotReverseEle -> &NotReverseElem
				new Transition (5895, 5934), // &ns -> &nsm
				new Transition (5913, 5914), // &nshort -> &nshortm
				new Transition (5927, 5928), // &nsi -> &nsim
				new Transition (6032, 6034), // &nu -> &num
				new Transition (6108, 6109), // &nvsi -> &nvsim
				new Transition (6131, 6258), // &O -> &Om
				new Transition (6138, 6263), // &o -> &om
				new Transition (6227, 6232), // &oh -> &ohm
				new Transition (6348, 6358), // &ord -> &ordm
				new Transition (6400, 6411), // &Oti -> &Otim
				new Transition (6406, 6415), // &oti -> &otim
				new Transition (6422, 6423), // &Ou -> &Oum
				new Transition (6426, 6427), // &ou -> &oum
				new Transition (6463, 6607), // &p -> &pm
				new Transition (6475, 6476), // &parsi -> &parsim
				new Transition (6498, 6507), // &per -> &perm
				new Transition (6527, 6532), // &ph -> &phm
				new Transition (6532, 6533), // &phm -> &phmm
				new Transition (6567, 6596), // &plus -> &plusm
				new Transition (6600, 6601), // &plussi -> &plussim
				new Transition (6718, 6719), // &precnsi -> &precnsim
				new Transition (6722, 6723), // &precsi -> &precsim
				new Transition (6725, 6726), // &Pri -> &Prim
				new Transition (6729, 6730), // &pri -> &prim
				new Transition (6742, 6743), // &prnsi -> &prnsim
				new Transition (6787, 6788), // &prsi -> &prsim
				new Transition (6835, 6836), // &qpri -> &qprim
				new Transition (6876, 7453), // &r -> &rm
				new Transition (6901, 6902), // &rae -> &raem
				new Transition (6957, 6958), // &rarrsi -> &rarrsim
				new Transition (7104, 7105), // &ReverseEle -> &ReverseElem
				new Transition (7118, 7119), // &ReverseEquilibriu -> &ReverseEquilibrium
				new Transition (7132, 7133), // &ReverseUpEquilibriu -> &ReverseUpEquilibrium
				new Transition (7360, 7361), // &rightthreeti -> &rightthreetim
				new Transition (7442, 7451), // &rl -> &rlm
				new Transition (7464, 7465), // &rn -> &rnm
				new Transition (7496, 7497), // &roti -> &rotim
				new Transition (7504, 7505), // &RoundI -> &RoundIm
				new Transition (7573, 7574), // &rti -> &rtim
				new Transition (7610, 7883), // &S -> &Sm
				new Transition (7617, 7894), // &s -> &sm
				new Transition (7677, 7678), // &scnsi -> &scnsim
				new Transition (7688, 7689), // &scsi -> &scsim
				new Transition (7703, 7721), // &se -> &sem
				new Transition (7729, 7730), // &set -> &setm
				new Transition (7798, 7799), // &short -> &shortm
				new Transition (7834, 7835), // &Sig -> &Sigm
				new Transition (7838, 7847), // &si -> &sim
				new Transition (7839, 7840), // &sig -> &sigm
				new Transition (7900, 7901), // &smallset -> &smallsetm
				new Transition (8077, 8086), // &ss -> &ssm
				new Transition (8082, 8083), // &sset -> &ssetm
				new Transition (8127, 8275), // &Su -> &Sum
				new Transition (8130, 8277), // &su -> &sum
				new Transition (8131, 8145), // &sub -> &subm
				new Transition (8190, 8191), // &subsi -> &subsim
				new Transition (8262, 8263), // &succnsi -> &succnsim
				new Transition (8266, 8267), // &succsi -> &succsim
				new Transition (8284, 8333), // &sup -> &supm
				new Transition (8367, 8368), // &supsi -> &supsim
				new Transition (8488, 8489), // &thetasy -> &thetasym
				new Transition (8504, 8505), // &thicksi -> &thicksim
				new Transition (8532, 8533), // &thksi -> &thksim
				new Transition (8549, 8576), // &ti -> &tim
				new Transition (8619, 8620), // &tpri -> &tprim
				new Transition (8633, 8670), // &tri -> &trim
				new Transition (8694, 8695), // &triti -> &tritim
				new Transition (8702, 8703), // &trpeziu -> &trpezium
				new Transition (8768, 8904), // &U -> &Um
				new Transition (8775, 8909), // &u -> &um
				new Transition (9043, 9044), // &UpEquilibriu -> &UpEquilibrium
				new Transition (9182, 9191), // &uu -> &uum
				new Transition (9187, 9188), // &Uu -> &Uum
				new Transition (9254, 9255), // &varsig -> &varsigm
				new Transition (9548, 9594), // &x -> &xm
				new Transition (9619, 9620), // &xoti -> &xotim
				new Transition (9736, 9744), // &yu -> &yum
				new Transition (9740, 9741) // &Yu -> &Yum
			};
			TransitionTable_n = new Transition[303] {
				new Transition (0, 4965), // & -> &n
				new Transition (1, 116), // &A -> &An
				new Transition (8, 119), // &a -> &an
				new Transition (122, 123), // &anda -> &andan
				new Transition (185, 186), // &Aogo -> &Aogon
				new Transition (190, 191), // &aogo -> &aogon
				new Transition (221, 222), // &ApplyFu -> &ApplyFun
				new Transition (226, 227), // &ApplyFunctio -> &ApplyFunction
				new Transition (238, 239), // &Ari -> &Arin
				new Transition (243, 244), // &ari -> &arin
				new Transition (257, 258), // &Assig -> &Assign
				new Transition (291, 292), // &awco -> &awcon
				new Transition (293, 294), // &awconi -> &awconin
				new Transition (297, 298), // &awi -> &awin
				new Transition (301, 579), // &b -> &bn
				new Transition (306, 307), // &backco -> &backcon
				new Transition (315, 316), // &backepsilo -> &backepsilon
				new Transition (370, 371), // &bco -> &bcon
				new Transition (409, 410), // &ber -> &bern
				new Transition (414, 415), // &Ber -> &Bern
				new Transition (433, 434), // &betwee -> &between
				new Transition (484, 485), // &bigtria -> &bigtrian
				new Transition (491, 492), // &bigtriangledow -> &bigtriangledown
				new Transition (520, 563), // &bla -> &blan
				new Transition (526, 527), // &blackloze -> &blacklozen
				new Transition (541, 542), // &blacktria -> &blacktrian
				new Transition (549, 550), // &blacktriangledow -> &blacktriangledown
				new Transition (657, 658), // &boxmi -> &boxmin
				new Transition (807, 808), // &capa -> &capan
				new Transition (838, 839), // &CapitalDiffere -> &CapitalDifferen
				new Transition (852, 853), // &caro -> &caron
				new Transition (869, 870), // &Ccaro -> &Ccaron
				new Transition (873, 874), // &ccaro -> &ccaron
				new Transition (894, 895), // &Cco -> &Ccon
				new Transition (896, 897), // &Cconi -> &Cconin
				new Transition (915, 933), // &ce -> &cen
				new Transition (920, 936), // &Ce -> &Cen
				new Transition (1033, 1034), // &CircleMi -> &CircleMin
				new Transition (1053, 1054), // &cirf -> &cirfn
				new Transition (1055, 1056), // &cirfni -> &cirfnin
				new Transition (1077, 1078), // &ClockwiseCo -> &ClockwiseCon
				new Transition (1083, 1084), // &ClockwiseContourI -> &ClockwiseContourIn
				new Transition (1126, 1171), // &Co -> &Con
				new Transition (1128, 1129), // &Colo -> &Colon
				new Transition (1131, 1164), // &co -> &con
				new Transition (1133, 1134), // &colo -> &colon
				new Transition (1150, 1151), // &compf -> &compfn
				new Transition (1156, 1157), // &compleme -> &complemen
				new Transition (1175, 1176), // &Congrue -> &Congruen
				new Transition (1179, 1180), // &Coni -> &Conin
				new Transition (1183, 1184), // &coni -> &conin
				new Transition (1191, 1192), // &ContourI -> &ContourIn
				new Transition (1226, 1227), // &Cou -> &Coun
				new Transition (1241, 1242), // &CounterClockwiseCo -> &CounterClockwiseCon
				new Transition (1247, 1248), // &CounterClockwiseContourI -> &CounterClockwiseContourIn
				new Transition (1378, 1379), // &curre -> &curren
				new Transition (1409, 1410), // &cwco -> &cwcon
				new Transition (1411, 1412), // &cwconi -> &cwconin
				new Transition (1415, 1416), // &cwi -> &cwin
				new Transition (1477, 1478), // &Dcaro -> &Dcaron
				new Transition (1483, 1484), // &dcaro -> &dcaron
				new Transition (1604, 1605), // &Diamo -> &Diamon
				new Transition (1608, 1609), // &diamo -> &diamon
				new Transition (1625, 1626), // &Differe -> &Differen
				new Transition (1640, 1641), // &disi -> &disin
				new Transition (1649, 1650), // &divideo -> &divideon
				new Transition (1657, 1658), // &divo -> &divon
				new Transition (1672, 1673), // &dlcor -> &dlcorn
				new Transition (1714, 1715), // &dotmi -> &dotmin
				new Transition (1749, 1750), // &DoubleCo -> &DoubleCon
				new Transition (1755, 1756), // &DoubleContourI -> &DoubleContourIn
				new Transition (1768, 1769), // &DoubleDow -> &DoubleDown
				new Transition (1801, 1802), // &DoubleLo -> &DoubleLon
				new Transition (1861, 1862), // &DoubleUpDow -> &DoubleUpDown
				new Transition (1881, 1882), // &Dow -> &Down
				new Transition (1895, 1896), // &dow -> &down
				new Transition (1923, 1924), // &downdow -> &downdown
				new Transition (1937, 1938), // &downharpoo -> &downharpoon
				new Transition (2033, 2034), // &drcor -> &drcorn
				new Transition (2087, 2088), // &dwa -> &dwan
				new Transition (2115, 2290), // &e -> &en
				new Transition (2130, 2131), // &Ecaro -> &Ecaron
				new Transition (2136, 2137), // &ecaro -> &ecaron
				new Transition (2150, 2151), // &ecolo -> &ecolon
				new Transition (2209, 2210), // &Eleme -> &Elemen
				new Transition (2213, 2214), // &eli -> &elin
				new Transition (2298, 2299), // &Eogo -> &Eogon
				new Transition (2303, 2304), // &eogo -> &eogon
				new Transition (2330, 2331), // &Epsilo -> &Epsilon
				new Transition (2334, 2335), // &epsilo -> &epsilon
				new Transition (2347, 2348), // &eqcolo -> &eqcolon
				new Transition (2355, 2356), // &eqsla -> &eqslan
				new Transition (2479, 2480), // &expectatio -> &expectation
				new Transition (2483, 2484), // &Expo -> &Expon
				new Transition (2485, 2486), // &Expone -> &Exponen
				new Transition (2493, 2494), // &expo -> &expon
				new Transition (2495, 2496), // &expone -> &exponen
				new Transition (2503, 2604), // &f -> &fn
				new Transition (2507, 2508), // &falli -> &fallin
				new Transition (2600, 2601), // &flt -> &fltn
				new Transition (2643, 2644), // &fparti -> &fpartin
				new Transition (2690, 2691), // &frow -> &frown
				new Transition (2701, 2832), // &g -> &gn
				new Transition (2777, 2778), // &geqsla -> &geqslan
				new Transition (2908, 2909), // &GreaterSla -> &GreaterSlan
				new Transition (3002, 3011), // &gv -> &gvn
				new Transition (3005, 3006), // &gvert -> &gvertn
				new Transition (3091, 3092), // &herco -> &hercon
				new Transition (3174, 3175), // &Horizo -> &Horizon
				new Transition (3180, 3181), // &HorizontalLi -> &HorizontalLin
				new Transition (3212, 3213), // &HumpDow -> &HumpDown
				new Transition (3233, 3234), // &hyphe -> &hyphen
				new Transition (3236, 3398), // &I -> &In
				new Transition (3243, 3378), // &i -> &in
				new Transition (3301, 3311), // &ii -> &iin
				new Transition (3303, 3308), // &iii -> &iiin
				new Transition (3304, 3305), // &iiii -> &iiiin
				new Transition (3313, 3314), // &iinfi -> &iinfin
				new Transition (3345, 3346), // &Imagi -> &Imagin
				new Transition (3353, 3354), // &imagli -> &imaglin
				new Transition (3386, 3387), // &infi -> &infin
				new Transition (3430, 3431), // &Intersectio -> &Intersection
				new Transition (3473, 3474), // &Iogo -> &Iogon
				new Transition (3477, 3478), // &iogo -> &iogon
				new Transition (3511, 3512), // &isi -> &isin
				new Transition (3657, 3658), // &kgree -> &kgreen
				new Transition (3692, 4401), // &l -> &ln
				new Transition (3699, 3733), // &La -> &Lan
				new Transition (3705, 3736), // &la -> &lan
				new Transition (3720, 3721), // &lagra -> &lagran
				new Transition (3840, 3841), // &Lcaro -> &Lcaron
				new Transition (3846, 3847), // &lcaro -> &lcaron
				new Transition (3901, 3902), // &LeftA -> &LeftAn
				new Transition (3957, 3958), // &LeftCeili -> &LeftCeilin
				new Transition (3975, 3976), // &LeftDow -> &LeftDown
				new Transition (4009, 4010), // &leftharpoo -> &leftharpoon
				new Transition (4013, 4014), // &leftharpoondow -> &leftharpoondown
				new Transition (4070, 4071), // &leftrightharpoo -> &leftrightharpoon
				new Transition (4122, 4123), // &LeftTria -> &LeftTrian
				new Transition (4142, 4143), // &LeftUpDow -> &LeftUpDown
				new Transition (4193, 4194), // &leqsla -> &leqslan
				new Transition (4286, 4287), // &LessSla -> &LessSlan
				new Transition (4356, 4357), // &llcor -> &llcorn
				new Transition (4422, 4457), // &lo -> &lon
				new Transition (4423, 4424), // &loa -> &loan
				new Transition (4434, 4435), // &Lo -> &Lon
				new Transition (4614, 4615), // &loze -> &lozen
				new Transition (4635, 4636), // &lrcor -> &lrcorn
				new Transition (4755, 4764), // &lv -> &lvn
				new Transition (4758, 4759), // &lvert -> &lvertn
				new Transition (4767, 4916), // &m -> &mn
				new Transition (4793, 4794), // &mapstodow -> &mapstodown
				new Transition (4837, 4838), // &measureda -> &measuredan
				new Transition (4856, 4857), // &Melli -> &Mellin
				new Transition (4871, 4890), // &mi -> &min
				new Transition (4900, 4901), // &Mi -> &Min
				new Transition (4966, 4983), // &na -> &nan
				new Transition (5027, 5028), // &Ncaro -> &Ncaron
				new Transition (5031, 5032), // &ncaro -> &ncaron
				new Transition (5044, 5045), // &nco -> &ncon
				new Transition (5105, 5114), // &NegativeThi -> &NegativeThin
				new Transition (5127, 5128), // &NegativeVeryThi -> &NegativeVeryThin
				new Transition (5178, 5179), // &NewLi -> &NewLin
				new Transition (5206, 5207), // &ngeqsla -> &ngeqslan
				new Transition (5318, 5319), // &nleqsla -> &nleqslan
				new Transition (5347, 5354), // &No -> &Non
				new Transition (5360, 5361), // &NonBreaki -> &NonBreakin
				new Transition (5378, 5620), // &not -> &notn
				new Transition (5381, 5382), // &NotCo -> &NotCon
				new Transition (5386, 5387), // &NotCongrue -> &NotCongruen
				new Transition (5418, 5419), // &NotEleme -> &NotElemen
				new Transition (5478, 5479), // &NotGreaterSla -> &NotGreaterSlan
				new Transition (5499, 5500), // &NotHumpDow -> &NotHumpDown
				new Transition (5512, 5513), // &noti -> &notin
				new Transition (5535, 5536), // &NotLeftTria -> &NotLeftTrian
				new Transition (5575, 5576), // &NotLessSla -> &NotLessSlan
				new Transition (5647, 5648), // &NotPrecedesSla -> &NotPrecedesSlan
				new Transition (5667, 5668), // &NotReverseEleme -> &NotReverseElemen
				new Transition (5678, 5679), // &NotRightTria -> &NotRightTrian
				new Transition (5753, 5754), // &NotSucceedsSla -> &NotSucceedsSlan
				new Transition (5838, 5839), // &npoli -> &npolin
				new Transition (6008, 6009), // &ntria -> &ntrian
				new Transition (6078, 6079), // &nvi -> &nvin
				new Transition (6081, 6082), // &nvinfi -> &nvinfin
				new Transition (6111, 6126), // &nw -> &nwn
				new Transition (6211, 6212), // &ogo -> &ogon
				new Transition (6234, 6235), // &oi -> &oin
				new Transition (6252, 6253), // &oli -> &olin
				new Transition (6279, 6280), // &Omicro -> &Omicron
				new Transition (6282, 6290), // &omi -> &omin
				new Transition (6285, 6286), // &omicro -> &omicron
				new Transition (6307, 6308), // &Ope -> &Open
				new Transition (6454, 6455), // &OverPare -> &OverParen
				new Transition (6499, 6500), // &perc -> &percn
				new Transition (6514, 6515), // &perte -> &perten
				new Transition (6537, 6538), // &pho -> &phon
				new Transition (6556, 6557), // &pla -> &plan
				new Transition (6591, 6592), // &PlusMi -> &PlusMin
				new Transition (6596, 6597), // &plusm -> &plusmn
				new Transition (6610, 6611), // &Poi -> &Poin
				new Transition (6618, 6619), // &Poincarepla -> &Poincareplan
				new Transition (6623, 6624), // &poi -> &poin
				new Transition (6626, 6627), // &pointi -> &pointin
				new Transition (6636, 6637), // &pou -> &poun
				new Transition (6642, 6735), // &pr -> &prn
				new Transition (6655, 6705), // &prec -> &precn
				new Transition (6687, 6688), // &PrecedesSla -> &PrecedesSlan
				new Transition (6761, 6762), // &profli -> &proflin
				new Transition (6777, 6778), // &Proportio -> &Proportion
				new Transition (6807, 6808), // &pu -> &pun
				new Transition (6821, 6822), // &qi -> &qin
				new Transition (6851, 6852), // &quater -> &quatern
				new Transition (6854, 6855), // &quaternio -> &quaternion
				new Transition (6858, 6859), // &quati -> &quatin
				new Transition (6876, 7464), // &r -> &rn
				new Transition (6882, 6911), // &ra -> &ran
				new Transition (6887, 6908), // &Ra -> &Ran
				new Transition (6979, 6981), // &ratio -> &ration
				new Transition (7024, 7025), // &Rcaro -> &Rcaron
				new Transition (7030, 7031), // &rcaro -> &rcaron
				new Transition (7078, 7079), // &reali -> &realin
				new Transition (7106, 7107), // &ReverseEleme -> &ReverseElemen
				new Transition (7175, 7176), // &RightA -> &RightAn
				new Transition (7199, 7428), // &ri -> &rin
				new Transition (7232, 7233), // &RightCeili -> &RightCeilin
				new Transition (7250, 7251), // &RightDow -> &RightDown
				new Transition (7284, 7285), // &rightharpoo -> &rightharpoon
				new Transition (7288, 7289), // &rightharpoondow -> &rightharpoondown
				new Transition (7310, 7311), // &rightleftharpoo -> &rightleftharpoon
				new Transition (7367, 7368), // &RightTria -> &RightTrian
				new Transition (7387, 7388), // &RightUpDow -> &RightUpDown
				new Transition (7432, 7433), // &risi -> &risin
				new Transition (7470, 7471), // &roa -> &roan
				new Transition (7501, 7502), // &Rou -> &Roun
				new Transition (7522, 7523), // &rppoli -> &rppolin
				new Transition (7631, 7670), // &sc -> &scn
				new Transition (7638, 7639), // &Scaro -> &Scaron
				new Transition (7642, 7643), // &scaro -> &scaron
				new Transition (7683, 7684), // &scpoli -> &scpolin
				new Transition (7730, 7736), // &setm -> &setmn
				new Transition (7731, 7732), // &setmi -> &setmin
				new Transition (7748, 7749), // &sfrow -> &sfrown
				new Transition (7778, 7779), // &ShortDow -> &ShortDown
				new Transition (7847, 7865), // &sim -> &simn
				new Transition (7902, 7903), // &smallsetmi -> &smallsetmin
				new Transition (8019, 8020), // &SquareI -> &SquareIn
				new Transition (8029, 8030), // &SquareIntersectio -> &SquareIntersection
				new Transition (8058, 8059), // &SquareU -> &SquareUn
				new Transition (8061, 8062), // &SquareUnio -> &SquareUnion
				new Transition (8083, 8084), // &ssetm -> &ssetmn
				new Transition (8106, 8124), // &str -> &strn
				new Transition (8117, 8118), // &straightepsilo -> &straightepsilon
				new Transition (8130, 8279), // &su -> &sun
				new Transition (8131, 8150), // &sub -> &subn
				new Transition (8171, 8184), // &subset -> &subsetn
				new Transition (8199, 8249), // &succ -> &succn
				new Transition (8231, 8232), // &SucceedsSla -> &SucceedsSlan
				new Transition (8284, 8338), // &sup -> &supn
				new Transition (8354, 8361), // &supset -> &supsetn
				new Transition (8375, 8390), // &sw -> &swn
				new Transition (8422, 8423), // &Tcaro -> &Tcaron
				new Transition (8428, 8429), // &tcaro -> &tcaron
				new Transition (8493, 8516), // &thi -> &thin
				new Transition (8507, 8520), // &Thi -> &Thin
				new Transition (8541, 8542), // &thor -> &thorn
				new Transition (8549, 8587), // &ti -> &tin
				new Transition (8634, 8635), // &tria -> &trian
				new Transition (8642, 8643), // &triangledow -> &triangledown
				new Transition (8671, 8672), // &trimi -> &trimin
				new Transition (8768, 8916), // &U -> &Un
				new Transition (8890, 8891), // &ulcor -> &ulcorn
				new Transition (8936, 8937), // &UnderPare -> &UnderParen
				new Transition (8946, 8947), // &Unio -> &Union
				new Transition (8956, 8957), // &Uogo -> &Uogon
				new Transition (8961, 8962), // &uogo -> &uogon
				new Transition (8996, 8997), // &UpArrowDow -> &UpArrowDown
				new Transition (9006, 9007), // &UpDow -> &UpDown
				new Transition (9016, 9017), // &Updow -> &Updown
				new Transition (9026, 9027), // &updow -> &updown
				new Transition (9051, 9052), // &upharpoo -> &upharpoon
				new Transition (9101, 9102), // &Upsilo -> &Upsilon
				new Transition (9105, 9106), // &upsilo -> &upsilon
				new Transition (9130, 9131), // &urcor -> &urcorn
				new Transition (9141, 9142), // &Uri -> &Urin
				new Transition (9145, 9146), // &uri -> &urin
				new Transition (9195, 9196), // &uwa -> &uwan
				new Transition (9201, 9425), // &v -> &vn
				new Transition (9202, 9203), // &va -> &van
				new Transition (9208, 9223), // &var -> &varn
				new Transition (9214, 9215), // &varepsilo -> &varepsilon
				new Transition (9227, 9228), // &varnothi -> &varnothin
				new Transition (9262, 9263), // &varsubset -> &varsubsetn
				new Transition (9272, 9273), // &varsupset -> &varsupsetn
				new Transition (9287, 9288), // &vartria -> &vartrian
				new Transition (9383, 9384), // &VerticalLi -> &VerticalLin
				new Transition (9406, 9407), // &VeryThi -> &VeryThin
				new Transition (9459, 9460), // &vsub -> &vsubn
				new Transition (9465, 9466), // &vsup -> &vsupn
				new Transition (9548, 9598), // &x -> &xn
				new Transition (9699, 9700), // &ye -> &yen
				new Transition (9764, 9765), // &Zcaro -> &Zcaron
				new Transition (9770, 9771), // &zcaro -> &zcaron
				new Transition (9848, 9851) // &zw -> &zwn
			};
			TransitionTable_o = new Transition[460] {
				new Transition (0, 6138), // & -> &o
				new Transition (1, 183), // &A -> &Ao
				new Transition (8, 188), // &a -> &ao
				new Transition (129, 130), // &andsl -> &andslo
				new Transition (184, 185), // &Aog -> &Aogo
				new Transition (189, 190), // &aog -> &aogo
				new Transition (199, 213), // &ap -> &apo
				new Transition (225, 226), // &ApplyFuncti -> &ApplyFunctio
				new Transition (230, 231), // &appr -> &appro
				new Transition (290, 291), // &awc -> &awco
				new Transition (301, 598), // &b -> &bo
				new Transition (305, 306), // &backc -> &backco
				new Transition (314, 315), // &backepsil -> &backepsilo
				new Transition (331, 594), // &B -> &Bo
				new Transition (369, 370), // &bc -> &bco
				new Transition (381, 382), // &bdqu -> &bdquo
				new Transition (410, 411), // &bern -> &berno
				new Transition (415, 416), // &Bern -> &Berno
				new Transition (443, 455), // &big -> &bigo
				new Transition (456, 457), // &bigod -> &bigodo
				new Transition (489, 490), // &bigtriangled -> &bigtriangledo
				new Transition (515, 516), // &bkar -> &bkaro
				new Transition (519, 575), // &bl -> &blo
				new Transition (523, 524), // &blackl -> &blacklo
				new Transition (547, 548), // &blacktriangled -> &blacktriangledo
				new Transition (579, 591), // &bn -> &bno
				new Transition (587, 588), // &bN -> &bNo
				new Transition (604, 605), // &bott -> &botto
				new Transition (614, 615), // &boxb -> &boxbo
				new Transition (744, 757), // &bs -> &bso
				new Transition (789, 1126), // &C -> &Co
				new Transition (796, 1131), // &c -> &co
				new Transition (824, 825), // &capd -> &capdo
				new Transition (848, 852), // &car -> &caro
				new Transition (866, 894), // &Cc -> &Cco
				new Transition (868, 869), // &Ccar -> &Ccaro
				new Transition (872, 873), // &ccar -> &ccaro
				new Transition (907, 908), // &Cd -> &Cdo
				new Transition (911, 912), // &cd -> &cdo
				new Transition (940, 941), // &CenterD -> &CenterDo
				new Transition (946, 947), // &centerd -> &centerdo
				new Transition (990, 991), // &circlearr -> &circlearro
				new Transition (1024, 1025), // &CircleD -> &CircleDo
				new Transition (1068, 1069), // &Cl -> &Clo
				new Transition (1076, 1077), // &ClockwiseC -> &ClockwiseCo
				new Transition (1079, 1080), // &ClockwiseCont -> &ClockwiseConto
				new Transition (1099, 1100), // &CloseCurlyD -> &CloseCurlyDo
				new Transition (1106, 1107), // &CloseCurlyDoubleQu -> &CloseCurlyDoubleQuo
				new Transition (1112, 1113), // &CloseCurlyQu -> &CloseCurlyQuo
				new Transition (1127, 1128), // &Col -> &Colo
				new Transition (1132, 1133), // &col -> &colo
				new Transition (1167, 1168), // &congd -> &congdo
				new Transition (1187, 1188), // &Cont -> &Conto
				new Transition (1206, 1207), // &copr -> &copro
				new Transition (1210, 1211), // &Copr -> &Copro
				new Transition (1232, 1233), // &CounterCl -> &CounterClo
				new Transition (1240, 1241), // &CounterClockwiseC -> &CounterClockwiseCo
				new Transition (1243, 1244), // &CounterClockwiseCont -> &CounterClockwiseConto
				new Transition (1256, 1266), // &cr -> &cro
				new Transition (1261, 1262), // &Cr -> &Cro
				new Transition (1288, 1289), // &ctd -> &ctdo
				new Transition (1318, 1341), // &cup -> &cupo
				new Transition (1337, 1338), // &cupd -> &cupdo
				new Transition (1385, 1386), // &curvearr -> &curvearro
				new Transition (1408, 1409), // &cwc -> &cwco
				new Transition (1425, 1685), // &D -> &Do
				new Transition (1432, 1679), // &d -> &do
				new Transition (1466, 1467), // &dbkar -> &dbkaro
				new Transition (1476, 1477), // &Dcar -> &Dcaro
				new Transition (1482, 1483), // &dcar -> &dcaro
				new Transition (1490, 1503), // &DD -> &DDo
				new Transition (1492, 1510), // &dd -> &ddo
				new Transition (1573, 1574), // &DiacriticalD -> &DiacriticalDo
				new Transition (1601, 1608), // &diam -> &diamo
				new Transition (1603, 1604), // &Diam -> &Diamo
				new Transition (1643, 1657), // &div -> &divo
				new Transition (1647, 1649), // &divide -> &divideo
				new Transition (1670, 1671), // &dlc -> &dlco
				new Transition (1675, 1676), // &dlcr -> &dlcro
				new Transition (1696, 1697), // &DotD -> &DotDo
				new Transition (1703, 1704), // &doteqd -> &doteqdo
				new Transition (1748, 1749), // &DoubleC -> &DoubleCo
				new Transition (1751, 1752), // &DoubleCont -> &DoubleConto
				new Transition (1764, 1765), // &DoubleD -> &DoubleDo
				new Transition (1772, 1773), // &DoubleDownArr -> &DoubleDownArro
				new Transition (1776, 1801), // &DoubleL -> &DoubleLo
				new Transition (1782, 1783), // &DoubleLeftArr -> &DoubleLeftArro
				new Transition (1793, 1794), // &DoubleLeftRightArr -> &DoubleLeftRightArro
				new Transition (1810, 1811), // &DoubleLongLeftArr -> &DoubleLongLeftArro
				new Transition (1821, 1822), // &DoubleLongLeftRightArr -> &DoubleLongLeftRightArro
				new Transition (1832, 1833), // &DoubleLongRightArr -> &DoubleLongRightArro
				new Transition (1843, 1844), // &DoubleRightArr -> &DoubleRightArro
				new Transition (1855, 1856), // &DoubleUpArr -> &DoubleUpArro
				new Transition (1859, 1860), // &DoubleUpD -> &DoubleUpDo
				new Transition (1865, 1866), // &DoubleUpDownArr -> &DoubleUpDownArro
				new Transition (1885, 1886), // &DownArr -> &DownArro
				new Transition (1891, 1892), // &Downarr -> &Downarro
				new Transition (1899, 1900), // &downarr -> &downarro
				new Transition (1911, 1912), // &DownArrowUpArr -> &DownArrowUpArro
				new Transition (1921, 1922), // &downd -> &downdo
				new Transition (1927, 1928), // &downdownarr -> &downdownarro
				new Transition (1935, 1936), // &downharp -> &downharpo
				new Transition (1936, 1937), // &downharpo -> &downharpoo
				new Transition (1962, 1963), // &DownLeftRightVect -> &DownLeftRightVecto
				new Transition (1972, 1973), // &DownLeftTeeVect -> &DownLeftTeeVecto
				new Transition (1979, 1980), // &DownLeftVect -> &DownLeftVecto
				new Transition (1998, 1999), // &DownRightTeeVect -> &DownRightTeeVecto
				new Transition (2005, 2006), // &DownRightVect -> &DownRightVecto
				new Transition (2019, 2020), // &DownTeeArr -> &DownTeeArro
				new Transition (2027, 2028), // &drbkar -> &drbkaro
				new Transition (2031, 2032), // &drc -> &drco
				new Transition (2036, 2037), // &drcr -> &drcro
				new Transition (2044, 2054), // &ds -> &dso
				new Transition (2058, 2059), // &Dstr -> &Dstro
				new Transition (2063, 2064), // &dstr -> &dstro
				new Transition (2068, 2069), // &dtd -> &dtdo
				new Transition (2108, 2296), // &E -> &Eo
				new Transition (2115, 2301), // &e -> &eo
				new Transition (2129, 2130), // &Ecar -> &Ecaro
				new Transition (2133, 2148), // &ec -> &eco
				new Transition (2135, 2136), // &ecar -> &ecaro
				new Transition (2149, 2150), // &ecol -> &ecolo
				new Transition (2157, 2166), // &eD -> &eDo
				new Transition (2158, 2159), // &eDD -> &eDDo
				new Transition (2162, 2163), // &Ed -> &Edo
				new Transition (2169, 2170), // &ed -> &edo
				new Transition (2176, 2177), // &efD -> &efDo
				new Transition (2200, 2201), // &egsd -> &egsdo
				new Transition (2224, 2225), // &elsd -> &elsdo
				new Transition (2297, 2298), // &Eog -> &Eogo
				new Transition (2302, 2303), // &eog -> &eogo
				new Transition (2329, 2330), // &Epsil -> &Epsilo
				new Transition (2333, 2334), // &epsil -> &epsilo
				new Transition (2340, 2345), // &eqc -> &eqco
				new Transition (2346, 2347), // &eqcol -> &eqcolo
				new Transition (2414, 2415), // &erD -> &erDo
				new Transition (2426, 2427), // &esd -> &esdo
				new Transition (2455, 2456), // &eur -> &euro
				new Transition (2472, 2493), // &exp -> &expo
				new Transition (2478, 2479), // &expectati -> &expectatio
				new Transition (2482, 2483), // &Exp -> &Expo
				new Transition (2503, 2612), // &f -> &fo
				new Transition (2510, 2511), // &fallingd -> &fallingdo
				new Transition (2517, 2608), // &F -> &Fo
				new Transition (2604, 2605), // &fn -> &fno
				new Transition (2647, 2689), // &fr -> &fro
				new Transition (2701, 2857), // &g -> &go
				new Transition (2708, 2853), // &G -> &Go
				new Transition (2755, 2756), // &Gd -> &Gdo
				new Transition (2759, 2760), // &gd -> &gdo
				new Transition (2786, 2787), // &gesd -> &gesdo
				new Transition (2788, 2790), // &gesdot -> &gesdoto
				new Transition (2837, 2838), // &gnappr -> &gnappro
				new Transition (2950, 2951), // &gtd -> &gtdo
				new Transition (2969, 2970), // &gtrappr -> &gtrappro
				new Transition (2976, 2977), // &gtrd -> &gtrdo
				new Transition (3014, 3159), // &H -> &Ho
				new Transition (3020, 3126), // &h -> &ho
				new Transition (3090, 3091), // &herc -> &herco
				new Transition (3116, 3117), // &hksear -> &hksearo
				new Transition (3122, 3123), // &hkswar -> &hkswaro
				new Transition (3126, 3136), // &ho -> &hoo
				new Transition (3144, 3145), // &hookleftarr -> &hookleftarro
				new Transition (3155, 3156), // &hookrightarr -> &hookrightarro
				new Transition (3173, 3174), // &Horiz -> &Horizo
				new Transition (3198, 3199), // &Hstr -> &Hstro
				new Transition (3203, 3204), // &hstr -> &hstro
				new Transition (3210, 3211), // &HumpD -> &HumpDo
				new Transition (3236, 3471), // &I -> &Io
				new Transition (3243, 3467), // &i -> &io
				new Transition (3265, 3266), // &Id -> &Ido
				new Transition (3301, 3316), // &ii -> &iio
				new Transition (3336, 3365), // &im -> &imo
				new Transition (3378, 3393), // &in -> &ino
				new Transition (3394, 3395), // &inod -> &inodo
				new Transition (3429, 3430), // &Intersecti -> &Intersectio
				new Transition (3440, 3441), // &intpr -> &intpro
				new Transition (3451, 3452), // &InvisibleC -> &InvisibleCo
				new Transition (3472, 3473), // &Iog -> &Iogo
				new Transition (3476, 3477), // &iog -> &iogo
				new Transition (3493, 3494), // &ipr -> &ipro
				new Transition (3514, 3515), // &isind -> &isindo
				new Transition (3555, 3582), // &J -> &Jo
				new Transition (3561, 3586), // &j -> &jo
				new Transition (3618, 3676), // &K -> &Ko
				new Transition (3624, 3680), // &k -> &ko
				new Transition (3692, 4422), // &l -> &lo
				new Transition (3698, 4434), // &L -> &Lo
				new Transition (3756, 3757), // &laqu -> &laquo
				new Transition (3839, 3840), // &Lcar -> &Lcaro
				new Transition (3845, 3846), // &lcar -> &lcaro
				new Transition (3874, 3875), // &ldqu -> &ldquo
				new Transition (3915, 3916), // &LeftArr -> &LeftArro
				new Transition (3921, 3922), // &Leftarr -> &Leftarro
				new Transition (3929, 3930), // &leftarr -> &leftarro
				new Transition (3944, 3945), // &LeftArrowRightArr -> &LeftArrowRightArro
				new Transition (3961, 3962), // &LeftD -> &LeftDo
				new Transition (3983, 3984), // &LeftDownTeeVect -> &LeftDownTeeVecto
				new Transition (3990, 3991), // &LeftDownVect -> &LeftDownVecto
				new Transition (3999, 4000), // &LeftFl -> &LeftFlo
				new Transition (4000, 4001), // &LeftFlo -> &LeftFloo
				new Transition (4007, 4008), // &leftharp -> &leftharpo
				new Transition (4008, 4009), // &leftharpo -> &leftharpoo
				new Transition (4011, 4012), // &leftharpoond -> &leftharpoondo
				new Transition (4025, 4026), // &leftleftarr -> &leftleftarro
				new Transition (4037, 4038), // &LeftRightArr -> &LeftRightArro
				new Transition (4048, 4049), // &Leftrightarr -> &Leftrightarro
				new Transition (4059, 4060), // &leftrightarr -> &leftrightarro
				new Transition (4068, 4069), // &leftrightharp -> &leftrightharpo
				new Transition (4069, 4070), // &leftrightharpo -> &leftrightharpoo
				new Transition (4081, 4082), // &leftrightsquigarr -> &leftrightsquigarro
				new Transition (4088, 4089), // &LeftRightVect -> &LeftRightVecto
				new Transition (4098, 4099), // &LeftTeeArr -> &LeftTeeArro
				new Transition (4105, 4106), // &LeftTeeVect -> &LeftTeeVecto
				new Transition (4140, 4141), // &LeftUpD -> &LeftUpDo
				new Transition (4147, 4148), // &LeftUpDownVect -> &LeftUpDownVecto
				new Transition (4157, 4158), // &LeftUpTeeVect -> &LeftUpTeeVecto
				new Transition (4164, 4165), // &LeftUpVect -> &LeftUpVecto
				new Transition (4175, 4176), // &LeftVect -> &LeftVecto
				new Transition (4202, 4203), // &lesd -> &lesdo
				new Transition (4204, 4206), // &lesdot -> &lesdoto
				new Transition (4219, 4220), // &lessappr -> &lessappro
				new Transition (4223, 4224), // &lessd -> &lessdo
				new Transition (4307, 4308), // &lfl -> &lflo
				new Transition (4308, 4309), // &lflo -> &lfloo
				new Transition (4354, 4355), // &llc -> &llco
				new Transition (4366, 4367), // &Lleftarr -> &Lleftarro
				new Transition (4381, 4382), // &Lmid -> &Lmido
				new Transition (4385, 4391), // &lm -> &lmo
				new Transition (4387, 4388), // &lmid -> &lmido
				new Transition (4406, 4407), // &lnappr -> &lnappro
				new Transition (4422, 4542), // &lo -> &loo
				new Transition (4443, 4444), // &LongLeftArr -> &LongLeftArro
				new Transition (4453, 4454), // &Longleftarr -> &Longleftarro
				new Transition (4465, 4466), // &longleftarr -> &longleftarro
				new Transition (4476, 4477), // &LongLeftRightArr -> &LongLeftRightArro
				new Transition (4487, 4488), // &Longleftrightarr -> &Longleftrightarro
				new Transition (4498, 4499), // &longleftrightarr -> &longleftrightarro
				new Transition (4506, 4507), // &longmapst -> &longmapsto
				new Transition (4516, 4517), // &LongRightArr -> &LongRightArro
				new Transition (4527, 4528), // &Longrightarr -> &Longrightarro
				new Transition (4538, 4539), // &longrightarr -> &longrightarro
				new Transition (4546, 4547), // &looparr -> &looparro
				new Transition (4597, 4598), // &LowerLeftArr -> &LowerLeftArro
				new Transition (4608, 4609), // &LowerRightArr -> &LowerRightArro
				new Transition (4633, 4634), // &lrc -> &lrco
				new Transition (4655, 4656), // &lsaqu -> &lsaquo
				new Transition (4679, 4680), // &lsqu -> &lsquo
				new Transition (4685, 4686), // &Lstr -> &Lstro
				new Transition (4690, 4691), // &lstr -> &lstro
				new Transition (4706, 4707), // &ltd -> &ltdo
				new Transition (4767, 4922), // &m -> &mo
				new Transition (4781, 4928), // &M -> &Mo
				new Transition (4788, 4789), // &mapst -> &mapsto
				new Transition (4791, 4792), // &mapstod -> &mapstodo
				new Transition (4809, 4810), // &mc -> &mco
				new Transition (4826, 4827), // &mDD -> &mDDo
				new Transition (4868, 4869), // &mh -> &mho
				new Transition (4873, 4874), // &micr -> &micro
				new Transition (4886, 4887), // &midd -> &middo
				new Transition (4946, 4947), // &mstp -> &mstpo
				new Transition (4965, 5372), // &n -> &no
				new Transition (4971, 5347), // &N -> &No
				new Transition (4986, 4993), // &nap -> &napo
				new Transition (4997, 4998), // &nappr -> &nappro
				new Transition (5020, 5044), // &nc -> &nco
				new Transition (5026, 5027), // &Ncar -> &Ncaro
				new Transition (5030, 5031), // &ncar -> &ncaro
				new Transition (5048, 5049), // &ncongd -> &ncongdo
				new Transition (5075, 5077), // &nearr -> &nearro
				new Transition (5080, 5081), // &ned -> &nedo
				new Transition (5278, 5279), // &nLeftarr -> &nLeftarro
				new Transition (5286, 5287), // &nleftarr -> &nleftarro
				new Transition (5297, 5298), // &nLeftrightarr -> &nLeftrightarro
				new Transition (5308, 5309), // &nleftrightarr -> &nleftrightarro
				new Transition (5380, 5381), // &NotC -> &NotCo
				new Transition (5396, 5397), // &NotD -> &NotDo
				new Transition (5497, 5498), // &NotHumpD -> &NotHumpDo
				new Transition (5515, 5516), // &notind -> &notindo
				new Transition (5821, 5836), // &np -> &npo
				new Transition (5875, 5876), // &nRightarr -> &nRightarro
				new Transition (5885, 5886), // &nrightarr -> &nrightarro
				new Transition (5910, 5911), // &nsh -> &nsho
				new Transition (6037, 6038), // &numer -> &numero
				new Transition (6121, 6123), // &nwarr -> &nwarro
				new Transition (6131, 6294), // &O -> &Oo
				new Transition (6138, 6298), // &o -> &oo
				new Transition (6163, 6182), // &od -> &odo
				new Transition (6185, 6186), // &ods -> &odso
				new Transition (6210, 6211), // &og -> &ogo
				new Transition (6247, 6248), // &olcr -> &olcro
				new Transition (6278, 6279), // &Omicr -> &Omicro
				new Transition (6284, 6285), // &omicr -> &omicro
				new Transition (6314, 6315), // &OpenCurlyD -> &OpenCurlyDo
				new Transition (6321, 6322), // &OpenCurlyDoubleQu -> &OpenCurlyDoubleQuo
				new Transition (6327, 6328), // &OpenCurlyQu -> &OpenCurlyQuo
				new Transition (6342, 6365), // &or -> &oro
				new Transition (6351, 6353), // &order -> &ordero
				new Transition (6361, 6362), // &orig -> &origo
				new Transition (6369, 6370), // &orsl -> &orslo
				new Transition (6382, 6396), // &os -> &oso
				new Transition (6463, 6622), // &p -> &po
				new Transition (6482, 6609), // &P -> &Po
				new Transition (6503, 6504), // &peri -> &perio
				new Transition (6527, 6537), // &ph -> &pho
				new Transition (6548, 6549), // &pitchf -> &pitchfo
				new Transition (6580, 6581), // &plusd -> &plusdo
				new Transition (6604, 6605), // &plustw -> &plustwo
				new Transition (6640, 6748), // &Pr -> &Pro
				new Transition (6642, 6745), // &pr -> &pro
				new Transition (6660, 6661), // &precappr -> &precappro
				new Transition (6709, 6710), // &precnappr -> &precnappro
				new Transition (6772, 6773), // &Prop -> &Propo
				new Transition (6776, 6777), // &Proporti -> &Proportio
				new Transition (6783, 6784), // &propt -> &propto
				new Transition (6813, 6825), // &Q -> &Qo
				new Transition (6817, 6829), // &q -> &qo
				new Transition (6847, 6873), // &qu -> &quo
				new Transition (6853, 6854), // &quaterni -> &quaternio
				new Transition (6876, 7469), // &r -> &ro
				new Transition (6886, 7485), // &R -> &Ro
				new Transition (6922, 6923), // &raqu -> &raquo
				new Transition (6978, 6979), // &rati -> &ratio
				new Transition (7023, 7024), // &Rcar -> &Rcaro
				new Transition (7029, 7030), // &rcar -> &rcaro
				new Transition (7064, 7065), // &rdqu -> &rdquo
				new Transition (7141, 7142), // &rfl -> &rflo
				new Transition (7142, 7143), // &rflo -> &rfloo
				new Transition (7155, 7167), // &rh -> &rho
				new Transition (7164, 7165), // &Rh -> &Rho
				new Transition (7189, 7190), // &RightArr -> &RightArro
				new Transition (7195, 7196), // &Rightarr -> &Rightarro
				new Transition (7205, 7206), // &rightarr -> &rightarro
				new Transition (7219, 7220), // &RightArrowLeftArr -> &RightArrowLeftArro
				new Transition (7236, 7237), // &RightD -> &RightDo
				new Transition (7258, 7259), // &RightDownTeeVect -> &RightDownTeeVecto
				new Transition (7265, 7266), // &RightDownVect -> &RightDownVecto
				new Transition (7274, 7275), // &RightFl -> &RightFlo
				new Transition (7275, 7276), // &RightFlo -> &RightFloo
				new Transition (7282, 7283), // &rightharp -> &rightharpo
				new Transition (7283, 7284), // &rightharpo -> &rightharpoo
				new Transition (7286, 7287), // &rightharpoond -> &rightharpoondo
				new Transition (7300, 7301), // &rightleftarr -> &rightleftarro
				new Transition (7308, 7309), // &rightleftharp -> &rightleftharpo
				new Transition (7309, 7310), // &rightleftharpo -> &rightleftharpoo
				new Transition (7321, 7322), // &rightrightarr -> &rightrightarro
				new Transition (7333, 7334), // &rightsquigarr -> &rightsquigarro
				new Transition (7343, 7344), // &RightTeeArr -> &RightTeeArro
				new Transition (7350, 7351), // &RightTeeVect -> &RightTeeVecto
				new Transition (7385, 7386), // &RightUpD -> &RightUpDo
				new Transition (7392, 7393), // &RightUpDownVect -> &RightUpDownVecto
				new Transition (7402, 7403), // &RightUpTeeVect -> &RightUpTeeVecto
				new Transition (7409, 7410), // &RightUpVect -> &RightUpVecto
				new Transition (7420, 7421), // &RightVect -> &RightVecto
				new Transition (7435, 7436), // &risingd -> &risingdo
				new Transition (7453, 7454), // &rm -> &rmo
				new Transition (7519, 7520), // &rpp -> &rppo
				new Transition (7538, 7539), // &Rrightarr -> &Rrightarro
				new Transition (7545, 7546), // &rsaqu -> &rsaquo
				new Transition (7562, 7563), // &rsqu -> &rsquo
				new Transition (7610, 7949), // &S -> &So
				new Transition (7617, 7936), // &s -> &so
				new Transition (7626, 7627), // &sbqu -> &sbquo
				new Transition (7637, 7638), // &Scar -> &Scaro
				new Transition (7641, 7642), // &scar -> &scaro
				new Transition (7680, 7681), // &scp -> &scpo
				new Transition (7695, 7696), // &sd -> &sdo
				new Transition (7713, 7715), // &searr -> &searro
				new Transition (7745, 7747), // &sfr -> &sfro
				new Transition (7751, 7796), // &sh -> &sho
				new Transition (7772, 7773), // &Sh -> &Sho
				new Transition (7776, 7777), // &ShortD -> &ShortDo
				new Transition (7782, 7783), // &ShortDownArr -> &ShortDownArro
				new Transition (7792, 7793), // &ShortLeftArr -> &ShortLeftArro
				new Transition (7819, 7820), // &ShortRightArr -> &ShortRightArro
				new Transition (7827, 7828), // &ShortUpArr -> &ShortUpArro
				new Transition (7849, 7850), // &simd -> &simdo
				new Transition (8028, 8029), // &SquareIntersecti -> &SquareIntersectio
				new Transition (8060, 8061), // &SquareUni -> &SquareUnio
				new Transition (8116, 8117), // &straightepsil -> &straightepsilo
				new Transition (8133, 8134), // &subd -> &subdo
				new Transition (8141, 8142), // &subed -> &subedo
				new Transition (8204, 8205), // &succappr -> &succappro
				new Transition (8253, 8254), // &succnappr -> &succnappro
				new Transition (8292, 8293), // &supd -> &supdo
				new Transition (8304, 8305), // &suped -> &supedo
				new Transition (8321, 8322), // &suphs -> &suphso
				new Transition (8385, 8387), // &swarr -> &swarro
				new Transition (8400, 8604), // &T -> &To
				new Transition (8404, 8590), // &t -> &to
				new Transition (8421, 8422), // &Tcar -> &Tcaro
				new Transition (8427, 8428), // &tcar -> &tcaro
				new Transition (8445, 8446), // &td -> &tdo
				new Transition (8461, 8540), // &th -> &tho
				new Transition (8471, 8472), // &Theref -> &Therefo
				new Transition (8476, 8477), // &theref -> &therefo
				new Transition (8499, 8500), // &thickappr -> &thickappro
				new Transition (8596, 8597), // &topb -> &topbo
				new Transition (8608, 8610), // &topf -> &topfo
				new Transition (8640, 8641), // &triangled -> &triangledo
				new Transition (8664, 8665), // &trid -> &trido
				new Transition (8681, 8682), // &TripleD -> &TripleDo
				new Transition (8728, 8729), // &Tstr -> &Tstro
				new Transition (8733, 8734), // &tstr -> &tstro
				new Transition (8737, 8742), // &tw -> &two
				new Transition (8753, 8754), // &twoheadleftarr -> &twoheadleftarro
				new Transition (8764, 8765), // &twoheadrightarr -> &twoheadrightarro
				new Transition (8768, 8954), // &U -> &Uo
				new Transition (8775, 8959), // &u -> &uo
				new Transition (8783, 8792), // &Uarr -> &Uarro
				new Transition (8888, 8889), // &ulc -> &ulco
				new Transition (8896, 8897), // &ulcr -> &ulcro
				new Transition (8945, 8946), // &Uni -> &Unio
				new Transition (8955, 8956), // &Uog -> &Uogo
				new Transition (8960, 8961), // &uog -> &uogo
				new Transition (8973, 8974), // &UpArr -> &UpArro
				new Transition (8979, 8980), // &Uparr -> &Uparro
				new Transition (8986, 8987), // &uparr -> &uparro
				new Transition (8994, 8995), // &UpArrowD -> &UpArrowDo
				new Transition (9000, 9001), // &UpArrowDownArr -> &UpArrowDownArro
				new Transition (9004, 9005), // &UpD -> &UpDo
				new Transition (9010, 9011), // &UpDownArr -> &UpDownArro
				new Transition (9014, 9015), // &Upd -> &Updo
				new Transition (9020, 9021), // &Updownarr -> &Updownarro
				new Transition (9024, 9025), // &upd -> &updo
				new Transition (9030, 9031), // &updownarr -> &updownarro
				new Transition (9049, 9050), // &upharp -> &upharpo
				new Transition (9050, 9051), // &upharpo -> &upharpoo
				new Transition (9077, 9078), // &UpperLeftArr -> &UpperLeftArro
				new Transition (9088, 9089), // &UpperRightArr -> &UpperRightArro
				new Transition (9100, 9101), // &Upsil -> &Upsilo
				new Transition (9104, 9105), // &upsil -> &upsilo
				new Transition (9114, 9115), // &UpTeeArr -> &UpTeeArro
				new Transition (9122, 9123), // &upuparr -> &upuparro
				new Transition (9128, 9129), // &urc -> &urco
				new Transition (9136, 9137), // &urcr -> &urcro
				new Transition (9162, 9163), // &utd -> &utdo
				new Transition (9201, 9436), // &v -> &vo
				new Transition (9213, 9214), // &varepsil -> &varepsilo
				new Transition (9223, 9224), // &varn -> &varno
				new Transition (9237, 9238), // &varpr -> &varpro
				new Transition (9240, 9241), // &varpropt -> &varpropto
				new Transition (9249, 9250), // &varrh -> &varrho
				new Transition (9303, 9432), // &V -> &Vo
				new Transition (9393, 9394), // &VerticalSeparat -> &VerticalSeparato
				new Transition (9441, 9442), // &vpr -> &vpro
				new Transition (9484, 9523), // &W -> &Wo
				new Transition (9490, 9527), // &w -> &wo
				new Transition (9548, 9602), // &x -> &xo
				new Transition (9565, 9607), // &X -> &Xo
				new Transition (9603, 9604), // &xod -> &xodo
				new Transition (9665, 9716), // &Y -> &Yo
				new Transition (9672, 9720), // &y -> &yo
				new Transition (9747, 9832), // &Z -> &Zo
				new Transition (9754, 9836), // &z -> &zo
				new Transition (9763, 9764), // &Zcar -> &Zcaro
				new Transition (9769, 9770), // &zcar -> &zcaro
				new Transition (9777, 9778), // &Zd -> &Zdo
				new Transition (9781, 9782), // &zd -> &zdo
				new Transition (9792, 9793) // &Zer -> &Zero
			};
			TransitionTable_p = new Transition[278] {
				new Transition (0, 6463), // & -> &p
				new Transition (1, 216), // &A -> &Ap
				new Transition (8, 199), // &a -> &ap
				new Transition (79, 94), // &al -> &alp
				new Transition (80, 86), // &ale -> &alep
				new Transition (89, 90), // &Al -> &Alp
				new Transition (103, 114), // &am -> &amp
				new Transition (130, 131), // &andslo -> &andslop
				new Transition (172, 173), // &angs -> &angsp
				new Transition (183, 193), // &Ao -> &Aop
				new Transition (188, 196), // &ao -> &aop
				new Transition (199, 229), // &ap -> &app
				new Transition (216, 217), // &Ap -> &App
				new Transition (263, 264), // &asym -> &asymp
				new Transition (301, 719), // &b -> &bp
				new Transition (304, 318), // &back -> &backp
				new Transition (310, 311), // &backe -> &backep
				new Transition (384, 405), // &be -> &bep
				new Transition (399, 400), // &bem -> &bemp
				new Transition (445, 446), // &bigca -> &bigcap
				new Transition (452, 453), // &bigcu -> &bigcup
				new Transition (455, 460), // &bigo -> &bigop
				new Transition (474, 475), // &bigsqcu -> &bigsqcup
				new Transition (494, 495), // &bigtriangleu -> &bigtriangleup
				new Transition (497, 498), // &bigu -> &bigup
				new Transition (594, 595), // &Bo -> &Bop
				new Transition (598, 599), // &bo -> &bop
				new Transition (613, 662), // &box -> &boxp
				new Transition (774, 775), // &bum -> &bump
				new Transition (782, 783), // &Bum -> &Bump
				new Transition (790, 803), // &Ca -> &Cap
				new Transition (797, 805), // &ca -> &cap
				new Transition (814, 815), // &capbrcu -> &capbrcup
				new Transition (818, 819), // &capca -> &capcap
				new Transition (821, 822), // &capcu -> &capcup
				new Transition (862, 863), // &cca -> &ccap
				new Transition (900, 901), // &ccu -> &ccup
				new Transition (927, 928), // &cem -> &cemp
				new Transition (1126, 1200), // &Co -> &Cop
				new Transition (1131, 1203), // &co -> &cop
				new Transition (1142, 1148), // &com -> &comp
				new Transition (1278, 1283), // &csu -> &csup
				new Transition (1292, 1318), // &cu -> &cup
				new Transition (1301, 1302), // &cue -> &cuep
				new Transition (1311, 1313), // &cularr -> &cularrp
				new Transition (1315, 1316), // &Cu -> &Cup
				new Transition (1323, 1324), // &cupbrca -> &cupbrcap
				new Transition (1327, 1328), // &CupCa -> &CupCap
				new Transition (1331, 1332), // &cupca -> &cupcap
				new Transition (1334, 1335), // &cupcu -> &cupcup
				new Transition (1356, 1357), // &curlyeq -> &curlyeqp
				new Transition (1529, 1530), // &dem -> &demp
				new Transition (1676, 1677), // &dlcro -> &dlcrop
				new Transition (1679, 1689), // &do -> &dop
				new Transition (1685, 1686), // &Do -> &Dop
				new Transition (1694, 1719), // &dot -> &dotp
				new Transition (1851, 1852), // &DoubleU -> &DoubleUp
				new Transition (1907, 1908), // &DownArrowU -> &DownArrowUp
				new Transition (1934, 1935), // &downhar -> &downharp
				new Transition (2037, 2038), // &drcro -> &drcrop
				new Transition (2108, 2326), // &E -> &Ep
				new Transition (2115, 2312), // &e -> &ep
				new Transition (2228, 2246), // &Em -> &Emp
				new Transition (2233, 2238), // &em -> &emp
				new Transition (2279, 2280), // &ems -> &emsp
				new Transition (2293, 2294), // &ens -> &ensp
				new Transition (2296, 2306), // &Eo -> &Eop
				new Transition (2301, 2309), // &eo -> &eop
				new Transition (2402, 2403), // &eqv -> &eqvp
				new Transition (2458, 2472), // &ex -> &exp
				new Transition (2466, 2482), // &Ex -> &Exp
				new Transition (2503, 2639), // &f -> &fp
				new Transition (2608, 2609), // &Fo -> &Fop
				new Transition (2612, 2613), // &fo -> &fop
				new Transition (2702, 2722), // &ga -> &gap
				new Transition (2833, 2834), // &gna -> &gnap
				new Transition (2834, 2836), // &gnap -> &gnapp
				new Transition (2853, 2854), // &Go -> &Gop
				new Transition (2857, 2858), // &go -> &gop
				new Transition (2966, 2967), // &gtra -> &gtrap
				new Transition (2967, 2968), // &gtrap -> &gtrapp
				new Transition (3024, 3025), // &hairs -> &hairsp
				new Transition (3086, 3087), // &helli -> &hellip
				new Transition (3106, 3107), // &HilbertS -> &HilbertSp
				new Transition (3126, 3163), // &ho -> &hop
				new Transition (3159, 3160), // &Ho -> &Hop
				new Transition (3208, 3209), // &Hum -> &Hump
				new Transition (3216, 3217), // &HumpDownHum -> &HumpDownHump
				new Transition (3225, 3231), // &hy -> &hyp
				new Transition (3243, 3492), // &i -> &ip
				new Transition (3330, 3372), // &Im -> &Imp
				new Transition (3336, 3368), // &im -> &imp
				new Transition (3341, 3357), // &imag -> &imagp
				new Transition (3401, 3439), // &int -> &intp
				new Transition (3467, 3483), // &io -> &iop
				new Transition (3471, 3480), // &Io -> &Iop
				new Transition (3582, 3583), // &Jo -> &Jop
				new Transition (3586, 3587), // &jo -> &jop
				new Transition (3619, 3620), // &Ka -> &Kap
				new Transition (3620, 3621), // &Kap -> &Kapp
				new Transition (3625, 3626), // &ka -> &kap
				new Transition (3626, 3627), // &kap -> &kapp
				new Transition (3676, 3677), // &Ko -> &Kop
				new Transition (3680, 3681), // &ko -> &kop
				new Transition (3692, 4621), // &l -> &lp
				new Transition (3699, 3746), // &La -> &Lap
				new Transition (3705, 3744), // &la -> &lap
				new Transition (3712, 3713), // &laem -> &laemp
				new Transition (3766, 3782), // &larr -> &larrp
				new Transition (3779, 3780), // &larrl -> &larrlp
				new Transition (4006, 4007), // &lefthar -> &leftharp
				new Transition (4016, 4017), // &leftharpoonu -> &leftharpoonup
				new Transition (4067, 4068), // &leftrighthar -> &leftrightharp
				new Transition (4138, 4139), // &LeftU -> &LeftUp
				new Transition (4216, 4217), // &lessa -> &lessap
				new Transition (4217, 4218), // &lessap -> &lessapp
				new Transition (4402, 4403), // &lna -> &lnap
				new Transition (4403, 4405), // &lnap -> &lnapp
				new Transition (4422, 4560), // &lo -> &lop
				new Transition (4434, 4564), // &Lo -> &Lop
				new Transition (4503, 4504), // &longma -> &longmap
				new Transition (4542, 4543), // &loo -> &loop
				new Transition (4767, 4935), // &m -> &mp
				new Transition (4768, 4785), // &ma -> &map
				new Transition (4782, 4783), // &Ma -> &Map
				new Transition (4801, 4802), // &mapstou -> &mapstoup
				new Transition (4848, 4849), // &MediumS -> &MediumSp
				new Transition (4910, 4911), // &mlc -> &mlcp
				new Transition (4916, 4917), // &mn -> &mnp
				new Transition (4922, 4932), // &mo -> &mop
				new Transition (4928, 4929), // &Mo -> &Mop
				new Transition (4945, 4946), // &mst -> &mstp
				new Transition (4958, 4959), // &multima -> &multimap
				new Transition (4962, 4963), // &muma -> &mumap
				new Transition (4965, 5821), // &n -> &np
				new Transition (4966, 4986), // &na -> &nap
				new Transition (4986, 4996), // &nap -> &napp
				new Transition (5011, 5012), // &nbs -> &nbsp
				new Transition (5015, 5016), // &nbum -> &nbump
				new Transition (5021, 5022), // &nca -> &ncap
				new Transition (5052, 5053), // &ncu -> &ncup
				new Transition (5097, 5098), // &NegativeMediumS -> &NegativeMediumSp
				new Transition (5108, 5109), // &NegativeThickS -> &NegativeThickSp
				new Transition (5115, 5116), // &NegativeThinS -> &NegativeThinSp
				new Transition (5129, 5130), // &NegativeVeryThinS -> &NegativeVeryThinSp
				new Transition (5227, 5236), // &nh -> &nhp
				new Transition (5347, 5369), // &No -> &Nop
				new Transition (5363, 5364), // &NonBreakingS -> &NonBreakingSp
				new Transition (5372, 5373), // &no -> &nop
				new Transition (5390, 5391), // &NotCu -> &NotCup
				new Transition (5393, 5394), // &NotCupCa -> &NotCupCap
				new Transition (5495, 5496), // &NotHum -> &NotHump
				new Transition (5503, 5504), // &NotHumpDownHum -> &NotHumpDownHump
				new Transition (5701, 5713), // &NotSquareSu -> &NotSquareSup
				new Transition (5726, 5768), // &NotSu -> &NotSup
				new Transition (5895, 5938), // &ns -> &nsp
				new Transition (5913, 5918), // &nshort -> &nshortp
				new Transition (5944, 5948), // &nsqsu -> &nsqsup
				new Transition (5951, 5973), // &nsu -> &nsup
				new Transition (6040, 6041), // &nums -> &numsp
				new Transition (6044, 6045), // &nva -> &nvap
				new Transition (6131, 6306), // &O -> &Op
				new Transition (6138, 6302), // &o -> &op
				new Transition (6294, 6295), // &Oo -> &Oop
				new Transition (6298, 6299), // &oo -> &oop
				new Transition (6333, 6334), // &oper -> &operp
				new Transition (6370, 6371), // &orslo -> &orslop
				new Transition (6498, 6511), // &per -> &perp
				new Transition (6609, 6630), // &Po -> &Pop
				new Transition (6615, 6616), // &Poincare -> &Poincarep
				new Transition (6622, 6633), // &po -> &pop
				new Transition (6644, 6645), // &pra -> &prap
				new Transition (6657, 6658), // &preca -> &precap
				new Transition (6658, 6659), // &precap -> &precapp
				new Transition (6706, 6707), // &precna -> &precnap
				new Transition (6707, 6708), // &precnap -> &precnapp
				new Transition (6736, 6737), // &prna -> &prnap
				new Transition (6745, 6770), // &pro -> &prop
				new Transition (6748, 6772), // &Pro -> &Prop
				new Transition (6810, 6811), // &puncs -> &puncsp
				new Transition (6817, 6833), // &q -> &qp
				new Transition (6825, 6826), // &Qo -> &Qop
				new Transition (6829, 6830), // &qo -> &qop
				new Transition (6876, 7512), // &r -> &rp
				new Transition (6902, 6903), // &raem -> &raemp
				new Transition (6932, 6953), // &rarr -> &rarrp
				new Transition (6934, 6935), // &rarra -> &rarrap
				new Transition (6950, 6951), // &rarrl -> &rarrlp
				new Transition (7076, 7082), // &real -> &realp
				new Transition (7121, 7122), // &ReverseU -> &ReverseUp
				new Transition (7281, 7282), // &righthar -> &rightharp
				new Transition (7291, 7292), // &rightharpoonu -> &rightharpoonup
				new Transition (7307, 7308), // &rightlefthar -> &rightleftharp
				new Transition (7383, 7384), // &RightU -> &RightUp
				new Transition (7469, 7481), // &ro -> &rop
				new Transition (7485, 7486), // &Ro -> &Rop
				new Transition (7505, 7506), // &RoundIm -> &RoundImp
				new Transition (7512, 7519), // &rp -> &rpp
				new Transition (7617, 7956), // &s -> &sp
				new Transition (7631, 7680), // &sc -> &scp
				new Transition (7633, 7634), // &sca -> &scap
				new Transition (7671, 7672), // &scna -> &scnap
				new Transition (7753, 7754), // &shar -> &sharp
				new Transition (7798, 7803), // &short -> &shortp
				new Transition (7823, 7824), // &ShortU -> &ShortUp
				new Transition (7847, 7868), // &sim -> &simp
				new Transition (7908, 7909), // &smash -> &smashp
				new Transition (7911, 7912), // &sme -> &smep
				new Transition (7936, 7953), // &so -> &sop
				new Transition (7949, 7950), // &So -> &Sop
				new Transition (7970, 7971), // &sqca -> &sqcap
				new Transition (7975, 7976), // &sqcu -> &sqcup
				new Transition (7985, 7997), // &sqsu -> &sqsup
				new Transition (8033, 8045), // &SquareSu -> &SquareSup
				new Transition (8111, 8120), // &straight -> &straightp
				new Transition (8112, 8113), // &straighte -> &straightep
				new Transition (8127, 8282), // &Su -> &Sup
				new Transition (8130, 8284), // &su -> &sup
				new Transition (8131, 8155), // &sub -> &subp
				new Transition (8193, 8196), // &subsu -> &subsup
				new Transition (8201, 8202), // &succa -> &succap
				new Transition (8202, 8203), // &succap -> &succapp
				new Transition (8250, 8251), // &succna -> &succnap
				new Transition (8251, 8252), // &succnap -> &succnapp
				new Transition (8284, 8343), // &sup -> &supp
				new Transition (8370, 8373), // &supsu -> &supsup
				new Transition (8404, 8617), // &t -> &tp
				new Transition (8496, 8497), // &thicka -> &thickap
				new Transition (8497, 8498), // &thickap -> &thickapp
				new Transition (8510, 8511), // &ThickS -> &ThickSp
				new Transition (8517, 8518), // &thins -> &thinsp
				new Transition (8521, 8522), // &ThinS -> &ThinSp
				new Transition (8528, 8529), // &thka -> &thkap
				new Transition (8590, 8594), // &to -> &top
				new Transition (8604, 8605), // &To -> &Top
				new Transition (8628, 8698), // &tr -> &trp
				new Transition (8633, 8685), // &tri -> &trip
				new Transition (8677, 8678), // &Tri -> &Trip
				new Transition (8768, 8970), // &U -> &Up
				new Transition (8775, 8983), // &u -> &up
				new Transition (8897, 8898), // &ulcro -> &ulcrop
				new Transition (8954, 8964), // &Uo -> &Uop
				new Transition (8959, 8967), // &uo -> &uop
				new Transition (8970, 9068), // &Up -> &Upp
				new Transition (9048, 9049), // &uphar -> &upharp
				new Transition (9118, 9119), // &upu -> &upup
				new Transition (9137, 9138), // &urcro -> &urcrop
				new Transition (9201, 9440), // &v -> &vp
				new Transition (9208, 9231), // &var -> &varp
				new Transition (9209, 9210), // &vare -> &varep
				new Transition (9218, 9219), // &varka -> &varkap
				new Transition (9219, 9220), // &varkap -> &varkapp
				new Transition (9238, 9239), // &varpro -> &varprop
				new Transition (9258, 9269), // &varsu -> &varsup
				new Transition (9357, 9358), // &velli -> &vellip
				new Transition (9388, 9389), // &VerticalSe -> &VerticalSep
				new Transition (9408, 9409), // &VeryThinS -> &VeryThinSp
				new Transition (9427, 9430), // &vnsu -> &vnsup
				new Transition (9432, 9433), // &Vo -> &Vop
				new Transition (9436, 9437), // &vo -> &vop
				new Transition (9442, 9443), // &vpro -> &vprop
				new Transition (9458, 9465), // &vsu -> &vsup
				new Transition (9490, 9531), // &w -> &wp
				new Transition (9514, 9515), // &weier -> &weierp
				new Transition (9523, 9524), // &Wo -> &Wop
				new Transition (9527, 9528), // &wo -> &wop
				new Transition (9550, 9551), // &xca -> &xcap
				new Transition (9557, 9558), // &xcu -> &xcup
				new Transition (9595, 9596), // &xma -> &xmap
				new Transition (9602, 9611), // &xo -> &xop
				new Transition (9607, 9608), // &Xo -> &Xop
				new Transition (9642, 9643), // &xsqcu -> &xsqcup
				new Transition (9645, 9646), // &xu -> &xup
				new Transition (9716, 9717), // &Yo -> &Yop
				new Transition (9720, 9721), // &yo -> &yop
				new Transition (9799, 9800), // &ZeroWidthS -> &ZeroWidthSp
				new Transition (9832, 9833), // &Zo -> &Zop
				new Transition (9836, 9837) // &zo -> &zop
			};
			TransitionTable_q = new Transition[144] {
				new Transition (0, 6817), // & -> &q
				new Transition (234, 235), // &approxe -> &approxeq
				new Transition (266, 267), // &asympe -> &asympeq
				new Transition (328, 329), // &backsime -> &backsimeq
				new Transition (379, 380), // &bd -> &bdq
				new Transition (471, 472), // &bigs -> &bigsq
				new Transition (531, 532), // &blacks -> &blacksq
				new Transition (580, 582), // &bne -> &bneq
				new Transition (779, 787), // &bumpe -> &bumpeq
				new Transition (784, 785), // &Bumpe -> &Bumpeq
				new Transition (983, 984), // &circe -> &circeq
				new Transition (1138, 1140), // &colone -> &coloneq
				new Transition (1355, 1356), // &curlye -> &curlyeq
				new Transition (1513, 1514), // &ddotse -> &ddotseq
				new Transition (1700, 1701), // &dote -> &doteq
				new Transition (1707, 1708), // &DotE -> &DotEq
				new Transition (1724, 1725), // &dots -> &dotsq
				new Transition (2108, 2367), // &E -> &Eq
				new Transition (2115, 2339), // &e -> &eq
				new Transition (2254, 2255), // &EmptySmallS -> &EmptySmallSq
				new Transition (2272, 2273), // &EmptyVerySmallS -> &EmptyVerySmallSq
				new Transition (2514, 2515), // &fallingdotse -> &fallingdotseq
				new Transition (2564, 2565), // &FilledSmallS -> &FilledSmallSq
				new Transition (2580, 2581), // &FilledVerySmallS -> &FilledVerySmallSq
				new Transition (2765, 2771), // &ge -> &geq
				new Transition (2771, 2773), // &geq -> &geqq
				new Transition (2843, 2845), // &gne -> &gneq
				new Transition (2845, 2847), // &gneq -> &gneqq
				new Transition (2872, 2873), // &GreaterE -> &GreaterEq
				new Transition (2887, 2888), // &GreaterFullE -> &GreaterFullEq
				new Transition (2911, 2912), // &GreaterSlantE -> &GreaterSlantEq
				new Transition (2942, 2959), // &gt -> &gtq
				new Transition (2980, 2981), // &gtre -> &gtreq
				new Transition (2981, 2987), // &gtreq -> &gtreqq
				new Transition (3007, 3008), // &gvertne -> &gvertneq
				new Transition (3008, 3009), // &gvertneq -> &gvertneqq
				new Transition (3219, 3220), // &HumpE -> &HumpEq
				new Transition (3243, 3497), // &i -> &iq
				new Transition (3705, 3755), // &la -> &laq
				new Transition (3869, 3873), // &ld -> &ldq
				new Transition (3896, 4187), // &le -> &leq
				new Transition (4074, 4075), // &leftrights -> &leftrightsq
				new Transition (4132, 4133), // &LeftTriangleE -> &LeftTriangleEq
				new Transition (4187, 4189), // &leq -> &leqq
				new Transition (4227, 4228), // &lesse -> &lesseq
				new Transition (4228, 4233), // &lesseq -> &lesseqq
				new Transition (4240, 4241), // &LessE -> &LessEq
				new Transition (4257, 4258), // &LessFullE -> &LessFullEq
				new Transition (4289, 4290), // &LessSlantE -> &LessSlantEq
				new Transition (4412, 4414), // &lne -> &lneq
				new Transition (4414, 4416), // &lneq -> &lneqq
				new Transition (4652, 4676), // &ls -> &lsq
				new Transition (4653, 4654), // &lsa -> &lsaq
				new Transition (4698, 4725), // &lt -> &ltq
				new Transition (4760, 4761), // &lvertne -> &lvertneq
				new Transition (4761, 4762), // &lvertneq -> &lvertneqq
				new Transition (5064, 5135), // &ne -> &neq
				new Transition (5198, 5200), // &nge -> &ngeq
				new Transition (5200, 5202), // &ngeq -> &ngeqq
				new Transition (5270, 5312), // &nle -> &nleq
				new Transition (5312, 5314), // &nleq -> &nleqq
				new Transition (5414, 5422), // &NotE -> &NotEq
				new Transition (5447, 5448), // &NotGreaterE -> &NotGreaterEq
				new Transition (5457, 5458), // &NotGreaterFullE -> &NotGreaterFullEq
				new Transition (5481, 5482), // &NotGreaterSlantE -> &NotGreaterSlantEq
				new Transition (5506, 5507), // &NotHumpE -> &NotHumpEq
				new Transition (5545, 5546), // &NotLeftTriangleE -> &NotLeftTriangleEq
				new Transition (5554, 5555), // &NotLessE -> &NotLessEq
				new Transition (5578, 5579), // &NotLessSlantE -> &NotLessSlantEq
				new Transition (5639, 5640), // &NotPrecedesE -> &NotPrecedesEq
				new Transition (5650, 5651), // &NotPrecedesSlantE -> &NotPrecedesSlantEq
				new Transition (5688, 5689), // &NotRightTriangleE -> &NotRightTriangleEq
				new Transition (5694, 5695), // &NotS -> &NotSq
				new Transition (5707, 5708), // &NotSquareSubsetE -> &NotSquareSubsetEq
				new Transition (5720, 5721), // &NotSquareSupersetE -> &NotSquareSupersetEq
				new Transition (5732, 5733), // &NotSubsetE -> &NotSubsetEq
				new Transition (5745, 5746), // &NotSucceedsE -> &NotSucceedsEq
				new Transition (5756, 5757), // &NotSucceedsSlantE -> &NotSucceedsSlantEq
				new Transition (5775, 5776), // &NotSupersetE -> &NotSupersetEq
				new Transition (5787, 5788), // &NotTildeE -> &NotTildeEq
				new Transition (5797, 5798), // &NotTildeFullE -> &NotTildeFullEq
				new Transition (5852, 5853), // &nprece -> &npreceq
				new Transition (5895, 5942), // &ns -> &nsq
				new Transition (5930, 5932), // &nsime -> &nsimeq
				new Transition (5962, 5963), // &nsubsete -> &nsubseteq
				new Transition (5963, 5965), // &nsubseteq -> &nsubseteqq
				new Transition (5970, 5971), // &nsucce -> &nsucceq
				new Transition (5983, 5984), // &nsupsete -> &nsupseteq
				new Transition (5984, 5986), // &nsupseteq -> &nsupseteqq
				new Transition (6018, 6019), // &ntrianglelefte -> &ntrianglelefteq
				new Transition (6027, 6028), // &ntrianglerighte -> &ntrianglerighteq
				new Transition (6669, 6670), // &preccurlye -> &preccurlyeq
				new Transition (6679, 6680), // &PrecedesE -> &PrecedesEq
				new Transition (6690, 6691), // &PrecedesSlantE -> &PrecedesSlantEq
				new Transition (6702, 6703), // &prece -> &preceq
				new Transition (6713, 6714), // &precne -> &precneq
				new Transition (6714, 6715), // &precneq -> &precneqq
				new Transition (6866, 6867), // &queste -> &questeq
				new Transition (6882, 6921), // &ra -> &raq
				new Transition (7053, 7063), // &rd -> &rdq
				new Transition (7102, 7110), // &ReverseE -> &ReverseEq
				new Transition (7123, 7124), // &ReverseUpE -> &ReverseUpEq
				new Transition (7326, 7327), // &rights -> &rightsq
				new Transition (7377, 7378), // &RightTriangleE -> &RightTriangleEq
				new Transition (7439, 7440), // &risingdotse -> &risingdotseq
				new Transition (7542, 7559), // &rs -> &rsq
				new Transition (7543, 7544), // &rsa -> &rsaq
				new Transition (7610, 7980), // &S -> &Sq
				new Transition (7617, 7968), // &s -> &sq
				new Transition (7624, 7625), // &sb -> &sbq
				new Transition (7853, 7855), // &sime -> &simeq
				new Transition (7994, 7995), // &sqsubsete -> &sqsubseteq
				new Transition (8005, 8006), // &sqsupsete -> &sqsupseteq
				new Transition (8039, 8040), // &SquareSubsetE -> &SquareSubsetEq
				new Transition (8052, 8053), // &SquareSupersetE -> &SquareSupersetEq
				new Transition (8173, 8174), // &subsete -> &subseteq
				new Transition (8174, 8176), // &subseteq -> &subseteqq
				new Transition (8178, 8179), // &SubsetE -> &SubsetEq
				new Transition (8185, 8186), // &subsetne -> &subsetneq
				new Transition (8186, 8188), // &subsetneq -> &subsetneqq
				new Transition (8213, 8214), // &succcurlye -> &succcurlyeq
				new Transition (8223, 8224), // &SucceedsE -> &SucceedsEq
				new Transition (8234, 8235), // &SucceedsSlantE -> &SucceedsSlantEq
				new Transition (8246, 8247), // &succe -> &succeq
				new Transition (8257, 8258), // &succne -> &succneq
				new Transition (8258, 8259), // &succneq -> &succneqq
				new Transition (8314, 8315), // &SupersetE -> &SupersetEq
				new Transition (8356, 8357), // &supsete -> &supseteq
				new Transition (8357, 8359), // &supseteq -> &supseteqq
				new Transition (8362, 8363), // &supsetne -> &supsetneq
				new Transition (8363, 8365), // &supsetneq -> &supsetneqq
				new Transition (8554, 8555), // &TildeE -> &TildeEq
				new Transition (8564, 8565), // &TildeFullE -> &TildeFullEq
				new Transition (8638, 8653), // &triangle -> &triangleq
				new Transition (8650, 8651), // &trianglelefte -> &trianglelefteq
				new Transition (8661, 8662), // &trianglerighte -> &trianglerighteq
				new Transition (9034, 9035), // &UpE -> &UpEq
				new Transition (9264, 9265), // &varsubsetne -> &varsubsetneq
				new Transition (9265, 9267), // &varsubsetneq -> &varsubsetneqq
				new Transition (9274, 9275), // &varsupsetne -> &varsupsetneq
				new Transition (9275, 9277), // &varsupsetneq -> &varsupsetneqq
				new Transition (9352, 9353), // &veee -> &veeeq
				new Transition (9508, 9510), // &wedge -> &wedgeq
				new Transition (9636, 9640) // &xs -> &xsq
			};
			TransitionTable_r = new Transition[942] {
				new Transition (0, 6876), // & -> &r
				new Transition (1, 237), // &A -> &Ar
				new Transition (8, 242), // &a -> &ar
				new Transition (15, 16), // &Ab -> &Abr
				new Transition (21, 22), // &ab -> &abr
				new Transition (34, 35), // &Aci -> &Acir
				new Transition (38, 39), // &aci -> &acir
				new Transition (60, 65), // &af -> &afr
				new Transition (62, 63), // &Af -> &Afr
				new Transition (67, 68), // &Ag -> &Agr
				new Transition (73, 74), // &ag -> &agr
				new Transition (100, 101), // &Amac -> &Amacr
				new Transition (105, 106), // &amac -> &amacr
				new Transition (136, 164), // &ang -> &angr
				new Transition (179, 180), // &angza -> &angzar
				new Transition (180, 181), // &angzar -> &angzarr
				new Transition (203, 204), // &apaci -> &apacir
				new Transition (229, 230), // &app -> &appr
				new Transition (248, 249), // &Asc -> &Ascr
				new Transition (252, 253), // &asc -> &ascr
				new Transition (301, 730), // &b -> &br
				new Transition (302, 344), // &ba -> &bar
				new Transition (318, 319), // &backp -> &backpr
				new Transition (331, 725), // &B -> &Br
				new Transition (332, 341), // &Ba -> &Bar
				new Transition (360, 361), // &bb -> &bbr
				new Transition (365, 366), // &bbrktb -> &bbrktbr
				new Transition (384, 409), // &be -> &ber
				new Transition (390, 414), // &Be -> &Ber
				new Transition (436, 437), // &Bf -> &Bfr
				new Transition (439, 440), // &bf -> &bfr
				new Transition (448, 449), // &bigci -> &bigcir
				new Transition (478, 479), // &bigsta -> &bigstar
				new Transition (481, 482), // &bigt -> &bigtr
				new Transition (514, 515), // &bka -> &bkar
				new Transition (534, 535), // &blacksqua -> &blacksquar
				new Transition (538, 539), // &blackt -> &blacktr
				new Transition (545, 557), // &blacktriangle -> &blacktriangler
				new Transition (618, 630), // &boxD -> &boxDr
				new Transition (623, 634), // &boxd -> &boxdr
				new Transition (673, 685), // &boxU -> &boxUr
				new Transition (678, 689), // &boxu -> &boxur
				new Transition (691, 713), // &boxV -> &boxVr
				new Transition (693, 717), // &boxv -> &boxvr
				new Transition (719, 720), // &bp -> &bpr
				new Transition (737, 738), // &brvba -> &brvbar
				new Transition (741, 742), // &Bsc -> &Bscr
				new Transition (745, 746), // &bsc -> &bscr
				new Transition (789, 1261), // &C -> &Cr
				new Transition (796, 1256), // &c -> &cr
				new Transition (797, 848), // &ca -> &car
				new Transition (811, 812), // &capb -> &capbr
				new Transition (836, 837), // &CapitalDiffe -> &CapitalDiffer
				new Transition (862, 872), // &cca -> &ccar
				new Transition (867, 868), // &Cca -> &Ccar
				new Transition (886, 887), // &Cci -> &Ccir
				new Transition (890, 891), // &cci -> &ccir
				new Transition (938, 939), // &Cente -> &Center
				new Transition (944, 945), // &cente -> &center
				new Transition (950, 951), // &Cf -> &Cfr
				new Transition (953, 954), // &cf -> &cfr
				new Transition (969, 970), // &checkma -> &checkmar
				new Transition (978, 979), // &ci -> &cir
				new Transition (988, 989), // &circlea -> &circlear
				new Transition (989, 990), // &circlear -> &circlearr
				new Transition (992, 998), // &circlearrow -> &circlearrowr
				new Transition (1010, 1011), // &circledci -> &circledcir
				new Transition (1019, 1020), // &Ci -> &Cir
				new Transition (1065, 1066), // &cirsci -> &cirscir
				new Transition (1081, 1082), // &ClockwiseContou -> &ClockwiseContour
				new Transition (1087, 1088), // &ClockwiseContourInteg -> &ClockwiseContourIntegr
				new Transition (1095, 1096), // &CloseCu -> &CloseCur
				new Transition (1172, 1173), // &Cong -> &Congr
				new Transition (1189, 1190), // &Contou -> &Contour
				new Transition (1195, 1196), // &ContourInteg -> &ContourIntegr
				new Transition (1200, 1210), // &Cop -> &Copr
				new Transition (1203, 1206), // &cop -> &copr
				new Transition (1223, 1224), // &copys -> &copysr
				new Transition (1229, 1230), // &Counte -> &Counter
				new Transition (1245, 1246), // &CounterClockwiseContou -> &CounterClockwiseContour
				new Transition (1251, 1252), // &CounterClockwiseContourInteg -> &CounterClockwiseContourIntegr
				new Transition (1257, 1258), // &cra -> &crar
				new Transition (1258, 1259), // &crar -> &crarr
				new Transition (1271, 1272), // &Csc -> &Cscr
				new Transition (1275, 1276), // &csc -> &cscr
				new Transition (1292, 1346), // &cu -> &cur
				new Transition (1294, 1295), // &cuda -> &cudar
				new Transition (1295, 1296), // &cudar -> &cudarr
				new Transition (1296, 1299), // &cudarr -> &cudarrr
				new Transition (1302, 1303), // &cuep -> &cuepr
				new Transition (1309, 1310), // &cula -> &cular
				new Transition (1310, 1311), // &cular -> &cularr
				new Transition (1320, 1321), // &cupb -> &cupbr
				new Transition (1341, 1342), // &cupo -> &cupor
				new Transition (1346, 1377), // &cur -> &curr
				new Transition (1347, 1348), // &cura -> &curar
				new Transition (1348, 1349), // &curar -> &curarr
				new Transition (1357, 1358), // &curlyeqp -> &curlyeqpr
				new Transition (1383, 1384), // &curvea -> &curvear
				new Transition (1384, 1385), // &curvear -> &curvearr
				new Transition (1387, 1393), // &curvearrow -> &curvearrowr
				new Transition (1426, 1444), // &Da -> &Dar
				new Transition (1429, 1430), // &Dagge -> &Dagger
				new Transition (1432, 2023), // &d -> &dr
				new Transition (1433, 1451), // &da -> &dar
				new Transition (1436, 1437), // &dagge -> &dagger
				new Transition (1444, 1445), // &Dar -> &Darr
				new Transition (1447, 1448), // &dA -> &dAr
				new Transition (1448, 1449), // &dAr -> &dArr
				new Transition (1451, 1452), // &dar -> &darr
				new Transition (1465, 1466), // &dbka -> &dbkar
				new Transition (1475, 1476), // &Dca -> &Dcar
				new Transition (1481, 1482), // &dca -> &dcar
				new Transition (1494, 1500), // &dda -> &ddar
				new Transition (1497, 1498), // &ddagge -> &ddagger
				new Transition (1500, 1501), // &ddar -> &ddarr
				new Transition (1504, 1505), // &DDot -> &DDotr
				new Transition (1535, 1544), // &df -> &dfr
				new Transition (1541, 1542), // &Df -> &Dfr
				new Transition (1547, 1548), // &dHa -> &dHar
				new Transition (1551, 1552), // &dha -> &dhar
				new Transition (1552, 1555), // &dhar -> &dharr
				new Transition (1559, 1560), // &Diac -> &Diacr
				new Transition (1587, 1588), // &DiacriticalG -> &DiacriticalGr
				new Transition (1623, 1624), // &Diffe -> &Differ
				new Transition (1670, 1675), // &dlc -> &dlcr
				new Transition (1671, 1672), // &dlco -> &dlcor
				new Transition (1682, 1683), // &dolla -> &dollar
				new Transition (1727, 1728), // &dotsqua -> &dotsquar
				new Transition (1736, 1737), // &doubleba -> &doublebar
				new Transition (1753, 1754), // &DoubleContou -> &DoubleContour
				new Transition (1759, 1760), // &DoubleContourInteg -> &DoubleContourIntegr
				new Transition (1770, 1771), // &DoubleDownA -> &DoubleDownAr
				new Transition (1771, 1772), // &DoubleDownAr -> &DoubleDownArr
				new Transition (1780, 1781), // &DoubleLeftA -> &DoubleLeftAr
				new Transition (1781, 1782), // &DoubleLeftAr -> &DoubleLeftArr
				new Transition (1791, 1792), // &DoubleLeftRightA -> &DoubleLeftRightAr
				new Transition (1792, 1793), // &DoubleLeftRightAr -> &DoubleLeftRightArr
				new Transition (1808, 1809), // &DoubleLongLeftA -> &DoubleLongLeftAr
				new Transition (1809, 1810), // &DoubleLongLeftAr -> &DoubleLongLeftArr
				new Transition (1819, 1820), // &DoubleLongLeftRightA -> &DoubleLongLeftRightAr
				new Transition (1820, 1821), // &DoubleLongLeftRightAr -> &DoubleLongLeftRightArr
				new Transition (1830, 1831), // &DoubleLongRightA -> &DoubleLongRightAr
				new Transition (1831, 1832), // &DoubleLongRightAr -> &DoubleLongRightArr
				new Transition (1841, 1842), // &DoubleRightA -> &DoubleRightAr
				new Transition (1842, 1843), // &DoubleRightAr -> &DoubleRightArr
				new Transition (1853, 1854), // &DoubleUpA -> &DoubleUpAr
				new Transition (1854, 1855), // &DoubleUpAr -> &DoubleUpArr
				new Transition (1863, 1864), // &DoubleUpDownA -> &DoubleUpDownAr
				new Transition (1864, 1865), // &DoubleUpDownAr -> &DoubleUpDownArr
				new Transition (1870, 1871), // &DoubleVe -> &DoubleVer
				new Transition (1878, 1879), // &DoubleVerticalBa -> &DoubleVerticalBar
				new Transition (1883, 1884), // &DownA -> &DownAr
				new Transition (1884, 1885), // &DownAr -> &DownArr
				new Transition (1889, 1890), // &Downa -> &Downar
				new Transition (1890, 1891), // &Downar -> &Downarr
				new Transition (1897, 1898), // &downa -> &downar
				new Transition (1898, 1899), // &downar -> &downarr
				new Transition (1904, 1905), // &DownArrowBa -> &DownArrowBar
				new Transition (1909, 1910), // &DownArrowUpA -> &DownArrowUpAr
				new Transition (1910, 1911), // &DownArrowUpAr -> &DownArrowUpArr
				new Transition (1915, 1916), // &DownB -> &DownBr
				new Transition (1925, 1926), // &downdowna -> &downdownar
				new Transition (1926, 1927), // &downdownar -> &downdownarr
				new Transition (1933, 1934), // &downha -> &downhar
				new Transition (1938, 1944), // &downharpoon -> &downharpoonr
				new Transition (1963, 1964), // &DownLeftRightVecto -> &DownLeftRightVector
				new Transition (1973, 1974), // &DownLeftTeeVecto -> &DownLeftTeeVector
				new Transition (1980, 1981), // &DownLeftVecto -> &DownLeftVector
				new Transition (1984, 1985), // &DownLeftVectorBa -> &DownLeftVectorBar
				new Transition (1999, 2000), // &DownRightTeeVecto -> &DownRightTeeVector
				new Transition (2006, 2007), // &DownRightVecto -> &DownRightVector
				new Transition (2010, 2011), // &DownRightVectorBa -> &DownRightVectorBar
				new Transition (2017, 2018), // &DownTeeA -> &DownTeeAr
				new Transition (2018, 2019), // &DownTeeAr -> &DownTeeArr
				new Transition (2026, 2027), // &drbka -> &drbkar
				new Transition (2031, 2036), // &drc -> &drcr
				new Transition (2032, 2033), // &drco -> &drcor
				new Transition (2041, 2042), // &Dsc -> &Dscr
				new Transition (2045, 2046), // &dsc -> &dscr
				new Transition (2057, 2058), // &Dst -> &Dstr
				new Transition (2062, 2063), // &dst -> &dstr
				new Transition (2067, 2072), // &dt -> &dtr
				new Transition (2078, 2079), // &dua -> &duar
				new Transition (2079, 2080), // &duar -> &duarr
				new Transition (2083, 2084), // &duha -> &duhar
				new Transition (2102, 2103), // &dzig -> &dzigr
				new Transition (2104, 2105), // &dzigra -> &dzigrar
				new Transition (2105, 2106), // &dzigrar -> &dzigrarr
				new Transition (2115, 2409), // &e -> &er
				new Transition (2124, 2125), // &easte -> &easter
				new Transition (2128, 2129), // &Eca -> &Ecar
				new Transition (2134, 2135), // &eca -> &ecar
				new Transition (2139, 2140), // &eci -> &ecir
				new Transition (2142, 2143), // &Eci -> &Ecir
				new Transition (2175, 2183), // &ef -> &efr
				new Transition (2180, 2181), // &Ef -> &Efr
				new Transition (2185, 2193), // &eg -> &egr
				new Transition (2187, 2188), // &Eg -> &Egr
				new Transition (2216, 2217), // &elinte -> &elinter
				new Transition (2230, 2231), // &Emac -> &Emacr
				new Transition (2235, 2236), // &emac -> &emacr
				new Transition (2257, 2258), // &EmptySmallSqua -> &EmptySmallSquar
				new Transition (2264, 2265), // &EmptyVe -> &EmptyVer
				new Transition (2275, 2276), // &EmptyVerySmallSqua -> &EmptyVerySmallSquar
				new Transition (2313, 2314), // &epa -> &epar
				new Transition (2341, 2342), // &eqci -> &eqcir
				new Transition (2359, 2360), // &eqslantgt -> &eqslantgtr
				new Transition (2390, 2391), // &Equilib -> &Equilibr
				new Transition (2404, 2405), // &eqvpa -> &eqvpar
				new Transition (2410, 2411), // &era -> &erar
				new Transition (2411, 2412), // &erar -> &erarr
				new Transition (2419, 2420), // &Esc -> &Escr
				new Transition (2423, 2424), // &esc -> &escr
				new Transition (2451, 2455), // &eu -> &eur
				new Transition (2503, 2647), // &f -> &fr
				new Transition (2530, 2547), // &ff -> &ffr
				new Transition (2544, 2545), // &Ff -> &Ffr
				new Transition (2567, 2568), // &FilledSmallSqua -> &FilledSmallSquar
				new Transition (2572, 2573), // &FilledVe -> &FilledVer
				new Transition (2583, 2584), // &FilledVerySmallSqua -> &FilledVerySmallSquar
				new Transition (2608, 2616), // &Fo -> &For
				new Transition (2612, 2621), // &fo -> &for
				new Transition (2630, 2631), // &Fou -> &Four
				new Transition (2633, 2634), // &Fourie -> &Fourier
				new Transition (2635, 2636), // &Fouriert -> &Fouriertr
				new Transition (2640, 2641), // &fpa -> &fpar
				new Transition (2694, 2695), // &Fsc -> &Fscr
				new Transition (2698, 2699), // &fsc -> &fscr
				new Transition (2701, 2861), // &g -> &gr
				new Transition (2708, 2866), // &G -> &Gr
				new Transition (2724, 2725), // &Gb -> &Gbr
				new Transition (2730, 2731), // &gb -> &gbr
				new Transition (2742, 2743), // &Gci -> &Gcir
				new Transition (2747, 2748), // &gci -> &gcir
				new Transition (2799, 2800), // &Gf -> &Gfr
				new Transition (2802, 2803), // &gf -> &gfr
				new Transition (2836, 2837), // &gnapp -> &gnappr
				new Transition (2870, 2871), // &Greate -> &Greater
				new Transition (2893, 2894), // &GreaterG -> &GreaterGr
				new Transition (2898, 2899), // &GreaterGreate -> &GreaterGreater
				new Transition (2924, 2925), // &Gsc -> &Gscr
				new Transition (2928, 2929), // &gsc -> &gscr
				new Transition (2942, 2965), // &gt -> &gtr
				new Transition (2947, 2948), // &gtci -> &gtcir
				new Transition (2956, 2957), // &gtlPa -> &gtlPar
				new Transition (2966, 2973), // &gtra -> &gtrar
				new Transition (2968, 2969), // &gtrapp -> &gtrappr
				new Transition (2973, 2974), // &gtrar -> &gtrarr
				new Transition (3003, 3004), // &gve -> &gver
				new Transition (3021, 3041), // &ha -> &har
				new Transition (3022, 3023), // &hai -> &hair
				new Transition (3041, 3050), // &har -> &harr
				new Transition (3046, 3047), // &hA -> &hAr
				new Transition (3047, 3048), // &hAr -> &hArr
				new Transition (3053, 3054), // &harrci -> &harrcir
				new Transition (3061, 3062), // &hba -> &hbar
				new Transition (3065, 3066), // &Hci -> &Hcir
				new Transition (3070, 3071), // &hci -> &hcir
				new Transition (3074, 3089), // &he -> &her
				new Transition (3075, 3076), // &hea -> &hear
				new Transition (3094, 3095), // &Hf -> &Hfr
				new Transition (3097, 3098), // &hf -> &hfr
				new Transition (3103, 3104), // &Hilbe -> &Hilber
				new Transition (3115, 3116), // &hksea -> &hksear
				new Transition (3121, 3122), // &hkswa -> &hkswar
				new Transition (3126, 3166), // &ho -> &hor
				new Transition (3127, 3128), // &hoa -> &hoar
				new Transition (3128, 3129), // &hoar -> &hoarr
				new Transition (3137, 3148), // &hook -> &hookr
				new Transition (3142, 3143), // &hooklefta -> &hookleftar
				new Transition (3143, 3144), // &hookleftar -> &hookleftarr
				new Transition (3153, 3154), // &hookrighta -> &hookrightar
				new Transition (3154, 3155), // &hookrightar -> &hookrightarr
				new Transition (3159, 3171), // &Ho -> &Hor
				new Transition (3168, 3169), // &horba -> &horbar
				new Transition (3185, 3186), // &Hsc -> &Hscr
				new Transition (3189, 3190), // &hsc -> &hscr
				new Transition (3197, 3198), // &Hst -> &Hstr
				new Transition (3202, 3203), // &hst -> &hstr
				new Transition (3253, 3254), // &Ici -> &Icir
				new Transition (3257, 3258), // &ici -> &icir
				new Transition (3281, 3287), // &if -> &ifr
				new Transition (3284, 3285), // &If -> &Ifr
				new Transition (3289, 3290), // &Ig -> &Igr
				new Transition (3295, 3296), // &ig -> &igr
				new Transition (3333, 3334), // &Imac -> &Imacr
				new Transition (3338, 3339), // &imac -> &imacr
				new Transition (3347, 3348), // &Imagina -> &Imaginar
				new Transition (3358, 3359), // &imagpa -> &imagpar
				new Transition (3381, 3382), // &inca -> &incar
				new Transition (3407, 3419), // &inte -> &inter
				new Transition (3409, 3410), // &intege -> &integer
				new Transition (3413, 3424), // &Inte -> &Inter
				new Transition (3414, 3415), // &Integ -> &Integr
				new Transition (3434, 3435), // &intla -> &intlar
				new Transition (3439, 3440), // &intp -> &intpr
				new Transition (3492, 3493), // &ip -> &ipr
				new Transition (3504, 3505), // &Isc -> &Iscr
				new Transition (3508, 3509), // &isc -> &iscr
				new Transition (3557, 3558), // &Jci -> &Jcir
				new Transition (3563, 3564), // &jci -> &jcir
				new Transition (3571, 3572), // &Jf -> &Jfr
				new Transition (3574, 3575), // &jf -> &jfr
				new Transition (3591, 3592), // &Jsc -> &Jscr
				new Transition (3595, 3596), // &jsc -> &jscr
				new Transition (3598, 3599), // &Jse -> &Jser
				new Transition (3603, 3604), // &jse -> &jser
				new Transition (3648, 3649), // &Kf -> &Kfr
				new Transition (3651, 3652), // &kf -> &kfr
				new Transition (3654, 3655), // &kg -> &kgr
				new Transition (3685, 3686), // &Ksc -> &Kscr
				new Transition (3689, 3690), // &ksc -> &kscr
				new Transition (3692, 4628), // &l -> &lr
				new Transition (3693, 3762), // &lA -> &lAr
				new Transition (3694, 3695), // &lAa -> &lAar
				new Transition (3695, 3696), // &lAar -> &lAarr
				new Transition (3699, 3759), // &La -> &Lar
				new Transition (3705, 3765), // &la -> &lar
				new Transition (3718, 3719), // &lag -> &lagr
				new Transition (3751, 3752), // &Laplacet -> &Laplacetr
				new Transition (3759, 3760), // &Lar -> &Larr
				new Transition (3762, 3763), // &lAr -> &lArr
				new Transition (3765, 3766), // &lar -> &larr
				new Transition (3808, 3809), // &lBa -> &lBar
				new Transition (3809, 3810), // &lBar -> &lBarr
				new Transition (3812, 3821), // &lb -> &lbr
				new Transition (3813, 3814), // &lba -> &lbar
				new Transition (3814, 3815), // &lbar -> &lbarr
				new Transition (3817, 3818), // &lbb -> &lbbr
				new Transition (3838, 3839), // &Lca -> &Lcar
				new Transition (3844, 3845), // &lca -> &lcar
				new Transition (3869, 3879), // &ld -> &ldr
				new Transition (3875, 3877), // &ldquo -> &ldquor
				new Transition (3882, 3883), // &ldrdha -> &ldrdhar
				new Transition (3888, 3889), // &ldrusha -> &ldrushar
				new Transition (3900, 4041), // &Left -> &Leftr
				new Transition (3901, 3914), // &LeftA -> &LeftAr
				new Transition (3906, 3907), // &LeftAngleB -> &LeftAngleBr
				new Transition (3914, 3915), // &LeftAr -> &LeftArr
				new Transition (3919, 3920), // &Lefta -> &Leftar
				new Transition (3920, 3921), // &Leftar -> &Leftarr
				new Transition (3926, 4052), // &left -> &leftr
				new Transition (3927, 3928), // &lefta -> &leftar
				new Transition (3928, 3929), // &leftar -> &leftarr
				new Transition (3934, 3935), // &LeftArrowBa -> &LeftArrowBar
				new Transition (3942, 3943), // &LeftArrowRightA -> &LeftArrowRightAr
				new Transition (3943, 3944), // &LeftArrowRightAr -> &LeftArrowRightArr
				new Transition (3967, 3968), // &LeftDoubleB -> &LeftDoubleBr
				new Transition (3984, 3985), // &LeftDownTeeVecto -> &LeftDownTeeVector
				new Transition (3991, 3992), // &LeftDownVecto -> &LeftDownVector
				new Transition (3995, 3996), // &LeftDownVectorBa -> &LeftDownVectorBar
				new Transition (4001, 4002), // &LeftFloo -> &LeftFloor
				new Transition (4005, 4006), // &leftha -> &lefthar
				new Transition (4023, 4024), // &leftlefta -> &leftleftar
				new Transition (4024, 4025), // &leftleftar -> &leftleftarr
				new Transition (4035, 4036), // &LeftRightA -> &LeftRightAr
				new Transition (4036, 4037), // &LeftRightAr -> &LeftRightArr
				new Transition (4046, 4047), // &Leftrighta -> &Leftrightar
				new Transition (4047, 4048), // &Leftrightar -> &Leftrightarr
				new Transition (4057, 4058), // &leftrighta -> &leftrightar
				new Transition (4058, 4059), // &leftrightar -> &leftrightarr
				new Transition (4066, 4067), // &leftrightha -> &leftrighthar
				new Transition (4079, 4080), // &leftrightsquiga -> &leftrightsquigar
				new Transition (4080, 4081), // &leftrightsquigar -> &leftrightsquigarr
				new Transition (4089, 4090), // &LeftRightVecto -> &LeftRightVector
				new Transition (4092, 4120), // &LeftT -> &LeftTr
				new Transition (4096, 4097), // &LeftTeeA -> &LeftTeeAr
				new Transition (4097, 4098), // &LeftTeeAr -> &LeftTeeArr
				new Transition (4106, 4107), // &LeftTeeVecto -> &LeftTeeVector
				new Transition (4110, 4111), // &leftth -> &leftthr
				new Transition (4129, 4130), // &LeftTriangleBa -> &LeftTriangleBar
				new Transition (4148, 4149), // &LeftUpDownVecto -> &LeftUpDownVector
				new Transition (4158, 4159), // &LeftUpTeeVecto -> &LeftUpTeeVector
				new Transition (4165, 4166), // &LeftUpVecto -> &LeftUpVector
				new Transition (4169, 4170), // &LeftUpVectorBa -> &LeftUpVectorBar
				new Transition (4176, 4177), // &LeftVecto -> &LeftVector
				new Transition (4180, 4181), // &LeftVectorBa -> &LeftVectorBar
				new Transition (4206, 4208), // &lesdoto -> &lesdotor
				new Transition (4218, 4219), // &lessapp -> &lessappr
				new Transition (4230, 4231), // &lesseqgt -> &lesseqgtr
				new Transition (4235, 4236), // &lesseqqgt -> &lesseqqgtr
				new Transition (4245, 4246), // &LessEqualG -> &LessEqualGr
				new Transition (4250, 4251), // &LessEqualGreate -> &LessEqualGreater
				new Transition (4263, 4264), // &LessG -> &LessGr
				new Transition (4268, 4269), // &LessGreate -> &LessGreater
				new Transition (4272, 4273), // &lessgt -> &lessgtr
				new Transition (4301, 4315), // &lf -> &lfr
				new Transition (4309, 4310), // &lfloo -> &lfloor
				new Transition (4312, 4313), // &Lf -> &Lfr
				new Transition (4322, 4323), // &lHa -> &lHar
				new Transition (4326, 4327), // &lha -> &lhar
				new Transition (4350, 4351), // &lla -> &llar
				new Transition (4351, 4352), // &llar -> &llarr
				new Transition (4355, 4356), // &llco -> &llcor
				new Transition (4358, 4359), // &llcorne -> &llcorner
				new Transition (4364, 4365), // &Llefta -> &Lleftar
				new Transition (4365, 4366), // &Lleftar -> &Lleftarr
				new Transition (4371, 4372), // &llha -> &llhar
				new Transition (4375, 4376), // &llt -> &lltr
				new Transition (4405, 4406), // &lnapp -> &lnappr
				new Transition (4423, 4427), // &loa -> &loar
				new Transition (4427, 4428), // &loar -> &loarr
				new Transition (4430, 4431), // &lob -> &lobr
				new Transition (4436, 4520), // &Long -> &Longr
				new Transition (4441, 4442), // &LongLeftA -> &LongLeftAr
				new Transition (4442, 4443), // &LongLeftAr -> &LongLeftArr
				new Transition (4450, 4480), // &Longleft -> &Longleftr
				new Transition (4451, 4452), // &Longlefta -> &Longleftar
				new Transition (4452, 4453), // &Longleftar -> &Longleftarr
				new Transition (4458, 4531), // &long -> &longr
				new Transition (4462, 4491), // &longleft -> &longleftr
				new Transition (4463, 4464), // &longlefta -> &longleftar
				new Transition (4464, 4465), // &longleftar -> &longleftarr
				new Transition (4474, 4475), // &LongLeftRightA -> &LongLeftRightAr
				new Transition (4475, 4476), // &LongLeftRightAr -> &LongLeftRightArr
				new Transition (4485, 4486), // &Longleftrighta -> &Longleftrightar
				new Transition (4486, 4487), // &Longleftrightar -> &Longleftrightarr
				new Transition (4496, 4497), // &longleftrighta -> &longleftrightar
				new Transition (4497, 4498), // &longleftrightar -> &longleftrightarr
				new Transition (4514, 4515), // &LongRightA -> &LongRightAr
				new Transition (4515, 4516), // &LongRightAr -> &LongRightArr
				new Transition (4525, 4526), // &Longrighta -> &Longrightar
				new Transition (4526, 4527), // &Longrightar -> &Longrightarr
				new Transition (4536, 4537), // &longrighta -> &longrightar
				new Transition (4537, 4538), // &longrightar -> &longrightarr
				new Transition (4544, 4545), // &loopa -> &loopar
				new Transition (4545, 4546), // &loopar -> &looparr
				new Transition (4548, 4554), // &looparrow -> &looparrowr
				new Transition (4561, 4562), // &lopa -> &lopar
				new Transition (4585, 4586), // &lowba -> &lowbar
				new Transition (4589, 4590), // &Lowe -> &Lower
				new Transition (4595, 4596), // &LowerLeftA -> &LowerLeftAr
				new Transition (4596, 4597), // &LowerLeftAr -> &LowerLeftArr
				new Transition (4606, 4607), // &LowerRightA -> &LowerRightAr
				new Transition (4607, 4608), // &LowerRightAr -> &LowerRightArr
				new Transition (4622, 4623), // &lpa -> &lpar
				new Transition (4629, 4630), // &lra -> &lrar
				new Transition (4630, 4631), // &lrar -> &lrarr
				new Transition (4634, 4635), // &lrco -> &lrcor
				new Transition (4637, 4638), // &lrcorne -> &lrcorner
				new Transition (4641, 4642), // &lrha -> &lrhar
				new Transition (4648, 4649), // &lrt -> &lrtr
				new Transition (4659, 4660), // &Lsc -> &Lscr
				new Transition (4662, 4663), // &lsc -> &lscr
				new Transition (4680, 4682), // &lsquo -> &lsquor
				new Transition (4684, 4685), // &Lst -> &Lstr
				new Transition (4689, 4690), // &lst -> &lstr
				new Transition (4698, 4731), // &lt -> &ltr
				new Transition (4703, 4704), // &ltci -> &ltcir
				new Transition (4710, 4711), // &lth -> &lthr
				new Transition (4721, 4722), // &ltla -> &ltlar
				new Transition (4722, 4723), // &ltlar -> &ltlarr
				new Transition (4739, 4740), // &ltrPa -> &ltrPar
				new Transition (4742, 4743), // &lu -> &lur
				new Transition (4747, 4748), // &lurdsha -> &lurdshar
				new Transition (4752, 4753), // &luruha -> &luruhar
				new Transition (4756, 4757), // &lve -> &lver
				new Transition (4768, 4804), // &ma -> &mar
				new Transition (4769, 4770), // &mac -> &macr
				new Transition (4806, 4807), // &marke -> &marker
				new Transition (4833, 4834), // &measu -> &measur
				new Transition (4858, 4859), // &Mellint -> &Mellintr
				new Transition (4862, 4863), // &Mf -> &Mfr
				new Transition (4865, 4866), // &mf -> &mfr
				new Transition (4872, 4873), // &mic -> &micr
				new Transition (4883, 4884), // &midci -> &midcir
				new Transition (4913, 4914), // &mld -> &mldr
				new Transition (4938, 4939), // &Msc -> &Mscr
				new Transition (4942, 4943), // &msc -> &mscr
				new Transition (4965, 5855), // &n -> &nr
				new Transition (4996, 4997), // &napp -> &nappr
				new Transition (5002, 5003), // &natu -> &natur
				new Transition (5021, 5030), // &nca -> &ncar
				new Transition (5025, 5026), // &Nca -> &Ncar
				new Transition (5066, 5067), // &nea -> &near
				new Transition (5067, 5075), // &near -> &nearr
				new Transition (5071, 5072), // &neA -> &neAr
				new Transition (5072, 5073), // &neAr -> &neArr
				new Transition (5122, 5123), // &NegativeVe -> &NegativeVer
				new Transition (5142, 5143), // &nesea -> &nesear
				new Transition (5152, 5153), // &NestedG -> &NestedGr
				new Transition (5157, 5158), // &NestedGreate -> &NestedGreater
				new Transition (5159, 5160), // &NestedGreaterG -> &NestedGreaterGr
				new Transition (5164, 5165), // &NestedGreaterGreate -> &NestedGreaterGreater
				new Transition (5189, 5190), // &Nf -> &Nfr
				new Transition (5192, 5193), // &nf -> &nfr
				new Transition (5221, 5223), // &ngt -> &ngtr
				new Transition (5228, 5229), // &nhA -> &nhAr
				new Transition (5229, 5230), // &nhAr -> &nhArr
				new Transition (5232, 5233), // &nha -> &nhar
				new Transition (5233, 5234), // &nhar -> &nharr
				new Transition (5237, 5238), // &nhpa -> &nhpar
				new Transition (5257, 5258), // &nlA -> &nlAr
				new Transition (5258, 5259), // &nlAr -> &nlArr
				new Transition (5261, 5262), // &nla -> &nlar
				new Transition (5262, 5263), // &nlar -> &nlarr
				new Transition (5265, 5266), // &nld -> &nldr
				new Transition (5275, 5290), // &nLeft -> &nLeftr
				new Transition (5276, 5277), // &nLefta -> &nLeftar
				new Transition (5277, 5278), // &nLeftar -> &nLeftarr
				new Transition (5283, 5301), // &nleft -> &nleftr
				new Transition (5284, 5285), // &nlefta -> &nleftar
				new Transition (5285, 5286), // &nleftar -> &nleftarr
				new Transition (5295, 5296), // &nLeftrighta -> &nLeftrightar
				new Transition (5296, 5297), // &nLeftrightar -> &nLeftrightarr
				new Transition (5306, 5307), // &nleftrighta -> &nleftrightar
				new Transition (5307, 5308), // &nleftrightar -> &nleftrightarr
				new Transition (5334, 5336), // &nlt -> &nltr
				new Transition (5348, 5349), // &NoB -> &NoBr
				new Transition (5355, 5356), // &NonB -> &NonBr
				new Transition (5383, 5384), // &NotCong -> &NotCongr
				new Transition (5403, 5404), // &NotDoubleVe -> &NotDoubleVer
				new Transition (5411, 5412), // &NotDoubleVerticalBa -> &NotDoubleVerticalBar
				new Transition (5439, 5440), // &NotG -> &NotGr
				new Transition (5444, 5445), // &NotGreate -> &NotGreater
				new Transition (5463, 5464), // &NotGreaterG -> &NotGreaterGr
				new Transition (5468, 5469), // &NotGreaterGreate -> &NotGreaterGreater
				new Transition (5532, 5533), // &NotLeftT -> &NotLeftTr
				new Transition (5542, 5543), // &NotLeftTriangleBa -> &NotLeftTriangleBar
				new Transition (5560, 5561), // &NotLessG -> &NotLessGr
				new Transition (5565, 5566), // &NotLessGreate -> &NotLessGreater
				new Transition (5596, 5597), // &NotNestedG -> &NotNestedGr
				new Transition (5601, 5602), // &NotNestedGreate -> &NotNestedGreater
				new Transition (5603, 5604), // &NotNestedGreaterG -> &NotNestedGreaterGr
				new Transition (5608, 5609), // &NotNestedGreaterGreate -> &NotNestedGreaterGreater
				new Transition (5630, 5631), // &NotP -> &NotPr
				new Transition (5659, 5660), // &NotReve -> &NotRever
				new Transition (5675, 5676), // &NotRightT -> &NotRightTr
				new Transition (5685, 5686), // &NotRightTriangleBa -> &NotRightTriangleBar
				new Transition (5697, 5698), // &NotSqua -> &NotSquar
				new Transition (5714, 5715), // &NotSquareSupe -> &NotSquareSuper
				new Transition (5769, 5770), // &NotSupe -> &NotSuper
				new Transition (5810, 5811), // &NotVe -> &NotVer
				new Transition (5818, 5819), // &NotVerticalBa -> &NotVerticalBar
				new Transition (5821, 5842), // &np -> &npr
				new Transition (5822, 5823), // &npa -> &npar
				new Transition (5856, 5857), // &nrA -> &nrAr
				new Transition (5857, 5858), // &nrAr -> &nrArr
				new Transition (5860, 5861), // &nra -> &nrar
				new Transition (5861, 5862), // &nrar -> &nrarr
				new Transition (5873, 5874), // &nRighta -> &nRightar
				new Transition (5874, 5875), // &nRightar -> &nRightarr
				new Transition (5883, 5884), // &nrighta -> &nrightar
				new Transition (5884, 5885), // &nrightar -> &nrightarr
				new Transition (5889, 5890), // &nrt -> &nrtr
				new Transition (5896, 5908), // &nsc -> &nscr
				new Transition (5905, 5906), // &Nsc -> &Nscr
				new Transition (5911, 5912), // &nsho -> &nshor
				new Transition (5919, 5920), // &nshortpa -> &nshortpar
				new Transition (5939, 5940), // &nspa -> &nspar
				new Transition (5988, 6006), // &nt -> &ntr
				new Transition (6012, 6021), // &ntriangle -> &ntriangler
				new Transition (6036, 6037), // &nume -> &numer
				new Transition (6043, 6097), // &nv -> &nvr
				new Transition (6074, 6075), // &nvHa -> &nvHar
				new Transition (6075, 6076), // &nvHar -> &nvHarr
				new Transition (6085, 6086), // &nvlA -> &nvlAr
				new Transition (6086, 6087), // &nvlAr -> &nvlArr
				new Transition (6091, 6093), // &nvlt -> &nvltr
				new Transition (6098, 6099), // &nvrA -> &nvrAr
				new Transition (6099, 6100), // &nvrAr -> &nvrArr
				new Transition (6102, 6103), // &nvrt -> &nvrtr
				new Transition (6112, 6113), // &nwa -> &nwar
				new Transition (6113, 6121), // &nwar -> &nwarr
				new Transition (6117, 6118), // &nwA -> &nwAr
				new Transition (6118, 6119), // &nwAr -> &nwArr
				new Transition (6128, 6129), // &nwnea -> &nwnear
				new Transition (6131, 6340), // &O -> &Or
				new Transition (6138, 6342), // &o -> &or
				new Transition (6149, 6150), // &oci -> &ocir
				new Transition (6153, 6154), // &Oci -> &Ocir
				new Transition (6200, 6208), // &of -> &ofr
				new Transition (6202, 6203), // &ofci -> &ofcir
				new Transition (6205, 6206), // &Of -> &Ofr
				new Transition (6210, 6220), // &og -> &ogr
				new Transition (6214, 6215), // &Og -> &Ogr
				new Transition (6229, 6230), // &ohba -> &ohbar
				new Transition (6239, 6240), // &ola -> &olar
				new Transition (6240, 6241), // &olar -> &olarr
				new Transition (6243, 6247), // &olc -> &olcr
				new Transition (6244, 6245), // &olci -> &olcir
				new Transition (6260, 6261), // &Omac -> &Omacr
				new Transition (6265, 6266), // &omac -> &omacr
				new Transition (6277, 6278), // &Omic -> &Omicr
				new Transition (6283, 6284), // &omic -> &omicr
				new Transition (6303, 6304), // &opa -> &opar
				new Transition (6310, 6311), // &OpenCu -> &OpenCur
				new Transition (6332, 6333), // &ope -> &oper
				new Transition (6344, 6345), // &ora -> &orar
				new Transition (6345, 6346), // &orar -> &orarr
				new Transition (6350, 6351), // &orde -> &order
				new Transition (6365, 6366), // &oro -> &oror
				new Transition (6379, 6380), // &Osc -> &Oscr
				new Transition (6383, 6384), // &osc -> &oscr
				new Transition (6432, 6433), // &ovba -> &ovbar
				new Transition (6436, 6437), // &Ove -> &Over
				new Transition (6438, 6442), // &OverB -> &OverBr
				new Transition (6439, 6440), // &OverBa -> &OverBar
				new Transition (6452, 6453), // &OverPa -> &OverPar
				new Transition (6463, 6642), // &p -> &pr
				new Transition (6464, 6465), // &pa -> &par
				new Transition (6482, 6640), // &P -> &Pr
				new Transition (6483, 6484), // &Pa -> &Par
				new Transition (6497, 6498), // &pe -> &per
				new Transition (6518, 6519), // &Pf -> &Pfr
				new Transition (6521, 6522), // &pf -> &pfr
				new Transition (6549, 6550), // &pitchfo -> &pitchfor
				new Transition (6571, 6572), // &plusaci -> &plusacir
				new Transition (6577, 6578), // &plusci -> &pluscir
				new Transition (6613, 6614), // &Poinca -> &Poincar
				new Transition (6659, 6660), // &precapp -> &precappr
				new Transition (6665, 6666), // &preccu -> &preccur
				new Transition (6708, 6709), // &precnapp -> &precnappr
				new Transition (6757, 6758), // &profala -> &profalar
				new Transition (6766, 6767), // &profsu -> &profsur
				new Transition (6773, 6774), // &Propo -> &Propor
				new Transition (6790, 6791), // &pru -> &prur
				new Transition (6796, 6797), // &Psc -> &Pscr
				new Transition (6800, 6801), // &psc -> &pscr
				new Transition (6814, 6815), // &Qf -> &Qfr
				new Transition (6818, 6819), // &qf -> &qfr
				new Transition (6833, 6834), // &qp -> &qpr
				new Transition (6840, 6841), // &Qsc -> &Qscr
				new Transition (6844, 6845), // &qsc -> &qscr
				new Transition (6850, 6851), // &quate -> &quater
				new Transition (6876, 7526), // &r -> &rr
				new Transition (6877, 6928), // &rA -> &rAr
				new Transition (6878, 6879), // &rAa -> &rAar
				new Transition (6879, 6880), // &rAar -> &rAarr
				new Transition (6882, 6931), // &ra -> &rar
				new Transition (6886, 7531), // &R -> &Rr
				new Transition (6887, 6925), // &Ra -> &Rar
				new Transition (6925, 6926), // &Rar -> &Rarr
				new Transition (6928, 6929), // &rAr -> &rArr
				new Transition (6931, 6932), // &rar -> &rarr
				new Transition (6987, 6988), // &RBa -> &RBar
				new Transition (6988, 6989), // &RBar -> &RBarr
				new Transition (6992, 6993), // &rBa -> &rBar
				new Transition (6993, 6994), // &rBar -> &rBarr
				new Transition (6996, 7005), // &rb -> &rbr
				new Transition (6997, 6998), // &rba -> &rbar
				new Transition (6998, 6999), // &rbar -> &rbarr
				new Transition (7001, 7002), // &rbb -> &rbbr
				new Transition (7022, 7023), // &Rca -> &Rcar
				new Transition (7028, 7029), // &rca -> &rcar
				new Transition (7060, 7061), // &rdldha -> &rdldhar
				new Transition (7065, 7067), // &rdquo -> &rdquor
				new Transition (7083, 7084), // &realpa -> &realpar
				new Transition (7098, 7099), // &Reve -> &Rever
				new Transition (7115, 7116), // &ReverseEquilib -> &ReverseEquilibr
				new Transition (7129, 7130), // &ReverseUpEquilib -> &ReverseUpEquilibr
				new Transition (7135, 7149), // &rf -> &rfr
				new Transition (7143, 7144), // &rfloo -> &rfloor
				new Transition (7146, 7147), // &Rf -> &Rfr
				new Transition (7152, 7153), // &rHa -> &rHar
				new Transition (7156, 7157), // &rha -> &rhar
				new Transition (7175, 7188), // &RightA -> &RightAr
				new Transition (7180, 7181), // &RightAngleB -> &RightAngleBr
				new Transition (7188, 7189), // &RightAr -> &RightArr
				new Transition (7193, 7194), // &Righta -> &Rightar
				new Transition (7194, 7195), // &Rightar -> &Rightarr
				new Transition (7202, 7314), // &right -> &rightr
				new Transition (7203, 7204), // &righta -> &rightar
				new Transition (7204, 7205), // &rightar -> &rightarr
				new Transition (7210, 7211), // &RightArrowBa -> &RightArrowBar
				new Transition (7217, 7218), // &RightArrowLeftA -> &RightArrowLeftAr
				new Transition (7218, 7219), // &RightArrowLeftAr -> &RightArrowLeftArr
				new Transition (7242, 7243), // &RightDoubleB -> &RightDoubleBr
				new Transition (7259, 7260), // &RightDownTeeVecto -> &RightDownTeeVector
				new Transition (7266, 7267), // &RightDownVecto -> &RightDownVector
				new Transition (7270, 7271), // &RightDownVectorBa -> &RightDownVectorBar
				new Transition (7276, 7277), // &RightFloo -> &RightFloor
				new Transition (7280, 7281), // &rightha -> &righthar
				new Transition (7298, 7299), // &rightlefta -> &rightleftar
				new Transition (7299, 7300), // &rightleftar -> &rightleftarr
				new Transition (7306, 7307), // &rightleftha -> &rightlefthar
				new Transition (7319, 7320), // &rightrighta -> &rightrightar
				new Transition (7320, 7321), // &rightrightar -> &rightrightarr
				new Transition (7331, 7332), // &rightsquiga -> &rightsquigar
				new Transition (7332, 7333), // &rightsquigar -> &rightsquigarr
				new Transition (7337, 7365), // &RightT -> &RightTr
				new Transition (7341, 7342), // &RightTeeA -> &RightTeeAr
				new Transition (7342, 7343), // &RightTeeAr -> &RightTeeArr
				new Transition (7351, 7352), // &RightTeeVecto -> &RightTeeVector
				new Transition (7355, 7356), // &rightth -> &rightthr
				new Transition (7374, 7375), // &RightTriangleBa -> &RightTriangleBar
				new Transition (7393, 7394), // &RightUpDownVecto -> &RightUpDownVector
				new Transition (7403, 7404), // &RightUpTeeVecto -> &RightUpTeeVector
				new Transition (7410, 7411), // &RightUpVecto -> &RightUpVector
				new Transition (7414, 7415), // &RightUpVectorBa -> &RightUpVectorBar
				new Transition (7421, 7422), // &RightVecto -> &RightVector
				new Transition (7425, 7426), // &RightVectorBa -> &RightVectorBar
				new Transition (7443, 7444), // &rla -> &rlar
				new Transition (7444, 7445), // &rlar -> &rlarr
				new Transition (7448, 7449), // &rlha -> &rlhar
				new Transition (7470, 7474), // &roa -> &roar
				new Transition (7474, 7475), // &roar -> &roarr
				new Transition (7477, 7478), // &rob -> &robr
				new Transition (7482, 7483), // &ropa -> &ropar
				new Transition (7513, 7514), // &rpa -> &rpar
				new Transition (7527, 7528), // &rra -> &rrar
				new Transition (7528, 7529), // &rrar -> &rrarr
				new Transition (7536, 7537), // &Rrighta -> &Rrightar
				new Transition (7537, 7538), // &Rrightar -> &Rrightarr
				new Transition (7549, 7550), // &Rsc -> &Rscr
				new Transition (7552, 7553), // &rsc -> &rscr
				new Transition (7563, 7565), // &rsquo -> &rsquor
				new Transition (7567, 7578), // &rt -> &rtr
				new Transition (7568, 7569), // &rth -> &rthr
				new Transition (7586, 7587), // &rtrilt -> &rtriltr
				new Transition (7605, 7606), // &ruluha -> &ruluhar
				new Transition (7617, 8068), // &s -> &sr
				new Transition (7633, 7641), // &sca -> &scar
				new Transition (7636, 7637), // &Sca -> &Scar
				new Transition (7662, 7663), // &Sci -> &Scir
				new Transition (7666, 7667), // &sci -> &scir
				new Transition (7704, 7705), // &sea -> &sear
				new Transition (7705, 7713), // &sear -> &searr
				new Transition (7709, 7710), // &seA -> &seAr
				new Transition (7710, 7711), // &seAr -> &seArr
				new Transition (7726, 7727), // &seswa -> &seswar
				new Transition (7741, 7742), // &Sf -> &Sfr
				new Transition (7744, 7745), // &sf -> &sfr
				new Transition (7752, 7753), // &sha -> &shar
				new Transition (7773, 7774), // &Sho -> &Shor
				new Transition (7780, 7781), // &ShortDownA -> &ShortDownAr
				new Transition (7781, 7782), // &ShortDownAr -> &ShortDownArr
				new Transition (7790, 7791), // &ShortLeftA -> &ShortLeftAr
				new Transition (7791, 7792), // &ShortLeftAr -> &ShortLeftArr
				new Transition (7796, 7797), // &sho -> &shor
				new Transition (7804, 7805), // &shortpa -> &shortpar
				new Transition (7817, 7818), // &ShortRightA -> &ShortRightAr
				new Transition (7818, 7819), // &ShortRightAr -> &ShortRightArr
				new Transition (7825, 7826), // &ShortUpA -> &ShortUpAr
				new Transition (7826, 7827), // &ShortUpAr -> &ShortUpArr
				new Transition (7847, 7873), // &sim -> &simr
				new Transition (7874, 7875), // &simra -> &simrar
				new Transition (7875, 7876), // &simrar -> &simrarr
				new Transition (7879, 7880), // &sla -> &slar
				new Transition (7880, 7881), // &slar -> &slarr
				new Transition (7888, 7889), // &SmallCi -> &SmallCir
				new Transition (7913, 7914), // &smepa -> &smepar
				new Transition (7946, 7947), // &solba -> &solbar
				new Transition (7957, 7966), // &spa -> &spar
				new Transition (7980, 7981), // &Sq -> &Sqr
				new Transition (8011, 8012), // &Squa -> &Squar
				new Transition (8015, 8016), // &squa -> &squar
				new Transition (8022, 8023), // &SquareInte -> &SquareInter
				new Transition (8046, 8047), // &SquareSupe -> &SquareSuper
				new Transition (8069, 8070), // &sra -> &srar
				new Transition (8070, 8071), // &srar -> &srarr
				new Transition (8074, 8075), // &Ssc -> &Sscr
				new Transition (8078, 8079), // &ssc -> &sscr
				new Transition (8092, 8093), // &ssta -> &sstar
				new Transition (8097, 8098), // &Sta -> &Star
				new Transition (8100, 8106), // &st -> &str
				new Transition (8101, 8102), // &sta -> &star
				new Transition (8131, 8160), // &sub -> &subr
				new Transition (8161, 8162), // &subra -> &subrar
				new Transition (8162, 8163), // &subrar -> &subrarr
				new Transition (8203, 8204), // &succapp -> &succappr
				new Transition (8209, 8210), // &succcu -> &succcur
				new Transition (8252, 8253), // &succnapp -> &succnappr
				new Transition (8308, 8309), // &Supe -> &Super
				new Transition (8329, 8330), // &supla -> &suplar
				new Transition (8330, 8331), // &suplar -> &suplarr
				new Transition (8376, 8377), // &swa -> &swar
				new Transition (8377, 8385), // &swar -> &swarr
				new Transition (8381, 8382), // &swA -> &swAr
				new Transition (8382, 8383), // &swAr -> &swArr
				new Transition (8392, 8393), // &swnwa -> &swnwar
				new Transition (8400, 8676), // &T -> &Tr
				new Transition (8404, 8628), // &t -> &tr
				new Transition (8405, 8406), // &ta -> &tar
				new Transition (8415, 8416), // &tb -> &tbr
				new Transition (8420, 8421), // &Tca -> &Tcar
				new Transition (8426, 8427), // &tca -> &tcar
				new Transition (8450, 8451), // &tel -> &telr
				new Transition (8455, 8456), // &Tf -> &Tfr
				new Transition (8458, 8459), // &tf -> &tfr
				new Transition (8462, 8463), // &the -> &ther
				new Transition (8468, 8469), // &The -> &Ther
				new Transition (8472, 8473), // &Therefo -> &Therefor
				new Transition (8477, 8478), // &therefo -> &therefor
				new Transition (8498, 8499), // &thickapp -> &thickappr
				new Transition (8540, 8541), // &tho -> &thor
				new Transition (8582, 8583), // &timesba -> &timesbar
				new Transition (8601, 8602), // &topci -> &topcir
				new Transition (8610, 8611), // &topfo -> &topfor
				new Transition (8617, 8618), // &tp -> &tpr
				new Transition (8638, 8655), // &triangle -> &triangler
				new Transition (8706, 8707), // &Tsc -> &Tscr
				new Transition (8710, 8711), // &tsc -> &tscr
				new Transition (8727, 8728), // &Tst -> &Tstr
				new Transition (8732, 8733), // &tst -> &tstr
				new Transition (8746, 8757), // &twohead -> &twoheadr
				new Transition (8751, 8752), // &twoheadlefta -> &twoheadleftar
				new Transition (8752, 8753), // &twoheadleftar -> &twoheadleftarr
				new Transition (8762, 8763), // &twoheadrighta -> &twoheadrightar
				new Transition (8763, 8764), // &twoheadrightar -> &twoheadrightarr
				new Transition (8768, 9140), // &U -> &Ur
				new Transition (8769, 8782), // &Ua -> &Uar
				new Transition (8775, 9127), // &u -> &ur
				new Transition (8776, 8789), // &ua -> &uar
				new Transition (8782, 8783), // &Uar -> &Uarr
				new Transition (8785, 8786), // &uA -> &uAr
				new Transition (8786, 8787), // &uAr -> &uArr
				new Transition (8789, 8790), // &uar -> &uarr
				new Transition (8794, 8795), // &Uarroci -> &Uarrocir
				new Transition (8797, 8798), // &Ub -> &Ubr
				new Transition (8802, 8803), // &ub -> &ubr
				new Transition (8816, 8817), // &Uci -> &Ucir
				new Transition (8821, 8822), // &uci -> &ucir
				new Transition (8830, 8831), // &uda -> &udar
				new Transition (8831, 8832), // &udar -> &udarr
				new Transition (8846, 8847), // &udha -> &udhar
				new Transition (8849, 8858), // &uf -> &ufr
				new Transition (8855, 8856), // &Uf -> &Ufr
				new Transition (8860, 8861), // &Ug -> &Ugr
				new Transition (8866, 8867), // &ug -> &ugr
				new Transition (8873, 8874), // &uHa -> &uHar
				new Transition (8877, 8878), // &uha -> &uhar
				new Transition (8878, 8881), // &uhar -> &uharr
				new Transition (8888, 8896), // &ulc -> &ulcr
				new Transition (8889, 8890), // &ulco -> &ulcor
				new Transition (8893, 8894), // &ulcorne -> &ulcorner
				new Transition (8900, 8901), // &ult -> &ultr
				new Transition (8906, 8907), // &Umac -> &Umacr
				new Transition (8911, 8912), // &umac -> &umacr
				new Transition (8918, 8919), // &Unde -> &Under
				new Transition (8920, 8924), // &UnderB -> &UnderBr
				new Transition (8921, 8922), // &UnderBa -> &UnderBar
				new Transition (8934, 8935), // &UnderPa -> &UnderPar
				new Transition (8971, 8972), // &UpA -> &UpAr
				new Transition (8972, 8973), // &UpAr -> &UpArr
				new Transition (8977, 8978), // &Upa -> &Upar
				new Transition (8978, 8979), // &Upar -> &Uparr
				new Transition (8984, 8985), // &upa -> &upar
				new Transition (8985, 8986), // &upar -> &uparr
				new Transition (8991, 8992), // &UpArrowBa -> &UpArrowBar
				new Transition (8998, 8999), // &UpArrowDownA -> &UpArrowDownAr
				new Transition (8999, 9000), // &UpArrowDownAr -> &UpArrowDownArr
				new Transition (9008, 9009), // &UpDownA -> &UpDownAr
				new Transition (9009, 9010), // &UpDownAr -> &UpDownArr
				new Transition (9018, 9019), // &Updowna -> &Updownar
				new Transition (9019, 9020), // &Updownar -> &Updownarr
				new Transition (9028, 9029), // &updowna -> &updownar
				new Transition (9029, 9030), // &updownar -> &updownarr
				new Transition (9040, 9041), // &UpEquilib -> &UpEquilibr
				new Transition (9047, 9048), // &upha -> &uphar
				new Transition (9052, 9058), // &upharpoon -> &upharpoonr
				new Transition (9069, 9070), // &Uppe -> &Upper
				new Transition (9075, 9076), // &UpperLeftA -> &UpperLeftAr
				new Transition (9076, 9077), // &UpperLeftAr -> &UpperLeftArr
				new Transition (9086, 9087), // &UpperRightA -> &UpperRightAr
				new Transition (9087, 9088), // &UpperRightAr -> &UpperRightArr
				new Transition (9112, 9113), // &UpTeeA -> &UpTeeAr
				new Transition (9113, 9114), // &UpTeeAr -> &UpTeeArr
				new Transition (9120, 9121), // &upupa -> &upupar
				new Transition (9121, 9122), // &upupar -> &upuparr
				new Transition (9128, 9136), // &urc -> &urcr
				new Transition (9129, 9130), // &urco -> &urcor
				new Transition (9133, 9134), // &urcorne -> &urcorner
				new Transition (9149, 9150), // &urt -> &urtr
				new Transition (9154, 9155), // &Usc -> &Uscr
				new Transition (9158, 9159), // &usc -> &uscr
				new Transition (9161, 9177), // &ut -> &utr
				new Transition (9183, 9184), // &uua -> &uuar
				new Transition (9184, 9185), // &uuar -> &uuarr
				new Transition (9201, 9445), // &v -> &vr
				new Transition (9202, 9208), // &va -> &var
				new Transition (9204, 9205), // &vang -> &vangr
				new Transition (9208, 9247), // &var -> &varr
				new Transition (9231, 9237), // &varp -> &varpr
				new Transition (9243, 9244), // &vA -> &vAr
				new Transition (9244, 9245), // &vAr -> &vArr
				new Transition (9279, 9285), // &vart -> &vartr
				new Transition (9291, 9297), // &vartriangle -> &vartriangler
				new Transition (9305, 9306), // &Vba -> &Vbar
				new Transition (9309, 9310), // &vBa -> &vBar
				new Transition (9342, 9360), // &Ve -> &Ver
				new Transition (9345, 9365), // &ve -> &ver
				new Transition (9349, 9350), // &veeba -> &veebar
				new Transition (9362, 9363), // &Verba -> &Verbar
				new Transition (9367, 9368), // &verba -> &verbar
				new Transition (9379, 9380), // &VerticalBa -> &VerticalBar
				new Transition (9390, 9391), // &VerticalSepa -> &VerticalSepar
				new Transition (9394, 9395), // &VerticalSeparato -> &VerticalSeparator
				new Transition (9414, 9415), // &Vf -> &Vfr
				new Transition (9417, 9418), // &vf -> &vfr
				new Transition (9421, 9422), // &vlt -> &vltr
				new Transition (9440, 9441), // &vp -> &vpr
				new Transition (9446, 9447), // &vrt -> &vrtr
				new Transition (9451, 9452), // &Vsc -> &Vscr
				new Transition (9455, 9456), // &vsc -> &vscr
				new Transition (9486, 9487), // &Wci -> &Wcir
				new Transition (9490, 9533), // &w -> &wr
				new Transition (9492, 9493), // &wci -> &wcir
				new Transition (9499, 9500), // &wedba -> &wedbar
				new Transition (9513, 9514), // &weie -> &weier
				new Transition (9517, 9518), // &Wf -> &Wfr
				new Transition (9520, 9521), // &wf -> &wfr
				new Transition (9541, 9542), // &Wsc -> &Wscr
				new Transition (9545, 9546), // &wsc -> &wscr
				new Transition (9548, 9623), // &x -> &xr
				new Transition (9553, 9554), // &xci -> &xcir
				new Transition (9561, 9562), // &xdt -> &xdtr
				new Transition (9566, 9567), // &Xf -> &Xfr
				new Transition (9569, 9570), // &xf -> &xfr
				new Transition (9573, 9574), // &xhA -> &xhAr
				new Transition (9574, 9575), // &xhAr -> &xhArr
				new Transition (9577, 9578), // &xha -> &xhar
				new Transition (9578, 9579), // &xhar -> &xharr
				new Transition (9586, 9587), // &xlA -> &xlAr
				new Transition (9587, 9588), // &xlAr -> &xlArr
				new Transition (9590, 9591), // &xla -> &xlar
				new Transition (9591, 9592), // &xlar -> &xlarr
				new Transition (9624, 9625), // &xrA -> &xrAr
				new Transition (9625, 9626), // &xrAr -> &xrArr
				new Transition (9628, 9629), // &xra -> &xrar
				new Transition (9629, 9630), // &xrar -> &xrarr
				new Transition (9633, 9634), // &Xsc -> &Xscr
				new Transition (9637, 9638), // &xsc -> &xscr
				new Transition (9651, 9652), // &xut -> &xutr
				new Transition (9686, 9687), // &Yci -> &Ycir
				new Transition (9691, 9692), // &yci -> &ycir
				new Transition (9702, 9703), // &Yf -> &Yfr
				new Transition (9705, 9706), // &yf -> &yfr
				new Transition (9725, 9726), // &Ysc -> &Yscr
				new Transition (9729, 9730), // &ysc -> &yscr
				new Transition (9762, 9763), // &Zca -> &Zcar
				new Transition (9768, 9769), // &zca -> &zcar
				new Transition (9787, 9788), // &zeet -> &zeetr
				new Transition (9791, 9792), // &Ze -> &Zer
				new Transition (9811, 9812), // &Zf -> &Zfr
				new Transition (9814, 9815), // &zf -> &zfr
				new Transition (9826, 9827), // &zig -> &zigr
				new Transition (9828, 9829), // &zigra -> &zigrar
				new Transition (9829, 9830), // &zigrar -> &zigrarr
				new Transition (9841, 9842), // &Zsc -> &Zscr
				new Transition (9845, 9846) // &zsc -> &zscr
			};
			TransitionTable_s = new Transition[368] {
				new Transition (0, 7617), // & -> &s
				new Transition (1, 247), // &A -> &As
				new Transition (8, 251), // &a -> &as
				new Transition (81, 82), // &alef -> &alefs
				new Transition (120, 128), // &and -> &ands
				new Transition (136, 172), // &ang -> &angs
				new Transition (143, 144), // &angm -> &angms
				new Transition (213, 214), // &apo -> &apos
				new Transition (247, 255), // &As -> &Ass
				new Transition (301, 744), // &b -> &bs
				new Transition (304, 324), // &back -> &backs
				new Transition (311, 312), // &backep -> &backeps
				new Transition (331, 740), // &B -> &Bs
				new Transition (334, 335), // &Back -> &Backs
				new Transition (337, 338), // &Backsla -> &Backslas
				new Transition (387, 388), // &becau -> &becaus
				new Transition (393, 394), // &Becau -> &Becaus
				new Transition (405, 406), // &bep -> &beps
				new Transition (420, 421), // &Bernoulli -> &Bernoullis
				new Transition (443, 471), // &big -> &bigs
				new Transition (462, 463), // &bigoplu -> &bigoplus
				new Transition (468, 469), // &bigotime -> &bigotimes
				new Transition (500, 501), // &biguplu -> &biguplus
				new Transition (522, 531), // &black -> &blacks
				new Transition (659, 660), // &boxminu -> &boxminus
				new Transition (664, 665), // &boxplu -> &boxplus
				new Transition (670, 671), // &boxtime -> &boxtimes
				new Transition (762, 763), // &bsolh -> &bsolhs
				new Transition (789, 1270), // &C -> &Cs
				new Transition (796, 1274), // &c -> &cs
				new Transition (805, 846), // &cap -> &caps
				new Transition (858, 859), // &Cayley -> &Cayleys
				new Transition (863, 864), // &ccap -> &ccaps
				new Transition (901, 902), // &ccup -> &ccups
				new Transition (902, 904), // &ccups -> &ccupss
				new Transition (979, 1063), // &cir -> &cirs
				new Transition (1005, 1006), // &circleda -> &circledas
				new Transition (1015, 1016), // &circledda -> &circleddas
				new Transition (1035, 1036), // &CircleMinu -> &CircleMinus
				new Transition (1040, 1041), // &CirclePlu -> &CirclePlus
				new Transition (1046, 1047), // &CircleTime -> &CircleTimes
				new Transition (1069, 1092), // &Clo -> &Clos
				new Transition (1073, 1074), // &Clockwi -> &Clockwis
				new Transition (1119, 1120), // &club -> &clubs
				new Transition (1161, 1162), // &complexe -> &complexes
				new Transition (1221, 1223), // &copy -> &copys
				new Transition (1237, 1238), // &CounterClockwi -> &CounterClockwis
				new Transition (1262, 1263), // &Cro -> &Cros
				new Transition (1263, 1264), // &Cros -> &Cross
				new Transition (1266, 1267), // &cro -> &cros
				new Transition (1267, 1268), // &cros -> &cross
				new Transition (1301, 1305), // &cue -> &cues
				new Transition (1318, 1344), // &cup -> &cups
				new Transition (1356, 1362), // &curlyeq -> &curlyeqs
				new Transition (1425, 2040), // &D -> &Ds
				new Transition (1426, 1457), // &Da -> &Das
				new Transition (1432, 2044), // &d -> &ds
				new Transition (1433, 1454), // &da -> &das
				new Transition (1511, 1512), // &ddot -> &ddots
				new Transition (1536, 1537), // &dfi -> &dfis
				new Transition (1599, 1639), // &di -> &dis
				new Transition (1601, 1617), // &diam -> &diams
				new Transition (1610, 1612), // &diamond -> &diamonds
				new Transition (1654, 1655), // &divideontime -> &divideontimes
				new Transition (1694, 1724), // &dot -> &dots
				new Transition (1716, 1717), // &dotminu -> &dotminus
				new Transition (1721, 1722), // &dotplu -> &dotplus
				new Transition (1929, 1930), // &downdownarrow -> &downdownarrows
				new Transition (2108, 2418), // &E -> &Es
				new Transition (2115, 2422), // &e -> &es
				new Transition (2116, 2122), // &ea -> &eas
				new Transition (2185, 2198), // &eg -> &egs
				new Transition (2204, 2222), // &el -> &els
				new Transition (2217, 2218), // &elinter -> &elinters
				new Transition (2233, 2279), // &em -> &ems
				new Transition (2240, 2242), // &empty -> &emptys
				new Transition (2290, 2293), // &en -> &ens
				new Transition (2312, 2323), // &ep -> &eps
				new Transition (2314, 2316), // &epar -> &epars
				new Transition (2320, 2321), // &eplu -> &eplus
				new Transition (2326, 2327), // &Ep -> &Eps
				new Transition (2339, 2350), // &eq -> &eqs
				new Transition (2363, 2364), // &eqslantle -> &eqslantles
				new Transition (2364, 2365), // &eqslantles -> &eqslantless
				new Transition (2374, 2375), // &equal -> &equals
				new Transition (2383, 2384), // &eque -> &eques
				new Transition (2405, 2406), // &eqvpar -> &eqvpars
				new Transition (2462, 2463), // &exi -> &exis
				new Transition (2467, 2468), // &Exi -> &Exis
				new Transition (2469, 2470), // &Exist -> &Exists
				new Transition (2503, 2697), // &f -> &fs
				new Transition (2512, 2513), // &fallingdot -> &fallingdots
				new Transition (2517, 2693), // &F -> &Fs
				new Transition (2601, 2602), // &fltn -> &fltns
				new Transition (2648, 2686), // &fra -> &fras
				new Transition (2701, 2927), // &g -> &gs
				new Transition (2708, 2923), // &G -> &Gs
				new Transition (2765, 2781), // &ge -> &ges
				new Transition (2771, 2775), // &geq -> &geqs
				new Transition (2796, 2797), // &gesle -> &gesles
				new Transition (2832, 2849), // &gn -> &gns
				new Transition (2879, 2880), // &GreaterEqualLe -> &GreaterEqualLes
				new Transition (2880, 2881), // &GreaterEqualLes -> &GreaterEqualLess
				new Transition (2902, 2903), // &GreaterLe -> &GreaterLes
				new Transition (2903, 2904), // &GreaterLes -> &GreaterLess
				new Transition (2961, 2962), // &gtque -> &gtques
				new Transition (2965, 2998), // &gtr -> &gtrs
				new Transition (2983, 2984), // &gtreqle -> &gtreqles
				new Transition (2984, 2985), // &gtreqles -> &gtreqless
				new Transition (2989, 2990), // &gtreqqle -> &gtreqqles
				new Transition (2990, 2991), // &gtreqqles -> &gtreqqless
				new Transition (2994, 2995), // &gtrle -> &gtrles
				new Transition (2995, 2996), // &gtrles -> &gtrless
				new Transition (3014, 3184), // &H -> &Hs
				new Transition (3020, 3188), // &h -> &hs
				new Transition (3023, 3024), // &hair -> &hairs
				new Transition (3077, 3078), // &heart -> &hearts
				new Transition (3112, 3113), // &hk -> &hks
				new Transition (3193, 3194), // &hsla -> &hslas
				new Transition (3236, 3503), // &I -> &Is
				new Transition (3243, 3507), // &i -> &is
				new Transition (3375, 3376), // &Implie -> &Implies
				new Transition (3410, 3411), // &integer -> &integers
				new Transition (3424, 3425), // &Inter -> &Inters
				new Transition (3445, 3446), // &Invi -> &Invis
				new Transition (3460, 3461), // &InvisibleTime -> &InvisibleTimes
				new Transition (3499, 3500), // &ique -> &iques
				new Transition (3512, 3520), // &isin -> &isins
				new Transition (3555, 3590), // &J -> &Js
				new Transition (3561, 3594), // &j -> &js
				new Transition (3618, 3684), // &K -> &Ks
				new Transition (3624, 3688), // &k -> &ks
				new Transition (3692, 4652), // &l -> &ls
				new Transition (3698, 4658), // &L -> &Ls
				new Transition (3766, 3785), // &larr -> &larrs
				new Transition (3770, 3771), // &larrbf -> &larrbfs
				new Transition (3773, 3774), // &larrf -> &larrfs
				new Transition (3803, 3805), // &late -> &lates
				new Transition (3828, 3831), // &lbrk -> &lbrks
				new Transition (3869, 3891), // &ld -> &lds
				new Transition (3885, 3886), // &ldru -> &ldrus
				new Transition (3896, 4197), // &le -> &les
				new Transition (3898, 4238), // &Le -> &Les
				new Transition (4027, 4028), // &leftleftarrow -> &leftleftarrows
				new Transition (4056, 4074), // &leftright -> &leftrights
				new Transition (4061, 4063), // &leftrightarrow -> &leftrightarrows
				new Transition (4071, 4072), // &leftrightharpoon -> &leftrightharpoons
				new Transition (4117, 4118), // &leftthreetime -> &leftthreetimes
				new Transition (4187, 4191), // &leq -> &leqs
				new Transition (4197, 4215), // &les -> &less
				new Transition (4212, 4213), // &lesge -> &lesges
				new Transition (4215, 4280), // &less -> &lesss
				new Transition (4238, 4239), // &Les -> &Less
				new Transition (4276, 4277), // &LessLe -> &LessLes
				new Transition (4277, 4278), // &LessLes -> &LessLess
				new Transition (4302, 4303), // &lfi -> &lfis
				new Transition (4392, 4393), // &lmou -> &lmous
				new Transition (4401, 4418), // &ln -> &lns
				new Transition (4504, 4505), // &longmap -> &longmaps
				new Transition (4570, 4571), // &loplu -> &loplus
				new Transition (4576, 4577), // &lotime -> &lotimes
				new Transition (4580, 4581), // &lowa -> &lowas
				new Transition (4717, 4718), // &ltime -> &ltimes
				new Transition (4727, 4728), // &ltque -> &ltques
				new Transition (4744, 4745), // &lurd -> &lurds
				new Transition (4767, 4941), // &m -> &ms
				new Transition (4777, 4778), // &malte -> &maltes
				new Transition (4781, 4937), // &M -> &Ms
				new Transition (4785, 4787), // &map -> &maps
				new Transition (4821, 4822), // &mda -> &mdas
				new Transition (4831, 4832), // &mea -> &meas
				new Transition (4878, 4879), // &mida -> &midas
				new Transition (4891, 4892), // &minu -> &minus
				new Transition (4902, 4903), // &Minu -> &Minus
				new Transition (4906, 4907), // &MinusPlu -> &MinusPlus
				new Transition (4919, 4920), // &mnplu -> &mnplus
				new Transition (4925, 4926), // &model -> &models
				new Transition (4947, 4948), // &mstpo -> &mstpos
				new Transition (4965, 5895), // &n -> &ns
				new Transition (4971, 5904), // &N -> &Ns
				new Transition (4993, 4994), // &napo -> &napos
				new Transition (5006, 5008), // &natural -> &naturals
				new Transition (5010, 5011), // &nb -> &nbs
				new Transition (5060, 5061), // &nda -> &ndas
				new Transition (5064, 5140), // &ne -> &nes
				new Transition (5084, 5148), // &Ne -> &Nes
				new Transition (5168, 5169), // &NestedLe -> &NestedLes
				new Transition (5169, 5170), // &NestedLes -> &NestedLess
				new Transition (5172, 5173), // &NestedLessLe -> &NestedLessLes
				new Transition (5173, 5174), // &NestedLessLes -> &NestedLessLess
				new Transition (5183, 5184), // &nexi -> &nexis
				new Transition (5185, 5187), // &nexist -> &nexists
				new Transition (5195, 5215), // &ng -> &ngs
				new Transition (5198, 5210), // &nge -> &nges
				new Transition (5200, 5204), // &ngeq -> &ngeqs
				new Transition (5240, 5242), // &ni -> &nis
				new Transition (5256, 5328), // &nl -> &nls
				new Transition (5270, 5322), // &nle -> &nles
				new Transition (5312, 5316), // &nleq -> &nleqs
				new Transition (5322, 5324), // &nles -> &nless
				new Transition (5434, 5435), // &NotExi -> &NotExis
				new Transition (5436, 5437), // &NotExist -> &NotExists
				new Transition (5472, 5473), // &NotGreaterLe -> &NotGreaterLes
				new Transition (5473, 5474), // &NotGreaterLes -> &NotGreaterLess
				new Transition (5529, 5551), // &NotLe -> &NotLes
				new Transition (5551, 5552), // &NotLes -> &NotLess
				new Transition (5569, 5570), // &NotLessLe -> &NotLessLes
				new Transition (5570, 5571), // &NotLessLes -> &NotLessLess
				new Transition (5591, 5592), // &NotNe -> &NotNes
				new Transition (5612, 5613), // &NotNestedLe -> &NotNestedLes
				new Transition (5613, 5614), // &NotNestedLes -> &NotNestedLess
				new Transition (5616, 5617), // &NotNestedLessLe -> &NotNestedLessLes
				new Transition (5617, 5618), // &NotNestedLessLes -> &NotNestedLessLess
				new Transition (5636, 5637), // &NotPrecede -> &NotPrecedes
				new Transition (5660, 5661), // &NotRever -> &NotRevers
				new Transition (5702, 5703), // &NotSquareSub -> &NotSquareSubs
				new Transition (5715, 5716), // &NotSquareSuper -> &NotSquareSupers
				new Transition (5727, 5728), // &NotSub -> &NotSubs
				new Transition (5742, 5743), // &NotSucceed -> &NotSucceeds
				new Transition (5770, 5771), // &NotSuper -> &NotSupers
				new Transition (5823, 5831), // &npar -> &npars
				new Transition (5942, 5943), // &nsq -> &nsqs
				new Transition (5952, 5958), // &nsub -> &nsubs
				new Transition (5973, 5979), // &nsup -> &nsups
				new Transition (6034, 6040), // &num -> &nums
				new Transition (6043, 6107), // &nv -> &nvs
				new Transition (6049, 6050), // &nVDa -> &nVDas
				new Transition (6054, 6055), // &nVda -> &nVdas
				new Transition (6059, 6060), // &nvDa -> &nvDas
				new Transition (6064, 6065), // &nvda -> &nvdas
				new Transition (6131, 6378), // &O -> &Os
				new Transition (6138, 6382), // &o -> &os
				new Transition (6139, 6145), // &oa -> &oas
				new Transition (6163, 6185), // &od -> &ods
				new Transition (6164, 6165), // &oda -> &odas
				new Transition (6248, 6249), // &olcro -> &olcros
				new Transition (6249, 6250), // &olcros -> &olcross
				new Transition (6291, 6292), // &ominu -> &ominus
				new Transition (6337, 6338), // &oplu -> &oplus
				new Transition (6342, 6368), // &or -> &ors
				new Transition (6387, 6388), // &Osla -> &Oslas
				new Transition (6392, 6393), // &osla -> &oslas
				new Transition (6412, 6413), // &Otime -> &Otimes
				new Transition (6416, 6417), // &otime -> &otimes
				new Transition (6419, 6420), // &otimesa -> &otimesas
				new Transition (6458, 6459), // &OverParenthe -> &OverParenthes
				new Transition (6460, 6461), // &OverParenthesi -> &OverParenthesis
				new Transition (6463, 6799), // &p -> &ps
				new Transition (6465, 6474), // &par -> &pars
				new Transition (6482, 6795), // &P -> &Ps
				new Transition (6566, 6567), // &plu -> &plus
				new Transition (6567, 6599), // &plus -> &pluss
				new Transition (6588, 6589), // &Plu -> &Plus
				new Transition (6593, 6594), // &PlusMinu -> &PlusMinus
				new Transition (6642, 6786), // &pr -> &prs
				new Transition (6655, 6721), // &prec -> &precs
				new Transition (6676, 6677), // &Precede -> &Precedes
				new Transition (6705, 6717), // &precn -> &precns
				new Transition (6731, 6733), // &prime -> &primes
				new Transition (6735, 6741), // &prn -> &prns
				new Transition (6754, 6765), // &prof -> &profs
				new Transition (6809, 6810), // &punc -> &puncs
				new Transition (6813, 6839), // &Q -> &Qs
				new Transition (6817, 6843), // &q -> &qs
				new Transition (6855, 6856), // &quaternion -> &quaternions
				new Transition (6862, 6863), // &que -> &ques
				new Transition (6876, 7542), // &r -> &rs
				new Transition (6886, 7548), // &R -> &Rs
				new Transition (6932, 6956), // &rarr -> &rarrs
				new Transition (6939, 6940), // &rarrbf -> &rarrbfs
				new Transition (6944, 6945), // &rarrf -> &rarrfs
				new Transition (6983, 6984), // &rational -> &rationals
				new Transition (7012, 7015), // &rbrk -> &rbrks
				new Transition (7053, 7069), // &rd -> &rds
				new Transition (7076, 7087), // &real -> &reals
				new Transition (7099, 7100), // &Rever -> &Revers
				new Transition (7136, 7137), // &rfi -> &rfis
				new Transition (7199, 7431), // &ri -> &ris
				new Transition (7202, 7326), // &right -> &rights
				new Transition (7302, 7303), // &rightleftarrow -> &rightleftarrows
				new Transition (7311, 7312), // &rightleftharpoon -> &rightleftharpoons
				new Transition (7323, 7324), // &rightrightarrow -> &rightrightarrows
				new Transition (7362, 7363), // &rightthreetime -> &rightthreetimes
				new Transition (7437, 7438), // &risingdot -> &risingdots
				new Transition (7455, 7456), // &rmou -> &rmous
				new Transition (7492, 7493), // &roplu -> &roplus
				new Transition (7498, 7499), // &rotime -> &rotimes
				new Transition (7509, 7510), // &RoundImplie -> &RoundImplies
				new Transition (7575, 7576), // &rtime -> &rtimes
				new Transition (7610, 8073), // &S -> &Ss
				new Transition (7617, 8077), // &s -> &ss
				new Transition (7631, 7687), // &sc -> &scs
				new Transition (7670, 7676), // &scn -> &scns
				new Transition (7703, 7724), // &se -> &ses
				new Transition (7733, 7734), // &setminu -> &setminus
				new Transition (7870, 7871), // &simplu -> &simplus
				new Transition (7895, 7907), // &sma -> &smas
				new Transition (7897, 7898), // &small -> &smalls
				new Transition (7904, 7905), // &smallsetminu -> &smallsetminus
				new Transition (7914, 7915), // &smepar -> &smepars
				new Transition (7926, 7928), // &smte -> &smtes
				new Transition (7959, 7960), // &spade -> &spades
				new Transition (7968, 7984), // &sq -> &sqs
				new Transition (7971, 7973), // &sqcap -> &sqcaps
				new Transition (7976, 7978), // &sqcup -> &sqcups
				new Transition (7986, 7990), // &sqsub -> &sqsubs
				new Transition (7997, 8001), // &sqsup -> &sqsups
				new Transition (8023, 8024), // &SquareInter -> &SquareInters
				new Transition (8034, 8035), // &SquareSub -> &SquareSubs
				new Transition (8047, 8048), // &SquareSuper -> &SquareSupers
				new Transition (8113, 8114), // &straightep -> &straighteps
				new Transition (8124, 8125), // &strn -> &strns
				new Transition (8128, 8165), // &Sub -> &Subs
				new Transition (8131, 8169), // &sub -> &subs
				new Transition (8157, 8158), // &subplu -> &subplus
				new Transition (8199, 8265), // &succ -> &succs
				new Transition (8220, 8221), // &Succeed -> &Succeeds
				new Transition (8249, 8261), // &succn -> &succns
				new Transition (8282, 8348), // &Sup -> &Sups
				new Transition (8284, 8352), // &sup -> &sups
				new Transition (8292, 8296), // &supd -> &supds
				new Transition (8309, 8310), // &Super -> &Supers
				new Transition (8320, 8321), // &suph -> &suphs
				new Transition (8345, 8346), // &supplu -> &supplus
				new Transition (8400, 8705), // &T -> &Ts
				new Transition (8404, 8709), // &t -> &ts
				new Transition (8485, 8487), // &theta -> &thetas
				new Transition (8495, 8503), // &thick -> &thicks
				new Transition (8516, 8517), // &thin -> &thins
				new Transition (8527, 8531), // &thk -> &thks
				new Transition (8577, 8578), // &time -> &times
				new Transition (8590, 8614), // &to -> &tos
				new Transition (8633, 8690), // &tri -> &tris
				new Transition (8673, 8674), // &triminu -> &triminus
				new Transition (8687, 8688), // &triplu -> &triplus
				new Transition (8768, 9153), // &U -> &Us
				new Transition (8775, 9157), // &u -> &us
				new Transition (8850, 8851), // &ufi -> &ufis
				new Transition (8940, 8941), // &UnderParenthe -> &UnderParenthes
				new Transition (8942, 8943), // &UnderParenthesi -> &UnderParenthesis
				new Transition (8951, 8952), // &UnionPlu -> &UnionPlus
				new Transition (8970, 9092), // &Up -> &Ups
				new Transition (8983, 9095), // &up -> &ups
				new Transition (9065, 9066), // &uplu -> &uplus
				new Transition (9124, 9125), // &upuparrow -> &upuparrows
				new Transition (9201, 9454), // &v -> &vs
				new Transition (9208, 9252), // &var -> &vars
				new Transition (9210, 9211), // &varep -> &vareps
				new Transition (9259, 9260), // &varsub -> &varsubs
				new Transition (9269, 9270), // &varsup -> &varsups
				new Transition (9303, 9450), // &V -> &Vs
				new Transition (9321, 9322), // &VDa -> &VDas
				new Transition (9326, 9327), // &Vda -> &Vdas
				new Transition (9331, 9332), // &vDa -> &vDas
				new Transition (9336, 9337), // &vda -> &vdas
				new Transition (9425, 9426), // &vn -> &vns
				new Transition (9473, 9474), // &Vvda -> &Vvdas
				new Transition (9484, 9540), // &W -> &Ws
				new Transition (9490, 9544), // &w -> &ws
				new Transition (9548, 9636), // &x -> &xs
				new Transition (9565, 9632), // &X -> &Xs
				new Transition (9599, 9600), // &xni -> &xnis
				new Transition (9615, 9616), // &xoplu -> &xoplus
				new Transition (9648, 9649), // &xuplu -> &xuplus
				new Transition (9665, 9724), // &Y -> &Ys
				new Transition (9672, 9728), // &y -> &ys
				new Transition (9747, 9840), // &Z -> &Zs
				new Transition (9754, 9844) // &z -> &zs
			};
			TransitionTable_t = new Transition[499] {
				new Transition (0, 8404), // & -> &t
				new Transition (1, 269), // &A -> &At
				new Transition (4, 5), // &Aacu -> &Aacut
				new Transition (8, 275), // &a -> &at
				new Transition (11, 12), // &aacu -> &aacut
				new Transition (42, 43), // &acu -> &acut
				new Transition (164, 165), // &angr -> &angrt
				new Transition (172, 176), // &angs -> &angst
				new Transition (223, 224), // &ApplyFunc -> &ApplyFunct
				new Transition (251, 260), // &as -> &ast
				new Transition (294, 295), // &awconin -> &awconint
				new Transition (298, 299), // &awin -> &awint
				new Transition (362, 364), // &bbrk -> &bbrkt
				new Transition (384, 426), // &be -> &bet
				new Transition (390, 423), // &Be -> &Bet
				new Transition (400, 401), // &bemp -> &bempt
				new Transition (443, 481), // &big -> &bigt
				new Transition (455, 465), // &bigo -> &bigot
				new Transition (457, 458), // &bigodo -> &bigodot
				new Transition (471, 477), // &bigs -> &bigst
				new Transition (522, 538), // &black -> &blackt
				new Transition (554, 555), // &blacktrianglelef -> &blacktriangleleft
				new Transition (560, 561), // &blacktrianglerigh -> &blacktriangleright
				new Transition (588, 589), // &bNo -> &bNot
				new Transition (591, 592), // &bno -> &bnot
				new Transition (598, 602), // &bo -> &bot
				new Transition (602, 604), // &bot -> &bott
				new Transition (608, 609), // &bow -> &bowt
				new Transition (613, 667), // &box -> &boxt
				new Transition (771, 772), // &bulle -> &bullet
				new Transition (792, 793), // &Cacu -> &Cacut
				new Transition (796, 1287), // &c -> &ct
				new Transition (799, 800), // &cacu -> &cacut
				new Transition (825, 826), // &capdo -> &capdot
				new Transition (828, 829), // &Capi -> &Capit
				new Transition (839, 840), // &CapitalDifferen -> &CapitalDifferent
				new Transition (849, 850), // &care -> &caret
				new Transition (897, 898), // &Cconin -> &Cconint
				new Transition (908, 909), // &Cdo -> &Cdot
				new Transition (912, 913), // &cdo -> &cdot
				new Transition (928, 929), // &cemp -> &cempt
				new Transition (933, 934), // &cen -> &cent
				new Transition (936, 937), // &Cen -> &Cent
				new Transition (941, 942), // &CenterDo -> &CenterDot
				new Transition (947, 948), // &centerdo -> &centerdot
				new Transition (995, 996), // &circlearrowlef -> &circlearrowleft
				new Transition (1001, 1002), // &circlearrowrigh -> &circlearrowright
				new Transition (1006, 1007), // &circledas -> &circledast
				new Transition (1025, 1026), // &CircleDo -> &CircleDot
				new Transition (1056, 1057), // &cirfnin -> &cirfnint
				new Transition (1078, 1079), // &ClockwiseCon -> &ClockwiseCont
				new Transition (1084, 1085), // &ClockwiseContourIn -> &ClockwiseContourInt
				new Transition (1107, 1108), // &CloseCurlyDoubleQuo -> &CloseCurlyDoubleQuot
				new Transition (1113, 1114), // &CloseCurlyQuo -> &CloseCurlyQuot
				new Transition (1123, 1124), // &clubsui -> &clubsuit
				new Transition (1144, 1146), // &comma -> &commat
				new Transition (1157, 1158), // &complemen -> &complement
				new Transition (1168, 1169), // &congdo -> &congdot
				new Transition (1171, 1187), // &Con -> &Cont
				new Transition (1176, 1177), // &Congruen -> &Congruent
				new Transition (1180, 1181), // &Conin -> &Conint
				new Transition (1184, 1185), // &conin -> &conint
				new Transition (1192, 1193), // &ContourIn -> &ContourInt
				new Transition (1214, 1215), // &Coproduc -> &Coproduct
				new Transition (1227, 1228), // &Coun -> &Count
				new Transition (1242, 1243), // &CounterClockwiseCon -> &CounterClockwiseCont
				new Transition (1248, 1249), // &CounterClockwiseContourIn -> &CounterClockwiseContourInt
				new Transition (1289, 1290), // &ctdo -> &ctdot
				new Transition (1338, 1339), // &cupdo -> &cupdot
				new Transition (1390, 1391), // &curvearrowlef -> &curvearrowleft
				new Transition (1396, 1397), // &curvearrowrigh -> &curvearrowright
				new Transition (1412, 1413), // &cwconin -> &cwconint
				new Transition (1416, 1417), // &cwin -> &cwint
				new Transition (1421, 1422), // &cylc -> &cylct
				new Transition (1432, 2067), // &d -> &dt
				new Transition (1440, 1441), // &dale -> &dalet
				new Transition (1503, 1504), // &DDo -> &DDot
				new Transition (1510, 1511), // &ddo -> &ddot
				new Transition (1520, 1522), // &Del -> &Delt
				new Transition (1525, 1526), // &del -> &delt
				new Transition (1530, 1531), // &demp -> &dempt
				new Transition (1538, 1539), // &dfish -> &dfisht
				new Transition (1561, 1562), // &Diacri -> &Diacrit
				new Transition (1569, 1570), // &DiacriticalAcu -> &DiacriticalAcut
				new Transition (1574, 1575), // &DiacriticalDo -> &DiacriticalDot
				new Transition (1583, 1584), // &DiacriticalDoubleAcu -> &DiacriticalDoubleAcut
				new Transition (1614, 1615), // &diamondsui -> &diamondsuit
				new Transition (1626, 1627), // &Differen -> &Different
				new Transition (1650, 1651), // &divideon -> &divideont
				new Transition (1679, 1694), // &do -> &dot
				new Transition (1685, 1692), // &Do -> &Dot
				new Transition (1697, 1698), // &DotDo -> &DotDot
				new Transition (1704, 1705), // &doteqdo -> &doteqdot
				new Transition (1750, 1751), // &DoubleCon -> &DoubleCont
				new Transition (1756, 1757), // &DoubleContourIn -> &DoubleContourInt
				new Transition (1765, 1766), // &DoubleDo -> &DoubleDot
				new Transition (1778, 1779), // &DoubleLef -> &DoubleLeft
				new Transition (1789, 1790), // &DoubleLeftRigh -> &DoubleLeftRight
				new Transition (1806, 1807), // &DoubleLongLef -> &DoubleLongLeft
				new Transition (1817, 1818), // &DoubleLongLeftRigh -> &DoubleLongLeftRight
				new Transition (1828, 1829), // &DoubleLongRigh -> &DoubleLongRight
				new Transition (1839, 1840), // &DoubleRigh -> &DoubleRight
				new Transition (1871, 1872), // &DoubleVer -> &DoubleVert
				new Transition (1941, 1942), // &downharpoonlef -> &downharpoonleft
				new Transition (1947, 1948), // &downharpoonrigh -> &downharpoonright
				new Transition (1952, 1953), // &DownLef -> &DownLeft
				new Transition (1957, 1958), // &DownLeftRigh -> &DownLeftRight
				new Transition (1961, 1962), // &DownLeftRightVec -> &DownLeftRightVect
				new Transition (1971, 1972), // &DownLeftTeeVec -> &DownLeftTeeVect
				new Transition (1978, 1979), // &DownLeftVec -> &DownLeftVect
				new Transition (1990, 1991), // &DownRigh -> &DownRight
				new Transition (1997, 1998), // &DownRightTeeVec -> &DownRightTeeVect
				new Transition (2004, 2005), // &DownRightVec -> &DownRightVect
				new Transition (2040, 2057), // &Ds -> &Dst
				new Transition (2044, 2062), // &ds -> &dst
				new Transition (2069, 2070), // &dtdo -> &dtdot
				new Transition (2108, 2436), // &E -> &Et
				new Transition (2111, 2112), // &Eacu -> &Eacut
				new Transition (2115, 2439), // &e -> &et
				new Transition (2118, 2119), // &eacu -> &eacut
				new Transition (2122, 2123), // &eas -> &east
				new Transition (2159, 2160), // &eDDo -> &eDDot
				new Transition (2163, 2164), // &Edo -> &Edot
				new Transition (2166, 2167), // &eDo -> &eDot
				new Transition (2170, 2171), // &edo -> &edot
				new Transition (2177, 2178), // &efDo -> &efDot
				new Transition (2201, 2202), // &egsdo -> &egsdot
				new Transition (2210, 2211), // &Elemen -> &Element
				new Transition (2214, 2215), // &elin -> &elint
				new Transition (2225, 2226), // &elsdo -> &elsdot
				new Transition (2238, 2239), // &emp -> &empt
				new Transition (2243, 2244), // &emptyse -> &emptyset
				new Transition (2246, 2247), // &Emp -> &Empt
				new Transition (2356, 2357), // &eqslan -> &eqslant
				new Transition (2358, 2359), // &eqslantg -> &eqslantgt
				new Transition (2384, 2385), // &eques -> &equest
				new Transition (2415, 2416), // &erDo -> &erDot
				new Transition (2427, 2428), // &esdo -> &esdot
				new Transition (2463, 2464), // &exis -> &exist
				new Transition (2468, 2469), // &Exis -> &Exist
				new Transition (2474, 2475), // &expec -> &expect
				new Transition (2476, 2477), // &expecta -> &expectat
				new Transition (2486, 2487), // &Exponen -> &Exponent
				new Transition (2496, 2497), // &exponen -> &exponent
				new Transition (2511, 2512), // &fallingdo -> &fallingdot
				new Transition (2592, 2600), // &fl -> &flt
				new Transition (2593, 2594), // &fla -> &flat
				new Transition (2634, 2635), // &Fourier -> &Fouriert
				new Transition (2641, 2642), // &fpar -> &fpart
				new Transition (2644, 2645), // &fpartin -> &fpartint
				new Transition (2701, 2942), // &g -> &gt
				new Transition (2704, 2705), // &gacu -> &gacut
				new Transition (2708, 2940), // &G -> &Gt
				new Transition (2756, 2757), // &Gdo -> &Gdot
				new Transition (2760, 2761), // &gdo -> &gdot
				new Transition (2778, 2779), // &geqslan -> &geqslant
				new Transition (2787, 2788), // &gesdo -> &gesdot
				new Transition (2868, 2869), // &Grea -> &Great
				new Transition (2896, 2897), // &GreaterGrea -> &GreaterGreat
				new Transition (2909, 2910), // &GreaterSlan -> &GreaterSlant
				new Transition (2951, 2952), // &gtdo -> &gtdot
				new Transition (2962, 2963), // &gtques -> &gtquest
				new Transition (2977, 2978), // &gtrdo -> &gtrdot
				new Transition (3004, 3005), // &gver -> &gvert
				new Transition (3015, 3058), // &Ha -> &Hat
				new Transition (3032, 3033), // &hamil -> &hamilt
				new Transition (3076, 3077), // &hear -> &heart
				new Transition (3081, 3082), // &heartsui -> &heartsuit
				new Transition (3104, 3105), // &Hilber -> &Hilbert
				new Transition (3131, 3132), // &hom -> &homt
				new Transition (3133, 3134), // &homth -> &homtht
				new Transition (3140, 3141), // &hooklef -> &hookleft
				new Transition (3151, 3152), // &hookrigh -> &hookright
				new Transition (3175, 3176), // &Horizon -> &Horizont
				new Transition (3184, 3197), // &Hs -> &Hst
				new Transition (3188, 3202), // &hs -> &hst
				new Transition (3236, 3528), // &I -> &It
				new Transition (3239, 3240), // &Iacu -> &Iacut
				new Transition (3243, 3526), // &i -> &it
				new Transition (3246, 3247), // &iacu -> &iacut
				new Transition (3266, 3267), // &Ido -> &Idot
				new Transition (3305, 3306), // &iiiin -> &iiiint
				new Transition (3308, 3309), // &iiin -> &iiint
				new Transition (3316, 3317), // &iio -> &iiot
				new Transition (3337, 3362), // &ima -> &imat
				new Transition (3359, 3360), // &imagpar -> &imagpart
				new Transition (3378, 3401), // &in -> &int
				new Transition (3387, 3389), // &infin -> &infint
				new Transition (3395, 3396), // &inodo -> &inodot
				new Transition (3398, 3399), // &In -> &Int
				new Transition (3427, 3428), // &Intersec -> &Intersect
				new Transition (3467, 3489), // &io -> &iot
				new Transition (3471, 3486), // &Io -> &Iot
				new Transition (3500, 3501), // &iques -> &iquest
				new Transition (3515, 3516), // &isindo -> &isindot
				new Transition (3578, 3579), // &jma -> &jmat
				new Transition (3692, 4698), // &l -> &lt
				new Transition (3693, 3794), // &lA -> &lAt
				new Transition (3698, 4696), // &L -> &Lt
				new Transition (3701, 3702), // &Lacu -> &Lacut
				new Transition (3705, 3792), // &la -> &lat
				new Transition (3707, 3708), // &lacu -> &lacut
				new Transition (3713, 3714), // &laemp -> &laempt
				new Transition (3750, 3751), // &Laplace -> &Laplacet
				new Transition (3766, 3789), // &larr -> &larrt
				new Transition (3899, 3900), // &Lef -> &Left
				new Transition (3911, 3912), // &LeftAngleBracke -> &LeftAngleBracket
				new Transition (3925, 3926), // &lef -> &left
				new Transition (3926, 4109), // &left -> &leftt
				new Transition (3931, 3948), // &leftarrow -> &leftarrowt
				new Transition (3940, 3941), // &LeftArrowRigh -> &LeftArrowRight
				new Transition (3972, 3973), // &LeftDoubleBracke -> &LeftDoubleBracket
				new Transition (3982, 3983), // &LeftDownTeeVec -> &LeftDownTeeVect
				new Transition (3989, 3990), // &LeftDownVec -> &LeftDownVect
				new Transition (4021, 4022), // &leftlef -> &leftleft
				new Transition (4033, 4034), // &LeftRigh -> &LeftRight
				new Transition (4044, 4045), // &Leftrigh -> &Leftright
				new Transition (4055, 4056), // &leftrigh -> &leftright
				new Transition (4087, 4088), // &LeftRightVec -> &LeftRightVect
				new Transition (4104, 4105), // &LeftTeeVec -> &LeftTeeVect
				new Transition (4113, 4114), // &leftthree -> &leftthreet
				new Transition (4146, 4147), // &LeftUpDownVec -> &LeftUpDownVect
				new Transition (4156, 4157), // &LeftUpTeeVec -> &LeftUpTeeVect
				new Transition (4163, 4164), // &LeftUpVec -> &LeftUpVect
				new Transition (4174, 4175), // &LeftVec -> &LeftVect
				new Transition (4194, 4195), // &leqslan -> &leqslant
				new Transition (4203, 4204), // &lesdo -> &lesdot
				new Transition (4224, 4225), // &lessdo -> &lessdot
				new Transition (4229, 4230), // &lesseqg -> &lesseqgt
				new Transition (4234, 4235), // &lesseqqg -> &lesseqqgt
				new Transition (4248, 4249), // &LessEqualGrea -> &LessEqualGreat
				new Transition (4266, 4267), // &LessGrea -> &LessGreat
				new Transition (4271, 4272), // &lessg -> &lessgt
				new Transition (4287, 4288), // &LessSlan -> &LessSlant
				new Transition (4304, 4305), // &lfish -> &lfisht
				new Transition (4348, 4375), // &ll -> &llt
				new Transition (4362, 4363), // &Llef -> &Lleft
				new Transition (4382, 4383), // &Lmido -> &Lmidot
				new Transition (4388, 4389), // &lmido -> &lmidot
				new Transition (4393, 4394), // &lmous -> &lmoust
				new Transition (4422, 4573), // &lo -> &lot
				new Transition (4439, 4440), // &LongLef -> &LongLeft
				new Transition (4449, 4450), // &Longlef -> &Longleft
				new Transition (4461, 4462), // &longlef -> &longleft
				new Transition (4472, 4473), // &LongLeftRigh -> &LongLeftRight
				new Transition (4483, 4484), // &Longleftrigh -> &Longleftright
				new Transition (4494, 4495), // &longleftrigh -> &longleftright
				new Transition (4505, 4506), // &longmaps -> &longmapst
				new Transition (4512, 4513), // &LongRigh -> &LongRight
				new Transition (4523, 4524), // &Longrigh -> &Longright
				new Transition (4534, 4535), // &longrigh -> &longright
				new Transition (4551, 4552), // &looparrowlef -> &looparrowleft
				new Transition (4557, 4558), // &looparrowrigh -> &looparrowright
				new Transition (4581, 4582), // &lowas -> &lowast
				new Transition (4593, 4594), // &LowerLef -> &LowerLeft
				new Transition (4604, 4605), // &LowerRigh -> &LowerRight
				new Transition (4625, 4626), // &lparl -> &lparlt
				new Transition (4628, 4648), // &lr -> &lrt
				new Transition (4652, 4689), // &ls -> &lst
				new Transition (4658, 4684), // &Ls -> &Lst
				new Transition (4707, 4708), // &ltdo -> &ltdot
				new Transition (4728, 4729), // &ltques -> &ltquest
				new Transition (4757, 4758), // &lver -> &lvert
				new Transition (4772, 4775), // &mal -> &malt
				new Transition (4787, 4788), // &maps -> &mapst
				new Transition (4798, 4799), // &mapstolef -> &mapstoleft
				new Transition (4827, 4828), // &mDDo -> &mDDot
				new Transition (4857, 4858), // &Mellin -> &Mellint
				new Transition (4879, 4880), // &midas -> &midast
				new Transition (4887, 4888), // &middo -> &middot
				new Transition (4941, 4945), // &ms -> &mst
				new Transition (4954, 4955), // &mul -> &mult
				new Transition (4965, 5988), // &n -> &nt
				new Transition (4966, 5001), // &na -> &nat
				new Transition (4971, 5992), // &N -> &Nt
				new Transition (4974, 4975), // &Nacu -> &Nacut
				new Transition (4979, 4980), // &nacu -> &nacut
				new Transition (5049, 5050), // &ncongdo -> &ncongdot
				new Transition (5081, 5082), // &nedo -> &nedot
				new Transition (5086, 5087), // &Nega -> &Negat
				new Transition (5148, 5149), // &Nes -> &Nest
				new Transition (5155, 5156), // &NestedGrea -> &NestedGreat
				new Transition (5162, 5163), // &NestedGreaterGrea -> &NestedGreaterGreat
				new Transition (5184, 5185), // &nexis -> &nexist
				new Transition (5195, 5221), // &ng -> &ngt
				new Transition (5207, 5208), // &ngeqslan -> &ngeqslant
				new Transition (5212, 5219), // &nG -> &nGt
				new Transition (5256, 5334), // &nl -> &nlt
				new Transition (5272, 5332), // &nL -> &nLt
				new Transition (5274, 5275), // &nLef -> &nLeft
				new Transition (5282, 5283), // &nlef -> &nleft
				new Transition (5293, 5294), // &nLeftrigh -> &nLeftright
				new Transition (5304, 5305), // &nleftrigh -> &nleftright
				new Transition (5319, 5320), // &nleqslan -> &nleqslant
				new Transition (5347, 5376), // &No -> &Not
				new Transition (5372, 5378), // &no -> &not
				new Transition (5387, 5388), // &NotCongruen -> &NotCongruent
				new Transition (5404, 5405), // &NotDoubleVer -> &NotDoubleVert
				new Transition (5419, 5420), // &NotElemen -> &NotElement
				new Transition (5435, 5436), // &NotExis -> &NotExist
				new Transition (5442, 5443), // &NotGrea -> &NotGreat
				new Transition (5466, 5467), // &NotGreaterGrea -> &NotGreaterGreat
				new Transition (5479, 5480), // &NotGreaterSlan -> &NotGreaterSlant
				new Transition (5516, 5517), // &notindo -> &notindot
				new Transition (5530, 5531), // &NotLef -> &NotLeft
				new Transition (5563, 5564), // &NotLessGrea -> &NotLessGreat
				new Transition (5576, 5577), // &NotLessSlan -> &NotLessSlant
				new Transition (5592, 5593), // &NotNes -> &NotNest
				new Transition (5599, 5600), // &NotNestedGrea -> &NotNestedGreat
				new Transition (5606, 5607), // &NotNestedGreaterGrea -> &NotNestedGreaterGreat
				new Transition (5648, 5649), // &NotPrecedesSlan -> &NotPrecedesSlant
				new Transition (5668, 5669), // &NotReverseElemen -> &NotReverseElement
				new Transition (5673, 5674), // &NotRigh -> &NotRight
				new Transition (5704, 5705), // &NotSquareSubse -> &NotSquareSubset
				new Transition (5717, 5718), // &NotSquareSuperse -> &NotSquareSuperset
				new Transition (5729, 5730), // &NotSubse -> &NotSubset
				new Transition (5754, 5755), // &NotSucceedsSlan -> &NotSucceedsSlant
				new Transition (5772, 5773), // &NotSuperse -> &NotSuperset
				new Transition (5811, 5812), // &NotVer -> &NotVert
				new Transition (5823, 5834), // &npar -> &npart
				new Transition (5839, 5840), // &npolin -> &npolint
				new Transition (5855, 5889), // &nr -> &nrt
				new Transition (5871, 5872), // &nRigh -> &nRight
				new Transition (5881, 5882), // &nrigh -> &nright
				new Transition (5912, 5913), // &nshor -> &nshort
				new Transition (5959, 5960), // &nsubse -> &nsubset
				new Transition (5980, 5981), // &nsupse -> &nsupset
				new Transition (6015, 6016), // &ntrianglelef -> &ntriangleleft
				new Transition (6024, 6025), // &ntrianglerigh -> &ntriangleright
				new Transition (6068, 6071), // &nvg -> &nvgt
				new Transition (6084, 6091), // &nvl -> &nvlt
				new Transition (6097, 6102), // &nvr -> &nvrt
				new Transition (6131, 6399), // &O -> &Ot
				new Transition (6134, 6135), // &Oacu -> &Oacut
				new Transition (6138, 6405), // &o -> &ot
				new Transition (6141, 6142), // &oacu -> &oacut
				new Transition (6145, 6146), // &oas -> &oast
				new Transition (6182, 6183), // &odo -> &odot
				new Transition (6210, 6225), // &og -> &ogt
				new Transition (6235, 6236), // &oin -> &oint
				new Transition (6238, 6256), // &ol -> &olt
				new Transition (6322, 6323), // &OpenCurlyDoubleQuo -> &OpenCurlyDoubleQuot
				new Transition (6328, 6329), // &OpenCurlyQuo -> &OpenCurlyQuot
				new Transition (6448, 6449), // &OverBracke -> &OverBracket
				new Transition (6455, 6456), // &OverParen -> &OverParent
				new Transition (6465, 6480), // &par -> &part
				new Transition (6484, 6485), // &Par -> &Part
				new Transition (6498, 6513), // &per -> &pert
				new Transition (6500, 6501), // &percn -> &percnt
				new Transition (6534, 6535), // &phmma -> &phmmat
				new Transition (6543, 6545), // &pi -> &pit
				new Transition (6567, 6603), // &plus -> &plust
				new Transition (6624, 6625), // &poin -> &point
				new Transition (6627, 6628), // &pointin -> &pointint
				new Transition (6688, 6689), // &PrecedesSlan -> &PrecedesSlant
				new Transition (6751, 6752), // &Produc -> &Product
				new Transition (6770, 6783), // &prop -> &propt
				new Transition (6774, 6775), // &Propor -> &Proport
				new Transition (6822, 6823), // &qin -> &qint
				new Transition (6848, 6849), // &qua -> &quat
				new Transition (6859, 6860), // &quatin -> &quatint
				new Transition (6863, 6864), // &ques -> &quest
				new Transition (6873, 6874), // &quo -> &quot
				new Transition (6876, 7567), // &r -> &rt
				new Transition (6877, 6968), // &rA -> &rAt
				new Transition (6882, 6973), // &ra -> &rat
				new Transition (6889, 6890), // &Racu -> &Racut
				new Transition (6893, 6894), // &racu -> &racut
				new Transition (6903, 6904), // &raemp -> &raempt
				new Transition (6926, 6960), // &Rarr -> &Rarrt
				new Transition (6932, 6963), // &rarr -> &rarrt
				new Transition (7084, 7085), // &realpar -> &realpart
				new Transition (7089, 7090), // &rec -> &rect
				new Transition (7107, 7108), // &ReverseElemen -> &ReverseElement
				new Transition (7138, 7139), // &rfish -> &rfisht
				new Transition (7173, 7174), // &Righ -> &Right
				new Transition (7185, 7186), // &RightAngleBracke -> &RightAngleBracket
				new Transition (7201, 7202), // &righ -> &right
				new Transition (7202, 7354), // &right -> &rightt
				new Transition (7207, 7223), // &rightarrow -> &rightarrowt
				new Transition (7215, 7216), // &RightArrowLef -> &RightArrowLeft
				new Transition (7247, 7248), // &RightDoubleBracke -> &RightDoubleBracket
				new Transition (7257, 7258), // &RightDownTeeVec -> &RightDownTeeVect
				new Transition (7264, 7265), // &RightDownVec -> &RightDownVect
				new Transition (7296, 7297), // &rightlef -> &rightleft
				new Transition (7317, 7318), // &rightrigh -> &rightright
				new Transition (7349, 7350), // &RightTeeVec -> &RightTeeVect
				new Transition (7358, 7359), // &rightthree -> &rightthreet
				new Transition (7391, 7392), // &RightUpDownVec -> &RightUpDownVect
				new Transition (7401, 7402), // &RightUpTeeVec -> &RightUpTeeVect
				new Transition (7408, 7409), // &RightUpVec -> &RightUpVect
				new Transition (7419, 7420), // &RightVec -> &RightVect
				new Transition (7436, 7437), // &risingdo -> &risingdot
				new Transition (7456, 7457), // &rmous -> &rmoust
				new Transition (7469, 7495), // &ro -> &rot
				new Transition (7516, 7517), // &rparg -> &rpargt
				new Transition (7523, 7524), // &rppolin -> &rppolint
				new Transition (7534, 7535), // &Rrigh -> &Rright
				new Transition (7585, 7586), // &rtril -> &rtrilt
				new Transition (7610, 8096), // &S -> &St
				new Transition (7613, 7614), // &Sacu -> &Sacut
				new Transition (7617, 8100), // &s -> &st
				new Transition (7620, 7621), // &sacu -> &sacut
				new Transition (7684, 7685), // &scpolin -> &scpolint
				new Transition (7696, 7697), // &sdo -> &sdot
				new Transition (7703, 7729), // &se -> &set
				new Transition (7718, 7719), // &sec -> &sect
				new Transition (7738, 7739), // &sex -> &sext
				new Transition (7774, 7775), // &Shor -> &Short
				new Transition (7788, 7789), // &ShortLef -> &ShortLeft
				new Transition (7797, 7798), // &shor -> &short
				new Transition (7815, 7816), // &ShortRigh -> &ShortRight
				new Transition (7850, 7851), // &simdo -> &simdot
				new Transition (7894, 7924), // &sm -> &smt
				new Transition (7899, 7900), // &smallse -> &smallset
				new Transition (7937, 7938), // &sof -> &soft
				new Transition (7963, 7964), // &spadesui -> &spadesuit
				new Transition (7981, 7982), // &Sqr -> &Sqrt
				new Transition (7991, 7992), // &sqsubse -> &sqsubset
				new Transition (8002, 8003), // &sqsupse -> &sqsupset
				new Transition (8020, 8021), // &SquareIn -> &SquareInt
				new Transition (8026, 8027), // &SquareIntersec -> &SquareIntersect
				new Transition (8036, 8037), // &SquareSubse -> &SquareSubset
				new Transition (8049, 8050), // &SquareSuperse -> &SquareSuperset
				new Transition (8077, 8091), // &ss -> &sst
				new Transition (8081, 8082), // &sse -> &sset
				new Transition (8110, 8111), // &straigh -> &straight
				new Transition (8134, 8135), // &subdo -> &subdot
				new Transition (8142, 8143), // &subedo -> &subedot
				new Transition (8147, 8148), // &submul -> &submult
				new Transition (8166, 8167), // &Subse -> &Subset
				new Transition (8170, 8171), // &subse -> &subset
				new Transition (8232, 8233), // &SucceedsSlan -> &SucceedsSlant
				new Transition (8272, 8273), // &SuchTha -> &SuchThat
				new Transition (8293, 8294), // &supdo -> &supdot
				new Transition (8305, 8306), // &supedo -> &supedot
				new Transition (8311, 8312), // &Superse -> &Superset
				new Transition (8335, 8336), // &supmul -> &supmult
				new Transition (8349, 8350), // &Supse -> &Supset
				new Transition (8353, 8354), // &supse -> &supset
				new Transition (8408, 8409), // &targe -> &target
				new Transition (8446, 8447), // &tdo -> &tdot
				new Transition (8462, 8484), // &the -> &thet
				new Transition (8468, 8481), // &The -> &Thet
				new Transition (8587, 8588), // &tin -> &tint
				new Transition (8597, 8598), // &topbo -> &topbot
				new Transition (8633, 8693), // &tri -> &trit
				new Transition (8647, 8648), // &trianglelef -> &triangleleft
				new Transition (8658, 8659), // &trianglerigh -> &triangleright
				new Transition (8665, 8666), // &trido -> &tridot
				new Transition (8682, 8683), // &TripleDo -> &TripleDot
				new Transition (8705, 8727), // &Ts -> &Tst
				new Transition (8709, 8732), // &ts -> &tst
				new Transition (8739, 8740), // &twix -> &twixt
				new Transition (8749, 8750), // &twoheadlef -> &twoheadleft
				new Transition (8760, 8761), // &twoheadrigh -> &twoheadright
				new Transition (8768, 9166), // &U -> &Ut
				new Transition (8771, 8772), // &Uacu -> &Uacut
				new Transition (8775, 9161), // &u -> &ut
				new Transition (8778, 8779), // &uacu -> &uacut
				new Transition (8852, 8853), // &ufish -> &ufisht
				new Transition (8887, 8900), // &ul -> &ult
				new Transition (8930, 8931), // &UnderBracke -> &UnderBracket
				new Transition (8937, 8938), // &UnderParen -> &UnderParent
				new Transition (9055, 9056), // &upharpoonlef -> &upharpoonleft
				new Transition (9061, 9062), // &upharpoonrigh -> &upharpoonright
				new Transition (9073, 9074), // &UpperLef -> &UpperLeft
				new Transition (9084, 9085), // &UpperRigh -> &UpperRight
				new Transition (9127, 9149), // &ur -> &urt
				new Transition (9163, 9164), // &utdo -> &utdot
				new Transition (9205, 9206), // &vangr -> &vangrt
				new Transition (9208, 9279), // &var -> &vart
				new Transition (9224, 9225), // &varno -> &varnot
				new Transition (9239, 9240), // &varprop -> &varpropt
				new Transition (9261, 9262), // &varsubse -> &varsubset
				new Transition (9271, 9272), // &varsupse -> &varsupset
				new Transition (9281, 9282), // &varthe -> &varthet
				new Transition (9294, 9295), // &vartrianglelef -> &vartriangleleft
				new Transition (9300, 9301), // &vartrianglerigh -> &vartriangleright
				new Transition (9360, 9370), // &Ver -> &Vert
				new Transition (9365, 9372), // &ver -> &vert
				new Transition (9392, 9393), // &VerticalSepara -> &VerticalSeparat
				new Transition (9420, 9421), // &vl -> &vlt
				new Transition (9445, 9446), // &vr -> &vrt
				new Transition (9536, 9537), // &wrea -> &wreat
				new Transition (9560, 9561), // &xd -> &xdt
				new Transition (9602, 9618), // &xo -> &xot
				new Transition (9604, 9605), // &xodo -> &xodot
				new Transition (9645, 9651), // &xu -> &xut
				new Transition (9668, 9669), // &Yacu -> &Yacut
				new Transition (9675, 9676), // &yacu -> &yacut
				new Transition (9750, 9751), // &Zacu -> &Zacut
				new Transition (9757, 9758), // &zacu -> &zacut
				new Transition (9778, 9779), // &Zdo -> &Zdot
				new Transition (9782, 9783), // &zdo -> &zdot
				new Transition (9785, 9808), // &ze -> &zet
				new Transition (9786, 9787), // &zee -> &zeet
				new Transition (9791, 9805), // &Ze -> &Zet
				new Transition (9796, 9797) // &ZeroWid -> &ZeroWidt
			};
			TransitionTable_u = new Transition[278] {
				new Transition (0, 8775), // & -> &u
				new Transition (1, 281), // &A -> &Au
				new Transition (3, 4), // &Aac -> &Aacu
				new Transition (8, 285), // &a -> &au
				new Transition (10, 11), // &aac -> &aacu
				new Transition (27, 42), // &ac -> &acu
				new Transition (220, 221), // &ApplyF -> &ApplyFu
				new Transition (301, 767), // &b -> &bu
				new Transition (331, 781), // &B -> &Bu
				new Transition (380, 381), // &bdq -> &bdqu
				new Transition (386, 387), // &beca -> &becau
				new Transition (392, 393), // &Beca -> &Becau
				new Transition (411, 412), // &berno -> &bernou
				new Transition (416, 417), // &Berno -> &Bernou
				new Transition (443, 497), // &big -> &bigu
				new Transition (444, 452), // &bigc -> &bigcu
				new Transition (461, 462), // &bigopl -> &bigoplu
				new Transition (473, 474), // &bigsqc -> &bigsqcu
				new Transition (488, 494), // &bigtriangle -> &bigtriangleu
				new Transition (499, 500), // &bigupl -> &biguplu
				new Transition (532, 533), // &blacksq -> &blacksqu
				new Transition (582, 583), // &bneq -> &bnequ
				new Transition (613, 678), // &box -> &boxu
				new Transition (636, 650), // &boxH -> &boxHu
				new Transition (638, 654), // &boxh -> &boxhu
				new Transition (658, 659), // &boxmin -> &boxminu
				new Transition (663, 664), // &boxpl -> &boxplu
				new Transition (763, 764), // &bsolhs -> &bsolhsu
				new Transition (789, 1315), // &C -> &Cu
				new Transition (791, 792), // &Cac -> &Cacu
				new Transition (796, 1292), // &c -> &cu
				new Transition (798, 799), // &cac -> &cacu
				new Transition (813, 814), // &capbrc -> &capbrcu
				new Transition (817, 821), // &capc -> &capcu
				new Transition (861, 900), // &cc -> &ccu
				new Transition (1034, 1035), // &CircleMin -> &CircleMinu
				new Transition (1039, 1040), // &CirclePl -> &CirclePlu
				new Transition (1080, 1081), // &ClockwiseConto -> &ClockwiseContou
				new Transition (1094, 1095), // &CloseC -> &CloseCu
				new Transition (1100, 1101), // &CloseCurlyDo -> &CloseCurlyDou
				new Transition (1105, 1106), // &CloseCurlyDoubleQ -> &CloseCurlyDoubleQu
				new Transition (1111, 1112), // &CloseCurlyQ -> &CloseCurlyQu
				new Transition (1117, 1118), // &cl -> &clu
				new Transition (1120, 1122), // &clubs -> &clubsu
				new Transition (1126, 1226), // &Co -> &Cou
				new Transition (1173, 1174), // &Congr -> &Congru
				new Transition (1188, 1189), // &Conto -> &Contou
				new Transition (1212, 1213), // &Coprod -> &Coprodu
				new Transition (1244, 1245), // &CounterClockwiseConto -> &CounterClockwiseContou
				new Transition (1274, 1278), // &cs -> &csu
				new Transition (1330, 1334), // &cupc -> &cupcu
				new Transition (1362, 1363), // &curlyeqs -> &curlyeqsu
				new Transition (1432, 2077), // &d -> &du
				new Transition (1568, 1569), // &DiacriticalAc -> &DiacriticalAcu
				new Transition (1574, 1577), // &DiacriticalDo -> &DiacriticalDou
				new Transition (1582, 1583), // &DiacriticalDoubleAc -> &DiacriticalDoubleAcu
				new Transition (1612, 1613), // &diamonds -> &diamondsu
				new Transition (1679, 1731), // &do -> &dou
				new Transition (1685, 1744), // &Do -> &Dou
				new Transition (1708, 1709), // &DotEq -> &DotEqu
				new Transition (1715, 1716), // &dotmin -> &dotminu
				new Transition (1720, 1721), // &dotpl -> &dotplu
				new Transition (1725, 1726), // &dotsq -> &dotsqu
				new Transition (1752, 1753), // &DoubleConto -> &DoubleContou
				new Transition (2108, 2447), // &E -> &Eu
				new Transition (2110, 2111), // &Eac -> &Eacu
				new Transition (2115, 2451), // &e -> &eu
				new Transition (2117, 2118), // &eac -> &eacu
				new Transition (2255, 2256), // &EmptySmallSq -> &EmptySmallSqu
				new Transition (2273, 2274), // &EmptyVerySmallSq -> &EmptyVerySmallSqu
				new Transition (2319, 2320), // &epl -> &eplu
				new Transition (2339, 2372), // &eq -> &equ
				new Transition (2367, 2368), // &Eq -> &Equ
				new Transition (2392, 2393), // &Equilibri -> &Equilibriu
				new Transition (2565, 2566), // &FilledSmallSq -> &FilledSmallSqu
				new Transition (2581, 2582), // &FilledVerySmallSq -> &FilledVerySmallSqu
				new Transition (2608, 2630), // &Fo -> &Fou
				new Transition (2703, 2704), // &gac -> &gacu
				new Transition (2873, 2874), // &GreaterEq -> &GreaterEqu
				new Transition (2883, 2884), // &GreaterF -> &GreaterFu
				new Transition (2888, 2889), // &GreaterFullEq -> &GreaterFullEqu
				new Transition (2912, 2913), // &GreaterSlantEq -> &GreaterSlantEqu
				new Transition (2959, 2960), // &gtq -> &gtqu
				new Transition (3014, 3207), // &H -> &Hu
				new Transition (3078, 3080), // &hearts -> &heartsu
				new Transition (3214, 3215), // &HumpDownH -> &HumpDownHu
				new Transition (3220, 3221), // &HumpEq -> &HumpEqu
				new Transition (3226, 3227), // &hyb -> &hybu
				new Transition (3236, 3539), // &I -> &Iu
				new Transition (3238, 3239), // &Iac -> &Iacu
				new Transition (3243, 3544), // &i -> &iu
				new Transition (3245, 3246), // &iac -> &iacu
				new Transition (3497, 3498), // &iq -> &iqu
				new Transition (3555, 3608), // &J -> &Ju
				new Transition (3561, 3613), // &j -> &ju
				new Transition (3692, 4742), // &l -> &lu
				new Transition (3700, 3701), // &Lac -> &Lacu
				new Transition (3706, 3707), // &lac -> &lacu
				new Transition (3755, 3756), // &laq -> &laqu
				new Transition (3832, 3835), // &lbrksl -> &lbrkslu
				new Transition (3843, 3862), // &lc -> &lcu
				new Transition (3873, 3874), // &ldq -> &ldqu
				new Transition (3879, 3885), // &ldr -> &ldru
				new Transition (3962, 3963), // &LeftDo -> &LeftDou
				new Transition (4010, 4016), // &leftharpoon -> &leftharpoonu
				new Transition (4075, 4076), // &leftrightsq -> &leftrightsqu
				new Transition (4133, 4134), // &LeftTriangleEq -> &LeftTriangleEqu
				new Transition (4241, 4242), // &LessEq -> &LessEqu
				new Transition (4253, 4254), // &LessF -> &LessFu
				new Transition (4258, 4259), // &LessFullEq -> &LessFullEqu
				new Transition (4290, 4291), // &LessSlantEq -> &LessSlantEqu
				new Transition (4327, 4330), // &lhar -> &lharu
				new Transition (4391, 4392), // &lmo -> &lmou
				new Transition (4569, 4570), // &lopl -> &loplu
				new Transition (4654, 4655), // &lsaq -> &lsaqu
				new Transition (4676, 4679), // &lsq -> &lsqu
				new Transition (4725, 4726), // &ltq -> &ltqu
				new Transition (4743, 4750), // &lur -> &luru
				new Transition (4767, 4952), // &m -> &mu
				new Transition (4781, 4950), // &M -> &Mu
				new Transition (4789, 4801), // &mapsto -> &mapstou
				new Transition (4832, 4833), // &meas -> &measu
				new Transition (4845, 4846), // &Medi -> &Mediu
				new Transition (4890, 4891), // &min -> &minu
				new Transition (4896, 4898), // &minusd -> &minusdu
				new Transition (4901, 4902), // &Min -> &Minu
				new Transition (4905, 4906), // &MinusPl -> &MinusPlu
				new Transition (4918, 4919), // &mnpl -> &mnplu
				new Transition (4965, 6032), // &n -> &nu
				new Transition (4971, 6030), // &N -> &Nu
				new Transition (4973, 4974), // &Nac -> &Nacu
				new Transition (4978, 4979), // &nac -> &nacu
				new Transition (5001, 5002), // &nat -> &natu
				new Transition (5010, 5014), // &nb -> &nbu
				new Transition (5020, 5052), // &nc -> &ncu
				new Transition (5094, 5095), // &NegativeMedi -> &NegativeMediu
				new Transition (5135, 5136), // &neq -> &nequ
				new Transition (5380, 5390), // &NotC -> &NotCu
				new Transition (5384, 5385), // &NotCongr -> &NotCongru
				new Transition (5397, 5398), // &NotDo -> &NotDou
				new Transition (5422, 5423), // &NotEq -> &NotEqu
				new Transition (5448, 5449), // &NotGreaterEq -> &NotGreaterEqu
				new Transition (5453, 5454), // &NotGreaterF -> &NotGreaterFu
				new Transition (5458, 5459), // &NotGreaterFullEq -> &NotGreaterFullEqu
				new Transition (5482, 5483), // &NotGreaterSlantEq -> &NotGreaterSlantEqu
				new Transition (5493, 5494), // &NotH -> &NotHu
				new Transition (5501, 5502), // &NotHumpDownH -> &NotHumpDownHu
				new Transition (5507, 5508), // &NotHumpEq -> &NotHumpEqu
				new Transition (5546, 5547), // &NotLeftTriangleEq -> &NotLeftTriangleEqu
				new Transition (5555, 5556), // &NotLessEq -> &NotLessEqu
				new Transition (5579, 5580), // &NotLessSlantEq -> &NotLessSlantEqu
				new Transition (5640, 5641), // &NotPrecedesEq -> &NotPrecedesEqu
				new Transition (5651, 5652), // &NotPrecedesSlantEq -> &NotPrecedesSlantEqu
				new Transition (5689, 5690), // &NotRightTriangleEq -> &NotRightTriangleEqu
				new Transition (5694, 5726), // &NotS -> &NotSu
				new Transition (5695, 5696), // &NotSq -> &NotSqu
				new Transition (5700, 5701), // &NotSquareS -> &NotSquareSu
				new Transition (5708, 5709), // &NotSquareSubsetEq -> &NotSquareSubsetEqu
				new Transition (5721, 5722), // &NotSquareSupersetEq -> &NotSquareSupersetEqu
				new Transition (5733, 5734), // &NotSubsetEq -> &NotSubsetEqu
				new Transition (5746, 5747), // &NotSucceedsEq -> &NotSucceedsEqu
				new Transition (5757, 5758), // &NotSucceedsSlantEq -> &NotSucceedsSlantEqu
				new Transition (5776, 5777), // &NotSupersetEq -> &NotSupersetEqu
				new Transition (5788, 5789), // &NotTildeEq -> &NotTildeEqu
				new Transition (5793, 5794), // &NotTildeF -> &NotTildeFu
				new Transition (5798, 5799), // &NotTildeFullEq -> &NotTildeFullEqu
				new Transition (5844, 5845), // &nprc -> &nprcu
				new Transition (5895, 5951), // &ns -> &nsu
				new Transition (5898, 5899), // &nscc -> &nsccu
				new Transition (5943, 5944), // &nsqs -> &nsqsu
				new Transition (6131, 6422), // &O -> &Ou
				new Transition (6133, 6134), // &Oac -> &Oacu
				new Transition (6138, 6426), // &o -> &ou
				new Transition (6140, 6141), // &oac -> &oacu
				new Transition (6290, 6291), // &omin -> &ominu
				new Transition (6309, 6310), // &OpenC -> &OpenCu
				new Transition (6315, 6316), // &OpenCurlyDo -> &OpenCurlyDou
				new Transition (6320, 6321), // &OpenCurlyDoubleQ -> &OpenCurlyDoubleQu
				new Transition (6326, 6327), // &OpenCurlyQ -> &OpenCurlyQu
				new Transition (6336, 6337), // &opl -> &oplu
				new Transition (6463, 6807), // &p -> &pu
				new Transition (6555, 6566), // &pl -> &plu
				new Transition (6580, 6583), // &plusd -> &plusdu
				new Transition (6587, 6588), // &Pl -> &Plu
				new Transition (6592, 6593), // &PlusMin -> &PlusMinu
				new Transition (6622, 6636), // &po -> &pou
				new Transition (6642, 6790), // &pr -> &pru
				new Transition (6647, 6648), // &prc -> &prcu
				new Transition (6664, 6665), // &precc -> &preccu
				new Transition (6680, 6681), // &PrecedesEq -> &PrecedesEqu
				new Transition (6691, 6692), // &PrecedesSlantEq -> &PrecedesSlantEqu
				new Transition (6749, 6750), // &Prod -> &Produ
				new Transition (6765, 6766), // &profs -> &profsu
				new Transition (6817, 6847), // &q -> &qu
				new Transition (6876, 7601), // &r -> &ru
				new Transition (6883, 6893), // &rac -> &racu
				new Transition (6886, 7590), // &R -> &Ru
				new Transition (6888, 6889), // &Rac -> &Racu
				new Transition (6921, 6922), // &raq -> &raqu
				new Transition (7016, 7019), // &rbrksl -> &rbrkslu
				new Transition (7027, 7046), // &rc -> &rcu
				new Transition (7063, 7064), // &rdq -> &rdqu
				new Transition (7110, 7111), // &ReverseEq -> &ReverseEqu
				new Transition (7117, 7118), // &ReverseEquilibri -> &ReverseEquilibriu
				new Transition (7124, 7125), // &ReverseUpEq -> &ReverseUpEqu
				new Transition (7131, 7132), // &ReverseUpEquilibri -> &ReverseUpEquilibriu
				new Transition (7157, 7160), // &rhar -> &rharu
				new Transition (7237, 7238), // &RightDo -> &RightDou
				new Transition (7285, 7291), // &rightharpoon -> &rightharpoonu
				new Transition (7327, 7328), // &rightsq -> &rightsqu
				new Transition (7378, 7379), // &RightTriangleEq -> &RightTriangleEqu
				new Transition (7454, 7455), // &rmo -> &rmou
				new Transition (7485, 7501), // &Ro -> &Rou
				new Transition (7491, 7492), // &ropl -> &roplu
				new Transition (7544, 7545), // &rsaq -> &rsaqu
				new Transition (7559, 7562), // &rsq -> &rsqu
				new Transition (7602, 7603), // &rul -> &rulu
				new Transition (7610, 8127), // &S -> &Su
				new Transition (7612, 7613), // &Sac -> &Sacu
				new Transition (7617, 8130), // &s -> &su
				new Transition (7619, 7620), // &sac -> &sacu
				new Transition (7625, 7626), // &sbq -> &sbqu
				new Transition (7645, 7646), // &scc -> &sccu
				new Transition (7732, 7733), // &setmin -> &setminu
				new Transition (7869, 7870), // &simpl -> &simplu
				new Transition (7903, 7904), // &smallsetmin -> &smallsetminu
				new Transition (7960, 7962), // &spades -> &spadesu
				new Transition (7968, 8008), // &sq -> &squ
				new Transition (7969, 7975), // &sqc -> &sqcu
				new Transition (7980, 8010), // &Sq -> &Squ
				new Transition (7984, 7985), // &sqs -> &sqsu
				new Transition (8032, 8033), // &SquareS -> &SquareSu
				new Transition (8040, 8041), // &SquareSubsetEq -> &SquareSubsetEqu
				new Transition (8053, 8054), // &SquareSupersetEq -> &SquareSupersetEqu
				new Transition (8145, 8146), // &subm -> &submu
				new Transition (8156, 8157), // &subpl -> &subplu
				new Transition (8169, 8193), // &subs -> &subsu
				new Transition (8179, 8180), // &SubsetEq -> &SubsetEqu
				new Transition (8208, 8209), // &succc -> &succcu
				new Transition (8224, 8225), // &SucceedsEq -> &SucceedsEqu
				new Transition (8235, 8236), // &SucceedsSlantEq -> &SucceedsSlantEqu
				new Transition (8296, 8297), // &supds -> &supdsu
				new Transition (8315, 8316), // &SupersetEq -> &SupersetEqu
				new Transition (8321, 8325), // &suphs -> &suphsu
				new Transition (8333, 8334), // &supm -> &supmu
				new Transition (8344, 8345), // &suppl -> &supplu
				new Transition (8352, 8370), // &sups -> &supsu
				new Transition (8401, 8411), // &Ta -> &Tau
				new Transition (8405, 8413), // &ta -> &tau
				new Transition (8555, 8556), // &TildeEq -> &TildeEqu
				new Transition (8560, 8561), // &TildeF -> &TildeFu
				new Transition (8565, 8566), // &TildeFullEq -> &TildeFullEqu
				new Transition (8672, 8673), // &trimin -> &triminu
				new Transition (8686, 8687), // &tripl -> &triplu
				new Transition (8701, 8702), // &trpezi -> &trpeziu
				new Transition (8768, 9187), // &U -> &Uu
				new Transition (8770, 8771), // &Uac -> &Uacu
				new Transition (8775, 9182), // &u -> &uu
				new Transition (8777, 8778), // &uac -> &uacu
				new Transition (8950, 8951), // &UnionPl -> &UnionPlu
				new Transition (8983, 9118), // &up -> &upu
				new Transition (9035, 9036), // &UpEq -> &UpEqu
				new Transition (9042, 9043), // &UpEquilibri -> &UpEquilibriu
				new Transition (9064, 9065), // &upl -> &uplu
				new Transition (9252, 9258), // &vars -> &varsu
				new Transition (9426, 9427), // &vns -> &vnsu
				new Transition (9454, 9458), // &vs -> &vsu
				new Transition (9548, 9645), // &x -> &xu
				new Transition (9549, 9557), // &xc -> &xcu
				new Transition (9614, 9615), // &xopl -> &xoplu
				new Transition (9641, 9642), // &xsqc -> &xsqcu
				new Transition (9647, 9648), // &xupl -> &xuplu
				new Transition (9665, 9740), // &Y -> &Yu
				new Transition (9667, 9668), // &Yac -> &Yacu
				new Transition (9672, 9736), // &y -> &yu
				new Transition (9674, 9675), // &yac -> &yacu
				new Transition (9749, 9750), // &Zac -> &Zacu
				new Transition (9756, 9757) // &zac -> &zacu
			};
			TransitionTable_v = new Transition[75] {
				new Transition (0, 9201), // & -> &v
				new Transition (17, 18), // &Abre -> &Abrev
				new Transition (23, 24), // &abre -> &abrev
				new Transition (69, 70), // &Agra -> &Agrav
				new Transition (75, 76), // &agra -> &agrav
				new Transition (120, 134), // &and -> &andv
				new Transition (165, 167), // &angrt -> &angrtv
				new Transition (341, 342), // &Bar -> &Barv
				new Transition (344, 345), // &bar -> &barv
				new Transition (402, 403), // &bempty -> &bemptyv
				new Transition (443, 503), // &big -> &bigv
				new Transition (584, 585), // &bnequi -> &bnequiv
				new Transition (613, 693), // &box -> &boxv
				new Transition (726, 727), // &Bre -> &Brev
				new Transition (730, 735), // &br -> &brv
				new Transition (731, 732), // &bre -> &brev
				new Transition (930, 931), // &cempty -> &cemptyv
				new Transition (1292, 1399), // &cu -> &cuv
				new Transition (1346, 1381), // &cur -> &curv
				new Transition (1354, 1367), // &curly -> &curlyv
				new Transition (1455, 1461), // &dash -> &dashv
				new Transition (1458, 1459), // &Dash -> &Dashv
				new Transition (1532, 1533), // &dempty -> &demptyv
				new Transition (1589, 1590), // &DiacriticalGra -> &DiacriticalGrav
				new Transition (1599, 1643), // &di -> &div
				new Transition (1917, 1918), // &DownBre -> &DownBrev
				new Transition (2189, 2190), // &Egra -> &Egrav
				new Transition (2194, 2195), // &egra -> &egrav
				new Transition (2240, 2261), // &empty -> &emptyv
				new Transition (2324, 2337), // &epsi -> &epsiv
				new Transition (2339, 2402), // &eq -> &eqv
				new Transition (2396, 2397), // &equi -> &equiv
				new Transition (2626, 2628), // &fork -> &forkv
				new Transition (2701, 3002), // &g -> &gv
				new Transition (2726, 2727), // &Gbre -> &Gbrev
				new Transition (2732, 2733), // &gbre -> &gbrev
				new Transition (2862, 2863), // &gra -> &grav
				new Transition (3291, 3292), // &Igra -> &Igrav
				new Transition (3297, 3298), // &igra -> &igrav
				new Transition (3398, 3444), // &In -> &Inv
				new Transition (3512, 3524), // &isin -> &isinv
				new Transition (3520, 3522), // &isins -> &isinsv
				new Transition (3628, 3630), // &kappa -> &kappav
				new Transition (3692, 4755), // &l -> &lv
				new Transition (3715, 3716), // &laempty -> &laemptyv
				new Transition (4965, 6043), // &n -> &nv
				new Transition (5088, 5089), // &Negati -> &Negativ
				new Transition (5137, 5138), // &nequi -> &nequiv
				new Transition (5219, 5225), // &nGt -> &nGtv
				new Transition (5240, 5246), // &ni -> &niv
				new Transition (5332, 5341), // &nLt -> &nLtv
				new Transition (5513, 5521), // &notin -> &notinv
				new Transition (5621, 5623), // &notni -> &notniv
				new Transition (5657, 5658), // &NotRe -> &NotRev
				new Transition (6131, 6435), // &O -> &Ov
				new Transition (6138, 6430), // &o -> &ov
				new Transition (6179, 6180), // &odi -> &odiv
				new Transition (6216, 6217), // &Ogra -> &Ograv
				new Transition (6221, 6222), // &ogra -> &ograv
				new Transition (6342, 6374), // &or -> &orv
				new Transition (6528, 6530), // &phi -> &phiv
				new Transition (6543, 6553), // &pi -> &piv
				new Transition (6563, 6564), // &plank -> &plankv
				new Transition (6905, 6906), // &raempty -> &raemptyv
				new Transition (7072, 7097), // &Re -> &Rev
				new Transition (7167, 7169), // &rho -> &rhov
				new Transition (7841, 7845), // &sigma -> &sigmav
				new Transition (8485, 8491), // &theta -> &thetav
				new Transition (8807, 8808), // &Ubre -> &Ubrev
				new Transition (8811, 8812), // &ubre -> &ubrev
				new Transition (8862, 8863), // &Ugra -> &Ugrav
				new Transition (8868, 8869), // &ugra -> &ugrav
				new Transition (9303, 9471), // &V -> &Vv
				new Transition (9310, 9312), // &vBar -> &vBarv
				new Transition (9548, 9655) // &x -> &xv
			};
			TransitionTable_w = new Transition[137] {
				new Transition (0, 9490), // & -> &w
				new Transition (8, 289), // &a -> &aw
				new Transition (341, 349), // &Bar -> &Barw
				new Transition (344, 353), // &bar -> &barw
				new Transition (426, 431), // &bet -> &betw
				new Transition (443, 507), // &big -> &bigw
				new Transition (490, 491), // &bigtriangledo -> &bigtriangledow
				new Transition (516, 517), // &bkaro -> &bkarow
				new Transition (548, 549), // &blacktriangledo -> &blacktriangledow
				new Transition (598, 608), // &bo -> &bow
				new Transition (796, 1407), // &c -> &cw
				new Transition (991, 992), // &circlearro -> &circlearrow
				new Transition (1071, 1072), // &Clock -> &Clockw
				new Transition (1235, 1236), // &CounterClock -> &CounterClockw
				new Transition (1292, 1403), // &cu -> &cuw
				new Transition (1354, 1371), // &curly -> &curlyw
				new Transition (1386, 1387), // &curvearro -> &curvearrow
				new Transition (1432, 2086), // &d -> &dw
				new Transition (1467, 1468), // &dbkaro -> &dbkarow
				new Transition (1679, 1895), // &do -> &dow
				new Transition (1685, 1881), // &Do -> &Dow
				new Transition (1737, 1738), // &doublebar -> &doublebarw
				new Transition (1765, 1768), // &DoubleDo -> &DoubleDow
				new Transition (1773, 1774), // &DoubleDownArro -> &DoubleDownArrow
				new Transition (1783, 1784), // &DoubleLeftArro -> &DoubleLeftArrow
				new Transition (1794, 1795), // &DoubleLeftRightArro -> &DoubleLeftRightArrow
				new Transition (1811, 1812), // &DoubleLongLeftArro -> &DoubleLongLeftArrow
				new Transition (1822, 1823), // &DoubleLongLeftRightArro -> &DoubleLongLeftRightArrow
				new Transition (1833, 1834), // &DoubleLongRightArro -> &DoubleLongRightArrow
				new Transition (1844, 1845), // &DoubleRightArro -> &DoubleRightArrow
				new Transition (1856, 1857), // &DoubleUpArro -> &DoubleUpArrow
				new Transition (1860, 1861), // &DoubleUpDo -> &DoubleUpDow
				new Transition (1866, 1867), // &DoubleUpDownArro -> &DoubleUpDownArrow
				new Transition (1886, 1887), // &DownArro -> &DownArrow
				new Transition (1892, 1893), // &Downarro -> &Downarrow
				new Transition (1900, 1901), // &downarro -> &downarrow
				new Transition (1912, 1913), // &DownArrowUpArro -> &DownArrowUpArrow
				new Transition (1922, 1923), // &downdo -> &downdow
				new Transition (1928, 1929), // &downdownarro -> &downdownarrow
				new Transition (2020, 2021), // &DownTeeArro -> &DownTeeArrow
				new Transition (2028, 2029), // &drbkaro -> &drbkarow
				new Transition (2689, 2690), // &fro -> &frow
				new Transition (3050, 3056), // &harr -> &harrw
				new Transition (3113, 3120), // &hks -> &hksw
				new Transition (3117, 3118), // &hksearo -> &hksearow
				new Transition (3123, 3124), // &hkswaro -> &hkswarow
				new Transition (3145, 3146), // &hookleftarro -> &hookleftarrow
				new Transition (3156, 3157), // &hookrightarro -> &hookrightarrow
				new Transition (3211, 3212), // &HumpDo -> &HumpDow
				new Transition (3916, 3917), // &LeftArro -> &LeftArrow
				new Transition (3922, 3923), // &Leftarro -> &Leftarrow
				new Transition (3930, 3931), // &leftarro -> &leftarrow
				new Transition (3945, 3946), // &LeftArrowRightArro -> &LeftArrowRightArrow
				new Transition (3962, 3975), // &LeftDo -> &LeftDow
				new Transition (4012, 4013), // &leftharpoondo -> &leftharpoondow
				new Transition (4026, 4027), // &leftleftarro -> &leftleftarrow
				new Transition (4038, 4039), // &LeftRightArro -> &LeftRightArrow
				new Transition (4049, 4050), // &Leftrightarro -> &Leftrightarrow
				new Transition (4060, 4061), // &leftrightarro -> &leftrightarrow
				new Transition (4082, 4083), // &leftrightsquigarro -> &leftrightsquigarrow
				new Transition (4099, 4100), // &LeftTeeArro -> &LeftTeeArrow
				new Transition (4141, 4142), // &LeftUpDo -> &LeftUpDow
				new Transition (4367, 4368), // &Lleftarro -> &Lleftarrow
				new Transition (4422, 4579), // &lo -> &low
				new Transition (4434, 4588), // &Lo -> &Low
				new Transition (4444, 4445), // &LongLeftArro -> &LongLeftArrow
				new Transition (4454, 4455), // &Longleftarro -> &Longleftarrow
				new Transition (4466, 4467), // &longleftarro -> &longleftarrow
				new Transition (4477, 4478), // &LongLeftRightArro -> &LongLeftRightArrow
				new Transition (4488, 4489), // &Longleftrightarro -> &Longleftrightarrow
				new Transition (4499, 4500), // &longleftrightarro -> &longleftrightarrow
				new Transition (4517, 4518), // &LongRightArro -> &LongRightArrow
				new Transition (4528, 4529), // &Longrightarro -> &Longrightarrow
				new Transition (4539, 4540), // &longrightarro -> &longrightarrow
				new Transition (4547, 4548), // &looparro -> &looparrow
				new Transition (4598, 4599), // &LowerLeftArro -> &LowerLeftArrow
				new Transition (4609, 4610), // &LowerRightArro -> &LowerRightArrow
				new Transition (4792, 4793), // &mapstodo -> &mapstodow
				new Transition (4965, 6111), // &n -> &nw
				new Transition (5077, 5078), // &nearro -> &nearrow
				new Transition (5084, 5176), // &Ne -> &New
				new Transition (5279, 5280), // &nLeftarro -> &nLeftarrow
				new Transition (5287, 5288), // &nleftarro -> &nleftarrow
				new Transition (5298, 5299), // &nLeftrightarro -> &nLeftrightarrow
				new Transition (5309, 5310), // &nleftrightarro -> &nleftrightarrow
				new Transition (5498, 5499), // &NotHumpDo -> &NotHumpDow
				new Transition (5862, 5866), // &nrarr -> &nrarrw
				new Transition (5876, 5877), // &nRightarro -> &nRightarrow
				new Transition (5886, 5887), // &nrightarro -> &nrightarrow
				new Transition (6123, 6124), // &nwarro -> &nwarrow
				new Transition (6603, 6604), // &plust -> &plustw
				new Transition (6932, 6966), // &rarr -> &rarrw
				new Transition (7190, 7191), // &RightArro -> &RightArrow
				new Transition (7196, 7197), // &Rightarro -> &Rightarrow
				new Transition (7206, 7207), // &rightarro -> &rightarrow
				new Transition (7220, 7221), // &RightArrowLeftArro -> &RightArrowLeftArrow
				new Transition (7237, 7250), // &RightDo -> &RightDow
				new Transition (7287, 7288), // &rightharpoondo -> &rightharpoondow
				new Transition (7301, 7302), // &rightleftarro -> &rightleftarrow
				new Transition (7322, 7323), // &rightrightarro -> &rightrightarrow
				new Transition (7334, 7335), // &rightsquigarro -> &rightsquigarrow
				new Transition (7344, 7345), // &RightTeeArro -> &RightTeeArrow
				new Transition (7386, 7387), // &RightUpDo -> &RightUpDow
				new Transition (7539, 7540), // &Rrightarro -> &Rrightarrow
				new Transition (7617, 8375), // &s -> &sw
				new Transition (7715, 7716), // &searro -> &searrow
				new Transition (7724, 7725), // &ses -> &sesw
				new Transition (7747, 7748), // &sfro -> &sfrow
				new Transition (7777, 7778), // &ShortDo -> &ShortDow
				new Transition (7783, 7784), // &ShortDownArro -> &ShortDownArrow
				new Transition (7793, 7794), // &ShortLeftArro -> &ShortLeftArrow
				new Transition (7820, 7821), // &ShortRightArro -> &ShortRightArrow
				new Transition (7828, 7829), // &ShortUpArro -> &ShortUpArrow
				new Transition (8387, 8388), // &swarro -> &swarrow
				new Transition (8390, 8391), // &swn -> &swnw
				new Transition (8404, 8737), // &t -> &tw
				new Transition (8641, 8642), // &triangledo -> &triangledow
				new Transition (8754, 8755), // &twoheadleftarro -> &twoheadleftarrow
				new Transition (8765, 8766), // &twoheadrightarro -> &twoheadrightarrow
				new Transition (8775, 9194), // &u -> &uw
				new Transition (8974, 8975), // &UpArro -> &UpArrow
				new Transition (8980, 8981), // &Uparro -> &Uparrow
				new Transition (8987, 8988), // &uparro -> &uparrow
				new Transition (8995, 8996), // &UpArrowDo -> &UpArrowDow
				new Transition (9001, 9002), // &UpArrowDownArro -> &UpArrowDownArrow
				new Transition (9005, 9006), // &UpDo -> &UpDow
				new Transition (9011, 9012), // &UpDownArro -> &UpDownArrow
				new Transition (9015, 9016), // &Updo -> &Updow
				new Transition (9021, 9022), // &Updownarro -> &Updownarrow
				new Transition (9025, 9026), // &updo -> &updow
				new Transition (9031, 9032), // &updownarro -> &updownarrow
				new Transition (9078, 9079), // &UpperLeftArro -> &UpperLeftArrow
				new Transition (9089, 9090), // &UpperRightArro -> &UpperRightArrow
				new Transition (9115, 9116), // &UpTeeArro -> &UpTeeArrow
				new Transition (9123, 9124), // &upuparro -> &upuparrow
				new Transition (9548, 9659), // &x -> &xw
				new Transition (9754, 9848) // &z -> &zw
			};
			TransitionTable_x = new Transition[24] {
				new Transition (0, 9548), // & -> &x
				new Transition (231, 232), // &appro -> &approx
				new Transition (598, 613), // &bo -> &box
				new Transition (615, 616), // &boxbo -> &boxbox
				new Transition (1154, 1160), // &comple -> &complex
				new Transition (1658, 1659), // &divon -> &divonx
				new Transition (2108, 2466), // &E -> &Ex
				new Transition (2115, 2458), // &e -> &ex
				new Transition (2838, 2839), // &gnappro -> &gnapprox
				new Transition (2970, 2971), // &gtrappro -> &gtrapprox
				new Transition (3273, 3277), // &ie -> &iex
				new Transition (4220, 4221), // &lessappro -> &lessapprox
				new Transition (4407, 4408), // &lnappro -> &lnapprox
				new Transition (4998, 4999), // &nappro -> &napprox
				new Transition (5064, 5182), // &ne -> &nex
				new Transition (5414, 5433), // &NotE -> &NotEx
				new Transition (6661, 6662), // &precappro -> &precapprox
				new Transition (6710, 6711), // &precnappro -> &precnapprox
				new Transition (6876, 7608), // &r -> &rx
				new Transition (7703, 7738), // &se -> &sex
				new Transition (8205, 8206), // &succappro -> &succapprox
				new Transition (8254, 8255), // &succnappro -> &succnapprox
				new Transition (8500, 8501), // &thickappro -> &thickapprox
				new Transition (8738, 8739) // &twi -> &twix
			};
			TransitionTable_y = new Transition[122] {
				new Transition (0, 9672), // & -> &y
				new Transition (27, 48), // &ac -> &acy
				new Transition (33, 46), // &Ac -> &Acy
				new Transition (82, 83), // &alefs -> &alefsy
				new Transition (218, 219), // &Appl -> &Apply
				new Transition (251, 262), // &as -> &asy
				new Transition (369, 377), // &bc -> &bcy
				new Transition (374, 375), // &Bc -> &Bcy
				new Transition (401, 402), // &bempt -> &bempty
				new Transition (790, 855), // &Ca -> &Cay
				new Transition (796, 1419), // &c -> &cy
				new Transition (857, 858), // &Cayle -> &Cayley
				new Transition (929, 930), // &cempt -> &cempty
				new Transition (957, 958), // &CHc -> &CHcy
				new Transition (961, 962), // &chc -> &chcy
				new Transition (1097, 1098), // &CloseCurl -> &CloseCurly
				new Transition (1203, 1221), // &cop -> &copy
				new Transition (1353, 1354), // &curl -> &curly
				new Transition (1422, 1423), // &cylct -> &cylcty
				new Transition (1474, 1486), // &Dc -> &Dcy
				new Transition (1480, 1488), // &dc -> &dcy
				new Transition (1531, 1532), // &dempt -> &dempty
				new Transition (1662, 1663), // &DJc -> &DJcy
				new Transition (1666, 1667), // &djc -> &djcy
				new Transition (2045, 2052), // &dsc -> &dscy
				new Transition (2049, 2050), // &DSc -> &DScy
				new Transition (2094, 2095), // &DZc -> &DZcy
				new Transition (2098, 2099), // &dzc -> &dzcy
				new Transition (2127, 2153), // &Ec -> &Ecy
				new Transition (2133, 2155), // &ec -> &ecy
				new Transition (2239, 2240), // &empt -> &empty
				new Transition (2247, 2248), // &Empt -> &Empty
				new Transition (2265, 2266), // &EmptyVer -> &EmptyVery
				new Transition (2518, 2519), // &Fc -> &Fcy
				new Transition (2521, 2522), // &fc -> &fcy
				new Transition (2573, 2574), // &FilledVer -> &FilledVery
				new Transition (2736, 2751), // &Gc -> &Gcy
				new Transition (2746, 2753), // &gc -> &gcy
				new Transition (2817, 2818), // &GJc -> &GJcy
				new Transition (2821, 2822), // &gjc -> &gjcy
				new Transition (3020, 3225), // &h -> &hy
				new Transition (3038, 3039), // &HARDc -> &HARDcy
				new Transition (3043, 3044), // &hardc -> &hardcy
				new Transition (3250, 3263), // &ic -> &icy
				new Transition (3252, 3261), // &Ic -> &Icy
				new Transition (3270, 3271), // &IEc -> &IEcy
				new Transition (3274, 3275), // &iec -> &iecy
				new Transition (3348, 3349), // &Imaginar -> &Imaginary
				new Transition (3464, 3465), // &IOc -> &IOcy
				new Transition (3468, 3469), // &ioc -> &iocy
				new Transition (3541, 3542), // &Iukc -> &Iukcy
				new Transition (3546, 3547), // &iukc -> &iukcy
				new Transition (3556, 3567), // &Jc -> &Jcy
				new Transition (3562, 3569), // &jc -> &jcy
				new Transition (3600, 3601), // &Jserc -> &Jsercy
				new Transition (3605, 3606), // &jserc -> &jsercy
				new Transition (3610, 3611), // &Jukc -> &Jukcy
				new Transition (3615, 3616), // &jukc -> &jukcy
				new Transition (3632, 3644), // &Kc -> &Kcy
				new Transition (3638, 3646), // &kc -> &kcy
				new Transition (3661, 3662), // &KHc -> &KHcy
				new Transition (3665, 3666), // &khc -> &khcy
				new Transition (3669, 3670), // &KJc -> &KJcy
				new Transition (3673, 3674), // &kjc -> &kjcy
				new Transition (3714, 3715), // &laempt -> &laempty
				new Transition (3837, 3865), // &Lc -> &Lcy
				new Transition (3843, 3867), // &lc -> &lcy
				new Transition (4339, 4340), // &LJc -> &LJcy
				new Transition (4343, 4344), // &ljc -> &ljcy
				new Transition (4809, 4818), // &mc -> &mcy
				new Transition (4815, 4816), // &Mc -> &Mcy
				new Transition (5020, 5057), // &nc -> &ncy
				new Transition (5024, 5055), // &Nc -> &Ncy
				new Transition (5123, 5124), // &NegativeVer -> &NegativeVery
				new Transition (5249, 5250), // &NJc -> &NJcy
				new Transition (5253, 5254), // &njc -> &njcy
				new Transition (6148, 6161), // &oc -> &ocy
				new Transition (6152, 6159), // &Oc -> &Ocy
				new Transition (6312, 6313), // &OpenCurl -> &OpenCurly
				new Transition (6491, 6492), // &Pc -> &Pcy
				new Transition (6494, 6495), // &pc -> &pcy
				new Transition (6667, 6668), // &preccurl -> &preccurly
				new Transition (6904, 6905), // &raempt -> &raempty
				new Transition (7021, 7049), // &Rc -> &Rcy
				new Transition (7027, 7051), // &rc -> &rcy
				new Transition (7596, 7597), // &RuleDela -> &RuleDelay
				new Transition (7629, 7691), // &Sc -> &Scy
				new Transition (7631, 7693), // &sc -> &scy
				new Transition (7751, 7831), // &sh -> &shy
				new Transition (7759, 7760), // &SHCHc -> &SHCHcy
				new Transition (7762, 7770), // &shc -> &shcy
				new Transition (7764, 7765), // &shchc -> &shchcy
				new Transition (7767, 7768), // &SHc -> &SHcy
				new Transition (7933, 7934), // &SOFTc -> &SOFTcy
				new Transition (7939, 7940), // &softc -> &softcy
				new Transition (8211, 8212), // &succcurl -> &succcurly
				new Transition (8419, 8441), // &Tc -> &Tcy
				new Transition (8425, 8443), // &tc -> &tcy
				new Transition (8487, 8488), // &thetas -> &thetasy
				new Transition (8710, 8717), // &tsc -> &tscy
				new Transition (8714, 8715), // &TSc -> &TScy
				new Transition (8720, 8721), // &TSHc -> &TSHcy
				new Transition (8724, 8725), // &tshc -> &tshcy
				new Transition (8799, 8800), // &Ubrc -> &Ubrcy
				new Transition (8804, 8805), // &ubrc -> &ubrcy
				new Transition (8815, 8825), // &Uc -> &Ucy
				new Transition (8820, 8827), // &uc -> &ucy
				new Transition (9314, 9315), // &Vc -> &Vcy
				new Transition (9317, 9318), // &vc -> &vcy
				new Transition (9360, 9403), // &Ver -> &Very
				new Transition (9674, 9683), // &yac -> &yacy
				new Transition (9680, 9681), // &YAc -> &YAcy
				new Transition (9685, 9695), // &Yc -> &Ycy
				new Transition (9690, 9697), // &yc -> &ycy
				new Transition (9709, 9710), // &YIc -> &YIcy
				new Transition (9713, 9714), // &yic -> &yicy
				new Transition (9733, 9734), // &YUc -> &YUcy
				new Transition (9737, 9738), // &yuc -> &yucy
				new Transition (9761, 9773), // &Zc -> &Zcy
				new Transition (9767, 9775), // &zc -> &zcy
				new Transition (9818, 9819), // &ZHc -> &ZHcy
				new Transition (9822, 9823) // &zhc -> &zhcy
			};
			TransitionTable_z = new Transition[10] {
				new Transition (0, 9754), // & -> &z
				new Transition (136, 178), // &ang -> &angz
				new Transition (524, 525), // &blacklo -> &blackloz
				new Transition (1432, 2097), // &d -> &dz
				new Transition (3172, 3173), // &Hori -> &Horiz
				new Transition (4422, 4612), // &lo -> &loz
				new Transition (7617, 8395), // &s -> &sz
				new Transition (8699, 8700), // &trpe -> &trpez
				new Transition (9201, 9477), // &v -> &vz
				new Transition (9479, 9480) // &vzig -> &vzigz
			};

			NamedEntities = new Dictionary<int, string> {
				[6] = "\u00C1", // &Aacute
				[7] = "\u00C1", // &Aacute;
				[13] = "\u00E1", // &aacute
				[14] = "\u00E1", // &aacute;
				[20] = "\u0102", // &Abreve;
				[26] = "\u0103", // &abreve;
				[28] = "\u223E", // &ac;
				[30] = "\u223F", // &acd;
				[32] = "\u223E\u0333", // &acE;
				[36] = "\u00C2", // &Acirc
				[37] = "\u00C2", // &Acirc;
				[40] = "\u00E2", // &acirc
				[41] = "\u00E2", // &acirc;
				[44] = "\u00B4", // &acute
				[45] = "\u00B4", // &acute;
				[47] = "\u0410", // &Acy;
				[49] = "\u0430", // &acy;
				[53] = "\u00C6", // &AElig
				[54] = "\u00C6", // &AElig;
				[58] = "\u00E6", // &aelig
				[59] = "\u00E6", // &aelig;
				[61] = "\u2061", // &af;
				[64] = "\uD835\uDD04", // &Afr;
				[66] = "\uD835\uDD1E", // &afr;
				[71] = "\u00C0", // &Agrave
				[72] = "\u00C0", // &Agrave;
				[77] = "\u00E0", // &agrave
				[78] = "\u00E0", // &agrave;
				[85] = "\u2135", // &alefsym;
				[88] = "\u2135", // &aleph;
				[93] = "\u0391", // &Alpha;
				[97] = "\u03B1", // &alpha;
				[102] = "\u0100", // &Amacr;
				[107] = "\u0101", // &amacr;
				[110] = "\u2A3F", // &amalg;
				[112] = "\u0026", // &AMP
				[113] = "\u0026", // &AMP;
				[114] = "\u0026", // &amp
				[115] = "\u0026", // &amp;
				[118] = "\u2A53", // &And;
				[121] = "\u2227", // &and;
				[125] = "\u2A55", // &andand;
				[127] = "\u2A5C", // &andd;
				[133] = "\u2A58", // &andslope;
				[135] = "\u2A5A", // &andv;
				[137] = "\u2220", // &ang;
				[139] = "\u29A4", // &ange;
				[142] = "\u2220", // &angle;
				[146] = "\u2221", // &angmsd;
				[149] = "\u29A8", // &angmsdaa;
				[151] = "\u29A9", // &angmsdab;
				[153] = "\u29AA", // &angmsdac;
				[155] = "\u29AB", // &angmsdad;
				[157] = "\u29AC", // &angmsdae;
				[159] = "\u29AD", // &angmsdaf;
				[161] = "\u29AE", // &angmsdag;
				[163] = "\u29AF", // &angmsdah;
				[166] = "\u221F", // &angrt;
				[169] = "\u22BE", // &angrtvb;
				[171] = "\u299D", // &angrtvbd;
				[175] = "\u2222", // &angsph;
				[177] = "\u00C5", // &angst;
				[182] = "\u237C", // &angzarr;
				[187] = "\u0104", // &Aogon;
				[192] = "\u0105", // &aogon;
				[195] = "\uD835\uDD38", // &Aopf;
				[198] = "\uD835\uDD52", // &aopf;
				[200] = "\u2248", // &ap;
				[205] = "\u2A6F", // &apacir;
				[207] = "\u2A70", // &apE;
				[209] = "\u224A", // &ape;
				[212] = "\u224B", // &apid;
				[215] = "\u0027", // &apos;
				[228] = "\u2061", // &ApplyFunction;
				[233] = "\u2248", // &approx;
				[236] = "\u224A", // &approxeq;
				[240] = "\u00C5", // &Aring
				[241] = "\u00C5", // &Aring;
				[245] = "\u00E5", // &aring
				[246] = "\u00E5", // &aring;
				[250] = "\uD835\uDC9C", // &Ascr;
				[254] = "\uD835\uDCB6", // &ascr;
				[259] = "\u2254", // &Assign;
				[261] = "\u002A", // &ast;
				[265] = "\u2248", // &asymp;
				[268] = "\u224D", // &asympeq;
				[273] = "\u00C3", // &Atilde
				[274] = "\u00C3", // &Atilde;
				[279] = "\u00E3", // &atilde
				[280] = "\u00E3", // &atilde;
				[283] = "\u00C4", // &Auml
				[284] = "\u00C4", // &Auml;
				[287] = "\u00E4", // &auml
				[288] = "\u00E4", // &auml;
				[296] = "\u2233", // &awconint;
				[300] = "\u2A11", // &awint;
				[309] = "\u224C", // &backcong;
				[317] = "\u03F6", // &backepsilon;
				[323] = "\u2035", // &backprime;
				[327] = "\u223D", // &backsim;
				[330] = "\u22CD", // &backsimeq;
				[340] = "\u2216", // &Backslash;
				[343] = "\u2AE7", // &Barv;
				[348] = "\u22BD", // &barvee;
				[352] = "\u2306", // &Barwed;
				[356] = "\u2305", // &barwed;
				[359] = "\u2305", // &barwedge;
				[363] = "\u23B5", // &bbrk;
				[368] = "\u23B6", // &bbrktbrk;
				[373] = "\u224C", // &bcong;
				[376] = "\u0411", // &Bcy;
				[378] = "\u0431", // &bcy;
				[383] = "\u201E", // &bdquo;
				[389] = "\u2235", // &becaus;
				[396] = "\u2235", // &Because;
				[398] = "\u2235", // &because;
				[404] = "\u29B0", // &bemptyv;
				[408] = "\u03F6", // &bepsi;
				[413] = "\u212C", // &bernou;
				[422] = "\u212C", // &Bernoullis;
				[425] = "\u0392", // &Beta;
				[428] = "\u03B2", // &beta;
				[430] = "\u2136", // &beth;
				[435] = "\u226C", // &between;
				[438] = "\uD835\uDD05", // &Bfr;
				[441] = "\uD835\uDD1F", // &bfr;
				[447] = "\u22C2", // &bigcap;
				[451] = "\u25EF", // &bigcirc;
				[454] = "\u22C3", // &bigcup;
				[459] = "\u2A00", // &bigodot;
				[464] = "\u2A01", // &bigoplus;
				[470] = "\u2A02", // &bigotimes;
				[476] = "\u2A06", // &bigsqcup;
				[480] = "\u2605", // &bigstar;
				[493] = "\u25BD", // &bigtriangledown;
				[496] = "\u25B3", // &bigtriangleup;
				[502] = "\u2A04", // &biguplus;
				[506] = "\u22C1", // &bigvee;
				[512] = "\u22C0", // &bigwedge;
				[518] = "\u290D", // &bkarow;
				[530] = "\u29EB", // &blacklozenge;
				[537] = "\u25AA", // &blacksquare;
				[546] = "\u25B4", // &blacktriangle;
				[551] = "\u25BE", // &blacktriangledown;
				[556] = "\u25C2", // &blacktriangleleft;
				[562] = "\u25B8", // &blacktriangleright;
				[565] = "\u2423", // &blank;
				[569] = "\u2592", // &blk12;
				[571] = "\u2591", // &blk14;
				[574] = "\u2593", // &blk34;
				[578] = "\u2588", // &block;
				[581] = "\u003D\u20E5", // &bne;
				[586] = "\u2261\u20E5", // &bnequiv;
				[590] = "\u2AED", // &bNot;
				[593] = "\u2310", // &bnot;
				[597] = "\uD835\uDD39", // &Bopf;
				[601] = "\uD835\uDD53", // &bopf;
				[603] = "\u22A5", // &bot;
				[607] = "\u22A5", // &bottom;
				[612] = "\u22C8", // &bowtie;
				[617] = "\u29C9", // &boxbox;
				[620] = "\u2557", // &boxDL;
				[622] = "\u2556", // &boxDl;
				[625] = "\u2555", // &boxdL;
				[627] = "\u2510", // &boxdl;
				[629] = "\u2554", // &boxDR;
				[631] = "\u2553", // &boxDr;
				[633] = "\u2552", // &boxdR;
				[635] = "\u250C", // &boxdr;
				[637] = "\u2550", // &boxH;
				[639] = "\u2500", // &boxh;
				[641] = "\u2566", // &boxHD;
				[643] = "\u2564", // &boxHd;
				[645] = "\u2565", // &boxhD;
				[647] = "\u252C", // &boxhd;
				[649] = "\u2569", // &boxHU;
				[651] = "\u2567", // &boxHu;
				[653] = "\u2568", // &boxhU;
				[655] = "\u2534", // &boxhu;
				[661] = "\u229F", // &boxminus;
				[666] = "\u229E", // &boxplus;
				[672] = "\u22A0", // &boxtimes;
				[675] = "\u255D", // &boxUL;
				[677] = "\u255C", // &boxUl;
				[680] = "\u255B", // &boxuL;
				[682] = "\u2518", // &boxul;
				[684] = "\u255A", // &boxUR;
				[686] = "\u2559", // &boxUr;
				[688] = "\u2558", // &boxuR;
				[690] = "\u2514", // &boxur;
				[692] = "\u2551", // &boxV;
				[694] = "\u2502", // &boxv;
				[696] = "\u256C", // &boxVH;
				[698] = "\u256B", // &boxVh;
				[700] = "\u256A", // &boxvH;
				[702] = "\u253C", // &boxvh;
				[704] = "\u2563", // &boxVL;
				[706] = "\u2562", // &boxVl;
				[708] = "\u2561", // &boxvL;
				[710] = "\u2524", // &boxvl;
				[712] = "\u2560", // &boxVR;
				[714] = "\u255F", // &boxVr;
				[716] = "\u255E", // &boxvR;
				[718] = "\u251C", // &boxvr;
				[724] = "\u2035", // &bprime;
				[729] = "\u02D8", // &Breve;
				[734] = "\u02D8", // &breve;
				[738] = "\u00A6", // &brvbar
				[739] = "\u00A6", // &brvbar;
				[743] = "\u212C", // &Bscr;
				[747] = "\uD835\uDCB7", // &bscr;
				[751] = "\u204F", // &bsemi;
				[754] = "\u223D", // &bsim;
				[756] = "\u22CD", // &bsime;
				[759] = "\u005C", // &bsol;
				[761] = "\u29C5", // &bsolb;
				[766] = "\u27C8", // &bsolhsub;
				[770] = "\u2022", // &bull;
				[773] = "\u2022", // &bullet;
				[776] = "\u224E", // &bump;
				[778] = "\u2AAE", // &bumpE;
				[780] = "\u224F", // &bumpe;
				[786] = "\u224E", // &Bumpeq;
				[788] = "\u224F", // &bumpeq;
				[795] = "\u0106", // &Cacute;
				[802] = "\u0107", // &cacute;
				[804] = "\u22D2", // &Cap;
				[806] = "\u2229", // &cap;
				[810] = "\u2A44", // &capand;
				[816] = "\u2A49", // &capbrcup;
				[820] = "\u2A4B", // &capcap;
				[823] = "\u2A47", // &capcup;
				[827] = "\u2A40", // &capdot;
				[845] = "\u2145", // &CapitalDifferentialD;
				[847] = "\u2229\uFE00", // &caps;
				[851] = "\u2041", // &caret;
				[854] = "\u02C7", // &caron;
				[860] = "\u212D", // &Cayleys;
				[865] = "\u2A4D", // &ccaps;
				[871] = "\u010C", // &Ccaron;
				[875] = "\u010D", // &ccaron;
				[879] = "\u00C7", // &Ccedil
				[880] = "\u00C7", // &Ccedil;
				[884] = "\u00E7", // &ccedil
				[885] = "\u00E7", // &ccedil;
				[889] = "\u0108", // &Ccirc;
				[893] = "\u0109", // &ccirc;
				[899] = "\u2230", // &Cconint;
				[903] = "\u2A4C", // &ccups;
				[906] = "\u2A50", // &ccupssm;
				[910] = "\u010A", // &Cdot;
				[914] = "\u010B", // &cdot;
				[918] = "\u00B8", // &cedil
				[919] = "\u00B8", // &cedil;
				[926] = "\u00B8", // &Cedilla;
				[932] = "\u29B2", // &cemptyv;
				[934] = "\u00A2", // &cent
				[935] = "\u00A2", // &cent;
				[943] = "\u00B7", // &CenterDot;
				[949] = "\u00B7", // &centerdot;
				[952] = "\u212D", // &Cfr;
				[955] = "\uD835\uDD20", // &cfr;
				[959] = "\u0427", // &CHcy;
				[963] = "\u0447", // &chcy;
				[967] = "\u2713", // &check;
				[972] = "\u2713", // &checkmark;
				[975] = "\u03A7", // &Chi;
				[977] = "\u03C7", // &chi;
				[980] = "\u25CB", // &cir;
				[982] = "\u02C6", // &circ;
				[985] = "\u2257", // &circeq;
				[997] = "\u21BA", // &circlearrowleft;
				[1003] = "\u21BB", // &circlearrowright;
				[1008] = "\u229B", // &circledast;
				[1013] = "\u229A", // &circledcirc;
				[1018] = "\u229D", // &circleddash;
				[1027] = "\u2299", // &CircleDot;
				[1029] = "\u00AE", // &circledR;
				[1031] = "\u24C8", // &circledS;
				[1037] = "\u2296", // &CircleMinus;
				[1042] = "\u2295", // &CirclePlus;
				[1048] = "\u2297", // &CircleTimes;
				[1050] = "\u29C3", // &cirE;
				[1052] = "\u2257", // &cire;
				[1058] = "\u2A10", // &cirfnint;
				[1062] = "\u2AEF", // &cirmid;
				[1067] = "\u29C2", // &cirscir;
				[1091] = "\u2232", // &ClockwiseContourIntegral;
				[1110] = "\u201D", // &CloseCurlyDoubleQuote;
				[1116] = "\u2019", // &CloseCurlyQuote;
				[1121] = "\u2663", // &clubs;
				[1125] = "\u2663", // &clubsuit;
				[1130] = "\u2237", // &Colon;
				[1135] = "\u003A", // &colon;
				[1137] = "\u2A74", // &Colone;
				[1139] = "\u2254", // &colone;
				[1141] = "\u2254", // &coloneq;
				[1145] = "\u002C", // &comma;
				[1147] = "\u0040", // &commat;
				[1149] = "\u2201", // &comp;
				[1152] = "\u2218", // &compfn;
				[1159] = "\u2201", // &complement;
				[1163] = "\u2102", // &complexes;
				[1166] = "\u2245", // &cong;
				[1170] = "\u2A6D", // &congdot;
				[1178] = "\u2261", // &Congruent;
				[1182] = "\u222F", // &Conint;
				[1186] = "\u222E", // &conint;
				[1199] = "\u222E", // &ContourIntegral;
				[1202] = "\u2102", // &Copf;
				[1205] = "\uD835\uDD54", // &copf;
				[1209] = "\u2210", // &coprod;
				[1216] = "\u2210", // &Coproduct;
				[1219] = "\u00A9", // &COPY
				[1220] = "\u00A9", // &COPY;
				[1221] = "\u00A9", // &copy
				[1222] = "\u00A9", // &copy;
				[1225] = "\u2117", // &copysr;
				[1255] = "\u2233", // &CounterClockwiseContourIntegral;
				[1260] = "\u21B5", // &crarr;
				[1265] = "\u2A2F", // &Cross;
				[1269] = "\u2717", // &cross;
				[1273] = "\uD835\uDC9E", // &Cscr;
				[1277] = "\uD835\uDCB8", // &cscr;
				[1280] = "\u2ACF", // &csub;
				[1282] = "\u2AD1", // &csube;
				[1284] = "\u2AD0", // &csup;
				[1286] = "\u2AD2", // &csupe;
				[1291] = "\u22EF", // &ctdot;
				[1298] = "\u2938", // &cudarrl;
				[1300] = "\u2935", // &cudarrr;
				[1304] = "\u22DE", // &cuepr;
				[1307] = "\u22DF", // &cuesc;
				[1312] = "\u21B6", // &cularr;
				[1314] = "\u293D", // &cularrp;
				[1317] = "\u22D3", // &Cup;
				[1319] = "\u222A", // &cup;
				[1325] = "\u2A48", // &cupbrcap;
				[1329] = "\u224D", // &CupCap;
				[1333] = "\u2A46", // &cupcap;
				[1336] = "\u2A4A", // &cupcup;
				[1340] = "\u228D", // &cupdot;
				[1343] = "\u2A45", // &cupor;
				[1345] = "\u222A\uFE00", // &cups;
				[1350] = "\u21B7", // &curarr;
				[1352] = "\u293C", // &curarrm;
				[1361] = "\u22DE", // &curlyeqprec;
				[1366] = "\u22DF", // &curlyeqsucc;
				[1370] = "\u22CE", // &curlyvee;
				[1376] = "\u22CF", // &curlywedge;
				[1379] = "\u00A4", // &curren
				[1380] = "\u00A4", // &curren;
				[1392] = "\u21B6", // &curvearrowleft;
				[1398] = "\u21B7", // &curvearrowright;
				[1402] = "\u22CE", // &cuvee;
				[1406] = "\u22CF", // &cuwed;
				[1414] = "\u2232", // &cwconint;
				[1418] = "\u2231", // &cwint;
				[1424] = "\u232D", // &cylcty;
				[1431] = "\u2021", // &Dagger;
				[1438] = "\u2020", // &dagger;
				[1443] = "\u2138", // &daleth;
				[1446] = "\u21A1", // &Darr;
				[1450] = "\u21D3", // &dArr;
				[1453] = "\u2193", // &darr;
				[1456] = "\u2010", // &dash;
				[1460] = "\u2AE4", // &Dashv;
				[1462] = "\u22A3", // &dashv;
				[1469] = "\u290F", // &dbkarow;
				[1473] = "\u02DD", // &dblac;
				[1479] = "\u010E", // &Dcaron;
				[1485] = "\u010F", // &dcaron;
				[1487] = "\u0414", // &Dcy;
				[1489] = "\u0434", // &dcy;
				[1491] = "\u2145", // &DD;
				[1493] = "\u2146", // &dd;
				[1499] = "\u2021", // &ddagger;
				[1502] = "\u21CA", // &ddarr;
				[1509] = "\u2911", // &DDotrahd;
				[1515] = "\u2A77", // &ddotseq;
				[1517] = "\u00B0", // &deg
				[1518] = "\u00B0", // &deg;
				[1521] = "\u2207", // &Del;
				[1524] = "\u0394", // &Delta;
				[1528] = "\u03B4", // &delta;
				[1534] = "\u29B1", // &demptyv;
				[1540] = "\u297F", // &dfisht;
				[1543] = "\uD835\uDD07", // &Dfr;
				[1545] = "\uD835\uDD21", // &dfr;
				[1549] = "\u2965", // &dHar;
				[1554] = "\u21C3", // &dharl;
				[1556] = "\u21C2", // &dharr;
				[1572] = "\u00B4", // &DiacriticalAcute;
				[1576] = "\u02D9", // &DiacriticalDot;
				[1586] = "\u02DD", // &DiacriticalDoubleAcute;
				[1592] = "\u0060", // &DiacriticalGrave;
				[1598] = "\u02DC", // &DiacriticalTilde;
				[1602] = "\u22C4", // &diam;
				[1607] = "\u22C4", // &Diamond;
				[1611] = "\u22C4", // &diamond;
				[1616] = "\u2666", // &diamondsuit;
				[1618] = "\u2666", // &diams;
				[1620] = "\u00A8", // &die;
				[1632] = "\u2146", // &DifferentialD;
				[1638] = "\u03DD", // &digamma;
				[1642] = "\u22F2", // &disin;
				[1644] = "\u00F7", // &div;
				[1647] = "\u00F7", // &divide
				[1648] = "\u00F7", // &divide;
				[1656] = "\u22C7", // &divideontimes;
				[1660] = "\u22C7", // &divonx;
				[1664] = "\u0402", // &DJcy;
				[1668] = "\u0452", // &djcy;
				[1674] = "\u231E", // &dlcorn;
				[1678] = "\u230D", // &dlcrop;
				[1684] = "\u0024", // &dollar;
				[1688] = "\uD835\uDD3B", // &Dopf;
				[1691] = "\uD835\uDD55", // &dopf;
				[1693] = "\u00A8", // &Dot;
				[1695] = "\u02D9", // &dot;
				[1699] = "\u20DC", // &DotDot;
				[1702] = "\u2250", // &doteq;
				[1706] = "\u2251", // &doteqdot;
				[1712] = "\u2250", // &DotEqual;
				[1718] = "\u2238", // &dotminus;
				[1723] = "\u2214", // &dotplus;
				[1730] = "\u22A1", // &dotsquare;
				[1743] = "\u2306", // &doublebarwedge;
				[1763] = "\u222F", // &DoubleContourIntegral;
				[1767] = "\u00A8", // &DoubleDot;
				[1775] = "\u21D3", // &DoubleDownArrow;
				[1785] = "\u21D0", // &DoubleLeftArrow;
				[1796] = "\u21D4", // &DoubleLeftRightArrow;
				[1800] = "\u2AE4", // &DoubleLeftTee;
				[1813] = "\u27F8", // &DoubleLongLeftArrow;
				[1824] = "\u27FA", // &DoubleLongLeftRightArrow;
				[1835] = "\u27F9", // &DoubleLongRightArrow;
				[1846] = "\u21D2", // &DoubleRightArrow;
				[1850] = "\u22A8", // &DoubleRightTee;
				[1858] = "\u21D1", // &DoubleUpArrow;
				[1868] = "\u21D5", // &DoubleUpDownArrow;
				[1880] = "\u2225", // &DoubleVerticalBar;
				[1888] = "\u2193", // &DownArrow;
				[1894] = "\u21D3", // &Downarrow;
				[1902] = "\u2193", // &downarrow;
				[1906] = "\u2913", // &DownArrowBar;
				[1914] = "\u21F5", // &DownArrowUpArrow;
				[1920] = "\u0311", // &DownBreve;
				[1931] = "\u21CA", // &downdownarrows;
				[1943] = "\u21C3", // &downharpoonleft;
				[1949] = "\u21C2", // &downharpoonright;
				[1965] = "\u2950", // &DownLeftRightVector;
				[1975] = "\u295E", // &DownLeftTeeVector;
				[1982] = "\u21BD", // &DownLeftVector;
				[1986] = "\u2956", // &DownLeftVectorBar;
				[2001] = "\u295F", // &DownRightTeeVector;
				[2008] = "\u21C1", // &DownRightVector;
				[2012] = "\u2957", // &DownRightVectorBar;
				[2016] = "\u22A4", // &DownTee;
				[2022] = "\u21A7", // &DownTeeArrow;
				[2030] = "\u2910", // &drbkarow;
				[2035] = "\u231F", // &drcorn;
				[2039] = "\u230C", // &drcrop;
				[2043] = "\uD835\uDC9F", // &Dscr;
				[2047] = "\uD835\uDCB9", // &dscr;
				[2051] = "\u0405", // &DScy;
				[2053] = "\u0455", // &dscy;
				[2056] = "\u29F6", // &dsol;
				[2061] = "\u0110", // &Dstrok;
				[2066] = "\u0111", // &dstrok;
				[2071] = "\u22F1", // &dtdot;
				[2074] = "\u25BF", // &dtri;
				[2076] = "\u25BE", // &dtrif;
				[2081] = "\u21F5", // &duarr;
				[2085] = "\u296F", // &duhar;
				[2092] = "\u29A6", // &dwangle;
				[2096] = "\u040F", // &DZcy;
				[2100] = "\u045F", // &dzcy;
				[2107] = "\u27FF", // &dzigrarr;
				[2113] = "\u00C9", // &Eacute
				[2114] = "\u00C9", // &Eacute;
				[2120] = "\u00E9", // &eacute
				[2121] = "\u00E9", // &eacute;
				[2126] = "\u2A6E", // &easter;
				[2132] = "\u011A", // &Ecaron;
				[2138] = "\u011B", // &ecaron;
				[2141] = "\u2256", // &ecir;
				[2144] = "\u00CA", // &Ecirc
				[2145] = "\u00CA", // &Ecirc;
				[2146] = "\u00EA", // &ecirc
				[2147] = "\u00EA", // &ecirc;
				[2152] = "\u2255", // &ecolon;
				[2154] = "\u042D", // &Ecy;
				[2156] = "\u044D", // &ecy;
				[2161] = "\u2A77", // &eDDot;
				[2165] = "\u0116", // &Edot;
				[2168] = "\u2251", // &eDot;
				[2172] = "\u0117", // &edot;
				[2174] = "\u2147", // &ee;
				[2179] = "\u2252", // &efDot;
				[2182] = "\uD835\uDD08", // &Efr;
				[2184] = "\uD835\uDD22", // &efr;
				[2186] = "\u2A9A", // &eg;
				[2191] = "\u00C8", // &Egrave
				[2192] = "\u00C8", // &Egrave;
				[2196] = "\u00E8", // &egrave
				[2197] = "\u00E8", // &egrave;
				[2199] = "\u2A96", // &egs;
				[2203] = "\u2A98", // &egsdot;
				[2205] = "\u2A99", // &el;
				[2212] = "\u2208", // &Element;
				[2219] = "\u23E7", // &elinters;
				[2221] = "\u2113", // &ell;
				[2223] = "\u2A95", // &els;
				[2227] = "\u2A97", // &elsdot;
				[2232] = "\u0112", // &Emacr;
				[2237] = "\u0113", // &emacr;
				[2241] = "\u2205", // &empty;
				[2245] = "\u2205", // &emptyset;
				[2260] = "\u25FB", // &EmptySmallSquare;
				[2262] = "\u2205", // &emptyv;
				[2278] = "\u25AB", // &EmptyVerySmallSquare;
				[2281] = "\u2003", // &emsp;
				[2284] = "\u2004", // &emsp13;
				[2286] = "\u2005", // &emsp14;
				[2289] = "\u014A", // &ENG;
				[2292] = "\u014B", // &eng;
				[2295] = "\u2002", // &ensp;
				[2300] = "\u0118", // &Eogon;
				[2305] = "\u0119", // &eogon;
				[2308] = "\uD835\uDD3C", // &Eopf;
				[2311] = "\uD835\uDD56", // &eopf;
				[2315] = "\u22D5", // &epar;
				[2318] = "\u29E3", // &eparsl;
				[2322] = "\u2A71", // &eplus;
				[2325] = "\u03B5", // &epsi;
				[2332] = "\u0395", // &Epsilon;
				[2336] = "\u03B5", // &epsilon;
				[2338] = "\u03F5", // &epsiv;
				[2344] = "\u2256", // &eqcirc;
				[2349] = "\u2255", // &eqcolon;
				[2353] = "\u2242", // &eqsim;
				[2361] = "\u2A96", // &eqslantgtr;
				[2366] = "\u2A95", // &eqslantless;
				[2371] = "\u2A75", // &Equal;
				[2376] = "\u003D", // &equals;
				[2382] = "\u2242", // &EqualTilde;
				[2386] = "\u225F", // &equest;
				[2395] = "\u21CC", // &Equilibrium;
				[2398] = "\u2261", // &equiv;
				[2401] = "\u2A78", // &equivDD;
				[2408] = "\u29E5", // &eqvparsl;
				[2413] = "\u2971", // &erarr;
				[2417] = "\u2253", // &erDot;
				[2421] = "\u2130", // &Escr;
				[2425] = "\u212F", // &escr;
				[2429] = "\u2250", // &esdot;
				[2432] = "\u2A73", // &Esim;
				[2435] = "\u2242", // &esim;
				[2438] = "\u0397", // &Eta;
				[2441] = "\u03B7", // &eta;
				[2443] = "\u00D0", // &ETH
				[2444] = "\u00D0", // &ETH;
				[2445] = "\u00F0", // &eth
				[2446] = "\u00F0", // &eth;
				[2449] = "\u00CB", // &Euml
				[2450] = "\u00CB", // &Euml;
				[2453] = "\u00EB", // &euml
				[2454] = "\u00EB", // &euml;
				[2457] = "\u20AC", // &euro;
				[2461] = "\u0021", // &excl;
				[2465] = "\u2203", // &exist;
				[2471] = "\u2203", // &Exists;
				[2481] = "\u2130", // &expectation;
				[2492] = "\u2147", // &ExponentialE;
				[2502] = "\u2147", // &exponentiale;
				[2516] = "\u2252", // &fallingdotseq;
				[2520] = "\u0424", // &Fcy;
				[2523] = "\u0444", // &fcy;
				[2529] = "\u2640", // &female;
				[2535] = "\uFB03", // &ffilig;
				[2539] = "\uFB00", // &fflig;
				[2543] = "\uFB04", // &ffllig;
				[2546] = "\uD835\uDD09", // &Ffr;
				[2548] = "\uD835\uDD23", // &ffr;
				[2553] = "\uFB01", // &filig;
				[2570] = "\u25FC", // &FilledSmallSquare;
				[2586] = "\u25AA", // &FilledVerySmallSquare;
				[2591] = "\u0066\u006A", // &fjlig;
				[2595] = "\u266D", // &flat;
				[2599] = "\uFB02", // &fllig;
				[2603] = "\u25B1", // &fltns;
				[2607] = "\u0192", // &fnof;
				[2611] = "\uD835\uDD3D", // &Fopf;
				[2615] = "\uD835\uDD57", // &fopf;
				[2620] = "\u2200", // &ForAll;
				[2625] = "\u2200", // &forall;
				[2627] = "\u22D4", // &fork;
				[2629] = "\u2AD9", // &forkv;
				[2638] = "\u2131", // &Fouriertrf;
				[2646] = "\u2A0D", // &fpartint;
				[2651] = "\u00BD", // &frac12
				[2652] = "\u00BD", // &frac12;
				[2654] = "\u2153", // &frac13;
				[2655] = "\u00BC", // &frac14
				[2656] = "\u00BC", // &frac14;
				[2658] = "\u2155", // &frac15;
				[2660] = "\u2159", // &frac16;
				[2662] = "\u215B", // &frac18;
				[2665] = "\u2154", // &frac23;
				[2667] = "\u2156", // &frac25;
				[2669] = "\u00BE", // &frac34
				[2670] = "\u00BE", // &frac34;
				[2672] = "\u2157", // &frac35;
				[2674] = "\u215C", // &frac38;
				[2677] = "\u2158", // &frac45;
				[2680] = "\u215A", // &frac56;
				[2682] = "\u215D", // &frac58;
				[2685] = "\u215E", // &frac78;
				[2688] = "\u2044", // &frasl;
				[2692] = "\u2322", // &frown;
				[2696] = "\u2131", // &Fscr;
				[2700] = "\uD835\uDCBB", // &fscr;
				[2707] = "\u01F5", // &gacute;
				[2713] = "\u0393", // &Gamma;
				[2717] = "\u03B3", // &gamma;
				[2719] = "\u03DC", // &Gammad;
				[2721] = "\u03DD", // &gammad;
				[2723] = "\u2A86", // &gap;
				[2729] = "\u011E", // &Gbreve;
				[2735] = "\u011F", // &gbreve;
				[2741] = "\u0122", // &Gcedil;
				[2745] = "\u011C", // &Gcirc;
				[2750] = "\u011D", // &gcirc;
				[2752] = "\u0413", // &Gcy;
				[2754] = "\u0433", // &gcy;
				[2758] = "\u0120", // &Gdot;
				[2762] = "\u0121", // &gdot;
				[2764] = "\u2267", // &gE;
				[2766] = "\u2265", // &ge;
				[2768] = "\u2A8C", // &gEl;
				[2770] = "\u22DB", // &gel;
				[2772] = "\u2265", // &geq;
				[2774] = "\u2267", // &geqq;
				[2780] = "\u2A7E", // &geqslant;
				[2782] = "\u2A7E", // &ges;
				[2785] = "\u2AA9", // &gescc;
				[2789] = "\u2A80", // &gesdot;
				[2791] = "\u2A82", // &gesdoto;
				[2793] = "\u2A84", // &gesdotol;
				[2795] = "\u22DB\uFE00", // &gesl;
				[2798] = "\u2A94", // &gesles;
				[2801] = "\uD835\uDD0A", // &Gfr;
				[2804] = "\uD835\uDD24", // &gfr;
				[2806] = "\u22D9", // &Gg;
				[2808] = "\u226B", // &gg;
				[2810] = "\u22D9", // &ggg;
				[2815] = "\u2137", // &gimel;
				[2819] = "\u0403", // &GJcy;
				[2823] = "\u0453", // &gjcy;
				[2825] = "\u2277", // &gl;
				[2827] = "\u2AA5", // &gla;
				[2829] = "\u2A92", // &glE;
				[2831] = "\u2AA4", // &glj;
				[2835] = "\u2A8A", // &gnap;
				[2840] = "\u2A8A", // &gnapprox;
				[2842] = "\u2269", // &gnE;
				[2844] = "\u2A88", // &gne;
				[2846] = "\u2A88", // &gneq;
				[2848] = "\u2269", // &gneqq;
				[2852] = "\u22E7", // &gnsim;
				[2856] = "\uD835\uDD3E", // &Gopf;
				[2860] = "\uD835\uDD58", // &gopf;
				[2865] = "\u0060", // &grave;
				[2877] = "\u2265", // &GreaterEqual;
				[2882] = "\u22DB", // &GreaterEqualLess;
				[2892] = "\u2267", // &GreaterFullEqual;
				[2900] = "\u2AA2", // &GreaterGreater;
				[2905] = "\u2277", // &GreaterLess;
				[2916] = "\u2A7E", // &GreaterSlantEqual;
				[2922] = "\u2273", // &GreaterTilde;
				[2926] = "\uD835\uDCA2", // &Gscr;
				[2930] = "\u210A", // &gscr;
				[2933] = "\u2273", // &gsim;
				[2935] = "\u2A8E", // &gsime;
				[2937] = "\u2A90", // &gsiml;
				[2938] = "\u003E", // &GT
				[2939] = "\u003E", // &GT;
				[2941] = "\u226B", // &Gt;
				[2942] = "\u003E", // &gt
				[2943] = "\u003E", // &gt;
				[2946] = "\u2AA7", // &gtcc;
				[2949] = "\u2A7A", // &gtcir;
				[2953] = "\u22D7", // &gtdot;
				[2958] = "\u2995", // &gtlPar;
				[2964] = "\u2A7C", // &gtquest;
				[2972] = "\u2A86", // &gtrapprox;
				[2975] = "\u2978", // &gtrarr;
				[2979] = "\u22D7", // &gtrdot;
				[2986] = "\u22DB", // &gtreqless;
				[2992] = "\u2A8C", // &gtreqqless;
				[2997] = "\u2277", // &gtrless;
				[3001] = "\u2273", // &gtrsim;
				[3010] = "\u2269\uFE00", // &gvertneqq;
				[3013] = "\u2269\uFE00", // &gvnE;
				[3019] = "\u02C7", // &Hacek;
				[3026] = "\u200A", // &hairsp;
				[3029] = "\u00BD", // &half;
				[3034] = "\u210B", // &hamilt;
				[3040] = "\u042A", // &HARDcy;
				[3045] = "\u044A", // &hardcy;
				[3049] = "\u21D4", // &hArr;
				[3051] = "\u2194", // &harr;
				[3055] = "\u2948", // &harrcir;
				[3057] = "\u21AD", // &harrw;
				[3059] = "\u005E", // &Hat;
				[3063] = "\u210F", // &hbar;
				[3068] = "\u0124", // &Hcirc;
				[3073] = "\u0125", // &hcirc;
				[3079] = "\u2665", // &hearts;
				[3083] = "\u2665", // &heartsuit;
				[3088] = "\u2026", // &hellip;
				[3093] = "\u22B9", // &hercon;
				[3096] = "\u210C", // &Hfr;
				[3099] = "\uD835\uDD25", // &hfr;
				[3111] = "\u210B", // &HilbertSpace;
				[3119] = "\u2925", // &hksearow;
				[3125] = "\u2926", // &hkswarow;
				[3130] = "\u21FF", // &hoarr;
				[3135] = "\u223B", // &homtht;
				[3147] = "\u21A9", // &hookleftarrow;
				[3158] = "\u21AA", // &hookrightarrow;
				[3162] = "\u210D", // &Hopf;
				[3165] = "\uD835\uDD59", // &hopf;
				[3170] = "\u2015", // &horbar;
				[3183] = "\u2500", // &HorizontalLine;
				[3187] = "\u210B", // &Hscr;
				[3191] = "\uD835\uDCBD", // &hscr;
				[3196] = "\u210F", // &hslash;
				[3201] = "\u0126", // &Hstrok;
				[3206] = "\u0127", // &hstrok;
				[3218] = "\u224E", // &HumpDownHump;
				[3224] = "\u224F", // &HumpEqual;
				[3230] = "\u2043", // &hybull;
				[3235] = "\u2010", // &hyphen;
				[3241] = "\u00CD", // &Iacute
				[3242] = "\u00CD", // &Iacute;
				[3248] = "\u00ED", // &iacute
				[3249] = "\u00ED", // &iacute;
				[3251] = "\u2063", // &ic;
				[3255] = "\u00CE", // &Icirc
				[3256] = "\u00CE", // &Icirc;
				[3259] = "\u00EE", // &icirc
				[3260] = "\u00EE", // &icirc;
				[3262] = "\u0418", // &Icy;
				[3264] = "\u0438", // &icy;
				[3268] = "\u0130", // &Idot;
				[3272] = "\u0415", // &IEcy;
				[3276] = "\u0435", // &iecy;
				[3279] = "\u00A1", // &iexcl
				[3280] = "\u00A1", // &iexcl;
				[3283] = "\u21D4", // &iff;
				[3286] = "\u2111", // &Ifr;
				[3288] = "\uD835\uDD26", // &ifr;
				[3293] = "\u00CC", // &Igrave
				[3294] = "\u00CC", // &Igrave;
				[3299] = "\u00EC", // &igrave
				[3300] = "\u00EC", // &igrave;
				[3302] = "\u2148", // &ii;
				[3307] = "\u2A0C", // &iiiint;
				[3310] = "\u222D", // &iiint;
				[3315] = "\u29DC", // &iinfin;
				[3319] = "\u2129", // &iiota;
				[3324] = "\u0132", // &IJlig;
				[3329] = "\u0133", // &ijlig;
				[3331] = "\u2111", // &Im;
				[3335] = "\u012A", // &Imacr;
				[3340] = "\u012B", // &imacr;
				[3343] = "\u2111", // &image;
				[3351] = "\u2148", // &ImaginaryI;
				[3356] = "\u2110", // &imagline;
				[3361] = "\u2111", // &imagpart;
				[3364] = "\u0131", // &imath;
				[3367] = "\u22B7", // &imof;
				[3371] = "\u01B5", // &imped;
				[3377] = "\u21D2", // &Implies;
				[3379] = "\u2208", // &in;
				[3384] = "\u2105", // &incare;
				[3388] = "\u221E", // &infin;
				[3392] = "\u29DD", // &infintie;
				[3397] = "\u0131", // &inodot;
				[3400] = "\u222C", // &Int;
				[3402] = "\u222B", // &int;
				[3406] = "\u22BA", // &intcal;
				[3412] = "\u2124", // &integers;
				[3418] = "\u222B", // &Integral;
				[3423] = "\u22BA", // &intercal;
				[3432] = "\u22C2", // &Intersection;
				[3438] = "\u2A17", // &intlarhk;
				[3443] = "\u2A3C", // &intprod;
				[3456] = "\u2063", // &InvisibleComma;
				[3462] = "\u2062", // &InvisibleTimes;
				[3466] = "\u0401", // &IOcy;
				[3470] = "\u0451", // &iocy;
				[3475] = "\u012E", // &Iogon;
				[3479] = "\u012F", // &iogon;
				[3482] = "\uD835\uDD40", // &Iopf;
				[3485] = "\uD835\uDD5A", // &iopf;
				[3488] = "\u0399", // &Iota;
				[3491] = "\u03B9", // &iota;
				[3496] = "\u2A3C", // &iprod;
				[3501] = "\u00BF", // &iquest
				[3502] = "\u00BF", // &iquest;
				[3506] = "\u2110", // &Iscr;
				[3510] = "\uD835\uDCBE", // &iscr;
				[3513] = "\u2208", // &isin;
				[3517] = "\u22F5", // &isindot;
				[3519] = "\u22F9", // &isinE;
				[3521] = "\u22F4", // &isins;
				[3523] = "\u22F3", // &isinsv;
				[3525] = "\u2208", // &isinv;
				[3527] = "\u2062", // &it;
				[3533] = "\u0128", // &Itilde;
				[3538] = "\u0129", // &itilde;
				[3543] = "\u0406", // &Iukcy;
				[3548] = "\u0456", // &iukcy;
				[3550] = "\u00CF", // &Iuml
				[3551] = "\u00CF", // &Iuml;
				[3553] = "\u00EF", // &iuml
				[3554] = "\u00EF", // &iuml;
				[3560] = "\u0134", // &Jcirc;
				[3566] = "\u0135", // &jcirc;
				[3568] = "\u0419", // &Jcy;
				[3570] = "\u0439", // &jcy;
				[3573] = "\uD835\uDD0D", // &Jfr;
				[3576] = "\uD835\uDD27", // &jfr;
				[3581] = "\u0237", // &jmath;
				[3585] = "\uD835\uDD41", // &Jopf;
				[3589] = "\uD835\uDD5B", // &jopf;
				[3593] = "\uD835\uDCA5", // &Jscr;
				[3597] = "\uD835\uDCBF", // &jscr;
				[3602] = "\u0408", // &Jsercy;
				[3607] = "\u0458", // &jsercy;
				[3612] = "\u0404", // &Jukcy;
				[3617] = "\u0454", // &jukcy;
				[3623] = "\u039A", // &Kappa;
				[3629] = "\u03BA", // &kappa;
				[3631] = "\u03F0", // &kappav;
				[3637] = "\u0136", // &Kcedil;
				[3643] = "\u0137", // &kcedil;
				[3645] = "\u041A", // &Kcy;
				[3647] = "\u043A", // &kcy;
				[3650] = "\uD835\uDD0E", // &Kfr;
				[3653] = "\uD835\uDD28", // &kfr;
				[3659] = "\u0138", // &kgreen;
				[3663] = "\u0425", // &KHcy;
				[3667] = "\u0445", // &khcy;
				[3671] = "\u040C", // &KJcy;
				[3675] = "\u045C", // &kjcy;
				[3679] = "\uD835\uDD42", // &Kopf;
				[3683] = "\uD835\uDD5C", // &kopf;
				[3687] = "\uD835\uDCA6", // &Kscr;
				[3691] = "\uD835\uDCC0", // &kscr;
				[3697] = "\u21DA", // &lAarr;
				[3704] = "\u0139", // &Lacute;
				[3710] = "\u013A", // &lacute;
				[3717] = "\u29B4", // &laemptyv;
				[3722] = "\u2112", // &lagran;
				[3727] = "\u039B", // &Lambda;
				[3732] = "\u03BB", // &lambda;
				[3735] = "\u27EA", // &Lang;
				[3738] = "\u27E8", // &lang;
				[3740] = "\u2991", // &langd;
				[3743] = "\u27E8", // &langle;
				[3745] = "\u2A85", // &lap;
				[3754] = "\u2112", // &Laplacetrf;
				[3757] = "\u00AB", // &laquo
				[3758] = "\u00AB", // &laquo;
				[3761] = "\u219E", // &Larr;
				[3764] = "\u21D0", // &lArr;
				[3767] = "\u2190", // &larr;
				[3769] = "\u21E4", // &larrb;
				[3772] = "\u291F", // &larrbfs;
				[3775] = "\u291D", // &larrfs;
				[3778] = "\u21A9", // &larrhk;
				[3781] = "\u21AB", // &larrlp;
				[3784] = "\u2939", // &larrpl;
				[3788] = "\u2973", // &larrsim;
				[3791] = "\u21A2", // &larrtl;
				[3793] = "\u2AAB", // &lat;
				[3798] = "\u291B", // &lAtail;
				[3802] = "\u2919", // &latail;
				[3804] = "\u2AAD", // &late;
				[3806] = "\u2AAD\uFE00", // &lates;
				[3811] = "\u290E", // &lBarr;
				[3816] = "\u290C", // &lbarr;
				[3820] = "\u2772", // &lbbrk;
				[3825] = "\u007B", // &lbrace;
				[3827] = "\u005B", // &lbrack;
				[3830] = "\u298B", // &lbrke;
				[3834] = "\u298F", // &lbrksld;
				[3836] = "\u298D", // &lbrkslu;
				[3842] = "\u013D", // &Lcaron;
				[3848] = "\u013E", // &lcaron;
				[3853] = "\u013B", // &Lcedil;
				[3858] = "\u013C", // &lcedil;
				[3861] = "\u2308", // &lceil;
				[3864] = "\u007B", // &lcub;
				[3866] = "\u041B", // &Lcy;
				[3868] = "\u043B", // &lcy;
				[3872] = "\u2936", // &ldca;
				[3876] = "\u201C", // &ldquo;
				[3878] = "\u201E", // &ldquor;
				[3884] = "\u2967", // &ldrdhar;
				[3890] = "\u294B", // &ldrushar;
				[3893] = "\u21B2", // &ldsh;
				[3895] = "\u2266", // &lE;
				[3897] = "\u2264", // &le;
				[3913] = "\u27E8", // &LeftAngleBracket;
				[3918] = "\u2190", // &LeftArrow;
				[3924] = "\u21D0", // &Leftarrow;
				[3932] = "\u2190", // &leftarrow;
				[3936] = "\u21E4", // &LeftArrowBar;
				[3947] = "\u21C6", // &LeftArrowRightArrow;
				[3952] = "\u21A2", // &leftarrowtail;
				[3960] = "\u2308", // &LeftCeiling;
				[3974] = "\u27E6", // &LeftDoubleBracket;
				[3986] = "\u2961", // &LeftDownTeeVector;
				[3993] = "\u21C3", // &LeftDownVector;
				[3997] = "\u2959", // &LeftDownVectorBar;
				[4003] = "\u230A", // &LeftFloor;
				[4015] = "\u21BD", // &leftharpoondown;
				[4018] = "\u21BC", // &leftharpoonup;
				[4029] = "\u21C7", // &leftleftarrows;
				[4040] = "\u2194", // &LeftRightArrow;
				[4051] = "\u21D4", // &Leftrightarrow;
				[4062] = "\u2194", // &leftrightarrow;
				[4064] = "\u21C6", // &leftrightarrows;
				[4073] = "\u21CB", // &leftrightharpoons;
				[4084] = "\u21AD", // &leftrightsquigarrow;
				[4091] = "\u294E", // &LeftRightVector;
				[4095] = "\u22A3", // &LeftTee;
				[4101] = "\u21A4", // &LeftTeeArrow;
				[4108] = "\u295A", // &LeftTeeVector;
				[4119] = "\u22CB", // &leftthreetimes;
				[4127] = "\u22B2", // &LeftTriangle;
				[4131] = "\u29CF", // &LeftTriangleBar;
				[4137] = "\u22B4", // &LeftTriangleEqual;
				[4150] = "\u2951", // &LeftUpDownVector;
				[4160] = "\u2960", // &LeftUpTeeVector;
				[4167] = "\u21BF", // &LeftUpVector;
				[4171] = "\u2958", // &LeftUpVectorBar;
				[4178] = "\u21BC", // &LeftVector;
				[4182] = "\u2952", // &LeftVectorBar;
				[4184] = "\u2A8B", // &lEg;
				[4186] = "\u22DA", // &leg;
				[4188] = "\u2264", // &leq;
				[4190] = "\u2266", // &leqq;
				[4196] = "\u2A7D", // &leqslant;
				[4198] = "\u2A7D", // &les;
				[4201] = "\u2AA8", // &lescc;
				[4205] = "\u2A7F", // &lesdot;
				[4207] = "\u2A81", // &lesdoto;
				[4209] = "\u2A83", // &lesdotor;
				[4211] = "\u22DA\uFE00", // &lesg;
				[4214] = "\u2A93", // &lesges;
				[4222] = "\u2A85", // &lessapprox;
				[4226] = "\u22D6", // &lessdot;
				[4232] = "\u22DA", // &lesseqgtr;
				[4237] = "\u2A8B", // &lesseqqgtr;
				[4252] = "\u22DA", // &LessEqualGreater;
				[4262] = "\u2266", // &LessFullEqual;
				[4270] = "\u2276", // &LessGreater;
				[4274] = "\u2276", // &lessgtr;
				[4279] = "\u2AA1", // &LessLess;
				[4283] = "\u2272", // &lesssim;
				[4294] = "\u2A7D", // &LessSlantEqual;
				[4300] = "\u2272", // &LessTilde;
				[4306] = "\u297C", // &lfisht;
				[4311] = "\u230A", // &lfloor;
				[4314] = "\uD835\uDD0F", // &Lfr;
				[4316] = "\uD835\uDD29", // &lfr;
				[4318] = "\u2276", // &lg;
				[4320] = "\u2A91", // &lgE;
				[4324] = "\u2962", // &lHar;
				[4329] = "\u21BD", // &lhard;
				[4331] = "\u21BC", // &lharu;
				[4333] = "\u296A", // &lharul;
				[4337] = "\u2584", // &lhblk;
				[4341] = "\u0409", // &LJcy;
				[4345] = "\u0459", // &ljcy;
				[4347] = "\u22D8", // &Ll;
				[4349] = "\u226A", // &ll;
				[4353] = "\u21C7", // &llarr;
				[4360] = "\u231E", // &llcorner;
				[4369] = "\u21DA", // &Lleftarrow;
				[4374] = "\u296B", // &llhard;
				[4378] = "\u25FA", // &lltri;
				[4384] = "\u013F", // &Lmidot;
				[4390] = "\u0140", // &lmidot;
				[4395] = "\u23B0", // &lmoust;
				[4400] = "\u23B0", // &lmoustache;
				[4404] = "\u2A89", // &lnap;
				[4409] = "\u2A89", // &lnapprox;
				[4411] = "\u2268", // &lnE;
				[4413] = "\u2A87", // &lne;
				[4415] = "\u2A87", // &lneq;
				[4417] = "\u2268", // &lneqq;
				[4421] = "\u22E6", // &lnsim;
				[4426] = "\u27EC", // &loang;
				[4429] = "\u21FD", // &loarr;
				[4433] = "\u27E6", // &lobrk;
				[4446] = "\u27F5", // &LongLeftArrow;
				[4456] = "\u27F8", // &Longleftarrow;
				[4468] = "\u27F5", // &longleftarrow;
				[4479] = "\u27F7", // &LongLeftRightArrow;
				[4490] = "\u27FA", // &Longleftrightarrow;
				[4501] = "\u27F7", // &longleftrightarrow;
				[4508] = "\u27FC", // &longmapsto;
				[4519] = "\u27F6", // &LongRightArrow;
				[4530] = "\u27F9", // &Longrightarrow;
				[4541] = "\u27F6", // &longrightarrow;
				[4553] = "\u21AB", // &looparrowleft;
				[4559] = "\u21AC", // &looparrowright;
				[4563] = "\u2985", // &lopar;
				[4566] = "\uD835\uDD43", // &Lopf;
				[4568] = "\uD835\uDD5D", // &lopf;
				[4572] = "\u2A2D", // &loplus;
				[4578] = "\u2A34", // &lotimes;
				[4583] = "\u2217", // &lowast;
				[4587] = "\u005F", // &lowbar;
				[4600] = "\u2199", // &LowerLeftArrow;
				[4611] = "\u2198", // &LowerRightArrow;
				[4613] = "\u25CA", // &loz;
				[4618] = "\u25CA", // &lozenge;
				[4620] = "\u29EB", // &lozf;
				[4624] = "\u0028", // &lpar;
				[4627] = "\u2993", // &lparlt;
				[4632] = "\u21C6", // &lrarr;
				[4639] = "\u231F", // &lrcorner;
				[4643] = "\u21CB", // &lrhar;
				[4645] = "\u296D", // &lrhard;
				[4647] = "\u200E", // &lrm;
				[4651] = "\u22BF", // &lrtri;
				[4657] = "\u2039", // &lsaquo;
				[4661] = "\u2112", // &Lscr;
				[4664] = "\uD835\uDCC1", // &lscr;
				[4666] = "\u21B0", // &Lsh;
				[4668] = "\u21B0", // &lsh;
				[4671] = "\u2272", // &lsim;
				[4673] = "\u2A8D", // &lsime;
				[4675] = "\u2A8F", // &lsimg;
				[4678] = "\u005B", // &lsqb;
				[4681] = "\u2018", // &lsquo;
				[4683] = "\u201A", // &lsquor;
				[4688] = "\u0141", // &Lstrok;
				[4693] = "\u0142", // &lstrok;
				[4694] = "\u003C", // &LT
				[4695] = "\u003C", // &LT;
				[4697] = "\u226A", // &Lt;
				[4698] = "\u003C", // &lt
				[4699] = "\u003C", // &lt;
				[4702] = "\u2AA6", // &ltcc;
				[4705] = "\u2A79", // &ltcir;
				[4709] = "\u22D6", // &ltdot;
				[4714] = "\u22CB", // &lthree;
				[4719] = "\u22C9", // &ltimes;
				[4724] = "\u2976", // &ltlarr;
				[4730] = "\u2A7B", // &ltquest;
				[4733] = "\u25C3", // &ltri;
				[4735] = "\u22B4", // &ltrie;
				[4737] = "\u25C2", // &ltrif;
				[4741] = "\u2996", // &ltrPar;
				[4749] = "\u294A", // &lurdshar;
				[4754] = "\u2966", // &luruhar;
				[4763] = "\u2268\uFE00", // &lvertneqq;
				[4766] = "\u2268\uFE00", // &lvnE;
				[4770] = "\u00AF", // &macr
				[4771] = "\u00AF", // &macr;
				[4774] = "\u2642", // &male;
				[4776] = "\u2720", // &malt;
				[4780] = "\u2720", // &maltese;
				[4784] = "\u2905", // &Map;
				[4786] = "\u21A6", // &map;
				[4790] = "\u21A6", // &mapsto;
				[4795] = "\u21A7", // &mapstodown;
				[4800] = "\u21A4", // &mapstoleft;
				[4803] = "\u21A5", // &mapstoup;
				[4808] = "\u25AE", // &marker;
				[4814] = "\u2A29", // &mcomma;
				[4817] = "\u041C", // &Mcy;
				[4819] = "\u043C", // &mcy;
				[4824] = "\u2014", // &mdash;
				[4829] = "\u223A", // &mDDot;
				[4842] = "\u2221", // &measuredangle;
				[4853] = "\u205F", // &MediumSpace;
				[4861] = "\u2133", // &Mellintrf;
				[4864] = "\uD835\uDD10", // &Mfr;
				[4867] = "\uD835\uDD2A", // &mfr;
				[4870] = "\u2127", // &mho;
				[4874] = "\u00B5", // &micro
				[4875] = "\u00B5", // &micro;
				[4877] = "\u2223", // &mid;
				[4881] = "\u002A", // &midast;
				[4885] = "\u2AF0", // &midcir;
				[4888] = "\u00B7", // &middot
				[4889] = "\u00B7", // &middot;
				[4893] = "\u2212", // &minus;
				[4895] = "\u229F", // &minusb;
				[4897] = "\u2238", // &minusd;
				[4899] = "\u2A2A", // &minusdu;
				[4908] = "\u2213", // &MinusPlus;
				[4912] = "\u2ADB", // &mlcp;
				[4915] = "\u2026", // &mldr;
				[4921] = "\u2213", // &mnplus;
				[4927] = "\u22A7", // &models;
				[4931] = "\uD835\uDD44", // &Mopf;
				[4934] = "\uD835\uDD5E", // &mopf;
				[4936] = "\u2213", // &mp;
				[4940] = "\u2133", // &Mscr;
				[4944] = "\uD835\uDCC2", // &mscr;
				[4949] = "\u223E", // &mstpos;
				[4951] = "\u039C", // &Mu;
				[4953] = "\u03BC", // &mu;
				[4960] = "\u22B8", // &multimap;
				[4964] = "\u22B8", // &mumap;
				[4970] = "\u2207", // &nabla;
				[4977] = "\u0143", // &Nacute;
				[4982] = "\u0144", // &nacute;
				[4985] = "\u2220\u20D2", // &nang;
				[4987] = "\u2249", // &nap;
				[4989] = "\u2A70\u0338", // &napE;
				[4992] = "\u224B\u0338", // &napid;
				[4995] = "\u0149", // &napos;
				[5000] = "\u2249", // &napprox;
				[5004] = "\u266E", // &natur;
				[5007] = "\u266E", // &natural;
				[5009] = "\u2115", // &naturals;
				[5012] = "\u00A0", // &nbsp
				[5013] = "\u00A0", // &nbsp;
				[5017] = "\u224E\u0338", // &nbump;
				[5019] = "\u224F\u0338", // &nbumpe;
				[5023] = "\u2A43", // &ncap;
				[5029] = "\u0147", // &Ncaron;
				[5033] = "\u0148", // &ncaron;
				[5038] = "\u0145", // &Ncedil;
				[5043] = "\u0146", // &ncedil;
				[5047] = "\u2247", // &ncong;
				[5051] = "\u2A6D\u0338", // &ncongdot;
				[5054] = "\u2A42", // &ncup;
				[5056] = "\u041D", // &Ncy;
				[5058] = "\u043D", // &ncy;
				[5063] = "\u2013", // &ndash;
				[5065] = "\u2260", // &ne;
				[5070] = "\u2924", // &nearhk;
				[5074] = "\u21D7", // &neArr;
				[5076] = "\u2197", // &nearr;
				[5079] = "\u2197", // &nearrow;
				[5083] = "\u2250\u0338", // &nedot;
				[5102] = "\u200B", // &NegativeMediumSpace;
				[5113] = "\u200B", // &NegativeThickSpace;
				[5120] = "\u200B", // &NegativeThinSpace;
				[5134] = "\u200B", // &NegativeVeryThinSpace;
				[5139] = "\u2262", // &nequiv;
				[5144] = "\u2928", // &nesear;
				[5147] = "\u2242\u0338", // &nesim;
				[5166] = "\u226B", // &NestedGreaterGreater;
				[5175] = "\u226A", // &NestedLessLess;
				[5181] = "\u000A", // &NewLine;
				[5186] = "\u2204", // &nexist;
				[5188] = "\u2204", // &nexists;
				[5191] = "\uD835\uDD11", // &Nfr;
				[5194] = "\uD835\uDD2B", // &nfr;
				[5197] = "\u2267\u0338", // &ngE;
				[5199] = "\u2271", // &nge;
				[5201] = "\u2271", // &ngeq;
				[5203] = "\u2267\u0338", // &ngeqq;
				[5209] = "\u2A7E\u0338", // &ngeqslant;
				[5211] = "\u2A7E\u0338", // &nges;
				[5214] = "\u22D9\u0338", // &nGg;
				[5218] = "\u2275", // &ngsim;
				[5220] = "\u226B\u20D2", // &nGt;
				[5222] = "\u226F", // &ngt;
				[5224] = "\u226F", // &ngtr;
				[5226] = "\u226B\u0338", // &nGtv;
				[5231] = "\u21CE", // &nhArr;
				[5235] = "\u21AE", // &nharr;
				[5239] = "\u2AF2", // &nhpar;
				[5241] = "\u220B", // &ni;
				[5243] = "\u22FC", // &nis;
				[5245] = "\u22FA", // &nisd;
				[5247] = "\u220B", // &niv;
				[5251] = "\u040A", // &NJcy;
				[5255] = "\u045A", // &njcy;
				[5260] = "\u21CD", // &nlArr;
				[5264] = "\u219A", // &nlarr;
				[5267] = "\u2025", // &nldr;
				[5269] = "\u2266\u0338", // &nlE;
				[5271] = "\u2270", // &nle;
				[5281] = "\u21CD", // &nLeftarrow;
				[5289] = "\u219A", // &nleftarrow;
				[5300] = "\u21CE", // &nLeftrightarrow;
				[5311] = "\u21AE", // &nleftrightarrow;
				[5313] = "\u2270", // &nleq;
				[5315] = "\u2266\u0338", // &nleqq;
				[5321] = "\u2A7D\u0338", // &nleqslant;
				[5323] = "\u2A7D\u0338", // &nles;
				[5325] = "\u226E", // &nless;
				[5327] = "\u22D8\u0338", // &nLl;
				[5331] = "\u2274", // &nlsim;
				[5333] = "\u226A\u20D2", // &nLt;
				[5335] = "\u226E", // &nlt;
				[5338] = "\u22EA", // &nltri;
				[5340] = "\u22EC", // &nltrie;
				[5342] = "\u226A\u0338", // &nLtv;
				[5346] = "\u2224", // &nmid;
				[5353] = "\u2060", // &NoBreak;
				[5368] = "\u00A0", // &NonBreakingSpace;
				[5371] = "\u2115", // &Nopf;
				[5375] = "\uD835\uDD5F", // &nopf;
				[5377] = "\u2AEC", // &Not;
				[5378] = "\u00AC", // &not
				[5379] = "\u00AC", // &not;
				[5389] = "\u2262", // &NotCongruent;
				[5395] = "\u226D", // &NotCupCap;
				[5413] = "\u2226", // &NotDoubleVerticalBar;
				[5421] = "\u2209", // &NotElement;
				[5426] = "\u2260", // &NotEqual;
				[5432] = "\u2242\u0338", // &NotEqualTilde;
				[5438] = "\u2204", // &NotExists;
				[5446] = "\u226F", // &NotGreater;
				[5452] = "\u2271", // &NotGreaterEqual;
				[5462] = "\u2267\u0338", // &NotGreaterFullEqual;
				[5470] = "\u226B\u0338", // &NotGreaterGreater;
				[5475] = "\u2279", // &NotGreaterLess;
				[5486] = "\u2A7E\u0338", // &NotGreaterSlantEqual;
				[5492] = "\u2275", // &NotGreaterTilde;
				[5505] = "\u224E\u0338", // &NotHumpDownHump;
				[5511] = "\u224F\u0338", // &NotHumpEqual;
				[5514] = "\u2209", // &notin;
				[5518] = "\u22F5\u0338", // &notindot;
				[5520] = "\u22F9\u0338", // &notinE;
				[5523] = "\u2209", // &notinva;
				[5525] = "\u22F7", // &notinvb;
				[5527] = "\u22F6", // &notinvc;
				[5540] = "\u22EA", // &NotLeftTriangle;
				[5544] = "\u29CF\u0338", // &NotLeftTriangleBar;
				[5550] = "\u22EC", // &NotLeftTriangleEqual;
				[5553] = "\u226E", // &NotLess;
				[5559] = "\u2270", // &NotLessEqual;
				[5567] = "\u2278", // &NotLessGreater;
				[5572] = "\u226A\u0338", // &NotLessLess;
				[5583] = "\u2A7D\u0338", // &NotLessSlantEqual;
				[5589] = "\u2274", // &NotLessTilde;
				[5610] = "\u2AA2\u0338", // &NotNestedGreaterGreater;
				[5619] = "\u2AA1\u0338", // &NotNestedLessLess;
				[5622] = "\u220C", // &notni;
				[5625] = "\u220C", // &notniva;
				[5627] = "\u22FE", // &notnivb;
				[5629] = "\u22FD", // &notnivc;
				[5638] = "\u2280", // &NotPrecedes;
				[5644] = "\u2AAF\u0338", // &NotPrecedesEqual;
				[5655] = "\u22E0", // &NotPrecedesSlantEqual;
				[5670] = "\u220C", // &NotReverseElement;
				[5683] = "\u22EB", // &NotRightTriangle;
				[5687] = "\u29D0\u0338", // &NotRightTriangleBar;
				[5693] = "\u22ED", // &NotRightTriangleEqual;
				[5706] = "\u228F\u0338", // &NotSquareSubset;
				[5712] = "\u22E2", // &NotSquareSubsetEqual;
				[5719] = "\u2290\u0338", // &NotSquareSuperset;
				[5725] = "\u22E3", // &NotSquareSupersetEqual;
				[5731] = "\u2282\u20D2", // &NotSubset;
				[5737] = "\u2288", // &NotSubsetEqual;
				[5744] = "\u2281", // &NotSucceeds;
				[5750] = "\u2AB0\u0338", // &NotSucceedsEqual;
				[5761] = "\u22E1", // &NotSucceedsSlantEqual;
				[5767] = "\u227F\u0338", // &NotSucceedsTilde;
				[5774] = "\u2283\u20D2", // &NotSuperset;
				[5780] = "\u2289", // &NotSupersetEqual;
				[5786] = "\u2241", // &NotTilde;
				[5792] = "\u2244", // &NotTildeEqual;
				[5802] = "\u2247", // &NotTildeFullEqual;
				[5808] = "\u2249", // &NotTildeTilde;
				[5820] = "\u2224", // &NotVerticalBar;
				[5824] = "\u2226", // &npar;
				[5830] = "\u2226", // &nparallel;
				[5833] = "\u2AFD\u20E5", // &nparsl;
				[5835] = "\u2202\u0338", // &npart;
				[5841] = "\u2A14", // &npolint;
				[5843] = "\u2280", // &npr;
				[5847] = "\u22E0", // &nprcue;
				[5849] = "\u2AAF\u0338", // &npre;
				[5851] = "\u2280", // &nprec;
				[5854] = "\u2AAF\u0338", // &npreceq;
				[5859] = "\u21CF", // &nrArr;
				[5863] = "\u219B", // &nrarr;
				[5865] = "\u2933\u0338", // &nrarrc;
				[5867] = "\u219D\u0338", // &nrarrw;
				[5878] = "\u21CF", // &nRightarrow;
				[5888] = "\u219B", // &nrightarrow;
				[5892] = "\u22EB", // &nrtri;
				[5894] = "\u22ED", // &nrtrie;
				[5897] = "\u2281", // &nsc;
				[5901] = "\u22E1", // &nsccue;
				[5903] = "\u2AB0\u0338", // &nsce;
				[5907] = "\uD835\uDCA9", // &Nscr;
				[5909] = "\uD835\uDCC3", // &nscr;
				[5917] = "\u2224", // &nshortmid;
				[5926] = "\u2226", // &nshortparallel;
				[5929] = "\u2241", // &nsim;
				[5931] = "\u2244", // &nsime;
				[5933] = "\u2244", // &nsimeq;
				[5937] = "\u2224", // &nsmid;
				[5941] = "\u2226", // &nspar;
				[5947] = "\u22E2", // &nsqsube;
				[5950] = "\u22E3", // &nsqsupe;
				[5953] = "\u2284", // &nsub;
				[5955] = "\u2AC5\u0338", // &nsubE;
				[5957] = "\u2288", // &nsube;
				[5961] = "\u2282\u20D2", // &nsubset;
				[5964] = "\u2288", // &nsubseteq;
				[5966] = "\u2AC5\u0338", // &nsubseteqq;
				[5969] = "\u2281", // &nsucc;
				[5972] = "\u2AB0\u0338", // &nsucceq;
				[5974] = "\u2285", // &nsup;
				[5976] = "\u2AC6\u0338", // &nsupE;
				[5978] = "\u2289", // &nsupe;
				[5982] = "\u2283\u20D2", // &nsupset;
				[5985] = "\u2289", // &nsupseteq;
				[5987] = "\u2AC6\u0338", // &nsupseteqq;
				[5991] = "\u2279", // &ntgl;
				[5996] = "\u00D1", // &Ntilde
				[5997] = "\u00D1", // &Ntilde;
				[6001] = "\u00F1", // &ntilde
				[6002] = "\u00F1", // &ntilde;
				[6005] = "\u2278", // &ntlg;
				[6017] = "\u22EA", // &ntriangleleft;
				[6020] = "\u22EC", // &ntrianglelefteq;
				[6026] = "\u22EB", // &ntriangleright;
				[6029] = "\u22ED", // &ntrianglerighteq;
				[6031] = "\u039D", // &Nu;
				[6033] = "\u03BD", // &nu;
				[6035] = "\u0023", // &num;
				[6039] = "\u2116", // &numero;
				[6042] = "\u2007", // &numsp;
				[6046] = "\u224D\u20D2", // &nvap;
				[6052] = "\u22AF", // &nVDash;
				[6057] = "\u22AE", // &nVdash;
				[6062] = "\u22AD", // &nvDash;
				[6067] = "\u22AC", // &nvdash;
				[6070] = "\u2265\u20D2", // &nvge;
				[6072] = "\u003E\u20D2", // &nvgt;
				[6077] = "\u2904", // &nvHarr;
				[6083] = "\u29DE", // &nvinfin;
				[6088] = "\u2902", // &nvlArr;
				[6090] = "\u2264\u20D2", // &nvle;
				[6092] = "\u003C\u20D2", // &nvlt;
				[6096] = "\u22B4\u20D2", // &nvltrie;
				[6101] = "\u2903", // &nvrArr;
				[6106] = "\u22B5\u20D2", // &nvrtrie;
				[6110] = "\u223C\u20D2", // &nvsim;
				[6116] = "\u2923", // &nwarhk;
				[6120] = "\u21D6", // &nwArr;
				[6122] = "\u2196", // &nwarr;
				[6125] = "\u2196", // &nwarrow;
				[6130] = "\u2927", // &nwnear;
				[6136] = "\u00D3", // &Oacute
				[6137] = "\u00D3", // &Oacute;
				[6143] = "\u00F3", // &oacute
				[6144] = "\u00F3", // &oacute;
				[6147] = "\u229B", // &oast;
				[6151] = "\u229A", // &ocir;
				[6155] = "\u00D4", // &Ocirc
				[6156] = "\u00D4", // &Ocirc;
				[6157] = "\u00F4", // &ocirc
				[6158] = "\u00F4", // &ocirc;
				[6160] = "\u041E", // &Ocy;
				[6162] = "\u043E", // &ocy;
				[6167] = "\u229D", // &odash;
				[6173] = "\u0150", // &Odblac;
				[6178] = "\u0151", // &odblac;
				[6181] = "\u2A38", // &odiv;
				[6184] = "\u2299", // &odot;
				[6189] = "\u29BC", // &odsold;
				[6194] = "\u0152", // &OElig;
				[6199] = "\u0153", // &oelig;
				[6204] = "\u29BF", // &ofcir;
				[6207] = "\uD835\uDD12", // &Ofr;
				[6209] = "\uD835\uDD2C", // &ofr;
				[6213] = "\u02DB", // &ogon;
				[6218] = "\u00D2", // &Ograve
				[6219] = "\u00D2", // &Ograve;
				[6223] = "\u00F2", // &ograve
				[6224] = "\u00F2", // &ograve;
				[6226] = "\u29C1", // &ogt;
				[6231] = "\u29B5", // &ohbar;
				[6233] = "\u03A9", // &ohm;
				[6237] = "\u222E", // &oint;
				[6242] = "\u21BA", // &olarr;
				[6246] = "\u29BE", // &olcir;
				[6251] = "\u29BB", // &olcross;
				[6255] = "\u203E", // &oline;
				[6257] = "\u29C0", // &olt;
				[6262] = "\u014C", // &Omacr;
				[6267] = "\u014D", // &omacr;
				[6271] = "\u03A9", // &Omega;
				[6275] = "\u03C9", // &omega;
				[6281] = "\u039F", // &Omicron;
				[6287] = "\u03BF", // &omicron;
				[6289] = "\u29B6", // &omid;
				[6293] = "\u2296", // &ominus;
				[6297] = "\uD835\uDD46", // &Oopf;
				[6301] = "\uD835\uDD60", // &oopf;
				[6305] = "\u29B7", // &opar;
				[6325] = "\u201C", // &OpenCurlyDoubleQuote;
				[6331] = "\u2018", // &OpenCurlyQuote;
				[6335] = "\u29B9", // &operp;
				[6339] = "\u2295", // &oplus;
				[6341] = "\u2A54", // &Or;
				[6343] = "\u2228", // &or;
				[6347] = "\u21BB", // &orarr;
				[6349] = "\u2A5D", // &ord;
				[6352] = "\u2134", // &order;
				[6355] = "\u2134", // &orderof;
				[6356] = "\u00AA", // &ordf
				[6357] = "\u00AA", // &ordf;
				[6358] = "\u00BA", // &ordm
				[6359] = "\u00BA", // &ordm;
				[6364] = "\u22B6", // &origof;
				[6367] = "\u2A56", // &oror;
				[6373] = "\u2A57", // &orslope;
				[6375] = "\u2A5B", // &orv;
				[6377] = "\u24C8", // &oS;
				[6381] = "\uD835\uDCAA", // &Oscr;
				[6385] = "\u2134", // &oscr;
				[6389] = "\u00D8", // &Oslash
				[6390] = "\u00D8", // &Oslash;
				[6394] = "\u00F8", // &oslash
				[6395] = "\u00F8", // &oslash;
				[6398] = "\u2298", // &osol;
				[6403] = "\u00D5", // &Otilde
				[6404] = "\u00D5", // &Otilde;
				[6409] = "\u00F5", // &otilde
				[6410] = "\u00F5", // &otilde;
				[6414] = "\u2A37", // &Otimes;
				[6418] = "\u2297", // &otimes;
				[6421] = "\u2A36", // &otimesas;
				[6424] = "\u00D6", // &Ouml
				[6425] = "\u00D6", // &Ouml;
				[6428] = "\u00F6", // &ouml
				[6429] = "\u00F6", // &ouml;
				[6434] = "\u233D", // &ovbar;
				[6441] = "\u203E", // &OverBar;
				[6446] = "\u23DE", // &OverBrace;
				[6450] = "\u23B4", // &OverBracket;
				[6462] = "\u23DC", // &OverParenthesis;
				[6466] = "\u2225", // &par;
				[6467] = "\u00B6", // &para
				[6468] = "\u00B6", // &para;
				[6473] = "\u2225", // &parallel;
				[6477] = "\u2AF3", // &parsim;
				[6479] = "\u2AFD", // &parsl;
				[6481] = "\u2202", // &part;
				[6490] = "\u2202", // &PartialD;
				[6493] = "\u041F", // &Pcy;
				[6496] = "\u043F", // &pcy;
				[6502] = "\u0025", // &percnt;
				[6506] = "\u002E", // &period;
				[6510] = "\u2030", // &permil;
				[6512] = "\u22A5", // &perp;
				[6517] = "\u2031", // &pertenk;
				[6520] = "\uD835\uDD13", // &Pfr;
				[6523] = "\uD835\uDD2D", // &pfr;
				[6526] = "\u03A6", // &Phi;
				[6529] = "\u03C6", // &phi;
				[6531] = "\u03D5", // &phiv;
				[6536] = "\u2133", // &phmmat;
				[6540] = "\u260E", // &phone;
				[6542] = "\u03A0", // &Pi;
				[6544] = "\u03C0", // &pi;
				[6552] = "\u22D4", // &pitchfork;
				[6554] = "\u03D6", // &piv;
				[6560] = "\u210F", // &planck;
				[6562] = "\u210E", // &planckh;
				[6565] = "\u210F", // &plankv;
				[6568] = "\u002B", // &plus;
				[6573] = "\u2A23", // &plusacir;
				[6575] = "\u229E", // &plusb;
				[6579] = "\u2A22", // &pluscir;
				[6582] = "\u2214", // &plusdo;
				[6584] = "\u2A25", // &plusdu;
				[6586] = "\u2A72", // &pluse;
				[6595] = "\u00B1", // &PlusMinus;
				[6597] = "\u00B1", // &plusmn
				[6598] = "\u00B1", // &plusmn;
				[6602] = "\u2A26", // &plussim;
				[6606] = "\u2A27", // &plustwo;
				[6608] = "\u00B1", // &pm;
				[6621] = "\u210C", // &Poincareplane;
				[6629] = "\u2A15", // &pointint;
				[6632] = "\u2119", // &Popf;
				[6635] = "\uD835\uDD61", // &popf;
				[6638] = "\u00A3", // &pound
				[6639] = "\u00A3", // &pound;
				[6641] = "\u2ABB", // &Pr;
				[6643] = "\u227A", // &pr;
				[6646] = "\u2AB7", // &prap;
				[6650] = "\u227C", // &prcue;
				[6652] = "\u2AB3", // &prE;
				[6654] = "\u2AAF", // &pre;
				[6656] = "\u227A", // &prec;
				[6663] = "\u2AB7", // &precapprox;
				[6671] = "\u227C", // &preccurlyeq;
				[6678] = "\u227A", // &Precedes;
				[6684] = "\u2AAF", // &PrecedesEqual;
				[6695] = "\u227C", // &PrecedesSlantEqual;
				[6701] = "\u227E", // &PrecedesTilde;
				[6704] = "\u2AAF", // &preceq;
				[6712] = "\u2AB9", // &precnapprox;
				[6716] = "\u2AB5", // &precneqq;
				[6720] = "\u22E8", // &precnsim;
				[6724] = "\u227E", // &precsim;
				[6728] = "\u2033", // &Prime;
				[6732] = "\u2032", // &prime;
				[6734] = "\u2119", // &primes;
				[6738] = "\u2AB9", // &prnap;
				[6740] = "\u2AB5", // &prnE;
				[6744] = "\u22E8", // &prnsim;
				[6747] = "\u220F", // &prod;
				[6753] = "\u220F", // &Product;
				[6759] = "\u232E", // &profalar;
				[6764] = "\u2312", // &profline;
				[6769] = "\u2313", // &profsurf;
				[6771] = "\u221D", // &prop;
				[6779] = "\u2237", // &Proportion;
				[6782] = "\u221D", // &Proportional;
				[6785] = "\u221D", // &propto;
				[6789] = "\u227E", // &prsim;
				[6794] = "\u22B0", // &prurel;
				[6798] = "\uD835\uDCAB", // &Pscr;
				[6802] = "\uD835\uDCC5", // &pscr;
				[6804] = "\u03A8", // &Psi;
				[6806] = "\u03C8", // &psi;
				[6812] = "\u2008", // &puncsp;
				[6816] = "\uD835\uDD14", // &Qfr;
				[6820] = "\uD835\uDD2E", // &qfr;
				[6824] = "\u2A0C", // &qint;
				[6828] = "\u211A", // &Qopf;
				[6832] = "\uD835\uDD62", // &qopf;
				[6838] = "\u2057", // &qprime;
				[6842] = "\uD835\uDCAC", // &Qscr;
				[6846] = "\uD835\uDCC6", // &qscr;
				[6857] = "\u210D", // &quaternions;
				[6861] = "\u2A16", // &quatint;
				[6865] = "\u003F", // &quest;
				[6868] = "\u225F", // &questeq;
				[6871] = "\u0022", // &QUOT
				[6872] = "\u0022", // &QUOT;
				[6874] = "\u0022", // &quot
				[6875] = "\u0022", // &quot;
				[6881] = "\u21DB", // &rAarr;
				[6885] = "\u223D\u0331", // &race;
				[6892] = "\u0154", // &Racute;
				[6896] = "\u0155", // &racute;
				[6900] = "\u221A", // &radic;
				[6907] = "\u29B3", // &raemptyv;
				[6910] = "\u27EB", // &Rang;
				[6913] = "\u27E9", // &rang;
				[6915] = "\u2992", // &rangd;
				[6917] = "\u29A5", // &range;
				[6920] = "\u27E9", // &rangle;
				[6923] = "\u00BB", // &raquo
				[6924] = "\u00BB", // &raquo;
				[6927] = "\u21A0", // &Rarr;
				[6930] = "\u21D2", // &rArr;
				[6933] = "\u2192", // &rarr;
				[6936] = "\u2975", // &rarrap;
				[6938] = "\u21E5", // &rarrb;
				[6941] = "\u2920", // &rarrbfs;
				[6943] = "\u2933", // &rarrc;
				[6946] = "\u291E", // &rarrfs;
				[6949] = "\u21AA", // &rarrhk;
				[6952] = "\u21AC", // &rarrlp;
				[6955] = "\u2945", // &rarrpl;
				[6959] = "\u2974", // &rarrsim;
				[6962] = "\u2916", // &Rarrtl;
				[6965] = "\u21A3", // &rarrtl;
				[6967] = "\u219D", // &rarrw;
				[6972] = "\u291C", // &rAtail;
				[6977] = "\u291A", // &ratail;
				[6980] = "\u2236", // &ratio;
				[6985] = "\u211A", // &rationals;
				[6990] = "\u2910", // &RBarr;
				[6995] = "\u290F", // &rBarr;
				[7000] = "\u290D", // &rbarr;
				[7004] = "\u2773", // &rbbrk;
				[7009] = "\u007D", // &rbrace;
				[7011] = "\u005D", // &rbrack;
				[7014] = "\u298C", // &rbrke;
				[7018] = "\u298E", // &rbrksld;
				[7020] = "\u2990", // &rbrkslu;
				[7026] = "\u0158", // &Rcaron;
				[7032] = "\u0159", // &rcaron;
				[7037] = "\u0156", // &Rcedil;
				[7042] = "\u0157", // &rcedil;
				[7045] = "\u2309", // &rceil;
				[7048] = "\u007D", // &rcub;
				[7050] = "\u0420", // &Rcy;
				[7052] = "\u0440", // &rcy;
				[7056] = "\u2937", // &rdca;
				[7062] = "\u2969", // &rdldhar;
				[7066] = "\u201D", // &rdquo;
				[7068] = "\u201D", // &rdquor;
				[7071] = "\u21B3", // &rdsh;
				[7073] = "\u211C", // &Re;
				[7077] = "\u211C", // &real;
				[7081] = "\u211B", // &realine;
				[7086] = "\u211C", // &realpart;
				[7088] = "\u211D", // &reals;
				[7091] = "\u25AD", // &rect;
				[7093] = "\u00AE", // &REG
				[7094] = "\u00AE", // &REG;
				[7095] = "\u00AE", // &reg
				[7096] = "\u00AE", // &reg;
				[7109] = "\u220B", // &ReverseElement;
				[7120] = "\u21CB", // &ReverseEquilibrium;
				[7134] = "\u296F", // &ReverseUpEquilibrium;
				[7140] = "\u297D", // &rfisht;
				[7145] = "\u230B", // &rfloor;
				[7148] = "\u211C", // &Rfr;
				[7150] = "\uD835\uDD2F", // &rfr;
				[7154] = "\u2964", // &rHar;
				[7159] = "\u21C1", // &rhard;
				[7161] = "\u21C0", // &rharu;
				[7163] = "\u296C", // &rharul;
				[7166] = "\u03A1", // &Rho;
				[7168] = "\u03C1", // &rho;
				[7170] = "\u03F1", // &rhov;
				[7187] = "\u27E9", // &RightAngleBracket;
				[7192] = "\u2192", // &RightArrow;
				[7198] = "\u21D2", // &Rightarrow;
				[7208] = "\u2192", // &rightarrow;
				[7212] = "\u21E5", // &RightArrowBar;
				[7222] = "\u21C4", // &RightArrowLeftArrow;
				[7227] = "\u21A3", // &rightarrowtail;
				[7235] = "\u2309", // &RightCeiling;
				[7249] = "\u27E7", // &RightDoubleBracket;
				[7261] = "\u295D", // &RightDownTeeVector;
				[7268] = "\u21C2", // &RightDownVector;
				[7272] = "\u2955", // &RightDownVectorBar;
				[7278] = "\u230B", // &RightFloor;
				[7290] = "\u21C1", // &rightharpoondown;
				[7293] = "\u21C0", // &rightharpoonup;
				[7304] = "\u21C4", // &rightleftarrows;
				[7313] = "\u21CC", // &rightleftharpoons;
				[7325] = "\u21C9", // &rightrightarrows;
				[7336] = "\u219D", // &rightsquigarrow;
				[7340] = "\u22A2", // &RightTee;
				[7346] = "\u21A6", // &RightTeeArrow;
				[7353] = "\u295B", // &RightTeeVector;
				[7364] = "\u22CC", // &rightthreetimes;
				[7372] = "\u22B3", // &RightTriangle;
				[7376] = "\u29D0", // &RightTriangleBar;
				[7382] = "\u22B5", // &RightTriangleEqual;
				[7395] = "\u294F", // &RightUpDownVector;
				[7405] = "\u295C", // &RightUpTeeVector;
				[7412] = "\u21BE", // &RightUpVector;
				[7416] = "\u2954", // &RightUpVectorBar;
				[7423] = "\u21C0", // &RightVector;
				[7427] = "\u2953", // &RightVectorBar;
				[7430] = "\u02DA", // &ring;
				[7441] = "\u2253", // &risingdotseq;
				[7446] = "\u21C4", // &rlarr;
				[7450] = "\u21CC", // &rlhar;
				[7452] = "\u200F", // &rlm;
				[7458] = "\u23B1", // &rmoust;
				[7463] = "\u23B1", // &rmoustache;
				[7468] = "\u2AEE", // &rnmid;
				[7473] = "\u27ED", // &roang;
				[7476] = "\u21FE", // &roarr;
				[7480] = "\u27E7", // &robrk;
				[7484] = "\u2986", // &ropar;
				[7488] = "\u211D", // &Ropf;
				[7490] = "\uD835\uDD63", // &ropf;
				[7494] = "\u2A2E", // &roplus;
				[7500] = "\u2A35", // &rotimes;
				[7511] = "\u2970", // &RoundImplies;
				[7515] = "\u0029", // &rpar;
				[7518] = "\u2994", // &rpargt;
				[7525] = "\u2A12", // &rppolint;
				[7530] = "\u21C9", // &rrarr;
				[7541] = "\u21DB", // &Rrightarrow;
				[7547] = "\u203A", // &rsaquo;
				[7551] = "\u211B", // &Rscr;
				[7554] = "\uD835\uDCC7", // &rscr;
				[7556] = "\u21B1", // &Rsh;
				[7558] = "\u21B1", // &rsh;
				[7561] = "\u005D", // &rsqb;
				[7564] = "\u2019", // &rsquo;
				[7566] = "\u2019", // &rsquor;
				[7572] = "\u22CC", // &rthree;
				[7577] = "\u22CA", // &rtimes;
				[7580] = "\u25B9", // &rtri;
				[7582] = "\u22B5", // &rtrie;
				[7584] = "\u25B8", // &rtrif;
				[7589] = "\u29CE", // &rtriltri;
				[7600] = "\u29F4", // &RuleDelayed;
				[7607] = "\u2968", // &ruluhar;
				[7609] = "\u211E", // &rx;
				[7616] = "\u015A", // &Sacute;
				[7623] = "\u015B", // &sacute;
				[7628] = "\u201A", // &sbquo;
				[7630] = "\u2ABC", // &Sc;
				[7632] = "\u227B", // &sc;
				[7635] = "\u2AB8", // &scap;
				[7640] = "\u0160", // &Scaron;
				[7644] = "\u0161", // &scaron;
				[7648] = "\u227D", // &sccue;
				[7650] = "\u2AB4", // &scE;
				[7652] = "\u2AB0", // &sce;
				[7657] = "\u015E", // &Scedil;
				[7661] = "\u015F", // &scedil;
				[7665] = "\u015C", // &Scirc;
				[7669] = "\u015D", // &scirc;
				[7673] = "\u2ABA", // &scnap;
				[7675] = "\u2AB6", // &scnE;
				[7679] = "\u22E9", // &scnsim;
				[7686] = "\u2A13", // &scpolint;
				[7690] = "\u227F", // &scsim;
				[7692] = "\u0421", // &Scy;
				[7694] = "\u0441", // &scy;
				[7698] = "\u22C5", // &sdot;
				[7700] = "\u22A1", // &sdotb;
				[7702] = "\u2A66", // &sdote;
				[7708] = "\u2925", // &searhk;
				[7712] = "\u21D8", // &seArr;
				[7714] = "\u2198", // &searr;
				[7717] = "\u2198", // &searrow;
				[7719] = "\u00A7", // &sect
				[7720] = "\u00A7", // &sect;
				[7723] = "\u003B", // &semi;
				[7728] = "\u2929", // &seswar;
				[7735] = "\u2216", // &setminus;
				[7737] = "\u2216", // &setmn;
				[7740] = "\u2736", // &sext;
				[7743] = "\uD835\uDD16", // &Sfr;
				[7746] = "\uD835\uDD30", // &sfr;
				[7750] = "\u2322", // &sfrown;
				[7755] = "\u266F", // &sharp;
				[7761] = "\u0429", // &SHCHcy;
				[7766] = "\u0449", // &shchcy;
				[7769] = "\u0428", // &SHcy;
				[7771] = "\u0448", // &shcy;
				[7785] = "\u2193", // &ShortDownArrow;
				[7795] = "\u2190", // &ShortLeftArrow;
				[7802] = "\u2223", // &shortmid;
				[7811] = "\u2225", // &shortparallel;
				[7822] = "\u2192", // &ShortRightArrow;
				[7830] = "\u2191", // &ShortUpArrow;
				[7831] = "\u00AD", // &shy
				[7832] = "\u00AD", // &shy;
				[7837] = "\u03A3", // &Sigma;
				[7842] = "\u03C3", // &sigma;
				[7844] = "\u03C2", // &sigmaf;
				[7846] = "\u03C2", // &sigmav;
				[7848] = "\u223C", // &sim;
				[7852] = "\u2A6A", // &simdot;
				[7854] = "\u2243", // &sime;
				[7856] = "\u2243", // &simeq;
				[7858] = "\u2A9E", // &simg;
				[7860] = "\u2AA0", // &simgE;
				[7862] = "\u2A9D", // &siml;
				[7864] = "\u2A9F", // &simlE;
				[7867] = "\u2246", // &simne;
				[7872] = "\u2A24", // &simplus;
				[7877] = "\u2972", // &simrarr;
				[7882] = "\u2190", // &slarr;
				[7893] = "\u2218", // &SmallCircle;
				[7906] = "\u2216", // &smallsetminus;
				[7910] = "\u2A33", // &smashp;
				[7917] = "\u29E4", // &smeparsl;
				[7920] = "\u2223", // &smid;
				[7923] = "\u2323", // &smile;
				[7925] = "\u2AAA", // &smt;
				[7927] = "\u2AAC", // &smte;
				[7929] = "\u2AAC\uFE00", // &smtes;
				[7935] = "\u042C", // &SOFTcy;
				[7941] = "\u044C", // &softcy;
				[7943] = "\u002F", // &sol;
				[7945] = "\u29C4", // &solb;
				[7948] = "\u233F", // &solbar;
				[7952] = "\uD835\uDD4A", // &Sopf;
				[7955] = "\uD835\uDD64", // &sopf;
				[7961] = "\u2660", // &spades;
				[7965] = "\u2660", // &spadesuit;
				[7967] = "\u2225", // &spar;
				[7972] = "\u2293", // &sqcap;
				[7974] = "\u2293\uFE00", // &sqcaps;
				[7977] = "\u2294", // &sqcup;
				[7979] = "\u2294\uFE00", // &sqcups;
				[7983] = "\u221A", // &Sqrt;
				[7987] = "\u228F", // &sqsub;
				[7989] = "\u2291", // &sqsube;
				[7993] = "\u228F", // &sqsubset;
				[7996] = "\u2291", // &sqsubseteq;
				[7998] = "\u2290", // &sqsup;
				[8000] = "\u2292", // &sqsupe;
				[8004] = "\u2290", // &sqsupset;
				[8007] = "\u2292", // &sqsupseteq;
				[8009] = "\u25A1", // &squ;
				[8014] = "\u25A1", // &Square;
				[8018] = "\u25A1", // &square;
				[8031] = "\u2293", // &SquareIntersection;
				[8038] = "\u228F", // &SquareSubset;
				[8044] = "\u2291", // &SquareSubsetEqual;
				[8051] = "\u2290", // &SquareSuperset;
				[8057] = "\u2292", // &SquareSupersetEqual;
				[8063] = "\u2294", // &SquareUnion;
				[8065] = "\u25AA", // &squarf;
				[8067] = "\u25AA", // &squf;
				[8072] = "\u2192", // &srarr;
				[8076] = "\uD835\uDCAE", // &Sscr;
				[8080] = "\uD835\uDCC8", // &sscr;
				[8085] = "\u2216", // &ssetmn;
				[8090] = "\u2323", // &ssmile;
				[8095] = "\u22C6", // &sstarf;
				[8099] = "\u22C6", // &Star;
				[8103] = "\u2606", // &star;
				[8105] = "\u2605", // &starf;
				[8119] = "\u03F5", // &straightepsilon;
				[8123] = "\u03D5", // &straightphi;
				[8126] = "\u00AF", // &strns;
				[8129] = "\u22D0", // &Sub;
				[8132] = "\u2282", // &sub;
				[8136] = "\u2ABD", // &subdot;
				[8138] = "\u2AC5", // &subE;
				[8140] = "\u2286", // &sube;
				[8144] = "\u2AC3", // &subedot;
				[8149] = "\u2AC1", // &submult;
				[8152] = "\u2ACB", // &subnE;
				[8154] = "\u228A", // &subne;
				[8159] = "\u2ABF", // &subplus;
				[8164] = "\u2979", // &subrarr;
				[8168] = "\u22D0", // &Subset;
				[8172] = "\u2282", // &subset;
				[8175] = "\u2286", // &subseteq;
				[8177] = "\u2AC5", // &subseteqq;
				[8183] = "\u2286", // &SubsetEqual;
				[8187] = "\u228A", // &subsetneq;
				[8189] = "\u2ACB", // &subsetneqq;
				[8192] = "\u2AC7", // &subsim;
				[8195] = "\u2AD5", // &subsub;
				[8197] = "\u2AD3", // &subsup;
				[8200] = "\u227B", // &succ;
				[8207] = "\u2AB8", // &succapprox;
				[8215] = "\u227D", // &succcurlyeq;
				[8222] = "\u227B", // &Succeeds;
				[8228] = "\u2AB0", // &SucceedsEqual;
				[8239] = "\u227D", // &SucceedsSlantEqual;
				[8245] = "\u227F", // &SucceedsTilde;
				[8248] = "\u2AB0", // &succeq;
				[8256] = "\u2ABA", // &succnapprox;
				[8260] = "\u2AB6", // &succneqq;
				[8264] = "\u22E9", // &succnsim;
				[8268] = "\u227F", // &succsim;
				[8274] = "\u220B", // &SuchThat;
				[8276] = "\u2211", // &Sum;
				[8278] = "\u2211", // &sum;
				[8281] = "\u266A", // &sung;
				[8283] = "\u22D1", // &Sup;
				[8285] = "\u2283", // &sup;
				[8286] = "\u00B9", // &sup1
				[8287] = "\u00B9", // &sup1;
				[8288] = "\u00B2", // &sup2
				[8289] = "\u00B2", // &sup2;
				[8290] = "\u00B3", // &sup3
				[8291] = "\u00B3", // &sup3;
				[8295] = "\u2ABE", // &supdot;
				[8299] = "\u2AD8", // &supdsub;
				[8301] = "\u2AC6", // &supE;
				[8303] = "\u2287", // &supe;
				[8307] = "\u2AC4", // &supedot;
				[8313] = "\u2283", // &Superset;
				[8319] = "\u2287", // &SupersetEqual;
				[8324] = "\u27C9", // &suphsol;
				[8327] = "\u2AD7", // &suphsub;
				[8332] = "\u297B", // &suplarr;
				[8337] = "\u2AC2", // &supmult;
				[8340] = "\u2ACC", // &supnE;
				[8342] = "\u228B", // &supne;
				[8347] = "\u2AC0", // &supplus;
				[8351] = "\u22D1", // &Supset;
				[8355] = "\u2283", // &supset;
				[8358] = "\u2287", // &supseteq;
				[8360] = "\u2AC6", // &supseteqq;
				[8364] = "\u228B", // &supsetneq;
				[8366] = "\u2ACC", // &supsetneqq;
				[8369] = "\u2AC8", // &supsim;
				[8372] = "\u2AD4", // &supsub;
				[8374] = "\u2AD6", // &supsup;
				[8380] = "\u2926", // &swarhk;
				[8384] = "\u21D9", // &swArr;
				[8386] = "\u2199", // &swarr;
				[8389] = "\u2199", // &swarrow;
				[8394] = "\u292A", // &swnwar;
				[8398] = "\u00DF", // &szlig
				[8399] = "\u00DF", // &szlig;
				[8403] = "\u0009", // &Tab;
				[8410] = "\u2316", // &target;
				[8412] = "\u03A4", // &Tau;
				[8414] = "\u03C4", // &tau;
				[8418] = "\u23B4", // &tbrk;
				[8424] = "\u0164", // &Tcaron;
				[8430] = "\u0165", // &tcaron;
				[8435] = "\u0162", // &Tcedil;
				[8440] = "\u0163", // &tcedil;
				[8442] = "\u0422", // &Tcy;
				[8444] = "\u0442", // &tcy;
				[8448] = "\u20DB", // &tdot;
				[8454] = "\u2315", // &telrec;
				[8457] = "\uD835\uDD17", // &Tfr;
				[8460] = "\uD835\uDD31", // &tfr;
				[8466] = "\u2234", // &there4;
				[8475] = "\u2234", // &Therefore;
				[8480] = "\u2234", // &therefore;
				[8483] = "\u0398", // &Theta;
				[8486] = "\u03B8", // &theta;
				[8490] = "\u03D1", // &thetasym;
				[8492] = "\u03D1", // &thetav;
				[8502] = "\u2248", // &thickapprox;
				[8506] = "\u223C", // &thicksim;
				[8515] = "\u205F\u200A", // &ThickSpace;
				[8519] = "\u2009", // &thinsp;
				[8526] = "\u2009", // &ThinSpace;
				[8530] = "\u2248", // &thkap;
				[8534] = "\u223C", // &thksim;
				[8538] = "\u00DE", // &THORN
				[8539] = "\u00DE", // &THORN;
				[8542] = "\u00FE", // &thorn
				[8543] = "\u00FE", // &thorn;
				[8548] = "\u223C", // &Tilde;
				[8553] = "\u02DC", // &tilde;
				[8559] = "\u2243", // &TildeEqual;
				[8569] = "\u2245", // &TildeFullEqual;
				[8575] = "\u2248", // &TildeTilde;
				[8578] = "\u00D7", // &times
				[8579] = "\u00D7", // &times;
				[8581] = "\u22A0", // &timesb;
				[8584] = "\u2A31", // &timesbar;
				[8586] = "\u2A30", // &timesd;
				[8589] = "\u222D", // &tint;
				[8593] = "\u2928", // &toea;
				[8595] = "\u22A4", // &top;
				[8599] = "\u2336", // &topbot;
				[8603] = "\u2AF1", // &topcir;
				[8607] = "\uD835\uDD4B", // &Topf;
				[8609] = "\uD835\uDD65", // &topf;
				[8613] = "\u2ADA", // &topfork;
				[8616] = "\u2929", // &tosa;
				[8622] = "\u2034", // &tprime;
				[8627] = "\u2122", // &TRADE;
				[8632] = "\u2122", // &trade;
				[8639] = "\u25B5", // &triangle;
				[8644] = "\u25BF", // &triangledown;
				[8649] = "\u25C3", // &triangleleft;
				[8652] = "\u22B4", // &trianglelefteq;
				[8654] = "\u225C", // &triangleq;
				[8660] = "\u25B9", // &triangleright;
				[8663] = "\u22B5", // &trianglerighteq;
				[8667] = "\u25EC", // &tridot;
				[8669] = "\u225C", // &trie;
				[8675] = "\u2A3A", // &triminus;
				[8684] = "\u20DB", // &TripleDot;
				[8689] = "\u2A39", // &triplus;
				[8692] = "\u29CD", // &trisb;
				[8697] = "\u2A3B", // &tritime;
				[8704] = "\u23E2", // &trpezium;
				[8708] = "\uD835\uDCAF", // &Tscr;
				[8712] = "\uD835\uDCC9", // &tscr;
				[8716] = "\u0426", // &TScy;
				[8718] = "\u0446", // &tscy;
				[8722] = "\u040B", // &TSHcy;
				[8726] = "\u045B", // &tshcy;
				[8731] = "\u0166", // &Tstrok;
				[8736] = "\u0167", // &tstrok;
				[8741] = "\u226C", // &twixt;
				[8756] = "\u219E", // &twoheadleftarrow;
				[8767] = "\u21A0", // &twoheadrightarrow;
				[8773] = "\u00DA", // &Uacute
				[8774] = "\u00DA", // &Uacute;
				[8780] = "\u00FA", // &uacute
				[8781] = "\u00FA", // &uacute;
				[8784] = "\u219F", // &Uarr;
				[8788] = "\u21D1", // &uArr;
				[8791] = "\u2191", // &uarr;
				[8796] = "\u2949", // &Uarrocir;
				[8801] = "\u040E", // &Ubrcy;
				[8806] = "\u045E", // &ubrcy;
				[8810] = "\u016C", // &Ubreve;
				[8814] = "\u016D", // &ubreve;
				[8818] = "\u00DB", // &Ucirc
				[8819] = "\u00DB", // &Ucirc;
				[8823] = "\u00FB", // &ucirc
				[8824] = "\u00FB", // &ucirc;
				[8826] = "\u0423", // &Ucy;
				[8828] = "\u0443", // &ucy;
				[8833] = "\u21C5", // &udarr;
				[8839] = "\u0170", // &Udblac;
				[8844] = "\u0171", // &udblac;
				[8848] = "\u296E", // &udhar;
				[8854] = "\u297E", // &ufisht;
				[8857] = "\uD835\uDD18", // &Ufr;
				[8859] = "\uD835\uDD32", // &ufr;
				[8864] = "\u00D9", // &Ugrave
				[8865] = "\u00D9", // &Ugrave;
				[8870] = "\u00F9", // &ugrave
				[8871] = "\u00F9", // &ugrave;
				[8875] = "\u2963", // &uHar;
				[8880] = "\u21BF", // &uharl;
				[8882] = "\u21BE", // &uharr;
				[8886] = "\u2580", // &uhblk;
				[8892] = "\u231C", // &ulcorn;
				[8895] = "\u231C", // &ulcorner;
				[8899] = "\u230F", // &ulcrop;
				[8903] = "\u25F8", // &ultri;
				[8908] = "\u016A", // &Umacr;
				[8913] = "\u016B", // &umacr;
				[8914] = "\u00A8", // &uml
				[8915] = "\u00A8", // &uml;
				[8923] = "\u005F", // &UnderBar;
				[8928] = "\u23DF", // &UnderBrace;
				[8932] = "\u23B5", // &UnderBracket;
				[8944] = "\u23DD", // &UnderParenthesis;
				[8948] = "\u22C3", // &Union;
				[8953] = "\u228E", // &UnionPlus;
				[8958] = "\u0172", // &Uogon;
				[8963] = "\u0173", // &uogon;
				[8966] = "\uD835\uDD4C", // &Uopf;
				[8969] = "\uD835\uDD66", // &uopf;
				[8976] = "\u2191", // &UpArrow;
				[8982] = "\u21D1", // &Uparrow;
				[8989] = "\u2191", // &uparrow;
				[8993] = "\u2912", // &UpArrowBar;
				[9003] = "\u21C5", // &UpArrowDownArrow;
				[9013] = "\u2195", // &UpDownArrow;
				[9023] = "\u21D5", // &Updownarrow;
				[9033] = "\u2195", // &updownarrow;
				[9045] = "\u296E", // &UpEquilibrium;
				[9057] = "\u21BF", // &upharpoonleft;
				[9063] = "\u21BE", // &upharpoonright;
				[9067] = "\u228E", // &uplus;
				[9080] = "\u2196", // &UpperLeftArrow;
				[9091] = "\u2197", // &UpperRightArrow;
				[9094] = "\u03D2", // &Upsi;
				[9097] = "\u03C5", // &upsi;
				[9099] = "\u03D2", // &upsih;
				[9103] = "\u03A5", // &Upsilon;
				[9107] = "\u03C5", // &upsilon;
				[9111] = "\u22A5", // &UpTee;
				[9117] = "\u21A5", // &UpTeeArrow;
				[9126] = "\u21C8", // &upuparrows;
				[9132] = "\u231D", // &urcorn;
				[9135] = "\u231D", // &urcorner;
				[9139] = "\u230E", // &urcrop;
				[9144] = "\u016E", // &Uring;
				[9148] = "\u016F", // &uring;
				[9152] = "\u25F9", // &urtri;
				[9156] = "\uD835\uDCB0", // &Uscr;
				[9160] = "\uD835\uDCCA", // &uscr;
				[9165] = "\u22F0", // &utdot;
				[9171] = "\u0168", // &Utilde;
				[9176] = "\u0169", // &utilde;
				[9179] = "\u25B5", // &utri;
				[9181] = "\u25B4", // &utrif;
				[9186] = "\u21C8", // &uuarr;
				[9189] = "\u00DC", // &Uuml
				[9190] = "\u00DC", // &Uuml;
				[9192] = "\u00FC", // &uuml
				[9193] = "\u00FC", // &uuml;
				[9200] = "\u29A7", // &uwangle;
				[9207] = "\u299C", // &vangrt;
				[9216] = "\u03F5", // &varepsilon;
				[9222] = "\u03F0", // &varkappa;
				[9230] = "\u2205", // &varnothing;
				[9234] = "\u03D5", // &varphi;
				[9236] = "\u03D6", // &varpi;
				[9242] = "\u221D", // &varpropto;
				[9246] = "\u21D5", // &vArr;
				[9248] = "\u2195", // &varr;
				[9251] = "\u03F1", // &varrho;
				[9257] = "\u03C2", // &varsigma;
				[9266] = "\u228A\uFE00", // &varsubsetneq;
				[9268] = "\u2ACB\uFE00", // &varsubsetneqq;
				[9276] = "\u228B\uFE00", // &varsupsetneq;
				[9278] = "\u2ACC\uFE00", // &varsupsetneqq;
				[9284] = "\u03D1", // &vartheta;
				[9296] = "\u22B2", // &vartriangleleft;
				[9302] = "\u22B3", // &vartriangleright;
				[9307] = "\u2AEB", // &Vbar;
				[9311] = "\u2AE8", // &vBar;
				[9313] = "\u2AE9", // &vBarv;
				[9316] = "\u0412", // &Vcy;
				[9319] = "\u0432", // &vcy;
				[9324] = "\u22AB", // &VDash;
				[9329] = "\u22A9", // &Vdash;
				[9334] = "\u22A8", // &vDash;
				[9339] = "\u22A2", // &vdash;
				[9341] = "\u2AE6", // &Vdashl;
				[9344] = "\u22C1", // &Vee;
				[9347] = "\u2228", // &vee;
				[9351] = "\u22BB", // &veebar;
				[9354] = "\u225A", // &veeeq;
				[9359] = "\u22EE", // &vellip;
				[9364] = "\u2016", // &Verbar;
				[9369] = "\u007C", // &verbar;
				[9371] = "\u2016", // &Vert;
				[9373] = "\u007C", // &vert;
				[9381] = "\u2223", // &VerticalBar;
				[9386] = "\u007C", // &VerticalLine;
				[9396] = "\u2758", // &VerticalSeparator;
				[9402] = "\u2240", // &VerticalTilde;
				[9413] = "\u200A", // &VeryThinSpace;
				[9416] = "\uD835\uDD19", // &Vfr;
				[9419] = "\uD835\uDD33", // &vfr;
				[9424] = "\u22B2", // &vltri;
				[9429] = "\u2282\u20D2", // &vnsub;
				[9431] = "\u2283\u20D2", // &vnsup;
				[9435] = "\uD835\uDD4D", // &Vopf;
				[9439] = "\uD835\uDD67", // &vopf;
				[9444] = "\u221D", // &vprop;
				[9449] = "\u22B3", // &vrtri;
				[9453] = "\uD835\uDCB1", // &Vscr;
				[9457] = "\uD835\uDCCB", // &vscr;
				[9462] = "\u2ACB\uFE00", // &vsubnE;
				[9464] = "\u228A\uFE00", // &vsubne;
				[9468] = "\u2ACC\uFE00", // &vsupnE;
				[9470] = "\u228B\uFE00", // &vsupne;
				[9476] = "\u22AA", // &Vvdash;
				[9483] = "\u299A", // &vzigzag;
				[9489] = "\u0174", // &Wcirc;
				[9495] = "\u0175", // &wcirc;
				[9501] = "\u2A5F", // &wedbar;
				[9506] = "\u22C0", // &Wedge;
				[9509] = "\u2227", // &wedge;
				[9511] = "\u2259", // &wedgeq;
				[9516] = "\u2118", // &weierp;
				[9519] = "\uD835\uDD1A", // &Wfr;
				[9522] = "\uD835\uDD34", // &wfr;
				[9526] = "\uD835\uDD4E", // &Wopf;
				[9530] = "\uD835\uDD68", // &wopf;
				[9532] = "\u2118", // &wp;
				[9534] = "\u2240", // &wr;
				[9539] = "\u2240", // &wreath;
				[9543] = "\uD835\uDCB2", // &Wscr;
				[9547] = "\uD835\uDCCC", // &wscr;
				[9552] = "\u22C2", // &xcap;
				[9556] = "\u25EF", // &xcirc;
				[9559] = "\u22C3", // &xcup;
				[9564] = "\u25BD", // &xdtri;
				[9568] = "\uD835\uDD1B", // &Xfr;
				[9571] = "\uD835\uDD35", // &xfr;
				[9576] = "\u27FA", // &xhArr;
				[9580] = "\u27F7", // &xharr;
				[9582] = "\u039E", // &Xi;
				[9584] = "\u03BE", // &xi;
				[9589] = "\u27F8", // &xlArr;
				[9593] = "\u27F5", // &xlarr;
				[9597] = "\u27FC", // &xmap;
				[9601] = "\u22FB", // &xnis;
				[9606] = "\u2A00", // &xodot;
				[9610] = "\uD835\uDD4F", // &Xopf;
				[9613] = "\uD835\uDD69", // &xopf;
				[9617] = "\u2A01", // &xoplus;
				[9622] = "\u2A02", // &xotime;
				[9627] = "\u27F9", // &xrArr;
				[9631] = "\u27F6", // &xrarr;
				[9635] = "\uD835\uDCB3", // &Xscr;
				[9639] = "\uD835\uDCCD", // &xscr;
				[9644] = "\u2A06", // &xsqcup;
				[9650] = "\u2A04", // &xuplus;
				[9654] = "\u25B3", // &xutri;
				[9658] = "\u22C1", // &xvee;
				[9664] = "\u22C0", // &xwedge;
				[9670] = "\u00DD", // &Yacute
				[9671] = "\u00DD", // &Yacute;
				[9677] = "\u00FD", // &yacute
				[9678] = "\u00FD", // &yacute;
				[9682] = "\u042F", // &YAcy;
				[9684] = "\u044F", // &yacy;
				[9689] = "\u0176", // &Ycirc;
				[9694] = "\u0177", // &ycirc;
				[9696] = "\u042B", // &Ycy;
				[9698] = "\u044B", // &ycy;
				[9700] = "\u00A5", // &yen
				[9701] = "\u00A5", // &yen;
				[9704] = "\uD835\uDD1C", // &Yfr;
				[9707] = "\uD835\uDD36", // &yfr;
				[9711] = "\u0407", // &YIcy;
				[9715] = "\u0457", // &yicy;
				[9719] = "\uD835\uDD50", // &Yopf;
				[9723] = "\uD835\uDD6A", // &yopf;
				[9727] = "\uD835\uDCB4", // &Yscr;
				[9731] = "\uD835\uDCCE", // &yscr;
				[9735] = "\u042E", // &YUcy;
				[9739] = "\u044E", // &yucy;
				[9743] = "\u0178", // &Yuml;
				[9745] = "\u00FF", // &yuml
				[9746] = "\u00FF", // &yuml;
				[9753] = "\u0179", // &Zacute;
				[9760] = "\u017A", // &zacute;
				[9766] = "\u017D", // &Zcaron;
				[9772] = "\u017E", // &zcaron;
				[9774] = "\u0417", // &Zcy;
				[9776] = "\u0437", // &zcy;
				[9780] = "\u017B", // &Zdot;
				[9784] = "\u017C", // &zdot;
				[9790] = "\u2128", // &zeetrf;
				[9804] = "\u200B", // &ZeroWidthSpace;
				[9807] = "\u0396", // &Zeta;
				[9810] = "\u03B6", // &zeta;
				[9813] = "\u2128", // &Zfr;
				[9816] = "\uD835\uDD37", // &zfr;
				[9820] = "\u0416", // &ZHcy;
				[9824] = "\u0436", // &zhcy;
				[9831] = "\u21DD", // &zigrarr;
				[9835] = "\u2124", // &Zopf;
				[9839] = "\uD835\uDD6B", // &zopf;
				[9843] = "\uD835\uDCB5", // &Zscr;
				[9847] = "\uD835\uDCCF", // &zscr;
				[9850] = "\u200D", // &zwj;
				[9853] = "\u200C", // &zwnj;
			};
		}

		static int BinarySearchNextState (Transition[] transitions, int state)
		{
			int min = 0, max = transitions.Length;

			do {
				int i = min + ((max - min) / 2);

				if (state > transitions[i].From) {
					min = i + 1;
				} else if (state < transitions[i].From) {
					max = i;
				} else {
					return transitions[i].To;
				}
			} while (min < max);

			return -1;
		}

		bool PushNamedEntity (char c)
		{
			int next, state = states[index - 1];
			Transition[] table = null;

			switch (c) {
			case '1': table = TransitionTable_1; break;
			case '2': table = TransitionTable_2; break;
			case '3': table = TransitionTable_3; break;
			case '4': table = TransitionTable_4; break;
			case '5': table = TransitionTable_5; break;
			case '6': table = TransitionTable_6; break;
			case '7': table = TransitionTable_7; break;
			case '8': table = TransitionTable_8; break;
			case ';': table = TransitionTable_semicolon; break;
			case 'A': table = TransitionTable_A; break;
			case 'B': table = TransitionTable_B; break;
			case 'C': table = TransitionTable_C; break;
			case 'D': table = TransitionTable_D; break;
			case 'E': table = TransitionTable_E; break;
			case 'F': table = TransitionTable_F; break;
			case 'G': table = TransitionTable_G; break;
			case 'H': table = TransitionTable_H; break;
			case 'I': table = TransitionTable_I; break;
			case 'J': table = TransitionTable_J; break;
			case 'K': table = TransitionTable_K; break;
			case 'L': table = TransitionTable_L; break;
			case 'M': table = TransitionTable_M; break;
			case 'N': table = TransitionTable_N; break;
			case 'O': table = TransitionTable_O; break;
			case 'P': table = TransitionTable_P; break;
			case 'Q': table = TransitionTable_Q; break;
			case 'R': table = TransitionTable_R; break;
			case 'S': table = TransitionTable_S; break;
			case 'T': table = TransitionTable_T; break;
			case 'U': table = TransitionTable_U; break;
			case 'V': table = TransitionTable_V; break;
			case 'W': table = TransitionTable_W; break;
			case 'X': table = TransitionTable_X; break;
			case 'Y': table = TransitionTable_Y; break;
			case 'Z': table = TransitionTable_Z; break;
			case 'a': table = TransitionTable_a; break;
			case 'b': table = TransitionTable_b; break;
			case 'c': table = TransitionTable_c; break;
			case 'd': table = TransitionTable_d; break;
			case 'e': table = TransitionTable_e; break;
			case 'f': table = TransitionTable_f; break;
			case 'g': table = TransitionTable_g; break;
			case 'h': table = TransitionTable_h; break;
			case 'i': table = TransitionTable_i; break;
			case 'j': table = TransitionTable_j; break;
			case 'k': table = TransitionTable_k; break;
			case 'l': table = TransitionTable_l; break;
			case 'm': table = TransitionTable_m; break;
			case 'n': table = TransitionTable_n; break;
			case 'o': table = TransitionTable_o; break;
			case 'p': table = TransitionTable_p; break;
			case 'q': table = TransitionTable_q; break;
			case 'r': table = TransitionTable_r; break;
			case 's': table = TransitionTable_s; break;
			case 't': table = TransitionTable_t; break;
			case 'u': table = TransitionTable_u; break;
			case 'v': table = TransitionTable_v; break;
			case 'w': table = TransitionTable_w; break;
			case 'x': table = TransitionTable_x; break;
			case 'y': table = TransitionTable_y; break;
			case 'z': table = TransitionTable_z; break;
			default: return false;
			}

			if ((next = BinarySearchNextState (table, state)) == -1)
				return false;

			states[index] = next;
			pushed[index] = c;
			index++;

			return true;
		}

		string GetNamedEntityValue ()
		{
			int startIndex = index;
			string decoded = null;

			while (startIndex > 0) {
				if (NamedEntities.TryGetValue (states[startIndex - 1], out decoded))
					break;

				startIndex--;
			}

			if (decoded is null)
				decoded = string.Empty;

			if (startIndex < index)
				decoded += new string (pushed, startIndex, index - startIndex);

			return decoded;
		}
	}
}
