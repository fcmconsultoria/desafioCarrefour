# Diagramas da Arquitetura

Este diretório contém os 8 diagramas da arquitetura em formato Mermaid (.mmd).

## Lista de Diagramas

1. **01-contexto-arquitetura.mmd** - Visão geral dos componentes e suas interações
2. **02-fluxo-lancamento.mmd** - Sequence diagram do processo de criação de lançamento
3. **03-fluxo-consulta-consolidado.mmd** - Sequence diagram das consultas de consolidado
4. **04-arquitetura-componentes.mmd** - Estrutura interna dos serviços (layers)
5. **05-modelo-dados.mmd** - Diagrama Entidade-Relacionamento do banco de dados
6. **06-arquitetura-deploy.mmd** - Visão da infraestrutura em produção (AWS)
7. **07-escalabilidade-performance.mmd** - Como o sistema lida com picos de carga
8. **08-tratamento-falhas-resiliencia.mmd** - Como o sistema se recupera de falhas

## Como Converter para Imagens

### Opção 1: Mermaid Live Editor (Mais Simples)

1. Acesse https://mermaid.live
2. Clique em "Code" e cole o conteúdo do arquivo .mmd
3. O diagrama será renderizado automaticamente
4. Clique em "Download" para salvar como PNG ou SVG

### Opção 2: VS Code com Extensão

1. Instale a extensão "Markdown Preview Mermaid Support" ou "Mermaid Preview"
2. Abra qualquer arquivo .mmd no VS Code
3. Use o preview (Ctrl+Shift+V) para visualizar
4. Use a extensão para exportar como imagem

### Opção 3: Usando Node.js (Automatizado)

```bash
# Instalar ferramenta
npm install -g @mermaid-js/mermaid-cli

# Navegar para pasta de diagramas
cd diagrams

# Converter todos os arquivos
for file in *.mmd; do
    mmdc -i "$file" -o "${file%.mmd}.png" -b transparent
done
```

### Opção 4: GitHub (Após Commit)

1. Faça commit dos arquivos .mmd no GitHub
2. GitHub renderiza diagramas Mermaid automaticamente
3. Você pode fazer screenshot da renderização

## Como Visualizar no Navegador

Alguns navegadores suportam Mermaid nativamente com extensões:
- Chrome: Mermaid Diagrams Extension
- Firefox: Mermaid addon

## Script de Conversão (Windows)

Execute o script `scripts\convert-diagrams.bat` para converter todos os diagramas automaticamente (requer Node.js instalado).

## Notas

- Os diagramas estão em formato vetorial (Mermaid)
- Ao converter para PNG, use alta resolução para qualidade
- SVG é recomendado para documentos (escalável)
- As cores estão configuradas para melhor legibilidade
