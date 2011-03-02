using System;
using System.Collections.Generic;
using System.IO;
using FRom.Logger;

namespace FRom.Logic
{
	/// <summary>
	/// ���������� �������� ����� ������ FRom
	/// </summary>
	public partial class From
	{
		byte[] _bin;					//������ ������ (��������)
		string _filenameROM;			//���� �� �������� ��������� �������� ������
		Log _logger = Log.Instance;

		/// <summary>
		/// �������������� ������� ROM ������
		/// </summary>
		static List<int> _supportedROMSizes = new List<int>()
		{
			0x4000,
			0x8000,
		};

		/// <summary>
		/// ����������� � ��������������
		/// </summary>
		/// <param name="binFileName">������ ���� � ��������� �����</param>
		/// <param name="addressFileName">������ ���� � ��������� �����</param>
		public From(string binFileName, string addressFileName)
		{
			InitMembers();
			OpenROMFile(binFileName);
			OpenAddressFile(addressFileName);
		}

		/// <summary>
		/// ����������� �� ���������
		/// </summary>
		public From()
		{
			InitMembers();
		}

		/// <summary>
		/// ������������� ��������
		/// </summary>
		private void InitMembers()
		{
			_logger.WriteEntry(this, "Launch FRom Editor Main Module...", Logger.EventEntryType.Event);
			//��������� ����� �������
			_stringToAdressMap = new Dictionary<string, AddressInstance>();
		}

		/// <summary>
		/// ����� �����
		/// </summary>
		/// <param name="cnst">���������, ������������ ���������� �����</param>
		/// <returns>���������� �����, ��������� ����������� ����������� ����� (�������� + ������)</returns>
		public Map GetMap(string cnst)
		{
			//��������� ����� ����� map � ����������������
			return new Map(this, _stringToAdressMap[cnst]);
		}

		/// <summary>
		/// ������������� ��������� ������
		/// </summary>
		/// <param name="fileName">������ ���� � .bin �����</param>
		public void OpenROMFile(string fileName)
		{
			//�������� ������������� ����
			if (!File.Exists(fileName)) throw new ArgumentException("File '" + _filenameROM + "' not found!", fileName);

			//������� ������ �����
			try
			{
				_bin = File.ReadAllBytes(fileName);
				_filenameROM = fileName;
				if (DataSourceChanged != null)
					DataSourceChanged(this, null);
			}
			catch (Exception e) { throw new FromException("������ ������ �� ����: [" + fileName + "]", e); }
		}

		/// <summary>
		/// ������������� ROM ������ �� �������
		/// </summary>
		/// <param name="data">������ ������</param>
		public void OpenROMFile(byte[] data)
		{
			//���� ������ ��������������
			if (_supportedROMSizes.Contains(data.Length))
			{
				_bin = data;
				_filenameROM = null;
				if (DataSourceChanged != null)
					DataSourceChanged(this, null);
			}
			else
				throw new FromException(
					String.Format("������ ROM ����� �� ��������������. ({0})", data.Length)
				);
		}

		/// <summary>
		/// ����� ������ ��������
		/// </summary>
		/// <returns>������ ����</returns>
		internal byte[] GetBin()
		{
			return _bin;
		}

		/// <summary>
		/// ��������� ���������� ��������� ������ � ���� .bin
		/// </summary>
		/// <param name="fileName">������ ���� � �����</param>
		public void SaveBin(string fileName)
		{
			//�������� ��������������� �� �����
			if (!InitializedBin) throw new Exception("����� [" + this.ToString() + "] �� ������������������");

			//���� ���������� �� �������� - ��������� � ������� ���� _filenameBin
			if (fileName == "") fileName = _filenameROM;

			//�������� ��������� ����� ����, ���� �� ��� ����������
			CreateBackFile(fileName);

			//������� ������ ������
			try
			{ File.WriteAllBytes(fileName, _bin); }
			catch (Exception e)
			{ throw new FromException("������ ���������� � ����: [" + fileName + "]", e); }
		}

		/// <summary>
		/// ���������� ROM � ������� ����
		/// </summary>
		public void SaveBin()
		{
			SaveBin("");
		}

		/// <summary>
		/// �������� ��������� ����� �����
		/// </summary>
		/// <param name="_filenameBin">���� � �����, ��������� ����� �������� ���������� �������</param>
		private void CreateBackFile(string fileName)
		{
			string backFilename = fileName;
			//���� ���� ����������
			if (File.Exists(fileName))
			{
				int i = 0;
				//��� ��� �����, ������� �� ������ � ������� �����
				do
				{
					backFilename = backFilename.Substring(0, backFilename.LastIndexOf('.')) + ".bak#" + i++;
				} while (File.Exists(backFilename));
				//������ ��������� �����
				File.Copy(fileName, backFilename);
			}
		}

		/// <summary>
		/// �������� ������ ��� ��������� ������
		/// </summary>
		public string DataSourceROM
		{
			get { return _filenameROM; }
		}

		/// <summary>
		/// ������� ������������� ������ ������
		/// </summary>
		protected bool InitializedBin
		{
			get { return _filenameROM != null && _filenameROM.Length > 0; }
		}

		/// <summary>
		/// True, ���� ���� ������ � ����� ����� ����������������
		/// </summary>
		public bool Initialized
		{
			get { return InitializedAddr && InitializedBin; }
		}

		/// <summary>
		/// ������������ ������ �� ������.
		/// </summary>
		public void Reload()
		{
			if (Initialized)
			{
				string fileAddr = _filenameAddr;
				string fileBin = _filenameROM;
				Clear();
				OpenAddressFile(fileAddr);
				OpenROMFile(fileBin);
			}
			DataSourceChanged(this, null);
		}

		/// <summary>
		/// �������� �����
		/// </summary>
		internal void Clear()
		{
			_filenameROM = _filenameAddr = "";
			_stringToAdressMap.Clear();
			_bin = null;
		}
	}
}

