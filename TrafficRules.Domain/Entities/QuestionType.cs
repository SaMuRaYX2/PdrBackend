namespace TrafficRules.Domain.Entities;

public enum QuestionType
{
    SingleChoice = 0,    // 1. Звичайне (одна правильна відповідь)                                                                                                                                                          
    MultipleChoice = 1,  // 3. Декілька правильних відповідей                                                                                                                                                               
    Matching = 3,        // 5. Встановлення відповідності                                                                                                                                                                   
    TrueFalse = 2,       // 4. Правда / Неправда                                                                                                                                                                            
    Sequence = 4,        // 6. Послідовність дій                                                                                                                                                                            
    ImageAreaClick = 5,  // 7. Вибір області на зображенні                                                                                                                                                                  
    DragAndDrop = 6,     // 8. Перетягування (Drag & Drop)                                                                                                                                                                  
    NumberInput = 7,     // 9. Введення числа                                                                                                                                                                               
    ShortAnswer = 8      // 10. Введення короткої відповіді   
}