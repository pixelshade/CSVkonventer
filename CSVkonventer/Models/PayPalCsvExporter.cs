using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSVkonventer.Models
{
    public class PayPalCsvExporter
    {

        public static Dictionary<string, InvoiceModel> CreatePayPalInvoices(List<string> contentOfFiles, String warningMessage)
        {
            RatesHistoryModel ratesHistory = new RatesHistoryModel();
            Dictionary<String, InvoiceModel> invoices = new Dictionary<string, InvoiceModel>();

            String invoicesPayPal = getPayPalInvoices(contentOfFiles);
            String accountsPayPal = getPayPalWinfAccountInfo(contentOfFiles);

            if (invoicesPayPal == null) { warningMessage += "Nepodarilo sa najst invoices Paypal subor! "; return null; };
            if (accountsPayPal == null) { warningMessage += "Nepodarilo sa najst accounts PayPal-winf subor! "; return null; };

            string[] lines = CSVSplitter.SplitCsvToLines(invoicesPayPal);
            for (int i = 1; i < lines.Length; ++i)
            {
                var cells = CSVSplitter.SplitCsvLineToCells(lines[i]);
                InvoiceModel invoice = paypalInvoiceFromCells(cells);

                if ((invoice != null) && (invoice.line_item_total >= 0))
                {
                    invoice.rate = ratesHistory.getRateForDate(invoice.date, invoice.currency);
                    invoice.homeTax = invoice.homeTotal * InvoiceModel._TAX;
                    invoice.homePrice = invoice.homeTotal - invoice.homeTax;

                    if (!invoices.ContainsKey(invoice.id) && (!invoice.status.Equals("open")))
                    {
                        invoices.Add(invoice.id, invoice);
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
            invoice.currency = "USD";
            invoice.line_item_start_date = cells[3].Substring(0, 10);
            invoice.line_item_end_date = cells[4].Substring(0, 10);
            invoice.account_name = cells[5] + " " + cells[6];
            invoice.company = cells[7].Replace("&", "&amp;");
            invoice.street = cells[9].Replace("&", "&amp;");
            invoice.city = cells[10].Replace("&", "&amp;");
            invoice.postal_code = cells[11];
            invoice.purchase_country = cells[12];
        }



        private static InvoiceModel paypalInvoiceFromCells(string[] cells)
        {
            if (isItemIDInPayPalCellAllowed(cells[20])) return null; //itemId identifikator

            InvoiceModel invoice = new InvoiceModel();
            invoice.invoice_number = generateInvoiceNumberForPayPal();
            invoice.id = cells[12]; //Transaction ID                    
            invoice.account_name = cells[3]; //name
            invoice.email = cells[10];
            invoice.email_to = cells[11];
            invoice.total = Convert.ToDecimal(cells[7]); //gross <---------asi net
            invoice.subtotal = Convert.ToDecimal(cells[9]); //asi Net?                    
            invoice.currency = cells[7]; //currency
            string[] date = cells[0].Split('/');
            invoice.date = date[2] + '-' + date[0] + '-' + date[1];//date
            invoice.closed_at = invoice.date;
            invoice.status = cells[5];//status
            invoice.purchase_country = cells[14]; //address status                    
            invoice.line_item_total = Convert.ToDecimal(cells[8]); // Gross
            invoice.net = Convert.ToDecimal(cells[10]); //net


            invoice.line_item_description = cells[15]; //title 
            invoice.line_item_product_code = cells[16];  //product code                 
            invoice.type = cells[4];  //type   
            return invoice;
        }

        private static bool isItemIDInPayPalCellAllowed(string itemID)
        {
            if (itemID.Equals("paypal-1") || itemID.Equals("paypal-2") || itemID.Equals("paypal-vat-1") ||
                itemID.Equals("paypal-vat-2"))
            {
                return true;
            }
            return false;
        }

        private static InvoiceModel getInvoiceFromInvoicesByTransactionId(Dictionary<String, InvoiceModel> invoices, string transactionId)
        {
            if (invoices == null) return null;
            foreach (InvoiceModel invoice in invoices.Values)
            {
                if (invoice.transactionId.Equals(transactionId))
                    return invoice;
            }
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


        private static string generateInvoiceNumberForPayPal()
        {
            return DateTime.Now.ToString("MMddssffff");
        }
    }
}