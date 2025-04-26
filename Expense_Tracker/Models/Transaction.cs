using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        [Range(1,int.MaxValue, ErrorMessage = "Please Select a Category.")]
        public int CategoryId { get; set; } //Foreign Key
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        public int Amount { get; set; }
        [Column(TypeName ="nvarchar(50)")]
        public string? Note { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
