using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    enum Position{
        Fixed, Absolute, Relative
    }
    enum Layout
    {
        ocupy, none, left, right
    }
    enum MouseCall
    {
        MouseDown, MouseUp
    }
    enum KeyCall
    {
        KeyDown, KeyUp
    }
    enum SizeType
    {
        Auto, Break, Limit, Scroll
    }
    class CloneElement : Element
    {
        Element element;
        int fromn, ton;
    }
    class EndElement : Element
    {
        public EndElement(Element parent) : base(parent)
        {
            type = LetterType.ElemEnd;
            this.parent = parent;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            return null;
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            return -1;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            return -1;
        }
    }
    class Element
    {
        public Element next, before, parent, childend;
        public int[] margins = new int[4];
        public int[] paddings = new int[4];
        public PointF pos, scroll, size, size2;
        public SizeType xtype, ytype;
        public Position position;
        public Layout layout;
        public bool selectable;
        public Font font;
        public Graphics g;
        public bool single = false;
        public Func<MouseEvent, Local, bool> mouse;
        public Func<KeyEvent, Local, bool> key;
        public bool update = true;
        public bool recompile = false;
        public Brush background;
        public LetterType type;
        public int id;
        public Element()
        {
            childend = new EndElement(this);
            next = before = this;
            id = new Random().Next();
        }
        public Element(Element parent)
        {
            next = before = this;
            id = new Random().Next();
        }
        public void Next(Element element)
        {
            element.parent = parent;
            element.next = next;
            element.before = this;
            next.before = element;
            next = element;

        }
        public void AddRange(Element element)
        {
            Element next = null;
            for (; element.type != LetterType.ElemEnd; element = next)
            {
                next = element.next;
                childend.Before(element);
            }
        }
        public void FirstRange(Element element)
        {
            Element next = null;
            for (; element.type != LetterType.ElemEnd; element = next)
            {
                next = element.next;
                childend.Next(element);
            }
        }
        public void Before(Element element)
        {
            before.Next(element);
        }
        public void RemoveBefore()
        {
            before = before.before;
            before.next = this;
        }
        public virtual void add(Element e)
        {
            if (e.single)
            {
                if (childend.before is Line)
                {
                    childend.before.add(e);
                    if (e.type == LetterType.Kaigyou)
                    {
                        childend.Before(new Line());
                    }
                }
                else
                {
                    var line = new Line();
                    line.add(e);
                    childend.Before(line);
                }
            }
            else childend.Before(e);
        }
        public virtual void Draw(Graphic g, Local local, ref bool select)
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
                    for (var elem = childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
                    {
                        elem.Draw(g3, local, ref select);
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
        public virtual Element Measure(Measure m, Local local, ref int order)
        {
            var measure = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel };
            if (font != null) measure.font = font;
            var font0 = measure.font;
            measure.x += margins[1] + paddings[1];
            measure.y += margins[0] + paddings[0];
            measure.sizex = size.X - margins[3] - paddings[3];
            measure.sizey = size.Y - margins[2] - paddings[2];
            for (var element = childend.next; element != childend; element = element.next)
            {
                if (element is VirtualLine)
                {
                    element.RemoveBefore();
                    continue;
                }
            head:
                measure.state.elements.Add(this);
                var elem = element.Measure(measure, local, ref order);
                measure.font = font0;
                measure.state.elements.RemoveAt(measure.state.elements.Count - 1);
                if (elem is VirtualLine)
                {
                    element.Next(elem);
                    element = element.next;
                    goto head;
                }
            }
            pos = new Point((int)m.x, (int)m.y);
            size2 = new Point((int)measure.sizex + margins[3] + paddings[3] + 1, (int)measure.py + margins[2] + paddings[2] + 1);
            update = false;
            if (m.sizex < measure.sizex) m.sizex = measure.sizex;
            m.py += measure.py + margins[2] + paddings[2] + 1; 
            return null;
        }
        public virtual int Mouse(MouseEvent e, Local local)
        {
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            for (var element = childend.next; element != childend; element = element.next)
            {
                if (element.pos.Y <= e.y && e.y < element.pos.Y + element.size2.Y)
                {
                    select = true;
                    e.state.elements.Add(element);
                    ret = element.Mouse(e, local);
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                    return 0;
                }
            }
            if (!select)
            {
                var elem = this;
            head:
                elem = elem.childend.before;
                if (elem.single)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        local.selects[0] = local.selects[1] = new Select() { element = elem, n = 0 };
                    }
                    else
                    {
                        local.selects[1] = new Select() { element = elem, n = 0 };
                    }
                }
                else goto head;
            }
            return -1;
        }
        public virtual int Key(KeyEvent e, Local local, ref bool select)
        {
            if (select) select = false;
            e.state.elements.Add(childend.next);
            for (var element = e.state.elements.Last(); element != childend; element = e.state.elements[e.state.elements.Count - 1] = e.state.elements.Last().next)
            {
                element.Key(e, local, ref select);
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return 0;
        }
        public virtual int Count() { return 0; }
        public virtual int plus(int n)
        {
            return n;
        }
        public virtual void nextplus(State state)
        {
            if (state.elements.Last().type == LetterType.ElemEnd)
            {
                state.elements.RemoveAt(state.elements.Count - 1);
            }
            else
            {
                state.elements.Add(childend.next);
                state.elements[state.elements.Count - 2] = state.elements[state.elements.Count - 2].next;
            }
        }
        public virtual String Text{
            get
            {
                var ret = "";
                for (var elem = childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
                {
                    ret += elem.Text;
                }
                return ret;
            }
        }
    }
    class State
    {
        public List<Element> elements = new List<Element>();
        public List<int> ns = new List<int>();
        public Element element = new Letter() { type = LetterType.None };
        public List<Element> histories = new List<Element>();
        public void plus(int n)
        {
            var l1 = element as Letter;
        head:
            if (elements.Count == 0)
            {
                element = null;
                return;
            }
            element = elements.Last();
            var l2 = element as Letter;
            var n0 = n;
            n = element.plus(n);
            if (n >= 0)
            {
                element.nextplus(this);
            }
            else if (l2.type == LetterType.Space || (l1 != l2 && l1.type == LetterType.Kaigyou && l2.type == LetterType.Kaigyou))
            {
                n = n0;
                element.nextplus(this);
            }
            else
            {
                histories.Add(element);
                return;
            }
            goto head;
        }
        public Letter letter
        {
            get { return element as Letter; }
        }
        public bool lettersearch(LetterType[] success, LetterType[] fail)
        {
            var body = elements[elements.Count - 2];
            var elem = elements[elements.Count - 1];
        head:
            for (var element = elem; element.type != LetterType.ElemEnd; element = element.next)
            {
                var letter = element as Letter;
                for (var j = 0; j < success.Length; j++)
                {
                    if (letter.type == success[j]) return true;
                }
                for (var j = 0; j < fail.Length; j++)
                {
                    if (letter.type == fail[j]) return false;
                }
            }
        head2:
            body = body.next;
            if (body.type == LetterType.ElemEnd) throw new Exception();
            elem = body.next.childend.next;
            if (elem is VirtualLine) goto head2;
            goto head;
        }
        public void Update()
        {
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].update = true;
                elements[i].recompile = true;
            }
        }
    }
    class Measure {
        public float x, y;
        public float px, py;
        public float sizex, sizey;
        public float h;
        public SizeType xtype, ytype;
        public Font font;
        public Graphics g;
        public RichTextPanel panel;
        public State state = new State();
    }
    class Graphic
    {
        public Graphics g;
        public Font font;
        public float x, y;
        public float px, py;
        public float h;
    }
    class MouseEvent
    {
        public int x, y;
        public MouseCall call;
        public RichTextPanel panel;
        public State state = new State();
    }
    class KeyEvent
    {
        public String text;
        public Keys key;
        public KeyCall call;
        public State state = new State();
    }
    class Select
    {
        public Element element;
        public int n;
    }
    class Line : Element
    {
        public Element childstart;
        public Line()
        {
            childstart = childend.next;
        }
        public override void add(Element e)
        {
            if (e.single) childend.Before(e);
            else throw new Exception();
            if (childstart == childend) childstart = childend.next;
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (update)
            {
                if (recompile)
                {
                    var text = "";
                    int n = 0, n1 = -1, n2 = -1;
                    for(var element = childend.next; element != childend; element = element.next)
                    {
                        if (element is Letter)
                        {
                            var letter = element as Letter;
                            text += letter.text;
                            if (letter == local.selects[0].element)
                            {
                                n1 = n + local.selects[0].n;
                            }
                            if (letter == local.selects[1].element)
                            {
                                n2 = n + local.selects[1].n;
                            }
                            n += letter.text.Length;
                        }
                    }
                    var elements = new List<Element>(Form1.Compile(text));
                    childend.next = childend.before = childend;
                    for (var i = 0; i < elements.Count; i++) childend.Before(elements[i]);
                    childstart = childend.next;
                    for(var i = 0; n1 >= 0; i++)
                    {
                        if (elements[i] is Letter)
                        {
                            var letter = elements[i] as Letter;
                            n1 -= letter.text.Length;
                            if (n1 < 0)
                            {
                                local.selects[0].element = letter;
                                local.selects[0].n = letter.text.Length + n1;
                                break;
                            }
                        }
                    }
                    for (var i = 0; n2 >= 0; i++)
                    {
                        if (elements[i] is Letter)
                        {
                            var letter = elements[i] as Letter;
                            n2 -= letter.text.Length;
                            if (n2 < 0)
                            {
                                local.selects[1].element = letter;
                                local.selects[1].n = letter.text.Length + n2;
                                break;
                            }
                        }
                    }
                    recompile = false;
                }
                pos = new PointF(m.x + m.px, m.y + m.py);
                m.px = m.x;
                for (var element = childstart; element != childend; element = element.next)
                {
                    m.state.elements.Add(this);
                    var elem = element.Measure(m, local, ref order);
                    m.state.elements.RemoveAt(m.state.elements.Count - 1);
                    if (elem is VirtualLine)
                    {
                        var ve = elem as VirtualLine;
                        ve.childstart = element;
                        ve.childend = childend;
                        childend = element;
                        size2.Y = m.h;
                        size2.X = m.px;
                        m.py += m.h;
                        m.h = 0;
                        update = false;
                        m.sizey = m.py;
                        if (m.sizex < m.px) m.sizex = m.px;
                        return elem;
                    }
                }
                size2.Y = m.h;
                size2.X = m.px;
                m.py += m.h;
                m.h = 0;
                update = false;
                if (m.sizex < m.px) m.sizex = m.px;
                m.sizey = m.py;
            }
            else
            {
                if (m.sizex < size2.X) m.sizex = size2.X;
                m.py += size2.Y;
                m.sizey = m.py;
            }
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            g.h = size2.Y;
            for (var element = childstart; element != childend; element = element.next)
            {
                element.Draw(g, local, ref select);
                if (element is VirtualLine) break;
            }
            g.px = g.x;
            g.py += size2.Y;
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (mouse != null) mouse(e, local.local);
            if (e.x < childstart.pos.X)
            {
                if (e.call == MouseCall.MouseDown)
                {
                    local.selects[0] = local.selects[1] = new Select() { element = childstart, n = 0 };
                }
                else
                {
                    local.selects[1] = new Select() { element = childstart, n = 0 };
                }
                return 0;
            }
            else
            {
                List<Element> rets = new List<Element>();
                List<int> ns = new List<int>();
                var element = childstart;
                for (; element != childend; element = element.next)
                {
                    e.state.elements.Add(element);
                    var ret = element.Mouse(e, local);
                    if (ret >= 0)
                    {
                        rets.Add(element);
                        ns.Add(ret);
                    }
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                }
                if (rets.Count > 0)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        local.selects[0] = local.selects[1] = new Select() { element = rets[0], n = ns[0] };
                    }
                    else
                    {
                        local.selects[1] = new Select() { element = rets.Last(), n = ns.Last() };
                    }
                    return 0;
                }
                if (element.type == LetterType.ElemEnd) element = element.before;
                if (e.call == MouseCall.MouseDown)
                {
                    local.selects[0] = local.selects[1] = new Select() { element = element, n = 0 };
                }
                else
                {
                    local.selects[1] = new Select() { element = element, n = 0 };
                }
                return 0;
            }
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (select)
            {
                var sel2 = local.selects[(local.seln + 1) % 2];
                if (sel2.element == childstart && sel2.n == 0)
                {
                    select = false;
                }
                else recompile = update = true;
            }
            e.state.elements.Add(childstart);
            for( ; e.state.elements.Last().type != LetterType.ElemEnd;)
            {
                var ret = e.state.elements.Last().Key(e, local, ref select);
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return 0;
        }
    }
    class VirtualLine : Line
    {
        public VirtualLine()
        {
        }
        public override void nextplus(State state)
        {
            state.elements.RemoveAt(state.elements.Count - 1);
            state.ns.RemoveAt(state.ns.Count - 1);
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            return 0;
        }
        public override string Text
        {
            get { return ""; }
        }
    }
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
            foreach(var kv in divid.statuses)
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
            req.Store(new MouseEventObj(mouse, local) { cls = local.MouseEvent}, local);
            var res = new ConnectStock();
            m.basicexe(req, res, local);
            return false;
        }

        public bool KeyExe(KeyEvent key, Local local)
        {
            var k = statuses["key"] as SignalFunction;
            var req = new ConnectStock();
            req.Store(new KeyEventObj(key, local) { cls = local.KeyEvent}, local);
            var res = new ConnectStock();
            k.basicexe(req, res, local);
            return false;
        }
    }
    class KeyEventObj: Val
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
                        return new Number((int)mouse.call){ cls = local.Int };
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
    class Sheet: Element
    {
        public Dictionary<int, RowData> rowdatas = new Dictionary<int, RowData>();
        public Dictionary<int, ColData> coldatas = new Dictionary<int, ColData>();
        public Dictionary<int, Dictionary<int, Cell>> cells = new Dictionary<int, Dictionary<int, Cell>>();
        public void add(int x, int y, Element element)
        {
            if (!cells.ContainsKey(y))
            {
                cells[y] = new Dictionary<int, Cell>();
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
            cells = cells.OrderBy(x => x.Key).ToDictionary();
            var height = 0;
            foreach(var kv in cells)
            {
                cells[kv.Key] = kv.Value.OrderBy(x => x.Key).ToDictionary();
                var n = 0;
                var width = 0f;
                foreach(var kv2 in cells[kv.Key])
                {
                    for(; n < kv2.Key; n++)
                    {
                        if (coldatas.ContainsKey(n))
                        {
                            width += coldatas[n].width;
                        }
                        else
                        {
                            width += 45f;
                        }
                    }
                }
            }
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
    class RowData: Line
    {
    }
    class ColData
    {
        public float width;
    }
    class Cell : Element
    {

    }
}
