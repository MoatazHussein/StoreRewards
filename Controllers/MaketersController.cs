using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreRewards.Data;
using StoreRewards.Models;
using StoreRewards.Services;
using StoreRewards.DTOs;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Sprache;

namespace StoreRewards.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class MaketersController : AppBaseController
    {

        private readonly AuthService _authService;
        private readonly IMailService _mailService;

        public MaketersController(DataContext context, IMailService mailService,
            AuthService authService, ILogger<UsersController> logger) : base(context)
        {
            _mailService = mailService;
            _authService = authService;
        }

        private GetMarketerDto MapMarketerData(Marketer marketer)
        {
            var marketerDto = new GetMarketerDto
            {
                UserId = marketer.UserId,
                BankName = marketer.BankName,
                IBAN = marketer.IBAN,
                SWIFTCode = marketer.SWIFTCode,
                ProductQuery = marketer.ProductQuery,
                User = new GetUserDto
                {
                    Id = marketer.UserId,
                    FirstName = marketer.User.FirstName,
                    LastName = marketer.User.LastName,
                    Gender = marketer.User.Gender.ToString(),
                    Email = marketer.User.Email,
                    MobileNo = marketer.User.MobileNo,
                    WhatsAppNo = marketer.User.WhatsAppNo,
                    Country = marketer.User.Country,
                    City = marketer.User.City,
                    Region = marketer.User.Region,
                    Birthday = marketer.User.Birthday,
                    ProfileImagePath = marketer.User.ProfileImagePath,
                }
            };

            return marketerDto;
        }

        [Authorize]
        [HttpGet(nameof(GetMarketer))]
        public async Task<IActionResult> GetMarketer()
        {
            // Retrieve the user ID from the JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var marketer = await _context.Marketers
                              .Include(m => m.User)
                              .FirstOrDefaultAsync(m => m.UserId == userId);

            if (marketer is null)
            {
                return NotFound();
            }
            var marketerDto = MapMarketerData(marketer);
            return Ok(marketerDto);
        }

        [HttpGet(nameof(GetAllMarketers))]
        public async Task<IActionResult> GetAllMarketers(int pageNumber = 1, int pageSize = 10)
        {
            var marketers = await _context.Marketers.Include(m => m.User)
                                                    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                                                    .ToListAsync();

            var marketerDtos = marketers.Select(m => MapMarketerData(m));

            var totalCount = await _context.Marketers.CountAsync();
            return Ok(new
            {
                Data = marketerDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpPost(nameof(AddMarketer))]
        public async Task<IActionResult> AddMarketer([FromBody] MarketerDto marketerDto)
        {
            var saveStatus = new List<string>();

            if (marketerDto is null)
            {
                return BadRequest(new { msg = "Marketer Data is required." });
            }

            if (_context.Users.Any(u => u.Id != marketerDto.UserId))
            {
                return BadRequest(new { msg = "User not exists." });
            }

            if (_context.Marketers.Any(m => m.UserId == marketerDto.UserId))
            {
                return BadRequest(new { msg = "This user is already a markter." });
            }

            if (_context.Marketers.Any(m => m.ProductQuery == marketerDto.ProductQuery))
            {
                return BadRequest(new { msg = "Product Link is already exists." });
            }

            var marketer = new Marketer
            {
                UserId = marketerDto.UserId,
                BankName = marketerDto.BankName,
                IBAN = marketerDto.IBAN,
                SWIFTCode = marketerDto.SWIFTCode,
                ProductQuery = marketerDto.ProductQuery,
            };

            _context.Marketers.Add(marketer);
            var marketerSaveResult = await SaveChangesWithDetailedResultAsync();

            if (marketerSaveResult.Success)
            {
                saveStatus.Add("Marketer successfully created");
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred while registering the marketer", err = marketerSaveResult.ErrorMessage });
            }

            var userRole = new UserRole
            {
                UserId = marketerDto.UserId,
                RoleId = 2
            };

            _context.UserRoles.Add(userRole);
            var userRoleSaveResult = await SaveChangesWithDetailedResultAsync();

            if (userRoleSaveResult.Success)
            {
                return Ok(new { msg = "the marketer has been added successfully" });
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred while registering the user role", err = userRoleSaveResult.ErrorMessage });
            }
        }

        [HttpPut(nameof(UpdateMarketer))]
        public async Task<IActionResult> UpdateMarketer([FromBody] MarketerDto marketerDto)
        {
            var marketer = await _context.Marketers.FirstOrDefaultAsync(p => p.UserId == marketerDto.UserId);

            if (marketer == null)  return BadRequest(new { msg = "user Not found as a marketer" });

            if (_context.Marketers.Any(m => m.ProductQuery == marketerDto.ProductQuery && m.UserId != marketerDto.UserId))
            {
                return BadRequest(new { msg = "Product Link is already exists." });
            }

            marketer.BankName = marketerDto.BankName;
            marketer.IBAN = marketerDto.IBAN;
            marketer.SWIFTCode = marketerDto.SWIFTCode;
            marketer.ProductQuery = marketerDto.ProductQuery;

            var saveResult = await SaveChangesWithDetailedResultAsync();

            if (saveResult.Success)
            {
                return Ok(new { msg = $"Updated successfully." });
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred with ID: {marketerDto.UserId}", err = saveResult.ErrorMessage });
            }

        }


    }

}
