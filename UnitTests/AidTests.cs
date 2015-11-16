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
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using X3270if;

    /// <summary>
    /// Tests for basic emulation features (documented ws3270 actions).
    /// AID actions.
    /// </summary>
    [TestFixture]
    public class AidTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AidTests"/> class.
        /// </summary>
        public AidTests()
        {
        }

        /// <summary>
        /// Test fixture set-up.
        /// </summary>
        [TestFixtureSetUp]
        public void Setup()
        {
            Util.ConsoleDebug = false;
        }

        /// <summary>
        /// Exercise AID-generating methods.
        /// </summary>
        [Test]
        public void TestAid()
        {
            var session = new MockTaskSession();
            session.VerifyStart();

            session.VerifyCommand(() => session.Enter(), "Enter()");
            session.VerifyCommand(() => session.Clear(), "Clear()");
            session.VerifyCommand(() => session.PF(1), "PF(1)");
            session.VerifyCommand(() => session.PF(2), "PF(2)");
            session.VerifyCommand(() => session.PA(1), "PA(1)");
            session.VerifyCommand(() => session.PA(2), "PA(2)");

            // Force some exceptions.
            Assert.Throws<ArgumentOutOfRangeException>(() => session.PF(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.PF(25));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.PA(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.PA(4));

            session.ExceptionMode = true;
            session.AllFail = true;
            Assert.Throws<X3270ifCommandException>(() => session.Enter());
            Assert.Throws<X3270ifCommandException>(() => session.Clear());
            Assert.Throws<X3270ifCommandException>(() => session.PF(1));
            Assert.Throws<X3270ifCommandException>(() => session.PA(1));

            session.Close();
        }
    }
}
