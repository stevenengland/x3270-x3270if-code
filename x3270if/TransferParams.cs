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

namespace X3270if.Transfer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using X3270if;

    /// <summary>
    /// Transfer additional parameter types.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Add or remove carriage returns in an ASCII transfer.
        /// </summary>
        AsciiCr,

        /// <summary>
        /// Remap the character set in an ASCII transfer.
        /// </summary>
        AsciiRemap,

        /// <summary>
        /// Specify the action when the destination file exists.
        /// </summary>
        ExistAction,

        /// <summary>
        /// Specify the record format for the host file.
        /// </summary>
        SendRecordFormat,

        /// <summary>
        /// Specify the record length for the host file.
        /// </summary>
        SendLogicalRecordLength,

        /// <summary>
        /// Specify the host file block size.
        /// </summary>
        BlockSize,

        /// <summary>
        /// Specify the file allocation for TSO.
        /// </summary>
        TsoSendAllocation,

        /// <summary>
        /// Specify the buffer size for the transfer.
        /// </summary>
        BufferSize
    }

    /// <summary>
    /// Actions to take when the target file exists.
    /// </summary>
    public enum ExistAction
    {
        /// <summary>
        /// Replace the file.
        /// </summary>
        Replace,

        /// <summary>
        /// Keep the file and abort the transfer.
        /// </summary>
        Keep,

        /// <summary>
        /// Append to the file.
        /// </summary>
        Append
    }

    /// <summary>
    /// Host record formats.
    /// </summary>
    public enum RecordFormat
    {
        /// <summary>
        /// Fixed-length records.
        /// </summary>
        Fixed,

        /// <summary>
        /// Variable-length records.
        /// </summary>
        Variable,

        /// <summary>
        /// Undefined-length records.
        /// </summary>
        Undefined
    }

    /// <summary>
    /// TSO file allocation units.
    /// </summary>
    public enum TsoAllocationUnits
    {
        /// <summary>
        /// Allocate tracks.
        /// </summary>
        Tracks,

        /// <summary>
        /// Allocate cylinders.
        /// </summary>
        Cylinders,

        /// <summary>
        /// Allocate <c>Avblocks</c>.
        /// </summary>
        Avblock
    }

    /// <summary>
    /// Transfer modes.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Text file, translated between EBCDIC and ASCII.
        /// </summary>
        Ascii,

        /// <summary>
        /// Binary file, untranslated.
        /// </summary>
        Binary
    }

    /// <summary>
    /// Transfer direction.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Send file to host.
        /// </summary>
        Send,

        /// <summary>
        /// Receive file from host.
        /// </summary>
        Receive
    }

    /// <summary>
    /// Transfer host type.
    /// </summary>
    public enum HostType
    {
        /// <summary>
        /// TSO (MVS) host.
        /// </summary>
        Tso,

        /// <summary>
        /// VM/CMS host.
        /// </summary>
        Vm,

        /// <summary>
        /// CICS host.
        /// </summary>
        Cics
    }

    /// <summary>
    /// File transfer parameter.
    /// <para>This is an abstract base class used to derive specific parameter types.</para>
    /// </summary>
    public abstract class Parameter
    {
        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        public ParameterType Type { get; set; }
    }

    /// <summary>
    /// ASCII transfer CR add/remove parameter.
    /// </summary>
    public class ParameterAsciiCr : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAsciiCr"/> class.
        /// </summary>
        /// <param name="addRemove">Add/remove flag.</param>
        public ParameterAsciiCr(bool addRemove)
        {
            this.Type = ParameterType.AsciiCr;
            this.AddRemove = addRemove;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to add or remove carriage returns.
        /// If true, add or remove CRs when transferring text files.
        /// </summary>
        public bool AddRemove { get; set; }
    }

    /// <summary>
    /// ASCII character set remap parameter.
    /// </summary>
    public class ParameterAsciiRemap : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAsciiRemap"/> class.
        /// </summary>
        /// <param name="remap">Remap flag.</param>
        /// <param name="codePage">Optional code page, if remap is true.</param>
        public ParameterAsciiRemap(bool remap, uint? codePage = null)
        {
            this.Type = ParameterType.AsciiRemap;
            this.Remap = remap;
            if (!remap && codePage.HasValue)
            {
                throw new ArgumentException("codePage requies remap");
            }

            if (codePage == 0)
            {
                throw new ArgumentOutOfRangeException("codePage");
            }

            this.CodePage = codePage;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to remap the host code page.
        /// </summary>
        public bool Remap { get; set; }

        /// <summary>
        /// Gets or sets the optional code page (if Remap is true).
        /// </summary>
        public uint? CodePage { get; set; }
    }

    /// <summary>
    /// Exist action parameter.
    /// </summary>
    public class ParameterExistAction : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterExistAction"/> class.
        /// </summary>
        /// <param name="existAction">Exist action.</param>
        public ParameterExistAction(ExistAction existAction)
        {
            this.Type = ParameterType.ExistAction;
            this.ExistAction = existAction;
        }

        /// <summary>
        /// Gets or sets the exist action (what to do if the destination file already exists).
        /// </summary>
        public ExistAction ExistAction { get; set; }
    }

    /// <summary>
    /// Host record length format parameter.
    /// </summary>
    public class ParameterSendRecordFormat : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSendRecordFormat"/> class.
        /// </summary>
        /// <param name="recordFormat">Record format.</param>
        public ParameterSendRecordFormat(RecordFormat recordFormat)
        {
            this.Type = ParameterType.SendRecordFormat;
            this.RecordFormat = recordFormat;
        }

        /// <summary>
        /// Gets or sets the record format.
        /// </summary>
        public RecordFormat RecordFormat { get; set; }
    }

    /// <summary>
    /// Host logical record length parameter.
    /// </summary>
    public class ParameterSendLogicalRecordLength : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSendLogicalRecordLength"/> class.
        /// </summary>
        /// <param name="recordLength">Record length.</param>
        public ParameterSendLogicalRecordLength(uint recordLength)
        {
            if (recordLength == 0)
            {
                throw new ArgumentOutOfRangeException("recordLength");
            }

            this.Type = ParameterType.SendLogicalRecordLength;
            this.RecordLength = recordLength;
        }

        /// <summary>
        /// Gets or sets the host file logical record length.
        /// </summary>
        public uint RecordLength { get; set; }
    }

    /// <summary>
    /// Block size parameter.
    /// </summary>
    public class ParameterBlockSize : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBlockSize"/> class.
        /// </summary>
        /// <param name="blockSize">Block size.</param>
        public ParameterBlockSize(uint blockSize)
        {
            if (blockSize == 0)
            {
                throw new ArgumentOutOfRangeException("blockSize");
            }

            this.Type = ParameterType.BlockSize;
            this.BlockSize = blockSize;
        }

        /// <summary>
        /// Gets or sets the host file block size.
        /// </summary>
        public uint BlockSize { get; set; }
    }

    /// <summary>
    /// TSO file allocation parameter.
    /// </summary>
    public class ParameterTsoSendAllocation : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTsoSendAllocation"/> class.
        /// </summary>
        /// <param name="allocationUnits">Allocation units.</param>
        /// <param name="primarySpace">Primary space.</param>
        /// <param name="secondarySpace">Secondary space.</param>
        /// <param name="avblock">Bytes per <c>avblock</c>.</param>
        public ParameterTsoSendAllocation(
            TsoAllocationUnits allocationUnits,
            uint primarySpace,
            uint? secondarySpace = null,
            uint? avblock = null)
        {
            // The way to create an avblock allocation without secondary space is:
            //  var x = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, avblock: 200);
            this.Type = ParameterType.TsoSendAllocation;

            this.AllocationUnits = allocationUnits;
            if (primarySpace == 0)
            {
                throw new ArgumentOutOfRangeException("primarySpace");
            }

            this.PrimarySpace = primarySpace;
            if (secondarySpace.HasValue && secondarySpace.Value == 0)
            {
                throw new ArgumentOutOfRangeException("secondarySpace");
            }

            this.SecondarySpace = secondarySpace;
            if (allocationUnits == TsoAllocationUnits.Avblock)
            {
                if (!avblock.HasValue)
                {
                    throw new ArgumentException("avblock is required");
                }

                if (avblock.Value == 0)
                {
                    throw new ArgumentOutOfRangeException("avblock");
                }

                this.Avblock = avblock;
            }
            else
            {
                if (avblock.HasValue)
                {
                    throw new ArgumentException("avblock is prohibited");
                }
            }
        }

        /// <summary>
        /// Gets or sets the TSO file allocation units.
        /// </summary>
        public TsoAllocationUnits AllocationUnits { get; set; }

        /// <summary>
        /// Gets or sets the TSO host primary space.
        /// </summary>
        public uint PrimarySpace { get; set; }

        /// <summary>
        /// Gets or sets the TSO host secondary space.
        /// </summary>
        public uint? SecondarySpace { get; set; }

        /// <summary>
        /// Gets or sets the <c>avblock</c> size.
        /// </summary>
        public uint? Avblock { get; set; }
    }

    /// <summary>
    /// Transfer parameter for buffer size.
    /// </summary>
    public class ParameterBufferSize : Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBufferSize"/> class.
        /// </summary>
        /// <param name="bufferSize">Buffer size.</param>
        public ParameterBufferSize(uint bufferSize)
        {
            this.Type = ParameterType.BufferSize;
            if (bufferSize == 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.BufferSize = bufferSize;
        }

        /// <summary>
        /// Gets or sets the buffer size (maximum I/O transfer size).
        /// </summary>
        public uint BufferSize { get; set; }
    }
}