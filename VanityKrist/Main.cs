
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
        private List<IDisposable> hashes = new List<IDisposable>();
        private List<long> times = new List<long>();
        private bool running = false;

        private Logger l = new LoggerConfiguration()
            .WriteTo.File("output.txt", outputTemplate: "{Message:l}")
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
            running = true;

            Output.AppendText($"starting with base {NumToHex(basepasswd)}" + "\n");
            Output.AppendText($"using {threads} threads" + "\n");

            Regex reg = null;
            if (term == string.Empty && regex != string.Empty) reg = new Regex(regex);

            ulong perThread = (ulong.MaxValue - basepasswd) / (ulong)threads;

            var bp = basepasswd;

            if (string.IsNullOrEmpty(term) && string.IsNullOrEmpty(regex))
            {
                Output.AppendText("enter a term or regex");
                StopIt();
            }
            else
            {
                for (int i = 0; i < threads; i++)
                {
                    SHA256 h = SHA256.Create();
                    Output.AppendText($"spawned thread {i}, working from {NumToHex(bp)} to {NumToHex(bp + perThread)}" + "\n");
                    Task.Run(() => MinerThread(i, perThread, bp, reg, h));
                    bp += perThread;
                    hashes.Add(h);
                }
            }
            Output.AppendText("\n");

            Task.Run(() => UpdateCounter(cts.Token));
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            StopIt();
        }
        private void StopIt()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();

            foreach (IDisposable h in hashes)
                h.Dispose();

            Stop.Enabled = false;
            Term.Enabled = true;
            Threads.Enabled = true;
            Start.Enabled = true;
            Numbers.Enabled = true;
            Regex.Enabled = true;

            basepasswd = RandUlong();
        }

        private void Clear_Click(object sender, EventArgs e) => Output.Text = string.Empty;

        private Task MinerThread(int id, ulong workSize, ulong basepasswd, Regex reg, SHA256 h)
        {
            for (ulong curr = basepasswd; running && curr < (basepasswd + workSize); curr++)
            {
                var passwd = NumToHex(curr);

                var protein = new string[9];

                var stick = BytesToHex(DoubleHash(Encoding.UTF8.GetBytes(passwd), h));
                var link = 0;
                var v2 = new StringBuilder();
                int n;
                for (n = 0; n < 9; n++)
                {
                    protein[n] = new string(new char[] { stick[0], stick[1] });
                    stick = BytesToHex(DoubleHash(Encoding.UTF8.GetBytes(stick), h));
                }
                n = 0;

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
                    else stick = BytesToHex(h.ComputeHash(Encoding.UTF8.GetBytes(stick)));
                }
                var address = "k" + v2.ToString();
                counter++;

                if (reg != null)
                {
                    var match = reg.Match(address);
                    if (!match.Success)
                        continue;
                }

                if (!address.Contains(term) || (!check && address.Any(x => char.IsDigit(x))))
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

        private byte[] DoubleHash(byte[] input, SHA256 h)
        {
            return h.ComputeHash(Encoding.UTF8.GetBytes(BytesToHex(h.ComputeHash(input))));
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

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string BytesToHex(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
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