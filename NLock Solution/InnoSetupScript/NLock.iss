; Author : KRV
; Date : 2016/03/29

#define MyAppName "NLock"
#define MyAppVersion "1.1"
#define MyAppPublisher "Neurotechnology Lab (Private) Limited"
#define MyAppURL "http://www.neurotechnology.com/"
#define MyAppExeName "NLock.exe"
; Dont change the extention becaue it is a hardcoded value inside NLock application default is ".nlk"
#define MyAppFileAssoc ".nlk"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{E8506827-8C04-4988-B92C-EABA04BF52A6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
ChangesAssociations = yes
DefaultDirName={pf}\Neurotechnology\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\\NLockFile\Resources\NLock_License_Aggreement.rtf
OutputDir=.\InnoSetupOut
OutputBaseFilename=NLock-Installer
SetupIconFile=..\NLockFile\Resources\LockControls_322.ico
Compression=lzma
SolidCompression=yes            

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
; NLock Files
Source: "..\NLockFile\bin\Win32_x86\NLock.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NLock.exe.config"; DestDir: "{app}"; Flags: ignoreversion
; log4net libraries
Source: "..\NLockFile\bin\Win32_x86\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\log4net.xml"; DestDir: "{app}"; Flags: ignoreversion
; Neurotec Neorotec. Libraries
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Biometrics.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Biometrics.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Biometrics.Gui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Devices.dll"; DestDir: "{app}"; Flags: ignoreversion 
Source: "..\NLockFile\bin\Win32_x86\Neurotec.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Gui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Media.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Media.Processing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\LiveMedia.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NCore.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NMedia.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NMediaProc.dll"; DestDir: "{app}"; Flags: ignoreversion 
Source: "..\NLockFile\bin\Win32_x86\NBiometricClient.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NBiometrics.dll"; DestDir: "{app}"; Flags: ignoreversion 
Source: "..\NLockFile\bin\Win32_x86\NDevices.dll"; DestDir: "{app}"; Flags: ignoreversion
; Ndm files
Source: "..\NLockFile\bin\Win32_x86\NdmMedia.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NdmMedia.ini"; DestDir: "{app}"; Flags: ignoreversion
; Nlicense Files
Source: "..\NLockFile\bin\Win32_x86\Neurotec.Licensing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NLicensing.cfg"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\NLicensing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\Activation\*"; DestDir: "{app}\Activation"; Flags: ignoreversion recursesubdirs createallsubdirs
; Registry icon
Source: "..\NLockFile\Resources\Login_6031.ico*"; DestDir: "{app}\Resources"; Flags: ignoreversion
;Face
Source: "..\NLockFile\bin\Data\Faces.ndf"; DestDir: "{app}\Data"; Flags: ignoreversion
; Installation extra folders
Source: "..\NLockFile\bin\Win32_x86\Cameras\*"; DestDir: "{app}\Cameras"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\NLockFile\bin\Win32_x86\libopenblas.dll"; DestDir: "{app}"; Flags: ignoreversion
;msvcp/r files
Source: "..\NLockFile\bin\Win32_x86\msvcp100.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcp110.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcp80.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcp90.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcr100.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcr110.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcr80.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NLockFile\bin\Win32_x86\msvcr90.dll"; DestDir: "{app}"; Flags: ignoreversion


; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; Extention registration
Root: "HKCR"; Subkey: "{#MyAppFileAssoc}"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: "{#MyAppName}"; ValueType: string; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
Root: "HKCR"; Subkey: "{#MyAppName}\DefaultIcon"; ValueType: string; ValueData: "{app}\Resources\Login_6031.ico,0"
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\open"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" -o ""%1"""
; Shell commands
; NLock menu
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}"; ValueType: string; ValueName: "subcommands"; Flags: uninsdeletekey preservestringtype deletevalue
; NLock menu Lock command
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd1"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Lock"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd1"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd1\command"; ValueType: string; ValueData: "{app}\{#MyAppExeName} -a ""%1"""; Flags: uninsdeletekey preservestringtype deletevalue
; NLock menu Unlock to command
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd2"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Unlock to"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd2"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd2\command"; ValueType: string; ValueData: "{app}\{#MyAppExeName} -ut ""%1"""; Flags: uninsdeletekey preservestringtype deletevalue
; NLock menu Unlock here command
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd3"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Unlock here"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd3"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "{#MyAppName}\Shell\{#MyAppName}\Shell\cmd3\command"; ValueType: string; ValueData: "{app}\{#MyAppExeName} -uh ""%1"""; Flags: uninsdeletekey preservestringtype deletevalue
; Lock - Applies to all files
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{#MyAppName}"; Flags: preservestringtype deletekey deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}"; ValueType: string; ValueName: "AppliesTo"; ValueData: "System.FileExtension:<>{#MyAppFileAssoc}"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}"; ValueType: string; ValueName: "subcommands"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}\Shell\cmd1"; ValueType: string; ValueName: "subcommands"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}\Shell\cmd1"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Add to {#MyAppExeName} list"; Flags: preservestringtype deletekey deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}\Shell\cmd1"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Resources\Login_6031.ico"; Flags: uninsdeletekey preservestringtype deletevalue
Root: "HKCR"; Subkey: "*\shell\{#MyAppName}\Shell\cmd1\command"; ValueType: string; ValueData: "{app}\{#MyAppExeName} -a ""%1"""; Flags: uninsdeletekey preservestringtype deletevalue
