using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
  [Route("api/users/{userId}/photos")]
  [ApiController]
  public class PhotosController : ControllerBase
  {
    private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
    private readonly IMapper _mapper;
    private readonly IDatingRepository _repo;
    private Cloudinary _cloudinary;

    public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
    {
      _repo = repo;
      _mapper = mapper;
      _cloudinaryConfig = cloudinaryConfig;
      Account acc = new Account(
          _cloudinaryConfig.Value.CloudName,
          _cloudinaryConfig.Value.ApiKey,
          _cloudinaryConfig.Value.ApiSecret
      );

      _cloudinary = new Cloudinary(acc);

    }
    [HttpGet("{id}", Name = "GetPhoto")]
    public async Task<IActionResult> GetPhoto(int id)
    {
      var photoFromRepo = await _repo.GetPhoto(id);
      var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

      return Ok(photo);

    }

    [HttpPost]
    public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
    {
      // Authenticate the user, making sure that the user id matches the user who is logged in.  This may also be where you check against user role, for things that require admin privelages etc.
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      //Get the user, and get the file sent up from the client.
      var userFromRepo = await _repo.GetUser(userId);
      var file = photoForCreationDto.File;

      // This is a cloudinary class.
      var uploadResult = new ImageUploadResult();
      //Checking to see if the file has a length to it, then use .net's native file reading library to open a stream and read the file. Then upload it to cloudinary.
      if (file.Length > 0)
      {
        using (var stream = file.OpenReadStream())
        {
          var uploadParams = new ImageUploadParams()
          {
            File = new FileDescription(file.Name, stream),
            Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
          };
          uploadResult = _cloudinary.Upload(uploadParams);


        }
      }
      photoForCreationDto.Url = uploadResult.Uri.ToString();
      photoForCreationDto.PublicId = uploadResult.PublicId;
      photoForCreationDto.IsApproved = false;

      var photo = _mapper.Map<Photo>(photoForCreationDto);
      //Check to see if the user has a main photo.  If not, set this one to their main.
      if (!userFromRepo.Photos.Any(u => u.IsMain))
      {
        photo.IsMain = true;
      }
      userFromRepo.Photos.Add(photo);

      if (await _repo.SaveAll())
      {
        var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);

        // Returns an Http 201
        return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
      }
      return BadRequest("Coul not add the photo");
    }
    [HttpPost("{id}/setMain")]
    public async Task<IActionResult> SetMainPhoto(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var user = await _repo.GetUser(userId);

      if (!user.Photos.Any(p => p.Id == id))
      {
        return Unauthorized();
      }

      var photoFromRepo = await _repo.GetPhoto(id);
      if (photoFromRepo.IsMain)
      {
        return BadRequest("This is already the main photo");
      }
      var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
      currentMainPhoto.IsMain = false;
      photoFromRepo.IsMain = true;
      if (await _repo.SaveAll())
      {
        return NoContent();
      }
      return BadRequest("Could not set photo to mian");
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var user = await _repo.GetUser(userId);

      if (!user.Photos.Any(p => p.Id == id))
      {
        return Unauthorized();
      }

      var photoFromRepo = await _repo.GetPhoto(id);
      if (photoFromRepo.IsMain)
      {
        return BadRequest("You cannot delete your main photo");
      }

      if (photoFromRepo.PublicId != null)
      {

        var deleteParams = new DeletionParams(photoFromRepo.PublicId);
        var result = _cloudinary.Destroy(deleteParams);

        if (result.Result == "ok")
        {
          _repo.Delete(photoFromRepo);
        }
        if (await _repo.SaveAll())
        {
          return Ok();
        }

      }

      _repo.Delete(photoFromRepo);
      if (await _repo.SaveAll())
      {
        return Ok();
      }


      return BadRequest("Failed to delete the photo");


    }
  }
}