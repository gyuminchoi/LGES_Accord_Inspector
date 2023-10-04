using CrevisFnIOLib;
using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.IO.Models;
using Service.Logger.Services;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.IO.Services
{
    internal class IOManager : LazySingleton<IOManager>, IDisposable
    {
        private readonly LogWrite _logWrite = LogWrite.Instance;
        private CrevisFnIO _fnIO = new CrevisFnIO();
        private Thread _fnIOSignalCheckThread = new Thread(() => { });
        private IntPtr _hSystem = new IntPtr();
        private IntPtr _hDevice = new IntPtr();
        public string Version = "1.0.0";
        public List<IOSlot> DOutputs { get; set; } = new List<IOSlot>();
        public List<IOSlot> DInputs { get; set; } = new List<IOSlot>();
        public List<IOSlot> AInputs { get; set; } = new List<IOSlot>();
        public List<IOSlot> AOutputs { get; set; } = new List<IOSlot>();
        public bool IsOpen { get; set; } = false;
        public bool IsRun { get; set; } = false;
        public string IPAddress { get; set; }

        private IOManager() { }
        ~IOManager() => Dispose();

        //Open하면 init, open device까지 진행
        public void Open(string ipAddress)
        {
            _logWrite.DefaultSet();
            try
            {
                if (IsOpen)
                {
                    Close();
                    _fnIO = new CrevisFnIO();
                }
                //시스템 열기
                int status = _fnIO.FNIO_LibInitSystem(ref _hSystem);
                if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                    throw new FnIOException(status, EFnIOError.FnIOInitFail);

                //IO IP불러오기
                CrevisFnIO.DEVICEINFOMODBUSTCP2 modTCP = new CrevisFnIO.DEVICEINFOMODBUSTCP2 { IpAddress = new byte[4] };
                try
                {
                    modTCP.IpAddress[0] = byte.Parse(ipAddress.Split('.')[0]);
                    modTCP.IpAddress[1] = byte.Parse(ipAddress.Split('.')[1]);
                    modTCP.IpAddress[2] = byte.Parse(ipAddress.Split('.')[2]);
                    modTCP.IpAddress[3] = byte.Parse(ipAddress.Split('.')[3]);
                }
                catch
                {
                    throw new FnIOException(null, EFnIOError.InvalidIPAddress);
                }

                //디바이스 열기
                IntPtr _Hdev = new IntPtr();
                status = _fnIO.FNIO_DevOpenDevice(_hSystem, ref modTCP, CrevisFnIO.MODBUS_TCP, ref _Hdev);
                _hDevice = _Hdev;
                if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                    throw new FnIOException(status, EFnIOError.FnIOOpenFail);

                StringBuilder saveIP = new StringBuilder();
                saveIP.Append(modTCP.IpAddress[0]);
                saveIP.Append('.');
                saveIP.Append(modTCP.IpAddress[1]);
                saveIP.Append('.');
                saveIP.Append(modTCP.IpAddress[2]);
                saveIP.Append('.');
                saveIP.Append(modTCP.IpAddress[3]);
                IPAddress = saveIP.ToString();

                status = _fnIO.FNIO_DevSetParam(_hDevice, CrevisFnIO.DEV_UPDATE_FREQUENCY, 30);
                if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                    throw new FnIOException(status, EFnIOError.FnIOSetFrequencyFail);

                //사용가능한 IO 개수 가져오기
                int slotNum = 0;
                status = _fnIO.FNIO_DevGetParam(_hDevice, CrevisFnIO.DEV_EXPANSION_SLOT_NUMBER, ref slotNum);
                if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                    throw new FnIOException(status, EFnIOError.FnIOGetSlotNumberFail);

                //In / Out 구분하기
                IntPtr phSlot = new IntPtr();
                int inputCount = 0;
                int outputCount = 0;
                string strBuff = "";
                /////////////////////////////////////////////////////////////////////////
                for (int i = 0; i < slotNum; i++)
                {
                    //활성 슬롯 주소 가져오기
                    status = _fnIO.FNIO_IoGetIoModule(_hDevice, i, ref phSlot);
                    if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                        throw new FnIOException(status, EFnIOError.FnIOIOGetIOModuleFail);

                    strBuff = "";
                    //IO Desc 가져오기
                    status = _fnIO.FNIO_IoGetParam(phSlot, CrevisFnIO.IO_MODULE_DESCRIPTION, ref strBuff);
                    if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                        throw new FnIOException(status, EFnIOError.FnIOGetParamFail);

                    string nameIO = strBuff.Split(',')[0];
                    switch (nameIO.Split('-')[1].ToCharArray()[0])
                    {
                        case '1':
                            AddDInputs(phSlot, ref inputCount, nameIO);
                            break;

                        case '2':
                            AddDOutputs(phSlot, ref outputCount, nameIO);
                            break;

                        case '3':
                            AddAInputs(phSlot, ref inputCount, ref outputCount, nameIO);
                            break;

                        case '4':
                            AddAOutputs(phSlot, ref outputCount, nameIO);
                            break;

                        case '5':
                            AddAInputs(phSlot, ref inputCount, ref outputCount, nameIO);
                            break;

                        case '7':
                            break;
                        default:
                            AddDInputs(phSlot, ref inputCount, nameIO);
                            AddDOutputs(phSlot, ref outputCount, nameIO);
                            break;
                    }

                }
                IsOpen = true;
            }
            catch
            {
                Close();
                throw;
            }
        }

        //close, free까지 진행
        public void Close()
        {
            if (IsRun) { Stop(); }
            if (IsOpen) { _ = _fnIO.FNIO_DevCloseDevice(_hDevice); }
            IsOpen = false;
            _ = _fnIO.FNIO_LibFreeSystem(_hSystem);
            DInputs.Clear();
            DOutputs.Clear();
            AOutputs.Clear();
            AInputs.Clear();
        }

        //Update start 및 IO 신호 체크
        public void Start(bool isUseUpdateThread)
        {
            int status = 0;
            if (!IsRun)
            {
                status = _fnIO.FNIO_DevIoUpdateStart(_hDevice, CrevisFnIO.IO_UPDATE_PERIODIC);
                if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                    throw new FnIOException(status, EFnIOError.FnIOUpdateStartFail);

                IsRun = true;

                if (isUseUpdateThread) { UpdateThreadStart(); }
            }
        }

        //Update Stop 만 진행
        public void Stop()
        {
            if (IsRun) { _ = _fnIO.FNIO_DevIoUpdateStop(_hDevice); }

            IsRun = false;
            if (_fnIOSignalCheckThread.IsAlive)
            {
                if (!_fnIOSignalCheckThread.Join(100)) { _fnIOSignalCheckThread.Abort(); }
            }
        }

        public void DInputUpdate()
        {
            int bit = 0;

            foreach (var input in DInputs)
            {
                for (int i = 0; i < input.Size; i++)
                {
                    if (i > 23)
                    {
                        _fnIO.FNIO_DevReadInputImageBit(_hDevice, input.Index + 3, i - 24, ref bit);
                        input.DataBits[i].Value = Convert.ToBoolean(bit);

                        continue;
                    }

                    if (i > 15)
                    {
                        _fnIO.FNIO_DevReadInputImageBit(_hDevice, input.Index + 2, i - 16, ref bit);
                        input.DataBits[i].Value = Convert.ToBoolean(bit);

                        continue;
                    }

                    if (i > 7)
                    {
                        _fnIO.FNIO_DevReadInputImageBit(_hDevice, input.Index + 1, i - 8, ref bit);
                        input.DataBits[i].Value = Convert.ToBoolean(bit);

                        continue;
                    }

                    _fnIO.FNIO_DevReadInputImageBit(_hDevice, input.Index, i, ref bit);
                    input.DataBits[i].Value = Convert.ToBoolean(bit);
                }
            }
        }

        public void DOutputUpdate()
        {
            int bit = 0;

            foreach (var output in DOutputs)
            {
                for (int i = 0; i < output.Size; i++)
                {
                    if (i > 7)
                    {
                        _fnIO.FNIO_DevReadOutputImageBit(_hDevice, output.Index + 1, i - 8, ref bit);
                        output.DataBits[i].Value = Convert.ToBoolean(bit);
                    }
                    else
                    {
                        _fnIO.FNIO_DevReadOutputImageBit(_hDevice, output.Index, i, ref bit);
                        output.DataBits[i].Value = Convert.ToBoolean(bit);
                    }
                }
            }
        }

        public void AInputUpdate()
        {
            byte @byte = 0;

            foreach (var input in AInputs)
            {
                for (int i = 0; i < input.DataBytes.Length; i++)
                {
                    int err = _fnIO.FNIO_DevReadInputImage(_hDevice, input.Index + i, ref @byte, 1);
                    input.DataBytes[i] = @byte;
                }
            }
        }

        public void AOutputUpdate()
        {
            byte @byte = 0;

            foreach (var output in AOutputs)
            {
                for (int i = 0; i < output.DataBytes.Length; i++)
                {
                    int err = _fnIO.FNIO_DevReadOutputImage(_hDevice, output.Index + i, ref @byte, 1);
                    output.DataBytes[i] = @byte;
                }
            }
        }

        public void AddDInputs(IntPtr phSlot, ref int inputCount, string name)
        {
            int bitSize = 0;

            int status = _fnIO.FNIO_IoGetParam(phSlot, CrevisFnIO.IO_INPUT_BIT_SIZE, ref bitSize);
            if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                throw new FnIOException(status, EFnIOError.FnIOGetParamFail);

            //Input추가
            if (bitSize != 0)
            {
                BitData[] dbs = new BitData[bitSize];
                for (int i = 0; i < dbs.Length; i++) { dbs[i] = new BitData() { MyIndex = i, SlotIndex = inputCount }; }
                DInputs.Add(new IOSlot
                {
                    DataBits = dbs,
                    IOType = EIOType.DigitalInput,
                    Index = inputCount,
                    Size = bitSize,
                    Name = name
                });

                if (bitSize % 8 == 0) { inputCount += bitSize / 8; }
                else { inputCount += bitSize / 8 + 1; }
            }
        }

        public void AddDOutputs(IntPtr phSlot, ref int outputCount, string name)
        {
            int bitSize = 0;

            int status = _fnIO.FNIO_IoGetParam(phSlot, CrevisFnIO.IO_OUTPUT_BIT_SIZE, ref bitSize);
            if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                throw new FnIOException(status, EFnIOError.FnIOGetParamFail);

            //Output 추가
            if (bitSize != 0)
            {
                BitData[] dbs = new BitData[bitSize];
                for (int i = 0; i < dbs.Length; i++) { dbs[i] = new BitData() { MyIndex = i, SlotIndex = outputCount }; }
                DOutputs.Add(new IOSlot
                {
                    DataBits = dbs,
                    IOType = EIOType.DigitalOutput,
                    Index = outputCount,
                    Size = bitSize,
                    Name = name
                });
                if (bitSize % 8 == 0) { outputCount += bitSize / 8; }
                else { outputCount += bitSize / 8 + 1; }
            }

        }

        // TODO : AInput 추가 메서드
        public void AddAInputs(IntPtr phSlot, ref int inputCount, ref int outputCount, string name)
        {
            int inputBitSize = 0;
            int inputByteSize = 0;
            int status = _fnIO.FNIO_IoGetParam(phSlot, CrevisFnIO.IO_INPUT_BIT_SIZE, ref inputBitSize);
            if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                throw new FnIOException(status, EFnIOError.FnIOGetParamFail);

            if (inputBitSize % 8 == 0)
                inputByteSize = inputBitSize / 8;
            else
                inputByteSize = inputBitSize / 8 + 1;

            AInputs.Add(new IOSlot
            {
                DataBytes = new byte[inputByteSize],
                Index = inputCount,
                IOType = EIOType.AnalogInput,
                //Size = inputByteSize % 2 == 0 ? inputByteSize / 2 : inputByteSize / 2 + 1,
                Size = inputByteSize,
                Name = name.Substring(0, 7)
            });
            inputCount += inputByteSize;

            string nameIO = name.Split(',')[0];
            switch (nameIO.Split('-')[1])
            {
                case "3102":
                    AddDOutputs(phSlot, ref outputCount, name);
                    break;

                case "5221":
                case "5231":
                case "5232":
                    AddAOutputs(phSlot, ref outputCount, name);
                    break;

                default:
                    break;
            }
        }

        public void AddAOutputs(IntPtr phSlot, ref int outputCount, string name)
        {
            int bitSize = 0;
            int byteSize = 0;

            int status = _fnIO.FNIO_IoGetParam(phSlot, CrevisFnIO.IO_OUTPUT_BIT_SIZE, ref bitSize);
            if (status != CrevisFnIO.FNIO_ERROR_SUCCESS)
                throw new FnIOException(status, EFnIOError.FnIOGetParamFail);

            if (bitSize % 8 == 0)
                byteSize = bitSize / 8;
            else
                byteSize = bitSize / 8 + 1;

            AOutputs.Add(new IOSlot
            {
                DataBytes = new byte[byteSize],
                Index = outputCount,
                IOType = EIOType.AnalogOutput,
                //Size = byteSize % 2 == 0 ? byteSize / 2 : byteSize / 2 + 1,
                Size = byteSize,
                Name = name.Substring(0, 7)
            });

            outputCount += byteSize;
        }

        public void WriteOutputBit(int slotIndex, int bitIndex, bool bitValue)
        {
            int status = _fnIO.FNIO_DevWriteOutputImageBit(_hDevice, slotIndex, bitIndex, bitValue ? 1 : 0);
            if (status != 0)
                throw new FnIOException(status, EFnIOError.FnIOWriteOutputBitFail);
        }

        public void WriteOutputByte(int slotIndex, byte byteData)
        {
            int status = _fnIO.FNIO_DevWriteOutputImage(_hDevice, slotIndex, ref byteData, 1);
            if (status != 0)
                throw new FnIOException(status, EFnIOError.FnIOWriteOutputByteFail);
        }

        public void ClearBit()
        {
            foreach (var slot in DOutputs)
            {
                WriteOutputByte(slot.Index, 0);

                if (slot.DataBits.Length > 8)
                {
                    for (int i = 0; i < slot.DataBits.Length - 8; i++)
                    {
                        WriteOutputBit(slot.Index + 1, i, false);
                    }
                }
            }
        }

        public void ReadOutputBit(int slotIndex, int bitIndex, ref bool bitValue)
        {
            int bitNum = 0;
            int status = _fnIO.FNIO_DevReadOutputImageBit(_hDevice, slotIndex, bitIndex, ref bitNum);
            if (status != 0)
                throw new FnIOException(status, EFnIOError.FnIOReadOutputBitFail);

            bitValue = bitNum == 0 ? false : true;
        }

        public void ReadInputBit(int slotIndex, int bitIndex, ref bool bitValue)
        {
            int bitVal = 0;
            int status = _fnIO.FNIO_DevReadInputImageBit(_hDevice, slotIndex, bitIndex, ref bitVal);
            if (status != 0)
                throw new FnIOException(status, EFnIOError.FnIOReadIntputBitFail);

            bitValue = bitVal == 0 ? false : true;
        }

        public void ReadInputByte(int slotIndex, ref byte pBuffer)
        {
            int status = _fnIO.FNIO_DevReadInputImage(_hDevice, slotIndex, ref pBuffer, 1);
            if (status != 0)
                throw new FnIOException(status, EFnIOError.FnIOReadInputByteFail);
        }

        public void WriteOutputByte(int slotIndex, ref byte pBuffer)
        {
            int status = _fnIO.FNIO_DevWriteOutputImage(_hDevice, slotIndex, ref pBuffer, 1);
            if (status != 0)
                throw new FnIOException(status, EFnIOError.FnIOWriteOutputByteFail);
        }

        private void UpdateThreadStart()
        {
            _fnIOSignalCheckThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    int errCount = 0;
                    while (IsRun)
                    {
                        try
                        {
                            DInputUpdate();
                            DOutputUpdate();
                            AInputUpdate();
                            AOutputUpdate();

                            errCount = 0;
                            Thread.Sleep(30);
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logWrite.Error(ex, false, true); // 로그파일에만 남김.
                            if (errCount > 100) { throw new FnIOException(null, EFnIOError.FnIOSignalCheckThreadDie); }
                            errCount++;
                        }
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex) { _logWrite.Error(ex); }
                finally { IsRun = false; }
            }));
            _fnIOSignalCheckThread.Start();
        }

        public void Dispose()
        {
            try { ClearBit(); } catch { }
            Thread.Sleep(30);
            try { Close(); } catch { }
            //TODO : 왜 터지누?
            try
            {
                if (_hDevice != null) { Marshal.FreeHGlobal(_hDevice); }
                if (_hSystem != null) { Marshal.FreeHGlobal(_hSystem); }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }
    }
}
