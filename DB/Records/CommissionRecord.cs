using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("commission")]
    public class CommissionRecord
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        
        [Column("id_user")]
        public int? UserId { set; get; }
        public UserRecord UserRecord { set; get; }

        [Column("id_currency")]
        public int? CurrencyId { set; get; }
        public CurrencyRecord CurrencyRecord { set; get; }

        [Column("transfer")]
        public float? TransferCommission { set; get; }
        
        [Column("withdraw")]
        public float? WithdrawCommission { set; get; }
        
        [Column("deposit")]
        public float? DepositCommission { set; get; }
        
        [Column("is_absolute_type")]
        public bool? IsAbsoluteType { set; get; }
        
        
    }
}