using System.Data;

namespace TrafficRules.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}