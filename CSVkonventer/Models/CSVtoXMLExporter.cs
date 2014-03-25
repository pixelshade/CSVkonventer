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
        private const string _PAYPAL = "PaypalCSV";
        private const string _RECURLY_BASIC = "RecurlyBasicCSV";
        private const string _RECURLY_ACCOUNTS = "RecurlyAccountsCSV";
        private const string _RECURLY_BILLINGS = "RecurlyBillingsCSV";
        private const string _UNKNOWN = "UknownTypeCSV";

        private const string _UD = "UD";
        private const string _UN = "UN";
        private const string _UDSK = "UDzahrSl";

        private const decimal _TAX = 0.2m;

        
        private static string[] _EUCOUNTRIES = { "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE", 
                                                  "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SI", "ES", "SE", "GB" };
        public static string warningMessage { get; set; }

        public static Dictionary<string, InvoiceModel> ReadCSVAddToInvoices(List<string> contentOfFiles)
        {
            RatesHistoryModel ratesHistory = new RatesHistoryModel();
            Dictionary<String, InvoiceModel> invoices = new Dictionary<string, InvoiceModel>();

            if (contentOfFiles.Count != 3) { warningMessage += "Nedostatocny pocet suborov! "; return null; }

            String invoicesRecurly = getBasicInvoicesFromFiles(contentOfFiles);
            String accountsRecurly = getAccountsFromFiles(contentOfFiles);
            String billingsRecurly = getBillingsFromFiles(contentOfFiles);

            if (invoicesRecurly == null) { warningMessage += "Nepodarilo sa najst invoices subor! "; return null; };
            if (accountsRecurly == null) { warningMessage += "Nepodarilo sa najst accounts subor! "; return null; };
            if (billingsRecurly == null) { warningMessage += "Nepodarilo sa najst billings subor! "; return null; };
            
            string[] lines = SplitCsvToLines(invoicesRecurly);            
            for(int i = 1; i < lines.Length; ++i){                
                var cells = SplitCsvLineToCells(lines[i]);
                InvoiceModel invoice = new InvoiceModel();            
                invoice = recurlyBasicInvoiceFromCells(invoice,cells);
                
                if ((invoice!=null) && (invoice.line_item_total >= 0))
                {
                    invoice.rate = ratesHistory.getRateForDate(invoice.date, invoice.currency);
                    //invoice.homeTotal = GethomeTotalFrom(invoice.line_item_total, invoice.rate);
                    invoice.homeTotal = GethomeTotalFrom(invoice.total, invoice.rate);
                    invoice.homeTax = invoice.homeTotal * _TAX;
                    invoice.homePrice = invoice.homeTotal - invoice.homeTax;
                    invoices.Add(invoice.id, invoice);
                }
            }

            lines = SplitCsvToLines(accountsRecurly);
            for (int i = 1; i < lines.Length; ++i){
                var cells = SplitCsvLineToCells(lines[i]);
                var acc_id = cells[0];

                InvoiceModel invoiceWithSameAccID = getInvoiceFromInvoicesByAccountId(invoices, acc_id);

                if (invoiceWithSameAccID!=null)
                {
                    addAccountInfoToInvoiceFromCells(invoiceWithSameAccID,cells);
                }
            }

            lines = SplitCsvToLines(billingsRecurly);
            for (int i = 1; i < lines.Length; ++i){
                var cells = SplitCsvLineToCells(lines[i]);
                var acc_id = cells[0];

                InvoiceModel invoiceWithSameAccID = getInvoiceFromInvoicesByAccountId(invoices, acc_id);

                if (invoiceWithSameAccID!=null)
                {
                    addBillingInfoToInvoiceFromCells(invoiceWithSameAccID, cells);
                }
            }            
            return invoices;
        }

        private static InvoiceModel getInvoiceFromInvoicesByAccountId(Dictionary<String, InvoiceModel> invoices ,string acc_id)
        {
            if(invoices!=null){
                foreach(InvoiceModel invoice in invoices.Values){
                    if(invoice.account_code.Equals(acc_id))
                        return invoice;
                }                
            }
            return null;
        }


        private static String getBasicInvoicesFromFiles(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (getCSVFormat(fileContent) == _RECURLY_BASIC)
                {
                    return fileContent;
                }
            }
            return null;
        }

        private static String getAccountsFromFiles(List<String> contentOfFiles)
        {
            foreach (String fileContent in contentOfFiles)
            {
                if (getCSVFormat(fileContent) == _RECURLY_ACCOUNTS)
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
                if (getCSVFormat(fileContent) == _RECURLY_BILLINGS)
                {
                    return fileContent;
                }
            }
            return null;
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
            invoice.totalTax = invoice.total / (_TAX + 1);
            invoice.totalPrice = invoice.total - invoice.totalTax;
            invoice.subtotal = Convert.ToDecimal(cells[8].Replace('.', ','));
            invoice.vat_amount = cells[9];
            invoice.currency = cells[10];
            invoice.date = cells[11].Substring(0, 10);
            invoice.status = cells[12];

            if (!invoice.status.Equals("closed"))
            {
                return null;
            }

            invoice.closed_at = cells[13].Substring(0, 10);
            invoice.purchase_country = cells[14];
            invoice.vat_number = cells[15];
            invoice.line_item_total = Convert.ToDecimal(cells[16].Replace('.', ','));
            invoice.line_item_description = cells[17];
            invoice.line_item_origin = cells[18];
            invoice.line_item_product_code = cells[19];
            invoice.line_item_accounting_code = cells[20];
            invoice.line_item_start_date = cells[21].Substring(0, 10);
            invoice.line_item_end_date = cells[21].Substring(0, 10);
            invoice.net_terms = cells[23];
            invoice.po_number = cells[24];
            invoice.collection_method = cells[25];
            invoice.line_item_uuid = cells[26];

            setUD_UNType(invoice);
                        
            return invoice;
        }

        private static void setUD_UNType(InvoiceModel invoice)
        {
            List<String> euCountries = new List<string>(_EUCOUNTRIES);
            if (invoice.purchase_country == "SK") { invoice.UD_UN = _UD; } else 
            if (euCountries.Contains(invoice.purchase_country))
            {
                if (invoice.vat_number !=null && invoice.vat_number.Length > 1){ 
                    invoice.UD_UN = _UDSK; 
                } else { 
                    invoice.UD_UN = _UD; 
                }
            }
            else
            {
                invoice.UD_UN = _UN;
            }
        }


        private static InvoiceModel paypalInvoiceFromCells(string[] cells)
        {
            InvoiceModel invoice = new InvoiceModel();
            invoice.invoice_number = generateInvoiceNumberForPayPal();
            invoice.id = cells[12]; //Transaction ID                    
            invoice.account_name = cells[3]; //name
            invoice.email = cells[10];
            invoice.email_to = cells[11];
            invoice.total = Convert.ToDecimal(cells[7]); //gross <---------asi net
            invoice.subtotal = Convert.ToDecimal(cells[9]); //asi Net?                    
            invoice.currency = cells[6]; //currency
            string[] date = cells[0].Split('/');
            invoice.date = date[2] + '-' + date[0] + '-' + date[1];//date
            invoice.closed_at = invoice.date;
            invoice.status = cells[5];//status
            invoice.purchase_country = cells[14]; //address status                    
            invoice.line_item_total = Convert.ToDecimal(cells[7]); // ?? Net ? Gross?
            invoice.line_item_description = cells[15]; //title 
            invoice.line_item_product_code = cells[16];  //product code                 
            invoice.type = cells[4];  //type   
            return invoice;
        }

        private static string generateInvoiceNumberForPayPal()
        {
            return DateTime.Now.ToString("MMddssffff");
        }

        private static decimal GethomeTotalFrom(decimal total, decimal tax)
        {
            if (tax == 0) return 0;
            decimal home = Math.Round(total / tax, 2);
            return home;
        }

        private static string getCSVFormat(string firstLine)
        {
            if (firstLine.StartsWith("Date,")) { return _PAYPAL; }
            if (firstLine.StartsWith("id,")) { return _RECURLY_BASIC; }
            if (firstLine.StartsWith("account_code,account_email,account_username")) { return _RECURLY_ACCOUNTS; }
            if (firstLine.StartsWith("account_code,first_name,last_name")) { return _RECURLY_BILLINGS; }
            else return _UNKNOWN;
        }

        private static string[] SplitCsvToLines(string csv, char delimeter = '\n')
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

        private static string[] SplitCsvLineToCells(string line, char delimeter = ',')
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