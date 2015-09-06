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
using x3270if.Transfer;
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
                        Direction.Send,
                        Mode.Ascii,
                        HostType.Vm,
                        new ParameterExistAction(ExistAction.Replace));
                },
                "Transfer(direction=Send,exist=Replace,host=Vm,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",mode=Ascii)");


            // Bad parameter type.
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("foo.txt", "foo txt a", Direction.Send, Mode.Ascii, HostType.Vm, "wrong!"); });
            Assert.Throws<ArgumentNullException>(
                () => { var result = session.Transfer("foo.txt", "foo txt a", Direction.Send, Mode.Ascii, HostType.Vm,
                    new ParameterExistAction(ExistAction.Replace), null); });

            // Bad file names.
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("", "foo txt a", Direction.Send, Mode.Ascii, HostType.Vm); });
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer(null, "foo txt a", Direction.Send, Mode.Ascii, HostType.Vm); });
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("foo.txt", "", Direction.Send, Mode.Ascii, HostType.Vm); });
            Assert.Throws<ArgumentException>(
                () => { var result = session.Transfer("foo.txt", null, Direction.Send, Mode.Ascii, HostType.Vm); });

            // Bad AsciiRemap.
            Assert.Throws<ArgumentException>(
                () => { var param = new ParameterAsciiRemap(false, 252); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterAsciiRemap(true, 0); });

            // Bad block size.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterBlockSize(0); });

            // Bad logical record length.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterSendLogicalRecordLength(0); });

            // Bad TsoSendAllocations.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 0); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100, 0); });
            Assert.Throws<ArgumentException>(
                () => { var param = new ParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, 200); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, 200, 0); });
            Assert.Throws<ArgumentException>(
                () => { var param = new ParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100, 200, 300); });

            // Bad buffer size.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { var param = new ParameterBufferSize(0); });

            // All possible options.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        Direction.Send,
                        Mode.Ascii,
                        HostType.Tso,
                        new ParameterAsciiCr(false),
                        new ParameterAsciiRemap(true, 252),
                        new ParameterExistAction(ExistAction.Replace),
                        new ParameterSendRecordFormat(RecordFormat.Fixed),
                        new ParameterSendLogicalRecordLength(80),
                        new ParameterBlockSize(1024),
                        new ParameterTsoSendAllocation(TsoAllocationUnits.Avblock, 100, 200, 300),
                        new ParameterBufferSize(4096));
                },
                "Transfer(allocation=Avblock,avblock=300,blocksize=1024,buffersize=4096,cr=keep,direction=Send,exist=Replace,host=Tso,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",lrecl=80,mode=Ascii,primaryspace=100,recfm=Fixed,remap=yes,secondaryspace=200,windowscodepage=252)");

            // Some ASCII option variations.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        Direction.Send,
                        Mode.Ascii,
                        HostType.Tso,
                        new ParameterAsciiCr(true),
                        new ParameterAsciiRemap(false));
                },
                "Transfer(cr=add,direction=Send,host=Tso,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",mode=Ascii,remap=no)");

            // Same thing, using the IEnumerable API.
            session.VerifyCommand(
                () =>
                {
                    return session.Transfer(
                        @"C:\foo.txt",
                        "FOO TXT A",
                        Direction.Send,
                        Mode.Ascii,
                        HostType.Tso,
                        new List<Parameter> {
                            new ParameterAsciiCr(true),
                            new ParameterAsciiRemap(false)
                        });
                },
                "Transfer(cr=add,direction=Send,host=Tso,\"hostfile=FOO TXT A\",\"localfile=C:\\\\foo.txt\",mode=Ascii,remap=no)");

            // AsciiCr without Ascii.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Receive, Mode.Binary, HostType.Tso,
                        new ParameterAsciiCr(true));
                });


            // Same thing, using the IEnumerable API.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Receive, Mode.Binary, HostType.Tso,
                        new List<Parameter> { new ParameterAsciiCr(true) });
                });

            // AsciiRemap without Ascii.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Receive, Mode.Binary, HostType.Tso,
                        new ParameterAsciiRemap(true));
                });


            // Lrecl without send.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Receive, Mode.Binary, HostType.Tso,
                        new ParameterSendLogicalRecordLength(80));
                });

            // Lrecl on CICS.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Cics,
                        new ParameterSendLogicalRecordLength(80));
                });

            // Recfm without send.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Receive, Mode.Binary, HostType.Tso,
                        new ParameterSendRecordFormat(RecordFormat.Fixed));
                });

            // Recfm on CICS.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Cics,
                        new ParameterSendRecordFormat(RecordFormat.Fixed));
                });

            // TSO allocation without send.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Receive, Mode.Binary, HostType.Tso,
                        new ParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100));
                });

            // TSO allocation on non-TSO.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Vm,
                        new ParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100));
                });

            // Append with recfm.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Tso,
                        new ParameterExistAction(ExistAction.Append), new ParameterSendRecordFormat(RecordFormat.Fixed));
                });

            // Append with lrecl.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Tso,
                        new ParameterExistAction(ExistAction.Append), new ParameterSendLogicalRecordLength(80));
                });

            // Append with TSO allocation.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Tso,
                        new ParameterExistAction(ExistAction.Append), new ParameterTsoSendAllocation(TsoAllocationUnits.Cylinders, 100));
                });

            // Blocksize without TSO.
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Cics,
                        new ParameterBlockSize(1024));
                });
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var result = session.Transfer("foo.txt", "FOO.TXT",
                        Direction.Send, Mode.Binary, HostType.Vm,
                        new ParameterBlockSize(1024));
                });

            session.Close();
        }
    }
}
