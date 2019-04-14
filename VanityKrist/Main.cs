using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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
        private bool breakpls = false;
        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private TextWriter output = TextWriter.Synchronized(File.AppendText("output.txt"));

        private void Term_TextChanged(object sender, EventArgs e) => term = Term.Text.Replace(" ", "").ToLower();

        private void Numbers_CheckedChanged(object sender, EventArgs e) => check = Numbers.Checked;

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
            breakpls = false;
            started = true;

            Term.Enabled = false;
            Threads.Enabled = false;
            Start.Enabled = false;
            Numbers.Enabled = false;
            Stop.Enabled = true;

            for (int i = 0; i < threads; i++)
                new Thread(new ThreadStart(MinerThread)).Start();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            breakpls = true;
            started = false;

            Stop.Enabled = false;
            Term.Enabled = true;
            Threads.Enabled = true;
            Start.Enabled = true;
            Numbers.Enabled = true;
        }

        private void Clear_Click(object sender, EventArgs e) => Output.Text = "";

        private async void MinerThread()
        {
            while (true)
            {
                if (breakpls == true) break;
                char[] stringChars = new char[16];
                while (true)
                {
                    for (int i = 0; i < stringChars.Length; i++)
                    {
                        stringChars[i] = chars[random.Next(chars.Length)];
                    }
                    var test = new string(stringChars);
                    if (IsIn(queue, test) == false) break;
                }

                var pkey = new string(stringChars);
                queue.Enqueue(pkey);

                var address = (await KristMethods.MakeV2Address(pkey)).ToLower();
                if (check)
                {
                    if (address.Contains(term))
                    {
                        Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                        await output.WriteLineAsync($"{address}:{pkey}");
                        await output.FlushAsync();
                    }
                }
                else if (!address.Any(x => char.IsDigit(x)))
                {
                    if (address.Contains(term))
                    {
                        Output.Invoke(new Action(() => Output.AppendText($"found {address}, with pw {pkey}\n")));
                        await output.WriteLineAsync($"{address}:{pkey}");
                        await output.FlushAsync();
                    }
                }
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
    }
}
