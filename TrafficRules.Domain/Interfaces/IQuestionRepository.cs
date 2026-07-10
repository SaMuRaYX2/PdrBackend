using TrafficRules.Domain.Entities;
namespace TrafficRules.Domain.Interfaces;

public interface IQuestionRepository
{
    Task<IEnumerable<Question>> GetAllAsync();
    Task<Question?> GetByIdAsync(Guid id);
    Task AddAsync(Question question);
    Task SaveChangesAsync();
}