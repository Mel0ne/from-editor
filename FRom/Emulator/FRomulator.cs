using System;
using System.IO.Ports;
using FRom.Emulator;

namespace FRom.Emulator
{
	/// <summary>
	/// ����� ����������� ���������������� ���������� IFromulator
	/// </summary>
	class FRomulator : IFRomulator
	{

		//��������� ������������ � ���������
		const byte _cVersion = (byte)'V';
		const byte _cReadBlock = (byte)'R';
		const byte _cReadByte = (byte)'r';
		const byte _cWriteBlock = (byte)'X';
		const byte _cHiddenWrite = (byte)'w';
		const byte _cHiddenWriteOk = (byte)'O';
		const byte _cStatus = (byte)'S';
		const byte _cStatusReady = (byte)'O';
		const byte _cStatusBusy = (byte)'B';
		const byte _cUnknownCommand = (byte)'?';

		/// <summary>
		/// ����� ��� ������/�������� ���������� � RS-232
		/// </summary>
		private SerialPort _Port;

		/// <summary>
		/// ���������� �������� ��� ������ ������ (�� ���������� ����������� ������)
		/// </summary>
		public int _RetryReadCount
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� �������� ��� ������ ������ (�� ���������� ����������� ������)
		/// </summary>
		public int _RetryWriteCount
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� �������� ��� ������ ������� ������ (�� ���������� ����������� ������)
		/// </summary>
		public int _RetryHiddenWriteCount
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� ������� ��������� ������� 'Ready' �� ����������
		/// </summary>
		public int _RetryGetStatusCount
		{
			get;
			set;
		}

		public FRomulator(SerialPort port)
		{
			_Port = port;
			Init();
		}
		public FRomulator(string port)
		{
			_Port = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
			Init();
		}
		~FRomulator()
		{
			_Port.Close();
			_Port.Dispose();
		}

		public static RomulatorVersion _SWver = new RomulatorVersion(new byte[] { 1, 0, 0 });	//������ ��������� ����������
		private RomulatorVersion _HWver;		//������ ��������� ��������

		/// <summary>
		/// ����� ������������� ������
		/// </summary>
		private void Init()
		{
			_RetryReadCount = 10;			//���������� �������� ��� ������� ������
			_RetryWriteCount = 10;			//���������� �������� ��� ������� ������
			_RetryHiddenWriteCount = 10;	//���������� �������� ��� ������� ������� ������
			_RetryGetStatusCount = 1000;	//���������� �������� �������� � �������� ������� 'Ready'

			_Port.ReadTimeout = 5000;		//������� ������ ������ �� �����
			_Port.Open();
			if (GetVersion() >= _SWver)		//������ ��������� ��������� ������ ��� ����� ������ ��
				_HWver = GetVersion();
			else
				throw new Exception("������ ��������� ��������� ���� ��� ������ ��������� ��, ���������� ����������.");
		}

		/// <summary>
		/// ������� ������ � ����
		/// </summary>
		/// <param name="arr">������ ��� ��������</param>
		private void Write(byte[] arr)
		{
			_Port.Write(arr, 0, arr.Length);
		}

		/// <summary>
		/// ���������� ����������� ���� � ����� ������, ��� ����� �������� �������
		/// </summary>
		/// <param name="arr">������ ���� ��� �������� ������������ �����</param>
		/// <returns>���� ����������� �����</returns>		
		private byte[] CalcCheckSumByte(byte[] arr)
		{
			byte checksum = 0;
			unchecked
			{
				for (int i = 0; i < arr.Length; i++)
					checksum += arr[i];
			}
			return new byte[] { checksum };
		}

		/// <summary>
		/// ���������� ��� ����������� ����� � ����� ������, ��� ����� �������� �������
		/// </summary>
		/// <param name="arr">������ ������ ��� �������� ������������ �����</param>
		/// <returns>��� ����� ����������� �����</returns>		
		private short CalcCheckSumShort(byte[] arr)
		{
			short checksum = 0;
			unchecked
			{
				for (int i = 0; i < arr.Length; i++)
					checksum += arr[i];
			}
			return checksum;
		}

		/*
		  Name  : CRC-16 CCITT
		  Poly  : 0x1021    x^16 + x^12 + x^5 + 1
		  Init  : 0xFFFF
		  Revert: false
		  XorOut: 0x0000
		  Check : 0x29B1 ("123456789")
		  MaxLen: 4095 ���� (32767 ���) - �����������
		    ���������, �������, ������� � ���� �������� ������
		*/
		ushort[] Crc16Table = {
		    0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
		    0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
		    0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
		    0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
		    0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
		    0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
		    0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
		    0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
		    0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
		    0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
		    0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
		    0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
		    0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
		    0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
		    0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
		    0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
		    0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
		    0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
		    0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
		    0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
		    0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
		    0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
		    0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
		    0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
		    0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
		    0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
		    0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
		    0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
		    0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
		    0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
		    0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
		    0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
		};

		ushort Crc16(byte[] arr)
		{
			ushort crc = 0xFFFF;
			int len = arr.Length;
			unchecked
			{
				//while (len-- != 0)
				//	crc = (crc << 8) ^ Crc16Table[(crc >> 8) ^ *pcBlock++];

				for (int i = len; i > 0; i--)
				{
					crc = Convert.ToUInt16((crc << 8) ^ Crc16Table[(crc >> 8) ^ arr[i]]);
				}
			}
			return crc;
		}


		/// <summary>
		/// ��������� ���� ������ �� �����
		/// </summary>
		/// <param name="n">����������� ���� ��� ����������</param>
		/// <returns>������ ����</returns>
		private byte[] Read(int n)
		{
			byte[] arr = new byte[n];
			int res = _Port.Read(arr, 0, n);
			return arr;
		}

		/// <summary>
		/// �������� ��� �������. True - ���� �����.
		/// </summary>
		/// <param name="arr1">������ ������</param>
		/// <param name="arr2">������ ������</param>
		/// <returns>������ - ������� �����, ��� - ������� ��������</returns>
		private bool Compare(byte[] arr1, byte[] arr2)
		{
			if (arr1.Length != arr2.Length)
				return false;
			for (int i = 0; i < arr1.Length; i++)
				if (arr1[i] != arr2[i]) return false;
			return true;
		}

		#region IFRomulator Members

		/// <summary>
		/// �������� ������ �� ���������
		/// </summary>
		/// <returns>������ � ������� #(XXh).(XXh),ID(XXh)</returns>
		public RomulatorVersion GetVersion()
		{
			RomulatorVersion v = null;

			//��� ������� Ready
			if (!this.GetStatus(true))
				throw new TimeoutException("����� �������� ������� 'Ready' �������");

			//������������� ������� ������� � ������� � ����
			byte[] arr = new byte[] { _cVersion, _cVersion };
			//*** ������ ***
			this.Write(arr);

			//������� ������������� ������ ������� ������������ �� �����, �������� 3 �����
			v = new RomulatorVersion(Read(3));
			return v;
		}

		/// <summary>
		/// ��������� ������ ����
		/// </summary>
		/// <param name="size">������ �������</param>
		/// <param name="address">����� ������� �����</param>
		/// <returns>����������� ������ ����</returns>
		public byte[] ReadBlock(byte size, ushort address)
		{
			//�������� ushort �� 2 �����
			byte[] addr = BitConverter.GetBytes(address);
			//������������ ��������� �������
			byte[] arr_send = new byte[4] { _cReadBlock, size, addr[0], addr[1] };
			//������� ����������� �����
			byte[] checksum_send = CalcCheckSumByte(arr_send);
			//���������� ���������� ��� �������� �������
			byte[] arr_receive = null;
			//����������� ����
			byte[] checksum_receive = null;

			//������� ���������� �������� �������
			for (int i = 0; i < _RetryReadCount; i++)
			{
				//��� ������� Ready
				if (!this.GetStatus(true))
					throw new TimeoutException("����� �������� ������� 'Ready' �������");

				//*** ������ ***
				//���������
				this.Write(arr_send);
				//����������� �����
				this.Write(checksum_send);

				//*** ����� ***
				//������ ����������� ����
				arr_receive = this.Read(size);
				//����������� �����
				checksum_receive = this.Read(1);
				if (this.Compare(checksum_send, checksum_receive))
					return arr_receive;
			}
			throw new RomulatorException("����������� ����� �� ���������. ���������: " + checksum_send + "  �������: " + checksum_receive, checksum_receive);
		}

		/// <summary>
		/// ��������� ����
		/// </summary>
		/// <param name="address">����� �����</param>
		/// <returns>����������� ����</returns>
		public byte ReadByte(ushort address)
		{
			//�������� ushort �� 2 ����� ��� ��������
			byte[] addr = BitConverter.GetBytes(address);
			//������������ ������� �������
			byte[] arr_send = new byte[3] { _cReadByte, addr[0], addr[1] };

			//��� ������� Ready
			if (!this.GetStatus(true))
				throw new TimeoutException("����� �������� ������� 'Ready' �������");


			//*** ������ ***
			//������� ������� � ����
			this.Write(arr_send);

			//*** ����� ***								
			//����� ���� �� ����� � ��������� ���
			return this.Read(1)[0];
		}

		/// <summary>
		/// �������� ������ ����
		/// </summary>
		/// <param name="address">����� ������� �����</param>
		/// <param name="data">������ ��� ������</param>
		public void WriteBlock(ushort address, byte[] data)
		{
			//�������� ����� �������
			if (data.Length > 256)
				throw new ArgumentException("����� ������� �� ����� ���� ������ 256 ����");

			//�������� ushort �� 2 �����
			byte[] addr = BitConverter.GetBytes(address);
			//���������� ����������� ����� ������� ������
			byte[] checksumSend = BitConverter.GetBytes(CalcCheckSumShort(data));
			//������������ ������� �������
			byte[] arr_request = new byte[4] { _cWriteBlock, (byte)data.Length, addr[0], addr[1] };

			//�������� �������� ������
			for (int i = 0; i < _RetryWriteCount; i++)
			{
				//��� ������� Ready
				if (!this.GetStatus(true))
					throw new TimeoutException("����� �������� ������� 'Ready' �������");

				//*** ������ ***//
				//���������
				this.Write(arr_request);
				//������� ������
				this.Write(data);
				//����������� �����
				this.Write(checksumSend);

				//*** ����� ***//
				//����� ����������� ����� � ���� ������
				byte[] checksumReceive = this.Read(2);
				//��������� �������, ���� ����� - �������. ����� - ������
				if (this.Compare(checksumSend, checksumReceive))
					return;
			}
			throw new Exception("������ ������");
		}

		/// <summary>
		/// "����������" ��� ��������� ���������� ������ ������
		/// </summary>
		/// <param name="address">����� ������� �����</param>
		/// <param name="data">������ ����</param>
		public void HiddenWrite(ushort address, byte[] data)
		{
			//�������� ����� �������
			if (data.Length > 256)
				throw new ArgumentException("����� ������� �� ����� ���� ������ 256 ����");

			//�������� ushort �� 2 �����
			byte[] addr = BitConverter.GetBytes(address);
			//���������� ����������� ����� ������� ������
			byte[] checksumSend = CalcCheckSumByte(data);
			//������������ ������� �������
			byte[] arr_request = new byte[4] { _cHiddenWrite, (byte)data.Length, addr[0], addr[1] };

			//��� ������� Ready
			if (!this.GetStatus(true))
				throw new TimeoutException("����� �������� ������� 'Ready' �������");

			//�������� �������� ������
			for (int i = 0; i < _RetryHiddenWriteCount; i++)
			{

				//*** ������ ***//
				//���������
				this.Write(arr_request);
				//������� ������
				this.Write(data);
				//����������� �����
				this.Write(checksumSend);

				//*** ����� ***//
				//������ ����������� ����� 1 ����
				byte[] checksumReceive = this.Read(1);
				//��������� ����������� �����, ���� ����� - �������.
				if (this.Compare(checksumSend, checksumReceive))
					return;
			}
			throw new Exception("������ ������");
		}

		/// <summary>
		/// �������� ��������� ����������
		/// </summary>
		/// <param name="waitReady">������� �������� ������� 'Ready'</param>
		/// <returns></returns>
		public bool GetStatus(bool waitReady)
		{
			//�������
			int i = _RetryGetStatusCount;
			do
			{
				//*** ������ ***
				_Port.Write(new byte[] { _cStatus }, 0, 1);

				//*** ����� ***
				byte status = (byte)_Port.ReadByte();

				if (status == _cStatusReady) return true;
				else if (status == _cStatusBusy) continue;
				//else throw new ProtocolException("������ ��������� ��������� ����������", new byte[] { status });
			} while (waitReady && i-- > 0);
			return false;
		}

		public bool GetStatus()
		{
			return GetStatus(false);
		}

		#endregion


	}


}
