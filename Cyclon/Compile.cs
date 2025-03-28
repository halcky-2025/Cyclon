﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclon
{
    partial class Form1 : Form
    {
        public static List<Element> Compile(string text, Local local)
        {
            var letters = new List<Element>();
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
                    Letter l = new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Letter }.Let();
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
                    for (; ; j++)
                    {
                        if (j >= text.Length)
                        {

                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.Str, brush = Brushes.Brown });
                            break;
                        }
                        else if (text[j] == '"')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i + 1), name = text.Substring(i + 1, j - i - 1), type = LetterType.Str, brush = Brushes.Brown });
                            j++;
                            break;
                        }
                        else if (text[j] == '\n' || text[j] == '\0')
                        {

                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.Str, brush = Brushes.Brown });
                            break;
                        }
                    }
                    i = j - 1;
                }
                else if (text[i] == '&')
                {

                    var j = i + 1;
                    for (; ; j++)
                    {
                        if (j >= text.Length)
                        {

                            letters.Add(new AndLet() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.And, brush = Brushes.Brown });
                            break;
                        }
                        else if (text[j] == '\n' || text[j] == '\0')
                        {

                            letters.Add(new AndLet() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.And, brush = Brushes.Brown });
                            break;
                        }
                    }
                    i = j - 1;
                }
                else if (text[i] == '^')
                {
                    letters.Add(new Letter() { text = "^", name = "^", type = LetterType.Mountain });
                }
                else if (text[i] == '`')
                {
                    var j = i + 1;
                    for (; ; j++)
                    {
                        if (j >= text.Length)
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter });
                            break;
                        }
                        else if (text[j] == '`')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i + 1), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter });
                            j++;
                            break;
                        }
                        else if (text[j] == '<' || text[j] == '>' || text[j] == '~' || text[j] == '$')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter });
                            break;
                        }
                        else if (text[j] == '\n' || text[j] == '\0')
                        {
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i + 1, j - i - 1), type = LetterType.HLetter });
                            break;
                        }
                    }
                    i = j - 1;
                }
                else if (text[i] == ' ')
                {
                    var j = i + 1;
                    for (; j < text.Length; j++)
                    {
                        if (text[j] == ' ') continue;
                        else break;
                    }
                    letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Space });
                    i = j - 1;
                }
                else if (text[i] == ':')
                {
                    letters.Add(new Letter() { text = ":", name = ":", type = LetterType.Colon });
                }
                else if (text[i] == ';')
                {
                    letters.Add(new Letter() { text = ";", name = ";", type = LetterType.Semicolon });
                }
                else if (text[i] == '$')
                {
                    letters.Add(new Letter() { text = "$", name = "$", type = LetterType.Dolor });
                }
                else if (text[i] == ',')
                {
                    letters.Add(new Letter() { text = ",", name = ",", type = LetterType.Comma });
                }
                else if (text[i] == '|')
                {
                    letters.Add(new Letter() { text = "|", name = "|", type = LetterType.Bou });
                }
                else if (text[i] == '#')
                {
                    letters.Add(new Letter() { text = "#", name = "#", type = LetterType.Sharp });
                }
                else if (text[i] == '%')
                {
                    letters.Add(new Letter() { text = "%", name = "%", type = LetterType.Percent });
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
                        letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.AtLetter });
                        i = j - 1;
                    }
                }
                else if (text[i] == '\n')
                {
                    var j = i + 1;
                    var kaigyou = new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou };
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
                            letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.Space });
                            j--;
                        }
                        else if (text[j] == '\n')
                        {
                            letters.Add(new Kaigyou() { text = "\n", name = "\n", type = LetterType.Kaigyou });
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
                        letters.Add(new Letter() { text = "==", name = "==", type = LetterType.EqualEqual });
                    }
                    else letters.Add(new Letter() { text = "=", name = "=", type = LetterType.Equal });
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
                    letters.Add(new Letter() { text = "+", name = text[i].ToString(), type = LetterType.Plus });
                }
                else if (text[i] == '-')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '>')
                        {
                            letters.Add(new Letter() { text = "->", name = "->", type = LetterType.Right });
                            i++;
                        }
                        else if (text[i + 1] == '-')
                        {
                            i++;
                            /*if (i + 1 > text.Length && text[i + 1] == '-')
                            {
                                i++;
                                letters.Add(new Letter() { text = "---", name = "---", type = LetterType.CommentMany });
                            }
                            else*/
                            {
                                var j = i + 1;
                                for (; ; j++)
                                {
                                    if (j >= text.Length)
                                    {

                                        letters.Add(new Letter() { text = text.Substring(i - 1, j - i + 1), name = text.Substring(i + 1, j - i - 1), type = LetterType.CommentSingle, brush = Brushes.Green });
                                        break;
                                    }
                                    else if (text[j] == '\n' || text[j] == '\0')
                                    {

                                        letters.Add(new Letter() { text = text.Substring(i - 1, j - i + 1), name = text.Substring(i + 1, j - i - 1), type = LetterType.CommentSingle, brush = Brushes.Green });
                                        break;
                                    }
                                }
                                i = j - 1;
                            }
                        }
                        else letters.Add(new Letter() { text = "-", name = "-", type = LetterType.Minus });
                    }
                    else letters.Add(new Letter() { text = "-", name = "-", type = LetterType.Minus });
                }
                else if (text[i] == '<')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '-')
                        {
                            letters.Add(new Letter() { text = "<-", name = "<-", type = LetterType.Left });
                            i++;
                        }
                        else if (text[i + 1] == '=')
                        {
                            letters.Add(new Letter() { text = "<=", name = "<=", type = LetterType.LessEqual });
                            i++;
                        }
                        else if (text[i + 1] == '&')
                        {
                            letters.Add(new Letter() { text = "<&", name = "<&", type = LetterType.StringTag });
                            i++;
                        }
                        else letters.Add(new Letter() { text = "<", name = "<", type = LetterType.LessThan });
                    }
                    else letters.Add(new Letter() { text = "<", name = "<", type = LetterType.LessThan });
                }
                else if (text[i] == '>')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '=')
                        {
                            letters.Add(new Letter() { text = ">=", name = ">=", type = LetterType.MoreEqual });
                            i++;
                        }
                        else if (text[i + 1] == '>')
                        {
                            letters.Add(new Letter() { text = ">>", name = ">>", type = LetterType.RightRight });
                            i++;
                        }
                        else letters.Add(new Letter() { text = ">", name = ">", type = LetterType.MoreThan });
                    }
                    else letters.Add(new Letter() { text = ">", name = ">", type = LetterType.MoreThan });
                }
                else if (text[i] == '*')
                {
                    letters.Add(new Letter() { text = "*", name = "*", type = LetterType.Mul });
                }
                else if (text[i] == '/')
                {
                    letters.Add(new Letter() { text = "/", name = "/", type = LetterType.Div });
                }
                else if (text[i] == '|')
                {
                    letters.Add(new Letter() { text = "|", name = "|", type = LetterType.Bou });
                }
                else if (text[i] == '~')
                {
                    if (i + 1 < text.Length && text[i + 1] == '~')
                    {
                        i++;
                        if (i + 1 < text.Length && text[i + 1] == '~')
                        {
                            i++;
                            letters.Add(new CommentLet() { text = "~~~", name = "~~~", type = LetterType.NyoroNyoroNyoro });
                        }
                        else letters.Add(new CommentLet() { text = "~~", name = "~~", type = LetterType.NyoroNyoro });
                    }
                    else letters.Add(new CommentLet() { text = "~", name = "~", type = LetterType.Nyoro });
                }
                else if (text[i] == '.')
                {
                    letters.Add(new Letter() { text = ".", name = ".", type = LetterType.Dot });
                }
                else if (text[i] == '(')
                {
                    letters.Add(new Letter() { text = "(", name = "(", type = LetterType.BracketS });
                }
                else if (text[i] == ')')
                {
                    letters.Add(new Letter() { text = ")", name = ")", type = LetterType.BracketE });
                }
                else if (text[i] == '[')
                {
                    letters.Add(new Letter() { text = "[", name = "[", type = LetterType.BlockS });
                }
                else if (text[i] == ']')
                {
                    letters.Add(new Letter() { text = "]", name = "]", type = LetterType.BlockE });
                }
                else if (text[i] == '{')
                {
                    letters.Add(new Letter() { text = "{", name = "{", type = LetterType.BraceS });
                }
                else if (text[i] == '}')
                {
                    letters.Add(new Letter() { text = "}", name = "}", type = LetterType.BraceE });
                }
                else if (text[i] == '\0')
                {
                    letters.Add(new Kaigyou() { text = "\0", name = "\0", type = LetterType.End });
                    break;
                }
                else if (text[i] == '?')
                {
                    letters.Add(new Letter() { text = "?", name = "?", type = LetterType.Question });
                }
                else if (text[i] >= '\uE000')
                {
                    letters.Add(new SelectLetter(text[i] - '\uE000'));
                }
                else if (text[i] >= 256)
                {
                    var j = i + 1;
                    for (; j < text.Length && text[j] >= 256; j++)
                    {
                    }
                    letters.Add(new Letter() { text = text.Substring(i, j - i), name = text.Substring(i, j - i), type = LetterType.CommentSingle });
                    i = j - 1;
                }
            }
            var state2 = new State2() { elements = letters };
            local.state = state2;
            local.state.plus(1);
            local.blockslist.Add(new List<Block>());
            var block = new Block(ObjType.Comment);
            block.vmap["div"] = new ElemType(ObjType.Div);
            block.vmap["br"] = new ElemType(ObjType.Br);
            block.vmap["sheet"] = new ElemType(ObjType.Sheet);
            block.vmap["cell"] = new ElemType(ObjType.Cell);
            block.vmap["span"] = new ElemType(ObjType.Span);
            block.vmap["linear"] = new LinearFunction();
            local.blocks.Add(block);
            var commentlet = new CommentLet();
            commentlet.instanceslist.Add(new List<Element>());
            local.comments.Add(commentlet);
            var error = false;
            local.panel.form.TagString(local, ObjType.Call1, ref error);
            local.blockslist = new List<List<Block>>();
            local.comments = new List<CommentLet>();
            return letters;
        }
        class State2 : State
        {
            public int plu = 0;
            public List<Element> tags = new List<Element>();
            public override void plus(int n)
            {
                plu = 0;
                for (; this.n < elements.Count;)
                {
                    element = elements[this.n];
                    if (element.type == LetterType.End)
                    {
                        return;
                    }
                    else if (element.type == LetterType.StringTag)
                    {
                        tags.Add(element);
                        elements.RemoveAt(this.n);
                        return;
                    }
                    else if (tags.Count > 0)
                    {
                        elements.RemoveAt(this.n);
                        if (element.type == LetterType.Space) continue;
                        else if (element.type == LetterType.Select)
                        {
                            elements[this.n].Before(element);
                            if (elements[this.n].type == LetterType.Select)
                            {
                                elements[this.n + 1].Before(element);
                                elements[this.n + 1].Before(elements[this.n]);
                                elements.RemoveAt(this.n);
                            }
                            else elements[this.n].Before(element);
                            continue;
                        }
                        else if (element.type == LetterType.MoreThan)
                        {
                            tags.RemoveAt(tags.Count - 1);
                        }
                        return;
                    }
                    plu++;
                    this.n++;
                }
            }

        }
        public Obj Start(Local local, ref bool error)
        {
            local.analblocks = new List<Block>();
            local.state = new State();
            local.state.elements.Add(local);
            local.state.plus(0);
            var item = new CallBlock();
            item.children.Add(new Block(ObjType.Call1));
            var item2 = Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, LetterType.End, ObjType.Call2, 0, ref error).tobj;
            item.children.Add(item2);
            local.analblocks.Add(item2);
            return item;
        }
        Obj Block(Local local, LetterType end, int comments, ref bool error)
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
            var item2 = Lines(local, LetterType.Kaigyou, LetterType.Comma, LetterType.Semicolon, LetterType.Bou, ObjType.Call1, comments, ref error).tobj;
            local.analblocks.Add(item2);
            item.children.Add(item2);
            var item3 = Block2(local, end, comments, ref error);
            item.children.Add(item3);
            local.analblocks.Add(item3);
            return item;
        }
        Block Block2(Local local, LetterType finish, int comments, ref bool error)
        {
            var item = Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, finish, ObjType.Call2, comments, ref error);
            return item.tobj;
        }
        public (Letter letter, Block tobj) Lines(Local local, LetterType sub, LetterType sub2, LetterType sub3, LetterType finish, ObjType type, int comments, ref bool error)
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
                        local.state.letter.error = null;
                        local.state.plus(1);
                        Label label0 = new Label() { letter = let, name = name };
                        if (local.labelmap.ContainsKey(name))
                        {
                            label0 = local.labelmap[name];
                        }
                        else
                        {
                            local.labelmap[name] = label0;
                        }
                        var label = new Label() { letter = let, name = name, n = item.children.Count };
                        if (item.branchmap.ContainsKey(name)) label = item.branchmap[name];
                        else item.branchmap[name] = label;
                        if (local.state.letter.type == LetterType.Dot)
                        {
                            local.state.letter.error = null;
                            local.state.plus(1);
                            if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                            {
                                var name2 = local.state.letter.name;
                                Label label00 = new Label() { letter = local.state.letter, name = name2 };
                                local.state.letter.error = null;
                                if (label0.labelmap.ContainsKey(name2))
                                {
                                    label00 = label0.labelmap[name2];
                                }
                                else
                                {
                                    label0.labelmap[name2] = label00;
                                }
                                var label2 = new Label() { letter = local.state.letter, name = name2, n = item.children.Count };
                                if (label.labelmap.ContainsKey(name2)) label2 = label.labelmap[name2];
                                else label.labelmap[name2] = label2;
                                local.state.letter.error = null;
                                local.state.plus(1);
                                if (local.state.letter.type == LetterType.Dot)
                                {
                                    local.state.plus(1);
                                    if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                                    {
                                        Label label000 = new Label() { letter = local.state.letter, name = local.state.letter.name };
                                        local.state.letter.error = null;
                                        if (label00.labelmap.ContainsKey(name))
                                        {
                                            label000 = label00.labelmap[name];
                                        }
                                        else
                                        {
                                            label00.labelmap[name] = label00;
                                        }
                                        var name3 = local.state.letter.name;
                                        var label3 = new Label() { letter = local.state.letter, name = name3, n = item.children.Count };
                                        if (label2.labelmap.ContainsKey(name3)) label3 = label.labelmap[name3];
                                        else label2.labelmap[name3] = label3;
                                        local.state.letter.error = null;
                                        local.state.plus(1);
                                    }
                                    else
                                    {
                                        local.state.letter.error = "Labelの構文が間違っています";
                                        error = true;
                                    }
                                }
                            }
                            else
                            {
                                local.state.letter.error = "Labelの構文が間違っています";
                                error = true;
                            }
                        }
                    }
                }
                else if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                {
                    var name = local.state.letter.name;
                    var let = local.state.letter;
                    let.error = null;
                    local.state.plus(1);
                    if (local.state.letter.type == LetterType.Dot)
                    {
                        local.state.letter.error = null;
                        local.state.plus(1);
                        if (local.state.letter.type == LetterType.Letter || local.state.letter.type == LetterType.Number || local.state.letter.type == LetterType.Str)
                        {
                            var label = new Label() { letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                            if (item.labelmap.ContainsKey(name)) label = item.labelmap[name];
                            else item.labelmap[name] = label;
                            if (label.labelmap.ContainsKey(local.state.letter.name))
                            {
                                local.state.letter.error = "Labelの構文が間違っています";
                                error = true;
                            }
                            else label.labelmap[local.state.letter.name] = new Label() { letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                            local.state.plus(1);
                        }
                        else {
                            error = true;
                        };
                    }
                    else
                    {
                        if (item.labelmap.ContainsKey(name))
                        {
                            local.state.letter.error = "Labelの構文が間違っています";
                            error = true;
                        }
                        else
                        {
                            item.labelmap[name] = new Label() { letter = let, name = name, n = item.children.Count };
                        }
                    }
                }
                else if (local.state.letter.type == LetterType.Str)
                {
                    item.labelmap[local.state.letter.name] = new Label() { letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Number)
                {
                    item.labelmap[Convert.ToInt32(local.state.letter.name).ToString()] = new Label() { letter = local.state.letter, name = local.state.letter.name, n = item.children.Count };
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else
                {
                    local.state.letter.error = "Labelの構文が間違っています";
                    error = true;
                }
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
                letter.error = null;
                item.letter2 = letter;
                local.state.plus(1);
                return (letter, item);
            }
            item.children.Add(Operator(local, 0, comments, tag, type, ref error));
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
                letter.error = null;
                item.letter2 = letter;
                local.state.plus(1);
                return (letter, item);
            }
            else if (local.state.letter.type == LetterType.End)
            {
                return (local.state.letter, item);
            }
            else
            {
                error = true;
                local.state.letter.error = "構文解析エラーです";
                local.state.plus(1);
                goto head;
            }
        }
        Obj Ope1(Local local, int n, int comments, bool tag, ObjType type, ref bool error)
        {
            if (n < local.operators.Count) return Operator(local, n, comments, tag, type, ref error);
            else return Primary(local, comments, type, ref error);
        }
        Obj Operator(Local local, int n, int comments, bool tag, ObjType type, ref bool error)
        {
            var item = Ope1(local, n + 1, comments, tag, type,ref error);
            foreach (var op in local.operators[n].types)
            {
                if (local.state.letter.type == op)
                {
                    if (local.state.letter.type == LetterType.MoreThan && tag)
                    {
                        return item;
                    }
                    var item2 = new Operator(local.state.letter);
                    local.state.letter.error = null;
                    local.state.plus(1);
                    item2.children.Add(item);
                    item2.children.Add(Ope1(local, n + 1, comments, tag, type, ref error));
                    return item2;
                }
            }
            return item;
        }
        public Obj TagString(Local local, ObjType type, ref bool error)
        {
            var tags = new List<Obj>();
            for (; ; )
            {
                if (local.state.letter.type == LetterType.StringTag)
                {
                    var letter = local.state.letter;
                    local.state.plus(1);
                    var tagblock = Block(local, LetterType.MoreThan, 1, ref error) as TagBlock;
                    tagblock.letter = letter;
                    letter.error = null;
                    tagblock.exe(local);
                    local.state.elements.Insert(local.state.n - (local.state as State2).plu, tagblock.divobj.elem);
                    local.state.n++;
                }
                else if (local.state.letter.type == LetterType.End || local.state.letter.type == LetterType.Kaigyou)
                {
                    return null;
                }
                else
                {
                    local.state.plus(1);
                }
            }
            return null;
        }
        Obj Primary(Local local, int comments, ObjType type, ref bool error)
        {
            var item = new Primary();
            if (comments > 0)
            {
                if (local.state.letter.type == LetterType.Dot || local.state.letter.type == LetterType.Mul || local.state.letter.type == LetterType.RightRight)
                {
                    item.singleop = new SingleOp(local.state.letter);
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Plus || local.state.letter.type == LetterType.Minus || local.state.letter.type == LetterType.Not)
                {
                    item.singleop = new SingleOp(local.state.letter);
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
            }
            else if (local.state.letter.type == LetterType.Plus || local.state.letter.type == LetterType.Minus || local.state.letter.type == LetterType.Not)
            {
                item.singleop = new SingleOp(local.state.letter);
                local.state.letter.error = null;
                local.state.plus(1);
            }
            var first = true;
            for (; ; )
            {
                if (comments > 0 && type != ObjType.Call1 && (local.state.letter.type == LetterType.LessThan || local.state.letter.type == LetterType.StringTag))
                {
                    var letter = local.state.letter;
                    letter.error = null;
                    local.state.plus(1);
                    var tagblock = Block(local, LetterType.MoreThan, comments, ref error);
                    tagblock.letter = letter;
                    item.children.Add(tagblock);
                }
                else if (comments > 0 && local.state.letter.type == LetterType.Dolor)
                {
                    item.children.Add(new Dolor(local.state.letter));
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Nyoro)
                {
                    var item2 = new Comment() { letter = local.state.letter };
                    local.state.letter.error = null;
                    item.children.Add(item2);
                    local.state.plus(1);
                    var ret = Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, LetterType.NyoroNyoro, ObjType.Comment, comments + 1, ref error);
                    item2.children.Add(ret.tobj);
                }
                else if (local.state.letter.type == LetterType.NyoroNyoroNyoro)
                {
                    var item2 = new Comment2() { letter = local.state.letter };
                    local.state.letter.error = null;
                    item.children.Add(item2);
                    local.state.plus(1);
                    var ret = Lines(local, LetterType.Kaigyou, LetterType.Semicolon, LetterType.Comma, LetterType.NyoroNyoro, ObjType.Comment, comments + 1, ref error);
                    item2.children.Add(ret.tobj);
                }
                else if (comments > 0 && local.state.letter.type == LetterType.HLetter)
                {
                    item.children.Add(new HtmObj(local.state.letter));
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Mountain)
                {
                    item.children.Add(new Mountain(local.state.letter));
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Question)
                {
                    item.children.Add(new Question(local.state.letter));
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Letter)
                {
                    item.children.Add(new Word(local.state.letter).Change(local));
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Number)
                {
                    item.children.Add(new Number(local.state.letter) { cls = local.Int });
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Decimal)
                {
                    item.children.Add(new FloatVal(local.state.letter) { cls = local.Float });
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.Str)
                {
                    item.children.Add(new StrObj(local.state.letter) { cls = local.Str });
                    local.state.letter.error = null;
                    local.state.plus(1);
                }
                else if (local.state.letter.type == LetterType.BracketS)
                {
                    var letter = local.state.letter;
                    letter.error = null;
                    local.state.plus(1);
                    var block = Lines(local, LetterType.Semicolon, LetterType.Comma, LetterType.Kaigyou, LetterType.BracketE, ObjType.Bracket, comments, ref error).tobj;
                    block.letter = letter;
                    item.children.Add(block);
                }
                else if (local.state.letter.type == LetterType.BlockS)
                {
                    var letter = local.state.letter;
                    letter.error = null;
                    local.state.plus(1);
                    var block = Lines(local, LetterType.Semicolon, LetterType.Comma, LetterType.Kaigyou, LetterType.BlockE, ObjType.Block, comments, ref error).tobj;
                    block.letter = letter;
                    item.children.Add(block);
                }
                else if (local.state.letter.type == LetterType.BraceS)
                {
                    var letter = local.state.letter;
                    letter.error = null;
                    local.state.plus(1);
                    var callblock = Block(local, LetterType.BraceE, comments, ref error);
                    callblock.letter = letter;
                    item.children.Add(callblock);
                }
                else if (!first)
                {
                    if (local.state.letter.type == LetterType.Dot)
                    {
                        item.children.Add(new PrimOp(local.state.letter, ObjType.Dot));
                        local.state.letter.error = null;
                        local.state.plus(1);
                        first = true;
                        continue;
                    }
                    else if (local.state.letter.type == LetterType.Left)
                    {
                        item.children.Add(new PrimOp(local.state.letter, ObjType.Left));
                        local.state.letter.error = null;
                        local.state.plus(1);
                        first = true;
                        continue;
                    }
                    else if (local.state.letter.type == LetterType.Right)
                    {
                        item.children.Add(new PrimOp(local.state.letter, ObjType.Right));
                        local.state.letter.error = null;
                        local.state.plus(1);
                        first = true;
                        continue;
                    }
                    else
                    {
                        item.children.Add(new Obj(ObjType.None) { letter = local.state.letter });
                        return item;
                    }
                }
                else
                {
                    item.children.Add(new Obj(ObjType.None) { letter = local.state.letter});
                    return item;
                }
                first = false;
            }
        }
    }
}
