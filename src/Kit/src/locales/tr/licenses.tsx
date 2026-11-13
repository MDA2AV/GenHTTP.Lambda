import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Lisans ve telif hakkı bildirimi yanında kaldığı sürece herkes onu her türlü işte kullanabilir, değiştirebilir ve başkalarına verebilir.',
  'Apache-2.0': 'MIT gibi; ayrıca katkıda bulunan herkesten bir patent lisansı içerir ve değişikliklerin değişiklik olarak belirtilmesini ister.',
  'BSD-3-Clause': 'MIT gibi; ayrıca kimse, ondan yaptığı şeyin tanıtımında yazarların adını kullanamaz.',
  'MPL-2.0': 'Bu dosyalardaki değişiklikler aynı lisansla kalır; dosyalar başka herhangi bir lisanstaki kodla birleştirilebilir.',
  'GPL-3.0-or-later': 'Onu değiştirerek ya da olduğu gibi başkalarına veren herkes, kaynak kodunu da aynı lisansla vermek zorundadır.',
  'AGPL-3.0-or-later': 'GPL gibi; ayrıca değiştirilmiş bir kopyayı insanlara ağ üzerinden sunmak da onu başkalarına vermek sayılır.',
  Unlicense: 'Kamu malına bırakılmıştır: herkes onunla koşulsuz olarak her şeyi yapabilir.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'İzin verici',
  Copyleft: 'Copyleft',
  PublicDomain: 'Kamu malı',
};
