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
    public class TodosControllerTests
    {
        private const string TestUserId = "test-user-123";
        private const string OtherUserId = "other-user-456";

        private ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
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
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            
            return controller;
        }

        [Fact]
        public async Task Get_WithPagination_ReturnsCorrectPage()
        {
            using (var context = CreateInMemoryContext())
            {
                for (int i = 1; i <= 15; i++)
                {
                    context.Todos.Add(new Todo
                    {
                        Id = $"todo-{i}",
                        Title = $"Todo {i}",
                        IsDone = i % 2 == 0,
                        CreatedAt = DateTime.UtcNow.AddDays(-i),
                        UserId = TestUserId
                    });
                }
                await context.SaveChangesAsync();

                var controller = CreateControllerWithUser(context, TestUserId);

                var result = await controller.Get(page: 2, pageSize: 5);
                
                var okResult = Assert.IsType<ActionResult<PagedResult<Todo>>>(result);
                var pagedResult = okResult.Value;
                
                Assert.NotNull(pagedResult);
                Assert.Equal(15, pagedResult.TotalCount);
                Assert.Equal(2, pagedResult.Page);
                Assert.Equal(5, pagedResult.PageSize);
                Assert.Equal(3, pagedResult.TotalPages);
                Assert.Equal(5, pagedResult.Items.Count);
            }
        }

        [Fact]
        public async Task Get_WithIsDoneFilter_ReturnsFilteredResults()
        {
            using (var context = CreateInMemoryContext())
            {
                context.Todos.Add(new Todo
                {
                    Id = "todo-1",
                    Title = "Completed Todo",
                    IsDone = true,
                    CreatedAt = DateTime.UtcNow,
                    UserId = TestUserId
                });
                context.Todos.Add(new Todo
                {
                    Id = "todo-2",
                    Title = "Incomplete Todo",
                    IsDone = false,
                    CreatedAt = DateTime.UtcNow,
                    UserId = TestUserId
                });
                context.Todos.Add(new Todo
                {
                    Id = "todo-3",
                    Title = "Another Completed Todo",
                    IsDone = true,
                    CreatedAt = DateTime.UtcNow,
                    UserId = TestUserId
                });
                await context.SaveChangesAsync();

                var controller = CreateControllerWithUser(context, TestUserId);

                var result = await controller.Get(isDone: true);
                
                var okResult = Assert.IsType<ActionResult<PagedResult<Todo>>>(result);
                var pagedResult = okResult.Value;
                
                Assert.NotNull(pagedResult);
                Assert.Equal(2, pagedResult.TotalCount);
                Assert.All(pagedResult.Items, t => Assert.True(t.IsDone));
            }
        }

        [Fact]
        public async Task Post_CreatesTodoWithCurrentUserId()
        {
            using (var context = CreateInMemoryContext())
            {
                var controller = CreateControllerWithUser(context, TestUserId);
                
                var newTodo = new Todo
                {
                    Title = "New Test Todo",
                    IsDone = false,
                    DueDate = DateTime.UtcNow.AddDays(7)
                };

                var result = await controller.Post(newTodo);
                
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var createdTodo = Assert.IsType<Todo>(createdAtActionResult.Value);
                
                Assert.NotNull(createdTodo.Id);
                Assert.Equal("New Test Todo", createdTodo.Title);
                Assert.Equal(TestUserId, createdTodo.UserId);
                Assert.False(createdTodo.IsDone);
                Assert.NotEqual(default(DateTime), createdTodo.CreatedAt);

                var todoInDb = await context.Todos.FirstOrDefaultAsync(t => t.Id == createdTodo.Id);
                Assert.NotNull(todoInDb);
                Assert.Equal(TestUserId, todoInDb.UserId);
            }
        }

        [Fact]
        public async Task Get_UserCanOnlyAccessOwnTodos()
        {
            using (var context = CreateInMemoryContext())
            {
                context.Todos.Add(new Todo
                {
                    Id = "todo-1",
                    Title = "Test User's Todo",
                    IsDone = false,
                    CreatedAt = DateTime.UtcNow,
                    UserId = TestUserId
                });
                context.Todos.Add(new Todo
                {
                    Id = "todo-2",
                    Title = "Other User's Todo",
                    IsDone = false,
                    CreatedAt = DateTime.UtcNow,
                    UserId = OtherUserId
                });
                await context.SaveChangesAsync();

                var controller = CreateControllerWithUser(context, TestUserId);

                var result = await controller.Get();
                
                var okResult = Assert.IsType<ActionResult<PagedResult<Todo>>>(result);
                var pagedResult = okResult.Value;
                
                Assert.NotNull(pagedResult);
                Assert.Equal(1, pagedResult.TotalCount);
                Assert.Single(pagedResult.Items);
                Assert.Equal("Test User's Todo", pagedResult.Items[0].Title);
                Assert.Equal(TestUserId, pagedResult.Items[0].UserId);
            }
        }
    }
}
