#!/usr/bin/env node
// Regenerates src/bridge/generated/contract.ts from the C# contract (Master/Nucleus.Bridge/Contracts):
// builds Nucleus.Bridge.Gen with MSBuild, then runs it.
//
//   node scripts/bridge-gen.mjs           write contract.ts (untouched when nothing changed)
//   node scripts/bridge-gen.mjs --check   exit 1 when contract.ts is stale (CI)
//
// MSBuild is found with vswhere (Visual Studio 2022 or Build Tools, with the .NET desktop build tools and a .NET SDK);
// set MSBUILD to use another one.
import { execFileSync, spawnSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const webui = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const generator = resolve(webui, '../Nucleus.Bridge.Gen');
const contract = join(webui, 'src/bridge/generated/contract.ts');
const check = process.argv.includes('--check');

run(findMSBuild(), [join(generator, 'Nucleus.Bridge.Gen.csproj'), '-restore', '-p:Configuration=Release', '-v:minimal', '-nologo']);
run(join(generator, 'bin/Release/net472/Nucleus.Bridge.Gen.exe'), ['--out', contract, ...(check ? ['--check'] : [])]);

function findMSBuild() {
  if (process.env.MSBUILD) return process.env.MSBUILD;
  const programFiles = process.env['ProgramFiles(x86)'] ?? 'C:\\Program Files (x86)';
  const vswhere = join(programFiles, 'Microsoft Visual Studio/Installer/vswhere.exe');
  if (existsSync(vswhere)) {
    const args = ['-latest', '-prerelease', '-products', '*', '-requires', 'Microsoft.Component.MSBuild', '-find', 'MSBuild\\**\\Bin\\MSBuild.exe'];
    const found = execFileSync(vswhere, args, { encoding: 'utf8' }).split(/\r?\n/).find(Boolean);
    if (found) return found;
  }
  const buildTools = join(programFiles, 'Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/MSBuild.exe');
  if (existsSync(buildTools)) return buildTools;
  console.error('MSBuild not found. Install Visual Studio 2022 or its Build Tools (.NET desktop build tools), or set MSBUILD.');
  process.exit(1);
}

function run(command, args) {
  const result = spawnSync(command, args, { stdio: 'inherit' });
  if (result.error) throw result.error;
  if (result.status !== 0) process.exit(result.status ?? 1);
}
