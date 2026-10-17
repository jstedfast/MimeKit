# Security Policy

The MimeKit team takes the security of our software products and services seriously.

If you believe you have found a security vulnerability in the MimeKit repository, please report it to us as described below.

## Supported Versions

Due to the fact that the MimeKit team is small (currently only myself), I can
only support security fixes for the latest version.

| Version | Supported          |
| ------- | ------------------ |
| 4.x     | :white_check_mark: |
| < 4.0   | :x:                |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them via a [Private Security Advisory](https://github.com/jstedfast/MimeKit/security/advisories/new) submission.

For more information about the fields available and guidance on filling in the form, see
"[Creating a repository security advisory](https://docs.github.com/en/code-security/security-advisories/working-with-repository-security-advisories/creating-a-repository-security-advisory)"
and "[Best practices for writing repository security advisories.](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/best-practices-for-writing-repository-security-advisories)"

You should receive a response within 24 hours. If for some reason you do not, please follow up via [email](mailto:jestedfa@microsoft.com?subject=MimeKit%20Security%20Advisory) to ensure we received your original message.

## Verifying Releases

Starting with MimeKit 5.0, every release built by the [Release workflow](.github/workflows/release.yml)
has a signed [build provenance attestation](https://docs.github.com/en/actions/security-guides/using-artifact-attestations-to-establish-provenance-for-builds)
for its NuGet packages and for the assemblies within them. You can use the
[GitHub CLI](https://cli.github.com/) to verify that an assembly was built from this repository:

```
gh attestation verify ~/.nuget/packages/mimekit.core/<version>/lib/net8.0/MimeKit.Core.dll --repo jstedfast/MimeKit
```

Note: Verify the assemblies rather than the `.nupkg` files that you download from nuget.org. NuGet.org adds its own
repository signature to every package, so a downloaded `.nupkg` will not match the attested one.

## Preferred Languages

All communications should be in English.
