﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Script
{
    public interface IMatcher
    {
        /// <summary>
        /// Return the number of characters that this "regex" or equivalent
        /// matches.
        /// </summary>
        /// <param name="text">The text to be matched</param>
        /// <returns>The number of characters that matched</returns>
        int Match(string text);
    }

    internal sealed class RegexMatcher : IMatcher
    {
        private readonly Regex regex;
        public RegexMatcher(string regex)
        {
            this.regex = new Regex(string.Format("^{0}", regex));
        }

        public int Match(string text)
        {
            var m = regex.Match(text);
            return m.Success ? m.Length : 0;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }

    public sealed class TokenDefinition
    {
        public readonly IMatcher Matcher;
        public readonly string Token;

        public TokenDefinition(string regex, string token)
        {
            this.Matcher = new RegexMatcher(regex);
            this.Token = token;
        }
    }

    internal sealed class Lexer : IDisposable
    {
        private readonly TextReader reader;
        private readonly TokenDefinition[] tokenDefinitions;

        private string lineRemaining;
        private string prevLineRemaining;
        private Action<int, int, string> error;

        public Lexer(TextReader reader, TokenDefinition[] tokenDefinitions, Action<int, int, string> error)
        {
            this.reader = reader;
            this.tokenDefinitions = tokenDefinitions;
            this.error = error;
            nextLine();
        }

        private void nextLine()
        {
            do
            {
                lineRemaining = reader.ReadLine();
                ++LineNumber;
                Position = -1;
            } while (lineRemaining != null && lineRemaining.Length == 0);
        }

        public void SkipBlock()
        {
            nextLine();
        }

        private Stack<KeyValuePair<int, string>> History = new Stack<KeyValuePair<int, string>>();
        public bool Next()
        {
            History.Push(new KeyValuePair<int,string>(Position, lineRemaining));

            if (lineRemaining == null)
                return false;

            if (Position == -1)
            {
                Token = "BLOCK";
                Position = 0;
                return true;
            }

            foreach (var def in tokenDefinitions)
            {
                var matched = def.Matcher.Match(lineRemaining);
                if (matched > 0)
                {
                    Position += matched;
                    Token = def.Token;
                    TokenContents = lineRemaining.Substring(0, matched);
                    lineRemaining = lineRemaining.Substring(matched);
                    if (lineRemaining.Length == 0) // Return true while still empty somehow.
                    {
                        nextLine();
                    }
                    return true;
                }
            }
            error.DynamicInvoke(LineNumber, Position, lineRemaining);
            return false;
        }
        
        public bool Prev()
        {
            var history = History.Pop();
            Position = history.Key;
            lineRemaining = history.Value;
            return true;
        }

        public string TokenContents { get; private set; }

        public string Token { get; private set; }

        public int LineNumber { get; private set; }

        public int Position { get; private set; }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
