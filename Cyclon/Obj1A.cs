using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Obj
    {
        public Letter let;
        public String error;
        public virtual Obj exeA(Local local)
        {
            throw new Exception();
        }
        public virtual Obj exepA(ref int n, Local local, Primary primary)
        {
            throw new Exception();
        }
        public virtual Obj GetterA(Local local)
        {
            return this;
        }
        public virtual Obj SelfA(Local local)
        {
            return this;
        }
        public virtual Obj opeA(String key, Local local, Obj val2)
        {
            return this;
        }
        public virtual Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
        public virtual String Info()
        {
            throw new Exception();
        }
    }
    partial class Operator
    {
        public override Obj exeA(Local local)
        {
            if (name == "=" || name == ":" || name == "in")
            {
                var val1 = children[0].exeA(local).SelfA(local);
                if (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG) return val1;
                var val2 = children[1].exeA(local).GetterA(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                return val1;
            }
            else
            {
                var val1 = children[0].exeA(local).GetterA(local);
                if (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG) return val1;
                var val2 = children[1].exeA(local).GetterA(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                return val1;
            }
        }
    }
    partial class Primary
    {
        public override Obj exeA(Local local)
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
                        children[i].Clone().exepA(ref i, local, this);
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
                            val1 = children[i].exepA(ref i, local, this);
                            i++;
                        }
                        else
                        {
                            val1 = val1.GetterA(local);
                            val1 = val1.PrimaryA(ref i, local, this, children[i].Clone().exepA(ref i, local, this));
                        }
                        break;
                    case ObjType.Dot:
                    case ObjType.Left:
                    case ObjType.Right:
                        val1 = val1.GetterA(local);
                        val1 = val1.PrimaryA(ref i, local, this, children[i]);
                        break;
                    case ObjType.CallBlock:
                        if (val1 == null)
                        {
                            val1 = children[i].exepA(ref i, local, this);
                            i++;
                        }
                        else
                        {
                            val1 = val1.GetterA(local);
                            val1 = val1.PrimaryA(ref i, local, this, children[i]);
                        }
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
        public Dictionary<String, Obj> vmapA = new Dictionary<string, Obj>();
        public List<Block> childblocks = new List<Block>();
        public List<Block> blocks = new List<Block>();
        public String Show(String space, ref bool error)
        {
            var text = "";
            if (obj.obj.type != ObjType.None) text += space + "$" + obj.n + ":" + obj.obj.Info() + "\n";
            foreach(var obj in vmapA)
            {
                text += space + "  " + obj.Value.Info() + "\n";
            }
            for (var i = 0; i < statuses.Length; i++)
            {
                if (statuses[i].status == FinishStatus.Yet)
                {
                    text += "Yet:";
                    error = true;
                }
                else if (statuses[i].status == FinishStatus.Error)
                {
                    text += "Error:";
                    statuses[i].letter.error = statuses[i].error;
                    error = true;
                }
                else if (statuses[i].status == FinishStatus.Finish) text += "Finish:";
            }
            text += "\n";
            for (var i = 0; i < childblocks.Count; i++)
            {
                text += childblocks[i].Show("    " + space, ref error); 
            }
            return text;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
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
                            else return Obj.Error(ObjType.Error, val2.letter, "配列型は引数が0です");
                        }
                        n = 1;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var word = val2 as Word;
                            n++;
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Block)
                            {
                                var func = new GenericFunction(type);
                                var val = val2.exeA(local);
                                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                var blk = val as Block;
                                for (var i = 0; i < blk.rets.Count; i++)
                                {
                                    if (blk.rets[i].type == ObjType.Word)
                                    {
                                        var word2 = blk.rets[i] as Word;
                                        func.vmap[word2.name] = new VariClass(i);
                                    }
                                    else return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスの引数は名前を指定してください");
                                }
                                n++;
                                val2 = primary.children[n];
                                if (val2.type == ObjType.CallBlock)
                                {
                                    func.draw = val2 as CallBlock;
                                    foreach (var b in local.blocks) func.blocks.Add(b);
                                    local.declareA(word.name, func);
                                    return func;
                                }
                                return Obj.Error(ObjType.Error, val2.letter, "ジェネリックス関数の{|}が宣言されていません");
                            }
                            else if (val2.type == ObjType.CallBlock)
                            {
                                var func = new Function(type);
                                func.draw = val2 as CallBlock;
                                foreach (var b in local.blocks) func.blocks.Add(b);
                                local.declareA(word.name, func);
                                return func;
                            }
                        }
                    }
                }
            }
            return exeRangeB(0, children.Count, local, true);
        }
        public override Obj exeA(Local local)
        {
            if (type == ObjType.Array) return this;
            return exeRangeA(0, children.Count, local, true);
        }
        public Obj exeRangeA(int start, int end, Local local, bool id)
        {
            rets = new List<Obj>();
            if (type == ObjType.Call1 || type == ObjType.Call2)
            {
                statuses = new CodeStatus[children.Count];
                for (var i = 0; i < children.Count; i++)
                {
                    statuses[i] = new CodeStatus();
                    var val = children[i].exeA(local);
                    if (val.type == ObjType.Wait)
                    {
                        statuses[i].status = FinishStatus.Yet;
                    }
                    else if (val.type == ObjType.Error || val.type == ObjType.NG)
                    {
                        statuses[i] = new CodeStatus() { letter = val.let, error = val.error, status = FinishStatus.Error };
                    }
                    else statuses[i].status = FinishStatus.Finish;
                    rets.Add(val);
                }
            }
            else
            {
                for (var i = 0; i < children.Count; i++)
                {
                    var val = children[i].exeA(local);
                    if (val.letter == null)
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    if (val.letter == null) val.letter = children[i].letter;
                    rets.Add(val);
                }
            }
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (type == ObjType.Bracket)
            {
                if (val2.type == ObjType.Block)
                {
                    if (rets.Count == 1)
                    {
                        if (val2.children.Count == 0)
                        {
                            var cls = rets[0] as Type;
                            if (cls != null)
                            {
                                var typearray = new ArrType(cls);
                                return typearray;
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "配列方の宣言は型を元にしてください");
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "配列型の宣言の[]は引数が0です");
                    }
                    else return Obj.Error(ObjType.Error, letter, "配列方の宣言は1つの型を配列にしてください");
                }
                else if (val2.type == ObjType.Bracket)
                {
                    var val = val2.exe(local);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    var block = val as Block;
                    n++;
                    val2 = primary.children[n];
                    for (var i = 0; i < block.rets.Count; i++)
                    {
                        val2 = block.rets[i];
                        if (val2.type == ObjType.Word)
                        {
                            var cls = rets[i % rets.Count] as Type;
                            if (cls != null)
                            {
                                var variable = new Variable((Type)rets[i % rets.Count]);
                                local.declareA((val2 as Word).name, variable);
                                block.rets[i] = variable;
                            }
                            else return Obj.Error(ObjType.Error, rets[i % rets.Count].letter, "複数型の変数宣言は型を指定してください");
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "複数の変数の宣言には名前を指定してください");
                    }
                    return block;
                }
                else if (val2.type == ObjType.Word)
                {
                    var word = val2 as Word;
                    n++;
                    val2 = primary.children[n];
                    var variable = new Variable((Type)rets[0]);
                    local.declareA(word.name, variable);
                    return variable;
                }
                else if (val2.type == ObjType.Left)
                {
                    n++;
                    val2 = primary.children[n];
                    if (rets.Count == 1)
                    {
                        return rets[0].PrimaryA(ref n, local, primary, val2);
                    }
                    else return Obj.Error(ObjType.Error, letter, "この()は1つしかとれない");
                }
                else if (val2.type == ObjType.Dot)
                {
                    if (rets.Count == 1)
                    {
                        return rets[0].PrimaryA(ref n, local, primary, val2);
                    }
                    else return Obj.Error(ObjType.Error, letter, "この()は1つしかとれない");
                }
            }
            else if (type == ObjType.Block || type == ObjType.Array)
            {
                if (val2.type == ObjType.Block)
                {
                    var val = val2.exeA(local).GetterA(local);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    var block = val as Block;
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
                            else return Obj.Error(ObjType.Error, block.rets[i].letter, "配列の引数は文字列か整数です");
                        }
                        else return Obj.Error(ObjType.Error, block.rets[i].letter, "配列の深さが足りません");
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
                        else if (word.name == "Sort")
                        {
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Bracket)
                            {
                                var val = val2.exeA(local).GetterA(local);
                                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                n++;
                                return this;
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "Sortの()がありません");
                        }
                        else if (word.name == "Range")
                        {
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Bracket)
                            {
                                var val = val2.exeA(local).GetterA(local);
                                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                n++;
                                return this;
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "Rangeの()がありません");
                        }
                        else return Obj.Error(ObjType.NG, val2.letter, "配列には" + word.name + "は宣言されていません");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前が必要です");
                }
                else return Obj.Error(ObjType.NG, val2.letter, "[]の後が必要です");
            }
            return Obj.Error(ObjType.Error, val2.letter, "不正なエラー");
        }
        public override Obj SelfA(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].SelfA(local);
            }
            return this;
        }
        public override Obj GetterA(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].GetterA(local);
            }
            return this;
        }
    }
    partial class CallBlock
    {
        public static int N;
        public int n;
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj exeA(Local local)
        {
            var block2 = children[1] as Block;
            block2.blocks = new List<Block> { block2 };
            local.blocks = new List<Block>();
            local.blocks.Add(block2);
            block2.exeA(local);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.blockslist.RemoveAt(local.blockslist.Count - 1);
            return this;
        }

    }
    partial class Local
    {
        public List<Block> analblocks = new List<Block>();
        public Dictionary<String, Obj> sigmapA = new Dictionary<string, Obj>();
        public Dictionary<int, Obj> vmapA2 = new Dictionary<int, Obj>();
        public void declareA2(CallBlock call, Obj obj)
        {
            call.n = CallBlock.N;
            vmapA2[CallBlock.N] = obj;
            CallBlock.N++;
        }
        public void declareA(String name, Obj obj)
        {
            blocks.Last().vmapA[name] = obj;
        }
        public Obj getA(String name)
        {
            for (var i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].vmapA.ContainsKey(name)) return blocks[i].vmapA[name];
            }
            if (local.sigmapA.ContainsKey(name)) return local.sigmapA[name];
            return new Value(null);
        }
    }
    partial class CommentLet
    {
    }
}
