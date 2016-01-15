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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using X3270if;
    using X3270if.ProcessOptions;

    /// <summary>
    /// Tests for functionality internal to the session and feature methods, like string quoting.
    /// </summary>
    public class InnerTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InnerTests"/> class.
        /// </summary>
        public InnerTests()
        {
        }

        /// <summary>
        /// Test set-up.
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Util.ConsoleDebug = false;
        }

        /// <summary>
        /// Exercise the JoinNonEmpty String extension.
        /// </summary>
        [Test]
        public void TestJoinNonEmpty()
        {
            string left;

            // If right is empty, return left.
            left = "bar";
            left += left.JoinNonEmpty(" ", null);
            Assert.AreEqual("bar", left);

            // If left is empty, return right.
            left = string.Empty;
            left += left.JoinNonEmpty(" ", "foo");
            Assert.AreEqual("foo", left);

            // Join them with a separator.
            left = "bar";
            left += left.JoinNonEmpty(" ", "foo");
            Assert.AreEqual("bar foo", left);
        }

        /// <summary>
        /// Test the QuoteString method.
        /// </summary>
        [Test]
        public void TestQuoteString()
        {
            // String that goes through unscathed.
            string s = Session.QuoteString("xxx");
            Assert.AreEqual("xxx", s);

            // Strings that just need double quotes.
            s = Session.QuoteString("hello there"); // space
            Assert.AreEqual("\"hello there\"", s);
            s = Session.QuoteString("a,b"); // comma
            Assert.AreEqual("\"a,b\"", s);
            s = Session.QuoteString("a(b"); // left paren
            Assert.AreEqual("\"a(b\"", s);
            s = Session.QuoteString("a)b"); // right paren
            Assert.AreEqual("\"a)b\"", s);

            // Strings that need backslashes.
            s = Session.QuoteString("a\"b"); // double quote
            Assert.AreEqual("\"a\\\"b\"", s);
            s = Session.QuoteString(@"a\nb"); // backslash
            Assert.AreEqual("\"a\\\\nb\"", s);

            // Backslashes that are left alone.
            s = Session.QuoteString(@"a\nb", quoteBackslashes: false);
            Assert.AreEqual("\"a\\nb\"", s);

            // More than one of something, to make sure the whole string is scanned.
            s = Session.QuoteString("My, my (oh!) \"foo\"\\n");
            Assert.AreEqual("\"My, my (oh!) \\\"foo\\\"\\\\n\"", s);

            // Now the whole ASCII-7 character set, except the special characters, to make sure nothing else is molested.
            const string Special = "\" ,()\\";
            string ascii7 = string.Empty;
            for (int i = 33; i < 127; i++)
            {
                if (!Special.Contains((char)i))
                {
                    ascii7 += (char)i;
                }
            }

            s = Session.QuoteString(ascii7);
            Assert.AreEqual(ascii7, s);

            // Verify that known control characters are expanded and quotes are added.
            s = Session.QuoteString("hello\r\n\f\t\b");
            Assert.AreEqual("\"hello\\r\\n\\f\\t\\b\"", s);

            // Verify that other control characters are rejected.
            Assert.Throws<ArgumentException>(() => Session.QuoteString("hello\x7fthere"));
        }

        /// <summary>
        /// Test the ProcessOptions classes.
        /// </summary>
        [Test]
        public void TestProcessOptions()
        {
            // Trivial operations.
            Assert.AreEqual("-foo", new ProcessOptionWithoutValue("foo").Quote());
            Assert.AreEqual("-foo hello", new ProcessOptionWithValue("foo", "hello").Quote());
            Assert.AreEqual("-xrm \"ws3270.foo: bar\"", new ProcessOptionXrm("foo", "bar").Quote());

            // Option name validation.
            Assert.Throws<ArgumentNullException>(() => new ProcessOptionWithoutValue(null));
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithoutValue(string.Empty));
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithoutValue(" "));
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithoutValue("ab\nc"));
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithoutValue("ab\"c"));
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithoutValue("ab c"));

            // Option value validation.
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithValue("bob", "ab\nc"));
            Assert.Throws<ArgumentException>(() => new ProcessOptionWithValue("bob", "ab\"c"));

            // Leading dash is optional (and discouraged).
            Assert.AreEqual("-foo hello", new ProcessOptionWithValue("-foo", "hello").Quote());

            // "ws3270." is optional (and discouraged).
            Assert.AreEqual("-xrm \"ws3270.foo: bar\"", new ProcessOptionXrm("ws3270.foo", "bar").Quote());

            // "*." is tolerated on xrm.
            Assert.AreEqual("-xrm \"*foo: bar\"", new ProcessOptionXrm("*foo", "bar").Quote());

            // Non-xrm arguments are (unnecessarily) surrounded by double quotes, but backslashes are not quoted.
            var complex = new ProcessOptionWithValue("foo", @"C:\a\b");
            Assert.AreEqual("-foo \"C:\\a\\b\"", complex.Quote());

            // -xrm arguments get backslashes quoted.
            complex = new ProcessOptionXrm("foo", @"C:\a\b");
            Assert.AreEqual("-xrm \"ws3270.foo: C:\\\\a\\\\b\"", complex.Quote());

            // -xrm also allows certain control characters, and translates them to C escapes.
            complex = new ProcessOptionXrm("foo", "a\nb");
            Assert.AreEqual("-xrm \"ws3270.foo: a\\nb\"", complex.Quote());
        }

        /// <summary>
        /// Test the ExpandHostName method.
        /// </summary>
        [Test]
        public void TestExpandHostName()
        {
            // Start out with an X3270if session with no options, hence no default connect flags.
            var emulator = new ProcessSession();

            // Trivial version, does nothing.
            var s = emulator.ExpandHostName("host", null, null, ConnectFlags.None);
            Assert.AreEqual("host", s);

            // Make the host quotable.
            s = emulator.ExpandHostName("a:b::27", null, null, ConnectFlags.None);
            Assert.AreEqual("[a:b::27]", s);

            // Add a port.
            s = emulator.ExpandHostName("host", "port", null, ConnectFlags.None);
            Assert.AreEqual("host:port", s);

            // Add some LUs.
            s = emulator.ExpandHostName("host", null, new string[] { "lu1", "lu2" }, ConnectFlags.None);
            Assert.AreEqual("\"lu1,lu2@host\"", s);

            // Add some options.
            s = emulator.ExpandHostName("host", null, null, ConnectFlags.Secure);
            Assert.AreEqual("L:host", s);

            // Combine options, LUs and a port.
            // This is a little bit tricky to test, because the connect flags can appear in any order.
            // Otherwise, the order of the elements is fixed.
            s = emulator.ExpandHostName("1::2", "port", new string[] { "lu1", "lu2" }, ConnectFlags.Secure | ConnectFlags.NonTN3270E);
            Assert.IsTrue(s.StartsWith("\""));
            Assert.IsTrue(s.EndsWith("\""));
            s = s.Substring(1, s.Length - 2);
            const string LogicalUnitHostPort = "lu1,lu2@[1::2]:port";
            Assert.IsTrue(s.EndsWith(LogicalUnitHostPort));
            var flags = s.Substring(0, s.Length - LogicalUnitHostPort.Length);
            Assert.AreEqual(4, flags.Length);
            Assert.IsTrue(flags.Contains("L:"));
            Assert.IsTrue(flags.Contains("N:"));

            // Try all of the connect flags.
            s = emulator.ExpandHostName("host", null, null, ConnectFlags.All);
            const string LogicalUnitHostPort2 = "host";
            Assert.IsTrue(s.EndsWith(LogicalUnitHostPort2));
            flags = s.Substring(0, s.Length - LogicalUnitHostPort2.Length);
            Assert.AreEqual(12, flags.Length);
            Assert.IsTrue(flags.Contains("C:"));
            Assert.IsTrue(flags.Contains("L:"));
            Assert.IsTrue(flags.Contains("N:"));
            Assert.IsTrue(flags.Contains("P:"));
            Assert.IsTrue(flags.Contains("S:"));
            Assert.IsTrue(flags.Contains("B:"));

            // Try a session with non-default connect flags.
            var portEmulator = new PortSession(new PortConfig { AutoStart = false, DefaultConnectFlags = ConnectFlags.Secure });
            s = portEmulator.ExpandHostName("host", null, null, ConnectFlags.None);
            Assert.AreEqual("L:host", s);

            // Make sure the specific connect flags override the defaults (and are not ORed in).
            s = portEmulator.ExpandHostName("host", null, null, ConnectFlags.NonTN3270E);
            Assert.AreEqual("N:host", s);

            // Check the exceptions thrown on bad hostname, port, and LU.
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host/wrong"));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host@wrong"));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", "port/wrong"));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", "port:wrong"));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", "port.wrong"));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", "port@wrong"));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", lus: new string[] { "lu/wrong" }));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", lus: new string[] { "lu-okay", "lu:wrong" }));
            Assert.Throws<ArgumentException>(() => portEmulator.ExpandHostName("host", lus: new string[] { "lu@wrong" }));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("123:456::1.2.3.4"));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host-okay"));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host_okay"));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host", "port-okay"));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host", "port_okay"));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host", lus: new string[] { "lu.okay" }));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host", lus: new string[] { "lu-okay" }));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("host", lus: new string[] { "lu_okay" }));
            Assert.DoesNotThrow(() => portEmulator.ExpandHostName("года.ru"));
            Assert.DoesNotThrow(() => emulator.ExpandHostName("六.cn"));
        }
    }
}
