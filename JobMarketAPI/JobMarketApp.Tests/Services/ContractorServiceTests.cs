using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

// adjust namespaces if needed
using JobMarketApp.API.DTO.Contractors;
using JobMarketApp.API.Services;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.Tests.Services;

public class ContractorServiceTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IContractorRepository> _repoMock = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 1024 });

    private ContractorService CreateSut()
        => new ContractorService(_mapperMock.Object, _cache, _repoMock.Object);

    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCached_AndDoesNotCallRepoOrMapper()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"contractors:{id}";

        var cachedDto = new ContractorDto { Id = id, Name = "Cached", Rating = 5 };
        _cache.Set(cacheKey, cachedDto, new MemoryCacheEntryOptions { Size = 1 });

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mapperMock.Verify(m => m.Map<ContractorDto>(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepoReturnsNull_ReturnsNull_AndDoesNotMap()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id))
                 .ReturnsAsync((Contractor?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _mapperMock.Verify(m => m.Map<ContractorDto>(It.IsAny<object>()), Times.Never);
        _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepoReturnsEntity_MapsCachesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"contractors:{id}";

        var entity = new Contractor { Id = id, Name = "Test Company", Rating = 4.5m };
        var expectedDto = new ContractorDto { Id = id, Name = "Test Company", Rating = 4.5m };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _mapperMock.Setup(m => m.Map<ContractorDto>(entity)).Returns(expectedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result!.Id);

        // verify cached
        var cacheHit = _cache.TryGetValue(cacheKey, out ContractorDto? cached);
        Assert.True(cacheHit);
        Assert.NotNull(cached);
        Assert.Equal(id, cached!.Id);

        _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mapperMock.Verify(m => m.Map<ContractorDto>(entity), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMappedList()
    {
        // Arrange
        string? term = "jo";
        int page = 1;
        int pageSize = 10;

        var entities = new List<Contractor>
        {
            new Contractor { Id = Guid.NewGuid(), Name = "John", Rating = 4.0m },
            new Contractor { Id = Guid.NewGuid(), Name = "Joanna", Rating = 5.0m }
        };

        var expectedDtos = new List<ContractorDto>
        {
            new ContractorDto { Id = entities[0].Id, Name = "John", Rating = 4.0m },
            new ContractorDto { Id = entities[1].Id, Name = "Joanna", Rating = 5.0m }
        };

        _repoMock.Setup(r => r.SearchAsync(term, page, pageSize)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<ContractorDto>>(entities)).Returns(expectedDtos);

        var sut = CreateSut();

        // Act
        var result = await sut.SearchAsync(term, page, pageSize);

        // Assert
        Assert.Equal(2, result.Count);
        _repoMock.Verify(r => r.SearchAsync(term, page, pageSize), Times.Once);
        _mapperMock.Verify(m => m.Map<List<ContractorDto>>(entities), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenRepoReturnsEmptyList_ReturnsEmptyList()
    {
        // Arrange
        string? term = "no-match";
        int page = 1;
        int pageSize = 10;

        var entities = new List<Contractor>();
        var mappedDtos = new List<ContractorDto>();

        _repoMock.Setup(r => r.SearchAsync(term, page, pageSize)).ReturnsAsync(entities);
        _mapperMock.Setup(m => m.Map<List<ContractorDto>>(entities)).Returns(mappedDtos);

        var sut = CreateSut();

        // Act
        var result = await sut.SearchAsync(term, page, pageSize);

        // Assert
        Assert.Empty(result);
    }
}