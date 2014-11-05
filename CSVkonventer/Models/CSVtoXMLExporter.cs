using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace CSVkonventer.Models
{
    public class CSVtoXMLExporter
    {
        public static string WarningMessage { get; set; }
        public static string ExportType;

        public static Dictionary<string, InvoiceModel> ReadCSVAddToInvoices(List<string> contentOfFiles)
        {
            WarningMessage = "";
            if (contentOfFiles.Count == 3)
            {
                ExportType = "Recurly";
                return RecurlyCsvExporter.CreateRecurlyInvoices(contentOfFiles);
            }
            if (contentOfFiles.Count == 2)
            {
                ExportType = "PayPal";
                return PayPalCsvExporter.CreatePayPalInvoices(contentOfFiles);
            }
            WarningMessage += "Zly pocet suborov (recurly 3, paypal ma 2)";
            return null;
        }

    }
}