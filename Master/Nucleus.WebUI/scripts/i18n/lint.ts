// Finds user-visible strings written in the code instead of going through t() / <T> (npm run i18n:lint). Heuristic, on
// the ESTree AST of oxc (parseSync from vite): JSX text, text props (title, aria-label, placeholder, alt, label...),
// object properties that carry text (label:, title:, message:...), toast() calls and document.title. Opt out with a
// comment containing `i18n-ignore` on the line or the line before (say why), or `i18n-ignore-file` for a whole file.

/** An ESTree node, as far as the linter cares. */
export interface AstNode {
  type: string;
  start: number;
  end: number;
  [field: string]: unknown;
}

/** A hard-coded string. */
export interface Finding {
  file: string;
  line: number;
  column: number;
  /** Where it was found: `jsx-text`, `attribute title`, `property label`, `call toast`, `assign title`. */
  kind: string;
  text: string;
}

/** JSX props whose string value is shown to the user (or read by a screen reader). */
export const TEXT_ATTRIBUTES = new Set([
  'title', 'aria-label', 'aria-description', 'aria-roledescription', 'aria-valuetext', 'aria-placeholder', 'placeholder', 'alt',
  'label', 'description', 'message', 'hint', 'tooltip', 'confirmLabel', 'cancelLabel', 'emptyText', 'heading', 'subtitle',
  'caption', 'text', 'success', 'detail',
]);

/** Object properties whose string value is shown to the user (hint bars, menus, confirmations, toasts...). */
export const TEXT_PROPERTIES = new Set([
  'label', 'title', 'description', 'message', 'placeholder', 'hint', 'tooltip', 'confirmLabel', 'cancelLabel', 'ariaLabel',
  'aria-label', 'alt', 'success', 'heading', 'subtitle', 'caption', 'emptyText', 'text', 'detail',
]);

/** Functions whose first argument is shown to the user. */
export const TEXT_CALLS = new Set(['toast', 'notify', 'alert']);

/** Elements whose text is a key cap or code, not a sentence. */
export const LITERAL_ELEMENTS = new Set(['Kbd', 'kbd', 'GamepadGlyph', 'code', 'pre', 'samp']);

/** Names that stay as they are in every language. A string made only of these is not reported. */
export const BRANDS = [
  'Nucleus Co-op', 'SplitScreen.Me', 'Nucleus', 'Steam', 'Epic Games', 'Epic', 'GOG', 'Xbox', 'PlayStation', 'Discord',
  'GitHub', 'Windows', 'WebView2', 'Goldberg', 'XInput', 'DirectInput', 'SDL',
];

const LETTER = /\p{L}/u;
const IDENTIFIER_LIKE = /^[a-z][a-zA-Z0-9]*([._:/#-][a-zA-Z0-9]+)+$/;
const URL_LIKE = /^(https?:|mailto:|data:|\/\/)/i;

/** Whether a string reads as words for the user (not a key, an id, a URL, a brand or punctuation). */
export function isUserText(raw: string): boolean {
  const text = raw.replace(/\s+/g, ' ').trim();
  if (!LETTER.test(text) || IDENTIFIER_LIKE.test(text) || URL_LIKE.test(text)) return false;
  let rest = text;
  for (const brand of BRANDS) rest = rest.split(brand).join(' ');
  return LETTER.test(rest);
}

/** Hard-coded strings of one parsed file. */
export function lintProgram(program: AstNode, source: string, file: string): Finding[] {
  if (source.includes('i18n-ignore-file')) return [];
  const lineStarts = [0];
  for (let i = 0; i < source.length; i++) if (source[i] === '\n') lineStarts.push(i + 1);
  const lines = source.split('\n');
  const position = (offset: number) => {
    let low = 0;
    let high = lineStarts.length - 1;
    while (low < high) {
      const mid = (low + high + 1) >> 1;
      if (lineStarts[mid]! <= offset) low = mid;
      else high = mid - 1;
    }
    return { line: low + 1, column: offset - lineStarts[low]! + 1 };
  };
  const ignored = (line: number) => Boolean(lines[line - 1]?.includes('i18n-ignore') || lines[line - 2]?.includes('i18n-ignore'));

  const findings: Finding[] = [];
  const report = (offset: number, kind: string, text: string) => {
    if (!isUserText(text)) return;
    const at = position(offset);
    if (ignored(at.line)) return;
    findings.push({ file, line: at.line, column: at.column, kind, text: text.replace(/\s+/g, ' ').trim() });
  };

  /** Reports the literal strings an expression can evaluate to. */
  const strings = (expression: unknown, kind: string): void => {
    const node = expression as AstNode | null | undefined;
    if (!node || typeof node !== 'object') return;
    switch (node.type) {
      case 'Literal':
        if (typeof node.value === 'string') report(node.start, kind, node.value);
        break;
      case 'TemplateLiteral': {
        const quasis = (node.quasis as AstNode[]).map((q) => (q.value as { cooked?: string; raw: string }).cooked ?? '');
        if (quasis.some((q) => LETTER.test(q))) report(node.start, kind, quasis.join('{…}'));
        break;
      }
      case 'ConditionalExpression':
        strings(node.consequent, kind);
        strings(node.alternate, kind);
        break;
      case 'LogicalExpression':
        strings(node.left, kind);
        strings(node.right, kind);
        break;
      case 'BinaryExpression':
        if (node.operator === '+') {
          strings(node.left, kind);
          strings(node.right, kind);
        }
        break;
      case 'ParenthesizedExpression':
      case 'TSAsExpression':
      case 'TSSatisfiesExpression':
      case 'TSNonNullExpression':
      case 'JSXExpressionContainer':
        strings(node.expression, kind);
        break;
    }
  };

  const nameOf = (node: unknown): string | undefined => {
    const n = node as AstNode | undefined;
    if (!n) return undefined;
    if (n.type === 'JSXIdentifier' || n.type === 'Identifier') return n.name as string;
    if (n.type === 'Literal' && typeof n.value === 'string') return n.value;
    if (n.type === 'JSXMemberExpression' || n.type === 'MemberExpression') return nameOf(n.property);
    if (n.type === 'JSXNamespacedName') return `${nameOf(n.namespace)}:${nameOf(n.name)}`;
    return undefined;
  };

  const visit = (value: unknown, literalElement: boolean): void => {
    if (Array.isArray(value)) {
      for (const item of value) visit(item, literalElement);
      return;
    }
    const node = value as AstNode | null;
    if (!node || typeof node !== 'object' || typeof node.type !== 'string') return;

    switch (node.type) {
      case 'JSXElement':
      case 'JSXFragment': {
        const opening = node.openingElement as AstNode | undefined;
        if (opening) visit(opening, literalElement);
        const literal = literalElement || LITERAL_ELEMENTS.has(nameOf(opening?.name) ?? '');
        for (const child of node.children as AstNode[]) {
          // {cond ? 'Yes' : 'No'} as a child.
          if (child.type === 'JSXExpressionContainer' && !literal) strings(child.expression, 'jsx-expression');
          visit(child, literal);
        }
        return;
      }
      case 'JSXText':
        if (!literalElement) {
          const raw = node.value as string;
          const lead = raw.length - raw.trimStart().length;
          report(node.start + lead, 'jsx-text', raw);
        }
        return;
      case 'JSXAttribute': {
        const name = nameOf(node.name) ?? '';
        if (TEXT_ATTRIBUTES.has(name)) strings(node.value, `attribute ${name}`);
        break;
      }
      case 'Property': {
        const name = node.computed ? undefined : nameOf(node.key);
        if (name && TEXT_PROPERTIES.has(name)) strings(node.value, `property ${name}`);
        break;
      }
      case 'CallExpression': {
        const name = nameOf(node.callee);
        if (name && TEXT_CALLS.has(name)) strings((node.arguments as unknown[])[0], `call ${name}`);
        break;
      }
      case 'AssignmentExpression': {
        const left = node.left as AstNode;
        if (left.type === 'MemberExpression' && nameOf(left.property) === 'title') strings(node.right, 'assign title');
        break;
      }
    }

    for (const [field, child] of Object.entries(node)) {
      if (field === 'type' || field === 'start' || field === 'end' || field === 'range' || field === 'loc') continue;
      if (child && typeof child === 'object') visit(child, literalElement);
    }
  };

  visit(program, false);
  // A string reached twice (a JSX expression inside a text prop) is reported once.
  const seen = new Set<string>();
  return findings.filter((f) => {
    const id = `${f.line}:${f.column}`;
    if (seen.has(id)) return false;
    seen.add(id);
    return true;
  });
}

/** Folder a finding counts under: `screens/library`, `shell`, `ui`, `(root)` for files directly in src. */
export function groupOf(relativePath: string): string {
  const parts = relativePath.replace(/\\/g, '/').replace(/^src\//, '').split('/');
  if (parts.length === 1) return '(root)';
  return parts[0] === 'screens' && parts.length > 2 ? `screens/${parts[1]}` : parts[0]!;
}
