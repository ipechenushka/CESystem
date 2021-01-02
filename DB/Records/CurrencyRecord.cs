using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("currency")]
    public class CurrencyRecord
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        
        [Column("name"), Required]
        public string Name { set; get; }
        
        
        [Column("upper_commission_limit")]
        public float? UpperCommissionLimit { set; get; } 
        
        [Column("lower_commission_limit")]
        public float? LowerCommissionLimit { set; get; }
        
        [Column("confirm_limit")]
        public float? ConfirmCommissionLimit { set; get; } 
        
        public CommissionRecord CommissionRecord { set; get; }
        public List<WalletRecord> WalletRecords { set; get; }
        public List<AccountRecord> AccountRecords { set; get; }
    }
}