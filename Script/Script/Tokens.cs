using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public enum Tokens
    {
        Block,
        Comment,
        Regex,
        String,
        Double,
        Integer,
        Boolean,
        Null,
        If,
        ElseIf,
        Else,
        For,
        Arithmetic,
        ListType,
        BaseType,
        Keyword,
        Symbol,
        ArrayLeft,
        ArrayRight,
        Dot,
        Comma,
        Left,
        Right,
        Assignment,
        Operator,
        Tab,
        Space,
        Arguments,
        Parameters,
        EventName,
        EventNameParameters,
        Event,
        Function,
        Code
    }
}