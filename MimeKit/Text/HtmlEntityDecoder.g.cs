//
// HtmlEntityDecoder.g.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
		const int MaxEntityLength = 32;

		bool PushNamedEntity (char c)
		{
			int state = states[index - 1];

			switch (c) {
			case '1':
				switch (state) {
				case 436: state = 437; break; // &blk -> &blk1
				case 1791: state = 1792; break; // &emsp -> &emsp1
				case 2085: state = 2086; break; // &frac -> &frac1
				case 6500: state = 6501; break; // &sup -> &sup1
				default: return false;
				}
				break;
			case '2':
				switch (state) {
				case 437: state = 438; break; // &blk1 -> &blk12
				case 2085: state = 2093; break; // &frac -> &frac2
				case 2086: state = 2087; break; // &frac1 -> &frac12
				case 6500: state = 6502; break; // &sup -> &sup2
				default: return false;
				}
				break;
			case '3':
				switch (state) {
				case 436: state = 440; break; // &blk -> &blk3
				case 1792: state = 1793; break; // &emsp1 -> &emsp13
				case 2085: state = 2096; break; // &frac -> &frac3
				case 2086: state = 2088; break; // &frac1 -> &frac13
				case 2093: state = 2094; break; // &frac2 -> &frac23
				case 6500: state = 6503; break; // &sup -> &sup3
				default: return false;
				}
				break;
			case '4':
				switch (state) {
				case 437: state = 439; break; // &blk1 -> &blk14
				case 440: state = 441; break; // &blk3 -> &blk34
				case 1792: state = 1794; break; // &emsp1 -> &emsp14
				case 2085: state = 2100; break; // &frac -> &frac4
				case 2086: state = 2089; break; // &frac1 -> &frac14
				case 2096: state = 2097; break; // &frac3 -> &frac34
				case 6632: state = 6633; break; // &there -> &there4
				default: return false;
				}
				break;
			case '5':
				switch (state) {
				case 2085: state = 2102; break; // &frac -> &frac5
				case 2086: state = 2090; break; // &frac1 -> &frac15
				case 2093: state = 2095; break; // &frac2 -> &frac25
				case 2096: state = 2098; break; // &frac3 -> &frac35
				case 2100: state = 2101; break; // &frac4 -> &frac45
				default: return false;
				}
				break;
			case '6':
				switch (state) {
				case 2086: state = 2091; break; // &frac1 -> &frac16
				case 2102: state = 2103; break; // &frac5 -> &frac56
				default: return false;
				}
				break;
			case '7':
				switch (state) {
				case 2085: state = 2105; break; // &frac -> &frac7
				default: return false;
				}
				break;
			case '8':
				switch (state) {
				case 2086: state = 2092; break; // &frac1 -> &frac18
				case 2096: state = 2099; break; // &frac3 -> &frac38
				case 2102: state = 2104; break; // &frac5 -> &frac58
				case 2105: state = 2106; break; // &frac7 -> &frac78
				default: return false;
				}
				break;
			case 'A':
				switch (state) {
				case 0: state = 1; break; // & -> &A
				case 1097: state = 1109; break; // &d -> &dA
				case 1200: state = 1201; break; // &Diacritical -> &DiacriticalA
				case 1212: state = 1213; break; // &DiacriticalDouble -> &DiacriticalDoubleA
				case 1366: state = 1367; break; // &DoubleDown -> &DoubleDownA
				case 1375: state = 1376; break; // &DoubleLeft -> &DoubleLeftA
				case 1385: state = 1386; break; // &DoubleLeftRight -> &DoubleLeftRightA
				case 1400: state = 1401; break; // &DoubleLongLeft -> &DoubleLongLeftA
				case 1410: state = 1411; break; // &DoubleLongLeftRight -> &DoubleLongLeftRightA
				case 1420: state = 1421; break; // &DoubleLongRight -> &DoubleLongRightA
				case 1430: state = 1431; break; // &DoubleRight -> &DoubleRightA
				case 1440: state = 1441; break; // &DoubleUp -> &DoubleUpA
				case 1449: state = 1450; break; // &DoubleUpDown -> &DoubleUpDownA
				case 1467: state = 1468; break; // &Down -> &DownA
				case 1489: state = 1490; break; // &DownArrowUp -> &DownArrowUpA
				case 1584: state = 1585; break; // &DownTee -> &DownTeeA
				case 2058: state = 2059; break; // &For -> &ForA
				case 2351: state = 2368; break; // &H -> &HA
				case 2356: state = 2377; break; // &h -> &hA
				case 2881: state = 2882; break; // &l -> &lA
				case 3035: state = 3036; break; // &Left -> &LeftA
				case 3071: state = 3072; break; // &LeftArrowRight -> &LeftArrowRightA
				case 3153: state = 3154; break; // &LeftRight -> &LeftRightA
				case 3206: state = 3207; break; // &LeftTee -> &LeftTeeA
				case 3481: state = 3482; break; // &LongLeft -> &LongLeftA
				case 3511: state = 3512; break; // &LongLeftRight -> &LongLeftRightA
				case 3547: state = 3548; break; // &LongRight -> &LongRightA
				case 3616: state = 3617; break; // &LowerLeft -> &LowerLeftA
				case 3626: state = 3627; break; // &LowerRight -> &LowerRightA
				case 3970: state = 3975; break; // &ne -> &neA
				case 4101: state = 4102; break; // &nh -> &nhA
				case 4121: state = 4122; break; // &nl -> &nlA
				case 4621: state = 4622; break; // &nr -> &nrA
				case 4792: state = 4793; break; // &nvl -> &nvlA
				case 4801: state = 4802; break; // &nvr -> &nvrA
				case 4812: state = 4817; break; // &nw -> &nwA
				case 5397: state = 5398; break; // &r -> &rA
				case 5620: state = 5621; break; // &Right -> &RightA
				case 5657: state = 5658; break; // &RightArrowLeft -> &RightArrowLeftA
				case 5766: state = 5767; break; // &RightTee -> &RightTeeA
				case 6053: state = 6058; break; // &se -> &seA
				case 6111: state = 6112; break; // &ShortDown -> &ShortDownA
				case 6120: state = 6121; break; // &ShortLeft -> &ShortLeftA
				case 6144: state = 6145; break; // &ShortRight -> &ShortRightA
				case 6151: state = 6152; break; // &ShortUp -> &ShortUpA
				case 6564: state = 6569; break; // &sw -> &swA
				case 6756: state = 6757; break; // &TR -> &TRA
				case 6879: state = 6887; break; // &u -> &uA
				case 7031: state = 7032; break; // &Up -> &UpA
				case 7054: state = 7055; break; // &UpArrowDown -> &UpArrowDownA
				case 7063: state = 7064; break; // &UpDown -> &UpDownA
				case 7123: state = 7124; break; // &UpperLeft -> &UpperLeftA
				case 7133: state = 7134; break; // &UpperRight -> &UpperRightA
				case 7152: state = 7153; break; // &UpTee -> &UpTeeA
				case 7223: state = 7258; break; // &v -> &vA
				case 7513: state = 7514; break; // &xh -> &xhA
				case 7522: state = 7523; break; // &xl -> &xlA
				case 7551: state = 7552; break; // &xr -> &xrA
				case 7584: state = 7596; break; // &Y -> &YA
				default: return false;
				}
				break;
			case 'B':
				switch (state) {
				case 0: state = 247; break; // & -> &B
				case 1462: state = 1463; break; // &DoubleVertical -> &DoubleVerticalB
				case 1467: state = 1495; break; // &Down -> &DownB
				case 1472: state = 1485; break; // &DownArrow -> &DownArrowB
				case 1555: state = 1556; break; // &DownLeftVector -> &DownLeftVectorB
				case 1578: state = 1579; break; // &DownRightVector -> &DownRightVectorB
				case 2881: state = 2966; break; // &l -> &lB
				case 3040: state = 3041; break; // &LeftAngle -> &LeftAngleB
				case 3051: state = 3064; break; // &LeftArrow -> &LeftArrowB
				case 3093: state = 3094; break; // &LeftDouble -> &LeftDoubleB
				case 3117: state = 3118; break; // &LeftDownVector -> &LeftDownVectorB
				case 3234: state = 3235; break; // &LeftTriangle -> &LeftTriangleB
				case 3269: state = 3270; break; // &LeftUpVector -> &LeftUpVectorB
				case 3278: state = 3279; break; // &LeftVector -> &LeftVectorB
				case 4190: state = 4191; break; // &No -> &NoB
				case 4196: state = 4197; break; // &Non -> &NonB
				case 4244: state = 4245; break; // &NotDoubleVertical -> &NotDoubleVerticalB
				case 4354: state = 4355; break; // &NotLeftTriangle -> &NotLeftTriangleB
				case 4478: state = 4479; break; // &NotRightTriangle -> &NotRightTriangleB
				case 4593: state = 4594; break; // &NotVertical -> &NotVerticalB
				case 5061: state = 5062; break; // &Over -> &OverB
				case 5397: state = 5480; break; // &r -> &rB
				case 5405: state = 5476; break; // &R -> &RB
				case 5625: state = 5626; break; // &RightAngle -> &RightAngleB
				case 5636: state = 5651; break; // &RightArrow -> &RightArrowB
				case 5679: state = 5680; break; // &RightDouble -> &RightDoubleB
				case 5703: state = 5704; break; // &RightDownVector -> &RightDownVectorB
				case 5794: state = 5795; break; // &RightTriangle -> &RightTriangleB
				case 5829: state = 5830; break; // &RightUpVector -> &RightUpVectorB
				case 5838: state = 5839; break; // &RightVector -> &RightVectorB
				case 6990: state = 6991; break; // &Under -> &UnderB
				case 7036: state = 7048; break; // &UpArrow -> &UpArrowB
				case 7223: state = 7311; break; // &v -> &vB
				case 7362: state = 7363; break; // &Vertical -> &VerticalB
				default: return false;
				}
				break;
			case 'C':
				switch (state) {
				case 0: state = 583; break; // & -> &C
				case 810: state = 811; break; // &Clockwise -> &ClockwiseC
				case 827: state = 828; break; // &Close -> &CloseC
				case 936: state = 937; break; // &Counter -> &CounterC
				case 945: state = 946; break; // &CounterClockwise -> &CounterClockwiseC
				case 1005: state = 1012; break; // &Cup -> &CupC
				case 1346: state = 1347; break; // &Double -> &DoubleC
				case 2699: state = 2700; break; // &Invisible -> &InvisibleC
				case 3035: state = 3081; break; // &Left -> &LeftC
				case 4215: state = 4217; break; // &Not -> &NotC
				case 4227: state = 4228; break; // &NotCup -> &NotCupC
				case 4962: state = 4963; break; // &Open -> &OpenC
				case 5620: state = 5667; break; // &Right -> &RightC
				case 6092: state = 6093; break; // &SH -> &SHC
				case 6195: state = 6196; break; // &Small -> &SmallC
				default: return false;
				}
				break;
			case 'D':
				switch (state) {
				case 0: state = 1091; break; // & -> &D
				case 470: state = 474; break; // &box -> &boxD
				case 484: state = 486; break; // &boxH -> &boxHD
				case 485: state = 488; break; // &boxh -> &boxhD
				case 616: state = 617; break; // &Capital -> &CapitalD
				case 628: state = 629; break; // &CapitalDifferential -> &CapitalDifferentialD
				case 703: state = 704; break; // &Center -> &CenterD
				case 769: state = 770; break; // &Circle -> &CircleD
				case 832: state = 833; break; // &CloseCurly -> &CloseCurlyD
				case 1091: state = 1141; break; // &D -> &DD
				case 1200: state = 1206; break; // &Diacritical -> &DiacriticalD
				case 1253: state = 1254; break; // &Differential -> &DifferentialD
				case 1301: state = 1303; break; // &Dot -> &DotD
				case 1346: state = 1362; break; // &Double -> &DoubleD
				case 1440: state = 1446; break; // &DoubleUp -> &DoubleUpD
				case 1662: state = 1694; break; // &e -> &eD
				case 1694: state = 1695; break; // &eD -> &eDD
				case 1707: state = 1708; break; // &ef -> &efD
				case 1881: state = 1882; break; // &equiv -> &equivD
				case 1882: state = 1883; break; // &equivD -> &equivDD
				case 1890: state = 1894; break; // &er -> &erD
				case 2369: state = 2370; break; // &HAR -> &HARD
				case 2510: state = 2511; break; // &Hump -> &HumpD
				case 3035: state = 3088; break; // &Left -> &LeftD
				case 3244: state = 3245; break; // &LeftUp -> &LeftUpD
				case 3745: state = 3788; break; // &m -> &mD
				case 3788: state = 3789; break; // &mD -> &mDD
				case 4215: state = 4231; break; // &Not -> &NotD
				case 4319: state = 4320; break; // &NotHump -> &NotHumpD
				case 4760: state = 4772; break; // &nv -> &nvD
				case 4763: state = 4764; break; // &nV -> &nVD
				case 4967: state = 4968; break; // &OpenCurly -> &OpenCurlyD
				case 5102: state = 5103; break; // &Partial -> &PartialD
				case 5620: state = 5674; break; // &Right -> &RightD
				case 5804: state = 5805; break; // &RightUp -> &RightUpD
				case 5970: state = 5971; break; // &Rule -> &RuleD
				case 6107: state = 6108; break; // &Short -> &ShortD
				case 6757: state = 6758; break; // &TRA -> &TRAD
				case 6801: state = 6802; break; // &Triple -> &TripleD
				case 7031: state = 7060; break; // &Up -> &UpD
				case 7036: state = 7051; break; // &UpArrow -> &UpArrowD
				case 7223: state = 7327; break; // &v -> &vD
				case 7307: state = 7319; break; // &V -> &VD
				default: return false;
				}
				break;
			case 'E':
				switch (state) {
				case 0: state = 1656; break; // & -> &E
				case 1: state = 38; break; // &A -> &AE
				case 23: state = 25; break; // &ac -> &acE
				case 143: state = 148; break; // &ap -> &apE
				case 574: state = 575; break; // &bump -> &bumpE
				case 733: state = 789; break; // &cir -> &cirE
				case 1301: state = 1311; break; // &Dot -> &DotE
				case 1953: state = 1954; break; // &Exponential -> &ExponentialE
				case 2118: state = 2165; break; // &g -> &gE
				case 2204: state = 2206; break; // &gl -> &glE
				case 2208: state = 2215; break; // &gn -> &gnE
				case 2237: state = 2238; break; // &Greater -> &GreaterE
				case 2250: state = 2251; break; // &GreaterFull -> &GreaterFullE
				case 2271: state = 2272; break; // &GreaterSlant -> &GreaterSlantE
				case 2349: state = 2350; break; // &gvn -> &gvnE
				case 2510: state = 2519; break; // &Hump -> &HumpE
				case 2533: state = 2558; break; // &I -> &IE
				case 2747: state = 2751; break; // &isin -> &isinE
				case 2881: state = 3031; break; // &l -> &lE
				case 3234: state = 3238; break; // &LeftTriangle -> &LeftTriangleE
				case 3322: state = 3323; break; // &Less -> &LessE
				case 3338: state = 3339; break; // &LessFull -> &LessFullE
				case 3365: state = 3366; break; // &LessSlant -> &LessSlantE
				case 3388: state = 3389; break; // &lg -> &lgE
				case 3452: state = 3459; break; // &ln -> &lnE
				case 3743: state = 3744; break; // &lvn -> &lvnE
				case 3914: state = 3915; break; // &nap -> &napE
				case 4081: state = 4082; break; // &ng -> &ngE
				case 4121: state = 4130; break; // &nl -> &nlE
				case 4215: state = 4248; break; // &Not -> &NotE
				case 4275: state = 4276; break; // &NotGreater -> &NotGreaterE
				case 4284: state = 4285; break; // &NotGreaterFull -> &NotGreaterFullE
				case 4305: state = 4306; break; // &NotGreaterSlant -> &NotGreaterSlantE
				case 4319: state = 4328; break; // &NotHump -> &NotHumpE
				case 4334: state = 4338; break; // &notin -> &notinE
				case 4354: state = 4358; break; // &NotLeftTriangle -> &NotLeftTriangleE
				case 4364: state = 4365; break; // &NotLess -> &NotLessE
				case 4385: state = 4386; break; // &NotLessSlant -> &NotLessSlantE
				case 4437: state = 4438; break; // &NotPrecedes -> &NotPrecedesE
				case 4447: state = 4448; break; // &NotPrecedesSlant -> &NotPrecedesSlantE
				case 4459: state = 4460; break; // &NotReverse -> &NotReverseE
				case 4478: state = 4482; break; // &NotRightTriangle -> &NotRightTriangleE
				case 4498: state = 4499; break; // &NotSquareSubset -> &NotSquareSubsetE
				case 4509: state = 4510; break; // &NotSquareSuperset -> &NotSquareSupersetE
				case 4519: state = 4520; break; // &NotSubset -> &NotSubsetE
				case 4530: state = 4531; break; // &NotSucceeds -> &NotSucceedsE
				case 4540: state = 4541; break; // &NotSucceedsSlant -> &NotSucceedsSlantE
				case 4556: state = 4557; break; // &NotSuperset -> &NotSupersetE
				case 4566: state = 4567; break; // &NotTilde -> &NotTildeE
				case 4575: state = 4576; break; // &NotTildeFull -> &NotTildeFullE
				case 4696: state = 4697; break; // &nsub -> &nsubE
				case 4709: state = 4710; break; // &nsup -> &nsupE
				case 4827: state = 4872; break; // &O -> &OE
				case 5216: state = 5222; break; // &pr -> &prE
				case 5243: state = 5244; break; // &Precedes -> &PrecedesE
				case 5253: state = 5254; break; // &PrecedesSlant -> &PrecedesSlantE
				case 5289: state = 5292; break; // &prn -> &prnE
				case 5405: state = 5554; break; // &R -> &RE
				case 5561: state = 5562; break; // &Reverse -> &ReverseE
				case 5580: state = 5581; break; // &ReverseUp -> &ReverseUpE
				case 5794: state = 5798; break; // &RightTriangle -> &RightTriangleE
				case 6002: state = 6015; break; // &sc -> &scE
				case 6030: state = 6033; break; // &scn -> &scnE
				case 6174: state = 6175; break; // &simg -> &simgE
				case 6176: state = 6177; break; // &siml -> &simlE
				case 6310: state = 6311; break; // &SquareSubset -> &SquareSubsetE
				case 6321: state = 6322; break; // &SquareSuperset -> &SquareSupersetE
				case 6384: state = 6388; break; // &sub -> &subE
				case 6397: state = 6398; break; // &subn -> &subnE
				case 6410: state = 6417; break; // &Subset -> &SubsetE
				case 6451: state = 6452; break; // &Succeeds -> &SucceedsE
				case 6461: state = 6462; break; // &SucceedsSlant -> &SucceedsSlantE
				case 6500: state = 6510; break; // &sup -> &supE
				case 6519: state = 6520; break; // &Superset -> &SupersetE
				case 6539: state = 6540; break; // &supn -> &supnE
				case 6699: state = 6704; break; // &Tilde -> &TildeE
				case 6712: state = 6713; break; // &TildeFull -> &TildeFullE
				case 6758: state = 6759; break; // &TRAD -> &TRADE
				case 7031: state = 7087; break; // &Up -> &UpE
				case 7429: state = 7430; break; // &vsubn -> &vsubnE
				case 7433: state = 7434; break; // &vsupn -> &vsupnE
				default: return false;
				}
				break;
			case 'F':
				switch (state) {
				case 0: state = 1977; break; // & -> &F
				case 157: state = 158; break; // &Apply -> &ApplyF
				case 2237: state = 2247; break; // &Greater -> &GreaterF
				case 3035: state = 3121; break; // &Left -> &LeftF
				case 3322: state = 3335; break; // &Less -> &LessF
				case 4275: state = 4281; break; // &NotGreater -> &NotGreaterF
				case 4566: state = 4572; break; // &NotTilde -> &NotTildeF
				case 5620: state = 5707; break; // &Right -> &RightF
				case 6230: state = 6231; break; // &SO -> &SOF
				case 6699: state = 6709; break; // &Tilde -> &TildeF
				default: return false;
				}
				break;
			case 'G':
				switch (state) {
				case 0: state = 2124; break; // & -> &G
				case 1200: state = 1218; break; // &Diacritical -> &DiacriticalG
				case 1795: state = 1796; break; // &EN -> &ENG
				case 2237: state = 2256; break; // &Greater -> &GreaterG
				case 3322: state = 3344; break; // &Less -> &LessG
				case 3327: state = 3328; break; // &LessEqual -> &LessEqualG
				case 3897: state = 4092; break; // &n -> &nG
				case 4044: state = 4045; break; // &Nested -> &NestedG
				case 4051: state = 4052; break; // &NestedGreater -> &NestedGreaterG
				case 4215: state = 4269; break; // &Not -> &NotG
				case 4275: state = 4290; break; // &NotGreater -> &NotGreaterG
				case 4364: state = 4370; break; // &NotLess -> &NotLessG
				case 4401: state = 4402; break; // &NotNested -> &NotNestedG
				case 4408: state = 4409; break; // &NotNestedGreater -> &NotNestedGreaterG
				case 5554: state = 5555; break; // &RE -> &REG
				default: return false;
				}
				break;
			case 'H':
				switch (state) {
				case 0: state = 2351; break; // & -> &H
				case 470: state = 484; break; // &box -> &boxH
				case 518: state = 520; break; // &boxV -> &boxVH
				case 519: state = 522; break; // &boxv -> &boxvH
				case 583: state = 716; break; // &C -> &CH
				case 1097: state = 1183; break; // &d -> &dH
				case 1914: state = 1915; break; // &ET -> &ETH
				case 2514: state = 2515; break; // &HumpDown -> &HumpDownH
				case 2825: state = 2857; break; // &K -> &KH
				case 2881: state = 3390; break; // &l -> &lH
				case 4215: state = 4316; break; // &Not -> &NotH
				case 4323: state = 4324; break; // &NotHumpDown -> &NotHumpDownH
				case 4760: state = 4783; break; // &nv -> &nvH
				case 5397: state = 5604; break; // &r -> &rH
				case 5985: state = 6092; break; // &S -> &SH
				case 6093: state = 6094; break; // &SHC -> &SHCH
				case 6583: state = 6689; break; // &T -> &TH
				case 6827: state = 6831; break; // &TS -> &TSH
				case 6879: state = 6954; break; // &u -> &uH
				case 7645: state = 7701; break; // &Z -> &ZH
				default: return false;
				}
				break;
			case 'I':
				switch (state) {
				case 0: state = 2533; break; // & -> &I
				case 817: state = 818; break; // &ClockwiseContour -> &ClockwiseContourI
				case 904: state = 905; break; // &Contour -> &ContourI
				case 952: state = 953; break; // &CounterClockwiseContour -> &CounterClockwiseContourI
				case 1353: state = 1354; break; // &DoubleContour -> &DoubleContourI
				case 2619: state = 2620; break; // &Imaginary -> &ImaginaryI
				case 5901: state = 5902; break; // &Round -> &RoundI
				case 6289: state = 6293; break; // &Square -> &SquareI
				case 7584: state = 7616; break; // &Y -> &YI
				default: return false;
				}
				break;
			case 'J':
				switch (state) {
				case 0: state = 2777; break; // & -> &J
				case 1091: state = 1277; break; // &D -> &DJ
				case 2124: state = 2198; break; // &G -> &GJ
				case 2533: state = 2596; break; // &I -> &IJ
				case 2825: state = 2863; break; // &K -> &KJ
				case 2886: state = 3402; break; // &L -> &LJ
				case 3902: state = 4115; break; // &N -> &NJ
				default: return false;
				}
				break;
			case 'K':
				switch (state) {
				case 0: state = 2825; break; // & -> &K
				default: return false;
				}
				break;
			case 'L':
				switch (state) {
				case 0: state = 2886; break; // & -> &L
				case 474: state = 475; break; // &boxD -> &boxDL
				case 477: state = 478; break; // &boxd -> &boxdL
				case 508: state = 509; break; // &boxU -> &boxUL
				case 511: state = 512; break; // &boxu -> &boxuL
				case 518: state = 524; break; // &boxV -> &boxVL
				case 519: state = 526; break; // &boxv -> &boxvL
				case 1346: state = 1372; break; // &Double -> &DoubleL
				case 1396: state = 1397; break; // &DoubleLong -> &DoubleLongL
				case 1467: state = 1526; break; // &Down -> &DownL
				case 2237: state = 2263; break; // &Greater -> &GreaterL
				case 2242: state = 2243; break; // &GreaterEqual -> &GreaterEqualL
				case 2485: state = 2486; break; // &Horizontal -> &HorizontalL
				case 3322: state = 3354; break; // &Less -> &LessL
				case 3477: state = 3478; break; // &Long -> &LongL
				case 3612: state = 3613; break; // &Lower -> &LowerL
				case 3897: state = 4132; break; // &n -> &nL
				case 4044: state = 4059; break; // &Nested -> &NestedL
				case 4062: state = 4063; break; // &NestedLess -> &NestedLessL
				case 4067: state = 4068; break; // &New -> &NewL
				case 4215: state = 4343; break; // &Not -> &NotL
				case 4275: state = 4297; break; // &NotGreater -> &NotGreaterL
				case 4364: state = 4377; break; // &NotLess -> &NotLessL
				case 4401: state = 4416; break; // &NotNested -> &NotNestedL
				case 4419: state = 4420; break; // &NotNestedLess -> &NotNestedLessL
				case 5636: state = 5654; break; // &RightArrow -> &RightArrowL
				case 6107: state = 6117; break; // &Short -> &ShortL
				case 7119: state = 7120; break; // &Upper -> &UpperL
				case 7362: state = 7366; break; // &Vertical -> &VerticalL
				default: return false;
				}
				break;
			case 'M':
				switch (state) {
				case 0: state = 3755; break; // & -> &M
				case 1: state = 85; break; // &A -> &AM
				case 769: state = 775; break; // &Circle -> &CircleM
				case 3990: state = 3991; break; // &Negative -> &NegativeM
				case 5174: state = 5175; break; // &Plus -> &PlusM
				default: return false;
				}
				break;
			case 'N':
				switch (state) {
				case 0: state = 3902; break; // & -> &N
				case 222: state = 451; break; // &b -> &bN
				case 1656: state = 1795; break; // &E -> &EN
				case 4215: state = 4396; break; // &Not -> &NotN
				case 6691: state = 6692; break; // &THOR -> &THORN
				default: return false;
				}
				break;
			case 'O':
				switch (state) {
				case 0: state = 4827; break; // & -> &O
				case 583: state = 926; break; // &C -> &CO
				case 2533: state = 2710; break; // &I -> &IO
				case 5392: state = 5393; break; // &QU -> &QUO
				case 5985: state = 6230; break; // &S -> &SO
				case 6689: state = 6690; break; // &TH -> &THO
				default: return false;
				}
				break;
			case 'P':
				switch (state) {
				case 0: state = 5096; break; // & -> &P
				case 85: state = 86; break; // &AM -> &AMP
				case 769: state = 780; break; // &Circle -> &CircleP
				case 926: state = 927; break; // &CO -> &COP
				case 2302: state = 2303; break; // &gtl -> &gtlP
				case 3717: state = 3721; break; // &ltr -> &ltrP
				case 3850: state = 3851; break; // &Minus -> &MinusP
				case 4215: state = 4430; break; // &Not -> &NotP
				case 5061: state = 5072; break; // &Over -> &OverP
				case 6990: state = 7001; break; // &Under -> &UnderP
				case 7014: state = 7015; break; // &Union -> &UnionP
				default: return false;
				}
				break;
			case 'Q':
				switch (state) {
				case 0: state = 5348; break; // & -> &Q
				case 832: state = 844; break; // &CloseCurly -> &CloseCurlyQ
				case 838: state = 839; break; // &CloseCurlyDouble -> &CloseCurlyDoubleQ
				case 4967: state = 4979; break; // &OpenCurly -> &OpenCurlyQ
				case 4973: state = 4974; break; // &OpenCurlyDouble -> &OpenCurlyDoubleQ
				default: return false;
				}
				break;
			case 'R':
				switch (state) {
				case 0: state = 5405; break; // & -> &R
				case 474: state = 480; break; // &boxD -> &boxDR
				case 477: state = 482; break; // &boxd -> &boxdR
				case 508: state = 514; break; // &boxU -> &boxUR
				case 511: state = 516; break; // &boxu -> &boxuR
				case 518: state = 528; break; // &boxV -> &boxVR
				case 519: state = 530; break; // &boxv -> &boxvR
				case 753: state = 773; break; // &circled -> &circledR
				case 1346: state = 1426; break; // &Double -> &DoubleR
				case 1375: state = 1381; break; // &DoubleLeft -> &DoubleLeftR
				case 1396: state = 1416; break; // &DoubleLong -> &DoubleLongR
				case 1400: state = 1406; break; // &DoubleLongLeft -> &DoubleLongLeftR
				case 1467: state = 1559; break; // &Down -> &DownR
				case 1529: state = 1530; break; // &DownLeft -> &DownLeftR
				case 2368: state = 2369; break; // &HA -> &HAR
				case 3035: state = 3149; break; // &Left -> &LeftR
				case 3051: state = 3067; break; // &LeftArrow -> &LeftArrowR
				case 3477: state = 3543; break; // &Long -> &LongR
				case 3481: state = 3507; break; // &LongLeft -> &LongLeftR
				case 3612: state = 3622; break; // &Lower -> &LowerR
				case 3897: state = 4630; break; // &n -> &nR
				case 4215: state = 4453; break; // &Not -> &NotR
				case 6107: state = 6140; break; // &Short -> &ShortR
				case 6583: state = 6756; break; // &T -> &TR
				case 6690: state = 6691; break; // &THO -> &THOR
				case 7119: state = 7129; break; // &Upper -> &UpperR
				default: return false;
				}
				break;
			case 'S':
				switch (state) {
				case 0: state = 5985; break; // & -> &S
				case 753: state = 774; break; // &circled -> &circledS
				case 1091: state = 1610; break; // &D -> &DS
				case 1762: state = 1763; break; // &Empty -> &EmptyS
				case 1767: state = 1768; break; // &EmptySmall -> &EmptySmallS
				case 1778: state = 1779; break; // &EmptyVery -> &EmptyVeryS
				case 1783: state = 1784; break; // &EmptyVerySmall -> &EmptyVerySmallS
				case 2009: state = 2010; break; // &Filled -> &FilledS
				case 2014: state = 2015; break; // &FilledSmall -> &FilledSmallS
				case 2024: state = 2025; break; // &FilledVery -> &FilledVeryS
				case 2029: state = 2030; break; // &FilledVerySmall -> &FilledVerySmallS
				case 2237: state = 2267; break; // &Greater -> &GreaterS
				case 2422: state = 2423; break; // &Hilbert -> &HilbertS
				case 3322: state = 3361; break; // &Less -> &LessS
				case 3808: state = 3809; break; // &Medium -> &MediumS
				case 3996: state = 3997; break; // &NegativeMedium -> &NegativeMediumS
				case 4006: state = 4007; break; // &NegativeThick -> &NegativeThickS
				case 4012: state = 4013; break; // &NegativeThin -> &NegativeThinS
				case 4025: state = 4026; break; // &NegativeVeryThin -> &NegativeVeryThinS
				case 4204: state = 4205; break; // &NonBreaking -> &NonBreakingS
				case 4215: state = 4487; break; // &Not -> &NotS
				case 4275: state = 4301; break; // &NotGreater -> &NotGreaterS
				case 4364: state = 4381; break; // &NotLess -> &NotLessS
				case 4437: state = 4443; break; // &NotPrecedes -> &NotPrecedesS
				case 4492: state = 4493; break; // &NotSquare -> &NotSquareS
				case 4530: state = 4536; break; // &NotSucceeds -> &NotSucceedsS
				case 4833: state = 5014; break; // &o -> &oS
				case 5243: state = 5249; break; // &Precedes -> &PrecedesS
				case 6289: state = 6305; break; // &Square -> &SquareS
				case 6451: state = 6457; break; // &Succeeds -> &SucceedsS
				case 6583: state = 6827; break; // &T -> &TS
				case 6668: state = 6669; break; // &Thick -> &ThickS
				case 6677: state = 6678; break; // &Thin -> &ThinS
				case 7362: state = 7370; break; // &Vertical -> &VerticalS
				case 7388: state = 7389; break; // &VeryThin -> &VeryThinS
				case 7687: state = 7688; break; // &ZeroWidth -> &ZeroWidthS
				default: return false;
				}
				break;
			case 'T':
				switch (state) {
				case 0: state = 6583; break; // & -> &T
				case 769: state = 784; break; // &Circle -> &CircleT
				case 1200: state = 1223; break; // &Diacritical -> &DiacriticalT
				case 1375: state = 1391; break; // &DoubleLeft -> &DoubleLeftT
				case 1430: state = 1436; break; // &DoubleRight -> &DoubleRightT
				case 1467: state = 1582; break; // &Down -> &DownT
				case 1529: state = 1541; break; // &DownLeft -> &DownLeftT
				case 1563: state = 1564; break; // &DownRight -> &DownRightT
				case 1656: state = 1914; break; // &E -> &ET
				case 1859: state = 1864; break; // &Equal -> &EqualT
				case 2124: state = 2292; break; // &G -> &GT
				case 2237: state = 2277; break; // &Greater -> &GreaterT
				case 2699: state = 2705; break; // &Invisible -> &InvisibleT
				case 2886: state = 3690; break; // &L -> &LT
				case 3035: state = 3204; break; // &Left -> &LeftT
				case 3102: state = 3103; break; // &LeftDown -> &LeftDownT
				case 3244: state = 3255; break; // &LeftUp -> &LeftUpT
				case 3322: state = 3371; break; // &Less -> &LessT
				case 3990: state = 4002; break; // &Negative -> &NegativeT
				case 4021: state = 4022; break; // &NegativeVery -> &NegativeVeryT
				case 4215: state = 4562; break; // &Not -> &NotT
				case 4258: state = 4259; break; // &NotEqual -> &NotEqualT
				case 4275: state = 4311; break; // &NotGreater -> &NotGreaterT
				case 4346: state = 4347; break; // &NotLeft -> &NotLeftT
				case 4364: state = 4391; break; // &NotLess -> &NotLessT
				case 4470: state = 4471; break; // &NotRight -> &NotRightT
				case 4530: state = 4546; break; // &NotSucceeds -> &NotSucceedsT
				case 4566: state = 4581; break; // &NotTilde -> &NotTildeT
				case 5243: state = 5259; break; // &Precedes -> &PrecedesT
				case 5393: state = 5394; break; // &QUO -> &QUOT
				case 5620: state = 5764; break; // &Right -> &RightT
				case 5688: state = 5689; break; // &RightDown -> &RightDownT
				case 5804: state = 5815; break; // &RightUp -> &RightUpT
				case 6231: state = 6232; break; // &SOF -> &SOFT
				case 6451: state = 6467; break; // &Succeeds -> &SucceedsT
				case 6490: state = 6491; break; // &Such -> &SuchT
				case 6699: state = 6718; break; // &Tilde -> &TildeT
				case 7031: state = 7150; break; // &Up -> &UpT
				case 7362: state = 7379; break; // &Vertical -> &VerticalT
				case 7384: state = 7385; break; // &Very -> &VeryT
				default: return false;
				}
				break;
			case 'U':
				switch (state) {
				case 0: state = 6873; break; // & -> &U
				case 470: state = 508; break; // &box -> &boxU
				case 484: state = 490; break; // &boxH -> &boxHU
				case 485: state = 492; break; // &boxh -> &boxhU
				case 1346: state = 1439; break; // &Double -> &DoubleU
				case 1472: state = 1488; break; // &DownArrow -> &DownArrowU
				case 3035: state = 3243; break; // &Left -> &LeftU
				case 5348: state = 5392; break; // &Q -> &QU
				case 5561: state = 5579; break; // &Reverse -> &ReverseU
				case 5620: state = 5803; break; // &Right -> &RightU
				case 6107: state = 6150; break; // &Short -> &ShortU
				case 6289: state = 6327; break; // &Square -> &SquareU
				case 7584: state = 7634; break; // &Y -> &YU
				default: return false;
				}
				break;
			case 'V':
				switch (state) {
				case 0: state = 7307; break; // & -> &V
				case 470: state = 518; break; // &box -> &boxV
				case 1346: state = 1455; break; // &Double -> &DoubleV
				case 1529: state = 1550; break; // &DownLeft -> &DownLeftV
				case 1534: state = 1535; break; // &DownLeftRight -> &DownLeftRightV
				case 1543: state = 1544; break; // &DownLeftTee -> &DownLeftTeeV
				case 1563: state = 1573; break; // &DownRight -> &DownRightV
				case 1566: state = 1567; break; // &DownRightTee -> &DownRightTeeV
				case 1762: state = 1775; break; // &Empty -> &EmptyV
				case 2009: state = 2021; break; // &Filled -> &FilledV
				case 3035: state = 3273; break; // &Left -> &LeftV
				case 3102: state = 3112; break; // &LeftDown -> &LeftDownV
				case 3105: state = 3106; break; // &LeftDownTee -> &LeftDownTeeV
				case 3153: state = 3198; break; // &LeftRight -> &LeftRightV
				case 3206: state = 3212; break; // &LeftTee -> &LeftTeeV
				case 3244: state = 3264; break; // &LeftUp -> &LeftUpV
				case 3248: state = 3249; break; // &LeftUpDown -> &LeftUpDownV
				case 3257: state = 3258; break; // &LeftUpTee -> &LeftUpTeeV
				case 3897: state = 4763; break; // &n -> &nV
				case 3990: state = 4018; break; // &Negative -> &NegativeV
				case 4215: state = 4586; break; // &Not -> &NotV
				case 4236: state = 4237; break; // &NotDouble -> &NotDoubleV
				case 5620: state = 5833; break; // &Right -> &RightV
				case 5688: state = 5698; break; // &RightDown -> &RightDownV
				case 5691: state = 5692; break; // &RightDownTee -> &RightDownTeeV
				case 5766: state = 5772; break; // &RightTee -> &RightTeeV
				case 5804: state = 5824; break; // &RightUp -> &RightUpV
				case 5808: state = 5809; break; // &RightUpDown -> &RightUpDownV
				case 5817: state = 5818; break; // &RightUpTee -> &RightUpTeeV
				default: return false;
				}
				break;
			case 'W':
				switch (state) {
				case 0: state = 7447; break; // & -> &W
				case 7682: state = 7683; break; // &Zero -> &ZeroW
				default: return false;
				}
				break;
			case 'X':
				switch (state) {
				case 0: state = 7508; break; // & -> &X
				default: return false;
				}
				break;
			case 'Y':
				switch (state) {
				case 0: state = 7584; break; // & -> &Y
				case 927: state = 928; break; // &COP -> &COPY
				default: return false;
				}
				break;
			case 'Z':
				switch (state) {
				case 0: state = 7645; break; // & -> &Z
				case 1091: state = 1644; break; // &D -> &DZ
				default: return false;
				}
				break;
			case 'a':
				switch (state) {
				case 0: state = 7; break; // & -> &a
				case 1: state = 2; break; // &A -> &Aa
				case 7: state = 8; break; // &a -> &aa
				case 51: state = 52; break; // &Agr -> &Agra
				case 56: state = 57; break; // &agr -> &agra
				case 70: state = 71; break; // &Alph -> &Alpha
				case 73: state = 74; break; // &alph -> &alpha
				case 75: state = 76; break; // &Am -> &Ama
				case 79: state = 80; break; // &am -> &ama
				case 91: state = 92; break; // &and -> &anda
				case 108: state = 109; break; // &angmsd -> &angmsda
				case 109: state = 110; break; // &angmsda -> &angmsdaa
				case 127: state = 128; break; // &angz -> &angza
				case 143: state = 144; break; // &ap -> &apa
				case 222: state = 223; break; // &b -> &ba
				case 247: state = 248; break; // &B -> &Ba
				case 252: state = 253; break; // &Backsl -> &Backsla
				case 289: state = 290; break; // &bec -> &beca
				case 294: state = 295; break; // &Bec -> &Beca
				case 320: state = 321; break; // &Bet -> &Beta
				case 322: state = 323; break; // &bet -> &beta
				case 335: state = 336; break; // &bigc -> &bigca
				case 361: state = 362; break; // &bigst -> &bigsta
				case 366: state = 367; break; // &bigtri -> &bigtria
				case 391: state = 392; break; // &bk -> &bka
				case 396: state = 397; break; // &bl -> &bla
				case 409: state = 410; break; // &blacksqu -> &blacksqua
				case 415: state = 416; break; // &blacktri -> &blacktria
				case 546: state = 547; break; // &brvb -> &brvba
				case 583: state = 584; break; // &C -> &Ca
				case 589: state = 590; break; // &c -> &ca
				case 596: state = 597; break; // &cap -> &capa
				case 605: state = 606; break; // &capc -> &capca
				case 614: state = 615; break; // &Capit -> &Capita
				case 626: state = 627; break; // &CapitalDifferenti -> &CapitalDifferentia
				case 641: state = 642; break; // &cc -> &cca
				case 645: state = 646; break; // &Cc -> &Cca
				case 691: state = 692; break; // &Cedill -> &Cedilla
				case 725: state = 726; break; // &checkm -> &checkma
				case 738: state = 739; break; // &circle -> &circlea
				case 753: state = 754; break; // &circled -> &circleda
				case 761: state = 762; break; // &circledd -> &circledda
				case 823: state = 824; break; // &ClockwiseContourIntegr -> &ClockwiseContourIntegra
				case 868: state = 869; break; // &comm -> &comma
				case 910: state = 911; break; // &ContourIntegr -> &ContourIntegra
				case 958: state = 959; break; // &CounterClockwiseContourIntegr -> &CounterClockwiseContourIntegra
				case 961: state = 962; break; // &cr -> &cra
				case 988: state = 989; break; // &cud -> &cuda
				case 999: state = 1000; break; // &cul -> &cula
				case 1009: state = 1010; break; // &cupbrc -> &cupbrca
				case 1012: state = 1013; break; // &CupC -> &CupCa
				case 1015: state = 1016; break; // &cupc -> &cupca
				case 1026: state = 1027; break; // &cur -> &cura
				case 1055: state = 1056; break; // &curve -> &curvea
				case 1091: state = 1092; break; // &D -> &Da
				case 1097: state = 1098; break; // &d -> &da
				case 1121: state = 1122; break; // &dbk -> &dbka
				case 1126: state = 1127; break; // &dbl -> &dbla
				case 1129: state = 1130; break; // &Dc -> &Dca
				case 1134: state = 1135; break; // &dc -> &dca
				case 1142: state = 1143; break; // &dd -> &dda
				case 1152: state = 1153; break; // &DDotr -> &DDotra
				case 1165: state = 1166; break; // &Delt -> &Delta
				case 1168: state = 1169; break; // &delt -> &delta
				case 1183: state = 1184; break; // &dH -> &dHa
				case 1186: state = 1187; break; // &dh -> &dha
				case 1191: state = 1192; break; // &Di -> &Dia
				case 1198: state = 1199; break; // &Diacritic -> &Diacritica
				case 1219: state = 1220; break; // &DiacriticalGr -> &DiacriticalGra
				case 1228: state = 1229; break; // &di -> &dia
				case 1251: state = 1252; break; // &Differenti -> &Differentia
				case 1255: state = 1256; break; // &dig -> &diga
				case 1258: state = 1259; break; // &digamm -> &digamma
				case 1293: state = 1294; break; // &doll -> &dolla
				case 1313: state = 1314; break; // &DotEqu -> &DotEqua
				case 1327: state = 1328; break; // &dotsqu -> &dotsqua
				case 1335: state = 1336; break; // &doubleb -> &doubleba
				case 1359: state = 1360; break; // &DoubleContourIntegr -> &DoubleContourIntegra
				case 1460: state = 1461; break; // &DoubleVertic -> &DoubleVertica
				case 1463: state = 1464; break; // &DoubleVerticalB -> &DoubleVerticalBa
				case 1467: state = 1473; break; // &Down -> &Downa
				case 1479: state = 1480; break; // &down -> &downa
				case 1485: state = 1486; break; // &DownArrowB -> &DownArrowBa
				case 1503: state = 1504; break; // &downdown -> &downdowna
				case 1510: state = 1511; break; // &downh -> &downha
				case 1556: state = 1557; break; // &DownLeftVectorB -> &DownLeftVectorBa
				case 1579: state = 1580; break; // &DownRightVectorB -> &DownRightVectorBa
				case 1592: state = 1593; break; // &drbk -> &drbka
				case 1631: state = 1632; break; // &du -> &dua
				case 1635: state = 1636; break; // &duh -> &duha
				case 1638: state = 1639; break; // &dw -> &dwa
				case 1652: state = 1653; break; // &dzigr -> &dzigra
				case 1656: state = 1657; break; // &E -> &Ea
				case 1662: state = 1663; break; // &e -> &ea
				case 1672: state = 1673; break; // &Ec -> &Eca
				case 1677: state = 1678; break; // &ec -> &eca
				case 1716: state = 1717; break; // &Egr -> &Egra
				case 1720: state = 1721; break; // &egr -> &egra
				case 1746: state = 1747; break; // &Em -> &Ema
				case 1750: state = 1751; break; // &em -> &ema
				case 1764: state = 1765; break; // &EmptySm -> &EmptySma
				case 1770: state = 1771; break; // &EmptySmallSqu -> &EmptySmallSqua
				case 1780: state = 1781; break; // &EmptyVerySm -> &EmptyVerySma
				case 1786: state = 1787; break; // &EmptyVerySmallSqu -> &EmptyVerySmallSqua
				case 1813: state = 1814; break; // &ep -> &epa
				case 1845: state = 1846; break; // &eqsl -> &eqsla
				case 1857: state = 1858; break; // &Equ -> &Equa
				case 1860: state = 1861; break; // &equ -> &equa
				case 1885: state = 1886; break; // &eqvp -> &eqvpa
				case 1890: state = 1891; break; // &er -> &era
				case 1910: state = 1911; break; // &Et -> &Eta
				case 1912: state = 1913; break; // &et -> &eta
				case 1939: state = 1940; break; // &expect -> &expecta
				case 1951: state = 1952; break; // &Exponenti -> &Exponentia
				case 1960: state = 1961; break; // &exponenti -> &exponentia
				case 1964: state = 1965; break; // &f -> &fa
				case 1983: state = 1984; break; // &fem -> &fema
				case 2011: state = 2012; break; // &FilledSm -> &FilledSma
				case 2017: state = 2018; break; // &FilledSmallSqu -> &FilledSmallSqua
				case 2026: state = 2027; break; // &FilledVerySm -> &FilledVerySma
				case 2032: state = 2033; break; // &FilledVerySmallSqu -> &FilledVerySmallSqua
				case 2040: state = 2041; break; // &fl -> &fla
				case 2062: state = 2063; break; // &for -> &fora
				case 2076: state = 2077; break; // &fp -> &fpa
				case 2083: state = 2084; break; // &fr -> &fra
				case 2118: state = 2119; break; // &g -> &ga
				case 2124: state = 2125; break; // &G -> &Ga
				case 2127: state = 2128; break; // &Gamm -> &Gamma
				case 2130: state = 2131; break; // &gamm -> &gamma
				case 2172: state = 2173; break; // &geqsl -> &geqsla
				case 2204: state = 2205; break; // &gl -> &gla
				case 2208: state = 2209; break; // &gn -> &gna
				case 2228: state = 2229; break; // &gr -> &gra
				case 2233: state = 2234; break; // &Gre -> &Grea
				case 2240: state = 2241; break; // &GreaterEqu -> &GreaterEqua
				case 2253: state = 2254; break; // &GreaterFullEqu -> &GreaterFullEqua
				case 2258: state = 2259; break; // &GreaterGre -> &GreaterGrea
				case 2268: state = 2269; break; // &GreaterSl -> &GreaterSla
				case 2274: state = 2275; break; // &GreaterSlantEqu -> &GreaterSlantEqua
				case 2303: state = 2304; break; // &gtlP -> &gtlPa
				case 2311: state = 2312; break; // &gtr -> &gtra
				case 2351: state = 2352; break; // &H -> &Ha
				case 2356: state = 2357; break; // &h -> &ha
				case 2386: state = 2387; break; // &hb -> &hba
				case 2397: state = 2398; break; // &he -> &hea
				case 2424: state = 2425; break; // &HilbertSp -> &HilbertSpa
				case 2430: state = 2431; break; // &hkse -> &hksea
				case 2435: state = 2436; break; // &hksw -> &hkswa
				case 2440: state = 2441; break; // &ho -> &hoa
				case 2453: state = 2454; break; // &hookleft -> &hooklefta
				case 2463: state = 2464; break; // &hookright -> &hookrighta
				case 2475: state = 2476; break; // &horb -> &horba
				case 2483: state = 2484; break; // &Horizont -> &Horizonta
				case 2496: state = 2497; break; // &hsl -> &hsla
				case 2521: state = 2522; break; // &HumpEqu -> &HumpEqua
				case 2533: state = 2534; break; // &I -> &Ia
				case 2539: state = 2540; break; // &i -> &ia
				case 2573: state = 2574; break; // &Igr -> &Igra
				case 2578: state = 2579; break; // &igr -> &igra
				case 2594: state = 2595; break; // &iiot -> &iiota
				case 2604: state = 2605; break; // &Im -> &Ima
				case 2608: state = 2609; break; // &im -> &ima
				case 2616: state = 2617; break; // &Imagin -> &Imagina
				case 2625: state = 2626; break; // &imagp -> &imagpa
				case 2642: state = 2643; break; // &inc -> &inca
				case 2659: state = 2660; break; // &intc -> &intca
				case 2669: state = 2670; break; // &Integr -> &Integra
				case 2673: state = 2674; break; // &interc -> &interca
				case 2684: state = 2685; break; // &intl -> &intla
				case 2703: state = 2704; break; // &InvisibleComm -> &InvisibleComma
				case 2727: state = 2728; break; // &Iot -> &Iota
				case 2729: state = 2730; break; // &iot -> &iota
				case 2793: state = 2794; break; // &jm -> &jma
				case 2825: state = 2826; break; // &K -> &Ka
				case 2828: state = 2829; break; // &Kapp -> &Kappa
				case 2830: state = 2831; break; // &k -> &ka
				case 2833: state = 2834; break; // &kapp -> &kappa
				case 2881: state = 2892; break; // &l -> &la
				case 2882: state = 2883; break; // &lA -> &lAa
				case 2886: state = 2887; break; // &L -> &La
				case 2904: state = 2905; break; // &lagr -> &lagra
				case 2909: state = 2910; break; // &Lambd -> &Lambda
				case 2913: state = 2914; break; // &lambd -> &lambda
				case 2924: state = 2925; break; // &Lapl -> &Lapla
				case 2956: state = 2961; break; // &lat -> &lata
				case 2957: state = 2958; break; // &lAt -> &lAta
				case 2966: state = 2967; break; // &lB -> &lBa
				case 2970: state = 2971; break; // &lb -> &lba
				case 2977: state = 2978; break; // &lbr -> &lbra
				case 2988: state = 2989; break; // &Lc -> &Lca
				case 2993: state = 2994; break; // &lc -> &lca
				case 3013: state = 3014; break; // &ldc -> &ldca
				case 3021: state = 3022; break; // &ldrdh -> &ldrdha
				case 3026: state = 3027; break; // &ldrush -> &ldrusha
				case 3035: state = 3052; break; // &Left -> &Lefta
				case 3042: state = 3043; break; // &LeftAngleBr -> &LeftAngleBra
				case 3058: state = 3059; break; // &left -> &lefta
				case 3064: state = 3065; break; // &LeftArrowB -> &LeftArrowBa
				case 3077: state = 3078; break; // &leftarrowt -> &leftarrowta
				case 3095: state = 3096; break; // &LeftDoubleBr -> &LeftDoubleBra
				case 3118: state = 3119; break; // &LeftDownVectorB -> &LeftDownVectorBa
				case 3126: state = 3127; break; // &lefth -> &leftha
				case 3142: state = 3143; break; // &leftleft -> &leftlefta
				case 3163: state = 3164; break; // &Leftright -> &Leftrighta
				case 3173: state = 3174; break; // &leftright -> &leftrighta
				case 3180: state = 3181; break; // &leftrighth -> &leftrightha
				case 3192: state = 3193; break; // &leftrightsquig -> &leftrightsquiga
				case 3229: state = 3230; break; // &LeftTri -> &LeftTria
				case 3235: state = 3236; break; // &LeftTriangleB -> &LeftTriangleBa
				case 3240: state = 3241; break; // &LeftTriangleEqu -> &LeftTriangleEqua
				case 3270: state = 3271; break; // &LeftUpVectorB -> &LeftUpVectorBa
				case 3279: state = 3280; break; // &LeftVectorB -> &LeftVectorBa
				case 3287: state = 3288; break; // &leqsl -> &leqsla
				case 3302: state = 3303; break; // &less -> &lessa
				case 3325: state = 3326; break; // &LessEqu -> &LessEqua
				case 3330: state = 3331; break; // &LessEqualGre -> &LessEqualGrea
				case 3341: state = 3342; break; // &LessFullEqu -> &LessFullEqua
				case 3346: state = 3347; break; // &LessGre -> &LessGrea
				case 3362: state = 3363; break; // &LessSl -> &LessSla
				case 3368: state = 3369; break; // &LessSlantEqu -> &LessSlantEqua
				case 3390: state = 3391; break; // &lH -> &lHa
				case 3393: state = 3394; break; // &lh -> &lha
				case 3409: state = 3410; break; // &ll -> &lla
				case 3421: state = 3422; break; // &Lleft -> &Llefta
				case 3427: state = 3428; break; // &llh -> &llha
				case 3447: state = 3448; break; // &lmoust -> &lmousta
				case 3452: state = 3453; break; // &ln -> &lna
				case 3466: state = 3467; break; // &lo -> &loa
				case 3490: state = 3491; break; // &Longleft -> &Longlefta
				case 3501: state = 3502; break; // &longleft -> &longlefta
				case 3521: state = 3522; break; // &Longleftright -> &Longleftrighta
				case 3531: state = 3532; break; // &longleftright -> &longleftrighta
				case 3537: state = 3538; break; // &longm -> &longma
				case 3557: state = 3558; break; // &Longright -> &Longrighta
				case 3567: state = 3568; break; // &longright -> &longrighta
				case 3574: state = 3575; break; // &loop -> &loopa
				case 3589: state = 3590; break; // &lop -> &lopa
				case 3603: state = 3604; break; // &low -> &lowa
				case 3607: state = 3608; break; // &lowb -> &lowba
				case 3638: state = 3639; break; // &lp -> &lpa
				case 3643: state = 3644; break; // &lr -> &lra
				case 3653: state = 3654; break; // &lrh -> &lrha
				case 3661: state = 3662; break; // &ls -> &lsa
				case 3708: state = 3709; break; // &ltl -> &ltla
				case 3721: state = 3722; break; // &ltrP -> &ltrPa
				case 3728: state = 3729; break; // &lurdsh -> &lurdsha
				case 3732: state = 3733; break; // &luruh -> &luruha
				case 3745: state = 3746; break; // &m -> &ma
				case 3755: state = 3756; break; // &M -> &Ma
				case 3779: state = 3780; break; // &mcomm -> &mcomma
				case 3784: state = 3785; break; // &md -> &mda
				case 3792: state = 3793; break; // &me -> &mea
				case 3798: state = 3799; break; // &measured -> &measureda
				case 3810: state = 3811; break; // &MediumSp -> &MediumSpa
				case 3831: state = 3832; break; // &mid -> &mida
				case 3891: state = 3892; break; // &multim -> &multima
				case 3894: state = 3895; break; // &mum -> &muma
				case 3897: state = 3898; break; // &n -> &na
				case 3900: state = 3901; break; // &nabl -> &nabla
				case 3902: state = 3903; break; // &N -> &Na
				case 3926: state = 3927; break; // &natur -> &natura
				case 3937: state = 3938; break; // &nc -> &nca
				case 3940: state = 3941; break; // &Nc -> &Nca
				case 3966: state = 3967; break; // &nd -> &nda
				case 3970: state = 3971; break; // &ne -> &nea
				case 3985: state = 3986; break; // &Neg -> &Nega
				case 3998: state = 3999; break; // &NegativeMediumSp -> &NegativeMediumSpa
				case 4008: state = 4009; break; // &NegativeThickSp -> &NegativeThickSpa
				case 4014: state = 4015; break; // &NegativeThinSp -> &NegativeThinSpa
				case 4027: state = 4028; break; // &NegativeVeryThinSp -> &NegativeVeryThinSpa
				case 4036: state = 4037; break; // &nese -> &nesea
				case 4047: state = 4048; break; // &NestedGre -> &NestedGrea
				case 4054: state = 4055; break; // &NestedGreaterGre -> &NestedGreaterGrea
				case 4087: state = 4088; break; // &ngeqsl -> &ngeqsla
				case 4101: state = 4105; break; // &nh -> &nha
				case 4108: state = 4109; break; // &nhp -> &nhpa
				case 4121: state = 4125; break; // &nl -> &nla
				case 4135: state = 4136; break; // &nLeft -> &nLefta
				case 4142: state = 4143; break; // &nleft -> &nlefta
				case 4152: state = 4153; break; // &nLeftright -> &nLeftrighta
				case 4162: state = 4163; break; // &nleftright -> &nleftrighta
				case 4171: state = 4172; break; // &nleqsl -> &nleqsla
				case 4193: state = 4194; break; // &NoBre -> &NoBrea
				case 4199: state = 4200; break; // &NonBre -> &NonBrea
				case 4206: state = 4207; break; // &NonBreakingSp -> &NonBreakingSpa
				case 4228: state = 4229; break; // &NotCupC -> &NotCupCa
				case 4242: state = 4243; break; // &NotDoubleVertic -> &NotDoubleVertica
				case 4245: state = 4246; break; // &NotDoubleVerticalB -> &NotDoubleVerticalBa
				case 4256: state = 4257; break; // &NotEqu -> &NotEqua
				case 4271: state = 4272; break; // &NotGre -> &NotGrea
				case 4278: state = 4279; break; // &NotGreaterEqu -> &NotGreaterEqua
				case 4287: state = 4288; break; // &NotGreaterFullEqu -> &NotGreaterFullEqua
				case 4292: state = 4293; break; // &NotGreaterGre -> &NotGreaterGrea
				case 4302: state = 4303; break; // &NotGreaterSl -> &NotGreaterSla
				case 4308: state = 4309; break; // &NotGreaterSlantEqu -> &NotGreaterSlantEqua
				case 4330: state = 4331; break; // &NotHumpEqu -> &NotHumpEqua
				case 4339: state = 4340; break; // &notinv -> &notinva
				case 4349: state = 4350; break; // &NotLeftTri -> &NotLeftTria
				case 4355: state = 4356; break; // &NotLeftTriangleB -> &NotLeftTriangleBa
				case 4360: state = 4361; break; // &NotLeftTriangleEqu -> &NotLeftTriangleEqua
				case 4367: state = 4368; break; // &NotLessEqu -> &NotLessEqua
				case 4372: state = 4373; break; // &NotLessGre -> &NotLessGrea
				case 4382: state = 4383; break; // &NotLessSl -> &NotLessSla
				case 4388: state = 4389; break; // &NotLessSlantEqu -> &NotLessSlantEqua
				case 4404: state = 4405; break; // &NotNestedGre -> &NotNestedGrea
				case 4411: state = 4412; break; // &NotNestedGreaterGre -> &NotNestedGreaterGrea
				case 4426: state = 4427; break; // &notniv -> &notniva
				case 4440: state = 4441; break; // &NotPrecedesEqu -> &NotPrecedesEqua
				case 4444: state = 4445; break; // &NotPrecedesSl -> &NotPrecedesSla
				case 4450: state = 4451; break; // &NotPrecedesSlantEqu -> &NotPrecedesSlantEqua
				case 4473: state = 4474; break; // &NotRightTri -> &NotRightTria
				case 4479: state = 4480; break; // &NotRightTriangleB -> &NotRightTriangleBa
				case 4484: state = 4485; break; // &NotRightTriangleEqu -> &NotRightTriangleEqua
				case 4489: state = 4490; break; // &NotSqu -> &NotSqua
				case 4501: state = 4502; break; // &NotSquareSubsetEqu -> &NotSquareSubsetEqua
				case 4512: state = 4513; break; // &NotSquareSupersetEqu -> &NotSquareSupersetEqua
				case 4522: state = 4523; break; // &NotSubsetEqu -> &NotSubsetEqua
				case 4533: state = 4534; break; // &NotSucceedsEqu -> &NotSucceedsEqua
				case 4537: state = 4538; break; // &NotSucceedsSl -> &NotSucceedsSla
				case 4543: state = 4544; break; // &NotSucceedsSlantEqu -> &NotSucceedsSlantEqua
				case 4559: state = 4560; break; // &NotSupersetEqu -> &NotSupersetEqua
				case 4569: state = 4570; break; // &NotTildeEqu -> &NotTildeEqua
				case 4578: state = 4579; break; // &NotTildeFullEqu -> &NotTildeFullEqua
				case 4591: state = 4592; break; // &NotVertic -> &NotVertica
				case 4594: state = 4595; break; // &NotVerticalB -> &NotVerticalBa
				case 4597: state = 4598; break; // &np -> &npa
				case 4599: state = 4600; break; // &npar -> &npara
				case 4621: state = 4625; break; // &nr -> &nra
				case 4634: state = 4635; break; // &nRight -> &nRighta
				case 4643: state = 4644; break; // &nright -> &nrighta
				case 4670: state = 4671; break; // &nshortp -> &nshortpa
				case 4672: state = 4673; break; // &nshortpar -> &nshortpara
				case 4685: state = 4686; break; // &nsp -> &nspa
				case 4733: state = 4734; break; // &ntri -> &ntria
				case 4760: state = 4761; break; // &nv -> &nva
				case 4764: state = 4765; break; // &nVD -> &nVDa
				case 4768: state = 4769; break; // &nVd -> &nVda
				case 4772: state = 4773; break; // &nvD -> &nvDa
				case 4776: state = 4777; break; // &nvd -> &nvda
				case 4783: state = 4784; break; // &nvH -> &nvHa
				case 4812: state = 4813; break; // &nw -> &nwa
				case 4824: state = 4825; break; // &nwne -> &nwnea
				case 4827: state = 4828; break; // &O -> &Oa
				case 4833: state = 4834; break; // &o -> &oa
				case 4851: state = 4852; break; // &od -> &oda
				case 4857: state = 4858; break; // &Odbl -> &Odbla
				case 4861: state = 4862; break; // &odbl -> &odbla
				case 4891: state = 4892; break; // &Ogr -> &Ogra
				case 4895: state = 4896; break; // &ogr -> &ogra
				case 4901: state = 4902; break; // &ohb -> &ohba
				case 4908: state = 4909; break; // &ol -> &ola
				case 4923: state = 4924; break; // &Om -> &Oma
				case 4927: state = 4928; break; // &om -> &oma
				case 4932: state = 4933; break; // &Omeg -> &Omega
				case 4935: state = 4936; break; // &omeg -> &omega
				case 4957: state = 4958; break; // &op -> &opa
				case 4991: state = 4992; break; // &or -> &ora
				case 5021: state = 5022; break; // &Osl -> &Osla
				case 5025: state = 5026; break; // &osl -> &osla
				case 5046: state = 5047; break; // &otimes -> &otimesa
				case 5056: state = 5057; break; // &ovb -> &ovba
				case 5062: state = 5063; break; // &OverB -> &OverBa
				case 5065: state = 5066; break; // &OverBr -> &OverBra
				case 5072: state = 5073; break; // &OverP -> &OverPa
				case 5083: state = 5084; break; // &p -> &pa
				case 5085: state = 5086; break; // &par -> &para
				case 5096: state = 5097; break; // &P -> &Pa
				case 5100: state = 5101; break; // &Parti -> &Partia
				case 5134: state = 5135; break; // &phmm -> &phmma
				case 5150: state = 5151; break; // &pl -> &pla
				case 5159: state = 5160; break; // &plus -> &plusa
				case 5192: state = 5193; break; // &Poinc -> &Poinca
				case 5197: state = 5198; break; // &Poincarepl -> &Poincarepla
				case 5216: state = 5217; break; // &pr -> &pra
				case 5224: state = 5225; break; // &prec -> &preca
				case 5246: state = 5247; break; // &PrecedesEqu -> &PrecedesEqua
				case 5250: state = 5251; break; // &PrecedesSl -> &PrecedesSla
				case 5256: state = 5257; break; // &PrecedesSlantEqu -> &PrecedesSlantEqua
				case 5266: state = 5267; break; // &precn -> &precna
				case 5289: state = 5290; break; // &prn -> &prna
				case 5303: state = 5304; break; // &prof -> &profa
				case 5305: state = 5306; break; // &profal -> &profala
				case 5323: state = 5324; break; // &Proportion -> &Proportiona
				case 5374: state = 5375; break; // &qu -> &qua
				case 5397: state = 5402; break; // &r -> &ra
				case 5398: state = 5399; break; // &rA -> &rAa
				case 5405: state = 5406; break; // &R -> &Ra
				case 5439: state = 5440; break; // &rarr -> &rarra
				case 5462: state = 5463; break; // &rAt -> &rAta
				case 5466: state = 5467; break; // &rat -> &rata
				case 5472: state = 5473; break; // &ration -> &rationa
				case 5476: state = 5477; break; // &RB -> &RBa
				case 5480: state = 5481; break; // &rB -> &rBa
				case 5484: state = 5485; break; // &rb -> &rba
				case 5491: state = 5492; break; // &rbr -> &rbra
				case 5502: state = 5503; break; // &Rc -> &Rca
				case 5507: state = 5508; break; // &rc -> &rca
				case 5527: state = 5528; break; // &rdc -> &rdca
				case 5531: state = 5532; break; // &rdldh -> &rdldha
				case 5541: state = 5542; break; // &re -> &rea
				case 5547: state = 5548; break; // &realp -> &realpa
				case 5604: state = 5605; break; // &rH -> &rHa
				case 5607: state = 5608; break; // &rh -> &rha
				case 5620: state = 5637; break; // &Right -> &Righta
				case 5627: state = 5628; break; // &RightAngleBr -> &RightAngleBra
				case 5645: state = 5646; break; // &right -> &righta
				case 5651: state = 5652; break; // &RightArrowB -> &RightArrowBa
				case 5663: state = 5664; break; // &rightarrowt -> &rightarrowta
				case 5681: state = 5682; break; // &RightDoubleBr -> &RightDoubleBra
				case 5704: state = 5705; break; // &RightDownVectorB -> &RightDownVectorBa
				case 5712: state = 5713; break; // &righth -> &rightha
				case 5728: state = 5729; break; // &rightleft -> &rightlefta
				case 5735: state = 5736; break; // &rightlefth -> &rightleftha
				case 5747: state = 5748; break; // &rightright -> &rightrighta
				case 5758: state = 5759; break; // &rightsquig -> &rightsquiga
				case 5789: state = 5790; break; // &RightTri -> &RightTria
				case 5795: state = 5796; break; // &RightTriangleB -> &RightTriangleBa
				case 5800: state = 5801; break; // &RightTriangleEqu -> &RightTriangleEqua
				case 5830: state = 5831; break; // &RightUpVectorB -> &RightUpVectorBa
				case 5839: state = 5840; break; // &RightVectorB -> &RightVectorBa
				case 5854: state = 5855; break; // &rl -> &rla
				case 5858: state = 5859; break; // &rlh -> &rlha
				case 5866: state = 5867; break; // &rmoust -> &rmousta
				case 5875: state = 5876; break; // &ro -> &roa
				case 5884: state = 5885; break; // &rop -> &ropa
				case 5909: state = 5910; break; // &rp -> &rpa
				case 5920: state = 5921; break; // &rr -> &rra
				case 5928: state = 5929; break; // &Rright -> &Rrighta
				case 5934: state = 5935; break; // &rs -> &rsa
				case 5973: state = 5974; break; // &RuleDel -> &RuleDela
				case 5981: state = 5982; break; // &ruluh -> &ruluha
				case 5985: state = 5986; break; // &S -> &Sa
				case 5991: state = 5992; break; // &s -> &sa
				case 6001: state = 6005; break; // &Sc -> &Sca
				case 6002: state = 6003; break; // &sc -> &sca
				case 6030: state = 6031; break; // &scn -> &scna
				case 6053: state = 6054; break; // &se -> &sea
				case 6069: state = 6070; break; // &sesw -> &seswa
				case 6088: state = 6089; break; // &sh -> &sha
				case 6132: state = 6133; break; // &shortp -> &shortpa
				case 6134: state = 6135; break; // &shortpar -> &shortpara
				case 6160: state = 6161; break; // &Sigm -> &Sigma
				case 6164: state = 6165; break; // &sigm -> &sigma
				case 6184: state = 6185; break; // &simr -> &simra
				case 6188: state = 6189; break; // &sl -> &sla
				case 6192: state = 6193; break; // &Sm -> &Sma
				case 6202: state = 6203; break; // &sm -> &sma
				case 6218: state = 6219; break; // &smep -> &smepa
				case 6241: state = 6242; break; // &solb -> &solba
				case 6249: state = 6250; break; // &sp -> &spa
				case 6259: state = 6260; break; // &sqc -> &sqca
				case 6285: state = 6290; break; // &squ -> &squa
				case 6286: state = 6287; break; // &Squ -> &Squa
				case 6313: state = 6314; break; // &SquareSubsetEqu -> &SquareSubsetEqua
				case 6324: state = 6325; break; // &SquareSupersetEqu -> &SquareSupersetEqua
				case 6334: state = 6335; break; // &sr -> &sra
				case 6352: state = 6353; break; // &sst -> &ssta
				case 6356: state = 6357; break; // &St -> &Sta
				case 6359: state = 6360; break; // &st -> &sta
				case 6363: state = 6364; break; // &str -> &stra
				case 6404: state = 6405; break; // &subr -> &subra
				case 6419: state = 6420; break; // &SubsetEqu -> &SubsetEqua
				case 6432: state = 6433; break; // &succ -> &succa
				case 6454: state = 6455; break; // &SucceedsEqu -> &SucceedsEqua
				case 6458: state = 6459; break; // &SucceedsSl -> &SucceedsSla
				case 6464: state = 6465; break; // &SucceedsSlantEqu -> &SucceedsSlantEqua
				case 6474: state = 6475; break; // &succn -> &succna
				case 6492: state = 6493; break; // &SuchTh -> &SuchTha
				case 6522: state = 6523; break; // &SupersetEqu -> &SupersetEqua
				case 6531: state = 6532; break; // &supl -> &supla
				case 6564: state = 6565; break; // &sw -> &swa
				case 6576: state = 6577; break; // &swnw -> &swnwa
				case 6583: state = 6584; break; // &T -> &Ta
				case 6586: state = 6587; break; // &t -> &ta
				case 6597: state = 6598; break; // &Tc -> &Tca
				case 6602: state = 6603; break; // &tc -> &tca
				case 6646: state = 6647; break; // &Thet -> &Theta
				case 6648: state = 6649; break; // &thet -> &theta
				case 6656: state = 6657; break; // &thick -> &thicka
				case 6670: state = 6671; break; // &ThickSp -> &ThickSpa
				case 6679: state = 6680; break; // &ThinSp -> &ThinSpa
				case 6683: state = 6684; break; // &thk -> &thka
				case 6706: state = 6707; break; // &TildeEqu -> &TildeEqua
				case 6715: state = 6716; break; // &TildeFullEqu -> &TildeFullEqua
				case 6726: state = 6727; break; // &timesb -> &timesba
				case 6733: state = 6734; break; // &toe -> &toea
				case 6749: state = 6750; break; // &tos -> &tosa
				case 6760: state = 6761; break; // &tr -> &tra
				case 6764: state = 6765; break; // &tri -> &tria
				case 6851: state = 6852; break; // &twohe -> &twohea
				case 6857: state = 6858; break; // &twoheadleft -> &twoheadlefta
				case 6867: state = 6868; break; // &twoheadright -> &twoheadrighta
				case 6873: state = 6874; break; // &U -> &Ua
				case 6879: state = 6880; break; // &u -> &ua
				case 6920: state = 6921; break; // &ud -> &uda
				case 6926: state = 6927; break; // &Udbl -> &Udbla
				case 6930: state = 6931; break; // &udbl -> &udbla
				case 6933: state = 6934; break; // &udh -> &udha
				case 6945: state = 6946; break; // &Ugr -> &Ugra
				case 6950: state = 6951; break; // &ugr -> &ugra
				case 6954: state = 6955; break; // &uH -> &uHa
				case 6957: state = 6958; break; // &uh -> &uha
				case 6978: state = 6979; break; // &Um -> &Uma
				case 6982: state = 6983; break; // &um -> &uma
				case 6991: state = 6992; break; // &UnderB -> &UnderBa
				case 6994: state = 6995; break; // &UnderBr -> &UnderBra
				case 7001: state = 7002; break; // &UnderP -> &UnderPa
				case 7031: state = 7037; break; // &Up -> &Upa
				case 7042: state = 7043; break; // &up -> &upa
				case 7048: state = 7049; break; // &UpArrowB -> &UpArrowBa
				case 7072: state = 7073; break; // &Updown -> &Updowna
				case 7081: state = 7082; break; // &updown -> &updowna
				case 7098: state = 7099; break; // &uph -> &upha
				case 7159: state = 7160; break; // &upup -> &upupa
				case 7208: state = 7209; break; // &uu -> &uua
				case 7217: state = 7218; break; // &uw -> &uwa
				case 7223: state = 7224; break; // &v -> &va
				case 7237: state = 7238; break; // &vark -> &varka
				case 7240: state = 7241; break; // &varkapp -> &varkappa
				case 7267: state = 7268; break; // &varsigm -> &varsigma
				case 7289: state = 7290; break; // &varthet -> &vartheta
				case 7292: state = 7293; break; // &vartri -> &vartria
				case 7308: state = 7309; break; // &Vb -> &Vba
				case 7311: state = 7312; break; // &vB -> &vBa
				case 7319: state = 7320; break; // &VD -> &VDa
				case 7323: state = 7324; break; // &Vd -> &Vda
				case 7327: state = 7328; break; // &vD -> &vDa
				case 7331: state = 7332; break; // &vd -> &vda
				case 7340: state = 7341; break; // &veeb -> &veeba
				case 7350: state = 7351; break; // &Verb -> &Verba
				case 7354: state = 7355; break; // &verb -> &verba
				case 7360: state = 7361; break; // &Vertic -> &Vertica
				case 7363: state = 7364; break; // &VerticalB -> &VerticalBa
				case 7372: state = 7373; break; // &VerticalSep -> &VerticalSepa
				case 7374: state = 7375; break; // &VerticalSepar -> &VerticalSepara
				case 7390: state = 7391; break; // &VeryThinSp -> &VeryThinSpa
				case 7437: state = 7438; break; // &Vvd -> &Vvda
				case 7444: state = 7445; break; // &vzigz -> &vzigza
				case 7459: state = 7460; break; // &wedb -> &wedba
				case 7485: state = 7486; break; // &wre -> &wrea
				case 7496: state = 7497; break; // &xc -> &xca
				case 7513: state = 7517; break; // &xh -> &xha
				case 7522: state = 7526; break; // &xl -> &xla
				case 7529: state = 7530; break; // &xm -> &xma
				case 7551: state = 7555; break; // &xr -> &xra
				case 7584: state = 7585; break; // &Y -> &Ya
				case 7590: state = 7591; break; // &y -> &ya
				case 7645: state = 7646; break; // &Z -> &Za
				case 7651: state = 7652; break; // &z -> &za
				case 7657: state = 7658; break; // &Zc -> &Zca
				case 7662: state = 7663; break; // &zc -> &zca
				case 7689: state = 7690; break; // &ZeroWidthSp -> &ZeroWidthSpa
				case 7693: state = 7694; break; // &Zet -> &Zeta
				case 7695: state = 7696; break; // &zet -> &zeta
				case 7709: state = 7710; break; // &zigr -> &zigra
				default: return false;
				}
				break;
			case 'b':
				switch (state) {
				case 0: state = 222; break; // & -> &b
				case 1: state = 13; break; // &A -> &Ab
				case 7: state = 18; break; // &a -> &ab
				case 109: state = 111; break; // &angmsda -> &angmsdab
				case 120: state = 121; break; // &angrtv -> &angrtvb
				case 222: state = 270; break; // &b -> &bb
				case 273: state = 274; break; // &bbrkt -> &bbrktb
				case 470: state = 471; break; // &box -> &boxb
				case 545: state = 546; break; // &brv -> &brvb
				case 562: state = 563; break; // &bsol -> &bsolb
				case 566: state = 567; break; // &bsolhsu -> &bsolhsub
				case 596: state = 600; break; // &cap -> &capb
				case 835: state = 836; break; // &CloseCurlyDou -> &CloseCurlyDoub
				case 850: state = 851; break; // &clu -> &club
				case 978: state = 979; break; // &csu -> &csub
				case 1006: state = 1007; break; // &cup -> &cupb
				case 1097: state = 1120; break; // &d -> &db
				case 1209: state = 1210; break; // &DiacriticalDou -> &DiacriticalDoub
				case 1331: state = 1332; break; // &dou -> &doub
				case 1334: state = 1335; break; // &double -> &doubleb
				case 1343: state = 1344; break; // &Dou -> &Doub
				case 1590: state = 1591; break; // &dr -> &drb
				case 1874: state = 1875; break; // &Equili -> &Equilib
				case 2118: state = 2140; break; // &g -> &gb
				case 2124: state = 2135; break; // &G -> &Gb
				case 2356: state = 2386; break; // &h -> &hb
				case 2418: state = 2419; break; // &Hil -> &Hilb
				case 2474: state = 2475; break; // &hor -> &horb
				case 2524: state = 2525; break; // &hy -> &hyb
				case 2696: state = 2697; break; // &Invisi -> &Invisib
				case 2881: state = 2970; break; // &l -> &lb
				case 2907: state = 2908; break; // &Lam -> &Lamb
				case 2911: state = 2912; break; // &lam -> &lamb
				case 2939: state = 2940; break; // &larr -> &larrb
				case 2970: state = 2974; break; // &lb -> &lbb
				case 3008: state = 3009; break; // &lcu -> &lcub
				case 3090: state = 3091; break; // &LeftDou -> &LeftDoub
				case 3393: state = 3399; break; // &lh -> &lhb
				case 3466: state = 3472; break; // &lo -> &lob
				case 3603: state = 3607; break; // &low -> &lowb
				case 3677: state = 3678; break; // &lsq -> &lsqb
				case 3843: state = 3844; break; // &minus -> &minusb
				case 3897: state = 3930; break; // &n -> &nb
				case 3898: state = 3899; break; // &na -> &nab
				case 4233: state = 4234; break; // &NotDou -> &NotDoub
				case 4339: state = 4341; break; // &notinv -> &notinvb
				case 4426: state = 4428; break; // &notniv -> &notnivb
				case 4494: state = 4495; break; // &NotSquareSu -> &NotSquareSub
				case 4515: state = 4516; break; // &NotSu -> &NotSub
				case 4690: state = 4691; break; // &nsqsu -> &nsqsub
				case 4695: state = 4696; break; // &nsu -> &nsub
				case 4851: state = 4860; break; // &od -> &odb
				case 4855: state = 4856; break; // &Od -> &Odb
				case 4900: state = 4901; break; // &oh -> &ohb
				case 4970: state = 4971; break; // &OpenCurlyDou -> &OpenCurlyDoub
				case 5055: state = 5056; break; // &ov -> &ovb
				case 5159: state = 5164; break; // &plus -> &plusb
				case 5397: state = 5484; break; // &r -> &rb
				case 5439: state = 5442; break; // &rarr -> &rarrb
				case 5484: state = 5488; break; // &rb -> &rbb
				case 5522: state = 5523; break; // &rcu -> &rcub
				case 5573: state = 5574; break; // &ReverseEquili -> &ReverseEquilib
				case 5586: state = 5587; break; // &ReverseUpEquili -> &ReverseUpEquilib
				case 5676: state = 5677; break; // &RightDou -> &RightDoub
				case 5875: state = 5881; break; // &ro -> &rob
				case 5946: state = 5947; break; // &rsq -> &rsqb
				case 5991: state = 5997; break; // &s -> &sb
				case 6050: state = 6051; break; // &sdot -> &sdotb
				case 6240: state = 6241; break; // &sol -> &solb
				case 6270: state = 6271; break; // &sqsu -> &sqsub
				case 6306: state = 6307; break; // &SquareSu -> &SquareSub
				case 6381: state = 6382; break; // &Su -> &Sub
				case 6383: state = 6384; break; // &su -> &sub
				case 6428: state = 6429; break; // &subsu -> &subsub
				case 6508: state = 6509; break; // &supdsu -> &supdsub
				case 6529: state = 6530; break; // &suphsu -> &suphsub
				case 6561: state = 6562; break; // &supsu -> &supsub
				case 6584: state = 6585; break; // &Ta -> &Tab
				case 6586: state = 6594; break; // &t -> &tb
				case 6725: state = 6726; break; // &times -> &timesb
				case 6735: state = 6736; break; // &top -> &topb
				case 6809: state = 6810; break; // &tris -> &trisb
				case 6873: state = 6896; break; // &U -> &Ub
				case 6879: state = 6900; break; // &u -> &ub
				case 6920: state = 6929; break; // &ud -> &udb
				case 6924: state = 6925; break; // &Ud -> &Udb
				case 6957: state = 6962; break; // &uh -> &uhb
				case 7092: state = 7093; break; // &UpEquili -> &UpEquilib
				case 7269: state = 7270; break; // &varsu -> &varsub
				case 7307: state = 7308; break; // &V -> &Vb
				case 7339: state = 7340; break; // &vee -> &veeb
				case 7349: state = 7350; break; // &Ver -> &Verb
				case 7353: state = 7354; break; // &ver -> &verb
				case 7404: state = 7405; break; // &vnsu -> &vnsub
				case 7427: state = 7428; break; // &vsu -> &vsub
				case 7458: state = 7459; break; // &wed -> &wedb
				default: return false;
				}
				break;
			case 'c':
				switch (state) {
				case 0: state = 589; break; // & -> &c
				case 1: state = 26; break; // &A -> &Ac
				case 2: state = 3; break; // &Aa -> &Aac
				case 7: state = 23; break; // &a -> &ac
				case 8: state = 9; break; // &aa -> &aac
				case 28: state = 29; break; // &Acir -> &Acirc
				case 31: state = 32; break; // &acir -> &acirc
				case 76: state = 77; break; // &Ama -> &Amac
				case 80: state = 81; break; // &ama -> &amac
				case 109: state = 112; break; // &angmsda -> &angmsdac
				case 144: state = 145; break; // &apa -> &apac
				case 160: state = 161; break; // &ApplyFun -> &ApplyFunc
				case 180: state = 181; break; // &As -> &Asc
				case 183: state = 184; break; // &as -> &asc
				case 212: state = 213; break; // &aw -> &awc
				case 222: state = 277; break; // &b -> &bc
				case 223: state = 224; break; // &ba -> &bac
				case 225: state = 226; break; // &back -> &backc
				case 247: state = 281; break; // &B -> &Bc
				case 248: state = 249; break; // &Ba -> &Bac
				case 288: state = 289; break; // &be -> &bec
				case 293: state = 294; break; // &Be -> &Bec
				case 334: state = 335; break; // &big -> &bigc
				case 339: state = 340; break; // &bigcir -> &bigcirc
				case 357: state = 358; break; // &bigsq -> &bigsqc
				case 397: state = 398; break; // &bla -> &blac
				case 442: state = 443; break; // &blo -> &bloc
				case 549: state = 550; break; // &Bs -> &Bsc
				case 552: state = 553; break; // &bs -> &bsc
				case 583: state = 645; break; // &C -> &Cc
				case 584: state = 585; break; // &Ca -> &Cac
				case 589: state = 641; break; // &c -> &cc
				case 590: state = 591; break; // &ca -> &cac
				case 596: state = 605; break; // &cap -> &capc
				case 601: state = 602; break; // &capbr -> &capbrc
				case 662: state = 663; break; // &Ccir -> &Ccirc
				case 665: state = 666; break; // &ccir -> &ccirc
				case 716: state = 717; break; // &CH -> &CHc
				case 719: state = 720; break; // &ch -> &chc
				case 722: state = 723; break; // &che -> &chec
				case 733: state = 734; break; // &cir -> &circ
				case 753: state = 757; break; // &circled -> &circledc
				case 759: state = 760; break; // &circledcir -> &circledcirc
				case 766: state = 767; break; // &Cir -> &Circ
				case 799: state = 800; break; // &cirs -> &cirsc
				case 804: state = 805; break; // &Clo -> &Cloc
				case 923: state = 924; break; // &Coprodu -> &Coproduc
				case 939: state = 940; break; // &CounterClo -> &CounterCloc
				case 972: state = 973; break; // &Cs -> &Csc
				case 975: state = 976; break; // &cs -> &csc
				case 997: state = 998; break; // &cues -> &cuesc
				case 1006: state = 1015; break; // &cup -> &cupc
				case 1008: state = 1009; break; // &cupbr -> &cupbrc
				case 1037: state = 1038; break; // &curlyeqpre -> &curlyeqprec
				case 1040: state = 1041; break; // &curlyeqsu -> &curlyeqsuc
				case 1041: state = 1042; break; // &curlyeqsuc -> &curlyeqsucc
				case 1076: state = 1077; break; // &cw -> &cwc
				case 1087: state = 1088; break; // &cyl -> &cylc
				case 1091: state = 1129; break; // &D -> &Dc
				case 1097: state = 1134; break; // &d -> &dc
				case 1127: state = 1128; break; // &dbla -> &dblac
				case 1192: state = 1193; break; // &Dia -> &Diac
				case 1197: state = 1198; break; // &Diacriti -> &Diacritic
				case 1201: state = 1202; break; // &DiacriticalA -> &DiacriticalAc
				case 1213: state = 1214; break; // &DiacriticalDoubleA -> &DiacriticalDoubleAc
				case 1277: state = 1278; break; // &DJ -> &DJc
				case 1280: state = 1281; break; // &dj -> &djc
				case 1283: state = 1284; break; // &dl -> &dlc
				case 1459: state = 1460; break; // &DoubleVerti -> &DoubleVertic
				case 1536: state = 1537; break; // &DownLeftRightVe -> &DownLeftRightVec
				case 1545: state = 1546; break; // &DownLeftTeeVe -> &DownLeftTeeVec
				case 1551: state = 1552; break; // &DownLeftVe -> &DownLeftVec
				case 1568: state = 1569; break; // &DownRightTeeVe -> &DownRightTeeVec
				case 1574: state = 1575; break; // &DownRightVe -> &DownRightVec
				case 1590: state = 1597; break; // &dr -> &drc
				case 1604: state = 1605; break; // &Ds -> &Dsc
				case 1607: state = 1608; break; // &ds -> &dsc
				case 1610: state = 1611; break; // &DS -> &DSc
				case 1644: state = 1645; break; // &DZ -> &DZc
				case 1647: state = 1648; break; // &dz -> &dzc
				case 1656: state = 1672; break; // &E -> &Ec
				case 1657: state = 1658; break; // &Ea -> &Eac
				case 1662: state = 1677; break; // &e -> &ec
				case 1663: state = 1664; break; // &ea -> &eac
				case 1683: state = 1687; break; // &ecir -> &ecirc
				case 1685: state = 1686; break; // &Ecir -> &Ecirc
				case 1747: state = 1748; break; // &Ema -> &Emac
				case 1751: state = 1752; break; // &ema -> &emac
				case 1833: state = 1834; break; // &eq -> &eqc
				case 1836: state = 1837; break; // &eqcir -> &eqcirc
				case 1897: state = 1898; break; // &Es -> &Esc
				case 1900: state = 1901; break; // &es -> &esc
				case 1925: state = 1926; break; // &ex -> &exc
				case 1937: state = 1938; break; // &expe -> &expec
				case 1964: state = 1980; break; // &f -> &fc
				case 1977: state = 1978; break; // &F -> &Fc
				case 2084: state = 2085; break; // &fra -> &frac
				case 2112: state = 2113; break; // &Fs -> &Fsc
				case 2115: state = 2116; break; // &fs -> &fsc
				case 2118: state = 2153; break; // &g -> &gc
				case 2119: state = 2120; break; // &ga -> &gac
				case 2124: state = 2145; break; // &G -> &Gc
				case 2151: state = 2152; break; // &Gcir -> &Gcirc
				case 2155: state = 2156; break; // &gcir -> &gcirc
				case 2176: state = 2177; break; // &ges -> &gesc
				case 2177: state = 2178; break; // &gesc -> &gescc
				case 2198: state = 2199; break; // &GJ -> &GJc
				case 2201: state = 2202; break; // &gj -> &gjc
				case 2282: state = 2283; break; // &Gs -> &Gsc
				case 2285: state = 2286; break; // &gs -> &gsc
				case 2294: state = 2295; break; // &gt -> &gtc
				case 2295: state = 2296; break; // &gtc -> &gtcc
				case 2351: state = 2389; break; // &H -> &Hc
				case 2352: state = 2353; break; // &Ha -> &Hac
				case 2356: state = 2393; break; // &h -> &hc
				case 2370: state = 2371; break; // &HARD -> &HARDc
				case 2374: state = 2375; break; // &hard -> &hardc
				case 2380: state = 2381; break; // &harr -> &harrc
				case 2391: state = 2392; break; // &Hcir -> &Hcirc
				case 2395: state = 2396; break; // &hcir -> &hcirc
				case 2409: state = 2410; break; // &her -> &herc
				case 2425: state = 2426; break; // &HilbertSpa -> &HilbertSpac
				case 2490: state = 2491; break; // &Hs -> &Hsc
				case 2493: state = 2494; break; // &hs -> &hsc
				case 2533: state = 2546; break; // &I -> &Ic
				case 2534: state = 2535; break; // &Ia -> &Iac
				case 2539: state = 2545; break; // &i -> &ic
				case 2540: state = 2541; break; // &ia -> &iac
				case 2548: state = 2549; break; // &Icir -> &Icirc
				case 2551: state = 2552; break; // &icir -> &icirc
				case 2558: state = 2559; break; // &IE -> &IEc
				case 2561: state = 2562; break; // &ie -> &iec
				case 2564: state = 2565; break; // &iex -> &iexc
				case 2605: state = 2606; break; // &Ima -> &Imac
				case 2609: state = 2610; break; // &ima -> &imac
				case 2641: state = 2642; break; // &in -> &inc
				case 2658: state = 2659; break; // &int -> &intc
				case 2672: state = 2673; break; // &inter -> &interc
				case 2678: state = 2679; break; // &Interse -> &Intersec
				case 2710: state = 2711; break; // &IO -> &IOc
				case 2713: state = 2714; break; // &io -> &ioc
				case 2740: state = 2741; break; // &Is -> &Isc
				case 2743: state = 2744; break; // &is -> &isc
				case 2766: state = 2767; break; // &Iuk -> &Iukc
				case 2770: state = 2771; break; // &iuk -> &iukc
				case 2777: state = 2778; break; // &J -> &Jc
				case 2780: state = 2781; break; // &Jcir -> &Jcirc
				case 2782: state = 2783; break; // &j -> &jc
				case 2785: state = 2786; break; // &jcir -> &jcirc
				case 2803: state = 2804; break; // &Js -> &Jsc
				case 2806: state = 2807; break; // &js -> &jsc
				case 2810: state = 2811; break; // &Jser -> &Jserc
				case 2814: state = 2815; break; // &jser -> &jserc
				case 2818: state = 2819; break; // &Juk -> &Jukc
				case 2822: state = 2823; break; // &juk -> &jukc
				case 2825: state = 2836; break; // &K -> &Kc
				case 2830: state = 2841; break; // &k -> &kc
				case 2857: state = 2858; break; // &KH -> &KHc
				case 2860: state = 2861; break; // &kh -> &khc
				case 2863: state = 2864; break; // &KJ -> &KJc
				case 2866: state = 2867; break; // &kj -> &kjc
				case 2875: state = 2876; break; // &Ks -> &Ksc
				case 2878: state = 2879; break; // &ks -> &ksc
				case 2881: state = 2993; break; // &l -> &lc
				case 2886: state = 2988; break; // &L -> &Lc
				case 2887: state = 2888; break; // &La -> &Lac
				case 2892: state = 2893; break; // &la -> &lac
				case 2925: state = 2926; break; // &Lapla -> &Laplac
				case 2978: state = 2979; break; // &lbra -> &lbrac
				case 3012: state = 3013; break; // &ld -> &ldc
				case 3043: state = 3044; break; // &LeftAngleBra -> &LeftAngleBrac
				case 3096: state = 3097; break; // &LeftDoubleBra -> &LeftDoubleBrac
				case 3107: state = 3108; break; // &LeftDownTeeVe -> &LeftDownTeeVec
				case 3113: state = 3114; break; // &LeftDownVe -> &LeftDownVec
				case 3199: state = 3200; break; // &LeftRightVe -> &LeftRightVec
				case 3213: state = 3214; break; // &LeftTeeVe -> &LeftTeeVec
				case 3250: state = 3251; break; // &LeftUpDownVe -> &LeftUpDownVec
				case 3259: state = 3260; break; // &LeftUpTeeVe -> &LeftUpTeeVec
				case 3265: state = 3266; break; // &LeftUpVe -> &LeftUpVec
				case 3274: state = 3275; break; // &LeftVe -> &LeftVec
				case 3291: state = 3292; break; // &les -> &lesc
				case 3292: state = 3293; break; // &lesc -> &lescc
				case 3402: state = 3403; break; // &LJ -> &LJc
				case 3405: state = 3406; break; // &lj -> &ljc
				case 3409: state = 3413; break; // &ll -> &llc
				case 3448: state = 3449; break; // &lmousta -> &lmoustac
				case 3643: state = 3647; break; // &lr -> &lrc
				case 3661: state = 3669; break; // &ls -> &lsc
				case 3666: state = 3667; break; // &Ls -> &Lsc
				case 3692: state = 3693; break; // &lt -> &ltc
				case 3693: state = 3694; break; // &ltc -> &ltcc
				case 3745: state = 3776; break; // &m -> &mc
				case 3746: state = 3747; break; // &ma -> &mac
				case 3755: state = 3781; break; // &M -> &Mc
				case 3811: state = 3812; break; // &MediumSpa -> &MediumSpac
				case 3827: state = 3828; break; // &mi -> &mic
				case 3831: state = 3835; break; // &mid -> &midc
				case 3855: state = 3856; break; // &ml -> &mlc
				case 3876: state = 3877; break; // &Ms -> &Msc
				case 3879: state = 3880; break; // &ms -> &msc
				case 3897: state = 3937; break; // &n -> &nc
				case 3898: state = 3908; break; // &na -> &nac
				case 3902: state = 3940; break; // &N -> &Nc
				case 3903: state = 3904; break; // &Na -> &Nac
				case 3999: state = 4000; break; // &NegativeMediumSpa -> &NegativeMediumSpac
				case 4004: state = 4005; break; // &NegativeThi -> &NegativeThic
				case 4009: state = 4010; break; // &NegativeThickSpa -> &NegativeThickSpac
				case 4015: state = 4016; break; // &NegativeThinSpa -> &NegativeThinSpac
				case 4028: state = 4029; break; // &NegativeVeryThinSpa -> &NegativeVeryThinSpac
				case 4115: state = 4116; break; // &NJ -> &NJc
				case 4118: state = 4119; break; // &nj -> &njc
				case 4207: state = 4208; break; // &NonBreakingSpa -> &NonBreakingSpac
				case 4241: state = 4242; break; // &NotDoubleVerti -> &NotDoubleVertic
				case 4339: state = 4342; break; // &notinv -> &notinvc
				case 4426: state = 4429; break; // &notniv -> &notnivc
				case 4432: state = 4433; break; // &NotPre -> &NotPrec
				case 4515: state = 4525; break; // &NotSu -> &NotSuc
				case 4525: state = 4526; break; // &NotSuc -> &NotSucc
				case 4590: state = 4591; break; // &NotVerti -> &NotVertic
				case 4613: state = 4614; break; // &npr -> &nprc
				case 4617: state = 4618; break; // &npre -> &nprec
				case 4627: state = 4628; break; // &nrarr -> &nrarrc
				case 4653: state = 4654; break; // &ns -> &nsc
				case 4654: state = 4655; break; // &nsc -> &nscc
				case 4659: state = 4660; break; // &Ns -> &Nsc
				case 4695: state = 4705; break; // &nsu -> &nsuc
				case 4705: state = 4706; break; // &nsuc -> &nsucc
				case 4827: state = 4844; break; // &O -> &Oc
				case 4828: state = 4829; break; // &Oa -> &Oac
				case 4833: state = 4841; break; // &o -> &oc
				case 4834: state = 4835; break; // &oa -> &oac
				case 4843: state = 4848; break; // &ocir -> &ocirc
				case 4846: state = 4847; break; // &Ocir -> &Ocirc
				case 4858: state = 4859; break; // &Odbla -> &Odblac
				case 4862: state = 4863; break; // &odbla -> &odblac
				case 4880: state = 4881; break; // &of -> &ofc
				case 4908: state = 4912; break; // &ol -> &olc
				case 4924: state = 4925; break; // &Oma -> &Omac
				case 4928: state = 4929; break; // &oma -> &omac
				case 4937: state = 4938; break; // &Omi -> &Omic
				case 4942: state = 4943; break; // &omi -> &omic
				case 5015: state = 5016; break; // &Os -> &Osc
				case 5018: state = 5019; break; // &os -> &osc
				case 5066: state = 5067; break; // &OverBra -> &OverBrac
				case 5083: state = 5106; break; // &p -> &pc
				case 5096: state = 5104; break; // &P -> &Pc
				case 5109: state = 5110; break; // &per -> &perc
				case 5142: state = 5143; break; // &pit -> &pitc
				case 5152: state = 5153; break; // &plan -> &planc
				case 5159: state = 5165; break; // &plus -> &plusc
				case 5160: state = 5161; break; // &plusa -> &plusac
				case 5191: state = 5192; break; // &Poin -> &Poinc
				case 5216: state = 5219; break; // &pr -> &prc
				case 5223: state = 5224; break; // &pre -> &prec
				case 5224: state = 5231; break; // &prec -> &precc
				case 5238: state = 5239; break; // &Pre -> &Prec
				case 5300: state = 5301; break; // &Produ -> &Produc
				case 5335: state = 5336; break; // &Ps -> &Psc
				case 5338: state = 5339; break; // &ps -> &psc
				case 5344: state = 5345; break; // &pun -> &punc
				case 5368: state = 5369; break; // &Qs -> &Qsc
				case 5371: state = 5372; break; // &qs -> &qsc
				case 5397: state = 5507; break; // &r -> &rc
				case 5402: state = 5403; break; // &ra -> &rac
				case 5405: state = 5502; break; // &R -> &Rc
				case 5406: state = 5407; break; // &Ra -> &Rac
				case 5415: state = 5416; break; // &radi -> &radic
				case 5439: state = 5445; break; // &rarr -> &rarrc
				case 5492: state = 5493; break; // &rbra -> &rbrac
				case 5526: state = 5527; break; // &rd -> &rdc
				case 5541: state = 5552; break; // &re -> &rec
				case 5628: state = 5629; break; // &RightAngleBra -> &RightAngleBrac
				case 5682: state = 5683; break; // &RightDoubleBra -> &RightDoubleBrac
				case 5693: state = 5694; break; // &RightDownTeeVe -> &RightDownTeeVec
				case 5699: state = 5700; break; // &RightDownVe -> &RightDownVec
				case 5773: state = 5774; break; // &RightTeeVe -> &RightTeeVec
				case 5810: state = 5811; break; // &RightUpDownVe -> &RightUpDownVec
				case 5819: state = 5820; break; // &RightUpTeeVe -> &RightUpTeeVec
				case 5825: state = 5826; break; // &RightUpVe -> &RightUpVec
				case 5834: state = 5835; break; // &RightVe -> &RightVec
				case 5867: state = 5868; break; // &rmousta -> &rmoustac
				case 5934: state = 5942; break; // &rs -> &rsc
				case 5939: state = 5940; break; // &Rs -> &Rsc
				case 5985: state = 6001; break; // &S -> &Sc
				case 5986: state = 5987; break; // &Sa -> &Sac
				case 5991: state = 6002; break; // &s -> &sc
				case 5992: state = 5993; break; // &sa -> &sac
				case 6002: state = 6012; break; // &sc -> &scc
				case 6025: state = 6026; break; // &Scir -> &Scirc
				case 6028: state = 6029; break; // &scir -> &scirc
				case 6053: state = 6064; break; // &se -> &sec
				case 6088: state = 6097; break; // &sh -> &shc
				case 6092: state = 6101; break; // &SH -> &SHc
				case 6094: state = 6095; break; // &SHCH -> &SHCHc
				case 6098: state = 6099; break; // &shch -> &shchc
				case 6198: state = 6199; break; // &SmallCir -> &SmallCirc
				case 6232: state = 6233; break; // &SOFT -> &SOFTc
				case 6237: state = 6238; break; // &soft -> &softc
				case 6258: state = 6259; break; // &sq -> &sqc
				case 6299: state = 6300; break; // &SquareInterse -> &SquareIntersec
				case 6338: state = 6339; break; // &Ss -> &Ssc
				case 6341: state = 6342; break; // &ss -> &ssc
				case 6381: state = 6446; break; // &Su -> &Suc
				case 6383: state = 6431; break; // &su -> &suc
				case 6431: state = 6432; break; // &suc -> &succ
				case 6432: state = 6439; break; // &succ -> &succc
				case 6446: state = 6447; break; // &Suc -> &Succ
				case 6583: state = 6597; break; // &T -> &Tc
				case 6586: state = 6602; break; // &t -> &tc
				case 6623: state = 6624; break; // &telre -> &telrec
				case 6654: state = 6655; break; // &thi -> &thic
				case 6666: state = 6667; break; // &Thi -> &Thic
				case 6671: state = 6672; break; // &ThickSpa -> &ThickSpac
				case 6680: state = 6681; break; // &ThinSpa -> &ThinSpac
				case 6735: state = 6739; break; // &top -> &topc
				case 6821: state = 6822; break; // &Ts -> &Tsc
				case 6824: state = 6825; break; // &ts -> &tsc
				case 6827: state = 6828; break; // &TS -> &TSc
				case 6831: state = 6832; break; // &TSH -> &TSHc
				case 6834: state = 6835; break; // &tsh -> &tshc
				case 6873: state = 6910; break; // &U -> &Uc
				case 6874: state = 6875; break; // &Ua -> &Uac
				case 6879: state = 6914; break; // &u -> &uc
				case 6880: state = 6881; break; // &ua -> &uac
				case 6892: state = 6893; break; // &Uarro -> &Uarroc
				case 6897: state = 6898; break; // &Ubr -> &Ubrc
				case 6901: state = 6902; break; // &ubr -> &ubrc
				case 6912: state = 6913; break; // &Ucir -> &Ucirc
				case 6916: state = 6917; break; // &ucir -> &ucirc
				case 6927: state = 6928; break; // &Udbla -> &Udblac
				case 6931: state = 6932; break; // &udbla -> &udblac
				case 6965: state = 6966; break; // &ul -> &ulc
				case 6979: state = 6980; break; // &Uma -> &Umac
				case 6983: state = 6984; break; // &uma -> &umac
				case 6995: state = 6996; break; // &UnderBra -> &UnderBrac
				case 7166: state = 7167; break; // &ur -> &urc
				case 7186: state = 7187; break; // &Us -> &Usc
				case 7189: state = 7190; break; // &us -> &usc
				case 7223: state = 7317; break; // &v -> &vc
				case 7307: state = 7315; break; // &V -> &Vc
				case 7359: state = 7360; break; // &Verti -> &Vertic
				case 7391: state = 7392; break; // &VeryThinSpa -> &VeryThinSpac
				case 7421: state = 7422; break; // &Vs -> &Vsc
				case 7424: state = 7425; break; // &vs -> &vsc
				case 7447: state = 7448; break; // &W -> &Wc
				case 7450: state = 7451; break; // &Wcir -> &Wcirc
				case 7452: state = 7453; break; // &w -> &wc
				case 7455: state = 7456; break; // &wcir -> &wcirc
				case 7489: state = 7490; break; // &Ws -> &Wsc
				case 7492: state = 7493; break; // &ws -> &wsc
				case 7495: state = 7496; break; // &x -> &xc
				case 7500: state = 7501; break; // &xcir -> &xcirc
				case 7558: state = 7559; break; // &Xs -> &Xsc
				case 7561: state = 7562; break; // &xs -> &xsc
				case 7564: state = 7565; break; // &xsq -> &xsqc
				case 7584: state = 7600; break; // &Y -> &Yc
				case 7585: state = 7586; break; // &Ya -> &Yac
				case 7590: state = 7604; break; // &y -> &yc
				case 7591: state = 7592; break; // &ya -> &yac
				case 7596: state = 7597; break; // &YA -> &YAc
				case 7602: state = 7603; break; // &Ycir -> &Ycirc
				case 7606: state = 7607; break; // &ycir -> &ycirc
				case 7616: state = 7617; break; // &YI -> &YIc
				case 7619: state = 7620; break; // &yi -> &yic
				case 7628: state = 7629; break; // &Ys -> &Ysc
				case 7631: state = 7632; break; // &ys -> &ysc
				case 7634: state = 7635; break; // &YU -> &YUc
				case 7637: state = 7638; break; // &yu -> &yuc
				case 7645: state = 7657; break; // &Z -> &Zc
				case 7646: state = 7647; break; // &Za -> &Zac
				case 7651: state = 7662; break; // &z -> &zc
				case 7652: state = 7653; break; // &za -> &zac
				case 7690: state = 7691; break; // &ZeroWidthSpa -> &ZeroWidthSpac
				case 7701: state = 7702; break; // &ZH -> &ZHc
				case 7704: state = 7705; break; // &zh -> &zhc
				case 7719: state = 7720; break; // &Zs -> &Zsc
				case 7722: state = 7723; break; // &zs -> &zsc
				default: return false;
				}
				break;
			case 'd':
				switch (state) {
				case 0: state = 1097; break; // & -> &d
				case 23: state = 24; break; // &ac -> &acd
				case 88: state = 89; break; // &An -> &And
				case 90: state = 91; break; // &an -> &and
				case 91: state = 95; break; // &and -> &andd
				case 93: state = 94; break; // &andan -> &andand
				case 107: state = 108; break; // &angms -> &angmsd
				case 109: state = 113; break; // &angmsda -> &angmsdad
				case 121: state = 122; break; // &angrtvb -> &angrtvbd
				case 150: state = 151; break; // &api -> &apid
				case 198: state = 199; break; // &Atil -> &Atild
				case 203: state = 204; break; // &atil -> &atild
				case 222: state = 284; break; // &b -> &bd
				case 263: state = 264; break; // &Barwe -> &Barwed
				case 266: state = 267; break; // &barwe -> &barwed
				case 343: state = 344; break; // &bigo -> &bigod
				case 371: state = 372; break; // &bigtriangle -> &bigtriangled
				case 387: state = 388; break; // &bigwe -> &bigwed
				case 420: state = 421; break; // &blacktriangle -> &blacktriangled
				case 470: state = 477; break; // &box -> &boxd
				case 484: state = 487; break; // &boxH -> &boxHd
				case 485: state = 489; break; // &boxh -> &boxhd
				case 583: state = 677; break; // &C -> &Cd
				case 589: state = 680; break; // &c -> &cd
				case 596: state = 610; break; // &cap -> &capd
				case 598: state = 599; break; // &capan -> &capand
				case 653: state = 654; break; // &Cce -> &Cced
				case 657: state = 658; break; // &cce -> &cced
				case 683: state = 684; break; // &ce -> &ced
				case 687: state = 688; break; // &Ce -> &Ced
				case 708: state = 709; break; // &center -> &centerd
				case 738: state = 753; break; // &circle -> &circled
				case 753: state = 761; break; // &circled -> &circledd
				case 797: state = 798; break; // &cirmi -> &cirmid
				case 884: state = 885; break; // &cong -> &congd
				case 918: state = 919; break; // &copro -> &coprod
				case 921: state = 922; break; // &Copro -> &Coprod
				case 983: state = 984; break; // &ct -> &ctd
				case 987: state = 988; break; // &cu -> &cud
				case 1006: state = 1020; break; // &cup -> &cupd
				case 1047: state = 1048; break; // &curlywe -> &curlywed
				case 1074: state = 1075; break; // &cuwe -> &cuwed
				case 1097: state = 1142; break; // &d -> &dd
				case 1154: state = 1155; break; // &DDotrah -> &DDotrahd
				case 1225: state = 1226; break; // &DiacriticalTil -> &DiacriticalTild
				case 1233: state = 1234; break; // &Diamon -> &Diamond
				case 1236: state = 1237; break; // &diamon -> &diamond
				case 1264: state = 1265; break; // &divi -> &divid
				case 1307: state = 1308; break; // &doteq -> &doteqd
				case 1339: state = 1340; break; // &doublebarwe -> &doublebarwed
				case 1479: state = 1500; break; // &down -> &downd
				case 1624: state = 1625; break; // &dt -> &dtd
				case 1656: state = 1698; break; // &E -> &Ed
				case 1662: state = 1703; break; // &e -> &ed
				case 1724: state = 1725; break; // &egs -> &egsd
				case 1742: state = 1743; break; // &els -> &elsd
				case 1866: state = 1867; break; // &EqualTil -> &EqualTild
				case 1900: state = 1903; break; // &es -> &esd
				case 1970: state = 1971; break; // &falling -> &fallingd
				case 2008: state = 2009; break; // &Fille -> &Filled
				case 2118: state = 2162; break; // &g -> &gd
				case 2124: state = 2159; break; // &G -> &Gd
				case 2128: state = 2132; break; // &Gamma -> &Gammad
				case 2131: state = 2133; break; // &gamma -> &gammad
				case 2146: state = 2147; break; // &Gce -> &Gced
				case 2176: state = 2179; break; // &ges -> &gesd
				case 2279: state = 2280; break; // &GreaterTil -> &GreaterTild
				case 2294: state = 2299; break; // &gt -> &gtd
				case 2311: state = 2320; break; // &gtr -> &gtrd
				case 2373: state = 2374; break; // &har -> &hard
				case 2533: state = 2555; break; // &I -> &Id
				case 2634: state = 2635; break; // &impe -> &imped
				case 2652: state = 2653; break; // &ino -> &inod
				case 2691: state = 2692; break; // &intpro -> &intprod
				case 2733: state = 2734; break; // &ipro -> &iprod
				case 2747: state = 2748; break; // &isin -> &isind
				case 2758: state = 2759; break; // &Itil -> &Itild
				case 2762: state = 2763; break; // &itil -> &itild
				case 2837: state = 2838; break; // &Kce -> &Kced
				case 2842: state = 2843; break; // &kce -> &kced
				case 2881: state = 3012; break; // &l -> &ld
				case 2908: state = 2909; break; // &Lamb -> &Lambd
				case 2912: state = 2913; break; // &lamb -> &lambd
				case 2918: state = 2919; break; // &lang -> &langd
				case 2985: state = 2986; break; // &lbrksl -> &lbrksld
				case 2998: state = 2999; break; // &Lce -> &Lced
				case 3002: state = 3003; break; // &lce -> &lced
				case 3019: state = 3020; break; // &ldr -> &ldrd
				case 3132: state = 3133; break; // &leftharpoon -> &leftharpoond
				case 3291: state = 3294; break; // &les -> &lesd
				case 3302: state = 3309; break; // &less -> &lessd
				case 3373: state = 3374; break; // &LessTil -> &LessTild
				case 3395: state = 3396; break; // &lhar -> &lhard
				case 3429: state = 3430; break; // &llhar -> &llhard
				case 3435: state = 3436; break; // &Lmi -> &Lmid
				case 3440: state = 3441; break; // &lmi -> &lmid
				case 3655: state = 3656; break; // &lrhar -> &lrhard
				case 3692: state = 3697; break; // &lt -> &ltd
				case 3725: state = 3726; break; // &lur -> &lurd
				case 3745: state = 3784; break; // &m -> &md
				case 3761: state = 3762; break; // &mapsto -> &mapstod
				case 3797: state = 3798; break; // &measure -> &measured
				case 3804: state = 3805; break; // &Me -> &Med
				case 3827: state = 3831; break; // &mi -> &mid
				case 3831: state = 3838; break; // &mid -> &midd
				case 3843: state = 3845; break; // &minus -> &minusd
				case 3855: state = 3858; break; // &ml -> &mld
				case 3865: state = 3866; break; // &mo -> &mod
				case 3897: state = 3966; break; // &n -> &nd
				case 3916: state = 3917; break; // &napi -> &napid
				case 3948: state = 3949; break; // &Nce -> &Nced
				case 3952: state = 3953; break; // &nce -> &nced
				case 3958: state = 3959; break; // &ncong -> &ncongd
				case 3970: state = 3981; break; // &ne -> &ned
				case 3992: state = 3993; break; // &NegativeMe -> &NegativeMed
				case 4043: state = 4044; break; // &Neste -> &Nested
				case 4112: state = 4113; break; // &nis -> &nisd
				case 4121: state = 4128; break; // &nl -> &nld
				case 4188: state = 4189; break; // &nmi -> &nmid
				case 4261: state = 4262; break; // &NotEqualTil -> &NotEqualTild
				case 4313: state = 4314; break; // &NotGreaterTil -> &NotGreaterTild
				case 4334: state = 4335; break; // &notin -> &notind
				case 4393: state = 4394; break; // &NotLessTil -> &NotLessTild
				case 4400: state = 4401; break; // &NotNeste -> &NotNested
				case 4434: state = 4435; break; // &NotPrece -> &NotPreced
				case 4528: state = 4529; break; // &NotSuccee -> &NotSucceed
				case 4548: state = 4549; break; // &NotSucceedsTil -> &NotSucceedsTild
				case 4564: state = 4565; break; // &NotTil -> &NotTild
				case 4583: state = 4584; break; // &NotTildeTil -> &NotTildeTild
				case 4668: state = 4669; break; // &nshortmi -> &nshortmid
				case 4683: state = 4684; break; // &nsmi -> &nsmid
				case 4723: state = 4724; break; // &Ntil -> &Ntild
				case 4727: state = 4728; break; // &ntil -> &ntild
				case 4760: state = 4776; break; // &nv -> &nvd
				case 4763: state = 4768; break; // &nV -> &nVd
				case 4827: state = 4855; break; // &O -> &Od
				case 4833: state = 4851; break; // &o -> &od
				case 4870: state = 4871; break; // &odsol -> &odsold
				case 4942: state = 4947; break; // &omi -> &omid
				case 4991: state = 4995; break; // &or -> &ord
				case 5033: state = 5034; break; // &Otil -> &Otild
				case 5038: state = 5039; break; // &otil -> &otild
				case 5114: state = 5115; break; // &perio -> &period
				case 5159: state = 5168; break; // &plus -> &plusd
				case 5213: state = 5214; break; // &poun -> &pound
				case 5240: state = 5241; break; // &Prece -> &Preced
				case 5261: state = 5262; break; // &PrecedesTil -> &PrecedesTild
				case 5296: state = 5297; break; // &pro -> &prod
				case 5298: state = 5299; break; // &Pro -> &Prod
				case 5397: state = 5526; break; // &r -> &rd
				case 5402: state = 5414; break; // &ra -> &rad
				case 5426: state = 5427; break; // &rang -> &rangd
				case 5499: state = 5500; break; // &rbrksl -> &rbrksld
				case 5512: state = 5513; break; // &Rce -> &Rced
				case 5516: state = 5517; break; // &rce -> &rced
				case 5529: state = 5530; break; // &rdl -> &rdld
				case 5609: state = 5610; break; // &rhar -> &rhard
				case 5718: state = 5719; break; // &rightharpoon -> &rightharpoond
				case 5847: state = 5848; break; // &rising -> &risingd
				case 5873: state = 5874; break; // &rnmi -> &rnmid
				case 5900: state = 5901; break; // &Roun -> &Round
				case 5976: state = 5977; break; // &RuleDelaye -> &RuleDelayed
				case 5991: state = 6048; break; // &s -> &sd
				case 6016: state = 6021; break; // &sce -> &sced
				case 6017: state = 6018; break; // &Sce -> &Sced
				case 6130: state = 6131; break; // &shortmi -> &shortmid
				case 6168: state = 6169; break; // &sim -> &simd
				case 6223: state = 6224; break; // &smi -> &smid
				case 6250: state = 6251; break; // &spa -> &spad
				case 6384: state = 6385; break; // &sub -> &subd
				case 6389: state = 6390; break; // &sube -> &subed
				case 6449: state = 6450; break; // &Succee -> &Succeed
				case 6469: state = 6470; break; // &SucceedsTil -> &SucceedsTild
				case 6500: state = 6504; break; // &sup -> &supd
				case 6511: state = 6512; break; // &supe -> &suped
				case 6586: state = 6617; break; // &t -> &td
				case 6607: state = 6608; break; // &Tce -> &Tced
				case 6611: state = 6612; break; // &tce -> &tced
				case 6697: state = 6698; break; // &Til -> &Tild
				case 6701: state = 6702; break; // &til -> &tild
				case 6720: state = 6721; break; // &TildeTil -> &TildeTild
				case 6725: state = 6729; break; // &times -> &timesd
				case 6761: state = 6762; break; // &tra -> &trad
				case 6764: state = 6788; break; // &tri -> &trid
				case 6769: state = 6770; break; // &triangle -> &triangled
				case 6852: state = 6853; break; // &twohea -> &twohead
				case 6873: state = 6924; break; // &U -> &Ud
				case 6879: state = 6920; break; // &u -> &ud
				case 6987: state = 6988; break; // &Un -> &Und
				case 7031: state = 7069; break; // &Up -> &Upd
				case 7042: state = 7078; break; // &up -> &upd
				case 7192: state = 7193; break; // &ut -> &utd
				case 7198: state = 7199; break; // &Util -> &Utild
				case 7202: state = 7203; break; // &util -> &utild
				case 7223: state = 7331; break; // &v -> &vd
				case 7307: state = 7323; break; // &V -> &Vd
				case 7381: state = 7382; break; // &VerticalTil -> &VerticalTild
				case 7436: state = 7437; break; // &Vv -> &Vvd
				case 7457: state = 7458; break; // &we -> &wed
				case 7462: state = 7463; break; // &We -> &Wed
				case 7495: state = 7504; break; // &x -> &xd
				case 7535: state = 7536; break; // &xo -> &xod
				case 7580: state = 7581; break; // &xwe -> &xwed
				case 7645: state = 7669; break; // &Z -> &Zd
				case 7651: state = 7672; break; // &z -> &zd
				case 7684: state = 7685; break; // &ZeroWi -> &ZeroWid
				default: return false;
				}
				break;
			case 'e':
				switch (state) {
				case 0: state = 1662; break; // & -> &e
				case 5: state = 6; break; // &Aacut -> &Aacute
				case 7: state = 42; break; // &a -> &ae
				case 11: state = 12; break; // &aacut -> &aacute
				case 14: state = 15; break; // &Abr -> &Abre
				case 16: state = 17; break; // &Abrev -> &Abreve
				case 19: state = 20; break; // &abr -> &abre
				case 21: state = 22; break; // &abrev -> &abreve
				case 34: state = 35; break; // &acut -> &acute
				case 53: state = 54; break; // &Agrav -> &Agrave
				case 58: state = 59; break; // &agrav -> &agrave
				case 60: state = 61; break; // &al -> &ale
				case 99: state = 100; break; // &andslop -> &andslope
				case 102: state = 103; break; // &ang -> &ange
				case 104: state = 105; break; // &angl -> &angle
				case 109: state = 114; break; // &angmsda -> &angmsdae
				case 143: state = 149; break; // &ap -> &ape
				case 169: state = 170; break; // &approx -> &approxe
				case 193: state = 194; break; // &asymp -> &asympe
				case 199: state = 200; break; // &Atild -> &Atilde
				case 204: state = 205; break; // &atild -> &atilde
				case 222: state = 288; break; // &b -> &be
				case 225: state = 230; break; // &back -> &backe
				case 240: state = 241; break; // &backprim -> &backprime
				case 244: state = 245; break; // &backsim -> &backsime
				case 247: state = 293; break; // &B -> &Be
				case 259: state = 260; break; // &barv -> &barve
				case 260: state = 261; break; // &barve -> &barvee
				case 262: state = 263; break; // &Barw -> &Barwe
				case 265: state = 266; break; // &barw -> &barwe
				case 268: state = 269; break; // &barwedg -> &barwedge
				case 292: state = 299; break; // &becaus -> &because
				case 297: state = 298; break; // &Becaus -> &Because
				case 325: state = 326; break; // &betw -> &betwe
				case 326: state = 327; break; // &betwe -> &betwee
				case 353: state = 354; break; // &bigotim -> &bigotime
				case 370: state = 371; break; // &bigtriangl -> &bigtriangle
				case 383: state = 384; break; // &bigv -> &bigve
				case 384: state = 385; break; // &bigve -> &bigvee
				case 386: state = 387; break; // &bigw -> &bigwe
				case 389: state = 390; break; // &bigwedg -> &bigwedge
				case 402: state = 403; break; // &blackloz -> &blackloze
				case 405: state = 406; break; // &blacklozeng -> &blacklozenge
				case 411: state = 412; break; // &blacksquar -> &blacksquare
				case 419: state = 420; break; // &blacktriangl -> &blacktriangle
				case 425: state = 426; break; // &blacktrianglel -> &blacktrianglele
				case 445: state = 446; break; // &bn -> &bne
				case 468: state = 469; break; // &bowti -> &bowtie
				case 505: state = 506; break; // &boxtim -> &boxtime
				case 535: state = 536; break; // &bprim -> &bprime
				case 537: state = 538; break; // &Br -> &Bre
				case 539: state = 540; break; // &Brev -> &Breve
				case 541: state = 542; break; // &br -> &bre
				case 543: state = 544; break; // &brev -> &breve
				case 552: state = 555; break; // &bs -> &bse
				case 559: state = 560; break; // &bsim -> &bsime
				case 570: state = 571; break; // &bull -> &bulle
				case 574: state = 576; break; // &bump -> &bumpe
				case 579: state = 580; break; // &Bump -> &Bumpe
				case 583: state = 687; break; // &C -> &Ce
				case 587: state = 588; break; // &Cacut -> &Cacute
				case 589: state = 683; break; // &c -> &ce
				case 593: state = 594; break; // &cacut -> &cacute
				case 620: state = 621; break; // &CapitalDiff -> &CapitalDiffe
				case 622: state = 623; break; // &CapitalDiffer -> &CapitalDiffere
				case 631: state = 632; break; // &car -> &care
				case 637: state = 638; break; // &Cayl -> &Cayle
				case 641: state = 657; break; // &cc -> &cce
				case 645: state = 653; break; // &Cc -> &Cce
				case 699: state = 707; break; // &cent -> &cente
				case 701: state = 702; break; // &Cent -> &Cente
				case 719: state = 722; break; // &ch -> &che
				case 733: state = 790; break; // &cir -> &cire
				case 734: state = 735; break; // &circ -> &circe
				case 737: state = 738; break; // &circl -> &circle
				case 744: state = 745; break; // &circlearrowl -> &circlearrowle
				case 768: state = 769; break; // &Circl -> &Circle
				case 786: state = 787; break; // &CircleTim -> &CircleTime
				case 809: state = 810; break; // &Clockwis -> &Clockwise
				case 820: state = 821; break; // &ClockwiseContourInt -> &ClockwiseContourInte
				case 826: state = 827; break; // &Clos -> &Close
				case 837: state = 838; break; // &CloseCurlyDoubl -> &CloseCurlyDouble
				case 842: state = 843; break; // &CloseCurlyDoubleQuot -> &CloseCurlyDoubleQuote
				case 847: state = 848; break; // &CloseCurlyQuot -> &CloseCurlyQuote
				case 859: state = 864; break; // &Colon -> &Colone
				case 863: state = 865; break; // &colon -> &colone
				case 874: state = 875; break; // &compl -> &comple
				case 876: state = 877; break; // &complem -> &compleme
				case 880: state = 881; break; // &complex -> &complexe
				case 891: state = 892; break; // &Congru -> &Congrue
				case 907: state = 908; break; // &ContourInt -> &ContourInte
				case 934: state = 935; break; // &Count -> &Counte
				case 944: state = 945; break; // &CounterClockwis -> &CounterClockwise
				case 955: state = 956; break; // &CounterClockwiseContourInt -> &CounterClockwiseContourInte
				case 979: state = 980; break; // &csub -> &csube
				case 981: state = 982; break; // &csup -> &csupe
				case 987: state = 994; break; // &cu -> &cue
				case 1032: state = 1033; break; // &curly -> &curlye
				case 1036: state = 1037; break; // &curlyeqpr -> &curlyeqpre
				case 1043: state = 1044; break; // &curlyv -> &curlyve
				case 1044: state = 1045; break; // &curlyve -> &curlyvee
				case 1046: state = 1047; break; // &curlyw -> &curlywe
				case 1049: state = 1050; break; // &curlywedg -> &curlywedge
				case 1051: state = 1052; break; // &curr -> &curre
				case 1054: state = 1055; break; // &curv -> &curve
				case 1061: state = 1062; break; // &curvearrowl -> &curvearrowle
				case 1070: state = 1071; break; // &cuv -> &cuve
				case 1071: state = 1072; break; // &cuve -> &cuvee
				case 1073: state = 1074; break; // &cuw -> &cuwe
				case 1091: state = 1163; break; // &D -> &De
				case 1094: state = 1095; break; // &Dagg -> &Dagge
				case 1097: state = 1161; break; // &d -> &de
				case 1100: state = 1101; break; // &dagg -> &dagge
				case 1103: state = 1104; break; // &dal -> &dale
				case 1145: state = 1146; break; // &ddagg -> &ddagge
				case 1158: state = 1159; break; // &ddots -> &ddotse
				case 1204: state = 1205; break; // &DiacriticalAcut -> &DiacriticalAcute
				case 1211: state = 1212; break; // &DiacriticalDoubl -> &DiacriticalDouble
				case 1216: state = 1217; break; // &DiacriticalDoubleAcut -> &DiacriticalDoubleAcute
				case 1221: state = 1222; break; // &DiacriticalGrav -> &DiacriticalGrave
				case 1226: state = 1227; break; // &DiacriticalTild -> &DiacriticalTilde
				case 1228: state = 1243; break; // &di -> &die
				case 1245: state = 1246; break; // &Diff -> &Diffe
				case 1247: state = 1248; break; // &Differ -> &Differe
				case 1265: state = 1266; break; // &divid -> &divide
				case 1271: state = 1272; break; // &divideontim -> &divideontime
				case 1302: state = 1306; break; // &dot -> &dote
				case 1329: state = 1330; break; // &dotsquar -> &dotsquare
				case 1333: state = 1334; break; // &doubl -> &double
				case 1338: state = 1339; break; // &doublebarw -> &doublebarwe
				case 1341: state = 1342; break; // &doublebarwedg -> &doublebarwedge
				case 1345: state = 1346; break; // &Doubl -> &Double
				case 1356: state = 1357; break; // &DoubleContourInt -> &DoubleContourInte
				case 1372: state = 1373; break; // &DoubleL -> &DoubleLe
				case 1391: state = 1392; break; // &DoubleLeftT -> &DoubleLeftTe
				case 1392: state = 1393; break; // &DoubleLeftTe -> &DoubleLeftTee
				case 1397: state = 1398; break; // &DoubleLongL -> &DoubleLongLe
				case 1436: state = 1437; break; // &DoubleRightT -> &DoubleRightTe
				case 1437: state = 1438; break; // &DoubleRightTe -> &DoubleRightTee
				case 1455: state = 1456; break; // &DoubleV -> &DoubleVe
				case 1496: state = 1497; break; // &DownBr -> &DownBre
				case 1498: state = 1499; break; // &DownBrev -> &DownBreve
				case 1517: state = 1518; break; // &downharpoonl -> &downharpoonle
				case 1526: state = 1527; break; // &DownL -> &DownLe
				case 1535: state = 1536; break; // &DownLeftRightV -> &DownLeftRightVe
				case 1541: state = 1542; break; // &DownLeftT -> &DownLeftTe
				case 1542: state = 1543; break; // &DownLeftTe -> &DownLeftTee
				case 1544: state = 1545; break; // &DownLeftTeeV -> &DownLeftTeeVe
				case 1550: state = 1551; break; // &DownLeftV -> &DownLeftVe
				case 1564: state = 1565; break; // &DownRightT -> &DownRightTe
				case 1565: state = 1566; break; // &DownRightTe -> &DownRightTee
				case 1567: state = 1568; break; // &DownRightTeeV -> &DownRightTeeVe
				case 1573: state = 1574; break; // &DownRightV -> &DownRightVe
				case 1582: state = 1583; break; // &DownT -> &DownTe
				case 1583: state = 1584; break; // &DownTe -> &DownTee
				case 1642: state = 1643; break; // &dwangl -> &dwangle
				case 1660: state = 1661; break; // &Eacut -> &Eacute
				case 1662: state = 1706; break; // &e -> &ee
				case 1666: state = 1667; break; // &eacut -> &eacute
				case 1669: state = 1670; break; // &east -> &easte
				case 1718: state = 1719; break; // &Egrav -> &Egrave
				case 1722: state = 1723; break; // &egrav -> &egrave
				case 1729: state = 1730; break; // &El -> &Ele
				case 1731: state = 1732; break; // &Elem -> &Eleme
				case 1737: state = 1738; break; // &elint -> &elinte
				case 1757: state = 1758; break; // &emptys -> &emptyse
				case 1772: state = 1773; break; // &EmptySmallSquar -> &EmptySmallSquare
				case 1775: state = 1776; break; // &EmptyV -> &EmptyVe
				case 1788: state = 1789; break; // &EmptyVerySmallSquar -> &EmptyVerySmallSquare
				case 1852: state = 1853; break; // &eqslantl -> &eqslantle
				case 1860: state = 1869; break; // &equ -> &eque
				case 1867: state = 1868; break; // &EqualTild -> &EqualTilde
				case 1936: state = 1937; break; // &exp -> &expe
				case 1947: state = 1948; break; // &Expon -> &Expone
				case 1956: state = 1957; break; // &expon -> &expone
				case 1962: state = 1963; break; // &exponential -> &exponentiale
				case 1964: state = 1982; break; // &f -> &fe
				case 1974: state = 1975; break; // &fallingdots -> &fallingdotse
				case 1985: state = 1986; break; // &femal -> &female
				case 2007: state = 2008; break; // &Fill -> &Fille
				case 2019: state = 2020; break; // &FilledSmallSquar -> &FilledSmallSquare
				case 2021: state = 2022; break; // &FilledV -> &FilledVe
				case 2034: state = 2035; break; // &FilledVerySmallSquar -> &FilledVerySmallSquare
				case 2070: state = 2071; break; // &Fouri -> &Fourie
				case 2118: state = 2166; break; // &g -> &ge
				case 2122: state = 2123; break; // &gacut -> &gacute
				case 2136: state = 2137; break; // &Gbr -> &Gbre
				case 2138: state = 2139; break; // &Gbrev -> &Gbreve
				case 2141: state = 2142; break; // &gbr -> &gbre
				case 2143: state = 2144; break; // &gbrev -> &gbreve
				case 2145: state = 2146; break; // &Gc -> &Gce
				case 2184: state = 2185; break; // &gesl -> &gesle
				case 2195: state = 2196; break; // &gim -> &gime
				case 2208: state = 2216; break; // &gn -> &gne
				case 2230: state = 2231; break; // &grav -> &grave
				case 2232: state = 2233; break; // &Gr -> &Gre
				case 2235: state = 2236; break; // &Great -> &Greate
				case 2243: state = 2244; break; // &GreaterEqualL -> &GreaterEqualLe
				case 2257: state = 2258; break; // &GreaterGr -> &GreaterGre
				case 2260: state = 2261; break; // &GreaterGreat -> &GreaterGreate
				case 2263: state = 2264; break; // &GreaterL -> &GreaterLe
				case 2280: state = 2281; break; // &GreaterTild -> &GreaterTilde
				case 2289: state = 2290; break; // &gsim -> &gsime
				case 2307: state = 2308; break; // &gtqu -> &gtque
				case 2311: state = 2323; break; // &gtr -> &gtre
				case 2325: state = 2326; break; // &gtreql -> &gtreqle
				case 2330: state = 2331; break; // &gtreqql -> &gtreqqle
				case 2334: state = 2335; break; // &gtrl -> &gtrle
				case 2341: state = 2342; break; // &gv -> &gve
				case 2345: state = 2346; break; // &gvertn -> &gvertne
				case 2353: state = 2354; break; // &Hac -> &Hace
				case 2356: state = 2397; break; // &h -> &he
				case 2419: state = 2420; break; // &Hilb -> &Hilbe
				case 2426: state = 2427; break; // &HilbertSpac -> &HilbertSpace
				case 2429: state = 2430; break; // &hks -> &hkse
				case 2450: state = 2451; break; // &hookl -> &hookle
				case 2488: state = 2489; break; // &HorizontalLin -> &HorizontalLine
				case 2530: state = 2531; break; // &hyph -> &hyphe
				case 2537: state = 2538; break; // &Iacut -> &Iacute
				case 2539: state = 2561; break; // &i -> &ie
				case 2543: state = 2544; break; // &iacut -> &iacute
				case 2575: state = 2576; break; // &Igrav -> &Igrave
				case 2580: state = 2581; break; // &igrav -> &igrave
				case 2612: state = 2613; break; // &imag -> &image
				case 2623: state = 2624; break; // &imaglin -> &imagline
				case 2633: state = 2634; break; // &imp -> &impe
				case 2638: state = 2639; break; // &Impli -> &Implie
				case 2644: state = 2645; break; // &incar -> &incare
				case 2650: state = 2651; break; // &infinti -> &infintie
				case 2657: state = 2667; break; // &Int -> &Inte
				case 2658: state = 2662; break; // &int -> &inte
				case 2663: state = 2664; break; // &integ -> &intege
				case 2677: state = 2678; break; // &Inters -> &Interse
				case 2698: state = 2699; break; // &Invisibl -> &Invisible
				case 2707: state = 2708; break; // &InvisibleTim -> &InvisibleTime
				case 2736: state = 2737; break; // &iqu -> &ique
				case 2759: state = 2760; break; // &Itild -> &Itilde
				case 2763: state = 2764; break; // &itild -> &itilde
				case 2803: state = 2809; break; // &Js -> &Jse
				case 2806: state = 2813; break; // &js -> &jse
				case 2836: state = 2837; break; // &Kc -> &Kce
				case 2841: state = 2842; break; // &kc -> &kce
				case 2853: state = 2854; break; // &kgr -> &kgre
				case 2854: state = 2855; break; // &kgre -> &kgree
				case 2881: state = 3032; break; // &l -> &le
				case 2886: state = 3033; break; // &L -> &Le
				case 2890: state = 2891; break; // &Lacut -> &Lacute
				case 2892: state = 2897; break; // &la -> &lae
				case 2895: state = 2896; break; // &lacut -> &lacute
				case 2920: state = 2921; break; // &langl -> &langle
				case 2926: state = 2927; break; // &Laplac -> &Laplace
				case 2956: state = 2964; break; // &lat -> &late
				case 2979: state = 2980; break; // &lbrac -> &lbrace
				case 2982: state = 2983; break; // &lbrk -> &lbrke
				case 2988: state = 2998; break; // &Lc -> &Lce
				case 2993: state = 3002; break; // &lc -> &lce
				case 3039: state = 3040; break; // &LeftAngl -> &LeftAngle
				case 3045: state = 3046; break; // &LeftAngleBrack -> &LeftAngleBracke
				case 3081: state = 3082; break; // &LeftC -> &LeftCe
				case 3092: state = 3093; break; // &LeftDoubl -> &LeftDouble
				case 3098: state = 3099; break; // &LeftDoubleBrack -> &LeftDoubleBracke
				case 3103: state = 3104; break; // &LeftDownT -> &LeftDownTe
				case 3104: state = 3105; break; // &LeftDownTe -> &LeftDownTee
				case 3106: state = 3107; break; // &LeftDownTeeV -> &LeftDownTeeVe
				case 3112: state = 3113; break; // &LeftDownV -> &LeftDownVe
				case 3139: state = 3140; break; // &leftl -> &leftle
				case 3198: state = 3199; break; // &LeftRightV -> &LeftRightVe
				case 3204: state = 3205; break; // &LeftT -> &LeftTe
				case 3205: state = 3206; break; // &LeftTe -> &LeftTee
				case 3212: state = 3213; break; // &LeftTeeV -> &LeftTeeVe
				case 3220: state = 3221; break; // &leftthr -> &leftthre
				case 3221: state = 3222; break; // &leftthre -> &leftthree
				case 3225: state = 3226; break; // &leftthreetim -> &leftthreetime
				case 3233: state = 3234; break; // &LeftTriangl -> &LeftTriangle
				case 3249: state = 3250; break; // &LeftUpDownV -> &LeftUpDownVe
				case 3255: state = 3256; break; // &LeftUpT -> &LeftUpTe
				case 3256: state = 3257; break; // &LeftUpTe -> &LeftUpTee
				case 3258: state = 3259; break; // &LeftUpTeeV -> &LeftUpTeeVe
				case 3264: state = 3265; break; // &LeftUpV -> &LeftUpVe
				case 3273: state = 3274; break; // &LeftV -> &LeftVe
				case 3299: state = 3300; break; // &lesg -> &lesge
				case 3302: state = 3312; break; // &less -> &lesse
				case 3329: state = 3330; break; // &LessEqualGr -> &LessEqualGre
				case 3332: state = 3333; break; // &LessEqualGreat -> &LessEqualGreate
				case 3345: state = 3346; break; // &LessGr -> &LessGre
				case 3348: state = 3349; break; // &LessGreat -> &LessGreate
				case 3354: state = 3355; break; // &LessL -> &LessLe
				case 3374: state = 3375; break; // &LessTild -> &LessTilde
				case 3408: state = 3419; break; // &Ll -> &Lle
				case 3416: state = 3417; break; // &llcorn -> &llcorne
				case 3450: state = 3451; break; // &lmoustach -> &lmoustache
				case 3452: state = 3460; break; // &ln -> &lne
				case 3478: state = 3479; break; // &LongL -> &LongLe
				case 3487: state = 3488; break; // &Longl -> &Longle
				case 3498: state = 3499; break; // &longl -> &longle
				case 3580: state = 3581; break; // &looparrowl -> &looparrowle
				case 3600: state = 3601; break; // &lotim -> &lotime
				case 3610: state = 3611; break; // &Low -> &Lowe
				case 3613: state = 3614; break; // &LowerL -> &LowerLe
				case 3632: state = 3633; break; // &loz -> &loze
				case 3635: state = 3636; break; // &lozeng -> &lozenge
				case 3650: state = 3651; break; // &lrcorn -> &lrcorne
				case 3674: state = 3675; break; // &lsim -> &lsime
				case 3701: state = 3702; break; // &lthr -> &lthre
				case 3702: state = 3703; break; // &lthre -> &lthree
				case 3705: state = 3706; break; // &ltim -> &ltime
				case 3713: state = 3714; break; // &ltqu -> &ltque
				case 3718: state = 3719; break; // &ltri -> &ltrie
				case 3735: state = 3736; break; // &lv -> &lve
				case 3739: state = 3740; break; // &lvertn -> &lvertne
				case 3745: state = 3792; break; // &m -> &me
				case 3749: state = 3750; break; // &mal -> &male
				case 3751: state = 3752; break; // &malt -> &malte
				case 3753: state = 3754; break; // &maltes -> &maltese
				case 3755: state = 3804; break; // &M -> &Me
				case 3766: state = 3767; break; // &mapstol -> &mapstole
				case 3773: state = 3774; break; // &mark -> &marke
				case 3796: state = 3797; break; // &measur -> &measure
				case 3802: state = 3803; break; // &measuredangl -> &measuredangle
				case 3812: state = 3813; break; // &MediumSpac -> &MediumSpace
				case 3866: state = 3867; break; // &mod -> &mode
				case 3897: state = 3970; break; // &n -> &ne
				case 3902: state = 3984; break; // &N -> &Ne
				case 3906: state = 3907; break; // &Nacut -> &Nacute
				case 3910: state = 3911; break; // &nacut -> &nacute
				case 3935: state = 3936; break; // &nbump -> &nbumpe
				case 3937: state = 3952; break; // &nc -> &nce
				case 3940: state = 3948; break; // &Nc -> &Nce
				case 3989: state = 3990; break; // &Negativ -> &Negative
				case 3991: state = 3992; break; // &NegativeM -> &NegativeMe
				case 4000: state = 4001; break; // &NegativeMediumSpac -> &NegativeMediumSpace
				case 4010: state = 4011; break; // &NegativeThickSpac -> &NegativeThickSpace
				case 4016: state = 4017; break; // &NegativeThinSpac -> &NegativeThinSpace
				case 4018: state = 4019; break; // &NegativeV -> &NegativeVe
				case 4029: state = 4030; break; // &NegativeVeryThinSpac -> &NegativeVeryThinSpace
				case 4035: state = 4036; break; // &nes -> &nese
				case 4042: state = 4043; break; // &Nest -> &Neste
				case 4046: state = 4047; break; // &NestedGr -> &NestedGre
				case 4049: state = 4050; break; // &NestedGreat -> &NestedGreate
				case 4053: state = 4054; break; // &NestedGreaterGr -> &NestedGreaterGre
				case 4056: state = 4057; break; // &NestedGreaterGreat -> &NestedGreaterGreate
				case 4059: state = 4060; break; // &NestedL -> &NestedLe
				case 4063: state = 4064; break; // &NestedLessL -> &NestedLessLe
				case 4070: state = 4071; break; // &NewLin -> &NewLine
				case 4081: state = 4083; break; // &ng -> &nge
				case 4121: state = 4131; break; // &nl -> &nle
				case 4132: state = 4133; break; // &nL -> &nLe
				case 4184: state = 4185; break; // &nltri -> &nltrie
				case 4192: state = 4193; break; // &NoBr -> &NoBre
				case 4198: state = 4199; break; // &NonBr -> &NonBre
				case 4208: state = 4209; break; // &NonBreakingSpac -> &NonBreakingSpace
				case 4222: state = 4223; break; // &NotCongru -> &NotCongrue
				case 4235: state = 4236; break; // &NotDoubl -> &NotDouble
				case 4237: state = 4238; break; // &NotDoubleV -> &NotDoubleVe
				case 4249: state = 4250; break; // &NotEl -> &NotEle
				case 4251: state = 4252; break; // &NotElem -> &NotEleme
				case 4262: state = 4263; break; // &NotEqualTild -> &NotEqualTilde
				case 4270: state = 4271; break; // &NotGr -> &NotGre
				case 4273: state = 4274; break; // &NotGreat -> &NotGreate
				case 4291: state = 4292; break; // &NotGreaterGr -> &NotGreaterGre
				case 4294: state = 4295; break; // &NotGreaterGreat -> &NotGreaterGreate
				case 4297: state = 4298; break; // &NotGreaterL -> &NotGreaterLe
				case 4314: state = 4315; break; // &NotGreaterTild -> &NotGreaterTilde
				case 4343: state = 4344; break; // &NotL -> &NotLe
				case 4353: state = 4354; break; // &NotLeftTriangl -> &NotLeftTriangle
				case 4371: state = 4372; break; // &NotLessGr -> &NotLessGre
				case 4374: state = 4375; break; // &NotLessGreat -> &NotLessGreate
				case 4377: state = 4378; break; // &NotLessL -> &NotLessLe
				case 4394: state = 4395; break; // &NotLessTild -> &NotLessTilde
				case 4396: state = 4397; break; // &NotN -> &NotNe
				case 4399: state = 4400; break; // &NotNest -> &NotNeste
				case 4403: state = 4404; break; // &NotNestedGr -> &NotNestedGre
				case 4406: state = 4407; break; // &NotNestedGreat -> &NotNestedGreate
				case 4410: state = 4411; break; // &NotNestedGreaterGr -> &NotNestedGreaterGre
				case 4413: state = 4414; break; // &NotNestedGreaterGreat -> &NotNestedGreaterGreate
				case 4416: state = 4417; break; // &NotNestedL -> &NotNestedLe
				case 4420: state = 4421; break; // &NotNestedLessL -> &NotNestedLessLe
				case 4431: state = 4432; break; // &NotPr -> &NotPre
				case 4433: state = 4434; break; // &NotPrec -> &NotPrece
				case 4435: state = 4436; break; // &NotPreced -> &NotPrecede
				case 4453: state = 4454; break; // &NotR -> &NotRe
				case 4455: state = 4456; break; // &NotRev -> &NotReve
				case 4458: state = 4459; break; // &NotRevers -> &NotReverse
				case 4461: state = 4462; break; // &NotReverseEl -> &NotReverseEle
				case 4463: state = 4464; break; // &NotReverseElem -> &NotReverseEleme
				case 4477: state = 4478; break; // &NotRightTriangl -> &NotRightTriangle
				case 4491: state = 4492; break; // &NotSquar -> &NotSquare
				case 4496: state = 4497; break; // &NotSquareSubs -> &NotSquareSubse
				case 4504: state = 4505; break; // &NotSquareSup -> &NotSquareSupe
				case 4507: state = 4508; break; // &NotSquareSupers -> &NotSquareSuperse
				case 4517: state = 4518; break; // &NotSubs -> &NotSubse
				case 4526: state = 4527; break; // &NotSucc -> &NotSucce
				case 4527: state = 4528; break; // &NotSucce -> &NotSuccee
				case 4549: state = 4550; break; // &NotSucceedsTild -> &NotSucceedsTilde
				case 4551: state = 4552; break; // &NotSup -> &NotSupe
				case 4554: state = 4555; break; // &NotSupers -> &NotSuperse
				case 4565: state = 4566; break; // &NotTild -> &NotTilde
				case 4584: state = 4585; break; // &NotTildeTild -> &NotTildeTilde
				case 4586: state = 4587; break; // &NotV -> &NotVe
				case 4602: state = 4603; break; // &nparall -> &nparalle
				case 4613: state = 4617; break; // &npr -> &npre
				case 4615: state = 4616; break; // &nprcu -> &nprcue
				case 4618: state = 4619; break; // &nprec -> &nprece
				case 4651: state = 4652; break; // &nrtri -> &nrtrie
				case 4654: state = 4658; break; // &nsc -> &nsce
				case 4656: state = 4657; break; // &nsccu -> &nsccue
				case 4675: state = 4676; break; // &nshortparall -> &nshortparalle
				case 4679: state = 4680; break; // &nsim -> &nsime
				case 4691: state = 4692; break; // &nsqsub -> &nsqsube
				case 4693: state = 4694; break; // &nsqsup -> &nsqsupe
				case 4696: state = 4698; break; // &nsub -> &nsube
				case 4699: state = 4700; break; // &nsubs -> &nsubse
				case 4701: state = 4702; break; // &nsubset -> &nsubsete
				case 4706: state = 4707; break; // &nsucc -> &nsucce
				case 4709: state = 4711; break; // &nsup -> &nsupe
				case 4712: state = 4713; break; // &nsups -> &nsupse
				case 4714: state = 4715; break; // &nsupset -> &nsupsete
				case 4724: state = 4725; break; // &Ntild -> &Ntilde
				case 4728: state = 4729; break; // &ntild -> &ntilde
				case 4737: state = 4738; break; // &ntriangl -> &ntriangle
				case 4739: state = 4740; break; // &ntrianglel -> &ntrianglele
				case 4742: state = 4743; break; // &ntriangleleft -> &ntrianglelefte
				case 4749: state = 4750; break; // &ntriangleright -> &ntrianglerighte
				case 4754: state = 4755; break; // &num -> &nume
				case 4780: state = 4781; break; // &nvg -> &nvge
				case 4792: state = 4796; break; // &nvl -> &nvle
				case 4799: state = 4800; break; // &nvltri -> &nvltrie
				case 4807: state = 4808; break; // &nvrtri -> &nvrtrie
				case 4823: state = 4824; break; // &nwn -> &nwne
				case 4831: state = 4832; break; // &Oacut -> &Oacute
				case 4833: state = 4876; break; // &o -> &oe
				case 4837: state = 4838; break; // &oacut -> &oacute
				case 4893: state = 4894; break; // &Ograv -> &Ograve
				case 4897: state = 4898; break; // &ograv -> &ograve
				case 4920: state = 4921; break; // &olin -> &oline
				case 4923: state = 4931; break; // &Om -> &Ome
				case 4927: state = 4934; break; // &om -> &ome
				case 4957: state = 4984; break; // &op -> &ope
				case 4960: state = 4961; break; // &Op -> &Ope
				case 4972: state = 4973; break; // &OpenCurlyDoubl -> &OpenCurlyDouble
				case 4977: state = 4978; break; // &OpenCurlyDoubleQuot -> &OpenCurlyDoubleQuote
				case 4982: state = 4983; break; // &OpenCurlyQuot -> &OpenCurlyQuote
				case 4995: state = 4996; break; // &ord -> &orde
				case 5011: state = 5012; break; // &orslop -> &orslope
				case 5034: state = 5035; break; // &Otild -> &Otilde
				case 5039: state = 5040; break; // &otild -> &otilde
				case 5041: state = 5042; break; // &Otim -> &Otime
				case 5044: state = 5045; break; // &otim -> &otime
				case 5059: state = 5060; break; // &Ov -> &Ove
				case 5067: state = 5068; break; // &OverBrac -> &OverBrace
				case 5069: state = 5070; break; // &OverBrack -> &OverBracke
				case 5074: state = 5075; break; // &OverPar -> &OverPare
				case 5078: state = 5079; break; // &OverParenth -> &OverParenthe
				case 5083: state = 5108; break; // &p -> &pe
				case 5088: state = 5089; break; // &parall -> &paralle
				case 5120: state = 5121; break; // &pert -> &perte
				case 5138: state = 5139; break; // &phon -> &phone
				case 5159: state = 5171; break; // &plus -> &pluse
				case 5194: state = 5195; break; // &Poincar -> &Poincare
				case 5199: state = 5200; break; // &Poincareplan -> &Poincareplane
				case 5215: state = 5238; break; // &Pr -> &Pre
				case 5216: state = 5223; break; // &pr -> &pre
				case 5220: state = 5221; break; // &prcu -> &prcue
				case 5224: state = 5264; break; // &prec -> &prece
				case 5235: state = 5236; break; // &preccurly -> &preccurlye
				case 5239: state = 5240; break; // &Prec -> &Prece
				case 5241: state = 5242; break; // &Preced -> &Precede
				case 5262: state = 5263; break; // &PrecedesTild -> &PrecedesTilde
				case 5266: state = 5273; break; // &precn -> &precne
				case 5283: state = 5284; break; // &Prim -> &Prime
				case 5286: state = 5287; break; // &prim -> &prime
				case 5310: state = 5311; break; // &proflin -> &profline
				case 5332: state = 5333; break; // &prur -> &prure
				case 5366: state = 5367; break; // &qprim -> &qprime
				case 5374: state = 5387; break; // &qu -> &que
				case 5376: state = 5377; break; // &quat -> &quate
				case 5389: state = 5390; break; // &quest -> &queste
				case 5397: state = 5541; break; // &r -> &re
				case 5402: state = 5417; break; // &ra -> &rae
				case 5403: state = 5404; break; // &rac -> &race
				case 5405: state = 5540; break; // &R -> &Re
				case 5409: state = 5410; break; // &Racut -> &Racute
				case 5412: state = 5413; break; // &racut -> &racute
				case 5426: state = 5428; break; // &rang -> &range
				case 5429: state = 5430; break; // &rangl -> &rangle
				case 5493: state = 5494; break; // &rbrac -> &rbrace
				case 5496: state = 5497; break; // &rbrk -> &rbrke
				case 5502: state = 5512; break; // &Rc -> &Rce
				case 5507: state = 5516; break; // &rc -> &rce
				case 5545: state = 5546; break; // &realin -> &realine
				case 5557: state = 5558; break; // &Rev -> &Reve
				case 5560: state = 5561; break; // &Revers -> &Reverse
				case 5563: state = 5564; break; // &ReverseEl -> &ReverseEle
				case 5565: state = 5566; break; // &ReverseElem -> &ReverseEleme
				case 5624: state = 5625; break; // &RightAngl -> &RightAngle
				case 5630: state = 5631; break; // &RightAngleBrack -> &RightAngleBracke
				case 5654: state = 5655; break; // &RightArrowL -> &RightArrowLe
				case 5667: state = 5668; break; // &RightC -> &RightCe
				case 5678: state = 5679; break; // &RightDoubl -> &RightDouble
				case 5684: state = 5685; break; // &RightDoubleBrack -> &RightDoubleBracke
				case 5689: state = 5690; break; // &RightDownT -> &RightDownTe
				case 5690: state = 5691; break; // &RightDownTe -> &RightDownTee
				case 5692: state = 5693; break; // &RightDownTeeV -> &RightDownTeeVe
				case 5698: state = 5699; break; // &RightDownV -> &RightDownVe
				case 5725: state = 5726; break; // &rightl -> &rightle
				case 5764: state = 5765; break; // &RightT -> &RightTe
				case 5765: state = 5766; break; // &RightTe -> &RightTee
				case 5772: state = 5773; break; // &RightTeeV -> &RightTeeVe
				case 5780: state = 5781; break; // &rightthr -> &rightthre
				case 5781: state = 5782; break; // &rightthre -> &rightthree
				case 5785: state = 5786; break; // &rightthreetim -> &rightthreetime
				case 5793: state = 5794; break; // &RightTriangl -> &RightTriangle
				case 5809: state = 5810; break; // &RightUpDownV -> &RightUpDownVe
				case 5815: state = 5816; break; // &RightUpT -> &RightUpTe
				case 5816: state = 5817; break; // &RightUpTe -> &RightUpTee
				case 5818: state = 5819; break; // &RightUpTeeV -> &RightUpTeeVe
				case 5824: state = 5825; break; // &RightUpV -> &RightUpVe
				case 5833: state = 5834; break; // &RightV -> &RightVe
				case 5851: state = 5852; break; // &risingdots -> &risingdotse
				case 5869: state = 5870; break; // &rmoustach -> &rmoustache
				case 5896: state = 5897; break; // &rotim -> &rotime
				case 5906: state = 5907; break; // &RoundImpli -> &RoundImplie
				case 5953: state = 5954; break; // &rthr -> &rthre
				case 5954: state = 5955; break; // &rthre -> &rthree
				case 5957: state = 5958; break; // &rtim -> &rtime
				case 5961: state = 5962; break; // &rtri -> &rtrie
				case 5969: state = 5970; break; // &Rul -> &Rule
				case 5971: state = 5972; break; // &RuleD -> &RuleDe
				case 5975: state = 5976; break; // &RuleDelay -> &RuleDelaye
				case 5989: state = 5990; break; // &Sacut -> &Sacute
				case 5991: state = 6053; break; // &s -> &se
				case 5995: state = 5996; break; // &sacut -> &sacute
				case 6001: state = 6017; break; // &Sc -> &Sce
				case 6002: state = 6016; break; // &sc -> &sce
				case 6013: state = 6014; break; // &sccu -> &sccue
				case 6050: state = 6052; break; // &sdot -> &sdote
				case 6117: state = 6118; break; // &ShortL -> &ShortLe
				case 6137: state = 6138; break; // &shortparall -> &shortparalle
				case 6168: state = 6172; break; // &sim -> &sime
				case 6178: state = 6179; break; // &simn -> &simne
				case 6200: state = 6201; break; // &SmallCircl -> &SmallCircle
				case 6202: state = 6217; break; // &sm -> &sme
				case 6206: state = 6207; break; // &smalls -> &smallse
				case 6225: state = 6226; break; // &smil -> &smile
				case 6227: state = 6228; break; // &smt -> &smte
				case 6251: state = 6252; break; // &spad -> &spade
				case 6271: state = 6272; break; // &sqsub -> &sqsube
				case 6273: state = 6274; break; // &sqsubs -> &sqsubse
				case 6275: state = 6276; break; // &sqsubset -> &sqsubsete
				case 6278: state = 6279; break; // &sqsup -> &sqsupe
				case 6280: state = 6281; break; // &sqsups -> &sqsupse
				case 6282: state = 6283; break; // &sqsupset -> &sqsupsete
				case 6288: state = 6289; break; // &Squar -> &Square
				case 6291: state = 6292; break; // &squar -> &square
				case 6295: state = 6296; break; // &SquareInt -> &SquareInte
				case 6298: state = 6299; break; // &SquareInters -> &SquareInterse
				case 6308: state = 6309; break; // &SquareSubs -> &SquareSubse
				case 6316: state = 6317; break; // &SquareSup -> &SquareSupe
				case 6319: state = 6320; break; // &SquareSupers -> &SquareSuperse
				case 6341: state = 6344; break; // &ss -> &sse
				case 6350: state = 6351; break; // &ssmil -> &ssmile
				case 6368: state = 6369; break; // &straight -> &straighte
				case 6384: state = 6389; break; // &sub -> &sube
				case 6397: state = 6399; break; // &subn -> &subne
				case 6408: state = 6409; break; // &Subs -> &Subse
				case 6411: state = 6412; break; // &subs -> &subse
				case 6413: state = 6414; break; // &subset -> &subsete
				case 6422: state = 6423; break; // &subsetn -> &subsetne
				case 6432: state = 6472; break; // &succ -> &succe
				case 6443: state = 6444; break; // &succcurly -> &succcurlye
				case 6447: state = 6448; break; // &Succ -> &Succe
				case 6448: state = 6449; break; // &Succe -> &Succee
				case 6470: state = 6471; break; // &SucceedsTild -> &SucceedsTilde
				case 6474: state = 6481; break; // &succn -> &succne
				case 6499: state = 6515; break; // &Sup -> &Supe
				case 6500: state = 6511; break; // &sup -> &supe
				case 6517: state = 6518; break; // &Supers -> &Superse
				case 6539: state = 6541; break; // &supn -> &supne
				case 6546: state = 6547; break; // &Sups -> &Supse
				case 6549: state = 6550; break; // &sups -> &supse
				case 6551: state = 6552; break; // &supset -> &supsete
				case 6555: state = 6556; break; // &supsetn -> &supsetne
				case 6586: state = 6620; break; // &t -> &te
				case 6589: state = 6590; break; // &targ -> &targe
				case 6597: state = 6607; break; // &Tc -> &Tce
				case 6602: state = 6611; break; // &tc -> &tce
				case 6622: state = 6623; break; // &telr -> &telre
				case 6629: state = 6630; break; // &th -> &the
				case 6631: state = 6632; break; // &ther -> &there
				case 6634: state = 6635; break; // &Th -> &The
				case 6636: state = 6637; break; // &Ther -> &There
				case 6640: state = 6641; break; // &Therefor -> &Therefore
				case 6644: state = 6645; break; // &therefor -> &therefore
				case 6672: state = 6673; break; // &ThickSpac -> &ThickSpace
				case 6681: state = 6682; break; // &ThinSpac -> &ThinSpace
				case 6698: state = 6699; break; // &Tild -> &Tilde
				case 6702: state = 6703; break; // &tild -> &tilde
				case 6721: state = 6722; break; // &TildeTild -> &TildeTilde
				case 6723: state = 6724; break; // &tim -> &time
				case 6732: state = 6733; break; // &to -> &toe
				case 6754: state = 6755; break; // &tprim -> &tprime
				case 6762: state = 6763; break; // &trad -> &trade
				case 6764: state = 6791; break; // &tri -> &trie
				case 6768: state = 6769; break; // &triangl -> &triangle
				case 6774: state = 6775; break; // &trianglel -> &trianglele
				case 6777: state = 6778; break; // &triangleleft -> &trianglelefte
				case 6785: state = 6786; break; // &triangleright -> &trianglerighte
				case 6800: state = 6801; break; // &Tripl -> &Triple
				case 6813: state = 6814; break; // &tritim -> &tritime
				case 6815: state = 6816; break; // &trp -> &trpe
				case 6850: state = 6851; break; // &twoh -> &twohe
				case 6854: state = 6855; break; // &twoheadl -> &twoheadle
				case 6877: state = 6878; break; // &Uacut -> &Uacute
				case 6883: state = 6884; break; // &uacut -> &uacute
				case 6897: state = 6904; break; // &Ubr -> &Ubre
				case 6901: state = 6907; break; // &ubr -> &ubre
				case 6905: state = 6906; break; // &Ubrev -> &Ubreve
				case 6908: state = 6909; break; // &ubrev -> &ubreve
				case 6947: state = 6948; break; // &Ugrav -> &Ugrave
				case 6952: state = 6953; break; // &ugrav -> &ugrave
				case 6969: state = 6970; break; // &ulcorn -> &ulcorne
				case 6988: state = 6989; break; // &Und -> &Unde
				case 6996: state = 6997; break; // &UnderBrac -> &UnderBrace
				case 6998: state = 6999; break; // &UnderBrack -> &UnderBracke
				case 7003: state = 7004; break; // &UnderPar -> &UnderPare
				case 7007: state = 7008; break; // &UnderParenth -> &UnderParenthe
				case 7105: state = 7106; break; // &upharpoonl -> &upharpoonle
				case 7117: state = 7118; break; // &Upp -> &Uppe
				case 7120: state = 7121; break; // &UpperL -> &UpperLe
				case 7150: state = 7151; break; // &UpT -> &UpTe
				case 7151: state = 7152; break; // &UpTe -> &UpTee
				case 7170: state = 7171; break; // &urcorn -> &urcorne
				case 7199: state = 7200; break; // &Utild -> &Utilde
				case 7203: state = 7204; break; // &utild -> &utilde
				case 7221: state = 7222; break; // &uwangl -> &uwangle
				case 7223: state = 7338; break; // &v -> &ve
				case 7229: state = 7230; break; // &var -> &vare
				case 7271: state = 7272; break; // &varsubs -> &varsubse
				case 7274: state = 7275; break; // &varsubsetn -> &varsubsetne
				case 7279: state = 7280; break; // &varsups -> &varsupse
				case 7282: state = 7283; break; // &varsupsetn -> &varsupsetne
				case 7287: state = 7288; break; // &varth -> &varthe
				case 7296: state = 7297; break; // &vartriangl -> &vartriangle
				case 7298: state = 7299; break; // &vartrianglel -> &vartrianglele
				case 7307: state = 7336; break; // &V -> &Ve
				case 7336: state = 7337; break; // &Ve -> &Vee
				case 7338: state = 7339; break; // &ve -> &vee
				case 7339: state = 7343; break; // &vee -> &veee
				case 7368: state = 7369; break; // &VerticalLin -> &VerticalLine
				case 7370: state = 7371; break; // &VerticalS -> &VerticalSe
				case 7382: state = 7383; break; // &VerticalTild -> &VerticalTilde
				case 7392: state = 7393; break; // &VeryThinSpac -> &VeryThinSpace
				case 7429: state = 7431; break; // &vsubn -> &vsubne
				case 7433: state = 7435; break; // &vsupn -> &vsupne
				case 7447: state = 7462; break; // &W -> &We
				case 7452: state = 7457; break; // &w -> &we
				case 7464: state = 7465; break; // &Wedg -> &Wedge
				case 7466: state = 7467; break; // &wedg -> &wedge
				case 7469: state = 7470; break; // &wei -> &weie
				case 7484: state = 7485; break; // &wr -> &wre
				case 7549: state = 7550; break; // &xotim -> &xotime
				case 7576: state = 7577; break; // &xv -> &xve
				case 7577: state = 7578; break; // &xve -> &xvee
				case 7579: state = 7580; break; // &xw -> &xwe
				case 7582: state = 7583; break; // &xwedg -> &xwedge
				case 7588: state = 7589; break; // &Yacut -> &Yacute
				case 7590: state = 7610; break; // &y -> &ye
				case 7594: state = 7595; break; // &yacut -> &yacute
				case 7645: state = 7680; break; // &Z -> &Ze
				case 7649: state = 7650; break; // &Zacut -> &Zacute
				case 7651: state = 7675; break; // &z -> &ze
				case 7655: state = 7656; break; // &zacut -> &zacute
				case 7675: state = 7676; break; // &ze -> &zee
				case 7691: state = 7692; break; // &ZeroWidthSpac -> &ZeroWidthSpace
				default: return false;
				}
				break;
			case 'f':
				switch (state) {
				case 0: state = 1964; break; // & -> &f
				case 1: state = 47; break; // &A -> &Af
				case 7: state = 46; break; // &a -> &af
				case 61: state = 62; break; // &ale -> &alef
				case 109: state = 115; break; // &angmsda -> &angmsdaf
				case 139: state = 140; break; // &Aop -> &Aopf
				case 141: state = 142; break; // &aop -> &aopf
				case 222: state = 331; break; // &b -> &bf
				case 247: state = 329; break; // &B -> &Bf
				case 426: state = 427; break; // &blacktrianglele -> &blacktrianglelef
				case 457: state = 458; break; // &Bop -> &Bopf
				case 460: state = 461; break; // &bop -> &bopf
				case 583: state = 712; break; // &C -> &Cf
				case 589: state = 714; break; // &c -> &cf
				case 618: state = 619; break; // &CapitalDi -> &CapitalDif
				case 619: state = 620; break; // &CapitalDif -> &CapitalDiff
				case 733: state = 791; break; // &cir -> &cirf
				case 745: state = 746; break; // &circlearrowle -> &circlearrowlef
				case 871: state = 872; break; // &comp -> &compf
				case 913: state = 914; break; // &Cop -> &Copf
				case 915: state = 916; break; // &cop -> &copf
				case 1062: state = 1063; break; // &curvearrowle -> &curvearrowlef
				case 1091: state = 1180; break; // &D -> &Df
				case 1097: state = 1175; break; // &d -> &df
				case 1191: state = 1244; break; // &Di -> &Dif
				case 1244: state = 1245; break; // &Dif -> &Diff
				case 1297: state = 1298; break; // &Dop -> &Dopf
				case 1299: state = 1300; break; // &dop -> &dopf
				case 1373: state = 1374; break; // &DoubleLe -> &DoubleLef
				case 1398: state = 1399; break; // &DoubleLongLe -> &DoubleLongLef
				case 1518: state = 1519; break; // &downharpoonle -> &downharpoonlef
				case 1527: state = 1528; break; // &DownLe -> &DownLef
				case 1629: state = 1630; break; // &dtri -> &dtrif
				case 1656: state = 1711; break; // &E -> &Ef
				case 1662: state = 1707; break; // &e -> &ef
				case 1809: state = 1810; break; // &Eop -> &Eopf
				case 1811: state = 1812; break; // &eop -> &eopf
				case 1964: state = 1987; break; // &f -> &ff
				case 1977: state = 1998; break; // &F -> &Ff
				case 2050: state = 2051; break; // &fno -> &fnof
				case 2053: state = 2054; break; // &Fop -> &Fopf
				case 2056: state = 2057; break; // &fop -> &fopf
				case 2074: state = 2075; break; // &Fouriertr -> &Fouriertrf
				case 2118: state = 2189; break; // &g -> &gf
				case 2124: state = 2187; break; // &G -> &Gf
				case 2223: state = 2224; break; // &Gop -> &Gopf
				case 2226: state = 2227; break; // &gop -> &gopf
				case 2351: state = 2413; break; // &H -> &Hf
				case 2356: state = 2415; break; // &h -> &hf
				case 2362: state = 2363; break; // &hal -> &half
				case 2451: state = 2452; break; // &hookle -> &hooklef
				case 2470: state = 2471; break; // &Hop -> &Hopf
				case 2472: state = 2473; break; // &hop -> &hopf
				case 2533: state = 2569; break; // &I -> &If
				case 2539: state = 2567; break; // &i -> &if
				case 2567: state = 2568; break; // &if -> &iff
				case 2589: state = 2590; break; // &iin -> &iinf
				case 2631: state = 2632; break; // &imo -> &imof
				case 2641: state = 2646; break; // &in -> &inf
				case 2723: state = 2724; break; // &Iop -> &Iopf
				case 2725: state = 2726; break; // &iop -> &iopf
				case 2777: state = 2789; break; // &J -> &Jf
				case 2782: state = 2791; break; // &j -> &jf
				case 2798: state = 2799; break; // &Jop -> &Jopf
				case 2801: state = 2802; break; // &jop -> &jopf
				case 2825: state = 2848; break; // &K -> &Kf
				case 2830: state = 2850; break; // &k -> &kf
				case 2870: state = 2871; break; // &Kop -> &Kopf
				case 2873: state = 2874; break; // &kop -> &kopf
				case 2881: state = 3376; break; // &l -> &lf
				case 2886: state = 3385; break; // &L -> &Lf
				case 2929: state = 2930; break; // &Laplacetr -> &Laplacetrf
				case 2939: state = 2943; break; // &larr -> &larrf
				case 2940: state = 2941; break; // &larrb -> &larrbf
				case 3032: state = 3057; break; // &le -> &lef
				case 3033: state = 3034; break; // &Le -> &Lef
				case 3140: state = 3141; break; // &leftle -> &leftlef
				case 3419: state = 3420; break; // &Lle -> &Llef
				case 3479: state = 3480; break; // &LongLe -> &LongLef
				case 3488: state = 3489; break; // &Longle -> &Longlef
				case 3499: state = 3500; break; // &longle -> &longlef
				case 3581: state = 3582; break; // &looparrowle -> &looparrowlef
				case 3589: state = 3594; break; // &lop -> &lopf
				case 3592: state = 3593; break; // &Lop -> &Lopf
				case 3614: state = 3615; break; // &LowerLe -> &LowerLef
				case 3632: state = 3637; break; // &loz -> &lozf
				case 3718: state = 3720; break; // &ltri -> &ltrif
				case 3745: state = 3823; break; // &m -> &mf
				case 3755: state = 3821; break; // &M -> &Mf
				case 3767: state = 3768; break; // &mapstole -> &mapstolef
				case 3819: state = 3820; break; // &Mellintr -> &Mellintrf
				case 3871: state = 3872; break; // &Mop -> &Mopf
				case 3873: state = 3874; break; // &mop -> &mopf
				case 3897: state = 4079; break; // &n -> &nf
				case 3902: state = 4077; break; // &N -> &Nf
				case 4131: state = 4141; break; // &nle -> &nlef
				case 4133: state = 4134; break; // &nLe -> &nLef
				case 4210: state = 4211; break; // &Nop -> &Nopf
				case 4213: state = 4214; break; // &nop -> &nopf
				case 4344: state = 4345; break; // &NotLe -> &NotLef
				case 4740: state = 4741; break; // &ntrianglele -> &ntrianglelef
				case 4788: state = 4789; break; // &nvin -> &nvinf
				case 4827: state = 4884; break; // &O -> &Of
				case 4833: state = 4880; break; // &o -> &of
				case 4952: state = 4953; break; // &Oop -> &Oopf
				case 4955: state = 4956; break; // &oop -> &oopf
				case 4995: state = 5000; break; // &ord -> &ordf
				case 4998: state = 4999; break; // &ordero -> &orderof
				case 5004: state = 5005; break; // &origo -> &origof
				case 5083: state = 5126; break; // &p -> &pf
				case 5096: state = 5124; break; // &P -> &Pf
				case 5144: state = 5145; break; // &pitch -> &pitchf
				case 5208: state = 5209; break; // &Pop -> &Popf
				case 5210: state = 5211; break; // &pop -> &popf
				case 5296: state = 5303; break; // &pro -> &prof
				case 5314: state = 5315; break; // &profsur -> &profsurf
				case 5348: state = 5349; break; // &Q -> &Qf
				case 5351: state = 5352; break; // &q -> &qf
				case 5358: state = 5359; break; // &Qop -> &Qopf
				case 5361: state = 5362; break; // &qop -> &qopf
				case 5397: state = 5592; break; // &r -> &rf
				case 5405: state = 5601; break; // &R -> &Rf
				case 5439: state = 5446; break; // &rarr -> &rarrf
				case 5442: state = 5443; break; // &rarrb -> &rarrbf
				case 5655: state = 5656; break; // &RightArrowLe -> &RightArrowLef
				case 5726: state = 5727; break; // &rightle -> &rightlef
				case 5884: state = 5890; break; // &rop -> &ropf
				case 5888: state = 5889; break; // &Rop -> &Ropf
				case 5961: state = 5963; break; // &rtri -> &rtrif
				case 5985: state = 6081; break; // &S -> &Sf
				case 5991: state = 6083; break; // &s -> &sf
				case 6118: state = 6119; break; // &ShortLe -> &ShortLef
				case 6165: state = 6166; break; // &sigma -> &sigmaf
				case 6235: state = 6236; break; // &so -> &sof
				case 6245: state = 6246; break; // &Sop -> &Sopf
				case 6247: state = 6248; break; // &sop -> &sopf
				case 6285: state = 6333; break; // &squ -> &squf
				case 6291: state = 6332; break; // &squar -> &squarf
				case 6354: state = 6355; break; // &sstar -> &sstarf
				case 6361: state = 6362; break; // &star -> &starf
				case 6583: state = 6625; break; // &T -> &Tf
				case 6586: state = 6627; break; // &t -> &tf
				case 6632: state = 6642; break; // &there -> &theref
				case 6637: state = 6638; break; // &There -> &Theref
				case 6735: state = 6745; break; // &top -> &topf
				case 6743: state = 6744; break; // &Top -> &Topf
				case 6775: state = 6776; break; // &trianglele -> &trianglelef
				case 6855: state = 6856; break; // &twoheadle -> &twoheadlef
				case 6873: state = 6941; break; // &U -> &Uf
				case 6879: state = 6936; break; // &u -> &uf
				case 7027: state = 7028; break; // &Uop -> &Uopf
				case 7029: state = 7030; break; // &uop -> &uopf
				case 7106: state = 7107; break; // &upharpoonle -> &upharpoonlef
				case 7121: state = 7122; break; // &UpperLe -> &UpperLef
				case 7206: state = 7207; break; // &utri -> &utrif
				case 7223: state = 7396; break; // &v -> &vf
				case 7299: state = 7300; break; // &vartrianglele -> &vartrianglelef
				case 7307: state = 7394; break; // &V -> &Vf
				case 7408: state = 7409; break; // &Vop -> &Vopf
				case 7411: state = 7412; break; // &vop -> &vopf
				case 7447: state = 7473; break; // &W -> &Wf
				case 7452: state = 7475; break; // &w -> &wf
				case 7478: state = 7479; break; // &Wop -> &Wopf
				case 7481: state = 7482; break; // &wop -> &wopf
				case 7495: state = 7511; break; // &x -> &xf
				case 7508: state = 7509; break; // &X -> &Xf
				case 7540: state = 7541; break; // &Xop -> &Xopf
				case 7542: state = 7543; break; // &xop -> &xopf
				case 7584: state = 7612; break; // &Y -> &Yf
				case 7590: state = 7614; break; // &y -> &yf
				case 7623: state = 7624; break; // &Yop -> &Yopf
				case 7626: state = 7627; break; // &yop -> &yopf
				case 7645: state = 7697; break; // &Z -> &Zf
				case 7651: state = 7699; break; // &z -> &zf
				case 7678: state = 7679; break; // &zeetr -> &zeetrf
				case 7714: state = 7715; break; // &Zop -> &Zopf
				case 7717: state = 7718; break; // &zop -> &zopf
				default: return false;
				}
				break;
			case 'g':
				switch (state) {
				case 0: state = 2118; break; // & -> &g
				case 1: state = 50; break; // &A -> &Ag
				case 7: state = 55; break; // &a -> &ag
				case 40: state = 41; break; // &AEli -> &AElig
				case 44: state = 45; break; // &aeli -> &aelig
				case 83: state = 84; break; // &amal -> &amalg
				case 90: state = 102; break; // &an -> &ang
				case 109: state = 116; break; // &angmsda -> &angmsdag
				case 131: state = 132; break; // &Ao -> &Aog
				case 135: state = 136; break; // &ao -> &aog
				case 174: state = 175; break; // &Arin -> &Aring
				case 178: state = 179; break; // &arin -> &aring
				case 187: state = 188; break; // &Assi -> &Assig
				case 228: state = 229; break; // &backcon -> &backcong
				case 267: state = 268; break; // &barwed -> &barwedg
				case 279: state = 280; break; // &bcon -> &bcong
				case 333: state = 334; break; // &bi -> &big
				case 368: state = 369; break; // &bigtrian -> &bigtriang
				case 388: state = 389; break; // &bigwed -> &bigwedg
				case 404: state = 405; break; // &blacklozen -> &blacklozeng
				case 417: state = 418; break; // &blacktrian -> &blacktriang
				case 430: state = 431; break; // &blacktriangleri -> &blacktrianglerig
				case 749: state = 750; break; // &circlearrowri -> &circlearrowrig
				case 821: state = 822; break; // &ClockwiseContourInte -> &ClockwiseContourInteg
				case 883: state = 884; break; // &con -> &cong
				case 888: state = 889; break; // &Con -> &Cong
				case 908: state = 909; break; // &ContourInte -> &ContourInteg
				case 956: state = 957; break; // &CounterClockwiseContourInte -> &CounterClockwiseContourInteg
				case 1048: state = 1049; break; // &curlywed -> &curlywedg
				case 1066: state = 1067; break; // &curvearrowri -> &curvearrowrig
				case 1092: state = 1093; break; // &Da -> &Dag
				case 1093: state = 1094; break; // &Dag -> &Dagg
				case 1098: state = 1099; break; // &da -> &dag
				case 1099: state = 1100; break; // &dag -> &dagg
				case 1143: state = 1144; break; // &dda -> &ddag
				case 1144: state = 1145; break; // &ddag -> &ddagg
				case 1161: state = 1162; break; // &de -> &deg
				case 1228: state = 1255; break; // &di -> &dig
				case 1340: state = 1341; break; // &doublebarwed -> &doublebarwedg
				case 1357: state = 1358; break; // &DoubleContourInte -> &DoubleContourInteg
				case 1382: state = 1383; break; // &DoubleLeftRi -> &DoubleLeftRig
				case 1395: state = 1396; break; // &DoubleLon -> &DoubleLong
				case 1407: state = 1408; break; // &DoubleLongLeftRi -> &DoubleLongLeftRig
				case 1417: state = 1418; break; // &DoubleLongRi -> &DoubleLongRig
				case 1427: state = 1428; break; // &DoubleRi -> &DoubleRig
				case 1522: state = 1523; break; // &downharpoonri -> &downharpoonrig
				case 1531: state = 1532; break; // &DownLeftRi -> &DownLeftRig
				case 1560: state = 1561; break; // &DownRi -> &DownRig
				case 1640: state = 1641; break; // &dwan -> &dwang
				case 1650: state = 1651; break; // &dzi -> &dzig
				case 1656: state = 1715; break; // &E -> &Eg
				case 1662: state = 1714; break; // &e -> &eg
				case 1797: state = 1798; break; // &en -> &eng
				case 1801: state = 1802; break; // &Eo -> &Eog
				case 1805: state = 1806; break; // &eo -> &eog
				case 1848: state = 1849; break; // &eqslant -> &eqslantg
				case 1969: state = 1970; break; // &fallin -> &falling
				case 1990: state = 1991; break; // &ffili -> &ffilig
				case 1993: state = 1994; break; // &ffli -> &fflig
				case 1996: state = 1997; break; // &fflli -> &ffllig
				case 2003: state = 2004; break; // &fili -> &filig
				case 2038: state = 2039; break; // &fjli -> &fjlig
				case 2044: state = 2045; break; // &flli -> &fllig
				case 2118: state = 2192; break; // &g -> &gg
				case 2124: state = 2191; break; // &G -> &Gg
				case 2192: state = 2193; break; // &gg -> &ggg
				case 2460: state = 2461; break; // &hookri -> &hookrig
				case 2533: state = 2572; break; // &I -> &Ig
				case 2539: state = 2577; break; // &i -> &ig
				case 2598: state = 2599; break; // &IJli -> &IJlig
				case 2602: state = 2603; break; // &ijli -> &ijlig
				case 2605: state = 2614; break; // &Ima -> &Imag
				case 2609: state = 2612; break; // &ima -> &imag
				case 2662: state = 2663; break; // &inte -> &integ
				case 2667: state = 2668; break; // &Inte -> &Integ
				case 2713: state = 2720; break; // &io -> &iog
				case 2716: state = 2717; break; // &Io -> &Iog
				case 2830: state = 2852; break; // &k -> &kg
				case 2881: state = 3388; break; // &l -> &lg
				case 2892: state = 2903; break; // &la -> &lag
				case 2915: state = 2916; break; // &Lan -> &Lang
				case 2917: state = 2918; break; // &lan -> &lang
				case 3031: state = 3282; break; // &lE -> &lEg
				case 3032: state = 3283; break; // &le -> &leg
				case 3037: state = 3038; break; // &LeftAn -> &LeftAng
				case 3068: state = 3069; break; // &LeftArrowRi -> &LeftArrowRig
				case 3086: state = 3087; break; // &LeftCeilin -> &LeftCeiling
				case 3150: state = 3151; break; // &LeftRi -> &LeftRig
				case 3160: state = 3161; break; // &Leftri -> &Leftrig
				case 3170: state = 3171; break; // &leftri -> &leftrig
				case 3191: state = 3192; break; // &leftrightsqui -> &leftrightsquig
				case 3231: state = 3232; break; // &LeftTrian -> &LeftTriang
				case 3291: state = 3299; break; // &les -> &lesg
				case 3302: state = 3351; break; // &less -> &lessg
				case 3313: state = 3314; break; // &lesseq -> &lesseqg
				case 3317: state = 3318; break; // &lesseqq -> &lesseqqg
				case 3468: state = 3469; break; // &loan -> &loang
				case 3476: state = 3477; break; // &Lon -> &Long
				case 3496: state = 3497; break; // &lon -> &long
				case 3508: state = 3509; break; // &LongLeftRi -> &LongLeftRig
				case 3518: state = 3519; break; // &Longleftri -> &Longleftrig
				case 3528: state = 3529; break; // &longleftri -> &longleftrig
				case 3544: state = 3545; break; // &LongRi -> &LongRig
				case 3554: state = 3555; break; // &Longri -> &Longrig
				case 3564: state = 3565; break; // &longri -> &longrig
				case 3585: state = 3586; break; // &looparrowri -> &looparrowrig
				case 3623: state = 3624; break; // &LowerRi -> &LowerRig
				case 3634: state = 3635; break; // &lozen -> &lozeng
				case 3674: state = 3676; break; // &lsim -> &lsimg
				case 3800: state = 3801; break; // &measuredan -> &measuredang
				case 3897: state = 4081; break; // &n -> &ng
				case 3912: state = 3913; break; // &nan -> &nang
				case 3957: state = 3958; break; // &ncon -> &ncong
				case 3984: state = 3985; break; // &Ne -> &Neg
				case 4092: state = 4093; break; // &nG -> &nGg
				case 4149: state = 4150; break; // &nLeftri -> &nLeftrig
				case 4159: state = 4160; break; // &nleftri -> &nleftrig
				case 4203: state = 4204; break; // &NonBreakin -> &NonBreaking
				case 4219: state = 4220; break; // &NotCon -> &NotCong
				case 4351: state = 4352; break; // &NotLeftTrian -> &NotLeftTriang
				case 4467: state = 4468; break; // &NotRi -> &NotRig
				case 4475: state = 4476; break; // &NotRightTrian -> &NotRightTriang
				case 4631: state = 4632; break; // &nRi -> &nRig
				case 4640: state = 4641; break; // &nri -> &nrig
				case 4718: state = 4719; break; // &nt -> &ntg
				case 4730: state = 4731; break; // &ntl -> &ntlg
				case 4735: state = 4736; break; // &ntrian -> &ntriang
				case 4746: state = 4747; break; // &ntriangleri -> &ntrianglerig
				case 4760: state = 4780; break; // &nv -> &nvg
				case 4827: state = 4890; break; // &O -> &Og
				case 4833: state = 4887; break; // &o -> &og
				case 4874: state = 4875; break; // &OEli -> &OElig
				case 4878: state = 4879; break; // &oeli -> &oelig
				case 4931: state = 4932; break; // &Ome -> &Omeg
				case 4934: state = 4935; break; // &ome -> &omeg
				case 5002: state = 5003; break; // &ori -> &orig
				case 5423: state = 5424; break; // &Ran -> &Rang
				case 5425: state = 5426; break; // &ran -> &rang
				case 5541: state = 5556; break; // &re -> &reg
				case 5617: state = 5618; break; // &Ri -> &Rig
				case 5622: state = 5623; break; // &RightAn -> &RightAng
				case 5642: state = 5643; break; // &ri -> &rig
				case 5672: state = 5673; break; // &RightCeilin -> &RightCeiling
				case 5744: state = 5745; break; // &rightri -> &rightrig
				case 5757: state = 5758; break; // &rightsqui -> &rightsquig
				case 5791: state = 5792; break; // &RightTrian -> &RightTriang
				case 5842: state = 5843; break; // &rin -> &ring
				case 5846: state = 5847; break; // &risin -> &rising
				case 5877: state = 5878; break; // &roan -> &roang
				case 5911: state = 5912; break; // &rpar -> &rparg
				case 5925: state = 5926; break; // &Rri -> &Rrig
				case 6141: state = 6142; break; // &ShortRi -> &ShortRig
				case 6158: state = 6159; break; // &Si -> &Sig
				case 6162: state = 6163; break; // &si -> &sig
				case 6168: state = 6174; break; // &sim -> &simg
				case 6365: state = 6366; break; // &strai -> &straig
				case 6497: state = 6498; break; // &sun -> &sung
				case 6581: state = 6582; break; // &szli -> &szlig
				case 6588: state = 6589; break; // &tar -> &targ
				case 6766: state = 6767; break; // &trian -> &triang
				case 6782: state = 6783; break; // &triangleri -> &trianglerig
				case 6864: state = 6865; break; // &twoheadri -> &twoheadrig
				case 6873: state = 6944; break; // &U -> &Ug
				case 6879: state = 6949; break; // &u -> &ug
				case 7019: state = 7020; break; // &Uo -> &Uog
				case 7023: state = 7024; break; // &uo -> &uog
				case 7110: state = 7111; break; // &upharpoonri -> &upharpoonrig
				case 7130: state = 7131; break; // &UpperRi -> &UpperRig
				case 7178: state = 7179; break; // &Urin -> &Uring
				case 7181: state = 7182; break; // &urin -> &uring
				case 7219: state = 7220; break; // &uwan -> &uwang
				case 7225: state = 7226; break; // &van -> &vang
				case 7247: state = 7248; break; // &varnothin -> &varnothing
				case 7265: state = 7266; break; // &varsi -> &varsig
				case 7294: state = 7295; break; // &vartrian -> &vartriang
				case 7303: state = 7304; break; // &vartriangleri -> &vartrianglerig
				case 7442: state = 7443; break; // &vzi -> &vzig
				case 7445: state = 7446; break; // &vzigza -> &vzigzag
				case 7458: state = 7466; break; // &wed -> &wedg
				case 7463: state = 7464; break; // &Wed -> &Wedg
				case 7581: state = 7582; break; // &xwed -> &xwedg
				case 7707: state = 7708; break; // &zi -> &zig
				default: return false;
				}
				break;
			case 'h':
				switch (state) {
				case 0: state = 2356; break; // & -> &h
				case 66: state = 67; break; // &alep -> &aleph
				case 69: state = 70; break; // &Alp -> &Alph
				case 72: state = 73; break; // &alp -> &alph
				case 109: state = 117; break; // &angmsda -> &angmsdah
				case 124: state = 125; break; // &angsp -> &angsph
				case 254: state = 255; break; // &Backslas -> &Backslash
				case 322: state = 324; break; // &bet -> &beth
				case 431: state = 432; break; // &blacktrianglerig -> &blacktrianglerigh
				case 470: state = 485; break; // &box -> &boxh
				case 518: state = 521; break; // &boxV -> &boxVh
				case 519: state = 523; break; // &boxv -> &boxvh
				case 562: state = 564; break; // &bsol -> &bsolh
				case 583: state = 729; break; // &C -> &Ch
				case 589: state = 719; break; // &c -> &ch
				case 750: state = 751; break; // &circlearrowrig -> &circlearrowrigh
				case 763: state = 764; break; // &circleddas -> &circleddash
				case 1067: state = 1068; break; // &curvearrowrig -> &curvearrowrigh
				case 1097: state = 1186; break; // &d -> &dh
				case 1105: state = 1106; break; // &dalet -> &daleth
				case 1114: state = 1115; break; // &das -> &dash
				case 1116: state = 1117; break; // &Das -> &Dash
				case 1153: state = 1154; break; // &DDotra -> &DDotrah
				case 1177: state = 1178; break; // &dfis -> &dfish
				case 1383: state = 1384; break; // &DoubleLeftRig -> &DoubleLeftRigh
				case 1408: state = 1409; break; // &DoubleLongLeftRig -> &DoubleLongLeftRigh
				case 1418: state = 1419; break; // &DoubleLongRig -> &DoubleLongRigh
				case 1428: state = 1429; break; // &DoubleRig -> &DoubleRigh
				case 1479: state = 1510; break; // &down -> &downh
				case 1523: state = 1524; break; // &downharpoonrig -> &downharpoonrigh
				case 1532: state = 1533; break; // &DownLeftRig -> &DownLeftRigh
				case 1561: state = 1562; break; // &DownRig -> &DownRigh
				case 1631: state = 1635; break; // &du -> &duh
				case 1912: state = 1916; break; // &et -> &eth
				case 2445: state = 2446; break; // &homt -> &homth
				case 2461: state = 2462; break; // &hookrig -> &hookrigh
				case 2498: state = 2499; break; // &hslas -> &hslash
				case 2529: state = 2530; break; // &hyp -> &hyph
				case 2629: state = 2630; break; // &imat -> &imath
				case 2686: state = 2687; break; // &intlar -> &intlarh
				case 2795: state = 2796; break; // &jmat -> &jmath
				case 2830: state = 2860; break; // &k -> &kh
				case 2881: state = 3393; break; // &l -> &lh
				case 2939: state = 2945; break; // &larr -> &larrh
				case 3020: state = 3021; break; // &ldrd -> &ldrdh
				case 3025: state = 3026; break; // &ldrus -> &ldrush
				case 3029: state = 3030; break; // &lds -> &ldsh
				case 3058: state = 3126; break; // &left -> &lefth
				case 3069: state = 3070; break; // &LeftArrowRig -> &LeftArrowRigh
				case 3151: state = 3152; break; // &LeftRig -> &LeftRigh
				case 3161: state = 3162; break; // &Leftrig -> &Leftrigh
				case 3171: state = 3172; break; // &leftrig -> &leftrigh
				case 3173: state = 3180; break; // &leftright -> &leftrighth
				case 3218: state = 3219; break; // &leftt -> &leftth
				case 3378: state = 3379; break; // &lfis -> &lfish
				case 3409: state = 3427; break; // &ll -> &llh
				case 3449: state = 3450; break; // &lmoustac -> &lmoustach
				case 3509: state = 3510; break; // &LongLeftRig -> &LongLeftRigh
				case 3519: state = 3520; break; // &Longleftrig -> &Longleftrigh
				case 3529: state = 3530; break; // &longleftrig -> &longleftrigh
				case 3545: state = 3546; break; // &LongRig -> &LongRigh
				case 3555: state = 3556; break; // &Longrig -> &Longrigh
				case 3565: state = 3566; break; // &longrig -> &longrigh
				case 3586: state = 3587; break; // &looparrowrig -> &looparrowrigh
				case 3624: state = 3625; break; // &LowerRig -> &LowerRigh
				case 3643: state = 3653; break; // &lr -> &lrh
				case 3661: state = 3672; break; // &ls -> &lsh
				case 3666: state = 3671; break; // &Ls -> &Lsh
				case 3692: state = 3700; break; // &lt -> &lth
				case 3727: state = 3728; break; // &lurds -> &lurdsh
				case 3731: state = 3732; break; // &luru -> &luruh
				case 3745: state = 3825; break; // &m -> &mh
				case 3786: state = 3787; break; // &mdas -> &mdash
				case 3897: state = 4101; break; // &n -> &nh
				case 3968: state = 3969; break; // &ndas -> &ndash
				case 3972: state = 3973; break; // &near -> &nearh
				case 4002: state = 4003; break; // &NegativeT -> &NegativeTh
				case 4022: state = 4023; break; // &NegativeVeryT -> &NegativeVeryTh
				case 4150: state = 4151; break; // &nLeftrig -> &nLeftrigh
				case 4160: state = 4161; break; // &nleftrig -> &nleftrigh
				case 4468: state = 4469; break; // &NotRig -> &NotRigh
				case 4632: state = 4633; break; // &nRig -> &nRigh
				case 4641: state = 4642; break; // &nrig -> &nrigh
				case 4653: state = 4663; break; // &ns -> &nsh
				case 4747: state = 4748; break; // &ntrianglerig -> &ntrianglerigh
				case 4766: state = 4767; break; // &nVDas -> &nVDash
				case 4770: state = 4771; break; // &nVdas -> &nVdash
				case 4774: state = 4775; break; // &nvDas -> &nvDash
				case 4778: state = 4779; break; // &nvdas -> &nvdash
				case 4814: state = 4815; break; // &nwar -> &nwarh
				case 4833: state = 4900; break; // &o -> &oh
				case 4853: state = 4854; break; // &odas -> &odash
				case 5023: state = 5024; break; // &Oslas -> &Oslash
				case 5027: state = 5028; break; // &oslas -> &oslash
				case 5077: state = 5078; break; // &OverParent -> &OverParenth
				case 5083: state = 5130; break; // &p -> &ph
				case 5096: state = 5128; break; // &P -> &Ph
				case 5143: state = 5144; break; // &pitc -> &pitch
				case 5154: state = 5155; break; // &planck -> &planckh
				case 5397: state = 5607; break; // &r -> &rh
				case 5405: state = 5613; break; // &R -> &Rh
				case 5439: state = 5448; break; // &rarr -> &rarrh
				case 5530: state = 5531; break; // &rdld -> &rdldh
				case 5538: state = 5539; break; // &rds -> &rdsh
				case 5594: state = 5595; break; // &rfis -> &rfish
				case 5618: state = 5619; break; // &Rig -> &Righ
				case 5643: state = 5644; break; // &rig -> &righ
				case 5645: state = 5712; break; // &right -> &righth
				case 5728: state = 5735; break; // &rightleft -> &rightlefth
				case 5745: state = 5746; break; // &rightrig -> &rightrigh
				case 5778: state = 5779; break; // &rightt -> &rightth
				case 5854: state = 5858; break; // &rl -> &rlh
				case 5868: state = 5869; break; // &rmoustac -> &rmoustach
				case 5926: state = 5927; break; // &Rrig -> &Rrigh
				case 5934: state = 5945; break; // &rs -> &rsh
				case 5939: state = 5944; break; // &Rs -> &Rsh
				case 5951: state = 5952; break; // &rt -> &rth
				case 5980: state = 5981; break; // &rulu -> &ruluh
				case 5985: state = 6104; break; // &S -> &Sh
				case 5991: state = 6088; break; // &s -> &sh
				case 6055: state = 6056; break; // &sear -> &searh
				case 6097: state = 6098; break; // &shc -> &shch
				case 6142: state = 6143; break; // &ShortRig -> &ShortRigh
				case 6214: state = 6215; break; // &smas -> &smash
				case 6366: state = 6367; break; // &straig -> &straigh
				case 6376: state = 6377; break; // &straightp -> &straightph
				case 6446: state = 6490; break; // &Suc -> &Such
				case 6491: state = 6492; break; // &SuchT -> &SuchTh
				case 6500: state = 6525; break; // &sup -> &suph
				case 6566: state = 6567; break; // &swar -> &swarh
				case 6583: state = 6634; break; // &T -> &Th
				case 6586: state = 6629; break; // &t -> &th
				case 6783: state = 6784; break; // &trianglerig -> &trianglerigh
				case 6824: state = 6834; break; // &ts -> &tsh
				case 6849: state = 6850; break; // &two -> &twoh
				case 6865: state = 6866; break; // &twoheadrig -> &twoheadrigh
				case 6879: state = 6957; break; // &u -> &uh
				case 6920: state = 6933; break; // &ud -> &udh
				case 6938: state = 6939; break; // &ufis -> &ufish
				case 7006: state = 7007; break; // &UnderParent -> &UnderParenth
				case 7042: state = 7098; break; // &up -> &uph
				case 7111: state = 7112; break; // &upharpoonrig -> &upharpoonrigh
				case 7131: state = 7132; break; // &UpperRig -> &UpperRigh
				case 7142: state = 7143; break; // &upsi -> &upsih
				case 7244: state = 7245; break; // &varnot -> &varnoth
				case 7249: state = 7250; break; // &varp -> &varph
				case 7261: state = 7262; break; // &varr -> &varrh
				case 7286: state = 7287; break; // &vart -> &varth
				case 7304: state = 7305; break; // &vartrianglerig -> &vartrianglerigh
				case 7321: state = 7322; break; // &VDas -> &VDash
				case 7325: state = 7326; break; // &Vdas -> &Vdash
				case 7329: state = 7330; break; // &vDas -> &vDash
				case 7333: state = 7334; break; // &vdas -> &vdash
				case 7385: state = 7386; break; // &VeryT -> &VeryTh
				case 7439: state = 7440; break; // &Vvdas -> &Vvdash
				case 7487: state = 7488; break; // &wreat -> &wreath
				case 7495: state = 7513; break; // &x -> &xh
				case 7651: state = 7704; break; // &z -> &zh
				case 7686: state = 7687; break; // &ZeroWidt -> &ZeroWidth
				default: return false;
				}
				break;
			case 'i':
				switch (state) {
				case 0: state = 2539; break; // & -> &i
				case 23: state = 30; break; // &ac -> &aci
				case 26: state = 27; break; // &Ac -> &Aci
				case 39: state = 40; break; // &AEl -> &AEli
				case 43: state = 44; break; // &ael -> &aeli
				case 143: state = 150; break; // &ap -> &api
				case 145: state = 146; break; // &apac -> &apaci
				case 162: state = 163; break; // &ApplyFunct -> &ApplyFuncti
				case 172: state = 173; break; // &Ar -> &Ari
				case 176: state = 177; break; // &ar -> &ari
				case 186: state = 187; break; // &Ass -> &Assi
				case 196: state = 197; break; // &At -> &Ati
				case 201: state = 202; break; // &at -> &ati
				case 212: state = 219; break; // &aw -> &awi
				case 215: state = 216; break; // &awcon -> &awconi
				case 222: state = 333; break; // &b -> &bi
				case 232: state = 233; break; // &backeps -> &backepsi
				case 238: state = 239; break; // &backpr -> &backpri
				case 242: state = 243; break; // &backs -> &backsi
				case 306: state = 307; break; // &beps -> &bepsi
				case 317: state = 318; break; // &Bernoull -> &Bernoulli
				case 335: state = 338; break; // &bigc -> &bigci
				case 351: state = 352; break; // &bigot -> &bigoti
				case 365: state = 366; break; // &bigtr -> &bigtri
				case 414: state = 415; break; // &blacktr -> &blacktri
				case 429: state = 430; break; // &blacktriangler -> &blacktriangleri
				case 448: state = 449; break; // &bnequ -> &bnequi
				case 467: state = 468; break; // &bowt -> &bowti
				case 494: state = 495; break; // &boxm -> &boxmi
				case 503: state = 504; break; // &boxt -> &boxti
				case 533: state = 534; break; // &bpr -> &bpri
				case 552: state = 558; break; // &bs -> &bsi
				case 556: state = 557; break; // &bsem -> &bsemi
				case 583: state = 765; break; // &C -> &Ci
				case 589: state = 732; break; // &c -> &ci
				case 595: state = 613; break; // &Cap -> &Capi
				case 617: state = 618; break; // &CapitalD -> &CapitalDi
				case 625: state = 626; break; // &CapitalDifferent -> &CapitalDifferenti
				case 641: state = 664; break; // &cc -> &cci
				case 645: state = 661; break; // &Cc -> &Cci
				case 654: state = 655; break; // &Cced -> &Ccedi
				case 658: state = 659; break; // &cced -> &ccedi
				case 668: state = 669; break; // &Ccon -> &Cconi
				case 684: state = 685; break; // &ced -> &cedi
				case 688: state = 689; break; // &Ced -> &Cedi
				case 719: state = 731; break; // &ch -> &chi
				case 729: state = 730; break; // &Ch -> &Chi
				case 748: state = 749; break; // &circlearrowr -> &circlearrowri
				case 757: state = 758; break; // &circledc -> &circledci
				case 775: state = 776; break; // &CircleM -> &CircleMi
				case 784: state = 785; break; // &CircleT -> &CircleTi
				case 792: state = 793; break; // &cirfn -> &cirfni
				case 796: state = 797; break; // &cirm -> &cirmi
				case 800: state = 801; break; // &cirsc -> &cirsci
				case 807: state = 808; break; // &Clockw -> &Clockwi
				case 853: state = 854; break; // &clubsu -> &clubsui
				case 883: state = 898; break; // &con -> &coni
				case 888: state = 895; break; // &Con -> &Coni
				case 942: state = 943; break; // &CounterClockw -> &CounterClockwi
				case 1065: state = 1066; break; // &curvearrowr -> &curvearrowri
				case 1076: state = 1083; break; // &cw -> &cwi
				case 1079: state = 1080; break; // &cwcon -> &cwconi
				case 1091: state = 1191; break; // &D -> &Di
				case 1097: state = 1228; break; // &d -> &di
				case 1175: state = 1176; break; // &df -> &dfi
				case 1194: state = 1195; break; // &Diacr -> &Diacri
				case 1196: state = 1197; break; // &Diacrit -> &Diacriti
				case 1223: state = 1224; break; // &DiacriticalT -> &DiacriticalTi
				case 1239: state = 1240; break; // &diamondsu -> &diamondsui
				case 1250: state = 1251; break; // &Different -> &Differenti
				case 1260: state = 1261; break; // &dis -> &disi
				case 1263: state = 1264; break; // &div -> &divi
				case 1269: state = 1270; break; // &divideont -> &divideonti
				case 1316: state = 1317; break; // &dotm -> &dotmi
				case 1381: state = 1382; break; // &DoubleLeftR -> &DoubleLeftRi
				case 1406: state = 1407; break; // &DoubleLongLeftR -> &DoubleLongLeftRi
				case 1416: state = 1417; break; // &DoubleLongR -> &DoubleLongRi
				case 1426: state = 1427; break; // &DoubleR -> &DoubleRi
				case 1458: state = 1459; break; // &DoubleVert -> &DoubleVerti
				case 1521: state = 1522; break; // &downharpoonr -> &downharpoonri
				case 1530: state = 1531; break; // &DownLeftR -> &DownLeftRi
				case 1559: state = 1560; break; // &DownR -> &DownRi
				case 1628: state = 1629; break; // &dtr -> &dtri
				case 1647: state = 1650; break; // &dz -> &dzi
				case 1672: state = 1684; break; // &Ec -> &Eci
				case 1677: state = 1682; break; // &ec -> &eci
				case 1728: state = 1735; break; // &el -> &eli
				case 1821: state = 1822; break; // &eps -> &epsi
				case 1824: state = 1825; break; // &Eps -> &Epsi
				case 1834: state = 1835; break; // &eqc -> &eqci
				case 1842: state = 1843; break; // &eqs -> &eqsi
				case 1857: state = 1872; break; // &Equ -> &Equi
				case 1860: state = 1880; break; // &equ -> &equi
				case 1864: state = 1865; break; // &EqualT -> &EqualTi
				case 1873: state = 1874; break; // &Equil -> &Equili
				case 1876: state = 1877; break; // &Equilibr -> &Equilibri
				case 1897: state = 1906; break; // &Es -> &Esi
				case 1900: state = 1908; break; // &es -> &esi
				case 1925: state = 1928; break; // &ex -> &exi
				case 1931: state = 1932; break; // &Ex -> &Exi
				case 1941: state = 1942; break; // &expectat -> &expectati
				case 1950: state = 1951; break; // &Exponent -> &Exponenti
				case 1959: state = 1960; break; // &exponent -> &exponenti
				case 1964: state = 2001; break; // &f -> &fi
				case 1967: state = 1968; break; // &fall -> &falli
				case 1977: state = 2005; break; // &F -> &Fi
				case 1987: state = 1988; break; // &ff -> &ffi
				case 1989: state = 1990; break; // &ffil -> &ffili
				case 1992: state = 1993; break; // &ffl -> &ffli
				case 1995: state = 1996; break; // &ffll -> &fflli
				case 2002: state = 2003; break; // &fil -> &fili
				case 2037: state = 2038; break; // &fjl -> &fjli
				case 2043: state = 2044; break; // &fll -> &flli
				case 2069: state = 2070; break; // &Four -> &Fouri
				case 2079: state = 2080; break; // &fpart -> &fparti
				case 2118: state = 2194; break; // &g -> &gi
				case 2145: state = 2150; break; // &Gc -> &Gci
				case 2147: state = 2148; break; // &Gced -> &Gcedi
				case 2153: state = 2154; break; // &gc -> &gci
				case 2219: state = 2220; break; // &gns -> &gnsi
				case 2277: state = 2278; break; // &GreaterT -> &GreaterTi
				case 2285: state = 2288; break; // &gs -> &gsi
				case 2295: state = 2297; break; // &gtc -> &gtci
				case 2338: state = 2339; break; // &gtrs -> &gtrsi
				case 2351: state = 2417; break; // &H -> &Hi
				case 2357: state = 2358; break; // &ha -> &hai
				case 2364: state = 2365; break; // &ham -> &hami
				case 2381: state = 2382; break; // &harrc -> &harrci
				case 2389: state = 2390; break; // &Hc -> &Hci
				case 2393: state = 2394; break; // &hc -> &hci
				case 2402: state = 2403; break; // &heartsu -> &heartsui
				case 2406: state = 2407; break; // &hell -> &helli
				case 2459: state = 2460; break; // &hookr -> &hookri
				case 2478: state = 2479; break; // &Hor -> &Hori
				case 2486: state = 2487; break; // &HorizontalL -> &HorizontalLi
				case 2539: state = 2582; break; // &i -> &ii
				case 2545: state = 2550; break; // &ic -> &ici
				case 2546: state = 2547; break; // &Ic -> &Ici
				case 2582: state = 2583; break; // &ii -> &iii
				case 2583: state = 2584; break; // &iii -> &iiii
				case 2590: state = 2591; break; // &iinf -> &iinfi
				case 2597: state = 2598; break; // &IJl -> &IJli
				case 2601: state = 2602; break; // &ijl -> &ijli
				case 2614: state = 2615; break; // &Imag -> &Imagi
				case 2621: state = 2622; break; // &imagl -> &imagli
				case 2637: state = 2638; break; // &Impl -> &Impli
				case 2646: state = 2647; break; // &inf -> &infi
				case 2649: state = 2650; break; // &infint -> &infinti
				case 2680: state = 2681; break; // &Intersect -> &Intersecti
				case 2693: state = 2694; break; // &Inv -> &Invi
				case 2695: state = 2696; break; // &Invis -> &Invisi
				case 2705: state = 2706; break; // &InvisibleT -> &InvisibleTi
				case 2743: state = 2746; break; // &is -> &isi
				case 2755: state = 2761; break; // &it -> &iti
				case 2756: state = 2757; break; // &It -> &Iti
				case 2778: state = 2779; break; // &Jc -> &Jci
				case 2783: state = 2784; break; // &jc -> &jci
				case 2838: state = 2839; break; // &Kced -> &Kcedi
				case 2843: state = 2844; break; // &kced -> &kcedi
				case 2951: state = 2952; break; // &larrs -> &larrsi
				case 2958: state = 2959; break; // &lAta -> &lAtai
				case 2961: state = 2962; break; // &lata -> &latai
				case 2999: state = 3000; break; // &Lced -> &Lcedi
				case 3002: state = 3006; break; // &lce -> &lcei
				case 3003: state = 3004; break; // &lced -> &lcedi
				case 3067: state = 3068; break; // &LeftArrowR -> &LeftArrowRi
				case 3078: state = 3079; break; // &leftarrowta -> &leftarrowtai
				case 3082: state = 3083; break; // &LeftCe -> &LeftCei
				case 3084: state = 3085; break; // &LeftCeil -> &LeftCeili
				case 3149: state = 3150; break; // &LeftR -> &LeftRi
				case 3159: state = 3160; break; // &Leftr -> &Leftri
				case 3169: state = 3170; break; // &leftr -> &leftri
				case 3190: state = 3191; break; // &leftrightsqu -> &leftrightsqui
				case 3223: state = 3224; break; // &leftthreet -> &leftthreeti
				case 3228: state = 3229; break; // &LeftTr -> &LeftTri
				case 3358: state = 3359; break; // &lesss -> &lesssi
				case 3371: state = 3372; break; // &LessT -> &LessTi
				case 3376: state = 3377; break; // &lf -> &lfi
				case 3432: state = 3433; break; // &lltr -> &lltri
				case 3434: state = 3435; break; // &Lm -> &Lmi
				case 3439: state = 3440; break; // &lm -> &lmi
				case 3463: state = 3464; break; // &lns -> &lnsi
				case 3507: state = 3508; break; // &LongLeftR -> &LongLeftRi
				case 3517: state = 3518; break; // &Longleftr -> &Longleftri
				case 3527: state = 3528; break; // &longleftr -> &longleftri
				case 3543: state = 3544; break; // &LongR -> &LongRi
				case 3553: state = 3554; break; // &Longr -> &Longri
				case 3563: state = 3564; break; // &longr -> &longri
				case 3584: state = 3585; break; // &looparrowr -> &looparrowri
				case 3598: state = 3599; break; // &lot -> &loti
				case 3622: state = 3623; break; // &LowerR -> &LowerRi
				case 3659: state = 3660; break; // &lrtr -> &lrtri
				case 3661: state = 3673; break; // &ls -> &lsi
				case 3692: state = 3704; break; // &lt -> &lti
				case 3693: state = 3695; break; // &ltc -> &ltci
				case 3717: state = 3718; break; // &ltr -> &ltri
				case 3745: state = 3827; break; // &m -> &mi
				case 3755: state = 3847; break; // &M -> &Mi
				case 3805: state = 3806; break; // &Med -> &Medi
				case 3815: state = 3816; break; // &Mell -> &Melli
				case 3835: state = 3836; break; // &midc -> &midci
				case 3889: state = 3890; break; // &mult -> &multi
				case 3897: state = 4111; break; // &n -> &ni
				case 3914: state = 3916; break; // &nap -> &napi
				case 3949: state = 3950; break; // &Nced -> &Ncedi
				case 3953: state = 3954; break; // &nced -> &ncedi
				case 3987: state = 3988; break; // &Negat -> &Negati
				case 3993: state = 3994; break; // &NegativeMed -> &NegativeMedi
				case 4003: state = 4004; break; // &NegativeTh -> &NegativeThi
				case 4023: state = 4024; break; // &NegativeVeryTh -> &NegativeVeryThi
				case 4032: state = 4033; break; // &nequ -> &nequi
				case 4035: state = 4039; break; // &nes -> &nesi
				case 4068: state = 4069; break; // &NewL -> &NewLi
				case 4072: state = 4073; break; // &nex -> &nexi
				case 4094: state = 4095; break; // &ngs -> &ngsi
				case 4148: state = 4149; break; // &nLeftr -> &nLeftri
				case 4158: state = 4159; break; // &nleftr -> &nleftri
				case 4178: state = 4179; break; // &nls -> &nlsi
				case 4183: state = 4184; break; // &nltr -> &nltri
				case 4187: state = 4188; break; // &nm -> &nmi
				case 4201: state = 4202; break; // &NonBreak -> &NonBreaki
				case 4216: state = 4333; break; // &not -> &noti
				case 4240: state = 4241; break; // &NotDoubleVert -> &NotDoubleVerti
				case 4259: state = 4260; break; // &NotEqualT -> &NotEqualTi
				case 4264: state = 4265; break; // &NotEx -> &NotExi
				case 4311: state = 4312; break; // &NotGreaterT -> &NotGreaterTi
				case 4348: state = 4349; break; // &NotLeftTr -> &NotLeftTri
				case 4391: state = 4392; break; // &NotLessT -> &NotLessTi
				case 4424: state = 4425; break; // &notn -> &notni
				case 4453: state = 4467; break; // &NotR -> &NotRi
				case 4472: state = 4473; break; // &NotRightTr -> &NotRightTri
				case 4546: state = 4547; break; // &NotSucceedsT -> &NotSucceedsTi
				case 4562: state = 4563; break; // &NotT -> &NotTi
				case 4581: state = 4582; break; // &NotTildeT -> &NotTildeTi
				case 4589: state = 4590; break; // &NotVert -> &NotVerti
				case 4609: state = 4610; break; // &npol -> &npoli
				case 4621: state = 4640; break; // &nr -> &nri
				case 4630: state = 4631; break; // &nR -> &nRi
				case 4650: state = 4651; break; // &nrtr -> &nrtri
				case 4653: state = 4678; break; // &ns -> &nsi
				case 4667: state = 4668; break; // &nshortm -> &nshortmi
				case 4682: state = 4683; break; // &nsm -> &nsmi
				case 4718: state = 4726; break; // &nt -> &nti
				case 4721: state = 4722; break; // &Nt -> &Nti
				case 4732: state = 4733; break; // &ntr -> &ntri
				case 4745: state = 4746; break; // &ntriangler -> &ntriangleri
				case 4760: state = 4787; break; // &nv -> &nvi
				case 4789: state = 4790; break; // &nvinf -> &nvinfi
				case 4798: state = 4799; break; // &nvltr -> &nvltri
				case 4806: state = 4807; break; // &nvrtr -> &nvrtri
				case 4809: state = 4810; break; // &nvs -> &nvsi
				case 4833: state = 4905; break; // &o -> &oi
				case 4841: state = 4842; break; // &oc -> &oci
				case 4844: state = 4845; break; // &Oc -> &Oci
				case 4851: state = 4864; break; // &od -> &odi
				case 4873: state = 4874; break; // &OEl -> &OEli
				case 4877: state = 4878; break; // &oel -> &oeli
				case 4881: state = 4882; break; // &ofc -> &ofci
				case 4908: state = 4919; break; // &ol -> &oli
				case 4912: state = 4913; break; // &olc -> &olci
				case 4923: state = 4937; break; // &Om -> &Omi
				case 4927: state = 4942; break; // &om -> &omi
				case 4991: state = 5002; break; // &or -> &ori
				case 5031: state = 5032; break; // &Ot -> &Oti
				case 5036: state = 5037; break; // &ot -> &oti
				case 5080: state = 5081; break; // &OverParenthes -> &OverParenthesi
				case 5083: state = 5141; break; // &p -> &pi
				case 5091: state = 5092; break; // &pars -> &parsi
				case 5096: state = 5140; break; // &P -> &Pi
				case 5099: state = 5100; break; // &Part -> &Parti
				case 5109: state = 5113; break; // &per -> &peri
				case 5116: state = 5117; break; // &perm -> &permi
				case 5128: state = 5129; break; // &Ph -> &Phi
				case 5130: state = 5131; break; // &ph -> &phi
				case 5161: state = 5162; break; // &plusac -> &plusaci
				case 5165: state = 5166; break; // &plusc -> &plusci
				case 5175: state = 5176; break; // &PlusM -> &PlusMi
				case 5182: state = 5183; break; // &pluss -> &plussi
				case 5189: state = 5190; break; // &Po -> &Poi
				case 5201: state = 5202; break; // &po -> &poi
				case 5204: state = 5205; break; // &point -> &pointi
				case 5215: state = 5282; break; // &Pr -> &Pri
				case 5216: state = 5285; break; // &pr -> &pri
				case 5259: state = 5260; break; // &PrecedesT -> &PrecedesTi
				case 5276: state = 5277; break; // &precns -> &precnsi
				case 5279: state = 5280; break; // &precs -> &precsi
				case 5293: state = 5294; break; // &prns -> &prnsi
				case 5308: state = 5309; break; // &profl -> &profli
				case 5320: state = 5321; break; // &Proport -> &Proporti
				case 5328: state = 5329; break; // &prs -> &prsi
				case 5335: state = 5341; break; // &Ps -> &Psi
				case 5338: state = 5342; break; // &ps -> &psi
				case 5351: state = 5354; break; // &q -> &qi
				case 5364: state = 5365; break; // &qpr -> &qpri
				case 5376: state = 5384; break; // &quat -> &quati
				case 5379: state = 5380; break; // &quatern -> &quaterni
				case 5397: state = 5642; break; // &r -> &ri
				case 5405: state = 5617; break; // &R -> &Ri
				case 5414: state = 5415; break; // &rad -> &radi
				case 5454: state = 5455; break; // &rarrs -> &rarrsi
				case 5463: state = 5464; break; // &rAta -> &rAtai
				case 5466: state = 5470; break; // &rat -> &rati
				case 5467: state = 5468; break; // &rata -> &ratai
				case 5513: state = 5514; break; // &Rced -> &Rcedi
				case 5516: state = 5520; break; // &rce -> &rcei
				case 5517: state = 5518; break; // &rced -> &rcedi
				case 5543: state = 5544; break; // &real -> &reali
				case 5570: state = 5571; break; // &ReverseEqu -> &ReverseEqui
				case 5572: state = 5573; break; // &ReverseEquil -> &ReverseEquili
				case 5575: state = 5576; break; // &ReverseEquilibr -> &ReverseEquilibri
				case 5583: state = 5584; break; // &ReverseUpEqu -> &ReverseUpEqui
				case 5585: state = 5586; break; // &ReverseUpEquil -> &ReverseUpEquili
				case 5588: state = 5589; break; // &ReverseUpEquilibr -> &ReverseUpEquilibri
				case 5592: state = 5593; break; // &rf -> &rfi
				case 5664: state = 5665; break; // &rightarrowta -> &rightarrowtai
				case 5668: state = 5669; break; // &RightCe -> &RightCei
				case 5670: state = 5671; break; // &RightCeil -> &RightCeili
				case 5743: state = 5744; break; // &rightr -> &rightri
				case 5756: state = 5757; break; // &rightsqu -> &rightsqui
				case 5783: state = 5784; break; // &rightthreet -> &rightthreeti
				case 5788: state = 5789; break; // &RightTr -> &RightTri
				case 5844: state = 5845; break; // &ris -> &risi
				case 5872: state = 5873; break; // &rnm -> &rnmi
				case 5894: state = 5895; break; // &rot -> &roti
				case 5905: state = 5906; break; // &RoundImpl -> &RoundImpli
				case 5916: state = 5917; break; // &rppol -> &rppoli
				case 5924: state = 5925; break; // &Rr -> &Rri
				case 5951: state = 5956; break; // &rt -> &rti
				case 5960: state = 5961; break; // &rtr -> &rtri
				case 5966: state = 5967; break; // &rtriltr -> &rtriltri
				case 5985: state = 6158; break; // &S -> &Si
				case 5991: state = 6162; break; // &s -> &si
				case 6001: state = 6024; break; // &Sc -> &Sci
				case 6002: state = 6027; break; // &sc -> &sci
				case 6018: state = 6019; break; // &Sced -> &Scedi
				case 6021: state = 6022; break; // &sced -> &scedi
				case 6034: state = 6035; break; // &scns -> &scnsi
				case 6039: state = 6040; break; // &scpol -> &scpoli
				case 6043: state = 6044; break; // &scs -> &scsi
				case 6066: state = 6067; break; // &sem -> &semi
				case 6073: state = 6074; break; // &setm -> &setmi
				case 6129: state = 6130; break; // &shortm -> &shortmi
				case 6140: state = 6141; break; // &ShortR -> &ShortRi
				case 6196: state = 6197; break; // &SmallC -> &SmallCi
				case 6202: state = 6223; break; // &sm -> &smi
				case 6209: state = 6210; break; // &smallsetm -> &smallsetmi
				case 6254: state = 6255; break; // &spadesu -> &spadesui
				case 6301: state = 6302; break; // &SquareIntersect -> &SquareIntersecti
				case 6328: state = 6329; break; // &SquareUn -> &SquareUni
				case 6348: state = 6349; break; // &ssm -> &ssmi
				case 6364: state = 6365; break; // &stra -> &strai
				case 6371: state = 6372; break; // &straighteps -> &straightepsi
				case 6377: state = 6378; break; // &straightph -> &straightphi
				case 6411: state = 6426; break; // &subs -> &subsi
				case 6467: state = 6468; break; // &SucceedsT -> &SucceedsTi
				case 6484: state = 6485; break; // &succns -> &succnsi
				case 6487: state = 6488; break; // &succs -> &succsi
				case 6549: state = 6559; break; // &sups -> &supsi
				case 6580: state = 6581; break; // &szl -> &szli
				case 6583: state = 6696; break; // &T -> &Ti
				case 6586: state = 6700; break; // &t -> &ti
				case 6608: state = 6609; break; // &Tced -> &Tcedi
				case 6612: state = 6613; break; // &tced -> &tcedi
				case 6629: state = 6654; break; // &th -> &thi
				case 6634: state = 6666; break; // &Th -> &Thi
				case 6663: state = 6664; break; // &thicks -> &thicksi
				case 6686: state = 6687; break; // &thks -> &thksi
				case 6718: state = 6719; break; // &TildeT -> &TildeTi
				case 6739: state = 6740; break; // &topc -> &topci
				case 6752: state = 6753; break; // &tpr -> &tpri
				case 6760: state = 6764; break; // &tr -> &tri
				case 6781: state = 6782; break; // &triangler -> &triangleri
				case 6792: state = 6793; break; // &trim -> &trimi
				case 6797: state = 6798; break; // &Tr -> &Tri
				case 6811: state = 6812; break; // &trit -> &triti
				case 6817: state = 6818; break; // &trpez -> &trpezi
				case 6845: state = 6846; break; // &tw -> &twi
				case 6863: state = 6864; break; // &twoheadr -> &twoheadri
				case 6893: state = 6894; break; // &Uarroc -> &Uarroci
				case 6910: state = 6911; break; // &Uc -> &Uci
				case 6914: state = 6915; break; // &uc -> &uci
				case 6936: state = 6937; break; // &uf -> &ufi
				case 6976: state = 6977; break; // &ultr -> &ultri
				case 6987: state = 7012; break; // &Un -> &Uni
				case 7009: state = 7010; break; // &UnderParenthes -> &UnderParenthesi
				case 7089: state = 7090; break; // &UpEqu -> &UpEqui
				case 7091: state = 7092; break; // &UpEquil -> &UpEquili
				case 7094: state = 7095; break; // &UpEquilibr -> &UpEquilibri
				case 7109: state = 7110; break; // &upharpoonr -> &upharpoonri
				case 7129: state = 7130; break; // &UpperR -> &UpperRi
				case 7139: state = 7140; break; // &Ups -> &Upsi
				case 7141: state = 7142; break; // &ups -> &upsi
				case 7166: state = 7180; break; // &ur -> &uri
				case 7176: state = 7177; break; // &Ur -> &Uri
				case 7184: state = 7185; break; // &urtr -> &urtri
				case 7192: state = 7201; break; // &ut -> &uti
				case 7196: state = 7197; break; // &Ut -> &Uti
				case 7205: state = 7206; break; // &utr -> &utri
				case 7232: state = 7233; break; // &vareps -> &varepsi
				case 7245: state = 7246; break; // &varnoth -> &varnothi
				case 7249: state = 7252; break; // &varp -> &varpi
				case 7250: state = 7251; break; // &varph -> &varphi
				case 7264: state = 7265; break; // &vars -> &varsi
				case 7291: state = 7292; break; // &vartr -> &vartri
				case 7302: state = 7303; break; // &vartriangler -> &vartriangleri
				case 7346: state = 7347; break; // &vell -> &velli
				case 7357: state = 7359; break; // &Vert -> &Verti
				case 7366: state = 7367; break; // &VerticalL -> &VerticalLi
				case 7379: state = 7380; break; // &VerticalT -> &VerticalTi
				case 7386: state = 7387; break; // &VeryTh -> &VeryThi
				case 7400: state = 7401; break; // &vltr -> &vltri
				case 7419: state = 7420; break; // &vrtr -> &vrtri
				case 7441: state = 7442; break; // &vz -> &vzi
				case 7448: state = 7449; break; // &Wc -> &Wci
				case 7453: state = 7454; break; // &wc -> &wci
				case 7457: state = 7469; break; // &we -> &wei
				case 7495: state = 7521; break; // &x -> &xi
				case 7496: state = 7499; break; // &xc -> &xci
				case 7506: state = 7507; break; // &xdtr -> &xdtri
				case 7508: state = 7520; break; // &X -> &Xi
				case 7532: state = 7533; break; // &xn -> &xni
				case 7547: state = 7548; break; // &xot -> &xoti
				case 7574: state = 7575; break; // &xutr -> &xutri
				case 7590: state = 7619; break; // &y -> &yi
				case 7600: state = 7601; break; // &Yc -> &Yci
				case 7604: state = 7605; break; // &yc -> &yci
				case 7651: state = 7707; break; // &z -> &zi
				case 7683: state = 7684; break; // &ZeroW -> &ZeroWi
				default: return false;
				}
				break;
			case 'j':
				switch (state) {
				case 0: state = 2782; break; // & -> &j
				case 1097: state = 1280; break; // &d -> &dj
				case 1964: state = 2036; break; // &f -> &fj
				case 2118: state = 2201; break; // &g -> &gj
				case 2204: state = 2207; break; // &gl -> &glj
				case 2539: state = 2600; break; // &i -> &ij
				case 2830: state = 2866; break; // &k -> &kj
				case 2881: state = 3405; break; // &l -> &lj
				case 3897: state = 4118; break; // &n -> &nj
				case 7725: state = 7726; break; // &zw -> &zwj
				case 7727: state = 7728; break; // &zwn -> &zwnj
				default: return false;
				}
				break;
			case 'k':
				switch (state) {
				case 0: state = 2830; break; // & -> &k
				case 222: state = 391; break; // &b -> &bk
				case 224: state = 225; break; // &bac -> &back
				case 249: state = 250; break; // &Bac -> &Back
				case 271: state = 272; break; // &bbr -> &bbrk
				case 275: state = 276; break; // &bbrktbr -> &bbrktbrk
				case 396: state = 436; break; // &bl -> &blk
				case 398: state = 399; break; // &blac -> &black
				case 434: state = 435; break; // &blan -> &blank
				case 443: state = 444; break; // &bloc -> &block
				case 723: state = 724; break; // &chec -> &check
				case 727: state = 728; break; // &checkmar -> &checkmark
				case 805: state = 806; break; // &Cloc -> &Clock
				case 940: state = 941; break; // &CounterCloc -> &CounterClock
				case 1120: state = 1121; break; // &db -> &dbk
				case 1591: state = 1592; break; // &drb -> &drbk
				case 1618: state = 1619; break; // &Dstro -> &Dstrok
				case 1622: state = 1623; break; // &dstro -> &dstrok
				case 2062: state = 2066; break; // &for -> &fork
				case 2354: state = 2355; break; // &Hace -> &Hacek
				case 2356: state = 2428; break; // &h -> &hk
				case 2448: state = 2449; break; // &hoo -> &hook
				case 2502: state = 2503; break; // &Hstro -> &Hstrok
				case 2506: state = 2507; break; // &hstro -> &hstrok
				case 2687: state = 2688; break; // &intlarh -> &intlarhk
				case 2765: state = 2766; break; // &Iu -> &Iuk
				case 2769: state = 2770; break; // &iu -> &iuk
				case 2817: state = 2818; break; // &Ju -> &Juk
				case 2821: state = 2822; break; // &ju -> &juk
				case 2945: state = 2946; break; // &larrh -> &larrhk
				case 2975: state = 2976; break; // &lbbr -> &lbbrk
				case 2977: state = 2982; break; // &lbr -> &lbrk
				case 2979: state = 2981; break; // &lbrac -> &lbrack
				case 3044: state = 3045; break; // &LeftAngleBrac -> &LeftAngleBrack
				case 3097: state = 3098; break; // &LeftDoubleBrac -> &LeftDoubleBrack
				case 3400: state = 3401; break; // &lhbl -> &lhblk
				case 3473: state = 3474; break; // &lobr -> &lobrk
				case 3684: state = 3685; break; // &Lstro -> &Lstrok
				case 3688: state = 3689; break; // &lstro -> &lstrok
				case 3772: state = 3773; break; // &mar -> &mark
				case 3973: state = 3974; break; // &nearh -> &nearhk
				case 4005: state = 4006; break; // &NegativeThic -> &NegativeThick
				case 4194: state = 4195; break; // &NoBrea -> &NoBreak
				case 4200: state = 4201; break; // &NonBrea -> &NonBreak
				case 4815: state = 4816; break; // &nwarh -> &nwarhk
				case 5067: state = 5069; break; // &OverBrac -> &OverBrack
				case 5122: state = 5123; break; // &perten -> &pertenk
				case 5147: state = 5148; break; // &pitchfor -> &pitchfork
				case 5152: state = 5156; break; // &plan -> &plank
				case 5153: state = 5154; break; // &planc -> &planck
				case 5448: state = 5449; break; // &rarrh -> &rarrhk
				case 5489: state = 5490; break; // &rbbr -> &rbbrk
				case 5491: state = 5496; break; // &rbr -> &rbrk
				case 5493: state = 5495; break; // &rbrac -> &rbrack
				case 5629: state = 5630; break; // &RightAngleBrac -> &RightAngleBrack
				case 5683: state = 5684; break; // &RightDoubleBrac -> &RightDoubleBrack
				case 5882: state = 5883; break; // &robr -> &robrk
				case 6056: state = 6057; break; // &searh -> &searhk
				case 6567: state = 6568; break; // &swarh -> &swarhk
				case 6595: state = 6596; break; // &tbr -> &tbrk
				case 6629: state = 6683; break; // &th -> &thk
				case 6655: state = 6656; break; // &thic -> &thick
				case 6667: state = 6668; break; // &Thic -> &Thick
				case 6747: state = 6748; break; // &topfor -> &topfork
				case 6839: state = 6840; break; // &Tstro -> &Tstrok
				case 6843: state = 6844; break; // &tstro -> &tstrok
				case 6963: state = 6964; break; // &uhbl -> &uhblk
				case 6996: state = 6998; break; // &UnderBrac -> &UnderBrack
				case 7229: state = 7237; break; // &var -> &vark
				default: return false;
				}
				break;
			case 'l':
				switch (state) {
				case 0: state = 2881; break; // & -> &l
				case 1: state = 68; break; // &A -> &Al
				case 7: state = 60; break; // &a -> &al
				case 38: state = 39; break; // &AE -> &AEl
				case 42: state = 43; break; // &ae -> &ael
				case 80: state = 83; break; // &ama -> &amal
				case 96: state = 97; break; // &ands -> &andsl
				case 102: state = 104; break; // &ang -> &angl
				case 155: state = 156; break; // &App -> &Appl
				case 197: state = 198; break; // &Ati -> &Atil
				case 202: state = 203; break; // &ati -> &atil
				case 207: state = 208; break; // &Aum -> &Auml
				case 210: state = 211; break; // &aum -> &auml
				case 222: state = 396; break; // &b -> &bl
				case 233: state = 234; break; // &backepsi -> &backepsil
				case 251: state = 252; break; // &Backs -> &Backsl
				case 315: state = 316; break; // &Bernou -> &Bernoul
				case 316: state = 317; break; // &Bernoul -> &Bernoull
				case 347: state = 348; break; // &bigop -> &bigopl
				case 369: state = 370; break; // &bigtriang -> &bigtriangl
				case 379: state = 380; break; // &bigup -> &bigupl
				case 399: state = 400; break; // &black -> &blackl
				case 418: state = 419; break; // &blacktriang -> &blacktriangl
				case 420: state = 425; break; // &blacktriangle -> &blacktrianglel
				case 474: state = 476; break; // &boxD -> &boxDl
				case 477: state = 479; break; // &boxd -> &boxdl
				case 499: state = 500; break; // &boxp -> &boxpl
				case 508: state = 510; break; // &boxU -> &boxUl
				case 511: state = 513; break; // &boxu -> &boxul
				case 518: state = 525; break; // &boxV -> &boxVl
				case 519: state = 527; break; // &boxv -> &boxvl
				case 561: state = 562; break; // &bso -> &bsol
				case 568: state = 569; break; // &bu -> &bul
				case 569: state = 570; break; // &bul -> &bull
				case 583: state = 803; break; // &C -> &Cl
				case 589: state = 849; break; // &c -> &cl
				case 615: state = 616; break; // &Capita -> &Capital
				case 627: state = 628; break; // &CapitalDifferentia -> &CapitalDifferential
				case 636: state = 637; break; // &Cay -> &Cayl
				case 655: state = 656; break; // &Ccedi -> &Ccedil
				case 659: state = 660; break; // &ccedi -> &ccedil
				case 685: state = 686; break; // &cedi -> &cedil
				case 689: state = 690; break; // &Cedi -> &Cedil
				case 690: state = 691; break; // &Cedil -> &Cedill
				case 734: state = 737; break; // &circ -> &circl
				case 743: state = 744; break; // &circlearrow -> &circlearrowl
				case 767: state = 768; break; // &Circ -> &Circl
				case 780: state = 781; break; // &CircleP -> &CirclePl
				case 824: state = 825; break; // &ClockwiseContourIntegra -> &ClockwiseContourIntegral
				case 830: state = 831; break; // &CloseCur -> &CloseCurl
				case 836: state = 837; break; // &CloseCurlyDoub -> &CloseCurlyDoubl
				case 856: state = 857; break; // &Co -> &Col
				case 860: state = 861; break; // &co -> &col
				case 871: state = 874; break; // &comp -> &compl
				case 911: state = 912; break; // &ContourIntegra -> &ContourIntegral
				case 937: state = 938; break; // &CounterC -> &CounterCl
				case 959: state = 960; break; // &CounterClockwiseContourIntegra -> &CounterClockwiseContourIntegral
				case 987: state = 999; break; // &cu -> &cul
				case 991: state = 992; break; // &cudarr -> &cudarrl
				case 1026: state = 1031; break; // &cur -> &curl
				case 1060: state = 1061; break; // &curvearrow -> &curvearrowl
				case 1086: state = 1087; break; // &cy -> &cyl
				case 1097: state = 1283; break; // &d -> &dl
				case 1098: state = 1103; break; // &da -> &dal
				case 1120: state = 1126; break; // &db -> &dbl
				case 1161: state = 1167; break; // &de -> &del
				case 1163: state = 1164; break; // &De -> &Del
				case 1188: state = 1189; break; // &dhar -> &dharl
				case 1199: state = 1200; break; // &Diacritica -> &Diacritical
				case 1210: state = 1211; break; // &DiacriticalDoub -> &DiacriticalDoubl
				case 1224: state = 1225; break; // &DiacriticalTi -> &DiacriticalTil
				case 1252: state = 1253; break; // &Differentia -> &Differential
				case 1291: state = 1292; break; // &do -> &dol
				case 1292: state = 1293; break; // &dol -> &doll
				case 1314: state = 1315; break; // &DotEqua -> &DotEqual
				case 1321: state = 1322; break; // &dotp -> &dotpl
				case 1332: state = 1333; break; // &doub -> &doubl
				case 1344: state = 1345; break; // &Doub -> &Doubl
				case 1360: state = 1361; break; // &DoubleContourIntegra -> &DoubleContourIntegral
				case 1461: state = 1462; break; // &DoubleVertica -> &DoubleVertical
				case 1516: state = 1517; break; // &downharpoon -> &downharpoonl
				case 1614: state = 1615; break; // &dso -> &dsol
				case 1641: state = 1642; break; // &dwang -> &dwangl
				case 1656: state = 1729; break; // &E -> &El
				case 1662: state = 1728; break; // &e -> &el
				case 1688: state = 1689; break; // &eco -> &ecol
				case 1728: state = 1741; break; // &el -> &ell
				case 1765: state = 1766; break; // &EmptySma -> &EmptySmal
				case 1766: state = 1767; break; // &EmptySmal -> &EmptySmall
				case 1781: state = 1782; break; // &EmptyVerySma -> &EmptyVerySmal
				case 1782: state = 1783; break; // &EmptyVerySmal -> &EmptyVerySmall
				case 1813: state = 1818; break; // &ep -> &epl
				case 1816: state = 1817; break; // &epars -> &eparsl
				case 1822: state = 1829; break; // &epsi -> &epsil
				case 1825: state = 1826; break; // &Epsi -> &Epsil
				case 1838: state = 1839; break; // &eqco -> &eqcol
				case 1842: state = 1845; break; // &eqs -> &eqsl
				case 1848: state = 1852; break; // &eqslant -> &eqslantl
				case 1858: state = 1859; break; // &Equa -> &Equal
				case 1861: state = 1862; break; // &equa -> &equal
				case 1865: state = 1866; break; // &EqualTi -> &EqualTil
				case 1872: state = 1873; break; // &Equi -> &Equil
				case 1888: state = 1889; break; // &eqvpars -> &eqvparsl
				case 1918: state = 1919; break; // &Eum -> &Euml
				case 1921: state = 1922; break; // &eum -> &euml
				case 1926: state = 1927; break; // &exc -> &excl
				case 1952: state = 1953; break; // &Exponentia -> &Exponential
				case 1961: state = 1962; break; // &exponentia -> &exponential
				case 1964: state = 2040; break; // &f -> &fl
				case 1965: state = 1966; break; // &fa -> &fal
				case 1966: state = 1967; break; // &fal -> &fall
				case 1984: state = 1985; break; // &fema -> &femal
				case 1987: state = 1992; break; // &ff -> &ffl
				case 1988: state = 1989; break; // &ffi -> &ffil
				case 1992: state = 1995; break; // &ffl -> &ffll
				case 2001: state = 2002; break; // &fi -> &fil
				case 2005: state = 2006; break; // &Fi -> &Fil
				case 2006: state = 2007; break; // &Fil -> &Fill
				case 2012: state = 2013; break; // &FilledSma -> &FilledSmal
				case 2013: state = 2014; break; // &FilledSmal -> &FilledSmall
				case 2027: state = 2028; break; // &FilledVerySma -> &FilledVerySmal
				case 2028: state = 2029; break; // &FilledVerySmal -> &FilledVerySmall
				case 2036: state = 2037; break; // &fj -> &fjl
				case 2040: state = 2043; break; // &fl -> &fll
				case 2059: state = 2060; break; // &ForA -> &ForAl
				case 2060: state = 2061; break; // &ForAl -> &ForAll
				case 2063: state = 2064; break; // &fora -> &foral
				case 2064: state = 2065; break; // &foral -> &forall
				case 2107: state = 2108; break; // &fras -> &frasl
				case 2118: state = 2204; break; // &g -> &gl
				case 2148: state = 2149; break; // &Gcedi -> &Gcedil
				case 2165: state = 2167; break; // &gE -> &gEl
				case 2166: state = 2168; break; // &ge -> &gel
				case 2171: state = 2172; break; // &geqs -> &geqsl
				case 2176: state = 2184; break; // &ges -> &gesl
				case 2182: state = 2183; break; // &gesdoto -> &gesdotol
				case 2196: state = 2197; break; // &gime -> &gimel
				case 2241: state = 2242; break; // &GreaterEqua -> &GreaterEqual
				case 2248: state = 2249; break; // &GreaterFu -> &GreaterFul
				case 2249: state = 2250; break; // &GreaterFul -> &GreaterFull
				case 2254: state = 2255; break; // &GreaterFullEqua -> &GreaterFullEqual
				case 2267: state = 2268; break; // &GreaterS -> &GreaterSl
				case 2275: state = 2276; break; // &GreaterSlantEqua -> &GreaterSlantEqual
				case 2278: state = 2279; break; // &GreaterTi -> &GreaterTil
				case 2289: state = 2291; break; // &gsim -> &gsiml
				case 2294: state = 2302; break; // &gt -> &gtl
				case 2311: state = 2334; break; // &gtr -> &gtrl
				case 2324: state = 2325; break; // &gtreq -> &gtreql
				case 2329: state = 2330; break; // &gtreqq -> &gtreqql
				case 2357: state = 2362; break; // &ha -> &hal
				case 2365: state = 2366; break; // &hami -> &hamil
				case 2397: state = 2405; break; // &he -> &hel
				case 2405: state = 2406; break; // &hel -> &hell
				case 2417: state = 2418; break; // &Hi -> &Hil
				case 2449: state = 2450; break; // &hook -> &hookl
				case 2484: state = 2485; break; // &Horizonta -> &Horizontal
				case 2493: state = 2496; break; // &hs -> &hsl
				case 2522: state = 2523; break; // &HumpEqua -> &HumpEqual
				case 2526: state = 2527; break; // &hybu -> &hybul
				case 2527: state = 2528; break; // &hybul -> &hybull
				case 2565: state = 2566; break; // &iexc -> &iexcl
				case 2596: state = 2597; break; // &IJ -> &IJl
				case 2600: state = 2601; break; // &ij -> &ijl
				case 2612: state = 2621; break; // &imag -> &imagl
				case 2636: state = 2637; break; // &Imp -> &Impl
				case 2658: state = 2684; break; // &int -> &intl
				case 2660: state = 2661; break; // &intca -> &intcal
				case 2670: state = 2671; break; // &Integra -> &Integral
				case 2674: state = 2675; break; // &interca -> &intercal
				case 2697: state = 2698; break; // &Invisib -> &Invisibl
				case 2757: state = 2758; break; // &Iti -> &Itil
				case 2761: state = 2762; break; // &iti -> &itil
				case 2773: state = 2774; break; // &Ium -> &Iuml
				case 2775: state = 2776; break; // &ium -> &iuml
				case 2839: state = 2840; break; // &Kcedi -> &Kcedil
				case 2844: state = 2845; break; // &kcedi -> &kcedil
				case 2881: state = 3409; break; // &l -> &ll
				case 2886: state = 3408; break; // &L -> &Ll
				case 2918: state = 2920; break; // &lang -> &langl
				case 2923: state = 2924; break; // &Lap -> &Lapl
				case 2939: state = 2947; break; // &larr -> &larrl
				case 2949: state = 2950; break; // &larrp -> &larrpl
				case 2954: state = 2955; break; // &larrt -> &larrtl
				case 2959: state = 2960; break; // &lAtai -> &lAtail
				case 2962: state = 2963; break; // &latai -> &latail
				case 2984: state = 2985; break; // &lbrks -> &lbrksl
				case 3000: state = 3001; break; // &Lcedi -> &Lcedil
				case 3004: state = 3005; break; // &lcedi -> &lcedil
				case 3006: state = 3007; break; // &lcei -> &lceil
				case 3038: state = 3039; break; // &LeftAng -> &LeftAngl
				case 3058: state = 3139; break; // &left -> &leftl
				case 3079: state = 3080; break; // &leftarrowtai -> &leftarrowtail
				case 3083: state = 3084; break; // &LeftCei -> &LeftCeil
				case 3091: state = 3092; break; // &LeftDoub -> &LeftDoubl
				case 3121: state = 3122; break; // &LeftF -> &LeftFl
				case 3232: state = 3233; break; // &LeftTriang -> &LeftTriangl
				case 3241: state = 3242; break; // &LeftTriangleEqua -> &LeftTriangleEqual
				case 3286: state = 3287; break; // &leqs -> &leqsl
				case 3326: state = 3327; break; // &LessEqua -> &LessEqual
				case 3336: state = 3337; break; // &LessFu -> &LessFul
				case 3337: state = 3338; break; // &LessFul -> &LessFull
				case 3342: state = 3343; break; // &LessFullEqua -> &LessFullEqual
				case 3361: state = 3362; break; // &LessS -> &LessSl
				case 3369: state = 3370; break; // &LessSlantEqua -> &LessSlantEqual
				case 3372: state = 3373; break; // &LessTi -> &LessTil
				case 3376: state = 3381; break; // &lf -> &lfl
				case 3397: state = 3398; break; // &lharu -> &lharul
				case 3399: state = 3400; break; // &lhb -> &lhbl
				case 3477: state = 3487; break; // &Long -> &Longl
				case 3497: state = 3498; break; // &long -> &longl
				case 3579: state = 3580; break; // &looparrow -> &looparrowl
				case 3589: state = 3595; break; // &lop -> &lopl
				case 3640: state = 3641; break; // &lpar -> &lparl
				case 3692: state = 3708; break; // &lt -> &ltl
				case 3745: state = 3855; break; // &m -> &ml
				case 3746: state = 3749; break; // &ma -> &mal
				case 3761: state = 3766; break; // &mapsto -> &mapstol
				case 3801: state = 3802; break; // &measuredang -> &measuredangl
				case 3804: state = 3814; break; // &Me -> &Mel
				case 3814: state = 3815; break; // &Mel -> &Mell
				case 3851: state = 3852; break; // &MinusP -> &MinusPl
				case 3861: state = 3862; break; // &mnp -> &mnpl
				case 3867: state = 3868; break; // &mode -> &model
				case 3887: state = 3888; break; // &mu -> &mul
				case 3897: state = 4121; break; // &n -> &nl
				case 3899: state = 3900; break; // &nab -> &nabl
				case 3927: state = 3928; break; // &natura -> &natural
				case 3950: state = 3951; break; // &Ncedi -> &Ncedil
				case 3954: state = 3955; break; // &ncedi -> &ncedil
				case 4086: state = 4087; break; // &ngeqs -> &ngeqsl
				case 4132: state = 4177; break; // &nL -> &nLl
				case 4170: state = 4171; break; // &nleqs -> &nleqsl
				case 4234: state = 4235; break; // &NotDoub -> &NotDoubl
				case 4243: state = 4244; break; // &NotDoubleVertica -> &NotDoubleVertical
				case 4248: state = 4249; break; // &NotE -> &NotEl
				case 4257: state = 4258; break; // &NotEqua -> &NotEqual
				case 4260: state = 4261; break; // &NotEqualTi -> &NotEqualTil
				case 4279: state = 4280; break; // &NotGreaterEqua -> &NotGreaterEqual
				case 4282: state = 4283; break; // &NotGreaterFu -> &NotGreaterFul
				case 4283: state = 4284; break; // &NotGreaterFul -> &NotGreaterFull
				case 4288: state = 4289; break; // &NotGreaterFullEqua -> &NotGreaterFullEqual
				case 4301: state = 4302; break; // &NotGreaterS -> &NotGreaterSl
				case 4309: state = 4310; break; // &NotGreaterSlantEqua -> &NotGreaterSlantEqual
				case 4312: state = 4313; break; // &NotGreaterTi -> &NotGreaterTil
				case 4331: state = 4332; break; // &NotHumpEqua -> &NotHumpEqual
				case 4352: state = 4353; break; // &NotLeftTriang -> &NotLeftTriangl
				case 4361: state = 4362; break; // &NotLeftTriangleEqua -> &NotLeftTriangleEqual
				case 4368: state = 4369; break; // &NotLessEqua -> &NotLessEqual
				case 4381: state = 4382; break; // &NotLessS -> &NotLessSl
				case 4389: state = 4390; break; // &NotLessSlantEqua -> &NotLessSlantEqual
				case 4392: state = 4393; break; // &NotLessTi -> &NotLessTil
				case 4441: state = 4442; break; // &NotPrecedesEqua -> &NotPrecedesEqual
				case 4443: state = 4444; break; // &NotPrecedesS -> &NotPrecedesSl
				case 4451: state = 4452; break; // &NotPrecedesSlantEqua -> &NotPrecedesSlantEqual
				case 4460: state = 4461; break; // &NotReverseE -> &NotReverseEl
				case 4476: state = 4477; break; // &NotRightTriang -> &NotRightTriangl
				case 4485: state = 4486; break; // &NotRightTriangleEqua -> &NotRightTriangleEqual
				case 4502: state = 4503; break; // &NotSquareSubsetEqua -> &NotSquareSubsetEqual
				case 4513: state = 4514; break; // &NotSquareSupersetEqua -> &NotSquareSupersetEqual
				case 4523: state = 4524; break; // &NotSubsetEqua -> &NotSubsetEqual
				case 4534: state = 4535; break; // &NotSucceedsEqua -> &NotSucceedsEqual
				case 4536: state = 4537; break; // &NotSucceedsS -> &NotSucceedsSl
				case 4544: state = 4545; break; // &NotSucceedsSlantEqua -> &NotSucceedsSlantEqual
				case 4547: state = 4548; break; // &NotSucceedsTi -> &NotSucceedsTil
				case 4560: state = 4561; break; // &NotSupersetEqua -> &NotSupersetEqual
				case 4563: state = 4564; break; // &NotTi -> &NotTil
				case 4570: state = 4571; break; // &NotTildeEqua -> &NotTildeEqual
				case 4573: state = 4574; break; // &NotTildeFu -> &NotTildeFul
				case 4574: state = 4575; break; // &NotTildeFul -> &NotTildeFull
				case 4579: state = 4580; break; // &NotTildeFullEqua -> &NotTildeFullEqual
				case 4582: state = 4583; break; // &NotTildeTi -> &NotTildeTil
				case 4592: state = 4593; break; // &NotVertica -> &NotVertical
				case 4600: state = 4601; break; // &npara -> &nparal
				case 4601: state = 4602; break; // &nparal -> &nparall
				case 4603: state = 4604; break; // &nparalle -> &nparallel
				case 4605: state = 4606; break; // &npars -> &nparsl
				case 4608: state = 4609; break; // &npo -> &npol
				case 4673: state = 4674; break; // &nshortpara -> &nshortparal
				case 4674: state = 4675; break; // &nshortparal -> &nshortparall
				case 4676: state = 4677; break; // &nshortparalle -> &nshortparallel
				case 4718: state = 4730; break; // &nt -> &ntl
				case 4719: state = 4720; break; // &ntg -> &ntgl
				case 4722: state = 4723; break; // &Nti -> &Ntil
				case 4726: state = 4727; break; // &nti -> &ntil
				case 4736: state = 4737; break; // &ntriang -> &ntriangl
				case 4738: state = 4739; break; // &ntriangle -> &ntrianglel
				case 4760: state = 4792; break; // &nv -> &nvl
				case 4833: state = 4908; break; // &o -> &ol
				case 4856: state = 4857; break; // &Odb -> &Odbl
				case 4860: state = 4861; break; // &odb -> &odbl
				case 4869: state = 4870; break; // &odso -> &odsol
				case 4872: state = 4873; break; // &OE -> &OEl
				case 4876: state = 4877; break; // &oe -> &oel
				case 4957: state = 4987; break; // &op -> &opl
				case 4965: state = 4966; break; // &OpenCur -> &OpenCurl
				case 4971: state = 4972; break; // &OpenCurlyDoub -> &OpenCurlyDoubl
				case 5008: state = 5009; break; // &ors -> &orsl
				case 5015: state = 5021; break; // &Os -> &Osl
				case 5018: state = 5025; break; // &os -> &osl
				case 5029: state = 5030; break; // &oso -> &osol
				case 5032: state = 5033; break; // &Oti -> &Otil
				case 5037: state = 5038; break; // &oti -> &otil
				case 5050: state = 5051; break; // &Oum -> &Ouml
				case 5053: state = 5054; break; // &oum -> &ouml
				case 5083: state = 5150; break; // &p -> &pl
				case 5086: state = 5087; break; // &para -> &paral
				case 5087: state = 5088; break; // &paral -> &parall
				case 5089: state = 5090; break; // &paralle -> &parallel
				case 5091: state = 5094; break; // &pars -> &parsl
				case 5096: state = 5172; break; // &P -> &Pl
				case 5101: state = 5102; break; // &Partia -> &Partial
				case 5117: state = 5118; break; // &permi -> &permil
				case 5196: state = 5197; break; // &Poincarep -> &Poincarepl
				case 5233: state = 5234; break; // &preccur -> &preccurl
				case 5247: state = 5248; break; // &PrecedesEqua -> &PrecedesEqual
				case 5249: state = 5250; break; // &PrecedesS -> &PrecedesSl
				case 5257: state = 5258; break; // &PrecedesSlantEqua -> &PrecedesSlantEqual
				case 5260: state = 5261; break; // &PrecedesTi -> &PrecedesTil
				case 5303: state = 5308; break; // &prof -> &profl
				case 5304: state = 5305; break; // &profa -> &profal
				case 5324: state = 5325; break; // &Proportiona -> &Proportional
				case 5333: state = 5334; break; // &prure -> &prurel
				case 5397: state = 5854; break; // &r -> &rl
				case 5426: state = 5429; break; // &rang -> &rangl
				case 5439: state = 5450; break; // &rarr -> &rarrl
				case 5452: state = 5453; break; // &rarrp -> &rarrpl
				case 5457: state = 5458; break; // &Rarrt -> &Rarrtl
				case 5459: state = 5460; break; // &rarrt -> &rarrtl
				case 5464: state = 5465; break; // &rAtai -> &rAtail
				case 5468: state = 5469; break; // &ratai -> &ratail
				case 5473: state = 5474; break; // &rationa -> &rational
				case 5498: state = 5499; break; // &rbrks -> &rbrksl
				case 5514: state = 5515; break; // &Rcedi -> &Rcedil
				case 5518: state = 5519; break; // &rcedi -> &rcedil
				case 5520: state = 5521; break; // &rcei -> &rceil
				case 5526: state = 5529; break; // &rd -> &rdl
				case 5542: state = 5543; break; // &rea -> &real
				case 5562: state = 5563; break; // &ReverseE -> &ReverseEl
				case 5571: state = 5572; break; // &ReverseEqui -> &ReverseEquil
				case 5584: state = 5585; break; // &ReverseUpEqui -> &ReverseUpEquil
				case 5592: state = 5597; break; // &rf -> &rfl
				case 5611: state = 5612; break; // &rharu -> &rharul
				case 5623: state = 5624; break; // &RightAng -> &RightAngl
				case 5645: state = 5725; break; // &right -> &rightl
				case 5665: state = 5666; break; // &rightarrowtai -> &rightarrowtail
				case 5669: state = 5670; break; // &RightCei -> &RightCeil
				case 5677: state = 5678; break; // &RightDoub -> &RightDoubl
				case 5707: state = 5708; break; // &RightF -> &RightFl
				case 5792: state = 5793; break; // &RightTriang -> &RightTriangl
				case 5801: state = 5802; break; // &RightTriangleEqua -> &RightTriangleEqual
				case 5884: state = 5891; break; // &rop -> &ropl
				case 5904: state = 5905; break; // &RoundImp -> &RoundImpl
				case 5915: state = 5916; break; // &rppo -> &rppol
				case 5961: state = 5964; break; // &rtri -> &rtril
				case 5968: state = 5969; break; // &Ru -> &Rul
				case 5972: state = 5973; break; // &RuleDe -> &RuleDel
				case 5978: state = 5979; break; // &ru -> &rul
				case 5991: state = 6188; break; // &s -> &sl
				case 6019: state = 6020; break; // &Scedi -> &Scedil
				case 6022: state = 6023; break; // &scedi -> &scedil
				case 6038: state = 6039; break; // &scpo -> &scpol
				case 6135: state = 6136; break; // &shortpara -> &shortparal
				case 6136: state = 6137; break; // &shortparal -> &shortparall
				case 6138: state = 6139; break; // &shortparalle -> &shortparallel
				case 6168: state = 6176; break; // &sim -> &siml
				case 6180: state = 6181; break; // &simp -> &simpl
				case 6193: state = 6194; break; // &Sma -> &Smal
				case 6194: state = 6195; break; // &Smal -> &Small
				case 6199: state = 6200; break; // &SmallCirc -> &SmallCircl
				case 6203: state = 6204; break; // &sma -> &smal
				case 6204: state = 6205; break; // &smal -> &small
				case 6221: state = 6222; break; // &smepars -> &smeparsl
				case 6223: state = 6225; break; // &smi -> &smil
				case 6235: state = 6240; break; // &so -> &sol
				case 6314: state = 6315; break; // &SquareSubsetEqua -> &SquareSubsetEqual
				case 6325: state = 6326; break; // &SquareSupersetEqua -> &SquareSupersetEqual
				case 6349: state = 6350; break; // &ssmi -> &ssmil
				case 6372: state = 6373; break; // &straightepsi -> &straightepsil
				case 6394: state = 6395; break; // &submu -> &submul
				case 6400: state = 6401; break; // &subp -> &subpl
				case 6420: state = 6421; break; // &SubsetEqua -> &SubsetEqual
				case 6441: state = 6442; break; // &succcur -> &succcurl
				case 6455: state = 6456; break; // &SucceedsEqua -> &SucceedsEqual
				case 6457: state = 6458; break; // &SucceedsS -> &SucceedsSl
				case 6465: state = 6466; break; // &SucceedsSlantEqua -> &SucceedsSlantEqual
				case 6468: state = 6469; break; // &SucceedsTi -> &SucceedsTil
				case 6500: state = 6531; break; // &sup -> &supl
				case 6523: state = 6524; break; // &SupersetEqua -> &SupersetEqual
				case 6527: state = 6528; break; // &suphso -> &suphsol
				case 6536: state = 6537; break; // &supmu -> &supmul
				case 6542: state = 6543; break; // &supp -> &suppl
				case 6579: state = 6580; break; // &sz -> &szl
				case 6609: state = 6610; break; // &Tcedi -> &Tcedil
				case 6613: state = 6614; break; // &tcedi -> &tcedil
				case 6620: state = 6621; break; // &te -> &tel
				case 6696: state = 6697; break; // &Ti -> &Til
				case 6700: state = 6701; break; // &ti -> &til
				case 6707: state = 6708; break; // &TildeEqua -> &TildeEqual
				case 6710: state = 6711; break; // &TildeFu -> &TildeFul
				case 6711: state = 6712; break; // &TildeFul -> &TildeFull
				case 6716: state = 6717; break; // &TildeFullEqua -> &TildeFullEqual
				case 6719: state = 6720; break; // &TildeTi -> &TildeTil
				case 6767: state = 6768; break; // &triang -> &triangl
				case 6769: state = 6774; break; // &triangle -> &trianglel
				case 6799: state = 6800; break; // &Trip -> &Tripl
				case 6805: state = 6806; break; // &trip -> &tripl
				case 6853: state = 6854; break; // &twohead -> &twoheadl
				case 6879: state = 6965; break; // &u -> &ul
				case 6925: state = 6926; break; // &Udb -> &Udbl
				case 6929: state = 6930; break; // &udb -> &udbl
				case 6959: state = 6960; break; // &uhar -> &uharl
				case 6962: state = 6963; break; // &uhb -> &uhbl
				case 6982: state = 6986; break; // &um -> &uml
				case 7015: state = 7016; break; // &UnionP -> &UnionPl
				case 7042: state = 7114; break; // &up -> &upl
				case 7090: state = 7091; break; // &UpEqui -> &UpEquil
				case 7104: state = 7105; break; // &upharpoon -> &upharpoonl
				case 7140: state = 7144; break; // &Upsi -> &Upsil
				case 7142: state = 7147; break; // &upsi -> &upsil
				case 7197: state = 7198; break; // &Uti -> &Util
				case 7201: state = 7202; break; // &uti -> &util
				case 7213: state = 7214; break; // &Uum -> &Uuml
				case 7215: state = 7216; break; // &uum -> &uuml
				case 7220: state = 7221; break; // &uwang -> &uwangl
				case 7223: state = 7398; break; // &v -> &vl
				case 7233: state = 7234; break; // &varepsi -> &varepsil
				case 7295: state = 7296; break; // &vartriang -> &vartriangl
				case 7297: state = 7298; break; // &vartriangle -> &vartrianglel
				case 7326: state = 7335; break; // &Vdash -> &Vdashl
				case 7338: state = 7345; break; // &ve -> &vel
				case 7345: state = 7346; break; // &vel -> &vell
				case 7361: state = 7362; break; // &Vertica -> &Vertical
				case 7380: state = 7381; break; // &VerticalTi -> &VerticalTil
				case 7495: state = 7522; break; // &x -> &xl
				case 7542: state = 7544; break; // &xop -> &xopl
				case 7569: state = 7570; break; // &xup -> &xupl
				case 7641: state = 7642; break; // &Yum -> &Yuml
				case 7643: state = 7644; break; // &yum -> &yuml
				default: return false;
				}
				break;
			case 'm':
				switch (state) {
				case 0: state = 3745; break; // & -> &m
				case 1: state = 75; break; // &A -> &Am
				case 7: state = 79; break; // &a -> &am
				case 64: state = 65; break; // &alefsy -> &alefsym
				case 102: state = 106; break; // &ang -> &angm
				case 191: state = 192; break; // &asy -> &asym
				case 206: state = 207; break; // &Au -> &Aum
				case 209: state = 210; break; // &au -> &aum
				case 239: state = 240; break; // &backpri -> &backprim
				case 243: state = 244; break; // &backsi -> &backsim
				case 288: state = 300; break; // &be -> &bem
				case 352: state = 353; break; // &bigoti -> &bigotim
				case 464: state = 465; break; // &botto -> &bottom
				case 470: state = 494; break; // &box -> &boxm
				case 504: state = 505; break; // &boxti -> &boxtim
				case 534: state = 535; break; // &bpri -> &bprim
				case 555: state = 556; break; // &bse -> &bsem
				case 558: state = 559; break; // &bsi -> &bsim
				case 568: state = 573; break; // &bu -> &bum
				case 577: state = 578; break; // &Bu -> &Bum
				case 675: state = 676; break; // &ccupss -> &ccupssm
				case 683: state = 693; break; // &ce -> &cem
				case 724: state = 725; break; // &check -> &checkm
				case 733: state = 796; break; // &cir -> &cirm
				case 785: state = 786; break; // &CircleTi -> &CircleTim
				case 860: state = 867; break; // &co -> &com
				case 867: state = 868; break; // &com -> &comm
				case 875: state = 876; break; // &comple -> &complem
				case 1029: state = 1030; break; // &curarr -> &curarrm
				case 1161: state = 1170; break; // &de -> &dem
				case 1192: state = 1231; break; // &Dia -> &Diam
				case 1229: state = 1230; break; // &dia -> &diam
				case 1256: state = 1257; break; // &diga -> &digam
				case 1257: state = 1258; break; // &digam -> &digamm
				case 1270: state = 1271; break; // &divideonti -> &divideontim
				case 1302: state = 1316; break; // &dot -> &dotm
				case 1656: state = 1746; break; // &E -> &Em
				case 1662: state = 1750; break; // &e -> &em
				case 1730: state = 1731; break; // &Ele -> &Elem
				case 1763: state = 1764; break; // &EmptyS -> &EmptySm
				case 1779: state = 1780; break; // &EmptyVeryS -> &EmptyVerySm
				case 1843: state = 1844; break; // &eqsi -> &eqsim
				case 1878: state = 1879; break; // &Equilibriu -> &Equilibrium
				case 1906: state = 1907; break; // &Esi -> &Esim
				case 1908: state = 1909; break; // &esi -> &esim
				case 1917: state = 1918; break; // &Eu -> &Eum
				case 1920: state = 1921; break; // &eu -> &eum
				case 1982: state = 1983; break; // &fe -> &fem
				case 2010: state = 2011; break; // &FilledS -> &FilledSm
				case 2025: state = 2026; break; // &FilledVeryS -> &FilledVerySm
				case 2119: state = 2129; break; // &ga -> &gam
				case 2125: state = 2126; break; // &Ga -> &Gam
				case 2126: state = 2127; break; // &Gam -> &Gamm
				case 2129: state = 2130; break; // &gam -> &gamm
				case 2194: state = 2195; break; // &gi -> &gim
				case 2220: state = 2221; break; // &gnsi -> &gnsim
				case 2288: state = 2289; break; // &gsi -> &gsim
				case 2339: state = 2340; break; // &gtrsi -> &gtrsim
				case 2357: state = 2364; break; // &ha -> &ham
				case 2440: state = 2444; break; // &ho -> &hom
				case 2508: state = 2509; break; // &Hu -> &Hum
				case 2516: state = 2517; break; // &HumpDownHu -> &HumpDownHum
				case 2533: state = 2604; break; // &I -> &Im
				case 2539: state = 2608; break; // &i -> &im
				case 2701: state = 2702; break; // &InvisibleCo -> &InvisibleCom
				case 2702: state = 2703; break; // &InvisibleCom -> &InvisibleComm
				case 2706: state = 2707; break; // &InvisibleTi -> &InvisibleTim
				case 2765: state = 2773; break; // &Iu -> &Ium
				case 2769: state = 2775; break; // &iu -> &ium
				case 2782: state = 2793; break; // &j -> &jm
				case 2881: state = 3439; break; // &l -> &lm
				case 2886: state = 3434; break; // &L -> &Lm
				case 2887: state = 2907; break; // &La -> &Lam
				case 2892: state = 2911; break; // &la -> &lam
				case 2897: state = 2898; break; // &lae -> &laem
				case 2952: state = 2953; break; // &larrsi -> &larrsim
				case 3224: state = 3225; break; // &leftthreeti -> &leftthreetim
				case 3359: state = 3360; break; // &lesssi -> &lesssim
				case 3464: state = 3465; break; // &lnsi -> &lnsim
				case 3497: state = 3537; break; // &long -> &longm
				case 3599: state = 3600; break; // &loti -> &lotim
				case 3643: state = 3657; break; // &lr -> &lrm
				case 3673: state = 3674; break; // &lsi -> &lsim
				case 3704: state = 3705; break; // &lti -> &ltim
				case 3777: state = 3778; break; // &mco -> &mcom
				case 3778: state = 3779; break; // &mcom -> &mcomm
				case 3807: state = 3808; break; // &Mediu -> &Medium
				case 3887: state = 3894; break; // &mu -> &mum
				case 3890: state = 3891; break; // &multi -> &multim
				case 3897: state = 4187; break; // &n -> &nm
				case 3933: state = 3934; break; // &nbu -> &nbum
				case 3995: state = 3996; break; // &NegativeMediu -> &NegativeMedium
				case 4039: state = 4040; break; // &nesi -> &nesim
				case 4095: state = 4096; break; // &ngsi -> &ngsim
				case 4179: state = 4180; break; // &nlsi -> &nlsim
				case 4250: state = 4251; break; // &NotEle -> &NotElem
				case 4317: state = 4318; break; // &NotHu -> &NotHum
				case 4325: state = 4326; break; // &NotHumpDownHu -> &NotHumpDownHum
				case 4462: state = 4463; break; // &NotReverseEle -> &NotReverseElem
				case 4653: state = 4682; break; // &ns -> &nsm
				case 4666: state = 4667; break; // &nshort -> &nshortm
				case 4678: state = 4679; break; // &nsi -> &nsim
				case 4753: state = 4754; break; // &nu -> &num
				case 4810: state = 4811; break; // &nvsi -> &nvsim
				case 4827: state = 4923; break; // &O -> &Om
				case 4833: state = 4927; break; // &o -> &om
				case 4900: state = 4904; break; // &oh -> &ohm
				case 4995: state = 5001; break; // &ord -> &ordm
				case 5032: state = 5041; break; // &Oti -> &Otim
				case 5037: state = 5044; break; // &oti -> &otim
				case 5049: state = 5050; break; // &Ou -> &Oum
				case 5052: state = 5053; break; // &ou -> &oum
				case 5083: state = 5188; break; // &p -> &pm
				case 5092: state = 5093; break; // &parsi -> &parsim
				case 5109: state = 5116; break; // &per -> &perm
				case 5130: state = 5133; break; // &ph -> &phm
				case 5133: state = 5134; break; // &phm -> &phmm
				case 5159: state = 5180; break; // &plus -> &plusm
				case 5183: state = 5184; break; // &plussi -> &plussim
				case 5277: state = 5278; break; // &precnsi -> &precnsim
				case 5280: state = 5281; break; // &precsi -> &precsim
				case 5282: state = 5283; break; // &Pri -> &Prim
				case 5285: state = 5286; break; // &pri -> &prim
				case 5294: state = 5295; break; // &prnsi -> &prnsim
				case 5329: state = 5330; break; // &prsi -> &prsim
				case 5365: state = 5366; break; // &qpri -> &qprim
				case 5397: state = 5862; break; // &r -> &rm
				case 5417: state = 5418; break; // &rae -> &raem
				case 5455: state = 5456; break; // &rarrsi -> &rarrsim
				case 5564: state = 5565; break; // &ReverseEle -> &ReverseElem
				case 5577: state = 5578; break; // &ReverseEquilibriu -> &ReverseEquilibrium
				case 5590: state = 5591; break; // &ReverseUpEquilibriu -> &ReverseUpEquilibrium
				case 5784: state = 5785; break; // &rightthreeti -> &rightthreetim
				case 5854: state = 5861; break; // &rl -> &rlm
				case 5871: state = 5872; break; // &rn -> &rnm
				case 5895: state = 5896; break; // &roti -> &rotim
				case 5902: state = 5903; break; // &RoundI -> &RoundIm
				case 5956: state = 5957; break; // &rti -> &rtim
				case 5985: state = 6192; break; // &S -> &Sm
				case 5991: state = 6202; break; // &s -> &sm
				case 6035: state = 6036; break; // &scnsi -> &scnsim
				case 6044: state = 6045; break; // &scsi -> &scsim
				case 6053: state = 6066; break; // &se -> &sem
				case 6072: state = 6073; break; // &set -> &setm
				case 6128: state = 6129; break; // &short -> &shortm
				case 6159: state = 6160; break; // &Sig -> &Sigm
				case 6162: state = 6168; break; // &si -> &sim
				case 6163: state = 6164; break; // &sig -> &sigm
				case 6208: state = 6209; break; // &smallset -> &smallsetm
				case 6341: state = 6348; break; // &ss -> &ssm
				case 6345: state = 6346; break; // &sset -> &ssetm
				case 6381: state = 6495; break; // &Su -> &Sum
				case 6383: state = 6496; break; // &su -> &sum
				case 6384: state = 6393; break; // &sub -> &subm
				case 6426: state = 6427; break; // &subsi -> &subsim
				case 6485: state = 6486; break; // &succnsi -> &succnsim
				case 6488: state = 6489; break; // &succsi -> &succsim
				case 6500: state = 6535; break; // &sup -> &supm
				case 6559: state = 6560; break; // &supsi -> &supsim
				case 6651: state = 6652; break; // &thetasy -> &thetasym
				case 6664: state = 6665; break; // &thicksi -> &thicksim
				case 6687: state = 6688; break; // &thksi -> &thksim
				case 6700: state = 6723; break; // &ti -> &tim
				case 6753: state = 6754; break; // &tpri -> &tprim
				case 6764: state = 6792; break; // &tri -> &trim
				case 6812: state = 6813; break; // &triti -> &tritim
				case 6819: state = 6820; break; // &trpeziu -> &trpezium
				case 6873: state = 6978; break; // &U -> &Um
				case 6879: state = 6982; break; // &u -> &um
				case 7096: state = 7097; break; // &UpEquilibriu -> &UpEquilibrium
				case 7208: state = 7215; break; // &uu -> &uum
				case 7212: state = 7213; break; // &Uu -> &Uum
				case 7266: state = 7267; break; // &varsig -> &varsigm
				case 7495: state = 7529; break; // &x -> &xm
				case 7548: state = 7549; break; // &xoti -> &xotim
				case 7637: state = 7643; break; // &yu -> &yum
				case 7640: state = 7641; break; // &Yu -> &Yum
				default: return false;
				}
				break;
			case 'n':
				switch (state) {
				case 0: state = 3897; break; // & -> &n
				case 1: state = 88; break; // &A -> &An
				case 7: state = 90; break; // &a -> &an
				case 92: state = 93; break; // &anda -> &andan
				case 133: state = 134; break; // &Aogo -> &Aogon
				case 137: state = 138; break; // &aogo -> &aogon
				case 159: state = 160; break; // &ApplyFu -> &ApplyFun
				case 164: state = 165; break; // &ApplyFunctio -> &ApplyFunction
				case 173: state = 174; break; // &Ari -> &Arin
				case 177: state = 178; break; // &ari -> &arin
				case 188: state = 189; break; // &Assig -> &Assign
				case 214: state = 215; break; // &awco -> &awcon
				case 216: state = 217; break; // &awconi -> &awconin
				case 219: state = 220; break; // &awi -> &awin
				case 222: state = 445; break; // &b -> &bn
				case 227: state = 228; break; // &backco -> &backcon
				case 235: state = 236; break; // &backepsilo -> &backepsilon
				case 278: state = 279; break; // &bco -> &bcon
				case 308: state = 309; break; // &ber -> &bern
				case 312: state = 313; break; // &Ber -> &Bern
				case 327: state = 328; break; // &betwee -> &between
				case 367: state = 368; break; // &bigtria -> &bigtrian
				case 374: state = 375; break; // &bigtriangledow -> &bigtriangledown
				case 397: state = 434; break; // &bla -> &blan
				case 403: state = 404; break; // &blackloze -> &blacklozen
				case 416: state = 417; break; // &blacktria -> &blacktrian
				case 423: state = 424; break; // &blacktriangledow -> &blacktriangledown
				case 495: state = 496; break; // &boxmi -> &boxmin
				case 597: state = 598; break; // &capa -> &capan
				case 623: state = 624; break; // &CapitalDiffere -> &CapitalDifferen
				case 634: state = 635; break; // &caro -> &caron
				case 648: state = 649; break; // &Ccaro -> &Ccaron
				case 651: state = 652; break; // &ccaro -> &ccaron
				case 667: state = 668; break; // &Cco -> &Ccon
				case 669: state = 670; break; // &Cconi -> &Cconin
				case 683: state = 698; break; // &ce -> &cen
				case 687: state = 700; break; // &Ce -> &Cen
				case 776: state = 777; break; // &CircleMi -> &CircleMin
				case 791: state = 792; break; // &cirf -> &cirfn
				case 793: state = 794; break; // &cirfni -> &cirfnin
				case 812: state = 813; break; // &ClockwiseCo -> &ClockwiseCon
				case 818: state = 819; break; // &ClockwiseContourI -> &ClockwiseContourIn
				case 856: state = 888; break; // &Co -> &Con
				case 858: state = 859; break; // &Colo -> &Colon
				case 860: state = 883; break; // &co -> &con
				case 862: state = 863; break; // &colo -> &colon
				case 872: state = 873; break; // &compf -> &compfn
				case 877: state = 878; break; // &compleme -> &complemen
				case 892: state = 893; break; // &Congrue -> &Congruen
				case 895: state = 896; break; // &Coni -> &Conin
				case 898: state = 899; break; // &coni -> &conin
				case 905: state = 906; break; // &ContourI -> &ContourIn
				case 932: state = 933; break; // &Cou -> &Coun
				case 947: state = 948; break; // &CounterClockwiseCo -> &CounterClockwiseCon
				case 953: state = 954; break; // &CounterClockwiseContourI -> &CounterClockwiseContourIn
				case 1052: state = 1053; break; // &curre -> &curren
				case 1078: state = 1079; break; // &cwco -> &cwcon
				case 1080: state = 1081; break; // &cwconi -> &cwconin
				case 1083: state = 1084; break; // &cwi -> &cwin
				case 1132: state = 1133; break; // &Dcaro -> &Dcaron
				case 1137: state = 1138; break; // &dcaro -> &dcaron
				case 1232: state = 1233; break; // &Diamo -> &Diamon
				case 1235: state = 1236; break; // &diamo -> &diamon
				case 1248: state = 1249; break; // &Differe -> &Differen
				case 1261: state = 1262; break; // &disi -> &disin
				case 1267: state = 1268; break; // &divideo -> &divideon
				case 1274: state = 1275; break; // &divo -> &divon
				case 1286: state = 1287; break; // &dlcor -> &dlcorn
				case 1317: state = 1318; break; // &dotmi -> &dotmin
				case 1348: state = 1349; break; // &DoubleCo -> &DoubleCon
				case 1354: state = 1355; break; // &DoubleContourI -> &DoubleContourIn
				case 1365: state = 1366; break; // &DoubleDow -> &DoubleDown
				case 1394: state = 1395; break; // &DoubleLo -> &DoubleLon
				case 1448: state = 1449; break; // &DoubleUpDow -> &DoubleUpDown
				case 1466: state = 1467; break; // &Dow -> &Down
				case 1478: state = 1479; break; // &dow -> &down
				case 1502: state = 1503; break; // &downdow -> &downdown
				case 1515: state = 1516; break; // &downharpoo -> &downharpoon
				case 1599: state = 1600; break; // &drcor -> &drcorn
				case 1639: state = 1640; break; // &dwa -> &dwan
				case 1662: state = 1797; break; // &e -> &en
				case 1675: state = 1676; break; // &Ecaro -> &Ecaron
				case 1680: state = 1681; break; // &ecaro -> &ecaron
				case 1690: state = 1691; break; // &ecolo -> &ecolon
				case 1732: state = 1733; break; // &Eleme -> &Elemen
				case 1735: state = 1736; break; // &eli -> &elin
				case 1803: state = 1804; break; // &Eogo -> &Eogon
				case 1807: state = 1808; break; // &eogo -> &eogon
				case 1827: state = 1828; break; // &Epsilo -> &Epsilon
				case 1830: state = 1831; break; // &epsilo -> &epsilon
				case 1840: state = 1841; break; // &eqcolo -> &eqcolon
				case 1846: state = 1847; break; // &eqsla -> &eqslan
				case 1943: state = 1944; break; // &expectatio -> &expectation
				case 1946: state = 1947; break; // &Expo -> &Expon
				case 1948: state = 1949; break; // &Expone -> &Exponen
				case 1955: state = 1956; break; // &expo -> &expon
				case 1957: state = 1958; break; // &expone -> &exponen
				case 1964: state = 2049; break; // &f -> &fn
				case 1968: state = 1969; break; // &falli -> &fallin
				case 2046: state = 2047; break; // &flt -> &fltn
				case 2080: state = 2081; break; // &fparti -> &fpartin
				case 2110: state = 2111; break; // &frow -> &frown
				case 2118: state = 2208; break; // &g -> &gn
				case 2173: state = 2174; break; // &geqsla -> &geqslan
				case 2269: state = 2270; break; // &GreaterSla -> &GreaterSlan
				case 2341: state = 2349; break; // &gv -> &gvn
				case 2344: state = 2345; break; // &gvert -> &gvertn
				case 2411: state = 2412; break; // &herco -> &hercon
				case 2481: state = 2482; break; // &Horizo -> &Horizon
				case 2487: state = 2488; break; // &HorizontalLi -> &HorizontalLin
				case 2513: state = 2514; break; // &HumpDow -> &HumpDown
				case 2531: state = 2532; break; // &hyphe -> &hyphen
				case 2533: state = 2656; break; // &I -> &In
				case 2539: state = 2641; break; // &i -> &in
				case 2582: state = 2589; break; // &ii -> &iin
				case 2583: state = 2587; break; // &iii -> &iiin
				case 2584: state = 2585; break; // &iiii -> &iiiin
				case 2591: state = 2592; break; // &iinfi -> &iinfin
				case 2615: state = 2616; break; // &Imagi -> &Imagin
				case 2622: state = 2623; break; // &imagli -> &imaglin
				case 2647: state = 2648; break; // &infi -> &infin
				case 2682: state = 2683; break; // &Intersectio -> &Intersection
				case 2718: state = 2719; break; // &Iogo -> &Iogon
				case 2721: state = 2722; break; // &iogo -> &iogon
				case 2746: state = 2747; break; // &isi -> &isin
				case 2855: state = 2856; break; // &kgree -> &kgreen
				case 2881: state = 3452; break; // &l -> &ln
				case 2887: state = 2915; break; // &La -> &Lan
				case 2892: state = 2917; break; // &la -> &lan
				case 2905: state = 2906; break; // &lagra -> &lagran
				case 2991: state = 2992; break; // &Lcaro -> &Lcaron
				case 2996: state = 2997; break; // &lcaro -> &lcaron
				case 3036: state = 3037; break; // &LeftA -> &LeftAn
				case 3085: state = 3086; break; // &LeftCeili -> &LeftCeilin
				case 3101: state = 3102; break; // &LeftDow -> &LeftDown
				case 3131: state = 3132; break; // &leftharpoo -> &leftharpoon
				case 3135: state = 3136; break; // &leftharpoondow -> &leftharpoondown
				case 3185: state = 3186; break; // &leftrightharpoo -> &leftrightharpoon
				case 3230: state = 3231; break; // &LeftTria -> &LeftTrian
				case 3247: state = 3248; break; // &LeftUpDow -> &LeftUpDown
				case 3288: state = 3289; break; // &leqsla -> &leqslan
				case 3363: state = 3364; break; // &LessSla -> &LessSlan
				case 3415: state = 3416; break; // &llcor -> &llcorn
				case 3466: state = 3496; break; // &lo -> &lon
				case 3467: state = 3468; break; // &loa -> &loan
				case 3475: state = 3476; break; // &Lo -> &Lon
				case 3633: state = 3634; break; // &loze -> &lozen
				case 3649: state = 3650; break; // &lrcor -> &lrcorn
				case 3735: state = 3743; break; // &lv -> &lvn
				case 3738: state = 3739; break; // &lvert -> &lvertn
				case 3745: state = 3860; break; // &m -> &mn
				case 3764: state = 3765; break; // &mapstodow -> &mapstodown
				case 3799: state = 3800; break; // &measureda -> &measuredan
				case 3816: state = 3817; break; // &Melli -> &Mellin
				case 3827: state = 3841; break; // &mi -> &min
				case 3847: state = 3848; break; // &Mi -> &Min
				case 3898: state = 3912; break; // &na -> &nan
				case 3943: state = 3944; break; // &Ncaro -> &Ncaron
				case 3946: state = 3947; break; // &ncaro -> &ncaron
				case 3956: state = 3957; break; // &nco -> &ncon
				case 4004: state = 4012; break; // &NegativeThi -> &NegativeThin
				case 4024: state = 4025; break; // &NegativeVeryThi -> &NegativeVeryThin
				case 4069: state = 4070; break; // &NewLi -> &NewLin
				case 4088: state = 4089; break; // &ngeqsla -> &ngeqslan
				case 4172: state = 4173; break; // &nleqsla -> &nleqslan
				case 4190: state = 4196; break; // &No -> &Non
				case 4202: state = 4203; break; // &NonBreaki -> &NonBreakin
				case 4216: state = 4424; break; // &not -> &notn
				case 4218: state = 4219; break; // &NotCo -> &NotCon
				case 4223: state = 4224; break; // &NotCongrue -> &NotCongruen
				case 4252: state = 4253; break; // &NotEleme -> &NotElemen
				case 4303: state = 4304; break; // &NotGreaterSla -> &NotGreaterSlan
				case 4322: state = 4323; break; // &NotHumpDow -> &NotHumpDown
				case 4333: state = 4334; break; // &noti -> &notin
				case 4350: state = 4351; break; // &NotLeftTria -> &NotLeftTrian
				case 4383: state = 4384; break; // &NotLessSla -> &NotLessSlan
				case 4445: state = 4446; break; // &NotPrecedesSla -> &NotPrecedesSlan
				case 4464: state = 4465; break; // &NotReverseEleme -> &NotReverseElemen
				case 4474: state = 4475; break; // &NotRightTria -> &NotRightTrian
				case 4538: state = 4539; break; // &NotSucceedsSla -> &NotSucceedsSlan
				case 4610: state = 4611; break; // &npoli -> &npolin
				case 4734: state = 4735; break; // &ntria -> &ntrian
				case 4787: state = 4788; break; // &nvi -> &nvin
				case 4790: state = 4791; break; // &nvinfi -> &nvinfin
				case 4812: state = 4823; break; // &nw -> &nwn
				case 4888: state = 4889; break; // &ogo -> &ogon
				case 4905: state = 4906; break; // &oi -> &oin
				case 4919: state = 4920; break; // &oli -> &olin
				case 4940: state = 4941; break; // &Omicro -> &Omicron
				case 4942: state = 4948; break; // &omi -> &omin
				case 4945: state = 4946; break; // &omicro -> &omicron
				case 4961: state = 4962; break; // &Ope -> &Open
				case 5075: state = 5076; break; // &OverPare -> &OverParen
				case 5110: state = 5111; break; // &perc -> &percn
				case 5121: state = 5122; break; // &perte -> &perten
				case 5137: state = 5138; break; // &pho -> &phon
				case 5151: state = 5152; break; // &pla -> &plan
				case 5176: state = 5177; break; // &PlusMi -> &PlusMin
				case 5180: state = 5181; break; // &plusm -> &plusmn
				case 5190: state = 5191; break; // &Poi -> &Poin
				case 5198: state = 5199; break; // &Poincarepla -> &Poincareplan
				case 5202: state = 5203; break; // &poi -> &poin
				case 5205: state = 5206; break; // &pointi -> &pointin
				case 5212: state = 5213; break; // &pou -> &poun
				case 5216: state = 5289; break; // &pr -> &prn
				case 5224: state = 5266; break; // &prec -> &precn
				case 5251: state = 5252; break; // &PrecedesSla -> &PrecedesSlan
				case 5309: state = 5310; break; // &profli -> &proflin
				case 5322: state = 5323; break; // &Proportio -> &Proportion
				case 5343: state = 5344; break; // &pu -> &pun
				case 5354: state = 5355; break; // &qi -> &qin
				case 5378: state = 5379; break; // &quater -> &quatern
				case 5381: state = 5382; break; // &quaternio -> &quaternion
				case 5384: state = 5385; break; // &quati -> &quatin
				case 5397: state = 5871; break; // &r -> &rn
				case 5402: state = 5425; break; // &ra -> &ran
				case 5406: state = 5423; break; // &Ra -> &Ran
				case 5471: state = 5472; break; // &ratio -> &ration
				case 5505: state = 5506; break; // &Rcaro -> &Rcaron
				case 5510: state = 5511; break; // &rcaro -> &rcaron
				case 5544: state = 5545; break; // &reali -> &realin
				case 5566: state = 5567; break; // &ReverseEleme -> &ReverseElemen
				case 5621: state = 5622; break; // &RightA -> &RightAn
				case 5642: state = 5842; break; // &ri -> &rin
				case 5671: state = 5672; break; // &RightCeili -> &RightCeilin
				case 5687: state = 5688; break; // &RightDow -> &RightDown
				case 5717: state = 5718; break; // &rightharpoo -> &rightharpoon
				case 5721: state = 5722; break; // &rightharpoondow -> &rightharpoondown
				case 5740: state = 5741; break; // &rightleftharpoo -> &rightleftharpoon
				case 5790: state = 5791; break; // &RightTria -> &RightTrian
				case 5807: state = 5808; break; // &RightUpDow -> &RightUpDown
				case 5845: state = 5846; break; // &risi -> &risin
				case 5876: state = 5877; break; // &roa -> &roan
				case 5899: state = 5900; break; // &Rou -> &Roun
				case 5917: state = 5918; break; // &rppoli -> &rppolin
				case 6002: state = 6030; break; // &sc -> &scn
				case 6007: state = 6008; break; // &Scaro -> &Scaron
				case 6010: state = 6011; break; // &scaro -> &scaron
				case 6040: state = 6041; break; // &scpoli -> &scpolin
				case 6073: state = 6078; break; // &setm -> &setmn
				case 6074: state = 6075; break; // &setmi -> &setmin
				case 6086: state = 6087; break; // &sfrow -> &sfrown
				case 6110: state = 6111; break; // &ShortDow -> &ShortDown
				case 6168: state = 6178; break; // &sim -> &simn
				case 6210: state = 6211; break; // &smallsetmi -> &smallsetmin
				case 6293: state = 6294; break; // &SquareI -> &SquareIn
				case 6303: state = 6304; break; // &SquareIntersectio -> &SquareIntersection
				case 6327: state = 6328; break; // &SquareU -> &SquareUn
				case 6330: state = 6331; break; // &SquareUnio -> &SquareUnion
				case 6346: state = 6347; break; // &ssetm -> &ssetmn
				case 6363: state = 6379; break; // &str -> &strn
				case 6374: state = 6375; break; // &straightepsilo -> &straightepsilon
				case 6383: state = 6497; break; // &su -> &sun
				case 6384: state = 6397; break; // &sub -> &subn
				case 6413: state = 6422; break; // &subset -> &subsetn
				case 6432: state = 6474; break; // &succ -> &succn
				case 6459: state = 6460; break; // &SucceedsSla -> &SucceedsSlan
				case 6500: state = 6539; break; // &sup -> &supn
				case 6551: state = 6555; break; // &supset -> &supsetn
				case 6564: state = 6575; break; // &sw -> &swn
				case 6600: state = 6601; break; // &Tcaro -> &Tcaron
				case 6605: state = 6606; break; // &tcaro -> &tcaron
				case 6654: state = 6674; break; // &thi -> &thin
				case 6666: state = 6677; break; // &Thi -> &Thin
				case 6694: state = 6695; break; // &thor -> &thorn
				case 6700: state = 6730; break; // &ti -> &tin
				case 6765: state = 6766; break; // &tria -> &trian
				case 6772: state = 6773; break; // &triangledow -> &triangledown
				case 6793: state = 6794; break; // &trimi -> &trimin
				case 6873: state = 6987; break; // &U -> &Un
				case 6968: state = 6969; break; // &ulcor -> &ulcorn
				case 7004: state = 7005; break; // &UnderPare -> &UnderParen
				case 7013: state = 7014; break; // &Unio -> &Union
				case 7021: state = 7022; break; // &Uogo -> &Uogon
				case 7025: state = 7026; break; // &uogo -> &uogon
				case 7053: state = 7054; break; // &UpArrowDow -> &UpArrowDown
				case 7062: state = 7063; break; // &UpDow -> &UpDown
				case 7071: state = 7072; break; // &Updow -> &Updown
				case 7080: state = 7081; break; // &updow -> &updown
				case 7103: state = 7104; break; // &upharpoo -> &upharpoon
				case 7145: state = 7146; break; // &Upsilo -> &Upsilon
				case 7148: state = 7149; break; // &upsilo -> &upsilon
				case 7169: state = 7170; break; // &urcor -> &urcorn
				case 7177: state = 7178; break; // &Uri -> &Urin
				case 7180: state = 7181; break; // &uri -> &urin
				case 7218: state = 7219; break; // &uwa -> &uwan
				case 7223: state = 7402; break; // &v -> &vn
				case 7224: state = 7225; break; // &va -> &van
				case 7229: state = 7242; break; // &var -> &varn
				case 7235: state = 7236; break; // &varepsilo -> &varepsilon
				case 7246: state = 7247; break; // &varnothi -> &varnothin
				case 7273: state = 7274; break; // &varsubset -> &varsubsetn
				case 7281: state = 7282; break; // &varsupset -> &varsupsetn
				case 7293: state = 7294; break; // &vartria -> &vartrian
				case 7367: state = 7368; break; // &VerticalLi -> &VerticalLin
				case 7387: state = 7388; break; // &VeryThi -> &VeryThin
				case 7428: state = 7429; break; // &vsub -> &vsubn
				case 7432: state = 7433; break; // &vsup -> &vsupn
				case 7495: state = 7532; break; // &x -> &xn
				case 7610: state = 7611; break; // &ye -> &yen
				case 7660: state = 7661; break; // &Zcaro -> &Zcaron
				case 7665: state = 7666; break; // &zcaro -> &zcaron
				case 7725: state = 7727; break; // &zw -> &zwn
				default: return false;
				}
				break;
			case 'o':
				switch (state) {
				case 0: state = 4833; break; // & -> &o
				case 1: state = 131; break; // &A -> &Ao
				case 7: state = 135; break; // &a -> &ao
				case 97: state = 98; break; // &andsl -> &andslo
				case 132: state = 133; break; // &Aog -> &Aogo
				case 136: state = 137; break; // &aog -> &aogo
				case 143: state = 152; break; // &ap -> &apo
				case 163: state = 164; break; // &ApplyFuncti -> &ApplyFunctio
				case 167: state = 168; break; // &appr -> &appro
				case 213: state = 214; break; // &awc -> &awco
				case 222: state = 459; break; // &b -> &bo
				case 226: state = 227; break; // &backc -> &backco
				case 234: state = 235; break; // &backepsil -> &backepsilo
				case 247: state = 456; break; // &B -> &Bo
				case 277: state = 278; break; // &bc -> &bco
				case 286: state = 287; break; // &bdqu -> &bdquo
				case 309: state = 310; break; // &bern -> &berno
				case 313: state = 314; break; // &Bern -> &Berno
				case 334: state = 343; break; // &big -> &bigo
				case 344: state = 345; break; // &bigod -> &bigodo
				case 372: state = 373; break; // &bigtriangled -> &bigtriangledo
				case 393: state = 394; break; // &bkar -> &bkaro
				case 396: state = 442; break; // &bl -> &blo
				case 400: state = 401; break; // &blackl -> &blacklo
				case 421: state = 422; break; // &blacktriangled -> &blacktriangledo
				case 445: state = 454; break; // &bn -> &bno
				case 451: state = 452; break; // &bN -> &bNo
				case 463: state = 464; break; // &bott -> &botto
				case 471: state = 472; break; // &boxb -> &boxbo
				case 552: state = 561; break; // &bs -> &bso
				case 583: state = 856; break; // &C -> &Co
				case 589: state = 860; break; // &c -> &co
				case 610: state = 611; break; // &capd -> &capdo
				case 631: state = 634; break; // &car -> &caro
				case 645: state = 667; break; // &Cc -> &Cco
				case 647: state = 648; break; // &Ccar -> &Ccaro
				case 650: state = 651; break; // &ccar -> &ccaro
				case 677: state = 678; break; // &Cd -> &Cdo
				case 680: state = 681; break; // &cd -> &cdo
				case 704: state = 705; break; // &CenterD -> &CenterDo
				case 709: state = 710; break; // &centerd -> &centerdo
				case 741: state = 742; break; // &circlearr -> &circlearro
				case 770: state = 771; break; // &CircleD -> &CircleDo
				case 803: state = 804; break; // &Cl -> &Clo
				case 811: state = 812; break; // &ClockwiseC -> &ClockwiseCo
				case 814: state = 815; break; // &ClockwiseCont -> &ClockwiseConto
				case 833: state = 834; break; // &CloseCurlyD -> &CloseCurlyDo
				case 840: state = 841; break; // &CloseCurlyDoubleQu -> &CloseCurlyDoubleQuo
				case 845: state = 846; break; // &CloseCurlyQu -> &CloseCurlyQuo
				case 857: state = 858; break; // &Col -> &Colo
				case 861: state = 862; break; // &col -> &colo
				case 885: state = 886; break; // &congd -> &congdo
				case 901: state = 902; break; // &Cont -> &Conto
				case 917: state = 918; break; // &copr -> &copro
				case 920: state = 921; break; // &Copr -> &Copro
				case 938: state = 939; break; // &CounterCl -> &CounterClo
				case 946: state = 947; break; // &CounterClockwiseC -> &CounterClockwiseCo
				case 949: state = 950; break; // &CounterClockwiseCont -> &CounterClockwiseConto
				case 961: state = 969; break; // &cr -> &cro
				case 965: state = 966; break; // &Cr -> &Cro
				case 984: state = 985; break; // &ctd -> &ctdo
				case 1006: state = 1023; break; // &cup -> &cupo
				case 1020: state = 1021; break; // &cupd -> &cupdo
				case 1058: state = 1059; break; // &curvearr -> &curvearro
				case 1077: state = 1078; break; // &cwc -> &cwco
				case 1091: state = 1296; break; // &D -> &Do
				case 1097: state = 1291; break; // &d -> &do
				case 1123: state = 1124; break; // &dbkar -> &dbkaro
				case 1131: state = 1132; break; // &Dcar -> &Dcaro
				case 1136: state = 1137; break; // &dcar -> &dcaro
				case 1141: state = 1150; break; // &DD -> &DDo
				case 1142: state = 1156; break; // &dd -> &ddo
				case 1206: state = 1207; break; // &DiacriticalD -> &DiacriticalDo
				case 1230: state = 1235; break; // &diam -> &diamo
				case 1231: state = 1232; break; // &Diam -> &Diamo
				case 1263: state = 1274; break; // &div -> &divo
				case 1266: state = 1267; break; // &divide -> &divideo
				case 1284: state = 1285; break; // &dlc -> &dlco
				case 1288: state = 1289; break; // &dlcr -> &dlcro
				case 1303: state = 1304; break; // &DotD -> &DotDo
				case 1308: state = 1309; break; // &doteqd -> &doteqdo
				case 1347: state = 1348; break; // &DoubleC -> &DoubleCo
				case 1350: state = 1351; break; // &DoubleCont -> &DoubleConto
				case 1362: state = 1363; break; // &DoubleD -> &DoubleDo
				case 1369: state = 1370; break; // &DoubleDownArr -> &DoubleDownArro
				case 1372: state = 1394; break; // &DoubleL -> &DoubleLo
				case 1378: state = 1379; break; // &DoubleLeftArr -> &DoubleLeftArro
				case 1388: state = 1389; break; // &DoubleLeftRightArr -> &DoubleLeftRightArro
				case 1403: state = 1404; break; // &DoubleLongLeftArr -> &DoubleLongLeftArro
				case 1413: state = 1414; break; // &DoubleLongLeftRightArr -> &DoubleLongLeftRightArro
				case 1423: state = 1424; break; // &DoubleLongRightArr -> &DoubleLongRightArro
				case 1433: state = 1434; break; // &DoubleRightArr -> &DoubleRightArro
				case 1443: state = 1444; break; // &DoubleUpArr -> &DoubleUpArro
				case 1446: state = 1447; break; // &DoubleUpD -> &DoubleUpDo
				case 1452: state = 1453; break; // &DoubleUpDownArr -> &DoubleUpDownArro
				case 1470: state = 1471; break; // &DownArr -> &DownArro
				case 1475: state = 1476; break; // &Downarr -> &Downarro
				case 1482: state = 1483; break; // &downarr -> &downarro
				case 1492: state = 1493; break; // &DownArrowUpArr -> &DownArrowUpArro
				case 1500: state = 1501; break; // &downd -> &downdo
				case 1506: state = 1507; break; // &downdownarr -> &downdownarro
				case 1513: state = 1514; break; // &downharp -> &downharpo
				case 1514: state = 1515; break; // &downharpo -> &downharpoo
				case 1538: state = 1539; break; // &DownLeftRightVect -> &DownLeftRightVecto
				case 1547: state = 1548; break; // &DownLeftTeeVect -> &DownLeftTeeVecto
				case 1553: state = 1554; break; // &DownLeftVect -> &DownLeftVecto
				case 1570: state = 1571; break; // &DownRightTeeVect -> &DownRightTeeVecto
				case 1576: state = 1577; break; // &DownRightVect -> &DownRightVecto
				case 1587: state = 1588; break; // &DownTeeArr -> &DownTeeArro
				case 1594: state = 1595; break; // &drbkar -> &drbkaro
				case 1597: state = 1598; break; // &drc -> &drco
				case 1601: state = 1602; break; // &drcr -> &drcro
				case 1607: state = 1614; break; // &ds -> &dso
				case 1617: state = 1618; break; // &Dstr -> &Dstro
				case 1621: state = 1622; break; // &dstr -> &dstro
				case 1625: state = 1626; break; // &dtd -> &dtdo
				case 1656: state = 1801; break; // &E -> &Eo
				case 1662: state = 1805; break; // &e -> &eo
				case 1674: state = 1675; break; // &Ecar -> &Ecaro
				case 1677: state = 1688; break; // &ec -> &eco
				case 1679: state = 1680; break; // &ecar -> &ecaro
				case 1689: state = 1690; break; // &ecol -> &ecolo
				case 1694: state = 1701; break; // &eD -> &eDo
				case 1695: state = 1696; break; // &eDD -> &eDDo
				case 1698: state = 1699; break; // &Ed -> &Edo
				case 1703: state = 1704; break; // &ed -> &edo
				case 1708: state = 1709; break; // &efD -> &efDo
				case 1725: state = 1726; break; // &egsd -> &egsdo
				case 1743: state = 1744; break; // &elsd -> &elsdo
				case 1802: state = 1803; break; // &Eog -> &Eogo
				case 1806: state = 1807; break; // &eog -> &eogo
				case 1826: state = 1827; break; // &Epsil -> &Epsilo
				case 1829: state = 1830; break; // &epsil -> &epsilo
				case 1834: state = 1838; break; // &eqc -> &eqco
				case 1839: state = 1840; break; // &eqcol -> &eqcolo
				case 1894: state = 1895; break; // &erD -> &erDo
				case 1903: state = 1904; break; // &esd -> &esdo
				case 1923: state = 1924; break; // &eur -> &euro
				case 1936: state = 1955; break; // &exp -> &expo
				case 1942: state = 1943; break; // &expectati -> &expectatio
				case 1945: state = 1946; break; // &Exp -> &Expo
				case 1964: state = 2055; break; // &f -> &fo
				case 1971: state = 1972; break; // &fallingd -> &fallingdo
				case 1977: state = 2052; break; // &F -> &Fo
				case 2049: state = 2050; break; // &fn -> &fno
				case 2083: state = 2109; break; // &fr -> &fro
				case 2118: state = 2225; break; // &g -> &go
				case 2124: state = 2222; break; // &G -> &Go
				case 2159: state = 2160; break; // &Gd -> &Gdo
				case 2162: state = 2163; break; // &gd -> &gdo
				case 2179: state = 2180; break; // &gesd -> &gesdo
				case 2181: state = 2182; break; // &gesdot -> &gesdoto
				case 2212: state = 2213; break; // &gnappr -> &gnappro
				case 2299: state = 2300; break; // &gtd -> &gtdo
				case 2315: state = 2316; break; // &gtrappr -> &gtrappro
				case 2320: state = 2321; break; // &gtrd -> &gtrdo
				case 2351: state = 2469; break; // &H -> &Ho
				case 2356: state = 2440; break; // &h -> &ho
				case 2410: state = 2411; break; // &herc -> &herco
				case 2432: state = 2433; break; // &hksear -> &hksearo
				case 2437: state = 2438; break; // &hkswar -> &hkswaro
				case 2440: state = 2448; break; // &ho -> &hoo
				case 2456: state = 2457; break; // &hookleftarr -> &hookleftarro
				case 2466: state = 2467; break; // &hookrightarr -> &hookrightarro
				case 2480: state = 2481; break; // &Horiz -> &Horizo
				case 2501: state = 2502; break; // &Hstr -> &Hstro
				case 2505: state = 2506; break; // &hstr -> &hstro
				case 2511: state = 2512; break; // &HumpD -> &HumpDo
				case 2533: state = 2716; break; // &I -> &Io
				case 2539: state = 2713; break; // &i -> &io
				case 2555: state = 2556; break; // &Id -> &Ido
				case 2582: state = 2593; break; // &ii -> &iio
				case 2608: state = 2631; break; // &im -> &imo
				case 2641: state = 2652; break; // &in -> &ino
				case 2653: state = 2654; break; // &inod -> &inodo
				case 2681: state = 2682; break; // &Intersecti -> &Intersectio
				case 2690: state = 2691; break; // &intpr -> &intpro
				case 2700: state = 2701; break; // &InvisibleC -> &InvisibleCo
				case 2717: state = 2718; break; // &Iog -> &Iogo
				case 2720: state = 2721; break; // &iog -> &iogo
				case 2732: state = 2733; break; // &ipr -> &ipro
				case 2748: state = 2749; break; // &isind -> &isindo
				case 2777: state = 2797; break; // &J -> &Jo
				case 2782: state = 2800; break; // &j -> &jo
				case 2825: state = 2869; break; // &K -> &Ko
				case 2830: state = 2872; break; // &k -> &ko
				case 2881: state = 3466; break; // &l -> &lo
				case 2886: state = 3475; break; // &L -> &Lo
				case 2932: state = 2933; break; // &laqu -> &laquo
				case 2990: state = 2991; break; // &Lcar -> &Lcaro
				case 2995: state = 2996; break; // &lcar -> &lcaro
				case 3016: state = 3017; break; // &ldqu -> &ldquo
				case 3049: state = 3050; break; // &LeftArr -> &LeftArro
				case 3054: state = 3055; break; // &Leftarr -> &Leftarro
				case 3061: state = 3062; break; // &leftarr -> &leftarro
				case 3074: state = 3075; break; // &LeftArrowRightArr -> &LeftArrowRightArro
				case 3088: state = 3089; break; // &LeftD -> &LeftDo
				case 3109: state = 3110; break; // &LeftDownTeeVect -> &LeftDownTeeVecto
				case 3115: state = 3116; break; // &LeftDownVect -> &LeftDownVecto
				case 3122: state = 3123; break; // &LeftFl -> &LeftFlo
				case 3123: state = 3124; break; // &LeftFlo -> &LeftFloo
				case 3129: state = 3130; break; // &leftharp -> &leftharpo
				case 3130: state = 3131; break; // &leftharpo -> &leftharpoo
				case 3133: state = 3134; break; // &leftharpoond -> &leftharpoondo
				case 3145: state = 3146; break; // &leftleftarr -> &leftleftarro
				case 3156: state = 3157; break; // &LeftRightArr -> &LeftRightArro
				case 3166: state = 3167; break; // &Leftrightarr -> &Leftrightarro
				case 3176: state = 3177; break; // &leftrightarr -> &leftrightarro
				case 3183: state = 3184; break; // &leftrightharp -> &leftrightharpo
				case 3184: state = 3185; break; // &leftrightharpo -> &leftrightharpoo
				case 3195: state = 3196; break; // &leftrightsquigarr -> &leftrightsquigarro
				case 3201: state = 3202; break; // &LeftRightVect -> &LeftRightVecto
				case 3209: state = 3210; break; // &LeftTeeArr -> &LeftTeeArro
				case 3215: state = 3216; break; // &LeftTeeVect -> &LeftTeeVecto
				case 3245: state = 3246; break; // &LeftUpD -> &LeftUpDo
				case 3252: state = 3253; break; // &LeftUpDownVect -> &LeftUpDownVecto
				case 3261: state = 3262; break; // &LeftUpTeeVect -> &LeftUpTeeVecto
				case 3267: state = 3268; break; // &LeftUpVect -> &LeftUpVecto
				case 3276: state = 3277; break; // &LeftVect -> &LeftVecto
				case 3294: state = 3295; break; // &lesd -> &lesdo
				case 3296: state = 3297; break; // &lesdot -> &lesdoto
				case 3306: state = 3307; break; // &lessappr -> &lessappro
				case 3309: state = 3310; break; // &lessd -> &lessdo
				case 3381: state = 3382; break; // &lfl -> &lflo
				case 3382: state = 3383; break; // &lflo -> &lfloo
				case 3413: state = 3414; break; // &llc -> &llco
				case 3424: state = 3425; break; // &Lleftarr -> &Lleftarro
				case 3436: state = 3437; break; // &Lmid -> &Lmido
				case 3439: state = 3444; break; // &lm -> &lmo
				case 3441: state = 3442; break; // &lmid -> &lmido
				case 3456: state = 3457; break; // &lnappr -> &lnappro
				case 3466: state = 3573; break; // &lo -> &loo
				case 3484: state = 3485; break; // &LongLeftArr -> &LongLeftArro
				case 3493: state = 3494; break; // &Longleftarr -> &Longleftarro
				case 3504: state = 3505; break; // &longleftarr -> &longleftarro
				case 3514: state = 3515; break; // &LongLeftRightArr -> &LongLeftRightArro
				case 3524: state = 3525; break; // &Longleftrightarr -> &Longleftrightarro
				case 3534: state = 3535; break; // &longleftrightarr -> &longleftrightarro
				case 3541: state = 3542; break; // &longmapst -> &longmapsto
				case 3550: state = 3551; break; // &LongRightArr -> &LongRightArro
				case 3560: state = 3561; break; // &Longrightarr -> &Longrightarro
				case 3570: state = 3571; break; // &longrightarr -> &longrightarro
				case 3577: state = 3578; break; // &looparr -> &looparro
				case 3619: state = 3620; break; // &LowerLeftArr -> &LowerLeftArro
				case 3629: state = 3630; break; // &LowerRightArr -> &LowerRightArro
				case 3647: state = 3648; break; // &lrc -> &lrco
				case 3664: state = 3665; break; // &lsaqu -> &lsaquo
				case 3679: state = 3680; break; // &lsqu -> &lsquo
				case 3683: state = 3684; break; // &Lstr -> &Lstro
				case 3687: state = 3688; break; // &lstr -> &lstro
				case 3697: state = 3698; break; // &ltd -> &ltdo
				case 3745: state = 3865; break; // &m -> &mo
				case 3755: state = 3870; break; // &M -> &Mo
				case 3760: state = 3761; break; // &mapst -> &mapsto
				case 3762: state = 3763; break; // &mapstod -> &mapstodo
				case 3776: state = 3777; break; // &mc -> &mco
				case 3789: state = 3790; break; // &mDD -> &mDDo
				case 3825: state = 3826; break; // &mh -> &mho
				case 3829: state = 3830; break; // &micr -> &micro
				case 3838: state = 3839; break; // &midd -> &middo
				case 3883: state = 3884; break; // &mstp -> &mstpo
				case 3897: state = 4212; break; // &n -> &no
				case 3902: state = 4190; break; // &N -> &No
				case 3914: state = 3918; break; // &nap -> &napo
				case 3921: state = 3922; break; // &nappr -> &nappro
				case 3937: state = 3956; break; // &nc -> &nco
				case 3942: state = 3943; break; // &Ncar -> &Ncaro
				case 3945: state = 3946; break; // &ncar -> &ncaro
				case 3959: state = 3960; break; // &ncongd -> &ncongdo
				case 3978: state = 3979; break; // &nearr -> &nearro
				case 3981: state = 3982; break; // &ned -> &nedo
				case 4138: state = 4139; break; // &nLeftarr -> &nLeftarro
				case 4145: state = 4146; break; // &nleftarr -> &nleftarro
				case 4155: state = 4156; break; // &nLeftrightarr -> &nLeftrightarro
				case 4165: state = 4166; break; // &nleftrightarr -> &nleftrightarro
				case 4217: state = 4218; break; // &NotC -> &NotCo
				case 4231: state = 4232; break; // &NotD -> &NotDo
				case 4320: state = 4321; break; // &NotHumpD -> &NotHumpDo
				case 4335: state = 4336; break; // &notind -> &notindo
				case 4597: state = 4608; break; // &np -> &npo
				case 4637: state = 4638; break; // &nRightarr -> &nRightarro
				case 4646: state = 4647; break; // &nrightarr -> &nrightarro
				case 4663: state = 4664; break; // &nsh -> &nsho
				case 4756: state = 4757; break; // &numer -> &numero
				case 4820: state = 4821; break; // &nwarr -> &nwarro
				case 4827: state = 4951; break; // &O -> &Oo
				case 4833: state = 4954; break; // &o -> &oo
				case 4851: state = 4866; break; // &od -> &odo
				case 4868: state = 4869; break; // &ods -> &odso
				case 4887: state = 4888; break; // &og -> &ogo
				case 4915: state = 4916; break; // &olcr -> &olcro
				case 4939: state = 4940; break; // &Omicr -> &Omicro
				case 4944: state = 4945; break; // &omicr -> &omicro
				case 4968: state = 4969; break; // &OpenCurlyD -> &OpenCurlyDo
				case 4975: state = 4976; break; // &OpenCurlyDoubleQu -> &OpenCurlyDoubleQuo
				case 4980: state = 4981; break; // &OpenCurlyQu -> &OpenCurlyQuo
				case 4991: state = 5006; break; // &or -> &oro
				case 4997: state = 4998; break; // &order -> &ordero
				case 5003: state = 5004; break; // &orig -> &origo
				case 5009: state = 5010; break; // &orsl -> &orslo
				case 5018: state = 5029; break; // &os -> &oso
				case 5083: state = 5201; break; // &p -> &po
				case 5096: state = 5189; break; // &P -> &Po
				case 5113: state = 5114; break; // &peri -> &perio
				case 5130: state = 5137; break; // &ph -> &pho
				case 5145: state = 5146; break; // &pitchf -> &pitchfo
				case 5168: state = 5169; break; // &plusd -> &plusdo
				case 5186: state = 5187; break; // &plustw -> &plustwo
				case 5215: state = 5298; break; // &Pr -> &Pro
				case 5216: state = 5296; break; // &pr -> &pro
				case 5228: state = 5229; break; // &precappr -> &precappro
				case 5270: state = 5271; break; // &precnappr -> &precnappro
				case 5317: state = 5318; break; // &Prop -> &Propo
				case 5321: state = 5322; break; // &Proporti -> &Proportio
				case 5326: state = 5327; break; // &propt -> &propto
				case 5348: state = 5357; break; // &Q -> &Qo
				case 5351: state = 5360; break; // &q -> &qo
				case 5374: state = 5395; break; // &qu -> &quo
				case 5380: state = 5381; break; // &quaterni -> &quaternio
				case 5397: state = 5875; break; // &r -> &ro
				case 5405: state = 5887; break; // &R -> &Ro
				case 5432: state = 5433; break; // &raqu -> &raquo
				case 5470: state = 5471; break; // &rati -> &ratio
				case 5504: state = 5505; break; // &Rcar -> &Rcaro
				case 5509: state = 5510; break; // &rcar -> &rcaro
				case 5535: state = 5536; break; // &rdqu -> &rdquo
				case 5597: state = 5598; break; // &rfl -> &rflo
				case 5598: state = 5599; break; // &rflo -> &rfloo
				case 5607: state = 5615; break; // &rh -> &rho
				case 5613: state = 5614; break; // &Rh -> &Rho
				case 5634: state = 5635; break; // &RightArr -> &RightArro
				case 5639: state = 5640; break; // &Rightarr -> &Rightarro
				case 5648: state = 5649; break; // &rightarr -> &rightarro
				case 5660: state = 5661; break; // &RightArrowLeftArr -> &RightArrowLeftArro
				case 5674: state = 5675; break; // &RightD -> &RightDo
				case 5695: state = 5696; break; // &RightDownTeeVect -> &RightDownTeeVecto
				case 5701: state = 5702; break; // &RightDownVect -> &RightDownVecto
				case 5708: state = 5709; break; // &RightFl -> &RightFlo
				case 5709: state = 5710; break; // &RightFlo -> &RightFloo
				case 5715: state = 5716; break; // &rightharp -> &rightharpo
				case 5716: state = 5717; break; // &rightharpo -> &rightharpoo
				case 5719: state = 5720; break; // &rightharpoond -> &rightharpoondo
				case 5731: state = 5732; break; // &rightleftarr -> &rightleftarro
				case 5738: state = 5739; break; // &rightleftharp -> &rightleftharpo
				case 5739: state = 5740; break; // &rightleftharpo -> &rightleftharpoo
				case 5750: state = 5751; break; // &rightrightarr -> &rightrightarro
				case 5761: state = 5762; break; // &rightsquigarr -> &rightsquigarro
				case 5769: state = 5770; break; // &RightTeeArr -> &RightTeeArro
				case 5775: state = 5776; break; // &RightTeeVect -> &RightTeeVecto
				case 5805: state = 5806; break; // &RightUpD -> &RightUpDo
				case 5812: state = 5813; break; // &RightUpDownVect -> &RightUpDownVecto
				case 5821: state = 5822; break; // &RightUpTeeVect -> &RightUpTeeVecto
				case 5827: state = 5828; break; // &RightUpVect -> &RightUpVecto
				case 5836: state = 5837; break; // &RightVect -> &RightVecto
				case 5848: state = 5849; break; // &risingd -> &risingdo
				case 5862: state = 5863; break; // &rm -> &rmo
				case 5914: state = 5915; break; // &rpp -> &rppo
				case 5931: state = 5932; break; // &Rrightarr -> &Rrightarro
				case 5937: state = 5938; break; // &rsaqu -> &rsaquo
				case 5948: state = 5949; break; // &rsqu -> &rsquo
				case 5985: state = 6244; break; // &S -> &So
				case 5991: state = 6235; break; // &s -> &so
				case 5999: state = 6000; break; // &sbqu -> &sbquo
				case 6006: state = 6007; break; // &Scar -> &Scaro
				case 6009: state = 6010; break; // &scar -> &scaro
				case 6037: state = 6038; break; // &scp -> &scpo
				case 6048: state = 6049; break; // &sd -> &sdo
				case 6061: state = 6062; break; // &searr -> &searro
				case 6084: state = 6085; break; // &sfr -> &sfro
				case 6088: state = 6126; break; // &sh -> &sho
				case 6104: state = 6105; break; // &Sh -> &Sho
				case 6108: state = 6109; break; // &ShortD -> &ShortDo
				case 6114: state = 6115; break; // &ShortDownArr -> &ShortDownArro
				case 6123: state = 6124; break; // &ShortLeftArr -> &ShortLeftArro
				case 6147: state = 6148; break; // &ShortRightArr -> &ShortRightArro
				case 6154: state = 6155; break; // &ShortUpArr -> &ShortUpArro
				case 6169: state = 6170; break; // &simd -> &simdo
				case 6302: state = 6303; break; // &SquareIntersecti -> &SquareIntersectio
				case 6329: state = 6330; break; // &SquareUni -> &SquareUnio
				case 6373: state = 6374; break; // &straightepsil -> &straightepsilo
				case 6385: state = 6386; break; // &subd -> &subdo
				case 6390: state = 6391; break; // &subed -> &subedo
				case 6436: state = 6437; break; // &succappr -> &succappro
				case 6478: state = 6479; break; // &succnappr -> &succnappro
				case 6504: state = 6505; break; // &supd -> &supdo
				case 6512: state = 6513; break; // &suped -> &supedo
				case 6526: state = 6527; break; // &suphs -> &suphso
				case 6572: state = 6573; break; // &swarr -> &swarro
				case 6583: state = 6742; break; // &T -> &To
				case 6586: state = 6732; break; // &t -> &to
				case 6599: state = 6600; break; // &Tcar -> &Tcaro
				case 6604: state = 6605; break; // &tcar -> &tcaro
				case 6617: state = 6618; break; // &td -> &tdo
				case 6629: state = 6693; break; // &th -> &tho
				case 6638: state = 6639; break; // &Theref -> &Therefo
				case 6642: state = 6643; break; // &theref -> &therefo
				case 6660: state = 6661; break; // &thickappr -> &thickappro
				case 6736: state = 6737; break; // &topb -> &topbo
				case 6745: state = 6746; break; // &topf -> &topfo
				case 6770: state = 6771; break; // &triangled -> &triangledo
				case 6788: state = 6789; break; // &trid -> &trido
				case 6802: state = 6803; break; // &TripleD -> &TripleDo
				case 6838: state = 6839; break; // &Tstr -> &Tstro
				case 6842: state = 6843; break; // &tstr -> &tstro
				case 6845: state = 6849; break; // &tw -> &two
				case 6860: state = 6861; break; // &twoheadleftarr -> &twoheadleftarro
				case 6870: state = 6871; break; // &twoheadrightarr -> &twoheadrightarro
				case 6873: state = 7019; break; // &U -> &Uo
				case 6879: state = 7023; break; // &u -> &uo
				case 6886: state = 6892; break; // &Uarr -> &Uarro
				case 6966: state = 6967; break; // &ulc -> &ulco
				case 6972: state = 6973; break; // &ulcr -> &ulcro
				case 7012: state = 7013; break; // &Uni -> &Unio
				case 7020: state = 7021; break; // &Uog -> &Uogo
				case 7024: state = 7025; break; // &uog -> &uogo
				case 7034: state = 7035; break; // &UpArr -> &UpArro
				case 7039: state = 7040; break; // &Uparr -> &Uparro
				case 7045: state = 7046; break; // &uparr -> &uparro
				case 7051: state = 7052; break; // &UpArrowD -> &UpArrowDo
				case 7057: state = 7058; break; // &UpArrowDownArr -> &UpArrowDownArro
				case 7060: state = 7061; break; // &UpD -> &UpDo
				case 7066: state = 7067; break; // &UpDownArr -> &UpDownArro
				case 7069: state = 7070; break; // &Upd -> &Updo
				case 7075: state = 7076; break; // &Updownarr -> &Updownarro
				case 7078: state = 7079; break; // &upd -> &updo
				case 7084: state = 7085; break; // &updownarr -> &updownarro
				case 7101: state = 7102; break; // &upharp -> &upharpo
				case 7102: state = 7103; break; // &upharpo -> &upharpoo
				case 7126: state = 7127; break; // &UpperLeftArr -> &UpperLeftArro
				case 7136: state = 7137; break; // &UpperRightArr -> &UpperRightArro
				case 7144: state = 7145; break; // &Upsil -> &Upsilo
				case 7147: state = 7148; break; // &upsil -> &upsilo
				case 7155: state = 7156; break; // &UpTeeArr -> &UpTeeArro
				case 7162: state = 7163; break; // &upuparr -> &upuparro
				case 7167: state = 7168; break; // &urc -> &urco
				case 7173: state = 7174; break; // &urcr -> &urcro
				case 7193: state = 7194; break; // &utd -> &utdo
				case 7223: state = 7410; break; // &v -> &vo
				case 7234: state = 7235; break; // &varepsil -> &varepsilo
				case 7242: state = 7243; break; // &varn -> &varno
				case 7253: state = 7254; break; // &varpr -> &varpro
				case 7256: state = 7257; break; // &varpropt -> &varpropto
				case 7262: state = 7263; break; // &varrh -> &varrho
				case 7307: state = 7407; break; // &V -> &Vo
				case 7376: state = 7377; break; // &VerticalSeparat -> &VerticalSeparato
				case 7414: state = 7415; break; // &vpr -> &vpro
				case 7447: state = 7477; break; // &W -> &Wo
				case 7452: state = 7480; break; // &w -> &wo
				case 7495: state = 7535; break; // &x -> &xo
				case 7508: state = 7539; break; // &X -> &Xo
				case 7536: state = 7537; break; // &xod -> &xodo
				case 7584: state = 7622; break; // &Y -> &Yo
				case 7590: state = 7625; break; // &y -> &yo
				case 7645: state = 7713; break; // &Z -> &Zo
				case 7651: state = 7716; break; // &z -> &zo
				case 7659: state = 7660; break; // &Zcar -> &Zcaro
				case 7664: state = 7665; break; // &zcar -> &zcaro
				case 7669: state = 7670; break; // &Zd -> &Zdo
				case 7672: state = 7673; break; // &zd -> &zdo
				case 7681: state = 7682; break; // &Zer -> &Zero
				default: return false;
				}
				break;
			case 'p':
				switch (state) {
				case 0: state = 5083; break; // & -> &p
				case 1: state = 154; break; // &A -> &Ap
				case 7: state = 143; break; // &a -> &ap
				case 60: state = 72; break; // &al -> &alp
				case 61: state = 66; break; // &ale -> &alep
				case 68: state = 69; break; // &Al -> &Alp
				case 79: state = 87; break; // &am -> &amp
				case 98: state = 99; break; // &andslo -> &andslop
				case 123: state = 124; break; // &angs -> &angsp
				case 131: state = 139; break; // &Ao -> &Aop
				case 135: state = 141; break; // &ao -> &aop
				case 143: state = 166; break; // &ap -> &app
				case 154: state = 155; break; // &Ap -> &App
				case 192: state = 193; break; // &asym -> &asymp
				case 222: state = 532; break; // &b -> &bp
				case 225: state = 237; break; // &back -> &backp
				case 230: state = 231; break; // &backe -> &backep
				case 288: state = 305; break; // &be -> &bep
				case 300: state = 301; break; // &bem -> &bemp
				case 336: state = 337; break; // &bigca -> &bigcap
				case 341: state = 342; break; // &bigcu -> &bigcup
				case 343: state = 347; break; // &bigo -> &bigop
				case 359: state = 360; break; // &bigsqcu -> &bigsqcup
				case 376: state = 377; break; // &bigtriangleu -> &bigtriangleup
				case 378: state = 379; break; // &bigu -> &bigup
				case 456: state = 457; break; // &Bo -> &Bop
				case 459: state = 460; break; // &bo -> &bop
				case 470: state = 499; break; // &box -> &boxp
				case 573: state = 574; break; // &bum -> &bump
				case 578: state = 579; break; // &Bum -> &Bump
				case 584: state = 595; break; // &Ca -> &Cap
				case 590: state = 596; break; // &ca -> &cap
				case 603: state = 604; break; // &capbrcu -> &capbrcup
				case 606: state = 607; break; // &capca -> &capcap
				case 608: state = 609; break; // &capcu -> &capcup
				case 642: state = 643; break; // &cca -> &ccap
				case 672: state = 673; break; // &ccu -> &ccup
				case 693: state = 694; break; // &cem -> &cemp
				case 856: state = 913; break; // &Co -> &Cop
				case 860: state = 915; break; // &co -> &cop
				case 867: state = 871; break; // &com -> &comp
				case 978: state = 981; break; // &csu -> &csup
				case 987: state = 1006; break; // &cu -> &cup
				case 994: state = 995; break; // &cue -> &cuep
				case 1002: state = 1003; break; // &cularr -> &cularrp
				case 1004: state = 1005; break; // &Cu -> &Cup
				case 1010: state = 1011; break; // &cupbrca -> &cupbrcap
				case 1013: state = 1014; break; // &CupCa -> &CupCap
				case 1016: state = 1017; break; // &cupca -> &cupcap
				case 1018: state = 1019; break; // &cupcu -> &cupcup
				case 1034: state = 1035; break; // &curlyeq -> &curlyeqp
				case 1170: state = 1171; break; // &dem -> &demp
				case 1289: state = 1290; break; // &dlcro -> &dlcrop
				case 1291: state = 1299; break; // &do -> &dop
				case 1296: state = 1297; break; // &Do -> &Dop
				case 1302: state = 1321; break; // &dot -> &dotp
				case 1439: state = 1440; break; // &DoubleU -> &DoubleUp
				case 1488: state = 1489; break; // &DownArrowU -> &DownArrowUp
				case 1512: state = 1513; break; // &downhar -> &downharp
				case 1602: state = 1603; break; // &drcro -> &drcrop
				case 1656: state = 1823; break; // &E -> &Ep
				case 1662: state = 1813; break; // &e -> &ep
				case 1746: state = 1760; break; // &Em -> &Emp
				case 1750: state = 1754; break; // &em -> &emp
				case 1790: state = 1791; break; // &ems -> &emsp
				case 1799: state = 1800; break; // &ens -> &ensp
				case 1801: state = 1809; break; // &Eo -> &Eop
				case 1805: state = 1811; break; // &eo -> &eop
				case 1884: state = 1885; break; // &eqv -> &eqvp
				case 1925: state = 1936; break; // &ex -> &exp
				case 1931: state = 1945; break; // &Ex -> &Exp
				case 1964: state = 2076; break; // &f -> &fp
				case 2052: state = 2053; break; // &Fo -> &Fop
				case 2055: state = 2056; break; // &fo -> &fop
				case 2119: state = 2134; break; // &ga -> &gap
				case 2209: state = 2210; break; // &gna -> &gnap
				case 2210: state = 2211; break; // &gnap -> &gnapp
				case 2222: state = 2223; break; // &Go -> &Gop
				case 2225: state = 2226; break; // &go -> &gop
				case 2312: state = 2313; break; // &gtra -> &gtrap
				case 2313: state = 2314; break; // &gtrap -> &gtrapp
				case 2360: state = 2361; break; // &hairs -> &hairsp
				case 2407: state = 2408; break; // &helli -> &hellip
				case 2423: state = 2424; break; // &HilbertS -> &HilbertSp
				case 2440: state = 2472; break; // &ho -> &hop
				case 2469: state = 2470; break; // &Ho -> &Hop
				case 2509: state = 2510; break; // &Hum -> &Hump
				case 2517: state = 2518; break; // &HumpDownHum -> &HumpDownHump
				case 2524: state = 2529; break; // &hy -> &hyp
				case 2539: state = 2731; break; // &i -> &ip
				case 2604: state = 2636; break; // &Im -> &Imp
				case 2608: state = 2633; break; // &im -> &imp
				case 2612: state = 2625; break; // &imag -> &imagp
				case 2658: state = 2689; break; // &int -> &intp
				case 2713: state = 2725; break; // &io -> &iop
				case 2716: state = 2723; break; // &Io -> &Iop
				case 2797: state = 2798; break; // &Jo -> &Jop
				case 2800: state = 2801; break; // &jo -> &jop
				case 2826: state = 2827; break; // &Ka -> &Kap
				case 2827: state = 2828; break; // &Kap -> &Kapp
				case 2831: state = 2832; break; // &ka -> &kap
				case 2832: state = 2833; break; // &kap -> &kapp
				case 2869: state = 2870; break; // &Ko -> &Kop
				case 2872: state = 2873; break; // &ko -> &kop
				case 2881: state = 3638; break; // &l -> &lp
				case 2887: state = 2923; break; // &La -> &Lap
				case 2892: state = 2922; break; // &la -> &lap
				case 2898: state = 2899; break; // &laem -> &laemp
				case 2939: state = 2949; break; // &larr -> &larrp
				case 2947: state = 2948; break; // &larrl -> &larrlp
				case 3128: state = 3129; break; // &lefthar -> &leftharp
				case 3137: state = 3138; break; // &leftharpoonu -> &leftharpoonup
				case 3182: state = 3183; break; // &leftrighthar -> &leftrightharp
				case 3243: state = 3244; break; // &LeftU -> &LeftUp
				case 3303: state = 3304; break; // &lessa -> &lessap
				case 3304: state = 3305; break; // &lessap -> &lessapp
				case 3453: state = 3454; break; // &lna -> &lnap
				case 3454: state = 3455; break; // &lnap -> &lnapp
				case 3466: state = 3589; break; // &lo -> &lop
				case 3475: state = 3592; break; // &Lo -> &Lop
				case 3538: state = 3539; break; // &longma -> &longmap
				case 3573: state = 3574; break; // &loo -> &loop
				case 3745: state = 3875; break; // &m -> &mp
				case 3746: state = 3758; break; // &ma -> &map
				case 3756: state = 3757; break; // &Ma -> &Map
				case 3770: state = 3771; break; // &mapstou -> &mapstoup
				case 3809: state = 3810; break; // &MediumS -> &MediumSp
				case 3856: state = 3857; break; // &mlc -> &mlcp
				case 3860: state = 3861; break; // &mn -> &mnp
				case 3865: state = 3873; break; // &mo -> &mop
				case 3870: state = 3871; break; // &Mo -> &Mop
				case 3882: state = 3883; break; // &mst -> &mstp
				case 3892: state = 3893; break; // &multima -> &multimap
				case 3895: state = 3896; break; // &muma -> &mumap
				case 3897: state = 4597; break; // &n -> &np
				case 3898: state = 3914; break; // &na -> &nap
				case 3914: state = 3920; break; // &nap -> &napp
				case 3931: state = 3932; break; // &nbs -> &nbsp
				case 3934: state = 3935; break; // &nbum -> &nbump
				case 3938: state = 3939; break; // &nca -> &ncap
				case 3962: state = 3963; break; // &ncu -> &ncup
				case 3997: state = 3998; break; // &NegativeMediumS -> &NegativeMediumSp
				case 4007: state = 4008; break; // &NegativeThickS -> &NegativeThickSp
				case 4013: state = 4014; break; // &NegativeThinS -> &NegativeThinSp
				case 4026: state = 4027; break; // &NegativeVeryThinS -> &NegativeVeryThinSp
				case 4101: state = 4108; break; // &nh -> &nhp
				case 4190: state = 4210; break; // &No -> &Nop
				case 4205: state = 4206; break; // &NonBreakingS -> &NonBreakingSp
				case 4212: state = 4213; break; // &no -> &nop
				case 4226: state = 4227; break; // &NotCu -> &NotCup
				case 4229: state = 4230; break; // &NotCupCa -> &NotCupCap
				case 4318: state = 4319; break; // &NotHum -> &NotHump
				case 4326: state = 4327; break; // &NotHumpDownHum -> &NotHumpDownHump
				case 4494: state = 4504; break; // &NotSquareSu -> &NotSquareSup
				case 4515: state = 4551; break; // &NotSu -> &NotSup
				case 4653: state = 4685; break; // &ns -> &nsp
				case 4666: state = 4670; break; // &nshort -> &nshortp
				case 4690: state = 4693; break; // &nsqsu -> &nsqsup
				case 4695: state = 4709; break; // &nsu -> &nsup
				case 4758: state = 4759; break; // &nums -> &numsp
				case 4761: state = 4762; break; // &nva -> &nvap
				case 4827: state = 4960; break; // &O -> &Op
				case 4833: state = 4957; break; // &o -> &op
				case 4951: state = 4952; break; // &Oo -> &Oop
				case 4954: state = 4955; break; // &oo -> &oop
				case 4985: state = 4986; break; // &oper -> &operp
				case 5010: state = 5011; break; // &orslo -> &orslop
				case 5109: state = 5119; break; // &per -> &perp
				case 5189: state = 5208; break; // &Po -> &Pop
				case 5195: state = 5196; break; // &Poincare -> &Poincarep
				case 5201: state = 5210; break; // &po -> &pop
				case 5217: state = 5218; break; // &pra -> &prap
				case 5225: state = 5226; break; // &preca -> &precap
				case 5226: state = 5227; break; // &precap -> &precapp
				case 5267: state = 5268; break; // &precna -> &precnap
				case 5268: state = 5269; break; // &precnap -> &precnapp
				case 5290: state = 5291; break; // &prna -> &prnap
				case 5296: state = 5316; break; // &pro -> &prop
				case 5298: state = 5317; break; // &Pro -> &Prop
				case 5346: state = 5347; break; // &puncs -> &puncsp
				case 5351: state = 5363; break; // &q -> &qp
				case 5357: state = 5358; break; // &Qo -> &Qop
				case 5360: state = 5361; break; // &qo -> &qop
				case 5397: state = 5909; break; // &r -> &rp
				case 5418: state = 5419; break; // &raem -> &raemp
				case 5439: state = 5452; break; // &rarr -> &rarrp
				case 5440: state = 5441; break; // &rarra -> &rarrap
				case 5450: state = 5451; break; // &rarrl -> &rarrlp
				case 5543: state = 5547; break; // &real -> &realp
				case 5579: state = 5580; break; // &ReverseU -> &ReverseUp
				case 5714: state = 5715; break; // &righthar -> &rightharp
				case 5723: state = 5724; break; // &rightharpoonu -> &rightharpoonup
				case 5737: state = 5738; break; // &rightlefthar -> &rightleftharp
				case 5803: state = 5804; break; // &RightU -> &RightUp
				case 5875: state = 5884; break; // &ro -> &rop
				case 5887: state = 5888; break; // &Ro -> &Rop
				case 5903: state = 5904; break; // &RoundIm -> &RoundImp
				case 5909: state = 5914; break; // &rp -> &rpp
				case 5991: state = 6249; break; // &s -> &sp
				case 6002: state = 6037; break; // &sc -> &scp
				case 6003: state = 6004; break; // &sca -> &scap
				case 6031: state = 6032; break; // &scna -> &scnap
				case 6090: state = 6091; break; // &shar -> &sharp
				case 6128: state = 6132; break; // &short -> &shortp
				case 6150: state = 6151; break; // &ShortU -> &ShortUp
				case 6168: state = 6180; break; // &sim -> &simp
				case 6215: state = 6216; break; // &smash -> &smashp
				case 6217: state = 6218; break; // &sme -> &smep
				case 6235: state = 6247; break; // &so -> &sop
				case 6244: state = 6245; break; // &So -> &Sop
				case 6260: state = 6261; break; // &sqca -> &sqcap
				case 6263: state = 6264; break; // &sqcu -> &sqcup
				case 6270: state = 6278; break; // &sqsu -> &sqsup
				case 6306: state = 6316; break; // &SquareSu -> &SquareSup
				case 6368: state = 6376; break; // &straight -> &straightp
				case 6369: state = 6370; break; // &straighte -> &straightep
				case 6381: state = 6499; break; // &Su -> &Sup
				case 6383: state = 6500; break; // &su -> &sup
				case 6384: state = 6400; break; // &sub -> &subp
				case 6428: state = 6430; break; // &subsu -> &subsup
				case 6433: state = 6434; break; // &succa -> &succap
				case 6434: state = 6435; break; // &succap -> &succapp
				case 6475: state = 6476; break; // &succna -> &succnap
				case 6476: state = 6477; break; // &succnap -> &succnapp
				case 6500: state = 6542; break; // &sup -> &supp
				case 6561: state = 6563; break; // &supsu -> &supsup
				case 6586: state = 6751; break; // &t -> &tp
				case 6657: state = 6658; break; // &thicka -> &thickap
				case 6658: state = 6659; break; // &thickap -> &thickapp
				case 6669: state = 6670; break; // &ThickS -> &ThickSp
				case 6675: state = 6676; break; // &thins -> &thinsp
				case 6678: state = 6679; break; // &ThinS -> &ThinSp
				case 6684: state = 6685; break; // &thka -> &thkap
				case 6732: state = 6735; break; // &to -> &top
				case 6742: state = 6743; break; // &To -> &Top
				case 6760: state = 6815; break; // &tr -> &trp
				case 6764: state = 6805; break; // &tri -> &trip
				case 6798: state = 6799; break; // &Tri -> &Trip
				case 6873: state = 7031; break; // &U -> &Up
				case 6879: state = 7042; break; // &u -> &up
				case 6973: state = 6974; break; // &ulcro -> &ulcrop
				case 7019: state = 7027; break; // &Uo -> &Uop
				case 7023: state = 7029; break; // &uo -> &uop
				case 7031: state = 7117; break; // &Up -> &Upp
				case 7100: state = 7101; break; // &uphar -> &upharp
				case 7158: state = 7159; break; // &upu -> &upup
				case 7174: state = 7175; break; // &urcro -> &urcrop
				case 7223: state = 7413; break; // &v -> &vp
				case 7229: state = 7249; break; // &var -> &varp
				case 7230: state = 7231; break; // &vare -> &varep
				case 7238: state = 7239; break; // &varka -> &varkap
				case 7239: state = 7240; break; // &varkap -> &varkapp
				case 7254: state = 7255; break; // &varpro -> &varprop
				case 7269: state = 7278; break; // &varsu -> &varsup
				case 7347: state = 7348; break; // &velli -> &vellip
				case 7371: state = 7372; break; // &VerticalSe -> &VerticalSep
				case 7389: state = 7390; break; // &VeryThinS -> &VeryThinSp
				case 7404: state = 7406; break; // &vnsu -> &vnsup
				case 7407: state = 7408; break; // &Vo -> &Vop
				case 7410: state = 7411; break; // &vo -> &vop
				case 7415: state = 7416; break; // &vpro -> &vprop
				case 7427: state = 7432; break; // &vsu -> &vsup
				case 7452: state = 7483; break; // &w -> &wp
				case 7471: state = 7472; break; // &weier -> &weierp
				case 7477: state = 7478; break; // &Wo -> &Wop
				case 7480: state = 7481; break; // &wo -> &wop
				case 7497: state = 7498; break; // &xca -> &xcap
				case 7502: state = 7503; break; // &xcu -> &xcup
				case 7530: state = 7531; break; // &xma -> &xmap
				case 7535: state = 7542; break; // &xo -> &xop
				case 7539: state = 7540; break; // &Xo -> &Xop
				case 7566: state = 7567; break; // &xsqcu -> &xsqcup
				case 7568: state = 7569; break; // &xu -> &xup
				case 7622: state = 7623; break; // &Yo -> &Yop
				case 7625: state = 7626; break; // &yo -> &yop
				case 7688: state = 7689; break; // &ZeroWidthS -> &ZeroWidthSp
				case 7713: state = 7714; break; // &Zo -> &Zop
				case 7716: state = 7717; break; // &zo -> &zop
				default: return false;
				}
				break;
			case 'q':
				switch (state) {
				case 0: state = 5351; break; // & -> &q
				case 170: state = 171; break; // &approxe -> &approxeq
				case 194: state = 195; break; // &asympe -> &asympeq
				case 245: state = 246; break; // &backsime -> &backsimeq
				case 284: state = 285; break; // &bd -> &bdq
				case 356: state = 357; break; // &bigs -> &bigsq
				case 407: state = 408; break; // &blacks -> &blacksq
				case 446: state = 447; break; // &bne -> &bneq
				case 576: state = 582; break; // &bumpe -> &bumpeq
				case 580: state = 581; break; // &Bumpe -> &Bumpeq
				case 735: state = 736; break; // &circe -> &circeq
				case 865: state = 866; break; // &colone -> &coloneq
				case 1033: state = 1034; break; // &curlye -> &curlyeq
				case 1159: state = 1160; break; // &ddotse -> &ddotseq
				case 1306: state = 1307; break; // &dote -> &doteq
				case 1311: state = 1312; break; // &DotE -> &DotEq
				case 1325: state = 1326; break; // &dots -> &dotsq
				case 1656: state = 1856; break; // &E -> &Eq
				case 1662: state = 1833; break; // &e -> &eq
				case 1768: state = 1769; break; // &EmptySmallS -> &EmptySmallSq
				case 1784: state = 1785; break; // &EmptyVerySmallS -> &EmptyVerySmallSq
				case 1975: state = 1976; break; // &fallingdotse -> &fallingdotseq
				case 2015: state = 2016; break; // &FilledSmallS -> &FilledSmallSq
				case 2030: state = 2031; break; // &FilledVerySmallS -> &FilledVerySmallSq
				case 2166: state = 2169; break; // &ge -> &geq
				case 2169: state = 2170; break; // &geq -> &geqq
				case 2216: state = 2217; break; // &gne -> &gneq
				case 2217: state = 2218; break; // &gneq -> &gneqq
				case 2238: state = 2239; break; // &GreaterE -> &GreaterEq
				case 2251: state = 2252; break; // &GreaterFullE -> &GreaterFullEq
				case 2272: state = 2273; break; // &GreaterSlantE -> &GreaterSlantEq
				case 2294: state = 2306; break; // &gt -> &gtq
				case 2323: state = 2324; break; // &gtre -> &gtreq
				case 2324: state = 2329; break; // &gtreq -> &gtreqq
				case 2346: state = 2347; break; // &gvertne -> &gvertneq
				case 2347: state = 2348; break; // &gvertneq -> &gvertneqq
				case 2519: state = 2520; break; // &HumpE -> &HumpEq
				case 2539: state = 2735; break; // &i -> &iq
				case 2892: state = 2931; break; // &la -> &laq
				case 3012: state = 3015; break; // &ld -> &ldq
				case 3032: state = 3284; break; // &le -> &leq
				case 3188: state = 3189; break; // &leftrights -> &leftrightsq
				case 3238: state = 3239; break; // &LeftTriangleE -> &LeftTriangleEq
				case 3284: state = 3285; break; // &leq -> &leqq
				case 3312: state = 3313; break; // &lesse -> &lesseq
				case 3313: state = 3317; break; // &lesseq -> &lesseqq
				case 3323: state = 3324; break; // &LessE -> &LessEq
				case 3339: state = 3340; break; // &LessFullE -> &LessFullEq
				case 3366: state = 3367; break; // &LessSlantE -> &LessSlantEq
				case 3460: state = 3461; break; // &lne -> &lneq
				case 3461: state = 3462; break; // &lneq -> &lneqq
				case 3661: state = 3677; break; // &ls -> &lsq
				case 3662: state = 3663; break; // &lsa -> &lsaq
				case 3692: state = 3712; break; // &lt -> &ltq
				case 3740: state = 3741; break; // &lvertne -> &lvertneq
				case 3741: state = 3742; break; // &lvertneq -> &lvertneqq
				case 3970: state = 4031; break; // &ne -> &neq
				case 4083: state = 4084; break; // &nge -> &ngeq
				case 4084: state = 4085; break; // &ngeq -> &ngeqq
				case 4131: state = 4168; break; // &nle -> &nleq
				case 4168: state = 4169; break; // &nleq -> &nleqq
				case 4248: state = 4255; break; // &NotE -> &NotEq
				case 4276: state = 4277; break; // &NotGreaterE -> &NotGreaterEq
				case 4285: state = 4286; break; // &NotGreaterFullE -> &NotGreaterFullEq
				case 4306: state = 4307; break; // &NotGreaterSlantE -> &NotGreaterSlantEq
				case 4328: state = 4329; break; // &NotHumpE -> &NotHumpEq
				case 4358: state = 4359; break; // &NotLeftTriangleE -> &NotLeftTriangleEq
				case 4365: state = 4366; break; // &NotLessE -> &NotLessEq
				case 4386: state = 4387; break; // &NotLessSlantE -> &NotLessSlantEq
				case 4438: state = 4439; break; // &NotPrecedesE -> &NotPrecedesEq
				case 4448: state = 4449; break; // &NotPrecedesSlantE -> &NotPrecedesSlantEq
				case 4482: state = 4483; break; // &NotRightTriangleE -> &NotRightTriangleEq
				case 4487: state = 4488; break; // &NotS -> &NotSq
				case 4499: state = 4500; break; // &NotSquareSubsetE -> &NotSquareSubsetEq
				case 4510: state = 4511; break; // &NotSquareSupersetE -> &NotSquareSupersetEq
				case 4520: state = 4521; break; // &NotSubsetE -> &NotSubsetEq
				case 4531: state = 4532; break; // &NotSucceedsE -> &NotSucceedsEq
				case 4541: state = 4542; break; // &NotSucceedsSlantE -> &NotSucceedsSlantEq
				case 4557: state = 4558; break; // &NotSupersetE -> &NotSupersetEq
				case 4567: state = 4568; break; // &NotTildeE -> &NotTildeEq
				case 4576: state = 4577; break; // &NotTildeFullE -> &NotTildeFullEq
				case 4619: state = 4620; break; // &nprece -> &npreceq
				case 4653: state = 4688; break; // &ns -> &nsq
				case 4680: state = 4681; break; // &nsime -> &nsimeq
				case 4702: state = 4703; break; // &nsubsete -> &nsubseteq
				case 4703: state = 4704; break; // &nsubseteq -> &nsubseteqq
				case 4707: state = 4708; break; // &nsucce -> &nsucceq
				case 4715: state = 4716; break; // &nsupsete -> &nsupseteq
				case 4716: state = 4717; break; // &nsupseteq -> &nsupseteqq
				case 4743: state = 4744; break; // &ntrianglelefte -> &ntrianglelefteq
				case 4750: state = 4751; break; // &ntrianglerighte -> &ntrianglerighteq
				case 5236: state = 5237; break; // &preccurlye -> &preccurlyeq
				case 5244: state = 5245; break; // &PrecedesE -> &PrecedesEq
				case 5254: state = 5255; break; // &PrecedesSlantE -> &PrecedesSlantEq
				case 5264: state = 5265; break; // &prece -> &preceq
				case 5273: state = 5274; break; // &precne -> &precneq
				case 5274: state = 5275; break; // &precneq -> &precneqq
				case 5390: state = 5391; break; // &queste -> &questeq
				case 5402: state = 5431; break; // &ra -> &raq
				case 5526: state = 5534; break; // &rd -> &rdq
				case 5562: state = 5569; break; // &ReverseE -> &ReverseEq
				case 5581: state = 5582; break; // &ReverseUpE -> &ReverseUpEq
				case 5754: state = 5755; break; // &rights -> &rightsq
				case 5798: state = 5799; break; // &RightTriangleE -> &RightTriangleEq
				case 5852: state = 5853; break; // &risingdotse -> &risingdotseq
				case 5934: state = 5946; break; // &rs -> &rsq
				case 5935: state = 5936; break; // &rsa -> &rsaq
				case 5985: state = 6266; break; // &S -> &Sq
				case 5991: state = 6258; break; // &s -> &sq
				case 5997: state = 5998; break; // &sb -> &sbq
				case 6172: state = 6173; break; // &sime -> &simeq
				case 6276: state = 6277; break; // &sqsubsete -> &sqsubseteq
				case 6283: state = 6284; break; // &sqsupsete -> &sqsupseteq
				case 6311: state = 6312; break; // &SquareSubsetE -> &SquareSubsetEq
				case 6322: state = 6323; break; // &SquareSupersetE -> &SquareSupersetEq
				case 6414: state = 6415; break; // &subsete -> &subseteq
				case 6415: state = 6416; break; // &subseteq -> &subseteqq
				case 6417: state = 6418; break; // &SubsetE -> &SubsetEq
				case 6423: state = 6424; break; // &subsetne -> &subsetneq
				case 6424: state = 6425; break; // &subsetneq -> &subsetneqq
				case 6444: state = 6445; break; // &succcurlye -> &succcurlyeq
				case 6452: state = 6453; break; // &SucceedsE -> &SucceedsEq
				case 6462: state = 6463; break; // &SucceedsSlantE -> &SucceedsSlantEq
				case 6472: state = 6473; break; // &succe -> &succeq
				case 6481: state = 6482; break; // &succne -> &succneq
				case 6482: state = 6483; break; // &succneq -> &succneqq
				case 6520: state = 6521; break; // &SupersetE -> &SupersetEq
				case 6552: state = 6553; break; // &supsete -> &supseteq
				case 6553: state = 6554; break; // &supseteq -> &supseteqq
				case 6556: state = 6557; break; // &supsetne -> &supsetneq
				case 6557: state = 6558; break; // &supsetneq -> &supsetneqq
				case 6704: state = 6705; break; // &TildeE -> &TildeEq
				case 6713: state = 6714; break; // &TildeFullE -> &TildeFullEq
				case 6769: state = 6780; break; // &triangle -> &triangleq
				case 6778: state = 6779; break; // &trianglelefte -> &trianglelefteq
				case 6786: state = 6787; break; // &trianglerighte -> &trianglerighteq
				case 7087: state = 7088; break; // &UpE -> &UpEq
				case 7275: state = 7276; break; // &varsubsetne -> &varsubsetneq
				case 7276: state = 7277; break; // &varsubsetneq -> &varsubsetneqq
				case 7283: state = 7284; break; // &varsupsetne -> &varsupsetneq
				case 7284: state = 7285; break; // &varsupsetneq -> &varsupsetneqq
				case 7343: state = 7344; break; // &veee -> &veeeq
				case 7467: state = 7468; break; // &wedge -> &wedgeq
				case 7561: state = 7564; break; // &xs -> &xsq
				default: return false;
				}
				break;
			case 'r':
				switch (state) {
				case 0: state = 5397; break; // & -> &r
				case 1: state = 172; break; // &A -> &Ar
				case 7: state = 176; break; // &a -> &ar
				case 13: state = 14; break; // &Ab -> &Abr
				case 18: state = 19; break; // &ab -> &abr
				case 27: state = 28; break; // &Aci -> &Acir
				case 30: state = 31; break; // &aci -> &acir
				case 46: state = 49; break; // &af -> &afr
				case 47: state = 48; break; // &Af -> &Afr
				case 50: state = 51; break; // &Ag -> &Agr
				case 55: state = 56; break; // &ag -> &agr
				case 77: state = 78; break; // &Amac -> &Amacr
				case 81: state = 82; break; // &amac -> &amacr
				case 102: state = 118; break; // &ang -> &angr
				case 128: state = 129; break; // &angza -> &angzar
				case 129: state = 130; break; // &angzar -> &angzarr
				case 146: state = 147; break; // &apaci -> &apacir
				case 166: state = 167; break; // &app -> &appr
				case 181: state = 182; break; // &Asc -> &Ascr
				case 184: state = 185; break; // &asc -> &ascr
				case 222: state = 541; break; // &b -> &br
				case 223: state = 258; break; // &ba -> &bar
				case 237: state = 238; break; // &backp -> &backpr
				case 247: state = 537; break; // &B -> &Br
				case 248: state = 256; break; // &Ba -> &Bar
				case 270: state = 271; break; // &bb -> &bbr
				case 274: state = 275; break; // &bbrktb -> &bbrktbr
				case 288: state = 308; break; // &be -> &ber
				case 293: state = 312; break; // &Be -> &Ber
				case 329: state = 330; break; // &Bf -> &Bfr
				case 331: state = 332; break; // &bf -> &bfr
				case 338: state = 339; break; // &bigci -> &bigcir
				case 362: state = 363; break; // &bigsta -> &bigstar
				case 364: state = 365; break; // &bigt -> &bigtr
				case 392: state = 393; break; // &bka -> &bkar
				case 410: state = 411; break; // &blacksqua -> &blacksquar
				case 413: state = 414; break; // &blackt -> &blacktr
				case 420: state = 429; break; // &blacktriangle -> &blacktriangler
				case 474: state = 481; break; // &boxD -> &boxDr
				case 477: state = 483; break; // &boxd -> &boxdr
				case 508: state = 515; break; // &boxU -> &boxUr
				case 511: state = 517; break; // &boxu -> &boxur
				case 518: state = 529; break; // &boxV -> &boxVr
				case 519: state = 531; break; // &boxv -> &boxvr
				case 532: state = 533; break; // &bp -> &bpr
				case 547: state = 548; break; // &brvba -> &brvbar
				case 550: state = 551; break; // &Bsc -> &Bscr
				case 553: state = 554; break; // &bsc -> &bscr
				case 583: state = 965; break; // &C -> &Cr
				case 589: state = 961; break; // &c -> &cr
				case 590: state = 631; break; // &ca -> &car
				case 600: state = 601; break; // &capb -> &capbr
				case 621: state = 622; break; // &CapitalDiffe -> &CapitalDiffer
				case 642: state = 650; break; // &cca -> &ccar
				case 646: state = 647; break; // &Cca -> &Ccar
				case 661: state = 662; break; // &Cci -> &Ccir
				case 664: state = 665; break; // &cci -> &ccir
				case 702: state = 703; break; // &Cente -> &Center
				case 707: state = 708; break; // &cente -> &center
				case 712: state = 713; break; // &Cf -> &Cfr
				case 714: state = 715; break; // &cf -> &cfr
				case 726: state = 727; break; // &checkma -> &checkmar
				case 732: state = 733; break; // &ci -> &cir
				case 739: state = 740; break; // &circlea -> &circlear
				case 740: state = 741; break; // &circlear -> &circlearr
				case 743: state = 748; break; // &circlearrow -> &circlearrowr
				case 758: state = 759; break; // &circledci -> &circledcir
				case 765: state = 766; break; // &Ci -> &Cir
				case 801: state = 802; break; // &cirsci -> &cirscir
				case 816: state = 817; break; // &ClockwiseContou -> &ClockwiseContour
				case 822: state = 823; break; // &ClockwiseContourInteg -> &ClockwiseContourIntegr
				case 829: state = 830; break; // &CloseCu -> &CloseCur
				case 889: state = 890; break; // &Cong -> &Congr
				case 903: state = 904; break; // &Contou -> &Contour
				case 909: state = 910; break; // &ContourInteg -> &ContourIntegr
				case 913: state = 920; break; // &Cop -> &Copr
				case 915: state = 917; break; // &cop -> &copr
				case 930: state = 931; break; // &copys -> &copysr
				case 935: state = 936; break; // &Counte -> &Counter
				case 951: state = 952; break; // &CounterClockwiseContou -> &CounterClockwiseContour
				case 957: state = 958; break; // &CounterClockwiseContourInteg -> &CounterClockwiseContourIntegr
				case 962: state = 963; break; // &cra -> &crar
				case 963: state = 964; break; // &crar -> &crarr
				case 973: state = 974; break; // &Csc -> &Cscr
				case 976: state = 977; break; // &csc -> &cscr
				case 987: state = 1026; break; // &cu -> &cur
				case 989: state = 990; break; // &cuda -> &cudar
				case 990: state = 991; break; // &cudar -> &cudarr
				case 991: state = 993; break; // &cudarr -> &cudarrr
				case 995: state = 996; break; // &cuep -> &cuepr
				case 1000: state = 1001; break; // &cula -> &cular
				case 1001: state = 1002; break; // &cular -> &cularr
				case 1007: state = 1008; break; // &cupb -> &cupbr
				case 1023: state = 1024; break; // &cupo -> &cupor
				case 1026: state = 1051; break; // &cur -> &curr
				case 1027: state = 1028; break; // &cura -> &curar
				case 1028: state = 1029; break; // &curar -> &curarr
				case 1035: state = 1036; break; // &curlyeqp -> &curlyeqpr
				case 1056: state = 1057; break; // &curvea -> &curvear
				case 1057: state = 1058; break; // &curvear -> &curvearr
				case 1060: state = 1065; break; // &curvearrow -> &curvearrowr
				case 1092: state = 1107; break; // &Da -> &Dar
				case 1095: state = 1096; break; // &Dagge -> &Dagger
				case 1097: state = 1590; break; // &d -> &dr
				case 1098: state = 1112; break; // &da -> &dar
				case 1101: state = 1102; break; // &dagge -> &dagger
				case 1107: state = 1108; break; // &Dar -> &Darr
				case 1109: state = 1110; break; // &dA -> &dAr
				case 1110: state = 1111; break; // &dAr -> &dArr
				case 1112: state = 1113; break; // &dar -> &darr
				case 1122: state = 1123; break; // &dbka -> &dbkar
				case 1130: state = 1131; break; // &Dca -> &Dcar
				case 1135: state = 1136; break; // &dca -> &dcar
				case 1143: state = 1148; break; // &dda -> &ddar
				case 1146: state = 1147; break; // &ddagge -> &ddagger
				case 1148: state = 1149; break; // &ddar -> &ddarr
				case 1151: state = 1152; break; // &DDot -> &DDotr
				case 1175: state = 1182; break; // &df -> &dfr
				case 1180: state = 1181; break; // &Df -> &Dfr
				case 1184: state = 1185; break; // &dHa -> &dHar
				case 1187: state = 1188; break; // &dha -> &dhar
				case 1188: state = 1190; break; // &dhar -> &dharr
				case 1193: state = 1194; break; // &Diac -> &Diacr
				case 1218: state = 1219; break; // &DiacriticalG -> &DiacriticalGr
				case 1246: state = 1247; break; // &Diffe -> &Differ
				case 1284: state = 1288; break; // &dlc -> &dlcr
				case 1285: state = 1286; break; // &dlco -> &dlcor
				case 1294: state = 1295; break; // &dolla -> &dollar
				case 1328: state = 1329; break; // &dotsqua -> &dotsquar
				case 1336: state = 1337; break; // &doubleba -> &doublebar
				case 1352: state = 1353; break; // &DoubleContou -> &DoubleContour
				case 1358: state = 1359; break; // &DoubleContourInteg -> &DoubleContourIntegr
				case 1367: state = 1368; break; // &DoubleDownA -> &DoubleDownAr
				case 1368: state = 1369; break; // &DoubleDownAr -> &DoubleDownArr
				case 1376: state = 1377; break; // &DoubleLeftA -> &DoubleLeftAr
				case 1377: state = 1378; break; // &DoubleLeftAr -> &DoubleLeftArr
				case 1386: state = 1387; break; // &DoubleLeftRightA -> &DoubleLeftRightAr
				case 1387: state = 1388; break; // &DoubleLeftRightAr -> &DoubleLeftRightArr
				case 1401: state = 1402; break; // &DoubleLongLeftA -> &DoubleLongLeftAr
				case 1402: state = 1403; break; // &DoubleLongLeftAr -> &DoubleLongLeftArr
				case 1411: state = 1412; break; // &DoubleLongLeftRightA -> &DoubleLongLeftRightAr
				case 1412: state = 1413; break; // &DoubleLongLeftRightAr -> &DoubleLongLeftRightArr
				case 1421: state = 1422; break; // &DoubleLongRightA -> &DoubleLongRightAr
				case 1422: state = 1423; break; // &DoubleLongRightAr -> &DoubleLongRightArr
				case 1431: state = 1432; break; // &DoubleRightA -> &DoubleRightAr
				case 1432: state = 1433; break; // &DoubleRightAr -> &DoubleRightArr
				case 1441: state = 1442; break; // &DoubleUpA -> &DoubleUpAr
				case 1442: state = 1443; break; // &DoubleUpAr -> &DoubleUpArr
				case 1450: state = 1451; break; // &DoubleUpDownA -> &DoubleUpDownAr
				case 1451: state = 1452; break; // &DoubleUpDownAr -> &DoubleUpDownArr
				case 1456: state = 1457; break; // &DoubleVe -> &DoubleVer
				case 1464: state = 1465; break; // &DoubleVerticalBa -> &DoubleVerticalBar
				case 1468: state = 1469; break; // &DownA -> &DownAr
				case 1469: state = 1470; break; // &DownAr -> &DownArr
				case 1473: state = 1474; break; // &Downa -> &Downar
				case 1474: state = 1475; break; // &Downar -> &Downarr
				case 1480: state = 1481; break; // &downa -> &downar
				case 1481: state = 1482; break; // &downar -> &downarr
				case 1486: state = 1487; break; // &DownArrowBa -> &DownArrowBar
				case 1490: state = 1491; break; // &DownArrowUpA -> &DownArrowUpAr
				case 1491: state = 1492; break; // &DownArrowUpAr -> &DownArrowUpArr
				case 1495: state = 1496; break; // &DownB -> &DownBr
				case 1504: state = 1505; break; // &downdowna -> &downdownar
				case 1505: state = 1506; break; // &downdownar -> &downdownarr
				case 1511: state = 1512; break; // &downha -> &downhar
				case 1516: state = 1521; break; // &downharpoon -> &downharpoonr
				case 1539: state = 1540; break; // &DownLeftRightVecto -> &DownLeftRightVector
				case 1548: state = 1549; break; // &DownLeftTeeVecto -> &DownLeftTeeVector
				case 1554: state = 1555; break; // &DownLeftVecto -> &DownLeftVector
				case 1557: state = 1558; break; // &DownLeftVectorBa -> &DownLeftVectorBar
				case 1571: state = 1572; break; // &DownRightTeeVecto -> &DownRightTeeVector
				case 1577: state = 1578; break; // &DownRightVecto -> &DownRightVector
				case 1580: state = 1581; break; // &DownRightVectorBa -> &DownRightVectorBar
				case 1585: state = 1586; break; // &DownTeeA -> &DownTeeAr
				case 1586: state = 1587; break; // &DownTeeAr -> &DownTeeArr
				case 1593: state = 1594; break; // &drbka -> &drbkar
				case 1597: state = 1601; break; // &drc -> &drcr
				case 1598: state = 1599; break; // &drco -> &drcor
				case 1605: state = 1606; break; // &Dsc -> &Dscr
				case 1608: state = 1609; break; // &dsc -> &dscr
				case 1616: state = 1617; break; // &Dst -> &Dstr
				case 1620: state = 1621; break; // &dst -> &dstr
				case 1624: state = 1628; break; // &dt -> &dtr
				case 1632: state = 1633; break; // &dua -> &duar
				case 1633: state = 1634; break; // &duar -> &duarr
				case 1636: state = 1637; break; // &duha -> &duhar
				case 1651: state = 1652; break; // &dzig -> &dzigr
				case 1653: state = 1654; break; // &dzigra -> &dzigrar
				case 1654: state = 1655; break; // &dzigrar -> &dzigrarr
				case 1662: state = 1890; break; // &e -> &er
				case 1670: state = 1671; break; // &easte -> &easter
				case 1673: state = 1674; break; // &Eca -> &Ecar
				case 1678: state = 1679; break; // &eca -> &ecar
				case 1682: state = 1683; break; // &eci -> &ecir
				case 1684: state = 1685; break; // &Eci -> &Ecir
				case 1707: state = 1713; break; // &ef -> &efr
				case 1711: state = 1712; break; // &Ef -> &Efr
				case 1714: state = 1720; break; // &eg -> &egr
				case 1715: state = 1716; break; // &Eg -> &Egr
				case 1738: state = 1739; break; // &elinte -> &elinter
				case 1748: state = 1749; break; // &Emac -> &Emacr
				case 1752: state = 1753; break; // &emac -> &emacr
				case 1771: state = 1772; break; // &EmptySmallSqua -> &EmptySmallSquar
				case 1776: state = 1777; break; // &EmptyVe -> &EmptyVer
				case 1787: state = 1788; break; // &EmptyVerySmallSqua -> &EmptyVerySmallSquar
				case 1814: state = 1815; break; // &epa -> &epar
				case 1835: state = 1836; break; // &eqci -> &eqcir
				case 1850: state = 1851; break; // &eqslantgt -> &eqslantgtr
				case 1875: state = 1876; break; // &Equilib -> &Equilibr
				case 1886: state = 1887; break; // &eqvpa -> &eqvpar
				case 1891: state = 1892; break; // &era -> &erar
				case 1892: state = 1893; break; // &erar -> &erarr
				case 1898: state = 1899; break; // &Esc -> &Escr
				case 1901: state = 1902; break; // &esc -> &escr
				case 1920: state = 1923; break; // &eu -> &eur
				case 1964: state = 2083; break; // &f -> &fr
				case 1987: state = 2000; break; // &ff -> &ffr
				case 1998: state = 1999; break; // &Ff -> &Ffr
				case 2018: state = 2019; break; // &FilledSmallSqua -> &FilledSmallSquar
				case 2022: state = 2023; break; // &FilledVe -> &FilledVer
				case 2033: state = 2034; break; // &FilledVerySmallSqua -> &FilledVerySmallSquar
				case 2052: state = 2058; break; // &Fo -> &For
				case 2055: state = 2062; break; // &fo -> &for
				case 2068: state = 2069; break; // &Fou -> &Four
				case 2071: state = 2072; break; // &Fourie -> &Fourier
				case 2073: state = 2074; break; // &Fouriert -> &Fouriertr
				case 2077: state = 2078; break; // &fpa -> &fpar
				case 2113: state = 2114; break; // &Fsc -> &Fscr
				case 2116: state = 2117; break; // &fsc -> &fscr
				case 2118: state = 2228; break; // &g -> &gr
				case 2124: state = 2232; break; // &G -> &Gr
				case 2135: state = 2136; break; // &Gb -> &Gbr
				case 2140: state = 2141; break; // &gb -> &gbr
				case 2150: state = 2151; break; // &Gci -> &Gcir
				case 2154: state = 2155; break; // &gci -> &gcir
				case 2187: state = 2188; break; // &Gf -> &Gfr
				case 2189: state = 2190; break; // &gf -> &gfr
				case 2211: state = 2212; break; // &gnapp -> &gnappr
				case 2236: state = 2237; break; // &Greate -> &Greater
				case 2256: state = 2257; break; // &GreaterG -> &GreaterGr
				case 2261: state = 2262; break; // &GreaterGreate -> &GreaterGreater
				case 2283: state = 2284; break; // &Gsc -> &Gscr
				case 2286: state = 2287; break; // &gsc -> &gscr
				case 2294: state = 2311; break; // &gt -> &gtr
				case 2297: state = 2298; break; // &gtci -> &gtcir
				case 2304: state = 2305; break; // &gtlPa -> &gtlPar
				case 2312: state = 2318; break; // &gtra -> &gtrar
				case 2314: state = 2315; break; // &gtrapp -> &gtrappr
				case 2318: state = 2319; break; // &gtrar -> &gtrarr
				case 2342: state = 2343; break; // &gve -> &gver
				case 2357: state = 2373; break; // &ha -> &har
				case 2358: state = 2359; break; // &hai -> &hair
				case 2373: state = 2380; break; // &har -> &harr
				case 2377: state = 2378; break; // &hA -> &hAr
				case 2378: state = 2379; break; // &hAr -> &hArr
				case 2382: state = 2383; break; // &harrci -> &harrcir
				case 2387: state = 2388; break; // &hba -> &hbar
				case 2390: state = 2391; break; // &Hci -> &Hcir
				case 2394: state = 2395; break; // &hci -> &hcir
				case 2397: state = 2409; break; // &he -> &her
				case 2398: state = 2399; break; // &hea -> &hear
				case 2413: state = 2414; break; // &Hf -> &Hfr
				case 2415: state = 2416; break; // &hf -> &hfr
				case 2420: state = 2421; break; // &Hilbe -> &Hilber
				case 2431: state = 2432; break; // &hksea -> &hksear
				case 2436: state = 2437; break; // &hkswa -> &hkswar
				case 2440: state = 2474; break; // &ho -> &hor
				case 2441: state = 2442; break; // &hoa -> &hoar
				case 2442: state = 2443; break; // &hoar -> &hoarr
				case 2449: state = 2459; break; // &hook -> &hookr
				case 2454: state = 2455; break; // &hooklefta -> &hookleftar
				case 2455: state = 2456; break; // &hookleftar -> &hookleftarr
				case 2464: state = 2465; break; // &hookrighta -> &hookrightar
				case 2465: state = 2466; break; // &hookrightar -> &hookrightarr
				case 2469: state = 2478; break; // &Ho -> &Hor
				case 2476: state = 2477; break; // &horba -> &horbar
				case 2491: state = 2492; break; // &Hsc -> &Hscr
				case 2494: state = 2495; break; // &hsc -> &hscr
				case 2500: state = 2501; break; // &Hst -> &Hstr
				case 2504: state = 2505; break; // &hst -> &hstr
				case 2547: state = 2548; break; // &Ici -> &Icir
				case 2550: state = 2551; break; // &ici -> &icir
				case 2567: state = 2571; break; // &if -> &ifr
				case 2569: state = 2570; break; // &If -> &Ifr
				case 2572: state = 2573; break; // &Ig -> &Igr
				case 2577: state = 2578; break; // &ig -> &igr
				case 2606: state = 2607; break; // &Imac -> &Imacr
				case 2610: state = 2611; break; // &imac -> &imacr
				case 2617: state = 2618; break; // &Imagina -> &Imaginar
				case 2626: state = 2627; break; // &imagpa -> &imagpar
				case 2643: state = 2644; break; // &inca -> &incar
				case 2662: state = 2672; break; // &inte -> &inter
				case 2664: state = 2665; break; // &intege -> &integer
				case 2667: state = 2676; break; // &Inte -> &Inter
				case 2668: state = 2669; break; // &Integ -> &Integr
				case 2685: state = 2686; break; // &intla -> &intlar
				case 2689: state = 2690; break; // &intp -> &intpr
				case 2731: state = 2732; break; // &ip -> &ipr
				case 2741: state = 2742; break; // &Isc -> &Iscr
				case 2744: state = 2745; break; // &isc -> &iscr
				case 2779: state = 2780; break; // &Jci -> &Jcir
				case 2784: state = 2785; break; // &jci -> &jcir
				case 2789: state = 2790; break; // &Jf -> &Jfr
				case 2791: state = 2792; break; // &jf -> &jfr
				case 2804: state = 2805; break; // &Jsc -> &Jscr
				case 2807: state = 2808; break; // &jsc -> &jscr
				case 2809: state = 2810; break; // &Jse -> &Jser
				case 2813: state = 2814; break; // &jse -> &jser
				case 2848: state = 2849; break; // &Kf -> &Kfr
				case 2850: state = 2851; break; // &kf -> &kfr
				case 2852: state = 2853; break; // &kg -> &kgr
				case 2876: state = 2877; break; // &Ksc -> &Kscr
				case 2879: state = 2880; break; // &ksc -> &kscr
				case 2881: state = 3643; break; // &l -> &lr
				case 2882: state = 2936; break; // &lA -> &lAr
				case 2883: state = 2884; break; // &lAa -> &lAar
				case 2884: state = 2885; break; // &lAar -> &lAarr
				case 2887: state = 2934; break; // &La -> &Lar
				case 2892: state = 2938; break; // &la -> &lar
				case 2903: state = 2904; break; // &lag -> &lagr
				case 2928: state = 2929; break; // &Laplacet -> &Laplacetr
				case 2934: state = 2935; break; // &Lar -> &Larr
				case 2936: state = 2937; break; // &lAr -> &lArr
				case 2938: state = 2939; break; // &lar -> &larr
				case 2967: state = 2968; break; // &lBa -> &lBar
				case 2968: state = 2969; break; // &lBar -> &lBarr
				case 2970: state = 2977; break; // &lb -> &lbr
				case 2971: state = 2972; break; // &lba -> &lbar
				case 2972: state = 2973; break; // &lbar -> &lbarr
				case 2974: state = 2975; break; // &lbb -> &lbbr
				case 2989: state = 2990; break; // &Lca -> &Lcar
				case 2994: state = 2995; break; // &lca -> &lcar
				case 3012: state = 3019; break; // &ld -> &ldr
				case 3017: state = 3018; break; // &ldquo -> &ldquor
				case 3022: state = 3023; break; // &ldrdha -> &ldrdhar
				case 3027: state = 3028; break; // &ldrusha -> &ldrushar
				case 3035: state = 3159; break; // &Left -> &Leftr
				case 3036: state = 3048; break; // &LeftA -> &LeftAr
				case 3041: state = 3042; break; // &LeftAngleB -> &LeftAngleBr
				case 3048: state = 3049; break; // &LeftAr -> &LeftArr
				case 3052: state = 3053; break; // &Lefta -> &Leftar
				case 3053: state = 3054; break; // &Leftar -> &Leftarr
				case 3058: state = 3169; break; // &left -> &leftr
				case 3059: state = 3060; break; // &lefta -> &leftar
				case 3060: state = 3061; break; // &leftar -> &leftarr
				case 3065: state = 3066; break; // &LeftArrowBa -> &LeftArrowBar
				case 3072: state = 3073; break; // &LeftArrowRightA -> &LeftArrowRightAr
				case 3073: state = 3074; break; // &LeftArrowRightAr -> &LeftArrowRightArr
				case 3094: state = 3095; break; // &LeftDoubleB -> &LeftDoubleBr
				case 3110: state = 3111; break; // &LeftDownTeeVecto -> &LeftDownTeeVector
				case 3116: state = 3117; break; // &LeftDownVecto -> &LeftDownVector
				case 3119: state = 3120; break; // &LeftDownVectorBa -> &LeftDownVectorBar
				case 3124: state = 3125; break; // &LeftFloo -> &LeftFloor
				case 3127: state = 3128; break; // &leftha -> &lefthar
				case 3143: state = 3144; break; // &leftlefta -> &leftleftar
				case 3144: state = 3145; break; // &leftleftar -> &leftleftarr
				case 3154: state = 3155; break; // &LeftRightA -> &LeftRightAr
				case 3155: state = 3156; break; // &LeftRightAr -> &LeftRightArr
				case 3164: state = 3165; break; // &Leftrighta -> &Leftrightar
				case 3165: state = 3166; break; // &Leftrightar -> &Leftrightarr
				case 3174: state = 3175; break; // &leftrighta -> &leftrightar
				case 3175: state = 3176; break; // &leftrightar -> &leftrightarr
				case 3181: state = 3182; break; // &leftrightha -> &leftrighthar
				case 3193: state = 3194; break; // &leftrightsquiga -> &leftrightsquigar
				case 3194: state = 3195; break; // &leftrightsquigar -> &leftrightsquigarr
				case 3202: state = 3203; break; // &LeftRightVecto -> &LeftRightVector
				case 3204: state = 3228; break; // &LeftT -> &LeftTr
				case 3207: state = 3208; break; // &LeftTeeA -> &LeftTeeAr
				case 3208: state = 3209; break; // &LeftTeeAr -> &LeftTeeArr
				case 3216: state = 3217; break; // &LeftTeeVecto -> &LeftTeeVector
				case 3219: state = 3220; break; // &leftth -> &leftthr
				case 3236: state = 3237; break; // &LeftTriangleBa -> &LeftTriangleBar
				case 3253: state = 3254; break; // &LeftUpDownVecto -> &LeftUpDownVector
				case 3262: state = 3263; break; // &LeftUpTeeVecto -> &LeftUpTeeVector
				case 3268: state = 3269; break; // &LeftUpVecto -> &LeftUpVector
				case 3271: state = 3272; break; // &LeftUpVectorBa -> &LeftUpVectorBar
				case 3277: state = 3278; break; // &LeftVecto -> &LeftVector
				case 3280: state = 3281; break; // &LeftVectorBa -> &LeftVectorBar
				case 3297: state = 3298; break; // &lesdoto -> &lesdotor
				case 3305: state = 3306; break; // &lessapp -> &lessappr
				case 3315: state = 3316; break; // &lesseqgt -> &lesseqgtr
				case 3319: state = 3320; break; // &lesseqqgt -> &lesseqqgtr
				case 3328: state = 3329; break; // &LessEqualG -> &LessEqualGr
				case 3333: state = 3334; break; // &LessEqualGreate -> &LessEqualGreater
				case 3344: state = 3345; break; // &LessG -> &LessGr
				case 3349: state = 3350; break; // &LessGreate -> &LessGreater
				case 3352: state = 3353; break; // &lessgt -> &lessgtr
				case 3376: state = 3387; break; // &lf -> &lfr
				case 3383: state = 3384; break; // &lfloo -> &lfloor
				case 3385: state = 3386; break; // &Lf -> &Lfr
				case 3391: state = 3392; break; // &lHa -> &lHar
				case 3394: state = 3395; break; // &lha -> &lhar
				case 3410: state = 3411; break; // &lla -> &llar
				case 3411: state = 3412; break; // &llar -> &llarr
				case 3414: state = 3415; break; // &llco -> &llcor
				case 3417: state = 3418; break; // &llcorne -> &llcorner
				case 3422: state = 3423; break; // &Llefta -> &Lleftar
				case 3423: state = 3424; break; // &Lleftar -> &Lleftarr
				case 3428: state = 3429; break; // &llha -> &llhar
				case 3431: state = 3432; break; // &llt -> &lltr
				case 3455: state = 3456; break; // &lnapp -> &lnappr
				case 3467: state = 3470; break; // &loa -> &loar
				case 3470: state = 3471; break; // &loar -> &loarr
				case 3472: state = 3473; break; // &lob -> &lobr
				case 3477: state = 3553; break; // &Long -> &Longr
				case 3482: state = 3483; break; // &LongLeftA -> &LongLeftAr
				case 3483: state = 3484; break; // &LongLeftAr -> &LongLeftArr
				case 3490: state = 3517; break; // &Longleft -> &Longleftr
				case 3491: state = 3492; break; // &Longlefta -> &Longleftar
				case 3492: state = 3493; break; // &Longleftar -> &Longleftarr
				case 3497: state = 3563; break; // &long -> &longr
				case 3501: state = 3527; break; // &longleft -> &longleftr
				case 3502: state = 3503; break; // &longlefta -> &longleftar
				case 3503: state = 3504; break; // &longleftar -> &longleftarr
				case 3512: state = 3513; break; // &LongLeftRightA -> &LongLeftRightAr
				case 3513: state = 3514; break; // &LongLeftRightAr -> &LongLeftRightArr
				case 3522: state = 3523; break; // &Longleftrighta -> &Longleftrightar
				case 3523: state = 3524; break; // &Longleftrightar -> &Longleftrightarr
				case 3532: state = 3533; break; // &longleftrighta -> &longleftrightar
				case 3533: state = 3534; break; // &longleftrightar -> &longleftrightarr
				case 3548: state = 3549; break; // &LongRightA -> &LongRightAr
				case 3549: state = 3550; break; // &LongRightAr -> &LongRightArr
				case 3558: state = 3559; break; // &Longrighta -> &Longrightar
				case 3559: state = 3560; break; // &Longrightar -> &Longrightarr
				case 3568: state = 3569; break; // &longrighta -> &longrightar
				case 3569: state = 3570; break; // &longrightar -> &longrightarr
				case 3575: state = 3576; break; // &loopa -> &loopar
				case 3576: state = 3577; break; // &loopar -> &looparr
				case 3579: state = 3584; break; // &looparrow -> &looparrowr
				case 3590: state = 3591; break; // &lopa -> &lopar
				case 3608: state = 3609; break; // &lowba -> &lowbar
				case 3611: state = 3612; break; // &Lowe -> &Lower
				case 3617: state = 3618; break; // &LowerLeftA -> &LowerLeftAr
				case 3618: state = 3619; break; // &LowerLeftAr -> &LowerLeftArr
				case 3627: state = 3628; break; // &LowerRightA -> &LowerRightAr
				case 3628: state = 3629; break; // &LowerRightAr -> &LowerRightArr
				case 3639: state = 3640; break; // &lpa -> &lpar
				case 3644: state = 3645; break; // &lra -> &lrar
				case 3645: state = 3646; break; // &lrar -> &lrarr
				case 3648: state = 3649; break; // &lrco -> &lrcor
				case 3651: state = 3652; break; // &lrcorne -> &lrcorner
				case 3654: state = 3655; break; // &lrha -> &lrhar
				case 3658: state = 3659; break; // &lrt -> &lrtr
				case 3667: state = 3668; break; // &Lsc -> &Lscr
				case 3669: state = 3670; break; // &lsc -> &lscr
				case 3680: state = 3681; break; // &lsquo -> &lsquor
				case 3682: state = 3683; break; // &Lst -> &Lstr
				case 3686: state = 3687; break; // &lst -> &lstr
				case 3692: state = 3717; break; // &lt -> &ltr
				case 3695: state = 3696; break; // &ltci -> &ltcir
				case 3700: state = 3701; break; // &lth -> &lthr
				case 3709: state = 3710; break; // &ltla -> &ltlar
				case 3710: state = 3711; break; // &ltlar -> &ltlarr
				case 3722: state = 3723; break; // &ltrPa -> &ltrPar
				case 3724: state = 3725; break; // &lu -> &lur
				case 3729: state = 3730; break; // &lurdsha -> &lurdshar
				case 3733: state = 3734; break; // &luruha -> &luruhar
				case 3736: state = 3737; break; // &lve -> &lver
				case 3746: state = 3772; break; // &ma -> &mar
				case 3747: state = 3748; break; // &mac -> &macr
				case 3774: state = 3775; break; // &marke -> &marker
				case 3795: state = 3796; break; // &measu -> &measur
				case 3818: state = 3819; break; // &Mellint -> &Mellintr
				case 3821: state = 3822; break; // &Mf -> &Mfr
				case 3823: state = 3824; break; // &mf -> &mfr
				case 3828: state = 3829; break; // &mic -> &micr
				case 3836: state = 3837; break; // &midci -> &midcir
				case 3858: state = 3859; break; // &mld -> &mldr
				case 3877: state = 3878; break; // &Msc -> &Mscr
				case 3880: state = 3881; break; // &msc -> &mscr
				case 3897: state = 4621; break; // &n -> &nr
				case 3920: state = 3921; break; // &napp -> &nappr
				case 3925: state = 3926; break; // &natu -> &natur
				case 3938: state = 3945; break; // &nca -> &ncar
				case 3941: state = 3942; break; // &Nca -> &Ncar
				case 3971: state = 3972; break; // &nea -> &near
				case 3972: state = 3978; break; // &near -> &nearr
				case 3975: state = 3976; break; // &neA -> &neAr
				case 3976: state = 3977; break; // &neAr -> &neArr
				case 4019: state = 4020; break; // &NegativeVe -> &NegativeVer
				case 4037: state = 4038; break; // &nesea -> &nesear
				case 4045: state = 4046; break; // &NestedG -> &NestedGr
				case 4050: state = 4051; break; // &NestedGreate -> &NestedGreater
				case 4052: state = 4053; break; // &NestedGreaterG -> &NestedGreaterGr
				case 4057: state = 4058; break; // &NestedGreaterGreate -> &NestedGreaterGreater
				case 4077: state = 4078; break; // &Nf -> &Nfr
				case 4079: state = 4080; break; // &nf -> &nfr
				case 4098: state = 4099; break; // &ngt -> &ngtr
				case 4102: state = 4103; break; // &nhA -> &nhAr
				case 4103: state = 4104; break; // &nhAr -> &nhArr
				case 4105: state = 4106; break; // &nha -> &nhar
				case 4106: state = 4107; break; // &nhar -> &nharr
				case 4109: state = 4110; break; // &nhpa -> &nhpar
				case 4122: state = 4123; break; // &nlA -> &nlAr
				case 4123: state = 4124; break; // &nlAr -> &nlArr
				case 4125: state = 4126; break; // &nla -> &nlar
				case 4126: state = 4127; break; // &nlar -> &nlarr
				case 4128: state = 4129; break; // &nld -> &nldr
				case 4135: state = 4148; break; // &nLeft -> &nLeftr
				case 4136: state = 4137; break; // &nLefta -> &nLeftar
				case 4137: state = 4138; break; // &nLeftar -> &nLeftarr
				case 4142: state = 4158; break; // &nleft -> &nleftr
				case 4143: state = 4144; break; // &nlefta -> &nleftar
				case 4144: state = 4145; break; // &nleftar -> &nleftarr
				case 4153: state = 4154; break; // &nLeftrighta -> &nLeftrightar
				case 4154: state = 4155; break; // &nLeftrightar -> &nLeftrightarr
				case 4163: state = 4164; break; // &nleftrighta -> &nleftrightar
				case 4164: state = 4165; break; // &nleftrightar -> &nleftrightarr
				case 4182: state = 4183; break; // &nlt -> &nltr
				case 4191: state = 4192; break; // &NoB -> &NoBr
				case 4197: state = 4198; break; // &NonB -> &NonBr
				case 4220: state = 4221; break; // &NotCong -> &NotCongr
				case 4238: state = 4239; break; // &NotDoubleVe -> &NotDoubleVer
				case 4246: state = 4247; break; // &NotDoubleVerticalBa -> &NotDoubleVerticalBar
				case 4269: state = 4270; break; // &NotG -> &NotGr
				case 4274: state = 4275; break; // &NotGreate -> &NotGreater
				case 4290: state = 4291; break; // &NotGreaterG -> &NotGreaterGr
				case 4295: state = 4296; break; // &NotGreaterGreate -> &NotGreaterGreater
				case 4347: state = 4348; break; // &NotLeftT -> &NotLeftTr
				case 4356: state = 4357; break; // &NotLeftTriangleBa -> &NotLeftTriangleBar
				case 4370: state = 4371; break; // &NotLessG -> &NotLessGr
				case 4375: state = 4376; break; // &NotLessGreate -> &NotLessGreater
				case 4402: state = 4403; break; // &NotNestedG -> &NotNestedGr
				case 4407: state = 4408; break; // &NotNestedGreate -> &NotNestedGreater
				case 4409: state = 4410; break; // &NotNestedGreaterG -> &NotNestedGreaterGr
				case 4414: state = 4415; break; // &NotNestedGreaterGreate -> &NotNestedGreaterGreater
				case 4430: state = 4431; break; // &NotP -> &NotPr
				case 4456: state = 4457; break; // &NotReve -> &NotRever
				case 4471: state = 4472; break; // &NotRightT -> &NotRightTr
				case 4480: state = 4481; break; // &NotRightTriangleBa -> &NotRightTriangleBar
				case 4490: state = 4491; break; // &NotSqua -> &NotSquar
				case 4505: state = 4506; break; // &NotSquareSupe -> &NotSquareSuper
				case 4552: state = 4553; break; // &NotSupe -> &NotSuper
				case 4587: state = 4588; break; // &NotVe -> &NotVer
				case 4595: state = 4596; break; // &NotVerticalBa -> &NotVerticalBar
				case 4597: state = 4613; break; // &np -> &npr
				case 4598: state = 4599; break; // &npa -> &npar
				case 4622: state = 4623; break; // &nrA -> &nrAr
				case 4623: state = 4624; break; // &nrAr -> &nrArr
				case 4625: state = 4626; break; // &nra -> &nrar
				case 4626: state = 4627; break; // &nrar -> &nrarr
				case 4635: state = 4636; break; // &nRighta -> &nRightar
				case 4636: state = 4637; break; // &nRightar -> &nRightarr
				case 4644: state = 4645; break; // &nrighta -> &nrightar
				case 4645: state = 4646; break; // &nrightar -> &nrightarr
				case 4649: state = 4650; break; // &nrt -> &nrtr
				case 4654: state = 4662; break; // &nsc -> &nscr
				case 4660: state = 4661; break; // &Nsc -> &Nscr
				case 4664: state = 4665; break; // &nsho -> &nshor
				case 4671: state = 4672; break; // &nshortpa -> &nshortpar
				case 4686: state = 4687; break; // &nspa -> &nspar
				case 4718: state = 4732; break; // &nt -> &ntr
				case 4738: state = 4745; break; // &ntriangle -> &ntriangler
				case 4755: state = 4756; break; // &nume -> &numer
				case 4760: state = 4801; break; // &nv -> &nvr
				case 4784: state = 4785; break; // &nvHa -> &nvHar
				case 4785: state = 4786; break; // &nvHar -> &nvHarr
				case 4793: state = 4794; break; // &nvlA -> &nvlAr
				case 4794: state = 4795; break; // &nvlAr -> &nvlArr
				case 4797: state = 4798; break; // &nvlt -> &nvltr
				case 4802: state = 4803; break; // &nvrA -> &nvrAr
				case 4803: state = 4804; break; // &nvrAr -> &nvrArr
				case 4805: state = 4806; break; // &nvrt -> &nvrtr
				case 4813: state = 4814; break; // &nwa -> &nwar
				case 4814: state = 4820; break; // &nwar -> &nwarr
				case 4817: state = 4818; break; // &nwA -> &nwAr
				case 4818: state = 4819; break; // &nwAr -> &nwArr
				case 4825: state = 4826; break; // &nwnea -> &nwnear
				case 4827: state = 4990; break; // &O -> &Or
				case 4833: state = 4991; break; // &o -> &or
				case 4842: state = 4843; break; // &oci -> &ocir
				case 4845: state = 4846; break; // &Oci -> &Ocir
				case 4880: state = 4886; break; // &of -> &ofr
				case 4882: state = 4883; break; // &ofci -> &ofcir
				case 4884: state = 4885; break; // &Of -> &Ofr
				case 4887: state = 4895; break; // &og -> &ogr
				case 4890: state = 4891; break; // &Og -> &Ogr
				case 4902: state = 4903; break; // &ohba -> &ohbar
				case 4909: state = 4910; break; // &ola -> &olar
				case 4910: state = 4911; break; // &olar -> &olarr
				case 4912: state = 4915; break; // &olc -> &olcr
				case 4913: state = 4914; break; // &olci -> &olcir
				case 4925: state = 4926; break; // &Omac -> &Omacr
				case 4929: state = 4930; break; // &omac -> &omacr
				case 4938: state = 4939; break; // &Omic -> &Omicr
				case 4943: state = 4944; break; // &omic -> &omicr
				case 4958: state = 4959; break; // &opa -> &opar
				case 4964: state = 4965; break; // &OpenCu -> &OpenCur
				case 4984: state = 4985; break; // &ope -> &oper
				case 4992: state = 4993; break; // &ora -> &orar
				case 4993: state = 4994; break; // &orar -> &orarr
				case 4996: state = 4997; break; // &orde -> &order
				case 5006: state = 5007; break; // &oro -> &oror
				case 5016: state = 5017; break; // &Osc -> &Oscr
				case 5019: state = 5020; break; // &osc -> &oscr
				case 5057: state = 5058; break; // &ovba -> &ovbar
				case 5060: state = 5061; break; // &Ove -> &Over
				case 5062: state = 5065; break; // &OverB -> &OverBr
				case 5063: state = 5064; break; // &OverBa -> &OverBar
				case 5073: state = 5074; break; // &OverPa -> &OverPar
				case 5083: state = 5216; break; // &p -> &pr
				case 5084: state = 5085; break; // &pa -> &par
				case 5096: state = 5215; break; // &P -> &Pr
				case 5097: state = 5098; break; // &Pa -> &Par
				case 5108: state = 5109; break; // &pe -> &per
				case 5124: state = 5125; break; // &Pf -> &Pfr
				case 5126: state = 5127; break; // &pf -> &pfr
				case 5146: state = 5147; break; // &pitchfo -> &pitchfor
				case 5162: state = 5163; break; // &plusaci -> &plusacir
				case 5166: state = 5167; break; // &plusci -> &pluscir
				case 5193: state = 5194; break; // &Poinca -> &Poincar
				case 5227: state = 5228; break; // &precapp -> &precappr
				case 5232: state = 5233; break; // &preccu -> &preccur
				case 5269: state = 5270; break; // &precnapp -> &precnappr
				case 5306: state = 5307; break; // &profala -> &profalar
				case 5313: state = 5314; break; // &profsu -> &profsur
				case 5318: state = 5319; break; // &Propo -> &Propor
				case 5331: state = 5332; break; // &pru -> &prur
				case 5336: state = 5337; break; // &Psc -> &Pscr
				case 5339: state = 5340; break; // &psc -> &pscr
				case 5349: state = 5350; break; // &Qf -> &Qfr
				case 5352: state = 5353; break; // &qf -> &qfr
				case 5363: state = 5364; break; // &qp -> &qpr
				case 5369: state = 5370; break; // &Qsc -> &Qscr
				case 5372: state = 5373; break; // &qsc -> &qscr
				case 5377: state = 5378; break; // &quate -> &quater
				case 5397: state = 5920; break; // &r -> &rr
				case 5398: state = 5436; break; // &rA -> &rAr
				case 5399: state = 5400; break; // &rAa -> &rAar
				case 5400: state = 5401; break; // &rAar -> &rAarr
				case 5402: state = 5438; break; // &ra -> &rar
				case 5405: state = 5924; break; // &R -> &Rr
				case 5406: state = 5434; break; // &Ra -> &Rar
				case 5434: state = 5435; break; // &Rar -> &Rarr
				case 5436: state = 5437; break; // &rAr -> &rArr
				case 5438: state = 5439; break; // &rar -> &rarr
				case 5477: state = 5478; break; // &RBa -> &RBar
				case 5478: state = 5479; break; // &RBar -> &RBarr
				case 5481: state = 5482; break; // &rBa -> &rBar
				case 5482: state = 5483; break; // &rBar -> &rBarr
				case 5484: state = 5491; break; // &rb -> &rbr
				case 5485: state = 5486; break; // &rba -> &rbar
				case 5486: state = 5487; break; // &rbar -> &rbarr
				case 5488: state = 5489; break; // &rbb -> &rbbr
				case 5503: state = 5504; break; // &Rca -> &Rcar
				case 5508: state = 5509; break; // &rca -> &rcar
				case 5532: state = 5533; break; // &rdldha -> &rdldhar
				case 5536: state = 5537; break; // &rdquo -> &rdquor
				case 5548: state = 5549; break; // &realpa -> &realpar
				case 5558: state = 5559; break; // &Reve -> &Rever
				case 5574: state = 5575; break; // &ReverseEquilib -> &ReverseEquilibr
				case 5587: state = 5588; break; // &ReverseUpEquilib -> &ReverseUpEquilibr
				case 5592: state = 5603; break; // &rf -> &rfr
				case 5599: state = 5600; break; // &rfloo -> &rfloor
				case 5601: state = 5602; break; // &Rf -> &Rfr
				case 5605: state = 5606; break; // &rHa -> &rHar
				case 5608: state = 5609; break; // &rha -> &rhar
				case 5621: state = 5633; break; // &RightA -> &RightAr
				case 5626: state = 5627; break; // &RightAngleB -> &RightAngleBr
				case 5633: state = 5634; break; // &RightAr -> &RightArr
				case 5637: state = 5638; break; // &Righta -> &Rightar
				case 5638: state = 5639; break; // &Rightar -> &Rightarr
				case 5645: state = 5743; break; // &right -> &rightr
				case 5646: state = 5647; break; // &righta -> &rightar
				case 5647: state = 5648; break; // &rightar -> &rightarr
				case 5652: state = 5653; break; // &RightArrowBa -> &RightArrowBar
				case 5658: state = 5659; break; // &RightArrowLeftA -> &RightArrowLeftAr
				case 5659: state = 5660; break; // &RightArrowLeftAr -> &RightArrowLeftArr
				case 5680: state = 5681; break; // &RightDoubleB -> &RightDoubleBr
				case 5696: state = 5697; break; // &RightDownTeeVecto -> &RightDownTeeVector
				case 5702: state = 5703; break; // &RightDownVecto -> &RightDownVector
				case 5705: state = 5706; break; // &RightDownVectorBa -> &RightDownVectorBar
				case 5710: state = 5711; break; // &RightFloo -> &RightFloor
				case 5713: state = 5714; break; // &rightha -> &righthar
				case 5729: state = 5730; break; // &rightlefta -> &rightleftar
				case 5730: state = 5731; break; // &rightleftar -> &rightleftarr
				case 5736: state = 5737; break; // &rightleftha -> &rightlefthar
				case 5748: state = 5749; break; // &rightrighta -> &rightrightar
				case 5749: state = 5750; break; // &rightrightar -> &rightrightarr
				case 5759: state = 5760; break; // &rightsquiga -> &rightsquigar
				case 5760: state = 5761; break; // &rightsquigar -> &rightsquigarr
				case 5764: state = 5788; break; // &RightT -> &RightTr
				case 5767: state = 5768; break; // &RightTeeA -> &RightTeeAr
				case 5768: state = 5769; break; // &RightTeeAr -> &RightTeeArr
				case 5776: state = 5777; break; // &RightTeeVecto -> &RightTeeVector
				case 5779: state = 5780; break; // &rightth -> &rightthr
				case 5796: state = 5797; break; // &RightTriangleBa -> &RightTriangleBar
				case 5813: state = 5814; break; // &RightUpDownVecto -> &RightUpDownVector
				case 5822: state = 5823; break; // &RightUpTeeVecto -> &RightUpTeeVector
				case 5828: state = 5829; break; // &RightUpVecto -> &RightUpVector
				case 5831: state = 5832; break; // &RightUpVectorBa -> &RightUpVectorBar
				case 5837: state = 5838; break; // &RightVecto -> &RightVector
				case 5840: state = 5841; break; // &RightVectorBa -> &RightVectorBar
				case 5855: state = 5856; break; // &rla -> &rlar
				case 5856: state = 5857; break; // &rlar -> &rlarr
				case 5859: state = 5860; break; // &rlha -> &rlhar
				case 5876: state = 5879; break; // &roa -> &roar
				case 5879: state = 5880; break; // &roar -> &roarr
				case 5881: state = 5882; break; // &rob -> &robr
				case 5885: state = 5886; break; // &ropa -> &ropar
				case 5910: state = 5911; break; // &rpa -> &rpar
				case 5921: state = 5922; break; // &rra -> &rrar
				case 5922: state = 5923; break; // &rrar -> &rrarr
				case 5929: state = 5930; break; // &Rrighta -> &Rrightar
				case 5930: state = 5931; break; // &Rrightar -> &Rrightarr
				case 5940: state = 5941; break; // &Rsc -> &Rscr
				case 5942: state = 5943; break; // &rsc -> &rscr
				case 5949: state = 5950; break; // &rsquo -> &rsquor
				case 5951: state = 5960; break; // &rt -> &rtr
				case 5952: state = 5953; break; // &rth -> &rthr
				case 5965: state = 5966; break; // &rtrilt -> &rtriltr
				case 5982: state = 5983; break; // &ruluha -> &ruluhar
				case 5991: state = 6334; break; // &s -> &sr
				case 6003: state = 6009; break; // &sca -> &scar
				case 6005: state = 6006; break; // &Sca -> &Scar
				case 6024: state = 6025; break; // &Sci -> &Scir
				case 6027: state = 6028; break; // &sci -> &scir
				case 6054: state = 6055; break; // &sea -> &sear
				case 6055: state = 6061; break; // &sear -> &searr
				case 6058: state = 6059; break; // &seA -> &seAr
				case 6059: state = 6060; break; // &seAr -> &seArr
				case 6070: state = 6071; break; // &seswa -> &seswar
				case 6081: state = 6082; break; // &Sf -> &Sfr
				case 6083: state = 6084; break; // &sf -> &sfr
				case 6089: state = 6090; break; // &sha -> &shar
				case 6105: state = 6106; break; // &Sho -> &Shor
				case 6112: state = 6113; break; // &ShortDownA -> &ShortDownAr
				case 6113: state = 6114; break; // &ShortDownAr -> &ShortDownArr
				case 6121: state = 6122; break; // &ShortLeftA -> &ShortLeftAr
				case 6122: state = 6123; break; // &ShortLeftAr -> &ShortLeftArr
				case 6126: state = 6127; break; // &sho -> &shor
				case 6133: state = 6134; break; // &shortpa -> &shortpar
				case 6145: state = 6146; break; // &ShortRightA -> &ShortRightAr
				case 6146: state = 6147; break; // &ShortRightAr -> &ShortRightArr
				case 6152: state = 6153; break; // &ShortUpA -> &ShortUpAr
				case 6153: state = 6154; break; // &ShortUpAr -> &ShortUpArr
				case 6168: state = 6184; break; // &sim -> &simr
				case 6185: state = 6186; break; // &simra -> &simrar
				case 6186: state = 6187; break; // &simrar -> &simrarr
				case 6189: state = 6190; break; // &sla -> &slar
				case 6190: state = 6191; break; // &slar -> &slarr
				case 6197: state = 6198; break; // &SmallCi -> &SmallCir
				case 6219: state = 6220; break; // &smepa -> &smepar
				case 6242: state = 6243; break; // &solba -> &solbar
				case 6250: state = 6257; break; // &spa -> &spar
				case 6266: state = 6267; break; // &Sq -> &Sqr
				case 6287: state = 6288; break; // &Squa -> &Squar
				case 6290: state = 6291; break; // &squa -> &squar
				case 6296: state = 6297; break; // &SquareInte -> &SquareInter
				case 6317: state = 6318; break; // &SquareSupe -> &SquareSuper
				case 6335: state = 6336; break; // &sra -> &srar
				case 6336: state = 6337; break; // &srar -> &srarr
				case 6339: state = 6340; break; // &Ssc -> &Sscr
				case 6342: state = 6343; break; // &ssc -> &sscr
				case 6353: state = 6354; break; // &ssta -> &sstar
				case 6357: state = 6358; break; // &Sta -> &Star
				case 6359: state = 6363; break; // &st -> &str
				case 6360: state = 6361; break; // &sta -> &star
				case 6384: state = 6404; break; // &sub -> &subr
				case 6405: state = 6406; break; // &subra -> &subrar
				case 6406: state = 6407; break; // &subrar -> &subrarr
				case 6435: state = 6436; break; // &succapp -> &succappr
				case 6440: state = 6441; break; // &succcu -> &succcur
				case 6477: state = 6478; break; // &succnapp -> &succnappr
				case 6515: state = 6516; break; // &Supe -> &Super
				case 6532: state = 6533; break; // &supla -> &suplar
				case 6533: state = 6534; break; // &suplar -> &suplarr
				case 6565: state = 6566; break; // &swa -> &swar
				case 6566: state = 6572; break; // &swar -> &swarr
				case 6569: state = 6570; break; // &swA -> &swAr
				case 6570: state = 6571; break; // &swAr -> &swArr
				case 6577: state = 6578; break; // &swnwa -> &swnwar
				case 6583: state = 6797; break; // &T -> &Tr
				case 6586: state = 6760; break; // &t -> &tr
				case 6587: state = 6588; break; // &ta -> &tar
				case 6594: state = 6595; break; // &tb -> &tbr
				case 6598: state = 6599; break; // &Tca -> &Tcar
				case 6603: state = 6604; break; // &tca -> &tcar
				case 6621: state = 6622; break; // &tel -> &telr
				case 6625: state = 6626; break; // &Tf -> &Tfr
				case 6627: state = 6628; break; // &tf -> &tfr
				case 6630: state = 6631; break; // &the -> &ther
				case 6635: state = 6636; break; // &The -> &Ther
				case 6639: state = 6640; break; // &Therefo -> &Therefor
				case 6643: state = 6644; break; // &therefo -> &therefor
				case 6659: state = 6660; break; // &thickapp -> &thickappr
				case 6693: state = 6694; break; // &tho -> &thor
				case 6727: state = 6728; break; // &timesba -> &timesbar
				case 6740: state = 6741; break; // &topci -> &topcir
				case 6746: state = 6747; break; // &topfo -> &topfor
				case 6751: state = 6752; break; // &tp -> &tpr
				case 6769: state = 6781; break; // &triangle -> &triangler
				case 6822: state = 6823; break; // &Tsc -> &Tscr
				case 6825: state = 6826; break; // &tsc -> &tscr
				case 6837: state = 6838; break; // &Tst -> &Tstr
				case 6841: state = 6842; break; // &tst -> &tstr
				case 6853: state = 6863; break; // &twohead -> &twoheadr
				case 6858: state = 6859; break; // &twoheadlefta -> &twoheadleftar
				case 6859: state = 6860; break; // &twoheadleftar -> &twoheadleftarr
				case 6868: state = 6869; break; // &twoheadrighta -> &twoheadrightar
				case 6869: state = 6870; break; // &twoheadrightar -> &twoheadrightarr
				case 6873: state = 7176; break; // &U -> &Ur
				case 6874: state = 6885; break; // &Ua -> &Uar
				case 6879: state = 7166; break; // &u -> &ur
				case 6880: state = 6890; break; // &ua -> &uar
				case 6885: state = 6886; break; // &Uar -> &Uarr
				case 6887: state = 6888; break; // &uA -> &uAr
				case 6888: state = 6889; break; // &uAr -> &uArr
				case 6890: state = 6891; break; // &uar -> &uarr
				case 6894: state = 6895; break; // &Uarroci -> &Uarrocir
				case 6896: state = 6897; break; // &Ub -> &Ubr
				case 6900: state = 6901; break; // &ub -> &ubr
				case 6911: state = 6912; break; // &Uci -> &Ucir
				case 6915: state = 6916; break; // &uci -> &ucir
				case 6921: state = 6922; break; // &uda -> &udar
				case 6922: state = 6923; break; // &udar -> &udarr
				case 6934: state = 6935; break; // &udha -> &udhar
				case 6936: state = 6943; break; // &uf -> &ufr
				case 6941: state = 6942; break; // &Uf -> &Ufr
				case 6944: state = 6945; break; // &Ug -> &Ugr
				case 6949: state = 6950; break; // &ug -> &ugr
				case 6955: state = 6956; break; // &uHa -> &uHar
				case 6958: state = 6959; break; // &uha -> &uhar
				case 6959: state = 6961; break; // &uhar -> &uharr
				case 6966: state = 6972; break; // &ulc -> &ulcr
				case 6967: state = 6968; break; // &ulco -> &ulcor
				case 6970: state = 6971; break; // &ulcorne -> &ulcorner
				case 6975: state = 6976; break; // &ult -> &ultr
				case 6980: state = 6981; break; // &Umac -> &Umacr
				case 6984: state = 6985; break; // &umac -> &umacr
				case 6989: state = 6990; break; // &Unde -> &Under
				case 6991: state = 6994; break; // &UnderB -> &UnderBr
				case 6992: state = 6993; break; // &UnderBa -> &UnderBar
				case 7002: state = 7003; break; // &UnderPa -> &UnderPar
				case 7032: state = 7033; break; // &UpA -> &UpAr
				case 7033: state = 7034; break; // &UpAr -> &UpArr
				case 7037: state = 7038; break; // &Upa -> &Upar
				case 7038: state = 7039; break; // &Upar -> &Uparr
				case 7043: state = 7044; break; // &upa -> &upar
				case 7044: state = 7045; break; // &upar -> &uparr
				case 7049: state = 7050; break; // &UpArrowBa -> &UpArrowBar
				case 7055: state = 7056; break; // &UpArrowDownA -> &UpArrowDownAr
				case 7056: state = 7057; break; // &UpArrowDownAr -> &UpArrowDownArr
				case 7064: state = 7065; break; // &UpDownA -> &UpDownAr
				case 7065: state = 7066; break; // &UpDownAr -> &UpDownArr
				case 7073: state = 7074; break; // &Updowna -> &Updownar
				case 7074: state = 7075; break; // &Updownar -> &Updownarr
				case 7082: state = 7083; break; // &updowna -> &updownar
				case 7083: state = 7084; break; // &updownar -> &updownarr
				case 7093: state = 7094; break; // &UpEquilib -> &UpEquilibr
				case 7099: state = 7100; break; // &upha -> &uphar
				case 7104: state = 7109; break; // &upharpoon -> &upharpoonr
				case 7118: state = 7119; break; // &Uppe -> &Upper
				case 7124: state = 7125; break; // &UpperLeftA -> &UpperLeftAr
				case 7125: state = 7126; break; // &UpperLeftAr -> &UpperLeftArr
				case 7134: state = 7135; break; // &UpperRightA -> &UpperRightAr
				case 7135: state = 7136; break; // &UpperRightAr -> &UpperRightArr
				case 7153: state = 7154; break; // &UpTeeA -> &UpTeeAr
				case 7154: state = 7155; break; // &UpTeeAr -> &UpTeeArr
				case 7160: state = 7161; break; // &upupa -> &upupar
				case 7161: state = 7162; break; // &upupar -> &upuparr
				case 7167: state = 7173; break; // &urc -> &urcr
				case 7168: state = 7169; break; // &urco -> &urcor
				case 7171: state = 7172; break; // &urcorne -> &urcorner
				case 7183: state = 7184; break; // &urt -> &urtr
				case 7187: state = 7188; break; // &Usc -> &Uscr
				case 7190: state = 7191; break; // &usc -> &uscr
				case 7192: state = 7205; break; // &ut -> &utr
				case 7209: state = 7210; break; // &uua -> &uuar
				case 7210: state = 7211; break; // &uuar -> &uuarr
				case 7223: state = 7417; break; // &v -> &vr
				case 7224: state = 7229; break; // &va -> &var
				case 7226: state = 7227; break; // &vang -> &vangr
				case 7229: state = 7261; break; // &var -> &varr
				case 7249: state = 7253; break; // &varp -> &varpr
				case 7258: state = 7259; break; // &vA -> &vAr
				case 7259: state = 7260; break; // &vAr -> &vArr
				case 7286: state = 7291; break; // &vart -> &vartr
				case 7297: state = 7302; break; // &vartriangle -> &vartriangler
				case 7309: state = 7310; break; // &Vba -> &Vbar
				case 7312: state = 7313; break; // &vBa -> &vBar
				case 7336: state = 7349; break; // &Ve -> &Ver
				case 7338: state = 7353; break; // &ve -> &ver
				case 7341: state = 7342; break; // &veeba -> &veebar
				case 7351: state = 7352; break; // &Verba -> &Verbar
				case 7355: state = 7356; break; // &verba -> &verbar
				case 7364: state = 7365; break; // &VerticalBa -> &VerticalBar
				case 7373: state = 7374; break; // &VerticalSepa -> &VerticalSepar
				case 7377: state = 7378; break; // &VerticalSeparato -> &VerticalSeparator
				case 7394: state = 7395; break; // &Vf -> &Vfr
				case 7396: state = 7397; break; // &vf -> &vfr
				case 7399: state = 7400; break; // &vlt -> &vltr
				case 7413: state = 7414; break; // &vp -> &vpr
				case 7418: state = 7419; break; // &vrt -> &vrtr
				case 7422: state = 7423; break; // &Vsc -> &Vscr
				case 7425: state = 7426; break; // &vsc -> &vscr
				case 7449: state = 7450; break; // &Wci -> &Wcir
				case 7452: state = 7484; break; // &w -> &wr
				case 7454: state = 7455; break; // &wci -> &wcir
				case 7460: state = 7461; break; // &wedba -> &wedbar
				case 7470: state = 7471; break; // &weie -> &weier
				case 7473: state = 7474; break; // &Wf -> &Wfr
				case 7475: state = 7476; break; // &wf -> &wfr
				case 7490: state = 7491; break; // &Wsc -> &Wscr
				case 7493: state = 7494; break; // &wsc -> &wscr
				case 7495: state = 7551; break; // &x -> &xr
				case 7499: state = 7500; break; // &xci -> &xcir
				case 7505: state = 7506; break; // &xdt -> &xdtr
				case 7509: state = 7510; break; // &Xf -> &Xfr
				case 7511: state = 7512; break; // &xf -> &xfr
				case 7514: state = 7515; break; // &xhA -> &xhAr
				case 7515: state = 7516; break; // &xhAr -> &xhArr
				case 7517: state = 7518; break; // &xha -> &xhar
				case 7518: state = 7519; break; // &xhar -> &xharr
				case 7523: state = 7524; break; // &xlA -> &xlAr
				case 7524: state = 7525; break; // &xlAr -> &xlArr
				case 7526: state = 7527; break; // &xla -> &xlar
				case 7527: state = 7528; break; // &xlar -> &xlarr
				case 7552: state = 7553; break; // &xrA -> &xrAr
				case 7553: state = 7554; break; // &xrAr -> &xrArr
				case 7555: state = 7556; break; // &xra -> &xrar
				case 7556: state = 7557; break; // &xrar -> &xrarr
				case 7559: state = 7560; break; // &Xsc -> &Xscr
				case 7562: state = 7563; break; // &xsc -> &xscr
				case 7573: state = 7574; break; // &xut -> &xutr
				case 7601: state = 7602; break; // &Yci -> &Ycir
				case 7605: state = 7606; break; // &yci -> &ycir
				case 7612: state = 7613; break; // &Yf -> &Yfr
				case 7614: state = 7615; break; // &yf -> &yfr
				case 7629: state = 7630; break; // &Ysc -> &Yscr
				case 7632: state = 7633; break; // &ysc -> &yscr
				case 7658: state = 7659; break; // &Zca -> &Zcar
				case 7663: state = 7664; break; // &zca -> &zcar
				case 7677: state = 7678; break; // &zeet -> &zeetr
				case 7680: state = 7681; break; // &Ze -> &Zer
				case 7697: state = 7698; break; // &Zf -> &Zfr
				case 7699: state = 7700; break; // &zf -> &zfr
				case 7708: state = 7709; break; // &zig -> &zigr
				case 7710: state = 7711; break; // &zigra -> &zigrar
				case 7711: state = 7712; break; // &zigrar -> &zigrarr
				case 7720: state = 7721; break; // &Zsc -> &Zscr
				case 7723: state = 7724; break; // &zsc -> &zscr
				default: return false;
				}
				break;
			case 's':
				switch (state) {
				case 0: state = 5991; break; // & -> &s
				case 1: state = 180; break; // &A -> &As
				case 7: state = 183; break; // &a -> &as
				case 62: state = 63; break; // &alef -> &alefs
				case 91: state = 96; break; // &and -> &ands
				case 102: state = 123; break; // &ang -> &angs
				case 106: state = 107; break; // &angm -> &angms
				case 152: state = 153; break; // &apo -> &apos
				case 180: state = 186; break; // &As -> &Ass
				case 222: state = 552; break; // &b -> &bs
				case 225: state = 242; break; // &back -> &backs
				case 231: state = 232; break; // &backep -> &backeps
				case 247: state = 549; break; // &B -> &Bs
				case 250: state = 251; break; // &Back -> &Backs
				case 253: state = 254; break; // &Backsla -> &Backslas
				case 291: state = 292; break; // &becau -> &becaus
				case 296: state = 297; break; // &Becau -> &Becaus
				case 305: state = 306; break; // &bep -> &beps
				case 318: state = 319; break; // &Bernoulli -> &Bernoullis
				case 334: state = 356; break; // &big -> &bigs
				case 349: state = 350; break; // &bigoplu -> &bigoplus
				case 354: state = 355; break; // &bigotime -> &bigotimes
				case 381: state = 382; break; // &biguplu -> &biguplus
				case 399: state = 407; break; // &black -> &blacks
				case 497: state = 498; break; // &boxminu -> &boxminus
				case 501: state = 502; break; // &boxplu -> &boxplus
				case 506: state = 507; break; // &boxtime -> &boxtimes
				case 564: state = 565; break; // &bsolh -> &bsolhs
				case 583: state = 972; break; // &C -> &Cs
				case 589: state = 975; break; // &c -> &cs
				case 596: state = 630; break; // &cap -> &caps
				case 639: state = 640; break; // &Cayley -> &Cayleys
				case 643: state = 644; break; // &ccap -> &ccaps
				case 673: state = 674; break; // &ccup -> &ccups
				case 674: state = 675; break; // &ccups -> &ccupss
				case 733: state = 799; break; // &cir -> &cirs
				case 754: state = 755; break; // &circleda -> &circledas
				case 762: state = 763; break; // &circledda -> &circleddas
				case 778: state = 779; break; // &CircleMinu -> &CircleMinus
				case 782: state = 783; break; // &CirclePlu -> &CirclePlus
				case 787: state = 788; break; // &CircleTime -> &CircleTimes
				case 804: state = 826; break; // &Clo -> &Clos
				case 808: state = 809; break; // &Clockwi -> &Clockwis
				case 851: state = 852; break; // &club -> &clubs
				case 881: state = 882; break; // &complexe -> &complexes
				case 929: state = 930; break; // &copy -> &copys
				case 943: state = 944; break; // &CounterClockwi -> &CounterClockwis
				case 966: state = 967; break; // &Cro -> &Cros
				case 967: state = 968; break; // &Cros -> &Cross
				case 969: state = 970; break; // &cro -> &cros
				case 970: state = 971; break; // &cros -> &cross
				case 994: state = 997; break; // &cue -> &cues
				case 1006: state = 1025; break; // &cup -> &cups
				case 1034: state = 1039; break; // &curlyeq -> &curlyeqs
				case 1091: state = 1604; break; // &D -> &Ds
				case 1092: state = 1116; break; // &Da -> &Das
				case 1097: state = 1607; break; // &d -> &ds
				case 1098: state = 1114; break; // &da -> &das
				case 1157: state = 1158; break; // &ddot -> &ddots
				case 1176: state = 1177; break; // &dfi -> &dfis
				case 1228: state = 1260; break; // &di -> &dis
				case 1230: state = 1242; break; // &diam -> &diams
				case 1237: state = 1238; break; // &diamond -> &diamonds
				case 1272: state = 1273; break; // &divideontime -> &divideontimes
				case 1302: state = 1325; break; // &dot -> &dots
				case 1319: state = 1320; break; // &dotminu -> &dotminus
				case 1323: state = 1324; break; // &dotplu -> &dotplus
				case 1508: state = 1509; break; // &downdownarrow -> &downdownarrows
				case 1656: state = 1897; break; // &E -> &Es
				case 1662: state = 1900; break; // &e -> &es
				case 1663: state = 1668; break; // &ea -> &eas
				case 1714: state = 1724; break; // &eg -> &egs
				case 1728: state = 1742; break; // &el -> &els
				case 1739: state = 1740; break; // &elinter -> &elinters
				case 1750: state = 1790; break; // &em -> &ems
				case 1756: state = 1757; break; // &empty -> &emptys
				case 1797: state = 1799; break; // &en -> &ens
				case 1813: state = 1821; break; // &ep -> &eps
				case 1815: state = 1816; break; // &epar -> &epars
				case 1819: state = 1820; break; // &eplu -> &eplus
				case 1823: state = 1824; break; // &Ep -> &Eps
				case 1833: state = 1842; break; // &eq -> &eqs
				case 1853: state = 1854; break; // &eqslantle -> &eqslantles
				case 1854: state = 1855; break; // &eqslantles -> &eqslantless
				case 1862: state = 1863; break; // &equal -> &equals
				case 1869: state = 1870; break; // &eque -> &eques
				case 1887: state = 1888; break; // &eqvpar -> &eqvpars
				case 1928: state = 1929; break; // &exi -> &exis
				case 1932: state = 1933; break; // &Exi -> &Exis
				case 1934: state = 1935; break; // &Exist -> &Exists
				case 1964: state = 2115; break; // &f -> &fs
				case 1973: state = 1974; break; // &fallingdot -> &fallingdots
				case 1977: state = 2112; break; // &F -> &Fs
				case 2047: state = 2048; break; // &fltn -> &fltns
				case 2084: state = 2107; break; // &fra -> &fras
				case 2118: state = 2285; break; // &g -> &gs
				case 2124: state = 2282; break; // &G -> &Gs
				case 2166: state = 2176; break; // &ge -> &ges
				case 2169: state = 2171; break; // &geq -> &geqs
				case 2185: state = 2186; break; // &gesle -> &gesles
				case 2208: state = 2219; break; // &gn -> &gns
				case 2244: state = 2245; break; // &GreaterEqualLe -> &GreaterEqualLes
				case 2245: state = 2246; break; // &GreaterEqualLes -> &GreaterEqualLess
				case 2264: state = 2265; break; // &GreaterLe -> &GreaterLes
				case 2265: state = 2266; break; // &GreaterLes -> &GreaterLess
				case 2308: state = 2309; break; // &gtque -> &gtques
				case 2311: state = 2338; break; // &gtr -> &gtrs
				case 2326: state = 2327; break; // &gtreqle -> &gtreqles
				case 2327: state = 2328; break; // &gtreqles -> &gtreqless
				case 2331: state = 2332; break; // &gtreqqle -> &gtreqqles
				case 2332: state = 2333; break; // &gtreqqles -> &gtreqqless
				case 2335: state = 2336; break; // &gtrle -> &gtrles
				case 2336: state = 2337; break; // &gtrles -> &gtrless
				case 2351: state = 2490; break; // &H -> &Hs
				case 2356: state = 2493; break; // &h -> &hs
				case 2359: state = 2360; break; // &hair -> &hairs
				case 2400: state = 2401; break; // &heart -> &hearts
				case 2428: state = 2429; break; // &hk -> &hks
				case 2497: state = 2498; break; // &hsla -> &hslas
				case 2533: state = 2740; break; // &I -> &Is
				case 2539: state = 2743; break; // &i -> &is
				case 2639: state = 2640; break; // &Implie -> &Implies
				case 2665: state = 2666; break; // &integer -> &integers
				case 2676: state = 2677; break; // &Inter -> &Inters
				case 2694: state = 2695; break; // &Invi -> &Invis
				case 2708: state = 2709; break; // &InvisibleTime -> &InvisibleTimes
				case 2737: state = 2738; break; // &ique -> &iques
				case 2747: state = 2752; break; // &isin -> &isins
				case 2777: state = 2803; break; // &J -> &Js
				case 2782: state = 2806; break; // &j -> &js
				case 2825: state = 2875; break; // &K -> &Ks
				case 2830: state = 2878; break; // &k -> &ks
				case 2881: state = 3661; break; // &l -> &ls
				case 2886: state = 3666; break; // &L -> &Ls
				case 2939: state = 2951; break; // &larr -> &larrs
				case 2941: state = 2942; break; // &larrbf -> &larrbfs
				case 2943: state = 2944; break; // &larrf -> &larrfs
				case 2964: state = 2965; break; // &late -> &lates
				case 2982: state = 2984; break; // &lbrk -> &lbrks
				case 3012: state = 3029; break; // &ld -> &lds
				case 3024: state = 3025; break; // &ldru -> &ldrus
				case 3032: state = 3291; break; // &le -> &les
				case 3033: state = 3321; break; // &Le -> &Les
				case 3147: state = 3148; break; // &leftleftarrow -> &leftleftarrows
				case 3173: state = 3188; break; // &leftright -> &leftrights
				case 3178: state = 3179; break; // &leftrightarrow -> &leftrightarrows
				case 3186: state = 3187; break; // &leftrightharpoon -> &leftrightharpoons
				case 3226: state = 3227; break; // &leftthreetime -> &leftthreetimes
				case 3284: state = 3286; break; // &leq -> &leqs
				case 3291: state = 3302; break; // &les -> &less
				case 3300: state = 3301; break; // &lesge -> &lesges
				case 3302: state = 3358; break; // &less -> &lesss
				case 3321: state = 3322; break; // &Les -> &Less
				case 3355: state = 3356; break; // &LessLe -> &LessLes
				case 3356: state = 3357; break; // &LessLes -> &LessLess
				case 3377: state = 3378; break; // &lfi -> &lfis
				case 3445: state = 3446; break; // &lmou -> &lmous
				case 3452: state = 3463; break; // &ln -> &lns
				case 3539: state = 3540; break; // &longmap -> &longmaps
				case 3596: state = 3597; break; // &loplu -> &loplus
				case 3601: state = 3602; break; // &lotime -> &lotimes
				case 3604: state = 3605; break; // &lowa -> &lowas
				case 3706: state = 3707; break; // &ltime -> &ltimes
				case 3714: state = 3715; break; // &ltque -> &ltques
				case 3726: state = 3727; break; // &lurd -> &lurds
				case 3745: state = 3879; break; // &m -> &ms
				case 3752: state = 3753; break; // &malte -> &maltes
				case 3755: state = 3876; break; // &M -> &Ms
				case 3758: state = 3759; break; // &map -> &maps
				case 3785: state = 3786; break; // &mda -> &mdas
				case 3793: state = 3794; break; // &mea -> &meas
				case 3832: state = 3833; break; // &mida -> &midas
				case 3842: state = 3843; break; // &minu -> &minus
				case 3849: state = 3850; break; // &Minu -> &Minus
				case 3853: state = 3854; break; // &MinusPlu -> &MinusPlus
				case 3863: state = 3864; break; // &mnplu -> &mnplus
				case 3868: state = 3869; break; // &model -> &models
				case 3884: state = 3885; break; // &mstpo -> &mstpos
				case 3897: state = 4653; break; // &n -> &ns
				case 3902: state = 4659; break; // &N -> &Ns
				case 3918: state = 3919; break; // &napo -> &napos
				case 3928: state = 3929; break; // &natural -> &naturals
				case 3930: state = 3931; break; // &nb -> &nbs
				case 3967: state = 3968; break; // &nda -> &ndas
				case 3970: state = 4035; break; // &ne -> &nes
				case 3984: state = 4041; break; // &Ne -> &Nes
				case 4060: state = 4061; break; // &NestedLe -> &NestedLes
				case 4061: state = 4062; break; // &NestedLes -> &NestedLess
				case 4064: state = 4065; break; // &NestedLessLe -> &NestedLessLes
				case 4065: state = 4066; break; // &NestedLessLes -> &NestedLessLess
				case 4073: state = 4074; break; // &nexi -> &nexis
				case 4075: state = 4076; break; // &nexist -> &nexists
				case 4081: state = 4094; break; // &ng -> &ngs
				case 4083: state = 4091; break; // &nge -> &nges
				case 4084: state = 4086; break; // &ngeq -> &ngeqs
				case 4111: state = 4112; break; // &ni -> &nis
				case 4121: state = 4178; break; // &nl -> &nls
				case 4131: state = 4175; break; // &nle -> &nles
				case 4168: state = 4170; break; // &nleq -> &nleqs
				case 4175: state = 4176; break; // &nles -> &nless
				case 4265: state = 4266; break; // &NotExi -> &NotExis
				case 4267: state = 4268; break; // &NotExist -> &NotExists
				case 4298: state = 4299; break; // &NotGreaterLe -> &NotGreaterLes
				case 4299: state = 4300; break; // &NotGreaterLes -> &NotGreaterLess
				case 4344: state = 4363; break; // &NotLe -> &NotLes
				case 4363: state = 4364; break; // &NotLes -> &NotLess
				case 4378: state = 4379; break; // &NotLessLe -> &NotLessLes
				case 4379: state = 4380; break; // &NotLessLes -> &NotLessLess
				case 4397: state = 4398; break; // &NotNe -> &NotNes
				case 4417: state = 4418; break; // &NotNestedLe -> &NotNestedLes
				case 4418: state = 4419; break; // &NotNestedLes -> &NotNestedLess
				case 4421: state = 4422; break; // &NotNestedLessLe -> &NotNestedLessLes
				case 4422: state = 4423; break; // &NotNestedLessLes -> &NotNestedLessLess
				case 4436: state = 4437; break; // &NotPrecede -> &NotPrecedes
				case 4457: state = 4458; break; // &NotRever -> &NotRevers
				case 4495: state = 4496; break; // &NotSquareSub -> &NotSquareSubs
				case 4506: state = 4507; break; // &NotSquareSuper -> &NotSquareSupers
				case 4516: state = 4517; break; // &NotSub -> &NotSubs
				case 4529: state = 4530; break; // &NotSucceed -> &NotSucceeds
				case 4553: state = 4554; break; // &NotSuper -> &NotSupers
				case 4599: state = 4605; break; // &npar -> &npars
				case 4688: state = 4689; break; // &nsq -> &nsqs
				case 4696: state = 4699; break; // &nsub -> &nsubs
				case 4709: state = 4712; break; // &nsup -> &nsups
				case 4754: state = 4758; break; // &num -> &nums
				case 4760: state = 4809; break; // &nv -> &nvs
				case 4765: state = 4766; break; // &nVDa -> &nVDas
				case 4769: state = 4770; break; // &nVda -> &nVdas
				case 4773: state = 4774; break; // &nvDa -> &nvDas
				case 4777: state = 4778; break; // &nvda -> &nvdas
				case 4827: state = 5015; break; // &O -> &Os
				case 4833: state = 5018; break; // &o -> &os
				case 4834: state = 4839; break; // &oa -> &oas
				case 4851: state = 4868; break; // &od -> &ods
				case 4852: state = 4853; break; // &oda -> &odas
				case 4916: state = 4917; break; // &olcro -> &olcros
				case 4917: state = 4918; break; // &olcros -> &olcross
				case 4949: state = 4950; break; // &ominu -> &ominus
				case 4988: state = 4989; break; // &oplu -> &oplus
				case 4991: state = 5008; break; // &or -> &ors
				case 5022: state = 5023; break; // &Osla -> &Oslas
				case 5026: state = 5027; break; // &osla -> &oslas
				case 5042: state = 5043; break; // &Otime -> &Otimes
				case 5045: state = 5046; break; // &otime -> &otimes
				case 5047: state = 5048; break; // &otimesa -> &otimesas
				case 5079: state = 5080; break; // &OverParenthe -> &OverParenthes
				case 5081: state = 5082; break; // &OverParenthesi -> &OverParenthesis
				case 5083: state = 5338; break; // &p -> &ps
				case 5085: state = 5091; break; // &par -> &pars
				case 5096: state = 5335; break; // &P -> &Ps
				case 5158: state = 5159; break; // &plu -> &plus
				case 5159: state = 5182; break; // &plus -> &pluss
				case 5173: state = 5174; break; // &Plu -> &Plus
				case 5178: state = 5179; break; // &PlusMinu -> &PlusMinus
				case 5216: state = 5328; break; // &pr -> &prs
				case 5224: state = 5279; break; // &prec -> &precs
				case 5242: state = 5243; break; // &Precede -> &Precedes
				case 5266: state = 5276; break; // &precn -> &precns
				case 5287: state = 5288; break; // &prime -> &primes
				case 5289: state = 5293; break; // &prn -> &prns
				case 5303: state = 5312; break; // &prof -> &profs
				case 5345: state = 5346; break; // &punc -> &puncs
				case 5348: state = 5368; break; // &Q -> &Qs
				case 5351: state = 5371; break; // &q -> &qs
				case 5382: state = 5383; break; // &quaternion -> &quaternions
				case 5387: state = 5388; break; // &que -> &ques
				case 5397: state = 5934; break; // &r -> &rs
				case 5405: state = 5939; break; // &R -> &Rs
				case 5439: state = 5454; break; // &rarr -> &rarrs
				case 5443: state = 5444; break; // &rarrbf -> &rarrbfs
				case 5446: state = 5447; break; // &rarrf -> &rarrfs
				case 5474: state = 5475; break; // &rational -> &rationals
				case 5496: state = 5498; break; // &rbrk -> &rbrks
				case 5526: state = 5538; break; // &rd -> &rds
				case 5543: state = 5551; break; // &real -> &reals
				case 5559: state = 5560; break; // &Rever -> &Revers
				case 5593: state = 5594; break; // &rfi -> &rfis
				case 5642: state = 5844; break; // &ri -> &ris
				case 5645: state = 5754; break; // &right -> &rights
				case 5733: state = 5734; break; // &rightleftarrow -> &rightleftarrows
				case 5741: state = 5742; break; // &rightleftharpoon -> &rightleftharpoons
				case 5752: state = 5753; break; // &rightrightarrow -> &rightrightarrows
				case 5786: state = 5787; break; // &rightthreetime -> &rightthreetimes
				case 5850: state = 5851; break; // &risingdot -> &risingdots
				case 5864: state = 5865; break; // &rmou -> &rmous
				case 5892: state = 5893; break; // &roplu -> &roplus
				case 5897: state = 5898; break; // &rotime -> &rotimes
				case 5907: state = 5908; break; // &RoundImplie -> &RoundImplies
				case 5958: state = 5959; break; // &rtime -> &rtimes
				case 5985: state = 6338; break; // &S -> &Ss
				case 5991: state = 6341; break; // &s -> &ss
				case 6002: state = 6043; break; // &sc -> &scs
				case 6030: state = 6034; break; // &scn -> &scns
				case 6053: state = 6068; break; // &se -> &ses
				case 6076: state = 6077; break; // &setminu -> &setminus
				case 6182: state = 6183; break; // &simplu -> &simplus
				case 6203: state = 6214; break; // &sma -> &smas
				case 6205: state = 6206; break; // &small -> &smalls
				case 6212: state = 6213; break; // &smallsetminu -> &smallsetminus
				case 6220: state = 6221; break; // &smepar -> &smepars
				case 6228: state = 6229; break; // &smte -> &smtes
				case 6252: state = 6253; break; // &spade -> &spades
				case 6258: state = 6269; break; // &sq -> &sqs
				case 6261: state = 6262; break; // &sqcap -> &sqcaps
				case 6264: state = 6265; break; // &sqcup -> &sqcups
				case 6271: state = 6273; break; // &sqsub -> &sqsubs
				case 6278: state = 6280; break; // &sqsup -> &sqsups
				case 6297: state = 6298; break; // &SquareInter -> &SquareInters
				case 6307: state = 6308; break; // &SquareSub -> &SquareSubs
				case 6318: state = 6319; break; // &SquareSuper -> &SquareSupers
				case 6370: state = 6371; break; // &straightep -> &straighteps
				case 6379: state = 6380; break; // &strn -> &strns
				case 6382: state = 6408; break; // &Sub -> &Subs
				case 6384: state = 6411; break; // &sub -> &subs
				case 6402: state = 6403; break; // &subplu -> &subplus
				case 6432: state = 6487; break; // &succ -> &succs
				case 6450: state = 6451; break; // &Succeed -> &Succeeds
				case 6474: state = 6484; break; // &succn -> &succns
				case 6499: state = 6546; break; // &Sup -> &Sups
				case 6500: state = 6549; break; // &sup -> &sups
				case 6504: state = 6507; break; // &supd -> &supds
				case 6516: state = 6517; break; // &Super -> &Supers
				case 6525: state = 6526; break; // &suph -> &suphs
				case 6544: state = 6545; break; // &supplu -> &supplus
				case 6583: state = 6821; break; // &T -> &Ts
				case 6586: state = 6824; break; // &t -> &ts
				case 6649: state = 6650; break; // &theta -> &thetas
				case 6656: state = 6663; break; // &thick -> &thicks
				case 6674: state = 6675; break; // &thin -> &thins
				case 6683: state = 6686; break; // &thk -> &thks
				case 6724: state = 6725; break; // &time -> &times
				case 6732: state = 6749; break; // &to -> &tos
				case 6764: state = 6809; break; // &tri -> &tris
				case 6795: state = 6796; break; // &triminu -> &triminus
				case 6807: state = 6808; break; // &triplu -> &triplus
				case 6873: state = 7186; break; // &U -> &Us
				case 6879: state = 7189; break; // &u -> &us
				case 6937: state = 6938; break; // &ufi -> &ufis
				case 7008: state = 7009; break; // &UnderParenthe -> &UnderParenthes
				case 7010: state = 7011; break; // &UnderParenthesi -> &UnderParenthesis
				case 7017: state = 7018; break; // &UnionPlu -> &UnionPlus
				case 7031: state = 7139; break; // &Up -> &Ups
				case 7042: state = 7141; break; // &up -> &ups
				case 7115: state = 7116; break; // &uplu -> &uplus
				case 7164: state = 7165; break; // &upuparrow -> &upuparrows
				case 7223: state = 7424; break; // &v -> &vs
				case 7229: state = 7264; break; // &var -> &vars
				case 7231: state = 7232; break; // &varep -> &vareps
				case 7270: state = 7271; break; // &varsub -> &varsubs
				case 7278: state = 7279; break; // &varsup -> &varsups
				case 7307: state = 7421; break; // &V -> &Vs
				case 7320: state = 7321; break; // &VDa -> &VDas
				case 7324: state = 7325; break; // &Vda -> &Vdas
				case 7328: state = 7329; break; // &vDa -> &vDas
				case 7332: state = 7333; break; // &vda -> &vdas
				case 7402: state = 7403; break; // &vn -> &vns
				case 7438: state = 7439; break; // &Vvda -> &Vvdas
				case 7447: state = 7489; break; // &W -> &Ws
				case 7452: state = 7492; break; // &w -> &ws
				case 7495: state = 7561; break; // &x -> &xs
				case 7508: state = 7558; break; // &X -> &Xs
				case 7533: state = 7534; break; // &xni -> &xnis
				case 7545: state = 7546; break; // &xoplu -> &xoplus
				case 7571: state = 7572; break; // &xuplu -> &xuplus
				case 7584: state = 7628; break; // &Y -> &Ys
				case 7590: state = 7631; break; // &y -> &ys
				case 7645: state = 7719; break; // &Z -> &Zs
				case 7651: state = 7722; break; // &z -> &zs
				default: return false;
				}
				break;
			case 't':
				switch (state) {
				case 0: state = 6586; break; // & -> &t
				case 1: state = 196; break; // &A -> &At
				case 4: state = 5; break; // &Aacu -> &Aacut
				case 7: state = 201; break; // &a -> &at
				case 10: state = 11; break; // &aacu -> &aacut
				case 33: state = 34; break; // &acu -> &acut
				case 118: state = 119; break; // &angr -> &angrt
				case 123: state = 126; break; // &angs -> &angst
				case 161: state = 162; break; // &ApplyFunc -> &ApplyFunct
				case 183: state = 190; break; // &as -> &ast
				case 217: state = 218; break; // &awconin -> &awconint
				case 220: state = 221; break; // &awin -> &awint
				case 272: state = 273; break; // &bbrk -> &bbrkt
				case 288: state = 322; break; // &be -> &bet
				case 293: state = 320; break; // &Be -> &Bet
				case 301: state = 302; break; // &bemp -> &bempt
				case 334: state = 364; break; // &big -> &bigt
				case 343: state = 351; break; // &bigo -> &bigot
				case 345: state = 346; break; // &bigodo -> &bigodot
				case 356: state = 361; break; // &bigs -> &bigst
				case 399: state = 413; break; // &black -> &blackt
				case 427: state = 428; break; // &blacktrianglelef -> &blacktriangleleft
				case 432: state = 433; break; // &blacktrianglerigh -> &blacktriangleright
				case 452: state = 453; break; // &bNo -> &bNot
				case 454: state = 455; break; // &bno -> &bnot
				case 459: state = 462; break; // &bo -> &bot
				case 462: state = 463; break; // &bot -> &bott
				case 466: state = 467; break; // &bow -> &bowt
				case 470: state = 503; break; // &box -> &boxt
				case 571: state = 572; break; // &bulle -> &bullet
				case 586: state = 587; break; // &Cacu -> &Cacut
				case 589: state = 983; break; // &c -> &ct
				case 592: state = 593; break; // &cacu -> &cacut
				case 611: state = 612; break; // &capdo -> &capdot
				case 613: state = 614; break; // &Capi -> &Capit
				case 624: state = 625; break; // &CapitalDifferen -> &CapitalDifferent
				case 632: state = 633; break; // &care -> &caret
				case 670: state = 671; break; // &Cconin -> &Cconint
				case 678: state = 679; break; // &Cdo -> &Cdot
				case 681: state = 682; break; // &cdo -> &cdot
				case 694: state = 695; break; // &cemp -> &cempt
				case 698: state = 699; break; // &cen -> &cent
				case 700: state = 701; break; // &Cen -> &Cent
				case 705: state = 706; break; // &CenterDo -> &CenterDot
				case 710: state = 711; break; // &centerdo -> &centerdot
				case 746: state = 747; break; // &circlearrowlef -> &circlearrowleft
				case 751: state = 752; break; // &circlearrowrigh -> &circlearrowright
				case 755: state = 756; break; // &circledas -> &circledast
				case 771: state = 772; break; // &CircleDo -> &CircleDot
				case 794: state = 795; break; // &cirfnin -> &cirfnint
				case 813: state = 814; break; // &ClockwiseCon -> &ClockwiseCont
				case 819: state = 820; break; // &ClockwiseContourIn -> &ClockwiseContourInt
				case 841: state = 842; break; // &CloseCurlyDoubleQuo -> &CloseCurlyDoubleQuot
				case 846: state = 847; break; // &CloseCurlyQuo -> &CloseCurlyQuot
				case 854: state = 855; break; // &clubsui -> &clubsuit
				case 869: state = 870; break; // &comma -> &commat
				case 878: state = 879; break; // &complemen -> &complement
				case 886: state = 887; break; // &congdo -> &congdot
				case 888: state = 901; break; // &Con -> &Cont
				case 893: state = 894; break; // &Congruen -> &Congruent
				case 896: state = 897; break; // &Conin -> &Conint
				case 899: state = 900; break; // &conin -> &conint
				case 906: state = 907; break; // &ContourIn -> &ContourInt
				case 924: state = 925; break; // &Coproduc -> &Coproduct
				case 933: state = 934; break; // &Coun -> &Count
				case 948: state = 949; break; // &CounterClockwiseCon -> &CounterClockwiseCont
				case 954: state = 955; break; // &CounterClockwiseContourIn -> &CounterClockwiseContourInt
				case 985: state = 986; break; // &ctdo -> &ctdot
				case 1021: state = 1022; break; // &cupdo -> &cupdot
				case 1063: state = 1064; break; // &curvearrowlef -> &curvearrowleft
				case 1068: state = 1069; break; // &curvearrowrigh -> &curvearrowright
				case 1081: state = 1082; break; // &cwconin -> &cwconint
				case 1084: state = 1085; break; // &cwin -> &cwint
				case 1088: state = 1089; break; // &cylc -> &cylct
				case 1097: state = 1624; break; // &d -> &dt
				case 1104: state = 1105; break; // &dale -> &dalet
				case 1150: state = 1151; break; // &DDo -> &DDot
				case 1156: state = 1157; break; // &ddo -> &ddot
				case 1164: state = 1165; break; // &Del -> &Delt
				case 1167: state = 1168; break; // &del -> &delt
				case 1171: state = 1172; break; // &demp -> &dempt
				case 1178: state = 1179; break; // &dfish -> &dfisht
				case 1195: state = 1196; break; // &Diacri -> &Diacrit
				case 1203: state = 1204; break; // &DiacriticalAcu -> &DiacriticalAcut
				case 1207: state = 1208; break; // &DiacriticalDo -> &DiacriticalDot
				case 1215: state = 1216; break; // &DiacriticalDoubleAcu -> &DiacriticalDoubleAcut
				case 1240: state = 1241; break; // &diamondsui -> &diamondsuit
				case 1249: state = 1250; break; // &Differen -> &Different
				case 1268: state = 1269; break; // &divideon -> &divideont
				case 1291: state = 1302; break; // &do -> &dot
				case 1296: state = 1301; break; // &Do -> &Dot
				case 1304: state = 1305; break; // &DotDo -> &DotDot
				case 1309: state = 1310; break; // &doteqdo -> &doteqdot
				case 1349: state = 1350; break; // &DoubleCon -> &DoubleCont
				case 1355: state = 1356; break; // &DoubleContourIn -> &DoubleContourInt
				case 1363: state = 1364; break; // &DoubleDo -> &DoubleDot
				case 1374: state = 1375; break; // &DoubleLef -> &DoubleLeft
				case 1384: state = 1385; break; // &DoubleLeftRigh -> &DoubleLeftRight
				case 1399: state = 1400; break; // &DoubleLongLef -> &DoubleLongLeft
				case 1409: state = 1410; break; // &DoubleLongLeftRigh -> &DoubleLongLeftRight
				case 1419: state = 1420; break; // &DoubleLongRigh -> &DoubleLongRight
				case 1429: state = 1430; break; // &DoubleRigh -> &DoubleRight
				case 1457: state = 1458; break; // &DoubleVer -> &DoubleVert
				case 1519: state = 1520; break; // &downharpoonlef -> &downharpoonleft
				case 1524: state = 1525; break; // &downharpoonrigh -> &downharpoonright
				case 1528: state = 1529; break; // &DownLef -> &DownLeft
				case 1533: state = 1534; break; // &DownLeftRigh -> &DownLeftRight
				case 1537: state = 1538; break; // &DownLeftRightVec -> &DownLeftRightVect
				case 1546: state = 1547; break; // &DownLeftTeeVec -> &DownLeftTeeVect
				case 1552: state = 1553; break; // &DownLeftVec -> &DownLeftVect
				case 1562: state = 1563; break; // &DownRigh -> &DownRight
				case 1569: state = 1570; break; // &DownRightTeeVec -> &DownRightTeeVect
				case 1575: state = 1576; break; // &DownRightVec -> &DownRightVect
				case 1604: state = 1616; break; // &Ds -> &Dst
				case 1607: state = 1620; break; // &ds -> &dst
				case 1626: state = 1627; break; // &dtdo -> &dtdot
				case 1656: state = 1910; break; // &E -> &Et
				case 1659: state = 1660; break; // &Eacu -> &Eacut
				case 1662: state = 1912; break; // &e -> &et
				case 1665: state = 1666; break; // &eacu -> &eacut
				case 1668: state = 1669; break; // &eas -> &east
				case 1696: state = 1697; break; // &eDDo -> &eDDot
				case 1699: state = 1700; break; // &Edo -> &Edot
				case 1701: state = 1702; break; // &eDo -> &eDot
				case 1704: state = 1705; break; // &edo -> &edot
				case 1709: state = 1710; break; // &efDo -> &efDot
				case 1726: state = 1727; break; // &egsdo -> &egsdot
				case 1733: state = 1734; break; // &Elemen -> &Element
				case 1736: state = 1737; break; // &elin -> &elint
				case 1744: state = 1745; break; // &elsdo -> &elsdot
				case 1754: state = 1755; break; // &emp -> &empt
				case 1758: state = 1759; break; // &emptyse -> &emptyset
				case 1760: state = 1761; break; // &Emp -> &Empt
				case 1847: state = 1848; break; // &eqslan -> &eqslant
				case 1849: state = 1850; break; // &eqslantg -> &eqslantgt
				case 1870: state = 1871; break; // &eques -> &equest
				case 1895: state = 1896; break; // &erDo -> &erDot
				case 1904: state = 1905; break; // &esdo -> &esdot
				case 1929: state = 1930; break; // &exis -> &exist
				case 1933: state = 1934; break; // &Exis -> &Exist
				case 1938: state = 1939; break; // &expec -> &expect
				case 1940: state = 1941; break; // &expecta -> &expectat
				case 1949: state = 1950; break; // &Exponen -> &Exponent
				case 1958: state = 1959; break; // &exponen -> &exponent
				case 1972: state = 1973; break; // &fallingdo -> &fallingdot
				case 2040: state = 2046; break; // &fl -> &flt
				case 2041: state = 2042; break; // &fla -> &flat
				case 2072: state = 2073; break; // &Fourier -> &Fouriert
				case 2078: state = 2079; break; // &fpar -> &fpart
				case 2081: state = 2082; break; // &fpartin -> &fpartint
				case 2118: state = 2294; break; // &g -> &gt
				case 2121: state = 2122; break; // &gacu -> &gacut
				case 2124: state = 2293; break; // &G -> &Gt
				case 2160: state = 2161; break; // &Gdo -> &Gdot
				case 2163: state = 2164; break; // &gdo -> &gdot
				case 2174: state = 2175; break; // &geqslan -> &geqslant
				case 2180: state = 2181; break; // &gesdo -> &gesdot
				case 2234: state = 2235; break; // &Grea -> &Great
				case 2259: state = 2260; break; // &GreaterGrea -> &GreaterGreat
				case 2270: state = 2271; break; // &GreaterSlan -> &GreaterSlant
				case 2300: state = 2301; break; // &gtdo -> &gtdot
				case 2309: state = 2310; break; // &gtques -> &gtquest
				case 2321: state = 2322; break; // &gtrdo -> &gtrdot
				case 2343: state = 2344; break; // &gver -> &gvert
				case 2352: state = 2385; break; // &Ha -> &Hat
				case 2366: state = 2367; break; // &hamil -> &hamilt
				case 2399: state = 2400; break; // &hear -> &heart
				case 2403: state = 2404; break; // &heartsui -> &heartsuit
				case 2421: state = 2422; break; // &Hilber -> &Hilbert
				case 2444: state = 2445; break; // &hom -> &homt
				case 2446: state = 2447; break; // &homth -> &homtht
				case 2452: state = 2453; break; // &hooklef -> &hookleft
				case 2462: state = 2463; break; // &hookrigh -> &hookright
				case 2482: state = 2483; break; // &Horizon -> &Horizont
				case 2490: state = 2500; break; // &Hs -> &Hst
				case 2493: state = 2504; break; // &hs -> &hst
				case 2533: state = 2756; break; // &I -> &It
				case 2536: state = 2537; break; // &Iacu -> &Iacut
				case 2539: state = 2755; break; // &i -> &it
				case 2542: state = 2543; break; // &iacu -> &iacut
				case 2556: state = 2557; break; // &Ido -> &Idot
				case 2585: state = 2586; break; // &iiiin -> &iiiint
				case 2587: state = 2588; break; // &iiin -> &iiint
				case 2593: state = 2594; break; // &iio -> &iiot
				case 2609: state = 2629; break; // &ima -> &imat
				case 2627: state = 2628; break; // &imagpar -> &imagpart
				case 2641: state = 2658; break; // &in -> &int
				case 2648: state = 2649; break; // &infin -> &infint
				case 2654: state = 2655; break; // &inodo -> &inodot
				case 2656: state = 2657; break; // &In -> &Int
				case 2679: state = 2680; break; // &Intersec -> &Intersect
				case 2713: state = 2729; break; // &io -> &iot
				case 2716: state = 2727; break; // &Io -> &Iot
				case 2738: state = 2739; break; // &iques -> &iquest
				case 2749: state = 2750; break; // &isindo -> &isindot
				case 2794: state = 2795; break; // &jma -> &jmat
				case 2881: state = 3692; break; // &l -> &lt
				case 2882: state = 2957; break; // &lA -> &lAt
				case 2886: state = 3691; break; // &L -> &Lt
				case 2889: state = 2890; break; // &Lacu -> &Lacut
				case 2892: state = 2956; break; // &la -> &lat
				case 2894: state = 2895; break; // &lacu -> &lacut
				case 2899: state = 2900; break; // &laemp -> &laempt
				case 2927: state = 2928; break; // &Laplace -> &Laplacet
				case 2939: state = 2954; break; // &larr -> &larrt
				case 3034: state = 3035; break; // &Lef -> &Left
				case 3046: state = 3047; break; // &LeftAngleBracke -> &LeftAngleBracket
				case 3057: state = 3058; break; // &lef -> &left
				case 3058: state = 3218; break; // &left -> &leftt
				case 3063: state = 3077; break; // &leftarrow -> &leftarrowt
				case 3070: state = 3071; break; // &LeftArrowRigh -> &LeftArrowRight
				case 3099: state = 3100; break; // &LeftDoubleBracke -> &LeftDoubleBracket
				case 3108: state = 3109; break; // &LeftDownTeeVec -> &LeftDownTeeVect
				case 3114: state = 3115; break; // &LeftDownVec -> &LeftDownVect
				case 3141: state = 3142; break; // &leftlef -> &leftleft
				case 3152: state = 3153; break; // &LeftRigh -> &LeftRight
				case 3162: state = 3163; break; // &Leftrigh -> &Leftright
				case 3172: state = 3173; break; // &leftrigh -> &leftright
				case 3200: state = 3201; break; // &LeftRightVec -> &LeftRightVect
				case 3214: state = 3215; break; // &LeftTeeVec -> &LeftTeeVect
				case 3222: state = 3223; break; // &leftthree -> &leftthreet
				case 3251: state = 3252; break; // &LeftUpDownVec -> &LeftUpDownVect
				case 3260: state = 3261; break; // &LeftUpTeeVec -> &LeftUpTeeVect
				case 3266: state = 3267; break; // &LeftUpVec -> &LeftUpVect
				case 3275: state = 3276; break; // &LeftVec -> &LeftVect
				case 3289: state = 3290; break; // &leqslan -> &leqslant
				case 3295: state = 3296; break; // &lesdo -> &lesdot
				case 3310: state = 3311; break; // &lessdo -> &lessdot
				case 3314: state = 3315; break; // &lesseqg -> &lesseqgt
				case 3318: state = 3319; break; // &lesseqqg -> &lesseqqgt
				case 3331: state = 3332; break; // &LessEqualGrea -> &LessEqualGreat
				case 3347: state = 3348; break; // &LessGrea -> &LessGreat
				case 3351: state = 3352; break; // &lessg -> &lessgt
				case 3364: state = 3365; break; // &LessSlan -> &LessSlant
				case 3379: state = 3380; break; // &lfish -> &lfisht
				case 3409: state = 3431; break; // &ll -> &llt
				case 3420: state = 3421; break; // &Llef -> &Lleft
				case 3437: state = 3438; break; // &Lmido -> &Lmidot
				case 3442: state = 3443; break; // &lmido -> &lmidot
				case 3446: state = 3447; break; // &lmous -> &lmoust
				case 3466: state = 3598; break; // &lo -> &lot
				case 3480: state = 3481; break; // &LongLef -> &LongLeft
				case 3489: state = 3490; break; // &Longlef -> &Longleft
				case 3500: state = 3501; break; // &longlef -> &longleft
				case 3510: state = 3511; break; // &LongLeftRigh -> &LongLeftRight
				case 3520: state = 3521; break; // &Longleftrigh -> &Longleftright
				case 3530: state = 3531; break; // &longleftrigh -> &longleftright
				case 3540: state = 3541; break; // &longmaps -> &longmapst
				case 3546: state = 3547; break; // &LongRigh -> &LongRight
				case 3556: state = 3557; break; // &Longrigh -> &Longright
				case 3566: state = 3567; break; // &longrigh -> &longright
				case 3582: state = 3583; break; // &looparrowlef -> &looparrowleft
				case 3587: state = 3588; break; // &looparrowrigh -> &looparrowright
				case 3605: state = 3606; break; // &lowas -> &lowast
				case 3615: state = 3616; break; // &LowerLef -> &LowerLeft
				case 3625: state = 3626; break; // &LowerRigh -> &LowerRight
				case 3641: state = 3642; break; // &lparl -> &lparlt
				case 3643: state = 3658; break; // &lr -> &lrt
				case 3661: state = 3686; break; // &ls -> &lst
				case 3666: state = 3682; break; // &Ls -> &Lst
				case 3698: state = 3699; break; // &ltdo -> &ltdot
				case 3715: state = 3716; break; // &ltques -> &ltquest
				case 3737: state = 3738; break; // &lver -> &lvert
				case 3749: state = 3751; break; // &mal -> &malt
				case 3759: state = 3760; break; // &maps -> &mapst
				case 3768: state = 3769; break; // &mapstolef -> &mapstoleft
				case 3790: state = 3791; break; // &mDDo -> &mDDot
				case 3817: state = 3818; break; // &Mellin -> &Mellint
				case 3833: state = 3834; break; // &midas -> &midast
				case 3839: state = 3840; break; // &middo -> &middot
				case 3879: state = 3882; break; // &ms -> &mst
				case 3888: state = 3889; break; // &mul -> &mult
				case 3897: state = 4718; break; // &n -> &nt
				case 3898: state = 3924; break; // &na -> &nat
				case 3902: state = 4721; break; // &N -> &Nt
				case 3905: state = 3906; break; // &Nacu -> &Nacut
				case 3909: state = 3910; break; // &nacu -> &nacut
				case 3960: state = 3961; break; // &ncongdo -> &ncongdot
				case 3982: state = 3983; break; // &nedo -> &nedot
				case 3986: state = 3987; break; // &Nega -> &Negat
				case 4041: state = 4042; break; // &Nes -> &Nest
				case 4048: state = 4049; break; // &NestedGrea -> &NestedGreat
				case 4055: state = 4056; break; // &NestedGreaterGrea -> &NestedGreaterGreat
				case 4074: state = 4075; break; // &nexis -> &nexist
				case 4081: state = 4098; break; // &ng -> &ngt
				case 4089: state = 4090; break; // &ngeqslan -> &ngeqslant
				case 4092: state = 4097; break; // &nG -> &nGt
				case 4121: state = 4182; break; // &nl -> &nlt
				case 4132: state = 4181; break; // &nL -> &nLt
				case 4134: state = 4135; break; // &nLef -> &nLeft
				case 4141: state = 4142; break; // &nlef -> &nleft
				case 4151: state = 4152; break; // &nLeftrigh -> &nLeftright
				case 4161: state = 4162; break; // &nleftrigh -> &nleftright
				case 4173: state = 4174; break; // &nleqslan -> &nleqslant
				case 4190: state = 4215; break; // &No -> &Not
				case 4212: state = 4216; break; // &no -> &not
				case 4224: state = 4225; break; // &NotCongruen -> &NotCongruent
				case 4239: state = 4240; break; // &NotDoubleVer -> &NotDoubleVert
				case 4253: state = 4254; break; // &NotElemen -> &NotElement
				case 4266: state = 4267; break; // &NotExis -> &NotExist
				case 4272: state = 4273; break; // &NotGrea -> &NotGreat
				case 4293: state = 4294; break; // &NotGreaterGrea -> &NotGreaterGreat
				case 4304: state = 4305; break; // &NotGreaterSlan -> &NotGreaterSlant
				case 4336: state = 4337; break; // &notindo -> &notindot
				case 4345: state = 4346; break; // &NotLef -> &NotLeft
				case 4373: state = 4374; break; // &NotLessGrea -> &NotLessGreat
				case 4384: state = 4385; break; // &NotLessSlan -> &NotLessSlant
				case 4398: state = 4399; break; // &NotNes -> &NotNest
				case 4405: state = 4406; break; // &NotNestedGrea -> &NotNestedGreat
				case 4412: state = 4413; break; // &NotNestedGreaterGrea -> &NotNestedGreaterGreat
				case 4446: state = 4447; break; // &NotPrecedesSlan -> &NotPrecedesSlant
				case 4465: state = 4466; break; // &NotReverseElemen -> &NotReverseElement
				case 4469: state = 4470; break; // &NotRigh -> &NotRight
				case 4497: state = 4498; break; // &NotSquareSubse -> &NotSquareSubset
				case 4508: state = 4509; break; // &NotSquareSuperse -> &NotSquareSuperset
				case 4518: state = 4519; break; // &NotSubse -> &NotSubset
				case 4539: state = 4540; break; // &NotSucceedsSlan -> &NotSucceedsSlant
				case 4555: state = 4556; break; // &NotSuperse -> &NotSuperset
				case 4588: state = 4589; break; // &NotVer -> &NotVert
				case 4599: state = 4607; break; // &npar -> &npart
				case 4611: state = 4612; break; // &npolin -> &npolint
				case 4621: state = 4649; break; // &nr -> &nrt
				case 4633: state = 4634; break; // &nRigh -> &nRight
				case 4642: state = 4643; break; // &nrigh -> &nright
				case 4665: state = 4666; break; // &nshor -> &nshort
				case 4700: state = 4701; break; // &nsubse -> &nsubset
				case 4713: state = 4714; break; // &nsupse -> &nsupset
				case 4741: state = 4742; break; // &ntrianglelef -> &ntriangleleft
				case 4748: state = 4749; break; // &ntrianglerigh -> &ntriangleright
				case 4780: state = 4782; break; // &nvg -> &nvgt
				case 4792: state = 4797; break; // &nvl -> &nvlt
				case 4801: state = 4805; break; // &nvr -> &nvrt
				case 4827: state = 5031; break; // &O -> &Ot
				case 4830: state = 4831; break; // &Oacu -> &Oacut
				case 4833: state = 5036; break; // &o -> &ot
				case 4836: state = 4837; break; // &oacu -> &oacut
				case 4839: state = 4840; break; // &oas -> &oast
				case 4866: state = 4867; break; // &odo -> &odot
				case 4887: state = 4899; break; // &og -> &ogt
				case 4906: state = 4907; break; // &oin -> &oint
				case 4908: state = 4922; break; // &ol -> &olt
				case 4976: state = 4977; break; // &OpenCurlyDoubleQuo -> &OpenCurlyDoubleQuot
				case 4981: state = 4982; break; // &OpenCurlyQuo -> &OpenCurlyQuot
				case 5070: state = 5071; break; // &OverBracke -> &OverBracket
				case 5076: state = 5077; break; // &OverParen -> &OverParent
				case 5085: state = 5095; break; // &par -> &part
				case 5098: state = 5099; break; // &Par -> &Part
				case 5109: state = 5120; break; // &per -> &pert
				case 5111: state = 5112; break; // &percn -> &percnt
				case 5135: state = 5136; break; // &phmma -> &phmmat
				case 5141: state = 5142; break; // &pi -> &pit
				case 5159: state = 5185; break; // &plus -> &plust
				case 5203: state = 5204; break; // &poin -> &point
				case 5206: state = 5207; break; // &pointin -> &pointint
				case 5252: state = 5253; break; // &PrecedesSlan -> &PrecedesSlant
				case 5301: state = 5302; break; // &Produc -> &Product
				case 5316: state = 5326; break; // &prop -> &propt
				case 5319: state = 5320; break; // &Propor -> &Proport
				case 5355: state = 5356; break; // &qin -> &qint
				case 5375: state = 5376; break; // &qua -> &quat
				case 5385: state = 5386; break; // &quatin -> &quatint
				case 5388: state = 5389; break; // &ques -> &quest
				case 5395: state = 5396; break; // &quo -> &quot
				case 5397: state = 5951; break; // &r -> &rt
				case 5398: state = 5462; break; // &rA -> &rAt
				case 5402: state = 5466; break; // &ra -> &rat
				case 5408: state = 5409; break; // &Racu -> &Racut
				case 5411: state = 5412; break; // &racu -> &racut
				case 5419: state = 5420; break; // &raemp -> &raempt
				case 5435: state = 5457; break; // &Rarr -> &Rarrt
				case 5439: state = 5459; break; // &rarr -> &rarrt
				case 5549: state = 5550; break; // &realpar -> &realpart
				case 5552: state = 5553; break; // &rec -> &rect
				case 5567: state = 5568; break; // &ReverseElemen -> &ReverseElement
				case 5595: state = 5596; break; // &rfish -> &rfisht
				case 5619: state = 5620; break; // &Righ -> &Right
				case 5631: state = 5632; break; // &RightAngleBracke -> &RightAngleBracket
				case 5644: state = 5645; break; // &righ -> &right
				case 5645: state = 5778; break; // &right -> &rightt
				case 5650: state = 5663; break; // &rightarrow -> &rightarrowt
				case 5656: state = 5657; break; // &RightArrowLef -> &RightArrowLeft
				case 5685: state = 5686; break; // &RightDoubleBracke -> &RightDoubleBracket
				case 5694: state = 5695; break; // &RightDownTeeVec -> &RightDownTeeVect
				case 5700: state = 5701; break; // &RightDownVec -> &RightDownVect
				case 5727: state = 5728; break; // &rightlef -> &rightleft
				case 5746: state = 5747; break; // &rightrigh -> &rightright
				case 5774: state = 5775; break; // &RightTeeVec -> &RightTeeVect
				case 5782: state = 5783; break; // &rightthree -> &rightthreet
				case 5811: state = 5812; break; // &RightUpDownVec -> &RightUpDownVect
				case 5820: state = 5821; break; // &RightUpTeeVec -> &RightUpTeeVect
				case 5826: state = 5827; break; // &RightUpVec -> &RightUpVect
				case 5835: state = 5836; break; // &RightVec -> &RightVect
				case 5849: state = 5850; break; // &risingdo -> &risingdot
				case 5865: state = 5866; break; // &rmous -> &rmoust
				case 5875: state = 5894; break; // &ro -> &rot
				case 5912: state = 5913; break; // &rparg -> &rpargt
				case 5918: state = 5919; break; // &rppolin -> &rppolint
				case 5927: state = 5928; break; // &Rrigh -> &Rright
				case 5964: state = 5965; break; // &rtril -> &rtrilt
				case 5985: state = 6356; break; // &S -> &St
				case 5988: state = 5989; break; // &Sacu -> &Sacut
				case 5991: state = 6359; break; // &s -> &st
				case 5994: state = 5995; break; // &sacu -> &sacut
				case 6041: state = 6042; break; // &scpolin -> &scpolint
				case 6049: state = 6050; break; // &sdo -> &sdot
				case 6053: state = 6072; break; // &se -> &set
				case 6064: state = 6065; break; // &sec -> &sect
				case 6079: state = 6080; break; // &sex -> &sext
				case 6106: state = 6107; break; // &Shor -> &Short
				case 6119: state = 6120; break; // &ShortLef -> &ShortLeft
				case 6127: state = 6128; break; // &shor -> &short
				case 6143: state = 6144; break; // &ShortRigh -> &ShortRight
				case 6170: state = 6171; break; // &simdo -> &simdot
				case 6202: state = 6227; break; // &sm -> &smt
				case 6207: state = 6208; break; // &smallse -> &smallset
				case 6236: state = 6237; break; // &sof -> &soft
				case 6255: state = 6256; break; // &spadesui -> &spadesuit
				case 6267: state = 6268; break; // &Sqr -> &Sqrt
				case 6274: state = 6275; break; // &sqsubse -> &sqsubset
				case 6281: state = 6282; break; // &sqsupse -> &sqsupset
				case 6294: state = 6295; break; // &SquareIn -> &SquareInt
				case 6300: state = 6301; break; // &SquareIntersec -> &SquareIntersect
				case 6309: state = 6310; break; // &SquareSubse -> &SquareSubset
				case 6320: state = 6321; break; // &SquareSuperse -> &SquareSuperset
				case 6341: state = 6352; break; // &ss -> &sst
				case 6344: state = 6345; break; // &sse -> &sset
				case 6367: state = 6368; break; // &straigh -> &straight
				case 6386: state = 6387; break; // &subdo -> &subdot
				case 6391: state = 6392; break; // &subedo -> &subedot
				case 6395: state = 6396; break; // &submul -> &submult
				case 6409: state = 6410; break; // &Subse -> &Subset
				case 6412: state = 6413; break; // &subse -> &subset
				case 6460: state = 6461; break; // &SucceedsSlan -> &SucceedsSlant
				case 6493: state = 6494; break; // &SuchTha -> &SuchThat
				case 6505: state = 6506; break; // &supdo -> &supdot
				case 6513: state = 6514; break; // &supedo -> &supedot
				case 6518: state = 6519; break; // &Superse -> &Superset
				case 6537: state = 6538; break; // &supmul -> &supmult
				case 6547: state = 6548; break; // &Supse -> &Supset
				case 6550: state = 6551; break; // &supse -> &supset
				case 6590: state = 6591; break; // &targe -> &target
				case 6618: state = 6619; break; // &tdo -> &tdot
				case 6630: state = 6648; break; // &the -> &thet
				case 6635: state = 6646; break; // &The -> &Thet
				case 6730: state = 6731; break; // &tin -> &tint
				case 6737: state = 6738; break; // &topbo -> &topbot
				case 6764: state = 6811; break; // &tri -> &trit
				case 6776: state = 6777; break; // &trianglelef -> &triangleleft
				case 6784: state = 6785; break; // &trianglerigh -> &triangleright
				case 6789: state = 6790; break; // &trido -> &tridot
				case 6803: state = 6804; break; // &TripleDo -> &TripleDot
				case 6821: state = 6837; break; // &Ts -> &Tst
				case 6824: state = 6841; break; // &ts -> &tst
				case 6847: state = 6848; break; // &twix -> &twixt
				case 6856: state = 6857; break; // &twoheadlef -> &twoheadleft
				case 6866: state = 6867; break; // &twoheadrigh -> &twoheadright
				case 6873: state = 7196; break; // &U -> &Ut
				case 6876: state = 6877; break; // &Uacu -> &Uacut
				case 6879: state = 7192; break; // &u -> &ut
				case 6882: state = 6883; break; // &uacu -> &uacut
				case 6939: state = 6940; break; // &ufish -> &ufisht
				case 6965: state = 6975; break; // &ul -> &ult
				case 6999: state = 7000; break; // &UnderBracke -> &UnderBracket
				case 7005: state = 7006; break; // &UnderParen -> &UnderParent
				case 7107: state = 7108; break; // &upharpoonlef -> &upharpoonleft
				case 7112: state = 7113; break; // &upharpoonrigh -> &upharpoonright
				case 7122: state = 7123; break; // &UpperLef -> &UpperLeft
				case 7132: state = 7133; break; // &UpperRigh -> &UpperRight
				case 7166: state = 7183; break; // &ur -> &urt
				case 7194: state = 7195; break; // &utdo -> &utdot
				case 7227: state = 7228; break; // &vangr -> &vangrt
				case 7229: state = 7286; break; // &var -> &vart
				case 7243: state = 7244; break; // &varno -> &varnot
				case 7255: state = 7256; break; // &varprop -> &varpropt
				case 7272: state = 7273; break; // &varsubse -> &varsubset
				case 7280: state = 7281; break; // &varsupse -> &varsupset
				case 7288: state = 7289; break; // &varthe -> &varthet
				case 7300: state = 7301; break; // &vartrianglelef -> &vartriangleleft
				case 7305: state = 7306; break; // &vartrianglerigh -> &vartriangleright
				case 7349: state = 7357; break; // &Ver -> &Vert
				case 7353: state = 7358; break; // &ver -> &vert
				case 7375: state = 7376; break; // &VerticalSepara -> &VerticalSeparat
				case 7398: state = 7399; break; // &vl -> &vlt
				case 7417: state = 7418; break; // &vr -> &vrt
				case 7486: state = 7487; break; // &wrea -> &wreat
				case 7504: state = 7505; break; // &xd -> &xdt
				case 7535: state = 7547; break; // &xo -> &xot
				case 7537: state = 7538; break; // &xodo -> &xodot
				case 7568: state = 7573; break; // &xu -> &xut
				case 7587: state = 7588; break; // &Yacu -> &Yacut
				case 7593: state = 7594; break; // &yacu -> &yacut
				case 7648: state = 7649; break; // &Zacu -> &Zacut
				case 7654: state = 7655; break; // &zacu -> &zacut
				case 7670: state = 7671; break; // &Zdo -> &Zdot
				case 7673: state = 7674; break; // &zdo -> &zdot
				case 7675: state = 7695; break; // &ze -> &zet
				case 7676: state = 7677; break; // &zee -> &zeet
				case 7680: state = 7693; break; // &Ze -> &Zet
				case 7685: state = 7686; break; // &ZeroWid -> &ZeroWidt
				default: return false;
				}
				break;
			case 'u':
				switch (state) {
				case 0: state = 6879; break; // & -> &u
				case 1: state = 206; break; // &A -> &Au
				case 3: state = 4; break; // &Aac -> &Aacu
				case 7: state = 209; break; // &a -> &au
				case 9: state = 10; break; // &aac -> &aacu
				case 23: state = 33; break; // &ac -> &acu
				case 158: state = 159; break; // &ApplyF -> &ApplyFu
				case 222: state = 568; break; // &b -> &bu
				case 247: state = 577; break; // &B -> &Bu
				case 285: state = 286; break; // &bdq -> &bdqu
				case 290: state = 291; break; // &beca -> &becau
				case 295: state = 296; break; // &Beca -> &Becau
				case 310: state = 311; break; // &berno -> &bernou
				case 314: state = 315; break; // &Berno -> &Bernou
				case 334: state = 378; break; // &big -> &bigu
				case 335: state = 341; break; // &bigc -> &bigcu
				case 348: state = 349; break; // &bigopl -> &bigoplu
				case 358: state = 359; break; // &bigsqc -> &bigsqcu
				case 371: state = 376; break; // &bigtriangle -> &bigtriangleu
				case 380: state = 381; break; // &bigupl -> &biguplu
				case 408: state = 409; break; // &blacksq -> &blacksqu
				case 447: state = 448; break; // &bneq -> &bnequ
				case 470: state = 511; break; // &box -> &boxu
				case 484: state = 491; break; // &boxH -> &boxHu
				case 485: state = 493; break; // &boxh -> &boxhu
				case 496: state = 497; break; // &boxmin -> &boxminu
				case 500: state = 501; break; // &boxpl -> &boxplu
				case 565: state = 566; break; // &bsolhs -> &bsolhsu
				case 583: state = 1004; break; // &C -> &Cu
				case 585: state = 586; break; // &Cac -> &Cacu
				case 589: state = 987; break; // &c -> &cu
				case 591: state = 592; break; // &cac -> &cacu
				case 602: state = 603; break; // &capbrc -> &capbrcu
				case 605: state = 608; break; // &capc -> &capcu
				case 641: state = 672; break; // &cc -> &ccu
				case 777: state = 778; break; // &CircleMin -> &CircleMinu
				case 781: state = 782; break; // &CirclePl -> &CirclePlu
				case 815: state = 816; break; // &ClockwiseConto -> &ClockwiseContou
				case 828: state = 829; break; // &CloseC -> &CloseCu
				case 834: state = 835; break; // &CloseCurlyDo -> &CloseCurlyDou
				case 839: state = 840; break; // &CloseCurlyDoubleQ -> &CloseCurlyDoubleQu
				case 844: state = 845; break; // &CloseCurlyQ -> &CloseCurlyQu
				case 849: state = 850; break; // &cl -> &clu
				case 852: state = 853; break; // &clubs -> &clubsu
				case 856: state = 932; break; // &Co -> &Cou
				case 890: state = 891; break; // &Congr -> &Congru
				case 902: state = 903; break; // &Conto -> &Contou
				case 922: state = 923; break; // &Coprod -> &Coprodu
				case 950: state = 951; break; // &CounterClockwiseConto -> &CounterClockwiseContou
				case 975: state = 978; break; // &cs -> &csu
				case 1015: state = 1018; break; // &cupc -> &cupcu
				case 1039: state = 1040; break; // &curlyeqs -> &curlyeqsu
				case 1097: state = 1631; break; // &d -> &du
				case 1202: state = 1203; break; // &DiacriticalAc -> &DiacriticalAcu
				case 1207: state = 1209; break; // &DiacriticalDo -> &DiacriticalDou
				case 1214: state = 1215; break; // &DiacriticalDoubleAc -> &DiacriticalDoubleAcu
				case 1238: state = 1239; break; // &diamonds -> &diamondsu
				case 1291: state = 1331; break; // &do -> &dou
				case 1296: state = 1343; break; // &Do -> &Dou
				case 1312: state = 1313; break; // &DotEq -> &DotEqu
				case 1318: state = 1319; break; // &dotmin -> &dotminu
				case 1322: state = 1323; break; // &dotpl -> &dotplu
				case 1326: state = 1327; break; // &dotsq -> &dotsqu
				case 1351: state = 1352; break; // &DoubleConto -> &DoubleContou
				case 1656: state = 1917; break; // &E -> &Eu
				case 1658: state = 1659; break; // &Eac -> &Eacu
				case 1662: state = 1920; break; // &e -> &eu
				case 1664: state = 1665; break; // &eac -> &eacu
				case 1769: state = 1770; break; // &EmptySmallSq -> &EmptySmallSqu
				case 1785: state = 1786; break; // &EmptyVerySmallSq -> &EmptyVerySmallSqu
				case 1818: state = 1819; break; // &epl -> &eplu
				case 1833: state = 1860; break; // &eq -> &equ
				case 1856: state = 1857; break; // &Eq -> &Equ
				case 1877: state = 1878; break; // &Equilibri -> &Equilibriu
				case 2016: state = 2017; break; // &FilledSmallSq -> &FilledSmallSqu
				case 2031: state = 2032; break; // &FilledVerySmallSq -> &FilledVerySmallSqu
				case 2052: state = 2068; break; // &Fo -> &Fou
				case 2120: state = 2121; break; // &gac -> &gacu
				case 2239: state = 2240; break; // &GreaterEq -> &GreaterEqu
				case 2247: state = 2248; break; // &GreaterF -> &GreaterFu
				case 2252: state = 2253; break; // &GreaterFullEq -> &GreaterFullEqu
				case 2273: state = 2274; break; // &GreaterSlantEq -> &GreaterSlantEqu
				case 2306: state = 2307; break; // &gtq -> &gtqu
				case 2351: state = 2508; break; // &H -> &Hu
				case 2401: state = 2402; break; // &hearts -> &heartsu
				case 2515: state = 2516; break; // &HumpDownH -> &HumpDownHu
				case 2520: state = 2521; break; // &HumpEq -> &HumpEqu
				case 2525: state = 2526; break; // &hyb -> &hybu
				case 2533: state = 2765; break; // &I -> &Iu
				case 2535: state = 2536; break; // &Iac -> &Iacu
				case 2539: state = 2769; break; // &i -> &iu
				case 2541: state = 2542; break; // &iac -> &iacu
				case 2735: state = 2736; break; // &iq -> &iqu
				case 2777: state = 2817; break; // &J -> &Ju
				case 2782: state = 2821; break; // &j -> &ju
				case 2881: state = 3724; break; // &l -> &lu
				case 2888: state = 2889; break; // &Lac -> &Lacu
				case 2893: state = 2894; break; // &lac -> &lacu
				case 2931: state = 2932; break; // &laq -> &laqu
				case 2985: state = 2987; break; // &lbrksl -> &lbrkslu
				case 2993: state = 3008; break; // &lc -> &lcu
				case 3015: state = 3016; break; // &ldq -> &ldqu
				case 3019: state = 3024; break; // &ldr -> &ldru
				case 3089: state = 3090; break; // &LeftDo -> &LeftDou
				case 3132: state = 3137; break; // &leftharpoon -> &leftharpoonu
				case 3189: state = 3190; break; // &leftrightsq -> &leftrightsqu
				case 3239: state = 3240; break; // &LeftTriangleEq -> &LeftTriangleEqu
				case 3324: state = 3325; break; // &LessEq -> &LessEqu
				case 3335: state = 3336; break; // &LessF -> &LessFu
				case 3340: state = 3341; break; // &LessFullEq -> &LessFullEqu
				case 3367: state = 3368; break; // &LessSlantEq -> &LessSlantEqu
				case 3395: state = 3397; break; // &lhar -> &lharu
				case 3444: state = 3445; break; // &lmo -> &lmou
				case 3595: state = 3596; break; // &lopl -> &loplu
				case 3663: state = 3664; break; // &lsaq -> &lsaqu
				case 3677: state = 3679; break; // &lsq -> &lsqu
				case 3712: state = 3713; break; // &ltq -> &ltqu
				case 3725: state = 3731; break; // &lur -> &luru
				case 3745: state = 3887; break; // &m -> &mu
				case 3755: state = 3886; break; // &M -> &Mu
				case 3761: state = 3770; break; // &mapsto -> &mapstou
				case 3794: state = 3795; break; // &meas -> &measu
				case 3806: state = 3807; break; // &Medi -> &Mediu
				case 3841: state = 3842; break; // &min -> &minu
				case 3845: state = 3846; break; // &minusd -> &minusdu
				case 3848: state = 3849; break; // &Min -> &Minu
				case 3852: state = 3853; break; // &MinusPl -> &MinusPlu
				case 3862: state = 3863; break; // &mnpl -> &mnplu
				case 3897: state = 4753; break; // &n -> &nu
				case 3902: state = 4752; break; // &N -> &Nu
				case 3904: state = 3905; break; // &Nac -> &Nacu
				case 3908: state = 3909; break; // &nac -> &nacu
				case 3924: state = 3925; break; // &nat -> &natu
				case 3930: state = 3933; break; // &nb -> &nbu
				case 3937: state = 3962; break; // &nc -> &ncu
				case 3994: state = 3995; break; // &NegativeMedi -> &NegativeMediu
				case 4031: state = 4032; break; // &neq -> &nequ
				case 4217: state = 4226; break; // &NotC -> &NotCu
				case 4221: state = 4222; break; // &NotCongr -> &NotCongru
				case 4232: state = 4233; break; // &NotDo -> &NotDou
				case 4255: state = 4256; break; // &NotEq -> &NotEqu
				case 4277: state = 4278; break; // &NotGreaterEq -> &NotGreaterEqu
				case 4281: state = 4282; break; // &NotGreaterF -> &NotGreaterFu
				case 4286: state = 4287; break; // &NotGreaterFullEq -> &NotGreaterFullEqu
				case 4307: state = 4308; break; // &NotGreaterSlantEq -> &NotGreaterSlantEqu
				case 4316: state = 4317; break; // &NotH -> &NotHu
				case 4324: state = 4325; break; // &NotHumpDownH -> &NotHumpDownHu
				case 4329: state = 4330; break; // &NotHumpEq -> &NotHumpEqu
				case 4359: state = 4360; break; // &NotLeftTriangleEq -> &NotLeftTriangleEqu
				case 4366: state = 4367; break; // &NotLessEq -> &NotLessEqu
				case 4387: state = 4388; break; // &NotLessSlantEq -> &NotLessSlantEqu
				case 4439: state = 4440; break; // &NotPrecedesEq -> &NotPrecedesEqu
				case 4449: state = 4450; break; // &NotPrecedesSlantEq -> &NotPrecedesSlantEqu
				case 4483: state = 4484; break; // &NotRightTriangleEq -> &NotRightTriangleEqu
				case 4487: state = 4515; break; // &NotS -> &NotSu
				case 4488: state = 4489; break; // &NotSq -> &NotSqu
				case 4493: state = 4494; break; // &NotSquareS -> &NotSquareSu
				case 4500: state = 4501; break; // &NotSquareSubsetEq -> &NotSquareSubsetEqu
				case 4511: state = 4512; break; // &NotSquareSupersetEq -> &NotSquareSupersetEqu
				case 4521: state = 4522; break; // &NotSubsetEq -> &NotSubsetEqu
				case 4532: state = 4533; break; // &NotSucceedsEq -> &NotSucceedsEqu
				case 4542: state = 4543; break; // &NotSucceedsSlantEq -> &NotSucceedsSlantEqu
				case 4558: state = 4559; break; // &NotSupersetEq -> &NotSupersetEqu
				case 4568: state = 4569; break; // &NotTildeEq -> &NotTildeEqu
				case 4572: state = 4573; break; // &NotTildeF -> &NotTildeFu
				case 4577: state = 4578; break; // &NotTildeFullEq -> &NotTildeFullEqu
				case 4614: state = 4615; break; // &nprc -> &nprcu
				case 4653: state = 4695; break; // &ns -> &nsu
				case 4655: state = 4656; break; // &nscc -> &nsccu
				case 4689: state = 4690; break; // &nsqs -> &nsqsu
				case 4827: state = 5049; break; // &O -> &Ou
				case 4829: state = 4830; break; // &Oac -> &Oacu
				case 4833: state = 5052; break; // &o -> &ou
				case 4835: state = 4836; break; // &oac -> &oacu
				case 4948: state = 4949; break; // &omin -> &ominu
				case 4963: state = 4964; break; // &OpenC -> &OpenCu
				case 4969: state = 4970; break; // &OpenCurlyDo -> &OpenCurlyDou
				case 4974: state = 4975; break; // &OpenCurlyDoubleQ -> &OpenCurlyDoubleQu
				case 4979: state = 4980; break; // &OpenCurlyQ -> &OpenCurlyQu
				case 4987: state = 4988; break; // &opl -> &oplu
				case 5083: state = 5343; break; // &p -> &pu
				case 5150: state = 5158; break; // &pl -> &plu
				case 5168: state = 5170; break; // &plusd -> &plusdu
				case 5172: state = 5173; break; // &Pl -> &Plu
				case 5177: state = 5178; break; // &PlusMin -> &PlusMinu
				case 5201: state = 5212; break; // &po -> &pou
				case 5216: state = 5331; break; // &pr -> &pru
				case 5219: state = 5220; break; // &prc -> &prcu
				case 5231: state = 5232; break; // &precc -> &preccu
				case 5245: state = 5246; break; // &PrecedesEq -> &PrecedesEqu
				case 5255: state = 5256; break; // &PrecedesSlantEq -> &PrecedesSlantEqu
				case 5299: state = 5300; break; // &Prod -> &Produ
				case 5312: state = 5313; break; // &profs -> &profsu
				case 5351: state = 5374; break; // &q -> &qu
				case 5397: state = 5978; break; // &r -> &ru
				case 5403: state = 5411; break; // &rac -> &racu
				case 5405: state = 5968; break; // &R -> &Ru
				case 5407: state = 5408; break; // &Rac -> &Racu
				case 5431: state = 5432; break; // &raq -> &raqu
				case 5499: state = 5501; break; // &rbrksl -> &rbrkslu
				case 5507: state = 5522; break; // &rc -> &rcu
				case 5534: state = 5535; break; // &rdq -> &rdqu
				case 5569: state = 5570; break; // &ReverseEq -> &ReverseEqu
				case 5576: state = 5577; break; // &ReverseEquilibri -> &ReverseEquilibriu
				case 5582: state = 5583; break; // &ReverseUpEq -> &ReverseUpEqu
				case 5589: state = 5590; break; // &ReverseUpEquilibri -> &ReverseUpEquilibriu
				case 5609: state = 5611; break; // &rhar -> &rharu
				case 5675: state = 5676; break; // &RightDo -> &RightDou
				case 5718: state = 5723; break; // &rightharpoon -> &rightharpoonu
				case 5755: state = 5756; break; // &rightsq -> &rightsqu
				case 5799: state = 5800; break; // &RightTriangleEq -> &RightTriangleEqu
				case 5863: state = 5864; break; // &rmo -> &rmou
				case 5887: state = 5899; break; // &Ro -> &Rou
				case 5891: state = 5892; break; // &ropl -> &roplu
				case 5936: state = 5937; break; // &rsaq -> &rsaqu
				case 5946: state = 5948; break; // &rsq -> &rsqu
				case 5979: state = 5980; break; // &rul -> &rulu
				case 5985: state = 6381; break; // &S -> &Su
				case 5987: state = 5988; break; // &Sac -> &Sacu
				case 5991: state = 6383; break; // &s -> &su
				case 5993: state = 5994; break; // &sac -> &sacu
				case 5998: state = 5999; break; // &sbq -> &sbqu
				case 6012: state = 6013; break; // &scc -> &sccu
				case 6075: state = 6076; break; // &setmin -> &setminu
				case 6181: state = 6182; break; // &simpl -> &simplu
				case 6211: state = 6212; break; // &smallsetmin -> &smallsetminu
				case 6253: state = 6254; break; // &spades -> &spadesu
				case 6258: state = 6285; break; // &sq -> &squ
				case 6259: state = 6263; break; // &sqc -> &sqcu
				case 6266: state = 6286; break; // &Sq -> &Squ
				case 6269: state = 6270; break; // &sqs -> &sqsu
				case 6305: state = 6306; break; // &SquareS -> &SquareSu
				case 6312: state = 6313; break; // &SquareSubsetEq -> &SquareSubsetEqu
				case 6323: state = 6324; break; // &SquareSupersetEq -> &SquareSupersetEqu
				case 6393: state = 6394; break; // &subm -> &submu
				case 6401: state = 6402; break; // &subpl -> &subplu
				case 6411: state = 6428; break; // &subs -> &subsu
				case 6418: state = 6419; break; // &SubsetEq -> &SubsetEqu
				case 6439: state = 6440; break; // &succc -> &succcu
				case 6453: state = 6454; break; // &SucceedsEq -> &SucceedsEqu
				case 6463: state = 6464; break; // &SucceedsSlantEq -> &SucceedsSlantEqu
				case 6507: state = 6508; break; // &supds -> &supdsu
				case 6521: state = 6522; break; // &SupersetEq -> &SupersetEqu
				case 6526: state = 6529; break; // &suphs -> &suphsu
				case 6535: state = 6536; break; // &supm -> &supmu
				case 6543: state = 6544; break; // &suppl -> &supplu
				case 6549: state = 6561; break; // &sups -> &supsu
				case 6584: state = 6592; break; // &Ta -> &Tau
				case 6587: state = 6593; break; // &ta -> &tau
				case 6705: state = 6706; break; // &TildeEq -> &TildeEqu
				case 6709: state = 6710; break; // &TildeF -> &TildeFu
				case 6714: state = 6715; break; // &TildeFullEq -> &TildeFullEqu
				case 6794: state = 6795; break; // &trimin -> &triminu
				case 6806: state = 6807; break; // &tripl -> &triplu
				case 6818: state = 6819; break; // &trpezi -> &trpeziu
				case 6873: state = 7212; break; // &U -> &Uu
				case 6875: state = 6876; break; // &Uac -> &Uacu
				case 6879: state = 7208; break; // &u -> &uu
				case 6881: state = 6882; break; // &uac -> &uacu
				case 7016: state = 7017; break; // &UnionPl -> &UnionPlu
				case 7042: state = 7158; break; // &up -> &upu
				case 7088: state = 7089; break; // &UpEq -> &UpEqu
				case 7095: state = 7096; break; // &UpEquilibri -> &UpEquilibriu
				case 7114: state = 7115; break; // &upl -> &uplu
				case 7264: state = 7269; break; // &vars -> &varsu
				case 7403: state = 7404; break; // &vns -> &vnsu
				case 7424: state = 7427; break; // &vs -> &vsu
				case 7495: state = 7568; break; // &x -> &xu
				case 7496: state = 7502; break; // &xc -> &xcu
				case 7544: state = 7545; break; // &xopl -> &xoplu
				case 7565: state = 7566; break; // &xsqc -> &xsqcu
				case 7570: state = 7571; break; // &xupl -> &xuplu
				case 7584: state = 7640; break; // &Y -> &Yu
				case 7586: state = 7587; break; // &Yac -> &Yacu
				case 7590: state = 7637; break; // &y -> &yu
				case 7592: state = 7593; break; // &yac -> &yacu
				case 7647: state = 7648; break; // &Zac -> &Zacu
				case 7653: state = 7654; break; // &zac -> &zacu
				default: return false;
				}
				break;
			case 'v':
				switch (state) {
				case 0: state = 7223; break; // & -> &v
				case 15: state = 16; break; // &Abre -> &Abrev
				case 20: state = 21; break; // &abre -> &abrev
				case 52: state = 53; break; // &Agra -> &Agrav
				case 57: state = 58; break; // &agra -> &agrav
				case 91: state = 101; break; // &and -> &andv
				case 119: state = 120; break; // &angrt -> &angrtv
				case 256: state = 257; break; // &Bar -> &Barv
				case 258: state = 259; break; // &bar -> &barv
				case 303: state = 304; break; // &bempty -> &bemptyv
				case 334: state = 383; break; // &big -> &bigv
				case 449: state = 450; break; // &bnequi -> &bnequiv
				case 470: state = 519; break; // &box -> &boxv
				case 538: state = 539; break; // &Bre -> &Brev
				case 541: state = 545; break; // &br -> &brv
				case 542: state = 543; break; // &bre -> &brev
				case 696: state = 697; break; // &cempty -> &cemptyv
				case 987: state = 1070; break; // &cu -> &cuv
				case 1026: state = 1054; break; // &cur -> &curv
				case 1032: state = 1043; break; // &curly -> &curlyv
				case 1115: state = 1119; break; // &dash -> &dashv
				case 1117: state = 1118; break; // &Dash -> &Dashv
				case 1173: state = 1174; break; // &dempty -> &demptyv
				case 1220: state = 1221; break; // &DiacriticalGra -> &DiacriticalGrav
				case 1228: state = 1263; break; // &di -> &div
				case 1497: state = 1498; break; // &DownBre -> &DownBrev
				case 1717: state = 1718; break; // &Egra -> &Egrav
				case 1721: state = 1722; break; // &egra -> &egrav
				case 1756: state = 1774; break; // &empty -> &emptyv
				case 1822: state = 1832; break; // &epsi -> &epsiv
				case 1833: state = 1884; break; // &eq -> &eqv
				case 1880: state = 1881; break; // &equi -> &equiv
				case 2066: state = 2067; break; // &fork -> &forkv
				case 2118: state = 2341; break; // &g -> &gv
				case 2137: state = 2138; break; // &Gbre -> &Gbrev
				case 2142: state = 2143; break; // &gbre -> &gbrev
				case 2229: state = 2230; break; // &gra -> &grav
				case 2574: state = 2575; break; // &Igra -> &Igrav
				case 2579: state = 2580; break; // &igra -> &igrav
				case 2656: state = 2693; break; // &In -> &Inv
				case 2747: state = 2754; break; // &isin -> &isinv
				case 2752: state = 2753; break; // &isins -> &isinsv
				case 2834: state = 2835; break; // &kappa -> &kappav
				case 2881: state = 3735; break; // &l -> &lv
				case 2901: state = 2902; break; // &laempty -> &laemptyv
				case 3897: state = 4760; break; // &n -> &nv
				case 3988: state = 3989; break; // &Negati -> &Negativ
				case 4033: state = 4034; break; // &nequi -> &nequiv
				case 4097: state = 4100; break; // &nGt -> &nGtv
				case 4111: state = 4114; break; // &ni -> &niv
				case 4181: state = 4186; break; // &nLt -> &nLtv
				case 4334: state = 4339; break; // &notin -> &notinv
				case 4425: state = 4426; break; // &notni -> &notniv
				case 4454: state = 4455; break; // &NotRe -> &NotRev
				case 4827: state = 5059; break; // &O -> &Ov
				case 4833: state = 5055; break; // &o -> &ov
				case 4864: state = 4865; break; // &odi -> &odiv
				case 4892: state = 4893; break; // &Ogra -> &Ograv
				case 4896: state = 4897; break; // &ogra -> &ograv
				case 4991: state = 5013; break; // &or -> &orv
				case 5131: state = 5132; break; // &phi -> &phiv
				case 5141: state = 5149; break; // &pi -> &piv
				case 5156: state = 5157; break; // &plank -> &plankv
				case 5421: state = 5422; break; // &raempty -> &raemptyv
				case 5540: state = 5557; break; // &Re -> &Rev
				case 5615: state = 5616; break; // &rho -> &rhov
				case 6165: state = 6167; break; // &sigma -> &sigmav
				case 6649: state = 6653; break; // &theta -> &thetav
				case 6904: state = 6905; break; // &Ubre -> &Ubrev
				case 6907: state = 6908; break; // &ubre -> &ubrev
				case 6946: state = 6947; break; // &Ugra -> &Ugrav
				case 6951: state = 6952; break; // &ugra -> &ugrav
				case 7307: state = 7436; break; // &V -> &Vv
				case 7313: state = 7314; break; // &vBar -> &vBarv
				case 7495: state = 7576; break; // &x -> &xv
				default: return false;
				}
				break;
			case 'w':
				switch (state) {
				case 0: state = 7452; break; // & -> &w
				case 7: state = 212; break; // &a -> &aw
				case 256: state = 262; break; // &Bar -> &Barw
				case 258: state = 265; break; // &bar -> &barw
				case 322: state = 325; break; // &bet -> &betw
				case 334: state = 386; break; // &big -> &bigw
				case 373: state = 374; break; // &bigtriangledo -> &bigtriangledow
				case 394: state = 395; break; // &bkaro -> &bkarow
				case 422: state = 423; break; // &blacktriangledo -> &blacktriangledow
				case 459: state = 466; break; // &bo -> &bow
				case 589: state = 1076; break; // &c -> &cw
				case 742: state = 743; break; // &circlearro -> &circlearrow
				case 806: state = 807; break; // &Clock -> &Clockw
				case 941: state = 942; break; // &CounterClock -> &CounterClockw
				case 987: state = 1073; break; // &cu -> &cuw
				case 1032: state = 1046; break; // &curly -> &curlyw
				case 1059: state = 1060; break; // &curvearro -> &curvearrow
				case 1097: state = 1638; break; // &d -> &dw
				case 1124: state = 1125; break; // &dbkaro -> &dbkarow
				case 1291: state = 1478; break; // &do -> &dow
				case 1296: state = 1466; break; // &Do -> &Dow
				case 1337: state = 1338; break; // &doublebar -> &doublebarw
				case 1363: state = 1365; break; // &DoubleDo -> &DoubleDow
				case 1370: state = 1371; break; // &DoubleDownArro -> &DoubleDownArrow
				case 1379: state = 1380; break; // &DoubleLeftArro -> &DoubleLeftArrow
				case 1389: state = 1390; break; // &DoubleLeftRightArro -> &DoubleLeftRightArrow
				case 1404: state = 1405; break; // &DoubleLongLeftArro -> &DoubleLongLeftArrow
				case 1414: state = 1415; break; // &DoubleLongLeftRightArro -> &DoubleLongLeftRightArrow
				case 1424: state = 1425; break; // &DoubleLongRightArro -> &DoubleLongRightArrow
				case 1434: state = 1435; break; // &DoubleRightArro -> &DoubleRightArrow
				case 1444: state = 1445; break; // &DoubleUpArro -> &DoubleUpArrow
				case 1447: state = 1448; break; // &DoubleUpDo -> &DoubleUpDow
				case 1453: state = 1454; break; // &DoubleUpDownArro -> &DoubleUpDownArrow
				case 1471: state = 1472; break; // &DownArro -> &DownArrow
				case 1476: state = 1477; break; // &Downarro -> &Downarrow
				case 1483: state = 1484; break; // &downarro -> &downarrow
				case 1493: state = 1494; break; // &DownArrowUpArro -> &DownArrowUpArrow
				case 1501: state = 1502; break; // &downdo -> &downdow
				case 1507: state = 1508; break; // &downdownarro -> &downdownarrow
				case 1588: state = 1589; break; // &DownTeeArro -> &DownTeeArrow
				case 1595: state = 1596; break; // &drbkaro -> &drbkarow
				case 2109: state = 2110; break; // &fro -> &frow
				case 2380: state = 2384; break; // &harr -> &harrw
				case 2429: state = 2435; break; // &hks -> &hksw
				case 2433: state = 2434; break; // &hksearo -> &hksearow
				case 2438: state = 2439; break; // &hkswaro -> &hkswarow
				case 2457: state = 2458; break; // &hookleftarro -> &hookleftarrow
				case 2467: state = 2468; break; // &hookrightarro -> &hookrightarrow
				case 2512: state = 2513; break; // &HumpDo -> &HumpDow
				case 3050: state = 3051; break; // &LeftArro -> &LeftArrow
				case 3055: state = 3056; break; // &Leftarro -> &Leftarrow
				case 3062: state = 3063; break; // &leftarro -> &leftarrow
				case 3075: state = 3076; break; // &LeftArrowRightArro -> &LeftArrowRightArrow
				case 3089: state = 3101; break; // &LeftDo -> &LeftDow
				case 3134: state = 3135; break; // &leftharpoondo -> &leftharpoondow
				case 3146: state = 3147; break; // &leftleftarro -> &leftleftarrow
				case 3157: state = 3158; break; // &LeftRightArro -> &LeftRightArrow
				case 3167: state = 3168; break; // &Leftrightarro -> &Leftrightarrow
				case 3177: state = 3178; break; // &leftrightarro -> &leftrightarrow
				case 3196: state = 3197; break; // &leftrightsquigarro -> &leftrightsquigarrow
				case 3210: state = 3211; break; // &LeftTeeArro -> &LeftTeeArrow
				case 3246: state = 3247; break; // &LeftUpDo -> &LeftUpDow
				case 3425: state = 3426; break; // &Lleftarro -> &Lleftarrow
				case 3466: state = 3603; break; // &lo -> &low
				case 3475: state = 3610; break; // &Lo -> &Low
				case 3485: state = 3486; break; // &LongLeftArro -> &LongLeftArrow
				case 3494: state = 3495; break; // &Longleftarro -> &Longleftarrow
				case 3505: state = 3506; break; // &longleftarro -> &longleftarrow
				case 3515: state = 3516; break; // &LongLeftRightArro -> &LongLeftRightArrow
				case 3525: state = 3526; break; // &Longleftrightarro -> &Longleftrightarrow
				case 3535: state = 3536; break; // &longleftrightarro -> &longleftrightarrow
				case 3551: state = 3552; break; // &LongRightArro -> &LongRightArrow
				case 3561: state = 3562; break; // &Longrightarro -> &Longrightarrow
				case 3571: state = 3572; break; // &longrightarro -> &longrightarrow
				case 3578: state = 3579; break; // &looparro -> &looparrow
				case 3620: state = 3621; break; // &LowerLeftArro -> &LowerLeftArrow
				case 3630: state = 3631; break; // &LowerRightArro -> &LowerRightArrow
				case 3763: state = 3764; break; // &mapstodo -> &mapstodow
				case 3897: state = 4812; break; // &n -> &nw
				case 3979: state = 3980; break; // &nearro -> &nearrow
				case 3984: state = 4067; break; // &Ne -> &New
				case 4139: state = 4140; break; // &nLeftarro -> &nLeftarrow
				case 4146: state = 4147; break; // &nleftarro -> &nleftarrow
				case 4156: state = 4157; break; // &nLeftrightarro -> &nLeftrightarrow
				case 4166: state = 4167; break; // &nleftrightarro -> &nleftrightarrow
				case 4321: state = 4322; break; // &NotHumpDo -> &NotHumpDow
				case 4627: state = 4629; break; // &nrarr -> &nrarrw
				case 4638: state = 4639; break; // &nRightarro -> &nRightarrow
				case 4647: state = 4648; break; // &nrightarro -> &nrightarrow
				case 4821: state = 4822; break; // &nwarro -> &nwarrow
				case 5185: state = 5186; break; // &plust -> &plustw
				case 5439: state = 5461; break; // &rarr -> &rarrw
				case 5635: state = 5636; break; // &RightArro -> &RightArrow
				case 5640: state = 5641; break; // &Rightarro -> &Rightarrow
				case 5649: state = 5650; break; // &rightarro -> &rightarrow
				case 5661: state = 5662; break; // &RightArrowLeftArro -> &RightArrowLeftArrow
				case 5675: state = 5687; break; // &RightDo -> &RightDow
				case 5720: state = 5721; break; // &rightharpoondo -> &rightharpoondow
				case 5732: state = 5733; break; // &rightleftarro -> &rightleftarrow
				case 5751: state = 5752; break; // &rightrightarro -> &rightrightarrow
				case 5762: state = 5763; break; // &rightsquigarro -> &rightsquigarrow
				case 5770: state = 5771; break; // &RightTeeArro -> &RightTeeArrow
				case 5806: state = 5807; break; // &RightUpDo -> &RightUpDow
				case 5932: state = 5933; break; // &Rrightarro -> &Rrightarrow
				case 5991: state = 6564; break; // &s -> &sw
				case 6062: state = 6063; break; // &searro -> &searrow
				case 6068: state = 6069; break; // &ses -> &sesw
				case 6085: state = 6086; break; // &sfro -> &sfrow
				case 6109: state = 6110; break; // &ShortDo -> &ShortDow
				case 6115: state = 6116; break; // &ShortDownArro -> &ShortDownArrow
				case 6124: state = 6125; break; // &ShortLeftArro -> &ShortLeftArrow
				case 6148: state = 6149; break; // &ShortRightArro -> &ShortRightArrow
				case 6155: state = 6156; break; // &ShortUpArro -> &ShortUpArrow
				case 6573: state = 6574; break; // &swarro -> &swarrow
				case 6575: state = 6576; break; // &swn -> &swnw
				case 6586: state = 6845; break; // &t -> &tw
				case 6771: state = 6772; break; // &triangledo -> &triangledow
				case 6861: state = 6862; break; // &twoheadleftarro -> &twoheadleftarrow
				case 6871: state = 6872; break; // &twoheadrightarro -> &twoheadrightarrow
				case 6879: state = 7217; break; // &u -> &uw
				case 7035: state = 7036; break; // &UpArro -> &UpArrow
				case 7040: state = 7041; break; // &Uparro -> &Uparrow
				case 7046: state = 7047; break; // &uparro -> &uparrow
				case 7052: state = 7053; break; // &UpArrowDo -> &UpArrowDow
				case 7058: state = 7059; break; // &UpArrowDownArro -> &UpArrowDownArrow
				case 7061: state = 7062; break; // &UpDo -> &UpDow
				case 7067: state = 7068; break; // &UpDownArro -> &UpDownArrow
				case 7070: state = 7071; break; // &Updo -> &Updow
				case 7076: state = 7077; break; // &Updownarro -> &Updownarrow
				case 7079: state = 7080; break; // &updo -> &updow
				case 7085: state = 7086; break; // &updownarro -> &updownarrow
				case 7127: state = 7128; break; // &UpperLeftArro -> &UpperLeftArrow
				case 7137: state = 7138; break; // &UpperRightArro -> &UpperRightArrow
				case 7156: state = 7157; break; // &UpTeeArro -> &UpTeeArrow
				case 7163: state = 7164; break; // &upuparro -> &upuparrow
				case 7495: state = 7579; break; // &x -> &xw
				case 7651: state = 7725; break; // &z -> &zw
				default: return false;
				}
				break;
			case 'x':
				switch (state) {
				case 0: state = 7495; break; // & -> &x
				case 168: state = 169; break; // &appro -> &approx
				case 459: state = 470; break; // &bo -> &box
				case 472: state = 473; break; // &boxbo -> &boxbox
				case 875: state = 880; break; // &comple -> &complex
				case 1275: state = 1276; break; // &divon -> &divonx
				case 1656: state = 1931; break; // &E -> &Ex
				case 1662: state = 1925; break; // &e -> &ex
				case 2213: state = 2214; break; // &gnappro -> &gnapprox
				case 2316: state = 2317; break; // &gtrappro -> &gtrapprox
				case 2561: state = 2564; break; // &ie -> &iex
				case 3307: state = 3308; break; // &lessappro -> &lessapprox
				case 3457: state = 3458; break; // &lnappro -> &lnapprox
				case 3922: state = 3923; break; // &nappro -> &napprox
				case 3970: state = 4072; break; // &ne -> &nex
				case 4248: state = 4264; break; // &NotE -> &NotEx
				case 5229: state = 5230; break; // &precappro -> &precapprox
				case 5271: state = 5272; break; // &precnappro -> &precnapprox
				case 5397: state = 5984; break; // &r -> &rx
				case 6053: state = 6079; break; // &se -> &sex
				case 6437: state = 6438; break; // &succappro -> &succapprox
				case 6479: state = 6480; break; // &succnappro -> &succnapprox
				case 6661: state = 6662; break; // &thickappro -> &thickapprox
				case 6846: state = 6847; break; // &twi -> &twix
				default: return false;
				}
				break;
			case 'y':
				switch (state) {
				case 0: state = 7590; break; // & -> &y
				case 23: state = 37; break; // &ac -> &acy
				case 26: state = 36; break; // &Ac -> &Acy
				case 63: state = 64; break; // &alefs -> &alefsy
				case 156: state = 157; break; // &Appl -> &Apply
				case 183: state = 191; break; // &as -> &asy
				case 277: state = 283; break; // &bc -> &bcy
				case 281: state = 282; break; // &Bc -> &Bcy
				case 302: state = 303; break; // &bempt -> &bempty
				case 584: state = 636; break; // &Ca -> &Cay
				case 589: state = 1086; break; // &c -> &cy
				case 638: state = 639; break; // &Cayle -> &Cayley
				case 695: state = 696; break; // &cempt -> &cempty
				case 717: state = 718; break; // &CHc -> &CHcy
				case 720: state = 721; break; // &chc -> &chcy
				case 831: state = 832; break; // &CloseCurl -> &CloseCurly
				case 915: state = 929; break; // &cop -> &copy
				case 1031: state = 1032; break; // &curl -> &curly
				case 1089: state = 1090; break; // &cylct -> &cylcty
				case 1129: state = 1139; break; // &Dc -> &Dcy
				case 1134: state = 1140; break; // &dc -> &dcy
				case 1172: state = 1173; break; // &dempt -> &dempty
				case 1278: state = 1279; break; // &DJc -> &DJcy
				case 1281: state = 1282; break; // &djc -> &djcy
				case 1608: state = 1613; break; // &dsc -> &dscy
				case 1611: state = 1612; break; // &DSc -> &DScy
				case 1645: state = 1646; break; // &DZc -> &DZcy
				case 1648: state = 1649; break; // &dzc -> &dzcy
				case 1672: state = 1692; break; // &Ec -> &Ecy
				case 1677: state = 1693; break; // &ec -> &ecy
				case 1755: state = 1756; break; // &empt -> &empty
				case 1761: state = 1762; break; // &Empt -> &Empty
				case 1777: state = 1778; break; // &EmptyVer -> &EmptyVery
				case 1978: state = 1979; break; // &Fc -> &Fcy
				case 1980: state = 1981; break; // &fc -> &fcy
				case 2023: state = 2024; break; // &FilledVer -> &FilledVery
				case 2145: state = 2157; break; // &Gc -> &Gcy
				case 2153: state = 2158; break; // &gc -> &gcy
				case 2199: state = 2200; break; // &GJc -> &GJcy
				case 2202: state = 2203; break; // &gjc -> &gjcy
				case 2356: state = 2524; break; // &h -> &hy
				case 2371: state = 2372; break; // &HARDc -> &HARDcy
				case 2375: state = 2376; break; // &hardc -> &hardcy
				case 2545: state = 2554; break; // &ic -> &icy
				case 2546: state = 2553; break; // &Ic -> &Icy
				case 2559: state = 2560; break; // &IEc -> &IEcy
				case 2562: state = 2563; break; // &iec -> &iecy
				case 2618: state = 2619; break; // &Imaginar -> &Imaginary
				case 2711: state = 2712; break; // &IOc -> &IOcy
				case 2714: state = 2715; break; // &ioc -> &iocy
				case 2767: state = 2768; break; // &Iukc -> &Iukcy
				case 2771: state = 2772; break; // &iukc -> &iukcy
				case 2778: state = 2787; break; // &Jc -> &Jcy
				case 2783: state = 2788; break; // &jc -> &jcy
				case 2811: state = 2812; break; // &Jserc -> &Jsercy
				case 2815: state = 2816; break; // &jserc -> &jsercy
				case 2819: state = 2820; break; // &Jukc -> &Jukcy
				case 2823: state = 2824; break; // &jukc -> &jukcy
				case 2836: state = 2846; break; // &Kc -> &Kcy
				case 2841: state = 2847; break; // &kc -> &kcy
				case 2858: state = 2859; break; // &KHc -> &KHcy
				case 2861: state = 2862; break; // &khc -> &khcy
				case 2864: state = 2865; break; // &KJc -> &KJcy
				case 2867: state = 2868; break; // &kjc -> &kjcy
				case 2900: state = 2901; break; // &laempt -> &laempty
				case 2988: state = 3010; break; // &Lc -> &Lcy
				case 2993: state = 3011; break; // &lc -> &lcy
				case 3403: state = 3404; break; // &LJc -> &LJcy
				case 3406: state = 3407; break; // &ljc -> &ljcy
				case 3776: state = 3783; break; // &mc -> &mcy
				case 3781: state = 3782; break; // &Mc -> &Mcy
				case 3937: state = 3965; break; // &nc -> &ncy
				case 3940: state = 3964; break; // &Nc -> &Ncy
				case 4020: state = 4021; break; // &NegativeVer -> &NegativeVery
				case 4116: state = 4117; break; // &NJc -> &NJcy
				case 4119: state = 4120; break; // &njc -> &njcy
				case 4841: state = 4850; break; // &oc -> &ocy
				case 4844: state = 4849; break; // &Oc -> &Ocy
				case 4966: state = 4967; break; // &OpenCurl -> &OpenCurly
				case 5104: state = 5105; break; // &Pc -> &Pcy
				case 5106: state = 5107; break; // &pc -> &pcy
				case 5234: state = 5235; break; // &preccurl -> &preccurly
				case 5420: state = 5421; break; // &raempt -> &raempty
				case 5502: state = 5524; break; // &Rc -> &Rcy
				case 5507: state = 5525; break; // &rc -> &rcy
				case 5974: state = 5975; break; // &RuleDela -> &RuleDelay
				case 6001: state = 6046; break; // &Sc -> &Scy
				case 6002: state = 6047; break; // &sc -> &scy
				case 6088: state = 6157; break; // &sh -> &shy
				case 6095: state = 6096; break; // &SHCHc -> &SHCHcy
				case 6097: state = 6103; break; // &shc -> &shcy
				case 6099: state = 6100; break; // &shchc -> &shchcy
				case 6101: state = 6102; break; // &SHc -> &SHcy
				case 6233: state = 6234; break; // &SOFTc -> &SOFTcy
				case 6238: state = 6239; break; // &softc -> &softcy
				case 6442: state = 6443; break; // &succcurl -> &succcurly
				case 6597: state = 6615; break; // &Tc -> &Tcy
				case 6602: state = 6616; break; // &tc -> &tcy
				case 6650: state = 6651; break; // &thetas -> &thetasy
				case 6825: state = 6830; break; // &tsc -> &tscy
				case 6828: state = 6829; break; // &TSc -> &TScy
				case 6832: state = 6833; break; // &TSHc -> &TSHcy
				case 6835: state = 6836; break; // &tshc -> &tshcy
				case 6898: state = 6899; break; // &Ubrc -> &Ubrcy
				case 6902: state = 6903; break; // &ubrc -> &ubrcy
				case 6910: state = 6918; break; // &Uc -> &Ucy
				case 6914: state = 6919; break; // &uc -> &ucy
				case 7315: state = 7316; break; // &Vc -> &Vcy
				case 7317: state = 7318; break; // &vc -> &vcy
				case 7349: state = 7384; break; // &Ver -> &Very
				case 7592: state = 7599; break; // &yac -> &yacy
				case 7597: state = 7598; break; // &YAc -> &YAcy
				case 7600: state = 7608; break; // &Yc -> &Ycy
				case 7604: state = 7609; break; // &yc -> &ycy
				case 7617: state = 7618; break; // &YIc -> &YIcy
				case 7620: state = 7621; break; // &yic -> &yicy
				case 7635: state = 7636; break; // &YUc -> &YUcy
				case 7638: state = 7639; break; // &yuc -> &yucy
				case 7657: state = 7667; break; // &Zc -> &Zcy
				case 7662: state = 7668; break; // &zc -> &zcy
				case 7702: state = 7703; break; // &ZHc -> &ZHcy
				case 7705: state = 7706; break; // &zhc -> &zhcy
				default: return false;
				}
				break;
			case 'z':
				switch (state) {
				case 0: state = 7651; break; // & -> &z
				case 102: state = 127; break; // &ang -> &angz
				case 401: state = 402; break; // &blacklo -> &blackloz
				case 1097: state = 1647; break; // &d -> &dz
				case 2479: state = 2480; break; // &Hori -> &Horiz
				case 3466: state = 3632; break; // &lo -> &loz
				case 5991: state = 6579; break; // &s -> &sz
				case 6816: state = 6817; break; // &trpe -> &trpez
				case 7223: state = 7441; break; // &v -> &vz
				case 7443: state = 7444; break; // &vzig -> &vzigz
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
			case 12: return "\u00E1"; // &aacute
			case 17: return "\u0102"; // &Abreve
			case 22: return "\u0103"; // &abreve
			case 23: return "\u223E"; // &ac
			case 24: return "\u223F"; // &acd
			case 25: return "\u223E\u0333"; // &acE
			case 29: return "\u00C2"; // &Acirc
			case 32: return "\u00E2"; // &acirc
			case 35: return "\u00B4"; // &acute
			case 36: return "\u0410"; // &Acy
			case 37: return "\u0430"; // &acy
			case 41: return "\u00C6"; // &AElig
			case 45: return "\u00E6"; // &aelig
			case 46: return "\u2061"; // &af
			case 48: return "\uD835\uDD04"; // &Afr
			case 49: return "\uD835\uDD1E"; // &afr
			case 54: return "\u00C0"; // &Agrave
			case 59: return "\u00E0"; // &agrave
			case 65: return "\u2135"; // &alefsym
			case 67: return "\u2135"; // &aleph
			case 71: return "\u0391"; // &Alpha
			case 74: return "\u03B1"; // &alpha
			case 78: return "\u0100"; // &Amacr
			case 82: return "\u0101"; // &amacr
			case 84: return "\u2A3F"; // &amalg
			case 86: return "\u0026"; // &AMP
			case 87: return "\u0026"; // &amp
			case 89: return "\u2A53"; // &And
			case 91: return "\u2227"; // &and
			case 94: return "\u2A55"; // &andand
			case 95: return "\u2A5C"; // &andd
			case 100: return "\u2A58"; // &andslope
			case 101: return "\u2A5A"; // &andv
			case 102: return "\u2220"; // &ang
			case 103: return "\u29A4"; // &ange
			case 105: return "\u2220"; // &angle
			case 108: return "\u2221"; // &angmsd
			case 110: return "\u29A8"; // &angmsdaa
			case 111: return "\u29A9"; // &angmsdab
			case 112: return "\u29AA"; // &angmsdac
			case 113: return "\u29AB"; // &angmsdad
			case 114: return "\u29AC"; // &angmsdae
			case 115: return "\u29AD"; // &angmsdaf
			case 116: return "\u29AE"; // &angmsdag
			case 117: return "\u29AF"; // &angmsdah
			case 119: return "\u221F"; // &angrt
			case 121: return "\u22BE"; // &angrtvb
			case 122: return "\u299D"; // &angrtvbd
			case 125: return "\u2222"; // &angsph
			case 126: return "\u00C5"; // &angst
			case 130: return "\u237C"; // &angzarr
			case 134: return "\u0104"; // &Aogon
			case 138: return "\u0105"; // &aogon
			case 140: return "\uD835\uDD38"; // &Aopf
			case 142: return "\uD835\uDD52"; // &aopf
			case 143: return "\u2248"; // &ap
			case 147: return "\u2A6F"; // &apacir
			case 148: return "\u2A70"; // &apE
			case 149: return "\u224A"; // &ape
			case 151: return "\u224B"; // &apid
			case 153: return "\u0027"; // &apos
			case 165: return "\u2061"; // &ApplyFunction
			case 169: return "\u2248"; // &approx
			case 171: return "\u224A"; // &approxeq
			case 175: return "\u00C5"; // &Aring
			case 179: return "\u00E5"; // &aring
			case 182: return "\uD835\uDC9C"; // &Ascr
			case 185: return "\uD835\uDCB6"; // &ascr
			case 189: return "\u2254"; // &Assign
			case 190: return "\u002A"; // &ast
			case 193: return "\u2248"; // &asymp
			case 195: return "\u224D"; // &asympeq
			case 200: return "\u00C3"; // &Atilde
			case 205: return "\u00E3"; // &atilde
			case 208: return "\u00C4"; // &Auml
			case 211: return "\u00E4"; // &auml
			case 218: return "\u2233"; // &awconint
			case 221: return "\u2A11"; // &awint
			case 229: return "\u224C"; // &backcong
			case 236: return "\u03F6"; // &backepsilon
			case 241: return "\u2035"; // &backprime
			case 244: return "\u223D"; // &backsim
			case 246: return "\u22CD"; // &backsimeq
			case 255: return "\u2216"; // &Backslash
			case 257: return "\u2AE7"; // &Barv
			case 261: return "\u22BD"; // &barvee
			case 264: return "\u2306"; // &Barwed
			case 267: return "\u2305"; // &barwed
			case 269: return "\u2305"; // &barwedge
			case 272: return "\u23B5"; // &bbrk
			case 276: return "\u23B6"; // &bbrktbrk
			case 280: return "\u224C"; // &bcong
			case 282: return "\u0411"; // &Bcy
			case 283: return "\u0431"; // &bcy
			case 287: return "\u201E"; // &bdquo
			case 292: return "\u2235"; // &becaus
			case 298: return "\u2235"; // &Because
			case 299: return "\u2235"; // &because
			case 304: return "\u29B0"; // &bemptyv
			case 307: return "\u03F6"; // &bepsi
			case 311: return "\u212C"; // &bernou
			case 319: return "\u212C"; // &Bernoullis
			case 321: return "\u0392"; // &Beta
			case 323: return "\u03B2"; // &beta
			case 324: return "\u2136"; // &beth
			case 328: return "\u226C"; // &between
			case 330: return "\uD835\uDD05"; // &Bfr
			case 332: return "\uD835\uDD1F"; // &bfr
			case 337: return "\u22C2"; // &bigcap
			case 340: return "\u25EF"; // &bigcirc
			case 342: return "\u22C3"; // &bigcup
			case 346: return "\u2A00"; // &bigodot
			case 350: return "\u2A01"; // &bigoplus
			case 355: return "\u2A02"; // &bigotimes
			case 360: return "\u2A06"; // &bigsqcup
			case 363: return "\u2605"; // &bigstar
			case 375: return "\u25BD"; // &bigtriangledown
			case 377: return "\u25B3"; // &bigtriangleup
			case 382: return "\u2A04"; // &biguplus
			case 385: return "\u22C1"; // &bigvee
			case 390: return "\u22C0"; // &bigwedge
			case 395: return "\u290D"; // &bkarow
			case 406: return "\u29EB"; // &blacklozenge
			case 412: return "\u25AA"; // &blacksquare
			case 420: return "\u25B4"; // &blacktriangle
			case 424: return "\u25BE"; // &blacktriangledown
			case 428: return "\u25C2"; // &blacktriangleleft
			case 433: return "\u25B8"; // &blacktriangleright
			case 435: return "\u2423"; // &blank
			case 438: return "\u2592"; // &blk12
			case 439: return "\u2591"; // &blk14
			case 441: return "\u2593"; // &blk34
			case 444: return "\u2588"; // &block
			case 446: return "\u003D\u20E5"; // &bne
			case 450: return "\u2261\u20E5"; // &bnequiv
			case 453: return "\u2AED"; // &bNot
			case 455: return "\u2310"; // &bnot
			case 458: return "\uD835\uDD39"; // &Bopf
			case 461: return "\uD835\uDD53"; // &bopf
			case 462: return "\u22A5"; // &bot
			case 465: return "\u22A5"; // &bottom
			case 469: return "\u22C8"; // &bowtie
			case 473: return "\u29C9"; // &boxbox
			case 475: return "\u2557"; // &boxDL
			case 476: return "\u2556"; // &boxDl
			case 478: return "\u2555"; // &boxdL
			case 479: return "\u2510"; // &boxdl
			case 480: return "\u2554"; // &boxDR
			case 481: return "\u2553"; // &boxDr
			case 482: return "\u2552"; // &boxdR
			case 483: return "\u250C"; // &boxdr
			case 484: return "\u2550"; // &boxH
			case 485: return "\u2500"; // &boxh
			case 486: return "\u2566"; // &boxHD
			case 487: return "\u2564"; // &boxHd
			case 488: return "\u2565"; // &boxhD
			case 489: return "\u252C"; // &boxhd
			case 490: return "\u2569"; // &boxHU
			case 491: return "\u2567"; // &boxHu
			case 492: return "\u2568"; // &boxhU
			case 493: return "\u2534"; // &boxhu
			case 498: return "\u229F"; // &boxminus
			case 502: return "\u229E"; // &boxplus
			case 507: return "\u22A0"; // &boxtimes
			case 509: return "\u255D"; // &boxUL
			case 510: return "\u255C"; // &boxUl
			case 512: return "\u255B"; // &boxuL
			case 513: return "\u2518"; // &boxul
			case 514: return "\u255A"; // &boxUR
			case 515: return "\u2559"; // &boxUr
			case 516: return "\u2558"; // &boxuR
			case 517: return "\u2514"; // &boxur
			case 518: return "\u2551"; // &boxV
			case 519: return "\u2502"; // &boxv
			case 520: return "\u256C"; // &boxVH
			case 521: return "\u256B"; // &boxVh
			case 522: return "\u256A"; // &boxvH
			case 523: return "\u253C"; // &boxvh
			case 524: return "\u2563"; // &boxVL
			case 525: return "\u2562"; // &boxVl
			case 526: return "\u2561"; // &boxvL
			case 527: return "\u2524"; // &boxvl
			case 528: return "\u2560"; // &boxVR
			case 529: return "\u255F"; // &boxVr
			case 530: return "\u255E"; // &boxvR
			case 531: return "\u251C"; // &boxvr
			case 536: return "\u2035"; // &bprime
			case 540: return "\u02D8"; // &Breve
			case 544: return "\u02D8"; // &breve
			case 548: return "\u00A6"; // &brvbar
			case 551: return "\u212C"; // &Bscr
			case 554: return "\uD835\uDCB7"; // &bscr
			case 557: return "\u204F"; // &bsemi
			case 559: return "\u223D"; // &bsim
			case 560: return "\u22CD"; // &bsime
			case 562: return "\u005C"; // &bsol
			case 563: return "\u29C5"; // &bsolb
			case 567: return "\u27C8"; // &bsolhsub
			case 570: return "\u2022"; // &bull
			case 572: return "\u2022"; // &bullet
			case 574: return "\u224E"; // &bump
			case 575: return "\u2AAE"; // &bumpE
			case 576: return "\u224F"; // &bumpe
			case 581: return "\u224E"; // &Bumpeq
			case 582: return "\u224F"; // &bumpeq
			case 588: return "\u0106"; // &Cacute
			case 594: return "\u0107"; // &cacute
			case 595: return "\u22D2"; // &Cap
			case 596: return "\u2229"; // &cap
			case 599: return "\u2A44"; // &capand
			case 604: return "\u2A49"; // &capbrcup
			case 607: return "\u2A4B"; // &capcap
			case 609: return "\u2A47"; // &capcup
			case 612: return "\u2A40"; // &capdot
			case 629: return "\u2145"; // &CapitalDifferentialD
			case 630: return "\u2229\uFE00"; // &caps
			case 633: return "\u2041"; // &caret
			case 635: return "\u02C7"; // &caron
			case 640: return "\u212D"; // &Cayleys
			case 644: return "\u2A4D"; // &ccaps
			case 649: return "\u010C"; // &Ccaron
			case 652: return "\u010D"; // &ccaron
			case 656: return "\u00C7"; // &Ccedil
			case 660: return "\u00E7"; // &ccedil
			case 663: return "\u0108"; // &Ccirc
			case 666: return "\u0109"; // &ccirc
			case 671: return "\u2230"; // &Cconint
			case 674: return "\u2A4C"; // &ccups
			case 676: return "\u2A50"; // &ccupssm
			case 679: return "\u010A"; // &Cdot
			case 682: return "\u010B"; // &cdot
			case 686: return "\u00B8"; // &cedil
			case 692: return "\u00B8"; // &Cedilla
			case 697: return "\u29B2"; // &cemptyv
			case 699: return "\u00A2"; // &cent
			case 706: return "\u00B7"; // &CenterDot
			case 711: return "\u00B7"; // &centerdot
			case 713: return "\u212D"; // &Cfr
			case 715: return "\uD835\uDD20"; // &cfr
			case 718: return "\u0427"; // &CHcy
			case 721: return "\u0447"; // &chcy
			case 724: return "\u2713"; // &check
			case 728: return "\u2713"; // &checkmark
			case 730: return "\u03A7"; // &Chi
			case 731: return "\u03C7"; // &chi
			case 733: return "\u25CB"; // &cir
			case 734: return "\u02C6"; // &circ
			case 736: return "\u2257"; // &circeq
			case 747: return "\u21BA"; // &circlearrowleft
			case 752: return "\u21BB"; // &circlearrowright
			case 756: return "\u229B"; // &circledast
			case 760: return "\u229A"; // &circledcirc
			case 764: return "\u229D"; // &circleddash
			case 772: return "\u2299"; // &CircleDot
			case 773: return "\u00AE"; // &circledR
			case 774: return "\u24C8"; // &circledS
			case 779: return "\u2296"; // &CircleMinus
			case 783: return "\u2295"; // &CirclePlus
			case 788: return "\u2297"; // &CircleTimes
			case 789: return "\u29C3"; // &cirE
			case 790: return "\u2257"; // &cire
			case 795: return "\u2A10"; // &cirfnint
			case 798: return "\u2AEF"; // &cirmid
			case 802: return "\u29C2"; // &cirscir
			case 825: return "\u2232"; // &ClockwiseContourIntegral
			case 843: return "\u201D"; // &CloseCurlyDoubleQuote
			case 848: return "\u2019"; // &CloseCurlyQuote
			case 852: return "\u2663"; // &clubs
			case 855: return "\u2663"; // &clubsuit
			case 859: return "\u2237"; // &Colon
			case 863: return "\u003A"; // &colon
			case 864: return "\u2A74"; // &Colone
			case 865: return "\u2254"; // &colone
			case 866: return "\u2254"; // &coloneq
			case 869: return "\u002C"; // &comma
			case 870: return "\u0040"; // &commat
			case 871: return "\u2201"; // &comp
			case 873: return "\u2218"; // &compfn
			case 879: return "\u2201"; // &complement
			case 882: return "\u2102"; // &complexes
			case 884: return "\u2245"; // &cong
			case 887: return "\u2A6D"; // &congdot
			case 894: return "\u2261"; // &Congruent
			case 897: return "\u222F"; // &Conint
			case 900: return "\u222E"; // &conint
			case 912: return "\u222E"; // &ContourIntegral
			case 914: return "\u2102"; // &Copf
			case 916: return "\uD835\uDD54"; // &copf
			case 919: return "\u2210"; // &coprod
			case 925: return "\u2210"; // &Coproduct
			case 928: return "\u00A9"; // &COPY
			case 929: return "\u00A9"; // &copy
			case 931: return "\u2117"; // &copysr
			case 960: return "\u2233"; // &CounterClockwiseContourIntegral
			case 964: return "\u21B5"; // &crarr
			case 968: return "\u2A2F"; // &Cross
			case 971: return "\u2717"; // &cross
			case 974: return "\uD835\uDC9E"; // &Cscr
			case 977: return "\uD835\uDCB8"; // &cscr
			case 979: return "\u2ACF"; // &csub
			case 980: return "\u2AD1"; // &csube
			case 981: return "\u2AD0"; // &csup
			case 982: return "\u2AD2"; // &csupe
			case 986: return "\u22EF"; // &ctdot
			case 992: return "\u2938"; // &cudarrl
			case 993: return "\u2935"; // &cudarrr
			case 996: return "\u22DE"; // &cuepr
			case 998: return "\u22DF"; // &cuesc
			case 1002: return "\u21B6"; // &cularr
			case 1003: return "\u293D"; // &cularrp
			case 1005: return "\u22D3"; // &Cup
			case 1006: return "\u222A"; // &cup
			case 1011: return "\u2A48"; // &cupbrcap
			case 1014: return "\u224D"; // &CupCap
			case 1017: return "\u2A46"; // &cupcap
			case 1019: return "\u2A4A"; // &cupcup
			case 1022: return "\u228D"; // &cupdot
			case 1024: return "\u2A45"; // &cupor
			case 1025: return "\u222A\uFE00"; // &cups
			case 1029: return "\u21B7"; // &curarr
			case 1030: return "\u293C"; // &curarrm
			case 1038: return "\u22DE"; // &curlyeqprec
			case 1042: return "\u22DF"; // &curlyeqsucc
			case 1045: return "\u22CE"; // &curlyvee
			case 1050: return "\u22CF"; // &curlywedge
			case 1053: return "\u00A4"; // &curren
			case 1064: return "\u21B6"; // &curvearrowleft
			case 1069: return "\u21B7"; // &curvearrowright
			case 1072: return "\u22CE"; // &cuvee
			case 1075: return "\u22CF"; // &cuwed
			case 1082: return "\u2232"; // &cwconint
			case 1085: return "\u2231"; // &cwint
			case 1090: return "\u232D"; // &cylcty
			case 1096: return "\u2021"; // &Dagger
			case 1102: return "\u2020"; // &dagger
			case 1106: return "\u2138"; // &daleth
			case 1108: return "\u21A1"; // &Darr
			case 1111: return "\u21D3"; // &dArr
			case 1113: return "\u2193"; // &darr
			case 1115: return "\u2010"; // &dash
			case 1118: return "\u2AE4"; // &Dashv
			case 1119: return "\u22A3"; // &dashv
			case 1125: return "\u290F"; // &dbkarow
			case 1128: return "\u02DD"; // &dblac
			case 1133: return "\u010E"; // &Dcaron
			case 1138: return "\u010F"; // &dcaron
			case 1139: return "\u0414"; // &Dcy
			case 1140: return "\u0434"; // &dcy
			case 1141: return "\u2145"; // &DD
			case 1142: return "\u2146"; // &dd
			case 1147: return "\u2021"; // &ddagger
			case 1149: return "\u21CA"; // &ddarr
			case 1155: return "\u2911"; // &DDotrahd
			case 1160: return "\u2A77"; // &ddotseq
			case 1162: return "\u00B0"; // &deg
			case 1164: return "\u2207"; // &Del
			case 1166: return "\u0394"; // &Delta
			case 1169: return "\u03B4"; // &delta
			case 1174: return "\u29B1"; // &demptyv
			case 1179: return "\u297F"; // &dfisht
			case 1181: return "\uD835\uDD07"; // &Dfr
			case 1182: return "\uD835\uDD21"; // &dfr
			case 1185: return "\u2965"; // &dHar
			case 1189: return "\u21C3"; // &dharl
			case 1190: return "\u21C2"; // &dharr
			case 1205: return "\u00B4"; // &DiacriticalAcute
			case 1208: return "\u02D9"; // &DiacriticalDot
			case 1217: return "\u02DD"; // &DiacriticalDoubleAcute
			case 1222: return "\u0060"; // &DiacriticalGrave
			case 1227: return "\u02DC"; // &DiacriticalTilde
			case 1230: return "\u22C4"; // &diam
			case 1234: return "\u22C4"; // &Diamond
			case 1237: return "\u22C4"; // &diamond
			case 1241: return "\u2666"; // &diamondsuit
			case 1242: return "\u2666"; // &diams
			case 1243: return "\u00A8"; // &die
			case 1254: return "\u2146"; // &DifferentialD
			case 1259: return "\u03DD"; // &digamma
			case 1262: return "\u22F2"; // &disin
			case 1263: return "\u00F7"; // &div
			case 1266: return "\u00F7"; // &divide
			case 1273: return "\u22C7"; // &divideontimes
			case 1276: return "\u22C7"; // &divonx
			case 1279: return "\u0402"; // &DJcy
			case 1282: return "\u0452"; // &djcy
			case 1287: return "\u231E"; // &dlcorn
			case 1290: return "\u230D"; // &dlcrop
			case 1295: return "\u0024"; // &dollar
			case 1298: return "\uD835\uDD3B"; // &Dopf
			case 1300: return "\uD835\uDD55"; // &dopf
			case 1301: return "\u00A8"; // &Dot
			case 1302: return "\u02D9"; // &dot
			case 1305: return "\u20DC"; // &DotDot
			case 1307: return "\u2250"; // &doteq
			case 1310: return "\u2251"; // &doteqdot
			case 1315: return "\u2250"; // &DotEqual
			case 1320: return "\u2238"; // &dotminus
			case 1324: return "\u2214"; // &dotplus
			case 1330: return "\u22A1"; // &dotsquare
			case 1342: return "\u2306"; // &doublebarwedge
			case 1361: return "\u222F"; // &DoubleContourIntegral
			case 1364: return "\u00A8"; // &DoubleDot
			case 1371: return "\u21D3"; // &DoubleDownArrow
			case 1380: return "\u21D0"; // &DoubleLeftArrow
			case 1390: return "\u21D4"; // &DoubleLeftRightArrow
			case 1393: return "\u2AE4"; // &DoubleLeftTee
			case 1405: return "\u27F8"; // &DoubleLongLeftArrow
			case 1415: return "\u27FA"; // &DoubleLongLeftRightArrow
			case 1425: return "\u27F9"; // &DoubleLongRightArrow
			case 1435: return "\u21D2"; // &DoubleRightArrow
			case 1438: return "\u22A8"; // &DoubleRightTee
			case 1445: return "\u21D1"; // &DoubleUpArrow
			case 1454: return "\u21D5"; // &DoubleUpDownArrow
			case 1465: return "\u2225"; // &DoubleVerticalBar
			case 1472: return "\u2193"; // &DownArrow
			case 1477: return "\u21D3"; // &Downarrow
			case 1484: return "\u2193"; // &downarrow
			case 1487: return "\u2913"; // &DownArrowBar
			case 1494: return "\u21F5"; // &DownArrowUpArrow
			case 1499: return "\u0311"; // &DownBreve
			case 1509: return "\u21CA"; // &downdownarrows
			case 1520: return "\u21C3"; // &downharpoonleft
			case 1525: return "\u21C2"; // &downharpoonright
			case 1540: return "\u2950"; // &DownLeftRightVector
			case 1549: return "\u295E"; // &DownLeftTeeVector
			case 1555: return "\u21BD"; // &DownLeftVector
			case 1558: return "\u2956"; // &DownLeftVectorBar
			case 1572: return "\u295F"; // &DownRightTeeVector
			case 1578: return "\u21C1"; // &DownRightVector
			case 1581: return "\u2957"; // &DownRightVectorBar
			case 1584: return "\u22A4"; // &DownTee
			case 1589: return "\u21A7"; // &DownTeeArrow
			case 1596: return "\u2910"; // &drbkarow
			case 1600: return "\u231F"; // &drcorn
			case 1603: return "\u230C"; // &drcrop
			case 1606: return "\uD835\uDC9F"; // &Dscr
			case 1609: return "\uD835\uDCB9"; // &dscr
			case 1612: return "\u0405"; // &DScy
			case 1613: return "\u0455"; // &dscy
			case 1615: return "\u29F6"; // &dsol
			case 1619: return "\u0110"; // &Dstrok
			case 1623: return "\u0111"; // &dstrok
			case 1627: return "\u22F1"; // &dtdot
			case 1629: return "\u25BF"; // &dtri
			case 1630: return "\u25BE"; // &dtrif
			case 1634: return "\u21F5"; // &duarr
			case 1637: return "\u296F"; // &duhar
			case 1643: return "\u29A6"; // &dwangle
			case 1646: return "\u040F"; // &DZcy
			case 1649: return "\u045F"; // &dzcy
			case 1655: return "\u27FF"; // &dzigrarr
			case 1661: return "\u00C9"; // &Eacute
			case 1667: return "\u00E9"; // &eacute
			case 1671: return "\u2A6E"; // &easter
			case 1676: return "\u011A"; // &Ecaron
			case 1681: return "\u011B"; // &ecaron
			case 1683: return "\u2256"; // &ecir
			case 1686: return "\u00CA"; // &Ecirc
			case 1687: return "\u00EA"; // &ecirc
			case 1691: return "\u2255"; // &ecolon
			case 1692: return "\u042D"; // &Ecy
			case 1693: return "\u044D"; // &ecy
			case 1697: return "\u2A77"; // &eDDot
			case 1700: return "\u0116"; // &Edot
			case 1702: return "\u2251"; // &eDot
			case 1705: return "\u0117"; // &edot
			case 1706: return "\u2147"; // &ee
			case 1710: return "\u2252"; // &efDot
			case 1712: return "\uD835\uDD08"; // &Efr
			case 1713: return "\uD835\uDD22"; // &efr
			case 1714: return "\u2A9A"; // &eg
			case 1719: return "\u00C8"; // &Egrave
			case 1723: return "\u00E8"; // &egrave
			case 1724: return "\u2A96"; // &egs
			case 1727: return "\u2A98"; // &egsdot
			case 1728: return "\u2A99"; // &el
			case 1734: return "\u2208"; // &Element
			case 1740: return "\u23E7"; // &elinters
			case 1741: return "\u2113"; // &ell
			case 1742: return "\u2A95"; // &els
			case 1745: return "\u2A97"; // &elsdot
			case 1749: return "\u0112"; // &Emacr
			case 1753: return "\u0113"; // &emacr
			case 1756: return "\u2205"; // &empty
			case 1759: return "\u2205"; // &emptyset
			case 1773: return "\u25FB"; // &EmptySmallSquare
			case 1774: return "\u2205"; // &emptyv
			case 1789: return "\u25AB"; // &EmptyVerySmallSquare
			case 1791: return "\u2003"; // &emsp
			case 1793: return "\u2004"; // &emsp13
			case 1794: return "\u2005"; // &emsp14
			case 1796: return "\u014A"; // &ENG
			case 1798: return "\u014B"; // &eng
			case 1800: return "\u2002"; // &ensp
			case 1804: return "\u0118"; // &Eogon
			case 1808: return "\u0119"; // &eogon
			case 1810: return "\uD835\uDD3C"; // &Eopf
			case 1812: return "\uD835\uDD56"; // &eopf
			case 1815: return "\u22D5"; // &epar
			case 1817: return "\u29E3"; // &eparsl
			case 1820: return "\u2A71"; // &eplus
			case 1822: return "\u03B5"; // &epsi
			case 1828: return "\u0395"; // &Epsilon
			case 1831: return "\u03B5"; // &epsilon
			case 1832: return "\u03F5"; // &epsiv
			case 1837: return "\u2256"; // &eqcirc
			case 1841: return "\u2255"; // &eqcolon
			case 1844: return "\u2242"; // &eqsim
			case 1851: return "\u2A96"; // &eqslantgtr
			case 1855: return "\u2A95"; // &eqslantless
			case 1859: return "\u2A75"; // &Equal
			case 1863: return "\u003D"; // &equals
			case 1868: return "\u2242"; // &EqualTilde
			case 1871: return "\u225F"; // &equest
			case 1879: return "\u21CC"; // &Equilibrium
			case 1881: return "\u2261"; // &equiv
			case 1883: return "\u2A78"; // &equivDD
			case 1889: return "\u29E5"; // &eqvparsl
			case 1893: return "\u2971"; // &erarr
			case 1896: return "\u2253"; // &erDot
			case 1899: return "\u2130"; // &Escr
			case 1902: return "\u212F"; // &escr
			case 1905: return "\u2250"; // &esdot
			case 1907: return "\u2A73"; // &Esim
			case 1909: return "\u2242"; // &esim
			case 1911: return "\u0397"; // &Eta
			case 1913: return "\u03B7"; // &eta
			case 1915: return "\u00D0"; // &ETH
			case 1916: return "\u00F0"; // &eth
			case 1919: return "\u00CB"; // &Euml
			case 1922: return "\u00EB"; // &euml
			case 1924: return "\u20AC"; // &euro
			case 1927: return "\u0021"; // &excl
			case 1930: return "\u2203"; // &exist
			case 1935: return "\u2203"; // &Exists
			case 1944: return "\u2130"; // &expectation
			case 1954: return "\u2147"; // &ExponentialE
			case 1963: return "\u2147"; // &exponentiale
			case 1976: return "\u2252"; // &fallingdotseq
			case 1979: return "\u0424"; // &Fcy
			case 1981: return "\u0444"; // &fcy
			case 1986: return "\u2640"; // &female
			case 1991: return "\uFB03"; // &ffilig
			case 1994: return "\uFB00"; // &fflig
			case 1997: return "\uFB04"; // &ffllig
			case 1999: return "\uD835\uDD09"; // &Ffr
			case 2000: return "\uD835\uDD23"; // &ffr
			case 2004: return "\uFB01"; // &filig
			case 2020: return "\u25FC"; // &FilledSmallSquare
			case 2035: return "\u25AA"; // &FilledVerySmallSquare
			case 2039: return "\u0066\u006A"; // &fjlig
			case 2042: return "\u266D"; // &flat
			case 2045: return "\uFB02"; // &fllig
			case 2048: return "\u25B1"; // &fltns
			case 2051: return "\u0192"; // &fnof
			case 2054: return "\uD835\uDD3D"; // &Fopf
			case 2057: return "\uD835\uDD57"; // &fopf
			case 2061: return "\u2200"; // &ForAll
			case 2065: return "\u2200"; // &forall
			case 2066: return "\u22D4"; // &fork
			case 2067: return "\u2AD9"; // &forkv
			case 2075: return "\u2131"; // &Fouriertrf
			case 2082: return "\u2A0D"; // &fpartint
			case 2087: return "\u00BD"; // &frac12
			case 2088: return "\u2153"; // &frac13
			case 2089: return "\u00BC"; // &frac14
			case 2090: return "\u2155"; // &frac15
			case 2091: return "\u2159"; // &frac16
			case 2092: return "\u215B"; // &frac18
			case 2094: return "\u2154"; // &frac23
			case 2095: return "\u2156"; // &frac25
			case 2097: return "\u00BE"; // &frac34
			case 2098: return "\u2157"; // &frac35
			case 2099: return "\u215C"; // &frac38
			case 2101: return "\u2158"; // &frac45
			case 2103: return "\u215A"; // &frac56
			case 2104: return "\u215D"; // &frac58
			case 2106: return "\u215E"; // &frac78
			case 2108: return "\u2044"; // &frasl
			case 2111: return "\u2322"; // &frown
			case 2114: return "\u2131"; // &Fscr
			case 2117: return "\uD835\uDCBB"; // &fscr
			case 2123: return "\u01F5"; // &gacute
			case 2128: return "\u0393"; // &Gamma
			case 2131: return "\u03B3"; // &gamma
			case 2132: return "\u03DC"; // &Gammad
			case 2133: return "\u03DD"; // &gammad
			case 2134: return "\u2A86"; // &gap
			case 2139: return "\u011E"; // &Gbreve
			case 2144: return "\u011F"; // &gbreve
			case 2149: return "\u0122"; // &Gcedil
			case 2152: return "\u011C"; // &Gcirc
			case 2156: return "\u011D"; // &gcirc
			case 2157: return "\u0413"; // &Gcy
			case 2158: return "\u0433"; // &gcy
			case 2161: return "\u0120"; // &Gdot
			case 2164: return "\u0121"; // &gdot
			case 2165: return "\u2267"; // &gE
			case 2166: return "\u2265"; // &ge
			case 2167: return "\u2A8C"; // &gEl
			case 2168: return "\u22DB"; // &gel
			case 2169: return "\u2265"; // &geq
			case 2170: return "\u2267"; // &geqq
			case 2175: return "\u2A7E"; // &geqslant
			case 2176: return "\u2A7E"; // &ges
			case 2178: return "\u2AA9"; // &gescc
			case 2181: return "\u2A80"; // &gesdot
			case 2182: return "\u2A82"; // &gesdoto
			case 2183: return "\u2A84"; // &gesdotol
			case 2184: return "\u22DB\uFE00"; // &gesl
			case 2186: return "\u2A94"; // &gesles
			case 2188: return "\uD835\uDD0A"; // &Gfr
			case 2190: return "\uD835\uDD24"; // &gfr
			case 2191: return "\u22D9"; // &Gg
			case 2192: return "\u226B"; // &gg
			case 2193: return "\u22D9"; // &ggg
			case 2197: return "\u2137"; // &gimel
			case 2200: return "\u0403"; // &GJcy
			case 2203: return "\u0453"; // &gjcy
			case 2204: return "\u2277"; // &gl
			case 2205: return "\u2AA5"; // &gla
			case 2206: return "\u2A92"; // &glE
			case 2207: return "\u2AA4"; // &glj
			case 2210: return "\u2A8A"; // &gnap
			case 2214: return "\u2A8A"; // &gnapprox
			case 2215: return "\u2269"; // &gnE
			case 2216: return "\u2A88"; // &gne
			case 2217: return "\u2A88"; // &gneq
			case 2218: return "\u2269"; // &gneqq
			case 2221: return "\u22E7"; // &gnsim
			case 2224: return "\uD835\uDD3E"; // &Gopf
			case 2227: return "\uD835\uDD58"; // &gopf
			case 2231: return "\u0060"; // &grave
			case 2242: return "\u2265"; // &GreaterEqual
			case 2246: return "\u22DB"; // &GreaterEqualLess
			case 2255: return "\u2267"; // &GreaterFullEqual
			case 2262: return "\u2AA2"; // &GreaterGreater
			case 2266: return "\u2277"; // &GreaterLess
			case 2276: return "\u2A7E"; // &GreaterSlantEqual
			case 2281: return "\u2273"; // &GreaterTilde
			case 2284: return "\uD835\uDCA2"; // &Gscr
			case 2287: return "\u210A"; // &gscr
			case 2289: return "\u2273"; // &gsim
			case 2290: return "\u2A8E"; // &gsime
			case 2291: return "\u2A90"; // &gsiml
			case 2292: return "\u003E"; // &GT
			case 2293: return "\u226B"; // &Gt
			case 2294: return "\u003E"; // &gt
			case 2296: return "\u2AA7"; // &gtcc
			case 2298: return "\u2A7A"; // &gtcir
			case 2301: return "\u22D7"; // &gtdot
			case 2305: return "\u2995"; // &gtlPar
			case 2310: return "\u2A7C"; // &gtquest
			case 2317: return "\u2A86"; // &gtrapprox
			case 2319: return "\u2978"; // &gtrarr
			case 2322: return "\u22D7"; // &gtrdot
			case 2328: return "\u22DB"; // &gtreqless
			case 2333: return "\u2A8C"; // &gtreqqless
			case 2337: return "\u2277"; // &gtrless
			case 2340: return "\u2273"; // &gtrsim
			case 2348: return "\u2269\uFE00"; // &gvertneqq
			case 2350: return "\u2269\uFE00"; // &gvnE
			case 2355: return "\u02C7"; // &Hacek
			case 2361: return "\u200A"; // &hairsp
			case 2363: return "\u00BD"; // &half
			case 2367: return "\u210B"; // &hamilt
			case 2372: return "\u042A"; // &HARDcy
			case 2376: return "\u044A"; // &hardcy
			case 2379: return "\u21D4"; // &hArr
			case 2380: return "\u2194"; // &harr
			case 2383: return "\u2948"; // &harrcir
			case 2384: return "\u21AD"; // &harrw
			case 2385: return "\u005E"; // &Hat
			case 2388: return "\u210F"; // &hbar
			case 2392: return "\u0124"; // &Hcirc
			case 2396: return "\u0125"; // &hcirc
			case 2401: return "\u2665"; // &hearts
			case 2404: return "\u2665"; // &heartsuit
			case 2408: return "\u2026"; // &hellip
			case 2412: return "\u22B9"; // &hercon
			case 2414: return "\u210C"; // &Hfr
			case 2416: return "\uD835\uDD25"; // &hfr
			case 2427: return "\u210B"; // &HilbertSpace
			case 2434: return "\u2925"; // &hksearow
			case 2439: return "\u2926"; // &hkswarow
			case 2443: return "\u21FF"; // &hoarr
			case 2447: return "\u223B"; // &homtht
			case 2458: return "\u21A9"; // &hookleftarrow
			case 2468: return "\u21AA"; // &hookrightarrow
			case 2471: return "\u210D"; // &Hopf
			case 2473: return "\uD835\uDD59"; // &hopf
			case 2477: return "\u2015"; // &horbar
			case 2489: return "\u2500"; // &HorizontalLine
			case 2492: return "\u210B"; // &Hscr
			case 2495: return "\uD835\uDCBD"; // &hscr
			case 2499: return "\u210F"; // &hslash
			case 2503: return "\u0126"; // &Hstrok
			case 2507: return "\u0127"; // &hstrok
			case 2518: return "\u224E"; // &HumpDownHump
			case 2523: return "\u224F"; // &HumpEqual
			case 2528: return "\u2043"; // &hybull
			case 2532: return "\u2010"; // &hyphen
			case 2538: return "\u00CD"; // &Iacute
			case 2544: return "\u00ED"; // &iacute
			case 2545: return "\u2063"; // &ic
			case 2549: return "\u00CE"; // &Icirc
			case 2552: return "\u00EE"; // &icirc
			case 2553: return "\u0418"; // &Icy
			case 2554: return "\u0438"; // &icy
			case 2557: return "\u0130"; // &Idot
			case 2560: return "\u0415"; // &IEcy
			case 2563: return "\u0435"; // &iecy
			case 2566: return "\u00A1"; // &iexcl
			case 2568: return "\u21D4"; // &iff
			case 2570: return "\u2111"; // &Ifr
			case 2571: return "\uD835\uDD26"; // &ifr
			case 2576: return "\u00CC"; // &Igrave
			case 2581: return "\u00EC"; // &igrave
			case 2582: return "\u2148"; // &ii
			case 2586: return "\u2A0C"; // &iiiint
			case 2588: return "\u222D"; // &iiint
			case 2592: return "\u29DC"; // &iinfin
			case 2595: return "\u2129"; // &iiota
			case 2599: return "\u0132"; // &IJlig
			case 2603: return "\u0133"; // &ijlig
			case 2604: return "\u2111"; // &Im
			case 2607: return "\u012A"; // &Imacr
			case 2611: return "\u012B"; // &imacr
			case 2613: return "\u2111"; // &image
			case 2620: return "\u2148"; // &ImaginaryI
			case 2624: return "\u2110"; // &imagline
			case 2628: return "\u2111"; // &imagpart
			case 2630: return "\u0131"; // &imath
			case 2632: return "\u22B7"; // &imof
			case 2635: return "\u01B5"; // &imped
			case 2640: return "\u21D2"; // &Implies
			case 2641: return "\u2208"; // &in
			case 2645: return "\u2105"; // &incare
			case 2648: return "\u221E"; // &infin
			case 2651: return "\u29DD"; // &infintie
			case 2655: return "\u0131"; // &inodot
			case 2657: return "\u222C"; // &Int
			case 2658: return "\u222B"; // &int
			case 2661: return "\u22BA"; // &intcal
			case 2666: return "\u2124"; // &integers
			case 2671: return "\u222B"; // &Integral
			case 2675: return "\u22BA"; // &intercal
			case 2683: return "\u22C2"; // &Intersection
			case 2688: return "\u2A17"; // &intlarhk
			case 2692: return "\u2A3C"; // &intprod
			case 2704: return "\u2063"; // &InvisibleComma
			case 2709: return "\u2062"; // &InvisibleTimes
			case 2712: return "\u0401"; // &IOcy
			case 2715: return "\u0451"; // &iocy
			case 2719: return "\u012E"; // &Iogon
			case 2722: return "\u012F"; // &iogon
			case 2724: return "\uD835\uDD40"; // &Iopf
			case 2726: return "\uD835\uDD5A"; // &iopf
			case 2728: return "\u0399"; // &Iota
			case 2730: return "\u03B9"; // &iota
			case 2734: return "\u2A3C"; // &iprod
			case 2739: return "\u00BF"; // &iquest
			case 2742: return "\u2110"; // &Iscr
			case 2745: return "\uD835\uDCBE"; // &iscr
			case 2747: return "\u2208"; // &isin
			case 2750: return "\u22F5"; // &isindot
			case 2751: return "\u22F9"; // &isinE
			case 2752: return "\u22F4"; // &isins
			case 2753: return "\u22F3"; // &isinsv
			case 2754: return "\u2208"; // &isinv
			case 2755: return "\u2062"; // &it
			case 2760: return "\u0128"; // &Itilde
			case 2764: return "\u0129"; // &itilde
			case 2768: return "\u0406"; // &Iukcy
			case 2772: return "\u0456"; // &iukcy
			case 2774: return "\u00CF"; // &Iuml
			case 2776: return "\u00EF"; // &iuml
			case 2781: return "\u0134"; // &Jcirc
			case 2786: return "\u0135"; // &jcirc
			case 2787: return "\u0419"; // &Jcy
			case 2788: return "\u0439"; // &jcy
			case 2790: return "\uD835\uDD0D"; // &Jfr
			case 2792: return "\uD835\uDD27"; // &jfr
			case 2796: return "\u0237"; // &jmath
			case 2799: return "\uD835\uDD41"; // &Jopf
			case 2802: return "\uD835\uDD5B"; // &jopf
			case 2805: return "\uD835\uDCA5"; // &Jscr
			case 2808: return "\uD835\uDCBF"; // &jscr
			case 2812: return "\u0408"; // &Jsercy
			case 2816: return "\u0458"; // &jsercy
			case 2820: return "\u0404"; // &Jukcy
			case 2824: return "\u0454"; // &jukcy
			case 2829: return "\u039A"; // &Kappa
			case 2834: return "\u03BA"; // &kappa
			case 2835: return "\u03F0"; // &kappav
			case 2840: return "\u0136"; // &Kcedil
			case 2845: return "\u0137"; // &kcedil
			case 2846: return "\u041A"; // &Kcy
			case 2847: return "\u043A"; // &kcy
			case 2849: return "\uD835\uDD0E"; // &Kfr
			case 2851: return "\uD835\uDD28"; // &kfr
			case 2856: return "\u0138"; // &kgreen
			case 2859: return "\u0425"; // &KHcy
			case 2862: return "\u0445"; // &khcy
			case 2865: return "\u040C"; // &KJcy
			case 2868: return "\u045C"; // &kjcy
			case 2871: return "\uD835\uDD42"; // &Kopf
			case 2874: return "\uD835\uDD5C"; // &kopf
			case 2877: return "\uD835\uDCA6"; // &Kscr
			case 2880: return "\uD835\uDCC0"; // &kscr
			case 2885: return "\u21DA"; // &lAarr
			case 2891: return "\u0139"; // &Lacute
			case 2896: return "\u013A"; // &lacute
			case 2902: return "\u29B4"; // &laemptyv
			case 2906: return "\u2112"; // &lagran
			case 2910: return "\u039B"; // &Lambda
			case 2914: return "\u03BB"; // &lambda
			case 2916: return "\u27EA"; // &Lang
			case 2918: return "\u27E8"; // &lang
			case 2919: return "\u2991"; // &langd
			case 2921: return "\u27E8"; // &langle
			case 2922: return "\u2A85"; // &lap
			case 2930: return "\u2112"; // &Laplacetrf
			case 2933: return "\u00AB"; // &laquo
			case 2935: return "\u219E"; // &Larr
			case 2937: return "\u21D0"; // &lArr
			case 2939: return "\u2190"; // &larr
			case 2940: return "\u21E4"; // &larrb
			case 2942: return "\u291F"; // &larrbfs
			case 2944: return "\u291D"; // &larrfs
			case 2946: return "\u21A9"; // &larrhk
			case 2948: return "\u21AB"; // &larrlp
			case 2950: return "\u2939"; // &larrpl
			case 2953: return "\u2973"; // &larrsim
			case 2955: return "\u21A2"; // &larrtl
			case 2956: return "\u2AAB"; // &lat
			case 2960: return "\u291B"; // &lAtail
			case 2963: return "\u2919"; // &latail
			case 2964: return "\u2AAD"; // &late
			case 2965: return "\u2AAD\uFE00"; // &lates
			case 2969: return "\u290E"; // &lBarr
			case 2973: return "\u290C"; // &lbarr
			case 2976: return "\u2772"; // &lbbrk
			case 2980: return "\u007B"; // &lbrace
			case 2981: return "\u005B"; // &lbrack
			case 2983: return "\u298B"; // &lbrke
			case 2986: return "\u298F"; // &lbrksld
			case 2987: return "\u298D"; // &lbrkslu
			case 2992: return "\u013D"; // &Lcaron
			case 2997: return "\u013E"; // &lcaron
			case 3001: return "\u013B"; // &Lcedil
			case 3005: return "\u013C"; // &lcedil
			case 3007: return "\u2308"; // &lceil
			case 3009: return "\u007B"; // &lcub
			case 3010: return "\u041B"; // &Lcy
			case 3011: return "\u043B"; // &lcy
			case 3014: return "\u2936"; // &ldca
			case 3017: return "\u201C"; // &ldquo
			case 3018: return "\u201E"; // &ldquor
			case 3023: return "\u2967"; // &ldrdhar
			case 3028: return "\u294B"; // &ldrushar
			case 3030: return "\u21B2"; // &ldsh
			case 3031: return "\u2266"; // &lE
			case 3032: return "\u2264"; // &le
			case 3047: return "\u27E8"; // &LeftAngleBracket
			case 3051: return "\u2190"; // &LeftArrow
			case 3056: return "\u21D0"; // &Leftarrow
			case 3063: return "\u2190"; // &leftarrow
			case 3066: return "\u21E4"; // &LeftArrowBar
			case 3076: return "\u21C6"; // &LeftArrowRightArrow
			case 3080: return "\u21A2"; // &leftarrowtail
			case 3087: return "\u2308"; // &LeftCeiling
			case 3100: return "\u27E6"; // &LeftDoubleBracket
			case 3111: return "\u2961"; // &LeftDownTeeVector
			case 3117: return "\u21C3"; // &LeftDownVector
			case 3120: return "\u2959"; // &LeftDownVectorBar
			case 3125: return "\u230A"; // &LeftFloor
			case 3136: return "\u21BD"; // &leftharpoondown
			case 3138: return "\u21BC"; // &leftharpoonup
			case 3148: return "\u21C7"; // &leftleftarrows
			case 3158: return "\u2194"; // &LeftRightArrow
			case 3168: return "\u21D4"; // &Leftrightarrow
			case 3178: return "\u2194"; // &leftrightarrow
			case 3179: return "\u21C6"; // &leftrightarrows
			case 3187: return "\u21CB"; // &leftrightharpoons
			case 3197: return "\u21AD"; // &leftrightsquigarrow
			case 3203: return "\u294E"; // &LeftRightVector
			case 3206: return "\u22A3"; // &LeftTee
			case 3211: return "\u21A4"; // &LeftTeeArrow
			case 3217: return "\u295A"; // &LeftTeeVector
			case 3227: return "\u22CB"; // &leftthreetimes
			case 3234: return "\u22B2"; // &LeftTriangle
			case 3237: return "\u29CF"; // &LeftTriangleBar
			case 3242: return "\u22B4"; // &LeftTriangleEqual
			case 3254: return "\u2951"; // &LeftUpDownVector
			case 3263: return "\u2960"; // &LeftUpTeeVector
			case 3269: return "\u21BF"; // &LeftUpVector
			case 3272: return "\u2958"; // &LeftUpVectorBar
			case 3278: return "\u21BC"; // &LeftVector
			case 3281: return "\u2952"; // &LeftVectorBar
			case 3282: return "\u2A8B"; // &lEg
			case 3283: return "\u22DA"; // &leg
			case 3284: return "\u2264"; // &leq
			case 3285: return "\u2266"; // &leqq
			case 3290: return "\u2A7D"; // &leqslant
			case 3291: return "\u2A7D"; // &les
			case 3293: return "\u2AA8"; // &lescc
			case 3296: return "\u2A7F"; // &lesdot
			case 3297: return "\u2A81"; // &lesdoto
			case 3298: return "\u2A83"; // &lesdotor
			case 3299: return "\u22DA\uFE00"; // &lesg
			case 3301: return "\u2A93"; // &lesges
			case 3308: return "\u2A85"; // &lessapprox
			case 3311: return "\u22D6"; // &lessdot
			case 3316: return "\u22DA"; // &lesseqgtr
			case 3320: return "\u2A8B"; // &lesseqqgtr
			case 3334: return "\u22DA"; // &LessEqualGreater
			case 3343: return "\u2266"; // &LessFullEqual
			case 3350: return "\u2276"; // &LessGreater
			case 3353: return "\u2276"; // &lessgtr
			case 3357: return "\u2AA1"; // &LessLess
			case 3360: return "\u2272"; // &lesssim
			case 3370: return "\u2A7D"; // &LessSlantEqual
			case 3375: return "\u2272"; // &LessTilde
			case 3380: return "\u297C"; // &lfisht
			case 3384: return "\u230A"; // &lfloor
			case 3386: return "\uD835\uDD0F"; // &Lfr
			case 3387: return "\uD835\uDD29"; // &lfr
			case 3388: return "\u2276"; // &lg
			case 3389: return "\u2A91"; // &lgE
			case 3392: return "\u2962"; // &lHar
			case 3396: return "\u21BD"; // &lhard
			case 3397: return "\u21BC"; // &lharu
			case 3398: return "\u296A"; // &lharul
			case 3401: return "\u2584"; // &lhblk
			case 3404: return "\u0409"; // &LJcy
			case 3407: return "\u0459"; // &ljcy
			case 3408: return "\u22D8"; // &Ll
			case 3409: return "\u226A"; // &ll
			case 3412: return "\u21C7"; // &llarr
			case 3418: return "\u231E"; // &llcorner
			case 3426: return "\u21DA"; // &Lleftarrow
			case 3430: return "\u296B"; // &llhard
			case 3433: return "\u25FA"; // &lltri
			case 3438: return "\u013F"; // &Lmidot
			case 3443: return "\u0140"; // &lmidot
			case 3447: return "\u23B0"; // &lmoust
			case 3451: return "\u23B0"; // &lmoustache
			case 3454: return "\u2A89"; // &lnap
			case 3458: return "\u2A89"; // &lnapprox
			case 3459: return "\u2268"; // &lnE
			case 3460: return "\u2A87"; // &lne
			case 3461: return "\u2A87"; // &lneq
			case 3462: return "\u2268"; // &lneqq
			case 3465: return "\u22E6"; // &lnsim
			case 3469: return "\u27EC"; // &loang
			case 3471: return "\u21FD"; // &loarr
			case 3474: return "\u27E6"; // &lobrk
			case 3486: return "\u27F5"; // &LongLeftArrow
			case 3495: return "\u27F8"; // &Longleftarrow
			case 3506: return "\u27F5"; // &longleftarrow
			case 3516: return "\u27F7"; // &LongLeftRightArrow
			case 3526: return "\u27FA"; // &Longleftrightarrow
			case 3536: return "\u27F7"; // &longleftrightarrow
			case 3542: return "\u27FC"; // &longmapsto
			case 3552: return "\u27F6"; // &LongRightArrow
			case 3562: return "\u27F9"; // &Longrightarrow
			case 3572: return "\u27F6"; // &longrightarrow
			case 3583: return "\u21AB"; // &looparrowleft
			case 3588: return "\u21AC"; // &looparrowright
			case 3591: return "\u2985"; // &lopar
			case 3593: return "\uD835\uDD43"; // &Lopf
			case 3594: return "\uD835\uDD5D"; // &lopf
			case 3597: return "\u2A2D"; // &loplus
			case 3602: return "\u2A34"; // &lotimes
			case 3606: return "\u2217"; // &lowast
			case 3609: return "\u005F"; // &lowbar
			case 3621: return "\u2199"; // &LowerLeftArrow
			case 3631: return "\u2198"; // &LowerRightArrow
			case 3632: return "\u25CA"; // &loz
			case 3636: return "\u25CA"; // &lozenge
			case 3637: return "\u29EB"; // &lozf
			case 3640: return "\u0028"; // &lpar
			case 3642: return "\u2993"; // &lparlt
			case 3646: return "\u21C6"; // &lrarr
			case 3652: return "\u231F"; // &lrcorner
			case 3655: return "\u21CB"; // &lrhar
			case 3656: return "\u296D"; // &lrhard
			case 3657: return "\u200E"; // &lrm
			case 3660: return "\u22BF"; // &lrtri
			case 3665: return "\u2039"; // &lsaquo
			case 3668: return "\u2112"; // &Lscr
			case 3670: return "\uD835\uDCC1"; // &lscr
			case 3671: return "\u21B0"; // &Lsh
			case 3672: return "\u21B0"; // &lsh
			case 3674: return "\u2272"; // &lsim
			case 3675: return "\u2A8D"; // &lsime
			case 3676: return "\u2A8F"; // &lsimg
			case 3678: return "\u005B"; // &lsqb
			case 3680: return "\u2018"; // &lsquo
			case 3681: return "\u201A"; // &lsquor
			case 3685: return "\u0141"; // &Lstrok
			case 3689: return "\u0142"; // &lstrok
			case 3690: return "\u003C"; // &LT
			case 3691: return "\u226A"; // &Lt
			case 3692: return "\u003C"; // &lt
			case 3694: return "\u2AA6"; // &ltcc
			case 3696: return "\u2A79"; // &ltcir
			case 3699: return "\u22D6"; // &ltdot
			case 3703: return "\u22CB"; // &lthree
			case 3707: return "\u22C9"; // &ltimes
			case 3711: return "\u2976"; // &ltlarr
			case 3716: return "\u2A7B"; // &ltquest
			case 3718: return "\u25C3"; // &ltri
			case 3719: return "\u22B4"; // &ltrie
			case 3720: return "\u25C2"; // &ltrif
			case 3723: return "\u2996"; // &ltrPar
			case 3730: return "\u294A"; // &lurdshar
			case 3734: return "\u2966"; // &luruhar
			case 3742: return "\u2268\uFE00"; // &lvertneqq
			case 3744: return "\u2268\uFE00"; // &lvnE
			case 3748: return "\u00AF"; // &macr
			case 3750: return "\u2642"; // &male
			case 3751: return "\u2720"; // &malt
			case 3754: return "\u2720"; // &maltese
			case 3757: return "\u2905"; // &Map
			case 3758: return "\u21A6"; // &map
			case 3761: return "\u21A6"; // &mapsto
			case 3765: return "\u21A7"; // &mapstodown
			case 3769: return "\u21A4"; // &mapstoleft
			case 3771: return "\u21A5"; // &mapstoup
			case 3775: return "\u25AE"; // &marker
			case 3780: return "\u2A29"; // &mcomma
			case 3782: return "\u041C"; // &Mcy
			case 3783: return "\u043C"; // &mcy
			case 3787: return "\u2014"; // &mdash
			case 3791: return "\u223A"; // &mDDot
			case 3803: return "\u2221"; // &measuredangle
			case 3813: return "\u205F"; // &MediumSpace
			case 3820: return "\u2133"; // &Mellintrf
			case 3822: return "\uD835\uDD10"; // &Mfr
			case 3824: return "\uD835\uDD2A"; // &mfr
			case 3826: return "\u2127"; // &mho
			case 3830: return "\u00B5"; // &micro
			case 3831: return "\u2223"; // &mid
			case 3834: return "\u002A"; // &midast
			case 3837: return "\u2AF0"; // &midcir
			case 3840: return "\u00B7"; // &middot
			case 3843: return "\u2212"; // &minus
			case 3844: return "\u229F"; // &minusb
			case 3845: return "\u2238"; // &minusd
			case 3846: return "\u2A2A"; // &minusdu
			case 3854: return "\u2213"; // &MinusPlus
			case 3857: return "\u2ADB"; // &mlcp
			case 3859: return "\u2026"; // &mldr
			case 3864: return "\u2213"; // &mnplus
			case 3869: return "\u22A7"; // &models
			case 3872: return "\uD835\uDD44"; // &Mopf
			case 3874: return "\uD835\uDD5E"; // &mopf
			case 3875: return "\u2213"; // &mp
			case 3878: return "\u2133"; // &Mscr
			case 3881: return "\uD835\uDCC2"; // &mscr
			case 3885: return "\u223E"; // &mstpos
			case 3886: return "\u039C"; // &Mu
			case 3887: return "\u03BC"; // &mu
			case 3893: return "\u22B8"; // &multimap
			case 3896: return "\u22B8"; // &mumap
			case 3901: return "\u2207"; // &nabla
			case 3907: return "\u0143"; // &Nacute
			case 3911: return "\u0144"; // &nacute
			case 3913: return "\u2220\u20D2"; // &nang
			case 3914: return "\u2249"; // &nap
			case 3915: return "\u2A70\u0338"; // &napE
			case 3917: return "\u224B\u0338"; // &napid
			case 3919: return "\u0149"; // &napos
			case 3923: return "\u2249"; // &napprox
			case 3926: return "\u266E"; // &natur
			case 3928: return "\u266E"; // &natural
			case 3929: return "\u2115"; // &naturals
			case 3932: return "\u00A0"; // &nbsp
			case 3935: return "\u224E\u0338"; // &nbump
			case 3936: return "\u224F\u0338"; // &nbumpe
			case 3939: return "\u2A43"; // &ncap
			case 3944: return "\u0147"; // &Ncaron
			case 3947: return "\u0148"; // &ncaron
			case 3951: return "\u0145"; // &Ncedil
			case 3955: return "\u0146"; // &ncedil
			case 3958: return "\u2247"; // &ncong
			case 3961: return "\u2A6D\u0338"; // &ncongdot
			case 3963: return "\u2A42"; // &ncup
			case 3964: return "\u041D"; // &Ncy
			case 3965: return "\u043D"; // &ncy
			case 3969: return "\u2013"; // &ndash
			case 3970: return "\u2260"; // &ne
			case 3974: return "\u2924"; // &nearhk
			case 3977: return "\u21D7"; // &neArr
			case 3978: return "\u2197"; // &nearr
			case 3980: return "\u2197"; // &nearrow
			case 3983: return "\u2250\u0338"; // &nedot
			case 4001: return "\u200B"; // &NegativeMediumSpace
			case 4011: return "\u200B"; // &NegativeThickSpace
			case 4017: return "\u200B"; // &NegativeThinSpace
			case 4030: return "\u200B"; // &NegativeVeryThinSpace
			case 4034: return "\u2262"; // &nequiv
			case 4038: return "\u2928"; // &nesear
			case 4040: return "\u2242\u0338"; // &nesim
			case 4058: return "\u226B"; // &NestedGreaterGreater
			case 4066: return "\u226A"; // &NestedLessLess
			case 4071: return "\u000A"; // &NewLine
			case 4075: return "\u2204"; // &nexist
			case 4076: return "\u2204"; // &nexists
			case 4078: return "\uD835\uDD11"; // &Nfr
			case 4080: return "\uD835\uDD2B"; // &nfr
			case 4082: return "\u2267\u0338"; // &ngE
			case 4083: return "\u2271"; // &nge
			case 4084: return "\u2271"; // &ngeq
			case 4085: return "\u2267\u0338"; // &ngeqq
			case 4090: return "\u2A7E\u0338"; // &ngeqslant
			case 4091: return "\u2A7E\u0338"; // &nges
			case 4093: return "\u22D9\u0338"; // &nGg
			case 4096: return "\u2275"; // &ngsim
			case 4097: return "\u226B\u20D2"; // &nGt
			case 4098: return "\u226F"; // &ngt
			case 4099: return "\u226F"; // &ngtr
			case 4100: return "\u226B\u0338"; // &nGtv
			case 4104: return "\u21CE"; // &nhArr
			case 4107: return "\u21AE"; // &nharr
			case 4110: return "\u2AF2"; // &nhpar
			case 4111: return "\u220B"; // &ni
			case 4112: return "\u22FC"; // &nis
			case 4113: return "\u22FA"; // &nisd
			case 4114: return "\u220B"; // &niv
			case 4117: return "\u040A"; // &NJcy
			case 4120: return "\u045A"; // &njcy
			case 4124: return "\u21CD"; // &nlArr
			case 4127: return "\u219A"; // &nlarr
			case 4129: return "\u2025"; // &nldr
			case 4130: return "\u2266\u0338"; // &nlE
			case 4131: return "\u2270"; // &nle
			case 4140: return "\u21CD"; // &nLeftarrow
			case 4147: return "\u219A"; // &nleftarrow
			case 4157: return "\u21CE"; // &nLeftrightarrow
			case 4167: return "\u21AE"; // &nleftrightarrow
			case 4168: return "\u2270"; // &nleq
			case 4169: return "\u2266\u0338"; // &nleqq
			case 4174: return "\u2A7D\u0338"; // &nleqslant
			case 4175: return "\u2A7D\u0338"; // &nles
			case 4176: return "\u226E"; // &nless
			case 4177: return "\u22D8\u0338"; // &nLl
			case 4180: return "\u2274"; // &nlsim
			case 4181: return "\u226A\u20D2"; // &nLt
			case 4182: return "\u226E"; // &nlt
			case 4184: return "\u22EA"; // &nltri
			case 4185: return "\u22EC"; // &nltrie
			case 4186: return "\u226A\u0338"; // &nLtv
			case 4189: return "\u2224"; // &nmid
			case 4195: return "\u2060"; // &NoBreak
			case 4209: return "\u00A0"; // &NonBreakingSpace
			case 4211: return "\u2115"; // &Nopf
			case 4214: return "\uD835\uDD5F"; // &nopf
			case 4215: return "\u2AEC"; // &Not
			case 4216: return "\u00AC"; // &not
			case 4225: return "\u2262"; // &NotCongruent
			case 4230: return "\u226D"; // &NotCupCap
			case 4247: return "\u2226"; // &NotDoubleVerticalBar
			case 4254: return "\u2209"; // &NotElement
			case 4258: return "\u2260"; // &NotEqual
			case 4263: return "\u2242\u0338"; // &NotEqualTilde
			case 4268: return "\u2204"; // &NotExists
			case 4275: return "\u226F"; // &NotGreater
			case 4280: return "\u2271"; // &NotGreaterEqual
			case 4289: return "\u2267\u0338"; // &NotGreaterFullEqual
			case 4296: return "\u226B\u0338"; // &NotGreaterGreater
			case 4300: return "\u2279"; // &NotGreaterLess
			case 4310: return "\u2A7E\u0338"; // &NotGreaterSlantEqual
			case 4315: return "\u2275"; // &NotGreaterTilde
			case 4327: return "\u224E\u0338"; // &NotHumpDownHump
			case 4332: return "\u224F\u0338"; // &NotHumpEqual
			case 4334: return "\u2209"; // &notin
			case 4337: return "\u22F5\u0338"; // &notindot
			case 4338: return "\u22F9\u0338"; // &notinE
			case 4340: return "\u2209"; // &notinva
			case 4341: return "\u22F7"; // &notinvb
			case 4342: return "\u22F6"; // &notinvc
			case 4354: return "\u22EA"; // &NotLeftTriangle
			case 4357: return "\u29CF\u0338"; // &NotLeftTriangleBar
			case 4362: return "\u22EC"; // &NotLeftTriangleEqual
			case 4364: return "\u226E"; // &NotLess
			case 4369: return "\u2270"; // &NotLessEqual
			case 4376: return "\u2278"; // &NotLessGreater
			case 4380: return "\u226A\u0338"; // &NotLessLess
			case 4390: return "\u2A7D\u0338"; // &NotLessSlantEqual
			case 4395: return "\u2274"; // &NotLessTilde
			case 4415: return "\u2AA2\u0338"; // &NotNestedGreaterGreater
			case 4423: return "\u2AA1\u0338"; // &NotNestedLessLess
			case 4425: return "\u220C"; // &notni
			case 4427: return "\u220C"; // &notniva
			case 4428: return "\u22FE"; // &notnivb
			case 4429: return "\u22FD"; // &notnivc
			case 4437: return "\u2280"; // &NotPrecedes
			case 4442: return "\u2AAF\u0338"; // &NotPrecedesEqual
			case 4452: return "\u22E0"; // &NotPrecedesSlantEqual
			case 4466: return "\u220C"; // &NotReverseElement
			case 4478: return "\u22EB"; // &NotRightTriangle
			case 4481: return "\u29D0\u0338"; // &NotRightTriangleBar
			case 4486: return "\u22ED"; // &NotRightTriangleEqual
			case 4498: return "\u228F\u0338"; // &NotSquareSubset
			case 4503: return "\u22E2"; // &NotSquareSubsetEqual
			case 4509: return "\u2290\u0338"; // &NotSquareSuperset
			case 4514: return "\u22E3"; // &NotSquareSupersetEqual
			case 4519: return "\u2282\u20D2"; // &NotSubset
			case 4524: return "\u2288"; // &NotSubsetEqual
			case 4530: return "\u2281"; // &NotSucceeds
			case 4535: return "\u2AB0\u0338"; // &NotSucceedsEqual
			case 4545: return "\u22E1"; // &NotSucceedsSlantEqual
			case 4550: return "\u227F\u0338"; // &NotSucceedsTilde
			case 4556: return "\u2283\u20D2"; // &NotSuperset
			case 4561: return "\u2289"; // &NotSupersetEqual
			case 4566: return "\u2241"; // &NotTilde
			case 4571: return "\u2244"; // &NotTildeEqual
			case 4580: return "\u2247"; // &NotTildeFullEqual
			case 4585: return "\u2249"; // &NotTildeTilde
			case 4596: return "\u2224"; // &NotVerticalBar
			case 4599: return "\u2226"; // &npar
			case 4604: return "\u2226"; // &nparallel
			case 4606: return "\u2AFD\u20E5"; // &nparsl
			case 4607: return "\u2202\u0338"; // &npart
			case 4612: return "\u2A14"; // &npolint
			case 4613: return "\u2280"; // &npr
			case 4616: return "\u22E0"; // &nprcue
			case 4617: return "\u2AAF\u0338"; // &npre
			case 4618: return "\u2280"; // &nprec
			case 4620: return "\u2AAF\u0338"; // &npreceq
			case 4624: return "\u21CF"; // &nrArr
			case 4627: return "\u219B"; // &nrarr
			case 4628: return "\u2933\u0338"; // &nrarrc
			case 4629: return "\u219D\u0338"; // &nrarrw
			case 4639: return "\u21CF"; // &nRightarrow
			case 4648: return "\u219B"; // &nrightarrow
			case 4651: return "\u22EB"; // &nrtri
			case 4652: return "\u22ED"; // &nrtrie
			case 4654: return "\u2281"; // &nsc
			case 4657: return "\u22E1"; // &nsccue
			case 4658: return "\u2AB0\u0338"; // &nsce
			case 4661: return "\uD835\uDCA9"; // &Nscr
			case 4662: return "\uD835\uDCC3"; // &nscr
			case 4669: return "\u2224"; // &nshortmid
			case 4677: return "\u2226"; // &nshortparallel
			case 4679: return "\u2241"; // &nsim
			case 4680: return "\u2244"; // &nsime
			case 4681: return "\u2244"; // &nsimeq
			case 4684: return "\u2224"; // &nsmid
			case 4687: return "\u2226"; // &nspar
			case 4692: return "\u22E2"; // &nsqsube
			case 4694: return "\u22E3"; // &nsqsupe
			case 4696: return "\u2284"; // &nsub
			case 4697: return "\u2AC5\u0338"; // &nsubE
			case 4698: return "\u2288"; // &nsube
			case 4701: return "\u2282\u20D2"; // &nsubset
			case 4703: return "\u2288"; // &nsubseteq
			case 4704: return "\u2AC5\u0338"; // &nsubseteqq
			case 4706: return "\u2281"; // &nsucc
			case 4708: return "\u2AB0\u0338"; // &nsucceq
			case 4709: return "\u2285"; // &nsup
			case 4710: return "\u2AC6\u0338"; // &nsupE
			case 4711: return "\u2289"; // &nsupe
			case 4714: return "\u2283\u20D2"; // &nsupset
			case 4716: return "\u2289"; // &nsupseteq
			case 4717: return "\u2AC6\u0338"; // &nsupseteqq
			case 4720: return "\u2279"; // &ntgl
			case 4725: return "\u00D1"; // &Ntilde
			case 4729: return "\u00F1"; // &ntilde
			case 4731: return "\u2278"; // &ntlg
			case 4742: return "\u22EA"; // &ntriangleleft
			case 4744: return "\u22EC"; // &ntrianglelefteq
			case 4749: return "\u22EB"; // &ntriangleright
			case 4751: return "\u22ED"; // &ntrianglerighteq
			case 4752: return "\u039D"; // &Nu
			case 4753: return "\u03BD"; // &nu
			case 4754: return "\u0023"; // &num
			case 4757: return "\u2116"; // &numero
			case 4759: return "\u2007"; // &numsp
			case 4762: return "\u224D\u20D2"; // &nvap
			case 4767: return "\u22AF"; // &nVDash
			case 4771: return "\u22AE"; // &nVdash
			case 4775: return "\u22AD"; // &nvDash
			case 4779: return "\u22AC"; // &nvdash
			case 4781: return "\u2265\u20D2"; // &nvge
			case 4782: return "\u003E\u20D2"; // &nvgt
			case 4786: return "\u2904"; // &nvHarr
			case 4791: return "\u29DE"; // &nvinfin
			case 4795: return "\u2902"; // &nvlArr
			case 4796: return "\u2264\u20D2"; // &nvle
			case 4797: return "\u003C\u20D2"; // &nvlt
			case 4800: return "\u22B4\u20D2"; // &nvltrie
			case 4804: return "\u2903"; // &nvrArr
			case 4808: return "\u22B5\u20D2"; // &nvrtrie
			case 4811: return "\u223C\u20D2"; // &nvsim
			case 4816: return "\u2923"; // &nwarhk
			case 4819: return "\u21D6"; // &nwArr
			case 4820: return "\u2196"; // &nwarr
			case 4822: return "\u2196"; // &nwarrow
			case 4826: return "\u2927"; // &nwnear
			case 4832: return "\u00D3"; // &Oacute
			case 4838: return "\u00F3"; // &oacute
			case 4840: return "\u229B"; // &oast
			case 4843: return "\u229A"; // &ocir
			case 4847: return "\u00D4"; // &Ocirc
			case 4848: return "\u00F4"; // &ocirc
			case 4849: return "\u041E"; // &Ocy
			case 4850: return "\u043E"; // &ocy
			case 4854: return "\u229D"; // &odash
			case 4859: return "\u0150"; // &Odblac
			case 4863: return "\u0151"; // &odblac
			case 4865: return "\u2A38"; // &odiv
			case 4867: return "\u2299"; // &odot
			case 4871: return "\u29BC"; // &odsold
			case 4875: return "\u0152"; // &OElig
			case 4879: return "\u0153"; // &oelig
			case 4883: return "\u29BF"; // &ofcir
			case 4885: return "\uD835\uDD12"; // &Ofr
			case 4886: return "\uD835\uDD2C"; // &ofr
			case 4889: return "\u02DB"; // &ogon
			case 4894: return "\u00D2"; // &Ograve
			case 4898: return "\u00F2"; // &ograve
			case 4899: return "\u29C1"; // &ogt
			case 4903: return "\u29B5"; // &ohbar
			case 4904: return "\u03A9"; // &ohm
			case 4907: return "\u222E"; // &oint
			case 4911: return "\u21BA"; // &olarr
			case 4914: return "\u29BE"; // &olcir
			case 4918: return "\u29BB"; // &olcross
			case 4921: return "\u203E"; // &oline
			case 4922: return "\u29C0"; // &olt
			case 4926: return "\u014C"; // &Omacr
			case 4930: return "\u014D"; // &omacr
			case 4933: return "\u03A9"; // &Omega
			case 4936: return "\u03C9"; // &omega
			case 4941: return "\u039F"; // &Omicron
			case 4946: return "\u03BF"; // &omicron
			case 4947: return "\u29B6"; // &omid
			case 4950: return "\u2296"; // &ominus
			case 4953: return "\uD835\uDD46"; // &Oopf
			case 4956: return "\uD835\uDD60"; // &oopf
			case 4959: return "\u29B7"; // &opar
			case 4978: return "\u201C"; // &OpenCurlyDoubleQuote
			case 4983: return "\u2018"; // &OpenCurlyQuote
			case 4986: return "\u29B9"; // &operp
			case 4989: return "\u2295"; // &oplus
			case 4990: return "\u2A54"; // &Or
			case 4991: return "\u2228"; // &or
			case 4994: return "\u21BB"; // &orarr
			case 4995: return "\u2A5D"; // &ord
			case 4997: return "\u2134"; // &order
			case 4999: return "\u2134"; // &orderof
			case 5000: return "\u00AA"; // &ordf
			case 5001: return "\u00BA"; // &ordm
			case 5005: return "\u22B6"; // &origof
			case 5007: return "\u2A56"; // &oror
			case 5012: return "\u2A57"; // &orslope
			case 5013: return "\u2A5B"; // &orv
			case 5014: return "\u24C8"; // &oS
			case 5017: return "\uD835\uDCAA"; // &Oscr
			case 5020: return "\u2134"; // &oscr
			case 5024: return "\u00D8"; // &Oslash
			case 5028: return "\u00F8"; // &oslash
			case 5030: return "\u2298"; // &osol
			case 5035: return "\u00D5"; // &Otilde
			case 5040: return "\u00F5"; // &otilde
			case 5043: return "\u2A37"; // &Otimes
			case 5046: return "\u2297"; // &otimes
			case 5048: return "\u2A36"; // &otimesas
			case 5051: return "\u00D6"; // &Ouml
			case 5054: return "\u00F6"; // &ouml
			case 5058: return "\u233D"; // &ovbar
			case 5064: return "\u203E"; // &OverBar
			case 5068: return "\u23DE"; // &OverBrace
			case 5071: return "\u23B4"; // &OverBracket
			case 5082: return "\u23DC"; // &OverParenthesis
			case 5085: return "\u2225"; // &par
			case 5086: return "\u00B6"; // &para
			case 5090: return "\u2225"; // &parallel
			case 5093: return "\u2AF3"; // &parsim
			case 5094: return "\u2AFD"; // &parsl
			case 5095: return "\u2202"; // &part
			case 5103: return "\u2202"; // &PartialD
			case 5105: return "\u041F"; // &Pcy
			case 5107: return "\u043F"; // &pcy
			case 5112: return "\u0025"; // &percnt
			case 5115: return "\u002E"; // &period
			case 5118: return "\u2030"; // &permil
			case 5119: return "\u22A5"; // &perp
			case 5123: return "\u2031"; // &pertenk
			case 5125: return "\uD835\uDD13"; // &Pfr
			case 5127: return "\uD835\uDD2D"; // &pfr
			case 5129: return "\u03A6"; // &Phi
			case 5131: return "\u03C6"; // &phi
			case 5132: return "\u03D5"; // &phiv
			case 5136: return "\u2133"; // &phmmat
			case 5139: return "\u260E"; // &phone
			case 5140: return "\u03A0"; // &Pi
			case 5141: return "\u03C0"; // &pi
			case 5148: return "\u22D4"; // &pitchfork
			case 5149: return "\u03D6"; // &piv
			case 5154: return "\u210F"; // &planck
			case 5155: return "\u210E"; // &planckh
			case 5157: return "\u210F"; // &plankv
			case 5159: return "\u002B"; // &plus
			case 5163: return "\u2A23"; // &plusacir
			case 5164: return "\u229E"; // &plusb
			case 5167: return "\u2A22"; // &pluscir
			case 5169: return "\u2214"; // &plusdo
			case 5170: return "\u2A25"; // &plusdu
			case 5171: return "\u2A72"; // &pluse
			case 5179: return "\u00B1"; // &PlusMinus
			case 5181: return "\u00B1"; // &plusmn
			case 5184: return "\u2A26"; // &plussim
			case 5187: return "\u2A27"; // &plustwo
			case 5188: return "\u00B1"; // &pm
			case 5200: return "\u210C"; // &Poincareplane
			case 5207: return "\u2A15"; // &pointint
			case 5209: return "\u2119"; // &Popf
			case 5211: return "\uD835\uDD61"; // &popf
			case 5214: return "\u00A3"; // &pound
			case 5215: return "\u2ABB"; // &Pr
			case 5216: return "\u227A"; // &pr
			case 5218: return "\u2AB7"; // &prap
			case 5221: return "\u227C"; // &prcue
			case 5222: return "\u2AB3"; // &prE
			case 5223: return "\u2AAF"; // &pre
			case 5224: return "\u227A"; // &prec
			case 5230: return "\u2AB7"; // &precapprox
			case 5237: return "\u227C"; // &preccurlyeq
			case 5243: return "\u227A"; // &Precedes
			case 5248: return "\u2AAF"; // &PrecedesEqual
			case 5258: return "\u227C"; // &PrecedesSlantEqual
			case 5263: return "\u227E"; // &PrecedesTilde
			case 5265: return "\u2AAF"; // &preceq
			case 5272: return "\u2AB9"; // &precnapprox
			case 5275: return "\u2AB5"; // &precneqq
			case 5278: return "\u22E8"; // &precnsim
			case 5281: return "\u227E"; // &precsim
			case 5284: return "\u2033"; // &Prime
			case 5287: return "\u2032"; // &prime
			case 5288: return "\u2119"; // &primes
			case 5291: return "\u2AB9"; // &prnap
			case 5292: return "\u2AB5"; // &prnE
			case 5295: return "\u22E8"; // &prnsim
			case 5297: return "\u220F"; // &prod
			case 5302: return "\u220F"; // &Product
			case 5307: return "\u232E"; // &profalar
			case 5311: return "\u2312"; // &profline
			case 5315: return "\u2313"; // &profsurf
			case 5316: return "\u221D"; // &prop
			case 5323: return "\u2237"; // &Proportion
			case 5325: return "\u221D"; // &Proportional
			case 5327: return "\u221D"; // &propto
			case 5330: return "\u227E"; // &prsim
			case 5334: return "\u22B0"; // &prurel
			case 5337: return "\uD835\uDCAB"; // &Pscr
			case 5340: return "\uD835\uDCC5"; // &pscr
			case 5341: return "\u03A8"; // &Psi
			case 5342: return "\u03C8"; // &psi
			case 5347: return "\u2008"; // &puncsp
			case 5350: return "\uD835\uDD14"; // &Qfr
			case 5353: return "\uD835\uDD2E"; // &qfr
			case 5356: return "\u2A0C"; // &qint
			case 5359: return "\u211A"; // &Qopf
			case 5362: return "\uD835\uDD62"; // &qopf
			case 5367: return "\u2057"; // &qprime
			case 5370: return "\uD835\uDCAC"; // &Qscr
			case 5373: return "\uD835\uDCC6"; // &qscr
			case 5383: return "\u210D"; // &quaternions
			case 5386: return "\u2A16"; // &quatint
			case 5389: return "\u003F"; // &quest
			case 5391: return "\u225F"; // &questeq
			case 5394: return "\u0022"; // &QUOT
			case 5396: return "\u0022"; // &quot
			case 5401: return "\u21DB"; // &rAarr
			case 5404: return "\u223D\u0331"; // &race
			case 5410: return "\u0154"; // &Racute
			case 5413: return "\u0155"; // &racute
			case 5416: return "\u221A"; // &radic
			case 5422: return "\u29B3"; // &raemptyv
			case 5424: return "\u27EB"; // &Rang
			case 5426: return "\u27E9"; // &rang
			case 5427: return "\u2992"; // &rangd
			case 5428: return "\u29A5"; // &range
			case 5430: return "\u27E9"; // &rangle
			case 5433: return "\u00BB"; // &raquo
			case 5435: return "\u21A0"; // &Rarr
			case 5437: return "\u21D2"; // &rArr
			case 5439: return "\u2192"; // &rarr
			case 5441: return "\u2975"; // &rarrap
			case 5442: return "\u21E5"; // &rarrb
			case 5444: return "\u2920"; // &rarrbfs
			case 5445: return "\u2933"; // &rarrc
			case 5447: return "\u291E"; // &rarrfs
			case 5449: return "\u21AA"; // &rarrhk
			case 5451: return "\u21AC"; // &rarrlp
			case 5453: return "\u2945"; // &rarrpl
			case 5456: return "\u2974"; // &rarrsim
			case 5458: return "\u2916"; // &Rarrtl
			case 5460: return "\u21A3"; // &rarrtl
			case 5461: return "\u219D"; // &rarrw
			case 5465: return "\u291C"; // &rAtail
			case 5469: return "\u291A"; // &ratail
			case 5471: return "\u2236"; // &ratio
			case 5475: return "\u211A"; // &rationals
			case 5479: return "\u2910"; // &RBarr
			case 5483: return "\u290F"; // &rBarr
			case 5487: return "\u290D"; // &rbarr
			case 5490: return "\u2773"; // &rbbrk
			case 5494: return "\u007D"; // &rbrace
			case 5495: return "\u005D"; // &rbrack
			case 5497: return "\u298C"; // &rbrke
			case 5500: return "\u298E"; // &rbrksld
			case 5501: return "\u2990"; // &rbrkslu
			case 5506: return "\u0158"; // &Rcaron
			case 5511: return "\u0159"; // &rcaron
			case 5515: return "\u0156"; // &Rcedil
			case 5519: return "\u0157"; // &rcedil
			case 5521: return "\u2309"; // &rceil
			case 5523: return "\u007D"; // &rcub
			case 5524: return "\u0420"; // &Rcy
			case 5525: return "\u0440"; // &rcy
			case 5528: return "\u2937"; // &rdca
			case 5533: return "\u2969"; // &rdldhar
			case 5536: return "\u201D"; // &rdquo
			case 5537: return "\u201D"; // &rdquor
			case 5539: return "\u21B3"; // &rdsh
			case 5540: return "\u211C"; // &Re
			case 5543: return "\u211C"; // &real
			case 5546: return "\u211B"; // &realine
			case 5550: return "\u211C"; // &realpart
			case 5551: return "\u211D"; // &reals
			case 5553: return "\u25AD"; // &rect
			case 5555: return "\u00AE"; // &REG
			case 5556: return "\u00AE"; // &reg
			case 5568: return "\u220B"; // &ReverseElement
			case 5578: return "\u21CB"; // &ReverseEquilibrium
			case 5591: return "\u296F"; // &ReverseUpEquilibrium
			case 5596: return "\u297D"; // &rfisht
			case 5600: return "\u230B"; // &rfloor
			case 5602: return "\u211C"; // &Rfr
			case 5603: return "\uD835\uDD2F"; // &rfr
			case 5606: return "\u2964"; // &rHar
			case 5610: return "\u21C1"; // &rhard
			case 5611: return "\u21C0"; // &rharu
			case 5612: return "\u296C"; // &rharul
			case 5614: return "\u03A1"; // &Rho
			case 5615: return "\u03C1"; // &rho
			case 5616: return "\u03F1"; // &rhov
			case 5632: return "\u27E9"; // &RightAngleBracket
			case 5636: return "\u2192"; // &RightArrow
			case 5641: return "\u21D2"; // &Rightarrow
			case 5650: return "\u2192"; // &rightarrow
			case 5653: return "\u21E5"; // &RightArrowBar
			case 5662: return "\u21C4"; // &RightArrowLeftArrow
			case 5666: return "\u21A3"; // &rightarrowtail
			case 5673: return "\u2309"; // &RightCeiling
			case 5686: return "\u27E7"; // &RightDoubleBracket
			case 5697: return "\u295D"; // &RightDownTeeVector
			case 5703: return "\u21C2"; // &RightDownVector
			case 5706: return "\u2955"; // &RightDownVectorBar
			case 5711: return "\u230B"; // &RightFloor
			case 5722: return "\u21C1"; // &rightharpoondown
			case 5724: return "\u21C0"; // &rightharpoonup
			case 5734: return "\u21C4"; // &rightleftarrows
			case 5742: return "\u21CC"; // &rightleftharpoons
			case 5753: return "\u21C9"; // &rightrightarrows
			case 5763: return "\u219D"; // &rightsquigarrow
			case 5766: return "\u22A2"; // &RightTee
			case 5771: return "\u21A6"; // &RightTeeArrow
			case 5777: return "\u295B"; // &RightTeeVector
			case 5787: return "\u22CC"; // &rightthreetimes
			case 5794: return "\u22B3"; // &RightTriangle
			case 5797: return "\u29D0"; // &RightTriangleBar
			case 5802: return "\u22B5"; // &RightTriangleEqual
			case 5814: return "\u294F"; // &RightUpDownVector
			case 5823: return "\u295C"; // &RightUpTeeVector
			case 5829: return "\u21BE"; // &RightUpVector
			case 5832: return "\u2954"; // &RightUpVectorBar
			case 5838: return "\u21C0"; // &RightVector
			case 5841: return "\u2953"; // &RightVectorBar
			case 5843: return "\u02DA"; // &ring
			case 5853: return "\u2253"; // &risingdotseq
			case 5857: return "\u21C4"; // &rlarr
			case 5860: return "\u21CC"; // &rlhar
			case 5861: return "\u200F"; // &rlm
			case 5866: return "\u23B1"; // &rmoust
			case 5870: return "\u23B1"; // &rmoustache
			case 5874: return "\u2AEE"; // &rnmid
			case 5878: return "\u27ED"; // &roang
			case 5880: return "\u21FE"; // &roarr
			case 5883: return "\u27E7"; // &robrk
			case 5886: return "\u2986"; // &ropar
			case 5889: return "\u211D"; // &Ropf
			case 5890: return "\uD835\uDD63"; // &ropf
			case 5893: return "\u2A2E"; // &roplus
			case 5898: return "\u2A35"; // &rotimes
			case 5908: return "\u2970"; // &RoundImplies
			case 5911: return "\u0029"; // &rpar
			case 5913: return "\u2994"; // &rpargt
			case 5919: return "\u2A12"; // &rppolint
			case 5923: return "\u21C9"; // &rrarr
			case 5933: return "\u21DB"; // &Rrightarrow
			case 5938: return "\u203A"; // &rsaquo
			case 5941: return "\u211B"; // &Rscr
			case 5943: return "\uD835\uDCC7"; // &rscr
			case 5944: return "\u21B1"; // &Rsh
			case 5945: return "\u21B1"; // &rsh
			case 5947: return "\u005D"; // &rsqb
			case 5949: return "\u2019"; // &rsquo
			case 5950: return "\u2019"; // &rsquor
			case 5955: return "\u22CC"; // &rthree
			case 5959: return "\u22CA"; // &rtimes
			case 5961: return "\u25B9"; // &rtri
			case 5962: return "\u22B5"; // &rtrie
			case 5963: return "\u25B8"; // &rtrif
			case 5967: return "\u29CE"; // &rtriltri
			case 5977: return "\u29F4"; // &RuleDelayed
			case 5983: return "\u2968"; // &ruluhar
			case 5984: return "\u211E"; // &rx
			case 5990: return "\u015A"; // &Sacute
			case 5996: return "\u015B"; // &sacute
			case 6000: return "\u201A"; // &sbquo
			case 6001: return "\u2ABC"; // &Sc
			case 6002: return "\u227B"; // &sc
			case 6004: return "\u2AB8"; // &scap
			case 6008: return "\u0160"; // &Scaron
			case 6011: return "\u0161"; // &scaron
			case 6014: return "\u227D"; // &sccue
			case 6015: return "\u2AB4"; // &scE
			case 6016: return "\u2AB0"; // &sce
			case 6020: return "\u015E"; // &Scedil
			case 6023: return "\u015F"; // &scedil
			case 6026: return "\u015C"; // &Scirc
			case 6029: return "\u015D"; // &scirc
			case 6032: return "\u2ABA"; // &scnap
			case 6033: return "\u2AB6"; // &scnE
			case 6036: return "\u22E9"; // &scnsim
			case 6042: return "\u2A13"; // &scpolint
			case 6045: return "\u227F"; // &scsim
			case 6046: return "\u0421"; // &Scy
			case 6047: return "\u0441"; // &scy
			case 6050: return "\u22C5"; // &sdot
			case 6051: return "\u22A1"; // &sdotb
			case 6052: return "\u2A66"; // &sdote
			case 6057: return "\u2925"; // &searhk
			case 6060: return "\u21D8"; // &seArr
			case 6061: return "\u2198"; // &searr
			case 6063: return "\u2198"; // &searrow
			case 6065: return "\u00A7"; // &sect
			case 6067: return "\u003B"; // &semi
			case 6071: return "\u2929"; // &seswar
			case 6077: return "\u2216"; // &setminus
			case 6078: return "\u2216"; // &setmn
			case 6080: return "\u2736"; // &sext
			case 6082: return "\uD835\uDD16"; // &Sfr
			case 6084: return "\uD835\uDD30"; // &sfr
			case 6087: return "\u2322"; // &sfrown
			case 6091: return "\u266F"; // &sharp
			case 6096: return "\u0429"; // &SHCHcy
			case 6100: return "\u0449"; // &shchcy
			case 6102: return "\u0428"; // &SHcy
			case 6103: return "\u0448"; // &shcy
			case 6116: return "\u2193"; // &ShortDownArrow
			case 6125: return "\u2190"; // &ShortLeftArrow
			case 6131: return "\u2223"; // &shortmid
			case 6139: return "\u2225"; // &shortparallel
			case 6149: return "\u2192"; // &ShortRightArrow
			case 6156: return "\u2191"; // &ShortUpArrow
			case 6157: return "\u00AD"; // &shy
			case 6161: return "\u03A3"; // &Sigma
			case 6165: return "\u03C3"; // &sigma
			case 6166: return "\u03C2"; // &sigmaf
			case 6167: return "\u03C2"; // &sigmav
			case 6168: return "\u223C"; // &sim
			case 6171: return "\u2A6A"; // &simdot
			case 6172: return "\u2243"; // &sime
			case 6173: return "\u2243"; // &simeq
			case 6174: return "\u2A9E"; // &simg
			case 6175: return "\u2AA0"; // &simgE
			case 6176: return "\u2A9D"; // &siml
			case 6177: return "\u2A9F"; // &simlE
			case 6179: return "\u2246"; // &simne
			case 6183: return "\u2A24"; // &simplus
			case 6187: return "\u2972"; // &simrarr
			case 6191: return "\u2190"; // &slarr
			case 6201: return "\u2218"; // &SmallCircle
			case 6213: return "\u2216"; // &smallsetminus
			case 6216: return "\u2A33"; // &smashp
			case 6222: return "\u29E4"; // &smeparsl
			case 6224: return "\u2223"; // &smid
			case 6226: return "\u2323"; // &smile
			case 6227: return "\u2AAA"; // &smt
			case 6228: return "\u2AAC"; // &smte
			case 6229: return "\u2AAC\uFE00"; // &smtes
			case 6234: return "\u042C"; // &SOFTcy
			case 6239: return "\u044C"; // &softcy
			case 6240: return "\u002F"; // &sol
			case 6241: return "\u29C4"; // &solb
			case 6243: return "\u233F"; // &solbar
			case 6246: return "\uD835\uDD4A"; // &Sopf
			case 6248: return "\uD835\uDD64"; // &sopf
			case 6253: return "\u2660"; // &spades
			case 6256: return "\u2660"; // &spadesuit
			case 6257: return "\u2225"; // &spar
			case 6261: return "\u2293"; // &sqcap
			case 6262: return "\u2293\uFE00"; // &sqcaps
			case 6264: return "\u2294"; // &sqcup
			case 6265: return "\u2294\uFE00"; // &sqcups
			case 6268: return "\u221A"; // &Sqrt
			case 6271: return "\u228F"; // &sqsub
			case 6272: return "\u2291"; // &sqsube
			case 6275: return "\u228F"; // &sqsubset
			case 6277: return "\u2291"; // &sqsubseteq
			case 6278: return "\u2290"; // &sqsup
			case 6279: return "\u2292"; // &sqsupe
			case 6282: return "\u2290"; // &sqsupset
			case 6284: return "\u2292"; // &sqsupseteq
			case 6285: return "\u25A1"; // &squ
			case 6289: return "\u25A1"; // &Square
			case 6292: return "\u25A1"; // &square
			case 6304: return "\u2293"; // &SquareIntersection
			case 6310: return "\u228F"; // &SquareSubset
			case 6315: return "\u2291"; // &SquareSubsetEqual
			case 6321: return "\u2290"; // &SquareSuperset
			case 6326: return "\u2292"; // &SquareSupersetEqual
			case 6331: return "\u2294"; // &SquareUnion
			case 6332: return "\u25AA"; // &squarf
			case 6333: return "\u25AA"; // &squf
			case 6337: return "\u2192"; // &srarr
			case 6340: return "\uD835\uDCAE"; // &Sscr
			case 6343: return "\uD835\uDCC8"; // &sscr
			case 6347: return "\u2216"; // &ssetmn
			case 6351: return "\u2323"; // &ssmile
			case 6355: return "\u22C6"; // &sstarf
			case 6358: return "\u22C6"; // &Star
			case 6361: return "\u2606"; // &star
			case 6362: return "\u2605"; // &starf
			case 6375: return "\u03F5"; // &straightepsilon
			case 6378: return "\u03D5"; // &straightphi
			case 6380: return "\u00AF"; // &strns
			case 6382: return "\u22D0"; // &Sub
			case 6384: return "\u2282"; // &sub
			case 6387: return "\u2ABD"; // &subdot
			case 6388: return "\u2AC5"; // &subE
			case 6389: return "\u2286"; // &sube
			case 6392: return "\u2AC3"; // &subedot
			case 6396: return "\u2AC1"; // &submult
			case 6398: return "\u2ACB"; // &subnE
			case 6399: return "\u228A"; // &subne
			case 6403: return "\u2ABF"; // &subplus
			case 6407: return "\u2979"; // &subrarr
			case 6410: return "\u22D0"; // &Subset
			case 6413: return "\u2282"; // &subset
			case 6415: return "\u2286"; // &subseteq
			case 6416: return "\u2AC5"; // &subseteqq
			case 6421: return "\u2286"; // &SubsetEqual
			case 6424: return "\u228A"; // &subsetneq
			case 6425: return "\u2ACB"; // &subsetneqq
			case 6427: return "\u2AC7"; // &subsim
			case 6429: return "\u2AD5"; // &subsub
			case 6430: return "\u2AD3"; // &subsup
			case 6432: return "\u227B"; // &succ
			case 6438: return "\u2AB8"; // &succapprox
			case 6445: return "\u227D"; // &succcurlyeq
			case 6451: return "\u227B"; // &Succeeds
			case 6456: return "\u2AB0"; // &SucceedsEqual
			case 6466: return "\u227D"; // &SucceedsSlantEqual
			case 6471: return "\u227F"; // &SucceedsTilde
			case 6473: return "\u2AB0"; // &succeq
			case 6480: return "\u2ABA"; // &succnapprox
			case 6483: return "\u2AB6"; // &succneqq
			case 6486: return "\u22E9"; // &succnsim
			case 6489: return "\u227F"; // &succsim
			case 6494: return "\u220B"; // &SuchThat
			case 6495: return "\u2211"; // &Sum
			case 6496: return "\u2211"; // &sum
			case 6498: return "\u266A"; // &sung
			case 6499: return "\u22D1"; // &Sup
			case 6500: return "\u2283"; // &sup
			case 6501: return "\u00B9"; // &sup1
			case 6502: return "\u00B2"; // &sup2
			case 6503: return "\u00B3"; // &sup3
			case 6506: return "\u2ABE"; // &supdot
			case 6509: return "\u2AD8"; // &supdsub
			case 6510: return "\u2AC6"; // &supE
			case 6511: return "\u2287"; // &supe
			case 6514: return "\u2AC4"; // &supedot
			case 6519: return "\u2283"; // &Superset
			case 6524: return "\u2287"; // &SupersetEqual
			case 6528: return "\u27C9"; // &suphsol
			case 6530: return "\u2AD7"; // &suphsub
			case 6534: return "\u297B"; // &suplarr
			case 6538: return "\u2AC2"; // &supmult
			case 6540: return "\u2ACC"; // &supnE
			case 6541: return "\u228B"; // &supne
			case 6545: return "\u2AC0"; // &supplus
			case 6548: return "\u22D1"; // &Supset
			case 6551: return "\u2283"; // &supset
			case 6553: return "\u2287"; // &supseteq
			case 6554: return "\u2AC6"; // &supseteqq
			case 6557: return "\u228B"; // &supsetneq
			case 6558: return "\u2ACC"; // &supsetneqq
			case 6560: return "\u2AC8"; // &supsim
			case 6562: return "\u2AD4"; // &supsub
			case 6563: return "\u2AD6"; // &supsup
			case 6568: return "\u2926"; // &swarhk
			case 6571: return "\u21D9"; // &swArr
			case 6572: return "\u2199"; // &swarr
			case 6574: return "\u2199"; // &swarrow
			case 6578: return "\u292A"; // &swnwar
			case 6582: return "\u00DF"; // &szlig
			case 6585: return "\u0009"; // &Tab
			case 6591: return "\u2316"; // &target
			case 6592: return "\u03A4"; // &Tau
			case 6593: return "\u03C4"; // &tau
			case 6596: return "\u23B4"; // &tbrk
			case 6601: return "\u0164"; // &Tcaron
			case 6606: return "\u0165"; // &tcaron
			case 6610: return "\u0162"; // &Tcedil
			case 6614: return "\u0163"; // &tcedil
			case 6615: return "\u0422"; // &Tcy
			case 6616: return "\u0442"; // &tcy
			case 6619: return "\u20DB"; // &tdot
			case 6624: return "\u2315"; // &telrec
			case 6626: return "\uD835\uDD17"; // &Tfr
			case 6628: return "\uD835\uDD31"; // &tfr
			case 6633: return "\u2234"; // &there4
			case 6641: return "\u2234"; // &Therefore
			case 6645: return "\u2234"; // &therefore
			case 6647: return "\u0398"; // &Theta
			case 6649: return "\u03B8"; // &theta
			case 6652: return "\u03D1"; // &thetasym
			case 6653: return "\u03D1"; // &thetav
			case 6662: return "\u2248"; // &thickapprox
			case 6665: return "\u223C"; // &thicksim
			case 6673: return "\u205F\u200A"; // &ThickSpace
			case 6676: return "\u2009"; // &thinsp
			case 6682: return "\u2009"; // &ThinSpace
			case 6685: return "\u2248"; // &thkap
			case 6688: return "\u223C"; // &thksim
			case 6692: return "\u00DE"; // &THORN
			case 6695: return "\u00FE"; // &thorn
			case 6699: return "\u223C"; // &Tilde
			case 6703: return "\u02DC"; // &tilde
			case 6708: return "\u2243"; // &TildeEqual
			case 6717: return "\u2245"; // &TildeFullEqual
			case 6722: return "\u2248"; // &TildeTilde
			case 6725: return "\u00D7"; // &times
			case 6726: return "\u22A0"; // &timesb
			case 6728: return "\u2A31"; // &timesbar
			case 6729: return "\u2A30"; // &timesd
			case 6731: return "\u222D"; // &tint
			case 6734: return "\u2928"; // &toea
			case 6735: return "\u22A4"; // &top
			case 6738: return "\u2336"; // &topbot
			case 6741: return "\u2AF1"; // &topcir
			case 6744: return "\uD835\uDD4B"; // &Topf
			case 6745: return "\uD835\uDD65"; // &topf
			case 6748: return "\u2ADA"; // &topfork
			case 6750: return "\u2929"; // &tosa
			case 6755: return "\u2034"; // &tprime
			case 6759: return "\u2122"; // &TRADE
			case 6763: return "\u2122"; // &trade
			case 6769: return "\u25B5"; // &triangle
			case 6773: return "\u25BF"; // &triangledown
			case 6777: return "\u25C3"; // &triangleleft
			case 6779: return "\u22B4"; // &trianglelefteq
			case 6780: return "\u225C"; // &triangleq
			case 6785: return "\u25B9"; // &triangleright
			case 6787: return "\u22B5"; // &trianglerighteq
			case 6790: return "\u25EC"; // &tridot
			case 6791: return "\u225C"; // &trie
			case 6796: return "\u2A3A"; // &triminus
			case 6804: return "\u20DB"; // &TripleDot
			case 6808: return "\u2A39"; // &triplus
			case 6810: return "\u29CD"; // &trisb
			case 6814: return "\u2A3B"; // &tritime
			case 6820: return "\u23E2"; // &trpezium
			case 6823: return "\uD835\uDCAF"; // &Tscr
			case 6826: return "\uD835\uDCC9"; // &tscr
			case 6829: return "\u0426"; // &TScy
			case 6830: return "\u0446"; // &tscy
			case 6833: return "\u040B"; // &TSHcy
			case 6836: return "\u045B"; // &tshcy
			case 6840: return "\u0166"; // &Tstrok
			case 6844: return "\u0167"; // &tstrok
			case 6848: return "\u226C"; // &twixt
			case 6862: return "\u219E"; // &twoheadleftarrow
			case 6872: return "\u21A0"; // &twoheadrightarrow
			case 6878: return "\u00DA"; // &Uacute
			case 6884: return "\u00FA"; // &uacute
			case 6886: return "\u219F"; // &Uarr
			case 6889: return "\u21D1"; // &uArr
			case 6891: return "\u2191"; // &uarr
			case 6895: return "\u2949"; // &Uarrocir
			case 6899: return "\u040E"; // &Ubrcy
			case 6903: return "\u045E"; // &ubrcy
			case 6906: return "\u016C"; // &Ubreve
			case 6909: return "\u016D"; // &ubreve
			case 6913: return "\u00DB"; // &Ucirc
			case 6917: return "\u00FB"; // &ucirc
			case 6918: return "\u0423"; // &Ucy
			case 6919: return "\u0443"; // &ucy
			case 6923: return "\u21C5"; // &udarr
			case 6928: return "\u0170"; // &Udblac
			case 6932: return "\u0171"; // &udblac
			case 6935: return "\u296E"; // &udhar
			case 6940: return "\u297E"; // &ufisht
			case 6942: return "\uD835\uDD18"; // &Ufr
			case 6943: return "\uD835\uDD32"; // &ufr
			case 6948: return "\u00D9"; // &Ugrave
			case 6953: return "\u00F9"; // &ugrave
			case 6956: return "\u2963"; // &uHar
			case 6960: return "\u21BF"; // &uharl
			case 6961: return "\u21BE"; // &uharr
			case 6964: return "\u2580"; // &uhblk
			case 6969: return "\u231C"; // &ulcorn
			case 6971: return "\u231C"; // &ulcorner
			case 6974: return "\u230F"; // &ulcrop
			case 6977: return "\u25F8"; // &ultri
			case 6981: return "\u016A"; // &Umacr
			case 6985: return "\u016B"; // &umacr
			case 6986: return "\u00A8"; // &uml
			case 6993: return "\u005F"; // &UnderBar
			case 6997: return "\u23DF"; // &UnderBrace
			case 7000: return "\u23B5"; // &UnderBracket
			case 7011: return "\u23DD"; // &UnderParenthesis
			case 7014: return "\u22C3"; // &Union
			case 7018: return "\u228E"; // &UnionPlus
			case 7022: return "\u0172"; // &Uogon
			case 7026: return "\u0173"; // &uogon
			case 7028: return "\uD835\uDD4C"; // &Uopf
			case 7030: return "\uD835\uDD66"; // &uopf
			case 7036: return "\u2191"; // &UpArrow
			case 7041: return "\u21D1"; // &Uparrow
			case 7047: return "\u2191"; // &uparrow
			case 7050: return "\u2912"; // &UpArrowBar
			case 7059: return "\u21C5"; // &UpArrowDownArrow
			case 7068: return "\u2195"; // &UpDownArrow
			case 7077: return "\u21D5"; // &Updownarrow
			case 7086: return "\u2195"; // &updownarrow
			case 7097: return "\u296E"; // &UpEquilibrium
			case 7108: return "\u21BF"; // &upharpoonleft
			case 7113: return "\u21BE"; // &upharpoonright
			case 7116: return "\u228E"; // &uplus
			case 7128: return "\u2196"; // &UpperLeftArrow
			case 7138: return "\u2197"; // &UpperRightArrow
			case 7140: return "\u03D2"; // &Upsi
			case 7142: return "\u03C5"; // &upsi
			case 7143: return "\u03D2"; // &upsih
			case 7146: return "\u03A5"; // &Upsilon
			case 7149: return "\u03C5"; // &upsilon
			case 7152: return "\u22A5"; // &UpTee
			case 7157: return "\u21A5"; // &UpTeeArrow
			case 7165: return "\u21C8"; // &upuparrows
			case 7170: return "\u231D"; // &urcorn
			case 7172: return "\u231D"; // &urcorner
			case 7175: return "\u230E"; // &urcrop
			case 7179: return "\u016E"; // &Uring
			case 7182: return "\u016F"; // &uring
			case 7185: return "\u25F9"; // &urtri
			case 7188: return "\uD835\uDCB0"; // &Uscr
			case 7191: return "\uD835\uDCCA"; // &uscr
			case 7195: return "\u22F0"; // &utdot
			case 7200: return "\u0168"; // &Utilde
			case 7204: return "\u0169"; // &utilde
			case 7206: return "\u25B5"; // &utri
			case 7207: return "\u25B4"; // &utrif
			case 7211: return "\u21C8"; // &uuarr
			case 7214: return "\u00DC"; // &Uuml
			case 7216: return "\u00FC"; // &uuml
			case 7222: return "\u29A7"; // &uwangle
			case 7228: return "\u299C"; // &vangrt
			case 7236: return "\u03F5"; // &varepsilon
			case 7241: return "\u03F0"; // &varkappa
			case 7248: return "\u2205"; // &varnothing
			case 7251: return "\u03D5"; // &varphi
			case 7252: return "\u03D6"; // &varpi
			case 7257: return "\u221D"; // &varpropto
			case 7260: return "\u21D5"; // &vArr
			case 7261: return "\u2195"; // &varr
			case 7263: return "\u03F1"; // &varrho
			case 7268: return "\u03C2"; // &varsigma
			case 7276: return "\u228A\uFE00"; // &varsubsetneq
			case 7277: return "\u2ACB\uFE00"; // &varsubsetneqq
			case 7284: return "\u228B\uFE00"; // &varsupsetneq
			case 7285: return "\u2ACC\uFE00"; // &varsupsetneqq
			case 7290: return "\u03D1"; // &vartheta
			case 7301: return "\u22B2"; // &vartriangleleft
			case 7306: return "\u22B3"; // &vartriangleright
			case 7310: return "\u2AEB"; // &Vbar
			case 7313: return "\u2AE8"; // &vBar
			case 7314: return "\u2AE9"; // &vBarv
			case 7316: return "\u0412"; // &Vcy
			case 7318: return "\u0432"; // &vcy
			case 7322: return "\u22AB"; // &VDash
			case 7326: return "\u22A9"; // &Vdash
			case 7330: return "\u22A8"; // &vDash
			case 7334: return "\u22A2"; // &vdash
			case 7335: return "\u2AE6"; // &Vdashl
			case 7337: return "\u22C1"; // &Vee
			case 7339: return "\u2228"; // &vee
			case 7342: return "\u22BB"; // &veebar
			case 7344: return "\u225A"; // &veeeq
			case 7348: return "\u22EE"; // &vellip
			case 7352: return "\u2016"; // &Verbar
			case 7356: return "\u007C"; // &verbar
			case 7357: return "\u2016"; // &Vert
			case 7358: return "\u007C"; // &vert
			case 7365: return "\u2223"; // &VerticalBar
			case 7369: return "\u007C"; // &VerticalLine
			case 7378: return "\u2758"; // &VerticalSeparator
			case 7383: return "\u2240"; // &VerticalTilde
			case 7393: return "\u200A"; // &VeryThinSpace
			case 7395: return "\uD835\uDD19"; // &Vfr
			case 7397: return "\uD835\uDD33"; // &vfr
			case 7401: return "\u22B2"; // &vltri
			case 7405: return "\u2282\u20D2"; // &vnsub
			case 7406: return "\u2283\u20D2"; // &vnsup
			case 7409: return "\uD835\uDD4D"; // &Vopf
			case 7412: return "\uD835\uDD67"; // &vopf
			case 7416: return "\u221D"; // &vprop
			case 7420: return "\u22B3"; // &vrtri
			case 7423: return "\uD835\uDCB1"; // &Vscr
			case 7426: return "\uD835\uDCCB"; // &vscr
			case 7430: return "\u2ACB\uFE00"; // &vsubnE
			case 7431: return "\u228A\uFE00"; // &vsubne
			case 7434: return "\u2ACC\uFE00"; // &vsupnE
			case 7435: return "\u228B\uFE00"; // &vsupne
			case 7440: return "\u22AA"; // &Vvdash
			case 7446: return "\u299A"; // &vzigzag
			case 7451: return "\u0174"; // &Wcirc
			case 7456: return "\u0175"; // &wcirc
			case 7461: return "\u2A5F"; // &wedbar
			case 7465: return "\u22C0"; // &Wedge
			case 7467: return "\u2227"; // &wedge
			case 7468: return "\u2259"; // &wedgeq
			case 7472: return "\u2118"; // &weierp
			case 7474: return "\uD835\uDD1A"; // &Wfr
			case 7476: return "\uD835\uDD34"; // &wfr
			case 7479: return "\uD835\uDD4E"; // &Wopf
			case 7482: return "\uD835\uDD68"; // &wopf
			case 7483: return "\u2118"; // &wp
			case 7484: return "\u2240"; // &wr
			case 7488: return "\u2240"; // &wreath
			case 7491: return "\uD835\uDCB2"; // &Wscr
			case 7494: return "\uD835\uDCCC"; // &wscr
			case 7498: return "\u22C2"; // &xcap
			case 7501: return "\u25EF"; // &xcirc
			case 7503: return "\u22C3"; // &xcup
			case 7507: return "\u25BD"; // &xdtri
			case 7510: return "\uD835\uDD1B"; // &Xfr
			case 7512: return "\uD835\uDD35"; // &xfr
			case 7516: return "\u27FA"; // &xhArr
			case 7519: return "\u27F7"; // &xharr
			case 7520: return "\u039E"; // &Xi
			case 7521: return "\u03BE"; // &xi
			case 7525: return "\u27F8"; // &xlArr
			case 7528: return "\u27F5"; // &xlarr
			case 7531: return "\u27FC"; // &xmap
			case 7534: return "\u22FB"; // &xnis
			case 7538: return "\u2A00"; // &xodot
			case 7541: return "\uD835\uDD4F"; // &Xopf
			case 7543: return "\uD835\uDD69"; // &xopf
			case 7546: return "\u2A01"; // &xoplus
			case 7550: return "\u2A02"; // &xotime
			case 7554: return "\u27F9"; // &xrArr
			case 7557: return "\u27F6"; // &xrarr
			case 7560: return "\uD835\uDCB3"; // &Xscr
			case 7563: return "\uD835\uDCCD"; // &xscr
			case 7567: return "\u2A06"; // &xsqcup
			case 7572: return "\u2A04"; // &xuplus
			case 7575: return "\u25B3"; // &xutri
			case 7578: return "\u22C1"; // &xvee
			case 7583: return "\u22C0"; // &xwedge
			case 7589: return "\u00DD"; // &Yacute
			case 7595: return "\u00FD"; // &yacute
			case 7598: return "\u042F"; // &YAcy
			case 7599: return "\u044F"; // &yacy
			case 7603: return "\u0176"; // &Ycirc
			case 7607: return "\u0177"; // &ycirc
			case 7608: return "\u042B"; // &Ycy
			case 7609: return "\u044B"; // &ycy
			case 7611: return "\u00A5"; // &yen
			case 7613: return "\uD835\uDD1C"; // &Yfr
			case 7615: return "\uD835\uDD36"; // &yfr
			case 7618: return "\u0407"; // &YIcy
			case 7621: return "\u0457"; // &yicy
			case 7624: return "\uD835\uDD50"; // &Yopf
			case 7627: return "\uD835\uDD6A"; // &yopf
			case 7630: return "\uD835\uDCB4"; // &Yscr
			case 7633: return "\uD835\uDCCE"; // &yscr
			case 7636: return "\u042E"; // &YUcy
			case 7639: return "\u044E"; // &yucy
			case 7642: return "\u0178"; // &Yuml
			case 7644: return "\u00FF"; // &yuml
			case 7650: return "\u0179"; // &Zacute
			case 7656: return "\u017A"; // &zacute
			case 7661: return "\u017D"; // &Zcaron
			case 7666: return "\u017E"; // &zcaron
			case 7667: return "\u0417"; // &Zcy
			case 7668: return "\u0437"; // &zcy
			case 7671: return "\u017B"; // &Zdot
			case 7674: return "\u017C"; // &zdot
			case 7679: return "\u2128"; // &zeetrf
			case 7692: return "\u200B"; // &ZeroWidthSpace
			case 7694: return "\u0396"; // &Zeta
			case 7696: return "\u03B6"; // &zeta
			case 7698: return "\u2128"; // &Zfr
			case 7700: return "\uD835\uDD37"; // &zfr
			case 7703: return "\u0416"; // &ZHcy
			case 7706: return "\u0436"; // &zhcy
			case 7712: return "\u21DD"; // &zigrarr
			case 7715: return "\u2124"; // &Zopf
			case 7718: return "\uD835\uDD6B"; // &zopf
			case 7721: return "\uD835\uDCB5"; // &Zscr
			case 7724: return "\uD835\uDCCF"; // &zscr
			case 7726: return "\u200D"; // &zwj
			case 7728: return "\u200C"; // &zwnj
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
