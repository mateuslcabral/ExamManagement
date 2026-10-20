"use client";

import Link from "next/link";
import { useActionState, useEffect, useRef, useState, useTransition } from "react";
import type { Cadastro, Exame, ExameCatalogo, TipoMedico } from "@/lib/api";
import { formatarData, formatarDocumento, UFS, valorParaCampo } from "@/lib/formatos";
import { buscarPacientes, salvarExame, type EstadoSalvarExame, type PacienteEncontrado } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Dialogo } from "@/components/ui/dialogo";
import { Selecao } from "@/components/ui/selecao";

const NOME_MEDICO_INTERNO = "Dr. Arsonval Lamounier Junior";

/**
 * Cadastro e edição do exame solicitado. Campos controlados (o React limpa formulários não controlados após o
 * envio). Na edição, o formulário abre somente leitura: "Editar" pede confirmação antes de liberar os campos
 * (pedido do cliente em 17/10/2026). O paciente só é escolhido na criação.
 */
export function FormularioExame({
  exame,
  pacienteInicial,
  catalogo,
  origens,
  destinos,
}: {
  exame?: Exame;
  pacienteInicial?: PacienteEncontrado;
  /** Exames ativos do catálogo (mais o atual do exame, se tiver sido desativado depois). */
  catalogo: ExameCatalogo[];
  origens: Cadastro[];
  destinos: Cadastro[];
}) {
  const [paciente, setPaciente] = useState<PacienteEncontrado | undefined>(
    exame
      ? { id: exame.pacienteId, nome: exame.pacienteNome, tipoDocumento: exame.pacienteTipoDocumento, numeroDocumento: exame.pacienteNumeroDocumento, dataNascimento: "" }
      : pacienteInicial,
  );
  const inicial = () => ({
    exameCatalogoId: exame?.exameCatalogoId ?? "",
    origemId: exame?.origemId ?? "",
    destinoId: exame?.destinoId ?? "",
    tipoMedico: exame?.tipoMedico ?? ("Interno" as TipoMedico),
    nomeMedicoExterno: exame?.tipoMedico === "Externo" ? exame.nomeMedico : "",
    crmMedico: exame?.crmMedico ?? "",
    ufCrm: exame?.ufCrm ?? "",
    preco: exame ? valorParaCampo(exame.preco) : "",
  });
  const [v, setV] = useState(inicial);
  const [editando, setEditando] = useState(!exame);
  const [dialogo, setDialogo] = useState<"editar" | "salvar" | null>(null);
  const [estado, acao, salvando] = useActionState<EstadoSalvarExame, FormData>(salvarExame, {});
  const formRef = useRef<HTMLFormElement>(null);
  const erro = estado.campos ?? {};
  const bloqueado = !editando;

  const selecionado = catalogo.find((c) => c.id === v.exameCatalogoId);
  const set = (campo: keyof ReturnType<typeof inicial>) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setV((a) => ({ ...a, [campo]: e.target.value }));

  function escolherExame(e: React.ChangeEvent<HTMLSelectElement>) {
    const item = catalogo.find((c) => c.id === e.target.value);
    // Preço e destino herdados do catálogo a cada troca de exame; continuam editáveis.
    setV((a) => ({
      ...a,
      exameCatalogoId: e.target.value,
      preco: item ? valorParaCampo(item.precoReferencia) : a.preco,
      destinoId: item?.destinoId ?? a.destinoId,
    }));
  }

  function cancelarEdicao() {
    setV(inicial());
    setEditando(false);
  }

  const origensVisiveis = origens.filter((o) => o.ativo || o.id === exame?.origemId);
  const destinosVisiveis = destinos.filter((d) => d.ativo || d.id === exame?.destinoId);

  return (
    <form
      ref={formRef}
      action={acao}
      onSubmit={(e) => {
        // Na edição, a confirmação vem antes do envio; o diálogo reenvia o formulário via requestSubmit.
        if (exame && dialogo !== "salvar") {
          e.preventDefault();
          setDialogo("salvar");
          return;
        }
        setDialogo(null);
      }}
      className="space-y-6"
    >
      {exame && <input type="hidden" name="id" value={exame.id} />}
      <input type="hidden" name="pacienteId" value={paciente?.id ?? ""} />

      {exame && (
        <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-borda bg-white px-5 py-4 shadow-sm">
          <div>
            <h2 className="font-semibold text-texto">Dados do exame</h2>
            <p className="text-xs text-texto-suave">{editando ? "Edição liberada. Salve ou cancele ao terminar." : "Somente leitura. Clique em Editar para alterar."}</p>
          </div>
          {editando ? (
            <Botao type="button" variante="secundaria" tamanho="sm" onClick={cancelarEdicao}>
              Cancelar edição
            </Botao>
          ) : (
            <Botao type="button" variante="secundaria" tamanho="sm" onClick={() => setDialogo("editar")}>
              Editar
            </Botao>
          )}
        </div>
      )}

      <fieldset disabled={bloqueado} className="space-y-6 disabled:opacity-80">
        <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
          <h2 className="font-semibold text-texto">Paciente</h2>
          {paciente ? (
            <div className="mt-3 flex flex-wrap items-center justify-between gap-3 rounded-lg bg-fundo px-4 py-3">
              <div>
                <p className="font-medium text-texto">{paciente.nome}</p>
                <p className="text-xs text-texto-suave">
                  {paciente.tipoDocumento === "Cpf" ? "CPF" : "Passaporte"} {formatarDocumento(paciente.tipoDocumento, paciente.numeroDocumento)}
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
                disabled={bloqueado || (!!exame && exame.estado !== "AguardandoAmostra")}
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
              {exame && (bloqueado || exame.estado !== "AguardandoAmostra") && <input type="hidden" name="exameCatalogoId" value={v.exameCatalogoId} />}
              <p className="mt-1 text-xs text-texto-suave">
                {selecionado
                  ? `Entrega ao paciente em ${selecionado.prazoDias} dias corridos após o acolhimento da amostra.`
                  : "A data de liberação só existe depois do acolhimento da amostra."}
              </p>
            </div>

            <Selecao label="Origem" name="origemId" required value={v.origemId} onChange={set("origemId")} erro={erro.origemId}>
              <option value="">Selecione…</option>
              {origensVisiveis.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.nome}
                  {!o.ativo && " (inativa)"}
                </option>
              ))}
            </Selecao>
            <Selecao label="Destino (opcional)" name="destinoId" value={v.destinoId} onChange={set("destinoId")} erro={erro.destinoId}>
              <option value="">Sem destino</option>
              {destinosVisiveis.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.nome}
                  {!d.ativo && " (inativo)"}
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
              dica={selecionado ? `Herdado do catálogo; mínimo ${valorParaCampo(selecionado.precoLimite)} após desconto.` : undefined}
              erro={erro.preco}
            />
          </div>
        </section>

        <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
          <h2 className="font-semibold text-texto">Médico solicitante</h2>
          <p className="mt-1 text-xs text-texto-suave">Define o modelo da mensagem enviada quando o laudo for disponibilizado.</p>
          <div className="mt-4 space-y-3">
            {(["Interno", "Externo"] as TipoMedico[]).map((tipo) => (
              <label key={tipo} className="flex items-center gap-2 text-sm text-texto">
                <input type="radio" name="tipoMedico" value={tipo} checked={v.tipoMedico === tipo} onChange={set("tipoMedico")} className="size-4 accent-primaria" />
                {tipo === "Interno" ? `Interno — ${NOME_MEDICO_INTERNO}` : "Externo"}
              </label>
            ))}
            {v.tipoMedico === "Externo" && (
              <div className="grid gap-3 sm:grid-cols-[1fr_140px_100px]">
                <Campo label="Nome do médico externo" name="nomeMedicoExterno" required autoComplete="off" maxLength={200} value={v.nomeMedicoExterno} onChange={set("nomeMedicoExterno")} erro={erro.nomeMedicoExterno} />
                <Campo label="CRM" name="crmMedico" autoComplete="off" maxLength={20} value={v.crmMedico} onChange={set("crmMedico")} erro={erro.crmMedico} />
                <Selecao label="UF" name="ufCrm" value={v.ufCrm} onChange={set("ufCrm")} erro={erro.ufCrm}>
                  <option value="">—</option>
                  {UFS.map((uf) => (
                    <option key={uf} value={uf}>
                      {uf}
                    </option>
                  ))}
                </Selecao>
              </div>
            )}
          </div>
        </section>
      </fieldset>

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      {(editando || !exame) && (
        <div className="flex flex-wrap justify-end gap-3">
          {!exame && (
            <Link
              href={paciente ? `/fluxos?pacienteId=${paciente.id}` : "/fluxos"}
              className="inline-flex items-center rounded-pill border border-primaria bg-white px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
            >
              Cancelar
            </Link>
          )}
          <Botao type="submit" carregando={salvando}>
            {exame ? "Salvar alterações" : "Cadastrar exame"}
          </Botao>
        </div>
      )}

      <Dialogo
        aberto={dialogo === "editar"}
        titulo="Liberar edição do exame?"
        confirmar="Editar"
        onCancelar={() => setDialogo(null)}
        onConfirmar={() => {
          setDialogo(null);
          setEditando(true);
        }}
      >
        Os dados cadastrados ficam protegidos contra alteração acidental. Confirme para liberar os campos.
      </Dialogo>
      <Dialogo
        aberto={dialogo === "salvar"}
        titulo="Salvar as alterações do exame?"
        confirmar="Salvar"
        carregando={salvando}
        onCancelar={() => setDialogo(null)}
        onConfirmar={() => formRef.current?.requestSubmit()}
      >
        As alterações valem a partir de agora. Preço e destino de exames já acolhidos continuam valendo para a previsão gravada.
      </Dialogo>
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
              <button type="button" onClick={() => onEscolher(p)} className="w-full px-4 py-2.5 text-left text-sm transition hover:bg-fundo">
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
