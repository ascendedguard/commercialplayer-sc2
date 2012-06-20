;Starboard Install Script
;Uses Modern UI in NSIS
;Written by Will Eddins
;Based off Modern UI Example code

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Twitch Commercial Runner"
  OutFile "TwitchCommercialSetup.exe"
  SetCompressor lzma
  RequestExecutionLevel admin
  
  CRCCheck on

  ;Default installation folder
  !ifdef D
	InstallDir "${D}"
  !else
	InstallDir "$LOCALAPPDATA\Twitch Commercial Runner\Application\"
  !endif
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Ascend\TwitchCommercialSC2" "Install_Dir"
  
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Twitch Commercial Runner"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Will 'Ascend' Eddins"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © 2012"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "Installation package for Twitch Commercial Runner"
  VIProductVersion 1.0.0.0
  
  BrandingText "Twitch Commercial Runner v1.0.0 Installation"
  
;--------------------------------
;Interface Configuration

  !define MUI_HEADERIMAGE
;  !define MUI_HEADERIMAGE_BITMAP "logo.bmp"
  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Prerequisites" Prerequisites

  DetailPrint "Installing Prerequisite Files..."

  SetOutPath "$TEMP"
  
  File .\vcredist_x86.exe
  ExecWait '"$TEMP\vcredist_x86.exe" /passive'
  Delete "$TEMP\vcredist_x86.exe"

  ; Check if .NET 4.0 (Full Version) is already installed
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client" Install
  IntOp $8 $0 & 1
  
  IntCmp $8 1 InstallDone
  
  File .\dotNetFx40_Client_setup.exe
  ExecWait '"$TEMP\dotNetFx40_Client_setup.exe" /passive /showfinalerror /norestart' $0
  Delete "$TEMP\dotNetFx40_Client_setup.exe"
  
  IntCmp $0 1614 RequireReboot
  IntCmp $0 1641 RequireReboot
  IntCmp $0 3010 RequireReboot
  
  Goto InstallDone
  
  RequireReboot:
  ; .NET needs to reboot the system
  SetRebootFlag true
  
  InstallDone:
  ; Nothing left to do here.
  
SectionEnd

Section "Remove Previous Versions" RemovePreviousVersion

  DetailPrint "Uninstall previous version..."

  ; We don't want any left-over files. There's nothing important from previous versions.
  RMDir /r "$LOCALAPPDATA\Twitch Commercial Runner\Application\"

SectionEnd

Section "Required Files" RequiredFiles
SectionIn 1 RO

  DetailPrint "Installation Application Files..."

  SetOutPath "$INSTDIR"
  
  File "..\TwitchCommercialSC2\bin\Release\Newtonsoft.Json.dll"
  File "..\TwitchCommercialSC2\bin\Release\MpqLib.dll"
  File "..\TwitchCommercialSC2\bin\Release\Starcraft2.ReplayParser.dll"
  File "..\TwitchCommercialSC2\bin\Release\TwitchCommercialSC2.exe"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Ascend\TwitchCommercialSC2" "Install_Dir" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  ;Add to Add/Remove Programs
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "DisplayName" "Twitch Commercial Runner"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "InstallLocation" '"$INSTDIR"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "DisplayIcon" '"$INSTDIR\TwitchCommercialSC2.exe"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "Publisher" "Ascend"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "URLInfoAbout" "http://ascendtv.com"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2" "NoRepair" 1
  
SectionEnd

Section "Desktop Shortcut" DesktopShort

  CreateShortCut "$DESKTOP\Twitch Commercial Runner.lnk" "$INSTDIR\TwitchCommercialSC2.exe" "" "" "" "" "" "Starcraft 2 TwitchTV Commercial Automater"

SectionEnd

Section "Start Menu Shortcut" StartShort

  CreateShortCut "$SMPROGRAMS\Twitch Commercial Runner.lnk" "$INSTDIR\TwitchCommercialSC2.exe" "" "" "" "" "" "Starcraft 2 TwitchTV Commercial Automater"

SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_RequiredFiles ${LANG_ENGLISH} "Files required to install Starboard. This step is required."
  LangString DESC_DesktopShort ${LANG_ENGLISH} "Creates a shortcut on the desktop."
  LangString DESC_StartShort ${LANG_ENGLISH} "Creates a shortcut in the start menu."
  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${RequiredFiles} $(DESC_RequiredFiles)
	!insertmacro MUI_DESCRIPTION_TEXT ${DesktopShort} $(DESC_DesktopShort)
	!insertmacro MUI_DESCRIPTION_TEXT ${StartShort} $(DESC_StartShort)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END
 
;--------------------------------
;Uninstaller Section

Section "Uninstall"

  RMDir /r "$LOCALAPPDATA\Twitch Commercial Runner\Application\"
  
  RMDir /r "$LOCALAPPDATA\Twitch Commercial Runner\"
  
  DeleteRegKey HKCU "Software\Ascend\TwitchCommercialSC2"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\TwitchCommercialSC2"
  
  Delete "$DESKTOP\Twitch Commercial Runner.lnk"
  Delete "$SMPROGRAMS\Twitch Commercial Runner.lnk"
  
SectionEnd