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
    public class TodosController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public TodosController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Todo>>> Get(
            [FromQuery] bool? isDone = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool sortDesc = true)
        {
            var userId = GetUserId();
            var query = _dbContext.Todos.Where(t => t.UserId == userId);

            if (isDone.HasValue)
            {
                query = query.Where(t => t.IsDone == isDone.Value);
            }

            var totalCount = await query.CountAsync();

            switch (sortBy.ToLower())
            {
                case "title":
                    query = sortDesc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);
                    break;
                case "duedate":
                    query = sortDesc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate);
                    break;
                case "isdone":
                    query = sortDesc ? query.OrderByDescending(t => t.IsDone) : query.OrderBy(t => t.IsDone);
                    break;
                default:
                    query = sortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
                    break;
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Todo>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> Get(string id)
        {
            var userId = GetUserId();
            var todo = await _dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            
            if (todo == null)
            {
                return NotFound();
            }
            
            return todo;
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> Post(Todo model)
        {
            var userId = GetUserId();
            
            model.Id = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.UtcNow;
            model.UserId = userId;

            _dbContext.Todos.Add(model);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Todo model)
        {
            var userId = GetUserId();
            var exists = await _dbContext.Todos.AnyAsync(t => t.Id == id && t.UserId == userId);
            
            if (!exists)
            {
                return NotFound();
            }

            model.Id = id;
            model.UserId = userId;

            _dbContext.Todos.Update(model);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var userId = GetUserId();
            var entity = await _dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            
            if (entity == null)
            {
                return NotFound();
            }

            _dbContext.Todos.Remove(entity);
            await _dbContext.SaveChangesAsync();
            
            return Ok();
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
