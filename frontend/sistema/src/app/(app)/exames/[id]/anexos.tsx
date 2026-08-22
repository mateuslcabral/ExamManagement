"use client";

import { useActionState, useEffect, useRef, useState, useTransition } from "react";
import type { Exame } from "@/lib/api";
import { formatarData, formatarTamanho } from "@/lib/idade";
import { Botao } from "@/components/ui/botao";
import { enviarAnexo, removerAnexo, type EstadoAnexo } from "../actions";

const MAXIMO = 3;

export function Anexos({ exame }: { exame: Exame }) {
  const [estado, acao, pendente] = useActionState<EstadoAnexo, FormData>(enviarAnexo.bind(null, exame.id), {});
  const [removendo, iniciarRemocao] = useTransition();
  const [erroRemocao, setErroRemocao] = useState<string | null>(null);
  const formRef = useRef<HTMLFormElement>(null);

  useEffect(() => {
    if (estado.sucesso) formRef.current?.reset();
  }, [estado]);

  const podeEnviar = !exame.excluido && exame.anexos.length < MAXIMO;

  return (
    <div>
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="font-semibold text-texto">Anexos</h2>
        <p className="text-xs text-texto-suave">
          {exame.anexos.length}/{MAXIMO} · PDF, JPG ou PNG até 50 MB · não visíveis ao paciente
        </p>
      </div>

      {exame.anexos.length === 0 ? (
        <p className="mt-3 text-sm text-texto-suave">Nenhum anexo.</p>
      ) : (
        <ul className="mt-3 divide-y divide-borda/60">
          {exame.anexos.map((a) => (
            <li key={a.id} className="flex flex-wrap items-center justify-between gap-3 py-2 text-sm">
              <div>
                <a
                  href={`/api/anexos/${exame.id}/${a.id}`}
                  target="_blank"
                  rel="noopener"
                  className="font-medium text-primaria hover:underline"
                >
                  {a.nomeOriginal}
                </a>
                <p className="text-xs text-texto-suave">
                  {formatarTamanho(a.tamanhoBytes)} · enviado em {formatarData(a.enviadoEm)}
                </p>
              </div>
              {!exame.excluido && (
                <Botao
                  variante="fantasma"
                  tamanho="sm"
                  disabled={removendo}
                  onClick={() => {
                    setErroRemocao(null);
                    iniciarRemocao(async () => {
                      const r = await removerAnexo(exame.id, a.id);
                      if (!r.ok) setErroRemocao(r.erro ?? "Falha ao remover.");
                    });
                  }}
                >
                  Remover
                </Botao>
              )}
            </li>
          ))}
        </ul>
      )}

      {erroRemocao && (
        <p role="alert" className="mt-2 text-xs text-erro">
          {erroRemocao}
        </p>
      )}

      {podeEnviar && (
        <form ref={formRef} action={acao} className="mt-4 flex flex-wrap items-center gap-3 border-t border-borda/60 pt-4">
          <input
            type="file"
            name="arquivo"
            required
            accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
            className="text-sm text-texto file:mr-3 file:rounded-pill file:border file:border-borda file:bg-white file:px-3 file:py-1.5 file:text-xs file:font-semibold file:text-primaria"
          />
          <Botao type="submit" variante="secundaria" tamanho="sm" carregando={pendente}>
            Enviar anexo
          </Botao>
          {(estado.erro || estado.sucesso) && (
            <p role={estado.erro ? "alert" : "status"} className={`text-xs ${estado.erro ? "text-erro" : "text-sucesso"}`}>
              {estado.erro ?? estado.sucesso}
            </p>
          )}
        </form>
      )}
    </div>
  );
}
