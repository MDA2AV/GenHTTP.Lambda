import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Bir sürüm, programın kendisidir: kodu ve statik dosyaları. Kaydedildikten sonra bir daha değişmez, bu yüzden herhangi biriyle karşılaştırma yapabilir ya da herhangi birini tam olduğu gibi yeniden yayına alabilirsiniz. Her sürüm, ne istendiğini ve neyi değiştirdiğini saklar. Lambdayı değiştirmek için bir taslak başlatın: hazır olunca bir sonraki sürüm olur. En fazla ${limit} sürüm tutulur, fazlası olunca en eskiler silinir. Yayındaki sürüm asla silinmez.`,
  none: 'Henüz sürüm yok.',
  noDescription: 'Açıklama yok',
  online: 'yayında',
  putOnline: 'Bu sürümü yayına al',
  rollBackTitle: 'Bu eski sürümü yeniden yayına al',
  deploy: 'Yayına al',
  rollBack: 'Geri dön',
  readFailed: 'Bu sürüm okunamadı.',
  comparing: 'Karşılaştırılıyor…',
  unchanged: 'Bir önceki sürüme göre değişiklik yok.',
  first: 'İlk sürüm.',
  status: { added: 'eklendi', removed: 'silindi', changed: 'değişti', same: 'aynı' },
  groups: {
    code: 'Kod',
    assets: 'Statik dosyalar',
    build: 'Derleme',
    context: 'Dokümantasyon ve testler',
  },
  browse: 'Dosyalarına göz at',
  docs: 'Dokümantasyonunu oku',
  build: 'Neyden derlendiğini gör',
  edit: 'Buradan düzenle',
  feature: 'Buradan bir taslak başlat',
  featureTitle: 'Bu sürümde bir değişikliği lambdanın yanında hazırlayın, hazır olunca birleştirip bir sonraki sürüm yapın',
  binary: 'Metin değil, karşılaştırılacak satır yok.',
  tooLarge: 'Satır satır karşılaştırmak için çok büyük.',
};
