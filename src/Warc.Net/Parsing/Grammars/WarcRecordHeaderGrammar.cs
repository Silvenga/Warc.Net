using System;
using System.Collections.Generic;
using System.Globalization;
using Pidgin;
using Warc.Net.Models;
using static Pidgin.Parser;
using static Warc.Net.Parsing.Grammars.WarcCommonGrammar;

namespace Warc.Net.Parsing.Grammars
{
    public static class WarcRecordHeaderGrammar
    {
        // header       = version warc-fields
        // version      = "WARC/1.1" CRLF
        // warc-fields  = *named-field CRLF
        // 
        // named-field   = field-name ":" [ field-value ]
        // field-name    = token
        // field-value   = *( field-content | LWS )     ; further qualified by field definitions
        // field-content = <the OCTETs making up the field-value and consisting of either *TEXT or combinations of token, separators, and quoted-string>
        // OCTET         = <any 8-bit sequence of data>
        // token         = 1*<any US-ASCII character except CTLs or separators>
        // separators    = "(" | ")" | "<" | ">" | "@" | "," | ";" | ":" | "\" | <"> | "/" | "[" | "]" | "?" | "=" | "{" | "}" | SP | HT
        // TEXT          = <any OCTET except CTLs, but including LWS>
        // CHAR          = <UTF-8 characters; RFC3629>  ; (0-191, 194-244)
        // DIGIT         = <any US-ASCII digit "0".."9">
        // CTL           = <any US-ASCII control character (octets 0 - 31) and DEL (127)>
        // CR            = <ASCII CR, carriage return>  ; (13)
        // LF            = <ASCII LF, linefeed>         ; (10)
        // SP            = <ASCII SP, space>            ; (32)
        // HT            = <ASCII HT, horizontal-tab>   ; (9)
        // CRLF          = CR LF
        // LWS           = [CRLF] 1*( SP | HT )         ; semantics same as single SP
        // quoted-string = ( <"> *(qdtext | quoted-pair ) <"> )
        // qdtext        = <any TEXT except <">>
        // quoted-pair   = "\" CHAR ; single-character quoting
        // uri           = <'URI' per RFC3986>

        private static readonly Parser<char, string> FieldName = Token.AtLeastOnceString();
        private static readonly Parser<char, string> FieldValue = Try(Sp.Or(Ht).Many()).Then(Text).ManyString();

        public static readonly Parser<char, NamedField> NamedField = Map(
                                                                         (name, value) => new NamedField(name, value),
                                                                         FieldName.Before(Char(':')),
                                                                         FieldValue
                                                                     )
                                                                     .Before(Crlf)
                                                                     .Labelled(nameof(NamedField));

        public static readonly Parser<char, IEnumerable<NamedField>> NamedFields = NamedField.Many().Labelled(nameof(NamedFields));

        public static readonly Parser<char, Version> WarcVersion = CIString("WARC/").Then(Real)
                                                                                    .Map(x => Version.Parse(x.ToString(CultureInfo.InvariantCulture)))
                                                                                    .Before(Crlf)
                                                                                    .Labelled(nameof(WarcVersion));

        public static readonly Parser<char, WarcRecordHeader> WarcHeader = Map(
                                                                               (version, fields) => new WarcRecordHeader(version, new List<NamedField>(fields)),
                                                                               WarcVersion,
                                                                               NamedFields
                                                                           )
                                                                           .Before(Crlf)
                                                                           .Labelled(nameof(WarcHeader));
    }
}