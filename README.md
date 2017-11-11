# Purpose
Every time the PIA VPN starts, it deploys some Ruby files to a random temp directory and then launches those. The random path prevents it from being whitelisted by firewalls. This program helps to run PIA from a fixed location so it can be whitelisted in firewalls.

# First-time Setup
1. Start PIA as usual so it can deploy its files to a random temp directory.
2. Find this directory. It seems to match the pattern `%USERPROFILE%\AppData\Local\Temp\ocr*.tmp` and should contain `bin`, `lib`, and `src` folders.
3. Copy the contents of this directory into a new directory called `pia_ruby_files` inside your PIA install folder (probably `%PROGRAMFILES%\pia_manager` unless you changed it). So when you're done you should have `pia_manager\pia_ruby_files\src` etc.
4. Rename the existing `pia_manager.exe` inside your PIA install folder to something like `pia_manager.exe.backup`.
5. Build this project and copy the output `pia_manager.exe` into your PIA install folder. That should be it!

# Usage
Calling this program with no arguments will have it first kill any existing PIA processes and then start up PIA using the fixed `pia_ruby_files` location. If you had PIA set to automatically start with Windows before, that should continue to work - it will call `pia_manager.exe` with no arguments and PIA should start up as usual, but from this fixed path.

If you call `pia_manager.exe` with the `--stop` argument, it will just kill any existing PIA processes. This may be useful for automation applications to give you a way to start/stop PIA at will.