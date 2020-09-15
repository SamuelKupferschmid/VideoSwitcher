using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using VideoSwitcher.Core;

namespace VideoSwitcher.BMD
{
    public class BmdSwitcher : Switcher
    {
        private IBMDSwitcher _switcher;
        private IBMDSwitcherMixEffectBlock _effectBlock;

        public event EventHandler CurrentProgramInputChanged;

        public void Connect(string deviceAddress)
        {
            var discovery = new CBMDSwitcherDiscoveryClass();
            discovery.ConnectTo(deviceAddress, out _switcher, out var reason);

            Inputs = Iterate<IBMDSwitcherInputIterator, IBMDSwitcherInput>(iterator => iterator.Next).Select(GetInput).Where(input => input.Cut).ToList();
            // ATEM Mini Pro only has one MixEffectBlock
            _effectBlock = Iterate<IBMDSwitcherMixEffectBlockIterator, IBMDSwitcherMixEffectBlock>(iterator => iterator.Next).First();

            updateCurrentProgramInput();
            _effectBlock.AddCallback(new EffectBlockHandler(this));
        }

        SwitcherInput GetInput(IBMDSwitcherInput comInput)
        {
            comInput.GetInputAvailability(out var availability);
            comInput.GetLongName(out var name);
            comInput.GetInputId(out var id);

            return new SwitcherInput
            {
                Id = id,
                Name = name,
                Cut = availability.HasFlag(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut)
            };
        }

        public string ProductName
        {
            get
            {
                _switcher.GetProductName(out string name); return name;
            }
        }

        public IList<SwitcherInput> Inputs { get; private set; }
        public Guid Id { get; } = Guid.NewGuid();

        protected void updateCurrentProgramInput()
        {
            _effectBlock.GetProgramInput(out var value);
            CurrentProgramInput = Inputs.FirstOrDefault(input => input.Id == value);
        }

        public SwitcherInput CurrentProgramInput { get; protected set; }

        delegate void Iterator<T>(out T next);

        IEnumerable<TElement> Iterate<TIterator, TElement>(Func<TIterator, Iterator<TElement>> nextSelector)
        {
            var guid = typeof(TIterator).GUID;

            _switcher.CreateIterator(ref guid, out var ptr);
            TIterator iterator = (TIterator)Marshal.GetObjectForIUnknown(ptr);

            TElement getNext()
            {
                nextSelector(iterator)(out TElement result);
                return result;
            }

            while (true)
            {
                TElement item = getNext();

                if (item == null) break;

                yield return item;
            }
        }

        public void SetProgramInput(long id)
        {
            _effectBlock.SetProgramInput(id);
        }

        class EffectBlockHandler : IBMDSwitcherMixEffectBlockCallback
        {
            private readonly BmdSwitcher switcher;

            public EffectBlockHandler(BmdSwitcher switcher)
            {
                this.switcher = switcher;
            }
            public void Notify(_BMDSwitcherMixEffectBlockEventType eventType)
            {
                switch (eventType)
                {
                    case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeProgramInputChanged:
                        switcher.updateCurrentProgramInput();
                        switcher.CurrentProgramInputChanged?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
        }

    }
}
