using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrafficRules.Domain.Entities;
using TrafficRules.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace TrafficRules.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set; }                                                                                                                                                                         
    public DbSet<Question> Questions { get; set; }                                                                                                                                                                          
    public DbSet<Answer> Answers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Category)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.CategoryId);
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId);

        var categoryId = CategoryConstants.TrafficSignsId;

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = categoryId, Name = "Дорожні знаки" },
            new Category { Id = CategoryConstants.Topic01_GeneralId, Name = "1. Загальні положення" },
            new Category { Id = CategoryConstants.Topic02_DriversDutiesId, Name = "2. Обов'язки і права водіїв" },
            new Category { Id = CategoryConstants.Topic03_SpecialSignalsId, Name = "3. Рух із спецсигналами" },
            new Category { Id = CategoryConstants.Topic04_PedestriansId, Name = "4. Обов'язки пішоходів" },
            new Category { Id = CategoryConstants.Topic05_PassengersId, Name = "5. Обов'язки і права пасажирів" },
            new Category { Id = CategoryConstants.Topic06_CyclistsId, Name = "6. Вимоги до велосипедистів" },
            new Category { Id = CategoryConstants.Topic07_AnimalDriversId, Name = "7. Вимоги до осіб, які керують гужовим транспортом" },
            new Category { Id = CategoryConstants.Topic08_TrafficRegulationId, Name = "8. Регулювання дорожнього руху" },
            new Category { Id = CategoryConstants.Topic09_WarningSignalsId, Name = "9. Попереджувальні сигнали" },
            new Category { Id = CategoryConstants.Topic10_StartAndDirectionId, Name = "10. Початок руху та зміна напрямку" },
            new Category { Id = CategoryConstants.Topic11_PositionOnRoadId, Name = "11. Розташування ТЗ на дорозі" },
            new Category { Id = CategoryConstants.Topic12_SpeedLimitsId, Name = "12. Швидкість руху" },
            new Category { Id = CategoryConstants.Topic13_DistanceAndOvertakingId, Name = "13. Дистанція, інтервал, зустрічний роз'їзд" },
            new Category { Id = CategoryConstants.Topic14_OvertakingId, Name = "14. Обгін" },
            new Category { Id = CategoryConstants.Topic15_StopAndParkId, Name = "15. Зупинка і стоянка" },
            new Category { Id = CategoryConstants.Topic16_IntersectionsId, Name = "16. Проїзд перехресть" },
            new Category { Id = CategoryConstants.Topic17_PublicTransportId, Name = "17. Переваги маршрутних ТЗ" },
            new Category { Id = CategoryConstants.Topic18_PedestrianCrossingsId, Name = "18. Проїзд пішохідних переходів і зупинок" },
            new Category { Id = CategoryConstants.Topic19_LightsId, Name = "19. Користування зовнішніми світловими приладами" },
            new Category { Id = CategoryConstants.Topic20_RailwayCrossingsId, Name = "20. Рух через залізничні переїзди" },
            new Category { Id = CategoryConstants.Topic21_PassengersTransportId, Name = "21. Перевезення пасажирів" },
            new Category { Id = CategoryConstants.Topic22_CargoTransportId, Name = "22. Перевезення вантажу" },
            new Category { Id = CategoryConstants.Topic23_TowingId, Name = "23. Буксирування та експлуатація складів" },
            new Category { Id = CategoryConstants.Topic24_TrainingDrivingId, Name = "24. Навчальна їзда" },
            new Category { Id = CategoryConstants.Topic25_ConvoyDrivingId, Name = "25. Рух у колонах" },
            new Category { Id = CategoryConstants.Topic26_ResidentialZonesId, Name = "26. Рух у житловій та пішохідній зоні" },
            new Category { Id = CategoryConstants.Topic27_HighwaysId, Name = "27. Рух по автомагістралях і дорогах для автомобілів" },
            new Category { Id = CategoryConstants.Topic28_MountainRoadsId, Name = "28. Рух по гірських дорогах і на крутих спусках" },
            new Category { Id = CategoryConstants.Topic29_InternationalTrafficId, Name = "29. Міжнародний рух" },
            new Category { Id = CategoryConstants.Topic30_LicensePlatesId, Name = "30. Номерні, розпізнавальні знаки" },
            new Category { Id = CategoryConstants.Topic31_TechConditionId, Name = "31. Технічний стан ТЗ" },
            new Category { Id = CategoryConstants.Topic32_OtherQuestionsId, Name = "32. Окремі питання, що потребують узгодження" },
            new Category { Id = CategoryConstants.RoadMarkingsId, Name = "34. Дорожня розмітка" },
            new Category { Id = CategoryConstants.MedicineId, Name = "Медична допомога" }
        );
        
        var adminRoleId = "b50a741a-0cfe-4196-aa42-5a759347a851";
        modelBuilder.Entity<IdentityRole>().HasData( new IdentityRole
        {
            Id = adminRoleId,
            Name = "Admin",
            NormalizedName = "ADMIN"
        });
        
    }
    
}