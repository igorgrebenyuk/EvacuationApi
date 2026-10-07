using EvacuationApi.Models;

namespace EvacuationApi.Data;

/// <summary>
/// Демо-данные для первого запуска (включается настройкой SeedDemoData в appsettings.json).
/// </summary>
public static class DbSeeder
{
    public static void SeedDemoData(AppDbContext db)
    {
        if (db.Employees.Any() || db.Vehicles.Any() || db.ReceptionPoints.Any() || db.Routes.Any())
            return;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var points = new[]
        {
            new ReceptionPoint
            {
                Name = "ПВР №1 — Дом культуры п. Лесной",
                Address = "п. Лесной, ул. Школьная, 5",
                HostOrganization = "Администрация п. Лесной",
                ContactPerson = "Громова Елена Сергеевна",
                ContactPhone = "+7 900 111-22-33",
                Capacity = 25,
                AgreementNumber = "СОГЛ-12/2025"
            },
            new ReceptionPoint
            {
                Name = "ПВР №2 — Школа д. Заречье",
                Address = "д. Заречье, ул. Центральная, 12",
                HostOrganization = "МБОУ «Заречная школа»",
                ContactPerson = "Кузнецов Андрей Петрович",
                ContactPhone = "+7 900 444-55-66",
                Capacity = 20,
                AgreementNumber = "СОГЛ-13/2025"
            }
        };
        db.ReceptionPoints.AddRange(points);
        db.SaveChanges();

        db.Routes.AddRange(
            new EvacuationRoute
            {
                Name = "Основной: музей → п. Лесной",
                Kind = RouteKind.Main,
                StartPoint = "ул. Музейная, 1 (главный вход)",
                ReceptionPointId = points[0].Id,
                DistanceKm = 38.5,
                TravelTimeMinutes = 55,
                Description = "Объездная дорога на север, поворот на п. Лесной на 32-м км."
            },
            new EvacuationRoute
            {
                Name = "Запасной: музей → д. Заречье",
                Kind = RouteKind.Reserve,
                StartPoint = "ул. Музейная, 1 (служебный въезд)",
                ReceptionPointId = points[1].Id,
                DistanceKm = 44,
                TravelTimeMinutes = 70,
                Description = "Через мост по ул. Речной, далее по региональной трассе."
            });

        db.Vehicles.AddRange(
            new Vehicle
            {
                Name = "Автобус ПАЗ-3205", PlateNumber = "А123ВС", Type = VehicleType.Bus, Capacity = 23,
                Ownership = VehicleOwnership.Own, DriverName = "Орлов Виктор Станиславович", DriverPhone = "+7 900 100-00-01"
            },
            new Vehicle
            {
                Name = "ГАЗель NEXT", PlateNumber = "К456МН", Type = VehicleType.Minibus, Capacity = 13,
                Ownership = VehicleOwnership.Own, DriverName = "Тихонов Максим Артёмович", DriverPhone = "+7 900 100-00-02"
            },
            new Vehicle
            {
                Name = "Автобус ЛиАЗ-5256", PlateNumber = "Е789ОР", Type = VehicleType.Bus, Capacity = 40,
                Ownership = VehicleOwnership.Contracted, ProviderName = "ООО «Автотранс»",
                ContractNumber = "Д-45/2026", ContractValidUntil = today.AddYears(1),
                DriverName = "Мельников Олег Тимофеевич", DriverPhone = "+7 900 200-00-01"
            },
            new Vehicle
            {
                Name = "Автобус Mercedes Tourismo", PlateNumber = "Т321УХ", Type = VehicleType.Bus, Capacity = 50,
                Ownership = VehicleOwnership.Contracted, ProviderName = "ИП Савельев",
                ContractNumber = "Д-12/2024", ContractValidUntil = today.AddMonths(-1)
            });

        var names = new[]
        {
            "Иванов Сергей Петрович", "Петрова Анна Ивановна", "Сидоров Алексей Николаевич", "Козлова Мария Андреевна",
            "Морозов Дмитрий Сергеевич", "Волкова Елена Викторовна", "Соколов Игорь Олегович", "Лебедева Ольга Павловна",
            "Новиков Андрей Юрьевич", "Фёдорова Татьяна Михайловна", "Крылов Павел Антонович", "Зайцева Наталья Романовна",
            "Белова Ирина Алексеевна", "Егорова Светлана Дмитриевна", "Громов Николай Васильевич", "Власова Галина Борисовна",
            "Комаров Денис Игоревич", "Субботина Юлия Евгеньевна", "Попова Людмила Фёдоровна", "Рябов Артём Константинович",
            "Жукова Валентина Степановна", "Кузьмин Роман Леонидович", "Никитина Алла Георгиевна", "Захаров Илья Маркович"
        };
        var departments = new[]
        {
            ("Экспозиционный отдел", "Научный сотрудник"),
            ("Фонды", "Хранитель фондов"),
            ("Охрана", "Охранник"),
            ("Администрация", "Специалист"),
            ("Служба эксплуатации", "Техник")
        };

        for (var i = 0; i < names.Length; i++)
        {
            var (department, position) = departments[i % departments.Length];
            db.Employees.Add(new Employee
            {
                FullName = names[i],
                Department = department,
                Position = position,
                Shift = i % 3 + 1,
                IsSubjectToEvacuation = i % 8 != 7,   // каждый 8-й остаётся на объекте (дежурный персонал)
                NeedsAssistance = i % 12 == 5,
                Phone = $"+7 900 300-00-{i + 1:00}"
            });
        }

        db.SaveChanges();
    }
}
