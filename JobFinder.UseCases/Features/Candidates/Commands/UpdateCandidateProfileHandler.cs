using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.UpdateProfile;

public class UpdateCandidateProfileHandler : IRequestHandler<UpdateCandidateProfileCommand, UpdateProfileResult>
{
    private readonly ICandidateRepository _repository;
    private readonly IMapper _mapper;

    public UpdateCandidateProfileHandler(ICandidateRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UpdateProfileResult> Handle(UpdateCandidateProfileCommand request, CancellationToken cancellationToken)
    {
      var profile = await _repository.GetTrackedByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
        {
            profile = new CandidateProfile { UserId = request.UserId };
            _mapper.Map(request.Dto, profile);
            profile.IsCompleted = ComputeIsCompleted(profile);
            profile = await _repository.CreateAsync(profile, cancellationToken);
        }
        else
        {
            _mapper.Map(request.Dto, profile);
            profile.IsCompleted = ComputeIsCompleted(profile);
            await _repository.UpdateAsync(profile, cancellationToken);
        }
        var updated = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        var dto = _mapper.Map<Shared.DTOs.Candidates.CandidateProfileDto>(updated);
        return new UpdateProfileResult(true, "Profil actualizat cu succes", dto);
    }
        private static bool ComputeIsCompleted(CandidateProfile p) =>
        !string.IsNullOrWhiteSpace(p.FirstName) &&
        !string.IsNullOrWhiteSpace(p.LastName) &&
        !string.IsNullOrWhiteSpace(p.Summary) &&
        p.Educations.Any() &&
        p.Experiences.Any() &&
        p.Skills.Any();
}