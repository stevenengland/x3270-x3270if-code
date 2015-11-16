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

namespace X3270if.ProcessOptions
{
    using System;
    using System.Linq;

    using X3270if;

    /// <summary>
    /// Abstract class for defining additional ws3270 process options.
    /// </summary>
    public abstract class ProcessOption
    {
        /// <summary>
        /// Backing field for <see cref="OptionName"/>
        /// </summary>
        private string optionName;

        /// <summary>
        /// Gets or sets the option name.
        /// </summary>
        protected string OptionName
        {
            get
            {
                return this.optionName;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("name");
                }

                var plainValue = value.StartsWith("-") ? value.Substring(1) : value;
                if (string.IsNullOrWhiteSpace(plainValue) ||
                    plainValue.ToCharArray().Any(c => c == '"' || c == '\\' || char.IsWhiteSpace(c) || char.IsControl(c)))
                {
                    throw new ArgumentException("name");
                }

                this.optionName = plainValue;
            }
        }

        /// <summary>
        /// Expand an option into a properly-quoted string to pass on the command line.
        /// </summary>
        /// <returns>Quoted string.</returns>
        public abstract string Quote();
    }

    /// <summary>
    /// Extra command-line option without a parameter.
    /// </summary>
    public class ProcessOptionWithoutValue : ProcessOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessOptionWithoutValue"/> class.
        /// </summary>
        /// <param name="optionName">Option name. A leading '-' will be added if needed.</param>
        public ProcessOptionWithoutValue(string optionName)
        {
            this.OptionName = optionName;
        }

        /// <summary>
        /// Expand an option into a properly-quoted string to pass on the command line.
        /// </summary>
        /// <returns>Quoted string.</returns>
        public override string Quote()
        {
            return "-" + this.OptionName;
        }
    }

    /// <summary>
    /// Extra command-line option with a parameter.
    /// </summary>
    public class ProcessOptionWithValue : ProcessOption
    {
        /// <summary>
        /// Backing field for <see cref="OptionValue"/>.
        /// </summary>
        private string optionValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessOptionWithValue"/> class.
        /// </summary>
        /// <param name="option">Option name. A leading '-' will be added if needed.</param>
        /// <param name="value">Option value.</param>
        public ProcessOptionWithValue(string option, string value)
        {
            this.OptionName = option;
            this.OptionValue = value;
        }

        /// <summary>
        /// Gets or sets the option value.
        /// </summary>
        protected string OptionValue
        {
            get
            {
                return this.optionValue;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value.ToCharArray().Any(c =>
                    c == '"' ||
                    (char.IsControl(c) &&
                        (!this.AllowCControl || !"\r\n\b\f\t".Contains(c)))))
                {
                    throw new ArgumentException("value");
                }

                this.optionValue = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether values can contain control characters. By default, it's false.
        /// The <see cref="ProcessOptionXrm"/> subclass overrides this property with one that returns true.
        /// </summary>
        protected virtual bool AllowCControl
        {
            get { return false; }
        }

        /// <summary>
        /// Expand an option into a properly-quoted string to pass on the command line.
        /// </summary>
        /// <returns>Quoted string.</returns>
        public override string Quote()
        {
            // Note: Command-line options do not generally need quoted backslashes.
            return string.Format("-{0} {1}", this.OptionName, Session.QuoteString(this.OptionValue, quoteBackslashes: false));
        }
    }

    /// <summary>
    /// Extra command-line <c>-xrm</c> option.
    /// </summary>
    public class ProcessOptionXrm : ProcessOptionWithValue
    {
        /// <summary>
        /// The common prefix for ws3270 resources.
        /// </summary>
        private const string Ws3270Dot = "ws3270.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessOptionXrm"/> class.
        /// </summary>
        /// <param name="resource">Resource name. Do not include "ws3270." in the name.</param>
        /// <param name="value">Resource value.</param>
        public ProcessOptionXrm(string resource, string value)
            : base("xrm", string.Format("{0}: {1}", AddWs3270(resource), value))
        {
        }

        /// <summary>
        /// Gets a flag indicating whether values can contain control characters. For <c>Xrm</c>, it's true.
        /// </summary>
        protected override bool AllowCControl
        {
            get { return true; }
        }

        /// <summary>
        /// Expand an option into a properly-quoted string to pass on the command line.
        /// </summary>
        /// <returns>Quoted string.</returns>
        public override string Quote()
        {
            // Note: -xrm options *do* need quoted backslashes.
            return string.Format("-{0} {1}", this.OptionName, Session.QuoteString(this.OptionValue, quoteBackslashes: true));
        }

        /// <summary>
        /// Return the full name of a resource.
        /// </summary>
        /// <param name="resource">Resource base name.</param>
        /// <returns>Expanded name.</returns>
        private static string AddWs3270(string resource)
        {
            return resource.StartsWith(Ws3270Dot) || resource.StartsWith("*") ? resource : Ws3270Dot + resource;
        }
    }
}