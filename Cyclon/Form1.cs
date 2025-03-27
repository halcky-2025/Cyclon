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
using System.Diagnostics;
using WMPLib;
using System.IO.Pipelines;
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
        public Local local;
        public Vision vis;
        public Form1()
        {
            int? x = 1;
            InitializeComponent();
            using TestContext tc = new();
            text = new RichTextPanel(this);
            vision1 = new RichVisionPanel(this);
            panel.Controls.Add(text);
            vision.Controls.Add(vision1);
            vision1.local.size.X = vision1.Parent.Width;
            vision1.local.size.Y = vision1.Parent.Height;
            init();
            //OPI();
        }
        public async Task<int> OPI(Line line, Element element, String str, Local local)
        {
            var messages = new List<OpenAI.Chat.Message> { new OpenAI.Chat.Message(Role.User, str) };
            var chat = new ChatRequest(messages, model: "gpt-4o-mini");
            var result = await client.ChatEndpoint.GetCompletionAsync(chat);
            for (; ; element = element.next)
            {
                if (element.type == LetterType.NyoroNyoro)
                {
                    var lastline = new Line();
                    element = element.before;
                    lastline.AddRange(element.next);
                    line.Next(lastline);
                    lastline.childstart = lastline.childend.next;
                    element.next = line.childend;
                    line.childend.before = element;
                    var kaigyou = new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou };
                    line.Next(kaigyou);
                    break;
                }
                else if (element.type == LetterType.ElemEnd) break;
            }
            foreach (var s in result.ToString().Split('\n'))
            {
                var line2 = new Line();
                line2.childend.Next(new Letter() { text = "`" + s + "<br|>\n" });
                line2.recompile = true;
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
                    var error = false;
                    var item = Start(text.local, ref error);
                    text.local.Setid();
                    item.exe(text.local);
                }
                catch (Exception e) {
                    local.blockslist = new List<List<Block>>();
                }
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
            td.Name = text.Text.Substring(0, text.Text.Length);
            await tc.SaveChangesAsync();
            var error = false;
            var item = Start(text.local, ref error);
            text.local.Setid();
            item.exe(text.local);
            if (text.local.sigmap.ContainsKey("server")) text.local.sigmap["server"].exe(text.local);
            vision1.input = true;
            vision1.Invalidate();
            text.Invalidate();
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
            text.Add("");
        }
        public String totext = "";
        private void button4_Click(object sender, EventArgs e)
        {
            totext = vision1.local.Text(vision1.local);
            MessageBox.Show(totext);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            vision1.Add(totext);
        }

        private async void button6_Click(object sender, EventArgs e)
        {

            listen = false;
            int n = 0;
            using TestContext tc = new();
            var td = await tc.TestData.FirstAsync();
            td.Name = text.Text.Substring(0, text.Text.Length);
            await tc.SaveChangesAsync();
            Compile2();
        }
        public void Compile2()
        {
            bool error = false;
            var item = Start(text.local, ref error);
            text.local.Setid();
            item.exeZ(text.local);
            error = false;
            item.exeA(text.local);
            console.Text += "\n" + (item.children[1] as Block).Show("", ref error);
            error = false;
            local.calls.Add(local.KouhoSet);
            item.exeB(text.local);
            local.calls.RemoveAt(local.calls.Count - 1);
            console.Text += "\n" + (item.children[1] as Block).Show("@", ref error);
            vision1.input = true;
            vision1.Invalidate();
            text.Invalidate();
            listen = true;
        }
    }
    class History
    {
        public List<String> texts = new List<string>();
        public int n = 0;
        public void Add(String text)
        {
            if (n != texts.Count) texts.RemoveRange(n, texts.Count - n);
            texts.Add(text);
            n++;
        }
        public String Back()
        {
            if (n == 1) return null;
            n--;
            return texts[n - 1];
        }
        public String Go()
        {
            if (n == texts.Count) return null;
            n++;
            return texts[n - 1];
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
        public System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
        public RichTextPanel(Form1 form)
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            this.form = form;
            Font = new System.Drawing.Font(new FontFamily("Consolas"), 7.5f);
            TabStop = true;
            timer2.Interval = 10;
            timer2.Tick += Timer2_Tick;
            timer2.Start();
        }

        private void Timer2_Tick(object? sender, EventArgs e)
        {
            for (var i = 0; i < local.animations.Count; i++)
            {
                local.animations[i].Interval(Environment.TickCount, local);
            }
        }

        public override string Text
        {
            get
            {
                return local.Text(local);
            }
        }
        public virtual void Add(String text)
        {
            var textlocal = new TextLocal() { console = form.console, panel = this };
            textlocal.history.Add(text);
            local = textlocal;
            form.local = local;
            var letters = Form1.Compile(text + "\0", local);
            for (var i = 0; i < letters.Count; i++) local.add(letters[i]);
            local.xtype = SizeType.Scroll;
            local.ytype = SizeType.Scroll;
            local.size.X = this.Parent.Width;
            local.size.Y = this.Parent.Height;
            input = true;
            Invalidate();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool select = false;
            switch(e.KeyCode)
            {
                case Keys.Back:
                case Keys.Delete:
                case Keys.Enter:
                case Keys.Left:
                case Keys.Right:
                    local.countn = -1;
                    goto case Keys.Up;
                case Keys.Up:
                case Keys.Down:
                    e.Handled = true;
                    local.seln = -1;
                    local.selects[0].state.n = local.selects[1].state.n = 0;
                    local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = "", ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control, shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift }, local, ref select);
                    Invalidate();
                    break;
            }
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Y:
                    case Keys.Z:
                        local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = "", ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control, shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift }, local, ref select);
                        Invalidate();
                        break;
                }
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
            local.countn = -1;
            local.selects[0].state.n = local.selects[1].state.n = 0;
            local.Key(new KeyEvent() { call = KeyCall.KeyDown, text = e.KeyChar.ToString(), key = Keys.None, ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control, shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift }, local, ref select);
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
            local.Key(new KeyEvent() {call = KeyCall.KeyDown, text = ja, key = Keys.None, ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control, shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift }, local, ref select);
            ja = "";
            Invalidate();
        }
        bool mousedown = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            var mouse = new MouseEvent() { call = MouseCall.MouseDown, x = e.X, y = e.Y, basepos = new Point(e.X, e.Y), panel = this };
            local.countn = -1;
            local.Mouse(mouse, local);
            local.comlet = null;
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            var mouse = new MouseEvent() { call = MouseCall.MouseUp, x = e.X, y = e.Y, basepos = new Point(e.X, e.Y), panel = this };
            local.countn = -1;
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
                var mouse = new MouseEvent() { call = MouseCall.MouseMove, x = e.X, y = e.Y, basepos = new Point(e.X, e.Y), panel = this };
                local.countn = -1;
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
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            var mouse = new MouseEvent() { call = MouseCall.DoubleClick, x = e.X, y = e.Y, basepos = new Point(e.X, e.Y), panel = this };
            local.countn = -1;
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
                if (this is not RichVisionPanel)
                {
                    if (local.selects[0].state.elements.Last() == local.selects[1].state.elements.Last() && local.selects[0].n == local.selects[1].n)
                    {
                        var letter = local.selects[0].state.elements.Last();
                        if (letter.type == LetterType.Letter)
                        {
                            local.letter = letter;
                        }
                        else if (local.selects[0].n == 0 && letter.before.type == LetterType.Letter)
                        {
                            local.letter = letter.before;
                        }
                        else if (local.selects[0].n == letter.Count() && letter.next.type == LetterType.Letter)
                        {
                            local.letter = letter.next;
                        }
                        else local.letter = new Letter();
                        local.kouhos = null;
                        form.Compile2();
                        if (local.kouhos != null)
                        {
                            local.Measure(new Measure() { g = g, font = Font, xtype = SizeType.Scroll, ytype = SizeType.Scroll, panel = this }, local, ref order);
                        }
                    }
                }
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
            form.vis = local as Vision;
            local.xtype = SizeType.Break;
            local.ytype = SizeType.Scroll;
            input = true;
        }
        public override void Add(String text)
        {
            local = new Vision() { console = form.console, panel = this};
            form.vis = local as Vision;
            var letters = Form1.Compile(text + "\0", local);
            for (var i = 0; i < letters.Count; i++) local.add(letters[i]);
            local.xtype = SizeType.Break;
            local.ytype = SizeType.Scroll;
            local.size.X = this.Parent.Width;
            local.size.Y = this.Parent.Height;
            input = true;
            Invalidate();
        }
    }
}


