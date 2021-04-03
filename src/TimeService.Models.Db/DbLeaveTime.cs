using System;
using System.ComponentModel.DataAnnotations;

namespace LT.DigitalOffice.TimeService.Models.Db
{
    public class DbLeaveTime
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid WorkerUserId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int LeaveType { get; set; }
        public string Comment { get; set; }
    }
}
