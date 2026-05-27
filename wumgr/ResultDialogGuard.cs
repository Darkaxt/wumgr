namespace wumgr
{
    internal class ResultDialogGuard
    {
        private readonly object sync = new object();
        private bool active;

        public bool IsActive
        {
            get
            {
                lock (sync)
                    return active;
            }
        }

        public bool TryBegin()
        {
            lock (sync)
            {
                if (active)
                    return false;

                active = true;
                return true;
            }
        }

        public void End()
        {
            lock (sync)
                active = false;
        }

        public bool TryRun(System.Action action)
        {
            if (!TryBegin())
                return false;

            try
            {
                action();
            }
            finally
            {
                End();
            }

            return true;
        }
    }
}
