"use client";

import { useRouter } from "next/navigation";
import { useRef, useState, useTransition } from "react";
import type { Anexo } from "@/lib/api";
import { formatarDataHora, formatarTamanho } from "@/lib/formatos";
import { removerAnexo } from "../actions";
import { Botao } from "@/components/ui/botao";

const MAXIMO_ANEXOS = 3; // Q22
const LIMITE_BYTES = 50 * 1024 * 1024; // P1
const TIPOS_ACEITOS = ["application/pdf", "image/jpeg", "image/png"];

type Mensagem = { tipo: "ok" | "erro"; texto: string };

/**
 * O upload vai por fetch ao route handler (não por server action, limitada a 1 MB), que repassa o arquivo em stream.
 * A checagem aqui é só conveniência: a API confere o formato pelos bytes do arquivo.
 */
export function AnexosExame({ exameId, anexos }: { exameId: string; anexos: Anexo[] }) {
  const router = useRouter();
  const entrada = useRef<HTMLInputElement>(null);
  const [enviando, setEnviando] = useState(false);
  const [removendo, iniciarRemocao] = useTransition();
  const [mensagem, setMensagem] = useState<Mensagem | null>(null);
  const cheio = anexos.length >= MAXIMO_ANEXOS;

  async function enviar(arquivo: File) {
    setMensagem(null);
    if (!TIPOS_ACEITOS.includes(arquivo.type))
      return setMensagem({ tipo: "erro", texto: "Formato não aceito: envie PDF, JPG ou PNG." });
    if (arquivo.size > LIMITE_BYTES)
      return setMensagem({ tipo: "erro", texto: "O arquivo excede o limite de 50 MB." });

    setEnviando(true);
    try {
      const corpo = new FormData();
      corpo.append("arquivo", arquivo);
      const res = await fetch(`/exames/${exameId}/anexos`, { method: "POST", body: corpo });
      if (!res.ok) {
        const problema = (await res.json().catch(() => ({}))) as { detail?: string };
        setMensagem({ tipo: "erro", texto: problema.detail ?? "Não foi possível enviar o arquivo." });
        return;
      }
      setMensagem({ tipo: "ok", texto: `${arquivo.name} anexado.` });
      router.refresh();
    } catch {
      setMensagem({ tipo: "erro", texto: "Falha de conexão durante o envio." });
    } finally {
      setEnviando(false);
      if (entrada.current) entrada.current.value = "";
    }
  }

  function remover(anexo: Anexo) {
    if (!confirm(`Remover "${anexo.nomeOriginal}" do exame? O arquivo continua guardado, mas deixa de aparecer aqui.`)) return;
    setMensagem(null);
    iniciarRemocao(async () => {
      const r = await removerAnexo(exameId, anexo.id);
      setMensagem(r.ok ? { tipo: "ok", texto: `${anexo.nomeOriginal} removido.` } : { tipo: "erro", texto: r.erro ?? "Falha." });
    });
  }

  return (
    <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="font-semibold text-texto">Anexos</h2>
          <p className="mt-1 text-xs text-texto-suave">
            Até {MAXIMO_ANEXOS} arquivos PDF, JPG ou PNG, com até 50 MB cada. Não ficam visíveis ao paciente.
          </p>
        </div>
        <span className="text-xs text-texto-suave">
          {anexos.length} de {MAXIMO_ANEXOS}
        </span>
      </div>

      {anexos.length > 0 && (
        <ul className="mt-4 divide-y divide-borda/60 rounded-lg border border-borda">
          {anexos.map((a) => (
            <li key={a.id} className="flex flex-wrap items-center justify-between gap-3 px-4 py-3">
              <div className="min-w-0">
                <a
                  href={`/exames/${exameId}/anexos/${a.id}`}
                  className="block truncate text-sm font-medium text-teal hover:underline"
                  title={`SHA-256 ${a.hashSha256}`}
                >
                  {a.nomeOriginal}
                </a>
                <p className="text-xs text-texto-suave">
                  {formatarTamanho(a.tamanhoBytes)} · enviado em {formatarDataHora(a.enviadoEm)}
                </p>
              </div>
              <Botao variante="fantasma" tamanho="sm" disabled={removendo || enviando} onClick={() => remover(a)}>
                Remover
              </Botao>
            </li>
          ))}
        </ul>
      )}

      <div className="mt-4">
        <input
          ref={entrada}
          type="file"
          accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
          className="hidden"
          onChange={(e) => e.target.files?.[0] && enviar(e.target.files[0])}
        />
        <Botao
          variante="secundaria"
          carregando={enviando}
          disabled={cheio || removendo}
          title={cheio ? "Remova um anexo para enviar outro" : undefined}
          onClick={() => entrada.current?.click()}
        >
          {enviando ? "Enviando…" : "Anexar arquivo"}
        </Botao>
      </div>

      {mensagem && (
        <p
          role={mensagem.tipo === "erro" ? "alert" : "status"}
          className={`mt-3 rounded-lg px-3 py-2 text-sm ${mensagem.tipo === "erro" ? "bg-erro/10 text-erro" : "bg-sucesso/10 text-sucesso"}`}
        >
          {mensagem.texto}
        </p>
      )}
    </section>
  );
}
