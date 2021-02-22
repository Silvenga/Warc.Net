using Pidgin;
using static Pidgin.Parser;

namespace Warc.Net.Parsing.Grammars
{
    public class WarcCommonGrammar
    {
        // *token         = 1*<any US-ASCII character except CTLs or separators>
        // *separators    = "(" | ")" | "<" | ">" | "@" | "," | ";" | ":" | "\" | <"> | "/" | "[" | "]" | "?" | "=" | "{" | "}" | SP | HT
        // TEXT          = <any OCTET except CTLs, but including LWS>
        // CHAR          = <UTF-8 characters; RFC3629>  ; (0-191, 194-244)
        // DIGIT         = <any US-ASCII digit "0".."9">
        // *CTL           = <any US-ASCII control character (octets 0 - 31) and DEL (127)>
        // *CR            = <ASCII CR, carriage return>  ; (13)
        // *LF            = <ASCII LF, linefeed>         ; (10)
        // *SP            = <ASCII SP, space>            ; (32)
        // *HT            = <ASCII HT, horizontal-tab>   ; (9)
        // *CRLF          = CR LF
        // *LWS           = [CRLF] 1*( SP | HT )         ; semantics same as single SP
        // quoted-string = ( <"> *(qdtext | quoted-pair ) <"> )
        // qdtext        = <any TEXT except <">>
        // quoted-pair   = "\" CHAR ; single-character quoting
        // uri           = <'URI' per RFC3986>

        public static readonly Parser<char, char> Sp = Char((char) 32);
        public static readonly Parser<char, char> Ht = Char((char) 9);
        public static readonly Parser<char, string> Crlf = String("\r\n").Labelled("CRLF");
        public static readonly Parser<char, char> Lws = Crlf.Then(Sp.Or(Ht)).ThenReturn(' ').Labelled(nameof(Lws));
        public static readonly Parser<char, char> Ctl = Parser<char>.Token(x => (x >= 0 && x <= 31) || x == 127);
        public static readonly Parser<char, char> Separator = Parser<char>.Token(IsSeparator);
        public static readonly Parser<char, char> Token = Parser<char>.Token(x => x > 31 && x != 127 && !IsSeparator(x));
        public static readonly Parser<char, char> Text = Parser<char>.Token(x => x > 31 && x != 127 && x != '\r').Or(Try(Lws));

        public static readonly Parser<char, char> Ascii = Parser<char>.Token(x => x < 128);
        public static Parser<char, char> AsciiExcept(char c) => Parser<char>.Token(x => x < 128 && x != c);

        private static bool IsSeparator(char x)
        {
            switch (x)
            {
                case '(':
                case ')':
                case '<':
                case '>':
                case '@':
                case ',':
                case ';':
                case ':':
                case '\\':
                case '"':
                case '/':
                case '[':
                case ']':
                case '?':
                case '=':
                case '{':
                case '}':
                case (char) 32:
                case (char) 9:
                    return true;
                default:
                    return false;
            }
        }
    }
}