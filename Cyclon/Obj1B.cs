using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Obj
    {
        public Dictionary<String, Func<String, Local, Obj, Obj>> opesB = new Dictionary<string, Func<string, Local, Obj, Obj>>();
        public virtual Obj exeB(Local local)
        {
            throw new Exception();
        }
        public virtual Obj exepB(ref int n, Local local, Primary primary)
        {
            throw new Exception();
        }
        public virtual Obj GetterB(Local local)
        {
            return this;
        }
        public virtual Obj SelfB(Local local)
        {
            return this;
        }
        public virtual Obj opeB(String key, Local local, Obj val2)
        {
            return opesB[key](key, local, val2);
        }
        public virtual Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
    }
    partial class Operator
    {
        public override Obj exeB(Local local)
        {
            if (name == "=" || name == ":" || name == "in")
            {
                var val1 = children[0].exeB(local).SelfB(local);
                if (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG) return val1;
                local.calls.Add(local.KouhoSet2);
                var val2 = children[1].exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                return val1.opeB(name, local, val2);
            }
            else
            {
                local.calls.Add(local.KouhoSet2);
                var val1 = children[0].exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG) return val1;
                local.calls.Add(local.KouhoSet2);
                var val2 = children[1].exeB(local).GetterB(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                local.calls.RemoveAt(local.calls.Count - 1);
                return val1.opeB(name, local, val2);
            }
            throw new Exception();
        }
    }
    partial class Primary
    {
        public override Obj exeB(Local local)
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
                        children[i].Clone().exepB(ref i, local, this);
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
                            val1 = children[i].exepB(ref i, local, this);
                            i++;
                        }
                        else
                        {
                            val1 = val1.GetterB(local);
                            val1 = val1.PrimaryB(ref i, local, this, children[i].Clone().exep(ref i, local, this));
                        }
                        break;
                    case ObjType.Dot:
                    case ObjType.Left:
                    case ObjType.Right:
                        val1 = val1.GetterB(local);
                        val1 = val1.PrimaryB(ref i, local, this, children[i]);
                        break;
                    case ObjType.CallBlock:
                        if (val1 == null)
                        {
                            val1 = children[i].exepB(ref i, local, this);
                            i++;
                        }
                        else
                        {
                            val1 = val1.GetterB(local);
                            val1 = val1.PrimaryB(ref i, local, this, children[i]);
                        }
                        break;
                    default:
                        throw new Exception();
                }
                if (val1 != null && (val1.type == ObjType.Wait || val1.type == ObjType.Error || val1.type == ObjType.NG)) return val1;
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
                        if (val1 != children.Last()) val1 = val1.opeB(str, local, null); break;
                    default:
                        val1 = val1.GetterB(local);
                        val1 = val1.opeB(str, local, null); break;
                }
            }
            if (local.comments.Count > 0) local.comments.Last().SingleB(op, local);
            return val1;
        }
    }
    enum FinishStatus
    {
        Yet, Finish,
        Error
    }
    class CodeStatus
    {
        public FinishStatus status;
        public Letter letter;
        public String error;
    }
    class YetException : Exception
    {

    }
    partial class Block
    {
        public List<Obj> returns = new List<Obj>();
        public CodeStatus[] statuses;
        public SortedList<int, Label> branchmapn = new SortedList<int, Label>();
        public SortedList<int, Label> labelmapn = new SortedList<int, Label>();
        public bool finished;
        public bool analyse(Local local)
        {
            var check = true;
            for (var i = 0; i < children.Count; i++)
            {
                if (statuses[i].status == FinishStatus.Yet)
                {
                    var val = children[i].exeB(local);
                    if (val.type == ObjType.Wait)
                    {
                        statuses[i].status = FinishStatus.Yet;
                        check = false;
                    }
                    else if (val.type == ObjType.Error || val.type == ObjType.NG)
                    {
                        statuses[i] = new CodeStatus() { letter = val.let, error = val.error, status = FinishStatus.Error };
                    }
                    else
                    {
                        statuses[i].status = FinishStatus.Finish;
                        local.step++;
                        if (obj.n == 1)
                        {
                            if (obj.obj.type == ObjType.ClassObj || obj.obj.type == ObjType.GeneObj || obj.obj.type == ObjType.ModelObj) continue;
                            if (obj.obj.type == ObjType.Switch)
                            {
                                if (i == children.Count - 1 || labelmapn.ContainsKey(i + 1))
                                {
                                    var ret = TypeCheck.CheckB(obj.obj, val, CheckType.Finish, local);
                                    if (ret.type == ObjType.Wait) statuses[i].status = FinishStatus.Yet;
                                    else if (ret.type == ObjType.Error || ret.type == ObjType.NG)
                                    {
                                        statuses[i] = new CodeStatus() { letter = ret.letter, error = ret.error, status = FinishStatus.Error };
                                    }
                                }
                                continue;
                            }
                            if (i == children.Count - 1 ||
                                ((!branchmap.ContainsKey("end") || branchmap["end"].n == children.Count) && branchmapn.ContainsKey(i + 1)))
                            {
                                var ret = TypeCheck.CheckB(obj.obj, val, CheckType.Finish, local);
                                if (ret.type == ObjType.Wait) statuses[i].status = FinishStatus.Yet;
                                else if (ret.type == ObjType.Error || ret.type == ObjType.NG)
                                {
                                    statuses[i] = new CodeStatus() { letter = ret.let, error = ret.error, status = FinishStatus.Error };
                                }
                            }
                        }
                    }
                }
            }
            return check;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
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
                                var blk = val2.exe(local) as Block;
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
                                    local.declareB(word.name, func);
                                    return func;
                                }
                                return Obj.Error(ObjType.Error, val2.letter, "ジェネリックス関数の{|}が宣言されていません");
                            }
                            else if (val2.type == ObjType.CallBlock)
                            {
                                var func = new Function(type);
                                func.draw = val2 as CallBlock;
                                foreach (var b in local.blocks) func.blocks.Add(b);
                                local.declareB(word.name, func);
                                return func;
                            }
                            return Obj.Error(ObjType.Error, val2.letter, "ジェネリックス関数の{|}が宣言されていません");
                        }
                    }
                }
            }
            return exeRangeB(0, children.Count, local, true);
        }
        public override Obj exeB(Local local)
        {
            if (type == ObjType.Array) return this;
            return exeRangeB(0, children.Count, local, true);
        }
        public Obj exeRangeB(int start, int end, Local local, bool id)
        {
            if (local.elem && local.kouhos == null)
            {
                foreach(var kv in labelmap)
                {
                    if (kv.Value.letter == local.letter)
                    {
                        local.KouhoSetElem();
                    }
                }
            }
            rets = new List<Obj>();
            for (var i = 0; i < children.Count; i++)
            {
                var val = children[i].exeB(local);
                rets.Add(val);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                if (val.letter == null) val.letter = children[i].letter;
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
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
                    var block = val2.exe(local) as Block;
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
                                local.declareB((val2 as Word).name, variable);
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
                    local.declareB(word.name, variable);
                    return variable;
                }
                else if (val2.type == ObjType.Left)
                {
                    n++;
                    val2 = primary.children[n];
                    if (rets.Count == 1)
                    {
                        return rets[0].PrimaryB(ref n, local, primary, val2);
                    }
                    else return Obj.Error(ObjType.Error, letter, "この()は1つしかとれない");

                }
                else if (val2.type == ObjType.Dot)
                {
                    if (rets.Count == 1)
                    {
                        return rets[0].PrimaryB(ref n, local, primary, val2);
                    }
                    else return Obj.Error(ObjType.Error, letter, "この()は1つしかとれない");
                }
                else return Obj.Error(ObjType.NG, val2.letter, "()の後が必要です");
            }
            else if (type == ObjType.Block || type == ObjType.Array)
            {
                if (val2.type == ObjType.Block)
                {
                    local.calls.Add(local.KouhoSet2);
                    var val = val2.exeB(local).GetterB(local);
                    local.calls.RemoveAt(local.calls.Count - 1);
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
                        if (val2.letter == local.letter && local.kouhos == null)
                        {
                            local.kouhos = new SortedList<string, Obj>();
                            local.kouhos.Add("length", new Obj(ObjType.None));
                            local.kouhos.Add("Sort", new Obj(ObjType.None));
                            local.kouhos.Add("Range", new Obj(ObjType.None));
                        }
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
                                n++;
                                var val = val2.exeB(local).GetterB(local);
                                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                var blk1 = val as Block;
                                if (blk1.rets[0].type == ObjType.Function)
                                {
                                    return SortB(blk1.rets[0] as Function, local);
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "Sortの引数は関数しかだめです");
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "Sortの()がありません");
                        }
                        else if (word.name == "Range")
                        {
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Bracket)
                            {
                                var val = val2.exeB(local).GetterB(local);
                                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                var blk = val as Block;
                                if (blk.rets.Count == 0) { }
                                else if (blk.rets.Count == 1)
                                {
                                    if (blk.rets[0].type != ObjType.Number) return Obj.Error(ObjType.Error, blk.rets[0].letter, "Rangeの引数1は整数です");
                                }
                                else if (blk.rets.Count == 2)
                                {
                                    if (blk.rets[0].type != ObjType.Number) return Obj.Error(ObjType.Error, blk.rets[0].letter, "Rangeの引数1は整数です");
                                    if (blk.rets[1].type != ObjType.Number) return Obj.Error(ObjType.Error, blk.rets[0].letter, "Rangeの引数2は整数です");
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "Rangeの引数は0,1,2です");
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
        public override Obj SelfB(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].SelfB(local);
                if (rets[i].type == ObjType.Wait || rets[i].type == ObjType.Error || rets[i].type == ObjType.NG) return rets[i];
            }
            return this;
        }
        public override Obj GetterB(Local local)
        {
            for (var i = 0; i < rets.Count; i++)
            {
                rets[i] = rets[i].GetterB(local).GetterB(local);
                if (rets[i].type == ObjType.Wait || rets[i].type == ObjType.Error || rets[i].type == ObjType.NG) return rets[i];
            }
            return this;
        }
        public Obj EqualB(String op, Local local, Obj val2)
        {
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
        public Obj InB(String op, Local local, Obj val2)
        {
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
            else return Obj.Error(ObjType.Error, val2.letter, "イテレーターには配列を代入してください");
        }
        public Obj OpeB(String op, Local local, Obj val2)
        {
            if (type == ObjType.Bracket)
            {
                if (rets.Count == 1)
                {
                    return rets[0].ope(op, local, val2);
                }
                else return Obj.Error(ObjType.Error, letter, "()は1つだけです");
            }
            else return Obj.Error(ObjType.Error, letter, "演算子は()だけです");
        }
        public Obj SortB(Function func, Local local)
        {
            var ftype = new FuncType(local.Bool);
            ftype.draws.Add((rets[0] as Val).cls);
            ftype.draws.Add((rets[0] as Val).cls);
            var ret = TypeCheck.CheckCVB(ftype, func, CheckType.Setter, local);
            if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
            return this;
        }
    }
    partial class CallBlock
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj exeB(Local local)
        {
            for(var i = 0; i < local.analblocks.Count; i++)
            {
                var blk = local.analblocks[i];
                blk.statuses = new CodeStatus[blk.children.Count];
                for (var j = 0; j < blk.statuses.Length; j++) {
                    blk.statuses[j] = new CodeStatus();
                }
                foreach(var label in blk.branchmap)
                {
                    blk.branchmapn[label.Value.n] = label.Value;
                }
                foreach(var label in blk.labelmap)
                {
                    blk.labelmapn[label.Value.n] = label.Value;
                }
                if (blk.branchmapn.Count > 0) blk.branchmapn.RemoveAt(0);
                if (blk.labelmapn.Count > 0) blk.labelmapn.RemoveAt(0);
            }
            for (; local.analblocks.Count != 0; )
            {
                local.step = 0;
                for (var i = 0; i < local.analblocks.Count; i++)
                {
                    var block = local.analblocks[i];
                    if (block.finished) continue;
                    if (block.blocks.Count == 0) continue;
                    local.blocks = block.blocks;
                    local.obj = block.obj;
                    var ret = block.analyse(local);
                    local.blockslist.RemoveAt(local.blockslist.Count - 1);
                    if (ret)
                    {
                        block.finished = true;
                    }
                }
                if (local.step == 0) break;
            }
            for (var i = 0; i < local.analblocks.Count; i++)
            {
                var block = local.analblocks[i];
                block.statuses = new CodeStatus[block.children.Count];
                for (var j = 0; j < block.statuses.Length; j++)
                {
                    block.statuses[j] = new CodeStatus();
                }
                local.blocks = block.blocks;
                local.obj = block.obj;
                var ret = block.analyse(local);
                local.blockslist.RemoveAt(local.blockslist.Count - 1);
                if (block.obj.n == 0 || block.obj.obj.type == ObjType.TagBlock && local.kouhos == null)
                {
                    foreach (var kv in block.labelmap)
                    {
                        if (kv.Value.letter == local.letter)
                        {
                            local.KouhoSetElem();
                        }
                    }
                }
                if (ret)
                {
                    local.analblocks.RemoveAt(i);
                    i--;
                }
            }
            return this;
        }

    }
    partial class Local
    {
        public bool elem;
        public ObjBlock obj;
        public int step = 0;
        public void declareB(String name, Obj obj)
        {
        }
        public Obj getB(String name, Letter letter)
        {
            for (var i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].vmapA.ContainsKey(name)) return blocks[i].vmapA[name];
            }
            if (local.sigmapA.ContainsKey(name)) return local.sigmapA[name];
            return Obj.Error(ObjType.Error, letter, name + "という変数が見つかりません");
        }
        public void KouhoSet()
        {

            kouhos = new SortedList<string, Obj>();
            var reach = false;
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    kouhos.Add(kv.Key, kv.Value);
                }
                if (block.obj.n == 1)
                {
                    if (block.obj.obj.type == ObjType.Function)
                    {
                        reach = true;
                        local.kouhos.Add("return", new Return());
                    }
                    else if (!reach && (block.obj.obj.type == ObjType.For || block.obj.obj.type == ObjType.Switch || block.obj.obj.type == ObjType.While))
                    {
                        kouhos.Add("continue", new Continue());
                        kouhos.Add("break", new Break());
                    }
                }
            }
            kouhos.Add("class", new Class());
            kouhos.Add("if", new If());
            kouhos.Add("for", new For());
            kouhos.Add("while", new While());
            kouhos.Add("switch", new Switch());
            kouhos.Add("goto", new Goto());
            kouhos.Add("gene", new Gene());
            kouhos.Add("model", new Model());
        }
        public void KouhoSet2()
        {

            kouhos = new SortedList<string, Obj>();
            var reach = false;
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    kouhos.Add(kv.Key, kv.Value);
                }
                if (block.obj.n == 1)
                {
                    if (block.obj.obj.type == ObjType.Function)
                    {
                        reach = true;
                        local.kouhos.Add("return", new Return());
                    }
                    else if (!reach && (block.obj.obj.type == ObjType.For || block.obj.obj.type == ObjType.Switch || block.obj.obj.type == ObjType.While))
                    {
                        kouhos.Add("continue", new Continue());
                        kouhos.Add("break", new Break());
                    }
                }
            }
        }
        public void KouhoSetClaGen()
        {

            kouhos = new SortedList<string, Obj>();
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    if (kv.Value.type == ObjType.ClassObj || kv.Value.type == ObjType.Generic) kouhos.Add(kv.Key, kv.Value);
                }
            }
        }
        public void KouhoSetType()
        {
            kouhos = new SortedList<string, Obj>();
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    if (kv.Value is Type || kv.Value.type == ObjType.Generic) kouhos.Add(kv.Key, kv.Value);
                }
            }
        }
        public void KouhoSetFunction()
        {
            kouhos = new SortedList<string, Obj>();
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    if (kv.Value.type == ObjType.Function) kouhos.Add(kv.Key, kv.Value);
                }
            }
        }
        public void KouhoSetLabel()
        {
            kouhos = new SortedList<string, Obj>();
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.labelmap)
                {
                    kouhos.Add(kv.Key, new Obj(ObjType.None));
                }
                if (obj.obj.type == ObjType.Function) return;
            }
        }
        public void KouhoSetModelGene()
        {
            kouhos = new SortedList<string, Obj>();
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    if (kv.Value.type == ObjType.ModelObj || kv.Value.type == ObjType.GeneObj) kouhos.Add(kv.Key, kv.Value);
                }
            }
        }
        public void KouhoSetSignal()
        {
            kouhos = new SortedList<string, Obj>();
            for (var i = local.blocks.Count - 1; i >= 0; i--)
            {
                var block = local.blocks[i];
                foreach (var kv in block.vmapA)
                {
                    if (kv.Value.type == ObjType.ModelObj || kv.Value.type == ObjType.GeneObj) kouhos.Add(kv.Key, kv.Value);
                }
            }
        }
        public void KouhoSetElem()
        {
            local.kouhos = new SortedList<string, Obj>();
            local.kouhos.Add("left", new Obj(ObjType.None));
            local.kouhos.Add("top", new Obj(ObjType.None));
            local.kouhos.Add("z", new Obj(ObjType.None));
            local.kouhos.Add("p", new Obj(ObjType.None));
            local.kouhos.Add("point", new Obj(ObjType.None));
            local.kouhos.Add("w", new Obj(ObjType.None));
            local.kouhos.Add("width", new Obj(ObjType.None));
            local.kouhos.Add("h", new Obj(ObjType.None));
            local.kouhos.Add("height", new Obj(ObjType.None));
            local.kouhos.Add("size", new Obj(ObjType.None));
            local.kouhos.Add("xtype", new Obj(ObjType.None));
            local.kouhos.Add("ytype", new Obj(ObjType.None));
            local.kouhos.Add("ptype", new Obj(ObjType.None));
            local.kouhos.Add("pos", new Obj(ObjType.None));
            local.kouhos.Add("anim", new Obj(ObjType.None));
            local.kouhos.Add("c", new Obj(ObjType.None));
            local.kouhos.Add("color", new Obj(ObjType.None));
            local.kouhos.Add("b", new Obj(ObjType.None));
            local.kouhos.Add("background", new Obj(ObjType.None));
            local.kouhos.Add("a", new Obj(ObjType.None));
            local.kouhos.Add("align", new Obj(ObjType.None));
            local.kouhos.Add("gokeydown", new Obj(ObjType.None));
            local.kouhos.Add("backkeydown", new Obj(ObjType.None));
            local.kouhos.Add("gomousedown", new Obj(ObjType.None));
            local.kouhos.Add("backmousedown", new Obj(ObjType.None));
            local.kouhos.Add("gomousemove", new Obj(ObjType.None));
            local.kouhos.Add("backmousemove", new Obj(ObjType.None));
            local.kouhos.Add("gomouseup", new Obj(ObjType.None));
            local.kouhos.Add("backmouseup", new Obj(ObjType.None));
        }
    }
    partial class CommentLet
    {
        public async void SingleB(SingleOp op, Local local)
        {
            if (op == null)
            {
                foreach (var elem in instances) elems[elems.Count - 1].add(elem);
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
                    foreach (var elem in instances)
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
    }
    partial class TypeCheck
    {
        public static Type CheckB(Obj val, Obj val2, CheckType type, Local local)
        {
            Type cls = null;
        head0:
            if (val.type == ObjType.If)
            {
                cls = (val as If).ret;
            }
            else if (val.type == ObjType.IfBlock)
            {
                cls = (val as IfBlock).ifobj.ret;
            }
            else if (val.type == ObjType.While)
            {
                cls = (val as While).ret;
            }
            else if (val.type == ObjType.For)
            {
                cls = (val as For).ret;
            }
            else if (val.type == ObjType.Switch)
            {
                cls = (val as Switch).ret;
            }
            else if (val.type == ObjType.Variable)
            {
                cls = (val as Variable).cls;
            }
            else if (val.type == ObjType.Function)
            {
                cls = (val as Function).ret;
            }
            else if (val.type == ObjType.SingnalFunction)
            {
                cls = (val as SignalFunction).cls;
            }
            else if (val.type == ObjType.ServerFunction)
            {
                cls = (val as ServerFunction).cls;
            }
            else throw new Exception();
            if (val2.type == ObjType.Variable)
            {
                val2 = Value.New((val2 as Variable).cls, local);
            }
            return CheckCVB(cls, val2, type, local);
        }
        public static Type CheckCVB(Type cls, Obj val2, CheckType type, Local local)
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
                                    var ret = CheckCVB(block.cls, block.rets[i], type, local);
                                    if (ret.type == ObjType.Wait || ret.type == ObjType.Error) return ret;
                                }
                                return arrtype;
                            }
                        }
                        else
                        {
                            block.cls = cls;
                            for (var i = 0; i < block.rets.Count - 1; i++)
                            {
                                var ret = CheckCVB(block.cls, block.rets[i], type, local);
                                if (ret.type == ObjType.Wait || ret.type == ObjType.Error) return ret;
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
                            var ret = EqualClassB(cls as Type, block.cls, local, cls.letter);
                            if (ret.type == ObjType.Wait || ret.type == ObjType.Error) return ret;
                        }
                        else if (ExtendCompareB(cls as Type, block.cls, local) == null) return Obj.Error(ObjType.Error, val2.letter, "共通の継承元がありません");
                    }
                    return arrtype;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "配列型は配列を引数にとります");
            }
            else if (cls.type == ObjType.FuncType)
            {
                var functype = cls as FuncType;
                if (val2.type == ObjType.Function)
                {
                    var wait = false;
                    var func = val2 as Function;
                    var ret = EqualClassB(functype.cls, func.ret as Type, local, func.letter);
                    if (ret.type == ObjType.Wait) wait = true;
                    else if (ret.type == ObjType.Error) return ret;
                    var blk = func.draw.children[0] as Block;
                    var varray = new List<Obj>(blk.vmapA.Values);
                    if (functype.draws.Count != varray.Count) return Obj.Error(ObjType.Error, blk.letter, "関数の引数があっていません");
                    for (var i = 0; i < varray.Count; i++)
                    {
                        if (varray[i].type == ObjType.Variable)
                        {
                            var variable = varray[i] as Variable;
                            ret = EqualClassB(functype.draws[i], variable.cls, local, varray[i].letter);
                            if (ret.type == ObjType.Wait) wait = true;
                            else if (ret.type == ObjType.Error) return ret;
                        }
                        else return Obj.Error(ObjType.Error, varray[i].letter, "関数の引数は変数でなければなりません");
                    }
                    if (wait) return new Type(ObjType.Wait);
                    return functype;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "戻り値は関数になっています");
            }
            else if (cls.type == ObjType.GenericObj || cls.type == ObjType.ClassObj || cls.type == ObjType.ModelObj || cls.type == ObjType.GeneObj)
            {
                var clst = cls as Type;
                if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.GeneValue)
                {
                    var cls2 = (val2 as Val).cls;
                    if (vartype2 == null)
                    {
                        return ExtendCompareB(clst, cls2, local);
                    }
                    else
                    {
                        return ExtendCompareB(vartype2, cls2, local);
                    }
                }
                else return Obj.Error(ObjType.Error, val2.letter, "値が来ていません");
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
                            if (CheckCVB(block.cls, block.rets[i], type, local) == null) throw new Exception();
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
                else return Obj.Error(ObjType.Error, val2.letter, "値ではありません");
            }
            else if (cls.type == ObjType.GeneLabel && val2.type == ObjType.GeneLabelValue) return cls;
            else return Obj.Error(ObjType.Error, val2.letter, "不正なエラー");
        }
        public static Type EqualClassB(Type cls, Type cls2, Local local, Letter let)
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
                    return new Type(ObjType.Wait);
                }
                else
                {
                    if (vart2 != null) cls2 = vart2.cls;
                    VarConnectB(vartype, cls2);
                    return cls;
                }
            }
            else if (cls2.type == ObjType.Var)
            {
                if (vart != null) cls = vart.cls;
                var vartype2 = cls2 as Var;
                VarConnectB(vartype2, cls);
                return cls;
            }
            else if (cls.type == ObjType.ArrayType)
            {
                if (cls2.type == ObjType.ArrayType)
                {
                    return EqualClassB(cls.cls, cls2.cls, local, let);
                }
                else return Obj.Error(ObjType.Error, let, "型が合ってない");
            }
            else if (cls.type == ObjType.FuncType)
            {
                var functype = cls as FuncType;
                if (cls2.type == ObjType.FuncType)
                {
                    var functype2 = cls2 as FuncType;
                    var wait = false;
                    var ret = EqualClassB(functype, cls2.cls, local, let);
                    if (ret.type == ObjType.Wait) wait = true;
                    else if (ret.type == ObjType.Error) return ret;
                    if (functype.draws.Count != functype2.draws.Count) return Obj.Error(ObjType.Error, let, "関数型の引数の数が合わない");
                    for (var i = 0; i < functype.draws.Count; i++)
                    {
                        ret = EqualClassB(functype.draws[i], functype2.draws[i], local, let);
                        if (ret.type == ObjType.Wait) wait = true;
                        else if (ret.type == ObjType.Error) return ret;
                    }
                    if (wait) return new Type(ObjType.Wait);
                    return cls;
                }
                else return Obj.Error(ObjType.Error, let, "関数型が適切です");
            }
            else if (cls.type == ObjType.GenericObj)
            {
                var gj = cls as GenericObj;
                if (cls2.type == ObjType.GenericObj)
                {
                    var gj2 = cls2 as GenericObj;
                    if (gj.gene != gj2.gene) return Obj.Error(ObjType.Error, let, "ジェネリックスが違います" );
                    if (gj.draws.Count != gj2.draws.Count) return Obj.Error(ObjType.Error, let, "ジェネリックスの引数の数が違います");
                    for (var i = 0; i < gj.draws.Count; i++)
                    {
                        var ret = EqualClassB(gj.draws[i], gj2.draws[i], local, let);
                        if (ret.type == ObjType.Wait) return ret;
                        else return ret;
                    }
                    return cls;
                }
                else return Obj.Error(ObjType.Error, let, "型が違います");
            }
            else if (cls.type == ObjType.ClassObj)
            {
                if (cls2.type == ObjType.ClassObj)
                {
                    if (cls != cls2) Obj.Error(ObjType.Error, let, "型が違います");
                    return cls;
                }
                else return Obj.Error(ObjType.Error, let, "型が違います");
            }
            else if (cls.type == ObjType.ModelObj)
            {
                if (cls2.type == ObjType.ModelObj)
                {
                    if (cls != cls2) Obj.Error(ObjType.Error, let, "型が違います");
                    return cls;
                }
                else return Obj.Error(ObjType.Error, let, "型が違います");
            }
            else if (cls.type == ObjType.GeneObj)
            {
                if (cls2.type == ObjType.GeneObj)
                {
                    if (cls != cls2) Obj.Error(ObjType.Error, let, "型が違います");
                    return cls;
                }
                else return Obj.Error(ObjType.Error, let, "型が違います");
            }
            else return Obj.Error(ObjType.Error, let, "不正なエラーです");
        }
        public static Type ExtendCompareB(Type cls1, Type cls2, Local local)
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
            ExtendsAddB(cls1, list1);
            list2.Add(cls2);
            ExtendsAddB(cls2, list2);
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
            return local.Object;
        }
        public static void ExtendsAddB(Type cls, List<Type> list)
        {
            if (cls.type == ObjType.ClassObj)
            {
                var cj = cls as ClassObj;
                for (var i = 0; i < cj.extends.Count; i++)
                {
                    list.Add(cj.extends[i]);
                }
                for (var i = 0; i < cj.extends.Count; i++)
                {
                    ExtendsAddB(cj.extends[i], list);
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
                    ExtendsAddB(gj.extends[i], list);
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
                    ExtendsAddB(gj.extends[i], list);
                }
            }
        }
        public static void VarConnectB(Var vartype, Type cls)
        {
            vartype.cls = cls;
            for (var i = 0; i < vartype.connects.Count; i++)
            {
                if (vartype.connects[i].cls == null)
                {
                    VarConnectB(vartype.connects[i], cls);
                }
            }

        }
    }
}
