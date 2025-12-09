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
        // 볼드체, 밑줄 상태 변수
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

        // ====== ANSI 변환: 스타일 전환 최소화 / 불필요한 0m 제거 / 줄 끝에서만 리셋 ======
        private string BuildDiscordAnsiFromMarks(string content)
        {
            const string ESC = "\u001b";

            if (_marks.Count == 0)
                _marks.Add((0, _state));

            _marks.RemoveAll(m => m.pos < 0 || m.pos > content.Length);
            _marks.Sort((a, b) => a.pos.CompareTo(b.pos));

            var sb = new StringBuilder(content.Length * 2);
            int markIdx = 0;

            AnsiState? current = null; // null = 플레인(아무 스타일 없음)

            bool SameStyle(AnsiState? a, AnsiState? b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                return a.Value.Bold == b.Value.Bold &&
                       a.Value.Under == b.Value.Under &&
                       a.Value.Fg == b.Value.Fg &&
                       a.Value.Bg == b.Value.Bg;
            }

            for (int i = 0; i <= content.Length; i++)
            {
                // i 위치에 마커가 있다면 즉시 스타일 전환 판단
                while (markIdx < _marks.Count && _marks[markIdx].pos == i)
                {
                    var next = _marks[markIdx].state;

                    if (!SameStyle(current, next))
                    {
                        bool nextHasAnyStyle = next.Bold || next.Under || next.Fg.HasValue || next.Bg.HasValue;

                        if (current is AnsiState cur)
                        {
                            bool curHasAnyStyle = cur.Bold || cur.Under || cur.Fg.HasValue || cur.Bg.HasValue;

                            // 1) 색 → 색 없음(볼드/밑줄만 or 완전 플레인)  또는
                            // 2) 색 일부 제거(전경/배경 중 하나만 해제)  →  하드 리셋 후 재적용
                            bool needHardReset =
                                (HasColor(cur) && !HasColor(next)) ||
                                (HasColor(cur) && LosingColorComponent(cur, next));

                            if (needHardReset)
                            {
                                // 색을 확실히 지움
                                sb.Append(ESC).Append("[0m");
                                // 다음 상태가 플레인이 아니면(볼드/밑줄/색 중 하나라도 있으면) 다시 켬
                                if (nextHasAnyStyle)
                                    sb.Append(ESC).Append('[').Append(CodesOf(next)).Append('m');
                                current = nextHasAnyStyle ? next : (AnsiState?)null;
                            }
                            else
                            {
                                // 플레인으로 가는 단순 해제(스타일만 꺼짐): 0m 한 번
                                if (!nextHasAnyStyle)
                                {
                                    if (curHasAnyStyle)
                                    {
                                        sb.Append(ESC).Append("[0m");
                                        current = null;
                                    }
                                }
                                else
                                {
                                    // 그 외(색↔색, 스타일 변경 포함)는 바로 새 코드
                                    sb.Append(ESC).Append('[').Append(CodesOf(next)).Append('m');
                                    current = next;
                                }
                            }
                        }
                        else
                        {
                            // 현재 플레인 → 무언가 켬
                            if (nextHasAnyStyle)
                            {
                                sb.Append(ESC).Append('[').Append(CodesOf(next)).Append('m');
                                current = next;
                            }
                            // 플레인→플레인 전환은 출력 없음
                        }
                    }
                    markIdx++;
                }

                if (i == content.Length) break;

                char ch = content[i];

                // 줄바꿈: 줄 끝에서 스타일이 켜져 있으면 닫고 줄바꿈 출력
                if (ch == '\r' || ch == '\n')
                {
                    if (current != null)
                    {
                        sb.Append(ESC).Append("[0m");
                        current = null; // 다음 줄은 플레인으로 시작
                    }
                    sb.Append(ch);
                    continue;
                }

                // 일반 문자
                sb.Append(ch);
            }

            // 파일 끝에서 스타일 남아 있으면 닫기
            if (current != null)
                sb.Append(ESC).Append("[0m");

            return $"```ansi\n{sb}\n```";
        }


        // ====== 미리보기 실시간 업데이트 ======
        private void UpdatePreviewNow()
        {
            string content = TextBox.Text;
            CMcode = CodesOf(_state);                 // 참고용(현재 상태)
            DcCM = BuildDiscordAnsiFromMarks(content);

            if (DcCMOutput != null)
                DcCMOutput.Text = DcCM;
        }

        // ====== 복사 버튼 ======
        private void CopyBtnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DcCM))
                UpdatePreviewNow();

            Clipboard.SetText(DcCM, TextDataFormat.UnicodeText);
            MessageBox.Show("복사 완료! 디스코드에 붙여넣기 해 주세요.");
        }

        // ====== 초기화 버튼 ======
        private void ClearBtnClick(object sender, EventArgs e)
        {
            TextBox.Clear();
            // 상태 초기화
            isBold = false;
            isUnderline = false;
            TextColor = "Default";
            BgColor = "Default";
            _state = new(false, false, null, null);
            _marks.Clear();
            AddMarkAtCaret(forcePos: 0);

            // 체크박스 초기화
            BoldChkBox.Checked = false;
            UndlChkBox.Checked = false;

            TextStyleUpdate();
            TextColorUpdate();
            UpdatePreviewNow();
        }

        // ====== 테마 전환 ======
        private void LightThemeBtnClick(object sender, EventArgs e)
        {
            // 라이트모드 -> 다크모드
            // 50,51,57
            // 223 224 226

            this.BackColor = Color.FromArgb(50, 51, 57);
            TextBox.BackColor = Color.FromArgb(50, 51, 57);
            TextBox.ForeColor = Color.FromArgb(230, 230, 230);
            DcCMOutput.BackColor = Color.FromArgb(50, 51, 57);
            DcCMOutput.ForeColor = Color.FromArgb(230, 230, 230);

            MadeLabel.ForeColor = Color.FromArgb(230, 230, 230);
            EmailLabel.ForeColor = Color.FromArgb(230, 230, 230);
            DiscordLabel.ForeColor = Color.FromArgb(230, 230, 230);

            BoldChkBox.ForeColor = Color.FromArgb(223, 224, 226);
            UndlChkBox.ForeColor = Color.FromArgb(223, 224, 226);

            GreyBtn.BackColor = Color.FromArgb(50, 51, 57);
            RedBtn.BackColor = Color.FromArgb(50, 51, 57);
            GreenBtn.BackColor = Color.FromArgb(50, 51, 57);
            YellowBtn.BackColor = Color.FromArgb(50, 51, 57);
            BlueBtn.BackColor = Color.FromArgb(50, 51, 57);
            PinkBtn.BackColor = Color.FromArgb(50, 51, 57);
            TealBtn.BackColor = Color.FromArgb(50, 51, 57);
            WhiteBtn.BackColor = Color.FromArgb(50, 51, 57);
            WhiteBtn.ForeColor = Color.FromArgb(255, 255, 255);
            DefaultBtn.BackColor = Color.FromArgb(50, 51, 57);
            DefaultBtn.ForeColor = Color.FromArgb(223, 224, 226);

            DeepTealBgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            OrangeBgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            Grey1BgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            Grey2BgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            Grey3BgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            BlurpleBgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            Grey4BgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            WarmIvoryBgBtn.ForeColor = Color.FromArgb(223, 224, 226);
            DefaultBgBtn.BackColor = Color.FromArgb(50, 51, 57);
            DefaultBgBtn.ForeColor = Color.FromArgb(223, 224, 226);

            DarkThemeBtn.BackColor = Color.FromArgb(50, 51, 57);
            DarkThemeBtn.ForeColor = Color.FromArgb(223, 224, 226);

            ClrBtn.BackColor = Color.FromArgb(50, 51, 57);
            ClrBtn.ForeColor = Color.FromArgb(223, 224, 226);

            CopyBtn.BackColor = Color.FromArgb(50, 51, 57);
            CopyBtn.ForeColor = Color.FromArgb(223, 224, 226);

            SetTitleBarDark(true);

            LightThemeBtn.Visible = false;
            DarkThemeBtn.Visible = true;
        }

        private void DarkThemeBtnClick(object sender, EventArgs e)
        {
            // 다크모드 -> 라이트모드
            // 251 251 251
            // 50 51 57
            this.BackColor = Color.FromArgb(251, 251, 251);
            TextBox.BackColor = Color.FromArgb(251, 251, 251);
            TextBox.ForeColor = Color.FromArgb(50, 51, 57);
            DcCMOutput.BackColor = Color.FromArgb(251, 251, 251);
            DcCMOutput.ForeColor = Color.FromArgb(50, 51, 57);

            MadeLabel.ForeColor = Color.FromArgb(50, 51, 57);
            EmailLabel.ForeColor = Color.FromArgb(50, 51, 57);
            DiscordLabel.ForeColor = Color.FromArgb(50, 51, 57);

            BoldChkBox.ForeColor = Color.FromArgb(50, 51, 57);
            UndlChkBox.ForeColor = Color.FromArgb(50, 51, 57);

            GreyBtn.BackColor = Color.FromArgb(251, 251, 251);
            RedBtn.BackColor = Color.FromArgb(251, 251, 251);
            GreenBtn.BackColor = Color.FromArgb(251, 251, 251);
            YellowBtn.BackColor = Color.FromArgb(251, 251, 251);
            BlueBtn.BackColor = Color.FromArgb(251, 251, 251);
            PinkBtn.BackColor = Color.FromArgb(251, 251, 251);
            TealBtn.BackColor = Color.FromArgb(251, 251, 251);
            WhiteBtn.BackColor = Color.FromArgb(251, 251, 251);
            WhiteBtn.ForeColor = Color.FromArgb(50, 51, 57);
            DefaultBtn.BackColor = Color.FromArgb(251, 251, 251);
            DefaultBtn.ForeColor = Color.FromArgb(50, 51, 57);

            OrangeBgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            Grey1BgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            Grey2BgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            Grey3BgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            BlurpleBgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            Grey4BgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            WarmIvoryBgBtn.ForeColor = Color.FromArgb(50, 51, 57);
            DefaultBgBtn.BackColor = Color.FromArgb(251, 251, 251);
            DefaultBgBtn.ForeColor = Color.FromArgb(50, 51, 57);

            DarkThemeBtn.BackColor = Color.FromArgb(50, 51, 57);
            DarkThemeBtn.ForeColor = Color.FromArgb(223, 224, 226);

            ClrBtn.BackColor = Color.FromArgb(251, 251, 251);
            ClrBtn.ForeColor = Color.FromArgb(50, 51, 57);

            CopyBtn.BackColor = Color.FromArgb(251, 251, 251);
            CopyBtn.ForeColor = Color.FromArgb(50, 51, 57);

            SetTitleBarDark(false);

            DarkThemeBtn.Visible = false;
            LightThemeBtn.Visible = true;
        }

        // 안내창 닫기
        private void InstructionClick(object sender, EventArgs e)
        {
            InstructionPic.Visible = false;
        }
        
        // 서버 초대 버튼 (디스코드 텍스트 클릭시 초대장 전송)
        private void ServerInvClick(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://discord.gg/sXNQBhGSwr",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"링크를 열 수 없습니다. 다시 시도해주세요.");
            }
        }

        // 이메일로 이동 (이메일 텍스트 클릭시 지메일 오픈)
        // 추후에 윈도우 기본 설정 메일 앱/사이트로 경로 변경할계획
        private void EmailLabel_Click(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://mail.google.com/mail/",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"링크를 열 수 없습니다. 다시 시도해주세요.");
            }
        }
    }
}
