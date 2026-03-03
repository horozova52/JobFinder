using JobFinder.UseCases.Contracts;
using JobFinder.UseCases.Features.Jobs.Commands.Delete;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Commands;

public class DeleteHandler : IRequestHandler<DeleteCommand, bool>
{
    private readonly IJobPostingRepository _repository;

    public DeleteHandler(IJobPostingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(request.Id, cancellationToken);
        if (!exists) return false;

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}