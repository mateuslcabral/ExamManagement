"use client";

import Link from "next/link";
import { useActionState, useState } from "react";
import type { Exame, ExameCatalogo, PacienteResumo } from "@/lib/api";
import { NOME_MEDICO_INTERNO, ORIGENS } from "@/lib/exames";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Selecao } from "@/components/ui/selecao";
import { atualizarExame, criarExame, type EstadoExame } from "./actions";

type Props = {
  exame?: Exame;
  pacientes?: PacienteResumo[];
  catalogo?: ExameCatalogo[];
  pacienteFixo?: { id: string; nome: string };
};

export function FormularioExame({ exame, pacientes = [], catalogo = [], pacienteFixo }: Props) {
  const acaoServidor = exame ? atualizarExame.bind(null, exame.id) : criarExame;
  const [estado, acao, pendente] = useActionState<EstadoExame, FormData>(acaoServidor, {});
  const v = (campo: keyof NonNullable<EstadoExame["valores"]>, original = "") => estado.valores?.[campo] ?? original;

  const [tipoMedico, setTipoMedico] = useState(v("tipoMedico", exame?.tipoMedico ?? "Interno"));
  const [catalogoId, setCatalogoId] = useState(v("exameCatalogoId", exame?.exameCatalogoId ?? ""));
  const itemCatalogo = catalogo.find((c) => c.id === catalogoId);
  const precoPadrao = exame ? exame.preco.toFixed(2).replace(".", ",") : "";

  return (
    <form action={acao} className="space-y-6">
      {!exame && (
        <fieldset className="space-y-4">
          <legend className="text-sm font-semibold uppercase tracking-wider text-texto-suave">Vínculo</legend>
          {pacienteFixo ? (
            <div>
              <input type="hidden" name="pacienteId" value={pacienteFixo.id} />
              <p className="text-sm text-texto-suave">Paciente</p>
              <p className="font-medium text-texto">{pacienteFixo.nome}</p>
            </div>
          ) : (
            <Selecao label="Paciente" name="pacienteId" required defaultValue={v("pacienteId")} erro={estado.campos?.pacienteId}>
              <option value="">Selecione…</option>
              {pacientes.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nome} — {p.numeroDocumentoFormatado}
                </option>
              ))}
            </Selecao>
          )}
          <Selecao
            label="Exame do catálogo"
            name="exameCatalogoId"
            required
            value={catalogoId}
            onChange={(e) => setCatalogoId(e.target.value)}
            erro={estado.campos?.exameCatalogoId}
          >
            <option value="">Selecione…</option>
            {catalogo.map((c) => (
              <option key={c.id} value={c.id}>
                {c.nome} — {c.prazoExecucaoDias} dias
              </option>
            ))}
          </Selecao>
          {itemCatalogo && (
            <p className="text-xs text-texto-suave">
              Prazo de execução {itemCatalogo.prazoExecucaoDias} dias (+{itemCatalogo.diasRevisao} de revisão = {itemCatalogo.prazoTotalDias} dias).
              A data prevista só existe após o acolhimento da amostra.
            </p>
          )}
        </fieldset>
      )}

      <fieldset className="space-y-4">
        <legend className="text-sm font-semibold uppercase tracking-wider text-texto-suave">Solicitação</legend>
        <div className="grid gap-4 sm:grid-cols-2">
          <Selecao label="Origem" name="origem" required defaultValue={v("origem", exame?.origem ?? "Cligen")} erro={estado.campos?.origem}>
            {(Object.keys(ORIGENS) as (keyof typeof ORIGENS)[]).map((k) => (
              <option key={k} value={k}>
                {ORIGENS[k]}
              </option>
            ))}
          </Selecao>
          <Campo
            label="Destino"
            name="destino"
            placeholder="Laboratório executor (texto livre)"
            defaultValue={v("destino", exame?.destino ?? "")}
            erro={estado.campos?.destino}
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <Selecao
            label="Médico solicitante"
            name="tipoMedico"
            required
            value={tipoMedico}
            onChange={(e) => setTipoMedico(e.target.value)}
            erro={estado.campos?.tipoMedico}
          >
            <option value="Interno">Interno — {NOME_MEDICO_INTERNO}</option>
            <option value="Externo">Externo</option>
          </Selecao>
          {tipoMedico === "Externo" ? (
            <Campo
              label="Nome do médico externo"
              name="nomeMedico"
              required
              defaultValue={v("nomeMedico", exame?.tipoMedico === "Externo" ? exame.nomeMedico : "")}
              erro={estado.campos?.nomeMedico}
            />
          ) : (
            <Campo
              label="Preço (R$)"
              name="preco"
              inputMode="decimal"
              placeholder={itemCatalogo ? itemCatalogo.precoReferencia.toFixed(2).replace(".", ",") : "Herdado do catálogo"}
              defaultValue={v("preco", precoPadrao)}
              erro={estado.campos?.preco}
              dica="Vazio = preço de referência do catálogo"
            />
          )}
        </div>
        {tipoMedico === "Externo" && (
          <Campo
            label="Preço (R$)"
            name="preco"
            inputMode="decimal"
            placeholder={itemCatalogo ? itemCatalogo.precoReferencia.toFixed(2).replace(".", ",") : "Herdado do catálogo"}
            defaultValue={v("preco", precoPadrao)}
            erro={estado.campos?.preco}
            dica="Vazio = preço de referência do catálogo"
            className="sm:w-1/2"
          />
        )}
      </fieldset>

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      <div className="flex items-center justify-end gap-3">
        <Link
          href={exame ? `/exames/${exame.id}` : pacienteFixo ? `/pacientes/${pacienteFixo.id}` : "/exames"}
          className="text-sm font-medium text-texto-suave hover:text-texto"
        >
          Cancelar
        </Link>
        <Botao type="submit" carregando={pendente}>
          {exame ? "Salvar alterações" : "Cadastrar exame"}
        </Botao>
      </div>
    </form>
  );
}
