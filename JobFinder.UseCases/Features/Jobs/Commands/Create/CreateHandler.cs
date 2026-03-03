using AutoMapper;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Shared.DTOs.Jobs;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Commands.Create;

public class CreateHandler : IRequestHandler<CreateCommand, JobPostingDto>
{
    private readonly IJobPostingRepository _repository;
    private readonly IMapper _mapper;

    public CreateHandler(IJobPostingRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<JobPostingDto> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        var jobPosting = new JobPosting
        {
            EmployerProfileId = request.EmployerProfileId,
            Title = request.Title,
            Description = request.Description,
            Requirements = request.Requirements,
            Responsibilities = request.Responsibilities,
            Location = request.Location,
            JobType = request.JobType,
            EmploymentType = request.EmploymentType,
            SalaryFrom = request.SalaryFrom,
            SalaryTo = request.SalaryTo,
            IsSalaryNegotiable = request.IsSalaryNegotiable,
            Status = JobStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(jobPosting, cancellationToken);
        return _mapper.Map<JobPostingDto>(created);
    }
}