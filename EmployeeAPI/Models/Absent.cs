using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Absent //Đơn xin nghỉ phép
    {
        public Guid Id { get; set; }
        public string? Note { get; set; } //Nội dung nghỉ phép
        public Guid StaffId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; //Ngày tạo đơn
        public DateOnly AbsentDate { get; set; } //Ngày nghỉ phép
        public int AbsentDay { get; set; } //Số ngày nghỉ phép
        public AbsentStatus Status { get; set; } = AbsentStatus.Pending;
        //public bool isFined { get; set; } = false; // xử phạt
        public bool IsDeleted { get; set; } = false; //xóa mềm


        //public Staff Staff { get; set; }

    }
}
