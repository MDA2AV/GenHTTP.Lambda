import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Geliştirme alanı',
  hint: 'Bir sürümün statik dosyalarının, bir araç zincirinin onları derlediği yerde neyden derlendiği: frontend’in projesi, kaynakları, yapılandırması ve kilit dosyasıyla. Her sürümle birlikte saklanır ve asla derlenmez ya da sunulmaz. Onu kim değiştirirse - ajanınız, bir klonda - çalıştığı yerde derler ve derlediği şeyle birlikte kaydeder: bu platform hiçbir şey derlemez. Bu yüzden burada okunur, düzenlenmez.',
  overview: 'Genel bakış',
  files: 'Dosyalar',
  scope: (version) =>
    `${version}. sürümün statik dosyalarının neyden derlendiği - onunla birlikte saklanır, asla derlenmez ya da sunulmaz ve onu değiştiren kişi tarafından derlenir, burada asla.`,
  scopeDraft: 'Bu taslağın statik dosyalarının neyden derlendiği - onunla birlikte saklanır, asla derlenmez ya da sunulmaz ve onu değiştiren kişi tarafından derlenir, burada asla.',
  reading: 'Geliştirme alanı okunuyor…',
  readFailed: 'Geliştirme alanı okunamadı.',

  emptyTitle: (version) => `${version}. sürümde geliştirme alanı yok`,
  emptyTitleDraft: 'Bu taslakta geliştirme alanı yok',
  emptyText: (code) => (
    <>
      Bir frontend bir araç zinciriyle derlendiğinde - Vite ile React, Vue ya da Svelte, TypeScript, Tailwind - projesi
      burada, her sürümle birlikte saklanır: statik dosyaların neyden derlendiği. Ajanınız onu çalıştığı yerde derler
      ve kaynakları, derlediği şeyle birlikte kaydeder - bir klonda bu {code('dev/')} klasörüdür. Düz HTML, CSS ve
      JavaScript’ten oluşan bir frontend’e gerek yoktur.
    </>
  ),
  emptyHow: (code) => (
    <>Bir klondaki {code('AGENTS.md')}, kodlama ajanına bunu nasıl kuracağını anlatır.</>
  ),

  projects: 'Projeler',
  atTheTop: 'geliştirme alanının kendisi',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'Derleyen araç',
  build: 'Derleme',
  noBuild: 'package.json dosyasında derleme betiği yok.',
  into: 'Derleme hedefi',
  intoAssets: (folder, files, size) => (
    <>
      statik dosyaların {folder} klasörü - bu sürümde {files === 1 ? '1 dosya' : `${files} dosya`}, {size}
    </>
  ),
  intoNothing: (folder) => <>statik dosyaların {folder} klasörü - bu sürümde boş</>,
  packages: 'Paketler',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? 'çalıştırmak için 1' : `çalıştırmak için ${runtime}`}, ${tooling === 1 ? 'derlemek için 1' : `derlemek için ${tooling}`}`,
  showPackages: 'Göster',
  hidePackages: 'Gizle',
  runtime: 'Çalıştırmak için',
  tooling: 'Derlemek için',
  missing: (page, files) => (
    <>
      {page}, statik dosyalar arasında olmayan {files.length === 1 ? 'bir dosyaya' : `${files.length} dosyaya`}
      {' '}başvuruyor ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): derlemenin yazdıkları
      eksiksiz kaydedilmedi ve sayfa yüklenmiyor.
    </>
  ),
  noLock: 'Kilit dosyası yok: bir sonraki derleme, paketlerin sonuncusundan farklı sürümlerini kurabilir.',
  noIgnore: '.gitignore yok: araç zincirinin kurduğu ve derlediği şeyler bir sürümün içine girebilir.',

  inVersion: (version) => `${version}. sürümde`,
  inDraft: 'Bu taslakta',
  comparedWith: (version) => `${version}. sürümle karşılaştırıldığında`,
  first: 'Buna sahip olan ilk sürüm.',
  both: (here, assets) =>
    `Burada ${here === 1 ? '1 dosya' : `${here} dosya`} ve statik dosyalardan ${assets === 1 ? '1 dosya' : `${assets} dosya`} değişti.`,
  hereOnly: (here) =>
    `Burada ${here === 1 ? '1 dosya' : `${here} dosya`} değişti, statik dosyalardan hiçbiri değişmedi: değişiklik derleme gerektirmediyse ziyaretçiler öncekiyle aynı şeyi görür.`,
  builtOnly: (folder) => (
    <>{folder} içine derlenen şey değişti, burada hiçbir şey değişmedi: derlemenin yazdıklarında yapılan bir değişiklik bir sonraki derlemeyle geri alınır.</>
  ),
  assetsOnly: 'Burada hiçbir şey değişmedi.',
  unchanged: 'Burada da statik dosyalarda da hiçbir şey değişmedi.',
  showChanges: 'Değişiklikleri göster',
  hideChanges: 'Değişiklikleri gizle',
  noChanges: 'Burada hiçbir şey değişmedi.',

  readme: 'Nasıl derlendiği',
  noReadme: (code) => (
    <>
      Nasıl derlendiğini anlatan bir şey yok. Geliştirme alanının en üstündeki bir {code('README.md')} - komutlar ve
      derlemenin gittiği yer - bir sonraki ajanın derlemeyi yaparken dayanacağı şeydir.
    </>
  ),
  readOnly: 'Salt okunur: derlendiği yerde değiştirilir.',
  noFiles: 'Dosya yok.',
};
