import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Derleme',
  hint: 'Bir sürümün statik dosyalarının veya kodunun neyden derlendiği: uygulamayı değiştiren kişinin - bir klonda çalışan ajanınızın - üzerinde derleme aracı çalıştırdığı dosyalar. Her sürümle birlikte saklanır, asla derlenmez ya da sunulmaz. Bu platform hiçbir şey derlemez, bu yüzden burada düzenlenmez, yalnızca okunur.',
  overview: 'Genel bakış',
  files: 'Dosyalar',
  scope: (version) =>
    `${version}. sürümün neyden derlendiği - onunla birlikte saklanır, asla derlenmez ya da sunulmaz, onu değiştiren kişi tarafından derlenir, burada asla.`,
  scopeDraft: 'Bu taslağın neyden derlendiği - onunla birlikte saklanır, asla derlenmez ya da sunulmaz, onu değiştiren kişi tarafından derlenir, burada asla.',
  reading: 'Neyden derlendiği okunuyor…',
  readFailed: 'Neyden derlendiği okunamadı.',

  emptyTitle: (version) => `${version}. sürüm neyden derlendiğini saklamıyor`,
  emptyTitleDraft: 'Bu taslak neyden derlendiğini saklamıyor',
  emptyText: (code) => (
    <>
      Bir sürümün statik dosyaları veya kodu bir derleme aracıyla - derlenerek, paketlenerek ya da üretilerek -
      oluşturuluyorsa, onların oluşturulduğu dosyalar her sürümle birlikte burada saklanır: bir klonda {code('build/')}
      klasörü. Uygulamayı değiştiren kişi derlemeyi çalıştığı yerde yapar ve ikisini birlikte kaydeder; bu platform
      hiçbir şey derlemez. Sunulduğu ya da derlendiği şekliyle yazılan şeylerin buna ihtiyacı yoktur.
    </>
  ),
  emptyHow: (code) => (
    <>Bir klondaki {code('AGENTS.md')}, kodlama ajanına bunun nasıl kullanıldığını anlatır.</>
  ),

  inVersion: (version) => `${version}. sürümde`,
  inDraft: 'Bu taslakta',
  comparedWith: (version) => `${version}. sürüme göre`,
  first: 'Bunu saklayan ilk sürüm.',
  both: (here, program) =>
    `Burada ${here === 1 ? '1 dosya' : `${here} dosya`} değişti, kod ve statik dosyalarda ise ${program === 1 ? '1 dosya' : `${program} dosya`}.`,
  hereOnly: (here) =>
    `Burada ${here === 1 ? '1 dosya' : `${here} dosya`} değişti, kodda ve statik dosyalarda hiçbir şey değişmedi: değişenler onlara derleniyorsa, derleme yapılmamış demektir.`,
  programOnly: 'Burada hiçbir şey değişmedi.',
  unchanged: 'Burada, kodda ve statik dosyalarda hiçbir şey değişmedi.',
  showChanges: 'Değişiklikleri göster',
  hideChanges: 'Değişiklikleri gizle',
  noChanges: 'Burada hiçbir şey değişmedi.',

  readme: 'Nasıl derlendiği',
  noReadme: (code) => (
    <>
      Nasıl derlendiğini anlatan bir şey yok. En üstte bir {code('README.md')} - komutlar ve derlemenin nereye
      gittiği - bir sonraki ajanın derlemeyi yaparken dayanacağı şeydir.
    </>
  ),
  readOnly: 'Salt okunur: derlendiği yerde değiştirilir.',
  noFiles: 'Dosya yok.',
};
