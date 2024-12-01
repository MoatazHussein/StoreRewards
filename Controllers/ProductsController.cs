using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreRewards.Data;
using StoreRewards.Models;
using StoreRewards.Services;
using StoreRewards.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace StoreRewards.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductsController : AppBaseController
    {

        private readonly AuthService _authService;
        private readonly IMailService _mailService;
        private readonly ImageService _imageService;

        public ProductsController(DataContext context, IMailService mailService,
            AuthService authService, ILogger<UsersController> logger, ImageService imageService) : base(context)
        {
            _mailService = mailService;
            _authService = authService;
            _imageService = imageService;

        }

        [HttpGet(nameof(GetProductById))]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                                .Include(p => p.Images) // Include images in the query
                                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet(nameof(GetAllProducts))]
        public async Task<IActionResult> GetAllProducts(int pageNumber = 1, int pageSize = 10)
        {
            var products = await _context.Products.Include(m => m.Images)
                                                    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                                                    .ToListAsync();

            var totalCount = await _context.Products.CountAsync();
            return Ok(new
            {
                Data = products,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto)
        {
            var saveStatus = new List<string>();

            foreach (var image in productDto.Images)
            {
                var imageValidationResult = await _imageService.ValidateImage(image);

                if (!imageValidationResult.Success)
                {
                    return BadRequest(new { msg = imageValidationResult.ErrorMessage });
                }
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Commission = productDto.Commission,
            };

            _context.Products.Add(product);
            var saveResult = await SaveChangesWithDetailedResultAsync();

            if (saveResult.Success)
            {
                saveStatus.Add("Product has been added");
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred with ID: {product.Id}", err = saveResult.ErrorMessage });
            }

            var imageSaveResult = await UploadProductImages(product.Id, productDto.Images);

            if (imageSaveResult.Success)
            {
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }

            else
            {
                return StatusCode(500, new { msg = $"Product has been added with error occurred with uploading images: {product.Id}", err = imageSaveResult.ErrorMessage });
            }
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductDto productDto)
        {
            var saveStatus = new List<string>();

            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productDto.Id);
            if (product == null)
                return NotFound();

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.Commission = productDto.Commission;

            var saveResult = await SaveChangesWithDetailedResultAsync();

            if (saveResult.Success)
            {
                saveStatus.Add("Updated successfully");
            }
            else
            {
                return StatusCode(500, new { msg = $"error occurred with ID: {product.Id}", err = saveResult.ErrorMessage });
            }

            // Update images
            if (productDto.Images is not null && productDto.Images.Count > 0)
            {
                foreach (var image in productDto.Images)
                {
                    var imageValidationResult = await _imageService.ValidateImage(image);

                    if (!imageValidationResult.Success)
                    {
                        return BadRequest(new { msg = imageValidationResult.ErrorMessage });
                    }
                }

                if (product is not null)
                {
                    await DeleteProductImages(product.Id);
                    product.Images.Clear();
                }

                var imageSaveResult = await UploadProductImages(product.Id, productDto.Images);

                if (imageSaveResult.Success)
                {
                    return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
                }

                else
                {
                    return StatusCode(500, new { msg = $"Product has been updated with error occurred with uploading images: {product.Id}", err = imageSaveResult.ErrorMessage });
                }

            }
            else
            {
                return Ok(new { msg = $"Updated successfully." });
            }

        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            var saveResult = await SaveChangesWithDetailedResultAsync();

            if (saveResult.Success)
            {
                return Ok(new { msg = $"Deleted successfully." });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting");
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<SaveResult> UploadProductImages(int productId, List<IFormFile> images)
        {
            foreach (var image in images)
            {
                var imageSaveResult = await _imageService.SaveImageAsync($"Products\\{productId}", image);

                if (!imageSaveResult.Success)
                {
                    return new SaveResult
                    {
                        Success = false,
                        ErrorMessage = imageSaveResult.ErrorMessage
                    };
                }

                var filePath = imageSaveResult.FilePath;

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    Url = filePath
                };
                _context.ProductImages.Add(productImage);
            }
            var saveResult = await SaveChangesWithDetailedResultAsync();
            if (saveResult.Success)
            {
                return new SaveResult
                {
                    Success = true,
                };
            }
            else
            {
                return new SaveResult
                {
                    Success = false,
                    ErrorMessage = saveResult.ErrorMessage
                };
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task DeleteProductImages(int productId)
        {
            var oldImages = await _context.ProductImages.Where(e => e.ProductId == productId)
                .Select(e => e.Url)
                .ToListAsync();

            foreach (var image in oldImages)
            {
                if (image is not null)
                {
                    await _imageService.DeleteImageAsync(image);
                }
            }
        }

    }

}
