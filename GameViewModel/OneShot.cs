namespace KFC.ViewModel {
    public class OneShot
    {
        private bool used = false;
        private event EventHandler handler;
        public OneShot(EventHandler handler)
        {
            this.handler = handler;
            used = false;
        }

        public bool Call()
        {
            if (used) return false;
            handler?.Invoke(this, EventArgs.Empty);
            used = true;
            return true;
        }
    }
}
