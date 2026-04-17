using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class MealsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MealsController> _logger;

        public MealsController(ApplicationDbContext dbContext, ILogger<MealsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Meal>>> GetMeals(
            [FromQuery] string keyword = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _dbContext.Meals.AsQueryable();

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

                query = query.OrderByDescending(m => m.Date).ThenByDescending(m => m.Name);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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
                    TotalPages = totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取餐食列表时发生错误");
                return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Meal>> GetMeal(string id)
        {
            try
            {
                var meal = await _dbContext.Meals.FindAsync(id);

                if (meal == null)
                {
                    return NotFound(new { error = "未找到", message = $"餐食记录 ID={id} 不存在" });
                }

                return Ok(meal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取餐食详情时发生错误");
                return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Meal>> CreateMeal([FromBody] Meal model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return BadRequest(new { error = "验证失败", errors });
                }

                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    model.Id = Guid.NewGuid().ToString();
                }

                await _dbContext.Meals.AddAsync(model);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMeal), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建餐食记录时发生错误");
                return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMeal(string id, [FromBody] Meal model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest(new { error = "验证失败", message = "ID 不匹配" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return BadRequest(new { error = "验证失败", errors });
                }

                var exists = await _dbContext.Meals.AnyAsync(m => m.Id == id);
                if (!exists)
                {
                    return NotFound(new { error = "未找到", message = $"餐食记录 ID={id} 不存在" });
                }

                _dbContext.Meals.Update(model);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新餐食记录时发生错误");
                return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMeal(string id)
        {
            try
            {
                var meal = await _dbContext.Meals.FindAsync(id);

                if (meal == null)
                {
                    return NotFound(new { error = "未找到", message = $"餐食记录 ID={id} 不存在" });
                }

                _dbContext.Meals.Remove(meal);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除餐食记录时发生错误");
                return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
            }
        }
    }
}
