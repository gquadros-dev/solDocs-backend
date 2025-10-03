using Moq;
using MongoDB.Driver;
using solDocs.Services;
using solDocs.Models;
using solDocs.Dtos.Tenant;
using FluentAssertions;

namespace solDocs.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly Mock<IMongoCollection<TenantModel>> _mockCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly TenantService _tenantService;

        public TenantServiceTests()
        {
            _mockCollection = new Mock<IMongoCollection<TenantModel>>();
            _mockDatabase = new Mock<IMongoDatabase>();

            _mockDatabase.Setup(db => db.GetCollection<TenantModel>("tenants", null))
                         .Returns(_mockCollection.Object);
            
            var mockIndexes = new Mock<IMongoIndexManager<TenantModel>>();
            _mockCollection.Setup(c => c.Indexes).Returns(mockIndexes.Object);

            _tenantService = new TenantService(_mockDatabase.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTenant_WhenSlugAndEmailAreUnique()
        {
            var createDto = new CreateTenantDto
            {
                Nome = "Tenant de Teste",
                Slug = "tenant-de-teste",
                Email = "teste@email.com",
                VencimentoDaLicenca = DateTime.UtcNow.AddDays(30)
            };

            _mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<TenantModel>>(), 
                                                   It.IsAny<FindOptions<TenantModel, TenantModel>>(), 
                                                   It.IsAny<CancellationToken>()))
                           .ReturnsAsync(() => 
                           {
                               var mockCursor = new Mock<IAsyncCursor<TenantModel>>();
                               mockCursor.Setup(_ => _.Current).Returns(new List<TenantModel>());
                               mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
                               mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
                               return mockCursor.Object;
                           });

            _mockCollection.Setup(c => c.InsertOneAsync(It.IsAny<TenantModel>(), null, default))
                           .Returns(Task.CompletedTask);

            var result = await _tenantService.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.Slug.Should().Be(createDto.Slug);
            result.Email.Should().Be(createDto.Email);

            _mockCollection.Verify(c => c.InsertOneAsync(It.Is<TenantModel>(t => t.Slug == createDto.Slug), 
                                                         null, 
                                                         default), 
                                   Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenSlugAlreadyExists()
        {
            var createDto = new CreateTenantDto { Slug = "slug-existente", Email = "email@unico.com" };
            var existingTenant = new TenantModel { Slug = "slug-existente" };

            var mockCursor = new Mock<IAsyncCursor<TenantModel>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<TenantModel> { existingTenant });
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

             _mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<TenantModel>>(),
                                                   It.IsAny<FindOptions<TenantModel, TenantModel>>(),
                                                   It.IsAny<CancellationToken>()))
                           .ReturnsAsync(mockCursor.Object);

            Func<Task> act = async () => await _tenantService.CreateAsync(createDto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Slug já está em uso");
        }
    }
}