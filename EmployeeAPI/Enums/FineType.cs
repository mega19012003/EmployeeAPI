namespace EmployeeAPI.Enums
{
    public enum FineType
    {
        BadBehavior = 0, 
        AbsentWithoutPermission = 1, //Vắng mặt không phép
        Late = 2, //Đi trễ
        EarlyLeave = 3, //Về sớm
        /*UnapprovedLeave = 4, //Nghỉ phép không được duyệt
        UnapprovedOvertime = 5, //Tăng ca không được duyệt
        UnapprovedLeaveEarly = 6, //Về sớm không được duyệt*/
    }
}
