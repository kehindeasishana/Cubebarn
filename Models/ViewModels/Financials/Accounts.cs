using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models.ViewModels.Financials
{
    public class Accounts
    {
        public Accounts()
        {
            AccountsListLines = new HashSet<AccountsListLine>();
        }

        public ICollection<AccountsListLine> AccountsListLines { get; set; }
    }

    public class AccountsListLine
    {
        public int Id { get; set; }
        public string AccountCode { get; set; }
        public string Description { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
    }

    public class EditAccountViewModel
    {
        public int Id { get; set; }
        [Required]
        public string AccountCode { get; set; }
        [Required]
        public string AccountName { get; set; }
        public string AccountClass { get; set; }
        public decimal Balance { get; set; }
        
    }

    public class AddAccountViewModel
    {
        public int Id { get; set; }
        [Required]
        public string AccountCode { get; set; }
        [Required]
        public string AccountName { get; set; }
        public int AccountClass { get; set; }
        public string Description { get; set; }

        public AddAccountViewModel()
        {
        }
    }
   
    public class AccountTransactionViewModel
    {
        public int Id { get; set; }
        public ICollection<Services.Financial.MasterGeneralLedger> Transactions { get; set; }
    }
}
