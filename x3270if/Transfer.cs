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

namespace X3270if
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using X3270if.Transfer;

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// IND$FILE file transfer, asynchronous version.
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary).</param>
        /// <param name="hostType">Host type (TSO/VM/CICS).</param>
        /// <param name="parameters">Additional parameters.</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> TransferAsync(
            string localFile,
            string hostFile,
            Direction direction,
            Mode mode,
            HostType hostType,
            IEnumerable<Parameter> parameters)
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
                        case ParameterType.AsciiCr:
                            if (mode != Mode.Ascii)
                            {
                                throw new ArgumentException("AsciiCr requires Ascii mode");
                            }

                            argd["cr"] = ((ParameterAsciiCr)p).AddRemove ? "add" : "keep";
                            break;
                        case ParameterType.AsciiRemap:
                            if (mode != Mode.Ascii)
                            {
                                throw new ArgumentException("AsciiRemap requires Ascii mode");
                            }

                            var remap = (ParameterAsciiRemap)p;
                            argd["remap"] = remap.Remap ? "yes" : "no";
                            if (remap.CodePage.HasValue)
                            {
                                argd["windowscodepage"] = remap.CodePage.ToString();
                            }

                            break;
                        case ParameterType.BlockSize:
                            if (hostType != HostType.Tso)
                            {
                                throw new ArgumentException("BlockSize only works on TSO hosts");
                            }

                            argd["blocksize"] = ((ParameterBlockSize)p).BlockSize.ToString();
                            break;
                        case ParameterType.ExistAction:
                            argd["exist"] = ((ParameterExistAction)p).ExistAction.ToString();
                            break;
                        case ParameterType.SendLogicalRecordLength:
                            if (direction != Direction.Send)
                            {
                                throw new ArgumentException("SendLogicalRecordLength requires send");
                            }

                            if (hostType == HostType.Cics)
                            {
                                throw new ArgumentException("SendLogicalRecordLength does not work on CICS");
                            }

                            argd["lrecl"] = ((ParameterSendLogicalRecordLength)p).RecordLength.ToString();
                            break;
                        case ParameterType.SendRecordFormat:
                            if (direction != Direction.Send)
                            {
                                throw new ArgumentException("SendRecordFormat requires send");
                            }

                            if (hostType == HostType.Cics)
                            {
                                throw new ArgumentException("SendRecordFormat does not work with CICS hosts");
                            }

                            argd["recfm"] = ((ParameterSendRecordFormat)p).RecordFormat.ToString();
                            break;
                        case ParameterType.TsoSendAllocation:
                            if (direction != Direction.Send)
                            {
                                throw new ArgumentException("SendTsoAllocation requires send");
                            }

                            if (hostType != HostType.Tso)
                            {
                                throw new ArgumentException("SendTsoAllocation requires Tso host");
                            }

                            var alloc = (ParameterTsoSendAllocation)p;
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
                        case ParameterType.BufferSize:
                            argd["buffersize"] = ((ParameterBufferSize)p).BufferSize.ToString();
                            break;
                    }
                }
            }

            // Check for parameter interference (that's why the keywords went into a Dictionary).
            string v;
            if (direction == Direction.Send &&
                argd.TryGetValue("exist", out v) &&
                v == ExistAction.Append.ToString() &&
                (argd.TryGetValue("recfm", out v) ||
                 argd.TryGetValue("lrecl", out v) ||
                 argd.TryGetValue("allocation", out v)))
            {
                throw new ArgumentException("Host file creation properties do not work with append");
            }

            // Join the dictionary of keywords and values together, quoting each argument as necessary.
            var arge = argd.Select(kv => QuoteString(kv.Key + "=" + kv.Value));
            var result = await this.IoAsync("Transfer(" + string.Join(",", arge) + ")", isModify: true)
                .ConfigureAwait(continueOnCapturedContext: false);
            return result;
        }

        /// <summary>
        /// IND$FILE file transfer, asynchronous version.
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary).</param>
        /// <param name="hostType">Host type (TSO/VM/CICS).</param>
        /// <param name="parameters">Additional parameters.</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentNullException">An element of <paramref name="parameters"/> is null.</exception>
        /// <exception cref="ArgumentException">An element of <paramref name="parameters"/> is of the wrong type.</exception>
        public async Task<IoResult> TransferAsync(
            string localFile,
            string hostFile,
            Direction direction,
            Mode mode,
            HostType hostType,
            params object[] parameters)
        {
            var parameterList = new List<Parameter>();

            // Validate the parameter types.
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    throw new ArgumentNullException("parameter");
                }

                var p = parameters[i] as Parameter;
                if (p == null)
                {
                    throw new ArgumentException("parameter");
                }

                parameterList.Add(p);
            }

            return await this.TransferAsync(localFile, hostFile, direction, mode, hostType, parameterList)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// IND$FILE file transfer.
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary).</param>
        /// <param name="hostType">Host type (TSO/VM/CICS).</param>
        /// <param name="parameters">Additional parameters.</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentNullException">An element of <paramref name="parameters"/> is null.</exception>
        /// <exception cref="ArgumentException">An element of <paramref name="parameters"/> is of the wrong type.</exception>
        public IoResult Transfer(
            string localFile,
            string hostFile,
            Direction direction,
            Mode mode,
            HostType hostType,
            IEnumerable<Parameter> parameters)
        {
            try
            {
                return this.TransferAsync(localFile, hostFile, direction, mode, hostType, parameters).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// IND$FILE file transfer.
        /// </summary>
        /// <param name="localFile">Local file name.</param>
        /// <param name="hostFile">Host file name.</param>
        /// <param name="direction">Direction of transfer.</param>
        /// <param name="mode">Type of transfer (ASCII/binary).</param>
        /// <param name="hostType">Host type (TSO/VM/CICS).</param>
        /// <param name="parameters">Additional parameters.</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentNullException">An element of <paramref name="parameters"/> is null.</exception>
        /// <exception cref="ArgumentException">An element of <paramref name="parameters"/> is of the wrong type.</exception>
        public IoResult Transfer(
            string localFile,
            string hostFile,
            Direction direction,
            Mode mode,
            HostType hostType,
            params object[] parameters)
        {
            try
            {
                return this.TransferAsync(localFile, hostFile, direction, mode, hostType, parameters).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
