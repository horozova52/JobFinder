using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Commands.Update;

public class UpdateHandler : IRequestHandler<UpdateCommand, bool>
{
    private readonly IJobPostingRepository _repository;

    public UpdateHandler(IJobPostingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (job is null) return false;

        job.Title = request.Title;
        job.Description = request.Description;
        job.Requirements = request.Requirements;
        job.Responsibilities = request.Responsibilities;
        job.Location = request.Location;
        job.JobType = request.JobType;
        job.EmploymentType = request.EmploymentType;
        job.SalaryFrom = request.SalaryFrom;
        job.SalaryTo = request.SalaryTo;
        job.IsSalaryNegotiable = request.IsSalaryNegotiable;
        job.Status = request.Status;

        await _repository.UpdateAsync(job, cancellationToken);
        return true;
    }
}