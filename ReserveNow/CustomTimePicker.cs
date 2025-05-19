using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace ReserveNow.Controls
{
    public class CustomTimePicker:Picker
    {
        public TimeSpan MinimumTime
        {
            get => (TimeSpan)GetValue(MinimumTimeProperty);
            set => SetValue(MinimumTimeProperty, value);
        }

        public static readonly BindableProperty MinimumTimeProperty =
            BindableProperty.Create(
                nameof(MinimumTime),
                typeof(TimeSpan),
                typeof(CustomTimePicker),
                new TimeSpan(0, 0, 0), // Значение по умолчанию: 00:00
                propertyChanged: OnTimeRangeChanged);

        // Свойство для максимального времени
        public TimeSpan MaximumTime
        {
            get => (TimeSpan)GetValue(MaximumTimeProperty);
            set => SetValue(MaximumTimeProperty, value);
        }

        public static readonly BindableProperty MaximumTimeProperty =
            BindableProperty.Create(
                nameof(MaximumTime),
                typeof(TimeSpan),
                typeof(CustomTimePicker),
                new TimeSpan(23, 59, 59), // Значение по умолчанию: 23:59
                propertyChanged: OnTimeRangeChanged);

        // Конструктор
        public CustomTimePicker()
        {
            PopulateTimeRange();
        }

        // Метод для заполнения диапазона времени
        private void PopulateTimeRange()
        {
            if (MinimumTime > MaximumTime)
            {
                Console.WriteLine("Error: MinimumTime is greater than MaximumTime.");
                return;
            }

            Items.Clear();

            Console.WriteLine($"Populating time range from {MinimumTime} to {MaximumTime}");

            for (var time = MinimumTime; time <= MaximumTime; time = time.Add(TimeSpan.FromMinutes(30)))
            {
                Items.Add(time.ToString(@"hh\:mm")); // Формат времени (например, "10:00")
            }

            Console.WriteLine($"Items count: {Items.Count}");
        }

        // Обработчик изменения свойств MinimumTime или MaximumTime
        private static void OnTimeRangeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (CustomTimePicker)bindable;
            picker.PopulateTimeRange();
        }
    }
}
