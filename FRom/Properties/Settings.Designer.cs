﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FRom.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />")]
        public global::System.Collections.Specialized.StringCollection cfgRecentBinFiles {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["cfgRecentBinFiles"]));
            }
            set {
                this["cfgRecentBinFiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />")]
        public global::System.Collections.Specialized.StringCollection cfgRecentAdrFiles {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["cfgRecentAdrFiles"]));
            }
            set {
                this["cfgRecentAdrFiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool cfgOpenLastConfig {
            get {
                return ((bool)(this["cfgOpenLastConfig"]));
            }
            set {
                this["cfgOpenLastConfig"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string cfgdlgADRPath {
            get {
                return ((string)(this["cfgdlgADRPath"]));
            }
            set {
                this["cfgdlgADRPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string cfgdlgROMPath {
            get {
                return ((string)(this["cfgdlgROMPath"]));
            }
            set {
                this["cfgdlgROMPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("adr\\")]
        public string cfgADRFilesPath {
            get {
                return ((string)(this["cfgADRFilesPath"]));
            }
            set {
                this["cfgADRFilesPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("bin\\")]
        public string cfgROMFilesPath {
            get {
                return ((string)(this["cfgROMFilesPath"]));
            }
            set {
                this["cfgROMFilesPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM12")]
        public string cfgConsultPort {
            get {
                return ((string)(this["cfgConsultPort"]));
            }
            set {
                this["cfgConsultPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string cfgEmulatorPort {
            get {
                return ((string)(this["cfgEmulatorPort"]));
            }
            set {
                this["cfgEmulatorPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool cfgConsultConnectAtStartup {
            get {
                return ((bool)(this["cfgConsultConnectAtStartup"]));
            }
            set {
                this["cfgConsultConnectAtStartup"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool cfgEmulatorConnectAtStartup {
            get {
                return ((bool)(this["cfgEmulatorConnectAtStartup"]));
            }
            set {
                this["cfgEmulatorConnectAtStartup"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NeedUpgrade {
            get {
                return ((bool)(this["NeedUpgrade"]));
            }
            set {
                this["NeedUpgrade"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool cfgEmulatorSaveFileAfterRead {
            get {
                return ((bool)(this["cfgEmulatorSaveFileAfterRead"]));
            }
            set {
                this["cfgEmulatorSaveFileAfterRead"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public char cfgConsultDiagECUType {
            get {
                return ((char)(this["cfgConsultDiagECUType"]));
            }
            set {
                this["cfgConsultDiagECUType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string cfgUserName {
            get {
                return ((string)(this["cfgUserName"]));
            }
            set {
                this["cfgUserName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string cfgUserEmail {
            get {
                return ((string)(this["cfgUserEmail"]));
            }
            set {
                this["cfgUserEmail"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool cfgConsultKeepALive {
            get {
                return ((bool)(this["cfgConsultKeepALive"]));
            }
            set {
                this["cfgConsultKeepALive"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool cfgFormLogTopMost {
            get {
                return ((bool)(this["cfgFormLogTopMost"]));
            }
            set {
                this["cfgFormLogTopMost"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool cfgFormLogAttachToMain {
            get {
                return ((bool)(this["cfgFormLogAttachToMain"]));
            }
            set {
                this["cfgFormLogAttachToMain"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::FRom.Consult.TyreParams cfgTyreOrigin {
            get {
                return ((global::FRom.Consult.TyreParams)(this["cfgTyreOrigin"]));
            }
            set {
                this["cfgTyreOrigin"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::FRom.Consult.TyreParams cfgTyreCurrent {
            get {
                return ((global::FRom.Consult.TyreParams)(this["cfgTyreCurrent"]));
            }
            set {
                this["cfgTyreCurrent"] = value;
            }
        }
    }
}
