using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;

namespace Cyclon
{
    partial class Form1 : Form
    {

        public RichTextPanel text;
        public RichTextPanel vision1;
        public Form1()
        {
            InitializeComponent();
            using TestContext tc = new();
            text = new RichTextPanel(this);
            vision1 = new RichVisionPanel(this);
            init();
            panel.Controls.Add(text);
            vision.Controls.Add(vision1);
            text.local.vision = vision1.local as Vision;
            text.local.local = text.local;
            vision1.local.local = text.local;
        }
        public async void init()
        {
            using TestContext tc = new();
            await tc.Database.EnsureCreatedAsync();
            if (await tc.TestData.CountAsync() == 0)
            {
                tc.Add(new TTestData { Name = "" });
                await tc.SaveChangesAsync();
            }
            else
            {
                var td = await tc.TestData.FirstAsync();
                text.Add(td.Name);
                try
                {
                    /*var item = Start(text.local);
                    text.local.Setid();
                    item.exe(text.local);*/
                }
                catch(Exception e) { }
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
            td.Name = text.Text;
            await tc.SaveChangesAsync();
            var item = Start(text.local);
            text.local.Setid();
            item.exe(text.local);
            text.local.sigmap["server"].exe(text.local);
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
        public RichTextPanel(Form1 form)
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            this.form = form;
            Font = new System.Drawing.Font(new FontFamily("Consolas"), 7.5f);
            TabStop = true;

        }
        public override string Text {
            get {
                return local.Text;
            }
            set {
            }
        }
        public void Add(String text)
        {
            local = new Local() { console = form.console, panel = this };
            var letters = Form1.Compile(text + "\0");
            for (var i = 0; i < letters.Count; i++) local.add(letters[i]);
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
                    local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = ""}, local, ref select);
                    input = true;
                    Invalidate();
                    break;
                case Keys.Delete:
                    e.Handled = true;
                    local.seln = -1;
                    local.Key(new KeyEvent() { call = KeyCall.KeyDown, key = e.KeyCode, text = ""}, local, ref select);
                    input = true;
                    Invalidate();
                    break;
                case Keys.Enter:
                    e.Handled = true;
                    local.seln = -1;
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
                }
                return;
            }
            bool select = false;
            form.console.Text += e.KeyChar;
            local.seln = -1;
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
            form.console.Text += ja.Substring(ja.Length / 2);
            local.seln = -1;
            local.Key(new KeyEvent() { call = KeyCall.KeyDown, text = ja.Substring(ja.Length / 2), key = Keys.None}, local, ref select);
            ja = "";
            input = true;
            Invalidate();
        }
        bool mousedown = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            local.Mouse(new MouseEvent() { call = MouseCall.MouseDown, x = e.X, y = e.Y, panel = this }, local);
            local.comlet = null;
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            local.Mouse(new MouseEvent() { call = MouseCall.MouseUp, x = e.X, y = e.Y, panel = this }, local);
            local.comlet = null;
            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Control.MouseButtons == MouseButtons.Left)
            {
                local.Mouse(new MouseEvent() { call = MouseCall.MouseUp, x = e.X, y = e.Y, panel = this }, local);
                local.comlet = null;
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
            input = true;
        }
    }
}
