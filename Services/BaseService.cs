using Core.Data;
using Core.Domain;
using Core.Domain.Financials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public abstract class BaseService
    {
        private readonly IRepository<SequenceNumber> _sequenceNumberRepo;
      
        private readonly IRepository<PaymentTerm> _paymentTermRepo;
        private readonly IRepository<Bank> _bankRepo;

        internal BaseService(IRepository<SequenceNumber> sequenceNumberRepo,
           
            IRepository<PaymentTerm> paymentTermRepo,
            IRepository<Bank> bankRepo)
        {
            _sequenceNumberRepo = sequenceNumberRepo;
            
            _paymentTermRepo = paymentTermRepo;
            _bankRepo = bankRepo;
        }

        protected int GetNextNumber(SequenceNumberTypes type)
        {
            int nextNumber = 1;
            var sequence = (from n in _sequenceNumberRepo.Table
                            where n.SequenceNumberType == type
                            select n).FirstOrDefault();
            if (sequence == null)
            {
                sequence = new SequenceNumber();
                sequence.Description = Enum.GetName(typeof(SequenceNumberTypes), type);
                sequence.UsePrefix = false;
                sequence.SequenceNumberType = type;
                sequence.NextNumber = nextNumber + 1;
                _sequenceNumberRepo.Insert(sequence);
            }
            else
            {
                nextNumber = sequence.NextNumber;
                sequence.NextNumber += 1;
                _sequenceNumberRepo.Update(sequence);
            }
            return nextNumber;
        }

       
        protected IEnumerable<Bank> GetCashAndBanks()
        {
            var query = from b in _bankRepo.Table
                        select b;
            return query;
        }
    }
}
