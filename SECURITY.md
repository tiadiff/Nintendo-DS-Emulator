# Security Policy

The DSZ team takes security and user safety seriously. We appreciate your efforts to responsibly disclose any potential security issues.

---

## Supported Versions

Security updates and patches are provided for the latest release branch of DSZ.

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

---

## Reporting a Vulnerability

If you discover a security vulnerability or exploit (such as memory corruption, buffer overflow during Libretro memory marshaling, or unsafe file handling):

1. **Do not disclose the vulnerability publicly** on GitHub Issues, pull requests, or public forums.
2. Please report the vulnerability privately via **[GitHub Private Vulnerability Reporting](https://github.com/tiadiff/Nintendo-DS-Emulator/security/advisories/new)**.
3. If private reporting is unavailable, you may contact the maintainer directly at: `mattia.scalise@gmail.com` with the subject line `[SECURITY] DSZ Vulnerability Report`.

### What to Include in Your Report
To help us investigate and reproduce the issue quickly, please provide:
- A clear description of the vulnerability and its potential impact.
- Detailed step-by-step instructions to reproduce the issue.
- Proof of Concept (PoC) code or non-copyrighted test ROM/binary demonstrating the behavior.
- Operating system version, .NET runtime version, and hardware specifications.

---

## Response & Disclosure Process

- **Acknowledgement:** We will acknowledge receipt of your vulnerability report within 48 to 72 hours.
- **Investigation:** We will investigate and verify the report, providing updates on remediation progress.
- **Fix & Advisory:** Once a patch is developed and verified, we will publish a security advisory and release an updated build, crediting you for the discovery (unless you prefer to remain anonymous).
