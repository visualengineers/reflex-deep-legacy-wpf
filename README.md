# DeeP_LegacyWPF (ReFlex)

Conversion of the legacy WPF application for xCoAx 2015 Paper "Data Exploration on Elastic Projections" to use ReFlex framework

* Version: 2.3.0.0
* ReFlex-Version: 0.9.8
* Websocket-Address (default, can be configured in `appsettings`): `ws://127.,0.0.1:40001/ReFlex`

## change WebSWocket Address

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
