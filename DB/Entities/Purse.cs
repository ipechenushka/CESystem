using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("purse")]
    public class Purse
    {
        [Column("id_account"), Required]
        public int IdAccount { set; get; }
        public Account Account { set; get; }
        
        [Column("id_currency"), Required]
        public int IdCurrency{ set; get; }
        public Currency Currency { set; get; }


        [Column("cash_value")] 
        public double CashValue { set; get; }
        
        [Column("transfer_commission")]
        public double? TransferCommission { set; get; }
        
        [Column("withdraw_commission")]
        public double? WithdrawCommission { set; get; }
        
        [Column("deposit_commission")]
        public double? DepositCommission { set; get; }
        
        [Column("absolute_commission_status")]
        public bool? IsAbsoluteCommissionValue { set; get; }
    }
}