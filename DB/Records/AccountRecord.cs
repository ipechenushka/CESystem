using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("account")]
    public class AccountRecord
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }


        [Column("id_user"), Required]
        public int UserId { set; get; } 
        public UserRecord UserRecord { set; get; }
        
        
        public List<ConfirmRequestRecord> ConfirmRequestRecords { set; get; }
        public List<WalletRecord> WalletRecords { set; get; }
        public List<CurrencyRecord> CurrencyRecords { set; get; }
    }
}