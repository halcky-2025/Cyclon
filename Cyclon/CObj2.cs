using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Cyclon
{
    partial class Comment : Obj
    {
        public Dictionary<String, Letter> vmap = new Dictionary<String, Letter>();
        public Comment() : base(ObjType.Comment)
        {
        }
        public override Obj exe(Local local)
        {
            var block = new Block(ObjType.Comment);
            block.vmap["div"] = new ElemType(ObjType.Div);
            block.vmap["br"] = new ElemType(ObjType.Br);
            block.vmap["sheet"] = new ElemType(ObjType.Sheet);
            block.vmap["cell"] = new ElemType(ObjType.Cell);
            block.vmap["span"] = new ElemType(ObjType.Span);
            block.vmap["linear"] = new LinearFunction();
            local.blocks.Add(block);
            Obj ret = null;
            var comelet = (CommentLet)letter;
            comelet.comment = this;
            local.comments.Add(comelet);
            comelet.Renew();
            comelet.elems.Add(comelet.childend.next);
            comelet.nums.Add(0);
            for (var i = 0; i < children.Count; i++) ret = children[i].exe(local);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.comments.RemoveAt(local.comments.Count - 1);
            return this;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var block = new Block(ObjType.Comment);
            block.vmap["div"] = new ElemType(ObjType.Div);
            block.vmap["br"] = new ElemType(ObjType.Br);
            block.vmap["sheet"] = new ElemType(ObjType.Sheet);
            block.vmap["cell"] = new ElemType(ObjType.Cell);
            block.vmap["span"] = new ElemType(ObjType.Span);
            block.vmap["linear"] = new LinearFunction();
            local.blocks.Add(block);
            Obj ret = null;
            var comelet = (CommentLet)letter;
            comelet.comment = this;
            local.comments.Add(comelet);
            comelet.Renew();
            comelet.elems.Add(comelet.childend.next);
            comelet.nums.Add(0);
            var htms = new List<HtmObj>();
            for (var i = 0; i < children.Count; i++) ret = children[i].exe(local);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.comments.RemoveAt(local.comments.Count - 1);
            return this;
        }
        public override Obj Clone()
        {
            return new Comment() { letter = letter, vmap = vmap, children = children };
        }
    }
    partial class LinearFunction : Obj
    {
        public LinearFunction() : base(ObjType.LineraFunction)
        {
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                var blk = val2.Clone().exe(local).Getter(local) as Block;
                var anif = new AnimationFunction();
                foreach(var v in blk.labelmap)
                {
                    if (int.TryParse(v.Key, out int n2))
                    {
                        anif.imap.Add(n2, blk.rets[v.Value.n].Getter(local) as Block);
                    }
                    var val = blk.rets[v.Value.n];
                }
                foreach (var val in blk.rets)
                {
                    if (val.type == ObjType.FloatVal)
                    {
                        anif.interval = (val as FloatVal).value;
                    }
                    else if (val.type == ObjType.Number)
                    {
                        anif.loop = (val as Number).value;
                    }
                }
                anif.SetInit(local);
                return anif;
            }
            throw new Exception();
        }
    }
    partial class AnimationFunction : Obj
    {
        public float interval;
        public int loop = 1;
        public SortedList<int, Block> imap = new SortedList<int, Block>();
        public Div div;
        public int start;
        public AnimationFunction() : base(ObjType.AnimationFunction)
        {
        }
        public void SetInit(Local local)
        {
            Dictionary<String, SortedList<int, int>> perterns = new Dictionary<string, SortedList<int, int>>();
            for (var i = 0; i < imap.Count; i++)
            {
                foreach (var key in imap.Values[i].labelmap)
                {
                    if (!perterns.ContainsKey(key.Key))
                    {
                        perterns[key.Key] = new SortedList<int, int>();
                    }
                    perterns[key.Key][i] = imap.Keys[i];
                }
            }
            foreach (var key in perterns.Keys)
            {
                int n = -1;
                for (var i = 0; i < imap.Count; i++)
                {
                    if (!perterns[key].ContainsKey(i))
                    {
                        if (n == -1)
                        {
                            imap[i].labelmap.Add(key, new Label() { n = imap[i].rets.Count, name = key });
                            imap[i].rets.Add(new Null());
                        }
                        else
                        {
                            var through = true;
                            for (var j = i + 1; j < imap.Count; j++)
                            {
                                if (perterns[key].ContainsKey(j))
                                {
                                    imap[i].labelmap.Add(key, new Label() { n = imap[i].rets.Count, name = key });
                                    imap[i].rets.Add(imap[n].rets[imap[n].labelmap[key].n].Hokan(imap[j].rets[imap[j].labelmap[key].n], (float)(imap.Keys[i] - perterns[key][n]) / (perterns[key][j] - perterns[key][n]), local));
                                    through = false;
                                    break;
                                }
                            }
                            if (through)
                            {
                                imap[i].labelmap.Add(key, new Label() { n = imap[i].rets.Count, name = key });
                                imap[i].rets.Add(new Null());
                            }
                            continue;
                        }
                    }
                    else n = i;
                }
            }
            start = Environment.TickCount;
        }
        public void Interval(int n, Local local)
        {
            float n2 = ((n - start) / interval / 10);
            if (loop > 0 && loop - ((int)n2) / 100 <= 0)
            {
                local.animations.Remove(this);
                Set(div, imap.Values[imap.Count - 1], local);
                return;
            }
            for (var i = 0; i < imap.Count; i++)
            {
                if (imap.Keys[i] >= n2 % 100)
                {
                    if (i == 0)
                    {
                        Set(div, imap.Values[0], local);
                        return;
                    }
                    else
                    {
                        var before = imap.Values[i - 1];
                        var after = imap.Values[i];
                        var blk = new Block(ObjType.Array);
                        foreach (var v in before.labelmap)
                        {
                            var obj1 = before.rets[before.labelmap[v.Key].n];
                            var obj2 = after.rets[after.labelmap[v.Key].n];
                            if (obj1.type == ObjType.Null) continue;
                            else if (obj2.type == ObjType.Null) continue;
                            blk.labelmap.Add(v.Key, new Label() { n = blk.rets.Count, name = v.Key });
                            blk.rets.Add(obj1.Hokan(obj2, n2 % 100 / (imap.Keys[i] - imap.Keys[i - 1]), local));
                        }
                        Set(div, blk, local);
                        return;
                    }
                }
            }
            Set(div,imap.Values[imap.Count - 1], local);
        }
        public void Set(Div div, Block block, Local local)
        {
            foreach (var labelmap in block.labelmap)
            {
                div.SetParam(labelmap.Key, block.rets[labelmap.Value.n], local);
            }
            local.panel.input = true;
            local.panel.Invalidate();
        }
    }
    partial class Comment2 : Comment
    {
        public Comment2()
        {
            type = ObjType.Comment2;
        }
        public override Obj exe(Local local)
        {
            return this;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class Clones : Obj
    {
        public List<Letter[]> objs = new List<Letter[]>();
        public Clones() : base(ObjType.Clones)
        {
            opes[">>"] = RightRight;
        }
        public Obj RightRight(String op, Local local, Obj val2)
        {
            local.panel.input = true;
            var line2 = letter.parent;
        head:
            line2 = line2.next;
            if (line2.type == LetterType.CloneElement)
            {
                line2.next.RemoveBefore();
                goto head;
            }
            var line = letter.parent;
            for (Element element = letter; ; element = element.next)
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
                    line.childend.before.Next(kaigyou);
                    break;
                }
                else if (element.type == LetterType.ElemEnd) break;
            }
            Element elem = line;
            foreach (var obj in objs)
            {
                var elem2 = new CloneElement(obj[0].parent, obj[1].parent);
                elem.Next(elem2);
                elem = elem2;
            }
            return this;
        }
    }
    partial class Dolor : Obj
    {
        public Dolor() : base(ObjType.Dolor)
        {

        }
        public Dolor(Letter letter) : this()
        {
            this.letter = letter;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                switch (word.name)
                {
                    case "type":
                        var clones = new Clones();
                        clones.letter = letter;
                        foreach (var blk in local.blocks)
                        {
                            foreach (var val in blk.vmap.Values)
                            {
                                if (val.type == ObjType.ClassObj || val.type == ObjType.ModelObj || val.type == ObjType.GeneObj)
                                {
                                    var type = val as Type;
                                    if (type.initial) continue;
                                    clones.objs.Add(new Letter[] { type.letter, type.letter2 });
                                }
                                else if (val.type == ObjType.Generic)
                                {
                                    var generic = val as Generic;
                                    clones.objs.Add(new Letter[] { generic.letter, generic.letter2 });
                                }
                            }
                        }
                        return clones;
                    case "func":
                        break;
                }
            }
            else if (val2.type == ObjType.Bracket)
            {
                var blk = val2.Clone().exe(local).Getter(local) as Block;
                val2 = blk.rets[0];
                if (blk.rets[0].type == ObjType.Number)
                {
                    var str = (val2 as Number).value.ToString();
                    if (local.comments.Count > 0) local.comments.Last().Add(new Letter() { text = str, name = str, type = LetterType.Htm });
                }
                else if (blk.rets[0].type == ObjType.StrObj)
                {
                    var str = (val2 as StrObj).value;
                    if (local.comments.Count > 0) local.comments.Last().Add(new Letter() { text = str, name = str, type = LetterType.Htm });
                }
                return null;
            }
            throw new Exception();
        }
    }
    partial class HtmObj : Obj
    {
        public String text;
        public HtmObj(String text) : base(ObjType.Htm)
        {
            this.text = text;
        }
        public HtmObj(Letter letter) : this(letter.name)
        {
            this.letter = letter;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            if (local.comments.Count > 0)
            {
                if (letter.before.type == LetterType.Select)
                {
                    local.comments.Last().Add(letter.before);
                    if (letter.before.before.type == LetterType.Select)
                    {
                        local.comments.Last().Add(letter.before.before);
                    }
                }
                local.comments.Last().Add(new Letter() { text = text, name = text, type = LetterType.Htm });
            }
            return this;
        }
    }
    partial class TagBlock : CallBlock
    {
        public ElemObj divobj;
        public TagBlock() : base()
        {
            type = ObjType.TagBlock;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var block1 = children[0].Clone().exe(local).Getter(local) as Block;
            if (block1.rets[0] is ElemType)
            {
                divobj = new ElemObj(block1.rets[0] as ElemType, null);
            }
            else if (block1.rets[0] is ElemObj) divobj = block1.rets[0] as ElemObj;
            else divobj = new ElemObj(new ElemType(ObjType.Div), null);
            if (local.comments.Count > 0)
            {
                local.comments.Last().AddNext(divobj.elem);
                foreach (var l in block1.labelmap.Values)
                {
                    divobj.param(l.name, block1.rets[l.n], local);
                }
                var block2 = children[1].Clone().exe(local).Getter(local) as Block;
                local.comments.Last().Back();
            }
            return this;
        }
        public override Obj exe(Local local)
        {
            var block1 = children[0].Clone().exe(local).Getter(local) as Block;
            if (block1.rets[0] is ElemType)
            {
                divobj = new ElemObj(block1.rets[0] as ElemType, null);
            }
            else if (block1.rets[0] is ElemObj) divobj = block1.rets[0] as ElemObj;
            else divobj = new ElemObj(new ElemType(ObjType.Div), null);
            if (local.comments.Count > 0)
            {
                local.comments.Last().AddNext(divobj.elem);
                foreach (var l in block1.labelmap.Values)
                {
                    divobj.param(l.name, block1.rets[l.n], local);
                }
                var block2 = children[1].Clone().exe(local).Getter(local) as Block;
                local.comments.Last().Back();
            }
            return this;
        }
    }
    partial class ElemType : Obj
    {
        public ElemType(ObjType type) : base(type)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var val2 = primary.children[n + 1];
            if (val2.type == ObjType.Word)
            {
                n++;
                var word = val2 as Word;
                var elemobj = new ElemObj(this, word.name);
                local.declare(word.name, elemobj);
                return elemobj;
            }
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                var elemobj = new ElemObj(this, word.name);
                local.declare(word.name, elemobj);
                return elemobj;
            }
            else throw new Exception();
        }
        public override Obj ope(string key, Local local, Obj val2)
        {
            if ((key == "+" || key == "!" || key == "*") && val2 == null)
            {
                return this;
            }
            throw new Exception();
        }
    }
    partial class ElemObj : Obj
    {
        public String key;
        public ElemType type;
        public Element elem;
        public ElemObj(ElemType type, String id) : base(ObjType.ElemObj)
        {
            this.type = type;
            if (type.type == ObjType.Br)
            {
                elem = new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou };
            }
            else if (type.type == ObjType.Div)
            {
                elem = new Div() { id = id };
            }
            else if (type.type == ObjType.Sheet)
            {
                elem = new Sheet() { id = id };
            }
            else if (type.type == ObjType.Cell)
            {
                elem = new Cell() { id = id };
            }
            else if (type.type == ObjType.Span)
            {
                elem = new Span() { id = id };
            }
        }
        public void param(String name, Obj obj, Local local)
        {
            if (elem is Div)
            {
                (elem as Div).statuses.Add(name, obj);
                (elem as Div).SetParam(name, obj, local);
            }
            else if (elem is Span)
            {
                (elem as Span).statuses.Add(name, obj);
                (elem as Span).SetParam(name, obj, local);
            }
        }
        public override Obj ope(string key, Local local, Obj val2)
        {
            if ((key == "+" || key == "!" || key == "*") && val2 == null)
            {
                if (type.type == ObjType.Div || type.type == ObjType.Sheet || type.type == ObjType.Cell)
                {
                    (elem as Div).sop = key;
                }
                this.key = key;
                return this;
            }
            throw new Exception();
        }
    }
    partial class CDec : Obj
    {
        public CDec() : base(ObjType.Cdec)
        {

        }
    }
    partial class CFunc : Obj
    {
        public CFunc() : base(ObjType.CFunc)
        {

        }

    }
    partial class CType : Obj
    {
        public CType() : base(ObjType.CType)
        {

        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {

            }
            else if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word = val2 as Word;
                    n++;
                    val2 = primary.children[n];
                    if (word.name == "Select")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var block = val2.Clone().exe(local).Getter(local);
                            n++;
                        }
                    }
                }
            }
            return base.Primary(ref n, local, primary, val2);
        }
    }
    partial class Signal : Type
    {
        public Signal() : base(ObjType.Signal)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    var func = new SignalFunction() { draw = val2 as CallBlock, name = word.name };
                    foreach (var b in local.blocks) func.blocks.Add(b);
                    for (var i = local.blocks.Count - 1; i > 0; i--)
                    {
                        if (local.blocks[i].obj.obj.type == ObjType.ServerFunction)
                        {
                            var sf = local.blocks[i].obj.obj as ServerFunction;
                            local.sigmap[sf.server.name + ":" + word.name] = func;
                            local.declare(word.name, func);
                            return func;
                        }
                    }
                }
            }
            throw new Exception();
        }
    }
    partial class SignalFunction : Obj
    {
        public String name;
        public List<Block> blocks = new List<Block>();
        public CallBlock draw;
        public SignalFunction() : base(ObjType.SingnalFunction)
        {
            cls = new CommentType();
        }
        public override Obj exe(Local local)
        {
            return basicexe(new Stock(), new Stock(), local);
        }
        public Obj basicexe(Stock stock1, Stock stock2, Local local)
        {
            var block1 = draw.children[0].Clone() as Block;
            block1.vmap["req"] = stock1;
            block1.vmap["res"] = stock2;
            local.blockslist.Add(blocks);
            local.blocks.Add(block1);
            var block2 = draw.children[1].Clone().exe(local).Getter(local) as Block;
            var val2 = block2.rets.Last();
            if (val2.type == ObjType.Return)
            {
                val2 = (val2 as Return).value;
                if (val2.type == ObjType.Comment)
                {
                    var comment = val2 as Comment;
                    local.vision.addcomment(comment);
                    local.vision.panel.input = true;
                    local.vision.panel.Invalidate();
                }
            }
            else if (val2.type == ObjType.Continue && val2.type == ObjType.Break && val2.type == ObjType.Goto) throw new Exception();
            if (val2.type != ObjType.Comment) throw new Exception();
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return val2;
        }
        public override string Text()
        {
            return name;
        }
    }
    partial class ServerClient : Obj
    {
        public String name;
        public ServerClient(String name) : base(ObjType.ServerClient)
        {
            this.name = name;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
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
                    return local.sigmap[name + ":" + word.name];
                }
            }
            else if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    var sf = new ServerFunction(this) { draw = val2 as CallBlock, name = name + ":" + word.name };
                    foreach (var b in local.blocks) sf.blocks.Add(b);
                    local.sigmap[sf.name] = sf;
                    return sf;
                }
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var sf = new ServerFunction(this) { draw = val2 as CallBlock, name = name };
                foreach (var b in local.blocks) sf.blocks.Add(b);
                local.sigmap[sf.name] = sf;
                return sf;
            }
            throw new Exception();
        }
    }
    partial class ServerFunction : Obj
    {
        public String name;
        public ServerClient server;
        public CallBlock draw;
        public List<Block> blocks = new List<Block>();
        public ServerFunction(ServerClient server) : base(ObjType.ServerFunction)
        {
            this.server = server;
            this.cls = new CommentType();
        }
        public override Obj exe(Local local)
        {
            local.blockslist.Add(blocks);
            var blk = draw.children[1] as Block;
            blk.obj = new ObjBlock() { obj = this, n = 1 };
            local.blocks.Add(blk);
            blk.exe(local).Getter(local);
            if (blk.rets.Last().type == ObjType.Return)
            {
                var val2 = (blk.rets.Last() as Return).value;
                if (val2.type == ObjType.Comment)
                {
                    var comment = val2 as Comment;
                    local.vision.addcomment(comment);
                    local.vision.panel.input = true;
                    local.vision.panel.Invalidate();
                }
            }
            else throw new Exception();
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return this;
        }
    }
    partial class Connect : Obj
    {
        public Connect() : base(ObjType.Connect)
        {
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
                    val2 = primary.children[n];
                    if (word.name == "new")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var blk1 = val2.Clone().exe(local).Getter(local) as Block;
                            if (blk1.rets.Count == 0)
                            {
                                return new ConnectStock();
                            }
                            else if (blk1.rets.Count == 1)
                            {
                                if (blk1.rets[0].type == ObjType.Address) return new ConnectStock() { address = blk1.rets[0] as Address };
                            }
                            else throw new Exception();
                        }
                    }
                }
                else if (val2.type == ObjType.Gene)
                {

                }
            }
            throw new Exception();
        }
    }
    partial class ConnectStock : Stock
    {
        public Address address;
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
                    val2 = primary.children[n];
                    if (word.name == "send")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var block = val2.Clone().exe(local).Getter(local) as Block;
                            n++;
                            if (block.rets.Count == 1)
                            {
                            }
                            else if (block.rets.Count == 2)
                            {
                                address = (Address)block.rets[1];
                            }

                            return block.rets[0];
                        }
                    }
                    else if (word.name == "back")
                    {
                        var variable = new Variable(new AddressType());
                        return variable;
                    }
                    else if (word.name == "Store")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var block = val2.Clone().exe(local).Getter(local) as Block;
                            StoreAny(block, local);
                            n++;
                        }
                    }
                    else
                    {
                        if (val2.type == ObjType.Dot)
                        {
                            n++;
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Word)
                            {
                                var word2 = val2 as Word;
                                n++;
                                val2 = primary.children[n];
                                if (word2.name == "Output")
                                {
                                    var comment = new Comment();
                                    var comlet = new CommentLet();
                                    var div = new Div();
                                    comment.letter = comlet;
                                    comlet.add(div);
                                    div.add(Output(local.get(word.name), local));
                                    return comment;
                                }
                                else if (word2.name == "Select")
                                {
                                    if (val2.type == ObjType.Bracket)
                                    {
                                        var blk = val2.exe(local).Getter(local) as Block;
                                        var func = blk.rets[0] as Function;
                                        var ftype = new FuncType(local.Bool);
                                        ftype.draws.Add((Type)local.get(word.name));
                                        if (TypeCheck.CheckCV(ftype, func, CheckType.Setter, local) == null) throw new Exception();
                                        return Select(local.get(word.name), local, func);
                                    }
                                }
                                else if (word2.name == "First")
                                {
                                    if (val2.type == ObjType.Bracket && val2.children.Count == 0)
                                    {
                                        return First(local.get(word.name), local);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new Exception();
        }
        public void StoreAny(Block block, Local local)
        {

            foreach (var v in block.rets)
            {
                if (v.type == ObjType.Block || v.type == ObjType.Array)
                {
                    var blk = v as Block;
                    StoreAny(blk, local);
                }
                else if (v.type == ObjType.ModelValue || v.type == ObjType.GeneValue)
                {
                    Store(v as Val, local);
                }
            }
        }
    }
    partial class AddressType : Type
    {
        public AddressType() : base(ObjType.AddressType)
        {
        }
    }
    partial class Address : Obj
    {
        public String address;
        public Block draw;
        public Address(String name) : base(ObjType.Address)
        {
            address = name;
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
                    address += "/" + word.name;
                }
            }
            else if (val2.type == ObjType.Bracket)
            {
                var block = val2.Clone().exe(local).Getter(local) as Block;
                draw = block;
                n++;
                return this;
            }
            throw new Exception();
        }
    }
    partial class Mountain : Obj
    {
        public Mountain(Letter letter) : base(ObjType.Mountain)
        {
            this.letter = letter;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
            var rets = new List<List<int>>();
        head:
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                var num = true;
                var n2 = word.name.Length;
                List<int> ret = new List<int>();
                for (var i = word.name.Length - 1; i >= 0; i--)
                {
                    if (num)
                    {
                        if ('0' <= word.name[i] && word.name[i] <= '9')
                        {
                            if (!num)
                            {
                                var n3 = 0;
                                for (var j = i + 1; j < n2; j++)
                                {
                                    n3 *= ('Z' - 'A' + 1);
                                    n3 += word.name[j] - 'A' + 1;
                                }
                                ret.Add(n3);
                            }
                        }
                        else if ('A' <= word.name[i] && word.name[i] <= 'Z')
                        {
                            if (num)
                            {
                                var text = word.name.Substring(i + 1, n2 - i - 1);
                                ret.Add(Convert.ToInt32(text) - 1);
                                n2 = i + 1;
                                num = !num;
                            }
                        }
                        else throw new Exception();
                    }
                }
                if (num)
                {
                    var text = word.name.Substring(0, n2);
                    ret.Add(Convert.ToInt32(text) - 1);
                }
                else
                {
                    var n3 = 0;
                    for (var j = 0; j < n2; j++)
                    {
                        n3 *= ('Z' - 'A' + 2);
                        n3 += word.name[j] - 'A' + 1;
                    }
                    ret.Add(n3 - 1);
                }
                n++;
                val2 = primary.children[n];
                rets.Add(ret);
                if (rets.Count == 1)
                {
                    if (val2.type == ObjType.Mountain)
                    {
                        n++;
                        val2 = primary.children[n];
                        goto head;
                    }
                    var blk = new Block(ObjType.Array);
                    var blk1 = new Block(ObjType.Array);
                    blk.rets.Add(blk1);
                    blk.rets.Add(blk1);
                    for (var i = 0; i < rets[0].Count; i++)
                    {
                        blk1.rets.Add(new Number(rets[0][i]) { cls = local.Int });
                    }
                    return blk;
                }
                else
                {
                    n--;
                    var blk = new Block(ObjType.Array);
                    var blk1 = new Block(ObjType.Array);
                    blk.rets.Add(blk1);
                    var blk2 = new Block(ObjType.Array);
                    blk.rets.Add(blk2);
                    for (var i = 0; i < rets[0].Count; i++)
                    {
                        blk1.rets.Add(new Number(rets[0][i]) { cls = local.Int });
                    }
                    for (var i = 0; i < rets[1].Count; i++)
                    {
                        blk2.rets.Add(new Number(rets[1][i]) { cls = local.Int });
                    }
                    //SetArray(blk, new int[minis.Count], minis, maxes, 0, local);
                    return blk;
                }
            }
            throw new Exception();
        }
    }
    partial class Sum : Obj
    {
        public Sheet sheet;
        public Cell range;
        public Sum(Sheet sheet, Cell range) : base(ObjType.Sum)
        {
            this.sheet = sheet;
            this.range = range;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                var blk = val2.Clone().exe(local).Getter(local) as Block;
                var rets = new List<List<int>>();
                if (blk.rets.Count == 1 && blk.rets[0].type == ObjType.Array)
                {
                    var blk2 = blk.rets[0].Clone().exe(local).Getter(local) as Block;
                    for (var i = 0; i < blk2.rets.Count; i++)
                    {
                        rets.Add(new List<int>());
                        foreach (var nums in (blk2.rets[i].Clone().Getter(local) as Block).rets)
                        {
                            if (nums.type == ObjType.Number) rets[i].Add((nums as Number).value);
                            else throw new Exception();
                        }
                    }
                }
                else throw new Exception();
                if (rets[0].Count < rets[1].Count)
                {
                    for (var i = rets[0].Count; i < rets[1].Count; i++) rets[0].Add(0);
                }
                else if (rets[0].Count > rets[1].Count)
                {
                    for (var i = rets[1].Count; i < rets[0].Count; i++) rets[1].Add(0);
                }
                var minmax = new List<List<int>>();
                minmax.Add(new List<int>());
                minmax.Add(new List<int>());
                for (var i = 0; i < rets[0].Count; i++)
                {
                    if (rets[0][i] <= rets[1][i])
                    {
                        minmax[0].Add(rets[0][i]);
                        minmax[1].Add(rets[1][i]);
                    }
                    else
                    {
                        minmax[1].Add(rets[0][i]);
                        minmax[0].Add(rets[1][i]);
                    }
                }
                int[] array = new int[minmax.Count];
                var n2 = 0;
                ObjInt c = new ObjInt();
                List<Cell> cells = new List<Cell>();
                SetArray((nums) =>
                {
                    c.count++;
                    var text = sheet.cells[nums[0]][nums[1]].Text3(local).Trim(); ;
                    sheet.cells[nums[0]][nums[1]].relations.relations[new Point(nums[1], nums[0])] = new RelationCell() { x = nums[1], y = nums[0] , count = c};
                    if (text != "") n2 += Convert.ToInt32(text);
                    cells.Add(sheet.cells[nums[0]][nums[1]]);
                }, array, minmax, 0, local);
                return new Number(n2) { cls = local.Int };
            }
            throw new Exception();
        }
        void SetArray(Action<int[]> task, int[] nums, List<List<int>> minimax, int n, Local local)
        {
            if (n == minimax[0].Count)
            {
                task(nums);
                return;
            }
            for (var i = minimax[0][n]; i <= minimax[1][n]; i++)
            {
                nums[n] = i;
                SetArray(task, nums, minimax, n + 1, local);
            }
        }
    }
    partial class Question : Obj
    {
        public Question(Letter letter) : base(ObjType.Question)
        {
            this.letter = letter;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var blk = new Block(ObjType.Array);
            var blk1 = new Block(ObjType.Array);
            blk.rets.Add(blk1);
            blk.rets.Add(blk1);
            for (var i = 0; i < 2; i++)
            {
                blk1.rets.Add(new Number(0) { cls = local.Int });
            }
            return blk1;
        }
    }
}

