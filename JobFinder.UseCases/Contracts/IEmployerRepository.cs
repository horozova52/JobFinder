using JobFinder.Core.Entities.Employers;

namespace JobFinder.UseCases.Contracts;

public interface IEmployerRepository
{
    Task<EmployerProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployerProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<EmployerProfile> CreateAsync(EmployerProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmployerProfile profile, CancellationToken cancellationToken = default);
}