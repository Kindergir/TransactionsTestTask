namespace JFF.DTO;

public record Period(DateTimeOffset Start, DateTimeOffset End)
{
    public bool Contains(DateTimeOffset date) =>
        date >= Start && date < End;
}