# Flairy Version 0.2  

## Опис версії  

### Створено:  
- **Головне вікно** – менеджер сторінок.  
- **Сторінка пошуку** – містить елементи управління (User Control) для пошуку авіарейсу.  

### Елементи керування для пошуку:  
- ✈ **Обирач аеропорту**  
- 📅 **Обирач дати**  
- 👥 **Обирач кількості людей**  

### Додано:  
**🔹 Конвертори**  
- Реалізовано конвертори для `UserControl`, що забезпечують коректну взаємодію з біндінгом.  
- Додані конвертори для сторінки пошуку, які покращують взаємодію з `UserControl`.  

**🔹 Підтримка MVVM**  
- Підключено бібліотеку `Xaml Behavior` для покращеної підтримки MVVM.  

**🔹 Оформлення**  
- У ресурси додатку додано базову кольорову гаму.
- Створено статичний клас, що дозволяє отримувати кольори з `DynamicResource` у коді.  
- У ресурси додатку додано дані для SVG-графіки.

**🔹 Елемент керування для головного вікна**  
- Реалізовано простий UI-елемент для виведення повідомлень на екран.  

**🔹 Робота з базою даних**  
- `UserControl` для обирання аеропорту взаємодіє з базою даних.  
- До бази даних додано три тестові аеропорти.
## Коментарі  
Основний фокус у цій версії – створення сторінки для пошуку авіарейсів за вказаними даними.  <br>
⚠ **Ця версія сторінки пошуку не є фінальною та буде вдосконалюватися у майбутньому.**  
