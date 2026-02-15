using JobMarketApp.API.DTO.Jobs;
using JobMarketApp.API.Services;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace JobMarketApp.Tests.Services;
public class JobServiceTests
{
    private readonly Mock<IMapper> _mapper = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 1024 });
    private readonly Mock<IJobRepository> _jobRepo = new();
    private readonly Mock<ICustomerRepository> _customerRepo = new();
    private readonly Mock<IContractorRepository> _contractorRepo = new();

    private JobService CreateSut()
        => new JobService(
            _mapper.Object,
            _cache,
            _jobRepo.Object,
            _customerRepo.Object,
            _contractorRepo.Object);

    [Fact]
    public async Task GetPaginatedAsync_MapsAndReturnsList()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        var entities = new List<Job>
        {
            new Job { Id = Guid.NewGuid() },
            new Job { Id = Guid.NewGuid() }
        };

        var mappedDtos = new List<JobDto>
        {
            new JobDto { Id = entities[0].Id },
            new JobDto { Id = entities[1].Id }
        };

        _jobRepo.Setup(r => r.GetPaginatedAsync(page, pageSize))
                .ReturnsAsync(entities);

        _mapper.Setup(m => m.Map<List<JobDto>>(entities))
               .Returns(mappedDtos);

        var sut = CreateSut();

        // Act
        var result = await sut.GetPaginatedAsync(page, pageSize);

        // Assert
        Assert.Equal(2, result.Count);
        _jobRepo.Verify(r => r.GetPaginatedAsync(page, pageSize), Times.Once);
        _mapper.Verify(m => m.Map<List<JobDto>>(entities), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCached_AndDoesNotCallRepo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"jobs:{id}";

        var cachedDto = new JobDto { Id = id };
        _cache.Set(cacheKey, cachedDto, new MemoryCacheEntryOptions { Size = 1 });

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        _jobRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mapper.Verify(m => m.Map<JobDto>(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepoReturnsNull_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Job?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _mapper.Verify(m => m.Map<JobDto>(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_CachesDto_AndReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"jobs:{id}";

        var entity = new Job { Id = id };
        var mappedDto = new JobDto { Id = id };

        _jobRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

        _mapper.Setup(m => m.Map<JobDto>(entity))
               .Returns(mappedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        var cacheHasDto = _cache.TryGetValue(cacheKey, out JobDto? cachedDto);
        Assert.True(cacheHasDto);
        Assert.NotNull(cachedDto);
        Assert.Equal(id, cachedDto!.Id);

        _jobRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mapper.Verify(m => m.Map<JobDto>(entity), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDueDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateJobDto
        {
            CustomerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(-1) // invalid
        };

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(dto));
        Assert.Equal("DueDate must be on/after StartDate.", ex.Message);

        _customerRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _jobRepo.Verify(r => r.CreateAsync(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateJobDto
        {
            CustomerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(1)
        };

        _customerRepo.Setup(r => r.GetByIdAsync(dto.CustomerId))
                     .ReturnsAsync((Customer?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(dto));
        Assert.Equal("Customer does not exist.", ex.Message);

        _jobRepo.Verify(r => r.CreateAsync(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesAndReturnsMappedDto()
    {
        // Arrange
        var dto = new CreateJobDto
        {
            CustomerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(1),
            Budget = 1000,
            Description = "Test job"
        };

        _customerRepo.Setup(r => r.GetByIdAsync(dto.CustomerId))
                     .ReturnsAsync(new Customer { Id = dto.CustomerId });

        var entityToCreate = new Job { CustomerId = dto.CustomerId };
        var createdEntity = new Job { Id = Guid.NewGuid(), CustomerId = dto.CustomerId };
        var mappedDto = new JobDto { Id = createdEntity.Id };

        _mapper.Setup(m => m.Map<Job>(dto)).Returns(entityToCreate);
        _jobRepo.Setup(r => r.CreateAsync(entityToCreate)).ReturnsAsync(createdEntity);
        _mapper.Setup(m => m.Map<JobDto>(createdEntity)).Returns(mappedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mappedDto.Id, result.Id);

        _mapper.Verify(m => m.Map<Job>(dto), Times.Once);
        _jobRepo.Verify(r => r.CreateAsync(entityToCreate), Times.Once);
        _mapper.Verify(m => m.Map<JobDto>(createdEntity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenDueDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new UpdateJobDto
        {
            StartDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(-1) // invalid
        };

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateAsync(id, dto));
        Assert.Equal("DueDate must be on/after StartDate.", ex.Message);

        _customerRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _jobRepo.Verify(r => r.UpdateAsync(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenJobNotFound_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new UpdateJobDto
        {
            StartDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(1),
            Budget = 500,
            Description = "Update"
        };

        _jobRepo.Setup(r => r.GetByIdAsync(id))
               .ReturnsAsync((Job?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAsync(id, dto);

        // Assert
        Assert.Null(result);
        _jobRepo.Verify(r => r.UpdateAsync(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesAndReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new UpdateJobDto
        {
            StartDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(1),
            Budget = 500,
            Description = "Update"
        };

        _jobRepo.Setup(r => r.GetByIdAsync(id))
               .ReturnsAsync(new Job { Id = id });


        var mappedEntity = new Job();      
        var updatedEntity = new Job { Id = id };
        var mappedDto = new JobDto { Id = id };


        _mapper.Setup(m => m.Map<Job>(dto)).Returns(mappedEntity);
        _jobRepo.Setup(r => r.UpdateAsync(It.Is<Job>(j => j.Id == id)))
               .ReturnsAsync(updatedEntity);
        _mapper.Setup(m => m.Map<JobDto>(updatedEntity)).Returns(mappedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        _mapper.Verify(m => m.Map<Job>(dto), Times.Once);
        _jobRepo.Verify(r => r.UpdateAsync(It.Is<Job>(j => j.Id == id)), Times.Once);
        _mapper.Verify(m => m.Map<JobDto>(updatedEntity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse_AndDoesNotDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(id))
               .ReturnsAsync((Job?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(id);

        // Assert
        Assert.False(result);
        _jobRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(id))
               .ReturnsAsync(new Job { Id = id });

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _jobRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}