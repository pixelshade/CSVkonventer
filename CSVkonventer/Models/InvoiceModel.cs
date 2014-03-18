using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Xml;


namespace CSVkonventer.Models
{   

    public class InvoiceModel
    {        
        public string id { get; set; }
        public string account_code { get; set; }
        public string account_name { get; set; }
        public string invoice_number { get; set; }
        public string subscription_id { get; set; }
        public string plan_code { get; set; }
        public string coupon_code { get; set; }
        public decimal total { get; set; }
        public decimal subtotal { get; set; }
        public string vat_amount { get; set; }
        public string currency { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string closed_at { get; set; }
        public string purchase_country { get; set; }
        public string vat_number { get; set; }
        public decimal line_item_total { get; set; }
        public string line_item_description { get; set; }
        public string line_item_origin { get; set; }
        public string line_item_product_code { get; set; }
        public string line_item_accounting_code { get; set; }
        public string line_item_start_date { get; set; }
        public string line_item_end_date { get; set; }
        public string net_terms { get; set; }
        public string po_number { get; set; }
        public string collection_method { get; set; }
        public string line_item_uuid { get; set; }
        public string email { get; set; }
        public string email_to { get; set; }
        public string type { get; set; }
        public string city { get; set; }
        public string street { get; set; }       
        public decimal rate { get; set; }
        public decimal homeTotal { get; set; }
        public decimal homeTax { get; set; }
        public decimal homePrice { get; set; }

        public string company { get; set; }
        public string postal_code { get; set; }
        public string UD_UN { get; set; }




        public decimal totalTax { get; set; }

        public decimal totalPrice { get; set; }
    }
}