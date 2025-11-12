; ===== CONFIGURACIÓN =====
[Setup]
AppName=NoBright
AppVersion={#Version}
DefaultDirName={pf}\NoBright
OutputBaseFilename=NoBright-installer
DisableDirPage=no
Compression=lzma
SolidCompression=yes
CreateAppDir=yes
AllowNoIcons=yes

; Detecta idioma del sistema y selecciona uno automáticamente
; (NO usamos DefaultLanguage porque no es reconocido por la versión del compilador en el runner)
LanguageDetectionMethod=uilanguage

; ===== IDIOMAS =====
[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

; ===== PÁGINAS OPCIONALES =====
[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "{cm:CreateStartMenuIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

; ===== ARCHIVOS =====
[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; ===== ACCESOS DIRECTOS =====
[Icons]
Name: "{commondesktop}\NoBright"; Filename: "{app}\NoBright.exe"; Tasks: desktopicon
Name: "{group}\NoBright"; Filename: "{app}\NoBright.exe"; Tasks: startmenuicon

; ===== FINAL — EJECUTAR APLICACIÓN =====
[Run]
Filename: "{app}\NoBright.exe"; Description: "{cm:LaunchProgram,NoBright}"; Flags: nowait postinstall skipifsilent unchecked
