using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cyclon
{
    class Div : Element
    {
        public String tag;
        public String id;
        public String sop;
        public Dictionary<String, Obj> statuses = new Dictionary<string, Obj>();
        public Div()
        {
            type = LetterType.Div;
        }
        public void SetStatus(Div divid)
        {
            foreach (var kv in divid.statuses)
            {
                if (!statuses.ContainsKey(kv.Key) || statuses[kv.Key] == null)
                {
                    SetParam(kv.Key, kv.Value);
                    statuses[kv.Key] = kv.Value;
                }
            }
        }
        public virtual void SetParam(String name, Obj obj)
        {

            switch (name)
            {
                case "left":
                    pos.X = (obj as Number).value;
                    break;
                case "top":
                    pos.Y = (obj as Number).value;
                    break;
                case "pos":
                    var blk1 = obj as Block;
                    pos = new PointF((blk1.rets[0] as Number).value, (blk1.rets[1] as Number).value);
                    break;
                case "w":
                case "width":
                    size.X = (obj as Number).value;
                    break;
                case "h":
                case "height":
                    size.Y = (obj as Number).value;
                    break;
                case "size":
                    var blk2 = obj as Block;
                    pos = new PointF((blk2.rets[0] as Number).value, (blk2.rets[1] as Number).value);
                    break;
                case "xtype":
                    switch ((obj as StrObj).value)
                    {
                        case "auto":
                            xtype = SizeType.Auto;
                            break;
                        case "break":
                            xtype = SizeType.Break;
                            break;
                        case "limit":
                            xtype = SizeType.Limit;
                            break;
                        case "scroll":
                            xtype = SizeType.Scroll;
                            break;
                    }
                    break;
                case "ytype":
                    switch ((obj as StrObj).value)
                    {
                        case "auto":
                            ytype = SizeType.Auto;
                            break;
                        case "limit":
                            ytype = SizeType.Limit;
                            break;
                        case "scroll":
                            ytype = SizeType.Scroll;
                            break;
                    }
                    break;
                case "b":
                case "background":
                    var strobj = (obj as StrObj).value;
                    switch (strobj)
                    {
                        case "red":
                            background = Brushes.Red;
                            break;
                        case "green":
                            background = Brushes.Green;
                            break;
                        case "blue":
                            background = Brushes.Blue;
                            break;
                        case "white":
                            background = Brushes.White;
                            break;
                        case "black":
                            background = Brushes.Black;
                            break;
                    }
                    break;
                case "align":
                case "a":
                    var str = (obj as StrObj).value;
                    switch (str)
                    {
                        case "left":
                            align = Align.Left;
                            break;
                        case "center":
                            align = Align.Center;
                            break;
                        case "right":
                            align = Align.Right;
                            break;
                        case "separate":
                            align = Align.Separate;
                            break;
                    }
                    break;
                /*case "onclick":
                    statuses.Add("mouse", obj);
                    break;*/
            }
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            base.Draw(g, local, ref select);
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (update)
            {

                if (statuses.ContainsKey("key")) key = KeyExe;
                if (statuses.ContainsKey("onclick")) mouse = MouseExe;
            }
            return base.Measure(m, local, ref order);
        }

        public bool MouseExe(MouseEvent mouse, Local local)
        {
            var m = statuses["onclick"] as SignalFunction;
            var req = new ConnectStock();
            req.Store(new MouseEventObj(mouse, local) { cls = local.MouseEvent }, local);
            var res = new ConnectStock();
            m.basicexe(req, res, local);
            return false;
        }

        public bool KeyExe(KeyEvent key, Local local)
        {
            var k = statuses["key"] as SignalFunction;
            var req = new ConnectStock();
            req.Store(new KeyEventObj(key, local) { cls = local.KeyEvent }, local);
            var res = new ConnectStock();
            k.basicexe(req, res, local);
            return false;
        }
        public override string Text(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|" + base.Text2(local) + " >";
            return ret;
        }
        public override string Text2(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|" + base.Text2(local) + " >";
            return ret;
        }
    }
    class KeyEventObj : Val
    {
        public KeyEvent key;
        public KeyEventObj(KeyEvent key, Local local) : base(ObjType.KeyEventObj)
        {
            vmap = new Dictionary<string, Obj>();
            vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            vmap["action"] = new Number((int)key.call) { cls = local.Int };
            vmap["text"] = new StrObj(key.text) { cls = local.Str };
            this.key = key;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word = val2 as Word;
                    n++;
                    return vmap[word.name];
                }
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class MouseEventObj : Val
    {
        public MouseEvent mouse;
        public MouseEventObj(MouseEvent mouse, Local local) : base(ObjType.MouseEventObj)
        {
            vmap = new Dictionary<string, Obj>();
            vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            vmap["action"] = new Number((int)mouse.call) { cls = local.Int };
            vmap["x"] = new Number(mouse.x) { cls = local.Int };
            vmap["y"] = new Number(mouse.y) { cls = local.Int };
            this.mouse = mouse;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    return vmap["word"];
                }
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class Sheet : Div
    {
        public const String alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public Dictionary<int, RowData> rowdatas = new Dictionary<int, RowData>();
        public Dictionary<int, ColData> coldatas = new Dictionary<int, ColData>();
        public SortedDictionary<int, SortedDictionary<int, Cell>> cells = new SortedDictionary<int, SortedDictionary<int, Cell>>();
        public Sheet()
        {
            type = LetterType.Sheet;
            tag = "sheet";
            xtype = SizeType.Scroll;
            ytype = SizeType.Scroll;
            size.X = 500;
            size.Y = 300;
        }
        public override string Text(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.ToString();
            }
            ret += "|";
            foreach (var cs in cells)
            {
                foreach (var cell in cs.Value)
                {
                    ret += cell.Value.Text2(local);
                }
            }
            return ret + " >";
        }
        public override string Text2(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|";
            foreach (var cs in cells)
            {
                foreach (var cell in cs.Value)
                {
                    ret += cell.Value.Text2(local);
                }
            }
            return ret + " >";
        }
        public void add(int x, int y, Element element)
        {
            if (!cells.ContainsKey(y))
            {
                cells[y] = new SortedDictionary<int, Cell>();
            }
            var row = cells[y];
            if (!row.ContainsKey(x))
            {
                row[x] = new Cell();
            }
            row[x].add(element);
        }
        public int x = 1, y = 0;
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (update)
            {

                if (statuses.ContainsKey("key")) key = KeyExe;
                if (statuses.ContainsKey("onclick")) mouse = MouseExe;
            }
            Element next = null;
            for(var elem = childend.next; elem != childend ; elem = next )
            {
                next = elem.next;
                if (elem.type == LetterType.Cell)
                {
                    var cell = elem as Cell;
                    int x, y;
                    if (cell.statuses.ContainsKey("y"))
                    {
                        var obj = cell.statuses["y"];
                        if (obj.type == ObjType.Number)
                        {
                            y = (obj as Number).value;
                        }
                        else if (obj.type == ObjType.BoolVal && (obj as BoolVal).value)
                        {
                            y = ++this.y;
                            this.x = 1;
                        }
                        else throw new Exception();
                    }
                    else
                    {
                        y = this.y;
                    }
                    if (cell.statuses.ContainsKey("x"))
                    {
                        x = (cell.statuses["x"] as Number).value;
                    }
                    else
                    {
                        x = this.x++;
                    }
                    if (!cells.ContainsKey(y))
                    {
                        cells[y] = new SortedDictionary<int, Cell>();
                    }
                    cells[y][x] = cell;
                }
                elem.next.RemoveBefore();
            }
            var measure = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel, state = m.state };
            if (font != null) measure.font = font;
            var font0 = measure.font;
            measure.x += margins[1] + paddings[1] + 60f;
            measure.y += margins[0] + paddings[0] + 18f;
            measure.sizex = size.X - margins[3] - paddings[3];
            measure.sizey = size.Y - margins[2] - paddings[2];
            var n0 = 0;
            foreach (var kv in cells)
            {
                var n = 0;
                var width = 0f;
                for (; n0 < kv.Key; n0++)
                {
                    if (rowdatas.ContainsKey(n0))
                    {
                        measure.py += rowdatas[n0].height;
                    }
                    else
                    {
                        measure.py += 18f;
                    }
                }
                foreach (var kv2 in cells[kv.Key])
                {
                    for (; n < kv2.Key; n++)
                    {
                        if (coldatas.ContainsKey(n))
                        {
                            measure.px += coldatas[n].width;
                        }
                        else
                        {
                            measure.px += 90f;
                        }
                    }
                    var measure2 = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel };
                    measure2.state.elements.Add(kv2.Value);
                    kv2.Value.Measure(measure2, local, ref order);
                    measure2.state.elements.RemoveAt(measure2.state.elements.Count - 1);
                    if (rowdatas.ContainsKey(n0))
                    {
                        if (rowdatas[n0].height < kv2.Value.size2.Y) rowdatas[n0].height = kv2.Value.size2.Y + 2;
                    }
                    else rowdatas[n0] = new RowData() { height = kv2.Value.size2.Y + 2 };
                }
                measure.px = measure.x;
            }
            pos = new Point((int)m.x, (int)(m.y + m.py));
            size2 = new Point((int)(measure.sizex + margins[3] + paddings[3] + size.X + scroll.X + 101), (int)(measure.py + margins[2] + paddings[2] + size.Y + scroll.Y + 101));
            if (xtype == SizeType.Scroll) size2.Y += 10;
            if (ytype == SizeType.Scroll) size2.X += 10;
            update = false;
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            var px = g.px;
            var py = g.py;
            g.px += margins[1] + paddings[1];
            g.py += margins[0] + paddings[0];
            using (Bitmap bitmap = new Bitmap((int)size.X, (int)size.Y))
            {
                using (Graphics g2 = Graphics.FromImage(bitmap))
                {
                    var height = 18f;
                    var width = 60f;
                    var n = 0;
                    g2.FillRectangle(Brushes.LightGreen, new RectangleF(60f, 0, size.X - 60f, 18f));
                    g2.DrawLine(Pens.Gray, new PointF(60f, 0), new PointF(60f, size.Y));
                    for (n = 0; n < size.X + scroll.X; n++)
                    {
                        if (scroll.X + size.X <= width) break;
                        var width2 = 90f;
                        if (coldatas.ContainsKey(n))
                        {
                            width2 = coldatas[n].width;
                        }
                        if (scroll.X + 60f <= width)
                        {
                            var ret = "";
                            var n2 = n;
                         head:
                            ret = (alpha[n2 % alpha.Length]).ToString() + ret;
                            if (n2 >= alpha.Length)
                            {
                                n2 /= alpha.Length;
                                n2--;
                                goto head;
                            }
                            g2.DrawString(ret, g.font, Brushes.Black, width + (width2 - 7.5f * ret.Length) / 2 - scroll.X, 0);
                            g2.DrawLine(Pens.Gray, new PointF(width - scroll.X, 0), new PointF(width - scroll.X, size.Y));
                        }
                        width += width2;
                    }
                    width = 60f;
                    var g3 = new Graphic() { g = g2, font = g.font };
                    g3.px += 60f;
                    g3.py += 18f;
                    g2.FillRectangle(Brushes.LightGreen, new RectangleF(0, 18f, 60f, size.Y - 18f));
                    for (var n0 = 0; height < size.Y + scroll.Y; n0++)
                    {
                        var height2 = 18f;
                        if (rowdatas.ContainsKey(n0))
                        {
                            height2 = rowdatas[n0].height;
                        }
                        else
                        {
                            height2 = 18f;
                        }
                        if (scroll.Y + size.Y <= height) break;
                        else if (scroll.Y + 18f <= height)
                        {
                            g2.DrawLine(Pens.Gray, new PointF(0, height - scroll.Y), new PointF(size.X, height - scroll.Y));
                            g2.DrawString((n0 + 1).ToString(), g.font, Brushes.Black, 30f - (n0 + 1).ToString().Length * 7.5f / 2, height - scroll.Y);
                            for (n = 0; n < size.X + scroll.X; n++)
                            {
                                var width2 = 90f;
                                if (coldatas.ContainsKey(n)) width2 = coldatas[n].width;
                                if (scroll.X + size.X <= width) break;
                                if (scroll.X + 60f <= width)
                                {
                                    if (cells.ContainsKey(n0) && cells[n0].ContainsKey(n))
                                    {
                                        var g4 = new Graphic() { g = g2, font = g.font, px = 0, py = 0, x = g3.px + 1 - scroll.X, y = g3.py + 1 - scroll.Y};
                                        cells[n0][n].size.X = width2 - 1;
                                        cells[n0][n].Draw(g4, local, ref select);
                                        g2.DrawLine(Pens.Gray, new PointF(width - scroll.X, height - scroll.Y), new PointF(width - scroll.X, height + height2 - scroll.Y));
                                    }
                                }
                                g3.px += width2;
                                width += width2;
                            }
                        }
                        g3.py += height2;
                        height += height2;
                        g3.px = g3.x + 60f;
                        width = 60f;
                    }
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px, g.y + g.py));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, bitmap.Width, size.Y), new RectangleF(0, 0, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(px + size.X - 10, py, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(px + size.X - 10, py + scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(0, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(0,  0, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(px + size.X - 10, py, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(px +size.X - 10, py + scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(px, py + size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(px + scroll.X / size2.X * size.X,  py + size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (xtype == SizeType.Scroll)
            {
                if (e.y >= size.Y - 10)
                {
                    if (scroll.X / size2.X * size.X <= e.x && e.x < (scroll.X + size.X) / size2.X * size.X)
                    {
                        var x = scroll.X;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.X = x + (e.basepos.X - c.down.basepos.X) * size2.X / size.X;
                                if (scroll.X > size2.X - size.X) scroll.X = size2.X - size.X;
                                if (scroll.X < 0) scroll.X = 0;
                                size2.X = size.X + scroll.X + 100;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (scroll.Y / size2.Y * (size.Y - 12) <= e.y && e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
                    {
                        var y = scroll.Y;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.Y = y + (e.basepos.Y - c.down.basepos.Y) * size2.Y / size.Y;
                                if (scroll.Y > size2.Y - size.Y) scroll.Y = size2.Y - size.Y;
                                if (scroll.Y < 0) scroll.Y = 0;
                                size2.Y = size.Y + scroll.Y + 100;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            if (e.y < 18)
            {
                e.x += (int)scroll.X - 60;
                e.y += (int)scroll.Y - 18;
                var width3 = 0f;
                for (var n0 = 0; width3 < size.X + scroll.X; n0++)
                {
                    var width2 = 90f;
                    if (coldatas.ContainsKey(n0))
                    {
                        width2 = coldatas[n0].width;
                    }
                    else
                    {
                        coldatas[n0] = new ColData() { width = 90f };
                    }
                    width3 += width2;
                    if (-2 + width3 <= e.x && e.x <= width3 + 2)
                    {
                        var col = coldatas[n0];
                        var width4 = col.width;
                        if (e.call == MouseCall.MouseDown)
                        {
                            local.panel.capture = new Capture()
                            {
                                down = e,
                                capture = (c, e) =>
                                {
                                    col.width = width4 + (e.basepos.X - c.down.basepos.X);
                                    return true;
                                }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                    }
                }
            }
            else if (e.x < 60)
            {
                e.x += (int)scroll.X - 60;
                e.y += (int)scroll.Y - 18;
                var height3 = 0f;
                for (var n0 = 0; height3 < size.Y + scroll.Y; n0++)
                {
                    var height2 = 18f;
                    if (rowdatas.ContainsKey(n0))
                    {
                        height2 = rowdatas[n0].height;
                    }
                    else
                    {
                        rowdatas[n0] = new RowData() { height = 18f };
                    }
                    height3 += height2;
                    if (-2 + height3 <= e.y && e.y <= height3 + 2)
                    {
                        var row = rowdatas[n0];
                        var height4 = row.height;
                        if (e.call == MouseCall.MouseDown)
                        {
                            local.panel.capture = new Capture()
                            {
                                down = e,
                                capture = (c, e) =>
                                {
                                    row.height = height4 + (e.basepos.Y - c.down.basepos.Y);
                                    return true;
                                }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                    }
                }
            }
            else
            {
                e.x += (int)scroll.X - 60;
                e.y += (int)scroll.Y - 18;
            }
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            var height = 0f;
            for (var n0 = 0; height < size.Y + scroll.Y; n0++)
            {
                var height2 = 18f;
                if (rowdatas.ContainsKey(n0))
                {
                    height2 = rowdatas[n0].height;
                }
                if (height <= e.y && e.y < height + height2)
                {
                    var width = 0f;
                    for (var n = 0; n < size.X + scroll.X; n++)
                    {
                        var width2 = 90f;
                        if (coldatas.ContainsKey(n))
                        {
                            width2 = coldatas[n].width;
                        }
                        if (width <= e.x && e.x < width + width2)
                        {
                            if (!cells.ContainsKey(n0)) {
                                cells[n0] = new SortedDictionary<int, Cell>();
                            }
                            if (cells[n0].ContainsKey(n))
                            {
                                e.x -= (int)width;
                                e.y -= (int)height;
                                e.state.elements.Add(cells[n0][n]);
                                cells[n0][n].Mouse(e, local);
                            }
                            else {
                                var div = new Cell() { background = Brushes.White };
                                div.statuses.Add("x", new Number(n));
                                div.statuses.Add("y", new Number(n0));
                                div.add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                                cells[n0][n] = div;
                                e.state.Update();
                                local.panel.input = true;
                                e.state.elements.Add(cells[n0][n]);
                                cells[n0][n].Mouse(e, local);
                                Element elem = cells[n0][n];
                                var state = e.state.Clone();
                            head:
                                elem = elem.childend.before; ;
                                state.elements.Add(elem);
                                if (elem.single)
                                {
                                    if (e.call == MouseCall.MouseDown)
                                    {
                                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                    }
                                    else
                                    {
                                        local.selects[1] = new Select() { state = state, n = 0 };
                                    }
                                }
                                else goto head;
                            }
                            break;
                        }
                        width += width2;
                    }
                }
                height += height2;
            }
            return -1;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (select) select = false;
            var go = false;
            var seln = -1;
            foreach(var cs in cells) {
                foreach (var element in cs.Value.Values)
                {
                    if (local.selects[0].state.elements[local.selects[0].state.n] == element)
                    {
                        local.selects[0].state.n++;
                        seln = 0;
                        go = true;
                    }
                    if (local.selects[1].state.elements[local.selects[1].state.n] == element)
                    {
                        local.selects[1].state.n++;
                        seln = 1;
                        go = true;
                    }
                    if (select || go)
                    {
                        bool move = false;
                        int dx = 0, dy = 0;
                        switch(e.key)
                        {
                            case Keys.Left:
                                move = true;
                                dx = -1;
                                break;
                            case Keys.Right:
                                move = true;
                                dx = 1;
                                break;
                            case Keys.Up:
                                move = true;
                                dy = -1;
                                break;
                            case Keys.Down:
                                move = true;
                                dy = 1;
                                break;
                        }
                        if (move)
                        {
                            var cell = element as Cell;
                            var x = (cell.statuses["x"] as Number).value + dx;
                            var y = (cell.statuses["y"] as Number).value + dy;
                            if (x < 0 || y < 0) return 0;
                            if (!cells.ContainsKey(y))
                            {
                                cells[y] = new SortedDictionary<int, Cell>();
                            }
                            if (!cells[y].ContainsKey(x))
                            {
                                var div = new Cell() { background = Brushes.White };
                                div.statuses.Add("x", new Number(x));
                                div.statuses.Add("y", new Number(y));
                                div.add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                                cells[y][x] = div;
                            }
                            Element cell2 = cells[y][x];
                            local.selects[seln].state.elements[local.selects[seln].state.n - 1] = cell2;
                            for(var i = local.selects[seln].state.elements.Count - 1; i >= local.selects[seln].state.n; i--)
                            {
                                local.selects[seln].state.elements.RemoveAt(i);
                            }
                            head:
                            local.selects[seln].state.elements.Add(cell2.childend.next);
                            if (!cell2.childend.next.single)
                            {
                                cell2 = cell2.childend.next;
                                goto head;
                            }
                            local.selects[seln].n = 0;
                            local.selects[(seln + 1) % 2] = local.selects[seln];
                            return 0;
                        }
                        e.state.elements.Add(element);
                        element.Key(e, local, ref select);
                        if (local.seln == 2) return 0;
                        e.state.elements.RemoveAt(e.state.elements.Count - 1);
                        go = false;
                    }
                }
            }
            return 0;
        }
    }
    class RowData : Line
    {
        public float height;
    }
    class ColData
    {
        public float width;
    }
    class Cell : Div
    {
        public Cell()
        {
            tag = "cell";
            type = LetterType.Cell;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            g.x += margins[1] + paddings[1];
            g.y += margins[0] + paddings[0];
            using (Bitmap bitmap = new Bitmap((int)size2.X, (int)size2.Y))
            {
                using (Graphics g2 = Graphics.FromImage(bitmap))
                {
                    if (background != null)
                    {
                        g2.FillRectangle(background, new RectangleF(0, 0, (int)size2.X, (int)size2.Y));
                    }
                    else g2.Clear(Color.Transparent);
                    var g3 = new Graphic() { g = g2, font = g.font };
                    var first = true;
                    var sizey = 0f;
                    var posy = 0f;
                    for (var elem = childend.next; elem != childend; elem = elem.next)
                    {
                        if (align == Align.Left)
                        {
                            g3.px = 0;
                        }
                        else if (align == Align.Center)
                        {
                            g3.px = (size.X - elem.size2.X) / 2;
                        }
                        else if (align == Align.Right)
                        {
                            g3.px = size.X - elem.size2.X;
                        }
                        else if (align == Align.Separate)
                        {
                            if (first)
                            {
                                g3.px = 0;
                                g3.py = posy;
                                elem.Draw(g3, local, ref select);
                                sizey = elem.size2.Y;
                            }
                            else
                            {
                                g3.py = posy;
                                g3.px = (size2.X - elem.size2.X);
                                elem.Draw(g3, local, ref select);
                                posy += Math.Max(sizey, elem.size2.Y);
                                sizey = 0;
                            }
                            first = !first;
                            continue;
                        }
                        elem.Draw(g3, local, ref select);
                        g3.py += elem.size2.Y;
                    }
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px, g.y + g.py));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, bitmap.Width, size.Y), new RectangleF(0, scroll.Y, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto || ytype == SizeType.Break)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(scroll.X, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(scroll.X, scroll.Y, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(0, size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(scroll.X / size2.X * size.X, size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (xtype == SizeType.Scroll)
            {
                if (e.y >= size.Y - 10)
                {
                    if (scroll.X / size2.X * size.X <= e.x && e.x < (scroll.X + size.X) / size2.X * size.X)
                    {
                        var x = scroll.X;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.X = x + (e.x - c.down.x) * size2.X / size.X;
                                if (scroll.X > size2.X - size.X) scroll.X = size2.X - size.X;
                                if (scroll.X < 0) scroll.X = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (scroll.Y / size2.Y * (size.Y - 12) <= e.y && e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
                    {
                        var y = scroll.Y;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.Y = y + (e.y - c.down.y) * size2.Y / size.Y;
                                if (scroll.Y > size2.Y - size.Y) scroll.Y = size2.Y - size.Y;
                                if (scroll.Y < 0) scroll.Y = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            e.x += (int)scroll.X;
            e.y += (int)scroll.Y;
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            for (var element = childend.next; element != childend; element = element.next)
            {
                if (element.pos.Y <= e.y && e.y < element.pos.Y + element.size2.Y)
                {
                    select = true;
                    e.state.elements.Add(element);
                    if (align == Align.Left) { }
                    else if (align == Align.Center)
                    {
                        e.x -= (int)((size.X - element.size2.X) / 2);
                    }
                    else if (align == Align.Right)
                    {
                        e.x -= (int)(size.X - element.size2.X);
                    }
                    else if (align == Align.Separate)
                    {
                        if (e.x > (int)(size.X - element.next.size2.X))
                        {
                            e.state.elements[e.state.elements.Count - 1] = element.next;
                            ret = element.next.Mouse(e, local);
                            e.state.elements.RemoveAt(e.state.elements.Count - 1);
                            return 0;
                        }
                    }
                    ret = element.Mouse(e, local);
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                    return 0;
                }
            }
            if (!select)
            {
                Element elem = this;
                var state = e.state.Clone();
            head:
                elem = elem.childend.before;
                state.elements.Add(elem);
                if (elem.single)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                    }
                    else
                    {
                        local.selects[1] = new Select() { state = state, n = 0 };
                    }
                }
                else goto head;
            }
            return -1;
        }

    }
    class CloneElement : Div
    {
        public Element childstart;
        public CloneElement(Element start, Element end)
        {
            childstart = start;
            childend = end;
            type = LetterType.CloneElement;
        }
        public override void nextplus(State state)
        {
            state.elements[state.elements.Count - 1] = state.elements.Last().next;
        }
        public override string Text(Local local)
        {
            return "";
        }
        public override string Text2(Local local)
        {
            return "";
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            pos.X = m.px;
            pos.Y = m.py;
            var py = 0f;
            if (childstart.next.before != childstart)
            {
                next.RemoveBefore();
                return null;
            }
            var measure = new Measure() { x = m.x, y = m.y, px = m.px, py = m.py, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel };
            for (var element = childstart; ; element = element.next)
            {
                if (element == childstart.before)
                {
                    next.RemoveBefore();
                    return null;
                }
                if (measure.sizex < element.size2.X) measure.sizex = element.size2.X;
                measure.py += element.size2.Y;
                py += element.size2.Y;
                if (element == childend) break;
            }
            size2.X = m.sizex;
            size2.Y = py;
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            g.x += margins[1] + paddings[1];
            g.y += margins[0] + paddings[0];
            using (Bitmap bitmap = new Bitmap((int)size2.X, (int)size2.Y))
            {
                using (Graphics g2 = Graphics.FromImage(bitmap))
                {
                    if (background != null)
                    {
                        g2.FillRectangle(background, new RectangleF(0, 0, (int)size2.X, (int)size2.Y));
                    }
                    else g2.Clear(Color.Transparent);
                    var g3 = new Graphic() { g = g2, font = g.font };
                    for (var elem = childstart; ; elem = elem.next)
                    {
                        elem.Draw(g3, local, ref select);
                        g3.px = g3.x;
                        g3.py += elem.size2.Y;
                        if (elem == childend) break;
                    }
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px, g.y + g.py));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, bitmap.Width, size.Y), new RectangleF(0, scroll.Y, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto || ytype == SizeType.Break)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(scroll.X, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(scroll.X, scroll.Y, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(0, size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(scroll.X / size2.X * size.X, size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (xtype == SizeType.Scroll)
            {
                if (e.y >= size.Y - 10)
                {
                    if (scroll.X / size2.X * size.X <= e.x && e.x < (scroll.X + size.X) / size2.X * size.X)
                    {
                        var x = scroll.X;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.X = x + (e.x - c.down.x) * size2.X / size.X;
                                if (scroll.X > size2.X - size.X) scroll.X = size2.X - size.X;
                                if (scroll.X < 0) scroll.X = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (scroll.Y / size2.Y * (size.Y - 12) <= e.y && e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
                    {
                        var y = scroll.Y;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.Y = y + (e.y - c.down.y) * size2.Y / size.Y;
                                if (scroll.Y > size2.Y - size.Y) scroll.Y = size2.Y - size.Y;
                                if (scroll.Y < 0) scroll.Y = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            e.x += (int)scroll.X;
            e.y += (int)scroll.Y;
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            var posy = 0f;
            for (var element = childstart; ; element = element.next)
            {
                if (posy <= e.y && e.y < posy + element.size2.Y)
                {
                    select = true;
                    e.state.elements.Add(element);
                    ret = element.Mouse(e, local);
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                    return 0;
                }
                posy += element.size2.Y;
                if (element == childend) break;
            }
            if (!select)
            {
                Element elem = this;
                var state = e.state.Clone();
            head:
                elem = elem.childend.before;
                state.elements.Add(elem);
                if (elem.single)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                    }
                    else
                    {
                        local.selects[1] = new Select() { state = state, n = 0 };
                    }
                }
                else goto head;
            }
            return -1;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (select) select = false;
            e.state.elements.Add(childstart);
            var go = false;
            for (var element = e.state.elements.Last(); ; element = e.state.elements[e.state.elements.Count - 1] = e.state.elements.Last().next)
            {
                if (local.selects[0].state.elements[local.selects[0].state.n] == element)
                {
                    local.selects[0].state.n++;
                    go = true;
                }
                if (local.selects[1].state.elements[local.selects[1].state.n] == element)
                {
                    local.selects[1].state.n++;
                    go = true;
                }
                if (select || go)
                {
                    element.Key(e, local, ref select);
                    if (local.seln == 2) return 0;
                    go = false;
                }
                if (element == childend) break;
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return 0;
        }
    }
}
