# DeeP_LegacyWPF (ReFlex)

[![Tests][test-ci-shield]][test-ci-url]
[![Release][release-ci-shield]][release-ci-url]

Conversion of the legacy WPF application for xCoAx 2015 Paper "Data Exploration on Elastic Projections" to use ReFlex framework

* Version: 2.4.0
* ReFlex-Version: 0.9.8 or newer
* Websocket-Address (default, can be configured in `appsettings`): `ws://127.0.0.1:40001/ReFlex`

![App Screenshot](docs/img/example.png)

## Prerequisites

.NET 10.0 SDK [Download][net-download]

## Change WebSocket Address

edit `App.config` (in project) or `DeeP.dll.config` (in compiled project) and edit the lines: 

``` xml

    <setting name="ReFlexServerAddress" serializeAs="String">
      <value>localhost</value>
    </setting>
    <setting name="ReFlexServerPort" serializeAs="String">
      <value>40001</value>
    </setting>
    <setting name="ReFlexServerEndpoint" serializeAs="String">
      <value>ReFlex</value>
    </setting>
```

## Enable Diagnostics Data

Sending diagnostics data is disabled by default - to enable it edit `App.config` (in project) or `DeeP.dll.config` (in compiled project) and edit the lines:

``` xml

    <setting name="SendDiagnosticData" serializeAs="String">
        <value>False</value>
    </setting>
    <setting name="DiagnosticsAddress" serializeAs="String">
        <value>http://localhost:4302/log/appData</value>
    </setting>
```

## Keyboard Shortcuts

| Shortcut               | Description                                                                                       |
|------------------------|---------------------------------------------------------------------------------------------------|
| `F1`                   | Toggle Help Panel                                                                                 |
| `F2`                   | Toggle Log Window                                                                                 |
| `F10`                  | Toggle Properties Panel                                                                           |
| `F11`                  | Minimize Main Window                                                                              |
| `F12`                  | Toggle Fullscreen                                                                                 |
| `Esc`                  | Close the application                                                                             |


## ReFlex usage

* the source files from [ReFlex Framework][reflex-url] are copied into `library/src/Core/Common` and `library/src/Core/Networking` (from [Version 0.9.8](https://github.com/visualengineers/reflex/releases/tag/v0.9.8))

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[net-download]: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
[reflex-url]: https://github.com/visualengineers/reflex
[release-ci-shield]: https://github.com/visualengineers/reflex-deep-legacy-wpf/actions/workflows/release.yml/badge.svg
[release-ci-url]: https://github.com/visualengineers/reflex-deep-legacy-wpf/actions/workflows/release.yml
[test-ci-shield]: https://github.com/visualengineers/reflex-deep-legacy-wpf/actions/workflows/smoke-test.yml/badge.svg
[test-ci-url]: https://github.com/visualengineers/reflex-deep-legacy-wpf/actions/workflows/smoke-test.yml
