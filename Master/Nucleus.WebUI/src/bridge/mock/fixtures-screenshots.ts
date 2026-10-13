// IGDB screenshot image ids of hub handlers, by hub handler id (captured 2026-10-11). The real host reads them from
// the handler detail of the API v3 (`screenshots[].imageId` of GET /api/v3/handlers/:id, `handlers.screenshots`).
const SCREENSHOT_IDS: Readonly<Record<string, readonly string[]>> = {
  rLSxwziuvTcuF3GTp: ['scxnim', 'scxj0w', 'scxj10', 'scxj0y'],
  QC7TTYjjZzaQ6Y6Nw: ['sclnvg', 'sctlac', 'sctlad', 'sc12xg1'],
  CgTFu6jc83pMKPZBy: ['scu5kw', 'scu5kx', 'scagdr', 'scagdo'],
  wXJ7GwedvhiBe2syS: ['dgrria8yrwofcmyio4em', 'hnh5yccbg9pekpaa6znh'],
  xjBSwFNj3ndRntsZh: ['sccqbi', 'sccqbn', 'sccqbk', 'iav7izshqdav3cwakdpj'],
  J5vdAQFvS9i3AZCRE: ['scljro', 'scljrl', 'scljrp', 'scljrw'],
  YztSFvxEtWgjuGwTT: ['ixynqzoedwo8vc5nzqma', 'scpuqr', 'scpuqs', 'scpuqt'],
  wFsXDjoAkHpL4XZm8: ['rj3csep68w7cjwv5129q', 'scrnjjf7pyzrvktfifzg', 'sctdyp', 'sctdyt'],
  '8ToryPKXFmff7D8h7': ['vdugyv42fp2jkh1w4lnz', 'dqyd3lhqnklcepe8dxw5', 'acdyxftnekdkxob4leek', 'scknql'],
  o8ixYAJ9Q382LJfLB: ['wivvkpn4banycjkkwbn5', 'wzx0sgc35in3e9iv4l3a', 'eodndyrz8m5ztsupqcnl', 'f14kd6um5cptjlsstiph'],
  HJtQAkDh9nyRd6z6J: ['l3afsfqphx2wuladftu1', 'uck0y6msk6r6kxefmb4b', 'xhmhtx32xxq07genjyqi', 'sc6vsu'],
  q926yWzpZZNiv7Cck: ['sc6sy3', 'sc6sy8', 'sc6sy7', 'sc6sy4'],
  fDmTaAoYKJmiWi3s3: ['scqn8u', 'scaewu', 'scqn8t', 'sctlbq'],
  '8ojcrL79RrFdpiaHD': ['xzfkhx3jzwzjfrj57w50', 'xtuyf1nemiyct2t66eqy', 'fejznasc7d0jbndc8wb3', 'icy9ouqv3jns8p8xroft'],
  '2W4NKZCTYmLqpwxrY': ['xzfkhx3jzwzjfrj57w50', 'xtuyf1nemiyct2t66eqy', 'fejznasc7d0jbndc8wb3', 'icy9ouqv3jns8p8xroft'],
  e63qNfxDrYv8vSK8h: ['sc8agd', 'sc8agh', 'sc8agf', 'scte31'],
  nBgGXtk5pTq9Taq2F: ['w23yfsxxkcc078yrpl1e', 'd4i6hkjjvw25hsffjxqz', 'hk4bruco13wfu1nnwovu', 'dkgfbtdjubjecjijtfb5'],
  PPct546raDXfjRLGw: ['jonqwskjocnykh9s6gug', 'f3fc6zqdew83zu4dgwdf', 'e9wm1jbpfbaflmvvldio', 'oornljaohheldfdli4tx'],
  '4i4d9P2K6YTcdyaBQ': ['scztch', 'scztck', 'scztcf', 'scztco'],
  ZeCGPXaap5Dzs5PXc: ['sctj9y', 'sctj9x'],
  sWgnoTqjmk8M49QML: ['ityinxmtkakwbokpcwws', 'p3svrq6ewzxnn7p1a3v9', 'q634ullxbvipm6q6mcq9', 'a3f72xprqkfuqdmha5ks'],
  yo3feQwberZKvghmZ: ['y3mcevfaze2vqv1qmapj', 'ldffm3ihtlvoqt9qtrnh', 'uuotpnsbi3rgjcmxqjhl', 'yvstmj75wfqrtz4um8x0'],
  f4Rddj3FbebKF4whb: ['tsz8jvbfvrk3pudrbnl0', 'sctf9j', 'mz3bhrhq4iwumdzokdpx', 'ilf2evl9fcszz7i1oobb'],
  esp2D3Wr49B3EBkbX: ['s8ktfgdgraigmjksthd4', 'st4aq06veb9xtawaxqwy', 'fkb70asrevsakb59nw4u', 'ibfg74pbx0sv6t6tunfy'],
  xLLksP2kS5GNxu6Ww: ['scv344', 'scv345', 'scv34b', 'scv341'],
  gNrREmQFCTRgYeBdd: ['sch0dk', 'sctdz3', 'sch0dn', 'sch0dh'],
  vQ8v7jm8rEwtoTwFA: ['sszdprkwkbhioqd52os2', 'hhgds6hkfreckczdxlvn', 'wppppfhgfqeuqoe6vbar', 'gxn94hwt4q2ieohzghib'],
  KY4arvm8WGeaSLmRW: ['scagp2', 'scagp3', 'scagox', 'scagow'],
  '2eJM7Cxn5kfqbz9pz': ['sctdza', 'j5yft14sj5hchgjy0nzl', 'scvhlz', 'sc132c9'],
  wN8RrD533Ags3wxTw: ['g8roegct3qyzmnubhoeb', 'ux3rollue9i9nu5fmnhv', 'j1ir2upgzvdnbytprbll', 'zu4jwolpao5yhcc9e5df'],
  zBbF958JXyHRwdTcb: ['sc5rod', 'sc5roi', 'sc5rog', 'sc5rof'],
  qvLsjBCuPz3ttXvec: ['scavam', 'scaval', 'scavao', 'scavaq'],
  '6hkBwKMXqpnjiWTdi': ['sc15bhk', 'sc15bhl', 'sc15bhm', 'sc15bhn'],
  '8xXSm3nNaLaZNiz3w': ['scmd4c', 'sctoor', 'scmd4a', 'scmd48'],
  SXxys76eLF6BC36PF: ['sc68wb', 'sc68wc', 'sc68w4', 'sc68w6'],
  pZTGGkLbhnMvCFWFe: ['cw4sb4mrkm9rnn8jp6j1', 'naky75obzzzbx5im3umm', 'sbjpurw2mm3ih2qtjm7w', 'jykqglnnppjdvcmogtbe'],
  RkbcpmL5dNbrDcfjL: ['zsps0veeqpn6sod53lxb', 'r6hhvtaozowkcf06bswz', 'scpoib', 'scpoi9'],
  DxWk5Q6uThnh3Bsrg: ['scrmbq', 'scrmbr', 'scrmbs', 'scrmbv'],
  NmdNMirLvXXzNkLfY: ['scrf65', 'scrf62', 'scrf61', 'scrf63'],
  vHGNLFakwRXdd6zJe: ['s8ktfgdgraigmjksthd4', 'st4aq06veb9xtawaxqwy', 'fkb70asrevsakb59nw4u', 'ibfg74pbx0sv6t6tunfy'],
  '8GbK8CraioYmevXAH': ['scabry', 'scabs7', 'scabs8', 'scabsg'],
  LT3wMdrR7gPrKgfzb: ['g1aakqbkp2quq0krqeky', 'sw7rtba7p1xs77klsime', 'ak7zmag7zztpqha1jjde', 'iwswpvxa9ytrpk8yjcyx'],
  J7qFPHSmH76LxBcJg: ['sctnzt', 'sctnzz', 'sctnzx', 'scto00'],
  GZjjedmmah2v7ibbj: ['sctohq', 'sctohj', 'sctohk', 'sctogy'],
  EBh2WSW2hhwP3etSq: ['scifce', 'scifcf', 'scifca', 'scifcb'],
  wBSduky3MfS5SCMAd: ['xbiwlwlajaxzxg18p1wn', 'v3sriikqzybazy2329b1', 'vtymyj3ortrtx6jaj9aj', 'vlgdiatdxggjsxppaufd'],
};

/** IGDB image URL of an image id, at the given size (see https://api-docs.igdb.com/#images). */
export function igdbImage(imageId: string, size: 'thumb' | 'cover_big_2x' | 'screenshot_big' | '1080p'): string {
  return `https://images.igdb.com/igdb/image/upload/t_${size}/${imageId}.jpg`;
}

/** Screenshot URLs of a hub handler's game. Empty when the hub has none. */
export function hubScreenshotUrls(handlerId: string): string[] {
  return (SCREENSHOT_IDS[handlerId] ?? []).map((id) => igdbImage(id, '1080p'));
}
