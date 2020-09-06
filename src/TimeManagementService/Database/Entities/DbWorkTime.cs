using System;
using System.ComponentModel.DataAnnotations;

namespace LT.DigitalOffice.TimeManagementService.Database.Entities
{
    public class DbWorkTime
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
        public string Title { get; set; }
        [Required]
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
    }
}
