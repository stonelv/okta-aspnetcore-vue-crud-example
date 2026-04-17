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

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Todo>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isDone = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool sortDesc = true)
        {
            var userId = GetCurrentUserId();
            var query = _dbContext.Todos.Where(t => t.UserId == userId);

            if (isDone.HasValue)
            {
                query = query.Where(t => t.IsDone == isDone.Value);
            }

            var totalCount = await query.CountAsync();

            switch (sortBy?.ToLower())
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
                case "createdat":
                default:
                    query = sortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
                    break;
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Todo>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> Get(string id)
        {
            var userId = GetCurrentUserId();
            var todo = await _dbContext.Todos
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            return Ok(todo);
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> Post(Todo model)
        {
            var userId = GetCurrentUserId();
            
            var todo = new Todo
            {
                Id = Guid.NewGuid().ToString(),
                Title = model.Title,
                IsDone = model.IsDone,
                DueDate = model.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _dbContext.Todos.AddAsync(todo);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Todo model)
        {
            var userId = GetCurrentUserId();
            var todo = await _dbContext.Todos
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            todo.Title = model.Title;
            todo.IsDone = model.IsDone;
            todo.DueDate = model.DueDate;

            _dbContext.Todos.Update(todo);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var userId = GetCurrentUserId();
            var todo = await _dbContext.Todos
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            _dbContext.Todos.Remove(todo);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
