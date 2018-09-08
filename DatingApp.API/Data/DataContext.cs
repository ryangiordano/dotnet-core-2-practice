using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> // Need to add configuration because we're using ints with our ids
  {
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    //Will designate the table name, so should be plural
    public DbSet<Value> Values { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    //Override the OnModelCreating method to give custom behavior when a model is created
    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder); //configures schema needed for identity framework

      builder.Entity<UserRole>(userRole =>
      {
        userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

        //Telling ef about the relationship that our role and user has with the userrole table, much like our likes and users
        userRole.HasOne(ur => ur.Role)
        .WithMany(r => r.UserRoles)
        .HasForeignKey(ur => ur.RoleId)
        .IsRequired();

        userRole.HasOne(ur => ur.User)
        .WithMany(r => r.UserRoles)
        .HasForeignKey(ur => ur.UserId)
        .IsRequired();

      });

      builder.Entity<Photo>().HasQueryFilter(photo=>photo.IsApproved);

      builder.Entity<Like>()
      .HasKey(k => new
      {
        k.LikerId,
        k.LikeeId
      });

      builder.Entity<Like>()
      .HasOne(u => u.Likee)
      .WithMany(u => u.Likers)
      .HasForeignKey(u => u.LikeeId)
      .OnDelete(DeleteBehavior.Restrict); // Do not have cascading deletion of a user

      builder.Entity<Like>()
        .HasOne(u => u.Liker)
        .WithMany(u => u.Likees)
        .HasForeignKey(u => u.LikerId)
        .OnDelete(DeleteBehavior.Restrict); // Do not have cascading deletion of a user

      builder.Entity<Message>()
        .HasOne(u => u.Sender)
        .WithMany(m => m.MessagesSent)
        .OnDelete(DeleteBehavior.Restrict);

      builder.Entity<Message>()
        .HasOne(u => u.Recipient)
        .WithMany(m => m.MessagesReceived)
        .OnDelete(DeleteBehavior.Restrict);
    }
  }
}