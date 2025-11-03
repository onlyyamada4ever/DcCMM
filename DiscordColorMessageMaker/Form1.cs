using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DiscordColorMessageMaker
{
    public partial class Form1 : Form
    {
        // ===== 상태 변수 =====
        bool isBold = false;
        bool isUnderline = false;

        string CMcode = "";
        string DcCM = "";
        string TextColor = "Default";
        string BgColor = "Default";

        // ===== 마커 기반 ANSI 상태 =====
        private record AnsiState(bool Bold, bool Under, int? Fg, int? Bg);
        private AnsiState _state = new(false, false, null, null);
        private readonly List<(int pos, AnsiState state)> _marks = new();

        public Form1()
        {
            InitializeComponent();

            // 실시간 미리보기(입력/선택 변화)
            TextBox.TextChanged += (s, e) => UpdatePreviewNow();
            TextBox.SelectionChanged += (s, e) => UpdatePreviewNow();

            // 시작 시 0 위치에 현재 상태를 하나 박아두면 편함
            AddMarkAtCaret(forcePos: 0);

            UpdatePreviewNow();
        }

        // ====== Bold / Underline ======
        private void BoldChkBox_CheckedChanged(object sender, EventArgs e)
        {
            isBold = BoldChkBox.Checked;
            _state = _state with { Bold = isBold };
            AddMarkAtCaret();                      // 커서 위치에 마커
            TextStyleUpdate();                     // 눈에 보이는 서식
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

        // ====== 시각 서식 업데이트 (RTB에 보이는 용도) ======
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
                _ => Color.Black
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
            // 같은 위치에 있으면 덮어쓰기
            int idx = _marks.FindIndex(m => m.pos == pos);
            if (idx >= 0) _marks[idx] = (pos, _state);
            else _marks.Add((pos, _state));

            _marks.Sort((a, b) => a.pos.CompareTo(b.pos));
        }

        // ====== 마커 기반 ANSI 빌더 (공백에서 밑줄/배경만 끊기) ======
        private string BuildDiscordAnsiFromMarks(string content)
        {
            const string ESC = "\u001b";

            // 마커가 하나도 없으면 0 위치에 현재 상태를 박음
            if (_marks.Count == 0) _marks.Add((0, _state));

            // 범위 밖 마커 제거
            _marks.RemoveAll(m => m.pos < 0 || m.pos > content.Length);

            var sb = new StringBuilder(content.Length * 2);
            int markIdx = 0;
            AnsiState? active = null;

            for (int i = 0; i <= content.Length; i++)
            {
                // i 위치에 마커 있으면 상태 전환
                while (markIdx < _marks.Count && _marks[markIdx].pos == i)
                {
                    active = _marks[markIdx].state;
                    sb.Append(ESC).Append('[').Append(CodesOf(active)).Append('m');
                    markIdx++;
                }

                if (i == content.Length) break;

                char ch = content[i];

                // 줄바꿈: 줄 끝에서 리셋
                if (ch == '\r' || ch == '\n')
                {
                    if (active is not null)
                        sb.Append(ESC).Append("[0m");
                    sb.Append(ch);
                    continue;
                }

                // 공백: 밑줄/배경만 해제(24,49). 다음 글자에서 다시 현재 상태 재적용
                if (ch == ' ' || ch == '\t')
                {
                    if (active is not null && (active.Under || active.Bg is not null))
                        sb.Append(ESC).Append("[24;49m");
                    sb.Append(ch);
                    if (active is not null)
                        sb.Append(ESC).Append('[').Append(CodesOf(active)).Append('m');
                    continue;
                }

                // 일반 문자
                sb.Append(ch);
            }

            if (active is not null) sb.Append(ESC).Append("[0m");
            return $"```ansi\n{sb}\n```";
        }

        // ====== 미리보기 즉시 갱신 ======
        private void UpdatePreviewNow()
        {
            string content = TextBox.Text;
            CMcode = CodesOf(_state);                                 // 참고용(현재 상태)
            DcCM = BuildDiscordAnsiFromMarks(content);

            if (DcCMOutput != null)
                DcCMOutput.Text = DcCM;
        }

        // 복사버튼
        private void CopyBtnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DcCM))
                UpdatePreviewNow();

            Clipboard.SetText(DcCM, TextDataFormat.UnicodeText);
        }
    }
}
