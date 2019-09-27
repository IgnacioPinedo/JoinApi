using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

[Table("TB_UserSession")]
public class UserSession
{
    [Key]
    public int Id { get; private set; }
    public int UserId { get; set; }
    public string SessionToken { get; set; }
    public DateTime ExpireDate { get; set; }
}