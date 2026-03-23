using AutoMapper;
using JobFinder.Core.Entities.Employers;
using JobFinder.Shared.DTOs.Employers;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Employers.Queries.GetProfile;

public class GetEmployerProfileHandler
    : IRequestHandler<GetEmployerProfileQuery, GetEmployerProfileResult>
{
    private readonly IEmployerRepository _repository;
    private readonly IMapper _mapper;

    public GetEmployerProfileHandler(IEmployerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetEmployerProfileResult> Handle(
        GetEmployerProfileQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (profile == null)
        {
         
            var blank = new EmployerProfile
            {
                UserId = request.UserId,
                CompanyName = "",
            };
            profile = await _repository.CreateAsync(blank, cancellationToken);
            profile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        }

        var dto = _mapper.Map<EmployerProfileDto>(profile);
        return new GetEmployerProfileResult(true, null, dto);
    }
}