using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetCore;
using AspNetCore.Controllers;
using Xunit;

namespace AspNetCore.Tests
{
    public class TodosControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TodosController _controller;
        private readonly string _testUserId = "test-user-123";

        public TodosControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId),
            }, "mock"));

            _controller = new TodosController(_context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                }
            };

            SeedTestData();
        }

        private void SeedTestData()
        {
            var todos = new List<Todo>
            {
                new Todo { Id = "1", Title = "Test Todo 1", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = _testUserId },
                new Todo { Id = "2", Title = "Test Todo 2", IsDone = true, CreatedAt = DateTime.UtcNow, UserId = _testUserId },
                new Todo { Id = "3", Title = "Other User's Todo", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = "other-user-456" }
            };

            _context.Todos.AddRange(todos);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Get_ReturnsOnlyUserTodos()
        {
            var result = await _controller.Get();
            
            var actionResult = Assert.IsType<ActionResult<PagedResult<Todo>>>(result);
            var model = Assert.IsAssignableFrom<PagedResult<Todo>>(actionResult.Value);
            
            Assert.Equal(2, model.Items.Count);
            Assert.All(model.Items, t => Assert.Equal(_testUserId, t.UserId));
        }

        [Fact]
        public async Task Get_WithIsDoneFilter_ReturnsFilteredTodos()
        {
            var result = await _controller.Get(isDone: true);
            
            var actionResult = Assert.IsType<ActionResult<PagedResult<Todo>>>(result);
            var model = Assert.IsAssignableFrom<PagedResult<Todo>>(actionResult.Value);
            
            Assert.Single(model.Items);
            Assert.True(model.Items.All(t => t.IsDone));
        }

        [Fact]
        public async Task Post_CreatesTodoWithCorrectUserId()
        {
            var newTodo = new Todo { Title = "New Test Todo", IsDone = false };

            var result = await _controller.Post(newTodo);

            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdTodo = Assert.IsType<Todo>(actionResult.Value);
            
            Assert.Equal(_testUserId, createdTodo.UserId);
            Assert.NotNull(createdTodo.Id);
            Assert.True(createdTodo.CreatedAt <= DateTime.UtcNow);
            Assert.Equal("New Test Todo", createdTodo.Title);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            var result = await _controller.Delete("non-existent-id");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
