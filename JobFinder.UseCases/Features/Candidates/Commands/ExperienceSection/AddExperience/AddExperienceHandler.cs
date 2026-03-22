using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.AddExperience;

public class AddExperienceHandler : IRequestHandler<AddExperienceCommand, Result<ExperienceDto>>
{
    private readonly IExperienceRepository _experienceRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public AddExperienceHandler(
        IExperienceRepository experienceRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _experienceRepo = experienceRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<ExperienceDto>> Handle(AddExperienceCommand request, CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<ExperienceDto>.Failure("Profilul nu a fost găsit");

        var experience = new Experience
        {
            CandidateProfileId = profile.Id,
            CompanyName = request.CompanyName,
            Position = request.Position,
            StartDate = request.StartDate,
            EndDate = request.IsCurrent ? null : request.EndDate,
            IsCurrent = request.IsCurrent,
            Description = request.Description,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
        };

        var created = await _experienceRepo.CreateAsync(experience, cancellationToken);
        var dto = _mapper.Map<ExperienceDto>(created);
        return Result<ExperienceDto>.Success(dto);
    }
}