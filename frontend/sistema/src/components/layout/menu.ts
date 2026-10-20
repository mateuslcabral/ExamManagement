/**
 * Menu do sistema interno. "Operação" é o dia a dia (pacientes, fila de exames, consulta ao catálogo, caixa);
 * "Cadastros" só cria ou mantém registros. A gestão dos exames inteira fica em Fluxos (pedido do cliente, 20/10/2026).
 */
export type ItemMenu = {
  rotulo: string;
  href?: string;
  /** Prefixos de rota que também marcam o item como ativo (ex.: o detalhe do exame pertence a Fluxos). */
  tambemAtivoEm?: string[];
  icone: "inicio" | "usuarios" | "pacientes" | "exames" | "catalogo" | "amostras" | "laudos" | "financeiro";
  grupo?: string;
};

export const MENU: ItemMenu[] = [
  { rotulo: "Início", href: "/", icone: "inicio" },
  { rotulo: "Pacientes", href: "/pacientes", icone: "pacientes", grupo: "Operação" },
  { rotulo: "Fluxos", href: "/fluxos", tambemAtivoEm: ["/exames"], icone: "laudos", grupo: "Operação" },
  { rotulo: "Catálogo", href: "/catalogo", icone: "catalogo", grupo: "Operação" },
  { rotulo: "Financeiro", href: "/financeiro", icone: "financeiro", grupo: "Operação" },
  { rotulo: "Novo exame", href: "/exames/novo", icone: "exames", grupo: "Cadastros" },
  { rotulo: "Catálogo de exames", href: "/catalogo/gerenciar", icone: "catalogo", grupo: "Cadastros" },
  { rotulo: "Cadastros auxiliares", href: "/cadastros", icone: "amostras", grupo: "Cadastros" },
  { rotulo: "Gestão de Usuários", href: "/usuarios", icone: "usuarios", grupo: "Cadastros" },
];

/** Comprimento do prefixo de `item` que casa com `pathname` (0 = não casa). O item com o maior prefixo é o ativo. */
export function pesoAtivo(item: ItemMenu, pathname: string): number {
  if (!item.href) return 0;
  const candidatos = [item.href, ...(item.tambemAtivoEm ?? [])];
  let melhor = 0;
  for (const c of candidatos) {
    const casa = c === "/" ? pathname === "/" : pathname === c || pathname.startsWith(`${c}/`);
    if (casa) melhor = Math.max(melhor, c.length);
  }
  return melhor;
}
