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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using x3270if;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Mock;

namespace UnitTests
{
    /// <summary>
    /// Tests for basic emulation features (documented ws3270/wc3270 actions).
    /// Transfer action.
    /// </summary>
    [TestFixture]
    public class TransferTests
    {
        public TransferTests()
        {
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            Util.ConsoleDebug = false;
        }

        /// <summary>
        /// Exercise the cursor movement methods.
        /// </summary>
        [Test]
        public void TestTransfer()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            // Basic functionality.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        TransferDirection.Send,
                        TransferMode.Ascii,
                        TransferHostType.Vm,
                        new TransferParameterExistAction(ExistAction.Replace));
                },
                "Transfer(direction=Send,exist=Replace,host=Vm,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",mode=Ascii)");


            // Bad parameter type.
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("foo.txt", "foo txt a", TransferDirection.Send, TransferMode.Ascii, TransferHostType.Vm, "wrong!"); });
            Assert.Throws<ArgumentNullException>(
                () => { var result = session.Transfer("foo.txt", "foo txt a", TransferDirection.Send, TransferMode.Ascii, TransferHostType.Vm,
                    new TransferParameterExistAction(ExistAction.Replace), null); });

            // Bad file names.
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("", "foo txt a", TransferDirection.Send, TransferMode.Ascii, TransferHostType.Vm); });
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer(null, "foo txt a", TransferDirection.Send, TransferMode.Ascii, TransferHostType.Vm); });
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("foo.txt", "", TransferDirection.Send, TransferMode.Ascii, TransferHostType.Vm); });
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("foo.txt", null, TransferDirection.Send, TransferMode.Ascii, TransferHostType.Vm); });

            // Bad AsciiRemap.
            Assert.Throws<ArgumentException>(
                () => { var param = new TransferParameterAsciiRemap(false, 252); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterAsciiRemap(true, 0); });

            // Bad block size.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterBlockSize(0); });

            // Bad logical record length.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterSendLogicalRecordLength(0); });

            // Bad TsoSendAllocations.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 0); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100, 0); });
            Assert.Throws<ArgumentException>(
                () => { var param = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, 200); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, 200, 0); });
            Assert.Throws<ArgumentException>(
                () => { var param = new TransferParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100, 200, 300); });

            // Bad buffer size.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new TransferParameterBufferSize(0); });

            // All possible options.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        TransferDirection.Send,
                        TransferMode.Ascii,
                        TransferHostType.Tso,
                        new TransferParameterAsciiCr(false),
                        new TransferParameterAsciiRemap(true, 252),
                        new TransferParameterExistAction(ExistAction.Replace),
                        new TransferParameterSendRecordFormat(RecordFormat.Fixed),
                        new TransferParameterSendLogicalRecordLength(80),
                        new TransferParameterBlockSize(1024),
                        new TransferParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, 200, 300),
                        new TransferParameterBufferSize(4096));
                },
                "Transfer(allocation=Avblock,avblock=300,blocksize=1024,buffersize=4096,cr=keep,direction=Send,exist=Replace,host=Tso,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",lrecl=80,mode=Ascii,primaryspace=100,recfm=Fixed,remap=yes,secondaryspace=200,windowscodepage=252)");

            // Some ASCII option variations.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        TransferDirection.Send,
                        TransferMode.Ascii,
                        TransferHostType.Tso,
                        new TransferParameterAsciiCr(true),
                        new TransferParameterAsciiRemap(false));
                },
                "Transfer(cr=add,direction=Send,host=Tso,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",mode=Ascii,remap=no)");

            // Same thing, using the IEnumerable API.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        TransferDirection.Send,
                        TransferMode.Ascii,
                        TransferHostType.Tso,
                        new List<TransferParameter> {
                            new TransferParameterAsciiCr(true),
                            new TransferParameterAsciiRemap(false)
                        });
                },
                "Transfer(cr=add,direction=Send,host=Tso,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",mode=Ascii,remap=no)");

            // AsciiCr without Ascii.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Receive, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterAsciiCr(true));
                });


            // Same thing, using the IEnumerable API.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Receive, TransferMode.Binary, TransferHostType.Tso,
                        new List<TransferParameter> { new TransferParameterAsciiCr(true) });
                });

            // AsciiRemap without Ascii.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Receive, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterAsciiRemap(true));
                });


            // Lrecl without send.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Receive, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterSendLogicalRecordLength(80));
                });

            // Lrecl on CICS.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Cics,
                        new TransferParameterSendLogicalRecordLength(80));
                });

            // Recfm without send.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Receive, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterSendRecordFormat(RecordFormat.Fixed));
                });

            // Recfm on CICS.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Cics,
                        new TransferParameterSendRecordFormat(RecordFormat.Fixed));
                });

            // TSO allocation without send.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Receive, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100));
                });

            // TSO allocation on non-TSO.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Vm,
                        new TransferParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100));
                });

            // Append with recfm.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterExistAction(ExistAction.Append), new TransferParameterSendRecordFormat(RecordFormat.Fixed));
                });

            // Append with lrecl.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterExistAction(ExistAction.Append), new TransferParameterSendLogicalRecordLength(80));
                });

            // Append with TSO allocation.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Tso,
                        new TransferParameterExistAction(ExistAction.Append), new TransferParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100));
                });

            // Blocksize without TSO.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Cics,
                        new TransferParameterBlockSize(1024));
                });
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        TransferDirection.Send, TransferMode.Binary, TransferHostType.Vm,
                        new TransferParameterBlockSize(1024));
                });

            session.Close();
        }
    }
}
