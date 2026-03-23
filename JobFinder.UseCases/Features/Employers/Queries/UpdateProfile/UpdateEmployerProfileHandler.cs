using AutoMapper;
using JobFinder.Core.Entities.Employers;
using JobFinder.Shared.DTOs.Employers;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Employers.Commands.UpdateProfile;

public class UpdateEmployerProfileHandler
    : IRequestHandler<UpdateEmployerProfileCommand, UpdateEmployerProfileResult>
{
    private readonly IEmployerRepository _repository;
    private readonly IMapper _mapper;

    public UpdateEmployerProfileHandler(IEmployerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UpdateEmployerProfileResult> Handle(
        UpdateEmployerProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetTrackedByUserIdAsync(request.UserId, cancellationToken);

        if (profile == null)
        {
            profile = new EmployerProfile { UserId = request.UserId };
            _mapper.Map(request.Dto, profile);
            profile = await _repository.CreateAsync(profile, cancellationToken);
        }
        else
        {
            _mapper.Map(request.Dto, profile);
            await _repository.UpdateAsync(profile, cancellationToken);
        }

        var updated = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        var dto = _mapper.Map<EmployerProfileDto>(updated);
        return new UpdateEmployerProfileResult(true, "Profil actualizat cu succes", dto);
    }
}