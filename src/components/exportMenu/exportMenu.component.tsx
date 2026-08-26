"use client";

import { useEffect, useRef, useState } from "react";
import { DotsIcon, CsvIcon, PdfIcon } from "@/components/icons/icons";
import "./exportMenu.style.css";

export interface ExportColuna {
    chave: string;
    label: string;
}

interface ExportMenuProps {
    nomeArquivo: string;
    colunas: ExportColuna[];
    dados: Record<string, string | number>[];
}

function baixarArquivo(blob: Blob, nome: string) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = nome;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

function ExportMenu({ nomeArquivo, colunas, dados }: ExportMenuProps) {
    const [aberto, setAberto] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function aoClicarFora(evento: MouseEvent) {
            if (
                containerRef.current &&
                !containerRef.current.contains(evento.target as Node)
            ) {
                setAberto(false);
            }
        }

        document.addEventListener("mousedown", aoClicarFora);
        return () => document.removeEventListener("mousedown", aoClicarFora);
    }, []);

    function exportarCsv() {
        const cabecalho = colunas.map((coluna) => `"${coluna.label}"`).join(";");

        const linhas = dados.map((linha) =>
            colunas
                .map((coluna) => `"${String(linha[coluna.chave] ?? "")}"`)
                .join(";")
        );

        const conteudo = [cabecalho, ...linhas].join("\n");

        // \uFEFF (BOM) garante que o Excel abra os acentos corretamente
        const blob = new Blob(["\uFEFF" + conteudo], {
            type: "text/csv;charset=utf-8;",
        });

        baixarArquivo(blob, `${nomeArquivo}.csv`);
        setAberto(false);
    }

    function exportarPdf() {
        const janela = window.open("", "_blank");

        if (!janela) {
            return;
        }

        const cabecalhoHtml = colunas
            .map((coluna) => `<th>${coluna.label}</th>`)
            .join("");

        const linhasHtml = dados
            .map(
                (linha) =>
                    `<tr>${colunas
                        .map((coluna) => `<td>${linha[coluna.chave] ?? ""}</td>`)
                        .join("")}</tr>`
            )
            .join("");

        janela.document.write(`
      <html>
        <head>
          <title>${nomeArquivo}</title>
          <style>
            body { font-family: Arial, sans-serif; padding: 24px; color: #232633; }
            h1 { font-size: 20px; margin-bottom: 16px; }
            table { width: 100%; border-collapse: collapse; }
            th, td { text-align: left; padding: 8px 12px; border-bottom: 1px solid #eff2fa; font-size: 13px; }
            th { background-color: #eff2fa; }
          </style>
        </head>
        <body>
          <h1>${nomeArquivo}</h1>
          <table>
            <thead><tr>${cabecalhoHtml}</tr></thead>
            <tbody>${linhasHtml}</tbody>
          </table>
        </body>
      </html>
    `);

        janela.document.close();
        janela.focus();
        janela.print();

        setAberto(false);
    }

    return (
        <div className="export-menu" ref={containerRef}>
            <button
                type="button"
                className="export-menu-trigger"
                aria-label="Mais opções"
                onClick={() => setAberto((estado) => !estado)}
            >
                <DotsIcon size={20} />
            </button>

            {aberto && (
                <div className="export-menu-options">
                    <button type="button" onClick={exportarCsv}>
                        <CsvIcon size={20} />
                        Exportar em CSV
                    </button>
                    <button type="button" onClick={exportarPdf}>
                        <PdfIcon size={20} />
                        Exportar em PDF
                    </button>
                </div>
            )}
        </div>
    );
}

export default ExportMenu;