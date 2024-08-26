namespace KFC.ViewModel {
    public class FinanceMenuItem
    {
        public int Amount { get; set; }
        public string? Time { get; set; }
        public string? Message { get; set; }

        public FinanceMenuItem(int amount, string? time, string? message)
        {
            Amount = amount;
            Time = time;
            Message = message;
        }
    }
}
