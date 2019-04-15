using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyEncryption;

namespace VanityKrist
{
    public static class KristMethods
    {
        private static string makeV2Address(string pkey)
        {
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
                    v2.Append(MakeByte(Convert.ToByte(protein[link], 16)));
                    protein[link] = "";
                    n++;
                }
                else stick = SHA.ComputeSHA256Hash(stick);
            }
            return "k" + v2.ToString();
        }

        public static async Task<string> MakeV2Address(string pkey) => await Task.Run(() => makeV2Address(pkey));

        public static string MakeKristWallet(string pkey) { return SHA.ComputeSHA256Hash("KRISTWALLET" + pkey) + "-000"; }

        private static char MakeByte(byte b)
        {
            var by = 48 + Math.Floor(b / 7d);
            return (char)(by + 39 > 122 ? 101 : by > 57 ? by +39 : by);
        }
    }
}
