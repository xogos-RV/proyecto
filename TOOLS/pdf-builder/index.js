#!/usr/bin/env node

const { program } = require('commander');
const fs = require('fs-extra');
const path = require('path');
const marked = require('marked');
const puppeteer = require('puppeteer');

// Configurar opciones de línea de comandos
program
    .version('1.0.0')
    .description('Convierte archivos Markdown a PDF con estilos CSS personalizados')
    .requiredOption('-i, --input <path>', 'Ruta al archivo Markdown de entrada')
    .option('-o, --output <path>', 'Ruta para guardar el PDF generado')
    .option('-c, --css <path>', 'Ruta al archivo CSS personalizado')
    .parse(process.argv);

const options = program.opts();

async function convertMarkdownToPdf() {
    try {
        if (!fs.existsSync(options.input)) {
            console.error(`Error: El archivo ${options.input} no existe.`);
            process.exit(1);
        }
        const markdownContent = fs.readFileSync(options.input, 'utf-8');
        const htmlContent = marked.parse(markdownContent);

        let cssPath;
        if (options.css) {
            if (!fs.existsSync(options.css)) {
                console.error(`Error: El archivo CSS ${options.css} no existe.`);
                process.exit(1);
            }
            cssPath = options.css;
        } else {
            cssPath = path.join(__dirname, 'css/default-style.css');
        }

        const cssContent = fs.readFileSync(cssPath, 'utf-8');

        let outputPath;
        if (options.output) {
            outputPath = options.output;
        } else {
            const inputBasename = path.basename(options.input, path.extname(options.input));
            outputPath = path.join(path.dirname(options.input), `${inputBasename}.pdf`);
        }

        const fullHtml = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <style>${cssContent}</style>
        </head>
        <body>
            ${htmlContent}
        </body>
        </html>
        `;

        const browser = await puppeteer.launch();
        const page = await browser.newPage();
        await page.setContent(fullHtml, { waitUntil: 'networkidle0' });

        fs.ensureDirSync(path.dirname(outputPath));

        await page.pdf({
            path: outputPath,
            format: 'A4',
            margin: {
                top: '1cm',
                right: '1cm',
                bottom: '1cm',
                left: '1cm'
            },
            printBackground: true
        });

        await browser.close();

        console.log(`PDF generado exitosamente: ${outputPath}`);
    } catch (error) {
        console.error('Error al convertir Markdown a PDF:', error);
        process.exit(1);
    }
}

convertMarkdownToPdf();