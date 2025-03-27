using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class CommentType : Type
    {
        public CommentType() : base(ObjType.CommentType)
        {

        }
    }
    partial class Comment
    {
        public override Obj exeA(Local local)
        {
            var block = children[0] as Block;
            block.vmapA["div"] = new ElemType(ObjType.Div);
            block.vmapA["br"] = new ElemType(ObjType.Br);
            block.vmapA["sheet"] = new ElemType(ObjType.Sheet);
            block.vmapA["cell"] = new ElemType(ObjType.Cell);
            block.vmapA["span"] = new ElemType(ObjType.Span);
            block.vmap  ["linear"] = new LinearFunction();
            local.blocks.Add(block);
            block.exeA(local);
            var comelet = (CommentLet)letter;
            comelet.comment = this;
            local.comments.Add(comelet);
            comelet.Renew();
            comelet.nums.Add(0);
            local.blocks.RemoveAt(local.blocks.Count - 1);
            local.comments.RemoveAt(local.comments.Count - 1);
            return this;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            var block = children[0] as Block;
            block.vmapA["div"] = new ElemType(ObjType.Div);
            block.vmapA["br"] = new ElemType(ObjType.Br);
            block.vmapA["sheet"] = new ElemType(ObjType.Sheet);
            block.vmapA["cell"] = new ElemType(ObjType.Cell);
            block.vmapA["span"] = new ElemType(ObjType.Span);
            block.vmap["linear"] = new LinearFunction();
            local.blocks.Add(block);
            block.exeA(local);
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
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                var blk = val2.exeA(local).GetterA(local) as Block;
                return new AnimationFunction();
            }
            return Obj.Error(ObjType.NG, val2.letter, "linearの()が書かれていません");
        }
    }
    partial class AnimationFunction
    {
    }
    partial class Comment2
    {
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    partial class Clones
    {
        public Obj RightRightA(String op, Local local, Obj val2)
        {
            return this;
        }
    }
    partial class Dolor
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
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
                    case "func":
                        var clones = new Clones();
                        clones.letter = letter;
                        return clones;
                }
            }
            else if (val2.type == ObjType.Bracket)
            {
                var val = val2.Clone().exeA(local).GetterA(local);
                if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                var blk = val as Block;
                if (blk.rets.Count != 1) return Obj.Error(ObjType.Error, val2.letter, "$()は引数を1つしかとれません");
                val2 = blk.rets[0];
                if (blk.rets[0].type == ObjType.Number || blk.rets[0].type == ObjType.StrObj || blk.rets[0].type == ObjType.FloatVal || blk.rets[0].type == ObjType.BoolVal)
                {
                    if (local.comments.Count > 0)
                    {
                        return new HtmObj(blk.rets[0].letter);
                    }
                    else return Obj.Error(ObjType.Error, blk.rets[0].letter, "$()は構造コメントブロックの中でしか使えません");
                }
                return Obj.Error(ObjType.Error, blk.rets[0].letter, "$()は整数、文字列を出力します");
            }
            return Obj.Error(ObjType.Error, val2.letter, "$の後ろには()かtype,funcが来ます");       }
    }
    partial class HtmObj
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
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
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            if (local.comments.Count > 0)
            {
                var blk1 = children[0].exeA(local).GetterA(local) as Block;
                blk1.obj = new ObjBlock() { obj = this, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                local.blocks.Add(blk1);
                for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                blk1.exeA(local);
                var blk2 = children[1].exeA(local).GetterA(local) as Block;
                blk2.obj = new ObjBlock() { obj =  this, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                local.blocks.Add(blk2);
                for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                blk2.exeA(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.comments.Last().Back();
                return null;
            }
            return Obj.Error(ObjType.Error, letter, "$()は構造コメントブロックの中でしか使えません");
        }
        public override Obj exeA(Local local)
        {
            if (local.comments.Count > 0)
            {
                var blk1 = children[0].exeA(local).GetterA(local) as Block;
                blk1.obj = new ObjBlock() { obj = this, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                local.blocks.Add(blk1);
                for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                blk1.exeA(local);
                var blk2 = children[1].exeA(local).GetterA(local) as Block;
                blk2.obj = new ObjBlock() { obj = this, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                local.blocks.Add(blk2);
                for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                blk2.exeA(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.comments.Last().Back();
            }
            return null;
        }
    }
    partial class ElemType
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            var val2 = primary.children[n + 1];
            if (val2.type == ObjType.Word)
            {
                n++;
                var word = val2 as Word;
                var elemobj = new ElemObj(this, word.name);
                local.declareA(word.name, elemobj);
                return elemobj;
            }
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Word)
            {
                var word = val2 as Word;
                n++;
                var elemobj = new ElemObj(this, word.name);
                local.declareA(word.name, elemobj);
                return elemobj;
            }
            return Obj.Error(ObjType.NG, val2.letter, "elem変数の名前が宣言されていません");
        }
    }
    partial class ElemObj
    {
        public override Obj opeA(string key, Local local, Obj val2)
        {
            if ((key == "+" || key == "!" || key == "*") && val2 == null)
            {
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
                if (val2.type == ObjType.CallBlock)
                {
                    var func = new SignalFunction() { draw = val2 as CallBlock, name = word.name };
                    local.sigmapA[word.name] = func;
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
                    for (var i = local.blocks.Count - 1; i > 0; i--)
                    {
                        if (local.blocks[i].obj.obj.type == ObjType.ServerFunction)
                        {
                            var sf = local.blocks[i].obj.obj as ServerFunction;
                            local.sigmapA[sf.server.name + ":" + word.name] = func;
                            local.declareA(word.name, func);
                            return func;
                        }
                        if (i == 0) return Obj.Error(ObjType.Error, letter, "signal関数はserver{|},client{|}の中でしか宣言出来ません");
                    }
                }
                return Obj.Error(ObjType.Error, val2.letter, "signal関数に{|}が宣言されていない"); ;
            }
            return Obj.Error(ObjType.Error, val2.letter, "signal関数を宣言してください");
        }
    }
    partial class SignalFunction
    {
        public Type cls;
        public override Obj exeA(Local local)
        {
            
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, val2.letter, "signal関数はユーザーから呼び出せません");
        }
    }
    partial class ServerClient
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
                    var sf = new ServerFunction(this) { draw = val2 as CallBlock, name = name + ":" + word.name };
                    var blk1 = val2.children[0] as Block;
                    blk1.obj = new ObjBlock() { obj = sf, n = 0 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                    local.blocks.Add(blk1);
                    for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                    blk1.exeA(local);
                    var blk2 = val2.children[1] as Block;
                    blk2.obj = new ObjBlock() { obj = sf, n = 1 };
                    local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                    local.blocks.Add(blk2);
                    for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                    blk2.exeA(local);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.blocks.RemoveAt(local.blocks.Count - 1);
                    local.sigmapA[sf.name] = sf;
                    return sf;
                }
                else return Obj.Error(ObjType.Error, val2.letter, "server,clientの{|}が宣言されていません");
            }
            else if (val2.type == ObjType.CallBlock)
            {
                var sf = new ServerFunction(this) { draw = val2 as CallBlock, name = name };
                var blk1 = val2.children[0] as Block;
                blk1.obj = new ObjBlock() { obj = sf, n = 0 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk1);
                local.blocks.Add(blk1);
                for (var i = 0; i < local.blocks.Count; i++) blk1.blocks.Add(local.blocks[i]);
                blk1.exeA(local);
                var blk2 = val2.children[1] as Block;
                blk2.obj = new ObjBlock() { obj = sf, n = 1 };
                local.blocks[local.blocks.Count - 1].childblocks.Add(blk2);
                local.blocks.Add(blk2);
                for (var i = 0; i < local.blocks.Count; i++) blk2.blocks.Add(local.blocks[i]);
                blk2.exeA(local);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.blocks.RemoveAt(local.blocks.Count - 1);
                local.sigmapA[sf.name] = sf;
                return sf;
            }
            return Obj.Error(ObjType.Error, val2.letter, "server,clientの後ろには名前か{|}がきます");
        }
    }
    partial class ServerFunction
    {
        public Type cls;
        public override Obj exeA(Local local)
        {
            return this;
        }
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            return Obj.Error(ObjType.NG, letter, "ServerClient関数は呼び出せません");
        }
    }
    partial class Connect
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
                    val2 = primary.children[n];
                    if (word.name == "new")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.Clone().exeA(local).GetterA(local);
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
                    if (word.name == "send")
                    {
                        if (val2.type == ObjType.Bracket)
                        {
                            var val = val2.Clone().exeA(local).GetterA(local);
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
                            var val = val2.Clone().exeA(local).GetterA(local);
                            if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
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
                                    else return Obj.Error(ObjType.NG, val2.letter, "Outputに()がありません");
                                }
                                else if (word2.name == "Select")
                                {
                                    if (val2.type == ObjType.Bracket)
                                    {
                                        var val = val2.exeA(local).GetterA(local);
                                        if (val.type == ObjType.Wait || val.type == ObjType.Error || val.type == ObjType.NG) return val;
                                        var blk = val as Block;
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
                                            return Value.New(local.getA(word.name) as Type, local);
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
                    address += "/" + word.name;
                }
                else return Obj.Error(ObjType.Error, val2.letter, ".の後には名前がきます");
            }
            else if (val2.type == ObjType.Bracket)
            {
                var block = val2.Clone().exeA(local).GetterA(local) as Block;
                draw = block;
                n++;
                return this;
            }
            return Obj.Error(ObjType.NG, val2.letter, "addressの後ろには.か()がきます");
        }
    }
    partial class Mountain
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
        {
            n++;
            var val2 = primary.children[n];
        head:
            if (val2.type == ObjType.Word)
            {
                return new Block(ObjType.ArrayVal);
            }
            return Obj.Error(ObjType.Error, val2.letter, "範囲の記述式がきていません");
        }
    }
    partial class Sum
    {
        public override Obj PrimaryA(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Bracket)
            {
                n++;
                return new Number(0) { cls = local.Int };
            }
            return Obj.Error(ObjType.NG, val2.letter, "Sumの()がありません");
        }
    }
    partial class Question
    {
        public override Obj exepA(ref int n, Local local, Primary primary)
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
