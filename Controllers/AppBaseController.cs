using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreRewards.Data;
using StoreRewards.DTOs;

namespace StoreRewards.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AppBaseController : ControllerBase
    {

        protected readonly DataContext _context;

        public AppBaseController(DataContext context)
        {
            _context = context;
        }

        protected async Task<SaveResult> SaveChangesWithDetailedResultAsync()
        {
            SaveResult result = new()
            {
                Success = false
            };

            try
            {
                // Check if there are any changes in the context
                var hasChanges = _context.ChangeTracker.HasChanges();

                // If changes were saved
                int changes = await _context.SaveChangesAsync();

                result.Success = changes > 0  || !hasChanges;
                result.ErrorMessage = result.Success ? null : "No changes were made to the database.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                result.ErrorMessage = $"DbUpdateConcurrencyException: {ex.Message}";
            }
            catch (DbUpdateException ex)
            {
                result.ErrorMessage = $"DbUpdateException: {ex.Message}";
            }
            catch (InvalidOperationException ex)
            {
                result.ErrorMessage = $"InvalidOperationException: {ex.Message}";
            }
            catch (ArgumentException ex)
            {
                result.ErrorMessage = $"ArgumentException: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return result;
        }


    }
}
