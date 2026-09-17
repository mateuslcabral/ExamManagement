"use client";

import { useRouter } from "next/navigation";
import { useRef, useState, useTransition } from "react";
import type { Exame, TipoEtapaLaudo } from "@/lib/api";
import { ETAPAS_LAUDO, formatarDataHora, formatarTamanho } from "@/lib/formatos";
import { disponibilizarLaudo } from "../actions";
import { Botao } from "@/components/ui/botao";
import { Etiqueta } from "@/components/ui/etiqueta";

const LIMITE_BYTES = 50 * 1024 * 1024; // P1

type Mensagem = { tipo: "ok" | "erro"; texto: string };

/** As três etapas do laudo, com registro/substituição do PDF, e o botão manual de disponibilizar (D9, D10, Q20). */
export function PainelLaudo({ exame }: { exame: Exame }) {
  const router = useRouter();
  const [mensagem, setMensagem] = useState<Mensagem | null>(null);
  const [disponibilizando, iniciar] = useTransition();
  const disponibilizado = exame.estado === "Disponibilizado";
  const semAmostra = exame.estado === "AguardandoAmostra";

  function disponibilizar() {
    if (!confirm("Disponibilizar o laudo revisado ao paciente? A data de liberação é gravada e as mensagens são disparadas.")) return;
    setMensagem(null);
    iniciar(async () => {
      const r = await disponibilizarLaudo(exame.id);
      setMensagem(r.ok ? { tipo: "ok", texto: "Laudo disponibilizado ao paciente." } : { tipo: "erro", texto: r.erro ?? "Falha." });
    });
  }

  return (
    <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="font-semibold text-texto">Fluxo do laudo</h2>
          <p className="mt-1 text-xs text-texto-suave">
            Somente PDF, gravado sem alteração para preservar a assinatura. Cada etapa registra a data no primeiro envio;
            o arquivo pode ser substituído sem mudar a data.
          </p>
        </div>
        {disponibilizado && exame.dataLiberacaoEfetiva && (
          <div className="text-right">
            <p className="text-xs text-texto-suave">Liberado ao paciente em</p>
            <p className="text-lg font-semibold text-sucesso">{formatarDataHora(exame.dataLiberacaoEfetiva)}</p>
          </div>
        )}
      </div>

      {semAmostra && (
        <p className="mt-4 rounded-lg bg-fundo px-3 py-2 text-sm text-texto-suave">
          As etapas do laudo começam depois do acolhimento da amostra.
        </p>
      )}

      <ol className="mt-4 space-y-3">
        {ETAPAS_LAUDO.map((def) => {
          const etapa = exame.etapas.find((e) => e.tipo === def.tipo);
          const proxima = !etapa && exame.estado === def.estadoAnterior;
          return (
            <li
              key={def.tipo}
              className={`rounded-xl border p-4 ${
                etapa ? "border-sucesso/30 bg-sucesso/5" : proxima ? "border-primaria/40" : "border-borda bg-fundo/60"
              }`}
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="text-sm font-semibold text-texto">
                  <span className="mr-2 inline-flex size-6 items-center justify-center rounded-full bg-primaria/10 text-xs text-primaria">
                    {def.numero}
                  </span>
                  {def.rotulo}
                </p>
                {etapa ? (
                  <Etiqueta cor="verde">Registrada em {formatarDataHora(etapa.data)}</Etiqueta>
                ) : proxima ? (
                  <Etiqueta cor="teal">Próxima etapa</Etiqueta>
                ) : (
                  <Etiqueta cor="cinza">Aguardando</Etiqueta>
                )}
              </div>

              {etapa && (
                <p className="mt-2 text-sm">
                  <a
                    href={`/exames/${exame.id}/laudo/${def.tipo}`}
                    className="font-medium text-teal hover:underline"
                    title={`SHA-256 ${etapa.hashSha256}`}
                  >
                    {etapa.nomeOriginal}
                  </a>
                  <span className="ml-2 text-xs text-texto-suave">
                    {formatarTamanho(etapa.tamanhoBytes)}
                    {etapa.substituicoes > 0 &&
                      etapa.substituidoEm &&
                      ` · substituído ${etapa.substituicoes}× (último em ${formatarDataHora(etapa.substituidoEm)})`}
                  </span>
                </p>
              )}

              {(etapa || proxima) && (
                <EnvioEtapa
                  exameId={exame.id}
                  etapa={def.tipo}
                  substituir={!!etapa}
                  avisoSubstituicao={disponibilizado && def.tipo === "LaudoRevisado"}
                  onResultado={(m) => {
                    setMensagem(m);
                    if (m.tipo === "ok") router.refresh();
                  }}
                />
              )}
            </li>
          );
        })}
      </ol>

      <div className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-borda/60 pt-4">
        <div className="text-sm">
          <p className="font-medium text-texto">Disponibilização ao paciente</p>
          <p className="text-xs text-texto-suave">
            {disponibilizado
              ? `E-mail e WhatsApp disparados no modelo de médico ${exame.tipoMedico === "Interno" ? "interno" : "externo"}.`
              : exame.estado === "LaudoRevisado"
                ? "Ato manual: torna o laudo revisado visível no portal e dispara e-mail e WhatsApp ao paciente."
                : "Disponível após o registro do laudo revisado."}
          </p>
        </div>
        {exame.estado === "LaudoRevisado" && (
          <Botao carregando={disponibilizando} onClick={disponibilizar}>
            Disponibilizar ao paciente
          </Botao>
        )}
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

/** Upload por fetch ao route handler (server actions limitam a 1 MB), em stream até a API. */
function EnvioEtapa({
  exameId,
  etapa,
  substituir,
  avisoSubstituicao,
  onResultado,
}: {
  exameId: string;
  etapa: TipoEtapaLaudo;
  substituir: boolean;
  avisoSubstituicao: boolean;
  onResultado: (m: Mensagem) => void;
}) {
  const entrada = useRef<HTMLInputElement>(null);
  const [enviando, setEnviando] = useState(false);

  async function enviar(arquivo: File) {
    if (arquivo.type !== "application/pdf") return onResultado({ tipo: "erro", texto: "O laudo deve ser um arquivo PDF." });
    if (arquivo.size > LIMITE_BYTES) return onResultado({ tipo: "erro", texto: "O arquivo excede o limite de 50 MB." });
    if (
      avisoSubstituicao &&
      !confirm("O laudo já foi disponibilizado. A substituição troca o arquivo que o paciente vê, sem aviso e sem histórico. Continuar?")
    )
      return;

    setEnviando(true);
    try {
      const corpo = new FormData();
      corpo.append("arquivo", arquivo);
      const res = await fetch(`/exames/${exameId}/laudo/${etapa}`, { method: "POST", body: corpo });
      if (!res.ok) {
        const problema = (await res.json().catch(() => ({}))) as { detail?: string };
        onResultado({ tipo: "erro", texto: problema.detail ?? "Não foi possível enviar o arquivo." });
        return;
      }
      onResultado({ tipo: "ok", texto: substituir ? `${arquivo.name} substituiu o arquivo da etapa.` : `${arquivo.name} registrado.` });
    } catch {
      onResultado({ tipo: "erro", texto: "Falha de conexão durante o envio." });
    } finally {
      setEnviando(false);
      if (entrada.current) entrada.current.value = "";
    }
  }

  return (
    <div className="mt-3">
      <input
        ref={entrada}
        type="file"
        accept=".pdf,application/pdf"
        className="hidden"
        onChange={(e) => e.target.files?.[0] && enviar(e.target.files[0])}
      />
      <Botao
        variante={substituir ? "fantasma" : "primaria"}
        tamanho="sm"
        carregando={enviando}
        onClick={() => entrada.current?.click()}
      >
        {enviando ? "Enviando…" : substituir ? "Substituir arquivo" : "Registrar etapa (PDF)"}
      </Botao>
    </div>
  );
}
