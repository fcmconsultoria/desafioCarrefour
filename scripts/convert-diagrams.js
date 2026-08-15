const fs = require('fs');
const path = require('path');
const { mermaid } = require('@mermaid-js/mermaid-cli');

// Configuração
const diagramsPath = path.join(__dirname, '../passoapasso/diagramas.md');
const outputPath = path.join(__dirname, '../diagrams');

// Criar diretório de saída
if (!fs.existsSync(outputPath)) {
    fs.mkdirSync(outputPath, { recursive: true });
}

// Ler arquivo de diagramas
const diagramsContent = fs.readFileSync(diagramsPath, 'utf8');

// Extrair blocos de código Mermaid
const mermaidBlocks = diagramsContent.match(/```mermaid\n([\s\S]*?)\n```/g);

if (!mermaidBlocks) {
    console.log('Nenhum diagrama Mermaid encontrado');
    process.exit(1);
}

console.log(`Encontrados ${mermaidBlocks.length} diagramas`);

// Converter cada diagrama
mermaidBlocks.forEach((block, index) => {
    const code = block.replace(/```mermaid\n/, '').replace(/\n```/, '');
    const outputFile = path.join(outputPath, `diagram-${index + 1}.png`);
    
    try {
        // Usar mermaid-cli para converter
        const { execSync } = require('child_process');
        const tempFile = path.join(__dirname, `temp-${index}.mmd`);
        fs.writeFileSync(tempFile, code);
        
        execSync(`npx @mermaid-js/mermaid-cli -i ${tempFile} -o ${outputFile}`);
        
        fs.unlinkSync(tempFile);
        console.log(`Diagrama ${index + 1} convertido: ${outputFile}`);
    } catch (error) {
        console.error(`Erro ao converter diagrama ${index + 1}:`, error.message);
    }
});

console.log('Conversão concluída!');
