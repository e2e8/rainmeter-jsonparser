rm $env:APPDATA\Rainmeter\Plugins\JsonParser.dll
rm "C:\Program Files\Rainmeter\Newtonsoft.Json.dll"

cp JsonParserPlugin\x64\Debug\JsonParser.dll $env:APPDATA\Rainmeter\Plugins
cp JsonParserPlugin\x64\Debug\Newtonsoft.Json.dll "C:\Program Files\Rainmeter"