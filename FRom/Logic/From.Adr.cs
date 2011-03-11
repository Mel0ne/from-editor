using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using FRom.Properties;

namespace FRom.Logic
{
	/// <summary>
	/// ���������� �������� ����� ������ FRom
	/// </summary>	
	partial class From
	{

		/// <summary>
		/// ������� �������� �������� ����������� �����
		/// </summary>
		Dictionary<string, AddressInstance> _stringToAdressMap;

		/// <summary>
		/// ��������� �������� �����
		/// </summary>
		Dictionary<string, string> _params;

		/// <summary>
		/// ���� � �����, �� �������� ��������� �������� ������
		/// </summary>
		string _filenameAddr;

		/// <summary>
		/// ������������� �������� ������ �� �������� adr ����
		/// </summary>
		/// <param name="fileName">������ ���� � ����� ��� ������ ��������</param>
		private void LoadAddrBase(string fileName)
		{
			//��������� ��������� �������� � ���������������� ���������� ������
			AddressStructREList adrRE = new AddressStructREList(fileName);

			//��������� ��������� ����������, 
			//����� � ������ ������� ������������� �� ��������� �������
			Dictionary<string, AddressInstance> tmpDictionary = new Dictionary<string, AddressInstance>();

			//����������� ���������� ����������� � ������ � ���������� �����
			foreach (AddressInstanceBase item in adrRE)
			{
				AddressInstance tmp = new AddressInstance(item);
				//���� ���� ��� ����������, �� �������������� � ���� ������
				while (tmpDictionary.ContainsKey(tmp.ConstName))
				{
					string key = tmp.ConstName;
					for (int i = key.Length - 1; i > 0; i--)
						if (Char.IsDigit(key[i]))
							continue;
						else
						{
							string strKey = key.Substring(0, i + 1);
							string valKey = key.Substring(i + 1);
							int keyNo = valKey == "" ? 1 : Int32.Parse(valKey) + 1;
							key = strKey + keyNo;
							tmp.ConstName = key;
							break;
						}
				}
				tmpDictionary.Add(tmp.ConstName, tmp);
			}
			_params = adrRE.GetParams();
			//���� ����� ����, ������ ���������� ��������. ��������.
			_stringToAdressMap = tmpDictionary;
		}

		/// <summary>
		/// ���������� �������� ����� �����
		/// </summary>
		/// <param name="key">��������� - �����</param>
		/// <returns>�������� �����</returns>
		public AddressInstance GetAddressInstance(string key)
		{
			//���� ����������� ���� ���������� � �������, �� ��������
			if (_stringToAdressMap.ContainsKey(key))
				return _stringToAdressMap[key];
			else
				throw new ArgumentOutOfRangeException("cnst", key, "Map const - '" + key + "' Not Found!");
		}

		/// <summary>
		/// ����� ������ ����
		/// </summary>
		/// <returns>������ ����</returns>
		public Map[] GetAllMaps()
		{
			Map[] arr = new Map[_stringToAdressMap.Count];
			int i = 0;
			foreach (var addr in _stringToAdressMap.Values)
				arr.SetValue(new Map(this, addr), i++);
			return arr;
		}

		#region Properties
		/// <summary>
		/// ��� ������������� ������� ���� � ����� .adr
		/// </summary>
		public string DataSourceAddress
		{
			get { return _filenameAddr; }
		}

		/// <summary>
		/// ������� ���������� ��������� ������ ������
		/// </summary>
		public event FromEventHandler DataSourceChanged;

		public delegate void FromEventHandler(object sender, FromEventArgs e);

		/// <summary>
		/// ������� ������������� ��������� ������
		/// </summary>
		protected bool InitializedAddr
		{
			get { return _filenameAddr != null && _filenameAddr.Length > 0; }
		}
		#endregion

		#region Serializers, Deserializers
		/// <summary>
		/// �������� �������� ������ �� �����.
		/// ����� ������������ � ����������� �� ���������� ����������� �����
		/// </summary>
		/// <param name="fileName">������ ���� � �����</param>
		public void OpenAddressFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				switch (fileName.Substring(fileName.LastIndexOf('.') + 1).ToUpper())
				{
					case "FROM":
						using (Stream inStream = new FileStream(fileName, FileMode.Open))
						{
							_stringToAdressMap = (Dictionary<string, AddressInstance>)GetBinarySerializer().Deserialize(inStream);
						}
						break;
					case "XML":
						using (Stream inStream = new FileStream(fileName, FileMode.Open))
						{
							List<AddressInstance> tmp = (List<AddressInstance>)GetXMLSerializer().Deserialize(inStream);
							_stringToAdressMap = new Dictionary<string, AddressInstance>();
							foreach (var item in tmp)
								_stringToAdressMap[item.ConstName] = item;
						}
						break;
					case "ADR": LoadAddrBase(fileName); break;
					default: throw new Exception(String.Format(Resources.strExMsgNotSupportedFileFormat, fileName));
				}
				_filenameAddr = fileName;
				//������� ������� ����� ��������� ������
				if (DataSourceChanged != null)
					DataSourceChanged(this, null);
			}
		}

		/// <summary>
		/// ���������� �������� ������ � ����. 
		/// ����� ������������ � ����������� �� ���������� ����������� �����
		/// </summary>
		/// <param name="fileName">������ ���� � �����</param>
		public void SaveAddr(string fileName)
		{
			if (!InitializedAddr) throw new ArgumentNullException("������ ���������");
			switch (fileName.Substring(fileName.LastIndexOf('.') + 1).ToUpper())
			{
				case "": SaveAddr(_filenameAddr); break;
				case "FROM":
					using (FileStream outStream = new FileStream(fileName, FileMode.OpenOrCreate))
					{
						GetBinarySerializer().Serialize(outStream, _stringToAdressMap);
					}
					break;
				case "XML":
					using (FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
					{
						List<AddressInstance> tmp = new List<AddressInstance>(_stringToAdressMap.Values);
						GetXMLSerializer().Serialize(outStream, tmp);
					}
					break;
				//case "ADR": LoadFromADR(fileName); break;
				default: throw new Exception(Resources.strExMsgNotSupportedFileFormat);
			}
		}

		XmlSerializer GetXMLSerializer()
		{
			return new XmlSerializer(typeof(List<AddressInstance>));
		}
		BinaryFormatter GetBinarySerializer()
		{
			return new BinaryFormatter();
		}
		#endregion
	}
}
