using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CESystem.Models
{
    [Table("users")]
    public class UserRecord
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column("name"), Required]
        public string Name { set; get; }
        
        [Column("password"), Required]
        public string Password { set; get; }
        
        [Column("role"), Required]
        public string Role { set; get; }
        
        [Column("current_account"), NotNull]
        public int CurrentAccount { set; get; }
        
        [Column("created_date"), Required]
        public string CreatedDate { set; get; }

        public List<AccountRecord> AccountRecords { set; get; }
        public CommissionRecord CommissionRecord { set; get; }
        public List<OperationsHistoryRecord> OperationsHistoryRecords { set; get; }
    }
}