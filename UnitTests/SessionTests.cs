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

namespace UnitTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Mock;

    using NUnit.Framework;

    using X3270if;
    using X3270if.ProcessOptions;

    /// <summary>
    /// Tests for configuring, starting and stopping sessions.
    /// </summary>
    [TestFixture]
    public class SessionTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTests"/> class.
        /// </summary>
        public SessionTests()
        {
        }

        /// <summary>
        /// Test set-up.
        /// </summary>
        [TestFixtureSetUp]
        public void Setup()
        {
            Util.ConsoleDebug = false;
        }

        /// <summary>
        /// Exercise the logging code, regardless of the flag set in Setup().
        /// </summary>
        [Test]
        public void TestLog()
        {
            // Temporarily change the ConsoleDebug flag.
            var oldDebug = Util.ConsoleDebug;
            try
            {
                Util.ConsoleDebug = true;

                // Temporarily redirect stdout to NUL:.
                var oldOut = Console.Out;
                try
                {
                    Console.SetOut(new StreamWriter(Stream.Null));

                    // Say hello.
                    Util.Log("Hello");
                }
                finally
                {
                    Console.SetOut(oldOut);
                }
            }
            finally
            {
                Util.ConsoleDebug = oldDebug;
            }
        }

        /// <summary>
        /// Test the Config class.
        /// </summary>
        [Test]
        public void TestConfig()
        {
            // Verify that a bogus model number throws an exception.
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var startup = new ProcessConfig
                {
                    Model = 6
                };
            });

            // Verify that a good one doesn't.
            var startup2 = new ProcessConfig
            {
                Model = 2
            };
            Assert.AreEqual(2, startup2.Model);

            // Verfify that a bogus Origin is bad.
            Assert.Throws<ArgumentOutOfRangeException>(() => { var config = new ProcessConfig { Origin = 2 }; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var config = new ProcessConfig { Origin = -1 }; });
        }

        /// <summary>
        /// Test the various flavors of history on an un-started session.
        /// </summary>
        [Test]
        public void TestEmptyHistory()
        {
            var session = new ProcessSession();

            Assert.AreEqual(null, session.LastCommand);
            var emptyIoResult = new IoResult[0];
            Assert.AreEqual(emptyIoResult, session.RecentCommands);
            Assert.AreEqual(null, session.StatusLine);
        }

        /// <summary>
        /// Test the limit on the history list.
        /// </summary>
        [Test]
        public void TestFullHistory()
        {
            // Start a mock thread session.
            var session = new MockTaskSession();
            var startResult = session.Start();
            Assert.AreEqual(true, startResult.Success);

            // Make sure the start shows the initial empty command.
            var history = session.RecentCommands;
            Assert.AreEqual(1, history.Length);
            Assert.AreEqual("Query(LocalEncoding)", history[0].Command);

            // Overflow the history list.
            for (int i = 0; i < Session.MaxCommands; i++)
            {
                Assert.AreEqual(true, session.Io("Lines 1").Success);
            }

            history = session.RecentCommands;
            Assert.AreEqual("Lines 1", history[0].Command);
            Assert.AreEqual("Line 1", history[0].Result[0]);

            session.Close();
        }

        /// <summary>
        /// Test the mock session, task flavor.
        /// </summary>
        [Test]
        public void TestTaskMockSession()
        {
            var session = new MockTaskSession();
            var startResult = session.Start();
            Assert.AreEqual(true, startResult.Success);

            // Test canned responses.
            var result = session.Io("Lines 1");
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(1, result.Result.Length);
            Assert.AreEqual("Line 1", result.Result[0]);

            result = session.Io("Lines 2");
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(2, result.Result.Length);
            Assert.AreEqual("Line 1", result.Result[0]);
            Assert.AreEqual("Line 2", result.Result[1]);

            result = session.Io("Fail");
            Assert.AreEqual(false, result.Success);
            Assert.AreEqual(1, result.Result.Length);
            Assert.AreEqual("failed", result.Result[0]);

            // Test a double start.
            Assert.Throws<InvalidOperationException>(() => session.Start());

            // Test the I/O timeout.
            Assert.AreEqual(true, startResult.Success);
            result = session.Io("Hang 100", 50);
            Assert.AreEqual(false, result.Success);
            session.Close();

            // Test the EOF response.
            startResult = session.Start();
            Assert.AreEqual(true, startResult.Success);
            result = session.Io("Quit");
            Assert.AreEqual(false, result.Success);
            Assert.AreEqual(false, session.EmulatorRunning);
            Assert.AreEqual(false, session.LastCommand.Success);
            session.Close();

            // Test the exception for I/O on a closed session.
            Assert.Throws<InvalidOperationException>(() => session.Io("Xxx"));
        }

        /// <summary>
        /// Exercise code page matching.
        /// </summary>
        [Test]
        public void TestCodePage()
        {
            var session = new MockTaskSession();
            session.CodePage = "CP1252";
            Assert.IsTrue(session.Start().Success);

            session.Close();
            session.CodePage = "Junk";
            Assert.IsFalse(session.Start().Success);
            
            session.CodePage = null;
            session.CodePageFail = true;
            Assert.IsFalse(session.Start().Success);
        }

        /// <summary>
        /// Test exception mode at start-up.
        /// </summary>
        [Test]
        public void TestStartExceptionMode()
        {
            // Set the handshake timeout to 50msec.
            var startup = new MockTaskConfig
            {
                HandshakeTimeoutMsec = 50
            };
            var session = new MockTaskSession(startup);

            // Set the response hang time to twice that.
            session.HangMsec = 100;

            // Set exception mode for any failure, including Start.
            session.ExceptionMode = true;

            // Boom.
            Assert.Throws<X3270ifCommandException>(() => session.Start());
        }

        /// <summary>
        /// Test a hung start (no response to the empty initial command) for the Task
        /// flavor of a mock session.
        /// </summary>
        [Test]
        public void TestTaskMockSessionHungStart()
        {
            var startup = new MockTaskConfig
            {
                HandshakeTimeoutMsec = 50
            };
            var session = new MockTaskSession(startup);
            session.HangMsec = 100;

            // Test a hung start.
            var startResult = session.Start();
            Assert.AreEqual(false, startResult.Success);
        }

        /// <summary>
        /// Test the mock session, separate process flavor.
        /// </summary>
        [Test]
        public void TestProcessMockSession()
        {
            var startup = new ProcessConfig
            {
                ProcessName = "MockWs3270.exe",
                ConnectRetryMsec = 50
            };

            // Try a successful start.
            var session = new ProcessSession(startup);
            var result = session.Start();
            Assert.AreEqual(true, result.Success);
            session.Close();

            // Exercise some bad command-line options.
            startup.TestFirstOptions = "-baz";
            result = session.Start();
            Assert.AreEqual(false, result.Success);

            startup.TestFirstOptions = "-scriptport 12345678";
            result = session.Start();
            Assert.AreEqual(false, result.Success);
        }

        /// <summary>
        /// Test connecting to an existing mock session, process flavor.
        /// </summary>
        [Test]
        public void TestProcessMockSessionStartExisting()
        {
            var session = new PortSession(new PortConfig { AutoStart = false });

            // Try no variable.
            var startResult = session.Start();
            Assert.AreEqual(false, startResult.Success);

            // Try junk variable.
            Environment.SetEnvironmentVariable(Util.X3270Port, "junk");
            startResult = session.Start();
            Assert.AreEqual(false, startResult.Success);

            // Set up an explicit config to shorten the retry delay.
            var config = new PortConfig
            {
                ConnectRetryMsec = 10,
                AutoStart = false
            };

            // Try an unused port.
            // We guarantee an unused port by binding a socket to port 0 (letting the
            // system pick one) and then *not* listening on it.
            using (var noListenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                noListenSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                Environment.SetEnvironmentVariable(Util.X3270Port, ((IPEndPoint)noListenSocket.LocalEndPoint).Port.ToString());
                session = new PortSession(config);
                startResult = session.Start();
                Assert.AreEqual(false, startResult.Success);
            }

            // Try again, without the connect retry override.
            // This is frustratingly slow, but needed to exercise the default timeout of 3x1000ms.
            config.ConnectRetryMsec = null;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            startResult = session.Start();
            Assert.AreEqual(false, startResult.Success);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 3000);

            // Verify that we can't set a ConnectRetryMsec < 0.
            Assert.Throws<ArgumentOutOfRangeException>(() => { var config2 = new PortConfig { ConnectRetryMsec = -3 }; });

            // Create a mock server thread by hand, so we can connect to it manually.
            using (var listener = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                listener.Listen(1);
                var mockServer = new Server();
                var server = Task.Run(() => mockServer.Ws3270(listener));

                // Now connect to it, which should be successful.
                // We also put the port in the config, to exercise that path.
                config.Port = ((IPEndPoint)listener.LocalEndPoint).Port;
                startResult = session.Start();
                Assert.AreEqual(true, startResult.Success);

                // All done, close the client side and wait for the mock server to complete.
                session.Close();
                server.Wait();
            }
        }

        /// <summary>
        /// Explicit test for the PortBackEnd.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "That's what we're testing"), Test]
        public void TestPortBackEnd()
        {
            var backEnd = new PortBackEnd(null);

            // Make sure GetErrorOutput does nothing.
            Assert.AreEqual("Nothing", backEnd.GetErrorOutput("Nothing"));

            // Dispose it twice, to exercise all of the Dispose logic.
            backEnd.Dispose();
            backEnd.Dispose();
        }

        /// <summary>
        /// Test a process session, using a .EXE that does not exist.
        /// </summary>
        [Test]
        public void TestNoSuchProcess()
        {
            var startup = new ProcessConfig
            {
                ProcessName = "MockWs3270NoSuch.exe"
            };
            var session = new ProcessSession(startup);

            var startResult = session.Start();
            Assert.AreEqual(false, startResult.Success);
        }

        /// <summary>
        /// Explicit test for the process back end.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "That's what we're testing"), Test]
        public void TestProcessBackEnd()
        {
            var backEnd = new ProcessBackEnd(null);
            Assert.AreEqual("nothing", backEnd.GetErrorOutput("nothing"));
            backEnd.Dispose();
            backEnd.Dispose();
        }

        /// <summary>
        /// Test the StatusField method.
        /// </summary>
        [Test]
        public void TestStatusField()
        {
            var session = new MockTaskSession();

            // Test StatusField exception when not running.
            Assert.Throws<InvalidOperationException>(() => session.StatusField(StatusLineField.Formatting));

            var startResult = session.Start();
            Assert.AreEqual(true, startResult.Success);

            // Test ordinary StatusField.
            Assert.AreEqual("F", session.StatusField(StatusLineField.Formatting));

            // Test origin-based StatusField.
            Assert.AreEqual("0", session.StatusField(StatusLineField.CursorRow));
            Assert.AreEqual("0", session.StatusField(StatusLineField.CursorColumn));

            session.Close();

            // Test origin-based StatusField with 1-origin.
            session = new MockTaskSession(new MockTaskConfig { Origin = 1 });
            session.Start();
            Assert.AreEqual("1", session.StatusField(StatusLineField.CursorRow));
            Assert.AreEqual("1", session.StatusField(StatusLineField.CursorColumn));

            // Exercise HostConnected (based on the status line).
            Assert.AreEqual(true, session.HostConnected);

            // Change that.
            session.Connected = false;

            // (Have to run a dummy command to get a new prompt and change state.)
            session.Io("Query()");
            Assert.AreEqual(false, session.HostConnected);

            // Now screen-modifying commands will fail.
            Assert.AreEqual(false, session.Enter().Success);

            // Now exception mode will fire, too.
            session.ExceptionMode = true;
            Assert.Throws<X3270ifCommandException>(() => session.Enter());

            // Try requiring 3270 mode instead.
            session.Config.ModifyFail = ModifyFailType.Require3270;
            Assert.Throws<X3270ifCommandException>(() => session.Enter());

            // And that normally, it's fine.
            session.Connected = true;
            session.Connect("bob");
            Assert.DoesNotThrow(() => session.Enter());

            session.Close();
        }

        /// <summary>
        /// Exercise session exception mode.
        /// </summary>
        [Test]
        public void TestExceptionMode()
        {
            var session = new MockTaskSession();
            var startResult = session.Start();
            Assert.AreEqual(true, startResult.Success);

            session.ExceptionMode = true;
            Assert.Throws<X3270ifCommandException>(() => session.Io("Fail"));
            Assert.Throws<X3270ifCommandException>(() => session.Io("Fail()"));
            Assert.Throws<X3270ifCommandException>(() => session.Io("Quit"));
            session.Close();
        }

        /// <summary>
        /// Exercise auto-start Port sessions.
        /// </summary>
        [Test]
        public void TestPortAutoStart()
        {
            // No X3270PORT to connect to.
            Assert.Throws<X3270ifInternalException>(() => new PortSession());
        }

        /// <summary>
        /// Exercise argument checking in the <c>Io</c> methods.
        /// </summary>
        [Test]
        public void TestIoArgs()
        {
            var session = new MockTaskSession();
            session.Start();

            Assert.Throws<ArgumentException>(() => session.Io("Foo(\0)"));
        }

        /// <summary>
        /// Exercise broken sessions.
        /// </summary>
        [Test]
        public void TestBrokenSession()
        {
            var session = new MockTaskSession();
            session.Start();

            var result = session.Io("Quit");
            Assert.AreEqual(false, result.Success);
            Assert.AreEqual(false, session.EmulatorRunning);

            session.Close();
            session.Start();
            result = session.Io("ReplyQuit");
            Assert.AreEqual(true, result.Success);
            Thread.Sleep(500);
            result = session.Io("Anything");
            Assert.AreEqual(false, result.Success);
            Assert.AreEqual(false, session.EmulatorRunning);
        }

        /// <summary>
        /// Blow up the Session base class.
        /// </summary>
        [Test]
        public void TestSessionNullBackEnd()
        {
            Assert.Throws<ArgumentNullException>(() => new MockTaskSession(new MockTaskConfig(), forceChaos: true));
        }

        /// <summary>
        /// Make process arguments too long.
        /// </summary>
        [Test]
        public void TestArgsTooLong()
        {
            var config = new ProcessConfig
            {
                ProcessName = "MockWs3270.exe",
                ExtraOptions = Enumerable.Range(1, 11000).Select(x => new ProcessOptionWithoutValue("x"))
            };
            var session = new ProcessSession(config);
            Assert.Throws<InvalidOperationException>(() => session.Start());
        }
    }
}
