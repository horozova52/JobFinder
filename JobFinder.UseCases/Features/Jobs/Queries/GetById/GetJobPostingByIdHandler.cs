using AutoMapper;
using JobFinder.Shared.DTOs.Jobs;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Queries.GetById;

public class GetJobPostingByIdHandler : IRequestHandler<GetJobPostingByIdQuery, JobPostingDto?>
{
    private readonly IJobPostingRepository _repository;
    private readonly IMapper _mapper;

    public GetJobPostingByIdHandler(IJobPostingRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<JobPostingDto?> Handle(GetJobPostingByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return job is null ? null : _mapper.Map<JobPostingDto>(job);
    }
}