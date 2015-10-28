using System;
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

    public sealed class TokenDefinitionGroup
    {
        public TokenDefinitionGroup (string name, TokenDefinition[] tokenDefinitions)
        {
            this.Name = name;
            this.TokenDefinitions = tokenDefinitions;
        }
        public readonly string Name;
        public readonly TokenDefinition[] TokenDefinitions;
    }

    public sealed class TokenDefinition
    {
        public IMatcher Matcher;
        public readonly Tokens Token;

        public TokenDefinition(string regex, Tokens token)
        {
            this.Matcher = new RegexMatcher(regex);
            this.Token = token;
        }
    }

    public sealed class History
    {
        public History(int lineNumber, int position, string lineRemaining)
        {
            LineNumber = lineNumber;
            Position = position;
            LineRemaining = lineRemaining;
        }
        public int LineNumber { get; set; }
        public int Position { get; set; }
        public string LineRemaining { get; set; }
    }

    public class Lexer : IDisposable
    {
        private readonly TextReader reader;
        private readonly List<TokenDefinitionGroup> tokenDefinitionGroups;
        private string groupName;

        private string lineRemaining;
        private Action<int, int, string> error;

        public Lexer(TextReader reader, List<TokenDefinitionGroup> tokenDefinitionGroups, Action<int, int, string> error)
        {
            this.reader = reader;
            this.tokenDefinitionGroups = tokenDefinitionGroups;
            this.error = error;
            this.groupName = tokenDefinitionGroups.First().Name;
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

        public void Step(string groupName)
        {
            this.groupName = groupName;
        }

        private Stack<History> Past = new Stack<History>();

        public bool Next()
        {
            return Next(false);
        }
        public bool Next(bool supressError)
        {
            Past.Push(new History(LineNumber, Position, lineRemaining));

            if (lineRemaining == null)
            {
                return false;
            }
            else if (lineRemaining.Length == 0) // Return true while still empty somehow.
            {
                nextLine();
            }

            if (Position == -1)
            {
                Token = Tokens.Block;
                Position = 0;
                return true;
            }

            foreach (var def in tokenDefinitionGroups.First(x => x.Name == this.groupName).TokenDefinitions)
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
                        //nextLine();
                    }
                    return true;
                }
            }
            if (!supressError)
            {
                error.DynamicInvoke(LineNumber, Position, lineRemaining);
            }
            return false;
        }

        public bool Prev()
        {
            var history = Past.Pop();
            Position = history.Position;
            LineNumber = history.LineNumber;
            lineRemaining = history.LineRemaining;
            return true;
        }

        public string TokenContents { get; private set; }

        public Tokens Token { get; private set; }

        public int LineNumber { get; private set; }

        public int Position { get; private set; }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
