namespace WorkSpace.Application.DTOs.Bookings;

public class BookingQuoteResponse
{
    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal FinalAmount { get; set; }
    public string Currency { get; set; } = "VND";
}