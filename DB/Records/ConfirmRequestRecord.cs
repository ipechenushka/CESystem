using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using CESystem.Controllers;

namespace CESystem.Models
{
    [Table("confirm_request")]
    public class ConfirmRequestRecord
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column("id_sender"), Required]
        public int SenderId { set; get; }
        public AccountRecord SenderAccountRecord { set; get; }
        
        [Column("id_recipient")]
        public int? RecipientId { set; get; }

        [Column("operation_type"), Required]
        public OperationType OperationType { set; get; }
        
        [Column("amount"), Required]
        public float Amount { set; get; }

        [Column("commission"), NotNull]
        public float Commission { set; get; }
        
        [Column("currency"), Required]
        public string Currency { set; get; }
        
        [Column("formation_date"), NotNull]
        public string FormationDate { set; get; }
        
        [Column("status"), Required]
        public RequestStatus Status { set; get; }
    }
}