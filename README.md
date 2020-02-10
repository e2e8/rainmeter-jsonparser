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

Queries can be JSONPath or json property names with dot and index notation

- Dot and index notation:
    | | |
    | --- | --- |
    |dot notation | ```a.b``` |
    |index notation | ```a['b']``` |
    |arrays| ```a[0]```|

    **Note**: json properties with a space must be accessed using index notation
    
- JSONPath: https://goessner.net/articles/JsonPath/


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

### Options

**Logging:** Set logging options. Syntax: _Option:Value,Option:Value..._

| Options | |
| --- | --- |
| NoMatch | Log Warning when no match for query found   |
| EmptySource | Log Error when source is empty          |

| Values |     |
| ------ | --- |
| 0      | Off |
| 1      | On  |

Example:
```
[Data]
Measure=Plugin
Plugin=JsonParser.dll
Source=...
Query=...
Logging=NoMatch:0,EmptySource:0
```

**Locale:** Set locale value. This defines certain region specific format such as decimal seperator. Defaults to InvariantCulture (empty string) which is basically English/American formating. More here: https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo

Example:
```
[Data]
Measure=Plugin
Plugin=JsonParser.dll
Source=...
Query=...
Locale="en-US"
```

### Section Variables

JsonParser provides command that can be used in section variables

**Query:** Parse Json inline. Arguments: _Query_ OR _Query, Source_ 

Example:
```
[MeasureJson]
Measure=Plugin
Plugin=JsonParser.dll

[Data]
Meter=String
Text=[MeasureJson:Query(query,{...})]
```

**Length:** Length of json array. Arguments: _Query_ OR _Query, Source_

Example:
```
[MeasureJson]
Measure=Plugin
Plugin=JsonParser.dll

[Length]
Measure=Calc
Formula=[MeasureJson:Length(query,{...})]
```
### Changes
