using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Form1 : Form
    {
        public static List<Letter> Compile(string text)
        {
            List<Letter> letters = new List<Letter>();
            var bk = 0;
            var row = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (('a' <= text[i] && text[i] <= 'z') || ('A' <= text[i] && text[i] <= 'Z'))
                {
                    var j = i + 1;
                    for (; j < text.Length; j++)
                    {
                        if (('a' <= text[j] && text[j] <= 'z') || ('A' <= text[j] && text[j] <= 'Z') || ('0' <= text[j] && text[j] <= '9')) continue;
                        break;
                    }
                    Letter l = new Letter() {text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Letter}.Let();
                    letters.Add(l);
                    i = j - 1;
                }
                else if ('0' <= text[i] && text[i] <= '9')
                {
                    var j = i + 1;
                    var flv = false;
                    for (; j < text.Length; j++)
                    {
                        if ('0' <= text[j] && text[j] <= '9') continue;
                        else if (text[j] == '.')
                        {
                            if (flv) break;
                            j++;
                            if ('0' <= text[j] && text[j] <= '9')
                            {
                                flv = true;
                                continue;
                            }
                            else
                            {
                                j--;
                                break;
                            }
                        }
                        break;
                    }
                    if (flv) letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Decimal });
                    else letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Number });
                    i = j - 1;
                }
                else if (text[i] == '"')
                {
                    var j = i + 1;
                    for (; j < text.Length; j++)
                    {
                        if (text[j] == '"')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i + 1), name = text.Substring(i + 1, j - i - 1), type = LetterType.Str, brush = Brushes.Brown});
                            break;
                        }
                        else if (text[j] == '\n')
                        {

                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.Str, brush = Brushes.Brown });
                            j--;
                            break;
                        }
                    }
                    i = j;
                }
                else if (text[i] == '`')
                {
                    var j = i + 1;
                    for (; j < text.Length; j++)
                    {
                        if (text[j] == '`')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i + 1), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter });
                            break;
                        }
                        else if (text[j] == '<' || text[j] == '>')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter });
                            j--;
                            break;
                        }
                        else if (text[j] == '\n')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter});
                            j--;
                            break;
                        }
                    }
                    i = j;
                }
                else if (text[i] == ' ')
                {
                    var j = i + 1;
                    for (; j < text.Length; j++)
                    {
                        if (text[j] == ' ') continue;
                        else break;
                    }
                    letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Space});
                    i = j - 1;
                }
                else if (text[i] == ':')
                {
                    letters.Add(new Letter() {text = ":", name = ":", type = LetterType.Colon});
                }
                else if (text[i] == ';')
                {
                    letters.Add(new Letter() { text = ";", name = ";", type = LetterType.Semicolon});
                }
                else if (text[i] == '$')
                {
                    letters.Add(new Letter() { text = "$", name = "$", type = LetterType.Dolor });
                }
                else if (text[i] == ',')
                {
                    letters.Add(new Letter() {text = ",", name = ",", type = LetterType.Comma});
                }
                else if (text[i] == '|')
                {
                    letters.Add(new Letter() { text = "|", name = "|", type = LetterType.Bou});
                }
                else if (text[i] == '#')
                {
                    letters.Add(new Letter() {text = "#", name = "#", type = LetterType.Sharp});
                }
                else if (text[i] == '@')
                {
                    var j = i + 1;
                    if (('a' <= text[j] && text[j] <= 'z') || ('A' <= text[j] && text[j] <= 'Z'))
                    {
                        j++;
                        for (; j < text.Length; j++)
                        {
                            if (('a' <= text[j] && text[j] <= 'z') || ('A' <= text[j] && text[j] <= 'Z')) continue;
                            break;
                        }
                        letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.AtLetter});
                        i = j - 1;
                    }
                }
                else if (text[i] == '\n')
                {
                    var j = i+1;
                    var kaigyou = new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou};
                    letters.Add(kaigyou);
                    for (; j < text.Length; j++)
                    {
                        if (text[j] == ' ')
                        {
                            i = j;
                            j++;
                            for (; j < text.Length; j++)
                            {
                                if (text[j] == ' ') continue;
                                else break;
                            }
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Space});
                            j--;
                        }
                        else if (text[j] == '\n')
                        {
                            letters.Add(new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou});
                        }
                        else break;
                    }
                    i = j - 1;
                    row++;
                }
                else if (text[i] == '=')
                {
                    if (i + 1 < text.Length && text[i + 1] == '=')
                    {
                        i++;
                        letters.Add(new Letter() { text = "==", name = "==", type = LetterType.EqualEqual});
                    }
                    else letters.Add(new Letter() { text = "=", name = "=", type = LetterType.Equal});
                }
                else if (text[i] == '!')
                {
                    if (i + 1 < text.Length && text[i + 1] == '=')
                    {
                        i++;
                        letters.Add(new Letter() { text = "!=", name = "!=", type = LetterType.NotEqual });
                    }
                    else letters.Add(new Letter() { text = "!", name = "!", type = LetterType.Not });
                }
                else if (text[i] == '+')
                {
                    letters.Add(new Letter() { text = "+", name = text[i].ToString(), type = LetterType.Plus});
                }
                else if (text[i] == '-')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '>')
                        {
                            letters.Add(new Letter() { text = "->", name = "->", type = LetterType.Right});
                            i++;
                        }
                        else letters.Add(new Letter() {text = "-", name = "-", type = LetterType.Minus});
                    }
                    else letters.Add(new Letter() { text = "-", name = "-", type = LetterType.Minus});
                }
                else if (text[i] == '<')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '-')
                        {
                            letters.Add(new Letter() { text = "<-", name = "<-", type = LetterType.Left});
                            i++;
                        }
                        else if (text[i + 1] == '=')
                        {
                            letters.Add(new Letter() { text = "<=", name = "<=", type = LetterType.LessEqual});
                            i++;
                        }
                        else letters.Add(new Letter() {text = "<", name = "<", type = LetterType.LessThan});
                    }
                    else letters.Add(new Letter() { text = "<", name = "<", type = LetterType.LessThan});
                }
                else if (text[i] == '>')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '=')
                        {
                            letters.Add(new Letter() { text = ">=", name = ">=", type = LetterType.MoreEqual});
                            i++;
                        }
                        else if (text[i + 1] == '>')
                        {
                            letters.Add(new Letter() { text = ">>", name = ">>", type = LetterType.Gpt});
                            i++;
                        }
                        else letters.Add(new Letter() { text = ">", name = ">", type = LetterType.MoreThan});
                    }
                    else letters.Add(new Letter() { text = ">", name = ">", type = LetterType.MoreThan});
                }
                else if (text[i] == '*')
                {
                    letters.Add(new Letter() { text = "*", name = "*", type = LetterType.Mul});
                }
                else if (text[i] == '/')
                {
                    letters.Add(new Letter() { text = "/", name = "/", type = LetterType.Div});
                }
                else if (text[i] == '|')
                {
                    letters.Add(new Letter() { text = "|", name = "|", type = LetterType.Bou});
                }
                else if (text[i] == '~')
                {
                    if (i + 1 < text.Length && text[i + 1] == '~')
                    {
                        i++;
                        letters.Add(new CommentLet() { text = "~~", name = "~~", type = LetterType.NyoroNyoro });
                    }
                    else letters.Add(new CommentLet() { text = "~", name = "~", type = LetterType.Nyoro});
                }
                else if (text[i] == '.')
                {
                    letters.Add(new Letter() { text = ".", name = ".", type = LetterType.Dot});
                }
                else if (text[i] == '(')
                {
                    letters.Add(new Letter() { text = "(", name = "(", type = LetterType.BracketS});
                }
                else if (text[i] == ')')
                {
                    letters.Add(new Letter() { text = ")", name = ")", type = LetterType.BracketE});
                }
                else if (text[i] == '[')
                {
                    letters.Add(new Letter() { text = "[", name = "[", type = LetterType.BlockS});
                }
                else if (text[i] == ']')
                {
                    letters.Add(new Letter() { text = "]", name = "]", type = LetterType.BlockE});
                }
                else if (text[i] == '{')
                {
                    letters.Add(new Letter() { text = "{", name = "{", type = LetterType.BraceS});
                }
                else if (text[i] == '}')
                {
                    letters.Add(new Letter() { text = "}", name = "}", type = LetterType.BraceE});
                }
                else if (text[i] =='\0')
                {
                    letters.Add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                }
            }
            return letters;
        }
        Obj Start(Local local)
        {
            local.state = new State();
            local.state.elements.Add(local);
            local.state.plus(0);
            var item = new CallBlock();
            item.children.Add(new Block(ObjType.Call1));
            item.children.Add(Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, LetterType.End, ObjType.Call2, 0).tobj);
            return item;
        }
        Obj Block(Local local, LetterType end, int comments)
        {
            var item = new CallBlock();
            if (end == LetterType.MoreThan) item = new TagBlock();
            /*if (local.state.lettersearch([end], [LetterType.Bou]))
            {
                item.children.Add(new Block(ObjType.Call1));
                if (local.state.letter.type == LetterType.Kaigyou) local.state.plus(1);
                item.children.Add(Block2(local, end, comments));
                return item;
            }*/
        head:
            if (local.state.letter.type == LetterType.Kaigyou) local.state.plus(1);
            var item2 = Lines(local, LetterType.Kaigyou, LetterType.Comma, LetterType.Semicolon, LetterType.Bou, ObjType.Call1, comments).tobj;
            item.children.Add(item2);
            item.children.Add(Block2(local, end, comments));
            return item;
        }
        Obj Block2(Local local, LetterType finish, int comments)
        {
            var item = Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, finish, ObjType.Call2, comments);
            return item.tobj;
        }
        (Letter letter, Obj tobj) Lines(Local local, LetterType sub, LetterType sub2, LetterType sub3, LetterType finish, ObjType type, int comments)
        {
            var item = new Block(type);
            bool tag = false;
            if (finish == LetterType.MoreThan) tag = true;
        head:
            if (local.state.letter.type == LetterType.Sharp)
            {
                local.state.plus(1);
                if (local.state.letter.type == LetterType.Sharp)
                {
                    local.state.plus(1);
                    if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                    {
                        var name = local.state.letter.name;
                        var let = local.state.letter;
                        local.state.plus(1);
                        Label label0 = new Label() {letter = let, name = name };
                        if (local.labelmap.ContainsKey(name))
                        {
                            label0 = local.labelmap[name];
                        }
                        else
                        {
                            local.labelmap[name] = label0;
                        }
                        var label = new Label() {letter = let, name = name, n = item.children.Count };
                        if (item.branchmap.ContainsKey(name)) label = item.branchmap[name];
                        else item.branchmap[name] = label;
                        if (local.state.letter.type == LetterType.Dot)
                        {
                            local.state.plus(1);
                            if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                            {
                                var name2 = local.state.letter.name;
                                Label label00 = new Label() {letter = local.state.letter, name = name2 };
                                if (label0.labelmap.ContainsKey(name2))
                                {
                                    label00 = label0.labelmap[name2];
                                }
                                else
                                {
                                    label0.labelmap[name2] = label00;
                                }
                                var label2 = new Label() {letter = local.state.letter, name = name2, n = item.children.Count };
                                if (label.labelmap.ContainsKey(name2)) label2 = label.labelmap[name2];
                                else label.labelmap[name2] = label2;
                                local.state.plus(1);
                                if (local.state.letter.type == LetterType.Dot)
                                {
                                    local.state.plus(1);
                                    if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                                    {
                                        Label label000 = new Label() {letter = local.state.letter, name = local.state.letter.name };
                                        if (label00.labelmap.ContainsKey(name))
                                        {
                                            label000 = label00.labelmap[name];
                                        }
                                        else
                                        {
                                            label00.labelmap[name] = label00;
                                        }
                                        var name3 = local.state.letter.name;
                                        var label3 = new Label() {letter = local.state.letter, name = name3, n = item.children.Count };
                                        if (label2.labelmap.ContainsKey(name3)) label3 = label.labelmap[name3];
                                        else label2.labelmap[name3] = label3;
                                        local.state.plus(1);
                                    }
                                    else throw new Exception();
                                }
                            }
                            else throw new Exception();
                        }
                    }
                }
                else if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                {
                    var name = local.state.letter.name;
                    var let = local.state.letter;
                    local.state.plus(1);
                    if (local.state.letter.type == LetterType.Dot)
                    {
                        local.state.plus(1);
                        if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                        {
                            var label = new Label() {letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                            if (item.labelmap.ContainsKey(name)) label = item.labelmap[name];
                            else item.labelmap[name] = label;
                            if (label.labelmap.ContainsKey(local.state.letter.name)) throw new Exception();
                            else label.labelmap[local.state.letter.name] = new Label() {letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                            local.state.plus(1);
                        }
                        else throw new Exception();
                    }
                    else
                    {
                        if (item.labelmap.ContainsKey(name)) throw new Exception();
                        item.labelmap[name] = new Label() { letter = let, name = name, n = item.children.Count };
                    }
                }
                else if (local.state.letter.type == LetterType.Str)
                {
                    item.labelmap[local.state.letter.name] = new Label() { letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Number)
                {
                    item.labelmap[Convert.ToInt32(local.state.letter.name).ToString()] = new Label() {letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                    local.state.plus(1);
                }
                else throw new Exception();
            }
            if (local.state.letter.type == sub)
            {
                local.state.plus(1);
                goto head;
            }
            else if (local.state.letter.type == sub2)
            {
                local.state.plus(1);
                goto head;
            }
            else if (local.state.letter.type == sub3)
            {
                local.state.plus(1);
                goto head;
            }
            else if (local.state.letter.type == finish)
            {
                var letter = local.state.letter;
                item.letter2 = letter;
                local.state.plus(1);
                return (letter, item);
            }
            item.children.Add(Operator(local, 0, comments, tag, type));
            if (local.state.letter.type == sub)
            {
                local.state.plus(1);
                goto head;
            }
            else if (local.state.letter.type == sub2)
            {
                local.state.plus(1);
                goto head;
            }
            else if (local.state.letter.type == sub3)
            {
                local.state.plus(1);
                goto head;
            }
            else if (local.state.letter.type == finish)
            {
                var letter = local.state.letter;
                item.letter2 = letter;
                local.state.plus(1);
                return (letter, item);
            }
            throw new Exception();
        }
        Obj Ope1(Local local, int n, int comments, bool tag, ObjType type)
        {
            if (n < local.operators.Count) return Operator(local, n, comments, tag, type);
            else return Primary(local, comments, type);
        }
        Obj Operator(Local local, int n, int comments, bool tag, ObjType type)
        {
            var item = Ope1(local, n + 1, comments, tag, type);
            foreach (var op in local.operators[n].types)
            {
                if (local.state.letter.type == op)
                {
                    if (local.state.letter.type == LetterType.MoreThan && tag)
                    {
                        return item;
                    }
                    var item2 = new Operator(local.state.letter);
                    local.state.plus(1);
                    item2.children.Add(item);
                    item2.children.Add(Ope1(local, n + 1, comments, tag, type));
                    return item2;
                }
            }
            return item;
        }
        Obj Primary(Local local, int comments, ObjType type)
        {
            var item = new Primary();
            if (comments > 0)
            {
                if (local.state.letter.type == LetterType.Dot || local.state.letter.type == LetterType.Mul || local.state.letter.type == LetterType.Gpt)
                {
                    item.singleop = new SingleOp(local.state.letter);
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Plus || local.state.letter.type == LetterType.Minus || local.state.letter.type == LetterType.Not)
                {
                    item.singleop = new SingleOp(local.state.letter);
                    local.state.plus(1);
                }
            }
            else if (local.state.letter.type == LetterType.Plus || local.state.letter.type == LetterType.Minus || local.state.letter.type == LetterType.Not)
            {
                item.singleop = new SingleOp(local.state.letter);
                local.state.plus(1);
            }
            var first = true;
            for (; ; )
            {
                if (comments > 0 && type != ObjType.Call1 && local.state.letter.type == LetterType.LessThan)
                {
                    var letter = local.state.letter;
                    local.state.plus(1);
                    var tagblock = Block(local, LetterType.MoreThan, comments);
                    tagblock.letter = letter;
                    item.children.Add(tagblock);
                }
                else if (comments > 0 && local.state.letter.type == LetterType.Dolor)
                {
                    item.children.Add(new Dolor(local.state.letter));
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Nyoro)
                {
                    var item2 = new Comment() { letter = local.state.letter };
                    item.children.Add(item2);
                    local.state.plus(1);
                    var ret = Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, LetterType.NyoroNyoro, ObjType.Comment, comments + 1);
                    item2.children.Add(ret.tobj);
                }
                else if (comments > 0 && local.state.letter.type == LetterType.HLetter)
                {
                    item.children.Add(new HtmObj(local.state.letter));
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Letter)
                {
                    item.children.Add(new Word(local.state.letter).Change(local));
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Number)
                {
                    item.children.Add(new Number(local.state.letter) { cls = local.Int });
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Decimal)
                {
                    item.children.Add(new FloatVal(local.state.letter) { cls = local.Float });
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Str)
                {
                    item.children.Add(new StrObj(local.state.letter) { cls = local.Str });
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.BracketS)
                {
                    var letter = local.state.letter;
                    local.state.plus(1);
                    var block = Lines(local, LetterType.Semicolon, LetterType.Comma, LetterType.Kaigyou, LetterType.BracketE, ObjType.Bracket, comments).tobj;
                    block.letter = letter;
                    item.children.Add(block);
                }
                else if (local.state.letter.type == LetterType.BlockS)
                {
                    var letter = local.state.letter;
                    local.state.plus(1);
                    var block = Lines(local, LetterType.Semicolon, LetterType.Comma, LetterType.Kaigyou, LetterType.BlockE, ObjType.Block, comments).tobj;
                    block.letter = letter;
                    item.children.Add(block);
                }
                else if (local.state.letter.type == LetterType.BraceS)
                {
                    var letter = local.state.letter;
                    local.state.plus(1);
                    var callblock = Block(local, LetterType.BraceE, comments);
                    callblock.letter = letter;
                    item.children.Add(callblock);
                }
                else if (!first)
                {
                    if (local.state.letter.type == LetterType.Dot)
                    {
                        item.children.Add(new PrimOp(local.state.letter, ObjType.Dot));
                        local.state.plus(1);
                        first = true;
                        continue;
                    }
                    else if (local.state.letter.type == LetterType.Left)
                    {
                        item.children.Add(new PrimOp(local.state.letter, ObjType.Left));
                        local.state.plus(1);
                        first = true;
                        continue;
                    }
                    else if (local.state.letter.type == LetterType.Right)
                    {
                        item.children.Add(new PrimOp(local.state.letter, ObjType.Right));
                        local.state.plus(1);
                        first = true;
                        continue;
                    }
                    else
                    {
                        item.children.Add(new Obj(ObjType.None));
                        return item;
                    }
                }
                else
                {
                    item.children.Add(new Obj(ObjType.None));
                    return item;
                }
                first = false;
            }
        }
    }
}
