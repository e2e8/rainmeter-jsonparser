# Rainmeter-JsonParser
Json Parser Plugin for Rainmeter

This plugin is a wrapper around Json.NET _LINQ to JSON_

### Install
Download latest build https://github.com/e2e8/Rainmeter-JsonParser/releases

Drop JsonParser.dll into Rainmeter Plugins Folder

### Usage

The plugin requires a Source and a Query

**Source:** valid json string

**Query:** path to json token. Details here https://www.newtonsoft.com/json/help/html/SelectToken.htm

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
