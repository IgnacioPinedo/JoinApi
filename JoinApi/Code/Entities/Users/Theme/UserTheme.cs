using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

[Table("tb_UserTheme")]
public class UserTheme
{
    [Key]
    public int Id { get; private set; }
    public string ThemeName { get; set; }
}