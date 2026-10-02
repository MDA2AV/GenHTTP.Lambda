import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Vitrin yüklenemedi.',
  loading: 'Yükleniyor…',
  title: 'başlık',
  description: 'açıklama',
  picture: 'görsel',
  updated: 'Vitrin kartı güncellendi.',
  listed: 'Artık vitrin sayfasında.',
  waiting: 'Kaydedildi. Lambda yayına girince vitrin sayfasında görünecek.',
  saveFailed: 'Vitrin kartı kaydedilemedi.',
  removed: 'Vitrin sayfasından kaldırıldı.',
  removeFailed: 'Vitrin kartı kaldırılamadı.',
  wrongType: 'Bu bir PNG, JPEG, GIF ya da WebP görseli değil.',
  tooLarge: (size, limit) => `Bu dosya ${size}. Bir görsel en fazla ${limit} olabilir.`,
  unreadable: 'Bu dosya okunamadı.',
  hint: (tool) => (
    <>
      Vitrin sayfası, sahiplerinin göstermeyi seçtiği lambdaları listeler; son zamanlarda kullanılanlar önce gelir. Bir
      lambdayı oraya yalnızca editör anahtarına sahip olan ekleyebilir ya da oradan kaldırabilir. Lambda yalnızca
      yayındayken listelenir. Bir ajan da aynısını {tool} aracıyla yapabilir.
    </>
  ),
  open: 'Vitrini aç',
  switch: 'Bu lambdayı vitrin sayfasında göster',
  listedNow: 'Şu anda listede. Vitrine göz atan herkes açabilir.',
  notListed: 'Kaydedildi ama listede değil, çünkü lambda yayında değil. Yeniden yayına alınınca tekrar görünür.',
  off: 'Kapalı. Bunu açıp kaydedene kadar bu lambdayla ilgili hiçbir şey hiçbir yerde gösterilmez.',
  offline: 'Lambda yayında değil, bu yüzden kart lambda yayına alınana kadar bekleyecek. Yalnızca yanıt veren lambdalar listelenir.',
  titleLabel: 'Başlık',
  titlePlaceholder: 'Quiz gecesi skor tablosu',
  descriptionLabel: 'Açıklama',
  descriptionPlaceholder:
    'Takımlar cevaplarını telefondan girer, quiz’i yöneten kişi puan verir ve skor tablosu salondaki herkes için güncellenir.',
  save: 'Değişiklikleri kaydet',
  add: 'Vitrine ekle',
  takeOff: 'Kaldır',
  needs: (missing) =>
    `Eksik: ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} ve ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Bazı alanlar çok uzun.',
  allSaved: 'Her şey kaydedildi.',
  preview: 'Önizleme',
  card: (address) => <>Ziyaretçilerin gördüğü kart bu. Tıklanınca {address} açılır.</>,
  confirm: 'Vitrinden kaldırılsın mı?',
  keep: 'Kalsın',
  confirmText: 'Başlık, açıklama ve görsel silinir. Lambdanın kendisi olduğu gibi kalır.',
  pictureLabel: 'Görsel',
  formats: (limit) => `PNG, JPEG, GIF ya da WebP, en fazla ${limit}`,
  notSaved: 'henüz kaydedilmedi',
  replace: 'Değiştirmek için yenisini buraya bırakın.',
  drop: 'Bir görseli buraya bırakın.',
  advice: 'En iyi sonucu 16:10 oranında bir ekran görüntüsü ya da kullanımını gösteren kısa bir GIF verir.',
  another: 'Başkasını seç',
  choose: 'Dosya seç',
  keepSaved: 'Kaydedileni koru',
  clear: 'Temizle',
};
