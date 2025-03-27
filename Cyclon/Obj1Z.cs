using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Obj
    {
        public virtual Obj exeZ(Local local)
        {
            throw new Exception();
        }
        public virtual Obj exepZ(ref int n, Local local, Primary primary)
        {
            throw new Exception();
        }
        public virtual Obj GetterZ(Local local)
        {
            return this;
        }
        public virtual Obj SelfZ(Local local)
        {
            return this;
        }
        public virtual Obj opeZ(String key, Local local, Obj val2)
        {
            return this;
        }
        public virtual Obj PrimaryZ(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
    }
    partial class Operator
    {
        public override Obj exeZ(Local local)
        {
            if (name == "=" || name == ":" || name == "in")
            {
                var val1 = children[0].exeZ(local).SelfZ(local);
                if (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG) return val1;
                var val2 = children[1].exeZ(local).GetterZ(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                return this;
            }
            else
            {
                var val1 = children[0].exeZ(local).GetterZ(local);
                if (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG) return val1;
                var val2 = children[1].exeZ(local).GetterZ(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                return this;
            }
            throw new Exception();
        }
    }
    partial class Primary
    {
        public override Obj exeZ(Local local)
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
                        i++;
                        break;
                    case ObjType.Model:
                    case ObjType.Gene:
                    case ObjType.Class:
                        val1 = children[i].exepZ(ref i, local, this);
                        break;
                    case ObjType.Dot:
                    case ObjType.Left:
                    case ObjType.Right:
                        i++;
                        break;
                    case ObjType.CallBlock:
                        i++;
                        break;
                    default:
                        throw new Exception();
                }
                if (val1 != null && (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG)) return val1;
            }
            if (val1 == null) val1 = children.Last();
            return val1;
        }
    }
    partial class Block
    {
        public override Obj exepZ(ref int n, Local local, Primary primary)
        {
            if (type == ObjType.Block)
            {
                var m = 0;
                if (children[m].type == ObjType.Word)
                {
                    if ((children[m] as Word).name == "T")
                    {
                        m++;
                        var val2 = children[m];
                        Type type = new VariClass(0);
                    head:
                        if (val2.type == ObjType.Block)
                        {
                            var blk = val2 as Block;
                            if (blk.children.Count == 0)
                            {
                                type = new ArrType(type);
                                m++;
                                val2 = children[m];
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
                            /*var gene = false;
                            if (val2.type == ObjType.Gene)
                            {
                                gene = true;
                                n++;
                                val2 = primary.children[n];
                            }*/
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
                                    local.declareZ(word.name, func);
                                    return func;
                                }
                                else throw new Exception();
                            }
                            else if (val2.type == ObjType.CallBlock)
                            {
                                var func = new Function(type);
                                func.draw = val2 as CallBlock;
                                foreach (var b in local.blocks) func.blocks.Add(b);
                                local.declareZ(word.name, func);
                                return func;
                            }
                        }
                    }
                }
            }
            return exeRangeB(0, children.Count, local, true);
        }
        public override Obj exeZ(Local local)
        {
            if (type == ObjType.Array) return this;
            return exeRangeZ(0, children.Count, local, true);
        }
        public Obj exeRangeZ(int start, int end, Local local, bool id)
        {
            for (var i = 0; i < children.Count; i++)
            {
                var val = children[i].exeZ(local);
                if (type != ObjType.Call1 && type != ObjType.Call2)
                {
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                }
                rets.Add(val);
            }
            return this;
        }
        public override Obj PrimaryZ(ref int n, Local local, Primary primary, Obj val2)
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
                            local.declareZ((val2 as Word).name, variable);
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
                    local.declareZ(word.name, variable);
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
                            val2.exeZ(local).GetterZ(local);
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
                                    val2.exeZ(local).GetterZ(local);
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
                                val2 = val2.exeZ(local).GetterZ(local);
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
                    block.GetterZ(local);
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
        public override Obj SelfZ(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].SelfZ(local);
            }
            return this;
        }
        public override Obj GetterZ(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].GetterZ(local);
            }
            return this;
        }
    }
    partial class CallBlock
    {
        public override Obj exepZ(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj exeZ(Local local)
        {
            var block2 = children[1] as Block;
            block2.vmapA["int"] = local.Int;
            block2.vmapA["Str"] = local.Str;
            block2.vmapA["bool"] = local.Bool;
            block2.vmapA["float"] = local.Float;
            block2.vmapA["MouseDown"] = local.MouseEvent;
            block2.vmapA["KeyDown"] = local.KeyEvent;
            for (var i = 0; i < local.analblocks.Count; i++)
            {
                var block = local.analblocks[i];
                local.blocks = new List<Block> { block };
                block.exeZ(local);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
            }
            return this;
        }

    }
    partial class Local
    {
        public void declareZ(String name, Obj obj)
        {
            blocks.Last().vmapA[name] = obj;
        }
    }
    partial class CommentLet
    {
    }
    partial class Class
    {
        public override Obj exepZ(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
            Obj obj = new ClassObj();
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Block)
                {
                    var generic = new Generic();
                    n++;
                    val2 = primary.children[n];
                    obj = generic;
                }
                local.declareZ(word.name, obj);
                obj.letter = word.letter;
            }
            return this;
        }
    }
    partial class Model : Obj
    {
        public override Obj exepZ(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var gj = new ModelObj();
                local.declareZ(word.name, gj);
                gj.letter = word.letter;
                if (val2.type == ObjType.CallBlock)
                {
                    gj.letter2 = (val2.children[1] as Block).letter2;
                    gj.draw = val2 as CallBlock;
                    (gj.draw.children[1] as Block).obj = new ObjBlock() { obj = gj, n = 1 };
                    n++;
                    return gj;
                }
            }
            return this;
        }
    }
    partial class Gene : Obj
    {
        public override Obj exepZ(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var gj = new GeneObj() { name = word.name };
                local.gene.vmap[word.name] = gj;
                local.declareZ(word.name, gj);
                gj.letter = letter;
                if (val2.type == ObjType.CallBlock)
                {
                    gj.letter2 = (val2.children[1] as Block).letter2;
                    gj.call = val2 as CallBlock;
                    (gj.call.children[1] as Block).obj = new ObjBlock() { obj = gj, n = 1 };
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
                (gj.call.children[1] as Block).obj = new ObjBlock() { obj = gj, n = 1 };
                foreach (var blk in local.blocks) gj.blocks.Add(blk);
                n++;
                return gj;

            }
            return this;
        }
    }
}
