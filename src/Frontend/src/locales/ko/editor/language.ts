/** 받침이 있으면 앞의 조사, 없으면 뒤의 조사를 붙여요: 워크스페이스를, 시크릿을. */
export const josa = (word: string, withFinal: string, without: string) => {
  const code = word.charCodeAt(word.length - 1) - 0xac00;

  return `${word}${code >= 0 && code < 11172 && code % 28 !== 0 ? withFinal : without}`;
};
