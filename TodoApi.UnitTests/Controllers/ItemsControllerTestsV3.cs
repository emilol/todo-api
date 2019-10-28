using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TodoApi.Data;
using TodoApi.Models;
using Xunit;

namespace TodoApi.UnitTests.Controllers
{
    public class ItemsControllerTestsV3 : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _fixture;
        private readonly HttpClient _client;

        public ItemsControllerTestsV3(TestWebApplicationFactory<Startup> fixture)
        {
            _fixture = fixture;
            _fixture.SeedDatabaseWith(SeedItems);
            _client = _fixture.CreateClient();
        }

        private void SeedItems(TodoDbContext context)
        {
            // Seed the database
            context.Items.AddRange(
                Enumerable.Range(1, 10).Select(t => new Item { Description = "Item " + t })
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task GetAll_ReturnsItems()
        {
            // Arrange

            // Act
            var result = await _client.GetAsync("/api/items");

            // Assert
            var model = await result.Content.ReadAsAsync<IEnumerable<Item>>();
            Assert.Equal(10, model.Count());
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_GivenInvalidId()
        {
            var result = await _client.GetAsync($"/api/items/{99}");

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsItem_GivenValidId()
        {
            var id = 2;
            var result = await _client.GetAsync($"/api/items/{id}");

            var task = await result.Content.ReadAsAsync<Item>();
            Assert.Equal("Item 2", task.Description);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_GivenNullItem()
        {
            var result = await _client.PostAsJsonAsync<Item>("/api/items", null);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var result = await _client.PostAsJsonAsync("/api/items", new Item());

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsNewlyCreatedTodoItem()
        {
            var result = await _client.PostAsJsonAsync("/api/items", new Item
            {
                Description = "This is a new task"
            });


            Assert.Equal(HttpStatusCode.Created, result.StatusCode);

            var item = _fixture
                .Find<Item>(i => i.Description == "This is a new task");

            Assert.NotNull(item);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdIsInvalid()
        {
            var result = await _client.PutAsJsonAsync($"/api/items/{99}", new Item
            {
                Id = 1,
                Description = "Task 1"
            });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsBadRequestWhenItemIsInvalid()
        {
            var result = await _client.PutAsJsonAsync<Item>($"/api/items/{1}", null);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var result = await _client.PutAsJsonAsync($"/api/items/{1}", new Item
            {
                Id = 1,
                Description = null
            });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenIdIsInvalid()
        {
            var result = await _client.PutAsJsonAsync($"/api/items/{99}", new Item
            {
                Id = 99,
                Description = "Task 99"
            });

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenItemUpdated()
        {
            var result = await _client.PutAsJsonAsync($"/api/items/{1}", new Item
            {
                Id = 1,
                Description = "Task 1",
                Done = true
            });

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

            var item = _fixture.Find<Item>(1);

            Assert.Equal(true, item.Done);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdIsInvalid()
        {
            var result = await _client.DeleteAsync($"/api/items/{99}");

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenItemDeleted()
        {
            var id = 2;
            var result = await _client.DeleteAsync($"/api/items/{id}");

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var item = _fixture.Find<Item>(2);
            Assert.Null(item);
        }
    }
}
