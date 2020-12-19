using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("account")]
    public class Account
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        
        [Column("created_date"), Required]
        public string CreatedDate { set; get; }


        [Column("id_user"), Required]
        public int UserId { set; get; } 
        public User User { set; get; }
        
        
        public List<Purse> Purses { set; get; }
        public List<Currency> Currencies { set; get; }

    }
}