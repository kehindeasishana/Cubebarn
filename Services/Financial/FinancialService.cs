using Core.Data;
using Core.Domain;
using Core.Domain.Financials;
using Core.Domain.Items;
using Core.Domain.Purchases;
using Core.Domain.Sales;
//using Core.Domain.TaxSystem;
using Services.Inventory;
using Services.TaxSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Financial
{
    public partial class FinancialService : IFinancialService
    {
        private readonly IInventoryService _inventoryService;
        private readonly ITaxService _taxService;
        private readonly IRepository<Account> _accountRepo;
        private readonly IRepository<AccountClass> _accountclassRepo;
        private readonly IRepository<Tax> _taxRepo;
        private readonly IRepository<AccountSubCategory> _accountSubCategory;
        private readonly IRepository<JournalEntryLine> _journalEntryLineRepo;
        private readonly IRepository<CompanySetUp> _company;
        private readonly IRepository<FinancialYear> _fiscalYearRepo;
        //private readonly IRepository<TaxGroup> _taxGroupRepo;
       // private readonly IRepository<ItemTaxGroup> _itemTaxGroupRepo;
        private readonly IRepository<PaymentTerm> _paymentTermRepo;
        private readonly IRepository<Bank> _bankRepo;
        private readonly IRepository<Item> _itemRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Vendor> _vendorRepo;
       

        public FinancialService(
           
            IRepository<Account> accountRepo,
            IRepository<AccountClass> accountclassRepo,
           IRepository<CompanySetUp> company,
            IRepository<JournalEntryLine> journalEntryLineRepo,
            IRepository<FinancialYear> fiscalYearRepo,
           IRepository<Tax> taxRepo,
            IRepository<Bank> bankRepo,
            IRepository<AccountSubCategory> accountSubCategory
           
            )
            
        {
            _accountSubCategory = accountSubCategory;
            _accountRepo = accountRepo;
            _accountclassRepo = accountclassRepo;
            
            _journalEntryLineRepo = journalEntryLineRepo;
            
            _fiscalYearRepo = fiscalYearRepo;
            
            _bankRepo = bankRepo;
            _taxRepo = taxRepo;
           
        }
        public ICollection<Tax> GetAllTaxes(bool includeInActive)
        {
            var query = from f in _taxRepo.Table
                        select f;
            return query.ToList();
        }
        public IEnumerable<Tax> GetAllTax()
        {
            var query = from f in _taxRepo.Table
                        select f;
            return query.AsEnumerable();
        }
        public IEnumerable<Bank> Banks()
        {
            var query = from f in _bankRepo.Table
                        select f;
            return query.ToList();
        }
        public void AddBank(Bank bank)
        {
            _bankRepo.Insert(bank);
        }
        public void AddNewTax(Tax tax)
        {
            _taxRepo.Insert(tax);
        }

        public void UpdateTax(Tax tax)
        {
            _taxRepo.Update(tax);
        }

        public void DeleteTax(int id)
        {
            throw new System.NotImplementedException();
        }

        public void AddCompany(CompanySetUp company)
        {
            _company.Insert(company);

        }
        public void InitializeCompany()
        {
            if (_company.Table.FirstOrDefault() == null)
            {
                //DbInitializerHelper.Initialize();
            }
        }

        public CompanySetUp GetDefaultCompany()
        {
            return _company.Table.ToList().FirstOrDefault();
        }

        public ICollection<CompanySetUp> GetCompany()
        {
            var query = from f in _company.Table
                        select f;
            return query.ToList();
        }

        public ICollection<FinancialYear> GetFinancialYears()
        {
            var query = from f in _fiscalYearRepo.Table
                        select f;
            return query.ToList();
        }
        public FinancialYear CurrentFiscalYear()
        {
            var query = (from fy in _fiscalYearRepo.Table
                        where fy.IsActive == true
                        select fy).FirstOrDefault();

            return query;
        }

        public void AddFiscalYear(FinancialYear financialYear)
        {
            _fiscalYearRepo.Insert(financialYear);
            
        }
        public IEnumerable<Account> GetAccounts()
        {
            var query = from f in _accountRepo.Table
                        select f;
            return query.AsEnumerable();
        }
        public void AddJournalEntry(JournalEntryLine journalEntry)
        {
            //journalEntry.Posted = false;

            _journalEntryLineRepo.Insert(journalEntry);
        }
        public IEnumerable<JournalEntryLine> GetJournalEntries()
        {
            var query = from je in _journalEntryLineRepo.Table
                        select je;
            return query.AsEnumerable();
        }

        public JournalEntryLine GetJournalEntry(int id)
        {
            //if (fromGL)
              //  return _journalEntryLineRepo.Table.Where(je => je.AccountId == id).FirstOrDefault();
            return _journalEntryLineRepo.Table.Where(je => je.Id == id).FirstOrDefault();
        }
        public Account GetAccount(int id)
        {
            return _accountRepo.Table.Where(a => a.Id == id).FirstOrDefault();
        }
        
        public Account GetAccountByAccountCode(string accountcode)
        {
            return _accountRepo.Table.Where(a => a.AccountCode == accountcode).FirstOrDefault();
        }

      
        //public IEnumerable<Tax> GetTaxes()
        //{
        //    var query = from f in _taxRepo.Table
        //                select f;
        //    return query.AsEnumerable();
        //}

        //public IEnumerable<ItemTaxGroup> GetItemTaxGroups()
        //{
        //    var query = from f in _itemTaxGroupRepo.Table
        //                select f;
        //    return query;
        //}

        //public IEnumerable<TaxGroup> GetTaxGroups()
        //{
        //    var query = from f in _taxGroupRepo.Table
        //                select f;
        //    return query;
        //}

    
        public ICollection<TrialBalance> TrialBalance(DateTime? from = default(DateTime?), DateTime? to = default(DateTime?))
        { 
            var allDr = (from dr in _journalEntryLineRepo.Table.AsEnumerable()
                         where dr.DrCr == DrOrCrSide.Dr

                         group dr by new { dr.AccountId, dr.Account.AccountCode, dr.Account.AccountName, dr.Amount } into tb
                         select new
                         {
                             AccountId = tb.Key.AccountId,
                             AccountCode = tb.Key.AccountCode,
                             AccountName = tb.Key.AccountName,
                             Debit = tb.Sum(d => d.Amount),
                         });

            var allCr = (from cr in _journalEntryLineRepo.Table.AsEnumerable()
                         where cr.DrCr == DrOrCrSide.Cr
                         
                         group cr by new { cr.AccountId, cr.Account.AccountCode, cr.Account.AccountName, cr.Amount } into tb
                         select new
                         {
                             AccountId = tb.Key.AccountId,
                             AccountCode = tb.Key.AccountCode,
                             AccountName = tb.Key.AccountName,
                             Credit = tb.Sum(c => c.Amount),
                         });

            var allDrcr = (from x in allDr
                           select new TrialBalance
                           {
                               AccountId = x.AccountId,
                               AccountCode = x.AccountCode,
                               AccountName = x.AccountName,
                               Debit = x.Debit,
                               Credit = (decimal)0,
                           }
                          ).Concat(from y in allCr
                                   select new TrialBalance
                                   {
                                       AccountId = y.AccountId,
                                       AccountCode = y.AccountCode,
                                       AccountName = y.AccountName,
                                       Debit = (decimal)0,
                                       Credit = y.Credit,
                                   });

            var sortedList = allDrcr
                .OrderBy(tb => tb.AccountCode)
                .ToList()
                .Reverse<TrialBalance>();

            var accounts = sortedList.ToList().GroupBy(a => a.AccountCode)
                .Select(tb => new TrialBalance
                {
                    AccountId = tb.First().AccountId,
                    AccountCode = tb.First().AccountCode,
                    AccountName = tb.First().AccountName,
                    Credit = tb.Sum(x => x.Credit),
                    Debit = tb.Sum(y => y.Debit)
                }).ToList();
            return accounts;
        }

        public ICollection<BalanceSheet> BalanceSheet(DateTime? from = default(DateTime?), DateTime? to = default(DateTime?))
        {
            var assets = from a in _accountRepo.Table
                         where a.AccountClassId == 1 
                         select a;
            var liabilities = from a in _accountRepo.Table
                              where a.AccountClassId == 2 
                              select a;
            var equities = from a in _accountRepo.Table
                           where a.AccountClassId == 3 
                           select a;
            

            var balanceSheet = new HashSet<BalanceSheet>();
            foreach (var asset in assets)
            {
                balanceSheet.Add(new BalanceSheet()
                {
                    AccountId = asset.Id,
                    AccountClassId = asset.AccountClassId,
                    
                    AccountCode = asset.AccountCode,
                    AccountName = asset.AccountName,
                    Amount = asset.Balance
                });
            }
            foreach (var liability in liabilities)
            {
                balanceSheet.Add(new BalanceSheet()
                {
                    AccountId = liability.Id,
                    AccountClassId = liability.AccountClassId,
                    AccountCode = liability.AccountCode,
                    AccountName = liability.AccountName,
                    Amount = liability.Balance
                });
            }
            foreach (var equity in equities)
            {
                balanceSheet.Add(new BalanceSheet()
                {
                    AccountId = equity.Id,
                    AccountClassId = equity.AccountClassId,
                    AccountCode = equity.AccountCode,
                    AccountName = equity.AccountName,
                    Amount = equity.Balance 
                });
            }
            return balanceSheet;
        }

        public ICollection<IncomeStatement> IncomeStatement(DateTime? from = default(DateTime?), DateTime? to = default(DateTime?))
        {
            var revenues = from r in _accountRepo.Table
                           where r.AccountClassId == 4
                           select r;

            var expenses = from e in _accountRepo.Table
                           where e.AccountClassId == 5
                           select e;
            
            var revenues_expenses = new HashSet<IncomeStatement>();
            foreach (var revenue in revenues)
            {
                revenues_expenses.Add(new IncomeStatement()
                {
                    AccountId = revenue.Id,
                    
                    AccountCode = revenue.AccountCode,
                    AccountName = revenue.AccountName,
                    Amount = revenue.Balance ,
                    IsExpense = false
                });
            }
            foreach (var expense in expenses)
            {
                revenues_expenses.Add(new IncomeStatement()
                {
                    AccountId = expense.Id,
                    AccountCode = expense.AccountCode,
                    AccountName = expense.AccountName,
                    Amount = expense.Balance ,
                    IsExpense = true
                });
            }
            return revenues_expenses;
        }
         public ICollection<ProfitAndLoss> ProfitAndLoss(DateTime? from = default(DateTime?), DateTime? to = default(DateTime?))
        {
            var revenues = from r in _accountRepo.Table
                           where r.AccountClassId == 4
                           select r;
            //var cost = from x in _accountSubCategory.Table
            //           where x.Accounts.AccountName == "Cost of Sales"
            //           select x;
            var cost = from x in _accountSubCategory.Table
                       where (x.Accounts.AccountName == "Cost of Sales"||
                              x.Accounts.AccountName == "Cost of Goods Sold"||
                              x.Accounts.AccountName == "Cost")
                       select x;
            var expenses = from e in _accountRepo.Table
                           where (e.AccountClassId == 5 && 
                                  (e.AccountName != "Cost of Sales" || e.AccountName != "Cost of Goods Sold" || e.AccountName != "Cost"))
                           select e;
            
            var revenues_expenses = new HashSet<ProfitAndLoss>();
            foreach (var revenue in revenues)
            {
                revenues_expenses.Add(new ProfitAndLoss()
                {
                    AccountId = revenue.Id,
                    //AccountSubCategory = revenue.SubCategory.ToString(),
                    //AccountCode = revenue.AccountCode,
                    AccountName = revenue.AccountName,
                    Amount = revenue.Balance,
                    IsExpense = false
                });
            }
            foreach (var expense in expenses)
            {
                revenues_expenses.Add(new ProfitAndLoss()
                {
                    AccountId = expense.Id,
                    //AccountCode = expense.AccountCode,
                    AccountName = expense.AccountName,
                    Amount = expense.Balance,
                    IsExpense = true
                });
            }
           // return revenues_expenses;

            foreach (var expense in cost)
            {
                revenues_expenses.Add(new ProfitAndLoss()
                {
                    AccountId = expense.Id,
                    AccountSubCategory = expense.AccountSubCategoryName,
                    AccountName = expense.Accounts .AccountName,
                    Amount = expense.Accounts.Balance,
                    IsExpense = true
                });
            }
            return revenues_expenses;
        }
        public new IEnumerable<Bank> GetCashAndBanks()
        {
            var query = from b in _bankRepo.Table
                        select b;
            return query;
        }

        /// <summary>
        /// Input VAT is the value added tax added to the price when you purchase goods or services liable to VAT. If the buyer is registered in the VAT Register, the buyer can deduct the amount of VAT paid from his/her settlement with the tax authorities. 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="quantity"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, decimal>> ComputeInputTax(int vendorId, int itemId, decimal quantity, decimal amount, decimal discount)
        {
            decimal taxAmount = 0, amountXquantity = 0, discountAmount = 0, subTotalAmount = 0;

            var taxes = new List<KeyValuePair<int, decimal>>();
            var item = _inventoryService.GetItemById(itemId);

            amountXquantity = amount * quantity;

            if (discount > 0)
                discountAmount = (discount / 100) * amountXquantity;

            subTotalAmount = amountXquantity - discountAmount;

            //var intersectionTaxes = _taxService.GetIntersectionTaxes(itemId, vendorId, Core.Domain.PartyTypes.Vendor);

            //foreach (var tax in intersectionTaxes)
            //{
            //    taxAmount = subTotalAmount - (subTotalAmount / (1 + (tax.Rate / 100)));
            //    taxes.Add(new KeyValuePair<int, decimal>(tax.Id, taxAmount));
            //}

            return taxes;
        }

        /// <summary>
        /// Output VAT is the value added tax you calculate and charge on your own sales of goods and services
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="quantity"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, decimal>> ComputeOutputTax(int customerId, int itemId, decimal quantity, decimal amount, decimal discount)
        {
            decimal taxAmount = 0, amountXquantity = 0, discountAmount = 0, subTotalAmount = 0;

            var item = _itemRepo.GetById(itemId);
            var customer = _customerRepo.GetById(customerId);
            var taxes = new List<KeyValuePair<int, decimal>>();

            amountXquantity = amount * quantity;

            if(discount > 0)
                discountAmount = (discount / 100) * amountXquantity;

            subTotalAmount = amountXquantity - discountAmount;

            //var intersectionTaxes = _taxService.GetIntersectionTaxes(itemId, customerId, Core.Domain.PartyTypes.Customer);

            //foreach (var tax in intersectionTaxes)
            //{
            //    taxAmount = subTotalAmount - (subTotalAmount / (1 + (tax.Rate / 100)));
            //    taxes.Add(new KeyValuePair<int, decimal>(tax.Id, taxAmount));
            //}

            return taxes;
        }

        public void UpdateAccount(Account account)
        {
           
            _accountRepo.Update(account);
        }
      
        public void AddAccount(Account account)
        {
            _accountRepo.Insert(account);
        }
        
        public void AddAccountClass(AccountClass accountClass)
        {
            _accountclassRepo.Insert(accountClass);
        }
       
       public void AddAccountSubCategory(AccountSubCategory accountSubCategory)
        {
            _accountSubCategory.Insert(accountSubCategory);
        }

       public void EditAccountSubCategory(AccountSubCategory accountSubCategory)
       {
           _accountSubCategory.Update(accountSubCategory);
       }
       public AccountSubCategory GetSubCategoryById(int id)
       {
           return _accountSubCategory.Table.Where(a => a.Id == id).FirstOrDefault();
       }
       public IEnumerable<AccountSubCategory> ListAccountSubCategory()
       {
           var query = from f in _accountSubCategory.Table
                       select f;
           return query.AsEnumerable();
       }
       public IEnumerable<AccountClass> ViewAccountClass()
       {
           var query = from f in _accountclassRepo.Table
                       select f;
           return query.AsEnumerable();
       }
        
       //public void ListAccountSubCategory()
       //{
       //    _accountSubCategory.Table.ToList();
       //}
       //public void GetSubCategoryById(object id)
       //{
       //    _accountSubCategory.GetById(id);
       //}
        public void UpdateJournalEntry(JournalEntryLine journalEntry)
        {
            //if (posted)
            //{
            //    journalEntry.Posted = posted;

               
                    var glEntry = new JournalEntryLine()
                    {
                        Date = DateTime.Now
                        
                    };

               
            //}

            _journalEntryLineRepo.Update(journalEntry);

            //var glEntry = _generalLedgerRepository.Table.Where(gl => gl.Id == journalEntry.GeneralLedgerHeaderId).FirstOrDefault();

            //glEntry.Date = journalEntry.Date;

            //foreach (var je in journalEntry.JournalEntryLines)
            //{
            //    if (glEntry.GeneralLedgerLines.Any(l => l.AccountId == je.AccountId))
            //    {
            //        var existingLine = glEntry.GeneralLedgerLines.Where(l => l.AccountId == je.AccountId).FirstOrDefault();
            //        existingLine.Amount = je.Amount;
            //        existingLine.DrCr = je.DrCr;
            //    }
            //    else
            //    {
            //        glEntry.GeneralLedgerLines.Add(new GeneralLedgerLine()
            //        {
            //            AccountId = je.AccountId,
            //            DrCr = je.DrCr,
            //            Amount = je.Amount,
            //        });
            //    }
            //}

            //if (ValidateGeneralLedgerEntry(glEntry) && glEntry.ValidateAccountingEquation())
            //{
            //    journalEntry.GeneralLedgerHeader = glEntry;
            //    _journalEntryRepo.Update(journalEntry);
            //}
        }

        public JournalEntryLine CloseAccountingPeriod()
        {
            /*
            Example:

            The following example shows the closing entries based on the adjusted trial balance of Company A.

            Note	
            
            Date	        Account	                Debit	        Credit
            1	Jan 31	    Service Revenue	        85,600	
                            Income Summary		                    85,600
            2	Jan 31	    Income Summary	        77,364	
                            Wages Expense		                    38,200
                            Supplies Expense	                    18,480
                            Rent Expense		                    12,000
                            Miscellaneous Expense	                3,470
                            Electricity Expense		                2,470
                            Telephone Expense		                1,494
                            Depreciation Expense	                1,100
                            Interest Expense		                150
            3	Jan 31	    Income Summary	        8,236	
                            Retained Earnings		                8,236
            4	Jan 31	    Retained Earnings	    5,000	
                            Dividend		                        5,000
            
            Notes

            1. Service revenue account is debited and its balance it credited to income summary account. If a business has other income accounts, for example gain on sale account, then the debit side of the first closing entry will also include the gain on sale account and the income summary account will be credited for the sum of all income accounts.
            2. Each expense account is credited and the income summary is debited for the sum of the balances of expense accounts. This will reduce the balance in income summary account.
            3. Income summary account is debited and retained earnings account is credited for the an amount equal to the excess of service revenue over total expenses i.e. the net balance in income summary account after posting the first two closing entries. In this case $85,600 − $77,364 = $8,236. Please note that, if the balance in income summary account is negative at this stage, this closing entry will be opposite i.e. debit to retained earnings and credit to income summary.
            4. The last closing entry transfers the dividend or withdrawal account balance to the retained earnings account. Since dividend and withdrawal accounts are contra to the retained earnings account, they reduce the balance in the retained earnings.
            */


            var journalEntry = new JournalEntryLine();
            journalEntry.Memo = "Closing entries";
            journalEntry.Date = DateTime.Now;
            journalEntry.Posted = false;
            

            return journalEntry;
        }
    }
}
