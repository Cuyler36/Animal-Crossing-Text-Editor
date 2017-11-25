using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Animal_Crossing_Text_Editor
{
    public static class TextUtility
    {
        public static readonly Dictionary<byte, string> Animal_Crossing_Character_Map = new Dictionary<byte, string>()
        {
            { 0x00, "¡" },
            { 0x01, "¿" },
            { 0x02, "Ä" },
            { 0x03, "À" },
            { 0x04, "Á" },
            { 0x05, "Â" },
            { 0x06, "Ã" },
            { 0x07, "Å" },
            { 0x08, "Ç" },
            { 0x09, "È" },
            { 0x0A, "É" },
            { 0x0B, "Ê" },
            { 0x0C, "Ë" },
            { 0x0D, "Ì" },
            { 0x0E, "Í" },
            { 0x0F, "Î" },
            { 0x10, "Ï" },
            { 0x11, "Ð" },
            { 0x12, "Ñ" },
            { 0x13, "Ò" },
            { 0x14, "Ó" },
            { 0x15, "Ô" },
            { 0x16, "Õ" },
            { 0x17, "Ö" },
            { 0x18, "Ø" },
            { 0x19, "Ù" },
            { 0x1A, "Ú" },
            { 0x1B, "Û" },
            { 0x1C, "Ü" },
            { 0x1D, "ß" },
            { 0x1E, "Þ" },
            { 0x1F, "à" },
            { 0x20, " " },
            { 0x21, "!" },
            { 0x22, "\"" },
            { 0x23, "á" },
            { 0x24, "â" },
            { 0x25, "%" },
            { 0x26, "&" },
            { 0x27, "'" },
            { 0x28, "(" },
            { 0x29, ")" },
            { 0x2A, "~" },
            { 0x2B, "♥" },
            { 0x2C, "," },
            { 0x2D, "-" },
            { 0x2E, "." },
            { 0x2F, "♪" },
            { 0x30, "0" },
            { 0x31, "1" },
            { 0x32, "2" },
            { 0x33, "3" },
            { 0x34, "4" },
            { 0x35, "5" },
            { 0x36, "6" },
            { 0x37, "7" },
            { 0x38, "8" },
            { 0x39, "9" },
            { 0x3A, ":" },
            { 0x3B, "🌢" },
            { 0x3C, "<" },
            { 0x3D, "=" },
            { 0x3E, ">" },
            { 0x3F, "?" },
            { 0x40, "@" },
            { 0x41, "A" },
            { 0x42, "B" },
            { 0x43, "C" },
            { 0x44, "D" },
            { 0x45, "E" },
            { 0x46, "F" },
            { 0x47, "G" },
            { 0x48, "H" },
            { 0x49, "I" },
            { 0x4A, "J" },
            { 0x4B, "K" },
            { 0x4C, "L" },
            { 0x4D, "M" },
            { 0x4E, "N" },
            { 0x4F, "O" },
            { 0x50, "P" },
            { 0x51, "Q" },
            { 0x52, "R" },
            { 0x53, "S" },
            { 0x54, "T" },
            { 0x55, "U" },
            { 0x56, "V" },
            { 0x57, "W" },
            { 0x58, "X" },
            { 0x59, "Y" },
            { 0x5A, "Z" },
            { 0x5B, "ã" },
            { 0x5C, "💢" },
            { 0x5D, "ä" },
            { 0x5E, "å" },
            { 0x5F, "_" },
            { 0x60, "ç" },
            { 0x61, "a" },
            { 0x62, "b" },
            { 0x63, "c" },
            { 0x64, "d" },
            { 0x65, "e" },
            { 0x66, "f" },
            { 0x67, "g" },
            { 0x68, "h" },
            { 0x69, "i" },
            { 0x6A, "j" },
            { 0x6B, "k" },
            { 0x6C, "l" },
            { 0x6D, "m" },
            { 0x6E, "n" },
            { 0x6F, "o" },
            { 0x70, "p" },
            { 0x71, "q" },
            { 0x72, "r" },
            { 0x73, "s" },
            { 0x74, "t" },
            { 0x75, "u" },
            { 0x76, "v" },
            { 0x77, "w" },
            { 0x78, "x" },
            { 0x79, "y" },
            { 0x7A, "z" },
            { 0x7B, "è" },
            { 0x7C, "é" },
            { 0x7D, "ê" },
            { 0x7E, "ë" },
            { 0x7F, "□" },
            { 0x80, "�" },
            { 0x81, "ì" },
            { 0x82, "í" },
            { 0x83, "î" },
            { 0x84, "ï" },
            { 0x85, "•" },
            { 0x86, "ð" },
            { 0x87, "ñ" },
            { 0x88, "ò" },
            { 0x89, "ó" },
            { 0x8A, "ô" },
            { 0x8B, "õ" },
            { 0x8C, "ö" },
            { 0x8D, "⁰" },
            { 0x8E, "ù" },
            { 0x8F, "ú" },
            { 0x90, "–" },
            { 0x91, "û" },
            { 0x92, "ü" },
            { 0x93, "ý" },
            { 0x94, "ÿ" },
            { 0x95, "þ" },
            { 0x96, "Ý" },
            { 0x97, "¦" },
            { 0x98, "§" },
            { 0x99, "a̱" },
            { 0x9A, "o̱" },
            { 0x9B, "‖" },
            { 0x9C, "µ" },
            { 0x9D, "³" },
            { 0x9E, "²" },
            { 0x9F, "¹" },
            { 0xA0, "¯" },
            { 0xA1, "¬" },
            { 0xA2, "Æ" },
            { 0xA3, "æ" },
            { 0xA4, "„" },
            { 0xA5, "»" },
            { 0xA6, "«" },
            { 0xA7, "☀" },
            { 0xA8, "☁" },
            { 0xA9, "☂" },
            { 0xAA, "🌬" }, //Wind...
            { 0xAB, "☃" },
            { 0xAC, "∋" },
            { 0xAD, "∈" },
            { 0xAE, "/" },
            { 0xAF, "∞" },
            { 0xB0, "○" },
            { 0xB1, "🗙" },
            { 0xB2, "□" },
            { 0xB3, "△" },
            { 0xB4, "+" },
            { 0xB5, "⚡" },
            { 0xB6, "♂" },
            { 0xB7, "♀" },
            { 0xB8, "🍀"},
            { 0xB9, "★" },
            { 0xBA, "💀" },
            { 0xBB, "😮" },
            { 0xBC, "😄" },
            { 0xBD, "😣" },
            { 0xBE, "😠" },
            { 0xBF, "😃" },
            { 0xC0, "×" },
            { 0xC1, "÷" },
            { 0xC2, "🔨" }, //Hammer??
            { 0xC3, "🎀" }, //Not sure wtf this is (put it as ribbon)
            { 0xC4, "✉" },
            { 0xC5, "💰" },
            { 0xC6, "🐾" },
            { 0xC7, "🐶" },
            { 0xC8, "🐱" },
            { 0xC9, "🐰" },
            { 0xCA, "🐦" },
            { 0xCB, "🐮" },
            { 0xCC, "🐷" },
            { 0xCD, "\n" },
            { 0xCE, "🐟" },
            { 0xCF, "🐞" },
            { 0xD0, ";" },
            { 0xD1, "#" },
            //0xD2
            //0xD3
            { 0xD4, "⚷" },
            //0xD5
            //0xD6
            //0xD7
            //0xD8
            //0xD9
            //0xDA
            //0xDB
            //0xDC
            //0xDD
            //0xDE
            //0xDF
            //0xE0
            //0xE1
            //0xE2
            //0xE3
            //0xE4
            //0xE5
            //0xE6
            //0xE7
            //0xE8
            //0xE9
            //0xEA
            //0xEB
            //0xEC
            //0xED
            //0xEE
            //0xEF
            //0xF0
            //0xF1
            //0xF2
            //0xF3
            //0xF4
            //0xF5
            //0xF6
            //0xF7
            //0xF8
            //0xF9
            //0xFA
            //0xFB
            //0xFC
            //0xFD
            //0xFE
            //0xFF
        };

        public static Dictionary<byte, string> Doubutsu_no_Mori_Plus_Character_Map = new Dictionary<byte, string>
        {
            { 0x00, "あ" },
            { 0x01, "い" },
            { 0x02, "う" },
            { 0x03, "え" },
            { 0x04, "お" },
            { 0x05, "か" },
            { 0x06, "き" },
            { 0x07, "く" },
            { 0x08, "け" },
            { 0x09, "こ" },
            { 0x0A, "さ" },
            { 0x0B, "し" },
            { 0x0C, "す" },
            { 0x0D, "せ" },
            { 0x0E, "そ" },
            { 0x0F, "た" },
            { 0x10, "ち" },
            { 0x11, "つ" },
            { 0x12, "て" },
            { 0x13, "と" },
            { 0x14, "な" },
            { 0x15, "に" },
            { 0x16, "ぬ" },
            { 0x17, "ね" },
            { 0x18, "の" },
            { 0x19, "は" },
            { 0x1A, "ひ" },
            { 0x1B, "ふ" },
            { 0x1C, "へ" },
            { 0x1D, "ほ" },
            { 0x1E, "ま" },
            { 0x1F, "み" },
            { 0x20, " " },
            { 0x21, "!" },
            { 0x22, "\"" },
            { 0x23, "む" },
            { 0x24, "め" },
            { 0x25, "%" },
            { 0x26, "&" },
            { 0x27, "'" },
            { 0x28, "(" },
            { 0x29, ")" },
            { 0x2A, "~" },
            { 0x2B, "+" },
            { 0x2C, "," },
            { 0x2D, "-" },
            { 0x2E, "." },
            { 0x2F, "/" },
            { 0x30, "0" },
            { 0x31, "1" },
            { 0x32, "2" },
            { 0x33, "3" },
            { 0x34, "4" },
            { 0x35, "5" },
            { 0x36, "6" },
            { 0x37, "7" },
            { 0x38, "8" },
            { 0x39, "9" },
            { 0x3A, ":" },
            { 0x3B, ";" },
            { 0x3C, "<" },
            { 0x3D, "=" },
            { 0x3E, ">" },
            { 0x3F, "?" },
            { 0x40, "@" },
            { 0x41, "A" },
            { 0x42, "B" },
            { 0x43, "C" },
            { 0x44, "D" },
            { 0x45, "E" },
            { 0x46, "F" },
            { 0x47, "G" },
            { 0x48, "H" },
            { 0x49, "I" },
            { 0x4A, "J" },
            { 0x4B, "K" },
            { 0x4C, "L" },
            { 0x4D, "M" },
            { 0x4E, "N" },
            { 0x4F, "O" },
            { 0x50, "P" },
            { 0x51, "Q" },
            { 0x52, "R" },
            { 0x53, "S" },
            { 0x54, "T" },
            { 0x55, "U" },
            { 0x56, "V" },
            { 0x57, "W" },
            { 0x58, "X" },
            { 0x59, "Y" },
            { 0x5A, "Z" },
            { 0x5B, "も" },
            { 0x5C, "\\" },
            { 0x5D, "や" },
            { 0x5E, "ゆ" },
            { 0x5F, "_" },
            { 0x60, "よ" },
            { 0x61, "a" },
            { 0x62, "b" },
            { 0x63, "c" },
            { 0x64, "d" },
            { 0x65, "e" },
            { 0x66, "f" },
            { 0x67, "g" },
            { 0x68, "h" },
            { 0x69, "i" },
            { 0x6A, "j" },
            { 0x6B, "k" },
            { 0x6C, "l" },
            { 0x6D, "m" },
            { 0x6E, "n" },
            { 0x6F, "o" },
            { 0x70, "p" },
            { 0x71, "q" },
            { 0x72, "r" },
            { 0x73, "s" },
            { 0x74, "t" },
            { 0x75, "u" },
            { 0x76, "v" },
            { 0x77, "w" },
            { 0x78, "x" },
            { 0x79, "y" },
            { 0x7A, "z" },
            { 0x7B, "ら" },
            { 0x7C, "り" },
            { 0x7D, "る" },
            { 0x7E, "れ" },
            { 0x7F, "�" }, // Cont Character
            { 0x80, "□" }, // Tag Character
            { 0x81, "。" },
            { 0x82, "｢" },
            { 0x83, "｣" },
            { 0x84, "、" },
            { 0x85, "･" },
            { 0x86, "ヲ" },
            { 0x87, "ァ" },
            { 0x88, "ィ" },
            { 0x89, "ゥ" },
            { 0x8A, "ェ" },
            { 0x8B, "ォ" },
            { 0x8C, "ャ" },
            { 0x8D, "ュ" },
            { 0x8E, "ョ" },
            { 0x8F, "ッ" },
            { 0x90, "ー" },
            { 0x91, "ア" },
            { 0x92, "イ" },
            { 0x93, "ウ" },
            { 0x94, "エ" },
            { 0x95, "オ" },
            { 0x96, "カ" },
            { 0x97, "キ" },
            { 0x98, "ク" },
            { 0x99, "ケ" },
            { 0x9A, "コ" },
            { 0x9B, "サ" },
            { 0x9C, "シ" },
            { 0x9D, "ス" },
            { 0x9E, "セ" },
            { 0x9F, "ソ" },
            { 0xA0, "タ" },
            { 0xA1, "チ" },
            { 0xA2, "ツ" },
            { 0xA3, "テ" },
            { 0xA4, "ト" },
            { 0xA5, "ナ" },
            { 0xA6, "ニ" },
            { 0xA7, "ヌ" },
            { 0xA8, "ネ" },
            { 0xA9, "ノ" },
            { 0xAA, "ハ" },
            { 0xAB, "ヒ" },
            { 0xAC, "フ" },
            { 0xAD, "ヘ" },
            { 0xAE, "ホ" },
            { 0xAF, "マ" },
            { 0xB0, "ミ" },
            { 0xB1, "ム" },
            { 0xB2, "メ" },
            { 0xB3, "モ" },
            { 0xB4, "ヤ" },
            { 0xB5, "ユ" },
            { 0xB6, "ヨ" },
            { 0xB7, "ラ" },
            { 0xB8, "リ" },
            { 0xB9, "ル" },
            { 0xBA, "レ" },
            { 0xBB, "ロ" },
            { 0xBC, "ワ" },
            { 0xBD, "ン" },
            { 0xBE, "ヴ" },
            { 0xBF, "☺" },
            { 0xC0, "ろ" },
            { 0xC1, "わ" },
            { 0xC2, "を" },
            { 0xC3, "ん" },
            { 0xC4, "ぁ" },
            { 0xC5, "ぃ" },
            { 0xC6, "ぅ" },
            { 0xC7, "ぇ" },
            { 0xC8, "ぉ" },
            { 0xC9, "ゃ" },
            { 0xCA, "ゅ" },
            { 0xCB, "ょ" },
            { 0xCC, "っ" },
            { 0xCD, "\n" },
            { 0xCE, "ガ" },
            { 0xCF, "ギ" },
            { 0xD0, "グ" },
            { 0xD1, "ゲ" },
            { 0xD2, "ゴ" },
            { 0xD3, "ザ" },
            { 0xD4, "ジ" },
            { 0xD5, "ズ" },
            { 0xD6, "ゼ" },
            { 0xD7, "ゾ" },
            { 0xD8, "ダ" },
            { 0xD9, "ヂ" },
            { 0xDA, "ヅ" },
            { 0xDB, "デ" },
            { 0xDC, "ド" },
            { 0xDD, "バ" },
            { 0xDE, "ビ" },
            { 0xDF, "ブ" },
            { 0xE0, "ベ" },
            { 0xE1, "ボ" },
            { 0xE2, "パ" },
            { 0xE3, "ピ" },
            { 0xE4, "プ" },
            { 0xE5, "ペ" },
            { 0xE6, "ポ" },
            { 0xE7, "が" },
            { 0xE8, "ぎ" },
            { 0xE9, "ぐ" },
            { 0xEA, "げ" },
            { 0xEB, "ご" },
            { 0xEC, "ざ" },
            { 0xED, "じ" },
            { 0xEE, "ず" },
            { 0xEF, "ぜ" },
            { 0xF0, "ぞ" },
            { 0xF1, "だ" },
            { 0xF2, "ぢ" },
            { 0xF3, "づ" },
            { 0xF4, "で" },
            { 0xF5, "ど" },
            { 0xF6, "ば" },
            { 0xF7, "び" },
            { 0xF8, "ぶ" },
            { 0xF9, "べ" },
            { 0xFA, "ぼ" },
            { 0xFB, "ぱ" },
            { 0xFC, "ぴ" },
            { 0xFD, "ぷ" },
            { 0xFE, "ぺ" },
            { 0xFF, "ぽ" },
        };

        public static string[] DnMe_Plus_Kanji_Bank_0 = new string[256]
        {
            "😮", "😄", "😠", "😣", "🐾", "⚷", "✉", "💰", "★", "☀", "☁", "☂", "☃", "🌬", "×", "÷",
            "🐶", "🐱", "🐰", "🐦", "🐮", "🐷", "🐟", "🐞", "💀", "🍀", "ひ", "○", "🗙", "□", "△", "=", // ひ = Gyroid
            "予", "岸", "向", "定", "写", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ",
            "貝", "耳", "川", "虫", "入", "王", "学", " ", " ", " ", " ", " ", " ", " ", " ", " ",
            "帰", "魚", "教", "兄", "細", "姉", "走", "太", "体", "池", "弟", "当", "歩", "妹", "南", "頭",
            "育", "寒", "急", "橋", "決", "者", "勝", "送", "想", "島", "悲", "負", "勉", "放", "重", "他",
            "愛", "改", "完", "芸", "成", "選", "置", "働", "別", "望", "満", "冷", "辺", "熱", "必", "要",
            "久", "術", "増", "貸", "断", "築", "預", "接", "解", "準", "備", "保", "設", " ", " ", " ",
            "危", "困", "砂", "姿", "捨", "操", "届", "収", "泉", "呼", "除", "存", " ", " ", " ", " ",
            "釣", "捕", "怒", "舟", "浜", "誰", "恋", "払", "贈", "頼", "渡", "残", "埋", "押", "込", "深",
            "通", "同", "止", "語", "答", "活", "首", "遠", " ", " ", " ", " ", " ", " ", " ", " ",
            "飲", "消", "落", "助", "暗", "化", "遊", "漢", "祭", "級", "去", "次", "流", "発", "旅", "深",
            "続", "治", "量", "不", "照", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ",
            " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ",
            " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ",
            "越", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " "
        };

        public static string[] DnMe_Plus_Kanji_Bank_1 = new string[256]
        {
            "右", "雨", "音", "下", "火", "花", "一", "気", "休", "金", "空", "月", "見", "ロ", "左", "山",
            "子", "字", "天", "車", "手", "出", "女", "小", "上", "森", "人", "水", "正", "生", "青", "タ",
            "石", "赤", "声", "先", "早", "草", "足", "村", "大", "男", "中", "館", "町", "土", "日", "年",
            "白", "文", "木", "本", "名", "目", "立", "力", "引", "何", "夏", "家", "歌", "画", "回", "会",
            "海", "絵", "外", "楽", "間", "岩", "顔", "汽", "記", "仕", "根", "強", "由", "近", "具", "計",
            "元", "言", "原", "戸", "午", "後", "広", "交", "光", "考", "行", "高", "黄", "合", "黒", "今",
            "作", "算", "代", "思", "紙", "自", "時", "弱", "秋", "週", "春", "書", "少", "場", "色", "食",
            "心", "新", "親", "図", "数", "星", "晴", "切", "雪", "前", "多", "台", "地", "起", "知", "昼",
            "長", "朝", "直", "道", "店", "雷", "冬", "読", "売", "買", "番", "分", "聞", "方", "毎", "財",
            "明", "夜", "野", "友", "用", "躍", "来", "話", "安", "源", "意", "員", "運", "駅", "屋", "荷",
            "階", "感", "期", "世", "曲", "局", "使", "始", "事", "持", "実", "受", "終", "集", "住", "所",
            "商", "植", "真", "全", "相", "待", "題", "短", "議", "着", "注", "丁", "度", "室", "動", "配",
            "初", "美", "秒", "品", "部", "服", "福", "物", "返", "味", "面", "問", "有", "葉", "縁", "礼",
            "開", "以", "貨", "願", "季", "好", "最", "昨", "残", "笑", "信", "節", "達", "貯", "伝", "念",
            "建", "変", "便", "末", "無", "良", "確", "券", "雑", "性", "界", "暴", "穴", "病", "社", "座",
            "誕", "段", "値", "取", "認", "宝", "忘", "郵", "欲", "冗", "録", "博", "仲", "利", "似", "応"
        };

        public static Dictionary<byte, string> ContId_Map = new Dictionary<byte, string> // Cont stands for Control
        {
            { 0x00, "<End Conversation>" }, // "Last" Code
            { 0x01, "<Goto Jump Entry>" }, // "Continue" Code
            { 0x02, "<New Page>" }, 
            { 0x03, "<Pause [{0}]>" }, // "SetTime" Code
            { 0x04, "<Press A>" }, // Might be "bring up next arrow"
            { 0x05, "<Color Line [{0}]>" }, // "Color" Code
            { 0x06, "<Instant Text Skip>" }, // Persists for the entire dialog duration
            { 0x07, "<Unskippable>" }, // Unsure about this one
            { 0x08, "<Player Emotion [{0}] [{1}]>" }, // (mMsg_Main_Cursol_SetDemoOrderPlayer_ControlCursol) (Affects Player_actor_RecieveDemoOrder_EffectOrder) (Player_actor_ChangeAnimation_FromDemoOrder_Talk)
            { 0x09, "<Expression [{0}]>" }, // NPC Expressions (mMsg_Main_Cursol_SetDemoOrderNPC0_ControlCursol) (aNPC_check_manpu_demoCode)
            //{ 0x0A, "<Unknown ContId 0x0A>" }, // Three extra bytes
            //{ 0x0B, "<Unknown ContId 0x0B>" }, // Three extra bytes
            //{ 0x0C, "<Unknown ContId 0x0C>" }, // "Quest" Code (See mMsg_Main_Cursol_SetDemoOrderQuest_ControlCursol)
            { 0x0D, "<Open Choice Selection Menu>" },
            { 0x0E, "<Set Jump Entry [{0}]>" },
            { 0x0F, "<Choice #1 MessageId [{0}]>" }, // Choice #1? (+ two bytes / one ushort) might point to the choice index to use (hex)
            { 0x10, "<Choice #2 MessageId [{0}]>" }, // Choice #2?
            { 0x11, "<Choice #3 MessageId [{0}]>" }, // Choice #3?
            { 0x12, "<Choice #4 MessageId [{0}]>" }, // Same as above
            { 0x13, "<Set 2 Random Jump Entries [{0}] [{1}]>" }, // Selects one of the entries to jump to (mMsg_Main_Cursol_SetNextMessageRandom2)
            { 0x14, "<Set 3 Random Jump Entries [{0}] [{1}] [{2}]>" }, // (mMsg_Main_Cursol_SetNextMessageRandom3)
            { 0x15, "<Set 4 Random Jump Entries [{0}] [{1}] [{2}] [{3}]>" }, // (mMsg_Main_Cursol_SetNextMessageRandom4)
            { 0x16, "<Set 2 Choices [{0}] [{1}]>" }, // Extra 4 bytes
            { 0x17, "<Set 3 Choices [{0}] [{1}] [{2}]>" }, // Extra 6 bytes
            { 0x18, "<Set 4 Choices [{0}] [{1}] [{2}] [{3}]>" }, // Extra 8 bytes
            { 0x19, "<End of Choices>" }, // Unsure about this one (could also be display 1 character) (auto skips to next text box)
            { 0x1A, "<Player Name>" },
            { 0x1B, "<NPC Name>" },
            { 0x1C, "<Catchphrase>" },
            { 0x1D, "<Year>" },
            { 0x1E, "<Month>" },
            { 0x1F, "<Day of Week>" },
            { 0x20, "<Day>" },
            { 0x21, "<Hour>" },
            { 0x22, "<Minute>" },
            { 0x23, "<Second>" },
            { 0x24, "<String 0>" }, // Strings are placed on the stack by the code that runs the current text sequence
            { 0x25, "<String 1>" },
            { 0x26, "<String 2>" },
            { 0x27, "<String 3>" },
            { 0x28, "<String 4>" }, // Might be current memory card
            { 0x29, "<String 5>" },
            { 0x2A, "<String 6>" },
            { 0x2B, "<String 7>" },
            { 0x2C, "<String 8>" },
            { 0x2D, "<String 9>" },
            { 0x2E, "<Last Choice Selected>" },
            { 0x2F, "<Town Name>" },
            { 0x30, "<Random Number>" }, // 0 - 99
            { 0x31, "<Item 0>" },
            { 0x32, "<Item 1>" },
            { 0x33, "<Item 2>" },
            { 0x34, "<Item 3>" },
            { 0x35, "<Item 4>" },
            { 0x36, "<Item 5>" },
            { 0x37, "<Item 6>" },
            { 0x38, "<String 10>" },
            { 0x39, "<String 11>" },
            { 0x3A, "<String 12>" },
            { 0x3B, "<String 13>" },
            { 0x3C, "<String 14>" },
            { 0x3D, "<String 15>" },
            { 0x3E, "<String 16>" },
            { 0x3F, "<String 17>" },
            { 0x40, "<Show Gyroid Message>" },
            { 0x41, "<Neutral Luck>" },
            { 0x42, "<Relationship Luck>" },
            { 0x43, "<Unpopular Luck>" },
            { 0x44, "<Bad Luck>" },
            { 0x45, "<Bell Luck>" },
            { 0x46, "<Item Luck>" },
            { 0x4B, "<Villager Normal State>" },
            { 0x4C, "<Villager Angry State>" },
            { 0x4D, "<Villager Sad State>" },
            { 0x4E, "<Villager Happy State>" },
            { 0x4F, "<Villager Sleepy State>" },
            { 0x50, "<Color [{0}] Characters [{1}]>" }, // RGB in hex follows, then the character count
            { 0x51, "<Actor Speech Type [{0}]>" },
            { 0x52, "<Line Offset [{0}]>" }, // Sets how far down the reset of the line is
            { 0x53, "<Line Type [{0}]>" }, // "LineType" Code (mMsg_Main_Cursol_SetLineType_ControlCursol)
            { 0x54, "<Next Character Size [{0}]>" },
            { 0x55, "<Press A (No Sound)>" }, // (mMsg_Main_Cursol_Button2_ControlCursol)
            { 0x56, "<Play Music [{0}] [{1}]>" }, // Music Id, Transition Type
            { 0x57, "<Stop Music [{0}] [{1}]>" },
            { 0x58, "<End Conversation after [{0}]>" }, // "MsgTimeEnd" Code
            { 0x59, "<Play Sound Effect [{0}]>" }, // (See _mMsg_Get_sound_trg_sys_forData) (mMsg_Main_Cursol_SoundTrgSys_ControlCursol) (Appears to be clamped to a max of 0x07) (3 and 4 are special?)
            { 0x5A, "<Line Size [{0}]>" },
            { 0x5B, "<Game Sets Jump Entry>" }, // The current routine automatically sets the jump entry
            { 0x5C, "<Main Nookling>" },
            { 0x5D, "<Secondary Nookling>" },
            { 0x5E, "<B Button Selects Last Choice>" },
            // 0x5F (GiveOpen) (mMsg_Main_Cursol_GiveOpen_ControlCursol)
            // 0x60 (GiveClothes) (mMsg_Main_Cursol_GiveClose_ControlCursol)
            { 0x61, "<Set NPC Voice Gloomy>" }, // Changes the tone for the NPC's voice to be "gloomy" until the dialog ends (mMsg_Main_Cursol_SetMessageContentsGloomy_ControlCursol)
            { 0x62, "<B Button Cannot Close Menu>" }, // mMsg_Main_Cursol_SelectNoBClose_Control_Cursol
            { 0x63, "<Set Random Jump Entry Section [{0}] [{1}]>" }, // Sets a random jump entry between the first and second arguments (second argument must be greater than or equal to the first argument's message id!) (mMsg_Main_Cursol_SetNextMessageRamdomSection_ControlCursol)
            // 0x64 (mMsg_Main_Cursol_AgbDummy_ControlCursol) (Probably something to do with the Gameboy Advance connection [Agb = Advance Gameboy])
            // 0x65 (Same as above)
            // 0x66 (Same as above 2)
            { 0x67, "<Character Margin [{0}]>" },
            // 0x68 (Same as 0x64 > 0x66)
            // 0x69 (Same as above)
            { 0x6A, "<Set Gendered Jump Entry [{0}] [{1}]>" }, // (First Argument is the Male MessageId, Second Argument is the Female MessageId) (mMsg_Main_Cursol_MaleFemaleCheck_ControlCursol)
            // 0x6B (Same as 0x68)
            // 0x6C (Same as above)
            // 0x6D (Same as above)
            // 0x6E (Same as above)
            // 0x6F (Same as above)
            // 0x70 (Same as above)
            { 0x71, "<Island Name>" },
            //{ 0x72, "<Indent Line>" }, // Adds an indentation (tab) (mMsg_Main_Cursol_SetCursolJust_ControlCursol)
            //{ 0x73, "<End Line Indentation>" }, // I don't know what this does, it acts the same as above (mMsg_Main_Cursol_ClrCursolJust_ControlCursol)
            //{ 0x74, "<Transferred Item>" }, // (mMsg_Main_Cursol_CutArticle_ControlCursol)
            //{ 0x75, "" }, // (mMsg_Cursol_CapitalLetter_ControlCursol)
            { 0x76, "<AM/PM>" },
            { 0x77, "<Choice #5 MessageId [{0}]>" },
            { 0x78, "<Choice #6 MessageId [{0}]>" },
            { 0x79, "<Set 5 Choices [{0}] [{1}] [{2}] [{3}] [{4}]>" },
            { 0x7A, "<Set 6 Choices [{0}] [{1}] [{2}] [{3}] [{4}] [{5}]>" }
            // 7A = Last Control Code? (Found in mFont_CodeSize_get)
        };

        public static Dictionary<int, string> Expression_List = new Dictionary<int, string> // Might actually have "banks" but we're just gonna use shorts for now
        {
            { 0x00, "None?" },
            { 0x01, "Glare (Red Bolt)" },
            { 0x02, "Shocked" },
            { 0x03, "Laugh" },
            { 0x04, "Surprised" },
            { 0x05, "Angry" },
            { 0x06, "Excitement (Exclamation Mark)" },
            { 0x07, "Worried" },
            { 0x08, "Scared" },
            { 0x09, "Cry" },
            { 0x0A, "Happy" },
            { 0x0B, "Wondering (Question Mark)" },
            { 0x0C, "Idea (Light Bulb)" },
            { 0x0D, "Sad (Wind Blows)" },
            { 0x0E, "Happy (Dance)" },
            { 0x0F, "Thinking" },
            { 0x10, "Depressed (Rain Cloud)" },
            { 0x11, "Heartbroken" },
            { 0x12, "Sinister" },
            { 0x13, "Tired" },
            { 0x14, "Love" },
            { 0x15, "Smile" },
            { 0x16, "Scowl" },
            { 0x17, "Frown" },
            { 0x18, "Laughing (Sitting)" },
            { 0x19, "Shocked (Sitting)" },
            { 0x1A, "Idea (Sitting)" },
            { 0x1B, "Surprised (Sitting)" },
            { 0x1C, "Angry (Sitting)" },
            { 0x1D, "Smile (Sitting)" },
            { 0x1E, "Frown (Sitting)" },
            { 0x1F, "Wondering (Sitting)" },
            { 0x20, "Salute" },
            { 0x21, "Angry (Resetti)" },
            { 0x22, "Reset Expressions (Resetti)" },
            { 0x23, "Sad (Resetti)" },
            { 0x24, "Excitement (Resetti)" },
            { 0x25, "Jaw Drop (Resetti)" },
            { 0x26, "Annoyed (Resetti)" },
            { 0x27, "Furious (Resetti)" },
            { 0x28, "Surprised (K.K.)" },
            { 0x29, "Fortune" },
            { 0x2A, "Smile (Resetti)" },
            // 18 - 1F = rover emotions
            // 20 = copper salute
            // 21 - 27 = resetti emotions
            // 28 KK emotion when telling you he almost forgot (new town creation)
            // 29 = katrina's fortune emotion
            // 2A = resetti emotion
            // 2B = nook running emotion
            // 2D - 31 = emotions without sound effects
            { 0xFD, "Reset Expressions (K.K.)" },
            { 0xFE, "Reset Expressions (Sitting)" },
            { 0xFF, "Reset Expressions" }
            // More (can be found here here: https://www.youtube.com/watch?v=PEeyGxR6NWs)
        };

        public static Dictionary<ushort, string> Player_Emotions = new Dictionary<ushort, string>
        {
            { 0x02, "Surprised" },
            { 0xFD, "Purple Mist" }, // Sick Emotion?
            { 0xFE, "Scared" },
            { 0xFF, "Reset Emotion" }
        };

        public static Dictionary<byte, string> Music_List = new Dictionary<byte, string>
        {
            { 0x00, "Silence" },
            { 0x01, "Arriving in Town" },
            { 0x02, "House Selection" },
            { 0x03, "House Selected" },
            { 0x04, "House Selected (2)" }, // From after you hand Nook the 1,000 bells
            { 0x05, "Resetti" },
            { 0x06, "Current Hourly Music" },
            { 0x07, "Resetti (2)" }, // From after the "Fake Reset" screen transition
            { 0x08, "Don Resetti" }
        };

        public static Dictionary<byte, string> Music_Transitions = new Dictionary<byte, string>
        {
            { 0x00, "None" },
            { 0x01, "Undetermined" },
            { 0x02, "Fade" }
        };

        public static Dictionary<byte, string> SoundEffect_List = new Dictionary<byte, string>
        {
            { 0x00, "Bell Transaction" },
            { 0x01, "Happy" },
            { 0x02, "Very Happy" },
            { 0x03, "Variable 0" }, // 03 and 04 are some special case (the code handles them differently)
            { 0x04, "Variable 1" },
            { 0x05, "Annoyed" }, // Resetti
            { 0x06, "Thunder" }, // Resetti
            { 0x07, "None" } // Doesn't produce a sound effect and anything greater than 07 is clamped to 07
        };

        /*
         * Doubutsu no Mori e+ Tag Map
         */

        public static Dictionary<byte, Dictionary<ushort, string>> Tag_Map = new Dictionary<byte, Dictionary<ushort, string>>
        {
            { 0x01, new Dictionary<ushort, string>
            {
                { 0x0001, "<End Conversation>" },
                { 0x0002, "<Switch to Selected Dialog>" },
                { 0x0003, "<New Page>" },
                { 0x0004, "<Pause [{0}]>" },
                { 0x0005, "<Required Kanji Level [{0}]>" },
                { 0x000B, "<End Conversation after [{0}]>" }
                //{ 0x001B, "<End Conversation after [{0}]>" }
            }
            },
            { 0x02, new Dictionary<ushort, string>
            {
                { 0x0000, "<Set 2 Choices [{0}] [{1}]>" },
                { 0x0001, "<Set 3 Choices [{0}] [{1}] [{2}]>" },
                { 0x0002, "<Set 4 Choices [{0}] [{1}] [{2}] [{3}]>" },
                { 0x0005, "<Choice #1 MessageId [{0}]>" },
                { 0x0006, "<Choice #2 MessageId [{0}]>" },
                { 0x0007, "<Choice #3 MessageId [{0}]>" },
                { 0x0008, "<Choice #4 MessageId [{0}]>" }
            }
            },
            { 0x04, new Dictionary<ushort, string>
            {
                { 0x0000, "<Player Name>" },
                { 0x0001, "<NPC Name>" },
                { 0x0002, "<Catchphrase>" },
                { 0x0003, "<Last Choice Selected>" },
                { 0x0004, "<Gyroid Message>" },
                { 0x0005, "<Town Name>" },
                { 0x0006, "<Island Name>" },
                { 0x0007, "<Year>" },
                { 0x0008, "<Month>" },
                { 0x0009, "<Day of Week>" },
                { 0x000A, "<Day>" },
                { 0x000B, "<Hour>" },
                { 0x000C, "<Minute>" },
                { 0x000D, "<Second>" },
                { 0x000E, "<Random Number>" },
                { 0x0012, "<String 0>" },
                { 0x0013, "<String 1>" },
                { 0x0014, "<String 2>" },
                { 0x0015, "<String 3>" },
                { 0x0016, "<String 4>" },
                { 0x0017, "<String 5>" },
                { 0x0018, "<String 6>" },
                { 0x0019, "<String 7>" },
                { 0x001A, "<String 8>" },
                { 0x001B, "<String 9>" },
                { 0x001C, "<Unknown Mail Variable 1>" },
                { 0x001D, "<Unknown Mail Variable 2>" },
                { 0x001E, "<Unknown Mail Variable 3>" },
                { 0x001F, "<Unknown Mail Variable 4>" },
                { 0x0020, "<Year>" },
                { 0x0021, "<Month>" },
                { 0x0022, "<Day>" },
                { 0x0023, "<Year (2)>" },
                { 0x0024, "<Day (2)>" },
                { 0x0025, "<Unknown Mail Variable 5>" },
                { 0x0026, "<String 10>" },
                { 0x0027, "<String 11>" },
                { 0x0028, "<String 12>" },
                { 0x0029, "<String 13>" },
                { 0x002A, "<String 14>" },
            }
            },
            { 0x05, new Dictionary<ushort, string>
            {
                { 0x0000, "<Press A>" },
                { 0x0002, "<B Button Selects Last Choice>" },
                { 0x0004, "<Instantly Skippable Text>" },
                { 0x0005, "<Unskippable Text>" },
                { 0x0006, "<Open Choice Selection Menu>" },
                { 0x0007, "<End of Choices>" }
            }
            },
            { 0x06, new Dictionary<ushort, string>
            {
                { 0x0000, "<Clear Expression>" },
                { 0x0001, "<Expression [Glare]>" },
                { 0x0002, "<Expression [Shocked]>" },
                { 0x0003, "<Expression [Laugh]>" },
                { 0x0004, "<Expression [Surprised]>" },
                { 0x0005, "<Expression [Angry]>" },
                { 0x0006, "<Expression [Excited]>" },
                { 0x0007, "<Expression [Worried]>" },
                { 0x0008, "<Expression [Scared]>" },
                { 0x0009, "<Expression [Cry]>" },
                { 0x000A, "<Expression [Happy]>" },
                { 0x000B, "<Expression [Wondering]>" },
                { 0x000C, "<Expression [Idea]>" },
                { 0x000D, "<Expression [Sad]>" },
                { 0x000E, "<Expression [Happy Dance]>" },
                { 0x000F, "<Expression [Thinking]>" },
                { 0x0010, "<Expression [Depressed]>" },
                { 0x0011, "<Expression [Heartbroken]>" },
                { 0x0012, "<Expression [Sinister]>" },
                { 0x0013, "<Expression [Tired]>" },
                { 0x0014, "<Expression [Love]>" },
                { 0x0015, "<Expression [Smile]>" },
                { 0x0016, "<Expression [Scowl]>" },
                { 0x0017, "<Expression [Frown]>" },
                { 0x0018, "<Clear Expression (K.K.)>" },
                { 0x0023, "<Expression [Angry] (Resetti)" },
                { 0x0024, "<Clear Expression (Resetti)>" },
                { 0x002A, "<Expression [Furious] (Resetti)>" },
                { 0x002B, "<Expression [Surprised] (K.K.)>" }
            }
            },
            { 0x08, new Dictionary<ushort, string>
            {
                { 0x0000, "<Neutral Luck>" },
                { 0x0001, "<Relationship Luck>" },
                { 0x0002, "<Unpopular Luck>" },
                { 0x0003, "<Bad Luck>" },
                { 0x0004, "<Bell Luck>" },
                { 0x0005, "<Item Luck>" },
                { 0x0006, "<Player Emotion [UNKNOWN]>" },
                { 0x0008, "<Player Emotion [UNKNOWN 2]>" },
                { 0x0009, "<Clear Player Emotion>" },
            }
            },
            { 0x09, new Dictionary<ushort, string>
            {
                { 0x0000, "<Secondary Nookling>" },
                { 0x0001, "<Main Nookling>" }
            }
            },
            { 0x0A, new Dictionary<ushort, string>
            {
                { 0x0006, "<Sound Cut Off>" }, // Investigate these
                { 0x0007, "<Sound Cut On>" }
            }
            },
            { 0xFF, new Dictionary<ushort, string>
            {
                { 0x0000, "<Line Color Index [{0}]>" }, // Might not be only "next character"
                { 0x0001, "<Line Size [{0}]>" }, // Ends at the next Line Size or end of line?
                { 0x0002, "<Next Character replaced with [{0} ]>" } // Only if the player's Kanji Level is greater than or equal to the previously set one
                // TODO: Figure out what the extra bytes at the end of the Next Character replaced with command does
            }
            }
        };

        public static Dictionary<byte, string> Character_Map = Animal_Crossing_Character_Map; // Current Character Map
        public static Dictionary<byte, int> Cont_Id_Appearance = new Dictionary<byte, int>();

        public static string Decode(byte[] Data)
        {
            string Text = "";
            for (int i = 0; i < Data.Length; i++)
            {
                byte Current_Byte = Data[i];
                if (Current_Byte == 0x7F) // "Cont" Character
                {
                    if (i + 1 < Data.Length)
                    {
                        byte Cont_Param = Data[i + 1];
                        if (Cont_Param < 0x7B)
                        {
                            i++;
                            switch (Cont_Param)
                            {
                                case 0x03:
                                    Text += string.Format(ContId_Map[0x03], Data[i + 1]);
                                    i++;
                                    break;
                                case 0x05:
                                    byte R = Data[i + 1];
                                    byte G = Data[i + 2];
                                    byte B = Data[i + 3];
                                    i += 3;
                                    Text += string.Format(ContId_Map[0x05], ((R << 16) | (G << 8) | B).ToString("X6"));
                                    break;
                                case 0x08:
                                    byte Modifier = Data[i + 1];
                                    ushort Emotion = (ushort)((Data[i + 2] << 8) | Data[i + 3]);
                                    if (Player_Emotions.ContainsKey(Emotion))
                                    {
                                        Text += string.Format(ContId_Map[Cont_Param], Player_Emotions[Emotion], Modifier.ToString("X2")); // TODO: Modifier list (and encoding)
                                    }
                                    else
                                    {
                                        Text += string.Format(ContId_Map[Cont_Param], "Unknown Emotion 0x" + Emotion.ToString("X4"), Modifier.ToString("X2"));
                                    }
                                    break;
                                case 0x09:
                                    int Expression = (Data[i + 1] << 16) | (Data[i + 2] << 8) | Data[i + 3];
                                    if (Expression_List.ContainsKey(Expression))
                                        Text += string.Format(ContId_Map[0x09], Expression_List[Expression]);
                                    else
                                        Text += string.Format(ContId_Map[0x09], "Unknown 0x" + Expression.ToString("X6"));
                                    i += 3;
                                    break;
                                case 0x0E:
                                case 0x0F:
                                case 0x10:
                                case 0x11:
                                case 0x12:
                                case 0x77:
                                case 0x78:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString("X2") + Data[i + 2].ToString("X2"));
                                    i += 2;
                                    break;
                                case 0x13:
                                case 0x16:
                                case 0x63:
                                case 0x6A:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString("X2") + Data[i + 2].ToString("X2"), Data[i + 3].ToString("X2") + Data[i + 4].ToString("X2"));
                                    i += 4;
                                    break;
                                case 0x14:
                                case 0x17:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString("X2") + Data[i + 2].ToString("X2"), Data[i + 3].ToString("X2") + Data[i + 4].ToString("X2"), Data[i + 5].ToString("X2") + Data[i + 6].ToString("X2"));
                                    i += 6;
                                    break;
                                case 0x15:
                                case 0x18:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString("X2") + Data[i + 2].ToString("X2"), Data[i + 3].ToString("X2") + Data[i + 4].ToString("X2"), Data[i + 5].ToString("X2") + Data[i + 6].ToString("X2"), Data[i + 7].ToString("X2") + Data[i + 8].ToString("X2"));
                                    i += 8;
                                    break;
                                case 0x50:
                                    byte R1 = Data[i + 1];
                                    byte G1 = Data[i + 2];
                                    byte B1 = Data[i + 3];
                                    byte Length = Data[i + 4];
                                    i += 4;
                                    Text += string.Format(ContId_Map[0x50], Length, ((R1 << 16) | (G1 << 8) | B1).ToString("X6"));
                                    break;
                                case 0x56:
                                case 0x57:
                                    Text += string.Format(ContId_Map[Cont_Param], Music_List.ContainsKey(Data[i + 1]) ? Music_List[Data[i + 1]] : Music_List[0], Music_Transitions.ContainsKey(Data[i + 2]) ? Music_Transitions[Data[i + 2]] : Music_Transitions[0]);
                                    i += 2;
                                    break;
                                case 0x59:
                                    Text += string.Format(ContId_Map[Cont_Param], SoundEffect_List.ContainsKey(Data[i + 1]) ? SoundEffect_List[Data[i + 1]] : SoundEffect_List[7]);
                                    i++;
                                    break;
                                case 0x51:
                                case 0x52:
                                case 0x53:
                                case 0x54:
                                case 0x58:
                                case 0x5A:
                                case 0x67:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString());
                                    i++;
                                    break;
                                case 0x79:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString("X2") + Data[i + 2].ToString("X2"), Data[i + 3].ToString("X2") + Data[i + 4].ToString("X2"),
                                        Data[i + 5].ToString("X2") + Data[i + 6].ToString("X2"), Data[i + 7].ToString("X2") + Data[i + 8].ToString("X2"), Data[i + 9].ToString("X2") + Data[i + 10].ToString("X2"));
                                    i += 10;
                                    break; // TODO: Encoding
                                case 0x7A:
                                    Text += string.Format(ContId_Map[Cont_Param], Data[i + 1].ToString("X2") + Data[i + 2].ToString("X2"), Data[i + 3].ToString("X2") + Data[i + 4].ToString("X2"),
                                        Data[i + 5].ToString("X2") + Data[i + 6].ToString("X2"), Data[i + 7].ToString("X2") + Data[i + 8].ToString("X2"), Data[i + 9].ToString("X2") + Data[i + 10].ToString("X2"),
                                        Data[i + 11].ToString("X2") + Data[i + 12].ToString("X2"));
                                    i += 12;
                                    break; // TODO: Encoding
                                default:
                                    if (ContId_Map.ContainsKey(Cont_Param))
                                    {
                                        Text += ContId_Map[Cont_Param];
                                    }
                                    else
                                    {
                                        Text += string.Format("<Unknown ContId 0x{0}>", Cont_Param.ToString("X2"));
                                        if (Cont_Id_Appearance.ContainsKey(Cont_Param))
                                            Cont_Id_Appearance[Cont_Param] += 1;
                                        else
                                            Cont_Id_Appearance[Cont_Param] = 1;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ContId was out of bounds!");
                    }
                }
                else if (Current_Byte == 0x80 && MainWindow.IsBMG)
                {
                    byte Size = Data[i + 1];
                    if (Size > 4)
                    {
                        byte Group = Data[i + 2];
                        ushort Index = (ushort)((Data[i + 3] << 8) | Data[i + 4]);

                        if (Tag_Map.ContainsKey(Group) && Tag_Map[Group].ContainsKey(Index))
                        {
                            string Description = Tag_Map[Group][Index];
                            switch (Group)
                            {
                                case 0x01:
                                    switch (Index)
                                    {
                                        case 0x0004:
                                        case 0x000B:
                                            Text += string.Format(Description, (ushort)((Data[i + 5] << 8) | Data[i + 6]));
                                            break;
                                        case 0x0005:
                                            Text += string.Format(Description, (Data[i + 5] > 0x09 ? Data[i + 5] - 0x0A : Data[i + 5]));
                                            break;
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                case 0x02:
                                    switch (Index)
                                    {
                                        case 0x0000:
                                            Text += string.Format(Description, ((ushort)((Data[i + 5] << 8) | Data[i + 6])).ToString("X4"), ((ushort)((Data[i + 7] << 8) | Data[i + 8])).ToString("X4"));
                                            break;
                                        case 0x0001:
                                            Text += string.Format(Description, ((ushort)((Data[i + 5] << 8) | Data[i + 6])).ToString("X4"), ((ushort)((Data[i + 7] << 8) | Data[i + 8])).ToString("X4"),
                                                ((ushort)((Data[i + 9] << 8) | Data[i + 10])).ToString("X4"));
                                            break;
                                        case 0x0002:
                                            Text += string.Format(Description, ((ushort)((Data[i + 5] << 8) | Data[i + 6])).ToString("X4"), ((ushort)((Data[i + 7] << 8) | Data[i + 8])).ToString("X4"),
                                                ((ushort)((Data[i + 9] << 8) | Data[i + 10])).ToString("X4"), ((ushort)((Data[i + 11] << 8) | Data[i + 12])).ToString("X4"));
                                            break;
                                        case 0x0005:
                                        case 0x0006:
                                        case 0x0007:
                                        case 0x0008:
                                            Text += string.Format(Description, ((ushort)((Data[i + 5] << 8) | Data[i + 6])).ToString("X4"));
                                            break;
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                case 0x04:
                                    switch (Index)
                                    {
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                case 0x05:
                                    switch (Index)
                                    {
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                case 0x09:
                                    switch (Index)
                                    {
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                case 0x0A:
                                    switch (Index)
                                    {
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                case 0xFF:
                                    switch (Index)
                                    {
                                        case 0x0000:
                                            Text += string.Format(Description, Data[i + 5].ToString());
                                            break;
                                        case 0x0001:
                                            Text += string.Format(Description, ((ushort)((Data[i + 5] << 8) | Data[i + 6])).ToString());
                                            break;
                                        case 0x0002:
                                            if (i + Size < Data.Length)
                                            {
                                                Text += string.Format(Description, (Data[i + 5] == 0 ? DnMe_Plus_Kanji_Bank_0[Data[i + Size]] : DnMe_Plus_Kanji_Bank_1[Data[i + Size]])); // This is still not right. Need to figure it out.
                                            }
                                            else
                                            {
                                                Text += Description;
                                            }
                                            break;
                                        default:
                                            Text += Description;
                                            break;
                                    }
                                    break;
                                default:
                                    Text += Description;
                                    break;
                            }
                        }
                        else
                        {
                            Text += string.Format("<Tag Size={0} Group={1} Index={2} Params={3}>", Size.ToString("X2"), Data[i + 2].ToString("X2"), ((Data[i + 3] << 8) | Data[i + 4]).ToString("X4"),
                                BitConverter.ToString(Data.Skip(i + 5).Take(Size - 5).ToArray()).Replace("-", ""));
                        }
                        i += Size - 1;
                    }
                    else
                    {
                        Debug.WriteLine("0x80 Param Size was less than 3! Size: " + Size);
                    }
                }
                else if (Character_Map.ContainsKey(Current_Byte))
                {
                    Text += Character_Map[Current_Byte];
                }
                else
                {
                    Text += Encoding.ASCII.GetString(new byte[1] { Current_Byte });
                }
            }
            return Text;
        }

        public static byte[] Encode(string Text, File_Type Character_Set_Type)
        {
            List<byte> Data = new List<byte>();
            Dictionary<byte, string> Character_Map = Character_Set_Type == File_Type.Animal_Crossing
                ? Animal_Crossing_Character_Map : Doubutsu_no_Mori_Plus_Character_Map;

            for (int i = 0; i < Text.Length; i++)
            {
                string Character = Text[i].ToString();
                bool ContId_Set = false;
                if (Character.Equals("<"))
                {
                    int Index = Text.IndexOf(">", i + 1);
                    if (Index > -1)
                    {
                        string Cont_Id = Text.Substring(i, Index - i + 1).ToLower();
                        if ((Cont_Id.Contains("[") && !Cont_Id.Contains("]")) || (Cont_Id.Contains("]") && !Cont_Id.Contains("[")))
                            MessageBox.Show("Error: Incomplete argument detected! Please ensure that your arguments are structured like this: [Argument]");

                        var Matches = Regex.Matches(Cont_Id, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList(); // Strip arguments
                        int Count = 0;
                        Cont_Id = Regex.Replace(Cont_Id, @"\[.+?\]", m => "[{" + Count++ +"}]"); // Turn ContId into it's argumentless form

                        var Match = ContId_Map.FirstOrDefault(o => o.Value.ToLower().Equals(Cont_Id)); // Value will be null/empty string if no match is found
                        if (!string.IsNullOrEmpty(Match.Value))
                        {
                            var Actual_Count = Regex.Matches(Match.Value, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList().Count;
                            if (Count < Actual_Count || Count > Actual_Count)
                            {
                                MessageBox.Show(string.Format("Argument Error: ContId {0} takes {1} arguments, but {2} were supplied!", Match, Actual_Count, Count));
                            }

                            Data.Add(0x7F);
                            Data.Add(Match.Key);
                            int Value = 0;
                            
                            for (int x = 0; x < Actual_Count; x++) // We only want to go up to count, otherwise we might add too many arguments
                                switch (Match.Key)
                                {
                                    case 0x05:
                                        if (int.TryParse(Matches[x], NumberStyles.AllowHexSpecifier, null, out Value))
                                        {
                                            Data.Add((byte)(Value >> 16));
                                            Data.Add((byte)(Value >> 8));
                                            Data.Add((byte)Value);
                                        }
                                        break;
                                    case 0x08: // Player Expressions

                                    case 0x09: // Expressions
                                        var Expression = Expression_List.FirstOrDefault(o => o.Value.ToLower().Equals(Matches[x]));
                                        if (!string.IsNullOrEmpty(Expression.Value))
                                        {
                                            Data.Add((byte)(Expression.Key >> 16));
                                            Data.Add((byte)(Expression.Key >> 8));
                                            Data.Add((byte)Expression.Key);
                                        }
                                        else if (int.TryParse(Matches[x], NumberStyles.AllowHexSpecifier, null, out Value))
                                        {
                                            Data.Add((byte)(Value >> 16));
                                            Data.Add((byte)(Value >> 8));
                                            Data.Add((byte)Value);
                                        }
                                        else
                                        {
                                            Data.Add(0);
                                            Data.Add(0);
                                            Data.Add(0);
                                            MessageBox.Show("Argument Error: <Expression []> unable to find a match or parse hex for type: " + Matches[x]);
                                        }
                                        break;
                                    case 0x50:
                                        if (x == 0 && int.TryParse(Matches[0], out Value)
                                            && int.TryParse(Matches[1], NumberStyles.AllowHexSpecifier, null, out int Value2))
                                        {
                                            Data.Add((byte)(Value2 >> 16));
                                            Data.Add((byte)(Value2 >> 8));
                                            Data.Add((byte)Value2);
                                            Data.Add((byte)Value);
                                        }
                                        break;
                                    case 0x59:
                                        var Effect = SoundEffect_List.FirstOrDefault(o => o.Value.ToLower().Equals(Matches[x]));
                                        if (!string.IsNullOrEmpty(Effect.Value))
                                        {
                                            Data.Add(Effect.Key);
                                        }
                                        else
                                        {
                                            Data.Add(0x07);
                                            MessageBox.Show("Argument Error: <Play Sound Effect []> has an invalid sound effect! Will default to None.");
                                        }
                                        break;
                                    default:
                                        if (byte.TryParse(Matches[x], NumberStyles.AllowHexSpecifier, null, out byte Result))
                                        {
                                            Data.Add(Result);
                                        }
                                        else
                                        {
                                            MessageBox.Show(string.Format("Argument Error: Arguments must be in hexidecimal! Recieved: {0}", Matches[x]));
                                            Data.Add(0);
                                        }
                                        break;
                                }

                            i += Index - i;
                            ContId_Set = true;
                        }
                        else if (Cont_Id.Contains("<unknown contid 0x")
                            && byte.TryParse(Cont_Id.Substring(18, 2), NumberStyles.AllowHexSpecifier, null, out byte ContId))
                        {
                            Data.Add(0x7F);
                            Data.Add(ContId);
                            i += Index - i;
                            ContId_Set = true;
                            //System.Windows.Forms.MessageBox.Show(Text[i].ToString());
                        }
                        else
                        {
                            MessageBox.Show("Couldn't find a cont id match for: " + Cont_Id);
                        }
                    }
                }

                if (!ContId_Set)
                {
                    if (Character_Map.ContainsValue(Character))
                    {
                        Data.Add(Character_Map.First(o => o.Value.Equals(Character)).Key);
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find byte for character: " + Character);
                    }
                }
            }

            return Data.ToArray();
        }
    }
}
