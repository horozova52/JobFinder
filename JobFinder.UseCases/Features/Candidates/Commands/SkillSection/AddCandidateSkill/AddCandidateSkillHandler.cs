using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.AddCandidateSkill;

public class AddCandidateSkillHandler : IRequestHandler<AddCandidateSkillCommand, Result<CandidateSkillDto>>
{
    private readonly ICandidateSkillRepository _skillRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public AddCandidateSkillHandler(
        ICandidateSkillRepository skillRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _skillRepo = skillRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<CandidateSkillDto>> Handle(
        AddCandidateSkillCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validare
        var skillName = request.SkillName.Trim();
        if (string.IsNullOrWhiteSpace(skillName))
            return Result<CandidateSkillDto>.Failure("Numele competenței nu poate fi gol");

        // 2. Profilul candidatului
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<CandidateSkillDto>.Failure("Profilul nu a fost găsit");

        // 3. Find-or-create Skill — delegat repository-ului (Infrastructure)
        //    Repository-ul lucrează pe același DbContext → fără FK violations
        var skill = await _skillRepo.FindOrCreateSkillAsync(skillName, cancellationToken);

        // 4. Verificare duplicate
        var duplicate = await _skillRepo.AlreadyHasSkillAsync(profile.Id, skill.Id, cancellationToken);
        if (duplicate)
            return Result<CandidateSkillDto>.Failure("Competența este deja adăugată în profilul tău");

        // 5. Asociere candidat ↔ skill
        var entity = new CandidateSkill
        {
            CandidateProfileId = profile.Id,
            SkillId = skill.Id,
            Level = request.Level,
        };

        var created = await _skillRepo.CreateAsync(entity, cancellationToken);
        var dto = _mapper.Map<CandidateSkillDto>(created);
        return Result<CandidateSkillDto>.Success(dto);
    }
}