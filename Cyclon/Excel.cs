using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    class Div : Element
    {
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
                if (statuses[kv.Key] == null) statuses[kv.Key] = kv.Value;
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
                if (statuses.ContainsKey("mouse")) mouse = MouseExe;
            }
            return base.Measure(m, local, ref order);
        }

        private bool MouseExe(MouseEvent mouse, Local local)
        {
            var m = statuses["mouse"] as SignalFunction;
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
    }
    class KeyEventObj : Val
    {
        public KeyEvent key;
        public KeyEventObj(KeyEvent key, Local local) : base(ObjType.KeyEventObj)
        {
            vmap = new Dictionary<string, Obj>();
            vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
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
                    if (word.name == "action")
                    {
                        return new Number((int)key.call) { cls = local.Int };
                    }
                    else if (word.name == "text")
                    {
                        return new StrObj(key.text) { cls = local.Str };
                    }
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
                    var word = val2 as Word;
                    n++;
                    if (word.name == "action")
                    {
                        return new Number((int)mouse.call) { cls = local.Int };
                    }
                    else if (word.name == "x")
                    {
                        return new Number(mouse.x) { cls = local.Int };
                    }
                    else if (word.name == "y")
                    {
                        return new Number(mouse.x) { cls = local.Int };
                    }
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
        public Dictionary<int, RowData> rowdatas = new Dictionary<int, RowData>();
        public Dictionary<int, ColData> coldatas = new Dictionary<int, ColData>();
        public SortedDictionary<int, SortedDictionary<int, Cell>> cells = new SortedDictionary<int, SortedDictionary<int, Cell>>();
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
        public override Element Measure(Measure m, Local local, ref int order)
        {
            var measure = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel };
            if (font != null) measure.font = font;
            var font0 = measure.font;
            measure.x += margins[1] + paddings[1];
            measure.y += margins[0] + paddings[0];
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
                        measure.py += 10f;
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
                            measure.px += 45f;
                        }
                    }
                    kv2.Value.Measure(measure, local, ref order);
                }
                measure.px = measure.x;
            }
            pos = new Point((int)m.x, (int)m.y);
            size2 = new Point((int)measure.sizex + margins[3] + paddings[3] + 1, (int)measure.py + margins[2] + paddings[2] + 1);
            update = false;
            if (m.sizex < measure.sizex) m.sizex = measure.sizex;
            m.py += measure.py + margins[2] + paddings[2] + 1;
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
                    var n0 = 0;
                    foreach (var kv in cells)
                    {
                        var n = 0;
                        var width = 0f;
                        for (; n0 < kv.Key; n0++)
                        {
                            if (rowdatas.ContainsKey(n0))
                            {
                                g.py += rowdatas[n0].height;
                            }
                            else
                            {
                                g.py += 10f;
                            }
                        }
                        foreach (var kv2 in cells[kv.Key])
                        {
                            for (; n < kv2.Key; n++)
                            {
                                if (coldatas.ContainsKey(n))
                                {
                                    g.px += coldatas[n].width;
                                }
                                else
                                {
                                    g.px += 45f;
                                }
                            }
                            kv2.Value.Draw(g, local, ref select);
                        }
                        g.px = g.x;
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
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(scroll.X, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(scroll.X, scroll.Y, size.X, size.Y), GraphicsUnit.Pixel);
                    }
                }
            }
            g.py += size2.Y;
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            var pos = 0f;
            for (var n = 0; ; n++)
            {
                var height = 10f;
                if (rowdatas.ContainsKey(n)) height = rowdatas[n].height;
                pos += height;
                if (e.y < pos + height)
                {
                    select = true;
                    var px = 0f;
                    for (var n2 = 0; ; n2++)
                    {
                        var width = 45f;
                        if (coldatas.ContainsKey(n2)) width = coldatas[n2].width;
                        px += width;
                        if (e.x < px + width)
                        {
                            Element element = new Cell();
                            if (cells.ContainsKey(n) && cells[n].ContainsKey(n2))
                            {
                                element = cells[n][n2];
                            }
                            e.state.elements.Add(element);
                            ret = element.Mouse(e, local);
                            e.state.elements.RemoveAt(e.state.elements.Count - 1);
                            return 0;
                        }
                    }
                }
            }
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            return -1;
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

    }
    class CloneElement : Div
    {
        public Element childstart;
        public CloneElement(Element start, Element end)
        {
            childstart = start;
            childend = end;
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            return base.Measure(m, local, ref order);
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            base.Draw(g, local, ref select);
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            return base.Mouse(e, local);
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            return base.Key(e, local, ref select);
        }
    }
}
