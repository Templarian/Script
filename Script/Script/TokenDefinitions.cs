using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public static class TokenDefinitions
    {
        public static TokenDefinition Comment = new TokenDefinition(@"#.*", Tokens.Comment);
        public static TokenDefinition Regex = new TokenDefinition(@"/[^/]+/[ims]*", Tokens.Regex);
        public static TokenDefinition String = new TokenDefinition(@"([""'])(?:\\\1|.)*?\1", Tokens.String);
        public static TokenDefinition Integer = new TokenDefinition(@"[-+]?\d+", Tokens.Integer);
        public static TokenDefinition Double = new TokenDefinition(@"[-+]?\d*\.\d+", Tokens.Double);
        public static TokenDefinition Boolean = new TokenDefinition(@"(true|false)", Tokens.Boolean);
        public static TokenDefinition Null = new TokenDefinition(@"null", Tokens.Null);
        public static TokenDefinition If = new TokenDefinition(@"if\s?\(", Tokens.If);
        public static TokenDefinition ElseIf = new TokenDefinition(@"else if\s?\(", Tokens.ElseIf);
        public static TokenDefinition Else = new TokenDefinition(@"else", Tokens.Else);
        public static TokenDefinition For = new TokenDefinition(@"for\s?\(", Tokens.For);
        public static TokenDefinition Arithmetic = new TokenDefinition(@"(\+|-|\*|\/)", Tokens.Arithmetic);
        public static TokenDefinition ListType = new TokenDefinition(@"(var|int|double|string|bool)\[\]", Tokens.ListType);
        public static TokenDefinition BaseType = new TokenDefinition(@"(var|int|double|string|bool)", Tokens.BaseType);
        public static TokenDefinition Keyword = new TokenDefinition(@"(name|extends|event|function|string|integer|double|boolean|array|return)", Tokens.Keyword);
        public static TokenDefinition Symbol = new TokenDefinition(@"[*<>\?\-+/A-Za-z->!][*<>\?\-+/A-Za-z0-9->!]*", Tokens.Symbol);
        public static TokenDefinition ArrayLeft = new TokenDefinition(@"\[", Tokens.ArrayLeft);
        public static TokenDefinition ArrayRight = new TokenDefinition(@"\]", Tokens.ArrayRight);
        public static TokenDefinition Dot = new TokenDefinition(@"\.", Tokens.Dot);
        public static TokenDefinition Comma = new TokenDefinition(@",", Tokens.Comma);
        public static TokenDefinition Left = new TokenDefinition(@"\(", Tokens.Left);
        public static TokenDefinition Right = new TokenDefinition(@"\)", Tokens.Right);
        public static TokenDefinition Assignment = new TokenDefinition(@"(=|\+=|-=|\*=|\\=)", Tokens.Assignment);
        public static TokenDefinition Operator = new TokenDefinition(@" (and|or) ", Tokens.Operator);
        public static TokenDefinition Tab = new TokenDefinition(@"[ ]{4}", Tokens.Tab);
        public static TokenDefinition Space = new TokenDefinition(@"[ ]+", Tokens.Space);
        public static TokenDefinition Event = new TokenDefinition(@"event[ ]+[*<>\?\-+/A-Za-z->!][*<>\?\-+/A-Za-z0-9->!]*", Tokens.Event);
    }
}
