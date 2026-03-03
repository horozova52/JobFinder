using AutoMapper;
using JobFinder.Shared.DTOs.Jobs;
using JobFinder.UseCases.Contracts;
using JobFinder.UseCases.Features.Jobs.Queries.GetAll;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Queries.GetAll;

public class GetAllJobPostingsHandler : IRequestHandler<GetAllJobPostingsQuery, IEnumerable<JobPostingDto>>
{
    private readonly IJobPostingRepository _repository;
    private readonly IMapper _mapper;

    public GetAllJobPostingsHandler(IJobPostingRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JobPostingDto>> Handle(
        GetAllJobPostingsQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<JobPostingDto>>(jobs);
    }
}