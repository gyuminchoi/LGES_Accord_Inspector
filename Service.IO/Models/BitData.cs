using Prism.Commands;
using Prism.Mvvm;
using Service.IO.Services;
using Service.Logger.Services;
using System;

namespace Service.IO.Models
{
    public class BitData : BindableBase
    {
        #region Fields For Properties
        private bool _value = false;
        private int _myIndex = -1;
        private int _slotIndex = -1;

        #endregion

        public bool Value { get => _value; set => SetProperty(ref _value, value); }
        public int MyIndex { get => _myIndex; set => SetProperty(ref _myIndex, value); }
        public int SlotIndex { get => _slotIndex; set => SetProperty(ref _slotIndex, value); }

        public DelegateCommand ChangeClickCommand => new DelegateCommand(OnChangeClick);

        public BitData(BitData data)
        {
            Value = data.Value;
            MyIndex = data.MyIndex;
            SlotIndex = data.SlotIndex;
        }

        public BitData()
        {
                
        }

        public void OnChangeClick()
        {
            try
            {
                int slot = SlotIndex;
                int num = MyIndex;
                if (MyIndex > 7)
                {
                    slot++;
                    num -= 8;
                }

                IOManager.Instance.WriteOutputBit(slot, num, !Value);
            }
            catch (Exception ex) { LogWrite.Instance?.Error(ex); }
        }
    }
}
