namespace wumgr
{
    internal struct WpfOperationResultMessage
    {
        public bool ShowDialog;
        public string Message;
        public string DuplicateDescription;
        public string AdditionalLogMessage;

        public bool HasMessage { get { return !string.IsNullOrEmpty(Message); } }
        public bool HasAdditionalLogMessage { get { return !string.IsNullOrEmpty(AdditionalLogMessage); } }

        public static WpfOperationResultMessage Create(WuAgent.AgentOperation operation, WuAgent.RetCodes ret, bool reboot, string operationLabel, string retLabel)
        {
            if (ret == WuAgent.RetCodes.InProgress)
                return new WpfOperationResultMessage();

            if (ret == WuAgent.RetCodes.Abborted)
            {
                return new WpfOperationResultMessage
                {
                    Message = operationLabel + " aborted."
                };
            }

            if (operation == WuAgent.AgentOperation.InstallingUpdates && reboot && ret == WuAgent.RetCodes.Success)
            {
                string message = Translate.fmt("msg_inst_done");
                return new WpfOperationResultMessage
                {
                    ShowDialog = true,
                    Message = message,
                    DuplicateDescription = operationLabel + ": " + message
                };
            }

            if (ret == WuAgent.RetCodes.Success)
                return new WpfOperationResultMessage();

            WpfOperationResultMessage result = new WpfOperationResultMessage
            {
                ShowDialog = true,
                Message = retLabel,
                DuplicateDescription = operationLabel + ": " + retLabel
            };

            if (reboot)
                result.AdditionalLogMessage = "A reboot is required to finish the operation.";

            return result;
        }
    }
}
