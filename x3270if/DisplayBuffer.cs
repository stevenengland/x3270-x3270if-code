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
//
// Tools for interrogating the result of ReadBuffer.

namespace X3270if
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using X3270if.Attributes;

    /// <summary>
    /// Types of display buffer data (one position). Used by the <see cref="DisplayBuffer"/> class.
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
        /// The right-hand side of a DBCS character. Effectively empty -- the left-hand side (the preceding position) is an ASCII or EBCDIC character with the DBCS character code.
        /// </summary>
        DbcsRight,

        /// <summary>
        /// Field attribute order.
        /// </summary>
        FieldAttribute  // Field attribute order
    }
        
    /// <summary>
    /// 3270 field attributes. Used by the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public class Attrs
    {
        // Basic 3270 attributes. These are actually part of the same byte, but four of the bits
        // are treated like an enum, so it's easier to split them here.

        /// <summary>
        /// Gets or sets the field intensity.
        /// </summary>
        public FieldIntensity Intensity { get; set; }

        /// <summary>
        /// Gets or sets the field flags.
        /// </summary>
        public FieldFlags Flags { get; set; }

        // Extended attributes.

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        public FieldColor Foreground { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public FieldColor Background { get; set; }

        /// <summary>
        /// Gets or sets the character set.
        /// </summary>
        public CharacterSet CharacterSet { get; set; }

        /// <summary>
        /// Gets or sets highlighting options.
        /// </summary>
        public Highlighting Highlighting { get; set; }

        /// <summary>
        /// Gets or sets outlining options.
        /// </summary>
        public Outlining Outlining { get; set; }

        /// <summary>
        /// Gets or sets text transparency.
        /// </summary>
        public Transparency Transparency { get; set; }

        /// <summary>
        /// Gets or sets input control.
        /// </summary>
        public InputControl InputControl { get; set; }

        /// <summary>
        /// Gets or sets field validation.
        /// </summary>
        public Validation Validation { get; set; }

        /// <summary>
        /// Clone an <see cref="Attrs"/> object.
        /// </summary>
        /// <returns>New copy.</returns>
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
    /// A row and column for specifying a location in a display buffer.
    /// </summary>
    public class Coordinates : IEquatable<Coordinates>
    {
        /// <summary>
        /// The number of rows.
        /// </summary>
        private int rows;

        /// <summary>
        /// The number of columns.
        /// </summary>
        private int columns;

        /// <summary>
        /// The coordinate origin (0 or 1).
        /// </summary>
        private int origin;

        /// <summary>
        /// The row, always 0-origin.
        /// </summary>
        private int row;

        /// <summary>
        /// The column, always zero-origin.
        /// </summary>
        private int column;

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinates"/> class.
        /// Constructor, given a <see cref="DisplayBuffer"/> and an optional initial row and column.
        /// </summary>
        /// <param name="displayBuffer">Display buffer to get screen dimensions and origin from.</param>
        /// <param name="row">Optional initial row, defaults to <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Optional initial column, defaults to <see cref="X3270if.Config.Origin"/>.</param>
        public Coordinates(DisplayBuffer displayBuffer, int? row = null, int? column = null)
        {
            this.rows = displayBuffer.Rows;
            this.columns = displayBuffer.Columns;
            this.origin = displayBuffer.Origin;

            this.Row = row ?? this.origin;
            this.Column = column ?? this.origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinates"/> class.
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
        /// Gets or sets the row.
        /// </summary>
        public int Row
        {
            get
            {
                return this.row + this.origin;
            }

            set
            {
                value -= this.origin;
                if (value < 0 || value >= this.rows)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.row = value;
            }
        }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        public int Column
        {
            get
            {
                return this.column + this.origin;
            }

            set
            {
                value -= this.origin;
                if (value < 0 || value >= this.columns)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.column = value;
            }
        }

        /// <summary>
        /// Gets the buffer address (0-origin index into screen buffer).
        /// </summary>
        public int BufferAddress
        {
            get { return (this.row * this.columns) + this.column; }
        }

        /// <summary>
        /// Increment operator.
        /// </summary>
        /// <param name="c">Coordinates to increment.</param>
        /// <returns>New incremented coordinates, wrapped if necessary.</returns>
        public static Coordinates operator ++(Coordinates c)
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
        /// <returns>New decremented coordinates, wrapped if necessary.</returns>
        public static Coordinates operator --(Coordinates c)
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
        /// <param name="c1">First coordinates.</param>
        /// <param name="c2">Second coordinates.</param>
        /// <returns>True if <paramref name="c1"/> and <paramref name="c2"/> are equal.</returns>
        public static bool operator ==(Coordinates c1, Coordinates c2)
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
        /// <param name="c1">First coordinates.</param>
        /// <param name="c2">Second coordinates.</param>
        /// <returns>True if <paramref name="c1"/> and <paramref name="c2"/> are unequal.</returns>
        public static bool operator !=(Coordinates c1, Coordinates c2)
        {
            return !(c1 == c2);
        }

        /// <summary>
        /// Greater-than operator.
        /// </summary>
        /// <param name="c1">First coordinates.</param>
        /// <param name="c2">Second coordinates.</param>
        /// <returns>True if c1 &gt; c2.</returns>
        public static bool operator >(Coordinates c1, Coordinates c2)
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
        /// <returns>True if c1 &lt; c2.</returns>
        public static bool operator <(Coordinates c1, Coordinates c2)
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
        /// Equality method.
        /// </summary>
        /// <param name="other">Other coordinates.</param>
        /// <returns>True if this equals <paramref name="other"/>.</returns>
        public bool Equals(Coordinates other)
        {
            return this == other;
        }

        /// <summary>
        /// Equality method.
        /// </summary>
        /// <param name="other">Other coordinates (which might not be a <see cref="Coordinates"/>).</param>
        /// <returns>True if this equals <paramref name="other"/>.</returns>
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
        /// <returns>Hash value.</returns>
        public override int GetHashCode()
        {
            return this.Row ^ this.Column;
        }

        /// <summary>
        /// String conversion method.
        /// </summary>
        /// <returns>Human-readable text.</returns>
        public override string ToString()
        {
            return "[" + this.Row + "," + this.Column + "]";
        }

        /// <summary>
        /// Clone method.
        /// </summary>
        /// <returns>New copy.</returns>
        public Coordinates Clone()
        {
            return new Coordinates(this);
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
        /// The displayed text.
        /// </summary>
        private ushort ebcdicChar;

        /// <summary>
        /// Backing field for <see cref="AsciiChar"/>.
        /// </summary>
        private char asciiChar;

        /// <summary>
        /// Gets or sets the type of data in this position.
        /// </summary>
        public PositionType Type { get; set; }

        /// <summary>
        /// Gets or sets the EBCDIC value for a screen position.
        /// </summary>
        public ushort EbcdicChar
        {
            get
            {
                switch (this.Type)
                {
                    case PositionType.Ascii:
                        throw new InvalidOperationException("Cannot get EBCDIC value from an ASCII ReadBuffer result");
                    case PositionType.Ebcdic:
                        return (this.Attrs.Intensity != FieldIntensity.Zero) ? this.ebcdicChar : (ushort)0x40;
                    default:
                        throw new InvalidOperationException("Cannot get EBCDIC value from non-display position");
                }
            }

            set
            {
                this.ebcdicChar = value;
            }
        }

        /// <summary>
        /// Gets or sets the ASCII value for a screen position.
        /// </summary>
        public char AsciiChar
        {
            get
            {
                switch (this.Type)
                {
                    case PositionType.Ascii:
                        return (this.Attrs.Intensity != FieldIntensity.Zero) ? this.asciiChar : ' ';
                    case PositionType.Ebcdic:
                        throw new InvalidOperationException("Cannot get ASCII value from an EBCDIC ReadBuffer result");
                    default:
                        throw new InvalidOperationException("Cannot get ASCII value from non-display position");
                }
            }

            set
            {
                this.asciiChar = value;
            }
        }

        /// <summary>
        /// Gets or sets the field attributes.
        /// </summary>
        public Attrs Attrs { get; set; }
    }

    /// <summary>
    /// Helper class for manipulating the output of <see cref="Session.ReadBuffer"/>.
    /// </summary>
    /// <remarks>
    /// This class can be used to optimize interactions with the emulator and host.
    /// Every time the application sends an AID to the host, it can call <see cref="Session.ReadBuffer"/> and construct a
    /// DisplayBuffer from the result. Then it can interrogate the contents of the DisplayBuffer instead of asking the host
    /// for each field it wants to inspect. This eliminates the latency of multiple requests to the emulator, as well as
    /// ensuring that the screen image being interrogated for one field is the same image as queried for others (i.e., that this
    /// is one screen drawn by the host).
    /// <para>If the host has not yet finished drawing the screen, the application can call the <see cref="Session.Wait"/> method
    /// with the parameter <see cref="X3270if.WaitMode.Output"/>, waiting for the host to update the screen. Then it
    /// can call <see cref="Session.ReadBuffer"/> again, construct a new DisplayBuffer, and continue.
    /// </para>
    /// </remarks>
    public class DisplayBuffer
    {
        /// <summary>
        /// The original ioResult.
        /// </summary>
        private Session.ReadBufferIoResult ioResult;

        /// <summary>
        /// The encoding.
        /// </summary>
        private Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayBuffer"/> class.
        /// Basic constructor for a <see cref="DisplayBuffer"/>.
        /// </summary>
        /// <param name="r">Result from ReadBuffer or ReadBufferAsync.</param>
        public DisplayBuffer(Session.ReadBufferIoResult r)
        {
            this.ioResult = r;
            string[] statusFields = r.StatusLine.Split(' ');
            this.Rows = int.Parse(statusFields[(int)StatusLineField.Rows]);
            this.Columns = int.Parse(statusFields[(int)StatusLineField.Columns]);
            this.CursorRow = int.Parse(statusFields[(int)StatusLineField.CursorRow]);
            this.CursorColumn = int.Parse(statusFields[(int)StatusLineField.CursorColumn]);
            this.encoding = r.Encoding;
            this.ContentsArray = new DisplayPosition[this.Rows, this.Columns];
            Attrs faAttrs = new Attrs();
            Attrs saAttrs = new Attrs();

            for (int row = 0; row < this.Rows; row++)
            {
                this.ParseRow(r.Result[row], row, ref faAttrs, ref saAttrs);
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
                if (this.Contents(c).Type == PositionType.FieldAttribute)
                {
                    if (firstField == null)
                    {
                        firstField = c.Clone();
                    }

                    lastField = c.Clone();
                }
            }
            while (++c != zero);

            if (firstField != null)
            {
                Attrs lastAttrs = this.Contents(lastField).Attrs;
                for (c = zero; c < firstField; c++)
                {
                    this.Contents(c).Attrs.Flags = lastAttrs.Flags;
                    this.Contents(c).Attrs.Intensity = lastAttrs.Intensity;
                }
            }
        }

        /// <summary>
        /// Gets the coordinate origin (0 [default] or 1).
        /// This is derived from the session's <see cref="X3270if.Config.Origin"/>.
        /// </summary>
        public int Origin
        {
            get { return this.ioResult.Origin; }
        }

        /// <summary>
        /// Gets the number of rows on the screen.
        /// </summary>
        public int Rows { get; private set; }

        /// <summary>
        /// Gets the number of columns on the screen.
        /// </summary>
        public int Columns { get; private set; }

        /// <summary>
        /// Gets the cursor row, using the session's <see cref="X3270if.Config.Origin"/>.
        /// </summary>
        public int CursorRow { get; private set; }

        /// <summary>
        /// Gets the cursor column, using the session's <see cref="X3270if.Config.Origin"/>.
        /// </summary>
        public int CursorColumn { get; private set; }

        /// <summary>
        /// Gets or sets the screen contents, as an array indexed by 0-origin row and column.
        /// </summary>
        public DisplayPosition[,] ContentsArray { get; set; }

        /// <summary>
        /// The screen contents, one location.
        /// </summary>
        /// <param name="row">Row, using the defined <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Column, using the defined <see cref="X3270if.Config.Origin"/>.</param>
        /// <returns>Contents of one screen location.</returns>
        public DisplayPosition Contents(int row, int column)
        {
            return this.ContentsArray[row - this.Origin, column - this.Origin];
        }

        /// <summary>
        /// The screen contents, one location, indexed by Coordinates.
        /// </summary>
        /// <param name="c">Coordinate indices.</param>
        /// <returns>Contents of one screen location.</returns>
        public DisplayPosition Contents(Coordinates c)
        {
            return this.ContentsArray[c.Row - this.Origin, c.Column - this.Origin];
        }

        /// <summary>
        /// Write an ASCII ReadBuffer buffer out to the console, with proper formatting (if possible).
        /// </summary>
        public void DumpAsciiConsole()
        {
            if (this.ioResult.ReadBufferType != Session.ReadBufferType.Ascii)
            {
                throw new InvalidOperationException("ReadBuffer is not Ascii");
            }

            FieldColor fg = FieldColor.NeutralWhite;
            FieldColor bg = FieldColor.NeutralBlack;
            Console.ForegroundColor = this.ColorMap(fg, true);
            Console.BackgroundColor = this.ColorMap(bg, false);
            for (int row = 0; row < this.Rows; row++)
            {
                for (int column = 0; column < this.Columns; column++)
                {
                    var c = this.ContentsArray[row, column];
                    var a = c.Attrs;
                    if (a.Foreground != fg)
                    {
                        fg = a.Foreground;
                        Console.ForegroundColor = this.ColorMap(fg, true);
                    }

                    if (a.Background != bg)
                    {
                        bg = a.Background;
                        Console.BackgroundColor = this.ColorMap(bg, false);
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

        #region Ascii methods
        /// <summary>
        /// Translate a DisplayBuffer to a text string, starting at the cursor address.
        /// </summary>
        /// <param name="length">Length of field to return. Can wrap rows.</param>
        /// <returns>The text.</returns>
        /// <remarks>
        /// <note type="note">
        /// For DBCS text, this method may return data from more buffer positions than <paramref name="length"/>.
        /// It will attempt to return <paramref name="length"/> characters, skipping over the right-hand sides of DBCS characters
        /// as necessary.
        /// </note>
        /// </remarks>
        public string Ascii(int length)
        {
            return this.Ascii(this.CursorRow, this.CursorColumn, length);
        }

        /// <summary>
        /// Translate a <see cref="DisplayBuffer"/> to a text string, starting at the specified address.
        /// </summary>
        /// <param name="row">Starting row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="length">Length of field.</param>
        /// <returns>The text.</returns>
        /// <remarks>
        /// <note type="note">
        /// For DBCS text, this method may return data from more buffer positions than <paramref name="length"/>.
        /// It will attempt to return <paramref name="length"/> characters, skipping over the right-hand sides of DBCS characters
        /// as necessary.
        /// </note>
        /// </remarks>
        public string Ascii(int row, int column, int length)
        {
            if (this.ioResult.ReadBufferType != Session.ReadBufferType.Ascii)
            {
                throw new InvalidOperationException("ReadBuffer is not Ascii");
            }

            row -= this.Origin;
            column -= this.Origin;
            if (row < 0 || row >= this.Rows)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < 0 || column >= this.Columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            if (length < 0 || (row * this.Columns) + column + length > this.Rows * this.Columns)
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
            var lengthLeft = length;
            while (lengthLeft > 0)
            {
                var r = this.TranslatePosition(this.ContentsArray[curRow, curColumn]);
                if (r.HasValue)
                {
                    sb.Append((char)r);
                    lengthLeft--;
                }

                // Otherwise do not decrement lengthLeft.
                if (++curColumn >= this.Columns)
                {
                    curRow++;
                    curColumn = 0;
                }

                if (curRow == row && curColumn == column)
                {
                    // Wrapped without getting enough characters.
                    break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Translate a rectangular region of a DisplayBuffer to text.
        /// </summary>
        /// <param name="row">Starting row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        /// <returns>Array of strings, one entry per row.</returns>
        /// <remarks>
        /// <note type="note">
        /// For DBCS text, this method may return data from more buffer positions than <paramref name="columns"/>.
        /// It will attempt to return <paramref name="columns"/> characters, skipping over the right-hand sides of DBCS characters
        /// as necessary. It will not wrap across rows to do this.
        /// </note>
        /// </remarks>
        public string[] Ascii(int row, int column, int rows, int columns)
        {
            if (this.ioResult.ReadBufferType != Session.ReadBufferType.Ascii)
            {
                throw new InvalidOperationException("ReadBuffer is not Ascii");
            }

            row -= this.Origin;
            column -= this.Origin;
            if (row < 0 || row >= this.Rows)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < 0 || column >= this.Columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            if (rows <= 0 || row + rows > this.Rows)
            {
                throw new ArgumentOutOfRangeException("rows");
            }

            if (columns <= 0 || column + columns > this.Columns)
            {
                throw new ArgumentOutOfRangeException("columns");
            }

            string[] ret = new string[rows];
            for (var r = row; r < row + rows; r++)
            {
                StringBuilder sb = new StringBuilder();
                int columnsLeft = columns;
                int c = column;

                while (columnsLeft > 0)
                {
                    var rc = this.TranslatePosition(this.ContentsArray[r, c]);
                    if (rc.HasValue)
                    {
                        sb.Append((char)rc);
                        columnsLeft--;
                    }

                    if (++c >= this.Columns)
                    {
                        break;
                    }
                }

                ret[r - row] = sb.ToString();
            }

            return ret;
        }

        /// <summary>
        /// Translate an entire DisplayBuffer buffer to text.
        /// </summary>
        /// <returns>Text array.</returns>
        public string[] Ascii()
        {
            return this.Ascii(this.Origin, this.Origin, this.Rows, this.Columns);
        }

        /// <summary>
        /// Returns the length of the field that includes the specified
        /// coordinates.
        /// </summary>
        /// <param name="c">Coordinate indices.</param>
        /// <returns>Field length. The length does not include the field attribute itself, so it can be zero.</returns>
        /// <remarks>
        /// <note type="note">
        /// If the screen is unformatted, returns the size of the entire screen.
        /// </note>
        /// <note type="note">
        /// For DBCS text, returns the number of characters in the field, not the number of buffer positions in
        /// the field.
        /// </note>
        /// </remarks>
        public int FieldLength(Coordinates c)
        {
            // Work backwards until we find the field attribute, or wrap.
            // We count c (if it is not an FA) and each non-FA to the left of it.
            int count = 0;
            var d = c.Clone();
            while (this.Contents(d).Type != PositionType.FieldAttribute)
            {
                if (this.TranslatePosition(this.Contents(d)).HasValue)
                {
                    count++;
                }

                if (--d == c)
                {
                    // Wrapped back to the start. Unformatted screen.
                    return count;
                }
            }

            // Count forward from (c+1) until we find the next field attribute.
            d = c.Clone();
            while (this.Contents(++d).Type != PositionType.FieldAttribute)
            {
                if (this.TranslatePosition(this.Contents(d)).HasValue)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Returns the length of the field that includes the specified
        /// row and column location.
        /// </summary>
        /// <param name="row">Row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Column using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <returns>Field length. The length does not include the field attribute itself, so it can be zero.</returns>
        /// <remarks>
        /// <note type="note">
        /// If the screen is unformatted, returns the size of the entire screen.
        /// </note>
        /// <note type="note">
        /// For DBCS text, returns the number of characters in the field, not the number of buffer positions in
        /// the field.
        /// </note>
        /// </remarks>
        public int FieldLength(int row, int column)
        {
            return this.FieldLength(new Coordinates(this, row, column));
        }

        /// <summary>
        /// Return a field value in ASCII.
        /// </summary>
        /// <param name="c">Coordinate indices.</param>
        /// <returns>The text.</returns>
        public string AsciiField(Coordinates c = null)
        {
            if (c == null)
            {
                c = new Coordinates(this, this.CursorRow, this.CursorColumn);
            }

            // Find the field that contains this position.
            var faPosition = this.FaPosition(c);
            if (faPosition == null)
            {
                // Unformatted. One big blob.
                return this.Ascii(0, 0, this.Rows * this.Columns);
            }

            // Return just this field.
            var fieldLength = this.FieldLength(faPosition);
            faPosition++;
            return this.Ascii(faPosition.Row, faPosition.Column, fieldLength);
        }

        /// <summary>
        /// Return a field value in ASCII.
        /// </summary>
        /// <param name="row">Row number, using the object's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Column number, using the object's <see cref="X3270if.Config.Origin"/>.</param>
        /// <returns>The text.</returns>
        public string AsciiField(int row, int column)
        {
            return this.AsciiField(new Coordinates(this, row, column));
        }

        /// <summary>
        /// Convenience method for checking screen contents (exact match).
        /// </summary>
        /// <param name="row">Row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="text">Text to compare.</param>
        /// <returns>True if the DisplayBuffer at that location equals <paramref name="text"/>.</returns>
        public bool AsciiEquals(int row, int column, string text)
        {
            return this.Ascii(row, column, text.Length) == text;
        }

        /// <summary>
        /// Convenience method for checking screen contents (pattern match).
        /// </summary>
        /// <param name="row">Row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="length">Length of region.</param>
        /// <param name="regex">Regular expression to match against. (See <see cref="System.Text.RegularExpressions"/>.)</param>
        /// <returns>True if <paramref name="regex"/> matches the specified region of the DisplayBuffer.</returns>
        public bool AsciiMatches(int row, int column, int length, string regex)
        {
            return Regex.IsMatch(this.Ascii(row, column, length), regex);
        }
        #endregion

        /// <summary>
        /// Find the coordinates of the Field Attribute for a given screen position.
        /// </summary>
        /// <param name="c">Coordinate indices.</param>
        /// <returns>Row and column, or null if the screen is unformatted.</returns>
        private Coordinates FaPosition(Coordinates c)
        {
            Coordinates b = new Coordinates(c);
            do
            {
                if (this.Contents(b).Type == PositionType.FieldAttribute)
                {
                    return b;
                }

                b--;
            }
            while (b != c);

            // Unformatted screen.
            return null;
        }

        /// <summary>
        /// Parse a row of ReadBuffer fields.
        /// </summary>
        /// <param name="text">One line of output from <c>EbcdicField</c>.</param>
        /// <param name="row">Row index.</param>
        /// <param name="faAttrs">Current FA attributes.</param>
        /// <param name="saAttrs">Current SA attributes.</param>
        private void ParseRow(string text, int row, ref Attrs faAttrs, ref Attrs saAttrs)
        {
            string[] split = text.Split(' ');
            int column = 0;
            Attrs attrs = this.CombineAttrs(faAttrs, saAttrs);
            foreach (string s in split)
            {
                if (s.StartsWith("SA("))
                {
                    this.ParseSA(s, ref saAttrs);
                    attrs = this.CombineAttrs(faAttrs, saAttrs);
                    continue;
                }

                if (s == "-")
                {
                    this.ContentsArray[row, column] = new DisplayPosition
                    {
                        Type = PositionType.DbcsRight,
                        Attrs = attrs
                    };
                }
                else if (s.StartsWith("SF("))
                {
                    faAttrs = new Attrs();
                    this.ParseSA(s, ref faAttrs);
                    attrs = this.CombineAttrs(faAttrs, saAttrs);
                    this.ContentsArray[row, column] = new DisplayPosition
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
                    if (this.ioResult.ReadBufferType == Session.ReadBufferType.Ascii)
                    {
                        this.ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ascii,
                            AsciiChar = this.ParseHexEncoded(hexChar),
                            Attrs = geAttrs
                        };
                    }
                    else
                    {
                        this.ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ebcdic,
                            EbcdicChar = ushort.Parse(hexChar, System.Globalization.NumberStyles.HexNumber),
                            Attrs = geAttrs
                        };
                    }
                }
                else
                {
                    if (this.ioResult.ReadBufferType == Session.ReadBufferType.Ascii)
                    {
                        this.ContentsArray[row, column] = new DisplayPosition
                        {
                            Type = PositionType.Ascii,
                            AsciiChar = this.ParseHexEncoded(s),
                            Attrs = attrs
                        };
                    }
                    else
                    {
                        this.ContentsArray[row, column] = new DisplayPosition
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
        /// <param name="hexChar">Hexadecimal text.</param>
        /// <returns>Decoded character.</returns>
        private char ParseHexEncoded(string hexChar)
        {
            var bytes = new byte[hexChar.Length / 2];
            for (int i = 0; i < hexChar.Length; i += 2)
            {
                bytes[i / 2] = byte.Parse(hexChar.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return this.encoding.GetString(bytes)[0];
        }

        /// <summary>
        /// Helper function to parse a hexadecimal byte.
        /// </summary>
        /// <param name="value">Text to parse.</param>
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
        /// Parse an extended attribute enumeration in hex.
        /// If the value cannot be parsed or is unsupported, return the default for the type.
        /// </summary>
        /// <typeparam name="T">Enumeration type.</typeparam>
        /// <param name="value">Text to parse.</param>
        /// <param name="result">Value stored here, if valid.</param>
        private void ParseExtendedAttribute<T>(string value, ref T result) where T : struct, IConvertible
        {
            var i = this.TryParseHex(value);
            if (i.HasValue)
            {
                T resultValue = (T)(object)i.Value;
                if (Enum.IsDefined(typeof(T), resultValue))
                {
                    result = resultValue;
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
        /// <param name="field">SA(XX=YY) text.</param>
        /// <param name="attrs">Current/modified attributes.</param>
        private void ParseSA(string field, ref Attrs attrs)
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
                        attrs.Flags = (FieldFlags)(ea3270 & (byte)FieldFlags.All);
                        attrs.Intensity = (FieldIntensity)(ea3270 & IntensityMask);
                        break;
                    case ExtendedAttribute.Background:
                        var background = attrs.Background;
                        this.ParseExtendedAttribute<FieldColor>(expr[1], ref background);
                        attrs.Background = background;
                        break;
                    case ExtendedAttribute.Foreground:
                        var foreground = attrs.Foreground;
                        this.ParseExtendedAttribute<FieldColor>(expr[1], ref foreground);
                        attrs.Foreground = foreground;
                        break;
                    case ExtendedAttribute.CharacterSet:
                        var characterSet = attrs.CharacterSet;
                        this.ParseExtendedAttribute<CharacterSet>(expr[1], ref characterSet);
                        attrs.CharacterSet = characterSet;
                        break;
                    case ExtendedAttribute.Highlighting:
                        var highlighting = attrs.Highlighting;
                        this.ParseExtendedAttribute<Highlighting>(expr[1], ref highlighting);
                        attrs.Highlighting = highlighting;
                        break;
                    case ExtendedAttribute.Transparency:
                        var transparency = attrs.Transparency;
                        this.ParseExtendedAttribute<Transparency>(expr[1], ref transparency);
                        attrs.Transparency = transparency;
                        break;
                    case ExtendedAttribute.Outlining:
                        var outlining = attrs.Outlining;
                        this.ParseExtendedAttribute<Outlining>(expr[1], ref outlining);
                        attrs.Outlining = outlining;
                        break;
                    case ExtendedAttribute.InputControl:
                        var inputControl = attrs.InputControl;
                        this.ParseExtendedAttribute<InputControl>(expr[1], ref inputControl);
                        attrs.InputControl = inputControl;
                        break;
                    case ExtendedAttribute.Validation:
                        var validation = attrs.Validation;
                        this.ParseExtendedAttribute<Validation>(expr[1], ref validation);
                        attrs.Validation = validation;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Return sa if non-default, else fa.
        /// </summary>
        /// <typeparam name="T">Enumeration type that defines Default.</typeparam>
        /// <param name="fa">Other value.</param>
        /// <param name="sa">Value that takes precedence.</param>
        /// <returns><paramref name="sa"/> if <paramref name="sa"/> is non-default, else <paramref name="fa"/>.</returns>
        private T SetNonDefault<T>(T fa, T sa) where T : IComparable
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
        /// <param name="faAttrs">Default field attributes.</param>
        /// <param name="saAttrs">Override attributes, if non-default.</param>
        /// <returns>Merged attributes.</returns>
        private Attrs CombineAttrs(Attrs faAttrs, Attrs saAttrs)
        {
            Attrs resultAttrs = new Attrs();

            resultAttrs.Intensity = faAttrs.Intensity;
            resultAttrs.Flags = faAttrs.Flags;

            resultAttrs.Foreground = this.SetNonDefault<FieldColor>(faAttrs.Foreground, saAttrs.Foreground);
            resultAttrs.Background = this.SetNonDefault<FieldColor>(faAttrs.Background, saAttrs.Background);
            resultAttrs.CharacterSet = this.SetNonDefault<CharacterSet>(faAttrs.CharacterSet, saAttrs.CharacterSet);
            resultAttrs.Highlighting = this.SetNonDefault<Highlighting>(faAttrs.Highlighting, saAttrs.Highlighting);
            resultAttrs.Transparency = this.SetNonDefault<Transparency>(faAttrs.Transparency, saAttrs.Transparency);
            resultAttrs.Outlining = this.SetNonDefault<Outlining>(faAttrs.Outlining, saAttrs.Outlining);
            resultAttrs.InputControl = this.SetNonDefault<InputControl>(faAttrs.InputControl, saAttrs.InputControl);
            resultAttrs.Validation = this.SetNonDefault<Validation>(faAttrs.Validation, saAttrs.Validation);
                
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
        /// Translate one screen position to a character.
        /// </summary>
        /// <param name="c">Display position to translate.</param>
        /// <returns>Character, or null if position contains a hole.</returns>
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
    }
}
