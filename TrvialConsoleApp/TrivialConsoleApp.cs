﻿namespace TrvialConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using X3270if;

    /// <summary>
    /// Trivial console app.
    /// </summary>
    public static class TrivialConsoleApp
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TrivialConsoleApp <hostname>");
                Environment.Exit(1);
            }

            var session = new ProcessSession();
            var startResult = session.Start();
            if (!startResult.Success)
            {
                Console.WriteLine("Start failed: {0}", startResult.FailReason);
                session.Close();
                Environment.Exit(1);
            }

            var ioResult = session.Connect(args[0]);
            if (!ioResult.Success)
            {
                Console.WriteLine("Connect failed: {0}", ioResult.Result[0]);
                session.Close();
                Environment.Exit(1);
            }

            ioResult = session.Wait(WaitMode.InputField);
            if (!ioResult.Success)
            {
                Console.WriteLine("Wait failed: {0}", ioResult.Result[0]);
                session.Close();
                Environment.Exit(1);
            }

            ioResult = session.Ascii();
            if (!ioResult.Success)
            {
                Console.WriteLine("Ascii failed: {0}", ioResult.Result[0]);
                session.Close();
                Environment.Exit(1);
            }

            Console.Write(string.Join("\n", ioResult.Result));

            session.Close();
        }
    }
}
