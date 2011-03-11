﻿using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace FRom.Logger
{
	internal class FileLogger : IDisposable
	{
		// Fields
		private string _currentFileName;
		private string _logFileNameFormat;
		private Encoding _encoding;
		private string _logDirectory;
		private string _postfix;
		private bool _disposed = false;
		private DateTime _lastClearTime;
		private Dictionary<string, TimeStreamWriter> _openFiles;
#if DEBUG
		internal string _dateTimeFormatLogRecord = "yyyy.MM.dd HH:mm:ss.fff";
#else
		internal string _dateTimeFormatLogRecord = "yyyy.MM.dd HH:mm:ss";
#endif

		// Methods
		public FileLogger()
		{
			_currentFileName = string.Empty;
			_logFileNameFormat = "FromEditor.log";
			_encoding = Encoding.UTF8;
			_logDirectory = string.Empty;
			_postfix = string.Empty;
			_currentFileName = string.Empty;
			_openFiles = new Dictionary<string, TimeStreamWriter>(1);
			_lastClearTime = DateTime.Now;
			Init();
		}

		private void Init()
		{
			//string str = Path.Combine(Path.GetDirectoryName(base.GetType().Assembly.Location), "logs");
			//LogDirectory = str;
			LogDirectory = Path.GetDirectoryName(base.GetType().Assembly.Location);
		}

		public void Append(LogEvent message)
		{
			DateTime now = DateTime.Now;
			TimeSpan span = (TimeSpan)(now - _lastClearTime);
			if (span.TotalSeconds > 60.0)
			{
				this._lastClearTime = now;
				this.CloseUnusedFiles(this._lastClearTime);
			}
			string key = this.LogFileNamePath;
			try
			{
				if (!this._openFiles.ContainsKey(key))
				{
					this._openFiles[key] = new TimeStreamWriter(key, this._encoding);
				}
				this._openFiles[key].StreamWriter.WriteLine(this.LogNote(message));
			}
			catch (NotImplementedException) { throw; }
			catch { }

		}

		/// <summary>
		/// Закрыть файлы которые не используется больше заданного времени 
		/// </summary>
		/// <param name="maxActiveTime">maxActiveTime</param>
		private void CloseUnusedFiles(DateTime maxActiveTime)
		{
			DateTime now = DateTime.Now;
			List<TimeStreamWriter> list = new List<TimeStreamWriter>(1);
			foreach (KeyValuePair<string, TimeStreamWriter> pair in this._openFiles)
			{
				if (pair.Value.LastAccsessTime < maxActiveTime)
				{
					list.Add(pair.Value);
				}
			}
			foreach (TimeStreamWriter writer in list)
			{
				writer.Close();
				this._openFiles.Remove(writer.FileName);
			}
		}

		~FileLogger()
		{
			Dispose(false);
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		internal void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (TimeStreamWriter writer in this._openFiles.Values)
				{
					writer.Dispose();
				}
				this._openFiles = null;
			}
		}

		public void Flush()
		{
			try
			{
				foreach (KeyValuePair<string, TimeStreamWriter> pair in this._openFiles)
				{
					pair.Value.Flush();
				}
			}
			catch { }
		}

		internal string LogNote(LogEvent message)
		{
			Object o = message.Sender;
			return string.Format("[{0} : {1} : {2}] {3}",
				message.Time.ToString(_dateTimeFormatLogRecord),
				message.Type.ToString(),
				o.GetType().ToString(),
				message.Message);
		}

		/// <summary>
		/// Текущая папка логов
		/// </summary>
		public string LogDirectory
		{
			get { return _logDirectory; }
			set
			{
				if (!Directory.Exists(value))
				{
					Directory.CreateDirectory(value);
				}
				_logDirectory = value;
			}
		}

		/// <summary>
		/// Полный путь к текущему лог файлу 
		/// </summary>
		public string LogFileNamePath
		{
			get { return Path.Combine(LogDirectory, LogFileName); }
		}

		/// <summary>
		/// Имя текущего лог файла
		/// </summary>
		public string LogFileName
		{
			get { return string.Format(LogFileNameFormat, DateTime.Now); }
		}

		/// <summary>
		/// Формат имени лог файла. Параметры: {0} - дата
		/// </summary>
		public string LogFileNameFormat
		{
			get { return _logFileNameFormat; }
			set
			{
				try
				{
					string.Format(value, DateTime.Now);
					_logFileNameFormat = value;
				}
				catch { }
			}
		}

		public string LogDateTimeRecordFormat
		{
			get { return _dateTimeFormatLogRecord; }
			set
			{
				try
				{
					string.Format(value, DateTime.Now);
					_dateTimeFormatLogRecord = value;
				}
				catch { }
			}
		}

		public string Postfix
		{
			get { return _postfix; }
			set
			{ _postfix = (value == null) ? string.Empty : value; }
		}
	}
}

