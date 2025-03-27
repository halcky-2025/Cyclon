using Cyclon;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.DirectoryServices;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Markup;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Cyclon
{
    partial class Obj
    {
        public Letter letter;
        public List<Obj> children = new List<Obj>();
        public ObjType type;
        public Dictionary<String, Func<String, Local, Obj, Obj>> opes = new Dictionary<string, Func<String, Local, Obj, Obj>>();
        public Obj(ObjType type)
        {
            this.type = type;
        }
        public static Type Error(ObjType type, Letter let, String error)
        {
            if (type == ObjType.Error)
            {
            }
            else if (type == ObjType.NG)
            {

            }
            return new Type(type) { let = let, error = error };
        }
        public virtual Obj exe(Local local)
        {
            throw new Exception();
        }
        public virtual Obj exep(ref int n, Local local, Primary primary)
        {
            throw new Exception();
        }
        public virtual Obj Getter(Local local)
        {
            return this;
        }
        public virtual Obj Self(Local local)
        {
            return this;
        }
        public virtual Obj ope(String key, Local local, Obj val2)
        {
            return opes[key](key, local, val2);
        }
        public virtual Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
        public virtual Obj Clone() { return this; }
        public virtual String Text()
        {
            return "";

        }
        public virtual Obj Hokan(Obj val2, float p, Local local)
        {
            return this;
        }
    }
    partial class Primary : Obj
    {
        public SingleOp singleop;
        public Primary() : base(ObjType.Primary)
        {
        }
        public override Obj exe(Local local)
        {
            if (local.comments.Count > 0) local.comments.Last().Start();
            Obj val1 = null;
            for (var i = 0; i < children.Count - 1;)
            {
                switch (children[i].type)
                {
                    case ObjType.Comment2:
                        i++;
                        continue;
                    case ObjType.Htm:
                    case ObjType.TagBlock:
                        children[i].Clone().exep(ref i, local, this);
                        i++;
                        val1 = null;
                        break;
                    case ObjType.Gene:
                    case ObjType.Model:
                    case ObjType.Class:
                    case ObjType.Word:
                    case ObjType.Number:
                    case ObjType.FloatVal:
                    case ObjType.BoolVal:
                    case ObjType.StrObj:
                    case ObjType.Bracket:
                    case ObjType.Print:
                    case ObjType.Return:
                    case ObjType.Goto:
                    case ObjType.Continue:
                    case ObjType.Var:
                    case ObjType.If:
                    case ObjType.While:
                    case ObjType.For:
                    case ObjType.Switch:
                    case ObjType.GeneLabel:
                    case ObjType.Block:
                    case ObjType.Comment:
                    case ObjType.Client:
                    case ObjType.Server:
                    case ObjType.Signal:
                    case ObjType.ServerClient:
                    case ObjType.Dolor:
                    case ObjType.Mountain:
                        if (val1 == null)
                        {
                            val1 = children[i].Clone().exep(ref i, local, this);
                            i++;
                        }
                        else
                        {
                            val1 = val1.Getter(local);
                            val1 = val1.Primary(ref i, local, this, children[i].Clone().exep(ref i, local, this));
                        }
                        break;
                    case ObjType.Dot:
                    case ObjType.Left:
                    case ObjType.Right:
                        val1 = val1.Getter(local);
                        val1 = val1.Primary(ref i, local, this, children[i]);
                        break;
                    case ObjType.CallBlock:
                        if (val1 == null)
                        {
                            val1 = children[i].exep(ref i, local, this);
                            i++;
                        }
                        else
                        {
                            val1 = val1.Getter(local);
                            val1 = val1.Primary(ref i, local, this, children[i]);
                        }
                        break;
                    default:
                        throw new Exception();
                }
                if (val1 != null && (val1.type == ObjType.Return || val1.type == ObjType.Break || val1.type == ObjType.Continue)) return val1;
            }
            if (val1 == null) val1 = children.Last();
            SingleOp op = null;
            if (singleop != null)
            {
                var str = singleop.name;
                switch (str)
                {
                    case ".":
                        op = singleop as SingleOp; break;
                    case "*":
                    case ">>":
                        op = singleop as SingleOp;
                        if (val1 != children.Last()) val1 = val1.ope(str, local, null); break;
                    default:
                        val1 = val1.Getter(local);
                        val1 = val1.ope(str, local, null); break;
                }
            }
            if (local.comments.Count > 0) local.comments.Last().Single(op, local);
            return val1;
        }
    }
    class FinishException : Exception { }
    class Label
    {
        public Letter letter;
        public String name;
        public int n;
        public Block block;
        public int loop_m = -1;
        public int lab_loop_m = -1;
        public int sw_loop_m = -1;
        public String id;
        public Dictionary<String, Label> labelmap = new Dictionary<string, Label>();
    }
    class Id
    {
        public String[] names;
        public List<Label> labels;
        public int n;
        public Id(int n, List<Label> labels, params String[] names)
        {
            this.n = n;
            this.names = names;
            this.labels = labels;
        }
    }
    class TextLocal: Local
    {
        public History history = new History();
        public TextLocal()
        {
            Recompile = Form1.Compile;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (e.ctrl)
            {
                switch(e.key)
                {
                    case Keys.Y:
                        var go = history.Go();
                        if (go == null) break;
                        setText(go, local);
                        //var item = form.Start(local);
                        //item.exe(local);
                        panel.input = true;
                        return -1;
                    case Keys.Z:
                        var back = history.Back();
                        if (back == null) break;
                        setText(back, local);
                        //var item = form.Start(local);
                        //item.exe(local);
                        panel.input = true;
                        return -1;
                }
            }
            var ret = base.Key(e, local, ref select);
            if (panel.input == true) history.Add(Text(local));
            return ret;
        }
    }
    partial class Local: Element
    {
        public RichTextPanel panel;
        public String name = "server";
        public State state;
        public Select[] selects = new Select[2] { new Select(), new Select() };
        public int seln;
        public int countn = -1;
        public List<int> indents = new List<int>();
        public RichTextBox console;
        public List<OpeFunc> operators = new List<OpeFunc>();
        public List<List<Block>> blockslist = new List<List<Block>>();
        public ModelObj Object = new ModelObj() { initial = true, letter = new Letter() { name = "object" } };
        public ModelObj Int = new ModelObj() { initial = true, letter = new Letter() { name = "int" } };
        public ModelObj Str = new ModelObj() { initial = true, letter = new Letter() { name = "Str" } };
        public ModelObj Bool = new ModelObj() { initial = true, letter = new Letter() { name = "bool" } };
        public ModelObj Float = new ModelObj() { initial = true, letter = new Letter() { name = "float" } };
        public ModelObj MouseEvent = new ModelObj() { initial = true, letter = new Letter() { name = "MouseEvent" } };
        public ModelObj KeyEvent = new ModelObj() { initial = true, letter = new Letter() { name = "KeyEvent" } };
        public Gene gene = new Gene();
        public Block block;
        public Dictionary<String, Label> labelmap = new Dictionary<string, Label>();
        public List<Id> ids = new List<Id>();
        public Stock db = new Stock(), mem = new Stock();
        public CommentLet comlet;
        public List<CommentLet> comments = new List<CommentLet>();
        public Dictionary<String, Obj> sigmap = new Dictionary<string, Obj>();
        public List<AnimationFunction> animations = new List<AnimationFunction>();
        public Element letter;
        public SortedList<String, Obj> kouhos;
        public List<Action> calls = new List<Action>();
        public Local local
        {
            get
            {
                return panel.form.local;
            }
        }
        public Vision vision
        {
            get
            {
                return panel.form.vis;
            }
        }
        public Local()
        {
            childend.Before(new Kouho() { position = Position.Absolute });
            selects[0].state = selects[1].state = new State();
            selects[0].state.elements.Add(new EndElement(null));
            Float.extends.Add(Int);
            operators.Add(new OpeFunc() { types = new LetterType[] { LetterType.Equal, LetterType.Colon, LetterType.In} });
            operators.Add(new OpeFunc() { types = new LetterType[] { LetterType.MoreEqual, LetterType.MoreThan, LetterType.LessEqual, LetterType.LessThan, LetterType.EqualEqual, LetterType.NotEqual } });
            operators.Add(new OpeFunc() { types = new LetterType[]{ LetterType.Plus, LetterType.Minus } });
            operators.Add(new OpeFunc() { types = new LetterType[] { LetterType.Mul, LetterType.Div, LetterType.Percent } });
        }
        public void Setid()
        {
            var ls = new List<Label>(labelmap.Values);
            foreach(var l in ls)
            {
                if (l.name == "end") continue;
                if (l.labelmap.Count == 0) throw new Exception();
                var ls2 = new List<Label>(l.labelmap.Values);
                foreach (var l2 in ls2)
                {
                    if (l2.labelmap.Count == 0)
                    {
                        var n = new Random().Next(ls2.Count);
                        ids.Add(new Id(n, ls2, l.name, ls2[n].name));
                    }
                    else
                    {
                        var ls3 = new List<Label>(l2.labelmap.Values);
                        var n = new Random().Next(ls3.Count);
                        ids.Add(new Id(n, ls3, l.name, l2.name, ls3[n].name));
                    }
                }
            }
        }
        public int indent
        {
            get { return indents[indents.Count - 1]; }
        }
        public List<Block> blocks
        {
            get { return blockslist.Last(); }
            set { blockslist.Add(value); }
        }
        public void declare(String name, Obj obj)
        {
            blocks.Last().vmap[name] = obj;
        }
        public Obj get(String name)
        {
            for (var i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].vmap.ContainsKey(name)) return blocks[i].vmap[name];
            }
            if (local.sigmap.ContainsKey(name)) return local.sigmap[name];
            throw new Exception();
        }
        public void label(String name)
        {
            //block.labelmap[name] = new Label { name = name, n = block.lines.Count, block = block };
        }
    }
    class Vision: Local
    {
        public Dictionary<String, Div> emap = new Dictionary<string, Div>();
        public Vision()
        {
            Recompile = Form1.Compile;
        }
        public Div GetById(String id)
        {
            if (emap.ContainsKey(id))
            {
                return emap[id];
            }
            else return null;
        }
        public void addcomment(Comment comment)
        {
            CommentLet let = comment.letter as CommentLet;
            AddElem(let.childend.next.childend, childend);
        }
        public void AddElem(Element childend, Element owner)
        {

            Element next2 = null;
            for (var elem = childend.next; elem.type != LetterType.ElemEnd; elem = next2)
            {
                next2 = elem.next;
                if (elem.type == LetterType.Div || elem.type == LetterType.Cell || elem.type == LetterType.Sheet)
                {
                    var div = elem as Div;
                    if (div.sop != null)
                    {
                        if (div.sop == "+")
                        {
                            if (div.id != null)
                            {
                                var elem2 = GetById(div.id);
                                emap[div.id] = div;
                                if (elem2 != null)
                                {
                                    div.SetStatus(elem2, local);
                                    if (elem.type == LetterType.Sheet || elem2.type == LetterType.Sheet)
                                    {
                                        var sheet = elem as Sheet;
                                        var sheet_old = elem2 as Sheet;
                                        sheet.cells = sheet_old.cells;
                                        sheet.rowdatas = sheet_old.rowdatas;
                                        sheet.coldatas = sheet_old.coldatas;
                                        sheet.x = sheet_old.x;
                                        sheet.y = sheet_old.y;
                                        sheet.history = new SheetHistory();
                                    }
                                    div.scroll.X = elem2.scroll.X;
                                    div.scroll.Y = elem2.scroll.Y;
                                    if (owner != this.childend) div.next.RemoveBefore();
                                    if (div.position == Position.Absolute || div.position == Position.Fixed)
                                    {
                                        elem2.next.RemoveBefore();
                                        this.childend.Before(div);
                                    }
                                    else
                                    {
                                        elem2.Next(div);
                                        div.RemoveBefore();
                                    }
                                    AddElem(elem.childend, elem.childend);
                                    div.FirstRange(elem2.childend.before);
                                    div.sop = null;
                                    continue;
                                }
                                else if (div.position == Position.Absolute || div.position == Position.Fixed)
                                {
                                    div.next.RemoveBefore();
                                    this.childend.Before(div);
                                }
                                else if (owner == this.childend) this.childend.Before(elem);
                            }
                            else if (div.position == Position.Absolute || div.position == Position.Fixed)
                            {
                                div.next.RemoveBefore();
                                this.childend.Before(div);
                            }
                            else if (owner == this.childend) this.childend.Before(elem);
                            div.sop = null;
                        }
                        else if (div.sop == "*")
                        {
                            if (div.id != null)
                            {
                                var elem2 = GetById(div.id);
                                emap[div.id] = div;
                                if (elem2 != null)
                                {
                                    div.SetStatus(elem2, local);
                                    div.scroll.X = elem2.scroll.X;
                                    div.scroll.Y = elem2.scroll.Y;
                                    if (owner != this.childend) div.next.RemoveBefore();
                                    if (div.position == Position.Absolute || div.position == Position.Fixed)
                                    {
                                        elem2.next.RemoveBefore();
                                        childend.Before(div);
                                    }
                                    else
                                    {
                                        elem2.Next(div);
                                        div.RemoveBefore();
                                    }
                                }
                                else if (div.position == Position.Absolute || div.position == Position.Fixed)
                                {
                                    div.next.RemoveBefore();
                                    this.childend.Before(div);
                                }
                                else if (owner == this.childend) owner.Before(elem);
                            }
                            else if (childend != owner) owner.Before(elem);
                            div.sop = null;
                        }
                        else if (div.sop == "!")
                        {
                            var elem2 = GetById(div.id);
                            if (elem2 != null) elem2.next.RemoveBefore();
                        }
                    }
                    else
                    {
                        if (div.id != null)
                        {
                            var elem2 = GetById(div.id);
                            emap[div.id] = div;
                            if (elem2 != null)
                            {
                                div.scroll.X = elem2.scroll.X;
                                div.scroll.Y = elem2.scroll.Y;
                                if (owner != this.childend) div.next.RemoveBefore();
                                if (div.position == Position.Absolute || div.position == Position.Fixed)
                                {
                                    elem2.next.RemoveBefore();
                                    this.childend.Before(div);
                                }
                                else
                                {
                                    elem2.Next(div);
                                    div.RemoveBefore();
                                }
                            }
                            else if (div.position == Position.Absolute || div.position == Position.Fixed)
                            {
                                div.next.RemoveBefore();
                                this.childend.Before(div);
                            }
                            else if (owner == this.childend) this.childend.Before(elem);
                        }
                        else if (owner == this.childend) this.childend.Before(elem);
                    }
                }
                else
                {
                    if (owner == this.childend){ this.childend.Before(elem);}
                }
            }
        }
    }
    enum ExeType
    {
        Feel, Result, Reason
    }
    enum ObjType
    {
        None, Optional, ClassObj, Function, Value, Class, Block, Word, Var, Type, Number, Variable, VoiVal, If, CallBlock, Elif, Else, Return, Goto, Str, StrObj, While, For, Break, Continue, BoolVal, Switch, ArrayVal, Property, Null, Array, NoFound, BlockInfo, Constructor, Base,
        Line, Operator, SingleOp, Primary, PrimOp, Bracket, Call1, Call2, ArrayType, FuncType, Generic, Left, Dot, GenericObj, VariClass, Print, Iterator, Model, ModelObj, Gene, GeneObj, Right, GenericFunction, GeneLabel, GeneNew, GeneStore, GeneSort, FloatVal, GeneValue,
        GeneChild, GeneMutate, GeneCross, GeneLabelValue,
        CrossBlock,
        MigrateBlock,
        GeneSelect,
        IdType,
        ModelValue,
        IdValue,
        ResetType,
        ModelStore,
        Stock,
        Connect,
        Address,
        Tag,
        Comment,
        AddressType,
        TagBlock,
        Cdec,
        CFunc,
        CType,
        ElemObj,
        ElemType,
        Div,
        Br,
        Htm,
        Dolor,
        Signal,
        ServerClient,
        ServerFunction,
        SingnalFunction,
        KeyEventObj,
        MouseEventObj,
        Server,
        Client,
        Comment2,
        Clones,
        Sheet,
        Cell,
        Mountain,
        AndBlock,
        Sum,
        Question,
        LineraFunction,
        AnimationFunction,
        Transition,
        Span,
        Wait,
        Error,
        NG,
        IfBlock,
        Any,
        TypeWord,
        StockType,
        CommentType
    }
    enum Accesor
    {
        Private, Public, Internal, Single
    }
    class Kaigyou : Letter
    {
        public Kaigyou()
        {
            single = true;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            if (local.comlet != null && type == LetterType.End)
            {
                local.comlet = null;
            }
            else if (local.comlet != null)
            {
                return;
            }
            else if (select)
            {
                var sel2 = local.selects[(local.seln + 1) % 2];
                if (sel2.state.elements.Last() == this) select = false;
            }
            else
            {
                if (local.selects[0].state.elements.Last() == this && local.selects[1].state.elements.Last() == this)
                {
                    g.g.FillRectangle(Brushes.LightGray, new RectangleF(pos2.X, pos2.Y, 1, size2.Y));
                }
                else if (local.selects[0].state.elements.Last() == this || local.selects[1].state.elements.Last() == this)
                {
                    g.g.FillRectangle(Brushes.LightGray, new RectangleF(pos2.X, pos2.Y, 1, size2.Y));
                    select = true;
                }
                g.px += 1;
            }
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (m.panel.switchdraw)
            {
                var line = m.state.elements[m.state.elements.Count - 1];
            head:
                line = line.next;
                if (line.type == LetterType.ElemEnd) m.panel.switchdraw = false;
                else
                {
                    if (line is VirtualLine) goto head;
                    line.update = true;
                }
            }
            pos2 = new PointF(m.x + m.px, m.y + m.py);
            m.px += 1;
            if (m.h < 16) m.h = 16;
            size2.Y = m.h;
            if (local.comlet != null && type == LetterType.End)
            {
                local.comlet = null;
                return this;
            }
            else if (local.comlet != null)
            {
                return this;
            }
            else
            {
                return this;
            }
        }
        public override void SelectExe(SelectE e, Local local, ref bool select)
        {
            if (local.comlet != null && type == LetterType.End)
            {
                local.comlet = null;
            }
            else if (local.comlet != null)
            {
            }
            else
            {
                if (local.seln == 2) { }
                else if (select)
                {
                    var sel2 = local.selects[(local.seln + 1) % 2];
                    if (sel2.state.elements[sel2.state.n] == this)
                    {
                        e.Select(e, this, null, sel2);
                        local.seln = 2;
                        select = false;
                    }
                    else
                    {
                        e.Select(e, this, null, null);
                    }
                }
                else
                {
                    for (var i = 0; i < 2; i++)
                    {
                        if (local.selects[i].state.elements[local.selects[i].state.n] == this)
                        {
                            local.seln = i;
                            var seln = i;
                            if (local.selects[(i + 1) % 2].state.elements[local.selects[(i + 1) % 2].state.n] == this)
                            {
                                e.Select(e, this, local.selects[i], local.selects[(i + 1) % 2]);
                                e.state.elements[e.state.elements.Count - 1] = next;
                                local.seln = 2;
                                return;
                            }
                            else
                            {
                                e.Select(e, this, local.selects[i], null);
                                e.state.elements[e.state.elements.Count - 1] = next;
                                select = true;
                                return;
                            }
                        }
                    }
                }
            }
            e.state.elements[e.state.elements.Count - 1] = next;
            return;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (local.comlet != null && type == LetterType.End)
            {
                local.comlet = null;
            }
            else if (local.comlet != null)
            {
            }
            else
            {
                if (local.seln == 2) { }
                else if (select)
                {
                    var sel2 = local.selects[(local.seln + 1) % 2];
                    if (sel2.state.elements[sel2.state.n] == this)
                    {
                        local.seln = 2;
                        select = false;
                        var state = e.state.Clone();
                        state.elements[state.elements.Count - 2] = this.parent;
                        state.elements[state.elements.Count - 1] = this;
                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                    }
                    else
                    {
                        var line = e.state.elements[e.state.elements.Count - 2];
                    head:
                        var line2 = line.next;
                        if (line2.type == LetterType.ElemEnd)
                        {
                            e.state.elements[e.state.elements.Count - 1] = e.state.elements.Last().next;
                            return 0;
                        }
                        else if (line2 is VirtualLine) goto head;
                        next.RemoveBefore();
                        var start = line2.childend.next;
                        line.AddRange(line2.childend.next);
                        line2.next.RemoveBefore();
                        e.state.elements[e.state.elements.Count - 1] = start;
                        return 0;
                    }
                }
                else
                {
                    for (var i = 0; i < 2; i++)
                    {
                        if (local.selects[i].state.elements[local.selects[i].state.n] == this)
                        {
                            local.seln = i;
                            var seln = i;
                            if (key != null) key(e, local);
                            if (local.selects[(i + 1) % 2].state.elements[local.selects[(i + 1) % 2].state.n] == this)
                            {
                                local.seln = 2;
                                switch (e.key)
                                {
                                    case Keys.Enter:
                                        local.panel.input = true;
                                        var line = new Line();
                                        var kaigyou = new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou };
                                        var line2 = e.state.elements[e.state.elements.Count - 2];
                                        var kaigyou2 = line2.childend.before;
                                        line2.childend.RemoveBefore();
                                        line.childend.Next(kaigyou2);
                                        e.state.Update();
                                        var state = e.state.Clone();
                                        state.elements[state.elements.Count - 2] = line;
                                        state.elements[state.elements.Count - 1] = kaigyou2;
                                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                        line2.childend.Before(kaigyou);
                                        line2.Next(line);
                                        line.childstart = line.childend.next;
                                        (line2 as Line).childstart = line2.childend.next;
                                        e.state.elements[e.state.elements.Count - 1] = line2.childend;
                                        return 1;
                                    case Keys.Back:
                                        local.panel.input = true;
                                        var line3 = e.state.elements[e.state.elements.Count - 2];
                                        if (line3.childend.next == this)
                                        {
                                            var line5 = e.state.elements[e.state.elements.Count - 2];
                                        head2:
                                            line5 = line5.before;
                                            if (line5.type == LetterType.ElemEnd)
                                            {
                                                e.state.elements[e.state.elements.Count - 1] = next;
                                                return 0;
                                            }
                                            else if (line5 is VirtualLine) goto head2;
                                            else
                                            {
                                                var start = line3.childend.next;
                                                line5.childend.RemoveBefore();
                                                line3.FirstRange(line5.childend.before);
                                                var state2 = e.state.Clone();
                                                state2.elements[state2.elements.Count - 1] = this;
                                                local.selects[0] = local.selects[1] = new Select() { state = state2, n = 0 };
                                                line5.next.RemoveBefore();
                                                e.state.Update();
                                                e.state.elements[e.state.elements.Count - 1] = next;
                                                (line3 as Line).childstart = childend;
                                                return 0;
                                            }

                                        }
                                        else
                                        {
                                            if (before is Letter)
                                            {
                                                var letter = before as Letter;
                                                letter.text = letter.text.Substring(0, letter.text.Length - 1);
                                                var state2 = e.state.Clone();
                                                state2.elements[state2.elements.Count - 1] = this;
                                                local.selects[0] = local.selects[1] = new Select() { state = state2, n = 0 };
                                                e.state.Update();
                                                e.state.elements[e.state.elements.Count - 1] = next;
                                                break;
                                            }
                                            else throw new Exception();
                                        }
                                    case Keys.Delete:
                                        local.panel.input = true;
                                        var line4 = e.state.elements[e.state.elements.Count - 2];
                                        var line7 = line4;
                                    head:
                                        line7 = line7.next;
                                        if (line7.type == LetterType.ElemEnd)
                                        {
                                            e.state.elements[e.state.elements.Count - 1] = next;
                                            return 0;
                                        }
                                        else if (line7 is VirtualLine) goto head;
                                        this.next.RemoveBefore();
                                        var start2 = line7.childend.next;
                                        line4.AddRange(line7.childend.next);
                                        line7.next.RemoveBefore();
                                        e.state.Update();
                                        var state3 = e.state.Clone();
                                        state3.elements[state3.elements.Count - 1] = start2;
                                        local.selects[0] = local.selects[1] = new Select() { state = state3, n = 0 };
                                        e.state.elements[e.state.elements.Count - 1] = start2;
                                        return 0;
                                    case Keys.Left:
                                        if (local.selects[seln].n == 0)
                                        {
                                            var elem = local.selects[seln].state.elements.Last();
                                            elem = elem.before;
                                            var line00 = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] as Line;
                                            Element body3 = null;
                                            if (local.selects[seln].state.elements.Count >= 3) body3 = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                            if (body3 != null && elem.type == LetterType.ElemEnd && body3.type == LetterType.CloneElement)
                                            {
                                                var clone = body3 as CloneElement;
                                                if (line00 == clone.childstart)
                                                {
                                                    var elem01 = body3;
                                                    elem01 = elem01.before;
                                                    if (elem01.type == LetterType.Line || elem01.type == LetterType.VirtualLine)
                                                    {
                                                        local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = elem01;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem01.childend.before;
                                                        local.selects[seln].n = Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else if (elem01.type == LetterType.CloneElement)
                                                    {
                                                        clone = elem01 as CloneElement;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childend;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = clone.childend.childend.before;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else { return 1; }
                                                }
                                            }
                                            var line06 = line00.before as Line;
                                            if (elem.type == LetterType.ElemEnd || (line00 != null && elem == line00.childend))
                                            {
                                                if (line06 == null)
                                                {
                                                    if (line00.before.type == LetterType.CloneElement)
                                                    {
                                                        var clone = line00.before as CloneElement;
                                                        var element = clone.childend.childend.before;
                                                        local.selects[seln].state.elements.Add(element);
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childend;
                                                        local.selects[seln].n = element.Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    return 1;
                                                }
                                                else
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line06;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line06.childend.before;
                                                    local.selects[seln].n = line06.childend.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                }
                                            }
                                            else
                                            {
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                local.selects[seln].n = elem.Count() - 1;
                                                local.selects[(seln + 1) % 2] = local.selects[seln];
                                            }
                                        }
                                        else
                                        {
                                            local.selects[0].n--;
                                            local.selects[1].n = local.selects[0].n;
                                        }
                                        break;
                                    case Keys.Right:
                                        if (type == LetterType.End) return 1;
                                        var line01 = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] as Line;
                                        Element body2 = null;
                                        if (local.selects[seln].state.elements.Count >= 3) body2 = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                        if (body2 != null && body2.type == LetterType.CloneElement)
                                        {
                                            var clone = body2 as CloneElement;
                                            if (line01 == clone.childend)
                                            {
                                                var elem01 = body2;
                                                elem01 = elem01.next;
                                                if (elem01.type == LetterType.Line || elem01.type == LetterType.VirtualLine)
                                                {
                                                    local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = elem01;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = (elem01 as Line).childstart;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else if (elem01.type == LetterType.CloneElement)
                                                {
                                                    clone = elem01 as CloneElement;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childstart;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = (clone.childstart as Line).childstart;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else { return 1; }
                                            }
                                        }
                                        var line07 = line01.next as Line;
                                        if (line07 == null)
                                        {
                                            if (line01.next.type == LetterType.CloneElement)
                                            {
                                                var clone = line01.next as CloneElement;
                                                var element = (clone.childstart as Line).childstart;
                                                local.selects[seln].state.elements.Add(element);
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childstart;
                                                local.selects[seln].n = 0;
                                                local.selects[(seln + 1) % 2] = local.selects[seln];
                                                return 1;
                                            }
                                            return 1;
                                        }
                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line07;
                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line07.childstart;
                                        local.selects[seln].n = 0;
                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                        break;
                                    case Keys.Up:
                                        var line02 = local.selects[seln].state.elements.Last().parent as Line;
                                        var line03 = line02.before as Line;
                                        Element body = null;
                                        if (local.selects[seln].state.elements.Count >= 3) body = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                        var n2 = 0;
                                        if (local.countn == -1)
                                        {
                                            for (var elem = line02.childstart; ; elem = elem.next)
                                            {
                                                if (elem == local.selects[seln].state.elements.Last())
                                                {
                                                    n2 += local.selects[seln].n;
                                                    break;
                                                }
                                                n2 += elem.Count();
                                            }
                                            local.countn = n2;
                                        }
                                        else n2 = local.countn;
                                        if (body != null && body.type == LetterType.CloneElement)
                                        {
                                            var clone = body as CloneElement;
                                            if (line02 == clone.childstart)
                                            {
                                                var elem2 = clone.before;
                                                if (elem2.type == LetterType.Line || elem2.type == LetterType.VirtualLine)
                                                {
                                                    line03 = elem2 as Line;
                                                    local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line03;
                                                    for (var elem = line03.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n2 - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line03.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;

                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n2;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n2 = n3;
                                                    }
                                                }
                                                else if (elem2.type == LetterType.CloneElement)
                                                {
                                                    clone = elem2 as CloneElement;
                                                    line2 = clone.childend as Line;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    for (var elem = line03.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n2 - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line2.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;

                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n2;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n2 = n3;
                                                    }
                                                }
                                                else { return 1; }
                                            }
                                        }
                                        if (line03 == null)
                                        {
                                            var clone = line02.before as CloneElement;
                                            local.selects[seln].state.elements.Add(null);
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childend;
                                            line2 = clone.childend as Line;
                                            for (var elem = line03.childstart; ; elem = elem.next)
                                            {
                                                var n3 = n2 - elem.Count();
                                                if (elem.type == LetterType.Kaigyou)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else if (elem == line2.childend)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                    local.selects[seln].n = elem.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;

                                                }
                                                if (n3 < 0)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = n2;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                n2 = n3;
                                            }
                                        }
                                        else
                                        {
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line03;
                                            for (var elem = line03.childstart; ; elem = elem.next)
                                            {
                                                var n3 = n2 - elem.Count();
                                                if (elem.type == LetterType.Kaigyou)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else if (elem == line03.childend)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                    local.selects[seln].n = elem.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;

                                                }
                                                if (n3 < 0)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = n2;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                n2 = n3;
                                            }
                                        }
                                        break;
                                    case Keys.Down:
                                        var line04 = local.selects[seln].state.elements.Last().parent as Line;
                                        var line05 = line04.next as Line;
                                        var n = 0;
                                        if (local.countn == -1)
                                        {
                                            for (var elem = line04.childstart; ; elem = elem.next)
                                            {
                                                if (elem == local.selects[seln].state.elements.Last())
                                                {
                                                    n += local.selects[seln].n;
                                                    break;
                                                }
                                                n += elem.Count();
                                            }
                                            local.countn = n;
                                        }
                                        else n = local.countn;
                                        Element body4 = null;
                                        if (local.selects[seln].state.elements.Count >= 3) body4 = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                        if (body4 != null && body4.type == LetterType.CloneElement)
                                        {
                                            var clone = body4 as CloneElement;
                                            if (line04 == clone.childend)
                                            {
                                                var elem2 = clone.next;
                                                if (elem2.type == LetterType.Line || elem2.type == LetterType.VirtualLine)
                                                {
                                                    line05 = elem2 as Line;
                                                    local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line05;
                                                    for (var elem = line05.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line05.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n = n3;
                                                    }
                                                }
                                                else if (elem2.type == LetterType.CloneElement)
                                                {
                                                    clone = elem2 as CloneElement;
                                                    line2 = clone.childstart as Line;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    for (var elem = line05.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line2.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n = n3;
                                                    }
                                                }
                                            }
                                        }
                                        if (line05 == null)
                                        {
                                            if (line04.next.type == LetterType.CloneElement)
                                            {
                                                var clone = line04.next as CloneElement;
                                                local.selects[seln].state.elements.Add(null);
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childstart;
                                                line2 = clone.childstart as Line;
                                                for (var elem = line05.childstart; ; elem = elem.next)
                                                {
                                                    var n3 = n - elem.Count();
                                                    if (elem.type == LetterType.Kaigyou)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else if (elem == line2.childend)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                        local.selects[seln].n = elem.before.Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;

                                                    }
                                                    if (n3 < 0)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                        local.selects[seln].n = n;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    n = n3;
                                                }
                                            }
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line04.childend.before;
                                            local.selects[seln].n = line04.childend.before.Count() - 1;
                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                        }
                                        else
                                        {
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line05;
                                            for (var elem = line05.childstart; ; elem = elem.next)
                                            {
                                                var n3 = n - elem.Count();
                                                if (elem.type == LetterType.Kaigyou)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else if (elem == line05.childend)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                    local.selects[seln].n = elem.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;

                                                }
                                                if (n3 < 0)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = n;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                n = n3;
                                            }
                                        }
                                        break;
                                    case Keys.None:
                                        local.panel.input = true;
                                        var let = new Letter() { text = e.text };
                                        Before(let);
                                        if ((parent as Line).childstart == this) (parent as Line).childstart = let;
                                        var state4 = e.state.Clone();
                                        state4.elements[state4.elements.Count - 1] = this;
                                        local.selects[0] = local.selects[1] = new Select() { state = state4, n = 0 };
                                        e.state.elements[e.state.elements.Count - 1] = next;
                                        e.state.Update();
                                        return 1;
                                }
                                return 1;
                            }
                            else
                            {
                                switch (e.key)
                                {
                                    case Keys.Left:
                                    case Keys.Up:
                                        local.seln = 2;
                                        local.selects[(local.seln + 1) % 2] = local.selects[local.seln];
                                        return 1;
                                    case Keys.Right:
                                    case Keys.Down:
                                        local.seln = 2;
                                        local.selects[local.seln] = local.selects[(local.seln + 1) % 2];
                                        return 1;
                                }
                                local.panel.input = true;
                                var line = e.state.elements[e.state.elements.Count - 2];
                                var line2 = line;
                            head:
                                line2 = line2.next;
                                if (line2.type == LetterType.ElemEnd)
                                {
                                    e.state.elements[e.state.elements.Count - 1] = next;
                                    return 0;
                                }
                                else if (line2 is VirtualLine) goto head;
                                this.next.RemoveBefore();
                                line.AddRange(line2.childend.next);
                                line2.next.RemoveBefore();
                                e.state.Update();
                                select = true;
                                return 0;
                            }
                        }
                    }
                }
            }
            e.state.elements[e.state.elements.Count - 1] = next;
            return 0;
        }
        public override int plus(int n)
        {
            return n - 1;
        }
        public override void nextplus(State state)
        {
            state.elements[state.elements.Count - 1] = state.elements.Last().next;
            if (state.elements.Last().type == LetterType.ElemEnd) state.elements.RemoveAt(state.elements.Count - 1);
        }
        public override int Count()
        {
            return 1;
        }
        public override string Text2(Local local)
        {
            if (type == LetterType.Kaigyou) return "\\n";
            else return "";
        }
        public override string Text(Local local)
        {
            var str = "";
            if (local.selects[0].state.elements.Last() == this)
            {
                str += (char)('\uE000' + local.selects[0].n * 2);
            }
            if (local.selects[1].state.elements.Last() == this)
            {
                str += (char)('\uE000' + local.selects[1].n * 2 + 1);
            }
            if (type == LetterType.End) return str;
            return str + text;
        }
    }
    partial class CommentLet : Letter
    {
        bool switched = false;
        public PointF pos3, size3;
        public Comment comment;
        public List<Element> elems = new List<Element>();
        public List<int> nums = new List<int>();
        public Letter ValueAdd(String name)
        {
            var letter = new Letter() { text = "", name = "", type = LetterType.Htm };
            Add(letter);
            comment.vmap[name] = letter;
            return letter;
        }
        public CommentLet() : base()
        {
            single = true;
        }
        public List<List<Element>> instanceslist = new List<List<Element>>();
        public List<Element> instances
        {
            get { return instanceslist.Last(); }
        }
        public void Renew()
        {
            instanceslist = new List<List<Element>>();
            childend.next = childend.before = childend;
            childend.Next(new Div());
            elems = new List<Element>();
            nums = new List<int>();
        }
        public void Start()
        {
            instanceslist.Add(new List<Element>());
        }
        public void Add(Element elem)
        {
            instances.Add(elem);
        }
        public void AddNext(Element elem)
        {
            instances.Add(elem);
            elems.Add(elem);
            nums.Add(0);
        }
        public async void Single(SingleOp op, Local local)
        {
            if (op == null)
            {
                foreach(var elem in instances) elems[elems.Count - 1].add(elem);
            }
            else if (instances.Count > 0)
            {
                var line = new Line();
                if (op.name == ".")
                {
                    line.add(new Letter() { text = "･", name = "･", type = LetterType.Htm });
                    nums[nums.Count - 1] = 0;
                }
                else if (op.name == "*")
                {
                    nums[nums.Count - 1]++;
                    line.add(new Letter() { text = nums.Last() + ".", name = nums.Last() + ".", type = LetterType.Htm });
                }
                else if (op.name == ">>")
                {
                    op.letter.text = "--";
                    op.letter.type = LetterType.CommentSingle;
                    op.letter.parent.update = true;
                    op.letter.parent.recompile = true;
                    var text = "";
                    foreach(var elem in instances)
                    {
                        text += elem.Text;
                    }
                    await local.panel.form.OPI(op.letter.parent as Line, op.letter, text, local);
                    instanceslist.RemoveAt(instanceslist.Count - 1);
                    nums[nums.Count - 1] = 0;
                    return;
                }
                else nums[nums.Count - 1] = 0;
                foreach (var elem in instances) line.add(elem);
                elems[elems.Count - 1].add(line);
            }
            instanceslist.RemoveAt(instanceslist.Count - 1);
        }
        public void Back()
        {
            elems.Last().add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
            elems.RemoveAt(elems.Count - 1);
            nums.RemoveAt(nums.Count - 1);
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            if (local.comlet != null && type == LetterType.NyoroNyoro)
            {
                local.comlet = null;
            }
            else
            {
                if (select)
                {
                    if (local.selects[(local.seln + 1) % 2].state.elements.Last() == this)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px, g.y + g.py, 7.5f * local.selects[(local.seln + 1) % 2].n + 1, size2.Y));
                        select = false;
                    }
                    else g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px, g.y + g.py, size2.X, size2.Y));
                }
                else
                {
                    for (var i = 0; i < 2; i++)
                    {
                        if (local.selects[i].state.elements.Last() == this)
                        {
                            if (local.selects[(i + 1) % 2].state.elements.Last() == this)
                            {
                                int n1 = local.selects[i].n, n2 = local.selects[(i + 1) % 2].n;
                                if (n1 > n2)
                                {
                                    var ins = n1;
                                    n1 = n2;
                                    n2 = ins;
                                }
                                g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + n1 * 7.5f, g.y + g.py, (n2 - n1) * 7.5f + 1, size2.Y));
                            }
                            else
                            {
                                int n1 = local.selects[i].n;
                                g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + n1 * 7.5f, g.y + g.py, (text.Length - n1) * 7.5f + 1, size.Y));
                                local.seln = i;
                                select = !select;
                            }
                            break;
                        }
                    }
                }
                if (font != null) g.font = font;
                g.g.DrawString(text, g.font, brush, g.x + g.px, g.y + g.py);
                g.px += size2.X;
                if (switched)
                {
                    if (comment != null)
                    {
                        childend.next.Draw(g, local, ref select);
                        local.comlet = this;
                    }
                }
            }
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (type == LetterType.NyoroNyoro && local.comlet != null)
            {
                local.comlet = null;
                if (m.panel.switchdraw) m.panel.switchdraw = false;
            }
            else
            {
                if (font != null) m.font = font;
                var s = m.g.MeasureString(text, m.font);
                if (m.xtype == SizeType.Break)
                {
                    if (m.px + s.Width > m.sizex)
                    {
                        return new VirtualLine();
                    }
                }
                else
                {
                    if (m.px > m.sizex) m.sizex = m.px;
                }
                pos2 = new PointF(m.x + m.px, m.y + m.py);
                size2.X = 7.5f * text.Length;
                size2.Y = s.Height;
                if (m.h < size2.Y) m.h = size2.Y;
                m.px += size2.X;
                size = new PointF(s.Width, s.Height);
                if (switched)
                {
                    pos3.X = m.x;
                    pos3.Y = m.py + m.h;
                    size3.X = 0;
                    size3.Y = 0;
                    if (comment != null)
                    {
                        childend.next.Measure(m, local, ref order);
                    }
                    local.comlet = this;
                }
                if (m.panel.switched == this)
                {
                    m.panel.switchdraw = true;
                    m.panel.switched = null;
                }
            }
            return null;
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (local.comlet != null)
            {
                local.comlet = null;
                return -1;
            }
            else
            if (pos2.X <= e.x && e.x < pos2.X + size2.X)
            {
                if (pos2.Y <= e.y && e.y < pos2.Y + size.Y)
                {
                    if (e.call == MouseCall.MouseDown && type == LetterType.Nyoro)
                    {
                        switched = !switched;
                        e.state.elements[e.state.elements.Count - 2].update = true;
                        e.panel.input = true;
                        e.panel.switched = this;
                    }
                    if (mouse != null) mouse(e, local.local);
                }
                return (int)((e.x - pos2.X) / 7.5f);
            }
            if (switched)
            {
                local.comlet = this;
            }
            return -1;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (local.comlet != null && type == LetterType.NyoroNyoro)
            {
                local.comlet = null;
            }
            else if (switched) {
                local.comlet = this;
            }
            else
            {
                return base.Key(e, local, ref select);
            }
            e.state.elements[e.state.elements.Count - 1] = next;
            return 0;
        }
        public override int plus(int n)
        {
            return n - 1;
        }
        public override void nextplus(State state)
        {
            state.elements[state.elements.Count - 1] = state.elements.Last().next;
            if (state.elements.Last().type == LetterType.ElemEnd) state.elements.RemoveAt(state.elements.Count - 1);
        }
    }
    class SelectLetter : Letter
    {
        public int seln;
        public int n;
        public SelectLetter(int n) : base()
        {
            seln = n % 2;
            this.n = n / 2;
            type = LetterType.Select;
            single = true;
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            Element nex = this;
        head:
            nex = nex.next;
            if (nex.type == LetterType.Select) goto head;
            local.selects[seln] = new Select() { state = m.state.Clone(), n = n };
            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = nex;
            next.RemoveBefore();
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            return;
        }
    }
    class Letter : Element
    {
        public String name;
        public String text;
        public int indent;
        public Brush brush = Brushes.Black;
        public String error = null;
        public Letter() : base()
        {
            single = true;
        }
        public override int Count()
        {
            return text.Length;
        }
        public Letter Let()
        {
            switch(name)
            {
                case "in":
                    type = LetterType.In;
                    break;
            }
            return this;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            if (local.comlet != null)
            {
                return;
            }
            if (select)
            {
                if (local.selects[(local.seln + 1) % 2].state.elements.Last() == this)
                {
                    g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px, g.y + g.py, 7.5f * local.selects[(local.seln + 1) % 2].n + 1, size2.Y));
                    select = false;
                }
                else g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px, g.y + g.py, size2.X, size2.Y));
            }
            else
            {
                for (var i = 0; i < 2; i++)
                {
                    if (local.selects[i].state.elements.Last() == this)
                    {
                        if (local.selects[(i + 1) % 2].state.elements.Last() == this)
                        {
                            int n1 = local.selects[i].n, n2 = local.selects[(i + 1) % 2].n;
                            if (n1 > n2)
                            {
                                var ins = n1;
                                n1 = n2;
                                n2 = ins;
                            }
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + n1 * 7.5f, g.y + g.py, (n2 - n1) * 7.5f + 1, size2.Y));
                        }
                        else
                        {
                            int n1 = local.selects[i].n;
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + n1 * 7.5f, g.y + g.py, (text.Length - n1) * 7.5f + 1, size.Y ));
                            local.seln = i;
                            select = !select;
                        }
                        break;
                    }
                }
            }
            if (font != null) g.font = font;
            g.g.DrawString(text, g.font, brush, g.x + g.px, g.y + g.py);
            if (error != null) g.g.DrawLine(new Pen(Brushes.Red, 1), new PointF(g.x + g.px, g.y + g.py + g.h), new PointF(g.x + g.px + size2.X, g.y + g.py + g.h));
            g.px += size2.X;
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (local.comlet != null)
            {
                return null;
            }
            if (font != null) m.font = font;
            var s = m.g.MeasureString(text, m.font);
            if (m.xtype == SizeType.Break)
            {
                if (m.px + s.Width > m.sizex)
                {
                    return new VirtualLine();
                }
            }
            else
            {
                if (m.px > m.sizex) m.sizex = m.px;
            }
            pos2 = new PointF(m.x + m.px, m.y + m.py);
            size2.X = 7.5f * text.Length;
            size2.Y = s.Height;
            if (m.h < size2.Y) m.h = size2.Y;
            m.px += size2.X;
            size = new PointF(s.Width, s.Height);
            return null;
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (local.comlet != null)
            {
                return -1;
            }
            if (pos2.X <= e.x && e.x < pos2.X + size2.X)
            {
                if (pos2.Y <= e.y && e.y < pos2.Y + size.Y)
                {
                    if (mouse != null) mouse(e, local.local);
                }
                return (int)((e.x - pos2.X) / 7.5f);
            }
            return -1;
        }
        public override void SelectExe(SelectE e, Local local, ref bool select)
        {
            if (local.comlet != null && type == LetterType.End)
            {
                local.comlet = null;
            }
            else if (local.comlet != null)
            {
            }
            else
            {
                if (local.seln == 2) { }
                else if (select)
                {
                    var sel2 = local.selects[(local.seln + 1) % 2];
                    if (sel2.state.elements[sel2.state.n] == this)
                    {
                        select = false;
                        local.seln = 2;
                        e.Select(e, this, null, sel2);
                    }
                    else
                    {
                        e.Select(e, this, null, null);
                    }
                }
                else
                {
                    for (var i = 0; i < 2; i++)
                    {
                        if (local.selects[i].state.elements[local.selects[i].state.n] == this)
                        {
                            local.seln = i;
                            if (local.selects[(i + 1) % 2].state.elements[local.selects[(i + 1) % 2].state.n] == this)
                            {
                                local.seln = 2;
                                var seln = i;
                                int n1 = local.selects[i].n, n2 = local.selects[(i + 1) % 2].n;
                                if (n1 > n2)
                                {
                                    var ins = n1;
                                    n1 = n2;
                                    n2 = ins;
                                    seln = (i + 1) % 2;
                                }
                                e.Select(e, this, local.selects[seln], local.selects[(seln + 1) % 2]);
                                e.state.elements[e.state.elements.Count - 1] = next;
                                return;
                            }
                            else
                            {
                                e.Select(e, this, local.selects[i], null);
                                e.state.elements[e.state.elements.Count - 1] = next;
                                select = true;
                                return;
                            }
                        }
                    }
                }
            }
            e.state.elements[e.state.elements.Count - 1] = next;
            return;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (local.comlet != null)
            {
            }
            else if (local.seln == 2) { }
            else if (select)
            {
                var sel2 = local.selects[(local.seln + 1) % 2];
                if (sel2.state.elements[sel2.state.n] == this)
                {
                    local.seln = 2;
                    var state = e.state.Clone();
                    state.elements[state.elements.Count - 1] = this;
                    local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                    text = text.Substring(sel2.n);
                    select = false;
                    e.state.elements[e.state.elements.Count - 1] = next;
                    (parent as Line).childstart = parent.childend.next;
                    return 0;
                }
                else
                {
                    this.next.RemoveBefore();
                    e.state.elements[e.state.elements.Count - 1] = next;
                    return 1;
                }
            }
            else {
                for (var i = 0; i < 2; i++)
                {
                    if (local.selects[i].state.elements[local.selects[i].state.n] == this)
                    {
                        local.seln = i;
                        if (key != null) key(e, local);
                        if (local.selects[(i + 1) % 2].state.elements[local.selects[(i + 1) % 2].state.n] == this)
                        {
                            local.seln = 2;
                            var seln = i;
                            int n1 = local.selects[i].n, n2 = local.selects[(i + 1) % 2].n;
                            if (n1 > n2)
                            {
                                var ins = n1;
                                n1 = n2;
                                n2 = ins;
                                seln = (i + 1) % 2;
                            }
                            switch (e.key)
                            {
                                case Keys.Enter:
                                    local.panel.input = true;
                                    var line3 = new Line();
                                    var line4 = e.state.elements[e.state.elements.Count - 2];
                                    line3.AddRange(this.next);
                                    if (this is Span)
                                    {
                                        var span = this as Span;
                                        line3.childend.Next(new Span() { text = text.Substring(n2, text.Length - n2), brush = span.brush, statuses = span.statuses });
                                    }
                                    else line3.childend.Next(new Letter() { text = text.Substring(n2, text.Length - n2) });
                                    text = text.Substring(0, n1);
                                    this.next = line4.childend;
                                    line4.childend.before = this;
                                    line4.childend.Before(new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou });
                                    var state = e.state.Clone();
                                    state.elements[state.elements.Count - 2] = line3;
                                    state.elements[state.elements.Count - 1] = line3.childend.next;
                                    local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                    line4.Next(line3);
                                    line3.childstart = line3.childend.next;
                                    line3.recompile = true;
                                    break;
                                case Keys.Back:
                                    local.panel.input = true;
                                    if (n1 == n2)
                                    {
                                        if (n1 == 0)
                                        {
                                            var line = e.state.elements[e.state.elements.Count - 2];
                                            if (this == line.childend.next)
                                            {
                                                var line2 = line;
                                            head:
                                                line2 = line2.before;
                                                if (line2.type == LetterType.ElemEnd)
                                                {
                                                    e.state.elements[e.state.elements.Count - 1] = next;
                                                    return 0;
                                                }
                                                else if (line2 is VirtualLine) goto head;
                                                else
                                                {
                                                    line2.childend.RemoveBefore();
                                                    line.FirstRange(line2.childend.before);
                                                    var state2 = e.state.Clone();
                                                    state2.elements[state2.elements.Count - 1] = this;
                                                    local.selects[0] = local.selects[1] = new Select() { state = state2, n = 0 };
                                                    e.state.Update();
                                                    line2.next.RemoveBefore();
                                                    e.state.elements[e.state.elements.Count - 1] = next;
                                                    (line as Line).childstart = line.childend.next;
                                                    return 0;
                                                }

                                            }
                                            else
                                            {
                                                if (before is Letter)
                                                {
                                                    var letter = before as Letter;
                                                    letter.text = letter.text.Substring(0, letter.text.Length - 1);
                                                    break;
                                                }
                                                else throw new Exception();
                                            }
                                        }
                                        else
                                        {
                                            text = text.Substring(0, n1 - 1) + text.Substring(n1, text.Length - n1);
                                            var state2 = e.state.Clone();
                                            state2.elements[state2.elements.Count - 1] = this;
                                            local.selects[0] = local.selects[1] = new Select() { state = state2, n = n1 - 1 };
                                            break;
                                        }
                                    }
                                    else goto case Keys.None;
                                case Keys.Delete:
                                    local.panel.input = true;
                                    if (n1 == n2)
                                    {
                                        text = text.Substring(0, n1) + text.Substring(n1 + 1, text.Length - n1 - 1);
                                        break;
                                    }
                                    else goto case Keys.None;
                                case Keys.Left:
                                    if (n1 == n2)
                                    {
                                        if (n1 == 0)
                                        {
                                            var elem = local.selects[seln].state.elements.Last();
                                            elem = elem.before;
                                            var line = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] as Line;

                                            Element body = null;
                                            if (local.selects[seln].state.elements.Count >= 3) body = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                            if (body != null && elem.type == LetterType.ElemEnd && body.type == LetterType.CloneElement)
                                            {
                                                var clone = body as CloneElement;
                                                if (line == clone.childstart)
                                                {
                                                    var elem01 = body;
                                                head:
                                                    elem01 = elem01.before;
                                                    if (elem01.type == LetterType.Line || elem01.type == LetterType.VirtualLine)
                                                    {
                                                        local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = elem01;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem01.childend.before;
                                                        local.selects[seln].n = Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else if (elem01.type == LetterType.CloneElement)
                                                    {
                                                        clone = elem01 as CloneElement;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childend;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = clone.childend.childend.before;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else { return 1; }
                                                }
                                            }
                                            var line2 = line.before as Line;
                                            if (elem.type == LetterType.ElemEnd || (line2 != null && elem == line2.childend))
                                            {
                                                if (line2 == null)
                                                {
                                                    if (line.before.type == LetterType.CloneElement)
                                                    {
                                                        var clone = line.before as CloneElement;
                                                        var element = clone.childend.childend.before;
                                                        local.selects[seln].state.elements.Add(element);
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childend;
                                                        local.selects[seln].n = element.Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    return 1;
                                                }
                                                else
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line2.childend.before;
                                                    local.selects[seln].n = line2.childend.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                }
                                            }
                                            else
                                            {
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                local.selects[seln].n = elem.Count() - 1;
                                                local.selects[(seln + 1) % 2] = local.selects[seln];
                                            }
                                        }
                                        else
                                        {
                                            local.selects[0].n--;
                                            local.selects[1].n = local.selects[0].n;
                                        }
                                    }
                                    else
                                    {
                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                    }
                                    break;
                                case Keys.Right:
                                    if (n1 == n2)
                                    {
                                        if (n1 == text.Length)
                                        {
                                            var elem = local.selects[seln].state.elements.Last();
                                            elem = elem.next;
                                            var line = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] as Line;

                                            Element body = null;
                                            if (local.selects[seln].state.elements.Count >= 3) body = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                            if (body != null && (elem.type == LetterType.ElemEnd || elem.type == LetterType.Kaigyou || elem.type == LetterType.End) && body.type == LetterType.CloneElement)
                                            {
                                                var clone = body as CloneElement;
                                                if (line == clone.childend)
                                                {
                                                    var elem01 = body;
                                                head:
                                                    elem01 = elem01.next;
                                                    if (elem01.type == LetterType.Line || elem01.type == LetterType.VirtualLine)
                                                    {
                                                        local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = elem01;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = (elem01 as Line).childstart;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else if (elem01.type == LetterType.CloneElement)
                                                    {
                                                        clone = elem01 as CloneElement;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childstart;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = (clone.childstart as Line).childstart;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else { return 1; }
                                                }
                                            }
                                            var line2 = line.next as Line;
                                            if (elem.type == LetterType.Kaigyou || elem.type == LetterType.ElemEnd || elem == line.childstart)
                                            {
                                                if (line2 == null)
                                                {
                                                    if (line.next.type == LetterType.CloneElement)
                                                    {
                                                        var clone = line.next as CloneElement;
                                                        var element = (clone.childstart as Line).childstart;
                                                        local.selects[seln].state.elements.Add(element);
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childstart;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    return 1;
                                                }
                                                else
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line.childstart;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];

                                                }
                                            }
                                            else if (elem.type == LetterType.End)
                                            {
                                                return 1;
                                            }
                                            else
                                            {
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                local.selects[seln].n = 1;
                                                local.selects[(seln + 1) % 2] = local.selects[seln];
                                            }
                                        }
                                        else
                                        {
                                            local.selects[0].n++;
                                            local.selects[1].n = local.selects[0].n;
                                        }
                                    }
                                    else
                                    {
                                        local.selects[seln] = local.selects[(seln + 1) % 2];
                                    }
                                    break;
                                case Keys.Up:
                                    if (n1 == n2)
                                    {
                                        var line = local.selects[seln].state.elements.Last().parent as Line;
                                        var line2 = line.before as Line;
                                        Element body = null;
                                        if (local.selects[seln].state.elements.Count >= 3) body = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                        var n = 0;
                                        if (local.countn == -1)
                                        {
                                            for (var elem = line.childstart; ; elem = elem.next)
                                            {
                                                if (elem == local.selects[seln].state.elements.Last())
                                                {
                                                    n += local.selects[seln].n;
                                                    break;
                                                }
                                                n += elem.Count();
                                            }
                                            local.countn = n;
                                        }
                                        else n = local.countn;
                                        if (body != null && body.type == LetterType.CloneElement)
                                        {
                                            var clone = body as CloneElement;
                                            if (line == clone.childstart)
                                            {
                                                var elem2 = clone.before;
                                                if (elem2.type == LetterType.Line || elem2.type == LetterType.VirtualLine)
                                                {
                                                    line2 = elem2 as Line;
                                                    local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    for (var elem = line2.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line2.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;

                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n = n3;
                                                    }
                                                }
                                                else if (elem2.type == LetterType.CloneElement)
                                                {
                                                    clone = elem2 as CloneElement;
                                                    line2 = clone.childend as Line;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    for (var elem = line2.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line2.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;

                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n = n3;
                                                    }
                                                }
                                                else { return 1; }
                                            }
                                        }
                                        if (line2 == null)
                                        {
                                            if (line.before.type == LetterType.CloneElement)
                                            {
                                                var clone = line.before as CloneElement;
                                                local.selects[seln].state.elements.Add(null);
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childend;
                                                line2 = clone.childend as Line;
                                                for (var elem = line2.childstart; ; elem = elem.next)
                                                {
                                                    var n3 = n - elem.Count();
                                                    if (elem.type == LetterType.Kaigyou)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else if (elem == line2.childend)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                        local.selects[seln].n = elem.before.Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;

                                                    }
                                                    if (n3 < 0)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                        local.selects[seln].n = n;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    n = n3;
                                                }
                                            }
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line.childstart;
                                            local.selects[seln].n = 0;
                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                        }
                                        else
                                        {
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                            for (var elem = line2.childstart; ; elem = elem.next)
                                            {
                                                var n3 = n - elem.Count();
                                                if (elem.type == LetterType.Kaigyou)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else if (elem == line2.childend)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                    local.selects[seln].n = elem.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;

                                                }
                                                if (n3 < 0)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = n;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                n = n3;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                    }
                                    break;
                                case Keys.Down:
                                    if (n1 == n2)
                                    {
                                        var line = local.selects[seln].state.elements.Last().parent as Line;
                                        var line2 = line.next as Line;
                                        var n = 0;
                                        if (local.countn == -1)
                                        {
                                            for (var elem = line.childstart; ; elem = elem.next)
                                            {
                                                if (elem == local.selects[seln].state.elements.Last())
                                                {
                                                    n += local.selects[seln].n;
                                                    break;
                                                }
                                                n += elem.Count();
                                            }
                                            local.countn = n;
                                        }
                                        else n = local.countn;
                                        Element body = null;
                                        if (local.selects[seln].state.elements.Count >= 3) body = local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3];
                                        if (body != null && body.type == LetterType.CloneElement)
                                        {
                                            var clone = body as CloneElement;
                                            if (line == clone.childend)
                                            {
                                                var elem2 = clone.next;
                                                if (elem2.type == LetterType.Line || elem2.type == LetterType.VirtualLine)
                                                {
                                                    line2 = elem2 as Line;
                                                    local.selects[seln].state.elements.RemoveAt(local.selects[seln].state.elements.Count - 1);
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    for (var elem = line2.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line2.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n = n3;
                                                    }
                                                }
                                                else if (elem2.type == LetterType.CloneElement)
                                                {
                                                    clone = elem2 as CloneElement;
                                                    line2 = clone.childstart as Line;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                                    for (var elem = line2.childstart; ; elem = elem.next)
                                                    {
                                                        var n3 = n - elem.Count();
                                                        if (elem.type == LetterType.Kaigyou)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = 0;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        else if (elem == line2.childend)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                            local.selects[seln].n = elem.before.Count() - 1;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        if (n3 < 0)
                                                        {
                                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                            local.selects[seln].n = n;
                                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                                            return 1;
                                                        }
                                                        n = n3;
                                                    }
                                                }
                                            }
                                        }
                                        if (line2 == null)
                                        {
                                            if (line.next.type == LetterType.CloneElement)
                                            {
                                                var clone = line.next as CloneElement;
                                                local.selects[seln].state.elements.Add(null);
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 3] = clone;
                                                local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = clone.childstart;
                                                line2 = clone.childstart as Line;
                                                for (var elem = line2.childstart; ; elem = elem.next)
                                                {
                                                    var n3 = n - elem.Count();
                                                    if (elem.type == LetterType.Kaigyou)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                        local.selects[seln].n = 0;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    else if (elem == line2.childend)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                        local.selects[seln].n = elem.before.Count() - 1;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;

                                                    }
                                                    if (n3 < 0)
                                                    {
                                                        local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                        local.selects[seln].n = n;
                                                        local.selects[(seln + 1) % 2] = local.selects[seln];
                                                        return 1;
                                                    }
                                                    n = n3;
                                                }
                                            }
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = line.childend.before;
                                            local.selects[seln].n = line.childend.before.Count() - 1;
                                            local.selects[(seln + 1) % 2] = local.selects[seln];
                                        }
                                        else
                                        {
                                            local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 2] = line2;
                                            for (var elem = line2.childstart; ; elem = elem.next)
                                            {
                                                var n3 = n - elem.Count();
                                                if (elem.type == LetterType.Kaigyou)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = 0;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                else if (elem == line2.childend)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem.before;
                                                    local.selects[seln].n = elem.before.Count() - 1;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;

                                                }
                                                if (n3 < 0)
                                                {
                                                    local.selects[seln].state.elements[local.selects[seln].state.elements.Count - 1] = elem;
                                                    local.selects[seln].n = n;
                                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                                    return 1;
                                                }
                                                n = n3;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        local.selects[seln] = local.selects[(seln + 1) % 2];
                                    }
                                    break;
                                case Keys.None:
                                    local.panel.input = true;
                                    text = text.Substring(0, n1) + e.text + text.Substring(n2, text.Length - n2);
                                    var state3 = e.state.Clone();
                                    state3.elements[state3.elements.Count - 1] = this;
                                    local.selects[0] = local.selects[1] = new Select() { state = state3, n = n1 + e.text.Length };
                                    break;
                            }
                            e.state.Update();
                            e.state.elements[e.state.elements.Count - 1] = next;
                            return 1;
                        }
                        else
                        {
                            switch (e.key)
                            {
                                case Keys.Enter:
                                    local.panel.input = true;
                                    var line = new Line();
                                    var line2 = e.state.elements[e.state.elements.Count - 2];
                                    line.AddRange(this.next);
                                    this.next = line2.childend;
                                    line2.childend.before = this;
                                    text = text.Substring(0, local.selects[i].n);
                                    var kaigyou = new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou };
                                    this.Next(kaigyou);
                                    line2.Next(line);
                                    line.childstart = line.childend.next;
                                    e.state.elements[e.state.elements.Count - 1] = line2.childend;
                                    e.state.Update();
                                    select = true;
                                    return 1;
                                case Keys.Left:
                                case Keys.Up:
                                    local.selects[(local.seln + 1) % 2] = local.selects[local.seln];
                                    local.seln = 2;
                                    return 1;
                                case Keys.Right:
                                case Keys.Down:
                                    local.selects[local.seln] = local.selects[(local.seln + 1) % 2];
                                    local.seln = 2;
                                    return 1;
                                case Keys.Back:
                                case Keys.Delete:
                                case Keys.None:
                                    local.panel.input = true;
                                    text = text.Substring(0, local.selects[i].n) + e.text;
                                    break;
                            }
                            e.state.Update();
                            e.state.elements[e.state.elements.Count - 1] = next;
                            select = true;
                            return 1;
                        }
                    }
                }
            }
            e.state.elements[e.state.elements.Count - 1] = next;
            return 0;
        }
        public override int plus(int n)
        {
            return n - 1;
        }
        public override void nextplus(State state)
        {
            state.elements[state.elements.Count - 1] = state.elements.Last().next;
            if (state.elements.Last().type == LetterType.ElemEnd) state.elements.RemoveAt(state.elements.Count - 1);
        }
        public override string Text(Local local)
        {
            var str = "";
            if (local.selects[0].state.elements.Last() == this)
            {
                str += (char)('\uE000' + local.selects[0].n * 2);
            }
            if (local.selects[1].state.elements.Last() == this)
            {
                str += (char)('\uE000' + local.selects[1].n * 2 + 1);
            }
            return str + text;
        }
        public override string Text2(Local local)
        {
            return text;
        }
    }
    enum LetterType
    {
        None, Letter, AtLetter, Symbol, Number, Colon, Comma, Kaigyou, Dot, BracketS, BracketE, BlockS, BlockE, Call,
        Class, If, For, Elif, Else, End, Bou, BraceS, BraceE, Right, Left, Semicolon, Sharp, Str,
        Value, Func, ClassObj, Array,
        Plus, Minus, Mul, Div, LessThan, MoreThan, LessEqual, MoreEqual, EqualEqual, Operator,
        SingleMinus, Equal, In, Not, NotEqual, Decimal,
        Nyoro,
        HLetter,
        RightRight,
        Space,
        Dolor,
        Htm,
        NyoroNyoro,
        ElemEnd,
        SingleComent,
        NyoroNyoroNyoro,
        MinusMinus,
        MinusMinusMinus,
        CommentSingle,
        CommentMany,
        CommentManyEnd,
        CloneElement,
        Cell,
        StringTag,
        Sheet,
        Line,
        VirtualLine,
        Select,
        Percent,
        And,
        Mountain,
        Question,
        Span
    }
    enum CheckType
    {

        Setter, Finish, Return
    }
    partial class TypeCheck
    {
        public static bool Check(Obj val, Obj val2, CheckType type, Local local)
        {
            Type cls = null;
        head0:
            if (val.type == ObjType.Variable)
            {
                cls = (val as Variable).cls;
            }
            else if (val.type == ObjType.Function)
            {
                cls = (val as Function).ret;
            }
            else throw new Exception();
            return CheckCV(cls, val2, type, local) != null;
        }
        public static Type CheckCV(Type cls, Obj val2, CheckType type, Local local)
        {
            Var vartype2 = null;
        head0:
            if (cls.type == ObjType.Var)
            {
                var vartype = cls as Var;
                if (vartype.cls != null)
                {
                    vartype2 = vartype;
                    cls = vartype.cls;
                }
            }
            if (cls.type == ObjType.ArrayType)
            {
                var arrtype = cls as ArrType;
                if (val2.type == ObjType.Block || val2.type == ObjType.Array)
                {
                    if (val2.type == ObjType.Block) val2.type = ObjType.Array;
                    cls = arrtype.cls;
                    var block = val2 as Block;
                    if (val2.children.Count == 0) return cls;
                    else if (block.cls == null)
                    {
                        if (cls.type == ObjType.Var)
                        {
                            var vartype = cls as Var;
                            if (vartype.cls != null) cls = vartype.cls;
                            else
                            {
                                block.cls = vartype;
                                for (var i = 0; i < block.rets.Count; i++)
                                {
                                    if (CheckCV(block.cls, block.rets[i] as Type, type, local) == null) throw new Exception();
                                }
                                block.cls = vartype.cls;
                                return arrtype;
                            }
                        }
                        else
                        {
                            block.cls = cls;
                            for (var i = 0; i < block.rets.Count - 1; i++)
                            {
                                if (CheckCV(block.cls, block.rets[i], type, local) == null) throw new Exception();
                            }
                        }
                    }
                    else
                    {
                        if (cls.type == ObjType.Var)
                        {
                            var vartype = cls as Var;
                            if (vartype.cls == null)
                            {
                                vartype.cls = block.cls;
                                return arrtype;
                            }
                        }
                        if (cls.type == ObjType.ArrayType || cls.type == ObjType.FuncType)
                        {
                            if (!EqualClass(cls as Type, block.cls)) throw new Exception();
                        }
                        else if (ExtendCompare(cls as Type, block.cls) == null) throw new Exception();
                    }
                    return arrtype;
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.FuncType)
            {
                var functype = cls as FuncType;
                if (val2.type == ObjType.Function)
                {
                    var func = val2 as Function;
                    if (!EqualClass(functype.cls, func.ret as Type)) throw new Exception();
                    local.blocks = func.blocks;
                    var blk = func.draw.children[0].Clone() as Block;
                    local.blocks.Add(blk);
                    blk.exe(local);
                    var varray = new List<Obj>(blk.vmap.Values);
                    if (functype.draws.Count != varray.Count) throw new Exception();
                    for (var i = 0; i < varray.Count; i++)
                    {
                        if (varray[i].type == ObjType.Variable)
                        {
                            var variable = varray[i] as Variable;
                            if (!EqualClass(functype.draws[i], variable.cls)) throw new Exception();
                        }
                        else throw new Exception();
                    }
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blockslist.RemoveAt(local.blockslist.Count - 1);
                    return functype;
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.GenericObj || cls.type == ObjType.ClassObj || cls.type == ObjType.ModelObj || cls.type == ObjType.GeneObj)
            {
                var clst = cls as Type;
                if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.GeneValue)
                {
                    var cls2 = (val2 as Val).cls;
                    if (vartype2 == null)
                    {
                        return ExtendCompare(clst, cls2);
                    }
                    else
                    {
                        return ExtendCompare(vartype2, cls2);
                    }
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.Var)
            {
                Type vartype = cls as Var;
            head:
                if (val2.type == ObjType.Block || val2.type == ObjType.Array)
                {
                    if (val2.type == ObjType.Block) val2.type = ObjType.Array;
                    var arrtype = new ArrType(new Var());
                    vartype.cls = arrtype;
                    var block = val2 as Block;
                    if (block.rets.Count == 0)
                    {
                        return vartype;
                    }
                    else if (block.cls == null)
                    {
                        block.cls = arrtype.cls as Var;
                        for (var i = 0; i < block.rets.Count; i++)
                        {
                            if (CheckCV(block.cls, block.rets[i], type, local) == null) throw new Exception();
                        }
                        block.cls = (arrtype.cls as Var).cls;
                    }
                    (arrtype.cls as Var).cls = block.cls;
                    return vartype;
                }
                else if (val2.type == ObjType.Function)
                {
                    var func = val2 as Function;
                    var functype = new FuncType(new Var() { cls = func.ret });
                    var block1 = func.draw.children[0] as Block;
                    foreach (var v in block1.vmap.Values)
                    {
                        if (v.type == ObjType.Variable)
                        {
                            var variable = v as Variable;
                            if (variable.cls.type == ObjType.Var)
                            {
                                functype.draws.Add(variable.cls as Type);
                            }
                            else
                            {
                                functype.draws.Add(new Var() { cls = variable.cls });
                            }
                        }
                    }
                    vartype.cls = functype;
                    return vartype;
                }
                else if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.GeneValue)
                {
                    var cls2 = (val2 as Val).cls;
                    vartype.cls = cls2 as Type;
                    return vartype;
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.GeneLabel && val2.type == ObjType.GeneLabelValue) return cls;
            else throw new Exception();
        }
        public static bool EqualClass(Type cls, Type cls2)
        {
            Var vart = null, vart2 = null;
            if (cls.type == ObjType.Var)
            {
                vart = cls as Var;
                if (vart.cls != null)
                {
                    cls = vart.cls;
                }
            }
            if (cls2.type == ObjType.Var)
            {
                vart2 = cls2 as Var;
                if (vart2.cls != null)
                {
                    cls2 = vart2.cls;
                }
            }
            if (cls.type == ObjType.Var)
            {
                var vartype = cls as Var;
                if (cls2.type == ObjType.Var)
                {
                    var vartype2 = cls2 as Var;
                    vartype2.connects.Add(vartype);
                    vartype.connects.Add(vartype2);
                    return true;
                }
                else
                {
                    if (vart2 != null) cls2 = vart2.cls;
                    VarConnect(vartype, cls2);
                    return true;
                }
            }
            else if (cls2.type == ObjType.Var)
            {
                if (vart != null) cls = vart.cls;
                var vartype2 = cls2 as Var;
                VarConnect(vartype2, cls);
                return true;
            }
            else if (cls.type == ObjType.ArrayType)
            {
                if (cls2.type == ObjType.ArrayType)
                {
                    return EqualClass(cls.cls, cls2.cls);
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.FuncType)
            {
                var functype = cls as FuncType;
                if (cls2.type == ObjType.FuncType)
                {
                    var functype2 = cls2 as FuncType;
                    if (!EqualClass(functype, cls2.cls)) throw new Exception();
                    if (functype.draws.Count != functype2.draws.Count) throw new Exception();
                    for(var i = 0; i < functype.draws.Count; i++)
                    {
                        if (!EqualClass(functype.draws[i], functype2.draws[i])) throw new Exception();
                    }
                    return true;
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.GenericObj)
            {
                var gj = cls as GenericObj;
                if (cls2.type == ObjType.GenericObj)
                {
                    var gj2 = cls2 as GenericObj;
                    if (gj.gene != gj2.gene) throw new Exception();
                    if (gj.draws.Count != gj2.draws.Count) throw new Exception();
                    for(var i = 0; i < gj.draws.Count; i++)
                    {
                        if (!EqualClass(gj.draws[i], gj2.draws[i])) throw new Exception();
                    }
                    return true;
                }
                else throw new Exception();
            }
            else if ( cls.type == ObjType.ClassObj)
            {
                if (cls2.type == ObjType.ClassObj)
                {
                    if (cls != cls) throw new Exception();
                    return true;
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.ModelObj)
            {
                if (cls2.type == ObjType.ModelObj)
                {
                    if (cls != cls) throw new Exception();
                    return true;
                }
                else throw new Exception();
            }
            else if (cls.type == ObjType.GeneObj)
            {
                if (cls2.type == ObjType.GeneObj)
                {
                    if (cls != cls) throw new Exception();
                    return true;
                }
                else throw new Exception();
            }
            throw new Exception();
        }
        public static Type ExtendCompare(Type cls1, Type cls2)
        {
            var list1 = new List<Type>();
            var list2 = new List<Type>();
            Var vartype = null;
            if (cls1.type == ObjType.Var)
            {
                vartype = cls1 as Var;
                cls1 = vartype.cls;
            }
            list1.Add(cls1);
            ExtendsAdd(cls1, list1);
            list2.Add(cls2);
            ExtendsAdd(cls2, list2);
            for (var i = 0; i < list1.Count; i++)
            {
                for (var j = 0; j < list2.Count; j++)
                {
                    if (list1[i].type == ObjType.ClassObj && list2[j].type == ObjType.ClassObj)
                    {
                        if (list1[i] == list2[j]) return list1[i];
                    }
                    else if (list1[i].type == ObjType.ModelObj && list2[j].type == ObjType.ModelObj)
                    {
                        if (list1[i] == list2[j]) return list1[i];
                    }
                    else if (list1[i].type == ObjType.GeneObj && list2[j].type == ObjType.GeneObj)
                    {
                        if (list1[i] == list2[j]) return list1[i];
                    }
                    else if (list1[i].type == ObjType.GenericObj && list2[j].type == ObjType.GenericObj)
                    {
                        var gj1 = list1[i] as GenericObj;
                        var gj2 = list2[i] as GenericObj;
                        if (gj1.gene == gj2.gene && gj1.draws.Count == gj2.draws.Count)
                        {
                            var result = true;
                            for (var k = 0; k < gj1.draws.Count; k++)
                            {
                                if (gj1.draws[i] != gj2.draws[j])
                                {
                                    result = false;
                                }
                            }
                            if (result) return gj1.draws[i];
                        }
                    }
                }
            }
            throw new Exception();
        }
        public static void ExtendsAdd(Type cls, List<Type> list)
        {
            if (cls.type == ObjType.ClassObj)
            {
                var cj = cls as ClassObj;
                for (var i = 0; i < cj.extends.Count; i++)
                {
                    list.Add(cj.extends[i]);
                }
                for(var i = 0; i < cj.extends.Count; i++)
                {
                    ExtendsAdd(cj.extends[i], list);
                }
            }
            else if (cls.type == ObjType.GenericObj)
            {
                var gj = cls as GenericObj;
                for (var i = 0; i < gj.extends.Count; i++)
                {
                    list.Add(gj.extends[i]);
                }
                for (var i = 0; i < gj.extends.Count; i++)
                {
                    ExtendsAdd(gj.extends[i], list);
                }
            }
            else if (cls.type == ObjType.ModelObj)
            {
                var gj = cls as ModelObj;
                for (var i = 0; i < gj.extends.Count; i++)
                {
                    list.Add(gj.extends[i]);
                }
                for (var i = 0; i < gj.extends.Count; i++)
                {
                    ExtendsAdd(gj.extends[i], list);
                }
            }
        }
        public static void VarConnect(Var vartype, Type cls)
        {
            vartype.cls = cls;
            for (var i = 0; i < vartype.connects.Count; i++)
            {
                if (vartype.connects[i].cls == null)
                {
                    VarConnect(vartype.connects[i], cls);
                }
            }

        }
    }
}