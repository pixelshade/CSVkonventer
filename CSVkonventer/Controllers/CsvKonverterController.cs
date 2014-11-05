using CSVkonventer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CSVkonventer.Controllers
{
    public class CsvKonverterController : Controller
    {
        //
        // GET: /CsvKonverter/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateXml(IEnumerable<HttpPostedFileBase> files, int startNumber = 0)
        {
            bool validInputFiles = false;
            Dictionary<string,InvoiceModel> invoices;
            List<String> contentOfFiles = new List<string>();            
            if (ModelState.IsValid && files != null)
            {                
                foreach(var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Server.MapPath("~/App_Data/" + fileName);
                        file.SaveAs(path);
                        contentOfFiles.Add(System.IO.File.ReadAllText(path));
                        System.IO.File.Delete(path);
                    }   
                }

                if (contentOfFiles.Count == 0)
                {
                    CSVtoXMLExporter.WarningMessage += "Neposlali ste ziaden subor";
                    return this.RedirectToAction("Warning");
                }
                invoices = CSVtoXMLExporter.ReadCSVAddToInvoices(contentOfFiles);
                validInputFiles = (invoices != null) ;
                InvoicesViewModel invoicesView = new InvoicesViewModel(invoices);
                invoicesView.StartNumberingFrom = startNumber;

                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

                if (validInputFiles)
                {
                    Response.ContentEncoding = Encoding.GetEncoding("Windows-1250");//new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    Response.ContentType = "text/xml";
                    Response.AddHeader("Content-Disposition", "attachment; filename=\"Pohoda-winf-" + CSVtoXMLExporter.ExportType + "-(" + System.DateTime.Now + ").xml\"");
                    return View(invoicesView);
                }
                else
                {
                    return this.RedirectToAction("Warning");
                }
            
            }

            
            return this.RedirectToAction("Index");
        }

        public ActionResult Warning(string warning)
        {
            ViewBag.WarningMessage = CSVtoXMLExporter.WarningMessage;
            return View();
        }
    }
}

