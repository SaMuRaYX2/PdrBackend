using Microsoft.EntityFrameworkCore;
using TrafficRules.Domain.Entities;
using TrafficRules.Domain.Interfaces;
using TrafficRules.Infrastructure.Data;

namespace TrafficRules.Infrastructure.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        return await _context.Questions
            .Include(q => q.Answers)
            .ToListAsync();
    }

    public async Task<Question?> GetByIdAsync(Guid id)
    {
        return await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task AddAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}