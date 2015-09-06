// Copyright (c) 2015 Paul Mattes.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the names of Paul Mattes nor the names of his contributors
//       may be used to endorse or promote products derived from this software
//       without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY PAUL MATTES "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
// EVENT SHALL PAUL MATTES BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
// OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

// Tools for interrogating the result of ReadBuffer.

namespace x3270if
{
    /// <summary>
    /// 3270 intensity enumeration (basic field attribute)
    /// </summary>
    public enum FieldIntensity : byte
    {
        /// <summary>
        /// Normal.
        /// </summary>
        Normal = 0x00,
        /// <summary>
        /// Normal, lightpen selectable.
        /// </summary>
        NormalSelectable = 0x04,
        /// <summary>
        /// Highlighted, lightpen selectable.
        /// </summary>
        HighlightedSelectable = 0x08,
        /// <summary>
        /// Invisible (passwords).
        /// </summary>
        Zero = 0x0c
    }

    /// <summary>
    /// Miscellaneous field attribute flags.
    /// </summary>
    [Flags]
    public enum FieldFlags : byte
    {
        /// <summary>
        /// Default (not protected or intensified).
        /// </summary>
        None = 0,
        /// <summary>
        /// Protected field (can't type input).
        /// </summary>
        Protected = 0x20,
        /// <summary>
        /// Numeric input only.
        /// </summary>
        Numeric = 0x10,
        /// <summary>
        /// Modified.
        /// </summary>
        Modified = 0x01,
        /// <summary>
        /// All possible flags.
        /// </summary>
        All = Protected | Numeric | Modified
    }

    /// <summary>
    /// A foreground or background color.
    /// </summary>
    public enum FieldColor : byte
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Neutral black (black on a screen, white on a printer)
        /// </summary>
        NeutralBlack = 0xf0,
        /// <summary>
        /// Blue.
        /// </summary>
        Blue = 0xf1,
        /// <summary>
        /// Red.
        /// </summary>
        Red = 0xf2,
        /// <summary>
        /// Pink.
        /// </summary>
        Pink = 0xf3,
        /// <summary>
        /// Green.
        /// </summary>
        Green = 0xf4,
        /// <summary>
        /// Turquiose.
        /// </summary>
        Turquoise = 0xf5,
        /// <summary>
        /// Yellow.
        /// </summary>
        Yellow = 0xf6,
        /// <summary>
        /// Neutral white (white on a screen, black on a printer)
        /// </summary>
        NeutralWhite = 0xf7,
        /// <summary>
        /// Black.
        /// </summary>
        Black = 0xf8,
        /// <summary>
        /// Deep blue.
        /// </summary>
        DeepBlue = 0xf9,
        /// <summary>
        /// Orange.
        /// </summary>
        Orange = 0xfa,
        /// <summary>
        /// Purple.
        /// </summary>
        Purple = 0xfb,
        /// <summary>
        /// Pale green.
        /// </summary>
        PaleGreen = 0xfc,
        /// <summary>
        /// Pale turquiose.
        /// </summary>
        PaleTurquoise = 0xfd,
        /// <summary>
        /// Gray.
        /// </summary>
        Gray = 0xfe,
        /// <summary>
        /// White.
        /// </summary>
        White = 0xff
    }

    /// <summary>
    /// An extended attribute.
    /// </summary>
    public enum ExtendedAttribute : byte
    {
        /// <summary>
        /// Standard 3270 field attributes.
        /// </summary>
        Ea3270 = 0xc0,
        /// <summary>
        /// Field validation.
        /// </summary>
        Validation = 0xc1,
        /// <summary>
        /// Field outlining.
        /// </summary>
        Outlining = 0xc2,
        /// <summary>
        /// Field highlighting.
        /// </summary>
        Highlighting = 0x41,
        /// <summary>
        /// Foreground color.
        /// </summary>
        Foreground = 0x42,
        /// <summary>
        /// Character set.
        /// </summary>
        CharacterSet = 0x43,
        /// <summary>
        /// Background color.
        /// </summary>
        Background = 0x45,
        /// <summary>
        /// Field transparency.
        /// </summary>
        Transparency = 0x46,
        /// <summary>
        /// Input control enable.
        /// </summary>
        InputControl = 0xfe
    }

    /// <summary>
    /// A character set.
    /// </summary>
    public enum CharacterSet : byte
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// APL and line drawing (EBCDIC).
        /// </summary>
        Apl = 0xf1,
        /// <summary>
        /// DEC line drawing (NVT mode, x3270 extension).
        /// </summary>
        LineDrawing = 0xf2,
        /// <summary>
        /// DBCS.
        /// </summary>
        Dbcs = 0xf8
    }

    /// <summary>
    /// The Validation extended attribute.
    /// </summary>
    public enum Validation : byte
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Mandatory fill.
        /// </summary>
        Fill = 0x04,
        /// <summary>
        /// Not sure.
        /// </summary>
        Entry = 0x02,
        /// <summary>
        /// Not sure.
        /// </summary>
        Trigger = 0x01
    }

    /// <summary>
    /// The Outlining extended attribute (ORed together).
    /// </summary>
    public enum Outlining : byte
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Line under.
        /// </summary>
        Underline = 0x01,
        /// <summary>
        /// Line to the left.
        /// </summary>
        Left = 0x02,
        /// <summary>
        /// Line over.
        /// </summary>
        Overline = 0x04,
        /// <summary>
        /// Line to the right.
        /// </summary>
        Right = 0x08
    }

    /// <summary>
    /// The Highlighting extended attribute.
    /// </summary>
    public enum Highlighting : byte
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Normal intensity.
        /// </summary>
        Normal = 0xf0,
        /// <summary>
        /// Blinking.
        /// </summary>
        Blink = 0xf1,
        /// <summary>
        /// Reverse foreground and background colors.
        /// </summary>
        Reverse = 0xf2,
        /// <summary>
        /// Underlined.
        /// </summary>
        Underscore = 0xf4,
        /// <summary>
        /// Intensified.
        /// </summary>
        Intensify = 0xf8
    }

    /// <summary>
    /// The Transparency extended attribute.
    /// </summary>
    public enum Transparency : byte
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Text is ORed.
        /// </summary>
        Or = 0xf0,
        /// <summary>
        /// Text is XORed.
        /// </summary>
        Xor = 0xf1,
        /// <summary>
        /// Text is opaque.
        /// </summary>
        Opaque = 0xff
    }

    /// <summary>
    /// The InputControl extended attribute.
    /// </summary>
    public enum InputControl : byte
    {
        /// <summary>
        /// Default (no input control).
        /// </summary>
        Default = 0,
        /// <summary>
        /// Input control enabled.
        /// </summary>
        Enabled = 0x01
    }

    /// <summary>
    /// Types of display buffer data (one position).
    /// </summary>
    public enum PositionType
    {
        /// <summary>
        /// An ASCII character (if buffer was read in ASCII mode).
        /// </summary>
        Ascii,
        /// <summary>
        /// An EBCDIC character (if buffer was read in EBCDIC mode).
        /// </summary>
        Ebcdic,
        /// <summary>
        /// The right-hand side of a DBCS character.
        /// </summary>
        DbcsRight,
        /// <summary>
        /// Field attribute order.
        /// </summary>
        FieldAttribute  // Field attribute order
    }
        
    /// <summary>
    /// 3270 field attributes.
    /// </summary>
    public class Attrs
    {
        // Basic 3270 attributes. These are actually part of the same byte, but four of the bits
        // are treated like an enum, so it's easier to split them here.
        /// <summary>
        /// Field intensity.
        /// </summary>
        public FieldIntensity Intensity;
        /// <summary>
        /// Flags.
        /// </summary>
        public FieldFlags Flags; 

        // Extended attributes.
        /// <summary>
        /// Foreground color.
        /// </summary>
        public FieldColor Foreground;
        /// <summary>
        /// Background color.
        /// </summary>
        public FieldColor Background;
        /// <summary>
        /// Character set.
        /// </summary>
        public CharacterSet CharacterSet;
        /// <summary>
        /// Highlighting options.
        /// </summary>
        public Highlighting Highlighting;
        /// <summary>
        /// Outlining options.
        /// </summary>
        public Outlining Outlining;
        /// <summary>
        /// Transparency.
        /// </summary>
        public Transparency Transparency;
        /// <summary>
        /// Input control.
        /// </summary>
        public InputControl InputControl;
        /// <summary>
        /// Field validation.
        /// </summary>
        public Validation Validation;

        /// <summary>
        /// Clone an Attrs object.
        /// </summary>
        /// <returns>New copy</returns>
        public Attrs Clone()
        {
            return new Attrs
            {
                Intensity = this.Intensity,
                Flags = this.Flags,
                Foreground = this.Foreground,
                Background = this.Background,
                CharacterSet = this.CharacterSet,
                Highlighting = this.Highlighting,
                Outlining = this.Outlining,
                Transparency = this.Transparency,
                InputControl = this.InputControl,
                Validation = this.Validation
            };
        }
    }

    /// <summary>
    /// Contents of one position in the 3270 display buffer.
    /// Consists of the Type numeration, restricted methods to get the ASCII or EBCDIC character, and
    /// a set of Attributes.
    /// </summary>
    public class DisplayPosition
    {
        /// <summary>
        /// The type of data in this position.
        /// </summary>
        public PositionType Type;

        // The displayed text.
        private ushort ebcdicChar;
        /// <summary>
        /// The EBCDIC value for a screen position.
        /// </summary>
        public ushort EbcdicChar
        {
            get
            {
                switch (Type)
                {
                    case PositionType.Ascii:
                        throw new InvalidOperationException("Cannot get EBCDIC value from an ASCII ReadBuffer result");
                    case PositionType.Ebcdic:
                        return (Attrs.Intensity != FieldIntensity.Zero) ? ebcdicChar : (ushort)0x40;
                    default:
                        throw new InvalidOperationException("Cannot get EBCDIC value from non-display position");
                }
            }
            set
            {
                ebcdicChar = value;
            }
        }
        private char asciiChar;
        /// <summary>
        /// The ASCII value for a screen position.
        /// </summary>
        public char AsciiChar
        {
            get
            {
                switch (Type)
                {
                    case PositionType.Ascii:
                        return (Attrs.Intensity != FieldIntensity.Zero) ? asciiChar : ' ';
                    case PositionType.Ebcdic:
                        throw new InvalidOperationException("Cannot get ASCII value from an EBCDIC ReadBuffer result");
                    default:
                        throw new InvalidOperationException("Cannot get ASCII value from non-display position");
                }
            }
            set
            {
                asciiChar = value;
            }
        }

        /// <summary>
        /// Field attributes.
        /// </summary>
        public Attrs Attrs;
    }

    /// <summary>
    /// Helper class for manipulating the output of ReadBuffer.
    /// </summary>
    public class DisplayBuffer
    {
        /// <summary>
        /// The original ioResult.
        /// </summary>
        private Session.ReadBufferIoResult ioResult;

        /// <summary>
        /// Coordinate origin (0 [default] or 1)
        /// </summary>
        public int Origin
        {
            get { return ioResult.Origin; }
        }

        // Screen dimensions.
        private int rows, columns;
        private int cursorRow, cursorColumn;

        // Encoding.
        private Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// The screen contents, as an array indexed by 0-origin row and column.
        /// </summary>
        public DisplayPosition[,] ContentsArray;

        /// <summary>
        /// The screen contents, one location.
        /// </summary>
        /// <param name="row">Row, using the defined origin.</param>
        /// <param name="column">Column, using the defined origin.</param>
        /// <returns>Contents of one screen location</returns>
        public DisplayPosition Contents(int row, int column)
        {
            return ContentsArray[row - Origin, column - Origin];
        }

        /// <summary>
        /// The screen contents, one location, indexed by Coordinates.
        /// </summary>
        /// <param name="c">Coordinates</param>
        /// <returns></returns>
        public DisplayPosition Contents(Coordinates c)
        {
            return ContentsArray[c.Row - Origin, c.Column - Origin];
        }

        /// <summary>
        /// Basic constructor for displayBuffer.
        /// </summary>
        /// <param name="r">Result from ReadBuffer or ReadBufferAsync.</param>
        public DisplayBuffer(Session.ReadBufferIoResult r)
        {
            ioResult = r;
            string[] statusFields = r.StatusLine.Split(' ');
            rows = int.Parse(statusFields[(int)StatusLineField.Rows]);
            columns = int.Parse(statusFields[(int)StatusLineField.Columns]);
            cursorRow = int.Parse(statusFields[(int)StatusLineField.CursorRow]);
            cursorColumn = int.Parse(statusFields[(int)StatusLineField.CursorColumn]);
            Encoding = r.Encoding;
            ContentsArray = new DisplayPosition[rows, columns];
            Attrs faAttrs = new Attrs();
            Attrs saAttrs = new Attrs();

            for (int row = 0; row < rows; row++)
            {
                parseRow(r.Result[row], row, ref faAttrs, ref saAttrs);
            }

            // Last, ugly step.
            // Apply the basic 3270 attributes from the last SF on the screen onto the beginning of the screen,
            // until we run into the first SF.
            Coordinates firstField = null;
            Coordinates lastField = null;
            var zero = new Coordinates(this);
            var c = zero.Clone();
            do
            {
                if (Contents(c).Type == PositionType.FieldAttribute)
                {
                    if (firstField == null)
                    {
                        firstField = c.Clone();
                    }
                    lastField = c.Clone();
                }
            } while (++c != zero);
            if (firstField != null)
            {
                Attrs lastAttrs = Contents(lastField).Attrs;
                for (c = zero; c < firstField; c++)
                {
                    Contents(c).Attrs.Flags = lastAttrs.Flags;
                    Contents(c).Attrs.Intensity = lastAttrs.Intensity;
                }
            }
        }

        /// <summary>
        /// Parse a row of ReadBuffer fields.
        /// </summary>
        /// <param name="text">One line out output from EbdcicField.</param>
        /// <param name="row">Row index.</param>
        /// <param name="faAttrs">Current FA attributes</param>
        /// <param name="saAttrs">Current SA attributes</param>
        private void parseRow(string text, int row, ref Attrs faAttrs, ref Attrs saAttrs)
        {
            string[] split = text.Split(' ');
            int column = 0;
            Attrs attrs = CombineAttrs(faAttrs, saAttrs);
            foreach (string s in split)
            {
                if (s.StartsWith("SA("))
                {
                    parseSA(s, ref saAttrs);
                    attrs = CombineAttrs(faAttrs, saAttrs);
                    continue;
                }

                if (s == "-")
                {
                    ContentsArray[row, column] = new DisplayPosition
                    {
                        Type = PositionType.DbcsRight,
                        Attrs = attrs
                    };
                }
                else if (s.StartsWith("SF("))
                {
                    faAttrs = new Attrs();
                    parseSA(s, ref faAttrs);
                    attrs = CombineAttrs(faAttrs, saAttrs);
                    ContentsArray[row, column] = new DisplayPosition
                    {
                        Type = PositionType.FieldAttribute,
                        Attrs = attrs
                    };
                }
                else if (s.StartsWith("GE("))
                {
                    // Unwrap GE(nn).
                    string hexChar = s.Substring(3, s.Length - 4);

                    Attrs geAttrs = attrs.Clone();
                    geAttrs.CharacterSet = CharacterSet.Apl;
                    if (ioResult.ReadBufferType == Session.ReadBufferType.Ascii)
                    {
                        ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ascii,
                            AsciiChar = parseHexEncoded(hexChar),
                            Attrs = geAttrs
                        };
                    }
                    else
                    {
                        ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ebcdic,
                            EbcdicChar = ushort.Parse(hexChar, System.Globalization.NumberStyles.HexNumber),
                            Attrs = geAttrs
                        };
                    }
                }
                else
                {
                    if (ioResult.ReadBufferType == Session.ReadBufferType.Ascii)
                    {
                        ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ascii,
                            AsciiChar = parseHexEncoded(s),
                            Attrs = attrs
                        };
                    }
                    else
                    {
                        ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ebcdic,
                            EbcdicChar = ushort.Parse(s, System.Globalization.NumberStyles.HexNumber),
                            Attrs = attrs
                        };
                    }
                }
                column++;
            }
        }

        /// <summary>
        /// Translate a sequence encoded in hex (e.g., "A1C3" for 'á' in UTF-8 mode) into a character.
        /// </summary>
        /// <param name="hexChar"></param>
        /// <returns></returns>
        private char parseHexEncoded(string hexChar)
        {
            var bytes = new byte[hexChar.Length / 2];
            for (int i = 0; i < hexChar.Length; i += 2)
            {
                bytes[i / 2] = byte.Parse(hexChar.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return Encoding.GetString(bytes)[0];
        }

        /// <summary>
        /// Helper function to parse a hexadecimal byte.
        /// </summary>
        /// <param name="value">Text to parse</param>
        /// <returns>Byte value if valid, or null.</returns>
        private byte? TryParseHex(string value)
        {
            byte i = 0;
            if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out i))
            {
                return i;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parse an extended attrbute enumeration in hex.
        /// If the value is unparseable or unsupported, return the default for the type.
        /// </summary>
        /// <typeparam name="T">Enumeration type.</typeparam>
        /// <param name="value">Text to parse.</param>
        /// <param name="result">Value stored here, if valid.</param>
        private void parseExtendedAttribute<T>(string value, ref T result) where T : struct, IConvertible
        {
            var i = TryParseHex(value);
            if (i.HasValue)
            {
                T rValue = (T)(object)i.Value;
                if (Enum.IsDefined(typeof(T), rValue))
                {
                    result = rValue;
                }
                else
                {
                    result = (T)Enum.Parse(typeof(T), "Default");
                }
            }
        }

        /// <summary>
        /// Parse an SA, which defines attributes that override SF attributes.
        /// </summary>
        /// <param name="field">SA(xx=yy) text</param>
        /// <param name="attrs">Current/modified attributes</param>
        private void parseSA(string field, ref Attrs attrs)
        {
            const byte IntensityMask = 0x0c;

            // Unwrap the SA(...).
            string guts = field.Substring(3, field.Length - 4);
            var attributes = guts.Split(',');
            foreach (string attribute in attributes)
            {
                var expr = attribute.Split('=');
                byte b;
                if (!byte.TryParse(expr[0], System.Globalization.NumberStyles.HexNumber, null, out b))
                {
                    continue;
                }
                switch ((ExtendedAttribute)b)
                {
                    case ExtendedAttribute.Ea3270:
                        var ea3270 = byte.Parse(expr[1], System.Globalization.NumberStyles.HexNumber);
                        attrs.Flags = (FieldFlags)(ea3270 & (byte)(FieldFlags.All));
                        attrs.Intensity = (FieldIntensity)(ea3270 & IntensityMask);
                        break;
                    case ExtendedAttribute.Background:
                        parseExtendedAttribute<FieldColor>(expr[1], ref attrs.Background);
                        break;
                    case ExtendedAttribute.Foreground:
                        parseExtendedAttribute<FieldColor>(expr[1], ref attrs.Foreground);
                        break;
                    case ExtendedAttribute.CharacterSet:
                        parseExtendedAttribute<CharacterSet>(expr[1], ref attrs.CharacterSet);
                        break;
                    case ExtendedAttribute.Highlighting:
                        parseExtendedAttribute<Highlighting>(expr[1], ref attrs.Highlighting);
                        break;
                    case ExtendedAttribute.Transparency:
                        parseExtendedAttribute<Transparency>(expr[1], ref attrs.Transparency);
                        break;
                    case ExtendedAttribute.Outlining:
                        parseExtendedAttribute<Outlining>(expr[1], ref attrs.Outlining);
                        break;
                    case ExtendedAttribute.InputControl:
                        parseExtendedAttribute<InputControl>(expr[1], ref attrs.InputControl);
                        break;
                    case ExtendedAttribute.Validation:
                        parseExtendedAttribute<Validation>(expr[1], ref attrs.Validation);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Return sa if non-default, else fa.
        /// </summary>
        /// <typeparam name="T">Enum type that defines Default.</typeparam>
        /// <param name="fa">Other value.</param>
        /// <param name="sa">Value that takes precedence.</param>
        /// <returns>sa if non-default, else fa.</returns>
        private T setNonDefault<T>(T fa, T sa) where T: IComparable
        {
            T defaultValue = (T)Enum.Parse(typeof(T), "Default");
            if (!sa.Equals(defaultValue))
            {
                return sa;
            }
            else
            {
                return fa;
            }
        }

        /// <summary>
        /// Combine SA (override) and FA attributes to determine field attributes for a buffer position.
        /// </summary>
        /// <param name="faAttrs">Default field attributes</param>
        /// <param name="saAttrs">Override attributes, if non-default</param>
        /// <returns>Merged attributes</returns>
        private Attrs CombineAttrs(Attrs faAttrs, Attrs saAttrs)
        {
            Attrs resultAttrs = new Attrs();

            resultAttrs.Intensity = faAttrs.Intensity;
            resultAttrs.Flags = faAttrs.Flags;

            resultAttrs.Foreground = setNonDefault<FieldColor>(faAttrs.Foreground, saAttrs.Foreground);
            resultAttrs.Background = setNonDefault<FieldColor>(faAttrs.Background, saAttrs.Background);
            resultAttrs.CharacterSet = setNonDefault<CharacterSet>(faAttrs.CharacterSet, saAttrs.CharacterSet);
            resultAttrs.Highlighting = setNonDefault<Highlighting>(faAttrs.Highlighting, saAttrs.Highlighting);
            resultAttrs.Transparency = setNonDefault<Transparency>(faAttrs.Transparency, saAttrs.Transparency);
            resultAttrs.Outlining = setNonDefault<Outlining>(faAttrs.Outlining, saAttrs.Outlining);
            resultAttrs.InputControl = setNonDefault<InputControl>(faAttrs.InputControl, saAttrs.InputControl);
            resultAttrs.Validation = setNonDefault<Validation>(faAttrs.Validation, saAttrs.Validation);
                
            return resultAttrs;
        }

        /// <summary>
        /// Map a host color onto a Windows console color.
        /// </summary>
        /// <param name="c">Host color.</param>
        /// <param name="isForeground">true if this is a foreground color.</param>
        /// <returns>Windows console color to use.</returns>
        private ConsoleColor ColorMap(FieldColor c, bool isForeground)
        {
            switch (c)
            {
                case FieldColor.NeutralBlack:
                    return ConsoleColor.Black;
                case FieldColor.Blue:
                    return ConsoleColor.Blue;
                case FieldColor.Red:
                    return ConsoleColor.Red;
                case FieldColor.Pink:
                    return ConsoleColor.Magenta;
                case FieldColor.Green:
                    return ConsoleColor.Green;
                case FieldColor.Turquoise:
                    return ConsoleColor.Cyan;
                case FieldColor.Yellow:
                    return ConsoleColor.Yellow;
                case FieldColor.NeutralWhite:
                    return ConsoleColor.White;
                case FieldColor.Black:
                    return ConsoleColor.Black;
                case FieldColor.DeepBlue:
                    return ConsoleColor.DarkBlue;
                case FieldColor.Orange:
                    return ConsoleColor.DarkRed;
                case FieldColor.Purple:
                    return ConsoleColor.DarkMagenta;
                case FieldColor.PaleGreen:
                    return ConsoleColor.DarkGreen;
                case FieldColor.PaleTurquoise:
                    return ConsoleColor.DarkCyan;
                case FieldColor.Gray:
                    return ConsoleColor.DarkGray;
                case FieldColor.White:
                    return ConsoleColor.White;
                default:
                case FieldColor.Default:
                    if (isForeground)
                    {
                        return ConsoleColor.White;
                    }
                    else
                    {
                        return ConsoleColor.Black;
                    }
            }
        }

        /// <summary>
        /// Write an ASCII ReadBuffer buffer out to the console, with proper formatting (if possible).
        /// </summary>
        public void DumpAsciiConsole()
        {
            if (ioResult.ReadBufferType != Session.ReadBufferType.Ascii)
            {
                throw new InvalidOperationException("ReadBuffer is not Ascii");
            }
            FieldColor fg = FieldColor.NeutralWhite;
            FieldColor bg = FieldColor.NeutralBlack;
            Console.ForegroundColor = ColorMap(fg, true);
            Console.BackgroundColor = ColorMap(bg, false);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    var c = ContentsArray[row, column];
                    var a = c.Attrs;
                    if (a.Foreground != fg)
                    {
                        fg = a.Foreground;
                        Console.ForegroundColor = ColorMap(fg, true);
                    }
                    if (a.Background != bg)
                    {
                        bg = a.Background;
                        Console.BackgroundColor = ColorMap(bg, false);
                    }
                    switch (c.Type)
                    {
                        case PositionType.Ascii:
                            // ASCII character.
                            if (a.Intensity == FieldIntensity.Zero || c.AsciiChar < ' ')
                            {
                                Console.Write(" ");
                            }
                            else
                            {
                                Console.Write(c.AsciiChar);
                            }
                            break;
                        case PositionType.DbcsRight:
                            // Right side of DBCS -- nothing.
                        case PositionType.Ebcdic:
                            // EBCDIC: Can't happen.
                            break;
                        case PositionType.FieldAttribute:
                            // Field attribute -- blank.
                            Console.Write(" ");
                            break;
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Translate one screen position to a character.
        /// </summary>
        /// <param name="c">Display position to translate</param>
        /// <returns>character, or null if position is to be skipped</returns>
        private char? TranslatePosition(DisplayPosition c)
        {
            switch (c.Type)
            {
                case PositionType.Ascii:
                    // ASCII character.
                    if (c.Attrs.Intensity == FieldIntensity.Zero || c.AsciiChar < ' ')
                    {
                        return ' ';
                    }
                    else
                    {
                        return c.AsciiChar;
                    }
                case PositionType.DbcsRight:
                    // Right side of DBCS -- skip it.
                    return null;
                default:
                    // Anything else -- blank.
                    return ' ';
            }
        }

        // The following methods allow the entire screen to be interrogated with a single
        // ReadBuffer call, which can then be picked apart into individual fields. This is
        // much faster than using the emulator Ascii methods, each of which requires a
        // round trip over the socket and context switches.

        #region Ascii methods
        /// <summary>
        /// Return an ASCII string at the cursor address.
        /// </summary>
        /// <param name="length">Length of field to return</param>
        /// <returns>String</returns>
        public string Ascii(int length)
        {
            return Ascii(cursorRow, cursorColumn, length);
        }

        /// <summary>
        /// Return an ASCII string starting at the specified address.
        /// </summary>
        /// <param name="row">Starting row</param>
        /// <param name="column">Starting column</param>
        /// <param name="length">Length of field></param>
        /// <returns>String</returns>
        public String Ascii(int row, int column, int length)
        {
            if (ioResult.ReadBufferType != Session.ReadBufferType.Ascii)
            {
                throw new InvalidOperationException("ReadBuffer is not Ascii");
            }
            row -= Origin;
            column -= Origin;
            if (row < 0 || row >= rows)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < 0 || column >= columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            if (length < 0 || (row * columns) + column + length > rows * columns)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var curRow = row;
            var curColumn = column;
            for (var lengthLeft = length; lengthLeft > 0; lengthLeft--)
            {
                var r = TranslatePosition(ContentsArray[curRow, curColumn]);
                if (r.HasValue)
                {
                    sb.Append((char)r);
                }
                if (++curColumn >= columns)
                {
                    curRow++;
                    curColumn = 0;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Translate a ReadBuffer region to ASCII.
        /// </summary>
        /// <param name="row">Starting row</param>
        /// <param name="column">Starting column</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <returns>Array of strings, one entry per row</returns>
        public string[] Ascii(int row, int column, int rows, int columns)
        {
            if (ioResult.ReadBufferType != Session.ReadBufferType.Ascii)
            {
                throw new InvalidOperationException("ReadBuffer is not Ascii");
            }
            row -= Origin;
            column -= Origin;
            if (row < 0 || row >= this.rows)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < 0 || column >= this.columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            if (rows <= 0 || row + rows > this.rows)
            {
                throw new ArgumentOutOfRangeException("rows");
            }
            if (columns <= 0 || column + columns > this.columns)
            {
                throw new ArgumentOutOfRangeException("columns");
            }

            string[] ret = new string[rows];
            for (var r = row; r < row + rows; r++)
            {
                StringBuilder sb = new StringBuilder();

                for (var c = column; c < column + columns; c++)
                {
                    var rc = TranslatePosition(ContentsArray[r, c]);
                    if (rc.HasValue)
                    {
                        sb.Append((char)rc);
                    }
                }
                ret[r - row] = sb.ToString();
            }
            return ret;
        }

        /// <summary>
        /// Translate an entire ASCII ReadBuffer buffer to simple ASCII.
        /// </summary>
        /// <returns>String</returns>
        public string[] Ascii()
        {
            return Ascii(Origin, Origin, rows, columns);
        }

        /// <summary>
        /// A row and column for specifying a location in a display buffer.
        /// </summary>
        public class Coordinates : IEquatable<Coordinates>
        {
            private int rows;
            private int columns;
            private int origin;

            // The row, 0-origin.
            private int row;

            /// <summary>
            /// Row.
            /// </summary>
            public int Row
            {
                get { return row + origin; }
                set
                {
                    value -= origin;
                    if (value < 0 || value >= rows)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    row = value;
                }
            }

            // The column, zero-origin.
            private int column;
            /// <summary>
            /// Column.
            /// </summary>
            public int Column
            {
                get { return column + origin; }
                set
                {
                    value -= origin;
                    if (value < 0 || value >= columns)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    column = value;
                }
            }

            /// <summary>
            /// Constructor, given a DisplayBuffer and an optional initial row and column.
            /// </summary>
            /// <param name="displayBuffer"></param>
            /// <param name="row"></param>
            /// <param name="column"></param>
            public Coordinates(DisplayBuffer displayBuffer, int? row = null, int? column = null)
            {
                rows = displayBuffer.rows;
                columns = displayBuffer.columns;
                origin = displayBuffer.Origin;

                Row = row ?? origin;
                Column = column ?? origin;
            }

            /// <summary>
            /// Cloning constructor.
            /// </summary>
            /// <param name="c">Coordinates to clone.</param>
            public Coordinates(Coordinates c)
            {
                this.rows = c.rows;
                this.columns = c.columns;
                this.origin = c.origin;

                this.Row = c.Row;
                this.Column = c.Column;
            }

            /// <summary>
            /// Increment operator.
            /// </summary>
            /// <param name="c">Coordinates to increment</param>
            /// <returns>New coordinates, incremented and wrapped if necessary</returns>
            public static Coordinates operator++(Coordinates c)
            {
                Coordinates ret = new Coordinates(c);

                if (++ret.column >= c.columns)
                {
                    ret.column = 0;
                    if (++ret.row >= c.rows)
                    {
                        ret.row = 0;
                    }
                }
                return ret;
            }

            /// <summary>
            /// Decrement operator.
            /// </summary>
            /// <param name="c">Coordinates to decrement.</param>
            /// <returns>New coordinates, decremented and wrapped if necessary</returns>
            public static Coordinates operator--(Coordinates c)
            {
                Coordinates ret = new Coordinates(c);

                if (--ret.column < 0)
                {
                    ret.column = c.columns - 1;
                    if (--ret.row < 0)
                    {
                        ret.row = c.rows - 1;
                    }
                }
                return ret;
            }

            /// <summary>
            /// Equality operator.
            /// </summary>
            /// <param name="c1">First coordinates</param>
            /// <param name="c2">Second coordinates</param>
            /// <returns>true if they are equal</returns>
            public static bool operator==(Coordinates c1, Coordinates c2)
            {
                if (ReferenceEquals(c1, c2))
                {
                    return true;
                }
                if ((object)c1 == null || (object)c2 == null)
                {
                    return false;
                }
                return c1.row == c2.row && c1.column == c2.column;
            }

            /// <summary>
            /// Inequality operator.
            /// </summary>
            /// <param name="c1">First coordinates</param>
            /// <param name="c2">Second coordinates</param>
            /// <returns>true if they are unequal</returns>
            public static bool operator!=(Coordinates c1, Coordinates c2)
            {
                return !(c1 == c2);
            }

            /// <summary>
            /// Equality method.
            /// </summary>
            /// <param name="other">Other coordinates</param>
            /// <returns>true if they are equal</returns>
            public bool Equals(Coordinates other)
            {
                return this == other;
            }

            /// <summary>
            /// Equality method.
            /// </summary>
            /// <param name="other">Other coordinates (which might not be a Coordinates object)</param>
            /// <returns>true if they are equal</returns>
            public override bool Equals(object other)
            {
                var otherCoordinate = other as Coordinates;
                if (otherCoordinate == null)
                {
                    return false;
                }
                return this == otherCoordinate;
            }

            /// <summary>
            /// Hash generator for Coordinates.
            /// </summary>
            /// <returns>Hash value</returns>
            public override int GetHashCode()
            {
                return Row ^ Column;
            }

            /// <summary>
            /// Greater-than operator.
            /// </summary>
            /// <param name="c1">First coordinates.</param>
            /// <param name="c2">Second coordinates.</param>
            /// <returns>true if c1 &gt; c2</returns>
            public static bool operator> (Coordinates c1, Coordinates c2)
            {
                if (c1 == null)
                {
                    throw new ArgumentNullException("c1");
                }
                if (c2 == null)
                {
                    throw new ArgumentNullException("c2");
                }
                return c1.BufferAddress > c2.BufferAddress;
            }

            /// <summary>
            /// Less-than operator.
            /// </summary>
            /// <param name="c1">First coordinates.</param>
            /// <param name="c2">Second coordinates.</param>
            /// <returns>true if c1 &lt; c2.</returns>
            public static bool operator< (Coordinates c1, Coordinates c2)
            {
                if (c1 == null)
                {
                    throw new ArgumentNullException("c1");
                }
                if (c2 == null)
                {
                    throw new ArgumentNullException("c2");
                }
                return c1.BufferAddress < c2.BufferAddress;
            }

            /// <summary>
            /// Clone method.
            /// </summary>
            /// <returns>New copy.</returns>
            public Coordinates Clone()
            {
                return new Coordinates(this);
            }

            /// <summary>
            /// Buffer address (0-origin index into screen buffer).
            /// </summary>
            public int BufferAddress
            {
                get { return (row * columns) + column; }
            }

            /// <summary>
            /// String conversion method.
            /// </summary>
            /// <returns>Human-readable text</returns>
            public override string ToString()
            {
                return "[" + Row + "," + Column + "]";
            }
        }

        /// <summary>
        /// Find the coordinates of the Field Attribute for a given screen position.
        /// </summary>
        /// <param name="c">coordinates</param>
        /// <returns>Row and column, or null if the screen is unformatted</returns>
        private Coordinates FaPosition(Coordinates c)
        {
            Coordinates b = new Coordinates(c);
            do
            {
                if (Contents(b).Type == PositionType.FieldAttribute)
                {
                    return b;
                }
                b--;
            } while (b != c);

            // Unformatted screen.
            return null;
        }

        /// <summary>
        /// Return the length of the field that starts with the field attribute at the specified
        /// coordinates. The length does not include the field attribute itself, so it can be zero.
        /// </summary>
        /// <param name="c">coordinates</param>
        /// <returns>Field length</returns>
        public int FieldLength(Coordinates c)
        {
            // Work backwards until we find the field attribute, or wrap.
            // We count c (if it is not an FA) and each non-FA to the left of it.
            int count = 0;
            var d = c.Clone();
            while (Contents(d).Type != PositionType.FieldAttribute)
            {
                count++;
                if (--d == c)
                {
                    // Wrapped back to the start. Unformatted screen.
                    return count;
                }
            }

            // Count forward from (c+1) until we find the next field attribute.
            d = c.Clone();
            while (Contents(++d).Type != PositionType.FieldAttribute)
            {
                count++;
            }
            return count;
        }

        /// <summary>
        /// Field length that uses a simple row and column.
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns>Field length</returns>
        public int FieldLength(int row, int column)
        {
            return FieldLength(new Coordinates(this, row, column));
        }

        /// <summary>
        /// Return a field value in ASCII.
        /// </summary>
        /// <param name="c">Coordinates.</param>
        /// <returns>Text</returns>
        public string AsciiField(Coordinates c = null)
        {
            if (c == null)
            {
                c = new Coordinates(this, cursorRow, cursorColumn);
            }

            // Find the field that contains this position.
            var faPosition = FaPosition(c);
            if (faPosition == null)
            {
                // Unformatted. One big blob.
                return Ascii(0, 0, rows * columns);
            }

            // Return just this field.
            var fieldLength = FieldLength(faPosition);
            faPosition++;
            return Ascii(faPosition.Row, faPosition.Column, fieldLength);
        }

        /// <summary>
        /// Return a field value in ASCII.
        /// </summary>
        /// <returns>Text</returns>
        public string AsciiField(int row, int column)
        {
            return AsciiField(new Coordinates(this, row, column));
        }

        /// <summary>
        /// Convenience method for checking screen contents.
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <param name="text">Text to compare</param>
        /// <returns>true if the screen at that location equals the string</returns>
        public bool AsciiEquals(int row, int column, string text)
        {
            return Ascii(row, column, text.Length) == text;
        }

        /// <summary>
        /// Convenience method for checking screen contents.
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <param name="length">Length of region</param>
        /// <param name="regex">Regular expression to match against</param>
        /// <returns>true if the regex matches the region of the screen</returns>
        public bool AsciiMatches(int row, int column, int length, string regex)
        {
            return Regex.Matches(Ascii(row, column, length), regex).Count != 0;
        }
        #endregion
    }
}
