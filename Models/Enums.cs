namespace EvacuationApi.Models;

/// <summary>
/// Тип транспортного средства
/// </summary>
public enum VehicleType
{
    /// <summary>
    /// Автобус
    /// </summary>
    Bus = 0,

    /// <summary>
    /// Микроавтобус
    /// </summary>
    Minibus = 1,

    /// <summary>
    /// Грузовой автомобиль (для имущества и ценностей)
    ///</summary>
    Truck = 2,

    /// <summary>Легковой автомобиль.</summary>
    Car = 3
}

/// <summary>
/// Принадлежность транспорта (транспортная ведомость: собственный + привлечённый)
/// </summary>
public enum VehicleOwnership
{
    /// <summary>
    /// Собственный транспорт организации
    /// </summary>
    Own = 0,

    /// <summary>
    /// Привлечённый по договору
    /// </summary>
    Contracted = 1
}

/// <summary>
/// Техническая готовность транспортного средства
/// </summary>
public enum VehicleStatus
{
    /// <summary>
    /// Исправен, готов к выделению
    /// </summary>
    Ready = 0,

    /// <summary>
    /// На обслуживании / в ремонте
    /// </summary>
    Maintenance = 1,

    /// <summary>
    /// Недоступен (отозван, нет водителя и т.п.)
    /// </summary>
    Unavailable = 2
}

/// <summary>
/// Вид маршрута эвакуации
/// </summary>
public enum RouteKind
{
    /// <summary>
    /// Основной маршрут
    /// </summary>
    Main = 0,

    /// <summary>
    /// Запасной маршрут
    /// </summary>
    Reserve = 1
}
