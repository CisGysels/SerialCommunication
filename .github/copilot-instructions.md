# Copilot instructions for SerialCommunication

Purpose: concise operational guidance for Copilot sessions working on this repository.

## Quick commands
- Build (Windows / CI): msbuild "SerialCommunication.slnx" /p:Configuration=Debug
- Open and run: open the solution in Visual Studio (recommended for WinForms .NET Framework 4.7.2 projects).
- Arduino: open SerialCommunication\SerialCommunication.ino in the Arduino IDE and upload (Baud: 115200).
- Single test: no automated tests present in the repo.
- Lint: no linting configuration included.

## Where components live
- C# WinForms desktop app: SerialCommunication\ (Form1.cs, Program.cs, SerialCommunication.csproj).
- Arduino firmware/sketch: SerialCommunication\SerialCommunication.ino.
- SerialCommand library (Arduino): SerialCommand.h / SerialCommand.cpp (included in repo).
- UI assets: SerialCommunication\Resources\

## High-level architecture
- Desktop app (C# WinForms) discovers COM ports and connects via System.IO.Ports to the Arduino device.
- Arduino sketch exposes a simple text command protocol (tokenized by the included SerialCommand library).
  - Commands: set, toggle, get, ping, help, debug
  - Digital pins: referenced with prefix "d" (e.g. d2). PWM pins use "pwm" prefix (e.g. pwm9). Analog pins with "a" (a0..a5).
- The C# app is the controller/UI; the sketch performs hardware IO and replies over serial. Keep baudrate in sync (115200).

## Key repository conventions and patterns
- Serial protocol is plain-text, whitespace-delimited. SerialCommand library is used to parse tokens; its behavior (terminator, delimiters) is in SerialCommand.h/.cpp.
- Baudrate is hardcoded to 115200 in the sketch (Baudrate define). If changing the sketch baudrate, update the app default selection in Form1.
- Pin ranges and roles are documented in the sketch: digital outputs 2..4, PWM 9..11, digital inputs 5..7, analog A0..A5.
- The Arduino code includes a local copy of the SerialCommand library (not installed as an Arduino library). When editing the sketch in the Arduino IDE, ensure SerialCommand files are available in the sketch folder or installed as a library.
- UI elements in Form1 use Dutch-like identifiers (e.g., comboBoxPoort). Be careful when searching for controls by name.

## Notes for Copilot sessions
- Prefer editing the sketch under SerialCommunication\ when modifying firmware behavior; SerialCommand.* is part of the same codebase and may be changed here.
- For desktop changes, open SerialCommunication.slnx in Visual Studio. Use msbuild for scripted builds.
- No tests or linters to run — suggest adding CI/tests if requested.
- Resources and images for the UI live under Resources/ — keep image filenames unchanged if referenced from the designer.

## Files checked when writing these instructions
- SerialCommunication\Form1.cs
- SerialCommunication\Program.cs
- SerialCommunication\SerialCommand.h
- SerialCommunication\SerialCommand.cpp
- SerialCommunication\SerialCommunication.ino

---
If you want adjustments (more detail on build targets, CI, or adding test/lint guidance), say which area to expand.