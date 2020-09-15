using System;
using System.Collections.Generic;
using System.Text;

namespace VideoSwitcher.Core
{
    public interface Switcher
    {
        Guid Id { get; }
        string ProductName { get; }

        IList<SwitcherInput> Inputs { get; }

        SwitcherInput CurrentProgramInput { get; }

        event EventHandler CurrentProgramInputChanged;

        void SetProgramInput(long id);
    }
}
