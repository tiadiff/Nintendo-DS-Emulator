# DSZ Emulator Frontend

DSZ is a lightweight, high-performance frontend for Nintendo DS emulation, built natively for Windows using C# and WinForms. Under the hood, it harnesses the power of the libretro API to interface with the renowned melonDS core, delivering a seamless and highly optimized gaming experience. 

<img width="742" height="771" alt="Screenshot 2026-10-08 201912" src="https://github.com/user-attachments/assets/29d2a37f-72b7-4c6f-b44d-acd8a1d47127" />

Designed for users who want a straightforward, no-nonsense application to play their Nintendo DS backups, DSZ eliminates the clutter of massive multi-emulator frontends. It focuses entirely on providing a smooth, native Windows experience with exactly the features you need.

## Core Features

- Direct melonDS Integration: Connects directly to the melonDS libretro core dynamically. No complex setups or dependencies required.
- Native UI: Built entirely in C# WinForms, guaranteeing minimal overhead and immediate startup times. The interface blends seamlessly with the Windows desktop environment.
- Hardware-Accelerated Rendering: Video output is handled efficiently through direct memory marshaling, maintaining 60 frames per second without frame pacing issues.
- Dynamic Fast-Forward: Integrated frame skipping and fast-forward controls allow you to speed up gameplay on the fly. Choose between 2x, 3x, 4x, or fully uncapped framerates directly from the settings panel.
- Interactive Touch Screen: Full support for Nintendo DS touch input via mouse integration, passing coordinates and click states accurately to the emulator core.
- Real-time Environment Adaptation: Features a dark overlay toggle (background brightness dimming) and live FPS monitoring superimposed on the display.
- Save State Management: Automatic management of standard .sav files directly in the ROM directory, ensuring your progress is never lost.

## Why DSZ?

Many modern frontends are built on heavy web frameworks or require massive installations to run a single console core. DSZ is built on the philosophy of doing one thing perfectly. By binding the libretro C API directly into C# via P/Invoke, we cut out the middleman. The result is a highly responsive application that consumes very little RAM and CPU outside of what the emulation core itself demands. 

The codebase has been refined to eliminate unsafe code blocks, utilizing secure memory marshaling techniques to parse Libretro memory maps and environment callbacks. This makes the project highly stable and crash-resistant.

## Technical Architecture

The frontend acts as a Libretro host. It loads the core DLL dynamically and handles all required callbacks:
- Environment Callback: Parses memory maps, variables (like touch mode), and system directories.
- Video Refresh Callback: Renders the 16-bit RGB555 memory buffer to a high-performance Windows Forms PictureBox using Bitmap locking.
- Audio Sample Callback: Synchronizes and outputs the audio stream using modern Windows Audio APIs.
- Input State Callback: Maps standard keyboard inputs and mouse clicks to the virtual Nintendo DS buttons (A, B, X, Y, D-Pad, L, R, Start, Select) and touch panel.

## Usage Guide

1. Place your Nintendo DS ROMs (in .nds format) into the designated Resources folder or load them directly.
2. Launch the application. The emulator will automatically bootstrap the melonDS core.
3. Access the Settings panel to adjust the background brightness to your liking, toggle the FPS counter, or change the fast-forward multiplier for long RPG grinds.
4. Audio volume can be adjusted directly from the main interface using the custom slider.

Note: BIOS and Firmware files required by melonDS should be placed in the system directory for full compatibility with all titles.

## Building from Source

To compile DSZ yourself, you will need the .NET 10.0 SDK installed on your Windows machine. 

Open a terminal in the project directory and run:
dotnet build -c Release

The compiled executable and its dependencies will be placed in the \bin\Release directory. 

## Limitations

- GameBoy Advance ROMs (.gba) are not supported. This frontend is strictly configured for Nintendo DS operation.
- Advanced VRAM and OAM debugging tools have been disabled, as the melonDS libretro core does not publicly expose these memory addresses to the frontend.

## Disclaimer

DSZ is an emulation frontend and does not include any copyrighted ROMs, BIOS, or firmware files. Users must provide their own legally obtained backups.
