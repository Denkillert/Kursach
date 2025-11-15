using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RowingBaseAccounting
{

    // Класс услуги
    public class Service
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public bool IsForLegalEntitiesOnly { get; set; }
        public int BaseNumber { get; set; }
        public string ServiceType { get; set; }
        public int Capacity { get; set; }
        public double DurationHours { get; set; }
        public int SessionsCount { get; set; }

        public decimal CalculateCost(Client client, double quantity = 1)
        {
            decimal cost = Price;

            if (ServiceType != "Терраса" && ServiceType != "Сауна" && ServiceType != "Абонемент")
                cost = Price * (decimal)quantity;

            return cost * (1 - client.DiscountPercent / 100);
        }

        public bool IsAvailableFor(Client client)
        {
            if (IsForLegalEntitiesOnly && client.Type != "Юр.лицо")
                return false;
            return true;
        }
    }

    // Менеджер услуг
    public static class ServiceManager
    {
        private static List<Service> services = new List<Service>();

        static ServiceManager()
        {
            InitializeServices();
        }

        public static void InitializeServices()
        {
            // База №1
            services.AddRange(new[]
            {
            new Service {
                Name = "Открытая площадка для игр",
                Price = 13.00m, Unit = "1 час",
                Description = "Волейбол, баскетбол, футбол",
                ServiceType = "Площадка", BaseNumber = 1
            },
            new Service {
                Name = "Организация мероприятия (площадка)",
                Price = 22.00m, Unit = "1 час",
                Description = "Организация и сопровождение мероприятия",
                IsForLegalEntitiesOnly = true, BaseNumber = 1
            },
            new Service {
                Name = "Тренажерный зал",
                Price = 4.00m, Unit = "1 чел./час",
                Description = "Современный тренажерный зал",
                ServiceType = "Тренажерный зал", BaseNumber = 1
            },
            new Service {
                Name = "Крытая терраса (30 мест)",
                Price = 45.00m, Unit = "посещение",
                Description = "Просторная терраса для мероприятий",
                ServiceType = "Терраса", Capacity = 30, DurationHours = 2, BaseNumber = 1
            },
            new Service {
                Name = "Крытая терраса (15 мест)",
                Price = 25.00m, Unit = "посещение",
                Description = "Компактная терраса для мероприятий",
                ServiceType = "Терраса", Capacity = 15, DurationHours = 2, BaseNumber = 1
            },
            new Service {
                Name = "Сауна (для юр.лиц)",
                Price = 125.00m, Unit = "сеанс",
                Description = "Сауна для корпоративных мероприятий",
                ServiceType = "Сауна", Capacity = 5, DurationHours = 2,
                IsForLegalEntitiesOnly = true, BaseNumber = 1
            },
            new Service {
                Name = "Сауна (для физ.лиц)",
                Price = 118.50m, Unit = "сеанс",
                Description = "Сауна для частных лиц",
                ServiceType = "Сауна", Capacity = 5, DurationHours = 2, BaseNumber = 1
            }
        });

            // База №2
            services.AddRange(new[]
            {
            new Service {
                Name = "Тренажёрный зал",
                Price = 4.00m, Unit = "1 чел./час",
                Description = "Основной тренажерный зал",
                ServiceType = "Тренажерный зал", BaseNumber = 2
            },
            new Service {
                Name = "Зал силовой подготовки",
                Price = 4.00m, Unit = "1 чел./час",
                Description = "Зал штанги и силовых тренировок",
                ServiceType = "Тренажерный зал", BaseNumber = 2
            },
            new Service {
                Name = "Зал игровых видов спорта",
                Price = 55.00m, Unit = "1 час",
                Description = "Универсальный зал для игровых видов",
                ServiceType = "Площадка", BaseNumber = 2
            },
            new Service {
                Name = "Гребной бассейн",
                Price = 29.00m, Unit = "1 час",
                Description = "Гребной бассейн для тренировок",
                ServiceType = "Бассейн", IsForLegalEntitiesOnly = true, BaseNumber = 2
            },
            new Service {
                Name = "Бокс для хранения лодочных моторов",
                Price = 2.15m, Unit = "1 место/сутки",
                Description = "Место в боксе для хранения",
                ServiceType = "Хранение", IsForLegalEntitiesOnly = true, BaseNumber = 2
            },
            new Service {
                Name = "Эллинг для лодок",
                Price = 2.15m, Unit = "1 лодко-место/сутки",
                Description = "Место в эллинге для лодок",
                ServiceType = "Хранение", IsForLegalEntitiesOnly = true, BaseNumber = 2
            },
            new Service {
                Name = "Зал концептов",
                Price = 5.40m, Unit = "1 чел./час",
                Description = "Специализированный зал для концептов",
                IsForLegalEntitiesOnly = true, BaseNumber = 2
            },
            new Service {
                Name = "Сауна (1 кабинка)",
                Price = 120.00m, Unit = "сеанс",
                Description = "Сауна для юр.лиц, 1 кабинка",
                ServiceType = "Сауна", Capacity = 5, DurationHours = 2,
                IsForLegalEntitiesOnly = true, BaseNumber = 2
            },
            new Service {
                Name = "Сауна (2 кабинки)",
                Price = 195.00m, Unit = "сеанс",
                Description = "Сауна для юр.лиц, 2 кабинки",
                ServiceType = "Сауна", Capacity = 9, DurationHours = 2,
                IsForLegalEntitiesOnly = true, BaseNumber = 2
            }
        });
        }

        public static List<Service> GetServices(int baseNumber)
        {
            return services.Where(s => s.BaseNumber == baseNumber).ToList();
        }

        public static Service GetService(string serviceName, int baseNumber)
        {
            return services.FirstOrDefault(s => s.Name == serviceName && s.BaseNumber == baseNumber);
        }

        public static void UpdateServicePrice(string serviceName, int baseNumber, decimal newPrice)
        {
            var service = GetService(serviceName, baseNumber);
            if (service != null)
                service.Price = newPrice;
        }

        public static void UpdateServiceNotes(string serviceName, int baseNumber, string notes)
        {
            var service = GetService(serviceName, baseNumber);
            if (service != null)
                service.Notes = notes;
        }
    }
}
