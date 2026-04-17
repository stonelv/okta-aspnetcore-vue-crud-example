using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore;
using AspNetCore.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AspNetCore.Tests
{
    public class TodosControllerTests
    {
        private readonly string _testUserId = "test-user-123";
        private readonly string _otherUserId = "other-user-456";

        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private TodosController CreateControllerWithUser(ApplicationDbContext context, string userId)
        {
            var controller = new TodosController(context);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            return controller;
        }

        [Fact]
        public async Task Get_ShouldReturnOnlyUserOwnTodos()
        {
            var options = CreateNewContextOptions();
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Todos.AddRange(new List<Todo>
                {
                    new Todo
                    {
                        Id = "todo-1",
                        Title = "User's Todo",
                        IsDone = false,
                        CreatedAt = DateTime.UtcNow,
                        UserId = _testUserId
                    },
                    new Todo
                    {
                        Id = "todo-2",
                        Title = "Other User's Todo",
                        IsDone = false,
                        CreatedAt = DateTime.UtcNow,
                        UserId = _otherUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get();
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var pagedResult = Assert.IsType<PagedResult<Todo>>(okResult.Value);
                
                Assert.Equal(1, pagedResult.TotalCount);
                Assert.Single(pagedResult.Items);
                Assert.Equal("User's Todo", pagedResult.Items[0].Title);
            }
        }

        [Fact]
        public async Task Get_WithIsDoneFilter_ShouldReturnFilteredTodos()
        {
            var options = CreateNewContextOptions();
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Todos.AddRange(new List<Todo>
                {
                    new Todo
                    {
                        Id = "todo-1",
                        Title = "Pending Todo",
                        IsDone = false,
                        CreatedAt = DateTime.UtcNow,
                        UserId = _testUserId
                    },
                    new Todo
                    {
                        Id = "todo-2",
                        Title = "Completed Todo",
                        IsDone = true,
                        CreatedAt = DateTime.UtcNow,
                        UserId = _testUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(isDone: true);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var pagedResult = Assert.IsType<PagedResult<Todo>>(okResult.Value);
                
                Assert.Equal(1, pagedResult.TotalCount);
                Assert.Single(pagedResult.Items);
                Assert.Equal("Completed Todo", pagedResult.Items[0].Title);
                Assert.True(pagedResult.Items[0].IsDone);
            }
        }

        [Fact]
        public async Task Post_ShouldCreateTodoWithCurrentUserId()
        {
            var options = CreateNewContextOptions();
            
            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var newTodo = new Todo
                {
                    Title = "New Todo",
                    IsDone = false,
                    DueDate = DateTime.UtcNow.AddDays(1)
                };
                
                var result = await controller.Post(newTodo);
                
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var createdTodo = Assert.IsType<Todo>(createdAtActionResult.Value);
                
                Assert.Equal("New Todo", createdTodo.Title);
                Assert.Equal(_testUserId, createdTodo.UserId);
                Assert.NotEqual(default(DateTime), createdTodo.CreatedAt);
            }
        }

        [Fact]
        public async Task GetById_OtherUsersTodo_ShouldReturnNotFound()
        {
            var options = CreateNewContextOptions();
            const string otherTodoId = "other-todo-id";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Todos.Add(new Todo
                {
                    Id = otherTodoId,
                    Title = "Other User's Todo",
                    IsDone = false,
                    CreatedAt = DateTime.UtcNow,
                    UserId = _otherUserId
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(otherTodoId);
                
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }
    }
}
