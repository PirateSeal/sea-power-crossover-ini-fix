"""Build from locally installed assemblies. This helper does not install the DLL."""
from pathlib import Path
import argparse
import re
import subprocess
import tempfile

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--game', required=True, type=Path)
parser.add_argument('--output', type=Path, default=Path('CrossOverIniFileExistsFix.dll'))
parser.add_argument('--compiler', type=Path, help='Optional explicit Roslyn csc.dll path')
args = parser.parse_args()
compiler = args.compiler
if compiler is None:
    result = subprocess.run(['dotnet', '--list-sdks'], check=True, capture_output=True, text=True)
    sdks = re.findall(r'^([^ ]+) \[(.+)\]$', result.stdout, re.MULTILINE)
    if not sdks:
        parser.error('A .NET SDK is required; the .NET runtime alone has no compiler.')
    version, directory = sdks[-1]
    compiler = Path(directory) / version / 'Roslyn/bincore/csc.dll'
game = args.game.resolve()
managed = game / 'Sea Power_Data/Managed'
refs = [managed / name for name in ['mscorlib.dll', 'System.dll', 'System.Core.dll',
        'UnityEngine.dll', 'UnityEngine.CoreModule.dll', 'netstandard.dll']]
refs += [game / 'BepInEx/core' / name for name in ['BepInEx.dll', '0Harmony.dll']]
for path in [compiler] + refs:
    if not path.is_file():
        parser.error(f'Required local dependency missing: {path}')
source = Path(__file__).with_name('CrossOverIniFileExistsFix.cs').resolve()
output = args.output.resolve()
output.parent.mkdir(parents=True, exist_ok=True)
options = ['-nologo', '-nostdlib+', '-target:library', '-out:' + str(output)]
options += ['-r:' + str(path) for path in refs]
options.append(str(source))
with tempfile.TemporaryDirectory(prefix='seapower-ini-build-') as temp:
    response = Path(temp) / 'compiler.rsp'
    response.write_text('\n'.join('"' + option + '"' for option in options))
    subprocess.run(['dotnet', '--roll-forward', 'Major', str(compiler.resolve()), '@' + str(response)], check=True)
assert output.is_file() and output.stat().st_size > 0
print(f'Built {output}')
