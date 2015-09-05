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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace x3270if
{
    // From ft.c:
    //{ "Direction",      NULL, { "receive", "send" } },
    //{ "HostFile" },
    //{ "LocalFile" },
    //{ "Host",           NULL, { "tso", "vm", "cics" } },
    //{ "Mode",           NULL, { "ascii", "binary" } },
    //{ "Cr",             NULL, { "auto", "remove",       "add", "keep" } },
    //{ "Remap",          NULL, { "yes", "no" } },
    //{ "Exist",          NULL, { "keep", "replace", "append" } },
    //{ "Recfm",          NULL, { "default", "fixed", "variable", "undefined" } },
    //{ "Lrecl" },
    //{ "Blksize" },
    //{ "Allocation",     NULL, { "default", "tracks", "cylinders", "avblock" } },
    //{ "PrimarySpace" },
    //{ "SecondarySpace" },
    //{ "BufferSize" },
    //{ "Avblock" },
    //{ "WindowsCodePage" },

    /// <summary>
    /// Transfer additional parameter types.
    /// </summary>
    public enum TransferParameterType
    {
        /// <summary>
        /// Add or remove CRs in an Ascii transfer.
        /// </summary>
        AsciiCr,
        /// <summary>
        /// Remap the character set in an Ascii transfer.
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
    };

    /// <summary>
    /// File transfer parameter.
    /// </summary>
    public class TransferParameter
    {
        /// <summary>
        /// Type of parameter.
        /// </summary>
        public TransferParameterType Type;
    }

    /// <summary>
    /// ASCII transfer CR add/remove parameter.
    /// </summary>
    public class TransferParameterAsciiCr : TransferParameter
    {
        /// <summary>
        /// Add/remove flag.
        /// </summary>
        public bool AddRemove;

        /// <summary>
        /// Constructor for AsciiCr parameter.
        /// </summary>
        /// <param name="addRemove">Add/remove flag.</param>
        public TransferParameterAsciiCr(bool addRemove)
        {
            Type = TransferParameterType.AsciiCr;
            AddRemove = addRemove;
        }
    }

    /// <summary>
    /// ASCII character set remap parameter.
    /// </summary>
    public class TransferParameterAsciiRemap : TransferParameter
    {
        /// <summary>
        /// Remap flag.
        /// </summary>
        public bool Remap;
        /// <summary>
        /// Optional code page (if Remap is true).
        /// </summary>
        public uint? CodePage;

        /// <summary>
        /// Constructor for remap parameter.
        /// </summary>
        /// <param name="remap">Remap flag.</param>
        /// <param name="codePage">Optional code page, if remap is true.</param>
        public TransferParameterAsciiRemap(bool remap, uint? codePage = null)
        {
            Type = TransferParameterType.AsciiRemap;
            Remap = remap;
            if (!remap && codePage.HasValue)
            {
                throw new ArgumentException("codePage requies remap");
            }
            if (codePage == 0)
            {
                throw new ArgumentOutOfRangeException("codePage");
            }
            CodePage = codePage;
        }
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
    /// Exist action parameter.
    /// </summary>
    public class TransferParameterExistAction : TransferParameter
    {
        /// <summary>
        /// Exist action.
        /// </summary>
        public ExistAction ExistAction;

        /// <summary>
        /// Constructor for exist action.
        /// </summary>
        /// <param name="existAction">Exist action.</param>
        public TransferParameterExistAction(ExistAction existAction)
        {
            Type = TransferParameterType.ExistAction;
            ExistAction = existAction;
        }
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
    /// Host record length format parameter.
    /// </summary>
    public class TransferParameterSendRecordFormat : TransferParameter
    {
        /// <summary>
        /// Record format.
        /// </summary>
        public RecordFormat RecordFormat;

        /// <summary>
        /// Constructor for host record format parameter.
        /// </summary>
        /// <param name="recordFormat">Record format.</param>
        public TransferParameterSendRecordFormat(RecordFormat recordFormat)
        {
            Type = TransferParameterType.SendRecordFormat;
            RecordFormat = recordFormat;
        }
    }

    /// <summary>
    /// Host logical record length parameter.
    /// </summary>
    public class TransferParameterSendLogicalRecordLength : TransferParameter
    {
        /// <summary>
        /// Record length.
        /// </summary>
        public uint RecordLength;

        /// <summary>
        /// Construtor for host logical record length parameter.
        /// </summary>
        /// <param name="recordLength">Record length.</param>
        public TransferParameterSendLogicalRecordLength(uint recordLength)
        {
            if (recordLength == 0)
            {
                throw new ArgumentOutOfRangeException("recordLength");
            }
            Type = TransferParameterType.SendLogicalRecordLength;
            RecordLength = recordLength;
        }
    }

    /// <summary>
    /// Block size parameter.
    /// </summary>
    public class TransferParameterBlockSize : TransferParameter
    {
        /// <summary>
        /// Block size.
        /// </summary>
        public uint BlockSize;

        /// <summary>
        /// Constructor for block size parameter.
        /// </summary>
        /// <param name="blockSize">Block size.</param>
        public TransferParameterBlockSize(uint blockSize)
        {
            if (blockSize == 0)
            {
                throw new ArgumentOutOfRangeException("blockSize");
            }
            Type = TransferParameterType.BlockSize;
            BlockSize = blockSize;
        }
    }

    /// <summary>
    /// TSO file allocation units.
    /// </summary>
    public enum TsoAllocationUnits
    {
        /// <summary>
        /// Tracks.
        /// </summary>
        Tracks,
        /// <summary>
        /// Cylinders.
        /// </summary>
        Cylinders,
        /// <summary>
        /// Avblocks.
        /// </summary>
        Avblock
    }

    /// <summary>
    /// TSO file allocation parameter.
    /// </summary>
    public class TransferParameterTsoSendAllocation : TransferParameter
    {
        /// <summary>
        /// Allocation units.
        /// </summary>
        public TsoAllocationUnits AllocationUnits;
        /// <summary>
        /// Primary space.
        /// </summary>
        public uint PrimarySpace;
        /// <summary>
        /// Secondary space.
        /// </summary>
        public uint? SecondarySpace;
        /// <summary>
        /// Number of bytes in an avblock.
        /// </summary>
        public uint? Avblock;

        /// <summary>
        /// Constructor for TSO file allocation parameter.
        /// </summary>
        /// <param name="allocationUnits">Allocation units.</param>
        /// <param name="primarySpace">Primary space.</param>
        /// <param name="secondarySpace">Secondary space.</param>
        /// <param name="avblock">Bytes per avblock</param>
        public TransferParameterTsoSendAllocation(
            TsoAllocationUnits allocationUnits,
            uint primarySpace,
            uint? secondarySpace = null,
            uint? avblock = null)
        {
            // The way to create an avblock allocation without secondary space is:
            //  var x = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, avblock: 200);

            Type = TransferParameterType.TsoSendAllocation;

            AllocationUnits = allocationUnits;
            if (primarySpace == 0)
            {
                throw new ArgumentOutOfRangeException("primarySpace");
            }
            PrimarySpace = primarySpace;
            if (secondarySpace.HasValue && secondarySpace.Value == 0)
            {
                throw new ArgumentOutOfRangeException("secondarySpace");
            }
            SecondarySpace = secondarySpace;
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
                Avblock = avblock;
            }
            else
            {
                if (avblock.HasValue)
                {
                    throw new ArgumentException("avblock is prohibited");
                }
            }
        }
    }

    /// <summary>
    /// Transfer parameter for buffer size.
    /// </summary>
    public class TransferParameterBufferSize : TransferParameter
    {
        /// <summary>
        /// Buffer size.
        /// </summary>
        public uint BufferSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bufferSize">Buffer size.</param>
        public TransferParameterBufferSize(uint bufferSize)
        {
            Type = TransferParameterType.BufferSize;
            if (bufferSize == 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            BufferSize = bufferSize;
        }
    }

    /// <summary>
    /// Transfer modes.
    /// </summary>
    public enum TransferMode
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
    public enum TransferDirection
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
    public enum TransferHostType
    {
        /// <summary>
        /// TSO (MVS).
        /// </summary>
        Tso,
        /// <summary>
        /// VM/CMS.
        /// </summary>
        Vm,
        /// <summary>
        /// CICS.
        /// </summary>
        Cics
    }

    public partial class Session
    {
        /// <summary>
        /// Asynchronous version of Transfer().
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary)</param>
        /// <param name="hostType">Host type (TSO/VM/CICS)</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>Success/failure and failure text</returns>
        public async Task<IoResult> TransferAsync(
            string localFile,
            string hostFile,
            TransferDirection direction,
            TransferMode mode,
            TransferHostType hostType,
            IEnumerable<TransferParameter> parameters)
        {
            // Check file names.
            if (string.IsNullOrEmpty(localFile))
            {
                throw new ArgumentException("localFile");
            }
            if (string.IsNullOrEmpty(hostFile))
            {
                throw new ArgumentException("hostFile");
            }

            // Build up the fixed arguments.
            var argd = new SortedDictionary<string, string>();
            argd["localfile"] = localFile;
            argd["hostfile"] = hostFile;
            argd["direction"] = direction.ToString();
            argd["mode"] = mode.ToString();
            argd["host"] = hostType.ToString();

            // Walk through the parameters and build up the optional agruments.
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    switch (p.Type)
                    {
                        case TransferParameterType.AsciiCr:
                            if (mode != TransferMode.Ascii)
                            {
                                throw new ArgumentException("AsciiCr requires Ascii mode");
                            }
                            argd["cr"] = ((TransferParameterAsciiCr)p).AddRemove ? "add" : "keep";
                            break;
                        case TransferParameterType.AsciiRemap:
                            if (mode != TransferMode.Ascii)
                            {
                                throw new ArgumentException("AsciiRemap requires Ascii mode");
                            }
                            var remap = (TransferParameterAsciiRemap)p;
                            argd["remap"] = remap.Remap ? "yes" : "no";
                            if (remap.CodePage.HasValue)
                            {
                                argd["windowscodepage"] = remap.CodePage.ToString();
                            }
                            break;
                        case TransferParameterType.BlockSize:
                            if (hostType != TransferHostType.Tso)
                            {
                                throw new ArgumentException("BlockSize only works on TSO hosts");
                            }
                            argd["blocksize"] = ((TransferParameterBlockSize)p).BlockSize.ToString();
                            break;
                        case TransferParameterType.ExistAction:
                            argd["exist"] = ((TransferParameterExistAction)p).ExistAction.ToString();
                            break;
                        case TransferParameterType.SendLogicalRecordLength:
                            if (direction != TransferDirection.Send)
                            {
                                throw new ArgumentException("SendLogicalRecordLength requires send");
                            }
                            if (hostType == TransferHostType.Cics)
                            {
                                throw new ArgumentException("SendLogicalRecordLength does not work on CICS");
                            }
                            argd["lrecl"] = ((TransferParameterSendLogicalRecordLength)p).RecordLength.ToString();
                            break;
                        case TransferParameterType.SendRecordFormat:
                            if (direction != TransferDirection.Send)
                            {
                                throw new ArgumentException("SendRecordFormat requires send");
                            }
                            if (hostType == TransferHostType.Cics)
                            {
                                throw new ArgumentException("SendRecordFormat does not work with CICS hosts");
                            }
                            argd["recfm"] = ((TransferParameterSendRecordFormat)p).RecordFormat.ToString();
                            break;
                        case TransferParameterType.TsoSendAllocation:
                            if (direction != TransferDirection.Send)
                            {
                                throw new ArgumentException("SendTsoAllocation requires send");
                            }
                            if (hostType != TransferHostType.Tso)
                            {
                                throw new ArgumentException("SendTsoAllocation requires Tso host");
                            }
                            var alloc = (TransferParameterTsoSendAllocation)p;
                            argd["allocation"] = alloc.AllocationUnits.ToString();
                            argd["primaryspace"] = alloc.PrimarySpace.ToString();
                            if (alloc.SecondarySpace.HasValue)
                            {
                                argd["secondaryspace"] = alloc.SecondarySpace.Value.ToString();
                            }
                            if (alloc.AllocationUnits == TsoAllocationUnits.Avblock)
                            {
                                argd["avblock"] = alloc.Avblock.ToString();
                            }
                            break;
                        case TransferParameterType.BufferSize:
                            argd["buffersize"] = ((TransferParameterBufferSize)p).BufferSize.ToString();
                            break;
                    }
                }
            }

            // Check for parameter interference (that's why the keywords went into a Dictionary).
            string v;
            if (direction == TransferDirection.Send &&
                argd.TryGetValue("exist", out v) &&
                v == ExistAction.Append.ToString() &&
                (argd.TryGetValue("recfm", out v) ||
                 argd.TryGetValue("lrecl", out v) ||
                 argd.TryGetValue("allocation", out v)))
            {
                throw new ArgumentException("Host file creation properties do not work with append");
            }

            // Join the dictionary of keywords and values together, quoting each argument as necessary.
            var arge = argd.Select(kv => QuoteString(kv.Key + "=" + kv.Value, true));
            var result = await IoAsync("Transfer(" + string.Join(",", arge) + ")")
                .ConfigureAwait(continueOnCapturedContext: false);
            return result;
        }

        /// <summary>
        /// Asynchronous version of Transfer().
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary)</param>
        /// <param name="hostType">Host type (TSO/VM/CICS)</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>Success/failure and failure text</returns>
        public async Task<IoResult> TransferAsync(
            string localFile,
            string hostFile,
            TransferDirection direction,
            TransferMode mode,
            TransferHostType hostType,
            params object[] parameters)
        {
            var pList = new List<TransferParameter>();

            // Validate the parameter types.
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    throw new ArgumentNullException("parameter");
                }
                var p = parameters[i] as TransferParameter;
                if (p == null)
                {
                    throw new ArgumentException("parameter");
                }
                pList.Add(p);
            }

            return await TransferAsync(localFile, hostFile, direction, mode, hostType, pList)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Run the Transfer() action.
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary)</param>
        /// <param name="hostType">Host type (TSO/VM/CICS)</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>Success/failure and failure text</returns>
        public IoResult Transfer(
            string localFile,
            string hostFile,
            TransferDirection direction,
            TransferMode mode,
            TransferHostType hostType,
            IEnumerable<TransferParameter> parameters)
        {
            try
            {
                return TransferAsync(localFile, hostFile, direction, mode, hostType, parameters).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Run the Transfer() action.
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary)</param>
        /// <param name="hostType">Host type (TSO/VM/CICS)</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>Success/failure and failure text</returns>
        public IoResult Transfer(
            string localFile,
            string hostFile,
            TransferDirection direction,
            TransferMode mode,
            TransferHostType hostType,
            params object[] parameters)
        {
            try
            {
                return TransferAsync(localFile, hostFile, direction, mode, hostType, parameters).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
