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

namespace x3270if
{
    public partial class Session
    {
        /// <summary>
        /// Send an Enter AID. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> EnterAsync()
        {
            return await IoAsync("Enter()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Send a Clear AID. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> ClearAsync()
        {
            return await IoAsync("Clear()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Send a PF AID. Asynchronous version.
        /// </summary>
        /// <param name="n">PF index.</param>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is not between 1 and 24.</exception>
        public async Task<IoResult> PFAsync(int n)
        {
            if (n < 1 || n > 24)
            {
                throw new ArgumentOutOfRangeException("n");
            }
            return await IoAsync("PF(" + n.ToString() + ")").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Send a PA AID. Asynchronous version.
        /// </summary>
        /// <param name="n">PA index.</param>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is not between 1 and 3.</exception>
        public async Task<IoResult> PAAsync(int n)
        {
            if (n < 1 || n > 3)
            {
                throw new ArgumentOutOfRangeException("n");
            }
            return await IoAsync("PA(" + n.ToString() + ")").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Send an Enter AID.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Enter()
        {
            try
            {
                return EnterAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Send a Clear AID.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Clear()
        {
            try
            {
                return ClearAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Send a PF AID.
        /// </summary>
        /// <param name="n">PF index.</param>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is not between 1 and 24.</exception>        
        public IoResult PF(int n)
        {
            try
            {
                return PFAsync(n).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Send a PA AID.
        /// </summary>
        /// <param name="n">PA index.</param>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is not between 1 and 3.</exception> 
        public IoResult PA(int n)
        {
            try
            {
                return PAAsync(n).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

    }
}
