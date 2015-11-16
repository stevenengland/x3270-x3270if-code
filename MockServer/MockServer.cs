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

namespace Mock
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using X3270if;

    /// <summary>
    /// A mock ws3270 server. Can be used as a task within a test, or as
    /// the core of a mock ws3270.exe process.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Backing field for <see cref="Connected"/>.
        /// </summary>
        private bool connected = true;

        /// <summary>
        /// Gets or sets a value indicating whether to make all commands fail.
        /// </summary>
        public bool AllFail { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds to make the next command hang.
        /// </summary>
        public int HangMsec { get; set; }

        /// <summary>
        /// Gets or sets the workstation code page, if not UTF-8. (Just what is returned by
        /// the Query action, for now.)
        /// </summary>
        public string CodePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to make the Query CodePage action fail.
        /// </summary>
        public bool CodePageFail { get; set; }

        /// <summary>
        /// Gets or sets the last command.
        /// </summary>
        public string LastCommandProcessed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session appears to be connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.connected;
            }

            set
            {
                this.connected = value;
            }
        }

        /// <summary>
        /// Mock ws3270 server, given a listening socket.
        /// </summary>
        /// <param name="listeningSocket">Socket to accept connection on and close.</param>
        public void Ws3270(Socket listeningSocket)
        {
            var connection = listeningSocket.Accept();
            listeningSocket.Close();
            this.Ws3270Common(connection);
        }

        /// <summary>
        /// Mock 3270 server, given a port to listen on.
        /// </summary>
        /// <param name="port">Port to listen on and accept one connection.</param>
        public void Ws3270(int port)
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.ExclusiveAddressUse = false;
            listener.Start();
            var connection = listener.AcceptSocket();
            listener.Stop();
            this.Ws3270Common(connection);
        }

        /// <summary>
        /// Mock ws3270 server.
        /// Allows most commands to succeed silently, with no output.
        /// Some special commands:
        ///     Fail
        ///         This command will fail.
        ///     Lines {count}
        ///         Succeeds with {count} lines of output.
        ///     Hang {milliseconds}
        ///         Wait 5s before responding.
        ///     Quit
        ///         Exit the server.
        /// This code is used both inline as a mock server, in a task, and in the standalone mock
        /// ws3270 program.
        /// </summary>
        /// <param name="connection">Socket with accepted connection.</param>
        public void Ws3270Common(Socket connection)
        {
            // Set up streams to process network I/O.
            // The output stream needs to use just newlines as line delimiters, as ws3270 does, and no BOM.
            var networkStream = new NetworkStream(connection);
            var streamReader = new System.IO.StreamReader(networkStream, Encoding.UTF8);
            var streamWriter = new System.IO.StreamWriter(networkStream, new UTF8Encoding(false));
            streamWriter.NewLine = "\n";

            // Process requests.
            bool dead = false;
            string line;
            try
            {
                while (!dead && (line = streamReader.ReadLine()) != null)
                {
                    // Remember the last command processed.
                    this.LastCommandProcessed = line;

                    // If AllFail is set, fail.
                    if (this.AllFail)
                    {
                        this.Fail(streamWriter);
                        continue;
                    }

                    // If HangMsec is set, wait.
                    if (this.HangMsec > 0)
                    {
                        Thread.Sleep(this.HangMsec);
                    }

                    // Split the command and arguments into tokens.
                    string[] token = line.Split(new char[] { ' ', '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    if (token.Length == 0)
                    {
                        token = new string[] { string.Empty };
                    }

                    // Process commands.
                    switch (token[0])
                    {
                        case "Fail":
                            // Fail on purpose.
                            this.Fail(streamWriter);
                            break;
                        case "Query":
                            if (token.Length == 2 && token[1] == "LocalEncoding")
                            {
                                if (!this.CodePageFail)
                                {
                                    // Respond with our local encoding.
                                    streamWriter.WriteLine("data: " + ((this.CodePage != null) ? this.CodePage : "UTF-8"));
                                    this.Prompt(streamWriter, true);
                                }
                                else
                                {
                                    streamWriter.WriteLine("data: unknown query");
                                    this.Prompt(streamWriter, false);
                                }
                            }
                            else if (token.Length == 2 && token[1] == "Cursor")
                            {
                                streamWriter.WriteLine("data: 0 0");
                                this.Prompt(streamWriter, true);
                            }
                            else if (token.Length == 2 && Enum.IsDefined(typeof(QueryType), token[1]))
                            {
                                streamWriter.WriteLine("data: xxx");
                                this.Prompt(streamWriter, true);
                            }
                            else
                            {
                                streamWriter.WriteLine("data: unknown query");
                                this.Prompt(streamWriter, false);
                            }

                            break;
                        case "Lines":
                            // Return n lines of data.
                            int lineCount;
                            if (token.Length < 2 || !int.TryParse(token[1], out lineCount) || lineCount < 1)
                            {
                                lineCount = 1;
                            }

                            for (int lineNum = 1; lineNum <= lineCount; lineNum++)
                            {
                                streamWriter.WriteLine(string.Format("Line {0}", lineNum));
                            }

                            this.Prompt(streamWriter, true);
                            break;
                        case "ReplyWith":
                            // Echo back the remainder of the line as data, with parens removed and commas translated to spaces.
                            streamWriter.WriteLine("data: " + string.Join(" ", token.Skip(1).ToArray()));
                            if (line.Length > token[0].Length)
                            {
                                streamWriter.WriteLine("data:" + line.Substring(token[0].Length));
                            }

                            this.Prompt(streamWriter, true);
                            break;
                        case "Hang":
                            // Wait before replying.
                            int sleepMsec;
                            if (token.Length < 2 || !int.TryParse(token[1], out sleepMsec))
                            {
                                sleepMsec = 5000;
                            }

                            Thread.Sleep(sleepMsec);
                            this.Prompt(streamWriter, true);
                            break;
                        case "Quit":
                            // Exit the server without replying.
                            dead = true;
                            break;
                        case "ReplyQuit":
                            // Exit the server with replying.
                            this.Prompt(streamWriter, true);
                            dead = true;
                            break;
                        default:
                            // Everything else succeeds.
                            this.Prompt(streamWriter, true);
                            break;
                    }
                }
            }
            catch (System.IO.IOException)
            {
            }

            connection.Close();
        }

        /// <summary>
        /// Write a prompt to the stream.
        /// </summary>
        /// <param name="streamWriter">StreamWriter to write to.</param>
        /// <param name="success">True if success, false if failure.</param>
        private void Prompt(StreamWriter streamWriter, bool success)
        {
            streamWriter.WriteLine(
                "U F U {0} {1} 2 24 80 0 0 0x0 -",
                this.Connected ? "C(fakehost.com)" : "N",
                this.Connected ? "I" : "N");
            streamWriter.WriteLine(success ? "ok" : "error");
            streamWriter.Flush();
        }

        /// <summary>
        /// Fail a command with some canned data.
        /// </summary>
        /// <param name="streamWriter">StreamWrite to write to.</param>
        private void Fail(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("data: failed");
            this.Prompt(streamWriter, false);
        }
    }
}
