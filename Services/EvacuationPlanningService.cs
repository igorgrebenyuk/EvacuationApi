using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Repositories;

namespace EvacuationApi.Services;

public class EvacuationPlanningService : IEvacuationPlanningService
{
    private readonly IEmployeeRepository _employees;
    private readonly IVehicleRepository _vehicles;
    private readonly IReceptionPointRepository _points;
    private readonly IRouteRepository _routes;

    public EvacuationPlanningService(
        IEmployeeRepository employees,
        IVehicleRepository vehicles,
        IReceptionPointRepository points,
        IRouteRepository routes)
    {
        _employees = employees;
        _vehicles = vehicles;
        _points = points;
        _routes = routes;
    }

    // ---------------------------------------------------------------- расчёты

    public async Task<EvacuationSummaryDto> GetSummaryAsync()
    {
        var employees = await _employees.GetAllAsync();
        var vehicles = await _vehicles.GetAllAsync();
        var points = await _points.GetAllAsync();
        var routes = await _routes.GetAllAsync();

        var subject = employees.Where(e => e.IsSubjectToEvacuation).ToList();
        var available = vehicles.Where(v => v.IsAvailableForEvacuation).ToList();
        var availableOwn = available.Where(v => v.Ownership == VehicleOwnership.Own).ToList();
        var availableContracted = available.Where(v => v.Ownership == VehicleOwnership.Contracted).ToList();

        var seatsOwn = availableOwn.Sum(v => v.Capacity);
        var seatsContracted = availableContracted.Sum(v => v.Capacity);
        var seatsTotal = seatsOwn + seatsContracted;
        var placementCapacity = points.Sum(p => p.Capacity);

        var employeeStats = new EmployeeStatsDto(
            Total: employees.Count,
            SubjectToEvacuation: subject.Count,
            StayingOnSite: employees.Count - subject.Count,
            NeedAssistance: subject.Count(e => e.NeedsAssistance),
            AssignedToVehicle: subject.Count(e => e.VehicleId != null),
            AssignedToPoint: subject.Count(e => e.ReceptionPointId != null));

        int? waves = null;
        if (subject.Count == 0)
            waves = 0;
        else if (seatsTotal > 0)
            waves = (int)Math.Ceiling(subject.Count / (double)seatsTotal);

        var transportStats = new TransportStatsDto(
            VehiclesTotal: vehicles.Count,
            VehiclesAvailable: available.Count,
            VehiclesUnavailable: vehicles.Count - available.Count,
            OwnAvailable: availableOwn.Count,
            ContractedAvailable: availableContracted.Count,
            SeatsOwn: seatsOwn,
            SeatsContracted: seatsContracted,
            SeatsTotal: seatsTotal,
            SeatsDeficit: Math.Max(0, subject.Count - seatsTotal),
            Waves: waves,
            MinVehiclesForOneWave: MinVehiclesFor(subject.Count, available));

        var placementStats = new PlacementStatsDto(
            points.Count, placementCapacity, Math.Max(0, subject.Count - placementCapacity));

        var routeStats = new RouteStatsDto(
            routes.Count,
            routes.Count(r => r.Kind == RouteKind.Main),
            routes.Count(r => r.Kind == RouteKind.Reserve),
            routes.Count(r => r.HasMap));

        var shifts = employees
            .GroupBy(e => e.Shift)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var inShift = g.Count(e => e.IsSubjectToEvacuation);
                return new ShiftCalcDto(
                    g.Key,
                    g.Count(),
                    inShift,
                    g.Count(e => e.IsSubjectToEvacuation && e.NeedsAssistance),
                    Math.Max(0, inShift - seatsTotal),
                    Math.Max(0, inShift - placementCapacity));
            })
            .ToList();

        var warnings = BuildWarnings(
            subject.Count, vehicles, transportStats, placementStats, routes, points);

        return new EvacuationSummaryDto(
            employeeStats, transportStats, placementStats, routeStats, shifts, warnings);
    }

    private static List<string> BuildWarnings(
        int people,
        List<Vehicle> vehicles,
        TransportStatsDto transport,
        PlacementStatsDto placement,
        List<EvacuationRoute> routes,
        List<ReceptionPoint> points)
    {
        var warnings = new List<string>();

        if (people == 0)
            warnings.Add("Нет сотрудников, подлежащих эвакуации.");

        if (transport.VehiclesAvailable == 0)
            warnings.Add("Нет ни одного доступного транспортного средства.");
        else if (transport.SeatsDeficit > 0)
            warnings.Add($"Не хватает {transport.SeatsDeficit} мест в транспорте для эвакуации за один рейс "
                         + $"(потребуется рейсов: {transport.Waves}).");

        var expired = vehicles.Where(v => v.IsContractExpired).ToList();
        if (expired.Count > 0)
            warnings.Add($"Истёк договор на привлечённый транспорт: {string.Join(", ", expired.Select(v => v.Name))}.");

        if (placement.Points == 0)
            warnings.Add("Не задано ни одного пункта временного размещения.");
        else if (placement.Deficit > 0)
            warnings.Add($"Не хватает {placement.Deficit} мест в пунктах временного размещения.");

        if (!routes.Any(r => r.Kind == RouteKind.Main))
            warnings.Add("Не задан основной маршрут эвакуации.");

        var noMap = routes.Where(r => !r.HasMap).ToList();
        if (noMap.Count > 0)
            warnings.Add($"У маршрутов нет электронной карты: {string.Join(", ", noMap.Select(r => r.Name))}.");

        var noContacts = points.Where(p => string.IsNullOrWhiteSpace(p.ContactPhone)).ToList();
        if (noContacts.Count > 0)
            warnings.Add($"Не указан телефон принимающей стороны: {string.Join(", ", noContacts.Select(p => p.Name))}.");

        return warnings;
    }

    /// <summary>Минимальное число машин, чтобы вывезти всех за один рейс (берём самые вместительные).</summary>
    private static int? MinVehiclesFor(int people, List<Vehicle> available)
    {
        if (people == 0) return 0;

        var seats = 0;
        var count = 0;
        foreach (var capacity in available.Select(v => v.Capacity).OrderByDescending(c => c))
        {
            seats += capacity;
            count++;
            if (seats >= people) return count;
        }
        return null;
    }

    // ---------------------------------------------------------------- план

    public async Task<EvacuationPlanDto> GetPlanAsync()
    {
        var employees = await _employees.GetAllAsync(subjectOnly: true);
        var vehicles = await _vehicles.GetAllAsync();
        var points = await _points.GetAllAsync();
        return BuildPlan(employees, vehicles, points);
    }

    public async Task<EvacuationPlanDto> BuildAutoPlanAsync(int? shift)
    {
        var employees = await _employees.GetAllAsync();
        var allVehicles = await _vehicles.GetAllAsync();
        var points = await _points.GetAllAsync();

        // Маломобильные — первыми (им места в первую очередь), дальше по смене и подразделению,
        // чтобы коллеги из одного отдела ехали и размещались вместе.
        var candidates = employees
            .Where(e => e.IsSubjectToEvacuation && (shift == null || e.Shift == shift))
            .OrderByDescending(e => e.NeedsAssistance)
            .ThenBy(e => e.Shift)
            .ThenBy(e => e.Department)
            .ThenBy(e => e.FullName)
            .ToList();

        if (candidates.Count == 0)
            throw new InvalidOperationException("Нет сотрудников, подлежащих эвакуации.");

        // Сначала собственный транспорт, затем привлечённый; внутри — самый вместительный первым.
        var vehicleSlots = allVehicles
            .Where(v => v.IsAvailableForEvacuation)
            .OrderBy(v => v.Ownership)
            .ThenByDescending(v => v.Capacity)
            .ThenBy(v => v.Id)
            .Select(v => (v.Id, v.Capacity))
            .ToList();

        var pointSlots = points
            .OrderBy(p => p.Id)
            .Select(p => (p.Id, p.Capacity))
            .ToList();

        foreach (var e in employees)
        {
            e.VehicleId = null;
            e.ReceptionPointId = null;
        }

        Distribute(candidates, vehicleSlots, (e, id) => e.VehicleId = id);
        Distribute(candidates, pointSlots, (e, id) => e.ReceptionPointId = id);

        await _employees.SaveChangesAsync();

        return BuildPlan(employees.Where(e => e.IsSubjectToEvacuation).ToList(), allVehicles, points);
    }

    public async Task ResetPlanAsync()
    {
        var employees = await _employees.GetAllAsync();
        foreach (var e in employees)
        {
            e.VehicleId = null;
            e.ReceptionPointId = null;
        }
        await _employees.SaveChangesAsync();
    }

    /// <summary>Последовательно заполняет «ячейки» (машины / пункты) людьми, пока есть места.</summary>
    private static void Distribute(
        List<Employee> people,
        List<(int Id, int Capacity)> slots,
        Action<Employee, int> assign)
    {
        var index = 0;
        var used = 0;

        foreach (var person in people)
        {
            while (index < slots.Count && used >= slots[index].Capacity)
            {
                index++;
                used = 0;
            }

            if (index >= slots.Count) break;

            assign(person, slots[index].Id);
            used++;
        }
    }

    private static EvacuationPlanDto BuildPlan(
        List<Employee> subject, List<Vehicle> vehicles, List<ReceptionPoint> points)
    {
        static PlanPersonDto ToPerson(Employee e) =>
            new(e.Id, e.FullName, e.Shift, e.NeedsAssistance, e.VehicleId, e.ReceptionPointId);

        var vehiclePlans = vehicles
            .Select(v =>
            {
                var list = subject.Where(e => e.VehicleId == v.Id).Select(ToPerson).ToList();
                return new PlanVehicleDto(
                    v.Id, v.Name, v.PlateNumber, v.DriverName,
                    v.Capacity, list.Count, v.IsAvailableForEvacuation, list);
            })
            .Where(p => p.Available || p.Assigned > 0)
            .ToList();

        var pointPlans = points
            .Select(p => new PlanPointDto(
                p.Id, p.Name, p.Address, p.ContactPerson, p.ContactPhone,
                p.Capacity, subject.Count(e => e.ReceptionPointId == p.Id)))
            .ToList();

        var unassigned = subject
            .Where(e => e.VehicleId == null || e.ReceptionPointId == null)
            .Select(ToPerson)
            .ToList();

        return new EvacuationPlanDto(vehiclePlans, pointPlans, unassigned);
    }
}
