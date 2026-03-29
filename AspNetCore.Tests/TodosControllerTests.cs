using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore;
using AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspNetCore.Tests
{
    public class TodosControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public TodosControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 移除现有的ApplicationDbContext配置
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == 
                            typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // 添加内存数据库用于测试
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    // 构建服务提供者
                    var serviceProvider = services.BuildServiceProvider();

                    // 创建数据库范围并初始化测试数据
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                        // 确保数据库已创建
                        db.Database.EnsureCreated();

                        // 添加测试数据
                        var userId = "test-user-id";
                        db.Todos.AddRange(new List<Todo>
                        {
                            new Todo
                            {
                                Id = "todo-1",
                                UserId = userId,
                                Title = "Test Todo 1",
                                IsDone = false,
                                CreatedAt = DateTime.UtcNow
                            },
                            new Todo
                            {
                                Id = "todo-2",
                                UserId = userId,
                                Title = "Test Todo 2",
                                IsDone = true,
                                CreatedAt = DateTime.UtcNow.AddHours(-1)
                            },
                            new Todo
                            {
                                Id = "todo-3",
                                UserId = "other-user-id",
                                Title = "Other User's Todo",
                                IsDone = false,
                                CreatedAt = DateTime.UtcNow
                            }
                        });
                        db.SaveChanges();
                    }
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetTodos_ReturnsCorrectPagedResult()
        {
            // Arrange
            // 模拟已认证用户
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/todos?page=1&pageSize=10");
            request.Headers.Add("User", "test-user-id");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<Todo>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public async Task GetTodos_WithIsDoneFilter_ReturnsFilteredResults()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/todos?isDone=true");
            request.Headers.Add("User", "test-user-id");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<Todo>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Test Todo 2", result.Items.First().Title);
            Assert.True(result.Items.First().IsDone);
        }

        [Fact]
        public async Task CreateTodo_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var newTodo = new Todo
            {
                Title = "New Test Todo",
                IsDone = false,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var json = JsonSerializer.Serialize(newTodo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/todos");
            request.Headers.Add("User", "test-user-id");
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var createdTodo = JsonSerializer.Deserialize<Todo>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(createdTodo);
            Assert.Equal("New Test Todo", createdTodo.Title);
            Assert.False(createdTodo.IsDone);
            Assert.Equal("test-user-id", createdTodo.UserId);
            Assert.True(createdTodo.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
        }

        [Fact]
        public async Task UpdateTodo_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            var updatedTodo = new Todo
            {
                Id = "todo-1",
                Title = "Updated Todo",
                IsDone = true,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            var json = JsonSerializer.Serialize(updatedTodo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, "/api/todos/todo-1");
            request.Headers.Add("User", "test-user-id");
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Todo>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.Equal("Updated Todo", result.Title);
            Assert.True(result.IsDone);
        }

        [Fact]
        public async Task DeleteTodo_WithValidId_RemovesTodoSuccessfully()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/todos/todo-1");
            request.Headers.Add("User", "test-user-id");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetTodos_UserCannotAccessOtherUsersData()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/todos");
            request.Headers.Add("User", "test-user-id");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<Todo>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // 确认不包含其他用户的Todo
            Assert.DoesNotContain(result.Items, t => t.UserId == "other-user-id");
        }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
