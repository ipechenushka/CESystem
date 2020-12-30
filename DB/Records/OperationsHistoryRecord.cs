using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using CESystem.Controllers;

namespace CESystem.Models
{

    [Table("operations_history")]
    public class OperationsHistoryRecord
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        
        [Column("id_user"), Required]
        public int UserId { set; get; }
        public UserRecord UserRecord { set; get; }
        
        
        [Column("id_account"), NotNull]
        public int AccountId { set; get; }
        public AccountRecord AccountRecord { set; get; }
        
        [Column("type"), NotNull]
        public OperationType Type { set; get; }
        
        [Column("amount"), NotNull]
        public float Sum { set; get; }

        [Column("commission"), NotNull]
        public float Commission { set; get; }

        [Column("date"), NotNull]
        public string Date { set; get; }
        
        
        
    }
}