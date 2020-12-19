using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("currency")]
    public class Currency
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        
        [Column("name"), Required]
        public string Name { set; get; }
        
        [Column("transfer_commission")]
        public double? TransferCommission { set; get; }
        
        [Column("withdraw_commission")]
        public double? WithdrawCommission { set; get; }
        
        [Column("deposit_commission")]
        public double? DepositCommission { set; get; }
        
        [Column("absolute_commission_status")]
        public bool? IsAbsoluteCommissionValue { set; get; }
        
        [Column("upper_commission_limit")]
        public double? UpperCommissionLimit { set; get; } 
        
        [Column("lower_commission_limit")]
        public double? LowerCommissionLimit { set; get; }
        
        [Column("confirm_limit")]
        public double? ConfirmCommissionLimit { set; get; } 
        
        public List<Purse> Purses { set; get; }
        public List<Account> Accounts { set; get; }
    }
}