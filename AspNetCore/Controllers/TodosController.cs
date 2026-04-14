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
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            if (sortBy == "Title")
            {
                query = sortDesc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);
            }
            else if (sortBy == "DueDate")
            {
                query = sortDesc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate);
            }
            else if (sortBy == "IsDone")
            {
                query = sortDesc ? query.OrderByDescending(t => t.IsDone) : query.OrderBy(t => t.IsDone);
            }
            else
            {
                query = sortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
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
            var userId = GetCurrentUserId();
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

            await _dbContext.AddAsync(todo);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Todo model)
        {
            var userId = GetCurrentUserId();
            var todo = await _dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            
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
            var todo = await _dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            
            if (todo == null)
            {
                return NotFound();
            }

            _dbContext.Todos.Remove(todo);
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
