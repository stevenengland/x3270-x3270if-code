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

namespace X3270if.Attributes
{
    using System;

    /// <summary>
    /// 3270 intensity enumeration. Used by the <see cref="DisplayBuffer"/> class.
    /// <para>These come from a Start Field order or the 3270 attributes of an extended field.</para>
    /// </summary>
    public enum FieldIntensity : byte
    {
        /// <summary>
        /// Normal intensity, not light pen selectable.
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// Normal, light pen selectable.
        /// </summary>
        NormalSelectable = 0x04,

        /// <summary>
        /// Highlighted, light pen selectable.
        /// </summary>
        HighlightedSelectable = 0x08,

        /// <summary>
        /// Invisible (a password, e.g.).
        /// </summary>
        Zero = 0x0c
    }

    /// <summary>
    /// Miscellaneous field attribute flags. Used by the <see cref="DisplayBuffer"/> class.
    /// <para>These come from a Start Field order or the 3270 attributes of an extended field.</para>
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
        /// Field has been modified.
        /// </summary>
        Modified = 0x01,

        /// <summary>
        /// All possible flags.
        /// </summary>
        All = Protected | Numeric | Modified
    }

    /// <summary>
    /// A foreground or background color.
    /// Used by <see cref="ExtendedAttribute.Foreground"/> and <see cref="ExtendedAttribute.Background"/>
    /// in the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum FieldColor : byte
    {
        /// <summary>
        /// Default color.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Neutral black (black on a screen, white on a printer).
        /// </summary>
        NeutralBlack = 0xf0,

        /// <summary>
        /// Blue color.
        /// </summary>
        Blue = 0xf1,

        /// <summary>
        /// Red color.
        /// </summary>
        Red = 0xf2,

        /// <summary>
        /// Pink color.
        /// </summary>
        Pink = 0xf3,

        /// <summary>
        /// Green color.
        /// </summary>
        Green = 0xf4,

        /// <summary>
        /// Turquoise color.
        /// </summary>
        Turquoise = 0xf5,

        /// <summary>
        /// Yellow color.
        /// </summary>
        Yellow = 0xf6,

        /// <summary>
        /// Neutral white (white on a screen, black on a printer).
        /// </summary>
        NeutralWhite = 0xf7,

        /// <summary>
        /// Black color.
        /// </summary>
        Black = 0xf8,

        /// <summary>
        /// Deep blue.
        /// </summary>
        DeepBlue = 0xf9,

        /// <summary>
        /// Orange color.
        /// </summary>
        Orange = 0xfa,

        /// <summary>
        /// Purple color.
        /// </summary>
        Purple = 0xfb,

        /// <summary>
        /// Pale green.
        /// </summary>
        PaleGreen = 0xfc,

        /// <summary>
        /// Pale turquoise.
        /// </summary>
        PaleTurquoise = 0xfd,

        /// <summary>
        /// Gray color.
        /// </summary>
        Gray = 0xfe,

        /// <summary>
        /// White color.
        /// </summary>
        White = 0xff
    }

    /// <summary>
    /// An extended attribute. Used by the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum ExtendedAttribute : byte
    {
        /// <summary>
        /// Standard 3270 field attributes (see <see cref="FieldIntensity"/> and <see cref="FieldFlags"/>).
        /// </summary>
        Ea3270 = 0xc0,

        /// <summary>
        /// Field validation (see <see cref="Validation"/>).
        /// </summary>
        Validation = 0xc1,

        /// <summary>
        /// Field outlining (see <see cref="Outlining"/>).
        /// </summary>
        Outlining = 0xc2,

        /// <summary>
        /// Field highlighting (see <see cref="Highlighting"/>).
        /// </summary>
        Highlighting = 0x41,

        /// <summary>
        /// Foreground color (see <see cref="FieldColor"/>).
        /// </summary>
        Foreground = 0x42,

        /// <summary>
        /// Character set (see <see cref="CharacterSet"/>).
        /// </summary>
        CharacterSet = 0x43,

        /// <summary>
        /// Background color (see <see cref="FieldColor"/>).
        /// </summary>
        Background = 0x45,

        /// <summary>
        /// Field transparency (see <see cref="Transparency"/>).
        /// </summary>
        Transparency = 0x46,

        /// <summary>
        /// Input control enable (see <see cref="InputControl"/>).
        /// </summary>
        InputControl = 0xfe
    }

    /// <summary>
    /// A character set. Used by <see cref="ExtendedAttribute.CharacterSet"/> in the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum CharacterSet : byte
    {
        /// <summary>
        /// Default character set.
        /// </summary>
        Default = 0,

        /// <summary>
        /// APL and line drawing.
        /// </summary>
        Apl = 0xf1,

        /// <summary>
        /// DEC line drawing (an x3270 NVT-mode extension).
        /// </summary>
        LineDrawing = 0xf2,

        /// <summary>
        /// Double-byte character set.
        /// </summary>
        Dbcs = 0xf8
    }

    /// <summary>
    /// The Validation extended attribute. Used by <see cref="ExtendedAttribute.Validation"/> in the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum Validation : byte
    {
        /// <summary>
        /// Default validation (none).
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
    /// The Outlining extended attribute (ORed together). Used by <see cref="ExtendedAttribute.Outlining"/> in the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum Outlining : byte
    {
        /// <summary>
        /// Default outlining (none).
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
    /// The Highlighting extended attribute. Used by <see cref="ExtendedAttribute.Highlighting"/> in the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum Highlighting : byte
    {
        /// <summary>
        /// Default (none).
        /// </summary>
        Default = 0,

        /// <summary>
        /// Normal intensity.
        /// </summary>
        Normal = 0xf0,

        /// <summary>
        /// Blinking text.
        /// </summary>
        Blink = 0xf1,

        /// <summary>
        /// Reverse foreground and background colors.
        /// </summary>
        Reverse = 0xf2,

        /// <summary>
        /// Underlined text.
        /// </summary>
        Underscore = 0xf4,

        /// <summary>
        /// Intensified text.
        /// </summary>
        Intensify = 0xf8
    }

    /// <summary>
    /// The Transparency extended attribute. Used by <see cref="ExtendedAttribute.Transparency"/> in the <see cref="DisplayBuffer"/> class.
    /// </summary>
    public enum Transparency : byte
    {
        /// <summary>
        /// Default transparency.
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
    /// The InputControl extended attribute. Used by <see cref="ExtendedAttribute.InputControl"/> in the <see cref="DisplayBuffer"/> class.
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
}
