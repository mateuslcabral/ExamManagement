import Link from "next/link";
import { notFound } from "next/navigation";
import { api, ApiError, type ExameResumo, type Paciente } from "@/lib/api";
import { formatarData, formatarTelefone } from "@/lib/idade";
import { Etiqueta } from "@/components/ui/etiqueta";
import { FormularioPaciente } from "../formulario-paciente";
import { AcoesPaciente } from "./acoes-paciente";
import { TabelaExames } from "../../exames/tabela-exames";

export const metadata = { title: "Paciente — Cligen" };

type Params = { id: string };
type Busca = { editar?: string; criado?: string; salvo?: string };

export default async function PaginaPaciente({
  params,
  searchParams,
}: {
  params: Promise<Params>;
  searchParams: Promise<Busca>;
}) {
  const [{ id }, { editar, criado, salvo }] = await Promise.all([params, searchParams]);

  let paciente: Paciente;
  try {
    paciente = await api<Paciente>(`/api/pacientes/${id}`);
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) notFound();
    throw e;
  }
  const exames = await api<ExameResumo[]>(`/api/pacientes/${id}/exames?incluirExcluidos=true`);

  const emEdicao = editar === "1" && !paciente.excluido;
  const aviso = criado ? "Paciente cadastrado. Boas-vindas enviadas ao contato informado." : salvo ? "Alterações salvas." : null;

  return (
    <div className="mx-auto max-w-5xl">
      <Link href="/pacientes" className="text-sm text-texto-suave hover:text-texto">
        ← Pacientes
      </Link>

      <div className="mt-2 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">{paciente.nome}</h1>
          <p className="mt-1 flex flex-wrap items-center gap-2 text-sm text-texto-suave">
            <span>
              {paciente.tipoDocumento === "Cpf" ? "CPF" : "Passaporte"} {paciente.numeroDocumentoFormatado}
            </span>
            <span aria-hidden>·</span>
            <span>
              {formatarData(paciente.dataNascimento)} ({paciente.idade} anos)
            </span>
            {paciente.menorDeIdade && <Etiqueta cor="amarelo">Menor de idade</Etiqueta>}
            {paciente.excluido ? <Etiqueta cor="vermelho">Excluído</Etiqueta> : <Etiqueta cor="verde">Ativo</Etiqueta>}
          </p>
        </div>
        {!emEdicao && <AcoesPaciente paciente={paciente} />}
      </div>

      {aviso && (
        <p role="status" className="mt-4 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {aviso}
        </p>
      )}

      {paciente.excluido && (
        <div className="mt-4 rounded-xl border border-erro/30 bg-erro/5 p-4 text-sm">
          <p className="font-semibold text-erro">Cadastro excluído em {formatarData(paciente.excluidoEm ?? "")}</p>
          <p className="mt-1 text-texto">Motivo: {paciente.motivoExclusao}</p>
          <p className="mt-1 text-xs text-texto-suave">
            O registro permanece armazenado (natureza de prontuário). Restaure para voltar a editá-lo.
          </p>
        </div>
      )}

      {emEdicao ? (
        <section className="mt-6 rounded-2xl border border-borda bg-white p-6 shadow-sm">
          <h2 className="mb-4 font-semibold text-texto">Editar cadastro</h2>
          <FormularioPaciente paciente={paciente} />
        </section>
      ) : (
        <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_1fr]">
          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
            <h2 className="font-semibold text-texto">Contato</h2>
            <dl className="mt-3 space-y-2 text-sm">
              <Linha rotulo="E-mail" valor={paciente.email} />
              <Linha rotulo="Telefone / WhatsApp" valor={formatarTelefone(paciente.telefone)} />
            </dl>
            {paciente.menorDeIdade && (
              <p className="mt-3 text-xs text-texto-suave">Contato do responsável legal — destino de todas as comunicações.</p>
            )}
          </section>

          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
            <h2 className="font-semibold text-texto">Responsável legal</h2>
            {paciente.responsavelLegal ? (
              <dl className="mt-3 space-y-2 text-sm">
                <Linha rotulo="Nome" valor={paciente.responsavelLegal.nome} />
                <Linha
                  rotulo={paciente.responsavelLegal.tipoDocumento === "Cpf" ? "CPF" : "Passaporte"}
                  valor={paciente.responsavelLegal.numeroDocumentoFormatado}
                />
                {paciente.responsavelLegal.parentesco && <Linha rotulo="Parentesco" valor={paciente.responsavelLegal.parentesco} />}
              </dl>
            ) : (
              <p className="mt-3 text-sm text-texto-suave">Não se aplica — paciente maior de idade.</p>
            )}
          </section>

          <section className="rounded-2xl border border-borda bg-white shadow-sm lg:col-span-2">
            <div className="flex flex-wrap items-center justify-between gap-3 border-b border-borda px-5 py-4">
              <h2 className="font-semibold text-texto">
                Exames <span className="ml-1 text-xs font-normal text-texto-suave">({exames.length})</span>
              </h2>
              {!paciente.excluido && (
                <Link
                  href={`/exames/novo?paciente=${paciente.id}`}
                  className="inline-flex items-center rounded-pill bg-primaria px-4 py-2 text-xs font-semibold text-white transition hover:bg-primaria-clara"
                >
                  + Novo exame
                </Link>
              )}
            </div>
            <TabelaExames exames={exames} mostrarPaciente={false} vazio="Nenhum exame para este paciente." />
          </section>

          <p className="text-xs text-texto-suave lg:col-span-2">
            Cadastrado em {formatarData(paciente.criadoEm)}
            {paciente.atualizadoEm && ` · atualizado em ${formatarData(paciente.atualizadoEm)}`}
          </p>
        </div>
      )}
    </div>
  );
}

function Linha({ rotulo, valor }: { rotulo: string; valor: string }) {
  return (
    <div className="flex gap-3">
      <dt className="w-40 shrink-0 text-texto-suave">{rotulo}</dt>
      <dd className="text-texto">{valor}</dd>
    </div>
  );
}
