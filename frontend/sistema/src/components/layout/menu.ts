/**
 * Menu do sistema interno. Itens sem rota ainda aparecem desabilitados: o escopo funcional
 * está definido (docs/03-escopo), mas pendências de negócio (C4.5, Q1.x) travam a implementação.
 */
export type ItemMenu = {
  rotulo: string;
  href?: string;
  icone: "inicio" | "usuarios" | "pacientes" | "exames" | "catalogo" | "amostras" | "laudos" | "financeiro";
  grupo?: string;
};

export const MENU: ItemMenu[] = [
  { rotulo: "Início", href: "/", icone: "inicio" },
  { rotulo: "Pacientes", href: "/pacientes", icone: "pacientes", grupo: "Operação" },
  { rotulo: "Exames", href: "/exames", icone: "exames", grupo: "Operação" },
  { rotulo: "Amostras", href: "/amostras", icone: "amostras", grupo: "Operação" },
  { rotulo: "Laudos", href: "/laudos", icone: "laudos", grupo: "Operação" },
  { rotulo: "Financeiro", icone: "financeiro", grupo: "Operação" },
  { rotulo: "Catálogo de exames", href: "/catalogo", icone: "catalogo", grupo: "Cadastros" },
  { rotulo: "Gestão de Usuários", href: "/usuarios", icone: "usuarios", grupo: "Cadastros" },
];
