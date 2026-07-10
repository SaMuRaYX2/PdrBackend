namespace TrafficRules.Domain.Constants;

public class CategoryConstants
{
    // Дорожні знаки (Ваш існуючий ID)
    public static readonly Guid TrafficSignsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    // Розділи ПДР
    public static readonly Guid Topic01_GeneralId = Guid.Parse("11111111-1111-1111-1111-000000000001"); // 1. Загальні положення
    public static readonly Guid Topic02_DriversDutiesId = Guid.Parse("11111111-1111-1111-1111-000000000002"); // 2. Обов'язки і права водіїв
    public static readonly Guid Topic03_SpecialSignalsId = Guid.Parse("11111111-1111-1111-1111-000000000003"); // 3. Рух із спецсигналами
    public static readonly Guid Topic04_PedestriansId = Guid.Parse("11111111-1111-1111-1111-000000000004"); // 4. Обов'язки і права пішоходів
    public static readonly Guid Topic05_PassengersId = Guid.Parse("11111111-1111-1111-1111-000000000005"); // 5. Обов'язки і права пасажирів
    public static readonly Guid Topic06_CyclistsId = Guid.Parse("11111111-1111-1111-1111-000000000006"); // 6. Вимоги до велосипедистів
    public static readonly Guid Topic07_AnimalDriversId = Guid.Parse("11111111-1111-1111-1111-000000000007"); // 7. Вимоги до осіб, які керують гужовим транспортом
    public static readonly Guid Topic08_TrafficRegulationId = Guid.Parse("11111111-1111-1111-1111-000000000008"); // 8. Регулювання дорожнього руху
    public static readonly Guid Topic09_WarningSignalsId = Guid.Parse("11111111-1111-1111-1111-000000000009"); // 9. Попереджувальні сигнали
    public static readonly Guid Topic10_StartAndDirectionId = Guid.Parse("11111111-1111-1111-1111-000000000010"); // 10. Початок руху та зміна напрямку
    public static readonly Guid Topic11_PositionOnRoadId = Guid.Parse("11111111-1111-1111-1111-000000000011"); // 11. Розташування ТЗ на дорозі
    public static readonly Guid Topic12_SpeedLimitsId = Guid.Parse("11111111-1111-1111-1111-000000000012"); // 12. Швидкість руху
    public static readonly Guid Topic13_DistanceAndOvertakingId = Guid.Parse("11111111-1111-1111-1111-000000000013"); // 13. Дистанція, інтервал, зустрічний роз'їзд
    public static readonly Guid Topic14_OvertakingId = Guid.Parse("11111111-1111-1111-1111-000000000014"); // 14. Обгін
    public static readonly Guid Topic15_StopAndParkId = Guid.Parse("11111111-1111-1111-1111-000000000015"); // 15. Зупинка і стоянка
    public static readonly Guid Topic16_IntersectionsId = Guid.Parse("11111111-1111-1111-1111-000000000016"); // 16. Проїзд перехресть
    public static readonly Guid Topic17_PublicTransportId = Guid.Parse("11111111-1111-1111-1111-000000000017"); // 17. Переваги маршрутних ТЗ
    public static readonly Guid Topic18_PedestrianCrossingsId = Guid.Parse("11111111-1111-1111-1111-000000000018"); // 18. Проїзд пішохідних переходів і зупинок
    public static readonly Guid Topic19_LightsId = Guid.Parse("11111111-1111-1111-1111-000000000019"); // 19. Користування зовнішніми світловими приладами
    public static readonly Guid Topic20_RailwayCrossingsId = Guid.Parse("11111111-1111-1111-1111-000000000020"); // 20. Рух через залізничні переїзди
    public static readonly Guid Topic21_PassengersTransportId = Guid.Parse("11111111-1111-1111-1111-000000000021"); // 21. Перевезення пасажирів
    public static readonly Guid Topic22_CargoTransportId = Guid.Parse("11111111-1111-1111-1111-000000000022"); // 22. Перевезення вантажу
    public static readonly Guid Topic23_TowingId = Guid.Parse("11111111-1111-1111-1111-000000000023"); // 23. Буксирування та експлуатація складів
    public static readonly Guid Topic24_TrainingDrivingId = Guid.Parse("11111111-1111-1111-1111-000000000024"); // 24. Навчальна їзда
    public static readonly Guid Topic25_ConvoyDrivingId = Guid.Parse("11111111-1111-1111-1111-000000000025"); // 25. Рух у колонах
    public static readonly Guid Topic26_ResidentialZonesId = Guid.Parse("11111111-1111-1111-1111-000000000026"); // 26. Рух у житловій та пішохідній зоні
    public static readonly Guid Topic27_HighwaysId = Guid.Parse("11111111-1111-1111-1111-000000000027"); // 27. Рух по автомагістралях і дорогах для автомобілів
    public static readonly Guid Topic28_MountainRoadsId = Guid.Parse("11111111-1111-1111-1111-000000000028"); // 28. Рух по гірських дорогах і на крутих спусках
    public static readonly Guid Topic29_InternationalTrafficId = Guid.Parse("11111111-1111-1111-1111-000000000029"); // 29. Міжнародний рух
    public static readonly Guid Topic30_LicensePlatesId = Guid.Parse("11111111-1111-1111-1111-000000000030"); // 30. Номерні, розпізнавальні знаки
    public static readonly Guid Topic31_TechConditionId = Guid.Parse("11111111-1111-1111-1111-000000000031"); // 31. Технічний стан ТЗ
    public static readonly Guid Topic32_OtherQuestionsId = Guid.Parse("11111111-1111-1111-1111-000000000032"); // 32. Окремі питання, що потребують узгодження
    
    // Дорожня розмітка та інше
    public static readonly Guid RoadMarkingsId = Guid.Parse("11111111-1111-1111-1111-000000000034"); // 34. Дорожня розмітка
    public static readonly Guid MedicineId = Guid.Parse("11111111-1111-1111-1111-000000000099"); // Основи надання медичної допомоги
}