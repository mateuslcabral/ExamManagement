"use client";

import { useActionState, useState, useTransition } from "react";
import type { Exame } from "@/lib/api";
import { ESTADOS, ETAPAS, type TipoEtapaLaudo } from "@/lib/exames";
import { formatarData, formatarTamanho } from "@/lib/idade";
import { Botao } from "@/components/ui/botao";
import { Etiqueta } from "@/components/ui/etiqueta";
import { disponibilizarLaudo, enviarEtapaLaudo, type EstadoAcao } from "../actions";

const ORDEM: Record<string, number> = {
  AguardandoAmostra: 1,
  AmostraAcolhida: 2,
  LaudoParceiroPronto: 3,
  LaudoCligenParaRevisao: 4,
  LaudoRevisado: 5,
  DisponibilizadoAoPaciente: 6,
};

/** As três etapas do laudo com upload/substituição e o botão manual de disponibilizar — P32–P34. */
export function PainelLaudo({ exame }: { exame: Exame }) {
  const ordemAtual = ORDEM[exame.estado];
  const [erro, setErro] = useState<string | null>(null);
  const [pendente, iniciar] = useTransition();

  return (
    <div>
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="font-semibold text-texto">Fluxo do laudo</h2>
        <p className="text-xs text-texto-suave">Somente PDF · arquivo gravado sem alteração (preserva a assinatura)</p>
      </div>

      <ol className="mt-3 space-y-3">
        {ETAPAS.map((def, i) => {
          const etapa = exame.etapas.find((e) => e.tipo === def.tipo);
          const numero = i + 3;
          const ehProxima = !exame.excluido && !etapa && exame.estado === def.estadoAnterior;
          const bloqueada = !etapa && !ehProxima;
          return (
            <li
              key={def.tipo}
              className={`rounded-xl border p-4 ${etapa ? "border-sucesso/30 bg-sucesso/5" : ehProxima ? "border-primaria/40 bg-white" : "border-borda bg-fundo/60"}`}
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="text-sm font-semibold text-texto">
                  <span className="mr-2 inline-flex size-6 items-center justify-center rounded-full bg-primaria/10 text-xs text-primaria">
                    {numero}
                  </span>
                  {def.rotulo}
                </p>
                {etapa ? (
                  <Etiqueta cor="verde">Registrada em {formatarData(etapa.data)}</Etiqueta>
                ) : ehProxima ? (
                  <Etiqueta cor="teal">Próxima etapa</Etiqueta>
                ) : (
                  <Etiqueta cor="cinza">Aguardando</Etiqueta>
                )}
              </div>

              {etapa && (
                <p className="mt-2 text-sm">
                  <a href={`/api/laudos/${exame.id}/${def.tipo}`} target="_blank" rel="noopener" className="font-medium text-primaria hover:underline">
                    {etapa.nomeOriginal}
                  </a>
                  <span className="ml-2 text-xs text-texto-suave">
                    {formatarTamanho(etapa.tamanhoBytes)}
                    {etapa.substituicoes > 0 && ` · substituído ${etapa.substituicoes}× (último em ${formatarData(etapa.arquivoSubstituidoEm ?? "")})`}
                  </span>
                </p>
              )}

              {!exame.excluido && !bloqueada && <FormularioEtapa exameId={exame.id} etapa={def.tipo} substituir={!!etapa} />}
            </li>
          );
        })}
      </ol>

      <div className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-borda/60 pt-4">
        <div className="text-sm">
          <p className="font-medium text-texto">Disponibilização ao paciente</p>
          <p className="text-xs text-texto-suave">
            {exame.estado === "DisponibilizadoAoPaciente"
              ? `Liberado em ${formatarData(exame.dataLiberacaoEfetiva ?? "")} — e-mail e WhatsApp enviados no modelo ${exame.tipoMedico === "Interno" ? "interno" : "externo"}.`
              : ordemAtual < 5
                ? "Disponível após o registro do laudo revisado."
                : "Ato manual: torna o laudo revisado visível no portal e dispara e-mail e WhatsApp ao paciente."}
          </p>
          {erro && (
            <p role="alert" className="mt-1 text-xs text-erro">
              {erro}
            </p>
          )}
        </div>
        {exame.estado === "LaudoRevisado" && !exame.excluido && (
          <Botao
            carregando={pendente}
            onClick={() => {
              setErro(null);
              iniciar(async () => {
                const r = await disponibilizarLaudo(exame.id);
                if (!r.ok) setErro(r.erro ?? "Falha.");
              });
            }}
          >
            Disponibilizar ao paciente
          </Botao>
        )}
        {exame.estado === "DisponibilizadoAoPaciente" && <Etiqueta cor="verde">{ESTADOS[exame.estado]}</Etiqueta>}
      </div>
    </div>
  );
}

function FormularioEtapa({ exameId, etapa, substituir }: { exameId: string; etapa: TipoEtapaLaudo; substituir: boolean }) {
  const [estado, acao, pendente] = useActionState<EstadoAcao, FormData>(enviarEtapaLaudo.bind(null, exameId, etapa), {});
  const [aberto, setAberto] = useState(!substituir);

  if (substituir && !aberto) {
    return (
      <div className="mt-2">
        <Botao variante="fantasma" tamanho="sm" onClick={() => setAberto(true)}>
          Substituir arquivo
        </Botao>
      </div>
    );
  }

  return (
    <form key={estado.sucesso ?? ""} action={acao} className="mt-3 flex flex-wrap items-center gap-3">
      <input
        type="file"
        name="arquivo"
        required
        accept=".pdf,application/pdf"
        className="text-sm text-texto file:mr-3 file:rounded-pill file:border file:border-borda file:bg-white file:px-3 file:py-1.5 file:text-xs file:font-semibold file:text-primaria"
      />
      <Botao type="submit" variante={substituir ? "secundaria" : "primaria"} tamanho="sm" carregando={pendente}>
        {substituir ? "Enviar substituto" : "Registrar etapa"}
      </Botao>
      {substituir && (
        <Botao type="button" variante="fantasma" tamanho="sm" onClick={() => setAberto(false)} disabled={pendente}>
          Cancelar
        </Botao>
      )}
      {(estado.erro || estado.sucesso) && (
        <p role={estado.erro ? "alert" : "status"} className={`text-xs ${estado.erro ? "text-erro" : "text-sucesso"}`}>
          {estado.erro ?? estado.sucesso}
        </p>
      )}
    </form>
  );
}
