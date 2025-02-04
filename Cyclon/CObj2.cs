using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cyclon
{
    class Comment : Obj
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
            local.blocks.Add(block);
            Obj ret = null;
            var comelet = (CommentLet)letter;
            comelet.comment = this;
            local.comments.Add(comelet);
            comelet.Renew();
            comelet.elems.Add(comelet.childend.next);
            comelet.nums.Add(0);
            for(var i = 0; i < children.Count; i++) ret = children[i].exe(local);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.comments.RemoveAt(local.comments.Count - 1);
            return this;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var block = new Block(ObjType.Comment);
            block.vmap["div"] = new ElemType(ObjType.Div);
            block.vmap["br"] = new ElemType(ObjType.Br);
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
    class Comment2 : Comment
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
    class Clones : Obj
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
                    line.Next(kaigyou);
                    break;
                }
                else if (element.type == LetterType.ElemEnd) break;
            }
            Element elem = line;
            foreach(var obj in objs)
            {
                var elem2 = new CloneElement(obj[0].parent, obj[1].parent);
                elem.Next(elem2);
                elem = elem2;
            }
            return this;
        }
    }
    class Dolor: Obj
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
                switch(word.name)
                {
                    case "type":
                        var clones = new Clones();
                        clones.letter = letter;
                        foreach(var blk in local.blocks)
                        {
                            foreach(var val in blk.vmap.Values)
                            {
                                if (val.type == ObjType.ClassObj || val.type == ObjType.ModelObj || val.type == ObjType.GeneObj )
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
                    if (local.comments.Count > 0) local.comments.Last().Add(new Letter() { text = str, name = str, type = LetterType.Htm});
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
    class HtmObj: Obj
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
            if (local.comments.Count > 0) local.comments.Last().Add(new Letter() { text = text, name = text, type = LetterType.Htm}); 
            return this;
        }
    }
    class TagBlock : CallBlock
    {
        public TagBlock() : base()
        {
            type = ObjType.TagBlock;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var block1 = children[0].Clone().exe(local).Getter(local) as Block;
            ElemObj divobj;
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
                    divobj.param(l.name, block1.rets[l.n]);
                }
                var block2 = children[1].Clone().exe(local).Getter(local) as Block;
                local.comments.Last().Back();
            }
            return this;
        }
        public override Obj exe(Local local)
        {
            var block1 = children[0].Clone().exe(local).Getter(local) as Block;
            ElemObj divobj;
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
                    divobj.param(l.name, block1.rets[l.n]);
                }
                var block2 = children[1].Clone().exe(local).Getter(local) as Block;
                local.comments.Last().Back();
            }
            return this;
        }
    }
    class ElemType: Obj
    {
        public ElemType(ObjType type): base(type)
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
    }
    class ElemObj: Obj
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
        }
        public void param(String name, Obj obj)
        {
            switch(name)
            {
                case "x":
                    elem.pos.X = (obj as Number).value;
                    break;
                case "y":
                    elem.pos.Y = (obj as Number).value;
                    break;
                case "pos":
                    var blk1 = obj as Block;
                    elem.pos = new PointF((blk1.rets[0] as Number).value, (blk1.rets[1] as Number).value);
                    break;
                case "w":
                case "width":
                    elem.size.X = (obj as Number).value;
                    break;
                case "h":
                case "height":
                    elem.size.Y = (obj as Number).value;
                    break;
                case "size":
                    var blk2 = obj as Block;
                    elem.pos = new PointF((blk2.rets[0] as Number).value, (blk2.rets[1] as Number).value);
                    break;
                case "b":
                case "background":
                    var strobj = (obj as StrObj).value;
                    switch(strobj)
                    {
                        case "red":
                            elem.background = Brushes.Red;
                            break;
                        case "green":
                            elem.background = Brushes.Green;
                            break;
                        case "blue":
                            elem.background = Brushes.Blue;
                            break;
                        case "white":
                            elem.background = Brushes.White;
                            break;
                        case "black":
                            elem.background = Brushes.Black;
                            break;
                    }
                    break;
                case "onclick":
                    (elem as Div).statuses.Add("mouse", obj);
                    break;
            }
        }
        public override Obj ope(string key, Local local, Obj val2)
        {
            if ((key == "+" || key == "!" || key == "*") && val2 == null)
            {
                if (type.type == ObjType.Div)
                {
                    (elem as Div).sop = key;
                }
                this.key = key;
                return this;
            }
            throw new Exception();
        }
    }
    class CDec : Obj
    {
        public CDec() : base(ObjType.Cdec)
        {

        }
    }
    class CFunc : Obj
    {
        public CFunc() : base(ObjType.CFunc)
        {

        }

    }
    class CType : Obj
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
    class Signal : Type
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
                    var func = new SignalFunction() { draw = val2 as CallBlock };
                    foreach (var b in local.blocks) func.blocks.Add(b);
                    for (var i = 0; i < local.blocks.Count; i--)
                    {
                        if (local.blocks[i].obj.type == ObjType.ServerFunction)
                        {
                            var sf = local.blocks[i].obj as ServerFunction;
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
    class SignalFunction : Obj
    {
        public List<Block> blocks = new List<Block>();
        public CallBlock draw;
        public SignalFunction() : base(ObjType.SingnalFunction)
        {
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
    }
    class ServerClient : Obj
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
    class ServerFunction : Obj
    {
        public String name;
        public ServerClient server;
        public CallBlock draw;
        public List<Block> blocks = new List<Block>();
        public ServerFunction(ServerClient server) : base(ObjType.ServerFunction)
        {
            this.server = server;
        }
        public override Obj exe(Local local)
        {
            local.blockslist.Add(new List<Block>());
            var blk = draw.children[1] as Block;
            blk.obj = this;
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
    class Connect : Obj
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
    class ConnectStock : Stock
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
    class AddressType : Type
    {
        public AddressType() : base(ObjType.AddressType)
        {
        }
    }
    class Address : Obj
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
}
