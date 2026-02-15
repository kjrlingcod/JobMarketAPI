using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

// adjust namespaces if needed
using JobMarketApp.API.DTO.Customers;
using JobMarketApp.API.Services;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICustomerRepository> _repoMock = new();

    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 1024 });

    private CustomerService CreateSut()
        => new CustomerService(_mapperMock.Object, _cache, _repoMock.Object);

    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCached_AndDoesNotCallRepoOrMapper()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"customers:{id}";

        var cachedDto = new CustomerDto { Id = id, FirstName = "Cached", LastName = "User" };
        _cache.Set(cacheKey, cachedDto, new MemoryCacheEntryOptions { Size = 1 });

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mapperMock.Verify(m => m.Map<CustomerDto>(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepoReturnsNull_ReturnsNull_AndDoesNotMap()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id))
                 .ReturnsAsync((Customer?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _mapperMock.Verify(m => m.Map<CustomerDto>(It.IsAny<object>()), Times.Never);
        _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepoReturnsEntity_MapsCachesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"customers:{id}";

        var entity = new Customer { Id = id, FirstName = "John", LastName = "Smith" };
        var expectedDto = new CustomerDto { Id = id, FirstName = "John", LastName = "Smith" };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _mapperMock.Setup(m => m.Map<CustomerDto>(entity)).Returns(expectedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        var cacheHit = _cache.TryGetValue(cacheKey, out CustomerDto? cached);
        Assert.True(cacheHit);
        Assert.NotNull(cached);
        Assert.Equal(id, cached!.Id);

        _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mapperMock.Verify(m => m.Map<CustomerDto>(entity), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_CallsRepo_AndReturnsMappedDtos()
    {
        // Arrange
        string? term = "smi";
        int page = 1;
        int pageSize = 10;

        var entities = new List<Customer>
        {
            new Customer { Id = Guid.NewGuid(), FirstName = "John", LastName = "Smith" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
        };

        var expectedDtos = new List<CustomerDto>
        {
            new CustomerDto { Id = entities[0].Id, FirstName = "John", LastName = "Smith" },
            new CustomerDto { Id = entities[1].Id, FirstName = "Jane", LastName = "Smith" }
        };

        _repoMock.Setup(r => r.SearchAsync(term, page, pageSize)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<CustomerDto>>(entities)).Returns(expectedDtos);

        var sut = CreateSut();

        // Act
        var result = await sut.SearchAsync(term, page, pageSize);

        // Assert
        Assert.Equal(2, result.Count);
        _repoMock.Verify(r => r.SearchAsync(term, page, pageSize), Times.Once);
        _mapperMock.Verify(m => m.Map<List<CustomerDto>>(entities), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenRepoReturnsEmptyList_ReturnsEmptyList()
    {
        // Arrange
        string? term = "no-match";
        int page = 1;
        int pageSize = 10;

        var entities = new List<Customer>();
        var mappedDtos = new List<CustomerDto>();

        _repoMock.Setup(r => r.SearchAsync(term, page, pageSize)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<CustomerDto>>(entities)).Returns(mappedDtos);

        var sut = CreateSut();

        // Act
        var result = await sut.SearchAsync(term, page, pageSize);

        // Assert
        Assert.Empty(result);
    }
}