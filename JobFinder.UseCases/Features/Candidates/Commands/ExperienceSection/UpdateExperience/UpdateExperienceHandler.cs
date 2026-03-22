using AutoMapper;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.UpdateExperience;

public class UpdateExperienceHandler : IRequestHandler<UpdateExperienceCommand, Result<ExperienceDto>>
{
    private readonly IExperienceRepository _experienceRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public UpdateExperienceHandler(
        IExperienceRepository experienceRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _experienceRepo = experienceRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<ExperienceDto>> Handle(UpdateExperienceCommand request, CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<ExperienceDto>.Failure("Profilul nu a fost găsit");

        var exists = await _experienceRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<ExperienceDto>.Failure("Înregistrarea nu a fost găsită");

        var experience = await _experienceRepo.GetByIdAsync(request.Id, cancellationToken);
        if (experience == null)
            return Result<ExperienceDto>.Failure("Înregistrarea nu a fost găsită");

        experience.CompanyName = request.CompanyName;
        experience.Position = request.Position;
        experience.StartDate = request.StartDate;
        experience.EndDate = request.IsCurrent ? null : request.EndDate;
        experience.IsCurrent = request.IsCurrent;
        experience.Description = request.Description;
        experience.Location = request.Location;
        experience.EmploymentType = request.EmploymentType;

        await _experienceRepo.UpdateAsync(experience, cancellationToken);

        var dto = _mapper.Map<ExperienceDto>(experience);
        return Result<ExperienceDto>.Success(dto);
    }
}