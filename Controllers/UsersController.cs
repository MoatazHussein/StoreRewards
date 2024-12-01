using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreRewards.Data;
using StoreRewards.Models;
using StoreRewards.Services;
using Microsoft.AspNetCore.Authorization;
using StoreRewards.DTOs;
using System.Security.Claims;

namespace StoreRewards.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : AppBaseController
    {

        private readonly AuthService _authService;
        private readonly IMailService _mailService;
        private readonly ImageService _imageService;

        public UsersController(DataContext context, IMailService mailService,
            AuthService authService, ILogger<UsersController> logger, ImageService imageService) : base(context)
        {
            _mailService = mailService;
            _authService = authService;
            _imageService = imageService;

        }

        [Authorize]
        [HttpGet(nameof(GetUserData))]
        public async Task<IActionResult> GetUserData()
        {
            // Retrieve the user ID from the JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var userId = int.Parse(userIdClaim.Value);

            // Fetch user data
             var user = await _context.Users
                                     .Where(n => n.Id == userId)
                                     .Select(e => new GetUserDto
                                     {
                                         Id = e.Id,
                                         FirstName = e.FirstName,
                                         LastName = e.LastName,
                                         Gender = e.Gender.ToString(),
                                         Email = e.Email,
                                         MobileNo = e.MobileNo,
                                         WhatsAppNo = e.WhatsAppNo,
                                         Country = e.Country,
                                         City = e.City,
                                         Region = e.Region,
                                         Birthday = e.Birthday,
                                         ProfileImagePath = e.ProfileImagePath,
                                     })
                                     .ToListAsync();
            if (user == null)
            {
                return NotFound(); // 404 Not Found
            }

            return Ok(user);
        }

        [HttpGet(nameof(GetAllUsersData))]
        [Authorize]
        public async Task<IActionResult> GetAllUsersData(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _context.Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(e => new GetUserDto
                                                                                              {
                                                                                                  Id = e.Id,
                                                                                                  FirstName = e.FirstName,
                                                                                                  LastName = e.LastName,
                                                                                                  Gender = e.Gender.ToString(),
                                                                                                  Email = e.Email,
                                                                                                  MobileNo = e.MobileNo,
                                                                                                  WhatsAppNo = e.WhatsAppNo,
                                                                                                  Country = e.Country,
                                                                                                  City = e.City,
                                                                                                  Region = e.Region,
                                                                                                  Birthday = e.Birthday,
                                                                                                  ProfileImagePath = e.ProfileImagePath,
                                                                                              })
                                                                                              .ToListAsync();
            var totalCount = await _context.Users.CountAsync();

            return Ok(new
            {
                Data = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            var saveStatus = new List<string>();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Gender == 0)
            {
                return BadRequest(new { msg = "User Gender is required." });
            }

            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { msg = "User already exists." });
            }

            _authService.CreatePasswordHash(request.Password,
                 out byte[] passwordHash,
                 out byte[] passwordSalt);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MobileNo = request.MobileNo,
                WhatsAppNo = request.WhatsAppNo,
                Country = request.Country,
                City = request.City,
                Region = request.Region,
                Gender = request.Gender,
                Birthday = request.Birthday,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = _authService.CreateRandomToken()
            };

            _context.Users.Add(user);

            var UserSaveResult = await SaveChangesWithDetailedResultAsync();

            if (UserSaveResult.Success)
            {
                saveStatus.Add("Inserted into users successfully");
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred while registering the user", err = UserSaveResult.ErrorMessage });
            }

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = request.RoleId
            };
            _context.UserRoles.Add(userRole);
            var userRoleSaveResult = await SaveChangesWithDetailedResultAsync();

            if (userRoleSaveResult.Success)
            {
                saveStatus.Add("Inserted into user roles successfully");
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred while registering the user role", err = userRoleSaveResult.ErrorMessage });
            }

            if (request.RoleId == 3)
            {
                await SendVerificationMail(user.Id);
                return Ok(new { msg = "User successfully created and Verification Mail has been send to the user" });
            }
            else {

                if (request.MarketerData is null)
                {
                    return BadRequest(new { msg = "Marketer Data is required." });

                }

                if (_context.Marketers.Any(u => u.ProductQuery == request.MarketerData.ProductQuery))
                {
                    return BadRequest(new { msg = "Product Link is already exists." });
                }

                var marketer = new Marketer
                {
                    UserId = user.Id,
                    BankName = request.MarketerData.BankName,
                    IBAN = request.MarketerData.IBAN,
                    SWIFTCode = request.MarketerData.SWIFTCode,
                    ProductQuery = request.MarketerData.ProductQuery,
                };

                _context.Marketers.Add(marketer);
                var marketerSaveResult = await SaveChangesWithDetailedResultAsync();

                if (marketerSaveResult.Success)
                {
                    saveStatus.Add("Inserted into Marketers successfully");
                    await SendVerificationMail(user.Id);
                    return Ok(new { msg = "User successfully created and Verification Mail has been send to the user" });
                }
                else
                {
                    return StatusCode(500, new { msg = $"error occurred while registering the marketer", err = marketerSaveResult.ErrorMessage });
                }
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            // Fetch the user with roles from the database
            var user = await GetUserWithRolesAsync(request.Email);

            if (user is null || user.Email is null)
            {
                return BadRequest(new { msg = "User not found." });
            }

            if (!_authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { msg = "Password is incorrect." });
            }

            if (!user.IsVerified)
            {
                return BadRequest(new { msg = "Not verified!" });
            }

            // Get roles from the database
            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();

            // Generate token with roles
            var token = _authService.GenerateToken(user.Id,user.Email, roles);
            return Ok(new { msg = $"Welcome back, {user.Email}! :)", userID = user.Id, Email = user.Email, roles = roles, Token = token });
        }

        [HttpPut("Update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto updatedUser)
        {
            if (updatedUser is null)
            {
                return BadRequest("User data is required.");
            }

            // Check if the user exists
            var existingUser = await _context.Users.FindAsync(updatedUser.Id);
            if (existingUser is null)
            {
                return NotFound();
            }

            // Update properties
            existingUser.FirstName = updatedUser.FirstName;
            existingUser.LastName = updatedUser.LastName;
            existingUser.MobileNo = updatedUser.MobileNo;
            existingUser.WhatsAppNo = updatedUser.WhatsAppNo;
            existingUser.Country = updatedUser.Country;
            existingUser.City = updatedUser.City;
            existingUser.Region = updatedUser.Region;
            existingUser.Birthday = updatedUser.Birthday;

            _context.Users.Update(existingUser);
            var saveResult = await SaveChangesWithDetailedResultAsync();

            if (saveResult.Success)
            {
                //return NoContent(); // Return 204 No Content
                return Ok(new { msg = $"Updated successfully." });
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred with ID: {updatedUser.Id}", err = saveResult.ErrorMessage });
            }

        }

        private async Task<User?> GetUserWithRolesAsync(string email)
        {
            return await  _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                return BadRequest(new { msg = "User not found." });
            }

            user.PasswordResetToken = _authService.CreateRandomToken(8);
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            await SendForgotPasswordMail(user.Id);

            return Ok(new { msg = "You may now reset your password." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user is null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest(new { msg = "Invalid Token." });
            }

            _authService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok(new { msg = "Password successfully reset." });
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user is null)
            {
                return BadRequest(new { msg = "Invalid token." });
            }

            user.IsVerified = true;
            user.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();


            string? Verification_Successful_URL = Environment.GetEnvironmentVariable("Verification_Successful_URL")?.ToString();

            if (Verification_Successful_URL is not null)
            {
                return Redirect(Verification_Successful_URL + "?token=" + token);
            }
            else
            {
                return Ok(new { msg = "User verified! :)" });
            }

        }
        private async Task SendVerificationMail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is not null)
            {
                var token = user.VerificationToken;
                var mail = user.Email;
                string? URL = Environment.GetEnvironmentVariable("Host")?.ToString();
                if (URL is not null && mail is not null)
                {
                    var verificationLink = $"{Environment.GetEnvironmentVariable("URL")}/api/Users/verify?token={token}";
                    var message = $"Please verify your email by clicking on the link: <a href='{verificationLink}'>Verify Email</a>";
                    await _mailService.SendEmailAsync(mail, "[StoreRewards] Verification Mail", message,null);
                }
            }
        }
        [HttpGet("IsVerified")]
        public async Task<IActionResult> IsVerified(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user is null)
            {
                return BadRequest(new { msg = "Invalid token." });
            }
            else
            {
                string status = "User is Not Verified";

                if (user.IsVerified)
                {
                    status = "User is already Verified";
                }

                return Ok(new { msg = status });
            }

        }
        private async Task SendForgotPasswordMail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is not null)
            {
                var mail = user.Email;
                if (mail is not null)
                {
                    var resetToken = user.PasswordResetToken;
                    var message = $"Please use the code below to reset your account password: <br> <b>{resetToken}</b>";
                    await _mailService.SendEmailAsync(mail, "[StoreRewards] Reset Password", message, null);
                }
            }
        }


        //[HttpPost("UploadProfileImage/{userId}")]
        [Authorize]
        [HttpPost("UploadProfileImage")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = _authService.GetUserId();

            if (userId == 0)
            {
                return Unauthorized("User ID not found");
            }

            // Retrieve the user ID from the JWT
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            //if (userIdClaim == null)
            //{
            //    return Unauthorized("User ID not found in token.");
            //}
            //var userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.ProfileImagePath is not null)
            {
                await _imageService.DeleteImageAsync(user.ProfileImagePath);
            }

            var ImageSaveResult = await _imageService.SaveImageAsync("Users", file);

            if (!ImageSaveResult.Success)
            {
                return BadRequest(new { msg = ImageSaveResult.ErrorMessage });
            }

            var filePath = ImageSaveResult.FilePath;

            // Update the user's profile image path
            user.ProfileImagePath = filePath;
            _context.Users.Update(user);
            var saveResult = await SaveChangesWithDetailedResultAsync();
            if (saveResult.Success)
            {
                return Ok(new { msg = "Profile image updated.", filePath });
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred while uploading the image", err = saveResult.ErrorMessage });
            }
        }



    }

}
