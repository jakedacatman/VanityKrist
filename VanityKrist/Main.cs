using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using Serilog;
using Serilog.Core;

namespace VanityKrist
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Stop.Enabled = false;
        }

        private string term = "";
        private int threads = 4;
        Random random = new Random();
        private bool check = false;
        private string regex = "";
        private ulong basepkey = RandUlong();

        private Logger l = new LoggerConfiguration()
            .WriteTo.Async(x => x.File("output.txt", outputTemplate: "{Message:l}"))
            .CreateLogger();

        private CancellationTokenSource cts = new CancellationTokenSource();

        private int counter = 0;

        private void Term_TextChanged(object sender, EventArgs e)
        {
            term = Term.Text.ToLower().MakeAlphanumeric();
            Term.Text = term;
        }

        private void Numbers_CheckedChanged(object sender, EventArgs e) => check = Numbers.Checked;

        private void Regex_TextChanged(object sender, EventArgs e) => regex = Regex.Text;

        private void Threads_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(Threads.Text, out int t)) threads = t;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            Term.Enabled = false;
            Threads.Enabled = false;
            Start.Enabled = false;
            Numbers.Enabled = false;
            Stop.Enabled = true;
            Regex.Enabled = false;

            Output.AppendText($"using {threads} threads" + "\n");

            Regex reg = null;
            if (regex != "") reg = new Regex(regex);

            ulong perThread = (ulong.MaxValue - basepkey) / (ulong)threads;

            var bp = basepkey;

            for (int i = 0; i < threads; i++)
            {
<<<<<<< HEAD
                Regex reg = null;
                if (regex != "") reg = new Regex(regex);
                if (length <= 0)
                {
                    int plength = (int)Math.Pow(2, i + 1);
                    new Task(() => MinerThread(plength, reg, cts.Token), cts.Token, TaskCreationOptions.LongRunning).Start();
                    Output.AppendText($"spawning thread {i + 1} with pkey length {plength}" + "\n");
                }
                else
                {
                    new Task(() => MinerThread(length, reg, cts.Token), cts.Token, TaskCreationOptions.LongRunning).Start();
                    Output.AppendText($"spawning thread {i + 1} with pkey length {length}" + "\n");
                }
=======
                Output.AppendText($"spawned thread {i}, working from {string.Format("0x{0:X}", bp).Replace("0x", "").ToLower()} to {string.Format("0x{0:X}", bp + perThread).Replace("0x", "").ToLower()}" + "\n");

                Task.Run(() => MinerThread(perThread, bp, reg, cts.Token), cts.Token);

                bp += perThread;
>>>>>>> rewrite kind of
            }

            Task.Run(() => UpdateCounter(cts.Token));
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            cts = new CancellationTokenSource();

            Stop.Enabled = false;
            Term.Enabled = true;
            Threads.Enabled = true;
            Start.Enabled = true;
            Numbers.Enabled = true;
            Regex.Enabled = true;

            basepkey = RandUlong();
        }

        private void Clear_Click(object sender, EventArgs e) => Output.Text = "";

        private Task MinerThread(ulong workSize, ulong basepkey, Regex reg, CancellationToken token)
        {
            for (ulong curr = basepkey; curr < (basepkey + workSize); curr++)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;

                var hex = NumToHex(curr);

                //var pkey = $"{Hash(hex)}-000";
                var pkey = hex;

                var protein = new string[9];

                var stick = Hash(Hash(pkey));
                var link = 0;
                var v2 = new StringBuilder();
                var n = 0;
                for (n = 0; n < 9; n++)
                {
                    if (n < 9)
                    {
                        protein[n] = new string(new char[] { stick[0], stick[1] });
                        stick = Hash(Hash(stick));
                    }
                }
                n = 0;

                var t = protein.Length;

                while (n < 9)
                {
                    var sub = stick.Substring(2 * n, 2);
                    link = Convert.ToInt32(sub, 16) % 9;
                    if (link >= 0 && protein[link] != null && protein[link].Length != 0)
                    {
                        var by = 48 + Math.Floor(Convert.ToByte(protein[link], 16) / 7d);
                        v2.Append((char)(by + 39 > 122 ? 101 : by > 57 ? by + 39 : by));
                        protein[link] = "";
                        n++;
                    }
                    else stick = Hash(stick);
                }
                var address = "k" + v2.ToString();
                counter++;

                //Output.Invoke(new Action(() => Output.AppendText($"{address}:{pkey}\n")));
                //pkey = Hash("KRISTWALLET" + pkey) + "-000";
                if (check)
                {
                    if (term != "" && address.Contains(term))
                    {
                        Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                        l.Information($"{address}:{pkey}");
                    }
                    else if (reg != null)
                    {
                        var match = reg.Match(address);
                        if (match.Success)
                        {
                            Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                            l.Information($"{address}:{pkey}");
                        }
                    }
                }
                else if (!address.Any(x => char.IsDigit(x)))
                {
                    if (term != "" && address.Contains(term))
                    {
                        Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                        l.Information($"{address}:{pkey}");
                    }
                    else if (reg != null)
                    {
                        var match = reg.Match(address);
                        if (match.Success)
                        {
                            Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                            l.Information($"{address}:{pkey}");
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task UpdateCounter(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;
                Addresses.Invoke(new Action(() => Addresses.Text = counter.ToString()));
                counter = 0;
                Thread.Sleep(1000);
            }
        }

        private string Hash(string toHash)
        {
            using (var h = SHA256.Create())
            {
                return string.Join("", h.ComputeHash(Encoding.UTF8.GetBytes(toHash)).Select(x => x.ToString("x2")));
            }
<<<<<<< HEAD
            return false;
            return false;
=======
>>>>>>> rewrite kind of
        }

        private string NumToHex(ulong num)
        {
            char[] hexChars = new char[16]
            {
                '0', '1', '2','3',
                '4', '5', '6', '7',
                '8', '9', 'a', 'b',
                'c', 'd', 'e', 'f'
            };

            char[] gen = new char[16];
            for (int i = 0; i < 16; i++)
            {
                gen[i] = hexChars[(num & 15ul * (ulong)Math.Pow(16, 15 - i)) >> (60   - (4 * i))];
            }
            return new string(gen);
        }
        private static ulong RandUlong()
        {
            var r = new Random();
            var buffer = new byte[sizeof(ulong)];
            r.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
    public static class StringExtensions
    {
        public static string MakeAlphanumeric(this string text)
        {
            var s = text.Where(x => char.IsLetterOrDigit(x)).ToArray();
            return new string(s);
        }
    }
}