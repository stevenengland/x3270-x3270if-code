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
    public static class SessionExtensions
    {
        // Delegate for VerifyMockCommand.
        public delegate IoResult VerifyDelegate();

        /// <summary>
        /// Verify that an action succeeds and produces a particular emulator command.
        /// </summary>
        /// <param name="session">Mock session.</param>
        /// <param name="del">Action to test.</param>
        /// <param name="expected">Expected text.</param>
        public static void VerifyCommand(this MockTaskSession session, VerifyDelegate del, string expected)
        {
            IoResult result;
            result = del();
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(expected, session.LastCommandProcessed);
        }

        /// <summary>
        /// Start a mock session and make sure it was successful.
        /// </summary>
        /// <param name="session">Session to start.</param>
        public static void VerifyStart(this MockTaskSession session)
        {
            var result = session.Start();
            Assert.AreEqual(true, result.Success);
        }
    }

    /// <summary>
    /// Tests for basic emulation features (documented ws3270/wc3270 actions).
    /// </summary>
    [TestFixture]
    public class FeatureTests
    {
        public FeatureTests()
        {
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            Util.ConsoleDebug = false;
        }

        /// <summary>
        /// Exercise the session Connect method.
        /// </summary>
        [Test]
        public void TestConnect()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.Connect("bob"); },
                "Connect(bob)");

            session.VerifyCommand(
                () => { return session.Connect("bob", "27", new string[] { "lu1", "lu2" }, ConnectFlags.NoLogin); },
                "Connect(\"C:lu1,lu2@bob:27\")");

            Assert.Throws<ArgumentException>(() => { var result = session.Connect(""); });

            // Force an exception.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Connect("foo"); });

            session.Close();
        }

        [Test]
        public void TestDisconnect()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.Disconnect(); },
                 "Disconnect()");

            // Force an exception.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Disconnect(); });

            session.Close();
        }

        /// <summary>
        /// Exercise the session String method.
        /// </summary>
        [Test]
        public void TestString()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.String("Fred"); },
                "String(Fred)");

            session.VerifyCommand(
                () => { return session.String("a\\b"); },
                "String(\"a\\\\b\")");

            session.VerifyCommand(
                () => { return session.String("a\\b", quoteBackslashes: false); },
                "String(\"a\\b\")");

            // Force an exception.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.String("foo"); });

            session.Close();
        }

        /// <summary>
        /// Exercise the session StringAt methods.
        /// </summary>
        [Test]
        public void TestStringAt()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.StringAt(1, 0, "Fred"); },
                "MoveCursor(1,0) String(Fred)");

            session.VerifyCommand(
                () => { return session.StringAt(new []
                {
                    new StringAtBlock { Row = 1, Column = 0, Text = "Fred" },
                    new StringAtBlock { Row = 2, Column = 4, Text = "Smith" }
                } ); },
                "MoveCursor(1,0) String(Fred) MoveCursor(2,4) String(Smith)");

            session.VerifyCommand(
                () => { return session.StringAt(1, 0, "Fred", eraseEof: true); },
                "MoveCursor(1,0) EraseEOF() String(Fred)");

            // Exercise row and column checking.
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.StringAt(-1, 0, "foo"); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.StringAt(0, -1, "foo"); });
            
            // Force exceptions.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.StringAt(1, 0, "foo"); });
            Assert.Throws<X3270ifCommandException>(() =>
            {
                var result = session.StringAt(new[]
                {
                    new StringAtBlock { Row = 1, Column = 0, Text = "Fred" },
                    new StringAtBlock { Row = 2, Column = 4, Text = "Smith" }
                });
            });

            session.Close();
        }

        /// <summary>
        /// Exercise the session Wait method.
        /// </summary>
        [Test]
        public void TestWait()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.Wait(WaitMode.Wait3270Mode); },
                "Wait(3270Mode)");

            session.VerifyCommand(
                () => { return session.Wait(WaitMode.Output, 10); },
                "Wait(10,Output)");

            // Force an exception.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Wait(WaitMode.Unlock); });

            session.Close();
        }

        // Exercise the session Ascii method.
        [Test]
        public void TestAscii()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.Ascii(); },
                "Ascii()");

            session.VerifyCommand(
                () => { return session.Ascii(10); },
                "Ascii(10)");

            session.VerifyCommand(
                () => { return session.Ascii(1, 2, 3); },
                "Ascii(1,2,3)");

            session.VerifyCommand(
                () => { return session.Ascii(1, 2, 3, 4); },
                "Ascii(1,2,3,4)");

            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.Ascii(-1, 2, 3); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.Ascii(1, -1, 3); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.Ascii(-1, 1, 3, 4); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.Ascii(1, -1, 3, 4); });

            // Force some exceptions.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Ascii(); });
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Ascii(1); });
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Ascii(1, 2, 3); });
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Ascii(1, 2, 3, 4); });

            session.Close();
        }

        // Exercise the session Ascii method with a 1-origin session.
        [Test]
        public void TestAscii1()
        {
            var session = new MockTaskSession(new MockTaskConfig { Origin = 1 });
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.Ascii(1, 2, 3); },
                "Ascii(0,1,3)");

            session.VerifyCommand(
                () => { return session.Ascii(1, 2, 3, 4); },
                "Ascii(0,1,3,4)");

            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.Ascii(0, 2, 3); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = session.Ascii(1, 0, 3); });

            session.Close();
        }

        /// <summary>
        /// Exercise the session ReadBuffer method.
        /// </summary>
        [Test]
        public void TestReadBuffer()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.ReadBuffer(); },
                "ReadBuffer(Ascii)");

            session.VerifyCommand(
                () => { return session.ReadBuffer(Session.ReadBufferType.Ascii); },
                "ReadBuffer(Ascii)");

            session.VerifyCommand(
                () => { return session.ReadBuffer(Session.ReadBufferType.Ebcdic); },
                "ReadBuffer(Ebcdic)");

            // Force an exception.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.ReadBuffer(); });

            session.Close();
        }

        /// <summary>
        /// Exercise the session Query method.
        /// </summary>
        [Test]
        public void TestQuery()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(
                () => { return session.Query(QueryType.BindPluName); },
                "Query(BindPluName)");

            Assert.AreEqual(session.Query(QueryType.Cursor).Result[0], "0 0");

            // Force an exception.
            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => { var result = session.Query(QueryType.Formatted); });

            session.Close();

            // Exercise 1-origin.
            session = new MockTaskSession(new MockTaskConfig { Origin = 1 });
            session.Start();
            Assert.AreEqual(session.Query(QueryType.Cursor).Result[0], "1 1");

            // Make sure the history contains the actual value from the emulator.
            Assert.AreEqual(session.RecentCommands[0].Result[0], "0 0");

            session.Close();
        }
    }
}
