namespace EmployeeAPI.Enums
{
    public enum LogStatus
    {
        None = 0,
        OnTime = 1,
        Late = 2,
        LeaveEarly = 3,
        LateAndLeaveEarly = 4,
        Overtime = 5,
        LateAndOvertime = 6,
        Absent = 7,
        OnHoliday = 8,
        OnHolidayLate = 9,
        OnHolidayLeaveEarly = 10,
        OnHolidayOvertime = 11,
        OnHolidayLateAndOvertime = 12,
        OnHolidayLateAndLeaveEarly = 13,
        Others = 14,
    }
}
