using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSVkonventer.Models
{
    public class RecurlyCsvExporter
    {

        public static Dictionary<string, InvoiceModel> CreateRecurlyInvoices(List<string> contentOfFiles, String warningMessage)
        {
            RatesHistoryModel ratesHistory = new RatesHistoryModel();
            Dictionary<String, InvoiceModel> invoices = new Dictionary<string, InvoiceModel>();

            String invoicesRecurly = getBasicInvoicesFromFiles(contentOfFiles);
            String accountsRecurly = getAccountsFromFiles(contentOfFiles);
            String billingsRecurly = getBillingsFromFiles(contentOfFiles);

            if (invoicesRecurly == null) { warningMessage += "Nepodarilo sa najst invoices subor! "; return null; };
            if (accountsRecurly == null) { warningMessage += "Nepodarilo sa najst accounts subor! "; return null; };
            if (billingsRecurly == null) { warningMessage += "Nepodarilo sa najst billings subor! "; return null; };

            string[] lines = CSVSplitter.SplitCsvToLines(invoicesRecurly);
            for (int i = 1; i < lines.Length; ++i)
            {
                var cells = CSVSplitter.SplitCsvLineToCells(lines[i]);
                InvoiceModel invoice = new InvoiceModel();
                invoice = recurlyBasicInvoiceFromCells(invoice, cells);

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

            lines = CSVSplitter.SplitCsvToLines(accountsRecurly);
            for (int i = 1; i < lines.Length; ++i)
            {
                var cells = CSVSplitter.SplitCsvLineToCells(lines[i]);
                var acc_id = cells[0];

                InvoiceModel invoiceWithSameAccID = getInvoiceFromInvoicesByAccountId(invoices, acc_id);

                if (invoiceWithSameAccID != null)
                {
                    addAccountInfoToInvoiceFromCells(invoiceWithSameAccID, cells);
                }
            }

            lines = CSVSplitter.SplitCsvToLines(billingsRecurly);
            for (int i = 1; i < lines.Length; ++i)
            {
                var cells = CSVSplitter.SplitCsvLineToCells(lines[i]);
                var acc_id = cells[0];

                InvoiceModel invoiceWithSameAccID = getInvoiceFromInvoicesByAccountId(invoices, acc_id);

                if (invoiceWithSameAccID != null)
                {
                    addBillingInfoToInvoiceFromCells(invoiceWithSameAccID, cells);
                }
            }
            return invoices;
        }

        private static void addAccountInfoToInvoiceFromCells(InvoiceModel invoice, string[] cells)
        {
            invoice.company = cells[3].Replace("&", "&amp;"); ;
        }

        private static void addBillingInfoToInvoiceFromCells(InvoiceModel invoice, string[] cells)
        {
            invoice.city = cells[6];
            invoice.street = cells[4];
            //invoice.vat_number = cells[16];
            invoice.postal_code = cells[8];
        }

        private static InvoiceModel recurlyBasicInvoiceFromCells(InvoiceModel invoice, string[] cells)
        {
            if (invoice == null)
            {
                invoice = new InvoiceModel();
            }
            invoice.id = cells[0];
            invoice.account_code = cells[1];
            invoice.account_name = cells[2];
            invoice.invoice_number = cells[3];
            invoice.subscription_id = cells[4];
            invoice.plan_code = cells[5];
            invoice.coupon_code = cells[6];
            invoice.total = Convert.ToDecimal(cells[7].Replace('.', ','));
            invoice.subtotal = Convert.ToDecimal(cells[8].Replace('.', ','));
            invoice.vat_amount = cells[9];
            invoice.currency = cells[10];
            invoice.date = cells[11].Substring(0, 10);
            invoice.status = cells[12];

            if (invoice.status.Equals("open"))
            {
                return null;
            }

            invoice.closed_at = cells[13].Length > 10 ? cells[13].Substring(0, 10) : invoice.date;
            invoice.purchase_country = cells[14];
            invoice.vat_number = cells[15];
            invoice.line_item_total = Convert.ToDecimal(cells[16].Replace('.', ','));
            invoice.line_item_description = cells[17];
            invoice.line_item_origin = cells[18];
            invoice.line_item_product_code = cells[19];
            invoice.line_item_accounting_code = cells[20];
            invoice.line_item_start_date = cells[21].Substring(0, 10);
            invoice.line_item_end_date = cells[22].Substring(0, 10);
            invoice.net_terms = cells[23];
            invoice.po_number = cells[24];
            invoice.collection_method = cells[25];
            invoice.line_item_uuid = cells[26];

            return invoice;
        }

        private static String getBasicInvoicesFromFiles(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (CSVSplitter.getCSVFormat(fileContent) == CSVSplitter._RECURLY_BASIC)
                {
                    return fileContent;
                }
            }
            return null;
        }

        private static InvoiceModel getInvoiceFromInvoicesByAccountId(Dictionary<String, InvoiceModel> invoices, string acc_id)
        {
            if (invoices == null) return null;
            foreach (InvoiceModel invoice in invoices.Values)
            {
                if (invoice.account_code.Equals(acc_id))
                    return invoice;
            }
            return null;
        }

        private static String getAccountsFromFiles(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (CSVSplitter.getCSVFormat(fileContent) == CSVSplitter._RECURLY_ACCOUNTS)
                {
                    return fileContent;
                }
            }
            return null;
        }

        private static String getBillingsFromFiles(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (CSVSplitter.getCSVFormat(fileContent) == CSVSplitter._RECURLY_BILLINGS)
                {
                    return fileContent;
                }
            }
            return null;
        }
    }
}