using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.Expressions;
using System.Xml.Serialization;
using System.Xml;


namespace CSVkonventer.Models
{   

    public class InvoiceModel
    {
        private const string _UD = "UD";
        private const string _UN = "UN";
        private const string _UDSK = "UDzahrSl";

        public const decimal _TAX = 0.2m;


        private static readonly string[] _EUCOUNTRIES = { "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE", 
            "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SI", "ES", "SE", "GB" };


        public string id { get; set; }
        public string account_code { get; set; }
        public string account_name { get; set; }
        public string invoice_number { get; set; }
        public string subscription_id { get; set; }
        public string plan_code { get; set; }
        public string coupon_code { get; set; }
        private decimal _total;
        public decimal total
        {
            get { return _total; }
            set
            {
                _total = value;
                homeTotal = _rate == 0 ? _total : Math.Round(_total / _rate, 2);
                totalPrice = _total / (1 + _TAX);
                totalTax = _total - totalPrice;
            }
        }

        public decimal subtotal { get; set; }
        public string vat_amount { get; set; }
        public string currency { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string closed_at { get; set; }
        private string _purchaseCountry;

        public string purchase_country
        {
            get
            {
                return _purchaseCountry;
            }
            set
            {
                _purchaseCountry = value;
                setUdUnType();
            } 
        }
        public string _vat_number { get; set; }

        public string vat_number
        {
            get
            {
                return _vat_number;
            }
            set
            {
                _vat_number = value;
                setUdUnType();
            }
        }

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
        private decimal _rate;
        public decimal rate { 
            get { return _rate; }
            set
            {
                _rate = value;
                homeTotal = value == 0 ? _total : Math.Round(_total / value, 2);
            } 
        }
        public decimal homeTotal { get; set; }
        public decimal homeTax { get; set; }
        public decimal homePrice { get; set; }
        public decimal net { get; set; }
        public string transactionId { get; set; }

        public string company { get; set; }
        public string postal_code { get; set; }
        public string UD_UN { get; set; }




        public decimal totalTax { get; set; }
        public decimal totalPrice { get; set; }






        private void setUdUnType()
        {
            if (_purchaseCountry == "SK") { UD_UN = _UD; }
            else
                if (_EUCOUNTRIES.Contains(_purchaseCountry))
                {
                    if (vat_number != null && vat_number.Length > 1)
                    {
                        UD_UN = _UDSK;
                    }
                    else
                    {
                        UD_UN = _UD;
                    }
                }
                else
                {
                    UD_UN = _UN;
                }
        }
      
    }
}