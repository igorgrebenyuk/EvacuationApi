namespace EvacuationApi.DTOs;

/// <summary>
/// Расчёт по персоналу.
/// </summary>
public record EmployeeStatsDto(
    int Total,
    int SubjectToEvacuation,
    int StayingOnSite,
    int NeedAssistance,
    int AssignedToVehicle,
    int AssignedToPoint);

/// <summary>
/// Расчёт по транспорту (учитывается только исправный и с действующим договором).
/// </summary>
public record TransportStatsDto(
    int VehiclesTotal,
    int VehiclesAvailable,
    int VehiclesUnavailable,
    int OwnAvailable,
    int ContractedAvailable,
    int SeatsOwn,
    int SeatsContracted,
    int SeatsTotal,
    int SeatsDeficit,
    int? Waves,
    int? MinVehiclesForOneWave);

/// <summary>
/// Расчёт по пунктам временного размещения.
/// </summary>
public record PlacementStatsDto(int Points, int TotalCapacity, int Deficit);

/// <summary>
/// Статистика маршрутов.
/// </summary>
public record RouteStatsDto(int Total, int Main, int Reserve, int WithMap);

/// <summary>
/// Строка расчётной таблицы по смене.
/// </summary>
public record ShiftCalcDto(
    int Shift,
    int Total,
    int SubjectToEvacuation,
    int NeedAssistance,
    int TransportDeficit,
    int PlacementDeficit);

/// <summary>
/// Сводная расчётная таблица эвакуации.
/// </summary>
public record EvacuationSummaryDto(
    EmployeeStatsDto Employees,
    TransportStatsDto Transport,
    PlacementStatsDto Placement,
    RouteStatsDto Routes,
    List<ShiftCalcDto> Shifts,
    List<string> Warnings);

public record PlanPersonDto(
    int Id, string FullName, int Shift, bool NeedsAssistance, int? VehicleId, int? ReceptionPointId);

public record PlanVehicleDto(
    int VehicleId, string Name, string? PlateNumber, string? DriverName,
    int Capacity, int Assigned, bool Available, List<PlanPersonDto> Employees);

public record PlanPointDto(
    int PointId, string Name, string Address, string? ContactPerson, string? ContactPhone,
    int Capacity, int Assigned);

/// <summary>
/// План эвакуации: кто в каком транспорте и в какой пункт размещения назначен
/// </summary>
public record EvacuationPlanDto(
    List<PlanVehicleDto> Vehicles,
    List<PlanPointDto> Points,
    List<PlanPersonDto> Unassigned);
