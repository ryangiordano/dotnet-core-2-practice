using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))]
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    public UsersController(IDatingRepository repo, IMapper mapper)
    {
      _mapper = mapper;
      _repo = repo;

    }
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
    {
      var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
      var userFromRepo = await _repo.GetUser(currentUserId);

      userParams.UserId = currentUserId;

      if (String.IsNullOrEmpty(userParams.Gender))
      { // Checking if it's nullorempty because they can specify in the querystring in the url what gender they're looking for
        userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male"; // SJW Todo: allow user to specify, otherwise return all?
      }
      var users = await _repo.GetUsers(userParams);
      var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

      //Add pagination to the response headers.  Because we're inside an API controller, we have access to the response.
      Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
      return Ok(usersToReturn);
    }
    [HttpGet("{id}", Name = "GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
      User user;
      var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
      if (currentUserId == id)
      {
        user = await _repo.GetUserWithAllPhotos(id);
      }
      else
      {
        user = await _repo.GetUser(id);
      }
      var userToReturn = _mapper.Map<UserForDetailedDto>(user);
      return Ok(userToReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
    {
      // Token verification with id
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var userFromRepo = await _repo.GetUser(id);

      _mapper.Map(userForUpdateDto, userFromRepo);
      if (await _repo.SaveAll())
      {
        return NoContent();
      }
      throw new Exception($"Updating user {id} failed on save");


    }
    [HttpPost("{id}/like/{recipientId}")]
    public async Task<IActionResult> LikeUser(int id, int recipientId)
    {
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var like = await _repo.GetLike(id, recipientId);
      if (like != null)
      {
        return BadRequest("You already like this user.");
      }
      if (await _repo.GetUser(recipientId) == null)
      {
        return BadRequest("Not found");
      }

      //Create a new like between the liker and the likee.
      like = new Like
      {
        LikerId = id,
        LikeeId = recipientId
      };
      // Synchronous, not saving to the DB, just saving into memory
      _repo.Add<Like>(like);
      if (await _repo.SaveAll())
      {
        return Ok();
      }
      return BadRequest("Failed to like user");
    }
  }
}