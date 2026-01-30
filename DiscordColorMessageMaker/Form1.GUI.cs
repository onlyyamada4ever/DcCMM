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
