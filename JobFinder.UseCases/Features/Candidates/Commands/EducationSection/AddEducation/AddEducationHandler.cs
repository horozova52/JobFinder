using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.EducationSection.AddEducation;

public class AddEducationHandler : IRequestHandler<AddEducationCommand, Result<EducationDto>>
{
    private readonly IEducationRepository _educationRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public AddEducationHandler(
        IEducationRepository educationRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _educationRepo = educationRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<EducationDto>> Handle(AddEducationCommand request, CancellationToken cancellationToken)
    {
        // Obținem profilul prin userId
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<EducationDto>.Failure("Profilul nu a fost găsit");

        var education = new Education
        {
            CandidateProfileId = profile.Id,
            Institution = request.Institution,
            Degree = request.Degree,
            FieldOfStudy = request.FieldOfStudy,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
        };

        var created = await _educationRepo.CreateAsync(education, cancellationToken);
        var dto = _mapper.Map<EducationDto>(created);
        return Result<EducationDto>.Success(dto);
    }
}