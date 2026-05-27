using System.Collections.Generic;

namespace wumgr
{
    internal struct UpdateSelectionState
    {
        public bool HasSelection;
        public bool HasUninstallableSelection;

        public static UpdateSelectionState FromUpdates(IEnumerable<MsUpdate> updates)
        {
            UpdateSelectionState state = new UpdateSelectionState();
            if (updates == null)
                return state;

            foreach (MsUpdate update in updates)
            {
                if (update == null)
                    continue;

                state.HasSelection = true;
                if ((update.Attributes & (int)MsUpdate.UpdateAttr.Uninstallable) != 0)
                    state.HasUninstallableSelection = true;
            }

            return state;
        }
    }
}
