using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace CSVkonventer.Models
{
    public class RatesHistoryModel
    {
        private string year;
        private Dictionary<String, RateModel> ratesHistory;


        private Dictionary<String, RateModel> downloadHistoryForYear(string year)
        {
            int yearBefore = (Convert.ToInt32(year))-1;
            Dictionary<string, RateModel> history = new Dictionary<string, RateModel>();
            // toto iba v januari ked nahodou treba pre nejaky den na ktory neni kurz v tom roku
            //string url = "http://www.nbs.sk/sk/statisticke-udaje/kurzovy-listok/mesacne-kumulativne-a-rocne-prehlady-kurzov/mesacne-kumulativne-a-rocne-prehlady-kurzov-detail/_3/4/" + yearBefore + "-01-01";
            //XDocument doc = XDocument.Load(url);
            //parseXMLDocToHistory(doc, history);
            string url2 = "http://www.nbs.sk/sk/statisticke-udaje/kurzovy-listok/mesacne-kumulativne-a-rocne-prehlady-kurzov/mesacne-kumulativne-a-rocne-prehlady-kurzov-detail/_3/4/" + year + "-01-01";           
            XDocument doc2 = XDocument.Load(url2);
            parseXMLDocToHistory(doc2, history);

            return history;
        }

        private void parseXMLDocToHistory(XDocument doc, Dictionary<string, RateModel> history)
        {
            var cubes = doc.Descendants("Cube")
                .Where(x => x.Attribute("currency") != null && (string)x.Attribute("rate").Value != "")
                .Select(x => new
                {
                    Currency = (string)x.Attribute("currency"),
                    Rate = (decimal)x.Attribute("rate"),
                    Time = (string)x.Parent.Attribute("time")
                });

            foreach (var result in cubes)
            {
                RateModel tempRateForDate;
                if (history.TryGetValue(result.Time, out tempRateForDate))
                {
                    tempRateForDate.AddCurrencyRate(result.Currency, result.Rate);
                }
                else
                {
                    tempRateForDate = new RateModel();
                    tempRateForDate.AddCurrencyRate(result.Currency, result.Rate);
                    history.Add(result.Time, tempRateForDate);
                }

                //  System.Diagnostics.Debug.WriteLine("{0}: {1} [ {2} ]", result.Currency, result.Rate, result.Time);
            }
        }


        public decimal getRateForDate(string date, string currency)
        {
            decimal rate = 0;
            string downloadYear = date.Substring(0, 4);
            if (ratesHistory == null || year != downloadYear)
            {
                ratesHistory = downloadHistoryForYear(downloadYear);
            }
            if (ratesHistory != null && ratesHistory.Count > 0)
            {
                SortedDictionary<string, RateModel> sortedRatesHistory = new SortedDictionary<string, RateModel>(ratesHistory);
                string minimumDate = sortedRatesHistory.First().Key;
                if (sortedRatesHistory.ContainsKey(date))
                {
                    rate = sortedRatesHistory[date].GetRateForCurrency(currency);
                }
                else
                {
                    DateTime minimumDateTime = Convert.ToDateTime(minimumDate);
                    DateTime checkDate = Convert.ToDateTime(date);
                    while (checkDate > minimumDateTime && !sortedRatesHistory.ContainsKey(checkDate.ToString("yyyy-MM-dd")))
                    {
                        DateTime oldDate = Convert.ToDateTime(date);
                        checkDate = checkDate.AddDays(-1);
                    }
                    string dat = checkDate.ToString("yyyy-MM-dd");
                    rate = sortedRatesHistory[dat].GetRateForCurrency(currency);
                }
            }
            return rate;
        }
    }

    public class RateModel
    {
        private Dictionary<string, decimal> rates { get; set; }

        public RateModel()
        {
            rates = new Dictionary<string, decimal>();
        }

        public void AddCurrencyRate(string currency, decimal rate)
        {
            rates.Add(currency, rate);
        }

        public decimal GetRateForCurrency(string currency)
        {
            if (rates.ContainsKey(currency))
            {
                return rates[currency];
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CHYYYYYYYBa pri " + currency);
                return rates[currency];
            }

        }
    }
}