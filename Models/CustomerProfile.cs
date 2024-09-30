using Azure.Data.Tables;
using Azure;

namespace SemesterTwo.Models
{
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom properties
        public string? FirstName { get; set; }  // Nullable string
        public string? LastName { get; set; }   // Nullable string
        public string? Email { get; set; }      // Nullable string
        public string? PhoneNumber { get; set; } // Nullable string

        public CustomerProfile()
        {
            PartitionKey = "CustomerProfile";
            RowKey = Guid.NewGuid().ToString();
        }
    }
}