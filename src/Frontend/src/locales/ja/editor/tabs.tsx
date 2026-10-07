import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'C#ファイルの名前：英数字、ハイフン、アンダースコア、ドットを使い、英字で始まり.csで終わる、40文字以内。',
  name: '英数字と - _ . + @ ( ) [ ] { } $ ~ が使えます。フォルダーはスラッシュで区切り、スペースや、ドットで終わる名前は使えません。',
  taken: 'エクスポートまたはクローンしたlambdaには、その名前のファイルまたはフォルダーが先頭にあります。フォルダーの中に置くか、別の名前にしてください。',
  lambda: 'lambdaに.lambda/フォルダーはもうありません。ドキュメントはdocs/に、テストはtests/に置きます。',
  assets: 'lambdaが配信するものは、今はリソースにあります。そちらに追加してください。',
  resourceName: '英数字、ハイフン、アンダースコア、ドットが使え、フォルダーはスラッシュで区切って6階層まで。拡張子も必要です（正しい種類のファイルとして配信されるため）。',
  exists: '同じ名前のファイルがすでにあります。',
  remove: (name) => `${name}を削除しますか？　中身も削除されます。`,
  removeFolder: (name, files) => `${name}とその中の${files === 1 ? '1個のファイル' : `${files}個のファイル`}を削除しますか？`,
  there: (name) => `${name}はすでにあります。`,
  entry: 'スニペット：これが返すものが配信されます',
  errors: 'エラーあり',
  removeFile: (name) => `${name}を削除`,
  removeTitle: '削除',
  codePlaceholder: 'Store.cs、models/Item.cs、docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'ファイルをアップロード',
};
