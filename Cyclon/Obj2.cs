using Cyclon;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.VisualBasic.Devices;
using SQLitePCL;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace Cyclon
{
    class OpeFunc
    {
        public LetterType[] types;
    }
    partial class CallBlock : Obj
    {
        public CallBlock() : base(ObjType.CallBlock)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj exe(Local local)
        {
            var block2 = children[1] as Block;
            block2.vmap["int"] = local.Int;
            block2.vmap["Str"] = local.Str;
            block2.vmap["bool"] = local.Bool;
            block2.vmap["float"] = local.Float;
            local.blocks = new List<Block>();
            local.blocks.Add(block2);
            block2.exe(local);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            return this;
        }
    }
    partial class Block : Obj
    {
        public Letter letter2;
        public Obj obj = new Obj(ObjType.None);
        public Dictionary<String, Obj> vmap = new Dictionary<string, Obj>();
        public List<Obj> rets = new List<Obj>();
        public Type cls;
        public bool template = false;
        public Dictionary<String, Label> labelmap = new Dictionary<string, Label>();
        public Dictionary<String, Label> branchmap = new Dictionary<string, Label>();
        public Block(ObjType type) : base(type)
        {
            if (type == ObjType.Bracket)
            {
                opes["="] = opes[":"] = Equal;
                opes["in"] = In;
                opes["+"] = opes["-"] = opes["*"] = opes["/"] = Ope;
                opes["<="] = opes[">="] = opes["<"] = opes["<"] = opes["=="] = opes["!="] = Ope;
            }
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            if (type == ObjType.Block)
            {
                var m = 0;
                if (children[m].type == ObjType.Word)
                {
                    if ((children[m] as Word).name == "T")
                    {
                        m++;
                        var val2 = children[n];
                        Type type = new VariClass(0);
                    head:
                        if (val2.type == ObjType.Block)
                        {
                            var blk = val2 as Block;
                            if (blk.children.Count == 0)
                            {
                                type = new ArrType(type);
                                m++;
                                val2 = children[n];
                                goto head;
                            }
                            else throw new Exception();
                        }
                        n = 1;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var word = val2 as Word;
                            n++;
                            val2 = primary.children[n];
                            var gene = false;
                            if (val2.type == ObjType.Gene)
                            {
                                gene = true;
                                n++;
                                val2 = primary.children[n];
                            }
                            if (val2.type == ObjType.Block)
                            {
                                var func = new GenericFunction(type);
                                var blk = val2.exe(local) as Block;
                                for (var i = 0; i < blk.rets.Count; i++)
                                {
                                    if (blk.rets[i].type == ObjType.Word)
                                    {
                                        var word2 = blk.rets[i] as Word;
                                        func.vmap[word2.name] = new VariClass(i);
                                    }
                                }
                                n++;
                                val2 = primary.children[n];
                                if (val2.type == ObjType.CallBlock)
                                {
                                    func.draw = val2 as CallBlock;
                                    foreach (var b in local.blocks) func.blocks.Add(b);
                                    local.declare(word.name, func);
                                    return func;
                                }
                                else throw new Exception();
                            }
                            else if (val2.type == ObjType.CallBlock)
                            {
                                var func = new Function(type);
                                func.draw = val2 as CallBlock;
                                foreach (var b in local.blocks) func.blocks.Add(b);
                                local.declare(word.name, func);
                                return func;
                            }
                        }
                    }
                }
            }
            return exeRange(0, children.Count, local, true);
        }
        public override Obj exe(Local local)
        {
            if (type == ObjType.Array) return this;
            return exeRange(0, children.Count, local, true);
        }
        public Obj exeRange(int start, int end, Local local, bool id)
        {
            if (id)
            {
                List<Id> rs = new List<Id>(), rs2 = new List<Id>();
                var first = true;
                foreach (var l in branchmap.Values)
                {
                    if (first)
                    {
                        for (var i = 0; i < l.n; i++)
                        {
                            rets.Add(children[i].exe(local));
                            var last = rets.Last();
                            if (last.type == ObjType.Break || last.type == ObjType.Continue || last.type == ObjType.Return)
                            {
                                if (type == ObjType.Call2) return this;
                                else throw new Exception();
                            }
                            else if (last.type == ObjType.Goto)
                            {
                                var name = (last as Goto).value;
                                if (labelmap.ContainsKey(name))
                                {
                                    i = labelmap[name].n - 1;
                                    continue;
                                }
                                else return this;
                            }
                        }
                        first = false;
                    }
                    if (l.name == "end") continue;
                    for (var i = 0; i < local.ids.Count; i++)
                    {
                        if (local.ids[i].names[0] == l.name)
                        {
                            rs.Add(local.ids[i]);
                        }
                    }
                    if (l.labelmap.Count == 0) throw new Exception();
                    var ls = new List<Label>(l.labelmap.Values);
                    for (var i = 0; i < ls.Count; i++)
                    {
                        var l2 = ls[i];
                        rs2 = new List<Id>();
                        for (var j = 0; j < rs.Count; j++)
                        {
                            if (rs[j].names[1] == l2.name)
                            {
                                if (rs[j].names.Length == 2)
                                {
                                    start = l2.n;
                                    if (i + 1 != ls.Count) end = ls[i + 1].n;
                                    else if (branchmap.ContainsKey("end")) end = branchmap["end"].n;
                                    goto finish;
                                }
                                else
                                {
                                    rs2.Add(rs[j]);
                                }
                            }
                        }
                        if (l.labelmap.Count != 0)
                        {
                            var ls2 = new List<Label>(l2.labelmap.Values);
                            for (var j = 0; j < ls2.Count; j++)
                            {
                                var l3 = ls2[j];
                                for (var k = 0; k < rs2.Count; j++)
                                {
                                    if (rs2[k].names[2] == l3.name)
                                    {
                                        start = l3.n;
                                        if (j + 1 != ls2.Count) end = ls2[j + 1].n;
                                        else if (branchmap.ContainsKey("end")) end = branchmap["end"].n;
                                        goto finish;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        finish:
            for (var i = start; i < end; i++)
            {
                rets.Add(children[i].exe(local));
                var last = rets.Last();
                if (last.type == ObjType.Break || last.type == ObjType.Continue || last.type == ObjType.Return)
                {
                    if (type == ObjType.Call2) return this;
                    else throw new Exception();
                }
                else if (last.type == ObjType.Goto)
                {
                    var name = (last as Goto).value;
                    if (labelmap.ContainsKey(name))
                    {
                        i = labelmap[name].n - 1;
                        continue;
                    }
                    else return this;
                }
            }
            if (branchmap.ContainsKey("end"))
            {
                var end1 = branchmap["end"];
                for (var i = end1.n; i < children.Count; i++)
                {
                    rets.Add(children[i].exe(local));
                    var last = rets.Last();
                    if (last.type == ObjType.Break || last.type == ObjType.Continue || last.type == ObjType.Return)
                    {
                        if (type == ObjType.Call2) return this;
                        else throw new Exception();
                    }
                    else if (last.type == ObjType.Goto)
                    {
                        var name = (last as Goto).value;
                        if (labelmap.ContainsKey(name))
                        {
                            i = labelmap[name].n - 1;
                            continue;
                        }
                        else return this;
                    }
                }
            }
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (type == ObjType.Bracket)
            {
                if (val2.type == ObjType.Block)
                {
                    if (rets.Count == 1 || val2.children.Count == 0)
                    {
                        var cls = rets[0] as Type;
                        if (cls != null)
                        {
                            var typearray = new ArrType(cls);
                            return typearray;
                        }
                    }
                    else throw new Exception();
                }
                else if (val2.type == ObjType.Bracket)
                {
                    var block = val2.exe(local) as Block;
                    n++;
                    val2 = primary.children[n];
                    for (var i = 0; i < block.rets.Count; i++)
                    {
                        val2 = block.rets[i];
                        var cls = rets[i % rets.Count] as Type;
                        if (val2.type == ObjType.Word && cls != null)
                        {
                            var variable = new Variable((Type)rets[i % rets.Count]);
                            local.declare((val2 as Word).name, variable);
                            block.rets[i] = variable;
                        }
                        else throw new Exception();
                    }
                    return block;
                }
                else if (val2.type == ObjType.Word)
                {
                    var word = val2 as Word;
                    n++;
                    val2 = primary.children[n];
                    var variable = new Variable((Type)rets[0]);
                    local.declare(word.name, variable);
                    return variable;
                }
                else if (val2.type == ObjType.Left)
                {
                    n++;
                    val2 = primary.children[n];
                    if (rets.Count == 1 || val2.children.Count == 0)
                    {
                        var type = rets[0] as Type;
                        if (type != null)
                        {
                            var functype = new FuncType(type);
                            n++;
                            val2 = primary.children[n];
                            val2.exe(local).Getter(local);
                            if (val2.type == ObjType.ClassObj)
                            {
                                functype.draws.Add(val2 as ClassObj);
                                n++;
                                return functype;
                            }
                            else if (val2.type == ObjType.Generic)
                            {
                                var generic = val2 as Generic;
                                n++;
                                val2 = primary.children[n];
                                if (val2.type == ObjType.Block)
                                {
                                    val2.exe(local).Getter(local);
                                    var blk = val2 as Block;
                                    var genericobj = new GenericObj(generic);
                                    for (var i = 0; i < blk.rets.Count; i++)
                                    {
                                        val2 = blk.rets[i];
                                        if (val2.type == ObjType.ClassObj || val2.type == ObjType.GenericObj)
                                        {
                                            genericobj.draws.Add(val2 as Type);
                                        }
                                        else throw new Exception();
                                    }
                                    if (generic.vmap.Count != genericobj.draws.Count) throw new Exception();
                                    n++;
                                    functype.draws.Add(genericobj);
                                    return functype;
                                }
                                else throw new Exception();
                            }
                            else if (val2.type == ObjType.Bracket)
                            {
                                val2 = val2.exe(local).Getter(local);
                                var blk = val2 as Block;
                                for (var i = 0; i < blk.rets.Count; i++)
                                {
                                    val2 = blk.rets[i];
                                    if (val2.type == ObjType.ClassObj || val2.type == ObjType.GenericObj)
                                    {
                                        functype.draws.Add(val2 as Type);
                                    }
                                    else throw new Exception();
                                }
                                n++;
                                return functype;
                            }
                            else throw new Exception();
                        }
                    }
                }
                else if (val2.type == ObjType.Dot)
                {
                    throw new Exception();
                }
            }
            else if (type == ObjType.Block || type == ObjType.Array)
            {
                if (val2.type == ObjType.Block)
                {
                    var block = val2 as Block;
                    block.Getter(local);
                    Obj me = this;
                    for (var i = 0; i < block.rets.Count; i++)
                    {
                        if (me.type == ObjType.Block || me.type == ObjType.Array)
                        {
                            var blk = me as Block;
                            if (block.rets[i].type == ObjType.Number)
                            {
                                var num = block.rets[i] as Number;
                                me = blk.rets[num.value];
                            }
                            else if (block.rets[i].type == ObjType.StrObj)
                            {
                                var str = block.rets[i] as StrObj;
                                me = blk.rets[blk.labelmap[str.value].n];
                            }
                        }
                    }
                    n++;
                    return me;
                }
                else if (val2.type == ObjType.Dot)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.Word)
                    {
                        var word = val2 as Word;
                        n++;
                        if (word.name == "length")
                        {
                            return new Number(rets.Count) { cls = local.Int };
                        }
                    }
                }
            }
            throw new Exception();
        }
        public override Obj Self(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].Self(local);
            }
            return this;
        }
        public override Obj Getter(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].Getter(local);
            }
            return this;
        }
        public Obj Equal(String op, Local local, Obj val2)
        {
            val2 = val2.Getter(local);
            if (val2.type == ObjType.Block)
            {
                var blk = val2 as Block;
                for (var i = 0; i < rets.Count; i++)
                {
                    (rets[i] as Variable).value = blk.rets[i % blk.rets.Count];
                }
            }
            else
            {
                for (var i = 0; i < rets.Count; i++)
                {
                    (rets[i] as Variable).ope(op, local, val2);
                }
            }
            return this;
        }
        public Obj In(String op, Local local, Obj val2)
        {
            val2 = val2.Getter(local);
            if (val2.type == ObjType.Block)
            {
                for (var i = 0; i < rets.Count; i++)
                {
                    var it = new Iterator(i);
                    it.value = val2 as Block;
                    (rets[i] as Variable).value = it;
                }
                return this;
            }
            else throw new Exception();
        }
        public Obj Ope(String op, Local local, Obj val2)
        {
            if (rets.Count == 1)
            {
                return rets[0].ope(op, local, val2);
            }
            throw new Exception();
        }
        public override Obj Clone()
        {
            var block = new Block(type) { labelmap = labelmap, branchmap = branchmap, template = template, obj = obj, letter = letter, letter2 = letter2 };
            for (var i = 0; i < children.Count; i++)
            {
                block.children.Add(children[i]);
            }
            foreach (var key in vmap)
            {
                block.vmap[key.Key] = key.Value.Clone();
            }
            if (type == ObjType.Array) block.rets = rets;
            return block;
        }
        public Block Sort(Function func, Local local)
        {

            for (var i = 0; i < rets.Count; i++)
            {
                for (var j = i + 1; j < rets.Count; j++)
                {
                    local.blocks = func.blocks;
                    var block1 = func.draw.children[0].Clone() as Block;
                    local.blocks.Add(block1);
                    block1.exe(local).Getter(local);
                    var k = 0;
                    foreach (var val in block1.vmap.Values)
                    {
                        if (val.type == ObjType.Variable)
                        {
                            if (k == 0) val.ope("=", local, rets[i]);
                            else if (k == 1) val.ope("=", local, rets[j]);
                        }
                        k++;
                    }
                    var block2 = func.draw.children[1].Clone() as Block;
                    local.blocks.Add(block2);
                    block2.exe(local).Getter(local);
                    if (block2.rets.Last().type == ObjType.BoolVal)
                    {
                        if (!(block2.rets.Last() as BoolVal).value)
                        {
                            var gv = rets[i];
                            rets[i] = rets[j];
                            rets[j] = gv;
                        }
                    }
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blockslist.RemoveAt(local.blockslist.Count - 1);
                }
            }
            return this;
        }
    }
    class Operator : Obj
    {
        public string name;
        public Operator(string name) : base(ObjType.Operator)
        {
            this.name = name;
        }
        public Operator(Letter letter) : this(letter.name)
        {
            this.letter = letter;
        }
        public override Obj exe(Local local)
        {
            if (name == "=" || name == ":" || name == "in")
            {
                var val1 = children[0].exe(local).Self(local);
                if (val1.type == ObjType.Continue || val1.type == ObjType.Break || val1.type == ObjType.Return || val1.type == ObjType.Goto) return val1;
                var val2 = children[1].exe(local).Getter(local);
                if (val2.type == ObjType.Continue || val2.type == ObjType.Break || val2.type == ObjType.Return || val2.type == ObjType.Goto) return val2;
                return val1.ope(name, local, val2);
            }
            else
            {
                var val1 = children[0].exe(local).Getter(local);
                if (val1.type == ObjType.Continue || val1.type == ObjType.Break || val1.type == ObjType.Return || val1.type == ObjType.Goto) return val1;
                var val2 = children[1].exe(local).Getter(local);
                if (val2.type == ObjType.Continue || val2.type == ObjType.Break || val2.type == ObjType.Return || val2.type == ObjType.Goto) return val2;
                return val1.ope(name, local, val2);
            }
            throw new Exception();
        }
    }
    class SingleOp : Obj
    {
        public string name;
        public SingleOp(string name) : base(ObjType.SingleOp)
        {
            this.name = name;
        }
        public SingleOp(Letter letter) : this(letter.name)
        {
            this.letter = letter;
        }
    }
    class PrimOp : Obj
    {
        public string name;
        public PrimOp(Letter letter, ObjType type) : base(type)
        {
            this.letter = letter;
            this.name = letter.name;
        }
    }
    class Word : Obj
    {
        public string name;
        public Word(string name) : base(ObjType.Word)
        {
            this.name = name;
        }
        public Word(Letter letter) : this(letter.name)
        {
            this.letter = letter;
        }
        public override Obj Self(Local local)
        {
            return local.get(name);
        }
        public override Obj Getter(Local local)
        {
            return local.get(name).Getter(local);
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
        public Obj Change(Local local)
        {
            switch (name)
            {
                case "class":
                    return new Class();
                case "var":
                    return new Var();
                case "if":
                    return new If();
                case "elif":
                    return new Obj(ObjType.Elif);
                case "else":
                    return new Obj(ObjType.Else);
                case "while":
                    return new While();
                case "label":
                    return new GeneLabel();
                case "for":
                    return new For();
                case "switch":
                    return new Switch();
                case "return":
                    return new Return();
                case "goto":
                    return new Goto();
                case "continue":
                    return new Continue();
                case "client":
                    return new ServerClient("client");
                case "server":
                    return new ServerClient("server");
                case "signal":
                    return new Signal();
                case "print":
                    return new Print();
                case "true":
                    return new BoolVal(true) { cls = local.Bool };
                case "false":
                    return new BoolVal(false) { cls = local.Bool };
                case "gene":
                    return local.gene;
                default:
                    return this;
            }
        }
    }
    class Val : Obj
    {
        public Type cls;
        public Dictionary<String, Obj> vmap;
        public Val(ObjType type) : base(type)
        {
        }
    }
    class Number : Val
    {
        public int value;
        public Number(int value) : base(ObjType.Number)
        {
            this.value = value;
            opes["+"] = Plus;
            opes["-"] = Minus;
            opes["*"] = Mul;
            opes["/"] = Div;
            opes[">"] = MoreThan;
            opes["<"] = LessThan;
            opes[">="] = MoreEqual;
            opes["<="] = LessEqual;
            opes["!="] = NotEqual;
            opes["=="] = EqualEqual;
        }
        public Number(Letter letter) : this(Convert.ToInt32(letter.name))
        {
            this.letter = letter;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public Obj Plus(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new Number(value + (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value + (val2 as FloatVal).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.StrObj)
            {
                return new StrObj(value + (val2 as StrObj).value) { cls = local.Str };
            }
            throw new Exception();
        }
        public Obj Minus(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new Number(value - (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value - (val2 as FloatVal).value) { cls = local.Float };
            }
            throw new Exception();
        }
        public Obj Mul(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new Number(value * (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value * (val2 as FloatVal).value) { cls = local.Float };
            }
            throw new Exception();
        }
        public Obj Div(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new Number(value / (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value / (val2 as FloatVal).value) { cls = local.Float };
            }
            throw new Exception();
        }
        public Obj MoreThan(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value > (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value > (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj LessThan(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value < (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value < (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj MoreEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value >= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value >= (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj LessEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value <= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value <= (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj NotEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value != (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value != (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj EqualEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value == (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value == (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
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
                    if (word.name == "random")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            n++;
                            if (val2.children.Count != 0) throw new Exception();
                            return new Number(new Random().Next(value)) { cls = local.Int };
                        }
                        else throw new Exception();
                    }
                }
            }
            throw new Exception();
        }
    }
    class FloatVal : Val
    {
        public float value;
        public FloatVal(float value) : base(ObjType.FloatVal)
        {
            this.value = value;
            opes["+"] = Plus;
            opes["-"] = Minus;
            opes["*"] = Mul;
            opes["/"] = Div;
            opes[">"] = MoreThan;
            opes["<"] = LessThan;
            opes[">="] = MoreEqual;
            opes["<="] = LessEqual;
            opes["!="] = NotEqual;
            opes["=="] = EqualEqual;
        }
        public FloatVal(Letter letter) : this((float)Convert.ToDouble(letter.name))
        {
            this.letter = letter;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public Obj Plus(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value + (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value + (val2 as FloatVal).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.StrObj)
            {
                return new StrObj(value + (val2 as StrObj).value) { cls = local.Str };
            }
            throw new Exception();
        }
        public Obj Minus(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value - (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value - (val2 as FloatVal).value) { cls = local.Float };
            }
            throw new Exception();
        }
        public Obj Mul(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value * (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value * (val2 as FloatVal).value) { cls = local.Float };
            }
            throw new Exception();
        }
        public Obj Div(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value / (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value / (val2 as FloatVal).value) { cls = local.Float };
            }
            throw new Exception();
        }
        public Obj MoreThan(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value > (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value > (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj LessThan(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value < (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value < (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj MoreEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value >= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value >= (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj LessEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value <= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value <= (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj NotEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value != (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value != (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public Obj EqualEqual(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value == (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value == (val2 as FloatVal).value) { cls = local.Bool };
            }
            throw new Exception();
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            throw new Exception();
        }
    }
    class StrObj : Val
    {
        public string value;
        public StrObj(string value) : base(ObjType.StrObj)
        {
            this.value = value;
        }
        public StrObj(Letter letter) : base(ObjType.StrObj)
        {
            this.letter = letter;
            value = letter.name;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            throw new Exception();
        }
    }
    class BoolVal : Val
    {
        public bool value;
        public BoolVal(bool value) : base(ObjType.BoolVal)
        {
            this.value = value;
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            throw new Exception();
        }
    }
    class VoiVal : Obj
    {
        public VoiVal() : base(ObjType.VoiVal)
        {
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            throw new Exception();
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            throw new Exception();
        }
    }
    class Null : Val
    {
        public Null() : base(ObjType.Null)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            throw new Exception();
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            throw new Exception();
        }
    }
    class Class : Obj
    {
        public Class() : base(ObjType.Class)
        {
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            var blk1 = new Block(ObjType.Call1);
            local.blocks.Add(blk1);
            Obj obj = new ClassObj();
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Block)
                {
                    var generic = new Generic();
                    generic.block = blk1;
                    var block = val2.exe(local) as Block;
                    for (var i = 0; i < block.rets.Count; i++)
                    {
                        val2 = block.rets[i];
                        if (val2.type == ObjType.Word)
                        {
                            generic.vmap[(val2 as Word).name] = new VariClass(i);
                        }
                    }
                    n++;
                    val2 = primary.children[n];
                    obj = generic;
                }
                local.declare(word.name, obj);
            }
        head:
            if (val2.type == ObjType.Left)
            {
                n++;
                val2 = primary.children[n].exe(local).Getter(local);
                if (val2.type == ObjType.ClassObj)
                {
                    var cls = val2 as ClassObj;
                    if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(cls);
                    else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(cls);
                    n++;
                    val2 = primary.children[n];
                    goto head;
                }
                else if (val2.type == ObjType.Generic)
                {
                    var generic = val2 as Generic;
                    n++;
                    val2 = primary.children[n].exe(local).Getter(local);
                    if (val2.type == ObjType.Block)
                    {
                        var block = val2 as Block;
                        var genericobj = new GenericObj(generic);
                        for (var i = 0; i < block.rets.Count; i++)
                        {
                            val2 = block.rets[i];
                            if (val2.type == ObjType.ClassObj || val2.type == ObjType.GenericObj)
                            {
                                genericobj.draws.Add(val2 as Type);
                            }
                        }
                        n++;
                        val2 = primary.children[n];
                        if (generic.vmap.Count != genericobj.draws.Count) throw new Exception();
                        if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(genericobj);
                        else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(genericobj);
                        goto head;
                    }
                }
                else throw new Exception();
            }
            if (val2.type == ObjType.CallBlock)
            {
                if (obj.type == ObjType.ClassObj) (obj as ClassObj).draw = val2 as CallBlock;
                else if (obj.type == ObjType.Generic) (obj as Generic).draw = val2 as CallBlock;
                n++;
                local.blocks.RemoveAt(local.blocks.Count - 1);
                return obj;
            }
            else throw new Exception();
        }
    }
    class Constructor : Obj
    {
        ClassObj cls;
        public Constructor(ClassObj ret) : base(ObjType.Constructor)
        {
            cls = ret;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            val2 = val2.Clone().exe(local).Getter(local);
            if (val2.type == ObjType.Bracket)
            {
                var blk = val2 as Block;
                local.blocks = cls.blocks;
                var block1 = cls.draw.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exeRange(0, block1.children.Count, local, false);
                var varr = new List<Obj>(block1.vmap.Values);
                if (varr.Count != blk.rets.Count) throw new Exception();
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opes["="]("=", local, blk.rets[i]);
                }
                var block2 = cls.draw.children[1].Clone() as Block;
                var value = new Value(cls, block2);
                block1.vmap["base"] = new Base(value, cls.extends);
                local.blocks.Add(block2);
                if (cls.type == ObjType.ModelObj)
                {
                    if (block1.rets.Count > 0) block2.vmap["id"] = new Variable(local.Int) { value = (Number)block1.rets.Last() };
                    else block2.vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
                }
                block2.exe(local);
                local.blocks.RemoveRange(local.blocks.Count - 2, 2);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                var retobj = block2.rets.Last();
                if (retobj.type == ObjType.Break || retobj.type == ObjType.Continue || retobj.type == ObjType.Goto || retobj.type == ObjType.Return) throw new Exception();

                n++;
                if (cls.type == ObjType.ModelObj) return new ModelVal(cls as ModelObj, block2);
                return new Value(cls, block2);
            }
            else if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {

                local.blocks = cls.blocks;
                var block1 = cls.draw.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exeRange(0, block1.children.Count, local, false);
                var varr = new List<Obj>(block1.vmap.Values);
                if (varr.Count != 1) throw new Exception();
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opes["="]("=", local, val2);
                }
                var block2 = cls.draw.children[1].Clone() as Block;
                var value = new Value(cls, block2);
                block1.vmap["base"] = new Base(value, cls.extends);
                local.blocks.Add(block2);
                block2.exe(local);
                local.blocks.RemoveRange(local.blocks.Count - 2, 2);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                var retobj = block2.rets.Last();
                if (retobj.type == ObjType.Break || retobj.type == ObjType.Continue || retobj.type == ObjType.Goto || retobj.type == ObjType.Return) throw new Exception();
                if (!TypeCheck.Check(this, retobj, CheckType.Finish, local)) throw new Exception();
                n++;
                return value;
            }
            throw new Exception();
        }
    }
    class Base : Obj
    {
        public Value value;
        public List<Type> extends;
        public Base(Value value, List<Type> extends) : base(ObjType.Base)
        {
            this.value = value;
            this.extends = extends;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            var m = 0;
            if (val2.type == ObjType.Block)
            {
                if (val2.children.Count == 1 && val2.children[0].type == ObjType.Number)
                {
                    m = (val2.children[0] as Number).value;
                }
                n++;
                val2 = primary.children[n];
            }
            var cls = extends[m] as ClassObj;
            val2 = val2.Clone().exe(local).Getter(local);
            if (val2.type == ObjType.Bracket)
            {
                var blk = val2 as Block;
                var block1 = cls.draw.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exeRange(0, block1.children.Count, local, false);
                block1.vmap["base"] = new Base(value, cls.extends);
                var varr = new List<Obj>(block1.vmap.Values);
                if (varr.Count != blk.rets.Count) throw new Exception();
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opes["="]("=", local, blk.rets[i]);
                }
                var block2 = cls.draw.children[1].Clone() as Block;
                local.blocks.Add(block2);
                block2.exe(local);
                local.blocks.RemoveRange(local.blocks.Count - 2, 2);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                var retobj = block2.rets.Last();
                if (retobj.type == ObjType.Break || retobj.type == ObjType.Continue || retobj.type == ObjType.Goto || retobj.type == ObjType.Return) throw new Exception();

                n++;
                foreach (var kv in block2.vmap)
                {
                    value.vmap[kv.Key] = kv.Value;
                }
                return new VoiVal();
            }
            else if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {
                var block1 = cls.draw.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exeRange(0, block1.children.Count, local, false);
                block1.vmap["base"] = new Base(value, cls.extends);
                var varr = new List<Obj>(block1.vmap.Values);
                if (varr.Count != 1) throw new Exception();
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opes["="]("=", local, val2);
                }
                var block2 = cls.draw.children[1].Clone() as Block;
                local.blocks.Add(block2);
                block2.exe(local);
                local.blocks.RemoveRange(local.blocks.Count - 2, 2);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                var retobj = block2.rets.Last();
                if (retobj.type == ObjType.Break || retobj.type == ObjType.Continue || retobj.type == ObjType.Goto || retobj.type == ObjType.Return) throw new Exception();
                if (!TypeCheck.Check(this, retobj, CheckType.Finish, local)) throw new Exception();
                n++;
                foreach (var kv in block2.vmap)
                {
                    value.vmap[kv.Key] = kv.Value;
                }
                return new VoiVal();
            }
            throw new Exception();
        }
    }
    class Type : Obj
    {
        public Type cls;
        public Type(ObjType type) : base(type)
        {
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            Type type = this;
        head:
            if (val2.type == ObjType.Block)
            {
                var blk = val2 as Block;
                if (blk.children.Count == 0)
                {
                    type = new ArrType(type);
                    n++;
                    val2 = primary.children[n];
                    goto head;
                }
                else throw new Exception();
            }
            else if (val2.type == ObjType.Left)
            {
                var functype = new FuncType(type);
                type = functype;
                n++;
                val2 = primary.children[n];
                val2.exe(local).Getter(local);
                if (val2.type == ObjType.ClassObj)
                {
                    functype.draws.Add(val2 as ClassObj);
                    n++;
                    val2 = primary.children[n];
                    goto head;
                }
                else if (val2.type == ObjType.Generic)
                {
                    var generic = val2 as Generic;
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.Block)
                    {
                        val2.exe(local).Getter(local);
                        var blk = val2 as Block;
                        var genericobj = new GenericObj(generic);
                        for (var i = 0; i < blk.rets.Count; i++)
                        {
                            val2 = blk.rets[i];
                            if (val2.type == ObjType.ClassObj || val2.type == ObjType.GenericObj)
                            {
                                genericobj.draws.Add(val2 as Type);
                            }
                            else throw new Exception();
                        }
                        if (generic.vmap.Count != genericobj.draws.Count) throw new Exception();
                        n++;
                        val2 = primary.children[n];
                        functype.draws.Add(genericobj);
                        goto head;
                    }
                    else throw new Exception();
                }
                else if (val2.type == ObjType.Bracket)
                {
                    val2 = val2.exe(local).Getter(local);
                    var blk = val2 as Block;
                    for (var i = 0; i < blk.rets.Count; i++)
                    {
                        val2 = blk.rets[i];
                        if (val2.type == ObjType.ClassObj || val2.type == ObjType.GenericObj)
                        {
                            functype.draws.Add(val2 as Type);
                        }
                        else throw new Exception();
                    }
                    n++;
                    val2 = primary.children[n];
                }
                else throw new Exception();
            }
            Word word = null;
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word2 = val2 as Word;
                    if (type.type == ObjType.ClassObj || type.type == ObjType.ArrayType)
                    {
                        if (word2.name == "new") return new Constructor(type as ClassObj);
                    }
                    else if (type.type == ObjType.ModelObj)
                    {
                        if (word2.name == "new") return new Constructor(type as ClassObj);
                    }
                    else throw new Exception();
                }
                else throw new Exception();
            }
            else if (val2.type == ObjType.Dolor)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word2 = val2 as Word;
                    n++;
                    if (local.comments.Count > 0)
                    {
                        var letter = local.comments.Last().ValueAdd(word.name);
                        var variable = new Variable(type) { comment = letter };
                        return type;
                    }
                    else throw new Exception();
                }
            }
            else if (val2.type == ObjType.Word)
            {
                word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var last = local.blocks.Last().obj;
                if (last.type == ObjType.ModelObj || last.type == ObjType.GeneObj)
                {
                    var obj = type;
                head2:
                    if (obj.type == ObjType.ClassObj || obj.type == ObjType.FuncType) throw new Exception();
                    else if (obj.type == ObjType.ArrayType)
                    {
                        obj = (obj as ArrType).cls;
                        goto head2;
                    }
                }
            }
            else if (val2.type == ObjType.Bracket)
            {
                val2.exe(local);
                var blk = val2 as Block;
                n++;

                for (var i = 0; i < blk.rets.Count; i++)
                {
                    val2 = blk.rets[i];
                    if (val2.type == ObjType.Word)
                    {
                        var variable = new Variable(type);
                        local.declare((val2 as Word).name, variable);
                    }
                }
                return blk;
            }
            if (val2.type == ObjType.Block)
            {
                var func = new GenericFunction(type);
                var blk = val2.exe(local) as Block;
                for (var i = 0; i < blk.rets.Count; i++)
                {
                    if (blk.rets[i].type == ObjType.Word)
                    {
                        var word2 = blk.rets[i] as Word;
                        func.vmap[word2.name] = new VariClass(i);
                    }
                }
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    func.draw = val2 as CallBlock;
                    foreach (var b in local.blocks) func.blocks.Add(b);
                    n++;
                    if (word != null) local.declare(word.name, func);
                    return func;
                }
                else throw new Exception();
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var func = new Function(type);
                func.draw = val2 as CallBlock;
                foreach (var b in local.blocks) func.blocks.Add(b);
                n++;
                if (word != null) local.declare(word.name, func);
                return func;
            }
            else
            {
                if (word == null) return type;
                else
                {
                    var variable = new Variable(type);
                    local.declare(word.name, variable);
                head1:
                    if (val2.type == ObjType.Left)
                    {
                        var func = new Function(this);
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var func2 = val2.Getter(local) as Function;
                            n++;
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Block)
                            {
                                n++;
                                val2 = primary.children[n];
                            }
                            if (val2.type == ObjType.Bracket)
                            {
                                n++;
                                val2 = primary.children[n];
                            }
                            variable.left = func2;
                            goto head1;
                        }
                        else
                        {
                            if (val2.type == ObjType.CallBlock)
                            {
                                func.draw = val2 as CallBlock;
                                n++;
                                val2 = primary.children[n];
                                variable.left = func;
                                goto head1;
                            }
                            else throw new Exception();
                        }
                    }
                    else if (val2.type == ObjType.Right)
                    {
                        var func = new Function(this);
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var func2 = val2.Getter(local) as Function;
                            n++;
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Block)
                            {
                                func.block = val2 as Block;
                                n++;
                                val2 = primary.children[n];
                            }
                            if (val2.type == ObjType.Bracket)
                            {
                                func.bracket = val2 as Block;
                                n++;
                                val2 = primary.children[n];
                            }
                            variable.right = func2;
                            goto head1;
                        }
                        else
                        {
                            if (val2.type == ObjType.CallBlock)
                            {
                                func.draw = val2 as CallBlock;
                                n++;
                                val2 = primary.children[n];
                                variable.right = func;
                                goto head1;
                            }
                            else throw new Exception();
                        }
                    }
                    return variable;
                }
            }
        }
    }
    class ArrType : Type
    {
        int array = 0;
        public ArrType(Type cls) : base(ObjType.ArrayType)
        {
            this.cls = cls;
        }
    }
    class FuncType : Type
    {
        public List<Type> draws = new List<Type>();
        public FuncType(Type cls) : base(ObjType.FuncType)
        {
            this.cls = cls;
        }
    }
    class VariClass : Type
    {
        public int n;
        public VariClass(int n) : base(ObjType.VariClass)
        {
            this.n = n;
        }
    }
    class Generic : Obj
    {
        public CallBlock draw;
        public Block block;
        public Dictionary<String, Type> vmap = new Dictionary<string, Type>();
        public List<Type> extends = new List<Type>();
        public Generic() : base(ObjType.Generic)
        {
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                var block = val2.Clone().exe(local).Getter(local) as Block;
                var geneobj = new GenericObj(this);
                if (this.block.vmap.Count != block.rets.Count) throw new Exception();
                for (var i = 0; i < block.rets.Count; i++)
                {
                    geneobj.draws.Add(block.rets[i] as Type);
                }
                var list = new List<Obj>(this.block.vmap.Values);
            }
            throw new Exception();
        }
    }
    class GenericObj : Type
    {
        public List<Type> extends = new List<Type>();
        public Generic gene;
        public List<Type> draws = new List<Type>();
        public void SetExtends()
        {

            for (var i = 0; i < gene.extends.Count; i++)
            {
                if (extends[i].type == ObjType.ClassObj)
                {
                    extends.Add(extends[i]);
                }
                else if (extends[i].type == ObjType.GenericObj)
                {
                    var geneobj2 = extends[i].Clone() as GenericObj;
                    for (var j = 0; j < geneobj2.draws.Count; j++)
                    {
                        if (geneobj2.draws[j].type == ObjType.VariClass)
                        {
                            geneobj2.draws[j] = draws[(geneobj2.draws[j] as VariClass).n];
                        }
                    }
                    geneobj2.SetExtends();
                    extends.Add(geneobj2);
                }
            }
        }
        public GenericObj(Generic gene) : base(ObjType.GenericObj)
        {
            this.gene = gene;
        }
        public override Obj Clone()
        {
            var gj = new GenericObj(gene) { letter = letter };
            for (var i = 0; i < draws.Count; i++) gj.draws.Add(draws[i]);
            return gj;
        }
    }
    class ClassObj : Type
    {
        public List<Block> blocks = new List<Block>();
        public CallBlock draw;
        public Type ret;
        public List<Type> extends = new List<Type>();
        public ClassObj() : base(ObjType.ClassObj)
        {

        }
    }
    class Var : Type
    {
        public List<Var> connects = new List<Var>();
        public Var() : base(ObjType.Var)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class Variable : Obj
    {
        public Obj value;
        public Type cls;
        public Obj left;
        public Obj right;
        public Letter comment;
        public Variable(Type cls) : base(ObjType.Variable)
        {
            this.cls = cls;
            opes["="] = opes[":"] = Equal;
            opes["in"] = In;
            opes["+"] = opes["-"] = opes["*"] = opes["/"] = opes["=="] = opes["!="] = Ope;
            opes["<"] = opes[">"] = opes["<="] = opes[">="] = Ope;
        }
        public override Obj Getter(Local local)
        {
            if (value != null) return value.Getter(local);
            else return new Null();
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            return value.Primary(ref n, local, primary, val2);
        }
        public Obj Ope(String op, Local local, Obj val2)
        {
            return value.opes[op](op, local, val2);
        }
        public Obj In(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                var it = new Iterator(-1);
                it.value = val2.Getter(local) as Block;
                value = it;
                return it;
            }
            else throw new Exception();
        }
        public Obj Equal(String op, Local local, Obj val2)
        {
            if (value != null && value.type == ObjType.Iterator) throw new Exception();
            if (!TypeCheck.Check(this, val2, CheckType.Setter, local)) throw new Exception();
            value = val2;
            if (comment != null)
            {
                if (value.type == ObjType.Number) comment.text = comment.name = (value as Number).value.ToString();
                else if (value.type == ObjType.FloatVal) comment.text = comment.name = (value as FloatVal).value.ToString();
                else if (value.type == ObjType.StrObj) comment.text = comment.name = (value as StrObj).value;
            }
            return this;
        }
        public override Obj Clone()
        {
            return new Variable(cls) { letter = letter};
        }
    }
    class Iterator : Obj
    {
        public Block value;
        public int m;
        public int n = 0;
        public Iterator(int m) : base(ObjType.Iterator)
        {
            this.m = m;
        }
        public override Obj Getter(Local local)
        {
            if (m == -1) return value.rets[n];
            else return (value.rets[n] as Block).rets[m];
        }
    }
    class Value : Val
    {
        public Value(Type cls, Block value) : base(ObjType.Value)
        {
            this.cls = cls;
            this.vmap = value.vmap;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    n++;
                    var name = (val2 as Word).name;
                    if (vmap.ContainsKey(name))
                    {
                        return vmap[name];
                    }
                }
            }
            throw new Exception();
        }
    }
    class GenericFunction : Function
    {
        public Dictionary<String, Type> vmap = new Dictionary<String, Type>();
        public GenericFunction(Type ret) : base(ret)
        {
            this.type = ObjType.GenericFunction;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                var blk = val2.Getter(local) as Block;
                if (blk.rets.Count != vmap.Count) throw new Exception();
                for (var i = 0; i < blk.rets.Count; i++)
                {
                    if (blk.rets[i] as Type == null) throw new Exception();
                }
                n++;
                val2 = primary.children[n];
                return new Function(ret) { blocks = blocks, draw = draw, bracket = bracket, block = blk };
            }
            return base.Primary(ref n, local, primary, val2);
        }
    }
    class Function : Obj
    {
        public List<Block> blocks = new List<Block>();
        public CallBlock draw;
        public Type ret;
        public Block bracket;
        public Block block;
        public Function(Type ret) : base(ObjType.Function)
        {
            this.ret = ret;
        }
        public override Obj exe(Local local)
        {
            return base.exe(local);
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            val2 = val2.Getter(local);
            if (bracket != null) throw new Exception();
            if (val2.type == ObjType.Bracket)
            {
                var blk = val2 as Block;
                local.blocks = blocks;
                var block1 = draw.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exe(local);
                var varr = new List<Obj>(block1.vmap.Values);
                if (varr.Count != blk.rets.Count) throw new Exception();
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opes["="]("=", local, blk.rets[i]);
                }
                var block2 = draw.children[1].Clone() as Block;
                local.blocks.Add(block2);
                block2.exe(local);
                local.blocks.RemoveRange(local.blocks.Count - 2, 2);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                var retobj = block2.rets.Last();
                if (retobj.type == ObjType.Break || retobj.type == ObjType.Continue || retobj.type == ObjType.Goto) throw new Exception();
                else if (retobj.type == ObjType.Return)
                {
                    retobj = (retobj as Return).value;
                    if (retobj == null) retobj = new VoiVal();
                }
                if (!TypeCheck.Check(this, retobj, CheckType.Finish, local)) throw new Exception();
                n++;
                return block2.rets.Last().Getter(local);
            }
            else if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {

                local.blocks = blocks;
                var block1 = draw.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exe(local);
                var varr = new List<Obj>(block1.vmap.Values);
                if (varr.Count != 1) throw new Exception();
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opes["="]("=", local, val2);
                }
                var block2 = draw.children[1].Clone() as Block;
                local.blocks.Add(block2);
                block2.exe(local);
                local.blocks.RemoveRange(local.blocks.Count - 2, 2);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                var retobj = block2.rets.Last();
                if (retobj.type == ObjType.Break || retobj.type == ObjType.Continue || retobj.type == ObjType.Goto) throw new Exception();
                else if (retobj.type == ObjType.Return)
                {
                    retobj = (retobj as Return).value;
                    if (retobj == null) retobj = new VoiVal();
                }
                if (!TypeCheck.Check(this, retobj, CheckType.Finish, local)) throw new Exception();
                n++;
                return block2.rets.Last().Getter(local);
            }
            throw new Exception();
        }
    }
    class If : Obj
    {
        public If() : base(ObjType.If) { }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var block1 = val2.children[0].Clone() as Block;
                local.blocks.Add(block1);
                block1.exeRange(0, block1.children.Count, local, false);
                var ret = block1.rets.Last().Getter(local);
                var result = false;
                Obj retobj = null;
                if (ret.type == ObjType.Number)
                {
                    var num = ret as Number;
                    if (num.value != 0) result = true;
                }
                else if (ret.type == ObjType.BoolVal)
                {
                    result = (ret as BoolVal).value;
                }
                if (result)
                {
                    var block2 = val2.children[1].Clone() as Block;
                    local.blocks.Add(block2);
                    block2.exe(local);
                    retobj = block2.rets.Last().Getter(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                }
                else retobj = new VoiVal();
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                val2 = primary.children[n];
            head:
                if (val2.type == ObjType.Elif)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        if (!result)
                        {
                            block1 = val2.children[0].Clone() as Block;
                            local.blocks.Add(block1);
                            block1.exe(local);
                            ret = block1.rets.Last().Getter(local);
                            if (ret.type == ObjType.Number)
                            {
                                var num = ret as Number;
                                if (num.value != 0) result = true;
                            }
                            else if (ret.type == ObjType.BoolVal)
                            {
                                result = (ret as BoolVal).value;
                            }
                            if (result)
                            {
                                var block2 = val2.children[1].Clone() as Block;
                                local.blocks.Add(block2);
                                block2.exe(local);
                                retobj = block2.rets.Last().Getter(local);
                                local.blocks.RemoveAt(local.blocks.Count - 1);
                            }
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                        }
                        n++;
                        val2 = primary.children[n];
                        goto head;
                    }
                    else throw new Exception();

                }
                else if (val2.type == ObjType.Else)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        if (!result)
                        {
                            block1 = val2.children[0].Clone() as Block;
                            local.blocks.Add(block1);
                            block1.exe(local);
                            var block2 = val2.children[1].Clone() as Block;
                            local.blocks.Add(block2);
                            block2.exe(local);
                            retobj = block2.rets.Last().Getter(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                        }
                        n++;
                        return retobj;
                    }
                    else throw new Exception();
                }
                else return retobj;
            }
            throw new Exception();
        }
    }
    class While : Obj
    {
        public While() : base(ObjType.While) { }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var block1 = val2.children[0] as Block;
                local.blocks.Add(block1);
                var loop = false;
                var result = false;
                block1.exeRange(0, block1.children.Count, local, false);
                var ret = block1.rets.Last().Getter(local);
                Obj retobj = null;
                var count = 0;
            head:
                foreach (var val in block1.vmap.Values)
                {
                    if (val.type == ObjType.Variable)
                    {
                        var variable = val as Variable;
                        if (variable.value != null && variable.value.type == ObjType.Iterator)
                        {
                            var it = variable.value as Iterator;
                            it.n = count;
                            if (it.n >= it.value.rets.Count)
                            {
                                n++;
                                return retobj;
                            }
                        }
                    }
                }
                if (loop) ret = block1.children.Last().exe(local).Getter(local);
                if (ret.type == ObjType.Number)
                {
                    var num = ret as Number;
                    if (num.value != 0) result = true;
                }
                else if (ret.type == ObjType.BoolVal)
                {
                    result = (ret as BoolVal).value;
                }
                if (result)
                {
                    var block2 = val2.children[1] as Block;
                    local.blocks.Add(block2);
                    block2.exe(local);
                    retobj = block2.rets.Last().Getter(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    if (retobj.type == ObjType.Break)
                    {
                        retobj = (retobj as Break).value;
                        if (retobj == null) retobj = new VoiVal();
                    }
                    else
                    {
                        loop = true;
                        count++;
                        goto head;
                    }
                }
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                return retobj;
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class For : Obj
    {
        public For() : base(ObjType.For) { }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var block1 = val2.children[0] as Block;
                local.blocks.Add(block1);
                var loop = false;
                var result = false;
                block1.exeRange(0, block1.children.Count - 1, local, false);
                var ret = block1.rets.Last().Getter(local);
                Obj retobj = null;
                var count = 0;
            head:
                foreach (var val in block1.vmap.Values)
                {
                    if (val.type == ObjType.Variable)
                    {
                        var variable = val as Variable;
                        if (variable.value.type == ObjType.Iterator)
                        {
                            var it = variable.value as Iterator;
                            it.n = count;
                            if (it.n >= it.value.rets.Count)
                            {
                                n++;
                                return retobj;
                            }
                        }
                    }
                }
                if (loop)
                {
                    block1.children.Last().exe(local).Getter(local);
                    ret = block1.children[block1.children.Count - 2].exe(local).Getter(local);
                }
                if (ret.type == ObjType.Number)
                {
                    var num = ret as Number;
                    if (num.value != 0) result = true;
                }
                else if (ret.type == ObjType.BoolVal)
                {
                    result = (ret as BoolVal).value;
                }
                if (result)
                {
                    var block2 = val2.children[1] as Block;
                    local.blocks.Add(block2);
                    block2.exe(local);
                    retobj = block2.rets.Last().Getter(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    if (retobj.type == ObjType.Break)
                    {
                        retobj = (retobj as Break).value;
                        if (retobj == null) retobj = new VoiVal();
                    }
                    else
                    {
                        loop = true;
                        count++;
                        goto head;
                    }
                }
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                return retobj;
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class Switch : Obj
    {
        public Switch() : base(ObjType.Switch) { }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var block1 = val2.children[0] as Block;
                local.blocks.Add(block1);
                block1.exeRange(0, block1.children.Count, local, false);
                var ret = block1.rets.Last().Getter(local);
                var block2 = val2.children[1] as Block;
                local.blocks.Add(block2);
                var str = "";
                if (ret.type == ObjType.Number)
                {
                    str = (ret as Number).value.ToString();
                }
                else if (ret.type == ObjType.StrObj)
                {
                    str = (ret as StrObj).value;
                }
                var label = block2.labelmap[str];
                var labelist = new List<Label>(block2.labelmap.Values);
                var last = block2.children.Count;
                for (var i = 0; i < labelist.Count - 1; i++)
                {
                    if (labelist[i] == label) last = labelist[i + 1].n;
                }
                Obj retobj = null;
                for (var i = label.n; i < last; i++)
                {
                    block2.rets.Add(retobj = block2.children[i].exe(local));
                    if (retobj.type == ObjType.Continue || retobj.type == ObjType.Return || retobj.type == ObjType.Goto) break;
                    else if (retobj.type == ObjType.Break)
                    {
                        retobj = (retobj as Break).value;
                        break;
                    }
                }
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                return retobj;
            }
            throw new Exception();
        }
    }
    class Break : Obj
    {
        public Obj value;
        public Break() : base(ObjType.Break)
        {
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
        head:
            val2 = val2.exe(local).Getter(local);
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block)
            {
                n++;
                value = val2;
                return this;
            }
            else if (val2.type == ObjType.Bracket)
            {
                var block = val2 as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else throw new Exception();
            }
            throw new Exception();
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
        head:
            val2 = val2.Clone().Getter(local);
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block)
            {
                n++;
                value = val2;
                return this;
            }
            else if (val2.type == ObjType.Bracket)
            {
                var block = val2 as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else throw new Exception();
            }
            throw new Exception();
        }
    }
    class Continue : Obj
    {
        public Continue() : base(ObjType.Continue)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class Return : Obj
    {
        public Obj value;
        public Return() : base(ObjType.Return)
        {
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            var val2 = primary.children[n + 1].Clone().exe(local).Getter(local);
        head:
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block || val2.type == ObjType.GeneValue || val2.type == ObjType.Comment)
            {
                n++;
                value = val2;
                return this;
            }
            else if (val2.type == ObjType.Bracket)
            {
                n++;
                var block = val2 as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else throw new Exception();
            }
            else return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            val2 = val2.Clone().Getter(local);
        head:
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block || val2.type == ObjType.GeneValue || val2.type == ObjType.Comment)
            {
                n++;
                value = val2;
                return this;
            }
            else if (val2.type == ObjType.Bracket)
            {
                var block = val2 as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else throw new Exception();
            }
            throw new Exception();
        }
    }
    class Goto : Obj
    {
        public String value;
        public Goto() : base(ObjType.Goto)
        {
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                value = (val2 as Number).value.ToString();
                return this;
            }
            else if (val2.type == ObjType.StrObj)
            {
                value = (val2 as StrObj).value;
                return this;
            }
            else if (val2.type == ObjType.Word)
            {
                value = (val2 as Word).name;
                return this;
            }
            throw new Exception();
        }
    }
    class Print : Obj
    {
        public Print() : base(ObjType.Print)
        {

        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            val2 = val2.Clone().exe(local).Getter(local);
            if (val2.type == ObjType.Number)
            {
                local.console.Text += (val2 as Number).value.ToString() + "\n";
                n++;
                return new VoiVal();
            }
            else if (val2.type == ObjType.StrObj)
            {
                local.console.Text += (val2 as StrObj).value + "\n";
                n++;
                return new VoiVal();
            }
            else if (val2.type == ObjType.Bracket || val2.type == ObjType.Block)
            {
                var block = val2 as Block;
                if (val2.type == ObjType.Bracket) local.console.Text += "(";
                else local.console.Text += "[";
                for (var i = 0; i < block.rets.Count; i++)
                {
                    if (i != 0) local.console.Text += ",";
                    var val3 = block.rets[i];
                    if (val3.type == ObjType.Number)
                    {
                        local.console.Text += (val3 as Number).value.ToString();
                    }
                    else if (val3.type == ObjType.StrObj)
                    {
                        local.console.Text += (val3 as StrObj).value;
                    }
                }
                if (val2.type == ObjType.Bracket) local.console.Text += ")\n";
                else local.console.Text += "]\n";
                n++;
                return new VoiVal();
            }
            return base.Primary(ref n, local, primary, val2);
        }
    }
    class Model : Obj
    {
        public Dictionary<String, Obj> vmap = new Dictionary<string, Obj>();
        public Model() : base(ObjType.Model)
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
                var gj = new ModelObj();
                local.declare(word.name, gj);
                if (val2.type == ObjType.CallBlock)
                {
                    gj.call = val2 as CallBlock;
                    (gj.call.children[1] as Block).obj = gj;
                    n++;
                    return gj;
                }
            }
            throw new Exception();
        }
    }
    class ModelObj : ClassObj
    {
        public CallBlock call;
        public ModelObj() : base()
        {
            this.type = ObjType.ModelObj;
        }
        public override Obj exe(Local local)
        {
            return this;
        }
    }
    class ModelVal : Val
    {
        public ModelVal(ModelObj cls, Block block) : base(ObjType.ModelValue)
        {
            this.cls = cls;
            vmap = block.vmap;
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
                    if (word.name == "update")
                    {
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Bracket)
                        {
                            var block = val2.exe(local).Getter(local) as Block;
                            n++;
                            if (block.rets.Count != 1) throw new Exception();
                            var last = block.rets[0];
                            if (last.type == ObjType.Stock)
                            {
                                var stock = last as Stock;
                                return stock.UpdateObj(this, local);
                            }
                        }
                    }
                    else if (vmap.ContainsKey(word.name)) return vmap[word.name];
                }
            }
            throw new Exception();
        }
    }
    class ModelStore : Obj
    {
        public ModelStore() : base(ObjType.ModelStore)
        {
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                var block = val2.exe(local).Getter(local) as Block;
                n++;
                foreach(var obj in block.rets)
                {
                    if (obj.type == ObjType.ModelObj)
                    {

                    }
                    else if (obj.type == ObjType.ArrayType)
                    {

                    }
                    else if (obj.type == ObjType.Stock)
                    {

                    }
                }
            }
            throw new Exception();
        }
    }
    class Stock : Obj
    {
        public Dictionary<Object, List<Val>> stocks = new Dictionary<object, List<Val>>();
        public Stock() : base(ObjType.Stock)
        {

        }
        public Obj Select(Object model, Local local, Function func)
        {
            List<Val> list;
            if (stocks.ContainsKey(model))
            {
                list = stocks[model];
            }
            else
            {
                return new Block(ObjType.Array);
            }
            List<Val> rets = new List<Val>();
            foreach (var obj in list)
            {
                var blk1 = func.draw.children[0].Clone() as Block;
                blk1.exe(local);
                if (blk1.vmap.Count == 1)
                {
                    blk1.vmap.Last().Value.ope("=", local, obj);
                }
                var blk2 = func.draw.children[1].Clone() as Block;
                blk2.exe(local);
                var retobj = blk2.rets.Last().Getter(local);
                if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                if (retobj.type == ObjType.BoolVal)
                {
                    var boolval = retobj as BoolVal;
                    if (boolval.value) rets.Add(obj);
                }
                else throw new Exception();
            }
            return new Block(ObjType.Array) { rets = new List<Obj>(rets) };
        }
        public Obj UpdateObj(Val val, Local local)
        {
            var id = (Number)val.vmap["id"].Getter(local);
            List<Val> list = stocks[val.cls];
            var val2 = list[id.value] as Val;
            foreach (var kv in val.vmap)
            {
                val.vmap[kv.Key].ope("=", local, val2.vmap[kv.Key]);
            }
            return val;
        }
        public Obj Sort(Object model, Local local, Function func)
        {
            List<Val> list;
            if (stocks.ContainsKey(model))
            {
                list = stocks[model];
            }
            else
            {
                return new Block(ObjType.Array);
            }
            return new Block(ObjType.Array) { rets = new List<Obj>(list) }.Sort(func, local);
        }
        public void Store(Val val, Local local)
        {
            List<Val> list;
            if (stocks.ContainsKey(val.cls))
            {
                list = stocks[val.cls];
            }
            else
            {
                list = new List<Val>();
                stocks.Add(val.cls, list);
            }
            var n = ((Number)val.vmap["id"].Getter(local)).value;
            if (n == 0)
            {
                list.Add(val);
                val.vmap["id"].ope("=", local, new Number(list.Count) { cls = local.Int});
            }
            else
            {
                list[n] = val;
            }
        }
    }
    class Gene : Obj
    {
        public Dictionary<String, Obj> vmap = new Dictionary<string, Obj>();
        public GeneObj gj;
        public Gene() : base(ObjType.Gene)
        {
            gj = new GeneObj();
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
                var gj = new GeneObj() { name = word.name };
                local.gene.vmap[word.name] = gj;
                if (val2.type == ObjType.CallBlock)
                {
                    gj.call = val2 as CallBlock;
                    (gj.call.children[1] as Block).obj = gj;
                    foreach (var blk in local.blocks) gj.blocks.Add(blk);
                    n++;
                    return gj;
                }
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var gj = new GeneObj();
                local.gene.vmap[""] = gj;
                gj.call = val2 as CallBlock;
                (gj.call.children[1] as Block).obj = gj;
                foreach (var blk in local.blocks) gj.blocks.Add(blk);
                n++;
                return gj;

            }
            else if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word = val2 as Word;
                    n++;
                    var gj = vmap[""] as GeneObj;
                    return gj.Dot(word.name);
                }
            }
            throw new Exception();
        }
    }
    class GeneChild : Obj
    {
        GeneObj gj;
        public GeneChild(GeneObj gj) : base(ObjType.GeneChild)
        {
            this.gj = gj;
        }

        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            List<Obj> rets = new List<Obj>();
            if (val2.type == ObjType.Bracket && val2.children.Count == 0)
            {
                n++;
                List<CallBlock> cbs = new List<CallBlock>();
                if (gj.left != null) cbs.Add(gj.left);
                if (gj.right != null) cbs.Add(gj.right);
                for (var i = 0; i < 2; i++)
                {
                    var cb = cbs[i % cbs.Count];
                    var block1 = cb.children[0] as Block;
                    block1.exe(local).Getter(local);
                    var fault = true;
                    if (block1.rets.Last().type == ObjType.FloatVal)
                    {
                        var rv = new Random().Next(1000 * 1000 * 1000);
                        if (rv < (block1.rets.Last() as FloatVal).value * 1000 * 1000 * 1000)
                        {
                            return gj.gv = (gj.Mutate(local)) as GeneVal;
                        }
                    }
                    var block2 = cb.children[1] as Block;
                    block2.exe(local).Getter(local);
                    var retobj = block2.rets.Last();
                    if (retobj.type == ObjType.Continue || retobj.type == ObjType.Goto || retobj.type == ObjType.Break) throw new Exception();
                    else if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                    if (retobj.type == ObjType.GeneValue)
                    {
                        rets.Add(((retobj as GeneVal).cls as GeneObj).PartMutate(local, (retobj as GeneVal)));
                    }
                    else if (retobj.type == ObjType.Null)
                    {
                        return gj.gv = (gj.Mutate(local)) as GeneVal;
                    }
                }
                var gv = gj.Cross(local, rets[0], rets[1]);
                gj.gv = gv as GeneVal;
                return gv;
            }
            throw new Exception();
        }
    }
    class GeneMutate : Obj
    {
        GeneObj gj;
        public GeneMutate(GeneObj gj) : base(ObjType.GeneMutate)
        {
            this.gj = gj;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket && val2.children.Count == 0)
            {
                n++;
                return gj.Mutate(local);
            }
            throw new Exception();
        }
    }
    class GeneNew : Obj
    {
        GeneObj gj;
        public GeneNew(GeneObj gj) : base(ObjType.GeneNew)
        {
            this.gj = gj;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket && val2.children.Count == 0)
            {
                n++;
                return gj.New(local);
            }
            throw new Exception();
        }
    }
    class GeneCross : Obj
    {
        GeneObj gj;
        public GeneCross(GeneObj gj) : base(ObjType.GeneCross)
        {
            this.gj = gj;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket && val2.children.Count == 2)
            {
                var block = val2.Clone().exe(local).Getter(local) as Block;
                n++;
                return gj.Cross(local, block.rets[0], block.rets[1]);
            }
            throw new Exception();
        }
    }
    class GeneVal : Val
    {
        public GeneVal(GeneObj cls) : base(ObjType.GeneValue)
        {
            this.cls = cls;
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
                    if (vmap.ContainsKey(word.name)) return vmap[word.name];
                }
            }
            throw new Exception();
        }
    }
    class GeneStore : Obj
    {
        public GeneObj gj;
        public GeneStore(GeneObj gj) : base(ObjType.GeneStore)
        {
            this.gj = gj;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket && val2.children.Count == 0)
            {
                local.db.Store(gj.gv, local);
                ValCheck(gj.gv, local);
                return gj.gv;
            }
            throw new Exception();
        }
        public void ValCheck(Val val0, Local local)
        {
            foreach (var val in val0.vmap.Values)
            {
                if (val.type == ObjType.Variable)
                {
                    var variable = val as Variable;
                    var cls = variable.cls;
                head:
                    if (cls.type == ObjType.ModelObj || cls.type == ObjType.GeneObj)
                    {
                        StoreMany(variable.value, local);
                    }
                    else if (cls.type == ObjType.ArrayType)
                    {
                        var arrtype = cls as ArrType;
                        cls = arrtype.cls;
                        goto head;
                    }
                    else continue;

                }
            }
        }
        public void StoreMany(Obj val, Local local)
        {
            if (val.type == ObjType.Block || val.type == ObjType.Array)
            {
                var block = val as Block;
                foreach(var val1 in block.rets)
                {
                    StoreMany(val1, local);
                }
            }
            else if (val.type == ObjType.GeneValue || val.type == ObjType.ModelValue)
            {
                var value = val as Val;
                local.db.Store(value, local);
                ValCheck(value, local);
            }
        }
    }
    class GeneSelect : Obj
    {
        public GeneObj gj;
        public GeneSelect(GeneObj gj) : base(ObjType.GeneSelect)
        {
            this.gj = gj;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return new Block(ObjType.Block) { rets = new List<Obj>(gj.gvs) };
                }
                else if (val2.children.Count == 1)
                {
                    var rets = new List<Obj>();
                    var blk = val2.exe(local).Getter(local) as Block;
                    if (blk.rets[0].type == ObjType.Function)
                    {
                        var func = blk.rets[0] as Function;
                        var ftype = new FuncType(local.Bool);
                        ftype.draws.Add(gj);
                        if (TypeCheck.CheckCV(ftype, func, CheckType.Setter, local) == null) throw new Exception();
                        local.db.Select(gj, local, func);
                        n++;
                        return new Block(ObjType.Array) { rets = rets };
                    }
                }
            }
            throw new Exception();
        }
    }
    class GeneSort: Obj
    {
        public GeneObj gj;
        public GeneSort(GeneObj gj) : base(ObjType.GeneSort)
        {
            this.gj = gj;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return new Block(ObjType.Block) { rets = new List<Obj>(gj.gvs) };
                }
                else if (val2.children.Count == 1)
                {
                    var blk = val2.exe(local).Getter(local) as Block;
                    n++;
                    if (blk.rets[0].type == ObjType.Function)
                    {
                        var func = blk.rets[0] as Function;
                        var ftype = new FuncType(local.Bool);
                        ftype.draws.Add(gj);
                        ftype.draws.Add(gj);
                        if (TypeCheck.CheckCV(ftype, func, CheckType.Setter, local) == null) throw new Exception();
                        return new Block(ObjType.Array) { rets = new List<Obj>(gj.gvs) }.Sort(func, local);
                    }
                }
            }
            throw new Exception();
        }
    }
    class GeneLabel : Type
    {
        public GeneLabel() : base(ObjType.GeneLabel)
        {

        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var variable = new Variable(this);
                var gvl = new GeneLabelValue();
                variable.value = gvl;
                List<String> names = new List<string>();
                if (local.blocks.Last().obj.type == ObjType.GeneObj)
                {
                    var gj = local.blocks.Last().obj as GeneObj;
                    if (gj.name != null) names.Add(gj.name);
                }
                names.Add(word.name);
                for (var i = 0; i < local.ids.Count; i++)
                {
                    var id = local.ids[i];
                    if (id.names.Length - 1 != names.Count) continue;
                    var success = true;
                    for (var j = 0; j < names.Count; j++)
                    {
                        if (id.names[j] != names[j])
                        {
                            success = false;
                            break;
                        }
                    }
                    if (success) gvl.id = id;
                }
                var call1 = new CallBlock();
                call1.children.Add(new NewBlock(ObjType.Call1) { gvl = gvl });
                call1.children.Add(new CrossBlock(ObjType.Call2));
                var call2 = new CallBlock();
                call2.children.Add(new MigrateBlock(ObjType.Call1));
                call2.children.Add(new NewBlock(ObjType.Call2) { gvl = gvl });
                variable.left = new Function(null) { draw = call1 };
                variable.right = new Function(null) { draw = call2 };
                local.declare(word.name, variable);
            head1:
                if (val2.type == ObjType.Left)
                {
                    var func = new Function(this);
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.Word)
                    {
                        var func2 = val2.Getter(local) as Function;
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Block)
                        {
                            n++;
                            val2 = primary.children[n];
                        }
                        if (val2.type == ObjType.Bracket)
                        {
                            n++;
                            val2 = primary.children[n];
                        }
                        variable.left = func2;
                        goto head1;
                    }
                    else
                    {
                        if (val2.type == ObjType.CallBlock)
                        {
                            func.draw = val2 as CallBlock;
                            n++;
                            val2 = primary.children[n];
                            variable.left = func;
                            goto head1;
                        }
                        else throw new Exception();
                    }
                }
                else if (val2.type == ObjType.Right)
                {
                    var func = new Function(this);
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.Word)
                    {
                        var func2 = val2.Getter(local) as Function;
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Block)
                        {
                            func.block = val2 as Block;
                            n++;
                            val2 = primary.children[n];
                        }
                        if (val2.type == ObjType.Bracket)
                        {
                            func.bracket = val2 as Block;
                            n++;
                            val2 = primary.children[n];
                        }
                        variable.right = func2;
                        goto head1;
                    }
                    else
                    {
                        if (val2.type == ObjType.CallBlock)
                        {
                            func.draw = val2 as CallBlock;
                            n++;
                            val2 = primary.children[n];
                            variable.right = func;
                            goto head1;
                        }
                        else throw new Exception();
                    }
                }
                return this;
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class GeneLabelValue : Val
    {
        public Id id;
        public GeneLabelValue() : base(ObjType.GeneLabelValue)
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
                    if (word.name == "length")
                    {
                        return new Number(id.labels.Count) { cls = local.Int };
                    }
                }
            }
            throw new Exception();
        }
    }
    class NewBlock : Block
    {
        public GeneLabelValue gvl;
        public NewBlock(ObjType type) : base(type)
        {

        }
        public override Obj exe(Local local)
        {
            var id = gvl.id;
            var random = new Random().Next(id.labels.Count);
            id.n = random;
            id.names[id.names.Length - 1] = id.labels[random].name;
            rets.Add(gvl);
            return this;
        }
        public override Obj Clone()
        {
            var block = new NewBlock(type) {gvl = gvl, labelmap = labelmap, branchmap = branchmap, template = template, obj = obj, letter = letter, letter2 = letter2 };
            for (var i = 0; i < children.Count; i++)
            {
                block.children.Add(children[i]);
            }
            foreach (var key in vmap)
            {
                block.vmap[key.Key] = key.Value.Clone();
            }
            if (type == ObjType.Array) block.rets = rets;
            return block;
        }
    }
    class CrossBlock: Block
    {
        public CrossBlock(ObjType type) : base(type)
        {

        }
        public override Obj exe(Local local)
        {
            var random = new Random().Next(2);
            if (random == 0) rets.Add(local.get("v1"));
            else rets.Add(local.get("v2"));
            return this;
        }
        public override Obj Clone()
        {
            var block = new CrossBlock(type) { labelmap = labelmap, branchmap = branchmap, template = template, obj = obj, letter = letter, letter2 = letter2 };
            for (var i = 0; i < children.Count; i++)
            {
                block.children.Add(children[i]);
            }
            foreach (var key in vmap)
            {
                block.vmap[key.Key] = key.Value.Clone();
            }
            if (type == ObjType.Array) block.rets = rets;
            return block;
        }
    }
    class MigrateBlock : Block
    {
        public MigrateBlock(ObjType type) : base(type)
        {

        }
        public override Obj exe(Local local)
        {
            rets.Add(new FloatVal(0.1f) { cls = local.Float });
            return this;
        }
        public override Obj Clone()
        {
            var block = new MigrateBlock(type) { labelmap = labelmap, branchmap = branchmap, template = template, obj = obj, letter = letter, letter2 = letter2};
            for (var i = 0; i < children.Count; i++)
            {
                block.children.Add(children[i]);
            }
            foreach (var key in vmap)
            {
                block.vmap[key.Key] = key.Value.Clone();
            }
            if (type == ObjType.Array) block.rets = rets;
            return block;
        }
    }
    class GeneObj: Type
    {
        public String name;
        public List<Block> blocks = new List<Block>();
        public List<GeneVal> gvs = new List<GeneVal>();
        public GeneVal gv;
        public CallBlock call;
        public CallBlock left;
        public CallBlock right;
        public Dictionary<String, Obj> vmap = new Dictionary<string, Obj>();
        public GeneObj() : base(ObjType.GeneObj)
        {

            vmap["New"] = new GeneNew(this);
            vmap["Store"] = new GeneStore(this);
            vmap["Sort"] = new GeneSort(this);
            vmap["Mutate"] = new GeneMutate(this);
            vmap["Cross"] = new GeneCross(this);
            vmap["Child"] = new GeneChild(this);
        }
        public Obj Dot(String name)
        {
            if (vmap.ContainsKey(name)) return vmap[name];
            return gv.vmap[name];
        }
        public override Obj exe(Local local)
        {
            return this;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Left)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    left = val2 as CallBlock;
                    n++;
                    return this;
                }
            }
            else if (val2.type == ObjType.Right)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    right = val2 as CallBlock;
                    n++;
                    return this;
                }
            }
            throw new Exception();
        }
        public Obj New(Local local)
        {
            var block2 = call.children[1].Clone() as Block;
            local.blocks = blocks;
            local.blocks.Add(block2);
            block2.vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            block2.exe(local).Getter(local);
            foreach (var v in block2.vmap.Values)
            {
                if (v.type == ObjType.Variable)
                {
                    var variable = v as Variable;
                    if (variable.left != null)
                    {
                        var func = variable.left as Function;
                        var blk2 = func.draw.children[0].Clone() as Block;
                        blk2.vmap["g"] = this;
                        blk2.vmap["val"] = variable;
                        local.blocks.Add(blk2);
                        blk2.exe(local).Getter(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        var retobj = blk2.rets.Last();
                        if (retobj.type == ObjType.Goto || retobj.type == ObjType.Continue || retobj.type == ObjType.Break) throw new Exception();
                        else if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                        variable.ope("=", local, retobj);
                    }
                }
            }
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return gv = new GeneVal(this) { vmap = block2.vmap };
        }
        public Obj Mutate(Local local)
        {
            var block2 = call.children[1].Clone() as Block;
            local.blocks = blocks;
            local.blocks.Add(block2);
            block2.vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            block2.exe(local).Getter(local);
            foreach(var v in block2.vmap.Values)
            {
                if (v.type == ObjType.Variable)
                {
                    var variable = v as Variable;
                    if (variable.right != null)
                    {
                        var func = variable.right as Function;
                        var blk2 = func.draw.children[1].Clone() as Block;
                        blk2.vmap["g"] = this;
                        blk2.vmap["val"] = v;
                        local.blocks.Add(blk2);
                        blk2.exe(local).Getter(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        var retobj = blk2.rets.Last();
                        if (retobj.type == ObjType.Goto || retobj.type == ObjType.Continue || retobj.type == ObjType.Break) throw new Exception();
                        else if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                        variable.ope("=", local, retobj);
                    }
                }
            }
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return gv = new GeneVal(this) { vmap = block2.vmap };
        }
        public Obj PartMutate(Local local, GeneVal gv)
        {
            var block2 = call.children[1].Clone() as Block;
            local.blocks = blocks;
            block2.vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            local.blocks.Add(block2);
            block2.exe(local).Getter(local);
            foreach (var kv in block2.vmap)
            {
                if (kv.Value.type == ObjType.Variable)
                {
                    var variable = kv.Value as Variable;
                    if (variable.right != null)
                    {
                        var func = variable.right as Function;
                        var blk1 = func.draw.children[0].Clone() as Block;
                        local.blocks.Add(blk1);
                        blk1.exe(local).Getter(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        var rv = new Random().Next(1000 * 1000 * 1000);
                        if (blk1.rets.Last().type == ObjType.FloatVal && rv < (blk1.rets.Last() as FloatVal).value * 1000 * 1000 * 1000)
                        {
                            var blk2 = func.draw.children[1].Clone() as Block;
                            blk2.vmap["g"] = this;
                            blk2.vmap["val"] = variable;
                            local.blocks.Add(blk2);
                            blk2.exe(local).Getter(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            var retobj = blk2.rets.Last();
                            if (retobj.type == ObjType.Goto || retobj.type == ObjType.Continue || retobj.type == ObjType.Break) throw new Exception();
                            else if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                            variable.ope("=", local, retobj);
                        }
                        else
                        {
                            variable.ope("=", local, (gv.vmap[kv.Key] as Variable).value);
                        }
                    }
                }
            }
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return new GeneVal(this) { vmap = block2.vmap };
        }
        public Obj Cross(Local local, Obj g1, Obj g2)
        {
            var block2 = call.children[1].Clone() as Block;
            local.blocks = blocks;
            block2.vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            local.blocks.Add(block2);
            block2.exe(local).Getter(local);
            foreach (var kv in block2.vmap)
            {

                if (kv.Value.type == ObjType.Variable)
                {
                    var variable = kv.Value as Variable;
                    if (variable.left != null)
                    {
                        var func = variable.right as Function;
                        var blk1 = func.draw.children[0].Clone() as Block;
                        blk1.vmap["g"] = this;
                        blk1.vmap["val"] = variable;
                        local.blocks.Add(blk1);
                        blk1.exe(local).Getter(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        var rv = new Random().Next(1000 * 1000 * 1000);
                        if (blk1.rets.Last().type == ObjType.FloatVal && rv < (blk1.rets.Last() as FloatVal).value * 1000 * 1000 * 1000)
                        {
                            var blk2 = func.draw.children[1].Clone() as Block;
                            blk2.vmap["g"] = this;
                            blk2.vmap["val"] = variable;
                            local.blocks.Add(blk2);
                            blk2.exe(local).Getter(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            var retobj = blk2.rets.Last();
                            if (retobj.type == ObjType.Goto || retobj.type == ObjType.Continue || retobj.type == ObjType.Break) throw new Exception();
                            else if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                            variable.ope("=", local, retobj);
                        }
                        else
                        {
                            var func2 = variable.left as Function;
                            var blk2 = func2.draw.children[1].Clone() as Block;
                            local.blocks.Add(blk2);
                            blk2.vmap["g1"] = (GeneVal)g1;
                            blk2.vmap["g2"] = (GeneVal)g2;
                            blk2.vmap["val1"] = ((GeneVal)g1).vmap[kv.Key];
                            blk2.vmap["val2"] = ((GeneVal)g2).vmap[kv.Key];
                            blk2.exe(local).Getter(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            var retobj = blk2.rets.Last();
                            if (retobj.type == ObjType.Goto || retobj.type == ObjType.Continue || retobj.type == ObjType.Break) throw new Exception();
                            else if (retobj.type == ObjType.Return) retobj = (retobj as Return).value;
                            variable.ope("=", local, retobj);
                        }
                    }
                }
            }
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return gv = new GeneVal(this) { vmap = block2.vmap };
        }
    }
    
}
