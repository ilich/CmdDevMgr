# CmdDevMgr
Command line hardware manager.

## Usage

Show all devices on you PC. The output will be device instance ID, device descriptions and the list of hardware IDs.
```
C:>CmdDevMgr.exe list
```

Find USD devices.
```
C:>CmdDevMgr.exe list USB
```

Find enabled USD devices.
```
C:>CmdDevMgr.exe list USB -e
```

Find disabled USD devices.
```
C:>CmdDevMgr.exe list USB -d
```

You can omit *'USB'* to show the list of enabled or disabled devices.

Show device status.
```
C:>CmdDevMgr.exe status ROOT\SYSTEM\0000
```

Enable device. The command has to be executed by **Administartor**.
```
C:>CmdDevMgr.exe enable ROOT\SYSTEM\0000
```

Disable device. The command has to be executed by **Administartor**.
```
C:>CmdDevMgr.exe disable ROOT\SYSTEM\0000
```