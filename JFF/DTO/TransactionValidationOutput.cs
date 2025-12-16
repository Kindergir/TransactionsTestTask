namespace JFF.DTO;

//DTO in a real world
internal struct TransactionValidationOutput(string id, string customerId, bool accepted)
{
    public string Id { get; set; } = id;
    public string CustomerId { get; set; } = customerId;
    public bool Accepted { get; set; } = accepted;
}