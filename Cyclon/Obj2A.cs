using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Word
    {
        public override Obj SelfA(Local local)
        {
            var obj = local.getA(name);
            return obj;
        }
        public override Obj GetterA(Local local)
        {
            var obj = local.getA(name).GetterA(local);
            return obj;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
    }
    partial class Value
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    n++;
                    return this;
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後は名前を指定してください");
            }
            else if (val2.type == ObjType.Bracket)
            {
                var blk = val2.exeA(local) as Block;
                n++;
                return this;
            }
            else if (val2.type == ObjType.Block)
            {
                var blk = val2.exeA(local) as Block;
                n++;
                return this;
            }
            return Obj.Error(ObjType.NG, val2.letter, "値の後続が適切ではないです");
        }
        public override string Info()
        {
            return "val:" + cls.letter.name;
        }
    }
    partial class Class
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            Obj obj = new ClassObj();
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                obj = local.getA(word.name);
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Block)
                {
                    var generic = obj as Generic;
                    var val = val2.exeA(local);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    var block = val as Block;
                    local.blocks[local.blocks.Count - 1].childblocks.Add(block);
                    local.blocks.Add(block);
                    for (var i = 0; i < block.rets.Count; i++)
                    {
                        val2 = block.rets[i];
                        if (val2.type == ObjType.Word)
                        {
                            generic.vmap[(val2 as Word).name] = new VariClass(i);
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "ジェネリッククラスの引数が適切ではありません");
                    }
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    generic.block = block;
                    n++;
                    val2 = primary.children[n];
                    obj = generic;
                }
                obj.letter = word.letter;
            }
        head:
            if (val2.type == ObjType.Left)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Bracket)
                {
                    val2 = val2.exeA(local).GetterA(local);
                    if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                    var blk = val2 as Block;
                    for (var i = 0; i < blk.rets.Count; i++)
                    {
                        if (blk.rets[i].type == ObjType.ClassObj || blk.rets[i].type == ObjType.GenericObj)
                        {
                            var cls = blk.rets[i] as Type;
                            if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(cls);
                            else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(cls);
                        }
                        else return Obj.Error(ObjType.Error, blk.rets[i].letter, "継承元はクラスかジェネリックスでなければなりません");
                    }
                }
                if (val2.type == ObjType.ClassObj)
                {
                    var cls = val2 as ClassObj;
                    if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(cls);
                    else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(cls);
                    else return Obj.Error(ObjType.Error, val2.letter, "継承元のクラスはクラスとジェネリッククラスだけです");
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
                        val2 = val2.exeA(local).GetterA(local);
                        if (val2.type == ObjType.NG || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                        var block = val2 as Block;
                        var genericobj = new GenericObj(generic);
                        for (var i = 0; i < block.rets.Count; i++)
                        {
                            val2 = block.rets[i];
                            if (val2 is Type)
                            {
                                genericobj.draws.Add(val2 as Type);
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスクラスの引数は型でなければならない");
                        }
                        n++;
                        val2 = primary.children[n];
                        if (generic.vmap.Count != genericobj.draws.Count) return Obj.Error(ObjType.Error, val2.letter, "ジェネリッククラスの引数の数が合っていません");
                        if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(genericobj);
                        else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(genericobj);
                        else return Obj.Error(ObjType.Error, val2.letter, "継承はクラスかジェネリッククラスだけです。");
                        goto head;
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "ジェネリッククラスは[]で引数をもたなければならないです");
                }
                else return Obj.Error(ObjType.Error, val2.letter, "クラスの継承元はクラスでなければなりません");
            }
            if (val2.type == ObjType.CallBlock)
            {
                if (obj.type == ObjType.Generic)
                {
                    var generic = obj as Generic;
                    local.declareA2(val2 as CallBlock, generic);
                    generic.letter2 = (val2.children[1] as Block).letter2;
                    generic.draw = val2 as CallBlock;
                    var blk1 = val2.children[0] as Block;
                    blk1.obj = new ObjBlock() { obj = obj, n = 0 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                    local.blocks.Add(blk1);
                    for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                    blk1.exeA(local);
                    var blk2 = val2.children[1] as Block;
                    if (generic.extends.Count > 0)
                    {
                        blk2.vmapA["base"] = new Base(null, generic.extends);
                    }
                    blk2.obj = new ObjBlock() { obj = obj, n = 1 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                    local.blocks.Add(blk2);
                    for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                    blk2.exeA(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                }
                else if (obj.type == ObjType.ClassObj)
                {
                    var cbj = obj as ClassObj;
                    local.declareA2(val2 as CallBlock, cbj);
                    cbj.letter2 = (val2.children[1] as Block).letter2;
                    cbj.draw = val2 as CallBlock;
                    var blk1 = val2.children[0] as Block;
                    blk1.obj = new ObjBlock() { obj = cbj, n = 0 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                    local.blocks.Add(blk1);
                    for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                    blk1.exeA(local);
                    var blk2 = val2.children[1] as Block;
                    if (cbj.extends.Count > 0)
                    {
                        blk2.vmapA["base"] = new Base(null, cbj.extends);
                    }
                    blk2.obj = new ObjBlock() { obj = obj, n = 1 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                    local.blocks.Add(blk2);
                    for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                    blk2.exeA(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                }
                n++;
                return obj;
            }
            else return Obj.Error(ObjType.NG, val2.letter, "クラスの宣言の{|}が抜けています");
        }

    }
    partial class Type
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
                else return Obj.Error(ObjType.Error, val2.letter, "配列型は引数の数が0です");
            }
            else if (val2.type == ObjType.Left)
            {
                var functype = new FuncType(type);
                type = functype;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Bracket)
                {
                    val2 = val2.exeA(local).GetterA(local);
                    if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                    var blk = val2 as Block;
                    for (var i = 0; i < blk.rets.Count; i++)
                    {
                        val2 = blk.rets[i];
                        if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                        else if (val2.type == ObjType.ClassObj || val2.type == ObjType.GenericObj)
                        {
                            functype.draws.Add(val2 as Type);
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "継承元はクラスでなければならない");
                    }
                    n++;
                    val2 = primary.children[n];
                }
                else
                {
                    val2 = val2.GetterA(local);
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
                            val2 = val2.exeA(local).GetterA(local);
                            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                            var blk = val2 as Block;
                            var genericobj = new GenericObj(generic);
                            for (var i = 0; i < blk.rets.Count; i++)
                            {
                                val2 = blk.rets[i];
                                if (val2 is Type)
                                {
                                    genericobj.draws.Add(val2 as Type);
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスクラスの引数は型でなければいけません");
                            }
                            if (generic.vmap.Count != genericobj.draws.Count) return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスクラスの引数が合っていません");
                            n++;
                            val2 = primary.children[n];
                            functype.draws.Add(genericobj);
                            goto head;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスクラスの引数として[]をお願いします");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "継承元はクラスでなければならない");
                }
            }
            Word word = null;
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word2 = val2 as Word;
                    n++;
                    if (type.type == ObjType.ClassObj || type.type == ObjType.ArrayType)
                    {
                        if (word2.name == "new") return new Constructor(type as ClassObj);
                        else return Obj.Error(ObjType.Error, val2.letter, word2.name + "は" + type.letter.name + "の後続にこれません");
                    }
                    else if (type.type == ObjType.ModelObj || type.type == ObjType.GeneObj)
                    {
                        if (word2.name == "new") return new Constructor(type as ClassObj);
                        else return Obj.Error(ObjType.Error, val2.letter, word2.name + "は" + type.letter.name + "の後続にこれません");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "関数型は.が来れません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には単語が必要です");
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
                    else return Obj.Error(ObjType.Error, val2.letter, "コメント変数はコメントの中に宣言してください。");
                }
                else return Obj.Error(ObjType.Error, val2.letter, "コメント変数の名前を指定してください。");
            }
            else if (val2.type == ObjType.Word)
            {
                word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var last = local.blocks.Last().obj;
                if (last.obj.type == ObjType.ModelObj || last.obj.type == ObjType.GeneObj)
                {
                    var obj = type;
                head2:
                    if (obj.type == ObjType.ClassObj || obj.type == ObjType.FuncType) return Obj.Error(ObjType.Error, val2.letter, "model,geneの中ではmodelかgeneの型しか宣言できません");
                    else if (obj.type == ObjType.ArrayType)
                    {
                        obj = (obj as ArrType).cls;
                        goto head2;
                    }
                }
            }
            else if (val2.type == ObjType.Bracket)
            {
                var blk = val2.exeA(local) as Block;
                n++;

                for (var i = 0; i < blk.rets.Count; i++)
                {
                    val2 = blk.rets[i];
                    if (val2.type == ObjType.Word)
                    {
                        var variable = new Variable(type);
                        local.declareB((val2 as Word).name, variable);
                        variable.letter = val2.letter;
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[i].letter, "変数宣言は名前だけです");
                }
                return blk;
            }
            if (val2.type == ObjType.Block)
            {
                var func = new GenericFunction(type);
                var val = val2.exeA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val.exeA(local) as Block;
                for (var i = 0; i < blk.rets.Count; i++)
                {
                    if (blk.rets[i].type == ObjType.Word)
                    {
                        var word2 = blk.rets[i] as Word;
                        func.vmap[word2.name] = new VariClass(i);
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[i].letter, "ジェネリック関数の宣言は名前しか引数をとれません");               }
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    func.draw = val2 as CallBlock;
                    local.declareA2(func.draw, func);
                    var blk1 = val2.children[0] as Block;
                    blk1.obj = new ObjBlock() { obj = func, n = 0 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                    local.blocks.Add(blk1);
                    for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                    blk1.exeA(local);
                    var blk2 = val2.children[1] as Block;
                    blk2.obj = new ObjBlock() { obj = func, n = 1 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                    local.blocks.Add(blk2);
                    for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                    blk2.exeA(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    n++;
                    if (word != null) local.declareA(word.name, func);
                    return func;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "ジェネリック関数の{|}が指定されていません");
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var func = new Function(type);
                func.draw = val2 as CallBlock;
                local.declareA2(func.draw, func);
                var blk1 = val2.children[0] as Block;
                blk1.obj = new ObjBlock() { obj = func, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                local.blocks.Add(blk1);
                for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                blk1.exeA(local);
                var blk2 = val2.children[1] as Block;
                blk2.obj = new ObjBlock() { obj = func, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                local.blocks.Add(blk2);
                for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                blk2.exeA(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                foreach (var b in local.blocks) func.blocks.Add(b);
                n++;
                if (word != null)
                {
                    local.declareA(word.name, func);
                    func.letter = word.letter;
                }
                else func.letter = val2.letter;
                return func;
            }
            else
            {
                if (word == null) return type;
                else
                {
                    var variable = new Variable(type);
                    local.declareA(word.name, variable);
                    variable.letter = word.letter;
                head1:
                    if (val2.type == ObjType.Left)
                    {
                        var func = new Function(this);
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var gj = local.blocks.Last().obj.obj as Type;
                            if (gj.type != ObjType.GeneObj && gj.type == ObjType.ModelObj) gj = null;
                            var val = val2.GetterA(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var func2 = val as Function;
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
                            goto head1;
                        }
                        else if (val2.type == ObjType.CallBlock)
                        {
                            var gj = local.blocks.Last().obj.obj as Type;
                            if (gj.type != ObjType.GeneObj && gj.type == ObjType.ModelObj) gj = null;
                            func.draw = val2 as CallBlock;
                            func.letter = val2.letter;
                            local.declareA2(func.draw, func);
                            var blk1 = val2.children[0] as Block;
                            blk1.obj = new ObjBlock() { obj = func, n = 0 };
                            local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                            local.blocks.Add(blk1);
                            for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                            blk1.exeA(local);
                            var blk2 = val2.children[1] as Block;
                            blk2.vmapA["v1"] = Value.New(type, local);
                            blk2.vmapA["v2"] = Value.New(type, local);
                            blk2.vmapA["g1"] = Value.New(gj, local);
                            blk2.vmapA["g2"] = Value.New(gj, local);
                            blk2.obj = new ObjBlock() { obj = func, n = 1 };
                            local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                            local.blocks.Add(blk2);
                            for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                            blk2.exeA(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            variable.right = func;
                            variable.left = func;
                            n++;
                            val2 = primary.children[n];
                            goto head1;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの{|}を指定してください");
                    }
                    else if (val2.type == ObjType.Right)
                    {
                        var func = new Function(this);
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var val = val2.GetterA(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var func2 = val as Function;
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
                        else if (val2.type == ObjType.CallBlock)
                        {
                            var gj = local.blocks.Last().obj.obj as Type;
                            if (gj.type != ObjType.GeneObj && gj.type == ObjType.ModelObj) gj = null;
                            func.draw = val2 as CallBlock;
                            func.letter = val2.letter;
                            local.declareA2(func.draw, func);
                            n++;
                            val2 = primary.children[n];
                            var blk1 = val2.children[0] as Block;
                            blk1.obj = new ObjBlock() { obj = func, n = 0 };
                            local.blocks[local.blocks.Count - 1].returns.Add(blk1);
                            local.blocks.Add(blk1);
                            for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                            blk1.exeA(local);
                            var blk2 = val2.children[1] as Block;
                            blk2.obj = new ObjBlock() { obj = func, n = 1 };
                            local.blocks[local.blocks.Count - 1].returns.Add(blk2);
                            local.blocks.Add(blk2);
                            for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                            blk2.exeA(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            variable.right = func;
                            goto head1;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの{|}を指定してください");
                    }
                    return variable;
                }
            }
        }
    }
    partial class Var
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class ClassObj
    {
        public override string Info()
        {
            var text = "cbj:" + letter.name;
            return text;
         }
    }
    partial class Generic
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                var val = val2.exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                var geneobj = new GenericObj(this);
                if (this.block.vmap.Count != block.rets.Count) return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスクラスの引数があっていません。");
                for (var i = 0; i < block.rets.Count; i++)
                {
                    if (block.rets[i] is Type)
                    {
                        geneobj.draws.Add(block.rets[i] as Type);
                    }
                    else return Obj.Error(ObjType.Error, block.rets[i].letter, "ジェネリックスクラスは型を引数にとる必要があります。");
                }
                var list = new List<Obj>(this.block.vmap.Values);
            }
            return Obj.Error(ObjType.NG, val2.letter, "ジェネリッククラスは[]を引数にとります");
        }
    }
    partial class Number
    {
        public override Obj exeA(Local local)
        {
            return this;
        }

        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
                            if (val2.children.Count != 0) return Obj.Error(ObjType.Error, val2.letter, "randomの引数は0個です");
                            return new Number(0) { cls = local.Int };
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "randomの()が指定されていません");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, word.name + "は適切な後続の単語ではありません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後に単語が来ていません");
            }
            return Obj.Error(ObjType.NG, val2.letter, ".が来ていません");
        }
        public override string Info()
        {
            return "int **";
        }

    }
    partial class FloatVal
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "小数には後続が来れません");
        }
        public override string Info()
        {
            return "float **";
        }
    }
    partial class StrObj
    {
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "文字列には後続が来れません");
        }
        public override string Info()
        {
            return "strobj **";
        }
    }
    partial class BoolVal
    {
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "bool値には後続がこれません");
        }
        public override string Info()
        {
            return "bool **";
        }
    }
    partial class VoiVal
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return new Obj(ObjType.NG);
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "void値には後続がこれません");
        }
        public override string Info()
        {
            return "void **";
        }
    }
    partial class Null
    {
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "nullには後続が来ません");
        }

    }
    partial class Constructor
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                val2 = val2.Clone().exeA(local).GetterA(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                var blk = val2 as Block;
                var block1 = cls.draw.children[0] as Block;
                block1.exeRangeA(0, block1.children.Count, local, false);
                n++;
                return new Value(cls);
            }
            val2 = val2.GetterA(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {

                var block1 = cls.draw.children[0] as Block;
                block1.exeRangeA(0, block1.children.Count, local, false);
                n++;
                return new Value(cls);
            }
            return Obj.Error(ObjType.NG, val2.letter, "コンストラクタの引数が適切ではありません");
        }
    }
    partial class Base
    {

        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
            if (val2.type == ObjType.Bracket)
            {
                val2 = val2.Clone().exeA(local).GetterA(local);
                if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                var blk = val2.exeA(local) as Block;

                n++;
                return new VoiVal();
            }
            val2 = val2.GetterA(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {
                n++;
                return new VoiVal();
            }
            return Obj.Error(ObjType.Error, val2.letter, "baseの引数が適切に設定されていません");
        }
    }
    partial class Variable
    {
        public override Obj GetterA(Local local)
        {
            if (value != null) return value.GetterA(local);
            else return new Null();
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return new Value(cls).PrimaryA(ref n, local, primary, val2);
        }
        public override string Info()
        {
            if (cls.type == ObjType.Var)
            {
                var varobj = cls as Var;
                if (varobj.cls != null) return "var:" + letter.name + " " + varobj.cls.letter.name;
            }
            return "var:" + letter.name + " " + cls.letter.name;
        }

    }
    partial class Function
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (bracket != null) throw new Exception();
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local).Getter(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                return new Value(ret);
            }
            else if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {
                return new Value(ret);
            }
            return Obj.Error(ObjType.NG, val2.letter, "()のような関数に適切な引数が来ていません");
        }
        public override string Info()
        {
            return "fun:" + letter.name + " " + ret.letter.name;
        }
    }
    class IfBlock: Obj
    {
        public CallBlock call;
        public Obj iflabel;
        public If ifobj;
        public IfBlock() : base(ObjType.IfBlock)
        {

        }
        public override string Info()
        {
            return "if :" + iflabel.letter.name;
        }
    }
    partial class If
    {
        public List<IfBlock> calls = new List<IfBlock>();
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override string Info()
        {
            return "if :" + letter.name;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var ifb = new IfBlock() { call = val2 as CallBlock, iflabel = this, ifobj = this };
                calls.Add(ifb);
                var block1 = val2.children[0] as Block;
                block1.obj = new ObjBlock() { obj = ifb, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block1);
                local.blocks.Add(block1);
                for (var i = 0; i < local.blocks.Count; i++) block1.blocks.Add(local.blocks[i]);
                block1.exeA(local);
                var ret = block1.rets.Last().GetterA(local);
                var block2 = val2.children[1] as Block;
                block2.obj = new ObjBlock() { obj = ifb, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block2);
                local.blocks.Add(block2);
                for (var i = 0; i < local.blocks.Count; i++) block2.blocks.Add(local.blocks[i]);
                block2.exeA(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                val2 = primary.children[n];
            head:
                var elif = val2;
                if (val2.type == ObjType.Elif)
                {
                    var iflabel = val2;
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        ifb = new IfBlock() { call = val2 as CallBlock, iflabel = iflabel, ifobj = this };
                        calls.Add(ifb);
                        block1 = val2.children[0] as Block;
                        block1.obj = new ObjBlock() { obj = ifb, n = 0 };
                        local.blocks[local.blocks.Count - 1].childblocks.Add(block1);
                        local.blocks.Add(block1);
                        for (var i = 0; i < local.blocks.Count; i++) block1.blocks.Add(local.blocks[i]);
                        block1.exeA(local);
                        ret = block1.rets.Last().GetterA(local);
                        var block22 = val2.children[1] as Block;
                        block22.obj = new ObjBlock() { obj = ifb, n = 1 };
                        local.blocks[local.blocks.Count - 1].childblocks.Add(block22);
                        local.blocks.Add(block22);
                        for (var i = 0; i < local.blocks.Count; i++) block22.blocks.Add(local.blocks[i]);
                        block22.exeA(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        n++;
                        val2 = primary.children[n];
                        goto head;
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "elif文の中身が宣言されていません。");

                }
                else if (val2.type == ObjType.Else)
                {
                    var iflabel = val2;
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        ifb = new IfBlock() { call = val2 as CallBlock, iflabel = iflabel, ifobj = this };
                        calls.Add(ifb);
                        block1 = val2.children[0] as Block;
                        block1.obj = new ObjBlock() { obj = ifb, n = 0 };
                        local.blocks[local.blocks.Count - 1].childblocks.Add(block1);
                        local.blocks.Add(block1);
                        for (var i = 0; i < local.blocks.Count; i++) block1.blocks.Add(local.blocks[i]);
                        block1.exeA(local);
                        var block22 = val2.children[1] as Block;
                        block22.obj = new ObjBlock() { obj = ifb, n = 0 };
                        local.blocks[local.blocks.Count - 1].childblocks.Add(block22);
                        local.blocks.Add(block22);
                        for (var i = 0; i < local.blocks.Count; i++) block22.blocks.Add(local.blocks[i]);
                        block22.exeA(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        n++;
                        return new Value(local.Int);
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "else文の中身が宣言されていません。");
                }
                else return new Value(local.Int);
            }
            return Obj.Error(ObjType.Error, val2.letter, "if文の中身が宣言されていません");
        }
    }
    partial class While
    {
        public override string Info()
        {
            return "whi:" + letter.name;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                local.declareA2(val2 as CallBlock, this);
                var block1 = val2.children[0] as Block;
                block1.obj = new ObjBlock() { obj = this, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block1);
                local.blocks.Add(block1);
                for (var i = 0; i < local.blocks.Count; i++) block1.blocks.Add(local.blocks[i]);
                block1.exeA(local);
            head:
                var block2 = val2.children[1] as Block;
                block2.obj = new ObjBlock() { obj = this, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block2);
                local.blocks.Add(block2);
                for (var i = 0; i < local.blocks.Count; i++) block2.blocks.Add(local.blocks[i]);
                block2.exeA(local);
                var retobj = block2.rets.Last().Getter(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                return retobj;
            }
            return Obj.Error(ObjType.Error, val2.letter, "while文の中身が宣言されていません");
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class For
    {
        public override string Info()
        {
            return "for:" + letter.name;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var block1 = val2.children[0] as Block;
                block1.obj = new ObjBlock() { obj = this, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block1);
                local.blocks.Add(block1);
                for (var i = 0; i < local.blocks.Count; i++) block1.blocks.Add(local.blocks[i]);
                var loop = false;
                var result = false;
                block1.exeA(local);
                var block2 = val2.children[1] as Block;
                block2.obj = new ObjBlock() { obj = this, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block2);
                local.blocks.Add(block2);
                for (var i = 0; i < local.blocks.Count; i++) block2.blocks.Add(local.blocks[i]);
                block2.exeA(local);
                var retobj = block2.rets.Last().Getter(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                return retobj;
            }
            return Obj.Error(ObjType.Error, val2.letter, "for文の中身が宣言されていません。");
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class Switch
    {
        public override string Info()
        {
            return "swi:" + letter.name;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                var block1 = val2.children[0] as Block;
                block1.obj = new ObjBlock() { obj = this, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block1);
                local.blocks.Add(block1);
                for (var i = 0; i < local.blocks.Count; i++) block1.blocks.Add(local.blocks[i]);
                block1.exeA(local);
                var block2 = val2.children[1] as Block;
                block2.obj = new ObjBlock() { obj = this, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(block2);
                local.blocks.Add(block2);
                for (var i = 0; i < local.blocks.Count; i++) block2.blocks.Add(local.blocks[i]);
                block2.exeA(local);
                var retobj = block2.rets.Last();
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
                return retobj;
            }
            return Obj.Error(ObjType.Error, val2.letter, "switch文の中身が宣言されていません");
        }
    }
    partial class Break
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
        head:
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val2 as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            val2 = val2.exeA(local).GetterA(local);
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block)
            {
                n++;
                value = val2;
                for (var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj.obj.type == ObjType.While || block.obj.obj.type == ObjType.For || block.obj.obj.type == ObjType.Switch)
                    {
                        n--;
                        return this;
                    }
                    else if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.ClassObj || block.obj.obj.type == ObjType.ModelObj || block.obj.obj.type == ObjType.GeneObj || block.obj.obj.type == ObjType.GenericFunction)
                    {
                        return Obj.Error(ObjType.Error, letter, "while,for,switchの中ではありません");
                    }
                }
                return Obj.Error(ObjType.Error, letter, "while,for,switchの中ではありません");
            }
            n--;
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
        head:
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val2 as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            val2 = val2.exeA(local).GetterA(local);
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block)
            {
                n++;
                value = val2;
                for (var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj.obj.type == ObjType.While || block.obj.obj.type == ObjType.For || block.obj.obj.type == ObjType.Switch)
                    {
                        return this;
                    }
                    else if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.ClassObj || block.obj.obj.type == ObjType.ModelObj || block.obj.obj.type == ObjType.GeneObj || block.obj.obj.type == ObjType.GenericFunction)
                    {
                        return Obj.Error(ObjType.Error, letter, "while,for,switchの中ではありません");
                    }
                }
                return Obj.Error(ObjType.Error, letter, "while,for,switchの中ではありません");
            }
            return Obj.Error(ObjType.NG, letter, "breakの戻り値を適切に指定してください");
        }
    }
    partial class Continue
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class Return
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
        head:
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            val2 = val2.GetterA(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block || val2.type == ObjType.GeneValue || val2.type == ObjType.Comment)
            {
                n++;
                value = val2;
                for (var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj != null)
                    {
                        if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.GenericFunction || block.obj.obj.type == ObjType.ServerFunction)
                        {
                            n--;
                            return this;
                        }
                    }
                }
                return Obj.Error(ObjType.Error, letter, "関数の中ではありません");
            }
            n--;
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
        head:
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            val2 = val2.GetterA(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block || val2.type == ObjType.GeneValue || val2.type == ObjType.Comment)
            {
                n++;
                value = val2;
                for (var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj != null)
                    {
                        if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.GenericFunction || block.obj.obj.type == ObjType.ServerFunction)
                        {
                            return this;
                        }
                    }
                }
                return Obj.Error(ObjType.Error, val2.letter, "関数の中ではありません");
            }
            return Obj.Error(ObjType.NG, letter, "returnの戻り値を適切に指定してください");
        }
    }
    partial class Goto
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
            return Obj.Error(ObjType.Error, val2.letter, "goto文の行き先を指定してください。");
        }
    }
    partial class Print
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket || val2.type == ObjType.Block)
            {
                var val = val2.exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (val2.type == ObjType.Bracket) local.console.Text += "(";
                else local.console.Text += "[";
                if (val2.type == ObjType.Bracket) local.console.Text += ")\n";
                else local.console.Text += "]\n";
                n++;
                return new VoiVal();
            }
            val2 = val2.GetterA(local);
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
            return Obj.Error(ObjType.Error, val2.letter, "()でprintの引数を指定してください");
        }
    }
    partial class Iterator
    {
    }
    partial class GenericFunction
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                var val = val2.GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                if (blk.rets.Count != vmap.Count) return Obj.Error(ObjType.Error, val2.let, "引数の数があっていません。");
                for (var i = 0; i < blk.rets.Count; i++)
                {
                    if (blk.rets[i] as Type == null) return Obj.Error(ObjType.Error, blk.rets[i].letter, "ジェネリック関数には型を引数にしてください");
                }
                n++;
                val2 = primary.children[n];
                return new Function(ret) { blocks = blocks, draw = draw, bracket = bracket, block = blk };
            }
            return Obj.Error(ObjType.NG, val2.letter, "[]でジェリック関数の引数を指定してください");
        }
    }
    partial class Model
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
                    if (val2.type == ObjType.Dot)
                    {
                        var modelobj = local.getA(word.name) as ModelObj;
                        if (modelobj == null) return Obj.Error(ObjType.Error, word.letter, "これはmodelオブジェではありません");
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            var word2 = val2 as Word;
                            n++;
                            val2 = primary.children[n];
                            if (val2.type == ObjType.Bracket)
                            {
                                var val = val2.exeA(local).GetterA(local);
                                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                var blk = val as Block;
                                if (word2.name == "First")
                                {
                                    if (blk.rets.Count != 0) return Obj.Error(ObjType.Error, blk.letter, "Firstは引数が1つです");
                                }
                                else if (word2.name == "Select")
                                {
                                    if (blk.rets.Count != 1) return Obj.Error(ObjType.Error, blk.letter, "Selectの引数は1つです");
                                }
                                else if (word2.name == "Sort")
                                {
                                    if (blk.rets.Count != 1) return Obj.Error(ObjType.Error, blk.letter, "Sortの引数は1つです");
                                }
                                else return Obj.Error(ObjType.Error, word2.letter, "First,Select,Sortがきます");
                            }
                            else return Obj.Error(ObjType.NG, val2.letter, "()がきます");
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
                    }
                    else if (word.name == "Store")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.exeA(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            return new VoiVal();
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "Storeの後に()がありません");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "modelには" + word.name + "は宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の次には名前が来ます");
            }
            else if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var gj = local.getA(word.name) as ModelObj;
                local.declare(word.name, gj);
                gj.letter = word.letter;
                if (val2.type == ObjType.CallBlock)
                {
                    gj.letter2 = (val2.children[1] as Block).letter2;
                    gj.draw = val2 as CallBlock;
                    var blk1 = val2.children[0] as Block;
                    blk1.obj = new ObjBlock() { obj = gj, n = 0 };
                    for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                    local.blocks.Add(blk1);
                    blk1.exeA(local);
                    var blk2 = val2.children[1] as Block;
                    blk2.obj = new ObjBlock() { obj = gj, n = 1 };
                    for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                    local.blocks.Add(blk2);
                    blk2.exeA(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    n++;
                    return gj;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "modelの内容を{}で宣言してください。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "model名を宣言してください。");
        }
    }
    partial class ModelObj
    {
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override string Info()
        {
            var text = "mod :" + letter.name;
            return text;
        }
    }
    partial class ModelVal
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
                            var val = val2.exeA(local).GetterA(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var block = val as Block;
                            n++;
                            if (block.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "updateの引数は1です");
                            return this;
                        }
                        else if (val2.type == ObjType.Stock)
                        {
                            return this;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "updateの引数はDataStockを取ります");
                    }
                    else if (vmap.ContainsKey(word.name)) return vmap[word.name];
                    else return Obj.Error(ObjType.Error, word.letter, "その単語は変数として宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後は単語を宣言してください。");
            }
            return Obj.Error(ObjType.NG, val2.let, ".が来ます");
        }
        public override string Info()
        {
            return "mval:" + cls.letter.name;
        }
    }
    partial class Stock
    {
    }
    partial class Gene
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var gj = local.getA(word.name) as GeneObj;
                gj.letter = word.letter;
                if (val2.type == ObjType.CallBlock)
                {
                    gj.letter2 = (val2.children[1] as Block).letter2;
                    foreach (var blk in local.blocks) gj.blocks.Add(blk);
                    gj.call = val2 as CallBlock;
                    var blk1 = val2.children[0] as Block;
                    blk1.obj = new ObjBlock() { obj = gj, n = 0 };
                    for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                    local.blocks.Add(blk1);
                    blk1.exeA(local);
                    var blk2 = val2.children[1] as Block;
                    blk2.obj = new ObjBlock() { obj = gj, n = 1 };
                    for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                    local.blocks.Add(blk2);
                    blk2.exeA(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    n++;
                head:
                    if (val2.type == ObjType.Left)
                    {
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.CallBlock)
                        {
                            var blk3 = val2.children[0] as Block;
                            blk3.obj = new ObjBlock() { obj = gj, n = 2 };
                            for (var i = 0; i < local.blocks.Count; i++) blk3.blocks.Add(local.blocks[i]);
                            local.blocks[local.blocks.Count - 1].childblocks.Add(blk3);
                            local.blocks.Add(blk3);
                            blk3.exeA(local);
                            var blk4 = val2.children[1] as Block;
                            blk4.obj = new ObjBlock() { obj = gj, n = 3 };
                            for (var i = 0; i < local.blocks.Count; i++) blk4.blocks.Add(local.blocks[i]);
                            local.blocks[local.blocks.Count - 1].childblocks.Add(blk4);
                            local.blocks.Add(blk4);
                            blk4.exeA(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            gj.left = val2 as CallBlock;
                            n++;
                            goto head;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの機能指定には{|}が必要です");
                    }
                    else if (val2.type == ObjType.Right)
                    {
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.CallBlock)
                        {
                            var blk3 = val2.children[0] as Block;
                            blk3.obj = new ObjBlock() { obj = gj, n = 2 };
                            for (var i = 0; i < local.blocks.Count; i++) blk3.blocks.Add(local.blocks[i]);
                            local.blocks[local.blocks.Count - 1].childblocks.Add(blk3);
                            local.blocks.Add(blk3);
                            blk3.exeA(local);
                            var blk4 = val2.children[1] as Block;
                            blk4.obj = new ObjBlock() { obj = gj, n = 3 };
                            for (var i = 0; i < local.blocks.Count; i++) blk4.blocks.Add(local.blocks[i]);
                            local.blocks[local.blocks.Count - 1].childblocks.Add(blk4);
                            local.blocks.Add(blk4);
                            blk4.exeA(local);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            local.blocks.RemoveAt(local.blocks.Count - 1);
                            gj.right = val2 as CallBlock;
                            n++;
                            goto head;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの機能指定には{|}が必要です");
                    }
                    return gj;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "{|}を宣言してください。");
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var gj = local.gene.vmap[""] as GeneObj;
                gj.letter = val2.letter;
                gj.call = val2 as CallBlock;
                foreach (var blk in local.blocks) gj.blocks.Add(blk);
                var blk1 = val2.children[0] as Block;
                blk1.obj = new ObjBlock() { obj = gj, n = 0 };
                for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                local.blocks.Add(blk1);
                blk1.exeA(local);
                var blk2 = val2.children[1] as Block;
                blk2.obj = new ObjBlock() { obj = gj, n = 1 };
                for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                local.blocks.Add(blk2);
                blk2.exeA(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                n++;
            head:
                if (val2.type == ObjType.Left)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        var blk3 = val2.children[0] as Block;
                        blk3.obj = new ObjBlock() { obj = gj, n = 2 };
                        for (var i = 0; i < local.blocks.Count; i++) blk3.blocks.Add(local.blocks[i]);
                        local.blocks[local.blocks.Count - 1].childblocks.Add(blk3);
                        local.blocks.Add(blk3);
                        blk3.exeA(local);
                        var blk4 = val2.children[1] as Block;
                        blk4.obj = new ObjBlock() { obj = gj, n = 3 };
                        for (var i = 0; i < local.blocks.Count; i++) blk4.blocks.Add(local.blocks[i]);
                        local.blocks[local.blocks.Count - 1].childblocks.Add(blk4);
                        local.blocks.Add(blk4);
                        blk4.exeA(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        gj.left = val2 as CallBlock;
                        n++;
                        goto head;
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの機能指定には{|}が必要です");
                }
                else if (val2.type == ObjType.Right)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        var blk3 = val2.children[0] as Block;
                        blk3.obj = new ObjBlock() { obj = gj, n = 2 };
                        for (var i = 0; i < local.blocks.Count; i++) blk3.blocks.Add(local.blocks[i]);
                        local.blocks[local.blocks.Count - 1].childblocks.Add(blk3);
                        local.blocks.Add(blk3);
                        blk3.exeA(local);
                        var blk4 = val2.children[1] as Block;
                        blk4.obj = new ObjBlock() { obj = gj, n = 3 };
                        for (var i = 0; i < local.blocks.Count; i++) blk4.blocks.Add(local.blocks[i]);
                        local.blocks[local.blocks.Count - 1].childblocks.Add(blk4);
                        local.blocks.Add(blk4);
                        blk4.exeA(local);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        local.blocks.RemoveAt(local.blocks.Count - 1);
                        gj.right = val2 as CallBlock;
                        n++;
                        goto head;
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの機能指定には{|}が必要です");
                }
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
                    if (local.gene.vmap.ContainsKey(word.name))
                    {
                        return local.gene.vmap[word.name];
                    }
                    if (local.gene.vmap.ContainsKey(""))
                    {
                        var gj = local.gene.vmap[""] as GeneObj;
                        return gj.DotA(word.name);
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "無名Geneクラスは宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前が必要です");
            }
            return Obj.Error(ObjType.NG, val2.letter, "geneの後に適切な単語が来ていません");
        }
    }
    partial class GeneObj
    {
        public Obj DotA(String name)
        {
            if (vmap.ContainsKey(name)) return vmap[name];
            return gv.vmap[name];
        }
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override string Info()
        {
            var text = "gene:" + letter.name;
            return text;
        }
    }
    partial class GeneChild
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            List<Obj> rets = new List<Obj>();
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return Value.New(gj, local);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Childの引数は0です。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneMutate
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return Value.New(gj, local);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Mutateの引数は0です。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneNew
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return Value.New(gj, local);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Newの引数は0です。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneCross
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 2)
                {
                    var val = val2.Clone().exeA(local).GetterA(local);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    n++;
                    return Value.New(gj, local);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Crossの引数は2です。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneVal
    {
        public override string Info()
        {
            return "gval:" + cls.letter.name;
        }
    }
    partial class GeneStore
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket) {
                if (val2.children.Count == 0)
                {
                    return Value.New(gj, local);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Storeの引数は0です。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneSelect
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                if (val2.children.Count == 0 || val2.children.Count == 1)
                {
                    n++;
                    return new Block(ObjType.Block);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Selectの引数があっていません");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneSort
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                var val = val2.exeA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                if (val2.children.Count == 0 || val2.children.Count == 1)
                {
                    n++;
                    return new Block(ObjType.Block);
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Sortの引数があっていません");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneLabel
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class GeneLabelValue
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
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
                    else return Obj.Error(ObjType.Error, val2.letter, "このGeneLabelには" + word.name + "は宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後は名前でないといけません");
            }
            return Obj.Error(ObjType.NG, val2.letter, ".が必要です。");
        }
    }
    partial class NewBlock
    {
    }
    partial class CrossBlock
    {
    }
    partial class MigrateBlock
    {
    }
}
