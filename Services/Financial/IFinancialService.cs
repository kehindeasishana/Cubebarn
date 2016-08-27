using Core.Domain;
using Core.Domain.Financials;
//using Core.Domain.TaxSystem;
using System;
using System.Collections.Generic;

namespace Services.Financial
{
    public partial interface IFinancialService
    {
        ICollection<Tax> GetAllTaxes(bool includeInActive);
        IEnumerable<Tax> GetAllTax();
        void AddCompany(CompanySetUp company);
        void AddNewTax(Tax tax);
        void UpdateTax(Tax tax);
        void DeleteTax(int id);
        void AddBank(Bank bank);
        IEnumerable<Bank> Banks();
        
        void InitializeCompany();
       
        CompanySetUp GetDefaultCompany();
       
        ICollection<FinancialYear> GetFinancialYears();
        IEnumerable<Account> GetAccounts();
        IEnumerable<JournalEntryLine> GetJournalEntries();
        void AddJournalEntry(JournalEntryLine journalEntry);
        ICollection<TrialBalance> TrialBalance(DateTime? from = null, DateTime? to = null);
        ICollection<BalanceSheet> BalanceSheet(DateTime? from = null, DateTime? to = null);
        ICollection<IncomeStatement> IncomeStatement(DateTime? from = null, DateTime? to = null);
        ICollection<ProfitAndLoss> ProfitAndLoss(DateTime? from = null, DateTime? to = null);
        //ICollection<MasterGeneralLedger> MasterGeneralLedger(DateTime? from = null, DateTime? to = null, string accountCode = null, int? transactionNo = null);
        FinancialYear CurrentFiscalYear();
        //IEnumerable<Tax> GetTaxes();
        //IEnumerable<ItemTaxGroup> GetItemTaxGroups();
        //IEnumerable<TaxGroup> GetTaxGroups();
        IEnumerable<Bank> GetCashAndBanks();
        List<KeyValuePair<int, decimal>> ComputeInputTax(int vendorId, int itemId, decimal quantity, decimal amount, decimal discount);
        List<KeyValuePair<int, decimal>> ComputeOutputTax(int customerId, int itemId, decimal quantity, decimal amount, decimal discount);
        
        //void AddMainContraAccountSetting(int masterAccountId, int contraAccountId);
        void UpdateAccount(Account account);
        //JournalEntryLine GetJournalEntry(int id, bool fromGL = false);
        void UpdateJournalEntry(JournalEntryLine journalEntry);
        JournalEntryLine GetJournalEntry(int id);
        Account GetAccountByAccountCode(string accountcode);
        Account GetAccount(int id);
        IEnumerable<AccountSubCategory> ListAccountSubCategory();
        IEnumerable<AccountClass> ViewAccountClass();
        AccountSubCategory GetSubCategoryById(int id);
        void EditAccountSubCategory(AccountSubCategory accountSubCategory);
        void AddAccountSubCategory(AccountSubCategory accountSubCategory);
        void AddAccountClass(AccountClass accountClass);
        void AddAccount(Account account);
        void AddFiscalYear(FinancialYear financialYear);
       // void UpdateFinancialYear(FinancialYear financialYear);
    }
}
