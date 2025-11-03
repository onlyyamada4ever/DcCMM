using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;

namespace DiscordColorMessageMaker
{
    public partial class Form1 : Form
    {
        // 변수 선언
        // Variable Declarations
        bool isBold = false;
        bool isUnderline = false;

        string DcCM = "";
        string TextColor = "";
        string BgColor = "";
        

        public Form1()
        {
            InitializeComponent();
        }

        // 볼드, 밑줄 이벤트
        // Bold, Underline Events
        private void BoldChkBox_CheckedChanged(object sender, EventArgs e)
        {
            isBold = BoldChkBox.Checked;
            TextStyleUpdate();
        }

        private void UndlChkBox_CheckedChanged(object sender, EventArgs e)
        {
            isUnderline = UndlChkBox.Checked;
            TextStyleUpdate();
        }

        // 글자색변경
        // Text Color Change Events
        private void GreyBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Grey";
            TextStyleUpdate();
        }
        private void RedBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Red";
            TextStyleUpdate();
        }
        private void GreenBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Green";
            TextStyleUpdate();
        }
        private void YellowBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Yellow";
            TextStyleUpdate();
        }
        private void BlueBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Blue";
            TextStyleUpdate();
        }
        private void PinkBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Pink";
            TextStyleUpdate();
        }
        private void TealBtn_Click(object sender, EventArgs e)
        {
            TextColor = "Teal";
            TextStyleUpdate();
        }
        private void WhiteBtn_Click(object sender, EventArgs e)
        {
            TextColor = "White";
            TextStyleUpdate();
        }
        // 배경색변경
        // Background Color Change Events
        private void DeepTealBgBtn_Click(object sender, EventArgs e)
        {
            BgColor = "Teal";
            TextStyleUpdate();
        }

        // 함수
        private void TextStyleUpdate()
        {

            Font crntFont = TextBox.SelectionFont ?? TextBox.Font;

            FontStyle FStyle = FontStyle.Regular;

            if (isBold)
                FStyle |= FontStyle.Bold;
            if (isUnderline)
                FStyle |= FontStyle.Underline;
            TextBox.SelectionColor = TextColor switch
            {
                "Grey" when BgColor != "Default" => System.Drawing.Color.FromArgb(7,54,66),
                "Grey" => System.Drawing.Color.FromArgb(79, 84, 92),
                "Red" => System.Drawing.Color.FromArgb(220, 50, 47),
                "Green" => System.Drawing.Color.FromArgb(141, 161, 24),
                "Yellow" => System.Drawing.Color.FromArgb(181, 137, 0),
                "Blue" => System.Drawing.Color.FromArgb(38, 139, 210),
                "Pink" => System.Drawing.Color.FromArgb(211, 54, 130),
                "Teal" => System.Drawing.Color.FromArgb(42, 161, 154),
                "White" when BgColor != "Default" => System.Drawing.Color.FromArgb(238,232,213),
                "White" => System.Drawing.Color.FromArgb(248, 245, 242),

                "Default" => System.Drawing.Color.Black
            };


            TextBox.SelectionBackColor = BgColor switch
            {
                "Teal" => System.Drawing.Color.FromArgb(0, 43, 54),
                "Orange" => System.Drawing.Color.FromArgb(203, 75, 22),
                "Grey1" => System.Drawing.Color.FromArgb(88, 110, 117),
                "Grey2" => System.Drawing.Color.FromArgb(101, 123, 131),
                "Grey3" => System.Drawing.Color.FromArgb(131, 148, 150),
                "Blurple" => System.Drawing.Color.FromArgb(108, 113, 196),
                "Grey4" => System.Drawing.Color.FromArgb(147, 161, 161),
                "Ivory" => System.Drawing.Color.FromArgb(253, 246, 227),

                "Default" => System.Drawing.Color.Transparent
            };
            TextBox.SelectionFont = new Font(crntFont, FStyle);
            TextBox.Focus();
        }
        // 테마 변경 버튼
        // Theme Change Button
        private void DarkThemeBtn_Click(object sender, EventArgs e)
        {
            TextBox.BackColor = Color.FromArgb(44, 45, 50);
            Hide();
            Show(LightThemeBtn);
        }
        private void LightThemeBtn_Click(object sender, EventArgs e)
        {
            TextBox.BackColor = Color.FromArgb(251, 251, 251);
            Hide();
            Show(DarkThemeBtn);
        }

    }
}
