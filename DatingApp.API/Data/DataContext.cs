using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    //Will designate the table name, so should be plural
    public DbSet<Value> Values { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Like> Likes { get; set; }
    //Override the OnModelCreating method to give custom behavior when a model is created
    protected override void OnModelCreating(ModelBuilder builder)
    {
      builder.Entity<Like>()
      .HasKey(k => new { k.LikerId, k.LikeeId });

      builder.Entity<Like>()
      .HasOne(u => u.Likee)
      .WithMany(u => u.Likers)
      .HasForeignKey(u=>u.LikeeId)
      .OnDelete(DeleteBehavior.Restrict); // Do not have cascading deletion of a user

      builder.Entity<Like>()
      .HasOne(u => u.Liker)
      .WithMany(u => u.Likees)
      .HasForeignKey(u=>u.LikerId)
      .OnDelete(DeleteBehavior.Restrict); // Do not have cascading deletion of a user
    }
  }
}