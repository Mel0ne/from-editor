using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using FRom.ConsultNS;
using FRom.ConsultNS.Data;
using FRom.Emulator;
using FRom.Grid;
using FRom.Logger;
using FRom.Logic;
using FRom.Properties;

namespace FRom
{
	public partial class FormMain : Form, IDisposable
	{
		internal From _bin;

		internal Romulator _emulator;

		internal Consult _consult;

		internal Log _log;

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
		internal Settings _settings;

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
			InitializeEmulatorMenu();
			InitializeConsultMenu();
			InitializeMenu(mstrpMain);			//������������� ������������ ��������� ����
			InitializeTabControl();				//������������� ������� � �������			
		}

		private void Dispose_Resources(bool disposing)
		{
			if (disposing)
			{
				if (_log != null)
					_log.Close();
				if (_consult != null)
					_consult.Disconnect();
				if (_emulator != null)
					_emulator.Dispose();
				if (_bin != null)
					_bin.Clear();
				if (_settings != null)
					_settings.Save();
			}
		}

		~FormMain()
		{
			Dispose_Resources(false);
			Dispose(false);
		}

		void IDisposable.Dispose()
		{
			Dispose_Resources(true);
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void InitializeConsultMenu()
		{
			bool flag = _consult.IsOnline;

			mnuConsultConnect.Checked =

			mnuConsultActiveTests.Enabled =
			mnuConsultSelfDiagnosticTool.Enabled =
			mnuConsultSensorsLive.Enabled =
			mnuConsultMode.Enabled =
				//ECU Info area
			txtConsultECUInfo.Visible =
			mnuConsultECUInfoSeparator.Visible =

				flag;

			if (flag)
			{
				txtConsultECUInfo.Text = _consult.GetECUInfo().ToString();
				mnuConsult.Image = Properties.Resources.imgConnected_16x16;
			}
			else
			{
				mnuConsult.Image = Properties.Resources.imgDisconnected_16x16;
			}
		}

		private void InitializeSettings()
		{
			//����
			_log = Log.Instance;
			_log.CatchExceptions = true;
			_log.LogLevel = EventEntryType.Debug;
			_log.LogFileEnabled = true;

			//������� ��������� ������� Click �� ToolStripMenuItems
			_EHmainMenu = new EventHandler(menu_Click);

			//������������� ������ �������� ����������
			_settings = new Settings();
			//���� ��������� ����������, �� �������� �������� ������.
			//������� ��������� �� ���������� ������ � ������ ����
			if (_settings.NeedUpgrade)
			{
				_settings.Upgrade();
				_settings.NeedUpgrade = false;
			}
			//�������� �� ���������� ��������� ���� ��������
			if (_settings.cfg_dlgADRPath == null
				|| _settings.cfg_dlgADRPath.Length == 0
				|| !new DirectoryInfo(_settings.cfg_dlgADRPath).Exists
			)
			{
				_settings.cfg_dlgADRPath = Environment.CurrentDirectory;
			}
			if (_settings.cfg_dlgROMPath == null
				|| _settings.cfg_dlgROMPath.Length == 0
				|| !new DirectoryInfo(_settings.cfg_dlgROMPath).Exists
			)
			{
				_settings.cfg_dlgROMPath = Environment.CurrentDirectory;
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
			_consult = new Consult(_consltDataList[0]);
			//���� ����� ��������� �� ��������������� - ����������� � ���������
			if (_settings.cfg_ConsultConnectAtStartup)
				menu_Click(mnuConsultConnect);

			//������� ����� ������ � ROM/ADR �������
			_bin = new From();
			//����������� ������� ���������� ���������� �� ������� ����� ��������� ������ ������
			_bin.DataSourceChanged += new From.FromEventHandler(InitFRomInterface);
			//InitInterface(_bin, null);

			//������� ���������� ����� ������������ ���� ����������
			if (_settings.cfg_OpenLastConfig)
			{
				if (_settings.cfg_RecentAdrFiles.Count > 0 && File.Exists(_settings.cfg_RecentAdrFiles[0]))
				{
					try { _bin.OpenAddressFile(_settings.cfg_RecentAdrFiles[0]); }
					catch { }
				}
				if (_settings.cfg_RecentBinFiles.Count > 0 && File.Exists(_settings.cfg_RecentBinFiles[0]))
				{
					try { _bin.OpenROMFile(_settings.cfg_RecentBinFiles[0]); }
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
			string romFileName = HelperClass.FindFolder("data") + "R32_rb26det.bin";
			_bin.OpenROMFile(romFileName);
			string addressFileName = HelperClass.FindFolder("data") + "HCR32_RB26_256_E_Z32_444cc.adr";
			_bin.OpenAddressFile(addressFileName);
			_log.LogLevel = EventEntryType.Debug;
			_Debug();
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

			if (_settings.cfg_EmulatorSaveFileAfterRead)
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
			message += HelperClass.GetExceptionInfo(ex);

			_log.WriteEntry(this, message, EventEntryType.Error);

			if (message != "" &&
				MessageBox.Show(message + "\n\n��������� ����� �� ������?", caption, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
			{
				List<string> att = new List<string>();
				bool enableAttach = false;
				if (MessageBox.Show("���������� �������� ���� � ������?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					att.AddRange(new string[] {
						_bin.DataSourceAddress,
						_bin.DataSourceROM
					});
					enableAttach = true;
				}

				ShowBugReportWindow(message, att, enableAttach);
			}
		}

		private void ShowBugReportWindow(string message, List<string> att, bool enableAttach)
		{
			FormFeedBack frm = new FormFeedBack(this, message, att, enableAttach);
			frm.StartPosition = FormStartPosition.CenterParent;
			frm.ShowDialog(this);
		}

		private void Message(string msg, string caption = "", MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			if (caption == "")
				caption = Application.ProductName;
			_log.WriteEntry(this, msg);
			MessageBox.Show(this, msg, caption, MessageBoxButtons.OK, icon);
		}
		private void Message(Exception ex, string caption = "", MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			if (caption == "")
				caption = Application.ProductName;
			string exMsg = HelperClass.GetExceptionMessages(ex);
			MessageBox.Show(this, exMsg, caption, MessageBoxButtons.OK, icon);
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

			System.Collections.Specialized.StringCollection files =
				(menu == mnuRecentFilesROM)
				? (_settings.cfg_RecentBinFiles)
				: (_settings.cfg_RecentAdrFiles);


			if (files.Count == 0)
			{
				menu.DropDownItems.Clear();
				menu.DropDownItems.Add(mnuRecentFilesEmpty);
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
			if (_settings.cfg_RecentAdrFiles == null) return;
			if (_settings.cfg_RecentAdrFiles.Contains(fileName))
				_settings.cfg_RecentAdrFiles.Remove(fileName);
			_settings.cfg_RecentAdrFiles.Insert(0, fileName);
		}
		private void InitAddBinRecentFiles(string fileName)
		{
			if (_settings.cfg_RecentBinFiles == null) return;
			if (_settings.cfg_RecentBinFiles.Contains(fileName))
				_settings.cfg_RecentBinFiles.Remove(fileName);
			_settings.cfg_RecentBinFiles.Insert(0, fileName);
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
				_frmConsult = new FormLiveScan(this);
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
			string fileName = HelperClass.ShowFileDialog(Resources.binFilesToShowOpen, false, _settings.cfg_dlgROMPath, this);
			_settings.cfg_dlgROMPath = fileName;
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
			string fileName = HelperClass.ShowFileDialog(Resources.adrFilesToShowOpen, false, _settings.cfg_dlgADRPath, this);
			_settings.cfg_dlgADRPath = fileName;
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
				string fileName = HelperClass.ShowFileDialog(Resources.binFilesToShowSave, true, _settings.cfg_dlgROMPath, this);
				_settings.cfg_dlgROMPath = fileName;
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

		/// <summary>
		/// ���������� ������� 'Click' �� ����
		/// </summary>
		void menu_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem menu = sender as ToolStripMenuItem;
			if (menu != null && menu.DropDownItems.Count > 0)
				return;

			ToolStripButton btn = sender as ToolStripButton;
			//StatusStrip status = sender as StatusStrip;
			ToolStripLabel status = sender as ToolStripLabel;

			string selectedItem = menu == null
				? (btn == null)
					? (status == null)
						? ""
						: (status.Name)
					: (btn.Name)
				: (menu.Name);
			//ToolStripItemClickedEventArgs args = e as ToolStripItemClickedEventArgs;

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
				else if (menu == mnuConsultSensorsLive)
					UIConsultSensorsLive();
				else if (menu == mnuConsultConnect)
					UIConsultConnect(menu);
				//mnuConsultMode item clicked
				else if (mnuConsultMode.DropDownItems.Contains(menu))
					mnuConsultMode_Click(sender, e);

				//=================== HELP =====================//
				else if (menu == mnuAbout)
					new FormAboutBox().Show();
				else if (menu == mnuSendFeedBack)
					UISendFeedBack();
				else
				{
#if DEBUG
					MessageBox.Show(@"Handle Event:  '" + selectedItem + "'  Not Found!");
#else
				MessageBox.Show(
					this, 
					"� ������� ������ ��� ���������������� �� �����������", 
					AboutBox.AssemblyProduct + " " + AboutBox.AssemblyVersion, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information
				);
#endif
				}
			}
			catch (Exception ex)
			{
				string message = "Error operation '" + selectedItem + "'\n";
				Error(ex, message, "Error !");
			}
		}

		private void menu_Click(object sender, ToolStripItemClickedEventArgs e)
		{
			menu_Click(e.ClickedItem, e == null ? null : (EventArgs)e);
		}
		private void menu_Click(object sender)
		{
			menu_Click(sender, new EventArgs());
		}


		private void UIConsultConnect(ToolStripMenuItem menu)
		{
			if (menu.Checked)
			{
				_consult.Disconnect();
			}
			else
				try
				{
					_consult.Initialise(_settings.cfg_ConsultPort);
				}
				catch (ConsultException ex)
				{
					Message(ex, "Connect to ECU failed!", MessageBoxIcon.Error);
				}
			InitializeConsultMenu();
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



		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Dispose_Resources(true);
		}
	}
}