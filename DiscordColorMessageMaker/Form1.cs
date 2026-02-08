using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DiscordColorMessageMaker
{
    public partial class Form1 : Form
    {
        // ===== 상태 변수 =====
        bool isBold = false;
        bool isUnderline = false;

        private static bool HasColor(AnsiState s) => s.Fg.HasValue || s.Bg.HasValue;
        // 현재 TextStyle에 배경/글자 색이 있는가.
        private static bool LosingColorComponent(AnsiState cur, AnsiState next) =>
            (cur.Fg.HasValue && !next.Fg.HasValue) ||
            (cur.Bg.HasValue && !next.Bg.HasValue);

        string CMcode = "";
        string DcCM = "";
        string TextColor = "Default";
        string BgColor = "Default";

        // ===== 마커 기반 ANSI 상태 =====
        // record struct 로 선언하여 Nullable<AnsiState> 사용 가능(.Value OK)
        private record struct AnsiState(bool Bold, bool Under, int? Fg, int? Bg);
        private AnsiState _state = new(false, false, null, null);
        private readonly List<(int pos, AnsiState state)> _marks = new();

        // ===== 타이틀바 다크모드용 Win32 API =====
        [DllImport("Dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        // Windows 버전에 따라 19/20 둘 다 시도
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private void SetTitleBarDark(bool enable)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                return; // Win10 1809 미만이면 지원 X

            int value = enable ? 1 : 0;

            // 새 값(20) 먼저 시도
            DwmSetWindowAttribute(this.Handle,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref value,
                sizeof(int));

            // 구버전(19)도 함께 시도
            DwmSetWindowAttribute(this.Handle,
                DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1,
                ref value,
                sizeof(int));
        }

        public Form1()
        {
            SetTitleBarDark(false);
            InitializeComponent();

            // 실시간 미리보기(입력/선택 변화)
            TextBox.TextChanged += (s, e) => UpdatePreviewNow();
            TextBox.SelectionChanged += (s, e) => UpdatePreviewNow();

            // 시작 시 0 위치에 현재 상태를 하나 박아두기
            AddMarkAtCaret(forcePos: 0);
            UpdatePreviewNow();
        }

        // ====== Bold / Underline ======
        private void BoldChkBox_CheckedChanged(object sender, EventArgs e)
        {
            isBold = BoldChkBox.Checked;
            _state = _state with { Bold = isBold };
            AddMarkAtCaret();          // 커서 위치에 마커
            TextStyleUpdate();         // RTB 시각 서식
            UpdatePreviewNow();
        }

        private void UndlChkBox_CheckedChanged(object sender, EventArgs e)
        {
            isUnderline = UndlChkBox.Checked;
            _state = _state with { Under = isUnderline };
            AddMarkAtCaret();
            TextStyleUpdate();
            UpdatePreviewNow();
        }

        // ====== 글자색 버튼 ======
        private void GreyBtn_Click(object sender, EventArgs e) { SetFg("Grey"); }
        private void RedBtn_Click(object sender, EventArgs e) { SetFg("Red"); }
        private void GreenBtn_Click(object sender, EventArgs e) { SetFg("Green"); }
        private void YellowBtn_Click(object sender, EventArgs e) { SetFg("Yellow"); }
        private void BlueBtn_Click(object sender, EventArgs e) { SetFg("Blue"); }
        private void PinkBtn_Click(object sender, EventArgs e) { SetFg("Pink"); }
        private void TealBtn_Click(object sender, EventArgs e) { SetFg("Teal"); }
        private void WhiteBtn_Click(object sender, EventArgs e) { SetFg("White"); }
        private void DefaultBtn_Click(object sender, EventArgs e) { SetFg("Default"); }

        private void SetFg(string name)
        {
            TextColor = name;
            _state = _state with { Fg = FgCodeFromName(TextColor) };
            AddMarkAtCaret();
            TextColorUpdate(); // RTB 시각 반영
            UpdatePreviewNow();
        }

        // ====== 배경색 버튼 ======
        private void DeepTealBgBtn_Click(object sender, EventArgs e) { SetBg("Teal"); }
        private void OrangeBgBtn_Click(object sender, EventArgs e) { SetBg("Orange"); }
        private void Grey1BgBtn_Click(object sender, EventArgs e) { SetBg("Grey1"); }
        private void Grey2BgBtn_Click(object sender, EventArgs e) { SetBg("Grey2"); }
        private void Grey3BgBtn_Click(object sender, EventArgs e) { SetBg("Grey3"); }
        private void BlurpleBgBtn_Click(object sender, EventArgs e) { SetBg("Blurple"); }
        private void Grey4BgBtn_Click(object sender, EventArgs e) { SetBg("Grey4"); }
        private void WarmIvoryBgBtn_Click(object sender, EventArgs e) { SetBg("Ivory"); }
        private void DefaultBgBtn_Click(object sender, EventArgs e) { SetBg("Default"); }

        private void SetBg(string name)
        {
            BgColor = name;
            _state = _state with { Bg = BgCodeFromName(BgColor) };
            AddMarkAtCaret();
            TextColorUpdate(); // RTB 시각 반영
            UpdatePreviewNow();
        }

        // ======　보이는 용도) ======
        private void TextStyleUpdate()
        {
            Font crntFont = TextBox.SelectionFont ?? TextBox.Font;
            FontStyle style = FontStyle.Regular;
            if (isBold) style |= FontStyle.Bold;
            if (isUnderline) style |= FontStyle.Underline;

            TextBox.SelectionFont = new Font(crntFont, style);
            TextBox.Focus();
        }

        private void TextColorUpdate()
        {
            // 글자색(시각용)
            TextBox.SelectionColor = TextColor switch
            {
                "Grey" when BgColor != "Default" => Color.FromArgb(7, 54, 66),
                "Grey" => Color.FromArgb(79, 84, 92),
                "Red" => Color.FromArgb(220, 50, 47),
                "Green" => Color.FromArgb(141, 161, 24),
                "Yellow" => Color.FromArgb(181, 137, 0),
                "Blue" => Color.FromArgb(38, 139, 210),
                "Pink" => Color.FromArgb(211, 54, 130),
                "Teal" => Color.FromArgb(42, 161, 154),
                "White" when BgColor != "Default" => Color.FromArgb(238, 232, 213),
                "White" => Color.FromArgb(248, 245, 242),
                _ => TextBox.ForeColor
            };

            // 하이라이트(시각용)
            TextBox.SelectionBackColor = BgColor switch
            {
                "Teal" => Color.FromArgb(0, 43, 54),
                "Orange" => Color.FromArgb(203, 75, 22),
                "Grey1" => Color.FromArgb(88, 110, 117),
                "Grey2" => Color.FromArgb(101, 123, 131),
                "Grey3" => Color.FromArgb(131, 148, 150),
                "Blurple" => Color.FromArgb(108, 113, 196),
                "Grey4" => Color.FromArgb(147, 161, 161),
                "Ivory" => Color.FromArgb(253, 246, 227),
                _ => TextBox.BackColor
            };

            TextBox.Focus();
        }

        // ====== 이름→ANSI 코드 매핑 ======
        private static int? FgCodeFromName(string name) => name switch
        {
            "Grey" => 30,
            "Red" => 31,
            "Green" => 32,
            "Yellow" => 33,
            "Blue" => 34,
            "Pink" => 35,
            "Teal" => 36,
            "White" => 37,
            _ => null
        };

        private static int? BgCodeFromName(string name) => name switch
        {
            "Teal" => 40,
            "Orange" => 41,
            "Grey1" => 42,
            "Grey2" => 43,
            "Grey3" => 44,
            "Blurple" => 45,
            "Grey4" => 46,
            "Ivory" => 47,
            _ => null
        };

        private static string CodesOf(AnsiState s)
        {
            var list = new List<int>();
            if (s.Bold) list.Add(1);
            if (s.Under) list.Add(4);
            if (s.Fg is int fg) list.Add(fg);
            if (s.Bg is int bg) list.Add(bg);
            if (list.Count == 0) list.Add(0);
            return string.Join(";", list);
        }

        // ====== 커서 위치에 마커 추가 ======
        private void AddMarkAtCaret(int? forcePos = null)
        {
            int pos = forcePos ?? TextBox.SelectionStart;
            int idx = _marks.FindIndex(m => m.pos == pos);
            if (idx >= 0) _marks[idx] = (pos, _state);
            else _marks.Add((pos, _state));
            _marks.Sort((a, b) => a.pos.CompareTo(b.pos));
        }
    }
}
