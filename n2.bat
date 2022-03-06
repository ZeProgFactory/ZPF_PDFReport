
Rem  *** compiles ZPF_PDFReport *** 
..\..\_Units_\_Tools_\Nuget pack ZPF_PDFReport\ZPF_PDFReport.csproj -build -Properties Configuration=Release 
move ZPF_PDFReport\bin\Release\*.nupkg .
 
pause
Rem  *** publish all ***
..\..\_Units_\_Tools_\Nuget push *.nupkg MossIsTheBoss -Source http://nugets.azurewebsites.net/nuget 
del *.nupkg
pause
