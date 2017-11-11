using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
    static class Common
    {
        public static List<string> EverythingBetween(this string source, string start, string end)
        {
            var results = new List<string>();

            string pattern = string.Format(
                "{0}({1}){2}",
                Regex.Escape(start),
                ".+?",
                 Regex.Escape(end));

            foreach (Match m in Regex.Matches(source, pattern))
            {
                results.Add(m.Groups[1].Value.Trim());
            }

            return results;
        }
    public static void SaveLog(string path, Obj obj)
    {
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            string data = string.Join(",", obj.customer, obj.wo, obj.model, obj.type, obj.quantity, obj.timeCall, obj.timeResponseStart, obj.timeResponseEnd, obj.status);
            writer.WriteLine(data);
        }
    }
    }
}
