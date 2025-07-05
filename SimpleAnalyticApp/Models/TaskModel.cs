using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleAnalyticApp.Models
{
    public class TaskModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string TaskName { get; set; }
        public string ModelType { get; set; }
        public string ModelVariation { get; set; }
        public string Detection { get; set; }
        public string InputURL { get; set; }
    }
}
