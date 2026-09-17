"use client";

import Link from "next/link";
import { useActionState, useState } from "react";
import type { Paciente, TipoDocumento } from "@/lib/api";
import { idade } from "@/lib/formatos";
import { salvarPaciente, type EstadoSalvarPaciente } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Selecao } from "@/components/ui/selecao";

const MAIORIDADE = 18;

/**
 * Campos controlados: o React limpa formulários não controlados após cada envio, e um erro de validação
 * não pode apagar o que a recepção digitou.
 */
export function FormularioPaciente({ paciente, hoje }: { paciente?: Paciente; hoje: string }) {
  const r = paciente?.responsavelLegal;
  const [v, setV] = useState({
    nome: paciente?.nome ?? "",
    dataNascimento: paciente?.dataNascimento ?? "",
    tipoDocumento: paciente?.tipoDocumento ?? ("Cpf" as TipoDocumento),
    numeroDocumento: paciente?.numeroDocumento ?? "",
    email: paciente?.email ?? "",
    telefone: paciente?.telefone ?? "",
    responsavelOpcional: !!r,
    responsavelNome: r?.nome ?? "",
    responsavelTipoDocumento: r?.tipoDocumento ?? ("Cpf" as TipoDocumento),
    responsavelNumeroDocumento: r?.numeroDocumento ?? "",
    responsavelParentesco: r?.parentesco ?? "",
  });
  const [estado, acao, salvando] = useActionState<EstadoSalvarPaciente, FormData>(salvarPaciente, {});

  const menor = /^\d{4}-\d{2}-\d{2}$/.test(v.dataNascimento) && idade(v.dataNascimento, hoje) < MAIORIDADE;
  const temResponsavel = menor || v.responsavelOpcional;

  function campo<K extends keyof typeof v>(nome: K) {
    return {
      name: nome,
      value: v[nome] as string,
      onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => setV((a) => ({ ...a, [nome]: e.target.value })),
    };
  }

  const erro = estado.campos ?? {};
  const placeholderDocumento = (tipo: TipoDocumento) => (tipo === "Cpf" ? "000.000.000-00" : "AB123456");

  return (
    <form action={acao} className="space-y-6">
      {paciente && <input type="hidden" name="id" value={paciente.id} />}
      <input type="hidden" name="temResponsavel" value={temResponsavel ? "sim" : "nao"} />

      <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">Paciente</h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <Campo label="Nome completo" required autoComplete="off" className="sm:col-span-2" erro={erro.nome} {...campo("nome")} />
          <Campo
            label="Data de nascimento"
            type="date"
            required
            max={hoje}
            erro={erro.dataNascimento}
            dica={menor ? "Menor de idade: o responsável legal é obrigatório." : undefined}
            {...campo("dataNascimento")}
          />
          <div className="grid grid-cols-[130px_1fr] gap-3">
            <Selecao label="Documento" {...campo("tipoDocumento")}>
              <option value="Cpf">CPF</option>
              <option value="Passaporte">Passaporte</option>
            </Selecao>
            <Campo
              label="Número"
              required
              autoComplete="off"
              placeholder={placeholderDocumento(v.tipoDocumento)}
              erro={erro.numeroDocumento}
              {...campo("numeroDocumento")}
            />
          </div>
        </div>
      </section>

      <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">Contato</h2>
        <p className="mt-1 text-xs text-texto-suave">
          Obrigatórios: são o destino das mensagens e do código de acesso ao portal.
          {temResponsavel && " Informe o e-mail e o telefone do responsável legal."}
        </p>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <Campo label="E-mail" type="email" required autoComplete="off" erro={erro.email} {...campo("email")} />
          <Campo
            label="Telefone (WhatsApp)"
            type="tel"
            required
            autoComplete="off"
            placeholder="(31) 99999-8888"
            dica="Número estrangeiro: comece com + e o código do país."
            erro={erro.telefone}
            {...campo("telefone")}
          />
        </div>
      </section>

      <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h2 className="font-semibold text-texto">Responsável legal</h2>
          {menor ? (
            <span className="text-xs font-semibold text-alerta">Obrigatório para menor de idade</span>
          ) : (
            <label className="flex items-center gap-2 text-sm text-texto">
              <input
                type="checkbox"
                checked={v.responsavelOpcional}
                onChange={(e) => setV((a) => ({ ...a, responsavelOpcional: e.target.checked }))}
                className="size-4 accent-primaria"
              />
              Paciente usa o contato de um responsável
            </label>
          )}
        </div>

        {temResponsavel ? (
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            <Campo
              label="Nome do responsável"
              required
              autoComplete="off"
              erro={erro.responsavelNome}
              {...campo("responsavelNome")}
            />
            <Campo label="Parentesco (opcional)" autoComplete="off" placeholder="Mãe, pai, tutor..." {...campo("responsavelParentesco")} />
            <div className="grid grid-cols-[130px_1fr] gap-3 sm:col-span-2 sm:max-w-md">
              <Selecao label="Documento" {...campo("responsavelTipoDocumento")}>
                <option value="Cpf">CPF</option>
                <option value="Passaporte">Passaporte</option>
              </Selecao>
              <Campo
                label="Número"
                required
                autoComplete="off"
                placeholder={placeholderDocumento(v.responsavelTipoDocumento)}
                erro={erro.responsavelNumeroDocumento}
                {...campo("responsavelNumeroDocumento")}
              />
            </div>
          </div>
        ) : (
          <p className="mt-2 text-xs text-texto-suave">
            Não se aplica. Marque se o paciente, mesmo maior de idade, depende do contato de outra pessoa.
          </p>
        )}
      </section>

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      <div className="flex flex-wrap justify-end gap-3">
        <Link
          href="/pacientes"
          className="inline-flex items-center rounded-pill border border-primaria bg-white px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
        >
          Cancelar
        </Link>
        <Botao type="submit" carregando={salvando}>
          {paciente ? "Salvar alterações" : "Cadastrar paciente"}
        </Botao>
      </div>
    </form>
  );
}
