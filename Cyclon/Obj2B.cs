using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Word
    {
        public override Obj SelfB(Local local)
        {
            var obj = local.getB(name, letter).Clone();
            obj.letter = letter;
            return obj;
        }
        public override Obj GetterB(Local local)
        {
            var obj = local.getB(name, letter).GetterB(local).Clone();
            obj.letter = letter;
            return obj;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            n++;
            return this;
        }
    }
    partial class Value
    {
        public Value(Type cls) : base(ObjType.Value)
        {
            this.cls = cls;
        }
        public  static Val New(Type cls, Local local)
        {
            if (cls.type == ObjType.Var)
            {
                var varobj = cls as Var;
                if (varobj.cls == null) return new Val(ObjType.Wait);
                cls = varobj.cls;
            }
            if (cls == local.Int) return new Number(0) { cls = local.Int };
            else if (cls == local.Str) return new StrObj("") { cls = local.Str };
            else if (cls == local.Float) return new FloatVal(0) { cls = local.Float };
            var value = new Value(cls);
            value.cls = cls;
            if (cls.type == ObjType.ClassObj)
            {
                value.vmap = ((cls as ClassObj).draw.children[1] as Block).vmapA;
            }
            else if (cls.type == ObjType.ModelObj)
            {
                return new ModelVal(cls as ModelObj);
            }
            else if (cls.type == ObjType.GeneObj)
            {
                return new GeneVal(cls as GeneObj);
            }
            else if (cls.type == ObjType.ArrayType)
            {
                value.vmap = new Dictionary<string, Obj>();
            }
            else if (cls.type == ObjType.FuncType)
            {
                value.vmap = new Dictionary<string, Obj>();
            }
            else if (cls.type == ObjType.GenericObj)
            {
                value.vmap = ((cls as GenericObj).gene.draw.children[1] as Block).vmapA;
            }
            else return new Val(ObjType.Error);
            return value;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.kouhos = new SortedList<string, Obj>();
                        foreach (var kv in vmap)
                        {
                            local.kouhos.Add(kv.Key, kv.Value);
                        }
                    }
                    n++;
                    var name = (val2 as Word).name;
                    if (vmap.ContainsKey(name))
                    {
                        var obj = vmap[name].Clone();
                        obj.letter = obj.letter;
                        return vmap[name];
                    }
                }
            }
            return Obj.Error(ObjType.NG, val2.letter, "値の後続が適切ではないです");
        }
    }
    partial class Class
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            Obj obj = new ClassObj();
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Block)
                {
                    var generic = local.getB(word.name, word.letter) as Generic;
                    var block = val2.exeB(local) as Block;
                    local.blocks.Add(block);
                    for (var i = 0; i < block.rets.Count; i++)
                    {
                        val2 = block.rets[i];
                    }
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    generic.block = block;
                    n++;
                    val2 = primary.children[n];
                    obj = generic;
                }
                local.declareB(word.name, obj);
                obj.letter = word.letter;
            }
        head:
            if (val2.type == ObjType.Left)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Bracket)
                {
                    local.calls.Add(local.KouhoSetClaGen);
                    var val = primary.children[n].exeB(local).GetterB(local);
                    local.calls.RemoveAt(local.calls.Count - 1);
                    if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                    var blk = val2 as Block;
                    n++;
                    val2 = primary.children[n];
                    for(var i = 0; i < blk.rets.Count; i++)
                    {
                        if (blk.rets[i].type == ObjType.ClassObj || blk.rets[i].type == ObjType.GenericObj)
                        {
                            var cls = val2 as Type;
                            if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(cls);
                            else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(cls);
                            else return Obj.Error(ObjType.Error, blk.rets[i].letter, "継承元のクラスはクラスとジェネリッククラスだけです");                           goto head;
                        }
                        else return Obj.Error(ObjType.Error, blk.rets[i].letter, "クラスの継承元はクラスでなければなりません");
                    }
                }
                else
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.KouhoSetClaGen();
                    }
                    val2 = val2.GetterB(local);
                    if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
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
                        if (val2.type == ObjType.Block)
                        {
                            local.calls.Add(local.KouhoSetType);
                            val2 = primary.children[n].exeB(local).GetterB(local);
                            local.calls.RemoveAt(local.calls.Count - 1);
                            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
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
                            if (generic.vmap.Count != genericobj.draws.Count) return Obj.Error(ObjType.Error, block.letter, "ジェネリックスクラスの引数の数が違います");
                            if (obj.type == ObjType.ClassObj) (obj as ClassObj).extends.Add(genericobj);
                            else if (obj.type == ObjType.Generic) (obj as Generic).extends.Add(genericobj);
                            else return Obj.Error(ObjType.Error, val2.letter, "継承はクラスかジェネリッククラスだけです。");
                            goto head;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "ジェネリッククラスは[]で引数をもたなければならないです");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "クラスの継承元はクラスでなければなりません");
                }
            }
            if (val2.type == ObjType.CallBlock)
            {
                n++;
                return obj;
            }
            return Obj.Error(ObjType.NG, val2.letter, "クラスの宣言の{|}が抜けています");
        }

    }
    partial class Type
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
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
                    local.calls.Add(local.KouhoSetType);
                    val2 = val2.exeB(local).GetterB(local);
                    local.calls.RemoveAt(local.calls.Count - 1);
                    if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                    var blk = val2 as Block;
                    for (var i = 0; i < blk.rets.Count; i++)
                    {
                        val2 = blk.rets[i];
                        if (val2 is Type)
                        {
                            functype.draws.Add(val2 as Type);
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "継承元はクラスでなければならない"); ;
                    }
                    n++;
                    val2 = primary.children[n];
                }
                else
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.KouhoSetType();
                    }
                    val2 = val2.GetterB(local);
                    if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
                    if (val2 is Type)
                    {
                        //need later implemented
                        functype.draws.Add(val2 as Type);
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
                            local.calls.Add(local.KouhoSetType);
                            var val = val2.exeB(local).GetterB(local);
                            local.calls.RemoveAt(local.calls.Count - 1);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var blk = val as Block;
                            var genericobj = new GenericObj(generic);
                            for (var i = 0; i < blk.rets.Count; i++)
                            {
                                val2 = blk.rets[i];
                                if (val2 is Type)
                                {
                                    genericobj.draws.Add(val2 as Type);
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "ジェネリックスクラスの引数は型でなければいけません"); ;
                            }
                            if (generic.vmap.Count != genericobj.draws.Count) return Obj.Error(ObjType.Error, blk.letter, "ジェネリックスクラスの引数の型の数が合っていません");
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
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.kouhos = new SortedList<string, Obj>();
                        local.kouhos.Add("new", new Obj(ObjType.None));
                    }
                    var word2 = val2 as Word;
                    n++;
                    if (type.type == ObjType.ClassObj || type.type == ObjType.ArrayType)
                    {
                        if (word2.name == "new") return new Constructor(type as ClassObj) { letter = val2.letter };
                        else return Obj.Error(ObjType.Error, val2.letter, word2.name + "は" + type.letter.name + "の後続にこれません");
                    }
                    else if (type.type == ObjType.ModelObj)
                    {
                        if (word2.name == "new") return new Constructor(type as ClassObj) { letter = val2.letter };
                        else return Obj.Error(ObjType.Error, val2.letter, word2.name + "は" + type.letter.name + "の後続にこれません" );
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
                var blk = val2.Clone().exeB(local) as Block;
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
                var blk = val2.Clone().exeB(local) as Block;
                for (var i = 0; i < blk.rets.Count; i++)
                {
                    if (blk.rets[i].type == ObjType.Word)
                    {
                        var word2 = blk.rets[i] as Word;
                        func.vmap[word2.name] = new VariClass(i);
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[i].letter, "ジェネリック関数の宣言は名前しか引数をとれません");
                }
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    func.draw = val2 as CallBlock;
                    n++;
                    if (word != null) local.declareB(word.name, func);
                    return func;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "ジェネリック関数の{|}が指定されていません");
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var func = new Function(type);
                func.draw = val2 as CallBlock;
                foreach (var b in local.blocks) func.blocks.Add(b);
                n++;
                if (word != null)
                {
                    local.declareB(word.name, func);
                    func.letter = word.letter;
                }
                return func;
            }
            else
            {
                if (word == null) return type;
                else
                {
                    var variable = new Variable(type);
                    local.declareB(word.name, variable);
                    variable.letter = word.letter;
                head1:
                    if (val2.type == ObjType.Left)
                    {
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            if (val2.letter == local.letter && local.kouhos == null)
                            {
                                local.KouhoSetFunction();
                            }
                            //create new instantfunc later
                            var func2 = val2.GetterB(local) as Function;
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
                        else
                        {
                            if (val2.type == ObjType.CallBlock)
                            {
                                n++;
                                val2 = primary.children[n];
                                goto head1;
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの{|}を指定してください");
                        }
                    }
                    else if (val2.type == ObjType.Right)
                    {
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            if (val2.letter == local.letter && local.kouhos == null)
                            {
                                local.KouhoSetFunction();
                            }
                            var func2 = val2.GetterB(local) as Function;
                            n++;
                            val2 = primary.children[n];
                            //create new instantfunc later
                            if (val2.type == ObjType.Block)
                            {
                                func2.block = val2 as Block;
                                n++;
                                val2 = primary.children[n];
                            }
                            if (val2.type == ObjType.Bracket)
                            {
                                func2.bracket = val2 as Block;
                                n++;
                                val2 = primary.children[n];
                            }
                            goto head1;
                        }
                        else
                        {
                            if (val2.type == ObjType.CallBlock)
                            {
                                n++;
                                val2 = primary.children[n];
                                goto head1;
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの{|}を指定してください");
                        }
                    }
                    return variable;
                }
            }
        }
    }
    partial class Var
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
    }
    partial class Generic
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                local.calls.Add(local.KouhoSetType);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
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
            return Obj.Error(ObjType.NG, val2.letter, "ジェネリッククラスは[]を引数にとります" );
        }
    }
    partial class Number
    {
        public override Obj exeB(Local local)
        {
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.kouhos = new SortedList<string, Obj>();
                        local.kouhos.Add("random", new Obj(ObjType.None));
                    }
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
        public Obj PlusB(String op, Local local, Obj val2)
        {
            if (val2 == null)
            {
                return this;
            }
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
            return Obj.Error(ObjType.Error, val2.letter, "整数の+は整数か小数か文字列です");
        }
        public Obj MinusB(String op, Local local, Obj val2)
        {
            if (val2 == null)
            {
                return new Number(-value) { cls = local.Int };
            }
            if (val2.type == ObjType.Number)
            {
                return new Number(value - (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value - (val2 as FloatVal).value) { cls = local.Float };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の-は整数か小数です");
        }
        public Obj MulB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new Number(value * (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value * (val2 as FloatVal).value) { cls = local.Float };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の*は整数か小数です");
        }
        public Obj DivB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new Number(value / (val2 as Number).value) { cls = local.Int };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal((float)value / (val2 as FloatVal).value) { cls = local.Float };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の/は整数か小数です");
        }
        public Obj MoreThanB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value > (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value > (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の>は整数か小数です");
        }
        public Obj LessThanB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value < (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value < (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の<は整数か小数です");
        }
        public Obj MoreEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value >= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value >= (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の>=は整数か小数です");
        }
        public Obj LessEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value <= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value <= (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の<=は整数か小数です");
        }
        public Obj NotEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value != (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value != (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の!=は整数か小数です");
        }
        public Obj EqualEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value == (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value == (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "整数の==は整数か小数です");
        }

    }
    partial class FloatVal
    {
        public override Obj exeB(Local local)
        {
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "小数には後続が来れません");
        }
        public Obj PlusB(String op, Local local, Obj val2)
        {
            if (val2 == null) return this;
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
            return Obj.Error(ObjType.Error, val2.letter, "小数の+は整数か小数です");
        }
        public Obj MinusB(String op, Local local, Obj val2)
        {
            if (val2 == null)
            {
                return new FloatVal(-value) { cls = local.Float };
            }
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value - (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value - (val2 as FloatVal).value) { cls = local.Float };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の-は整数か小数です");
        }
        public Obj MulB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value * (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value * (val2 as FloatVal).value) { cls = local.Float };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の*は整数か小数です");
        }
        public Obj DivB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new FloatVal(value / (float)(val2 as Number).value) { cls = local.Float };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new FloatVal(value / (val2 as FloatVal).value) { cls = local.Float };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の/は整数か小数です");
        }
        public Obj MoreThanB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value > (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value > (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の>は整数か小数です");
        }
        public Obj LessThanB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value < (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value < (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の<は整数か小数です");
        }
        public Obj MoreEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value >= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value >= (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の>=は整数か小数です");
        }
        public Obj LessEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value <= (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value <= (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の<=は整数か小数です");
        }
        public Obj NotEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value != (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value != (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の!=は整数か小数です");
        }
        public Obj EqualEqualB(String op, Local local, Obj val2)
        {
            if (val2.type == ObjType.Number)
            {
                return new BoolVal(value == (val2 as Number).value) { cls = local.Bool };
            }
            else if (val2.type == ObjType.FloatVal)
            {
                return new BoolVal(value == (val2 as FloatVal).value) { cls = local.Bool };
            }
            return Obj.Error(ObjType.Error, val2.letter, "小数の==は整数か小数です");
        }
    }
    partial class StrObj
    {
        public override Obj exeB(Local local)
        {
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "文字列には後続が来れません");
        }
    }
    partial class BoolVal
    {
        public override Obj exeB(Local local)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "bool値には後続がこれません");
        }
    }
    partial class VoiVal
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return new Obj(ObjType.Error);
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "void値には後続がこれません");
        }
    }
    partial class Null
    {
        public override Obj exeB(Local local)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "nullには後続が来ません");
        }

    }
    partial class Constructor
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            val2 = val2.Clone().exeB(local).GetterB(local);
            if (val2.type == ObjType.Bracket)
            {
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                var block1 = cls.draw.children[0] as Block;
                var varr = new List<Obj>(block1.vmapA.Values);
                if (varr.Count != blk.rets.Count) return Obj.Error(ObjType.Error, val2.letter, "コンストラクタの引数がありません");
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opesB["="]("=", local, blk.rets[i]);
                }
                n++;
                var value = Value.New(cls, local);
                value.letter = letter;
                return value;
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {

                var block1 = cls.draw.children[0].Clone() as Block;
                var varr = new List<Obj>(block1.vmapA.Values);
                if (varr.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "コンストラクタの引数が1個ではありません");
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opesB["="]("=", local, val2);
                }
                n++;
                var value = Value.New(cls, local);
                value.letter = letter;
                return value;
            }
            return Obj.Error(ObjType.NG, val2.letter, "コンストラクタの引数が適切ではありません");
        }
    }
    partial class Base
    {

        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
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
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                var block1 = cls.draw.children[0] as Block;
                var varr = new List<Obj>(block1.vmapA.Values);
                if (varr.Count != blk.rets.Count) return Obj.Error(ObjType.Error, val2.letter, "baseの引数があっていません");
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opesB["="]("=", local, blk.rets[i]);
                }
                return new VoiVal();
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {
                var block1 = cls.draw.children[0] as Block;
                var varr = new List<Obj>(block1.vmapA.Values);
                if (varr.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "base元のクラスの引数は1個ではありません");
                for (var i = 0; i < varr.Count; i++)
                {
                    varr[i].opesB["="]("=", local, val2);
                }
                n++;
                return new VoiVal();
            }
            return Obj.Error(ObjType.Error, val2.letter, "baseの引数が適切に設定されていません");
        }
    }
    partial class Variable
    {
        public override Obj GetterB(Local local)
        {
            return Value.New(cls, local).GetterB(local);
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Value.New(cls, local).PrimaryB(ref n, local, primary, val2);
        }
        public Obj OpeB(String op, Local local, Obj val2)
        {
            if (cls.type == ObjType.Var)
            {
                var varobj = cls as Var;
                if (varobj.cls == null) return new Obj(ObjType.Wait);
            }
            return Value.New(cls, local).opesB[op](op, local, val2);
        }
        public Obj InB(String op, Local local, Obj val2)
        {
            if (cls.type == ObjType.Var)
            {
                var varobj = cls as Var;
                if (varobj.cls == null) return new Obj(ObjType.Wait);
            }
        head:
            if (val2.type == ObjType.Bracket)
            {
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Block)
            {
                var it = new Iterator(-1);
                var val = val2;
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                it.value = val as Block;
                value = it;
                return it;
            }
            else return Obj.Error(ObjType.Error, val2.letter, "Iteratorの代入が不正です");
        }
        public Obj EqualB(String op, Local local, Obj val2)
        {
            if (value != null && value.type == ObjType.Iterator) return Obj.Error(ObjType.Error, letter, "イテレーターは値を変更できません");
            var ret = TypeCheck.CheckB(this, val2, CheckType.Setter, local);
            if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
            value = val2;
            if (comment != null)
            {
                if (value.type == ObjType.Number) comment.text = comment.name = (value as Number).value.ToString();
                else if (value.type == ObjType.FloatVal) comment.text = comment.name = (value as FloatVal).value.ToString();
                else if (value.type == ObjType.StrObj) comment.text = comment.name = (value as StrObj).value;
            }
            return this;
        }

    }
    partial class Function
    {
        public bool first = true;
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (bracket != null) throw new Exception();
            if (val2.type == ObjType.Bracket)
            {
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).Getter(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                var blk0 = draw.children[0] as Block;
                var vs1 = blk0.vmapA.Values.ToList();
                if (vs1.Count != blk.rets.Count) return Obj.Error(ObjType.Error, val2.letter, "関数の引数の数が合っていません");
                for (var i = 0; i < blk.vmapA.Values.Count; i++)
                {
                    var ret = TypeCheck.CheckB(vs1[i], blk.rets[i], CheckType.Setter, local);
                    if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
                }
                return Value.New(ret, local);
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value)
            {
                var blk0 = draw.children[0] as Block;
                var vs1 = blk0.vmapA.Values.ToList();
                if (vs1.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "関数の引数の数が合っていません");
                var ret = TypeCheck.CheckB(vs1[0], val2, CheckType.Setter, local);
                if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
                return Value.New(ret, local);
            }
            return Obj.Error(ObjType.NG, val2.letter, "()のような関数に適切な引数が来ていません");
        }
    }
    partial class If
    {
        public Type ret = new Var();
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                n++;
                val2 = primary.children[n];
                var ifb = calls[0];
                if (ret.cls == null) return new Obj(ObjType.Wait);
                var m = 1;
            head:
                var elif = val2;
                if (letter == local.letter && local.kouhos == null)
                {
                    local.calls.Last()();
                }
                if (val2.type == ObjType.Elif)
                {
                    n++;
                    val2 = primary.children[n];
                    m++;
                    if (val2.type == ObjType.CallBlock)
                    {
                        ifb = calls[m];
                        if (ret.cls == null) return new Obj(ObjType.Wait);
                        n++;
                        val2 = primary.children[n];
                        goto head;
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "elif文の中身が宣言されていません。");

                }
                else if (val2.type == ObjType.Else)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
                        ifb = calls[m];
                        if (ret.cls == null) return new Obj(ObjType.Wait);
                        n++;
                        return Value.New(ret, local);
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "else文の中身が宣言されていません。");
                }
                else return Value.New(ret, local);
            }
            return Obj.Error(ObjType.Error, val2.letter, "if文の中身が宣言されていません");
        }
    }
    partial class While
    {
        public Type ret = new Var();
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                if (ret.cls == null) return new Obj(ObjType.Wait);
                n++;
                return Value.New(ret, local);
            }
            return Obj.Error(ObjType.Error, val2.letter, "while文の中身が宣言されていません");
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
    }
    partial class For
    {
        public Type ret = new Var();
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                if (ret.cls == null) return new Obj(ObjType.Wait);
                n++;
                return Value.New(ret, local);
            }
            return Obj.Error(ObjType.Error, val2.letter, "for文の中身が宣言されていません。");
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
    }
    partial class Switch
    {
        public Type ret = new Var();
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.CallBlock)
            {
                if (ret.cls == null) return new Obj(ObjType.Wait);
                n++;
                return Value.New(ret, local);
            }
            return Obj.Error(ObjType.Error, val2.letter, "switch文の中身が宣言されていません");
        }
    }
    partial class Break
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            n++;
            var val2 = primary.children[n];
        head:
            if (val2.type == ObjType.Bracket)
            {
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block)
            {
                n++;
                value = val2;
                for (var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj != null)
                    {
                        if (block.obj.obj.type == ObjType.While || block.obj.obj.type == ObjType.For || block.obj.obj.type == ObjType.Switch)
                        {
                            block.returns.Add(val2);
                            n--;
                            return this;
                        }
                        else if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.ClassObj || block.obj.obj.type == ObjType.ModelObj || block.obj.obj.type == ObjType.GeneObj || block.obj.obj.type == ObjType.GenericFunction)
                        {
                            return Obj.Error(ObjType.Error, letter, "while,for,switchの中ではありません");
                        }
                    }
                }
                return Obj.Error(ObjType.Error, val2.letter, "while,for,switchの中ではありません");
            }
            n--;
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
        head:
            if (val2.type == ObjType.Bracket)
            {
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block)
            {
                n++;
                value = val2;
                for (var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj != null)
                    {
                        if (block.obj.obj.type == ObjType.While || block.obj.obj.type == ObjType.For || block.obj.obj.type == ObjType.Switch)
                        {
                            block.returns.Add(val2);
                            return this;
                        }
                        else if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.ClassObj || block.obj.obj.type == ObjType.ModelObj || block.obj.obj.type == ObjType.GeneObj || block.obj.obj.type == ObjType.GenericFunction)
                        {
                            return Obj.Error(ObjType.Error, letter, "while,for,switchの中ではありません");
                        }
                    }
                }
                return Obj.Error(ObjType.Error, val2.letter, "while,for,switchの中ではありません");
            }
            return Obj.Error(ObjType.NG, val2.letter, "breakの戻り値を適切に指定してください");
        }
    }
    partial class Continue
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
    }
    partial class  Return
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            n++;
            var val2 = primary.children[n];
        head:
            if (val2.type == ObjType.Bracket)
            {
                n++;
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
            if (val2.type == ObjType.Wait || val2.type == ObjType.Error || val2.type == ObjType.NG) return val2;
            if (val2.type == ObjType.Number || val2.type == ObjType.StrObj || val2.type == ObjType.BoolVal || val2.type == ObjType.Value || val2.type == ObjType.Block || val2.type == ObjType.GeneValue || val2.type == ObjType.Comment)
            {
                n++;
                value = val2;
                for(var i = local.blocks.Count - 1; i >= 0; i--)
                {
                    var block = local.blocks[i];
                    if (block.obj != null)
                    {
                        if (block.obj.obj.type == ObjType.Function || block.obj.obj.type == ObjType.GenericFunction || block.obj.obj.type == ObjType.ServerFunction)
                        {
                            var ret = TypeCheck.CheckB(block.obj.obj, value, CheckType.Return, local);
                            if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
                            n--;
                            return value;
                        }
                    }
                    return Obj.Error(ObjType.Error, val2.letter, "関数の中ではありません");
                }
                return Obj.Error(ObjType.Error, val2.letter, "return文は関数の中でしか使えません");
            }
            n--;
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
        head:
            if (val2.type == ObjType.Bracket)
            {
                n++;
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
                if (block.rets.Count == 1)
                {
                    val2 = block.rets[0];
                    goto head;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "()の中の数が間違っています。");
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
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
                            var ret = TypeCheck.CheckB(block.obj.obj, value, CheckType.Return, local);
                            if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
                            return value;
                        }
                    }
                    return Obj.Error(ObjType.Error, val2.letter, "関数の中ではありません");
                }
                return Obj.Error(ObjType.Error, val2.letter, "return文は関数の中でしか使えません");
            }
            return Obj.Error(ObjType.NG, val2.letter, "returnの戻り値を適切に指定してください");
        }
    }
    partial class Goto
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSetLabel();
            }
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
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket || val2.type == ObjType.Block)
            {
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var block = val as Block;
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
                    else return Obj.Error(ObjType.Error, val3.letter, "printできない変数です");               }
                if (val2.type == ObjType.Bracket) local.console.Text += ")\n";
                else local.console.Text += "]\n";
                n++;
                return new VoiVal();
            }
            if (val2.letter == local.letter && local.kouhos == null)
            {
                local.KouhoSet2();
            }
            val2 = val2.GetterB(local);
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
        public override Obj GetterB(Local local)
        {
            if (m == -1) return value.rets[n];
            else return (value.rets[n] as Block).rets[m];
        }
    }
    partial class GenericFunction
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Block)
            {
                local.calls.Add(local.KouhoSetType);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
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
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.KouhoSetModelGene();
                        local.kouhos.Add("Store", new Obj(ObjType.None));
                    }
                    var word = val2 as Word;
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.Dot)
                    {
                        var val = local.getB(word.name, word.letter);
                        if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                        if (val is not ModelObj) return Obj.Error(ObjType.Error, word.letter, "ModelクラスかGeneクラスを指定してください"); ;
                        var modelobj = val as ModelObj;
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Word)
                        {
                            if (val2.letter == local.letter && local.kouhos == null)
                            {
                                local.kouhos = new SortedList<string, Obj>();
                                local.kouhos.Add("First", new Obj(ObjType.None));
                                local.kouhos.Add("Select", new Obj(ObjType.None));
                                local.kouhos.Add("Sort", new Obj(ObjType.None));
                            }
                            var word2 = val2 as Word;
                            n++;
                            val2 = primary.children[n];
                            if (word2.name == "First")
                            {
                                if (val2.type == ObjType.Bracket)
                                {
                                    val = val2.exe(local).GetterB(local);
                                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                    var blk = val as Block;
                                    if (blk.rets.Count != 0) return Obj.Error(ObjType.Error, val2.letter, "Firstの引数は0です");
                                    return Value.New(modelobj, local);
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "Firstの()が指定されていません");
                            }
                            else if (word2.name == "Select")
                            {
                                if (val2.type == ObjType.Bracket)
                                {
                                    val = val2.exe(local).GetterB(local);
                                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                    var blk = val as Block;
                                    if (blk.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "Selectの引数は1です");
                                    else if (blk.rets[0].type != ObjType.Function) return Obj.Error(ObjType.Error, val2.letter, "Selectの引数は関数です");
                                    return local.db.SelectB(modelobj, local, blk.rets[0] as Function);
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "Selectの()が指定されていません");
                            }
                            else if (word2.name == "Sort")
                            {
                                if (val2.type == ObjType.Bracket)
                                {
                                    val = val2.exe(local).GetterB(local);
                                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                    var blk = val as Block;
                                    if (blk.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "Sortの引数は1です");
                                    else if (blk.rets[0].type != ObjType.Function) return Obj.Error(ObjType.Error, val2.letter, "Sortの引数は関数です");
                                    return new Block(ObjType.Array) { rets = new List<Obj> { Value.New(modelobj, local) } }.Sort(blk.rets[0] as Function, local);
                                }
                                else return Obj.Error(ObjType.Error, val2.letter, "Sortの()が指定されていません");
                            }
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前が来ます");
                    }
                    else if (word.name == "Store")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.exe(local).GetterB(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var blk = val as Block;
                            for (var i = 0; i < blk.rets.Count; i++)
                            {
                                if (blk.rets[i].type == ObjType.Wait || blk.rets[i].type == ObjType.Error || blk.rets[i].type == ObjType.NG) return blk.rets[0];
                                else if (blk.rets[i] is not Val) return Obj.Error(ObjType.Error, blk.rets[i].letter, "Storeの引数には値を指定してください");
                                local.db.StoreB(blk.rets[i] as Val, local);
                            }
                            return new VoiVal();
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "Storeの後に()がありません");
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "modelには" + word.name + "は宣言されていません" );
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の次には名前が来ます");
            }
            else if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var gj =  local.getB(word.name, word.letter) as ModelObj;
                if (val2.type == ObjType.CallBlock)
                {
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
        public override Obj exeB(Local local)
        {
            return this;
        }
    }
    partial class ModelVal
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        foreach(var kv in vmap)
                        {
                            local.kouhos.Add(kv.Key, kv.Value);
                        }
                        local.kouhos.Add("update", new Obj(ObjType.None));
                    }
                    var word = val2 as Word;
                    n++;
                    if (word.name == "update")
                    {
                        val2 = primary.children[n];
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.exeB(local).GetterB(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var block = val as Block;
                            n++;
                            if (block.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "updateの引数は1です");
                            var last = block.rets[0];
                            if (last.type == ObjType.Stock)
                            {
                                var stock = last as Stock;
                                return this;
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "updateの引数はDataStockを取ります");
                        }
                        else if (val2.type == ObjType.Stock)
                        {
                            return this;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "updateの引数はDataStockを取ります");
                    }
                    else if (vmap.ContainsKey(word.name)) return vmap[word.name];
                    else return Obj.Error(ObjType.Error, val2.letter, "この変数には" + word.name + "は宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の次は名前です");
            }
            return Obj.Error(ObjType.NG, val2.letter, "次は.ではありませんか");
        }
    }
    partial class StockType
    {

    }
    partial class Stock
    {
        public Obj SelectB(Object model, Local local, Function func)
        {
            var blk = func.draw.children[0] as Block;
            if (blk.vmapA.Count != 1) return Obj.Error(ObjType.Error, blk.letter, "Selectの引数は1つです");
            var ftype = new FuncType(local.Bool);
            ftype.draws.Add(model as Type);
            var ret = TypeCheck.CheckCVB(ftype, func, CheckType.Setter, local);
            if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
            return new Block(ObjType.Array) { rets = new List<Obj> { model as Obj } };
        }
        public Obj StoreB(Val val, Local local)
        {
            return new VoiVal();
        }
    }
    partial class Gene
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (letter == local.letter && local.kouhos == null)
            {
                local.calls.Last()();
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                var gj = local.getB(word.name, word.letter) as GeneObj;
                gj.letter = letter;
                if (val2.type == ObjType.CallBlock)
                {
                    gj.letter2 = (val2.children[1] as Block).letter2;
                    gj.call = val2 as CallBlock;
                    (gj.call.children[1] as Block).obj = new ObjBlock() { obj = gj, n = 1 };
                    foreach (var blk in local.blocks) gj.blocks.Add(blk);
                    n++;
                    val2 = primary.children[n];
                head:
                    if (val2.type == ObjType.Left)
                    {
                        n++;
                        val2 = primary.children[n];
                        if (val2.type == ObjType.CallBlock)
                        {
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
                            gj.right = val2 as CallBlock;
                            n++;
                            goto head;
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "進化プログラミングの機能指定には{|}が必要です");
                    }
                    return gj;
                }
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var gj = local.gene.vmap[""] as GeneObj;
                gj.call = val2 as CallBlock;
                (gj.call.children[1] as Block).obj = new ObjBlock() { obj = gj, n = 1 };
                foreach (var blk in local.blocks) gj.blocks.Add(blk);
                n++;
                val2 = primary.children[n];
            head:
                if (val2.type == ObjType.Left)
                {
                    n++;
                    val2 = primary.children[n];
                    if (val2.type == ObjType.CallBlock)
                    {
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
                        return gj.DotB(word.name);
                    }
                    else return Obj.Error(ObjType.Error, val2.letter, "無名Geneクラスは宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前が必要です");
            }
            return Obj.Error(ObjType.NG, val2.letter, "geneの後に適切な単語が来ていません");
        }
    }
    partial class GeneChild
    {

        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
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
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
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
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
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
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket) {
                if (val2.children.Count == 2)
                {
                    var val = val2.exeB(local).GetterB(local);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    var block = val as Block;
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
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    var word = val2 as Word;
                    n++;
                    var vmap = ((cls as GeneObj).call.children[1] as Block).vmapA;
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.kouhos = new SortedList<string, Obj>();
                        foreach(var kv in vmap)
                        {
                            local.kouhos.Add(kv.Key, kv.Value);
                        }
                    }
                    if (vmap.ContainsKey(word.name)) return vmap[word.name];
                    else return Obj.Error(ObjType.Error, val2.letter, word.name + "は宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
            }
            return Obj.Error(ObjType.NG, val2.letter, "変数の後には.ではありませんか");
        }
    }
    partial class GeneStore
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                if (val2.children.Count == 0)
                {
                    var value = Value.New(gj, local);
                    local.db.StoreB(value, local);
                    return value;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Storeの引数は0です。");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneSelect
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return new Block(ObjType.Block) { rets = new List<Obj> { Value.New(gj, local) } };
                }
                else if (val2.children.Count == 1)
                {
                    var rets = new List<Obj>{ Value.New(gj, local) };
                    local.calls.Add(local.KouhoSet2);
                    var val = val2.exeB(local).GetterB(local);
                    local.calls.RemoveAt(local.calls.Count - 1);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    var blk = val as Block;
                    if (blk.rets[0].type == ObjType.Function)
                    {
                        var func = blk.rets[0] as Function;
                        local.db.SelectB(gj, local, func);
                        n++;
                        return new Block(ObjType.Array) { rets = rets };
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[0].letter, "Selectは関数を引数にとります");
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Selectの引数があっていません");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneSort
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                if (val2.children.Count == 0)
                {
                    n++;
                    return new Block(ObjType.Block) { rets = new List<Obj> { Value.New(gj, local) } };
                }
                else if (val2.children.Count == 1)
                {
                    local.calls.Add(local.KouhoSet2);
                    var val = val2.exeB(local).GetterB(local);
                    local.calls.RemoveAt(local.calls.Count - 1);
                    if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                    var blk = val as Block;
                    n++;
                    if (blk.rets[0].type == ObjType.Function)
                    {
                        var func = blk.rets[0] as Function;
                        return new Block(ObjType.Array) { rets = new List<Obj> { Value.New(gj, local) } }.Sort(func, local);
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[0].letter, "Sortは関数を引数にとります");
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Sortの引数があっていません");
            }
            return Obj.Error(ObjType.NG, val2.letter, "関数の後には()が必要です。");
        }
    }
    partial class GeneLabel
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class GeneLabelValue
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    if (val2.letter == local.letter && local.kouhos == null)
                    {
                        local.kouhos.Add("length", new Obj(ObjType.None));
                    }
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
            return Obj.Error(ObjType.NG, val2.letter, "変数の後には.ではありませんか");
        }
    }
    partial class NewBlock
    {
        public override Obj exeB(Local local)
        {
            rets.Add(new Number(0));
            return this;
        }
    }
    partial class CrossBlock
    {
        public GeneObj gj;
        public override Obj exeB(Local local)
        {
            return Value.New(gj, local);
        }
    }
    partial class MigrateBlock
    {
        public override Obj exeB(Local local)
        {
            rets.Add(new FloatVal(0.1f) { cls = local.Float });
            return this;
        }
    }
    partial class GeneObj
    {
        public Obj DotB(String name)
        {
            if (vmap.ContainsKey(name)) return vmap[name];
            return gv.vmap[name];
        }
        public override Obj exeB(Local local)
        {
            return this;
        }
    }
}
