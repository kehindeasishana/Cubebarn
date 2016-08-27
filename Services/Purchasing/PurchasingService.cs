using Core.Data;
using Core.Domain;
using Core.Domain.Financials;
using Core.Domain.Items;
using Core.Domain.Purchases;
using Services.Financial;
using Services.Inventory;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

namespace Services.Purchasing
{
    public partial class PurchasingService :  IPurchasingService
    {
        private readonly IFinancialService _financialService;
        private readonly IInventoryService _inventoryService;

        private readonly IRepository<PurchaseOrderHeader> _purchaseOrderRepo;
        private readonly IRepository<PurchaseInvoiceHeader> _purchaseInvoiceRepo;
        private readonly IRepository<PurchaseReceiptHeader> _purchaseReceiptRepo;
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly IRepository<Account> _accountRepo;
        private readonly IRepository<Item> _itemRepo;
        private readonly IRepository<Measurement> _measurementRepo;
        private readonly IRepository<SequenceNumber> _sequenceNumberRepo;
        private readonly IRepository<VendorPayment> _vendorPaymentRepo;
       
        private readonly IRepository<PaymentTerm> _paymentTermRepo;
        private readonly IRepository<Bank> _bankRepo;

        public PurchasingService(IFinancialService financialService,
            IInventoryService inventoryService,
            IRepository<PurchaseOrderHeader> purchaseOrderRepo,
            IRepository<PurchaseInvoiceHeader> purchaseInvoiceRepo,
            IRepository<PurchaseReceiptHeader> purchaseReceiptRepo,
            IRepository<Vendor> vendorRepo,
            IRepository<Account> accountRepo,
            IRepository<Item> itemRepo,
            IRepository<Measurement> measurementRepo,
            IRepository<SequenceNumber> sequenceNumberRepo,
            IRepository<VendorPayment> vendorPaymentRepo,
            
            IRepository<PaymentTerm> paymentTermRepo,
            IRepository<Bank> bankRepo
            )
            
        {
            _financialService = financialService;
            _inventoryService = inventoryService;
            _purchaseOrderRepo = purchaseOrderRepo;
            _purchaseInvoiceRepo = purchaseInvoiceRepo;
            _purchaseReceiptRepo = purchaseReceiptRepo;
            _vendorRepo = vendorRepo;
            _accountRepo = accountRepo;
            _itemRepo = itemRepo;
            _measurementRepo = measurementRepo;
            _sequenceNumberRepo = sequenceNumberRepo;
            _vendorPaymentRepo = vendorPaymentRepo;
            
            _paymentTermRepo = paymentTermRepo;
            _bankRepo = bankRepo;
        }

        public void AddPurchaseInvoice(PurchaseInvoiceHeader purchaseIvoice, int? purchaseOrderId)
        {
            #region Auto create purchase order
            if (!purchaseOrderId.HasValue)
            {
                var po = new PurchaseOrderHeader()
                {
                    Date = purchaseIvoice.Date,
                    //No = GetNextNumber(SequenceNumberTypes.PurchaseOrder).ToString(),
                    Vendor = purchaseIvoice.Vendor,
                    VendorId = purchaseIvoice.VendorId.Value,
                    Description = purchaseIvoice.Description,
                };
                foreach (var line in purchaseIvoice.PurchaseInvoiceLines)
                {
                    var item = _itemRepo.GetById(line.ItemId);

                    po.PurchaseOrderLines.Add(new PurchaseOrderLine()
                    {
                        ItemId = item.Id,
                        MeasurementId = line.MeasurementId,
                        Quantity = line.Quantity,
                        Cost = item.Cost.Value,
                        Discount = line.Discount.HasValue ? line.Discount.Value : 0,
                        Amount = item.Cost.Value * line.Quantity,
                    });
                }
                purchaseIvoice.PurchaseOrders.Add(po);

                var poReceipt = new PurchaseReceiptHeader()
                {
                    Date = DateTime.Now,
                    Vendor = po.Vendor,
                    VendorId = po.VendorId.Value,
                    PurchaseOrderHeaderId = po.Id,
                };

                foreach (var line in purchaseIvoice.PurchaseInvoiceLines)
                {
                    poReceipt.PurchaseReceiptLines.Add(new PurchaseReceiptLine()
                    {
                        ItemId = line.ItemId,
                        MeasurementId = line.MeasurementId,
                        Quantity = line.Quantity,
                        ReceivedQuantity = (line.ReceivedQuantity.HasValue ? line.ReceivedQuantity.Value : 0),
                        Cost = line.Cost.Value,
                        Amount = line.Cost.Value * (line.ReceivedQuantity.HasValue ? line.ReceivedQuantity.Value : 0),
                    });
                }

                po.PurchaseReceipts.Add(poReceipt);

                AddPurchaseOrderReceipt(poReceipt);
            }
            #endregion


            decimal totalTaxAmount = 0, totalAmount = 0, totalDiscount = 0;
            var taxes = new List<KeyValuePair<int, decimal>>();

            foreach (var line in purchaseIvoice.PurchaseInvoiceLines)
            {
                var lineTaxes = _financialService.ComputeInputTax(purchaseIvoice.VendorId.Value, line.ItemId, line.Quantity, line.Cost.Value, decimal.Zero);

                var lineAmount = line.Quantity * line.Cost;

                var totalLineAmount = lineAmount + lineTaxes.Sum(t => t.Value);

                totalAmount += (decimal)totalLineAmount;
                
                foreach (var t in lineTaxes)
                    taxes.Add(t);
            }

            if (taxes != null && taxes.Count > 0)
            {
                var groupedTaxes = from t in taxes
                           group t by t.Key into grouped
                           select new
                           {
                               Key = grouped.Key,
                               Value = grouped.Sum(t => t.Value)
                           };

                totalTaxAmount = taxes.Sum(t => t.Value);

                //foreach (var tax in groupedTaxes)
                //{
                //    var tx = _financialService.GetTaxes().Where(t => t.Id == tax.Key).FirstOrDefault();
                    
                //}
            }

            if (totalDiscount > 0)
            {

            }

            Vendor vendor = _vendorRepo.GetById(purchaseIvoice.VendorId.Value);
           
        }

        public void AddPurchaseOrder(PurchaseOrderHeader purchaseOrder, bool toSave)
        {
            purchaseOrder.No =(SequenceNumberTypes.PurchaseOrder).ToString();
            
            if(toSave)
                _purchaseOrderRepo.Insert(purchaseOrder);
        }

        public void AddPurchaseOrderReceipt(PurchaseReceiptHeader purchaseOrderReceipt)
        {
           // var glLines = new List<GeneralLedgerLine>();
            decimal totalAmount = purchaseOrderReceipt.PurchaseReceiptLines.Sum(d => d.Amount);
            decimal taxAmount = purchaseOrderReceipt.GetTotalTax();
            decimal totalDiscount = 0;

            foreach (var lineItem in purchaseOrderReceipt.PurchaseReceiptLines)
            {
                var item = _itemRepo.GetById(lineItem.ItemId);
                //decimal lineItemTotalAmountAfterTax = lineItem.Amount - lineItem.LineTaxAmount;

                //lineItem.InventoryControlJournal = _inventoryService.CreateInventoryControlJournal(lineItem.ItemId,
                //    lineItem.MeasurementId,
                //    DocumentTypes.PurchaseReceipt,
                //    lineItem.ReceivedQuantity,
                //    null,
                //    lineItem.ReceivedQuantity * item.Cost,
                //    null);
            }

            if (taxAmount > 0)
            {
            }

            if (totalDiscount > 0)
            {
            }

          
        }

        public IEnumerable<Vendor> GetVendors()
        {
            var query = from f in _vendorRepo.Table
                        select f;
            return query.AsEnumerable();
        }

        public Vendor GetVendorById(int id)
        {
            return _vendorRepo.GetById(id);
        }

        public IEnumerable<PurchaseOrderHeader> GetPurchaseOrders()
        {
            var query = _purchaseOrderRepo.Table;

            return query.AsEnumerable();
        }

        public PurchaseOrderHeader GetPurchaseOrderById(int id)
        {
            return _purchaseOrderRepo.GetById(id);
        }

        public PurchaseReceiptHeader GetPurchaseReceiptById(int id)
        {
            return _purchaseReceiptRepo.GetById(id);
        }

        public void AddVendor(Vendor vendor)
        {
            vendor.AccountsPayableAccountId = _accountRepo.Table.Where(a => a.AccountCode == "20110").FirstOrDefault().Id;
            vendor.PurchaseAccountId = _accountRepo.Table.Where(a => a.AccountCode == "50200").FirstOrDefault().Id;
            vendor.PurchaseDiscountAccountId = _accountRepo.Table.Where(a => a.AccountCode == "50400").FirstOrDefault().Id;

            //vendor.IsActive = true;

            _vendorRepo.Insert(vendor);
        }

        public void UpdateVendor(Vendor vendor)
        {
            _vendorRepo.Update(vendor);
        }

        public IEnumerable<PurchaseInvoiceHeader> GetPurchaseInvoices()
        {
            var query = from purchInvoice in _purchaseInvoiceRepo.Table
                        select purchInvoice;
            return query.ToList();
        }

        public PurchaseInvoiceHeader GetPurchaseInvoiceById(int id)
        {
            return _purchaseInvoiceRepo.GetById(id);
        }

        public void SavePayment(int invoiceId, int vendorId, int accountId, decimal amount, DateTime date)
        {
            var payment = new VendorPayment()
            {
                VendorId = vendorId,
                PurchaseInvoiceHeaderId = invoiceId,
                Date = date,
                Amount = amount,
            };
            var vendor = GetVendorById(vendorId);
           
        }
    }
}
