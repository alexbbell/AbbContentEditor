namespace AbbContentEditor.Models
{
    public class BankOperation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TheSumm { get; set; }
        public DateTime PayDate { get; set; } = DateTime.Now;
        public bool IsPayable { get; set; }
    }

    public class BankOperationReponse
    {
        public IEnumerable<BankOperation> BankOperations{ get; set; }
        public int Total { get; set; }
    }
}
