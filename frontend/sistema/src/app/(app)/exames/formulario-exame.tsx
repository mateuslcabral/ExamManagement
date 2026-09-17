"use client";

import Link from "next/link";
import { useActionState, useEffect, useState, useTransition } from "react";
import type { Exame, ExameCatalogo, OrigemExame, TipoMedico } from "@/lib/api";
import { formatarData, formatarDocumento, ROTULOS_ORIGEM, valorParaCampo } from "@/lib/formatos";
import { buscarPacientes, salvarExame, type EstadoSalvarExame, type PacienteEncontrado } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Selecao } from "@/components/ui/selecao";

const NOME_MEDICO_INTERNO = "Dr. Arsonval Lamounier Junior";

/**
 * Cadastro e edição do exame solicitado. Campos controlados (o React limpa formulários não controlados após o
 * envio). O paciente só é escolhido na criação: exame no paciente errado se exclui e se cadastra de novo.
 */
export function FormularioExame({
  exame,
  pacienteInicial,
  catalogo,
}: {
  exame?: Exame;
  pacienteInicial?: PacienteEncontrado;
  /** Exames ativos do catálogo (mais o atual do exame, se tiver sido desativado depois). */
  catalogo: ExameCatalogo[];
}) {
  const [paciente, setPaciente] = useState<PacienteEncontrado | undefined>(
    exame
      ? {
          id: exame.pacienteId,
          nome: exame.pacienteNome,
          tipoDocumento: exame.pacienteTipoDocumento,
          numeroDocumento: exame.pacienteNumeroDocumento,
          dataNascimento: "",
        }
      : pacienteInicial,
  );
  const [v, setV] = useState({
    exameCatalogoId: exame?.exameCatalogoId ?? "",
    origem: exame?.origem ?? ("" as OrigemExame | ""),
    destino: exame?.destino ?? "",
    tipoMedico: exame?.tipoMedico ?? ("Interno" as TipoMedico),
    nomeMedicoExterno: exame?.tipoMedico === "Externo" ? exame.nomeMedico : "",
    preco: exame ? valorParaCampo(exame.preco) : "",
  });
  const [estado, acao, salvando] = useActionState<EstadoSalvarExame, FormData>(salvarExame, {});
  const erro = estado.campos ?? {};

  const selecionado = catalogo.find((c) => c.id === v.exameCatalogoId);
  const set = (campo: keyof typeof v) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setV((a) => ({ ...a, [campo]: e.target.value }));

  function escolherExame(e: React.ChangeEvent<HTMLSelectElement>) {
    const item = catalogo.find((c) => c.id === e.target.value);
    // Preço herdado do catálogo a cada troca de exame; continua editável.
    setV((a) => ({ ...a, exameCatalogoId: e.target.value, preco: item ? valorParaCampo(item.precoReferencia) : a.preco }));
  }

  return (
    <form action={acao} className="space-y-6">
      {exame && <input type="hidden" name="id" value={exame.id} />}
      <input type="hidden" name="pacienteId" value={paciente?.id ?? ""} />

      <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">Paciente</h2>
        {paciente ? (
          <div className="mt-3 flex flex-wrap items-center justify-between gap-3 rounded-lg bg-fundo px-4 py-3">
            <div>
              <p className="font-medium text-texto">{paciente.nome}</p>
              <p className="text-xs text-texto-suave">
                {paciente.tipoDocumento === "Cpf" ? "CPF" : "Passaporte"}{" "}
                {formatarDocumento(paciente.tipoDocumento, paciente.numeroDocumento)}
                {paciente.dataNascimento && ` · nascimento ${formatarData(paciente.dataNascimento)}`}
              </p>
            </div>
            {!exame && (
              <Botao type="button" variante="fantasma" tamanho="sm" onClick={() => setPaciente(undefined)}>
                Trocar
              </Botao>
            )}
          </div>
        ) : (
          <BuscaPaciente onEscolher={setPaciente} erro={erro.pacienteId} />
        )}
      </section>

      <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">Exame</h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <Selecao
              label="Exame do catálogo"
              name="exameCatalogoId"
              required
              value={v.exameCatalogoId}
              onChange={escolherExame}
              disabled={!!exame && exame.estado !== "AguardandoAmostra"}
              erro={erro.exameCatalogoId}
            >
              <option value="">Selecione…</option>
              {catalogo.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nome}
                  {!c.ativo && " (inativo)"}
                </option>
              ))}
            </Selecao>
            {/* select desabilitado não entra no envio */}
            {exame && exame.estado !== "AguardandoAmostra" && (
              <input type="hidden" name="exameCatalogoId" value={v.exameCatalogoId} />
            )}
            <p className="mt-1 text-xs text-texto-suave">
              {selecionado
                ? `Execução em ${selecionado.prazoExecucaoDias} dias; entrega ao paciente em ${selecionado.prazoEntregaDias} dias após o acolhimento da amostra.`
                : "A data de liberação só existe depois do acolhimento da amostra."}
            </p>
          </div>

          <Selecao label="Origem" name="origem" required value={v.origem} onChange={set("origem")} erro={erro.origem}>
            <option value="">Selecione…</option>
            {(Object.keys(ROTULOS_ORIGEM) as OrigemExame[]).map((o) => (
              <option key={o} value={o}>
                {ROTULOS_ORIGEM[o]}
              </option>
            ))}
          </Selecao>
          <Campo
            label="Preço (R$)"
            name="preco"
            inputMode="decimal"
            required
            autoComplete="off"
            placeholder="0,00"
            value={v.preco}
            onChange={set("preco")}
            dica={selecionado ? "Herdado do catálogo; pode ser ajustado." : undefined}
            erro={erro.preco}
          />
          <Campo
            label="Destino (opcional)"
            name="destino"
            autoComplete="off"
            maxLength={200}
            className="sm:col-span-2"
            value={v.destino}
            onChange={set("destino")}
            erro={erro.destino}
          />
        </div>
      </section>

      <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">Médico solicitante</h2>
        <p className="mt-1 text-xs text-texto-suave">Define o modelo da mensagem enviada quando o laudo for disponibilizado.</p>
        <div className="mt-4 space-y-3">
          {(["Interno", "Externo"] as TipoMedico[]).map((tipo) => (
            <label key={tipo} className="flex items-center gap-2 text-sm text-texto">
              <input
                type="radio"
                name="tipoMedico"
                value={tipo}
                checked={v.tipoMedico === tipo}
                onChange={set("tipoMedico")}
                className="size-4 accent-primaria"
              />
              {tipo === "Interno" ? `Interno — ${NOME_MEDICO_INTERNO}` : "Externo"}
            </label>
          ))}
          {v.tipoMedico === "Externo" && (
            <Campo
              label="Nome do médico externo"
              name="nomeMedicoExterno"
              required
              autoComplete="off"
              maxLength={200}
              className="sm:max-w-md"
              value={v.nomeMedicoExterno}
              onChange={set("nomeMedicoExterno")}
              erro={erro.nomeMedicoExterno}
            />
          )}
        </div>
      </section>

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      <div className="flex flex-wrap justify-end gap-3">
        <Link
          href={exame ? "/exames" : paciente ? `/exames?pacienteId=${paciente.id}` : "/exames"}
          className="inline-flex items-center rounded-pill border border-primaria bg-white px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
        >
          {exame ? "Voltar" : "Cancelar"}
        </Link>
        <Botao type="submit" carregando={salvando}>
          {exame ? "Salvar alterações" : "Cadastrar exame"}
        </Botao>
      </div>
    </form>
  );
}

function BuscaPaciente({ onEscolher, erro }: { onEscolher: (p: PacienteEncontrado) => void; erro?: string }) {
  const [termo, setTermo] = useState("");
  const [resultados, setResultados] = useState<PacienteEncontrado[] | null>(null);
  const [buscando, iniciar] = useTransition();

  useEffect(() => {
    if (termo.trim().length < 2) return;
    const t = setTimeout(() => iniciar(async () => setResultados(await buscarPacientes(termo))), 300);
    return () => clearTimeout(t);
  }, [termo]);

  const mostrarResultados = termo.trim().length >= 2 && resultados !== null;

  return (
    <div className="mt-3">
      <Campo
        label="Buscar paciente"
        name="buscaPaciente"
        type="search"
        autoComplete="off"
        placeholder="Nome, CPF ou passaporte"
        value={termo}
        onChange={(e) => setTermo(e.target.value)}
        dica={buscando ? "Buscando…" : "Digite ao menos 2 caracteres."}
        erro={erro}
      />
      {mostrarResultados && (
        <ul className="mt-2 divide-y divide-borda/60 rounded-lg border border-borda">
          {resultados.length === 0 && (
            <li className="px-4 py-3 text-sm text-texto-suave">
              Nenhum paciente encontrado.{" "}
              <Link href="/pacientes/novo" className="font-semibold text-teal hover:underline">
                Cadastrar paciente
              </Link>
            </li>
          )}
          {resultados.map((p) => (
            <li key={p.id}>
              <button
                type="button"
                onClick={() => onEscolher(p)}
                className="w-full px-4 py-2.5 text-left text-sm transition hover:bg-fundo"
              >
                <span className="font-medium text-texto">{p.nome}</span>
                <span className="ml-2 text-xs text-texto-suave">
                  {formatarDocumento(p.tipoDocumento, p.numeroDocumento)} · {formatarData(p.dataNascimento)}
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
