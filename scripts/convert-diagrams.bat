@echo off
echo Convertendo diagramas Mermaid para imagens PNG...
echo.

cd /d %~dp0\..\diagrams

for %%f in (*.mmd) do (
    echo Convertendo %%f...
    npx @mermaid-js/mermaid-cli -i "%%f" -o "%%~nf.png" -b transparent
)

echo.
echo Conversao concluida!
echo Os diagramas estao na pasta: diagrams\
pause
