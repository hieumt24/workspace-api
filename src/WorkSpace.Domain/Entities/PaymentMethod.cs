using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class PaymentMethod
{
    public int PaymentMethodID { get; set; }
    public string PaymentMethodName { get; set; }
    public decimal PaymentCost { get; set; }

    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; }

}