using System;
using System.Collections.Generic;
using System.Text;

namespace Jenmar.FunctionApp.SQLDB.Receive.Models
{
    public class SMIG_ProductSalesHistory
    {
        public int sig_rowid { get; set; }
        public string sig_businessunitid { get; set; }
        public string sig_businessunitname { get; set; }
        //public string sig_BusinessUnit_odata_bind { get; set; }
        public string sig_orderid { get; set; }
        public string sig_ordernumber { get; set; }
        public string sig_doctorname { get; set; }
        public string sig_postalcode { get; set; }
        public string sig_accountname { get; set; }
        public string sig_product { get; set; }
        public string sig_department { get; set; }
        public decimal sig_units { get; set; }
        public decimal sig_remakeunits { get; set; }
        public decimal sig_totalcharge { get; set; }
        public decimal sig_totaltax { get; set; }
        public decimal sig_remakedollars { get; set; }
        public string sig_remakereason { get; set; }
        public string sig_manufacturername { get; set; }
        public string sig_doctorid { get; set; }
        public string sig_accountid { get; set; }
        public decimal sig_productioncost { get; set; }
        public string sig_salespersonid { get; set; }
        public string sig_salesperson { get; set; }
        public string sig_reportgroupname { get; set; }
        public string sig_corporatename { get; set; }
        public string sig_workflow { get; set; }
        public string sig_groupname { get; set; }
        public string sig_producttype { get; set; }
        public string sig_routename { get; set; }
        public string sig_accountadjustmentid { get; set; }
        public string sig_datecreated { get; set; }
        public string sig_dateinvoiced { get; set; }

    }
}
