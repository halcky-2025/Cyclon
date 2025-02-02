using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;
using OpenAI;
using OpenAI.Chat;
using System.Text.RegularExpressions;
namespace Cyclon
{
    partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        public static extern void SetCapture(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void ReleaseCapture();
        public RichTextPanel text;
        public RichTextPanel vision1;
        public OpenAIClient client;
        public Form1()
        {
            InitializeComponent();
            using TestContext tc = new();
            text = new RichTextPanel(this);
            vision1 = new RichVisionPanel(this);
            panel.Controls.Add(text);
            vision.Controls.Add(vision1);
            vision1.local.size.X = vision1.Parent.Width;
            vision1.local.size.Y = vision1.Parent.Height;
            init();
            text.local.vision = vision1.local as Vision;
            text.local.local = text.local;
            vision1.local.local = text.local;
            //OPI();
        }
        public async Task<int> OPI(Line line, String str, Local local)
        {
            var messages = new List<OpenAI.Chat.Message> { new OpenAI.Chat.Message(Role.User, str) };
            var chat = new ChatRequest(messages, model: "gpt-4o-mini");
            var result = await client.ChatEndpoint.GetCompletionAsync(chat);
            foreach (var s in result.ToString().Split('\n'))
            {
                var line2 = new Line();
                line2.childend.Next(new Letter() { text = s });
                line.Next(line2);
                line2.childstart = line2.childend.next;
                line = line2;
            }
            local.panel.input = true;
            local.panel.Invalidate();
            return 0;
        }
        public async void init()
        {
            using TestContext tc = new();
            await tc.Database.EnsureCreatedAsync();
            if (await tc.TestData.CountAsync() == 0)
            {
                var td = new TTestData { Name = "" };
                tc.Add(td);
                await tc.SaveChangesAsync();
                client = new OpenAIClient(td.oapi);
                text.Add("");
            }
            else
            {
                var td = await tc.TestData.FirstAsync();
                client = new OpenAIClient(td.oapi);
                text.Add(td.Name);
                try
                {
                    /*var item = Start(text.local);
                    text.local.Setid();
                    item.exe(text.local);*/
                }
                catch (Exception e) { }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            listen = false;
            int n = 0;
            using TestContext tc = new();
            var td = await tc.TestData.FirstAsync();
            td.Name = text.Text.Substring(0, text.Text.Length - 1);
            await tc.SaveChangesAsync();
            var item = Start(text.local);
            text.local.Setid();
            item.exe(text.local);
            if (text.local.sigmap.ContainsKey("server")) text.local.sigmap["server"].exe(text.local);
            vision1.input = true;
            vision1.Invalidate();
            listen = true;
        }
        bool listen = false;
        List<Message> messages = new List<Message>();
        List<Browser> browsers = new List<Browser>();
        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                var browser = new Browser(this, browsers.Count) { StartPosition = FormStartPosition.Manual, Location = new Point(1000, 100) };
                browsers.Add(browser);
                browser.ShowDialog();
            }).Start();
            Listen();
        }
        public async void Listen()
        {
            for (; listen;)
            {
                foreach (var msg in messages)
                {

                }
                await Task.Delay(500);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            using TestContext tc = new();
            await tc.Database.EnsureCreatedAsync();
            var td = await tc.TestData.FirstAsync();
            td.Name = "";
            await tc.SaveChangesAsync();
        }
    }
    class RichTextPanel: Panel
    {
        public CommentLet switched;
        public Form1 form;
        public Local local;
        float h0;
        int n = 0;
        public bool input = false;
        public bool switchdraw = false;
        public Capture capture = null;
        public RichTextPanel(Form1 form)
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            this.form = form;
            Font = new System.Drawing.Font(new FontFamily("Consolas"), 7.5f);
            TabStop = true;
        }
        public override string Text
        {
            get
            {
                return local.Text;
            }
            set
            {
            }
        }
        public void Add(String text)
        {
            local = new Local() { console = form.console, panel = this };
            var letters = Form1.Compile(text + "\0");
            for (var i = 0; i < letters.Count; i++) local.add(letters[i]);
            local.xtype = SizeType.Scroll;
            local.ytype = SizeType.Scroll;
            local.size.X = this.Parent.Width;
            local.size.Y = this.Parent.Height;
            input = true;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool select = false;
            switch(e.KeyCode)
            {
                case Keys.Back:
                    e.Handled = true;
                    local.seln = -1;
                    local.selects[0].state.n = local.selects[1].state.n = 0;
                    local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = ""}, local, ref select);
                    input = true;
                    Invalidate();
                    break;
                case Keys.Delete:
                    e.Handled = true;
                    local.seln = -1;
                    local.selects[0].state.n = local.selects[1].state.n = 0;
                    local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = ""}, local, ref select);
                    input = true;
                    Invalidate();
                    break;
                case Keys.Enter:
                    e.Handled = true;
                    local.seln = -1;
                    local.selects[0].state.n = local.selects[1].state.n = 0;
                    local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = "" }, local, ref select);
                    input = true;
                    Invalidate();
                    break;
                case Keys.Left:
                    break;
                case Keys.Right:
                    break;
                case Keys.Up:
                    break;
                case Keys.Down:
                    break;
            }
        }
        String ja = "";
        System.Windows.Forms.Timer timer;
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar < 32)
            {
                e.Handled = true;
                return;
            }
            else if (e.KeyChar > 256)
            {
                e.Handled = true;
                ja += e.KeyChar;
                if (timer == null)
                {
                    timer = new System.Windows.Forms.Timer() { Interval = 10 };
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
                return;
            }
            bool select = false;
            form.console.Text += e.KeyChar;
            local.seln = -1;
            local.selects[0].state.n = local.selects[1].state.n = 0;
            local.Key(new KeyEvent() { call = KeyCall.KeyDown, text = e.KeyChar.ToString(), key = Keys.None }, local, ref select);
            input = true;
            e.Handled = true;
            Invalidate();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            timer = null;
            bool select = false;
            form.console.Text += ja;
            local.seln = -1;
            local.selects[0].state.n = local.selects[1].state.n = 0;
            local.Key(new KeyEvent() { call = KeyCall.KeyDown, text = ja, key = Keys.None}, local, ref select);
            ja = "";
            input = true;
            Invalidate();
        }
        bool mousedown = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            var mouse = new MouseEvent() { call = MouseCall.MouseDown, x = e.X, y = e.Y, panel = this };
            local.Mouse(mouse, local);
            local.comlet = null;
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            var mouse = new MouseEvent() { call = MouseCall.MouseUp, x = e.X, y = e.Y, panel = this };
            if (capture != null)
            {
                capture.capture(capture, mouse);
                Form1.ReleaseCapture();
                capture = null;
            }
            else
            {
                local.Mouse(mouse, local);
                local.comlet = null;
            }
            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Control.MouseButtons == MouseButtons.Left)
            {
                var mouse = new MouseEvent() { call = MouseCall.MouseUp, x = e.X, y = e.Y, panel = this };
                if (capture != null)
                {
                    capture.capture(capture, mouse);
                }
                else
                {
                    local.Mouse(mouse, local);
                    local.comlet = null;
                }
                Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            if (input)
            {
                int order = 0;
                local.Measure(new Measure() { g = g, font = Font, xtype = SizeType.Scroll, ytype = SizeType.Scroll, panel = this}, local, ref order);
                local.comlet = null;
                input = false;
            }
            bool select = false;
            local.Draw(new Graphic() { g = g, font = Font}, local, ref select);
            local.comlet = null;
        }
    }
    class RichVisionPanel : RichTextPanel
    {
        public RichVisionPanel(Form1 form) : base(form)
        {
            local = new Vision() { console = form.console, panel = this };
            local.vision = local as Vision;
            local.xtype = SizeType.Break;
            local.ytype = SizeType.Scroll;
            input = true;
        }
    }
}
