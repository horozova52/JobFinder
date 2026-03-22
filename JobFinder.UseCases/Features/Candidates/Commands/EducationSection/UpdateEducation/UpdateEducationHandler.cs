using AutoMapper;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.EducationSection.UpdateEducation;

public class UpdateEducationHandler : IRequestHandler<UpdateEducationCommand, Result<EducationDto>>
{
    private readonly IEducationRepository _educationRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public UpdateEducationHandler(
        IEducationRepository educationRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _educationRepo = educationRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<EducationDto>> Handle(UpdateEducationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<EducationDto>.Failure("Profilul nu a fost găsit");

        var exists = await _educationRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<EducationDto>.Failure("Înregistrarea nu a fost găsită");

        var education = await _educationRepo.GetByIdAsync(request.Id, cancellationToken);
        if (education == null)
            return Result<EducationDto>.Failure("Înregistrarea nu a fost găsită");

        education.Institution = request.Institution;
        education.Degree = request.Degree;
        education.FieldOfStudy = request.FieldOfStudy;
        education.StartDate = request.StartDate;
        education.EndDate = request.EndDate;
        education.Description = request.Description;

        await _educationRepo.UpdateAsync(education, cancellationToken);

        var dto = _mapper.Map<EducationDto>(education);
        return Result<EducationDto>.Success(dto);
    }
}