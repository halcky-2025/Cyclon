using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
        MouseDown, MouseUp,
        DoubleClick,
        MouseMove
    }
    enum KeyCall
    {
        KeyDown, KeyUp
    }
    enum Align
    {
        Left, Center, Right, Separate
    }
    enum SizeType
    {
        Auto, Break, Limit, Scroll
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
        public override void nextplus(State state)
        {
            state.elements.RemoveAt(state.elements.Count - 1);
        }
    }
    class Kouho: Element
    {
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (local.kouhos == null || local.kouhos.Count == 0) return null;
            else
            {
                pos.X = local.letter.pos2.X;
                pos.Y = local.letter.pos2.Y + local.letter.size2.Y;
                parent.margins[2] = 200;
                parent.margins[3] = 100;
                size.X = 100;
                size.Y = 200;
                xtype = SizeType.Auto;
                ytype = SizeType.Scroll;
                childend.next = childend.before = childend;
                var line0 = childend;
                for(var i = 0; i < local.kouhos.Count; i++)
                {
                    var line = new Line();
                    line.childend.Next(new Letter() { text = local.kouhos.Keys[i] });
                    line.childstart = line.childend.next;
                    line0.Next(line);
                    line0 = line;
                }
            }
            return base.Measure(m, local, ref order);
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            if (local.kouhos == null || local.kouhos.Count == 0) return;
            base.Draw(g, local, ref select);
        }
        public override int plus(int n)
        {
            return n;
        }
        public override void nextplus(State state)
        {
            state.elements[state.elements.Count - 1] = state.elements.Last().next;
            if (state.elements.Last().type == LetterType.ElemEnd) state.elements.RemoveAt(state.elements.Count - 1);
        }
    }
    class Element
    {
        public Element next, before, parent, childend;
        public int[] margins = new int[4];
        public int[] paddings = new int[4];
        public PointF pos2, pos, scroll, size, size2;
        public SizeType xtype, ytype;
        public Position position = Position.Relative;
        public Layout layout;
        public bool selectable;
        public Font font;
        public bool single = false;
        public Func<MouseEvent, Local, bool> mouse;
        public Func<KeyEvent, Local, bool> key;
        public bool update = true;
        public bool recompile = false;
        public Brush background;
        public LetterType type;
        public Align align;
        public int id;
        public int index;
        public List<Element> groups = new List<Element>();
        public Func<String, Local, List<Element>> Recompile;
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
            Element before = null;
            for (; element.type != LetterType.ElemEnd; element = before)
            {
                before = element.before;
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
            var px = g.px;
            var py = g.py;
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
                    for (var i = 0; i < groups.Count; i++)
                    {
                        var elem = groups[i];
                        if (elem.position == Position.Absolute)
                        {
                            var px1 = 0;
                            var py1 = g3.py;
                            var x1 = g3.x;
                            var y1 = g3.y;
                            g3.px = 0;
                            g3.py = 0;
                            g3.x = 0;
                            g3.y = 0;
                            elem.Draw(g3, local, ref select);
                            g3.px = px1;
                            g3.py = py1;
                            g3.x = x1;
                            g3.y = y1;
                            continue;
                        }
                        else if (elem.position == Position.Fixed)
                        {
                            var px1 = 0;
                            var py1 = g3.py;
                            var x1 = g3.x;
                            var y1 = g3.y;
                            g3.px = scroll.X;
                            g3.py = scroll.Y;
                            g3.x = 0;
                            g3.y = 0;
                            elem.Draw(g3, local, ref select);
                            g3.px = px1;
                            g3.py = py1;
                            g3.x = x1;
                            g3.y = y1;
                            continue;
                        }
                        else if (align == Align.Left)
                        {
                            g3.px = g3.x;
                            elem.Draw(g3, local, ref select);
                        }
                        else if (align == Align.Center)
                        {
                            g3.px = 0;
                            g3.px = (size2.X - elem.size2.X) / 2;
                            elem.Draw(g3, local, ref select);
                        }
                        else if (align == Align.Right)
                        {
                            g3.px = size2.X - elem.size2.X;
                            elem.Draw(g3, local, ref select);
                        }
                        else if (align == Align.Separate)
                        {
                            if (first)
                            {
                                g3.px = g3.x;
                                elem.Draw(g3, local, ref select);
                                sizey = elem.size2.Y;
                            }
                            else
                            {
                                g3.px = (size2.X - elem.size2.X);
                                elem.Draw(g3, local, ref select);
                                g3.py += Math.Max(sizey, elem.size2.Y);
                                sizey = 0;
                            }
                            first = !first;
                            continue;
                        }
                        if (elem.ytype == SizeType.Limit || elem.ytype == SizeType.Scroll) sizey = elem.size.Y;
                        else sizey = elem.size2.Y;
                        g3.py += sizey;
                    }
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px + pos.X, g.y + g.py + pos.Y));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px + pos.X, g.y + g.py + pos.Y, bitmap.Width, size.Y), new RectangleF(0, scroll.Y, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + pos.X + size.X - 10, g.y + g.py + pos.Y, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(g.x + g.px + pos.X + size.X - 10, g.y + g.py + pos.Y + scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto || ytype == SizeType.Break)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px + pos.X, g.y + g.py + pos.Y, size.X, bitmap.Height), new RectangleF(scroll.X, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px + pos.X, g.y + g.py + pos.Y, size.X, size.Y), new RectangleF(scroll.X, scroll.Y, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + pos.X + size.X - 10, g.y + g.py + pos.Y, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(g.x + g.px + pos.X + size.X - 10, g.y + g.py + pos.Y + scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + pos.X, g.y + g.py + pos.Y + size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(g.x + g.px + pos.X + scroll.X / size2.X * size.X, g.y + g.py + pos.Y + size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public virtual Element Measure(Measure m, Local local, ref int order)
        {
            var measure = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel, state = m.state, Recompile = m.Recompile };
            if (font != null) measure.font = font;
            if (Recompile != null) measure.Recompile = Recompile;
            var font0 = measure.font;
            measure.x += margins[1] + paddings[1];
            measure.y += margins[0] + paddings[0];
            measure.sizex = size.X - margins[3] - paddings[3] - 1;
            measure.sizey = size.Y - margins[2] - paddings[2] - 1;
            var first = true;
            float sizex, sizey;
            float sizex2 = 0, sizey2 = 0;
            groups = new List<Element>();
            for (var element = childend.next; element != childend; element = element.next)
            {
                if (element is VirtualLine)
                {
                    element.RemoveBefore();
                    continue;
                }
                groups.Add(element);
            head:
                measure.state.elements.Add(element);
                Element elem;
                if (element.position == Position.Fixed)
                {
                    var px = measure.px;
                    var py = measure.py;
                    measure.px = scroll.X;
                    measure.py = scroll.Y;
                    elem = element.Measure(measure, local, ref order);
                    measure.px = px;
                    measure.py = py;
                    measure.font = font0;
                    measure.state.elements.RemoveAt(measure.state.elements.Count - 1);
                    if (elem is VirtualLine)
                    {
                        element.Next(elem);
                        groups.Add(elem);
                        element = element.next;
                        goto head;
                    }
                    continue;
                }
                else if (element.position == Position.Absolute)
                {
                    var px = measure.px;
                    var py = measure.py;
                    measure.px = 0;
                    measure.py = 0;
                    elem = element.Measure(measure, local, ref order);
                    measure.px = px;
                    measure.py = py;
                    measure.font = font0;
                    measure.state.elements.RemoveAt(measure.state.elements.Count - 1);
                    if (elem is VirtualLine)
                    {
                        element.Next(elem);
                        groups.Add(elem);
                        element = element.next;
                        goto head;
                    }
                    continue;
                }
                else
                {
                    elem = element.Measure(measure, local, ref order);
                }
                if (element.ytype == SizeType.Limit || element.ytype == SizeType.Scroll) sizey = element.size.Y;
                else sizey = element.size2.Y;
                if (element.xtype == SizeType.Limit || element.xtype == SizeType.Scroll) sizex = element.size.X;
                else sizex = element.size2.X;
                if (align == Align.Separate)
                {
                    if (first)
                    {
                        sizex2 = sizex;
                        sizey2 = sizey;
                        sizey = 0;
                    }
                    else
                    {
                        sizex += sizex2;
                        sizey = Math.Max(sizey, sizey2);
                    }
                    first = !first;
                }
                measure.py += sizey;
                if (measure.sizex < sizex) measure.sizex = sizex;
                measure.font = font0;
                measure.state.elements.RemoveAt(measure.state.elements.Count - 1);
                if (elem is VirtualLine)
                {
                    element.Next(elem);
                    groups.Add(elem);
                    element = element.next;
                    goto head;
                }
            }
            groups.OrderBy((x) => x.index);
            pos2 = new Point((int)(m.x + pos.X), (int)(m.y + m.py + pos.Y));
            size2 = new Point((int)measure.sizex + margins[3] + paddings[3] + 1, (int)measure.py + margins[2] + paddings[2] + 1);
            if (xtype == SizeType.Scroll) size2.Y += 10;
            if (ytype == SizeType.Scroll) size2.X += 10;
            size2.X = Math.Max(size2.X, size.X);
            size2.Y = Math.Max(size2.Y, size.Y);
            update = false;
            return null;
        }
        public virtual int Mouse(MouseEvent e, Local local)
        {
            if (e.call == MouseCall.MouseDown)
            {
                if (xtype == SizeType.Scroll)
                {
                    if (e.y >= size.Y - 10)
                    {
                        if (e.x < scroll.X / size2.X * size.X)
                        {

                            scroll.X -= size.X - 10f;
                            if (scroll.X < 0) scroll.X = 0;
                        }
                        else if (e.x < (scroll.X + size.X) / size2.X * size.X)
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
                                return true;
                            }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                        else
                        {
                            scroll.X += size.X - 10f;
                            if (scroll.X > size2.X - size.X) scroll.X = size2.X - size.X;
                        }
                        return -1;
                    }
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (e.y <scroll.Y / size2.Y * (size.Y - 12))
                    {
                        scroll.Y -= size.Y - 10f;
                        if (scroll.Y < 0) scroll.Y = 0;
                    }
                    else if (e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
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
                                return true;
                            }
                        };
                        Form1.SetCapture(local.panel.Handle);
                    }
                    else
                    {
                        scroll.Y += size.Y - 10f;
                        if (scroll.Y > size2.Y - size.Y) scroll.Y = size2.Y - size.Y;
                    }
                    return -1;
                }
            }
            e.x += (int)scroll.X;
            e.y += (int)scroll.Y;
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            for (var i = 0; i < groups.Count; i++)
            {
                var element = groups[i];
                if (element.pos2.Y <= e.y && e.y < element.pos2.Y + element.size2.Y)
                {
                    e.y -= (int)element.pos2.Y;
                    select = true;
                    var elem = element;
                head:
                    if (elem is VirtualLine)
                    {
                        elem = element.before;
                        goto head;
                    }
                    e.state.elements.Add(elem);
                    if (align == Align.Left) { }
                    else if (align == Align.Center)
                    {
                        e.x -= (int)((size2.X - element.size2.X) / 2);
                    }
                    else if (align == Align.Right)
                    {
                        e.x -= (int)(size2.X - element.size2.X);
                    }
                    else if (align == Align.Separate)
                    {
                        if (e.x > (int)(size2.X - element.next.size2.X))
                        {
                            e.state.elements[e.state.elements.Count - 1] = element.next;
                            e.x -= (int)(size2.X - element.next.size2.X);
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
                var elem = this;
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
                    else if (e.call == MouseCall.MouseUp || e.call == MouseCall.MouseMove)
                    {
                        local.selects[1] = new Select() { state = state, n = 0 };
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
            var go = false;
            for (var element = e.state.elements.Last(); element != childend; element = e.state.elements[e.state.elements.Count - 1] = e.state.elements.Last().next)
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
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return 0;
        }
        public virtual void SelectExe(SelectE e, Local local, ref bool select)
        {
            if (select) select = false;
            e.state.elements.Add(childend.next);
            var go = false;
            for (var element = e.state.elements.Last(); element != childend; element = e.state.elements[e.state.elements.Count - 1] = e.state.elements.Last().next)
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
                    element.SelectExe(e, local, ref select);
                    if (local.seln == 2) return;
                    go = false;
                }
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
        }
        public virtual int Count() { return 0; }
        public virtual int plus(int n)
        {
            return n;
        }
        public virtual void nextplus(State state)
        {
            state.elements.Add(childend.next);
            state.elements[state.elements.Count - 2] = state.elements[state.elements.Count - 2].next;
        }
        public virtual String Text(Local local)
        {
            var ret = "";
            for (var elem = childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
            {
                ret += elem.Text(local);
            }
            return ret;
        }
        public virtual String Text2(Local local)
        {
            var ret = "";
            for (var elem = childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
            {
                ret += elem.Text2(local);
            }
            return ret;

        }
        public virtual void setText(String text, Local local)
        {
            var letters = Form1.Compile(text + "\0", local);
            childend.next = childend.before = childend;
            for (var i = 0; i < letters.Count; i++) add(letters[i]);
            local.xtype = SizeType.Scroll;
            local.ytype = SizeType.Scroll;
        }
    }
    class State
    {
        public List<Element> elements = new List<Element>();
        public Element element = new Letter() { type = LetterType.None };
        public List<Element> histories = new List<Element>();
        public int n = 0;
        public virtual void plus(int n)
        {
            var comany = false;
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
            if (l2 == null)
            {
                element.nextplus(this);
            }
            else if (l2.type == LetterType.End)
            {
                element = l2;
                return;
            }
            else if (l2.type == LetterType.CommentMany)
            {
                comany = !comany;
                n = n0;
                element.nextplus(this);
            }
            else if (comany)
            {
                n = n0;
                element.nextplus(this);
            }
            else if (l2.type == LetterType.Space || l2.type == LetterType.Select || l2.type == LetterType.CommentSingle || (l1 != l2 && l1.type == LetterType.Kaigyou && l2.type == LetterType.Kaigyou))
            {
                n = n0;
                element.nextplus(this);
                    }
            else if (n >= 0)
            {
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
        public State Clone()
        {
            var state = new State();
            for (var i = 0; i < elements.Count; i++) state.elements.Add(elements[i]);
            return state;
        }
    }
    class SelectE
    {
        public State state = new State();
        public Action<SelectE, Element, Select, Select> Select;
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
        public Func<String, Local, List<Element>> Recompile;
    }
    class Graphic
    {
        public Graphics g;
        public Font font;
        public float x, y;
        public float px, py;
        public float h;
    }
    class Capture
    {
        public MouseEvent down;
        public Func<Capture, MouseEvent, bool> capture;
    }
    class MouseEvent
    {
        public int x, y;
        public Point basepos;
        public MouseCall call;
        public RichTextPanel panel;
        public State state = new State();
    }
    class KeyEvent
    {
        public String text;
        public Keys key;
        public bool ctrl;
        public bool shift;
        public KeyCall call;
        public State state = new State();
    }
    class Select
    {
        public State state;
        public int n;
    }
    class Line : Element
    {
        public Element childstart;
        public Element childfinish;
        public Line()
        {
            type = LetterType.Line;
            childstart = childend.next;
            childfinish = childend;
        }
        public override void add(Element e)
        {
            if (e.single)
            {
                if (childend.before.type == LetterType.End)
                {
                    childend.before.Before(e);
                }
                else childend.Before(e);
            }
            else throw new Exception();
            childstart = childend.next;
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (update)
            {
                groups = new List<Element>();
                if (recompile)
                {
                    var text = "";
                    int n = 0, n1 = -1, n2 = -1;
                    for(var element = childend.next; element != childend; element = element.next)
                    {
                        if (element is Span)
                        {
                            var letter = element as Span;
                            text += letter.Text3(local);
                            if (letter == local.selects[0].state.elements.Last())
                            {
                                n1 = n + local.selects[0].n;
                            }
                            if (letter == local.selects[1].state.elements.Last())
                            {
                                n2 = n + local.selects[1].n;
                            }
                            n += letter.text.Length;
                        }
                        else if (element is Letter)
                        {
                            var letter = element as Letter;
                            text += letter.text;
                            if (letter == local.selects[0].state.elements.Last())
                            {
                                n1 = n + local.selects[0].n;
                            }
                            if (letter == local.selects[1].state.elements.Last())
                            {
                                n2 = n + local.selects[1].n;
                            }
                            n += letter.text.Length;
                        }
                    }
                    var elements = new List<Element>(m.Recompile(text, local));
                    childend.next = childend.before = childend;
                    var line = this;
                    for (var i = 0; i < elements.Count; i++)
                    {
                        line.childend.Before(elements[i]);
                        if (elements[i].type == LetterType.Kaigyou && i != elements.Count -1)
                        {
                            var line2 = new Line();
                            line.Next(line2);
                            line.childstart = line.childend.next;
                            line = line2;
                        }
                    }
                    line.childstart = line.childend.next;
                    for(var i = 0; n1 >= 0; i++)
                    {
                        if (elements[i] is Letter)
                        {
                            var letter = elements[i] as Letter;
                            n1 -= letter.text.Length;
                            if (n1 < 0)
                            {
                                local.selects[0].state.elements[local.selects[0].state.elements.Count - 2] = letter.parent;
                                local.selects[0].state.elements[local.selects[0].state.elements.Count - 1] = letter;
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
                                local.selects[1].state.elements[local.selects[1].state.elements.Count - 2] = letter.parent;
                                local.selects[1].state.elements[local.selects[1].state.elements.Count - 1] = letter;
                                local.selects[1].n = letter.text.Length + n2;
                                break;
                            }
                        }
                    }
                    recompile = false;
                }
                pos2 = new PointF(m.x + m.px, m.y + m.py);
                m.px = m.x;
                for (var element = childstart; element != childend; element = element.next)
                {
                    m.state.elements.Add(element);
                    var elem = element.Measure(m, local, ref order);
                    m.state.elements.RemoveAt(m.state.elements.Count - 1);
                    if (elem is VirtualLine)
                    {
                        var ve = elem as VirtualLine;
                        ve.childstart = element;
                        ve.childend = childend;
                        ve.childfinish = childend;
                        childfinish = element;
                        size2.Y = m.h;
                        size2.X = m.px;
                        m.h = 0;
                        update = false;
                        groups.OrderBy((x) => x.index);
                        return elem;
                    }
                    groups.Add(element);
                }
                size2.Y = m.h;
                size2.X = m.px;
                m.h = 0;
                update = false;
                groups.OrderBy((x) => x.index);
            }
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            g.h = size2.Y;
            for (var element = childstart; element != childfinish; element = element.next)
            {
                element.Draw(g, local, ref select);
                if (element is VirtualLine) break;
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (mouse != null) mouse(e, local.local);
            if (e.x < childstart.pos2.X)
            {
                var state = e.state.Clone();
                state.elements.Add(childstart);
                if (e.call == MouseCall.MouseDown)
                {
                    local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                }
                else if (e.call == MouseCall.MouseUp || e.call == MouseCall.MouseMove)
                {
                    state.elements.Add(childstart);
                    local.selects[1] = new Select() { state = state, n = 0 };
                }
                return 0;
            }
            else
            {
                List<Element> rets = new List<Element>();
                List<int> ns = new List<int>();
                Element element = null;
                for (var i = 0; i < groups.Count; i++)
                {
                    element = groups[i];
                    e.state.elements.Add(element);
                    var ret = element.Mouse(e, local);
                    if (ret >= 0)
                    {
                        rets.Add(element);
                        ns.Add(ret);
                    }
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                }
                var state = e.state.Clone();
                if (rets.Count > 0)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        state.elements.Add(rets[0]);
                        local.selects[0] = local.selects[1] = new Select() { state = state, n = ns[0] };
                    }
                    else if (e.call == MouseCall.MouseUp || e.call == MouseCall.MouseMove)
                    {
                        state.elements.Add(rets.Last());
                        local.selects[1] = new Select() { state = state, n = ns.Last() };
                    }
                    return 0;
                }
                if (element.type == LetterType.ElemEnd) element = element.before;
                state.elements.Add(element);
                if (e.call == MouseCall.MouseDown)
                {
                    local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                }
                else if (e.call == MouseCall.MouseUp || e.call == MouseCall.MouseMove)
                {
                    local.selects[1] = new Select() { state = state, n = 0 };
                }
                return 0;
            }
        }
        public override void SelectExe(SelectE e, Local local, ref bool select)
        {
            if (select)
            {
                var sel2 = local.selects[(local.seln + 1) % 2];
                if (sel2.state.elements.Last() == childstart && sel2.n == 0)
                {
                    select = false;
                }
                else recompile = update = true;
            }
            e.state.elements.Add(childstart);
            var go = false;
            for (; e.state.elements.Last().type != LetterType.ElemEnd;)
            {
                e.state.elements.Last().SelectExe(e, local, ref select);
                if (local.seln == 2) return;
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (select)
            {
                var sel2 = local.selects[(local.seln + 1) % 2];
                if (sel2.state.elements.Last() == childstart && sel2.n == 0)
                {
                    select = false;
                }
                else recompile = update = true;
            }
            e.state.elements.Add(childstart);
            var go = false;
            for( ; e.state.elements.Last().type != LetterType.ElemEnd;)
            {
                var ret = e.state.elements.Last().Key(e, local, ref select);
                if (local.seln == 2) return 0;
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return 0;
        }
        public override string Text2(Local local)
        {
            var ret = "`";
            var tex = "";
            for (var elem = childstart; elem.type != LetterType.ElemEnd; elem = elem.next)
            {
                if (local.selects[0].state.elements.Last() == elem)
                {
                    ret = (char)('\uE000' + (tex.Length + local.selects[0].n) * 2) + ret;
                }
                if (local.selects[1].state.elements.Last() == elem)
                {
                    ret = (char)('\uE000' + (tex.Length + local.selects[1].n) * 2 + 1) + ret;
                }
                tex += elem.Text2(local);
            }
            return ret + tex + "`";
        }
    }
    class VirtualLine : Line
    {
        public VirtualLine()
        {
            type = LetterType.VirtualLine;
        }
        public override void nextplus(State state)
        {
            state.elements.RemoveAt(state.elements.Count - 1);
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            return 0;
        }
        public override string Text(Local local)
        {
            return "";
        }
        public override string Text2(Local local)
        {
            return "";
        }
    }
}
