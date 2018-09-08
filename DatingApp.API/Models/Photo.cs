using System;

namespace DatingApp.API.Models
{
  public class Photo
  {
    public int Id { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public DateTime DateAdded { get; set; }
    public bool IsMain { get; set; }
    //Now that we define the relationship both ways, we can perform a cascade delete.
    public User User { get; set; }  
    public int UserId { get; set; }
    public string PublicId { get; set; }
    public Boolean IsApproved { get; set; }
  }
}