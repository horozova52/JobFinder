using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Queries.GetProfile;

public class GetCandidateProfileHandler : IRequestHandler<GetCandidateProfileQuery, GetProfileResult>
{
    private readonly ICandidateRepository _repository;
    private readonly IMapper _mapper;

    public GetCandidateProfileHandler(ICandidateRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetProfileResult> Handle(GetCandidateProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (profile == null)
        {
            // Crează profil gol automat pentru utilizator nou
            var newProfile = new CandidateProfile
            {
                UserId = request.UserId,
                FirstName = "",
                LastName = "",
            };

            profile = await _repository.CreateAsync(newProfile, cancellationToken);

            // Re-fetch cu colecții incluse
            profile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        }

        var dto = _mapper.Map<Shared.DTOs.Candidates.CandidateProfileDto>(profile);
        return new GetProfileResult(true, null, dto);
    }
}