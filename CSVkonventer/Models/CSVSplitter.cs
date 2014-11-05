using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CSVkonventer.Models
{
    public class CSVSplitter
    {
        public const string _WINF_ACCOUNTS = "WinfAccountsInfoCSV";
        public const string _PAYPAL = "PaypalCSV";
        public const string _RECURLY_BASIC = "RecurlyBasicCSV";
        public const string _RECURLY_ACCOUNTS = "RecurlyAccountsCSV";
        public const string _RECURLY_BILLINGS = "RecurlyBillingsCSV";
        private const string _UNKNOWN = "UknownTypeCSV";


        public static string getCSVFormat(string firstLine)
        {
            if (firstLine.StartsWith("\"Paypal Transaction Id\",\"Plan Code\",\"Paid Amount In Cents\"")) { return _WINF_ACCOUNTS; }
            if (firstLine.StartsWith("Date,")) { return _PAYPAL; }
            if (firstLine.StartsWith("id,")) { return _RECURLY_BASIC; }
            if (firstLine.StartsWith("account_code,account_email,account_username")) { return _RECURLY_ACCOUNTS; }
            if (firstLine.StartsWith("account_code,first_name,last_name")) { return _RECURLY_BILLINGS; }
            else return _UNKNOWN;
        }


           public static string[] SplitCsvToLines(string csv, char delimeter = '\n')
        {
            csv = csv.Replace("\r\n", "\n").Replace("\n\r", "\n");
            List<string> lines = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool isInsideACell = false;

            foreach (char ch in csv)
            {
                if (ch == delimeter)
                {
                    if (isInsideACell == false)
                    {
                        // nasli sme koniec riadka, vsetko co je teraz v StringBuilder-y je riadok
                        lines.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                    if (ch == '"')
                    {
                        isInsideACell = !isInsideACell;
                    }
                }
            }

            if (sb.Length > 0)
            {
                lines.Add(sb.ToString());
            }

            return lines.ToArray();
        }

        public static string[] SplitCsvLineToCells(string line, char delimeter = ',')
        {
            List<string> list = new List<string>();
            do
            {
                if (line.StartsWith("\""))
                {
                    line = line.Substring(1);
                    int idx = line.IndexOf("\"");
                    while (line.IndexOf("\"", idx) == line.IndexOf("\"\"", idx))
                    {
                        idx = line.IndexOf("\"\"", idx) + 2;
                    }
                    idx = line.IndexOf("\"", idx);
                    list.Add(line.Substring(0, idx).Replace("\"\"", "\""));
                    if (idx + 2 < line.Length)
                    {
                        line = line.Substring(idx + 2);
                    }
                    else
                    {
                        line = String.Empty;
                    }
                }
                else
                {
                    list.Add(line.Substring(0, Math.Max(line.IndexOf(delimeter), 0)).Replace("\"\"", "\""));
                    line = line.Substring(line.IndexOf(delimeter) + 1);
                }
            }
            while (line.IndexOf(delimeter) != -1);
            if (!String.IsNullOrEmpty(line))
            {
                if (line.StartsWith("\"") && line.EndsWith("\""))
                {
                    line = line.Substring(1, line.Length - 2);
                }
                list.Add(line.Replace("\"\"", "\""));
            }

            return list.ToArray();
        }


    }
    
}