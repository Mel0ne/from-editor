using System;
using System.Collections.Generic;
using System.IO;
using FRom.Logic;

namespace FRom
{

	/// <summary>
	/// ����� �������� ������ �������� ��������� RomEdit
	/// </summary>
	public class AddressStructREList : IEnumerable<AddressInstanceBase>
	{
		/// <summary>
		/// ������ ����
		/// </summary>
		List<AddressInstanceBase> _maps;

		/// <summary>
		/// ������� ���������� ��������� �����
		/// </summary>
		Dictionary<string, string> _params = new Dictionary<string, string>();

		/// <summary>
		/// ���
		/// </summary>
		private Logger.Log _log = Logger.Log.Instance;

		const int _defaultCountOfMaps = 32;
		public AddressStructREList(string fileName)
		{
			Init(fileName);
		}

		private void Init(string fileName)
		{
			try
			{
				using (StreamReader sr = new StreamReader(fileName))
				{
					_maps = new List<AddressInstanceBase>(_defaultCountOfMaps);
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine().Trim();

						//�����������
						if (line.Length == 0 || line[0] == '#') continue;

						//��������
						if (line.Contains("=") && !line.Contains(","))
						{
							AddParam(line);
						}
						//�������� ��� ����� TYPE_UNIT
						else if (line.Substring(0, 10).Contains("TYPE_UNIT,"))
						{
							string newLine = "TYPE_UNIT=" + line.Substring(11);
							AddParam(newLine);
						}
						//������ �����
						else if (line.Substring(0, 8).Contains("INCLUDE,"))
						{
							string incFile = line.Substring(8);
							string incPath = Directory.GetParent(fileName).ToString();
							Init(Path.Combine(incPath, incFile));
						}
						//�����
						else
						{
							try { _maps.Add(new AddressInstanceBase(line)); }
							catch (FromException ex)
							{
								string m = string.Format("������ ���������������: '{1}'{2}{0}", Environment.NewLine, line, ex.Message);
								_log.WriteEntry(this, m, Logger.EventEntryType.Error);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("������ ������ ��������� ����� '{0}'", fileName), ex);
			}
		}

		/// <summary>
		/// �������� ��������
		/// </summary>
		/// <param name="line"></param>
		private void AddParam(string line)
		{
			string[] param = line.Split(new char[] { '=' });
			if (param.Length != 2)
				throw new Exception(String.Format("������ ����������� ���������� �� ������: {0}{1}��������� '<key>,<value>'", line, Environment.NewLine));

			//���� �������� ����������, ������� ��� �����
			if (_params.ContainsKey(param[0]))
				_params.Remove(param[0]);

			_params.Add(param[0], param[1]);
		}

		internal Dictionary<string, string> GetParams()
		{
			return _params;
		}

		#region IEnumerable<AdressStructRE> Members
		public IEnumerator<AddressInstanceBase> GetEnumerator()
		{
			return _maps.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
