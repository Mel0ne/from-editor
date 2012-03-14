using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using FRom.Consult;
using FRom.Consult.Data;
using FRom.Emulator;
using FRom.Grid;
using FRom.Logic;
using FRom.Properties;
using Helper;
using Helper.Logger;
using Helper.ProgressBar;
using Helper.Properties;
using InfoLibrary;

namespace FRom
{
	public partial class FormMain : Form, IDisposable
	{
#if DEBUG
		internal static bool debugFlag = true;
#else
		internal static bool debugFlag = false;
#endif

		internal From _bin;

		internal Romulator _emulator;

		internal ConsultProvider _consult;

		internal Log _log;

		internal Library _lib;

		/// <summary>
		/// ������ ����������� ��� �����������
		/// </summary>
		internal ListIndexString<IConsultData> _consltDataList;

		/// <summary>
		/// ����� ��������
		/// </summary>
		internal FormSettings _frmOptions;

		/// <summary>
		/// ����� ������ � ����������� consult
		/// </summary>
		internal FormLiveScan _frmConsult;

		/// <summary>
		/// ����� �������� ����������
		/// </summary>
		internal Settings _cfg;

		FormSpeedTrial _frmSpeedTrial;

		/// <summary>
		/// ����������� ������� �������� ����.
		/// </summary>
		EventHandler _EHmainMenu;

		bool _pGridEnable;

		/// <summary>
		/// ���� ����������� ��������
		/// </summary>
		FormGraph3D _graphForm;

		public FormMain()
		{

			InitializeComponent();				//������������� ����������� �����
			InitializeSettings();				//������������� �������� ���������
			InitializeLibrary();
			InitializeEmulatorMenu();			//������������� ��������� ROM
			InitializeConsultMenu();			//������������� consult
			InitializeMenu(mstrpMain);			//������������� ������������ ��������� ����
			InitializeTabControl();				//������������� ������� � �������

			this.Text = HelperClass.GetProductInfo(this);

			Components.Add(this, 1000);
			Components.Add(_frmSpeedTrial);
		}

		private void InitializeLibrary()
		{
			try
			{
				Library lib = new Library(HelperClass.FindFolder("library.xls"));
			}
			catch
			{
				string a = "q";
			}
		}

		ResourceDisposer Components
		{
			get
			{
				if (components == null)
					components = new ResourceDisposer();
				return (ResourceDisposer)components;
			}
			set
			{
				components = value;
			}
		}

		private void Dispose_Resources(bool disposing)
		{
			if (disposing)
			{
				if (frmLog != null)
					frmLog.Close();
				if (_frmConsult != null)
					_frmConsult.Close();
				if (_consult != null)
					_consult.Disconnect();
				if (_emulator != null)
					_emulator.Dispose();
				if (_bin != null)
					_bin.Clear();
				if (_cfg != null)
					_cfg.Save();
				if (_log != null)
					_log.Close();
			}
		}

		~FormMain()
		{
			Dispose_Resources(false);
			Dispose(false);
		}

		void IDisposable.Dispose()
		{
			//Dispose(true);
			Dispose_Resources(true);
			GC.SuppressFinalize(this);
		}

		private void InitializeConsultMenu()
		{
			bool flag = _consult.IsOnline;

			mnuConsultConnect.Checked =

			mnuConsultActiveTests.Enabled =
			mnuConsultSelfDiagnostic.Enabled =
			mnuConsultSensorsLive.Enabled =
			mnuConsultMode.Enabled =
			txtConsultECUInfo.Visible =
			mnuConsultECUInfoSeparator.Visible =
			mnuConsultSpeedTrial.Enabled =
				flag;

			if (flag)
			{
				txtConsultECUInfo.Text = _consult.GetECUInfo().ToString();
				mnuConsult.Image = Resources.pngAccept;
			}
			else
			{
				mnuConsult.Image = Resources.pngStop;
			}
		}

		private void InitializeSettings()
		{
			//����
			_log = Log.Instance;
			_log.CatchExceptions = true;
			_log.LogLevel = debugFlag ? EventEntryType.Debug : EventEntryType.Event;
			_log.LogFileEnabled = true;

			//������� ��������� ������� Click �� ToolStripMenuItems
			_EHmainMenu = new EventHandler(menu_Click);

			//������������� ������ �������� ����������
			_cfg = new Settings();
			//���� ��������� ����������, �� �������� �������� ������.
			//������� ��������� �� ���������� ������ � ������ ����
			if (_cfg.NeedUpgrade)
			{
				_cfg.Upgrade();
				_cfg.NeedUpgrade = false;
			}
			//�������� �� ���������� ��������� ���� ��������
			if (_cfg.cfgdlgADRPath == null
				|| _cfg.cfgdlgADRPath.Length == 0
				|| !new DirectoryInfo(_cfg.cfgdlgADRPath).Exists
			)
			{
				_cfg.cfgdlgADRPath = Environment.CurrentDirectory;
			}
			if (_cfg.cfgdlgROMPath == null
				|| _cfg.cfgdlgROMPath.Length == 0
				|| !new DirectoryInfo(_cfg.cfgdlgROMPath).Exists
			)
			{
				_cfg.cfgdlgROMPath = Environment.CurrentDirectory;
			}

			//������ ��������� ����������� ����������� (���������)
			_consltDataList = new ListIndexString<IConsultData>()
			{
				new ConsultData(new DataEngine()),
				new ConsultData(new DataAT()),
				new ConsultData(new DataHICAS()),
				new ConsultData(new DataAirCon()),
			};
			mnuConsultMode.DropDownItems.Clear();
			//��������� ������ ������� ����������� � ����
			foreach (IConsultData i in _consltDataList)
			{
				string name = i.ToString();
				ToolStripMenuItem mnu = new ToolStripMenuItem();
				//mnu.Click -= new EventHandler(menu_Click);
				//mnu.Click += new EventHandler(mnuConsultMode_Click);
				mnu.Name = mnu.Text = name;
				mnuConsultMode.DropDownItems.Add(mnu);
			}
			//�������� ������ ����� ���� �� ���������
			mnuConsultMode_Click(
				mnuConsultMode.DropDownItems[_consltDataList[0].ToString()],
				new EventArgs());
			//����� ������ ����� ��������� consult
			_consult = new ConsultProvider(_consltDataList[0]);
			//��������� ����������� �������� ���� ������� ��� ������
			if (_cfg.cfgTyreOrigin != null && _cfg.cfgTyreCurrent != null)
				ConversionFunctions.SpeedCorrectCoefficient = TyreParams.CalcK(_cfg.cfgTyreOrigin, _cfg.cfgTyreCurrent);
			//���� ����� ��������� �� ��������������� - ����������� � ���������
			if (_cfg.cfgConsultConnectAtStartup)
				menu_Click(mnuConsultConnect);

			//������� ����� ������ � ROM/ADR �������
			_bin = new From();
			//����������� ������� ���������� ���������� �� ������� ����� ��������� ������ ������
			_bin.DataSourceChanged += new From.FromEventHandler(InitFRomInterface);
			//InitInterface(_bin, null);

			//������� ���������� ����� ������������ ���� ����������
			if (_cfg.cfgOpenLastConfig)
			{
				if (_cfg.cfgRecentAdrFiles.Count > 0 && File.Exists(_cfg.cfgRecentAdrFiles[0]))
				{
					try { _bin.OpenAddressFile(_cfg.cfgRecentAdrFiles[0]); }
					catch { }
				}
				if (_cfg.cfgRecentBinFiles.Count > 0 && File.Exists(_cfg.cfgRecentBinFiles[0]))
				{
					try { _bin.OpenROMFile(_cfg.cfgRecentBinFiles[0]); }
					catch { _bin.Clear(); }
				}
			}
		}

		/// <summary>
		/// ������������� ��������� ����
		/// </summary>
		/// <param name="mstrpMain">���� ��� �������������</param>
		/// <param name="eventHandler">���������� ������� ��������� ����'Click'</param>
		private void InitializeMenu(MenuStrip menu)
		{
			btnRecentFiles_DropDownOpening(mnuRecentFilesROM, null);
			btnRecentFiles_DropDownOpening(mnuRecentFilesADR, null);

			foreach (object item in menu.Items)
				if (item is ToolStripMenuItem)
					HandleClickEvent(item as ToolStripMenuItem, _EHmainMenu);
		}

		/// <summary>
		/// ����������� ��������� ������������ DropDown Items �� ������� Click
		/// </summary>
		/// <param name="menu">MenuItem ��� ����������� �� �������</param>
		/// <param name="eventHandler">���������� �������</param>
		private void HandleClickEvent(ToolStripMenuItem menu, EventHandler eventHandler)
		{
			if (menu.DropDown.Items != null)
				foreach (ToolStripItem item in menu.DropDown.Items)
				{
					if (item is ToolStripMenuItem)
					{
						HandleClickEvent(item as ToolStripMenuItem, eventHandler);
						item.Click += eventHandler;
					}
				}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{

#if DEBUG
			try
			{
				string romFileName = HelperClass.FindFolder("data") + "R32_rb26det.bin";
				_bin.OpenROMFile(romFileName);
				string addressFileName = HelperClass.FindFolder("data") + "HCR32_RB26_256_E_Z32_444cc.adr";
				_bin.OpenAddressFile(addressFileName);
				_log.LogLevel = EventEntryType.Debug;
			}
			catch (Exception ex)
			{
				MessageBox.Show(HelperClass.GetExceptionInfo(ex));
			}
#else
			_log.LogLevel = EventEntryType.Event;
#endif
		}

		/// <summary>
		/// ������������� ���������� ������������
		/// </summary>
		public void InitFRomInterface(object sender, FromEventArgs e)
		{
			if (sender is From)
			{
				From bin = sender as From;
				bool init = bin.Initialized;
				if (init)
				{
					InitComboBox();
					InitializeTabControl();
				}
				lblAddressFile.Text = _bin.DataSourceAddress == "" ? "Click to Load ADR" : bin.DataSourceAddress;
				lblBinFile.Text = _bin.DataSourceROM == "" ? "Cliack to Load BIN" : bin.DataSourceROM;

				cbMaps.Enabled =
				btnSaveBIN.Enabled =
				mnuSaveADRAs.Enabled =
				mnuSaveBIN.Enabled =
				mnuSaveBINAs.Enabled =
				mnuMapReload.Enabled = init;
			}
		}

		private void InitializeTabControl()
		{
			tabMain.TabPages.Clear();
		}

		private void InitializeEmulatorMenu()
		{
			bool emuEnable = _emulator != null;
			mnuEmulatorDownload.Enabled =
				mnuEmulatorUpload.Enabled =
				mnuEmulatorStreamMode.Enabled =
				btnEmulatorStreamMode.Enabled =
				emuEnable;
		}

		private void InitComboBox()
		{
			propertyGrid.SelectedObject = null;
			cbMaps.Items.Clear();
			cbMaps.SelectedIndex = -1;
			cbMaps.Text = " << SELECT MAP >> ";

			//���������� ComboBox �������
			Map[] maps = _bin.GetAllMaps();
			cbMaps.Items.AddRange(maps);
		}

		private void EmulatorUpload()
		{
			byte[] data = _bin.GetBin();
			if (data != null && (data.Length == 0x4000 || data.Length == 0x8000 || data.Length == 0x16000))
				try
				{
					_emulator.WriteBlockL(0, data);
				}
				catch (RomulatorException ex)
				{
					Error(ex, "������ ��� �������� ������ � ��������", "Emulator Upload Error");
				}
		}

		private void EmulatorDownload()
		{
			byte[] data = null;
			try
			{
				data = _emulator.ReadBlockL(0, 0x8000);
			}
			catch (RomulatorException ex)
			{
				Error(ex, "������ ��� ������ ������ �� ���������", "Emulator Download Error");
			}

			if (_cfg.cfgEmulatorSaveFileAfterRead)
			{
				string fileName = Path.Combine(Environment.CurrentDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_emulator.bin");

				try
				{
					//������� ������ ������
					File.WriteAllBytes(fileName, data);
					_bin.OpenROMFile(fileName);
				}
				catch (Exception ex)
				{
					Error(ex, "������ ��� ���������� ������ � ����: " + fileName, "Error save Bin to file.");
				}
			}
			else
			{
				try { _bin.OpenROMFile(data); }
				catch (FromException ex)
				{
					Error(ex, "������ ��� �������� ������ ��������� �� ���������", "Error open ROM data");
				}

			}
		}

		#region Error Handlers
		/// <summary>
		/// ����������� ������������ �� ������
		/// </summary>
		/// <param name="ex">����������</param>
		/// <param name="message">��������� �� ������</param>
		/// <param name="caption">��������� ���� ���������</param>
		public void Error(Exception ex, string message, string caption)
		{
			//���� �������, �� ������� ���
			if (debugFlag)
			{
				UIViewLog();
				message += HelperClass.GetExceptionInfo(ex);
			}
			else
				message += HelperClass.GetExceptionMessages(ex);

			_log.WriteEntry(this, message, EventEntryType.Error);

			DialogResult res = System.Windows.Forms.DialogResult.OK;
			HelperClass.Invoke(this, delegate()
			{
				res = MessageBox.Show(message + "\n\n��������� ����� �� ������?", caption, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
			});

			if (res == DialogResult.Yes)
			{
				List<string> att = new List<string>() { 
					_bin.DataSourceAddress,
					_bin.DataSourceROM
				};

				ShowBugReportWindow(message, att, false);
			}
		}

		private void ShowBugReportWindow(string message, List<string> att, bool enableAttach)
		{
			FormFeedBack frm = new FormFeedBack(this, message, att, enableAttach);
			frm.StartPosition = FormStartPosition.CenterParent;
			frm.ShowDialog(this);
		}


		#endregion

		#region Interface Handler
		/// <summary>
		/// ���������� ������� ������ �� �����
		/// </summary>
		private void tabMain_KeyPress(object sender, KeyPressEventArgs e)
		{
			//this.Text= e.KeyChar.ToString();
			if (127 == e.KeyChar)
				tabMain.SelectedTab.Dispose();
		}

		/// <summary>
		/// ���������� ������� ����� �� ����
		/// </summary>
		private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			TabPage tab = tabMain.SelectedTab;
			if (tab == null)
				txtComment.Text = "Select Map to add Tab...";
			else
			{
				string cnst = (sender as TabControl).SelectedTab.Text;
				AddressInstance adr = _bin.GetAddressInstance(cnst);
				string name = adr.MapName;
				propertyGrid.SelectedObject = adr;	//property grid
				txtComment.Text = GetComment(adr);
				cbMaps.SelectedIndex = (int)tab.Tag;
			}
		}

		private string GetComment(AddressInstance adr)
		{
			return adr.MapName + " [" + adr.ConstName + "]";
		}

		/// <summary>
		/// ���������� ������� DropDownOpening �������� ���� "Recent Files"
		/// </summary>
		private void btnRecentFiles_DropDownOpening(object sender, EventArgs e)
		{
			ToolStripDropDownItem menu = sender as ToolStripDropDownItem;
			if (menu == null)
				return;

			StringCollection files =
				(menu == mnuRecentFilesROM)
				? (_cfg.cfgRecentBinFiles)
				: (_cfg.cfgRecentAdrFiles);


			if (files.Count == 0)
			{
				menu.DropDownItems.Clear();
				menu.Enabled = false;
			}
			else
			{
				menu.DropDownItems.Clear();
				foreach (var i in files)
					menu.DropDownItems.Add(i);
			}
		}

		private void InitAddAdrRecentFiles(string fileName)
		{
			if (_cfg.cfgRecentAdrFiles == null) return;
			if (_cfg.cfgRecentAdrFiles.Contains(fileName))
				_cfg.cfgRecentAdrFiles.Remove(fileName);
			_cfg.cfgRecentAdrFiles.Insert(0, fileName);
		}
		private void InitAddBinRecentFiles(string fileName)
		{
			if (_cfg.cfgRecentBinFiles == null) return;
			if (_cfg.cfgRecentBinFiles.Contains(fileName))
				_cfg.cfgRecentBinFiles.Remove(fileName);
			_cfg.cfgRecentBinFiles.Insert(0, fileName);
		}

		/// <summary>
		/// ���������� ������� 'Click' �� ����
		/// </summary>
		void menu_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem menu = sender as ToolStripMenuItem;
			if (menu != null && menu.DropDownItems.Count > 0)
				return;

			ToolStripButton btn = sender as ToolStripButton;
			ToolStripLabel status = sender as ToolStripLabel;

			if (menu == null && btn == null && status == null)
				_log.WriteEntry(this, "Sender is NULL!\n" + Environment.StackTrace, EventEntryType.Warning);

			string selectedItem = menu == null
				? (btn == null)
					? (status == null)
						? ""
						: (status.Name)
					: (btn.Name)
				: (menu.Name);

			if (debugFlag)
				_log.WriteEntry(this,
					String.Format("Item clicked: {0} (Type:{1})",
					selectedItem,
					sender.GetType().ToString()),
					EventEntryType.Debug);

			try
			{
				//=================== FILE & STATUS STRIP =====================//
				if (menu == mnuSaveBIN || btn == btnSaveBIN)
					UISaveBin();
				else if (menu == mnuSaveBINAs)
					UISaveBinAs();
				else if (menu == mnuOpenADR || status == lblAddressFile || btn == btnOpenADR)
					UIOpenAddressFile();
				else if (menu == mnuOpenBIN || status == lblBinFile || btn == btnOpenBIN)
					UIOpenROMFile();
				else if (menu == mnuSettings)
					UISettings();
				else if (menu == mnuExit)
					UIExit();


				//=================== MAP =====================//
				else if (menu == mnuMapReload)
					_bin.Reload();
				else if (sender as ToolStripButton == btnToggleProp)
					UIMapToggleProperties();
				else if (menu == mnu3DMapViewToggle || btn == btnGraphShowSwitch)
					UIMapGraphSwitch();

				//=================== EMULATOR =====================//
				else if (menu == mnuEmulatorUpload)
					UIEmulatorUpload();
				else if (menu == mnuEmulatorDownload)
					UIEmulatorDownload();

				//=================== CONSULT =====================//
				else if (menu == mnuConsultSelfDiagnostic)
					UIConsultSelfDiagnostic();
				else if (menu == mnuConsultActiveTests)
					UIConsultActiveTest();
				else if (menu == mnuConsultSensorsLive)
					UIConsultSensorsLive();
				else if (menu == mnuConsultSpeedTrial)
					UIConsultSpeedTrial();
				else if (menu == mnuConsultConnect)
					UIConsultConnect(menu);
				//mnuConsultMode item clicked
				else if (mnuConsultMode.DropDownItems.Contains(menu))
					mnuConsultMode_Click(sender, e);

				//=================== HELP =====================//
				else if (menu == mnuHelpAbout)
					new FormAboutBox().Show();
				else if (menu == mnuHelpSendFeedBack)
					UISendFeedBack();
				else if (menu == mnuHelpViewLog)
					UIViewLog();
				else
				{
#if DEBUG
					MessageBox.Show(@"Handle Event:  '" + selectedItem + "'  Not Found!");
#else
					MessageBox.Show(
						this, 
						"� ������� ������ ��� ���������������� �� �����������", 
						FormAboutBox.AssemblyProduct + " " + FormAboutBox.AssemblyVersion, 
						MessageBoxButtons.OK, 
						MessageBoxIcon.Information
					);
#endif
				}
			}
			catch (Exception ex)
			{
				Error(ex, "Error operation '" + selectedItem + "'\n", "Error !");
			}
		}

		private void UIConsultActiveTest()
		{
			FormConsultActiveTest frm = new FormConsultActiveTest(_consult);
			frm.ShowDialog();
		}

		#region UserInterface functions
		private void UIConsultSpeedTrial()
		{
			if (_frmSpeedTrial == null || _frmSpeedTrial.IsDisposed)
				_frmSpeedTrial = new FormSpeedTrial(this._consult);
			_frmSpeedTrial.StartPosition = FormStartPosition.CenterParent;
			_frmSpeedTrial.Show(this);
		}

		private void UIConsultSelfDiagnostic()
		{
			FormDiagnosticCodes frm = new FormDiagnosticCodes(_consult);
			frm.ShowDialog(this);
		}

		FormLogView frmLog;
		private void UIViewLog()
		{
			if (frmLog == null || frmLog.IsDisposed)
				frmLog = new FormLogView(this);
			frmLog.Show();

		}

		private void UISaveBin()
		{
			if (_bin.Initialized)
			{
				string fileName = _bin.DataSourceROM;
				_bin.SaveBin();
				InitAddBinRecentFiles(fileName);
			}
		}

		private void UISettings()
		{
			if (_frmOptions == null || _frmOptions.IsDisposed)
				_frmOptions = new FormSettings(this);
			_frmOptions.ShowDialog(this);
			InitializeEmulatorMenu();
		}

		private void UIConsultSensorsLive()
		{
			if (_frmConsult == null || _frmConsult.IsDisposed)
				_frmConsult = new FormLiveScan(this._consult);
			_frmConsult.Show(this);
		}

		private void UISendFeedBack()
		{
			ShowBugReportWindow("!Custom user bug report",
				new List<string>(new string[] { _bin.DataSourceAddress, _bin.DataSourceROM }),
				false);
		}

		private void UIMapToggleProperties()
		{
			_pGridEnable = btnToggleProp.Checked = !btnToggleProp.Checked;
			if (_pGridEnable)
			{
				int tmp = this.Width;
				splitContMain.Panel2Collapsed = !_pGridEnable;

				this.Width += (int)splitContMain.Panel2.Tag;
				splitContMain.SplitterDistance = tmp;
			}
			else
			{
				splitContMain.Panel2.Tag = this.Width - splitContMain.SplitterDistance;
				this.Width = splitContMain.SplitterDistance;
				splitContMain.Panel2Collapsed = !_pGridEnable;
			}


			//���� �������� ���� �������, ���������� ��� �� �������� ����
			if (_pGridEnable && tabMain.TabCount > 0)
				try
				{
					propertyGrid.SelectedObject = _bin.GetAddressInstance(tabMain.SelectedTab.Text);
				}
				catch (Exception ex)
				{
					Error(ex, null, null);
				}
		}

		private void UIOpenROMFile()
		{
			string fileName = HelperClass.ShowFileDialog(Resources.strBinFilesToShowOpen, false, _cfg.cfgdlgROMPath, this);
			_cfg.cfgdlgROMPath = fileName;
			if (fileName != "")
			{
				string recentFileName = _bin.DataSourceROM;
				_bin.OpenROMFile(fileName);
				InitAddBinRecentFiles(recentFileName);
				//InitInterface();
			}
		}

		private void UIOpenAddressFile()
		{
			string fileName = HelperClass.ShowFileDialog(Resources.strAdrFilesToShowOpen, false, _cfg.cfgdlgADRPath, this);
			_cfg.cfgdlgADRPath = fileName;
			if (fileName != "")
			{
				string recentFileName = _bin.DataSourceAddress;
				_bin.OpenAddressFile(fileName);
				InitAddAdrRecentFiles(recentFileName);
				//InitInterface();
			}
		}

		private void UISaveBinAs()
		{
			if (_bin.Initialized)
			{
				string fileName = HelperClass.ShowFileDialog(Resources.strBinFilesToShowSave, true, _cfg.cfgdlgROMPath, this);
				_cfg.cfgdlgROMPath = fileName;
				if (fileName != "")
				{
					_bin.SaveBin(fileName);
					InitAddBinRecentFiles(fileName);
				}
			}
		}

		private void UIMapGraphSwitch()
		{
			if (cbMaps.SelectedItem != null)
			{
				if (btnGraphShowSwitch.Checked)
				{
					cbMaps.SelectedIndexChanged -= _graphForm.mapChanged;
					_graphForm.Dispose();
					btnGraphShowSwitch.Checked
						= mnu3DMapViewToggle.Checked
						= false;
				}
				else
				{
					if (_graphForm != null)
						_graphForm.Dispose();
					_graphForm = new FormGraph3D((Map)cbMaps.SelectedItem);
					cbMaps.SelectedIndexChanged += new EventHandler(_graphForm.mapChanged);

					_graphForm.Left = this.Right;
					_graphForm.Top = this.Top;
					_graphForm.Show(this);
					this.Focus();
					btnGraphShowSwitch.Checked
						= mnu3DMapViewToggle.Checked
						= true;
				}
			}
		}

		private void UIEmulatorDownload()
		{
			DialogResult res = MessageBox.Show(
				this,
				"����� ������������, ��������� ��������� � ��������� ���������.\r\n����������?",
				"�������� !!!",
				MessageBoxButtons.YesNo);
			if (res == DialogResult.Yes)
				EmulatorDownload();
		}

		private void UIEmulatorUpload()
		{
			DialogResult choise = MessageBox.Show(
				this,
				"����� ������������, ��������� ��������� � ��������� ���������.\r\n����������?",
				"�������� !!!",
				MessageBoxButtons.YesNo);
			if (choise == DialogResult.Yes)
				EmulatorUpload();
		}

		private void UIExit()
		{
			base.Close();
		}

		private void UIConsultConnect(ToolStripMenuItem menu)
		{
			if (menu.Checked)
			{
				_consult.Disconnect();
			}
			else
			{
				string port = _cfg.cfgConsultPort;
				string caption = String.Format("Consult init on port [{0}]", port);
				using (IProgressBar _progressForm = FormProgressBar.GetInstance(caption))
				{
					_progressForm.ShowProgressBar(delegate()
					{
						try
						{
							_consult.Initialise(_cfg.cfgConsultPort);
						}
						catch (ConsultException ex)
						{
							string msg = String.Format("Connect to ECU on [{0}] failed!", port);
							HelperClass.Message(this, ex, msg, MessageBoxIcon.Error);
							_progressForm.StopProgressBar();
						}
					});
				}

			}
			InitializeConsultMenu();
		}
		#endregion

		private void menu_Click(object sender, ToolStripItemClickedEventArgs e)
		{
			menu_Click(e.ClickedItem, e == null ? null : (EventArgs)e);
		}
		private void menu_Click(object sender)
		{
			menu_Click(sender, new EventArgs());
		}

		#endregion

		/// <summary>
		/// ���������� ������� ComboBox ����
		/// </summary>
		private void cbMaps_SelectedIndexChanged(object sender, EventArgs e)
		{
			//ComboBox combo = sender as ComboBox;
			ToolStripComboBox combo = sender as ToolStripComboBox;

			Map map = combo.SelectedItem as Map;
			if (map == null) return;

			int index = combo.SelectedIndex;

			//Property grid
			propertyGrid.SelectedObject = map.Address;
			string cnst = map.Address.ConstName;
			if (!tabMain.TabPages.ContainsKey(cnst)) //���� ���� � ����� ������ ��� - ���������
			{
				FRomGrid grid = new FRomGrid(_bin.GetMap(cnst));

				tabMain.TabPages.Add(cnst, cnst);
				TabPage tab = tabMain.TabPages[cnst];
				tab.Controls.Add(grid);     //��������� �� ���� - ����

				//��������� ����� ������� ����� � ����, ����� ��� ������������ ����� ������������ �����
				tab.Tag = combo.SelectedIndex;

				//����������� ���� �� ����� ������� � �������������
				this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(grid.propertyGrid_PropertyValueChanged);

				txtComment.Text = GetComment(map.Address);
			}

			tabMain.SelectedTab = tabMain.TabPages[cnst];	//�������� �� ������� �������
		}

		private void MainForm_Move(object sender, EventArgs e)
		{
			if (_graphForm != null)
			{
				//if (Math.Abs(_graphForm.Left - this.Right) < 20 && Math.Abs(_graphForm.Top - this.Top) < 20)
				{
					_graphForm.Left = this.Right;
					_graphForm.Top = this.Top;
				}
			}
		}

		/// <summary>
		/// ��������� ��������� ������� �� DropDownItems ���� mnuConsultMode
		/// </summary>
		private void mnuConsultMode_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mnuClicked = sender as ToolStripMenuItem;
			if (mnuClicked == null)
				return;

			IConsultData current = _consult == null ? null : _consult.DataSource;
			IConsultData clicked = _consltDataList[mnuClicked.Text];

			mnuClicked.Checked = true;
			mnuConsultMode.Text = mnuConsultMode.Tag + " [" + mnuClicked.Text + "]";

			//���� ������ ������� ������, �� �� ���� ���.
			if (current == null || current == clicked)
				return;

			_consult.DataSource = clicked;

			//��� ����������� ��������
			string currentTxt = current.ToString();
			ToolStripMenuItem mnuCurrent =
				mnuConsultMode.DropDownItems[currentTxt] as ToolStripMenuItem;
			if (mnuCurrent == null)
				return;
			mnuCurrent.Checked = false;
		}
	}
}