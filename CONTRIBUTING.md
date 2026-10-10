# Contributing to DSZ Emulator Frontend

Thank you for your interest in contributing to DSZ! We welcome contributions from developers, testers, and documentation writers of all experience levels.

Please take a moment to review these guidelines before getting started.

---

## Code of Conduct

By participating in this project, you agree to treat all community members with respect, maintain a welcoming environment, and provide constructive feedback.

---

## How Can I Contribute?

### 1. Reporting Bugs
If you encounter a bug or unexpected behavior:
1. Check the [Issues](https://github.com/tiadiff/Nintendo-DS-Emulator/issues) tab to ensure the issue has not already been reported.
2. If it hasn't, open a new issue with a descriptive title.
3. Include relevant details:
   - Your Windows OS version (e.g., Windows 10 22H2, Windows 11 23H2).
   - CPU and GPU specifications.
   - Exact steps to reproduce the bug.
   - Any log output or crash stack traces (check `debug_emu.txt` or console logs if applicable).
   > **Note:** Never upload or link to copyrighted Nintendo DS ROMs, BIOS, or firmware files in bug reports.

### 2. Suggesting Features
Have an idea to improve DSZ?
- Open an issue categorized as a feature request.
- Explain the use case, why it benefits users, and how you envision it working.

### 3. Submitting Code Contributions

#### Prerequisites & Development Setup
- **Operating System:** Windows 10 (1809+) or Windows 11 (x64)
- **SDK:** [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- **IDE:** Visual Studio 2022 / 2025 (with .NET desktop development workload) or Visual Studio Code / JetBrains Rider

#### Development Workflow
1. **Fork the repository** on GitHub and clone your fork locally:
   ```bash
   git clone https://github.com/<your-username>/Nintendo-DS-Emulator.git
   cd Nintendo-DS-Emulator
   ```
2. **Create a topic branch** from `main`:
   ```bash
   git checkout -b feature/my-new-feature
   ```
3. **Build the project** in Release configuration:
   ```bash
   dotnet build -c Release
   ```
4. **Make your changes**:
   - Adhere to the existing C# coding style and conventions.
   - Prioritize safe memory marshaling when interacting with the Libretro C API.
   - Avoid `unsafe` code blocks where safe marshaling or `Span<T>` / `Memory<T>` can be used.
   - Keep comments and documentation clear and written in English.
5. **Test your changes**:
   - Ensure the application builds cleanly with zero warnings or errors.
   - Test emulation stability and performance with legal homebrew or legally dumped backups.
6. **Commit your changes**:
   - Write clear, descriptive commit messages in English (e.g., `feat: add toggle for custom screen layouts` or `fix: handle window resize gracefully`).
7. **Submit a Pull Request (PR)**:
   - Push your branch to your fork: `git push origin feature/my-new-feature`
   - Open a PR against the `master` branch of `tiadiff/Nintendo-DS-Emulator`.
   - Provide a clear description of the changes, the problem solved, and any testing performed.

---

## Legal & Compliance Notice

- **No Copyrighted Material:** Do not commit or submit pull requests containing copyrighted ROMs, Nintendo DS BIOS files (`bios7.bin`, `bios9.bin`), or firmware dumps (`firmware.bin`).
- Contributions must be your original work or compatible with the repository's open-source license.
