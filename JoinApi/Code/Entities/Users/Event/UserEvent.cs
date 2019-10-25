using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

[Table("tb_UserEvent")]
public class UserEvent
{
    [Key]
    public int Id { get; private set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
}