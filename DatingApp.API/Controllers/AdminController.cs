using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;
using AutoMapper;
using System.Collections.Generic;
using CloudinaryDotNet.Actions;
using DatingApp.API.Helpers;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;

namespace DatingApp.API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AdminController : ControllerBase
  {
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
    private readonly Cloudinary _cloudinary;
    public AdminController(IDatingRepository repo, DataContext context, UserManager<User> userManager, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
    {
      _cloudinaryConfig = cloudinaryConfig;

      Account acc = new Account(
    _cloudinaryConfig.Value.CloudName,
    _cloudinaryConfig.Value.ApiKey,
    _cloudinaryConfig.Value.ApiSecret
);

      _cloudinary = new Cloudinary(acc);
      _mapper = mapper;
      _repo = repo;
      _context = context;
      _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("usersWithRoles")]
    public async Task<IActionResult> GetUsersWithRoles()
    {
      var userList = await (from user in _context.Users
                            orderby user.UserName
                            select new
                            {
                              Id = user.Id,
                              UserName = user.UserName,
                              Roles = (from userRole in user.UserRoles
                                       join role in _context.Roles
                                       on userRole.RoleId
                                       equals role.Id
                                       select role.Name).ToList()

                            }).ToListAsync();
      return Ok(userList);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photosForModeration")]
    public async Task<IActionResult> GetPhotosForModeration()
    {
      var photosFromRepo = await _repo.GetUnapprovedPhotos();
      var photosToReturn = _mapper.Map<IEnumerable<PhotoForReturnDto>>(photosFromRepo);

      return Ok(photosToReturn);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("approvePhoto/{photoId}/{approved}")]
    public async Task<IActionResult> ApprovePhoto(int photoId, bool approved)
    {
      var photoToApprove = await _repo.GetPhoto(photoId);
      if (approved)
      {
        photoToApprove.IsApproved = true;
        if (await _repo.SaveAll())
        {
          return Ok();
        }
      }
      else
      {

        if (photoToApprove.PublicId != null)
        {

          var deleteParams = new DeletionParams(photoToApprove.PublicId);
          var result = _cloudinary.Destroy(deleteParams);

          if (result.Result == "ok")
          {
            _repo.Delete(photoToApprove);
          }
          if (await _repo.SaveAll())
          {
            return Ok();
          }

        }
        if (photoToApprove.PublicId == null)
        {
          _repo.Delete(photoToApprove);

        }
        return Ok();
      }
      return BadRequest("The photo could not be approved.");
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("editRoles/{userName}")]
    public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
    {
      var user = await _userManager.FindByNameAsync(userName);
      var userRoles = await _userManager.GetRolesAsync(user);

      var selectedRoles = roleEditDto.RoleNames;

      //user can be potentially removed from all roles
      //selectedRoles = selectedRoles != null ? selectedRoles : new string[] {}
      selectedRoles = selectedRoles ?? new string[] { };
      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

      if (!result.Succeeded)
      {
        return BadRequest("Failed to add to roles");
      }
      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if (!result.Succeeded)
      {
        return BadRequest("Failed to remove the roles");
      }

      return Ok(await _userManager.GetRolesAsync(user));

    }
  }
}