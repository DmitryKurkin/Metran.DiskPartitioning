WScript.Echo("==== Autoversioning utility v2.0 ====\n");

if (WScript.Arguments.length !== 1) {
    WScript.Echo("Usage: <script> <inputFilePath>");
    WScript.Quit(1);
}

var inputFilePath = WScript.Arguments.Item(0);

var fso = new ActiveXObject("Scripting.FileSystemObject");
if (!fso.FileExists(inputFilePath)) {
    WScript.Echo("The input file is not found: " + inputFilePath);
    WScript.Quit(1);
}

var inputFileHandle = fso.GetFile(inputFilePath);
if (inputFileHandle.attributes & 1) {
    WScript.Echo("The input file is read-only: " + inputFilePath);
    WScript.Echo("Trying to checkout...");

    var shell = new ActiveXObject("WScript.Shell");
    var checkoutResult = shell.Run('"%VS140COMNTOOLS%..\\IDE\\TF.exe" checkout ' + inputFilePath, 0, true);
    if (checkoutResult !== 0) {
        WScript.Echo("Failed to checkout input file: " + inputFilePath);
        WScript.Echo("TF.EXE exite code: " + checkoutResult);
        WScript.Quit(1);
    }
}
else {
    WScript.Echo("The input file is writable: " + inputFilePath);
    WScript.Echo("Skipping the checkout phase...");
}

try {
    var srcFile = fso.OpenTextFile(inputFilePath, 1);
    var conts = srcFile.ReadAll();
}
catch (error) {
    WScript.Echo("Failed to read the source file: " + error.message);
    WScript.Quit(1);
}
finally {
    if (srcFile){
        srcFile.Close();
    }
}

var regex = /Version = "(\d+\.)(\d+\.)(\d+\.)(\d+)"/;

var match = regex.exec(conts);
if (!match) {
    WScript.Echo("The Version constant is not found");
    WScript.Quit(1);
}

var newRev = +match[4] + 1;
var newConts = conts.replace(regex, 'Version = "'.concat(match[1], match[2], match[3], newRev.toString(), '"'));

try {
    var dstFile = fso.OpenTextFile(inputFilePath, 2, true);
    dstFile.Write(newConts);
} catch (error) {
    WScript.Echo("Failed to write the destination file: " + error.message);
    WScript.Quit(1);
}
finally {
    if(dstFile){
        dstFile.Close();
    }
}

WScript.Echo("==== Done! ====\n");
