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

        // GET: api/todos
        [HttpGet]
        public async Task<ActionResult<TodoListResponse>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isDone = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortOrder = "desc")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var query = _dbContext.Todos.Where(t => t.UserId == userId);

            if (isDone.HasValue)
            {
                query = query.Where(t => t.IsDone == isDone.Value);
            }

            if (sortBy == "Title")
            {
                query = sortOrder == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title);
            }
            else if (sortBy == "DueDate")
            {
                query = sortOrder == "asc" ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate);
            }
            else if (sortBy == "IsDone")
            {
                query = sortOrder == "asc" ? query.OrderBy(t => t.IsDone) : query.OrderByDescending(t => t.IsDone);
            }
            else
            {
                query = sortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new TodoListResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // GET: api/todos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> Get(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var todo = await _dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            return todo;
        }

        // POST: api/todos
        [HttpPost]
        public async Task<ActionResult<Todo>> Post(Todo model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.Id = Guid.NewGuid().ToString();
            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;

            await _dbContext.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }

        // PUT: api/todos/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Todo model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingTodo = await _dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (existingTodo == null)
            {
                return NotFound();
            }

            existingTodo.Title = model.Title;
            existingTodo.IsDone = model.IsDone;
            existingTodo.DueDate = model.DueDate;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/todos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

    public class TodoListResponse
    {
        public List<Todo> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
