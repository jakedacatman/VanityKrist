
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

        private string term = string.Empty;
        private int threads = 4;
        Random random = new Random();
        private bool check = false;
        private string regex = string.Empty;
        private ulong basepasswd = RandUlong();

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

            Output.AppendText($"starting with base {NumToHex(basepasswd)}" + "\n");
            Output.AppendText($"using {threads} threads" + "\n");

            Regex reg = null;
            if (term == string.Empty && regex != string.Empty) reg = new Regex(regex);

                

            ulong perThread = (ulong.MaxValue - basepasswd) / (ulong)threads;

            var bp = basepasswd;

            if (string.IsNullOrEmpty(regex))
                Output.AppendText("enter a term or regex");
            else
            {
                for (int i = 0; i < threads; i++)
                {
                    Output.AppendText($"spawned thread {i}, working from {NumToHex(bp)} to {NumToHex(bp + perThread)}" + "\n");
                    Task.Run(() => MinerThread(i, perThread, bp, reg, cts.Token), cts.Token);
                    bp += perThread;
                }
            }
            Output.AppendText("\n");

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

            basepasswd = RandUlong();
        }

        private void Clear_Click(object sender, EventArgs e) => Output.Text = string.Empty;

        private Task MinerThread(int id, ulong workSize, ulong basepasswd, Regex reg, CancellationToken token)
        {
            for (ulong curr = basepasswd; curr < (basepasswd + workSize); curr++)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;

                var passwd = NumToHex(curr);

                var protein = new string[9];

                var stick = DoubleHash(passwd);
                var link = 0;
                var v2 = new StringBuilder();
                var n = 0;
                for (n = 0; n < 9; n++)
                {
                    protein[n] = new string(new char[] { stick[0], stick[1] });
                    stick = DoubleHash(stick);
                }
                n = 0;

                var t = protein.Length;

                while (n < 9)
                {
                    var sub = stick.Substring(2 * n, 2);
                    link = Convert.ToInt32(sub, 16) % 9;
                    if (!string.IsNullOrEmpty(protein[link]))
                    {
                        var by = 48 + Math.Floor(Convert.ToByte(protein[link], 16) / 7d);
                        v2.Append((char)(by + 39 > 122 ? 101 : by > 57 ? by + 39 : by));
                        protein[link] = string.Empty;
                        n++;
                    }
                    else stick = Hash(stick);
                }
                var address = "k" + v2.ToString();
                counter++;
                if (reg != null)
                {
                    var match = reg.Match(address);
                    if (!match.Success)
                        continue;
                }

                if (!address.Contains(term) || address.Any(x => char.IsDigit(x)))
                    continue;

                Write(id, address, passwd);
            }
            return Task.CompletedTask;
        }

        private void Write(int id, string address, string passwd)
        {
            Output.Invoke(new Action(() => Output.AppendText($"thread {id} found {address}, with pw {passwd}\n")));
            l.Information($"{address}:{passwd}\n");
        }

        private Task UpdateCounter(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;
                Addresses.Invoke(new Action(() => Addresses.Text = counter.ToString()));
                MsPerA.Invoke(new Action(() => MsPerA.Text = Math.Round(1000d / counter, 5).ToString()));
                counter = 0;
                Thread.Sleep(1000);
            }
        }

        private string Hash(string toHash)
        {
            using (var h = SHA256.Create())
            {
                var s = h.ComputeHash(Encoding.UTF8.GetBytes(toHash));
                StringBuilder z =  new StringBuilder();
                for (int i = 0; i < s.Length; i++)
                {
                    z.Append(s[i].ToString("x2"));
                }
                return z.ToString();
            }
        }

        private string DoubleHash(string toHash)
        {
            return Hash(Hash(toHash));
        }

        private string NumToHex(ulong num)
        {
            return $"{num:x}";
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