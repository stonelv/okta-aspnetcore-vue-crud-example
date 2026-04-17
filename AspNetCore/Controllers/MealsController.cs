using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class MealsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public MealsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<Meal>>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string keyword = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string sortBy = "Date",
            [FromQuery] bool sortDesc = true)
        {
            var userId = GetCurrentUserId();
            var query = _dbContext.Meals.Where(m => m.UserId == userId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m => m.Name.Contains(keyword));
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.Date <= endDate.Value);
            }

            var totalCount = await query.CountAsync();

            switch (sortBy?.ToLower())
            {
                case "name":
                    query = sortDesc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name);
                    break;
                case "calories":
                    query = sortDesc ? query.OrderByDescending(m => m.Calories) : query.OrderBy(m => m.Calories);
                    break;
                case "date":
                default:
                    query = sortDesc ? query.OrderByDescending(m => m.Date) : query.OrderBy(m => m.Date);
                    break;
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Meal>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(ApiResponse<PagedResult<Meal>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Meal>>> Get(string id)
        {
            var userId = GetCurrentUserId();
            var meal = await _dbContext.Meals
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (meal == null)
            {
                return NotFound(ApiResponse<Meal>.Fail("Meal not found"));
            }

            return Ok(ApiResponse<Meal>.Ok(meal));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Meal>>> Post([FromBody] MealDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<Meal>.Fail(errors, "Validation failed"));
            }

            var userId = GetCurrentUserId();
            
            var meal = new Meal
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name.Trim(),
                Calories = model.Calories,
                Date = model.Date,
                UserId = userId
            };

            await _dbContext.Meals.AddAsync(meal);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = meal.Id }, ApiResponse<Meal>.Ok(meal, "Meal created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Put(string id, [FromBody] MealDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.Fail(errors, "Validation failed"));
            }

            var userId = GetCurrentUserId();
            var meal = await _dbContext.Meals
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (meal == null)
            {
                return NotFound(ApiResponse.Fail("Meal not found"));
            }

            meal.Name = model.Name.Trim();
            meal.Calories = model.Calories;
            meal.Date = model.Date;

            _dbContext.Meals.Update(meal);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Meal updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(string id)
        {
            var userId = GetCurrentUserId();
            var meal = await _dbContext.Meals
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (meal == null)
            {
                return NotFound(ApiResponse.Fail("Meal not found"));
            }

            _dbContext.Meals.Remove(meal);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Meal deleted successfully"));
        }
    }
}
