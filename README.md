# Rainmeter-JsonParser
Json Parser Plugin for Rainmeter


### Instal
Drop JsonParser.dll into Rainmeter Plugins Folder

Drop Newtonsoft.Json.dll into Rainmeter root Folder

### Usage

The plugin requires a Source and a Query

**Soure:** valid json string

**Query:** path to json token. Details here https://www.newtonsoft.com/json/help/html/SelectToken.html

Example:
```
[Weather]
Measure=Plugin
Plugin=WebParser.dll
UpdateRate=600
Url=http://api.openweathermap.org/...
RegExp=(?siU)^(.*)$

[Current.Temp]
Measure=Plugin
Plugin=JsonParser.dll
Source=[Weather]
Query="main.temp"
```
