using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("wallet")]
    public class WalletRecord
    {
        [Column("id_account"), Required]
        public int IdAccount { set; get; }
        public AccountRecord AccountRecord { set; get; }
        
        [Column("id_currency"), Required]
        public int IdCurrency{ set; get; }
        public CurrencyRecord CurrencyRecord { set; get; }


        [Column("cash_value")] 
        public float CashValue { set; get; }
        
    }
}