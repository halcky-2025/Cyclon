using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Comment
    {
        public override Obj exeB(Local local)
        {
            var block = children[0] as Block;
            local.blocks.Add(block);
            block.exeB(local);
            var comelet = (CommentLet)letter;
            comelet.comment = this;
            local.comments.Add(comelet);
            comelet.Renew();
            comelet.nums.Add(0);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.comments.RemoveAt(local.comments.Count - 1);
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            var block = children[0] as Block;
            local.blocks.Add(block);
            block.exeB(local);
            var comelet = (CommentLet)letter;
            comelet.comment = this;
            local.comments.Add(comelet);
            comelet.Renew();
            comelet.nums.Add(0);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.comments.RemoveAt(local.comments.Count - 1);
            return this;
        }
    }
    partial class LinearFunction
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                local.calls.Add(local.KouhoSet2);
                local.elem = true;
                var val = val2.exeB(local).GetterB(local);
                local.elem = false;
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                var anif = new AnimationFunction();
                var bools = new Boolean[blk.rets.Count];
                for (var i = 0; i < bools.Length; i++) bools[i] = false;
                foreach (var v in blk.labelmap)
                {
                    if (int.TryParse(v.Key, out int n2))
                    {
                        bools[v.Value.n] = true;
                        if (blk.rets[v.Value.n].type == ObjType.Block || blk.rets[v.Value.n].type == ObjType.ArrayVal)
                        {
                            var blk1 = blk.rets[v.Value.n] as Block;
                            TagBlock tag = null;
                            for (var i = local.blocks.Count - 1; i >= 0; i--)
                            {
                                var blk2 = local.blocks[i] as Block;
                                if (blk2.obj.obj.type == ObjType.TagBlock)
                                {
                                    tag = blk2.obj.obj as TagBlock;
                                    break;
                                }
                            }
                            anif.SetB(tag.divobj.elem, blk1, local);
                        }
                        else return Obj.Error(ObjType.Error, blk.rets[v.Value.n].letter, "アニメーションするStyleを指定してください");
                    }
                }
                for(var i = 0; i < bools.Length; i++)
                {
                    if (bools[i]) continue;
                    var val3 = blk.rets[i];
                    if (val3.type == ObjType.FloatVal)
                    {
                        anif.interval = (val3 as FloatVal).value;
                    }
                    else if (val3.type == ObjType.Number)
                    {
                        anif.loop = (val3 as Number).value;
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[0].letter, "インターバルは小数、繰り返しの回数は整数で引数を与えてください");
                }
                return anif;
            }
            return Obj.Error(ObjType.NG, val2.letter, "linearの()が書かれていません");
        }
    }
    partial class AnimationFunction
    {
        public void SetB(Element div, Block block, Local local)
        {
            foreach (var labelmap in block.labelmap)
            {
                if (div is Div)
                {
                    (div as Div).SetParamB(labelmap.Key, block.rets[labelmap.Value.n], local);
                }
                else if (div is Span)
                {
                    (div as Span).SetParamB(labelmap.Key, block.rets[labelmap.Value.n], local);
                }
            }
        }
    }
    partial class Comment2
    {
        public override Obj exeB(Local local)
        {
            return this;
        }
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class Clones
    {
        public Obj RightRightB(String op, Local local, Obj val2)
        {
            return this;
        }
    }
    partial class Dolor
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
            if (val2.type == ObjType.Word)
            {
                if (val2.letter == local.letter && local.kouhos == null)
                {
                    local.kouhos = new SortedList<string, Obj>();
                    local.kouhos.Add("type", new Obj(ObjType.None));
                    local.kouhos.Add("func", new Obj(ObjType.None));
                }
                var word = val2 as Word;
                n++;
                switch (word.name)
                {
                    case "type":
                        var clones = new Clones();
                        clones.letter = letter;
                        return clones;
                    case "func":
                        break;
                }
            }
            else if (val2.type == ObjType.Bracket)
            {
                local.calls.Add(local.KouhoSet);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                if (blk.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "$()は引数を1つしかとれません");
                val2 = blk.rets[0];
                if (blk.rets[0].type == ObjType.Number || blk.rets[0].type == ObjType.StrObj)
                {
                    var str = (val2 as StrObj).value;
                    if (local.comments.Count > 0) return null;
                    else return Obj.Error(ObjType.Error, blk.rets[0].letter, "$()は構造コメントブロックの中でしか使えません");
                }
                else return Obj.Error(ObjType.Error, blk.rets[0].letter, "$()は整数、文字列を出力します");
            }
            return Obj.Error(ObjType.Error, val2.letter, "$の後ろには()かtype,funcが来ます");
        }
    }
    partial class HtmObj
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            if (local.comments.Count > 0)
            {
                return null;
            }
            return Obj.Error(ObjType.Error, letter, "コメント内で宣言してください");
        }
    }
    partial class TagBlock
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj exeB(Local local)
        {
            return this;
        }
    }
    partial class ElemType
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            var val2 = primary.children[n + 1];
            if (val2.type == ObjType.Word)
            {
                n++;
                var word = val2 as Word;
                var elemobj = new ElemObj(this, word.name);
                local.declareB(word.name, elemobj);
                return elemobj;
            }
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                var elemobj = new ElemObj(this, word.name);
                local.declareB(word.name, elemobj);
                return elemobj;
            }
            return Obj.Error(ObjType.NG, val2.letter, "elem変数の名前が宣言されていません");
        }
    }
    partial class ElemObj
    {
        public void paramB(String name, Obj obj, Local local)
        {
            if (elem is Div)
            {
                (elem as Div).SetParamB(name, obj, local);
            }
            else if (elem is Span)
            {
                (elem as Span).SetParamB(name, obj, local);
            }
        }
        public override Obj opeB(string key, Local local, Obj val2)
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
            return Obj.Error(ObjType.Error, letter, "elem値は+,!,*の単項演算子しか作用しません");
        }
    }
    partial class CDec
    {
    }
    partial class CFunc
    {
    }
    partial class CType
    {
    }
    partial class Signal
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    SignalFunction func = null;
                    for (var i = local.blocks.Count - 1; i > 0; i--)
                    {
                        if (local.blocks[i].obj.obj.type == ObjType.ServerFunction)
                        {
                            var sf = local.blocks[i].obj.obj as ServerFunction;
                            func = local.sigmapA[sf.server.name + ":" + word.name] as SignalFunction;
                            break;
                        }
                        if (i == 0) return Obj.Error(ObjType.Error, letter, "signal関数はserver{|},client{|}の中でしか宣言出来ません");
                    }
                    return func;
                }
                return Obj.Error(ObjType.Error, val2.letter, "signal関数に{|}が宣言されていない");
            }
            return Obj.Error(ObjType.Error, val2.letter, "signal関数を宣言してください");
        }
    }
    partial class SignalFunction
    {
        public override Obj exeB(Local local)
        {
            return basicexeB(new Stock(), new Stock(), local);
        }
        public Obj basicexeB(Stock stock1, Stock stock2, Local local)
        {
            var block1 = draw.children[0].Clone() as Block;
            block1.vmap["req"] = stock1;
            block1.vmap["res"] = stock2;
            local.blockslist.Add(blocks);
            local.blocks.Add(block1);
            var block2 = draw.children[1].Clone().exeB(local).GetterB(local) as Block;
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
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "signal関数はユーザーから呼び出せません");
        }
    }
    partial class ServerClient
    {
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
                        local.KouhoSetSignal();
                    }
                    var word = val2 as Word;
                    n++;
                    return local.sigmapA[name + ":" + word.name];
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
            }
            else if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.CallBlock)
                {
                    var sf = local.sigmapA[name + ":" + word.name] as ServerFunction;
                    return sf;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "server,clientの{|}が宣言されていません");
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var sf = local.sigmapA[name] as ServerFunction;
                return sf;
            }
            return Obj.Error(ObjType.Error, val2.letter, "server,clientの後ろには名前か{|}がきます");
        }
    }
    partial class ServerFunction
    {
        public override Obj exeB(Local local)
        {
            return this;
        }
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, letter, "ServerClient関数は呼び出せません");
        }
    }
    partial class Connect
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
                        local.kouhos = new SortedList<string, Obj>();
                        local.kouhos.Add("new", new Obj(ObjType.None));
                    }
                    var word = val2 as Word;
                    n++;
                    val2 = primary.children[n];
                    if (word.name == "new")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.exeB(local).GetterB(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var blk1 = val as Block;
                            if (blk1.rets.Count == 0)
                            {
                                return new ConnectStock();
                            }
                            else if (blk1.rets.Count == 1)
                            {
                                if (blk1.rets[0].type == ObjType.Address) return new ConnectStock() { address = blk1.rets[0] as Address };
                                else return Obj.Error(ObjType.Error, blk1.rets[0].letter, "connect.newはアドレスしか引数にとりません");
                            }
                            else Obj.Error(ObjType.Error, blk1.letter, "connect.newの引数は0か1です");
                        }
                        else return Obj.Error(ObjType.Error, val2.letter, "connect.newの()が宣言されていません");
                    }
                    else return Obj.Error(ObjType.Error, word.letter, "connectに" + word.name + "は宣言されていません");
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
            }
            return Obj.Error(ObjType.Error, val2.letter, ".が来ていません");
        }
    }
    partial class ConnectStock
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
                        local.kouhos = new SortedList<string, Obj>();
                        local.kouhos.Add("send", new Obj(ObjType.None));
                        local.kouhos.Add("back", new Obj(ObjType.None));
                        local.kouhos.Add("Store", new Obj(ObjType.None));
                    }
                    var word = val2 as Word;
                    n++;
                    val2 = primary.children[n];
                    if (word.name == "send")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.exeB(local).GetterB(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var block = val as Block;
                            n++;
                            if (block.rets.Count == 1)
                            {
                            }
                            else if (block.rets.Count == 2)
                            {
                                if (block.rets[1].type != ObjType.Address) return Obj.Error(ObjType.Error, block.rets[1].letter, "sendの引数の2つ目はアドレスでなければなりません");
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, "sendの引数は1か2です");
                            if (block.rets[0].type != ObjType.StrObj && block.rets[0].type != ObjType.Comment) return Obj.Error(ObjType.Error, val2.letter, "sendの引数の0コメは文字かコメント文字列です");
                            return new VoiVal();
                        }
                        else return Obj.Error(ObjType.NG, val2.letter, "sendの()が入力されていません");
                    }
                    else if (word.name == "back")
                    {
                        var variable = new Value(new AddressType());
                        return variable;
                    }
                    else if (word.name == "Store")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.exeB(local).GetterB(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                            var block = val as Block;
                            for (var i = 0; i < block.rets.Count; i++)
                            {
                                if (block.rets[i].type == ObjType.ModelValue || block.rets[i].type == ObjType.GeneValue)
                                {

                                }
                                else return Obj.Error(ObjType.Error, block.rets[0].letter, "Storeできるのはmodel変数かgene変数だけです");
                            }
                            n++;
                            return new VoiVal();
                        }
                        else return Obj.Error(ObjType.NG, val2.letter, "Storeの()が宣言されていません");
                    }
                    else
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
                                    local.kouhos.Add("Output", new Obj(ObjType.None));
                                    local.kouhos.Add("Select", new Obj(ObjType.None));
                                    local.kouhos.Add("First", new Obj(ObjType.None));
                                }
                                var word2 = val2 as Word;
                                n++;
                                val2 = primary.children[n];
                                if (word2.name == "Output")
                                {
                                    if (val2.type == ObjType.Bracket)
                                    {
                                        if (val2.children.Count == 0)
                                        {
                                            return new ElemObj(local.getA("sheet") as ElemType, null);
                                        }
                                        else return Obj.Error(ObjType.Error, val2.letter, "Outputの引数は0です");
                                    }
                                    else return Obj.Error(ObjType.NG, val2.letter, "Outputの()がありません");
                                }
                                else if (word2.name == "Select")
                                {
                                    if (val2.type == ObjType.Bracket)
                                    {
                                        var val = val2.exeB(local).GetterB(local);
                                        if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                        var blk0 = val as Block;
                                        if (blk0.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "Selectの引数は1です");
                                        if (blk0.rets[0].type != ObjType.Function) return Obj.Error(ObjType.Error, blk0.rets[0].letter, "Selectの引数は()でなければなりまえん");
                                        var func = blk0.rets[0] as Function;
                                        var blk = func.draw.children[0] as Block;
                                        if (blk.vmapA.Count != 1) return Obj.Error(ObjType.Error, blk0.rets[0].letter, "Selectの関数の引数は1です");
                                        var ftype = new FuncType(local.Bool);
                                        ftype.draws.Add(local.getB(word.name, word.letter) as Type);
                                        var ret = TypeCheck.CheckCVB(ftype, func, CheckType.Setter, local);
                                        if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
                                        return blk;
                                    }
                                    else return Obj.Error(ObjType.NG, val2.letter, "Selectの()がありません");
                                }
                                else if (word2.name == "First")
                                {
                                    if (val2.type == ObjType.Bracket)
                                    {

                                        if (val2.children.Count == 0)
                                        {
                                            return Value.New(local.getB(word.name, word.letter) as Type, local);
                                        }
                                        else return Obj.Error(ObjType.Error, val2.letter, "Firstの引数は0です");
                                    }
                                    else return Obj.Error(ObjType.NG, val2.letter, "Firstの()がありません");
                                }
                                else return Obj.Error(ObjType.Error, word2.letter, word.name + "には" + word2.name + "が宣言されていません");
                            }
                            else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
                        }
                        else return Obj.Error(ObjType.NG, val2.letter, ".が予測されます");
                    }
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
            }
            return Obj.Error(ObjType.NG, val2.letter, ".が予測されます");
        }
    }
    partial class AddressType
    {
    }
    partial class Address
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
                        local.kouhos = new SortedList<string, Obj>();
                    }
                    var word = val2 as Word;
                    n++;
                    address += "/" + word.name;
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
            }
            else if (val2.type == ObjType.Bracket)
            {
                var block = val2.Clone().exeB(local).GetterB(local) as Block;
                draw = block;
                n++;
                return this;
            }
            return Obj.Error(ObjType.NG, val2.letter, "addressの後ろには.か()がきます");
        }
    }
    partial class Mountain
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
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
                        else return Obj.Error(ObjType.Error, val2.letter, "範囲の記述方式が間違っています");
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
                    return blk;
                }
            }
            return Obj.Error(ObjType.Error, val2.letter, "範囲の記述式がきていません");
        }
    }
    partial class Sum
    {
        public override Obj PrimaryB(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                local.calls.Add(local.KouhoSet2);
                var val = val2.exeB(local).GetterB(local);
                local.calls.RemoveAt(local.calls.Count - 1);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                var rets = new List<List<int>>();
                if (blk.rets.Count == 1)
                {
                    if (blk.rets[0].type == ObjType.Array)
                    {
                        val = blk.rets[0].exeB(local).GetterB(local);
                        if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                        var blk2 = val as Block;
                        for (var i = 0; i < blk2.rets.Count; i++)
                        {
                            rets.Add(new List<int>());
                            foreach (var nums in (blk2.rets[i].Clone().GetterB(local) as Block).rets)
                            {
                                if (nums.type == ObjType.Number) rets[i].Add((nums as Number).value);
                                else return Obj.Error(ObjType.Error, val2.letter, "範囲の選択はint[]でなければあなりません");
                            }
                        }
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[0].letter, "Sumの引数はint[]です。");
                }
                else return Obj.Error(ObjType.Error, val2.letter, "Sumの引数は1です");
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
                SetArrayB((nums) =>
                {
                    c.count++;
                    var text = sheet.cells[nums[0]][nums[1]].Text3(local).Trim();
                    if (text != "") n2 += Convert.ToInt32(text);
                    cells.Add(sheet.cells[nums[0]][nums[1]]);
                }, array, minmax, 0, local);
                return new Number(n2) { cls = local.Int };
            }
            return Obj.Error(ObjType.NG, val2.letter, "Sumの()がありません");
        }
        void SetArrayB(Action<int[]> task, int[] nums, List<List<int>> minimax, int n, Local local)
        {
            if (n == minimax[0].Count)
            {
                task(nums);
                return;
            }
            for (var i = minimax[0][n]; i <= minimax[1][n]; i++)
            {
                nums[n] = i;
                SetArrayB(task, nums, minimax, n + 1, local);
            }
        }
    }
    partial class Question
    {
        public override Obj exepB(ref int n, Local local, Primary primary)
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
