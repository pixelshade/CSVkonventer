using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSVkonventer.Models
{
    public class PayPalCsvExporter
    {

        public static Dictionary<string, InvoiceModel> CreatePayPalInvoices(List<string> contentOfFiles)
        {
            RatesHistoryModel ratesHistory = new RatesHistoryModel();
            Dictionary<String, InvoiceModel> invoices = new Dictionary<string, InvoiceModel>();

            String invoicesPayPal = getPayPalInvoices(contentOfFiles);
            String accountsPayPal = getPayPalWinfAccountInfo(contentOfFiles);

            if (invoicesPayPal == null) { CSVtoXMLExporter.WarningMessage += "Nepodarilo sa najst invoices Paypal subor! \n"; return null; };
            if (accountsPayPal == null) { CSVtoXMLExporter.WarningMessage += "Nepodarilo sa najst accounts PayPal-winf subor! \n"; return null; };

            string[] lines = CSVSplitter.SplitCsvToLines(invoicesPayPal);
            for (int i = 1; i < lines.Length; ++i)
            {
                var cells = CSVSplitter.SplitCsvLineToCells(lines[i]);
                InvoiceModel invoice = paypalInvoiceFromCells(cells);

                if ((invoice != null))
                {
                    invoice.rate = ratesHistory.getRateForDate(invoice.date, invoice.currency);
                    if (!invoices.ContainsKey(invoice.transactionId))
                    {
                        invoices.Add(invoice.transactionId, invoice);
                    }
                    else
                    {
                        CSVtoXMLExporter.WarningMessage +=
                            "Pozor, boli dve rovnake transaction id keys v dictionary, treba premysliet";
                    }
                }
            }

            lines = CSVSplitter.SplitCsvToLines(accountsPayPal);
            for (int i = 1; i < lines.Length; ++i)
            {
                var cells = CSVSplitter.SplitCsvLineToCells(lines[i]);
                var transactionId = cells[0]; // transaction id

                InvoiceModel invoiceWithSameTransactionId = getInvoiceFromInvoicesByTransactionId(invoices, transactionId);

                if (invoiceWithSameTransactionId != null)
                {
                    addPayPalAccountInfoToInvoiceFromCells(invoiceWithSameTransactionId, cells);
                }
            }
            return invoices;
        }



        private static void addPayPalAccountInfoToInvoiceFromCells(InvoiceModel invoice, string[] cells)
        {
            invoice.plan_code = cells[1];
            invoice.line_item_start_date = cells[3].Substring(0, 10);
            invoice.line_item_end_date = cells[4].Substring(0, 10);
            if(cells[5].Length > 1 && cells[6].Length > 1) invoice.account_name = cells[5] + " " + cells[6];
            invoice.company = cells[7].Replace("&", "&amp;");
            invoice.street = cells[9].Replace("&", "&amp;");
            invoice.city = cells[10].Replace("&", "&amp;");
            invoice.postal_code = cells[11];
            invoice.purchase_country = cells[12];
        }



        private static InvoiceModel paypalInvoiceFromCells(string[] cells)
        {
            if (!isItemIDInPayPalCellAllowed(cells[20])) return null; //itemId identifikator

            InvoiceModel invoice = new InvoiceModel();
            string[] date = cells[0].Split('/');
            if (date[0].Length == 1) date[0] = "0"+date[0];
            if (date[1].Length == 1) date[1] = "0" + date[1];
            invoice.date = date[2] + '-' + date[0] + '-' + date[1];//date
            invoice.account_name = cells[3];
            invoice.closed_at = invoice.date;
            invoice.time = cells[1]; // time
            invoice.account_name = cells[3]; //name
            invoice.currency = cells[7]; //currency
            invoice.total = Convert.ToDecimal(cells[8]); //gross
            invoice.subtotal = Convert.ToDecimal(cells[10]); // Net     
            invoice.email = cells[12]; // from email
            invoice.email_to = cells[13]; // to email
            invoice.transactionId = cells[14]; //Transaction ID                    
            invoice.line_item_description = cells[19]; //item title 

            invoice.invoice_number = invoice.transactionId;
            return invoice;
        }

        private static bool isItemIDInPayPalCellAllowed(string itemID)
        {
            if (itemID.StartsWith("paypal-"))
            {
                return true;
            }
            return false;
        }

        private static InvoiceModel getInvoiceFromInvoicesByTransactionId(Dictionary<String, InvoiceModel> invoices, string transactionId)
        {
            if (invoices == null) return null;
            if (invoices.ContainsKey(transactionId)) return invoices[transactionId];
            return null;
        }

        private static String getPayPalInvoices(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (CSVSplitter.getCSVFormat(fileContent) == CSVSplitter._PAYPAL)
                {
                    return fileContent;
                }
            }
            return null;
        }

        private static String getPayPalWinfAccountInfo(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (CSVSplitter.getCSVFormat(fileContent) == CSVSplitter._WINF_ACCOUNTS)
                {
                    return fileContent;
                }
            }
            return null;
        }


        private static string generateInvoiceNumberFromInvoice(InvoiceModel invoice)
        {
            string[] time = invoice.time.Split(':'); 
            string[] date = invoice.date.Split('-'); // YYYY-mm-dd
            return (date[0].Substring(2))+ date[1]+ date[2] + time[0] + time[1];
//            return DateTime.Now.ToString("MMddssffff");
        }
    }
}