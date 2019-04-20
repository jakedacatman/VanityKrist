using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using EasyEncryption;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
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
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        private bool started = false;
        private bool check = false;
        private string regex = "";
        private int length = 0;
        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

        private Logger l = new LoggerConfiguration()
            .WriteTo.Async(x => x.File("output.txt", outputTemplate: "{Message:l}"))
            .CreateLogger();

        private CancellationTokenSource cts = new CancellationTokenSource();

        private int counter = 0;

        private void Term_TextChanged(object sender, EventArgs e) => term = Term.Text.Replace(" ", "").ToLower();

        private void Numbers_CheckedChanged(object sender, EventArgs e) => check = Numbers.Checked;

        private void Regex_TextChanged(object sender, EventArgs e) => regex = Regex.Text;

        private void Length_TextChanged(object sender, EventArgs e) => int.TryParse(Length.Text, out length);

        private void Threads_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(Threads.Text, out int t)) threads = t;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (started)
            {
                Output.AppendText("Already started!\n");
                return;
            }
            started = true;

            Term.Enabled = false;
            Threads.Enabled = false;
            Start.Enabled = false;
            Numbers.Enabled = false;
            Stop.Enabled = true;
            Regex.Enabled = false;
            Length.Enabled = false;

            Output.AppendText($"using {threads} threads" + "\n");

            for (int i = 0; i < threads; i++)
            {
                Regex reg = null;
                if (regex != "") reg = new Regex(regex);
                if (length <= 0)
                {
                    int plength = (int)Math.Pow(2, i + 1);
                    new Task(async () => await MinerThread(plength, reg, cts.Token), cts.Token, TaskCreationOptions.LongRunning).Start();
                    Output.AppendText($"spawning thread {i + 1} with pkey length {plength}" + "\n");
                }
                else
                {
                    new Task(async () => await MinerThread(length, reg, cts.Token), cts.Token, TaskCreationOptions.LongRunning).Start();
                    Output.AppendText($"spawning thread {i + 1} with pkey length {length}" + "\n");
                }
            }

            Task.Run(() => UpdateCounter(cts.Token));

        }

        private void Stop_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            started = false;

            Stop.Enabled = false;
            Term.Enabled = true;
            Threads.Enabled = true;
            Start.Enabled = true;
            Numbers.Enabled = true;
            Regex.Enabled = true;
            Length.Enabled = true;
        }

        private void Clear_Click(object sender, EventArgs e) => Output.Text = "";

        private Task MinerThread(int plength, Regex reg, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;
                char[] stringChars = new char[plength];
                var iter = 0;
                while (true)
                {
                    if (iter % 10000 == 0)
                        if (token.IsCancellationRequested) return Task.CompletedTask;
                    for (int i = 0; i < stringChars.Length; i++)
                    {
                        stringChars[i] = chars[random.Next(chars.Length)];
                    }
                    var test = new string(stringChars);
                    if (IsIn(queue, test) == false) break;
                    iter++;
                }

                var pkey = new string(stringChars);
                queue.Enqueue(pkey);
                if (pkey == null || pkey == string.Empty)
                    return null;
                var protein = new string[9];

                var stick = SHA.ComputeSHA256Hash(SHA.ComputeSHA256Hash(pkey));
                var link = 0;
                var v2 = new StringBuilder();
                var n = 0;
                for (n = 0; n < 9; n++)
                {
                    if (n < 9)
                    {
                        protein[n] = new string(new char[] { stick[0], stick[1] });
                        stick = SHA.ComputeSHA256Hash(SHA.ComputeSHA256Hash(stick));
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
                    else stick = SHA.ComputeSHA256Hash(stick);
                }
                var address = "k" + v2.ToString();
                counter++;
                if (check)
                    Find(address, pkey, reg);
                else if (!address.Any(x => char.IsDigit(x)))
                    Find(address, pkey, reg);
            }
        }

        private Task UpdateCounter(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;
                Output.Invoke(new Action(() =>  Addresses.Text = counter.ToString()));
                counter = 0;
                Thread.Sleep(1000);
            }
        }

        private bool IsIn(ConcurrentQueue<string> queue, string obj)
        {
            foreach (string t in queue)
            {
                if (t == obj) return true;
            }
            return false;
        }

        private void Find(string address, string pkey, Regex reg)
        {

            if (term != "" && address.Contains(term))
            {
                Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                l.Information($"{address}:{pkey}\n");
            }
            else
            {
                if (reg != null)
                {
                    var match = reg.Match(address);
                    if (match.Success)
                    {
                        Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                        l.Information($"{address}:{pkey}\n");
                    }
                }
            }
        }
    }
}