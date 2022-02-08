NSIS=""%ProgramFiles(x86)%\NSIS\makensis.exe""
SCRIPT=Script.nsi
OUTPUT="..\Publish\APMOk.Setup\APMOk.Setup.exe"
DEPEND=..\Publish\APMOk\APMOk.dll \
..\Publish\APMOk\APMOkLib.dll \
..\Publish\APMOk\APMDataProto.dll \
..\Publish\APMOk\appsettings.json \
..\Publish\APMOk\appsettings.Development.json \
..\Publish\APMOkSvc\APMOkSvc.dll \
..\Publish\APMOkSvc\hw.dll \
..\Publish\APMOkSvc\APMOkLib.dll \
..\Publish\APMOkSvc\APMDataProto.dll \
..\Publish\APMOkSvc\appsettings.json \
..\Publish\APMOkSvc\appsettings.Development.json \
$(SCRIPT) servicelib.nsh

all: build

clean:
	del /q $(OUTPUT)

build: $(OUTPUT)

rebuild: clean build

$(OUTPUT): $(DEPEND)
	$(NSIS) $(SCRIPT)
