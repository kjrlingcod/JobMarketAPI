using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.API.DTO.Jobs;
using JobMarketApp.API.Services;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace JobMarketApp.Tests.Services;

public class JobOfferServiceTests
{
    private readonly Mock<IMapper> _mapper = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 1024 });

    private readonly Mock<IJobOfferRepository> _jobOfferRepo = new();
    private readonly Mock<IContractorRepository> _contractorRepo = new();
    private readonly Mock<IJobRepository> _jobRepo = new();

    private JobOfferService CreateSut()
        => new JobOfferService(
            _mapper.Object,
            _cache,
            _jobOfferRepo.Object,
            _contractorRepo.Object,
            _jobRepo.Object);

    [Fact]
    public async Task GetPaginatedAsync_MapsAndReturnsList()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        var entities = new List<JobOffer>
        {
            new JobOffer { Id = Guid.NewGuid() },
            new JobOffer { Id = Guid.NewGuid() }
        };

        var mapped = new List<JobOfferDto>
        {
            new JobOfferDto { Id = entities[0].Id },
            new JobOfferDto { Id = entities[1].Id }
        };

        _jobOfferRepo.Setup(r => r.GetPaginatedAsync(page, pageSize))
                    .ReturnsAsync(entities);

        _mapper.Setup(m => m.Map<List<JobOfferDto>>(entities))
               .Returns(mapped);

        var sut = CreateSut();

        // Act
        var result = await sut.GetPaginatedAsync(page, pageSize);

        // Assert
        Assert.Equal(2, result.Count);
        _jobOfferRepo.Verify(r => r.GetPaginatedAsync(page, pageSize), Times.Once);
        _mapper.Verify(m => m.Map<List<JobOfferDto>>(entities), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCached_AndDoesNotCallRepo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"jobOffer:{id}";

        var cachedDto = new JobOfferDto { Id = id };
        _cache.Set(cacheKey, cachedDto, new MemoryCacheEntryOptions { Size = 1 });

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        _jobOfferRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mapper.Verify(m => m.Map<JobOfferDto>(It.IsAny<JobOffer>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepoReturnsNull_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _jobOfferRepo.Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync((JobOffer?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _mapper.Verify(m => m.Map<JobOfferDto>(It.IsAny<JobOffer>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_CachesDto_AndReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = $"jobOffer:{id}";

        var entity = new JobOffer { Id = id };
        var mappedDto = new JobOfferDto { Id = id };

        _jobOfferRepo.Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync(entity);

        _mapper.Setup(m => m.Map<JobOfferDto>(entity))
               .Returns(mappedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        var cacheHasDto = _cache.TryGetValue(cacheKey, out JobOfferDto? cachedDto);
        Assert.True(cacheHasDto);
        Assert.NotNull(cachedDto);
        Assert.Equal(id, cachedDto!.Id);

        _jobOfferRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mapper.Verify(m => m.Map<JobOfferDto>(entity), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenJobDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 100
        };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync((Job?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(dto));
        Assert.Equal("Job does not exist.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenPriceExceedsBudget_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 1000
        };

        var job = new Job { Id = dto.JobId, Budget = 500 };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync(job);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.CreateAsync(dto));
        Assert.Equal("Price exceeds job budget.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenContractorDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 100
        };

        var job = new Job { Id = dto.JobId, Budget = 500 };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync(job);

        _contractorRepo.Setup(r => r.GetByIdAsync(dto.ContractorId))
                      .ReturnsAsync((Contractor?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(dto));
        Assert.Equal("Contractor does not exist.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesAndReturnsMappedDto()
    {
        // Arrange
        var dto = new CreateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 100
        };

        var job = new Job { Id = dto.JobId, Budget = 500 };
        var contractor = new Contractor { Id = dto.ContractorId };

        var entityToCreate = new JobOffer { JobId = dto.JobId, ContractorId = dto.ContractorId, Price = dto.Price };
        var createdEntity = new JobOffer { Id = Guid.NewGuid(), JobId = dto.JobId, ContractorId = dto.ContractorId, Price = dto.Price };
        var mappedDto = new JobOfferDto { Id = createdEntity.Id };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId)).ReturnsAsync(job);
        _contractorRepo.Setup(r => r.GetByIdAsync(dto.ContractorId)).ReturnsAsync(contractor);

        _mapper.Setup(m => m.Map<JobOffer>(dto)).Returns(entityToCreate);
        _jobOfferRepo.Setup(r => r.CreateAsync(entityToCreate)).ReturnsAsync(createdEntity);
        _mapper.Setup(m => m.Map<JobOfferDto>(createdEntity)).Returns(mappedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mappedDto.Id, result.Id);

        _jobOfferRepo.Verify(r => r.CreateAsync(entityToCreate), Times.Once);
        _mapper.Verify(m => m.Map<JobOffer>(dto), Times.Once);
        _mapper.Verify(m => m.Map<JobOfferDto>(createdEntity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenJobDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 100
        };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync((Job?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateAsync(id, dto));
        Assert.Equal("Job does not exist.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_WhenContractorDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 100
        };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync(new Job { Id = dto.JobId, Budget = 999 });

        _contractorRepo.Setup(r => r.GetByIdAsync(dto.ContractorId))
                      .ReturnsAsync((Contractor?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateAsync(id, dto));
        Assert.Equal("Contractor does not exist.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_WhenOfferNotFound_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 100
        };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync(new Job { Id = dto.JobId, Budget = 999 });

        _contractorRepo.Setup(r => r.GetByIdAsync(dto.ContractorId))
                      .ReturnsAsync(new Contractor { Id = dto.ContractorId });

        _jobOfferRepo.Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync((JobOffer?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAsync(id, dto);

        // Assert
        Assert.Null(result);
        _jobOfferRepo.Verify(r => r.UpdateAsync(It.IsAny<JobOffer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesAndReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateJobOfferDto
        {
            JobId = Guid.NewGuid(),
            ContractorId = Guid.NewGuid(),
            Price = 150
        };

        _jobRepo.Setup(r => r.GetByIdAsync(dto.JobId))
               .ReturnsAsync(new Job { Id = dto.JobId, Budget = 999 });

        _contractorRepo.Setup(r => r.GetByIdAsync(dto.ContractorId))
                      .ReturnsAsync(new Contractor { Id = dto.ContractorId });

        _jobOfferRepo.Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync(new JobOffer { Id = id });

        var mappedEntity = new JobOffer { JobId = dto.JobId, ContractorId = dto.ContractorId, Price = dto.Price };
        var updatedEntity = new JobOffer { Id = id, JobId = dto.JobId, ContractorId = dto.ContractorId, Price = dto.Price };
        var mappedDto = new JobOfferDto { Id = id };

        _mapper.Setup(m => m.Map<JobOffer>(dto)).Returns(mappedEntity);
        _jobOfferRepo.Setup(r => r.UpdateAsync(It.Is<JobOffer>(x => x.Id == id)))
                    .ReturnsAsync(updatedEntity);
        _mapper.Setup(m => m.Map<JobOfferDto>(updatedEntity)).Returns(mappedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);

        _jobOfferRepo.Verify(r => r.UpdateAsync(It.Is<JobOffer>(x => x.Id == id)), Times.Once);
        _mapper.Verify(m => m.Map<JobOffer>(dto), Times.Once);
        _mapper.Verify(m => m.Map<JobOfferDto>(updatedEntity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse_AndDoesNotDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        _jobOfferRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((JobOffer?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(id);

        // Assert
        Assert.False(result);
        _jobOfferRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _jobOfferRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new JobOffer { Id = id });

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _jobOfferRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_WhenJobDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync((Job?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AcceptAsync(jobId, contractorId));
        Assert.Equal("Job does not exist.", ex.Message);
    }

    [Fact]
    public async Task AcceptAsync_WhenJobAlreadyAccepted_ThrowsValidationException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(new Job { Id = jobId, AcceptedBy = Guid.NewGuid() });

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AcceptAsync(jobId, contractorId));
        Assert.Equal("Job is no longer available.", ex.Message);
    }

    [Fact]
    public async Task AcceptAsync_WhenJobOfferDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(new Job { Id = jobId, AcceptedBy = Guid.Empty });

        _jobOfferRepo.Setup(r => r.GetByIdAndContractorIdAsync(jobId, contractorId)).ReturnsAsync((JobOffer?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AcceptAsync(jobId, contractorId));
        Assert.Equal("Job offer does not exist.", ex.Message);
    }

    [Fact]
    public async Task AcceptAsync_WhenContractorDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(jobId))
               .ReturnsAsync(new Job { Id = jobId, AcceptedBy = Guid.Empty });

        _jobOfferRepo.Setup(r => r.GetByIdAndContractorIdAsync(jobId, contractorId))
                    .ReturnsAsync(new JobOffer { Id = Guid.NewGuid(), JobId = jobId, ContractorId = contractorId });

        _contractorRepo.Setup(r => r.GetByIdAsync(contractorId))
                      .ReturnsAsync((Contractor?)null);

        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.AcceptAsync(jobId, contractorId));
        Assert.Equal("Contractor does not exist.", ex.Message);
    }

    [Fact]
    public async Task AcceptAsync_WhenValid_AcceptsAndReturnsMappedJobDto()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();

        _jobRepo.Setup(r => r.GetByIdAsync(jobId))
               .ReturnsAsync(new Job { Id = jobId, AcceptedBy = Guid.Empty });

        _jobOfferRepo.Setup(r => r.GetByIdAndContractorIdAsync(jobId, contractorId))
                    .ReturnsAsync(new JobOffer { Id = Guid.NewGuid(), JobId = jobId, ContractorId = contractorId });

        _contractorRepo.Setup(r => r.GetByIdAsync(contractorId))
                      .ReturnsAsync(new Contractor { Id = contractorId });

        var acceptedJobEntity = new Job { Id = jobId, AcceptedBy = contractorId };
        var mappedJobDto = new JobDto { Id = jobId, AcceptedBy = contractorId };

        _jobRepo.Setup(r => r.AcceptAsync(jobId, contractorId))
               .ReturnsAsync(acceptedJobEntity);

        _mapper.Setup(m => m.Map<JobDto>(acceptedJobEntity))
               .Returns(mappedJobDto);

        var sut = CreateSut();

        // Act
        var result = await sut.AcceptAsync(jobId, contractorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobId, result.Id);
        Assert.Equal(contractorId, result.AcceptedBy);

        _jobRepo.Verify(r => r.AcceptAsync(jobId, contractorId), Times.Once);
        _mapper.Verify(m => m.Map<JobDto>(acceptedJobEntity), Times.Once);
    }
}