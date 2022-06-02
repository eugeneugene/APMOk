;

SetCompress auto
SetCompressor bzip2

; Build Unicode installer
Unicode True

!include "MUI2.nsh"
!include "x64.nsh"
!include "logiclib.nsh"
!include "servicelib.nsh"
!include "nsProcess.nsh"

;--------------------------------
;General

!define PRODUCT "APMOk"
!define VERSION "1.2"
!define SERVICE "APMOkSvc"

Name "${PRODUCT} ${VERSION}"
!define MUI_ICON ..\APMOk\Properties\checked.ico

!verbose push
!verbose 4
!echo "${PRODUCT} ${VERSION}"
!verbose pop

!ifdef OUTFILE
  OutFile ${OUTFILE}
!else
  OutFile "..\Publish\APMOk.Setup\APMOk.Setup.exe"
!endif

; Get installation folder from registry if available
;InstallDir "C:\APMOk"
InstallDirRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "InstallLocation"

; Request application privileges for Windows Vista/7/8/10
RequestExecutionLevel admin

Caption "Контроль значений APM жёстких дисков"
!define MUI_ABORTWARNING

!include "FileFunc.nsh"

;--------------------------------
;Variables

Var cbInstallService
Var InstallServiceState
Var StartMenuFolder

; --------------------------------
; Pages

;!define MUI_COMPONENTSPAGE_TEXT_COMPLIST "Выберите компоненты программы для установки:"

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY

;Start Menu Folder Page Configuration
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\${PRODUCT}" 
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "${PRODUCT}"
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

!insertmacro MUI_PAGE_INSTFILES

Page custom customFinish customFinishLeave

!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_TITLE_3LINES
!define MUI_PAGE_CUSTOMFUNCTION_SHOW FinishShow
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!define MUI_UNFINISHPAGE_NOAUTOCLOSE
!insertmacro MUI_UNPAGE_FINISH
  
; --------------------------------
; Languages
 
!insertmacro MUI_LANGUAGE "Russian"

;--------------------------------
;Reserve Files
  
;These files should be inserted before other files in the data block
;Keep these lines before any File command
;Only for solid compression (by default, solid compression is enabled for BZIP2 and LZMA)
  
!insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------------
;Installer Sections

Section !$(SecAPMOk) SecAPMOk
  Call stopAPMOk

  SectionIn RO
  SetOutPath "$INSTDIR"

  SetOverwrite on
  File /r /x appsettings.Development.json /x appsettings.json ..\Publish\APMOk\*
  File /oname=appsettings.Development.json.dist "..\Publish\APMOk\appsettings.Development.json"
  File /oname=appsettings.json.dist "..\Publish\APMOk\appsettings.json"

  SetOverwrite off
  File "..\Publish\APMOk\appsettings.Development.json"
  File "..\Publish\APMOk\appsettings.json"

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "DisplayName" "${PRODUCT} ${VERSION} (Remove only)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "InstallLocation" "$INSTDIR"

  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application   
  ;Create shortcuts
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\APMOk.lnk" "$INSTDIR\APMOk.exe"
  !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

Section !$(SecAPMOkSvc) SecAPMOkSvc
  SectionIn RO
  SetOutPath "$INSTDIR\APMOkSvc"

  SetOverwrite on
  File /r /x appsettings.Development.json /x appsettings.json ..\Publish\APMOkSvc\*
  File /oname=appsettings.Development.json.dist "..\Publish\APMOkSvc\appsettings.Development.json"
  File /oname=appsettings.json.dist "..\Publish\APMOkSvc\appsettings.json"

  SetOverwrite off
  File "..\Publish\APMOkSvc\appsettings.Development.json"
  File "..\Publish\APMOkSvc\appsettings.json"
SectionEnd

LangString SecAPMOk ${LANG_RUSSIAN} "Приложение APMOk"
LangString SecAPMOkSvc ${LANG_RUSSIAN} "Сервис APMOkSvc"
LangString SecAPMOkDesc ${LANG_RUSSIAN} "Приложение APMOk, включая необходимые компоненты"
LangString SecAPMOkSvcDesc ${LANG_RUSSIAN} "Сервис APMOkSvc, включая необходимые компоненты"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${SecAPMOk} $(SecAPMOkDesc)
!insertmacro MUI_DESCRIPTION_TEXT ${SecAPMOkSvc} $(SecAPMOkSvcDesc)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"
  nsExec::Exec '"$INSTDIR\APMOkSvc\APMOkSvc.exe" /k'
  nsExec::Exec '"$INSTDIR\APMOkSvc\APMOkSvc.exe" /u'

  Call un.stopAPMOk

  Delete "$INSTDIR\APMOkSvc\*"
  RMDir "$INSTDIR\APMOkSvc"

  Delete "$INSTDIR\*"
  RMDir "$INSTDIR"

  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
    
  Delete "$SMPROGRAMS\$StartMenuFolder\APMOk.lnk"
  RMDir "$SMPROGRAMS\$StartMenuFolder"
SectionEnd

;--------------------------------
;Installer Functions

Function .onInit
  System::Call 'kernel32::CreateMutexA(i 0, i 0, t "Global\\__APMOK_SETUP_MTX") i .r1 ?e'
  Pop $R0
  StrCmp $R0 0 +3
    MessageBox MB_OK|MB_ICONEXCLAMATION "Установка уже запущена" /SD IDOK
    Abort

  ; Get the current status of a service
  Push "running"
  Push "${SERVICE}"
  Push ""
  Call Service
  Pop $0 ;response
  ${If} $0 == "true"
    MessageBox MB_OK|MB_ICONEXCLAMATION "Сервис должен быть остановлен перед установкой" /SD IDOK
    Abort
  ${EndIf}
  
  ${If} ${RunningX64}
    ${If} $INSTDIR == "" ; Don't override setup.exe /D=c:\custom\dir
      StrCpy $INSTDIR "$PROGRAMFILES64\${PRODUCT}"
    ${EndIf}
  ${Else}
    MessageBox MB_OK|MB_ICONEXCLAMATION "Ошибка установки!$\n$\n\
    Программа предназначена для работы в 64-х битной версии Windows" /SD IDOK
    Abort
  ${EndIf}
FunctionEnd

Function un.onInit
  System::Call 'kernel32::CreateMutexA(i 0, i 0, t "Global\\__APMOK_SETUP_MTX") i .r1 ?e'
  Pop $R0
  StrCmp $R0 0 +3
    MessageBox MB_OK|MB_ICONEXCLAMATION "Установка уже запущена" /SD IDOK
    Abort
FunctionEnd

Function stopAPMOk
  stopAPMOkloop:
  ${nsProcess::FindProcess} "APMOk.exe" $R0
  StrCmp $R0 0 0 stopAPMOkend

  MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION 'Close "APMOk" before continue$\nOk - Try again$\nCancel - Try to kill "APMOk" and continue' IDOK stopAPMOkloop IDCANCEL 0
  ${nsProcess::KillProcess} "APMOk.exe" $R0
  Sleep 1000

  stopAPMOkend:
  ${nsProcess::Unload}
FunctionEnd

Function un.stopAPMOk
  stopAPMOkloop:
  ${nsProcess::FindProcess} "APMOk.exe" $R0
  StrCmp $R0 0 0 stopAPMOkend

  MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION 'Close "APMOk" before continue$\nOk - Try again$\nCancel - Try to kill "APMOk" and continue' IDOK stopAPMOkloop IDCANCEL 0
  ${nsProcess::KillProcess} "APMOk.exe" $R0
  Sleep 1000

  stopAPMOkend:
  ${nsProcess::Unload}
FunctionEnd

Function customFinish
  nsDialogs::Create 1018
  Pop $0

  ${NSD_CreateLabel} 0 20u 75% 8u "Установка успешно завершена!"
  Pop $0

  ${NSD_CreateCheckbox} 0 40u 75% 8u "Установить и запустить сервис APMOkSvc?"
  Pop $cbInstallService

  nsDialogs::Show
FunctionEnd

Function customFinishLeave
  ${NSD_GetState} $cbInstallService $R0

  ${If} $R0 == ${BST_CHECKED}
    StrCpy $InstallServiceState 1
  ${Else}
    StrCpy $InstallServiceState 0
  ${Endif}
FunctionEnd

Function FinishShow
  StrCmp $InstallServiceState 1 FinishShowEnable
  ${NSD_Uncheck} $mui.FinishPage.Run
  ShowWindow $mui.FinishPage.Run ${SW_HIDE}
  goto FinishShowEnd
  FinishShowEnable:
  nsExec::Exec '"$INSTDIR\APMOkSvc\APMOkSvc.exe" /i'
  nsExec::Exec '"$INSTDIR\APMOkSvc\APMOkSvc.exe" /s'
  ${NSD_Uncheck} $mui.FinishPage.Run
  ShowWindow $mui.FinishPage.Run ${SW_SHOW}
  FinishShowEnd:
FunctionEnd
