using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESystem.Models
{
    [Table("users")]
    public class User
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column("name"), Required]
        public string Name { set; get; }
        
        [Column("password"), Required]
        public string Password { set; get; }
        
        [Column("role"), Required]
        public string Role { set; get; }
        
        
        public List<Account> Accounts { set; get; }
    }
}