using Cyclon;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WMPLib;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace Cyclon
{
    class Sound: Div
    {
        public WindowsMediaPlayer player = new WindowsMediaPlayer();
        public override Element Measure(Measure m, Local local, ref int order)
        {
            player.controls.currentPosition = 0;
            player.PlayStateChange += Player_PlayStateChange;
            return null;
        }

        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded)
            {
                player.controls.currentPosition = 0;
                player.controls.play();
            }
            throw new NotImplementedException();
        }

        public override void Draw(Graphic g, Local local, ref bool select)
        {
            return;
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            return -1;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            return 0;
        }
        public override int plus(int n)
        {
            return n;
        }
        public override void SelectExe(SelectE e, Local local, ref bool select)
        {
            return;
        }
        public override int Count()
        {
            return 0;
        }
    }
    class Span : Letter {
        public String tag;
        public String id;
        public String sop;
        public Dictionary<String, Obj> statuses = new Dictionary<string, Obj>();
        public Span()
        {
            type = LetterType.Span;
            text = "";
        }
        public String Text3(Local local)
        {
            var ret = "<&span";
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|`" + text + "` >";
            return ret;
        }
        public override void add(Element e)
        {
            if (e.type == LetterType.Kaigyou || e.type == LetterType.End)
            {
                return;
            }
            else if (e is Letter)
            {
                var letter = e as Letter;
                text += letter.text;
                return;
            }
            throw new Exception();
        }
        public void SetStatus(Element elem, Local local)
        {
            if (elem is Div)
            {
                var divid = elem as Div;
                foreach (var kv in divid.statuses)
                {
                    if (!statuses.ContainsKey(kv.Key) || statuses[kv.Key] == null)
                    {
                        SetParam(kv.Key, kv.Value, local);
                        statuses[kv.Key] = kv.Value;
                    }
                }
            }
            else if (elem is Span)
            {
                var divid = elem as Span;
                foreach (var kv in divid.statuses)
                {
                    if (!statuses.ContainsKey(kv.Key) || statuses[kv.Key] == null)
                    {
                        SetParam(kv.Key, kv.Value, local);
                        statuses[kv.Key] = kv.Value;
                    }
                }
            }
        }
        public virtual Obj SetParamB(String name, Obj obj, Local local)
        {
            switch (name)
            {
                case "left":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "leftは整数、小数しか値をとれません" };
                case "top":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "topは整数、小数しか値をとれません" };
                case "z":
                    if (obj.type == ObjType.Number) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "zは整数しか値をとれません" };
                case "p":
                case "point":
                    if (obj.type == ObjType.Block || obj.type == ObjType.ArrayVal)
                    {
                        var blk = obj.exeB(local).GetterB(local) as Block;
                        if (blk.rets.Count != 2) return new Obj(ObjType.Error) { let = obj.letter, error = name + "の配列の数は2です" };
                        if (blk.rets[0].type != ObjType.Number && blk.rets[0].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error) { let = blk.rets[0].letter, error = name + "は整数か小数の配列しか値をとれません" };
                        }
                        if (blk.rets[1].type != ObjType.Number && blk.rets[1].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error) { let = blk.rets[1].letter, error = name + "は整数か小数の配列しか値をとれません" };
                        }
                        return blk;
                    }
                    return new Obj(ObjType.Error) { let = obj.letter, error = name + "は整数か小数の配列しか値をとれません" };
                case "w":
                case "width":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = name + "は整数か小数しか値をとれません" };
                case "h":
                case "height":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = name + "は整数か小数しか値をとれません" };
                case "size":
                    if (obj.type == ObjType.Block || obj.type == ObjType.ArrayVal)
                    {
                        var blk = obj.exeB(local).GetterB(local) as Block;
                        if (blk.rets.Count != 2) return new Obj(ObjType.Error) { let = obj.letter, error = name + "の配列の数は2です" };
                        if (blk.rets[0].type != ObjType.Number && blk.rets[0].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error) { let = blk.rets[1].letter, error = "sizeは整数か小数の配列しか値をとれません" }; ;
                        }
                        if (blk.rets[1].type != ObjType.Number && blk.rets[1].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error) { let = blk.rets[1].letter, error = "sizeは整数か小数の配列しか値をとれません" };
                        }
                        return blk;
                    }
                    return new Obj(ObjType.Error) { let = obj.letter, error = "sizeは整数か小数の配列しか値をとれません" };
                case "xtype":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "xtypeは\"Auto\"か\"Break\"か\"Limit\"か\"Scroll\"しか値をとれません" };
                case "ytype":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "ytypeは\"Auto\"か\"Break\"か\"Limit\"か\"Scroll\"しか値をとれません" };
                case "ptype":
                    if (obj.type == ObjType.Block || obj.type == ObjType.ArrayVal)
                    {
                        var blk = obj.exeB(local).GetterB(local) as Block;
                        if (blk.rets.Count != 2) return new Obj(ObjType.Error) { let = obj.letter, error = "ptypeの配列の数は2です" }; ;
                        if (blk.rets[0].type != ObjType.StrObj)
                        {
                            return new Obj(ObjType.Error) { let = blk.rets[1].letter, error = "ptypeは\"Auto\"か\"Break\"か\"Limit\"か\"Scroll\"しか値をとれません" };
                        }
                        if (blk.rets[1].type != ObjType.StrObj)
                        {
                            return new Obj(ObjType.Error) { let = blk.rets[1].letter, error = "ptypeは\"Auto\"か\"Break\"か\"Limit\"か\"Scroll\"しか値をとれません" };
                        }
                        return blk;
                    }
                    return new Obj(ObjType.Error) { let = obj.letter, error = "posは\"Auto\"か\"Break\"か\"Limit\"か\"Scroll\"の配列しか値をとれません" };
                case "pos":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "posは\"Relative\"か\"Absolute\"か\"Fixed\"しか値をとれません" };
                case "anim":
                    if (obj.type == ObjType.AnimationFunction) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "animはアニメーション関数しか値をとれません" };
                case "c":
                case "color":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = name + "は色しか値をとれません" };
                case "b":
                case "background":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = name + "は色しか値をとれません" };
                case "a":
                case "align":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error) { let = obj.letter, error = "posは\"Left\"か\"Center\"か\"Right\"か\"Separate\"しか値をとれません" };
                case "gokeydown":
                case "backkeydown":
                case "gomousedown":
                case "backmousedown":
                case "gomousemove":
                case "backmousemove":
                case "gomouseup":
                case "backmouseup":
                    if (obj.type == ObjType.SingnalFunction)
                    {
                        var functype = new FuncType(new CommentType());
                        functype.draws.Add(new StockType());
                        functype.draws.Add(new StockType());
                        var ret = TypeCheck.CheckCVB(functype, obj, CheckType.Finish, local);
                        if (ret.type == ObjType.Wait || ret.type == ObjType.Error || ret.type == ObjType.NG) return ret;
                        return obj;
                    }
                    return new Obj(ObjType.Error) { let = obj.letter, error = name + "はシグナル関数しか値をとれません" };
            }
            if (obj.type is Val) return obj;
            return new Obj(ObjType.Error);
        }
        public virtual void SetParam(String name, Obj obj, Local local)
        {
            switch (name)
            {
                case "b":
                case "background":
                    var strobj = (obj as StrObj).value;
                    switch (strobj)
                    {
                        case "red":
                            background = Brushes.Red;
                            break;
                        case "green":
                            background = Brushes.Green;
                            break;
                        case "blue":
                            background = Brushes.Blue;
                            break;
                        case "white":
                            background = Brushes.White;
                            break;
                        case "black":
                            background = Brushes.Black;
                            break;
                    }
                    break;
                case "c":
                case "color":
                    var strobj2 = (obj as StrObj).value;
                    switch (strobj2)
                    {
                        case "red":
                            brush = Brushes.Red;
                            break;
                        case "green":
                            brush = Brushes.Green;
                            break;
                        case "blue":
                            brush = Brushes.Blue;
                            break;
                        case "white":
                            brush = Brushes.White;
                            break;
                        case "black":
                            brush = Brushes.Black;
                            break;
                    }
                    break;
            }
        }
    }
    class Div : Element
    {
        public String tag;
        public String id;
        public String sop;
        public Dictionary<String, Obj> statuses = new Dictionary<string, Obj>();
        public Div()
        {
            type = LetterType.Div;
        }
        public void SetStatus(Element elem, Local local)
        {
            if (elem is Div)
            {
                var divid = elem as Div;
                foreach (var kv in divid.statuses)
                {
                    if (!statuses.ContainsKey(kv.Key) || statuses[kv.Key] == null)
                    {
                        SetParam(kv.Key, kv.Value, local);
                        statuses[kv.Key] = kv.Value;
                    }
                }
            }
            else if (elem is Span)
            {
                var divid = elem as Span;
                foreach (var kv in divid.statuses)
                {
                    if (!statuses.ContainsKey(kv.Key) || statuses[kv.Key] == null)
                    {
                        SetParam(kv.Key, kv.Value, local);
                        statuses[kv.Key] = kv.Value;
                    }
                }
            }
        }
        public virtual Obj SetParamB(String name, Obj obj, Local local)
        {
            switch (name)
            {
                case "left":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error);
                case "top":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error);
                case "z":
                    if (obj.type == ObjType.Number) return obj;
                    return new Obj(ObjType.Error);
                case "p":
                case "point":
                    if (obj.type == ObjType.Block || obj.type == ObjType.ArrayVal)
                    {
                        var blk = obj.exeB(local).GetterB(local) as Block;
                        if (blk.rets.Count != 2) return new Obj(ObjType.Error);
                        if (blk.rets[0].type != ObjType.Number && blk.rets[0].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error);
                        }
                        if (blk.rets[1].type != ObjType.Number && blk.rets[1].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error);
                        }
                        return blk;
                    }
                    return new Obj(ObjType.Error);
                case "w":
                case "width":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error);
                case "h":
                case "height":
                    if (obj.type == ObjType.Number || obj.type == ObjType.FloatVal) return obj;
                    return new Obj(ObjType.Error);
                case "size":
                    if (obj.type == ObjType.Block || obj.type == ObjType.ArrayVal)
                    {
                        var blk = obj.exeB(local).GetterB(local) as Block;
                        if (blk.rets.Count != 2) return new Obj(ObjType.Error);
                        if (blk.rets[0].type != ObjType.Number && blk.rets[0].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error);
                        }
                        if (blk.rets[1].type != ObjType.Number && blk.rets[1].type != ObjType.FloatVal)
                        {
                            return new Obj(ObjType.Error);
                        }
                        return blk;
                    }
                    return new Obj(ObjType.Error);
                case "xtype":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error);
                case "ytype":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error);
                case "ptype":
                    if (obj.type == ObjType.Block || obj.type == ObjType.ArrayVal)
                    {
                        var blk = obj.exeB(local).GetterB(local) as Block;
                        if (blk.rets.Count != 2) return new Obj(ObjType.Error);
                        if (blk.rets[0].type != ObjType.StrObj)
                        {
                            return new Obj(ObjType.Error);
                        }
                        if (blk.rets[1].type != ObjType.StrObj)
                        {
                            return new Obj(ObjType.Error);
                        }
                        return blk;
                    }
                    return new Obj(ObjType.Error);
                case "pos":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error);
                case "anim":
                    if (obj.type == ObjType.AnimationFunction) return obj;
                    return new Obj(ObjType.Error);
                case "c":
                case "color":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error);
                case "b":
                case "background":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error);
                case "a":
                case "align":
                    if (obj.type == ObjType.StrObj) return obj;
                    return new Obj(ObjType.Error);
                case "gokeydown":
                case "backkeydown":
                case "gomousedown":
                case "backmousedown":
                case "gomousemove":
                case "backmousemove":
                case "gomouseup":
                case "backmouseup":
                    if (obj.type == ObjType.SingnalFunction)
                    {
                        var blk = (obj as SignalFunction).draw.children[0] as Block;
                        if (blk.vmapA.Count != 2) return new Obj(ObjType.Error);
                        var vs = blk.vmapA.Values.ToList();
                        for (var i = 0; i < 2; i++)
                        {
                            if (vs[i].type == ObjType.Variable)
                            {
                                var vari = vs[i] as Variable;
                                if (vari.cls.type == ObjType.Var)
                                {
                                    if (vari.cls == null) vari.cls = new StockType();
                                }
                                if (vari.cls.type == ObjType.StockType)
                                {

                                }
                                else return new Obj(ObjType.Error);
                            }
                            else return new Obj(ObjType.Error);
                        }
                        return obj;
                    }
                    return new Obj(ObjType.Error);
            }
            if (obj.type is Val) return obj;
            return new Obj(ObjType.Error);
        }
        public virtual void SetParam(String name, Obj obj, Local local)
        {

            switch (name)
            {
                case "left":
                    pos2.X = (obj as Number).value;
                    break;
                case "top":
                    pos2.Y = (obj as Number).value;
                    break;
                case "z":
                    index = (obj as Number).value;
                    break;
                case "p":
                case "point":
                    var blk1 = obj as Block;
                    pos2 = new PointF((blk1.rets[0] as Number).value, (blk1.rets[1] as Number).value);
                    break;
                case "w":
                case "width":
                    size.X = (obj as Number).value;
                    break;
                case "h":
                case "height":
                    size.Y = (obj as Number).value;
                    break;
                case "size":
                    var blk2 = obj as Block;
                    pos2 = new PointF((blk2.rets[0] as Number).value, (blk2.rets[1] as Number).value);
                    break;
                case "xtype":
                    switch ((obj as StrObj).value)
                    {
                        case "auto":
                            xtype = SizeType.Auto;
                            break;
                        case "break":
                            xtype = SizeType.Break;
                            break;
                        case "limit":
                            xtype = SizeType.Limit;
                            break;
                        case "scroll":
                            xtype = SizeType.Scroll;
                            break;
                    }
                    break;
                case "ytype":
                    switch ((obj as StrObj).value)
                    {
                        case "auto":
                            ytype = SizeType.Auto;
                            break;
                        case "limit":
                            ytype = SizeType.Limit;
                            break;
                        case "scroll":
                            ytype = SizeType.Scroll;
                            break;
                    }
                    break;
                case "pos":
                    switch((obj as StrObj).value)
                    {
                        case "fixed":
                            position = Position.Fixed;
                            break;
                        case "absolute":
                            position = Position.Absolute;
                            break;
                        case "relative":
                            position = Position.Relative;
                            break;
                    }
                    break;
                case "b":
                case "background":
                    var strobj = (obj as StrObj).value;
                    switch (strobj)
                    {
                        case "red":
                            background = Brushes.Red;
                            break;
                        case "green":
                            background = Brushes.Green;
                            break;
                        case "blue":
                            background = Brushes.Blue;
                            break;
                        case "white":
                            background = Brushes.White;
                            break;
                        case "black":
                            background = Brushes.Black;
                            break;
                    }
                    break;
                case "align":
                case "a":
                    var str = (obj as StrObj).value;
                    switch (str)
                    {
                        case "left":
                            align = Align.Left;
                            break;
                        case "center":
                            align = Align.Center;
                            break;
                        case "right":
                            align = Align.Right;
                            break;
                        case "separate":
                            align = Align.Separate;
                            break;
                    }
                    break;
                case "key":
                    key = KeyExe;
                    break;
                case "onclick":
                    mouse = MouseExe;
                    break;
                case "anim":
                    var anif = obj as AnimationFunction;
                    anif.div = this;
                    local.vision.animations.Add(anif);
                    break;
            }
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            base.Draw(g, local, ref select);
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (update)
            {

                if (statuses.ContainsKey("key")) key = KeyExe;
                if (statuses.ContainsKey("onclick")) mouse = MouseExe;
            }
            return base.Measure(m, local, ref order);
        }

        public bool MouseExe(MouseEvent mouse, Local local)
        {
            var m = statuses["onclick"] as SignalFunction;
            var req = new ConnectStock();
            req.Store(new MouseEventObj(mouse, local) { cls = local.MouseEvent }, local);
            var res = new ConnectStock();
            m.basicexe(req, res, local);
            return false;
        }

        public bool KeyExe(KeyEvent key, Local local)
        {
            var k = statuses["key"] as SignalFunction;
            var req = new ConnectStock();
            req.Store(new KeyEventObj(key, local) { cls = local.KeyEvent }, local);
            var res = new ConnectStock();
            k.basicexe(req, res, local);
            return false;
        }
        public override string Text(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|" + base.Text2(local) + " >";
            return ret;
        }
        public override string Text2(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|" + base.Text2(local) + " >";
            return ret;
        }
    }
    class KeyEventObj : Val
    {
        public KeyEvent key;
        public KeyEventObj(KeyEvent key, Local local) : base(ObjType.KeyEventObj)
        {
            vmap = new Dictionary<string, Obj>();
            vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            vmap["action"] = new Number((int)key.call) { cls = local.Int };
            vmap["text"] = new StrObj(key.text) { cls = local.Str };
            this.key = key;
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
                    return vmap[word.name];
                }
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class MouseEventObj : Val
    {
        public MouseEvent mouse;
        public MouseEventObj(MouseEvent mouse, Local local) : base(ObjType.MouseEventObj)
        {
            vmap = new Dictionary<string, Obj>();
            vmap["id"] = new Variable(local.Int) { value = new Number(0) { cls = local.Int } };
            vmap["action"] = new Number((int)mouse.call) { cls = local.Int };
            vmap["x"] = new Number(mouse.x) { cls = local.Int };
            vmap["y"] = new Number(mouse.y) { cls = local.Int };
            this.mouse = mouse;
        }
        public override Obj Primary(ref int n, Local local, Primary primary, Obj val2)
        {
            if (val2.type == ObjType.Dot)
            {
                n++;
                val2 = primary.children[n];
                if (val2.type == ObjType.Word)
                {
                    return vmap["word"];
                }
            }
            throw new Exception();
        }
        public override Obj exep(ref int n, Local local, Primary primary)
        {
            return this;
        }
    }
    class Icon
    {
        public String filename;
        public Action<Sheet, Local> click;
        public static void modoshi(Sheet sheet, Local local)
        {
            var back = sheet.history.Back();
            if (back.s2 == null) return;
            sheet.cells[back.s2.y][back.s2.x].setText(back.s2.before, local);
            sheet.cells[back.s2.y][back.s2.x].update = true;
            sheet.ChangeCell(new RelationCell() { x = back.s2.x, y = back.s2.y }, false, local);
            if (back.s1 == null)
            {
                local.selects[0] = local.selects[1] = new Select() { state = new State() { elements = new List<Element> { new EndElement(null)} } };
            }
            else
            {
                sheet.cells[back.s1.y][back.s1.x].setText(back.s1.text, local);
                for (Element elem0 = sheet.cells[back.s2.y][back.s2.x]; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                sheet.ChangeCell(new RelationCell() { x = back.s2.x, y = back.s2.y }, false, local);
            }
            //var item = form.Start(local);
            //item.exe(local);
            local.panel.input = true;
        }
        public static void naoshi(Sheet sheet, Local local)
        {
            var go = sheet.history.Go();
            if (go == null) return;
            sheet.cells[go.y][go.x].setText(go.text, local);
            for (Element elem0 = sheet.cells[go.y][go.x]; elem0 != null; elem0 = elem0.parent) elem0.update = true;
            sheet.ChangeCell(new RelationCell() { x = go.x, y = go.y }, false, local);
            //var item = form.Start(local);
            //item.exe(local);
            local.panel.input = true;
        }
        public static void sum(Sheet sheet, Local local)
        {
            for (var i = 0; i < local.selects[0].state.elements.Count; i++)
            {
                var elem = local.selects[0].state.elements[i];
                if (elem == sheet)
                {
                    var cell = local.selects[0].state.elements[i + 1] as Cell;
                    sheet.before = cell.Text3(local);
                    cell.add(new Letter() { text = "&Sum(?)" });
                    local.selects[0].state.Update();
                    local.panel.input = true;
                    sheet.range = cell;
                }
            }
        }

        public static void brush(Sheet sheet, Local local)
        {
            var e = new SelectE();
            bool select = false;
            var span = new Span() { text = "" };
            var red = new StrObj("red");
            span.statuses["c"] = red;
            span.SetParam("c", red, local);
            e.Select = (e, element, s1, s2) =>
            {
                if (element is Span)
                {
                    var span2 = element as Span;
                    if (s2 == null)
                    {
                        if (s1 == null)
                        {
                            if (span.text != "")
                            {
                                span2.Before(span);
                                span = new Span() { text = "" };
                                span.statuses["c"] = red;
                                span.SetParam("c", red, local);
                            }
                            span2.statuses["c"] = red;
                            span2.SetParam("c", red, local);
                        }
                        else
                        {
                            if (s1.n == 0)
                            {
                                span2.statuses["c"] = red;
                                span2.SetParam("c", red, local);
                                if (span.text != "")
                                {
                                    span2.Before(span);
                                    span = new Span() { text = "" };
                                    span.statuses["c"] = red;
                                    span.SetParam("c", red, local);
                                } 
                            }
                            else if (s1.n != span2.text.Length)
                            {
                                var span3 = new Span() { text = span2.text.Substring(s1.n) };
                                span3.SetStatus(span2, local);
                                span3.statuses["c"] = red;
                                span3.SetParam("c", red, local);
                                span2.text = span2.text.Substring(0, s1.n);
                                span2.Next(span3);
                            }
                        }
                    }
                    else if (s1 == null)
                    {
                        if (span.text != "")
                        {
                            span2.Before(span);
                        }
                        if (s2.n == span2.text.Length)
                        {

                            span2.statuses["c"] = red;
                            span2.SetParam("c", red, local);
                        }
                        else if (s2.n != 0)
                        {
                            var span3 = new Span() { text = span2.text.Substring(0, s2.n)};
                            span3.SetStatus(span2, local);
                            span3.statuses["c"] = red;
                            span3.SetParam("c", red, local);
                            span2.text = span2.text.Substring(s2.n);
                            span2.Before(span3);
                        }
                    }
                }
                if (element is Kaigyou)
                {
                    if (s2 == null)
                    {
                        if (s1 == null)
                        {
                            element.Before(span);
                            span = new Span() { text = "" };
                            span.statuses["c"] = red;
                            span.SetParam("c", red, local);
                        }
                    }
                    else if (s1 == null)
                    {
                        s2.state.elements[s2.state.elements.Count - 1] = span;
                        s2.n = span.text.Length;
                        element.Before(span);
                        local.panel.input = true;
                        local.panel.Invalidate();
                    }
                }
                else if (element is Letter) {
                    var letter = element as Letter;
                    if (s2 == null)
                    {
                        if (s1 == null)
                        {
                            span.text = letter.text;
                            letter.next.RemoveBefore();
                        }
                        else
                        {
                            span.text = letter.text.Substring(s1.n);
                            letter.text = letter.text.Substring(0, s1.n);
                            s1.state.elements[s1.state.elements.Count - 1] = span;
                            s1.n = 0;
                            s1.state.Update();
                        }
                    }
                    else if (s1 == null)
                    {
                        span.text = letter.text.Substring(0, s2.n);
                        letter.text = letter.text.Substring(s2.n);
                        s2.state.elements[s2.state.elements.Count - 1] = span;
                        s2.n = span.text.Length;
                        s2.state.Update();
                        letter.Before(span);
                        local.panel.input = true;
                        local.panel.Invalidate();
                    }
                    else
                    {
                        if (s1.n == s2.n) return;
                        Letter let = new Letter() { text = letter.text.Substring(s2.n), type = LetterType.Letter };
                        span.text = letter.text.Substring(s1.n, s2.n - s1.n);
                        letter.text = letter.text.Substring(0, s1.n);
                        letter.Next(let);
                        letter.Next(span);
                        s1.state.elements[s1.state.elements.Count - 1] = span;
                        s1.n = 0;
                        s2.state.elements[s2.state.elements.Count - 1] = span;
                        s2.n = span.text.Length;
                        s1.state.Update();
                        local.panel.input = true;
                        local.panel.Invalidate();
                    }
                }
            };
            local.seln = -1;
            local.selects[0].state.n = local.selects[1].state.n = 0;
            local.SelectExe(e, local, ref select);
        }
    }
    class SheetHistory
    {
        public List<SheetString> texts = new List<SheetString>();
        public int n = 0;
        public SheetHistory()
        {
            n++;
            texts.Add(null);
        }
        public void Add(String text, String before, int x, int y)
        {
            for(var i = 0; i < before.Length; i++)
            {
                if (before[i] >= '\uE000')
                {
                    before = before.Substring(0, i) + before.Substring(i + 1);
                    i--;
                }
            }
            if (n != texts.Count) texts.RemoveRange(n, texts.Count - n);
            texts.Add(new SheetString() { text = text, before = before, x = x, y = y});
            n++;
        }
        public (SheetString s1, SheetString s2) Back()
        {
            if (n == 1) return (null, null);
            n--;
            return (texts[n - 1], texts[n]);
        }
        public SheetString Go()
        {
            if (n == texts.Count) return null;
            n++;
            return texts[n - 1];
        }
    }
    class SheetString
    {
        public String text;
        public String before;
        public int x;
        public int y;
    }
    class Sheet : Div
    {
        public const String alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public Dictionary<int, RowData> rowdatas = new Dictionary<int, RowData>();
        public Dictionary<int, ColData> coldatas = new Dictionary<int, ColData>();
        public SortedDictionary<int, SortedDictionary<int, Cell>> cells = new SortedDictionary<int, SortedDictionary<int, Cell>>();
        public bool tool;
        public List<Icon> icons = new List<Icon>();
        public Cell range = null;
        public bool edit = false;
        public SheetHistory history = new SheetHistory();
        public String before = null;
        public Sheet()
        {
            type = LetterType.Sheet;
            tag = "sheet";
            xtype = SizeType.Scroll;
            ytype = SizeType.Scroll;
            size.X = 500;
            size.Y = 300;
            icons.Add(new Icon() { filename = "modoshi.png", click = Icon.modoshi });
            icons.Add(new Icon() { filename = "naoshi.png", click = Icon.naoshi });
            icons.Add(new Icon() { filename = "sum.png", click = Icon.sum });
            icons.Add(new Icon() { filename = "paint.png", click = Icon.brush });
        }
        public override Obj SetParamB(string name, Obj obj, Local local)
        {
            switch(name)
            {
                case "tool":
                    if (obj.type == ObjType.BoolVal) return obj;
                    return new Obj(ObjType.Error);
            }
            return base.SetParamB(name, obj, local);
        }
        public override void SetParam(string name, Obj obj, Local local)
        {
            base.SetParam(name, obj, local);
            switch (name)
            {
                case "tool":
                    tool = (obj as BoolVal).value;
                    break;
            }
        }
        public override string Text(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|";
            foreach (var cs in cells)
            {
                foreach (var cell in cs.Value)
                {
                    ret += cell.Value.Text2(local);
                }
            }
            return ret + " >";
        }
        public override string Text2(Local local)
        {
            var ret = "<&" + tag;
            if (id != null) ret += " " + id;
            foreach (var kv in statuses)
            {
                ret += ",#" + kv.Key + " " + kv.Value.Text();
            }
            ret += "|";
            foreach (var cs in cells)
            {
                foreach (var cell in cs.Value)
                {
                    ret += cell.Value.Text2(local);
                }
            }
            return ret + " >";
        }
        public void add(int x, int y, Element element)
        {
            if (!cells.ContainsKey(y))
            {
                cells[y] = new SortedDictionary<int, Cell>();
            }
            var row = cells[y];
            if (!row.ContainsKey(x))
            {
                row[x] = new Cell();
            }
            row[x].add(element);
        }
        public int x = 1, y = 0;
        public override Element Measure(Measure m, Local local, ref int order)
        {
            Element next = null;
            for (var elem = childend.next; elem != childend; elem = next)
            {
                next = elem.next;
                if (elem.type == LetterType.Cell)
                {
                    var cell = elem as Cell;
                    int x, y;
                    if (cell.statuses.ContainsKey("y"))
                    {
                        var obj = cell.statuses["y"];
                        if (obj.type == ObjType.Number)
                        {
                            y = (obj as Number).value;
                        }
                        else if (obj.type == ObjType.BoolVal && (obj as BoolVal).value)
                        {
                            y = ++this.y;
                            this.x = 1;
                        }
                        else throw new Exception();
                    }
                    else
                    {
                        y = this.y;
                    }
                    if (cell.statuses.ContainsKey("x"))
                    {
                        x = (cell.statuses["x"] as Number).value;
                    }
                    else
                    {
                        x = this.x++;
                    }
                    if (!cells.ContainsKey(y))
                    {
                        cells[y] = new SortedDictionary<int, Cell>();
                    }
                    cells[y][x] = cell;
                }
                elem.next.RemoveBefore();
            }
            var measure = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel, state = m.state, Recompile = m.Recompile};
            if (font != null) measure.font = font;
            if (Recompile != null) measure.Recompile = Recompile;
            var font0 = measure.font;
            measure.x += margins[1] + paddings[1] + 60f;
            measure.y += margins[0] + paddings[0] + 18f;
            measure.sizex = size.X - margins[3] - paddings[3];
            measure.sizey = size.Y - margins[2] - paddings[2];
            var n0 = 0;
            foreach (var kv in cells)
            {
                var n = 0;
                var width = 0f;
                for (; n0 < kv.Key; n0++)
                {
                    if (rowdatas.ContainsKey(n0))
                    {
                        measure.py += rowdatas[n0].height;
                    }
                    else
                    {
                        measure.py += 18f;
                    }
                }
                foreach (var kv2 in cells[kv.Key])
                {
                    for (; n < kv2.Key; n++)
                    {
                        if (coldatas.ContainsKey(n))
                        {
                            measure.px += coldatas[n].width;
                        }
                        else
                        {
                            measure.px += 90f;
                        }
                    }
                    var measure2 = new Measure() { x = 0, y = 0, px = 0, py = 0, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel, state = m.state, Recompile = m.Recompile};
                    measure2.state.elements.Add(kv2.Value);
                    kv2.Value.Measure(measure2, local, ref order);
                    measure2.state.elements.RemoveAt(measure2.state.elements.Count - 1);
                    if (rowdatas.ContainsKey(n0))
                    {
                        if (rowdatas[n0].height < kv2.Value.size2.Y) rowdatas[n0].height = kv2.Value.size2.Y + 2;
                    }
                    else rowdatas[n0] = new RowData() { height = kv2.Value.size2.Y + 2 };
                }
                measure.px = measure.x;
            }
            pos2 = new Point((int)m.x, (int)(m.y + m.py));
            size3.X = measure.sizex + 10;
            size3.Y = measure.py + 10;
            size2 = new Point((int)(measure.sizex + margins[3] + paddings[3] + size.X + scroll.X + 101), (int)(measure.py + margins[2] + paddings[2] + size.Y + scroll.Y + 101));
            if (xtype == SizeType.Scroll) size2.Y += 10;
            if (ytype == SizeType.Scroll) size2.X += 10;
            update = false;
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            var px = g.px;
            var py = g.py;
            g.px += margins[1] + paddings[1];
            g.py += margins[0] + paddings[0];
            using (Bitmap bitmap = new Bitmap((int)size.X, (int)size.Y))
            {
                using (Graphics g2 = Graphics.FromImage(bitmap))
                {
                    g2.Clear(Color.White);
                    var height = 18f;
                    var height3 = 0f;
                    if (tool)
                    {
                        height += 18f;
                        height3 = 18f;
                        var w = 0f;
                        foreach (var icon in icons)
                        {
                            g2.DrawImage(System.Drawing.Image.FromFile(icon.filename), new RectangleF(w, 0, 18f, 18f));
                            w += 18f;
                        }
                    }
                    var width = 60f;
                    var n = 0;
                    width = 60f;
                    var sels = new int[2] { -1, -1 };
                    for (var i = 0; i < local.selects[0].state.elements.Count; i++)
                    {
                        if (local.selects[0].state.elements[i] == this)
                        {
                            sels[0] = i;
                            break;
                        }
                    }
                    for (var i = 0; i < local.selects[1].state.elements.Count; i++)
                    {
                        if (local.selects[1].state.elements[i] == this)
                        {
                            sels[1] = i;
                            break;
                        }
                    }
                    Select sel1 = null, sel2 = null;
                    Element elem = null;
                    if (sels[0] >= 0)
                    {
                        if (sels[1] >= 0)
                        {
                            var cell1 = local.selects[0].state.elements[sels[0] + 1] as Cell;
                            var cell2 = local.selects[1].state.elements[sels[1] + 1] as Cell;
                            var x1 = (cell1.statuses["x"] as Number).value;
                            var y1 = (cell1.statuses["y"] as Number).value;
                            var x2 = (cell2.statuses["x"] as Number).value;
                            var y2 = (cell2.statuses["y"] as Number).value;
                            if (x1 > x2)
                            {
                                var ins = x1;
                                x1 = x2;
                                x2 = ins;
                            }
                            if (y1 > y2)
                            {
                                var ins = y1;
                                y1 = y2;
                                y2 = ins;
                            }
                            float fx1 = 0, fx2 = 0, fy1 = 0, fy2 = 0;
                            for (var n0 = 0; ; n0++)
                            {
                                var height2 = 18f;
                                if (rowdatas.ContainsKey(n0))
                                {
                                    height2 = rowdatas[n0].height;
                                }
                                else
                                {
                                    height2 = 18f;
                                }
                                if (n0 == y1)
                                {
                                    fy1 = height;
                                    for (var n2 = 0; ; n2++)
                                    {
                                        var width2 = 90f;
                                        if (coldatas.ContainsKey(n2)) width2 = coldatas[n2].width;
                                        if (n2 == x1)
                                        {
                                            fx1 = width;
                                        }
                                        if (n2 == x2)
                                        {
                                            fx2 = width + width2;
                                            break;
                                        }
                                        width += width2;
                                    }
                                }
                                if (n0 == y2)
                                {
                                    fy2 = height + height2;
                                    break;
                                }
                                height += height2;
                                width = 60f;
                            }
                            elem = local.selects[0].state.elements[sels[0] + 1];
                            if (x1 != x2 || y1 != y2)
                            {
                                var state = new State();
                                state.elements.Add(this);
                                sel1 = local.selects[0];
                                sel2 = local.selects[1];
                                local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                            }
                            if (range != null) g2.FillRectangle(Brushes.LightSalmon, new RectangleF(fx1 - scroll.X, fy1 - scroll.Y, fx2 - fx1, fy2 - fy1));
                            else g2.FillRectangle(Brushes.LightBlue, new RectangleF(fx1 - scroll.X, fy1 - scroll.Y, fx2 - fx1, fy2 - fy1));
                        }
                        else elem = local.selects[0].state.elements[sels[0] + 1];
                    }
                    else if (sels[1] >= 0)
                    {
                        elem = local.selects[1].state.elements[sels[1] + 1];
                    }
                    height = 18f + height3;
                    width = 60f;
                    n = 0;
                    var g3 = new Graphic() { g = g2, font = g.font };
                    g3.px += 60f;
                    g3.py += height;
                    for (var n0 = 0; height < size.Y + scroll.Y; n0++)
                    {
                        var height2 = 18f;
                        if (rowdatas.ContainsKey(n0))
                        {
                            height2 = rowdatas[n0].height;
                        }
                        else
                        {
                            height2 = 18f;
                        }
                        if (scroll.Y + size.Y + height3 + 18f <= height) break;
                        else if (scroll.Y + height3 <= height)
                        {
                            for (n = 0; n < size.X + scroll.X; n++)
                            {
                                var width2 = 90f;
                                if (coldatas.ContainsKey(n)) width2 = coldatas[n].width;
                                if (scroll.X + size.X <= width) break;
                                if (scroll.X <= width)
                                {
                                    if (cells.ContainsKey(n0) && cells[n0].ContainsKey(n))
                                    {
                                        var g4 = new Graphic() { g = g2, font = g.font, px = 0, py = 0, x = g3.px + 1 - scroll.X, y = g3.py + 1 - scroll.Y };
                                        cells[n0][n].size.X = width2 - 1;
                                        if (elem == cells[n0][n])
                                        {
                                            if (range != null)
                                            {
                                                g2.FillRectangle(new SolidBrush(Color.FromArgb(255, 128 + Color.LightSalmon.R / 2, 128 + Color.LightSalmon.G / 2, 128 + Color.LightSalmon.B / 2)), new RectangleF(width - scroll.X + 1, height - scroll.Y + 1, width2 - 2, height2 - 2));
                                                g2.DrawRectangle(new Pen(Brushes.Salmon), new RectangleF(width - scroll.X, height - scroll.Y, width2, height2));
                                            }
                                            else
                                            {
                                                g2.FillRectangle(Brushes.White, new RectangleF(width - scroll.X + 1, height - scroll.Y + 1, width2 - 2, height2 - 2));
                                                g2.DrawRectangle(new Pen(Brushes.Blue), new RectangleF(width - scroll.X, height - scroll.Y, width2, height2));
                                            }
                                        }
                                        cells[n0][n].Draw(g4, local, ref select);
                                        g2.DrawLine(Pens.Gray, new PointF(width - scroll.X, height - scroll.Y), new PointF(width - scroll.X, height + height2 - scroll.Y));

                                    }
                                }
                                g3.px += width2;
                                width += width2;
                            }
                        }
                        g3.py += height2;
                        height += height2;
                        g3.px = g3.x + 60f;
                        width = 60f;
                        if (sel1 != null)
                        {
                            local.selects[0] = sel1;
                            local.selects[1] = sel2;
                        }
                    }
                    height = 18f + height3;
                    g2.FillRectangle(Brushes.LightGreen, new RectangleF(0, height, 60f, size.Y - height));
                    for (var n0 = 0; height < size.Y + scroll.Y; n0++)
                    {
                        if (scroll.Y + size.Y + 18f + height3 <= height) break;
                        var height2 = 18f;
                        if (rowdatas.ContainsKey(n0))
                        {
                            height2 = rowdatas[n0].height;
                        }
                        if (scroll.Y + height3 + 18f <= height + height2)
                        {
                            g2.DrawString((n0 + 1).ToString(), g.font, Brushes.Black, 30f - (n0 + 1).ToString().Length * 7.5f / 2, height - scroll.Y);
                            if (scroll.Y + height3 + 18f <= height)
                            {
                                g2.DrawLine(Pens.Gray, new PointF(0, height - scroll.Y), new PointF(size.X, height - scroll.Y));
                            }
                        }
                        height += height2;
                    }
                    g2.DrawLine(Pens.Gray, new PointF(0, 18f + height3), new PointF(size.X, 18f + height3));
                    width = 60f;
                    g2.FillRectangle(Brushes.LightGreen, new RectangleF(60f, height3, size.X - 60f, 18f));
                    for (n = 0; n < size.X + scroll.X; n++)
                    {
                        if (scroll.X + size.X <= width) break;
                        var width2 = 90f;
                        if (coldatas.ContainsKey(n))
                        {
                            width2 = coldatas[n].width;
                        }
                        if (scroll.X + 60f <= width + width2)
                        {
                            var ret = "";
                            var n2 = n;
                        head:
                            ret = (alpha[n2 % alpha.Length]).ToString() + ret;
                            if (n2 >= alpha.Length)
                            {
                                n2 /= alpha.Length;
                                n2--;
                                goto head;
                            }
                            if (scroll.X + 60f <= width)
                            {
                                g2.DrawLine(Pens.Gray, new PointF(width - scroll.X, height3), new PointF(width - scroll.X, size.Y));
                            }
                            g2.DrawString(ret, g.font, Brushes.Black, width + (width2 - 7.5f * ret.Length) / 2 - scroll.X, height3);
                        }
                        width += width2;
                    }
                    g2.DrawLine(Pens.Gray, new PointF(60f, height3), new PointF(60f, size.Y));
                    g2.FillRectangle(Brushes.LightBlue, new RectangleF(0, height3, 60f, 18f));
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px, g.y + g.py));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, bitmap.Width, size.Y), new RectangleF(0, 0, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(px + size.X - 10, py, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(px + size.X - 10, py + scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(0, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(0, 0, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(px + size.X - 10, py, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(px + size.X - 10, py + scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(px, py + size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(px + scroll.X / size2.X * size.X, py + size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public PointF size3;
        public override int Mouse(MouseEvent e, Local local)
        {
            var height5 = 0;
            if (tool) height5 = 18;
            if (xtype == SizeType.Scroll)
            {
                if (e.y >= size.Y - 10)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        if (e.x < scroll.X / size2.X * size.X)
                        {

                            scroll.X -= size.X - 60f - 10f;
                            if (scroll.X < 0) scroll.X = 0;
                            size2.X = (int)(size3.X + margins[3] + paddings[3] + size.X + scroll.X + 101);
                        }
                        else if (e.x < (scroll.X + size.X) / size2.X * size.X)
                        {
                            var x = scroll.X;
                            local.panel.capture = new Capture()
                            {
                                down = e,
                                capture = (c, e) =>
                                {
                                    scroll.X = x + (e.basepos.X - c.down.basepos.X) * size2.X / size.X;
                                    if (scroll.X < 0) scroll.X = 0;
                                    size2.X = (int)(size3.X + margins[3] + paddings[3] + size.X + scroll.X + 101);
                                    return true;
                                }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                        else
                        {
                            scroll.X += size.X - 60f - 10f;
                            size2.X = (int)(size3.X + margins[3] + paddings[3] + size.X + scroll.X + 101);
                        }
                    }
                    return -1;
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        if (e.y < scroll.Y / size2.Y * (size.Y - 12))
                        {
                            scroll.Y -= size.Y - 18f - height5 - 10f;
                            if (scroll.Y < 0) scroll.Y = 0;
                            size2.Y = (int)(size3.Y + margins[2] + paddings[2] + size.Y + scroll.Y + 101);
                        }
                        else if (e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
                        {
                            var y = scroll.Y;
                            local.panel.capture = new Capture()
                            {
                                down = e,
                                capture = (c, e) =>
                                {
                                    scroll.Y = y + (e.basepos.Y - c.down.basepos.Y) * size2.Y / size.Y;
                                    if (scroll.Y < 0) scroll.Y = 0;
                                    size2.Y = (int)(size3.Y + margins[2] + paddings[2] + size.Y + scroll.Y + 101);
                                    return true;
                                }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                        else
                        {
                            scroll.Y += size.Y - 18f - height5 - 10f;
                            size2.Y = (int)(size3.Y + margins[2] + paddings[2] + size.Y + scroll.Y + 101);
                        }
                    }
                    return -1;
                }
            }
            if (e.y < height5)
            {
                if (e.call == MouseCall.MouseDown)
                {
                    var x = e.x / 18;
                    if (x < icons.Count)
                    {
                        icons[x].click(this, local);
                    }
                }
                return -1;
            }
            else if (e.y < 18 + height5)
            {
                e.x += (int)scroll.X - 60;
                e.y += (int)scroll.Y - 18 - height5;
                if (tool) e.y += 18;
                var width3 = 0f;
                for (var n0 = 0; width3 < size.X + scroll.X; n0++)
                {
                    var width2 = 90f;
                    if (coldatas.ContainsKey(n0))
                    {
                        width2 = coldatas[n0].width;
                    }
                    else
                    {
                        coldatas[n0] = new ColData() { width = 90f };
                    }
                    width3 += width2;
                    if (-2 + width3 <= e.x && e.x <= width3 + 2)
                    {
                        var col = coldatas[n0];
                        var width4 = col.width;
                        if (e.call == MouseCall.MouseDown)
                        {
                            local.panel.capture = new Capture()
                            {
                                down = e,
                                capture = (c, e) =>
                                {
                                    col.width = width4 + (e.basepos.X - c.down.basepos.X);
                                    return true;
                                }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                    }
                }
            }
            else if (e.x < 60)
            {
                e.x += (int)scroll.X - 60;
                e.y += (int)scroll.Y - 18 - height5;
                var height3 = 0f;
                for (var n0 = 0; height3 < size.Y + scroll.Y; n0++)
                {
                    var height2 = 18f;
                    if (rowdatas.ContainsKey(n0))
                    {
                        height2 = rowdatas[n0].height;
                    }
                    else
                    {
                        rowdatas[n0] = new RowData() { height = 18f };
                    }
                    height3 += height2;
                    if (-2 + height3 <= e.y && e.y <= height3 + 2)
                    {
                        var row = rowdatas[n0];
                        var height4 = row.height;
                        if (e.call == MouseCall.MouseDown)
                        {
                            local.panel.capture = new Capture()
                            {
                                down = e,
                                capture = (c, e) =>
                                {
                                    row.height = height4 + (e.basepos.Y - c.down.basepos.Y);
                                    return true;
                                }
                            };
                            Form1.SetCapture(local.panel.Handle);
                        }
                    }
                }
            }
            else
            {
                e.x += (int)scroll.X - 60;
                e.y += (int)scroll.Y - 18 - height5;
            }
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            var height = 0f;
            for (var n0 = 0; height < size.Y + scroll.Y; n0++)
            {
                var height2 = 18f;
                if (rowdatas.ContainsKey(n0))
                {
                    height2 = rowdatas[n0].height;
                }
                if (height <= e.y && e.y < height + height2)
                {
                    var width = 0f;
                    for (var n = 0; n < size.X + scroll.X; n++)
                    {
                        var width2 = 90f;
                        if (coldatas.ContainsKey(n))
                        {
                            width2 = coldatas[n].width;
                        }
                        if (width <= e.x && e.x < width + width2)
                        {
                            if (!cells.ContainsKey(n0))
                            {
                                cells[n0] = new SortedDictionary<int, Cell>();
                            }
                            if (cells[n0].ContainsKey(n))
                            {
                                e.x -= (int)width;
                                e.y -= (int)height;
                                var elem = cells[n0][n];
                                e.state.elements.Add(elem);
                                var state = e.state.Clone();
                                if (edit)
                                {
                                    if (e.call == MouseCall.MouseDown)
                                    {
                                        edit = false;
                                        for (var i = 0; i < local.selects[0].state.elements.Count; i++)
                                        {
                                            if (local.selects[0].state.elements[i] == elem)
                                            {
                                                edit = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!edit)
                                {
                                    if (e.call == MouseCall.MouseDown)
                                    {
                                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                        return -1;
                                    }
                                    else if (e.call == MouseCall.MouseUp || e.call == MouseCall.MouseMove)
                                    {
                                        local.selects[1] = new Select() { state = state, n = 0 };
                                        return -1;
                                    }
                                }
                                if (e.call == MouseCall.DoubleClick)
                                {
                                    edit = true;
                                    e.call = MouseCall.MouseDown;
                                    elem.Mouse(e, local);
                                    e.call = MouseCall.DoubleClick;
                                }
                                else if (e.call == MouseCall.MouseMove)
                                {
                                    var check = false;
                                    for (var i = 0; i < local.selects[0].state.elements.Count; i++)
                                    {
                                        if (local.selects[0].state.elements[i] == elem)
                                        {
                                            check = true;
                                            break;
                                        }
                                    }
                                    if (check) elem.Mouse(e, local);
                                }
                                else elem.Mouse(e, local);
                                if (range != null) SetQuestion(range, local);
                            }
                            else
                            {
                                var div = new Cell() { };
                                div.statuses.Add("x", new Number(n));
                                div.statuses.Add("y", new Number(n0));
                                div.add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                                cells[n0][n] = div;
                                e.state.Update();
                                local.panel.input = true;
                                e.state.elements.Add(cells[n0][n]);
                                Element elem = cells[n0][n];
                                var state = e.state.Clone();
                                if (edit)
                                {
                                    if (e.call == MouseCall.MouseDown)
                                    {
                                        edit = false;
                                        for (var i = 0; i < local.selects[0].state.elements.Count; i++)
                                        {
                                            if (local.selects[0].state.elements[i] == elem)
                                            {
                                                edit = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!edit || range != null) {
                                    if (e.call == MouseCall.MouseDown)
                                    {
                                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                    }
                                    else if (e.call == MouseCall.MouseUp || e.call == MouseCall.MouseMove)
                                    {
                                        local.selects[1] = new Select() { state = state, n = 0 };
                                    }
                                    return -1;
                                }
                                cells[n0][n].Mouse(e, local);
                            head:
                                elem = elem.childend.before; ;
                                state.elements.Add(elem);
                                if (elem.single)
                                {
                                    if (e.call == MouseCall.MouseDown)
                                    {
                                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                    }
                                    else if (e.call == MouseCall.DoubleClick)
                                    {
                                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                                        edit = true;
                                        return -1;
                                    }
                                    else if (e.call == MouseCall.MouseUp)
                                    {
                                        local.selects[1] = new Select() { state = state, n = 0 };
                                    }
                                    if (range != null) SetQuestion(range, local);
                                }
                                else goto head;
                            }
                            break;
                        }
                        width += width2;
                    }
                }
                height += height2;
            }
            return -1;
        }

        private void SetQuestion(Cell range, Local local)
        {
            var ret = "";
            for (var i = 0; i < local.selects[0].state.elements.Count; i++)
            {
                if (local.selects[0].state.elements[i] == this)
                {
                    var cell = local.selects[0].state.elements[i + 1] as Cell;
                    ret += "^" + SetAlpha((cell.statuses["x"] as Number).value) + ((cell.statuses["y"] as Number).value + 1);
                }
            }
            for (var i = 0; i < local.selects[1].state.elements.Count; i++)
            {
                if (local.selects[1].state.elements[i] == this)
                {
                    var cell = local.selects[1].state.elements[i + 1] as Cell;
                    ret += "^" + SetAlpha((cell.statuses["x"] as Number).value) + ((cell.statuses["y"] as Number).value + 1);
                }
            }
            for (var line = range.childend.next; line.type != LetterType.ElemEnd; line = line.next)
            {
                if (line.type == LetterType.Line)
                {
                    for (var elem = line.childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
                    {
                        if (elem.type == LetterType.And)
                        {
                            var let = elem as AndLet;

                            int index = let.text.IndexOf('?');
                            if (index != -1)
                            {
                                let.name = let.text.Substring(0, index) + ret + let.text.Substring(index + 1);
                            }
                            else continue;
                            for (; elem != null; elem = elem.parent)
                            {
                                elem.update = true;
                            }
                            local.panel.input = true;
                            break;
                        }
                    }
                }
            }
        }
        public String SetAlpha(int n)
        {
            n += 1;
            var ret = "";
        head:
            ret += (char)('A' + n % ('Z' - 'A' + 2) - 1);
            n /= ('Z' - 'A' + 2);
            if (n == 0) return ret;
            goto head;
        }
        public SortedList<int, SortedList<Point, RelationCell>> list = new SortedList<int, SortedList<Point, RelationCell>>();
        public Dictionary<Point, int> tyouhuku = new Dictionary<Point, int>();
        public void ChangeCell(RelationCell relation, bool name, Local local)
        {
            if (tyouhuku.ContainsKey(new Point(relation.x, relation.y)))
            {
                tyouhuku[new Point(relation.x, relation.y)]++;
            }
            else tyouhuku[new Point(relation.x, relation.y)] = 1;
            var range = cells[relation.y][relation.x];
            for (var line = range.childend.next; line.type != LetterType.ElemEnd; line = line.next)
            {
                if (line.type == LetterType.Line)
                {
                    for (var elem = line.childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
                    {
                        if (elem.type == LetterType.And)
                        {
                            var let = elem as AndLet;
                            if (name) let.text = let.name;
                            for (Element elem0 = let; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                            foreach (var letter in Form1.Compile(let.text.Substring(1) + "\0", local))
                            {
                                let.add(letter);
                            }
                            let.add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                            local.state = new State();
                            local.state.elements.Add(let);
                            local.state.plus(0);
                            var error = false;
                            var lines = local.panel.form.Lines(local, LetterType.Semicolon, LetterType.Comma, LetterType.Kaigyou, LetterType.End, ObjType.AndBlock, 0, ref error).tobj as Block;
                            local.blockslist.Add(new List<Block>());
                            local.blocks.Add(lines);
                            lines.vmap["Sum"] = new Sum(this, range);
                            var lins = lines.Clone().exe(local).Getter(local) as Block;
                            let.name = "";
                            foreach (var ret in lins.rets)
                            {
                                if (ret.type == ObjType.Number)
                                {
                                    let.name += (ret as Number).value;
                                }
                                else if (ret.type == ObjType.StrObj)
                                {
                                    let.name += (ret as StrObj).value;
                                }
                            }
                        }
                    }
                }
            }
            foreach (var rela in range.relations.relations.Values)
            {
                if (!list.ContainsKey(rela.count.count)) list[rela.count.count] = new SortedList<Point, RelationCell>();
                if (tyouhuku.ContainsKey(new Point(rela.x, rela.y)) && tyouhuku[new Point(rela.x, rela.y)] >= 20) continue;
                list[rela.count.count][new Point(rela.x, rela.y)] = rela;
            }
            CheckList(local);
        }
        public void CheckList(Local local)
        {
            for(var i = 0; i < list.Count; i++)
            {
                var k = list.Keys[i];
                var l = list.Values[i];
                for (var j = 0; ; j++)
                {
                    var k2 = l.Keys[j];
                    ChangeCell(l.Values[j], false, local);
                    l.Remove(k2);
                    if (l.Count == 0)
                    {
                        list.Remove(k);
                        i = -1;
                        break;
                    }
                }
            }
        }
        public override void SelectExe(SelectE e, Local local, ref bool select)
        {
            select = false;
            var seln = -1;
            if (range != null)
            {
                return;
            }
            foreach (var cs in cells)
            {
                foreach (var element in cs.Value.Values)
                {
                    if (local.selects[0].state.elements[local.selects[0].state.n] == element)
                    {
                        if (local.selects[0] != local.selects[1]) local.selects[0].state.n++;
                        seln = 0;
                        if (local.selects[1].state.elements[local.selects[1].state.n] == element)
                        {
                            seln = 1;
                            local.selects[1].state.n++;
                            if (edit)
                            {
                                e.state.elements.Add(element);
                                var text = element.Text3(local);
                                element.SelectExe(e, local, ref select);
                                local.seln = 2;
                                return;
                            }
                        }
                    }
                }
            }
            return;
        }

        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            select = false;
            var seln = -1;
            if (e.ctrl)
            {
                switch(e.key)
                {
                    case Keys.Y:
                        var go = history.Go();
                        if (go == null) return -1;
                        cells[go.y][go.x].setText(go.text, local);
                        for (Element elem0 = cells[go.y][go.x]; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                        ChangeCell(new RelationCell() { x = go.x, y = go.y }, false, local);
                        //var item = form.Start(local);
                        //item.exe(local);
                        local.panel.input = true;
                        local.seln = 2;
                        return -1;
                    case Keys.Z:
                        var back = history.Back();
                        if (back.s2 == null) return -1;
                        cells[back.s2.y][back.s2.x].setText(back.s2.before, local);
                        cells[back.s2.y][back.s2.x].update = true;
                        ChangeCell(new RelationCell() { x = back.s2.x, y = back.s2.y }, false, local);
                        if (back.s1 == null)
                        {
                            local.selects[0] = local.selects[1] = new Select() { state = new State() { elements = new List<Element> { new EndElement(null) } } };
                        }
                        else
                        {
                            cells[back.s1.y][back.s1.x].setText(back.s1.text, local);
                            for (Element elem0 = cells[back.s1.y][back.s1.x]; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                            ChangeCell(new RelationCell() { x = back.s1.x, y = back.s1.y }, false, local);
                            //var item = form.Start(local);
                            //item.exe(local);
                            local.panel.input = true;
                        }
                        local.seln = 2;
                        return -1;
                }
            }
            if (range != null)
            {
                switch (e.key)
                {
                    case Keys.Enter:
                        local.selects[0] = local.selects[1] = new Select();
                        local.selects[0].state = new State();
                        local.selects[0].state.elements.Add(new EndElement(null));
                        var x = (range.statuses["x"] as Number).value;
                        var y = (range.statuses["y"] as Number).value;
                        for (Element elem0 = cells[y][x]; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                        ChangeCell(new RelationCell() { x = x, y = y }, true, local);
                        history.Add(range.Text3(local), before, x, y);
                        range = null;
                        local.seln = 2;
                        break;
                }
                return -1;
            }
            foreach (var cs in cells)
            {
                foreach (var element in cs.Value.Values)
                {
                    if (local.selects[0].state.elements[local.selects[0].state.n] == element)
                    {
                        if (local.selects[0] != local.selects[1]) local.selects[0].state.n++;
                        seln = 0;
                        if (local.selects[1].state.elements[local.selects[1].state.n] == element)
                        {
                            seln = 1;
                            local.selects[1].state.n++;
                            if (edit)
                            {
                                e.state.elements.Add(element);
                                var text = element.Text3(local);
                                element.Key(e, local, ref select);
                                if (local.panel.input == true)
                                {
                                    var x = (element.statuses["x"] as Number).value;
                                    var y = (element.statuses["y"] as Number).value;
                                    history.Add(element.Text3(local), text, x, y);
                                    ChangeCell(new RelationCell() { x = x, y = y}, false, local);
                                }
                                if (local.seln == 2) return 0;
                                e.state.elements.RemoveAt(e.state.elements.Count - 1);

                            }
                            else
                            {

                                bool move = false;
                                int dx = 0, dy = 0;
                                switch (e.key)
                                {
                                    case Keys.Left:
                                        move = true;
                                        dx = -1;
                                        break;
                                    case Keys.Right:
                                        move = true;
                                        dx = 1;
                                        break;
                                    case Keys.Up:
                                        move = true;
                                        dy = -1;
                                        break;
                                    case Keys.Down:
                                        move = true;
                                        dy = 1;
                                        break;
                                    case Keys.Enter:
                                        edit = true;
                                        Element elem = element;
                                    head:
                                        e.state.elements.Add(elem);
                                        if (!elem.single)
                                        {
                                            elem = elem.childend.before;
                                            goto head;
                                        }
                                        local.selects[0] = local.selects[1] = new Select() { state = e.state.Clone(), n = 0 };
                                        local.seln = 2;
                                        return 0;
                                    case Keys.Back:
                                    case Keys.Delete:
                                        var x1 = (element.statuses["x"] as Number).value;
                                        var y1 = (element.statuses["y"] as Number).value;
                                        var text1 = element.Text3(local);
                                        element.setText("", local);
                                        for (Element elem0 = element; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                                        ChangeCell(new RelationCell() { x = x1, y = y1 }, false, local);
                                        local.panel.input = true;
                                        Element elem1 = element;
                                    head1:
                                        e.state.elements.Add(elem1);
                                        if (!elem1.single)
                                        {
                                            elem1 = elem1.childend.before;
                                            goto head1;
                                        }
                                        local.selects[0] = local.selects[1] = new Select() { state = e.state.Clone(), n = 0 };
                                        history.Add(element.Text3(local), text1, x1, y1);
                                        edit = true;
                                        local.seln = 2;
                                        return 0;
                                    case Keys.None:
                                        var x2 = (element.statuses["x"] as Number).value;
                                        var y2 = (element.statuses["y"] as Number).value;
                                        var state2 = e.state.Clone();
                                        var text2 = element.Text3(local);
                                        element.setText(e.text, local);
                                        for (Element elem0 = cells[y2][x2]; elem0 != null; elem0 = elem0.parent) elem0.update = true;
                                        ChangeCell(new RelationCell() { x = x2, y = y2 }, false, local);
                                        local.panel.input = true;
                                        Element elem2 = element;
                                        elem2 = element;
                                    head2:
                                        e.state.elements.Add(elem2);
                                        if (!elem2.single)
                                        {
                                            elem2 = elem2.childend.before;
                                            goto head2;
                                        }
                                        local.selects[0] = local.selects[1] = new Select() { state = e.state.Clone(), n = 0 };
                                        edit = true;
                                        history.Add(element.Text3(local), text2, x2, y2);
                                        local.seln = 2;
                                        return 0;
                       
                                }
                                if (move)
                                {
                                    var cell = element as Cell;
                                    var x = (cell.statuses["x"] as Number).value + dx;
                                    var y = (cell.statuses["y"] as Number).value + dy;
                                    if (x < 0 || y < 0) return 0;
                                    if (!cells.ContainsKey(y))
                                    {
                                        cells[y] = new SortedDictionary<int, Cell>();
                                    }
                                    if (!cells[y].ContainsKey(x))
                                    {
                                        var div = new Cell() { background = Brushes.White };
                                        div.statuses.Add("x", new Number(x));
                                        div.statuses.Add("y", new Number(y));
                                        div.add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                                        cells[y][x] = div;
                                    }
                                    Element cell2 = cells[y][x];
                                    local.selects[seln].state.elements[local.selects[seln].state.n - 1] = cell2;
                                    for (var i = local.selects[seln].state.elements.Count - 1; i >= local.selects[seln].state.n; i--)
                                    {
                                        local.selects[seln].state.elements.RemoveAt(i);
                                    }
                                head:
                                    local.selects[seln].state.elements.Add(cell2.childend.next);
                                    if (!cell2.childend.next.single)
                                    {
                                        cell2 = cell2.childend.next;
                                        goto head;
                                    }
                                    local.selects[seln].n = 0;
                                    local.selects[(seln + 1) % 2] = local.selects[seln];
                                    local.seln = 2;
                                    return 0;
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
    }
    class SortList
    {
        public SortedList<Point, RelationCell> relations = new SortedList<Point, RelationCell>();
    }
    class RelationCell
    {
        public int x, y;
        public ObjInt count;
    }
    class ObjInt
    {
        public int count;
    }
    class RowData : Line
    {
        public float height;
    }
    class ColData
    {
        public float width;
    }
    class Cell : Div
    {
        public SortList relations = new SortList();
        public Cell()
        {
            tag = "cell";
            type = LetterType.Cell;
        }
        public String Text3(Local local)
        {
            var ret = "";
            for (var elem = childend.next; elem.type != LetterType.ElemEnd; elem = elem.next)
            {
                ret += elem.Text(local);
            }
            return ret;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            g.x += margins[1] + paddings[1];
            g.y += margins[0] + paddings[0];
            using (Bitmap bitmap = new Bitmap((int)size2.X, (int)size2.Y))
            {
                using (Graphics g2 = Graphics.FromImage(bitmap))
                {
                    if (background != null)
                    {
                        g2.FillRectangle(background, new RectangleF(0, 0, (int)size2.X, (int)size2.Y));
                    }
                    else g2.Clear(Color.Transparent);
                    var g3 = new Graphic() { g = g2, font = g.font };
                    var first = true;
                    var sizey = 0f;
                    var posy = 0f;
                    for (var elem = childend.next; elem != childend; elem = elem.next)
                    {
                        if (align == Align.Left)
                        {
                            g3.px = 0;
                        }
                        else if (align == Align.Center)
                        {
                            g3.px = (size.X - elem.size2.X) / 2;
                        }
                        else if (align == Align.Right)
                        {
                            g3.px = size.X - elem.size2.X;
                        }
                        else if (align == Align.Separate)
                        {
                            if (first)
                            {
                                g3.px = 0;
                                g3.py = posy;
                                elem.Draw(g3, local, ref select);
                                sizey = elem.size2.Y;
                            }
                            else
                            {
                                g3.py = posy;
                                g3.px = (size2.X - elem.size2.X);
                                elem.Draw(g3, local, ref select);
                                posy += Math.Max(sizey, elem.size2.Y);
                                sizey = 0;
                            }
                            first = !first;
                            continue;
                        }
                        elem.Draw(g3, local, ref select);
                        g3.py += elem.size2.Y;
                    }
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px, g.y + g.py));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, bitmap.Width, size.Y), new RectangleF(0, scroll.Y, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto || ytype == SizeType.Break)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(scroll.X, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(scroll.X, scroll.Y, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(0, size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(scroll.X / size2.X * size.X, size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (xtype == SizeType.Scroll)
            {
                if (e.y >= size.Y - 10)
                {
                    if (scroll.X / size2.X * size.X <= e.x && e.x < (scroll.X + size.X) / size2.X * size.X)
                    {
                        var x = scroll.X;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.X = x + (e.x - c.down.x) * size2.X / size.X;
                                if (scroll.X > size2.X - size.X) scroll.X = size2.X - size.X;
                                if (scroll.X < 0) scroll.X = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (scroll.Y / size2.Y * (size.Y - 12) <= e.y && e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
                    {
                        var y = scroll.Y;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.Y = y + (e.y - c.down.y) * size2.Y / size.Y;
                                if (scroll.Y > size2.Y - size.Y) scroll.Y = size2.Y - size.Y;
                                if (scroll.Y < 0) scroll.Y = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            e.x += (int)scroll.X;
            e.y += (int)scroll.Y;
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            for (var element = childend.next; element != childend; element = element.next)
            {
                if (element.pos2.Y <= e.y && e.y < element.pos2.Y + element.size2.Y)
                {
                    select = true;
                    e.state.elements.Add(element);
                    if (align == Align.Left) { }
                    else if (align == Align.Center)
                    {
                        e.x -= (int)((size.X - element.size2.X) / 2);
                    }
                    else if (align == Align.Right)
                    {
                        e.x -= (int)(size.X - element.size2.X);
                    }
                    else if (align == Align.Separate)
                    {
                        if (e.x > (int)(size.X - element.next.size2.X))
                        {
                            e.state.elements[e.state.elements.Count - 1] = element.next;
                            ret = element.next.Mouse(e, local);
                            e.state.elements.RemoveAt(e.state.elements.Count - 1);
                            return 0;
                        }
                    }
                    ret = element.Mouse(e, local);
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                    return 0;
                }
            }
            if (!select)
            {
                Element elem = this;
                var state = e.state.Clone();
            head:
                elem = elem.childend.before;
                state.elements.Add(elem);
                if (elem.single)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                    }
                    else
                    {
                        local.selects[1] = new Select() { state = state, n = 0 };
                    }
                }
                else goto head;
            }
            return -1;
        }

    }
    class CloneElement : Div
    {
        public Element childstart;
        public CloneElement(Element start, Element end)
        {
            childstart = start;
            childend = end;
            type = LetterType.CloneElement;
        }
        public override void nextplus(State state)
        {
            state.elements[state.elements.Count - 1] = state.elements.Last().next;
        }
        public override string Text(Local local)
        {
            return "";
        }
        public override string Text2(Local local)
        {
            return "";
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            pos2.X = m.px;
            pos2.Y = m.py;
            var py = 0f;
            if (childstart.next.before != childstart)
            {
                next.RemoveBefore();
                return null;
            }
            var measure = new Measure() { x = m.x, y = m.y, px = m.px, py = m.py, xtype = xtype, ytype = ytype, font = m.font, g = m.g, panel = m.panel, Recompile = m.Recompile };
            for (var element = childstart; ; element = element.next)
            {
                if (element == childstart.before)
                {
                    next.RemoveBefore();
                    return null;
                }
                if (measure.sizex < element.size2.X) measure.sizex = element.size2.X;
                measure.py += element.size2.Y;
                py += element.size2.Y;
                if (element == childend) break;
            }
            size2.X = m.sizex;
            size2.Y = py;
            return null;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            g.x += margins[1] + paddings[1];
            g.y += margins[0] + paddings[0];
            using (Bitmap bitmap = new Bitmap((int)size2.X, (int)size2.Y))
            {
                using (Graphics g2 = Graphics.FromImage(bitmap))
                {
                    if (background != null)
                    {
                        g2.FillRectangle(background, new RectangleF(0, 0, (int)size2.X, (int)size2.Y));
                    }
                    else g2.Clear(Color.Transparent);
                    var g3 = new Graphic() { g = g2, font = g.font };
                    for (var elem = childstart; ; elem = elem.next)
                    {
                        elem.Draw(g3, local, ref select);
                        g3.px = g3.x;
                        g3.py += elem.size2.Y;
                        if (elem == childend) break;
                    }
                }
                if (xtype == SizeType.Auto || xtype == SizeType.Break)
                {
                    if (ytype == SizeType.Auto)
                    {
                        g.g.DrawImage(bitmap, new PointF(g.x + g.px, g.y + g.py));
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, bitmap.Width, size.Y), new RectangleF(0, scroll.Y, bitmap.Width, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                }
                else
                {
                    if (ytype == SizeType.Auto || ytype == SizeType.Break)
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, bitmap.Height), new RectangleF(scroll.X, 0, size.X, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.g.DrawImage(bitmap, new RectangleF(g.x + g.px, g.y + g.py, size.X, size.Y), new RectangleF(scroll.X, scroll.Y, size.X, size.Y), GraphicsUnit.Pixel);
                        if (ytype == SizeType.Scroll)
                        {
                            g.g.FillRectangle(Brushes.LightGray, new RectangleF(size.X - 10, 0, 10, size.Y - 12));
                            g.g.FillRectangle(Brushes.Gray, new RectangleF(size.X - 10, scroll.Y / size2.Y * (size.Y - 12), 10, Math.Min(size.Y / size2.Y * (size.Y - 12), size.Y - 12 - scroll.Y / size2.Y * (size.Y - 12))));
                        }
                    }
                    if (xtype == SizeType.Scroll)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(0, size.Y - 10, size.X, 10));
                        g.g.FillRectangle(Brushes.Gray, new RectangleF(scroll.X / size2.X * size.X, size.Y - 10, Math.Min(size.X / size2.X * size.X, size.X - scroll.X / size2.X * size.X), 10));
                    }
                }
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (xtype == SizeType.Scroll)
            {
                if (e.y >= size.Y - 10)
                {
                    if (scroll.X / size2.X * size.X <= e.x && e.x < (scroll.X + size.X) / size2.X * size.X)
                    {
                        var x = scroll.X;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.X = x + (e.x - c.down.x) * size2.X / size.X;
                                if (scroll.X > size2.X - size.X) scroll.X = size2.X - size.X;
                                if (scroll.X < 0) scroll.X = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            if (ytype == SizeType.Scroll)
            {
                if (e.x >= size.X - 10 && e.y <= size.Y - 10)
                {
                    if (scroll.Y / size2.Y * (size.Y - 12) <= e.y && e.y < (scroll.Y + size.Y) / size2.Y * (size.Y - 12))
                    {
                        var y = scroll.Y;
                        local.panel.capture = new Capture()
                        {
                            down = e,
                            capture = (c, e) =>
                            {
                                scroll.Y = y + (e.y - c.down.y) * size2.Y / size.Y;
                                if (scroll.Y > size2.Y - size.Y) scroll.Y = size2.Y - size.Y;
                                if (scroll.Y < 0) scroll.Y = 0;
                                return true;
                            }
                        };
                    }
                    Form1.SetCapture(local.panel.Handle);
                    return -1;
                }
            }
            e.x += (int)scroll.X;
            e.y += (int)scroll.Y;
            var ret = -1;
            var select = false;
            if (mouse != null) mouse(e, local.local);
            var posy = 0f;
            for (var element = childstart; ; element = element.next)
            {
                if (posy <= e.y && e.y < posy + element.size2.Y)
                {
                    select = true;
                    e.state.elements.Add(element);
                    ret = element.Mouse(e, local);
                    e.state.elements.RemoveAt(e.state.elements.Count - 1);
                    return 0;
                }
                posy += element.size2.Y;
                if (element == childend) break;
            }
            if (!select)
            {
                Element elem = this;
                var state = e.state.Clone();
            head:
                elem = elem.childend.before;
                state.elements.Add(elem);
                if (elem.single)
                {
                    if (e.call == MouseCall.MouseDown)
                    {
                        local.selects[0] = local.selects[1] = new Select() { state = state, n = 0 };
                    }
                    else
                    {
                        local.selects[1] = new Select() { state = state, n = 0 };
                    }
                }
                else goto head;
            }
            return -1;
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (select) select = false;
            e.state.elements.Add(childstart);
            var go = false;
            for (var element = e.state.elements.Last(); ; element = e.state.elements[e.state.elements.Count - 1] = e.state.elements.Last().next)
            {
                if (local.selects[0].state.elements[local.selects[0].state.n] == element)
                {
                    local.selects[0].state.n++;
                    go = true;
                }
                if (local.selects[1].state.elements[local.selects[1].state.n] == element)
                {
                    local.selects[1].state.n++;
                    go = true;
                }
                if (select || go)
                {
                    element.Key(e, local, ref select);
                    if (local.seln == 2) return 0;
                    go = false;
                }
                if (element == childend) break;
            }
            e.state.elements.RemoveAt(e.state.elements.Count - 1);
            return 0;
        }
    }
    class AndLet : Letter
    {
        public bool swithed = true;
        public AndLet() : base()
        {
            single = true;
        }
        public override int Count()
        {
            return text.Length;
        }
        public override void Draw(Graphic g, Local local, ref bool select)
        {
            if (swithed)
            {
                if (local.comlet != null)
                {
                    return;
                }
                if (select)
                {
                    if (local.selects[(local.seln + 1) % 2].state.elements.Last() == this)
                    {
                        g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px, g.y + g.py, 7.5f * local.selects[(local.seln + 1) % 2].n + 1, size2.Y));
                        select = false;
                    }
                    else g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px, g.y + g.py, size2.X, size2.Y));
                }
                else
                {
                    for (var i = 0; i < 2; i++)
                    {
                        if (local.selects[i].state.elements.Last() == this)
                        {
                            if (local.selects[(i + 1) % 2].state.elements.Last() == this)
                            {
                                int n1 = local.selects[i].n, n2 = local.selects[(i + 1) % 2].n;
                                if (n1 > n2)
                                {
                                    var ins = n1;
                                    n1 = n2;
                                    n2 = ins;
                                }
                                g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + n1 * 7.5f, g.y + g.py, (n2 - n1) * 7.5f + 1, size2.Y));
                            }
                            else
                            {
                                int n1 = local.selects[i].n;
                                g.g.FillRectangle(Brushes.LightGray, new RectangleF(g.x + g.px + n1 * 7.5f, g.y + g.py, (text.Length - n1) * 7.5f + 1, size.Y));
                                local.seln = i;
                                select = !select;
                            }
                            break;
                        }
                    }
                }
                if (font != null) g.font = font;
                g.g.DrawString(name, g.font, brush, g.x + g.px, g.y + g.py);
                g.px += size2.X;
            }
            else
            {
                base.Draw(g, local, ref select);
            }
        }
        public override Element Measure(Measure m, Local local, ref int order)
        {
            if (swithed)
            {
                if (local.comlet != null)
                {
                    return null;
                }
                if (font != null) m.font = font;
                var s = m.g.MeasureString(name, m.font);
                if (m.xtype == SizeType.Break)
                {
                    if (m.px + s.Width > m.sizex)
                    {
                        return new VirtualLine();
                    }
                }
                else
                {
                    if (m.px > m.sizex) m.sizex = m.px;
                }
                pos2 = new PointF(m.x + m.px, m.y + m.py);
                size2.X = 7.5f * name.Length;
                size2.Y = s.Height;
                if (m.h < size2.Y) m.h = size2.Y;
                m.px += size2.X;
                size = new PointF(s.Width, s.Height);
                return null;
            }
            else
            {
                return base.Measure(m, local, ref order);
            }
        }
        public override int Mouse(MouseEvent e, Local local)
        {
            if (swithed) { return -1; }
            else
            {
                return base.Mouse(e, local);
            }
        }
        public override int Key(KeyEvent e, Local local, ref bool select)
        {
            if (swithed) { return -1; }
            else
            {
                return base.Key(e, local, ref select);
            }
        }
        public override int plus(int n)
        {
            return 0;
        }
        public override void nextplus(State state)
        {
            if (state.elements.Count == 1)
            {
                state.elements.Add(childend.next);
                state.elements[state.elements.Count - 2] = state.elements[state.elements.Count - 2].next;
            }
            else
            {
                state.elements[state.elements.Count - 1] = state.elements.Last().next;
                if (state.elements.Last().type == LetterType.ElemEnd) state.elements.RemoveAt(state.elements.Count - 1);
            }
        }
        public override string Text(Local local)
        {
            var str = "";
            if (local.selects[0].state.elements.Last() == this)
            {
                str += (char)('\uE000' + local.selects[0].n * 2);
            }
            if (local.selects[1].state.elements.Last() == this)
            {
                str += (char)('\uE000' + local.selects[1].n * 2 + 1);
            }
            return str + text;
        }
        public override string Text2(Local local)
        {
            return text;
        }
    }
}
