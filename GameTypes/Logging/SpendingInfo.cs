namespace KFC.Model.Logging {
    public class SpendingInfo
    {
        public int amount { get; set; }
        public long time_stamp { get; set; }
        public string spending_message { get; set; }
        public SpendingInfo()
        {
            this.amount = 0;
            this.time_stamp = 0;
            this.spending_message = string.Empty;
        }
        public SpendingInfo(int amount, long time_stamp)
        { 
            this.amount = amount;
            this.time_stamp = time_stamp;
            this.spending_message = string.Empty;
        }
        public SpendingInfo(int amount, long time_stamp, string spending_message)
        {
            this.amount = amount;
            this.time_stamp = time_stamp;
            this.spending_message = spending_message;
        }
    }
}
