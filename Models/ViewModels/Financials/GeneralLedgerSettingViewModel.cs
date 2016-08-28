using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.ViewModels.Financials
{
    public class GeneralLedgerSettingViewModel
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public int? PayableAccountId { get; set; }
        public int? PurchaseDiscountAccountId { get; set; }
        public int? GoodsReceiptNoteClearingAccountId { get; set; }
        public int? SalesDiscountAccountId { get; set; }
        public int? ShippingChargeAccountId { get; set; }
    }
}
