using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSVkonventer.Models
{
    public class InvoicesViewModel
    {
        public List<InvoiceModel> Invoices { get; set; }

        public int StartNumberingFrom { get; set; }

        public InvoicesViewModel(List<InvoiceModel> inv)
        {            
            if (inv != null)
                Invoices = inv;
            else
                Invoices = new List<InvoiceModel>();
            getCompleteInvoices();
        }

        public InvoicesViewModel(Dictionary<String,InvoiceModel> inv)
        {
            if (inv != null)
                Invoices = inv.Values.ToList();
            else
                Invoices = new List<InvoiceModel>();
            getCompleteInvoices();
        }

        private void getCompleteInvoices(){
            List<InvoiceModel> completeInvoices = new List<InvoiceModel>();
            foreach(InvoiceModel Invoice in Invoices){
                if (Invoice.line_item_start_date != null)
                {
                    completeInvoices.Add(Invoice);
                }
            }
            Invoices = completeInvoices;
        }
    }
}