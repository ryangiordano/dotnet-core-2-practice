using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.API.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Username { get; set; }
    public byte[] PasswordHash { get; set; }
    [Column("PasswordSalt")]
    public byte[] PasswordSalt { get; set; }
  }
}

